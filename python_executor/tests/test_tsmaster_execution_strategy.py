from types import SimpleNamespace

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


class _Adapter:
    def __init__(self, capability):
        self._capability = capability

    def get_capability(self, name, default=None):
        if name == "tsmaster_execution":
            return self._capability
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
