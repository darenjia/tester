"""
配置管理器 - 支持热更新
用于生产环境的动态配置管理
"""
import os
import json
import time
import threading
from typing import Dict, Any, Optional, Callable, List
from pathlib import Path
from dataclasses import dataclass, field

from utils.logger import get_logger

logger = get_logger("config_manager")

@dataclass
class ConfigChangeEvent:
    """配置变更事件"""
    key: str
    old_value: Any
    new_value: Any
    timestamp: float = field(default_factory=time.time)

class ConfigManager:
    """配置管理器"""
    
    def __init__(self, config_file: str = "config/executor_config.json"):
        self.config_file = config_file
        self._config: Dict[str, Any] = {}
        self._default_config: Dict[str, Any] = {}
        self._lock = threading.RLock()
        self._callbacks: List[Callable] = []
        self._watcher_thread = None
        self._running = False
        self._last_modified = 0
        
        # 加载默认配置
        self._load_default_config()
        
        # 加载配置文件
        self._load_config()
    
    def _load_default_config(self):
        """加载默认配置"""
        self._default_config = {
            "websocket": {
                "host": "0.0.0.0",
                "port": 8080,
                "heartbeat_interval": 30,
                "reconnect_interval": 5,
                "max_reconnect_attempts": 3
            },
            "logging": {
                "level": "INFO",
                "file": "logs/executor.log",
                "max_size": 10485760,
                "backup_count": 5
            },
            "task": {
                "timeout": 3600,
                "check_interval": 1,
                "max_concurrent": 1
            },
            "canoe": {
                "timeout": 30,
                "config_timeout": 60,
                "max_retries": 3,
                "retry_delay": 2.0
            },
            "tsmaster": {
                "timeout": 30,
                "config_timeout": 60,
                "max_retries": 3,
                "retry_delay": 2.0
            },
            "performance": {
                "monitor_enabled": True,
                "monitor_interval": 60,
                "metrics_retention_hours": 24
            },
            "security": {
                "validate_inputs": True,
                "max_config_file_size_mb": 100,
                "allowed_config_extensions": [".cfg", ".xml", ".json", ".dbc", ".ldf"]
            }
        }
    
    def _load_config(self):
        """加载配置文件"""
        try:
            if os.path.exists(self.config_file):
                with open(self.config_file, 'r', encoding='utf-8') as f:
                    user_config = json.load(f)
                
                # 合并配置
                with self._lock:
                    self._config = self._deep_merge(self._default_config.copy(), user_config)
                
                # 更新文件修改时间
                self._last_modified = os.path.getmtime(self.config_file)
                
                logger.info(f"配置文件加载成功: {self.config_file}")
            else:
                logger.warning(f"配置文件不存在，使用默认配置: {self.config_file}")
                with self._lock:
                    self._config = self._default_config.copy()
                
        except Exception as e:
            logger.error(f"加载配置文件失败: {e}")
            with self._lock:
                self._config = self._default_config.copy()
    
    def _deep_merge(self, base: Dict, override: Dict) -> Dict:
        """深度合并字典"""
        result = base.copy()
        for key, value in override.items():
            if key in result and isinstance(result[key], dict) and isinstance(value, dict):
                result[key] = self._deep_merge(result[key], value)
            else:
                result[key] = value
        return result
    
    def get(self, key: str, default: Any = None) -> Any:
        """
        获取配置值（支持点号分隔的键）
        
        Args:
            key: 配置键，如 "websocket.port"
            default: 默认值
            
        Returns:
            配置值
        """
        with self._lock:
            keys = key.split('.')
            value = self._config
            
            for k in keys:
                if isinstance(value, dict) and k in value:
                    value = value[k]
                else:
                    return default
            
            return value
    
    def set(self, key: str, value: Any, persist: bool = False):
        """
        设置配置值
        
        Args:
            key: 配置键
            value: 配置值
            persist: 是否持久化到文件
        """
        with self._lock:
            keys = key.split('.')
            config = self._config
            
            # 获取旧值
            old_value = self.get(key)
            
            # 设置新值
            for k in keys[:-1]:
                if k not in config:
                    config[k] = {}
                config = config[k]
            
            config[keys[-1]] = value
            
            # 触发变更事件
            if old_value != value:
                event = ConfigChangeEvent(key, old_value, value)
                self._notify_change(event)
        
        # 持久化
        if persist:
            self._save_config()
    
    def _save_config(self):
        """保存配置到文件"""
        try:
            with self._lock:
                config_to_save = self._config.copy()
            
            # 确保目录存在
            config_dir = os.path.dirname(self.config_file)
            if config_dir and not os.path.exists(config_dir):
                os.makedirs(config_dir)
            
            with open(self.config_file, 'w', encoding='utf-8') as f:
                json.dump(config_to_save, f, indent=4, ensure_ascii=False)
            
            # 更新修改时间
            self._last_modified = os.path.getmtime(self.config_file)
            
            logger.info(f"配置文件已保存: {self.config_file}")
            
        except Exception as e:
            logger.error(f"保存配置文件失败: {e}")
    
    def start_watcher(self, interval: float = 5.0):
        """启动配置文件监控"""
        if self._running:
            return
        
        self._running = True
        self._watcher_thread = threading.Thread(target=self._watcher_loop, args=(interval,))
        self._watcher_thread.daemon = True
        self._watcher_thread.start()
        
        logger.info(f"配置监控已启动，检查间隔: {interval}秒")
    
    def stop_watcher(self):
        """停止配置文件监控"""
        self._running = False
        if self._watcher_thread:
            self._watcher_thread.join(timeout=5)
        logger.info("配置监控已停止")
    
    def _watcher_loop(self, interval: float):
        """监控循环"""
        while self._running:
            try:
                if os.path.exists(self.config_file):
                    current_modified = os.path.getmtime(self.config_file)
                    
                    if current_modified > self._last_modified:
                        logger.info("检测到配置文件变更，正在重新加载...")
                        self._load_config()
                
                time.sleep(interval)
                
            except Exception as e:
                logger.error(f"配置监控异常: {e}")
                time.sleep(1)
    
    def register_change_callback(self, callback: Callable):
        """注册配置变更回调"""
        self._callbacks.append(callback)
    
    def _notify_change(self, event: ConfigChangeEvent):
        """通知配置变更"""
        for callback in self._callbacks:
            try:
                callback(event)
            except Exception as e:
                logger.error(f"配置变更回调失败: {e}")
    
    def reload(self):
        """手动重新加载配置"""
        logger.info("手动重新加载配置...")
        self._load_config()
    
    def get_all(self) -> Dict[str, Any]:
        """获取所有配置"""
        with self._lock:
            return self._config.copy()
    
    def reset_to_default(self):
        """重置为默认配置"""
        with self._lock:
            self._config = self._default_config.copy()
        
        self._save_config()
        logger.info("配置已重置为默认值")
    
    def validate_config(self) -> List[str]:
        """验证配置有效性"""
        errors = []
        
        # 验证WebSocket配置
        port = self.get('websocket.port')
        if not isinstance(port, int) or port < 1 or port > 65535:
            errors.append("websocket.port 必须是1-65535之间的整数")
        
        # 验证日志级别
        log_level = self.get('logging.level')
        valid_levels = ['DEBUG', 'INFO', 'WARNING', 'ERROR', 'CRITICAL']
        if log_level not in valid_levels:
            errors.append(f"logging.level 必须是以下之一: {valid_levels}")
        
        # 验证超时时间
        task_timeout = self.get('task.timeout')
        if not isinstance(task_timeout, int) or task_timeout <= 0:
            errors.append("task.timeout 必须是正整数")
        
        return errors

# 全局配置管理器实例
config_manager = ConfigManager()

# 便捷函数
def get_config(key: str, default: Any = None) -> Any:
    """获取配置值"""
    return config_manager.get(key, default)

def set_config(key: str, value: Any, persist: bool = False):
    """设置配置值"""
    config_manager.set(key, value, persist)

def reload_config():
    """重新加载配置"""
    config_manager.reload()