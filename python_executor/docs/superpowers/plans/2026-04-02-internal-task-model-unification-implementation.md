# Internal Task Model Unification Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Introduce a single internal `ExecutionPlan` model and `TaskCompiler`, then migrate the production execution path to consume only that internal model while keeping external API and queue/view models stable.

**Architecture:** Add new internal planning models and a compiler layer between `main_production.py` and `TaskExecutorProduction`. Migrate the executor to a new `execute_plan()` path, align lifecycle stages with the real flow, then remove old internal compatibility reads from the production path.

**Tech Stack:** Python 3, `dataclasses`, Flask/SocketIO, pytest, existing observability and queue modules

---

## File Structure

- Create: `core/execution_plan.py`
  - Defines `ExecutionPlan`, `PlannedCase`, and small supporting enums/constants for internal execution.
- Create: `core/task_compiler.py`
  - Compiles platform task/message input plus mappings into `ExecutionPlan`.
- Modify: `main_production.py`
  - Replace inline task assembly with compiler-driven handoff to executor.
- Modify: `core/task_executor_production.py`
  - Add `execute_plan()` and migrate main execution path to consume `ExecutionPlan`.
- Modify: `core/execution_observability.py`
  - Add `compiled` and `collecting` lifecycle stages plus related transitions.
- Modify: `tests/test_task_dispatch_production.py`
  - Shift dispatch tests from `models.task.Task` assertions to compiler/plan assertions.
- Create: `tests/test_task_compiler.py`
  - Unit tests for compiler behavior.
- Create: `tests/test_execution_plan.py`
  - Unit tests for internal plan model defaults and normalization.
- Modify: `tests/test_execution_observability.py`
  - Extend lifecycle stage expectations.
- Modify: `tests/test_config_runtime_integration.py`
  - Verify production path uses compiler-driven plan execution.
- Modify: `README.md`
  - Update internal architecture description after migration is complete.

## Task 1: Add Internal Execution Models

**Files:**
- Create: `core/execution_plan.py`
- Test: `tests/test_execution_plan.py`

- [ ] **Step 1: Write the failing tests for the new internal plan model**

```python
from core.execution_plan import ConfigSource, ExecutionPlan, PlannedCase


def test_execution_plan_defaults_execution_constraints():
    plan = ExecutionPlan(
        task_no="TASK-1",
        project_no="PROJECT-1",
        task_name="Task 1",
        device_id="DEVICE-1",
        tool_type="canoe",
        cases=[
            PlannedCase(
                case_no="CASE-1",
                case_name="Case 1",
                case_type="test_module",
            )
        ],
    )

    assert plan.timeout_seconds == 3600
    assert plan.max_concurrency == 1
    assert plan.report_required is True
    assert plan.config_source == ConfigSource.UNSPECIFIED


def test_planned_case_preserves_execution_fields_only():
    planned_case = PlannedCase(
        case_no="CASE-2",
        case_name="Case 2",
        case_type="diagnostic",
        repeat=2,
        dtc_info="P1234",
        execution_params={"iniConfig": "TG1_TC2=1"},
        mapping_metadata={"category": "tsmaster"},
    )

    assert planned_case.case_no == "CASE-2"
    assert planned_case.repeat == 2
    assert planned_case.execution_params["iniConfig"] == "TG1_TC2=1"
    assert planned_case.mapping_metadata["category"] == "tsmaster"
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m pytest tests/test_execution_plan.py -q`
Expected: FAIL with `ModuleNotFoundError` or missing `ExecutionPlan` / `PlannedCase`

- [ ] **Step 3: Write minimal internal execution model implementation**

```python
from __future__ import annotations

from dataclasses import dataclass, field
from enum import Enum
from typing import Any


class ConfigSource(str, Enum):
    UNSPECIFIED = "unspecified"
    DIRECT_PATH = "direct_path"
    CONFIG_MANAGER = "config_manager"
    CASE_MAPPING = "case_mapping"
    TSMASTER_INLINE = "tsmaster_inline"


@dataclass(slots=True)
class PlannedCase:
    case_no: str
    case_name: str
    case_type: str
    repeat: int = 1
    dtc_info: str | None = None
    execution_params: dict[str, Any] = field(default_factory=dict)
    mapping_metadata: dict[str, Any] = field(default_factory=dict)


@dataclass(slots=True)
class ExecutionPlan:
    task_no: str
    project_no: str
    task_name: str
    device_id: str
    tool_type: str
    cases: list[PlannedCase]
    config_path: str | None = None
    config_name: str | None = None
    base_config_dir: str | None = None
    variables: dict[str, Any] = field(default_factory=dict)
    canoe_namespace: str | None = None
    timeout_seconds: int = 3600
    max_concurrency: int = 1
    retry_policy: dict[str, Any] = field(default_factory=dict)
    report_required: bool = True
    config_source: ConfigSource = ConfigSource.UNSPECIFIED
    resolution_notes: list[str] = field(default_factory=list)
    raw_refs: dict[str, Any] = field(default_factory=dict)
```

- [ ] **Step 4: Run test to verify it passes**

Run: `python -m pytest tests/test_execution_plan.py -q`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add core/execution_plan.py tests/test_execution_plan.py
git commit -m "feat: add internal execution plan models"
```

## Task 2: Add TaskCompiler

**Files:**
- Create: `core/task_compiler.py`
- Test: `tests/test_task_compiler.py`

- [ ] **Step 1: Write the failing compiler tests**

```python
import pytest

from core.execution_plan import ConfigSource
from core.task_compiler import TaskCompiler, TaskCompileError
from models.result import Message


class _FakeMapping:
    def __init__(self, category, enabled=True, script_path=None, case_name="", ini_config=None, para_config=None):
        self.category = category
        self.enabled = enabled
        self.script_path = script_path
        self.case_name = case_name
        self.ini_config = ini_config
        self.para_config = para_config


class _FakeMappingManager:
    def __init__(self, mappings):
        self._mappings = mappings

    def get_mapping(self, case_no):
        return self._mappings.get(case_no)


def test_task_compiler_rejects_mixed_tool_types():
    compiler = TaskCompiler(mapping_manager=_FakeMappingManager({
        "CASE-1": _FakeMapping(category="CANOE"),
        "CASE-2": _FakeMapping(category="TSMASTER"),
    }))
    message = Message(
        type="TASK_DISPATCH",
        taskNo="TASK-1",
        deviceId="DEVICE-1",
        payload={"testItems": [{"case_no": "CASE-1"}, {"case_no": "CASE-2"}]},
    )

    with pytest.raises(TaskCompileError):
        compiler.compile_message(message)


def test_task_compiler_resolves_mapping_backed_execution_plan():
    compiler = TaskCompiler(mapping_manager=_FakeMappingManager({
        "CASE-3": _FakeMapping(
            category="CANOE",
            script_path="D:/cfgs/main.cfg",
            case_name="Case Three",
            ini_config="A=1",
            para_config="B=2",
        )
    }))
    message = Message(
        type="TASK_DISPATCH",
        taskNo="TASK-3",
        deviceId="DEVICE-3",
        payload={"testItems": [{"case_no": "CASE-3", "name": "fallback-name"}]},
    )

    plan = compiler.compile_message(message)

    assert plan.tool_type == "canoe"
    assert plan.config_path == "D:/cfgs/main.cfg"
    assert plan.config_source == ConfigSource.CASE_MAPPING
    assert plan.cases[0].case_name == "Case Three"
    assert plan.cases[0].execution_params["iniConfig"] == "A=1"
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m pytest tests/test_task_compiler.py -q`
Expected: FAIL with `ModuleNotFoundError` or missing `TaskCompiler`

- [ ] **Step 3: Write minimal compiler implementation**

```python
from __future__ import annotations

from dataclasses import dataclass
from typing import Any

from core.execution_plan import ConfigSource, ExecutionPlan, PlannedCase
from models.result import Message


class TaskCompileError(ValueError):
    pass


@dataclass(slots=True)
class TaskCompiler:
    mapping_manager: Any

    def compile_message(self, message: Message) -> ExecutionPlan:
        payload = dict(message.payload or {})
        payload["taskNo"] = message.taskNo
        payload["deviceId"] = message.deviceId
        return self.compile_payload(payload)

    def compile_payload(self, payload: dict[str, Any]) -> ExecutionPlan:
        test_items = payload.get("testItems") or []
        if not test_items:
            raise TaskCompileError("testItems不能为空")

        tool_type = payload.get("toolType")
        discovered_tool_types: set[str] = set()
        planned_cases: list[PlannedCase] = []
        config_path = payload.get("configPath")
        config_source = ConfigSource.DIRECT_PATH if config_path else ConfigSource.UNSPECIFIED

        for item in test_items:
            case_no = item.get("case_no") or item.get("caseNo") or item.get("name", "")
            mapping = self.mapping_manager.get_mapping(case_no) if case_no else None
            if mapping and mapping.category:
                discovered_tool_types.add(mapping.category.lower())
            planned_case = PlannedCase(
                case_no=case_no,
                case_name=(mapping.case_name if mapping and mapping.case_name else item.get("name", "")),
                case_type=item.get("type", "test_module"),
                repeat=item.get("repeat", 1),
                dtc_info=item.get("dtc_info") or item.get("dtcInfo"),
                execution_params={},
                mapping_metadata={},
            )
            if mapping:
                planned_case.mapping_metadata["category"] = mapping.category.lower() if mapping.category else ""
                if mapping.ini_config:
                    planned_case.execution_params["iniConfig"] = mapping.ini_config
                if mapping.para_config:
                    planned_case.execution_params["paraConfig"] = mapping.para_config
                if not config_path and mapping.script_path:
                    config_path = mapping.script_path
                    config_source = ConfigSource.CASE_MAPPING
            planned_cases.append(planned_case)

        final_tool_type = (tool_type or "").lower()
        if final_tool_type:
            discovered_tool_types.add(final_tool_type)

        if len(discovered_tool_types) != 1:
            raise TaskCompileError("任务包含多个工具类型，无法编译")

        return ExecutionPlan(
            task_no=payload.get("taskNo", ""),
            project_no=payload.get("projectNo", ""),
            task_name=payload.get("taskName", ""),
            device_id=payload.get("deviceId", "") or "",
            tool_type=next(iter(discovered_tool_types)),
            cases=planned_cases,
            config_path=config_path,
            config_name=payload.get("configName"),
            base_config_dir=payload.get("baseConfigDir"),
            variables=payload.get("variables", {}),
            canoe_namespace=payload.get("canoeNamespace"),
            timeout_seconds=payload.get("timeout", 3600),
            config_source=config_source,
            resolution_notes=[],
            raw_refs={"message_type": "TASK_DISPATCH"},
        )
```

- [ ] **Step 4: Run test to verify it passes**

Run: `python -m pytest tests/test_task_compiler.py -q`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add core/task_compiler.py tests/test_task_compiler.py
git commit -m "feat: add task compiler for internal execution plans"
```

## Task 3: Add Compiler-Driven Dispatch Path

**Files:**
- Modify: `main_production.py`
- Modify: `tests/test_task_dispatch_production.py`

- [ ] **Step 1: Write the failing dispatch test for compiler handoff**

```python
def test_dispatch_compiles_message_and_calls_execute_plan(dispatcher, monkeypatch):
    class _FakePlan:
        task_no = "task-compiler"
        tool_type = "canoe"
        device_id = "device-compiler"

    compiled_payloads = []

    class _FakeCompiler:
        def compile_message(self, message):
            compiled_payloads.append(message.taskNo)
            return _FakePlan()

    class _FakeExecutorWithPlan(_FakeExecutor):
        def execute_plan(self, plan):
            self.executed_tasks.append(plan)
            return True

    monkeypatch.setattr(main_production, "TaskCompiler", lambda mapping_manager: _FakeCompiler())
    monkeypatch.setattr(main_production, "TaskExecutorProduction", _FakeExecutorWithPlan)

    message = Message(type="TASK_DISPATCH", taskNo="task-compiler", deviceId="device-compiler", payload={"testItems": [{"case_no": "CASE-1"}]})

    dispatcher.executor._handle_task_dispatch(message, "sid-compiler")

    assert compiled_payloads == ["task-compiler"]
    assert dispatcher.executor.task_executor.executed_tasks[0].task_no == "task-compiler"
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m pytest tests/test_task_dispatch_production.py::test_dispatch_compiles_message_and_calls_execute_plan -q`
Expected: FAIL because `TaskCompiler` is not imported/used or `execute_plan()` is not called

- [ ] **Step 3: Update `main_production.py` to use the compiler**

```python
from core.task_compiler import TaskCompiler, TaskCompileError


compiler = TaskCompiler(mapping_manager)
execution_plan = compiler.compile_message(message)

if not self.task_executor:
    self.task_executor = TaskExecutorProduction(
        message_sender=lambda msg: self._send_message_to_client(client_sid, msg)
    )
    self.task_executor.start()

success = self.task_executor.execute_plan(execution_plan)
```

- [ ] **Step 4: Update dispatch error handling to use compiler failures**

```python
except TaskCompileError as e:
    observability_manager.fail(
        message.taskNo,
        error_code="TASK_COMPILE_FAILED",
        error_message=str(e),
        retryable=False,
    )
    record_metric("task.failed", 1, {"task_no": message.taskNo, "stage": "compiled"})
    emit("error", {"error": f"任务编译失败: {e}", "timestamp": int(time.time() * 1000)})
```

- [ ] **Step 5: Run task dispatch tests to verify they pass**

Run: `python -m pytest tests/test_task_dispatch_production.py -q`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add main_production.py tests/test_task_dispatch_production.py
git commit -m "refactor: compile dispatch messages into execution plans"
```

## Task 4: Add Executor Plan Entry Point

**Files:**
- Modify: `core/task_executor_production.py`
- Test: `tests/test_task_compiler.py`

- [ ] **Step 1: Write the failing executor plan submission test**

```python
from core.execution_plan import ExecutionPlan, PlannedCase
from core.task_executor_production import TaskExecutorProduction


def test_execute_plan_queues_internal_execution_plan(monkeypatch):
    executor = TaskExecutorProduction(message_sender=lambda _: None)
    queued_items = []
    monkeypatch.setattr(executor._task_queue, "put", lambda plan: queued_items.append(plan) or True)

    plan = ExecutionPlan(
        task_no="TASK-QUEUE",
        project_no="PROJECT-QUEUE",
        task_name="Queue Task",
        device_id="DEVICE-QUEUE",
        tool_type="canoe",
        cases=[PlannedCase(case_no="CASE-QUEUE", case_name="Case Queue", case_type="test_module")],
    )

    assert executor.execute_plan(plan) is True
    assert queued_items[0] is plan
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m pytest tests/test_task_compiler.py::test_execute_plan_queues_internal_execution_plan -q`
Expected: FAIL because `execute_plan()` does not exist

- [ ] **Step 3: Add `execute_plan()` and keep `execute_task()` as a temporary adapter**

```python
def execute_plan(self, plan: ExecutionPlan) -> bool:
    if not plan.cases:
        logger.error(f"execute_plan 失败: 任务 {plan.task_no} 的 cases 为空")
        return False

    if self._task_queue.put(plan):
        from models.executor_task import Task as ExecutorTask, TaskStatus as ExecutorTaskStatus

        exec_task = ExecutorTask(
            id=plan.task_no,
            name=plan.task_name,
            task_type="test_module",
            priority=1,
            status=ExecutorTaskStatus.PENDING.value,
            params={
                "tool_type": plan.tool_type,
                "config_path": plan.config_path,
                "variables": plan.variables,
            },
            timeout=plan.timeout_seconds,
            metadata={
                "taskNo": plan.task_no,
                "projectNo": plan.project_no,
                "deviceId": plan.device_id,
                "caseCount": len(plan.cases),
            },
        )
        global_task_queue.add(exec_task)
        return True
    return False
```

- [ ] **Step 4: Adapt `execute_task()` to compile/convert rather than remain a second main path**

```python
def execute_task(self, task: Task) -> bool:
    plan = ExecutionPlan(
        task_no=task.task_id,
        project_no=task.projectNo,
        task_name=task.taskName,
        device_id=task.deviceId or "",
        tool_type=task.toolType or "",
        cases=[
            PlannedCase(
                case_no=item.caseNo,
                case_name=item.caseName,
                case_type=item.caseType,
                repeat=item.repeat,
                dtc_info=item.dtcInfo,
                execution_params=item.params,
            )
            for item in task.caseList
        ],
        config_path=task.configPath,
        config_name=task.configName,
        base_config_dir=task.baseConfigDir,
        variables=task.variables,
        canoe_namespace=task.canoeNamespace,
        timeout_seconds=task.timeout,
    )
    return self.execute_plan(plan)
```

- [ ] **Step 5: Run focused executor tests**

Run: `python -m pytest tests/test_task_compiler.py -q`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add core/task_executor_production.py tests/test_task_compiler.py
git commit -m "feat: add executor entry point for execution plans"
```

## Task 5: Migrate Main Execution Flow to ExecutionPlan

**Files:**
- Modify: `core/task_executor_production.py`
- Modify: `tests/test_config_runtime_integration.py`

- [ ] **Step 1: Write the failing integration test for plan-based production execution**

```python
def test_production_dispatch_uses_execute_plan_path(runtime_modules, monkeypatch):
    main_module = runtime_modules["main_module"]
    captured_plans = []

    class _FakeExecutor:
        def __init__(self, message_sender):
            self.message_sender = message_sender
        def start(self):
            pass
        def stop(self):
            pass
        def execute_plan(self, plan):
            captured_plans.append(plan)
            return True

    monkeypatch.setattr(main_module, "TaskExecutorProduction", _FakeExecutor)
    monkeypatch.setattr(main_module.signal, "signal", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_module.config_manager, "start_watcher", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_module.performance_monitor, "start", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_module.performance_monitor, "set_alert_threshold", lambda *args, **kwargs: None)
    monkeypatch.setattr(main_module.performance_monitor, "register_alert_callback", lambda *args, **kwargs: None)

    executor = main_module.PythonExecutorProduction()
    try:
        executor._handle_task_dispatch(
            Message(type="TASK_DISPATCH", taskNo="TASK-I", deviceId="DEVICE-I", payload={"testItems": [{"case_no": "CASE-I"}], "toolType": "canoe"}),
            "sid-I",
        )
    finally:
        executor.shutdown()

    assert captured_plans[0].task_no == "TASK-I"
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m pytest tests/test_config_runtime_integration.py::test_production_dispatch_uses_execute_plan_path -q`
Expected: FAIL because production dispatch still relies on old task execution object path

- [ ] **Step 3: Refactor `_execute_task_production()` to operate on `ExecutionPlan`**

```python
def _execute_task_production(self, plan: ExecutionPlan):
    logger.info(
        f"[_execute_task_production] 开始执行任务: task_no={plan.task_no}, tool_type={plan.tool_type}"
    )
    self.current_task = plan
    ...
    if plan.config_path:
        cfg_path = plan.config_path
    elif plan.config_name or plan.base_config_dir:
        ...
        test_cases=[self._planned_case_to_legacy_dict(case) for case in plan.cases]
        variables=plan.variables
    ...
```

- [ ] **Step 4: Add small adapter helpers inside the executor to reduce churn**

```python
def _planned_case_to_legacy_dict(self, case: PlannedCase) -> dict[str, Any]:
    return {
        "caseNo": case.case_no,
        "caseName": case.case_name,
        "caseType": case.case_type,
        "repeat": case.repeat,
        "dtcInfo": case.dtc_info,
        "params": case.execution_params,
    }


def _iter_plan_cases(self, plan: ExecutionPlan) -> list[PlannedCase]:
    return plan.cases
```

- [ ] **Step 5: Run integration tests**

Run: `python -m pytest tests/test_config_runtime_integration.py tests/test_task_dispatch_production.py -q`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add core/task_executor_production.py tests/test_config_runtime_integration.py tests/test_task_dispatch_production.py
git commit -m "refactor: migrate production execution flow to execution plans"
```

## Task 6: Align Observability Lifecycle With Compiled and Collecting Stages

**Files:**
- Modify: `core/execution_observability.py`
- Modify: `tests/test_execution_observability.py`

- [ ] **Step 1: Write the failing lifecycle stage test**

```python
from core.execution_observability import ExecutionLifecycleStage, ExecutionObservabilityManager


def test_lifecycle_supports_compiled_and_collecting_stages():
    manager = ExecutionObservabilityManager()
    manager.create_context(task_no="TASK-L", device_id="DEVICE-L", tool_type="canoe")
    manager.transition("TASK-L", ExecutionLifecycleStage.COMPILED)
    manager.transition("TASK-L", ExecutionLifecycleStage.QUEUED)
    manager.transition("TASK-L", ExecutionLifecycleStage.COLLECTING)

    snapshot = manager.get_snapshot("TASK-L")

    assert snapshot["stage_history"] == ["received", "compiled", "queued", "collecting"]
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python -m pytest tests/test_execution_observability.py::test_lifecycle_supports_compiled_and_collecting_stages -q`
Expected: FAIL because lifecycle enum lacks the new stages

- [ ] **Step 3: Add the new lifecycle stages**

```python
class ExecutionLifecycleStage(str, Enum):
    RECEIVED = "received"
    COMPILED = "compiled"
    QUEUED = "queued"
    PREPARING = "preparing"
    EXECUTING = "executing"
    COLLECTING = "collecting"
    REPORTING = "reporting"
    FINISHED = "finished"
```

- [ ] **Step 4: Update the production path to emit the new transitions**

```python
observability_manager.transition(message.taskNo, ExecutionLifecycleStage.COMPILED)
...
observability_manager.transition(plan.task_no, ExecutionLifecycleStage.COLLECTING)
```

- [ ] **Step 5: Run observability tests**

Run: `python -m pytest tests/test_execution_observability.py tests/test_task_dispatch_production.py -q`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add core/execution_observability.py tests/test_execution_observability.py main_production.py core/task_executor_production.py tests/test_task_dispatch_production.py
git commit -m "feat: align lifecycle stages with compiler and collection flow"
```

## Task 7: Remove Internal Compatibility Reads From Production Path

**Files:**
- Modify: `core/task_executor_production.py`
- Modify: `README.md`

- [ ] **Step 1: Write the failing regression test that guards against legacy task assumptions**

```python
def test_execute_plan_does_not_require_models_task_compatibility_properties(monkeypatch):
    executor = TaskExecutorProduction(message_sender=lambda _: None)
    plan = ExecutionPlan(
        task_no="TASK-P",
        project_no="PROJECT-P",
        task_name="Plan Only",
        device_id="DEVICE-P",
        tool_type="canoe",
        cases=[PlannedCase(case_no="CASE-P", case_name="Case P", case_type="test_module")],
    )

    monkeypatch.setattr(executor, "_task_queue", type("_Queue", (), {"put": lambda self, item: True})())
    assert executor.execute_plan(plan) is True
```

- [ ] **Step 2: Run test to verify it fails if any production path still expects `.task_id` / `.test_items` / `.tool_type` from legacy task objects**

Run: `python -m pytest tests/test_task_compiler.py tests/test_config_runtime_integration.py -q`
Expected: FAIL until the remaining plan path reads use `ExecutionPlan` fields directly

- [ ] **Step 3: Replace remaining legacy property access in the production path**

```python
plan.task_no
plan.tool_type
plan.config_path
plan.config_name
plan.base_config_dir
plan.variables
plan.cases
```

- [ ] **Step 4: Update README architecture notes**

```markdown
- WebSocket 入口消息先经 `TaskCompiler` 编译为内部 `ExecutionPlan`
- `TaskExecutorProduction` 只消费内部执行计划
- 外部 API/任务看板模型保持独立
```

- [ ] **Step 5: Run focused regression tests**

Run: `python -m pytest tests/test_task_compiler.py tests/test_config_runtime_integration.py tests/test_task_dispatch_production.py tests/test_execution_observability.py -q`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add core/task_executor_production.py README.md tests/test_task_compiler.py tests/test_config_runtime_integration.py tests/test_task_dispatch_production.py tests/test_execution_observability.py
git commit -m "refactor: remove legacy task compatibility from production path"
```

## Task 8: Full Verification

**Files:**
- Modify: none
- Test: `tests/test_execution_plan.py`
- Test: `tests/test_task_compiler.py`
- Test: `tests/test_task_dispatch_production.py`
- Test: `tests/test_execution_observability.py`
- Test: `tests/test_config_runtime_integration.py`

- [ ] **Step 1: Run the focused internal-model test suite**

Run: `python -m pytest tests/test_execution_plan.py tests/test_task_compiler.py tests/test_task_dispatch_production.py tests/test_execution_observability.py tests/test_config_runtime_integration.py -q`
Expected: PASS

- [ ] **Step 2: Run the full repository test suite**

Run: `python -m pytest -q`
Expected: PASS with existing intended skips only

- [ ] **Step 3: Review changed files before handoff**

Run: `git diff --stat HEAD~7..HEAD`
Expected: shows only the plan-model/compiler/executor/observability/test/doc changes from this phase

- [ ] **Step 4: Commit any final doc or naming cleanup**

```bash
git add .
git commit -m "chore: finalize internal task model unification"
```

## Self-Review

- Spec coverage:
  - Internal single execution model: covered in Tasks 1, 2, 4, 5, 7
  - Compiler boundary: covered in Tasks 2 and 3
  - Fixed lifecycle: covered in Task 6
  - No external API unification: respected by keeping queue/API changes out of scope
- Placeholder scan:
  - No `TODO`/`TBD` markers included
  - Every code-changing step includes concrete code or exact field usage
- Type consistency:
  - `ExecutionPlan`, `PlannedCase`, `TaskCompiler`, `TaskCompileError`, `ConfigSource`, and `execute_plan()` are used consistently across tasks
