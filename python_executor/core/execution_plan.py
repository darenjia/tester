from __future__ import annotations

from dataclasses import asdict, dataclass, field
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

    def __post_init__(self) -> None:
        if self.max_concurrency < 1:
            self.max_concurrency = 1
        if self.timeout_seconds <= 0:
            self.timeout_seconds = 3600

    def to_dict(self) -> dict[str, Any]:
        data = asdict(self)
        data["config_source"] = self.config_source.value
        return data

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
