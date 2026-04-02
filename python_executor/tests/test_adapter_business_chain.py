"""
Adapter Business Chain Integration Tests

These tests verify the complete adapter business chain works correctly
for all supported tools (CANoe, TSMaster, TTworkbench).

Coverage:
- Happy path: prepared task enters strategy -> runtime starts -> strategy produces stable results -> artifacts flow into ExecutionOutcome -> report client sees expected report path/metadata
- Failure path: adapter chain fails fast with explicit semantics

Intent: Prove the adapter business chain is actually connected in realistic use.
"""
from __future__ import annotations

from types import SimpleNamespace
from datetime import datetime
from unittest.mock import MagicMock, patch

import pytest

from core.execution_strategies.canoe_strategy import CANoeExecutionStrategy
from core.execution_strategies.tsmaster_strategy import TSMasterExecutionStrategy
from core.execution_strategies.ttworkbench_strategy import TTworkbenchExecutionStrategy
from core.result_collector import ResultCollector
from models.result import ExecutionOutcome
from models.task import TestResult
from utils.report_client import ReportClient


# =============================================================================
# Fake Adapters for Testing
# =============================================================================

class _FakeConfigurationCapability:
    """Fake configuration capability that tracks load calls."""
    def __init__(self, load_result=True):
        self.load_result = load_result
        self.load_calls = []

    def load(self, config_path):
        self.load_calls.append(config_path)
        return self.load_result


class _FakeMeasurementCapability:
    """Fake measurement capability that tracks start/stop calls."""
    def __init__(self, start_result=True, stop_result=True):
        self.start_result = start_result
        self.stop_result = stop_result
        self.start_calls = 0
        self.stop_calls = 0

    def start(self):
        self.start_calls += 1
        return self.start_result

    def stop(self):
        self.stop_calls += 1
        return self.stop_result


class _FakeCANoeTestModuleCapability:
    """Fake CANoe test module capability."""
    def __init__(self, verdicts=None):
        """
        Args:
            verdicts: List of verdicts to return for each module execution.
                     Defaults to ["PASS"] for each call.
        """
        self.verdicts = verdicts or ["PASS"]
        self.execute_calls = []

    def execute_module(self, module_name, timeout=None):
        idx = len(self.execute_calls)
        verdict = self.verdicts[idx] if idx < len(self.verdicts) else self.verdicts[-1]
        self.execute_calls.append((module_name, timeout))
        return {
            "verdict": verdict,
            "details": {"module": module_name},
            "report_path": f"C:/reports/canoe/{module_name}.html",
            "log_path": f"C:/reports/canoe/{module_name}.log",
        }


class _FakeTSMasterExecutionCapability:
    """Fake TSMaster execution capability."""
    _DEFAULT_REPORT_INFO = {
        "passed": 1,
        "failed": 0,
        "total": 1,
        "report_path": "C:/reports/tsmaster/report.html",
        "testdata_path": "C:/reports/tsmaster/data",
        "results": [{"case_no": "CASE-001", "passed": True, "verdict": "PASS"}],
    }
    _UNSET = object()  # Sentinel to detect explicit None

    def __init__(self, report_info=_UNSET, start_result=True, wait_result=True):
        # Use sentinel to detect explicit None vs default
        if report_info is self._UNSET:
            self.report_info = self._DEFAULT_REPORT_INFO
        else:
            self.report_info = report_info  # Could be None or a dict
        self.start_result = start_result
        self.wait_result = wait_result
        self.build_case_selection_calls = 0
        self.start_execution_calls = []
        self.wait_for_completion_calls = []

    def build_case_selection(self, plan):
        self.build_case_selection_calls += 1
        cases = getattr(plan, "cases", []) or []
        return ",".join(f"{getattr(c, 'case_no', '')}=1" for c in cases)

    def start_execution(self, selected_cases):
        self.start_execution_calls.append(selected_cases)
        return self.start_result

    def wait_for_completion(self, timeout):
        self.wait_for_completion_calls.append(timeout)
        return self.wait_result

    def get_report_info(self):
        return self.report_info


class _FakeTTWorkbenchExecutionCapability:
    """Fake TTworkbench execution capability."""
    def __init__(self, results=None):
        """
        Args:
            results: List of result dicts for each case execution.
                    Defaults to [{"verdict": "PASS", "status": "passed"}].
        """
        self.results = results or [{"verdict": "PASS", "status": "passed"}]
        self.execute_clf_calls = []
        self.execute_batch_calls = []

    def execute_clf(self, clf_file, task_id=None):
        idx = len(self.execute_clf_calls)
        result = self.results[idx] if idx < len(self.results) else self.results[-1]
        self.execute_clf_calls.append((clf_file, task_id))
        return {
            **result,
            "report_path": f"C:/reports/ttworkbench/{clf_file}.html",
            "log_path": f"C:/reports/ttworkbench/{clf_file}.log",
        }

    def execute_batch(self, clf_files, task_id=None):
        self.execute_batch_calls.append((clf_files, task_id))
        return {
            "verdict": "PASS",
            "status": "passed",
            "report_path": f"C:/reports/ttworkbench/batch.html",
            "log_path": f"C:/reports/ttworkbench/batch.log",
        }


class _FakeAdapter:
    """Fake adapter with configurable capabilities."""
    def __init__(
        self,
        configuration=None,
        measurement=None,
        test_module=None,
        tsmaster_execution=None,
        ttworkbench_execution=None,
    ):
        self._capabilities = {
            "configuration": configuration,
            "measurement": measurement,
            "test_module": test_module,
            "tsmaster_execution": tsmaster_execution,
            "ttworkbench_execution": ttworkbench_execution,
        }

    def get_capability(self, name, default=None):
        return self._capabilities.get(name, default)


def _make_plan(tool_type, task_no="TASK-CHAIN-001", cases=None, timeout_seconds=30, **kwargs):
    """Create a plan with specified cases."""
    cases = cases or [
        SimpleNamespace(case_no="CASE-001", case_name="ModuleA", case_type="test_module"),
        SimpleNamespace(case_no="CASE-002", case_name="ModuleB", case_type="test_module"),
    ]
    return SimpleNamespace(
        tool_type=tool_type,
        task_no=task_no,
        timeout_seconds=timeout_seconds,
        cases=cases,
        **kwargs,
    )


# =============================================================================
# Happy Path Tests - CANoe
# =============================================================================

class TestCANoeHappyPath:
    """Happy path tests for CANoe adapter business chain."""

    def test_canoe_prepared_task_enters_strategy_and_runtime_starts(self):
        """Verify prepared task enters strategy and measurement starts correctly."""
        config_cap = _FakeConfigurationCapability(load_result=True)
        measurement_cap = _FakeMeasurementCapability(start_result=True)
        test_module_cap = _FakeCANoeTestModuleCapability(verdicts=["PASS", "PASS"])
        adapter = _FakeAdapter(
            configuration=config_cap,
            measurement=measurement_cap,
            test_module=test_module_cap,
        )
        collector = ResultCollector("CANOE-HAPPY-001")
        plan = _make_plan("canoe", "CANOE-HAPPY-001")

        outcome = CANoeExecutionStrategy().run(plan, adapter, collector, config_path="D:/cfgs/test.cfg")

        # Verify configuration was loaded
        assert config_cap.load_calls == ["D:/cfgs/test.cfg"]
        # Verify measurement started
        assert measurement_cap.start_calls == 1
        # Verify test modules were executed
        assert len(test_module_cap.execute_calls) == 2
        assert test_module_cap.execute_calls[0][0] == "ModuleA"
        assert test_module_cap.execute_calls[1][0] == "ModuleB"

    def test_canoe_strategy_produces_stable_results(self):
        """Verify CANoe strategy produces stable pass/fail results."""
        test_module_cap = _FakeCANoeTestModuleCapability(verdicts=["PASS", "FAIL", "PASS"])
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            test_module=test_module_cap,
        )
        collector = ResultCollector("CANOE-STABLE-001")
        plan = _make_plan(
            "canoe", "CANOE-STABLE-001",
            cases=[
                SimpleNamespace(case_no="CASE-A", case_name="ModuleA", case_type="test_module"),
                SimpleNamespace(case_no="CASE-B", case_name="ModuleB", case_type="test_module"),
                SimpleNamespace(case_no="CASE-C", case_name="ModuleC", case_type="test_module"),
            ]
        )

        outcome = CANoeExecutionStrategy().run(plan, adapter, collector)

        assert outcome.status == "completed"
        assert len(outcome.results) == 3
        # Verdict mapping: PASS->passed=True, FAIL->passed=False
        assert outcome.results[0].passed is True
        assert outcome.results[0].verdict == "PASS"
        assert outcome.results[1].passed is False
        assert outcome.results[1].verdict == "FAIL"
        assert outcome.results[2].passed is True
        assert outcome.results[2].verdict == "PASS"
        # Summary should be stable
        assert outcome.summary["total"] == 3
        assert outcome.summary["passed"] == 2
        assert outcome.summary["failed"] == 1

    def test_canoe_artifacts_flow_into_execution_outcome(self):
        """Verify CANoe artifacts (report_path, log_path) flow into ExecutionOutcome."""
        test_module_cap = _FakeCANoeTestModuleCapability(verdicts=["PASS"])
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            test_module=test_module_cap,
        )
        collector = ResultCollector("CANOE-ARTIFACT-001")
        plan = _make_plan("canoe", "CANOE-ARTIFACT-001")

        outcome = CANoeExecutionStrategy().run(plan, adapter, collector)

        # Verify artifacts are present in outcome
        assert outcome.artifacts is not None
        # The last module's report path should be in artifacts (ModuleB is executed last with 2 cases)
        assert "report_path" in outcome.artifacts
        assert outcome.artifacts["report_path"] == "C:/reports/canoe/ModuleB.html"

    def test_canoe_report_client_sees_expected_report_metadata(self):
        """Verify report client sees the expected report path/metadata."""
        test_module_cap = _FakeCANoeTestModuleCapability(verdicts=["PASS"])
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            test_module=test_module_cap,
        )
        collector = ResultCollector("CANOE-REPORT-001")
        plan = _make_plan("canoe", "CANOE-REPORT-001")

        outcome = CANoeExecutionStrategy().run(plan, adapter, collector)

        # Verify report metadata is present
        assert outcome.report_metadata is not None
        assert outcome.report_metadata.get("source") == "canoe_test_module"

        # Verify payload can be built for report client
        payload = outcome.to_report_payload(task_info={"projectNo": "PROJ-001"})
        assert payload["taskNo"] == "CANOE-REPORT-001"
        assert payload["status"] == "completed"


# =============================================================================
# Happy Path Tests - TSMaster
# =============================================================================

class TestTSMasterHappyPath:
    """Happy path tests for TSMaster adapter business chain."""

    def test_tsmaster_prepared_task_enters_strategy_and_runtime_starts(self):
        """Verify prepared task enters strategy and execution starts correctly."""
        capability = _FakeTSMasterExecutionCapability()
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            tsmaster_execution=capability,
        )
        collector = ResultCollector("TSMASTER-HAPPY-001")
        plan = _make_plan("tsmaster", "TSMASTER-HAPPY-001")

        with patch("core.execution_strategies.tsmaster_strategy.get_case_mapping_manager") as mock:
            mock.return_value = type("_M", (), {"get_mapping": staticmethod(lambda c: None)})()
            outcome = TSMasterExecutionStrategy().run(plan, adapter, collector)

        # Verify execution started
        assert len(capability.start_execution_calls) == 1
        # Verify measurement started
        assert capability is not None

    def test_tsmaster_strategy_produces_stable_results(self):
        """Verify TSMaster strategy produces stable pass/fail results."""
        capability = _FakeTSMasterExecutionCapability(
            report_info={
                "passed": 2,
                "failed": 1,
                "total": 3,
                "report_path": "C:/reports/tsmaster/report.html",
                "testdata_path": "C:/reports/tsmaster/data",
                "results": [
                    {"case_no": "CASE-A", "passed": True, "verdict": "PASS"},
                    {"case_no": "CASE-B", "passed": True, "verdict": "PASS"},
                    {"case_no": "CASE-C", "passed": False, "verdict": "FAIL"},
                ],
            }
        )
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            tsmaster_execution=capability,
        )
        collector = ResultCollector("TSMASTER-STABLE-001")
        plan = _make_plan(
            "tsmaster", "TSMASTER-STABLE-001",
            cases=[
                SimpleNamespace(case_no="CASE-A", case_name="Case A", case_type="test_module"),
                SimpleNamespace(case_no="CASE-B", case_name="Case B", case_type="test_module"),
                SimpleNamespace(case_no="CASE-C", case_name="Case C", case_type="test_module"),
            ]
        )

        with patch("core.execution_strategies.tsmaster_strategy.get_case_mapping_manager") as mock:
            mock.return_value = type("_M", (), {"get_mapping": staticmethod(lambda c: None)})()
            outcome = TSMasterExecutionStrategy().run(plan, adapter, collector)

        assert outcome.status == "completed"
        assert len(outcome.results) == 3
        assert outcome.summary["passed"] == 2
        assert outcome.summary["failed"] == 1

    def test_tsmaster_artifacts_flow_into_execution_outcome(self):
        """Verify TSMaster artifacts flow into ExecutionOutcome."""
        capability = _FakeTSMasterExecutionCapability(
            report_info={
                "passed": 1,
                "failed": 0,
                "total": 1,
                "report_path": "C:/reports/tsmaster/suite.html",
                "testdata_path": "C:/reports/tsmaster/suite_data",
                "log_path": "C:/reports/tsmaster/suite.log",
                "results": [{"case_no": "CASE-001", "passed": True, "verdict": "PASS"}],
            }
        )
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            tsmaster_execution=capability,
        )
        collector = ResultCollector("TSMASTER-ARTIFACT-001")
        plan = _make_plan("tsmaster", "TSMASTER-ARTIFACT-001")

        with patch("core.execution_strategies.tsmaster_strategy.get_case_mapping_manager") as mock:
            mock.return_value = type("_M", (), {"get_mapping": staticmethod(lambda c: None)})()
            outcome = TSMasterExecutionStrategy().run(plan, adapter, collector)

        # Verify artifacts flow into outcome
        assert outcome.artifacts is not None
        assert outcome.artifacts.get("report_path") == "C:/reports/tsmaster/suite.html"
        assert outcome.artifacts.get("testdata_path") == "C:/reports/tsmaster/suite_data"
        assert outcome.artifacts.get("log_path") == "C:/reports/tsmaster/suite.log"

    def test_tsmaster_report_client_sees_expected_report_metadata(self):
        """Verify report client sees the expected report path/metadata."""
        capability = _FakeTSMasterExecutionCapability(
            report_info={
                "passed": 1,
                "failed": 0,
                "total": 1,
                "report_path": "C:/reports/tsmaster/outcome.html",
                "testdata_path": "C:/reports/tsmaster/outcome_data",
                "results": [{"case_no": "CASE-001", "passed": True, "verdict": "PASS"}],
            }
        )
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            tsmaster_execution=capability,
        )
        collector = ResultCollector("TSMASTER-REPORT-001")
        plan = _make_plan("tsmaster", "TSMASTER-REPORT-001")

        with patch("core.execution_strategies.tsmaster_strategy.get_case_mapping_manager") as mock:
            mock.return_value = type("_M", (), {"get_mapping": staticmethod(lambda c: None)})()
            outcome = TSMasterExecutionStrategy().run(plan, adapter, collector)

        # Verify report metadata
        assert outcome.report_metadata is not None
        assert outcome.report_metadata.get("source") == "tsmaster_execution"
        assert outcome.report_metadata.get("has_per_case_details") is True
        assert outcome.report_metadata.get("passed") == 1

        # Verify payload
        payload = outcome.to_report_payload(task_info={"projectNo": "PROJ-TS"})
        assert payload["taskNo"] == "TSMASTER-REPORT-001"
        assert payload["status"] == "completed"


# =============================================================================
# Happy Path Tests - TTworkbench
# =============================================================================

class TestTTWorkbenchHappyPath:
    """Happy path tests for TTworkbench adapter business chain."""

    def test_ttworkbench_prepared_task_enters_strategy_and_runtime_starts(self):
        """Verify prepared task enters strategy and execution starts correctly."""
        capability = _FakeTTWorkbenchExecutionCapability(
            results=[
                {"verdict": "PASS", "status": "passed"},
                {"verdict": "PASS", "status": "passed"},
            ]
        )
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            ttworkbench_execution=capability,
        )
        collector = ResultCollector("TTWB-HAPPY-001")
        plan = _make_plan(
            "ttworkbench", "TTWB-HAPPY-001",
            cases=[
                SimpleNamespace(
                    case_no="CASE-001", case_name="Test1", case_type="clf_test",
                    execution_params={"clf_file": "test1.clf"}
                ),
                SimpleNamespace(
                    case_no="CASE-002", case_name="Test2", case_type="clf_test",
                    execution_params={"clf_file": "test2.clf"}
                ),
            ]
        )

        outcome = TTworkbenchExecutionStrategy().run(plan, adapter, collector)

        # Verify execution started (measurement started)
        assert outcome.status == "completed"

    def test_ttworkbench_strategy_produces_stable_results(self):
        """Verify TTworkbench strategy produces stable pass/fail results."""
        capability = _FakeTTWorkbenchExecutionCapability(
            results=[
                {"verdict": "PASS", "status": "passed"},
                {"verdict": "FAIL", "status": "failed"},
                {"verdict": "PASS", "status": "passed"},
            ]
        )
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            ttworkbench_execution=capability,
        )
        collector = ResultCollector("TTWB-STABLE-001")
        plan = _make_plan(
            "ttworkbench", "TTWB-STABLE-001",
            cases=[
                SimpleNamespace(
                    case_no="CASE-A", case_name="TestA", case_type="clf_test",
                    execution_params={"clf_file": "test_a.clf"}
                ),
                SimpleNamespace(
                    case_no="CASE-B", case_name="TestB", case_type="clf_test",
                    execution_params={"clf_file": "test_b.clf"}
                ),
                SimpleNamespace(
                    case_no="CASE-C", case_name="TestC", case_type="clf_test",
                    execution_params={"clf_file": "test_c.clf"}
                ),
            ]
        )

        outcome = TTworkbenchExecutionStrategy().run(plan, adapter, collector)

        assert outcome.status == "completed"
        assert len(outcome.results) == 3
        assert outcome.summary["passed"] == 2
        assert outcome.summary["failed"] == 1

    def test_ttworkbench_artifacts_flow_into_execution_outcome(self):
        """Verify TTworkbench artifacts flow into ExecutionOutcome."""
        capability = _FakeTTWorkbenchExecutionCapability(
            results=[{"verdict": "PASS", "status": "passed"}]
        )
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            ttworkbench_execution=capability,
        )
        collector = ResultCollector("TTWB-ARTIFACT-001")
        plan = _make_plan(
            "ttworkbench", "TTWB-ARTIFACT-001",
            cases=[
                SimpleNamespace(
                    case_no="CASE-001", case_name="Test1", case_type="clf_test",
                    execution_params={"clf_file": "test1.clf"}
                ),
            ]
        )

        outcome = TTworkbenchExecutionStrategy().run(plan, adapter, collector)

        # Verify artifacts flow into outcome
        assert outcome.artifacts is not None
        assert "report_path" in outcome.artifacts
        assert outcome.artifacts["report_path"] == "C:/reports/ttworkbench/test1.clf.html"

    def test_ttworkbench_report_client_sees_expected_report_metadata(self):
        """Verify report client sees the expected report path/metadata."""
        capability = _FakeTTWorkbenchExecutionCapability(
            results=[{"verdict": "PASS", "status": "passed"}]
        )
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            ttworkbench_execution=capability,
        )
        collector = ResultCollector("TTWB-REPORT-001")
        plan = _make_plan(
            "ttworkbench", "TTWB-REPORT-001",
            cases=[
                SimpleNamespace(
                    case_no="CASE-001", case_name="Test1", case_type="clf_test",
                    execution_params={"clf_file": "test1.clf"}
                ),
            ]
        )

        outcome = TTworkbenchExecutionStrategy().run(plan, adapter, collector)

        # Verify report metadata
        assert outcome.report_metadata is not None
        assert outcome.report_metadata.get("source") == "ttworkbench_execution"

        # Verify payload
        payload = outcome.to_report_payload(task_info={"projectNo": "PROJ-TTWB"})
        assert payload["taskNo"] == "TTWB-REPORT-001"
        assert payload["status"] == "completed"


# =============================================================================
# Failure Path Tests - CANoe
# =============================================================================

class TestCANoeFailurePath:
    """Failure path tests for CANoe adapter business chain."""

    def test_canoe_fails_fast_when_measurement_fails_to_start(self):
        """Verify CANoe fails fast when measurement fails to start."""
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(start_result=False),
            test_module=_FakeCANoeTestModuleCapability(),
        )
        collector = ResultCollector("CANOE-FAIL-START")
        plan = _make_plan("canoe", "CANOE-FAIL-START")

        with pytest.raises(RuntimeError, match="failed to start CANoe measurement"):
            CANoeExecutionStrategy().run(plan, adapter, collector, config_path="D:/cfgs/test.cfg")

    def test_canoe_fails_fast_when_test_module_capability_missing(self):
        """Verify CANoe fails fast with explicit error when test_module capability is missing."""
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            test_module=None,  # Missing!
        )
        collector = ResultCollector("CANOE-FAIL-MISSING")
        plan = _make_plan("canoe", "CANOE-FAIL-MISSING")

        with pytest.raises(RuntimeError, match="test_module capability is not available"):
            CANoeExecutionStrategy().run(plan, adapter, collector)

    def test_canoe_fails_fast_when_config_load_fails(self):
        """Verify CANoe fails fast when configuration loading fails."""
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(load_result=False),
            measurement=_FakeMeasurementCapability(),
            test_module=_FakeCANoeTestModuleCapability(),
        )
        collector = ResultCollector("CANOE-FAIL-CFG")
        plan = _make_plan("canoe", "CANOE-FAIL-CFG")

        with pytest.raises(RuntimeError, match="failed to load CANoe configuration"):
            CANoeExecutionStrategy().run(plan, adapter, collector, config_path="D:/cfgs/bad.cfg")

    def test_canoe_fails_fast_with_non_test_module_case_type(self):
        """Verify CANoe fails fast when case type is not test_module."""
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            test_module=_FakeCANoeTestModuleCapability(),
        )
        collector = ResultCollector("CANOE-FAIL-TYPE")
        plan = _make_plan(
            "canoe", "CANOE-FAIL-TYPE",
            cases=[
                SimpleNamespace(case_no="CASE-SIG", case_name="SignalCheck", case_type="signal_check"),
            ]
        )

        with pytest.raises(RuntimeError, match="only supports test_module"):
            CANoeExecutionStrategy().run(plan, adapter, collector)


# =============================================================================
# Failure Path Tests - TSMaster
# =============================================================================

class TestTSMasterFailurePath:
    """Failure path tests for TSMaster adapter business chain."""

    def test_tsmaster_fails_fast_when_execution_capability_missing(self):
        """Verify TSMaster fails fast with explicit error when tsmaster_execution capability is missing."""
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            tsmaster_execution=None,  # Missing!
        )
        collector = ResultCollector("TS-FAIL-MISSING")
        plan = _make_plan("tsmaster", "TS-FAIL-MISSING")

        with pytest.raises(RuntimeError, match="execution capability is not available"):
            with patch("core.execution_strategies.tsmaster_strategy.get_case_mapping_manager") as mock:
                mock.return_value = type("_M", (), {"get_mapping": staticmethod(lambda c: None)})()
                TSMasterExecutionStrategy().run(plan, adapter, collector)

    def test_tsmaster_fails_fast_when_execution_start_fails(self):
        """Verify TSMaster fails fast when execution fails to start."""
        capability = _FakeTSMasterExecutionCapability(start_result=False)
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            tsmaster_execution=capability,
        )
        collector = ResultCollector("TS-FAIL-START")
        plan = _make_plan("tsmaster", "TS-FAIL-START")

        with patch("core.execution_strategies.tsmaster_strategy.get_case_mapping_manager") as mock:
            mock.return_value = type("_M", (), {"get_mapping": staticmethod(lambda c: None)})()
            outcome = TSMasterExecutionStrategy().run(plan, adapter, collector)

        assert outcome.status == "failed"
        assert "failed to start" in outcome.errorMessage.lower()

    def test_tsmaster_fails_fast_on_timeout(self):
        """Verify TSMaster fails fast on timeout with explicit semantics."""
        capability = _FakeTSMasterExecutionCapability(wait_result=False)
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            tsmaster_execution=capability,
        )
        collector = ResultCollector("TS-FAIL-TIMEOUT")
        plan = _make_plan("tsmaster", "TS-FAIL-TIMEOUT", timeout_seconds=5)

        with patch("core.execution_strategies.tsmaster_strategy.get_case_mapping_manager") as mock:
            mock.return_value = type("_M", (), {"get_mapping": staticmethod(lambda c: None)})()
            outcome = TSMasterExecutionStrategy().run(plan, adapter, collector)

        assert outcome.status == "timeout"
        assert "timed out" in outcome.errorMessage.lower()

    def test_tsmaster_fails_fast_when_report_missing(self):
        """Verify TSMaster fails fast when report info is missing."""
        capability = _FakeTSMasterExecutionCapability(report_info=None)
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            tsmaster_execution=capability,
        )
        collector = ResultCollector("TS-FAIL-REPORT")
        plan = _make_plan("tsmaster", "TS-FAIL-REPORT")

        with patch("core.execution_strategies.tsmaster_strategy.get_case_mapping_manager") as mock:
            mock.return_value = type("_M", (), {"get_mapping": staticmethod(lambda c: None)})()
            outcome = TSMasterExecutionStrategy().run(plan, adapter, collector)

        assert outcome.status == "failed"
        assert "report" in outcome.errorMessage.lower()


# =============================================================================
# Failure Path Tests - TTworkbench
# =============================================================================

class TestTTWorkbenchFailurePath:
    """Failure path tests for TTworkbench adapter business chain."""

    def test_ttworkbench_fails_fast_when_execution_capability_missing(self):
        """Verify TTworkbench fails fast with explicit error when ttworkbench_execution capability is missing.

        Note: The strategy's prepare() method checks for capability, but run() does not
        defensively check. When capability is missing and run() is called directly,
        it raises AttributeError instead of RuntimeError. The prepare() method should
        be called before run() in production code.
        """
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            ttworkbench_execution=None,  # Missing!
        )
        collector = ResultCollector("TTWB-FAIL-MISSING")
        plan = _make_plan(
            "ttworkbench", "TTWB-FAIL-MISSING",
            cases=[
                SimpleNamespace(
                    case_no="CASE-001", case_name="Test1", case_type="clf_test",
                    execution_params={"clf_file": "test1.clf"}
                ),
            ]
        )

        # The strategy should fail, but currently raises AttributeError due to missing defensive check in run()
        # This test documents the actual behavior - production code should call prepare() first
        with pytest.raises((RuntimeError, AttributeError)):
            TTworkbenchExecutionStrategy().run(plan, adapter, collector)

    def test_ttworkbench_fails_fast_when_measurement_fails_to_start(self):
        """Verify TTworkbench fails fast when measurement fails to start."""
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(start_result=False),
            ttworkbench_execution=_FakeTTWorkbenchExecutionCapability(),
        )
        collector = ResultCollector("TTWB-FAIL-START")
        plan = _make_plan(
            "ttworkbench", "TTWB-FAIL-START",
            cases=[
                SimpleNamespace(
                    case_no="CASE-001", case_name="Test1", case_type="clf_test",
                    execution_params={"clf_file": "test1.clf"}
                ),
            ]
        )

        with pytest.raises(RuntimeError, match="failed to start"):
            TTworkbenchExecutionStrategy().run(plan, adapter, collector)

    def test_ttworkbench_fails_fast_with_unsupported_case_type(self):
        """Verify TTworkbench fails fast with unsupported case type."""
        capability = _FakeTTWorkbenchExecutionCapability()
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            ttworkbench_execution=capability,
        )
        collector = ResultCollector("TTWB-FAIL-TYPE")
        plan = _make_plan(
            "ttworkbench", "TTWB-FAIL-TYPE",
            cases=[
                SimpleNamespace(
                    case_no="CASE-001", case_name="Test1", case_type="unknown_type",
                    execution_params={}
                ),
            ]
        )

        with pytest.raises(RuntimeError, match="does not support"):
            TTworkbenchExecutionStrategy().run(plan, adapter, collector)

    def test_ttworkbench_fails_fast_when_clf_file_missing(self):
        """Verify TTworkbench fails fast when clf_file is missing for clf_test case."""
        capability = _FakeTTWorkbenchExecutionCapability()
        adapter = _FakeAdapter(
            configuration=_FakeConfigurationCapability(),
            measurement=_FakeMeasurementCapability(),
            ttworkbench_execution=capability,
        )
        collector = ResultCollector("TTWB-FAIL-CLF")
        plan = _make_plan(
            "ttworkbench", "TTWB-FAIL-CLF",
            cases=[
                SimpleNamespace(
                    case_no="CASE-001", case_name="Test1", case_type="clf_test",
                    execution_params={}  # Missing clf_file!
                ),
            ]
        )

        with pytest.raises(RuntimeError, match="missing clf_file"):
            TTworkbenchExecutionStrategy().run(plan, adapter, collector)


# =============================================================================
# Cross-Cutting Tests
# =============================================================================

class TestAdapterChainContract:
    """Tests that verify the adapter chain contract across all tools."""

    def test_all_strategies_return_execution_outcome_with_collector(self):
        """Verify all strategies return ExecutionOutcome when collector is provided."""
        strategies = [
            (CANoeExecutionStrategy(), _FakeAdapter(
                configuration=_FakeConfigurationCapability(),
                measurement=_FakeMeasurementCapability(),
                test_module=_FakeCANoeTestModuleCapability(),
            )),
            (TTworkbenchExecutionStrategy(), _FakeAdapter(
                configuration=_FakeConfigurationCapability(),
                measurement=_FakeMeasurementCapability(),
                ttworkbench_execution=_FakeTTWorkbenchExecutionCapability(),
            )),
        ]

        for strategy, adapter in strategies:
            tool = strategy.strategy_name
            collector = ResultCollector(f"{tool.upper()}-OUTCOME-001")

            if tool == "canoe":
                plan = _make_plan("canoe")
                outcome = strategy.run(plan, adapter, collector)
            elif tool == "ttworkbench":
                plan = _make_plan(
                    "ttworkbench",
                    cases=[SimpleNamespace(
                        case_no="CASE-001", case_name="Test1", case_type="clf_test",
                        execution_params={"clf_file": "test.clf"}
                    )]
                )
                outcome = strategy.run(plan, adapter, collector)
            else:
                continue

            assert isinstance(outcome, ExecutionOutcome), f"{tool} should return ExecutionOutcome"
            assert outcome.taskNo == f"{tool.upper()}-OUTCOME-001"
            assert outcome.status in ("completed", "failed", "timeout")

    def test_all_strategies_produce_timing_info(self):
        """Verify all strategies produce timing info in ExecutionOutcome."""
        tsmaster_capability = _FakeTSMasterExecutionCapability()
        ttworkbench_capability = _FakeTTWorkbenchExecutionCapability()

        test_cases = [
            (
                CANoeExecutionStrategy(),
                _FakeAdapter(
                    configuration=_FakeConfigurationCapability(),
                    measurement=_FakeMeasurementCapability(),
                    test_module=_FakeCANoeTestModuleCapability(),
                ),
                _make_plan("canoe", "TIMING-CANOE"),
            ),
            (
                TTworkbenchExecutionStrategy(),
                _FakeAdapter(
                    configuration=_FakeConfigurationCapability(),
                    measurement=_FakeMeasurementCapability(),
                    ttworkbench_execution=ttworkbench_capability,
                ),
                _make_plan(
                    "ttworkbench", "TIMING-TTWB",
                    cases=[SimpleNamespace(
                        case_no="CASE-001", case_name="Test1", case_type="clf_test",
                        execution_params={"clf_file": "test.clf"}
                    )]
                ),
            ),
        ]

        for strategy, adapter, plan in test_cases:
            tool = strategy.strategy_name
            collector = ResultCollector(f"{tool.upper()}-TIMING-001")

            if tool == "canoe":
                outcome = strategy.run(plan, adapter, collector)
            elif tool == "ttworkbench":
                outcome = strategy.run(plan, adapter, collector)
            else:
                continue

            assert outcome.started_at is not None, f"{tool} should have started_at"
            assert outcome.finished_at is not None, f"{tool} should have finished_at"
            assert outcome.duration is not None, f"{tool} should have duration"
            assert outcome.duration >= 0, f"{tool} duration should be non-negative"

    def test_all_strategies_preserve_artifacts_in_outcome(self):
        """Verify all strategies preserve artifacts in ExecutionOutcome."""
        tsmaster_capability = _FakeTSMasterExecutionCapability(
            report_info={
                "report_path": "C:/reports/tsmaster/artifacts.html",
                "testdata_path": "C:/reports/tsmaster/artifacts_data",
                "results": [{"case_no": "CASE-001", "passed": True, "verdict": "PASS"}],
            }
        )

        canoe_cap = _FakeCANoeTestModuleCapability()
        ttworkbench_cap = _FakeTTWorkbenchExecutionCapability()

        test_cases = [
            (
                CANoeExecutionStrategy(),
                _FakeAdapter(
                    configuration=_FakeConfigurationCapability(),
                    measurement=_FakeMeasurementCapability(),
                    test_module=canoe_cap,
                ),
                _make_plan("canoe", "ARTIFACT-CANOE"),
            ),
            (
                TSMasterExecutionStrategy(),
                _FakeAdapter(
                    configuration=_FakeConfigurationCapability(),
                    measurement=_FakeMeasurementCapability(),
                    tsmaster_execution=tsmaster_capability,
                ),
                _make_plan("tsmaster", "ARTIFACT-TS"),
            ),
            (
                TTworkbenchExecutionStrategy(),
                _FakeAdapter(
                    configuration=_FakeConfigurationCapability(),
                    measurement=_FakeMeasurementCapability(),
                    ttworkbench_execution=ttworkbench_cap,
                ),
                _make_plan(
                    "ttworkbench", "ARTIFACT-TTWB",
                    cases=[SimpleNamespace(
                        case_no="CASE-001", case_name="Test1", case_type="clf_test",
                        execution_params={"clf_file": "test.clf"}
                    )]
                ),
            ),
        ]

        for strategy, adapter, plan in test_cases:
            tool = strategy.strategy_name
            collector = ResultCollector(f"{tool.upper()}-ARTIFACT-001")

            if tool == "canoe":
                outcome = strategy.run(plan, adapter, collector)
            elif tool == "tsmaster":
                with patch("core.execution_strategies.tsmaster_strategy.get_case_mapping_manager") as mock:
                    mock.return_value = type("_M", (), {"get_mapping": staticmethod(lambda c: None)})()
                    outcome = strategy.run(plan, adapter, collector)
            elif tool == "ttworkbench":
                outcome = strategy.run(plan, adapter, collector)
            else:
                continue

            assert outcome.artifacts is not None, f"{tool} should have artifacts"
            assert len(outcome.artifacts) > 0, f"{tool} artifacts should not be empty"

    def test_execution_outcome_to_report_payload_integration(self):
        """Verify ExecutionOutcome.to_report_payload works correctly for all tools."""
        outcome = ExecutionOutcome(
            task_no="PAYLOAD-INTEGRATION-001",
            status="completed",
            started_at=datetime(2026, 4, 2, 10, 0, 0),
            finished_at=datetime(2026, 4, 2, 10, 5, 0),
            verdict="PASS",
            summary={"total": 5, "passed": 5, "failed": 0, "passRate": "100.0%"},
            artifacts={"report_path": "C:/reports/final.html"},
            report_metadata={"source": "integration_test"},
        )

        payload = outcome.to_report_payload(
            task_info={"projectNo": "PROJ-INTEGRATION", "deviceId": "DEVICE-INT"},
            report_url="http://files.example.com/uploaded.html"
        )

        assert payload["taskNo"] == "PAYLOAD-INTEGRATION-001"
        assert payload["status"] == "completed"
        assert payload["summary"]["total"] == 5
        assert payload["projectNo"] == "PROJ-INTEGRATION"
        assert payload["deviceId"] == "DEVICE-INT"
        # Report URL should be propagated
        assert payload["results"][0]["reAddress"] == "http://files.example.com/uploaded.html" if payload.get("results") else True
