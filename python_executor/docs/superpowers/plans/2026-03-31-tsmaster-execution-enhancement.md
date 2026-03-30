# TSMaster Test Execution Enhancement Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add proper TSMaster RPC test execution flow (Master form startup, Init, SelectCases, Controller) to TSMasterAdapter, and integrate with TaskExecutorProduction.

**Architecture:** Add `wait_for_test_complete()` and `get_test_report_info()` methods to TSMasterAdapter, then update TaskExecutorProduction to use TSMaster-specific execution path. Follows the RPC flow from the reference example (`examples/tsmaster_test_framework`).

**Tech Stack:** Python, TSMaster RPC API, TSMasterAdapter, TaskExecutorProduction

---

## File Structure

- `core/adapters/tsmaster_adapter.py` - Add `wait_for_test_complete()` and `get_test_report_info()` methods
- `core/task_executor_production.py` - Add `_start_test_execution()` method for TSMaster

---

## Task 1: Add wait_for_test_complete() to TSMasterAdapter

**Files:**
- Modify: `core/adapters/tsmaster_adapter.py:527-557` (after `_wait_for_test_complete` private method)

- [ ] **Step 1: Add public wait_for_test_complete() method**

Location: After line 556 (after `_wait_for_test_complete` method). Add at approximately line 558.

```python
def wait_for_test_complete(self, timeout: Optional[int] = None) -> bool:
    """
    Wait for test to complete by polling is_test_finished()

    Args:
        timeout: Timeout in seconds. Defaults to operation_timeout.

    Returns:
        True if test finished, False if timeout
    """
    if not self.is_connected:
        self._set_error("TSMaster未连接，无法等待测试完成")
        return False

    timeout = timeout or self.operation_timeout
    start_time = time.time()

    self.logger.info(f"等待测试完成（超时: {timeout}秒）...")

    while time.time() - start_time < timeout:
        try:
            if self._using_rpc and self._rpc_client:
                if self._rpc_client.read_system_var("TestSystem.Controller") == "0":
                    self.logger.info("测试执行完成")
                    return True
            elif self._ts:
                controller = self._read_system_var("TestSystem.Controller")
                if controller == "0":
                    self.logger.info("测试执行完成")
                    return True
        except Exception as e:
            self.logger.debug(f"检查测试状态异常: {str(e)}")

        time.sleep(0.5)

    self.logger.warning(f"等待测试完成超时（{timeout}秒）")
    return False
```

- [ ] **Step 2: Commit**

```bash
git add core/adapters/tsmaster_adapter.py
git commit -m "feat(tsmaster): add wait_for_test_complete method"
```

---

## Task 2: Add get_test_report_info() to TSMasterAdapter

**Files:**
- Modify: `core/adapters/tsmaster_adapter.py` (after `get_test_results()` method around line 620)

- [ ] **Step 1: Add get_test_report_info() method after get_test_results()**

Location: After line 621 (after `get_test_results()` method). Add at approximately line 622.

```python
def get_test_report_info(self) -> Optional[Dict[str, Any]]:
    """
    Get test report paths and statistics after execution

    Returns:
        Dict with report_path, testdata_path, passed, failed, total
        Returns None if not connected or error
    """
    if not self.is_connected:
        self._set_error("TSMaster未连接，无法获取测试报告信息")
        return None

    try:
        self.logger.info("获取测试报告信息")

        if self._using_rpc and self._rpc_client:
            report_path = self._rpc_client.get_report_path()
            testdata_path = self._rpc_client.get_testdata_path()
            passed, failed = self._rpc_client.get_test_case_count()
        else:
            report_path = self._read_system_var("Master.ReportPath")
            testdata_path = self._read_system_var("Master.TestDataLogPath")
            passed_str = self._read_system_var("TestSystem.TestCasePassCount") or "0"
            failed_str = self._read_system_var("TestSystem.TestCaseFailCount") or "0"
            passed, failed = int(passed_str), int(failed_str)

        self._test_stats = {"passed": passed, "failed": failed, "total": passed + failed}

        result = {
            "report_path": report_path,
            "testdata_path": testdata_path,
            "passed": passed,
            "failed": failed,
            "total": passed + failed,
            "success_rate": (passed / (passed + failed) * 100) if (passed + failed) > 0 else 0
        }

        self.logger.info(f"测试报告信息: passed={passed}, failed={failed}, total={passed+failed}")
        return result

    except Exception as e:
        self._set_error(f"获取测试报告信息失败: {str(e)}")
        return None
```

- [ ] **Step 2: Commit**

```bash
git add core/adapters/tsmaster_adapter.py
git commit -m "feat(tsmaster): add get_test_report_info method"
```

---

## Task 3: Add _start_test_execution() to TaskExecutorProduction for TSMaster

**Files:**
- Modify: `core/task_executor_production.py:506-526` (after `_start_measurement`)

- [ ] **Step 1: Add _start_test_execution() method after _start_measurement()**

Location: After line 526 (after `_start_measurement` method). Add at approximately line 527.

```python
def _start_test_execution(self, task: Task):
    """
    Start TSMaster test execution with full RPC flow.

    Follows the reference example flow:
    1. Build test case selection string from task.test_items
    2. Call adapter.start_test_execution(test_cases, wait_for_complete=False)
    3. Wait for test completion with adapter.wait_for_test_complete()

    Args:
        task: Task object with test_items containing case info
    """
    tool_type_lower = task.tool_type.lower() if task.tool_type else ""

    if tool_type_lower != TestToolType.TSMASTER.value:
        # Fall back to regular start_measurement for non-TSMaster
        self._start_measurement(task)
        return

    self.logger.info("使用TSMaster测试执行流程")

    try:
        # Build test case selection string from test_items
        test_cases = self._build_tsmaster_test_cases_string(task)

        if test_cases:
            self.logger.info(f"执行TSMaster测试用例: {test_cases}")
            self.current_collector.add_log("INFO", f"选择测试用例: {test_cases}")

            # Call adapter's start_test_execution with test cases
            success = self.controller.start_test_execution(
                test_cases=test_cases,
                wait_for_complete=False,  # We wait separately
                timeout=task.timeout
            )

            if not success:
                raise ToolException("TSMaster测试执行启动失败")

            # Wait for test to complete
            self.logger.info("等待TSMaster测试执行完成...")
            if not self.controller.wait_for_test_complete(timeout=task.timeout):
                self.logger.warning("TSMaster测试执行超时")
        else:
            self.logger.info("无测试用例，启动仿真")
            self._start_measurement(task)

    except Exception as e:
        raise ToolException(f"TSMaster测试执行失败: {e}")
```

- [ ] **Step 2: Add helper method _build_tsmaster_test_cases_string()**

Location: After the new `_start_test_execution()` method. Add at approximately line 575.

```python
def _build_tsmaster_test_cases_string(self, task: Task) -> str:
    """
    Build TSMaster test case selection string from task.test_items.

    Format: "TG1_TC1=1,TG1_TC2=1" or similar

    Args:
        task: Task object

    Returns:
        Test case selection string, empty string if no valid cases
    """
    if not task.test_items:
        return ""

    cases = []
    for item in task.test_items:
        # Try to get case identifier from various sources
        case_id = (
            getattr(item, 'caseNo', None) or
            getattr(item, 'case_no', None) or
            getattr(item, 'name', None)
        )
        if case_id:
            # Format as "case_id=1" for selection
            cases.append(f"{case_id}=1")

    return ",".join(cases) if cases else ""
```

- [ ] **Step 3: Commit**

```bash
git add core/task_executor_production.py
git commit -m "feat(tsmaster): add _start_test_execution for TSMaster test flow"
```

---

## Task 4: Update _execute_task_production to use new TSMaster execution

**Files:**
- Modify: `core/task_executor_production.py` around line 364 (after start_measurement call)

- [ ] **Step 1: Replace start_measurement call with _start_test_execution for TSMaster**

Find the code around line 364:
```python
# 启动测量/仿真
step_start = time.time()
self._start_measurement(task)
```

Replace with:
```python
# 启动测量/仿真（对于TSMaster使用完整的测试执行流程）
step_start = time.time()
tool_type_lower = task.tool_type.lower() if task.tool_type else ""
if tool_type_lower == TestToolType.TSMASTER.value:
    self._start_test_execution(task)
else:
    self._start_measurement(task)
```

- [ ] **Step 2: After test execution completes, collect TSMaster report info**

Find after line 370 (after `_execute_test_items` call), before `_stop_measurement`:

Add:
```python
# 对于TSMaster，获取测试报告信息
tool_type_lower = task.tool_type.lower() if task.tool_type else ""
if tool_type_lower == TestToolType.TSMASTER.value:
    try:
        report_info = self.controller.get_test_report_info()
        if report_info:
            self.current_collector.add_log("INFO", f"测试报告路径: {report_info.get('report_path', 'N/A')}")
            self.current_collector.add_log("INFO", f"测试数据路径: {report_info.get('testdata_path', 'N/A')}")
            self.current_collector.add_log("INFO", f"通过: {report_info.get('passed', 0)}, 失败: {report_info.get('failed', 0)}")
    except Exception as e:
        self.logger.warning(f"获取TSMaster报告信息失败: {e}")
```

- [ ] **Step 3: Commit**

```bash
git add core/task_executor_production.py
git commit -m "feat(tsmaster): integrate TSMaster test execution flow into task executor"
```

---

## Task 5: Verify implementation

**Files:**
- Modify: `tests/manual/check_tsmaster_rpc.py` (existing test file)

- [ ] **Step 1: Check if test file exists and add verification test if needed**

```bash
ls tests/manual/check_tsmaster_rpc.py
```

If exists, verify the implementation by reviewing the code flow.

- [ ] **Step 2: Commit any test changes**

```bash
git add tests/ 2>/dev/null; git commit -m "test(tsmaster): verify implementation"
```

---

## Self-Review Checklist

- [ ] Spec coverage: All requirements from design spec are covered
- [ ] Placeholder scan: No TBD/TODO in steps
- [ ] Type consistency: Method names match between tasks
- [ ] Task 1: `wait_for_test_complete()` added to adapter
- [ ] Task 2: `get_test_report_info()` added to adapter
- [ ] Task 3: `_start_test_execution()` and `_build_tsmaster_test_cases_string()` added to executor
- [ ] Task 4: `_execute_task_production` updated to use TSMaster flow
- [ ] All methods use correct signatures from design