"""CANoe execution strategy.

CANoe Runtime Adapter Contract Implementation
---------------------------------------------

Capability Requirements:
- configuration: Load .cfg files
- measurement: Start/stop CANoe measurement
- test_module: Execute CANoe test modules

Execution Flow:
1. Load configuration via configuration.load(config_path)
2. Start measurement via measurement.start()
3. Execute each case via test_module.execute_module(module_name, timeout)
4. Wait for completion (synchronous execution per module)
5. Stop measurement in finally block
6. Return ExecutionOutcome or list[TestResult]
"""

from typing import Any, Optional

from models.result import ExecutionOutcome
from models.task import TestResult

from .base import ExecutionStrategy


class CANoeExecutionStrategy(ExecutionStrategy):
    """Strategy for CANoe execution flow.

    Follows the Runtime Adapter Contract defined in base.py.
    """

    strategy_name = "canoe"

    def prepare(self, plan: Any, adapter: Any) -> tuple[bool, Optional[str]]:
        """Validate CANoe-specific capabilities.

        Required capabilities:
        - configuration: Load .cfg files
        - measurement: Start/stop measurement
        - test_module: Execute test modules
        """
        has_configuration = adapter.get_capability("configuration") is not None
        has_measurement = adapter.get_capability("measurement") is not None
        has_test_module = adapter.get_capability("test_module") is not None

        if not has_configuration or not has_measurement:
            return False, "Missing CANoe capability: configuration/measurement"
        if not has_test_module:
            return False, "Missing CANoe capability: test_module"
        return True, None

    def _load_configuration(self, adapter: Any, config_path: Optional[str]) -> None:
        """Load CANoe configuration via configuration capability."""
        if not config_path:
            return
        configuration_capability = adapter.get_capability("configuration")
        if configuration_capability is None or not configuration_capability.load(config_path):
            raise RuntimeError(f"failed to load CANoe configuration: {config_path}")

    def _start_measurement(self, adapter: Any) -> None:
        """Start CANoe measurement via measurement capability."""
        measurement_capability = adapter.get_capability("measurement")
        if measurement_capability is None or not measurement_capability.start():
            raise RuntimeError("failed to start CANoe measurement")

    def _stop_measurement(self, adapter: Any) -> None:
        """Stop CANoe measurement via measurement capability."""
        measurement_capability = adapter.get_capability("measurement")
        if measurement_capability is not None:
            measurement_capability.stop()

    def _timeout_for(self, plan: Any) -> int:
        """Extract timeout from plan."""
        return int(getattr(plan, "timeout_seconds", 0) or 0)

    def _build_test_result(self, case: Any, raw_result: dict) -> TestResult:
        """Build TestResult from raw execution result."""
        verdict = raw_result.get("verdict", "UNKNOWN")
        passed = verdict.upper() in {"PASS", "PASSED", "SUCCESS"}
        return TestResult(
            name=getattr(case, "case_name", ""),
            type="test_module",
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
        """Execute CANoe test modules following the Runtime Adapter Contract.

        Execution Flow:
        1. Validate test_module capability
        2. Validate all cases are test_module type
        3. Load configuration
        4. Start measurement
        5. Execute each test module
        6. Stop measurement (in finally)
        7. Return ExecutionOutcome or list[TestResult]
        """
        test_module_capability = adapter.get_capability("test_module")
        plan_cases = getattr(plan, "cases", []) or []

        if test_module_capability is None:
            raise RuntimeError("CANoe test_module capability is not available")

        if not plan_cases:
            # Return empty result following contract
            return []

        if not all(getattr(case, "case_type", "") == "test_module" for case in plan_cases):
            raise RuntimeError("CANoeExecutionStrategy only supports test_module cases")

        # Use collector from executor if not provided directly
        runtime_collector = collector or getattr(executor, "current_collector", None)

        self._load_configuration(adapter, config_path)
        self._start_measurement(adapter)

        try:
            results: list[TestResult] = []
            artifacts: dict[str, Any] = {}

            for case in plan_cases:
                raw_result = test_module_capability.execute_module(
                    getattr(case, "case_name", ""),
                    timeout=self._timeout_for(plan),
                )
                test_result = self._build_test_result(case, raw_result)
                results.append(test_result)
                self._collect_results(runtime_collector, [test_result])

                # Collect artifact paths from raw_result if available
                if raw_result:
                    if raw_result.get("report_path"):
                        artifacts["report_path"] = raw_result["report_path"]
                    if raw_result.get("log_path"):
                        artifacts["log_path"] = raw_result["log_path"]
                    if raw_result.get("testdata_path"):
                        artifacts["testdata_path"] = raw_result["testdata_path"]

            # Return ExecutionOutcome if collector available, else list[TestResult]
            if runtime_collector is not None:
                return runtime_collector.finalize(
                    status="completed",
                    artifacts=artifacts if artifacts else None,
                    report_metadata={"source": "canoe_test_module"},
                )
            return results

        finally:
            self._stop_measurement(adapter)
