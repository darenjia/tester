"""
失败报告管理器 - 管理上报失败的测试结果持久化和重试
"""
import os
import sys
import json
import hashlib
import uuid
import threading
import copy
from typing import Dict, Any, List, Optional, Callable
from datetime import datetime, timedelta

from models.failed_report import (
    FailedReport, ReportAttempt, ReportStatus, RetryHistoryRecord,
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
            report_data = {
                'taskNo': task_no,
                'status': task_data.get('status', 'failed'),
                'errorMessage': error_message,
                'metadata': task_data.get('metadata', {})
            }
            metadata = self._build_report_metadata(
                report_data=report_data,
                task_info={
                    'taskNo': task_no,
                    'projectNo': task_data.get('metadata', {}).get('projectNo', ''),
                    'deviceId': task_data.get('metadata', {}).get('deviceId', ''),
                    'taskName': task_data.get('metadata', {}).get('taskName', ''),
                    'toolType': task_data.get('metadata', {}).get('toolType', '')
                },
                endpoint=task_data.get('metadata', {}).get('endpoint'),
                failure_reason=error_message,
                source='legacy_task_conversion'
            )
            payload_hash = metadata.get('payload_hash')
            initial_attempt = self._build_attempt(
                attempt_number=1,
                endpoint=metadata.get('endpoint'),
                payload_hash=payload_hash,
                success=False,
                status=ReportStatus.FAILED.value,
                error_message=error_message,
                source='legacy_task_conversion'
            )

            failed_report = FailedReport(
                report_id=report_id,
                task_no=task_no,
                report_data=report_data,
                task_info={
                    'taskNo': task_no,
                    'projectNo': task_data.get('metadata', {}).get('projectNo', ''),
                    'deviceId': task_data.get('metadata', {}).get('deviceId', ''),
                    'caseCount': task_data.get('metadata', {}).get('caseCount', 0)
                },
                metadata=metadata,
                attempts=[initial_attempt],
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
            # 先在锁外准备数据，避免长时间持有锁
            with self._data_lock:
                data = {
                    "reports": {rid: r.to_dict() for rid, r in self.reports.items()},
                    "retry_history": [h.to_dict() for h in self.retry_history[-1000:]],
                    "statistics": self.statistics.copy(),
                    "version": "1.0",
                    "last_updated": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
                }

            # 释放锁后再写入文件
            with open(self.storage_path, 'w', encoding='utf-8') as f:
                json.dump(data, f, ensure_ascii=False, indent=2)

        except Exception as e:
            logger.error(f"保存失败报告数据失败: {e}")

    def _get_config(self, key: str, default: Any = None) -> Any:
        """获取配置项"""
        if self.config_manager:
            return self.config_manager.get(key, default)
        return default

    def _canonical_payload(self, payload: Dict[str, Any]) -> str:
        """将上报内容规范化为稳定字符串，便于持久化 hash。"""
        return json.dumps(payload, ensure_ascii=False, sort_keys=True, separators=(",", ":"), default=str)

    def _calculate_payload_hash(self, payload: Dict[str, Any]) -> str:
        """计算 payload 的 SHA256 摘要。"""
        canonical_payload = self._canonical_payload(payload)
        return hashlib.sha256(canonical_payload.encode("utf-8")).hexdigest()

    def _build_report_metadata(
        self,
        report_data: Dict[str, Any],
        task_info: Dict[str, Any] = None,
        endpoint: str = None,
        failure_reason: str = None,
        source: str = None,
        max_retries: int = None,
        priority: int = None,
    ) -> Dict[str, Any]:
        """为失败报告构造结构化元数据。"""
        task_info = task_info or {}
        payload_hash = self._calculate_payload_hash(report_data)
        payload_size = len(self._canonical_payload(report_data).encode("utf-8"))

        task_no = report_data.get("taskNo") or task_info.get("taskNo") or ""
        project_no = task_info.get("projectNo") or report_data.get("projectNo") or ""
        device_id = task_info.get("deviceId") or report_data.get("deviceId") or ""
        task_name = task_info.get("taskName") or report_data.get("taskName") or ""
        tool_type = task_info.get("toolType") or report_data.get("toolType") or ""
        resolved_endpoint = endpoint or task_info.get("endpoint") or report_data.get("endpoint")
        trace_id = (
            task_info.get("trace_id")
            or task_info.get("traceId")
            or report_data.get("trace_id")
            or report_data.get("traceId")
        )
        attempt_id = (
            task_info.get("attempt_id")
            or task_info.get("attemptId")
            or report_data.get("attempt_id")
            or report_data.get("attemptId")
        )
        error_category = (
            task_info.get("error_category")
            or task_info.get("errorCategory")
            or report_data.get("error_category")
            or report_data.get("errorCategory")
        )
        report_error_category = (
            task_info.get("report_error_category")
            or task_info.get("reportErrorCategory")
            or report_data.get("report_error_category")
            or report_data.get("reportErrorCategory")
        )

        metadata = {
            "taskNo": task_no,
            "task_no": task_no,
            "projectNo": project_no,
            "project_no": project_no,
            "deviceId": device_id,
            "device_id": device_id,
            "taskName": task_name,
            "task_name": task_name,
            "toolType": tool_type,
            "tool_type": tool_type,
            "endpoint": resolved_endpoint,
            "payload_hash": payload_hash,
            "payload_size": payload_size,
            "failure_reason": failure_reason or report_data.get("errorMessage") or task_info.get("errorMessage"),
            "source": source or "failed_report_manager",
            "max_retries": max_retries,
            "priority": priority,
            "report_status": report_data.get("status"),
            "trace_id": trace_id,
            "traceId": trace_id,
            "attempt_id": attempt_id,
            "attemptId": attempt_id,
            "error_category": error_category,
            "errorCategory": error_category,
            "report_error_category": report_error_category,
            "reportErrorCategory": report_error_category,
        }
        metadata.update(task_info)
        return metadata

    def _build_attempt(
        self,
        attempt_number: int,
        endpoint: str = None,
        payload_hash: str = None,
        attempt_id: str = None,
        trace_id: str = None,
        error_category: str = None,
        success: bool = False,
        status: str = None,
        error_message: str = None,
        request_method: str = "POST",
        source: str = None,
    ) -> ReportAttempt:
        """构造结构化的上报尝试记录。"""
        attempted_at = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        return ReportAttempt(
            attempt_id=attempt_id or str(uuid.uuid4()),
            attempt_number=attempt_number,
            attempted_at=attempted_at,
            endpoint=endpoint,
            payload_hash=payload_hash,
            trace_id=trace_id,
            error_category=error_category,
            success=success,
            status=status or (ReportStatus.SUCCESS.value if success else ReportStatus.FAILED.value),
            error_message=error_message,
            request_method=request_method,
            source=source,
        )

    def add_failed_report(self, report_data: Dict[str, Any], task_info: Dict[str, Any] = None,
                          max_retries: int = None, priority: int = 0,
                          failure_reason: str = None, endpoint: str = None,
                          metadata: Dict[str, Any] = None) -> str:
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
        normalized_metadata = self._build_report_metadata(
            report_data=report_data,
            task_info=task_info or {},
            endpoint=endpoint,
            failure_reason=failure_reason,
            source="report_client" if metadata and metadata.get("report_source") == "report_client" else "failed_report_manager",
            max_retries=max_retries,
            priority=priority,
        )
        if metadata:
            normalized_metadata.update(metadata)
        normalized_metadata.setdefault("report_source", normalized_metadata.get("source", "failed_report_manager"))
        normalized_metadata.setdefault("endpoint", endpoint or (task_info or {}).get("endpoint") or report_data.get("endpoint"))
        normalized_metadata.setdefault("payload_hash", self._calculate_payload_hash(report_data))

        initial_attempt = self._build_attempt(
            attempt_number=1,
            endpoint=normalized_metadata.get("endpoint"),
            payload_hash=normalized_metadata.get("payload_hash"),
            attempt_id=normalized_metadata.get("attempt_id"),
            trace_id=normalized_metadata.get("trace_id"),
            error_category=normalized_metadata.get("error_category"),
            success=False,
            status=ReportStatus.FAILED.value,
            error_message=normalized_metadata.get("failure_reason"),
            source=normalized_metadata.get("report_source"),
        )

        failed_report = FailedReport(
            report_id=report_id,
            task_no=task_no,
            report_data=report_data,
            task_info=task_info or {},
            metadata=normalized_metadata,
            attempts=[initial_attempt],
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

    def list_report_projections(self, status: str = None, limit: int = 100, offset: int = 0) -> List[Dict[str, Any]]:
        """列出报告的查询投影。"""
        return [report.to_projection() for report in self.list_reports(status=status, limit=limit, offset=offset)]

    def get_report_projection(self, report_id: str) -> Optional[Dict[str, Any]]:
        """获取单个报告的查询投影。"""
        report = self.get_report(report_id)
        return report.to_projection() if report else None

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
        should_trigger_callback = False
        has_more_retries = False

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

            attempt = self._build_attempt(
                attempt_number=len(report.attempts) + 1,
                endpoint=report.metadata.get("endpoint") or report.metadata.get("report_endpoint"),
                payload_hash=report.metadata.get("payload_hash"),
                attempt_id=report.metadata.get("attempt_id"),
                trace_id=report.metadata.get("trace_id"),
                error_category=report.metadata.get("error_category"),
                success=success,
                status=ReportStatus.SUCCESS.value if success else ReportStatus.FAILED.value,
                error_message=error,
                source=report.metadata.get("report_source") or report.metadata.get("source"),
            )
            report.record_attempt(attempt)
            report.metadata["last_error"] = error
            report.metadata["retry_count"] = report.retry_count
            report.metadata["last_retry_time"] = report.last_retry_time
            report.metadata["latest_attempt"] = attempt.to_dict()

            if success:
                report.status = ReportStatus.SUCCESS.value
                has_more_retries = False
            elif report.retry_count >= report.max_retries:
                report.status = ReportStatus.FAILED.value
                should_trigger_callback = True
            else:
                report.status = ReportStatus.PENDING.value
                report.next_retry_time = calculate_next_retry_time(
                    report.retry_count,
                    base_delay=self._get_config('report_retry.base_delay', 30.0),
                    backoff_factor=self._get_config('report_retry.backoff_factor', 2.0),
                    max_delay=self._get_config('report_retry.max_delay', 3600.0)
                )
                has_more_retries = True

        # 在锁外面保存数据，避免死锁
        self._save()

        # 触发失败回调（在锁外面）
        if should_trigger_callback:
            with self._data_lock:
                if report_id in self.reports:
                    self._trigger_failure_callbacks(self.reports[report_id])

        return has_more_retries

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
            report.metadata["retry_count"] = report.retry_count
            report.metadata["last_retry_reset_at"] = report.updated_at

        self._save()
        logger.info(f"重置报告状态以便重试: {report_id}")
        return True

    def get_trace_context_summary(self, limit: int = 20) -> Dict[str, Any]:
        """获取失败报告中的最近观测上下文摘要。"""
        with self._data_lock:
            reports = sorted(self.reports.values(), key=lambda report: report.updated_at, reverse=True)

        recent = []
        seen_trace_ids = []
        for report in reports[:limit]:
            metadata = report.metadata or {}
            latest_attempt = report.latest_attempt()
            trace_id = metadata.get("trace_id") or metadata.get("traceId")
            if not trace_id and latest_attempt:
                trace_id = latest_attempt.trace_id
            attempt_id = metadata.get("attempt_id") or metadata.get("attemptId")
            if not attempt_id and latest_attempt:
                attempt_id = latest_attempt.attempt_id
            error_category = metadata.get("error_category") or metadata.get("errorCategory")
            if not error_category and latest_attempt:
                error_category = latest_attempt.error_category
            report_error_category = metadata.get("report_error_category") or metadata.get("reportErrorCategory")

            if trace_id:
                seen_trace_ids.append(trace_id)

            recent.append(
                {
                    "report_id": report.report_id,
                    "task_no": report.task_no,
                    "trace_id": trace_id,
                    "attempt_id": attempt_id,
                    "error_category": error_category,
                    "report_error_category": report_error_category,
                    "status": report.status,
                    "updated_at": report.updated_at,
                }
            )

        return {
            "recent": recent,
            "recent_trace_ids": seen_trace_ids[:limit],
            "total_reports": len(self.reports),
        }


# 单例实例
_failed_report_manager: Optional[FailedReportManager] = None


def get_failed_report_manager(config_manager=None) -> FailedReportManager:
    """获取失败报告管理器单例"""
    global _failed_report_manager
    if _failed_report_manager is None:
        _failed_report_manager = FailedReportManager(config_manager=config_manager)
    return _failed_report_manager
