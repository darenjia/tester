"""
历史记录管理
"""
import os
import json
import threading
from datetime import datetime
from typing import Dict, Any, List, Optional
from dataclasses import dataclass, field, asdict

from .models import CheckSession


@dataclass
class CheckHistoryItem:
    """历史记录项"""
    id: str
    mode: str
    started_at: str
    completed_at: str
    duration_ms: int
    summary: Dict[str, Any]
    results_count: int

    def to_dict(self) -> Dict[str, Any]:
        return asdict(self)


class CheckHistory:
    """
    历史记录管理器

    负责保存和加载检测历史记录
    """

    def __init__(self, history_dir: str = None, max_history: int = 100):
        self._history: List[CheckSession] = []
        self._max_history = max_history
        self._lock = threading.Lock()

        if history_dir:
            self._history_file = os.path.join(history_dir, "check_history.json")
            self._load_from_file()
        else:
            self._history_file = None

    def _load_from_file(self):
        """从文件加载历史记录"""
        if not self._history_file or not os.path.exists(self._history_file):
            return

        try:
            with open(self._history_file, 'r', encoding='utf-8') as f:
                data = json.load(f)
                self._history = [CheckSession.from_dict(s) for s in data]
        except Exception as e:
            print(f"加载历史记录失败: {e}")

    def _save_to_file(self):
        """保存历史记录到文件"""
        if not self._history_file:
            return

        try:
            os.makedirs(os.path.dirname(self._history_file), exist_ok=True)
            with open(self._history_file, 'w', encoding='utf-8') as f:
                json.dump([s.to_dict() for s in self._history], f, ensure_ascii=False, indent=2)
        except Exception as e:
            print(f"保存历史记录失败: {e}")

    def add_session(self, session: CheckSession):
        """添加检测会话到历史"""
        with self._lock:
            self._history.insert(0, session)
            if len(self._history) > self._max_history:
                self._history = self._history[:self._max_history]
            self._save_to_file()

    def get_history(self, limit: int = 20, mode: str = None) -> List[Dict[str, Any]]:
        """
        获取历史记录

        Args:
            limit: 最大数量
            mode: 筛选模式 (quick/detailed/None表示全部)

        Returns:
            历史记录列表
        """
        with self._lock:
            history = self._history
            if mode:
                history = [s for s in history if s.mode == mode]
            return [self._session_to_item(s).to_dict() for s in history[:limit]]

    def get_session(self, session_id: str) -> Optional[CheckSession]:
        """获取指定会话详情"""
        with self._lock:
            for session in self._history:
                if session.id == session_id:
                    return session
        return None

    def delete_session(self, session_id: str) -> bool:
        """删除指定会话"""
        with self._lock:
            for i, session in enumerate(self._history):
                if session.id == session_id:
                    self._history.pop(i)
                    self._save_to_file()
                    return True
        return False

    def clear_history(self):
        """清空历史记录"""
        with self._lock:
            self._history = []
            self._save_to_file()

    def get_stats(self) -> Dict[str, Any]:
        """获取统计信息"""
        with self._lock:
            total = len(self._history)
            if total == 0:
                return {
                    "total_sessions": 0,
                    "quick_sessions": 0,
                    "detailed_sessions": 0,
                    "total_passed": 0,
                    "total_failed": 0,
                    "average_pass_rate": 0
                }

            quick_count = len([s for s in self._history if s.mode == "quick"])
            detailed_count = len([s for s in self._history if s.mode == "detailed"])

            total_passed = sum(s.summary.get("passed", 0) for s in self._history)
            total_failed = sum(s.summary.get("failed", 0) for s in self._history)
            avg_pass_rate = sum(s.summary.get("pass_rate", 0) for s in self._history) / total

            return {
                "total_sessions": total,
                "quick_sessions": quick_count,
                "detailed_sessions": detailed_count,
                "total_passed": total_passed,
                "total_failed": total_failed,
                "average_pass_rate": round(avg_pass_rate, 1)
            }

    def _session_to_item(self, session: CheckSession) -> CheckHistoryItem:
        """转换会话为历史记录项"""
        return CheckHistoryItem(
            id=session.id,
            mode=session.mode,
            started_at=session.started_at,
            completed_at=session.completed_at,
            duration_ms=session.duration_ms,
            summary=session.summary,
            results_count=len(session.results)
        )