from __future__ import annotations

from flask import Flask

import pytest

from api import routes


class _FakeMapping:
    def __init__(self, category):
        self.category = category


class _FakeMappingManager:
    def __init__(self, mappings):
        self._mappings = mappings

    def get_mapping(self, case_no):
        return self._mappings.get(case_no)


@pytest.fixture
def tdm2_client(monkeypatch):
    app = Flask(__name__)
    app.register_blueprint(routes.api_bp)

    monkeypatch.setattr(routes.task_queue, "get_task", lambda task_no: None)
    monkeypatch.setattr(routes.task_queue, "get_running_tasks", lambda: [])
    monkeypatch.setattr(routes.task_queue, "add", lambda task: True)
    monkeypatch.setattr("api.task_executor.execute_task_async", lambda task_no, data: None)

    return app.test_client()


def test_tdm2_create_task_rejects_mixed_tool_types(tdm2_client, monkeypatch):
    monkeypatch.setattr(
        routes,
        "get_case_mapping_manager",
        lambda: _FakeMappingManager(
            {
                "CASE-1": _FakeMapping("CANOE"),
                "CASE-2": _FakeMapping("TSMASTER"),
            }
        ),
    )

    response = tdm2_client.post(
        "/api/tdm2/tasks",
        json={
            "projectNo": "PROJECT-MIXED",
            "taskNo": "TASK-MIXED",
            "caseList": [{"caseNo": "CASE-1"}, {"caseNo": "CASE-2"}],
        },
    )

    assert response.status_code == 400
    payload = response.get_json()
    assert payload["status"] == "error"
    assert "工具" in payload["message"] or "tool" in payload["message"].lower()


def test_tdm2_create_task_rejects_empty_case_list(tdm2_client):
    response = tdm2_client.post(
        "/api/tdm2/tasks",
        json={
            "projectNo": "PROJECT-EMPTY",
            "taskNo": "TASK-EMPTY",
            "caseList": [],
        },
    )

    assert response.status_code == 400
    payload = response.get_json()
    assert payload["status"] == "error"
    assert "caseList" in payload["message"]
