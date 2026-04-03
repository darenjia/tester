# TSMaster 报告归档压缩设计

## 背景

TSMaster 任务执行完成后，`get_test_report_info()` 返回的 `report_path` 和 `testdata_path` 是目录路径，其中包含多个文件（报告可能按时间戳放在最新子文件夹中）。上报结果时需要将报告和测试数据打包成 zip 压缩包后上传。

## 目标

修改 `TSMasterAdapter.get_test_report_info()` 方法，在返回报告信息时同步完成：
1. 查找报告目录和测试数据目录下的最新子文件夹
2. 收集所有文件到临时目录
3. 压缩为 `TSMaster_Report_{taskNo}_{timestamp}.zip`
4. 在返回结果中增加 `archive_path` 字段

## 实现方案

### 修改位置
- 文件：`core/adapters/tsmaster_adapter.py`
- 方法：`get_test_report_info()`

### 数据流

```
get_test_report_info()
  │
  ├── 获取 report_path, testdata_path（现有逻辑）
  │
  ├── 查找最新子文件夹
  │     ├─ os.listdir(report_path) 获取子文件夹列表
  │     ├─ os.path.getmtime() 获取修改时间
  │     └─ max(subfolders, key=os.path.getmtime)
  │
  ├── 收集文件到临时目录
  │     ├─ 创建临时目录
  │     ├─ shutil.copytree() 复制报告最新子文件夹内容
  │     └─ shutil.copytree() 复制测试数据最新子文件夹内容
  │
  ├── 创建压缩包
  │     ├─ 格式：TSMaster_Report_{taskNo}_{timestamp}.zip
  │     ├─ 位置：系统临时目录或配置的 report_archive_dir
  │     └─ shutil.make_archive() 或 zipfile
  │
  └── 返回 result
        ├─ report_path
        ├─ testdata_path
        ├─ archive_path  ← 新增
        ├─ passed
        ├─ failed
        └─ total
```

### taskNo 获取方式
- 在 `start_execution()` 被调用时，strategy 层传入 plan 对象
- adapter 内部通过 plan 获取 `task_no` 或 `taskNo`，存储在 `self._current_task_no`
- 如果未获取到，使用 "UNKNOWN" 作为默认值

### 压缩包命名
- 格式：`TSMaster_Report_{taskNo}_{timestamp}.zip`
- timestamp 格式：`YYYYMMDD_HHMMSS`
- 示例：`TSMaster_Report_TASK-001_20260403_143052.zip`

### 错误处理

| 场景 | 处理方式 |
|------|----------|
| 未找到子文件夹 | 记录警告，`archive_path` 返回 `None`，不影响统计信息 |
| 复制文件失败 | 记录错误，清理临时目录，`archive_path` 返回 `None` |
| 压缩失败 | 记录错误，清理临时目录，`archive_path` 返回 `None` |
| taskNo 未知 | 使用 "UNKNOWN"，压缩包名为 `TSMaster_Report_UNKNOWN_{timestamp}.zip` |

### 清理策略
- 临时目录：使用 `tempfile.mkdtemp()` 创建，执行完毕后清理
- 压缩包：保留在临时目录中，由调用方使用后清理
- 使用 `try/finally` 确保临时目录被清理

## 返回值结构

```python
{
    "report_path": str,           # 原始报告目录
    "testdata_path": str,         # 原始测试数据目录
    "archive_path": str,           # 压缩包路径（新增）
    "passed": int,
    "failed": int,
    "total": int,
    "success_rate": float
}
```

## 调用方影响

- strategy 层 (`tsmaster_strategy.py`) 无需修改
- `report_client.report_result()` 接收到的 `report_info` 将包含 `archive_path`
- 调用方可直接使用 `archive_path` 调用 `upload_report_file()`

## 依赖

- Python 标准库：`os`, `shutil`, `tempfile`, `zipfile`, `datetime`
- 无新增第三方依赖
