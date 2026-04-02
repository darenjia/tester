"""TTworkbench execution strategy.

TTworkbench Runtime Adapter Contract Implementation
--------------------------------------------------

Capability Requirements:
- configuration: Load .cfg files
- measurement: Start/stop TTworkbench execution
- ttworkbench_execution: Execute .clf test files

Execution Flow:
1. Load configuration via configuration.load(config_path)
2. Start execution via measurement.start()
3. Execute cases via ttworkbench_execution.execute_clf() or execute_batch()
4. Stop execution in finally block
5. Return ExecutionOutcome or list[TestResult]

Note: The 'artifact' capability is registered in some adapters but is NOT
part of the main execution path. Report collection is handled via execution
capability return values which contain report paths in 'details'.
"""

from typing import Any, Optional

from models.result import ExecutionOutcome
from models.task import TestResult

from .base import ExecutionStrategy


class TTworkbenchExecutionStrategy(ExecutionStrategy):
    """Strategy for TTworkbench execution flow.

    Follows the Runtime Adapter Contract defined in base.py.
    """

    strategy_name = "ttworkbench"

    def prepare(self, plan: Any, adapter: Any) -> tuple[bool, Optional[str]]:
        """Validate TTworkbench-specific capabilities.

        Required capabilities:
        - configuration: Load .cfg files
        - measurement: Start/stop execution
        - ttworkbench_execution: Execute .clf files

        Note: 'artifact' capability is NOT required for main execution path.
        """
        has_configuration = adapter.get_capability("configuration") is not None
        has_measurement = adapter.get_capability("measurement") is not None
        has_execution = adapter.get_capability("ttworkbench_execution") is not None

        if not has_configuration or not has_measurement:
            return False, "Missing TTworkbench capability: configuration/measurement"
        if not has_execution:
            return False, "Missing TTworkbench capability: ttworkbench_execution"
        return True, None

    def _load_configuration(
        self, adapter: Any, config_path: Optional[str]
    ) -> None:
        """Load TTworkbench configuration via configuration capability."""
        if not config_path:
            return
        configuration_capability = adapter.get_capability("configuration")
        if configuration_capability is None or not configuration_capability.load(config_path):
            raise RuntimeError(f"failed to load TTworkbench configuration: {config_path}")

    def _start_measurement(self, adapter: Any) -> None:
        """Start TTworkbench execution via measurement capability."""
        measurement_capability = adapter.get_capability("measurement")
        if measurement_capability is None or not measurement_capability.start():
            raise RuntimeError("failed to start TTworkbench execution")

    def _stop_measurement(self, adapter: Any) -> None:
        """Stop TTworkbench execution via measurement capability."""
        measurement_capability = adapter.get_capability("measurement")
        if measurement_capability is not None:
            measurement_capability.stop()

    def _execute_case(
        self, case: Any, execution_capability: Any, task_id: str
    ) -> TestResult:
        """Execute a single TTworkbench case."""
        case_type = getattr(case, "case_type", "") or "clf_test"
        params = getattr(case, "execution_params", {}) or {}

        if case_type == "clf_test":
            clf_file = params.get("clf_file")
            if not clf_file:
                raise RuntimeError("TTworkbench clf_test case missing clf_file")
            raw_result = execution_capability.execute_clf(clf_file, task_id=task_id)
        elif case_type == "batch_test":
            clf_files = params.get("clf_files") or []
            if not clf_files:
                raise RuntimeError("TTworkbench batch_test case missing clf_files")
            raw_result = execution_capability.execute_batch(clf_files, task_id=task_id)
        else:
            raise RuntimeError(
                f"TTworkbenchExecutionStrategy does not support case_type: {case_type}"
            )

        verdict = raw_result.get("verdict") or (
            "PASS" if raw_result.get("status") == "passed" else "FAIL"
        )
        passed = verdict.upper() in {"PASS", "PASSED", "SUCCESS"}
        return TestResult(
            name=getattr(case, "case_name", ""),
            type=case_type,
            passed=passed,
            verdict=verdict,
            details=raw_result,
        )

    def _collect_results(
        self,
        collector: Any,
        results: list[TestResult],
    ) -> None:
        """Collect results to collector if available."""
        if collector is not None:
            for result in results:
                collector.add_test_result(result)

    def run(
        self,
        plan: Any,
        adapter: Any,
        collector: Any = None,
        executor: Any = None,
        config_path: Optional[str] = None,
    ) -> ExecutionOutcome | list[TestResult]:
        """Execute TTworkbench test cases following the Runtime Adapter Contract.

        Execution Flow:
        1. Load configuration (optional)
        2. Start measurement/execution
        3. Execute each case (clf_test or batch_test)
        4. Stop measurement in finally block
        5. Return ExecutionOutcome or list[TestResult]
        """
        configuration_capability = adapter.get_capability("configuration")
        measurement_capability = adapter.get_capability("measurement")
        execution_capability = adapter.get_capability("ttworkbench_execution")

        # Use collector from executor if not provided directly
        runtime_collector = collector or getattr(executor, "current_collector", None)

        self._load_configuration(adapter, config_path)
        self._start_measurement(adapter)

        try:
            results: list[TestResult] = []
            task_id = getattr(plan, "task_no", "")

            for case in getattr(plan, "cases", []) or []:
                test_result = self._execute_case(case, execution_capability, task_id)
                results.append(test_result)
                self._collect_results(runtime_collector, [test_result])

            # Return ExecutionOutcome if collector available, else list[TestResult]
            if runtime_collector is not None:
                return runtime_collector.finalize(status="completed")
            return results

        finally:
            self._stop_measurement(adapter)
