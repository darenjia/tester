"""Legacy config-manager facade backed by the unified config manager."""

from __future__ import annotations

from typing import Any

from config.settings import ConfigManager, settings as config_manager
from config.unified_config import ConfigChangeEvent, get_config_manager


def get_config(key: str, default: Any = None) -> Any:
    """Return a config value from the active shared manager."""
    return get_config_manager().get(key, default)


def set_config(key: str, value: Any, persist: bool = False) -> None:
    """Set a config value through the active shared manager."""
    get_config_manager().set(key, value, persist)


def reload_config() -> bool:
    """Reload the active shared manager from disk."""
    return get_config_manager().reload()
