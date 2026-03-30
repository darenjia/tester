"""
TSMaster测试工具适配器

基于TSMaster Python API和RPC实现自动化控制
支持两种通信方式：
1. RPC模式（推荐）：基于共享内存的高性能通信
2. 传统模式：基于TSMaster Python API
"""

import time
import logging
from typing import Optional, Dict, Any, Callable, List

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
    - Master小程序管理
    - 测试序列执行
    """

    # 默认超时配置
    DEFAULT_START_TIMEOUT = 30
    DEFAULT_STOP_TIMEOUT = 10
    DEFAULT_OPERATION_TIMEOUT = 60

    def __init__(self, config: dict = None):
        """
        初始化TSMaster适配器

        Args:
            config: 配置字典，可包含：
                - start_timeout: 启动超时时间（默认30秒）
                - stop_timeout: 停止超时时间（默认10秒）
                - operation_timeout: 操作超时时间（默认60秒）
                - use_rpc: 是否使用RPC模式（默认True）
                - rpc_app_name: RPC连接的应用程序名称（默认自动发现）
                - fallback_to_traditional: RPC失败时是否回退到传统模式（默认True）
                - master_form_name: Master小程序名称（默认"C 代码编辑器 [Master]"）
                - auto_start_master: 是否在启动仿真前自动启动Master小程序（默认True）
                - auto_stop_master: 是否在停止仿真后自动停止Master小程序（默认True）
        """
        super().__init__(config)
        self.start_timeout = self.config.get("start_timeout", self.DEFAULT_START_TIMEOUT)
        self.stop_timeout = self.config.get("stop_timeout", self.DEFAULT_STOP_TIMEOUT)
        self.operation_timeout = self.config.get("operation_timeout", self.DEFAULT_OPERATION_TIMEOUT)
        self.use_rpc = self.config.get("use_rpc", True)
        self.rpc_app_name = self.config.get("rpc_app_name", None)
        self.fallback_to_traditional = self.config.get("fallback_to_traditional", True)
        self.master_form_name = self.config.get("master_form_name", "C 代码编辑器 [Master]")
        self.auto_start_master = self.config.get("auto_start_master", True)
        self.auto_stop_master = self.config.get("auto_stop_master", True)

        self._ts = None
        self._rpc_client: Optional[TSMasterRPCClient] = None
        self._using_rpc = False
        self._callbacks: Dict[str, Callable] = {}
        self._master_form_started = False
        self._current_project: Optional[str] = None

        # 测试执行统计
        self._test_stats = {"passed": 0, "failed": 0, "total": 0}
        
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

            # 设置超时
            self._rpc_client.connect_timeout = self.start_timeout
            self._rpc_client.operation_timeout = self.operation_timeout

            # 连接到TSMaster（带超时）
            if not self._rpc_client.connect(self.rpc_app_name, timeout=self.start_timeout):
                return False

            # 设置状态监控回调
            self._rpc_client.set_status_callback(self._on_status_change)

            return True

        except Exception as e:
            self.logger.error(f"RPC连接异常: {str(e)}")
            return False

    def _on_status_change(self, status: str, data: Dict[str, Any]):
        """RPC状态变化回调"""
        self.logger.debug(f"RPC状态变化: {status} -> {data}")
        if status == "simulation_status":
            sim_status = data.get("status", "")
            if sim_status == "running" and not self._rpc_client.simulation_running:
                self.logger.info("检测到仿真启动")
                self._rpc_client.simulation_running = True
    
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

            # 停止Master小程序
            if self._master_form_started:
                self.stop_master_form(self.master_form_name)

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
            self._current_project = None
            self._test_stats = {"passed": 0, "failed": 0, "total": 0}

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

            if self._using_rpc and self._rpc_client:
                # RPC模式：使用RPC客户端加载工程
                success = self._rpc_client.load_project(config_path)
            elif self._ts:
                # 传统模式
                self._ts.load_config(config_path)
                success = True
            else:
                self._set_error("未连接到TSMaster")
                return False

            if success:
                self._current_project = config_path
                self.logger.info("工程文件加载成功")
            else:
                self._set_error("工程文件加载失败")

            return success

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

            # RPC模式：支持Master小程序自动管理
            if self._using_rpc and self._rpc_client:
                # 自动启动Master小程序
                if self.auto_start_master and not self._master_form_started:
                    self.logger.info(f"自动启动Master小程序: {self.master_form_name}")
                    if not self.start_master_form(self.master_form_name):
                        self.logger.warning(f"Master小程序启动失败，继续执行...")

                success = self._rpc_client.start_simulation()
                if success:
                    # 等待仿真真正启动
                    if not self._rpc_client.wait_for_simulation_start(timeout=self.start_timeout):
                        self.logger.warning("等待仿真启动超时")

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

                # 自动停止Master小程序
                if self.auto_stop_master and self._master_form_started:
                    self.logger.info(f"自动停止Master小程序: {self.master_form_name}")
                    self.stop_master_form(self.master_form_name)

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

    def start_master_form(self, form_name: str = None) -> bool:
        """
        启动Master小程序

        按照RPC调用流程，在启动仿真前需要先启动Master小程序

        Args:
            form_name: 小程序名称，默认为配置中的master_form_name

        Returns:
            启动成功返回True，否则返回False
        """
        if not self.is_connected:
            self._set_error("TSMaster未连接，无法启动Master小程序")
            return False

        if form_name is None:
            form_name = self.master_form_name

        # 仅RPC模式支持
        if not self._using_rpc or not self._rpc_client:
            self._set_error("启动Master小程序仅支持RPC模式")
            return False

        try:
            self.logger.info(f"正在启动Master小程序: {form_name}")
            success = self._rpc_client.run_form(form_name)

            if success:
                self._master_form_started = True
                self.logger.info(f"Master小程序已启动: {form_name}")
            else:
                self.logger.warning(f"Master小程序启动失败: {form_name}")

            return success

        except Exception as e:
            self._set_error(f"启动Master小程序异常: {str(e)}")
            return False

    def stop_master_form(self, form_name: str = None) -> bool:
        """
        停止Master小程序

        按照RPC调用流程，在停止仿真后需要停止Master小程序

        Args:
            form_name: 小程序名称，默认为配置中的master_form_name

        Returns:
            停止成功返回True，否则返回False
        """
        if not self._master_form_started:
            self.logger.info("Master小程序未启动，无需停止")
            return True

        if form_name is None:
            form_name = self.master_form_name

        # 仅RPC模式支持
        if not self._using_rpc or not self._rpc_client:
            self._set_error("停止Master小程序仅支持RPC模式")
            return False

        try:
            self.logger.info(f"正在停止Master小程序: {form_name}")
            success = self._rpc_client.stop_form(form_name)

            if success:
                self._master_form_started = False
                self.logger.info(f"Master小程序已停止: {form_name}")
            else:
                self.logger.warning(f"Master小程序停止失败: {form_name}")

            return success

        except Exception as e:
            self._set_error(f"停止Master小程序异常: {str(e)}")
            return False

    def reset_test_system(self) -> bool:
        """
        重置测试系统

        Returns:
            重置成功返回True，否则返回False
        """
        if not self.is_connected:
            self._set_error("TSMaster未连接，无法重置测试系统")
            return False

        try:
            self.logger.info("正在重置测试系统...")

            if self._using_rpc and self._rpc_client:
                success = self._rpc_client.reset_test_system()
            elif self._ts:
                # 传统模式：停止测试
                if self.is_running:
                    self._ts.stop_bus()
                success = True
            else:
                return False

            if success:
                self._test_stats = {"passed": 0, "failed": 0, "total": 0}
                self.logger.info("测试系统已重置")

            return success

        except Exception as e:
            self._set_error(f"重置测试系统失败: {str(e)}")
            return False

    def start_test_execution(self, test_cases: Optional[str] = None,
                            wait_for_complete: bool = True,
                            timeout: Optional[int] = None) -> bool:
        """
        开始测试执行

        Args:
            test_cases: 测试用例选择字符串，格式如 "TG1_TC1=1,TG1_TC2=1"
            wait_for_complete: 是否等待测试完成
            timeout: 执行超时时间（秒）

        Returns:
            启动成功返回True，否则返回False
        """
        if not self.is_connected:
            self._set_error("TSMaster未连接，无法执行测试")
            return False

        timeout = timeout or self.operation_timeout

        try:
            self.logger.info("开始测试执行...")

            # 选择测试用例
            if test_cases:
                if self._using_rpc and self._rpc_client:
                    self._rpc_client.select_test_cases(test_cases)
                else:
                    self._write_system_var("TestSystem.SelectCases", test_cases)

            # 启动测试
            if self._using_rpc and self._rpc_client:
                success = self._rpc_client.start_test()
            else:
                success = self._write_system_var("TestSystem.Controller", "1")

            if not success:
                self._set_error("启动测试失败")
                return False

            if wait_for_complete:
                self.logger.info(f"等待测试完成（超时: {timeout}秒）...")
                if not self._wait_for_test_complete(timeout):
                    self.logger.warning("等待测试完成超时")

            return True

        except Exception as e:
            self._set_error(f"测试执行启动失败: {str(e)}")
            return False

    def _wait_for_test_complete(self, timeout: int) -> bool:
        """
        等待测试完成

        Args:
            timeout: 超时时间（秒）

        Returns:
            测试完成返回True，超时返回False
        """
        start_time = time.time()

        while time.time() - start_time < timeout:
            try:
                if self._using_rpc and self._rpc_client:
                    controller = self._rpc_client.read_system_var("TestSystem.Controller")
                    if controller == "0":
                        self.logger.info("测试执行完成")
                        return True
                else:
                    controller = self._read_system_var("TestSystem.Controller")
                    if controller == "0":
                        self.logger.info("测试执行完成")
                        return True
            except Exception as e:
                self.logger.debug(f"检查测试状态异常: {str(e)}")

            time.sleep(0.5)

        return False

    def wait_for_test_complete(self, timeout: Optional[int] = None) -> bool:
        """
        Wait for test to complete by polling is_test_finished()

        Args:
            timeout: Timeout in seconds. Defaults to operation_timeout.

        Returns:
            True if test finished, False if timeout
        """
        if not self.is_connected:
            self._set_error("TSMaster未连接，无法等待测试完成")
            return False

        timeout = timeout or self.operation_timeout
        start_time = time.time()

        self.logger.info(f"等待测试完成（超时: {timeout}秒）...")

        while time.time() - start_time < timeout:
            try:
                if self._using_rpc and self._rpc_client:
                    if self._rpc_client.read_system_var("TestSystem.Controller") == "0":
                        self.logger.info("测试执行完成")
                        return True
                elif self._ts:
                    controller = self._read_system_var("TestSystem.Controller")
                    if controller == "0":
                        self.logger.info("测试执行完成")
                        return True
            except Exception as e:
                self.logger.debug(f"检查测试状态异常: {str(e)}")

            time.sleep(0.5)

        self.logger.warning(f"等待测试完成超时（{timeout}秒）")
        return False

    def stop_test_execution(self) -> bool:
        """
        停止测试执行

        Returns:
            停止成功返回True，否则返回False
        """
        try:
            self.logger.info("正在停止测试执行...")

            if self._using_rpc and self._rpc_client:
                success = self._rpc_client.stop_test()
            else:
                success = self._write_system_var("TestSystem.Controller", "0")

            if success:
                self.logger.info("测试执行已停止")

            return success

        except Exception as e:
            self._set_error(f"停止测试执行失败: {str(e)}")
            return False

    def get_test_results(self, result_type: str = "xml") -> Optional[Dict[str, Any]]:
        """
        获取测试结果

        Args:
            result_type: 结果类型，"xml" 或 "html"

        Returns:
            结果信息字典，失败返回None
        """
        if not self.is_connected:
            self._set_error("TSMaster未连接，无法获取测试结果")
            return None

        try:
            self.logger.info(f"获取测试结果（类型: {result_type}）")

            if self._using_rpc and self._rpc_client:
                result_path = self._rpc_client.get_test_result(result_type)
                passed, failed = self._rpc_client.get_test_case_count()
            else:
                result_path = self._read_system_var(f"TestSystem.Result_{result_type.upper()}")
                passed_str = self._read_system_var("TestSystem.TestCasePassCount") or "0"
                failed_str = self._read_system_var("TestSystem.TestCaseFailCount") or "0"
                passed, failed = int(passed_str), int(failed_str)

            self._test_stats = {"passed": passed, "failed": failed, "total": passed + failed}

            return {
                "result_path": result_path,
                "passed": passed,
                "failed": failed,
                "total": passed + failed,
                "success_rate": (passed / (passed + failed) * 100) if (passed + failed) > 0 else 0
            }

        except Exception as e:
            self._set_error(f"获取测试结果失败: {str(e)}")
            return None

    def get_test_report_info(self) -> Optional[Dict[str, Any]]:
        """
        Get test report paths and statistics after execution

        Returns:
            Dict with report_path, testdata_path, passed, failed, total
            Returns None if not connected or error
        """
        if not self.is_connected:
            self._set_error("TSMaster未连接，无法获取测试报告信息")
            return None

        try:
            self.logger.info("获取测试报告信息")

            if self._using_rpc and self._rpc_client:
                report_path = self._rpc_client.get_report_path()
                testdata_path = self._rpc_client.get_testdata_path()
                passed, failed = self._rpc_client.get_test_case_count()
            else:
                report_path = self._read_system_var("Master.ReportPath")
                testdata_path = self._read_system_var("Master.TestDataLogPath")
                passed_str = self._read_system_var("TestSystem.TestCasePassCount") or "0"
                failed_str = self._read_system_var("TestSystem.TestCaseFailCount") or "0"
                passed, failed = int(passed_str), int(failed_str)

            self._test_stats = {"passed": passed, "failed": failed, "total": passed + failed}

            result = {
                "report_path": report_path,
                "testdata_path": testdata_path,
                "passed": passed,
                "failed": failed,
                "total": passed + failed,
                "success_rate": (passed / (passed + failed) * 100) if (passed + failed) > 0 else 0
            }

            self.logger.info(f"测试报告信息: passed={passed}, failed={failed}, total={passed+failed}")
            return result

        except Exception as e:
            self._set_error(f"获取测试报告信息失败: {str(e)}")
            return None

    def get_test_statistics(self) -> Dict[str, int]:
        """
        获取测试统计信息

        Returns:
            测试统计字典
        """
        if self._using_rpc and self._rpc_client:
            try:
                passed, failed = self._rpc_client.get_test_case_count()
                self._test_stats = {"passed": passed, "failed": failed, "total": passed + failed}
            except Exception:
                pass

        return self._test_stats.copy()

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
        - system_api: 调用系统API
        - run_form: 启动小程序
        - stop_form: 停止小程序
        - select_test_cases: 选择测试用例
        
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
            elif item_type == "test_module":
                return self._execute_test_module(item)
            elif item_type == "diagnostic":
                return self._execute_diagnostic(item)
            elif item_type == "wait":
                return self._execute_wait(item)
            elif item_type == "condition":
                return self._execute_condition(item)
            elif item_type == "system_api":
                return self._execute_system_api(item)
            elif item_type == "run_form":
                return self._execute_run_form(item)
            elif item_type == "stop_form":
                return self._execute_stop_form(item)
            elif item_type == "select_test_cases":
                return self._execute_select_test_cases(item)
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
        
        try:
            self.logger.info(f"检查信号: {signal_name}")
            
            actual_value = self._get_signal(signal_name)
            
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
            
        except Exception as e:
            self.logger.error(f"信号检查失败: {str(e)}")
            return {
                "name": item.get("name"),
                "type": "signal_check",
                "signal_name": signal_name,
                "status": "error",
                "error": str(e)
            }
    
    def _execute_signal_set(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行信号设置"""
        signal_name = item.get("signal_name")
        value = item.get("value")
        
        if not signal_name or value is None:
            raise ValueError("信号设置需要指定signal_name和value参数")
        
        try:
            self.logger.info(f"设置信号: {signal_name} = {value}")
            
            success = self._set_signal(signal_name, value)
            
            return {
                "name": item.get("name"),
                "type": "signal_set",
                "signal_name": signal_name,
                "value": value,
                "success": success,
                "status": "passed" if success else "failed"
            }
            
        except Exception as e:
            self.logger.error(f"信号设置失败: {str(e)}")
            return {
                "name": item.get("name"),
                "type": "signal_set",
                "signal_name": signal_name,
                "value": value,
                "status": "error",
                "error": str(e)
            }
    
    def _execute_message_send(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行报文发送"""
        channel = item.get("channel", 0)
        msg_id = item.get("msg_id")
        data = item.get("data", [])
        
        if msg_id is None:
            raise ValueError("报文发送需要指定msg_id参数")
        
        try:
            self.logger.info(f"发送报文: 通道{channel}, ID=0x{msg_id:X}")
            
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
            
        except Exception as e:
            self.logger.error(f"报文发送失败: {str(e)}")
            return {
                "name": item.get("name"),
                "type": "message_send",
                "channel": channel,
                "msg_id": msg_id,
                "status": "error",
                "error": str(e)
            }
    
    def _execute_c_script(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行C脚本"""
        script_name = item.get("script_name")
        function_name = item.get("function_name")
        params = item.get("params", [])
        
        if not script_name or not function_name:
            raise ValueError("C脚本执行需要指定script_name和function_name参数")
        
        try:
            self.logger.info(f"执行C脚本: {script_name}.{function_name}")
            
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
            
        except Exception as e:
            self.logger.error(f"执行C脚本失败: {str(e)}")
            return {
                "name": item.get("name"),
                "type": "c_script",
                "script_name": script_name,
                "function_name": function_name,
                "status": "error",
                "error": str(e)
            }
    
    def _execute_test_sequence(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行测试序列"""
        sequence_name = item.get("sequence_name")
        timeout = item.get("timeout", self.operation_timeout)
        wait_complete = item.get("wait_for_complete", True)

        if not sequence_name:
            raise ValueError("测试序列执行需要指定sequence_name参数")

        try:
            self.logger.info(f"执行测试序列: {sequence_name}")

            if self._using_rpc and self._rpc_client:
                result = self._rpc_client.execute_test_sequence(sequence_name)
                success = result is not None

                # 等待测试完成
                if success and wait_complete:
                    if not self._wait_for_test_complete(timeout):
                        self.logger.warning(f"等待测试序列完成超时: {sequence_name}")
            elif self._ts:
                result = self._ts.test_execute(sequence_name)
                success = result
            else:
                return {
                    "name": item.get("name"),
                    "type": "test_sequence",
                    "sequence_name": sequence_name,
                    "status": "error",
                    "error": "未连接到TSMaster"
                }

            # 获取测试统计
            stats = self.get_test_statistics()

            return {
                "name": item.get("name"),
                "type": "test_sequence",
                "sequence_name": sequence_name,
                "result": result,
                "passed": stats.get("passed", 0),
                "failed": stats.get("failed", 0),
                "total": stats.get("total", 0),
                "status": "passed" if success else "failed"
            }

        except Exception as e:
            self.logger.error(f"执行测试序列失败: {str(e)}")
            return {
                "name": item.get("name"),
                "type": "test_sequence",
                "sequence_name": sequence_name,
                "status": "error",
                "error": str(e)
            }
    
    def _execute_sysvar_check(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行系统变量检查"""
        var_name = item.get("var_name")
        expected_value = item.get("expected_value")
        
        if not var_name:
            raise ValueError("系统变量检查需要指定var_name参数")
        
        try:
            self.logger.info(f"检查系统变量: {var_name}")
            
            actual_value = self._read_system_var(var_name)
            
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
            
        except Exception as e:
            self.logger.error(f"系统变量检查失败: {str(e)}")
            return {
                "name": item.get("name"),
                "type": "sysvar_check",
                "var_name": var_name,
                "status": "error",
                "error": str(e)
            }
    
    def _execute_sysvar_set(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行系统变量设置"""
        var_name = item.get("var_name")
        value = item.get("value")
        
        if not var_name or value is None:
            raise ValueError("系统变量设置需要指定var_name和value参数")
        
        try:
            self.logger.info(f"设置系统变量: {var_name} = {value}")
            
            success = self._write_system_var(var_name, str(value))
            
            return {
                "name": item.get("name"),
                "type": "sysvar_set",
                "var_name": var_name,
                "value": value,
                "success": success,
                "status": "passed" if success else "failed"
            }
            
        except Exception as e:
            self.logger.error(f"系统变量设置失败: {str(e)}")
            return {
                "name": item.get("name"),
                "type": "sysvar_set",
                "var_name": var_name,
                "value": value,
                "status": "error",
                "error": str(e)
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
            if self._using_rpc and self._rpc_client:
                # RPC模式：通过RPC调用C脚本（如果TSMasterAPI支持）
                # 注意：RPC模式可能不支持直接调用C脚本，需要回退到传统模式
                self.logger.warning("RPC模式不支持直接调用C脚本，尝试回退到传统模式")
                if self._ts:
                    self._ts.load_c_script(script_name)
                    result = self._ts.call_c_function(function_name, *params)
                    return result
                else:
                    self.logger.error("传统模式未初始化，无法调用C脚本")
                    return None
            elif self._ts:
                # 传统模式
                self._ts.load_c_script(script_name)
                result = self._ts.call_c_function(function_name, *params)
                return result
            else:
                self.logger.error("未连接到TSMaster，无法调用C脚本")
                return None
        except Exception as e:
            self.logger.warning(f"调用C脚本失败: {str(e)}")
            return None
    
    def _execute_test_module(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """
        执行测试模块

        支持执行TSMaster中的测试模块或测试序列
        """
        module_name = item.get("module_name") or item.get("sequence_name")
        timeout = item.get("timeout", self.operation_timeout)
        wait_complete = item.get("wait_for_complete", True)

        if not module_name:
            raise ValueError("测试模块执行需要指定module_name或sequence_name参数")

        try:
            self.logger.info(f"执行测试模块: {module_name}")

            if self._using_rpc and self._rpc_client:
                # RPC模式：使用test_sequence方式执行
                result = self._rpc_client.execute_test_sequence(module_name)
                success = result is not None

                # 等待测试完成
                if success and wait_complete:
                    if not self._wait_for_test_complete(timeout):
                        self.logger.warning(f"等待测试模块完成超时: {module_name}")
            elif self._ts:
                # 传统模式
                result = self._ts.test_execute(module_name)
                success = result
            else:
                return {
                    "name": item.get("name"),
                    "type": "test_module",
                    "module_name": module_name,
                    "status": "error",
                    "error": "未连接到TSMaster"
                }

            # 获取测试统计
            stats = self.get_test_statistics()

            return {
                "name": item.get("name"),
                "type": "test_module",
                "module_name": module_name,
                "result": result,
                "passed": stats.get("passed", 0),
                "failed": stats.get("failed", 0),
                "total": stats.get("total", 0),
                "status": "passed" if success else "failed"
            }

        except Exception as e:
            self.logger.error(f"执行测试模块失败: {str(e)}")
            return {
                "name": item.get("name"),
                "type": "test_module",
                "module_name": module_name,
                "status": "error",
                "error": str(e)
            }
    
    def _execute_diagnostic(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """
        执行诊断操作
        
        支持发送诊断请求并接收响应
        """
        diag_request = item.get("diag_request")
        expected_response = item.get("expected_response")
        timeout = item.get("timeout", 5)
        channel = item.get("channel", 0)
        
        if not diag_request:
            raise ValueError("诊断操作需要指定diag_request参数")
        
        try:
            self.logger.info(f"执行诊断请求: {diag_request}")
            
            if self._using_rpc and self._rpc_client:
                # RPC模式：发送诊断请求
                # 注意：需要检查TSMasterAPI是否支持诊断功能
                if hasattr(self._rpc_client, 'send_diagnostic_request'):
                    response = self._rpc_client.send_diagnostic_request(channel, diag_request, timeout)
                else:
                    self.logger.warning("RPC客户端不支持诊断功能，尝试使用传统模式")
                    if self._ts:
                        response = self._ts.send_diagnostic_request(channel, diag_request, timeout)
                    else:
                        response = None
            elif self._ts:
                # 传统模式
                response = self._ts.send_diagnostic_request(channel, diag_request, timeout)
            else:
                return {
                    "name": item.get("name"),
                    "type": "diagnostic",
                    "status": "error",
                    "error": "未连接到TSMaster"
                }
            
            # 判断结果
            passed = True
            if expected_response and response:
                passed = response == expected_response
            
            return {
                "name": item.get("name"),
                "type": "diagnostic",
                "diag_request": diag_request,
                "expected_response": expected_response,
                "actual_response": response,
                "passed": passed,
                "status": "passed" if passed else "failed"
            }
            
        except Exception as e:
            self.logger.error(f"执行诊断操作失败: {str(e)}")
            return {
                "name": item.get("name"),
                "type": "diagnostic",
                "diag_request": diag_request,
                "status": "error",
                "error": str(e)
            }
    
    def _execute_wait(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """
        执行等待操作
        
        支持固定时间等待或条件等待
        """
        wait_time = item.get("wait_time", 1.0)  # 默认等待1秒
        wait_condition = item.get("wait_condition")  # 可选的等待条件
        timeout = item.get("timeout", 30.0)  # 条件等待的超时时间
        
        try:
            self.logger.info(f"执行等待: {wait_time}秒")
            
            if wait_condition:
                # 条件等待：等待某个条件满足
                import time
                start_time = time.time()
                condition_met = False
                
                while time.time() - start_time < timeout:
                    # 评估条件（简化实现，实际可能需要更复杂的条件解析）
                    condition_type = wait_condition.get("type")
                    if condition_type == "signal":
                        signal_name = wait_condition.get("signal_name")
                        expected_value = wait_condition.get("expected_value")
                        tolerance = wait_condition.get("tolerance", 0.01)
                        
                        actual_value = self._get_signal(signal_name)
                        if actual_value is not None and expected_value is not None:
                            if abs(actual_value - expected_value) < tolerance:
                                condition_met = True
                                break
                    elif condition_type == "system_var":
                        var_name = wait_condition.get("var_name")
                        expected_value = wait_condition.get("expected_value")
                        
                        actual_value = self._read_system_var(var_name)
                        if actual_value is not None and expected_value is not None:
                            if str(actual_value) == str(expected_value):
                                condition_met = True
                                break
                    
                    time.sleep(0.1)  # 100ms轮询间隔
                
                return {
                    "name": item.get("name"),
                    "type": "wait",
                    "wait_time": wait_time,
                    "wait_condition": wait_condition,
                    "condition_met": condition_met,
                    "actual_wait_time": time.time() - start_time,
                    "status": "passed" if condition_met else "failed"
                }
            else:
                # 固定时间等待
                import time
                time.sleep(wait_time)
                
                return {
                    "name": item.get("name"),
                    "type": "wait",
                    "wait_time": wait_time,
                    "status": "passed"
                }
                
        except Exception as e:
            self.logger.error(f"执行等待操作失败: {str(e)}")
            return {
                "name": item.get("name"),
                "type": "wait",
                "status": "error",
                "error": str(e)
            }
    
    def _execute_condition(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """
        执行条件判断
        
        支持信号值、系统变量等多种条件的判断
        """
        condition_type = item.get("condition_type", "signal")
        condition_operator = item.get("operator", "==")  # ==, !=, <, >, <=, >=
        
        try:
            self.logger.info(f"执行条件判断: {condition_type}")
            
            if condition_type == "signal":
                signal_name = item.get("signal_name")
                expected_value = item.get("expected_value")
                tolerance = item.get("tolerance", 0.01)
                
                if not signal_name:
                    raise ValueError("信号条件判断需要指定signal_name参数")
                
                actual_value = self._get_signal(signal_name)
                
                # 根据操作符判断
                condition_met = self._evaluate_condition(
                    actual_value, expected_value, condition_operator, tolerance
                )
                
                return {
                    "name": item.get("name"),
                    "type": "condition",
                    "condition_type": condition_type,
                    "signal_name": signal_name,
                    "expected_value": expected_value,
                    "actual_value": actual_value,
                    "operator": condition_operator,
                    "condition_met": condition_met,
                    "status": "passed" if condition_met else "failed"
                }
                
            elif condition_type == "system_var":
                var_name = item.get("var_name")
                expected_value = item.get("expected_value")
                
                if not var_name:
                    raise ValueError("系统变量条件判断需要指定var_name参数")
                
                actual_value = self._read_system_var(var_name)
                
                # 字符串比较
                condition_met = str(actual_value) == str(expected_value)
                
                return {
                    "name": item.get("name"),
                    "type": "condition",
                    "condition_type": condition_type,
                    "var_name": var_name,
                    "expected_value": expected_value,
                    "actual_value": actual_value,
                    "operator": condition_operator,
                    "condition_met": condition_met,
                    "status": "passed" if condition_met else "failed"
                }
                
            elif condition_type == "logical":
                # 逻辑组合条件（AND/OR）
                sub_conditions = item.get("sub_conditions", [])
                logic_operator = item.get("logic_operator", "AND")  # AND or OR
                
                results = []
                for sub_condition in sub_conditions:
                    sub_result = self._execute_condition(sub_condition)
                    results.append(sub_result.get("condition_met", False))
                
                if logic_operator == "AND":
                    condition_met = all(results)
                else:  # OR
                    condition_met = any(results)
                
                return {
                    "name": item.get("name"),
                    "type": "condition",
                    "condition_type": condition_type,
                    "logic_operator": logic_operator,
                    "sub_conditions_count": len(sub_conditions),
                    "condition_met": condition_met,
                    "status": "passed" if condition_met else "failed"
                }
            else:
                return {
                    "name": item.get("name"),
                    "type": "condition",
                    "status": "error",
                    "error": f"不支持的条件类型: {condition_type}"
                }
                
        except Exception as e:
            self.logger.error(f"执行条件判断失败: {str(e)}")
            return {
                "name": item.get("name"),
                "type": "condition",
                "status": "error",
                "error": str(e)
            }
    
    def _evaluate_condition(self, actual_value, expected_value, operator: str, tolerance: float = 0.01) -> bool:
        """
        评估条件
        
        Args:
            actual_value: 实际值
            expected_value: 期望值
            operator: 操作符 (==, !=, <, >, <=, >=)
            tolerance: 容差（用于浮点数比较）
            
        Returns:
            条件是否满足
        """
        if actual_value is None or expected_value is None:
            return False
        
        try:
            # 尝试转换为数值进行比较
            actual_float = float(actual_value)
            expected_float = float(expected_value)
            
            if operator == "==":
                return abs(actual_float - expected_float) < tolerance
            elif operator == "!=":
                return abs(actual_float - expected_float) >= tolerance
            elif operator == "<":
                return actual_float < expected_float
            elif operator == ">":
                return actual_float > expected_float
            elif operator == "<=":
                return actual_float <= expected_float
            elif operator == ">=":
                return actual_float >= expected_float
            else:
                self.logger.warning(f"未知的操作符: {operator}")
                return False
        except (ValueError, TypeError):
            # 字符串比较
            if operator == "==":
                return str(actual_value) == str(expected_value)
            elif operator == "!=":
                return str(actual_value) != str(expected_value)
            else:
                self.logger.warning(f"字符串不支持的操作符: {operator}")
                return False
    
    def _execute_system_api(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """
        执行系统API调用
        
        支持调用TSMaster的系统级API
        """
        api_name = item.get("api_name")
        args = item.get("args", [])
        buffer_size = item.get("buffer_size", 1024)
        encoding = item.get("encoding", "gbk")
        
        if not api_name:
            raise ValueError("系统API调用需要指定api_name参数")
        
        try:
            self.logger.info(f"执行系统API: {api_name}")
            
            if self._using_rpc and self._rpc_client:
                success = self._rpc_client.call_system_api(api_name, args, buffer_size, encoding)
            else:
                return {
                    "name": item.get("name"),
                    "type": "system_api",
                    "api_name": api_name,
                    "status": "error",
                    "error": "系统API调用仅支持RPC模式"
                }
            
            return {
                "name": item.get("name"),
                "type": "system_api",
                "api_name": api_name,
                "args": args,
                "success": success,
                "status": "passed" if success else "failed"
            }
            
        except Exception as e:
            self.logger.error(f"执行系统API失败: {str(e)}")
            return {
                "name": item.get("name"),
                "type": "system_api",
                "api_name": api_name,
                "status": "error",
                "error": str(e)
            }
    
    def _execute_run_form(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """
        执行启动小程序
        """
        form_name = item.get("form_name")
        encoding = item.get("encoding", "gbk")
        
        if not form_name:
            raise ValueError("启动小程序需要指定form_name参数")
        
        try:
            self.logger.info(f"启动小程序: {form_name}")
            
            if self._using_rpc and self._rpc_client:
                success = self._rpc_client.run_form(form_name, encoding)
            else:
                return {
                    "name": item.get("name"),
                    "type": "run_form",
                    "form_name": form_name,
                    "status": "error",
                    "error": "启动小程序仅支持RPC模式"
                }
            
            return {
                "name": item.get("name"),
                "type": "run_form",
                "form_name": form_name,
                "success": success,
                "status": "passed" if success else "failed"
            }
            
        except Exception as e:
            self.logger.error(f"启动小程序失败: {str(e)}")
            return {
                "name": item.get("name"),
                "type": "run_form",
                "form_name": form_name,
                "status": "error",
                "error": str(e)
            }
    
    def _execute_stop_form(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """
        执行停止小程序
        """
        form_name = item.get("form_name")
        encoding = item.get("encoding", "gbk")
        
        if not form_name:
            raise ValueError("停止小程序需要指定form_name参数")
        
        try:
            self.logger.info(f"停止小程序: {form_name}")
            
            if self._using_rpc and self._rpc_client:
                success = self._rpc_client.stop_form(form_name, encoding)
            else:
                return {
                    "name": item.get("name"),
                    "type": "stop_form",
                    "form_name": form_name,
                    "status": "error",
                    "error": "停止小程序仅支持RPC模式"
                }
            
            return {
                "name": item.get("name"),
                "type": "stop_form",
                "form_name": form_name,
                "success": success,
                "status": "passed" if success else "failed"
            }
            
        except Exception as e:
            self.logger.error(f"停止小程序失败: {str(e)}")
            return {
                "name": item.get("name"),
                "type": "stop_form",
                "form_name": form_name,
                "status": "error",
                "error": str(e)
            }
    
    def _execute_select_test_cases(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """
        执行选择测试用例
        """
        cases = item.get("cases")
        
        if not cases:
            raise ValueError("选择测试用例需要指定cases参数")
        
        try:
            self.logger.info(f"选择测试用例: {cases}")
            
            if self._using_rpc and self._rpc_client:
                success = self._rpc_client.select_test_cases(cases)
            else:
                success = self._write_system_var("TestSystem.SelectCases", cases)
            
            return {
                "name": item.get("name"),
                "type": "select_test_cases",
                "cases": cases,
                "success": success,
                "status": "passed" if success else "failed"
            }
            
        except Exception as e:
            self.logger.error(f"选择测试用例失败: {str(e)}")
            return {
                "name": item.get("name"),
                "type": "select_test_cases",
                "cases": cases,
                "status": "error",
                "error": str(e)
            }
    
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
