"""
TSMaster 测试步骤执行引擎

提供 TSMaster 测试步骤的执行和管理功能
支持 RPC 模式和传统模式
"""
import time
import uuid
import logging
from datetime import datetime
from typing import Dict, Any, List, Optional, Callable
from dataclasses import dataclass, field, asdict
from enum import Enum

from core.adapters.tsmaster_adapter import TSMasterAdapter
from utils.logger import get_logger


class TestStepType(Enum):
    """测试步骤类型"""
    SIGNAL_CHECK = "signal_check"
    SIGNAL_SET = "signal_set"
    LIN_SIGNAL_CHECK = "lin_signal_check"
    LIN_SIGNAL_SET = "lin_signal_set"
    SYSVAR_CHECK = "sysvar_check"
    SYSVAR_SET = "sysvar_set"
    MESSAGE_SEND = "message_send"
    WAIT = "wait"
    CONDITION = "condition"
    DIAGNOSTIC = "diagnostic"


class TestStepStatus(Enum):
    """测试步骤状态"""
    PENDING = "pending"
    RUNNING = "running"
    PASSED = "passed"
    FAILED = "failed"
    SKIPPED = "skipped"
    ERROR = "error"


@dataclass
class TestStep:
    """测试步骤定义"""
    id: str = ""
    name: str = ""
    type: str = ""
    enabled: bool = True
    parameters: Dict[str, Any] = field(default_factory=dict)
    timeout: int = 5000  # ms
    retry_count: int = 0
    on_failure: str = "continue"  # continue/stop/abort
    
    def to_dict(self) -> Dict[str, Any]:
        return asdict(self)
    
    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'TestStep':
        return cls(**data)


@dataclass
class TestStepResult:
    """测试步骤执行结果"""
    step_id: str = ""
    name: str = ""
    type: str = ""
    status: str = ""
    message: str = ""
    duration: float = 0.0
    actual_value: Any = None
    expected_value: Any = None
    details: Dict[str, Any] = field(default_factory=dict)
    timestamp: str = ""
    
    def to_dict(self) -> Dict[str, Any]:
        result = asdict(self)
        # 处理不可序列化的类型
        if self.actual_value is not None:
            result['actual_value'] = str(self.actual_value)
        if self.expected_value is not None:
            result['expected_value'] = str(self.expected_value)
        return result


@dataclass
class TestExecutionConfig:
    """测试执行配置"""
    use_rpc: bool = True
    rpc_app_name: Optional[str] = None
    fallback_to_traditional: bool = True
    start_timeout: int = 30
    stop_timeout: int = 10
    project_path: Optional[str] = None
    auto_start_simulation: bool = True
    auto_stop_simulation: bool = True
    master_form_name: str = "C 代码编辑器 [Master]"  # Master小程序名称

    def to_dict(self) -> Dict[str, Any]:
        return asdict(self)

    def to_adapter_config(self) -> Dict[str, Any]:
        """转换为适配器配置"""
        return {
            "use_rpc": self.use_rpc,
            "rpc_app_name": self.rpc_app_name,
            "fallback_to_traditional": self.fallback_to_traditional,
            "start_timeout": self.start_timeout,
            "stop_timeout": self.stop_timeout,
            "master_form_name": self.master_form_name
        }


@dataclass
class TestExecutionResult:
    """测试执行结果"""
    execution_id: str = ""
    status: str = ""  # completed/running/failed/aborted
    config: Dict[str, Any] = field(default_factory=dict)
    steps: List[Dict[str, Any]] = field(default_factory=list)
    results: List[Dict[str, Any]] = field(default_factory=list)
    total_duration: float = 0.0
    summary: Dict[str, int] = field(default_factory=dict)
    start_time: str = ""
    end_time: str = ""
    message: str = ""
    
    def to_dict(self) -> Dict[str, Any]:
        return asdict(self)


class TSMasterTestEngine:
    """
    TSMaster 测试步骤执行引擎
    
    功能：
    - 执行测试步骤序列
    - 管理 TSMaster 连接
    - 提供实时进度回调
    - 支持测试步骤的 CRUD 操作
    """
    
    def __init__(self):
        self.logger = get_logger("tsmaster_test_engine")
        self.adapter: Optional[TSMasterAdapter] = None
        self._progress_callbacks: List[Callable] = []
        self._step_templates: Dict[str, Dict[str, Any]] = self._load_step_templates()
        
    def _load_step_templates(self) -> Dict[str, Dict[str, Any]]:
        """加载测试步骤模板"""
        return {
            "signal_check": {
                "name": "信号检查",
                "description": "检查信号值是否符合预期",
                "parameters": {
                    "signal_name": {"type": "string", "required": True, "label": "信号名称"},
                    "expected_value": {"type": "number", "required": True, "label": "期望值"},
                    "tolerance": {"type": "number", "required": False, "label": "容差", "default": 0.01}
                }
            },
            "signal_set": {
                "name": "信号设置",
                "description": "设置信号值",
                "parameters": {
                    "signal_name": {"type": "string", "required": True, "label": "信号名称"},
                    "value": {"type": "number", "required": True, "label": "设置值"}
                }
            },
            "lin_signal_check": {
                "name": "LIN信号检查",
                "description": "检查LIN信号值",
                "parameters": {
                    "signal_name": {"type": "string", "required": True, "label": "信号名称"},
                    "expected_value": {"type": "number", "required": True, "label": "期望值"},
                    "tolerance": {"type": "number", "required": False, "label": "容差", "default": 0.01}
                }
            },
            "lin_signal_set": {
                "name": "LIN信号设置",
                "description": "设置LIN信号值",
                "parameters": {
                    "signal_name": {"type": "string", "required": True, "label": "信号名称"},
                    "value": {"type": "number", "required": True, "label": "设置值"}
                }
            },
            "sysvar_check": {
                "name": "系统变量检查",
                "description": "检查系统变量值",
                "parameters": {
                    "var_name": {"type": "string", "required": True, "label": "变量名称"},
                    "expected_value": {"type": "string", "required": True, "label": "期望值"}
                }
            },
            "sysvar_set": {
                "name": "系统变量设置",
                "description": "设置系统变量值",
                "parameters": {
                    "var_name": {"type": "string", "required": True, "label": "变量名称"},
                    "value": {"type": "string", "required": True, "label": "设置值"}
                }
            },
            "message_send": {
                "name": "发送报文",
                "description": "发送CAN报文",
                "parameters": {
                    "channel": {"type": "integer", "required": False, "label": "通道", "default": 0},
                    "msg_id": {"type": "integer", "required": True, "label": "报文ID"},
                    "data": {"type": "array", "required": True, "label": "数据字节"},
                    "is_extended": {"type": "boolean", "required": False, "label": "扩展帧", "default": False},
                    "is_fd": {"type": "boolean", "required": False, "label": "CAN FD", "default": False}
                }
            },
            "wait": {
                "name": "等待",
                "description": "等待指定时间",
                "parameters": {
                    "duration": {"type": "integer", "required": True, "label": "等待时间(ms)"}
                }
            },
            "condition": {
                "name": "条件判断",
                "description": "根据条件执行不同步骤",
                "parameters": {
                    "condition": {"type": "string", "required": True, "label": "条件表达式"},
                    "true_steps": {"type": "array", "required": False, "label": "条件为真时执行的步骤"},
                    "false_steps": {"type": "array", "required": False, "label": "条件为假时执行的步骤"}
                }
            }
        }
    
    def register_progress_callback(self, callback: Callable):
        """注册进度回调函数"""
        self._progress_callbacks.append(callback)
    
    def unregister_progress_callback(self, callback: Callable):
        """注销进度回调函数"""
        if callback in self._progress_callbacks:
            self._progress_callbacks.remove(callback)
    
    def _notify_progress(self, event: str, data: Dict[str, Any]):
        """通知进度更新"""
        for callback in self._progress_callbacks:
            try:
                callback(event, data)
            except Exception as e:
                self.logger.error(f"进度回调执行失败: {e}")
    
    def get_step_templates(self) -> Dict[str, Dict[str, Any]]:
        """获取测试步骤模板"""
        return self._step_templates
    
    def get_step_template(self, step_type: str) -> Optional[Dict[str, Any]]:
        """获取指定类型的步骤模板"""
        return self._step_templates.get(step_type)
    
    def validate_step(self, step: TestStep) -> tuple[bool, str]:
        """验证测试步骤"""
        if not step.type:
            return False, "步骤类型不能为空"
        
        template = self._step_templates.get(step.type)
        if not template:
            return False, f"未知的步骤类型: {step.type}"
        
        # 验证必需参数
        params_def = template.get("parameters", {})
        for param_name, param_def in params_def.items():
            if param_def.get("required", False):
                if param_name not in step.parameters:
                    return False, f"缺少必需参数: {param_def.get('label', param_name)}"
        
        return True, ""
    
    def connect(self, config: TestExecutionConfig) -> bool:
        """
        连接到 TSMaster

        RPC调用流程:
        1. 初始化并连接RPC客户端
        2. 启动Master小程序 (app.run_form)
        3. 启动仿真 (start_simulation)

        Args:
            config: 测试执行配置

        Returns:
            连接成功返回 True
        """
        try:
            self.logger.info("正在连接 TSMaster...")

            # 断开现有连接
            if self.adapter:
                self.disconnect()

            # 创建适配器
            adapter_config = config.to_adapter_config()
            self.adapter = TSMasterAdapter(adapter_config)

            # 连接
            if not self.adapter.connect():
                self.logger.error("连接 TSMaster 失败")
                return False

            # 加载工程文件
            if config.project_path:
                self.logger.info(f"加载工程文件: {config.project_path}")
                if not self.adapter.load_configuration(config.project_path):
                    self.logger.warning(f"加载工程文件失败: {config.project_path}")

            # RPC调用流程: 启动Master小程序
            self.logger.info(f"启动Master小程序: {config.master_form_name}")
            self.adapter.start_master_form(config.master_form_name)

            # 启动仿真
            if config.auto_start_simulation:
                self.logger.info("自动启动仿真")
                if not self.adapter.start_test():
                    self.logger.warning("自动启动仿真失败")

            self.logger.info("TSMaster 连接成功")
            return True

        except Exception as e:
            self.logger.error(f"连接 TSMaster 异常: {e}")
            return False
    
    def disconnect(self):
        """
        断开 TSMaster 连接

        RPC调用流程:
        1. 停止仿真 (stop_simulation)
        2. 停止Master小程序 (app.stop_form)
        3. 断开RPC连接
        """
        if self.adapter:
            try:
                # RPC调用流程: 停止Master小程序
                self.logger.info("正在停止Master小程序...")
                self.adapter.stop_master_form()

                # 断开连接
                self.adapter.disconnect()
            except Exception as e:
                self.logger.error(f"断开连接异常: {e}")
            finally:
                self.adapter = None
    
    def execute_steps(self, steps: List[TestStep], config: TestExecutionConfig) -> TestExecutionResult:
        """
        执行测试步骤序列
        
        Args:
            steps: 测试步骤列表
            config: 测试执行配置
            
        Returns:
            测试执行结果
        """
        execution_id = str(uuid.uuid4())[:8]
        start_time = datetime.now()
        
        result = TestExecutionResult(
            execution_id=execution_id,
            status="running",
            config=config.to_dict(),
            steps=[step.to_dict() for step in steps],
            start_time=start_time.isoformat()
        )
        
        self.logger.info(f"开始执行测试步骤序列 [ID: {execution_id}], 共 {len(steps)} 个步骤")
        self._notify_progress("execution_started", {
            "execution_id": execution_id,
            "total_steps": len(steps)
        })
        
        # 连接 TSMaster
        if not self.connect(config):
            result.status = "failed"
            result.message = "连接 TSMaster 失败"
            result.end_time = datetime.now().isoformat()
            result.summary = {
                "total": len(steps),
                "passed": 0,
                "failed": len(steps),
                "skipped": 0
            }
            self._notify_progress("execution_failed", result.to_dict())
            return result
        
        try:
            # 执行测试步骤
            step_results = []
            total_start = time.time()
            
            for i, step in enumerate(steps):
                self.logger.info(f"处理步骤 {i+1}: {step.name}, enabled={step.enabled}")
                
                if not step.enabled:
                    self.logger.info(f"步骤 {i+1} 已禁用，跳过")
                    continue
                
                # 通知步骤开始
                self._notify_progress("step_started", {
                    "execution_id": execution_id,
                    "step_index": i,
                    "step_id": step.id,
                    "step_name": step.name
                })
                
                # 执行步骤
                self.logger.info(f"开始执行步骤: {step.name}, type={step.type}")
                step_result = self._execute_step(step)
                self.logger.info(f"步骤执行完成: {step.name}, status={step_result.status}")
                step_results.append(step_result.to_dict())
                
                # 通知步骤完成
                self._notify_progress("step_completed", {
                    "execution_id": execution_id,
                    "step_index": i,
                    "step_id": step.id,
                    "result": step_result.to_dict()
                })
                
                # 检查失败处理策略
                if step_result.status == "failed":
                    if step.on_failure == "stop":
                        self.logger.info(f"步骤 {step.name} 失败，根据策略停止执行")
                        break
                    elif step.on_failure == "abort":
                        self.logger.info(f"步骤 {step.name} 失败，根据策略中止执行")
                        result.status = "aborted"
                        break
            
            total_duration = time.time() - total_start
            
            # 计算统计信息
            passed = len([r for r in step_results if r["status"] == "passed"])
            failed = len([r for r in step_results if r["status"] == "failed"])
            skipped = len([r for r in step_results if r["status"] == "skipped"])
            
            result.results = step_results
            result.total_duration = total_duration
            result.summary = {
                "total": len(step_results),
                "passed": passed,
                "failed": failed,
                "skipped": skipped
            }
            result.status = "completed" if result.status != "aborted" else "aborted"
            result.end_time = datetime.now().isoformat()
            
            self.logger.info(f"测试步骤序列执行完成 [ID: {execution_id}], "
                           f"通过: {passed}, 失败: {failed}, 跳过: {skipped}")
            
        except Exception as e:
            self.logger.error(f"执行测试步骤序列异常: {e}")
            result.status = "error"
            result.message = str(e)
            result.end_time = datetime.now().isoformat()
            # 确保有统计信息
            if not result.summary:
                result.summary = {
                    "total": len(steps),
                    "passed": 0,
                    "failed": len(steps),
                    "skipped": 0
                }
            
        finally:
            # 断开连接（内部会停止仿真和Master小程序）
            self.disconnect()
        
        self._notify_progress("execution_completed", result.to_dict())
        return result
    
    def _execute_step(self, step: TestStep) -> TestStepResult:
        """执行单个测试步骤"""
        start_time = time.time()
        timestamp = datetime.now().isoformat()
        
        result = TestStepResult(
            step_id=step.id,
            name=step.name,
            type=step.type,
            status="running",
            timestamp=timestamp
        )
        
        self.logger.info(f"执行步骤: {step.name} (类型: {step.type})")
        
        try:
            # 验证步骤
            valid, error_msg = self.validate_step(step)
            if not valid:
                result.status = "error"
                result.message = error_msg
                result.duration = time.time() - start_time
                return result
            
            # 根据类型执行
            if step.type == "signal_check":
                result = self._execute_signal_check(step, start_time, timestamp)
            elif step.type == "signal_set":
                result = self._execute_signal_set(step, start_time, timestamp)
            elif step.type == "lin_signal_check":
                result = self._execute_lin_signal_check(step, start_time, timestamp)
            elif step.type == "lin_signal_set":
                result = self._execute_lin_signal_set(step, start_time, timestamp)
            elif step.type == "sysvar_check":
                result = self._execute_sysvar_check(step, start_time, timestamp)
            elif step.type == "sysvar_set":
                result = self._execute_sysvar_set(step, start_time, timestamp)
            elif step.type == "message_send":
                result = self._execute_message_send(step, start_time, timestamp)
            elif step.type == "wait":
                result = self._execute_wait(step, start_time, timestamp)
            else:
                result.status = "error"
                result.message = f"未实现的步骤类型: {step.type}"
                result.duration = time.time() - start_time
            
        except Exception as e:
            self.logger.error(f"执行步骤 {step.name} 异常: {e}")
            result.status = "error"
            result.message = f"执行异常: {str(e)}"
            result.duration = time.time() - start_time
        
        return result
    
    def _execute_signal_check(self, step: TestStep, start_time: float, timestamp: str) -> TestStepResult:
        """执行信号检查"""
        params = step.parameters
        signal_name = params.get("signal_name")
        expected_value = float(params.get("expected_value", 0))
        tolerance = float(params.get("tolerance", 0.01))
        
        self.logger.info(f"执行信号检查: signal_name={signal_name}, expected_value={expected_value}, tolerance={tolerance}")
        
        # 读取信号值
        actual_value = self.adapter._get_signal(signal_name)
        self.logger.info(f"读取信号结果: {actual_value}")
        
        # 判断结果
        passed = False
        if actual_value is not None:
            passed = abs(actual_value - expected_value) < tolerance
        
        self.logger.info(f"信号检查完成: passed={passed}")
        
        return TestStepResult(
            step_id=step.id,
            name=step.name,
            type=step.type,
            status="passed" if passed else "failed",
            message=f"信号 {signal_name} 期望值: {expected_value}, 实际值: {actual_value}",
            duration=time.time() - start_time,
            actual_value=actual_value,
            expected_value=expected_value,
            timestamp=timestamp
        )
    
    def _execute_signal_set(self, step: TestStep, start_time: float, timestamp: str) -> TestStepResult:
        """执行信号设置"""
        params = step.parameters
        signal_name = params.get("signal_name")
        value = float(params.get("value", 0))
        
        self.logger.info(f"执行信号设置: signal_name={signal_name}, value={value}")
        
        # 设置信号值
        success = self.adapter._set_signal(signal_name, value)
        
        self.logger.info(f"信号设置完成: success={success}")
        
        return TestStepResult(
            step_id=step.id,
            name=step.name,
            type=step.type,
            status="passed" if success else "failed",
            message=f"设置信号 {signal_name} = {value} {'成功' if success else '失败'}",
            duration=time.time() - start_time,
            timestamp=timestamp
        )
    
    def _execute_lin_signal_check(self, step: TestStep, start_time: float, timestamp: str) -> TestStepResult:
        """执行 LIN 信号检查"""
        params = step.parameters
        signal_name = params.get("signal_name")
        expected_value = float(params.get("expected_value", 0))
        tolerance = float(params.get("tolerance", 0.01))
        
        # 使用 RPC 客户端读取 LIN 信号
        actual_value = None
        if self.adapter._rpc_client:
            actual_value = self.adapter._rpc_client.get_lin_signal(signal_name)
        
        # 判断结果
        passed = False
        if actual_value is not None:
            passed = abs(actual_value - expected_value) < tolerance
        
        return TestStepResult(
            step_id=step.id,
            name=step.name,
            type=step.type,
            status="passed" if passed else "failed",
            message=f"LIN信号 {signal_name} 期望值: {expected_value}, 实际值: {actual_value}",
            duration=time.time() - start_time,
            actual_value=actual_value,
            expected_value=expected_value,
            timestamp=timestamp
        )
    
    def _execute_lin_signal_set(self, step: TestStep, start_time: float, timestamp: str) -> TestStepResult:
        """执行 LIN 信号设置"""
        params = step.parameters
        signal_name = params.get("signal_name")
        value = float(params.get("value", 0))
        
        # 使用 RPC 客户端设置 LIN 信号
        success = False
        if self.adapter._rpc_client:
            success = self.adapter._rpc_client.set_lin_signal(signal_name, value)
        
        return TestStepResult(
            step_id=step.id,
            name=step.name,
            type=step.type,
            status="passed" if success else "failed",
            message=f"设置LIN信号 {signal_name} = {value} {'成功' if success else '失败'}",
            duration=time.time() - start_time,
            timestamp=timestamp
        )
    
    def _execute_sysvar_check(self, step: TestStep, start_time: float, timestamp: str) -> TestStepResult:
        """执行系统变量检查"""
        params = step.parameters
        var_name = params.get("var_name")
        expected_value = str(params.get("expected_value", ""))
        
        self.logger.info(f"执行系统变量检查: var_name={var_name}, expected_value={expected_value}")
        
        # 读取系统变量
        actual_value = self.adapter._read_system_var(var_name)
        self.logger.info(f"读取系统变量结果: {actual_value}")
        
        # 判断结果
        passed = False
        if actual_value is not None:
            passed = str(actual_value) == expected_value
        
        self.logger.info(f"系统变量检查完成: passed={passed}")
        
        return TestStepResult(
            step_id=step.id,
            name=step.name,
            type=step.type,
            status="passed" if passed else "failed",
            message=f"系统变量 {var_name} 期望值: {expected_value}, 实际值: {actual_value}",
            duration=time.time() - start_time,
            actual_value=actual_value,
            expected_value=expected_value,
            timestamp=timestamp
        )
    
    def _execute_sysvar_set(self, step: TestStep, start_time: float, timestamp: str) -> TestStepResult:
        """执行系统变量设置"""
        params = step.parameters
        var_name = params.get("var_name")
        value = str(params.get("value", ""))
        
        # 设置系统变量
        success = self.adapter._write_system_var(var_name, value)
        
        return TestStepResult(
            step_id=step.id,
            name=step.name,
            type=step.type,
            status="passed" if success else "failed",
            message=f"设置系统变量 {var_name} = {value} {'成功' if success else '失败'}",
            duration=time.time() - start_time,
            timestamp=timestamp
        )
    
    def _execute_message_send(self, step: TestStep, start_time: float, timestamp: str) -> TestStepResult:
        """执行报文发送"""
        params = step.parameters
        channel = int(params.get("channel", 0))
        msg_id = int(params.get("msg_id", 0))
        data = params.get("data", [])
        
        # 发送报文
        success = self.adapter._send_message(channel, msg_id, data)
        
        return TestStepResult(
            step_id=step.id,
            name=step.name,
            type=step.type,
            status="passed" if success else "failed",
            message=f"发送报文 ID=0x{msg_id:X} {'成功' if success else '失败'}",
            duration=time.time() - start_time,
            timestamp=timestamp
        )
    
    def _execute_wait(self, step: TestStep, start_time: float, timestamp: str) -> TestStepResult:
        """执行等待"""
        params = step.parameters
        duration_ms = int(params.get("duration", 1000))
        
        self.logger.info(f"执行等待: duration={duration_ms}ms")
        
        # 等待
        time.sleep(duration_ms / 1000)
        
        self.logger.info("等待完成")
        
        return TestStepResult(
            step_id=step.id,
            name=step.name,
            type=step.type,
            status="passed",
            message=f"等待 {duration_ms}ms",
            duration=time.time() - start_time,
            timestamp=timestamp
        )


# 全局测试引擎实例
_test_engine: Optional[TSMasterTestEngine] = None


def get_test_engine() -> TSMasterTestEngine:
    """获取全局测试引擎实例"""
    global _test_engine
    if _test_engine is None:
        _test_engine = TSMasterTestEngine()
    return _test_engine
