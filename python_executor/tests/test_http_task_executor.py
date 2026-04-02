from __future__ import annotations

import pytest

from api import task_executor as http_task_executor


def test_cancel_task_execution_passes_requested_task_no(monkeypatch):
    cancelled = []

    class _FakeExecutor:
        def cancel_task(self, task_no=None):
            cancelled.append(task_no)
            return True

    monkeypatch.setattr(http_task_executor, "_get_executor", lambda: _FakeExecutor())

    http_task_executor.cancel_task_execution("TASK-CANCEL")

    assert cancelled == ["TASK-CANCEL"]
