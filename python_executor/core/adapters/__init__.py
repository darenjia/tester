"""
测试工具适配器模块

对外保留原有包级导出，但内部统一转发到 adapter_factory 这一套真源。
"""

from .adapter_factory import (
    AdapterFactory,
    AdapterStatus,
    BaseTestAdapter,
    TestToolType,
    create_adapter,
    get_adapter,
)
from .canoe import CANoeAdapter
from .tsmaster_adapter import TSMasterAdapter
from .ttworkbench_adapter import TTworkbenchAdapter

__all__ = [
    "AdapterFactory",
    "AdapterStatus",
    "BaseTestAdapter",
    "CANoeAdapter",
    "TSMasterAdapter",
    "TTworkbenchAdapter",
    "TestToolType",
    "create_adapter",
    "get_adapter",
]
