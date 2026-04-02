from __future__ import annotations

import json
import os
import time
from pathlib import Path

from config.unified_config import ConfigChangeEvent, UnifiedConfigManager


def test_unified_manager_reads_and_merges_single_config_file(tmp_path: Path):
    config_file = tmp_path / "config.json"
    config_file.write_text(
        json.dumps(
            {
                "websocket": {"port": 9001},
                "report": {"enabled": True},
            }
        ),
        encoding="utf-8",
    )

    manager = UnifiedConfigManager(config_file=str(config_file))

    assert manager.get("websocket.port") == 9001
    assert manager.get("report.enabled") is True
    assert manager.get("logging.level") == "INFO"
    assert manager.get("websocket.host") == "0.0.0.0"


def test_unified_manager_creates_config_file_when_missing(tmp_path: Path):
    config_file = tmp_path / "config.json"

    manager = UnifiedConfigManager(config_file=str(config_file))

    assert config_file.exists()
    assert manager.get("websocket.port") == 8180
    persisted = json.loads(config_file.read_text(encoding="utf-8"))
    assert persisted["report"]["enabled"] is True


def test_reload_keeps_last_good_snapshot_when_file_is_invalid(tmp_path: Path):
    config_file = tmp_path / "config.json"
    config_file.write_text(json.dumps({"websocket": {"port": 8180}}), encoding="utf-8")

    manager = UnifiedConfigManager(config_file=str(config_file))

    config_file.write_text("{invalid json", encoding="utf-8")
    manager.reload()

    assert manager.get("websocket.port") == 8180
    assert manager.last_reload_error is not None


def test_validate_config_reports_invalid_values_and_callbacks_fire(tmp_path: Path):
    config_file = tmp_path / "config.json"
    manager = UnifiedConfigManager(config_file=str(config_file))
    events: list[ConfigChangeEvent] = []

    manager.register_change_callback(events.append)
    manager.set("websocket.port", 70000)
    manager.set("logging.level", "TRACE")
    manager.set("task.timeout", 0)

    assert [(event.key, event.new_value) for event in events] == [
        ("websocket.port", 70000),
        ("logging.level", "TRACE"),
        ("task.timeout", 0),
    ]
    assert manager.validate_config() == [
        "websocket.port must be an integer between 1 and 65535",
        "logging.level must be one of DEBUG, INFO, WARNING, ERROR, CRITICAL",
        "task.timeout must be a positive integer",
    ]


def test_validate_config_can_validate_pending_partial_updates(tmp_path: Path):
    config_file = tmp_path / "config.json"
    manager = UnifiedConfigManager(config_file=str(config_file))

    errors = manager.validate_config({"logging": {"level": "TRACE"}, "task": {"timeout": -1}})

    assert errors == [
        "logging.level must be one of DEBUG, INFO, WARNING, ERROR, CRITICAL",
        "task.timeout must be a positive integer",
    ]


def test_watcher_reloads_file_changes_and_notifies_callbacks(tmp_path: Path):
    config_file = tmp_path / "config.json"
    config_file.write_text(json.dumps({"websocket": {"port": 8180}}), encoding="utf-8")
    manager = UnifiedConfigManager(config_file=str(config_file))
    events: list[ConfigChangeEvent] = []
    manager.register_change_callback(events.append)

    manager.start_watcher(interval=0.05)
    try:
        time.sleep(0.1)
        config_file.write_text(
            json.dumps({"websocket": {"port": 9002}, "report": {"enabled": False}}),
            encoding="utf-8",
        )
        os.utime(config_file, None)

        deadline = time.time() + 2.0
        while time.time() < deadline and manager.get("websocket.port") != 9002:
            time.sleep(0.05)
    finally:
        manager.stop_watcher()

    assert manager.get("websocket.port") == 9002
    assert ("websocket.port", 8180, 9002) in [
        (event.key, event.old_value, event.new_value) for event in events
    ]
