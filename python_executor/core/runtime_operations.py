from __future__ import annotations

import json
import os
from dataclasses import asdict, dataclass, field
from datetime import datetime
from pathlib import Path
from typing import Any, Callable
from urllib.parse import urlparse

from config.settings import get_config as get_runtime_config
from core.execution_observability import get_execution_observability_manager
from core.failed_report_manager import get_failed_report_manager
from core.status_monitor import get_status_monitor
from core.task_executor_production import get_task_executor
from core.task_scheduler import task_scheduler
from models.executor_task import task_queue
from utils.metrics import build_business_metrics_summary


RUNTIME_STATUS_READY = "ready"
RUNTIME_STATUS_WARNING = "warning"
RUNTIME_STATUS_BLOCKED = "blocked"


@dataclass(slots=True)
class RuntimeOperationItem:
    check_id: str
    name: str
    status: str
    message: str
    details: dict[str, Any] = field(default_factory=dict)

    def to_dict(self) -> dict[str, Any]:
        return asdict(self)


@dataclass(slots=True)
class PreflightReport:
    status: str
    items: list[RuntimeOperationItem]
    generated_at: str = field(default_factory=lambda: datetime.now().isoformat())

    @property
    def summary(self) -> dict[str, int]:
        counts = {
            RUNTIME_STATUS_READY: 0,
            RUNTIME_STATUS_WARNING: 0,
            RUNTIME_STATUS_BLOCKED: 0,
            "total": len(self.items),
        }
        for item in self.items:
            counts[item.status] = counts.get(item.status, 0) + 1
        return counts

    def to_dict(self) -> dict[str, Any]:
        return {
            "status": self.status,
            "generatedAt": self.generated_at,
            "summary": self.summary,
            "items": [item.to_dict() for item in self.items],
        }


class PreflightChecker:
    def __init__(
        self,
        *,
        config_manager=None,
        failed_report_manager=None,
        status_monitor=None,
        data_dir: str | Path | None = None,
        business_metrics_provider: Callable[[], dict[str, Any]] | None = None,
    ):
        self.config_manager = config_manager or get_runtime_config()
        self.failed_report_manager = failed_report_manager or get_failed_report_manager(self.config_manager)
        self.status_monitor = status_monitor or get_status_monitor()
        self.data_dir = Path(data_dir) if data_dir else Path(__file__).resolve().parents[1] / "data"
        self.business_metrics_provider = (
            business_metrics_provider
            or (lambda: build_business_metrics_summary(get_execution_observability_manager().get_business_summary()))
        )

    def run(self) -> PreflightReport:
        items = [
            self._check_config(),
            self._check_directories(),
            self._check_reporting(),
            self._check_adapter_readiness(),
            self._check_failed_reports(),
        ]
        return PreflightReport(status=self._derive_overall_status(items), items=items)

    def _derive_overall_status(self, items: list[RuntimeOperationItem]) -> str:
        statuses = {item.status for item in items}
        if RUNTIME_STATUS_BLOCKED in statuses:
            return RUNTIME_STATUS_BLOCKED
        if RUNTIME_STATUS_WARNING in statuses:
            return RUNTIME_STATUS_WARNING
        return RUNTIME_STATUS_READY

    def _check_config(self) -> RuntimeOperationItem:
        errors = list(self.config_manager.validate_config())
        reload_error = getattr(self.config_manager, "last_reload_error", None)
        if reload_error:
            errors.append(f"reload_error: {reload_error}")

        if errors:
            return RuntimeOperationItem(
                check_id="config",
                name="配置检查",
                status=RUNTIME_STATUS_BLOCKED,
                message="运行配置存在阻塞问题",
                details={"errors": errors},
            )

        return RuntimeOperationItem(
            check_id="config",
            name="配置检查",
            status=RUNTIME_STATUS_READY,
            message="运行配置有效",
            details={"errors": []},
        )

    def _check_directories(self) -> RuntimeOperationItem:
        log_dir = Path(self.config_manager.get("logging.log_dir", "logs"))
        data_dir = Path(self.data_dir)
        checked: list[dict[str, Any]] = []
        failures: list[str] = []

        for name, path in (("logs", log_dir), ("data", data_dir)):
            ok, error = self._check_writable_directory(path)
            checked.append({"name": name, "path": str(path), "writable": ok})
            if not ok:
                failures.append(f"{name}: {error}")

        if failures:
            return RuntimeOperationItem(
                check_id="filesystem",
                name="目录可写检查",
                status=RUNTIME_STATUS_BLOCKED,
                message="关键目录不可写",
                details={"checked": checked, "errors": failures},
            )

        return RuntimeOperationItem(
            check_id="filesystem",
            name="目录可写检查",
            status=RUNTIME_STATUS_READY,
            message="关键目录可写",
            details={"checked": checked},
        )

    def _check_writable_directory(self, path: Path) -> tuple[bool, str | None]:
        try:
            if not path.exists():
                return False, "directory does not exist"
            if not path.is_dir():
                return False, "path is not a directory"
            probe = path / ".codex-write-check"
            probe.write_text("ok", encoding="utf-8")
            probe.unlink()
            return True, None
        except Exception as exc:
            return False, str(exc)

    def _check_reporting(self) -> RuntimeOperationItem:
        enabled = bool(self.config_manager.get("report.enabled", False))
        result_api_url = self.config_manager.get("report.result_api_url", "")
        upload_url = self.config_manager.get("report.file_upload_url", "")
        details = {
            "enabled": enabled,
            "resultApiUrl": result_api_url,
            "fileUploadUrl": upload_url,
        }

        if not enabled:
            return RuntimeOperationItem(
                check_id="reporting",
                name="上报链路检查",
                status=RUNTIME_STATUS_READY,
                message="上报功能未启用",
                details=details,
            )

        if not self._is_valid_url(result_api_url):
            return RuntimeOperationItem(
                check_id="reporting",
                name="上报链路检查",
                status=RUNTIME_STATUS_BLOCKED,
                message="结果上报地址缺失或无效",
                details=details,
            )

        if upload_url and not self._is_valid_url(upload_url):
            return RuntimeOperationItem(
                check_id="reporting",
                name="上报链路检查",
                status=RUNTIME_STATUS_WARNING,
                message="文件上传地址无效",
                details=details,
            )

        return RuntimeOperationItem(
            check_id="reporting",
            name="上报链路检查",
            status=RUNTIME_STATUS_READY,
            message="上报链路配置有效",
            details=details,
        )

    def _is_valid_url(self, value: str) -> bool:
        if not value:
            return False
        parsed = urlparse(value)
        return bool(parsed.scheme and parsed.netloc)

    def _check_adapter_readiness(self) -> RuntimeOperationItem:
        software = self.status_monitor.get_software_status()
        ready_tools = [name for name, item in software.items() if item.get("ready")]

        if not ready_tools:
            return RuntimeOperationItem(
                check_id="adapters",
                name="适配器能力检查",
                status=RUNTIME_STATUS_WARNING,
                message="未检测到处于 ready 状态的测试工具",
                details={"software": software},
            )

        return RuntimeOperationItem(
            check_id="adapters",
            name="适配器能力检查",
            status=RUNTIME_STATUS_READY,
            message=f"可用工具: {', '.join(ready_tools)}",
            details={"software": software, "readyTools": ready_tools},
        )

    def _check_failed_reports(self) -> RuntimeOperationItem:
        stats = self.failed_report_manager.get_statistics()
        pending = int(stats.get("pending", 0))
        failed = int(stats.get("failed", 0))

        if pending or failed:
            return RuntimeOperationItem(
                check_id="failed_reports",
                name="失败报告堆积检查",
                status=RUNTIME_STATUS_WARNING,
                message="存在待重试或最终失败的报告",
                details=stats,
            )

        return RuntimeOperationItem(
            check_id="failed_reports",
            name="失败报告堆积检查",
            status=RUNTIME_STATUS_READY,
            message="失败报告队列干净",
            details=stats,
        )


class RuntimeDiagnoseService:
    def __init__(
        self,
        *,
        config_manager=None,
        task_executor=None,
        task_scheduler=None,
        task_scheduler_instance=None,
        task_queue=None,
        task_queue_instance=None,
        status_monitor=None,
        failed_report_manager=None,
        business_metrics_provider: Callable[[], dict[str, Any]] | None = None,
        observability_manager=None,
    ):
        self.config_manager = config_manager or get_runtime_config()
        self.task_executor = task_executor
        self.task_scheduler = task_scheduler_instance or task_scheduler or globals()["task_scheduler"]
        self.task_queue = task_queue_instance or task_queue or globals()["task_queue"]
        self.status_monitor = status_monitor or get_status_monitor()
        self.failed_report_manager = failed_report_manager or get_failed_report_manager(self.config_manager)
        self.observability_manager = observability_manager or get_execution_observability_manager()
        self.business_metrics_provider = (
            business_metrics_provider
            or (lambda: build_business_metrics_summary(self.observability_manager.get_business_summary()))
        )

    def build_snapshot(self) -> dict[str, Any]:
        executor_stats = self._get_executor_stats()
        services = {
            "executor": executor_stats,
            "scheduler": self.task_scheduler.get_stats(),
        }
        queues = self.task_queue.get_stats()
        failed_reports = self.failed_report_manager.get_statistics()
        software = self.status_monitor.get_software_status()
        business_metrics = self.business_metrics_provider()
        observability = self.observability_manager.get_business_summary()
        failed_report_traceability = self.failed_report_manager.get_trace_context_summary()

        status = RUNTIME_STATUS_READY
        ready_tools = [name for name, item in software.items() if item.get("ready")]
        if not services["scheduler"].get("running", False):
            status = RUNTIME_STATUS_BLOCKED
        elif not ready_tools:
            status = RUNTIME_STATUS_BLOCKED
        elif failed_reports.get("failed", 0) or business_metrics.get("recent_failed_count", 0):
            status = RUNTIME_STATUS_WARNING

        return {
            "status": status,
            "generatedAt": datetime.now().isoformat(),
            "config": {
                "httpPort": self.config_manager.get("http.port"),
                "reportEnabled": self.config_manager.get("report.enabled", False),
                "reportEndpoint": self.config_manager.get("report.result_api_url", ""),
            },
            "services": services,
            "queues": queues,
            "failed_reports": failed_reports,
            "software": software,
            "business_metrics": business_metrics,
            "observability": observability,
            "failed_report_traceability": failed_report_traceability,
            "status_monitor": self.status_monitor.get_all_status(),
        }

    def _get_executor_stats(self) -> dict[str, Any]:
        if self.task_executor is None:
            return {
                "status": "not_initialized",
                "running_count": 0,
                "queue_size": 0,
            }
        try:
            stats = self.task_executor.get_stats()
            if "status" not in stats:
                stats = {"status": "running", **stats}
            return stats
        except Exception as exc:
            return {
                "status": "error",
                "error": str(exc),
                "running_count": 0,
                "queue_size": 0,
            }


_preflight_checker: PreflightChecker | None = None
_diagnose_service: RuntimeDiagnoseService | None = None
_housekeeping_service: "RuntimeHousekeepingService" | None = None


class RuntimeHousekeepingService:
    def __init__(self, *, config_manager=None, failed_report_manager=None, data_dir: str | Path | None = None):
        self.config_manager = config_manager or get_runtime_config()
        self.failed_report_manager = failed_report_manager or get_failed_report_manager(self.config_manager)
        self.data_dir = Path(data_dir) if data_dir else Path(__file__).resolve().parents[1] / "data"

    def run(self) -> dict[str, Any]:
        log_dir = Path(self.config_manager.get("logging.log_dir", "logs"))
        ensured = [str(log_dir), str(self.data_dir)]
        for path in (log_dir, self.data_dir):
            path.mkdir(parents=True, exist_ok=True)

        cleaned = self.failed_report_manager.cleanup_old_reports(days=7)

        return {
            "status": RUNTIME_STATUS_READY,
            "generatedAt": datetime.now().isoformat(),
            "ensuredDirectories": ensured,
            "cleanedFailedReports": cleaned,
        }


def get_preflight_checker() -> PreflightChecker:
    global _preflight_checker
    if _preflight_checker is None:
        _preflight_checker = PreflightChecker()
    return _preflight_checker


def get_runtime_diagnose_service() -> RuntimeDiagnoseService:
    global _diagnose_service
    if _diagnose_service is None:
        _diagnose_service = RuntimeDiagnoseService()
    return _diagnose_service


def get_runtime_housekeeping_service() -> RuntimeHousekeepingService:
    global _housekeeping_service
    if _housekeeping_service is None:
        _housekeeping_service = RuntimeHousekeepingService()
    return _housekeeping_service


def dump_runtime_diagnose_json() -> str:
    return json.dumps(get_runtime_diagnose_service().build_snapshot(), ensure_ascii=False, indent=2)
