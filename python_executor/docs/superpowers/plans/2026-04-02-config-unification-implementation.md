# Config Unification Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the two in-repo configuration implementations with one `config.json`-backed configuration core and migrate all runtime consumers onto that single source of truth.

**Architecture:** Build a single `UnifiedConfigManager` around `config.json`, move all default values and watcher behavior into that implementation, and convert `config/settings.py` plus `config/config_manager.py` into thin export layers over the same instance. Then migrate the executor, reporting, API, and monitoring paths onto the unified interface and delete duplicated configuration logic.

**Tech Stack:** Python 3.10, dataclasses, JSON config persistence, file watcher thread, pytest.

---

## File Structure

**Primary configuration files**
- Create: `config/unified_config.py`
- Modify: `config/settings.py`
- Modify: `config/config_manager.py`

**Runtime consumers**
- Modify: `main_production.py`
- Modify: `utils/report_client.py`
- Modify: `core/task_executor_production.py`
- Modify: `core/case_mapping_manager.py`
- Modify: `core/status_monitor.py`
- Modify: `api/config_api.py`

**Tests**
- Create: `tests/test_unified_config.py`
- Create: `tests/test_config_facades.py`
- Create: `tests/test_config_runtime_integration.py`

**Docs**
- Modify: `README.md`

### Task 1: Build The Unified Config Core

**Files:**
- Create: `config/unified_config.py`
- Test: `tests/test_unified_config.py`

- [ ] **Step 1: Write the failing tests**

```python
from pathlib import Path

from config.unified_config import UnifiedConfigManager


def test_unified_manager_reads_and_merges_single_config_file(tmp_path: Path):
    config_file = tmp_path / "config.json"
    config_file.write_text('{"websocket": {"port": 9001}, "report": {"enabled": true}}', encoding="utf-8")

    manager = UnifiedConfigManager(config_file=str(config_file))

    assert manager.get("websocket.port") == 9001
    assert manager.get("report.enabled") is True
    assert manager.get("logging.level") == "INFO"


def test_unified_manager_creates_config_file_when_missing(tmp_path: Path):
    config_file = tmp_path / "config.json"

    manager = UnifiedConfigManager(config_file=str(config_file))

    assert config_file.exists()
    assert manager.get("websocket.port") == 8180


def test_reload_keeps_last_good_snapshot_when_file_is_invalid(tmp_path: Path):
    config_file = tmp_path / "config.json"
    config_file.write_text('{"websocket": {"port": 8180}}', encoding="utf-8")
    manager = UnifiedConfigManager(config_file=str(config_file))

    config_file.write_text("{invalid json", encoding="utf-8")
    manager.reload()

    assert manager.get("websocket.port") == 8180
    assert manager.last_reload_error is not None
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m pytest tests/test_unified_config.py -q`  
Expected: FAIL with `ModuleNotFoundError: No module named 'config.unified_config'`

- [ ] **Step 3: Write minimal implementation**

```python
import json
import os
import sys
import threading
import time
from copy import deepcopy
from dataclasses import dataclass, field
from typing import Any, Callable, Dict, List, Optional


@dataclass
class ConfigChangeEvent:
    key: str
    old_value: Any
    new_value: Any
    timestamp: float = field(default_factory=time.time)


DEFAULT_CONFIG: Dict[str, Any] = {
    "server": {
        "websocket_url": "ws://localhost:8080/ws/executor",
        "heartbeat_interval": 30,
        "reconnect_interval": 5,
        "max_reconnect_attempts": 10,
    },
    "http": {"port": 8180, "host": "0.0.0.0", "debug": False},
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
    "task": {"timeout": 3600, "check_interval": 1, "max_concurrent": 1},
    "execution": {"default_timeout": 3600, "auto_start": True, "keep_alive": True},
    "canoe": {"timeout": 30, "config_timeout": 60, "max_retries": 3, "retry_delay": 2.0},
    "tsmaster": {"timeout": 30, "config_timeout": 60, "max_retries": 3, "retry_delay": 2.0},
    "performance": {"monitor_enabled": True, "monitor_interval": 60, "metrics_retention_hours": 24},
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


class UnifiedConfigManager:
    def __init__(self, config_file: str = None):
        if config_file is None:
            if getattr(sys, "frozen", False):
                base_dir = os.path.dirname(sys.executable)
            else:
                base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
            config_file = os.path.join(base_dir, "config.json")
        self.config_file = config_file
        self._default_config = deepcopy(DEFAULT_CONFIG)
        self._config = deepcopy(DEFAULT_CONFIG)
        self._lock = threading.RLock()
        self._callbacks: List[Callable[[ConfigChangeEvent], None]] = []
        self._watcher_thread = None
        self._running = False
        self._last_modified = 0.0
        self.last_reload_error: Optional[str] = None
        self._load_config(initial_load=True)

    def _deep_merge(self, base: Dict[str, Any], override: Dict[str, Any]) -> Dict[str, Any]:
        result = deepcopy(base)
        for key, value in override.items():
            if key in result and isinstance(result[key], dict) and isinstance(value, dict):
                result[key] = self._deep_merge(result[key], value)
            else:
                result[key] = value
        return result

    def _load_json_file(self) -> Dict[str, Any]:
        with open(self.config_file, "r", encoding="utf-8") as fh:
            return json.load(fh)

    def _persist_current_config(self) -> None:
        config_dir = os.path.dirname(self.config_file)
        if config_dir and not os.path.exists(config_dir):
            os.makedirs(config_dir, exist_ok=True)
        with open(self.config_file, "w", encoding="utf-8") as fh:
            json.dump(self._config, fh, ensure_ascii=False, indent=2)
        self._last_modified = os.path.getmtime(self.config_file)

    def _load_config(self, initial_load: bool = False) -> None:
        if not os.path.exists(self.config_file):
            with self._lock:
                self._config = deepcopy(self._default_config)
            self._persist_current_config()
            self.last_reload_error = None
            return

        previous = self.get_all() if not initial_load else deepcopy(self._default_config)
        try:
            loaded = self._load_json_file()
            merged = self._deep_merge(self._default_config, loaded)
            with self._lock:
                self._config = merged
            self._last_modified = os.path.getmtime(self.config_file)
            self.last_reload_error = None
        except Exception as exc:
            with self._lock:
                self._config = previous
            self.last_reload_error = str(exc)

    def get(self, key: str, default: Any = None) -> Any:
        value: Any = self._config
        for part in key.split("."):
            if isinstance(value, dict) and part in value:
                value = value[part]
            else:
                return default
        return value

    def set(self, key: str, value: Any, persist: bool = False) -> None:
        with self._lock:
            current = self._config
            parts = key.split(".")
            old_value = self.get(key)
            for part in parts[:-1]:
                current = current.setdefault(part, {})
            current[parts[-1]] = value
            if old_value != value:
                event = ConfigChangeEvent(key=key, old_value=old_value, new_value=value)
                for callback in self._callbacks:
                    callback(event)
        if persist:
            self._persist_current_config()

    def update(self, config_dict: Dict[str, Any], persist: bool = False) -> None:
        with self._lock:
            self._config = self._deep_merge(self._config, config_dict)
        if persist:
            self._persist_current_config()

    def get_all(self) -> Dict[str, Any]:
        with self._lock:
            return deepcopy(self._config)

    def reload(self) -> None:
        self._load_config()

    def validate_config(self) -> List[str]:
        errors: List[str] = []
        websocket_port = self.get("websocket.port")
        if not isinstance(websocket_port, int) or websocket_port < 1 or websocket_port > 65535:
            errors.append("websocket.port 必须是1-65535之间的整数")
        logging_level = self.get("logging.level")
        if logging_level not in ["DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL"]:
            errors.append("logging.level 必须是有效日志级别")
        timeout = self.get("task.timeout", self.get("execution.default_timeout"))
        if not isinstance(timeout, int) or timeout <= 0:
            errors.append("task.timeout 必须是正整数")
        return errors

    def reset_to_default(self) -> None:
        with self._lock:
            self._config = deepcopy(self._default_config)
        self._persist_current_config()

    def register_change_callback(self, callback: Callable[[ConfigChangeEvent], None]) -> None:
        self._callbacks.append(callback)

    def start_watcher(self, interval: float = 5.0) -> None:
        if self._running:
            return
        self._running = True

        def _watch() -> None:
            while self._running:
                try:
                    if os.path.exists(self.config_file):
                        modified = os.path.getmtime(self.config_file)
                        if modified > self._last_modified:
                            self.reload()
                    time.sleep(interval)
                except Exception:
                    time.sleep(1)

        self._watcher_thread = threading.Thread(target=_watch, daemon=True)
        self._watcher_thread.start()

    def stop_watcher(self) -> None:
        self._running = False
        if self._watcher_thread:
            self._watcher_thread.join(timeout=5)
```

- [ ] **Step 4: Run test to verify it passes**

Run: `python -m pytest tests/test_unified_config.py -q`  
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add config/unified_config.py tests/test_unified_config.py
git commit -m "feat: add unified config core"
```

### Task 2: Convert Existing Config Entrypoints Into Thin Facades

**Files:**
- Modify: `config/settings.py`
- Modify: `config/config_manager.py`
- Test: `tests/test_config_facades.py`

- [ ] **Step 1: Write the failing tests**

```python
from config import settings as settings_module
from config import config_manager as manager_module


def test_settings_and_config_manager_share_same_underlying_instance():
    assert settings_module.settings is manager_module.config_manager


def test_legacy_helpers_forward_to_unified_instance():
    manager_module.set_config("websocket.port", 9300)

    assert settings_module.settings.get("websocket.port") == 9300
    assert manager_module.get_config("websocket.port") == 9300
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m pytest tests/test_config_facades.py -q`  
Expected: FAIL because `settings` and `config_manager` are different objects

- [ ] **Step 3: Write minimal implementation**

```python
# config/settings.py
from config.unified_config import UnifiedConfigManager, get_config_manager

settings = get_config_manager()
ConfigManager = UnifiedConfigManager


def get_config(config_path: str = None) -> UnifiedConfigManager:
    return get_config_manager(config_path)
```

```python
# config/config_manager.py
from typing import Any

from config.unified_config import (
    ConfigChangeEvent,
    UnifiedConfigManager,
    get_config_manager,
)

config_manager = get_config_manager()
ConfigManager = UnifiedConfigManager


def get_config(key: str, default: Any = None) -> Any:
    return config_manager.get(key, default)


def set_config(key: str, value: Any, persist: bool = False):
    config_manager.set(key, value, persist)


def reload_config():
    config_manager.reload()
```

- [ ] **Step 4: Run test to verify it passes**

Run: `python -m pytest tests/test_config_facades.py -q`  
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add config/settings.py config/config_manager.py tests/test_config_facades.py
git commit -m "refactor: route legacy config imports to unified manager"
```

### Task 3: Migrate Runtime Consumers To The Unified Config Interface

**Files:**
- Modify: `main_production.py`
- Modify: `utils/report_client.py`
- Modify: `core/task_executor_production.py`
- Modify: `core/case_mapping_manager.py`
- Modify: `core/status_monitor.py`
- Modify: `api/config_api.py`
- Test: `tests/test_config_runtime_integration.py`

- [ ] **Step 1: Write the failing tests**

```python
from config.config_manager import config_manager
from utils.report_client import ReportClient


def test_report_client_reads_same_runtime_config_instance(monkeypatch, tmp_path):
    config_file = tmp_path / "config.json"
    config_file.write_text('{"report": {"enabled": true, "result_api_url": "http://example/report"}}', encoding="utf-8")

    from config import unified_config

    unified_config._config_manager_instance = unified_config.UnifiedConfigManager(str(config_file))
    report_client = ReportClient()

    assert report_client._result_api_url == "http://example/report"
    assert config_manager.get("report.result_api_url") == "http://example/report"


def test_config_api_returns_single_runtime_snapshot(client):
    response = client.get("/config")

    assert response.status_code == 200
    payload = response.get_json()
    assert "config" in payload
    assert "validation_errors" in payload
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m pytest tests/test_config_runtime_integration.py -q`  
Expected: FAIL because runtime modules still bind to separate config implementations

- [ ] **Step 3: Write minimal implementation**

```python
# Example consumer pattern
from config.config_manager import config_manager


class ReportClient:
    def _load_config(self):
        self._enabled = config_manager.get("report_server.enabled", True)
        self._result_api_url = config_manager.get("report.result_api_url", "")
        self._file_upload_url = config_manager.get("report_server.upload_url", "")
```

```python
# Example API pattern
@app.route("/config", methods=["GET"])
def get_config():
    return {
        "config": config_manager.get_all(),
        "validation_errors": config_manager.validate_config(),
    }
```

Update each listed runtime consumer to import only the unified facade-backed `config_manager`, remove direct dependence on duplicated config internals, and ensure reload/watcher operations use the same underlying instance.

- [ ] **Step 4: Run test to verify it passes**

Run: `python -m pytest tests/test_config_runtime_integration.py -q`  
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add main_production.py utils/report_client.py core/task_executor_production.py core/case_mapping_manager.py core/status_monitor.py api/config_api.py tests/test_config_runtime_integration.py
git commit -m "refactor: migrate runtime modules to unified config"
```

### Task 4: Remove Duplicated Config Logic And Update Documentation

**Files:**
- Modify: `config/settings.py`
- Modify: `config/config_manager.py`
- Modify: `README.md`
- Test: `tests/test_unified_config.py`
- Test: `tests/test_config_facades.py`
- Test: `tests/test_config_runtime_integration.py`

- [ ] **Step 1: Write the failing documentation/assertion test**

```python
from pathlib import Path


def test_readme_points_to_single_config_file():
    readme = Path("README.md").read_text(encoding="utf-8")

    assert "config.json" in readme
    assert "executor_config.json" not in readme
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m pytest tests/test_config_runtime_integration.py tests/test_config_facades.py tests/test_unified_config.py -q`  
Expected: FAIL if duplicated config logic or old file references remain

- [ ] **Step 3: Write minimal implementation**

```python
# config/settings.py
from config.unified_config import UnifiedConfigManager, get_config_manager

settings = get_config_manager()
ConfigManager = UnifiedConfigManager
```

```python
# config/config_manager.py
from config.unified_config import ConfigChangeEvent, UnifiedConfigManager, config_manager, get_config_manager
```

```md
# README excerpt
配置文件统一使用根目录下的 `config.json`。
执行器运行时热更新与 API 配置读写都基于同一份配置快照。
```

Remove all duplicated default-config data and duplicated file-loading code from the two facade modules so the repository keeps only one real implementation in `config/unified_config.py`.

- [ ] **Step 4: Run test to verify it passes**

Run: `python -m pytest tests/test_unified_config.py tests/test_config_facades.py tests/test_config_runtime_integration.py -q`  
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add config/settings.py config/config_manager.py README.md tests/test_unified_config.py tests/test_config_facades.py tests/test_config_runtime_integration.py
git commit -m "docs: finalize single-source config system"
```

### Task 5: Full Verification

**Files:**
- Modify: none
- Test: `tests/test_unified_config.py`
- Test: `tests/test_config_facades.py`
- Test: `tests/test_config_runtime_integration.py`

- [ ] **Step 1: Run targeted configuration test suite**

Run: `python -m pytest tests/test_unified_config.py tests/test_config_facades.py tests/test_config_runtime_integration.py -q`  
Expected: PASS

- [ ] **Step 2: Run full project test suite**

Run: `python -m pytest -q`  
Expected: PASS with the existing intended manual skips only

- [ ] **Step 3: Smoke-check the unified config imports**

Run: `python -c "from config.settings import settings; from config.config_manager import config_manager; print(settings is config_manager)"`  
Expected: `True`

- [ ] **Step 4: Commit verification-only follow-up if needed**

```bash
git add .
git commit -m "test: verify unified config migration"
```
