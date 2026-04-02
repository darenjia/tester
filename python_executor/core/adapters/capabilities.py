"""
Shared capability registry helpers for adapters.

CAPABILITY NAMES AND MEANINGS
============================

This module defines the audited capability names used across all adapters.
Each capability represents a specific tool-specific ability that strategies
use to interact with test tools.

## Authoritative Capability List

| Capability Name          | Purpose                              | Tools              |
|-------------------------|--------------------------------------|--------------------|
| configuration           | Load .cfg configuration files        | CANoe, TSMaster, TTworkbench |
| measurement            | Start/stop test measurement         | CANoe, TSMaster, TTworkbench |
| test_module            | Execute CANoe test modules           | CANoe              |
| tsmaster_execution     | TSMaster case execution + reporting | TSMaster           |
| ttworkbench_execution   | TTworkbench .clf execution          | TTworkbench        |
| artifact               | Collect report artifacts             | (registered but NOT used by strategies) |
| rpc_execution          | CANoe RPC-based execution            | CANoe (via RPCExecutionCapability) |

## Capability Contract

Each capability is a dataclass with specific methods:

### ConfigurationCapability
- `load(config_path: str) -> bool`: Load configuration file

### MeasurementCapability
- `start() -> bool`: Start measurement
- `stop() -> bool`: Stop measurement

### TestModuleCapability (CANoe)
- `execute_module(module_name: str, timeout: int) -> dict`: Execute test module
- `list_modules() -> list[str]`: List available modules (optional)

### TSMasterExecutionCapability
- `build_case_selection(plan) -> str`: Build case selection string
- `start_execution(selected_cases: str) -> bool`: Start execution
- `wait_for_completion(timeout: int) -> bool`: Wait for completion
- `get_report_info() -> dict`: Get report information

### TTworkbenchExecutionCapability
- `execute_clf(clf_file: str, task_id: str) -> dict`: Execute single .clf
- `execute_batch(clf_files: list[str], task_id: str) -> dict`: Execute batch

### ArtifactCapability
- `collect() -> dict`: Collect report artifacts
- **NOTE**: This capability is registered in some adapters but is NOT
  part of the main execution path. Strategies get artifact info from
  execution capability return values (e.g., report_info from get_report_info()).

## Legacy Capabilities

The following are LEGACY ONLY and should not be used in new execution paths:
- `BaseTestAdapter.execute_test_item()`: Deprecated direct test item execution
- Adapter methods that bypass strategy layer

## Capability Discovery

Strategies discover capabilities via:
    adapter.get_capability("capability_name")

This returns the capability object or None if not registered.
"""

from abc import ABC
from dataclasses import dataclass
from typing import Any, Dict, Iterable, Optional


CapabilityName = str
Capability = Any


@dataclass(frozen=True)
class ConfigurationCapability:
    """Load configuration files (.cfg).

    Methods:
        load(config_path: str) -> bool: Load configuration file, returns True on success
    """

    load: Any


@dataclass(frozen=True)
class MeasurementCapability:
    """Start and stop test measurement.

    Methods:
        start() -> bool: Start measurement, returns True on success
        stop() -> bool: Stop measurement, returns True on success
    """

    start: Any
    stop: Any


@dataclass(frozen=True)
class ArtifactCapability:
    """Collect report artifacts.

    NOTE: This capability exists but is NOT used by strategies in the main
    execution path. Artifact information is obtained from execution capability
    return values (report_info, details, etc.).

    Methods:
        collect() -> dict: Collect artifacts, returns dict with report paths
    """

    collect: Any


@dataclass(frozen=True)
class TestModuleCapability:
    """Execute CANoe test modules.

    Used by CANoeExecutionStrategy.

    Methods:
        execute_module(module_name: str, timeout: int) -> dict:
            Execute a test module, returns dict with verdict and details
        list_modules() -> list[str]: List available modules (optional)
    """

    execute_module: Any
    list_modules: Any = None


@dataclass(frozen=True)
class RPCExecutionCapability:
    """CANoe RPC-based execution capability.

    NOTE: This is an alternative execution path. The primary CANoe
    execution uses test_module capability.

    Methods:
        start_execution(...) -> bool: Start execution
        wait_for_complete(...) -> bool: Wait for completion
        get_report_info() -> dict: Get report information
    """

    start_execution: Any
    wait_for_complete: Any
    get_report_info: Any


@dataclass(frozen=True)
class TSMasterExecutionCapability:
    """TSMaster case execution and reporting capability.

    Used by TSMasterExecutionStrategy.

    Methods:
        build_case_selection(plan) -> str: Build case selection string from plan
        start_execution(selected_cases: str) -> bool: Start execution
        wait_for_completion(timeout: int) -> bool: Wait for completion
        get_report_info() -> dict: Get report info with results list
    """

    build_case_selection: Any
    start_execution: Any
    wait_for_completion: Any
    get_report_info: Any


@dataclass(frozen=True)
class ProjectControlCapability:
    """Project control capability (reserved for future use).

    Methods:
        start_form(...): Start form
        stop_form(...): Stop form
    """

    start_form: Any
    stop_form: Any


@dataclass(frozen=True)
class TTworkbenchExecutionCapability:
    """TTworkbench .clf file execution capability.

    Used by TTworkbenchExecutionStrategy.

    Methods:
        execute_clf(clf_file: str, task_id: str) -> dict:
            Execute single .clf file, returns dict with verdict and details
        execute_batch(clf_files: list[str], task_id: str) -> dict:
            Execute batch of .clf files, returns dict with results array
    """

    execute_clf: Any
    execute_batch: Any


class CapabilityRegistryMixin(ABC):
    """Mixin that stores and resolves named adapter capabilities.

    All test tool adapters inherit this mixin to provide a consistent
    capability discovery interface for strategies.
    """

    _capabilities: Dict[CapabilityName, Capability]

    def register_capability(self, name: CapabilityName, capability: Capability) -> None:
        """Register a capability under a stable name.

        Args:
            name: Stable capability name (see audited list above)
            capability: Capability instance (dataclass or object with methods)
        """
        self._capabilities[name] = capability

    def get_capability(
        self, name: CapabilityName, default: Optional[Capability] = None
    ) -> Capability:
        """Look up a capability by name.

        Args:
            name: Capability name to look up
            default: Default value if capability not found

        Returns:
            The capability object, or default if not registered
        """
        return self._capabilities.get(name, default)

    def has_capability(self, name: CapabilityName) -> bool:
        """Return whether a capability has been registered.

        Args:
            name: Capability name to check

        Returns:
            True if capability is registered, False otherwise
        """
        return name in self._capabilities

    def list_capabilities(self) -> Iterable[CapabilityName]:
        """Return registered capability names.

        Returns:
            Tuple of registered capability names
        """
        return tuple(self._capabilities.keys())

    def clear_capabilities(self) -> None:
        """Remove all registered capabilities.

        Note: This is primarily for testing. In production, capabilities
        are registered once during adapter initialization.
        """
        self._capabilities.clear()
