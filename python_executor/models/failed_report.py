"""
失败报告模型定义
用于存储上报失败的测试结果，支持重试机制
"""
import uuid
import random
import threading
from dataclasses import dataclass, field
from typing import Dict, Any, Optional, List
from datetime import datetime, timedelta
from enum import Enum


class ReportStatus(Enum):
    """报告状态枚举"""
    PENDING = "pending"         # 等待重试
    RETRYING = "retrying"       # 正在重试
    FAILED = "failed"           # 已达最大重试次数
    SUCCESS = "success"         # 上报成功


@dataclass
class FailedReport:
    """
    失败报告模型

    存储上报失败的测试结果，用于后续重试
    """
    # 主键
    report_id: str                       # 唯一ID (UUID)

    # 原始上报数据
    task_no: str                         # 任务编号
    report_data: Dict[str, Any]          # 完整的上报数据 (TDM2.0格式)
    task_info: Dict[str, Any] = field(default_factory=dict)  # 额外的任务信息

    # 重试追踪
    status: str = ReportStatus.PENDING.value
    retry_count: int = 0                 # 已重试次数
    max_retries: int = 10                # 最大重试次数
    next_retry_time: Optional[str] = None  # 下次重试时间
    last_retry_time: Optional[str] = None  # 上次重试时间
    last_error: Optional[str] = None     # 最后的错误信息

    # 时间戳
    created_at: str = ""                 # 创建时间
    updated_at: str = ""                 # 更新时间

    # 优先级 (越高越紧急)
    priority: int = 0

    def __post_init__(self):
        if not self.created_at:
            self.created_at = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        if not self.updated_at:
            self.updated_at = self.created_at

    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        return {
            "report_id": self.report_id,
            "task_no": self.task_no,
            "report_data": self.report_data,
            "task_info": self.task_info,
            "status": self.status,
            "retry_count": self.retry_count,
            "max_retries": self.max_retries,
            "next_retry_time": self.next_retry_time,
            "last_retry_time": self.last_retry_time,
            "last_error": self.last_error,
            "created_at": self.created_at,
            "updated_at": self.updated_at,
            "priority": self.priority
        }

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'FailedReport':
        """从字典创建"""
        return cls(
            report_id=data.get("report_id", ""),
            task_no=data.get("task_no", ""),
            report_data=data.get("report_data", {}),
            task_info=data.get("task_info", {}),
            status=data.get("status", ReportStatus.PENDING.value),
            retry_count=data.get("retry_count", 0),
            max_retries=data.get("max_retries", 10),
            next_retry_time=data.get("next_retry_time"),
            last_retry_time=data.get("last_retry_time"),
            last_error=data.get("last_error"),
            created_at=data.get("created_at", ""),
            updated_at=data.get("updated_at", ""),
            priority=data.get("priority", 0)
        )


@dataclass
class RetryHistoryRecord:
    """重试历史记录"""
    report_id: str
    attempt_number: int
    attempt_time: str
    delay_used: float                   # 本次重试前的延迟时间
    success: bool
    error_message: Optional[str] = None

    def to_dict(self) -> Dict[str, Any]:
        return {
            "report_id": self.report_id,
            "attempt_number": self.attempt_number,
            "attempt_time": self.attempt_time,
            "delay_used": self.delay_used,
            "success": self.success,
            "error_message": self.error_message
        }

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'RetryHistoryRecord':
        return cls(
            report_id=data.get("report_id", ""),
            attempt_number=data.get("attempt_number", 0),
            attempt_time=data.get("attempt_time", ""),
            delay_used=data.get("delay_used", 0.0),
            success=data.get("success", False),
            error_message=data.get("error_message")
        )


def calculate_next_retry_time(attempt: int, base_delay: float = 30.0,
                               backoff_factor: float = 2.0,
                               max_delay: float = 3600.0,
                               jitter_percent: float = 10.0) -> str:
    """
    计算下次重试时间（指数退避 + 抖动）

    Args:
        attempt: 当前尝试次数（从0开始）
        base_delay: 基础延迟（秒）
        backoff_factor: 退避因子
        max_delay: 最大延迟（秒）
        jitter_percent: 抖动百分比

    Returns:
        下次重试时间字符串
    """
    # 指数退避计算延迟
    delay = min(base_delay * (backoff_factor ** attempt), max_delay)

    # 添加随机抖动（避免同时重试）
    jitter = delay * (jitter_percent / 100.0) * (2 * random.random() - 1)
    delay = max(1, delay + jitter)

    next_time = datetime.now() + timedelta(seconds=delay)
    return next_time.strftime("%Y-%m-%d %H:%M:%S")


def get_delay_for_attempt(attempt: int, base_delay: float = 30.0,
                          backoff_factor: float = 2.0,
                          max_delay: float = 3600.0) -> float:
    """
    获取指定尝试次数的延迟时间

    Args:
        attempt: 尝试次数（从0开始）
        base_delay: 基础延迟
        backoff_factor: 退避因子
        max_delay: 最大延迟

    Returns:
        延迟时间（秒）
    """
    return min(base_delay * (backoff_factor ** attempt), max_delay)