"""
CANoe测试执行引擎

提供基于UltraANetT经验的测试执行功能，支持：
- 设备自检
- 显性用例测试
- 隐性用例测试
- 测试结果收集
- 设备信息读取

作者: AI Assistant
创建日期: 2026-02-25
"""

import time
import logging
import threading
from typing import Dict, Any, List, Optional, Callable
from dataclasses import dataclass, field
from datetime import datetime
from enum import Enum

from .com_wrapper import CANoeCOMWrapper, DeviceInfo, CANoeError


class TestStatus(Enum):
    """测试状态"""
    IDLE = "idle"
    RUNNING = "running"
    PAUSED = "paused"
    COMPLETED = "completed"
    FAILED = "failed"
    TIMEOUT = "timeout"
    ERROR = "error"


class TestCaseType(Enum):
    """测试用例类型"""
    EXPLICIT = "explicit"  # 显性用例
    IMPLICIT = "implicit"  # 隐性用例


@dataclass
class TestCaseResult:
    """测试用例结果"""
    name: str
    case_type: TestCaseType
    status: TestStatus
    start_time: Optional[str] = None
    end_time: Optional[str] = None
    result_code: Optional[int] = None
    log: List[str] = field(default_factory=list)
    error_message: Optional[str] = None
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        return {
            "name": self.name,
            "case_type": self.case_type.value,
            "status": self.status.value,
            "start_time": self.start_time,
            "end_time": self.end_time,
            "result_code": self.result_code,
            "log": self.log,
            "error_message": self.error_message
        }


@dataclass
class TestExecutionResult:
    """测试执行结果"""
    status: TestStatus
    device_info: DeviceInfo = field(default_factory=DeviceInfo)
    explicit_results: List[TestCaseResult] = field(default_factory=list)
    implicit_results: List[TestCaseResult] = field(default_factory=list)
    start_time: Optional[str] = None
    end_time: Optional[str] = None
    errors: List[str] = field(default_factory=list)
    
    @property
    def total_cases(self) -> int:
        """总用例数"""
        return len(self.explicit_results) + len(self.implicit_results)
    
    @property
    def passed_cases(self) -> int:
        """通过用例数"""
        return sum(1 for r in self.explicit_results + self.implicit_results 
                  if r.status == TestStatus.COMPLETED and r.result_code == 0)
    
    @property
    def failed_cases(self) -> int:
        """失败用例数"""
        return sum(1 for r in self.explicit_results + self.implicit_results 
                  if r.status == TestStatus.COMPLETED and r.result_code != 0)
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        return {
            "status": self.status.value,
            "device_info": self.device_info.to_dict(),
            "explicit_results": [r.to_dict() for r in self.explicit_results],
            "implicit_results": [r.to_dict() for r in self.implicit_results],
            "start_time": self.start_time,
            "end_time": self.end_time,
            "errors": self.errors,
            "total_cases": self.total_cases,
            "passed_cases": self.passed_cases,
            "failed_cases": self.failed_cases
        }


class CANoeTestEngine:
    """
    CANoe测试执行引擎
    
    基于UltraANetT项目经验实现，支持完整的测试流程：
    1. 设备自检
    2. 显性用例测试
    3. 隐性用例测试
    4. 结果收集和报告
    
    Attributes:
        canoe: CANoe COM包装器实例
        is_running: 是否正在执行测试
        current_case: 当前执行的用例名称
        
    Example:
        >>> engine = CANoeTestEngine(canoe_wrapper)
        >>> result = engine.execute_self_check("selfcheck.cfg")
        >>> if result.status == TestStatus.COMPLETED:
        ...     test_result = engine.execute_test_cases(
        ...         explicit_cases=["CASE_001", "CASE_002"],
        ...         implicit_cases=["CASE_003"],
        ...         dtc_info={"CASE_001": "DTC info"}
        ...     )
    """
    
    # 系统变量名称（与CAPL脚本交互）
    VAR_START_DEVICE_SELF_CHECK = "startDeviceSelfCheck"
    VAR_IS_END_DEVICE_SELF_CHECK = "isEndDeviceSelfCheck"
    VAR_START_PROTOTYPE_SELF_CHECK = "startPrototypeSelfCheck"
    VAR_IS_END_PROTOTYPE_SELF_CHECK = "isEndPrototySelfCheck"
    VAR_START_TEST = "startTest"
    VAR_END_TEST = "endTest"
    VAR_TEST_SCRIPT_NAME = "testScriptName"
    VAR_TEST_SCRIPT_NAME_STATE = "testscriptNameState"
    VAR_BUFFER_FLAG = "bufferFlag"
    VAR_BUFFER_VALUE = "bufferValue"
    VAR_TEST_CASE_RESULT_STATE = "testCaseResultState"
    VAR_DTC_TEST_INFORMATION = "dtcTestInformation"
    VAR_START_DEVICE_INFO = "startDeviceInfo"
    
    # 设备信息变量名
    DEVICE_INFO_VARS = [
        "carManufacturerECUHardwareNumber",
        "carManufacturerECUSoftware",
        "ECUBatchNumber",
        "ECUManufacturingDate",
        "softwareVersionNumber",
        "sparePartsNumberOfAutomobileManufacturers",
        "systemVendorECUHardwareNumber",
        "systemVendorECUSoftware",
        "systemVendorECUSoftwareVersionNumber",
        "systemVendorHardwareVersionNumber",
        "systemVendorNameCode",
        "VINCode"
    ]
    
    def __init__(self, canoe: CANoeCOMWrapper, 
                 namespace: str = "mutualVar",
                 logger: Optional[logging.Logger] = None):
        """
        初始化测试执行引擎
        
        Args:
            canoe: CANoe COM包装器实例
            namespace: 系统变量命名空间（默认mutualVar）
            logger: 日志记录器
        """
        self._canoe = canoe
        self._namespace = namespace
        self.logger = logger or logging.getLogger(__name__)
        
        # 执行状态
        self.is_running = False
        self.is_paused = False
        self.current_case: Optional[str] = None
        self._execution_thread: Optional[threading.Thread] = None
        self._stop_event = threading.Event()
        
        # 缓存
        self._test_cache: List[str] = []
        self._result_cache: Dict[str, TestCaseResult] = {}
        
        # 回调函数
        self._progress_callback: Optional[Callable[[str, int, int], None]] = None
        self._log_callback: Optional[Callable[[str, str], None]] = None
        
    def set_progress_callback(self, callback: Callable[[str, int, int], None]):
        """
        设置进度回调函数
        
        Args:
            callback: 回调函数，参数为(当前用例名, 已完成数, 总数)
        """
        self._progress_callback = callback
        
    def set_log_callback(self, callback: Callable[[str, str], None]):
        """
        设置日志回调函数
        
        Args:
            callback: 回调函数，参数为(用例名, 日志内容)
        """
        self._log_callback = callback
        
    def _notify_progress(self, case_name: str, completed: int, total: int):
        """通知进度更新"""
        if self._progress_callback:
            try:
                self._progress_callback(case_name, completed, total)
            except Exception as e:
                self.logger.warning(f"进度回调执行失败: {e}")
                
    def _notify_log(self, case_name: str, log_content: str):
        """通知日志更新"""
        if self._log_callback:
            try:
                self._log_callback(case_name, log_content)
            except Exception as e:
                self.logger.warning(f"日志回调执行失败: {e}")
    
    def execute_self_check(self, config_path: str, 
                          timeout: float = 300.0,
                          read_device_info: bool = True) -> TestExecutionResult:
        """
        执行设备自检
        
        基于UltraANetT ProcCANoeTest.StartDeviceSelfCheck实现
        
        Args:
            config_path: 自检配置文件路径
            timeout: 自检超时时间（秒）
            read_device_info: 是否读取设备信息
            
        Returns:
            测试结果
            
        Example:
            >>> result = engine.execute_self_check("selfcheck.cfg", timeout=300)
            >>> if result.status == TestStatus.COMPLETED:
            ...     print(f"设备信息: {result.device_info.vin_code}")
        """
        result = TestExecutionResult(
            status=TestStatus.RUNNING,
            start_time=datetime.now().isoformat()
        )
        
        try:
            self.logger.info(f"开始设备自检，配置文件: {config_path}")
            self.is_running = True
            
            # 1. 打开自检配置
            self.logger.info("步骤1: 打开自检配置")
            if not self._canoe.open_configuration(config_path):
                result.status = TestStatus.FAILED
                result.errors.append("打开自检配置失败")
                return result
            
            # 2. 启动测量
            self.logger.info("步骤2: 启动测量")
            if not self._canoe.start_measurement():
                result.status = TestStatus.FAILED
                result.errors.append("启动测量失败")
                return result
            
            # 3. 触发自检程序
            self.logger.info("步骤3: 触发自检程序")
            if not self._canoe.set_system_variable(
                self._namespace, 
                self.VAR_START_DEVICE_SELF_CHECK, 
                1
            ):
                result.status = TestStatus.FAILED
                result.errors.append("触发自检程序失败")
                return result
            
            # 4. 等待自检完成
            self.logger.info("步骤4: 等待自检完成...")
            start_time = time.time()
            self_check_completed = False
            
            while time.time() - start_time < timeout:
                # 检查停止信号
                if self._stop_event.is_set():
                    self.logger.info("收到停止信号，中止自检")
                    result.status = TestStatus.FAILED
                    result.errors.append("用户中止")
                    return result
                
                try:
                    is_end = self._canoe.get_system_variable(
                        self._namespace, 
                        self.VAR_IS_END_DEVICE_SELF_CHECK
                    )
                    if is_end == 1:
                        self_check_completed = True
                        self.logger.info("设备自检完成")
                        break
                except Exception as e:
                    self.logger.warning(f"读取自检状态失败: {e}")
                
                time.sleep(0.5)
            
            if not self_check_completed:
                result.status = TestStatus.TIMEOUT
                result.errors.append(f"设备自检超时（{timeout}秒）")
                return result
            
            # 5. 读取设备信息
            if read_device_info:
                self.logger.info("步骤5: 读取设备信息")
                result.device_info = self._read_device_info()
                self.logger.info(f"设备信息读取完成: {result.device_info.vin_code}")
            
            result.status = TestStatus.COMPLETED
            result.end_time = datetime.now().isoformat()
            
        except Exception as e:
            self.logger.error(f"设备自检执行失败: {e}")
            result.status = TestStatus.ERROR
            result.errors.append(str(e))
            
        finally:
            self.is_running = False
            
        return result
    
    def execute_test_cases(self, 
                          explicit_cases: Optional[List[str]] = None,
                          implicit_cases: Optional[List[str]] = None,
                          dtc_info: Optional[Dict[str, str]] = None,
                          case_timeout: float = 600.0) -> TestExecutionResult:
        """
        执行测试用例
        
        基于UltraANetT ProcCANoeTest.StartDExampleCheck和StartHExampleCheck实现
        
        Args:
            explicit_cases: 显性用例列表
            implicit_cases: 隐性用例列表
            dtc_info: DTC信息字典，键为用例名
            case_timeout: 单个用例超时时间（秒）
            
        Returns:
            测试结果
            
        Example:
            >>> result = engine.execute_test_cases(
            ...     explicit_cases=["CASE_001@1", "CASE_002@1"],
            ...     implicit_cases=["CASE_003@1"],
            ...     dtc_info={"CASE_001": "P0101"}
            ... )
        """
        result = TestExecutionResult(
            status=TestStatus.RUNNING,
            start_time=datetime.now().isoformat()
        )
        
        try:
            self.is_running = True
            self._stop_event.clear()
            
            # 执行显性用例
            if explicit_cases:
                self.logger.info(f"开始执行显性用例，共{len(explicit_cases)}个")
                explicit_results = self._execute_cases_batch(
                    explicit_cases, 
                    TestCaseType.EXPLICIT,
                    dtc_info or {},
                    case_timeout
                )
                result.explicit_results = explicit_results
            
            # 检查是否中止
            if self._stop_event.is_set():
                result.status = TestStatus.FAILED
                result.errors.append("用户中止")
                return result
            
            # 执行隐性用例
            if implicit_cases:
                self.logger.info(f"开始执行隐性用例，共{len(implicit_cases)}个")
                implicit_results = self._execute_cases_batch(
                    implicit_cases,
                    TestCaseType.IMPLICIT, 
                    dtc_info or {},
                    case_timeout
                )
                result.implicit_results = implicit_results
            
            # 判断整体结果
            all_results = result.explicit_results + result.implicit_results
            if any(r.status == TestStatus.ERROR for r in all_results):
                result.status = TestStatus.ERROR
            elif any(r.status == TestStatus.TIMEOUT for r in all_results):
                result.status = TestStatus.TIMEOUT
            elif any(r.status == TestStatus.FAILED for r in all_results):
                result.status = TestStatus.COMPLETED  # 有失败但已完成
            else:
                result.status = TestStatus.COMPLETED
                
            result.end_time = datetime.now().isoformat()
            
        except Exception as e:
            self.logger.error(f"测试用例执行失败: {e}")
            result.status = TestStatus.ERROR
            result.errors.append(str(e))
            
        finally:
            self.is_running = False
            self.current_case = None
            
        return result
    
    def _execute_cases_batch(self, 
                            cases: List[str], 
                            case_type: TestCaseType,
                            dtc_info: Dict[str, str],
                            timeout: float) -> List[TestCaseResult]:
        """
        批量执行用例
        
        Args:
            cases: 用例名称列表
            case_type: 用例类型
            dtc_info: DTC信息
            timeout: 超时时间
            
        Returns:
            用例结果列表
        """
        results = []
        total = len(cases)
        
        for index, case_full_name in enumerate(cases):
            # 检查停止信号
            if self._stop_event.is_set():
                self.logger.info("收到停止信号，中止用例执行")
                break
            
            # 解析用例名和测试次数
            case_name, test_count = self._parse_case_name(case_full_name)
            
            for test_index in range(1, test_count + 1):
                current_case = f"{case_name}@{test_index}"
                self.current_case = current_case
                
                self.logger.info(f"执行用例: {current_case} ({index + 1}/{total})")
                self._notify_progress(current_case, index + 1, total)
                
                # 执行单个用例
                case_result = self._execute_single_case(
                    current_case, case_name, case_type, dtc_info, timeout
                )
                results.append(case_result)
                
                # 缓存结果
                self._result_cache[current_case] = case_result
        
        return results
    
    def _parse_case_name(self, case_full_name: str) -> tuple:
        """
        解析用例名称
        
        格式: "CASE_NAME" 或 "CASE_NAME@COUNT"
        
        Args:
            case_full_name: 完整用例名
            
        Returns:
            (用例名, 测试次数)
        """
        if "@" in case_full_name:
            parts = case_full_name.split("@")
            try:
                count = int(parts[1])
                return parts[0], max(1, count)
            except:
                return case_full_name, 1
        return case_full_name, 1
    
    def _execute_single_case(self, 
                            current_case: str,
                            case_name: str,
                            case_type: TestCaseType,
                            dtc_info: Dict[str, str],
                            timeout: float) -> TestCaseResult:
        """
        执行单个用例
        
        Args:
            current_case: 当前用例完整名称（含测试次数）
            case_name: 用例基础名称
            case_type: 用例类型
            dtc_info: DTC信息
            timeout: 超时时间
            
        Returns:
            用例结果
        """
        result = TestCaseResult(
            name=current_case,
            case_type=case_type,
            status=TestStatus.RUNNING,
            start_time=datetime.now().isoformat()
        )
        
        try:
            # 1. 设置DTC信息
            if case_name in dtc_info:
                self._canoe.set_system_variable(
                    self._namespace,
                    self.VAR_DTC_TEST_INFORMATION,
                    dtc_info[case_name]
                )
            
            # 2. 设置用例名称
            self._canoe.set_system_variable(
                self._namespace,
                self.VAR_TEST_SCRIPT_NAME,
                current_case
            )
            time.sleep(0.2)
            
            # 3. 触发测试
            self._canoe.set_system_variable(
                self._namespace,
                self.VAR_START_TEST,
                1
            )
            time.sleep(0.2)
            
            # 4. 等待测试完成
            start_time = time.time()
            last_buffer_value = ""
            
            while time.time() - start_time < timeout:
                # 检查停止信号
                if self._stop_event.is_set():
                    result.status = TestStatus.FAILED
                    result.error_message = "用户中止"
                    break
                
                # 检查缓冲标志（日志输出）
                try:
                    buffer_flag = self._canoe.get_system_variable(
                        self._namespace, 
                        self.VAR_BUFFER_FLAG
                    )
                    if buffer_flag == 1:
                        buffer_value = self._canoe.get_system_variable(
                            self._namespace,
                            self.VAR_BUFFER_VALUE
                        )
                        if buffer_value != last_buffer_value:
                            result.log.append(buffer_value)
                            self._notify_log(current_case, buffer_value)
                            last_buffer_value = buffer_value
                        
                        # 重置缓冲标志
                        self._canoe.set_system_variable(
                            self._namespace,
                            self.VAR_BUFFER_FLAG,
                            0
                        )
                except Exception as e:
                    self.logger.warning(f"读取缓冲标志失败: {e}")
                
                # 检查测试结束
                try:
                    end_test = self._canoe.get_system_variable(
                        self._namespace,
                        self.VAR_END_TEST
                    )
                    if end_test == 1:
                        # 重置结束标志
                        self._canoe.set_system_variable(
                            self._namespace,
                            self.VAR_END_TEST,
                            0
                        )
                        
                        # 读取结果
                        result_code = self._canoe.get_system_variable(
                            self._namespace,
                            self.VAR_TEST_CASE_RESULT_STATE
                        )
                        result.result_code = result_code
                        result.status = TestStatus.COMPLETED
                        break
                except Exception as e:
                    self.logger.warning(f"读取测试结束标志失败: {e}")
                
                time.sleep(0.5)
            
            # 检查是否超时
            if result.status == TestStatus.RUNNING:
                result.status = TestStatus.TIMEOUT
                result.error_message = f"用例执行超时（{timeout}秒）"
                
        except Exception as e:
            self.logger.error(f"执行用例失败 [{current_case}]: {e}")
            result.status = TestStatus.ERROR
            result.error_message = str(e)
        
        result.end_time = datetime.now().isoformat()
        return result
    
    def _read_device_info(self) -> DeviceInfo:
        """
        读取设备信息
        
        基于UltraANetT ProcExample实现
        
        Returns:
            设备信息
        """
        device_info = DeviceInfo()
        
        # 触发设备信息读取
        try:
            self._canoe.set_system_variable(
                self._namespace,
                self.VAR_START_DEVICE_INFO,
                1
            )
            time.sleep(0.5)
        except Exception as e:
            self.logger.warning(f"触发设备信息读取失败: {e}")
        
        # 读取各个字段
        field_mapping = {
            "carManufacturerECUHardwareNumber": "car_manufacturer_ecu_hardware_number",
            "carManufacturerECUSoftware": "car_manufacturer_ecu_software",
            "ECUBatchNumber": "ecu_batch_number",
            "ECUManufacturingDate": "ecu_manufacturing_date",
            "softwareVersionNumber": "software_version_number",
            "sparePartsNumberOfAutomobileManufacturers": "spare_parts_number",
            "systemVendorECUHardwareNumber": "system_vendor_ecu_hardware_number",
            "systemVendorECUSoftware": "system_vendor_ecu_software",
            "systemVendorECUSoftwareVersionNumber": "system_vendor_software_version",
            "systemVendorHardwareVersionNumber": "system_vendor_hardware_version",
            "systemVendorNameCode": "system_vendor_name_code",
            "VINCode": "vin_code"
        }
        
        for var_name, attr_name in field_mapping.items():
            try:
                value = self._canoe.get_system_variable(self._namespace, var_name)
                if value:
                    setattr(device_info, attr_name, str(value))
            except Exception as e:
                self.logger.debug(f"读取设备信息字段失败 [{var_name}]: {e}")
        
        return device_info
    
    def stop(self):
        """停止测试执行"""
        self.logger.info("正在停止测试执行...")
        self._stop_event.set()
        self.is_running = False
        
    def pause(self):
        """暂停测试执行"""
        self.logger.info("暂停测试执行")
        self.is_paused = True
        
    def resume(self):
        """恢复测试执行"""
        self.logger.info("恢复测试执行")
        self.is_paused = False
        
    def get_execution_status(self) -> Dict[str, Any]:
        """
        获取执行状态
        
        Returns:
            执行状态字典
        """
        return {
            "is_running": self.is_running,
            "is_paused": self.is_paused,
            "current_case": self.current_case,
            "cached_cases": len(self._test_cache),
            "cached_results": len(self._result_cache)
        }
    
    def clear_cache(self):
        """清除缓存"""
        self._test_cache.clear()
        self._result_cache.clear()
        self.logger.debug("测试引擎缓存已清除")


# 便捷函数
def create_test_engine(canoe: CANoeCOMWrapper, 
                      namespace: str = "mutualVar",
                      logger: Optional[logging.Logger] = None) -> CANoeTestEngine:
    """
    创建测试执行引擎实例
    
    Args:
        canoe: CANoe COM包装器
        namespace: 命名空间
        logger: 日志记录器
        
    Returns:
        CANoeTestEngine实例
    """
    return CANoeTestEngine(canoe, namespace, logger)


# 测试代码
if __name__ == "__main__":
    # 配置日志
    logging.basicConfig(
        level=logging.DEBUG,
        format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
    )
    
    # 创建包装器和引擎
    from .com_wrapper import create_canoe_wrapper
    
    wrapper = create_canoe_wrapper()
    
    try:
        if wrapper.connect():
            engine = create_test_engine(wrapper)
            
            # 测试设备自检
            result = engine.execute_self_check("test.cfg", timeout=60)
            print(f"自检结果: {result.status.value}")
            print(f"设备信息: {result.device_info.to_dict()}")
            
            # 断开连接
            wrapper.disconnect()
        else:
            print("连接失败")
    except Exception as e:
        print(f"错误: {e}")
