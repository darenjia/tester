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
