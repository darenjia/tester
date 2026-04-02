"""Tests for adapter capability registration and lookup."""

from core.adapters.base_adapter import BaseTestAdapter, TestToolType
from core.adapters.canoe import CANoeAdapter
from core.adapters.ttworkbench_adapter import TTworkbenchAdapter
from core.adapters.tsmaster_adapter import TSMasterAdapter


class DemoAdapter(BaseTestAdapter):
    @property
    def tool_type(self):
        return TestToolType.CANOE

    def connect(self):
        return True

    def disconnect(self):
        return True

    def load_configuration(self, config_path):
        return True

    def start_test(self):
        return True

    def stop_test(self):
        return True

    def execute_test_item(self, item):
        return {"ok": True}


class MinimalLegacyFreeAdapter(BaseTestAdapter):
    @property
    def tool_type(self):
        return TestToolType.CANOE

    def connect(self):
        return True

    def disconnect(self):
        return True

    def load_configuration(self, config_path):
        return True

    def start_test(self):
        return True

    def stop_test(self):
        return True


def test_base_adapter_exposes_capability_lookup():
    adapter = DemoAdapter()

    assert adapter.get_capability("missing") is None


def test_base_adapter_registers_and_returns_capabilities():
    adapter = DemoAdapter()
    capability = object()

    adapter.register_capability("measurement", capability)

    assert adapter.get_capability("measurement") is capability


def test_base_adapter_rejects_legacy_execute_test_item_by_default():
    adapter = MinimalLegacyFreeAdapter({})

    result = adapter.execute_test_item({"type": "signal_check", "name": "legacy"})

    assert result["status"] == "error"
    assert "不再支持" in result["error"]


def test_canoe_adapter_registers_expected_capabilities():
    adapter = CANoeAdapter({})

    assert adapter.has_capability("configuration") is True
    assert adapter.has_capability("measurement") is True
    assert adapter.has_capability("test_module") is True
    assert adapter.has_capability("artifact") is True


def test_canoe_adapter_no_longer_exposes_removed_config_driven_apis():
    adapter = CANoeAdapter({})

    assert not hasattr(adapter, "get_signal_value")
    assert not hasattr(adapter, "set_signal_value")


def test_canoe_adapter_rejects_removed_test_cases_execution_mode():
    adapter = CANoeAdapter({})

    result = adapter.execute_test_item({"type": "test_cases", "name": "legacy"})

    assert result["status"] == "error"
    assert "不支持" in result["error"]


def test_canoe_adapter_rejects_removed_signal_item_modes():
    adapter = CANoeAdapter({})

    signal_check = adapter.execute_test_item({"type": "signal_check", "signal_name": "VehicleSpeed"})
    signal_set = adapter.execute_test_item({"type": "signal_set", "signal_name": "VehicleSpeed", "value": 42})

    assert signal_check["status"] == "error"
    assert "不支持" in signal_check["error"]
    assert signal_set["status"] == "error"
    assert "不支持" in signal_set["error"]


def test_tsmaster_adapter_registers_expected_capabilities():
    adapter = TSMasterAdapter({})

    assert adapter.has_capability("configuration") is True
    assert adapter.has_capability("measurement") is True
    assert adapter.has_capability("rpc_execution") is True
    assert adapter.has_capability("project_control") is True
    assert adapter.has_capability("artifact") is True


def test_tsmaster_adapter_rejects_legacy_execute_test_item_modes():
    adapter = TSMasterAdapter({})

    result = adapter.execute_test_item({"type": "signal_check", "signal_name": "VehicleSpeed"})

    assert result["status"] == "error"
    assert "不再支持" in result["error"]


def test_ttworkbench_adapter_registers_expected_capabilities():
    adapter = TTworkbenchAdapter({})

    assert adapter.has_capability("configuration") is True
    assert adapter.has_capability("measurement") is True
    assert adapter.has_capability("artifact") is True
    assert adapter.has_capability("ttworkbench_execution") is True


def test_ttworkbench_adapter_rejects_legacy_execute_test_item_modes():
    adapter = TTworkbenchAdapter({})

    result = adapter.execute_test_item({"type": "clf_test", "clf_file": "D:/workspace/demo.clf"})

    assert result["status"] == "error"
    assert "不再支持" in result["error"]


def test_tsmaster_adapter_no_longer_exposes_removed_legacy_execution_helpers():
    adapter = TSMasterAdapter({})

    assert not hasattr(adapter, "_execute_signal_check")
    assert not hasattr(adapter, "_execute_signal_set")
    assert not hasattr(adapter, "_execute_message_send")
    assert not hasattr(adapter, "_execute_test_sequence")
    assert not hasattr(adapter, "_execute_system_api")
