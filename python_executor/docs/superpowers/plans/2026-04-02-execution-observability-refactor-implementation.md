# Execution Observability Refactor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a state-machine-aware observability layer so task lifecycle, structured logs, metrics, and monitoring endpoints expose one coherent view of execution health.

**Architecture:** Introduce a lightweight execution observability core that tracks per-task execution context and lifecycle transitions, then wire it into dispatch and production execution without changing task protocol or adapter interfaces. Extend the existing logger and metrics modules so they consume the same execution context and surface business-health summaries through `/health`, `/status`, and `/metrics`.

**Tech Stack:** Python 3.10, dataclasses, stdlib logging, Flask, pytest.

---

## File Structure

**Observability core**
- Create: `core/execution_observability.py`
  - Owns execution context, lifecycle stages, transition helpers, snapshot state, and business summary helpers.

**Runtime integration**
- Modify: `main_production.py`
  - Creates task observability contexts at dispatch time and exposes health/status/metrics summaries.
- Modify: `core/task_executor_production.py`
  - Emits lifecycle transitions for prepare/execute/report/finish/fail paths.

**Logging**
- Modify: `utils/logger.py`
  - Adds context-aware logging helpers and ensures in-memory log entries retain structured task fields.

**Metrics**
- Modify: `utils/metrics.py`
  - Adds business metric recording helpers and aggregate summary functions used by observability endpoints.

**Tests**
- Create: `tests/test_execution_observability.py`
  - Covers execution context transitions and failure semantics.
- Modify: `tests/test_task_dispatch_production.py`
  - Verifies dispatch-stage lifecycle transitions.
- Modify: `tests/test_logger_manager.py`
  - Verifies task context fields are preserved in in-memory logging.
- Modify: `tests/test_config_runtime_integration.py`
  - Extends monitoring endpoint coverage for new health/metrics summaries.

**Docs**
- Modify: `README.md`
  - Documents lifecycle observability and monitoring endpoint behavior at a high level.

### Task 1: Build The Execution Observability Core

**Files:**
- Create: `core/execution_observability.py`
- Test: `tests/test_execution_observability.py`

- [ ] **Step 1: Write the failing tests**

```python
from core.execution_observability import (
    ExecutionLifecycleStage,
    ExecutionObservabilityManager,
)


def test_lifecycle_transitions_update_snapshot_and_stage_history():
    manager = ExecutionObservabilityManager()

    context = manager.create_context(
        task_no="TASK-1",
        device_id="DEVICE-1",
        tool_type="canoe",
    )
    manager.transition("TASK-1", ExecutionLifecycleStage.VALIDATED)
    manager.transition("TASK-1", ExecutionLifecycleStage.QUEUED)

    snapshot = manager.get_snapshot("TASK-1")

    assert context.current_stage == ExecutionLifecycleStage.QUEUED.value
    assert snapshot["task_no"] == "TASK-1"
    assert snapshot["stage_history"] == ["received", "validated", "queued"]


def test_fail_marks_stage_error_and_retryability():
    manager = ExecutionObservabilityManager()

    manager.create_context(task_no="TASK-2", device_id="DEVICE-2", tool_type="tsmaster")
    manager.transition("TASK-2", ExecutionLifecycleStage.EXECUTING)
    manager.fail(
        "TASK-2",
        error_code="EXEC_TIMEOUT",
        error_message="execution timed out",
        retryable=True,
    )

    snapshot = manager.get_snapshot("TASK-2")

    assert snapshot["current_stage"] == "finished"
    assert snapshot["failed_stage"] == "executing"
    assert snapshot["error_code"] == "EXEC_TIMEOUT"
    assert snapshot["retryable"] is True


def test_business_summary_counts_queue_active_and_failures():
    manager = ExecutionObservabilityManager()

    manager.create_context(task_no="TASK-A", device_id="D1", tool_type="canoe")
    manager.transition("TASK-A", ExecutionLifecycleStage.QUEUED)
    manager.create_context(task_no="TASK-B", device_id="D2", tool_type="tsmaster")
    manager.transition("TASK-B", ExecutionLifecycleStage.EXECUTING)
    manager.create_context(task_no="TASK-C", device_id="D3", tool_type="canoe")
    manager.fail("TASK-C", error_code="VALIDATION", error_message="bad input")

    summary = manager.get_business_summary()

    assert summary["queued_count"] == 1
    assert summary["active_count"] == 1
    assert summary["recent_failed_count"] == 1
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m pytest tests/test_execution_observability.py -q`  
Expected: FAIL with `ModuleNotFoundError: No module named 'core.execution_observability'`

- [ ] **Step 3: Write minimal implementation**

```python
from __future__ import annotations

import threading
import time
from copy import deepcopy
from dataclasses import dataclass, field
from enum import Enum
from typing import Any


class ExecutionLifecycleStage(str, Enum):
    RECEIVED = "received"
    VALIDATED = "validated"
    QUEUED = "queued"
    PREPARING = "preparing"
    EXECUTING = "executing"
    REPORTING = "reporting"
    FINISHED = "finished"


@dataclass
class ExecutionContext:
    task_no: str
    device_id: str
    tool_type: str
    current_stage: str = ExecutionLifecycleStage.RECEIVED.value
    attempt: int = 1
    error_code: str | None = None
    error_message: str | None = None
    failed_stage: str | None = None
    retryable: bool = False
    report_status: str | None = None
    started_at: float = field(default_factory=time.time)
    last_transition_at: float = field(default_factory=time.time)
    stage_history: list[str] = field(default_factory=lambda: [ExecutionLifecycleStage.RECEIVED.value])
    stage_durations: dict[str, float] = field(default_factory=dict)


class ExecutionObservabilityManager:
    def __init__(self):
        self._lock = threading.RLock()
        self._contexts: dict[str, ExecutionContext] = {}

    def create_context(self, task_no: str, device_id: str, tool_type: str) -> ExecutionContext:
        context = ExecutionContext(task_no=task_no, device_id=device_id, tool_type=tool_type)
        with self._lock:
            self._contexts[task_no] = context
        return context

    def transition(self, task_no: str, stage: ExecutionLifecycleStage) -> ExecutionContext:
        with self._lock:
            context = self._contexts[task_no]
            now = time.time()
            previous_stage = context.current_stage
            context.stage_durations.setdefault(previous_stage, 0.0)
            context.stage_durations[previous_stage] += max(0.0, now - context.last_transition_at)
            context.current_stage = stage.value
            context.last_transition_at = now
            context.stage_history.append(stage.value)
            return context

    def fail(
        self,
        task_no: str,
        *,
        error_code: str,
        error_message: str,
        retryable: bool = False,
    ) -> ExecutionContext:
        with self._lock:
            context = self._contexts[task_no]
            context.failed_stage = context.current_stage
            context.error_code = error_code
            context.error_message = error_message
            context.retryable = retryable
        return self.transition(task_no, ExecutionLifecycleStage.FINISHED)

    def finish(self, task_no: str, report_status: str | None = None) -> ExecutionContext:
        with self._lock:
            context = self._contexts[task_no]
            context.report_status = report_status
        return self.transition(task_no, ExecutionLifecycleStage.FINISHED)

    def get_snapshot(self, task_no: str) -> dict[str, Any]:
        with self._lock:
            context = self._contexts[task_no]
            return deepcopy(context.__dict__)

    def get_business_summary(self) -> dict[str, Any]:
        with self._lock:
            contexts = list(self._contexts.values())

        queued_count = sum(1 for context in contexts if context.current_stage == ExecutionLifecycleStage.QUEUED.value)
        active_count = sum(
            1
            for context in contexts
            if context.current_stage in {
                ExecutionLifecycleStage.PREPARING.value,
                ExecutionLifecycleStage.EXECUTING.value,
                ExecutionLifecycleStage.REPORTING.value,
            }
        )
        recent_failed_count = sum(1 for context in contexts if context.error_code)

        return {
            "queued_count": queued_count,
            "active_count": active_count,
            "recent_failed_count": recent_failed_count,
            "current_tasks": [deepcopy(context.__dict__) for context in contexts],
        }


_execution_observability_manager: ExecutionObservabilityManager | None = None


def get_execution_observability_manager() -> ExecutionObservabilityManager:
    global _execution_observability_manager
    if _execution_observability_manager is None:
        _execution_observability_manager = ExecutionObservabilityManager()
    return _execution_observability_manager
```

- [ ] **Step 4: Run test to verify it passes**

Run: `python -m pytest tests/test_execution_observability.py -q`  
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add core/execution_observability.py tests/test_execution_observability.py
git commit -m "feat: add execution observability core"
```

### Task 2: Add Context-Aware Structured Logging

**Files:**
- Modify: `utils/logger.py`
- Modify: `tests/test_logger_manager.py`

- [ ] **Step 1: Write the failing tests**

```python
from utils.logger import logger_manager


def test_memory_logs_include_task_context_fields():
    logger_manager.clear_memory_logs()
    logger = logger_manager.get_logger("structured")

    logger.info(
        "task moved to executing",
        extra={
            "task_no": "TASK-100",
            "device_id": "DEVICE-9",
            "tool_type": "canoe",
            "stage": "executing",
            "attempt": 1,
            "error_code": None,
        },
    )

    logs = logger_manager.get_memory_logs(limit=1)

    assert logs[0]["task_no"] == "TASK-100"
    assert logs[0]["device_id"] == "DEVICE-9"
    assert logs[0]["tool_type"] == "canoe"
    assert logs[0]["stage"] == "executing"


def test_bind_task_context_returns_logger_adapter():
    adapter = logger_manager.bind_task_context(
        logger_manager.get_logger("structured"),
        task_no="TASK-101",
        device_id="DEVICE-10",
        tool_type="tsmaster",
        stage="preparing",
    )

    adapter.info("preparing started")
    logs = logger_manager.get_memory_logs(limit=1)

    assert logs[0]["task_no"] == "TASK-101"
    assert logs[0]["stage"] == "preparing"
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m pytest tests/test_logger_manager.py -q`  
Expected: FAIL because log entries do not yet preserve `task_no`/`stage` fields and `bind_task_context` does not exist

- [ ] **Step 3: Write minimal implementation**

```python
import logging
from typing import Any


class TaskContextAdapter(logging.LoggerAdapter):
    def process(self, msg, kwargs):
        extra = dict(self.extra)
        extra.update(kwargs.get("extra", {}))
        kwargs["extra"] = extra
        return msg, kwargs


class MemoryLogHandler(logging.Handler):
    def emit(self, record):
        log_entry = {
            "timestamp": datetime.fromtimestamp(record.created).strftime("%Y-%m-%d %H:%M:%S"),
            "level": record.levelname,
            "message": self.format(record),
            "levelno": record.levelno,
            "task_no": getattr(record, "task_no", None),
            "device_id": getattr(record, "device_id", None),
            "tool_type": getattr(record, "tool_type", None),
            "stage": getattr(record, "stage", None),
            "attempt": getattr(record, "attempt", None),
            "error_code": getattr(record, "error_code", None),
        }
        self.log_entries.append(log_entry)


class LoggerManager:
    def bind_task_context(self, logger: logging.Logger, **context: Any) -> TaskContextAdapter:
        normalized = {
            "task_no": context.get("task_no"),
            "device_id": context.get("device_id"),
            "tool_type": context.get("tool_type"),
            "stage": context.get("stage"),
            "attempt": context.get("attempt"),
            "error_code": context.get("error_code"),
        }
        return TaskContextAdapter(logger, normalized)
```

- [ ] **Step 4: Run test to verify it passes**

Run: `python -m pytest tests/test_logger_manager.py -q`  
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add utils/logger.py tests/test_logger_manager.py
git commit -m "feat: add structured task logging context"
```

### Task 3: Wire Lifecycle, Metrics, And Monitoring Endpoints

**Files:**
- Modify: `main_production.py`
- Modify: `core/task_executor_production.py`
- Modify: `utils/metrics.py`
- Modify: `tests/test_task_dispatch_production.py`
- Modify: `tests/test_config_runtime_integration.py`

- [ ] **Step 1: Write the failing tests**

```python
from core.execution_observability import get_execution_observability_manager


def test_dispatch_creates_received_validated_and_queued_lifecycle(dispatcher):
    dispatcher.fake_manager._mappings = {
        "case-1": _FakeMapping(category="CANOE", case_name="case-1"),
    }

    message = Message(
        type="TASK_DISPATCH",
        taskNo="task-observed",
        deviceId="device-1",
        payload={"testItems": [{"case_no": "case-1", "name": "case-1"}], "timeout": 30},
    )

    dispatcher.executor._handle_task_dispatch(message, "sid-1")
    snapshot = get_execution_observability_manager().get_snapshot("task-observed")

    assert snapshot["stage_history"][:3] == ["received", "validated", "queued"]


def test_metrics_endpoint_includes_business_summary(runtime_modules, monkeypatch):
    main_module = runtime_modules["main_module"]
    observability = __import__("core.execution_observability", fromlist=["get_execution_observability_manager"])
    manager = observability.get_execution_observability_manager()
    manager.create_context(task_no="TASK-M", device_id="DEVICE-M", tool_type="canoe")
    manager.transition("TASK-M", observability.ExecutionLifecycleStage.QUEUED)

    monkeypatch.setattr(main_module.signal, "signal", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_module.config_manager, "start_watcher", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_module.performance_monitor, "start", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_module.performance_monitor, "set_alert_threshold", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_module.performance_monitor, "register_alert_callback", lambda *args, **kwargs: None)

    executor = main_module.PythonExecutorProduction()
    response = executor.app.test_client().get("/metrics")

    assert response.status_code == 200
    payload = response.get_json()
    assert payload["business_summary"]["queued_count"] == 1
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m pytest tests/test_task_dispatch_production.py tests/test_config_runtime_integration.py -q`  
Expected: FAIL because lifecycle snapshots and business summaries are not yet exposed

- [ ] **Step 3: Write minimal implementation**

```python
# utils/metrics.py
from collections import Counter


class MetricCollector:
    def get_metric_count(self, metric_name: str) -> int:
        with self._lock:
            return len(self.metrics.get(metric_name, []))


def build_business_metrics_summary(
    observability_summary: dict[str, Any],
    collector: MetricCollector,
) -> dict[str, Any]:
    return {
        "queued_count": observability_summary["queued_count"],
        "active_count": observability_summary["active_count"],
        "recent_failed_count": observability_summary["recent_failed_count"],
        "report_success_count": collector.get_metric_count("task.report.success"),
        "report_failure_count": collector.get_metric_count("task.report.failure"),
    }
```

```python
# main_production.py
from core.execution_observability import (
    ExecutionLifecycleStage,
    get_execution_observability_manager,
)
from utils.metrics import build_business_metrics_summary, record_metric


observability = get_execution_observability_manager()
context = observability.create_context(
    task_no=message.taskNo,
    device_id=message.deviceId or "",
    tool_type=str(validated_data.get("toolType", "")),
)
record_metric("task.received", 1, {"task_no": message.taskNo})
observability.transition(message.taskNo, ExecutionLifecycleStage.VALIDATED)
record_metric("task.validated", 1, {"task_no": message.taskNo})
observability.transition(message.taskNo, ExecutionLifecycleStage.QUEUED)
record_metric("task.queued", 1, {"task_no": message.taskNo})

metrics_result["business_summary"] = build_business_metrics_summary(
    get_execution_observability_manager().get_business_summary(),
    metric_collector,
)
```

```python
# core/task_executor_production.py
from core.execution_observability import (
    ExecutionLifecycleStage,
    get_execution_observability_manager,
)


observability = get_execution_observability_manager()
observability.transition(task.task_id, ExecutionLifecycleStage.PREPARING)
record_metric("task.preparing", 1, {"task_no": task.task_id, "tool_type": task.tool_type})
observability.transition(task.task_id, ExecutionLifecycleStage.EXECUTING)
record_metric("task.executing", 1, {"task_no": task.task_id, "tool_type": task.tool_type})
observability.transition(task.task_id, ExecutionLifecycleStage.REPORTING)
record_metric("task.reporting", 1, {"task_no": task.task_id, "tool_type": task.tool_type})
observability.finish(task.task_id, report_status="success")
record_metric("task.finished", 1, {"task_no": task.task_id, "status": "success"})
```

- [ ] **Step 4: Run test to verify it passes**

Run: `python -m pytest tests/test_task_dispatch_production.py tests/test_config_runtime_integration.py -q`  
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add main_production.py core/task_executor_production.py utils/metrics.py tests/test_task_dispatch_production.py tests/test_config_runtime_integration.py
git commit -m "feat: wire lifecycle observability into executor"
```

### Task 4: Add Failure Semantics, Health Summaries, And Documentation

**Files:**
- Modify: `main_production.py`
- Modify: `core/task_executor_production.py`
- Modify: `README.md`
- Modify: `tests/test_execution_observability.py`
- Modify: `tests/test_config_runtime_integration.py`

- [ ] **Step 1: Write the failing tests**

```python
def test_health_endpoint_reports_business_health(runtime_modules, monkeypatch):
    main_module = runtime_modules["main_module"]
    observability = __import__("core.execution_observability", fromlist=["get_execution_observability_manager"])
    manager = observability.get_execution_observability_manager()
    manager.create_context(task_no="TASK-H", device_id="DEVICE-H", tool_type="canoe")
    manager.fail("TASK-H", error_code="REPORT_FAIL", error_message="report failed")

    monkeypatch.setattr(main_module.signal, "signal", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_module.config_manager, "start_watcher", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_module.performance_monitor, "start", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_module.performance_monitor, "set_alert_threshold", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_module.performance_monitor, "register_alert_callback", lambda *args, **kwargs: None)

    executor = main_module.PythonExecutorProduction()
    response = executor.app.test_client().get("/health")

    assert response.status_code == 200
    payload = response.get_json()
    assert payload["business_health"]["recent_failed_count"] == 1
    assert payload["business_health"]["healthy"] is False


def test_readme_mentions_lifecycle_observability():
    readme = Path("README.md").read_text(encoding="utf-8")

    assert "received" in readme
    assert "/metrics" in readme
    assert "/health" in readme
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m pytest tests/test_execution_observability.py tests/test_config_runtime_integration.py -q`  
Expected: FAIL because health output and README do not yet expose business lifecycle observability

- [ ] **Step 3: Write minimal implementation**

```python
# main_production.py
business_summary = get_execution_observability_manager().get_business_summary()
health_result["business_health"] = {
    "healthy": business_summary["recent_failed_count"] == 0,
    "queued_count": business_summary["queued_count"],
    "active_count": business_summary["active_count"],
    "recent_failed_count": business_summary["recent_failed_count"],
}

status_result["business_summary"] = business_summary
```

```python
# core/task_executor_production.py
observability = get_execution_observability_manager()
observability.fail(
    task.task_id,
    error_code="TASK_EXCEPTION",
    error_message=str(e),
    retryable=False,
)
record_metric("task.failed", 1, {"task_no": task.task_id, "stage": observability.get_snapshot(task.task_id)["failed_stage"]})
record_metric("task.report.failure", 1, {"task_no": task.task_id})
```

```md
# README.md excerpt
- Execution lifecycle stages: `received -> validated -> queued -> preparing -> executing -> reporting -> finished`
- `/health` exposes business health for queued, active, and recently failed tasks
- `/metrics` exposes both raw metrics and business execution summaries
```

- [ ] **Step 4: Run test to verify it passes**

Run: `python -m pytest tests/test_execution_observability.py tests/test_config_runtime_integration.py -q`  
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add main_production.py core/task_executor_production.py README.md tests/test_execution_observability.py tests/test_config_runtime_integration.py
git commit -m "docs: expose business health observability"
```

### Task 5: Full Verification

**Files:**
- Modify: none
- Test: `tests/test_execution_observability.py`
- Test: `tests/test_task_dispatch_production.py`
- Test: `tests/test_logger_manager.py`
- Test: `tests/test_config_runtime_integration.py`

- [ ] **Step 1: Run targeted observability test suite**

Run: `python -m pytest tests/test_execution_observability.py tests/test_task_dispatch_production.py tests/test_logger_manager.py tests/test_config_runtime_integration.py -q`  
Expected: PASS

- [ ] **Step 2: Run full project test suite**

Run: `python -m pytest -q`  
Expected: PASS with the existing intended skips only

- [ ] **Step 3: Smoke-check monitoring endpoints data shape**

Run: `python -c "from core.execution_observability import get_execution_observability_manager; manager = get_execution_observability_manager(); manager.create_context('SMOKE', 'DEVICE', 'canoe'); print(manager.get_business_summary()['queued_count'])"`  
Expected: `0`

- [ ] **Step 4: Commit verification follow-up if needed**

```bash
git add .
git commit -m "test: verify execution observability refactor"
```

