import logging
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from utils.logger import get_logger, logger_manager


def test_get_logger_without_name_returns_executor_logger():
    logger = get_logger()

    assert logger is logger_manager.get_logger()
    assert logger.name == "TestExecutor"


def test_get_logger_with_name_returns_child_logger_with_configured_handlers(monkeypatch):
    base_logger = logger_manager.get_logger()
    test_handler = logging.StreamHandler()
    child_name = "logger_manager_test"
    child_logger = logging.getLogger(f"{base_logger.name}.{child_name}")

    child_logger.handlers.clear()
    child_logger.propagate = True

    monkeypatch.setattr(logger_manager, "_setup_done", True, raising=False)
    monkeypatch.setattr(base_logger, "handlers", [test_handler], raising=False)

    named_logger = get_logger(child_name)

    assert named_logger.name == f"{base_logger.name}.{child_name}"
    assert named_logger.handlers == [test_handler]
    assert named_logger.propagate is False

def test_memory_logs_preserve_structured_task_context_fields(tmp_path):
    logger_manager.setup(log_dir=str(tmp_path / "logs"))
    logger_manager.clear_memory_logs()

    logger = logger_manager.get_logger("structured")
    logger.info(
        "task moved to executing",
        extra={
            "task_no": "TASK-100",
            "device_id": "DEVICE-9",
            "tool_type": "canoe",
            "stage": "executing",
            "attempt": 2,
            "error_code": None,
        },
    )

    logs = logger_manager.get_memory_logs(limit=1)

    assert logs[-1]["task_no"] == "TASK-100"
    assert logs[-1]["device_id"] == "DEVICE-9"
    assert logs[-1]["tool_type"] == "canoe"
    assert logs[-1]["stage"] == "executing"
    assert logs[-1]["attempt"] == 2
    assert logs[-1]["error_code"] is None


def test_bind_task_context_returns_logger_adapter_that_preserves_fields(tmp_path):
    logger_manager.setup(log_dir=str(tmp_path / "logs"))
    logger_manager.clear_memory_logs()

    adapter = logger_manager.bind_task_context(
        logger_manager.get_logger("structured"),
        task_no="TASK-101",
        device_id="DEVICE-10",
        tool_type="tsmaster",
        stage="preparing",
        attempt=3,
        error_code="NONE",
    )

    adapter.info("preparing started")
    logs = logger_manager.get_memory_logs(limit=1)

    assert logs[-1]["task_no"] == "TASK-101"
    assert logs[-1]["device_id"] == "DEVICE-10"
    assert logs[-1]["tool_type"] == "tsmaster"
    assert logs[-1]["stage"] == "preparing"
    assert logs[-1]["attempt"] == 3
    assert logs[-1]["error_code"] == "NONE"
