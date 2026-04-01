"""
任务执行核心引擎 - 生产环境增强版
集成熔断器、重试、性能监控、配置热更新等功能
"""
import os
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
from utils.report_client import ReportClient
from config.settings import get_config as get_runtime_config
from models.task import Task, TaskStatus, TestToolType, TestResult, TestItemType, TaskResult
from models.result import CaseResult, ExecutionResult
from models.executor_task import task_queue as global_task_queue, TaskStatus as ExecutorTaskStatus
from core.result_collector import ResultCollector
from core.config_manager import TestConfigManager
from core.adapters import create_adapter_with_wrapper, TestToolType
from core.case_mapping_manager import get_case_mapping_manager
from core.execution_observability import (
    ExecutionLifecycleStage,
    get_execution_observability_manager,
)
from core.status_monitor import get_status_monitor

logger = get_logger("task_executor_production")


def _get_runtime_config_manager():
    """Return the active facade-backed config manager."""
    return get_runtime_config()


def _ensure_observability_context(task: Task):
    """Ensure direct executor/reporting calls still have an observability context."""
    observability_manager = get_execution_observability_manager()
    try:
        observability_manager.get_snapshot(task.task_id)
    except KeyError:
        observability_manager.create_context(
            task_no=task.task_id,
            device_id=getattr(task, "deviceId", None) or getattr(task, "device_id", "") or "",
            tool_type=getattr(task, "toolType", None) or getattr(task, "tool_type", "") or "",
        )
    return observability_manager

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
        self.logger = logger  # 实例logger，用于实例方法
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
        self._executor = ThreadPoolExecutor(max_workers=2, thread_name_prefix="report_")  # 异步上报
        self._running = False
        self._worker_thread = None
        self.max_workers = 1  # 最大并发任务数

        # 性能监控
        self._current_metrics = None

        # 配置管理器
        self.config_manager: Optional[TestConfigManager] = None

        # 上报客户端
        self.report_client = ReportClient()

        self.logger.info("任务执行器（生产环境版）初始化完成")
    
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
        # 获取任务ID
        task_id = getattr(task, 'task_id', None) or getattr(task, 'taskNo', None)
        if task_id is None:
            logger.error("execute_task 失败: 任务缺少 task_id 属性")
            return False

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

            # 显式检查 test_items 是否为空
            if not test_items:
                logger.error(f"execute_task 失败: 任务 {task_id} 的 test_items 为空")
                return False

            task_data = {
                'taskNo': task_id,
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
        except Exception as e:
            logger.error(f"execute_task 异常: {e}")
            return False

        # 添加到内部执行队列
        if self._task_queue.put(task):
            # 同时添加到全局队列供API查询（将TDMTask转换为内部Task格式）
            from models.executor_task import Task as ExecutorTask, TaskStatus as ExecutorTaskStatus
            exec_task = ExecutorTask(
                id=task_id,
                name=getattr(task, 'taskName', '') or getattr(task, 'name', ''),
                task_type=getattr(task, 'task_type', 'test_module'),
                priority=1,
                status=ExecutorTaskStatus.PENDING.value,
                params={
                    'tool_type': getattr(task, 'tool_type', 'canoe'),
                    'config_path': getattr(task, 'config_path', None),
                    'variables': getattr(task, 'variables', {})
                },
                timeout=getattr(task, 'timeout', 3600),
                metadata={
                    'taskNo': task_id,
                    'projectNo': getattr(task, 'projectNo', ''),
                    'deviceId': getattr(task, 'deviceId', None),
                    'caseCount': len(getattr(task, 'caseList', []) or [])
                }
            )
            global_task_queue.add(exec_task)
            logger.info(f"任务已加入队列: {task_id}")
            return True
        else:
            logger.error(f"任务加入队列失败: {task_id}")
            return False
    
    @TASK_CIRCUIT_BREAKER
    def _execute_task_production(self, task: Task):
        """生产环境任务执行"""
        logger.info(f"[_execute_task_production] 开始执行任务: task_id={task.task_id}, task_type={getattr(task, 'task_type', 'N/A')}, tool_type={getattr(task, 'tool_type', 'N/A')}")

        with self._lock:
            if self.current_task is not None:
                logger.warning("已有任务正在执行")
                return

            self.current_task = task
            self._stop_event.clear()
            self._start_time = time.time()

        # 初始化性能指标
        self._current_metrics = TaskMetrics(task.task_id)
        self._task_queue.mark_processing(task.task_id, task)
        observability_manager = _ensure_observability_context(task)

        try:
            logger.info(f"开始执行任务: {task.task_id}")
            self._current_metrics.record_step('start', 0)
            observability_manager.transition(task.task_id, ExecutionLifecycleStage.PREPARING)
            record_metric(
                'task.preparing',
                1,
                {'task_no': task.task_id, 'tool_type': task.tool_type or ''},
            )

            # 初始化结果收集器
            self.current_collector = ResultCollector(task.task_id)

            # 更新状态为运行中
            self._update_task_status(task.task_id, TaskStatus.RUNNING)

            logger.info(f"[_execute_task_production] 任务初始化完成，开始配置准备阶段")

            # ========== 配置准备 ==========
            cfg_path = None

            # 1. 优先使用任务指定的配置路径
            if task.config_path:
                cfg_path = task.config_path
                logger.info(f"使用任务指定的配置路径: {cfg_path}")

            # 2. 尝试从配置管理器准备配置
            elif task.config_name or task.base_config_dir:
                self.current_collector.add_log("INFO", "正在准备测试配置...")

                base_dir = task.base_config_dir or _get_runtime_config_manager().get(
                    'config_base_dir',
                    r'D:\TAMS\DTTC_CONFIG',
                )
                logger.info(f"[_execute_task_production] base_dir={base_dir}, config_name={task.config_name}")
                self.config_manager = TestConfigManager(base_config_dir=base_dir)

                # 确定配置名称
                config_name = task.config_name
                if not config_name and task.config_path:
                    config_name = os.path.basename(task.config_path).replace('.cfg', '')

                if config_name:
                    config_info = self.config_manager.prepare_config_for_task(
                        task_config_name=config_name,
                        test_cases=[item.to_dict() for item in task.test_items],
                        variables=task.variables
                    )
                    cfg_path = config_info.get('cfg_path')
                    ini_path = config_info.get('ini_path')

                    self.current_collector.add_log("INFO", f"cfg文件: {cfg_path}")
                    if ini_path:
                        self.current_collector.add_log("INFO", f"ini文件: {ini_path}")
                    logger.info(f"配置准备完成: cfg={cfg_path}, ini={ini_path}")

            # 3. 尝试从用例映射获取配置路径
            # 检查是否是TSMaster用例（TSMaster使用ini_config，不需要cfg_path）
            is_tsmaster_case = False
            if not cfg_path and task.test_items:
                mapping_manager = get_case_mapping_manager()
                for test_item in task.test_items:
                    case_no = getattr(test_item, 'caseNo', None) or getattr(test_item, 'case_no', None)
                    if case_no:
                        mapping = mapping_manager.get_mapping(case_no)
                        if mapping and mapping.enabled:
                            # 检查category是否是tsmaster
                            if mapping.category and mapping.category.lower() == 'tsmaster':
                                is_tsmaster_case = True
                                logger.info(f"检测到TSMaster用例: case_no={case_no}, 使用ini_config执行")
                                break
                            elif mapping.script_path:
                                cfg_path = mapping.script_path
                                logger.info(f"从用例映射获取配置路径: case_no={case_no}, cfg_path={cfg_path}")
                                break

            # 4. 如果仍然没有配置路径，检查是否是TSMaster用例
            if not cfg_path and not is_tsmaster_case:
                raise TaskException("未指定配置文件路径(configPath/configName)，也未找到用例映射，无法执行任务")

            # 根据工具类型选择控制器（使用适配器工厂）
            step_start = time.time()
            try:
                tool_type_lower = task.tool_type.lower() if task.tool_type else ""
                if tool_type_lower == TestToolType.CANOE.value:
                    adapter_type = TestToolType.CANOE
                elif tool_type_lower == TestToolType.TSMASTER.value:
                    adapter_type = TestToolType.TSMASTER
                elif tool_type_lower == TestToolType.TTWORKBENCH.value:
                    adapter_type = TestToolType.TTWORKBENCH
                else:
                    raise TaskException(f"不支持的测试工具类型: {task.tool_type}")
                
                # 使用适配器工厂创建控制器（带包装器，兼容原有接口）
                self.controller = create_adapter_with_wrapper(adapter_type)
                logger.info(f"已创建适配器: {adapter_type.value}")
            except Exception as e:
                raise TaskException(f"创建适配器失败: {e}")

            self._current_metrics.record_step('controller_init', time.time() - step_start)

            # 连接测试软件（带重试）
            step_start = time.time()
            self._connect_tool_with_retry(task)
            self._current_metrics.record_step('connect_tool', time.time() - step_start)

            # 加载配置文件（TSMaster用例不需要加载.cfg文件）
            step_start = time.time()
            tool_type_for_load = task.tool_type.lower() if task.tool_type else ""
            if tool_type_for_load != TestToolType.TSMASTER.value and cfg_path:
                self._load_configuration_by_path(cfg_path)
                current_cfg_path = cfg_path  # 记录当前加载的配置路径
            else:
                current_cfg_path = ""
                logger.info("TSMaster用例跳过配置文件加载")
            self._current_metrics.record_step('load_config', time.time() - step_start)

            # 启动测量/仿真（对于TSMaster使用完整的测试执行流程）
            step_start = time.time()
            tool_type_lower = task.tool_type.lower() if task.tool_type else ""
            observability_manager.transition(task.task_id, ExecutionLifecycleStage.EXECUTING)
            record_metric(
                'task.executing',
                1,
                {'task_no': task.task_id, 'tool_type': task.tool_type or ''},
            )
            if tool_type_lower == TestToolType.TSMASTER.value:
                # TSMaster 使用 RPC 批量执行，已在 _start_test_execution 中完成所有用例执行
                # 不需要再调用 _execute_test_items（会导致双重执行）
                self._start_test_execution(task)
                # 获取 TSMaster 测试结果
                results = self._collect_tsmaster_results(task)
            else:
                self._start_measurement(task)
                # 执行测试项
                step_start = time.time()
                results = self._execute_test_items(task)
                self._current_metrics.record_step('execute_items', time.time() - step_start)
            self._current_metrics.record_step('start_measurement', time.time() - step_start)

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
                self.config_manager = None
                self._start_time = None
    
    def _connect_tool_with_retry(self, task: Task):
        """带重试的连接工具"""
        runtime_config = _get_runtime_config_manager()
        max_retries = runtime_config.get('canoe.max_retries', 3)
        retry_delay = runtime_config.get('canoe.retry_delay', 2.0)
        
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

        # 如果所有重试都返回False而没有异常，说明连接失败但未抛出异常
        raise ToolException(f"连接{task.tool_type}失败: 适配器返回失败状态")

    def _load_configuration(self, task: Task):
        """加载配置文件"""
        logger.info(f"正在加载配置文件: {task.config_path}")
        self.current_collector.add_log("INFO", f"正在加载配置文件: {task.config_path}")

        try:
            result = self.controller.open_configuration(task.config_path)
            if result:
                logger.info("配置文件加载成功")
                self.current_collector.add_log("INFO", "配置文件加载成功")
            else:
                # 获取更详细的错误信息
                error_msg = "未知错误"
                if hasattr(self.controller, 'adapter') and hasattr(self.controller.adapter, 'last_error'):
                    error_msg = self.controller.adapter.last_error or error_msg
                elif hasattr(self.controller, 'last_error'):
                    error_msg = self.controller.last_error or error_msg
                raise ToolException(f"配置文件加载失败: {error_msg}")
        except ToolException:
            raise
        except Exception as e:
            raise ToolException(f"配置文件加载失败: {e}")

    def _load_configuration_by_path(self, config_path: str):
        """通过路径加载配置文件"""
        logger.info(f"正在加载配置文件: {config_path}")
        self.current_collector.add_log("INFO", f"正在加载配置文件: {config_path}")

        try:
            result = self.controller.open_configuration(config_path)
            if result:
                logger.info("配置文件加载成功")
                self.current_collector.add_log("INFO", "配置文件加载成功")
            else:
                # 获取更详细的错误信息
                error_msg = "未知错误"
                if hasattr(self.controller, 'adapter') and hasattr(self.controller.adapter, 'last_error'):
                    error_msg = self.controller.adapter.last_error or error_msg
                elif hasattr(self.controller, 'last_error'):
                    error_msg = self.controller.last_error or error_msg
                raise ToolException(f"配置文件加载失败: {error_msg}")
        except ToolException:
            raise
        except Exception as e:
            raise ToolException(f"配置文件加载失败: {e}")
    
    def _start_measurement(self, task: Task):
        """启动测量/仿真"""
        logger.info("正在启动测量/仿真...")
        self.current_collector.add_log("INFO", "正在启动测量/仿真")

        try:
            tool_type_lower = task.tool_type.lower() if task.tool_type else ""
            if tool_type_lower == TestToolType.CANOE.value:
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

    def _start_test_execution(self, task: Task):
        """
        Start TSMaster test execution with full RPC flow.

        Follows the reference example flow:
        1. Build test case selection string from task.test_items
        2. Call adapter.start_test_execution(test_cases, wait_for_complete=False)
        3. Wait for test completion with adapter.wait_for_test_complete()

        Args:
            task: Task object with test_items containing case info
        """
        tool_type_lower = task.tool_type.lower() if task.tool_type else ""

        if tool_type_lower != TestToolType.TSMASTER.value:
            # Fall back to regular start_measurement for non-TSMaster
            self._start_measurement(task)
            return

        self.logger.info("使用TSMaster测试执行流程")

        try:
            # Build test case selection string from test_items
            test_cases = self._build_tsmaster_test_cases_string(task)

            if test_cases:
                self.logger.info(f"执行TSMaster测试用例: {test_cases}")
                self.current_collector.add_log("INFO", f"选择测试用例: {test_cases}")

                # Call adapter's start_test_execution with test cases
                success = self.controller.start_test_execution(
                    test_cases=test_cases,
                    wait_for_complete=False,  # We wait separately
                    timeout=task.timeout
                )

                if not success:
                    raise ToolException("TSMaster测试执行启动失败")

                # Wait for test to complete
                self.logger.info("等待TSMaster测试执行完成...")
                if not self.controller.wait_for_test_complete(timeout=task.timeout):
                    self.logger.warning("TSMaster测试执行超时")
            else:
                self.logger.info("无测试用例，启动仿真")
                self._start_measurement(task)

        except Exception as e:
            raise ToolException(f"TSMaster测试执行失败: {e}")

    def _collect_tsmaster_results(self, task: Task) -> list:
        """
        收集 TSMaster 测试结果。

        TSMaster 的测试结果已经在 _start_test_execution 中通过 RPC 批量执行完成，
        这里从测试报告信息中收集结果，避免双重执行。

        Args:
            task: Task object

        Returns:
            list: TestResult 列表
        """
        results = []
        try:
            report_info = self.controller.get_test_report_info()
            if report_info:
                self._current_report_info = report_info
                self.current_collector.add_log("INFO", f"测试报告路径: {report_info.get('report_path', 'N/A')}")
                self.current_collector.add_log("INFO", f"测试数据路径: {report_info.get('testdata_path', 'N/A')}")
                self.current_collector.add_log("INFO", f"通过: {report_info.get('passed', 0)}, 失败: {report_info.get('failed', 0)}")

                # 从报告信息构建结果
                passed = report_info.get('passed', 0)
                failed = report_info.get('failed', 0)
                total = passed + failed

                if total > 0:
                    # 遍历 task.test_items 构建结果
                    for test_item in task.test_items:
                        case_no = getattr(test_item, 'caseNo', None) or getattr(test_item, 'case_no', None)
                        item_name = test_item.name

                        # 尝试从 report_info 的详细结果中获取 verdict
                        verdict = "UNKNOWN"
                        if report_info.get('details'):
                            for detail in report_info.get('details', []):
                                if detail.get('name') == item_name or detail.get('case_no') == case_no:
                                    verdict = detail.get('verdict', 'PASS' if detail.get('passed') else 'FAIL')
                                    break

                        result = TestResult(
                            name=item_name,
                            type=getattr(test_item, 'type', None) or 'test_module',
                            verdict=verdict,
                            details={
                                'case_no': case_no,
                                'report_info': report_info
                            }
                        )
                        results.append(result)
                        self.current_collector.add_test_result(result)
                else:
                    # 没有测试结果，生成未知状态
                    self.logger.warning("TSMaster 测试报告无详细结果")
                    for test_item in task.test_items:
                        result = TestResult(
                            name=test_item.name,
                            type=getattr(test_item, 'type', None) or 'test_module',
                            verdict="UNKNOWN",
                            details={'report_info': report_info}
                        )
                        results.append(result)
                        self.current_collector.add_test_result(result)
            else:
                self.logger.warning("获取 TSMaster 报告信息为空")
                # 生成空结果
                for test_item in task.test_items:
                    result = TestResult(
                        name=test_item.name,
                        type=getattr(test_item, 'type', None) or 'test_module',
                        error="无法获取测试报告信息"
                    )
                    results.append(result)
                    self.current_collector.add_test_result(result)

        except Exception as e:
            self.logger.error(f"收集 TSMaster 测试结果失败: {e}")
            # 出错时生成错误结果
            for test_item in task.test_items:
                result = TestResult(
                    name=test_item.name,
                    type=getattr(test_item, 'type', None) or 'test_module',
                    error=f"结果收集失败: {e}"
                )
                results.append(result)
                self.current_collector.add_test_result(result)

        return results

    def _build_tsmaster_test_cases_string(self, task: Task) -> str:
        """
        Build TSMaster test case selection string from task.test_items.

        从用例映射的ini_config获取TSMaster用例选择字符串
        Format: "TG1_TC1=1,TG1_TC2=1" or similar

        Args:
            task: Task object

        Returns:
            Test case selection string, empty string if no valid cases
        """
        if not task.test_items:
            return ""

        mapping_manager = get_case_mapping_manager()
        cases = []

        for item in task.test_items:
            case_no = getattr(item, 'caseNo', None)
            if case_no:
                mapping = mapping_manager.get_mapping(case_no)
                if mapping and mapping.ini_config:
                    # ini_config 直接存储 TSMaster 用例选择字符串
                    # 如 "TG1_TC1=1" 或 "TG1_TC1=1,TG1_TC2=1"
                    cases.append(mapping.ini_config)

        # 拼接多个 ini_config
        # 例如：["TG1_TC1=1,TG1_TC2=1", "TG1_TC3=1"] -> "TG1_TC1=1,TG1_TC2=1,TG1_TC3=1"
        return ",".join(cases) if cases else ""

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

    def _find_cfg_in_directory(self, directory: str, case_id: str = None) -> Optional[str]:
        """
        在目录中查找.cfg文件

        Args:
            directory: 目录路径
            case_id: 用例ID（可选，用于匹配文件名）

        Returns:
            .cfg文件路径，未找到返回None
        """
        if not os.path.isdir(directory):
            return None

        # 获取目录下所有.cfg文件
        cfg_files = [f for f in os.listdir(directory) if f.endswith('.cfg')]

        if not cfg_files:
            logger.warning(f"目录中未找到.cfg文件: {directory}")
            return None

        # 如果提供了case_id，尝试匹配
        if case_id:
            # 尝试精确匹配 case_id.cfg
            for cfg_file in cfg_files:
                if cfg_file.replace('.cfg', '') == case_id:
                    return os.path.join(directory, cfg_file)

            # 尝试模糊匹配（case_id作为前缀或后缀）
            for cfg_file in cfg_files:
                cfg_name = cfg_file.replace('.cfg', '')
                if case_id in cfg_name or cfg_name in case_id:
                    return os.path.join(directory, cfg_file)

        # 如果只有一个.cfg文件，直接返回
        if len(cfg_files) == 1:
            return os.path.join(directory, cfg_files[0])

        # 多于一个.cfg文件但没有匹配，返回第一个
        logger.warning(f"目录中有多个.cfg文件且未匹配到特定的case_id，返回第一个: {cfg_files[0]}")
        return os.path.join(directory, cfg_files[0])

    def _execute_test_items_with_config(self, task: Task, current_cfg_path: str = None) -> list:
        """
        使用配置执行测试项 - 新的配置驱动执行方式

        执行流程：
        1. 遍历任务中的测试用例
        2. 对每个用例，根据用例映射获取cfg和ini配置
        3. 重新加载cfg配置（如有变化）
        4. 根据用例配置更新ini文件
        5. 触发测试执行
        6. 等待并获取结果

        Args:
            task: 任务对象
            current_cfg_path: 当前已加载的配置路径
        """
        results = []
        total_items = len(task.test_items)

        logger.info(f"[_execute_test_items_with_config] 开始执行 {total_items} 个测试项（配置驱动方式）")
        self.current_collector.add_log("INFO", f"开始执行{total_items}个测试项")

        # 初始化配置管理器（用于生成ini）
        base_dir = task.base_config_dir or _get_runtime_config_manager().get(
            'config_base_dir',
            r'D:\TAMS\DTTC_CONFIG',
        )
        logger.info(f"[_execute_test_items_with_config] base_dir={base_dir}")
        test_config_manager = TestConfigManager(base_config_dir=base_dir)

        for i, test_item in enumerate(task.test_items, 1):
            # 检查是否被取消
            if self._stop_event.is_set():
                logger.info("任务被取消")
                self.current_collector.add_log("INFO", "任务被取消")
                break

            # 通过用例映射查找脚本标识和配置
            mapping_manager = get_case_mapping_manager()
            case_no = getattr(test_item, 'caseNo', None) or getattr(test_item, 'case_no', None)
            logger.info(f"[_execute_test_items_with_config] 第{i}项: test_item.name={test_item.name}, case_no={case_no}")
            logger.info(f"[_execute_test_items_with_config] 从映射管理器获取映射: case_no={case_no}")

            if case_no:
                mapping = mapping_manager.get_mapping(case_no)
                logger.info(f"[_execute_test_items_with_config] 获取到的映射: {mapping}")
                if mapping:
                    logger.info(f"[_execute_test_items_with_config] 映射详情: case_no={mapping.case_no}, enabled={mapping.enabled}, script_path={mapping.script_path}")
            else:
                mapping = None
                logger.info(f"[_execute_test_items_with_config] case_no为空，跳过映射查找")

            if mapping and mapping.enabled:
                case_id = mapping.case_no
                script_path = mapping.script_path
                ini_config = mapping.ini_config
                para_config = mapping.para_config
                logger.info(f"[_execute_test_items_with_config] 用例映射找到: name={test_item.name}, case_id={case_id}, script_path={script_path}, ini_config={ini_config[:100] if ini_config else None}...")

                # 如果script_path是目录，在目录中查找.cfg文件
                if script_path and os.path.isdir(script_path):
                    script_path = self._find_cfg_in_directory(script_path, case_id)
                    logger.info(f"[_execute_test_items_with_config] 在目录中找到cfg文件: {script_path}")
            else:
                case_id = test_item.name
                script_path = None
                ini_config = None
                para_config = None
                if mapping is None:
                    logger.info(f"[_execute_test_items_with_config] 未找到用例映射，使用原始名称: {test_item.name}")
                else:
                    logger.info(f"[_execute_test_items_with_config] 用例未启用: {case_no}")

            progress = int((i / total_items) * 100)
            logger.info(f"[_execute_test_items_with_config] 执行测试项 {i}/{total_items}: {test_item.name} (case_id: {case_id}, script_path: {script_path})")

            # 更新进度
            self._update_task_status(
                task.task_id,
                TaskStatus.RUNNING,
                f"执行测试项 {i}/{total_items}: {test_item.name}",
                progress
            )

            # 如果用例有独立的script_path且与当前加载的不同，需要重新加载配置
            if script_path and script_path != current_cfg_path:
                logger.info(f"切换配置: {current_cfg_path} -> {script_path}")
                self.current_collector.add_log("INFO", f"切换配置: {script_path}")

                # 停止测量
                self._stop_measurement(task)

                # 断开连接
                if self.controller:
                    try:
                        self.controller.disconnect()
                    except Exception as e:
                        logger.warning(f"断开连接失败: {e}")

                # 重新连接
                self._connect_tool_with_retry(task)

                # 加载新配置
                self._load_configuration_by_path(script_path)

                # 重新启动测量
                self._start_measurement(task)

                current_cfg_path = script_path

            # 如果用例有ini配置，生成ini文件
            if test_config_manager:
                try:
                    # 设置当前 cfg 路径，以便 ini 文件写入到正确位置
                    if script_path:
                        test_config_manager._current_cfg_path = script_path
                    elif current_cfg_path:
                        test_config_manager._current_cfg_path = current_cfg_path

                    # 获取属性参数（优先用例params，其次任务variables）
                    case_variables = getattr(test_item, 'params', {}) or task.variables
                    ini_info = test_config_manager._generate_ini_from_case_config(
                        case_no,
                        ini_config,
                        variables=case_variables,
                        para_config=para_config
                    )
                    logger.info(f"为用例 {case_no} 生成ini文件: {ini_info}")
                    self.current_collector.add_log("INFO", f"用例 {case_no} ini配置已更新")
                except Exception as e:
                    logger.error(f"生成ini配置失败: {e}")
                    self.current_collector.add_log("ERROR", f"生成ini配置失败: {e}")

            # 执行用例
            if hasattr(self.controller, 'run_test_case_with_config'):
                result = self.controller.run_test_case_with_config(
                    test_case_name=case_id,
                    config={
                        'dtc_info': getattr(test_item, 'dtc_info', None),
                        'params': getattr(test_item, 'params', {}),
                        'repeat': getattr(test_item, 'repeat', 1)
                    },
                    timeout=task.timeout // max(len(task.test_items), 1)
                )

                if result.get('success'):
                    test_result = TestResult(
                        name=test_item.name,
                        type=test_item.type or 'test_case',
                        verdict="PASS" if result.get('result') == 0 else "FAIL",
                        details=result
                    )
                else:
                    test_result = TestResult(
                        name=test_item.name,
                        type=test_item.type or 'test_case',
                        error=result.get('error', '执行失败'),
                        details=result
                    )
            else:
                test_result = self._execute_single_item(test_item)

            results.append(test_result)
            self.current_collector.add_test_result(test_result)
            self._report_progress(task.task_id, test_item, test_result)

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
            tool_type_lower = task.tool_type.lower() if task.tool_type else ""
            if tool_type_lower == TestToolType.CANOE.value:
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
        observability_manager = _ensure_observability_context(task)
        observability_manager.transition(task.task_id, ExecutionLifecycleStage.REPORTING)
        record_metric(
            'task.reporting',
            1,
            {'task_no': task.task_id, 'tool_type': task.tool_type or ''},
        )

        logger.info(f"任务执行完成: {task.task_id}, 耗时: {duration:.1f}秒")

        # 完成结果收集
        task_result = self.current_collector.finalize(TaskStatus.COMPLETED.value)

        # 上报最终结果到WebSocket客户端
        self._report_final_result(task.task_id, task_result)

        # 上报到远端服务器（包含报告文件上传）- 异步执行避免阻塞
        report_file_path = getattr(self, '_current_report_info', None).get('report_path') if hasattr(self, '_current_report_info') and self._current_report_info else None
        self._executor.submit(self._report_to_remote, task, task_result, report_file_path)

        # 更新状态
        self._update_task_status(task.task_id, TaskStatus.COMPLETED, f"任务执行完成，耗时{duration:.1f}秒")
    
    def _fail_task(self, task: Task, error_message: str):
        """任务失败"""
        logger.error(f"任务失败: {task.task_id}, 错误: {error_message}")
        observability_manager = get_execution_observability_manager()
        observability_manager.fail(
            task.task_id,
            error_code="TASK_FAILED",
            error_message=error_message,
            retryable=False,
        )
        record_metric(
            'task.failed',
            1,
            {'task_no': task.task_id, 'stage': observability_manager.get_snapshot(task.task_id).get('failed_stage') or ''},
        )

        # 为所有未执行的测试项生成失败结果
        if task.test_items:
            logger.info(f"[_fail_task] 为 {len(task.test_items)} 个测试项生成失败结果")
            for test_item in task.test_items:
                item_name = getattr(test_item, 'name', None) or getattr(test_item, 'caseName', '') or '未命名用例'
                item_type = getattr(test_item, 'type', None) or getattr(test_item, 'caseType', '') or 'test_module'
                case_no = getattr(test_item, 'caseNo', None) or getattr(test_item, 'case_no', None)

                # 检查是否已有结果，避免重复添加
                existing_names = [r.name for r in self.current_collector.results] if self.current_collector else []
                if item_name not in existing_names:
                    failed_result = TestResult(
                        name=item_name,
                        type=item_type,
                        verdict="FAIL",
                        error=error_message,
                        details={
                            "case_no": case_no,
                            "status": "failed",
                            "reason": error_message
                        }
                    )
                    if self.current_collector:
                        self.current_collector.add_test_result(failed_result)
                        logger.info(f"[_fail_task] 添加失败结果: {item_name}, case_no={case_no}")

        # 完成结果收集（失败状态）
        if self.current_collector:
            task_result = self.current_collector.finalize(TaskStatus.FAILED.value, error_message)
            logger.info(f"[_fail_task] 结果收集完成: results={len(task_result.results)}")
            self._report_final_result(task.task_id, task_result)

            # 上报到远端服务器
            logger.info(f"[_fail_task] 开始调用远端上报")
            self._report_to_remote(task, task_result)
        else:
            logger.warning(f"[_fail_task] current_collector 为 None，无法上报结果")

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
    
    def cancel_task(self, task_id: str = None) -> bool:
        """取消指定任务"""
        if self.current_task is None:
            logger.warning("没有正在执行的任务")
            return False

        # 如果指定了task_id，检查是否匹配
        if task_id and self.current_task.task_id != task_id:
            logger.warning(f"任务ID不匹配: 当前执行的是 {self.current_task.task_id}，要取消的是 {task_id}")
            return False

        logger.info(f"正在取消任务: {self.current_task.task_id}")
        self._stop_event.set()

        # 更新任务状态为已取消
        self._update_task_status(self.current_task.task_id, TaskStatus.CANCELLED, "任务被用户取消")

        return True

    def submit_task(self, task) -> bool:
        """
        提交任务到执行队列

        Args:
            task: 任务对象 (models.executor_task.Task)

        Returns:
            是否提交成功
        """
        try:
            # 获取任务ID（兼容不同任务类型）
            task_id = getattr(task, 'id', None) or getattr(task, 'task_id', None)
            if task_id is None:
                logger.error("任务缺少ID属性，无法提交")
                return False

            # 添加到全局任务队列
            if not global_task_queue.add(task):
                logger.warning(f"任务 {task_id} 已存在于队列中")
                return False

            # 转换为执行任务的 Task 模型 (TDM2.0格式)
            from models.task import Task as ExecTask

            exec_task = ExecTask(
                taskNo=task_id,
                taskName=getattr(task, 'name', '') or getattr(task, 'taskName', ''),
                timeout=getattr(task, 'timeout', 3600),
                toolType=getattr(task, 'params', {}).get('tool_type', 'canoe') if hasattr(task, 'params') else 'canoe',
                configPath=getattr(task, 'params', {}).get('config_path') if hasattr(task, 'params') else None,
                variables=getattr(task, 'params', {}).get('variables', {}) if hasattr(task, 'params') else {}
            )

            # 添加到内部执行队列
            self._task_queue.put(exec_task)

            logger.info(f"任务 {task_id} 已提交到执行队列")
            return True

        except Exception as e:
            logger.error(f"提交任务失败: {e}")
            return False

    def retry_task(self, task_id: str):
        """
        重试任务

        Args:
            task_id: 任务ID

        Returns:
            新任务对象，失败返回 None
        """
        try:
            # 从全局队列获取原任务
            old_task = global_task_queue.get_task(task_id)
            if not old_task:
                logger.warning(f"任务 {task_id} 不存在")
                return None

            # 检查是否可以重试
            if not old_task.can_retry():
                logger.warning(f"任务 {task_id} 无法重试")
                return None

            # 创建新任务
            from models.executor_task import Task as ExecutorTask, TaskPriority

            new_task = ExecutorTask(
                name=old_task.name,
                task_type=old_task.task_type,
                priority=old_task.priority,
                params=old_task.params,
                timeout=old_task.timeout,
                max_retries=old_task.max_retries,
                created_by=old_task.created_by,
                metadata=old_task.metadata
            )

            # 提交新任务
            if self.submit_task(new_task):
                # 更新原任务的重试计数
                old_task.retry_count += 1
                global_task_queue.update_task_status(
                    task_id,
                    old_task.status,
                    error_message=f"已重试，新任务ID: {new_task.id}"
                )
                return new_task

            return None

        except Exception as e:
            logger.error(f"重试任务失败: {e}")
            return None
    
    def _update_task_status(self, task_no: str, status: TaskStatus, message: str = None, progress: int = None):
        """更新任务状态"""
        if self.current_collector:
            self.current_collector.add_status_update(status.value, message, progress)

        # 更新全局任务队列的状态
        try:
            global_task_queue.update_task_status(
                task_no,
                status.value,
                error_message=message
            )
        except Exception as e:
            logger.warning(f"更新全局任务队列状态失败: {e}")

        # 更新 StatusMonitor 的任务状态
        try:
            monitor = get_status_monitor()
            from core.status_monitor import TaskStatus as MonitorTaskStatus

            # 状态映射
            status_mapping = {
                TaskStatus.PENDING: MonitorTaskStatus.PENDING,
                TaskStatus.RUNNING: MonitorTaskStatus.RUNNING,
                TaskStatus.COMPLETED: MonitorTaskStatus.COMPLETED,
                TaskStatus.FAILED: MonitorTaskStatus.FAILED,
                TaskStatus.CANCELLED: MonitorTaskStatus.CANCELLED,
            }

            monitor_status = status_mapping.get(status)

            if status == TaskStatus.COMPLETED or status == TaskStatus.FAILED or status == TaskStatus.CANCELLED:
                monitor.clear_current_task()
            else:
                monitor.update_task_status({
                    'task_id': task_no,
                    'tool_type': self.current_task.tool_type if self.current_task else None,
                    'progress': progress or 0,
                    'message': message
                }, monitor_status)
        except Exception as e:
            logger.warning(f"更新 StatusMonitor 任务状态失败: {e}")

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

    def _report_to_remote(self, task: Task, task_result: TaskResult, report_file_path: str = None):
        """
        上报任务结果到远端服务器

        Args:
            task: 任务对象
            task_result: 任务结果对象
            report_file_path: 报告文件路径（可选）
        """
        logger.info(f"[_report_to_remote] 开始上报任务结果: taskNo={task.task_id}, report_file_path={report_file_path}")

        try:
            # 构建 TDM2.0 格式的上报数据
            execution_result = self._build_execution_result(task, task_result)
            logger.info(f"[_report_to_remote] 构建执行结果完成: caseList数量={len(execution_result.caseList)}")

            # 构建上报数据
            report_data = execution_result.to_tdm2_format()

            # 添加额外信息
            report_data["projectNo"] = task.projectNo
            report_data["deviceId"] = task.deviceId
            report_data["taskName"] = task.taskName or task.taskNo or task.task_id
            report_data["toolType"] = task.toolType
            report_data["status"] = task_result.status
            report_data["summary"] = task_result.summary
            report_data["errorMessage"] = task_result.errorMessage
            report_data["timestamp"] = int(time.time() * 1000)

            if task_result.startTime:
                report_data["startTime"] = task_result.startTime.isoformat()
            if task_result.endTime:
                report_data["endTime"] = task_result.endTime.isoformat()

            # 如果有报告文件，先上传文件获取URL
            report_url = None
            if report_file_path:
                report_url = self._upload_report_file(report_file_path)
                if report_url:
                    logger.info(f"[_report_to_remote] 报告文件上传成功: {report_file_path} -> {report_url}")
                    # 将报告URL填充到caseList的reAddress字段
                    if "caseList" in report_data and report_data["caseList"]:
                        for case in report_data["caseList"]:
                            if not case.get("reAddress"):
                                case["reAddress"] = report_url
                else:
                    logger.warning(f"[_report_to_remote] 报告文件上传失败: {report_file_path}")

            logger.info(f"[_report_to_remote] 上报数据: taskNo={report_data.get('taskNo')}, caseList={len(report_data.get('caseList', []))}")

            # 直接执行上报
            success = self._do_report_direct(report_data)

            if success:
                logger.info(f"[_report_to_remote] 任务结果上报成功: {task.task_id}")
                record_metric('task.report.success', 1, {'task_no': task.task_id})
            else:
                logger.warning(f"[_report_to_remote] 任务结果上报失败: {task.task_id}")
                record_metric('task.report.failure', 1, {'task_no': task.task_id})
                # 上报失败时持久化以便后续重试
                self._persist_failed_report(report_data, task)

            _ensure_observability_context(task).finish(
                task.task_id,
                report_status='success' if success else 'failed',
            )
            record_metric(
                'task.finished',
                1,
                {'task_no': task.task_id, 'status': 'success' if success else 'failed'},
            )

        except Exception as e:
            logger.error(f"[_report_to_remote] 远端上报任务结果时出错: {e}", exc_info=True)
            record_metric('task.report.failure', 1, {'task_no': task.task_id})
            _ensure_observability_context(task).finish(
                task.task_id,
                report_status='failed',
            )
            record_metric(
                'task.finished',
                1,
                {'task_no': task.task_id, 'status': 'failed'},
            )
            # 异常时也持久化
            if 'report_data' in locals():
                self._persist_failed_report(report_data, task)

    def _persist_failed_report(self, report_data: Dict[str, Any], task: Task):
        """
        持久化失败的上报数据

        Args:
            report_data: 上报数据
            task: 任务对象
        """
        try:
            from core.failed_report_manager import get_failed_report_manager

            manager = get_failed_report_manager(self.config_manager)

            task_info = {
                'taskNo': task.task_id,
                'projectNo': getattr(task, 'projectNo', ''),
                'deviceId': getattr(task, 'deviceId', None),
                'toolType': getattr(task, 'toolType', None),
                'taskName': getattr(task, 'taskName', '') or getattr(task, 'name', '')
            }

            # 计算优先级（失败任务优先级更高）
            priority = 3 if report_data.get('status') in ['failed', 'FAILED'] else 0

            max_retries = 10
            if self.config_manager:
                max_retries = self.config_manager.get('report_retry.max_retries', 10)

            report_id = manager.add_failed_report(
                report_data=report_data,
                task_info=task_info,
                max_retries=max_retries,
                priority=priority
            )

            logger.info(f"失败报告已持久化: report_id={report_id}, task_no={task.task_id}")

        except Exception as e:
            logger.error(f"持久化失败报告时出错: {e}")

    def _upload_report_file(self, file_path: str) -> Optional[str]:
        """
        上传报告文件到文件服务器

        Args:
            file_path: 报告文件路径

        Returns:
            文件URL，上传失败返回None
        """
        logger.info(f"[_upload_report_file] 开始处理文件: {file_path}")

        report_client = getattr(self, 'report_client', None)
        if report_client is None:
            logger.error("[_upload_report_file] ReportClient 未初始化")
            return None

        if hasattr(report_client, 'reload_config'):
            report_client.reload_config()

        try:
            return report_client.upload_report_file(file_path)
        except Exception as e:
            logger.error(f"[_upload_report_file] 文件上传异常: {e}", exc_info=True)
            return None

    def _do_report_direct(self, report_data: Dict[str, Any]) -> bool:
        """
        直接执行HTTP上报

        Args:
            report_data: 上报数据

        Returns:
            是否成功
        """
        report_client = getattr(self, 'report_client', None)
        if report_client is None:
            logger.error("[_do_report_direct] ReportClient 未初始化")
            return False

        if hasattr(report_client, 'reload_config'):
            report_client.reload_config()

        try:
            return report_client.report_payload(report_data)
        except Exception as e:
            logger.error(f"[_do_report_direct] 上报异常: {e}")
            return False

    def _build_execution_result(self, task: Task, task_result: TaskResult) -> ExecutionResult:
        """
        构建 TDM2.0 格式的执行结果

        Args:
            task: 任务对象
            task_result: 任务结果对象

        Returns:
            ExecutionResult 实例
        """
        execution_result = ExecutionResult(taskNo=task.task_id)

        # 如果有测试结果，转换为 CaseResult
        if task_result.results:
            for test_result in task_result.results:
                case_no = self._get_case_no_from_result(test_result, task)

                # 根据结果状态确定 result 字段
                if test_result.verdict:
                    result_status = test_result.verdict
                elif test_result.passed is True:
                    result_status = "PASS"
                elif test_result.passed is False:
                    result_status = "FAIL"
                elif test_result.error:
                    result_status = "BLOCK"
                else:
                    result_status = "UNKNOWN"

                case_result = CaseResult(
                    caseNo=case_no,
                    result=result_status,
                    remark=test_result.error or f"测试类型: {test_result.type}",
                    created=datetime.now().strftime("%Y-%m-%d %H:%M:%S")
                )

                execution_result.add_case_result(case_result)

        # 如果没有结果但任务有测试项，生成 BLOCK 结果
        elif task.test_items:
            for test_item in task.test_items:
                case_no = getattr(test_item, 'caseNo', None) or getattr(test_item, 'case_no', None) or 'UNKNOWN'
                case_result = CaseResult(
                    caseNo=case_no,
                    result="BLOCK",
                    remark=task_result.errorMessage or "任务执行失败，用例未能执行",
                    created=datetime.now().strftime("%Y-%m-%d %H:%M:%S")
                )
                execution_result.add_case_result(case_result)

        # 生成摘要
        execution_result.generate_summary()

        return execution_result

    def _get_case_no_from_result(self, test_result: TestResult, task: Task) -> str:
        """
        从测试结果获取用例编号

        Args:
            test_result: 测试结果
            task: 任务对象

        Returns:
            用例编号
        """
        # 尝试从 details 中获取
        if test_result.details and isinstance(test_result.details, dict):
            case_no = test_result.details.get('case_no')
            if case_no:
                return case_no

        # 尝试从任务测试项中匹配
        if task.test_items:
            for test_item in task.test_items:
                item_name = getattr(test_item, 'name', None) or getattr(test_item, 'caseName', '')
                if test_result.name == item_name:
                    return getattr(test_item, 'caseNo', None) or getattr(test_item, 'case_no', None) or test_result.name

        # 使用测试结果名称作为 case_no
        return test_result.name or 'UNKNOWN'
    
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
            "start_time": datetime.fromtimestamp(self._start_time).isoformat() if self._start_time else None,
            "duration": time.time() - self._start_time if self._start_time else 0,
            "queue_size": self._task_queue.get_queue_size(),
            "processing_count": self._task_queue.get_processing_count(),
            "metrics": self._current_metrics.get_summary() if self._current_metrics else None
        }

    def get_running_count(self) -> int:
        """
        获取正在运行的任务数量

        Returns:
            正在运行的任务数量（0 或 1，因为是串行执行）
        """
        with self._lock:
            if self.current_task and self._running:
                return 1
            return 1 if self._task_queue.get_processing_count() > 0 else 0

    def get_stats(self) -> Dict[str, Any]:
        """
        获取执行器统计信息

        Returns:
            统计信息字典
        """
        return {
            "running": self.get_running_count(),
            "queue_size": self._task_queue.get_queue_size(),
            "max_workers": self.max_workers,
            "is_running": self._running
        }

# 保持向后兼容
TaskExecutor = TaskExecutorProduction

# 全局执行器实例
task_executor = None

def get_task_executor():
    """获取全局任务执行器实例"""
    global task_executor
    if task_executor is None:
        # 使用空的 message_sender 作为默认值
        task_executor = TaskExecutorProduction(message_sender=lambda msg: None)
        task_executor.start()
    return task_executor
