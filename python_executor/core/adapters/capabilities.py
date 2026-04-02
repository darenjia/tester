"""Shared capability registry helpers for adapters."""

from abc import ABC
from dataclasses import dataclass
from typing import Any, Dict, Iterable, Optional


CapabilityName = str
Capability = Any


@dataclass(frozen=True)
class ConfigurationCapability:
    load: Any


@dataclass(frozen=True)
class MeasurementCapability:
    start: Any
    stop: Any


@dataclass(frozen=True)
class ArtifactCapability:
    collect: Any


@dataclass(frozen=True)
class TestModuleCapability:
    execute_module: Any
    list_modules: Any = None

@dataclass(frozen=True)
class RPCExecutionCapability:
    start_execution: Any
    wait_for_complete: Any
    get_report_info: Any


@dataclass(frozen=True)
class ProjectControlCapability:
    start_form: Any
    stop_form: Any


@dataclass(frozen=True)
class TTworkbenchExecutionCapability:
    execute_clf: Any
    execute_batch: Any


class CapabilityRegistryMixin(ABC):
    """Mixin that stores and resolves named adapter capabilities."""

    _capabilities: Dict[CapabilityName, Capability]

    def register_capability(self, name: CapabilityName, capability: Capability) -> None:
        """Register a capability under a stable name."""
        self._capabilities[name] = capability

    def get_capability(
        self, name: CapabilityName, default: Optional[Capability] = None
    ) -> Capability:
        """Look up a capability by name."""
        return self._capabilities.get(name, default)

    def has_capability(self, name: CapabilityName) -> bool:
        """Return whether a capability has been registered."""
        return name in self._capabilities

    def list_capabilities(self) -> Iterable[CapabilityName]:
        """Return registered capability names."""
        return tuple(self._capabilities.keys())

    def clear_capabilities(self) -> None:
        """Remove all registered capabilities."""
        self._capabilities.clear()
