import json

import pytest

from core.case_mapping_manager import CaseMappingManager
from core.config_preparation import ConfigPreparationError
from core.task_compiler import TaskCompiler
from models.case_mapping import CaseMapping
from models.task import Task


@pytest.fixture(autouse=True)
def reset_case_mapping_singleton():
    CaseMappingManager._instance = None
    yield
    CaseMappingManager._instance = None


def _make_task(**overrides):
    base = {
        "projectNo": "PRJ",
        "taskNo": "TASK-INT-001",
        "taskName": "integration task",
        "toolType": "CANoe",
        "caseList": [{"caseNo": "CASE-010", "caseName": "Case 10", "caseType": "test_module"}],
        "timeout": 120,
        "variables": {"speed": 55},
    }
    base.update(overrides)
    return Task.from_dict(base)


def test_compiler_uses_real_config_manager_for_named_config(tmp_path):
    config_dir = tmp_path / "configs"
    config_dir.mkdir()
    (config_dir / "runtime.cfg").write_text("dummy", encoding="utf-8")
    (config_dir / "SelectInfo.ini").write_text("[CFG_PARA]\nCASE-010=1\n", encoding="utf-8")

    compiler = TaskCompiler(mapping_manager=None)
    task = _make_task(toolType="CANoe", configName="runtime", baseConfigDir=str(config_dir))

    plan = compiler.compile(task)

    assert plan.preparation_mode == "prepared_config"
    assert plan.resolved_config_path == str(config_dir / "runtime.cfg")
    assert plan.config_artifacts["cfg_path"] == str(config_dir / "runtime.cfg")
    assert plan.config_artifacts["ini_path"] == str(config_dir / "SelectInfo.ini")


def test_compiler_reads_mapping_view_for_canoe_config(tmp_path):
    storage_path = tmp_path / "case_mappings.json"
    mapping = CaseMapping(
        case_no="CASE-010",
        case_name="CANoe runtime case",
        category="canoe",
        module="CANoe module",
        script_path="D:/projects/canoe/runtime.cfg",
        ini_config="CASE-010=1",
    )
    storage_path.write_text(
        json.dumps(
            {
                "mappings": {"CASE-010": mapping.to_dict()},
                "change_history": [],
                "version": "1.0",
                "last_updated": "2026-04-02 00:00:00",
            },
            ensure_ascii=False,
            indent=2,
        ),
        encoding="utf-8",
    )

    manager = CaseMappingManager(storage_path=str(storage_path))
    compiler = TaskCompiler(mapping_manager=manager)

    plan = compiler.compile(_make_task(toolType="CANoe"))

    assert plan.preparation_mode == "mapping_material_only"
    assert plan.resolved_config_path == "D:/projects/canoe/runtime.cfg"
    assert plan.selection_material["ini_config"] == "CASE-010=1"


def test_compiler_fails_fast_for_missing_canoe_material():
    compiler = TaskCompiler(mapping_manager=None)
    task = _make_task(toolType="CANoe", configPath=None, configName=None)

    with pytest.raises(ConfigPreparationError):
        compiler.compile(task)


def test_compiler_reports_missing_named_config_as_preparation_error(tmp_path):
    compiler = TaskCompiler(mapping_manager=None)
    task = _make_task(toolType="CANoe", configName="missing", baseConfigDir=str(tmp_path))

    with pytest.raises(ConfigPreparationError, match="配置准备失败"):
        compiler.compile(task)


def test_compiler_rejects_conflicting_mapping_material_for_multi_case_task(tmp_path):
    storage_path = tmp_path / "case_mappings.json"
    first = CaseMapping(
        case_no="CASE-010",
        case_name="CANoe runtime case A",
        category="canoe",
        module="CANoe module",
        script_path="D:/projects/canoe/runtime-a.cfg",
        ini_config="CASE-010=1",
    )
    second = CaseMapping(
        case_no="CASE-011",
        case_name="CANoe runtime case B",
        category="canoe",
        module="CANoe module",
        script_path="D:/projects/canoe/runtime-b.cfg",
        ini_config="CASE-011=1",
    )
    storage_path.write_text(
        json.dumps(
            {
                "mappings": {
                    "CASE-010": first.to_dict(),
                    "CASE-011": second.to_dict(),
                },
                "change_history": [],
                "version": "1.0",
                "last_updated": "2026-04-02 00:00:00",
            },
            ensure_ascii=False,
            indent=2,
        ),
        encoding="utf-8",
    )

    manager = CaseMappingManager(storage_path=str(storage_path))
    compiler = TaskCompiler(mapping_manager=manager)
    task = _make_task(
        toolType="CANoe",
        caseList=[
            {"caseNo": "CASE-010", "caseName": "Case 10", "caseType": "test_module"},
            {"caseNo": "CASE-011", "caseName": "Case 11", "caseType": "test_module"},
        ],
    )

    with pytest.raises(ConfigPreparationError, match="映射材料冲突"):
        compiler.compile(task)
