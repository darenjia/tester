"""
用例映射模型 - 用于管理接口用例名称与脚本Case编号之间的映射关系
"""
from dataclasses import dataclass, field
from typing import Dict, Any, List, Optional
from datetime import datetime
from enum import Enum


class ChangeType(Enum):
    """变更类型枚举"""
    ADD = "ADD"
    MODIFY = "MODIFY"
    DELETE = "DELETE"
    ENABLE = "ENABLE"
    DISABLE = "DISABLE"


@dataclass
class CaseMapping:
    """用例映射模型

    用于建立接口用例名称与脚本Case编号之间的对应关系
    """
    case_no: str = ""                    # 脚本Case编号 (如 "CANOE-001")
    case_name: str = ""                  # 接口用例名称 (如 "CANoe安装路径检查")
    category: str = ""                   # 分类 (如 "canoe", "system", "tsmaster")
    module: str = ""                     # 模块名称 (如 "CANoe测试")
    script_path: str = ""                # cfg工程文件路径
    ini_config: str = ""                 # SelectInfo.ini配置内容 (原始INI格式)
    para_config: str = ""                # ParaInfo.ini默认参数 (原始INI格式)
    enabled: bool = True                 # 是否启用
    priority: int = 0                   # 优先级 (数字越大优先级越高)
    tags: List[str] = field(default_factory=list)  # 标签列表
    version: str = "1.0"                 # 版本号
    description: str = ""                # 用例描述
    created_at: str = ""                 # 创建时间
    updated_at: str = ""                 # 更新时间
    # TTworkbench专用字段
    ttcn3_source: str = ""               # TTCN-3源码路径
    ttthree_path: str = ""               # TTthree编译器路径
    compile_params: str = ""             # 编译参数字典（JSON格式）
    clf_file: str = ""                   # 预生成的CLF文件路径
    log_format: str = "pdf"              # 日志格式
    test_timeout: int = 3600             # 测试超时时间

    def __post_init__(self):
        if not self.created_at:
            self.created_at = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        if not self.updated_at:
            self.updated_at = self.created_at

    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        return {
            "case_no": self.case_no,
            "case_name": self.case_name,
            "category": self.category,
            "module": self.module,
            "script_path": self.script_path,
            "ini_config": self.ini_config,
            "para_config": self.para_config,
            "enabled": self.enabled,
            "priority": self.priority,
            "tags": self.tags,
            "version": self.version,
            "description": self.description,
            "created_at": self.created_at,
            "updated_at": self.updated_at,
            # TTworkbench专用字段
            "ttcn3_source": self.ttcn3_source,
            "ttthree_path": self.ttthree_path,
            "compile_params": self.compile_params,
            "clf_file": self.clf_file,
            "log_format": self.log_format,
            "test_timeout": self.test_timeout
        }

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'CaseMapping':
        """从字典创建"""
        return cls(
            case_no=data.get("case_no", ""),
            case_name=data.get("case_name", ""),
            category=data.get("category", ""),
            module=data.get("module", ""),
            script_path=data.get("script_path", ""),
            ini_config=data.get("ini_config", ""),
            para_config=data.get("para_config", ""),
            enabled=data.get("enabled", True),
            priority=data.get("priority", 0),
            tags=data.get("tags", []),
            version=data.get("version", "1.0"),
            description=data.get("description", ""),
            created_at=data.get("created_at", ""),
            updated_at=data.get("updated_at", ""),
            # TTworkbench专用字段
            ttcn3_source=data.get("ttcn3_source", ""),
            ttthree_path=data.get("ttthree_path", ""),
            compile_params=data.get("compile_params", ""),
            clf_file=data.get("clf_file", ""),
            log_format=data.get("log_format", "pdf"),
            test_timeout=data.get("test_timeout", 3600)
        )


@dataclass
class CaseChangeRecord:
    """用例变更记录模型

    用于追踪用例映射的所有变更历史
    """
    case_no: str = ""                    # Case编号
    change_type: str = ""                # 变更类型: ADD/MODIFY/DELETE/ENABLE/DISABLE
    old_value: Optional[Dict[str, Any]] = None   # 变更前的值
    new_value: Optional[Dict[str, Any]] = None   # 变更后的值
    change_reason: str = ""               # 变更原因
    changed_by: str = ""                  # 变更人
    changed_at: str = ""                 # 变更时间

    def __post_init__(self):
        if not self.changed_at:
            self.changed_at = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        return {
            "case_no": self.case_no,
            "change_type": self.change_type,
            "old_value": self.old_value,
            "new_value": self.new_value,
            "change_reason": self.change_reason,
            "changed_by": self.changed_by,
            "changed_at": self.changed_at
        }

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'CaseChangeRecord':
        """从字典创建"""
        return cls(
            case_no=data.get("case_no", ""),
            change_type=data.get("change_type", ""),
            old_value=data.get("old_value"),
            new_value=data.get("new_value"),
            change_reason=data.get("change_reason", ""),
            changed_by=data.get("changed_by", ""),
            changed_at=data.get("changed_at", "")
        )


@dataclass
class CaseMappingGroup:
    """用例映射分组模型

    用于对视图进行分组显示
    """
    category: str = ""                    # 分类ID
    category_name: str = ""                # 分类名称
    mappings: List[CaseMapping] = field(default_factory=list)  # 该分类下的映射
    total_count: int = 0                  # 总数
    enabled_count: int = 0                # 启用数量

    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        return {
            "category": self.category,
            "category_name": self.category_name,
            "mappings": [m.to_dict() for m in self.mappings],
            "total_count": self.total_count,
            "enabled_count": self.enabled_count
        }
