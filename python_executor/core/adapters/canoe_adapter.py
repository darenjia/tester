"""
Backward-compatible CANoe adapter import shim.

The implementation lives in ``core.adapters.canoe``.  This module keeps the
legacy ``core.adapters.canoe_adapter`` import path working for older scripts
and tests without duplicating adapter logic.
"""

from .canoe import *  # noqa: F401,F403
from .canoe.adapter import create_canoe_adapter
from .canoe import __all__ as canoe_all

__all__ = [*canoe_all, "create_canoe_adapter"]
