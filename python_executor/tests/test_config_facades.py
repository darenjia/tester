from __future__ import annotations

import importlib
from pathlib import Path

import pytest

from config import unified_config


@pytest.fixture
def facade_modules(tmp_path: Path):
    config_file = tmp_path / "config.json"
    config_file.write_text("{}", encoding="utf-8")

    original_instance = getattr(unified_config, "_config_manager_instance", None)
    unified_config._config_manager_instance = unified_config.UnifiedConfigManager(
        str(config_file)
    )

    settings_module = importlib.reload(importlib.import_module("config.settings"))
    manager_module = importlib.reload(importlib.import_module("config.config_manager"))

    yield settings_module, manager_module

    unified_config._config_manager_instance = original_instance
    importlib.reload(settings_module)
    importlib.reload(manager_module)


def test_settings_and_config_manager_share_same_underlying_instance(facade_modules):
    settings_module, manager_module = facade_modules

    assert settings_module.settings is manager_module.config_manager


def test_legacy_helpers_forward_to_unified_instance(facade_modules):
    settings_module, manager_module = facade_modules

    manager_module.set_config("websocket.port", 9300)

    assert settings_module.get_config() is manager_module.config_manager
    assert settings_module.settings.get("websocket.port") == 9300
    assert manager_module.get_config("websocket.port") == 9300
