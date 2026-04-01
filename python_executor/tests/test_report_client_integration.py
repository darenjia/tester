from pathlib import Path
import sys

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from core.task_executor_production import TaskExecutorProduction
from models.task import Task, TaskResult
from utils.report_client import ReportClient


class FakeConfigManager:
    def __init__(self, values):
        self._values = values

    def get(self, key, default=None):
        return self._values.get(key, default)


def build_report_config():
    return FakeConfigManager(
        {
            "report_server.enabled": True,
            "report_server.host": "report.example.com",
            "report_server.port": 9000,
            "report_server.path": "/api/report",
            "report_server.upload_url": "http://upload.example.com/files",
            "report_server.timeout": 12,
            "report_server.retry_count": 1,
            "report.result_api_url": "http://report.example.com/direct-report",
            "report.file_upload_url": "http://upload.example.com/fallback",
            "report.retry_delay": 0,
            "report.headers": {"X-Test": "1"},
            "report_retry.max_retries": 7,
        }
    )


def test_report_client_loads_configured_endpoints():
    client = ReportClient(build_report_config())

    assert client._api_url == "http://report.example.com:9000/api/report"
    assert client._result_api_url == "http://report.example.com/direct-report"
    assert client._file_upload_url == "http://upload.example.com/files"


def test_report_client_upload_and_direct_report_use_configured_endpoints(tmp_path, monkeypatch):
    client = ReportClient(build_report_config())
    captured_calls = []

    def fake_make_request(method, url, **kwargs):
        captured_calls.append((method, url, kwargs))
        if "files" in kwargs:
            return {"code": 200, "data": {"url": "http://files.example.com/report.html"}}
        return {"code": 200}

    monkeypatch.setattr(client, "_make_request", fake_make_request)

    report_file = tmp_path / "report.html"
    report_file.write_text("ok", encoding="utf-8")

    upload_url = client.upload_report_file(str(report_file))
    report_ok = client.report_payload({"taskNo": "TASK-1", "status": "completed"})

    assert upload_url == "http://files.example.com/report.html"
    assert report_ok is True
    assert captured_calls[0][1] == "http://upload.example.com/files"
    assert captured_calls[1][1] == "http://report.example.com/direct-report"


def test_executor_remote_report_uses_report_client_and_persists_failures(monkeypatch):
    executor = TaskExecutorProduction(message_sender=lambda _: None)
    report_calls = []
    persisted = {}

    class FakeReportClient:
        def upload_report_file(self, file_path):
            report_calls.append(("upload", file_path))
            return "http://files.example.com/failure-report.html"

        def report_payload(self, report_data):
            report_calls.append(("report", report_data))
            return False

    class FakeExecutionResult:
        caseList = [{"caseNo": "CASE-1"}]

        def to_tdm2_format(self):
            return {"taskNo": "TASK-9", "caseList": [{"caseNo": "CASE-1"}]}

    task = Task(
        projectNo="PROJ-1",
        taskNo="TASK-9",
        taskName="Remote Report",
        deviceId="DEVICE-1",
        toolType="canoe",
    )
    task_result = TaskResult(taskNo="TASK-9", status="failed", summary={"failed": 1}, errorMessage="boom")

    monkeypatch.setattr(executor, "report_client", FakeReportClient())
    monkeypatch.setattr(executor, "_build_execution_result", lambda task, result: FakeExecutionResult())
    monkeypatch.setattr(
        executor,
        "_persist_failed_report",
        lambda report_data, task_obj: persisted.update({"report_data": report_data, "task": task_obj}),
    )

    executor._report_to_remote(task, task_result, report_file_path="C:/temp/report.html")

    assert report_calls[0] == ("upload", "C:/temp/report.html")
    assert report_calls[1][0] == "report"
    assert report_calls[1][1]["caseList"][0]["reAddress"] == "http://files.example.com/failure-report.html"
    assert persisted["report_data"]["taskNo"] == "TASK-9"
    assert persisted["task"] is task
