"""Base execution strategy abstraction."""

from abc import ABC, abstractmethod
from typing import Any


class ExecutionStrategy(ABC):
    """Base class for tool-specific execution strategies."""

    strategy_name = "base"

    @abstractmethod
    def prepare(self, plan: Any, adapter: Any):
        """Validate required capabilities before execution."""

    @abstractmethod
    def run(self, plan: Any, adapter: Any, executor: Any = None, config_path: str | None = None) -> Any:
        """Execute a plan using the provided adapter."""
