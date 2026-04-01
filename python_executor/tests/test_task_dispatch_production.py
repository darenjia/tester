import types

import pytest

from core.execution_observability import (
    ExecutionLifecycleStage,
    get_execution_observability_manager,
)
import main_production
from models.result import Message
from models import executor_task as executor_task_module


class _FakeMapping:
    def __init__(
        self,
        category,
        enabled=True,
        script_path=None,
        case_name="",
        ini_config=None,
        para_config=None,
    ):
        self.category = category
        self.enabled = enabled
        self.script_path = script_path
        self.case_name = case_name
        self.ini_config = ini_config
        self.para_config = para_config


class _FakeMappingManager:
    def __init__(self, mappings):
        self._mappings = mappings

    def get_mapping(self, case_no):
        return self._mappings.get(case_no)


class _FakeExecutor:
    instances = []

    def __init__(self, message_sender):
        self.message_sender = message_sender
        self.started = False
        self.executed_tasks = []
        _FakeExecutor.instances.append(self)

    def start(self):
        self.started = True

    def execute_task(self, task):
        self.executed_tasks.append(task)
        executor_task_module.task_queue.add(task)
        return True


@pytest.fixture
def dispatcher(monkeypatch):
    observability_manager = get_execution_observability_manager()
    observability_manager._contexts.clear()

    monkeypatch.setattr(main_production.signal, "signal", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_production.config_manager, "start_watcher", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_production.performance_monitor, "start", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_production.performance_monitor, "set_alert_threshold", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_production.performance_monitor, "register_alert_callback", lambda *args, **kwargs: None)

    emitted = []
    monkeypatch.setattr(main_production, "emit", lambda event, payload: emitted.append((event, payload)))
    monkeypatch.setattr(main_production, "TaskExecutorProduction", _FakeExecutor)

    task_queue_calls = []
    monkeypatch.setattr(
        executor_task_module.task_queue,
        "add",
        lambda task: task_queue_calls.append(task) or True,
    )

    monkeypatch.setattr(
        main_production.InputValidator,
        "validate_task_data",
        lambda task_data: task_data,
    )

    fake_manager = _FakeMappingManager({})
    monkeypatch.setattr(
        "core.case_mapping_manager.get_case_mapping_manager",
        lambda: fake_manager,
    )

    executor = main_production.PythonExecutorProduction()
    return types.SimpleNamespace(
        executor=executor,
        emitted=emitted,
        task_queue_calls=task_queue_calls,
        fake_manager=fake_manager,
    )


def test_single_tool_dispatch_uses_executor_owned_queue_write(dispatcher, monkeypatch):
    dispatcher.fake_manager._mappings = {
        "case-1": _FakeMapping(category="CANOE", case_name="case-1"),
    }

    message = Message(
        type="TASK_DISPATCH",
        taskNo="task-1",
        deviceId="device-1",
        payload={
            "testItems": [
                {"case_no": "case-1", "name": "case-1"},
            ],
            "timeout": 60,
        },
    )

    dispatcher.executor._handle_task_dispatch(message, "sid-1")

    assert len(dispatcher.task_queue_calls) == 1
    assert dispatcher.task_queue_calls[0].toolType == "canoe"
    assert len(dispatcher.executor.task_executor.executed_tasks) == 1
    event, payload = dispatcher.emitted[0]
    assert event == "task_response"
    assert payload["taskNo"] == "task-1"
    assert payload["status"] == "queued"
    assert payload["message"] == "任务已加入队列"

    snapshot = get_execution_observability_manager().get_snapshot("task-1")
    assert snapshot["stage_history"][:3] == [
        ExecutionLifecycleStage.RECEIVED.value,
        ExecutionLifecycleStage.VALIDATED.value,
        ExecutionLifecycleStage.QUEUED.value,
    ]
    assert snapshot["tool_type"] == "canoe"
    assert snapshot["device_id"] == "device-1"


def test_mixed_tool_dispatch_is_rejected_before_execution(dispatcher):
    dispatcher.fake_manager._mappings = {
        "case-1": _FakeMapping(category="CANOE", case_name="case-1"),
        "case-2": _FakeMapping(category="TSMASTER", case_name="case-2"),
    }

    message = Message(
        type="TASK_DISPATCH",
        taskNo="task-2",
        deviceId="device-2",
        payload={
            "testItems": [
                {"case_no": "case-1", "name": "case-1"},
                {"case_no": "case-2", "name": "case-2"},
            ],
            "timeout": 60,
        },
    )

    dispatcher.executor._handle_task_dispatch(message, "sid-2")

    assert dispatcher.task_queue_calls == []
    assert dispatcher.executor.task_executor is None
    assert dispatcher.emitted
    event, payload = dispatcher.emitted[0]
    assert event == "error"
    assert "多个" in payload["error"]
    assert "tool" in payload["error"].lower() or "工具" in payload["error"]

    snapshot = get_execution_observability_manager().get_snapshot("task-2")
    assert snapshot["current_stage"] == ExecutionLifecycleStage.FINISHED.value
    assert snapshot["failed_stage"] == ExecutionLifecycleStage.VALIDATED.value
    assert snapshot["error_code"] == "TASK_VALIDATION_FAILED"
