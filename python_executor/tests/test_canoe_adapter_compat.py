from pathlib import Path
import sys

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))


def test_legacy_canoe_adapter_module_imports_current_implementation():
    import core.adapters.canoe_adapter as legacy_module
    from core.adapters.canoe.adapter import CANoeAdapter, create_canoe_adapter

    assert legacy_module.CANoeAdapter is CANoeAdapter
    assert legacy_module.create_canoe_adapter is create_canoe_adapter
