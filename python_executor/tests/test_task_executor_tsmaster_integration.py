from core.execution_plan import ExecutionPlan, PlannedCase
from core.task_executor_production import TaskExecutorProduction
from models.result import ExecutionOutcome


def _build_tsmaster_plan():
    return ExecutionPlan(
        task_no="TASK-TSMASTER-1",
        device_id="DEVICE-1",
        tool_type="tsmaster",
        timeout_seconds=30,
        config_path="D:/cfgs/tsmaster.tsp",
        cases=[PlannedCase(case_no="CASE-1", case_name="Case 1", case_type="test_module")],
    )


def test_execute_task_production_routes_tsmaster_through_strategy(monkeypatch):
    executor = TaskExecutorProduction(message_sender=lambda _: None)
    observed = {}

    class _Strategy:
        def run(self, plan, adapter, executor=None, config_path=None):
            observed["plan_id"] = plan.task_no
            observed["tool_type"] = plan.tool_type
            observed["adapter"] = adapter
            observed["executor"] = executor
            observed["config_path"] = config_path
            return ["strategy-result"]

    class _Selector:
        def select(self, plan):
            observed["selected_tool_type"] = plan.tool_type
            return _Strategy()

    class _Adapter:
        def connect(self):
            return True

        def disconnect(self):
            return True

    monkeypatch.setattr(
        "core.task_executor_production.create_adapter",
        lambda *args, **kwargs: _Adapter(),
    )
    monkeypatch.setattr("core.task_executor_production.record_metric", lambda *args, **kwargs: None)
    monkeypatch.setattr(
        "core.task_executor_production._ensure_observability_context",
        lambda task: type("_Obs", (), {"transition": staticmethod(lambda *args, **kwargs: None)})(),
    )
    monkeypatch.setattr(executor, "_connect_tool_with_retry", lambda task: None)
    monkeypatch.setattr(executor, "_update_task_status", lambda *args, **kwargs: None)
    monkeypatch.setattr(executor, "_complete_task", lambda *args, **kwargs: None)
    monkeypatch.setattr(executor, "_cleanup", lambda: None)
    monkeypatch.setattr(executor._task_queue, "mark_processing", lambda *args, **kwargs: None)
    monkeypatch.setattr(executor._task_queue, "mark_completed", lambda *args, **kwargs: None)
    executor._strategy_selector = _Selector()

    executor._execute_task_production(_build_tsmaster_plan())

    assert observed["selected_tool_type"] == "tsmaster"
    assert observed["plan_id"] == "TASK-TSMASTER-1"
    assert observed["tool_type"] == "tsmaster"
    assert observed["config_path"] == "D:/cfgs/tsmaster.tsp"
    assert observed["executor"] is executor
    assert observed["adapter"].__class__ is _Adapter


def test_task_executor_no_longer_exposes_legacy_tsmaster_helpers():
    executor = TaskExecutorProduction(message_sender=lambda _: None)

    assert not hasattr(executor, "_start_test_execution")
    assert not hasattr(executor, "_collect_tsmaster_results")
    assert not hasattr(executor, "_build_tsmaster_test_cases_string")


def test_complete_task_marks_failed_execution_outcome_as_failed(monkeypatch):
    executor = TaskExecutorProduction(message_sender=lambda _: None)
    task = _build_tsmaster_plan()
    status_updates = []
    reports = []

    monkeypatch.setattr(
        "core.task_executor_production._ensure_observability_context",
        lambda task: type(
            "_Obs",
            (),
            {
                "transition": staticmethod(lambda *args, **kwargs: None),
                "fail": staticmethod(lambda *args, **kwargs: None),
                "get_snapshot": staticmethod(lambda *args, **kwargs: {"failed_stage": "executing"}),
            },
        )(),
    )
    monkeypatch.setattr("core.task_executor_production.record_metric", lambda *args, **kwargs: None)
    monkeypatch.setattr(executor, "_update_task_status", lambda *args: status_updates.append(args))
    monkeypatch.setattr(executor, "_report_final_result", lambda *args: reports.append(args))
    monkeypatch.setattr(executor._executor, "submit", lambda *args, **kwargs: None)

    executor._start_time = 0
    executor._current_report_info = None
    outcome = ExecutionOutcome(task_no=task.task_no, status="failed", error_summary="rpc failed")

    executor._complete_task(task, outcome)

    assert reports
    assert status_updates[-1][1].value == "failed"
    assert status_updates[-1][2] == "rpc failed"
