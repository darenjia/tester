"""
系统检测模块
统一的环境检测和功能测试框架
"""

from .models import CheckResult, CheckSession, CheckStatus
from .base import BaseCheck
from .registry import check_registry, register_check
from .executor import CheckExecutor
from .history import CheckHistory

__all__ = [
    'CheckResult',
    'CheckSession',
    'CheckStatus',
    'BaseCheck',
    'check_registry',
    'register_check',
    'CheckExecutor',
    'CheckHistory',
]