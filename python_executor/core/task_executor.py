"""
任务执行器
负责任务的异步执行和状态管理
"""
import threading
import time
import traceback
from typing import Dict, Any, Optional, Callable, List
from datetime import datetime
from concurrent.futures import ThreadPoolExecutor, as_completed

import sys
import os
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from models.executor_task import Task, TaskStatus, TaskPriority, task_queue
from models.task_log import task_log_manager, LogLevel


class TaskExecutor:
    """
    任务执行器
    
    负责任务的异步执行、状态流转和并发控制
    """
    
    def __init__(self, max_workers: int = 5, default_timeout: int = 3600):
        """
        初始化任务执行器
        
        Args:
            max_workers: 最大并发工作线程数
            default_timeout: 默认任务超时时间(秒)
        """
        self.max_workers = max_workers
        self.default_timeout = default_timeout
        self._executor = ThreadPoolExecutor(max_workers=max_workers)
        self._running_tasks: Dict[str, threading.Thread] = {}
        self._task_handlers: Dict[str, Callable] = {}
        self._lock = threading.Lock()
        self._running = False
        self._monitor_thread: Optional[threading.Thread] = None
        
    def register_handler(self, task_type: str, handler: Callable[[Task], Dict[str, Any]]):
        """
        注册任务类型处理器
        
        Args:
            task_type: 任务类型
            handler: 处理函数，接收Task对象，返回结果字典
        """
        self._task_handlers[task_type] = handler
        
    def start(self):
        """启动执行器"""
        with self._lock:
            if self._running:
                return
            self._running = True
            
        # 启动监控线程
        self._monitor_thread = threading.Thread(target=self._monitor_loop, daemon=True)
        self._monitor_thread.start()
        
        task_log_manager.info("system", "任务执行器已启动", details={"max_workers": self.max_workers})
        
    def stop(self):
        """停止执行器"""
        with self._lock:
            self._running = False
            
        # 关闭线程池
        self._executor.shutdown(wait=True)
        
        task_log_manager.info("system", "任务执行器已停止")
        
    def _monitor_loop(self):
        """监控循环，检查任务超时"""
        while self._running:
            try:
                self._check_timeouts()
                time.sleep(5)  # 每5秒检查一次
            except Exception as e:
                task_log_manager.error("system", f"监控循环异常: {e}")
                
    def _check_timeouts(self):
        """检查任务超时"""
        current_time = datetime.now()
        
        with self._lock:
            running_tasks = list(self._running_tasks.keys())
            
        for task_id in running_tasks:
            task = task_queue.get_task(task_id)
            if task and task.is_running():
                # 检查是否超时
                if task.started_at:
                    start_time = datetime.fromisoformat(task.started_at)
                    elapsed = (current_time - start_time).total_seconds()
                    
                    if elapsed > task.timeout:
                        # 任务超时
                        self._handle_timeout(task)
                        
    def _handle_timeout(self, task: Task):
        """处理任务超时"""
        task.status = TaskStatus.TIMEOUT.value
        task.completed_at = datetime.now().isoformat()
        task.error_message = f"任务执行超时({task.timeout}秒)"
        
        task_log_manager.error(
            task.id,
            f"任务执行超时",
            details={"timeout": task.timeout, "elapsed": task.get_duration()}
        )
        
        with self._lock:
            if task.id in self._running_tasks:
                del self._running_tasks[task.id]
                
    def submit_task(self, task: Task) -> bool:
        """
        提交任务到队列
        
        Args:
            task: 任务对象
            
        Returns:
            是否提交成功
        """
        if not task_queue.add(task):
            return False
            
        task_log_manager.info(
            task.id,
            f"任务已提交到队列",
            details={"name": task.name, "type": task.task_type, "priority": task.priority}
        )
        
        return True
        
    def execute_task(self, task: Task) -> bool:
        """
        立即执行任务
        
        Args:
            task: 任务对象
            
        Returns:
            是否成功启动
        """
        if task.task_type not in self._task_handlers:
            task.status = TaskStatus.FAILED.value
            task.completed_at = datetime.now().isoformat()
            task.error_message = f"未知的任务类型: {task.task_type}"
            
            task_log_manager.error(task.id, task.error_message)
            return False
            
        # 更新任务状态
        task.status = TaskStatus.RUNNING.value
        task.started_at = datetime.now().isoformat()
        
        task_log_manager.info(
            task.id,
            f"开始执行任务",
            details={"name": task.name, "type": task.task_type}
        )
        
        # 提交到线程池执行
        future = self._executor.submit(self._execute_task_wrapper, task)
        
        with self._lock:
            self._running_tasks[task.id] = future
            
        return True
        
    def _execute_task_wrapper(self, task: Task):
        """任务执行包装器"""
        try:
            handler = self._task_handlers.get(task.task_type)
            if not handler:
                raise ValueError(f"未知的任务类型: {task.task_type}")
                
            # 执行任务
            result = handler(task)
            
            # 更新任务状态为完成
            task.status = TaskStatus.COMPLETED.value
            task.completed_at = datetime.now().isoformat()
            task.result = result
            
            task_log_manager.info(
                task.id,
                f"任务执行完成",
                details={"duration": task.get_duration(), "result": result}
            )
            
        except Exception as e:
            # 任务执行失败
            task.status = TaskStatus.FAILED.value
            task.completed_at = datetime.now().isoformat()
            task.error_message = str(e)
            
            task_log_manager.error(
                task.id,
                f"任务执行失败: {e}",
                details={"traceback": traceback.format_exc()}
            )
            
        finally:
            with self._lock:
                if task.id in self._running_tasks:
                    del self._running_tasks[task.id]
                    
    def cancel_task(self, task_id: str) -> bool:
        """
        取消任务
        
        Args:
            task_id: 任务ID
            
        Returns:
            是否取消成功
        """
        task = task_queue.get_task(task_id)
        if not task:
            return False
            
        if task.is_completed():
            return False
            
        if task.status == TaskStatus.PENDING.value:
            # 从队列中移除
            task_queue.remove(task_id)
            task.status = TaskStatus.CANCELLED.value
            task.completed_at = datetime.now().isoformat()
            
            task_log_manager.info(task.id, "任务已取消")
            return True
            
        if task.status == TaskStatus.RUNNING.value:
            # 标记为取消，实际线程会继续执行但结果会被忽略
            task.status = TaskStatus.CANCELLED.value
            task.completed_at = datetime.now().isoformat()
            
            task_log_manager.info(task.id, "运行中的任务已标记为取消")
            return True
            
        return False
        
    def retry_task(self, task_id: str) -> Optional[Task]:
        """
        重试任务
        
        Args:
            task_id: 任务ID
            
        Returns:
            新的任务对象，如果无法重试则返回None
        """
        task = task_queue.get_task(task_id)
        if not task or not task.can_retry():
            return None
            
        # 创建新任务
        new_task = Task(
            name=task.name,
            task_type=task.task_type,
            priority=task.priority,
            params=task.params.copy(),
            timeout=task.timeout,
            max_retries=task.max_retries,
            created_by=task.created_by,
            metadata=task.metadata.copy()
        )
        new_task.retry_count = task.retry_count + 1
        
        # 提交新任务
        if self.submit_task(new_task):
            task_log_manager.info(
                new_task.id,
                f"任务重试 (原任务: {task.id})",
                details={"retry_count": new_task.retry_count, "original_task": task_id}
            )
            return new_task
            
        return None
        
    def get_running_count(self) -> int:
        """获取正在运行的任务数量"""
        with self._lock:
            return len(self._running_tasks)
            
    def get_stats(self) -> Dict[str, Any]:
        """获取执行器统计信息"""
        queue_stats = task_queue.get_stats()
        
        return {
            "max_workers": self.max_workers,
            "running_count": self.get_running_count(),
            "queue_size": task_queue.size(),
            "total_tasks": queue_stats["total"],
            "pending": queue_stats["pending"],
            "running": queue_stats["running"],
            "completed": queue_stats["completed"],
            "failed": queue_stats["failed"],
            "cancelled": queue_stats["cancelled"],
            "timeout": queue_stats["timeout"]
        }


# 全局执行器实例
task_executor = TaskExecutor()


# 默认任务处理器示例
def default_task_handler(task: Task) -> Dict[str, Any]:
    """
    默认任务处理器
    
    示例处理器，实际使用时需要注册具体的处理器
    """
    task_log_manager.info(task.id, f"执行默认任务处理器", details={"params": task.params})
    
    # 模拟任务执行
    time.sleep(2)
    
    return {
        "success": True,
        "message": "任务执行成功",
        "processed_at": datetime.now().isoformat()
    }


# 注册默认处理器
task_executor.register_handler("default", default_task_handler)
