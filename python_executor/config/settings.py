"""Legacy settings facade backed by the unified config manager."""

from config.unified_config import UnifiedConfigManager, get_config_manager

ConfigManager = UnifiedConfigManager
settings = get_config_manager()


def get_config(config_path: str | None = None) -> UnifiedConfigManager:
    """Return the shared config manager instance for legacy imports."""
    return get_config_manager(config_path)
