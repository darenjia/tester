"""Normalized internal read model for case mappings."""

from dataclasses import dataclass, field
from typing import Any, Dict, List, Optional

from models.case_mapping import CaseMapping


def _normalize_tool_type(mapping: CaseMapping) -> str:
    """Infer a stable tool type from legacy flat mapping fields."""
    category = (mapping.category or "").strip().lower()
    if category in {"canoe", "tsmaster", "ttworkbench"}:
        return category

    if mapping.ttcn3_source or mapping.ttthree_path or mapping.clf_file:
        return "ttworkbench"

    if mapping.ini_config and not mapping.script_path:
        return "tsmaster"

    if mapping.script_path:
        return "canoe"

    return category or "unknown"


@dataclass
class MappingDeclaration:
    case_no: str = ""
    case_name: str = ""
    tool_type: str = ""
    category: str = ""
    module: str = ""
    enabled: bool = True
    priority: int = 0
    tags: List[str] = field(default_factory=list)
    description: str = ""
    version: str = "1.0"
    created_at: str = ""
    updated_at: str = ""

    def to_dict(self) -> Dict[str, Any]:
        return {
            "case_no": self.case_no,
            "case_name": self.case_name,
            "tool_type": self.tool_type,
            "category": self.category,
            "module": self.module,
            "enabled": self.enabled,
            "priority": self.priority,
            "tags": list(self.tags),
            "description": self.description,
            "version": self.version,
            "created_at": self.created_at,
            "updated_at": self.updated_at,
        }


@dataclass
class CANoeMaterial:
    config_path: Optional[str] = None
    module_name: Optional[str] = None
    ini_config: Optional[str] = None
    para_config: Optional[str] = None
    script_path: Optional[str] = None

    def to_dict(self) -> Dict[str, Any]:
        return {
            "config_path": self.config_path,
            "module_name": self.module_name,
            "ini_config": self.ini_config,
            "para_config": self.para_config,
            "script_path": self.script_path,
        }


@dataclass
class TSMasterMaterial:
    selection_key: Optional[str] = None
    ini_config: Optional[str] = None
    project_path: Optional[str] = None

    def to_dict(self) -> Dict[str, Any]:
        return {
            "selection_key": self.selection_key,
            "ini_config": self.ini_config,
            "project_path": self.project_path,
        }


@dataclass
class TTworkbenchMaterial:
    clf_file: Optional[str] = None
    clf_files: Optional[List[str]] = None
    ttcn3_source: Optional[str] = None
    ttthree_path: Optional[str] = None
    compile_params: Optional[str] = None
    log_format: Optional[str] = None
    test_timeout: Optional[int] = None

    def to_dict(self) -> Dict[str, Any]:
        return {
            "clf_file": self.clf_file,
            "clf_files": list(self.clf_files) if self.clf_files is not None else None,
            "ttcn3_source": self.ttcn3_source,
            "ttthree_path": self.ttthree_path,
            "compile_params": self.compile_params,
            "log_format": self.log_format,
            "test_timeout": self.test_timeout,
        }


@dataclass
class MappingMaterial:
    canoe: Optional[CANoeMaterial] = None
    tsmaster: Optional[TSMasterMaterial] = None
    ttworkbench: Optional[TTworkbenchMaterial] = None
    variables_template: Optional[Dict[str, Any]] = None
    artifacts_hint: Optional[Dict[str, Any]] = None

    def to_dict(self) -> Dict[str, Any]:
        return {
            "canoe": self.canoe.to_dict() if self.canoe else None,
            "tsmaster": self.tsmaster.to_dict() if self.tsmaster else None,
            "ttworkbench": self.ttworkbench.to_dict() if self.ttworkbench else None,
            "variables_template": self.variables_template,
            "artifacts_hint": self.artifacts_hint,
        }


@dataclass
class CaseMappingView:
    declaration: MappingDeclaration
    material: MappingMaterial

    @classmethod
    def from_mapping(cls, mapping: CaseMapping) -> "CaseMappingView":
        tool_type = _normalize_tool_type(mapping)
        declaration = MappingDeclaration(
            case_no=mapping.case_no,
            case_name=mapping.case_name,
            tool_type=tool_type,
            category=mapping.category,
            module=mapping.module,
            enabled=mapping.enabled,
            priority=mapping.priority,
            tags=list(mapping.tags),
            description=mapping.description,
            version=mapping.version,
            created_at=mapping.created_at,
            updated_at=mapping.updated_at,
        )

        canoe = None
        tsmaster = None
        ttworkbench = None

        if tool_type == "canoe":
            canoe = CANoeMaterial(
                config_path=mapping.script_path or None,
                module_name=mapping.module or None,
                ini_config=mapping.ini_config or None,
                para_config=mapping.para_config or None,
                script_path=mapping.script_path or None,
            )
        elif tool_type == "tsmaster":
            tsmaster = TSMasterMaterial(
                selection_key=mapping.case_no or None,
                ini_config=mapping.ini_config or None,
                project_path=mapping.script_path or None,
            )
        elif tool_type == "ttworkbench":
            ttworkbench = TTworkbenchMaterial(
                clf_file=mapping.clf_file or None,
                ttcn3_source=mapping.ttcn3_source or None,
                ttthree_path=mapping.ttthree_path or None,
                compile_params=mapping.compile_params or None,
                log_format=mapping.log_format or None,
                test_timeout=mapping.test_timeout or None,
            )

        material = MappingMaterial(
            canoe=canoe,
            tsmaster=tsmaster,
            ttworkbench=ttworkbench,
            variables_template=None,
            artifacts_hint={
                "script_path": mapping.script_path or None,
                "ini_config": mapping.ini_config or None,
                "para_config": mapping.para_config or None,
                "clf_file": mapping.clf_file or None,
            },
        )
        return cls(declaration=declaration, material=material)

    @property
    def case_no(self) -> str:
        return self.declaration.case_no

    @property
    def case_name(self) -> str:
        return self.declaration.case_name

    @property
    def tool_type(self) -> str:
        return self.declaration.tool_type

    @property
    def category(self) -> str:
        return self.declaration.category

    @property
    def module(self) -> str:
        return self.declaration.module

    @property
    def enabled(self) -> bool:
        return self.declaration.enabled

    @property
    def priority(self) -> int:
        return self.declaration.priority

    @property
    def tags(self) -> List[str]:
        return list(self.declaration.tags)

    @property
    def description(self) -> str:
        return self.declaration.description

    def to_dict(self) -> Dict[str, Any]:
        return {
            "declaration": self.declaration.to_dict(),
            "material": self.material.to_dict(),
        }
