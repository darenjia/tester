"""
日志工具模块
"""
import logging
import os
import sys
from datetime import datetime
from typing import List, Dict, Optional
from collections import deque


class MemoryLogHandler(logging.Handler):
    """内存日志处理器，用于在UI中显示最近日志"""
    
    def __init__(self, max_entries: int = 1000):
        super().__init__()
        self.max_entries = max_entries
        self.log_entries = deque(maxlen=max_entries)
        self.setFormatter(logging.Formatter(
            '%(asctime)s - %(levelname)s - %(message)s',
            datefmt='%Y-%m-%d %H:%M:%S'
        ))
    
    def emit(self, record):
        """添加日志记录"""
        try:
            log_entry = {
                'timestamp': datetime.fromtimestamp(record.created).strftime('%Y-%m-%d %H:%M:%S'),
                'level': record.levelname,
                'message': self.format(record),
                'levelno': record.levelno
            }
            self.log_entries.append(log_entry)
        except Exception:
            self.handleError(record)
    
    def get_logs(self, level: str = None, limit: int = None, search: str = None) -> List[Dict]:
        """
        获取日志条目
        
        Args:
            level: 日志级别过滤 (DEBUG, INFO, WARNING, ERROR, CRITICAL)
            limit: 返回条数限制
            search: 关键字搜索
            
        Returns:
            日志条目列表
        """
        logs = list(self.log_entries)
        
        # 级别过滤
        if level:
            level_upper = level.upper()
            logs = [log for log in logs if log['level'] == level_upper]
        
        # 关键字搜索
        if search:
            search_lower = search.lower()
            logs = [log for log in logs if search_lower in log['message'].lower()]
        
        # 限制条数
        if limit:
            logs = logs[-limit:]
        
        return logs
    
    def clear(self):
        """清空日志"""
        self.log_entries.clear()


class LoggerManager:
    """日志管理器"""
    
    _instance = None
    
    def __new__(cls):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
            cls._instance._initialized = False
        return cls._instance
    
    def __init__(self):
        if self._initialized:
            return
        
        self._initialized = True
        self.logger = logging.getLogger('TestExecutor')
        self.logger.setLevel(logging.INFO)
        self.memory_handler = None
        self.file_handler = None
        self.log_dir = None
    
    def setup(self, log_dir: str = 'logs', level: str = 'INFO', max_memory_entries: int = 1000):
        """
        设置日志系统
        
        Args:
            log_dir: 日志文件目录
            level: 日志级别
            max_memory_entries: 内存中保留的最大日志条数
        """
        self.log_dir = log_dir
        
        # 设置日志级别
        self.logger.setLevel(getattr(logging, level.upper(), logging.INFO))
        
        # 清除现有处理器
        self.logger.handlers.clear()
        
        # 创建内存处理器
        self.memory_handler = MemoryLogHandler(max_entries=max_memory_entries)
        self.logger.addHandler(self.memory_handler)
        
        # 创建文件处理器
        try:
            # 确保日志目录存在
            if not os.path.exists(log_dir):
                os.makedirs(log_dir)
            
            # 按日期命名日志文件
            log_file = os.path.join(log_dir, f'executor_{datetime.now().strftime("%Y%m%d")}.log')
            self.file_handler = logging.FileHandler(log_file, encoding='utf-8')
            self.file_handler.setFormatter(logging.Formatter(
                '%(asctime)s - %(name)s - %(levelname)s - %(message)s',
                datefmt='%Y-%m-%d %H:%M:%S'
            ))
            self.logger.addHandler(self.file_handler)
        except Exception as e:
            self.logger.error(f"创建日志文件失败: {e}")
        
        # 控制台处理器（开发调试用）
        if not getattr(sys, 'frozen', False):
            console_handler = logging.StreamHandler()
            console_handler.setFormatter(logging.Formatter(
                '%(asctime)s - %(levelname)s - %(message)s',
                datefmt='%H:%M:%S'
            ))
            self.logger.addHandler(console_handler)
    
    def get_logger(self) -> logging.Logger:
        """获取日志记录器"""
        return self.logger
    
    def get_memory_logs(self, level: str = None, limit: int = None, search: str = None) -> List[Dict]:
        """获取内存中的日志"""
        if self.memory_handler:
            return self.memory_handler.get_logs(level, limit, search)
        return []
    
    def clear_memory_logs(self):
        """清空内存日志"""
        if self.memory_handler:
            self.memory_handler.clear()
    
    def export_logs(self, output_path: str, level: str = None) -> bool:
        """
        导出日志到文件
        
        Args:
            output_path: 输出文件路径
            level: 日志级别过滤
            
        Returns:
            是否导出成功
        """
        try:
            logs = self.get_memory_logs(level=level)
            with open(output_path, 'w', encoding='utf-8') as f:
                for log in logs:
                    f.write(f"[{log['timestamp']}] {log['level']}: {log['message']}\n")
            return True
        except Exception as e:
            self.logger.error(f"导出日志失败: {e}")
            return False


# 全局日志管理器实例
logger_manager = LoggerManager()


def get_logger(name: str = None) -> logging.Logger:
    """获取日志记录器
    
    Args:
        name: 日志记录器名称（可选）
        
    Returns:
        logging.Logger: 日志记录器实例
    """
    return logger_manager.get_logger()


def setup_logging(log_dir: str = 'logs', level: str = 'INFO'):
    """初始化日志系统"""
    logger_manager.setup(log_dir, level)
