"""
任务日志模型
用于记录任务执行的详细日志
"""
import json
import uuid
import threading
from dataclasses import dataclass, field, asdict
from typing import Dict, Any, List, Optional
from datetime import datetime
from enum import Enum
import os


class LogLevel(Enum):
    """日志级别枚举"""
    DEBUG = "DEBUG"
    INFO = "INFO"
    WARNING = "WARNING"
    ERROR = "ERROR"
    CRITICAL = "CRITICAL"


@dataclass
class TaskLog:
    """
    任务日志条目
    
    Attributes:
        id: 日志唯一ID
        task_id: 关联任务ID
        timestamp: 时间戳
        level: 日志级别
        message: 日志消息
        details: 详细信息
        source: 日志来源
        line_number: 代码行号
    """
    id: str = field(default_factory=lambda: str(uuid.uuid4()))
    task_id: str = ""
    timestamp: str = field(default_factory=lambda: datetime.now().isoformat())
    level: str = field(default=LogLevel.INFO.value)
    message: str = ""
    details: Optional[Dict[str, Any]] = None
    source: Optional[str] = None
    line_number: Optional[int] = None
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        return asdict(self)
    
    def to_json(self) -> str:
        """转换为JSON字符串"""
        return json.dumps(self.to_dict(), ensure_ascii=False, indent=2)
    
    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'TaskLog':
        """从字典创建日志"""
        valid_fields = {f.name for f in cls.__dataclass_fields__.values()}
        filtered_data = {k: v for k, v in data.items() if k in valid_fields}
        return cls(**filtered_data)
    
    @classmethod
    def from_json(cls, json_str: str) -> 'TaskLog':
        """从JSON字符串创建日志"""
        return cls.from_dict(json.loads(json_str))
    
    def format_message(self) -> str:
        """格式化日志消息"""
        time_str = datetime.fromisoformat(self.timestamp).strftime("%Y-%m-%d %H:%M:%S.%f")[:-3]
        return f"[{time_str}] [{self.level}] {self.message}"


class TaskLogManager:
    """
    任务日志管理器
    
    管理任务执行日志的增删改查
    """
    
    def __init__(self, max_logs_per_task: int = 10000, max_total_logs: int = 100000):
        """
        初始化日志管理器
        
        Args:
            max_logs_per_task: 每个任务最大日志数量
            max_total_logs: 全局最大日志数量
        """
        self._logs: Dict[str, List[TaskLog]] = {}  # task_id -> logs
        self._all_logs: List[TaskLog] = []
        self._lock = threading.Lock()
        self.max_logs_per_task = max_logs_per_task
        self.max_total_logs = max_total_logs
        self._log_dir: Optional[str] = None
    
    def set_log_directory(self, log_dir: str):
        """设置日志文件存储目录"""
        self._log_dir = log_dir
        os.makedirs(log_dir, exist_ok=True)
    
    def add_log(self, log: TaskLog) -> bool:
        """
        添加日志
        
        Args:
            log: 日志对象
            
        Returns:
            是否添加成功
        """
        with self._lock:
            # 添加到任务日志列表
            if log.task_id not in self._logs:
                self._logs[log.task_id] = []
            
            self._logs[log.task_id].append(log)
            self._all_logs.append(log)
            
            # 限制单个任务的日志数量
            if len(self._logs[log.task_id]) > self.max_logs_per_task:
                removed = self._logs[log.task_id].pop(0)
                if removed in self._all_logs:
                    self._all_logs.remove(removed)
            
            # 限制全局日志数量
            if len(self._all_logs) > self.max_total_logs:
                removed = self._all_logs.pop(0)
                if removed.task_id in self._logs and removed in self._logs[removed.task_id]:
                    self._logs[removed.task_id].remove(removed)
            
            return True
    
    def log(self, task_id: str, level: str, message: str, 
            details: Optional[Dict[str, Any]] = None,
            source: Optional[str] = None,
            line_number: Optional[int] = None) -> TaskLog:
        """
        快速记录日志
        
        Args:
            task_id: 任务ID
            level: 日志级别
            message: 日志消息
            details: 详细信息
            source: 日志来源
            line_number: 代码行号
            
        Returns:
            创建的日志对象
        """
        log = TaskLog(
            task_id=task_id,
            level=level,
            message=message,
            details=details,
            source=source,
            line_number=line_number
        )
        self.add_log(log)
        return log
    
    def debug(self, task_id: str, message: str, **kwargs) -> TaskLog:
        """记录DEBUG级别日志"""
        return self.log(task_id, LogLevel.DEBUG.value, message, **kwargs)
    
    def info(self, task_id: str, message: str, **kwargs) -> TaskLog:
        """记录INFO级别日志"""
        return self.log(task_id, LogLevel.INFO.value, message, **kwargs)
    
    def warning(self, task_id: str, message: str, **kwargs) -> TaskLog:
        """记录WARNING级别日志"""
        return self.log(task_id, LogLevel.WARNING.value, message, **kwargs)
    
    def error(self, task_id: str, message: str, **kwargs) -> TaskLog:
        """记录ERROR级别日志"""
        return self.log(task_id, LogLevel.ERROR.value, message, **kwargs)
    
    def critical(self, task_id: str, message: str, **kwargs) -> TaskLog:
        """记录CRITICAL级别日志"""
        return self.log(task_id, LogLevel.CRITICAL.value, message, **kwargs)
    
    def get_logs_by_task(self, task_id: str, 
                         level: Optional[str] = None,
                         start_time: Optional[str] = None,
                         end_time: Optional[str] = None) -> List[TaskLog]:
        """
        获取指定任务的所有日志
        
        Args:
            task_id: 任务ID
            level: 日志级别筛选
            start_time: 开始时间筛选
            end_time: 结束时间筛选
            
        Returns:
            日志列表
        """
        with self._lock:
            logs = self._logs.get(task_id, [])
            
            # 应用筛选
            filtered_logs = logs
            
            if level:
                filtered_logs = [log for log in filtered_logs if log.level == level]
            
            if start_time:
                filtered_logs = [log for log in filtered_logs if log.timestamp >= start_time]
            
            if end_time:
                filtered_logs = [log for log in filtered_logs if log.timestamp <= end_time]
            
            return filtered_logs
    
    def get_all_logs(self, 
                     level: Optional[str] = None,
                     start_time: Optional[str] = None,
                     end_time: Optional[str] = None,
                     task_id: Optional[str] = None) -> List[TaskLog]:
        """
        获取所有日志
        
        Args:
            level: 日志级别筛选
            start_time: 开始时间筛选
            end_time: 结束时间筛选
            task_id: 任务ID筛选
            
        Returns:
            日志列表
        """
        with self._lock:
            logs = self._all_logs
            
            # 应用筛选
            if task_id:
                logs = [log for log in logs if log.task_id == task_id]
            
            if level:
                logs = [log for log in logs if log.level == level]
            
            if start_time:
                logs = [log for log in logs if log.timestamp >= start_time]
            
            if end_time:
                logs = [log for log in logs if log.timestamp <= end_time]
            
            return logs
    
    def get_latest_logs(self, count: int = 100, task_id: Optional[str] = None) -> List[TaskLog]:
        """
        获取最新的日志
        
        Args:
            count: 日志数量
            task_id: 任务ID筛选
            
        Returns:
            日志列表
        """
        with self._lock:
            if task_id:
                logs = self._logs.get(task_id, [])
            else:
                logs = self._all_logs
            
            return logs[-count:] if len(logs) > count else logs
    
    def clear_task_logs(self, task_id: str) -> int:
        """
        清理指定任务的所有日志
        
        Args:
            task_id: 任务ID
            
        Returns:
            清理的日志数量
        """
        with self._lock:
            if task_id not in self._logs:
                return 0
            
            logs_to_remove = self._logs[task_id]
            count = len(logs_to_remove)
            
            # 从全局日志中移除
            for log in logs_to_remove:
                if log in self._all_logs:
                    self._all_logs.remove(log)
            
            # 删除任务日志列表
            del self._logs[task_id]
            
            return count
    
    def clear_all_logs(self) -> int:
        """
        清理所有日志
        
        Returns:
            清理的日志数量
        """
        with self._lock:
            count = len(self._all_logs)
            self._logs.clear()
            self._all_logs.clear()
            return count
    
    def clear_old_logs(self, max_age: int) -> int:
        """
        清理过期的日志
        
        Args:
            max_age: 最大保留时间(秒)
            
        Returns:
            清理的日志数量
        """
        with self._lock:
            cutoff_time = (datetime.now() - __import__('datetime').timedelta(seconds=max_age)).isoformat()
            
            to_remove = [log for log in self._all_logs if log.timestamp < cutoff_time]
            
            for log in to_remove:
                self._all_logs.remove(log)
                if log.task_id in self._logs and log in self._logs[log.task_id]:
                    self._logs[log.task_id].remove(log)
            
            return len(to_remove)
    
    def get_log_count(self, task_id: Optional[str] = None) -> int:
        """
        获取日志数量
        
        Args:
            task_id: 任务ID，None表示所有日志
            
        Returns:
            日志数量
        """
        with self._lock:
            if task_id:
                return len(self._logs.get(task_id, []))
            return len(self._all_logs)
    
    def get_log_stats(self, task_id: Optional[str] = None) -> Dict[str, int]:
        """
        获取日志统计信息
        
        Args:
            task_id: 任务ID，None表示所有日志
            
        Returns:
            统计信息字典
        """
        with self._lock:
            if task_id:
                logs = self._logs.get(task_id, [])
            else:
                logs = self._all_logs
            
            stats = {
                "total": len(logs),
                "DEBUG": 0,
                "INFO": 0,
                "WARNING": 0,
                "ERROR": 0,
                "CRITICAL": 0
            }
            
            for log in logs:
                if log.level in stats:
                    stats[log.level] += 1
            
            return stats
    
    def export_logs_to_file(self, task_id: str, filepath: str) -> bool:
        """
        导出任务日志到文件
        
        Args:
            task_id: 任务ID
            filepath: 文件路径
            
        Returns:
            是否导出成功
        """
        try:
            logs = self.get_logs_by_task(task_id)
            
            with open(filepath, 'w', encoding='utf-8') as f:
                for log in logs:
                    f.write(log.format_message() + '\n')
                    if log.details:
                        f.write(f"  Details: {json.dumps(log.details, ensure_ascii=False)}\n")
            
            return True
        except Exception as e:
            print(f"导出日志失败: {e}")
            return False
    
    def save_to_file(self, filepath: str) -> bool:
        """
        保存所有日志到JSON文件
        
        Args:
            filepath: 文件路径
            
        Returns:
            是否保存成功
        """
        try:
            with self._lock:
                data = {
                    "export_time": datetime.now().isoformat(),
                    "logs": [log.to_dict() for log in self._all_logs]
                }
            
            with open(filepath, 'w', encoding='utf-8') as f:
                json.dump(data, f, ensure_ascii=False, indent=2)
            
            return True
        except Exception as e:
            print(f"保存日志失败: {e}")
            return False
    
    def load_from_file(self, filepath: str) -> bool:
        """
        从JSON文件加载日志
        
        Args:
            filepath: 文件路径
            
        Returns:
            是否加载成功
        """
        try:
            with open(filepath, 'r', encoding='utf-8') as f:
                data = json.load(f)
            
            with self._lock:
                self._logs.clear()
                self._all_logs.clear()
                
                for log_data in data.get("logs", []):
                    log = TaskLog.from_dict(log_data)
                    
                    if log.task_id not in self._logs:
                        self._logs[log.task_id] = []
                    
                    self._logs[log.task_id].append(log)
                    self._all_logs.append(log)
            
            return True
        except Exception as e:
            print(f"加载日志失败: {e}")
            return False


# 全局日志管理器实例
task_log_manager = TaskLogManager()
