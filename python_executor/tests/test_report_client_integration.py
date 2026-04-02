from pathlib import Path
import sys

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from core.task_executor_production import TaskExecutorProduction
from models.result import ExecutionOutcome
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


def test_report_client_enabled_uses_result_api_url_when_direct_api_is_configured():
    client = ReportClient(
        FakeConfigManager(
            {
                "report_server.enabled": True,
                "report_server.host": "",
                "report_server.port": "",
                "report_server.path": "",
                "report.result_api_url": "http://report.example.com/direct-report",
                "report.file_upload_url": "http://upload.example.com/fallback",
            }
        )
    )

    assert client.enabled is True


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


def test_executor_remote_report_accepts_execution_outcome(monkeypatch):
    executor = TaskExecutorProduction(message_sender=lambda _: None)
    captured = {}

    class FakeReportClient:
        def upload_report_file(self, file_path):
            return None

        def report_payload(self, report_data):
            captured["report_data"] = report_data
            return True

    class FakeExecutionResult:
        caseList = [{"caseNo": "CASE-EXEC"}]

        def to_tdm2_format(self):
            return {"taskNo": "TASK-EXEC-OUTCOME", "caseList": [{"caseNo": "CASE-EXEC"}]}

    task = Task(
        projectNo="PROJ-E",
        taskNo="TASK-EXEC-OUTCOME",
        taskName="Outcome Report",
        deviceId="DEVICE-E",
        toolType="canoe",
    )
    outcome = ExecutionOutcome(
        task_no="TASK-EXEC-OUTCOME",
        status="completed",
        summary={"total": 1, "passed": 1, "failed": 0},
    )

    monkeypatch.setattr(executor, "report_client", FakeReportClient())
    monkeypatch.setattr(executor, "_build_execution_result", lambda task, result: FakeExecutionResult())

    executor._report_to_remote(task, outcome)

    assert captured["report_data"]["taskNo"] == "TASK-EXEC-OUTCOME"
    assert captured["report_data"]["status"] == "completed"
    assert captured["report_data"]["summary"]["passed"] == 1


def test_executor_remote_report_attaches_traceable_metadata(monkeypatch):
    executor = TaskExecutorProduction(message_sender=lambda _: None)
    captured = {}

    class FakeReportClient:
        def upload_report_file(self, file_path):
            return None

        def report_payload(self, report_data):
            captured["report_data"] = report_data
            return True

    class FakeExecutionResult:
        caseList = [{"caseNo": "CASE-TRACE"}]

        def to_tdm2_format(self):
            return {"taskNo": "TASK-TRACE", "caseList": [{"caseNo": "CASE-TRACE"}]}

    task = Task(
        projectNo="PROJ-TRACE",
        taskNo="TASK-TRACE",
        taskName="Trace Report",
        deviceId="DEVICE-TRACE",
        toolType="canoe",
    )
    outcome = ExecutionOutcome(
        task_no="TASK-TRACE",
        status="failed",
        summary={"failed": 1},
        error_summary="boom",
    )

    monkeypatch.setattr(executor, "report_client", FakeReportClient())
    monkeypatch.setattr(executor, "_build_execution_result", lambda task, result: FakeExecutionResult())

    executor._report_to_remote(task, outcome)

    assert captured["report_data"]["trace_id"]
    assert captured["report_data"]["attempt_id"]
    assert captured["report_data"]["error_category"] == "execution_failure"
    assert captured["report_data"]["reportMetadata"]["trace_id"] == captured["report_data"]["trace_id"]
    assert captured["report_data"]["reportMetadata"]["attempt_id"] == captured["report_data"]["attempt_id"]


def test_persist_failed_report_uses_task_identity_for_execution_plan(monkeypatch):
    from core.execution_plan import ExecutionPlan, PlannedCase

    executor = TaskExecutorProduction(message_sender=lambda _: None)
    persisted = {}

    class _FakeFailedReportManager:
        def add_failed_report(self, report_data, task_info, max_retries, priority):
            persisted["report_data"] = report_data
            persisted["task_info"] = task_info
            persisted["max_retries"] = max_retries
            persisted["priority"] = priority
            return "report-1"

    monkeypatch.setattr(
        "core.failed_report_manager.get_failed_report_manager",
        lambda config_manager: _FakeFailedReportManager(),
    )

    plan = ExecutionPlan(
        task_no="TASK-PERSIST",
        project_no="PROJECT-PERSIST",
        task_name="Persist Task",
        device_id="DEVICE-PERSIST",
        tool_type="canoe",
        cases=[PlannedCase(case_no="CASE-PERSIST", case_name="Case Persist", case_type="test_module")],
    )

    executor._persist_failed_report({"taskNo": "TASK-PERSIST", "status": "failed"}, plan)

    assert persisted["task_info"]["taskNo"] == "TASK-PERSIST"
    assert persisted["task_info"]["projectNo"] == "PROJECT-PERSIST"


def test_report_client_persists_failure_with_structured_metadata_and_endpoint(monkeypatch):
    client = ReportClient(build_report_config())
    captured = {}

    class FakeFailedReportManager:
        def add_failed_report(
            self,
            report_data,
            task_info=None,
            max_retries=None,
            priority=0,
            failure_reason=None,
            endpoint=None,
            metadata=None,
        ):
            captured["report_data"] = report_data
            captured["task_info"] = task_info
            captured["max_retries"] = max_retries
            captured["priority"] = priority
            captured["failure_reason"] = failure_reason
            captured["endpoint"] = endpoint
            captured["metadata"] = metadata
            return "report-structured"

    monkeypatch.setattr(
        "core.failed_report_manager.get_failed_report_manager",
        lambda config_manager: FakeFailedReportManager(),
    )

    client._handle_report_failure(
        {"taskNo": "TASK-STRUCT", "status": "failed"},
        {"projectNo": "PROJECT-STRUCT", "deviceId": "DEVICE-STRUCT"},
        "timeout while posting report",
    )

    assert captured["report_data"]["taskNo"] == "TASK-STRUCT"
    assert captured["task_info"]["projectNo"] == "PROJECT-STRUCT"
    assert captured["failure_reason"] == "timeout while posting report"
    assert captured["endpoint"] == "http://report.example.com/direct-report"
    assert captured["metadata"]["report_source"] == "report_client"


def test_report_client_persists_failure_with_trace_context(monkeypatch):
    client = ReportClient(build_report_config())
    captured = {}

    class FakeFailedReportManager:
        def add_failed_report(
            self,
            report_data,
            task_info=None,
            max_retries=None,
            priority=0,
            failure_reason=None,
            endpoint=None,
            metadata=None,
        ):
            captured["metadata"] = metadata
            return "report-trace"

    monkeypatch.setattr(
        "core.failed_report_manager.get_failed_report_manager",
        lambda config_manager: FakeFailedReportManager(),
    )

    client._handle_report_failure(
        {
            "taskNo": "TASK-TRACE",
            "status": "failed",
            "trace_id": "trace-123",
            "attempt_id": "attempt-456",
            "error_category": "execution_failure",
        },
        {"projectNo": "PROJECT-TRACE", "deviceId": "DEVICE-TRACE"},
        "timeout while posting report",
    )

    assert captured["metadata"]["trace_id"] == "trace-123"
    assert captured["metadata"]["attempt_id"] == "attempt-456"
    assert captured["metadata"]["error_category"] == "execution_failure"


def test_report_client_builds_payload_from_execution_outcome():
    client = ReportClient(build_report_config())
    outcome = ExecutionOutcome(
        task_no="TASK-OUTCOME-1",
        status="completed",
        summary={"total": 1, "passed": 1, "failed": 0},
    )

    payload = client._build_report_data(outcome, {"projectNo": "PROJECT-OUTCOME"})

    assert payload["taskNo"] == "TASK-OUTCOME-1"
    assert payload["status"] == "completed"
    assert payload["projectNo"] == "PROJECT-OUTCOME"
    assert payload["summary"]["passed"] == 1


def test_report_client_execution_outcome_preserves_uploaded_report_address(tmp_path, monkeypatch):
    client = ReportClient(build_report_config())
    captured = {}

    class OutcomeResult:
        def __init__(self):
            self.reAddress = None

        def to_dict(self):
            return {"name": "CASE-URL", "type": "test_module", "reAddress": self.reAddress}

    def fake_make_request(method, url, **kwargs):
        if "files" in kwargs:
            return {"code": 200, "data": {"url": "http://files.example.com/outcome.html"}}
        captured["payload"] = kwargs["json"]
        return {"code": 200}

    monkeypatch.setattr(client, "_make_request", fake_make_request)

    report_file = tmp_path / "outcome.html"
    report_file.write_text("ok", encoding="utf-8")

    outcome = ExecutionOutcome(
        task_no="TASK-OUTCOME-URL",
        status="completed",
        case_results=[OutcomeResult()],
    )

    assert client.report_task_result(outcome, report_file_path=str(report_file)) is True
    client.shutdown()

    assert captured["payload"]["results"][0]["reAddress"] == "http://files.example.com/outcome.html"
