"""
Tests for Runtime Adapter Contract compliance.

This module verifies that all execution strategies follow the contract
defined in core/execution_strategies/base.py:

1. All strategies return ExecutionOutcome when collector is provided
2. All strategies return list[TestResult] when no collector is provided
3. All strategies have consistent prepare() signature returning tuple[bool, Optional[str]]
4. All strategies have consistent run() signature order: (plan, adapter, collector, executor, config_path)
5. All strategies follow the same execution flow (start runtime, execute, wait, collect, stop)
"""

import pytest
from types import SimpleNamespace

from core.execution_strategies.base import ExecutionStrategy
from core.execution_strategies.canoe_strategy import CANoeExecutionStrategy
from core.execution_strategies.tsmaster_strategy import TSMasterExecutionStrategy
from core.execution_strategies.ttworkbench_strategy import TTworkbenchExecutionStrategy
from core.result_collector import ResultCollector
from models.result import ExecutionOutcome
from models.task import TestResult


class _DummyAdapter:
    """Dummy adapter for testing capability lookups."""

    def __init__(self, capabilities=None):
        self._capabilities = capabilities or {}

    def get_capability(self, name, default=None):
        return self._capabilities.get(name, default)


class _MockConfiguration:
    """Mock configuration capability."""

    def load(self, config_path):
        return True


class _MockMeasurement:
    """Mock measurement capability."""

    def start(self):
        return True

    def stop(self):
        return True


class _MockTestModule:
    """Mock CANoe test module capability."""

    def execute_module(self, module_name, timeout=None):
        return {"verdict": "PASS", "details": {"module": module_name}}

    def list_modules(self):
        return ["ModuleA", "ModuleB"]


class _MockTSMasterExecution:
    """Mock TSMaster execution capability."""

    def __init__(self):
        self.case_selection = ""
        self.execution_started = False
        self.wait_called = False
        self.report_info = {
            "results": [
                {"case_no": "CASE-001", "passed": True, "verdict": "PASS"},
            ]
        }

    def build_case_selection(self, plan):
        self.case_selection = ",".join(f"{case.case_no}=1" for case in getattr(plan, "cases", []))
        return self.case_selection

    def start_execution(self, selected_cases):
        self.execution_started = True
        return True

    def wait_for_completion(self, timeout):
        self.wait_called = True
        return True

    def get_report_info(self):
        return self.report_info


class _MockTTWorkbenchExecution:
    """Mock TTworkbench execution capability."""

    def execute_clf(self, clf_file, task_id=None):
        return {
            "name": clf_file,
            "type": "clf_test",
            "verdict": "PASS",
            "status": "passed",
        }

    def execute_batch(self, clf_files, task_id=None):
        return {
            "name": "batch",
            "type": "batch_test",
            "results": [],
            "passed": len(clf_files),
            "failed": 0,
            "status": "passed",
        }


class TestContractPrepareSignature:
    """Test that all strategies have consistent prepare() signature."""

    @pytest.mark.parametrize(
        "strategy_class",
        [CANoeExecutionStrategy, TSMasterExecutionStrategy, TTworkbenchExecutionStrategy],
    )
    def test_prepare_returns_tuple(self, strategy_class):
        """All strategies' prepare() must return tuple[bool, Optional[str]]."""
        strategy = strategy_class()
        adapter = _DummyAdapter()

        result = strategy.prepare(None, adapter)

        assert isinstance(result, tuple), f"{strategy_class.__name__}.prepare must return tuple"
        assert len(result) == 2, f"{strategy_class.__name__}.prepare must return 2-tuple"
        assert isinstance(result[0], bool), "First element must be bool (success)"
        assert result[1] is None or isinstance(result[1], str), "Second element must be None or str (error)"

    @pytest.mark.parametrize(
        "strategy_class",
        [CANoeExecutionStrategy, TSMasterExecutionStrategy, TTworkbenchExecutionStrategy],
    )
    def test_prepare_returns_false_when_capability_missing(self, strategy_class):
        """prepare() returns (False, error) when required capability is missing."""
        strategy = strategy_class()
        adapter = _DummyAdapter()  # No capabilities registered

        success, error = strategy.prepare(None, adapter)

        assert success is False
        assert error is not None
        assert isinstance(error, str)


class TestContractRunSignature:
    """Test that all strategies have consistent run() signature."""

    @pytest.mark.parametrize(
        "strategy_class",
        [CANoeExecutionStrategy, TSMasterExecutionStrategy, TTworkbenchExecutionStrategy],
    )
    def test_run_method_exists(self, strategy_class):
        """All strategies must have run() method."""
        strategy = strategy_class()
        assert hasattr(strategy, "run"), f"{strategy_class.__name__} must have run() method"
        assert callable(getattr(strategy, "run")), f"{strategy_class.__name__}.run must be callable"

    @pytest.mark.parametrize(
        "strategy_class",
        [CANoeExecutionStrategy, TSMasterExecutionStrategy, TTworkbenchExecutionStrategy],
    )
    def test_run_accepts_collector_before_executor(self, strategy_class):
        """run() signature must have collector before executor."""
        import inspect

        strategy = strategy_class()
        sig = inspect.signature(strategy.run)
        params = list(sig.parameters.keys())

        # Find collector and executor positions
        if "collector" in params and "executor" in params:
            collector_pos = params.index("collector")
            executor_pos = params.index("executor")
            assert collector_pos < executor_pos, (
                f"{strategy_class.__name__}.run() must have 'collector' before 'executor'"
            )


class TestContractReturnValue:
    """Test that all strategies follow the return value contract."""

    def test_canoe_returns_execution_outcome_with_collector(self):
        """CANoe strategy returns ExecutionOutcome when collector is provided."""
        strategy = CANoeExecutionStrategy()
        adapter = _DummyAdapter({
            "configuration": _MockConfiguration(),
            "measurement": _MockMeasurement(),
            "test_module": _MockTestModule(),
        })

        plan = SimpleNamespace(
            cases=[
                SimpleNamespace(case_name="ModuleA", case_type="test_module"),
            ],
            timeout_seconds=30,
        )

        collector = ResultCollector("CANOE-001")
        outcome = strategy.run(plan, adapter, collector=collector)

        assert isinstance(outcome, ExecutionOutcome), "Must return ExecutionOutcome when collector provided"
        assert outcome.taskNo == "CANOE-001"
        assert outcome.status == "completed"

    def test_canoe_returns_list_without_collector(self):
        """CANoe strategy returns list[TestResult] when no collector is provided."""
        strategy = CANoeExecutionStrategy()
        adapter = _DummyAdapter({
            "configuration": _MockConfiguration(),
            "measurement": _MockMeasurement(),
            "test_module": _MockTestModule(),
        })

        plan = SimpleNamespace(
            cases=[
                SimpleNamespace(case_name="ModuleA", case_type="test_module"),
            ],
            timeout_seconds=30,
        )

        results = strategy.run(plan, adapter)

        assert isinstance(results, list), "Must return list when no collector provided"
        assert all(isinstance(r, TestResult) for r in results), "All items must be TestResult"

    def test_tsmaster_returns_execution_outcome_with_collector(self, monkeypatch):
        """TSMaster strategy returns ExecutionOutcome when collector is provided."""
        # Mock the case mapping manager
        monkeypatch.setattr(
            "core.execution_strategies.tsmaster_strategy.get_case_mapping_manager",
            lambda: type("_MM", (), {"get_mapping": lambda self, x: None})(),
        )

        strategy = TSMasterExecutionStrategy()
        mock_exec = _MockTSMasterExecution()
        adapter = _DummyAdapter({
            "measurement": _MockMeasurement(),
            "tsmaster_execution": mock_exec,
        })

        plan = SimpleNamespace(
            cases=[SimpleNamespace(case_no="CASE-001", case_name="Case 1", case_type="test_module")],
            timeout_seconds=30,
            timeout=30,
        )

        collector = ResultCollector("TSM-001")
        outcome = strategy.run(plan, adapter, collector=collector)

        assert isinstance(outcome, ExecutionOutcome), "Must return ExecutionOutcome when collector provided"
        assert outcome.taskNo == "TSM-001"

    def test_tsmaster_returns_list_without_collector(self, monkeypatch):
        """TSMaster strategy returns list[TestResult] when no collector is provided."""
        monkeypatch.setattr(
            "core.execution_strategies.tsmaster_strategy.get_case_mapping_manager",
            lambda: type("_MM", (), {"get_mapping": lambda self, x: None})(),
        )

        strategy = TSMasterExecutionStrategy()
        mock_exec = _MockTSMasterExecution()
        adapter = _DummyAdapter({
            "measurement": _MockMeasurement(),
            "tsmaster_execution": mock_exec,
        })

        plan = SimpleNamespace(
            cases=[SimpleNamespace(case_no="CASE-001", case_name="Case 1", case_type="test_module")],
            timeout_seconds=30,
            timeout=30,
        )

        results = strategy.run(plan, adapter)

        assert isinstance(results, list), "Must return list when no collector provided"
        assert all(isinstance(r, TestResult) for r in results), "All items must be TestResult"

    def test_ttworkbench_returns_execution_outcome_with_collector(self):
        """TTworkbench strategy returns ExecutionOutcome when collector is provided."""
        strategy = TTworkbenchExecutionStrategy()
        adapter = _DummyAdapter({
            "configuration": _MockConfiguration(),
            "measurement": _MockMeasurement(),
            "ttworkbench_execution": _MockTTWorkbenchExecution(),
        })

        plan = SimpleNamespace(
            task_no="TTW-001",
            cases=[
                SimpleNamespace(
                    case_name="Test1",
                    case_type="clf_test",
                    execution_params={"clf_file": "D:/test.clf"},
                ),
            ],
        )

        collector = ResultCollector("TTW-001")
        outcome = strategy.run(plan, adapter, collector=collector)

        assert isinstance(outcome, ExecutionOutcome), "Must return ExecutionOutcome when collector provided"
        assert outcome.taskNo == "TTW-001"
        assert outcome.status == "completed"

    def test_ttworkbench_returns_list_without_collector(self):
        """TTworkbench strategy returns list[TestResult] when no collector is provided."""
        strategy = TTworkbenchExecutionStrategy()
        adapter = _DummyAdapter({
            "configuration": _MockConfiguration(),
            "measurement": _MockMeasurement(),
            "ttworkbench_execution": _MockTTWorkbenchExecution(),
        })

        plan = SimpleNamespace(
            task_no="TTW-001",
            cases=[
                SimpleNamespace(
                    case_name="Test1",
                    case_type="clf_test",
                    execution_params={"clf_file": "D:/test.clf"},
                ),
            ],
        )

        results = strategy.run(plan, adapter)

        assert isinstance(results, list), "Must return list when no collector provided"
        assert all(isinstance(r, TestResult) for r in results), "All items must be TestResult"


class TestContractTestResultFields:
    """Test that TestResult objects have required fields per contract."""

    @pytest.mark.parametrize(
        "strategy_class,adapter_setup",
        [
            (
                CANoeExecutionStrategy,
                {
                    "configuration": _MockConfiguration(),
                    "measurement": _MockMeasurement(),
                    "test_module": _MockTestModule(),
                },
            ),
            (
                TTworkbenchExecutionStrategy,
                {
                    "configuration": _MockConfiguration(),
                    "measurement": _MockMeasurement(),
                    "ttworkbench_execution": _MockTTWorkbenchExecution(),
                },
            ),
        ],
    )
    def test_test_result_has_required_fields(self, strategy_class, adapter_setup):
        """TestResult must have name, type, passed, verdict fields."""
        strategy = strategy_class()
        adapter = _DummyAdapter(adapter_setup)

        if strategy_class == CANoeExecutionStrategy:
            plan = SimpleNamespace(
                cases=[SimpleNamespace(case_name="ModuleA", case_type="test_module")],
                timeout_seconds=30,
            )
        else:
            plan = SimpleNamespace(
                task_no="TEST-001",
                cases=[
                    SimpleNamespace(
                        case_name="Test1",
                        case_type="clf_test",
                        execution_params={"clf_file": "D:/test.clf"},
                    ),
                ],
            )

        results = strategy.run(plan, adapter)

        for result in results:
            assert hasattr(result, "name"), "TestResult must have name"
            assert hasattr(result, "type"), "TestResult must have type"
            assert hasattr(result, "passed"), "TestResult must have passed"
            assert hasattr(result, "verdict"), "TestResult must have verdict"
            assert isinstance(result.name, str), "name must be str"
            assert isinstance(result.type, str), "type must be str"
            assert isinstance(result.passed, bool), "passed must be bool"
            assert isinstance(result.verdict, str), "verdict must be str"


class TestContractStrategyName:
    """Test that all strategies have stable strategy_name."""

    @pytest.mark.parametrize(
        "strategy_class,expected_name",
        [
            (CANoeExecutionStrategy, "canoe"),
            (TSMasterExecutionStrategy, "tsmaster"),
            (TTworkbenchExecutionStrategy, "ttworkbench"),
        ],
    )
    def test_strategy_name_is_stable(self, strategy_class, expected_name):
        """Each strategy must have a stable strategy_name matching its tool."""
        strategy = strategy_class()
        assert strategy.strategy_name == expected_name
        assert isinstance(strategy.strategy_name, str)


class TestContractCapabilities:
    """Test capability usage is consistent with audit."""

    def test_canoe_uses_correct_capabilities(self):
        """CANoe strategy uses: configuration, measurement, test_module."""
        strategy = CANoeExecutionStrategy()
        adapter = _DummyAdapter({
            "configuration": _MockConfiguration(),
            "measurement": _MockMeasurement(),
            "test_module": _MockTestModule(),
        })

        success, error = strategy.prepare(None, adapter)
        assert success is True, f"CANoe should succeed with all capabilities: {error}"

    def test_tsmaster_uses_correct_capabilities(self):
        """TSMaster strategy uses: tsmaster_execution (optionally configuration, measurement)."""
        strategy = TSMasterExecutionStrategy()

        # Without tsmaster_execution, should fail
        adapter_no_exec = _DummyAdapter({
            "configuration": _MockConfiguration(),
            "measurement": _MockMeasurement(),
        })
        success, error = strategy.prepare(None, adapter_no_exec)
        assert success is False
        assert "tsmaster_execution" in error

        # With tsmaster_execution, should succeed
        adapter_with_exec = _DummyAdapter({
            "tsmaster_execution": _MockTSMasterExecution(),
        })
        success, error = strategy.prepare(None, adapter_with_exec)
        assert success is True

    def test_ttworkbench_uses_correct_capabilities(self):
        """TTworkbench strategy uses: configuration, measurement, ttworkbench_execution."""
        strategy = TTworkbenchExecutionStrategy()

        # Without ttworkbench_execution, should fail
        adapter_no_exec = _DummyAdapter({
            "configuration": _MockConfiguration(),
            "measurement": _MockMeasurement(),
        })
        success, error = strategy.prepare(None, adapter_no_exec)
        assert success is False
        assert "ttworkbench_execution" in error

        # With all required, should succeed
        adapter_full = _DummyAdapter({
            "configuration": _MockConfiguration(),
            "measurement": _MockMeasurement(),
            "ttworkbench_execution": _MockTTWorkbenchExecution(),
        })
        success, error = strategy.prepare(None, adapter_full)
        assert success is True
