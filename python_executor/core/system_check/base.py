"""
检测基类
所有检测项都继承此基类
"""
from abc import ABC, abstractmethod
from typing import List, Dict, Any, Optional
from datetime import datetime
import time

from .models import CheckResult, CheckDefinition


class BaseCheck(ABC):
    """
    检测项基类

    所有检测项都需要继承此类并实现 execute 方法
    """

    # 子类必须定义这些属性
    id: str = ""
    name: str = ""
    description: str = ""
    category: str = ""
    category_name: str = ""
    quick_check: bool = True
    timeout: int = 30

    def __init__(self):
        self._start_time: Optional[float] = None

    @property
    def definition(self) -> CheckDefinition:
        """获取检测项定义"""
        return CheckDefinition(
            id=self.id,
            name=self.name,
            description=self.description,
            category=self.category,
            category_name=self.category_name,
            quick_check=self.quick_check,
            timeout=self.timeout
        )

    @abstractmethod
    def execute(self) -> CheckResult:
        """
        执行检测

        Returns:
            CheckResult: 检测结果
        """
        pass

    def run(self) -> CheckResult:
        """
        运行检测（包装 execute 方法，添加计时和错误处理）
        """
        self._start_time = time.time()

        try:
            result = self.execute()
        except Exception as e:
            result = self.create_result(
                status="failed",
                message=f"检测执行异常: {str(e)}",
                suggestions=["请检查系统环境或联系管理员"]
            )

        # 计算耗时
        if self._start_time:
            result.duration_ms = int((time.time() - self._start_time) * 1000)

        result.timestamp = datetime.now().isoformat()
        return result

    def create_result(
        self,
        status: str,
        message: str,
        details: Dict[str, Any] = None,
        suggestions: List[str] = None
    ) -> CheckResult:
        """
        创建检测结果

        Args:
            status: 状态 (passed/failed/warning/skipped)
            message: 结果消息
            details: 详细信息
            suggestions: 修复建议

        Returns:
            CheckResult: 检测结果对象
        """
        return CheckResult(
            check_id=self.id,
            name=self.name,
            category=self.category,
            category_name=self.category_name,
            status=status,
            message=message,
            details=details or {},
            suggestions=suggestions or [],
            quick_check=self.quick_check
        )

    def get_suggestions(self, error: str) -> List[str]:
        """
        获取修复建议（子类可重写）

        Args:
            error: 错误信息

        Returns:
            List[str]: 建议列表
        """
        return []