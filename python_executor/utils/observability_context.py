from __future__ import annotations

from dataclasses import dataclass
from typing import Any, Mapping


OBSERVABILITY_CONTEXT_FIELDS: tuple[str, ...] = (
    "task_no",
    "attempt_id",
    "trace_id",
    "tool_type",
    "stage",
    "error_code",
    "error_category",
)

LEGACY_OBSERVABILITY_CONTEXT_FIELDS: tuple[str, ...] = (
    "device_id",
    "attempt",
)

OBSERVABILITY_LOG_FIELDS: tuple[str, ...] = (
    OBSERVABILITY_CONTEXT_FIELDS + LEGACY_OBSERVABILITY_CONTEXT_FIELDS
)


@dataclass(slots=True)
class SharedObservabilityContext:
    task_no: str | None = None
    attempt_id: Any | None = None
    trace_id: str | None = None
    tool_type: str | None = None
    stage: str | None = None
    error_code: str | None = None
    error_category: str | None = None
    device_id: str | None = None
    attempt: int | None = None

    @classmethod
    def from_mapping(
        cls,
        context: Mapping[str, Any] | None = None,
        **overrides: Any,
    ) -> "SharedObservabilityContext":
        merged: dict[str, Any] = {}
        if context:
            merged.update(context)
        merged.update(overrides)

        if merged.get("stage") is None and merged.get("current_stage") is not None:
            merged["stage"] = merged["current_stage"]
        if merged.get("attempt_id") is None and merged.get("attempt") is not None:
            merged["attempt_id"] = merged["attempt"]

        return cls(
            task_no=merged.get("task_no"),
            attempt_id=merged.get("attempt_id"),
            trace_id=merged.get("trace_id"),
            tool_type=merged.get("tool_type"),
            stage=merged.get("stage"),
            error_code=merged.get("error_code"),
            error_category=merged.get("error_category"),
            device_id=merged.get("device_id"),
            attempt=merged.get("attempt"),
        )

    def to_log_extra(self) -> dict[str, Any]:
        extra: dict[str, Any] = {}
        for field_name in OBSERVABILITY_LOG_FIELDS:
            value = getattr(self, field_name)
            if value is not None:
                extra[field_name] = value
        return extra


def build_observability_log_extra(
    context: Mapping[str, Any] | None = None,
    **overrides: Any,
) -> dict[str, Any]:
    """Normalize shared observability fields while preserving caller extras."""
    return SharedObservabilityContext.from_mapping(context, **overrides).to_log_extra()
