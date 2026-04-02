from __future__ import annotations

from core import adapters as package_exports
from core.adapters import adapter_factory as factory_module
from core.adapters.base_adapter import TestToolType


def test_adapter_factory_exports_share_single_true_source():
    assert package_exports.AdapterFactory is factory_module.AdapterFactory
    assert package_exports.TestToolType is factory_module.TestToolType
    assert package_exports.create_adapter is factory_module.create_adapter
    assert not hasattr(package_exports, "create_adapter_with_wrapper")
    assert not hasattr(package_exports, "AdapterWrapper")
    assert "create_adapter_with_wrapper" not in getattr(factory_module, "__all__", [])
    assert "AdapterWrapper" not in getattr(factory_module, "__all__", [])


def test_adapter_factory_accepts_enum_and_string_tool_types():
    factory_module.AdapterFactory.clear_instances()

    adapter_from_enum = factory_module.AdapterFactory.create_adapter(
        TestToolType.CANOE, config={}, singleton=True
    )
    adapter_from_string = factory_module.AdapterFactory.create_adapter(
        "canoe", config={}, singleton=True
    )

    assert adapter_from_enum is adapter_from_string
    assert factory_module.AdapterFactory.is_tool_supported("tsmaster") is True
    assert factory_module.AdapterFactory.is_tool_supported("unknown") is False


def test_create_adapter_helper_defaults_to_non_singleton_instances():
    factory_module.AdapterFactory.clear_instances()

    adapter_one = factory_module.create_adapter(TestToolType.CANOE, config={})
    adapter_two = factory_module.create_adapter(TestToolType.CANOE, config={})

    assert adapter_one is not adapter_two


def test_adapter_factory_classmethods_default_to_non_singleton_instances():
    factory_module.AdapterFactory.clear_instances()

    adapter_one = factory_module.AdapterFactory.create_adapter(TestToolType.CANOE, config={})
    adapter_two = factory_module.AdapterFactory.get_adapter(TestToolType.CANOE, config={})

    assert adapter_one is not adapter_two
