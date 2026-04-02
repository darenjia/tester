"""TSMaster execution strategy.

TSMaster Runtime Adapter Contract Implementation
------------------------------------------------

Capability Requirements:
- configuration: Load .cfg files (optional)
- measurement: Stop measurement (optional cleanup)
- tsmaster_execution: Execute test cases and get report info

Execution Flow:
1. Load configuration via configuration.load(config_path) if provided
2. Start execution via tsmaster_execution.start_execution(selected_cases)
3. Wait for completion via tsmaster_execution.wait_for_completion(timeout)
4. Collect report info via tsmaster_execution.get_report_info()
5. Stop measurement in finally block (if measurement capability exists)
6. Return ExecutionOutcome or list[TestResult]

Note: TSMaster handles configuration differently - case selection is built
from ini_config in case mappings, not direct configuration files.
"""

from __future__ import annotations

from typing import Any, Optional

from core.case_mapping_manager import get_case_mapping_manager
from core.result_collector import ResultCollector
from models.result import ExecutionOutcome
from models.task import TestResult

from .base import ExecutionStrategy


class TSMasterExecutionStrategy(ExecutionStrategy):
    """Strategy for TSMaster execution flow.

    Follows the Runtime Adapter Contract defined in base.py.
    """

    strategy_name = "tsmaster"

    def prepare(self, plan: Any, adapter: Any) -> tuple[bool, Optional[str]]:
        """Validate TSMaster-specific capabilities.

        Required capabilities:
        - tsmaster_execution: Execute cases and get reports

        Optional capabilities:
        - configuration: Load configuration files
        - measurement: Stop measurement cleanup
        """
        if adapter.get_capability("tsmaster_execution") is None:
            return False, "Missing TSMaster capability: tsmaster_execution"
        return True, None

    def _load_configuration(self, adapter: Any, config_path: Optional[str]) -> None:
        """Load TSMaster configuration via configuration capability."""
        if not config_path:
            return
        configuration_capability = adapter.get_capability("configuration")
        if configuration_capability is None or not configuration_capability.load(config_path):
            raise RuntimeError(f"failed to load TSMaster configuration: {config_path}")

    def _stop_measurement(self, adapter: Any) -> None:
        """Stop TSMaster measurement via measurement capability."""
        measurement_capability = adapter.get_capability("measurement")
        if measurement_capability is not None:
            measurement_capability.stop()

    def _timeout_for(self, plan: Any) -> int:
        """Extract timeout from plan."""
        timeout = getattr(plan, "timeout_seconds", None)
        if timeout is None:
            timeout = getattr(plan, "timeout", 0)
        return int(timeout or 0)

    def _plan_cases(self, plan: Any) -> list[Any]:
        """Get cases from plan."""
        return list(getattr(plan, "cases", []) or getattr(plan, "test_items", []) or [])

    def _case_selection_string(self, plan: Any) -> str:
        """Build case selection string from plan cases using case mappings."""
        mapping_manager = get_case_mapping_manager()
        cases = []
        for item in self._plan_cases(plan):
            case_no = getattr(item, "case_no", None) or getattr(item, "caseNo", None)
            if not case_no:
                continue
            mapping = mapping_manager.get_mapping(case_no)
            if mapping and getattr(mapping, "ini_config", None):
                cases.append(mapping.ini_config)
            else:
                cases.append(f"{case_no}=1")
        return ",".join(cases)

    def _execution_capability(self, adapter: Any) -> Any:
        """Get TSMaster execution capability."""
        capability = adapter.get_capability("tsmaster_execution")
        if capability is None:
            raise RuntimeError("TSMaster execution capability is not available")
        return capability

    def _build_case_selection(self, plan: Any, capability: Any) -> str:
        """Build case selection string."""
        if hasattr(capability, "build_case_selection"):
            return capability.build_case_selection(plan)
        return self._case_selection_string(plan)

    def _start_execution(self, capability: Any, selected_cases: str) -> bool:
        """Start TSMaster execution.

        Note: The timeout parameter is NOT passed to start_execution because
        TSMaster enforces timeout via wait_for_completion(), not here.
        This is documented in the Runtime Adapter Contract.
        """
        return bool(capability.start_execution(selected_cases))

    def _wait_for_completion(self, capability: Any, timeout: int) -> bool:
        """Wait for TSMaster execution to complete.

        Args:
            capability: The tsmaster_execution capability
            timeout: Timeout in seconds for wait operation

        Returns:
            True if execution completed, False if timeout
        """
        waiter = getattr(capability, "wait_for_completion", None) or getattr(
            capability, "wait_for_complete", None
        )
        if waiter is None:
            return True
        return bool(waiter(timeout))

    def _report_items(self, report_info: Any) -> list[TestResult]:
        """Extract TestResult items from report info."""
        if not report_info:
            return []

        raw_items = report_info.get("results") or report_info.get("details") or []
        results: list[TestResult] = []
        for item in raw_items:
            if not isinstance(item, dict):
                continue

            verdict = item.get("verdict")
            passed = item.get("passed")
            if passed is None and verdict is not None:
                passed = str(verdict).upper() in {"PASS", "PASSED", "SUCCESS"}
            passed = bool(passed)
            if verdict is None:
                verdict = "PASS" if passed else "FAIL"

            results.append(
                TestResult(
                    name=item.get("case_no") or item.get("name") or item.get("case_name") or "unknown",
                    type="test_module",
                    passed=passed,
                    verdict=verdict,
                    details=item,
                )
            )
        return results

    def _failure_results(self, plan: Any, verdict: str, message: str) -> list[TestResult]:
        """Create failure TestResult for each case in plan."""
        results: list[TestResult] = []
        for item in self._plan_cases(plan):
            results.append(
                TestResult(
                    name=getattr(item, "case_no", None)
                    or getattr(item, "case_name", None)
                    or getattr(item, "name", None)
                    or "unknown",
                    type=getattr(item, "case_type", None) or "test_module",
                    passed=False,
                    verdict=verdict,
                    error=message,
                )
            )

        if not results:
            results.append(
                TestResult(
                    name=getattr(plan, "task_no", None) or getattr(plan, "taskNo", None) or "unknown",
                    type="test_module",
                    passed=False,
                    verdict=verdict,
                    error=message,
                )
            )

        return results

    def _append_results(self, collector: Optional[ResultCollector], results: list[TestResult]) -> None:
        """Append results to collector if available."""
        if collector is None:
            return
        for result in results:
            collector.add_test_result(result)

    def _finalize_with_status(
        self,
        runtime_collector: Optional[ResultCollector],
        status: str,
        error_message: Optional[str],
        results: list[TestResult],
    ) -> ExecutionOutcome | list[TestResult]:
        """Finalize with given status."""
        if runtime_collector is not None:
            runtime_collector.add_log("ERROR", error_message or f"TSMaster execution {status}")
            return runtime_collector.finalize(status=status, error_message=error_message)
        return results

    def run(
        self,
        plan: Any,
        adapter: Any,
        collector: Optional[ResultCollector] = None,
        executor: Any = None,
        config_path: Optional[str] = None,
    ) -> ExecutionOutcome | list[TestResult]:
        """Execute TSMaster test cases following the Runtime Adapter Contract.

        Execution Flow:
        1. Load configuration (optional)
        2. Build case selection from plan
        3. Start execution
        4. Wait for completion
        5. Get report info
        6. Stop measurement in finally block
        7. Return ExecutionOutcome or list[TestResult]
        """
        self._load_configuration(adapter, config_path)

        # Use collector from executor if not provided directly
        runtime_collector = collector or getattr(executor, "current_collector", None)

        try:
            capability = self._execution_capability(adapter)
            selected_cases = self._build_case_selection(plan, capability)
            timeout = self._timeout_for(plan)

            if not self._start_execution(capability, selected_cases):
                results = self._failure_results(
                    plan,
                    verdict="ERROR",
                    message="TSMaster execution failed to start",
                )
                self._append_results(runtime_collector, results)
                return self._finalize_with_status(
                    runtime_collector,
                    status="failed",
                    error_message="TSMaster execution failed to start",
                    results=results,
                )

            if not self._wait_for_completion(capability, timeout):
                results = self._failure_results(
                    plan,
                    verdict="TIMEOUT",
                    message="TSMaster execution timed out",
                )
                self._append_results(runtime_collector, results)
                return self._finalize_with_status(
                    runtime_collector,
                    status="timeout",
                    error_message="TSMaster execution timed out",
                    results=results,
                )

            report_info = getattr(capability, "get_report_info", lambda: None)()
            if not report_info:
                results = self._failure_results(
                    plan,
                    verdict="ERROR",
                    message="TSMaster report information is missing",
                )
                self._append_results(runtime_collector, results)
                return self._finalize_with_status(
                    runtime_collector,
                    status="failed",
                    error_message="TSMaster report information is missing",
                    results=results,
                )

            results = self._report_items(report_info)
            self._append_results(runtime_collector, results)

            if runtime_collector is not None:
                return runtime_collector.finalize(status="completed")
            return results

        finally:
            self._stop_measurement(adapter)
