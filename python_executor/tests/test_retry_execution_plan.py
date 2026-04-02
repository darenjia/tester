from __future__ import annotations


def test_retry_task_preserves_execution_content_for_execution_plan_tasks(monkeypatch):
    from core.execution_plan import ExecutionPlan, PlannedCase
    from core.task_executor_production import TaskExecutorProduction

    executor = TaskExecutorProduction(message_sender=lambda _: None)
    global_tasks = {}
    status_updates = []

    def _add(task, overwrite=True):
        global_tasks[task.id] = task
        return True

    monkeypatch.setattr("core.task_executor_production.global_task_queue.add", _add)
    monkeypatch.setattr(
        "core.task_executor_production.global_task_queue.get_task",
        lambda task_id: global_tasks.get(task_id),
    )
    monkeypatch.setattr(
        "core.task_executor_production.global_task_queue.update_task_status",
        lambda task_id, status, error_message=None, result=None: status_updates.append(
            (task_id, status, error_message)
        )
        or True,
    )

    plan = ExecutionPlan(
        task_no="retry-source",
        project_no="PROJECT-RETRY",
        task_name="Retry Source",
        device_id="DEVICE-RETRY",
        tool_type="canoe",
        config_path="D:/cfgs/retry.cfg",
        timeout_seconds=30,
        cases=[
            PlannedCase(
                case_no="CASE-RETRY",
                case_name="Case Retry",
                case_type="test_module",
            )
        ],
    )

    assert executor.execute_plan(plan) is True

    original_task = global_tasks["retry-source"]
    original_task.status = "failed"

    new_task = executor.retry_task("retry-source")

    assert new_task is not None
    assert new_task.id != "retry-source"
    assert new_task.params["testItems"][0]["caseNo"] == "CASE-RETRY"
    assert new_task.params["testItems"][0]["name"] == "Case Retry"
    assert global_tasks[new_task.id] is new_task
    assert status_updates[-1][0] == "retry-source"
    assert "已重试" in (status_updates[-1][2] or "")
