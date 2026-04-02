from core.execution_strategies.tsmaster_strategy import TSMasterExecutionStrategy
from core.adapters.tsmaster_adapter import TSMasterAdapter


class _DummyAdapter:
    def __init__(self, capabilities):
        self._capabilities = capabilities

    def get_capability(self, name, default=None):
        return self._capabilities.get(name, default)


def test_tsmaster_strategy_requires_rpc_capability():
    strategy = TSMasterExecutionStrategy()
    adapter = _DummyAdapter(
        {
            "configuration": object(),
            "measurement": object(),
        }
    )

    ok, error = strategy.prepare(plan=None, adapter=adapter)

    assert ok is False
    assert "rpc" in error.lower()


def test_tsmaster_strategy_prepare_accepts_registered_capabilities():
    strategy = TSMasterExecutionStrategy()
    adapter = _DummyAdapter(
        {
            "configuration": object(),
            "measurement": object(),
            "rpc_execution": object(),
        }
    )

    ok, error = strategy.prepare(plan=None, adapter=adapter)

    assert ok is True
    assert error is None


def test_tsmaster_strategy_runs_via_capabilities_without_executor_helpers(monkeypatch):
    strategy = TSMasterExecutionStrategy()
    observed = {"start": [], "wait": [], "load": [], "stop": 0}

    monkeypatch.setattr(
        "core.execution_strategies.tsmaster_strategy.get_case_mapping_manager",
        lambda: type(
            "_MappingManager",
            (),
            {"get_mapping": staticmethod(lambda case_no: type("_Mapping", (), {"ini_config": f"{case_no}=1"})())},
        )(),
    )

    class _RPC:
        def start_execution(self, test_cases=None, wait_for_complete=True, timeout=None):
            observed["start"].append((test_cases, wait_for_complete, timeout))
            return True

        def wait_for_complete(self, timeout=None):
            observed["wait"].append(timeout)
            return True

        def get_report_info(self):
            return {
                "passed": 1,
                "failed": 0,
                "details": [{"name": "Case 1", "case_no": "CASE-1", "verdict": "PASS"}],
            }

    class _Artifact:
        def collect(self):
            return {
                "passed": 1,
                "failed": 0,
                "details": [{"name": "Case 1", "case_no": "CASE-1", "verdict": "PASS"}],
            }

    class _Measurement:
        def start(self):
            observed["measurement_started"] = True
            return True

        def stop(self):
            observed["stop"] += 1
            return True

    class _Configuration:
        def load(self, config_path):
            observed["load"].append(config_path)
            return True

    class _Adapter:
        def get_capability(self, name, default=None):
            mapping = {
                "configuration": _Configuration(),
                "measurement": _Measurement(),
                "rpc_execution": _RPC(),
                "artifact": _Artifact(),
            }
            return mapping.get(name, default)

    class _Collector:
        def __init__(self):
            self.results = []

        def add_test_result(self, result):
            self.results.append(result)

    class _Executor:
        def __init__(self):
            self.current_collector = _Collector()

        def _load_configuration_by_path(self, config_path):
            raise AssertionError("strategy should load configuration via capability")

        def _start_test_execution(self, plan):
            raise AssertionError("strategy should start execution via rpc capability")

        def _collect_tsmaster_results(self, plan):
            raise AssertionError("strategy should collect results via capability")

        def _stop_measurement(self, plan):
            raise AssertionError("strategy should stop measurement via capability")

    plan = type(
        "_Plan",
        (),
        {
            "timeout_seconds": 30,
            "cases": [type("_Case", (), {"case_no": "CASE-1", "case_name": "Case 1", "case_type": "test_module"})()],
        },
    )()

    executor = _Executor()
    results = strategy.run(plan, adapter=_Adapter(), executor=executor, config_path="D:/cfgs/tsmaster.tsp")

    assert observed["load"] == ["D:/cfgs/tsmaster.tsp"]
    assert observed["start"] == [("CASE-1=1", False, 30)]
    assert observed["wait"] == [30]
    assert observed["stop"] == 1
    assert len(results) == 1
    assert results[0].verdict == "PASS"
    assert len(executor.current_collector.results) == 1


def test_tsmaster_adapter_falls_back_to_traditional_connect(monkeypatch):
    adapter = TSMasterAdapter({"use_rpc": True, "fallback_to_traditional": True})
    calls = []

    monkeypatch.setattr(adapter, "_connect_via_rpc", lambda: calls.append("rpc") or False)
    monkeypatch.setattr(adapter, "_connect_via_traditional", lambda: calls.append("traditional") or True)

    assert adapter.connect() is True
    assert calls == ["rpc", "traditional"]


def test_tsmaster_adapter_skips_traditional_fallback_when_disabled(monkeypatch):
    adapter = TSMasterAdapter({"use_rpc": True, "fallback_to_traditional": False})
    calls = []

    monkeypatch.setattr(adapter, "_connect_via_rpc", lambda: calls.append("rpc") or False)
    monkeypatch.setattr(adapter, "_connect_via_traditional", lambda: calls.append("traditional") or True)

    assert adapter.connect() is False
    assert calls == ["rpc"]
