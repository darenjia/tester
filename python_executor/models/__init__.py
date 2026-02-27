# 模型模块初始化文件

from .task import (
    TaskStatus,
    TestToolType,
    CaseResultStatus,
    Case,
    Task,
    TestResult,
    TaskResult
)

from .result import (
    LogEntry,
    StatusUpdate,
    Message,
    CaseResult,
    ExecutionResult,
    TDM2Response
)

__all__ = [
    # Task模型
    'TaskStatus',
    'TestToolType',
    'CaseResultStatus',
    'Case',
    'Task',
    'TestResult',
    'TaskResult',
    # Result模型
    'LogEntry',
    'StatusUpdate',
    'Message',
    'CaseResult',
    'ExecutionResult',
    'TDM2Response'
]
