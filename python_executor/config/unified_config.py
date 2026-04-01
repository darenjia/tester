from __future__ import annotations

import json
import os
import sys
import threading
import time
from copy import deepcopy
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any, Callable


@dataclass(slots=True)
class ConfigChangeEvent:
    key: str
    old_value: Any
    new_value: Any
    timestamp: float = field(default_factory=time.time)


DEFAULT_CONFIG: dict[str, Any] = {
    "server": {
        "websocket_url": "ws://localhost:8080/ws/executor",
        "heartbeat_interval": 30,
        "reconnect_interval": 5,
        "max_reconnect_attempts": 10,
    },
    "http": {
        "port": 8180,
        "host": "0.0.0.0",
        "debug": False,
    },
    "websocket": {
        "enabled": False,
        "host": "0.0.0.0",
        "port": 8180,
        "heartbeat_interval": 30,
        "reconnect_interval": 5,
        "max_reconnect_attempts": 3,
    },
    "device": {
        "device_id": "DEVICE_001",
        "device_name": "测试执行设备-01",
        "location": "实验室",
    },
    "software": {
        "canoe_path": "C:\\Program Files\\Vector\\CANoe 17\\Exec64\\CANoe64.exe",
        "tsmaster_path": "C:\\Program Files\\TSMaster",
        "ttman_path": "C:\\Spirent\\TTman.bat",
        "workspace_path": "C:\\TestWorkspace",
    },
    "logging": {
        "level": "INFO",
        "log_dir": "logs",
        "file": "logs/executor.log",
        "max_log_files": 10,
        "max_log_size_mb": 100,
        "max_size": 10485760,
        "backup_count": 5,
    },
    "task": {
        "timeout": 3600,
        "check_interval": 1,
        "max_concurrent": 1,
    },
    "execution": {
        "default_timeout": 3600,
        "auto_start": True,
        "keep_alive": True,
    },
    "canoe": {
        "timeout": 30,
        "config_timeout": 60,
        "max_retries": 3,
        "retry_delay": 2.0,
    },
    "tsmaster": {
        "timeout": 30,
        "config_timeout": 60,
        "max_retries": 3,
        "retry_delay": 2.0,
    },
    "performance": {
        "monitor_enabled": True,
        "monitor_interval": 60,
        "metrics_retention_hours": 24,
    },
    "security": {
        "validate_inputs": True,
        "max_config_file_size_mb": 100,
        "allowed_config_extensions": [".cfg", ".xml", ".json", ".dbc", ".ldf"],
    },
    "config_cache": {
        "enabled": True,
        "cache_dir": "workspace/cache/configs",
        "max_cache_count": 50,
        "cache_ttl_hours": 168,
        "auto_cleanup": True,
    },
    "report": {
        "enabled": True,
        "api_url": "",
        "upload_url": "http://10.124.11.142:8204/upload",
        "result_api_url": "http://10.124.11.142:8315/api/python/report",
        "file_upload_url": "http://10.124.11.142:8204/upload",
        "timeout": 30,
        "max_retries": 3,
        "retry_delay": 2.0,
        "headers": {},
    },
    "report_server": {
        "enabled": True,
        "host": "10.124.11.142",
        "port": 8315,
        "path": "/api/python/report",
        "upload_report": True,
        "report_file_path": "",
        "upload_url": "http://10.124.11.142:8204/upload",
        "timeout": 30,
        "retry_count": 3,
    },
}

VALID_LOG_LEVELS = ("DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL")


def _resolve_config_path(config_file: str | None) -> Path:
    if config_file:
        return Path(config_file)
    if getattr(sys, "frozen", False):
        return Path(sys.executable).resolve().parent / "config.json"
    return Path(__file__).resolve().parents[1] / "config.json"


class UnifiedConfigManager:
    def __init__(self, config_file: str | None = None):
        self.config_file = str(_resolve_config_path(config_file))
        self._default_config = deepcopy(DEFAULT_CONFIG)
        self._config = deepcopy(self._default_config)
        self._last_good_config = deepcopy(self._default_config)
        self._lock = threading.RLock()
        self._callbacks: list[Callable[[ConfigChangeEvent], None]] = []
        self._watcher_thread: threading.Thread | None = None
        self._stop_event = threading.Event()
        self._last_modified = 0.0
        self.last_reload_error: str | None = None
        self._load_config()

    def _deep_merge(self, base: dict[str, Any], override: dict[str, Any]) -> dict[str, Any]:
        merged = deepcopy(base)
        for key, value in override.items():
            if isinstance(merged.get(key), dict) and isinstance(value, dict):
                merged[key] = self._deep_merge(merged[key], value)
            else:
                merged[key] = deepcopy(value)
        return merged

    def _read_file(self) -> dict[str, Any]:
        with open(self.config_file, "r", encoding="utf-8") as handle:
            data = json.load(handle)
        if not isinstance(data, dict):
            raise ValueError("config root must be a JSON object")
        return data

    def _write_file(self, payload: dict[str, Any]) -> None:
        config_path = Path(self.config_file)
        config_path.parent.mkdir(parents=True, exist_ok=True)
        with open(config_path, "w", encoding="utf-8") as handle:
            json.dump(payload, handle, ensure_ascii=False, indent=2)
        self._last_modified = os.path.getmtime(self.config_file)

    def _current_snapshot(self) -> dict[str, Any]:
        with self._lock:
            return deepcopy(self._config)

    def _collect_changes(
        self,
        old_value: Any,
        new_value: Any,
        prefix: str = "",
    ) -> list[ConfigChangeEvent]:
        if isinstance(old_value, dict) and isinstance(new_value, dict):
            events: list[ConfigChangeEvent] = []
            for key in sorted(set(old_value) | set(new_value)):
                child_prefix = f"{prefix}.{key}" if prefix else key
                old_child = old_value.get(key)
                new_child = new_value.get(key)
                events.extend(self._collect_changes(old_child, new_child, child_prefix))
            return events
        if old_value != new_value and prefix:
            return [ConfigChangeEvent(prefix, deepcopy(old_value), deepcopy(new_value))]
        return []

    def _notify_callbacks(self, events: list[ConfigChangeEvent]) -> None:
        if not events:
            return
        callbacks = list(self._callbacks)
        for event in events:
            for callback in callbacks:
                try:
                    callback(event)
                except Exception:
                    continue

    def _replace_config(
        self,
        new_config: dict[str, Any],
        *,
        update_last_good: bool,
    ) -> list[ConfigChangeEvent]:
        with self._lock:
            old_config = deepcopy(self._config)
            self._config = deepcopy(new_config)
            if update_last_good:
                self._last_good_config = deepcopy(new_config)
        return self._collect_changes(old_config, new_config)

    def _load_config(self) -> bool:
        config_path = Path(self.config_file)
        if not config_path.exists():
            default_config = deepcopy(self._default_config)
            events = self._replace_config(default_config, update_last_good=True)
            self._write_file(default_config)
            self.last_reload_error = None
            self._notify_callbacks(events)
            return True

        try:
            loaded_config = self._read_file()
            merged_config = self._deep_merge(self._default_config, loaded_config)
            events = self._replace_config(merged_config, update_last_good=True)
            self._last_modified = os.path.getmtime(self.config_file)
            self.last_reload_error = None
            self._notify_callbacks(events)
            return True
        except Exception as exc:
            self.last_reload_error = str(exc)
            with self._lock:
                self._config = deepcopy(self._last_good_config)
            return False

    def get(self, key: str, default: Any = None) -> Any:
        with self._lock:
            value: Any = self._config
            for part in key.split("."):
                if isinstance(value, dict) and part in value:
                    value = value[part]
                else:
                    return default
            return deepcopy(value)

    def set(self, key: str, value: Any, persist: bool = False) -> None:
        with self._lock:
            old_config = deepcopy(self._config)
            target = self._config
            parts = key.split(".")
            for part in parts[:-1]:
                existing = target.get(part)
                if not isinstance(existing, dict):
                    existing = {}
                    target[part] = existing
                target = existing
            target[parts[-1]] = deepcopy(value)
            new_config = deepcopy(self._config)
            self._last_good_config = deepcopy(new_config)

        if persist:
            self._write_file(new_config)

        self._notify_callbacks(self._collect_changes(old_config, new_config))

    def update(self, config_dict: dict[str, Any], persist: bool = False) -> None:
        with self._lock:
            old_config = deepcopy(self._config)
            self._config = self._deep_merge(self._config, config_dict)
            new_config = deepcopy(self._config)
            self._last_good_config = deepcopy(new_config)

        if persist:
            self._write_file(new_config)

        self._notify_callbacks(self._collect_changes(old_config, new_config))

    def get_all(self) -> dict[str, Any]:
        return self._current_snapshot()

    def reload(self) -> bool:
        return self._load_config()

    def _validate_snapshot(self, config: dict[str, Any]) -> list[str]:
        errors: list[str] = []

        websocket_port = self._get_from_snapshot(config, "websocket.port")
        if not isinstance(websocket_port, int) or not 1 <= websocket_port <= 65535:
            errors.append("websocket.port must be an integer between 1 and 65535")

        logging_level = self._get_from_snapshot(config, "logging.level")
        if logging_level not in VALID_LOG_LEVELS:
            errors.append(
                "logging.level must be one of DEBUG, INFO, WARNING, ERROR, CRITICAL"
            )

        task_timeout = self._get_from_snapshot(config, "task.timeout")
        if not isinstance(task_timeout, int) or task_timeout <= 0:
            errors.append("task.timeout must be a positive integer")

        return errors

    def _get_from_snapshot(self, config: dict[str, Any], key: str, default: Any = None) -> Any:
        value: Any = config
        for part in key.split("."):
            if isinstance(value, dict) and part in value:
                value = value[part]
            else:
                return default
        return deepcopy(value)

    def validate_config(self, config_dict: dict[str, Any] | None = None) -> list[str]:
        if config_dict is None:
            config = self.get_all()
        else:
            config = self._deep_merge(self.get_all(), config_dict)
        return self._validate_snapshot(config)

    def register_change_callback(self, callback: Callable[[ConfigChangeEvent], None]) -> None:
        self._callbacks.append(callback)

    def start_watcher(self, interval: float = 5.0) -> None:
        if self._watcher_thread and self._watcher_thread.is_alive():
            return

        self._stop_event.clear()

        def watch_loop() -> None:
            while not self._stop_event.wait(interval):
                try:
                    if not os.path.exists(self.config_file):
                        continue
                    modified = os.path.getmtime(self.config_file)
                    if modified > self._last_modified:
                        self.reload()
                except Exception:
                    continue

        self._watcher_thread = threading.Thread(
            target=watch_loop,
            name="unified-config-watcher",
            daemon=True,
        )
        self._watcher_thread.start()

    def stop_watcher(self) -> None:
        self._stop_event.set()
        if self._watcher_thread and self._watcher_thread.is_alive():
            self._watcher_thread.join(timeout=5)
        self._watcher_thread = None

    def reset_to_default(self) -> None:
        default_config = deepcopy(self._default_config)
        events = self._replace_config(default_config, update_last_good=True)
        self._write_file(default_config)
        self.last_reload_error = None
        self._notify_callbacks(events)


_config_manager_instance: UnifiedConfigManager | None = None


def get_config_manager(config_file: str | None = None) -> UnifiedConfigManager:
    global _config_manager_instance

    resolved_config_file = str(_resolve_config_path(config_file))
    if _config_manager_instance is None:
        _config_manager_instance = UnifiedConfigManager(resolved_config_file)
    elif config_file is not None and os.path.abspath(_config_manager_instance.config_file) != os.path.abspath(
        resolved_config_file
    ):
        _config_manager_instance = UnifiedConfigManager(resolved_config_file)

    return _config_manager_instance
