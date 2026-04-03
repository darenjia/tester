# CANoe 批量执行与 SelectInfo.ini 生成实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 实现 CANoe 用例按 config_path 分组合并执行，并在测量启动前生成 SelectInfo.ini

**Architecture:** 修改 `canoe_strategy.py` 的 `run()` 方法，增加分组逻辑和 SelectInfo.ini 生成；在 `config_manager.py` 暴露公开方法 `_generate_select_info_ini`

**Tech Stack:** Python, CANoe COM, TestConfigManager

---

## 文件结构

- Modify: `core/execution_strategies/canoe_strategy.py` — 主逻辑修改
- Modify: `core/config_manager.py` — 暴露公开接口
- Modify: `tests/test_canoe_execution_strategy.py` — 增加批量执行测试

---

## Task 1: 在 config_manager.py 暴露公开方法

**Files:**
- Modify: `core/config_manager.py:178-210`

- [ ] **Step 1: 添加公开方法 `generate_select_info_ini`**

在 `TestConfigManager` 类中添加公开方法，使其可被 `canoe_strategy.py` 调用。

在 `_generate_select_info_ini` 方法下方添加：

```python
def generate_select_info_ini(self, case_nos: List[str], output_dir: str) -> str:
    """
    生成 SelectInfo.ini 文件（公开接口）

    Args:
        case_nos: 用例编号列表，如 ["CANOE-001", "CANOE-002"]
        output_dir: 输出目录（cfg 文件所在目录）

    Returns:
        SelectInfo.ini 文件路径
    """
    select_info_path = os.path.join(output_dir, "SelectInfo.ini")
    self._generate_select_info_ini(select_info_path, [{"caseNo": c} for c in case_nos])
    return select_info_path
```

- [ ] **Step 2: 运行测试验证修改未破坏现有功能**

Run: `pytest tests/test_config_manager.py -v -k "not slow" --tb=short 2>&1 | head -50`
Expected: 所有测试通过

- [ ] **Step 3: Commit**

```bash
git add core/config_manager.py
git commit -m "feat(config_manager): expose generate_select_info_ini as public method"
```

---

## Task 2: 修改 canoe_strategy.py — 分组和 SelectInfo.ini 生成

**Files:**
- Modify: `core/execution_strategies/canoe_strategy.py`

- [ ] **Step 1: 添加 `_group_cases_by_config_path` 方法**

在 `CANoeExecutionStrategy` 类中，`run()` 方法之前添加：

```python
def _group_cases_by_config_path(
    self, cases: list, config_path: str | None
) -> dict[str, list]:
    """
    将用例按 config_path 分组。

    由于 CANoe 中 config_path 相同的用例在同一配置下执行，
    按 config_path 分组可以实现批量执行。

    Returns:
        dict[str, list] — {config_path: [cases]}
    """
    groups: dict[str, list] = {}
    for case in cases:
        path = config_path or ""
        if path not in groups:
            groups[path] = []
        groups[path].append(case)
    return groups
```

- [ ] **Step 2: 添加 `_generate_select_info_ini_for_group` 方法**

在 `_group_cases_by_config_path` 方法之后添加：

```python
def _generate_select_info_ini_for_group(
    self, cases: list, config_path: str
) -> None:
    """
    为一组用例生成 SelectInfo.ini，写入 cfg 所在目录。

    Args:
        cases: 同组用例列表
        config_path: .cfg 文件路径

    Raises:
        RuntimeError: SelectInfo.ini 生成失败
    """
    if not config_path:
        return

    cfg_dir = os.path.dirname(config_path)
    case_nos = [getattr(case, "case_no", "") or getattr(case, "caseName", "") or "" for case in cases]
    case_nos = [c for c in case_nos if c]

    if not case_nos:
        return

    try:
        from core.config_manager import TestConfigManager
        config_manager = TestConfigManager()
        config_manager.generate_select_info_ini(case_nos, cfg_dir)
    except Exception as e:
        raise RuntimeError(f"Failed to generate SelectInfo.ini: {e}")
```

- [ ] **Step 3: 修改 `run()` 方法以支持分组执行**

找到 `run()` 方法（第100行附近），修改其核心循环逻辑。

原逻辑（逐个执行）：
```python
for case in plan_cases:
    raw_result = test_module_capability.execute_module(
        getattr(case, "case_name", ""),
        timeout=self._timeout_for(plan),
    )
```

新逻辑（按 config_path 分组，每组一次测量）：
```python
# 按 config_path 分组
groups = self._group_cases_by_config_path(plan_cases, config_path)

for group_path, group_cases in groups.items():
    # 生成 SelectInfo.ini
    self._generate_select_info_ini_for_group(group_cases, group_path)

    # 加载配置
    self._load_configuration(adapter, group_path)

    # 启动测量
    self._start_measurement(adapter)

    try:
        for case in group_cases:
            raw_result = test_module_capability.execute_module(
                getattr(case, "case_name", ""),
                timeout=self._timeout_for(plan),
            )
            test_result = self._build_test_result(case, raw_result)
            results.append(test_result)
            self._collect_results(runtime_collector, [test_result])

            if raw_result:
                if raw_result.get("report_path"):
                    artifacts["report_path"] = raw_result["report_path"]
                if raw_result.get("log_path"):
                    artifacts["log_path"] = raw_result["log_path"]
                if raw_result.get("testdata_path"):
                    artifacts["testdata_path"] = raw_result["testdata_path"]
    finally:
        self._stop_measurement(adapter)
```

同时，在方法开头添加 `import os`（如果尚未导入）。

- [ ] **Step 4: 运行测试验证修改未破坏现有功能**

Run: `pytest tests/test_canoe_execution_strategy.py -v --tb=short 2>&1 | head -60`
Expected: 原有测试通过

- [ ] **Step 5: Commit**

```bash
git add core/execution_strategies/canoe_strategy.py
git commit -m "feat(canoe_strategy): add batch execution by config_path and SelectInfo.ini generation"
```

---

## Task 3: 添加批量执行测试

**Files:**
- Modify: `tests/test_canoe_execution_strategy.py`

- [ ] **Step 1: 添加分组执行测试**

在 `test_canoe_execution_strategy.py` 末尾添加：

```python
def test_canoe_strategy_groups_cases_by_config_path():
    """Test that CANoe strategy groups cases by config_path and executes them in batch."""
    strategy = CANoeExecutionStrategy()
    executed_paths: list[tuple] = []

    class _TestModuleCapability:
        def execute_module(self, module_name, timeout=None):
            executed_paths.append(("execute_module", module_name, timeout))
            return {"verdict": "PASS", "details": {"module": module_name}}

    class _ConfigurationCapability:
        def load(self, config_path):
            executed_paths.append(("load", config_path))
            return True

    class _MeasurementCapability:
        def start(self):
            executed_paths.append(("start_measurement", None))
            return True

        def stop(self):
            executed_paths.append(("stop_measurement", None))

    class _Adapter:
        def get_capability(self, name, default=None):
            mapping = {
                "configuration": _ConfigurationCapability(),
                "measurement": _MeasurementCapability(),
                "test_module": _TestModuleCapability(),
            }
            return mapping.get(name, default)

    class _Plan:
        timeout_seconds = 30
        cases = [
            type("_Case", (), {
                "case_name": "ModuleA",
                "case_type": "test_module",
                "case_no": "CANOE-001"
            })(),
            type("_Case", (), {
                "case_name": "ModuleB",
                "case_type": "test_module",
                "case_no": "CANOE-002"
            })(),
        ]

    collector = ResultCollector("CANOE-BATCH-001")

    outcome = strategy.run(
        _Plan(),
        adapter=_Adapter(),
        collector=collector,
        config_path=r"D:\TAMS\DTTC_CONFIG\S59\BCANFD\Test.cfg"
    )

    # Verify grouping: same config_path means one load + one start_measurement
    load_calls = [c for c in executed_paths if c[0] == "load"]
    start_calls = [c for c in executed_paths if c[0] == "start_measurement"]
    stop_calls = [c for c in executed_paths if c[0] == "stop_measurement"]

    assert len(load_calls) == 1, f"Expected 1 load call, got {len(load_calls)}"
    assert len(start_calls) == 1, f"Expected 1 start_measurement call, got {len(start_calls)}"
    assert len(stop_calls) == 1, f"Expected 1 stop_measurement call, got {len(stop_calls)}"

    # Verify all cases executed
    execute_calls = [c for c in executed_paths if c[0] == "execute_module"]
    assert len(execute_calls) == 2
    assert outcome.status == "completed"
    assert outcome.summary["total"] == 2


def test_canoe_strategy_generates_select_info_ini(tmp_path):
    """Test that CANoe strategy generates SelectInfo.ini before measurement."""
    strategy = CANoeExecutionStrategy()
    config_dir = tmp_path / "cfg"
    config_dir.mkdir()
    cfg_file = config_dir / "Test.cfg"
    cfg_file.write_text("")
    ini_file = config_dir / "SelectInfo.ini"

    class _TestModuleCapability:
        def execute_module(self, module_name, timeout=None):
            return {"verdict": "PASS", "details": {"module": module_name}}

    class _ConfigurationCapability:
        def load(self, config_path):
            return True

    class _MeasurementCapability:
        def start(self):
            # Verify SelectInfo.ini exists before measurement starts
            assert ini_file.exists(), "SelectInfo.ini should be generated before start_measurement"
            content = ini_file.read_text()
            assert "CANOE-001" in content, f"SelectInfo.ini should contain case_no, got: {content}"
            return True

        def stop(self):
            pass

    class _Adapter:
        def get_capability(self, name, default=None):
            mapping = {
                "configuration": _ConfigurationCapability(),
                "measurement": _MeasurementCapability(),
                "test_module": _TestModuleCapability(),
            }
            return mapping.get(name, default)

    class _Plan:
        timeout_seconds = 30
        cases = [
            type("_Case", (), {
                "case_name": "ModuleA",
                "case_type": "test_module",
                "case_no": "CANOE-001"
            })(),
        ]

    collector = ResultCollector("CANOE-INI-001")

    outcome = strategy.run(
        _Plan(),
        adapter=_Adapter(),
        collector=collector,
        config_path=str(cfg_file)
    )

    assert ini_file.exists()
    content = ini_file.read_text()
    assert "[CFG_PARA]" in content
    assert "CANOE-001=1" in content
```

- [ ] **Step 2: 运行新测试验证**

Run: `pytest tests/test_canoe_execution_strategy.py::test_canoe_strategy_groups_cases_by_config_path -v --tb=short`
Expected: PASS

Run: `pytest tests/test_canoe_execution_strategy.py::test_canoe_strategy_generates_select_info_ini -v --tb=short`
Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add tests/test_canoe_execution_strategy.py
git commit -m "test(canoe_strategy): add tests for batch execution and SelectInfo.ini generation"
```

---

## 自检清单

1. **Spec 覆盖**: 设计方案中的"分组逻辑"和"SelectInfo.ini 生成"是否都有对应的 Task 实现？**是**
2. **Placeholder 扫描**: 是否有 "TBD"、"TODO" 或 "填充细节" 等占位符？**无**
3. **类型一致性**: `PlannedCase` 的 `case_no` 属性（line 18 of execution_plan.py）和代码中使用的 `getattr(case, "case_no", "")` 是否一致？**是**
4. **现有测试**: 原有 `test_canoe_execution_strategy.py` 测试全部通过吗？需要验证
5. **Import**: `canoe_strategy.py` 是否需要添加 `import os`？**是**（用于 `os.path.dirname`）
