import json
from pathlib import Path

import pytest

from core.case_mapping_manager import CaseMappingManager
from models.case_mapping import CaseMapping


@pytest.fixture(autouse=True)
def reset_case_mapping_singleton():
    CaseMappingManager._instance = None
    yield
    CaseMappingManager._instance = None


def _write_legacy_mapping_store(storage_path: Path, mappings):
    payload = {
        "mappings": {mapping.case_no: mapping.to_dict() for mapping in mappings},
        "change_history": [],
        "version": "1.0",
        "last_updated": "2026-04-02 00:00:00",
    }
    storage_path.parent.mkdir(parents=True, exist_ok=True)
    storage_path.write_text(json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8")


def test_get_mapping_view_projects_tsmaster_material_from_legacy_fields(tmp_path):
    storage_path = tmp_path / "case_mappings.json"
    _write_legacy_mapping_store(
        storage_path,
        [
            CaseMapping(
                case_no="CASE-001",
                case_name="TSMaster selection case",
                category="tsmaster",
                module="TSMaster module",
                script_path="D:/projects/tsmaster/project",
                ini_config="CASE-001=1",
                description="legacy tsmaster record",
            )
        ],
    )

    manager = CaseMappingManager(storage_path=str(storage_path))
    view = manager.get_mapping_view("CASE-001")

    assert view is not None
    assert view.case_no == "CASE-001"
    assert view.tool_type == "tsmaster"
    assert view.declaration.module == "TSMaster module"
    assert view.material.tsmaster is not None
    assert view.material.canoe is None
    assert view.material.ttworkbench is None
    assert view.material.tsmaster.selection_key == "CASE-001"
    assert view.material.tsmaster.ini_config == "CASE-001=1"
    assert view.material.tsmaster.project_path == "D:/projects/tsmaster/project"


def test_get_mapping_view_projects_canoe_material_from_legacy_fields(tmp_path):
    storage_path = tmp_path / "case_mappings.json"
    _write_legacy_mapping_store(
        storage_path,
        [
            CaseMapping(
                case_no="CASE-002",
                case_name="CANoe config case",
                category="canoe",
                module="CANoe module",
                script_path="D:/projects/canoe/config.cfg",
                ini_config="CANOE_TEST=1",
                para_config="para_a=1",
                description="legacy canoe record",
            )
        ],
    )

    manager = CaseMappingManager(storage_path=str(storage_path))
    view = manager.get_mapping_view("CASE-002")

    assert view is not None
    assert view.case_no == "CASE-002"
    assert view.tool_type == "canoe"
    assert view.material.canoe is not None
    assert view.material.tsmaster is None
    assert view.material.ttworkbench is None
    assert view.material.canoe.config_path == "D:/projects/canoe/config.cfg"
    assert view.material.canoe.module_name == "CANoe module"
    assert view.material.canoe.ini_config == "CANOE_TEST=1"
    assert view.material.canoe.para_config == "para_a=1"


def test_iter_mapping_views_projects_ttworkbench_material_from_legacy_fields(tmp_path):
    storage_path = tmp_path / "case_mappings.json"
    _write_legacy_mapping_store(
        storage_path,
        [
            CaseMapping(
                case_no="CASE-001",
                case_name="TSMaster selection case",
                category="tsmaster",
                script_path="D:/projects/tsmaster/project",
                ini_config="CASE-001=1",
            ),
            CaseMapping(
                case_no="CASE-002",
                case_name="CANoe config case",
                category="canoe",
                script_path="D:/projects/canoe/config.cfg",
                ini_config="CANOE_TEST=1",
            ),
            CaseMapping(
                case_no="CASE-003",
                case_name="TTworkbench build case",
                category="ttworkbench",
                ttcn3_source="D:/projects/ttworkbench/src/test.ttcn3",
                ttthree_path="D:/tools/tt3/bin/tt3.exe",
                clf_file="D:/projects/ttworkbench/out/test.clf",
            ),
        ],
    )

    manager = CaseMappingManager(storage_path=str(storage_path))
    views = {view.case_no: view for view in manager.iter_mapping_views()}

    assert set(views) == {"CASE-001", "CASE-002", "CASE-003"}
    assert views["CASE-003"].tool_type == "ttworkbench"
    assert views["CASE-003"].material.ttworkbench is not None
    assert views["CASE-003"].material.canoe is None
    assert views["CASE-003"].material.tsmaster is None
    assert views["CASE-003"].material.ttworkbench.clf_file == "D:/projects/ttworkbench/out/test.clf"
    assert views["CASE-003"].material.ttworkbench.ttcn3_source == "D:/projects/ttworkbench/src/test.ttcn3"
    assert views["CASE-003"].material.ttworkbench.ttthree_path == "D:/tools/tt3/bin/tt3.exe"
