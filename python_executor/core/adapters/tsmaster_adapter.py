"""
TSMaster测试工具适配器

基于TSMaster Python API和RPC实现自动化控制
支持两种通信方式：
1. RPC模式（推荐）：基于共享内存的高性能通信
2. 传统模式：基于TSMaster Python API
"""

import time
import logging
from typing import Optional, Dict, Any, Callable

from .base_adapter import BaseTestAdapter, TestToolType, AdapterStatus

# 尝试导入TSMaster API
try:
    from TSMaster import *
    TSMASTER_AVAILABLE = True
except ImportError:
    TSMASTER_AVAILABLE = False
    logging.warning("TSMaster Python API未安装，传统模式不可用")

# 尝试导入TSMaster RPC API
try:
    from TSMasterAPI import *
    TSMASTER_RPC_AVAILABLE = True
except ImportError:
    TSMASTER_RPC_AVAILABLE = False
    logging.warning("TSMasterAPI未安装，RPC模式不可用")

# 导入RPC客户端
try:
    from .tsmaster.rpc_client import TSMasterRPCClient
    RPC_CLIENT_AVAILABLE = True
except ImportError:
    RPC_CLIENT_AVAILABLE = False


class TSMasterAdapter(BaseTestAdapter):
    """
    TSMaster测试工具适配器
    
    支持两种通信模式：
    - RPC模式（推荐）：通过TSMasterAPI进行高性能RPC通信
    - 传统模式：通过TSMaster Python API进行通信
    
    功能：
    - 连接/断开TSMaster应用
    - 加载工程文件(.tproj)
    - 启动/停止总线仿真
    - 信号读写
    - 报文收发
    - 系统变量操作
    - C脚本调用
    """
    
    def __init__(self, config: dict = None):
        """
        初始化TSMaster适配器
        
        Args:
            config: 配置字典，可包含：
                - start_timeout: 启动超时时间（默认30秒）
                - stop_timeout: 停止超时时间（默认10秒）
                - use_rpc: 是否使用RPC模式（默认True）
                - rpc_app_name: RPC连接的应用程序名称（默认自动发现）
                - fallback_to_traditional: RPC失败时是否回退到传统模式（默认True）
        """
        super().__init__(config)
        self.start_timeout = self.config.get("start_timeout", 30)
        self.stop_timeout = self.config.get("stop_timeout", 10)
        self.use_rpc = self.config.get("use_rpc", True)
        self.rpc_app_name = self.config.get("rpc_app_name", None)
        self.fallback_to_traditional = self.config.get("fallback_to_traditional", True)
        
        self._ts = None
        self._rpc_client: Optional[TSMasterRPCClient] = None
        self._using_rpc = False
        self._callbacks: Dict[str, Callable] = {}
        
    @property
    def tool_type(self) -> TestToolType:
        """返回测试工具类型"""
        return TestToolType.TSMASTER
    
    def connect(self) -> bool:
        """
        连接TSMaster应用
        
        优先尝试RPC模式，失败后根据配置决定是否回退到传统模式
        
        Returns:
            连接成功返回True，否则返回False
        """
        self.status = AdapterStatus.CONNECTING
        self.logger.info("正在连接TSMaster应用...")
        
        # 尝试RPC模式
        if self.use_rpc and RPC_CLIENT_AVAILABLE:
            self.logger.info("尝试使用RPC模式连接...")
            if self._connect_via_rpc():
                self._using_rpc = True
                self.status = AdapterStatus.CONNECTED
                self._clear_error()
                self.logger.info("TSMaster RPC连接成功")
                return True
            else:
                self.logger.warning("RPC模式连接失败")
                if not self.fallback_to_traditional:
                    self._set_error("RPC模式连接失败且未启用回退机制")
                    return False
        
        # 回退到传统模式
        if TSMASTER_AVAILABLE:
            self.logger.info("尝试使用传统模式连接...")
            if self._connect_via_traditional():
                self._using_rpc = False
                self.status = AdapterStatus.CONNECTED
                self._clear_error()
                self.logger.info("TSMaster传统模式连接成功")
                return True
            else:
                self.logger.warning("传统模式连接失败")
        
        self._set_error("所有连接方式均失败")
        return False
    
    def _connect_via_rpc(self) -> bool:
        """通过RPC模式连接"""
        try:
            self._rpc_client = TSMasterRPCClient()
            
            # 初始化RPC客户端
            if not self._rpc_client.initialize():
                return False
            
            # 连接到TSMaster
            if not self._rpc_client.connect(self.rpc_app_name):
                return False
            
            return True
            
        except Exception as e:
            self.logger.error(f"RPC连接异常: {str(e)}")
            return False
    
    def _connect_via_traditional(self) -> bool:
        """通过传统模式连接"""
        try:
            self._ts = TSMaster()
            self._ts.connect()
            return True
        except Exception as e:
            self.logger.error(f"传统模式连接异常: {str(e)}")
            return False
    
    def disconnect(self) -> bool:
        """
        断开TSMaster连接
        
        Returns:
            断开成功返回True，否则返回False
        """
        try:
            # 停止总线（如果正在运行）
            if self.is_connected:
                self.stop_test()
            
            # 断开RPC连接
            if self._rpc_client:
                self._rpc_client.finalize()
                self._rpc_client = None
            
            # 断开传统连接
            if self._ts:
                self._ts.disconnect()
                self._ts = None
            
            self._using_rpc = False
            self._callbacks.clear()
            
            self.status = AdapterStatus.DISCONNECTED
            self.logger.info("TSMaster连接已断开")
            return True
            
        except Exception as e:
            self._set_error(f"TSMaster断开连接失败: {str(e)}")
            return False
    
    def load_configuration(self, config_path: str) -> bool:
        """
        加载TSMaster工程文件
        
        Args:
            config_path: 工程文件路径(.tproj)
            
        Returns:
            加载成功返回True，否则返回False
        """
        if not self.is_connected:
            self._set_error("TSMaster未连接，无法加载配置")
            return False
        
        try:
            self.logger.info(f"正在加载工程文件: {config_path}")
            self._ts.load_config(config_path)
            self.logger.info("工程文件加载成功")
            return True
            
        except Exception as e:
            self._set_error(f"工程文件加载失败: {str(e)}")
            return False
    
    def start_test(self) -> bool:
        """
        启动TSMaster总线仿真
        
        Returns:
            启动成功返回True，否则返回False
        """
        if not self.is_connected:
            self._set_error("TSMaster未连接，无法启动仿真")
            return False
        
        try:
            self.logger.info("正在启动总线仿真...")
            
            if self._using_rpc and self._rpc_client:
                # RPC模式
                success = self._rpc_client.start_simulation()
            elif self._ts:
                # 传统模式
                self._ts.start_bus()
                success = True
            else:
                return False
            
            if success:
                self.status = AdapterStatus.RUNNING
                self.logger.info("总线仿真已启动")
                return True
            else:
                self._set_error("启动总线仿真失败")
                return False
            
        except Exception as e:
            self._set_error(f"启动总线仿真失败: {str(e)}")
            return False
    
    def stop_test(self) -> bool:
        """
        停止TSMaster总线仿真
        
        Returns:
            停止成功返回True，否则返回False
        """
        if self.status not in [AdapterStatus.CONNECTED, AdapterStatus.RUNNING]:
            self._set_error("TSMaster未连接，无法停止仿真")
            return False
        
        try:
            self.logger.info("正在停止总线仿真...")
            
            if self._using_rpc and self._rpc_client:
                # RPC模式
                success = self._rpc_client.stop_simulation()
            elif self._ts:
                # 传统模式
                self._ts.stop_bus()
                success = True
            else:
                return False
            
            if success:
                self.status = AdapterStatus.CONNECTED
                self.logger.info("总线仿真已停止")
                return True
            else:
                self._set_error("停止总线仿真失败")
                return False
            
        except Exception as e:
            self._set_error(f"停止总线仿真失败: {str(e)}")
            return False
    
    def execute_test_item(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """
        执行单个测试项
        
        支持的测试项类型：
        - signal_check: 信号检查
        - signal_set: 信号设置
        - message_send: 发送报文
        - c_script: 执行C脚本
        - test_sequence: 执行测试序列
        - sysvar_check: 系统变量检查
        - sysvar_set: 系统变量设置
        
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
            elif item_type == "message_send":
                return self._execute_message_send(item)
            elif item_type == "c_script":
                return self._execute_c_script(item)
            elif item_type == "test_sequence":
                return self._execute_test_sequence(item)
            elif item_type == "sysvar_check":
                return self._execute_sysvar_check(item)
            elif item_type == "sysvar_set":
                return self._execute_sysvar_set(item)
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
        signal_name = item.get("signal_name")
        expected_value = item.get("expected_value")
        tolerance = item.get("tolerance", 0.01)
        
        if not signal_name:
            raise ValueError("信号检查需要指定signal_name参数")
        
        # 读取信号值
        actual_value = self._get_signal(signal_name)
        
        # 判断结果
        passed = False
        if actual_value is not None and expected_value is not None:
            passed = abs(actual_value - expected_value) < tolerance
        
        return {
            "name": item.get("name"),
            "type": "signal_check",
            "signal_name": signal_name,
            "expected_value": expected_value,
            "actual_value": actual_value,
            "tolerance": tolerance,
            "passed": passed,
            "status": "passed" if passed else "failed"
        }
    
    def _execute_signal_set(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行信号设置"""
        signal_name = item.get("signal_name")
        value = item.get("value")
        
        if not signal_name or value is None:
            raise ValueError("信号设置需要指定signal_name和value参数")
        
        # 设置信号值
        success = self._set_signal(signal_name, value)
        
        return {
            "name": item.get("name"),
            "type": "signal_set",
            "signal_name": signal_name,
            "value": value,
            "success": success,
            "status": "passed" if success else "failed"
        }
    
    def _execute_message_send(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行报文发送"""
        channel = item.get("channel", 0)
        msg_id = item.get("msg_id")
        data = item.get("data", [])
        
        if msg_id is None:
            raise ValueError("报文发送需要指定msg_id参数")
        
        # 发送报文
        success = self._send_message(channel, msg_id, data)
        
        return {
            "name": item.get("name"),
            "type": "message_send",
            "channel": channel,
            "msg_id": msg_id,
            "data": data,
            "success": success,
            "status": "passed" if success else "failed"
        }
    
    def _execute_c_script(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行C脚本"""
        script_name = item.get("script_name")
        function_name = item.get("function_name")
        params = item.get("params", [])
        
        if not script_name or not function_name:
            raise ValueError("C脚本执行需要指定script_name和function_name参数")
        
        # 调用C脚本函数
        result = self._call_c_script(script_name, function_name, params)
        
        return {
            "name": item.get("name"),
            "type": "c_script",
            "script_name": script_name,
            "function_name": function_name,
            "params": params,
            "result": result,
            "status": "passed" if result is not None else "failed"
        }
    
    def _execute_test_sequence(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行测试序列"""
        sequence_name = item.get("sequence_name")
        
        if not sequence_name:
            raise ValueError("测试序列执行需要指定sequence_name参数")
        
        # 执行测试序列
        result = self._ts.test_execute(sequence_name)
        
        return {
            "name": item.get("name"),
            "type": "test_sequence",
            "sequence_name": sequence_name,
            "result": result,
            "status": "passed" if result else "failed"
        }
    
    def _execute_sysvar_check(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行系统变量检查"""
        var_name = item.get("var_name")
        expected_value = item.get("expected_value")
        
        if not var_name:
            raise ValueError("系统变量检查需要指定var_name参数")
        
        # 读取系统变量值
        actual_value = self._read_system_var(var_name)
        
        # 判断结果
        passed = False
        if actual_value is not None and expected_value is not None:
            passed = str(actual_value) == str(expected_value)
        
        return {
            "name": item.get("name"),
            "type": "sysvar_check",
            "var_name": var_name,
            "expected_value": expected_value,
            "actual_value": actual_value,
            "passed": passed,
            "status": "passed" if passed else "failed"
        }
    
    def _execute_sysvar_set(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行系统变量设置"""
        var_name = item.get("var_name")
        value = item.get("value")
        
        if not var_name or value is None:
            raise ValueError("系统变量设置需要指定var_name和value参数")
        
        # 设置系统变量值
        success = self._write_system_var(var_name, str(value))
        
        return {
            "name": item.get("name"),
            "type": "sysvar_set",
            "var_name": var_name,
            "value": value,
            "success": success,
            "status": "passed" if success else "failed"
        }
    
    def _get_signal(self, signal_name: str) -> Optional[float]:
        """获取信号值"""
        try:
            if self._using_rpc and self._rpc_client:
                # RPC模式
                return self._rpc_client.get_can_signal(signal_name)
            elif self._ts:
                # 传统模式
                return self._ts.get_signal_value(signal_name)
            else:
                return None
        except Exception as e:
            self.logger.warning(f"获取信号失败: {str(e)}")
            return None
    
    def _set_signal(self, signal_name: str, value: float) -> bool:
        """设置信号值"""
        try:
            if self._using_rpc and self._rpc_client:
                # RPC模式
                return self._rpc_client.set_can_signal(signal_name, value)
            elif self._ts:
                # 传统模式
                self._ts.set_signal_value(signal_name, value)
                return True
            else:
                return False
        except Exception as e:
            self.logger.warning(f"设置信号失败: {str(e)}")
            return False
    
    def _send_message(self, channel: int, msg_id: int, data: list) -> bool:
        """发送CAN报文"""
        try:
            if self._using_rpc and self._rpc_client:
                # RPC模式
                return self._rpc_client.transmit_can_message(channel, msg_id, data)
            elif self._ts:
                # 传统模式
                self._ts.transmit_can_msg(channel, msg_id, data)
                return True
            else:
                return False
        except Exception as e:
            self.logger.warning(f"发送报文失败: {str(e)}")
            return False
    
    def _read_system_var(self, var_name: str) -> Optional[str]:
        """读取系统变量值"""
        try:
            if self._using_rpc and self._rpc_client:
                # RPC模式
                return self._rpc_client.read_system_var(var_name)
            elif self._ts:
                # 传统模式
                return self._ts.get_system_var(var_name)
            else:
                return None
        except Exception as e:
            self.logger.warning(f"读取系统变量失败: {str(e)}")
            return None
    
    def _write_system_var(self, var_name: str, value: str) -> bool:
        """写入系统变量值"""
        try:
            if self._using_rpc and self._rpc_client:
                # RPC模式
                return self._rpc_client.write_system_var(var_name, value)
            elif self._ts:
                # 传统模式
                self._ts.set_system_var(var_name, value)
                return True
            else:
                return False
        except Exception as e:
            self.logger.warning(f"写入系统变量失败: {str(e)}")
            return False
    
    def _call_c_script(self, script_name: str, function_name: str, params: list) -> Any:
        """调用C脚本函数"""
        try:
            # 加载C脚本
            self._ts.load_c_script(script_name)
            
            # 调用函数
            result = self._ts.call_c_function(function_name, *params)
            return result
        except Exception as e:
            self.logger.warning(f"调用C脚本失败: {str(e)}")
            return None
    
    def register_callback(self, event_name: str, callback: Callable):
        """
        注册回调函数
        
        Args:
            event_name: 事件名称
            callback: 回调函数
        """
        try:
            self._callbacks[event_name] = callback
            self._ts.register_callback(event_name, callback)
        except Exception as e:
            self.logger.warning(f"注册回调失败: {str(e)}")
    
    def unregister_callback(self, event_name: str):
        """
        注销回调函数
        
        Args:
            event_name: 事件名称
        """
        try:
            if event_name in self._callbacks:
                del self._callbacks[event_name]
                self._ts.unregister_callback(event_name)
        except Exception as e:
            self.logger.warning(f"注销回调失败: {str(e)}")
