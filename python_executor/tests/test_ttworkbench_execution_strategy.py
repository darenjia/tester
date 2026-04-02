from core.execution_strategies.ttworkbench_strategy import TTworkbenchExecutionStrategy
from core.result_collector import ResultCollector


class _DummyAdapter:
    def __init__(self, capabilities):
        self._capabilities = capabilities

    def get_capability(self, name, default=None):
        return self._capabilities.get(name, default)


def test_ttworkbench_strategy_requires_execution_capability():
    strategy = TTworkbenchExecutionStrategy()
    adapter = _DummyAdapter(
        {
            "configuration": object(),
            "measurement": object(),
        }
    )

    ok, error = strategy.prepare(plan=None, adapter=adapter)

    assert ok is False
    assert "ttworkbench_execution" in error


def test_ttworkbench_strategy_runs_cases_via_capability():
    """Test that TTworkbench strategy executes cases via capability and returns ExecutionOutcome."""
    strategy = TTworkbenchExecutionStrategy()
    observed = {"loads": [], "starts": 0, "stops": 0, "single": [], "batch": []}

    class _Configuration:
        def load(self, config_path):
            observed["loads"].append(config_path)
            return True

    class _Measurement:
        def start(self):
            observed["starts"] += 1
            return True

        def stop(self):
            observed["stops"] += 1
            return True

    class _Artifact:
        def collect(self):
            return {"report_root": "D:/reports"}

    class _Execution:
        def execute_clf(self, clf_file, task_id=None):
            observed["single"].append((clf_file, task_id))
            return {
                "name": "single",
                "type": "clf_test",
                "clf_file": clf_file,
                "verdict": "PASS",
                "status": "passed",
            }

        def execute_batch(self, clf_files, task_id=None):
            observed["batch"].append((tuple(clf_files), task_id))
            return {
                "name": "batch",
                "type": "batch_test",
                "results": [],
                "passed": len(clf_files),
                "failed": 0,
                "status": "passed",
            }

    adapter = _DummyAdapter(
        {
            "configuration": _Configuration(),
            "measurement": _Measurement(),
            "artifact": _Artifact(),
            "ttworkbench_execution": _Execution(),
        }
    )

    plan = type(
        "_Plan",
        (),
        {
            "task_no": "TTW-1",
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
                type(
                    "_Case",
                    (),
                    {
                        "case_name": "Batch Case",
                        "case_type": "batch_test",
                        "execution_params": {"clf_files": ["D:/workspace/a.clf", "D:/workspace/b.clf"]},
                    },
                )(),
            ],
        },
    )()

    collector = ResultCollector("TTW-1")

    outcome = strategy.run(plan, adapter=adapter, collector=collector, config_path="D:/workspace/root.clf")

    assert observed["loads"] == ["D:/workspace/root.clf"]
    assert observed["starts"] == 1
    assert observed["stops"] == 1
    assert observed["single"] == [("D:/workspace/single.clf", "TTW-1")]
    assert observed["batch"] == [(("D:/workspace/a.clf", "D:/workspace/b.clf"), "TTW-1")]
    # Contract: when collector is provided, return ExecutionOutcome
    assert outcome.taskNo == "TTW-1"
    assert outcome.status == "completed"
    assert len(outcome.results) == 2
    assert outcome.results[0].verdict == "PASS"
    assert outcome.results[1].verdict == "PASS"
    # Verify passed boolean aligns with verdict
    assert outcome.results[0].passed is True
    assert outcome.results[1].passed is True


def test_ttworkbench_pass_result_increments_pass_count():
    """Verify TTworkbench PASS result increments pass count in summary."""
    strategy = TTworkbenchExecutionStrategy()
    observed = {"single": [], "batch": []}

    class _Measurement:
        def start(self):
            return True

        def stop(self):
            return True

    class _Execution:
        def execute_clf(self, clf_file, task_id=None):
            observed["single"].append((clf_file, task_id))
            return {
                "name": "single",
                "type": "clf_test",
                "clf_file": clf_file,
                "verdict": "PASS",
                "status": "passed",
            }

    adapter = _DummyAdapter(
        {
            "configuration": object(),
            "measurement": _Measurement(),
            "ttworkbench_execution": _Execution(),
        }
    )

    plan = type(
        "_Plan",
        (),
        {
            "task_no": "TTW-PASS-001",
            "cases": [
                type(
                    "_Case",
                    (),
                    {
                        "case_name": "Pass Case",
                        "case_type": "clf_test",
                        "execution_params": {"clf_file": "D:/workspace/pass.clf"},
                    },
                )(),
            ],
        },
    )()

    collector = ResultCollector("TTW-PASS-001")

    outcome = strategy.run(plan, adapter=adapter, collector=collector, config_path=None)

    assert outcome.results[0].verdict == "PASS"
    assert outcome.results[0].passed is True
    assert outcome.summary["passed"] == 1
    assert outcome.summary["failed"] == 0


def test_ttworkbench_fail_result_increments_fail_count():
    """Verify TTworkbench FAIL result increments fail count in summary."""
    strategy = TTworkbenchExecutionStrategy()

    class _Measurement:
        def start(self):
            return True

        def stop(self):
            return True

    class _Execution:
        def execute_clf(self, clf_file, task_id=None):
            return {
                "name": "single",
                "type": "clf_test",
                "clf_file": clf_file,
                "verdict": "FAIL",
                "status": "failed",
            }

    adapter = _DummyAdapter(
        {
            "configuration": object(),
            "measurement": _Measurement(),
            "ttworkbench_execution": _Execution(),
        }
    )

    plan = type(
        "_Plan",
        (),
        {
            "task_no": "TTW-FAIL-001",
            "cases": [
                type(
                    "_Case",
                    (),
                    {
                        "case_name": "Fail Case",
                        "case_type": "clf_test",
                        "execution_params": {"clf_file": "D:/workspace/fail.clf"},
                    },
                )(),
            ],
        },
    )()

    collector = ResultCollector("TTW-FAIL-001")

    outcome = strategy.run(plan, adapter=adapter, collector=collector, config_path=None)

    assert outcome.results[0].verdict == "FAIL"
    assert outcome.results[0].passed is False
    assert outcome.summary["passed"] == 0
    assert outcome.summary["failed"] == 1


def test_ttworkbench_mixed_results_produce_stable_summary():
    """Verify TTworkbench mixed pass/fail results produce stable summary."""
    strategy = TTworkbenchExecutionStrategy()
    case_index = [0]
    verdicts = ["PASS", "FAIL", "PASS", "PASSED", "FAIL"]

    class _Measurement:
        def start(self):
            return True

        def stop(self):
            return True

    class _Execution:
        def execute_clf(self, clf_file, task_id=None):
            idx = case_index[0]
            case_index[0] += 1
            return {
                "name": f"case_{idx}",
                "type": "clf_test",
                "clf_file": clf_file,
                "verdict": verdicts[idx],
                "status": "passed" if verdicts[idx].upper() in {"PASS", "PASSED", "SUCCESS"} else "failed",
            }

    adapter = _DummyAdapter(
        {
            "configuration": object(),
            "measurement": _Measurement(),
            "ttworkbench_execution": _Execution(),
        }
    )

    plan = type(
        "_Plan",
        (),
        {
            "task_no": "TTW-MIXED-001",
            "cases": [
                type(
                    "_Case",
                    (),
                    {
                        "case_name": f"Case {i}",
                        "case_type": "clf_test",
                        "execution_params": {"clf_file": f"D:/workspace/case_{i}.clf"},
                    },
                )()
                for i in range(5)
            ],
        },
    )()

    collector = ResultCollector("TTW-MIXED-001")

    outcome = strategy.run(plan, adapter=adapter, collector=collector, config_path=None)

    # verdicts: PASS, FAIL, PASS, PASSED, FAIL -> 3 pass, 2 fail
    assert outcome.summary["total"] == 5
    assert outcome.summary["passed"] == 3
    assert outcome.summary["failed"] == 2
    # Verify each result's passed aligns with verdict
    for i, result in enumerate(outcome.results):
        expected_passed = verdicts[i].upper() in {"PASS", "PASSED", "SUCCESS"}
        assert result.passed is expected_passed, f"Result {i} passed mismatch: {result.passed} vs verdict {verdicts[i]}"
