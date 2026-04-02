"""
CANoe适配器主类

基于BaseTestAdapter实现的CANoe测试工具适配器，
整合COM接口包装器和测试执行引擎，提供完整的测试功能。

作者: AI Assistant
创建日期: 2026-02-25
"""

import os
import time
import logging
from typing import Dict, Any, List, Optional, Callable
from datetime import datetime

from ..base_adapter import BaseTestAdapter, TestToolType, AdapterStatus
from ..capabilities import (
    ArtifactCapability,
    ConfigurationCapability,
    MeasurementCapability,
    TestModuleCapability,
)
from .com_wrapper import CANoeCOMWrapper, DeviceInfo, CANoeError
from .test_engine import CANoeTestEngine, TestStatus, TestExecutionResult


class CANoeAdapter(BaseTestAdapter):
    """
    CANoe测试工具适配器（完善版）
    
    通过Python直接调用CANoe COM接口，支持：
    - CANoe应用程序控制（连接/断开）
    - 配置文件加载（.cfg）
    - 测量控制（启动/停止）
    - TestModule 直接执行
    - 设备自检
    - 实时进度和日志回调
    
    Attributes:
        canoe_wrapper: CANoe COM包装器实例
        test_engine: 测试执行引擎实例
        config: 配置字典
        
    Example:
        >>> adapter = CANoeAdapter({
        ...     "start_timeout": 30,
        ...     "measurement_timeout": 3600
        ... })
        >>> 
        >>> # 使用上下文管理器
        >>> with adapter:
        ...     adapter.load_configuration("test.cfg")
        ...     result = adapter.execute_test_item({
        ...         "type": "test_module",
        ...         "name": "SmokeModule"
        ...     })
        ...     print(f"测试结果: {result['status']}")
        >>> 
        >>> # 或者手动管理连接
        >>> adapter.connect()
        >>> adapter.load_configuration("test.cfg")
        >>> adapter.start_test()
        >>> adapter.execute_test_module_direct("SmokeModule")
        >>> adapter.stop_test()
        >>> adapter.disconnect()
    """
    
    def __init__(self, config: Dict[str, Any] = None):
        """
        初始化CANoe适配器
        
        Args:
            config: 配置字典，可包含：
                - start_timeout: 启动超时时间（默认30秒）
                - stop_timeout: 停止超时时间（默认10秒）
                - measurement_timeout: 测量超时时间（默认3600秒）
                - open_timeout: 打开配置超时时间（默认30秒）
                - case_timeout: 单个用例超时时间（默认600秒）
                - self_check_timeout: 自检超时时间（默认300秒）
                - retry_count: 连接重试次数（默认3次）
                - retry_interval: 重试间隔（默认2秒）
                - canoe_version: 期望的CANoe版本（可选）
        """
        super().__init__(config)
        
        # 创建COM包装器和测试引擎
        self._canoe_wrapper = CANoeCOMWrapper(self.logger)
        self._test_engine = CANoeTestEngine(
            self._canoe_wrapper,
            namespace=self.config.get("namespace", "mutualVar"),
            logger=self.logger
        )
        
        # 配置参数
        self.start_timeout = self.config.get("start_timeout", 30)
        self.stop_timeout = self.config.get("stop_timeout", 10)
        self.measurement_timeout = self.config.get("measurement_timeout", 3600)
        self.open_timeout = self.config.get("open_timeout", 30)
        self.case_timeout = self.config.get("case_timeout", 600)
        self.self_check_timeout = self.config.get("self_check_timeout", 300)
        self.retry_count = self.config.get("retry_count", 3)
        self.retry_interval = self.config.get("retry_interval", 2.0)
        self.expected_version = self.config.get("canoe_version")
        
        # 进度和日志回调
        self._progress_callback: Optional[Callable[[str, int, int], None]] = None
        self._log_callback: Optional[Callable[[str, str], None]] = None
        
        # 当前执行状态
        self._current_task: Optional[Dict[str, Any]] = None
        self._last_result: Optional[Dict[str, Any]] = None
        self.error_message: Optional[str] = None
        self._register_capabilities()
        
    @property
    def tool_type(self) -> TestToolType:
        """返回测试工具类型"""
        return TestToolType.CANOE

    def _register_capabilities(self) -> None:
        self.register_capability(
            "configuration",
            ConfigurationCapability(load=self.load_configuration),
        )
        self.register_capability(
            "measurement",
            MeasurementCapability(start=self.start_test, stop=self.stop_test),
        )
        self.register_capability(
            "test_module",
            TestModuleCapability(
                execute_module=self.execute_test_module_direct,
                list_modules=self.get_test_modules,
            ),
        )
        self.register_capability(
            "artifact",
            ArtifactCapability(collect=lambda: self.get_status().get("last_result")),
        )
    
    @property
    def canoe_version(self) -> Optional[str]:
        """获取CANoe版本"""
        if self._canoe_wrapper.version:
            return str(self._canoe_wrapper.version)
        return None
    
    def set_progress_callback(self, callback: Callable[[str, int, int], None]):
        """
        设置进度回调函数
        
        Args:
            callback: 回调函数，参数为(当前用例名, 已完成数, 总数)
        """
        self._progress_callback = callback
        self._test_engine.set_progress_callback(callback)
        
    def set_log_callback(self, callback: Callable[[str, str], None]):
        """
        设置日志回调函数
        
        Args:
            callback: 回调函数，参数为(用例名, 日志内容)
        """
        self._log_callback = callback
        self._test_engine.set_log_callback(callback)
    
    def connect(self) -> bool:
        """
        连接CANoe应用程序
        
        Returns:
            连接成功返回True
            
        Raises:
            CANoeError: 连接失败时抛出
        """
        try:
            self.status = AdapterStatus.CONNECTING
            self.logger.info("正在连接CANoe...")
            
            # 连接CANoe
            if not self._canoe_wrapper.connect(
                retry_count=self.retry_count,
                retry_interval=self.retry_interval
            ):
                self._set_error("CANoe连接失败")
                return False
            
            # 验证版本（如果指定）
            if self.expected_version:
                if not self._check_version():
                    self.logger.warning("CANoe版本检查未通过，但连接继续")
            
            self.status = AdapterStatus.CONNECTED
            self._clear_error()
            self.logger.info(f"CANoe连接成功 (版本: {self.canoe_version})")
            return True
            
        except CANoeError as e:
            self._set_error(f"CANoe连接失败: {str(e)}")
            raise
        except Exception as e:
            self._set_error(f"CANoe连接异常: {str(e)}")
            return False

    def _check_version(self) -> bool:
        """
        检查CANoe版本是否匹配

        使用主版本号匹配而非严格的startswith匹配，
        例如期望"17.2"时，"17.3.91"也可以接受。

        Returns:
            版本匹配返回True
        """
        actual_version = self.canoe_version
        if not actual_version:
            self.logger.warning("无法获取CANoe版本信息")
            return True  # 无法获取版本时默认通过

        try:
            # 提取主版本号
            expected_major = self.expected_version.split('.')[0]
            actual_major = actual_version.split('.')[0]

            if actual_major != expected_major:
                self.logger.warning(
                    f"CANoe主版本不匹配: 期望 {expected_major}.x, 实际 {actual_version}"
                )
                return False

            self.logger.debug(f"CANoe版本检查通过: {actual_version}")
            return True

        except Exception as e:
            self.logger.warning(f"版本检查失败: {e}")
            return True  # 解析失败时默认通过

    def _ensure_connected(self) -> bool:
        """
        确保连接有效，必要时自动重连

        此方法用于在执行关键操作前检查连接状态，
        如果处于ERROR状态会尝试自动恢复。

        Returns:
            连接有效返回True
        """
        # 如果处于ERROR状态，尝试自动恢复
        if self.status == AdapterStatus.ERROR:
            self.logger.info("检测到ERROR状态，尝试自动恢复...")
            try:
                self.disconnect()
                time.sleep(1.0)
            except Exception as e:
                self.logger.warning(f"清理ERROR状态失败: {e}")

        # 检查是否已连接
        if not self.is_connected:
            self.logger.info("连接已断开，尝试重新连接...")
            return self.connect()

        # 验证连接有效性（健康检查）
        try:
            # 通过访问版本信息验证COM对象是否有效
            _ = self._canoe_wrapper.version
            return True
        except Exception as e:
            self.logger.warning(f"连接健康检查失败: {e}，尝试重新连接...")
            self.status = AdapterStatus.ERROR
            return self.connect()
    
    def disconnect(self) -> bool:
        """
        断开CANoe连接
        
        Returns:
            断开成功返回True
        """
        try:
            self.logger.info("正在断开CANoe连接...")
            
            # 停止测试引擎
            if self._test_engine.is_running:
                self._test_engine.stop()
            
            # 断开CANoe
            self._canoe_wrapper.disconnect()
            
            self.status = AdapterStatus.DISCONNECTED
            self.logger.info("CANoe已断开")
            return True
            
        except Exception as e:
            self._set_error(f"断开CANoe失败: {str(e)}")
            return False
    
    def load_configuration(self, config_path: str) -> bool:
        """
        加载CANoe配置文件

        Args:
            config_path: 配置文件路径（.cfg）

        Returns:
            加载成功返回True
        """
        # 确保连接有效
        if not self._ensure_connected():
            self._set_error("CANoe连接无效，无法加载配置")
            return False

        try:
            self.logger.info(f"加载配置: {config_path}")
            result = self._canoe_wrapper.open_configuration(
                config_path,
                timeout=self.open_timeout
            )
            if result:
                self.logger.info(f"配置加载成功: {config_path}")
                return True
            else:
                # 获取更详细的错误信息
                error_msg = self._canoe_wrapper.last_error or "未知错误"
                self._set_error(f"配置加载失败: {error_msg}")
                return False
        except CANoeError as e:
            # 尝试获取更详细的错误信息
            error_msg = str(e)
            if self._canoe_wrapper.last_error:
                error_msg = self._canoe_wrapper.last_error
            self._set_error(f"加载配置失败: {error_msg}")
            return False
        except Exception as e:
            self._set_error(f"加载配置异常: {str(e)}")
            return False
    
    def start_test(self) -> bool:
        """
        启动CANoe测量

        Returns:
            启动成功返回True
        """
        # 确保连接有效
        if not self._ensure_connected():
            self._set_error("CANoe连接无效，无法启动测量")
            return False

        try:
            self._canoe_wrapper.start_measurement(timeout=self.start_timeout)
            self.status = AdapterStatus.RUNNING
            self.logger.info("CANoe测量启动成功")
            return True
        except CANoeError as e:
            error_msg = str(e)
            # 尝试获取更详细的错误信息
            if self._canoe_wrapper.last_error:
                error_msg = self._canoe_wrapper.last_error
            self._set_error(f"启动测量失败: {error_msg}")
            return False
        except Exception as e:
            self._set_error(f"启动测量异常: {str(e)}")
            return False
    
    def stop_test(self) -> bool:
        """
        停止CANoe测量
        
        Returns:
            停止成功返回True
        """
        if not self.is_connected:
            return True
        
        try:
            self._canoe_wrapper.stop_measurement(timeout=self.stop_timeout)
            self.status = AdapterStatus.CONNECTED
            return True
        except Exception as e:
            self._set_error(f"停止测量失败: {str(e)}")
            return False
    
    def execute_test_item(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """
        执行测试项
        
        支持的测试项类型：
        - self_check: 设备自检
        - test_module: 直接执行 TestModule
        
        Args:
            item: 测试项配置字典
                - type: 测试项类型（必需）
                - name: 测试项名称（可选）
                - 其他参数根据类型不同
                
        Returns:
            测试结果字典
            
        Example:
            >>> # 设备自检
            >>> result = adapter.execute_test_item({
            ...     "type": "self_check",
            ...     "config_path": "selfcheck.cfg",
            ...     "timeout": 300
            ... })
            >>> 
            >>> # 直接执行 TestModule
            >>> result = adapter.execute_test_item({
            ...     "type": "test_module",
            ...     "name": "SmokeModule"
            ... })
        """
        item_type = item.get("type")
        item_name = item.get("name", "unnamed")
        
        self.logger.info(f"执行测试项: {item_name} (类型: {item_type})")
        self._current_task = item
        
        try:
            if item_type == "self_check":
                result = self._execute_self_check(item)
            elif item_type == "test_module":
                result = self._execute_test_module(item)
            else:
                result = {
                    "name": item_name,
                    "type": item_type,
                    "status": "error",
                    "error": f"不支持的测试项类型: {item_type}"
                }
            
            self._last_result = result
            return result
            
        except Exception as e:
            self.logger.error(f"执行测试项失败: {e}")
            error_result = {
                "name": item_name,
                "type": item_type,
                "status": "error",
                "error": str(e)
            }
            self._last_result = error_result
            return error_result
    
    def _execute_self_check(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行设备自检"""
        config_path = item.get("config_path")
        timeout = item.get("timeout", self.self_check_timeout)
        read_device_info = item.get("read_device_info", True)
        
        if not config_path:
            raise ValueError("self_check类型需要指定config_path参数")
        
        result = self._test_engine.execute_self_check(
            config_path, timeout, read_device_info
        )
        
        return {
            "name": item.get("name", "self_check"),
            "type": "self_check",
            "status": result.status.value,
            "device_info": result.device_info.to_dict() if result.device_info else {},
            "errors": result.errors,
            "start_time": result.start_time,
            "end_time": result.end_time
        }
    
    def _execute_test_module(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """
        执行测试模块

        直接通过 CANoe COM 接口执行 TestModule，不使用命名空间/系统变量。

        Args:
            item: 测试项配置字典
                - name: 测试模块名称（必需）
                - timeout: 超时时间（可选，默认600秒）

        Returns:
            测试结果字典
        """
        test_name = item.get("name", "unnamed")
        if not test_name or test_name == "unnamed":
            # 尝试从其他字段获取名称
            test_name = item.get("case_name") or item.get("caseName") or item.get("module_name") or test_name

        timeout = item.get("timeout", self.case_timeout)

        self.logger.info(f"执行测试模块: {test_name}, 超时: {timeout}秒")

        # 直接调用 COM wrapper 执行 TestModule
        result = self._canoe_wrapper.execute_test_module(test_name, timeout)

        return {
            "name": test_name,
            "type": "test_module",
            "status": "completed" if result.get("success") else ("timeout" if "超时" in str(result.get("error", "")) else "failed"),
            "verdict": result.get("verdict"),
            "duration": result.get("duration", 0),
            "error": result.get("error")
        }

    def get_test_modules(self) -> List[str]:
        """
        获取当前配置中的所有测试模块名称

        Returns:
            测试模块名称列表
        """
        return self._canoe_wrapper.get_test_modules()

    def execute_test_module_direct(self, module_name: str, timeout: float = None) -> Dict[str, Any]:
        """
        直接执行指定的测试模块

        Args:
            module_name: 测试模块名称
            timeout: 超时时间（秒）

        Returns:
            执行结果字典
        """
        return self._canoe_wrapper.execute_test_module(
            module_name,
            timeout or self.case_timeout
        )
    
    def get_status(self) -> Dict[str, Any]:
        """
        获取适配器状态
        
        Returns:
            状态字典
        """
        return {
            "tool_type": self.tool_type.value,
            "status": self.status.value,
            "is_connected": self.is_connected,
            "canoe_version": self.canoe_version,
            "is_measurement_running": self._canoe_wrapper.is_measurement_running if self._canoe_wrapper else False,
            "test_engine_status": self._test_engine.get_execution_status() if self._test_engine else {},
            "current_task": self._current_task,
            "last_result": self._last_result,
            "error": self.error_message
        }
    
    def stop(self) -> bool:
        """
        停止当前执行
        
        Returns:
            停止成功返回True
        """
        try:
            if self._test_engine.is_running:
                self._test_engine.stop()
            
            if self._canoe_wrapper.is_measurement_running:
                self._canoe_wrapper.stop_measurement()
            
            self.status = AdapterStatus.CONNECTED
            return True
        except Exception as e:
            self.logger.error(f"停止执行失败: {e}")
            return False
    
    def reset(self) -> bool:
        """
        重置适配器状态
        
        Returns:
            重置成功返回True
        """
        try:
            self.logger.info("正在重置适配器...")
            
            # 停止测试
            self.stop()
            
            # 清除缓存
            self._canoe_wrapper.clear_variable_cache()
            self._test_engine.clear_cache()
            
            # 重置状态
            self._current_task = None
            self._last_result = None
            self._clear_error()
            
            self.logger.info("适配器已重置")
            return True
            
        except Exception as e:
            self.logger.error(f"重置适配器失败: {e}")
            return False
    
    def __enter__(self):
        """上下文管理器入口"""
        self.connect()
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        """上下文管理器出口"""
        self.disconnect()
        return False


# 便捷函数
def create_canoe_adapter(config: Dict[str, Any] = None) -> CANoeAdapter:
    """
    创建CANoe适配器实例
    
    Args:
        config: 配置字典
        
    Returns:
        CANoeAdapter实例
    """
    return CANoeAdapter(config)


# 测试代码
if __name__ == "__main__":
    # 配置日志
    logging.basicConfig(
        level=logging.DEBUG,
        format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
    )
    
    # 测试适配器
    adapter = CANoeAdapter({
        "start_timeout": 30,
        "measurement_timeout": 3600
    })
    
    try:
        # 使用上下文管理器
        with adapter:
            print(f"CANoe版本: {adapter.canoe_version}")
            
            # 加载配置
            if adapter.load_configuration(r"D:\TAMS\DTTC_CONFIG\S59\BCANFD\SMFT\FDCANC_E\TestProjectFile\COMTest.cfg"):
                print("配置加载成功")
                
                # 执行信号检查
                result = adapter.execute_test_item({
                    "type": "signal_check",
                    "name": "EngineSpeed检查",
                    "signal_name": "EngineSpeed",
                    "expected_value": 1000,
                    "tolerance": 50
                })
                print(f"信号检查结果: {result}")
            
    except Exception as e:
        print(f"错误: {e}")
