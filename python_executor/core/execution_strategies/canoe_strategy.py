"""CANoe execution strategy."""

from typing import Any

from models.task import TestResult

from .base import ExecutionStrategy


class CANoeExecutionStrategy(ExecutionStrategy):
    """Strategy placeholder for CANoe execution flow."""

    strategy_name = "canoe"

    def prepare(self, plan: Any, adapter: Any):
        has_configuration = adapter.get_capability("configuration") is not None
        has_measurement = adapter.get_capability("measurement") is not None
        has_test_module = adapter.get_capability("test_module") is not None

        if not has_configuration or not has_measurement:
            return False, "Missing CANoe capability: configuration/measurement"
        if not has_test_module:
            return False, "Missing CANoe capability: test_module"
        return True, None

    def _load_configuration(self, adapter: Any, config_path: str | None) -> None:
        if not config_path:
            return
        configuration_capability = adapter.get_capability("configuration")
        if configuration_capability is None or not configuration_capability.load(config_path):
            raise RuntimeError(f"failed to load CANoe configuration: {config_path}")

    def _start_measurement(self, adapter: Any) -> None:
        measurement_capability = adapter.get_capability("measurement")
        if measurement_capability is None or not measurement_capability.start():
            raise RuntimeError("failed to start CANoe measurement")

    def _stop_measurement(self, adapter: Any) -> None:
        measurement_capability = adapter.get_capability("measurement")
        if measurement_capability is not None:
            measurement_capability.stop()

    def _timeout_for(self, plan: Any) -> int:
        return int(getattr(plan, "timeout_seconds", 0) or 0)

    def run(self, plan: Any, adapter: Any, executor: Any = None, config_path: str | None = None) -> Any:
        test_module_capability = adapter.get_capability("test_module")
        plan_cases = getattr(plan, "cases", []) or []
        if test_module_capability is None:
            raise RuntimeError("CANoe test_module capability is not available")
        if not plan_cases:
            return []
        if not all(getattr(case, "case_type", "") == "test_module" for case in plan_cases):
            raise RuntimeError("CANoeExecutionStrategy only supports test_module cases")

        self._load_configuration(adapter, config_path)
        self._start_measurement(adapter)
        try:
            results = []
            for case in plan_cases:
                result = test_module_capability.execute_module(
                    getattr(case, "case_name", ""),
                    timeout=self._timeout_for(plan),
                )
                test_result = TestResult(
                    name=getattr(case, "case_name", ""),
                    type="test_module",
                    verdict=result.get("verdict", "UNKNOWN"),
                    details=result,
                )
                if executor is not None and getattr(executor, "current_collector", None) is not None:
                    executor.current_collector.add_test_result(test_result)
                results.append(test_result)
            return results
        finally:
            self._stop_measurement(adapter)
