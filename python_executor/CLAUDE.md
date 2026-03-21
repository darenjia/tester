# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Python Test Executor (测试执行器) - a desktop application for automated testing with automotive diagnostic tools. Supports CANoe, TSMaster, and TTworkbench test execution through a unified adapter interface.

## Commands

### Run the Application
```bash
# Production environment (recommended)
python main_production.py

# Desktop GUI application (PyWebView + Flask)
python app.py

# Standard web service
python main.py
```

### Install Dependencies
```bash
pip install -r requirements.txt          # Basic dependencies
pip install -r requirements_production.txt  # Production dependencies
```

### Build Executable
```bash
# Using the build script
python pkg/build.py              # Single file executable
python pkg/build.py --onedir     # Directory-based package

# Using PyInstaller directly
pyinstaller TestExecutor.spec --clean --noconfirm
```

### Run Tests
```bash
pytest
pytest -v  # verbose output
```

## Architecture

### Layered Structure
- **Application Layer** (`app.py`, `main_production.py`, `main.py`) - Entry points
- **API Layer** (`api/`) - REST endpoints for task management, case mapping, configuration
- **Core Layer** (`core/`) - Task execution, scheduling, state machines
- **Adapter Layer** (`core/adapters/`) - Test tool abstraction
- **Model Layer** (`models/`) - Data classes for tasks, results, case mappings
- **Web Layer** (`web/`) - Flask templates and static files

### Adapter Pattern
All test tools implement `BaseTestAdapter` with unified interface:
- `connect()` / `disconnect()` - Tool connection management
- `load_configuration()` - Load .cfg files
- `start_test()` / `stop_test()` - Measurement control
- `execute_test_item()` - Execute individual test cases

Use `AdapterFactory.create_adapter(tool_type, config)` to instantiate adapters.

### Task Execution Flow
1. Receive task via WebSocket or REST API
2. Look up case mappings by `case_no` to get `script_path` and `ini_config`
3. Copy .cfg file to workspace (cache mechanism)
4. Generate ParaInfo.ini from `ini_config`
5. Execute test via appropriate adapter
6. Collect results and report

## Key Files

| File | Purpose |
|------|---------|
| `config.json` | Main configuration (ports, paths, device info) |
| `data/case_mappings.json` | Case mapping storage |
| `core/task_executor_production.py` | Production task executor with retry/circuit breaker |
| `core/case_mapping_manager.py` | Case mapping CRUD operations |
| `core/config_manager.py` | TestConfigManager for ini generation |
| `core/adapters/adapter_factory.py` | Adapter creation factory |

## Configuration

Config keys use dot notation: `server.websocket_url`, `http.port`, `software.canoe_path`.

Access via `config.settings.get_config()` singleton.

### Important Config Sections
- `software` - Paths to CANoe, TSMaster, TTworkbench executables
- `report_server` - Result reporting endpoint configuration
- `config_cache` - CFG file caching settings

## Test Tool Types

Enum values: `TestToolType.CANOE`, `TestToolType.TSMASTER`, `TestToolType.TTWORKBENCH`

## Case Mapping Model

Maps test case IDs to execution configurations:
- `case_no` - Script case identifier (e.g., "CANOE-001")
- `case_name` - Display name
- `script_path` - Path to .cfg project file
- `ini_config` - JSON configuration for ParaInfo.ini generation
- `category` - Tool category: canoe/tsmaster/system

## API Endpoints

Key REST endpoints:
- `GET/POST /api/tasks` - Task management
- `POST /api/tasks/{id}/execute` - Execute task
- `GET/POST/PUT/DELETE /api/case-mappings` - Case mapping CRUD
- `POST /api/case-mappings/import/execute` - Batch import from Excel
- `GET/POST /api/config` - Configuration management
- `GET /api/env-check` - Environment validation