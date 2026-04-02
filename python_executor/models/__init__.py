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
    ExecutionOutcome,
    ExecutionResult,
    TDM2Response
)

from .case_mapping import (
    ChangeType,
    CaseMapping,
    CaseChangeRecord,
    CaseMappingGroup
)
from .case_mapping_view import (
    CANoeMaterial,
    CaseMappingView,
    MappingDeclaration,
    MappingMaterial,
    TSMasterMaterial,
    TTworkbenchMaterial,
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
    'ExecutionOutcome',
    'ExecutionResult',
    'TDM2Response',
    # CaseMapping模型
    'ChangeType',
    'CaseMapping',
    'CaseChangeRecord',
    'CaseMappingGroup',
    'CaseMappingView',
    'MappingDeclaration',
    'MappingMaterial',
    'CANoeMaterial',
    'TSMasterMaterial',
    'TTworkbenchMaterial',
]
