"""
状态监控模块
用于收集和提供系统运行状态信息
"""
import psutil
import time
import os
from datetime import datetime
from typing import Dict, Any, Optional
from enum import Enum


class ConnectionStatus(Enum):
    """连接状态"""
    DISCONNECTED = "disconnected"
    CONNECTING = "connecting"
    CONNECTED = "connected"
    ERROR = "error"


class TaskStatus(Enum):
    """任务状态"""
    IDLE = "idle"
    PENDING = "pending"
    RUNNING = "running"
    COMPLETED = "completed"
    FAILED = "failed"
    CANCELLED = "cancelled"


class SoftwareStatus(Enum):
    """测试软件状态"""
    NOT_INSTALLED = "not_installed"
    NOT_RUNNING = "not_running"
    READY = "ready"
    BUSY = "busy"
    ERROR = "error"


class StatusMonitor:
    """状态监控器"""
    
    def __init__(self):
        self._websocket_status = ConnectionStatus.DISCONNECTED
        self._websocket_last_heartbeat = None
        self._current_task: Optional[Dict[str, Any]] = None
        self._task_start_time: Optional[datetime] = None
        self._software_status = {
            'canoe': SoftwareStatus.NOT_INSTALLED,
            'tsmaster': SoftwareStatus.NOT_INSTALLED,
            'ttworkbench': SoftwareStatus.NOT_INSTALLED
        }
        self._system_stats = {
            'cpu_percent': 0.0,
            'memory_percent': 0.0,
            'memory_used_gb': 0.0,
            'memory_total_gb': 0.0,
            'disk_percent': 0.0
        }
        self._start_time = datetime.now()
        self._last_update = time.time()
    
    def update_websocket_status(self, status: ConnectionStatus, last_heartbeat: datetime = None):
        """更新WebSocket连接状态"""
        self._websocket_status = status
        if last_heartbeat:
            self._websocket_last_heartbeat = last_heartbeat
    
    def update_task_status(self, task_info: Dict[str, Any], status: TaskStatus = None):
        """
        更新任务状态
        
        Args:
            task_info: 任务信息字典
            status: 任务状态（可选）
        """
        if status:
            task_info['status'] = status.value
        self._current_task = task_info
        
        if status == TaskStatus.RUNNING and not self._task_start_time:
            self._task_start_time = datetime.now()
        elif status in [TaskStatus.COMPLETED, TaskStatus.FAILED, TaskStatus.CANCELLED]:
            self._task_start_time = None
    
    def clear_current_task(self):
        """清除当前任务"""
        self._current_task = None
        self._task_start_time = None
    
    def update_software_status(self, software: str, status: SoftwareStatus):
        """
        更新测试软件状态
        
        Args:
            software: 软件名称 (canoe, tsmaster, ttworkbench)
            status: 软件状态
        """
        if software in self._software_status:
            self._software_status[software] = status
    
    def update_system_stats(self):
        """更新系统资源统计"""
        try:
            # CPU使用率
            self._system_stats['cpu_percent'] = psutil.cpu_percent(interval=0.1)
            
            # 内存信息
            memory = psutil.virtual_memory()
            self._system_stats['memory_percent'] = memory.percent
            self._system_stats['memory_used_gb'] = round(memory.used / (1024**3), 2)
            self._system_stats['memory_total_gb'] = round(memory.total / (1024**3), 2)
            
            # 磁盘信息（使用当前目录）
            disk = psutil.disk_usage('.')
            self._system_stats['disk_percent'] = disk.percent
            
            self._last_update = time.time()
        except Exception as e:
            print(f"更新系统统计信息失败: {e}")
    
    def get_websocket_status(self) -> Dict[str, Any]:
        """获取WebSocket连接状态"""
        return {
            'status': self._websocket_status.value,
            'connected': self._websocket_status == ConnectionStatus.CONNECTED,
            'last_heartbeat': self._websocket_last_heartbeat.isoformat() if self._websocket_last_heartbeat else None,
            'last_heartbeat_display': self._format_time(self._websocket_last_heartbeat) if self._websocket_last_heartbeat else '从未'
        }
    
    def get_task_status(self) -> Dict[str, Any]:
        """获取当前任务状态"""
        if not self._current_task:
            return {
                'has_task': False,
                'status': TaskStatus.IDLE.value,
                'status_display': '空闲',
                'message': '当前没有执行任务'
            }
        
        task_info = self._current_task.copy()
        task_info['has_task'] = True
        
        # 计算运行时间
        if self._task_start_time:
            elapsed = datetime.now() - self._task_start_time
            task_info['elapsed_seconds'] = int(elapsed.total_seconds())
            task_info['elapsed_display'] = self._format_duration(elapsed)
        
        # 状态显示文本
        status_mapping = {
            TaskStatus.PENDING.value: '等待中',
            TaskStatus.RUNNING.value: '执行中',
            TaskStatus.COMPLETED.value: '已完成',
            TaskStatus.FAILED.value: '失败',
            TaskStatus.CANCELLED.value: '已取消'
        }
        task_info['status_display'] = status_mapping.get(
            task_info.get('status'), '未知'
        )
        
        return task_info
    
    def get_software_status(self) -> Dict[str, Any]:
        """获取测试软件状态"""
        status_mapping = {
            SoftwareStatus.NOT_INSTALLED: '未安装',
            SoftwareStatus.NOT_RUNNING: '未运行',
            SoftwareStatus.READY: '就绪',
            SoftwareStatus.BUSY: '忙碌',
            SoftwareStatus.ERROR: '错误'
        }
        
        return {
            'canoe': {
                'status': self._software_status['canoe'].value,
                'status_display': status_mapping[self._software_status['canoe']],
                'ready': self._software_status['canoe'] == SoftwareStatus.READY
            },
            'tsmaster': {
                'status': self._software_status['tsmaster'].value,
                'status_display': status_mapping[self._software_status['tsmaster']],
                'ready': self._software_status['tsmaster'] == SoftwareStatus.READY
            },
            'ttworkbench': {
                'status': self._software_status['ttworkbench'].value,
                'status_display': status_mapping[self._software_status['ttworkbench']],
                'ready': self._software_status['ttworkbench'] == SoftwareStatus.READY
            }
        }
    
    def get_system_stats(self) -> Dict[str, Any]:
        """获取系统资源统计"""
        # 如果超过5秒未更新，重新获取
        if time.time() - self._last_update > 5:
            self.update_system_stats()
        
        return self._system_stats.copy()
    
    def get_all_status(self) -> Dict[str, Any]:
        """获取所有状态信息"""
        return {
            'websocket': self.get_websocket_status(),
            'task': self.get_task_status(),
            'software': self.get_software_status(),
            'system': self.get_system_stats(),
            'uptime': self._format_duration(datetime.now() - self._start_time),
            'timestamp': datetime.now().isoformat()
        }
    
    @staticmethod
    def _format_time(dt: datetime) -> str:
        """格式化时间显示"""
        if not dt:
            return ''
        now = datetime.now()
        if dt.date() == now.date():
            return dt.strftime('%H:%M:%S')
        return dt.strftime('%Y-%m-%d %H:%M:%S')
    
    @staticmethod
    def _format_duration(duration) -> str:
        """格式化时长显示"""
        total_seconds = int(duration.total_seconds())
        hours = total_seconds // 3600
        minutes = (total_seconds % 3600) // 60
        seconds = total_seconds % 60
        
        if hours > 0:
            return f"{hours}小时{minutes}分{seconds}秒"
        elif minutes > 0:
            return f"{minutes}分{seconds}秒"
        else:
            return f"{seconds}秒"


# 全局状态监控器实例
_status_monitor = None


def get_status_monitor() -> StatusMonitor:
    """获取状态监控器实例（单例模式）"""
    global _status_monitor
    if _status_monitor is None:
        _status_monitor = StatusMonitor()
    return _status_monitor
