"""
结果模型定义 - TDM2.0字段标准
"""
from dataclasses import dataclass, field
from typing import Dict, Any, List, Optional
from datetime import datetime


@dataclass
class LogEntry:
    """日志条目模型"""
    level: str
    message: str
    timestamp: datetime
    taskNo: Optional[str] = None
    details: Optional[Dict[str, Any]] = None
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        result = {
            "level": self.level,
            "message": self.message,
            "timestamp": self.timestamp.isoformat()
        }
        
        if self.taskNo:
            result["taskNo"] = self.taskNo
        if self.details:
            result["details"] = self.details
            
        return result


@dataclass
class StatusUpdate:
    """状态更新模型"""
    taskNo: str
    status: str
    message: Optional[str] = None
    progress: Optional[int] = None
    timestamp: datetime = None
    
    def __post_init__(self):
        if self.timestamp is None:
            self.timestamp = datetime.now()
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        result = {
            "taskNo": self.taskNo,
            "status": self.status,
            "timestamp": self.timestamp.isoformat()
        }
        
        if self.message:
            result["message"] = self.message
        if self.progress is not None:
            result["progress"] = self.progress
            
        return result


@dataclass
class Message:
    """消息模型"""
    type: str
    taskNo: Optional[str] = None
    deviceId: Optional[str] = None
    payload: Optional[Dict[str, Any]] = None
    timestamp: datetime = None
    
    def __post_init__(self):
        if self.timestamp is None:
            self.timestamp = datetime.now()
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        result = {
            "type": self.type,
            "timestamp": int(self.timestamp.timestamp() * 1000)
        }
        
        if self.taskNo:
            result["taskNo"] = self.taskNo
        if self.deviceId:
            result["deviceId"] = self.deviceId
        if self.payload:
            result["payload"] = self.payload
            
        return result
    
    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'Message':
        """从字典创建消息"""
        return cls(
            type=data.get("type", ""),
            taskNo=data.get("taskNo"),
            deviceId=data.get("deviceId"),
            payload=data.get("payload"),
            timestamp=datetime.fromtimestamp(data.get("timestamp", 0) / 1000) if data.get("timestamp") else None
        )


@dataclass
class CaseResult:
    """
    用例执行结果模型 - TDM2.0字段标准
    
    对应TDM2.0结果上报接口中的caseList项
    """
    caseNo: str                          # 用例编号 (必填)
    result: str                          # 结果: PASS/FAIL/BLOCK/SKIP (必填)
    remark: Optional[str] = None         # 备注 (可选)
    created: Optional[str] = None        # 测试执行时间 (可选)
    reportPath: Optional[str] = None     # 报告地址 (可选)
    
    # 扩展字段(内部使用，不上报TDM2.0)
    actualResult: Optional[str] = None   # 实际结果
    executionTime: Optional[float] = None  # 执行耗时(秒)
    
    def __post_init__(self):
        """初始化时设置默认时间"""
        if self.created is None:
            self.created = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    
    @classmethod
    def from_test_result(cls, case_no: str, passed: bool, message: str = None, 
                        report_path: str = None) -> 'CaseResult':
        """
        从测试结果创建CaseResult
        
        Args:
            case_no: 用例编号
            passed: 是否通过
            message: 备注信息
            report_path: 报告路径
            
        Returns:
            CaseResult实例
        """
        result_status = "PASS" if passed else "FAIL"
        return cls(
            caseNo=case_no,
            result=result_status,
            remark=message,
            reportPath=report_path
        )
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典 - TDM2.0格式"""
        result = {
            "caseNo": self.caseNo,
            "result": self.result
        }
        
        if self.remark is not None:
            result["remark"] = self.remark
        if self.created is not None:
            result["created"] = self.created
        if self.reportPath is not None:
            result["reportPath"] = self.reportPath
            
        return result


@dataclass
class ExecutionResult:
    """
    任务执行结果上报模型 - TDM2.0字段标准
    
    对应TDM2.0结果上报接口完整格式
    """
    taskNo: str                              # 任务编号 (必填)
    caseList: List[CaseResult] = field(default_factory=list)  # 用例集合 (必填)
    platform: str = "NETWORK"                # 平台名称 (必填)
    
    # 扩展字段(内部使用)
    startTime: Optional[datetime] = None     # 开始时间
    endTime: Optional[datetime] = None       # 结束时间
    summary: Optional[Dict[str, Any]] = None # 结果摘要
    
    def __post_init__(self):
        if self.caseList is None:
            self.caseList = []
    
    def add_case_result(self, case_result: CaseResult):
        """添加用例执行结果"""
        self.caseList.append(case_result)
    
    def generate_summary(self) -> Dict[str, Any]:
        """生成结果摘要"""
        if not self.caseList:
            return {
                "total": 0,
                "passed": 0,
                "failed": 0,
                "blocked": 0,
                "skipped": 0,
                "passRate": "0%"
            }
        
        total = len(self.caseList)
        passed = sum(1 for r in self.caseList if r.result == "PASS")
        failed = sum(1 for r in self.caseList if r.result == "FAIL")
        blocked = sum(1 for r in self.caseList if r.result == "BLOCK")
        skipped = sum(1 for r in self.caseList if r.result == "SKIP")
        pass_rate = f"{(passed / total * 100):.1f}%" if total > 0 else "0%"
        
        self.summary = {
            "total": total,
            "passed": passed,
            "failed": failed,
            "blocked": blocked,
            "skipped": skipped,
            "passRate": pass_rate
        }
        
        return self.summary
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典 - TDM2.0格式"""
        return {
            "taskNo": self.taskNo,
            "caseList": [case.to_dict() for case in self.caseList],
            "platform": self.platform
        }
    
    def to_tdm2_format(self) -> Dict[str, Any]:
        """
        转换为完整的TDM2.0上报格式
        
        包含所有TDM2.0接口要求的字段
        """
        return {
            "taskNo": self.taskNo,
            "platform": self.platform,
            "caseList": [case.to_dict() for case in self.caseList]
        }


@dataclass
class TDM2Response:
    """
    TDM2.0接口响应模型
    
    对应TDM2.0接口的响应格式
    """
    result: str                              # 状态码 (必填)
    msg: str                                 # 状态描述 (必填)
    extInfo: Optional[str] = None           # 扩展信息 (可选)
    rows: Optional[List[Dict]] = None       # 集合信息 (可选)
    rowcount: Optional[str] = None          # 总记录数 (可选)
    pageindex: Optional[str] = None         # 当前页 (可选)
    pagecount: Optional[str] = None         # 总页数 (可选)
    
    @classmethod
    def success(cls, message: str = "SUCCESS") -> 'TDM2Response':
        """创建成功响应"""
        return cls(result="1", msg=message)
    
    @classmethod
    def error(cls, message: str, result_code: str = "0") -> 'TDM2Response':
        """创建错误响应"""
        return cls(result=result_code, msg=message)
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        result = {
            "result": self.result,
            "msg": self.msg
        }
        
        if self.extInfo is not None:
            result["extInfo"] = self.extInfo
        if self.rows is not None:
            result["rows"] = self.rows
        if self.rowcount is not None:
            result["rowcount"] = self.rowcount
        if self.pageindex is not None:
            result["pageindex"] = self.pageindex
        if self.pagecount is not None:
            result["pagecount"] = self.pagecount
            
        return result
