"""Legacy config-manager facade backed by the unified config manager."""

from typing import Any

from config.unified_config import (
    ConfigChangeEvent,
    UnifiedConfigManager,
    get_config_manager,
)

ConfigManager = UnifiedConfigManager
config_manager = get_config_manager()


def get_config(key: str, default: Any = None) -> Any:
    """Return a config value from the shared manager."""
    return config_manager.get(key, default)


def set_config(key: str, value: Any, persist: bool = False) -> None:
    """Set a config value through the shared manager."""
    config_manager.set(key, value, persist)


def reload_config() -> bool:
    """Reload the shared manager from disk."""
    return config_manager.reload()
