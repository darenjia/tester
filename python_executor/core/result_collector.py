"""
结果收集与格式化模块
"""
import time
import logging
from datetime import datetime
from typing import Dict, Any, List, Optional
from dataclasses import dataclass, field

from utils.logger import get_logger
from models.task import TestResult
from models.result import StatusUpdate, LogEntry, ExecutionOutcome

logger = get_logger("result_collector")

@dataclass
class ResultCollector:
    """结果收集器"""

    taskNo: str
    start_time: datetime = field(default_factory=datetime.now)
    end_time: Optional[datetime] = None
    results: List[TestResult] = field(default_factory=list)
    logs: List[LogEntry] = field(default_factory=list)
    status_updates: List[StatusUpdate] = field(default_factory=list)

    def add_test_result(self, result: TestResult):
        """添加测试结果"""
        self.results.append(result)
        logger.debug(f"收集测试结果: {result.name} - {'通过' if result.passed else '失败'}")

    def add_log(self, level: str, message: str, details: Dict[str, Any] = None):
        """添加日志"""
        log_entry = LogEntry(
            level=level,
            message=message,
            timestamp=datetime.now(),
            taskNo=self.taskNo,
            details=details
        )
        self.logs.append(log_entry)
        level_mapping = {
            'DEBUG': logging.DEBUG,
            'INFO': logging.INFO,
            'WARNING': logging.WARNING,
            'ERROR': logging.ERROR,
            'CRITICAL': logging.CRITICAL
        }
        log_level = level_mapping.get(level.upper(), logging.INFO)
        logger.log(log_level, f"[{self.taskNo}] {message}")

    def add_status_update(self, status: str, message: str = None, progress: int = None):
        """添加状态更新"""
        status_update = StatusUpdate(
            taskNo=self.taskNo,
            status=status,
            message=message,
            progress=progress,
            timestamp=datetime.now()
        )
        self.status_updates.append(status_update)
        logger.info(f"任务状态更新: {status} - {message if message else ''}")

    def finalize(self, status: str = "completed", error_message: str = None) -> ExecutionOutcome:
        """完成结果收集"""
        self.end_time = datetime.now()

        # 生成结果摘要
        total = len(self.results)
        passed = sum(1 for r in self.results if r.passed is True)
        failed = total - passed

        duration = (self.end_time - self.start_time).total_seconds()

        summary = {
            "total": total,
            "passed": passed,
            "failed": failed,
            "passRate": f"{(passed / total * 100):.1f}%" if total > 0 else "0%",
            "duration": duration,
            "startedAt": self.start_time.isoformat(),
            "finishedAt": self.end_time.isoformat()
        }

        verdict = "PASS" if total > 0 and failed == 0 else "FAIL" if failed > 0 else None

        outcome = ExecutionOutcome(
            task_no=self.taskNo,
            status=status,
            started_at=self.start_time,
            finished_at=self.end_time,
            verdict=verdict,
            case_results=list(self.results),
            summary=summary,
            metrics={"duration": duration},
            error_summary=error_message,
        )

        logger.info(f"结果收集完成 - 总计: {total}, 通过: {passed}, 失败: {failed}, 耗时: {duration:.1f}秒")
        return outcome
    
    def get_progress(self) -> int:
        """获取执行进度百分比"""
        if not self.results:
            return 0
        
        # 简单计算：基于已完成的结果数量
        # 可以根据实际需求调整算法
        return min(100, len(self.results) * 10)  # 假设每个结果代表10%进度
    
    def get_current_status(self) -> Dict[str, Any]:
        """获取当前状态"""
        return {
            "task_id": self.taskNo,
            "start_time": self.start_time.isoformat(),
            "end_time": self.end_time.isoformat() if self.end_time else None,
            "result_count": len(self.results),
            "log_count": len(self.logs),
            "progress": self.get_progress(),
            "duration": (datetime.now() - self.start_time).total_seconds()
        }
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        task_result = self.finalize().to_task_result()
        return {
            "task_result": task_result.to_dict(),
            "logs": [log.to_dict() for log in self.logs],
            "status_updates": [update.to_dict() for update in self.status_updates]
        }

class ResultFormatter:
    """结果格式化器"""

    @staticmethod
    def _normalize_outcome(task_result: ExecutionOutcome):
        if isinstance(task_result, ExecutionOutcome):
            return task_result
        return ExecutionOutcome(
            task_no=task_result.taskNo,
            status=task_result.status,
            started_at=task_result.startTime,
            finished_at=task_result.endTime,
            case_results=list(task_result.results or []),
            summary=dict(task_result.summary or task_result.generate_summary() or {}),
            error_summary=task_result.errorMessage,
        )
    
    @staticmethod
    def format_test_result(result: TestResult) -> str:
        """格式化单个测试结果"""
        status = "✅ 通过" if result.passed else "❌ 失败"
        
        if result.type == "signal_check":
            return f"{status} {result.name}: 期望值={result.expected}, 实际值={result.actual}"
        elif result.type == "signal_set":
            return f"{status} {result.name}: {'设置成功' if result.success else '设置失败'}"
        elif result.type == "test_module":
            verdict = result.verdict or "未知"
            return f"{status} {result.name}: 判决={verdict}"
        else:
            return f"{status} {result.name}: 类型={result.type}"
    
    @staticmethod
    def format_task_summary(task_result) -> str:
        """格式化任务摘要"""
        outcome = ResultFormatter._normalize_outcome(task_result)
        summary = outcome.summary or {}
        duration = summary.get("duration")
        if duration is None and outcome.duration is not None:
            duration = outcome.duration
        
        lines = [
            "=" * 50,
            f"任务执行摘要 - {outcome.task_no}",
            "=" * 50,
            f"状态: {outcome.status}",
            f"开始时间: {outcome.started_at}",
            f"结束时间: {outcome.finished_at}",
            f"总计: {summary.get('total', 0)}, 通过: {summary.get('passed', 0)}, 失败: {summary.get('failed', 0)}",
            f"通过率: {summary.get('passRate', '0%')}",
            f"耗时: {(duration or 0):.1f}秒",
            "=" * 50
        ]
        
        if outcome.error_summary:
            lines.extend([
                "错误信息:",
                outcome.error_summary,
                "=" * 50
            ])
        
        return "\n".join(lines)
    
    @staticmethod
    def format_detailed_results(task_result) -> str:
        """格式化详细结果"""
        outcome = ResultFormatter._normalize_outcome(task_result)
        lines = [ResultFormatter.format_task_summary(outcome)]
        
        if outcome.results:
            lines.extend([
                "",
                "详细结果:",
                "-" * 30
            ])
            
            for i, result in enumerate(outcome.results, 1):
                lines.append(f"{i}. {ResultFormatter.format_test_result(result)}")
        
        return "\n".join(lines)
