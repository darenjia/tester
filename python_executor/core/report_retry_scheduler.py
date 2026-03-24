"""
报告重试调度器 - 后台定期重试失败的报告
"""
import threading
import time
from typing import Optional

from core.failed_report_manager import get_failed_report_manager, FailedReportManager
from utils.report_client import get_report_client, ReportClient
from utils.logger import get_logger

logger = get_logger("report_retry_scheduler")


class ReportRetryScheduler:
    """
    报告重试调度器

    后台线程定期检查并重试失败的报告
    参考 TaskScheduler 实现模式
    """

    def __init__(self, check_interval: float = None, batch_size: int = None,
                 config_manager=None):
        """
        初始化重试调度器

        Args:
            check_interval: 检查间隔（秒）
            batch_size: 每批处理数量
            config_manager: 配置管理器
        """
        self.config_manager = config_manager
        self._check_interval = check_interval
        self._batch_size = batch_size

        self._running = False
        self._scheduler_thread: Optional[threading.Thread] = None
        self._lock = threading.Lock()

        # 获取管理器实例
        self._report_manager = None
        self._report_client = None

    @property
    def check_interval(self) -> float:
        if self._check_interval is None:
            if self.config_manager:
                self._check_interval = self.config_manager.get('report_retry.check_interval', 60)
            else:
                self._check_interval = 60
        return self._check_interval

    @property
    def batch_size(self) -> int:
        if self._batch_size is None:
            if self.config_manager:
                self._batch_size = self.config_manager.get('report_retry.batch_size', 10)
            else:
                self._batch_size = 10
        return self._batch_size

    @property
    def report_manager(self) -> FailedReportManager:
        if self._report_manager is None:
            self._report_manager = get_failed_report_manager(self.config_manager)
        return self._report_manager

    @property
    def report_client(self) -> ReportClient:
        if self._report_client is None:
            self._report_client = get_report_client(self.config_manager)
        return self._report_client

    def start(self):
        """启动调度器"""
        with self._lock:
            if self._running:
                logger.warning("报告重试调度器已在运行")
                return
            self._running = True

        self._scheduler_thread = threading.Thread(
            target=self._scheduler_loop,
            daemon=True,
            name="ReportRetryScheduler"
        )
        self._scheduler_thread.start()
        logger.info(f"报告重试调度器已启动 (间隔={self.check_interval}秒)")

    def stop(self):
        """停止调度器"""
        with self._lock:
            self._running = False

        if self._scheduler_thread:
            self._scheduler_thread.join(timeout=5)

        logger.info("报告重试调度器已停止")

    def _scheduler_loop(self):
        """调度器主循环"""
        while self._running:
            try:
                self._process_pending_reports()
            except Exception as e:
                logger.error(f"处理待重试报告时出错: {e}")

            # 等待下次检查
            for _ in range(int(self.check_interval)):
                if not self._running:
                    break
                time.sleep(1)

    def _process_pending_reports(self):
        """处理待重试的报告"""
        pending = self.report_manager.get_pending_reports(limit=self.batch_size)

        if not pending:
            return

        logger.info(f"处理 {len(pending)} 条待重试报告")

        for report in pending:
            if not self._running:
                break
            try:
                self._retry_report(report)
            except Exception as e:
                logger.error(f"重试报告 {report.report_id} 时出错: {e}")

    def _retry_report(self, report):
        """
        重试单个报告

        Args:
            report: FailedReport 实例
        """
        # 更新状态为重试中
        self.report_manager.update_report_status(report.report_id, "retrying")

        # 尝试发送
        success, error = self._send_report(report)

        # 更新重试状态
        has_more_retries = self.report_manager.increment_retry(
            report.report_id,
            success,
            error
        )

        if success:
            logger.info(f"报告 {report.report_id} (task={report.task_no}) 上报成功")
        elif not has_more_retries:
            logger.error(f"报告 {report.report_id} (task={report.task_no}) 已达最大重试次数")
        else:
            logger.warning(f"报告 {report.report_id} (task={report.task_no}) 重试失败，将在稍后重试 (第{report.retry_count}次)")

    def _send_report(self, report) -> tuple:
        """
        发送报告到远程服务器

        Args:
            report: FailedReport 实例

        Returns:
            (success: bool, error_message: str or None)
        """
        try:
            response = self.report_client._make_request(
                method="POST",
                url=self.report_client._result_api_url,
                json=report.report_data
            )

            if response is not None:
                # 成功时重置连续失败计数
                self._reset_failure_counter()
                return True, None
            else:
                return False, "服务器无响应"

        except Exception as e:
            return False, str(e)

    def _reset_failure_counter(self):
        """重置连续失败计数器"""
        try:
            from core.report_callback_handler import get_callback_handler
            handler = get_callback_handler()
            handler.reset_consecutive_failures()
        except Exception:
            pass

    def retry_now(self, report_id: str) -> bool:
        """
        立即重试指定报告

        Args:
            report_id: 报告ID

        Returns:
            是否成功
        """
        report = self.report_manager.get_report(report_id)
        if not report:
            logger.warning(f"报告不存在: {report_id}")
            return False

        # 重置以便立即重试
        self.report_manager.reset_report_for_retry(report_id)

        # 执行重试
        self._retry_report(self.report_manager.get_report(report_id))

        return True

    def retry_all_pending(self) -> dict:
        """
        重试所有待重试的报告

        Returns:
            统计结果
        """
        stats = {"success": 0, "failed": 0, "total": 0}

        pending = self.report_manager.get_pending_reports(limit=1000)
        stats["total"] = len(pending)

        for report in pending:
            # 重置以便立即重试
            self.report_manager.reset_report_for_retry(report.report_id)
            refreshed_report = self.report_manager.get_report(report.report_id)

            # 更新状态为重试中
            self.report_manager.update_report_status(refreshed_report.report_id, "retrying")

            # 尝试发送
            success, error = self._send_report(refreshed_report)

            # 更新重试状态
            self.report_manager.increment_retry(refreshed_report.report_id, success, error)

            if success:
                stats["success"] += 1
            else:
                stats["failed"] += 1

        logger.info(f"批量重试完成: {stats}")
        return stats


# 单例实例
_report_retry_scheduler: Optional[ReportRetryScheduler] = None


def get_report_retry_scheduler(config_manager=None) -> ReportRetryScheduler:
    """获取报告重试调度器单例"""
    global _report_retry_scheduler
    if _report_retry_scheduler is None:
        _report_retry_scheduler = ReportRetryScheduler(config_manager=config_manager)
    return _report_retry_scheduler