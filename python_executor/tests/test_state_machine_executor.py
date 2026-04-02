from __future__ import annotations

from core.execution_plan import ExecutionPlan, PlannedCase
from core.state_machine_executor import StateMachineTaskExecutor
from core.test_state_handlers import ConnectingHandler
from core.state_machine import StateContext, TestState
from models.task import Case, Task, TaskStatus


def test_state_machine_executor_accepts_execution_plan():
    executor = StateMachineTaskExecutor(message_sender=lambda _: None)

    plan = ExecutionPlan(
        task_no="TASK-SM-1",
        device_id="DEVICE-SM-1",
        tool_type="canoe",
        timeout_seconds=120,
        cases=[PlannedCase(case_no="CASE-SM-1", case_name="Case SM 1", case_type="test_module")],
    )

    assert executor.execute_plan(plan) is True
    queued = executor._get_next_task()
    assert queued is plan


def test_state_machine_executor_converts_legacy_task_to_execution_plan():
    executor = StateMachineTaskExecutor(message_sender=lambda _: None)

    task = Task(
        taskNo="TASK-SM-2",
        deviceId="DEVICE-SM-2",
        toolType="canoe",
        timeout=30,
        caseList=[Case(caseNo="CASE-SM-2", caseName="Case SM 2", caseType="test_module")],
    )

    assert executor.execute_task(task) is True
    queued = executor._get_next_task()
    assert isinstance(queued, ExecutionPlan)
    assert queued.task_no == "TASK-SM-2"
    assert queued.cases[0].case_no == "CASE-SM-2"


def test_state_machine_executor_builds_handler_task_view_from_plan():
    executor = StateMachineTaskExecutor(message_sender=lambda _: None)
    plan = ExecutionPlan(
        task_no="TASK-SM-3",
        device_id="DEVICE-SM-3",
        tool_type="tsmaster",
        config_name="demo",
        base_config_dir="D:/cfgs",
        variables={"speed": 50},
        timeout_seconds=45,
        cases=[PlannedCase(case_no="CASE-SM-3", case_name="Case SM 3", case_type="test_module")],
    )

    task_view = executor._build_task_view(plan)

    assert task_view.task_id == "TASK-SM-3"
    assert task_view.tool_type == "tsmaster"
    assert task_view.config_name == "demo"
    assert task_view.base_config_dir == "D:/cfgs"
    assert task_view.config_params == {"speed": 50}
    assert task_view.test_items[0].case_no == "CASE-SM-3"


def test_state_machine_executor_emits_shared_task_status_message_format():
    messages = []
    executor = StateMachineTaskExecutor(message_sender=messages.append)

    executor._update_task_status("TASK-SM-4", TaskStatus.RUNNING)

    assert messages == [
        {
            "type": "TASK_STATUS",
            "taskNo": "TASK-SM-4",
            "status": TaskStatus.RUNNING.value,
            "timestamp": messages[0]["timestamp"],
        }
    ]


def test_state_machine_executor_emits_shared_result_report_message_format():
    messages = []
    executor = StateMachineTaskExecutor(message_sender=messages.append)
    plan = ExecutionPlan(task_no="TASK-SM-5", device_id="DEVICE-SM-5", tool_type="canoe")

    executor._complete_task(
        plan,
        {"summary": {"passed": 1}},
        collector=None,
        metrics=None,
    )

    assert messages[0]["type"] == "TASK_STATUS"
    assert messages[0]["taskNo"] == "TASK-SM-5"
    assert messages[1]["type"] == "RESULT_REPORT"
    assert messages[1]["taskNo"] == "TASK-SM-5"
    assert messages[1]["summary"] == {"passed": 1}


def test_connecting_handler_uses_raw_adapter_factory(monkeypatch):
    task_view = type("_TaskView", (), {"task_id": "TASK-SM-6"})()
    handler = ConnectingHandler(task_view, "canoe")
    created = {}

    class _Adapter:
        def connect(self):
            created["connected"] = True
            return True

    def _fake_create_adapter(tool_type, config=None, singleton=True):
        created["singleton"] = singleton
        return created.setdefault("adapter", _Adapter())

    monkeypatch.setattr("core.test_state_handlers.create_adapter", _fake_create_adapter)
    monkeypatch.setattr(
        "core.test_state_handlers.config_manager",
        type("_ConfigManager", (), {"get": staticmethod(lambda key, default=None: 1 if "max_retries" in key else 0)})(),
    )

    context = StateContext(task_id="TASK-SM-6")
    handler.on_enter(context)
    next_state = handler.on_execute(context)

    assert isinstance(context.data["controller"], _Adapter)
    assert next_state == TestState.RUNNING
    assert created["connected"] is True
    assert created["singleton"] is False


def test_state_machine_executor_rejects_tsmaster_execution_plan():
    executor = StateMachineTaskExecutor(message_sender=lambda _: None)
    plan = ExecutionPlan(
        task_no="TASK-SM-7",
        device_id="DEVICE-SM-7",
        tool_type="tsmaster",
        timeout_seconds=60,
        cases=[PlannedCase(case_no="CASE-SM-7", case_name="Case SM 7", case_type="test_module")],
    )

    assert executor.execute_plan(plan) is False


def test_state_machine_executor_rejects_unknown_tool_type():
    executor = StateMachineTaskExecutor(message_sender=lambda _: None)
    plan = ExecutionPlan(
        task_no="TASK-SM-8",
        device_id="DEVICE-SM-8",
        tool_type="unknown-tool",
        timeout_seconds=60,
        cases=[PlannedCase(case_no="CASE-SM-8", case_name="Case SM 8", case_type="test_module")],
    )

    assert executor.execute_plan(plan) is False
