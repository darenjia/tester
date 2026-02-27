"""
CANoe适配器模块

提供完整的CANoe测试工具适配功能，包括：
- COM接口包装
- 测试执行引擎
- 适配器主类

Example:
    >>> from core.adapters.canoe import CANoeAdapter
    >>> adapter = CANoeAdapter()
    >>> adapter.connect()
    >>> adapter.load_configuration("test.cfg")
    >>> result = adapter.execute_test_item({
    ...     "type": "self_check",
    ...     "config_path": "selfcheck.cfg"
    ... })
    >>> adapter.disconnect()
"""

from .com_wrapper import (
    CANoeCOMWrapper,
    CANoeVersion,
    DeviceInfo,
    CANoeError,
    CANoeConnectionError,
    CANoeConfigurationError,
    CANoeMeasurementError,
    CANoeVariableError,
    create_canoe_wrapper
)

from .test_engine import (
    CANoeTestEngine,
    TestStatus,
    TestCaseType,
    TestCaseResult,
    TestExecutionResult,
    create_test_engine
)

from .adapter import CANoeAdapter

__all__ = [
    # COM包装器
    'CANoeCOMWrapper',
    'CANoeVersion',
    'DeviceInfo',
    'CANoeError',
    'CANoeConnectionError',
    'CANoeConfigurationError',
    'CANoeMeasurementError',
    'CANoeVariableError',
    'create_canoe_wrapper',
    
    # 测试引擎
    'CANoeTestEngine',
    'TestStatus',
    'TestCaseType',
    'TestCaseResult',
    'TestExecutionResult',
    'create_test_engine',
    
    # 适配器
    'CANoeAdapter',
]
