from pathlib import Path
import sys

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from core.result_collector import ResultCollector, ResultFormatter
from models.result import ExecutionOutcome
from models.task import TestResult as CaseTestResult


def test_result_collector_finalize_returns_execution_outcome():
    collector = ResultCollector(taskNo="TASK-OUTCOME")
    collector.add_test_result(
        CaseTestResult(name="CASE-1", type="test_module", passed=True, verdict="PASS")
    )
    collector.add_test_result(
        CaseTestResult(name="CASE-2", type="test_module", passed=False, verdict="FAIL", error="boom")
    )

    outcome = collector.finalize(status="failed", error_message="task failed")

    assert isinstance(outcome, ExecutionOutcome)
    assert outcome.task_no == "TASK-OUTCOME"
    assert outcome.status == "failed"
    assert outcome.error_summary == "task failed"
    assert outcome.summary["total"] == 2
    assert outcome.summary["failed"] == 1
    assert outcome.summary["passRate"] == "50.0%"
    assert "pass_rate" not in outcome.summary
    assert "startedAt" in outcome.summary
    assert "finishedAt" in outcome.summary
    assert len(outcome.case_results) == 2


def test_execution_outcome_can_project_to_legacy_task_result():
    collector = ResultCollector(taskNo="TASK-LEGACY")
    collector.add_test_result(
        CaseTestResult(name="CASE-LEGACY", type="test_module", passed=True, verdict="PASS")
    )

    outcome = collector.finalize(status="completed")
    task_result = outcome.to_task_result()

    assert task_result.taskNo == "TASK-LEGACY"
    assert task_result.status == "completed"
    assert task_result.summary["passed"] == 1
    assert len(task_result.results) == 1


def test_result_formatter_accepts_execution_outcome():
    collector = ResultCollector(taskNo="TASK-FORMAT")
    collector.add_test_result(
        CaseTestResult(name="CASE-FORMAT", type="test_module", passed=True, verdict="PASS")
    )

    outcome = collector.finalize(status="completed")
    formatted = ResultFormatter.format_detailed_results(outcome)

    assert "任务执行摘要 - TASK-FORMAT" in formatted
    assert "CASE-FORMAT" in formatted


def test_result_collector_all_pass_increments_pass_count():
    """Verify all PASS results produce correct pass count."""
    collector = ResultCollector(taskNo="TASK-ALL-PASS")
    verdicts = ["PASS", "PASSED", "SUCCESS", "Pass", "pass"]

    for i, verdict in enumerate(verdicts):
        collector.add_test_result(
            CaseTestResult(name=f"CASE-{i}", type="test_module", passed=True, verdict=verdict)
        )

    outcome = collector.finalize(status="completed")

    assert outcome.summary["total"] == 5
    assert outcome.summary["passed"] == 5
    assert outcome.summary["failed"] == 0
    assert outcome.summary["passRate"] == "100.0%"


def test_result_collector_all_fail_increments_fail_count():
    """Verify all FAIL results produce correct fail count."""
    collector = ResultCollector(taskNo="TASK-ALL-FAIL")
    verdicts = ["FAIL", "ERROR", "FAILED", "Fail", "fail"]

    for i, verdict in enumerate(verdicts):
        collector.add_test_result(
            CaseTestResult(name=f"CASE-{i}", type="test_module", passed=False, verdict=verdict)
        )

    outcome = collector.finalize(status="completed")

    assert outcome.summary["total"] == 5
    assert outcome.summary["passed"] == 0
    assert outcome.summary["failed"] == 5
    assert outcome.summary["passRate"] == "0.0%"


def test_result_collector_mixed_results_stable_summary():
    """Verify mixed pass/fail results produce stable summary."""
    collector = ResultCollector(taskNo="TASK-MIXED")

    test_cases = [
        ("CASE-1", True, "PASS"),
        ("CASE-2", False, "FAIL"),
        ("CASE-3", True, "PASSED"),
        ("CASE-4", True, "SUCCESS"),
        ("CASE-5", False, "FAIL"),
    ]

    for name, passed, verdict in test_cases:
        collector.add_test_result(
            CaseTestResult(name=name, type="test_module", passed=passed, verdict=verdict)
        )

    outcome = collector.finalize(status="completed")

    # 3 passed, 2 failed
    assert outcome.summary["total"] == 5
    assert outcome.summary["passed"] == 3
    assert outcome.summary["failed"] == 2
    assert outcome.summary["passRate"] == "60.0%"
    # Verdict is FAIL since not all tests passed (failed > 0)
    assert outcome.verdict == "FAIL"


def test_result_collector_empty_results():
    """Verify empty results produce correct summary."""
    collector = ResultCollector(taskNo="TASK-EMPTY")

    outcome = collector.finalize(status="completed")

    assert outcome.summary["total"] == 0
    assert outcome.summary["passed"] == 0
    assert outcome.summary["failed"] == 0
    assert outcome.summary["passRate"] == "0%"
    assert outcome.verdict is None


def test_result_collector_none_passed_false_not_counted_as_pass():
    """Verify results with passed=None are not counted as pass."""
    collector = ResultCollector(taskNo="TASK-NONE-PASS")

    # Using None for passed should be treated as failed (not counted as pass)
    collector.add_test_result(
        CaseTestResult(name="CASE-1", type="test_module", passed=None, verdict="UNKNOWN")
    )
    collector.add_test_result(
        CaseTestResult(name="CASE-2", type="test_module", passed=True, verdict="PASS")
    )

    outcome = collector.finalize(status="completed")

    # Only 1 passed (the one with passed=True), the None is not counted as pass
    assert outcome.summary["total"] == 2
    assert outcome.summary["passed"] == 1
    assert outcome.summary["failed"] == 1
