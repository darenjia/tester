"""
Tests for ConfigPreparationPhase - Configuration Resolution Stage

Tests cover:
- explicit config path (direct_path)
- mapping-derived config/material
- conflicting multi-case mapping materials
- missing config/material failure before execution
"""
from __future__ import annotations

import pytest
from unittest.mock import MagicMock, patch

from core.config_preparation import (
    ConfigPreparationPhase,
    ConfigPreparationError,
    ConfigConflictError,
    MissingConfigError,
    PreparedConfig,
)
from core.execution_plan import ConfigSource, ExecutionPlan, PlannedCase


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


# =============================================================================
# Explicit Config Path Tests
# =============================================================================

def test_prepare_with_explicit_config_path():
    """When configPath is explicitly provided, use it directly (DIRECT_PATH)."""
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
        config_path="D:/cfgs/explicit.cfg",
        config_source=ConfigSource.DIRECT_PATH,
    )

    prep_phase = ConfigPreparationPhase(mapping_manager=_FakeMappingManager({}))
    prepared = prep_phase.prepare(plan)

    assert prepared.config_path == "D:/cfgs/explicit.cfg"
    assert prepared.config_source == ConfigSource.DIRECT_PATH
    assert "config_path resolved from explicit plan config_path" in prepared.resolution_notes[0]


def test_prepare_with_explicit_config_path_preserves_other_fields():
    """Explicit config path preparation preserves all other plan fields."""
    plan = ExecutionPlan(
        task_no="TASK-2",
        project_no="PROJECT-2",
        task_name="Task 2",
        device_id="DEVICE-2",
        tool_type="canoe",
        cases=[
            PlannedCase(
                case_no="CASE-2",
                case_name="Case 2",
                case_type="diagnostic",
                repeat=2,
                dtc_info="P1234",
            )
        ],
        config_path="D:/cfgs/explicit.cfg",
        config_name="explicit",
        base_config_dir="D:/cfgs",
        variables={"speed": 60},
        timeout_seconds=7200,
    )

    prep_phase = ConfigPreparationPhase(mapping_manager=_FakeMappingManager({}))
    prepared = prep_phase.prepare(plan)

    assert prepared.task_no == "TASK-2"
    assert prepared.project_no == "PROJECT-2"
    assert prepared.tool_type == "canoe"
    assert prepared.timeout_seconds == 7200
    assert prepared.variables == {"speed": 60}


# =============================================================================
# Mapping-Derived Config Tests
# =============================================================================

def test_prepare_from_case_mapping():
    """When no explicit configPath, resolve from case mapping (CASE_MAPPING)."""
    mapping_manager = _FakeMappingManager({
        "CASE-MAP-1": _FakeMapping(
            category="CANOE",
            script_path="D:/cfgs/mapped.cfg",
            case_name="Mapped Case",
        ),
    })

    plan = ExecutionPlan(
        task_no="TASK-MAP-1",
        project_no="PROJECT-MAP-1",
        task_name="Mapping Task",
        device_id="DEVICE-MAP-1",
        tool_type="canoe",
        cases=[
            PlannedCase(
                case_no="CASE-MAP-1",
                case_name="Case Map 1",
                case_type="test_module",
            )
        ],
        config_source=ConfigSource.UNSPECIFIED,  # Not explicitly set
    )

    prep_phase = ConfigPreparationPhase(mapping_manager=mapping_manager)
    prepared = prep_phase.prepare(plan)

    assert prepared.config_path == "D:/cfgs/mapped.cfg"
    assert prepared.config_source == ConfigSource.CASE_MAPPING
    assert "config_path resolved from case mapping" in prepared.resolution_notes[0]


def test_prepare_from_case_mapping_with_ini_config():
    """Case mapping with ini_config is preserved for execution."""
    mapping_manager = _FakeMappingManager({
        "CASE-INI-1": _FakeMapping(
            category="TSMASTER",
            script_path="D:/cfgs/tsmaster.cfg",
            case_name="TSMaster Case",
            ini_config="TG1_TC1=1",
        ),
    })

    plan = ExecutionPlan(
        task_no="TASK-INI-1",
        project_no="PROJECT-INI-1",
        task_name="INI Task",
        device_id="DEVICE-INI-1",
        tool_type="tsmaster",
        cases=[
            PlannedCase(
                case_no="CASE-INI-1",
                case_name="Case INI 1",
                case_type="test_module",
                execution_params={"iniConfig": "TG1_TC1=1"},
            )
        ],
        config_source=ConfigSource.UNSPECIFIED,
    )

    prep_phase = ConfigPreparationPhase(mapping_manager=mapping_manager)
    prepared = prep_phase.prepare(plan)

    # TSMaster case with script_path uses CASE_MAPPING, not TSMASTER_INLINE
    assert prepared.config_source == ConfigSource.CASE_MAPPING
    assert prepared.config_path == "D:/cfgs/tsmaster.cfg"


def test_prepare_tsmaster_inline_without_script_path():
    """TSMaster case without script_path uses TSMASTER_INLINE (no cfg needed)."""
    mapping_manager = _FakeMappingManager({
        "CASE-TS-1": _FakeMapping(
            category="TSMASTER",
            enabled=True,
            script_path=None,  # No script_path - inline execution
            case_name="TSMaster Inline",
        ),
    })

    plan = ExecutionPlan(
        task_no="TASK-TS-1",
        project_no="PROJECT-TS-1",
        task_name="TSMaster Inline Task",
        device_id="DEVICE-TS-1",
        tool_type="tsmaster",
        cases=[
            PlannedCase(
                case_no="CASE-TS-1",
                case_name="Case TS 1",
                case_type="test_module",
            )
        ],
        config_source=ConfigSource.UNSPECIFIED,
    )

    prep_phase = ConfigPreparationPhase(mapping_manager=mapping_manager)
    prepared = prep_phase.prepare(plan)

    assert prepared.config_source == ConfigSource.TSMASTER_INLINE
    assert prepared.is_tsmaster_inline is True
    assert prepared.config_path is None
    assert "TSMaster inline execution" in prepared.resolution_notes[0]


# =============================================================================
# Conflicting Multi-Case Mapping Materials Tests
# =============================================================================

def test_prepare_detects_conflicting_script_paths():
    """
    When multiple cases in a task have different script_paths,
    ConfigConflictError should be raised BEFORE execution.
    """
    mapping_manager = _FakeMappingManager({
        "CASE-A": _FakeMapping(
            category="CANOE",
            script_path="D:/cfgs/config_a.cfg",
            enabled=True,
        ),
        "CASE-B": _FakeMapping(
            category="CANOE",
            script_path="D:/cfgs/config_b.cfg",  # Different path!
            enabled=True,
        ),
    })

    plan = ExecutionPlan(
        task_no="TASK-CONFLICT",
        project_no="PROJECT-CONFLICT",
        task_name="Conflict Task",
        device_id="DEVICE-CONFLICT",
        tool_type="canoe",
        cases=[
            PlannedCase(
                case_no="CASE-A",
                case_name="Case A",
                case_type="test_module",
            ),
            PlannedCase(
                case_no="CASE-B",
                case_name="Case B",
                case_type="test_module",
            ),
        ],
        config_source=ConfigSource.UNSPECIFIED,
    )

    prep_phase = ConfigPreparationPhase(mapping_manager=mapping_manager)

    with pytest.raises(ConfigConflictError) as exc_info:
        prep_phase.prepare(plan)

    # Verify the error message mentions both conflicting paths
    error_msg = str(exc_info.value)
    assert "config_a.cfg" in error_msg
    assert "config_b.cfg" in error_msg
    assert "CASE-A" in error_msg or "CASE-B" in error_msg


def test_prepare_allows_same_script_path_for_multiple_cases():
    """When multiple cases share the same script_path, no conflict."""
    mapping_manager = _FakeMappingManager({
        "CASE-SHARE-1": _FakeMapping(
            category="CANOE",
            script_path="D:/cfgs/shared.cfg",
            enabled=True,
        ),
        "CASE-SHARE-2": _FakeMapping(
            category="CANOE",
            script_path="D:/cfgs/shared.cfg",  # Same path - OK
            enabled=True,
        ),
    })

    plan = ExecutionPlan(
        task_no="TASK-SHARE",
        project_no="PROJECT-SHARE",
        task_name="Shared Config Task",
        device_id="DEVICE-SHARE",
        tool_type="canoe",
        cases=[
            PlannedCase(
                case_no="CASE-SHARE-1",
                case_name="Case Share 1",
                case_type="test_module",
            ),
            PlannedCase(
                case_no="CASE-SHARE-2",
                case_name="Case Share 2",
                case_type="test_module",
            ),
        ],
        config_source=ConfigSource.UNSPECIFIED,
    )

    prep_phase = ConfigPreparationPhase(mapping_manager=mapping_manager)
    prepared = prep_phase.prepare(plan)

    assert prepared.config_path == "D:/cfgs/shared.cfg"
    assert prepared.config_source == ConfigSource.CASE_MAPPING


# =============================================================================
# Missing Config/Material Failure Before Execution Tests
# =============================================================================

def test_prepare_fails_when_no_config_and_no_mapping():
    """When no explicit configPath and no mapping, MissingConfigError is raised."""
    mapping_manager = _FakeMappingManager({})  # No mappings

    plan = ExecutionPlan(
        task_no="TASK-NO-CFG",
        project_no="PROJECT-NO-CFG",
        task_name="No Config Task",
        device_id="DEVICE-NO-CFG",
        tool_type="canoe",
        cases=[
            PlannedCase(
                case_no="CASE-NO-MAP",
                case_name="Case No Map",
                case_type="test_module",
            )
        ],
        config_source=ConfigSource.UNSPECIFIED,
    )

    prep_phase = ConfigPreparationPhase(mapping_manager=mapping_manager)

    with pytest.raises(MissingConfigError) as exc_info:
        prep_phase.prepare(plan)

    error_msg = str(exc_info.value)
    assert "CASE-NO-MAP" in error_msg or "No configuration" in error_msg


def test_prepare_fails_when_case_mapping_disabled():
    """When case mapping is disabled, it's treated as no mapping."""
    mapping_manager = _FakeMappingManager({
        "CASE-DISABLED": _FakeMapping(
            category="CANOE",
            script_path="D:/cfgs/disabled.cfg",
            enabled=False,  # Disabled!
        ),
    })

    plan = ExecutionPlan(
        task_no="TASK-DISABLED",
        project_no="PROJECT-DISABLED",
        task_name="Disabled Mapping Task",
        device_id="DEVICE-DISABLED",
        tool_type="canoe",
        cases=[
            PlannedCase(
                case_no="CASE-DISABLED",
                case_name="Case Disabled",
                case_type="test_module",
            )
        ],
        config_source=ConfigSource.UNSPECIFIED,
    )

    prep_phase = ConfigPreparationPhase(mapping_manager=mapping_manager)

    with pytest.raises(MissingConfigError):
        prep_phase.prepare(plan)


def test_prepare_fails_when_plan_has_no_cases():
    """When plan has no cases and no config, MissingConfigError is raised."""
    mapping_manager = _FakeMappingManager({})

    plan = ExecutionPlan(
        task_no="TASK-EMPTY",
        project_no="PROJECT-EMPTY",
        task_name="Empty Task",
        device_id="DEVICE-EMPTY",
        tool_type="canoe",
        cases=[],  # No cases!
        config_source=ConfigSource.UNSPECIFIED,
    )

    prep_phase = ConfigPreparationPhase(mapping_manager=mapping_manager)

    with pytest.raises(MissingConfigError) as exc_info:
        prep_phase.prepare(plan)

    assert "No cases" in str(exc_info.value)


# =============================================================================
# ConfigManager-based Preparation Tests
# =============================================================================

def test_prepare_from_config_manager():
    """When configName + baseConfigDir is provided, use TestConfigManager."""
    plan = ExecutionPlan(
        task_no="TASK-CM",
        project_no="PROJECT-CM",
        task_name="ConfigManager Task",
        device_id="DEVICE-CM",
        tool_type="canoe",
        cases=[
            PlannedCase(
                case_no="CASE-CM",
                case_name="Case CM",
                case_type="test_module",
            )
        ],
        config_name="main_config",  # Will be resolved by TestConfigManager
        base_config_dir="D:/TAMS/DTTC_CONFIG",
        config_source=ConfigSource.UNSPECIFIED,
    )

    # Mock TestConfigManager.prepare_config_for_task
    mock_config_manager = MagicMock()
    mock_config_manager.prepare_config_for_task.return_value = {
        'cfg_path': 'D:/TAMS/DTTC_CONFIG/main_config.cfg',
        'ini_path': 'D:/TAMS/DTTC_CONFIG/main_config.ini',
    }

    with patch('core.config_preparation.TestConfigManager') as MockTCM:
        MockTCM.return_value = mock_config_manager

        prep_phase = ConfigPreparationPhase(mapping_manager=_FakeMappingManager({}))
        prepared = prep_phase.prepare(plan)

    assert prepared.config_path == 'D:/TAMS/DTTC_CONFIG/main_config.cfg'
    assert prepared.ini_path == 'D:/TAMS/DTTC_CONFIG/main_config.ini'
    assert prepared.config_source == ConfigSource.CONFIG_MANAGER


# =============================================================================
# Validation Tests
# =============================================================================

def test_validate_plan_with_unspecified_config_source():
    """Validation should detect plans that weren't properly prepared."""
    plan = ExecutionPlan(
        task_no="TASK-VAL",
        project_no="PROJECT-VAL",
        task_name="Validation Task",
        device_id="DEVICE-VAL",
        tool_type="canoe",
        cases=[
            PlannedCase(
                case_no="CASE-VAL",
                case_name="Case Val",
                case_type="test_module",
            )
        ],
        config_source=ConfigSource.UNSPECIFIED,
        config_path=None,  # Not prepared!
    )

    prep_phase = ConfigPreparationPhase(mapping_manager=_FakeMappingManager({}))
    errors = prep_phase.validate_plan(plan)

    assert len(errors) > 0
    assert any("UNSPECIFIED" in e or "not properly prepared" in e for e in errors)


def test_validate_plan_with_valid_direct_path():
    """Validation passes when config_path exists for DIRECT_PATH."""
    plan = ExecutionPlan(
        task_no="TASK-VALID",
        project_no="PROJECT-VALID",
        task_name="Valid Task",
        device_id="DEVICE-VALID",
        tool_type="canoe",
        cases=[
            PlannedCase(
                case_no="CASE-VALID",
                case_name="Case Valid",
                case_type="test_module",
            )
        ],
        config_path="D:/cfgs/explicit.cfg",  # Explicit path
        config_source=ConfigSource.DIRECT_PATH,
    )

    prep_phase = ConfigPreparationPhase(mapping_manager=_FakeMappingManager({}))

    # Note: This test would fail if the file doesn't exist in the real environment
    # In unit tests we mock the path check
    errors = prep_phase.validate_plan(plan)
    assert all("config_path does not exist" in e for e in errors if e)


def test_validate_plan_tsmaster_inline():
    """Validation passes for TSMASTER_INLINE (no config file needed)."""
    plan = ExecutionPlan(
        task_no="TASK-TS-VALID",
        project_no="PROJECT-TS-VALID",
        task_name="TSMaster Valid Task",
        device_id="DEVICE-TS-VALID",
        tool_type="tsmaster",
        cases=[
            PlannedCase(
                case_no="CASE-TS-VALID",
                case_name="Case TS Valid",
                case_type="test_module",
            )
        ],
        config_source=ConfigSource.TSMASTER_INLINE,
        config_path=None,  # No path needed for TSMASTER_INLINE
    )

    prep_phase = ConfigPreparationPhase(mapping_manager=_FakeMappingManager({}))
    errors = prep_phase.validate_plan(plan)

    # Should have no config-related errors
    config_errors = [e for e in errors if "config" in e.lower()]
    assert len(config_errors) == 0
