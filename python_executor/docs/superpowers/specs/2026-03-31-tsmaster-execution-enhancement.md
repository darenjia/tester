# TSMaster Test Execution Enhancement Design

Date: 2026-03-31

## Problem Statement

The current TSMaster implementation has two issues:

1. **Adapter-level**: `TSMasterAdapter.start_test()` only starts simulation, not the actual test execution. The reference example (`examples/tsmaster_test_framework`) shows the full RPC flow:
   - `start_simulation()`
   - `run_form("C 代码编辑器 [Master]")`
   - `write_var("Master.Init", "1")`
   - `write_var("Master.AutoReport", "1")`
   - `write_var("TestSystem.SelectCases", cases)`
   - `write_var("TestSystem.Controller", "1")` (start test)
   - Poll `is_test_finished()`
   - `get_report_path()`

2. **Integration-level**: `TaskExecutorProduction` uses generic `start_simulation()`/`stop_simulation()` for TSMaster, not the test-specific execution methods.

## Design

### 1. TSMasterAdapter Enhancement

Add new methods to `core/adapters/tsmaster_adapter.py`:

#### `start_test_execution(test_cases, wait_for_complete, timeout)`
Execute test with full flow:
1. `start_simulation()`
2. Run Master form (if `auto_start_master` enabled)
3. `write_system_var("Master.Init", "1")`
4. `write_system_var("Master.AutoReport", "1")`
5. `write_system_var("TestSystem.SelectCases", test_cases)`
6. `write_system_var("TestSystem.Controller", "1")` (start test)
7. If `wait_for_complete=True`, poll until test finishes or timeout

Parameters:
- `test_cases`: str - Test case selection string, e.g., "TG1_TC1=1,TG1_TC2=1"
- `wait_for_complete`: bool - Whether to block until test completes
- `timeout`: int - Timeout in seconds

Returns: `bool` - True if test started successfully

#### `wait_for_test_complete(timeout) -> bool`
Poll `is_test_finished()` until True or timeout.

Parameters:
- `timeout`: int - Timeout in seconds

Returns: `bool` - True if test finished, False if timeout

#### `get_test_report_info() -> Dict[str, Any]`
Get report paths and test statistics after execution.

Returns:
```python
{
    "report_path": str,      # Path to test report
    "testdata_path": str,    # Path to test data logs
    "passed": int,           # Passed test count
    "failed": int,           # Failed test count
    "total": int             # Total test count
}
```

### 2. TaskExecutorProduction Enhancement

Add/enhance methods in `core/task_executor_production.py`:

#### `_start_test_execution(task)`
New method that calls `adapter.start_test_execution()` with proper test cases from task.

For TSMaster tool type:
- Extract test cases from `task.test_items` and format as selection string
- Call `adapter.start_test_execution(test_cases, wait_for_complete=True, timeout=task.timeout)`
- Wait for completion using `adapter.wait_for_test_complete()`

#### `_get_test_report_info()`
Call `adapter.get_test_report_info()` to collect report paths and statistics.

### 3. Test Case Selection String Format

Build test case selection string from task items:
- Format: "TG1_TC1=1,TG1_TC2=1" or similar
- Source: `task.test_items[i].name` or `task.test_items[i].caseNo`

### 4. Files to Modify

1. `core/adapters/tsmaster_adapter.py`:
   - Add `start_test_execution()` method
   - Add `wait_for_test_complete()` method
   - Add `get_test_report_info()` method

2. `core/task_executor_production.py`:
   - Add `_start_test_execution()` method
   - Modify `_execute_task_production()` to use new TSMaster-specific methods

### 5. Error Handling

- If any step in `start_test_execution()` fails, return False and log error
- If `wait_for_test_complete()` times out, log warning and return False
- If `get_test_report_info()` fails, return partial info with error flag

## Backward Compatibility

- Existing `start_test()` / `stop_test()` methods continue to work for simple simulation control
- New methods are additive only
- TaskExecutorProduction falls back to existing behavior if new methods unavailable