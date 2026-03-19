"""
TSMaster RPC客户端封装

基于TSMaster RPC API实现高性能的远程控制
"""

import logging
from typing import Optional, List, Tuple

try:
    from TSMasterAPI import *
    TSMASTER_RPC_AVAILABLE = True
except ImportError:
    TSMASTER_RPC_AVAILABLE = False
    logging.warning("TSMasterAPI未安装，RPC客户端将无法使用")


class TSMasterRPCClient:
    """
    TSMaster RPC客户端
    
    通过共享内存与TSMaster应用程序进行高性能通信
    
    功能：
    - 连接/断开TSMaster应用
    - 启动/停止仿真
    - 读写CAN/LIN/FlexRay信号
    - 读写系统变量
    - 发送报文
    """
    
    def __init__(self, app_name: str = "TSMasterTest"):
        """
        初始化RPC客户端
        
        Args:
            app_name: 应用程序名称，用于初始化TSMaster库
        """
        self.app_name = app_name
        self.rpchandle = size_t(0)
        self.connected = False
        self.simulation_running = False
        self._initialized = False
        
        self.logger = logging.getLogger(f"{__name__}.{self.__class__.__name__}")
        
    def initialize(self) -> bool:
        """
        初始化TSMaster库
        
        Returns:
            初始化成功返回True，否则返回False
        """
        if not TSMASTER_RPC_AVAILABLE:
            self.logger.error("TSMasterAPI未安装，无法初始化")
            return False
        
        try:
            if not self._initialized:
                self.logger.info(f"正在初始化TSMaster库: {self.app_name}")
                ret = initialize_lib_tsmaster(self.app_name.encode())
                
                if ret == 0:
                    self._initialized = True
                    self.logger.info("TSMaster库初始化成功")
                    return True
                else:
                    self.logger.error(f"TSMaster库初始化失败，错误码: {ret}")
                    return False
                    
        except Exception as e:
            self.logger.error(f"初始化TSMaster库时发生异常: {str(e)}")
            return False
    
    def connect(self, app_name: Optional[str] = None) -> bool:
        """
        连接到TSMaster应用程序
        
        Args:
            app_name: 指定要连接的应用程序名称，如果为None则自动发现
            
        Returns:
            连接成功返回True，否则返回False
        """
        if not self._initialized:
            if not self.initialize():
                return False
        
        try:
            self.logger.info("正在获取运行中的TSMaster应用程序列表...")
            
            namelist = pchar()
            ret = get_active_application_list(namelist)
            
            if ret != 0:
                self.logger.error(f"获取应用程序列表失败，错误码: {ret}")
                return False
            
            app_list = namelist.value.decode().split(';')
            app_list = [app for app in app_list if app.strip()]
            
            if not app_list:
                self.logger.error("未发现运行中的TSMaster应用程序")
                return False
            
            self.logger.info(f"发现运行中的应用程序: {app_list}")
            
            target_app = app_name if app_name else app_list[0]
            
            if target_app not in app_list:
                self.logger.error(f"指定的应用程序 '{target_app}' 未在运行")
                return False
            
            self.logger.info(f"正在连接到应用程序: {target_app}")
            
            ret = rpc_tsmaster_create_client(target_app.encode(), self.rpchandle)
            
            if ret != 0:
                self.logger.error(f"创建RPC客户端失败，错误码: {ret}")
                return False
            
            ret = rpc_tsmaster_activate_client(self.rpchandle, True)
            
            if ret != 0:
                self.logger.error(f"激活RPC客户端失败，错误码: {ret}")
                return False
            
            self.connected = True
            self.logger.info(f"RPC客户端连接成功: {target_app}")
            return True
            
        except Exception as e:
            self.logger.error(f"连接TSMaster时发生异常: {str(e)}")
            return False
    
    def disconnect(self) -> bool:
        """
        断开RPC连接
        
        Returns:
            断开成功返回True，否则返回False
        """
        try:
            if self.connected:
                self.logger.info("正在断开RPC连接...")
                
                if self.simulation_running:
                    self.stop_simulation()
                
                ret = rpc_tsmaster_activate_client(self.rpchandle, False)
                
                if ret != 0:
                    self.logger.warning(f"停用RPC客户端失败，错误码: {ret}")
                
                self.connected = False
                self.logger.info("RPC连接已断开")
            
            return True
            
        except Exception as e:
            self.logger.error(f"断开连接时发生异常: {str(e)}")
            return False
    
    def finalize(self) -> bool:
        """
        释放TSMaster库资源
        
        Returns:
            释放成功返回True，否则返回False
        """
        try:
            if self.connected:
                self.disconnect()
            
            if self._initialized:
                self.logger.info("正在释放TSMaster库资源...")
                ret = finalize_lib_tsmaster()
                
                if ret == 0:
                    self._initialized = False
                    self.logger.info("TSMaster库资源已释放")
                    return True
                else:
                    self.logger.warning(f"释放TSMaster库失败，错误码: {ret}")
                    return False
            
            return True
            
        except Exception as e:
            self.logger.error(f"释放资源时发生异常: {str(e)}")
            return False
    
    def start_simulation(self) -> bool:
        """
        启动总线仿真
        
        Returns:
            启动成功返回True，否则返回False
        """
        if not self.connected:
            self.logger.error("RPC客户端未连接，无法启动仿真")
            return False
        
        try:
            self.logger.info("正在启动总线仿真...")
            ret = rpc_tsmaster_cmd_start_simulation(self.rpchandle)
            
            if ret == 0:
                self.simulation_running = True
                self.logger.info("总线仿真已启动")
                return True
            else:
                self.logger.error(f"启动仿真失败，错误码: {ret}")
                return False
                
        except Exception as e:
            self.logger.error(f"启动仿真时发生异常: {str(e)}")
            return False
    
    def stop_simulation(self) -> bool:
        """
        停止总线仿真
        
        Returns:
            停止成功返回True，否则返回False
        """
        if not self.connected:
            self.logger.error("RPC客户端未连接，无法停止仿真")
            return False
        
        try:
            self.logger.info("正在停止总线仿真...")
            ret = rpc_tsmaster_cmd_stop_simulation(self.rpchandle)
            
            if ret == 0:
                self.simulation_running = False
                self.logger.info("总线仿真已停止")
                return True
            else:
                self.logger.error(f"停止仿真失败，错误码: {ret}")
                return False
                
        except Exception as e:
            self.logger.error(f"停止仿真时发生异常: {str(e)}")
            return False
    
    def set_can_signal(self, signal_path: str, value: float) -> bool:
        """
        设置CAN信号值
        
        Args:
            signal_path: 信号路径，格式为"通道/数据库/节点/报文/信号"或"信号名"
            value: 信号值
            
        Returns:
            设置成功返回True，否则返回False
        """
        if not self.connected:
            self.logger.error("RPC客户端未连接，无法设置信号")
            return False
        
        try:
            ret = rpc_tsmaster_cmd_set_can_signal(
                self.rpchandle,
                signal_path.encode(),
                value
            )
            
            if ret == 0:
                self.logger.debug(f"设置信号成功: {signal_path} = {value}")
                return True
            else:
                self.logger.warning(f"设置信号失败: {signal_path}, 错误码: {ret}")
                return False
                
        except Exception as e:
            self.logger.error(f"设置信号时发生异常: {str(e)}")
            return False
    
    def get_can_signal(self, signal_path: str) -> Optional[float]:
        """
        获取CAN信号值
        
        Args:
            signal_path: 信号路径，格式为"通道/数据库/节点/报文/信号"或"信号名"
            
        Returns:
            信号值，失败返回None
        """
        if not self.connected:
            self.logger.error("RPC客户端未连接，无法获取信号")
            return None
        
        try:
            d = double(0)
            ret = rpc_tsmaster_cmd_get_can_signal(
                self.rpchandle,
                signal_path.encode(),
                d
            )
            
            if ret == 0:
                self.logger.debug(f"获取信号成功: {signal_path} = {d.value}")
                return d.value
            else:
                self.logger.warning(f"获取信号失败: {signal_path}, 错误码: {ret}")
                return None
                
        except Exception as e:
            self.logger.error(f"获取信号时发生异常: {str(e)}")
            return None
    
    def set_lin_signal(self, signal_path: str, value: float) -> bool:
        """
        设置LIN信号值
        
        Args:
            signal_path: 信号路径
            value: 信号值
            
        Returns:
            设置成功返回True，否则返回False
        """
        if not self.connected:
            self.logger.error("RPC客户端未连接，无法设置LIN信号")
            return False
        
        try:
            ret = rpc_tsmaster_cmd_set_lin_signal(
                self.rpchandle,
                signal_path.encode(),
                value
            )
            
            if ret == 0:
                self.logger.debug(f"设置LIN信号成功: {signal_path} = {value}")
                return True
            else:
                self.logger.warning(f"设置LIN信号失败: {signal_path}, 错误码: {ret}")
                return False
                
        except Exception as e:
            self.logger.error(f"设置LIN信号时发生异常: {str(e)}")
            return False
    
    def get_lin_signal(self, signal_path: str) -> Optional[float]:
        """
        获取LIN信号值
        
        Args:
            signal_path: 信号路径
            
        Returns:
            信号值，失败返回None
        """
        if not self.connected:
            self.logger.error("RPC客户端未连接，无法获取LIN信号")
            return None
        
        try:
            d = double(0)
            ret = rpc_tsmaster_cmd_get_lin_signal(
                self.rpchandle,
                signal_path.encode(),
                d
            )
            
            if ret == 0:
                self.logger.debug(f"获取LIN信号成功: {signal_path} = {d.value}")
                return d.value
            else:
                self.logger.warning(f"获取LIN信号失败: {signal_path}, 错误码: {ret}")
                return None
                
        except Exception as e:
            self.logger.error(f"获取LIN信号时发生异常: {str(e)}")
            return None
    
    def write_system_var(self, var_name: str, value: str) -> bool:
        """
        写系统变量
        
        Args:
            var_name: 系统变量名称
            value: 变量值（字符串格式）
            
        Returns:
            写入成功返回True，否则返回False
        """
        if not self.connected:
            self.logger.error("RPC客户端未连接，无法写系统变量")
            return False
        
        try:
            ret = rpc_tsmaster_cmd_write_system_var(
                self.rpchandle,
                var_name.encode(),
                value.encode()
            )
            
            if ret == 0:
                self.logger.debug(f"写系统变量成功: {var_name} = {value}")
                return True
            else:
                self.logger.warning(f"写系统变量失败: {var_name}, 错误码: {ret}")
                return False
                
        except Exception as e:
            self.logger.error(f"写系统变量时发生异常: {str(e)}")
            return False
    
    def read_system_var(self, var_name: str) -> Optional[str]:
        """
        读系统变量
        
        Args:
            var_name: 系统变量名称
            
        Returns:
            变量值（字符串格式），失败返回None
        """
        if not self.connected:
            self.logger.error("RPC客户端未连接，无法读系统变量")
            return None
        
        try:
            value = pchar()
            ret = rpc_tsmaster_cmd_read_system_var(
                self.rpchandle,
                var_name.encode(),
                value
            )
            
            if ret == 0:
                result = value.value.decode()
                self.logger.debug(f"读系统变量成功: {var_name} = {result}")
                return result
            else:
                self.logger.warning(f"读系统变量失败: {var_name}, 错误码: {ret}")
                return None
                
        except Exception as e:
            self.logger.error(f"读系统变量时发生异常: {str(e)}")
            return None
    
    def transmit_can_message(self, channel: int, msg_id: int, data: List[int], 
                             is_extended: bool = False, is_fd: bool = False) -> bool:
        """
        发送CAN报文
        
        Args:
            channel: 通道号
            msg_id: 报文ID
            data: 数据字节列表
            is_extended: 是否为扩展帧
            is_fd: 是否为CAN FD帧
            
        Returns:
            发送成功返回True，否则返回False
        """
        if not self.connected:
            self.logger.error("RPC客户端未连接，无法发送CAN报文")
            return False
        
        try:
            data_bytes = bytes(data)
            ret = rpc_tsmaster_cmd_transmit_can(
                self.rpchandle,
                channel,
                msg_id,
                data_bytes,
                is_extended,
                is_fd
            )
            
            if ret == 0:
                self.logger.debug(f"发送CAN报文成功: 通道{channel}, ID=0x{msg_id:X}")
                return True
            else:
                self.logger.warning(f"发送CAN报文失败, 错误码: {ret}")
                return False
                
        except Exception as e:
            self.logger.error(f"发送CAN报文时发生异常: {str(e)}")
            return False
    
    def call_system_api(self, api_name: str, args: List[str] = None, 
                        buffer_size: int = 1024, encoding: str = 'gbk') -> bool:
        """
        调用TSMaster系统API
        
        支持调用TSMaster的系统级API，如启动/停止小程序等。
        
        Args:
            api_name: API名称，如 "app.run_form", "app.stop_form"
            args: 参数列表，默认为空列表
            buffer_size: 缓冲区大小，默认1024
            encoding: 参数编码方式，默认'gbk'（Windows中文环境）
            
        Returns:
            调用成功返回True，否则返回False
            
        Examples:
            >>> client.call_system_api("app.run_form", ["C 代码编辑器 [Master]"])
            >>> client.call_system_api("app.stop_form", ["C 代码编辑器 [Master]"])
        """
        if not self.connected:
            self.logger.error("RPC客户端未连接，无法调用系统API")
            return False
        
        if args is None:
            args = []
        
        try:
            import ctypes
            
            arg_count = len(args)
            
            if arg_count == 0:
                ret = rpc_tsmaster_call_system_api(
                    self.rpchandle,
                    api_name.encode(),
                    0,
                    buffer_size,
                    None
                )
            else:
                args_array_type = ctypes.c_char_p * arg_count
                encoded_args = [arg.encode(encoding) for arg in args]
                args_array = args_array_type(*encoded_args)
                
                ret = rpc_tsmaster_call_system_api(
                    self.rpchandle,
                    api_name.encode(),
                    arg_count,
                    buffer_size,
                    args_array
                )
            
            if ret == 0:
                self.logger.debug(f"调用系统API成功: {api_name}, 参数: {args}")
                return True
            else:
                self.logger.warning(f"调用系统API失败: {api_name}, 错误码: {ret}")
                return False
                
        except Exception as e:
            self.logger.error(f"调用系统API异常: {api_name}, {str(e)}")
            return False
    
    def run_form(self, form_name: str, encoding: str = 'gbk') -> bool:
        """
        启动TSMaster小程序
        
        Args:
            form_name: 小程序名称，如 "C 代码编辑器 [Master]"
            encoding: 编码方式，默认'gbk'
            
        Returns:
            启动成功返回True，否则返回False
        """
        self.logger.info(f"正在启动小程序: {form_name}")
        return self.call_system_api("app.run_form", [form_name], encoding=encoding)
    
    def stop_form(self, form_name: str, encoding: str = 'gbk') -> bool:
        """
        停止TSMaster小程序
        
        Args:
            form_name: 小程序名称，如 "C 代码编辑器 [Master]"
            encoding: 编码方式，默认'gbk'
            
        Returns:
            停止成功返回True，否则返回False
        """
        self.logger.info(f"正在停止小程序: {form_name}")
        return self.call_system_api("app.stop_form", [form_name], encoding=encoding)
    
    def select_test_cases(self, cases: str) -> bool:
        """
        选择测试用例
        
        Args:
            cases: 测试用例字符串，格式如 "TG1_TC1=1,TG1_TC2=1"
            
        Returns:
            设置成功返回True，否则返回False
        """
        self.logger.info(f"选择测试用例: {cases}")
        return self.write_system_var("TestSystem.SelectCases", cases)
    
    def start_test(self) -> bool:
        """
        开始测试
        
        通过设置系统变量 TestSystem.Controller = 1 启动测试
        
        Returns:
            设置成功返回True，否则返回False
        """
        self.logger.info("开始测试")
        return self.write_system_var("TestSystem.Controller", "1")
    
    def stop_test(self) -> bool:
        """
        停止测试
        
        通过设置系统变量 TestSystem.Controller = 0 停止测试
        
        Returns:
            设置成功返回True，否则返回False
        """
        self.logger.info("停止测试")
        return self.write_system_var("TestSystem.Controller", "0")
    
    def execute_test_sequence(self, sequence_name: str) -> Optional[str]:
        """
        执行测试序列
        
        通过调用系统API执行指定的测试序列
        
        Args:
            sequence_name: 测试序列名称
            
        Returns:
            执行结果，失败返回None
        """
        if not self.connected:
            self.logger.error("RPC客户端未连接，无法执行测试序列")
            return None
        
        try:
            self.logger.info(f"执行测试序列: {sequence_name}")
            
            success = self.call_system_api("test.run_sequence", [sequence_name])
            
            if success:
                return sequence_name
            else:
                self.logger.warning(f"执行测试序列失败: {sequence_name}")
                return None
                
        except Exception as e:
            self.logger.error(f"执行测试序列异常: {str(e)}")
            return None
    
    def get_active_applications(self) -> List[str]:
        """
        获取运行中的TSMaster应用程序列表
        
        Returns:
            应用程序名称列表
        """
        if not self._initialized:
            if not self.initialize():
                return []
        
        try:
            namelist = pchar()
            ret = get_active_application_list(namelist)
            
            if ret != 0:
                self.logger.error(f"获取应用程序列表失败，错误码: {ret}")
                return []
            
            app_list = namelist.value.decode().split(';')
            return [app for app in app_list if app.strip()]
            
        except Exception as e:
            self.logger.error(f"获取应用程序列表时发生异常: {str(e)}")
            return []
    
    @property
    def is_connected(self) -> bool:
        """检查是否已连接"""
        return self.connected
    
    @property
    def is_simulation_running(self) -> bool:
        """检查仿真是否正在运行"""
        return self.simulation_running
    
    def __enter__(self):
        """上下文管理器入口"""
        self.initialize()
        self.connect()
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        """上下文管理器出口"""
        self.finalize()
