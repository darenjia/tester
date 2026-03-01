"""
任务执行核心引擎 - 生产环境增强版
集成熔断器、重试、性能监控、配置热更新等功能
"""
import threading
import time
import queue
from datetime import datetime
from typing import Dict, Any, Optional, Callable
from concurrent.futures import ThreadPoolExecutor, as_completed

from utils.logger import get_logger
from utils.exceptions import TaskException, ToolException
from utils.retry import retry_with_config, RetryConfig, CircuitBreaker
from utils.validators import InputValidator, ValidationError
from utils.metrics import TaskMetrics, performance_monitor, record_metric
from config.settings import settings
from config.config_manager import config_manager
from models.task import Task, TaskStatus, TestToolType, TestResult, TestItemType, TaskResult
from core.result_collector import ResultCollector
from core.canoe_controller_production import CANoeControllerProduction
from core.tsmaster_controller import TSMasterController

logger = get_logger("task_executor_production")

# 任务执行熔断器
TASK_CIRCUIT_BREAKER = CircuitBreaker(
    failure_threshold=10,
    recovery_timeout=300.0,
    expected_exception=Exception
)

class TaskQueue:
    """任务队列管理器"""
    
    def __init__(self, max_size: int = 100):
        self.queue = queue.Queue(maxsize=max_size)
        self.processing = {}
        self.completed = {}
        self._lock = threading.Lock()
    
    def put(self, task: Task) -> bool:
        """添加任务到队列"""
        try:
            self.queue.put(task, block=False)
            return True
        except queue.Full:
            logger.warning("任务队列已满")
            return False
    
    def get(self, timeout: float = 1.0) -> Optional[Task]:
        """从队列获取任务"""
        try:
            return self.queue.get(timeout=timeout)
        except queue.Empty:
            return None
    
    def mark_processing(self, task_id: str, task: Task):
        """标记任务为处理中"""
        with self._lock:
            self.processing[task_id] = {
                'task': task,
                'start_time': time.time()
            }
    
    def mark_completed(self, task_id: str, result: Dict[str, Any]):
        """标记任务为已完成"""
        with self._lock:
            if task_id in self.processing:
                del self.processing[task_id]
            
            self.completed[task_id] = {
                'result': result,
                'completion_time': time.time()
            }
    
    def get_queue_size(self) -> int:
        """获取队列大小"""
        return self.queue.qsize()
    
    def get_processing_count(self) -> int:
        """获取处理中任务数"""
        with self._lock:
            return len(self.processing)

class TaskExecutorProduction:
    """任务执行器 - 生产环境版本"""
    
    def __init__(self, message_sender: Callable):
        """
        初始化任务执行器
        
        Args:
            message_sender: 消息发送函数
        """
        self.message_sender = message_sender
        self.current_task: Optional[Task] = None
        self.current_collector: Optional[ResultCollector] = None
        self.controller = None
        self._stop_event = threading.Event()
        self._task_thread = None
        self._start_time = None
        self._lock = threading.RLock()
        self._task_queue = TaskQueue()
        self._worker_pool = ThreadPoolExecutor(max_workers=1)  # 串行执行
        self._running = False
        self._worker_thread = None
        
        # 性能监控
        self._current_metrics = None
        
        logger.info("任务执行器（生产环境版）初始化完成")
    
    def start(self):
        """启动执行器"""
        if self._running:
            return
        
        self._running = True
        self._worker_thread = threading.Thread(target=self._worker_loop)
        self._worker_thread.daemon = True
        self._worker_thread.start()
        
        logger.info("任务执行器已启动")
    
    def stop(self):
        """停止执行器"""
        self._running = False
        self._stop_event.set()
        
        if self._worker_thread:
            self._worker_thread.join(timeout=10)
        
        self._worker_pool.shutdown(wait=True)
        
        logger.info("任务执行器已停止")
    
    def _worker_loop(self):
        """工作线程循环"""
        while self._running:
            try:
                # 从队列获取任务
                task = self._task_queue.get(timeout=1.0)
                
                if task:
                    # 提交到线程池执行
                    future = self._worker_pool.submit(self._execute_task_production, task)
                    
                    # 等待任务完成
                    try:
                        future.result(timeout=task.timeout)
                    except Exception as e:
                        logger.error(f"任务执行异常: {e}")
                
            except Exception as e:
                if self._running:
                    logger.error(f"工作循环异常: {e}")
    
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
            task_data = {
                'taskNo': task.task_id,
                'deviceId': task.device_id,
                'toolType': task.tool_type,
                'configPath': task.config_path,
                'testItems': [{'name': item.name, 'type': item.type} for item in task.test_items],
                'timeout': task.timeout
            }
            InputValidator.validate_task_data(task_data)
        except ValidationError as e:
            logger.error(f"任务数据验证失败: {e}")
            return False
        
        # 添加到队列
        if self._task_queue.put(task):
            logger.info(f"任务已加入队列: {task.task_id}")
            return True
        else:
            logger.error(f"任务加入队列失败: {task.task_id}")
            return False
    
    @TASK_CIRCUIT_BREAKER
    def _execute_task_production(self, task: Task):
        """生产环境任务执行"""
        with self._lock:
            if self.current_task is not None:
                logger.warning("已有任务正在执行")
                return
            
            self.current_task = task
            self._stop_event.clear()
        
        # 初始化性能指标
        self._current_metrics = TaskMetrics(task.task_id)
        self._task_queue.mark_processing(task.task_id, task)
        
        try:
            logger.info(f"开始执行任务: {task.task_id}")
            self._current_metrics.record_step('start', 0)
            
            # 初始化结果收集器
            self.current_collector = ResultCollector(task.task_id)
            
            # 更新状态为运行中
            self._update_task_status(task.task_id, TaskStatus.RUNNING)
            
            # 根据工具类型选择控制器
            step_start = time.time()
            if task.tool_type == TestToolType.CANOE.value:
                self.controller = CANoeControllerProduction()
            elif task.tool_type == TestToolType.TSMASTER.value:
                self.controller = TSMasterController()
            else:
                raise TaskException(f"不支持的测试工具类型: {task.tool_type}")
            
            self._current_metrics.record_step('controller_init', time.time() - step_start)
            
            # 连接测试软件（带重试）
            step_start = time.time()
            self._connect_tool_with_retry(task)
            self._current_metrics.record_step('connect_tool', time.time() - step_start)
            
            # 加载配置
            step_start = time.time()
            self._load_configuration(task)
            self._current_metrics.record_step('load_config', time.time() - step_start)
            
            # 启动测量/仿真
            step_start = time.time()
            self._start_measurement(task)
            self._current_metrics.record_step('start_measurement', time.time() - step_start)
            
            # 执行测试项
            step_start = time.time()
            results = self._execute_test_items(task)
            self._current_metrics.record_step('execute_items', time.time() - step_start)
            
            # 停止测量/仿真
            step_start = time.time()
            self._stop_measurement(task)
            self._current_metrics.record_step('stop_measurement', time.time() - step_start)
            
            # 完成任务
            self._complete_task(task, results)
            
            # 记录成功，重置熔断器
            TASK_CIRCUIT_BREAKER.record_success()
            
        except TaskException as e:
            logger.error(f"任务执行失败: {e}")
            self._current_metrics.record_error()
            TASK_CIRCUIT_BREAKER.record_failure()
            self._fail_task(task, str(e))
            
        except ToolException as e:
            logger.error(f"工具操作失败: {e}")
            self._current_metrics.record_error()
            TASK_CIRCUIT_BREAKER.record_failure()
            self._fail_task(task, f"工具操作失败: {e}")
            
        except Exception as e:
            logger.error(f"任务执行异常: {e}", exc_info=True)
            self._current_metrics.record_error()
            TASK_CIRCUIT_BREAKER.record_failure()
            self._fail_task(task, f"任务执行异常: {e}")
            
        finally:
            # 清理资源
            self._cleanup()
            
            # 完成任务指标收集
            self._current_metrics.finalize()
            metrics_summary = self._current_metrics.get_summary()
            
            # 记录性能指标
            for step_name, duration in metrics_summary['step_times'].items():
                record_metric(f'task.step_duration', duration, {
                    'task_id': task.task_id,
                    'step': step_name
                })
            
            record_metric('task.total_duration', metrics_summary['duration'], {
                'task_id': task.task_id
            })
            
            record_metric('task.error_count', metrics_summary['error_count'], {
                'task_id': task.task_id
            })
            
            # 标记任务完成
            self._task_queue.mark_completed(task.task_id, {
                'status': 'completed' if metrics_summary['error_count'] == 0 else 'failed',
                'metrics': metrics_summary
            })
            
            with self._lock:
                self.current_task = None
                self.current_collector = None
                self._current_metrics = None
    
    def _connect_tool_with_retry(self, task: Task):
        """带重试的连接工具"""
        max_retries = config_manager.get('canoe.max_retries', 3)
        retry_delay = config_manager.get('canoe.retry_delay', 2.0)
        
        for attempt in range(max_retries):
            try:
                logger.info(f"正在连接{task.tool_type}... (尝试 {attempt + 1}/{max_retries})")
                
                if self.controller.connect():
                    logger.info(f"{task.tool_type}连接成功")
                    return
                    
            except Exception as e:
                if attempt < max_retries - 1:
                    logger.warning(f"连接失败，{retry_delay}秒后重试...")
                    self._current_metrics.record_retry()
                    time.sleep(retry_delay)
                else:
                    raise ToolException(f"连接{task.tool_type}失败，已重试{max_retries}次: {e}")
    
    def _load_configuration(self, task: Task):
        """加载配置文件"""
        logger.info(f"正在加载配置文件: {task.config_path}")
        self.current_collector.add_log("INFO", f"正在加载配置文件: {task.config_path}")
        
        try:
            self.controller.open_configuration(task.config_path)
            logger.info("配置文件加载成功")
            self.current_collector.add_log("INFO", "配置文件加载成功")
        except Exception as e:
            raise ToolException(f"配置文件加载失败: {e}")
    
    def _start_measurement(self, task: Task):
        """启动测量/仿真"""
        logger.info("正在启动测量/仿真...")
        self.current_collector.add_log("INFO", "正在启动测量/仿真")
        
        try:
            if task.tool_type == TestToolType.CANOE.value:
                success = self.controller.start_measurement()
            else:
                success = self.controller.start_simulation()
            
            if success:
                logger.info("测量/仿真启动成功")
                self.current_collector.add_log("INFO", "测量/仿真启动成功")
            else:
                raise ToolException("测量/仿真启动失败")
                
        except Exception as e:
            raise ToolException(f"启动测量/仿真失败: {e}")
    
    def _execute_test_items(self, task: Task) -> list:
        """执行测试项"""
        results = []
        total_items = len(task.test_items)
        
        logger.info(f"开始执行{total_items}个测试项")
        self.current_collector.add_log("INFO", f"开始执行{total_items}个测试项")
        
        for i, test_item in enumerate(task.test_items, 1):
            # 检查是否被取消
            if self._stop_event.is_set():
                logger.info("任务被取消")
                self.current_collector.add_log("INFO", "任务被取消")
                break
            
            progress = int((i / total_items) * 100)
            logger.info(f"执行测试项 {i}/{total_items}: {test_item.name}")
            
            # 更新进度
            self._update_task_status(
                task.task_id, 
                TaskStatus.RUNNING, 
                f"执行测试项 {i}/{total_items}: {test_item.name}",
                progress
            )
            
            # 执行测试项
            result = self._execute_single_item(test_item)
            results.append(result)
            
            # 收集结果
            self.current_collector.add_test_result(result)
            
            # 上报进度
            self._report_progress(task.task_id, test_item, result)
            
            # 短暂延时
            time.sleep(0.5)
        
        return results
    
    def _execute_single_item(self, test_item) -> TestResult:
        """执行单个测试项"""
        try:
            if test_item.type == TestItemType.SIGNAL_CHECK.value:
                actual_value = self.controller.get_signal(test_item.signal_name)
                passed = abs(actual_value - test_item.expected_value) < 0.01 if actual_value is not None else False
                
                return TestResult(
                    name=test_item.name,
                    type=test_item.type,
                    expected=test_item.expected_value,
                    actual=actual_value,
                    passed=passed
                )
            
            elif test_item.type == TestItemType.SIGNAL_SET.value:
                success = self.controller.set_signal(test_item.signal_name, test_item.value)
                
                return TestResult(
                    name=test_item.name,
                    type=test_item.type,
                    success=success
                )
            
            elif test_item.type == TestItemType.TEST_MODULE.value:
                result = self.controller.run_test_module(test_item.name)
                
                return TestResult(
                    name=test_item.name,
                    type=test_item.type,
                    verdict=result.get("verdict", "UNKNOWN"),
                    details=result
                )
            
            else:
                return TestResult(
                    name=test_item.name,
                    type=test_item.type,
                    error=f"未知的测试项类型: {test_item.type}"
                )
                
        except Exception as e:
            logger.error(f"执行测试项失败: {test_item.name}, 错误: {e}")
            return TestResult(
                name=test_item.name,
                type=test_item.type,
                error=f"执行失败: {e}"
            )
    
    def _stop_measurement(self, task: Task):
        """停止测量/仿真"""
        logger.info("正在停止测量/仿真...")
        self.current_collector.add_log("INFO", "正在停止测量/仿真")
        
        try:
            if task.tool_type == TestToolType.CANOE.value:
                self.controller.stop_measurement()
            else:
                self.controller.stop_simulation()
            
            logger.info("测量/仿真停止成功")
            self.current_collector.add_log("INFO", "测量/仿真停止成功")
            
        except Exception as e:
            logger.warning(f"停止测量/仿真失败: {e}")
            self.current_collector.add_log("WARNING", f"停止测量/仿真失败: {e}")
    
    def _complete_task(self, task: Task, results: list):
        """完成任务"""
        duration = time.time() - self._start_time
        
        logger.info(f"任务执行完成: {task.task_id}, 耗时: {duration:.1f}秒")
        
        # 完成结果收集
        task_result = self.current_collector.finalize(TaskStatus.COMPLETED.value)
        
        # 上报最终结果
        self._report_final_result(task.task_id, task_result)
        
        # 更新状态
        self._update_task_status(task.task_id, TaskStatus.COMPLETED, f"任务执行完成，耗时{duration:.1f}秒")
    
    def _fail_task(self, task: Task, error_message: str):
        """任务失败"""
        logger.error(f"任务失败: {task.task_id}, 错误: {error_message}")
        
        # 完成结果收集（失败状态）
        if self.current_collector:
            task_result = self.current_collector.finalize(TaskStatus.FAILED.value, error_message)
            self._report_final_result(task.task_id, task_result)
        
        # 更新状态
        self._update_task_status(task.task_id, TaskStatus.FAILED, error_message)
    
    def _cleanup(self):
        """清理资源"""
        try:
            if self.controller:
                self.controller.disconnect()
                self.controller = None
        except Exception as e:
            logger.warning(f"清理控制器资源失败: {e}")
    
    def cancel_task(self) -> bool:
        """取消当前任务"""
        if self.current_task is None:
            logger.warning("没有正在执行的任务")
            return False
        
        logger.info(f"正在取消任务: {self.current_task.task_id}")
        self._stop_event.set()
        return True
    
    def _update_task_status(self, task_no: str, status: TaskStatus, message: str = None, progress: int = None):
        """更新任务状态"""
        if self.current_collector:
            self.current_collector.add_status_update(status.value, message, progress)
        
        if self.message_sender:
            self.message_sender({
                "type": "TASK_STATUS",
                "taskNo": task_no,
                "status": status.value,
                "message": message,
                "progress": progress,
                "timestamp": int(time.time() * 1000)
            })
    
    def _report_progress(self, task_no: str, test_item, result: TestResult):
        """上报执行进度"""
        if self.message_sender:
            self.message_sender({
                "type": "LOG_STREAM",
                "taskNo": task_no,
                "level": "INFO",
                "message": f"执行测试项: {test_item.name}",
                "result": result.to_dict(),
                "timestamp": int(time.time() * 1000)
            })
    
    def _report_final_result(self, task_no: str, task_result):
        """上报最终结果"""
        if self.message_sender:
            self.message_sender({
                "type": "RESULT_REPORT",
                "taskNo": task_no,
                "status": task_result.status,
                "results": [r.to_dict() for r in task_result.results],
                "summary": task_result.summary,
                "timestamp": int(time.time() * 1000)
            })
    
    def get_current_status(self) -> Dict[str, Any]:
        """获取当前状态"""
        if not self.current_task:
            return {
                "status": "idle",
                "queue_size": self._task_queue.get_queue_size(),
                "processing_count": self._task_queue.get_processing_count()
            }
        
        return {
            "status": "running",
            "task_id": self.current_task.task_id,
            "tool_type": self.current_task.tool_type,
            "start_time": datetime.fromtimestamp(self._start_time).isoformat(),
            "duration": time.time() - self._start_time,
            "queue_size": self._task_queue.get_queue_size(),
            "processing_count": self._task_queue.get_processing_count(),
            "metrics": self._current_metrics.get_summary() if self._current_metrics else None
        }

# 保持向后兼容
TaskExecutor = TaskExecutorProduction