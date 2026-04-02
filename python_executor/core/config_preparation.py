"""
Config Preparation Phase - Front-Loads Configuration Resolution

This module provides a dedicated configuration/material preparation stage
that runs between task compilation and execution. It resolves all configuration
sources explicitly before the executor begins running a task.

Responsibilities:
- Resolve config_path from explicit path, config_name, or case mapping
- Detect conflicting multi-case mapping materials
- Validate required config/material is available before execution
- Ensure prepared plans carry enough information for all execution modes:
  - direct config path execution
  - generated/prepared config execution
  - mapping-material-only execution
  - tool-runtime-only execution
"""
from __future__ import annotations

import os
from dataclasses import dataclass, field
from typing import Any, Optional

from utils.logger import get_logger
from utils.exceptions import TaskException
from core.execution_plan import ConfigSource, ExecutionPlan, PlannedCase
from core.config_manager import TestConfigManager
from core.case_mapping_manager import get_case_mapping_manager

logger = get_logger("config_preparation")


class ConfigPreparationError(ValueError):
    """Raised when configuration preparation fails."""
    pass


class ConfigConflictError(ConfigPreparationError):
    """Raised when conflicting configuration sources are detected."""
    pass


class MissingConfigError(ConfigPreparationError):
    """Raised when required configuration is not available."""
    pass


@dataclass
class PreparedConfig:
    """Result of configuration preparation."""
    cfg_path: str | None = None
    ini_path: str | None = None
    config_source: ConfigSource = ConfigSource.UNSPECIFIED
    resolution_notes: list[str] = field(default_factory=list)
    is_tsmaster_inline: bool = False
    raw_config_info: dict[str, Any] = field(default_factory=dict)


@dataclass
class ConfigPreparationPhase:
    """
    Dedicated configuration preparation stage.

    Resolves all configuration/material sources explicitly before execution,
    detecting conflicts and validating availability.
    """

    mapping_manager: Any = None
    base_config_dir: str | None = None

    def __post_init__(self):
        if self.mapping_manager is None:
            self.mapping_manager = get_case_mapping_manager()

    def prepare(self, plan: ExecutionPlan) -> ExecutionPlan:
        """
        Prepare configuration for the given execution plan.

        This method resolves all configuration sources and returns a new
        ExecutionPlan with resolved config_path and config_source.

        Args:
            plan: The execution plan to prepare

        Returns:
            A new ExecutionPlan with resolved configuration

        Raises:
            ConfigConflictError: When conflicting configuration sources detected
            MissingConfigError: When required configuration is not available
        """
        logger.info(f"[ConfigPreparation] Preparing plan: task_no={plan.task_no}")

        prepared = self._prepare_config(plan)

        # Update plan with prepared config
        resolved_plan = self._apply_prepared_config(plan, prepared)

        logger.info(
            f"[ConfigPreparation] Done: config_source={resolved_plan.config_source}, "
            f"config_path={resolved_plan.config_path}, "
            f"resolution_notes={resolved_plan.resolution_notes}"
        )

        return resolved_plan

    def _prepare_config(self, plan: ExecutionPlan) -> PreparedConfig:
        """
        Resolve configuration sources for the execution plan.

        Resolution order:
        1. Explicit config_path in plan (direct path execution)
        2. config_name + base_config_dir via TestConfigManager (generated/prepared config)
        3. Case mapping derived script_path (mapping-material-only)
        4. TSMaster inline config (tool-runtime-only)
        """
        # 1. Direct path - already specified in plan
        if plan.config_path:
            return PreparedConfig(
                cfg_path=plan.config_path,
                config_source=ConfigSource.DIRECT_PATH,
                resolution_notes=[f"config_path resolved from explicit plan config_path"],
            )

        # 2. Config name + base dir - generate via TestConfigManager
        if plan.config_name or plan.base_config_dir:
            return self._prepare_from_config_manager(plan)

        # 3. Case mapping derived - resolve from mappings
        return self._prepare_from_case_mappings(plan)

    def _prepare_from_config_manager(self, plan: ExecutionPlan) -> PreparedConfig:
        """Prepare config using TestConfigManager."""
        base_dir = plan.base_config_dir or self.base_config_dir or r'D:\TAMS\DTTC_CONFIG'

        config_manager = TestConfigManager(base_config_dir=base_dir)
        config_name = plan.config_name

        # Determine config name from plan
        if not config_name and plan.config_path:
            config_name = os.path.basename(plan.config_path).replace('.cfg', '')

        if not config_name:
            raise MissingConfigError(
                "config_name not specified and could not be derived for config_manager preparation"
            )

        # Build case dicts for config preparation
        case_dicts = []
        for case in plan.cases:
            case_dicts.append({
                'caseNo': case.case_no,
                'caseName': case.case_name,
                'caseType': case.case_type,
                'repeat': case.repeat,
                'dtcInfo': case.dtc_info,
                'params': case.execution_params,
            })

        config_info = config_manager.prepare_config_for_task(
            task_config_name=config_name,
            test_cases=case_dicts,
            variables=plan.variables,
        )

        cfg_path = config_info.get('cfg_path')
        ini_path = config_info.get('ini_path')

        return PreparedConfig(
            cfg_path=cfg_path,
            ini_path=ini_path,
            config_source=ConfigSource.CONFIG_MANAGER,
            resolution_notes=[
                f"config prepared via TestConfigManager: config_name={config_name}, base_dir={base_dir}"
            ],
            raw_config_info=config_info,
        )

    def _prepare_from_case_mappings(self, plan: ExecutionPlan) -> PreparedConfig:
        """
        Prepare config from case mappings.

        Raises:
            ConfigConflictError: When multiple cases have different script_paths
            MissingConfigError: When no mapping-derived config is available
        """
        if not plan.cases:
            raise MissingConfigError("No cases in plan and no explicit config path")

        # Collect script_paths from mappings, checking for conflicts
        script_paths: dict[str, list[str]] = {}  # script_path -> [case_nos]
        tsmaster_cases: list[str] = []
        no_mapping_cases: list[str] = []

        for case in plan.cases:
            case_no = case.case_no
            mapping = self.mapping_manager.get_mapping(case_no) if self.mapping_manager else None

            if mapping is None:
                no_mapping_cases.append(case_no)
                continue

            if not mapping.enabled:
                no_mapping_cases.append(case_no)
                continue

            # Check for TSMaster inline config (TSMaster without script_path)
            if mapping.category and mapping.category.lower() == 'tsmaster':
                if mapping.script_path:
                    # TSMaster WITH script_path is treated like CASE_MAPPING
                    if mapping.script_path not in script_paths:
                        script_paths[mapping.script_path] = []
                    script_paths[mapping.script_path].append(case_no)
                else:
                    # TSMaster WITHOUT script_path is TSMASTER_INLINE
                    tsmaster_cases.append(case_no)
                continue

            # Collect script_path
            if mapping.script_path:
                if mapping.script_path not in script_paths:
                    script_paths[mapping.script_path] = []
                script_paths[mapping.script_path].append(case_no)

        # Detect TSMaster inline case (no cfg needed)
        if tsmaster_cases and not script_paths:
            # All cases are TSMaster inline - tool-runtime-only execution
            logger.info(f"Detected TSMaster inline cases: {tsmaster_cases}")
            return PreparedConfig(
                cfg_path=None,
                config_source=ConfigSource.TSMASTER_INLINE,
                is_tsmaster_inline=True,
                resolution_notes=[
                    f"TSMaster inline execution for cases: {', '.join(tsmaster_cases)}"
                ],
            )

        # Handle mixed TSMaster inline + CASE_MAPPING scenario
        # This is valid when there's exactly one script_path shared by non-inline cases
        if tsmaster_cases and script_paths and len(script_paths) == 1:
            resolved_path = list(script_paths.keys())[0]
            case_nos = list(script_paths.values())[0]
            logger.info(
                f"Resolved config for mixed TSMaster+CASE_MAPPING scenario: "
                f"{resolved_path} for cases: {case_nos}, "
                f"TSMaster inline cases (no config): {tsmaster_cases}"
            )
            return PreparedConfig(
                cfg_path=resolved_path,
                config_source=ConfigSource.CASE_MAPPING,
                resolution_notes=[
                    f"config_path resolved for mixed scenario (TSMaster inline + mapped): {resolved_path}"
                ],
            )

        # Detect conflict: multiple different script_paths
        if len(script_paths) > 1:
            conflict_details = []
            for sp, cases in script_paths.items():
                conflict_details.append(f"{sp} (used by: {', '.join(cases)})")
            raise ConfigConflictError(
                f"Multiple conflicting script_paths detected in case mappings: "
                f"{'; '.join(conflict_details)}. "
                f"All cases in a task must use the same configuration."
            )

        # Single script_path found
        if script_paths:
            resolved_path = list(script_paths.keys())[0]
            case_nos = list(script_paths.values())[0]
            logger.info(f"Resolved config_path from mapping: {resolved_path} for cases: {case_nos}")
            return PreparedConfig(
                cfg_path=resolved_path,
                config_source=ConfigSource.CASE_MAPPING,
                resolution_notes=[
                    f"config_path resolved from case mapping for {case_nos}: {resolved_path}"
                ],
            )

        # No config found anywhere
        if no_mapping_cases:
            raise MissingConfigError(
                f"No configuration available for cases: {', '.join(no_mapping_cases)}. "
                f"Either provide explicit configPath/configName, or ensure cases have mappings "
                f"with script_path (for CANOE) or category=TSMaster (for TSMaster inline)."
            )

        raise MissingConfigError(
            "No configuration path could be resolved. "
            "Provide explicit configPath, configName, or ensure cases have mappings."
        )

    def _apply_prepared_config(
        self, plan: ExecutionPlan, prepared: PreparedConfig
    ) -> ExecutionPlan:
        """Apply prepared configuration to a copy of the plan."""
        import copy

        # Deep copy the plan to avoid mutating the original,
        # then update only the fields that changed during preparation
        new_plan = copy.deepcopy(plan)
        new_plan.config_path = prepared.cfg_path
        new_plan.config_source = prepared.config_source
        new_plan.resolution_notes = prepared.resolution_notes

        # Store prepared config info in raw_refs for downstream consumers
        new_plan.raw_refs["prepared_config"] = {
            "cfg_path": prepared.cfg_path,
            "ini_path": prepared.ini_path,
            "config_source": prepared.config_source.value,
            "is_tsmaster_inline": prepared.is_tsmaster_inline,
            "raw_config_info": prepared.raw_config_info,
        }

        return new_plan

    def validate_plan(self, plan: ExecutionPlan) -> list[str]:
        """
        Validate that a prepared plan has all required information.

        Args:
            plan: The execution plan to validate

        Returns:
            List of validation error messages (empty if valid)

        Raises:
            ConfigPreparationError: If validation fails critically
        """
        errors: list[str] = []

        # Check tool type
        if not plan.tool_type:
            errors.append("tool_type is not specified")

        # Check cases
        if not plan.cases:
            errors.append("No cases in plan")

        # Check config availability based on source
        if plan.config_source == ConfigSource.UNSPECIFIED:
            # This should not happen after preparation
            errors.append(
                "config_source is UNSPECIFIED - plan was not properly prepared. "
                "Call prepare() before execution."
            )
        elif plan.config_source == ConfigSource.DIRECT_PATH:
            if not plan.config_path:
                errors.append("config_source is DIRECT_PATH but config_path is not set")
            elif not os.path.exists(plan.config_path):
                errors.append(f"config_path does not exist: {plan.config_path}")
        elif plan.config_source == ConfigSource.CONFIG_MANAGER:
            # Config manager will generate at runtime
            pass
        elif plan.config_source == ConfigSource.CASE_MAPPING:
            if not plan.config_path:
                errors.append("config_source is CASE_MAPPING but config_path is not set")
        elif plan.config_source == ConfigSource.TSMASTER_INLINE:
            # TSMaster inline - no config file needed
            pass

        return errors
