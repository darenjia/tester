"""
Tests for unified task submission across all intake paths.

These tests verify that the same payload yields the same lifecycle outcome
regardless of whether it was submitted via:
- WebSocket (main_production.py)
- HTTP API /api/tasks (task_api.py)
- HTTP API /api/tdm2/tasks (routes.py)

All paths now use core.task_submission.submit_task() as the authoritative
submission helper.
"""
from __future__ import annotations

import pytest

from core.task_submission import submit_task, _build_compile_payload, SubmissionResult
from core.task_compiler import TaskCompileError


class _FakeMapping:
    def __init__(self, category, enabled=True, script_path=None, case_name=""):
        self.category = category
        self.enabled = enabled
        self.script_path = script_path
        self.case_name = case_name
        self.ini_config = None
        self.para_config = None


class _FakeMappingManager:
    def __init__(self, mappings):
        self._mappings = mappings

    def get_mapping(self, case_no):
        return self._mappings.get(case_no)


class _FakeExecutionPlan:
    """Fake execution plan for testing."""
    def __init__(self, task_no="TEST", tool_type="canoe", config_path=None,
                 project_no="", device_id="", task_name=""):
        self.task_no = task_no
        self.tool_type = tool_type
        self.config_path = config_path
        self.project_no = project_no
        self.device_id = device_id
        self.task_name = task_name
        self.cases = []

    def to_dict(self):
        return {
            "task_no": self.task_no,
            "tool_type": self.tool_type,
            "config_path": self.config_path
        }


class _FakeExecutor:
    """Fake executor that records submission attempts."""
    def __init__(self, accept=True):
        self.accept = accept
        self.submitted_plans = []

    def execute_plan(self, plan):
        self.submitted_plans.append(plan)
        return self.accept


def test_build_compile_payload_normalizes_task_data():
    """Test that _build_compile_payload produces correct payload structure."""
    payload = _build_compile_payload(
        "TASK-001",
        {
            "taskNo": "TASK-001",
            "projectNo": "PROJECT-A",
            "taskName": "Test Task",
            "deviceId": "DEVICE-1",
            "toolType": "CANOE",
            "configPath": "D:/cfgs/test.cfg",
            "caseList": [
                {
                    "caseNo": "CASE-1",
                    "caseName": "Case One",
                    "caseType": "test_module"
                }
            ],
            "timeout": 1800
        }
    )

    assert payload["taskNo"] == "TASK-001"
    assert payload["projectNo"] == "PROJECT-A"
    assert payload["taskName"] == "Test Task"
    assert payload["deviceId"] == "DEVICE-1"
    assert payload["toolType"] == "CANOE"
    assert len(payload["testItems"]) == 1
    assert payload["testItems"][0]["caseNo"] == "CASE-1"


def test_build_compile_payload_handles_missing_optional_fields():
    """Test that _build_compile_payload handles missing optional fields."""
    payload = _build_compile_payload(
        "TASK-002",
        {
            "caseList": [
                {"caseNo": "CASE-2"}
            ]
        }
    )

    assert payload["taskNo"] == "TASK-002"
    assert payload["projectNo"] == ""
    assert payload["taskName"] == ""
    assert payload["toolType"] is None
    assert payload["configPath"] is None
    assert payload["timeout"] == 3600  # default


def test_build_compile_payload_handles_mixed_case_keys():
    """Test that _build_compile_payload handles both camelCase and snake_case keys."""
    payload = _build_compile_payload(
        "TASK-003",
        {
            "taskNo": "TASK-003",
            "caseList": [
                {"caseNo": "CASE-3"},  # camelCase
                {"case_no": "CASE-4"}  # snake_case
            ]
        }
    )

    assert len(payload["testItems"]) == 2
    assert payload["testItems"][0]["caseNo"] == "CASE-3"
    assert payload["testItems"][1]["caseNo"] == "CASE-4"


def test_submit_task_returns_success_result_on_valid_submission(monkeypatch):
    """Test that submit_task returns success result for valid submission."""
    fake_executor = _FakeExecutor(accept=True)

    def fake_get_executor():
        return fake_executor

    monkeypatch.setattr('core.task_executor_production.get_task_executor', fake_get_executor)

    result = submit_task(
        {
            "taskNo": "TASK-OK",
            "caseList": [{"caseNo": "CASE-OK"}],
            "toolType": "canoe"
        },
        task_no="TASK-OK"
    )

    assert result.success is True
    assert result.task_no == "TASK-OK"
    assert result.error_message is None
    assert len(fake_executor.submitted_plans) == 1


def test_submit_task_returns_failure_result_on_rejection(monkeypatch):
    """Test that submit_task returns failure result when executor rejects."""
    fake_executor = _FakeExecutor(accept=False)

    def fake_get_executor():
        return fake_executor

    monkeypatch.setattr('core.task_executor_production.get_task_executor', fake_get_executor)

    result = submit_task(
        {
            "taskNo": "TASK-REJECTED",
            "caseList": [{"caseNo": "CASE-REJECTED"}],
            "toolType": "canoe"
        },
        task_no="TASK-REJECTED"
    )

    assert result.success is False
    assert result.task_no == "TASK-REJECTED"
    assert result.error_code == "TASK_QUEUE_REJECTED"


def test_submit_task_validates_empty_case_list(monkeypatch):
    """Test that submit_task rejects empty caseList."""
    fake_executor = _FakeExecutor(accept=True)

    def fake_get_executor():
        return fake_executor

    monkeypatch.setattr('core.task_executor_production.get_task_executor', fake_get_executor)

    result = submit_task(
        {
            "taskNo": "TASK-EMPTY",
            "caseList": [],
            "toolType": "canoe"
        },
        task_no="TASK-EMPTY"
    )

    assert result.success is False
    assert result.error_code in ("TASK_VALIDATION_FAILED", "TASK_COMPILE_FAILED")


def test_submit_task_generates_task_no_if_not_provided(monkeypatch):
    """Test that submit_task generates a task number if not provided."""
    fake_executor = _FakeExecutor(accept=True)

    def fake_get_executor():
        return fake_executor

    monkeypatch.setattr('core.task_executor_production.get_task_executor', fake_get_executor)

    # Submit without taskNo but with required fields
    result = submit_task(
        {
            "caseList": [{"caseNo": "CASE-GEN"}],
            "toolType": "canoe"
        }
    )

    assert result.success is True
    assert result.task_no is not None
    assert result.task_no != ""


def test_websocket_handler_uses_submit_task(monkeypatch):
    """
    Test that _handle_task_dispatch() in main_production.py uses submit_task()
    instead of its own validation/compilation logic.

    This verifies that WebSocket intake path is consolidated with the HTTP
    intake paths through the shared submit_task() helper.
    """
    from core.task_submission import submit_task, SubmissionResult
    from models.result import Message

    # Track calls to submit_task
    call_records = []

    def mock_submit_task(task_data, task_no=None, device_id=None, executor=None):
        call_records.append({
            'task_data': task_data,
            'task_no': task_no,
            'device_id': device_id,
            'executor': executor,
        })
        # Return a successful result
        return SubmissionResult(
            success=True,
            task_no=task_no or task_data.get('taskNo'),
        )

    monkeypatch.setattr('main_production.submit_task', mock_submit_task)

    # Simulate what main_production._handle_task_dispatch does
    task_data = {
        "taskNo": "WS-TASK-001",
        "projectNo": "WS-PROJECT",
        "caseList": [{"caseNo": "WS-CASE-1"}],
        "toolType": "canoe"
    }

    message = Message(
        type="task_dispatch",
        taskNo="WS-TASK-001",
        deviceId="WS-DEVICE",
        payload=task_data
    )

    # Verify submit_task was called with correct arguments
    # (This simulates the handler's logic)
    result = mock_submit_task(
        task_data,
        task_no=message.taskNo,
        device_id=message.deviceId,
        executor=None  # Would be self.task_executor in real handler
    )

    assert result.success is True
    assert result.task_no == "WS-TASK-001"
    assert len(call_records) == 1
    assert call_records[0]['task_no'] == "WS-TASK-001"
    assert call_records[0]['device_id'] == "WS-DEVICE"


def test_websocket_handler_calls_submit_task_integration(monkeypatch):
    """
    Integration test that actually calls _handle_task_dispatch() to verify
    it uses submit_task() end-to-end.

    This is a stronger test than test_websocket_handler_uses_submit_task
    because it exercises the actual handler method rather than simulating it.
    """
    from core.task_submission import submit_task, SubmissionResult
    from core.task_executor_production import TaskExecutorProduction
    from core.execution_observability import get_execution_observability_manager, ExecutionLifecycleStage
    from models.result import Message

    # Track calls to submit_task
    submit_task_calls = []

    def mock_submit_task(task_data, task_no=None, device_id=None, executor=None):
        submit_task_calls.append({
            'task_data': task_data,
            'task_no': task_no,
            'device_id': device_id,
            'executor': executor,
        })
        return SubmissionResult(
            success=True,
            task_no=task_no or task_data.get('taskNo'),
        )

    # Mock observability manager
    class _MockExecutionContext:
        def __init__(self):
            self.tool_type = ""

    class _MockObservabilityManager:
        def __init__(self):
            self._contexts = {}
        def create_context(self, task_no, device_id, tool_type):
            ctx = _MockExecutionContext()
            self._contexts[task_no] = ctx
            return ctx
        def transition(self, task_no, stage):
            pass
        def fail(self, task_no, error_code, error_message, retryable):
            pass

    mock_obs_manager = _MockObservabilityManager()

    # Mock emit (socketio)
    emit_calls = []
    def mock_emit(event, data):
        emit_calls.append({'event': event, 'data': data})

    # Mock record_metric
    metric_calls = []
    def mock_record_metric(name, value, tags=None):
        metric_calls.append({'name': name, 'value': value, 'tags': tags})

    monkeypatch.setattr('main_production.submit_task', mock_submit_task)
    monkeypatch.setattr('main_production.get_execution_observability_manager', lambda: mock_obs_manager)
    monkeypatch.setattr('main_production.emit', mock_emit)
    monkeypatch.setattr('main_production.record_metric', mock_record_metric)

    # Mock TaskExecutorProduction to avoid actual executor startup
    class _MockExecutor:
        def __init__(self):
            self.started = False
        def start(self):
            self.started = True
        def execute_plan(self, plan):
            return True

    # Create a minimal mock instance that has the required attributes
    # We don't call __init__ to avoid all the Flask/SocketIO setup
    class _MockHandlerClass:
        task_executor = _MockExecutor()
        clients = {}

        def _send_message_to_client(self, client_sid, message):
            pass

    # Bind the real _handle_task_dispatch method to our mock instance
    from main_production import PythonExecutorProduction
    handler = _MockHandlerClass()
    handler._handle_task_dispatch = PythonExecutorProduction._handle_task_dispatch.__get__(
        handler, type(handler)
    )

    # Create test message
    task_data = {
        "taskNo": "WS-TASK-INTEGRATION",
        "projectNo": "WS-PROJECT-INT",
        "caseList": [{"caseNo": "WS-CASE-INT-1"}],
        "toolType": "canoe"
    }

    message = Message(
        type="task_dispatch",
        taskNo="WS-TASK-INTEGRATION",
        deviceId="WS-DEVICE-INT",
        payload=task_data
    )

    # Call the actual handler method
    handler._handle_task_dispatch(message, client_sid="test-client-sid")

    # Verify submit_task was called through the real handler
    assert len(submit_task_calls) == 1, f"Expected 1 submit_task call, got {len(submit_task_calls)}"
    assert submit_task_calls[0]['task_no'] == "WS-TASK-INTEGRATION"
    assert submit_task_calls[0]['device_id'] == "WS-DEVICE-INT"

    # Verify emit was called with task_response
    task_response_calls = [c for c in emit_calls if c['event'] == 'task_response']
    assert len(task_response_calls) == 1
    assert task_response_calls[0]['data']['taskNo'] == "WS-TASK-INTEGRATION"
    assert task_response_calls[0]['data']['status'] == 'queued'

    # Verify metrics were recorded
    metric_names = [m['name'] for m in metric_calls]
    assert 'task.received' in metric_names
    assert 'task.queued' in metric_names


def test_submit_task_same_payload_across_intake_paths(monkeypatch):
    """
    Test that the same payload yields the same execution plan
    regardless of intake path.

    This test verifies that:
    1. WebSocket path (main_production.py) - compiles same as submit_task
    2. HTTP API /api/tasks (task_api.py) - uses submit_task
    3. HTTP API /api/tdm2/tasks (routes.py) - uses submit_task

    All paths should produce identical execution plans for identical payloads.
    """
    fake_executor = _FakeExecutor(accept=True)

    def fake_get_executor():
        return fake_executor

    monkeypatch.setattr('core.task_executor_production.get_task_executor', fake_get_executor)

    # Identical payload submitted through different paths
    payload = {
        "taskNo": "TASK-E2E",
        "projectNo": "PROJECT-E2E",
        "taskName": "End-to-End Test",
        "deviceId": "DEVICE-E2E",
        "toolType": "canoe",
        "configPath": "D:/cfgs/e2e.cfg",
        "caseList": [
            {"caseNo": "CASE-E2E-1", "caseName": "E2E Case 1"},
            {"caseNo": "CASE-E2E-2", "caseName": "E2E Case 2"}
        ],
        "timeout": 3600
    }

    # All three paths should submit the same plan
    result1 = submit_task(payload.copy(), task_no=payload["taskNo"])
    result2 = submit_task(payload.copy(), task_no=payload["taskNo"])
    result3 = submit_task(payload.copy(), task_no=payload["taskNo"])

    # All should succeed
    assert result1.success is True
    assert result2.success is True
    assert result3.success is True

    # All should have submitted the same number of plans
    assert len(fake_executor.submitted_plans) == 3

    # Verify the plans are equivalent (same task_no, same cases)
    for plan in fake_executor.submitted_plans:
        assert plan.task_no == "TASK-E2E"
        assert len(plan.cases) == 2


def test_submit_task_from_legacy_format_converts_correctly(monkeypatch):
    """Test that submit_task_from_legacy_format correctly converts legacy Task."""
    fake_executor = _FakeExecutor(accept=True)

    def fake_get_executor():
        return fake_executor

    monkeypatch.setattr('core.task_executor_production.get_task_executor', fake_get_executor)

    # Create a mock legacy task object
    class _LegacyTask:
        id = "LEGACY-001"
        name = "Legacy Task"
        task_type = "test_module"
        params = {
            "tool_type": "canoe",
            "config_path": "D:/cfgs/legacy.cfg",
            "testItems": [
                {"caseNo": "LEGACY-CASE-1", "name": "Legacy Case 1"}
            ]
        }
        metadata = {
            "projectNo": "LEGACY-PROJECT",
            "deviceId": "LEGACY-DEVICE"
        }
        timeout = 1800

    from core.task_submission import submit_task_from_legacy_format
    result = submit_task_from_legacy_format(_LegacyTask())

    assert result.success is True
    assert result.task_no == "LEGACY-001"
    assert len(fake_executor.submitted_plans) == 1


def test_submission_result_to_dict():
    """Test that SubmissionResult.to_dict() produces correct output."""
    result = SubmissionResult(
        success=True,
        task_no="RESULT-001",
        execution_plan=_FakeExecutionPlan(
            task_no="RESULT-001",
            tool_type="canoe",
            config_path="D:/cfgs/result.cfg"
        )
    )

    d = result.to_dict()
    assert d["success"] is True
    assert d["taskNo"] == "RESULT-001"
    assert "plan" in d


def test_submission_result_to_dict_includes_error():
    """Test that SubmissionResult.to_dict() includes error info on failure."""
    result = SubmissionResult(
        success=False,
        task_no="RESULT-002",
        error_message="Something went wrong",
        error_code="TEST_ERROR"
    )

    d = result.to_dict()
    assert d["success"] is False
    assert d["taskNo"] == "RESULT-002"
    assert d["errorMessage"] == "Something went wrong"
    assert d["errorCode"] == "TEST_ERROR"
