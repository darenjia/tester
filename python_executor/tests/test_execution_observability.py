from __future__ import annotations

import time

from core.execution_observability import (
    ExecutionLifecycleStage,
    ExecutionObservabilityManager,
)
from utils.logger import logger_manager


def test_lifecycle_transitions_update_snapshot_and_stage_history():
    manager = ExecutionObservabilityManager()

    context = manager.create_context(
        task_no="TASK-1",
        device_id="DEVICE-1",
        tool_type="canoe",
        attempt=2,
        attempt_id="ATTEMPT-2",
        trace_id="TRACE-1",
    )
    manager.transition("TASK-1", ExecutionLifecycleStage.VALIDATED)
    manager.transition("TASK-1", ExecutionLifecycleStage.QUEUED)

    snapshot = manager.get_snapshot("TASK-1")

    assert context.current_stage == ExecutionLifecycleStage.QUEUED.value
    assert context.stage == ExecutionLifecycleStage.QUEUED.value
    assert snapshot["task_no"] == "TASK-1"
    assert snapshot["stage_history"] == ["received", "validated", "queued"]
    assert snapshot["attempt"] == 2
    assert snapshot["attempt_id"] == "ATTEMPT-2"
    assert snapshot["trace_id"] == "TRACE-1"


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
        error_category="timeout",
        retryable=True,
    )

    snapshot = manager.get_snapshot("TASK-2")

    assert snapshot["current_stage"] == "finished"
    assert snapshot["failed_stage"] == "executing"
    assert snapshot["error_code"] == "EXEC_TIMEOUT"
    assert snapshot["error_category"] == "timeout"
    assert snapshot["retryable"] is True


def test_to_observability_log_extra_keeps_shared_fields_and_legacy_attempt():
    manager = ExecutionObservabilityManager()

    context = manager.create_context(
        task_no="TASK-LOG",
        device_id="DEVICE-LOG",
        tool_type="canoe",
        attempt=4,
        trace_id="TRACE-LOG",
    )
    manager.transition("TASK-LOG", ExecutionLifecycleStage.PREPARING)
    context.error_code = "E-001"
    context.error_category = "validation"

    extra = context.to_observability_log_extra()

    assert extra["task_no"] == "TASK-LOG"
    assert extra["attempt_id"] == 4
    assert extra["attempt"] == 4
    assert extra["trace_id"] == "TRACE-LOG"
    assert extra["tool_type"] == "canoe"
    assert extra["stage"] == "preparing"
    assert extra["error_code"] == "E-001"
    assert extra["error_category"] == "validation"


def test_memory_logs_store_shared_observability_context_fields(tmp_path):
    logger_manager.setup(log_dir=str(tmp_path / "logs"))
    logger_manager.clear_memory_logs()

    logger = logger_manager.bind_task_context(
        logger_manager.get_logger("observability"),
        task_no="TASK-MEM",
        attempt_id="ATTEMPT-MEM",
        trace_id="TRACE-MEM",
        tool_type="tsmaster",
        stage="executing",
        error_code="EXEC_TIMEOUT",
        error_category="timeout",
    )

    logger.info("structured log line")
    logs = logger_manager.get_memory_logs(limit=1)

    assert logs[-1]["task_no"] == "TASK-MEM"
    assert logs[-1]["attempt_id"] == "ATTEMPT-MEM"
    assert logs[-1]["trace_id"] == "TRACE-MEM"
    assert logs[-1]["tool_type"] == "tsmaster"
    assert logs[-1]["stage"] == "executing"
    assert logs[-1]["error_code"] == "EXEC_TIMEOUT"
    assert logs[-1]["error_category"] == "timeout"
    assert logs[-1]["attempt"] is None


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
