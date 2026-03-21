"""
任务和结果模型定义 - TDM2.0字段标准
"""
from dataclasses import dataclass, field
from typing import Dict, Any, List, Optional
from datetime import datetime
from enum import Enum


class TaskStatus(Enum):
    """任务状态枚举"""
    PENDING = "pending"
    RUNNING = "running"
    COMPLETED = "completed"
    FAILED = "failed"
    CANCELLED = "cancelled"


class TestToolType(Enum):
    """测试工具类型枚举"""
    CANOE = "canoe"
    TSMASTER = "tsmaster"
    TTWORKBENCH = "ttworkbench"


class TestItemType(Enum):
    """测试项类型枚举"""
    SIGNAL_CHECK = "signal_check"
    SIGNAL_SET = "signal_set"
    TEST_MODULE = "test_module"
    DIAGNOSTIC = "diagnostic"
    WAIT = "wait"
    CONDITION = "condition"


class CaseResultStatus(Enum):
    """用例执行结果状态 - TDM2.0标准"""
    PASS = "PASS"
    FAIL = "FAIL"
    BLOCK = "BLOCK"
    SKIP = "SKIP"


@dataclass
class Case:
    """
    测试用例模型 - TDM2.0字段标准
    
    对应TDM2.0接口中的用例集合字段(14个字段)
    """
    # 必填字段
    moduleLevel1: str = ""           # 一级模块
    moduleLevel2: str = ""           # 二级模块
    moduleLevel3: str = ""           # 三级模块
    caseName: str = ""               # 用例名称
    priority: str = ""               # 优先级
    caseType: str = ""               # 用例类型
    preCondition: str = ""           # 前置条件
    stepDescription: str = ""        # 步骤描述
    expectedResult: str = ""         # 预期结果
    maintainer: str = ""             # 维护人
    caseNo: str = ""                 # 用例编号
    caseSource: str = ""             # 用例来源
    
    # 可选字段
    changeRecord: Optional[str] = None   # 用例变更记录
    tags: Optional[str] = None           # 用例标签
    
    # 执行相关字段(内部使用)
    actualResult: Optional[str] = None   # 实际结果
    testStatus: Optional[str] = None     # 测试状态
    
    # 新增字段 - 用于配置驱动的用例执行
    dtcInfo: Optional[str] = None        # DTC信息
    params: Dict[str, Any] = field(default_factory=dict)  # 测试参数
    repeat: int = 1                      # 重复执行次数
    
    # 兼容属性
    @property
    def name(self) -> str:
        """用例名称 - 兼容core/task_executor.py"""
        return self.caseName
    
    @property
    def type(self) -> str:
        """用例类型 - 兼容core/task_executor.py"""
        return self.caseType
    
    @property
    def dtc_info(self) -> Optional[str]:
        """DTC信息 - 兼容下划线命名"""
        return self.dtcInfo
    
    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'Case':
        """从字典创建用例 - 支持TDM2.0字段名"""
        return cls(
            moduleLevel1=data.get("moduleLevel1", ""),
            moduleLevel2=data.get("moduleLevel2", ""),
            moduleLevel3=data.get("moduleLevel3", ""),
            caseName=data.get("caseName", ""),
            priority=data.get("priority", ""),
            caseType=data.get("caseType", ""),
            preCondition=data.get("preCondition", ""),
            stepDescription=data.get("stepDescription", ""),
            expectedResult=data.get("expectedResult", ""),
            maintainer=data.get("maintainer", ""),
            caseNo=data.get("caseNo", ""),
            caseSource=data.get("caseSource", ""),
            changeRecord=data.get("changeRecord"),
            tags=data.get("tags"),
            actualResult=data.get("actualResult"),
            testStatus=data.get("testStatus"),
            # 新增字段
            dtcInfo=data.get("dtcInfo"),
            params=data.get("params", {}),
            repeat=data.get("repeat", 1)
        )
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典 - TDM2.0格式"""
        result = {
            "moduleLevel1": self.moduleLevel1,
            "moduleLevel2": self.moduleLevel2,
            "moduleLevel3": self.moduleLevel3,
            "caseName": self.caseName,
            "priority": self.priority,
            "caseType": self.caseType,
            "preCondition": self.preCondition,
            "stepDescription": self.stepDescription,
            "expectedResult": self.expectedResult,
            "maintainer": self.maintainer,
            "caseNo": self.caseNo,
            "caseSource": self.caseSource
        }
        
        if self.changeRecord is not None:
            result["changeRecord"] = self.changeRecord
        if self.tags is not None:
            result["tags"] = self.tags
        if self.actualResult is not None:
            result["actualResult"] = self.actualResult
        if self.testStatus is not None:
            result["testStatus"] = self.testStatus
        # 新增字段
        if self.dtcInfo is not None:
            result["dtcInfo"] = self.dtcInfo
        if self.params:
            result["params"] = self.params
        if self.repeat != 1:
            result["repeat"] = self.repeat
            
        return result


@dataclass
class Task:
    """
    任务模型 - TDM2.0字段标准
    
    对应TDM2.0接口中的任务推送字段
    """
    # TDM2.0标准字段
    projectNo: str = ""              # 项目编号
    taskNo: str = ""                 # 任务编号
    taskName: str = ""               # 任务名称
    caseList: List[Case] = field(default_factory=list)  # 用例集合
    
    # 内部使用字段
    deviceId: Optional[str] = None   # 设备ID
    toolType: Optional[str] = None   # 测试工具类型
    configPath: Optional[str] = None # 配置文件路径（直接指定cfg路径）
    timeout: int = 3600              # 超时时间(秒)
    timestamp: Optional[int] = None  # 时间戳
    
    # 新增字段 - 用于配置驱动的用例执行
    configName: Optional[str] = None     # 配置名称（用于查找cfg文件）
    variables: Dict[str, Any] = field(default_factory=dict)  # 测试变量值字典
    baseConfigDir: Optional[str] = None  # 基础配置目录
    canoeNamespace: Optional[str] = None # CANoe系统变量命名空间

    # 兼容属性 - 用于core/task_executor.py
    @property
    def tool_type(self) -> Optional[str]:
        """测试工具类型 - 兼容下划线命名"""
        return self.toolType

    @property
    def config_path(self) -> Optional[str]:
        """配置文件路径 - 兼容下划线命名"""
        return self.configPath

    @property
    def device_id(self) -> Optional[str]:
        """设备ID - 兼容下划线命名"""
        return self.deviceId

    @property
    def test_items(self) -> List:
        """测试项列表 - 兼容core/task_executor.py"""
        # 将caseList转换为test_items格式
        return self.caseList
    
    @property
    def config_name(self) -> Optional[str]:
        """配置名称 - 兼容下划线命名"""
        return self.configName
    
    @property
    def base_config_dir(self) -> Optional[str]:
        """基础配置目录 - 兼容下划线命名"""
        return self.baseConfigDir
    
    @property
    def canoe_namespace(self) -> Optional[str]:
        """CANoe命名空间 - 兼容下划线命名"""
        return self.canoeNamespace

    @property
    def task_id(self) -> str:
        """任务ID - 兼容executor"""
        return self.taskNo

    @property
    def task_type(self) -> str:
        """任务类型 - 兼容executor"""
        return getattr(self, '_task_type', 'test_module')

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'Task':
        """从字典创建任务 - 支持TDM2.0字段名"""
        case_list = []
        for case_data in data.get("caseList", []):
            case_list.append(Case.from_dict(case_data))
            
        return cls(
            projectNo=data.get("projectNo", ""),
            taskNo=data.get("taskNo", ""),
            taskName=data.get("taskName", ""),
            caseList=case_list,
            deviceId=data.get("deviceId"),
            toolType=data.get("toolType"),
            configPath=data.get("configPath"),
            timeout=data.get("timeout", 3600),
            timestamp=data.get("timestamp"),
            # 新增字段
            configName=data.get("configName"),
            variables=data.get("variables", {}),
            baseConfigDir=data.get("baseConfigDir"),
            canoeNamespace=data.get("canoeNamespace")
        )
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典 - TDM2.0格式"""
        result = {
            "projectNo": self.projectNo,
            "taskNo": self.taskNo,
            "taskName": self.taskName,
            "caseList": [case.to_dict() for case in self.caseList]
        }
        
        if self.deviceId is not None:
            result["deviceId"] = self.deviceId
        if self.toolType is not None:
            result["toolType"] = self.toolType
        if self.configPath is not None:
            result["configPath"] = self.configPath
        if self.timestamp is not None:
            result["timestamp"] = self.timestamp
        # 新增字段
        if self.configName is not None:
            result["configName"] = self.configName
        if self.variables:
            result["variables"] = self.variables
        if self.baseConfigDir is not None:
            result["baseConfigDir"] = self.baseConfigDir
        if self.canoeNamespace is not None:
            result["canoeNamespace"] = self.canoeNamespace
            
        return result


@dataclass
class TestResult:
    """
    测试结果模型 - 兼容TDM2.0和内部使用
    
    注意: TDM2.0结果上报使用CaseResult模型
    """
    name: str
    type: str
    expected: Optional[Any] = None
    actual: Optional[Any] = None
    passed: Optional[bool] = None
    success: Optional[bool] = None
    verdict: Optional[str] = None
    error: Optional[str] = None
    details: Optional[Dict[str, Any]] = None
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        result = {
            "name": self.name,
            "type": self.type
        }
        
        if self.expected is not None:
            result["expected"] = self.expected
        if self.actual is not None:
            result["actual"] = self.actual
        if self.passed is not None:
            result["passed"] = self.passed
        if self.success is not None:
            result["success"] = self.success
        if self.verdict is not None:
            result["verdict"] = self.verdict
        if self.error is not None:
            result["error"] = self.error
        if self.details is not None:
            result["details"] = self.details
            
        return result


@dataclass
class TaskResult:
    """
    任务结果模型 - 内部使用
    
    注意: 上报TDM2.0时需要转换为TDM2.0结果格式
    """
    taskNo: str
    status: str  # TaskStatus
    startTime: Optional[datetime] = None
    endTime: Optional[datetime] = None
    results: List[TestResult] = None
    summary: Optional[Dict[str, Any]] = None
    errorMessage: Optional[str] = None
    
    def __post_init__(self):
        if self.results is None:
            self.results = []
    
    def add_result(self, result: TestResult):
        """添加测试结果"""
        self.results.append(result)
    
    def generate_summary(self) -> Dict[str, Any]:
        """生成结果摘要"""
        if not self.results:
            return {"total": 0, "passed": 0, "failed": 0, "passRate": "0%"}
        
        total = len(self.results)
        passed = sum(1 for r in self.results if r.passed is True)
        failed = total - passed
        pass_rate = f"{(passed / total * 100):.1f}%" if total > 0 else "0%"
        
        return {
            "total": total,
            "passed": passed,
            "failed": failed,
            "passRate": pass_rate
        }
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        result = {
            "taskNo": self.taskNo,
            "status": self.status
        }
        
        if self.startTime:
            result["startTime"] = self.startTime.isoformat()
        if self.endTime:
            result["endTime"] = self.endTime.isoformat()
        if self.results:
            result["results"] = [r.to_dict() for r in self.results]
        if self.summary:
            result["summary"] = self.summary
        if self.errorMessage:
            result["errorMessage"] = self.errorMessage
            
        return result
