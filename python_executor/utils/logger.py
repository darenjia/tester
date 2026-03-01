"""
统一日志工具模块
"""
import logging
import os
import sys
from logging.handlers import RotatingFileHandler
from datetime import datetime
from config.settings import settings

def setup_logger(name: str = "executor") -> logging.Logger:
    """
    设置日志器
    
    Args:
        name: 日志器名称
        
    Returns:
        配置好的日志器
    """
    # 创建日志器
    logger = logging.getLogger(name)
    
    # 如果已经配置过，直接返回
    if logger.handlers:
        return logger
    
    # 设置日志级别
    log_level = getattr(logging, settings.log_level.upper(), logging.INFO)
    logger.setLevel(log_level)
    
    # 获取项目根目录并构建绝对路径
    base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    log_file_path = os.path.join(base_dir, settings.log_file)
    
    # 创建日志目录
    log_dir = os.path.dirname(log_file_path)
    if log_dir and not os.path.exists(log_dir):
        os.makedirs(log_dir)
    
    # 创建格式化器
    formatter = logging.Formatter(
        fmt='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S'
    )
    
    # 文件处理器（轮转日志）
    try:
        file_handler = RotatingFileHandler(
            filename=log_file_path,
            maxBytes=settings.log_max_size,
            backupCount=settings.log_backup_count,
            encoding='utf-8'
        )
        file_handler.setLevel(log_level)
        file_handler.setFormatter(formatter)
        logger.addHandler(file_handler)
    except Exception as e:
        print(f"警告: 无法创建日志文件: {e}")
    
    # 控制台处理器
    console_handler = logging.StreamHandler(sys.stdout)
    console_handler.setLevel(log_level)
    console_handler.setFormatter(formatter)
    
    # 添加处理器到日志器
    logger.addHandler(console_handler)
    
    return logger

def get_logger(name: str = None) -> logging.Logger:
    """
    获取日志器
    
    Args:
        name: 模块名称，如果为None则返回根日志器
        
    Returns:
        日志器实例
    """
    if name is None:
        return logging.getLogger("executor")
    return logging.getLogger(f"executor.{name}")

class LogContext:
    """日志上下文管理器"""
    
    def __init__(self, task_id: str = None, device_id: str = None):
        self.task_id = task_id
        self.device_id = device_id
        self.original_formatters = []
    
    def __enter__(self):
        """进入上下文"""
        # 保存原始格式化器
        for handler in logging.getLogger().handlers:
            self.original_formatters.append(handler.formatter)
            
        # 创建新的格式化器，添加上下文信息
        context_formatter = logging.Formatter(
            fmt=f'%(asctime)s - %(name)s - %(levelname)s - [task:{self.task_id}] [device:{self.device_id}] - %(message)s',
            datefmt='%Y-%m-%d %H:%M:%S'
        )
        
        # 应用新的格式化器
        for handler in logging.getLogger().handlers:
            handler.setFormatter(context_formatter)
            
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        """退出上下文"""
        # 恢复原始格式化器
        for i, handler in enumerate(logging.getLogger().handlers):
            if i < len(self.original_formatters):
                handler.setFormatter(self.original_formatters[i])

# 初始化日志器
logger = setup_logger()