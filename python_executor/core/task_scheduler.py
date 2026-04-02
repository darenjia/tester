"""
任务调度器
负责任务的调度和分发
"""
import threading
import time
import logging
import copy
from typing import Dict, Any, Optional, List
from datetime import datetime, timedelta
from enum import Enum

from models.executor_task import Task, TaskStatus, TaskPriority, task_queue
from models.task_log import task_log_manager
from core.task_executor_production import get_task_executor
task_executor = get_task_executor()

logger = logging.getLogger(__name__)


class TaskScheduler:
    """
    任务调度器
    
    负责任务的调度、分发和监控
    """
    
    def __init__(self, check_interval: float = 1.0):
        """
        初始化任务调度器
        
        Args:
            check_interval: 检查间隔(秒)
        """
        self.check_interval = check_interval
        self._running = False
        self._scheduler_thread: Optional[threading.Thread] = None
        self._lock = threading.Lock()
        self._scheduled_tasks: Dict[str, datetime] = {}  # task_id -> scheduled_time
        self._periodic_tasks: Dict[str, Dict[str, Any]] = {}
        
    def start(self):
        """启动调度器"""
        with self._lock:
            if self._running:
                return
            self._running = True
            
        self._scheduler_thread = threading.Thread(target=self._scheduler_loop, daemon=True)
        self._scheduler_thread.start()
        
        logger.info("任务调度器已启动")
        task_log_manager.info("system", "任务调度器已启动")
        
    def stop(self):
        """停止调度器"""
        with self._lock:
            self._running = False
            periodic_entries = list(self._periodic_tasks.values())
            
        if self._scheduler_thread:
            self._scheduler_thread.join(timeout=5)
        for entry in periodic_entries:
            cancel_event = entry.get("cancel_event")
            thread = entry.get("thread")
            if cancel_event:
                cancel_event.set()
            if thread:
                thread.join(timeout=5)
        with self._lock:
            self._periodic_tasks.clear()
            
        logger.info("任务调度器已停止")
        task_log_manager.info("system", "任务调度器已停止")
        
    def _scheduler_loop(self):
        """调度循环"""
        while self._running:
            try:
                self._process_queue()
                self._process_scheduled_tasks()
                time.sleep(self.check_interval)
            except Exception as e:
                logger.error(f"调度循环异常: {e}")
                task_log_manager.error("system", f"调度循环异常: {e}")
                
    def _process_queue(self):
        """处理任务队列"""
        pending_tasks = task_queue.get_pending_tasks()
        running_count = task_executor.get_running_count()
        executor_stats = task_executor.get_stats()
        queued_count = executor_stats.get("queue_size", 0) if isinstance(executor_stats, dict) else 0

        if not pending_tasks or running_count >= task_executor.max_workers or queued_count > 0:
            return

        ready_task = None
        for task in pending_tasks:
            task_id = getattr(task, "id", None) or getattr(task, "task_id", None)
            if task_id in self._scheduled_tasks:
                continue
            ready_task = task
            break

        if ready_task:
            task_executor.submit_task(ready_task)
                 
    def _process_scheduled_tasks(self):
        """处理定时任务"""
        current_time = datetime.now()
        
        with self._lock:
            # 找出到期的定时任务
            ready_tasks = []
            for task_id, scheduled_time in list(self._scheduled_tasks.items()):
                if current_time >= scheduled_time:
                    ready_tasks.append(task_id)
                    
            # 移除已准备的任务
            for task_id in ready_tasks:
                del self._scheduled_tasks[task_id]
                
        # 将到期的任务提交到队列
        for task_id in ready_tasks:
            task = task_queue.get_task(task_id)
            if task and task.status == TaskStatus.PENDING.value:
                task_log_manager.info(task_id, "定时任务已到期，开始执行")
                task_executor.submit_task(task)
                
    def schedule_task(self, task: Task, delay: float = 0) -> bool:
        """
        调度任务
        
        Args:
            task: 任务对象
            delay: 延迟执行时间(秒)
            
        Returns:
            是否调度成功
        """
        if delay > 0:
            # 定时任务
            scheduled_time = datetime.now() + timedelta(seconds=delay)
            
            with self._lock:
                self._scheduled_tasks[task.id] = scheduled_time
                
            # 先提交到队列
            if task_queue.add(task):
                task_log_manager.info(
                    task.id,
                    f"任务已调度",
                    details={"delay": delay, "scheduled_time": scheduled_time.isoformat()}
                )
                return True
            with self._lock:
                self._scheduled_tasks.pop(task.id, None)
        else:
            # 立即执行
            return task_executor.submit_task(task)
            
        return False
        
    def schedule_periodic_task(self, task: Task, interval: float,
                               max_iterations: Optional[int] = None) -> Optional[str]:
        """
        调度周期性任务
        
        Args:
            task: 任务对象
            interval: 执行间隔(秒)
            max_iterations: 最大执行次数，None表示无限
            
        Returns:
            调度ID
        """
        if not self._running:
            logger.warning("调度器未启动，无法创建周期性任务")
            task_log_manager.error(task.id, "调度器未启动，周期性任务创建失败")
            return None

        try:
            task_executor._build_execution_plan_from_queue_task(task)
        except Exception as exc:
            logger.warning(f"周期性任务预校验失败: {exc}")
            task_log_manager.error(task.id, f"周期性任务预校验失败: {exc}")
            return None

        scheduler_id = f"periodic_{task.id}"
        cancel_event = threading.Event()
        registry_entry: Dict[str, Any] = {
            "scheduler_id": scheduler_id,
            "task_id": task.id,
            "task_name": task.name,
            "interval": interval,
            "max_iterations": max_iterations,
            "iteration": 0,
            "thread": None,
            "cancel_event": cancel_event,
        }
        
        def periodic_runner():
            iteration = 0
            try:
                while (
                    self._running
                    and not cancel_event.is_set()
                    and (max_iterations is None or iteration < max_iterations)
                ):
                    with self._lock:
                        if scheduler_id in self._periodic_tasks:
                            self._periodic_tasks[scheduler_id]["iteration"] = iteration
                    # 创建新任务实例
                    new_task = Task(
                        name=f"{task.name} #{iteration + 1}",
                        task_type=task.task_type,
                        priority=task.priority,
                        params=copy.deepcopy(task.params),
                        timeout=task.timeout,
                        max_retries=task.max_retries,
                        created_by=task.created_by,
                        metadata={**copy.deepcopy(task.metadata), "periodic": True, "iteration": iteration}
                    )
                    
                    # 提交任务
                    if not task_executor.submit_task(new_task):
                        task_log_manager.error(
                            task.id,
                            "周期性任务提交失败",
                            details={"iteration": iteration, "scheduler_id": scheduler_id},
                        )
                    
                    iteration += 1
                    if cancel_event.wait(interval):
                        break
            finally:
                with self._lock:
                    self._periodic_tasks.pop(scheduler_id, None)
                
        # 启动周期性任务线程
        thread = threading.Thread(target=periodic_runner, daemon=True)
        with self._lock:
            registry_entry["thread"] = thread
            self._periodic_tasks[scheduler_id] = registry_entry
        thread.start()
        
        task_log_manager.info(
            task.id,
            f"周期性任务已调度",
            details={"interval": interval, "max_iterations": max_iterations}
        )
        
        return scheduler_id
        
    def cancel_scheduled_task(self, task_id: str) -> bool:
        """
        取消定时任务
        
        Args:
            task_id: 任务ID
            
        Returns:
            是否取消成功
        """
        was_scheduled = False
        periodic_entry: Optional[Dict[str, Any]] = None
        with self._lock:
            if task_id in self._scheduled_tasks:
                del self._scheduled_tasks[task_id]
                was_scheduled = True
            elif task_id in self._periodic_tasks:
                periodic_entry = self._periodic_tasks.pop(task_id)
            else:
                for scheduler_id, entry in list(self._periodic_tasks.items()):
                    if entry.get("task_id") == task_id:
                        periodic_entry = self._periodic_tasks.pop(scheduler_id)
                        break
                 
        # 未到期的定时任务仍在全局队列里，直接标记为取消并保留审计记录
        if was_scheduled and task_queue.update_task_status(
            task_id,
            TaskStatus.CANCELLED.value,
            error_message="定时任务已取消",
        ):
            task_log_manager.info(task_id, "定时任务已取消")
            return True

        if periodic_entry:
            cancel_event = periodic_entry.get("cancel_event")
            if cancel_event:
                cancel_event.set()
            task_log_manager.info(
                periodic_entry.get("task_id", task_id),
                "周期性任务已取消",
                details={"scheduler_id": periodic_entry.get("scheduler_id", task_id)},
            )
            return True

        # 否则尝试取消已进入执行器的任务
        return task_executor.cancel_task(task_id)
        
    def get_scheduled_tasks(self) -> List[Dict[str, Any]]:
        """
        获取所有定时任务
        
        Returns:
            定时任务列表
        """
        with self._lock:
            result = []
            for task_id, scheduled_time in self._scheduled_tasks.items():
                task = task_queue.get_task(task_id)
                if task:
                    result.append({
                        "task_id": task_id,
                        "task_name": task.name,
                        "scheduled_time": scheduled_time.isoformat(),
                        "status": task.status
                    })
            for scheduler_id, entry in self._periodic_tasks.items():
                task_id = entry.get("task_id", scheduler_id)
                task = task_queue.get_task(task_id)
                thread = entry.get("thread")
                result.append({
                    "task_id": task_id,
                    "task_name": entry.get("task_name") or (task.name if task else task_id),
                    "scheduled_time": None,
                    "status": "running" if thread and thread.is_alive() else "pending",
                    "scheduler_id": scheduler_id,
                    "schedule_type": "periodic",
                    "interval": entry.get("interval"),
                    "max_iterations": entry.get("max_iterations"),
                    "iteration": entry.get("iteration", 0),
                })
            return result
            
    def get_stats(self) -> Dict[str, Any]:
        """获取调度器统计信息"""
        with self._lock:
            return {
                "running": self._running,
                "scheduled_count": len(self._scheduled_tasks),
                "periodic_count": len(self._periodic_tasks),
                "check_interval": self.check_interval
            }


# 全局调度器实例
task_scheduler = TaskScheduler()
