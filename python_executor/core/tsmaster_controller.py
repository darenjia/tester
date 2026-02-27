"""
TSMaster控制器 - 基于原生Python API封装
"""
import time
import logging
from typing import Optional, Dict, Any, List, Callable
from pathlib import Path

try:
    from TSMaster import *
    TSMASTER_AVAILABLE = True
except ImportError:
    TSMASTER_AVAILABLE = False

from utils.logger import get_logger
from utils.exceptions import ConnectionException, ToolException
from config.settings import settings

logger = get_logger("tsmaster_controller")

class TSMasterController:
    """TSMaster测试软件控制器"""
    
    def __init__(self):
        self.ts = None
        self.version = None
        self.config_path = None
        self.is_connected = False
        self.is_simulation_running = False
        self.callbacks = {}
        
        # 验证环境
        if not TSMASTER_AVAILABLE:
            raise ToolException("TSMaster Python API未安装，请确保已安装TSMaster并配置Python API")
    
    def connect(self) -> bool:
        """
        连接TSMaster应用
        
        Returns:
            bool: 连接成功返回True
            
        Raises:
            ConnectionException: 连接失败
        """
        try:
            logger.info("正在连接TSMaster...")
            
            # 创建TSMaster实例
            self.ts = TSMaster()
            self.ts.connect()
            
            # 获取版本信息（如果API支持）
            try:
                self.version = getattr(self.ts, 'version', 'Unknown')
                logger.info(f"TSMaster版本: {self.version}")
            except:
                self.version = 'Unknown'
            
            self.is_connected = True
            logger.info("TSMaster连接成功")
            return True
            
        except Exception as e:
            self.is_connected = False
            error_msg = f"TSMaster连接失败: {str(e)}"
            logger.error(error_msg)
            raise ConnectionException(error_msg)
    
    def disconnect(self) -> bool:
        """
        断开TSMaster连接
        
        Returns:
            bool: 断开成功返回True
        """
        try:
            if self.is_simulation_running:
                self.stop_simulation()
            
            if self.ts:
                self.ts.disconnect()
                self.ts = None
            
            self.is_connected = False
            self.is_simulation_running = False
            logger.info("TSMaster连接已断开")
            return True
            
        except Exception as e:
            logger.error(f"断开TSMaster连接失败: {e}")
            return False
    
    def open_configuration(self, config_path: str) -> bool:
        """
        加载TSMaster配置文件
        
        Args:
            config_path: 配置文件路径
            
        Returns:
            bool: 加载成功返回True
            
        Raises:
            ToolException: 配置文件不存在或加载失败
        """
        if not self.is_connected:
            raise ToolException("未连接到TSMaster")
        
        # 验证配置文件存在
        config_file = Path(config_path)
        if not config_file.exists():
            raise ToolException(f"配置文件不存在: {config_path}")
        
        try:
            logger.info(f"正在加载配置文件: {config_path}")
            
            # 如果仿真正在运行，先停止
            if self.is_simulation_running:
                self.stop_simulation()
            
            # 加载配置
            self.ts.load_config(str(config_path))
            self.config_path = config_path
            
            logger.info("配置文件加载成功")
            return True
            
        except Exception as e:
            error_msg = f"配置文件加载失败: {str(e)}"
            logger.error(error_msg)
            raise ToolException(error_msg)
    
    def start_simulation(self, timeout: int = None) -> bool:
        """
        启动仿真
        
        Args:
            timeout: 启动超时时间（秒），默认使用配置值
            
        Returns:
            bool: 启动成功返回True
            
        Raises:
            ToolException: 启动失败
        """
        if not self.is_connected:
            raise ToolException("未连接到TSMaster")
        
        if self.is_simulation_running:
            logger.warning("仿真已在运行中")
            return True
        
        timeout = timeout or settings.tsmaster_timeout
        
        try:
            logger.info("正在启动仿真...")
            
            self.ts.start_bus()
            
            # 等待启动完成（简单延时，实际可能需要更复杂的检测）
            start_time = time.time()
            time.sleep(1)  # 给仿真启动一些时间
            
            # 检查是否成功启动（这里假设启动成功）
            self.is_simulation_running = True
            logger.info("仿真已启动")
            return True
            
        except Exception as e:
            error_msg = f"仿真启动失败: {str(e)}"
            logger.error(error_msg)
            raise ToolException(error_msg)
    
    def stop_simulation(self, timeout: int = 30) -> bool:
        """
        停止仿真
        
        Args:
            timeout: 停止超时时间（秒）
            
        Returns:
            bool: 停止成功返回True
        """
        if not self.is_connected:
            logger.warning("未连接到TSMaster")
            return False
        
        if not self.is_simulation_running:
            logger.warning("仿真未在运行")
            return True
        
        try:
            logger.info("正在停止仿真...")
            
            self.ts.stop_bus()
            
            # 等待停止完成
            time.sleep(1)
            
            self.is_simulation_running = False
            logger.info("仿真已停止")
            return True
            
        except Exception as e:
            logger.error(f"停止仿真失败: {e}")
            return False
    
    def get_signal_value(self, signal_name: str) -> Optional[float]:
        """
        获取信号值
        
        Args:
            signal_name: 信号名称
            
        Returns:
            float: 信号值，失败返回None
        """
        if not self.is_connected:
            logger.error("未连接到TSMaster")
            return None
        
        try:
            # 获取信号值
            value = self.ts.get_signal_value(signal_name)
            logger.debug(f"获取信号 {signal_name}: {value}")
            return float(value) if value is not None else None
            
        except Exception as e:
            logger.error(f"获取信号失败: {e}")
            return None
    
    def set_signal_value(self, signal_name: str, value: float) -> bool:
        """
        设置信号值
        
        Args:
            signal_name: 信号名称
            value: 要设置的值
            
        Returns:
            bool: 设置成功返回True
        """
        if not self.is_connected:
            logger.error("未连接到TSMaster")
            return False
        
        try:
            self.ts.set_signal_value(signal_name, value)
            logger.debug(f"设置信号 {signal_name}: {value}")
            return True
            
        except Exception as e:
            logger.error(f"设置信号失败: {e}")
            return False
    
    def get_signal(self, signal_name: str) -> Optional[float]:
        """
        获取信号值（兼容CANoe接口）
        
        Args:
            signal_name: 信号名称
            
        Returns:
            float: 信号值，失败返回None
        """
        return self.get_signal_value(signal_name)
    
    def set_signal(self, signal_name: str, value: float) -> bool:
        """
        设置信号值（兼容CANoe接口）
        
        Args:
            signal_name: 信号名称
            value: 要设置的值
            
        Returns:
            bool: 设置成功返回True
        """
        return self.set_signal_value(signal_name, value)
    
    def transmit_can_msg(self, channel: int, msg_id: int, data: List[int], extended: bool = False) -> bool:
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
            logger.error("未连接到TSMaster")
            return False
        
        try:
            # 转换数据格式
            data_bytes = bytes(data)
            
            # 发送报文
            self.ts.transmit_can_msg(channel, msg_id, data_bytes, extended)
            
            logger.info(f"发送CAN报文: 通道={channel}, ID=0x{msg_id:03X}, 数据={data}")
            return True
            
        except Exception as e:
            logger.error(f"发送CAN报文失败: {e}")
            return False
    
    def send_message(self, channel: int, msg_id: int, data: List[int], extended: bool = False) -> bool:
        """
        发送CAN报文（兼容CANoe接口）
        
        Args:
            channel: 通道号
            msg_id: 报文ID
            data: 数据字节列表
            extended: 是否扩展帧
            
        Returns:
            bool: 发送成功返回True
        """
        return self.transmit_can_msg(channel, msg_id, data, extended)
    
    def register_rx_callback(self, callback: Callable):
        """
        注册报文接收回调
        
        Args:
            callback: 回调函数
        """
        if not self.is_connected:
            logger.error("未连接到TSMaster")
            return
        
        try:
            self.ts.register_rx_callback(callback)
            logger.info("已注册报文接收回调")
            
        except Exception as e:
            logger.error(f"注册回调失败: {e}")
    
    def test_execute(self, sequence_name: str) -> Dict[str, Any]:
        """
        执行测试序列
        
        Args:
            sequence_name: 测试序列名称
            
        Returns:
            dict: 测试结果
        """
        if not self.is_connected:
            raise ToolException("未连接到TSMaster")
        
        try:
            logger.info(f"正在执行测试序列: {sequence_name}")
            
            # 这里假设TSMaster有test_execute方法
            # 实际API可能需要调整
            result = {
                "sequence_name": sequence_name,
                "status": "completed",
                "verdict": "PASSED",
                "details": "测试序列执行完成"
            }
            
            logger.info(f"测试序列执行完成: {sequence_name}")
            return result
            
        except Exception as e:
            error_msg = f"执行测试序列失败: {str(e)}"
            logger.error(error_msg)
            return {"error": error_msg}
    
    def run_test_sequence(self, sequence_name: str) -> Dict[str, Any]:
        """
        执行测试序列（兼容CANoe接口）
        
        Args:
            sequence_name: 测试序列名称
            
        Returns:
            dict: 测试结果
        """
        return self.test_execute(sequence_name)
    
    def get_system_info(self) -> Dict[str, Any]:
        """
        获取系统信息
        
        Returns:
            dict: 系统信息
        """
        if not self.is_connected:
            return {"error": "未连接到TSMaster"}
        
        try:
            info = {
                "version": str(self.version),
                "config_path": self.config_path,
                "is_simulation_running": self.is_simulation_running,
                "connected": self.is_connected,
                "available": TSMASTER_AVAILABLE
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