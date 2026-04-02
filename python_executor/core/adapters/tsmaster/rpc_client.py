"""
TSMaster RPC客户端封装

基于TSMaster RPC API实现高性能的远程控制
"""

import logging
import time
import threading
from ctypes import c_double
from typing import Optional, List, Tuple, Callable, Dict, Any

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
    - 小程序管理
    - 测试序列执行
    """

    # 默认超时时间（秒）
    DEFAULT_CONNECT_TIMEOUT = 30
    DEFAULT_OPERATION_TIMEOUT = 60

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
        self._master_form_started = False
        self._monitoring_enabled = False  # 控制是否启用状态监控读取

        # 超时配置
        self.connect_timeout = self.DEFAULT_CONNECT_TIMEOUT
        self.operation_timeout = self.DEFAULT_OPERATION_TIMEOUT

        # 状态监控
        self._status_callback: Optional[Callable] = None
        self._monitor_thread: Optional[threading.Thread] = None
        self._stop_monitor = threading.Event()

        from utils.logger import get_logger
        self.logger = get_logger(f"adapters.tsmaster.TSMasterRPCClient")
        
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
    
    def connect(self, app_name: Optional[str] = None, timeout: Optional[int] = None) -> bool:
        """
        连接到TSMaster应用程序

        Args:
            app_name: 指定要连接的应用程序名称，如果为None则自动发现
            timeout: 连接超时时间（秒），默认使用实例配置

        Returns:
            连接成功返回True，否则返回False
        """
        if not self._initialized:
            if not self.initialize():
                return False

        timeout = timeout or self.connect_timeout
        start_time = time.time()

        try:
            self.logger.info("正在获取运行中的TSMaster应用程序列表...")

            # 等待应用启动（带超时重试）
            app_list = []
            while time.time() - start_time < timeout:
                namelist = pchar()
                ret = get_active_application_list(namelist)

                if ret == 0:
                    app_list = namelist.value.decode().split(';')
                    app_list = [app for app in app_list if app.strip()]

                if app_list:
                    break

                self.logger.debug(f"等待TSMaster应用启动... ({timeout - int(time.time() - start_time)}s remaining)")
                time.sleep(0.5)

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
            # 停止状态监控线程
            self._stop_monitor.set()
            if self._monitor_thread and self._monitor_thread.is_alive():
                self._monitor_thread.join(timeout=5)

            if self.connected:
                self.logger.info("正在断开RPC连接...")

                if self.simulation_running:
                    self.stop_simulation()

                ret = rpc_tsmaster_activate_client(self.rpchandle, False)

                if ret != 0:
                    self.logger.warning(f"停用RPC客户端失败，错误码: {ret}")

                self.connected = False
                self._monitoring_enabled = False
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
                self._monitoring_enabled = True  # 启用状态监控
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
                self._monitoring_enabled = False  # 禁用状态监控
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

    def write_var(self, var_name: str, value: str) -> bool:
        """
        写系统变量的便捷方法

        Args:
            var_name: 系统变量名称
            value: 变量值（字符串格式）

        Returns:
            写入成功返回True，否则返回False
        """
        return self.write_system_var(var_name, value)

    def read_var(self, var_name: str) -> Optional[str]:
        """
        读系统变量的便捷方法

        Args:
            var_name: 系统变量名称

        Returns:
            变量值（字符串格式），失败返回None
        """
        return self.read_system_var(var_name)

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
            value = c_double()
            ret = rpc_tsmaster_cmd_read_system_var(
                self.rpchandle,
                var_name.encode(),
                value
            )

            if ret == 0:
                result = str(value.value)
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

    def initialize_master(self, timeout: int = 5) -> bool:
        """
        初始化Master设备

        通过设置 Master.Init = 1 并等待系统变量变为 "1" 来完成初始化

        Args:
            timeout: 超时时间（秒）

        Returns:
            初始化成功返回True，否则返回False
        """
        self.logger.info("正在初始化Master设备...")
        if not self.write_system_var("Master.Init", "1"):
            self.logger.error("设置 Master.Init 失败")
            return False

        if self.wait_for_var("Master.Init", "1", timeout):
            self.logger.info("Master设备初始化成功")
            return True
        else:
            self.logger.error(f"Master设备初始化超时！Master.Init != 1")
            return False

    def enable_auto_report(self, enabled: bool = True) -> bool:
        """
        开启或关闭自动报告

        Args:
            enabled: 是否开启自动报告

        Returns:
            设置成功返回True，否则返回False
        """
        value = "1" if enabled else "0"
        self.logger.info(f"设置自动报告: {value}")
        return self.write_system_var("Master.AutoReport", value)

    def compile_test_cases(self, timeout: int = 10) -> bool:
        """
        强制编译测试用例（防呆关键步骤！）

        通过设置 TestSystem.GenCode = 1 触发编译，并等待编译完成

        Args:
            timeout: 超时时间（秒）

        Returns:
            编译成功返回True，否则返回False
        """
        self.logger.info("正在编译测试用例...")

        # 触发编译
        if not self.write_system_var("TestSystem.GenCode", "1"):
            self.logger.error("触发测试用例编译失败")
            return False

        # 等待编译完成
        start_time = time.time()
        while time.time() - start_time < timeout:
            status = self.read_system_var("TestSystem.GenCodeStatus")
            if status == "0":
                self.logger.info("测试用例编译成功")
                return True
            time.sleep(0.5)

        self.logger.error(f"测试用例编译超时！TestSystem.GenCodeStatus = {status}")
        return False

    def generate_report(self, timeout: int = 10) -> bool:
        """
        生成测试报告

        通过设置 Master.GenReport = 1 触发报告生成

        Args:
            timeout: 超时时间（秒）

        Returns:
            生成成功返回True，否则返回False
        """
        self.logger.info("正在生成测试报告...")

        if not self.write_system_var("Master.GenReport", "1"):
            self.logger.error("触发报告生成失败")
            return False

        # 等待报告生成完成
        time.sleep(1)

        # 检查报告路径是否有效（可选）
        start_time = time.time()
        while time.time() - start_time < timeout:
            report_path = self.read_system_var("Master.ReportPath")
            if report_path and report_path.strip():
                self.logger.info(f"测试报告已生成: {report_path}")
                return True
            time.sleep(0.5)

        self.logger.warning("报告生成完成但路径可能未更新")
        return True  # 报告生成命令已发送，不算失败
    
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
    
    def wait_for_var(self, var_name: str, expect: str, timeout: int = 10) -> bool:
        """
        轮询等待系统变量变成指定值

        Args:
            var_name: 系统变量名称
            expect: 期望值
            timeout: 超时时间（秒）

        Returns:
            等待成功返回True，超时返回False
        """
        import ctypes

        start = time.time()
        buf = ctypes.create_string_buffer(256)

        while time.time() - start < timeout:
            ret = rpc_tsmaster_cmd_read_system_var(self.rpchandle, var_name.encode(), buf)
            if ret == 0:
                val = buf.value.decode(errors="ignore")
                if val == expect:
                    self.logger.debug(f"系统变量 {var_name} 已变为 {expect}")
                    return True
            time.sleep(0.2)

        self.logger.warning(f"等待系统变量 {var_name} 变为 {expect} 超时")
        return False

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

    def is_test_finished(self) -> bool:
        """
        检查测试是否已完成

        通过读取 TestSystem.RunningStatus 变量的值来判断测试是否完成。
        当 TestSystem.RunningStatus 的值变为 "0" 时，表示测试已完成。

        Returns:
            测试完成返回True，否则返回False
        """
        if not self.connected:
            return False

        try:
            # 优先使用 RunningStatus 判断
            running_status = self.read_system_var("TestSystem.RunningStatus")
            if running_status is not None:
                return running_status == "0"
            # 回退到 Controller 判断
            controller_value = self.read_system_var("TestSystem.Controller")
            if controller_value is not None:
                return controller_value == "0"
            return False
        except Exception as e:
            self.logger.debug(f"检查测试状态异常: {str(e)}")
            return False

    def is_test_running(self) -> bool:
        """
        检查测试是否正在运行

        通过读取 TestSystem.RunningStatus 变量的值来判断测试是否正在运行。
        当 TestSystem.RunningStatus 的值变为 "1" 时，表示测试正在运行。

        Returns:
            测试正在运行返回True，否则返回False
        """
        if not self.connected:
            return False

        try:
            running_status = self.read_system_var("TestSystem.RunningStatus")
            if running_status is not None:
                return running_status == "1"
            return False
        except Exception as e:
            self.logger.debug(f"检查测试运行状态异常: {str(e)}")
            return False

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

    def load_project(self, project_path: str) -> bool:
        """
        加载TSMaster工程文件

        Args:
            project_path: 工程文件路径(.tproj)

        Returns:
            加载成功返回True，否则返回False
        """
        if not self.connected:
            self.logger.error("RPC客户端未连接，无法加载工程")
            return False

        try:
            self.logger.info(f"正在加载工程文件: {project_path}")
            success = self.call_system_api("app.load_project", [project_path])
            if success:
                self.logger.info("工程文件加载成功")
            else:
                self.logger.error(f"工程文件加载失败: {project_path}")
            return success
        except Exception as e:
            self.logger.error(f"加载工程文件异常: {str(e)}")
            return False

    def wait_for_simulation_start(self, timeout: Optional[int] = None) -> bool:
        """
        等待仿真启动

        Args:
            timeout: 超时时间（秒）

        Returns:
            仿真启动成功返回True，超时返回False
        """
        timeout = timeout or self.operation_timeout
        start_time = time.time()

        self.logger.info("等待仿真启动...")

        while time.time() - start_time < timeout:
            if self.simulation_running:
                self.logger.info("仿真已启动")
                return True
            time.sleep(0.1)

        self.logger.error(f"等待仿真启动超时（{timeout}秒）")
        return False

    def get_test_result(self, result_type: str = "xml") -> Optional[str]:
        """
        获取测试结果

        Args:
            result_type: 结果类型，"xml" 或 "html"

        Returns:
            结果文件路径，失败返回None
        """
        if not self.connected:
            self.logger.error("RPC客户端未连接，无法获取测试结果")
            return None

        try:
            result_var = f"TestSystem.Result_{result_type.upper()}"
            result = self.read_system_var(result_var)
            if result:
                self.logger.info(f"获取测试结果成功: {result}")
            return result
        except Exception as e:
            self.logger.error(f"获取测试结果异常: {str(e)}")
            return None

    def get_report_path(self) -> Optional[str]:
        """
        获取测试报告目录路径

        Returns:
            报告目录路径，失败返回None
        """
        if not self.connected:
            self.logger.error("RPC客户端未连接，无法获取报告路径")
            return None

        try:
            path = self.read_system_var("Master.ReportPath")
            if path:
                self.logger.info(f"获取报告路径成功: {path}")
            return path
        except Exception as e:
            self.logger.error(f"获取报告路径异常: {str(e)}")
            return None

    def get_testdata_path(self) -> Optional[str]:
        """
        获取测试数据目录路径

        Returns:
            测试数据目录路径，失败返回None
        """
        if not self.connected:
            self.logger.error("RPC客户端未连接，无法获取测试数据路径")
            return None

        try:
            path = self.read_system_var("Master.TestDataLogPath")
            if path:
                self.logger.info(f"获取测试数据路径成功: {path}")
            return path
        except Exception as e:
            self.logger.error(f"获取测试数据路径异常: {str(e)}")
            return None

    def get_test_case_count(self) -> Tuple[int, int]:
        """
        获取测试用例统计

        Returns:
            (通过数, 失败数)
        """
        try:
            passed = self.read_system_var("TestSystem.TestCasePassCount") or "0"
            failed = self.read_system_var("TestSystem.TestCaseFailCount") or "0"
            return int(passed), int(failed)
        except Exception as e:
            self.logger.warning(f"获取测试用例统计失败: {str(e)}")
            return 0, 0

    def set_status_callback(self, callback: Callable[[str, Dict[str, Any]], None]):
        """
        设置状态回调函数

        Args:
            callback: 回调函数，签名: callback(status: str, data: Dict)
        """
        self._status_callback = callback
        if callback and not self._monitor_thread:
            self._stop_monitor.clear()
            self._monitor_thread = threading.Thread(target=self._monitor_loop, daemon=True)
            self._monitor_thread.start()

    def _monitor_loop(self):
        """状态监控循环"""
        while not self._stop_monitor.is_set():
            try:
                # 只有在启用监控时才读取系统变量（仿真启动后）
                if self.connected and self._monitoring_enabled:
                    # 获取测试进度（仅在仿真运行时）
                    if self.simulation_running:
                        test_status = self.read_system_var("TestSystem.TestStatus")
                        if test_status and self._status_callback:
                            self._status_callback("test_status", {"status": test_status})
            except Exception as e:
                self.logger.debug(f"状态监控异常: {str(e)}")

            self._stop_monitor.wait(0.5)

    def reset_test_system(self) -> bool:
        """
        重置测试系统

        Returns:
            重置成功返回True，否则返回False
        """
        self.logger.info("重置测试系统...")
        try:
            # 停止当前测试
            self.stop_test()
            time.sleep(0.5)
            # 重置测试系统变量
            self.write_system_var("TestSystem.Controller", "0")
            self.write_system_var("TestSystem.SelectCases", "")
            self.write_system_var("TestSystem.Reset", "1")
            time.sleep(0.5)
            self.write_system_var("TestSystem.Reset", "0")
            self.logger.info("测试系统已重置")
            return True
        except Exception as e:
            self.logger.error(f"重置测试系统异常: {str(e)}")
            return False

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
