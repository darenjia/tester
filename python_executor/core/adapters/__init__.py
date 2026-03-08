"""
测试工具适配器模块

提供统一的测试工具接口适配器，支持CANoe、TSMaster、TTworkbench等多种测试工具
"""

import logging
from .base_adapter import BaseTestAdapter, TestToolType, AdapterStatus
from .canoe import CANoeAdapter
from .tsmaster_adapter import TSMasterAdapter
from .ttworkbench_adapter import TTworkbenchAdapter
from .adapter_wrapper import AdapterWrapper

__all__ = [
    'BaseTestAdapter',
    'TestToolType',
    'AdapterStatus',
    'CANoeAdapter',
    'TSMasterAdapter',
    'TTworkbenchAdapter',
    'AdapterWrapper',
    'create_adapter',
    'create_adapter_with_wrapper',
    'AdapterFactory',
]


class AdapterFactory:
    """
    适配器工厂（增强版）
    
    提供适配器实例的创建和管理功能，支持单例模式。
    """
    
    # 适配器注册表
    _registry = {
        TestToolType.CANOE: CANoeAdapter,
        TestToolType.TSMASTER: TSMasterAdapter,
        TestToolType.TTWORKBENCH: TTworkbenchAdapter,
    }
    
    # 单例实例缓存
    _instances: dict = {}
    
    # 日志记录器
    logger = logging.getLogger(__name__)
    
    @classmethod
    def create_adapter(cls, 
                      tool_type: TestToolType, 
                      config: dict = None,
                      singleton: bool = True) -> BaseTestAdapter:
        """
        创建适配器实例
        
        Args:
            tool_type: 测试工具类型
            config: 适配器配置字典
            singleton: 是否使用单例模式（默认True）
            
        Returns:
            适配器实例
            
        Raises:
            ValueError: 不支持的测试工具类型
            
        Example:
            >>> # 使用单例模式（推荐）
            >>> adapter1 = AdapterFactory.create_adapter(TestToolType.CANOE)
            >>> adapter2 = AdapterFactory.create_adapter(TestToolType.CANOE)
            >>> assert adapter1 is adapter2  # True
            >>> 
            >>> # 不使用单例模式
            >>> adapter3 = AdapterFactory.create_adapter(
            ...     TestToolType.CANOE, 
            ...     singleton=False
            ... )
            >>> assert adapter1 is not adapter3  # True
        """
        if tool_type not in cls._registry:
            raise ValueError(f"不支持的测试工具类型: {tool_type}")
        
        # 单例模式
        if singleton:
            # 生成缓存键（基于工具类型和配置）
            config_key = str(sorted(config.items())) if config else "default"
            cache_key = f"{tool_type.value}_{hash(config_key)}"
            
            if cache_key not in cls._instances:
                adapter_class = cls._registry[tool_type]
                cls._instances[cache_key] = adapter_class(config)
            
            return cls._instances[cache_key]
        
        # 新实例
        adapter_class = cls._registry[tool_type]
        return adapter_class(config)
    
    @classmethod
    def register_adapter(cls, 
                        tool_type: TestToolType, 
                        adapter_class: type) -> None:
        """
        注册新的适配器类型
        
        Args:
            tool_type: 测试工具类型
            adapter_class: 适配器类
            
        Example:
            >>> from my_adapter import MyAdapter
            >>> AdapterFactory.register_adapter(TestToolType.CANOE_CSHARP, MyAdapter)
        """
        cls._registry[tool_type] = adapter_class
        cls.logger.info(f"已注册适配器: {tool_type.value} -> {adapter_class.__name__}")
    
    @classmethod
    def unregister_adapter(cls, tool_type: TestToolType) -> None:
        """
        注销适配器类型
        
        Args:
            tool_type: 测试工具类型
        """
        if tool_type in cls._registry:
            del cls._registry[tool_type]
            # 清除相关实例缓存
            keys_to_remove = [k for k in cls._instances.keys() 
                            if k.startswith(f"{tool_type.value}_")]
            for key in keys_to_remove:
                del cls._instances[key]
    
    @classmethod
    def get_registered_types(cls) -> list:
        """
        获取所有已注册的适配器类型
        
        Returns:
            适配器类型列表
        """
        return list(cls._registry.keys())
    
    @classmethod
    def clear_instances(cls) -> None:
        """清除所有单例实例缓存"""
        cls._instances.clear()
    
    @classmethod
    def get_instance_count(cls) -> int:
        """获取当前缓存的实例数量"""
        return len(cls._instances)


# 便捷函数（保持向后兼容）
def create_adapter(tool_type: TestToolType, config: dict = None) -> BaseTestAdapter:
    """
    创建适配器实例（便捷函数）
    
    默认使用单例模式。
    
    Args:
        tool_type: 测试工具类型
        config: 适配器配置
        
    Returns:
        适配器实例
        
    Raises:
        ValueError: 不支持的测试工具类型
        
    Example:
        >>> adapter = create_adapter(TestToolType.CANOE, {"start_timeout": 60})
    """
    return AdapterFactory.create_adapter(tool_type, config, singleton=True)


def create_adapter_with_wrapper(tool_type: TestToolType, config: dict = None) -> AdapterWrapper:
    """
    创建带包装器的适配器实例
    
    返回的包装器提供与原有Controller类相同的接口
    
    Args:
        tool_type: 测试工具类型
        config: 适配器配置
        
    Returns:
        AdapterWrapper实例
        
    Example:
        >>> controller = create_adapter_with_wrapper(TestToolType.CANOE)
        >>> controller.connect()
        >>> controller.open_configuration("test.cfg")
    """
    adapter = AdapterFactory.create_adapter(tool_type, config, singleton=True)
    return AdapterWrapper(adapter)
