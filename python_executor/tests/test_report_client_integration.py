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
    """Test that _report_to_remote delegates upload-submit-persist to ReportClient.report_result()"""
    executor = TaskExecutorProduction(message_sender=lambda _: None)
    report_calls = []
    persisted = {}

    class FakeReportClient:
        def __init__(self):
            pass

        def upload_report_file(self, file_path):
            report_calls.append(("upload", file_path))
            return "http://files.example.com/failure-report.html"

        def report_payload(self, report_data):
            report_calls.append(("report", report_data))
            return False

        def report_result(self, report_data, task_info=None, report_file_path=None):
            """Simulates ReportClient.report_result() behavior - upload first, then submit"""
            effective_report_data = dict(report_data)
            # First do upload if file path provided (matches real behavior)
            if report_file_path:
                self.upload_report_file(report_file_path)
                # Set reAddress on cases in the copy (shallow copy shares caseList items)
                for case in effective_report_data.get("caseList", []):
                    if not case.get("reAddress"):
                        case["reAddress"] = "http://files.example.com/failure-report.html"
            report_calls.append(("report_result", effective_report_data, task_info, report_file_path))
            # Simulate failure - call the handle_report_failure path
            self._handle_report_failure(effective_report_data, task_info, "server error")
            return False

        def _handle_report_failure(self, report_data, task_info, error):
            """Simulates persisting failed report"""
            persisted.update({"report_data": report_data, "task_info": task_info, "error": error})

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

    executor._report_to_remote(task, task_result, report_file_path="C:/temp/report.html")

    # Verify upload was called with correct path
    assert report_calls[0] == ("upload", "C:/temp/report.html")
    # Verify report_result was called with the modified data (contains reAddress)
    assert report_calls[1][0] == "report_result"
    assert report_calls[1][1]["caseList"][0]["reAddress"] == "http://files.example.com/failure-report.html"
    # Verify persistence received correct data
    assert persisted["report_data"]["taskNo"] == "TASK-9"
    assert persisted["error"] == "server error"


def test_executor_remote_report_accepts_execution_outcome(monkeypatch):
    executor = TaskExecutorProduction(message_sender=lambda _: None)
    captured = {}

    class FakeReportClient:
        def report_result(self, report_data, task_info=None, report_file_path=None):
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
        def report_result(self, report_data, task_info=None, report_file_path=None):
            captured["report_data"] = report_data
            captured["task_info"] = task_info
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
    # Verify task_info also contains trace context for failure persistence
    assert captured["task_info"]["trace_id"] == captured["report_data"]["trace_id"]
    assert captured["task_info"]["attempt_id"] == captured["report_data"]["attempt_id"]


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


def test_report_result_successful_upload_and_submit(tmp_path, monkeypatch):
    """Test ReportClient.report_result() with successful upload and successful submit"""
    client = ReportClient(build_report_config())
    captured = {}

    def fake_make_request(method, url, **kwargs):
        if "files" in kwargs:
            return {"code": 200, "data": {"url": "http://files.example.com/report.html"}}
        captured["payload"] = kwargs["json"]
        return {"code": 200}

    monkeypatch.setattr(client, "_make_request", fake_make_request)

    report_file = tmp_path / "report.html"
    report_file.write_text("ok", encoding="utf-8")

    report_data = {
        "taskNo": "TASK-SUCCESS",
        "caseList": [{"caseNo": "CASE-1", "result": "PASS"}]
    }

    task_info = {
        "projectNo": "PROJ-SUCCESS",
        "deviceId": "DEVICE-SUCCESS",
        "trace_id": "trace-success-123",
        "attempt_id": "attempt-success-456",
    }

    result = client.report_result(report_data, task_info, report_file_path=str(report_file))
    client.shutdown()

    assert result is True
    assert captured["payload"]["caseList"][0]["reAddress"] == "http://files.example.com/report.html"
    assert captured["payload"]["taskNo"] == "TASK-SUCCESS"


def test_report_result_successful_upload_failed_submit(tmp_path, monkeypatch):
    """Test ReportClient.report_result() with successful upload but failed submit"""
    client = ReportClient(build_report_config())
    persisted = {}

    class FakeFailedReportManager:
        def add_failed_report(self, report_data, task_info=None, max_retries=None,
                              priority=0, failure_reason=None, endpoint=None, metadata=None):
            persisted["report_data"] = report_data
            persisted["task_info"] = task_info
            persisted["max_retries"] = max_retries
            persisted["failure_reason"] = failure_reason
            persisted["endpoint"] = endpoint
            persisted["metadata"] = metadata
            return "report-persist-id"

    monkeypatch.setattr(
        "core.failed_report_manager.get_failed_report_manager",
        lambda config_manager: FakeFailedReportManager(),
    )

    def fake_make_request(method, url, **kwargs):
        if "files" in kwargs:
            return {"code": 200, "data": {"url": "http://files.example.com/fail.html"}}
        # Simulate server error
        return None

    monkeypatch.setattr(client, "_make_request", fake_make_request)

    report_file = tmp_path / "fail.html"
    report_file.write_text("fail", encoding="utf-8")

    report_data = {
        "taskNo": "TASK-FAIL-SUBMIT",
        "caseList": [{"caseNo": "CASE-1", "result": "FAIL"}]
    }

    task_info = {
        "projectNo": "PROJ-FAIL",
        "deviceId": "DEVICE-FAIL",
        "trace_id": "trace-fail-123",
        "attempt_id": "attempt-fail-456",
        "error_category": "report_failure",
    }

    result = client.report_result(report_data, task_info, report_file_path=str(report_file))
    client.shutdown()

    assert result is False
    # Verify persistence was called with correct data
    assert persisted["report_data"]["taskNo"] == "TASK-FAIL-SUBMIT"
    assert persisted["report_data"]["caseList"][0]["reAddress"] == "http://files.example.com/fail.html"
    assert persisted["failure_reason"] == "服务器无响应"
    assert persisted["metadata"]["trace_id"] == "trace-fail-123"
    assert persisted["metadata"]["attempt_id"] == "attempt-fail-456"
    assert persisted["metadata"]["error_category"] == "report_failure"


def test_execution_outcome_report_url_retained_in_payload(tmp_path, monkeypatch):
    """Test that ExecutionOutcome retains uploaded report URL in the final payload"""
    client = ReportClient(build_report_config())
    captured = {}

    def fake_make_request(method, url, **kwargs):
        if "files" in kwargs:
            return {"code": 200, "data": {"url": "http://files.example.com/outcome-final.html"}}
        captured["payload"] = kwargs["json"]
        return {"code": 200}

    monkeypatch.setattr(client, "_make_request", fake_make_request)

    report_file = tmp_path / "outcome-final.html"
    report_file.write_text("ok", encoding="utf-8")

    # Create ExecutionOutcome with case_results that have reAddress attribute
    class OutcomeCaseResult:
        def __init__(self):
            self.reAddress = None

        def to_dict(self):
            return {"caseNo": "CASE-OUTCOME", "result": "PASS", "reAddress": self.reAddress}

    outcome = ExecutionOutcome(
        task_no="TASK-OUTCOME-FINAL",
        status="completed",
        summary={"total": 1, "passed": 1},
        case_results=[OutcomeCaseResult()],
    )

    result = client.report_task_result(outcome, report_file_path=str(report_file))
    client.shutdown()

    assert result is True
    # Verify the uploaded URL is in the payload
    assert captured["payload"]["results"][0]["reAddress"] == "http://files.example.com/outcome-final.html"
    assert captured["payload"]["taskNo"] == "TASK-OUTCOME-FINAL"


def test_retry_persistence_contains_trace_attempt_error_metadata(monkeypatch):
    """Test that failed report persistence contains trace/attempt/error metadata"""
    client = ReportClient(build_report_config())
    captured = {}

    class FakeFailedReportManager:
        def add_failed_report(self, report_data, task_info=None, max_retries=None,
                              priority=0, failure_reason=None, endpoint=None, metadata=None):
            captured["report_data"] = report_data
            captured["task_info"] = task_info
            captured["metadata"] = metadata
            return "retry-report-id"

    monkeypatch.setattr(
        "core.failed_report_manager.get_failed_report_manager",
        lambda config_manager: FakeFailedReportManager(),
    )

    client._handle_report_failure(
        report_data={
            "taskNo": "TASK-RETRY-META",
            "status": "failed",
        },
        task_info={
            "projectNo": "PROJ-RETRY",
            "deviceId": "DEVICE-RETRY",
            "trace_id": "trace-retry-abc",
            "traceId": "trace-retry-abc",
            "attempt_id": "attempt-retry-def",
            "attemptId": "attempt-retry-def",
            "error_category": "execution_failure",
            "errorCategory": "execution_failure",
            "execution_error_category": "timeout",
        },
        error="connection refused",
    )

    # Verify metadata contains all required fields
    assert captured["metadata"]["trace_id"] == "trace-retry-abc"
    assert captured["metadata"]["attempt_id"] == "attempt-retry-def"
    assert captured["metadata"]["error_category"] == "execution_failure"
    assert captured["metadata"]["execution_error_category"] == "timeout"
    assert captured["metadata"]["report_source"] == "report_client"
    assert captured["metadata"]["failure_reason"] == "connection refused"
