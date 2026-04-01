from __future__ import annotations

import threading
import time
from copy import deepcopy
from dataclasses import asdict, dataclass, field
from enum import Enum
from typing import Any


class ExecutionLifecycleStage(str, Enum):
    RECEIVED = "received"
    VALIDATED = "validated"
    COMPILED = "compiled"
    QUEUED = "queued"
    PREPARING = "preparing"
    EXECUTING = "executing"
    COLLECTING = "collecting"
    REPORTING = "reporting"
    FINISHED = "finished"


@dataclass(slots=True)
class ExecutionContext:
    task_no: str
    device_id: str
    tool_type: str
    current_stage: str = ExecutionLifecycleStage.RECEIVED.value
    attempt: int = 1
    error_code: str | None = None
    error_message: str | None = None
    failed_stage: str | None = None
    retryable: bool = False
    report_status: str | None = None
    started_at: float = field(default_factory=time.time)
    last_transition_at: float = field(default_factory=time.time)
    completed_at: float | None = None
    stage_history: list[str] = field(
        default_factory=lambda: [ExecutionLifecycleStage.RECEIVED.value]
    )
    stage_durations: dict[str, float] = field(default_factory=dict)


class ExecutionObservabilityManager:
    def __init__(
        self,
        *,
        max_contexts: int = 500,
        recent_failure_window_seconds: float = 900.0,
        retention_seconds: float = 3600.0,
    ):
        self._lock = threading.RLock()
        self._contexts: dict[str, ExecutionContext] = {}
        self._max_contexts = max_contexts
        self._recent_failure_window_seconds = recent_failure_window_seconds
        self._retention_seconds = retention_seconds

    def create_context(self, task_no: str, device_id: str, tool_type: str) -> ExecutionContext:
        context = ExecutionContext(task_no=task_no, device_id=device_id, tool_type=tool_type)
        with self._lock:
            self._contexts[task_no] = context
            self._prune_contexts_locked()
        return context

    def _get_context(self, task_no: str) -> ExecutionContext:
        with self._lock:
            return self._contexts[task_no]

    def transition(self, task_no: str, stage: ExecutionLifecycleStage) -> ExecutionContext:
        with self._lock:
            context = self._contexts[task_no]
            now = time.time()
            previous_stage = context.current_stage
            elapsed = max(0.0, now - context.last_transition_at)
            context.stage_durations[previous_stage] = (
                context.stage_durations.get(previous_stage, 0.0) + elapsed
            )
            context.current_stage = stage.value
            context.last_transition_at = now
            if stage == ExecutionLifecycleStage.FINISHED:
                context.completed_at = now
            context.stage_history.append(stage.value)
            self._prune_contexts_locked(now)
            return context

    def fail(
        self,
        task_no: str,
        *,
        error_code: str,
        error_message: str,
        retryable: bool = False,
    ) -> ExecutionContext:
        with self._lock:
            context = self._contexts[task_no]
            context.failed_stage = context.current_stage
            context.error_code = error_code
            context.error_message = error_message
            context.retryable = retryable
        return self.transition(task_no, ExecutionLifecycleStage.FINISHED)

    def finish(self, task_no: str, report_status: str | None = None) -> ExecutionContext:
        with self._lock:
            context = self._contexts[task_no]
            context.report_status = report_status
        return self.transition(task_no, ExecutionLifecycleStage.FINISHED)

    def get_snapshot(self, task_no: str) -> dict[str, Any]:
        with self._lock:
            context = self._contexts[task_no]
            return asdict(context)

    def get_business_summary(self) -> dict[str, Any]:
        with self._lock:
            now = time.time()
            self._prune_contexts_locked(now)
            contexts = list(self._contexts.values())

        queued_count = sum(
            1 for context in contexts if context.current_stage == ExecutionLifecycleStage.QUEUED.value
        )
        active_count = sum(
            1
            for context in contexts
            if context.current_stage
            in {
                ExecutionLifecycleStage.PREPARING.value,
                ExecutionLifecycleStage.EXECUTING.value,
                ExecutionLifecycleStage.REPORTING.value,
            }
        )
        recent_failed_count = sum(
            1
            for context in contexts
            if context.error_code is not None
            and (
                context.completed_at is None
                or now - context.completed_at <= self._recent_failure_window_seconds
            )
        )

        return {
            "queued_count": queued_count,
            "active_count": active_count,
            "recent_failed_count": recent_failed_count,
            "current_tasks": [asdict(context) for context in contexts],
        }

    def _prune_contexts_locked(self, now: float | None = None) -> None:
        if now is None:
            now = time.time()

        expired_task_nos = [
            task_no
            for task_no, context in self._contexts.items()
            if context.completed_at is not None
            and now - context.completed_at > self._retention_seconds
        ]
        for task_no in expired_task_nos:
            self._contexts.pop(task_no, None)

        if len(self._contexts) <= self._max_contexts:
            return

        finished_contexts = sorted(
            (
                (task_no, context)
                for task_no, context in self._contexts.items()
                if context.completed_at is not None
            ),
            key=lambda item: item[1].completed_at or item[1].last_transition_at,
        )
        while len(self._contexts) > self._max_contexts and finished_contexts:
            task_no, _ = finished_contexts.pop(0)
            self._contexts.pop(task_no, None)


_execution_observability_manager: ExecutionObservabilityManager | None = None


def get_execution_observability_manager() -> ExecutionObservabilityManager:
    global _execution_observability_manager
    if _execution_observability_manager is None:
        _execution_observability_manager = ExecutionObservabilityManager()
    return _execution_observability_manager
