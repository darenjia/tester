"""TTworkbench execution strategy."""

from typing import Any

from models.task import TestResult

from .base import ExecutionStrategy


class TTworkbenchExecutionStrategy(ExecutionStrategy):
    strategy_name = "ttworkbench"

    def prepare(self, plan: Any, adapter: Any):
        has_configuration = adapter.get_capability("configuration") is not None
        has_measurement = adapter.get_capability("measurement") is not None
        has_execution = adapter.get_capability("ttworkbench_execution") is not None

        if not has_configuration or not has_measurement:
            return False, "Missing TTworkbench capability: configuration/measurement"
        if not has_execution:
            return False, "Missing TTworkbench capability: ttworkbench_execution"
        return True, None

    def run(self, plan: Any, adapter: Any, executor: Any = None, config_path: str | None = None) -> Any:
        configuration_capability = adapter.get_capability("configuration")
        measurement_capability = adapter.get_capability("measurement")
        execution_capability = adapter.get_capability("ttworkbench_execution")

        if config_path and (configuration_capability is None or not configuration_capability.load(config_path)):
            raise RuntimeError(f"failed to load TTworkbench configuration: {config_path}")

        if measurement_capability is None or not measurement_capability.start():
            raise RuntimeError("failed to start TTworkbench execution")

        try:
            results: list[TestResult] = []
            for case in getattr(plan, "cases", []) or []:
                case_type = getattr(case, "case_type", "") or "clf_test"
                params = getattr(case, "execution_params", {}) or {}
                task_id = getattr(plan, "task_no", "")

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
                    raise RuntimeError(f"TTworkbenchExecutionStrategy does not support case_type: {case_type}")

                verdict = raw_result.get("verdict") or ("PASS" if raw_result.get("status") == "passed" else "FAIL")
                test_result = TestResult(
                    name=getattr(case, "case_name", ""),
                    type=case_type,
                    verdict=verdict,
                    details=raw_result,
                )
                if executor is not None and getattr(executor, "current_collector", None) is not None:
                    executor.current_collector.add_test_result(test_result)
                results.append(test_result)
            return results
        finally:
            measurement_capability.stop()
