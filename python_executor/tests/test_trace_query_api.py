from __future__ import annotations

import importlib
from pathlib import Path
import sys

import gevent.monkey
from flask import Flask

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from api import docs_api, trace_query_api


@pytest.fixture
def trace_query_client():
    app = Flask(__name__)
    app.register_blueprint(trace_query_api.trace_query_bp)
    app.register_blueprint(docs_api.docs_bp)
    return app.test_client()


def test_trace_query_returns_success_payload(trace_query_client, monkeypatch):
    captured = {}

    class _FakeService:
        def query(self, **kwargs):
            captured["kwargs"] = kwargs
            return {
                "meta": {"result_type": "full"},
                "task": {"taskNo": "TASK-1"},
                "report": None,
                "observability": None,
                "logs": [],
                "events": [],
            }

    monkeypatch.setattr(trace_query_api, "get_trace_query_service", lambda: _FakeService())

    response = trace_query_client.get("/api/trace/query", query_string={"task_no": "TASK-1"})

    assert response.status_code == 200
    payload = response.get_json()
    assert payload["success"] is True
    assert payload["data"]["task"]["taskNo"] == "TASK-1"
    assert captured["kwargs"] == {
        "trace_id": None,
        "attempt_id": None,
        "task_no": "TASK-1",
        "report_id": None,
    }


def test_trace_query_rejects_invalid_selector_combinations(trace_query_client):
    response = trace_query_client.get(
        "/api/trace/query",
        query_string={"trace_id": "TRACE-1", "task_no": "TASK-1"},
    )

    assert response.status_code == 400
    payload = response.get_json()
    assert payload["success"] is False
    assert payload["error"]["code"] == "TRACE_QUERY_INVALID"


def test_trace_query_rejects_duplicate_values_for_single_selector(trace_query_client, monkeypatch):
    called = {}

    class _FakeService:
        def query(self, **kwargs):
            called["kwargs"] = kwargs
            return {"meta": {"result_type": "full"}}

    monkeypatch.setattr(trace_query_api, "get_trace_query_service", lambda: _FakeService())

    response = trace_query_client.get(
        "/api/trace/query",
        query_string=[("trace_id", "TRACE-1"), ("trace_id", "TRACE-2")],
    )

    assert response.status_code == 400
    payload = response.get_json()
    assert payload["success"] is False
    assert payload["error"]["code"] == "TRACE_QUERY_INVALID"
    assert called == {}


def test_trace_query_returns_not_found_for_missing_trace(trace_query_client, monkeypatch):
    class _FakeService:
        def query(self, **kwargs):
            raise LookupError("TRACE_NOT_FOUND")

    monkeypatch.setattr(trace_query_api, "get_trace_query_service", lambda: _FakeService())

    response = trace_query_client.get("/api/trace/query", query_string={"report_id": "REPORT-404"})

    assert response.status_code == 404
    payload = response.get_json()
    assert payload["success"] is False
    assert payload["error"]["code"] == "TRACE_NOT_FOUND"


def test_trace_query_endpoint_is_documented(trace_query_client):
    response = trace_query_client.get("/api/docs/endpoints", query_string={"category": "系统服务"})

    assert response.status_code == 200
    payload = response.get_json()
    paths = {(item["path"], item["method"]) for item in payload["data"]["endpoints"]}
    assert ("/api/trace/query", "GET") in paths


def test_main_production_registers_trace_query_endpoint(monkeypatch):
    monkeypatch.setattr(gevent.monkey, "patch_all", lambda *args, **kwargs: None)
    main_production = importlib.import_module("main_production")

    monkeypatch.setattr(main_production.signal, "signal", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_production.config_manager, "start_watcher", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_production.config_manager, "stop_watcher", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_production.performance_monitor, "start", lambda *args, **kwargs: None)
    monkeypatch.setattr(
        main_production.performance_monitor,
        "set_alert_threshold",
        lambda *args, **kwargs: None,
    )
    monkeypatch.setattr(
        main_production.performance_monitor,
        "register_alert_callback",
        lambda *args, **kwargs: None,
    )
    monkeypatch.setattr(main_production.performance_monitor, "stop", lambda *args, **kwargs: None)
    monkeypatch.setattr(
        trace_query_api,
        "get_trace_query_service",
        lambda: type(
            "_FakeService",
            (),
            {"query": staticmethod(lambda **kwargs: {"meta": {"result_type": "partial"}})},
        )(),
    )

    executor = main_production.PythonExecutorProduction()

    try:
        response = executor.app.test_client().get("/api/trace/query", query_string={"task_no": "TASK-PROD"})
    finally:
        executor.shutdown()

    assert response.status_code == 200
    payload = response.get_json()
    assert payload["success"] is True
