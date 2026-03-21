"""
状态机驱动的任务执行器

使用状态机模式管理测试执行流程，提供更清晰的流程控制和更好的错误恢复能力
"""
import threading
import time
from typing import Dict, Any, Optional, Callable, List
from concurrent.futures import ThreadPoolExecutor

from utils.logger import get_logger
from utils.exceptions import TaskException, ToolException
from utils.validators import InputValidator, ValidationError
from utils.metrics import TaskMetrics, record_metric
from models.task import Task, TaskStatus
from core.state_machine import TestStateMachine, TestState, StateTransition, STANDARD_TRANSITIONS
from core.test_state_handlers import (
    SelfCheckHandler, ConfigLoadHandler, ConnectingHandler,
    RunningHandler, ResultCollectHandler, CleanupHandler, PausedHandler
)
from core.result_collector import ResultCollector
from core.adapters import create_adapter_with_wrapper, TestToolType

logger = get_logger("state_machine_executor")


class StateMachineTaskExecutor:
    """状态机驱动的任务执行器"""
    
    def __init__(self, message_sender: Callable):
        """
        初始化执行器
        
        Args:
            message_sender: 消息发送函数
        """
        self.message_sender = message_sender
        self._running = False
        self._task_queue = []
        self._queue_lock = threading.Lock()
        self._worker_pool = ThreadPoolExecutor(max_workers=1)
        self._current_task: Optional[Task] = None
        self._current_state_machine: Optional[TestStateMachine] = None
        self._stop_event = threading.Event()
        
        # 状态变化回调
        self._state_change_callbacks: List[Callable] = []
        
        logger.info("状态机任务执行器初始化完成")
    
    def start(self):
        """启动执行器"""
        if self._running:
            return
        
        self._running = True
        self._worker_thread = threading.Thread(target=self._worker_loop)
        self._worker_thread.daemon = True
        self._worker_thread.start()
        
        logger.info("状态机任务执行器已启动")
    
    def stop(self):
        """停止执行器"""
        self._running = False
        self._stop_event.set()
        
        # 停止当前状态机
        if self._current_state_machine:
            self._current_state_machine.stop()
        
        if hasattr(self, '_worker_thread') and self._worker_thread:
            self._worker_thread.join(timeout=10)
        
        self._worker_pool.shutdown(wait=True)
        
        logger.info("状态机任务执行器已停止")
    
    def _worker_loop(self):
        """工作线程循环"""
        while self._running:
            try:
                task = self._get_next_task()
                
                if task:
                    future = self._worker_pool.submit(self._execute_with_state_machine, task)
                    try:
                        future.result(timeout=task.timeout)
                    except Exception as e:
                        logger.error(f"任务执行异常: {e}")
                else:
                    time.sleep(0.5)
                    
            except Exception as e:
                if self._running:
                    logger.error(f"工作循环异常: {e}")
    
    def _get_next_task(self) -> Optional[Task]:
        """获取下一个任务"""
        with self._queue_lock:
            if self._task_queue:
                return self._task_queue.pop(0)
        return None
    
    def execute_task(self, task: Task) -> bool:
        """
        提交任务到队列
        
        Args:
            task: 任务对象
            
        Returns:
            bool: 提交成功返回True
        """
        # 验证任务数据
        try:
            test_items = []
            for item in task.test_items:
                item_dict = {
                    'name': item.name,
                    'type': item.type,
                    'case_no': getattr(item, 'caseNo', None) or getattr(item, 'case_no', None)
                }
                test_items.append(item_dict)

            task_data = {
                'taskNo': task.task_id,
                'deviceId': task.device_id,
                'toolType': task.tool_type,
                'configPath': task.config_path,
                'testItems': test_items,
                'timeout': task.timeout
            }
            InputValidator.validate_task_data(task_data)
        except ValidationError as e:
            logger.error(f"任务数据验证失败: {e}")
            return False
        
        # 添加到队列
        with self._queue_lock:
            self._task_queue.append(task)
            logger.info(f"任务已加入队列: {task.task_id}")
            return True
    
    def _execute_with_state_machine(self, task: Task):
        """使用状态机执行任务"""
        self._current_task = task
        self._stop_event.clear()
        
        # 创建结果收集器
        collector = ResultCollector(task.task_id)
        
        # 初始化性能指标
        metrics = TaskMetrics(task.task_id)
        
        try:
            logger.info(f"开始执行任务: {task.task_id}")
            metrics.record_step('start', 0)
            
            # 更新状态为运行中
            self._update_task_status(task.task_id, TaskStatus.RUNNING)
            
            # 创建状态机
            state_machine = TestStateMachine(task.task_id)
            self._current_state_machine = state_machine
            
            # 注册标准状态转换
            for transition in STANDARD_TRANSITIONS:
                state_machine.register_transition(transition)
            
            # 注册状态处理器
            self._register_state_handlers(state_machine, task, collector, metrics)
            
            # 添加状态变化回调
            state_machine.add_state_change_callback(
                lambda from_s, to_s: self._on_state_change(task.task_id, from_s, to_s)
            )
            
            # 启动状态机
            state_machine.start()
            
            # 运行状态机直到完成或取消
            while state_machine.is_running():
                if self._stop_event.is_set():
                    logger.info("任务被取消，停止状态机")
                    state_machine.stop()
                    break
                
                if not state_machine.step():
                    break
            
            # 获取执行结果
            context = state_machine.get_context()
            final_state = state_machine.get_state()
            
            if final_state == TestState.COMPLETED:
                logger.info(f"任务执行完成: {task.task_id}")
                final_result = context.data.get('final_result', {})
                self._complete_task(task, final_result, collector, metrics)
            elif final_state == TestState.FAILED:
                error_msg = context.error or "任务执行失败"
                logger.error(f"任务执行失败: {task.task_id}, error: {error_msg}")
                self._fail_task(task, error_msg, collector, metrics)
            else:
                logger.warning(f"任务异常终止: {task.task_id}, state: {final_state.name}")
                self._fail_task(task, f"任务异常终止，状态: {final_state.name}", collector, metrics)
            
        except Exception as e:
            logger.error(f"任务执行异常: {e}", exc_info=True)
            self._fail_task(task, f"任务执行异常: {e}", collector, metrics)
        
        finally:
            self._current_state_machine = None
            self._current_task = None
            
            # 完成指标收集
            metrics.finalize()
            metrics_summary = metrics.get_summary()
            
            # 记录性能指标
            for step_name, duration in metrics_summary['step_times'].items():
                record_metric(f'task.step_duration', duration, {
                    'task_id': task.task_id,
                    'step': step_name
                })
            
            record_metric('task.total_duration', metrics_summary['duration'], {
                'task_id': task.task_id
            })
    
    def _register_state_handlers(self, state_machine: TestStateMachine, 
                                 task: Task, collector: ResultCollector,
                                 metrics: TaskMetrics):
        """注册状态处理器"""
        # 获取测试项列表
        test_items = [item.name for item in task.test_items]
        
        # 确定适配器类型
        tool_type_lower = task.tool_type.lower() if task.tool_type else ""
        if tool_type_lower == TestToolType.CANOE.value:
            adapter_type = TestToolType.CANOE
        elif tool_type_lower == TestToolType.TSMASTER.value:
            adapter_type = TestToolType.TSMASTER
        elif tool_type_lower == TestToolType.TTWORKBENCH.value:
            adapter_type = TestToolType.TTWORKBENCH
        else:
            adapter_type = TestToolType.CANOE
        
        # 注册各状态处理器
        state_machine.register_handler(
            TestState.SELF_CHECK, 
            SelfCheckHandler(task)
        )
        state_machine.register_handler(
            TestState.CONFIG_LOAD, 
            ConfigLoadHandler(task)
        )
        state_machine.register_handler(
            TestState.CONNECTING, 
            ConnectingHandler(task, adapter_type.value)
        )
        state_machine.register_handler(
            TestState.RUNNING, 
            RunningHandler(task, test_items)
        )
        state_machine.register_handler(
            TestState.RESULT_COLLECT, 
            ResultCollectHandler(task)
        )
        state_machine.register_handler(
            TestState.CLEANUP, 
            CleanupHandler(task)
        )
        state_machine.register_handler(
            TestState.PAUSED, 
            PausedHandler()
        )
    
    def _on_state_change(self, task_id: str, from_state: TestState, to_state: TestState):
        """状态变化回调"""
        logger.info(f"Task {task_id}: {from_state.name} -> {to_state.name}")
        
        # 发送状态变化通知
        if self.message_sender:
            try:
                self.message_sender({
                    'type': 'state_change',
                    'task_id': task_id,
                    'from_state': from_state.name,
                    'to_state': to_state.name,
                    'timestamp': time.time()
                })
            except Exception as e:
                logger.error(f"发送状态变化通知失败: {e}")
    
    def _update_task_status(self, task_id: str, status: TaskStatus):
        """更新任务状态"""
        logger.info(f"任务状态更新: {task_id} -> {status.value}")
        
        if self.message_sender:
            try:
                self.message_sender({
                    'type': 'task_status',
                    'task_id': task_id,
                    'status': status.value,
                    'timestamp': time.time()
                })
            except Exception as e:
                logger.error(f"发送状态更新通知失败: {e}")
    
    def _complete_task(self, task: Task, result: Dict[str, Any], 
                      collector: ResultCollector, metrics: TaskMetrics):
        """完成任务"""
        logger.info(f"任务完成: {task.task_id}")
        
        # 更新状态
        self._update_task_status(task.task_id, TaskStatus.COMPLETED)
        
        # 发送完成通知
        if self.message_sender:
            try:
                self.message_sender({
                    'type': 'task_completed',
                    'task_id': task.task_id,
                    'result': result,
                    'timestamp': time.time()
                })
            except Exception as e:
                logger.error(f"发送完成通知失败: {e}")
    
    def _fail_task(self, task: Task, error_msg: str,
                  collector: ResultCollector, metrics: TaskMetrics):
        """标记任务失败"""
        logger.error(f"任务失败: {task.task_id}, error: {error_msg}")
        
        # 记录错误
        metrics.record_error()
        collector.add_log("ERROR", error_msg)
        
        # 更新状态
        self._update_task_status(task.task_id, TaskStatus.FAILED)
        
        # 发送失败通知
        if self.message_sender:
            try:
                self.message_sender({
                    'type': 'task_failed',
                    'task_id': task.task_id,
                    'error': error_msg,
                    'timestamp': time.time()
                })
            except Exception as e:
                logger.error(f"发送失败通知失败: {e}")
    
    def pause_current_task(self) -> bool:
        """暂停当前任务"""
        if self._current_state_machine:
            return self._current_state_machine.pause()
        return False
    
    def resume_current_task(self) -> bool:
        """恢复当前任务"""
        if self._current_state_machine:
            return self._current_state_machine.resume()
        return False
    
    def get_current_state(self) -> Optional[str]:
        """获取当前任务状态"""
        if self._current_state_machine:
            return self._current_state_machine.get_state().name
        return None
    
    def get_queue_size(self) -> int:
        """获取队列大小"""
        with self._queue_lock:
            return len(self._task_queue)
    
    def is_running(self) -> bool:
        """检查是否正在运行"""
        return self._running
