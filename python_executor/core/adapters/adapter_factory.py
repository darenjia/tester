"""
适配器工厂模块

提供适配器创建、单例缓存和包装器构建的唯一真源。
"""
from __future__ import annotations

import logging
from typing import Any, Optional

from .adapter_wrapper import AdapterWrapper
from .base_adapter import AdapterStatus, BaseTestAdapter, TestToolType
from .canoe import CANoeAdapter
from .tsmaster_adapter import TSMasterAdapter
from .ttworkbench_adapter import TTworkbenchAdapter

logger = logging.getLogger(__name__)


class AdapterFactory:
    """统一的适配器工厂。"""

    _registry = {
        TestToolType.CANOE: CANoeAdapter,
        TestToolType.TSMASTER: TSMasterAdapter,
        TestToolType.TTWORKBENCH: TTworkbenchAdapter,
    }
    _instances: dict[str, BaseTestAdapter] = {}

    @classmethod
    def _normalize_tool_type(cls, tool_type: TestToolType | str) -> TestToolType:
        if isinstance(tool_type, TestToolType):
            return tool_type
        normalized = str(tool_type).strip().lower()
        try:
            return TestToolType(normalized)
        except ValueError as exc:
            raise ValueError(f"不支持的测试工具类型: {tool_type}") from exc

    @classmethod
    def _cache_key(cls, tool_type: TestToolType, config: Optional[dict[str, Any]]) -> str:
        config_key = str(sorted((config or {}).items()))
        return f"{tool_type.value}_{hash(config_key)}"

    @classmethod
    def create_adapter(
        cls,
        tool_type: TestToolType | str,
        config: Optional[dict[str, Any]] = None,
        singleton: bool = True,
    ) -> BaseTestAdapter:
        normalized_type = cls._normalize_tool_type(tool_type)
        if normalized_type not in cls._registry:
            raise ValueError(f"不支持的测试工具类型: {normalized_type.value}")

        if singleton:
            cache_key = cls._cache_key(normalized_type, config)
            if cache_key not in cls._instances:
                cls._instances[cache_key] = cls._registry[normalized_type](config or {})
            return cls._instances[cache_key]

        return cls._registry[normalized_type](config or {})

    @classmethod
    def get_adapter(
        cls,
        tool_type: TestToolType | str,
        config: Optional[dict[str, Any]] = None,
        singleton: bool = True,
    ) -> BaseTestAdapter:
        return cls.create_adapter(tool_type, config=config, singleton=singleton)

    @classmethod
    def create_adapter_with_wrapper(
        cls,
        tool_type: TestToolType | str,
        config: Optional[dict[str, Any]] = None,
        singleton: bool = True,
    ) -> AdapterWrapper:
        return AdapterWrapper(
            cls.create_adapter(tool_type, config=config, singleton=singleton)
        )

    @classmethod
    def register_adapter(cls, tool_type: TestToolType | str, adapter_class: type) -> None:
        normalized_type = cls._normalize_tool_type(tool_type)
        cls._registry[normalized_type] = adapter_class
        logger.info(f"已注册适配器: {normalized_type.value} -> {adapter_class.__name__}")

    @classmethod
    def unregister_adapter(cls, tool_type: TestToolType | str) -> None:
        normalized_type = cls._normalize_tool_type(tool_type)
        cls._registry.pop(normalized_type, None)
        prefix = f"{normalized_type.value}_"
        for key in [key for key in cls._instances if key.startswith(prefix)]:
            cls._instances.pop(key, None)

    @classmethod
    def get_registered_types(cls) -> list[TestToolType]:
        return list(cls._registry.keys())

    @classmethod
    def clear_instances(cls) -> None:
        cls._instances.clear()

    @classmethod
    def get_instance_count(cls) -> int:
        return len(cls._instances)

    @classmethod
    def is_tool_supported(cls, tool_type: TestToolType | str) -> bool:
        try:
            normalized_type = cls._normalize_tool_type(tool_type)
        except ValueError:
            return False
        return normalized_type in cls._registry

    @classmethod
    def release_adapter(cls, tool_type: TestToolType | str) -> None:
        normalized_type = cls._normalize_tool_type(tool_type)
        prefix = f"{normalized_type.value}_"
        for key in [key for key in cls._instances if key.startswith(prefix)]:
            adapter = cls._instances.pop(key)
            try:
                if hasattr(adapter, "disconnect"):
                    adapter.disconnect()
            except Exception as exc:
                logger.warning(f"释放适配器失败: {normalized_type.value}, 错误: {exc}")

    @classmethod
    def release_all_adapters(cls) -> None:
        for tool_type in list(cls._registry.keys()):
            cls.release_adapter(tool_type)


def create_adapter(
    tool_type: TestToolType | str, config: Optional[dict[str, Any]] = None
) -> BaseTestAdapter:
    return AdapterFactory.create_adapter(tool_type, config=config, singleton=True)


def get_adapter(
    tool_type: TestToolType | str, config: Optional[dict[str, Any]] = None
) -> BaseTestAdapter:
    return AdapterFactory.get_adapter(tool_type, config=config, singleton=True)


def create_adapter_with_wrapper(
    tool_type: TestToolType | str, config: Optional[dict[str, Any]] = None
) -> AdapterWrapper:
    return AdapterFactory.create_adapter_with_wrapper(tool_type, config=config, singleton=True)


__all__ = [
    "AdapterFactory",
    "AdapterStatus",
    "AdapterWrapper",
    "BaseTestAdapter",
    "TestToolType",
    "create_adapter",
    "create_adapter_with_wrapper",
    "get_adapter",
]
