"""
性能指标收集和监控模块
用于生产环境的性能监控和告警
"""
import time
import threading
import statistics
from datetime import datetime, timedelta
from typing import Dict, Any, List, Optional, Callable
from collections import deque
from dataclasses import dataclass, field

from utils.logger import get_logger

logger = get_logger("metrics")

@dataclass
class MetricPoint:
    """指标数据点"""
    timestamp: float
    value: float
    labels: Dict[str, str] = field(default_factory=dict)

class MetricCollector:
    """指标收集器"""
    
    def __init__(self, max_history: int = 1000):
        self.max_history = max_history
        self.metrics: Dict[str, deque] = {}
        self._lock = threading.Lock()
    
    def record(self, metric_name: str, value: float, labels: Dict[str, str] = None):
        """记录指标"""
        with self._lock:
            if metric_name not in self.metrics:
                self.metrics[metric_name] = deque(maxlen=self.max_history)
            
            point = MetricPoint(
                timestamp=time.time(),
                value=value,
                labels=labels or {}
            )
            self.metrics[metric_name].append(point)
    
    def get_latest(self, metric_name: str) -> Optional[MetricPoint]:
        """获取最新指标值"""
        with self._lock:
            if metric_name in self.metrics and self.metrics[metric_name]:
                return self.metrics[metric_name][-1]
            return None
    
    def get_history(self, metric_name: str, duration: timedelta = None) -> List[MetricPoint]:
        """获取历史指标"""
        with self._lock:
            if metric_name not in self.metrics:
                return []
            
            points = list(self.metrics[metric_name])
            
            if duration:
                cutoff_time = time.time() - duration.total_seconds()
                points = [p for p in points if p.timestamp >= cutoff_time]
            
            return points
    
    def get_statistics(self, metric_name: str, duration: timedelta = None) -> Dict[str, float]:
        """获取统计信息"""
        points = self.get_history(metric_name, duration)
        
        if not points:
            return {}
        
        values = [p.value for p in points]
        
        return {
            'count': len(values),
            'min': min(values),
            'max': max(values),
            'mean': statistics.mean(values),
            'median': statistics.median(values),
            'stdev': statistics.stdev(values) if len(values) > 1 else 0
        }
    
    def get_all_metrics(self) -> Dict[str, Dict[str, Any]]:
        """获取所有指标信息"""
        with self._lock:
            result = {}
            for metric_name in self.metrics:
                points = list(self.metrics[metric_name])
                if points:
                    latest = points[-1]
                    values = [p.value for p in points]
                    result[metric_name] = {
                        'latest': {
                            'value': latest.value,
                            'timestamp': latest.timestamp,
                            'labels': latest.labels
                        },
                        'statistics': {
                            'count': len(values),
                            'min': min(values),
                            'max': max(values),
                            'mean': statistics.mean(values),
                            'median': statistics.median(values),
                            'stdev': statistics.stdev(values) if len(values) > 1 else 0
                        }
                    }
                else:
                    result[metric_name] = {
                        'latest': None,
                        'statistics': {}
                    }
            return result

class PerformanceMonitor:
    """性能监控器"""
    
    def __init__(self):
        self.collector = MetricCollector()
        self._running = False
        self._monitor_thread = None
        self._callbacks: List[Callable] = []
        self._alert_thresholds: Dict[str, float] = {}
    
    def start(self, interval: float = 60.0):
        """启动监控"""
        if self._running:
            return
        
        self._running = True
        self._monitor_thread = threading.Thread(target=self._monitor_loop, args=(interval,))
        self._monitor_thread.daemon = True
        self._monitor_thread.start()
        
        logger.info(f"性能监控器已启动，监控间隔: {interval}秒")
    
    def stop(self):
        """停止监控"""
        self._running = False
        if self._monitor_thread:
            self._monitor_thread.join(timeout=5)
        logger.info("性能监控器已停止")
    
    def _monitor_loop(self, interval: float):
        """监控循环"""
        while self._running:
            try:
                self._collect_system_metrics()
                self._check_alerts()
                time.sleep(interval)
            except Exception as e:
                logger.error(f"监控循环异常: {e}")
                time.sleep(1)
    
    def _collect_system_metrics(self):
        """收集系统指标"""
        try:
            import psutil
            
            # CPU使用率
            cpu_percent = psutil.cpu_percent(interval=1)
            self.collector.record('system.cpu_percent', cpu_percent)
            
            # 内存使用
            memory = psutil.virtual_memory()
            self.collector.record('system.memory_percent', memory.percent)
            self.collector.record('system.memory_used_mb', memory.used / 1024 / 1024)
            
            # 磁盘使用
            disk = psutil.disk_usage('/')
            self.collector.record('system.disk_percent', disk.percent)
            
            # 网络IO
            net_io = psutil.net_io_counters()
            self.collector.record('system.net_sent_mb', net_io.bytes_sent / 1024 / 1024)
            self.collector.record('system.net_recv_mb', net_io.bytes_recv / 1024 / 1024)
            
        except ImportError:
            logger.warning("psutil未安装，无法收集系统指标")
        except Exception as e:
            logger.error(f"收集系统指标失败: {e}")
    
    def _check_alerts(self):
        """检查告警阈值"""
        for metric_name, threshold in self._alert_thresholds.items():
            latest = self.collector.get_latest(metric_name)
            if latest and latest.value > threshold:
                self._trigger_alert(metric_name, latest.value, threshold)
    
    def _trigger_alert(self, metric_name: str, value: float, threshold: float):
        """触发告警"""
        alert_msg = f"告警: {metric_name} = {value:.2f}，超过阈值 {threshold:.2f}"
        logger.warning(alert_msg)
        
        # 调用告警回调
        for callback in self._callbacks:
            try:
                callback(metric_name, value, threshold)
            except Exception as e:
                logger.error(f"告警回调失败: {e}")
    
    def set_alert_threshold(self, metric_name: str, threshold: float):
        """设置告警阈值"""
        self._alert_thresholds[metric_name] = threshold
        logger.info(f"设置告警阈值: {metric_name} = {threshold}")
    
    def register_alert_callback(self, callback: Callable):
        """注册告警回调"""
        self._callbacks.append(callback)
    
    def record_task_metric(self, task_id: str, metric_name: str, value: float):
        """记录任务指标"""
        self.collector.record(
            f"task.{metric_name}",
            value,
            labels={'task_id': task_id}
        )
    
    def get_metrics_report(self) -> Dict[str, Any]:
        """获取指标报告"""
        return {
            'timestamp': datetime.now().isoformat(),
            'metrics': self.collector.get_all_metrics(),
            'alert_thresholds': self._alert_thresholds
        }

class TaskMetrics:
    """任务指标收集器"""
    
    def __init__(self, task_id: str):
        self.task_id = task_id
        self.start_time = time.time()
        self.end_time = None
        self.step_times: Dict[str, float] = {}
        self.error_count = 0
        self.retry_count = 0
    
    def record_step(self, step_name: str, duration: float):
        """记录步骤耗时"""
        self.step_times[step_name] = duration
        logger.debug(f"任务 {self.task_id} 步骤 {step_name} 耗时: {duration:.2f}秒")
    
    def record_error(self):
        """记录错误"""
        self.error_count += 1
    
    def record_retry(self):
        """记录重试"""
        self.retry_count += 1
    
    def finalize(self):
        """完成任务指标收集"""
        self.end_time = time.time()
    
    def get_summary(self) -> Dict[str, Any]:
        """获取指标摘要"""
        duration = (self.end_time or time.time()) - self.start_time
        
        return {
            'task_id': self.task_id,
            'duration': duration,
            'step_times': self.step_times,
            'error_count': self.error_count,
            'retry_count': self.retry_count,
            'start_time': datetime.fromtimestamp(self.start_time).isoformat(),
            'end_time': datetime.fromtimestamp(self.end_time).isoformat() if self.end_time else None
        }

# 全局性能监控器实例
performance_monitor = PerformanceMonitor()

# 全局指标收集器实例
metric_collector = MetricCollector()

def record_metric(metric_name: str, value: float, labels: Dict[str, str] = None):
    """记录指标的全局函数"""
    metric_collector.record(metric_name, value, labels)

def get_metric_stats(metric_name: str) -> Dict[str, float]:
    """获取指标统计的全局函数"""
    return metric_collector.get_statistics(metric_name)

def timeit(metric_name: str, labels: Dict[str, str] = None):
    """耗时统计装饰器"""
    def decorator(func):
        def wrapper(*args, **kwargs):
            start_time = time.time()
            try:
                result = func(*args, **kwargs)
                return result
            finally:
                duration = time.time() - start_time
                record_metric(metric_name, duration, labels)
        return wrapper
    return decorator