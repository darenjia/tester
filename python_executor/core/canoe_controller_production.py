"""
CANoe控制器 - 生产环境增强版
增加熔断器、重试机制、资源监控等功能
"""
import time
import threading
import logging
from typing import Optional, Dict, Any, List
from pathlib import Path
from contextlib import contextmanager

try:
    import win32com.client
    import pythoncom
    WIN32_AVAILABLE = True
except ImportError:
    WIN32_AVAILABLE = False

from utils.logger import get_logger
from utils.exceptions import ConnectionException, ToolException
from utils.retry import retry_with_config, RetryConfig, CircuitBreaker
from utils.validators import InputValidator, ValidationError
from config.settings import settings

logger = get_logger("canoe_controller")

# 熔断器配置
CANOE_CIRCUIT_BREAKER = CircuitBreaker(
    failure_threshold=5,
    recovery_timeout=300.0,  # 5分钟后尝试恢复
    expected_exception=Exception
)

class CANoeResourceMonitor:
    """CANoe资源监控器"""
    
    def __init__(self):
        self.connection_count = 0
        self.error_count = 0
        self.last_error_time = None
        self._lock = threading.Lock()
    
    def record_connection(self):
        """记录连接"""
        with self._lock:
            self.connection_count += 1
    
    def record_error(self, error: Exception):
        """记录错误"""
        with self._lock:
            self.error_count += 1
            self.last_error_time = time.time()
            logger.error(f"CANoe错误: {error}")
    
    def get_stats(self) -> Dict[str, Any]:
        """获取统计信息"""
        with self._lock:
            return {
                'connection_count': self.connection_count,
                'error_count': self.error_count,
                'last_error_time': self.last_error_time,
                'error_rate': self.error_count / max(self.connection_count, 1)
            }

# 全局资源监控器
resource_monitor = CANoeResourceMonitor()

class CANoeControllerProduction:
    """CANoe控制器 - 生产环境版本"""
    
    def __init__(self):
        self.app = None
        self.measurement = None
        self.version = None
        self.config_path = None
        self.is_connected = False
        self.is_measurement_running = False
        self._lock = threading.RLock()  # 可重入锁
        self._com_initialized = False
        
        # 验证环境
        if not WIN32_AVAILABLE:
            raise ToolException("pywin32库未安装，无法使用CANoe COM接口")
    
    def _init_com(self):
        """初始化COM环境（线程安全）"""
        if not self._com_initialized:
            try:
                pythoncom.CoInitialize()
                self._com_initialized = True
                logger.debug("COM环境初始化成功")
            except Exception as e:
                logger.warning(f"COM环境初始化失败（可能已初始化）: {e}")
    
    def _uninit_com(self):
        """反初始化COM环境"""
        if self._com_initialized:
            try:
                pythoncom.CoUninitialize()
                self._com_initialized = False
                logger.debug("COM环境反初始化成功")
            except Exception as e:
                logger.warning(f"COM环境反初始化失败: {e}")
    
    @CANOE_CIRCUIT_BREAKER
    @retry_with_config(RetryConfig(
        max_attempts=3,
        delay=2.0,
        backoff=2.0,
        exceptions=(ConnectionException, Exception)
    ))
    def connect(self) -> bool:
        """
        连接CANoe应用（带熔断器和重试）
        
        Returns:
            bool: 连接成功返回True
        """
        with self._lock:
            if self.is_connected:
                logger.warning("CANoe已连接")
                return True
            
            try:
                logger.info("正在连接CANoe...")
                self._init_com()
                
                # 创建COM对象
                self.app = win32com.client.Dispatch("CANoe.Application")
                self.measurement = self.app.Measurement
                
                # 获取版本信息
                self.version = self.app.Version
                logger.info(f"CANoe版本: {self.version}")
                
                self.is_connected = True
                resource_monitor.record_connection()
                
                # 记录成功，重置熔断器
                CANOE_CIRCUIT_BREAKER.record_success()
                
                logger.info("CANoe连接成功")
                return True
                
            except Exception as e:
                self.is_connected = False
                resource_monitor.record_error(e)
                CANOE_CIRCUIT_BREAKER.record_failure()
                
                error_msg = f"CANoe连接失败: {str(e)}"
                logger.error(error_msg)
                raise ConnectionException(error_msg)
    
    def disconnect(self) -> bool:
        """
        断开CANoe连接（线程安全）
        
        Returns:
            bool: 断开成功返回True
        """
        with self._lock:
            try:
                if self.is_measurement_running:
                    self.stop_measurement()
                
                if self.app:
                    # 尝试优雅关闭
                    try:
                        self.app.Quit()
                    except:
                        pass
                    
                    self.app = None
                    self.measurement = None
                
                self.is_connected = False
                self.is_measurement_running = False
                
                self._uninit_com()
                
                logger.info("CANoe连接已断开")
                return True
                
            except Exception as e:
                logger.error(f"断开CANoe连接失败: {e}")
                return False
    
    def open_configuration(self, config_path: str) -> bool:
        """
        加载CANoe配置文件（带验证）
        
        Args:
            config_path: 配置文件路径
            
        Returns:
            bool: 加载成功返回True
        """
        with self._lock:
            if not self.is_connected:
                raise ToolException("未连接到CANoe")
            
            # 验证路径
            try:
                validated_path = InputValidator.validate_config_path(config_path)
            except ValidationError as e:
                raise ToolException(f"配置文件路径验证失败: {e}")
            
            # 检查文件是否存在
            path = Path(validated_path)
            if not path.exists():
                raise ToolException(f"配置文件不存在: {validated_path}")
            
            try:
                logger.info(f"正在加载配置文件: {validated_path}")
                
                # 如果测量正在运行，先停止
                if self.is_measurement_running:
                    self.stop_measurement()
                
                # 加载配置
                self.app.Open(str(validated_path))
                self.config_path = validated_path
                
                logger.info("配置文件加载成功")
                return True
                
            except Exception as e:
                error_msg = f"配置文件加载失败: {str(e)}"
                logger.error(error_msg)
                raise ToolException(error_msg)
    
    @retry_with_config(RetryConfig(
        max_attempts=2,
        delay=1.0,
        backoff=1.5,
        exceptions=(ToolException,)
    ))
    def start_measurement(self, timeout: int = None) -> bool:
        """
        启动测量（带重试）
        
        Args:
            timeout: 启动超时时间（秒）
            
        Returns:
            bool: 启动成功返回True
        """
        with self._lock:
            return self._start_measurement_internal(timeout)
    
    def _start_measurement_internal(self, timeout: int = None) -> bool:
        """内部启动测量方法"""
        if not self.is_connected:
            raise ToolException("未连接到CANoe")
        
        if self.is_measurement_running:
            logger.warning("测量已在运行中")
            return True
        
        timeout = timeout or settings.canoe_timeout
        
        try:
            logger.info("正在启动测量...")
            
            if not self.measurement.Running:
                self.measurement.Start()
                
                # 等待启动完成
                start_time = time.time()
                while not self.measurement.Running:
                    if time.time() - start_time > timeout:
                        raise ToolException(f"测量启动超时（{timeout}秒）")
                    time.sleep(0.5)
                
                self.is_measurement_running = True
                logger.info("测量已启动")
                return True
            else:
                self.is_measurement_running = True
                logger.info("测量已在运行中")
                return True
                
        except Exception as e:
            error_msg = f"测量启动失败: {str(e)}"
            logger.error(error_msg)
            raise ToolException(error_msg)
    
    def stop_measurement(self, timeout: int = 30) -> bool:
        """
        停止测量（线程安全）
        
        Args:
            timeout: 停止超时时间（秒）
            
        Returns:
            bool: 停止成功返回True
        """
        with self._lock:
            if not self.is_connected:
                logger.warning("未连接到CANoe")
                return False
            
            if not self.is_measurement_running:
                logger.warning("测量未在运行")
                return True
            
            try:
                logger.info("正在停止测量...")
                
                if self.measurement.Running:
                    self.measurement.Stop()
                    
                    # 等待停止完成
                    start_time = time.time()
                    while self.measurement.Running:
                        if time.time() - start_time > timeout:
                            logger.warning(f"测量停止超时（{timeout}秒）")
                            break
                        time.sleep(0.5)
                
                self.is_measurement_running = False
                logger.info("测量已停止")
                return True
                
            except Exception as e:
                logger.error(f"停止测量失败: {e}")
                return False
    
    def read_signal(self, channel: int, message_name: str, signal_name: str) -> Optional[float]:
        """
        读取信号值（带验证）
        
        Args:
            channel: 通道号
            message_name: 报文名称
            signal_name: 信号名称
            
        Returns:
            float: 信号值，失败返回None
        """
        with self._lock:
            if not self.is_connected:
                logger.error("未连接到CANoe")
                return None
            
            # 验证信号名称
            try:
                validated_signal = InputValidator.validate_signal_name(signal_name)
            except ValidationError as e:
                logger.error(f"信号名称验证失败: {e}")
                return None
            
            try:
                # 获取总线系统
                bus = self.app.BusSystems(channel)
                if not bus:
                    logger.error(f"通道 {channel} 不存在")
                    return None
                
                # 读取信号值
                signal = bus.Signals.Item(validated_signal)
                if not signal:
                    logger.error(f"信号不存在: {validated_signal}")
                    return None
                
                value = float(signal.Value)
                logger.debug(f"读取信号 {validated_signal}: {value}")
                return value
                
            except Exception as e:
                logger.error(f"读取信号失败: {e}")
                return None
    
    def write_signal(self, channel: int, message_name: str, signal_name: str, value: float) -> bool:
        """
        写入信号值（带验证）
        
        Args:
            channel: 通道号
            message_name: 报文名称
            signal_name: 信号名称
            value: 要写入的值
            
        Returns:
            bool: 写入成功返回True
        """
        with self._lock:
            if not self.is_connected:
                logger.error("未连接到CANoe")
                return False
            
            # 验证信号名称
            try:
                validated_signal = InputValidator.validate_signal_name(signal_name)
            except ValidationError as e:
                logger.error(f"信号名称验证失败: {e}")
                return False
            
            try:
                # 获取总线系统
                bus = self.app.BusSystems(channel)
                if not bus:
                    logger.error(f"通道 {channel} 不存在")
                    return False
                
                # 写入信号值
                signal = bus.Signals.Item(validated_signal)
                if not signal:
                    logger.error(f"信号不存在: {validated_signal}")
                    return False
                
                signal.Value = value
                logger.debug(f"写入信号 {validated_signal}: {value}")
                return True
                
            except Exception as e:
                logger.error(f"写入信号失败: {e}")
                return False
    
    def run_test_module(self, test_name: str) -> Dict[str, Any]:
        """
        执行测试模块
        
        Args:
            test_name: 测试模块名称
            
        Returns:
            dict: 测试结果
        """
        with self._lock:
            if not self.is_connected:
                raise ToolException("未连接到CANoe")
            
            # 验证测试名称
            if not test_name or len(test_name) > 256:
                raise ToolException("测试模块名称无效")
            
            try:
                logger.info(f"正在执行测试模块: {test_name}")
                
                # 获取测试配置
                test_config = self.app.TestConfiguration
                if not test_config:
                    raise ToolException("测试配置不可用")
                
                # 执行测试
                test_config.ExecuteTest(test_name)
                
                # 获取测试结果
                result = {
                    "test_name": test_name,
                    "verdict": str(test_config.Verdict),
                    "start_time": str(test_config.StartTime) if hasattr(test_config, 'StartTime') else None,
                    "end_time": str(test_config.EndTime) if hasattr(test_config, 'EndTime') else None
                }
                
                logger.info(f"测试模块执行完成: {test_name}, 结果: {result['verdict']}")
                return result
                
            except Exception as e:
                error_msg = f"执行测试模块失败: {str(e)}"
                logger.error(error_msg)
                return {"error": error_msg}
    
    def get_system_info(self) -> Dict[str, Any]:
        """
        获取系统信息
        
        Returns:
            dict: 系统信息
        """
        with self._lock:
            if not self.is_connected:
                return {"error": "未连接到CANoe"}
            
            try:
                info = {
                    "version": str(self.version),
                    "config_path": self.config_path,
                    "is_measurement_running": self.is_measurement_running,
                    "connected": self.is_connected,
                    "resource_stats": resource_monitor.get_stats()
                }
                return info
            except Exception as e:
                logger.error(f"获取系统信息失败: {e}")
                return {"error": str(e)}
    
    def get_resource_stats(self) -> Dict[str, Any]:
        """获取资源统计信息"""
        return resource_monitor.get_stats()
    
    def __enter__(self):
        """上下文管理器入口"""
        self.connect()
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        """上下文管理器出口"""
        self.disconnect()

# 保持向后兼容
CANoeController = CANoeControllerProduction