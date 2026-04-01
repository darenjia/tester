from __future__ import annotations

from core.execution_plan import ConfigSource, ExecutionPlan, PlannedCase


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
