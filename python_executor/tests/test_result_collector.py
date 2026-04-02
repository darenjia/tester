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
