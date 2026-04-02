from core.execution_strategies.canoe_strategy import CANoeExecutionStrategy
from core.result_collector import ResultCollector


class _DummyAdapter:
    def __init__(self, capabilities):
        self._capabilities = capabilities

    def get_capability(self, name, default=None):
        return self._capabilities.get(name, default)


def test_canoe_strategy_requires_testmodule_capability():
    strategy = CANoeExecutionStrategy()
    adapter = _DummyAdapter(
        {
            "configuration": object(),
            "measurement": object(),
        }
    )

    ok, error = strategy.prepare(plan=None, adapter=adapter)

    assert ok is False
    assert "test_module" in error.lower()


def test_canoe_strategy_prepare_accepts_registered_capabilities():
    strategy = CANoeExecutionStrategy()
    adapter = _DummyAdapter(
        {
            "configuration": object(),
            "measurement": object(),
            "test_module": object(),
        }
    )

    ok, error = strategy.prepare(plan=None, adapter=adapter)

    assert ok is True
    assert error is None


def test_canoe_strategy_runs_test_modules_via_capability():
    """Test that CANoe strategy executes test modules via capability and returns ExecutionOutcome."""
    strategy = CANoeExecutionStrategy()
    executed = []

    class _TestModuleCapability:
        def execute_module(self, module_name, timeout=None):
            executed.append((module_name, timeout))
            return {"verdict": "PASS", "details": {"module": module_name}}

    class _Adapter:
        def get_capability(self, name, default=None):
            mapping = {
                "configuration": object(),
                "measurement": type(
                    "_Measurement",
                    (),
                    {"start": staticmethod(lambda: True), "stop": staticmethod(lambda: True)},
                )(),
                "test_module": _TestModuleCapability(),
            }
            return mapping.get(name, default)

    class _Plan:
        timeout_seconds = 45
        cases = [
            type("_Case", (), {"case_name": "ModuleA", "case_type": "test_module"})(),
            type("_Case", (), {"case_name": "ModuleB", "case_type": "test_module"})(),
        ]

    collector = ResultCollector("CANOE-TEST-001")

    outcome = strategy.run(_Plan(), adapter=_Adapter(), collector=collector, config_path=None)

    assert executed == [("ModuleA", 45), ("ModuleB", 45)]
    # Contract: when collector is provided, return ExecutionOutcome
    assert outcome.taskNo == "CANOE-TEST-001"
    assert outcome.status == "completed"
    assert len(outcome.results) == 2
    assert outcome.results[0].verdict == "PASS"


def test_canoe_pass_verdict_sets_passed_true():
    """Verify CANoe PASS verdict correctly sets passed=True."""
    strategy = CANoeExecutionStrategy()

    class _TestModuleCapability:
        def execute_module(self, module_name, timeout=None):
            return {"verdict": "PASS", "details": {"module": module_name}}

    class _Adapter:
        def get_capability(self, name, default=None):
            mapping = {
                "configuration": object(),
                "measurement": type(
                    "_Measurement",
                    (),
                    {"start": staticmethod(lambda: True), "stop": staticmethod(lambda: True)},
                )(),
                "test_module": _TestModuleCapability(),
            }
            return mapping.get(name, default)

    class _Plan:
        timeout_seconds = 30
        cases = [
            type("_Case", (), {"case_name": "ModuleA", "case_type": "test_module"})(),
        ]

    collector = ResultCollector("CANOE-PASS-001")

    outcome = strategy.run(_Plan(), adapter=_Adapter(), collector=collector, config_path=None)

    assert outcome.results[0].verdict == "PASS"
    assert outcome.results[0].passed is True
    assert outcome.summary["passed"] == 1
    assert outcome.summary["failed"] == 0


def test_canoe_fail_verdict_sets_passed_false():
    """Verify CANoe FAIL verdict correctly sets passed=False."""
    strategy = CANoeExecutionStrategy()

    class _TestModuleCapability:
        def execute_module(self, module_name, timeout=None):
            return {"verdict": "FAIL", "details": {"module": module_name}}

    class _Adapter:
        def get_capability(self, name, default=None):
            mapping = {
                "configuration": object(),
                "measurement": type(
                    "_Measurement",
                    (),
                    {"start": staticmethod(lambda: True), "stop": staticmethod(lambda: True)},
                )(),
                "test_module": _TestModuleCapability(),
            }
            return mapping.get(name, default)

    class _Plan:
        timeout_seconds = 30
        cases = [
            type("_Case", (), {"case_name": "ModuleA", "case_type": "test_module"})(),
        ]

    collector = ResultCollector("CANOE-FAIL-001")

    outcome = strategy.run(_Plan(), adapter=_Adapter(), collector=collector, config_path=None)

    assert outcome.results[0].verdict == "FAIL"
    assert outcome.results[0].passed is False
    assert outcome.summary["passed"] == 0
    assert outcome.summary["failed"] == 1


def test_canoe_mixed_results_produce_stable_summary():
    """Verify CANoe mixed pass/fail results produce stable summary."""
    strategy = CANoeExecutionStrategy()
    module_index = [0]
    verdicts = ["PASS", "FAIL", "PASS", "PASSED", "FAIL"]

    class _TestModuleCapability:
        def execute_module(self, module_name, timeout=None):
            idx = module_index[0]
            module_index[0] += 1
            return {"verdict": verdicts[idx], "details": {"module": module_name}}

    class _Adapter:
        def get_capability(self, name, default=None):
            mapping = {
                "configuration": object(),
                "measurement": type(
                    "_Measurement",
                    (),
                    {"start": staticmethod(lambda: True), "stop": staticmethod(lambda: True)},
                )(),
                "test_module": _TestModuleCapability(),
            }
            return mapping.get(name, default)

    class _Plan:
        timeout_seconds = 30
        cases = [
            type("_Case", (), {"case_name": f"Module{i}", "case_type": "test_module"})()
            for i in range(5)
        ]

    collector = ResultCollector("CANOE-MIXED-001")

    outcome = strategy.run(_Plan(), adapter=_Adapter(), collector=collector, config_path=None)

    # verdicts: PASS, FAIL, PASS, PASSED, FAIL -> 3 pass, 2 fail
    assert outcome.summary["total"] == 5
    assert outcome.summary["passed"] == 3
    assert outcome.summary["failed"] == 2
    # Verify each result's passed aligns with verdict
    for i, result in enumerate(outcome.results):
        expected_passed = verdicts[i].upper() in {"PASS", "PASSED", "SUCCESS"}
        assert result.passed is expected_passed, f"Result {i} passed mismatch: {result.passed} vs verdict {verdicts[i]}"


def test_canoe_strategy_rejects_non_test_module_cases():
    strategy = CANoeExecutionStrategy()

    class _Adapter:
        def get_capability(self, name, default=None):
            mapping = {
                "configuration": object(),
                "measurement": object(),
                "test_module": object(),
            }
            return mapping.get(name, default)

    class _Executor:
        def _load_configuration_by_path(self, config_path):
            raise AssertionError("strategy should load configuration via capability")

        def _start_measurement(self, plan):
            raise AssertionError("strategy should start measurement via capability")

        def _stop_measurement(self, plan):
            raise AssertionError("strategy should stop measurement via capability")

    class _Plan:
        cases = [type("_Case", (), {"case_name": "SignalCase", "case_type": "signal_check"})()]

    try:
        strategy.run(_Plan(), adapter=_Adapter(), executor=_Executor(), config_path=None)
    except RuntimeError as exc:
        assert "only supports test_module" in str(exc)
    else:
        raise AssertionError("non-test_module CANoe cases should be rejected")
