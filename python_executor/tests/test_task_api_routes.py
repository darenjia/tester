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

