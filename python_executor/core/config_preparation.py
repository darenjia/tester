"""Config preparation resolver for compilation-time material resolution."""

from dataclasses import replace
from typing import Any, Callable, Dict, Iterable, Optional

from config.settings import settings
from core.case_mapping_manager import get_case_mapping_manager
from core.config_manager import TestConfigManager
from models.case_mapping import CaseMapping
from models.case_mapping_view import CaseMappingView

from .execution_plan import ExecutionPlan, PlannedCase


class ConfigPreparationError(RuntimeError):
    """Raised when a task cannot be prepared for execution."""


class ConfigPreparationResolver:
    """Resolve execution preparation modes before runtime."""

    def __init__(
        self,
        mapping_lookup: Optional[Callable[[str], Any]] = None,
        config_manager_factory: Optional[Callable[[Optional[str]], TestConfigManager]] = None,
        runtime_only_tools: Optional[Iterable[str]] = None,
    ) -> None:
        self._mapping_lookup = mapping_lookup
        self._config_manager_factory = config_manager_factory or (
            lambda base_dir: TestConfigManager(base_config_dir=base_dir)
        )
        self._runtime_only_tools = {
            tool.lower() for tool in (runtime_only_tools or {"tsmaster"})
        }

    def resolve(self, plan: ExecutionPlan) -> ExecutionPlan:
        """Return a prepared copy of the supplied execution plan."""
        tool_type = self._normalize_tool_type(plan.tool_type)
        prepared_cases = list(plan.prepared_cases or plan.cases)
        case_nos = [case.case_no for case in prepared_cases if case.case_no]

        explicit = self._resolve_explicit_path(plan, case_nos)
        if explicit is not None:
            return explicit

        prepared_config = self._resolve_named_config(plan)
        if prepared_config is not None:
            return prepared_config

        mapping_result = self._resolve_from_mapping(plan, prepared_cases, tool_type)
        if mapping_result is not None:
            return mapping_result

        if tool_type in self._runtime_only_tools:
            return self._finalize(
                plan,
                preparation_mode="tool_runtime_only",
                resolved_config_path=None,
                compatibility_config_path=plan.config_path,
                selection_material={
                    "source": "tool_runtime",
                    "tool_type": tool_type,
                    "case_nos": case_nos,
                },
                config_artifacts={},
                summary=f"mode=tool_runtime_only tool={tool_type} cases={len(case_nos)}",
            )

        raise ConfigPreparationError(
            f"无法为工具 {plan.tool_type} 准备执行材料: 缺少 configPath、configName/baseConfigDir 或可用映射材料"
        )

    def _resolve_explicit_path(
        self, plan: ExecutionPlan, case_nos: list[str]
    ) -> Optional[ExecutionPlan]:
        if not plan.config_path:
            return None

        return self._finalize(
            plan,
            preparation_mode="explicit_path",
            resolved_config_path=plan.config_path,
            compatibility_config_path=plan.config_path,
            selection_material={
                "source": "task.configPath",
                "config_path": plan.config_path,
                "case_nos": case_nos,
            },
            config_artifacts={"config_path": plan.config_path},
            summary=f"mode=explicit_path config_path={plan.config_path} cases={len(case_nos)}",
        )

    def _resolve_named_config(self, plan: ExecutionPlan) -> Optional[ExecutionPlan]:
        if not plan.config_name:
            return None

        base_dir = plan.base_config_dir or settings.get("config_base_dir")
        manager = self._config_manager_factory(base_dir)
        try:
            config_info = manager.prepare_config_for_task(
                task_config_name=plan.config_name,
                test_cases=[case.to_dict() for case in plan.prepared_cases],
                variables=plan.variables,
            )
        except Exception as exc:
            raise ConfigPreparationError(
                f"配置准备失败: {plan.config_name} ({exc})"
            ) from exc

        cfg_path = config_info.get("cfg_path")
        if not cfg_path:
            raise ConfigPreparationError(f"配置准备失败: 未找到配置 {plan.config_name}")

        return self._finalize(
            plan,
            preparation_mode="prepared_config",
            resolved_config_path=cfg_path,
            compatibility_config_path=cfg_path,
            selection_material={
                "source": "task.configName",
                "config_name": plan.config_name,
                "base_config_dir": base_dir,
                "cfg_path": cfg_path,
                "ini_path": config_info.get("ini_path"),
                "case_nos": [case.case_no for case in plan.prepared_cases if case.case_no],
            },
            config_artifacts=dict(config_info),
            summary=(
                f"mode=prepared_config config_name={plan.config_name} "
                f"base_dir={base_dir} cfg_path={cfg_path}"
            ),
        )

    def _resolve_from_mapping(
        self,
        plan: ExecutionPlan,
        prepared_cases: list[PlannedCase],
        tool_type: str,
    ) -> Optional[ExecutionPlan]:
        mapping_views = []
        for case in prepared_cases:
            if not case.case_no:
                continue
            view = self._lookup_mapping_view(case.case_no)
            if view is not None:
                mapping_views.append(view)

        if not mapping_views:
            return None

        candidates: list[tuple[tuple[Any, ...], ExecutionPlan]] = []
        for view in mapping_views:
            prepared = self._finalize_from_mapping(plan, view, tool_type)
            if prepared is not None:
                candidates.append((self._material_identity(view, tool_type), prepared))

        if not candidates:
            return None

        unique_candidates: dict[tuple[Any, ...], ExecutionPlan] = {}
        for identity, prepared in candidates:
            unique_candidates.setdefault(identity, prepared)

        if len(unique_candidates) > 1:
            raise ConfigPreparationError(
                f"映射材料冲突: 工具 {tool_type} 的多个用例解析到不同执行素材，无法自动准备"
            )

        return next(iter(unique_candidates.values()))

    def _finalize_from_mapping(
        self,
        plan: ExecutionPlan,
        view: CaseMappingView,
        tool_type: str,
    ) -> Optional[ExecutionPlan]:
        declaration = view.declaration
        material = view.material

        if tool_type == "canoe" and material.canoe and material.canoe.config_path:
            return self._finalize(
                plan,
                preparation_mode="mapping_material_only",
                resolved_config_path=material.canoe.config_path,
                compatibility_config_path=material.canoe.config_path,
                selection_material=material.canoe.to_dict(),
                config_artifacts={"source": "mapping", "case_no": declaration.case_no},
                summary=(
                    f"mode=mapping_material_only tool=canoe case={declaration.case_no} "
                    f"config_path={material.canoe.config_path}"
                ),
            )

        if tool_type == "tsmaster" and material.tsmaster:
            selection_payload = material.tsmaster.to_dict()
            if selection_payload.get("selection_key") is None:
                selection_payload["selection_key"] = declaration.case_no
            return self._finalize(
                plan,
                preparation_mode="mapping_material_only",
                resolved_config_path=None,
                compatibility_config_path=selection_payload.get("project_path"),
                selection_material=selection_payload,
                config_artifacts={"source": "mapping", "case_no": declaration.case_no},
                summary=(
                    f"mode=mapping_material_only tool=tsmaster case={declaration.case_no} "
                    f"selection_key={selection_payload.get('selection_key')}"
                ),
            )

        if tool_type == "ttworkbench" and material.ttworkbench:
            selection_payload = material.ttworkbench.to_dict()
            return self._finalize(
                plan,
                preparation_mode="mapping_material_only",
                resolved_config_path=selection_payload.get("clf_file"),
                compatibility_config_path=selection_payload.get("clf_file"),
                selection_material=selection_payload,
                config_artifacts={"source": "mapping", "case_no": declaration.case_no},
                summary=(
                    f"mode=mapping_material_only tool=ttworkbench case={declaration.case_no} "
                    f"clf_file={selection_payload.get('clf_file')}"
                ),
            )

        return None

    def _lookup_mapping_view(self, case_no: str) -> Optional[CaseMappingView]:
        lookup = self._mapping_lookup
        result: Any = None

        if callable(lookup):
            result = lookup(case_no)
        elif lookup is not None:
            if hasattr(lookup, "get_mapping_view"):
                result = lookup.get_mapping_view(case_no)
            elif hasattr(lookup, "get_mapping"):
                result = lookup.get_mapping(case_no)

        if result is None:
            manager = get_case_mapping_manager()
            result = manager.get_mapping_view(case_no)
            if result is None:
                mapping = manager.get_mapping(case_no)
                if mapping is not None:
                    result = CaseMappingView.from_mapping(mapping)

        return self._coerce_mapping_view(result, case_no=case_no)

    def _material_identity(
        self,
        view: CaseMappingView,
        tool_type: str,
    ) -> tuple[Any, ...]:
        material = view.material
        if tool_type == "canoe" and material.canoe:
            return ("canoe", material.canoe.config_path, material.canoe.ini_config, material.canoe.para_config)
        if tool_type == "tsmaster" and material.tsmaster:
            return ("tsmaster", material.tsmaster.project_path, material.tsmaster.ini_config, material.tsmaster.selection_key)
        if tool_type == "ttworkbench" and material.ttworkbench:
            return ("ttworkbench", material.ttworkbench.clf_file, material.ttworkbench.ttcn3_source, material.ttworkbench.ttthree_path)
        return (tool_type, None)

    def _finalize(
        self,
        plan: ExecutionPlan,
        preparation_mode: str,
        resolved_config_path: Optional[str],
        compatibility_config_path: Optional[str],
        selection_material: Optional[Dict[str, Any]],
        config_artifacts: Dict[str, Any],
        summary: str,
    ) -> ExecutionPlan:
        return replace(
            plan,
            config_path=compatibility_config_path or plan.config_path,
            preparation_mode=preparation_mode,
            resolved_config_path=resolved_config_path,
            selection_material=selection_material,
            resolved_variables=dict(plan.variables),
            prepare_summary=summary,
            config_artifacts=config_artifacts,
            prepared_cases=list(plan.prepared_cases or plan.cases),
        )

    def _coerce_mapping_view(
        self, result: Any, *, case_no: Optional[str] = None
    ) -> Optional[CaseMappingView]:
        if isinstance(result, CaseMappingView):
            return result
        if isinstance(result, CaseMapping):
            return CaseMappingView.from_mapping(result)
        if isinstance(result, dict):
            return CaseMappingView.from_mapping(CaseMapping.from_dict(result))
        if result is None:
            return None

        attrs = {
            "case_no": getattr(result, "case_no", None) or case_no or "",
            "case_name": getattr(result, "case_name", None) or "",
            "category": getattr(result, "category", None) or "",
            "module": getattr(result, "module", None) or "",
            "script_path": getattr(result, "script_path", None) or "",
            "ini_config": getattr(result, "ini_config", None) or "",
            "para_config": getattr(result, "para_config", None) or "",
            "enabled": getattr(result, "enabled", True),
            "priority": getattr(result, "priority", 0) or 0,
            "tags": list(getattr(result, "tags", None) or []),
            "description": getattr(result, "description", None) or "",
            "version": getattr(result, "version", None) or "1.0",
            "created_at": getattr(result, "created_at", None) or "",
            "updated_at": getattr(result, "updated_at", None) or "",
            "clf_file": getattr(result, "clf_file", None) or "",
            "ttcn3_source": getattr(result, "ttcn3_source", None) or "",
            "ttthree_path": getattr(result, "ttthree_path", None) or "",
            "compile_params": getattr(result, "compile_params", None) or "",
            "log_format": getattr(result, "log_format", None) or "",
            "test_timeout": getattr(result, "test_timeout", None),
        }
        if not attrs["case_no"] and not attrs["category"] and not attrs["script_path"]:
            return None
        return CaseMappingView.from_mapping(CaseMapping.from_dict(attrs))

    @staticmethod
    def _normalize_tool_type(tool_type: Optional[str]) -> str:
        return (tool_type or "").strip().lower()
