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
from .capabilities import (
    ArtifactCapability,
    ConfigurationCapability,
    MeasurementCapability,
    ProjectControlCapability,
    TSMasterExecutionCapability,
)
from core.case_mapping_manager import get_case_mapping_manager

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
        self._register_capabilities()
        
    @property
    def tool_type(self) -> TestToolType:
        """返回测试工具类型"""
        return TestToolType.TSMASTER

    def _register_capabilities(self) -> None:
        self.register_capability(
            "tsmaster_execution",
            TSMasterExecutionCapability(
                build_case_selection=self.build_case_selection,
                start_execution=lambda selected_cases: self.start_test_execution(
                    test_cases=selected_cases,
                    wait_for_complete=False,
                    timeout=self.operation_timeout,
                ),
                wait_for_completion=self.wait_for_test_complete,
                get_report_info=self.get_test_report_info,
            ),
        )
        self.register_capability(
            "configuration",
            ConfigurationCapability(load=self.load_configuration),
        )
        self.register_capability(
            "measurement",
            MeasurementCapability(start=self.start_test, stop=self.stop_test),
        )
        self.register_capability(
            "project_control",
            ProjectControlCapability(
                start_form=self.start_master_form,
                stop_form=self.stop_master_form,
            ),
        )
        self.register_capability(
            "artifact",
            ArtifactCapability(collect=self.get_test_report_info),
        )

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
                if self.fallback_to_traditional:
                    self.logger.info("尝试回退到传统模式连接...")
                    if self._connect_via_traditional():
                        self._using_rpc = False
                        self.status = AdapterStatus.CONNECTED
                        self._clear_error()
                        self.logger.info("TSMaster传统模式连接成功")
                        return True

                self._set_error("RPC模式连接失败")
                return False

        if self._connect_via_traditional():
            self._using_rpc = False
            self.status = AdapterStatus.CONNECTED
            self._clear_error()
            self.logger.info("TSMaster传统模式连接成功")
            return True

        self._set_error("TSMaster连接失败")
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
                if not self.wait_for_test_complete(timeout):
                    self.logger.warning("等待测试完成超时")

            return True

        except Exception as e:
            self._set_error(f"测试执行启动失败: {str(e)}")
            return False

    def wait_for_test_complete(self, timeout: Optional[int] = None) -> bool:
        """
        Wait for test completion by polling the TSMaster controller state.

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

            # 如果使用Master小程序，先触发报告生成
            if self._using_rpc and self._rpc_client and self._master_form_started:
                # 生成报告（这会更新Master.ReportPath等变量）
                self._rpc_client.generate_report(timeout=10)

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

    def build_case_selection(self, plan) -> str:
        """
        Build a TSMaster test case selection string from plan cases.

        The returned string follows the conventional "CASE_A=1,CASE_B=1" form.
        """
        mapping_manager = get_case_mapping_manager()
        selection_parts: List[str] = []

        plan_cases = getattr(plan, "cases", None) or getattr(plan, "test_items", None) or []
        for case in plan_cases:
            case_no = getattr(case, "case_no", None) or getattr(case, "caseNo", None)
            if not case_no:
                continue

            mapping = mapping_manager.get_mapping(case_no)
            if mapping and getattr(mapping, "ini_config", None):
                selection_parts.append(mapping.ini_config)
            else:
                selection_parts.append(f"{case_no}=1")

        return ",".join(selection_parts)

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

        适配器层不再承载高层测试项执行语义，统一由 strategy 负责。

        Args:
            item: 测试项配置字典
            
        Returns:
            测试结果字典
        """
        item_type = item.get("type")
        item_name = item.get("name", "unnamed")
        self.logger.warning(f"execute_test_item 已废弃，当前类型不再由适配器层执行: {item_type}")
        return {
            "name": item_name,
            "type": item_type,
            "status": "error",
            "error": f"适配器层不再支持直接执行测试项: {item_type}",
        }
    
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
