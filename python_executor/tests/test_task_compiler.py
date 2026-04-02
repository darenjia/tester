from __future__ import annotations

import pytest

from core.execution_plan import ConfigSource, ExecutionPlan, PlannedCase
from core.task_compiler import TaskCompileError, TaskCompiler
from models.result import Message
from models.task import Case, Task


class _FakeMapping:
    def __init__(
        self,
        category,
        enabled=True,
        script_path=None,
        case_name="",
        ini_config=None,
        para_config=None,
    ):
        self.category = category
        self.enabled = enabled
        self.script_path = script_path
        self.case_name = case_name
        self.ini_config = ini_config
        self.para_config = para_config


class _FakeMappingManager:
    def __init__(self, mappings):
        self._mappings = mappings

    def get_mapping(self, case_no):
        return self._mappings.get(case_no)


def test_task_compiler_rejects_mixed_tool_types():
    compiler = TaskCompiler(
        mapping_manager=_FakeMappingManager(
            {
                "CASE-1": _FakeMapping(category="CANOE"),
                "CASE-2": _FakeMapping(category="TSMASTER"),
            }
        )
    )
    message = Message(
        type="TASK_DISPATCH",
        taskNo="TASK-1",
        deviceId="DEVICE-1",
        payload={"testItems": [{"case_no": "CASE-1"}, {"case_no": "CASE-2"}]},
    )

    with pytest.raises(TaskCompileError):
        compiler.compile_message(message)


def test_task_compiler_resolves_mapping_backed_execution_plan():
    compiler = TaskCompiler(
        mapping_manager=_FakeMappingManager(
            {
                "CASE-3": _FakeMapping(
                    category="CANOE",
                    script_path="D:/cfgs/main.cfg",
                    case_name="Case Three",
                    ini_config="A=1",
                    para_config="B=2",
                )
            }
        )
    )
    message = Message(
        type="TASK_DISPATCH",
        taskNo="TASK-3",
        deviceId="DEVICE-3",
        payload={"testItems": [{"case_no": "CASE-3", "name": "fallback-name"}]},
    )

    plan = compiler.compile_message(message)

    assert isinstance(plan, ExecutionPlan)
    assert plan.tool_type == "canoe"
    assert plan.config_path == "D:/cfgs/main.cfg"
    assert plan.config_source is ConfigSource.CASE_MAPPING
    assert plan.cases[0].case_name == "fallback-name"
    assert plan.cases[0].execution_params["iniConfig"] == "A=1"
    assert plan.cases[0].execution_params["paraConfig"] == "B=2"


def test_task_compiler_tolerates_null_params_and_variables():
    compiler = TaskCompiler(
        mapping_manager=_FakeMappingManager(
            {
                "CASE-4": _FakeMapping(category="CANOE"),
            }
        )
    )
    message = Message(
        type="TASK_DISPATCH",
        taskNo="TASK-4",
        deviceId="DEVICE-4",
        payload={
            "variables": None,
            "testItems": [{"case_no": "CASE-4", "params": None}],
        },
    )

    plan = compiler.compile_message(message)

    assert plan.variables == {}
    assert plan.cases[0].execution_params == {}


def test_execute_plan_queues_internal_execution_plan(monkeypatch):
    from core.task_executor_production import TaskExecutorProduction

    executor = TaskExecutorProduction(message_sender=lambda _: None)
    queued_items = []
    monkeypatch.setattr(executor._task_queue, "put", lambda plan: queued_items.append(plan) or True)

    plan = ExecutionPlan(
        task_no="TASK-QUEUE",
        project_no="PROJECT-QUEUE",
        task_name="Queue Task",
        device_id="DEVICE-QUEUE",
        tool_type="canoe",
        cases=[PlannedCase(case_no="CASE-QUEUE", case_name="Case Queue", case_type="test_module")],
    )

    assert executor.execute_plan(plan) is True
    assert queued_items[0] is plan


def test_execute_task_converts_legacy_task_to_execution_plan(monkeypatch):
    from core.task_executor_production import TaskExecutorProduction

    executor = TaskExecutorProduction(message_sender=lambda _: None)
    queued_items = []
    monkeypatch.setattr(executor._task_queue, "put", lambda plan: queued_items.append(plan) or True)

    task = Task(
        taskNo="TASK-LEGACY",
        taskName="Legacy Task",
        deviceId="DEVICE-LEGACY",
        toolType="canoe",
        timeout=45,
        caseList=[Case(caseNo="CASE-LEGACY", caseName="Case Legacy", caseType="test_module")],
    )

    assert executor.execute_task(task) is True
    assert isinstance(queued_items[0], ExecutionPlan)
    assert queued_items[0].task_no == "TASK-LEGACY"
    assert queued_items[0].cases[0].case_no == "CASE-LEGACY"
