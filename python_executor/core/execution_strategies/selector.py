"""Execution strategy selector."""

from typing import Any

from .canoe_strategy import CANoeExecutionStrategy
from .ttworkbench_strategy import TTworkbenchExecutionStrategy
from .tsmaster_strategy import TSMasterExecutionStrategy


class ExecutionStrategySelector:
    """Select a concrete execution strategy for a plan."""

    def select(self, plan: Any):
        tool_type = getattr(plan, "tool_type", None)
        normalized = str(tool_type).lower() if tool_type is not None else ""

        if normalized == "canoe":
            return CANoeExecutionStrategy()
        if normalized == "tsmaster":
            return TSMasterExecutionStrategy()
        if normalized == "ttworkbench":
            return TTworkbenchExecutionStrategy()

        raise ValueError(f"Unsupported tool type: {tool_type}")
