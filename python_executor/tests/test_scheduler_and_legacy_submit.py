from __future__ import annotations

from datetime import datetime, timedelta


def test_process_queue_skips_scheduled_tasks_until_due(monkeypatch):
    from core.task_scheduler import TaskScheduler
    from models.executor_task import Task, TaskPriority

    scheduler = TaskScheduler()
    task = Task(id="scheduled-1", name="scheduled", priority=TaskPriority.NORMAL.value)
    scheduler._scheduled_tasks[task.id] = datetime.now() + timedelta(seconds=60)

    submitted = []

    monkeypatch.setattr("core.task_scheduler.task_queue.get_pending_tasks", lambda: [task])
    monkeypatch.setattr("core.task_scheduler.task_executor.get_running_count", lambda: 0)
    monkeypatch.setattr("core.task_scheduler.task_executor.submit_task", lambda task: submitted.append(task) or True)

    scheduler._process_queue()

    assert submitted == []


def test_process_scheduled_tasks_submits_ready_tasks(monkeypatch):
    from core.task_scheduler import TaskScheduler
    from models.executor_task import Task, TaskStatus

    scheduler = TaskScheduler()
    task = Task(id="scheduled-2", name="scheduled-ready", status=TaskStatus.PENDING.value)
    scheduler._scheduled_tasks[task.id] = datetime.now() - timedelta(seconds=1)

    submitted = []

    monkeypatch.setattr("core.task_scheduler.task_queue.get_task", lambda task_id: task if task_id == task.id else None)
    monkeypatch.setattr("core.task_scheduler.task_executor.submit_task", lambda item: submitted.append(item) or True)
    monkeypatch.setattr("core.task_scheduler.task_log_manager.info", lambda *args, **kwargs: None)

    scheduler._process_scheduled_tasks()

    assert submitted == [task]
    assert scheduler._scheduled_tasks == {}


def test_submit_task_rejects_legacy_tasks_without_execution_cases(monkeypatch):
    from core.task_executor_production import TaskExecutorProduction
    from models.executor_task import Task, TaskPriority

    executor = TaskExecutorProduction(message_sender=lambda _: None)
    global_add_calls = []

    monkeypatch.setattr(
        "core.task_executor_production.global_task_queue.add",
        lambda task, overwrite=True: global_add_calls.append(task) or True,
    )

    legacy_task = Task(
        id="legacy-empty",
        name="legacy-empty",
        priority=TaskPriority.NORMAL.value,
        params={"tool_type": "canoe", "config_path": "D:/cfgs/demo.cfg"},
        timeout=30,
    )

    assert executor.submit_task(legacy_task) is False
    assert global_add_calls == []
    assert executor._task_queue.get_queue_size() == 0


def test_cancel_task_can_remove_queued_execution_plan(monkeypatch):
    from core.execution_plan import ExecutionPlan, PlannedCase
    from core.task_executor_production import TaskExecutorProduction

    executor = TaskExecutorProduction(message_sender=lambda _: None)
    status_updates = []

    monkeypatch.setattr(
        "core.task_executor_production.global_task_queue.add",
        lambda task, overwrite=True: True,
    )
    monkeypatch.setattr(
        "core.task_executor_production.global_task_queue.update_task_status",
        lambda task_id, status, error_message=None, result=None: status_updates.append((task_id, status, error_message)) or True,
    )

    plan = ExecutionPlan(
        task_no="queued-plan",
        device_id="DEVICE-Q",
        tool_type="canoe",
        timeout_seconds=30,
        cases=[PlannedCase(case_no="CASE-Q", case_name="Case Q", case_type="test_module")],
    )

    assert executor.execute_plan(plan) is True
    assert executor._task_queue.get_queue_size() == 1

    assert executor.cancel_task("queued-plan") is True
    assert executor._task_queue.get_queue_size() == 0
    assert status_updates[-1] == ("queued-plan", "cancelled", "任务被用户取消")


def test_process_queue_does_not_resubmit_pending_task_already_in_executor(monkeypatch):
    from core.task_scheduler import TaskScheduler
    from models.executor_task import Task, TaskPriority

    scheduler = TaskScheduler()
    task = Task(id="pending-dup", name="pending-dup", priority=TaskPriority.NORMAL.value)
    submitted = []

    monkeypatch.setattr("core.task_scheduler.task_queue.get_pending_tasks", lambda: [task])
    monkeypatch.setattr("core.task_scheduler.task_executor.get_running_count", lambda: 0)
    monkeypatch.setattr(
        "core.task_scheduler.task_executor.get_stats",
        lambda: {"running": 0, "queue_size": 1, "max_workers": 1, "is_running": True},
    )
    monkeypatch.setattr("core.task_scheduler.task_executor.submit_task", lambda item: submitted.append(item) or True)

    scheduler._process_queue()

    assert submitted == []


def test_cancel_scheduled_task_removes_pending_task_from_global_queue(monkeypatch):
    from core.task_scheduler import TaskScheduler

    scheduler = TaskScheduler()
    scheduler._scheduled_tasks["scheduled-cancel"] = datetime.now() + timedelta(seconds=30)
    removed = []

    monkeypatch.setattr("core.task_scheduler.task_queue.remove", lambda task_id: removed.append(task_id) or True)
    monkeypatch.setattr("core.task_scheduler.task_executor.cancel_task", lambda task_id: False)

    assert scheduler.cancel_scheduled_task("scheduled-cancel") is True
    assert removed == ["scheduled-cancel"]
    assert "scheduled-cancel" not in scheduler._scheduled_tasks


def test_submit_task_fails_when_internal_execution_queue_rejects_plan(monkeypatch):
    from core.task_executor_production import TaskExecutorProduction
    from models.executor_task import Task, TaskPriority

    executor = TaskExecutorProduction(message_sender=lambda _: None)
    global_add_calls = []
    removed = []

    monkeypatch.setattr(
        "core.task_executor_production.global_task_queue.add",
        lambda task, overwrite=True: global_add_calls.append(task) or True,
    )
    monkeypatch.setattr(
        executor._task_queue,
        "put",
        lambda plan: False,
    )
    monkeypatch.setattr(
        "core.task_executor_production.global_task_queue.remove",
        lambda task_id: removed.append(task_id) or True,
    )

    legacy_task = Task(
        id="legacy-queue-full",
        name="legacy-queue-full",
        priority=TaskPriority.NORMAL.value,
        params={
            "tool_type": "canoe",
            "config_path": "D:/cfgs/demo.cfg",
            "testItems": [{"caseNo": "CASE-1", "name": "Case 1", "type": "test_module"}],
        },
        timeout=30,
    )

    assert executor.submit_task(legacy_task) is False
    assert len(global_add_calls) == 1
    assert removed == ["legacy-queue-full"]
