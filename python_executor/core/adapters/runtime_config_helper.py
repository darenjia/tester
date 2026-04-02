"""
Runtime config to adapter config translation helper.

Provides build_adapter_config() that translates UnifiedConfigManager runtime
settings into the per-adapter config dict expected by each adapter's __init__.

This ensures executor-created adapters receive the runtime settings they need
to actually run in production (paths, timeouts, RPC mode, etc.).
"""

from __future__ import annotations

import os
from typing import TYPE_CHECKING, Any

from core.adapters.base_adapter import TestToolType

if TYPE_CHECKING:
    from config.unified_config import UnifiedConfigManager


def _require_path(software: dict, key: str, tool_name: str) -> str:
    """Return path value or raise ValueError if empty."""
    path = software.get(key, "")
    if not path:
        raise ValueError(f"{key} is required for {tool_name}")
    return path


def build_adapter_config(
    tool_type: TestToolType,
    runtime_config: UnifiedConfigManager,
) -> dict[str, Any]:
    """
    Translate runtime config into adapter-specific config.

    Call this when creating adapters inside the executor so each adapter
    receives the runtime settings it needs to connect and execute.

    Args:
        tool_type: The type of adapter (CANOE, TSMASTER, TTWORKBENCH).
        runtime_config: UnifiedConfigManager instance (from config.settings).

    Returns:
        Adapter-specific config dict to pass to adapter.__init__().
        Empty dict if the tool type is not recognised.
    """
    normalized = _normalize_tool_type(tool_type)

    if normalized == TestToolType.TTWORKBENCH:
        return _build_ttworkbench_config(runtime_config)
    elif normalized == TestToolType.TSMASTER:
        return _build_tsmaster_config(runtime_config)
    elif normalized == TestToolType.CANOE:
        return _build_canoe_config(runtime_config)

    return {}


def _normalize_tool_type(tool_type: TestToolType | str) -> TestToolType:
    """Normalise a tool type to TestToolType enum."""
    if isinstance(tool_type, TestToolType):
        return tool_type
    from core.adapters.base_adapter import TestToolType as T

    normalized = str(tool_type).strip().lower()
    return T(normalized)


# ---------------------------------------------------------------------------
# Per-adapter builders
# ---------------------------------------------------------------------------

def _build_ttworkbench_config(runtime_config: UnifiedConfigManager) -> dict[str, Any]:
    """
    Build TTworkbenchAdapter config from runtime config.

    Required fields (all read from software section):
        - ttman_path   -> software.ttman_path
        - workspace_path -> software.workspace_path

    Derived fields (placed under workspace_path):
        - log_path   -> software.workspace_path / logs
        - report_path -> software.workspace_path / reports

    Optional fields:
        - report_format (default: pdf)
        - log_format   (default: tlz)
        - timeout      (default: 3600)
    """
    software = runtime_config.get("software", {})

    ttman_path = software.get("ttman_path", "")
    if not ttman_path:
        raise ValueError("ttman_path is required for TTworkbench adapter")

    workspace = software.get("workspace_path", "")

    log_path = software.get("log_path", "")
    if not log_path and workspace:
        log_path = os.path.join(workspace, "logs")

    report_path = software.get("report_path", "")
    if not report_path and workspace:
        report_path = os.path.join(workspace, "reports")

    return {
        "ttman_path": ttman_path,
        "workspace_path": workspace,
        "log_path": log_path,
        "report_path": report_path,
        "report_format": software.get("report_format", "pdf"),
        "log_format": software.get("log_format", "tlz"),
        "timeout": software.get("timeout", 3600),
    }


def _build_tsmaster_config(runtime_config: UnifiedConfigManager) -> dict[str, Any]:
    """
    Build TSMasterAdapter config from runtime config.

    Runtime defaults supplied:
        - start_timeout   -> tsmaster.timeout        (default 30)
        - stop_timeout    -> tsmaster.stop_timeout   (default 10)
        - operation_timeout -> tsmaster.config_timeout (default 60)
        - use_rpc         -> tsmaster.use_rpc        (default True)
        - fallback_to_traditional -> tsmaster.fallback_to_traditional (default True)

    Paths (from software section):
        - tsmaster_path
    """
    tsmaster_cfg = runtime_config.get("tsmaster", {})
    software = runtime_config.get("software", {})

    return {
        "start_timeout": tsmaster_cfg.get("timeout", 30),
        "stop_timeout": tsmaster_cfg.get("stop_timeout", 10),
        "operation_timeout": tsmaster_cfg.get("config_timeout", 60),
        "use_rpc": tsmaster_cfg.get("use_rpc", True),
        "fallback_to_traditional": tsmaster_cfg.get("fallback_to_traditional", True),
        "master_form_name": tsmaster_cfg.get("master_form_name", "C 代码编辑器 [Master]"),
        "auto_start_master": tsmaster_cfg.get("auto_start_master", True),
        "auto_stop_master": tsmaster_cfg.get("auto_stop_master", True),
        "tsmaster_path": _require_path(software, "tsmaster_path", "TSMaster adapter"),
    }


def _build_canoe_config(runtime_config: UnifiedConfigManager) -> dict[str, Any]:
    """
    Build CANoeAdapter config from runtime config.

    Paths:
        - canoe_path -> software.canoe_path

    Timeouts / retry (from canoe section):
        - start_timeout  (default 30)
        - stop_timeout   (default 10)
        - measurement_timeout (default 3600)
        - open_timeout   (default 30)
        - case_timeout   (default 600)
        - self_check_timeout (default 300)
        - retry_count    (default 3)
        - retry_interval (default 2.0)
    """
    canoe_cfg = runtime_config.get("canoe", {})
    software = runtime_config.get("software", {})

    return {
        "start_timeout": canoe_cfg.get("timeout", 30),
        "stop_timeout": canoe_cfg.get("stop_timeout", 10),
        "measurement_timeout": canoe_cfg.get("measurement_timeout", 3600),
        "open_timeout": canoe_cfg.get("open_timeout", 30),
        "case_timeout": canoe_cfg.get("case_timeout", 600),
        "self_check_timeout": canoe_cfg.get("self_check_timeout", 300),
        "retry_count": canoe_cfg.get("max_retries", 3),
        "retry_interval": canoe_cfg.get("retry_interval", 2.0),
        "canoe_version": canoe_cfg.get("version"),
        "canoe_path": _require_path(software, "canoe_path", "CANoe adapter"),
    }
