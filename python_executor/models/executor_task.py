"""
任务执行管理模型
用于管理外部接口调用的任务
"""
import json
import uuid
import threading
import sys
from dataclasses import dataclass, field, asdict
from typing import Dict, Any, List, Optional, Callable
from datetime import datetime, timedelta
from enum import Enum
import os

from utils.logger import get_logger

logger = get_logger("task_queue")


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
    支持持久化到文件，重启后自动恢复
    """

    _instance = None
    _init_lock = threading.Lock()

    def __new__(cls, storage_path: str = None):
        if cls._instance is None:
            with cls._init_lock:
                if cls._instance is None:
                    cls._instance = super().__new__(cls)
        return cls._instance

    def __init__(self, storage_path: str = None):
        if hasattr(self, '_initialized'):
            return
        self._initialized = True

        self._queue: List[Task] = []
        self._lock = threading.Lock()
        self._task_map: Dict[str, Task] = {}

        # 设置存储路径
        if storage_path is None:
            if getattr(sys, 'frozen', False):
                base_dir = os.path.dirname(sys.executable)
            else:
                base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
            storage_path = os.path.join(base_dir, 'data', 'tasks.json')

        self.storage_path = storage_path
        self._ensure_storage_dir()
        self._load()

    def _ensure_storage_dir(self):
        """确保存储目录存在"""
        storage_dir = os.path.dirname(self.storage_path)
        if storage_dir and not os.path.exists(storage_dir):
            os.makedirs(storage_dir, exist_ok=True)
            logger.info(f"创建存储目录: {storage_dir}")

    def _load(self):
        """从文件加载任务"""
        try:
            if os.path.exists(self.storage_path):
                with open(self.storage_path, 'r', encoding='utf-8') as f:
                    data = json.load(f)

                self._task_map.clear()
                self._queue.clear()

                now = datetime.now()
                retention_days = 30

                for task_id, task_data in data.get('tasks', {}).items():
                    try:
                        task = Task.from_dict(task_data)

                        # 处理 id 为 null 的情况 - 生成新的 UUID
                        if task.id is None:
                            task.id = str(uuid.uuid4())
                            logger.warning(f"任务缺少ID，已生成新ID: {task.id}")

                        # 处理 RUNNING 状态任务 - 重启后标记为失败
                        if task.status == TaskStatus.RUNNING.value:
                            task.status = TaskStatus.FAILED.value
                            task.error_message = "应用重启导致任务中断"
                            task.completed_at = now.isoformat()
                            logger.warning(f"任务 {task.id} 运行中被中断，已标记为失败")

                        # 跳过过期的已完成任务
                        if task.is_completed() and task.completed_at:
                            completed_time = datetime.fromisoformat(task.completed_at)
                            if (now - completed_time) > timedelta(days=retention_days):
                                logger.debug(f"跳过过期任务: {task.id}")
                                continue

                        # 使用任务的 id（而不是 JSON 的键）作为 map 的键
                        self._task_map[task.id] = task
                    except Exception as e:
                        logger.error(f"加载任务 {task_id} 失败: {e}")
                        continue

                # 重建 pending 队列（按优先级排序）
                pending_tasks = [t for t in self._task_map.values()
                               if t.status == TaskStatus.PENDING.value]
                pending_tasks.sort(key=lambda t: t.priority, reverse=True)
                self._queue = pending_tasks

                # 清理过多的已完成任务
                self._cleanup_old_tasks()

                logger.info(f"从存储加载了 {len(self._task_map)} 个任务，其中 {len(self._queue)} 个待执行")
            else:
                logger.info("未找到任务存储文件，使用空队列")
        except Exception as e:
            logger.error(f"加载任务存储失败: {e}")

    def _save(self):
        """保存任务到文件"""
        try:
            with self._lock:
                data = {
                    "tasks": {task_id: task.to_dict() for task_id, task in self._task_map.items()},
                    "version": "1.0",
                    "last_updated": datetime.now().isoformat()
                }

                # 原子写入：先写临时文件，再重命名
                temp_path = self.storage_path + '.tmp'
                with open(temp_path, 'w', encoding='utf-8') as f:
                    json.dump(data, f, ensure_ascii=False, indent=2)

                os.replace(temp_path, self.storage_path)
                logger.debug("任务数据已保存")
        except Exception as e:
            logger.error(f"保存任务存储失败: {e}")

    def _cleanup_old_tasks(self):
        """清理过多的已完成任务（保留最近100条）"""
        completed_statuses = [
            TaskStatus.COMPLETED.value,
            TaskStatus.FAILED.value,
            TaskStatus.CANCELLED.value,
            TaskStatus.TIMEOUT.value
        ]

        completed_tasks = [
            (task_id, task) for task_id, task in self._task_map.items()
            if task.status in completed_statuses
        ]

        # 按完成时间降序排序
        completed_tasks.sort(
            key=lambda x: x[1].completed_at or '',
            reverse=True
        )

        max_keep = 100
        if len(completed_tasks) > max_keep:
            for task_id, _ in completed_tasks[max_keep:]:
                del self._task_map[task_id]
                logger.debug(f"清理过期任务: {task_id}")

    def update_task_status(self, task_id: str, status: str,
                          error_message: str = None,
                          result: Dict[str, Any] = None) -> bool:
        """
        更新任务状态并自动保存

        Args:
            task_id: 任务ID
            status: 新状态
            error_message: 错误信息（可选）
            result: 执行结果（可选）

        Returns:
            是否更新成功
        """
        with self._lock:
            task = self._task_map.get(task_id)
            if not task:
                return False

            task.status = status
            if error_message is not None:
                task.error_message = error_message
            if result is not None:
                task.result = result

            if status == TaskStatus.RUNNING.value:
                task.started_at = datetime.now().isoformat()
            elif status in [TaskStatus.COMPLETED.value, TaskStatus.FAILED.value,
                          TaskStatus.CANCELLED.value, TaskStatus.TIMEOUT.value]:
                task.completed_at = datetime.now().isoformat()

            # 从队列中移除非 pending 任务
            if status != TaskStatus.PENDING.value and task in self._queue:
                self._queue.remove(task)

        self._save()
        return True
    
    def add(self, task: Task, overwrite: bool = True) -> bool:
        """
        添加任务到队列

        Args:
            task: 任务对象
            overwrite: 是否覆盖已存在的任务（默认True）

        Returns:
            是否添加成功
        """
        with self._lock:
            # 获取任务ID（兼容executor_task.Task和models.task.Task两种格式）
            task_id = getattr(task, 'id', None) or getattr(task, 'task_id', None)

            # 确保任务有有效的ID
            if task_id is None:
                task_id = str(uuid.uuid4())
                # 尝试设置到task对象上（如果对象支持）
                if hasattr(task, 'id') and getattr(task, 'id', None) is None:
                    task.id = task_id
                elif hasattr(task, 'task_id'):
                    task.task_id = task_id
                logger.info(f"为任务生成新ID: {task_id}")
            else:
                # 如果task对象用的是task_id但没有id属性，将task_id同步到id
                if not hasattr(task, 'id') or getattr(task, 'id', None) is None:
                    if hasattr(task, 'id'):
                        task.id = task_id

            if task_id in self._task_map:
                if not overwrite:
                    return False

                # 覆盖模式：移除旧任务
                old_task = self._task_map[task_id]
                if old_task in self._queue:
                    self._queue.remove(old_task)
                logger.info(f"覆盖已存在的任务: {task_id}")

            # 按优先级插入队列（兼容没有priority属性的任务）
            inserted = False
            task_priority = getattr(task, 'priority', 0)
            for i, existing_task in enumerate(self._queue):
                existing_priority = getattr(existing_task, 'priority', 0)
                if task_priority > existing_priority:
                    self._queue.insert(i, task)
                    inserted = True
                    break

            if not inserted:
                self._queue.append(task)

            self._task_map[task_id] = task

        self._save()
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

        self._save()
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

        if to_remove:
            self._save()
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
