from types import SimpleNamespace

import pytest
from core.execution_strategies.tsmaster_strategy import TSMasterExecutionStrategy
from core.result_collector import ResultCollector


class _ExecutionCapability:
    def __init__(self, report_info):
        self.report_info = report_info
        self.build_case_selection_calls = 0
        self.start_execution_calls = []
        self.wait_for_completion_calls = []
        self.wait_for_complete_calls = []
        self.report_info_calls = 0

    def build_case_selection(self, plan):
        self.build_case_selection_calls += 1
        return ",".join(f"{case.case_no}=1" for case in getattr(plan, "cases", []) or [])

    def start_execution(self, *args, **kwargs):
        self.start_execution_calls.append((args, kwargs))
        return True

    def wait_for_completion(self, timeout):
        self.wait_for_completion_calls.append(timeout)
        return True

    def wait_for_complete(self, timeout=None):
        self.wait_for_complete_calls.append(timeout)
        return True

    def get_report_info(self):
        self.report_info_calls += 1
        return self.report_info


class _MeasurementCapability:
    def __init__(self, start_result=True, stop_result=True):
        self.start_result = start_result
        self.stop_result = stop_result
        self.start_calls = 0
        self.stop_calls = 0

    def start(self):
        self.start_calls += 1
        return self.start_result

    def stop(self):
        self.stop_calls += 1
        return self.stop_result


class _Adapter:
    def __init__(self, capability, measurement_capability=None):
        self._capability = capability
        self._measurement_capability = measurement_capability

    def get_capability(self, name, default=None):
        if name == "tsmaster_execution":
            return self._capability
        if name == "measurement":
            return self._measurement_capability
        return default


def _make_plan(timeout_seconds=30, case_nos=None):
    case_nos = case_nos or ["CASE-001"]
    return SimpleNamespace(
        tool_type="tsmaster",
        task_no="TASK-001",
        timeout_seconds=timeout_seconds,
        timeout=timeout_seconds,
        cases=[
            SimpleNamespace(case_no=case_no, case_name=f"Case {index + 1}", case_type="test_module")
            for index, case_no in enumerate(case_nos)
        ],
    )


def _patch_mapping_manager(monkeypatch):
    monkeypatch.setattr(
        "core.execution_strategies.tsmaster_strategy.get_case_mapping_manager",
        lambda: type(
            "_MappingManager",
            (),
            {
                "get_mapping": staticmethod(
                    lambda case_no: SimpleNamespace(ini_config=f"{case_no}=1")
                )
            },
        )(),
    )


def test_tsmaster_strategy_uses_dedicated_execution_capability_and_collects_outcome(monkeypatch):
    _patch_mapping_manager(monkeypatch)
    capability = _ExecutionCapability(
        {
            "report_path": "C:/reports/out.html",
            "testdata_path": "C:/reports/data",
            "results": [{"case_no": "CASE-001", "passed": True, "verdict": "PASS"}],
        }
    )
    adapter = _Adapter(capability)
    collector = ResultCollector("TASK-001")

    outcome = TSMasterExecutionStrategy().run(_make_plan(), adapter, collector)

    assert capability.build_case_selection_calls == 1
    assert getattr(outcome, "taskNo", None) == "TASK-001"
    assert getattr(outcome, "status", None) == "completed"
    assert outcome.summary["passed"] == 1
    assert outcome.summary["failed"] == 0
    assert outcome.results[0].name == "CASE-001"


def test_tsmaster_strategy_marks_timeout_as_failed_outcome(monkeypatch):
    _patch_mapping_manager(monkeypatch)

    capability = _ExecutionCapability(
        {
            "report_path": "C:/reports/out.html",
            "testdata_path": "C:/reports/data",
            "results": [],
        }
    )
    capability.wait_for_completion = lambda timeout: capability.wait_for_completion_calls.append(timeout) or False
    adapter = _Adapter(capability)
    collector = ResultCollector("TASK-001")

    outcome = TSMasterExecutionStrategy().run(_make_plan(timeout_seconds=5), adapter, collector)

    assert capability.build_case_selection_calls == 1
    assert getattr(outcome, "status", None) == "timeout"
    assert getattr(outcome, "errorMessage", None) == "TSMaster execution timed out"
    assert outcome.summary["failed"] == 1


def test_tsmaster_strategy_marks_missing_report_as_failed_outcome(monkeypatch):
    _patch_mapping_manager(monkeypatch)

    capability = _ExecutionCapability(None)
    capability.get_report_info = lambda: None
    adapter = _Adapter(capability)
    collector = ResultCollector("TASK-001")

    outcome = TSMasterExecutionStrategy().run(_make_plan(), adapter, collector)

    assert capability.build_case_selection_calls == 1
    assert getattr(outcome, "status", None) == "failed"
    assert "report" in getattr(outcome, "errorMessage", "").lower()


def test_tsmaster_strategy_returns_execution_outcome_for_executor_runtime_collector(monkeypatch):
    _patch_mapping_manager(monkeypatch)
    capability = _ExecutionCapability(
        {
            "report_path": "C:/reports/out.html",
            "testdata_path": "C:/reports/data",
            "results": [{"case_no": "CASE-001", "passed": True, "verdict": "PASS"}],
        }
    )
    adapter = _Adapter(capability)
    executor = SimpleNamespace(current_collector=ResultCollector("TASK-001"))

    outcome = TSMasterExecutionStrategy().run(_make_plan(), adapter, executor=executor)

    assert getattr(outcome, "taskNo", None) == "TASK-001"
    assert getattr(outcome, "status", None) == "completed"
    assert outcome.summary["passed"] == 1


def test_tsmaster_strategy_starts_measurement_before_execution(monkeypatch):
    """Verify measurement is started before test execution begins."""
    _patch_mapping_manager(monkeypatch)
    measurement_cap = _MeasurementCapability()
    capability = _ExecutionCapability(
        {
            "report_path": "C:/reports/out.html",
            "testdata_path": "C:/reports/data",
            "results": [{"case_no": "CASE-001", "passed": True, "verdict": "PASS"}],
        }
    )
    adapter = _Adapter(capability, measurement_cap)
    collector = ResultCollector("TASK-001")

    TSMasterExecutionStrategy().run(_make_plan(), adapter, collector)

    # Measurement should have been started
    assert measurement_cap.start_calls == 1
    # And stop should have been called since we started it
    assert measurement_cap.stop_calls == 1


def test_tsmaster_strategy_stops_measurement_only_if_started(monkeypatch):
    """Verify measurement stop only happens when strategy started the measurement."""
    _patch_mapping_manager(monkeypatch)
    measurement_cap = _MeasurementCapability(start_result=False)  # Simulate measurement already running
    capability = _ExecutionCapability(
        {
            "report_path": "C:/reports/out.html",
            "testdata_path": "C:/reports/data",
            "results": [{"case_no": "CASE-001", "passed": True, "verdict": "PASS"}],
        }
    )
    adapter = _Adapter(capability, measurement_cap)
    collector = ResultCollector("TASK-001")

    TSMasterExecutionStrategy().run(_make_plan(), adapter, collector)

    # Measurement was attempted but returned False (already running)
    assert measurement_cap.start_calls == 1
    # Stop should NOT be called since start returned False (not started by us)
    assert measurement_cap.stop_calls == 0


def test_tsmaster_strategy_uses_fallback_when_only_aggregate_stats(monkeypatch):
    """Verify fallback derivation when report info has only aggregate stats, no per-case details."""
    _patch_mapping_manager(monkeypatch)
    capability = _ExecutionCapability(
        {
            "report_path": "C:/reports/out.html",
            "testdata_path": "C:/reports/data",
            "passed": 3,
            "failed": 1,
            "total": 4,
            # No "results" or "details" key - only aggregate stats
        }
    )
    adapter = _Adapter(capability)
    collector = ResultCollector("TASK-001")

    outcome = TSMasterExecutionStrategy().run(_make_plan(case_nos=["CASE-001", "CASE-002", "CASE-003", "CASE-004"]), adapter, collector)

    assert getattr(outcome, "status", None) == "completed"
    # Should have 4 results (one per case)
    assert len(outcome.results) == 4
    # Fallback: first fail_count=1 cases fail, rest pass
    # Sorted by case_no: CASE-001 fails, others pass
    passed_results = [r for r in outcome.results if r.passed]
    failed_results = [r for r in outcome.results if not r.passed]
    assert len(passed_results) == 3
    assert len(failed_results) == 1
    assert failed_results[0].name == "CASE-001"
    # Fallback results should have details indicating fallback
    assert failed_results[0].details.get("fallback") is True
    assert failed_results[0].details.get("report_path") == "C:/reports/out.html"


def test_tsmaster_strategy_start_failure_returns_error_outcome(monkeypatch):
    """Verify execution failure when start_execution returns False."""
    _patch_mapping_manager(monkeypatch)
    capability = _ExecutionCapability(
        {
            "report_path": "C:/reports/out.html",
            "testdata_path": "C:/reports/data",
            "results": [{"case_no": "CASE-001", "passed": True, "verdict": "PASS"}],
        }
    )
    # Make start_execution return False to simulate start failure
    capability.start_execution = lambda *args, **kwargs: False
    adapter = _Adapter(capability)
    collector = ResultCollector("TASK-001")

    outcome = TSMasterExecutionStrategy().run(_make_plan(), adapter, collector)

    assert getattr(outcome, "status", None) == "failed"
    assert "failed to start" in getattr(outcome, "errorMessage", "").lower()
    assert outcome.summary["failed"] == 1


def test_tsmaster_strategy_timeout_preserves_timeout_semantics(monkeypatch):
    """Verify timeout is preserved as explicit task failure with TIMEOUT verdict."""
    _patch_mapping_manager(monkeypatch)
    capability = _ExecutionCapability(
        {
            "report_path": "C:/reports/out.html",
            "testdata_path": "C:/reports/data",
            "results": [],
        }
    )
    # Make wait_for_completion return False to simulate timeout
    capability.wait_for_completion = lambda timeout: False
    adapter = _Adapter(capability)
    collector = ResultCollector("TASK-001")

    outcome = TSMasterExecutionStrategy().run(_make_plan(timeout_seconds=5), adapter, collector)

    assert getattr(outcome, "status", None) == "timeout"
    assert getattr(outcome, "errorMessage", None) == "TSMaster execution timed out"
    # Verify timeout case has TIMEOUT verdict
    timeout_results = [r for r in outcome.results if r.verdict == "TIMEOUT"]
    assert len(timeout_results) >= 1


def test_tsmaster_strategy_report_missing_preserves_failure_semantics(monkeypatch):
    """Verify report missing is preserved as explicit task failure with ERROR verdict."""
    _patch_mapping_manager(monkeypatch)
    capability = _ExecutionCapability(None)
    capability.get_report_info = lambda: None
    adapter = _Adapter(capability)
    collector = ResultCollector("TASK-001")

    outcome = TSMasterExecutionStrategy().run(_make_plan(), adapter, collector)

    assert getattr(outcome, "status", None) == "failed"
    assert "report" in getattr(outcome, "errorMessage", "").lower()
    # Verify missing report case has ERROR verdict
    error_results = [r for r in outcome.results if r.verdict == "ERROR"]
    assert len(error_results) >= 1


def test_tsmaster_strategy_without_measurement_capability_completes_successfully(monkeypatch):
    """Verify execution succeeds when measurement capability is absent."""
    _patch_mapping_manager(monkeypatch)
    capability = _ExecutionCapability(
        {
            "report_path": "C:/reports/out.html",
            "testdata_path": "C:/reports/data",
            "results": [{"case_no": "CASE-001", "passed": True, "verdict": "PASS"}],
        }
    )
    adapter = _Adapter(capability, measurement_capability=None)  # No measurement capability
    collector = ResultCollector("TASK-001")

    outcome = TSMasterExecutionStrategy().run(_make_plan(), adapter, collector)

    assert getattr(outcome, "status", None) == "completed"
    assert outcome.summary["passed"] == 1
    assert outcome.summary["failed"] == 0

