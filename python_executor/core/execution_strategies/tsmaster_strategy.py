"""TSMaster execution strategy."""

from typing import Any

from models.task import TestResult

from core.case_mapping_manager import get_case_mapping_manager

from .base import ExecutionStrategy


class TSMasterExecutionStrategy(ExecutionStrategy):
    """Strategy placeholder for TSMaster execution flow."""

    strategy_name = "tsmaster"

    def prepare(self, plan: Any, adapter: Any):
        has_configuration = adapter.get_capability("configuration") is not None
        has_measurement = adapter.get_capability("measurement") is not None
        has_rpc = adapter.get_capability("rpc_execution") is not None

        if not has_configuration or not has_measurement:
            return False, "Missing TSMaster capability: configuration/measurement"
        if not has_rpc:
            return False, "Missing TSMaster RPC capability"
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
        return int(getattr(plan, "timeout_seconds", 0) or 0)

    def _case_selection_string(self, plan: Any) -> str:
        mapping_manager = get_case_mapping_manager()
        cases = []
        for item in getattr(plan, "cases", []) or []:
            case_no = getattr(item, "case_no", None)
            if not case_no:
                continue
            mapping = mapping_manager.get_mapping(case_no)
            if mapping and getattr(mapping, "ini_config", None):
                cases.append(mapping.ini_config)
        return ",".join(cases)

    def _collect_results(self, plan: Any, adapter: Any, executor: Any = None) -> list[TestResult]:
        rpc_capability = adapter.get_capability("rpc_execution")
        artifact_capability = adapter.get_capability("artifact")
        report_info = None
        if artifact_capability is not None:
            report_info = artifact_capability.collect()
        elif rpc_capability is not None and hasattr(rpc_capability, "get_report_info"):
            report_info = rpc_capability.get_report_info()

        results: list[TestResult] = []
        plan_cases = getattr(plan, "cases", []) or []
        if not report_info:
            for case in plan_cases:
                result = TestResult(
                    name=getattr(case, "case_name", ""),
                    type=getattr(case, "case_type", "") or "test_module",
                    error="无法获取测试报告信息",
                )
                if executor is not None and getattr(executor, "current_collector", None) is not None:
                    executor.current_collector.add_test_result(result)
                results.append(result)
            return results

        details = report_info.get("details", []) or []
        for case in plan_cases:
            case_no = getattr(case, "case_no", None)
            case_name = getattr(case, "case_name", "")
            verdict = "UNKNOWN"
            for detail in details:
                if detail.get("name") == case_name or (case_no and detail.get("case_no") == case_no):
                    verdict = detail.get("verdict", "PASS" if detail.get("passed") else "FAIL")
                    break
            result = TestResult(
                name=case_name,
                type=getattr(case, "case_type", "") or "test_module",
                verdict=verdict,
                details={"case_no": case_no, "report_info": report_info},
            )
            if executor is not None and getattr(executor, "current_collector", None) is not None:
                executor.current_collector.add_test_result(result)
            results.append(result)
        return results

    def run(self, plan: Any, adapter: Any, executor: Any = None, config_path: str | None = None) -> Any:
        self._load_configuration(adapter, config_path)

        rpc_capability = adapter.get_capability("rpc_execution")
        if rpc_capability is None:
            raise RuntimeError("TSMaster RPC capability is not available")
        try:
            test_cases = self._case_selection_string(plan)
            if test_cases:
                success = rpc_capability.start_execution(
                    test_cases=test_cases,
                    wait_for_complete=False,
                    timeout=self._timeout_for(plan),
                )
                if not success:
                    raise RuntimeError("TSMaster测试执行启动失败")
                rpc_capability.wait_for_complete(timeout=self._timeout_for(plan))
            else:
                measurement_capability = adapter.get_capability("measurement")
                if measurement_capability is not None:
                    measurement_capability.start()
            return self._collect_results(plan, adapter, executor=executor)
        finally:
            self._stop_measurement(adapter)
