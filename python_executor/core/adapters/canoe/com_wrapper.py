"""
CANoe COM接口包装器

提供对Vector CANoe应用程序的COM接口封装，支持：
- 应用程序连接/断开
- 配置文件加载
- 测量控制（启动/停止）
- 信号读写
- 系统变量操作
- 总线报文收发

依赖：
    - pywin32 (win32com.client)
    - CANoe 10/11/12/13/14/15/16/17

作者: AI Assistant
创建日期: 2026-02-25
"""

import time
import threading
import logging
from pathlib import Path
from typing import Optional, Dict, Any, List, Union
from dataclasses import dataclass
from enum import Enum

# 尝试导入win32com
WIN32COM_AVAILABLE = False
CANOE_AVAILABLE = False

try:
    import win32com.client
    import pythoncom
    WIN32COM_AVAILABLE = True
    
    # 尝试连接CANoe以验证可用性
    try:
        test_app = win32com.client.Dispatch("CANoe.Application")
        CANOE_AVAILABLE = True
        del test_app
    except:
        pass
        
except ImportError:
    logging.warning("pywin32未安装，CANoe COM接口不可用")


class CANoeError(Exception):
    """CANoe操作错误基类"""
    pass


class CANoeConnectionError(CANoeError):
    """CANoe连接错误"""
    pass


class CANoeConfigurationError(CANoeError):
    """CANoe配置错误"""
    pass


class CANoeMeasurementError(CANoeError):
    """CANoe测量控制错误"""
    pass


class CANoeVariableError(CANoeError):
    """CANoe变量操作错误"""
    pass


@dataclass
class CANoeVersion:
    """CANoe版本信息"""
    major: int
    minor: int
    patch: int
    full_version: str
    
    def __str__(self) -> str:
        return f"{self.major}.{self.minor}.{self.patch}"


@dataclass
class DeviceInfo:
    """设备信息"""
    car_manufacturer_ecu_hardware_number: str = ""
    car_manufacturer_ecu_software: str = ""
    ecu_batch_number: str = ""
    ecu_manufacturing_date: str = ""
    software_version_number: str = ""
    spare_parts_number: str = ""
    system_vendor_ecu_hardware_number: str = ""
    system_vendor_ecu_software: str = ""
    system_vendor_software_version: str = ""
    system_vendor_hardware_version: str = ""
    system_vendor_name_code: str = ""
    vin_code: str = ""
    
    def to_dict(self) -> Dict[str, str]:
        """转换为字典"""
        return {
            "car_manufacturer_ecu_hardware_number": self.car_manufacturer_ecu_hardware_number,
            "car_manufacturer_ecu_software": self.car_manufacturer_ecu_software,
            "ecu_batch_number": self.ecu_batch_number,
            "ecu_manufacturing_date": self.ecu_manufacturing_date,
            "software_version_number": self.software_version_number,
            "spare_parts_number": self.spare_parts_number,
            "system_vendor_ecu_hardware_number": self.system_vendor_ecu_hardware_number,
            "system_vendor_ecu_software": self.system_vendor_ecu_software,
            "system_vendor_software_version": self.system_vendor_software_version,
            "system_vendor_hardware_version": self.system_vendor_hardware_version,
            "system_vendor_name_code": self.system_vendor_name_code,
            "vin_code": self.vin_code,
        }


class CANoeCOMWrapper:
    """
    CANoe COM接口包装器
    
    封装CANoe COM接口，提供Python友好的API
    
    Attributes:
        app: CANoe应用程序对象
        measurement: CANoe测量对象
        system: CANoe系统对象
        namespaces: CANoe命名空间集合
        version: CANoe版本信息
        
    Example:
        >>> wrapper = CANoeCOMWrapper()
        >>> if wrapper.connect():
        ...     wrapper.open_configuration("test.cfg")
        ...     wrapper.start_measurement()
        ...     value = wrapper.get_signal_value("EngineSpeed")
        ...     wrapper.stop_measurement()
        ...     wrapper.close()
    """
    
    # 默认命名空间名称（与CAPL脚本交互）
    DEFAULT_NAMESPACE = "mutualVar"
    
    def __init__(self, logger: Optional[logging.Logger] = None):
        """
        初始化CANoe COM包装器
        
        Args:
            logger: 日志记录器，如果为None则使用默认日志记录器
        """
        self._app = None
        self._measurement = None
        self._system = None
        self._namespaces = None
        self._namespace_cache: Dict[str, Any] = {}
        self._variable_cache: Dict[str, Any] = {}
        
        self.version: Optional[CANoeVersion] = None
        self.is_connected = False
        self.is_measurement_running = False
        
        self.logger = logger or logging.getLogger(__name__)
        
        # 配置
        self.config_path: Optional[str] = None
        self.open_timeout = 30  # 打开配置超时时间（秒）
        self.measurement_timeout = 3600  # 测量超时时间（秒）
        self.config_load_wait_time = 15  # 配置加载等待时间（秒）
        
        # 线程安全
        self._lock = threading.RLock()
        self._com_initialized = False
        
    def _check_win32com(self) -> bool:
        """检查win32com是否可用"""
        if not WIN32COM_AVAILABLE:
            raise CANoeConnectionError(
                "pywin32未安装，无法使用CANoe COM接口。"
                "请执行: pip install pywin32"
            )
        return True
    
    def _init_com(self):
        """初始化COM环境（线程安全）"""
        if not self._com_initialized:
            try:
                pythoncom.CoInitialize()
                self._com_initialized = True
                self.logger.debug("COM环境初始化成功")
            except Exception as e:
                self.logger.warning(f"COM环境初始化失败（可能已初始化）: {e}")
    
    def _uninit_com(self):
        """反初始化COM环境"""
        if self._com_initialized:
            try:
                pythoncom.CoUninitialize()
                self._com_initialized = False
                self.logger.debug("COM环境反初始化成功")
            except Exception as e:
                self.logger.warning(f"COM环境反初始化失败: {e}")
    
    def connect(self, retry_count: int = 3, retry_interval: float = 2.0) -> bool:
        """
        连接CANoe应用程序
        
        尝试连接到正在运行的CANoe应用程序实例。
        如果CANoe未运行，将尝试启动它。
        
        Args:
            retry_count: 重试次数
            retry_interval: 重试间隔（秒）
            
        Returns:
            连接成功返回True，否则返回False
            
        Raises:
            CANoeConnectionError: 连接失败时抛出
            
        Example:
            >>> wrapper = CANoeCOMWrapper()
            >>> success = wrapper.connect(retry_count=5)
            >>> print(f"连接状态: {success}")
        """
        with self._lock:
            if self.is_connected:
                self.logger.warning("CANoe已连接")
                return True
            
            self._check_win32com()
            self._init_com()
            
            for attempt in range(retry_count):
                try:
                    self.logger.info(f"正在连接CANoe (尝试 {attempt + 1}/{retry_count})...")
                    
                    # 创建CANoe应用程序对象
                    self._app = win32com.client.Dispatch("CANoe.Application")
                    
                    # 获取测量对象
                    self._measurement = self._app.Measurement
                    
                    # 获取系统对象
                    self._system = self._app.System
                    
                    # 获取命名空间集合
                    self._namespaces = self._system.Namespaces
                    
                    # 获取版本信息
                    self._load_version_info()
                    
                    self.is_connected = True
                    self.logger.info(f"CANoe连接成功 (版本: {self.version})")
                    return True
                    
                except Exception as e:
                    self.logger.warning(f"连接尝试 {attempt + 1} 失败: {e}")
                    if attempt < retry_count - 1:
                        time.sleep(retry_interval)
                    else:
                        raise CANoeConnectionError(
                            f"连接CANoe失败（已重试{retry_count}次）: {e}"
                        )
            
            return False
    
    def _load_version_info(self):
        """加载CANoe版本信息"""
        try:
            version_str = str(self._app.Version)
            # 解析版本号，格式通常为 "17.3.91" 或 "CANoe 17 SP5"
            # 提取数字部分
            import re
            version_match = re.search(r'(\d+)(?:\.(\d+))?(?:\.(\d+))?', version_str)
            if version_match:
                major = int(version_match.group(1)) if version_match.group(1) else 0
                minor = int(version_match.group(2)) if version_match.group(2) else 0
                patch = int(version_match.group(3)) if version_match.group(3) else 0
            else:
                major, minor, patch = 0, 0, 0
            
            self.version = CANoeVersion(
                major=major,
                minor=minor,
                patch=patch,
                full_version=version_str
            )
            self.logger.info(f"CANoe版本信息: {self.version}")
        except Exception as e:
            self.logger.warning(f"获取CANoe版本信息失败: {e}")
            self.version = CANoeVersion(0, 0, 0, "unknown")
    
    def disconnect(self) -> bool:
        """
        断开CANoe连接
        
        停止测量（如果正在运行）并释放COM对象。
        
        Returns:
            断开成功返回True
            
        Example:
            >>> wrapper.disconnect()
            >>> print(f"连接状态: {wrapper.is_connected}")
            False
        """
        with self._lock:
            try:
                self.logger.info("正在断开CANoe连接...")
                
                # 停止测量
                if self._measurement and self.is_measurement_running:
                    self.stop_measurement()
                
                # 尝试关闭CANoe应用程序
                if self._app:
                    try:
                        self._app.Quit()
                    except:
                        pass
                    
                    self._app = None
                    self._measurement = None
                
                # 释放对象
                self._system = None
                self._namespaces = None
                self._namespace_cache.clear()
                self._variable_cache.clear()
                
                self.is_connected = False
                self.is_measurement_running = False
                
                # 反初始化COM环境
                self._uninit_com()
                
                self.logger.info("CANoe连接已断开")
                return True
                
            except Exception as e:
                self.logger.error(f"断开CANoe连接时出错: {e}")
                # 强制重置状态
                self._app = None
                self._measurement = None
                self.is_connected = False
                return False
    
    def open_configuration(self, config_path: str, timeout: Optional[int] = None) -> bool:
        """
        打开CANoe配置文件
        
        Args:
            config_path: 配置文件路径（.cfg）
            timeout: 打开超时时间（秒），默认使用self.open_timeout
            
        Returns:
            打开成功返回True
            
        Raises:
            CANoeConfigurationError: 打开失败时抛出
            
        Example:
            >>> wrapper.open_configuration("C:/Test/config.cfg")
            True
        """
        with self._lock:
            if not self.is_connected:
                raise CANoeConnectionError("CANoe未连接，无法打开配置")
            
            # 验证文件是否存在
            path = Path(config_path)
            if not path.exists():
                raise CANoeConfigurationError(f"配置文件不存在: {config_path}")
            
            try:
                self.logger.info(f"正在打开配置文件: {config_path}")
                
                # 停止当前测量（如果正在运行）
                if self.is_measurement_running:
                    self.stop_measurement()
                
                # 打开配置
                self._app.Open(str(config_path))
                
                # 等待配置加载完成
                self.logger.info(f"等待配置加载完成（{self.config_load_wait_time}秒）...")
                time.sleep(self.config_load_wait_time)
                
                self.config_path = str(config_path)
                self.logger.info("配置文件打开成功")
                return True
                
            except Exception as e:
                raise CANoeConfigurationError(f"打开配置文件失败: {e}")
    
    def close_configuration(self) -> bool:
        """
        关闭当前配置文件
        
        Returns:
            关闭成功返回True
        """
        with self._lock:
            if not self.is_connected:
                return True
            
            try:
                # 停止测量
                if self.is_measurement_running:
                    self.stop_measurement()
                
                # 关闭配置
                self._app.CloseConfiguration()
                self.config_path = None
                
                self.logger.info("配置文件已关闭")
                return True
                
            except Exception as e:
                self.logger.error(f"关闭配置文件失败: {e}")
                return False
    
    def start_measurement(self, timeout: int = 30) -> bool:
        """
        启动CANoe测量
        
        Args:
            timeout: 启动超时时间（秒）
            
        Returns:
            启动成功返回True
            
        Raises:
            CANoeMeasurementError: 启动失败时抛出
            
        Example:
            >>> wrapper.start_measurement(timeout=60)
            True
        """
        with self._lock:
            if not self.is_connected:
                raise CANoeConnectionError("CANoe未连接，无法启动测量")
            
            if self.is_measurement_running:
                self.logger.warning("测量已在运行中")
                return True
            
            try:
                self.logger.info("正在启动CANoe测量...")
                
                # 检查是否已经在运行
                if self._measurement.Running:
                    self.logger.info("测量已经在运行")
                    self.is_measurement_running = True
                    return True
                
                # 启动测量
                self._measurement.Start()
                
                # 等待测量启动
                start_time = time.time()
                while time.time() - start_time < timeout:
                    if self._measurement.Running:
                        self.is_measurement_running = True
                        self.logger.info("CANoe测量已启动")
                        return True
                    time.sleep(0.5)
                
                raise CANoeMeasurementError(f"测量启动超时（{timeout}秒）")
                
            except Exception as e:
                raise CANoeMeasurementError(f"启动测量失败: {e}")
    
    def stop_measurement(self, timeout: int = 10) -> bool:
        """
        停止CANoe测量
        
        Args:
            timeout: 停止超时时间（秒）
            
        Returns:
            停止成功返回True
            
        Example:
            >>> wrapper.stop_measurement()
            True
        """
        with self._lock:
            if not self.is_connected:
                return True
            
            if not self.is_measurement_running:
                return True
            
            try:
                self.logger.info("正在停止CANoe测量...")
                
                # 检查是否已经在停止
                if not self._measurement.Running:
                    self.is_measurement_running = False
                    return True
                
                # 停止测量
                self._measurement.Stop()
                
                # 等待测量停止
                start_time = time.time()
                while time.time() - start_time < timeout:
                    if not self._measurement.Running:
                        self.is_measurement_running = False
                        self.logger.info("CANoe测量已停止")
                        return True
                    time.sleep(0.5)
                
                self.logger.warning(f"测量停止超时（{timeout}秒）")
                return False
                
            except Exception as e:
                self.logger.error(f"停止测量失败: {e}")
                return False
    
    def get_measurement_state(self) -> bool:
        """
        获取测量状态
        
        Returns:
            测量正在运行返回True，否则返回False
        """
        if not self.is_connected or not self._measurement:
            return False
        
        try:
            return bool(self._measurement.Running)
        except:
            return False
    
    def get_signal_value(self, signal_name: str, 
                        bus: str = "CAN", 
                        channel: int = 1) -> Optional[float]:
        """
        获取信号值
        
        Args:
            signal_name: 信号名称
            bus: 总线类型（CAN/LIN/Ethernet）
            channel: 通道号
            
        Returns:
            信号值，如果失败返回None
            
        Example:
            >>> value = wrapper.get_signal_value("EngineSpeed", "CAN", 1)
            >>> print(f"发动机转速: {value} RPM")
        """
        if not self.is_connected:
            self.logger.warning("CANoe未连接，无法获取信号值")
            return None
        
        try:
            bus_obj = self._app.GetBus(bus)
            signal = bus_obj.GetSignal(channel, signal_name)
            return float(signal.Value)
        except Exception as e:
            self.logger.warning(f"获取信号值失败 [{signal_name}]: {e}")
            return None
    
    def set_signal_value(self, signal_name: str, 
                        value: float,
                        bus: str = "CAN", 
                        channel: int = 1) -> bool:
        """
        设置信号值
        
        Args:
            signal_name: 信号名称
            value: 信号值
            bus: 总线类型
            channel: 通道号
            
        Returns:
            设置成功返回True
            
        Example:
            >>> wrapper.set_signal_value("EngineSpeed", 1500.0, "CAN", 1)
            True
        """
        if not self.is_connected:
            self.logger.warning("CANoe未连接，无法设置信号值")
            return False
        
        try:
            bus_obj = self._app.GetBus(bus)
            signal = bus_obj.GetSignal(channel, signal_name)
            signal.Value = value
            return True
        except Exception as e:
            self.logger.warning(f"设置信号值失败 [{signal_name}={value}]: {e}")
            return False
    
    def get_system_variable(self, namespace: str, variable: str) -> Any:
        """
        获取系统变量值
        
        Args:
            namespace: 命名空间名称
            variable: 变量名称
            
        Returns:
            变量值
            
        Raises:
            CANoeVariableError: 获取失败时抛出
            
        Example:
            >>> value = wrapper.get_system_variable("mutualVar", "testResult")
            >>> print(f"测试结果: {value}")
        """
        if not self.is_connected:
            raise CANoeConnectionError("CANoe未连接")
        
        try:
            # 使用缓存
            cache_key = f"{namespace}.{variable}"
            if cache_key in self._variable_cache:
                var_obj = self._variable_cache[cache_key]
            else:
                # 获取命名空间
                if namespace in self._namespace_cache:
                    ns_obj = self._namespace_cache[namespace]
                else:
                    ns_obj = self._namespaces[namespace]
                    self._namespace_cache[namespace] = ns_obj
                
                # 获取变量
                var_obj = ns_obj.Variables[variable]
                self._variable_cache[cache_key] = var_obj
            
            return var_obj.Value
            
        except Exception as e:
            raise CANoeVariableError(f"获取系统变量失败 [{namespace}.{variable}]: {e}")
    
    def set_system_variable(self, namespace: str, variable: str, value: Any) -> bool:
        """
        设置系统变量值
        
        Args:
            namespace: 命名空间名称
            variable: 变量名称
            value: 变量值
            
        Returns:
            设置成功返回True
            
        Example:
            >>> wrapper.set_system_variable("mutualVar", "startTest", 1)
            True
        """
        if not self.is_connected:
            raise CANoeConnectionError("CANoe未连接")
        
        try:
            # 使用缓存
            cache_key = f"{namespace}.{variable}"
            if cache_key in self._variable_cache:
                var_obj = self._variable_cache[cache_key]
            else:
                # 获取命名空间
                if namespace in self._namespace_cache:
                    ns_obj = self._namespace_cache[namespace]
                else:
                    ns_obj = self._namespaces[namespace]
                    self._namespace_cache[namespace] = ns_obj
                
                # 获取变量
                var_obj = ns_obj.Variables[variable]
                self._variable_cache[cache_key] = var_obj
            
            var_obj.Value = value
            return True
            
        except Exception as e:
            self.logger.error(f"设置系统变量失败 [{namespace}.{variable}={value}]: {e}")
            return False
    
    def send_can_message(self, channel: int, msg_id: int, 
                        data: List[int], 
                        msg_type: str = "standard") -> bool:
        """
        发送CAN报文
        
        Args:
            channel: CAN通道号
            msg_id: 报文ID
            data: 数据字节列表（0-8字节）
            msg_type: 报文类型（standard/extended）
            
        Returns:
            发送成功返回True
            
        Example:
            >>> wrapper.send_can_message(1, 0x123, [0x01, 0x02, 0x03, 0x04])
            True
        """
        if not self.is_connected:
            self.logger.warning("CANoe未连接，无法发送报文")
            return False
        
        try:
            # 获取CAN总线
            can_bus = self._app.GetBus("CAN")
            
            # 创建报文
            frame = can_bus.CreateFrame()
            frame.ID = msg_id
            frame.Data = data
            frame.Channel = channel
            frame.Type = 1 if msg_type == "extended" else 0
            
            # 发送报文
            can_bus.SendFrame(frame)
            return True
            
        except Exception as e:
            self.logger.error(f"发送CAN报文失败: {e}")
            return False
    
    def wait_for_variable(self, namespace: str, variable: str, 
                         expected_value: Any, 
                         timeout: float = 30.0,
                         check_interval: float = 0.5) -> bool:
        """
        等待系统变量达到期望值
        
        Args:
            namespace: 命名空间
            variable: 变量名
            expected_value: 期望值
            timeout: 超时时间（秒）
            check_interval: 检查间隔（秒）
            
        Returns:
            达到期望值返回True，超时返回False
            
        Example:
            >>> success = wrapper.wait_for_variable("mutualVar", "isEndTest", 1, timeout=60)
        """
        start_time = time.time()
        
        while time.time() - start_time < timeout:
            try:
                current_value = self.get_system_variable(namespace, variable)
                if current_value == expected_value:
                    return True
            except:
                pass
            time.sleep(check_interval)
        
        return False
    
    def clear_variable_cache(self):
        """清除变量缓存"""
        self._variable_cache.clear()
        self._namespace_cache.clear()
        self.logger.debug("变量缓存已清除")
    
    def __enter__(self):
        """上下文管理器入口"""
        self.connect()
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        """上下文管理器出口"""
        self.disconnect()
        return False


# 便捷函数
def create_canoe_wrapper(logger: Optional[logging.Logger] = None) -> CANoeCOMWrapper:
    """
    创建CANoe包装器实例
    
    Args:
        logger: 日志记录器
        
    Returns:
        CANoeCOMWrapper实例
    """
    return CANoeCOMWrapper(logger)


# 测试代码
if __name__ == "__main__":
    # 配置日志
    logging.basicConfig(
        level=logging.DEBUG,
        format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
    )
    
    # 测试连接
    wrapper = CANoeCOMWrapper()
    
    try:
        if wrapper.connect():
            print(f"CANoe版本: {wrapper.version}")
            print(f"连接状态: {wrapper.is_connected}")
            
            # 测试信号读写
            value = wrapper.get_signal_value("EngineSpeed", "CAN", 1)
            print(f"EngineSpeed: {value}")
            
            # 断开连接
            wrapper.disconnect()
        else:
            print("连接失败")
    except Exception as e:
        print(f"错误: {e}")
