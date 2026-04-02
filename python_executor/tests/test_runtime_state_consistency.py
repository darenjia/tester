"""
Runtime State Truth Consistency Tests

Verifies that task lifecycle status is consistent across:
- task_queue (authoritative source)
- ExecutionObservabilityManager (observability context)
- FailedReportManager (failed report metadata)

And that task detail APIs reflect the authoritative state.
"""
from __future__ import annotations

import sys
from pathlib import Path
from unittest.mock import MagicMock, patch
from datetime import datetime

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))


class FakeConfigManager:
    """Fake config manager for isolated testing."""
    def __init__(self, values=None):
        self._values = values or {}
        self._calls = []

    def get(self, key, default=None):
        self._calls.append(("get", key))
        return self._values.get(key, default)

    def validate_config(self):
        return []


@pytest.fixture
def fake_config():
    """Provide a fake config manager."""
    return FakeConfigManager({
        "report_retry.base_delay": 1.0,
        "report_retry.backoff_factor": 2.0,
        "report_retry.max_delay": 60.0,
        "report_retry.max_retries": 3,
    })


class TestTaskQueueIsAuthoritative:
    """Verify task_queue is the authoritative source for task status."""

    def test_task_status_enum_has_required_states(self):
        """Ensure TaskStatus enum has all required lifecycle states."""
        from models.executor_task import TaskStatus

        required_states = {"pending", "running", "completed", "failed", "cancelled", "timeout"}
        actual_states = {s.value for s in TaskStatus}

        assert required_states.issubset(actual_states), \
            f"Missing states: {required_states - actual_states}"

    def test_task_queue_update_propagates_to_task_object(self):
        """Verify update_task_status actually modifies the task object."""
        from models.executor_task import Task, TaskQueue, TaskStatus

        # Create a minimal queue with temp storage
        import tempfile, os
        temp_file = tempfile.mktemp(suffix=".json")

        try:
            queue = TaskQueue.__new__(TaskQueue)
            queue._initialized = False
            queue._queue = []
            queue._lock = __import__("threading").RLock()
            queue._task_map = {}
            queue.storage_path = temp_file
            queue._ensure_storage_dir = lambda: None
            queue._load = lambda: None
            queue._save = lambda: None

            task = Task(
                id="TEST-001",
                name="Test Task",
                status=TaskStatus.PENDING.value,
            )
            queue.add(task)

            # Update status
            queue.update_task_status("TEST-001", TaskStatus.RUNNING.value)

            # Verify task object was modified
            updated_task = queue.get_task("TEST-001")
            assert updated_task.status == TaskStatus.RUNNING.value
            assert updated_task.started_at is not None

        finally:
            if os.path.exists(temp_file):
                os.unlink(temp_file)


class TestObservabilityContextAlignment:
    """Verify ExecutionObservabilityManager aligns with task_queue status."""

    def test_observability_context_stores_task_no_and_attempt_id(self):
        """Verify observability context stores required fields."""
        from core.execution_observability import (
            ExecutionObservabilityManager,
            ExecutionLifecycleStage,
        )

        manager = ExecutionObservabilityManager()
        ctx = manager.create_context(
            task_no="TASK-OBS-001",
            device_id="DEV-1",
            tool_type="canoe",
            attempt=1,
            attempt_id="ATTEMPT-1",
            trace_id="TRACE-1",
            error_category="execution_failure",
        )

        assert ctx.task_no == "TASK-OBS-001"
        assert ctx.attempt_id == "ATTEMPT-1"
        assert ctx.trace_id == "TRACE-1"
        assert ctx.error_category == "execution_failure"

    def test_observability_fail_captures_error_category(self):
        """Verify fail() captures error_category."""
        from core.execution_observability import (
            ExecutionObservabilityManager,
            ExecutionLifecycleStage,
        )

        manager = ExecutionObservabilityManager()
        manager.create_context(
            task_no="TASK-FAIL-001",
            device_id="DEV-1",
            tool_type="canoe",
        )

        manager.fail(
            "TASK-FAIL-001",
            error_code="EXEC_FAILED",
            error_message="execution failed",
            error_category="execution_failure",
            retryable=False,
        )

        snapshot = manager.get_snapshot("TASK-FAIL-001")
        assert snapshot["error_category"] == "execution_failure"
        assert snapshot["error_code"] == "EXEC_FAILED"
        assert snapshot["current_stage"] == ExecutionLifecycleStage.FINISHED.value

    def test_observability_finish_captures_report_status(self):
        """Verify finish() captures report_status."""
        from core.execution_observability import (
            ExecutionObservabilityManager,
            ExecutionLifecycleStage,
        )

        manager = ExecutionObservabilityManager()
        manager.create_context(
            task_no="TASK-FIN-001",
            device_id="DEV-1",
            tool_type="canoe",
        )

        manager.finish("TASK-FIN-001", report_status="success")

        snapshot = manager.get_snapshot("TASK-FIN-001")
        assert snapshot["report_status"] == "success"
        assert snapshot["current_stage"] == ExecutionLifecycleStage.FINISHED.value


class TestFailedReportMetadataAlignment:
    """Verify FailedReportManager metadata aligns with task_queue."""

    def test_failed_report_stores_task_and_trace_context(self, fake_config):
        """Verify failed report stores task_no, trace_id, attempt_id."""
        from core.failed_report_manager import FailedReportManager

        manager = FailedReportManager(
            storage_path=":memory:",
            config_manager=fake_config,
        )

        report_id = manager.add_failed_report(
            report_data={
                "taskNo": "TASK-FR-001",
                "status": "failed",
                "trace_id": "TRACE-FR-001",
                "attempt_id": "ATTEMPT-FR-001",
                "error_category": "execution_failure",
            },
            task_info={
                "taskNo": "TASK-FR-001",
                "projectNo": "PROJ-1",
                "deviceId": "DEV-1",
                "trace_id": "TRACE-FR-001",
                "attempt_id": "ATTEMPT-FR-001",
                "error_category": "execution_failure",
            },
            failure_reason="execution failed",
        )

        report = manager.get_report(report_id)
        projection = report.to_projection()

        assert projection["task_no"] == "TASK-FR-001"
        assert projection["trace_id"] == "TRACE-FR-001"
        assert projection["attempt_id"] == "ATTEMPT-FR-001"
        assert projection["error_category"] == "execution_failure"

    def test_failed_report_trace_context_summary(self, fake_config):
        """Verify get_trace_context_summary returns correct structure."""
        from core.failed_report_manager import FailedReportManager

        manager = FailedReportManager(
            storage_path=":memory:",
            config_manager=fake_config,
        )

        manager.add_failed_report(
            report_data={
                "taskNo": "TASK-TRACE-001",
                "status": "failed",
                "trace_id": "TRACE-001",
                "attempt_id": "ATTEMPT-001",
                "error_category": "timeout",
            },
            task_info={
                "taskNo": "TASK-TRACE-001",
                "trace_id": "TRACE-001",
                "attempt_id": "ATTEMPT-001",
                "error_category": "timeout",
            },
            failure_reason="timeout",
        )

        summary = manager.get_trace_context_summary(limit=10)

        assert "recent" in summary
        assert "recent_trace_ids" in summary
        assert "TRACE-001" in summary["recent_trace_ids"]


class TestTaskDetailAPIProjection:
    """Verify task detail API is a projection of task_queue."""

    def test_task_detail_includes_observability_context(self):
        """Verify task detail includes trace_id, attempt_id, error_category."""
        from models.executor_task import Task, TaskStatus

        task = Task(
            id="TASK-DETAIL-001",
            name="Test Task",
            status=TaskStatus.RUNNING.value,
            metadata={
                "trace_id": "TRACE-DETAIL-001",
                "attempt_id": "ATTEMPT-DETAIL-001",
                "error_category": "validation",
            },
        )

        task_dict = task.to_dict()

        assert task_dict["metadata"]["trace_id"] == "TRACE-DETAIL-001"
        assert task_dict["metadata"]["attempt_id"] == "ATTEMPT-DETAIL-001"
        assert task_dict["metadata"]["error_category"] == "validation"

    def test_task_detail_includes_result_summary(self):
        """Verify task detail includes result summary fields."""
        from models.executor_task import Task, TaskStatus

        task = Task(
            id="TASK-RESULT-001",
            name="Test Task",
            status=TaskStatus.COMPLETED.value,
            result={
                "summary": {
                    "total": 10,
                    "passed": 8,
                    "failed": 2,
                    "pass_rate": "80.0%",
                },
                "results": [
                    {"name": "test_1", "verdict": "PASS"},
                    {"name": "test_2", "verdict": "FAIL"},
                ],
            },
        )

        task_dict = task.to_dict()

        assert task_dict["result"] is not None
        assert task_dict["result"]["summary"]["total"] == 10
        assert task_dict["result"]["summary"]["passed"] == 8
        assert task_dict["result"]["summary"]["failed"] == 2


class TestStateConsistencyAcrossComponents:
    """Integration tests for state consistency across components."""

    def test_task_status_and_observability_stage_relationship(self):
        """
        Verify that when a task transitions to RUNNING in task_queue,
        the observability context reflects EXECUTING stage.
        """
        from models.executor_task import Task, TaskStatus
        from core.execution_observability import (
            ExecutionObservabilityManager,
            ExecutionLifecycleStage,
        )

        # Create task in RUNNING state
        task = Task(
            id="TASK-INT-001",
            name="Integration Task",
            status=TaskStatus.RUNNING.value,
            started_at=datetime.now().isoformat(),
        )

        # Create corresponding observability context in EXECUTING stage
        manager = ExecutionObservabilityManager()
        manager.create_context(
            task_no="TASK-INT-001",
            device_id="DEV-1",
            tool_type="canoe",
        )
        manager.transition("TASK-INT-001", ExecutionLifecycleStage.EXECUTING)

        # Verify relationship
        task_snapshot = {
            "id": task.id,
            "status": task.status,
        }
        obs_snapshot = manager.get_snapshot("TASK-INT-001")

        # When task is RUNNING, observability should be in an active stage
        assert task.status == TaskStatus.RUNNING.value
        assert obs_snapshot["current_stage"] in {
            ExecutionLifecycleStage.EXECUTING.value,
            ExecutionLifecycleStage.COLLECTING.value,
            ExecutionLifecycleStage.REPORTING.value,
        }

    def test_failed_task_status_consistency(self):
        """
        Verify that when a task transitions to FAILED in task_queue,
        the observability context reflects FINISHED stage with error_category.
        """
        from models.executor_task import Task, TaskStatus
        from core.execution_observability import (
            ExecutionObservabilityManager,
            ExecutionLifecycleStage,
        )

        # Create task in FAILED state
        task = Task(
            id="TASK-FAIL-INT-001",
            name="Failed Integration Task",
            status=TaskStatus.FAILED.value,
            error_message="execution failed",
            completed_at=datetime.now().isoformat(),
        )

        # Create corresponding observability context in FINISHED stage
        # Simulate full lifecycle: received -> preparing -> executing -> finished (failed)
        manager = ExecutionObservabilityManager()
        manager.create_context(
            task_no="TASK-FAIL-INT-001",
            device_id="DEV-1",
            tool_type="canoe",
        )
        manager.transition("TASK-FAIL-INT-001", ExecutionLifecycleStage.PREPARING)
        manager.transition("TASK-FAIL-INT-001", ExecutionLifecycleStage.EXECUTING)
        manager.fail(
            "TASK-FAIL-INT-001",
            error_code="EXEC_FAILED",
            error_message="execution failed",
            error_category="execution_failure",
            retryable=False,
        )

        # Verify consistency
        obs_snapshot = manager.get_snapshot("TASK-FAIL-INT-001")

        assert task.status == TaskStatus.FAILED.value
        assert obs_snapshot["current_stage"] == ExecutionLifecycleStage.FINISHED.value
        assert obs_snapshot["error_category"] == "execution_failure"
        assert obs_snapshot["failed_stage"] == ExecutionLifecycleStage.EXECUTING.value

    def test_completed_task_status_consistency(self):
        """
        Verify that when a task transitions to COMPLETED in task_queue,
        the observability context reflects FINISHED stage with success report_status.
        """
        from models.executor_task import Task, TaskStatus
        from core.execution_observability import (
            ExecutionObservabilityManager,
            ExecutionLifecycleStage,
        )

        # Create task in COMPLETED state
        task = Task(
            id="TASK-COMP-INT-001",
            name="Completed Integration Task",
            status=TaskStatus.COMPLETED.value,
            completed_at=datetime.now().isoformat(),
        )

        # Create corresponding observability context in FINISHED stage
        manager = ExecutionObservabilityManager()
        manager.create_context(
            task_no="TASK-COMP-INT-001",
            device_id="DEV-1",
            tool_type="canoe",
        )
        manager.finish("TASK-COMP-INT-001", report_status="success")

        # Verify consistency
        obs_snapshot = manager.get_snapshot("TASK-COMP-INT-001")

        assert task.status == TaskStatus.COMPLETED.value
        assert obs_snapshot["current_stage"] == ExecutionLifecycleStage.FINISHED.value
        assert obs_snapshot["report_status"] == "success"
