# 上报失败持久化修复计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 修复任务执行失败时上报数据未持久化的问题，以及 `failed_reports.json` 数据格式不匹配的问题

**Architecture:** 修改 `TaskExecutorProduction._report_to_remote()` 在 HTTP 上报失败时调用 `FailedReportManager.add_failed_report()` 持久化数据；同时修复数据格式不匹配问题

**Tech Stack:** Python, FailedReportManager, ReportClient, TaskExecutorProduction

---

## 问题分析

1. `TaskExecutorProduction._report_to_remote()` 直接调用 HTTP 请求，失败时不持久化
2. `failed_reports.json` 存储的是 `tasks` 格式，但 `FailedReportManager._load()` 期望 `reports` 格式
3. `tasks.json` 和 `failed_reports.json` 使用了相同的存储路径/机制导致混淆

---

## Task 1: 修复 TaskExecutorProduction._report_to_remote() 上报失败时持久化

**Files:**
- Modify: `core/task_executor_production.py:1110-1199`

- [ ] **Step 1: 修改 `_report_to_remote()` 方法，在 HTTP 上报失败时调用 `FailedReportManager.add_failed_report()`**

找到 `_report_to_remote()` 方法（第1110-1156行），在 `else` 分支（上报失败）添加持久化逻辑：

```python
if success:
    logger.info(f"[_report_to_remote] 任务结果上报成功: {task.task_id}")
else:
    logger.warning(f"[_report_to_remote] 任务结果上报失败: {task.task_id}")
    # 添加：持久化失败报告以便重试
    self._persist_failed_report(report_data, task)
```

然后在 `_report_to_remote()` 方法末尾添加新方法 `_persist_failed_report()`：

```python
def _persist_failed_report(self, report_data: Dict[str, Any], task: Task):
    """
    持久化失败的上报数据

    Args:
        report_data: 上报数据
        task: 任务对象
    """
    try:
        from core.failed_report_manager import get_failed_report_manager

        manager = get_failed_report_manager(self.config_manager)

        task_info = {
            'taskNo': task.task_id,
            'projectNo': getattr(task, 'projectNo', ''),
            'deviceId': getattr(task, 'deviceId', None),
            'toolType': getattr(task, 'toolType', None),
            'taskName': getattr(task, 'taskName', '') or getattr(task, 'name', '')
        }

        # 计算优先级（失败任务优先级更高）
        priority = 3 if report_data.get('status') in ['failed', 'FAILED'] else 0

        report_id = manager.add_failed_report(
            report_data=report_data,
            task_info=task_info,
            max_retries=self.config_manager.get('report_retry.max_retries', 10) if self.config_manager else 10,
            priority=priority
        )

        logger.info(f"失败报告已持久化: report_id={report_id}, task_no={task.task_id}")

    except Exception as e:
        logger.error(f"持久化失败报告时出错: {e}")
```

- [ ] **Step 2: 验证修改**

运行应用，执行一个会失败的任务，检查日志中是否出现 "失败报告已持久化" 字样

- [ ] **Step 3: 提交**

```bash
git add core/task_executor_production.py
git commit -m "fix(report): 上报失败时持久化到FailedReportManager"
```

---

## Task 2: 修复 failed_reports.json 数据格式问题

**Files:**
- Modify: `core/failed_report_manager.py:72-97`

- [ ] **Step 1: 修改 `_load()` 方法，同时兼容 `tasks` 和 `reports` 格式**

找到 `_load()` 方法（第72-97行），修改加载逻辑以兼容旧数据：

```python
def _load(self):
    """从文件加载数据"""
    try:
        if os.path.exists(self.storage_path):
            with open(self.storage_path, 'r', encoding='utf-8') as f:
                data = json.load(f)

            self.reports.clear()

            # 兼容两种格式：reports (新) 或 tasks (旧/混淆格式)
            report_data = data.get('reports', {})
            if not report_data:
                # 尝试从 tasks 格式加载（旧数据）
                report_data = data.get('tasks', {})

            for report_id, report_item in report_data.items():
                # 如果是 tasks 格式的任务数据，转换为 FailedReport 格式
                if 'status' in report_item and 'error_message' in report_item and 'task_type' in report_item:
                    # 这是 ExecutorTask 格式，需要转换
                    failed_report = self._convert_task_to_failed_report(report_id, report_item)
                    if failed_report:
                        self.reports[report_id] = failed_report
                else:
                    # 这是 FailedReport 格式
                    self.reports[report_id] = FailedReport.from_dict(report_item)

            self.retry_history.clear()
            for record_data in data.get('retry_history', [])[-1000:]:
                self.retry_history.append(RetryHistoryRecord.from_dict(record_data))

            # 从文件加载历史统计（仅保留非实时统计字段）
            self.statistics = {
                'last_cleanup': data.get('statistics', {}).get('last_cleanup')
            }

            logger.info(f"加载了 {len(self.reports)} 条失败报告记录")
        else:
            self._init_empty_storage()
    except Exception as e:
        logger.error(f"加载失败报告数据失败: {e}")
        self._init_empty_storage()
```

- [ ] **Step 2: 添加 `_convert_task_to_failed_report()` 辅助方法**

在 `_load()` 方法后添加：

```python
def _convert_task_to_failed_report(self, report_id: str, task_data: Dict[str, Any]) -> Optional[FailedReport]:
    """
    将 ExecutorTask 格式转换为 FailedReport 格式

    Args:
        report_id: 报告ID
        task_data: 任务数据

    Returns:
        FailedReport 实例，转换失败返回 None
    """
    try:
        task_no = task_data.get('id', report_id)
        error_message = task_data.get('error_message', '')

        failed_report = FailedReport(
            report_id=report_id,
            task_no=task_no,
            report_data={
                'taskNo': task_no,
                'status': task_data.get('status', 'failed'),
                'errorMessage': error_message,
                'metadata': task_data.get('metadata', {})
            },
            task_info={
                'taskNo': task_no,
                'projectNo': task_data.get('metadata', {}).get('projectNo', ''),
                'deviceId': task_data.get('metadata', {}).get('deviceId', ''),
                'caseCount': task_data.get('metadata', {}).get('caseCount', 0)
            },
            status=ReportStatus.PENDING.value,
            retry_count=0,
            max_retries=task_data.get('max_retries', 3),
            next_retry_time=calculate_next_retry_time(
                0,
                base_delay=self._get_config('report_retry.base_delay', 30.0),
                backoff_factor=self._get_config('report_retry.backoff_factor', 2.0),
                max_delay=self._get_config('report_retry.max_delay', 3600.0)
            ),
            last_error=error_message,
            created_at=task_data.get('created_at', datetime.now().strftime("%Y-%m-%d %H:%M:%S")),
            updated_at=task_data.get('completed_at', datetime.now().strftime("%Y-%m-%d %H:%M:%S"))
        )

        return failed_report
    except Exception as e:
        logger.error(f"转换任务数据失败: report_id={report_id}, error={e}")
        return None
```

- [ ] **Step 3: 清理 failed_reports.json 中的旧数据**

删除 `failed_reports.json` 文件中格式错误的数据（可选，因为下次加载会自动修复）

```bash
# 不需要手动操作，下次启动时会自动转换
```

- [ ] **Step 4: 提交**

```bash
git add core/failed_report_manager.py
git commit -m "fix(data): 兼容tasks格式并自动转换为FailedReport"
```

---

## Task 3: 验证整体修复

- [ ] **Step 1: 重启应用，执行一个会失败的任务**

观察日志：
- "失败报告已持久化" 表示持久化成功
- "加载了 X 条失败报告记录" 表示加载成功

- [ ] **Step 2: 检查 failed_reports.json 内容**

```bash
cat data/failed_reports.json
```

确认 `reports` 字段有数据，且格式为 `FailedReport`

- [ ] **Step 3: 提交**

```bash
git add -A
git commit -m "chore: 上报失败持久化完整修复"
```

---

## 验证清单

- [ ] 任务执行失败后，日志显示 "失败报告已持久化"
- [ ] `failed_reports.json` 中 `reports` 字段有数据
- [ ] 上报列表页面能正确显示失败报告
