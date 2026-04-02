from __future__ import annotations

import copy
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


def test_cancel_scheduled_task_marks_task_cancelled_in_global_queue(monkeypatch):
    from core.task_scheduler import TaskScheduler

    scheduler = TaskScheduler()
    scheduler._scheduled_tasks["scheduled-cancel"] = datetime.now() + timedelta(seconds=30)
    updated = []

    monkeypatch.setattr(
        "core.task_scheduler.task_queue.update_task_status",
        lambda task_id, status, error_message=None, result=None: updated.append((task_id, status, error_message)) or True,
    )
    monkeypatch.setattr("core.task_scheduler.task_executor.cancel_task", lambda task_id: False)

    assert scheduler.cancel_scheduled_task("scheduled-cancel") is True
    assert updated == [("scheduled-cancel", "cancelled", "定时任务已取消")]
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


def test_schedule_periodic_task_requires_started_scheduler(monkeypatch):
    from core.task_scheduler import TaskScheduler
    from models.executor_task import Task

    scheduler = TaskScheduler()
    task = Task(id="periodic-not-running", name="Periodic Not Running")

    monkeypatch.setattr("core.task_scheduler.task_log_manager.info", lambda *args, **kwargs: None)

    assert scheduler.schedule_periodic_task(task, interval=1.0, max_iterations=1) is None


def test_schedule_periodic_task_logs_failed_submissions_without_stopping(monkeypatch):
    from core.task_scheduler import TaskScheduler
    from models.executor_task import Task

    scheduler = TaskScheduler()
    scheduler._running = True
    task = Task(id="periodic-fail", name="Periodic Fail")
    submitted = []
    logs = []
    sleeps = []

    monkeypatch.setattr(
        "core.task_scheduler.task_executor.submit_task",
        lambda new_task: submitted.append(new_task.id) or False,
    )
    monkeypatch.setattr(
        "core.task_scheduler.task_executor._build_execution_plan_from_queue_task",
        lambda task: object(),
    )
    monkeypatch.setattr(
        "core.task_scheduler.task_log_manager.error",
        lambda task_id, message, details=None: logs.append((task_id, message)),
    )

    def _stop_after_two(_seconds):
        sleeps.append(_seconds)
        scheduler._running = False

    monkeypatch.setattr("core.task_scheduler.time.sleep", _stop_after_two)
    monkeypatch.setattr("core.task_scheduler.task_log_manager.info", lambda *args, **kwargs: None)

    scheduler_id = scheduler.schedule_periodic_task(task, interval=0.01, max_iterations=2)

    assert scheduler_id.startswith("periodic_periodic-fail_")
    for _ in range(50):
        if sleeps:
            break
        __import__("time").sleep(0.01)

    assert submitted
    assert logs
    assert logs[0][0] == "periodic-fail"


def test_schedule_task_rolls_back_scheduled_registry_when_queue_add_fails(monkeypatch):
    from core.task_scheduler import TaskScheduler
    from models.executor_task import Task

    scheduler = TaskScheduler()
    task = Task(id="scheduled-add-fail", name="Scheduled Add Fail")

    monkeypatch.setattr("core.task_scheduler.task_queue.add", lambda task: False)

    assert scheduler.schedule_task(task, delay=30) is False
    assert "scheduled-add-fail" not in scheduler._scheduled_tasks


def test_schedule_periodic_task_deep_copies_nested_task_payload(monkeypatch):
    from core.task_scheduler import TaskScheduler
    from models.executor_task import Task

    scheduler = TaskScheduler()
    scheduler._running = True
    source_task = Task(
        id="periodic-deepcopy",
        name="Periodic Deepcopy",
        params={"variables": {"count": 1}, "list": [{"value": "a"}]},
        metadata={"nested": {"flag": True}},
    )
    captured = []

    def _submit(new_task):
        new_task.params["variables"]["count"] = 99
        new_task.params["list"][0]["value"] = "mutated"
        new_task.metadata["nested"]["flag"] = False
        captured.append(copy.deepcopy(new_task.to_dict()))
        scheduler._running = False
        return True

    monkeypatch.setattr(
        "core.task_scheduler.task_executor._build_execution_plan_from_queue_task",
        lambda task: object(),
    )
    monkeypatch.setattr("core.task_scheduler.task_executor.submit_task", _submit)
    monkeypatch.setattr("core.task_scheduler.task_log_manager.info", lambda *args, **kwargs: None)
    monkeypatch.setattr("core.task_scheduler.task_log_manager.error", lambda *args, **kwargs: None)
    monkeypatch.setattr("core.task_scheduler.time.sleep", lambda _seconds: None)

    scheduler.schedule_periodic_task(source_task, interval=0.01, max_iterations=1)

    for _ in range(50):
        if captured:
            break
        __import__("time").sleep(0.01)

    assert captured
    assert source_task.params["variables"]["count"] == 1
    assert source_task.params["list"][0]["value"] == "a"
    assert source_task.metadata["nested"]["flag"] is True


def test_schedule_periodic_task_registers_scheduler_id(monkeypatch):
    from core.task_scheduler import TaskScheduler
    from models.executor_task import Task

    scheduler = TaskScheduler()
    scheduler._running = True
    task = Task(id="periodic-registered", name="Periodic Registered")
    started = []

    class _FakeThread:
        def __init__(self, target, daemon=True):
            self._target = target

        def start(self):
            started.append(True)

    monkeypatch.setattr(
        "core.task_scheduler.task_executor._build_execution_plan_from_queue_task",
        lambda task: object(),
    )
    monkeypatch.setattr("core.task_scheduler.task_executor.submit_task", lambda new_task: True)
    monkeypatch.setattr("core.task_scheduler.task_log_manager.info", lambda *args, **kwargs: None)
    monkeypatch.setattr("core.task_scheduler.task_log_manager.error", lambda *args, **kwargs: None)
    monkeypatch.setattr("core.task_scheduler.threading.Thread", _FakeThread)

    scheduler_id = scheduler.schedule_periodic_task(task, interval=0.01, max_iterations=1)

    assert scheduler_id.startswith("periodic_periodic-registered_")
    assert scheduler_id in scheduler._periodic_tasks
    assert started == [True]


def test_schedule_periodic_task_validates_task_before_starting_thread(monkeypatch):
    from core.task_scheduler import TaskScheduler
    from models.executor_task import Task
    from core.task_compiler import TaskCompileError

    scheduler = TaskScheduler()
    scheduler._running = True
    task = Task(id="periodic-invalid", name="Periodic Invalid")
    started = []

    class _FakeThread:
        def __init__(self, target, daemon=True):
            self._target = target

        def start(self):
            started.append(True)

    monkeypatch.setattr(
        "core.task_scheduler.task_executor._build_execution_plan_from_queue_task",
        lambda task: (_ for _ in ()).throw(TaskCompileError("periodic invalid")),
    )
    monkeypatch.setattr("core.task_scheduler.threading.Thread", _FakeThread)
    monkeypatch.setattr("core.task_scheduler.task_log_manager.info", lambda *args, **kwargs: None)
    monkeypatch.setattr("core.task_scheduler.task_log_manager.error", lambda *args, **kwargs: None)

    assert scheduler.schedule_periodic_task(task, interval=1.0, max_iterations=1) is None
    assert started == []
    assert scheduler._periodic_tasks == {}


def test_cancel_scheduled_task_can_cancel_periodic_registration(monkeypatch):
    from core.task_scheduler import TaskScheduler

    scheduler = TaskScheduler()
    scheduler._periodic_tasks["periodic-job-1"] = {
        "scheduler_id": "periodic-job-1",
        "task_id": "job-1",
        "task_name": "Periodic Job 1",
        "interval": 5.0,
        "max_iterations": None,
        "iteration": 0,
        "thread": type("_FakeThread", (), {"is_alive": staticmethod(lambda: True), "join": staticmethod(lambda timeout=None: None)})(),
        "cancel_event": type("_FakeEvent", (), {"set": staticmethod(lambda: None), "is_set": staticmethod(lambda: False)})(),
    }

    monkeypatch.setattr("core.task_scheduler.task_executor.cancel_task", lambda task_id: False)
    monkeypatch.setattr("core.task_scheduler.task_log_manager.info", lambda *args, **kwargs: None)

    assert scheduler.cancel_scheduled_task("periodic-job-1") is True
    assert scheduler._periodic_tasks == {}


def test_stop_joins_periodic_threads_and_clears_registry(monkeypatch):
    from core.task_scheduler import TaskScheduler

    scheduler = TaskScheduler()
    scheduler._running = True
    joined = []

    class _FakeThread:
        def join(self, timeout=None):
            joined.append(timeout)

        def is_alive(self):
            return False

    scheduler._scheduler_thread = _FakeThread()
    scheduler._periodic_tasks["periodic-job-2"] = {
        "scheduler_id": "periodic-job-2",
        "task_id": "job-2",
        "task_name": "Periodic Job 2",
        "interval": 5.0,
        "max_iterations": None,
        "iteration": 1,
        "thread": _FakeThread(),
        "cancel_event": type("_FakeEvent", (), {"set": staticmethod(lambda: None), "is_set": staticmethod(lambda: False)})(),
    }

    monkeypatch.setattr("core.task_scheduler.task_log_manager.info", lambda *args, **kwargs: None)

    scheduler.stop()

    assert joined == [5, 5]
    assert scheduler._periodic_tasks == {}


def test_get_scheduled_tasks_uses_periodic_registry_metadata(monkeypatch):
    from core.task_scheduler import TaskScheduler

    scheduler = TaskScheduler()
    scheduler._periodic_tasks["periodic-job-3"] = {
        "scheduler_id": "periodic-job-3",
        "task_id": "job-3",
        "task_name": "Periodic Job 3",
        "interval": 15.0,
        "max_iterations": 4,
        "iteration": 2,
        "thread": type("_FakeThread", (), {"is_alive": staticmethod(lambda: True)})(),
        "cancel_event": type("_FakeEvent", (), {"set": staticmethod(lambda: None), "is_set": staticmethod(lambda: False)})(),
    }

    monkeypatch.setattr("core.task_scheduler.task_queue.get_task", lambda task_id: None)

    scheduled = scheduler.get_scheduled_tasks()

    assert scheduled == [{
        "task_id": "job-3",
        "task_name": "Periodic Job 3",
        "scheduled_time": None,
        "status": "running",
        "scheduler_id": "periodic-job-3",
        "schedule_type": "periodic",
        "interval": 15.0,
        "max_iterations": 4,
        "iteration": 2,
    }]


def test_schedule_periodic_task_uses_unique_scheduler_ids(monkeypatch):
    from core.task_scheduler import TaskScheduler
    from models.executor_task import Task

    scheduler = TaskScheduler()
    scheduler._running = True
    task = Task(id="periodic-dup", name="Periodic Dup")
    started = []

    class _FakeThread:
        def __init__(self, target, daemon=True):
            self._target = target

        def start(self):
            started.append(True)

        def join(self, timeout=None):
            return None

        def is_alive(self):
            return True

    monkeypatch.setattr(
        "core.task_scheduler.task_executor._build_execution_plan_from_queue_task",
        lambda task: object(),
    )
    monkeypatch.setattr("core.task_scheduler.task_executor.submit_task", lambda new_task: True)
    monkeypatch.setattr("core.task_scheduler.task_log_manager.info", lambda *args, **kwargs: None)
    monkeypatch.setattr("core.task_scheduler.task_log_manager.error", lambda *args, **kwargs: None)
    monkeypatch.setattr("core.task_scheduler.threading.Thread", _FakeThread)

    first_id = scheduler.schedule_periodic_task(task, interval=1.0, max_iterations=1)
    second_id = scheduler.schedule_periodic_task(task, interval=1.0, max_iterations=1)

    assert first_id != second_id
    assert len(scheduler._periodic_tasks) == 2
    assert started == [True, True]


def test_stop_keeps_registry_for_periodic_threads_that_fail_to_exit(monkeypatch):
    from core.task_scheduler import TaskScheduler

    scheduler = TaskScheduler()
    scheduler._running = True
    joined = []

    class _FakeMainThread:
        def join(self, timeout=None):
            joined.append(("main", timeout))

    class _StuckThread:
        def join(self, timeout=None):
            joined.append(("periodic", timeout))

        def is_alive(self):
            return True

    class _FakeEvent:
        def __init__(self):
            self.set_called = False

        def set(self):
            self.set_called = True

        def is_set(self):
            return self.set_called

    cancel_event = _FakeEvent()
    scheduler._scheduler_thread = _FakeMainThread()
    scheduler._periodic_tasks["periodic-stuck"] = {
        "scheduler_id": "periodic-stuck",
        "task_id": "stuck-task",
        "task_name": "Stuck Periodic",
        "interval": 5.0,
        "max_iterations": None,
        "iteration": 3,
        "thread": _StuckThread(),
        "cancel_event": cancel_event,
    }

    monkeypatch.setattr("core.task_scheduler.task_log_manager.info", lambda *args, **kwargs: None)

    scheduler.stop()

    assert ("main", 5) in joined
    assert ("periodic", 5) in joined
    assert cancel_event.set_called is True
    assert "periodic-stuck" in scheduler._periodic_tasks
