from __future__ import annotations

import pytest

from core.config_preparation import ConfigPreparationError
from core.execution_plan import ExecutionPlan, PlannedCase
from core.task_compiler import TaskCompileError, TaskCompiler
from models.case_mapping_view import (
    CANoeMaterial,
    CaseMappingView,
    MappingDeclaration,
    MappingMaterial,
    TSMasterMaterial,
)
from models.result import Message
from models.task import Case, Task


class _FakeMappingManager:
    def __init__(self, views=None, mappings=None):
        self._views = views or {}
        self._mappings = mappings or {}

    def get_mapping(self, case_no):
        return self._mappings.get(case_no)

    def get_mapping_view(self, case_no):
        return self._views.get(case_no)


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


def _make_task(**overrides):
    base = {
        "projectNo": "PRJ",
        "taskNo": "TASK-001",
        "taskName": "demo task",
        "toolType": "CANoe",
        "caseList": [
            {"caseNo": "CASE-001", "caseName": "Case 1", "caseType": "test_module"},
        ],
        "timeout": 120,
        "variables": {"speed": 100},
    }
    base.update(overrides)
    return Task.from_dict(base)


def _make_canoe_view(case_no: str, config_path: str) -> CaseMappingView:
    return CaseMappingView(
        declaration=MappingDeclaration(
            case_no=case_no,
            case_name="Case",
            tool_type="canoe",
            category="canoe",
        ),
        material=MappingMaterial(
            canoe=CANoeMaterial(
                config_path=config_path,
                module_name="CANoe module",
                ini_config="CASE-001=1",
            ),
        ),
    )


def _make_tsmaster_view(case_no: str) -> CaseMappingView:
    return CaseMappingView(
        declaration=MappingDeclaration(
            case_no=case_no,
            case_name="TSMaster case",
            tool_type="tsmaster",
            category="tsmaster",
        ),
        material=MappingMaterial(
            tsmaster=TSMasterMaterial(
                selection_key=case_no,
                ini_config=f"{case_no}=1",
                project_path="D:/project",
            ),
        ),
    )


def test_task_compiler_rejects_mixed_tool_types():
    compiler = TaskCompiler(
        mapping_manager=_FakeMappingManager(
            mappings={
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


def test_task_compiler_prefers_explicit_config_path(tmp_path):
    config_path = tmp_path / "explicit.cfg"
    config_path.write_text("dummy", encoding="utf-8")

    compiler = TaskCompiler(mapping_manager=_FakeMappingManager())
    plan = compiler.compile(_make_task(configPath=str(config_path)))

    assert isinstance(plan, ExecutionPlan)
    assert plan.preparation_mode == "explicit_path"
    assert plan.resolved_config_path == str(config_path)
    assert plan.selection_material["source"] == "task.configPath"
    assert "explicit_path" in plan.prepare_summary


def test_task_compiler_prepares_named_config_from_base_dir(tmp_path):
    config_path = tmp_path / "named.cfg"
    config_path.write_text("dummy", encoding="utf-8")
    (tmp_path / "SelectInfo.ini").write_text("[CFG_PARA]\nCASE-001=1\n", encoding="utf-8")

    compiler = TaskCompiler(mapping_manager=_FakeMappingManager())
    task = _make_task(toolType="CANoe", configName="named", baseConfigDir=str(tmp_path))

    plan = compiler.compile(task)

    assert plan.preparation_mode == "prepared_config"
    assert plan.resolved_config_path == str(config_path)
    assert plan.resolved_variables == {"speed": 100}
    assert plan.selection_material["config_name"] == "named"
    assert plan.config_artifacts["cfg_path"] == str(config_path)


def test_task_compiler_uses_mapping_material_for_canoe():
    mapping = _make_canoe_view("CASE-001", "D:/projects/canoe/config.cfg")
    compiler = TaskCompiler(
        mapping_manager=_FakeMappingManager(views={"CASE-001": mapping})
    )

    plan = compiler.compile(_make_task(toolType="CANoe", configPath=None, configName=None))

    assert plan.preparation_mode == "mapping_material_only"
    assert plan.resolved_config_path == "D:/projects/canoe/config.cfg"
    assert plan.selection_material["config_path"] == "D:/projects/canoe/config.cfg"


def test_task_compiler_uses_mapping_material_for_tsmaster_selection_only():
    mapping = _make_tsmaster_view("CASE-001")
    compiler = TaskCompiler(
        mapping_manager=_FakeMappingManager(views={"CASE-001": mapping})
    )

    task = _make_task(toolType="TSMaster", configPath=None, configName=None)
    plan = compiler.compile(task)

    assert plan.preparation_mode == "mapping_material_only"
    assert plan.resolved_config_path is None
    assert plan.selection_material["selection_key"] == "CASE-001"
    assert plan.selection_material["ini_config"] == "CASE-001=1"


def test_task_compiler_allows_tool_runtime_only_without_cfg():
    compiler = TaskCompiler(mapping_manager=_FakeMappingManager())
    task = _make_task(toolType="TSMaster", configPath=None, configName=None)

    plan = compiler.compile(task)

    assert plan.preparation_mode == "tool_runtime_only"
    assert plan.resolved_config_path is None
    assert plan.selection_material["source"] == "tool_runtime"


def test_task_compiler_raises_when_canoe_material_missing():
    compiler = TaskCompiler(mapping_manager=_FakeMappingManager())
    task = _make_task(toolType="CANoe", configPath=None, configName=None)

    with pytest.raises(ConfigPreparationError, match="缺少 configPath"):
        compiler.compile(task)


def test_execute_plan_queues_internal_execution_plan(monkeypatch):
    from core.task_executor_production import TaskExecutorProduction

    executor = TaskExecutorProduction(message_sender=lambda _: None)
    queued_items = []
    monkeypatch.setattr(executor._task_queue, "put", lambda plan: queued_items.append(plan) or True)
    monkeypatch.setattr(
        executor._strategy_selector,
        "select",
        lambda plan: type("StubStrategy", (), {"prepare": staticmethod(lambda plan, adapter: (True, None))})(),
    )
    monkeypatch.setattr(
        "core.task_executor_production.create_adapter",
        lambda tool_type, singleton=False: object(),
    )

    plan = ExecutionPlan(
        task_no="TASK-QUEUE",
        project_no="PROJECT-QUEUE",
        task_name="Queue Task",
        device_id="DEVICE-QUEUE",
        tool_type="canoe",
        cases=[PlannedCase(case_no="CASE-QUEUE", case_name="Case Queue", case_type="test_module")],
        preparation_mode="mapping_material_only",
        resolved_config_path="D:/cfgs/prepared.cfg",
    )

    assert executor.execute_plan(plan) is True
    assert queued_items[0] is plan


def test_execute_task_converts_legacy_task_to_prepared_execution_plan(monkeypatch, tmp_path):
    from core.task_executor_production import TaskExecutorProduction

    config_path = tmp_path / "legacy.cfg"
    config_path.write_text("dummy", encoding="utf-8")

    executor = TaskExecutorProduction(message_sender=lambda _: None)
    queued_items = []
    monkeypatch.setattr(executor._task_queue, "put", lambda plan: queued_items.append(plan) or True)
    monkeypatch.setattr(
        executor._strategy_selector,
        "select",
        lambda plan: type("StubStrategy", (), {"prepare": staticmethod(lambda plan, adapter: (True, None))})(),
    )
    monkeypatch.setattr(
        "core.task_executor_production.create_adapter",
        lambda tool_type, singleton=False: object(),
    )

    task = Task(
        taskNo="TASK-LEGACY",
        taskName="Legacy Task",
        deviceId="DEVICE-LEGACY",
        toolType="canoe",
        configPath=str(config_path),
        timeout=45,
        caseList=[Case(caseNo="CASE-LEGACY", caseName="Case Legacy", caseType="test_module")],
    )

    assert executor.execute_task(task) is True
    assert isinstance(queued_items[0], ExecutionPlan)
    assert queued_items[0].task_no == "TASK-LEGACY"
    assert queued_items[0].prepared_cases[0].case_no == "CASE-LEGACY"
    assert queued_items[0].resolved_config_path == str(config_path)
