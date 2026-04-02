from __future__ import annotations

from flask import Flask

import pytest

from api import task_api


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
    def __init__(self, task_no="TEST", tool_type="canoe", config_path=None, project_no="", device_id=""):
        self.task_no = task_no
        self.tool_type = tool_type
        self.config_path = config_path
        self.project_no = project_no
        self.device_id = device_id
        self.cases = [type('Case', (), {'case_no': 'CASE-1'})()]


class _FakeSubmissionResult:
    """Fake submission result for testing."""
    def __init__(self, success=True, task_no="TEST", execution_plan=None, error_message=None, error_code=None):
        self.success = success
        self.task_no = task_no
        self.execution_plan = execution_plan
        self.error_message = error_message
        self.error_code = error_code


@pytest.fixture
def task_api_client():
    app = Flask(__name__)
    app.register_blueprint(task_api.task_bp)
    return app.test_client()


def test_create_task_tdm2_uses_submit_task(task_api_client, monkeypatch):
    """Test that TDM2.0 format task creation uses submit_task."""
    submitted_data = {}

    def fake_submit_task(task_data, task_no=None, device_id=None, executor=None):
        submitted_data['task_data'] = task_data
        submitted_data['task_no'] = task_no
        return _FakeSubmissionResult(
            success=True,
            task_no=task_no or task_data.get('taskNo'),
            execution_plan=_FakeExecutionPlan(
                task_no=task_no or task_data.get('taskNo'),
                tool_type='tsmaster',
                config_path='D:/cfgs/task-api.cfg'
            )
        )

    monkeypatch.setattr(task_api, 'submit_task', fake_submit_task)

    response = task_api_client.post(
        "/api/tasks",
        json={
            "taskNo": "TASK-API-1",
            "projectNo": "PROJECT-API",
            "deviceId": "DEVICE-API",
            "caseList": [{"caseNo": "CASE-1"}],
            "timeout": 45,
        },
    )

    assert response.status_code == 200
    data = response.get_json()
    assert data["success"] is True
    assert submitted_data['task_no'] == "TASK-API-1"


def test_create_task_tdm2_rejects_invalid_payload_as_bad_request(task_api_client, monkeypatch):
    """Test that invalid TDM2.0 payload is rejected."""
    call_count = [0]

    def fake_submit_task(task_data, task_no=None, device_id=None, executor=None):
        call_count[0] += 1
        # Return failure for empty caseList
        return _FakeSubmissionResult(
            success=False,
            task_no=task_no,
            error_message="testItems不能为空",
            error_code="TASK_VALIDATION_FAILED"
        )

    monkeypatch.setattr(task_api, 'submit_task', fake_submit_task)

    response = task_api_client.post(
        "/api/tasks",
        json={
            "taskNo": "TASK-API-BAD",
            "projectNo": "PROJECT-API",
            "deviceId": "DEVICE-API",
            "caseList": [],
        },
    )

    assert response.status_code == 500  # submit_task returns 500 on failure
    payload = response.get_json()
    assert payload["success"] is False
    # The error should mention the validation failure


def test_create_task_delayed_internal_request_rejects_invalid_payload(task_api_client, monkeypatch):
    """Test that delayed internal format tasks still use scheduler."""
    class _FakeScheduler:
        def schedule_task(self, task, delay):
            raise AssertionError("invalid delayed task should not reach scheduler")

    monkeypatch.setattr(task_api, 'task_scheduler', _FakeScheduler())

    response = task_api_client.post(
        "/api/tasks",
        json={
            "taskNo": "TASK-DELAY-BAD",
            "name": "Delayed Bad Task",
            "type": "default",
            "delay": 30,
            "params": {
                "tool_type": "canoe",
                "config_path": "D:/cfgs/demo.cfg",
            },
        },
    )

    assert response.status_code == 400
    payload = response.get_json()
    assert payload["success"] is False


def test_cancel_task_route_uses_scheduler_to_cancel_delayed_tasks(task_api_client, monkeypatch):
    """Test that cancel task uses scheduler for delayed tasks."""
    cancelled = []

    class _FakeScheduler:
        def cancel_scheduled_task(self, task_id):
            cancelled.append(task_id)
            return True

    class _FakeExecutor:
        def cancel_task(self, task_id):
            raise AssertionError("route should delegate to task scheduler first")

    monkeypatch.setattr(task_api, 'task_scheduler', _FakeScheduler())
    monkeypatch.setattr(task_api, 'task_executor', _FakeExecutor())

    response = task_api_client.post("/api/tasks/task-delay/cancel")

    assert response.status_code == 200
    assert cancelled == ["task-delay"]


def test_cancel_task_route_accepts_periodic_scheduler_id(task_api_client, monkeypatch):
    """Test that cancel task accepts periodic scheduler IDs."""
    cancelled = []

    class _FakeScheduler:
        def cancel_scheduled_task(self, task_id):
            cancelled.append(task_id)
            return task_id == "periodic_task-1"

    class _FakeExecutor:
        def cancel_task(self, task_id):
            raise AssertionError("periodic cancel should be handled by scheduler registry")

    monkeypatch.setattr(task_api, 'task_scheduler', _FakeScheduler())
    monkeypatch.setattr(task_api, 'task_executor', _FakeExecutor())

    response = task_api_client.post("/api/tasks/periodic_task-1/cancel")

    assert response.status_code == 200
    assert cancelled == ["periodic_task-1"]


def test_delete_task_removes_queued_executor_task_before_deleting_record(task_api_client, monkeypatch):
    """Test that delete removes task from executor before deleting record."""
    removed = []
    cancelled = []

    class _QueuedTask:
        def __init__(self):
            self.id = "queued-delete"
            self.status = "pending"

        def is_running(self):
            return False

    class _FakeExecutor:
        def cancel_task(self, task_id):
            cancelled.append(task_id)
            return True

    monkeypatch.setattr(task_api, 'task_executor', _FakeExecutor())
    monkeypatch.setattr(task_api.task_queue, 'get_task', lambda task_id: _QueuedTask())
    monkeypatch.setattr(task_api.task_queue, 'remove', lambda task_id: removed.append(task_id) or True)

    response = task_api_client.delete("/api/tasks/queued-delete")

    assert response.status_code == 200
    assert cancelled == ["queued-delete"]
    assert removed == ["queued-delete"]


def test_get_task_reports_tool_type_from_execution_metadata(task_api_client, monkeypatch):
    """Test that get_task reports tool type from metadata."""
    class _Task:
        id = "task-detail-1"
        name = "Task Detail"
        status = "pending"
        priority = 1
        task_type = "default"
        created_at = "2026-04-02T09:00:00"
        created_by = "tester"
        error_message = None
        started_at = None
        completed_at = None
        timeout = 30
        retry_count = 0
        max_retries = 3
        params = {"tool_type": "tsmaster"}
        metadata = {"toolType": "TSMASTER"}
        result = None

        def can_retry(self):
            return False

        def get_duration(self):
            return None

        def get_wait_time(self):
            return 1.0

    monkeypatch.setattr(task_api.task_queue, 'get_task', lambda task_id: _Task())
    monkeypatch.setattr(task_api.task_log_manager, 'get_log_stats', lambda task_id: {})
    monkeypatch.setattr(task_api.task_log_manager, 'get_latest_logs', lambda count=20, task_id=None: [])

    response = task_api_client.get("/api/tasks/task-detail-1")

    assert response.status_code == 200
    payload = response.get_json()
    assert payload["data"]["category"] == "tsmaster"


def test_get_scheduled_tasks_includes_periodic_registrations(task_api_client, monkeypatch):
    """Test that get_scheduled_tasks includes periodic registrations."""
    monkeypatch.setattr(
        task_api,
        'task_scheduler',
        type(
            "_FakeScheduler",
            (),
            {
                "get_scheduled_tasks": staticmethod(
                    lambda: [
                        {
                            "task_id": "scheduled-1",
                            "task_name": "Scheduled 1",
                            "scheduled_time": "2026-04-02T10:00:00",
                            "status": "pending",
                        },
                        {
                            "task_id": "periodic-1",
                            "task_name": "Periodic 1",
                            "scheduled_time": None,
                            "status": "running",
                            "scheduler_id": "periodic_periodic-1",
                            "schedule_type": "periodic",
                        },
                    ]
                )
            },
        )(),
    )

    response = task_api_client.get("/api/tasks/scheduled")

    assert response.status_code == 200
    payload = response.get_json()
    assert len(payload["data"]) == 2
    assert payload["data"][1]["schedule_type"] == "periodic"


def test_task_stats_exposes_periodic_scheduler_count(task_api_client, monkeypatch):
    """Test that task stats exposes periodic scheduler count."""
    monkeypatch.setattr(task_api.task_queue, 'get_stats', lambda: {"total": 1, "pending": 1})
    monkeypatch.setattr(task_api, 'task_executor', type("_FakeExecutor", (), {"get_stats": staticmethod(lambda: {"queue_size": 0})})())
    monkeypatch.setattr(
        task_api,
        'task_scheduler',
        type("_FakeScheduler", (), {"get_stats": staticmethod(lambda: {"running": True, "scheduled_count": 3, "periodic_count": 2})})(),
    )

    response = task_api_client.get("/api/tasks/stats")

    assert response.status_code == 200
    payload = response.get_json()
    assert payload["data"]["scheduler"]["periodic_count"] == 2
