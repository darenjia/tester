"""
配置管理模块
"""
import json
import os
import sys
from typing import Dict, Any, Optional


class ConfigManager:
    """配置管理器"""
    
    # 默认配置
    DEFAULT_CONFIG = {
        "server": {
            "websocket_url": "ws://localhost:8080/ws/executor",
            "heartbeat_interval": 30,
            "reconnect_interval": 5,
            "max_reconnect_attempts": 10
        },
        "http": {
            "port": 8180,
            "host": "0.0.0.0",
            "debug": False
        },
        "websocket": {
            "enabled": False,
            "port": 8080,
            "host": "0.0.0.0"
        },
        "device": {
            "device_id": "DEVICE_001",
            "device_name": "测试执行设备-01",
            "location": "实验室"
        },
        "software": {
            "canoe_path": "C:\\Program Files\\Vector\\CANoe 17\\Exec64\\CANoe64.exe",
            "tsmaster_path": "C:\\Program Files\\TSMaster",
            "ttman_path": "C:\\Spirent\\TTman.bat",
            "workspace_path": "C:\\TestWorkspace"
        },
        "logging": {
            "level": "INFO",
            "log_dir": "logs",
            "max_log_files": 10,
            "max_log_size_mb": 100
        },
        "execution": {
            "default_timeout": 3600,
            "auto_start": True,
            "keep_alive": True
        },
        "report": {
            "enabled": False,
            "api_url": "",
            "file_upload_url": "",
            "timeout": 30,
            "max_retries": 3,
            "retry_delay": 2.0,
            "headers": {}
        },
        "report_server": {
            "enabled": False,
            "host": "",
            "port": 8080,
            "path": "/api/report",
            "upload_report": False,
            "timeout": 30,
            "retry_count": 3
        },
        "config_cache": {
            "enabled": True,
            "cache_dir": "workspace/cache/configs",
            "max_cache_count": 50,
            "cache_ttl_hours": 168,
            "auto_cleanup": True
        }
    }
    
    def __init__(self, config_path: str = None):
        """
        初始化配置管理器
        
        Args:
            config_path: 配置文件路径，默认为程序目录下的 config.json
        """
        if config_path is None:
            # 获取程序所在目录
            if getattr(sys, 'frozen', False):
                # 打包后的exe运行
                base_dir = os.path.dirname(sys.executable)
            else:
                # 开发环境运行
                base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
            config_path = os.path.join(base_dir, "config.json")
        
        self.config_path = config_path
        self._config = {}
        self._load_config()
    
    def _load_config(self):
        """加载配置文件"""
        if os.path.exists(self.config_path):
            try:
                with open(self.config_path, 'r', encoding='utf-8') as f:
                    loaded_config = json.load(f)
                    # 合并配置，确保所有默认配置项都存在
                    self._config = self._merge_config(self.DEFAULT_CONFIG, loaded_config)
            except Exception as e:
                print(f"加载配置文件失败: {e}，使用默认配置")
                self._config = self.DEFAULT_CONFIG.copy()
                self.save_config()
        else:
            # 配置文件不存在，创建默认配置
            self._config = self.DEFAULT_CONFIG.copy()
            self.save_config()
    
    def _merge_config(self, default: Dict, loaded: Dict) -> Dict:
        """
        递归合并配置
        
        Args:
            default: 默认配置
            loaded: 已加载的配置
            
        Returns:
            合并后的配置
        """
        result = default.copy()
        for key, value in loaded.items():
            if key in result and isinstance(result[key], dict) and isinstance(value, dict):
                result[key] = self._merge_config(result[key], value)
            else:
                result[key] = value
        return result
    
    def save_config(self) -> bool:
        """
        保存配置到文件
        
        Returns:
            是否保存成功
        """
        try:
            # 确保目录存在
            config_dir = os.path.dirname(self.config_path)
            if config_dir and not os.path.exists(config_dir):
                os.makedirs(config_dir)
            
            with open(self.config_path, 'w', encoding='utf-8') as f:
                json.dump(self._config, f, ensure_ascii=False, indent=2)
            return True
        except Exception as e:
            print(f"保存配置文件失败: {e}")
            return False
    
    def get(self, key: str, default: Any = None) -> Any:
        """
        获取配置项
        
        Args:
            key: 配置键，支持点号分隔（如 "server.websocket_url"）
            default: 默认值
            
        Returns:
            配置值
        """
        keys = key.split('.')
        value = self._config
        for k in keys:
            if isinstance(value, dict) and k in value:
                value = value[k]
            else:
                return default
        return value
    
    def set(self, key: str, value: Any) -> bool:
        """
        设置配置项
        
        Args:
            key: 配置键，支持点号分隔
            value: 配置值
            
        Returns:
            是否设置成功
        """
        keys = key.split('.')
        config = self._config
        for k in keys[:-1]:
            if k not in config:
                config[k] = {}
            config = config[k]
        config[keys[-1]] = value
        return self.save_config()
    
    def get_all(self) -> Dict[str, Any]:
        """
        获取所有配置
        
        Returns:
            完整配置字典
        """
        return self._config.copy()
    
    def update(self, config: Dict[str, Any]) -> bool:
        """
        批量更新配置
        
        Args:
            config: 新的配置字典
            
        Returns:
            是否更新成功
        """
        self._config = self._merge_config(self._config, config)
        return self.save_config()
    
    def reset_to_default(self) -> bool:
        """
        重置为默认配置
        
        Returns:
            是否重置成功
        """
        self._config = self.DEFAULT_CONFIG.copy()
        return self.save_config()


# 全局配置实例
_config_instance: Optional[ConfigManager] = None


def get_config(config_path: str = None) -> ConfigManager:
    """
    获取配置管理器实例（单例模式）
    
    Args:
        config_path: 配置文件路径
        
    Returns:
        ConfigManager 实例
    """
    global _config_instance
    if _config_instance is None:
        _config_instance = ConfigManager(config_path)
    return _config_instance


# 全局设置实例（兼容旧代码）
settings = get_config()
