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

    def run(self, plan: Any, adapter: Any, executor: Any = None, config_path: str | None = None) -> Any:
        if executor is None:
            raise RuntimeError("CANoeExecutionStrategy requires executor context")

        if config_path:
            executor._load_configuration_by_path(config_path)

        executor._start_measurement(plan)
        try:
            test_module_capability = adapter.get_capability("test_module")
            plan_cases = getattr(plan, "cases", []) or []
            if test_module_capability is None:
                raise RuntimeError("CANoe test_module capability is not available")
            if not plan_cases:
                return []
            if not all(getattr(case, "case_type", "") == "test_module" for case in plan_cases):
                raise RuntimeError("CANoeExecutionStrategy only supports test_module cases")

            results = []
            for case in plan_cases:
                result = test_module_capability.execute_module(
                    getattr(case, "case_name", ""),
                    timeout=executor._task_timeout(plan),
                )
                test_result = TestResult(
                    name=getattr(case, "case_name", ""),
                    type="test_module",
                    verdict=result.get("verdict", "UNKNOWN"),
                    details=result,
                )
                if getattr(executor, "current_collector", None) is not None:
                    executor.current_collector.add_test_result(test_result)
                results.append(test_result)
            return results
        finally:
            executor._stop_measurement(plan)
