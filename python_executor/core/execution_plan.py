from __future__ import annotations

from dataclasses import asdict, dataclass, field, replace
from enum import Enum
from typing import Any


class ConfigSource(str, Enum):
    UNSPECIFIED = "unspecified"
    DIRECT_PATH = "direct_path"
    CONFIG_MANAGER = "config_manager"
    CASE_MAPPING = "case_mapping"
    TSMASTER_INLINE = "tsmaster_inline"


@dataclass(slots=True)
class PlannedCase:
    case_no: str
    case_name: str
    case_type: str
    repeat: int = 1
    dtc_info: str | None = None
    execution_params: dict[str, Any] = field(default_factory=dict)
    mapping_metadata: dict[str, Any] = field(default_factory=dict)
    enabled: bool = True
    priority: int = 0
    metadata: dict[str, Any] = field(default_factory=dict)

    @property
    def name(self) -> str:
        return self.case_name

    @property
    def type(self) -> str:
        return self.case_type

    @property
    def caseNo(self) -> str:
        return self.case_no

    @property
    def caseName(self) -> str:
        return self.case_name

    @property
    def caseType(self) -> str:
        return self.case_type

    @property
    def dtcInfo(self) -> str | None:
        return self.dtc_info

    @property
    def params(self) -> dict[str, Any]:
        return self.execution_params

    def to_dict(self) -> dict[str, Any]:
        return {
            "caseNo": self.case_no,
            "caseName": self.case_name,
            "caseType": self.case_type,
            "repeat": self.repeat,
            "dtcInfo": self.dtc_info,
            "params": self.execution_params,
            "enabled": self.enabled,
            "priority": self.priority,
            "metadata": self.metadata,
        }


@dataclass(slots=True)
class ExecutionPlan:
    task_no: str
    project_no: str = ""
    task_name: str = ""
    device_id: str = ""
    tool_type: str = ""
    cases: list[PlannedCase] = field(default_factory=list)
    config_path: str | None = None
    config_name: str | None = None
    base_config_dir: str | None = None
    variables: dict[str, Any] = field(default_factory=dict)
    canoe_namespace: str | None = None
    timeout_seconds: int = 3600
    max_concurrency: int = 1
    retry_policy: dict[str, Any] = field(default_factory=dict)
    report_required: bool = True
    config_source: ConfigSource = ConfigSource.UNSPECIFIED
    resolution_notes: list[str] = field(default_factory=list)
    raw_refs: dict[str, Any] = field(default_factory=dict)
    prepared_cases: list[PlannedCase] = field(default_factory=list)
    preparation_mode: str | None = None
    resolved_config_path: str | None = None
    selection_material: dict[str, Any] | None = None
    resolved_variables: dict[str, Any] = field(default_factory=dict)
    prepare_summary: str = ""
    config_artifacts: dict[str, Any] = field(default_factory=dict)

    def __post_init__(self) -> None:
        if self.max_concurrency < 1:
            self.max_concurrency = 1
        if self.timeout_seconds <= 0:
            self.timeout_seconds = 3600
        if not self.prepared_cases:
            self.prepared_cases = list(self.cases)
        if not self.resolved_variables:
            self.resolved_variables = dict(self.variables)

    def with_preparation(self, **updates: Any) -> "ExecutionPlan":
        return replace(self, **updates)

    def to_dict(self) -> dict[str, Any]:
        data = asdict(self)
        data["config_source"] = self.config_source.value
        return data

    @classmethod
    def from_legacy_task(cls, task: Any) -> "ExecutionPlan":
        cases: list[PlannedCase] = []
        for item in getattr(task, "caseList", None) or getattr(task, "test_items", None) or []:
            planned_case = item if isinstance(item, PlannedCase) else PlannedCase(
                case_no=getattr(item, "caseNo", None) or getattr(item, "case_no", "") or "",
                case_name=getattr(item, "caseName", None) or getattr(item, "name", "") or "",
                case_type=getattr(item, "caseType", None) or getattr(item, "type", "") or "test_module",
                repeat=getattr(item, "repeat", 1) or 1,
                dtc_info=getattr(item, "dtcInfo", None) or getattr(item, "dtc_info", None),
                execution_params=getattr(item, "params", None) or getattr(item, "execution_params", None) or {},
                enabled=getattr(item, "enabled", True),
                priority=getattr(item, "priority", 0) or 0,
                metadata=getattr(item, "metadata", None) or {},
            )
            cases.append(planned_case)

        return cls(
            task_no=getattr(task, "taskNo", None) or getattr(task, "task_id", "") or "",
            project_no=getattr(task, "projectNo", "") or "",
            task_name=getattr(task, "taskName", None) or getattr(task, "name", "") or "",
            device_id=getattr(task, "deviceId", None) or getattr(task, "device_id", "") or "",
            tool_type=getattr(task, "toolType", None) or getattr(task, "tool_type", "") or "",
            cases=cases,
            config_path=getattr(task, "configPath", None) or getattr(task, "config_path", None),
            config_name=getattr(task, "configName", None) or getattr(task, "config_name", None),
            base_config_dir=getattr(task, "baseConfigDir", None) or getattr(task, "base_config_dir", None),
            variables=getattr(task, "variables", None) or {},
            canoe_namespace=getattr(task, "canoeNamespace", None) or getattr(task, "canoe_namespace", None),
            timeout_seconds=getattr(task, "timeout", None) or getattr(task, "timeout_seconds", 3600) or 3600,
            raw_refs={"source": type(task).__name__},
        )

    @property
    def task_id(self) -> str:
        return self.task_no

    @property
    def taskNo(self) -> str:
        return self.task_no

    @property
    def taskName(self) -> str:
        return self.task_name

    @property
    def projectNo(self) -> str:
        return self.project_no

    @property
    def deviceId(self) -> str:
        return self.device_id

    @property
    def toolType(self) -> str:
        return self.tool_type

    @property
    def timeout(self) -> int:
        return self.timeout_seconds

    @property
    def baseConfigDir(self) -> str | None:
        return self.base_config_dir

    @property
    def canoeNamespace(self) -> str | None:
        return self.canoe_namespace

    @property
    def test_items(self) -> list[PlannedCase]:
        return self.cases
