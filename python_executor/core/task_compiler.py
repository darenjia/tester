from __future__ import annotations

from dataclasses import dataclass, field
from typing import Any

from core.config_preparation import ConfigPreparationResolver
from core.execution_plan import ConfigSource, ExecutionPlan, PlannedCase
from models.result import Message
from models.task import Task


class TaskCompileError(ValueError):
    """Raised when a platform task cannot be compiled into an execution plan."""


@dataclass(slots=True)
class TaskCompiler:
    mapping_manager: Any = None
    resolver: ConfigPreparationResolver | None = field(default=None)

    def __post_init__(self) -> None:
        if self.resolver is None:
            self.resolver = ConfigPreparationResolver(mapping_lookup=self.mapping_manager)

    def compile(self, task: Task) -> ExecutionPlan:
        return self.resolver.resolve(self.compile_execution_intent(task))

    def compile_task(self, task: Task) -> ExecutionPlan:
        return self.compile(task)

    def compile_message(self, message: Message) -> ExecutionPlan:
        payload = dict(message.payload or {})
        payload["taskNo"] = message.taskNo
        payload["deviceId"] = message.deviceId
        return self.compile_payload(payload)

    def compile_payload(self, payload: dict[str, Any]) -> ExecutionPlan:
        intent = self.compile_payload_intent(payload)
        return self.resolver.resolve(intent)

    def compile_execution_intent(self, task: Task) -> ExecutionPlan:
        payload = task.to_dict()
        if getattr(task, "deviceId", None):
            payload["deviceId"] = task.deviceId
        if getattr(task, "toolType", None):
            payload["toolType"] = task.toolType
        if getattr(task, "timeout", None) is not None:
            payload["timeout"] = task.timeout
        return self.compile_payload_intent(payload)

    def compile_payload_intent(self, payload: dict[str, Any]) -> ExecutionPlan:
        test_items = payload.get("testItems") or self._case_list_to_test_items(payload.get("caseList") or [])
        if not test_items:
            raise TaskCompileError("testItems不能为空")

        requested_tool_type = self._normalize_tool_type(payload.get("toolType"))
        resolved_tool_types: set[str] = set()
        planned_cases: list[PlannedCase] = []
        config_path = payload.get("configPath")
        config_source = ConfigSource.DIRECT_PATH if config_path else ConfigSource.UNSPECIFIED
        resolution_notes: list[str] = []

        for item in test_items:
            if not isinstance(item, dict):
                raise TaskCompileError("testItems中的每一项都必须是对象")

            case_no = item.get("case_no") or item.get("caseNo") or item.get("name", "")
            if not case_no:
                raise TaskCompileError("testItems中的每一项都必须包含case_no/caseNo/name")

            mapping = self.mapping_manager.get_mapping(case_no) if self.mapping_manager else None
            if mapping and getattr(mapping, "category", ""):
                resolved_tool_types.add(self._normalize_tool_type(mapping.category))

            planned_case = PlannedCase(
                case_no=case_no,
                case_name=item.get("name", "") or item.get("caseName", "") or (mapping.case_name if mapping else ""),
                case_type=item.get("type", "test_module") or item.get("caseType", "test_module"),
                repeat=item.get("repeat", 1),
                dtc_info=item.get("dtc_info") or item.get("dtcInfo"),
                execution_params=self._coerce_mapping(item.get("params"), field_name="params"),
                mapping_metadata={},
            )

            if mapping:
                if getattr(mapping, "category", None):
                    planned_case.mapping_metadata["category"] = self._normalize_tool_type(mapping.category)
                if getattr(mapping, "case_name", None) and not planned_case.case_name:
                    planned_case.case_name = mapping.case_name
                if getattr(mapping, "ini_config", None):
                    planned_case.execution_params["iniConfig"] = mapping.ini_config
                if getattr(mapping, "para_config", None):
                    planned_case.execution_params["paraConfig"] = mapping.para_config
            planned_cases.append(planned_case)

        if requested_tool_type:
            resolved_tool_types.add(requested_tool_type)

        if len(resolved_tool_types) != 1:
            raise TaskCompileError(
                f"任务包含多个工具类型，无法编译: {', '.join(sorted(filter(None, resolved_tool_types)))}"
            )

        tool_type = next(iter(resolved_tool_types))

        return ExecutionPlan(
            task_no=payload.get("taskNo", "") or "",
            project_no=payload.get("projectNo", "") or "",
            task_name=payload.get("taskName", "") or "",
            device_id=payload.get("deviceId", "") or "",
            tool_type=tool_type,
            cases=planned_cases,
            config_path=config_path,
            config_name=payload.get("configName"),
            base_config_dir=payload.get("baseConfigDir"),
            variables=self._coerce_mapping(payload.get("variables"), field_name="variables"),
            canoe_namespace=payload.get("canoeNamespace"),
            timeout_seconds=int(payload.get("timeout", 3600) or 3600),
            max_concurrency=1,
            report_required=True,
            config_source=config_source,
            resolution_notes=resolution_notes,
            raw_refs={"message_type": "TASK_DISPATCH"},
        )

    def _case_list_to_test_items(self, case_list: list[dict[str, Any]]) -> list[dict[str, Any]]:
        test_items: list[dict[str, Any]] = []
        for case in case_list:
            if not isinstance(case, dict):
                continue
            test_items.append(
                {
                    "caseNo": case.get("caseNo") or case.get("case_no") or case.get("name", ""),
                    "caseName": case.get("caseName") or case.get("case_name") or case.get("name", ""),
                    "name": case.get("caseName") or case.get("name", ""),
                    "type": case.get("caseType") or case.get("type", "test_module"),
                    "dtcInfo": case.get("dtcInfo") or case.get("dtc_info"),
                    "params": case.get("params"),
                    "repeat": case.get("repeat", 1),
                }
            )
        return test_items

    def _normalize_tool_type(self, value: Any) -> str:
        if value is None:
            return ""
        text = str(value).strip().lower()
        if text in {"can", "canoe"}:
            return "canoe"
        if text in {"tsmaster", "ts-master"}:
            return "tsmaster"
        if text in {"ttworkbench", "tt-workbench"}:
            return "ttworkbench"
        return text

    def _coerce_mapping(self, value: Any, *, field_name: str) -> dict[str, Any]:
        if value is None:
            return {}
        if isinstance(value, dict):
            return dict(value)
        raise TaskCompileError(f"{field_name}必须是对象")
