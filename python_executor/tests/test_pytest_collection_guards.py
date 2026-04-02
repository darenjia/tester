import importlib
import inspect


MODULES = [
    "core.adapters.canoe.test_engine",
    "core.state_machine",
    "core.adapters.base_adapter",
    "core.config_manager",
]


def test_test_prefixed_support_classes_opt_out_of_pytest_collection():
    for module_name in MODULES:
        module = importlib.import_module(module_name)
        for name, obj in inspect.getmembers(module, inspect.isclass):
            if name.startswith("Test"):
                assert getattr(obj, "__test__", None) is False, (
                    f"{module_name}.{name} should set __test__ = False"
                )
