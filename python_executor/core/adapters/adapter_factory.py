"""
适配器工厂模块
统一创建和管理测试工具适配器
"""
from enum import Enum
from typing import Dict, Type, Optional, Any
import logging

from .base_adapter import BaseTestAdapter as BaseAdapter

logger = logging.getLogger(__name__)


class TestToolType(Enum):
    """测试工具类型枚举"""
    CANOE = "canoe"
    TSMASTER = "tsmaster"
    TTWORKBENCH = "ttworkbench"


class AdapterFactory:
    """
    适配器工厂类
    
    负责创建和管理各种测试工具的适配器实例
    """
    
    # 存储适配器类
    _adapters: Dict[TestToolType, Type[BaseAdapter]] = {}
    
    # 存储适配器实例（单例模式）
    _instances: Dict[TestToolType, BaseAdapter] = {}
    
    @classmethod
    def register_adapter(cls, tool_type: TestToolType, adapter_class: Type[BaseAdapter]):
        """
        注册适配器类
        
        Args:
            tool_type: 测试工具类型
            adapter_class: 适配器类
        """
        cls._adapters[tool_type] = adapter_class
        logger.info(f"注册适配器: {tool_type.value} -> {adapter_class.__name__}")
    
    @classmethod
    def create_adapter(cls, tool_type: TestToolType, config: Optional[Dict[str, Any]] = None) -> BaseAdapter:
        """
        创建适配器实例
        
        Args:
            tool_type: 测试工具类型
            config: 适配器配置
            
        Returns:
            适配器实例
            
        Raises:
            ValueError: 不支持的测试工具类型
            RuntimeError: 适配器创建失败
        """
        # 检查是否已注册
        if tool_type not in cls._adapters:
            # 尝试延迟加载
            cls._load_adapter(tool_type)
        
        if tool_type not in cls._adapters:
            raise ValueError(f"不支持的测试工具类型: {tool_type.value}")
        
        try:
            adapter_class = cls._adapters[tool_type]
            adapter = adapter_class(config or {})
            logger.info(f"创建适配器成功: {tool_type.value}")
            return adapter
        except Exception as e:
            logger.error(f"创建适配器失败: {tool_type.value}, 错误: {e}")
            raise RuntimeError(f"创建适配器失败: {e}")
    
    @classmethod
    def get_adapter(cls, tool_type: TestToolType, config: Optional[Dict[str, Any]] = None, 
                    singleton: bool = True) -> BaseAdapter:
        """
        获取适配器实例（支持单例模式）
        
        Args:
            tool_type: 测试工具类型
            config: 适配器配置
            singleton: 是否使用单例模式
            
        Returns:
            适配器实例
        """
        if singleton and tool_type in cls._instances:
            return cls._instances[tool_type]
        
        adapter = cls.create_adapter(tool_type, config)
        
        if singleton:
            cls._instances[tool_type] = adapter
        
        return adapter
    
    @classmethod
    def _load_adapter(cls, tool_type: TestToolType):
        """
        延迟加载适配器模块
        
        Args:
            tool_type: 测试工具类型
        """
        try:
            if tool_type == TestToolType.CANOE:
                from .canoe.adapter import CANoeAdapter
                cls.register_adapter(tool_type, CANoeAdapter)
            elif tool_type == TestToolType.TSMASTER:
                from .tsmaster_adapter import TSMasterAdapter
                cls.register_adapter(tool_type, TSMasterAdapter)
            elif tool_type == TestToolType.TTWORKBENCH:
                from .ttworkbench_adapter import TTworkbenchAdapter
                cls.register_adapter(tool_type, TTworkbenchAdapter)
        except ImportError as e:
            logger.warning(f"加载适配器失败: {tool_type.value}, 错误: {e}")
    
    @classmethod
    def get_supported_tools(cls) -> list:
        """
        获取支持的测试工具类型列表
        
        Returns:
            测试工具类型列表
        """
        # 确保所有适配器都尝试加载
        for tool_type in TestToolType:
            if tool_type not in cls._adapters:
                cls._load_adapter(tool_type)
        
        return list(cls._adapters.keys())
    
    @classmethod
    def is_tool_supported(cls, tool_type: TestToolType) -> bool:
        """
        检查测试工具是否受支持
        
        Args:
            tool_type: 测试工具类型
            
        Returns:
            是否受支持
        """
        if tool_type not in cls._adapters:
            cls._load_adapter(tool_type)
        
        return tool_type in cls._adapters
    
    @classmethod
    def release_adapter(cls, tool_type: TestToolType):
        """
        释放适配器实例
        
        Args:
            tool_type: 测试工具类型
        """
        if tool_type in cls._instances:
            try:
                adapter = cls._instances[tool_type]
                if hasattr(adapter, 'disconnect'):
                    adapter.disconnect()
            except Exception as e:
                logger.warning(f"释放适配器失败: {tool_type.value}, 错误: {e}")
            finally:
                del cls._instances[tool_type]
                logger.info(f"释放适配器: {tool_type.value}")
    
    @classmethod
    def release_all_adapters(cls):
        """释放所有适配器实例"""
        for tool_type in list(cls._instances.keys()):
            cls.release_adapter(tool_type)


# 便捷函数
def create_adapter(tool_type: str, config: Optional[Dict[str, Any]] = None) -> BaseAdapter:
    """
    创建适配器实例（便捷函数）
    
    Args:
        tool_type: 测试工具类型字符串
        config: 适配器配置
        
    Returns:
        适配器实例
    """
    try:
        tool_enum = TestToolType(tool_type.lower())
    except ValueError:
        raise ValueError(f"不支持的测试工具类型: {tool_type}")
    
    return AdapterFactory.create_adapter(tool_enum, config)


def get_adapter(tool_type: str, config: Optional[Dict[str, Any]] = None) -> BaseAdapter:
    """
    获取适配器实例（便捷函数，使用单例模式）
    
    Args:
        tool_type: 测试工具类型字符串
        config: 适配器配置
        
    Returns:
        适配器实例
    """
    try:
        tool_enum = TestToolType(tool_type.lower())
    except ValueError:
        raise ValueError(f"不支持的测试工具类型: {tool_type}")
    
    return AdapterFactory.get_adapter(tool_enum, config, singleton=True)
