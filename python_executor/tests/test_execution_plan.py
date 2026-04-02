from __future__ import annotations

from core.execution_plan import ConfigSource, ExecutionPlan, PlannedCase
from models.task import Case, Task


def test_execution_plan_defaults_execution_constraints():
    plan = ExecutionPlan(
        task_no="TASK-1",
        project_no="PROJECT-1",
        task_name="Task 1",
        device_id="DEVICE-1",
        tool_type="canoe",
        cases=[
            PlannedCase(
                case_no="CASE-1",
                case_name="Case 1",
                case_type="test_module",
            )
        ],
    )

    assert plan.timeout_seconds == 3600
    assert plan.max_concurrency == 1
    assert plan.report_required is True
    assert plan.config_source is ConfigSource.UNSPECIFIED


def test_planned_case_preserves_execution_fields_only():
    planned_case = PlannedCase(
        case_no="CASE-2",
        case_name="Case 2",
        case_type="diagnostic",
        repeat=2,
        dtc_info="P1234",
        execution_params={"iniConfig": "TG1_TC2=1"},
        mapping_metadata={"category": "tsmaster"},
    )

    assert planned_case.case_no == "CASE-2"
    assert planned_case.repeat == 2
    assert planned_case.execution_params["iniConfig"] == "TG1_TC2=1"
    assert planned_case.mapping_metadata["category"] == "tsmaster"


def test_execution_plan_can_be_created_from_legacy_task():
    legacy_task = Task(
        projectNo="PROJECT-3",
        taskNo="TASK-3",
        taskName="Legacy Task",
        deviceId="DEVICE-3",
        toolType="canoe",
        configPath="D:/cfgs/legacy.cfg",
        timeout=120,
        variables={"speed": 60},
        caseList=[
            Case(
                caseNo="CASE-3",
                caseName="Case 3",
                caseType="test_module",
                params={"voltage": 12},
                repeat=2,
            )
        ],
    )

    plan = ExecutionPlan.from_legacy_task(legacy_task)

    assert plan.task_no == "TASK-3"
    assert plan.project_no == "PROJECT-3"
    assert plan.task_name == "Legacy Task"
    assert plan.device_id == "DEVICE-3"
    assert plan.tool_type == "canoe"
    assert plan.config_path == "D:/cfgs/legacy.cfg"
    assert plan.timeout_seconds == 120
    assert plan.variables == {"speed": 60}
    assert plan.cases[0].case_no == "CASE-3"
    assert plan.cases[0].execution_params == {"voltage": 12}
    assert plan.cases[0].repeat == 2
