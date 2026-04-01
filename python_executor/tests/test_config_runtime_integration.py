from __future__ import annotations

import importlib
import io
import json
from pathlib import Path

import pytest
from flask import Flask

from config import unified_config
from core.execution_observability import (
    ExecutionLifecycleStage,
    get_execution_observability_manager,
)
from models.task import TaskResult
from utils.metrics import metric_collector


def _write_config(
    path: Path,
    *,
    port: int,
    result_api_url: str,
    workspace_path: str,
    rule_replacement: str,
) -> None:
    path.write_text(
        json.dumps(
            {
                "websocket": {"port": port, "host": "127.0.0.1"},
                "report": {"enabled": True, "result_api_url": result_api_url},
                "software": {"workspace_path": workspace_path},
                "config_base_dir": workspace_path,
                "category_ini_config_rules": {
                    "canoe": {
                        "pattern": "^(.+)$",
                        "replacement": rule_replacement,
                    }
                },
            }
        ),
        encoding="utf-8",
    )


@pytest.fixture
def runtime_modules(tmp_path: Path):
    config_a = tmp_path / "config-a.json"
    config_b = tmp_path / "config-b.json"
    workspace_a = str(tmp_path / "workspace-a")
    workspace_b = str(tmp_path / "workspace-b")
    Path(workspace_a).mkdir()
    Path(workspace_b).mkdir()

    _write_config(
        config_a,
        port=9101,
        result_api_url="http://config-a.example/report",
        workspace_path=workspace_a,
        rule_replacement="CONFIG_A_RULE=1",
    )
    _write_config(
        config_b,
        port=9202,
        result_api_url="http://config-b.example/report",
        workspace_path=workspace_b,
        rule_replacement="CONFIG_B_RULE=1",
    )

    original_instance = getattr(unified_config, "_config_manager_instance", None)
    observability_manager = get_execution_observability_manager()
    observability_manager._contexts.clear()
    metric_collector.metrics.clear()
    unified_config._config_manager_instance = unified_config.UnifiedConfigManager(str(config_a))

    settings_module = importlib.reload(importlib.import_module("config.settings"))
    manager_module = importlib.reload(importlib.import_module("config.config_manager"))
    report_client_module = importlib.reload(importlib.import_module("utils.report_client"))
    case_mapping_module = importlib.reload(importlib.import_module("core.case_mapping_manager"))
    status_monitor_module = importlib.reload(importlib.import_module("core.status_monitor"))
    task_executor_module = importlib.reload(
        importlib.import_module("core.task_executor_production")
    )
    api_config_module = importlib.reload(importlib.import_module("api.config_api"))
    main_module = importlib.reload(importlib.import_module("main_production"))

    original_report_client = getattr(report_client_module, "_report_client_instance", None)
    original_case_manager = getattr(case_mapping_module, "_case_mapping_manager", None)
    original_status_monitor = getattr(status_monitor_module, "_status_monitor", None)
    original_task_executor = getattr(task_executor_module, "task_executor", None)

    unified_config._config_manager_instance = unified_config.UnifiedConfigManager(str(config_b))

    yield {
        "manager_module": manager_module,
        "report_client_module": report_client_module,
        "case_mapping_module": case_mapping_module,
        "status_monitor_module": status_monitor_module,
        "task_executor_module": task_executor_module,
        "api_config_module": api_config_module,
        "main_module": main_module,
        "workspace_b": workspace_b,
    }

    report_client_module._report_client_instance = original_report_client
    case_mapping_module._case_mapping_manager = original_case_manager
    status_monitor_module._status_monitor = original_status_monitor
    task_executor_module.task_executor = original_task_executor
    unified_config._config_manager_instance = original_instance
    observability_manager._contexts.clear()
    metric_collector.metrics.clear()

    importlib.reload(settings_module)
    importlib.reload(manager_module)
    importlib.reload(report_client_module)
    importlib.reload(case_mapping_module)
    importlib.reload(status_monitor_module)
    importlib.reload(task_executor_module)
    importlib.reload(api_config_module)
    importlib.reload(main_module)


def test_runtime_consumers_follow_active_unified_config_instance(
    runtime_modules, monkeypatch
):
    report_client_module = runtime_modules["report_client_module"]
    case_mapping_module = runtime_modules["case_mapping_module"]
    status_monitor_module = runtime_modules["status_monitor_module"]
    task_executor_module = runtime_modules["task_executor_module"]
    workspace_b = runtime_modules["workspace_b"]

    report_client = report_client_module.ReportClient()
    assert report_client._result_api_url == "http://config-b.example/report"

    case_mapping_module._case_mapping_manager = None
    case_manager = case_mapping_module.get_case_mapping_manager()
    assert case_manager.apply_ini_config_rule("CASE_001", "canoe") == "CONFIG_B_RULE=1"

    disk_usage_calls: list[str] = []

    class _MemoryInfo:
        percent = 42.0
        used = 4 * 1024**3
        total = 8 * 1024**3

    class _DiskInfo:
        percent = 73.0

    monkeypatch.setattr(status_monitor_module.psutil, "cpu_percent", lambda interval=0.1: 12.5)
    monkeypatch.setattr(status_monitor_module.psutil, "virtual_memory", lambda: _MemoryInfo())
    monkeypatch.setattr(
        status_monitor_module.psutil,
        "disk_usage",
        lambda path: disk_usage_calls.append(path) or _DiskInfo(),
    )

    monitor = status_monitor_module.StatusMonitor()
    monitor.update_system_stats()

    assert disk_usage_calls == [workspace_b]

    executor = task_executor_module.TaskExecutorProduction(message_sender=lambda _: None)
    try:
        assert executor.report_client._result_api_url == "http://config-b.example/report"
    finally:
        executor.stop()


def test_config_api_updates_the_active_unified_config_snapshot(runtime_modules):
    api_config_module = runtime_modules["api_config_module"]

    app = Flask(__name__)
    app.register_blueprint(api_config_module.config_bp)
    client = app.test_client()

    get_response = client.get("/api/config")
    assert get_response.status_code == 200
    assert get_response.get_json()["data"]["websocket"]["port"] == 9202

    update_response = client.post("/api/config", json={"websocket": {"port": 9303}})

    assert update_response.status_code == 200
    assert update_response.get_json()["success"] is True
    assert unified_config.get_config_manager().get("websocket.port") == 9303


def test_config_api_rejects_invalid_non_http_config(runtime_modules):
    api_config_module = runtime_modules["api_config_module"]

    app = Flask(__name__)
    app.register_blueprint(api_config_module.config_bp)
    client = app.test_client()

    response = client.post("/api/config", json={"logging": {"level": "TRACE"}})

    assert response.status_code == 400
    payload = response.get_json()
    assert payload["success"] is False
    assert "logging.level" in payload["errors"][0]


def test_config_api_file_import_honors_merge_false(runtime_modules):
    api_config_module = runtime_modules["api_config_module"]
    active_manager = unified_config.get_config_manager()
    active_manager.set("logging.level", "DEBUG", persist=True)

    app = Flask(__name__)
    app.register_blueprint(api_config_module.config_bp)
    client = app.test_client()

    response = client.post(
        "/api/config/import",
        data={
            "merge": "false",
            "file": (
                io.BytesIO(
                    json.dumps({"config": {"websocket": {"port": 9309}}}).encode("utf-8")
                ),
                "config.json",
            ),
        },
        content_type="multipart/form-data",
    )

    assert response.status_code == 200
    payload = response.get_json()
    assert payload["success"] is True
    assert unified_config.get_config_manager().get("websocket.port") == 9309
    assert unified_config.get_config_manager().get("logging.level") == "INFO"


def test_main_production_config_route_uses_active_unified_config_manager(
    runtime_modules, monkeypatch
):
    main_module = runtime_modules["main_module"]
    active_manager = unified_config.get_config_manager()

    monkeypatch.setattr(main_module.signal, "signal", lambda *args, **kwargs: None)
    monkeypatch.setattr(active_manager, "start_watcher", lambda *args, **kwargs: None)
    monkeypatch.setattr(
        main_module.performance_monitor, "start", lambda *args, **kwargs: None
    )
    monkeypatch.setattr(
        main_module.performance_monitor,
        "set_alert_threshold",
        lambda *args, **kwargs: None,
    )
    monkeypatch.setattr(
        main_module.performance_monitor,
        "register_alert_callback",
        lambda *args, **kwargs: None,
    )

    executor = main_module.PythonExecutorProduction()

    try:
        response = executor.app.test_client().get("/config")
    finally:
        executor.shutdown()

    assert response.status_code == 200
    payload = response.get_json()
    assert payload["config"]["websocket"]["port"] == 9202
    assert payload["validation_errors"] == []


def test_metrics_endpoint_includes_business_summary(runtime_modules, monkeypatch):
    main_module = runtime_modules["main_module"]
    observability_manager = get_execution_observability_manager()
    observability_manager.create_context(
        task_no="TASK-M",
        device_id="DEVICE-M",
        tool_type="canoe",
    )
    observability_manager.transition("TASK-M", ExecutionLifecycleStage.QUEUED)

    monkeypatch.setattr(main_module.signal, "signal", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_module.config_manager, "start_watcher", lambda *args, **kwargs: None)
    monkeypatch.setattr(
        main_module.performance_monitor, "start", lambda *args, **kwargs: None
    )
    monkeypatch.setattr(
        main_module.performance_monitor,
        "set_alert_threshold",
        lambda *args, **kwargs: None,
    )
    monkeypatch.setattr(
        main_module.performance_monitor,
        "register_alert_callback",
        lambda *args, **kwargs: None,
    )

    executor = main_module.PythonExecutorProduction()

    try:
        response = executor.app.test_client().get("/metrics")
    finally:
        executor.shutdown()

    assert response.status_code == 200
    payload = response.get_json()
    assert payload["business_summary"]["queued_count"] == 1
    assert payload["business_summary"]["active_count"] == 0
    assert payload["business_summary"]["recent_failed_count"] == 0
    assert payload["business_summary"]["report_success_count"] == 0
    assert payload["business_summary"]["report_failure_count"] == 0


def test_health_endpoint_reports_business_health(runtime_modules, monkeypatch):
    main_module = runtime_modules["main_module"]
    observability_manager = get_execution_observability_manager()
    observability_manager.create_context(
        task_no="TASK-H",
        device_id="DEVICE-H",
        tool_type="canoe",
    )
    observability_manager.fail(
        "TASK-H",
        error_code="REPORT_FAIL",
        error_message="report failed",
    )

    monkeypatch.setattr(main_module.signal, "signal", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_module.config_manager, "start_watcher", lambda *args, **kwargs: None)
    monkeypatch.setattr(
        main_module.performance_monitor, "start", lambda *args, **kwargs: None
    )
    monkeypatch.setattr(
        main_module.performance_monitor,
        "set_alert_threshold",
        lambda *args, **kwargs: None,
    )
    monkeypatch.setattr(
        main_module.performance_monitor,
        "register_alert_callback",
        lambda *args, **kwargs: None,
    )

    executor = main_module.PythonExecutorProduction()

    try:
        response = executor.app.test_client().get("/health")
    finally:
        executor.shutdown()

    assert response.status_code == 200
    payload = response.get_json()
    assert payload["status"] == "degraded"
    assert payload["business_health"]["recent_failed_count"] == 1
    assert payload["business_health"]["healthy"] is False


def test_report_exception_still_finishes_observability_context(runtime_modules, monkeypatch):
    task_executor_module = runtime_modules["task_executor_module"]
    observability_manager = get_execution_observability_manager()
    observability_manager.create_context(
        task_no="TASK-R",
        device_id="DEVICE-R",
        tool_type="canoe",
    )
    observability_manager.transition("TASK-R", ExecutionLifecycleStage.REPORTING)

    monkeypatch.setattr(
        task_executor_module.TaskExecutorProduction,
        "_build_execution_result",
        lambda self, task, task_result: type(
            "_ExecutionResult",
            (),
            {"caseList": [], "to_tdm2_format": lambda self: {"taskNo": "TASK-R", "caseList": []}},
        )(),
    )
    monkeypatch.setattr(
        task_executor_module.TaskExecutorProduction,
        "_do_report_direct",
        lambda self, report_data: (_ for _ in ()).throw(RuntimeError("boom")),
    )
    persisted_reports = []
    monkeypatch.setattr(
        task_executor_module.TaskExecutorProduction,
        "_persist_failed_report",
        lambda self, report_data, task: persisted_reports.append((report_data, task.task_id)),
    )

    executor = task_executor_module.TaskExecutorProduction(message_sender=lambda _: None)
    task = type(
        "_Task",
        (),
        {
            "task_id": "TASK-R",
            "projectNo": "PROJECT-R",
            "deviceId": "DEVICE-R",
            "taskName": "Task R",
            "taskNo": "TASK-R",
            "toolType": "canoe",
        },
    )()
    task_result = TaskResult(taskNo="TASK-R", status="failed", summary={}, errorMessage="boom")

    try:
        executor._report_to_remote(task, task_result)
    finally:
        executor.stop()

    snapshot = observability_manager.get_snapshot("TASK-R")
    assert snapshot["current_stage"] == ExecutionLifecycleStage.FINISHED.value
    assert snapshot["report_status"] == "failed"
    assert persisted_reports


def test_readme_mentions_only_config_json():
    readme = Path("README.md").read_text(encoding="utf-8")

    assert "config.json" in readme
    assert "executor_config.json" not in readme
    assert "received" in readme
    assert "/health" in readme
    assert "/metrics" in readme


def test_config_cache_manager_tracks_active_unified_config_instance(tmp_path: Path):
    from core import config_cache_manager as cache_module

    original_instance = getattr(unified_config, "_config_manager_instance", None)
    try:
        config_a = tmp_path / "config-a.json"
        config_b = tmp_path / "config-b.json"
        cache_dir = tmp_path / "cache"

        config_a.write_text(
            json.dumps({"config_cache": {"enabled": False}}),
            encoding="utf-8",
        )
        config_b.write_text(
            json.dumps({"config_cache": {"enabled": True}}),
            encoding="utf-8",
        )

        unified_config._config_manager_instance = unified_config.UnifiedConfigManager(
            str(config_a)
        )
        cache_manager = cache_module.ConfigCacheManager(cache_dir=str(cache_dir))
        assert cache_manager.enabled is False

        unified_config._config_manager_instance = unified_config.UnifiedConfigManager(
            str(config_b)
        )

        assert cache_manager.enabled is True
    finally:
        unified_config._config_manager_instance = original_instance
