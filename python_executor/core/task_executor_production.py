"""
任务执行核心引擎 - 生产环境增强版
集成熔断器、重试、性能监控、配置热更新等功能
"""
import os
import uuid
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
from models.task import Task, TaskStatus, TestToolType, TestResult, TaskResult
from models.result import CaseResult, ExecutionResult, ExecutionOutcome
from models.executor_task import task_queue as global_task_queue, TaskStatus as ExecutorTaskStatus
from core.result_collector import ResultCollector
from core.config_manager import TestConfigManager
from core.adapters import create_adapter, TestToolType
from core.case_mapping_manager import get_case_mapping_manager
from core.execution_plan import ExecutionPlan, PlannedCase
from core.execution_strategies.selector import ExecutionStrategySelector
from core.execution_observability import (
    ExecutionLifecycleStage,
    get_execution_observability_manager,
)
from core.status_monitor import get_status_monitor
from core.task_compiler import TaskCompiler, TaskCompileError
logger = get_logger("task_executor_production")


def _get_runtime_config_manager():
    """Return the active facade-backed config manager."""
    return get_runtime_config()


def _ensure_observability_context(task: ExecutionPlan):
    """Ensure direct executor/reporting calls still have an observability context."""
    observability_manager = get_execution_observability_manager()
    raw_refs = getattr(task, "raw_refs", None)
    observability_ref = raw_refs.get("observability", {}) if isinstance(raw_refs, dict) else {}
    attempt_id = observability_ref.get("attempt_id")
    trace_id = observability_ref.get("trace_id")
    error_category = observability_ref.get("error_category")
    try:
        observability_manager.get_snapshot(
            getattr(task, "task_id", None) or getattr(task, "task_no")
        )
    except KeyError:
        observability_manager.create_context(
            task_no=getattr(task, "task_id", None) or getattr(task, "task_no"),
            device_id=getattr(task, "deviceId", None)
            or getattr(task, "device_id", "")
            or getattr(task, "device_id", "")
            or "",
            tool_type=getattr(task, "toolType", None)
            or getattr(task, "tool_type", "")
            or getattr(task, "tool_type", "")
            or "",
            attempt=1,
            attempt_id=attempt_id,
            trace_id=trace_id,
            error_category=error_category,
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
    
    def put(self, task: ExecutionPlan) -> bool:
        """添加任务到队列"""
        try:
            self.queue.put(task, block=False)
            return True
        except queue.Full:
            logger.warning("任务队列已满")
            return False
    
    def get(self, timeout: float = 1.0) -> Optional[ExecutionPlan]:
        """从队列获取任务"""
        try:
            return self.queue.get(timeout=timeout)
        except queue.Empty:
            return None
    
    def mark_processing(self, task_id: str, task: ExecutionPlan):
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

    def remove(self, task_id: str) -> bool:
        """从内部执行队列移除尚未开始的任务。"""
        with self._lock:
            retained: queue.Queue[ExecutionPlan] = queue.Queue(maxsize=self.queue.maxsize)
            removed = False

            while not self.queue.empty():
                task = self.queue.get_nowait()
                current_task_id = getattr(task, "task_id", None) or getattr(task, "task_no", None)
                if not removed and current_task_id == task_id:
                    removed = True
                    continue
                retained.put_nowait(task)

            self.queue = retained
            return removed

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
        self.current_task: Optional[ExecutionPlan] = None
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
        self._current_trace_id: str | None = None
        self._current_attempt_id: str | None = None
        self._current_error_category: str | None = None
        self._current_execution_error_category: str | None = None

        # 配置管理器
        self.config_manager: Optional[TestConfigManager] = None

        # 上报客户端
        self.report_client = ReportClient()
        self._strategy_selector = ExecutionStrategySelector()

        self.logger.info("任务执行器（生产环境版）初始化完成")

    def _task_id(self, task: ExecutionPlan) -> str:
        return getattr(task, "task_id", None) or getattr(task, "task_no")

    def _task_name(self, task: ExecutionPlan) -> str:
        return (
            getattr(task, "taskName", None)
            or getattr(task, "task_name", None)
            or getattr(task, "name", "")
            or ""
        )

    def _task_project_no(self, task: ExecutionPlan) -> str:
        return getattr(task, "projectNo", None) or getattr(task, "project_no", "") or ""

    def _task_device_id(self, task: ExecutionPlan) -> str:
        return getattr(task, "deviceId", None) or getattr(task, "device_id", "") or ""

    def _task_tool_type(self, task: ExecutionPlan) -> str:
        return getattr(task, "toolType", None) or getattr(task, "tool_type", "") or ""

    def _task_observability_store(self, task: ExecutionPlan) -> Dict[str, Any]:
        raw_refs = getattr(task, "raw_refs", None)
        if isinstance(raw_refs, dict):
            return raw_refs.setdefault("observability", {})
        return {}

    def _set_task_observability_value(self, task: ExecutionPlan, key: str, value: Any) -> None:
        if value is None:
            return
        store = self._task_observability_store(task)
        store[key] = value

    def _task_observability_context(self, task: ExecutionPlan) -> Dict[str, Any]:
        store = self._task_observability_store(task)
        trace_id = store.get("trace_id") or self._current_trace_id
        attempt_id = store.get("attempt_id") or self._current_attempt_id
        error_category = store.get("error_category") or self._current_error_category
        execution_error_category = store.get("execution_error_category") or self._current_execution_error_category

        return {
            "trace_id": trace_id,
            "attempt_id": attempt_id,
            "error_category": error_category,
            "execution_error_category": execution_error_category,
        }

    def _ensure_task_observability_context(self, task: ExecutionPlan) -> Dict[str, Any]:
        context = self._task_observability_context(task)

        if not context["trace_id"]:
            context["trace_id"] = str(uuid.uuid4())
        if not context["attempt_id"]:
            context["attempt_id"] = str(uuid.uuid4())

        self._set_task_observability_value(task, "trace_id", context["trace_id"])
        self._set_task_observability_value(task, "attempt_id", context["attempt_id"])
        if context["error_category"] is not None:
            self._set_task_observability_value(task, "error_category", context["error_category"])
        if context["execution_error_category"] is not None:
            self._set_task_observability_value(task, "execution_error_category", context["execution_error_category"])

        self._current_trace_id = context["trace_id"]
        self._current_attempt_id = context["attempt_id"]
        if context["error_category"] is not None:
            self._current_error_category = context["error_category"]
        if context["execution_error_category"] is not None:
            self._current_execution_error_category = context["execution_error_category"]

        return context

    def _infer_execution_error_category(self, task_result: TaskResult | ExecutionOutcome) -> str | None:
        outcome = self._normalize_execution_outcome(task_result)
        if outcome.status in {"failed", "FAILED"}:
            return "execution_failure"
        if outcome.status in {"timeout", "TIMEOUT"}:
            return "timeout"
        if outcome.errorMessage:
            return "execution_failure"
        return None

    def _task_config_path(self, task: ExecutionPlan) -> str | None:
        return getattr(task, "configPath", None) or getattr(task, "config_path", None)

    def _task_config_name(self, task: ExecutionPlan) -> str | None:
        return getattr(task, "configName", None) or getattr(task, "config_name", None)

    def _task_base_config_dir(self, task: ExecutionPlan) -> str | None:
        return getattr(task, "baseConfigDir", None) or getattr(task, "base_config_dir", None)

    def _task_variables(self, task: ExecutionPlan) -> Dict[str, Any]:
        return getattr(task, "variables", {}) or {}

    def _task_canoe_namespace(self, task: ExecutionPlan) -> str | None:
        return getattr(task, "canoeNamespace", None) or getattr(task, "canoe_namespace", None)

    def _task_timeout(self, task: ExecutionPlan) -> int:
        return getattr(task, "timeout", None) or getattr(task, "timeout_seconds", 3600)

    def _task_cases(self, task: ExecutionPlan) -> list:
        return getattr(task, "test_items", None) or getattr(task, "cases", [])

    def _ensure_execution_plan(self, task: Task | ExecutionPlan) -> ExecutionPlan:
        if isinstance(task, ExecutionPlan):
            return task
        return ExecutionPlan.from_legacy_task(task)

    def _case_name(self, case: Any) -> str:
        return getattr(case, "name", None) or getattr(case, "caseName", None) or getattr(case, "case_name", "") or ""

    def _case_type(self, case: Any) -> str:
        return getattr(case, "type", None) or getattr(case, "caseType", None) or getattr(case, "case_type", "") or ""

    def _case_no(self, case: Any) -> str:
        return getattr(case, "caseNo", None) or getattr(case, "case_no", None) or ""

    def _case_repeat(self, case: Any) -> int:
        return getattr(case, "repeat", 1) or 1

    def _case_dtc_info(self, case: Any) -> str | None:
        return getattr(case, "dtcInfo", None) or getattr(case, "dtc_info", None)

    def _case_params(self, case: Any) -> Dict[str, Any]:
        return getattr(case, "params", None) or getattr(case, "execution_params", {}) or {}

    def _legacy_test_item_dict(self, case: Any) -> Dict[str, Any]:
        return {
            "name": self._case_name(case),
            "type": self._case_type(case),
            "case_no": self._case_no(case),
            "caseNo": self._case_no(case),
            "caseName": self._case_name(case),
            "caseType": self._case_type(case),
            "repeat": self._case_repeat(case),
            "dtcInfo": self._case_dtc_info(case),
            "params": self._case_params(case),
        }

    def _legacy_config_case_dict(self, case: Any) -> Dict[str, Any]:
        return {
            "caseNo": self._case_no(case),
            "caseName": self._case_name(case),
            "caseType": self._case_type(case),
            "repeat": self._case_repeat(case),
            "dtcInfo": self._case_dtc_info(case),
            "params": self._case_params(case),
        }

    def _controller_last_error(self) -> str:
        controller = self.controller
        if controller is None:
            return "未知错误"
        if hasattr(controller, "last_error") and getattr(controller, "last_error"):
            return getattr(controller, "last_error")
        if hasattr(controller, "error_message") and getattr(controller, "error_message"):
            return getattr(controller, "error_message")
        return "未知错误"

    def _controller_execute_test_module(self, module_name: str, timeout: int | None = None) -> Dict[str, Any]:
        test_module_capability = None
        if hasattr(self.controller, "get_capability"):
            test_module_capability = self.controller.get_capability("test_module")
        if test_module_capability is not None:
            return test_module_capability.execute_module(module_name, timeout=timeout)
        if hasattr(self.controller, "execute_test_module_direct"):
            return self.controller.execute_test_module_direct(module_name, timeout)
        if hasattr(self.controller, "run_test_module"):
            return self.controller.run_test_module(module_name)
        raise ToolException("当前适配器不支持TestModule执行")

    def _normalize_execution_outcome(self, task_result: TaskResult | ExecutionOutcome) -> ExecutionOutcome:
        if isinstance(task_result, ExecutionOutcome):
            return task_result
        return ExecutionOutcome(
            task_no=task_result.taskNo,
            status=task_result.status,
            started_at=task_result.startTime,
            finished_at=task_result.endTime,
            case_results=list(task_result.results or []),
            summary=dict(task_result.summary or {}),
            error_summary=task_result.errorMessage,
        )
    
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
                        future.result(timeout=self._task_timeout(task))
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
        plan = self._ensure_execution_plan(task)
        task_id = self._task_id(plan)
        if task_id is None:
            logger.error("execute_task 失败: 任务缺少 task_id 属性")
            return False

        # 验证任务数据
        try:
            test_items = [self._legacy_test_item_dict(item) for item in self._task_cases(plan)]

            # 显式检查 test_items 是否为空
            if not test_items:
                logger.error(f"execute_task 失败: 任务 {task_id} 的 test_items 为空")
                return False

            task_data = {
                'taskNo': task_id,
                'deviceId': self._task_device_id(plan),
                'toolType': self._task_tool_type(plan),
                'configPath': self._task_config_path(plan),
                'testItems': test_items,
                'timeout': self._task_timeout(plan)
            }
            InputValidator.validate_task_data(task_data)
        except ValidationError as e:
            logger.error(f"任务数据验证失败: {e}")
            return False
        except Exception as e:
            logger.error(f"execute_task 异常: {e}")
            return False

        try:
            strategy = self._strategy_selector.select(plan)
            adapter = create_adapter(self._task_tool_type(plan), singleton=False)
            ok, error = strategy.prepare(plan, adapter)
            if not ok:
                logger.error(f"策略预检查失败: {error}")
                return False
        except Exception as e:
            logger.error(f"策略选择/预检查失败: {e}")
            return False

        # 添加到内部执行队列
        if self._task_queue.put(plan):
            # 同时添加到全局队列供API查询（将TDMTask转换为内部Task格式）
            from models.executor_task import Task as ExecutorTask, TaskStatus as ExecutorTaskStatus
            exec_task = ExecutorTask(
                id=task_id,
                name=self._task_name(plan),
                task_type='test_module',
                priority=1,
                status=ExecutorTaskStatus.PENDING.value,
                params={
                    'tool_type': self._task_tool_type(plan),
                    'config_path': self._task_config_path(plan),
                    'config_name': self._task_config_name(plan),
                    'base_config_dir': self._task_base_config_dir(plan),
                    'variables': self._task_variables(plan),
                    'canoe_namespace': self._task_canoe_namespace(plan),
                    'testItems': test_items,
                },
                timeout=self._task_timeout(plan),
                metadata={
                    'taskNo': task_id,
                    'projectNo': self._task_project_no(plan),
                    'deviceId': self._task_device_id(plan),
                    'caseCount': len(self._task_cases(plan) or []),
                    'caseList': [self._legacy_config_case_dict(item) for item in self._task_cases(plan)],
                }
            )
            global_task_queue.add(exec_task)
            logger.info(f"任务已加入队列: {task_id}")
            return True
        else:
            logger.error(f"任务加入队列失败: {task_id}")
            return False

    def execute_plan(self, plan: ExecutionPlan) -> bool:
        return self.execute_task(plan)

    def _run_strategy_execution(self, task: ExecutionPlan, adapter: BaseException | Any, config_path: str | None):
        task_id = self._task_id(task)
        tool_type = self._task_tool_type(task)
        observability_manager = _ensure_observability_context(task)
        observability_manager.transition(task_id, ExecutionLifecycleStage.EXECUTING)
        record_metric(
            'task.executing',
            1,
            {'task_no': task_id, 'tool_type': tool_type or ''},
        )

        strategy = self._strategy_selector.select(task)
        return strategy.run(task, adapter=adapter, executor=self, config_path=config_path)

    def _build_execution_plan_from_queue_task(self, task) -> ExecutionPlan:
        params = getattr(task, 'params', {}) if hasattr(task, 'params') else {}
        metadata = getattr(task, 'metadata', {}) if hasattr(task, 'metadata') else {}
        compiler = TaskCompiler(get_case_mapping_manager())

        test_items = params.get('testItems')
        if not test_items:
            case_list = params.get('caseList') or metadata.get('caseList') or []
            test_items = []
            for case in case_list:
                if not isinstance(case, dict):
                    continue
                test_items.append(
                    {
                        'caseNo': case.get('caseNo') or case.get('case_no') or "",
                        'name': case.get('caseName') or case.get('name') or "",
                        'type': case.get('caseType') or case.get('type') or 'test_module',
                        'dtcInfo': case.get('dtcInfo') or case.get('dtc_info'),
                        'params': case.get('params') or {},
                        'repeat': case.get('repeat', 1),
                    }
                )

        if not test_items:
            raise TaskCompileError("submit_task 缺少 testItems/caseList，无法生成执行计划")

        payload = {
            'taskNo': getattr(task, 'id', None) or getattr(task, 'task_id', None) or "",
            'projectNo': metadata.get('projectNo', ''),
            'taskName': getattr(task, 'name', '') or getattr(task, 'taskName', ''),
            'deviceId': metadata.get('deviceId', ''),
            'toolType': params.get('tool_type') or getattr(task, 'task_type', None),
            'configPath': params.get('config_path'),
            'configName': params.get('config_name'),
            'baseConfigDir': params.get('base_config_dir'),
            'variables': params.get('variables', {}),
            'canoeNamespace': params.get('canoe_namespace'),
            'timeout': getattr(task, 'timeout', 3600),
            'testItems': test_items,
        }
        return compiler.compile_payload(payload)
    
    @TASK_CIRCUIT_BREAKER
    def _execute_task_production(self, task: ExecutionPlan):
        """生产环境任务执行"""
        task_id = self._task_id(task)
        tool_type = self._task_tool_type(task)
        logger.info(
            f"[_execute_task_production] 开始执行任务: task_id={task_id}, task_type=test_module, tool_type={tool_type}"
        )

        with self._lock:
            if self.current_task is not None:
                logger.warning("已有任务正在执行")
                return

            self.current_task = task
            self._stop_event.clear()
            self._start_time = time.time()
            self._current_trace_id = None
            self._current_attempt_id = None
            self._current_error_category = None
            self._current_execution_error_category = None

        # 初始化性能指标
        self._current_metrics = TaskMetrics(task_id)
        self._task_queue.mark_processing(task_id, task)
        self._ensure_task_observability_context(task)
        observability_manager = _ensure_observability_context(task)

        try:
            logger.info(f"开始执行任务: {task_id}")
            self._current_metrics.record_step('start', 0)
            observability_manager.transition(task_id, ExecutionLifecycleStage.PREPARING)
            record_metric(
                'task.preparing',
                1,
                {'task_no': task_id, 'tool_type': tool_type or ''},
            )

            # 初始化结果收集器
            self.current_collector = ResultCollector(task_id)

            # 更新状态为运行中
            self._update_task_status(task_id, TaskStatus.RUNNING)

            logger.info(f"[_execute_task_production] 任务初始化完成，开始配置准备阶段")

            # ========== 配置准备 ==========
            cfg_path = None

            # 1. 优先使用任务指定的配置路径
            if self._task_config_path(task):
                cfg_path = self._task_config_path(task)
                logger.info(f"使用任务指定的配置路径: {cfg_path}")

            # 2. 尝试从配置管理器准备配置
            elif self._task_config_name(task) or self._task_base_config_dir(task):
                self.current_collector.add_log("INFO", "正在准备测试配置...")

                base_dir = self._task_base_config_dir(task) or _get_runtime_config_manager().get(
                    'config_base_dir',
                    r'D:\TAMS\DTTC_CONFIG',
                )
                logger.info(f"[_execute_task_production] base_dir={base_dir}, config_name={self._task_config_name(task)}")
                self.config_manager = TestConfigManager(base_config_dir=base_dir)

                # 确定配置名称
                config_name = self._task_config_name(task)
                if not config_name and self._task_config_path(task):
                    config_name = os.path.basename(self._task_config_path(task)).replace('.cfg', '')

                if config_name:
                    config_info = self.config_manager.prepare_config_for_task(
                        task_config_name=config_name,
                        test_cases=[self._legacy_config_case_dict(item) for item in self._task_cases(task)],
                        variables=self._task_variables(task)
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
            if not cfg_path and self._task_cases(task):
                mapping_manager = get_case_mapping_manager()
                for test_item in self._task_cases(task):
                    case_no = self._case_no(test_item)
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
                tool_type_lower = tool_type.lower() if tool_type else ""
                if tool_type_lower == TestToolType.CANOE.value:
                    adapter_type = TestToolType.CANOE
                elif tool_type_lower == TestToolType.TSMASTER.value:
                    adapter_type = TestToolType.TSMASTER
                elif tool_type_lower == TestToolType.TTWORKBENCH.value:
                    adapter_type = TestToolType.TTWORKBENCH
                else:
                    raise TaskException(f"不支持的测试工具类型: {tool_type}")
                
                # 使用适配器工厂创建原生适配器，由 strategy/capability 驱动主路径
                raw_adapter = create_adapter(adapter_type, singleton=False)
                self.controller = raw_adapter
                logger.info(f"已创建适配器: {adapter_type.value}")
            except Exception as e:
                raise TaskException(f"创建适配器失败: {e}")

            self._current_metrics.record_step('controller_init', time.time() - step_start)

            # 连接测试软件（带重试）
            step_start = time.time()
            self._connect_tool_with_retry(task)
            self._current_metrics.record_step('connect_tool', time.time() - step_start)

            step_start = time.time()
            current_cfg_path = cfg_path or ""
            results = self._run_strategy_execution(task, adapter=self.controller, config_path=current_cfg_path)
            self._current_metrics.record_step('strategy_execute', time.time() - step_start)
            
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
                    'task_id': task_id,
                    'step': step_name
                })
            
            record_metric('task.total_duration', metrics_summary['duration'], {
                'task_id': task_id
            })
            
            record_metric('task.error_count', metrics_summary['error_count'], {
                'task_id': task_id
            })
            
            # 标记任务完成
            self._task_queue.mark_completed(task_id, {
                'status': 'completed' if metrics_summary['error_count'] == 0 else 'failed',
                'metrics': metrics_summary
            })
            
            with self._lock:
                self.current_task = None
                self.current_collector = None
                self._current_metrics = None
                self.config_manager = None
                self._start_time = None
    
    def _connect_tool_with_retry(self, task: Task | ExecutionPlan):
        """带重试的连接工具"""
        runtime_config = _get_runtime_config_manager()
        max_retries = runtime_config.get('canoe.max_retries', 3)
        retry_delay = runtime_config.get('canoe.retry_delay', 2.0)
        tool_type = self._task_tool_type(task)
        
        for attempt in range(max_retries):
            try:
                logger.info(f"正在连接{tool_type}... (尝试 {attempt + 1}/{max_retries})")
                
                if self.controller.connect():
                    logger.info(f"{tool_type}连接成功")
                    return
                    
            except Exception as e:
                if attempt < max_retries - 1:
                    logger.warning(f"连接失败，{retry_delay}秒后重试...")
                    self._current_metrics.record_retry()
                    time.sleep(retry_delay)
                else:
                    raise ToolException(f"连接{tool_type}失败，已重试{max_retries}次: {e}")

        # 如果所有重试都返回False而没有异常，说明连接失败但未抛出异常
        raise ToolException(f"连接{tool_type}失败: 适配器返回失败状态")

    def _complete_task(self, task: Task, results: list | ExecutionOutcome):
        """完成任务"""
        task_id = self._task_id(task)
        tool_type = self._task_tool_type(task)
        duration = time.time() - self._start_time
        observability_manager = _ensure_observability_context(task)
        observability_manager.transition(task_id, ExecutionLifecycleStage.COLLECTING)
        observability_manager.transition(task_id, ExecutionLifecycleStage.REPORTING)
        record_metric(
            'task.reporting',
            1,
            {'task_no': task_id, 'tool_type': tool_type or ''},
        )

        logger.info(f"任务执行完成: {task_id}, 耗时: {duration:.1f}秒")

        if isinstance(results, ExecutionOutcome):
            task_result = results
        else:
            task_result = self.current_collector.finalize(TaskStatus.COMPLETED.value)

        # 上报最终结果到WebSocket客户端
        self._report_final_result(task_id, task_result)

        # 上报到远端服务器（包含报告文件上传）- 异步执行避免阻塞
        report_file_path = getattr(self, '_current_report_info', None).get('report_path') if hasattr(self, '_current_report_info') and self._current_report_info else None
        self._executor.submit(self._report_to_remote, task, task_result, report_file_path)

        final_status = getattr(task_result, "status", TaskStatus.COMPLETED.value)
        if final_status == TaskStatus.COMPLETED.value:
            self._update_task_status(task_id, TaskStatus.COMPLETED, f"任务执行完成，耗时{duration:.1f}秒")
            return

        error_message = getattr(task_result, "errorMessage", None) or "任务执行失败"
        execution_error_category = self._infer_execution_error_category(task_result) or "execution_failure"
        self._current_execution_error_category = execution_error_category
        self._current_error_category = execution_error_category
        self._set_task_observability_value(task, "execution_error_category", execution_error_category)
        self._set_task_observability_value(task, "error_category", execution_error_category)
        observability_manager.fail(
            task_id,
            error_code="TASK_TIMEOUT" if final_status == "timeout" else "TASK_FAILED",
            error_message=error_message,
            error_category=execution_error_category,
            retryable=False,
        )
        record_metric(
            'task.failed',
            1,
            {'task_no': task_id, 'stage': observability_manager.get_snapshot(task_id).get('failed_stage') or ''},
        )
        self._update_task_status(task_id, TaskStatus.FAILED, error_message)
    
    def _fail_task(self, task: Task, error_message: str):
        """任务失败"""
        task_id = self._task_id(task)
        logger.error(f"任务失败: {task_id}, 错误: {error_message}")
        observability_manager = get_execution_observability_manager()
        execution_error_category = "execution_failure"
        self._current_execution_error_category = execution_error_category
        self._current_error_category = execution_error_category
        self._set_task_observability_value(task, "execution_error_category", execution_error_category)
        self._set_task_observability_value(task, "error_category", execution_error_category)
        observability_manager.fail(
            task_id,
            error_code="TASK_FAILED",
            error_message=error_message,
            error_category=execution_error_category,
            retryable=False,
        )
        record_metric(
            'task.failed',
            1,
            {'task_no': task_id, 'stage': observability_manager.get_snapshot(task_id).get('failed_stage') or ''},
        )

        # 为所有未执行的测试项生成失败结果
        task_cases = self._task_cases(task)
        if task_cases:
            logger.info(f"[_fail_task] 为 {len(task_cases)} 个测试项生成失败结果")
            for test_item in task_cases:
                item_name = self._case_name(test_item) or '未命名用例'
                item_type = self._case_type(test_item) or 'test_module'
                case_no = self._case_no(test_item)

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
            self._report_final_result(task_id, task_result)

            # 上报到远端服务器
            logger.info(f"[_fail_task] 开始调用远端上报")
            self._report_to_remote(task, task_result)
        else:
            logger.warning(f"[_fail_task] current_collector 为 None，无法上报结果")

        # 更新状态
        self._update_task_status(task_id, TaskStatus.FAILED, error_message)
    
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
        if task_id and self._task_queue.remove(task_id):
            logger.info(f"已取消排队中的任务: {task_id}")
            self._update_task_status(task_id, TaskStatus.CANCELLED, "任务被用户取消")
            return True

        if self.current_task is None:
            logger.warning("没有正在执行的任务")
            return False

        # 如果指定了task_id，检查是否匹配
        current_task_id = self._task_id(self.current_task)
        if task_id and current_task_id != task_id:
            if self._task_queue.remove(task_id):
                logger.info(f"已取消排队中的任务: {task_id}")
                self._update_task_status(task_id, TaskStatus.CANCELLED, "任务被用户取消")
                return True
            logger.warning(f"任务ID不匹配: 当前执行的是 {current_task_id}，要取消的是 {task_id}")
            return False

        logger.info(f"正在取消任务: {current_task_id}")
        self._stop_event.set()

        # 更新任务状态为已取消
        self._update_task_status(current_task_id, TaskStatus.CANCELLED, "任务被用户取消")

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

            execution_plan = self._build_execution_plan_from_queue_task(task)

            # 添加到全局任务队列
            if not global_task_queue.add(task):
                logger.warning(f"任务 {task_id} 已存在于队列中")
                return False

            # 添加到内部执行队列
            if not self._task_queue.put(execution_plan):
                logger.error(f"任务 {task_id} 加入内部执行队列失败")
                try:
                    global_task_queue.remove(task_id)
                except Exception as rollback_error:
                    logger.warning(f"回滚全局任务队列失败: {rollback_error}")
                return False

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

            copied_params = dict(old_task.params or {})
            copied_metadata = dict(old_task.metadata or {})

            new_task = ExecutorTask(
                name=old_task.name,
                task_type=old_task.task_type,
                priority=old_task.priority,
                params=copied_params,
                timeout=old_task.timeout,
                max_retries=old_task.max_retries,
                created_by=old_task.created_by,
                metadata=copied_metadata
            )
            new_task.metadata["taskNo"] = new_task.id

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
                    'tool_type': self._task_tool_type(self.current_task) if self.current_task else None,
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
        outcome = self._normalize_execution_outcome(task_result)
        if self.message_sender:
            self.message_sender({
                "type": "RESULT_REPORT",
                "taskNo": task_no,
                "status": outcome.status,
                "results": [r.to_dict() for r in outcome.results],
                "summary": outcome.summary,
                "timestamp": int(time.time() * 1000)
            })

    def _report_to_remote(self, task: Task, task_result: TaskResult | ExecutionOutcome, report_file_path: str = None):
        """
        上报任务结果到远端服务器

        Args:
            task: 任务对象
            task_result: 任务结果对象
            report_file_path: 报告文件路径（可选）
        """
        task_id = self._task_id(task)
        logger.info(f"[_report_to_remote] 开始上报任务结果: taskNo={task_id}, report_file_path={report_file_path}")

        try:
            outcome = self._normalize_execution_outcome(task_result)
            observability_context = self._ensure_task_observability_context(task)
            execution_error_category = self._infer_execution_error_category(outcome)
            if execution_error_category and not observability_context.get("error_category"):
                observability_context["error_category"] = execution_error_category
                self._current_error_category = execution_error_category
                self._set_task_observability_value(task, "error_category", execution_error_category)

            # 构建 TDM2.0 格式的上报数据
            execution_result = self._build_execution_result(task, outcome)
            logger.info(f"[_report_to_remote] 构建执行结果完成: caseList数量={len(execution_result.caseList)}")

            # 构建上报数据
            report_data = execution_result.to_tdm2_format()

            # 添加额外信息
            report_data["projectNo"] = self._task_project_no(task)
            report_data["deviceId"] = self._task_device_id(task)
            report_data["taskName"] = self._task_name(task) or task_id
            report_data["toolType"] = self._task_tool_type(task)
            report_data["status"] = outcome.status
            report_data["summary"] = outcome.summary
            report_data["errorMessage"] = outcome.errorMessage
            report_data["timestamp"] = int(time.time() * 1000)
            report_data["trace_id"] = observability_context["trace_id"]
            report_data["traceId"] = observability_context["trace_id"]
            report_data["attempt_id"] = observability_context["attempt_id"]
            report_data["attemptId"] = observability_context["attempt_id"]
            report_data["error_category"] = observability_context.get("error_category")
            report_data["errorCategory"] = observability_context.get("error_category")
            report_data["reportMetadata"] = {
                "trace_id": observability_context["trace_id"],
                "traceId": observability_context["trace_id"],
                "attempt_id": observability_context["attempt_id"],
                "attemptId": observability_context["attempt_id"],
                "error_category": observability_context.get("error_category"),
                "errorCategory": observability_context.get("error_category"),
                "execution_error_category": observability_context.get("execution_error_category"),
                "taskNo": task_id,
            }

            if outcome.startTime:
                report_data["startTime"] = outcome.startTime.isoformat()
            if outcome.endTime:
                report_data["endTime"] = outcome.endTime.isoformat()

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
                logger.info(f"[_report_to_remote] 任务结果上报成功: {task_id}")
                record_metric('task.report.success', 1, {'task_no': task_id})
            else:
                logger.warning(f"[_report_to_remote] 任务结果上报失败: {task_id}")
                record_metric('task.report.failure', 1, {'task_no': task_id})
                # 上报失败时持久化以便后续重试
                self._current_error_category = "report_failure"
                report_data["report_error_category"] = "report_failure"
                report_data["reportErrorCategory"] = "report_failure"
                self._persist_failed_report(report_data, task)

            _ensure_observability_context(task).finish(
                task_id,
                report_status='success' if success else 'failed',
            )
            record_metric(
                'task.finished',
                1,
                {'task_no': task_id, 'status': 'success' if success else 'failed'},
            )

        except Exception as e:
            logger.error(f"[_report_to_remote] 远端上报任务结果时出错: {e}", exc_info=True)
            record_metric('task.report.failure', 1, {'task_no': task_id})
            self._current_error_category = "report_failure"
            if 'report_data' in locals():
                report_data["report_error_category"] = "report_failure"
                report_data["reportErrorCategory"] = "report_failure"
            _ensure_observability_context(task).finish(
                task_id,
                report_status='failed',
            )
            record_metric(
                'task.finished',
                1,
                {'task_no': task_id, 'status': 'failed'},
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
            task_id = self._task_id(task)
            from core.failed_report_manager import get_failed_report_manager

            manager = get_failed_report_manager(self.config_manager)

            task_info = {
                'taskNo': task_id,
                'projectNo': self._task_project_no(task),
                'deviceId': self._task_device_id(task),
                'toolType': self._task_tool_type(task),
                'taskName': self._task_name(task),
                'trace_id': self._current_trace_id or report_data.get('trace_id'),
                'traceId': self._current_trace_id or report_data.get('traceId'),
                'attempt_id': self._current_attempt_id or report_data.get('attempt_id'),
                'attemptId': self._current_attempt_id or report_data.get('attemptId'),
                'error_category': self._current_error_category or report_data.get('error_category'),
                'errorCategory': self._current_error_category or report_data.get('errorCategory'),
                'execution_error_category': self._current_execution_error_category or report_data.get('error_category'),
                'executionErrorCategory': self._current_execution_error_category or report_data.get('errorCategory'),
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

            logger.info(f"失败报告已持久化: report_id={report_id}, task_no={task_id}")

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

    def _build_execution_result(self, task: Task, task_result: TaskResult | ExecutionOutcome) -> ExecutionResult:
        """
        构建 TDM2.0 格式的执行结果

        Args:
            task: 任务对象
            task_result: 任务结果对象

        Returns:
            ExecutionResult 实例
        """
        execution_outcome = self._normalize_execution_outcome(task_result)
        execution_result = ExecutionResult(taskNo=self._task_id(task))

        # 如果有测试结果，转换为 CaseResult
        if execution_outcome.results:
            for test_result in execution_outcome.results:
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
        elif self._task_cases(task):
            for test_item in self._task_cases(task):
                case_no = self._case_no(test_item) or 'UNKNOWN'
                case_result = CaseResult(
                    caseNo=case_no,
                    result="BLOCK",
                    remark=execution_outcome.errorMessage or "任务执行失败，用例未能执行",
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
        if self._task_cases(task):
            for test_item in self._task_cases(task):
                item_name = self._case_name(test_item)
                if test_result.name == item_name:
                    return self._case_no(test_item) or test_result.name

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
            "task_id": self._task_id(self.current_task),
            "tool_type": self._task_tool_type(self.current_task),
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
