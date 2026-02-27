"""
任务执行引擎 V2 - TDM2.0字段标准

基于适配器模式的任务执行引擎，支持CANoe、TSMaster、TTworkbench等多种测试工具
使用TDM2.0字段标准进行数据交换
"""

import threading
import time
import logging
from typing import Dict, Any, Optional, Callable
from enum import Enum, auto
from datetime import datetime

from .adapters import create_adapter, BaseTestAdapter, TestToolType, AdapterStatus
from ..models import Case, Task, CaseResult, ExecutionResult, CaseResultStatus


class TaskStatus(Enum):
    """任务状态枚举"""
    PENDING = auto()        # 待执行
    RUNNING = auto()        # 执行中
    COMPLETED = auto()      # 已完成
    FAILED = auto()         # 失败
    CANCELLED = auto()      # 已取消
    TIMEOUT = auto()        # 超时


class TaskExecutorV2:
    """
    任务执行引擎 V2
    
    基于适配器模式，支持多种测试工具的统一任务执行
    使用TDM2.0字段标准进行数据交换
    
    特性：
    - 支持CANoe、TSMaster、TTworkbench等多种测试工具
    - 统一的任务执行流程
    - 实时状态上报
    - 任务取消支持
    - 超时控制
    - TDM2.0字段标准兼容
    """
    
    def __init__(self, message_sender: Callable[[str, Dict], None]):
        """
        初始化任务执行引擎
        
        Args:
            message_sender: 消息发送回调函数，用于向Java端上报消息
                           参数: (message_type, payload)
        """
        self.message_sender = message_sender
        self.logger = logging.getLogger(self.__class__.__name__)
        
        # 任务状态
        self._current_task: Optional[Task] = None
        self._current_adapter: Optional[BaseTestAdapter] = None
        self._task_status = TaskStatus.PENDING
        
        # 执行结果
        self._execution_result: Optional[ExecutionResult] = None
        
        # 线程控制
        self._stop_event = threading.Event()
        self._task_thread: Optional[threading.Thread] = None
        self._lock = threading.RLock()
        
        # 执行统计
        self._start_time: Optional[float] = None
        
        self.logger.info("任务执行引擎 V2 初始化完成 (TDM2.0字段标准)")
    
    @property
    def is_busy(self) -> bool:
        """检查是否正在执行任务"""
        with self._lock:
            return self._current_task is not None
    
    @property
    def current_task_no(self) -> Optional[str]:
        """获取当前任务编号 (TDM2.0字段)"""
        with self._lock:
            return self._current_task.taskNo if self._current_task else None
    
    def execute_task(self, task_data: Dict[str, Any]) -> bool:
        """
        执行测试任务
        
        Args:
            task_data: 任务配置字典 (TDM2.0格式)
                - projectNo: 项目编号
                - taskNo: 任务编号
                - taskName: 任务名称
                - caseList: 用例集合
                - toolType: 测试工具类型 (canoe/tsmaster/ttworkbench)
                - configPath: 配置文件路径
                - adapterConfig: 适配器配置
                - timeout: 任务超时时间（秒）
                
        Returns:
            任务启动成功返回True，否则返回False
        """
        with self._lock:
            if self._current_task is not None:
                self.logger.warning(f"已有任务正在执行: {self._current_task.taskNo}")
                return False
            
            # 使用TDM2.0格式创建任务
            self._current_task = Task.from_dict(task_data)
            self._task_status = TaskStatus.PENDING
            self._execution_result = ExecutionResult(
                taskNo=self._current_task.taskNo,
                platform="NETWORK"
            )
            self._stop_event.clear()
        
        # 启动任务执行线程
        self._task_thread = threading.Thread(
            target=self._execute_task_thread,
            args=(self._current_task,),
            name=f"TaskExecutor-{self._current_task.taskNo}"
        )
        self._task_thread.start()
        
        self.logger.info(f"任务已启动: {self._current_task.taskNo}")
        return True
    
    def cancel_task(self, task_no: str) -> bool:
        """
        取消正在执行的任务
        
        Args:
            task_no: 任务编号 (TDM2.0字段)
            
        Returns:
            取消成功返回True，否则返回False
        """
        with self._lock:
            if self._current_task is None:
                self.logger.warning("没有正在执行的任务")
                return False
            
            current_task_no = self._current_task.taskNo
            if current_task_no != task_no:
                self.logger.warning(f"任务编号不匹配: 当前={current_task_no}, 请求={task_no}")
                return False
        
        self.logger.info(f"正在取消任务: {task_no}")
        self._stop_event.set()
        
        # 停止适配器
        if self._current_adapter:
            self._current_adapter.stop_test()
        
        return True
    
    def get_task_status(self) -> Dict[str, Any]:
        """
        获取当前任务状态
        
        Returns:
            任务状态字典 (TDM2.0格式)
        """
        with self._lock:
            status = {
                "taskNo": self.current_task_no,
                "status": self._task_status.name,
                "isBusy": self.is_busy
            }
            
            if self._execution_result:
                status["executionResult"] = self._execution_result.to_dict()
            
            if self._start_time:
                status["elapsedTime"] = time.time() - self._start_time
            
            if self._current_adapter:
                status["adapterStatus"] = self._current_adapter.get_status()
            
            return status
    
    def _execute_task_thread(self, task: Task):
        """任务执行线程"""
        task_no = task.taskNo
        tool_type_str = task.toolType or ""
        
        try:
            self._start_time = time.time()
            self._update_task_status(TaskStatus.RUNNING)
            self._send_status_update(task_no, "RUNNING")
            
            # 记录开始时间
            if self._execution_result:
                self._execution_result.startTime = datetime.now()
            
            # 1. 创建适配器
            tool_type = TestToolType(tool_type_str.lower())
            adapter_config = {}  # 可以从task中获取适配器配置
            self._current_adapter = create_adapter(tool_type, adapter_config)
            
            self.logger.info(f"已创建适配器: {tool_type.value}")
            
            # 2. 连接测试工具
            self._send_log(task_no, "正在连接测试工具...")
            if not self._current_adapter.connect():
                raise Exception(f"连接测试工具失败: {self._current_adapter.last_error}")
            
            self._send_log(task_no, "测试工具连接成功")
            
            # 3. 加载配置（如果需要）
            config_path = task.configPath
            if config_path and tool_type != TestToolType.TTWORKBENCH:
                self._send_log(task_no, f"正在加载配置: {config_path}")
                if not self._current_adapter.load_configuration(config_path):
                    raise Exception(f"加载配置失败: {self._current_adapter.last_error}")
                self._send_log(task_no, "配置加载成功")
            
            # 4. 启动测试（如果需要）
            if tool_type != TestToolType.TTWORKBENCH:
                self._send_log(task_no, "正在启动测试...")
                if not self._current_adapter.start_test():
                    raise Exception(f"启动测试失败: {self._current_adapter.last_error}")
                self._send_log(task_no, "测试已启动")
            
            # 5. 执行测试用例
            case_list = task.caseList
            total_cases = len(case_list)
            
            self._send_log(task_no, f"开始执行测试用例，共 {total_cases} 个")
            
            for index, case in enumerate(case_list):
                # 检查是否取消
                if self._stop_event.is_set():
                    self.logger.info("任务被取消")
                    self._update_task_status(TaskStatus.CANCELLED)
                    self._send_status_update(task_no, "CANCELLED")
                    return
                
                # 执行测试用例
                case_no = case.caseNo or f"CASE_{index + 1}"
                case_name = case.caseName or case_no
                
                self._send_log(task_no, f"执行测试用例 {index + 1}/{total_cases}: {case_name} ({case_no})")
                
                # 执行测试项
                test_result = self._current_adapter.execute_test_item(case.to_dict())
                
                # 创建TDM2.0格式的用例结果
                case_result = self._convert_to_case_result(case_no, test_result)
                
                if self._execution_result:
                    self._execution_result.add_case_result(case_result)
                
                # 上报进度
                progress = {
                    "current": index + 1,
                    "total": total_cases,
                    "percentage": int((index + 1) / total_cases * 100),
                    "currentCase": case_result.to_dict()
                }
                self._send_progress_update(task_no, progress)
                
                self._send_log(task_no, f"测试用例执行完成: {case_result.result}")
            
            # 6. 停止测试（如果需要）
            if tool_type != TestToolType.TTWORKBENCH:
                self._send_log(task_no, "正在停止测试...")
                self._current_adapter.stop_test()
                self._send_log(task_no, "测试已停止")
            
            # 7. 任务完成
            self._update_task_status(TaskStatus.COMPLETED)
            self._send_status_update(task_no, "COMPLETED")
            
            # 生成结果摘要并上报
            if self._execution_result:
                self._execution_result.endTime = datetime.now()
                self._execution_result.generate_summary()
                self._send_final_result(task_no, self._execution_result)
            
            elapsed_time = time.time() - self._start_time
            self.logger.info(f"任务执行完成: {task_no}, 耗时: {elapsed_time:.2f}秒")
            
        except Exception as e:
            self.logger.error(f"任务执行失败: {str(e)}")
            self._update_task_status(TaskStatus.FAILED)
            self._send_status_update(task_no, "FAILED", error=str(e))
            self._send_log(task_no, f"任务执行失败: {str(e)}", level="ERROR")
            
        finally:
            # 清理资源
            if self._current_adapter:
                self._current_adapter.disconnect()
                self._current_adapter = None
            
            with self._lock:
                self._current_task = None
                self._task_thread = None
    
    def _convert_to_case_result(self, case_no: str, test_result: Dict[str, Any]) -> CaseResult:
        """
        将测试结果转换为TDM2.0格式的CaseResult
        
        Args:
            case_no: 用例编号
            test_result: 测试结果字典
            
        Returns:
            CaseResult实例
        """
        # 判断测试结果状态
        passed = test_result.get("passed", False) or test_result.get("success", False)
        result_status = CaseResultStatus.PASS.value if passed else CaseResultStatus.FAIL.value
        
        # 构建备注信息
        remarks = []
        if test_result.get("error"):
            remarks.append(f"错误: {test_result['error']}")
        if test_result.get("actual"):
            remarks.append(f"实际值: {test_result['actual']}")
        if test_result.get("expected"):
            remarks.append(f"期望值: {test_result['expected']}")
        
        remark = "; ".join(remarks) if remarks else None
        
        return CaseResult(
            caseNo=case_no,
            result=result_status,
            remark=remark,
            created=datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
            actualResult=str(test_result.get("actual", ""))
        )
    
    def _update_task_status(self, status: TaskStatus):
        """更新任务状态"""
        with self._lock:
            self._task_status = status
    
    def _send_status_update(self, task_no: str, status: str, error: str = None):
        """发送状态更新 (TDM2.0格式)"""
        payload = {
            "taskNo": task_no,
            "status": status,
            "timestamp": int(time.time() * 1000)
        }
        if error:
            payload["error"] = error
        
        self.message_sender("TASK_STATUS", payload)
    
    def _send_progress_update(self, task_no: str, progress: Dict[str, Any]):
        """发送进度更新 (TDM2.0格式)"""
        payload = {
            "taskNo": task_no,
            "progress": progress,
            "timestamp": int(time.time() * 1000)
        }
        self.message_sender("PROGRESS_UPDATE", payload)
    
    def _send_log(self, task_no: str, message: str, level: str = "INFO"):
        """发送日志 (TDM2.0格式)"""
        payload = {
            "taskNo": task_no,
            "level": level,
            "message": message,
            "timestamp": int(time.time() * 1000)
        }
        self.message_sender("LOG", payload)
    
    def _send_final_result(self, task_no: str, execution_result: ExecutionResult):
        """发送最终结果 (TDM2.0格式)"""
        payload = {
            "taskNo": task_no,
            "executionResult": execution_result.to_tdm2_format(),
            "timestamp": int(time.time() * 1000)
        }
        self.message_sender("RESULT_REPORT", payload)


# 单例模式
_task_executor_instance: Optional[TaskExecutorV2] = None


def get_task_executor(message_sender: Callable[[str, Dict], None] = None) -> TaskExecutorV2:
    """
    获取任务执行引擎实例（单例模式）
    
    Args:
        message_sender: 消息发送回调函数（首次创建时需要）
        
    Returns:
        任务执行引擎实例
    """
    global _task_executor_instance
    
    if _task_executor_instance is None:
        if message_sender is None:
            raise ValueError("首次创建任务执行引擎需要提供message_sender")
        _task_executor_instance = TaskExecutorV2(message_sender)
    
    return _task_executor_instance


def reset_task_executor():
    """重置任务执行引擎实例（用于测试）"""
    global _task_executor_instance
    _task_executor_instance = None
