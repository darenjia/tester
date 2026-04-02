from __future__ import annotations

from core.adapters.adapter_wrapper import AdapterWrapper
from core.adapters.base_adapter import BaseTestAdapter, TestToolType


class _FakeAdapter(BaseTestAdapter):
    def __init__(self):
        super().__init__({})
        self.loaded_configs: list[str] = []
        self.started_forms: list[str | None] = []

    @property
    def tool_type(self) -> TestToolType:
        return TestToolType.CANOE

    def connect(self) -> bool:
        return True

    def disconnect(self) -> bool:
        return True

    def load_configuration(self, config_path: str) -> bool:
        self.loaded_configs.append(config_path)
        return True

    def start_test(self) -> bool:
        return True

    def stop_test(self) -> bool:
        return True

    def execute_test_item(self, item):
        return {"success": True, "verdict": "Passed", "duration": 1, "item": item}

    def execute_test_module_direct(self, module_name: str, timeout: int = None):
        return {"success": True, "verdict": "Passed", "duration": timeout or 0, "module": module_name}

    def start_test_execution(self, test_cases=None, wait_for_complete=True, timeout=None):
        return True

    def wait_for_test_complete(self, timeout=None):
        return True

    def get_test_report_info(self):
        return {"report_path": "C:/reports/result.html"}

    def start_master_form(self, form_name=None):
        self.started_forms.append(form_name)
        return True

    def stop_master_form(self, form_name=None):
        self.started_forms.append(f"stop:{form_name}")
        return True


def test_adapter_wrapper_delegates_common_capabilities():
    wrapper = AdapterWrapper(_FakeAdapter())

    assert wrapper.connect() is True
    assert wrapper.open_configuration("D:/cfgs/main.cfg") is True
    assert wrapper.run_test_module("ModuleA")["module"] == "ModuleA"
    assert wrapper.execute_test_module("ModuleB", timeout=12)["duration"] == 12


def test_adapter_wrapper_removes_canoe_config_driven_facade():
    wrapper = AdapterWrapper(_FakeAdapter())

    assert not hasattr(wrapper, "set_test_case_name")
    assert not hasattr(wrapper, "set_test_variable")
    assert not hasattr(wrapper, "get_test_variable")
    assert not hasattr(wrapper, "start_test_case")
    assert not hasattr(wrapper, "check_test_case_complete")
    assert not hasattr(wrapper, "run_test_case_with_config")


def test_adapter_wrapper_exposes_tsmaster_capabilities():
    wrapper = AdapterWrapper(_FakeAdapter())

    assert wrapper.start_test_execution(test_cases="CASE-1", wait_for_complete=False, timeout=30) is True
    assert wrapper.wait_for_test_complete(timeout=30) is True
    assert wrapper.get_test_report_info()["report_path"].endswith("result.html")
    assert wrapper.start_master_form("MasterForm") is True
    assert wrapper.stop_master_form("MasterForm") is True


def test_adapter_wrapper_exposes_capability_lookup():
    wrapper = AdapterWrapper(_FakeAdapter())

    wrapper.adapter.register_capability("measurement", object())

    assert wrapper.get_capability("measurement") is not None
