"""TSMaster execution strategy."""

from __future__ import annotations

from typing import Any

from core.case_mapping_manager import get_case_mapping_manager
from core.result_collector import ResultCollector
from models.task import TestResult

from .base import ExecutionStrategy


class TSMasterExecutionStrategy(ExecutionStrategy):
    """Strategy for TSMaster execution flow."""

    strategy_name = "tsmaster"

    def prepare(self, plan: Any, adapter: Any):
        has_execution = adapter.get_capability("tsmaster_execution") is not None
        if not has_execution:
            has_execution = adapter.get_capability("rpc_execution") is not None
        if not has_execution:
            return False, "Missing TSMaster capability: tsmaster_execution"
        return True, None

    def _load_configuration(self, adapter: Any, config_path: str | None) -> None:
        if not config_path:
            return
        configuration_capability = adapter.get_capability("configuration")
        if configuration_capability is None or not configuration_capability.load(config_path):
            raise RuntimeError(f"failed to load TSMaster configuration: {config_path}")

    def _stop_measurement(self, adapter: Any) -> None:
        measurement_capability = adapter.get_capability("measurement")
        if measurement_capability is not None:
            measurement_capability.stop()

    def _timeout_for(self, plan: Any) -> int:
        timeout = getattr(plan, "timeout_seconds", None)
        if timeout is None:
            timeout = getattr(plan, "timeout", 0)
        return int(timeout or 0)

    def _plan_cases(self, plan: Any) -> list[Any]:
        return list(getattr(plan, "cases", []) or getattr(plan, "test_items", []) or [])

    def _case_selection_string(self, plan: Any) -> str:
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

    def _execution_capability(self, adapter: Any) -> tuple[Any, str]:
        capability = adapter.get_capability("tsmaster_execution")
        if capability is not None:
            return capability, "tsmaster_execution"

        capability = adapter.get_capability("rpc_execution")
        if capability is not None:
            return capability, "rpc_execution"

        raise RuntimeError("TSMaster execution capability is not available")

    def _build_case_selection(self, plan: Any, capability: Any) -> str:
        if hasattr(capability, "build_case_selection"):
            return capability.build_case_selection(plan)
        return self._case_selection_string(plan)

    def _start_execution(self, capability: Any, capability_name: str, selected_cases: str, timeout: int) -> bool:
        if capability_name == "tsmaster_execution":
            return bool(capability.start_execution(selected_cases))
        return bool(
            capability.start_execution(
                test_cases=selected_cases,
                wait_for_complete=False,
                timeout=timeout,
            )
        )

    def _wait_for_completion(self, capability: Any, capability_name: str, timeout: int) -> bool:
        if capability_name == "tsmaster_execution":
            waiter = getattr(capability, "wait_for_completion", None) or getattr(
                capability, "wait_for_complete", None
            )
            if waiter is None:
                return True
            return bool(waiter(timeout))

        waiter = getattr(capability, "wait_for_complete", None) or getattr(
            capability, "wait_for_completion", None
        )
        if waiter is None:
            return True
        try:
            return bool(waiter(timeout=timeout))
        except TypeError:
            return bool(waiter(timeout))

    def _report_items(self, report_info: Any) -> list[TestResult]:
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
                )
            )
        return results

    def _failure_results(self, plan: Any, verdict: str, message: str) -> list[TestResult]:
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

    def _append_results(self, collector: ResultCollector | None, results: list[TestResult]) -> None:
        if collector is None:
            return
        for result in results:
            collector.add_test_result(result)

    def run(
        self,
        plan: Any,
        adapter: Any,
        collector: ResultCollector | None = None,
        executor: Any = None,
        config_path: str | None = None,
    ) -> Any:
        self._load_configuration(adapter, config_path)

        runtime_collector = collector or getattr(executor, "current_collector", None)

        try:
            capability, capability_name = self._execution_capability(adapter)
            selected_cases = self._build_case_selection(plan, capability)
            timeout = self._timeout_for(plan)

            if not self._start_execution(capability, capability_name, selected_cases, timeout):
                results = self._failure_results(
                    plan,
                    verdict="ERROR",
                    message="TSMaster execution failed to start",
                )
                self._append_results(runtime_collector, results)
                if runtime_collector is not None:
                    runtime_collector.add_log("ERROR", "TSMaster execution failed to start")
                    return runtime_collector.finalize(
                        status="failed",
                        error_message="TSMaster execution failed to start",
                    )
                return results

            if not self._wait_for_completion(capability, capability_name, timeout):
                results = self._failure_results(
                    plan,
                    verdict="TIMEOUT",
                    message="TSMaster execution timed out",
                )
                self._append_results(runtime_collector, results)
                if runtime_collector is not None:
                    runtime_collector.add_log("ERROR", "TSMaster execution timed out")
                    return runtime_collector.finalize(
                        status="timeout",
                        error_message="TSMaster execution timed out",
                    )
                return results

            report_info = getattr(capability, "get_report_info", lambda: None)()
            if not report_info:
                results = self._failure_results(
                    plan,
                    verdict="ERROR",
                    message="TSMaster report information is missing",
                )
                self._append_results(runtime_collector, results)
                if runtime_collector is not None:
                    runtime_collector.add_log("ERROR", "TSMaster report information is missing")
                    return runtime_collector.finalize(
                        status="failed",
                        error_message="TSMaster report information is missing",
                    )
                return results

            results = self._report_items(report_info)
            self._append_results(runtime_collector, results)
            if runtime_collector is not None:
                return runtime_collector.finalize(status="completed")
            return results
        finally:
            self._stop_measurement(adapter)
