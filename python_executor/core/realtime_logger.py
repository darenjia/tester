"""
实时日志流管理器

支持任务执行时的实时日志推送，通过WebSocket向客户端推送日志
"""
import time
import threading
import queue
from typing import Dict, Any, Optional, Callable, List
from dataclasses import dataclass, field
from enum import Enum
import logging

logger = logging.getLogger(__name__)


class LogLevel(Enum):
    """日志级别"""
    DEBUG = "DEBUG"
    INFO = "INFO"
    WARNING = "WARNING"
    ERROR = "ERROR"
    CRITICAL = "CRITICAL"


@dataclass
class LogEntry:
    """日志条目"""
    timestamp: float
    level: str
    message: str
    source: str = ""  # 日志来源（模块名）
    task_id: Optional[str] = None
    extra_data: Dict[str, Any] = field(default_factory=dict)
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        return {
            'timestamp': self.timestamp,
            'level': self.level,
            'message': self.message,
            'source': self.source,
            'task_id': self.task_id,
            'extra_data': self.extra_data,
            'time_str': time.strftime('%Y-%m-%d %H:%M:%S', time.localtime(self.timestamp)),
            'ms': int((self.timestamp % 1) * 1000)
        }


class RealtimeLogBuffer:
    """实时日志缓冲区"""
    
    def __init__(self, max_size: int = 10000):
        """
        初始化日志缓冲区
        
        Args:
            max_size: 最大日志条目数
        """
        self.max_size = max_size
        self._logs: List[LogEntry] = []
        self._lock = threading.RLock()
        self._subscribers: List[Callable[[LogEntry], None]] = []
        self._subscribers_lock = threading.Lock()
    
    def add_log(self, entry: LogEntry) -> None:
        """
        添加日志条目
        
        Args:
            entry: 日志条目
        """
        with self._lock:
            # 添加日志
            self._logs.append(entry)
            
            # 限制缓冲区大小
            if len(self._logs) > self.max_size:
                self._logs = self._logs[-self.max_size:]
        
        # 通知订阅者
        self._notify_subscribers(entry)
    
    def get_logs(self, since: Optional[float] = None, 
                 level: Optional[str] = None,
                 limit: int = 100) -> List[Dict[str, Any]]:
        """
        获取日志
        
        Args:
            since: 从此时间戳之后（可选）
            level: 日志级别过滤（可选）
            limit: 最大返回条数
            
        Returns:
            日志条目列表
        """
        with self._lock:
            logs = self._logs.copy()
        
        # 过滤
        if since is not None:
            logs = [log for log in logs if log.timestamp > since]
        
        if level is not None:
            level_priority = self._get_level_priority(level)
            logs = [log for log in logs 
                   if self._get_level_priority(log.level) >= level_priority]
        
        # 限制数量
        logs = logs[-limit:]
        
        return [log.to_dict() for log in logs]
    
    def subscribe(self, callback: Callable[[LogEntry], None]) -> None:
        """
        订阅日志
        
        Args:
            callback: 回调函数，接收LogEntry参数
        """
        with self._subscribers_lock:
            if callback not in self._subscribers:
                self._subscribers.append(callback)
    
    def unsubscribe(self, callback: Callable[[LogEntry], None]) -> None:
        """
        取消订阅
        
        Args:
            callback: 回调函数
        """
        with self._subscribers_lock:
            if callback in self._subscribers:
                self._subscribers.remove(callback)
    
    def _notify_subscribers(self, entry: LogEntry) -> None:
        """通知所有订阅者"""
        with self._subscribers_lock:
            subscribers = self._subscribers.copy()
        
        for callback in subscribers:
            try:
                callback(entry)
            except Exception as e:
                logger.error(f"Log subscriber error: {e}")
    
    def clear(self) -> None:
        """清空缓冲区"""
        with self._lock:
            self._logs.clear()
    
    def get_stats(self) -> Dict[str, Any]:
        """获取统计信息"""
        with self._lock:
            return {
                'total_logs': len(self._logs),
                'max_size': self.max_size,
                'subscriber_count': len(self._subscribers)
            }
    
    def _get_level_priority(self, level: str) -> int:
        """获取日志级别优先级"""
        priorities = {
            'DEBUG': 0,
            'INFO': 1,
            'WARNING': 2,
            'ERROR': 3,
            'CRITICAL': 4
        }
        return priorities.get(level.upper(), 0)


class RealtimeLogger:
    """实时日志管理器（单例）"""
    
    _instance = None
    _lock = threading.Lock()
    
    def __new__(cls):
        if cls._instance is None:
            with cls._lock:
                if cls._instance is None:
                    cls._instance = super().__new__(cls)
                    cls._instance._initialized = False
        return cls._instance
    
    def __init__(self):
        if self._initialized:
            return
        
        self._initialized = True
        self._buffers: Dict[str, RealtimeLogBuffer] = {}
        self._default_buffer = RealtimeLogBuffer()
        self._lock = threading.RLock()
        self._webhook_handlers: List[Callable[[LogEntry], None]] = []
        
        logger.info("RealtimeLogger initialized")
    
    def get_buffer(self, buffer_name: str = "default") -> RealtimeLogBuffer:
        """
        获取指定名称的日志缓冲区
        
        Args:
            buffer_name: 缓冲区名称
            
        Returns:
            RealtimeLogBuffer: 日志缓冲区实例
        """
        if buffer_name == "default":
            return self._default_buffer
        
        with self._lock:
            if buffer_name not in self._buffers:
                self._buffers[buffer_name] = RealtimeLogBuffer()
            return self._buffers[buffer_name]
    
    def create_buffer(self, buffer_name: str, max_size: int = 10000) -> RealtimeLogBuffer:
        """
        创建新的日志缓冲区
        
        Args:
            buffer_name: 缓冲区名称
            max_size: 最大日志条目数
            
        Returns:
            RealtimeLogBuffer: 创建的缓冲区实例
        """
        with self._lock:
            if buffer_name in self._buffers:
                logger.warning(f"Buffer {buffer_name} already exists, returning existing")
                return self._buffers[buffer_name]
            
            buffer = RealtimeLogBuffer(max_size=max_size)
            self._buffers[buffer_name] = buffer
            logger.info(f"Created log buffer: {buffer_name}")
            return buffer
    
    def remove_buffer(self, buffer_name: str) -> bool:
        """
        移除日志缓冲区
        
        Args:
            buffer_name: 缓冲区名称
            
        Returns:
            bool: 是否成功移除
        """
        with self._lock:
            if buffer_name in self._buffers:
                del self._buffers[buffer_name]
                logger.info(f"Removed log buffer: {buffer_name}")
                return True
            return False
    
    def log(self, level: str, message: str, source: str = "",
            task_id: Optional[str] = None, buffer_name: str = "default",
            extra_data: Optional[Dict[str, Any]] = None) -> None:
        """
        记录日志
        
        Args:
            level: 日志级别
            message: 日志消息
            source: 日志来源
            task_id: 任务ID（可选）
            buffer_name: 缓冲区名称
            extra_data: 额外数据（可选）
        """
        entry = LogEntry(
            timestamp=time.time(),
            level=level.upper(),
            message=message,
            source=source,
            task_id=task_id,
            extra_data=extra_data or {}
        )
        
        # 添加到缓冲区
        buffer = self.get_buffer(buffer_name)
        buffer.add_log(entry)
        
        # 触发webhook
        self._trigger_webhooks(entry)
    
    def debug(self, message: str, source: str = "", 
              task_id: Optional[str] = None, buffer_name: str = "default") -> None:
        """记录DEBUG级别日志"""
        self.log("DEBUG", message, source, task_id, buffer_name)
    
    def info(self, message: str, source: str = "",
             task_id: Optional[str] = None, buffer_name: str = "default") -> None:
        """记录INFO级别日志"""
        self.log("INFO", message, source, task_id, buffer_name)
    
    def warning(self, message: str, source: str = "",
                task_id: Optional[str] = None, buffer_name: str = "default") -> None:
        """记录WARNING级别日志"""
        self.log("WARNING", message, source, task_id, buffer_name)
    
    def error(self, message: str, source: str = "",
              task_id: Optional[str] = None, buffer_name: str = "default") -> None:
        """记录ERROR级别日志"""
        self.log("ERROR", message, source, task_id, buffer_name)
    
    def critical(self, message: str, source: str = "",
                 task_id: Optional[str] = None, buffer_name: str = "default") -> None:
        """记录CRITICAL级别日志"""
        self.log("CRITICAL", message, source, task_id, buffer_name)
    
    def subscribe(self, callback: Callable[[LogEntry], None], 
                  buffer_name: str = "default") -> None:
        """
        订阅日志
        
        Args:
            callback: 回调函数
            buffer_name: 缓冲区名称
        """
        buffer = self.get_buffer(buffer_name)
        buffer.subscribe(callback)
    
    def unsubscribe(self, callback: Callable[[LogEntry], None],
                    buffer_name: str = "default") -> None:
        """
        取消订阅
        
        Args:
            callback: 回调函数
            buffer_name: 缓冲区名称
        """
        buffer = self.get_buffer(buffer_name)
        buffer.unsubscribe(callback)
    
    def register_webhook(self, handler: Callable[[LogEntry], None]) -> None:
        """
        注册webhook处理器
        
        Args:
            handler: 处理器函数
        """
        if handler not in self._webhook_handlers:
            self._webhook_handlers.append(handler)
    
    def unregister_webhook(self, handler: Callable[[LogEntry], None]) -> None:
        """
        注销webhook处理器
        
        Args:
            handler: 处理器函数
        """
        if handler in self._webhook_handlers:
            self._webhook_handlers.remove(handler)
    
    def _trigger_webhooks(self, entry: LogEntry) -> None:
        """触发webhook"""
        for handler in self._webhook_handlers:
            try:
                handler(entry)
            except Exception as e:
                logger.error(f"Webhook handler error: {e}")
    
    def get_logs(self, buffer_name: str = "default", since: Optional[float] = None,
                 level: Optional[str] = None, limit: int = 100) -> List[Dict[str, Any]]:
        """
        获取日志
        
        Args:
            buffer_name: 缓冲区名称
            since: 从此时间戳之后（可选）
            level: 日志级别过滤（可选）
            limit: 最大返回条数
            
        Returns:
            日志条目列表
        """
        buffer = self.get_buffer(buffer_name)
        return buffer.get_logs(since, level, limit)
    
    def get_all_stats(self) -> Dict[str, Dict[str, Any]]:
        """获取所有缓冲区的统计信息"""
        stats = {'default': self._default_buffer.get_stats()}
        
        with self._lock:
            for name, buffer in self._buffers.items():
                stats[name] = buffer.get_stats()
        
        return stats
    
    def clear_buffer(self, buffer_name: str = "default") -> None:
        """清空缓冲区"""
        buffer = self.get_buffer(buffer_name)
        buffer.clear()
        logger.info(f"Cleared log buffer: {buffer_name}")


# 全局实时日志管理器实例
realtime_logger = RealtimeLogger()


def get_realtime_logger() -> RealtimeLogger:
    """获取实时日志管理器实例"""
    return realtime_logger


class TaskLogAdapter:
    """任务日志适配器，简化任务日志记录"""
    
    def __init__(self, task_id: str, source: str = ""):
        """
        初始化任务日志适配器
        
        Args:
            task_id: 任务ID
            source: 日志来源
        """
        self.task_id = task_id
        self.source = source
        self.logger = realtime_logger
    
    def debug(self, message: str) -> None:
        """记录DEBUG级别日志"""
        self.logger.debug(message, self.source, self.task_id, f"task_{self.task_id}")
    
    def info(self, message: str) -> None:
        """记录INFO级别日志"""
        self.logger.info(message, self.source, self.task_id, f"task_{self.task_id}")
    
    def warning(self, message: str) -> None:
        """记录WARNING级别日志"""
        self.logger.warning(message, self.source, self.task_id, f"task_{self.task_id}")
    
    def error(self, message: str) -> None:
        """记录ERROR级别日志"""
        self.logger.error(message, self.source, self.task_id, f"task_{self.task_id}")
    
    def critical(self, message: str) -> None:
        """记录CRITICAL级别日志"""
        self.logger.critical(message, self.source, self.task_id, f"task_{self.task_id}")
    
    def get_logs(self, since: Optional[float] = None, 
                 level: Optional[str] = None, limit: int = 100) -> List[Dict[str, Any]]:
        """获取此任务的日志"""
        return self.logger.get_logs(f"task_{self.task_id}", since, level, limit)
