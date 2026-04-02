from __future__ import annotations

from core.execution_plan import ExecutionPlan, PlannedCase
from core.state_machine_executor import StateMachineTaskExecutor
from models.task import Case, Task


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
