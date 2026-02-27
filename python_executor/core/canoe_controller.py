"""
CANoe控制器 - 基于COM接口封装
"""
import time
import logging
from typing import Optional, Dict, Any, List
from pathlib import Path

try:
    import win32com.client
    WIN32_AVAILABLE = True
except ImportError:
    WIN32_AVAILABLE = False

from utils.logger import get_logger
from utils.exceptions import ConnectionException, ToolException
from config.settings import settings

logger = get_logger("canoe_controller")

class CANoeController:
    """CANoe测试软件控制器"""
    
    def __init__(self):
        self.app = None
        self.measurement = None
        self.version = None
        self.config_path = None
        self.is_connected = False
        self.is_measurement_running = False
        
        # 验证环境
        if not WIN32_AVAILABLE:
            raise ToolException("pywin32库未安装，无法使用CANoe COM接口")
    
    def connect(self) -> bool:
        """
        连接CANoe应用
        
        Returns:
            bool: 连接成功返回True
            
        Raises:
            ConnectionException: 连接失败
        """
        try:
            logger.info("正在连接CANoe...")
            
            # 创建COM对象
            self.app = win32com.client.Dispatch("CANoe.Application")
            self.measurement = self.app.Measurement
            
            # 获取版本信息
            self.version = self.app.Version
            logger.info(f"CANoe版本: {self.version}")
            
            self.is_connected = True
            logger.info("CANoe连接成功")
            return True
            
        except Exception as e:
            self.is_connected = False
            error_msg = f"CANoe连接失败: {str(e)}"
            logger.error(error_msg)
            raise ConnectionException(error_msg)
    
    def disconnect(self) -> bool:
        """
        断开CANoe连接
        
        Returns:
            bool: 断开成功返回True
        """
        try:
            if self.is_measurement_running:
                self.stop_measurement()
            
            if self.app:
                self.app = None
                self.measurement = None
            
            self.is_connected = False
            self.is_measurement_running = False
            logger.info("CANoe连接已断开")
            return True
            
        except Exception as e:
            logger.error(f"断开CANoe连接失败: {e}")
            return False
    
    def open_configuration(self, config_path: str) -> bool:
        """
        加载CANoe配置文件
        
        Args:
            config_path: 配置文件路径
            
        Returns:
            bool: 加载成功返回True
            
        Raises:
            ToolException: 配置文件不存在或加载失败
        """
        if not self.is_connected:
            raise ToolException("未连接到CANoe")
        
        # 验证配置文件存在
        config_file = Path(config_path)
        if not config_file.exists():
            raise ToolException(f"配置文件不存在: {config_path}")
        
        try:
            logger.info(f"正在加载配置文件: {config_path}")
            
            # 如果测量正在运行，先停止
            if self.is_measurement_running:
                self.stop_measurement()
            
            # 加载配置
            self.app.Open(str(config_path))
            self.config_path = config_path
            
            logger.info("配置文件加载成功")
            return True
            
        except Exception as e:
            error_msg = f"配置文件加载失败: {str(e)}"
            logger.error(error_msg)
            raise ToolException(error_msg)
    
    def start_measurement(self, timeout: int = None) -> bool:
        """
        启动测量
        
        Args:
            timeout: 启动超时时间（秒），默认使用配置值
            
        Returns:
            bool: 启动成功返回True
            
        Raises:
            ToolException: 启动失败
        """
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
        停止测量
        
        Args:
            timeout: 停止超时时间（秒）
            
        Returns:
            bool: 停止成功返回True
        """
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
        读取信号值
        
        Args:
            channel: 通道号
            message_name: 报文名称
            signal_name: 信号名称
            
        Returns:
            float: 信号值，失败返回None
        """
        if not self.is_connected:
            logger.error("未连接到CANoe")
            return None
        
        try:
            # 获取总线系统
            bus = self.app.BusSystems(channel)
            if not bus:
                logger.error(f"通道 {channel} 不存在")
                return None
            
            # 读取信号值
            signal = bus.Signals.Item(signal_name)
            if not signal:
                logger.error(f"信号不存在: {signal_name}")
                return None
            
            value = float(signal.Value)
            logger.debug(f"读取信号 {signal_name}: {value}")
            return value
            
        except Exception as e:
            logger.error(f"读取信号失败: {e}")
            return None
    
    def write_signal(self, channel: int, message_name: str, signal_name: str, value: float) -> bool:
        """
        写入信号值
        
        Args:
            channel: 通道号
            message_name: 报文名称
            signal_name: 信号名称
            value: 要写入的值
            
        Returns:
            bool: 写入成功返回True
        """
        if not self.is_connected:
            logger.error("未连接到CANoe")
            return False
        
        try:
            # 获取总线系统
            bus = self.app.BusSystems(channel)
            if not bus:
                logger.error(f"通道 {channel} 不存在")
                return False
            
            # 写入信号值
            signal = bus.Signals.Item(signal_name)
            if not signal:
                logger.error(f"信号不存在: {signal_name}")
                return False
            
            signal.Value = value
            logger.debug(f"写入信号 {signal_name}: {value}")
            return True
            
        except Exception as e:
            logger.error(f"写入信号失败: {e}")
            return False
    
    def send_message(self, channel: int, msg_id: int, data: List[int], extended: bool = False) -> bool:
        """
        发送CAN报文
        
        Args:
            channel: 通道号
            msg_id: 报文ID
            data: 数据字节列表
            extended: 是否扩展帧
            
        Returns:
            bool: 发送成功返回True
        """
        if not self.is_connected:
            logger.error("未连接到CANoe")
            return False
        
        try:
            # 获取总线系统
            bus = self.app.BusSystems(channel)
            if not bus:
                logger.error(f"通道 {channel} 不存在")
                return False
            
            # 这里需要根据实际的CANoe API来实现报文发送
            # 由于不同版本的CANoe API可能不同，这里提供一个框架
            logger.info(f"发送报文: ID=0x{msg_id:03X}, Data={data}, Extended={extended}")
            
            # 注意：实际的报文发送API需要根据具体的CANoe版本和配置来确定
            # 这里仅作为示例，可能需要调整
            return True
            
        except Exception as e:
            logger.error(f"发送报文失败: {e}")
            return False
    
    def run_test_module(self, test_name: str) -> Dict[str, Any]:
        """
        执行测试模块
        
        Args:
            test_name: 测试模块名称
            
        Returns:
            dict: 测试结果
        """
        if not self.is_connected:
            raise ToolException("未连接到CANoe")
        
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
        if not self.is_connected:
            return {"error": "未连接到CANoe"}
        
        try:
            info = {
                "version": str(self.version),
                "config_path": self.config_path,
                "is_measurement_running": self.is_measurement_running,
                "connected": self.is_connected
            }
            return info
        except Exception as e:
            logger.error(f"获取系统信息失败: {e}")
            return {"error": str(e)}
    
    def __enter__(self):
        """上下文管理器入口"""
        self.connect()
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        """上下文管理器出口"""
        self.disconnect()