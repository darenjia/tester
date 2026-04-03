# P0-P1 稳定性修复设计文档

## 概述

修复 `code_review_report.txt` 中识别的 6 个 P0/P1 级问题，使执行器在 TDM 平台调度下成为一个**可用且稳健的 Release 版本**。

修复范围：
- P0 级（3个）：幽灵任务兜底、熔断器爆炸半径、TSMaster 报告解析（标记 TODO）
- P1 级（3个）：双重队列统一、配置解析解耦、适配器生命周期管理

---

## P0 级修复

### P0-1: 幽灵任务与兜底上报漏洞

**问题**：`task_executor_production.py` 的 `_fail_task` 方法（:875）和 `_execute_task_production` 的 `except` 块中，强依赖 `self.current_collector` 存在。如果在 `ConfigPreparationPhase` 阶段（:621-637）崩溃，`current_collector` 还未初始化（:613），此时 `_fail_task` 会跳过结果收集直接警告日志（:966-967），任务可能永远卡在"执行中"。

**修复方案**：在 `_fail_task` 方法开头增加防御性代码，确保即使 `current_collector` 未初始化也能兜底处理。

**修改文件**：`core/task_executor_production.py`

**修改点**：
```python
def _fail_task(self, task: Task, error_message: str):
    task_id = self._task_id(task)

    # 防御性初始化：如果 current_collector 未初始化，创建一个兜底的
    if not self.current_collector:
        self.current_collector = ResultCollector(task_id)
        self.current_collector.add_log("WARNING", "current_collector 未初始化，使用兜底收集器")
```

**验收标准**：
- 在 `ConfigPreparationPhase` 阶段制造异常，任务仍能上报 `BLOCK` 状态而非卡在 `RUNNING`
- `_report_to_remote` 在任何异常路径都能被调用

---

### P0-2: TSMaster 报告解析闭环

**问题**：`tsmaster_adapter.py` 的 `get_test_report_info`（:706）仅返回统计数字，缺失 `results` 详细用例列表。

**修复方案**：**本次标记为 TODO，暂不实现 XML 解析**。仅在 `get_test_report_info` 返回值中增加注释说明后续需要解析 XML 报告获取详细用例列表。理由：TSMaster XML 报告格式需确认，且该改动不影响核心执行闭环。

**修改文件**：`core/adapters/tsmaster_adapter.py`

**修改点**：在 `get_test_report_info` 方法 docstring 中添加 TODO 注释：
```python
def get_test_report_info(self) -> Optional[Dict[str, Any]]:
    """
    Get test report paths and statistics after execution

    Returns:
        Dict with report_path, testdata_path, passed, failed, total
        Returns None if not connected or error

    TODO: 需要解析 XML 报告获取详细用例列表，组装为:
        "results": [{"name": "TC01", "verdict": "PASS", "duration": 1.2, ...}, ...]
    """
```

**验收标准**：
- 方法返回结构不变，不引入破坏性变更
- TODO 注释清晰说明后续需要的实现

---

### P0-3: 熔断器爆炸半径

**问题**：全局熔断器 `TASK_CIRCUIT_BREAKER`（:83-87）设置 `expected_exception=Exception`，导致普通用例执行失败（如 `AssertionError`）也会触发全局熔断。

**修复方案**：缩小 `expected_exception` 范围，只捕获工具级异常。

**修改文件**：`core/task_executor_production.py`

**修改点**：
```python
# 任务执行熔断器 - 只对工具级异常熔断，普通用例失败不触发熔断
TASK_CIRCUIT_BREAKER = CircuitBreaker(
    failure_threshold=10,
    recovery_timeout=300.0,
    expected_exception=(ToolException, ConnectionError, TimeoutError, OSError)
)
```

**同时修改** `_execute_task_production` 中的异常处理（:698-714），确保 `TaskException`（配置找不到、用例映射错误）不触发熔断：
```python
except TaskException as e:
    # 用例级配置/映射错误不触发熔断
    self._current_metrics.record_error()
    self._fail_task(task, str(e))
    return  # 不调用 record_failure()

except ToolException as e:
    self._current_metrics.record_error()
    TASK_CIRCUIT_BREAKER.record_failure()  # 只有工具异常触发熔断
    self._fail_task(task, f"工具操作失败: {e}")
```

**验收标准**：
- 用例级别的 `AssertionError` 不会触发熔断
- 只有 `ToolException`、`ConnectionError`、`TimeoutError`、`OSError` 触发熔断

---

## P1 级修复

### P1-1: 双重队列状态同步

**问题**：全局 `task_queue`（单例，持久化到 `data/tasks.json`）和内部 `self._task_queue` 同时存在。`submit_task` 中双重入队（:508 和 :1035），状态同步复杂，易产生状态撕裂。

**修复方案**：**统一为一个队列**——以全局 `task_queue` 为唯一数据源，移除内部 `_task_queue` 的状态管理能力，内部只保留 Python `queue.Queue` 暂存 task_id 引用用于取任务，工作线程直接从全局队列获取任务。

**修改文件**：
- `core/task_executor_production.py`

**修改点**：
1. 移除 `TaskExecutorProduction` 中的 `_task_queue: TaskQueue`（第 178 行），替换为轻量级 `queue.Queue`
2. 修改 `_worker_loop`（:407）：从 `global_task_queue.get()` 获取任务而不是内部队列
3. 修改 `submit_task`（:1010）：只添加到 `global_task_queue`，不双重入队
4. 修改 `cancel_task`（:981）：通过 `global_task_queue.remove()` 取消
5. 修改 `retry_task`（:1050）：通过 `global_task_queue` 重试
6. 移除 `TaskQueue` 内部类（:89-157）——如无其他引用

**关键逻辑**：
```
旧逻辑：
  submit_task() → global_task_queue.add() + self._task_queue.put()
  worker_loop  → self._task_queue.get()

新逻辑：
  submit_task() → global_task_queue.add()
  worker_loop  → global_task_queue.get()  # 统一入口
```

**验收标准**：
- 任务提交、取消、重试全部通过 `global_task_queue` 操作
- 内部不再维护独立的任务状态副本
- 应用重启后 `global_task_queue` 恢复的 RUNNING 任务仍能正确标记为 FAILED（:208-212）

---

### P1-2: 配置解析逻辑解耦

**问题**：`_execute_task_production` 中包含数十行配置准备逻辑（:621-637 的 `ConfigPreparationPhase`），难以单元测试。

**修复方案**：将 `ConfigPreparationPhase` 重构为独立类 `TaskConfigResolver`，暴露 `resolve(task) -> ExecutionPlan` 接口。

**修改文件**：
- `core/task_executor_production.py`（移除内联逻辑，调用 Resolver）
- 新建 `core/task_config_resolver.py`（可选，如逻辑复杂则新建）

**修改点**：
1. 将 `ConfigPreparationPhase` 类（如果存在）或其逻辑提取为 `TaskConfigResolver`
2. 在 `_execute_task_production` 中调用 `resolver.resolve(task)` 而不是内联 `prep_phase.prepare(task)`
3. 配置查找失败时抛出 `TaskException`（已经被 `_fail_task` 捕获）

**验收标准**：
- `TaskConfigResolver` 可以独立实例化并测试
- `_execute_task_production` 中配置准备逻辑少于 10 行

---

### P1-3: 适配器生命周期管理

**问题**：`BaseTestAdapter` 没有实现 `__enter__` / `__exit__` 上下文管理器，如遇硬崩溃 `disconnect()` 可能不被调用。

**修复方案**：在 `BaseTestAdapter` 中实现上下文管理器协议，在 `TaskExecutorProduction._execute_task_production` 中使用 `with create_adapter(...) as adapter:` 语法。

**修改文件**：
- `core/adapters/base_adapter.py`
- `core/task_executor_production.py`

**修改点**：

`BaseTestAdapter`（`base_adapter.py`）增加：
```python
def __enter__(self) -> 'BaseTestAdapter':
    """上下文管理器入口"""
    return self

def __exit__(self, exc_type, exc_val, exc_tb):
    """上下文管理器出口，确保断开连接"""
    if self.is_connected:
        self.disconnect()
    return False  # 不吞没异常
```

`TaskExecutorProduction._execute_task_production`（`task_executor_production.py`）修改适配器创建部分：
```python
# 旧代码：
raw_adapter = create_adapter(adapter_type, config=adapter_cfg, singleton=False)
self.controller = raw_adapter

# 新代码：
with create_adapter(adapter_type, config=adapter_cfg, singleton=False) as adapter:
    self.controller = adapter
    # ... 执行逻辑 ...
# with 块结束自动调用 adapter.__exit__ → disconnect()
```

**验收标准**：
- `with` 块内正常结束和异常抛出都会调用 `disconnect()`
- 现有非 `with` 语法的 `disconnect()` 调用（如 `_cleanup`）仍能正常工作

---

## 修改文件清单

| 文件 | 修改类型 | 涉及问题 |
|------|---------|---------|
| `core/task_executor_production.py` | 修改 | P0-1, P0-3, P1-1, P1-2, P1-3 |
| `core/adapters/base_adapter.py` | 修改 | P1-3 |
| `core/adapters/tsmaster_adapter.py` | 修改 | P0-2（仅加 TODO 注释） |
| `core/task_config_resolver.py` | 新建（可选） | P1-2 |

---

## 测试计划

1. **P0-1 幽灵任务**：在 `ConfigPreparationPhase` 阶段注入异常，验证任务状态为 FAILED 且上报到远端
2. **P0-3 熔断器**：模拟 `ToolException`，验证熔断器计数增加；模拟 `AssertionError`，验证不触发熔断
3. **P1-1 队列统一**：提交任务后取消，验证两个队列状态一致；重启应用，验证队列恢复正确
4. **P1-3 上下文管理器**：在 `with` 块内抛出异常，验证 `disconnect()` 仍被调用

---

## 风险评估

| 修复项 | 风险等级 | 缓解措施 |
|--------|---------|---------|
| P1-1 队列统一 | 中 | 保留 `global_task_queue` 为唯一数据源，只移除内部队列状态管理，改动范围可控 |
| P1-2 ConfigResolver | 低 | 现有 `ConfigPreparationPhase` 逻辑平移，不改变业务行为 |
| P1-3 上下文管理器 | 低 | 保持向后兼容，现有 `disconnect()` 调用不受影响 |
