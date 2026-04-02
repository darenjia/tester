from __future__ import annotations

from pathlib import Path

from flask import Flask


class _FakeConfigManager:
    def __init__(self, *, config: dict, errors: list[str] | None = None, reload_error: str | None = None):
        self._config = config
        self._errors = errors or []
        self.last_reload_error = reload_error

    def get(self, key: str, default=None):
        value = self._config
        for part in key.split("."):
            if isinstance(value, dict) and part in value:
                value = value[part]
            else:
                return default
        return value

    def get_all(self):
        return self._config

    def validate_config(self):
        return list(self._errors)


class _FakeFailedReportManager:
    def __init__(self, stats: dict):
        self._stats = stats
        self.cleaned_days = None

    def get_statistics(self):
        return dict(self._stats)

    def cleanup_old_reports(self, days: int = 7):
        self.cleaned_days = days
        return 2


class _FakeStatusMonitor:
    def __init__(self, software_status: dict | None = None):
        self._software_status = software_status or {
            "canoe": {"ready": True, "status": "ready"},
            "tsmaster": {"ready": False, "status": "not_installed"},
            "ttworkbench": {"ready": False, "status": "not_installed"},
        }

    def get_software_status(self):
        return self._software_status

    def get_all_status(self):
        return {
            "software": self._software_status,
            "task": {"status": "idle", "has_task": False},
            "websocket": {"status": "disconnected"},
            "system": {"cpu_percent": 12.5},
        }


class _FakeTaskExecutor:
    def get_stats(self):
        return {"running_count": 1, "queue_size": 2}


class _FakeScheduler:
    def __init__(self, *, running: bool = True):
        self._running = running

    def get_stats(self):
        return {"running": self._running, "scheduled_count": 3}


class _FakeTaskQueue:
    def get_stats(self):
        return {"pending": 2, "running": 1, "failed": 0}


def test_preflight_checker_reports_warning_when_failed_reports_exist(tmp_path: Path):
    from core.runtime_operations import PreflightChecker

    logs_dir = tmp_path / "logs"
    data_dir = tmp_path / "data"
    logs_dir.mkdir()
    data_dir.mkdir()

    checker = PreflightChecker(
        config_manager=_FakeConfigManager(
            config={
                "logging": {"log_dir": str(logs_dir)},
                "report": {
                    "enabled": True,
                    "result_api_url": "http://report.example/api",
                    "file_upload_url": "http://upload.example/files",
                },
            }
        ),
        failed_report_manager=_FakeFailedReportManager(
            {"pending": 2, "failed": 1, "success": 0, "retrying": 0, "total": 3}
        ),
        status_monitor=_FakeStatusMonitor(),
        data_dir=data_dir,
    )

    report = checker.run()

    assert report.status == "warning"
    assert report.summary["warning"] >= 1
    assert any(item.check_id == "failed_reports" and item.status == "warning" for item in report.items)


def test_preflight_checker_does_not_create_missing_directories(tmp_path: Path):
    from core.runtime_operations import PreflightChecker

    logs_dir = tmp_path / "missing-logs"
    data_dir = tmp_path / "missing-data"

    checker = PreflightChecker(
        config_manager=_FakeConfigManager(
            config={
                "logging": {"log_dir": str(logs_dir)},
                "report": {"enabled": False},
            }
        ),
        failed_report_manager=_FakeFailedReportManager(
            {"pending": 0, "failed": 0, "success": 0, "retrying": 0, "total": 0}
        ),
        status_monitor=_FakeStatusMonitor(),
        data_dir=data_dir,
    )

    report = checker.run()

    assert report.status == "blocked"
    assert not logs_dir.exists()
    assert not data_dir.exists()
    assert any(item.check_id == "filesystem" and item.status == "blocked" for item in report.items)


def test_preflight_checker_reports_blocked_when_config_invalid(tmp_path: Path):
    from core.runtime_operations import PreflightChecker

    logs_dir = tmp_path / "logs"
    data_dir = tmp_path / "data"
    logs_dir.mkdir()
    data_dir.mkdir()

    checker = PreflightChecker(
        config_manager=_FakeConfigManager(
            config={
                "logging": {"log_dir": str(logs_dir)},
                "report": {"enabled": True, "result_api_url": ""},
            },
            errors=["task.timeout must be a positive integer"],
        ),
        failed_report_manager=_FakeFailedReportManager(
            {"pending": 0, "failed": 0, "success": 0, "retrying": 0, "total": 0}
        ),
        status_monitor=_FakeStatusMonitor(),
        data_dir=data_dir,
    )

    report = checker.run()

    assert report.status == "blocked"
    assert report.summary["blocked"] >= 1
    assert any(item.check_id == "config" and item.status == "blocked" for item in report.items)


def test_runtime_diagnose_service_builds_unified_snapshot(tmp_path: Path):
    from core.runtime_operations import RuntimeDiagnoseService

    service = RuntimeDiagnoseService(
        config_manager=_FakeConfigManager(
            config={
                "http": {"port": 8180},
                "report": {"enabled": True, "result_api_url": "http://report.example/api"},
            }
        ),
        task_executor=_FakeTaskExecutor(),
        task_scheduler=_FakeScheduler(),
        task_queue=_FakeTaskQueue(),
        status_monitor=_FakeStatusMonitor(),
        failed_report_manager=_FakeFailedReportManager(
            {"pending": 1, "failed": 2, "success": 3, "retrying": 0, "total": 6}
        ),
        business_metrics_provider=lambda: {"queued_count": 2, "recent_failed_count": 1},
    )

    snapshot = service.build_snapshot()

    assert snapshot["status"] == "warning"
    assert snapshot["services"]["executor"]["running_count"] == 1
    assert snapshot["queues"]["pending"] == 2
    assert snapshot["failed_reports"]["failed"] == 2
    assert snapshot["business_metrics"]["recent_failed_count"] == 1
    assert snapshot["software"]["canoe"]["ready"] is True


def test_runtime_diagnose_service_marks_blocked_when_scheduler_stopped():
    from core.runtime_operations import RuntimeDiagnoseService

    service = RuntimeDiagnoseService(
        config_manager=_FakeConfigManager(config={"http": {"port": 8180}}),
        task_executor=_FakeTaskExecutor(),
        task_scheduler=_FakeScheduler(running=False),
        task_queue=_FakeTaskQueue(),
        status_monitor=_FakeStatusMonitor(
            software_status={
                "canoe": {"ready": False, "status": "not_installed"},
                "tsmaster": {"ready": False, "status": "not_installed"},
                "ttworkbench": {"ready": False, "status": "not_installed"},
            }
        ),
        failed_report_manager=_FakeFailedReportManager(
            {"pending": 0, "failed": 0, "success": 0, "retrying": 0, "total": 0}
        ),
        business_metrics_provider=lambda: {"queued_count": 0, "recent_failed_count": 0},
    )

    snapshot = service.build_snapshot()

    assert snapshot["status"] == "blocked"


def test_runtime_diagnose_service_does_not_bootstrap_global_executor(monkeypatch):
    from core.runtime_operations import RuntimeDiagnoseService

    calls = {"count": 0}

    def _boom():
        calls["count"] += 1
        raise AssertionError("get_task_executor should not be called during diagnose construction")

    monkeypatch.setattr("core.runtime_operations.get_task_executor", _boom)

    service = RuntimeDiagnoseService(
        config_manager=_FakeConfigManager(config={"http": {"port": 8180}}),
        task_scheduler=_FakeScheduler(),
        task_queue=_FakeTaskQueue(),
        status_monitor=_FakeStatusMonitor(),
        failed_report_manager=_FakeFailedReportManager(
            {"pending": 0, "failed": 0, "success": 0, "retrying": 0, "total": 0}
        ),
        business_metrics_provider=lambda: {"queued_count": 0, "recent_failed_count": 0},
    )

    snapshot = service.build_snapshot()

    assert calls["count"] == 0
    assert snapshot["services"]["executor"]["status"] == "not_initialized"


def test_runtime_operations_api_exposes_preflight_and_diagnose(tmp_path: Path):
    from api.runtime_operations_api import runtime_ops_bp
    import api.runtime_operations_api as runtime_api

    app = Flask(__name__)
    app.register_blueprint(runtime_ops_bp)

    logs_dir = tmp_path / "logs"
    data_dir = tmp_path / "data"
    logs_dir.mkdir()
    data_dir.mkdir()

    runtime_api._preflight_checker = None
    runtime_api._diagnose_service = None
    runtime_api.get_preflight_checker = lambda: runtime_api.PreflightChecker(
        config_manager=_FakeConfigManager(
            config={
                "logging": {"log_dir": str(logs_dir)},
                "report": {"enabled": False},
            }
        ),
        failed_report_manager=_FakeFailedReportManager(
            {"pending": 0, "failed": 0, "success": 0, "retrying": 0, "total": 0}
        ),
        status_monitor=_FakeStatusMonitor(),
        data_dir=data_dir,
    )
    runtime_api.get_runtime_diagnose_service = lambda: runtime_api.RuntimeDiagnoseService(
        config_manager=_FakeConfigManager(config={"http": {"port": 8180}}),
        task_executor=_FakeTaskExecutor(),
        task_scheduler=_FakeScheduler(),
        task_queue=_FakeTaskQueue(),
        status_monitor=_FakeStatusMonitor(),
        failed_report_manager=_FakeFailedReportManager(
            {"pending": 0, "failed": 0, "success": 0, "retrying": 0, "total": 0}
        ),
        business_metrics_provider=lambda: {"queued_count": 0, "recent_failed_count": 0},
    )
    runtime_api.get_runtime_housekeeping_service = lambda: runtime_api.RuntimeHousekeepingService(
        config_manager=_FakeConfigManager(config={"logging": {"log_dir": str(logs_dir)}}),
        failed_report_manager=_FakeFailedReportManager(
            {"pending": 0, "failed": 0, "success": 0, "retrying": 0, "total": 0}
        ),
        data_dir=data_dir,
    )

    client = app.test_client()

    preflight_response = client.get("/api/runtime/preflight")
    diagnose_response = client.get("/api/runtime/diagnose")
    housekeeping_response = client.post("/api/runtime/housekeeping")

    assert preflight_response.status_code == 200
    assert preflight_response.get_json()["data"]["status"] == "ready"

    assert diagnose_response.status_code == 200
    assert diagnose_response.get_json()["data"]["services"]["scheduler"]["running"] is True
    assert housekeeping_response.status_code == 200
    assert housekeeping_response.get_json()["data"]["status"] == "ready"


def test_runtime_housekeeping_service_cleans_failed_reports(tmp_path: Path):
    from core.runtime_operations import RuntimeHousekeepingService

    logs_dir = tmp_path / "logs"
    data_dir = tmp_path / "data"
    failed_report_manager = _FakeFailedReportManager(
        {"pending": 0, "failed": 2, "success": 8, "retrying": 0, "total": 10}
    )

    service = RuntimeHousekeepingService(
        config_manager=_FakeConfigManager(
            config={"logging": {"log_dir": str(logs_dir)}}
        ),
        failed_report_manager=failed_report_manager,
        data_dir=data_dir,
    )

    result = service.run()

    assert result["status"] == "ready"
    assert result["cleanedFailedReports"] == 2
    assert failed_report_manager.cleaned_days == 7
    assert logs_dir.exists()
    assert data_dir.exists()


def test_readme_mentions_runtime_operations_endpoints():
    readme = Path("README.md").read_text(encoding="utf-8")

    assert "/api/runtime/preflight" in readme
    assert "/api/runtime/diagnose" in readme
    assert "/api/runtime/housekeeping" in readme
    assert "preflight_check.py" in readme
    assert "runtime_diagnose.py" in readme
    assert "runtime_housekeeping.py" in readme
