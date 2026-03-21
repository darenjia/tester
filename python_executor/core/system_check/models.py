"""
统一检测数据模型
"""
from dataclasses import dataclass, field, asdict
from datetime import datetime
from typing import Dict, Any, List, Literal, Optional
from enum import Enum


class CheckStatus(Enum):
    """检测状态枚举"""
    PENDING = "pending"
    RUNNING = "running"
    PASSED = "passed"
    FAILED = "failed"
    WARNING = "warning"
    SKIPPED = "skipped"


@dataclass
class CheckResult:
    """单项检测结果"""
    check_id: str
    name: str
    category: str
    category_name: str
    status: str  # passed/failed/warning/skipped
    message: str
    details: Dict[str, Any] = field(default_factory=dict)
    suggestions: List[str] = field(default_factory=list)
    duration_ms: int = 0
    timestamp: str = ""
    quick_check: bool = True  # 是否包含在快速检测中

    def __post_init__(self):
        if not self.timestamp:
            self.timestamp = datetime.now().isoformat()

    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        return asdict(self)

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'CheckResult':
        """从字典创建"""
        return cls(**data)


@dataclass
class CheckSession:
    """检测会话 - 一次完整的检测过程"""
    id: str
    mode: Literal["quick", "detailed"]  # 快速/详细模式
    results: List[CheckResult] = field(default_factory=list)
    started_at: str = ""
    completed_at: str = ""
    duration_ms: int = 0

    def __post_init__(self):
        if not self.started_at:
            self.started_at = datetime.now().isoformat()

    @property
    def summary(self) -> Dict[str, Any]:
        """获取检测汇总"""
        passed = len([r for r in self.results if r.status == "passed"])
        failed = len([r for r in self.results if r.status == "failed"])
        warning = len([r for r in self.results if r.status == "warning"])
        skipped = len([r for r in self.results if r.status == "skipped"])

        return {
            "total": len(self.results),
            "passed": passed,
            "failed": failed,
            "warning": warning,
            "skipped": skipped,
            "pass_rate": round(passed / len(self.results) * 100, 1) if self.results else 0,
            "overall_status": "passed" if failed == 0 else "failed"
        }

    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        return {
            "id": self.id,
            "mode": self.mode,
            "results": [r.to_dict() for r in self.results],
            "started_at": self.started_at,
            "completed_at": self.completed_at,
            "duration_ms": self.duration_ms,
            "summary": self.summary
        }

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'CheckSession':
        """从字典创建"""
        results = [CheckResult.from_dict(r) for r in data.get("results", [])]
        return cls(
            id=data["id"],
            mode=data["mode"],
            results=results,
            started_at=data.get("started_at", ""),
            completed_at=data.get("completed_at", ""),
            duration_ms=data.get("duration_ms", 0)
        )


@dataclass
class CheckDefinition:
    """检测项定义"""
    id: str
    name: str
    description: str
    category: str
    category_name: str
    quick_check: bool = True
    timeout: int = 30  # 超时时间（秒）

    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        return asdict(self)