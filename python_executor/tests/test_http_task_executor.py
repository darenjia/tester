from __future__ import annotations

import types

import pytest

from api import task_executor as http_task_executor


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


def test_compile_execution_plan_from_http_task_data(monkeypatch):
    monkeypatch.setattr(
        http_task_executor,
        "get_case_mapping_manager",
        lambda: _FakeMappingManager(
            {
                "CASE-1": _FakeMapping("CANOE", script_path="D:/cfgs/http.cfg", case_name="Case 1"),
            }
        ),
    )

    plan = http_task_executor._compile_execution_plan(
        "TASK-HTTP",
        {
            "taskNo": "TASK-HTTP",
            "deviceId": "DEVICE-HTTP",
            "caseList": [{"caseNo": "CASE-1"}],
            "timeout": 90,
        },
    )

    assert plan.task_no == "TASK-HTTP"
    assert plan.tool_type == "canoe"
    assert plan.config_path == "D:/cfgs/http.cfg"
    assert plan.cases[0].case_no == "CASE-1"


def test_compile_execution_plan_rejects_mixed_tool_types(monkeypatch):
    monkeypatch.setattr(
        http_task_executor,
        "get_case_mapping_manager",
        lambda: _FakeMappingManager(
            {
                "CASE-1": _FakeMapping("CANOE"),
                "CASE-2": _FakeMapping("TSMASTER"),
            }
        ),
    )

    with pytest.raises(ValueError):
        http_task_executor._compile_execution_plan(
            "TASK-MIXED",
            {
                "taskNo": "TASK-MIXED",
                "deviceId": "DEVICE-MIXED",
                "caseList": [{"caseNo": "CASE-1"}, {"caseNo": "CASE-2"}],
            },
        )


def test_execute_task_async_uses_execute_plan(monkeypatch):
    executed = {}
    updates = []

    class _ImmediateThread:
        def __init__(self, target, daemon=True):
            self._target = target

        def start(self):
            self._target()

    class _FakeExecutor:
        def execute_plan(self, plan):
            executed["plan"] = plan
            return True

        def get_current_status(self):
            if "seen" in executed:
                return {"status": "idle", "task_id": executed["plan"].task_no}
            executed["seen"] = True
            return {"status": "running", "task_id": executed["plan"].task_no}

    monkeypatch.setattr(http_task_executor.threading, "Thread", _ImmediateThread)
    monkeypatch.setattr(http_task_executor, "_get_executor", lambda: _FakeExecutor())
    monkeypatch.setattr(
        http_task_executor,
        "_update_task_in_queue",
        lambda task_no, **kwargs: updates.append((task_no, kwargs)),
    )
    monkeypatch.setattr(
        http_task_executor,
        "get_case_mapping_manager",
        lambda: _FakeMappingManager({"CASE-1": _FakeMapping("CANOE")}),
    )

    http_task_executor.execute_task_async(
        "TASK-ASYNC",
        {
            "taskNo": "TASK-ASYNC",
            "deviceId": "DEVICE-ASYNC",
            "caseList": [{"caseNo": "CASE-1", "caseName": "Case 1"}],
            "timeout": 30,
        },
    )

    assert executed["plan"].task_no == "TASK-ASYNC"
    assert executed["plan"].tool_type == "canoe"
    assert updates[0][0] == "TASK-ASYNC"
    assert updates[0][1]["status"] == "running"


def test_execute_task_async_marks_failure_when_execute_plan_is_rejected(monkeypatch):
    updates = []
    sleeps = []

    class _ImmediateThread:
        def __init__(self, target, daemon=True):
            self._target = target

        def start(self):
            self._target()

    class _RejectingExecutor:
        def execute_plan(self, plan):
            return False

        def get_current_status(self):
            return {"status": "idle", "task_id": None}

    monkeypatch.setattr(http_task_executor.threading, "Thread", _ImmediateThread)
    monkeypatch.setattr(http_task_executor, "_get_executor", lambda: _RejectingExecutor())
    monkeypatch.setattr(
        http_task_executor,
        "_update_task_in_queue",
        lambda task_no, **kwargs: updates.append((task_no, kwargs)),
    )
    monkeypatch.setattr(http_task_executor.time, "sleep", lambda seconds: sleeps.append(seconds))
    monkeypatch.setattr(
        http_task_executor,
        "get_case_mapping_manager",
        lambda: _FakeMappingManager({"CASE-1": _FakeMapping("CANOE")}),
    )

    http_task_executor.execute_task_async(
        "TASK-REJECTED",
        {
            "taskNo": "TASK-REJECTED",
            "deviceId": "DEVICE-REJECTED",
            "caseList": [{"caseNo": "CASE-1", "caseName": "Case 1"}],
            "timeout": 30,
        },
    )

    assert updates[0] == (
        "TASK-REJECTED",
        {"status": "running", "message": "任务开始执行", "progress": 0},
    )
    assert updates[-1][0] == "TASK-REJECTED"
    assert updates[-1][1]["status"] == "failed"
    assert "提交执行计划失败" in updates[-1][1]["message"]
    assert sleeps == []


def test_cancel_task_execution_passes_requested_task_no(monkeypatch):
    cancelled = []

    class _FakeExecutor:
        def cancel_task(self, task_no=None):
            cancelled.append(task_no)
            return True

    monkeypatch.setattr(http_task_executor, "_get_executor", lambda: _FakeExecutor())

    http_task_executor.cancel_task_execution("TASK-CANCEL")

    assert cancelled == ["TASK-CANCEL"]
