"""
CANoe测试工具适配器

基于COM接口实现CANoe自动化控制
支持CANoe 10/11/12/13/14/15/16/17等版本
"""

import time
import logging
from typing import Optional, Dict, Any

from .base_adapter import BaseTestAdapter, TestToolType, AdapterStatus
from ..variable_cache import get_variable_cache

# 尝试导入pywin32
try:
    import win32com.client
    WIN32_AVAILABLE = True
except ImportError:
    WIN32_AVAILABLE = False
    logging.warning("pywin32未安装，CANoe适配器将无法使用")


class CANoeAdapter(BaseTestAdapter):
    """
    CANoe测试工具适配器
    
    通过COM接口控制CANoe软件，支持以下功能：
    - 连接/断开CANoe应用
    - 加载配置文件(.cfg)
    - 启动/停止测量
    - 信号读写
    - 系统变量操作
    - 测试模块执行
    """
    
    def __init__(self, config: dict = None):
        """
        初始化CANoe适配器
        
        Args:
            config: 配置字典，可包含：
                - app_name: CANoe应用名称（默认"CANoe.Application"）
                - start_timeout: 启动超时时间（默认30秒）
                - stop_timeout: 停止超时时间（默认10秒）
        """
        super().__init__(config)
        self.app_name = self.config.get("app_name", "CANoe.Application")
        self.start_timeout = self.config.get("start_timeout", 30)
        self.stop_timeout = self.config.get("stop_timeout", 10)
        
        self._app = None
        self._measurement = None
        self._bus_systems = None
        self._system_variables = None
        
        # 初始化变量缓存
        self._variable_cache = get_variable_cache(f"canoe_{id(self)}")
        self._cache_enabled = True
        
    @property
    def tool_type(self) -> TestToolType:
        """返回测试工具类型"""
        return TestToolType.CANOE
    
    def connect(self) -> bool:
        """
        连接CANoe应用
        
        Returns:
            连接成功返回True，否则返回False
        """
        if not WIN32_AVAILABLE:
            self._set_error("pywin32未安装，无法连接CANoe")
            return False
        
        try:
            self.status = AdapterStatus.CONNECTING
            self.logger.info(f"正在连接CANoe应用: {self.app_name}")
            
            # 创建COM对象
            self._app = win32com.client.Dispatch(self.app_name)
            
            # 获取测量对象
            self._measurement = self._app.Measurement
            
            # 获取总线系统
            self._bus_systems = self._app.BusSystems
            
            # 获取系统变量
            self._system_variables = self._app.SystemVariables
            
            self.status = AdapterStatus.CONNECTED
            self._clear_error()
            self.logger.info("CANoe连接成功")
            return True
            
        except Exception as e:
            self._set_error(f"CANoe连接失败: {str(e)}")
            return False
    
    def disconnect(self) -> bool:
        """
        断开CANoe连接
        
        Returns:
            断开成功返回True，否则返回False
        """
        try:
            # 停止测量（如果正在运行）
            if self._measurement and self._measurement.Running:
                self.stop_test()
            
            # 释放COM对象
            self._app = None
            self._measurement = None
            self._bus_systems = None
            self._system_variables = None
            
            self.status = AdapterStatus.DISCONNECTED
            self.logger.info("CANoe连接已断开")
            return True
            
        except Exception as e:
            self._set_error(f"CANoe断开连接失败: {str(e)}")
            return False
    
    def load_configuration(self, config_path: str, timeout: int = 30) -> bool:
        """
        加载CANoe配置文件
        
        Args:
            config_path: 配置文件路径(.cfg)
            timeout: 加载超时时间（秒），默认30秒
            
        Returns:
            加载成功返回True，否则返回False
        """
        if not self.is_connected:
            self._set_error("CANoe未连接，无法加载配置")
            return False
        
        try:
            self.logger.info(f"正在加载配置文件: {config_path}")
            
            # 停止当前测量（如果正在运行）
            if self._measurement and self._measurement.Running:
                self.logger.info("停止当前测量...")
                self._measurement.Stop()
                time.sleep(1)
            
            # 打开配置
            self._app.Open(config_path)
            
            # 等待配置加载完成
            start_time = time.time()
            while time.time() - start_time < timeout:
                try:
                    result = self._app.Configuration.OpenConfigurationResult
                    if result == 0:
                        self.logger.info("配置文件加载成功")
                        return True
                    else:
                        self._set_error(f"配置文件加载失败，错误码: {result}")
                        return False
                except:
                    # 配置还在加载中，继续等待
                    time.sleep(0.5)
            
            # 超时
            self._set_error(f"配置文件加载超时（{timeout}秒）")
            return False
            
        except Exception as e:
            self._set_error(f"配置文件加载失败: {str(e)}")
            return False
    
    def start_test(self) -> bool:
        """
        启动CANoe测量
        
        Returns:
            启动成功返回True，否则返回False
        """
        if not self.is_connected:
            self._set_error("CANoe未连接，无法启动测量")
            return False
        
        try:
            if self._measurement.Running:
                self.logger.warning("测量已在运行")
                return True
            
            self.logger.info("正在启动测量...")
            self._measurement.Start()
            
            # 等待启动完成
            timeout = self.start_timeout
            while not self._measurement.Running and timeout > 0:
                time.sleep(0.5)
                timeout -= 0.5
            
            if self._measurement.Running:
                self.status = AdapterStatus.RUNNING
                self.logger.info("测量已启动")
                return True
            else:
                self._set_error("测量启动超时")
                return False
                
        except Exception as e:
            self._set_error(f"启动测量失败: {str(e)}")
            return False
    
    def stop_test(self) -> bool:
        """
        停止CANoe测量
        
        Returns:
            停止成功返回True，否则返回False
        """
        if not self.is_connected:
            self._set_error("CANoe未连接，无法停止测量")
            return False
        
        try:
            if not self._measurement.Running:
                self.logger.warning("测量未在运行")
                return True
            
            self.logger.info("正在停止测量...")
            self._measurement.Stop()
            
            # 等待停止完成
            timeout = self.stop_timeout
            while self._measurement.Running and timeout > 0:
                time.sleep(0.5)
                timeout -= 0.5
            
            if not self._measurement.Running:
                self.status = AdapterStatus.CONNECTED
                self.logger.info("测量已停止")
                return True
            else:
                self._set_error("测量停止超时")
                return False
                
        except Exception as e:
            self._set_error(f"停止测量失败: {str(e)}")
            return False
    
    def execute_test_item(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """
        执行单个测试项
        
        支持的测试项类型：
        - signal_check: 信号检查
        - signal_set: 信号设置
        - sysvar_check: 系统变量检查
        - sysvar_set: 系统变量设置
        - test_module: 执行测试模块
        
        Args:
            item: 测试项配置字典
            
        Returns:
            测试结果字典
        """
        item_type = item.get("type")
        item_name = item.get("name", "unnamed")
        
        self.logger.info(f"执行测试项: {item_name} (类型: {item_type})")
        
        try:
            if item_type == "signal_check":
                return self._execute_signal_check(item)
            elif item_type == "signal_set":
                return self._execute_signal_set(item)
            elif item_type == "sysvar_check":
                return self._execute_sysvar_check(item)
            elif item_type == "sysvar_set":
                return self._execute_sysvar_set(item)
            elif item_type == "test_module":
                return self._execute_test_module(item)
            else:
                return {
                    "name": item_name,
                    "type": item_type,
                    "status": "error",
                    "error": f"不支持的测试项类型: {item_type}"
                }
                
        except Exception as e:
            self.logger.error(f"执行测试项失败: {str(e)}")
            return {
                "name": item_name,
                "type": item_type,
                "status": "error",
                "error": str(e)
            }
    
    def _execute_signal_check(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行信号检查"""
        channel = item.get("channel", 1)
        message = item.get("message")
        signal = item.get("signal")
        expected_value = item.get("expected_value")
        tolerance = item.get("tolerance", 0.01)
        
        if not signal:
            raise ValueError("信号检查需要指定signal参数")
        
        # 读取信号值
        actual_value = self._read_signal(channel, message, signal)
        
        # 判断结果
        passed = False
        if actual_value is not None and expected_value is not None:
            passed = abs(actual_value - expected_value) < tolerance
        
        return {
            "name": item.get("name"),
            "type": "signal_check",
            "channel": channel,
            "message": message,
            "signal": signal,
            "expected_value": expected_value,
            "actual_value": actual_value,
            "tolerance": tolerance,
            "passed": passed,
            "status": "passed" if passed else "failed"
        }
    
    def _execute_signal_set(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行信号设置"""
        channel = item.get("channel", 1)
        message = item.get("message")
        signal = item.get("signal")
        value = item.get("value")
        
        if not signal or value is None:
            raise ValueError("信号设置需要指定signal和value参数")
        
        # 设置信号值
        success = self._write_signal(channel, message, signal, value)
        
        return {
            "name": item.get("name"),
            "type": "signal_set",
            "channel": channel,
            "message": message,
            "signal": signal,
            "value": value,
            "success": success,
            "status": "passed" if success else "failed"
        }
    
    def _execute_sysvar_check(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行系统变量检查"""
        namespace = item.get("namespace")
        variable = item.get("variable")
        expected_value = item.get("expected_value")
        
        if not namespace or not variable:
            raise ValueError("系统变量检查需要指定namespace和variable参数")
        
        # 读取系统变量
        actual_value = self._read_system_variable(namespace, variable)
        
        # 判断结果
        passed = actual_value == expected_value
        
        return {
            "name": item.get("name"),
            "type": "sysvar_check",
            "namespace": namespace,
            "variable": variable,
            "expected_value": expected_value,
            "actual_value": actual_value,
            "passed": passed,
            "status": "passed" if passed else "failed"
        }
    
    def _execute_sysvar_set(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行系统变量设置"""
        namespace = item.get("namespace")
        variable = item.get("variable")
        value = item.get("value")
        
        if not namespace or not variable or value is None:
            raise ValueError("系统变量设置需要指定namespace、variable和value参数")
        
        # 设置系统变量
        success = self._write_system_variable(namespace, variable, value)
        
        return {
            "name": item.get("name"),
            "type": "sysvar_set",
            "namespace": namespace,
            "variable": variable,
            "value": value,
            "success": success,
            "status": "passed" if success else "failed"
        }
    
    def _execute_test_module(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行测试模块"""
        test_name = item.get("test_name")
        
        if not test_name:
            raise ValueError("测试模块执行需要指定test_name参数")
        
        # 获取测试配置
        test_config = self._app.TestConfiguration
        
        # 执行测试
        test_config.ExecuteTest(test_name)
        
        # 获取测试结果
        verdict = test_config.Verdict
        
        return {
            "name": item.get("name"),
            "type": "test_module",
            "test_name": test_name,
            "verdict": verdict,
            "status": "passed" if verdict == "Passed" else "failed"
        }
    
    def _read_signal(self, channel: int, message: str, signal: str) -> Optional[float]:
        """读取信号值"""
        try:
            bus = self._bus_systems(channel)
            sig = bus.Signals(signal)
            return float(sig.Value)
        except Exception as e:
            self.logger.warning(f"读取信号失败: {str(e)}")
            return None
    
    def _write_signal(self, channel: int, message: str, signal: str, value: float) -> bool:
        """写入信号值"""
        try:
            bus = self._bus_systems(channel)
            sig = bus.Signals(signal)
            sig.Value = value
            return True
        except Exception as e:
            self.logger.warning(f"写入信号失败: {str(e)}")
            return False
    
    def _read_system_variable(self, namespace: str, variable: str, use_cache: bool = True) -> Any:
        """
        读取系统变量（支持缓存）
        
        Args:
            namespace: 命名空间
            variable: 变量名
            use_cache: 是否使用缓存，默认True
            
        Returns:
            变量值
        """
        cache_key = f"{namespace}.{variable}"
        
        # 尝试从缓存读取
        if use_cache and self._cache_enabled:
            cached = self._variable_cache.get(variable, namespace)
            if cached and cached.var_object:
                try:
                    value = cached.var_object.Value
                    cached.value = value
                    self.logger.debug(f"从缓存读取变量: {cache_key} = {value}")
                    return value
                except Exception as e:
                    self.logger.warning(f"缓存变量访问失败，重新获取: {str(e)}")
                    self._variable_cache.invalidate(variable, namespace)
        
        # 从CANoe读取
        try:
            ns = self._system_variables.Namespaces(namespace)
            var = ns.Variables(variable)
            value = var.Value
            
            # 缓存变量对象
            if use_cache and self._cache_enabled:
                self._variable_cache.put(variable, namespace, var, value)
                self.logger.debug(f"缓存变量: {cache_key} = {value}")
            
            return value
        except Exception as e:
            self.logger.warning(f"读取系统变量失败: {str(e)}")
            return None
    
    def _write_system_variable(self, namespace: str, variable: str, value: Any, 
                               use_cache: bool = True) -> bool:
        """
        写入系统变量（支持缓存）
        
        Args:
            namespace: 命名空间
            variable: 变量名
            value: 要写入的值
            use_cache: 是否使用缓存，默认True
            
        Returns:
            写入成功返回True
        """
        cache_key = f"{namespace}.{variable}"
        var_object = None
        
        # 尝试从缓存获取变量对象
        if use_cache and self._cache_enabled:
            cached = self._variable_cache.get(variable, namespace)
            if cached and cached.var_object:
                var_object = cached.var_object
                self.logger.debug(f"从缓存获取变量对象: {cache_key}")
        
        # 如果缓存中没有，从CANoe获取
        if var_object is None:
            try:
                ns = self._system_variables.Namespaces(namespace)
                var_object = ns.Variables(variable)
                
                # 缓存变量对象
                if use_cache and self._cache_enabled:
                    self._variable_cache.put(variable, namespace, var_object, value)
            except Exception as e:
                self.logger.warning(f"获取变量对象失败: {str(e)}")
                return False
        
        # 写入值
        try:
            var_object.Value = value
            
            # 更新缓存中的值
            if use_cache and self._cache_enabled:
                cached = self._variable_cache.get(variable, namespace)
                if cached:
                    cached.value = value
            
            self.logger.debug(f"写入变量: {cache_key} = {value}")
            return True
        except Exception as e:
            self.logger.warning(f"写入系统变量失败: {str(e)}")
            # 使缓存失效
            if use_cache and self._cache_enabled:
                self._variable_cache.invalidate(variable, namespace)
            return False
    
    def get_cache_stats(self) -> Dict[str, Any]:
        """获取变量缓存统计信息"""
        return self._variable_cache.get_stats()
    
    def clear_variable_cache(self) -> None:
        """清除变量缓存"""
        self._variable_cache.invalidate_all()
        self.logger.info("变量缓存已清除")
    
    def enable_variable_cache(self, enabled: bool = True) -> None:
        """启用或禁用变量缓存"""
        self._cache_enabled = enabled
        self.logger.info(f"变量缓存已{'启用' if enabled else '禁用'}")
    
    def batch_read_variables(self, variables: list) -> Dict[str, Any]:
        """
        批量读取系统变量
        
        Args:
            variables: 变量列表，每个元素为(namespace, variable)元组
            
        Returns:
            变量值字典，键为"namespace.variable"
        """
        results = {}
        
        for namespace, variable in variables:
            value = self._read_system_variable(namespace, variable)
            key = f"{namespace}.{variable}"
            results[key] = value
        
        return results
    
    def batch_write_variables(self, variables: Dict[str, Any]) -> Dict[str, bool]:
        """
        批量写入系统变量
        
        Args:
            variables: 变量值字典，键为"namespace.variable"
            
        Returns:
            写入结果字典
        """
        results = {}
        
        for key, value in variables.items():
            parts = key.split('.', 1)
            if len(parts) == 2:
                namespace, variable = parts
                success = self._write_system_variable(namespace, variable, value)
                results[key] = success
            else:
                results[key] = False
        
        return results
