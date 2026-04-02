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

    def _start_measurement(self, adapter: Any) -> bool:
        """Start TSMaster measurement via measurement capability.

        Returns:
            True if measurement was started, False if not available or failed
        """
        measurement_capability = adapter.get_capability("measurement")
        if measurement_capability is None:
            return False  # No measurement capability, nothing to start

        try:
            started = measurement_capability.start()
            return bool(started)
        except Exception:
            return False

    def _stop_measurement(self, adapter: Any, started_by_strategy: bool) -> None:
        """Stop TSMaster measurement via measurement capability.

        Args:
            adapter: The TSMaster adapter
            started_by_strategy: Only stop if the strategy started the measurement
        """
        if not started_by_strategy:
            return

        measurement_capability = adapter.get_capability("measurement")
        if measurement_capability is not None:
            try:
                measurement_capability.stop()
            except Exception:
                pass

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

    def _report_items(self, report_info: Any, selected_cases: list, aggregate_only: bool = False) -> list[TestResult]:
        """Extract TestResult items from report info.

        Args:
            report_info: Report information from get_report_info()
            selected_cases: List of selected case identifiers for fallback mapping
            aggregate_only: If True, per-case details were unavailable and we must use fallback

        Returns:
            List of TestResult objects derived from report info or deterministic fallback
        """
        if not report_info:
            return []

        raw_items = report_info.get("results") or report_info.get("details") or []

        # If we have per-case details, use them directly
        if raw_items and not aggregate_only:
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

        # Fallback: derive results from aggregate stats when per-case details unavailable
        return self._fallback_results(report_info, selected_cases)

    def _fallback_results(self, report_info: Any, selected_cases: list) -> list[TestResult]:
        """Create deterministic TestResult objects from aggregate stats.

        When TSMaster only provides aggregate statistics without per-case details,
        this method derives stable per-case results using a deterministic fallback:
        - Distribute passed/failed counts across selected cases in case_no order
        - Include report paths as artifacts for traceability

        Args:
            report_info: Report info dict with aggregate stats and report paths
            selected_cases: List of selected case identifiers

        Returns:
            Deterministic list of TestResult objects
        """
        passed = report_info.get("passed", 0)
        failed = report_info.get("failed", 0)
        total = report_info.get("total", passed + failed)
        report_path = report_info.get("report_path")
        testdata_path = report_info.get("testdata_path")

        # Build case list from selected cases, sorted for deterministic ordering
        case_list = sorted(selected_cases, key=lambda x: getattr(x, "case_no", None) or getattr(x, "name", "") or "")

        # If no cases available, create a single aggregate result
        if not case_list:
            overall_passed = passed > 0 and failed == 0
            return [
                TestResult(
                    name="aggregate",
                    type="test_module",
                    passed=overall_passed,
                    verdict="PASS" if overall_passed else "FAIL",
                    details={
                        "passed": passed,
                        "failed": failed,
                        "total": total,
                        "report_path": report_path,
                        "testdata_path": testdata_path,
                        "fallback": True,
                    },
                )
            ]

        # Distribute passed/failed counts deterministically across cases
        results: list[TestResult] = []
        all_items = list(case_list)

        # Sort cases by name for deterministic distribution
        def case_sort_key(item):
            return getattr(item, "case_no", None) or getattr(item, "name", "") or ""

        sorted_cases = sorted(all_items, key=case_sort_key)
        total_cases = len(sorted_cases)

        # Determine pass/fail distribution
        # Use passed ratio to determine how many should pass, but ensure consistency
        pass_count = min(passed, total_cases)
        fail_count = min(failed, total_cases - pass_count)

        for index, case in enumerate(sorted_cases):
            case_name = getattr(case, "case_no", None) or getattr(case, "name", None) or getattr(case, "case_name", None) or "unknown"
            case_type = getattr(case, "case_type", None) or "test_module"

            # Distribute: first fail_count cases fail, rest pass
            is_passed = index >= fail_count

            results.append(
                TestResult(
                    name=case_name,
                    type=case_type,
                    passed=is_passed,
                    verdict="PASS" if is_passed else "FAIL",
                    details={
                        "report_path": report_path,
                        "testdata_path": testdata_path,
                        "aggregate_passed": passed,
                        "aggregate_failed": failed,
                        "aggregate_total": total,
                        "fallback": True,
                    },
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
        2. Start measurement if not already running
        3. Build case selection from plan
        4. Start execution
        5. Wait for completion
        6. Get report info
        7. Stop measurement only if started by strategy
        8. Return ExecutionOutcome or list[TestResult]
        """
        self._load_configuration(adapter, config_path)

        # Use collector from executor if not provided directly
        runtime_collector = collector or getattr(executor, "current_collector", None)

        # Track whether we started the measurement (for ownership tracking)
        measurement_started_by_strategy = False

        try:
            capability = self._execution_capability(adapter)
            plan_cases = self._plan_cases(plan)
            selected_cases = self._build_case_selection(plan, capability)
            timeout = self._timeout_for(plan)

            # Step 2: Start measurement before execution if not already running
            # This ensures simulation/measurement is active during test execution
            measurement_started_by_strategy = self._start_measurement(adapter)

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

            # Check if per-case details are available in report_info
            has_per_case_details = bool(report_info.get("results") or report_info.get("details"))

            # Extract results - pass plan_cases for fallback derivation
            results = self._report_items(report_info, plan_cases, aggregate_only=not has_per_case_details)
            self._append_results(runtime_collector, results)

            if runtime_collector is not None:
                return runtime_collector.finalize(status="completed")
            return results

        finally:
            # Only stop measurement if we started it (ownership tracking)
            self._stop_measurement(adapter, measurement_started_by_strategy)
