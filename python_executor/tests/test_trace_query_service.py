from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path
import sys

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))


@dataclass
class FakeTaskRecord:
    task_no: str
    project_no: str = "PROJECT-1"
    task_name: str = "Trace Task"
    status: str = "running"
    created_at: str = "2026-04-01T09:00:00"
    started_at: str | None = "2026-04-01T09:01:00"
    completed_at: str | None = None
    progress: int = 50
    message: str | None = "task is running"
    error_message: str | None = None
    raw_data: dict | None = None
    results: list[dict] | None = None
    summary: dict | None = None
    trace_id: str | None = None
    attempt_id: str | None = None

    def to_dict(self):
        return {
            "taskNo": self.task_no,
            "projectNo": self.project_no,
            "taskName": self.task_name,
            "status": self.status,
            "progress": self.progress,
            "createdAt": self.created_at,
            "startedAt": self.started_at,
            "completedAt": self.completed_at,
            "message": self.message,
            "errorMessage": self.error_message,
            "rawData": self.raw_data or {},
            "results": self.results or [],
            "summary": self.summary or {},
            "trace_id": self.trace_id,
            "attempt_id": self.attempt_id,
        }


class FakeTaskQueue:
    def __init__(self, tasks: dict[str, FakeTaskRecord]):
        self._tasks = tasks

    def get_task(self, task_no: str):
        return self._tasks.get(task_no)

    def get_all_tasks(self):
        return list(self._tasks.values())

    def get_stats(self):
        return {"total": len(self._tasks), "running": 1, "pending": 0}


class FakeTaskStore:
    def __init__(self, tasks: dict[str, dict]):
        self._tasks = tasks

    def get_task(self, task_no: str):
        return self._tasks.get(task_no)


class FakeFailedReportManager:
    def __init__(self, reports: list[dict]):
        self._reports = reports

    def list_report_projections(self, status=None, limit=100, offset=0):
        projections = list(self._reports)
        if status:
            projections = [report for report in projections if report.get("status") == status]
        return projections[offset : offset + limit]

    def get_report_projection(self, report_id: str):
        for report in self._reports:
            if report["report_id"] == report_id:
                return report
        return None


class FakeObservabilityManager:
    def __init__(self, current_tasks: list[dict], snapshots: dict[str, dict]):
        self._current_tasks = current_tasks
        self._snapshots = snapshots
        self.summary_calls = 0

    def get_business_summary(self):
        self.summary_calls += 1
        return {
            "queued_count": 0,
            "active_count": 1,
            "recent_failed_count": 0,
            "current_tasks": list(self._current_tasks),
        }

    def get_snapshot(self, task_no: str):
        return self._snapshots.get(task_no)


class FakeLoggerManager:
    def __init__(self, logs: list[dict]):
        self._logs = logs

    def get_memory_logs(self, level=None, limit=None, search=None):
        logs = list(self._logs)
        if level:
            logs = [log for log in logs if log["level"] == level.upper()]
        if search:
            search_lower = search.lower()
            logs = [log for log in logs if search_lower in log["message"].lower()]
        if limit:
            logs = logs[-limit:]
        return logs


def _build_service(**overrides):
    from core.trace_query_service import TraceQueryService

    defaults = dict(
        task_queue=FakeTaskQueue({}),
        task_store=FakeTaskStore({}),
        failed_report_manager=FakeFailedReportManager([]),
        observability_manager=FakeObservabilityManager([], {}),
        logger_manager=FakeLoggerManager([]),
    )
    defaults.update(overrides)
    return TraceQueryService(**defaults)


def test_query_task_no_anchors_on_task_record_and_collects_related_sources():
    task = FakeTaskRecord(
        task_no="TASK-1",
        trace_id="TRACE-1",
        attempt_id="ATTEMPT-1",
        summary={"passed": 3, "failed": 1},
    )
    logs = [
        {
            "timestamp": "2026-04-01 09:03:00",
            "level": "INFO",
            "message": "[TASK_STATUS] task entered executing",
            "task_no": "TASK-1",
            "trace_id": "TRACE-1",
            "attempt_id": "ATTEMPT-1",
        },
        {
            "timestamp": "2026-04-01 09:02:00",
            "level": "INFO",
            "message": "[RESULT_REPORT] report attempt failed",
            "task_no": "TASK-1",
            "trace_id": "TRACE-1",
            "attempt_id": "ATTEMPT-1",
        },
    ]
    report = {
        "report_id": "REPORT-1",
        "task_no": "TASK-1",
        "task_name": "Trace Task",
        "status": "pending",
        "trace_id": "TRACE-1",
        "attempt_id": "ATTEMPT-1",
        "latest_attempt": {
            "attempt_id": "ATTEMPT-1",
            "attempt_number": 1,
            "attempted_at": "2026-04-01 09:01:30",
            "trace_id": "TRACE-1",
            "status": "failed",
            "error_message": "gateway timeout",
        },
        "metadata": {
            "trace_id": "TRACE-1",
            "attempt_id": "ATTEMPT-1",
            "payload_hash": "payload-1",
        },
        "attempt_count": 1,
    }
    observability = FakeObservabilityManager(
        current_tasks=[
            {
                "task_no": "TASK-1",
                "trace_id": "TRACE-1",
                "attempt_id": "ATTEMPT-1",
                "stage": "executing",
                "error_code": None,
            }
        ],
        snapshots={
            "TASK-1": {
                "task_no": "TASK-1",
                "trace_id": "TRACE-1",
                "attempt_id": "ATTEMPT-1",
                "stage_history": ["received", "validated", "executing"],
                "current_stage": "executing",
            }
        },
    )

    service = _build_service(
        task_queue=FakeTaskQueue({"TASK-1": task}),
        task_store=FakeTaskStore({"TASK-1": task.to_dict()}),
        failed_report_manager=FakeFailedReportManager([report]),
        observability_manager=observability,
        logger_manager=FakeLoggerManager(logs),
    )

    result = service.query(task_no="TASK-1")

    assert result["meta"]["result_type"] == "full"
    assert result["meta"]["task_no"] == "TASK-1"
    assert result["meta"]["trace_id"] == "TRACE-1"
    assert result["meta"]["attempt_id"] == "ATTEMPT-1"
    assert result["meta"]["report_id"] == "REPORT-1"
    assert result["task"]["taskNo"] == "TASK-1"
    assert result["report"]["report_id"] == "REPORT-1"
    assert result["observability"]["task_no"] == "TASK-1"
    assert [event["timestamp"] for event in result["events"]] == [
        "2026-04-01 09:01:30",
        "2026-04-01 09:02:00",
        "2026-04-01 09:03:00",
    ]
    assert result["events"][0]["name"] == "report.attempt"
    assert result["events"][1]["name"] == "task.log"
    assert result["events"][2]["name"] == "task.status"


def test_query_report_id_returns_partial_result_when_only_report_data_exists():
    service = _build_service(
        failed_report_manager=FakeFailedReportManager(
            [
                {
                    "report_id": "REPORT-ONLY",
                    "task_no": "TASK-ONLY",
                    "status": "failed",
                    "trace_id": "TRACE-ONLY",
                    "attempt_id": "ATTEMPT-ONLY",
                    "latest_attempt": {
                        "attempt_id": "ATTEMPT-ONLY",
                        "attempt_number": 1,
                        "attempted_at": "2026-04-01 10:00:00",
                        "trace_id": "TRACE-ONLY",
                        "status": "failed",
                        "error_message": "upload failed",
                    },
                    "metadata": {"trace_id": "TRACE-ONLY", "attempt_id": "ATTEMPT-ONLY"},
                    "attempt_count": 1,
                }
            ]
        )
    )

    result = service.query(report_id="REPORT-ONLY")

    assert result["meta"]["result_type"] == "partial"
    assert result["meta"]["report_id"] == "REPORT-ONLY"
    assert result["meta"]["task_no"] == "TASK-ONLY"
    assert result["meta"]["trace_id"] == "TRACE-ONLY"
    assert result["report"]["report_id"] == "REPORT-ONLY"
    assert result["events"][0]["name"] == "report.attempt"


def test_query_attempt_id_matches_report_and_logs_even_without_task_record():
    logs = [
        {
            "timestamp": "2026-04-01 11:00:00",
            "level": "ERROR",
            "message": "attempt failed",
            "trace_id": "TRACE-A",
            "attempt_id": "ATTEMPT-A",
            "task_no": None,
        }
    ]
    service = _build_service(
        failed_report_manager=FakeFailedReportManager(
            [
                {
                    "report_id": "REPORT-A",
                    "task_no": "TASK-A",
                    "status": "failed",
                    "trace_id": "TRACE-A",
                    "attempt_id": "ATTEMPT-A",
                    "latest_attempt": {
                        "attempt_id": "ATTEMPT-A",
                        "attempt_number": 1,
                        "attempted_at": "2026-04-01 10:59:30",
                        "trace_id": "TRACE-A",
                        "status": "failed",
                        "error_message": "timeout",
                    },
                    "metadata": {"trace_id": "TRACE-A", "attempt_id": "ATTEMPT-A"},
                    "attempt_count": 1,
                }
            ]
        ),
        logger_manager=FakeLoggerManager(logs),
    )

    result = service.query(attempt_id="ATTEMPT-A")

    assert result["meta"]["result_type"] == "partial"
    assert result["meta"]["attempt_id"] == "ATTEMPT-A"
    assert result["meta"]["trace_id"] == "TRACE-A"
    assert result["report"]["report_id"] == "REPORT-A"
    assert [event["source"] for event in result["events"]] == ["failed_report", "log"]


def test_query_report_id_retries_task_lookup_using_inferred_trace_and_attempt_ids():
    task = FakeTaskRecord(
        task_no="TASK-RESOLVED",
        trace_id="TRACE-RESOLVED",
        attempt_id="ATTEMPT-RESOLVED",
        summary={"passed": 1, "failed": 0},
    )
    service = _build_service(
        task_queue=FakeTaskQueue({"TASK-RESOLVED": task}),
        failed_report_manager=FakeFailedReportManager(
            [
                {
                    "report_id": "REPORT-RESOLVED",
                    "status": "failed",
                    "trace_id": "TRACE-RESOLVED",
                    "attempt_id": "ATTEMPT-RESOLVED",
                    "latest_attempt": {
                        "attempt_id": "ATTEMPT-RESOLVED",
                        "attempt_number": 1,
                        "attempted_at": "2026-04-01 12:00:00",
                        "trace_id": "TRACE-RESOLVED",
                        "status": "failed",
                        "error_message": "timeout",
                    },
                    "metadata": {
                        "trace_id": "TRACE-RESOLVED",
                        "attempt_id": "ATTEMPT-RESOLVED",
                    },
                    "attempt_count": 1,
                }
            ]
        ),
    )

    result = service.query(report_id="REPORT-RESOLVED")

    assert result["meta"]["result_type"] == "full"
    assert result["meta"]["task_no"] == "TASK-RESOLVED"
    assert result["meta"]["trace_id"] == "TRACE-RESOLVED"
    assert result["meta"]["attempt_id"] == "ATTEMPT-RESOLVED"
    assert result["task"]["taskNo"] == "TASK-RESOLVED"


def test_query_report_id_does_not_bind_task_that_matches_only_one_inferred_identity():
    task = FakeTaskRecord(
        task_no="TASK-MISMATCH",
        trace_id="TRACE-SHARED",
        attempt_id="ATTEMPT-OTHER",
        summary={"passed": 1, "failed": 0},
    )
    service = _build_service(
        task_queue=FakeTaskQueue({"TASK-MISMATCH": task}),
        failed_report_manager=FakeFailedReportManager(
            [
                {
                    "report_id": "REPORT-SHARED",
                    "task_no": "TASK-REPORT",
                    "status": "failed",
                    "trace_id": "TRACE-SHARED",
                    "attempt_id": "ATTEMPT-SHARED",
                    "latest_attempt": {
                        "attempt_id": "ATTEMPT-SHARED",
                        "attempt_number": 1,
                        "attempted_at": "2026-04-01 12:30:00",
                        "trace_id": "TRACE-SHARED",
                        "status": "failed",
                        "error_message": "timeout",
                    },
                    "metadata": {
                        "trace_id": "TRACE-SHARED",
                        "attempt_id": "ATTEMPT-SHARED",
                    },
                    "attempt_count": 1,
                }
            ]
        ),
    )

    result = service.query(report_id="REPORT-SHARED")

    assert result["meta"]["result_type"] == "partial"
    assert result["task"] is None
    assert result["meta"]["task_no"] == "TASK-REPORT"
    assert result["meta"]["trace_id"] == "TRACE-SHARED"
    assert result["meta"]["attempt_id"] == "ATTEMPT-SHARED"


def test_query_task_no_does_not_match_queue_records_missing_trace_identity():
    task = FakeTaskRecord(task_no="TASK-EMPTY")
    service = _build_service(
        task_queue=FakeTaskQueue({"TASK-EMPTY": task}),
    )

    with pytest.raises(LookupError, match="TRACE_NOT_FOUND"):
        service.query(task_no="TASK-MISSING")


def test_query_raises_lookup_error_when_no_sources_match():
    service = _build_service()

    with pytest.raises(LookupError, match="TRACE_NOT_FOUND"):
        service.query(task_no="TASK-MISSING")


def test_query_sorts_timezone_aware_and_naive_events_without_crashing():
    service = _build_service(
        failed_report_manager=FakeFailedReportManager(
            [
                {
                    "report_id": "REPORT-TZ",
                    "task_no": "TASK-TZ",
                    "status": "failed",
                    "trace_id": "TRACE-TZ",
                    "attempt_id": "ATTEMPT-TZ",
                    "latest_attempt": {
                        "attempt_id": "ATTEMPT-TZ",
                        "attempt_number": 1,
                        "attempted_at": "2026-04-01T01:00:00+02:00",
                        "trace_id": "TRACE-TZ",
                        "status": "failed",
                        "error_message": "timeout",
                    },
                    "metadata": {
                        "trace_id": "TRACE-TZ",
                        "attempt_id": "ATTEMPT-TZ",
                    },
                    "attempt_count": 1,
                }
            ]
        ),
        logger_manager=FakeLoggerManager(
            [
                {
                    "timestamp": "2026-03-31 23:30:00",
                    "level": "INFO",
                    "message": "trace step",
                    "task_no": "TASK-TZ",
                    "trace_id": "TRACE-TZ",
                    "attempt_id": "ATTEMPT-TZ",
                }
            ]
        ),
    )

    result = service.query(report_id="REPORT-TZ")

    assert [event["name"] for event in result["events"]] == ["report.attempt", "task.log"]
    assert [event["timestamp"] for event in result["events"]] == [
        "2026-04-01T01:00:00+02:00",
        "2026-03-31 23:30:00",
    ]


def test_query_report_id_scans_beyond_the_first_projection_page():
    reports = []
    for index in range(1001):
        report_id = f"REPORT-{index}"
        reports.append(
            {
                "report_id": report_id,
                "task_no": f"TASK-{index}",
                "status": "failed",
                "trace_id": f"TRACE-{index}",
                "attempt_id": f"ATTEMPT-{index}",
                "latest_attempt": {
                    "attempt_id": f"ATTEMPT-{index}",
                    "attempt_number": 1,
                    "attempted_at": "2026-04-01 13:00:00",
                    "trace_id": f"TRACE-{index}",
                    "status": "failed",
                    "error_message": "timeout",
                },
                "metadata": {
                    "trace_id": f"TRACE-{index}",
                    "attempt_id": f"ATTEMPT-{index}",
                },
                "attempt_count": 1,
            }
        )

    service = _build_service(
        failed_report_manager=FakeFailedReportManager(reports),
    )

    result = service.query(report_id="REPORT-1000")

    assert result["report"]["report_id"] == "REPORT-1000"
    assert result["meta"]["report_id"] == "REPORT-1000"
