"""
失败报告管理器 - 管理上报失败的测试结果持久化和重试
"""
import os
import sys
import json
import uuid
import threading
import copy
from typing import Dict, Any, List, Optional, Callable
from datetime import datetime, timedelta

from models.failed_report import (
    FailedReport, ReportStatus, RetryHistoryRecord,
    calculate_next_retry_time, get_delay_for_attempt
)
from utils.logger import get_logger

logger = get_logger("failed_report_manager")


class FailedReportManager:
    """
    失败报告管理器

    单例模式 - 管理上报失败结果的持久化存储和重试
    参考 CaseMappingManager 实现模式
    """

    _instance = None
    _lock = threading.Lock()

    def __new__(cls, *args, **kwargs):
        if cls._instance is None:
            with cls._lock:
                if cls._instance is None:
                    cls._instance = super().__new__(cls)
        return cls._instance

    def __init__(self, storage_path: str = None, config_manager=None):
        if hasattr(self, '_initialized'):
            return
        self._initialized = True

        # 确定存储路径
        if storage_path is None:
            if getattr(sys, 'frozen', False):
                base_dir = os.path.dirname(sys.executable)
            else:
                base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
            storage_path = os.path.join(base_dir, 'data', 'failed_reports.json')

        self.storage_path = storage_path
        self.config_manager = config_manager
        self.reports: Dict[str, FailedReport] = {}
        self.retry_history: List[RetryHistoryRecord] = []
        self.statistics: Dict[str, Any] = {}
        self._data_lock = threading.Lock()

        # 失败回调列表
        self._failure_callbacks: List[Callable] = []

        self._ensure_storage_dir()
        self._load()

    def _ensure_storage_dir(self):
        """确保存储目录存在"""
        storage_dir = os.path.dirname(self.storage_path)
        if storage_dir and not os.path.exists(storage_dir):
            os.makedirs(storage_dir, exist_ok=True)

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

    def _init_empty_storage(self):
        """初始化空存储"""
        self.reports.clear()
        self.retry_history.clear()
        self.statistics = {}
        self._save()
        logger.info("初始化空失败报告存储")

    def _save(self):
        """保存数据到文件"""
        try:
            with self._data_lock:
                data = {
                    "reports": {rid: r.to_dict() for rid, r in self.reports.items()},
                    "retry_history": [h.to_dict() for h in self.retry_history[-1000:]],
                    "statistics": self.statistics,
                    "version": "1.0",
                    "last_updated": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
                }

                with open(self.storage_path, 'w', encoding='utf-8') as f:
                    json.dump(data, f, ensure_ascii=False, indent=2)

        except Exception as e:
            logger.error(f"保存失败报告数据失败: {e}")

    def _get_config(self, key: str, default: Any = None) -> Any:
        """获取配置项"""
        if self.config_manager:
            return self.config_manager.get(key, default)
        return default

    def add_failed_report(self, report_data: Dict[str, Any], task_info: Dict[str, Any] = None,
                          max_retries: int = None, priority: int = 0) -> str:
        """
        添加失败报告

        Args:
            report_data: 完整的上报数据 (TDM2.0格式)
            task_info: 额外的任务信息
            max_retries: 最大重试次数
            priority: 优先级 (越高越紧急)

        Returns:
            report_id: 报告唯一标识
        """
        if max_retries is None:
            max_retries = self._get_config('report_retry.max_retries', 10)

        report_id = str(uuid.uuid4())
        task_no = report_data.get('taskNo', 'unknown')

        failed_report = FailedReport(
            report_id=report_id,
            task_no=task_no,
            report_data=report_data,
            task_info=task_info or {},
            status=ReportStatus.PENDING.value,
            retry_count=0,
            max_retries=max_retries,
            priority=priority,
            next_retry_time=calculate_next_retry_time(
                0,
                base_delay=self._get_config('report_retry.base_delay', 30.0),
                backoff_factor=self._get_config('report_retry.backoff_factor', 2.0),
                max_delay=self._get_config('report_retry.max_delay', 3600.0)
            )
        )

        with self._data_lock:
            self.reports[report_id] = failed_report

        self._save()
        logger.info(f"添加失败报告: {report_id}, task_no={task_no}")

        return report_id

    def get_report(self, report_id: str) -> Optional[FailedReport]:
        """获取单个报告（返回副本）"""
        report = self.reports.get(report_id)
        return copy.deepcopy(report) if report else None

    def get_pending_reports(self, limit: int = None) -> List[FailedReport]:
        """
        获取待重试的报告（返回副本）

        Args:
            limit: 返回数量限制

        Returns:
            待重试的报告列表
        """
        if limit is None:
            limit = self._get_config('report_retry.batch_size', 10)

        current_time = datetime.now()
        pending = []

        with self._data_lock:
            for report in self.reports.values():
                if report.status == ReportStatus.PENDING.value:
                    if report.next_retry_time:
                        try:
                            next_time = datetime.strptime(report.next_retry_time, "%Y-%m-%d %H:%M:%S")
                            if current_time >= next_time:
                                pending.append(copy.deepcopy(report))
                        except ValueError:
                            pending.append(copy.deepcopy(report))
                    else:
                        pending.append(copy.deepcopy(report))

                if len(pending) >= limit:
                    break

        # 按优先级排序（高优先级在前）
        pending.sort(key=lambda x: x.priority, reverse=True)
        return pending

    def get_reports_by_status(self, status: str, limit: int = 100) -> List[FailedReport]:
        """按状态获取报告（返回副本）"""
        results = []
        with self._data_lock:
            for report in self.reports.values():
                if report.status == status:
                    results.append(copy.deepcopy(report))
                if len(results) >= limit:
                    break
        return results

    def list_reports(self, status: str = None, limit: int = 100,
                     offset: int = 0) -> List[FailedReport]:
        """
        列出报告（返回副本）

        Args:
            status: 状态过滤
            limit: 返回数量
            offset: 偏移量

        Returns:
            报告列表
        """
        with self._data_lock:
            reports_list = [copy.deepcopy(r) for r in self.reports.values()]

        # 按创建时间倒序
        reports_list.sort(key=lambda x: x.created_at, reverse=True)

        if status:
            reports_list = [r for r in reports_list if r.status == status]

        return reports_list[offset:offset + limit]

    def update_report_status(self, report_id: str, status: str, error: str = None):
        """更新报告状态"""
        with self._data_lock:
            if report_id not in self.reports:
                return

            report = self.reports[report_id]
            report.status = status
            report.last_error = error
            report.updated_at = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

        self._save()

    def increment_retry(self, report_id: str, success: bool, error: str = None) -> bool:
        """
        增加重试计数

        Args:
            report_id: 报告ID
            success: 本次是否成功
            error: 错误信息

        Returns:
            是否还有重试机会
        """
        with self._data_lock:
            if report_id not in self.reports:
                return False

            report = self.reports[report_id]
            report.retry_count += 1
            report.last_retry_time = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            report.last_error = error
            report.updated_at = report.last_retry_time

            # 记录重试历史
            delay_used = get_delay_for_attempt(
                report.retry_count - 1,
                base_delay=self._get_config('report_retry.base_delay', 30.0),
                backoff_factor=self._get_config('report_retry.backoff_factor', 2.0),
                max_delay=self._get_config('report_retry.max_delay', 3600.0)
            )

            self.retry_history.append(RetryHistoryRecord(
                report_id=report_id,
                attempt_number=report.retry_count,
                attempt_time=report.last_retry_time,
                delay_used=delay_used,
                success=success,
                error_message=error
            ))

            if success:
                report.status = ReportStatus.SUCCESS.value
                self._save()
                return False
            elif report.retry_count >= report.max_retries:
                report.status = ReportStatus.FAILED.value
                self._save()
                # 触发失败回调
                self._trigger_failure_callbacks(report)
                return False
            else:
                report.status = ReportStatus.PENDING.value
                report.next_retry_time = calculate_next_retry_time(
                    report.retry_count,
                    base_delay=self._get_config('report_retry.base_delay', 30.0),
                    backoff_factor=self._get_config('report_retry.backoff_factor', 2.0),
                    max_delay=self._get_config('report_retry.max_delay', 3600.0)
                )
                self._save()
                return True

    def delete_report(self, report_id: str) -> bool:
        """删除报告"""
        with self._data_lock:
            if report_id not in self.reports:
                return False

            del self.reports[report_id]

        self._save()
        logger.info(f"删除报告: {report_id}")
        return True

    def cleanup_old_reports(self, days: int = None) -> int:
        """
        清理旧报告

        Args:
            days: 保留天数

        Returns:
            清理数量
        """
        if days is None:
            days = self._get_config('report_retry.cleanup_days', 7)

        cutoff = datetime.now() - timedelta(days=days)

        with self._data_lock:
            to_remove = []
            for report_id, report in self.reports.items():
                if report.status in [ReportStatus.SUCCESS.value, ReportStatus.FAILED.value]:
                    try:
                        updated = datetime.strptime(report.updated_at, "%Y-%m-%d %H:%M:%S")
                        if updated < cutoff:
                            to_remove.append(report_id)
                    except ValueError:
                        pass

            for report_id in to_remove:
                del self.reports[report_id]

            if to_remove:
                self.statistics['last_cleanup'] = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
                self._save()
                logger.info(f"清理了 {len(to_remove)} 条旧报告")

        return len(to_remove)

    def register_failure_callback(self, callback: Callable):
        """注册失败回调"""
        self._failure_callbacks.append(callback)

    def _trigger_failure_callbacks(self, report: FailedReport):
        """触发失败回调"""
        for callback in self._failure_callbacks:
            try:
                callback(report)
            except Exception as e:
                logger.error(f"失败回调执行错误: {e}")

    def get_statistics(self) -> Dict[str, Any]:
        """获取统计信息"""
        with self._data_lock:
            pending = sum(1 for r in self.reports.values() if r.status == ReportStatus.PENDING.value)
            failed = sum(1 for r in self.reports.values() if r.status == ReportStatus.FAILED.value)
            success = sum(1 for r in self.reports.values() if r.status == ReportStatus.SUCCESS.value)
            retrying = sum(1 for r in self.reports.values() if r.status == ReportStatus.RETRYING.value)

            return {
                "total": len(self.reports),
                "pending": pending,
                "failed": failed,
                "success": success,
                "retrying": retrying,
                "history_count": len(self.retry_history)
            }

    def reset_report_for_retry(self, report_id: str) -> bool:
        """
        重置报告状态以便立即重试

        Args:
            report_id: 报告ID

        Returns:
            是否成功
        """
        with self._data_lock:
            if report_id not in self.reports:
                return False

            report = self.reports[report_id]
            if report.status not in [ReportStatus.FAILED.value, ReportStatus.PENDING.value]:
                return False

            # 重置状态
            report.status = ReportStatus.PENDING.value
            report.retry_count = 0
            report.next_retry_time = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            report.updated_at = report.next_retry_time

        self._save()
        logger.info(f"重置报告状态以便重试: {report_id}")
        return True


# 单例实例
_failed_report_manager: Optional[FailedReportManager] = None


def get_failed_report_manager(config_manager=None) -> FailedReportManager:
    """获取失败报告管理器单例"""
    global _failed_report_manager
    if _failed_report_manager is None:
        _failed_report_manager = FailedReportManager(config_manager=config_manager)
    return _failed_report_manager