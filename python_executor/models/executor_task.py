"""
任务执行管理模型
用于管理外部接口调用的任务
"""
import json
import uuid
import threading
from dataclasses import dataclass, field, asdict
from typing import Dict, Any, List, Optional, Callable
from datetime import datetime
from enum import Enum
import os


class TaskStatus(Enum):
    """任务状态枚举"""
    PENDING = "pending"      # 排队中
    RUNNING = "running"      # 进行中
    COMPLETED = "completed"  # 已完成
    FAILED = "failed"        # 失败
    CANCELLED = "cancelled"  # 已取消
    TIMEOUT = "timeout"      # 超时


class TaskPriority(Enum):
    """任务优先级枚举"""
    LOW = 0
    NORMAL = 1
    HIGH = 2
    URGENT = 3


@dataclass
class Task:
    """
    任务模型
    
    Attributes:
        id: 任务唯一ID
        name: 任务名称
        status: 任务状态
        priority: 任务优先级
        task_type: 任务类型
        params: 任务参数
        result: 执行结果
        error_message: 错误信息
        created_at: 创建时间
        started_at: 开始时间
        completed_at: 完成时间
        timeout: 超时时间(秒)
        retry_count: 重试次数
        max_retries: 最大重试次数
        created_by: 创建者
        metadata: 额外元数据
    """
    id: str = field(default_factory=lambda: str(uuid.uuid4()))
    name: str = ""
    status: str = field(default=TaskStatus.PENDING.value)
    priority: int = field(default=TaskPriority.NORMAL.value)
    task_type: str = "default"
    params: Dict[str, Any] = field(default_factory=dict)
    result: Optional[Dict[str, Any]] = None
    error_message: Optional[str] = None
    created_at: str = field(default_factory=lambda: datetime.now().isoformat())
    started_at: Optional[str] = None
    completed_at: Optional[str] = None
    timeout: int = 3600
    retry_count: int = 0
    max_retries: int = 3
    created_by: Optional[str] = None
    metadata: Dict[str, Any] = field(default_factory=dict)
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        return asdict(self)
    
    def to_json(self) -> str:
        """转换为JSON字符串"""
        return json.dumps(self.to_dict(), ensure_ascii=False, indent=2)
    
    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'Task':
        """从字典创建任务"""
        # 过滤掉不存在的字段
        valid_fields = {f.name for f in cls.__dataclass_fields__.values()}
        filtered_data = {k: v for k, v in data.items() if k in valid_fields}
        return cls(**filtered_data)
    
    @classmethod
    def from_json(cls, json_str: str) -> 'Task':
        """从JSON字符串创建任务"""
        return cls.from_dict(json.loads(json_str))
    
    def get_duration(self) -> Optional[float]:
        """获取任务执行时长(秒)"""
        if self.started_at and self.completed_at:
            start = datetime.fromisoformat(self.started_at)
            end = datetime.fromisoformat(self.completed_at)
            return (end - start).total_seconds()
        elif self.started_at:
            start = datetime.fromisoformat(self.started_at)
            return (datetime.now() - start).total_seconds()
        return None
    
    def get_wait_time(self) -> float:
        """获取任务等待时间(秒)"""
        created = datetime.fromisoformat(self.created_at)
        if self.started_at:
            started = datetime.fromisoformat(self.started_at)
            return (started - created).total_seconds()
        return (datetime.now() - created).total_seconds()
    
    def is_running(self) -> bool:
        """检查任务是否正在运行"""
        return self.status == TaskStatus.RUNNING.value
    
    def is_completed(self) -> bool:
        """检查任务是否已完成"""
        return self.status in [
            TaskStatus.COMPLETED.value,
            TaskStatus.FAILED.value,
            TaskStatus.CANCELLED.value,
            TaskStatus.TIMEOUT.value
        ]
    
    def can_retry(self) -> bool:
        """检查任务是否可以重试"""
        return self.retry_count < self.max_retries and self.status in [
            TaskStatus.FAILED.value,
            TaskStatus.TIMEOUT.value
        ]


class TaskQueue:
    """
    任务队列管理器
    
    支持优先级队列，高优先级任务优先执行
    """
    
    def __init__(self):
        self._queue: List[Task] = []
        self._lock = threading.Lock()
        self._task_map: Dict[str, Task] = {}
    
    def add(self, task: Task) -> bool:
        """
        添加任务到队列
        
        Args:
            task: 任务对象
            
        Returns:
            是否添加成功
        """
        with self._lock:
            if task.id in self._task_map:
                return False
            
            # 按优先级插入队列
            inserted = False
            for i, existing_task in enumerate(self._queue):
                if task.priority > existing_task.priority:
                    self._queue.insert(i, task)
                    inserted = True
                    break
            
            if not inserted:
                self._queue.append(task)
            
            self._task_map[task.id] = task
            return True
    
    def get(self) -> Optional[Task]:
        """
        获取队列中的下一个任务
        
        Returns:
            任务对象，如果队列为空则返回None
        """
        with self._lock:
            if not self._queue:
                return None
            
            task = self._queue.pop(0)
            return task
    
    def peek(self) -> Optional[Task]:
        """
        查看队列中的下一个任务（不移除）
        
        Returns:
            任务对象，如果队列为空则返回None
        """
        with self._lock:
            if not self._queue:
                return None
            return self._queue[0]
    
    def remove(self, task_id: str) -> bool:
        """
        从队列中移除指定任务
        
        Args:
            task_id: 任务ID
            
        Returns:
            是否移除成功
        """
        with self._lock:
            if task_id not in self._task_map:
                return False
            
            task = self._task_map[task_id]
            if task in self._queue:
                self._queue.remove(task)
            del self._task_map[task_id]
            return True
    
    def get_task(self, task_id: str) -> Optional[Task]:
        """
        获取指定任务
        
        Args:
            task_id: 任务ID
            
        Returns:
            任务对象，如果不存在则返回None
        """
        with self._lock:
            return self._task_map.get(task_id)
    
    def get_all_tasks(self) -> List[Task]:
        """
        获取所有任务
        
        Returns:
            任务列表
        """
        with self._lock:
            return list(self._task_map.values())
    
    def get_tasks_by_status(self, status: str) -> List[Task]:
        """
        获取指定状态的所有任务
        
        Args:
            status: 任务状态
            
        Returns:
            任务列表
        """
        with self._lock:
            return [task for task in self._task_map.values() if task.status == status]
    
    def get_pending_tasks(self) -> List[Task]:
        """获取所有待处理任务"""
        return self.get_tasks_by_status(TaskStatus.PENDING.value)
    
    def get_running_tasks(self) -> List[Task]:
        """获取所有运行中任务"""
        return self.get_tasks_by_status(TaskStatus.RUNNING.value)
    
    def get_completed_tasks(self) -> List[Task]:
        """获取所有已完成任务"""
        with self._lock:
            return [
                task for task in self._task_map.values()
                if task.status in [
                    TaskStatus.COMPLETED.value,
                    TaskStatus.FAILED.value,
                    TaskStatus.CANCELLED.value,
                    TaskStatus.TIMEOUT.value
                ]
            ]
    
    def clear_completed(self, max_age: Optional[int] = None) -> int:
        """
        清理已完成的任务
        
        Args:
            max_age: 最大保留时间(秒)，None表示清理所有
            
        Returns:
            清理的任务数量
        """
        with self._lock:
            to_remove = []
            for task_id, task in self._task_map.items():
                if task.is_completed():
                    if max_age is None:
                        to_remove.append(task_id)
                    elif task.completed_at:
                        completed_time = datetime.fromisoformat(task.completed_at)
                        if (datetime.now() - completed_time).total_seconds() > max_age:
                            to_remove.append(task_id)
            
            for task_id in to_remove:
                del self._task_map[task_id]
            
            # 清理队列中的已完成任务
            self._queue = [task for task in self._queue if task.id not in to_remove]
            
            return len(to_remove)
    
    def size(self) -> int:
        """获取队列大小"""
        with self._lock:
            return len(self._queue)
    
    def total_count(self) -> int:
        """获取任务总数"""
        with self._lock:
            return len(self._task_map)
    
    def get_stats(self) -> Dict[str, int]:
        """获取任务统计信息"""
        with self._lock:
            stats = {
                "total": len(self._task_map),
                "pending": 0,
                "running": 0,
                "completed": 0,
                "failed": 0,
                "cancelled": 0,
                "timeout": 0
            }
            
            for task in self._task_map.values():
                if task.status in stats:
                    stats[task.status] += 1
            
            stats["completed_total"] = (
                stats["completed"] + stats["failed"] + 
                stats["cancelled"] + stats["timeout"]
            )
            
            return stats


# 全局任务队列实例
task_queue = TaskQueue()
