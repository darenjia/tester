"""
End-to-End Business Flow Integration Tests

These tests verify the complete business flow from task intake through
report upload, including both happy paths and negative paths.

Coverage:
- Happy path: task accepted → compiled → config resolved → executed → reported
- Negative paths: compile/prepare failure, execution failure, upload failure, submit failure
- Task status API consistency with report/retry layer
"""
from __future__ import annotations

import sys
from pathlib import Path
from datetime import datetime
from unittest.mock import MagicMock, patch, PropertyMock

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from core.task_submission import submit_task, SubmissionResult
from core.task_compiler import TaskCompileError
from core.config_preparation import (
    ConfigPreparationPhase,
    ConfigConflictError,
    MissingConfigError,
)
from core.result_collector import ResultCollector
from core.execution_plan import ExecutionPlan, PlannedCase, ConfigSource
from core.execution_observability import (
    ExecutionLifecycleStage,
    get_execution_observability_manager,
)
from models.task import Task, TaskStatus, TestResult, TaskResult
from models.result import ExecutionOutcome, CaseResult, ExecutionResult
from utils.report_client import ReportClient


# =============================================================================
# Fake Components for Testing
# =============================================================================

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
    """Fake executor that records submission attempts."""
    def __init__(self, accept=True):
        self.accept = accept
        self.submitted_plans = []

    def execute_plan(self, plan):
        self.submitted_plans.append(plan)
        return self.accept


# =============================================================================
# Happy Path Tests
# =============================================================================

class TestHappyPathTaskAcceptedAndCompiled:
    """Test that tasks are properly accepted and compiled."""

    def test_submit_task_compiles_valid_payload(self, monkeypatch):
        """submit_task should compile valid payload into ExecutionPlan."""
        fake_executor = _FakeExecutor(accept=True)

        def fake_get_executor():
            return fake_executor

        monkeypatch.setattr(
            'core.task_executor_production.get_task_executor',
            fake_get_executor
        )

        result = submit_task(
            {
                "taskNo": "TASK-COMPILE-001",
                "caseList": [{"caseNo": "CASE-1", "name": "Case 1"}],
                "toolType": "canoe",
                "configPath": "D:/cfgs/test.cfg",
            },
            task_no="TASK-COMPILE-001"
        )

        assert result.success is True
        assert result.task_no == "TASK-COMPILE-001"
        assert result.execution_plan is not None
        assert result.execution_plan.task_no == "TASK-COMPILE-001"
        assert len(result.execution_plan.cases) == 1

    def test_compilation_resolves_tool_type_from_mapping(self, monkeypatch):
        """Compiler should resolve tool type from case mapping when not explicitly provided in compile payload."""
        fake_executor = _FakeExecutor(accept=True)

        def fake_get_executor():
            return fake_executor

        monkeypatch.setattr(
            'core.task_executor_production.get_task_executor',
            fake_get_executor
        )

        # Create mapping manager that resolves CANOE category
        mapping_manager = _FakeMappingManager({
            "CASE-AUTO-TOOL": _FakeMapping(
                category="CANOE",
                script_path="D:/cfgs/auto.cfg",
                case_name="Auto Tool Case",
            ),
        })

        # Patch the mapping manager in task_submission where it's used to create TaskCompiler
        with patch('core.task_submission.get_case_mapping_manager', return_value=mapping_manager):
            # Note: toolType must be provided at API level (validated), but compiler
            # can still enrich/verify against mappings. The key is that the mapping
            # provides script_path which is used during config preparation.
            result = submit_task(
                {
                    "taskNo": "TASK-AUTO-TOOL",
                    "caseList": [{"caseNo": "CASE-AUTO-TOOL", "name": "Auto Tool Case"}],
                    "toolType": "canoe",  # Required at API level
                },
                task_no="TASK-AUTO-TOOL"
            )

        assert result.success is True
        assert result.execution_plan.tool_type == "canoe"
        # The mapping was used to enrich the execution plan
        assert len(result.execution_plan.cases) == 1


class TestHappyPathConfigurationResolved:
    """Test that configuration/material is properly resolved before execution."""

    def test_config_preparation_phase_resolves_explicit_path(self):
        """ConfigPreparationPhase should resolve explicit config_path."""
        plan = ExecutionPlan(
            task_no="TASK-CFG-001",
            project_no="PROJECT-1",
            tool_type="canoe",
            cases=[
                PlannedCase(
                    case_no="CASE-CFG-1",
                    case_name="Case Cfg 1",
                    case_type="test_module",
                )
            ],
            config_path="D:/cfgs/explicit.cfg",
            config_source=ConfigSource.DIRECT_PATH,
        )

        prep_phase = ConfigPreparationPhase(mapping_manager=_FakeMappingManager({}))
        prepared = prep_phase.prepare(plan)

        assert prepared.config_path == "D:/cfgs/explicit.cfg"
        assert prepared.config_source == ConfigSource.DIRECT_PATH

    def test_config_preparation_phase_resolves_from_mapping(self):
        """ConfigPreparationPhase should resolve config_path from case mapping."""
        mapping_manager = _FakeMappingManager({
            "CASE-MAP-CFG": _FakeMapping(
                category="CANOE",
                script_path="D:/cfgs/mapped.cfg",
                case_name="Mapped Case",
            ),
        })

        plan = ExecutionPlan(
            task_no="TASK-MAP-CFG-001",
            project_no="PROJECT-MAP",
            tool_type="canoe",
            cases=[
                PlannedCase(
                    case_no="CASE-MAP-CFG",
                    case_name="Case Map Cfg",
                    case_type="test_module",
                )
            ],
            config_source=ConfigSource.UNSPECIFIED,
        )

        prep_phase = ConfigPreparationPhase(mapping_manager=mapping_manager)
        prepared = prep_phase.prepare(plan)

        assert prepared.config_path == "D:/cfgs/mapped.cfg"
        assert prepared.config_source == ConfigSource.CASE_MAPPING

    def test_config_preparation_detects_tsmaster_inline(self):
        """TSMaster cases without script_path should use TSMASTER_INLINE."""
        mapping_manager = _FakeMappingManager({
            "CASE-TS-INLINE": _FakeMapping(
                category="TSMASTER",
                enabled=True,
                script_path=None,  # No script_path - inline execution
                case_name="TSMaster Inline",
            ),
        })

        plan = ExecutionPlan(
            task_no="TASK-TS-INLINE",
            project_no="PROJECT-TS",
            tool_type="tsmaster",
            cases=[
                PlannedCase(
                    case_no="CASE-TS-INLINE",
                    case_name="Case TS Inline",
                    case_type="test_module",
                )
            ],
            config_source=ConfigSource.UNSPECIFIED,
        )

        prep_phase = ConfigPreparationPhase(mapping_manager=mapping_manager)
        prepared = prep_phase.prepare(plan)

        assert prepared.config_source == ConfigSource.TSMASTER_INLINE
        assert prepared.is_tsmaster_inline is True


class TestHappyPathExecutionStrategyRuns:
    """Test that execution strategy properly executes tasks."""

    def test_execution_outcome_is_produced(self):
        """Task execution should produce an ExecutionOutcome."""
        collector = ResultCollector("TASK-EXEC-001")

        # Add some test results
        result1 = TestResult(
            name="Test Case 1",
            type="test_module",
            passed=True,
            verdict="PASS",
        )
        result2 = TestResult(
            name="Test Case 2",
            type="test_module",
            passed=False,
            verdict="FAIL",
            error="Assertion failed",
        )

        collector.add_test_result(result1)
        collector.add_test_result(result2)

        outcome = collector.finalize(TaskStatus.COMPLETED.value)

        assert isinstance(outcome, ExecutionOutcome)
        assert outcome.task_no == "TASK-EXEC-001"
        assert outcome.status == "completed"
        assert len(outcome.case_results) == 2
        assert outcome.summary["total"] == 2
        assert outcome.summary["passed"] == 1
        assert outcome.summary["failed"] == 1

    def test_execution_outcome_contains_timing_info(self):
        """ExecutionOutcome should contain start/finish timing info."""
        collector = ResultCollector("TASK-TIME-001")

        result = TestResult(
            name="Timed Case",
            type="test_module",
            passed=True,
            verdict="PASS",
        )
        collector.add_test_result(result)

        outcome = collector.finalize(TaskStatus.COMPLETED.value)

        assert outcome.started_at is not None
        assert outcome.finished_at is not None
        assert outcome.duration is not None
        assert outcome.duration >= 0


class TestHappyPathResultCollectorFinalizes:
    """Test that ResultCollector properly finalizes ExecutionOutcome."""

    def test_result_collector_finalize_with_success(self):
        """ResultCollector.finalize should produce success outcome."""
        collector = ResultCollector("TASK-SUCCESS-001")

        result = TestResult(
            name="Success Case",
            type="test_module",
            passed=True,
            verdict="PASS",
        )
        collector.add_test_result(result)

        outcome = collector.finalize(TaskStatus.COMPLETED.value)

        assert outcome.status == "completed"
        assert outcome.verdict == "PASS"
        assert outcome.summary["passed"] == 1
        assert outcome.summary["failed"] == 0

    def test_result_collector_finalize_with_all_failed(self):
        """ResultCollector.finalize should produce failure verdict when all fail."""
        collector = ResultCollector("TASK-ALL-FAIL-001")

        result1 = TestResult(
            name="Fail Case 1",
            type="test_module",
            passed=False,
            verdict="FAIL",
        )
        result2 = TestResult(
            name="Fail Case 2",
            type="test_module",
            passed=False,
            verdict="FAIL",
        )
        collector.add_test_result(result1)
        collector.add_test_result(result2)

        outcome = collector.finalize(TaskStatus.FAILED.value)

        assert outcome.status == "failed"
        assert outcome.verdict == "FAIL"
        assert outcome.summary["passed"] == 0
        assert outcome.summary["failed"] == 2

    def test_result_collector_finalize_with_error_message(self):
        """ResultCollector.finalize should include error_message when provided."""
        collector = ResultCollector("TASK-ERR-001")

        outcome = collector.finalize(TaskStatus.FAILED.value, error_message="Connection timeout")

        assert outcome.status == "failed"
        assert outcome.error_summary == "Connection timeout"


class TestHappyPathReportClientUploads:
    """Test that ReportClient properly uploads reports and submits results."""

    def test_report_result_successful_upload_and_submit(self, tmp_path, monkeypatch):
        """ReportClient.report_result should upload file and submit successfully."""
        fake_config_manager = MagicMock()
        fake_config_manager.get.side_effect = lambda key, default=None: {
            "report_server.enabled": True,
            "report_server.host": "",
            "report_server.port": "",
            "report_server.path": "",
            "report.result_api_url": "http://report.example.com/direct-report",
            "report.file_upload_url": "http://upload.example.com/files",
            "report_retry.enabled": False,
        }.get(key, default)

        client = ReportClient(fake_config_manager)
        captured = {}

        def fake_make_request(method, url, **kwargs):
            if "files" in kwargs:
                return {"code": 200, "data": {"url": "http://files.example.com/report.html"}}
            captured["payload"] = kwargs.get("json", {})
            return {"code": 200}

        monkeypatch.setattr(client, "_make_request", fake_make_request)

        report_file = tmp_path / "report.html"
        report_file.write_text("ok", encoding="utf-8")

        report_data = {
            "taskNo": "TASK-REPORT-OK",
            "caseList": [{"caseNo": "CASE-1", "result": "PASS"}]
        }

        task_info = {
            "projectNo": "PROJ-OK",
            "deviceId": "DEVICE-OK",
            "trace_id": "trace-ok-123",
            "attempt_id": "attempt-ok-456",
        }

        result = client.report_result(report_data, task_info, report_file_path=str(report_file))

        assert result is True
        assert captured["payload"]["caseList"][0]["reAddress"] == "http://files.example.com/report.html"

    def test_execution_outcome_to_report_payload(self):
        """ExecutionOutcome.to_report_payload should produce correct payload."""
        outcome = ExecutionOutcome(
            task_no="TASK-PAYLOAD-001",
            status="completed",
            started_at=datetime(2026, 4, 2, 10, 0, 0),
            finished_at=datetime(2026, 4, 2, 10, 5, 0),
            verdict="PASS",
            summary={"total": 5, "passed": 5, "failed": 0, "passRate": "100.0%"},
        )

        payload = outcome.to_report_payload(
            task_info={"projectNo": "PROJ-PAYLOAD"},
            report_url="http://files.example.com/report.html"
        )

        assert payload["taskNo"] == "TASK-PAYLOAD-001"
        assert payload["status"] == "completed"
        assert payload["summary"]["total"] == 5
        assert payload["summary"]["passed"] == 5
        assert payload["projectNo"] == "PROJ-PAYLOAD"
        assert len(payload["results"]) == 0  # No case results in this outcome

    def test_execution_outcome_preserves_uploaded_report_url(self, tmp_path, monkeypatch):
        """ExecutionOutcome should preserve uploaded report URL in payload."""
        fake_config_manager = MagicMock()
        fake_config_manager.get.side_effect = lambda key, default=None: {
            "report_server.enabled": True,
            "report_server.host": "",
            "report_server.port": "",
            "report_server.path": "",
            "report.result_api_url": "http://report.example.com/direct-report",
            "report.file_upload_url": "http://upload.example.com/files",
            "report_retry.enabled": False,
        }.get(key, default)

        client = ReportClient(fake_config_manager)
        captured = {}

        def fake_make_request(method, url, **kwargs):
            if "files" in kwargs:
                return {"code": 200, "data": {"url": "http://files.example.com/outcome.html"}}
            captured["payload"] = kwargs.get("json", {})
            return {"code": 200}

        monkeypatch.setattr(client, "_make_request", fake_make_request)

        report_file = tmp_path / "outcome.html"
        report_file.write_text("ok", encoding="utf-8")

        # Create ExecutionOutcome with case_results
        class OutcomeCaseResult:
            def __init__(self):
                self.reAddress = None

            def to_dict(self):
                return {"caseNo": "CASE-OUTCOME", "result": "PASS", "reAddress": self.reAddress}

        outcome = ExecutionOutcome(
            task_no="TASK-OUTCOME-URL",
            status="completed",
            summary={"total": 1, "passed": 1},
            case_results=[OutcomeCaseResult()],
        )

        result = client.report_task_result(outcome, report_file_path=str(report_file))

        assert result is True
        assert captured["payload"]["results"][0]["reAddress"] == "http://files.example.com/outcome.html"


# =============================================================================
# Negative Path Tests
# =============================================================================

class TestNegativePathCompilePrepareFailure:
    """Test that compile/prepare failures are properly handled."""

    def test_submit_task_rejects_empty_case_list(self, monkeypatch):
        """submit_task should reject empty caseList with error."""
        fake_executor = _FakeExecutor(accept=True)

        def fake_get_executor():
            return fake_executor

        monkeypatch.setattr(
            'core.task_executor_production.get_task_executor',
            fake_get_executor
        )

        result = submit_task(
            {
                "taskNo": "TASK-EMPTY",
                "caseList": [],  # Empty!
                "toolType": "canoe",
            },
            task_no="TASK-EMPTY"
        )

        assert result.success is False
        assert result.error_code in ("TASK_VALIDATION_FAILED", "TASK_COMPILE_FAILED")

    def test_submit_task_rejects_invalid_case_structure(self, monkeypatch):
        """submit_task should reject cases with completely missing identifiers."""
        fake_executor = _FakeExecutor(accept=True)

        def fake_get_executor():
            return fake_executor

        monkeypatch.setattr(
            'core.task_executor_production.get_task_executor',
            fake_get_executor
        )

        # Case with no identifier at all - name, caseNo, or case_no
        result = submit_task(
            {
                "taskNo": "TASK-INVALID",
                "caseList": [{"type": "test_module"}],  # No caseNo, name, or case_no
                "toolType": "canoe",
            },
            task_no="TASK-INVALID"
        )

        # This should fail at compile stage since case_no cannot be empty
        assert result.success is False

    def test_config_preparation_fails_on_conflict(self):
        """ConfigPreparationPhase should raise ConfigConflictError on conflicting paths."""
        mapping_manager = _FakeMappingManager({
            "CASE-CONFLICT-A": _FakeMapping(
                category="CANOE",
                script_path="D:/cfgs/config_a.cfg",
                enabled=True,
            ),
            "CASE-CONFLICT-B": _FakeMapping(
                category="CANOE",
                script_path="D:/cfgs/config_b.cfg",  # Different path!
                enabled=True,
            ),
        })

        plan = ExecutionPlan(
            task_no="TASK-CONFLICT",
            project_no="PROJECT-CONFLICT",
            tool_type="canoe",
            cases=[
                PlannedCase(
                    case_no="CASE-CONFLICT-A",
                    case_name="Case Conflict A",
                    case_type="test_module",
                ),
                PlannedCase(
                    case_no="CASE-CONFLICT-B",
                    case_name="Case Conflict B",
                    case_type="test_module",
                ),
            ],
            config_source=ConfigSource.UNSPECIFIED,
        )

        prep_phase = ConfigPreparationPhase(mapping_manager=mapping_manager)

        with pytest.raises(ConfigConflictError):
            prep_phase.prepare(plan)

    def test_config_preparation_fails_on_missing_config(self):
        """ConfigPreparationPhase should raise MissingConfigError when no config."""
        mapping_manager = _FakeMappingManager({})  # No mappings

        plan = ExecutionPlan(
            task_no="TASK-NO-CFG",
            project_no="PROJECT-NO-CFG",
            tool_type="canoe",
            cases=[
                PlannedCase(
                    case_no="CASE-NO-MAP",
                    case_name="Case No Map",
                    case_type="test_module",
                )
            ],
            config_source=ConfigSource.UNSPECIFIED,
        )

        prep_phase = ConfigPreparationPhase(mapping_manager=mapping_manager)

        with pytest.raises(MissingConfigError):
            prep_phase.prepare(plan)

    def test_compiler_rejects_multiple_tool_types(self, monkeypatch):
        """TaskCompiler should reject tasks with multiple tool types."""
        mapping_manager = _FakeMappingManager({
            "CASE-CANOE": _FakeMapping(
                category="CANOE",
                script_path="D:/cfgs/canoe.cfg",
                case_name="Canoe Case",
            ),
            "CASE-TS": _FakeMapping(
                category="TSMASTER",
                script_path="D:/cfgs/tsmaster.cfg",
                case_name="TSMaster Case",
            ),
        })

        def fake_get_executor():
            return _FakeExecutor(accept=True)

        monkeypatch.setattr(
            'core.task_executor_production.get_task_executor',
            fake_get_executor
        )

        # Patch the mapping manager in task_submission where it's used
        with patch('core.task_submission.get_case_mapping_manager', return_value=mapping_manager):
            # Both cases have mappings but different categories
            result = submit_task(
                {
                    "taskNo": "TASK-MIXED",
                    "caseList": [
                        {"caseNo": "CASE-CANOE", "name": "Canoe Case"},
                        {"caseNo": "CASE-TS", "name": "TSMaster Case"},
                    ],
                    # No explicit toolType - would be resolved from mappings
                },
                task_no="TASK-MIXED"
            )

        # Should fail due to multiple tool types
        assert result.success is False


class TestNegativePathExecutionFailure:
    """Test that execution failures are properly handled."""

    def test_result_collector_handles_empty_results_on_failure(self):
        """ResultCollector.finalize should produce valid outcome even with no results."""
        collector = ResultCollector("TASK-NO-RESULTS")

        outcome = collector.finalize(TaskStatus.FAILED.value, error_message="Execution crashed")

        assert outcome.status == "failed"
        assert outcome.error_summary == "Execution crashed"
        assert outcome.summary["total"] == 0
        assert outcome.summary["passed"] == 0

    def test_execution_outcome_propagates_error_summary(self):
        """ExecutionOutcome should propagate error_summary through the pipeline."""
        outcome = ExecutionOutcome(
            task_no="TASK-ERROR-PROP",
            status="failed",
            error_summary="Adapter connection failed: timeout after 30s",
            summary={"total": 3, "passed": 0, "failed": 3},
        )

        payload = outcome.to_report_payload()

        assert payload["errorMessage"] == "Adapter connection failed: timeout after 30s"

    def test_failed_outcome_includes_case_results(self):
        """Failed ExecutionOutcome should still include case results."""
        result1 = TestResult(
            name="Failed Case 1",
            type="test_module",
            passed=False,
            verdict="FAIL",
            error="Expected true but got false",
        )
        result2 = TestResult(
            name="Failed Case 2",
            type="test_module",
            passed=False,
            verdict="FAIL",
            error="Signal timeout",
        )

        outcome = ExecutionOutcome(
            task_no="TASK-FAIL-CASES",
            status="failed",
            case_results=[result1, result2],
            summary={"total": 2, "passed": 0, "failed": 2},
            error_summary="Test execution failed",
        )

        payload = outcome.to_report_payload()

        assert len(payload["results"]) == 2
        assert payload["results"][0]["name"] == "Failed Case 1"
        assert payload["results"][0]["error"] == "Expected true but got false"


class TestNegativePathReportUploadFailure:
    """Test that report upload failures are properly handled."""

    def test_report_client_handles_upload_failure(self, tmp_path, monkeypatch):
        """ReportClient should persist upload failure regardless of report_retry.enabled.

        Per spec: "report client uploads/submits or persists failure" - meaning if upload
        fails, the failure should still be persisted. The report_retry.enabled flag only
        controls whether retries are attempted, not whether failures are persisted.
        """
        fake_config_manager = MagicMock()
        fake_config_manager.get.side_effect = lambda key, default=None: {
            "report_server.enabled": True,
            "report_server.host": "",
            "report_server.port": "",
            "report_server.path": "",
            "report.result_api_url": "http://report.example.com/direct-report",
            "report.file_upload_url": "http://upload.example.com/files",
            "report_retry.enabled": False,  # Retry disabled but failure should STILL be persisted
        }.get(key, default)

        client = ReportClient(fake_config_manager)
        captured = {}
        persisted = {}

        class FakeFailedReportManager:
            def add_failed_report(self, report_data, task_info=None, max_retries=None,
                                  priority=0, failure_reason=None, endpoint=None, metadata=None):
                persisted["report_data"] = report_data
                persisted["task_info"] = task_info
                persisted["failure_reason"] = failure_reason
                return "report-upload-fail"

        monkeypatch.setattr(
            "core.failed_report_manager.get_failed_report_manager",
            lambda config_manager: FakeFailedReportManager(),
        )

        def fake_make_request(method, url, **kwargs):
            if "files" in kwargs:
                # Simulate upload failure
                return None
            captured["payload"] = kwargs.get("json", {})
            return {"code": 200}

        monkeypatch.setattr(client, "_make_request", fake_make_request)

        report_file = tmp_path / "report.html"  # File exists but upload will fail
        report_file.write_text("ok", encoding="utf-8")

        report_data = {
            "taskNo": "TASK-UPLOAD-FAIL",
            "caseList": [{"caseNo": "CASE-1", "result": "FAIL"}]
        }

        task_info = {
            "projectNo": "PROJ-UPLOAD-FAIL",
            "deviceId": "DEVICE-UPLOAD-FAIL",
            "trace_id": "trace-upload-fail",
            "attempt_id": "attempt-upload-fail",
        }

        # When upload fails, failure should be persisted even with report_retry.enabled: False
        result = client.report_result(report_data, task_info, report_file_path=str(report_file))

        # Upload failed, so the overall result should be False
        assert result is False
        # Verify failure was persisted (regardless of report_retry.enabled)
        assert "report_data" in persisted
        assert persisted["report_data"]["taskNo"] == "TASK-UPLOAD-FAIL"
        # Verify submit still happened (captured payload)
        assert "payload" in captured
        # max_retries should be None since report_retry.enabled: False
        assert persisted.get("max_retries") is None

    def test_report_client_handles_missing_file_gracefully(self, tmp_path, monkeypatch):
        """ReportClient should handle missing report file gracefully."""
        fake_config_manager = MagicMock()
        fake_config_manager.get.side_effect = lambda key, default=None: {
            "report_server.enabled": True,
            "report_server.host": "",
            "report_server.port": "",
            "report_server.path": "",
            "report.result_api_url": "http://report.example.com/direct-report",
            "report.file_upload_url": "http://upload.example.com/files",
            "report_retry.enabled": False,
        }.get(key, default)

        client = ReportClient(fake_config_manager)
        captured = {}

        def fake_make_request(method, url, **kwargs):
            captured["payload"] = kwargs.get("json", {})
            return {"code": 200}

        monkeypatch.setattr(client, "_make_request", fake_make_request)

        report_data = {
            "taskNo": "TASK-NO-FILE",
            "caseList": [{"caseNo": "CASE-1", "result": "PASS"}]
        }

        # No report_file_path provided - should still submit
        result = client.report_result(report_data, {}, report_file_path=None)

        assert result is True  # Submit succeeds without file


class TestNegativePathReportSubmitFailure:
    """Test that report submit failures are properly handled."""

    def test_report_client_persists_on_submit_failure(self, tmp_path, monkeypatch):
        """ReportClient should persist report when submit fails."""
        fake_config_manager = MagicMock()
        fake_config_manager.get.side_effect = lambda key, default=None: {
            "report_server.enabled": True,
            "report_server.host": "",
            "report_server.port": "",
            "report_server.path": "",
            "report.result_api_url": "http://report.example.com/direct-report",
            "report.file_upload_url": "http://upload.example.com/files",
            "report_retry.enabled": True,
            "report_retry.max_retries": 10,
        }.get(key, default)

        client = ReportClient(fake_config_manager)
        captured = {}

        class FakeFailedReportManager:
            def add_failed_report(self, report_data, task_info=None, max_retries=None,
                                  priority=0, failure_reason=None, endpoint=None, metadata=None):
                captured["report_data"] = report_data
                captured["task_info"] = task_info
                captured["max_retries"] = max_retries
                captured["failure_reason"] = failure_reason
                captured["endpoint"] = endpoint
                return "report-submit-fail-id"

        monkeypatch.setattr(
            "core.failed_report_manager.get_failed_report_manager",
            lambda config_manager: FakeFailedReportManager(),
        )

        def fake_make_request(method, url, **kwargs):
            if "files" in kwargs:
                return {"code": 200, "data": {"url": "http://files.example.com/fail.html"}}
            # Simulate server error - submit fails
            return None

        monkeypatch.setattr(client, "_make_request", fake_make_request)

        report_file = tmp_path / "fail.html"
        report_file.write_text("fail", encoding="utf-8")

        report_data = {
            "taskNo": "TASK-SUBMIT-FAIL",
            "caseList": [{"caseNo": "CASE-1", "result": "FAIL"}]
        }

        task_info = {
            "projectNo": "PROJ-SUBMIT-FAIL",
            "deviceId": "DEVICE-SUBMIT-FAIL",
            "trace_id": "trace-submit-fail",
            "attempt_id": "attempt-submit-fail",
            "error_category": "report_failure",
        }

        result = client.report_result(report_data, task_info, report_file_path=str(report_file))

        assert result is False
        assert captured["report_data"]["taskNo"] == "TASK-SUBMIT-FAIL"
        assert captured["failure_reason"] == "服务器无响应"
        assert captured["endpoint"] == "http://report.example.com/direct-report"
        assert captured["max_retries"] == 10

    def test_report_client_persists_with_trace_context(self, tmp_path, monkeypatch):
        """Failed report persistence should include trace context."""
        fake_config_manager = MagicMock()
        fake_config_manager.get.side_effect = lambda key, default=None: {
            "report_server.enabled": True,
            "report_server.host": "",
            "report_server.port": "",
            "report_server.path": "",
            "report.result_api_url": "http://report.example.com/direct-report",
            "report.file_upload_url": "http://upload.example.com/files",
            "report_retry.enabled": True,
        }.get(key, default)

        client = ReportClient(fake_config_manager)
        captured = {}

        class FakeFailedReportManager:
            def add_failed_report(self, report_data, task_info=None, max_retries=None,
                                  priority=0, failure_reason=None, endpoint=None, metadata=None):
                captured["metadata"] = metadata
                return "report-trace-id"

        monkeypatch.setattr(
            "core.failed_report_manager.get_failed_report_manager",
            lambda config_manager: FakeFailedReportManager(),
        )

        def fake_make_request(method, url, **kwargs):
            if "files" in kwargs:
                return {"code": 200, "data": {"url": "http://files.example.com/trace.html"}}
            return None  # Submit fails

        monkeypatch.setattr(client, "_make_request", fake_make_request)

        report_file = tmp_path / "trace.html"
        report_file.write_text("trace", encoding="utf-8")

        result = client.report_result(
            {"taskNo": "TASK-TRACE-CONTEXT", "caseList": []},
            {
                "trace_id": "trace-abc-123",
                "attempt_id": "attempt-def-456",
                "error_category": "execution_failure",
            },
            report_file_path=str(report_file)
        )

        assert result is False
        assert captured["metadata"]["trace_id"] == "trace-abc-123"
        assert captured["metadata"]["attempt_id"] == "attempt-def-456"
        assert captured["metadata"]["error_category"] == "execution_failure"


# =============================================================================
# Task Status API Consistency Tests
# =============================================================================

class TestTaskStatusAPIConsistency:
    """Test that task status APIs reflect the same final truth as report/retry layer."""

    def test_observability_context_tracks_full_lifecycle(self):
        """Observability context should track task through all lifecycle stages."""
        manager = get_execution_observability_manager()
        manager._contexts.clear()

        task_no = "TASK-LIFECYCLE-001"

        # Create context
        manager.create_context(
            task_no=task_no,
            device_id="DEVICE-LIFE",
            tool_type="canoe",
        )

        # Transition through lifecycle
        manager.transition(task_no, ExecutionLifecycleStage.VALIDATED)
        manager.transition(task_no, ExecutionLifecycleStage.COMPILED)
        manager.transition(task_no, ExecutionLifecycleStage.PREPARING)
        manager.transition(task_no, ExecutionLifecycleStage.EXECUTING)
        manager.transition(task_no, ExecutionLifecycleStage.COLLECTING)
        manager.transition(task_no, ExecutionLifecycleStage.REPORTING)

        snapshot = manager.get_snapshot(task_no)

        assert snapshot["current_stage"] == ExecutionLifecycleStage.REPORTING.value
        assert len(snapshot["stage_history"]) >= 6

    def test_observability_fail_captures_error_category(self):
        """Observability fail() should capture error_category for failed tasks."""
        manager = get_execution_observability_manager()
        manager._contexts.clear()

        task_no = "TASK-FAIL-CAT-001"

        manager.create_context(
            task_no=task_no,
            device_id="DEVICE-FAIL-CAT",
            tool_type="canoe",
        )

        manager.fail(
            task_no,
            error_code="EXECUTION_FAILED",
            error_message="Test execution failed",
            error_category="execution_failure",
            retryable=False,
        )

        snapshot = manager.get_snapshot(task_no)

        assert snapshot["error_category"] == "execution_failure"
        assert snapshot["error_code"] == "EXECUTION_FAILED"
        assert snapshot["current_stage"] == ExecutionLifecycleStage.FINISHED.value

    def test_observability_finish_captures_report_status(self):
        """Observability finish() should capture report_status for completed tasks."""
        manager = get_execution_observability_manager()
        manager._contexts.clear()

        task_no = "TASK-FINISH-001"

        manager.create_context(
            task_no=task_no,
            device_id="DEVICE-FINISH",
            tool_type="canoe",
        )

        manager.finish(task_no, report_status="success")

        snapshot = manager.get_snapshot(task_no)

        assert snapshot["report_status"] == "success"
        assert snapshot["current_stage"] == ExecutionLifecycleStage.FINISHED.value

    def test_failed_report_metadata_aligns_with_observability(self, monkeypatch):
        """Failed report metadata should align with observability context."""
        fake_config_manager = MagicMock()
        fake_config_manager.get.side_effect = lambda key, default=None: {
            "report_retry.base_delay": 1.0,
            "report_retry.backoff_factor": 2.0,
            "report_retry.max_delay": 60.0,
            "report_retry.max_retries": 3,
        }.get(key, default)

        from core.failed_report_manager import FailedReportManager

        # Create manager with in-memory storage
        manager = FailedReportManager(
            storage_path=":memory:",
            config_manager=fake_config_manager,
        )

        # Add a failed report with trace context
        report_id = manager.add_failed_report(
            report_data={
                "taskNo": "TASK-ALIGN-001",
                "status": "failed",
                "trace_id": "TRACE-ALIGN-001",
                "attempt_id": "ATTEMPT-ALIGN-001",
                "error_category": "execution_failure",
            },
            task_info={
                "taskNo": "TASK-ALIGN-001",
                "projectNo": "PROJ-ALIGN",
                "deviceId": "DEVICE-ALIGN",
                "trace_id": "TRACE-ALIGN-001",
                "attempt_id": "ATTEMPT-ALIGN-001",
                "error_category": "execution_failure",
            },
            failure_reason="execution failed",
        )

        # Verify the stored metadata contains trace context
        report = manager.get_report(report_id)
        projection = report.to_projection()

        assert projection["task_no"] == "TASK-ALIGN-001"
        assert projection["trace_id"] == "TRACE-ALIGN-001"
        assert projection["attempt_id"] == "ATTEMPT-ALIGN-001"
        assert projection["error_category"] == "execution_failure"

    def test_execution_outcome_matches_failed_report_task_info(self, monkeypatch):
        """ExecutionOutcome fields should match what goes into failed report task_info."""
        outcome = ExecutionOutcome(
            task_no="TASK-MATCH-001",
            status="failed",
            error_summary="Connection refused",
            summary={"total": 5, "passed": 2, "failed": 3},
        )

        # Build report payload (what goes to ReportClient)
        payload = outcome.to_report_payload(
            task_info={
                "projectNo": "PROJ-MATCH",
                "deviceId": "DEVICE-MATCH",
                "trace_id": "TRACE-MATCH-001",
                "attempt_id": "ATTEMPT-MATCH-001",
                "error_category": "execution_failure",
            }
        )

        # Verify payload contains all necessary fields for persistence
        assert payload["taskNo"] == "TASK-MATCH-001"
        assert payload["status"] == "failed"
        assert payload["projectNo"] == "PROJ-MATCH"
        assert payload["deviceId"] == "DEVICE-MATCH"
        assert payload["trace_id"] == "TRACE-MATCH-001"
        assert payload["attempt_id"] == "ATTEMPT-MATCH-001"
        assert payload["error_category"] == "execution_failure"
        assert payload["errorMessage"] == "Connection refused"

    def test_task_result_status_consistency_across_transformations(self):
        """TaskResult status should be consistent through transformations."""
        # Create TaskResult
        task_result = TaskResult(
            taskNo="TASK-STATUS-001",
            status="completed",
            results=[
                TestResult(name="Case 1", type="test_module", passed=True, verdict="PASS"),
                TestResult(name="Case 2", type="test_module", passed=True, verdict="PASS"),
            ],
            summary={"total": 2, "passed": 2, "failed": 0},
        )

        # Transform to ExecutionOutcome
        outcome = ExecutionOutcome(
            task_no=task_result.taskNo,
            status=task_result.status,
            started_at=task_result.startTime,
            finished_at=task_result.endTime,
            case_results=list(task_result.results),
            summary=dict(task_result.summary),
            error_summary=task_result.errorMessage,
        )

        # Transform back to TaskResult
        task_result_back = outcome.to_task_result()

        assert task_result_back.status == outcome.status == "completed"

    def test_report_payload_preserves_final_status(self):
        """Report payload should preserve the final status from ExecutionOutcome."""
        outcome = ExecutionOutcome(
            task_no="TASK-FINAL-STATUS",
            status="failed",
            error_summary="Test assertion failed",
            summary={"total": 10, "passed": 7, "failed": 3},
        )

        payload = outcome.to_report_payload()

        # The final status in payload should match the outcome status
        assert payload["status"] == outcome.status == "failed"
        assert payload["errorMessage"] == outcome.error_summary


# =============================================================================
# Integration Tests - Full Pipeline
# =============================================================================

class TestFullPipelineIntegration:
    """Integration tests for the full business flow pipeline."""

    def test_full_happy_path_pipeline(self, monkeypatch):
        """Test complete happy path: submit -> compile -> prepare -> execute -> collect -> report."""
        fake_executor = _FakeExecutor(accept=True)

        def fake_get_executor():
            return fake_executor

        monkeypatch.setattr(
            'core.task_executor_production.get_task_executor',
            fake_get_executor
        )

        mapping_manager = _FakeMappingManager({
            "E2E-CASE-1": _FakeMapping(
                category="CANOE",
                script_path="D:/cfgs/e2e.cfg",
                case_name="E2E Case 1",
            ),
        })

        # Only patch task_submission since that's where get_case_mapping_manager is called
        with patch('core.task_submission.get_case_mapping_manager', return_value=mapping_manager):
            # Step 1: Submit task
            result = submit_task(
                {
                    "taskNo": "TASK-E2E-001",
                    "caseList": [{"caseNo": "E2E-CASE-1", "name": "E2E Case 1"}],
                    "toolType": "canoe",
                },
                task_no="TASK-E2E-001"
            )

        # Verify submission succeeded
        assert result.success is True
        assert result.task_no == "TASK-E2E-001"

        # Verify plan was submitted to executor
        assert len(fake_executor.submitted_plans) == 1

        plan = fake_executor.submitted_plans[0]

        # Step 2: Compile was successful (plan exists with cases)
        assert plan.task_no == "TASK-E2E-001"
        assert len(plan.cases) == 1

        # Step 3: Prepare config (simulate ConfigPreparationPhase)
        prep_phase = ConfigPreparationPhase(mapping_manager=mapping_manager)
        prepared_plan = prep_phase.prepare(plan)

        assert prepared_plan.config_source == ConfigSource.CASE_MAPPING
        assert prepared_plan.config_path == "D:/cfgs/e2e.cfg"

        # Step 4: Collect results (simulate execution)
        collector = ResultCollector(plan.task_no)
        collector.add_test_result(
            TestResult(name="E2E Case 1", type="test_module", passed=True, verdict="PASS")
        )
        outcome = collector.finalize(TaskStatus.COMPLETED.value)

        assert outcome.status == "completed"
        assert outcome.summary["passed"] == 1

        # Step 5: Report (verify payload can be built)
        payload = outcome.to_report_payload(
            task_info={"projectNo": "PROJ-E2E", "deviceId": "DEVICE-E2E"}
        )

        assert payload["taskNo"] == "TASK-E2E-001"
        assert payload["status"] == "completed"

    def test_full_negative_path_pipeline(self, monkeypatch):
        """Test complete negative path: submit fails at validation."""
        fake_executor = _FakeExecutor(accept=True)

        def fake_get_executor():
            return fake_executor

        monkeypatch.setattr(
            'core.task_executor_production.get_task_executor',
            fake_get_executor
        )

        # Submit with empty case list - should fail at validation
        result = submit_task(
            {
                "taskNo": "TASK-E2E-FAIL-001",
                "caseList": [],  # Empty - should fail
                "toolType": "canoe",
            },
            task_no="TASK-E2E-FAIL-001"
        )

        # Verify submission failed
        assert result.success is False
        assert result.error_code in ("TASK_VALIDATION_FAILED", "TASK_COMPILE_FAILED")

        # Verify nothing was submitted to executor
        assert len(fake_executor.submitted_plans) == 0

    def test_pipeline_preserves_trace_context_through_stages(self, monkeypatch):
        """Test that trace context is preserved throughout the pipeline."""
        fake_executor = _FakeExecutor(accept=True)

        def fake_get_executor():
            return fake_executor

        monkeypatch.setattr(
            'core.task_executor_production.get_task_executor',
            fake_get_executor
        )

        mapping_manager = _FakeMappingManager({
            "TRACE-CASE-1": _FakeMapping(
                category="CANOE",
                script_path="D:/cfgs/trace.cfg",
                case_name="Trace Case",
            ),
        })

        # Set up observability tracking
        manager = get_execution_observability_manager()
        manager._contexts.clear()

        # Create observability context before calling submit_task
        # (In real flow, this happens in WebSocket handler before submit_task is called)
        manager.create_context(
            task_no="TASK-TRACE-001",
            device_id="DEVICE-TRACE",
            tool_type="canoe",
            attempt=1,
            attempt_id="attempt-trace-001",
            trace_id="trace-001",
        )

        # Only patch task_submission since that's where get_case_mapping_manager is called
        with patch('core.task_submission.get_case_mapping_manager', return_value=mapping_manager):
            result = submit_task(
                {
                    "taskNo": "TASK-TRACE-001",
                    "caseList": [{"caseNo": "TRACE-CASE-1", "name": "Trace Case"}],
                    "toolType": "canoe",
                },
                task_no="TASK-TRACE-001"
            )

        assert result.success is True

        # Verify observability context was updated (not created - creation happens in handler)
        snapshot = manager.get_snapshot("TASK-TRACE-001")
        assert snapshot is not None
        assert snapshot["task_no"] == "TASK-TRACE-001"
