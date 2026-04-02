from __future__ import annotations

import hashlib
import json
from pathlib import Path
import sys

import pytest
from flask import Flask

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

import core.failed_report_manager as failed_report_manager_module
from api import report_retry_api
from core.failed_report_manager import FailedReportManager
from core.report_retry_scheduler import ReportRetryScheduler


class FakeConfigManager:
    def __init__(self, values):
        self._values = values

    def get(self, key, default=None):
        return self._values.get(key, default)


@pytest.fixture(autouse=True)
def reset_failed_report_singletons():
    failed_report_manager_module._failed_report_manager = None
    failed_report_manager_module.FailedReportManager._instance = None
    yield
    failed_report_manager_module._failed_report_manager = None
    failed_report_manager_module.FailedReportManager._instance = None


def _canonical_payload_hash(payload):
    payload_json = json.dumps(payload, ensure_ascii=False, sort_keys=True, separators=(",", ":"), default=str)
    return hashlib.sha256(payload_json.encode("utf-8")).hexdigest()


def test_failed_report_manager_persists_structured_metadata_and_initial_attempt(tmp_path):
    manager = FailedReportManager(
        storage_path=str(tmp_path / "failed_reports.json"),
        config_manager=FakeConfigManager(
            {
                "report_retry.base_delay": 1,
                "report_retry.backoff_factor": 2,
                "report_retry.max_delay": 60,
            }
        ),
    )

    report_data = {
        "taskNo": "TASK-100",
        "status": "failed",
        "projectNo": "PROJECT-100",
        "deviceId": "DEVICE-100",
        "taskName": "Meta Report",
        "results": [{"caseNo": "CASE-1", "verdict": "FAIL"}],
    }

    report_id = manager.add_failed_report(
        report_data=report_data,
        task_info={
            "taskNo": "TASK-100",
            "projectNo": "PROJECT-100",
            "deviceId": "DEVICE-100",
            "taskName": "Meta Report",
            "toolType": "canoe",
        },
        max_retries=5,
        priority=3,
        failure_reason="gateway timeout",
        endpoint="http://report.example.com/api/report",
    )

    report = manager.get_report(report_id)

    assert report is not None
    assert report.metadata["endpoint"] == "http://report.example.com/api/report"
    assert report.metadata["payload_hash"] == _canonical_payload_hash(report_data)
    assert report.metadata["failure_reason"] == "gateway timeout"
    assert report.attempts[0].attempt_number == 1
    assert report.attempts[0].endpoint == "http://report.example.com/api/report"
    assert report.attempts[0].payload_hash == _canonical_payload_hash(report_data)
    assert report.attempts[0].error_message == "gateway timeout"

    projection = report.to_projection()
    assert projection["task_no"] == "TASK-100"
    assert projection["metadata"]["toolType"] == "canoe"
    assert projection["latest_attempt"]["attempt_number"] == 1
    assert projection["trace_id"] is None
    assert projection["attempt_id"] == report.attempts[0].attempt_id


def test_failed_report_manager_appends_retry_attempts_with_shared_metadata(tmp_path):
    manager = FailedReportManager(
        storage_path=str(tmp_path / "failed_reports.json"),
        config_manager=FakeConfigManager(
            {
                "report_retry.base_delay": 1,
                "report_retry.backoff_factor": 2,
                "report_retry.max_delay": 60,
            }
        ),
    )

    report_id = manager.add_failed_report(
        report_data={"taskNo": "TASK-200", "status": "failed"},
        task_info={"taskNo": "TASK-200", "projectNo": "PROJECT-200"},
        endpoint="http://report.example.com/api/report",
        failure_reason="initial failure",
    )

    has_more = manager.increment_retry(report_id, success=False, error="retry failed")

    report = manager.get_report(report_id)

    assert has_more is True
    assert report.retry_count == 1
    assert len(report.attempts) == 2
    assert report.attempts[-1].attempt_number == 2
    assert report.attempts[-1].endpoint == "http://report.example.com/api/report"
    assert report.attempts[-1].error_message == "retry failed"
    assert report.attempts[-1].payload_hash == report.metadata["payload_hash"]
    assert report.to_projection()["attempt_count"] == 2


def test_failed_report_projection_exposes_observability_metadata(tmp_path):
    manager = FailedReportManager(
        storage_path=str(tmp_path / "failed_reports.json"),
        config_manager=FakeConfigManager(
            {
                "report_retry.base_delay": 1,
                "report_retry.backoff_factor": 2,
                "report_retry.max_delay": 60,
            }
        ),
    )

    report_id = manager.add_failed_report(
        report_data={
            "taskNo": "TASK-OBS",
            "status": "failed",
            "trace_id": "trace-obs-1",
            "attempt_id": "attempt-obs-1",
            "error_category": "execution_failure",
        },
        task_info={"taskNo": "TASK-OBS", "projectNo": "PROJECT-OBS", "deviceId": "DEVICE-OBS"},
        failure_reason="execution failed",
        metadata={
            "trace_id": "trace-obs-1",
            "attempt_id": "attempt-obs-1",
            "error_category": "execution_failure",
        },
    )

    report = manager.get_report(report_id)
    projection = report.to_projection()

    assert projection["trace_id"] == "trace-obs-1"
    assert projection["attempt_id"] == "attempt-obs-1"
    assert projection["error_category"] == "execution_failure"


def test_report_retry_api_uses_report_projections_for_list_and_detail(monkeypatch):
    app = Flask(__name__)
    app.register_blueprint(report_retry_api.report_retry_bp)

    class FakeReport:
        def to_projection(self):
            return {
                "report_id": "report-1",
                "task_no": "TASK-300",
                "status": "pending",
                "retry_count": 1,
                "max_retries": 5,
                "last_error": "gateway timeout",
                "metadata": {"payload_hash": "abc123"},
                "latest_attempt": {"attempt_number": 2, "endpoint": "http://report.example.com/api/report"},
            }

        def to_detail_dict(self):
            return {
                "report_id": "report-1",
                "task_no": "TASK-300",
                "attempts": [{"attempt_number": 1}, {"attempt_number": 2}],
                "metadata": {"payload_hash": "abc123"},
            }

        def to_dict(self):
            raise AssertionError("API should use projection helpers instead of to_dict")

    class FakeManager:
        def list_reports(self, status=None, limit=100, offset=0):
            return [FakeReport()]

        def get_statistics(self):
            return {"total": 1, "pending": 1, "failed": 0, "success": 0, "retrying": 0, "history_count": 1}

        def get_report(self, report_id):
            return FakeReport()

    monkeypatch.setattr(report_retry_api, "get_manager", lambda: FakeManager())
    monkeypatch.setattr(report_retry_api, "get_scheduler", lambda: None)

    client = app.test_client()

    list_response = client.get("/api/report-retry/list")
    assert list_response.status_code == 200
    list_payload = list_response.get_json()
    assert list_payload["data"]["reports"][0]["latest_attempt"]["endpoint"] == "http://report.example.com/api/report"
    assert list_payload["data"]["reports"][0]["metadata"]["payload_hash"] == "abc123"

    detail_response = client.get("/api/report-retry/report-1")
    assert detail_response.status_code == 200
    detail_payload = detail_response.get_json()
    assert detail_payload["data"]["attempts"][1]["attempt_number"] == 2
    assert detail_payload["data"]["metadata"]["payload_hash"] == "abc123"


def test_report_retry_api_detail_prefers_detail_view_over_projection(monkeypatch):
    app = Flask(__name__)
    app.register_blueprint(report_retry_api.report_retry_bp)

    class FakeManager:
        def get_report_projection(self, report_id):
            return {
                "report_id": report_id,
                "task_no": "TASK-DETAIL",
                "metadata": {"payload_hash": "projection-only"},
            }

        def get_report(self, report_id):
            class _DetailReport:
                def to_detail_dict(self):
                    return {
                        "report_id": report_id,
                        "task_no": "TASK-DETAIL",
                        "attempts": [{"attempt_number": 1}, {"attempt_number": 2}],
                        "metadata": {"payload_hash": "detail-view"},
                    }

            return _DetailReport()

    monkeypatch.setattr(report_retry_api, "get_manager", lambda: FakeManager())
    monkeypatch.setattr(report_retry_api, "get_scheduler", lambda: None)

    client = app.test_client()
    response = client.get("/api/report-retry/report-detail")

    assert response.status_code == 200
    payload = response.get_json()
    assert payload["data"]["attempts"][1]["attempt_number"] == 2
    assert payload["data"]["metadata"]["payload_hash"] == "detail-view"


def test_report_retry_scheduler_uses_report_endpoint_metadata(monkeypatch):
    class FakeReport:
        report_id = "report-1"
        task_no = "TASK-RETRY"
        report_data = {"taskNo": "TASK-RETRY"}
        metadata = {"endpoint": "http://persisted.example.com/report"}

    scheduler = ReportRetryScheduler()

    class FakeClient:
        _result_api_url = "http://runtime.example.com/report"

        def _make_request(self, method, url, **kwargs):
            assert method == "POST"
            assert url == "http://persisted.example.com/report"
            assert kwargs["json"]["taskNo"] == "TASK-RETRY"
            return {"code": 200}

    scheduler._report_client = FakeClient()

    success, error = scheduler._send_report(FakeReport())

    assert success is True
    assert error is None


def test_report_retry_scheduler_retry_all_pending_runs_without_started_scheduler(monkeypatch):
    processed = []
    scheduler = ReportRetryScheduler()

    class FakeReport:
        report_id = "report-batch"
        task_no = "TASK-BATCH"
        retry_count = 0

    class FakeManager:
        def get_pending_reports(self, limit=1000):
            return [FakeReport()]

        def reset_report_for_retry(self, report_id):
            processed.append(("reset", report_id))
            return True

        def get_report(self, report_id):
            return FakeReport()

        def update_report_status(self, report_id, status):
            processed.append(("status", report_id, status))

        def increment_retry(self, report_id, success, error):
            processed.append(("increment", report_id, success, error))
            return False

    monkeypatch.setattr(scheduler, "_report_manager", FakeManager())
    monkeypatch.setattr(scheduler, "_send_report", lambda report: (processed.append(("send", report.report_id)) or (True, None)))

    stats = scheduler.retry_all_pending()
    scheduler._retry_all_pending_async(total=stats["total"])

    assert stats["total"] == 1
    assert ("send", "report-batch") in processed
