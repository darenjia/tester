"""
任务存储管理模块
提供线程安全的任务存储、查询和管理功能
"""
import threading
import uuid
from datetime import datetime
from typing import Dict, Any, List, Optional
from dataclasses import dataclass, field
from enum import Enum

from utils.logger import get_logger

logger = get_logger("task_store")


class TaskStatus(Enum):
    """任务状态枚举"""
    PENDING = "pending"
    RUNNING = "running"
    COMPLETED = "completed"
    FAILED = "failed"
    CANCELLED = "cancelled"


@dataclass
class TaskInfo:
    """任务信息数据类"""
    task_no: str                      # 使用taskNo作为主键
    project_no: str
    task_name: str
    status: TaskStatus
    created_at: datetime
    started_at: Optional[datetime] = None
    completed_at: Optional[datetime] = None
    progress: int = 0
    message: Optional[str] = None
    error_message: Optional[str] = None
    raw_data: Dict[str, Any] = field(default_factory=dict)
    results: List[Dict[str, Any]] = field(default_factory=list)
    summary: Optional[Dict[str, Any]] = None
    
    def to_dict(self, include_raw: bool = False) -> Dict[str, Any]:
        """转换为字典"""
        result = {
            "taskNo": self.task_no,      # 使用taskNo作为标识
            "projectNo": self.project_no,
            "taskName": self.task_name,
            "status": self.status.value,
            "progress": self.progress,
            "createdAt": self.created_at.isoformat(),
            "message": self.message
        }
        
        if self.started_at:
            result["startedAt"] = self.started_at.isoformat()
        if self.completed_at:
            result["completedAt"] = self.completed_at.isoformat()
        if self.error_message:
            result["errorMessage"] = self.error_message
        if self.summary:
            result["summary"] = self.summary
        if include_raw:
            result["rawData"] = self.raw_data
            result["results"] = self.results
            
        return result


class TaskStore:
    """
    任务存储管理器
    
    提供线程安全的任务CRUD操作和状态管理
    使用单例模式确保全局唯一实例
    """
    
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
            
        self._tasks: Dict[str, TaskInfo] = {}
        self._task_order: List[str] = []
        self._store_lock = threading.RLock()
        self._initialized = True
        
        logger.info("任务存储管理器初始化完成")
    
    def create_task(self, task_data: Dict[str, Any]) -> TaskInfo:
        """
        创建新任务
        
        Args:
            task_data: 任务数据，包含projectNo, taskNo, taskName等
            
        Returns:
            TaskInfo: 创建的任务信息
        """
        task_no = task_data.get("taskNo", "")
        if not task_no:
            raise ValueError("taskNo不能为空")
        
        task_info = TaskInfo(
            task_no=task_no,
            project_no=task_data.get("projectNo", ""),
            task_name=task_data.get("taskName", ""),
            status=TaskStatus.PENDING,
            created_at=datetime.now(),
            raw_data=task_data,
            message="任务已创建，等待执行"
        )
        
        with self._store_lock:
            self._tasks[task_no] = task_info
            self._task_order.append(task_no)
        
        logger.info(f"任务创建成功: taskNo={task_info.task_no}")
        return task_info
    
    def get_task(self, task_no: str) -> Optional[TaskInfo]:
        """
        获取任务信息
        
        Args:
            task_no: 任务编号
            
        Returns:
            TaskInfo或None
        """
        with self._store_lock:
            return self._tasks.get(task_no)
    
    def list_tasks(
        self, 
        status: Optional[TaskStatus] = None,
        page: int = 1,
        page_size: int = 20
    ) -> Dict[str, Any]:
        """
        获取任务列表
        
        Args:
            status: 筛选状态
            page: 页码（从1开始）
            page_size: 每页数量
            
        Returns:
            包含任务列表和分页信息的字典
        """
        with self._store_lock:
            # 筛选任务
            if status:
                filtered_tasks = [
                    task for task in self._tasks.values()
                    if task.status == status
                ]
            else:
                filtered_tasks = list(self._tasks.values())
            
            # 按创建时间倒序排序
            filtered_tasks.sort(key=lambda x: x.created_at, reverse=True)
            
            # 分页
            total = len(filtered_tasks)
            start_idx = (page - 1) * page_size
            end_idx = start_idx + page_size
            paginated_tasks = filtered_tasks[start_idx:end_idx]
            
            return {
                "tasks": [task.to_dict() for task in paginated_tasks],
                "total": total,
                "page": page,
                "pageSize": page_size,
                "totalPages": (total + page_size - 1) // page_size
            }
    
    def update_task_status(
        self, 
        task_no: str, 
        status: TaskStatus,
        message: Optional[str] = None,
        progress: Optional[int] = None
    ) -> bool:
        """
        更新任务状态
        
        Args:
            task_no: 任务编号
            status: 新状态
            message: 状态消息
            progress: 进度百分比
            
        Returns:
            bool: 更新成功返回True
        """
        with self._store_lock:
            task = self._tasks.get(task_no)
            if not task:
                logger.warning(f"更新状态失败，任务不存在: {task_no}")
                return False
            
            old_status = task.status
            task.status = status
            
            # 更新开始时间
            if status == TaskStatus.RUNNING and task.started_at is None:
                task.started_at = datetime.now()
            
            # 更新完成时间
            if status in [TaskStatus.COMPLETED, TaskStatus.FAILED, TaskStatus.CANCELLED]:
                task.completed_at = datetime.now()
            
            if message is not None:
                task.message = message
            if progress is not None:
                task.progress = max(0, min(100, progress))
            
            logger.info(
                f"任务状态更新: {task_no}, {old_status.value} -> {status.value}, "
                f"progress={task.progress}%"
            )
            return True
    
    def update_task_results(
        self,
        task_no: str,
        results: List[Dict[str, Any]],
        summary: Optional[Dict[str, Any]] = None
    ) -> bool:
        """
        更新任务结果
        
        Args:
            task_no: 任务编号
            results: 结果列表
            summary: 结果摘要
            
        Returns:
            bool: 更新成功返回True
        """
        with self._store_lock:
            task = self._tasks.get(task_no)
            if not task:
                logger.warning(f"更新结果失败，任务不存在: {task_no}")
                return False
            
            task.results = results
            if summary:
                task.summary = summary
            
            logger.info(f"任务结果更新: {task_no}, 结果数量={len(results)}")
            return True
    
    def add_task_result(
        self,
        task_no: str,
        result: Dict[str, Any]
    ) -> bool:
        """
        添加单个任务结果
        
        Args:
            task_no: 任务编号
            result: 单个结果
            
        Returns:
            bool: 添加成功返回True
        """
        with self._store_lock:
            task = self._tasks.get(task_no)
            if not task:
                logger.warning(f"添加结果失败，任务不存在: {task_no}")
                return False
            
            task.results.append(result)
            return True
    
    def set_task_error(self, task_no: str, error_message: str) -> bool:
        """
        设置任务错误信息
        
        Args:
            task_no: 任务编号
            error_message: 错误信息
            
        Returns:
            bool: 设置成功返回True
        """
        with self._store_lock:
            task = self._tasks.get(task_no)
            if not task:
                logger.warning(f"设置错误失败，任务不存在: {task_no}")
                return False
            
            task.error_message = error_message
            task.status = TaskStatus.FAILED
            task.completed_at = datetime.now()
            
            logger.error(f"任务错误: {task_no}, error={error_message}")
            return True
    
    def delete_task(self, task_no: str) -> bool:
        """
        删除任务
        
        Args:
            task_no: 任务编号
            
        Returns:
            bool: 删除成功返回True
        """
        with self._store_lock:
            if task_no not in self._tasks:
                return False
            
            del self._tasks[task_no]
            if task_no in self._task_order:
                self._task_order.remove(task_no)
            
            logger.info(f"任务删除: {task_no}")
            return True
    
    def cancel_task(self, task_no: str) -> bool:
        """
        取消任务
        
        Args:
            task_no: 任务编号
            
        Returns:
            bool: 取消成功返回True
        """
        with self._store_lock:
            task = self._tasks.get(task_no)
            if not task:
                return False
            
            # 只能取消待执行或执行中的任务
            if task.status not in [TaskStatus.PENDING, TaskStatus.RUNNING]:
                logger.warning(f"取消任务失败，任务状态不允许: {task_no}, status={task.status.value}")
                return False
            
            task.status = TaskStatus.CANCELLED
            task.completed_at = datetime.now()
            task.message = "任务已取消"
            
            logger.info(f"任务取消: {task_no}")
            return True
    
    def get_running_task(self) -> Optional[TaskInfo]:
        """
        获取当前正在执行的任务
        
        Returns:
            TaskInfo或None
        """
        with self._store_lock:
            for task in self._tasks.values():
                if task.status == TaskStatus.RUNNING:
                    return task
            return None
    
    def has_running_task(self) -> bool:
        """
        检查是否有正在执行的任务
        
        Returns:
            bool: 有正在执行的任务返回True
        """
        return self.get_running_task() is not None
    
    def get_statistics(self) -> Dict[str, int]:
        """
        获取任务统计信息
        
        Returns:
            各状态任务数量统计
        """
        with self._store_lock:
            stats = {status.value: 0 for status in TaskStatus}
            for task in self._tasks.values():
                stats[task.status.value] += 1
            stats["total"] = len(self._tasks)
            return stats
    
    def cleanup_old_tasks(self, max_age_hours: int = 24) -> int:
        """
        清理过期任务
        
        Args:
            max_age_hours: 最大保留时间（小时）
            
        Returns:
            int: 清理的任务数量
        """
        from datetime import timedelta
        
        cutoff_time = datetime.now() - timedelta(hours=max_age_hours)
        cleaned_count = 0
        
        with self._store_lock:
            tasks_to_remove = []
            for task_id, task in self._tasks.items():
                # 只清理已完成的任务
                if task.status in [TaskStatus.COMPLETED, TaskStatus.FAILED, TaskStatus.CANCELLED]:
                    if task.completed_at and task.completed_at < cutoff_time:
                        tasks_to_remove.append(task_id)
            
            for task_id in tasks_to_remove:
                del self._tasks[task_id]
                self._task_order.remove(task_id)
                cleaned_count += 1
        
        if cleaned_count > 0:
            logger.info(f"清理过期任务: {cleaned_count}个")
        
        return cleaned_count


# 全局任务存储实例
task_store = TaskStore()
