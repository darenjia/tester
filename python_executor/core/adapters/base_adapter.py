"""
测试工具适配器基类

定义统一的测试工具接口，所有具体适配器必须继承此类
"""

from abc import ABC, abstractmethod
from enum import Enum, auto
from typing import Optional, Dict, Any, List
import logging

from .capabilities import CapabilityRegistryMixin


class TestToolType(Enum):
    """测试工具类型枚举"""
    __test__ = False
    CANOE = "canoe"
    TSMASTER = "tsmaster"
    TTWORKBENCH = "ttworkbench"


class AdapterStatus(Enum):
    """适配器状态枚举"""
    IDLE = auto()           # 空闲
    CONNECTING = auto()     # 连接中
    CONNECTED = auto()      # 已连接
    RUNNING = auto()        # 运行中
    ERROR = auto()          # 错误
    DISCONNECTED = auto()   # 已断开


class BaseTestAdapter(CapabilityRegistryMixin, ABC):
    """
    测试工具适配器基类
    
    所有测试工具适配器必须继承此类，实现统一接口
    """
    
    def __init__(self, config: dict = None):
        """
        初始化适配器
        
        Args:
            config: 适配器配置字典
        """
        self.config = config or {}
        self.status = AdapterStatus.IDLE
        self._capabilities: Dict[str, Any] = {}
        from utils.logger import get_logger
        self.logger = get_logger(f"adapters.{self.__class__.__name__}")
        self._last_error: Optional[str] = None
        
    @property
    @abstractmethod
    def tool_type(self) -> TestToolType:
        """返回测试工具类型"""
        pass
    
    @property
    def is_connected(self) -> bool:
        """检查是否已连接（包括运行中状态）"""
        return self.status in [AdapterStatus.CONNECTED, AdapterStatus.RUNNING]
    
    @property
    def is_running(self) -> bool:
        """检查是否正在运行"""
        return self.status == AdapterStatus.RUNNING
    
    @property
    def last_error(self) -> Optional[str]:
        """获取最后一次错误信息"""
        return self._last_error
    
    @abstractmethod
    def connect(self) -> bool:
        """
        连接测试工具
        
        Returns:
            连接成功返回True，否则返回False
        """
        pass
    
    @abstractmethod
    def disconnect(self) -> bool:
        """
        断开测试工具连接
        
        Returns:
            断开成功返回True，否则返回False
        """
        pass
    
    @abstractmethod
    def load_configuration(self, config_path: str) -> bool:
        """
        加载测试配置文件
        
        Args:
            config_path: 配置文件路径
            
        Returns:
            加载成功返回True，否则返回False
        """
        pass
    
    @abstractmethod
    def start_test(self) -> bool:
        """
        启动测试（测量/仿真）
        
        Returns:
            启动成功返回True，否则返回False
        """
        pass
    
    @abstractmethod
    def stop_test(self) -> bool:
        """
        停止测试（测量/仿真）
        
        Returns:
            停止成功返回True，否则返回False
        """
        pass
    
    def execute_test_item(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """
        Legacy compatibility API for direct test-item execution.

        New execution paths should go through strategy + capability instead of
        invoking high-level test items on adapters directly.
        
        Args:
            item: 测试项配置
            
        Returns:
            测试结果字典
        """
        item_type = item.get("type")
        item_name = item.get("name", "unnamed")
        self.logger.warning(
            "execute_test_item 已退化为兼容接口，不建议在新执行链路中直接调用: %s",
            item_type,
        )
        return {
            "name": item_name,
            "type": item_type,
            "status": "error",
            "error": f"适配器层不再支持直接执行测试项: {item_type}",
        }
    
    def get_status(self) -> Dict[str, Any]:
        """
        获取适配器状态
        
        Returns:
            状态信息字典
        """
        return {
            "tool_type": self.tool_type.value,
            "status": self.status.name,
            "is_connected": self.is_connected,
            "is_running": self.is_running,
            "last_error": self._last_error
        }
    
    def _set_error(self, error_msg: str):
        """
        设置错误状态
        
        Args:
            error_msg: 错误信息
        """
        self._last_error = error_msg
        self.status = AdapterStatus.ERROR
        self.logger.error(error_msg)
    
    def _clear_error(self):
        """清除错误状态"""
        self._last_error = None
        if self.status == AdapterStatus.ERROR:
            self.status = AdapterStatus.IDLE
    
    def validate_config(self, required_keys: List[str]) -> bool:
        """
        验证配置是否包含必需的键

        Args:
            required_keys: 必需的配置键列表

        Returns:
            验证通过返回True，否则返回False
        """
        missing_keys = [key for key in required_keys if key not in self.config]
        if missing_keys:
            self._set_error(f"配置缺少必需的键: {missing_keys}")
            return False
        return True

    def __enter__(self) -> 'BaseTestAdapter':
        """上下文管理器入口"""
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        """上下文管理器出口，确保断开连接"""
        try:
            if self.is_connected:
                self.disconnect()
        except Exception as e:
            self.logger.warning(f"上下文管理器退出时断开连接失败: {e}")
        return False  # 不吞没异常
