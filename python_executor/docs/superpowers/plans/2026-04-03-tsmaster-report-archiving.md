# TSMaster 报告归档压缩实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在 TSMaster 任务执行完成后，将报告目录和测试数据目录压缩为 zip 包，上报结果时一并上传。

**Architecture:** 修改 `TSMasterAdapter.get_test_report_info()` 方法，新增：查找最新子文件夹、收集文件、创建压缩包、返回 `archive_path`。taskNo 通过 adapter 内部属性 `self._current_task_no` 获取，由 `start_test_execution()` 调用时存储。

**Tech Stack:** Python 标准库（`os`, `shutil`, `tempfile`, `zipfile`, `datetime`）

---

## 文件结构

- **修改:** `core/adapters/tsmaster_adapter.py` — `get_test_report_info()` 方法增加压缩逻辑
- **修改:** `core/adapters/tsmaster_adapter.py` — `start_test_execution()` 增加 task_no 参数和属性存储
- **修改:** `core/execution_strategies/tsmaster_strategy.py` — 调用 `start_execution` 时传入 taskNo

---

## Task 1: 添加 task_no 属性和参数存储

**Files:**
- Modify: `core/adapters/tsmaster_adapter.py`

- [ ] **Step 1: 找到 TSMasterAdapter 类的 `__init__` 方法**

读取 `core/adapters/tsmaster_adapter.py` 中 `TSMasterAdapter` 类的 `__init__` 方法（约在 70-150 行附近），在 `self.operation_timeout = operation_timeout` 后添加：

```python
self._current_task_no = None
```

- [ ] **Step 2: 修改 `start_test_execution()` 方法签名**

找到 `start_test_execution` 方法（约在 509 行），在 `test_cases: Optional[str] = None` 后添加 `task_no: Optional[str] = None` 参数：

```python
def start_test_execution(self, test_cases: Optional[str] = None,
                        task_no: Optional[str] = None,
                        wait_for_complete: bool = True,
                        timeout: Optional[int] = None) -> bool:
```

- [ ] **Step 3: 在方法开头添加 task_no 存储**

在 `start_test_execution` 方法体内，`if not self.is_connected:` 检查之前添加：

```python
if task_no:
    self._current_task_no = task_no
```

- [ ] **Step 4: 修改 capability 中的 lambda 绑定**

找到 `tsmaster_execution` capability 的 `start_execution` lambda（约在 113 行），改为：

```python
start_execution=lambda selected_cases, task_no=None: self.start_test_execution(
    test_cases=selected_cases,
    task_no=task_no,
    wait_for_complete=False,
    timeout=self.operation_timeout,
),
```

- [ ] **Step 5: 提交**

```bash
git add core/adapters/tsmaster_adapter.py
git commit -m "feat(adapter): add task_no tracking to TSMaster adapter"
```

---

## Task 2: 实现查找最新子文件夹方法

**Files:**
- Modify: `core/adapters/tsmaster_adapter.py`

- [ ] **Step 1: 在 `TSMasterAdapter` 类中添加辅助方法**

在 `get_test_report_info()` 方法之前（约 660 行）添加新方法：

```python
def _find_latest_subfolder(self, parent_path: str) -> Optional[str]:
    """
    查找目录下按修改时间最新的子文件夹

    Args:
        parent_path: 父目录路径

    Returns:
        最新子文件夹的完整路径，如果不存在则返回 None
    """
    if not os.path.exists(parent_path):
        self.logger.warning(f"目录不存在: {parent_path}")
        return None

    try:
        subfolders = [d for d in os.listdir(parent_path)
                      if os.path.isdir(os.path.join(parent_path, d))]
        if not subfolders:
            self.logger.warning(f"目录下没有子文件夹: {parent_path}")
            return None

        latest = max(subfolders,
                      key=lambda d: os.path.getmtime(os.path.join(parent_path, d)))
        latest_path = os.path.join(parent_path, latest)
        self.logger.info(f"找到最新子文件夹: {latest_path}")
        return latest_path
    except Exception as e:
        self.logger.error(f"查找最新子文件夹失败: {e}")
        return None
```

- [ ] **Step 2: 提交**

```bash
git add core/adapters/tsmaster_adapter.py
git commit -m "feat(adapter): add _find_latest_subfolder helper method"
```

---

## Task 3: 实现压缩逻辑

**Files:**
- Modify: `core/adapters/tsmaster_adapter.py`

- [ ] **Step 1: 读取 `get_test_report_info()` 方法的完整实现**

约在 662-709 行，确认所有 import 已存在（`os`, `shutil`, `tempfile`, `datetime` 在文件顶部已有）。

- [ ] **Step 2: 在 `get_test_report_info()` 方法末尾添加压缩逻辑**

在 `return result` 之前（约 705 行附近），在 `self.logger.info(...)` 之后添加：

```python
# 压缩报告和测试数据
archive_path = None
try:
    # 获取 taskNo
    task_no = self._current_task_no or "UNKNOWN"
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    archive_name = f"TSMaster_Report_{task_no}_{timestamp}"

    # 创建临时目录用于存放压缩源文件
    temp_dir = tempfile.mkdtemp(prefix="tsmaster_archive_")
    temp_extract_dir = os.path.join(temp_dir, "archive_content")
    os.makedirs(temp_extract_dir)

    # 查找最新子文件夹并复制内容
    report_latest = self._find_latest_subfolder(report_path) if report_path else None
    testdata_latest = self._find_latest_subfolder(testdata_path) if testdata_path else None

    if report_latest:
        dest_report = os.path.join(temp_extract_dir, "report")
        shutil.copytree(report_latest, dest_report)
        self.logger.info(f"已复制报告文件: {report_latest} -> {dest_report}")

    if testdata_latest:
        dest_testdata = os.path.join(temp_extract_dir, "testdata")
        shutil.copytree(testdata_latest, dest_testdata)
        self.logger.info(f"已复制测试数据: {testdata_latest} -> {dest_testdata}")

    # 检查是否有文件需要压缩
    if os.path.exists(temp_extract_dir) and os.listdir(temp_extract_dir):
        archive_base = os.path.join(temp_dir, archive_name)
        shutil.make_archive(archive_base, 'zip', temp_extract_dir)
        archive_path = archive_base + ".zip"
        self.logger.info(f"压缩包已创建: {archive_path}")
    else:
        self.logger.warning("没有找到可压缩的报告文件")

except Exception as e:
    self.logger.error(f"创建报告压缩包失败: {e}")
    archive_path = None
finally:
    # 清理临时目录
    if 'temp_dir' in dir() and os.path.exists(temp_dir):
        try:
            shutil.rmtree(temp_dir)
            self.logger.info("临时目录已清理")
        except Exception:
            pass
```

- [ ] **Step 3: 在返回值中添加 archive_path**

找到 `result = {...}` 字典（约 695 行），添加 `archive_path` 字段：

```python
result = {
    "report_path": report_path,
    "testdata_path": testdata_path,
    "archive_path": archive_path,  # 新增
    "passed": passed,
    "failed": failed,
    "total": passed + failed,
    "success_rate": (passed / (passed + failed) * 100) if (passed + failed) > 0 else 0
}
```

- [ ] **Step 4: 提交**

```bash
git add core/adapters/tsmaster_adapter.py
git commit -m "feat(adapter): add report archiving to get_test_report_info"
```

---

## Task 4: 修改 Strategy 层传入 taskNo

**Files:**
- Modify: `core/execution_strategies/tsmaster_strategy.py`

- [ ] **Step 1: 读取 `_start_execution` 方法**

约在 137 行，找到：

```python
def _start_execution(self, capability: Any, selected_cases: str) -> bool:
    return bool(capability.start_execution(selected_cases))
```

- [ ] **Step 2: 修改为传入 taskNo**

```python
def _start_execution(self, capability: Any, selected_cases: str, task_no: str = None) -> bool:
    return bool(capability.start_execution(selected_cases, task_no=task_no))
```

- [ ] **Step 3: 修改 `run()` 方法中调用 `_start_execution` 的地方**

找到 `run()` 方法中（约 399 行）调用 `_start_execution` 的地方，添加 `task_no` 参数：

```python
task_no = getattr(plan, "task_no", None) or getattr(plan, "taskNo", None)
if not self._start_execution(capability, selected_cases, task_no=task_no):
```

- [ ] **Step 4: 提交**

```bash
git add core/execution_strategies/tsmaster_strategy.py
git commit -m "feat(strategy): pass taskNo to TSMaster adapter start_execution"
```

---

## Task 5: 验证和测试

**Files:**
- Modify: `tests/test_tsmaster_execution_strategy.py`（如存在）
- Create: `tests/test_tsmaster_report_archiving.py`（可选）

- [ ] **Step 1: 运行现有测试确认无回归**

```bash
pytest tests/test_tsmaster_execution_strategy.py -v
```

- [ ] **Step 2: 检查语法错误**

```bash
python -m py_compile core/adapters/tsmaster_adapter.py
python -m py_compile core/execution_strategies/tsmaster_strategy.py
```

---

## 验证检查清单

- [ ] `start_test_execution` 接受 `task_no` 参数
- [ ] `self._current_task_no` 在 adapter 中被正确存储
- [ ] `_find_latest_subfolder` 正确查找最新子文件夹
- [ ] 压缩包命名为 `TSMaster_Report_{taskNo}_{timestamp}.zip`
- [ ] `get_test_report_info()` 返回值包含 `archive_path`
- [ ] 临时目录在压缩完成后被清理
- [ ] 错误情况下 `archive_path` 为 `None` 不影响原有统计信息
