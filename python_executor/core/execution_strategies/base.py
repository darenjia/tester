"""
Base execution strategy abstraction.

RUNTIME ADAPTER CONTRACT
========================

This module defines the authoritative contract that all tool-specific
execution strategies (CANoe, TSMaster, TTworkbench) must follow.

## Contract Overview

Each strategy MUST:
1. Define `strategy_name` - stable identifier for the tool
2. Implement `prepare(plan, adapter)` - validate required capabilities
3. Implement `run(plan, adapter, collector, executor, config_path)` - execute plan

## Runtime Config Source

The executor (not the strategy) is responsible for providing runtime config:
- `config_path`: Path to the .cfg configuration file
- The strategy loads config via `adapter.get_capability("configuration")`

The adapter receives runtime config indirectly through the executor's
workspace setup (copying .cfg files, generating ParaInfo.ini, etc.)
before the strategy's run() method is called.

## Shared Execution Expectations

All strategies MUST follow this execution sequence:

    1. Start required runtime state (measurement/execution)
    2. Execute cases via tool-specific capability
    3. Wait for completion if applicable
    4. Collect results
    5. Stop runtime state (in finally block)
    6. Return ExecutionOutcome (via collector.finalize()) or list[TestResult]

## Timeout Enforcement Policy

Timeout enforcement is tool-specific and documented per strategy:

| Strategy    | Timeout Enforcement Point              | Notes                                              |
|-------------|---------------------------------------|----------------------------------------------------|
| CANoe       | Passed to execute_module()            | Synchronous per-module timeout                     |
| TSMaster    | Passed to wait_for_completion()       | Adapter internally uses operation_timeout at start |
| TTworkbench | Not currently enforced               | Timeout extraction not implemented                 |

Strategies SHOULD:
- Extract timeout from plan via `_timeout_for()` helper
- Pass timeout to the appropriate enforcement point (start or wait)
- Document where timeout is enforced if non-standard

## Return Value Contract

The `run()` method MUST return ONE of:
- `ExecutionOutcome` - when collector is provided (preferred)
- `list[TestResult]` - when no collector is available (legacy fallback)

The returned `ExecutionOutcome` MUST contain:
- `task_no`: Task identifier
- `status`: "completed" | "failed" | "timeout" | "error"
- `verdict`: "PASS" | "FAIL" | "TIMEOUT" | "ERROR"
- `case_results`: list of TestResult with stable passed/verdict
- `summary`: {"total", "passed", "failed", "passRate", ...}
- `report_metadata`: artifact paths for downstream reporting

## TestResult Contract

Each TestResult in case_results MUST have:
- `name`: Case identifier (string)
- `type`: Case type (e.g., "test_module", "clf_test")
- `passed`: bool - True if passed, False if failed
- `verdict`: string - "PASS" | "FAIL" | "BLOCK" | "SKIP" | "TIMEOUT" | "ERROR"
- `details` or `error`: Additional context (optional)

## Capability Names (Audited)

| Capability        | Purpose                        | Used By      |
|-------------------|--------------------------------|--------------|
| configuration     | Load .cfg files                | CANoe, TSMaster, TTworkbench |
| measurement       | Start/stop measurement         | CANoe, TSMaster, TTworkbench |
| test_module       | Execute CANoe test modules     | CANoe        |
| tsmaster_execution| TSMaster case execution       | TSMaster     |
| ttworkbench_execution | TTworkbench execution     | TTworkbench  |
| artifact          | Collect report artifacts       | TTworkbench (registered but not used) |
| rpc_execution     | CANoe RPC-based execution      | CANoe (RPCExecutionCapability) |

## Legacy Compatibility

The following are LEGACY ONLY and NOT part of the main business path:
- `BaseTestAdapter.execute_test_item()` - deprecated, returns error
- Direct adapter methods bypassing strategy layer

These exist only for backward compatibility and should never be called
in new execution paths.
"""

from abc import ABC, abstractmethod
from typing import Any, Optional

from models.result import ExecutionOutcome
from models.task import TestResult


class ExecutionStrategy(ABC):
    """Base class for tool-specific execution strategies.

    All concrete strategies MUST follow the Runtime Adapter Contract
    as documented in this module's docstring.
    """

    strategy_name: str = "base"

    @abstractmethod
    def prepare(self, plan: Any, adapter: Any) -> tuple[bool, Optional[str]]:
        """Validate required capabilities before execution.

        Args:
            plan: Execution plan containing cases and configuration
            adapter: Tool adapter with registered capabilities

        Returns:
            Tuple of (success: bool, error_message: Optional[str])
            - (True, None) if all required capabilities are available
            - (False, "Missing X capability") if a required capability is missing
        """

    @abstractmethod
    def run(
        self,
        plan: Any,
        adapter: Any,
        collector: Any = None,
        executor: Any = None,
        config_path: Optional[str] = None,
    ) -> ExecutionOutcome | list[TestResult]:
        """Execute a plan using the provided adapter.

        This method MUST follow the shared execution expectations:
        1. Start required runtime state (measurement/execution)
        2. Execute cases via tool-specific capability
        3. Wait for completion if applicable
        4. Collect results to collector if provided
        5. Stop runtime state (in finally block)
        6. Return ExecutionOutcome (preferred) or list[TestResult]

        Args:
            plan: Execution plan with cases, timeout, task_no
            adapter: Tool adapter with registered capabilities
            collector: ResultCollector for collecting test results (optional)
            executor: Task executor with current_collector (optional)
            config_path: Path to .cfg configuration file (optional)

        Returns:
            ExecutionOutcome with full result details (preferred), or
            list[TestResult] if no collector provided (legacy fallback)
        """
        ...
