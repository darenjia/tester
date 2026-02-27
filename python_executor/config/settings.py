"""
配置文件管理模块
"""
import os
import json
from typing import Dict, Any

class Settings:
    """配置管理类"""
    
    def __init__(self):
        # WebSocket配置
        self.websocket_host = "0.0.0.0"
        self.websocket_port = 8080
        self.heartbeat_interval = 30  # 秒
        self.reconnect_interval = 5   # 秒
        self.max_reconnect_attempts = 3
        
        # 日志配置
        self.log_level = "INFO"
        self.log_file = "logs/executor.log"
        self.log_max_size = 10 * 1024 * 1024  # 10MB
        self.log_backup_count = 5
        
        # 任务执行配置
        self.task_timeout = 3600  # 秒
        self.task_check_interval = 1  # 秒
        self.max_concurrent_tasks = 1  # 串行执行
        
        # CANoe配置
        self.canoe_timeout = 30  # 连接超时
        self.canoe_config_timeout = 60  # 配置加载超时
        
        # TSMaster配置
        self.tsmaster_timeout = 30  # 连接超时
        self.tsmaster_config_timeout = 60  # 配置加载超时
        
        # 结果上报配置
        self.result_batch_size = 10
        self.result_upload_interval = 5  # 秒
        
        # 从配置文件加载自定义设置
        self._load_from_file()
    
    def _load_from_file(self):
        """从配置文件加载设置"""
        config_file = "config/executor_config.json"
        if os.path.exists(config_file):
            try:
                with open(config_file, 'r', encoding='utf-8') as f:
                    config = json.load(f)
                    self._update_config(config)
            except Exception as e:
                print(f"加载配置文件失败: {e}")
    
    def _update_config(self, config: Dict[str, Any]):
        """更新配置"""
        for key, value in config.items():
            if hasattr(self, key):
                setattr(self, key, value)
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        return {
            "websocket": {
                "host": self.websocket_host,
                "port": self.websocket_port,
                "heartbeat_interval": self.heartbeat_interval,
                "reconnect_interval": self.reconnect_interval,
                "max_reconnect_attempts": self.max_reconnect_attempts
            },
            "logging": {
                "level": self.log_level,
                "file": self.log_file,
                "max_size": self.log_max_size,
                "backup_count": self.log_backup_count
            },
            "task": {
                "timeout": self.task_timeout,
                "check_interval": self.task_check_interval,
                "max_concurrent": self.max_concurrent_tasks
            },
            "canoe": {
                "timeout": self.canoe_timeout,
                "config_timeout": self.canoe_config_timeout
            },
            "tsmaster": {
                "timeout": self.tsmaster_timeout,
                "config_timeout": self.tsmaster_config_timeout
            },
            "result": {
                "batch_size": self.result_batch_size,
                "upload_interval": self.result_upload_interval
            }
        }

# 全局配置实例
settings = Settings()