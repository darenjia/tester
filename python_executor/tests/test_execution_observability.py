from __future__ import annotations

import time

from core.execution_observability import (
    ExecutionLifecycleStage,
    ExecutionObservabilityManager,
)


def test_lifecycle_transitions_update_snapshot_and_stage_history():
    manager = ExecutionObservabilityManager()

    context = manager.create_context(
        task_no="TASK-1",
        device_id="DEVICE-1",
        tool_type="canoe",
    )
    manager.transition("TASK-1", ExecutionLifecycleStage.VALIDATED)
    manager.transition("TASK-1", ExecutionLifecycleStage.QUEUED)

    snapshot = manager.get_snapshot("TASK-1")

    assert context.current_stage == ExecutionLifecycleStage.QUEUED.value
    assert snapshot["task_no"] == "TASK-1"
    assert snapshot["stage_history"] == ["received", "validated", "queued"]


def test_lifecycle_supports_compiled_and_collecting_stages():
    manager = ExecutionObservabilityManager()

    manager.create_context(task_no="TASK-L", device_id="DEVICE-L", tool_type="canoe")
    manager.transition("TASK-L", ExecutionLifecycleStage.COMPILED)
    manager.transition("TASK-L", ExecutionLifecycleStage.QUEUED)
    manager.transition("TASK-L", ExecutionLifecycleStage.COLLECTING)

    snapshot = manager.get_snapshot("TASK-L")

    assert snapshot["stage_history"] == ["received", "compiled", "queued", "collecting"]


def test_fail_marks_stage_error_and_retryability():
    manager = ExecutionObservabilityManager()

    manager.create_context(task_no="TASK-2", device_id="DEVICE-2", tool_type="tsmaster")
    manager.transition("TASK-2", ExecutionLifecycleStage.EXECUTING)
    manager.fail(
        "TASK-2",
        error_code="EXEC_TIMEOUT",
        error_message="execution timed out",
        retryable=True,
    )

    snapshot = manager.get_snapshot("TASK-2")

    assert snapshot["current_stage"] == "finished"
    assert snapshot["failed_stage"] == "executing"
    assert snapshot["error_code"] == "EXEC_TIMEOUT"
    assert snapshot["retryable"] is True


def test_business_summary_counts_queue_active_and_failures():
    manager = ExecutionObservabilityManager()

    manager.create_context(task_no="TASK-A", device_id="D1", tool_type="canoe")
    manager.transition("TASK-A", ExecutionLifecycleStage.QUEUED)
    manager.create_context(task_no="TASK-B", device_id="D2", tool_type="tsmaster")
    manager.transition("TASK-B", ExecutionLifecycleStage.EXECUTING)
    manager.create_context(task_no="TASK-C", device_id="D3", tool_type="canoe")
    manager.fail("TASK-C", error_code="VALIDATION", error_message="bad input")

    summary = manager.get_business_summary()

    assert summary["queued_count"] == 1
    assert summary["active_count"] == 1
    assert summary["recent_failed_count"] == 1


def test_business_summary_prunes_old_finished_contexts_and_old_failures():
    manager = ExecutionObservabilityManager(
        max_contexts=2,
        recent_failure_window_seconds=30.0,
        retention_seconds=30.0,
    )

    manager.create_context(task_no="TASK-OLD", device_id="D1", tool_type="canoe")
    manager.fail("TASK-OLD", error_code="OLD_FAIL", error_message="old failure")
    manager._contexts["TASK-OLD"].completed_at = time.time() - 120.0

    manager.create_context(task_no="TASK-Q", device_id="D2", tool_type="canoe")
    manager.transition("TASK-Q", ExecutionLifecycleStage.QUEUED)
    manager.create_context(task_no="TASK-E", device_id="D3", tool_type="tsmaster")
    manager.transition("TASK-E", ExecutionLifecycleStage.EXECUTING)

    summary = manager.get_business_summary()

    assert "TASK-OLD" not in {task["task_no"] for task in summary["current_tasks"]}
    assert summary["recent_failed_count"] == 0
    assert summary["queued_count"] == 1
    assert summary["active_count"] == 1
