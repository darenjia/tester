"""
报告回调处理器 - 处理上报永久失败的通知
"""
import logging
from typing import Dict, Any, Optional, List, Callable
from dataclasses import dataclass, field
from datetime import datetime

from models.failed_report import FailedReport
from utils.logger import get_logger

logger = get_logger("report_callback_handler")


@dataclass
class CallbackConfig:
    """回调配置"""
    callback_type: str              # "webhook", "log", "custom"
    enabled: bool = True
    threshold: int = 3              # 连续失败阈值
    cooldown_minutes: int = 60      # 冷却时间（分钟）
    config: Dict[str, Any] = field(default_factory=dict)


class ReportCallbackHandler:
    """
    报告回调处理器

    当报告达到最大重试次数后触发通知
    """

    def __init__(self, config_manager=None):
        self.config_manager = config_manager
        self._callbacks: Dict[str, CallbackConfig] = {}
        self._consecutive_failures: int = 0
        self._last_callback_time: Optional[datetime] = None
        self._custom_handlers: Dict[str, Callable] = {}

        # 注册默认回调
        self._register_default_callbacks()

    def _register_default_callbacks(self):
        """注册默认回调类型"""
        # 日志回调 - 总是启用
        self._callbacks["log"] = CallbackConfig(
            callback_type="log",
            enabled=True,
            threshold=1
        )

        # 从配置加载webhook回调
        if self.config_manager:
            webhook_enabled = self.config_manager.get('report_callback.webhook.enabled', False)
            webhook_url = self.config_manager.get('report_callback.webhook.url', '')
            threshold = self.config_manager.get('report_callback.threshold', 3)
            cooldown = self.config_manager.get('report_callback.cooldown_minutes', 60)

            if webhook_enabled and webhook_url:
                self._callbacks["webhook"] = CallbackConfig(
                    callback_type="webhook",
                    enabled=True,
                    threshold=threshold,
                    cooldown_minutes=cooldown,
                    config={"url": webhook_url}
                )

    def register_callback(self, name: str, config: CallbackConfig):
        """注册回调配置"""
        self._callbacks[name] = config
        logger.info(f"注册回调: {name}")

    def register_custom_handler(self, name: str, handler: Callable):
        """注册自定义处理函数"""
        self._custom_handlers[name] = handler

    def handle_failure(self, report: FailedReport):
        """
        处理永久失败

        Args:
            report: 失败的报告
        """
        self._consecutive_failures += 1

        logger.error(
            f"报告永久失败: task_no={report.task_no}, "
            f"report_id={report.report_id}, retries={report.retry_count}, "
            f"last_error={report.last_error}"
        )

        # 触发启用的回调
        for name, config in self._callbacks.items():
            if not config.enabled:
                continue

            if self._consecutive_failures < config.threshold:
                continue

            if self._should_cooldown(config):
                continue

            try:
                self._execute_callback(name, config, report)
            except Exception as e:
                logger.error(f"回调 {name} 执行失败: {e}")

    def _should_cooldown(self, config: CallbackConfig) -> bool:
        """检查是否在冷却期"""
        if not self._last_callback_time:
            return False

        elapsed = (datetime.now() - self._last_callback_time).total_seconds()
        return elapsed < config.cooldown_minutes * 60

    def _execute_callback(self, name: str, config: CallbackConfig, report: FailedReport):
        """执行回调"""
        if config.callback_type == "log":
            self._log_callback(report)
        elif config.callback_type == "webhook":
            self._webhook_callback(config, report)
        elif config.callback_type == "custom":
            handler = self._custom_handlers.get(name)
            if handler:
                handler(report)

        self._last_callback_time = datetime.now()

    def _log_callback(self, report: FailedReport):
        """日志回调"""
        logger.critical(
            f"[警报] 上报失败 - 任务: {report.task_no}, "
            f"连续失败次数: {self._consecutive_failures}, "
            f"错误: {report.last_error}"
        )

    def _webhook_callback(self, config: CallbackConfig, report: FailedReport):
        """Webhook回调"""
        try:
            import requests

            url = config.config.get("url")
            if not url:
                logger.warning("Webhook URL未配置")
                return

            payload = {
                "event": "report_failure",
                "task_no": report.task_no,
                "report_id": report.report_id,
                "retry_count": report.retry_count,
                "last_error": report.last_error,
                "consecutive_failures": self._consecutive_failures,
                "timestamp": datetime.now().isoformat()
            }

            response = requests.post(
                url,
                json=payload,
                timeout=10,
                headers={"Content-Type": "application/json"}
            )

            if response.status_code == 200:
                logger.info(f"Webhook回调成功: {url}")
            else:
                logger.warning(f"Webhook回调失败: {response.status_code}")

        except ImportError:
            logger.error("requests库未安装，无法发送webhook")
        except Exception as e:
            logger.error(f"Webhook回调异常: {e}")

    def reset_consecutive_failures(self):
        """重置连续失败计数器"""
        self._consecutive_failures = 0

    def get_status(self) -> Dict[str, Any]:
        """获取状态信息"""
        return {
            "consecutive_failures": self._consecutive_failures,
            "last_callback_time": self._last_callback_time.isoformat() if self._last_callback_time else None,
            "callbacks": {name: {"type": cfg.callback_type, "enabled": cfg.enabled}
                         for name, cfg in self._callbacks.items()}
        }


# 单例实例
_callback_handler: Optional[ReportCallbackHandler] = None


def get_callback_handler(config_manager=None) -> ReportCallbackHandler:
    """获取回调处理器单例"""
    global _callback_handler
    if _callback_handler is None:
        _callback_handler = ReportCallbackHandler(config_manager=config_manager)
    return _callback_handler