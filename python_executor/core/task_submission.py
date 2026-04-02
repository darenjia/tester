"""
Task Submission Module - Authoritative Intake Pipeline

Provides one canonical function for compiling and submitting any task
regardless of intake path (WebSocket, HTTP API, etc.).

All entrypoints MUST use submit_task() for task submission to ensure
consistent execution semantics and avoid duplicate queue writes.
"""
from __future__ import annotations

from dataclasses import dataclass
from typing import Dict, Any, Optional, Tuple
import uuid

from utils.logger import get_logger
from utils.validators import InputValidator, ValidationError
from core.task_compiler import TaskCompiler, TaskCompileError
from core.case_mapping_manager import get_case_mapping_manager
from core.execution_plan import ExecutionPlan
from core.task_executor_production import TaskExecutorProduction

logger = get_logger("task_submission")


@dataclass
class SubmissionResult:
    """Result of a task submission attempt."""
    success: bool
    task_no: str
    execution_plan: Optional[ExecutionPlan] = None
    error_message: Optional[str] = None
    error_code: Optional[str] = None

    def to_dict(self) -> Dict[str, Any]:
        result = {
            "success": self.success,
            "taskNo": self.task_no,
        }
        if self.execution_plan:
            result["plan"] = self.execution_plan.to_dict()
        if self.error_message:
            result["errorMessage"] = self.error_message
        if self.error_code:
            result["errorCode"] = self.error_code
        return result


def _build_compile_payload(task_no: str, task_data: Dict[str, Any]) -> Dict[str, Any]:
    """
    Build a normalized compile payload from task data.

    Args:
        task_no: Task identifier
        task_data: Raw task data containing caseList/testItems, toolType, etc.

    Returns:
        Payload dict suitable for TaskCompiler.compile_payload()
    """
    # Handle both caseList (TDM2.0) and testItems (internal format)
    case_list = task_data.get("caseList") or task_data.get("testItems", []) or []

    return {
        "taskNo": task_no,
        "projectNo": task_data.get("projectNo", ""),
        "taskName": task_data.get("taskName", ""),
        "deviceId": task_data.get("deviceId"),
        "toolType": task_data.get("toolType"),
        "configPath": task_data.get("configPath"),
        "configName": task_data.get("configName"),
        "baseConfigDir": task_data.get("baseConfigDir"),
        "variables": task_data.get("variables", {}),
        "canoeNamespace": task_data.get("canoeNamespace"),
        "timeout": task_data.get("timeout", 3600),
        "testItems": [
            {
                "caseNo": case.get("caseNo") or case.get("case_no", ""),
                "name": case.get("caseName") or case.get("name", ""),
                "type": case.get("caseType", "test_module"),
                "dtcInfo": case.get("dtcInfo") or case.get("dtc_info"),
                "params": case.get("params", {}),
                "repeat": case.get("repeat", 1),
            }
            for case in case_list
        ],
    }


def submit_task(
    task_data: Dict[str, Any],
    task_no: Optional[str] = None,
    device_id: Optional[str] = None,
    executor: Optional[TaskExecutorProduction] = None,
) -> SubmissionResult:
    """
    Authoritative task submission function.

    Compiles the task data into an ExecutionPlan and submits it to the
    task executor. This is the single entry point for all task intake
    paths (WebSocket, HTTP API, etc.).

    Args:
        task_data: Task payload. For TDM2.0 format, should contain:
            - taskNo (optional if task_no provided)
            - caseList: list of test cases (will be transformed to testItems)
            - toolType (optional, auto-detected from mappings)
            - configPath (optional)
            - etc.
        task_no: Override task identifier (useful when task_data doesn't contain taskNo)
        device_id: Override device identifier
        executor: Task executor instance (uses global if not provided)

    Returns:
        SubmissionResult with success status and either execution plan or error

    Raises:
        This function catches all exceptions and returns them as SubmissionResult
        to provide a consistent error handling interface.
    """
    try:
        # Make a copy to avoid modifying the original
        data = dict(task_data)

        # Normalize task_no
        final_task_no = task_no or data.get("taskNo") or str(uuid.uuid4())

        # Enrich with device_id if provided separately
        if device_id:
            data["deviceId"] = device_id

        # Ensure taskNo is set in data for compilation
        data["taskNo"] = final_task_no

        # Transform caseList to testItems for TDM2.0 compatibility with validator
        if "caseList" in data and "testItems" not in data:
            data["testItems"] = data.pop("caseList")

        # Validate input
        try:
            validated_data = InputValidator.validate_task_data(data)
        except ValidationError as e:
            logger.warning(f"[submit_task] Validation failed for {final_task_no}: {e}")
            # Record observability for validation failure
            try:
                from core.execution_observability import get_execution_observability_manager
                from utils.metrics import record_metric
                observability_manager = get_execution_observability_manager()
                if final_task_no in observability_manager._contexts:
                    observability_manager.fail(
                        final_task_no,
                        error_code="TASK_VALIDATION_FAILED",
                        error_message=str(e),
                        retryable=False,
                    )
                    record_metric("task.failed", 1, {"task_no": final_task_no, "stage": "validated"})
            except Exception:
                pass
            return SubmissionResult(
                success=False,
                task_no=final_task_no,
                error_message=str(e),
                error_code="TASK_VALIDATION_FAILED",
            )

        # Record validation success observability
        try:
            from core.execution_observability import get_execution_observability_manager, ExecutionLifecycleStage
            from utils.metrics import record_metric
            observability_manager = get_execution_observability_manager()
            if final_task_no in observability_manager._contexts:
                observability_manager.transition(final_task_no, ExecutionLifecycleStage.VALIDATED)
                record_metric("task.validated", 1, {"task_no": final_task_no})
        except Exception:
            pass

        # Compile execution plan
        mapping_manager = get_case_mapping_manager()
        compiler = TaskCompiler(mapping_manager=mapping_manager)
        compile_payload = _build_compile_payload(final_task_no, validated_data)

        try:
            execution_plan = compiler.compile_payload(compile_payload)
        except TaskCompileError as e:
            logger.warning(f"[submit_task] Compilation failed for {final_task_no}: {e}")
            # Record observability for compilation failure
            try:
                from core.execution_observability import get_execution_observability_manager
                from utils.metrics import record_metric
                observability_manager = get_execution_observability_manager()
                if final_task_no in observability_manager._contexts:
                    observability_manager.fail(
                        final_task_no,
                        error_code="TASK_COMPILE_FAILED",
                        error_message=str(e),
                        retryable=False,
                    )
                    record_metric("task.failed", 1, {"task_no": final_task_no, "stage": "compiled"})
            except Exception:
                pass
            return SubmissionResult(
                success=False,
                task_no=final_task_no,
                error_message=str(e),
                error_code="TASK_COMPILE_FAILED",
            )

        # Record compilation success observability
        try:
            from core.execution_observability import get_execution_observability_manager, ExecutionLifecycleStage
            from utils.metrics import record_metric
            observability_manager = get_execution_observability_manager()
            if final_task_no in observability_manager._contexts:
                observability_manager.transition(final_task_no, ExecutionLifecycleStage.COMPILED)
                # Update tool_type in context from execution_plan
                context = observability_manager._contexts[final_task_no]
                context.tool_type = execution_plan.tool_type
                record_metric("task.compiled", 1, {"task_no": final_task_no})
        except Exception:
            pass

        # Get executor
        if executor is None:
            from core.task_executor_production import get_task_executor
            executor = get_task_executor()

        # Submit to executor
        if executor.execute_plan(execution_plan):
            logger.info(f"[submit_task] Task submitted successfully: {final_task_no}")
            return SubmissionResult(
                success=True,
                task_no=final_task_no,
                execution_plan=execution_plan,
            )
        else:
            logger.error(f"[submit_task] Executor rejected task: {final_task_no}")
            # Record observability for executor rejection
            try:
                from core.execution_observability import get_execution_observability_manager
                from utils.metrics import record_metric
                observability_manager = get_execution_observability_manager()
                if final_task_no in observability_manager._contexts:
                    observability_manager.fail(
                        final_task_no,
                        error_code="TASK_QUEUE_REJECTED",
                        error_message="任务加入队列失败",
                        retryable=False,
                    )
                    record_metric("task.failed", 1, {"task_no": final_task_no, "stage": "executed"})
            except Exception:
                pass
            return SubmissionResult(
                success=False,
                task_no=final_task_no,
                error_message="任务加入队列失败",
                error_code="TASK_QUEUE_REJECTED",
            )

    except Exception as e:
        logger.error(f"[submit_task] Unexpected error for task: {e}", exc_info=True)
        return SubmissionResult(
            success=False,
            task_no=task_no or "unknown",
            error_message=f"任务提交失败: {str(e)}",
            error_code="TASK_SUBMISSION_FAILED",
        )


def submit_task_from_legacy_format(
    task: Any,
    executor: Optional[TaskExecutorProduction] = None,
) -> SubmissionResult:
    """
    Submit a task using the legacy Task format.

    Converts the legacy Task object to an ExecutionPlan and submits it.

    Args:
        task: Legacy Task object (models.executor_task.Task)
        executor: Task executor instance

    Returns:
        SubmissionResult with success status
    """
    try:
        task_id = getattr(task, 'id', None) or getattr(task, 'task_id', None)
        if not task_id:
            return SubmissionResult(
                success=False,
                task_no="unknown",
                error_message="任务缺少ID",
                error_code="TASK_INVALID",
            )

        # Convert legacy task to execution plan
        execution_plan = ExecutionPlan.from_legacy_task(task)

        # Get executor
        if executor is None:
            from core.task_executor_production import get_task_executor
            executor = get_task_executor()

        # Submit
        if executor.execute_plan(execution_plan):
            return SubmissionResult(
                success=True,
                task_no=task_id,
                execution_plan=execution_plan,
            )
        else:
            return SubmissionResult(
                success=False,
                task_no=task_id,
                error_message="任务加入队列失败",
                error_code="TASK_QUEUE_REJECTED",
            )

    except Exception as e:
        logger.error(f"[submit_task_from_legacy_format] Error: {e}", exc_info=True)
        task_id = getattr(task, 'id', None) or "unknown"
        return SubmissionResult(
            success=False,
            task_no=task_id,
            error_message=f"任务提交失败: {str(e)}",
            error_code="TASK_SUBMISSION_FAILED",
        )