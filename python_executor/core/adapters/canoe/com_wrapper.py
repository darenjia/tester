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

import os
import time
import threading
import logging
from pathlib import Path
from typing import Optional, Dict, Any, List
from dataclasses import dataclass

# 尝试导入win32com
WIN32COM_AVAILABLE = False
CANOE_AVAILABLE = False

try:
    import win32com.client
    import pythoncom
    WIN32COM_AVAILABLE = True
    # 注意：不在模块导入时尝试连接CANoe，避免启动应用程序
    # CANoe的连接应该在实际使用时进行

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
        self.last_error: Optional[str] = None

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
                    try:
                        self.stop_measurement()
                    except Exception as e:
                        self.logger.warning(f"停止测量失败: {e}")

                # 关闭配置（不关闭CANoe应用程序）
                if self._app:
                    try:
                        self._app.CloseConfiguration()
                    except:
                        pass

                # 释放COM对象引用（不退出CANoe应用程序）
                self._app = None
                self._measurement = None
                self._system = None
                self._namespaces = None
                self._namespace_cache.clear()
                self._variable_cache.clear()

                self.is_connected = False
                self.is_measurement_running = False
                self.config_path = None

                # 不反初始化COM环境，保持线程兼容性
                # self._uninit_com()

                self.logger.info("CANoe连接已断开")
                return True

            except Exception as e:
                self.logger.error(f"断开CANoe连接时出错: {e}")
                # 强制重置状态
                self._app = None
                self._measurement = None
                self._system = None
                self._namespaces = None
                self.is_connected = False
                self.is_measurement_running = False
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
                self.last_error = "CANoe未连接，无法打开配置"
                raise CANoeConnectionError(self.last_error)

            # 确保COM环境已初始化（线程安全）
            self._init_com()

            timeout = timeout or self.open_timeout

            try:
                self.logger.info(f"正在打开配置文件: {config_path}")

                # 检查测量状态并停止
                try:
                    if self._measurement and self._measurement.Running:
                        self.logger.info("检测到测量正在运行，正在停止...")
                        self.stop_measurement()
                        time.sleep(1.0)
                except Exception as measure_err:
                    self.logger.debug(f"检查测量状态失败: {measure_err}")

                # 尝试关闭当前配置（如果有打开的配置）
                try:
                    self._app.CloseConfiguration()
                    self.logger.info("已关闭当前配置")
                    time.sleep(1.5)  # 等待关闭完成
                except Exception as close_err:
                    # 忽略关闭失败（可能没有打开的配置）
                    self.logger.debug(f"关闭配置时出错（可忽略）: {close_err}")

                # 检查文件是否存在
                if not os.path.exists(config_path):
                    selroutf.last_error = f"配置文件不存在: {config_path}"
                    raise CANoeConfigurationError(self.last_error)

                # 打开配置
                self.logger.info(f"调用 Open({config_path})...")
                try:
                    self._app.Open(config_path)
                except Exception as open_err:
                    # 如果Open失败，尝试获取更多信息
                    self.logger.error(f"Open()调用失败: {open_err}")
                    raise

                # 等待配置加载完成（CANoe的Open是异步操作）
                self.logger.info(f"等待配置加载完成（超时: {timeout}秒）...")
                wait_result = self._wait_for_configuration_loaded(config_path, timeout)

                if wait_result:
                    self.config_path = config_path
                    self.logger.info("配置文件加载成功")
                    return True
                else:
                    self.last_error = f"配置文件加载超时（{timeout}秒）"
                    raise CANoeConfigurationError(self.last_error)

            except CANoeConfigurationError:
                raise
            except Exception as e:
                self.last_error = f"打开配置文件失败: {e}"
                self.logger.error(self.last_error)
                raise CANoeConfigurationError(self.last_error)

    def _wait_for_configuration_loaded(self, config_path: str, timeout: int) -> bool:
        """
        等待配置文件加载完成

        通过访问Configuration对象验证配置是否加载成功

        Args:
            config_path: 配置文件路径
            timeout: 超时时间（秒）

        Returns:
            加载成功返回True
        """
        start_time = time.time()
        check_interval = 0.5  # 检查间隔

        self.logger.info("等待Configuration对象可访问...")

        while time.time() - start_time < timeout:
            try:
                # 尝试访问Configuration对象
                config = self._app.Configuration
                name = config.Name
                elapsed = time.time() - start_time
                self.logger.info(f"配置加载验证成功: 配置名称={name}, 等待时间={elapsed:.2f}秒")

                # 重新获取系统对象和命名空间（配置加载后需要更新）
                try:
                    self._system = self._app.System
                    self._namespaces = self._system.Namespaces
                    self.logger.debug("已更新System和Namespaces引用")
                except Exception as sys_err:
                    self.logger.warning(f"更新系统对象引用失败: {sys_err}")

                # 清除变量缓存
                self._namespace_cache.clear()
                self._variable_cache.clear()

                return True
            except Exception as check_err:
                self.logger.debug(f"验证中... ({check_err})")

            time.sleep(check_interval)

        self.logger.warning(f"配置加载验证超时，已等待 {timeout} 秒")
        return False
    
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
            self.last_error = None  # 清除之前的错误

            if not self.is_connected:
                self.last_error = "CANoe未连接，无法启动测量"
                raise CANoeConnectionError(self.last_error)

            if self._measurement is None:
                self.last_error = "Measurement对象未初始化，请检查CANoe连接状态"
                raise CANoeMeasurementError(self.last_error)

            if self.is_measurement_running:
                self.logger.warning("测量已在运行中")
                return True

            try:
                self.logger.info("正在启动CANoe测量...")

                # 检查是否已经在运行
                try:
                    is_running = self._measurement.Running
                    if is_running:
                        self.logger.info("测量已经在运行")
                        self.is_measurement_running = True
                        return True
                except Exception as e:
                    self.logger.warning(f"检查测量状态失败: {e}，尝试直接启动")

                # 启动测量
                self._measurement.Start()

                # 等待测量启动
                start_time = time.time()
                while time.time() - start_time < timeout:
                    try:
                        if self._measurement.Running:
                            self.is_measurement_running = True
                            self.logger.info("CANoe测量已启动")
                            return True
                    except Exception as check_err:
                        self.logger.debug(f"检查测量状态时出错: {check_err}")
                    time.sleep(0.5)

                self.last_error = f"测量启动超时（{timeout}秒）"
                raise CANoeMeasurementError(self.last_error)

            except CANoeMeasurementError:
                raise
            except Exception as e:
                error_msg = str(e)
                # 提供更详细的错误信息
                if "configuration" in error_msg.lower() or "no configuration" in error_msg.lower():
                    self.last_error = f"启动测量失败: 请先加载配置文件 ({e})"
                elif "license" in error_msg.lower():
                    self.last_error = f"启动测量失败: 许可证问题 ({e})"
                elif "hardware" in error_msg.lower() or "interface" in error_msg.lower():
                    self.last_error = f"启动测量失败: 硬件接口问题 ({e})"
                else:
                    self.last_error = f"启动测量失败: {e}"
                raise CANoeMeasurementError(self.last_error)
    
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
                    try:
                        ns_obj = self._namespaces[namespace]
                        self._namespace_cache[namespace] = ns_obj
                    except Exception as ns_err:
                        self.logger.error(f"获取命名空间失败 [{namespace}]: {ns_err}")
                        raise CANoeVariableError(f"命名空间不存在 [{namespace}]，请确认CANoe配置中定义了此命名空间")

                # 获取变量
                try:
                    var_obj = ns_obj.Variables[variable]
                    self._variable_cache[cache_key] = var_obj
                except Exception as var_err:
                    self.logger.error(f"获取变量失败 [{namespace}.{variable}]: {var_err}")
                    raise CANoeVariableError(f"变量不存在 [{namespace}.{variable}]，请确认CANoe配置中定义了此系统变量")

            return var_obj.Value

        except CANoeVariableError:
            raise
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
                    try:
                        ns_obj = self._namespaces[namespace]
                        self._namespace_cache[namespace] = ns_obj
                    except Exception as ns_err:
                        self.logger.error(f"获取命名空间失败 [{namespace}]: {ns_err}")
                        self.logger.error(f"请确认CANoe配置中定义了命名空间 '{namespace}'")
                        return False

                # 获取变量
                try:
                    var_obj = ns_obj.Variables[variable]
                    self._variable_cache[cache_key] = var_obj
                except Exception as var_err:
                    self.logger.error(f"获取变量失败 [{namespace}.{variable}]: {var_err}")
                    self.logger.error(f"请确认CANoe配置中定义了系统变量 '{namespace}.{variable}'")
                    return False

            var_obj.Value = value
            self.logger.debug(f"设置系统变量成功: {namespace}.{variable} = {value}")
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

    # ==================== TestModule 执行相关方法 ====================

    def get_test_modules(self) -> List[str]:
        """
        获取配置中所有测试模块名称

        Returns:
            测试模块名称列表
        """
        if not self.is_connected:
            self.logger.warning("CANoe未连接")
            return []

        try:
            test_setup = self._app.TestSetup
            test_modules = test_setup.TestModules

            module_names = []
            for i in range(1, test_modules.Count + 1):
                module = test_modules.Item(i)
                module_names.append(module.Name)

            self.logger.info(f"找到 {len(module_names)} 个测试模块: {module_names}")
            return module_names

        except Exception as e:
            self.logger.error(f"获取测试模块列表失败: {e}")
            return []

    def execute_test_module(self, module_name: str, timeout: float = 600.0) -> Dict[str, Any]:
        """
        直接执行测试模块（不使用命名空间/系统变量）

        通过 CANoe COM 接口直接调用 TestModule 执行

        Args:
            module_name: 测试模块名称
            timeout: 执行超时时间（秒）

        Returns:
            执行结果字典:
                - success: 是否成功
                - verdict: 测试结果 (Passed/Failed/None)
                - duration: 执行时长
                - error: 错误信息
        """
        result = {
            "module_name": module_name,
            "success": False,
            "verdict": None,
            "duration": 0,
            "error": None
        }

        if not self.is_connected:
            result["error"] = "CANoe未连接"
            return result

        start_time = time.time()

        try:
            self.logger.info(f"执行测试模块: {module_name}")

            # 获取 TestSetup
            test_setup = self._app.TestSetup
            test_modules = test_setup.TestModules

            # 查找测试模块
            test_module = None
            for i in range(1, test_modules.Count + 1):
                module = test_modules.Item(i)
                if module.Name == module_name:
                    test_module = module
                    break

            if test_module is None:
                result["error"] = f"未找到测试模块: {module_name}"
                self.logger.error(result["error"])
                return result

            # 执行测试模块
            self.logger.info(f"开始执行测试模块: {module_name}")
            test_module.Execute()

            # 等待执行完成
            while time.time() - start_time < timeout:
                try:
                    # 检查模块状态
                    # CANoe TestModule 执行完成后会返回结果
                    verdict = test_module.Verdict
                    if verdict:
                        result["verdict"] = verdict
                        result["success"] = (verdict == "Passed")
                        break
                except Exception as check_err:
                    self.logger.debug(f"检查测试状态: {check_err}")

                time.sleep(0.5)

            result["duration"] = time.time() - start_time

            if result["verdict"] is None:
                result["error"] = f"测试模块执行超时 ({timeout}秒)"
                self.logger.warning(result["error"])
            else:
                self.logger.info(f"测试模块执行完成: {module_name}, 结果: {result['verdict']}, 耗时: {result['duration']:.1f}秒")

            return result

        except Exception as e:
            result["error"] = str(e)
            result["duration"] = time.time() - start_time
            self.logger.error(f"执行测试模块失败 [{module_name}]: {e}")
            return result

    def execute_test_module_by_index(self, index: int, timeout: float = 600.0) -> Dict[str, Any]:
        """
        通过索引执行测试模块

        Args:
            index: 测试模块索引（从1开始）
            timeout: 执行超时时间（秒）

        Returns:
            执行结果字典
        """
        result = {
            "index": index,
            "module_name": None,
            "success": False,
            "verdict": None,
            "duration": 0,
            "error": None
        }

        if not self.is_connected:
            result["error"] = "CANoe未连接"
            return result

        try:
            test_setup = self._app.TestSetup
            test_modules = test_setup.TestModules

            if index < 1 or index > test_modules.Count:
                result["error"] = f"索引超出范围: {index} (共 {test_modules.Count} 个模块)"
                return result

            module = test_modules.Item(index)
            result["module_name"] = module.Name

            # 调用 execute_test_module
            exec_result = self.execute_test_module(module.Name, timeout)
            exec_result["index"] = index
            return exec_result

        except Exception as e:
            result["error"] = str(e)
            self.logger.error(f"通过索引执行测试模块失败 [{index}]: {e}")
            return result

    def get_test_module_verdict(self, module_name: str) -> Optional[str]:
        """
        获取测试模块的执行结果

        Args:
            module_name: 测试模块名称

        Returns:
            测试结果 (Passed/Failed/None)
        """
        if not self.is_connected:
            return None

        try:
            test_setup = self._app.TestSetup
            test_modules = test_setup.TestModules

            for i in range(1, test_modules.Count + 1):
                module = test_modules.Item(i)
                if module.Name == module_name:
                    return module.Verdict

            return None

        except Exception as e:
            self.logger.error(f"获取测试结果失败 [{module_name}]: {e}")
            return None
    
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
