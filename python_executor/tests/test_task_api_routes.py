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


@pytest.fixture
def task_api_client():
    app = Flask(__name__)
    app.register_blueprint(task_api.task_bp)
    return app.test_client()


def test_create_task_tdm2_uses_compiled_execution_plan(task_api_client, monkeypatch):
    executed = {}

    class _FakeExecutor:
        def execute_plan(self, plan):
            executed["plan"] = plan
            return True

    monkeypatch.setattr(task_api, "task_executor", _FakeExecutor())
    monkeypatch.setattr(
        task_api,
        "get_case_mapping_manager",
        lambda: _FakeMappingManager(
            {"CASE-1": _FakeMapping("TSMASTER", script_path="D:/cfgs/task-api.cfg")}
        ),
    )

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
    assert executed["plan"].task_no == "TASK-API-1"
    assert executed["plan"].tool_type == "tsmaster"
    assert executed["plan"].config_path == "D:/cfgs/task-api.cfg"


def test_create_task_tdm2_rejects_invalid_payload_as_bad_request(task_api_client, monkeypatch):
    class _FakeExecutor:
        def execute_plan(self, plan):
            raise AssertionError("invalid payload should not reach execute_plan")

    monkeypatch.setattr(task_api, "task_executor", _FakeExecutor())
    monkeypatch.setattr(
        task_api,
        "get_case_mapping_manager",
        lambda: _FakeMappingManager({"CASE-1": _FakeMapping("CANOE")}),
    )

    response = task_api_client.post(
        "/api/tasks",
        json={
            "taskNo": "TASK-API-BAD",
            "projectNo": "PROJECT-API",
            "deviceId": "DEVICE-API",
            "caseList": [],
        },
    )

    assert response.status_code == 400
    payload = response.get_json()
    assert payload["success"] is False
    assert "testItems" in payload["message"] or "caseList" in payload["message"]


def test_create_task_delayed_internal_request_rejects_invalid_payload(task_api_client, monkeypatch):
    class _FakeScheduler:
        def schedule_task(self, task, delay):
            raise AssertionError("invalid delayed task should not reach scheduler")

    monkeypatch.setattr(task_api, "task_scheduler", _FakeScheduler())

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
    assert "testItems" in payload["message"] or "caseList" in payload["message"]


def test_cancel_task_route_uses_scheduler_to_cancel_delayed_tasks(task_api_client, monkeypatch):
    cancelled = []

    class _FakeScheduler:
        def cancel_scheduled_task(self, task_id):
            cancelled.append(task_id)
            return True

    class _FakeExecutor:
        def cancel_task(self, task_id):
            raise AssertionError("route should delegate to task scheduler first")

    monkeypatch.setattr(task_api, "task_scheduler", _FakeScheduler())
    monkeypatch.setattr(task_api, "task_executor", _FakeExecutor())

    response = task_api_client.post("/api/tasks/task-delay/cancel")

    assert response.status_code == 200
    assert cancelled == ["task-delay"]


def test_delete_task_removes_queued_executor_task_before_deleting_record(task_api_client, monkeypatch):
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

    monkeypatch.setattr(task_api, "task_executor", _FakeExecutor())
    monkeypatch.setattr(task_api.task_queue, "get_task", lambda task_id: _QueuedTask())
    monkeypatch.setattr(task_api.task_queue, "remove", lambda task_id: removed.append(task_id) or True)

    response = task_api_client.delete("/api/tasks/queued-delete")

    assert response.status_code == 200
    assert cancelled == ["queued-delete"]
    assert removed == ["queued-delete"]


def test_get_task_reports_tool_type_from_execution_metadata(task_api_client, monkeypatch):
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

    monkeypatch.setattr(task_api.task_queue, "get_task", lambda task_id: _Task())
    monkeypatch.setattr(task_api.task_log_manager, "get_log_stats", lambda task_id: {})
    monkeypatch.setattr(task_api.task_log_manager, "get_latest_logs", lambda count=20, task_id=None: [])

    response = task_api_client.get("/api/tasks/task-detail-1")

    assert response.status_code == 200
    payload = response.get_json()
    assert payload["data"]["category"] == "tsmaster"
