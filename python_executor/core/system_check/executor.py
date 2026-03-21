"""
检测执行器
统一执行检测任务
"""
import time
import uuid
import threading
from typing import List, Dict, Any, Optional, Callable
from datetime import datetime

from .models import CheckResult, CheckSession
from .registry import check_registry
from .history import CheckHistory


class CheckExecutor:
    """
    检测执行器

    负责执行检测任务并管理执行状态
    """

    def __init__(self, history: CheckHistory = None):
        self._history = history or CheckHistory()
        self._lock = threading.Lock()
        self._running = False
        self._current_session: Optional[CheckSession] = None
        self._progress = 0
        self._current_check = ""
        self._callbacks: List[Callable] = []

    @property
    def is_running(self) -> bool:
        """是否正在执行"""
        with self._lock:
            return self._running

    @property
    def progress(self) -> int:
        """获取当前进度"""
        with self._lock:
            return self._progress

    @property
    def current_check(self) -> str:
        """获取当前检测项"""
        with self._lock:
            return self._current_check

    def add_callback(self, callback: Callable):
        """添加进度回调"""
        self._callbacks.append(callback)

    def _update_progress(self, progress: int, current: str):
        """更新进度"""
        with self._lock:
            self._progress = progress
            self._current_check = current

        for callback in self._callbacks:
            try:
                callback(progress, current)
            except Exception as e:
                print(f"回调执行失败: {e}")

    def execute_quick(self) -> CheckSession:
        """
        执行快速检测

        Returns:
            CheckSession: 检测会话
        """
        checks = check_registry.get_quick_checks()
        return self._execute_checks(checks, "quick")

    def execute_all(self) -> CheckSession:
        """
        执行所有检测

        Returns:
            CheckSession: 检测会话
        """
        checks = check_registry.get_all_checks()
        return self._execute_checks(checks, "detailed")

    def execute_category(self, category: str) -> CheckSession:
        """
        执行指定类别的检测

        Args:
            category: 类别ID

        Returns:
            CheckSession: 检测会话
        """
        checks = check_registry.get_checks_by_category(category)
        return self._execute_checks(checks, "detailed")

    def execute_checks(self, check_ids: List[str]) -> CheckSession:
        """
        执行指定的检测项

        Args:
            check_ids: 检测项ID列表

        Returns:
            CheckSession: 检测会话
        """
        checks = [check_registry.get_check(cid) for cid in check_ids]
        checks = [c for c in checks if c is not None]
        return self._execute_checks(checks, "detailed")

    def _execute_checks(self, checks: List, mode: str) -> CheckSession:
        """
        执行检测项列表

        Args:
            checks: 检测项列表
            mode: 检测模式

        Returns:
            CheckSession: 检测会话
        """
        with self._lock:
            if self._running:
                raise RuntimeError("检测正在执行中")
            self._running = True
            self._progress = 0
            self._current_check = ""

        session = CheckSession(
            id=str(uuid.uuid4()),
            mode=mode,
            started_at=datetime.now().isoformat()
        )

        start_time = time.time()
        total = len(checks)

        try:
            for i, check in enumerate(checks):
                self._update_progress(
                    int((i / total) * 100) if total > 0 else 0,
                    check.name
                )

                result = check.run()
                session.results.append(result)

            self._update_progress(100, "检测完成")

        finally:
            session.completed_at = datetime.now().isoformat()
            session.duration_ms = int((time.time() - start_time) * 1000)

            with self._lock:
                self._running = False
                self._current_session = session

            # 保存到历史
            self._history.add_session(session)

        return session

    def get_status(self) -> Dict[str, Any]:
        """获取执行状态"""
        with self._lock:
            return {
                "running": self._running,
                "progress": self._progress,
                "current_check": self._current_check,
                "session_id": self._current_session.id if self._current_session else None
            }


# 全局执行器实例
_executor: Optional[CheckExecutor] = None


def get_executor() -> CheckExecutor:
    """获取全局执行器实例"""
    global _executor
    if _executor is None:
        _executor = CheckExecutor()
    return _executor