from __future__ import annotations

from dataclasses import asdict, is_dataclass
from datetime import datetime, timezone
import inspect
from typing import Any, Iterable

from core.execution_observability import get_execution_observability_manager
from core.failed_report_manager import get_failed_report_manager
from core.task_store import task_store
from utils.logger import logger_manager


class TraceQueryService:
    """Read-only aggregator for trace, task, report, and observability data."""

    def __init__(
        self,
        *,
        task_queue=None,
        task_store=task_store,
        failed_report_manager=None,
        observability_manager=None,
        logger_manager=logger_manager,
    ):
        self.task_queue = task_queue
        self.task_store = task_store
        self.failed_report_manager = failed_report_manager or get_failed_report_manager()
        self.observability_manager = observability_manager or get_execution_observability_manager()
        self.logger_manager = logger_manager

    def query(
        self,
        *,
        trace_id: str | None = None,
        attempt_id: str | None = None,
        task_no: str | None = None,
        report_id: str | None = None,
    ) -> dict[str, Any]:
        lookup = {
            "trace_id": trace_id,
            "attempt_id": attempt_id,
            "task_no": task_no,
            "report_id": report_id,
        }
        provided = {key: value for key, value in lookup.items() if value not in (None, "")}
        if len(provided) != 1:
            raise ValueError("exactly one of trace_id, attempt_id, task_no, report_id is required")

        query_field, query_value = next(iter(provided.items()))

        context: dict[str, Any] = {
            "lookup": {"field": query_field, "value": query_value},
            "task": None,
            "report": None,
            "observability": None,
            "logs": [],
            "events": [],
            "meta": {
                "lookup": {"field": query_field, "value": query_value},
                "result_type": "partial",
                "task_no": None,
                "trace_id": None,
                "attempt_id": None,
                "report_id": None,
                "matched_sources": [],
            },
        }

        identities = {
            "task_no": task_no,
            "trace_id": trace_id,
            "attempt_id": attempt_id,
            "report_id": report_id,
        }

        task_record, task_source = self._find_task_record(
            task_no=task_no,
            trace_id=trace_id,
            attempt_id=attempt_id,
            report_id=report_id,
        )
        report_record, report_source = self._find_report_record(
            task_no=task_no,
            trace_id=trace_id,
            attempt_id=attempt_id,
            report_id=report_id,
        )
        observability_record, observability_source = self._find_observability_record(
            task_no=task_no,
            trace_id=trace_id,
            attempt_id=attempt_id,
            report=report_record,
            task=task_record,
        )

        inferred = self._infer_identities(
            identities,
            task=task_record,
            report=report_record,
            observability=observability_record,
        )

        if task_record is None and (
            inferred["task_no"] or inferred["trace_id"] or inferred["attempt_id"]
        ):
            task_record, task_source = self._find_task_record(
                task_no=inferred["task_no"],
                trace_id=inferred["trace_id"],
                attempt_id=inferred["attempt_id"],
            )
            inferred = self._infer_identities(
                inferred,
                task=task_record,
                report=report_record,
                observability=observability_record,
            )

        if report_record is None:
            report_record, report_source = self._find_report_record(
                task_no=inferred["task_no"],
                trace_id=inferred["trace_id"],
                attempt_id=inferred["attempt_id"],
                report_id=inferred["report_id"],
            )
            inferred = self._infer_identities(
                inferred,
                task=task_record,
                report=report_record,
                observability=observability_record,
            )

        if observability_record is None:
            observability_record, observability_source = self._find_observability_record(
                task_no=inferred["task_no"],
                trace_id=inferred["trace_id"],
                attempt_id=inferred["attempt_id"],
                report=report_record,
                task=task_record,
            )
            inferred = self._infer_identities(
                inferred,
                task=task_record,
                report=report_record,
                observability=observability_record,
            )

        logs = self._find_logs(
            task_no=inferred["task_no"],
            trace_id=inferred["trace_id"],
            attempt_id=inferred["attempt_id"],
        )

        if not any((task_record, report_record, observability_record, logs)):
            raise LookupError("TRACE_NOT_FOUND")

        events = self._build_events(report_record, logs)
        result_type = self._result_type(task_record, report_record, observability_record, logs)

        meta = {
            "lookup": {"field": query_field, "value": query_value},
            "result_type": result_type,
            "task_no": inferred["task_no"],
            "trace_id": inferred["trace_id"],
            "attempt_id": inferred["attempt_id"],
            "report_id": inferred["report_id"],
            "matched_sources": [name for name, value in (
                ("task", task_record),
                ("failed_report", report_record),
                ("observability", observability_record),
                ("logs", logs),
            ) if value],
        }

        context["task"] = task_record
        context["report"] = report_record
        context["observability"] = observability_record
        context["logs"] = logs
        context["events"] = events
        context["meta"] = meta

        return context

    def _find_task_record(self, *, task_no=None, trace_id=None, attempt_id=None, report_id=None):
        if task_no:
            record = self._get_task_by_task_no(task_no)
            if record is not None:
                normalized = self._normalize_task(record)
                if self._matches_identity(normalized, trace_id=trace_id, attempt_id=attempt_id):
                    return normalized, "task_no"
                if trace_id is None and attempt_id is None:
                    return normalized, "task_no"
        if self.task_queue is not None and (trace_id is not None or attempt_id is not None):
            for record in self._iter_tasks(self.task_queue):
                normalized = self._normalize_task(record)
                if self._matches_identity(normalized, trace_id=trace_id, attempt_id=attempt_id):
                    return normalized, "task_queue"
        if self.task_store is not None and task_no:
            record = self.task_store.get_task(task_no)
            if record is not None:
                normalized = self._normalize_task(record)
                if self._matches_identity(normalized, trace_id=trace_id, attempt_id=attempt_id):
                    return normalized, "task_store"
                if trace_id is None and attempt_id is None:
                    return normalized, "task_store"
        return None, None

    def _find_report_record(self, *, task_no=None, trace_id=None, attempt_id=None, report_id=None):
        if report_id:
            record = self._get_report_projection(report_id)
            if record is not None:
                return self._normalize_report(record), "report_id"

        for record in self._iter_report_projections():
            normalized = self._normalize_report(record)
            if task_no and normalized.get("task_no") == task_no and self._matches_identity(normalized, trace_id=trace_id, attempt_id=attempt_id):
                return normalized, "task_no"
            if not task_no and self._matches_identity(normalized, trace_id=trace_id, attempt_id=attempt_id):
                return normalized, "trace_id" if trace_id else "attempt_id"
        return None, None

    def _find_observability_record(self, *, task_no=None, trace_id=None, attempt_id=None, report=None, task=None):
        if task_no and hasattr(self.observability_manager, "get_snapshot"):
            snapshot = self.observability_manager.get_snapshot(task_no)
            if snapshot is not None:
                return self._normalize_observability_snapshot(snapshot), "snapshot"

        summary = self.observability_manager.get_business_summary()
        for item in summary.get("current_tasks", []):
            normalized = self._normalize_observability_snapshot(item)
            if task_no and normalized.get("task_no") == task_no and self._matches_identity(normalized, trace_id=trace_id, attempt_id=attempt_id):
                return normalized, "business_summary"
            if not task_no and self._matches_identity(normalized, trace_id=trace_id, attempt_id=attempt_id):
                return normalized, "business_summary"

        if report:
            if report.get("task_no"):
                for item in summary.get("current_tasks", []):
                    normalized = self._normalize_observability_snapshot(item)
                    if normalized.get("task_no") == report.get("task_no"):
                        return normalized, "business_summary"
        if task and task.get("task_no"):
            for item in summary.get("current_tasks", []):
                normalized = self._normalize_observability_snapshot(item)
                if normalized.get("task_no") == task.get("task_no"):
                    return normalized, "business_summary"
        return None, None

    def _find_logs(self, *, task_no=None, trace_id=None, attempt_id=None) -> list[dict[str, Any]]:
        logs = self.logger_manager.get_memory_logs()
        filtered = []
        for log in logs:
            normalized = self._normalize_log(log)
            if task_no and normalized.get("task_no") not in (None, task_no):
                continue
            if trace_id is not None or attempt_id is not None:
                if self._matches_identity(normalized, trace_id=trace_id, attempt_id=attempt_id):
                    filtered.append(normalized)
                continue
            if task_no and normalized.get("task_no") == task_no:
                filtered.append(normalized)
        return filtered

    def _infer_identities(self, identities: dict[str, Any], *, task=None, report=None, observability=None) -> dict[str, Any]:
        inferred = dict(identities)

        for source in (task, report, observability):
            if not source:
                continue
            inferred["task_no"] = inferred["task_no"] or source.get("task_no")
            inferred["trace_id"] = inferred["trace_id"] or source.get("trace_id")
            inferred["attempt_id"] = inferred["attempt_id"] or source.get("attempt_id")
            inferred["report_id"] = inferred["report_id"] or source.get("report_id")

        if task and not inferred["trace_id"]:
            inferred["trace_id"] = task.get("trace_id")
        if task and not inferred["attempt_id"]:
            inferred["attempt_id"] = task.get("attempt_id")

        if report and not inferred["task_no"]:
            inferred["task_no"] = report.get("task_no")
        if report and not inferred["trace_id"]:
            inferred["trace_id"] = report.get("trace_id")
        if report and not inferred["attempt_id"]:
            inferred["attempt_id"] = report.get("attempt_id")
        if report and not inferred["report_id"]:
            inferred["report_id"] = report.get("report_id")

        if observability and not inferred["task_no"]:
            inferred["task_no"] = observability.get("task_no")
        if observability and not inferred["trace_id"]:
            inferred["trace_id"] = observability.get("trace_id")
        if observability and not inferred["attempt_id"]:
            inferred["attempt_id"] = observability.get("attempt_id")

        return inferred

    def _result_type(self, task, report, observability, logs) -> str:
        if task and (report or observability or logs):
            return "full"
        return "partial"

    def _build_events(self, report: dict[str, Any] | None, logs: list[dict[str, Any]]) -> list[dict[str, Any]]:
        events: list[dict[str, Any]] = []

        if report:
            latest_attempt = report.get("latest_attempt") or {}
            attempt_timestamp = latest_attempt.get("attempted_at")
            if attempt_timestamp:
                events.append(
                    {
                        "source": "failed_report",
                        "name": "report.attempt",
                        "timestamp": attempt_timestamp,
                        "task_no": report.get("task_no"),
                        "trace_id": report.get("trace_id") or latest_attempt.get("trace_id"),
                        "attempt_id": report.get("attempt_id") or latest_attempt.get("attempt_id"),
                        "message": latest_attempt.get("error_message") or report.get("last_error"),
                    }
                )

        for log in logs:
            events.append(
                {
                    "source": "log",
                    "name": self._normalize_log_event_name(log),
                    "timestamp": log.get("timestamp"),
                    "task_no": log.get("task_no"),
                    "trace_id": log.get("trace_id"),
                    "attempt_id": log.get("attempt_id"),
                    "level": log.get("level"),
                    "message": log.get("message"),
                }
            )

        events.sort(key=lambda item: self._parse_timestamp(item.get("timestamp")) or datetime.max)
        return events

    def _normalize_task(self, record: Any) -> dict[str, Any]:
        if record is None:
            return {}
        if isinstance(record, dict):
            data = dict(record)
        elif hasattr(record, "to_dict"):
            data = record.to_dict()
        elif is_dataclass(record):
            data = asdict(record)
        else:
            data = {
                key: getattr(record, key)
                for key in dir(record)
                if not key.startswith("_") and not callable(getattr(record, key))
            }
        task_no = data.get("task_no") or data.get("taskNo") or data.get("id")
        return {
            "task_no": task_no,
            "taskNo": data.get("taskNo") or task_no,
            "task_name": data.get("task_name") or data.get("taskName"),
            "taskName": data.get("taskName") or data.get("task_name"),
            "status": data.get("status"),
            "progress": data.get("progress"),
            "created_at": data.get("created_at") or data.get("createdAt"),
            "started_at": data.get("started_at") or data.get("startedAt"),
            "completed_at": data.get("completed_at") or data.get("completedAt"),
            "message": data.get("message"),
            "error_message": data.get("error_message") or data.get("errorMessage"),
            "raw_data": data.get("raw_data") or data.get("rawData"),
            "results": data.get("results", []),
            "summary": data.get("summary"),
            "trace_id": data.get("trace_id") or data.get("traceId"),
            "attempt_id": data.get("attempt_id") or data.get("attemptId"),
        }

    def _normalize_report(self, record: Any) -> dict[str, Any]:
        if record is None:
            return {}
        if isinstance(record, dict):
            data = dict(record)
        elif hasattr(record, "to_projection"):
            data = record.to_projection()
        elif hasattr(record, "to_detail_dict"):
            data = record.to_detail_dict()
        else:
            data = {}

        latest_attempt = data.get("latest_attempt") or {}
        metadata = data.get("metadata") or {}
        task_no = data.get("task_no") or metadata.get("task_no") or metadata.get("taskNo")
        trace_id = data.get("trace_id") or metadata.get("trace_id") or metadata.get("traceId") or latest_attempt.get("trace_id")
        attempt_id = data.get("attempt_id") or metadata.get("attempt_id") or metadata.get("attemptId") or latest_attempt.get("attempt_id")
        return {
            "report_id": data.get("report_id"),
            "task_no": task_no,
            "task_name": data.get("task_name") or metadata.get("task_name") or metadata.get("taskName"),
            "status": data.get("status"),
            "trace_id": trace_id,
            "attempt_id": attempt_id,
            "latest_attempt": latest_attempt,
            "metadata": metadata,
            "attempt_count": data.get("attempt_count"),
        }

    def _normalize_observability_snapshot(self, snapshot: Any) -> dict[str, Any]:
        if snapshot is None:
            return {}
        if isinstance(snapshot, dict):
            data = dict(snapshot)
        elif is_dataclass(snapshot):
            data = asdict(snapshot)
        else:
            data = {}

        return {
            "task_no": data.get("task_no"),
            "trace_id": data.get("trace_id"),
            "attempt_id": data.get("attempt_id"),
            "stage": data.get("stage") or data.get("current_stage"),
            "current_stage": data.get("current_stage") or data.get("stage"),
            "stage_history": list(data.get("stage_history", []) or []),
            "error_code": data.get("error_code"),
            "error_category": data.get("error_category"),
            "report_status": data.get("report_status"),
        }

    def _normalize_log(self, log: Any) -> dict[str, Any]:
        if isinstance(log, dict):
            data = dict(log)
        elif is_dataclass(log):
            data = asdict(log)
        else:
            data = {}

        timestamp = data.get("timestamp") or data.get("created_at") or data.get("time")
        return {
            "timestamp": timestamp,
            "level": data.get("level"),
            "message": data.get("message"),
            "task_no": data.get("task_no"),
            "trace_id": data.get("trace_id"),
            "attempt_id": data.get("attempt_id"),
            "stage": data.get("stage"),
            "error_code": data.get("error_code"),
            "error_category": data.get("error_category"),
        }

    def _normalize_log_event_name(self, log: dict[str, Any]) -> str:
        message = (log.get("message") or "").upper()
        if "[TASK_STATUS]" in message:
            return "task.status"
        if "[RESULT_REPORT]" in message:
            return "task.log"
        return "task.log"

    def _parse_timestamp(self, value: Any) -> datetime | None:
        if not value:
            return None
        if isinstance(value, datetime):
            return self._normalize_datetime(value)
        if not isinstance(value, str):
            return None
        try:
            return self._normalize_datetime(datetime.fromisoformat(value))
        except ValueError:
            pass
        for fmt in ("%Y-%m-%d %H:%M:%S", "%Y-%m-%d %H:%M:%S.%f"):
            try:
                return self._normalize_datetime(datetime.strptime(value, fmt))
            except ValueError:
                continue
        return None

    def _normalize_datetime(self, value: datetime) -> datetime:
        if value.tzinfo is None:
            return value
        return value.astimezone(timezone.utc).replace(tzinfo=None)

    def _matches_identity(
        self,
        record: dict[str, Any],
        *,
        trace_id: str | None = None,
        attempt_id: str | None = None,
    ) -> bool:
        record_trace_id = record.get("trace_id")
        record_attempt_id = record.get("attempt_id")

        if trace_id and attempt_id:
            return record_trace_id == trace_id and record_attempt_id == attempt_id
        if trace_id:
            return record_trace_id == trace_id
        if attempt_id:
            return record_attempt_id == attempt_id
        return False

    def _iter_tasks(self, queue: Any) -> Iterable[Any]:
        if queue is None:
            return []
        if hasattr(queue, "get_all_tasks"):
            return queue.get_all_tasks()
        if hasattr(queue, "_task_map"):
            return list(queue._task_map.values())
        return []

    def _get_task_by_task_no(self, task_no: str):
        if self.task_queue is not None and hasattr(self.task_queue, "get_task"):
            record = self.task_queue.get_task(task_no)
            if record is not None:
                return record
        if self.task_store is not None and hasattr(self.task_store, "get_task"):
            record = self.task_store.get_task(task_no)
            if record is not None:
                return record
        return None

    def _iter_report_projections(self) -> Iterable[Any]:
        manager = self.failed_report_manager
        if manager is None:
            return []
        if hasattr(manager, "list_report_projections"):
            return self._iter_paginated_report_projections(manager.list_report_projections)
        if hasattr(manager, "list_reports"):
            return self._iter_paginated_report_projections(
                lambda limit=None, offset=0: [report.to_projection() for report in manager.list_reports(limit=limit or 100, offset=offset)]
            )
        return []

    def _iter_paginated_report_projections(self, fetch_page):
        page_size = 200
        offset = 0

        while True:
            page = self._call_report_page(fetch_page, page_size=page_size, offset=offset)
            if not page:
                break
            for record in page:
                yield record
            if len(page) < page_size:
                break
            offset += page_size

    def _call_report_page(self, fetch_page, *, page_size: int, offset: int):
        try:
            signature = inspect.signature(fetch_page)
        except (TypeError, ValueError):
            signature = None

        if signature is not None and "limit" in signature.parameters:
            return fetch_page(limit=page_size, offset=offset)
        return fetch_page()

    def _get_report_projection(self, report_id: str):
        manager = self.failed_report_manager
        if manager is None:
            return None
        if hasattr(manager, "get_report_projection"):
            record = manager.get_report_projection(report_id)
            if record is not None:
                return record
        if hasattr(manager, "get_report"):
            report = manager.get_report(report_id)
            if report is not None:
                if hasattr(report, "to_projection"):
                    return report.to_projection()
                if hasattr(report, "to_detail_dict"):
                    return report.to_detail_dict()
        for record in self._iter_report_projections():
            normalized = self._normalize_report(record)
            if normalized.get("report_id") == report_id:
                return record
        return None
