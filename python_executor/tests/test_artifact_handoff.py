"""
Artifact Handoff Tests for Workstream 5: Connect Artifact Handoff To Reporting

These tests verify that:
1. TSMaster report path reaches upload stage via ExecutionOutcome.artifacts
2. TTworkbench report/log path reaches report metadata
3. Missing report artifact does not masquerade as uploaded
"""
from pathlib import Path
import sys

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from core.result_collector import ResultCollector
from core.execution_strategies.tsmaster_strategy import TSMasterExecutionStrategy
from core.execution_strategies.ttworkbench_strategy import TTworkbenchExecutionStrategy
from models.result import ExecutionOutcome
from models.task import TestResult


class _DummyAdapter:
    def __init__(self, capabilities):
        self._capabilities = capabilities

    def get_capability(self, name, default=None):
        return self._capabilities.get(name, default)


# =============================================================================
# TSMaster Artifact Handoff Tests
# =============================================================================

def test_tsmaster_report_path_reaches_execution_outcome_artifacts():
    """Verify TSMaster report_path is published through ExecutionOutcome.artifacts."""
    strategy = TSMasterExecutionStrategy()

    class _Measurement:
        def start(self):
            return True

        def stop(self):
            return True

    class _Execution:
        def build_case_selection(self, plan):
            return "CASE-1=1,CASE-2=1"

        def start_execution(self, selected_cases):
            return True

        def wait_for_completion(self, timeout):
            return True

        def get_report_info(self):
            return {
                "results": [
                    {"case_no": "CASE-1", "passed": True, "verdict": "PASS"},
                    {"case_no": "CASE-2", "passed": False, "verdict": "FAIL"},
                ],
                "report_path": "D:/reports/tsmaster/TASK-001/report.html",
                "testdata_path": "D:/reports/tsmaster/TASK-001/testdata",
            }

    adapter = _DummyAdapter(
        {
            "configuration": object(),
            "measurement": _Measurement(),
            "tsmaster_execution": _Execution(),
        }
    )

    plan = type(
        "_Plan",
        (),
        {
            "task_no": "TASK-TSM-ARTIFACT",
            "timeout_seconds": 30,
            "cases": [
                type("_Case", (), {"case_no": "CASE-1", "name": "Case 1", "case_type": "test_module"})(),
                type("_Case", (), {"case_no": "CASE-2", "name": "Case 2", "case_type": "test_module"})(),
            ],
        },
    )()

    collector = ResultCollector("TASK-TSM-ARTIFACT")

    outcome = strategy.run(plan, adapter=adapter, collector=collector, config_path=None)

    assert isinstance(outcome, ExecutionOutcome)
    # Verify report_path is in artifacts
    assert "report_path" in outcome.artifacts
    assert outcome.artifacts["report_path"] == "D:/reports/tsmaster/TASK-001/report.html"
    # Verify testdata_path is also propagated
    assert "testdata_path" in outcome.artifacts
    assert outcome.artifacts["testdata_path"] == "D:/reports/tsmaster/TASK-001/testdata"
    # Verify report_metadata is populated
    assert outcome.report_metadata.get("source") == "tsmaster_execution"
    assert outcome.report_metadata.get("has_per_case_details") is True


def test_tsmaster_aggregate_only_reports_still_propagate_artifacts():
    """Verify TSMaster aggregate-only reports (no per-case details) still propagate artifacts."""
    strategy = TSMasterExecutionStrategy()

    class _Measurement:
        def start(self):
            return True

        def stop(self):
            return True

    class _Execution:
        def build_case_selection(self, plan):
            return "CASE-1=1,CASE-2=1"

        def start_execution(self, selected_cases):
            return True

        def wait_for_completion(self, timeout):
            return True

        def get_report_info(self):
            # Aggregate only - no per-case results
            return {
                "passed": 1,
                "failed": 1,
                "total": 2,
                "report_path": "D:/reports/tsmaster/TASK-AGG/report.html",
            }

    adapter = _DummyAdapter(
        {
            "configuration": object(),
            "measurement": _Measurement(),
            "tsmaster_execution": _Execution(),
        }
    )

    plan = type(
        "_Plan",
        (),
        {
            "task_no": "TASK-TSM-AGG",
            "timeout_seconds": 30,
            "cases": [
                type("_Case", (), {"case_no": "CASE-1", "name": "Case 1", "case_type": "test_module"})(),
                type("_Case", (), {"case_no": "CASE-2", "name": "Case 2", "case_type": "test_module"})(),
            ],
        },
    )()

    collector = ResultCollector("TASK-TSM-AGG")

    outcome = strategy.run(plan, adapter=adapter, collector=collector, config_path=None)

    assert isinstance(outcome, ExecutionOutcome)
    # Report path should still be propagated even without per-case details
    assert "report_path" in outcome.artifacts
    assert outcome.artifacts["report_path"] == "D:/reports/tsmaster/TASK-AGG/report.html"
    # Metadata should indicate aggregate-only
    assert outcome.report_metadata.get("has_per_case_details") is False
    assert outcome.report_metadata.get("passed") == 1
    assert outcome.report_metadata.get("failed") == 1


# =============================================================================
# TTworkbench Artifact Handoff Tests
# =============================================================================

def test_ttworkbench_report_path_reaches_report_metadata():
    """Verify TTworkbench report_path from execution details reaches report_metadata."""
    strategy = TTworkbenchExecutionStrategy()

    class _Measurement:
        def start(self):
            return True

        def stop(self):
            return True

    class _Configuration:
        def load(self, config_path):
            return True

    class _Execution:
        def execute_clf(self, clf_file, task_id=None):
            return {
                "name": "single",
                "type": "clf_test",
                "clf_file": clf_file,
                "verdict": "PASS",
                "status": "passed",
                "report_path": "D:/reports/ttworkbench/TASK-TTW/report.html",
                "log_path": "D:/reports/ttworkbench/TASK-TTW/log.txt",
            }

    adapter = _DummyAdapter(
        {
            "configuration": _Configuration(),
            "measurement": _Measurement(),
            "ttworkbench_execution": _Execution(),
        }
    )

    plan = type(
        "_Plan",
        (),
        {
            "task_no": "TASK-TTW-ARTIFACT",
            "cases": [
                type(
                    "_Case",
                    (),
                    {
                        "case_name": "Single Case",
                        "case_type": "clf_test",
                        "execution_params": {"clf_file": "D:/workspace/single.clf"},
                    },
                )(),
            ],
        },
    )()

    collector = ResultCollector("TASK-TTW-ARTIFACT")

    outcome = strategy.run(plan, adapter=adapter, collector=collector, config_path="D:/workspace/root.clf")

    assert isinstance(outcome, ExecutionOutcome)
    # Verify artifacts are populated from test result details
    assert "report_path" in outcome.artifacts
    assert outcome.artifacts["report_path"] == "D:/reports/ttworkbench/TASK-TTW/report.html"
    assert "log_path" in outcome.artifacts
    assert outcome.artifacts["log_path"] == "D:/reports/ttworkbench/TASK-TTW/log.txt"
    # Verify report_metadata source
    assert outcome.report_metadata.get("source") == "ttworkbench_execution"


# =============================================================================
# Missing Report Artifact Tests
# =============================================================================

def test_missing_report_artifact_does_not_masquerade_as_uploaded():
    """Verify that when no report artifact exists, upload is not falsely triggered."""
    collector = ResultCollector("TASK-NO-ARTIFACT")
    collector.add_test_result(
        TestResult(name="CASE-1", type="test_module", passed=True, verdict="PASS")
    )

    # Finalize WITHOUT artifacts
    outcome = collector.finalize(status="completed", error_message=None)

    assert isinstance(outcome, ExecutionOutcome)
    # No artifacts should be present
    assert outcome.artifacts == {} or outcome.artifacts is None
    # When artifacts is empty/None, report_file_path will be None
    report_file_path = outcome.artifacts.get('report_path') if outcome.artifacts else None
    assert report_file_path is None


def test_result_collector_finalize_with_artifacts():
    """Verify ResultCollector.finalize() correctly propagates artifacts and report_metadata."""
    collector = ResultCollector("TASK-WITH-ARTIFACTS")
    collector.add_test_result(
        TestResult(name="CASE-1", type="test_module", passed=True, verdict="PASS")
    )

    artifacts = {
        "report_path": "D:/reports/task_123/report.html",
        "log_path": "D:/reports/task_123/log.txt",
    }
    report_metadata = {
        "source": "test_source",
        "uploaded_by": "automation",
    }

    outcome = collector.finalize(
        status="completed",
        artifacts=artifacts,
        report_metadata=report_metadata,
    )

    assert isinstance(outcome, ExecutionOutcome)
    assert outcome.artifacts == artifacts
    assert outcome.report_metadata == report_metadata


def test_execution_outcome_artifacts_accessible_for_upload():
    """Verify ExecutionOutcome.artifacts is accessible for report upload logic."""
    collector = ResultCollector("TASK-UPLOAD-CHECK")
    collector.add_test_result(
        TestResult(name="CASE-1", type="test_module", passed=True, verdict="PASS")
    )

    artifacts = {"report_path": "D:/reports/upload_me.html"}
    outcome = collector.finalize(status="completed", artifacts=artifacts)

    # Simulate the upload logic in _complete_task
    report_file_path = None
    if isinstance(outcome, ExecutionOutcome) and outcome.artifacts:
        report_file_path = outcome.artifacts.get('report_path')

    assert report_file_path == "D:/reports/upload_me.html"


def test_execution_outcome_without_artifacts_upload_check_returns_none():
    """Verify that ExecutionOutcome without artifacts returns None for upload check."""
    collector = ResultCollector("TASK-NO-UPLOAD")
    collector.add_test_result(
        TestResult(name="CASE-1", type="test_module", passed=True, verdict="PASS")
    )

    outcome = collector.finalize(status="completed")

    # Simulate the upload logic in _complete_task
    report_file_path = None
    if isinstance(outcome, ExecutionOutcome) and outcome.artifacts:
        report_file_path = outcome.artifacts.get('report_path')

    assert report_file_path is None


# =============================================================================
# Integration: Strategy -> Collector -> Upload Flow
# =============================================================================

def test_full_artifact_flow_tsmaster_to_upload():
    """Integration test: TSMaster strategy -> ExecutionOutcome.artifacts -> upload path."""
    strategy = TSMasterExecutionStrategy()

    class _Measurement:
        def start(self):
            return True

        def stop(self):
            return True

    class _Execution:
        def build_case_selection(self, plan):
            return "CASE-1=1"

        def start_execution(self, selected_cases):
            return True

        def wait_for_completion(self, timeout):
            return True

        def get_report_info(self):
            return {
                "results": [{"case_no": "CASE-1", "passed": True, "verdict": "PASS"}],
                "report_path": "D:/reports/full_flow/report.html",
            }

    adapter = _DummyAdapter(
        {
            "configuration": object(),
            "measurement": _Measurement(),
            "tsmaster_execution": _Execution(),
        }
    )

    plan = type(
        "_Plan",
        (),
        {
            "task_no": "TASK-FULL-FLOW",
            "timeout_seconds": 30,
            "cases": [
                type("_Case", (), {"case_no": "CASE-1", "name": "Case 1", "case_type": "test_module"})(),
            ],
        },
    )()

    collector = ResultCollector("TASK-FULL-FLOW")

    # Step 1: Strategy runs and populates outcome with artifacts
    outcome = strategy.run(plan, adapter=adapter, collector=collector, config_path=None)

    # Step 2: Verify artifacts are in outcome
    assert outcome.artifacts.get("report_path") == "D:/reports/full_flow/report.html"

    # Step 3: Simulate _complete_task upload logic
    report_file_path = None
    if isinstance(outcome, ExecutionOutcome) and outcome.artifacts:
        report_file_path = outcome.artifacts.get('report_path')

    # Step 4: Verify upload would be triggered with correct path
    assert report_file_path == "D:/reports/full_flow/report.html"
