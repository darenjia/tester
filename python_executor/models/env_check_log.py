"""
环境检测日志模型
用于记录环境检测的历史记录
"""
import json
import uuid
import threading
from dataclasses import dataclass, field, asdict
from typing import Dict, Any, List, Optional
from datetime import datetime


@dataclass
class EnvCheckLog:
    """
    环境检测日志
    
    Attributes:
        id: 检测记录唯一ID
        check_time: 检测时间
        duration: 检测耗时(秒)
        overall_status: 总体状态 (passed/failed)
        details: 详细检测结果
        logs: 检测过程日志
        created_at: 创建时间
    """
    id: str = field(default_factory=lambda: str(uuid.uuid4()))
    check_time: str = field(default_factory=lambda: datetime.now().isoformat())
    duration: float = 0.0
    overall_status: str = "unknown"
    details: Dict[str, Any] = field(default_factory=dict)
    logs: List[str] = field(default_factory=list)
    created_at: str = field(default_factory=lambda: datetime.now().isoformat())
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        return asdict(self)
    
    def to_json(self) -> str:
        """转换为JSON字符串"""
        return json.dumps(self.to_dict(), ensure_ascii=False, indent=2)
    
    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'EnvCheckLog':
        """从字典创建"""
        valid_fields = {f.name for f in cls.__dataclass_fields__.values()}
        filtered_data = {k: v for k, v in data.items() if k in valid_fields}
        return cls(**filtered_data)


class EnvCheckLogManager:
    """
    环境检测日志管理器
    
    管理环境检测日志的增删改查
    """
    
    def __init__(self, max_logs: int = 100):
        """
        初始化日志管理器
        
        Args:
            max_logs: 最大保留日志数量
        """
        self._logs: List[EnvCheckLog] = []
        self._lock = threading.Lock()
        self.max_logs = max_logs
    
    def add_log(self, log: EnvCheckLog) -> bool:
        """
        添加日志
        
        Args:
            log: 日志对象
            
        Returns:
            是否添加成功
        """
        with self._lock:
            self._logs.append(log)
            
            # 限制日志数量
            if len(self._logs) > self.max_logs:
                self._logs.pop(0)
            
            return True
    
    def get_log(self, log_id: str) -> Optional[EnvCheckLog]:
        """
        获取指定日志
        
        Args:
            log_id: 日志ID
            
        Returns:
            日志对象，不存在则返回None
        """
        with self._lock:
            for log in self._logs:
                if log.id == log_id:
                    return log
            return None
    
    def get_all_logs(self, limit: Optional[int] = None) -> List[EnvCheckLog]:
        """
        获取所有日志
        
        Args:
            limit: 限制数量，None表示全部
            
        Returns:
            日志列表（按时间倒序）
        """
        with self._lock:
            logs = sorted(self._logs, key=lambda x: x.check_time, reverse=True)
            if limit:
                return logs[:limit]
            return logs
    
    def get_logs_by_status(self, status: str) -> List[EnvCheckLog]:
        """
        获取指定状态的日志
        
        Args:
            status: 状态 (passed/failed/unknown)
            
        Returns:
            日志列表
        """
        with self._lock:
            return [log for log in self._logs if log.overall_status == status]
    
    def clear_logs(self) -> int:
        """
        清空所有日志
        
        Returns:
            清空的日志数量
        """
        with self._lock:
            count = len(self._logs)
            self._logs.clear()
            return count
    
    def delete_log(self, log_id: str) -> bool:
        """
        删除指定日志
        
        Args:
            log_id: 日志ID
            
        Returns:
            是否删除成功
        """
        with self._lock:
            for i, log in enumerate(self._logs):
                if log.id == log_id:
                    self._logs.pop(i)
                    return True
            return False
    
    def get_stats(self) -> Dict[str, Any]:
        """
        获取统计信息
        
        Returns:
            统计信息字典
        """
        with self._lock:
            total = len(self._logs)
            passed = len([log for log in self._logs if log.overall_status == "passed"])
            failed = len([log for log in self._logs if log.overall_status == "failed"])
            
            return {
                "total": total,
                "passed": passed,
                "failed": failed,
                "pass_rate": (passed / total * 100) if total > 0 else 0
            }


# 全局日志管理器实例
env_check_log_manager = EnvCheckLogManager()
