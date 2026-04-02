"""
Tests for runtime_config_helper: verifies build_adapter_config translates
UnifiedConfigManager settings into the per-adapter config dicts that each
adapter's __init__ expects.
"""

from __future__ import annotations

import os
import pytest
from unittest.mock import MagicMock

from core.adapters.base_adapter import TestToolType
from core.adapters.runtime_config_helper import (
    build_adapter_config,
    _build_ttworkbench_config,
    _build_tsmaster_config,
    _build_canoe_config,
)
from core.adapters.ttworkbench_adapter import TTworkbenchAdapter
from core.adapters.tsmaster_adapter import TSMasterAdapter
from core.adapters.canoe.adapter import CANoeAdapter


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

class _FakeConfigManager:
    """Minimal UnifiedConfigManager stand-in that returns values from _data."""

    def __init__(self, data: dict):
        self._data = data

    def get(self, key: str, default=None):
        parts = key.split(".")
        value = self._data
        for part in parts:
            if isinstance(value, dict) and part in value:
                value = value[part]
            else:
                return default
        return value


# ---------------------------------------------------------------------------
# TTworkbench
# ---------------------------------------------------------------------------

class TestBuildTTworkbenchConfig:
    """build_adapter_config produces correct TTworkbenchAdapter config."""

    def test_ttworkbench_returns_required_paths(self):
        cfg = _FakeConfigManager({
            "software": {
                "ttman_path": "C:\\Spirent\\TTman.bat",
                "workspace_path": "D:\\TestWorkspace",
            }
        })
        result = build_adapter_config(TestToolType.TTWORKBENCH, cfg)

        assert result["ttman_path"] == "C:\\Spirent\\TTman.bat"
        assert result["workspace_path"] == "D:\\TestWorkspace"

    def test_ttworkbench_derives_log_and_report_paths_under_workspace(self):
        cfg = _FakeConfigManager({
            "software": {
                "ttman_path": "C:\\Spirent\\TTman.bat",
                "workspace_path": "D:\\TestWorkspace",
            }
        })
        result = build_adapter_config(TestToolType.TTWORKBENCH, cfg)

        assert result["log_path"] == os.path.join("D:\\TestWorkspace", "logs")
        assert result["report_path"] == os.path.join("D:\\TestWorkspace", "reports")

    def test_ttworkbench_uses_explicit_log_and_report_paths_when_set(self):
        cfg = _FakeConfigManager({
            "software": {
                "ttman_path": "C:\\Spirent\\TTman.bat",
                "workspace_path": "D:\\TestWorkspace",
                "log_path": "E:\\CustomLogs",
                "report_path": "F:\\CustomReports",
            }
        })
        result = build_adapter_config(TestToolType.TTWORKBENCH, cfg)

        assert result["log_path"] == "E:\\CustomLogs"
        assert result["report_path"] == "F:\\CustomReports"

    def test_ttworkbench_defaults_for_optional_fields(self):
        cfg = _FakeConfigManager({
            "software": {
                "ttman_path": "C:\\Spirent\\TTman.bat",
                "workspace_path": "D:\\TestWorkspace",
            }
        })
        result = build_adapter_config(TestToolType.TTWORKBENCH, cfg)

        assert result["report_format"] == "pdf"
        assert result["log_format"] == "tlz"
        assert result["timeout"] == 3600

    def test_ttworkbench_accepts_string_tool_type(self):
        cfg = _FakeConfigManager({
            "software": {
                "ttman_path": "C:\\Spirent\\TTman.bat",
                "workspace_path": "D:\\TestWorkspace",
            }
        })
        result = build_adapter_config("ttworkbench", cfg)

        assert result["ttman_path"] == "C:\\Spirent\\TTman.bat"


class TestTTworkbenchAdapterInheritsRuntimeConfig:
    """TTworkbenchAdapter receives the intended runtime config when created via helper."""

    def test_ttworkbench_adapter_gets_ttman_path(self):
        cfg = _FakeConfigManager({
            "software": {
                "ttman_path": "C:\\Spirent\\TTman.bat",
                "workspace_path": "D:\\TestWorkspace",
            }
        })
        adapter_cfg = build_adapter_config(TestToolType.TTWORKBENCH, cfg)
        adapter = TTworkbenchAdapter(adapter_cfg)

        assert adapter.ttman_path == "C:\\Spirent\\TTman.bat"

    def test_ttworkbench_adapter_gets_workspace_path(self):
        cfg = _FakeConfigManager({
            "software": {
                "ttman_path": "C:\\Spirent\\TTman.bat",
                "workspace_path": "D:\\TestWorkspace",
            }
        })
        adapter_cfg = build_adapter_config(TestToolType.TTWORKBENCH, cfg)
        adapter = TTworkbenchAdapter(adapter_cfg)

        assert adapter.workspace_path == "D:\\TestWorkspace"

    def test_ttworkbench_adapter_gets_log_path(self):
        cfg = _FakeConfigManager({
            "software": {
                "ttman_path": "C:\\Spirent\\TTman.bat",
                "workspace_path": "D:\\TestWorkspace",
            }
        })
        adapter_cfg = build_adapter_config(TestToolType.TTWORKBENCH, cfg)
        adapter = TTworkbenchAdapter(adapter_cfg)

        assert adapter.log_path == os.path.join("D:\\TestWorkspace", "logs")

    def test_ttworkbench_adapter_gets_report_path(self):
        cfg = _FakeConfigManager({
            "software": {
                "ttman_path": "C:\\Spirent\\TTman.bat",
                "workspace_path": "D:\\TestWorkspace",
            }
        })
        adapter_cfg = build_adapter_config(TestToolType.TTWORKBENCH, cfg)
        adapter = TTworkbenchAdapter(adapter_cfg)

        assert adapter.report_path == os.path.join("D:\\TestWorkspace", "reports")


# ---------------------------------------------------------------------------
# TSMaster
# ---------------------------------------------------------------------------

class TestBuildTSMasterConfig:
    """build_adapter_config produces correct TSMasterAdapter config."""

    def test_tsmaster_returns_timeout_defaults(self):
        cfg = _FakeConfigManager({
            "tsmaster": {
                "timeout": 45,
                "config_timeout": 90,
            },
            "software": {
                "tsmaster_path": "C:\\TSMaster\\TSMaster.exe",
            },
        })
        result = build_adapter_config(TestToolType.TSMASTER, cfg)

        assert result["start_timeout"] == 45
        assert result["operation_timeout"] == 90

    def test_tsmaster_returns_rpc_defaults(self):
        cfg = _FakeConfigManager({
            "tsmaster": {
                "use_rpc": False,
                "fallback_to_traditional": False,
            },
            "software": {
                "tsmaster_path": "C:\\TSMaster\\TSMaster.exe",
            },
        })
        result = build_adapter_config(TestToolType.TSMASTER, cfg)

        assert result["use_rpc"] is False
        assert result["fallback_to_traditional"] is False

    def test_tsmaster_returns_master_form_settings(self):
        cfg = _FakeConfigManager({
            "tsmaster": {
                "master_form_name": "MyMaster",
                "auto_start_master": False,
                "auto_stop_master": False,
            },
            "software": {
                "tsmaster_path": "C:\\TSMaster\\TSMaster.exe",
            },
        })
        result = build_adapter_config(TestToolType.TSMASTER, cfg)

        assert result["master_form_name"] == "MyMaster"
        assert result["auto_start_master"] is False
        assert result["auto_stop_master"] is False


class TestTSMasterAdapterInheritsRuntimeConfig:
    """TSMasterAdapter receives the intended runtime config when created via helper."""

    def test_tsmaster_adapter_gets_operation_timeout(self):
        cfg = _FakeConfigManager({
            "tsmaster": {"config_timeout": 120},
            "software": {
                "tsmaster_path": "C:\\TSMaster\\TSMaster.exe",
            },
        })
        adapter_cfg = build_adapter_config(TestToolType.TSMASTER, cfg)
        adapter = TSMasterAdapter(adapter_cfg)

        assert adapter.operation_timeout == 120

    def test_tsmaster_adapter_gets_use_rpc_flag(self):
        cfg = _FakeConfigManager({
            "tsmaster": {"use_rpc": False},
            "software": {
                "tsmaster_path": "C:\\TSMaster\\TSMaster.exe",
            },
        })
        adapter_cfg = build_adapter_config(TestToolType.TSMASTER, cfg)
        adapter = TSMasterAdapter(adapter_cfg)

        assert adapter.use_rpc is False


# ---------------------------------------------------------------------------
# CANoe
# ---------------------------------------------------------------------------

class TestBuildCANoeConfig:
    """build_adapter_config produces correct CANoeAdapter config."""

    def test_canoe_returns_canoe_path(self):
        cfg = _FakeConfigManager({
            "software": {
                "canoe_path": "C:\\Program Files\\Vector\\CANoe 17\\Exec64\\CANoe64.exe"
            },
            "canoe": {},
        })
        result = build_adapter_config(TestToolType.CANOE, cfg)

        assert result["canoe_path"] == "C:\\Program Files\\Vector\\CANoe 17\\Exec64\\CANoe64.exe"

    def test_canoe_returns_timeout_settings(self):
        cfg = _FakeConfigManager({
            "software": {
                "canoe_path": "C:\\Program Files\\Vector\\CANoe 17\\Exec64\\CANoe64.exe",
            },
            "canoe": {
                "timeout": 60,
                "stop_timeout": 20,
                "measurement_timeout": 7200,
            },
        })
        result = build_adapter_config(TestToolType.CANOE, cfg)

        assert result["start_timeout"] == 60
        assert result["stop_timeout"] == 20
        assert result["measurement_timeout"] == 7200

    def test_canoe_returns_retry_settings(self):
        cfg = _FakeConfigManager({
            "software": {
                "canoe_path": "C:\\Program Files\\Vector\\CANoe 17\\Exec64\\CANoe64.exe",
            },
            "canoe": {
                "max_retries": 5,
                "retry_interval": 3.0,
            },
        })
        result = build_adapter_config(TestToolType.CANOE, cfg)

        assert result["retry_count"] == 5
        assert result["retry_interval"] == 3.0


class TestCANoeAdapterInheritsRuntimeConfig:
    """CANoeAdapter receives the intended runtime config when created via helper."""

    def test_canoe_adapter_gets_canoe_path(self):
        cfg = _FakeConfigManager({
            "software": {"canoe_path": "C:\\Program Files\\Vector\\CANoe 17\\Exec64\\CANoe64.exe"},
            "canoe": {},
        })
        adapter_cfg = build_adapter_config(TestToolType.CANOE, cfg)
        adapter = CANoeAdapter(adapter_cfg)

        assert adapter.config.get("canoe_path") == "C:\\Program Files\\Vector\\CANoe 17\\Exec64\\CANoe64.exe"


# ---------------------------------------------------------------------------
# Unknown tool type
# ---------------------------------------------------------------------------

class TestUnknownToolType:
    def test_unknown_string_tool_type_raises_value_error(self):
        """An unknown string tool type should raise ValueError (not silently return {})."""
        cfg = _FakeConfigManager({})
        from core.adapters.runtime_config_helper import build_adapter_config

        with pytest.raises(ValueError, match="not a valid TestToolType"):
            build_adapter_config("unknown_tool", cfg)
