# P0-P1 稳定性修复实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 修复 6 个 P0/P1 级稳定性问题，使执行器成为可用且稳健的 Release 版本

**Architecture:** 3 个 P0 修改（熔断器收窄、幽灵任务兜底、TSMaster TODO）+ 3 个 P1 重构（队列统一、配置解耦、适配器上下文管理）

**Tech Stack:** Python, CircuitBreaker, Context Manager, TaskQueue singleton

---

## 文件修改总览

| 文件 | 修改类型 | 负责任务 |
|------|---------|---------|
| `core/task_executor_production.py` | 修改 | P0-1, P0-3, P1-1, P1-2, P1-3 |
| `core/adapters/base_adapter.py` | 修改 | P1-3 |
| `core/adapters/tsmaster_adapter.py` | 修改 | P0-2 |

---

## Task 1: P0-3 熔断器爆炸半径收窄

**Files:**
- Modify: `core/task_executor_production.py:83-87`

- [ ] **Step 1: 修改熔断器 expected_exception 范围**

将 `core/task_executor_production.py` 第 83-87 行：
```python
# 旧代码
TASK_CIRCUIT_BREAKER = CircuitBreaker(
    failure_threshold=10,
    recovery_timeout=300.0,
    expected_exception=Exception
)
```
改为：
```python
# 任务执行熔断器 - 只对工具级异常熔断，普通用例失败不触发熔断
TASK_CIRCUIT_BREAKER = CircuitBreaker(
    failure_threshold=10,
    recovery_timeout=300.0,
    expected_exception=(ToolException, ConnectionError, TimeoutError, OSError)
)
```

- [ ] **Step 2: 验证 TaskException 不触发熔断**

确认 `core/task_executor_production.py` 第 698-702 行的 `TaskException` 处理分支**不调用** `TASK_CIRCUIT_BREAKER.record_failure()`：
```python
except TaskException as e:
    logger.error(f"任务执行失败: {e}")
    self._current_metrics.record_error()
    # 注意：这里没有 TASK_CIRCUIT_BREAKER.record_failure()
    self._fail_task(task, str(e))
    return  # 新增：确保不继续执行到后面
```

（如果 `return` 语句不存在，在 `self._fail_task(task, str(e))` 后添加 `return`）

- [ ] **Step 3: 提交**

```bash
git add core/task_executor_production.py
git commit -m "fix(circuit-breaker): narrow expected_exception to tool-level exceptions only"
```

---

## Task 2: P0-1 幽灵任务与兜底上报

**Files:**
- Modify: `core/task_executor_production.py:875-970`（_fail_task 方法）

- [ ] **Step 1: 在 _fail_task 开头添加防御性初始化**

在 `core/task_executor_production.py` 第 875 行 `_fail_task` 方法开头，在 `task_id = self._task_id(task)` 后、`logger.error` 前插入：

```python
def _fail_task(self, task: Task, error_message: str):
    task_id = self._task_id(task)
    logger.error(f"任务失败: {task_id}, 错误: {error_message}")

    # 防御性初始化：如果 current_collector 未初始化（配置阶段崩溃场景），创建兜底的
    if not self.current_collector:
        self.current_collector = ResultCollector(task_id)
        self.current_collector.add_log("WARNING", f"current_collector 未初始化，使用兜底收集器，上报错误: {error_message}")
        logger.warning(f"[_fail_task] current_collector 为 None，已创建兜底收集器")
```

- [ ] **Step 2: 删除原有的 else 分支警告日志**

原来第 966-967 行的：
```python
else:
    logger.warning(f"[_fail_task] current_collector 为 None，无法上报结果")
```
这个 `else` 分支在防御性初始化后永远不会走到，可以删除（或保留作为调试信息，保留也无害）。

- [ ] **Step 3: 提交**

```bash
git add core/task_executor_production.py
git commit -m "fix(executor): add defensive ResultCollector initialization in _fail_task"
```

---

## Task 3: P0-2 TSMaster 报告解析标记 TODO

**Files:**
- Modify: `core/adapters/tsmaster_adapter.py:706-720`

- [ ] **Step 1: 在 get_test_report_info docstring 中添加 TODO**

在 `core/adapters/tsmaster_adapter.py` 第 706 行 `get_test_report_info` 方法的 docstring 中，在 `Returns:` 段落后、方法的实际代码前插入：

```python
def get_test_report_info(self) -> Optional[Dict[str, Any]]:
    """
    Get test report paths and statistics after execution

    Returns:
        Dict with report_path, testdata_path, passed, failed, total
        Returns None if not connected or error

    TODO: 当前只返回统计数字(passed/failed/total)，缺失 results 详细用例列表。
          后续需要解析 XML 报告文件，提取每个 <TestCase> 的 name/verdict/duration
          等属性，组装为: "results": [{"name": "TC01", "verdict": "PASS", ...}, ...]
          对接 tsmaster_strategy.py 的预期格式。
    """
```

- [ ] **Step 2: 提交**

```bash
git add core/adapters/tsmaster_adapter.py
git commit -m "docs(tsmaster): add TODO for results detailed list in get_test_report_info"
```

---

## Task 4: P1-3 适配器上下文管理器

**Files:**
- Modify: `core/adapters/base_adapter.py`
- Modify: `core/task_executor_production.py:658-680`（适配器创建和使用部分）

- [ ] **Step 1: 在 BaseTestAdapter 添加上下文管理器协议**

在 `core/adapters/base_adapter.py` 文件末尾（最后一个方法之后、类定义结束之前）添加：

```python
def __enter__(self) -> 'BaseTestAdapter':
    """上下文管理器入口"""
    return self

def __exit__(self, exc_type, exc_val, exc_tb):
    """上下文管理器出口，确保断开连接"""
    try:
        if self.is_connected:
            self.disconnect()
    except Exception as e:
        self.logger.warning(f"上下文管理器退出时断开连接失败: {e}")
    return False  # 不吞没异常
```

- [ ] **Step 2: 修改 _execute_task_production 中适配器创建逻辑**

在 `core/task_executor_production.py` 第 658-680 行，将：
```python
# 旧代码（约第 671-676 行）：
raw_adapter = create_adapter(adapter_type, config=adapter_cfg, singleton=False)
self.controller = raw_adapter
logger.info(f"已创建适配器: {adapter_type.value}, config keys={list(adapter_cfg.keys())}")
```
改为：
```python
# 使用上下文管理器创建适配器，确保异常时也能正确释放资源
self.controller = create_adapter(adapter_type, config=adapter_cfg, singleton=False)
logger.info(f"已创建适配器: {adapter_type.value}, config keys={list(adapter_cfg.keys())}")
```
（保留 `self.controller =` 赋值，上下文管理由外层 `with` 块负责，见下一步）

- [ ] **Step 3: 将整个适配器使用范围包裹在 with 块中**

找到 `_execute_task_production` 中适配器创建后的所有使用代码（从第 671 行 `self.controller = raw_adapter` 到第 689 行 `results = self._run_strategy_execution(...)` 之后 `self._complete_task` 之前），用 `with` 块包裹：

在 `self.controller = create_adapter(...)` 之前添加 `with`，并在其后的 `self._complete_task` 调用之前添加 `with` 块的结束。

具体来说，在约第 675 行 `self.controller = create_adapter(...)` 之前加一行 `with create_adapter(adapter_type, config=adapter_cfg, singleton=False) as self.controller:`，并删除那行独立的 `self.controller = create_adapter(...)`。

然后在 `self._complete_task(task, results)` 之前（大约第 693 行）确认 `with` 块正确闭合。

注意：修改后 `with` 块内的代码缩进需要增加一级（约 4 个空格），范围从适配器创建到 strategy 执行完成。`with` 块自动在离开时调用 `adapter.__exit__` → `disconnect()`，因此 `_cleanup()` 中的 `self.controller.disconnect()` 调用变为冗余，可以删除或保留（已有 `if self.controller:` 检查所以无害）。

- [ ] **Step 4: 提交**

```bash
git add core/adapters/base_adapter.py core/task_executor_production.py
git commit -m "feat(adapter): add __enter__/__exit__ context manager protocol"
```

---

## Task 5: P1-1 双重队列状态统一

**Files:**
- Modify: `core/task_executor_production.py`（多处）

**关键变更**：移除 `self._task_queue`（第 178 行定义的内部 TaskQueue 实例），将所有引用替换为 `global_task_queue`（第 24 行导入的单例）

- [ ] **Step 1: 替换类属性 _task_queue 定义**

将第 178 行：
```python
self._task_queue = TaskQueue()
```
删除这行。因为不再需要内部队列。

- [ ] **Step 2: 替换 _worker_loop 中的取任务逻辑**

将第 412 行：
```python
task = self._task_queue.get(timeout=1.0)
```
改为：
```python
task = global_task_queue.get()
```
同时删除 `self._task_queue.get_queue_size()` 相关调用（如果 worker_loop 中有的话）。如果没有，保留其他引用到 `global_task_queue`。

- [ ] **Step 3: 修改 submit_task 方法**

将 `submit_task` 中第 1035 行：
```python
if not self._task_queue.put(execution_plan):
```
删除这行和对应的 `if` 块（第 1034-1041 行）。因为 `global_task_queue.add(task)` 已经在第 1030 行执行过了，不需要再次添加到内部队列。

同时将第 1030 行的 `global_task_queue.add(task)` 检查逻辑简化：直接调用 `global_task_queue.add(task)` 并检查返回值即可。

- [ ] **Step 4: 修改 cancel_task 方法**

将第 983 行和第 995 行中的 `self._task_queue.remove(task_id)` 改为 `global_task_queue.remove(task_id)`：
```python
if task_id and global_task_queue.remove(task_id):
```
和
```python
if global_task_queue.remove(task_id):
```

- [ ] **Step 5: 修改 get_current_status 和 get_stats**

将 `get_current_status` 方法（第 1503-1504 行）中的：
```python
"queue_size": self._task_queue.get_queue_size(),
"processing_count": self._task_queue.get_processing_count()
```
改为：
```python
"queue_size": global_task_queue.size(),
"processing_count": global_task_queue.total_count()
```

将 `get_stats` 方法（第 1539 行）中的：
```python
"queue_size": self._task_queue.get_queue_size(),
```
改为：
```python
"queue_size": global_task_queue.size(),
```

将 `get_running_count` 方法（第 1528 行）中的：
```python
return 1 if self._task_queue.get_processing_count() > 0 else 0
```
改为：
```python
return 1 if global_task_queue.get_running_count() > 0 else 0
```
（如果 `global_task_queue` 没有 `get_running_count()` 方法，可以使用 `len(global_task_queue.get_running_tasks())`）

- [ ] **Step 6: 移除 mark_processing/mark_completed 调用**

删除第 598 行 `self._task_queue.mark_processing(task_id, task)` 和第 740 行 `self._task_queue.mark_completed(...)` 的调用。这些是内部队列的状态跟踪，移除后状态统一由 `global_task_queue.update_task_status()` 管理。

- [ ] **Step 7: 提交**

```bash
git add core/task_executor_production.py
git commit -m "refactor(executor): remove internal _task_queue, use global_task_queue as single source of truth"
```

---

## Task 6: P1-2 配置解析逻辑解耦

**Files:**
- Modify: `core/task_executor_production.py:620-638`
- Create: `core/task_config_resolver.py`（可选，视逻辑复杂度决定）

- [ ] **Step 1: 评估 ConfigPreparationPhase 复杂度**

打开 `core/config_preparation.py` 或相关文件（如果存在），查看 `ConfigPreparationPhase` 类的实现规模：

- 如果逻辑 < 50 行且可直接移动 → 在 `task_executor_production.py` 内新建一个 `_resolve_task_config` 私有方法
- 如果逻辑 > 50 行或有多处复用 → 新建 `core/task_config_resolver.py`

根据代码，`ConfigPreparationPhase` 已在 `core/config_preparation.py` 中实现（第 621 行 `prep_phase = ConfigPreparationPhase()`），只需将调用包装为独立方法。

- [ ] **Step 2: 在 TaskExecutorProduction 中添加配置解析代理方法**

在 `core/task_executor_production.py` 的 `_execute_task_production` 方法之前（约第 574 行之前）添加：

```python
def _resolve_task_config(self, task: ExecutionPlan) -> ExecutionPlan:
    """
    配置解析代理方法

    将配置准备逻辑从 _execute_task_production 中提取出来，单独管理。
    内部委托给 ConfigPreparationPhase 处理。

    Args:
        task: 原始执行计划

    Returns:
        配置解析后的执行计划

    Raises:
        TaskException: 配置冲突、缺失或准备失败时
    """
    try:
        prep_phase = ConfigPreparationPhase()
        task = prep_phase.prepare(task)
        self.logger.info(
            f"[_resolve_task_config] 配置准备完成: "
            f"config_source={task.config_source}, config_path={task.config_path}"
        )
        return task
    except ConfigConflictError as e:
        raise TaskException(f"配置冲突: {e}")
    except MissingConfigError as e:
        raise TaskException(f"缺少配置: {e}")
    except ConfigPreparationError as e:
        raise TaskException(f"配置准备失败: {e}")
```

- [ ] **Step 3: 替换 _execute_task_production 中的配置准备代码**

将第 620-637 行的配置准备代码块：
```python
# ========== 配置准备（使用 ConfigPreparationPhase） ==========
try:
    prep_phase = ConfigPreparationPhase()
    task = prep_phase.prepare(task)
    logger.info(...)
    self.current_collector.add_log("INFO", f"配置来源: {task.config_source.value}")
    ...
except ConfigConflictError as e:
    raise TaskException(f"配置冲突: {e}")
...
except ConfigPreparationError as e:
    raise TaskException(f"配置准备失败: {e}")
```
替换为：
```python
# ========== 配置准备（使用 TaskConfigResolver） ==========
try:
    task = self._resolve_task_config(task)
    self.current_collector.add_log("INFO", f"配置来源: {task.config_source.value}")
    if task.config_path:
        self.current_collector.add_log("INFO", f"cfg文件: {task.config_path}")
except TaskException:
    raise  # _resolve_task_config 已将内部异常转换为 TaskException，直接抛出
```

注意：保留 `self.current_collector.add_log` 调用（这些是 executor 级别的日志，不是 resolver 级别的）。

- [ ] **Step 4: 提交**

```bash
git add core/task_executor_production.py
git commit -m "refactor(executor): extract config preparation to _resolve_task_config method"
```

---

## Task 7: 全局验证

- [ ] **Step 1: 运行 pytest 确保没有语法错误**

```bash
cd D:/Deng/can_test/python_executor
python -m py_compile core/task_executor_production.py
python -m py_compile core/adapters/base_adapter.py
python -m py_compile core/adapters/tsmaster_adapter.py
echo "Syntax check passed"
```

- [ ] **Step 2: 检查是否有 import 缺失**

```bash
python -c "from core.task_executor_production import TaskExecutorProduction; print('Import OK')"
```

- [ ] **Step 3: 提交所有剩余更改**

```bash
git status
git add -A
git commit -m "chore: complete P0-P1 stability fixes"
```

---

## 实施顺序

1. Task 1 (P0-3 熔断器) - 影响异常处理逻辑，先修
2. Task 2 (P0-1 幽灵任务) - 独立修改
3. Task 3 (P0-2 TODO) - 纯文档修改
4. Task 4 (P1-3 上下文管理器) - 修改 executor 适配器使用方式
5. Task 5 (P1-1 队列统一) - 修改量最大，提前做便于后续修改无干扰
6. Task 6 (P1-2 配置解耦) - 最后做，因为依赖 executor 其他逻辑稳定
7. Task 7 全局验证
