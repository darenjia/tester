"""Legacy settings facade backed by the unified config manager."""

from __future__ import annotations

from typing import Any

from config.unified_config import UnifiedConfigManager, get_config_manager


class ConfigManagerFacade:
    """Proxy legacy imports to the active unified config manager."""

    def __getattr__(self, name: str) -> Any:
        return getattr(get_config_manager(), name)

    def __repr__(self) -> str:
        return repr(get_config_manager())


ConfigManager = UnifiedConfigManager
settings = ConfigManagerFacade()


def get_config(config_path: str | None = None) -> ConfigManagerFacade:
    """Return a facade that always forwards to the active config manager."""
    get_config_manager(config_path)
    return settings
