"""
测试配置管理器
用于准备和管理测试配置文件（cfg和ini）
"""
import os
import sys
import json
import shutil
import threading
import configparser
from typing import Dict, Any, Optional, Callable, List
from dataclasses import dataclass, field
from datetime import datetime

# 添加父目录到路径
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from config.settings import get_config
from core.config_cache_manager import ConfigCacheManager, get_config_cache_manager
from utils.logger import get_logger

logger = get_logger("config_manager")


class TestConfigManager:
    """
    测试配置管理器
    用于准备和管理测试配置文件（cfg和ini）
    支持配置缓存，提高加载效率
    """
    __test__ = False

    def __init__(self, base_config_dir: str = None, use_cache: bool = True):
        """
        初始化测试配置管理器

        Args:
            base_config_dir: 基础配置目录
            use_cache: 是否使用缓存
        """
        self.base_config_dir = base_config_dir or r'D:\TAMS\DTTC_CONFIG'
        self._current_cfg_path: Optional[str] = None
        self._current_ini_path: Optional[str] = None
        self._use_cache = use_cache
        self._cache_manager: Optional[ConfigCacheManager] = None

        # 初始化缓存管理器
        if self._use_cache:
            try:
                self._cache_manager = get_config_cache_manager()
            except Exception as e:
                logger.warning(f"初始化缓存管理器失败: {e}，将不使用缓存")
                self._use_cache = False

    def prepare_config_for_task(self, task_config_name: str, test_cases: List[Dict], variables: Dict[str, Any] = None) -> Dict[str, str]:
        """
        为任务准备配置文件

        Args:
            task_config_name: 任务配置名称
            test_cases: 测试用例列表（未使用，保留兼容性）
            variables: 变量字典（未使用，保留兼容性）

        Returns:
            包含cfg_path和ini_path的字典
        """
        source_cfg_path = self._find_cfg_file_in_source(task_config_name)
        if not source_cfg_path:
            raise FileNotFoundError(f"未找到配置: {task_config_name}")

        cfg_path = source_cfg_path
        self._current_cfg_path = cfg_path

        cfg_dir = os.path.dirname(cfg_path)
        ini_path = os.path.join(cfg_dir, "SelectInfo.ini")

        if os.path.exists(ini_path):
            self._current_ini_path = ini_path
        else:
            self._current_ini_path = None

        return {
            'cfg_path': cfg_path,
            'ini_path': ini_path if self._current_ini_path else None
        }

    def _find_cfg_file_in_source(self, config_name: str) -> Optional[str]:
        """
        在源目录中查找cfg文件

        Args:
            config_name: 配置名称

        Returns:
            cfg文件路径，未找到返回None
        """
        # 直接路径
        if os.path.isfile(config_name) and config_name.endswith('.cfg'):
            return config_name

        # 在基础目录中查找
        if self.base_config_dir and os.path.isdir(self.base_config_dir):
            cfg_path = os.path.join(self.base_config_dir, f"{config_name}.cfg")
            if os.path.isfile(cfg_path):
                return cfg_path

            # 递归查找
            for root, dirs, files in os.walk(self.base_config_dir):
                if f"{config_name}.cfg" in files:
                    return os.path.join(root, f"{config_name}.cfg")

        return None

    def _find_cfg_file(self, config_name: str) -> Optional[str]:
        """
        查找cfg文件（优先从缓存查找）

        Args:
            config_name: 配置名称

        Returns:
            cfg文件路径，未找到返回None
        """
        # 如果启用了缓存，优先从缓存获取
        if self._use_cache and self._cache_manager and self._cache_manager.enabled:
            cached = self._cache_manager.get_cached_config(config_name)
            if cached:
                return cached['cfg_path']

        # 从源目录查找
        return self._find_cfg_file_in_source(config_name)

    def _generate_ini_files(self, config_name: str, test_cases: List[Dict], variables: Dict[str, Any] = None) -> Dict[str, str]:
        """
        生成CANoe规范的ini配置文件
        生成两个文件：
        1. SelectInfo.ini - 测试用例选择配置
        2. ParaInfo.ini - 测试参数配置

        Args:
            config_name: 配置名称
            test_cases: 测试用例列表
            variables: 变量字典

        Returns:
            包含两个ini文件路径的字典 {'select_info': path, 'para_info': path}
        """
        # 确定ini文件输出目录
        if self._use_cache and self._cache_manager and self._cache_manager.enabled:
            # 使用缓存目录
            output_dir = self._cache_manager.get_cache_path(config_name)
            os.makedirs(output_dir, exist_ok=True)
        else:
            # 使用generated目录
            output_dir = os.path.join(self.base_config_dir, 'generated')
            os.makedirs(output_dir, exist_ok=True)

        # 生成SelectInfo.ini - 用例选择配置
        select_info_path = os.path.join(output_dir, "SelectInfo.ini")
        self._generate_select_info_ini(select_info_path, test_cases)

        # 生成ParaInfo.ini - 参数配置
        para_info_path = os.path.join(output_dir, "ParaInfo.ini")
        self._generate_para_info_ini(para_info_path, variables)

        # 更新缓存中的ini信息
        if self._use_cache and self._cache_manager:
            try:
                self._cache_manager.update_ini_info(config_name, select_info_path)
            except Exception as e:
                logger.debug(f"更新ini缓存信息失败: {e}")

        return {
            'select_info': select_info_path,
            'para_info': para_info_path
        }

    def _generate_select_info_ini(self, file_path: str, test_cases: List[Dict] = None):
        """
        生成SelectInfo.ini文件（CANoe规范）
        格式：
        [CFG_PARA]
        TG1_TC04_SC01=1
        TG1_TC05_SC01=1

        使用test_cases中的caseNo字段生成。

        Args:
            file_path: 输出文件路径
            test_cases: 测试用例列表（字典）
        """
        lines = ['[CFG_PARA]', '']

        if test_cases:
            # 使用test_cases生成
            for case in test_cases:
                # 获取用例标识（支持多种字段名）
                case_id = case.get('caseNo') or case.get('case_id') or case.get('name') or case.get('caseName')
                if case_id:
                    # CANoe格式：用例标识=1（表示启用）
                    lines.append(f"{case_id}=1")
        else:
            # 如果没有用例，添加一个默认条目
            lines.append("; 未配置测试用例")

        # 写入文件
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write('\n'.join(lines))

        logger.debug(f"生成SelectInfo.ini: {file_path}")

    def generate_select_info_ini(self, case_nos: List[str], output_dir: str) -> str:
        """
        生成 SelectInfo.ini 文件（公开接口）

        Args:
            case_nos: 用例编号列表，如 ["CANOE-001", "CANOE-002"]
            output_dir: 输出目录（cfg 文件所在目录）

        Returns:
            SelectInfo.ini 文件路径
        """
        select_info_path = os.path.join(output_dir, "SelectInfo.ini")
        self._generate_select_info_ini(select_info_path, [{"caseNo": c} for c in case_nos])
        return select_info_path

    def _generate_para_info_ini(self, file_path: str, variables: Dict[str, Any] = None,
                                default_config: str = ""):
        """
        生成ParaInfo.ini文件（CANoe规范）
        格式：
        [CFG_PARA]
        ECUName=MTCU1
        Terminal=1
        ...

        Args:
            file_path: 输出文件路径
            variables: 执行时传入的参数（优先级高，覆盖默认值）
            default_config: 用例映射中的默认参数（原始INI格式）
        """
        # 1. 解析默认配置（INI格式，保留大小写）
        merged_vars = {}
        if default_config:
            try:
                # 使用自定义解析保留大小写
                config = configparser.RawConfigParser()
                config.optionxform = str  # 保留key的原始大小写
                config.read_string(default_config)
                if config.has_section('CFG_PARA'):
                    for key in config.options('CFG_PARA'):
                        merged_vars[key] = config.get('CFG_PARA', key)
            except Exception as e:
                logger.warning(f"默认参数配置INI解析失败: {e}")

        # 2. 合并执行时参数（覆盖默认值）
        if variables:
            merged_vars.update(variables)

        # 3. 生成INI内容
        lines = ['[CFG_PARA]', '']

        if merged_vars:
            for key, value in merged_vars.items():
                # 处理不同类型的值
                if value is None:
                    value_str = ""
                elif isinstance(value, bool):
                    value_str = "1" if value else "0"
                else:
                    value_str = str(value)
                lines.append(f"{key}={value_str}")
        else:
            # 如果没有变量，添加注释
            lines.append("; 未配置参数")

        # 写入文件
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write('\n'.join(lines))

        logger.debug(f"生成ParaInfo.ini: {file_path}")

    def _generate_ini_file(self, config_name: str, test_cases: List[Dict], variables: Dict[str, Any] = None) -> str:
        """
        生成ini配置文件（兼容旧接口，返回SelectInfo.ini路径）

        Args:
            config_name: 配置名称
            test_cases: 测试用例列表
            variables: 变量字典

        Returns:
            SelectInfo.ini文件路径
        """
        ini_files = self._generate_ini_files(config_name, test_cases, variables)
        return ini_files['select_info']

    def get_current_cfg_path(self) -> Optional[str]:
        """获取当前cfg文件路径"""
        return self._current_cfg_path

    def get_current_ini_path(self) -> Optional[str]:
        """获取当前ini文件路径"""
        return self._current_ini_path

    def get_cache_stats(self) -> Dict[str, Any]:
        """获取缓存统计信息"""
        if self._cache_manager:
            return self._cache_manager.get_cache_stats()
        return {"error": "缓存管理器未初始化"}

    def clear_cache(self):
        """清空配置缓存"""
        if self._cache_manager:
            self._cache_manager.clear_all_cache()

    def prepare_config_for_case(self, case_no: str, script_path: str, ini_config: str,
                                variables: Dict[str, Any] = None,
                                para_config: str = "") -> Dict[str, str]:
        """
        为单个用例准备配置文件

        Args:
            case_no: 用例编号
            script_path: cfg工程文件路径
            ini_config: SelectInfo.ini配置内容（原始INI格式）
            variables: 属性参数字典（从任务variables或用例params获取，优先级高）
            para_config: ParaInfo.ini默认参数（从用例映射获取，JSON格式）

        Returns:
            包含cfg_path和ini_path的字典
        """
        if not script_path:
            raise ValueError(f"用例 {case_no} 未配置script_path")

        if not os.path.exists(script_path):
            raise FileNotFoundError(f"用例 {case_no} 的cfg文件不存在: {script_path}")

        # 直接使用script_path，无需缓存
        cfg_path = script_path

        ini_files = self._generate_ini_from_case_config(case_no, ini_config, variables, para_config)

        self._current_cfg_path = cfg_path
        self._current_ini_path = ini_files['para_info']

        return {
            'cfg_path': cfg_path,
            'ini_path': ini_files['para_info'],
            'select_info_path': ini_files.get('select_info', ''),
            'para_info_path': ini_files['para_info']
        }

    def _generate_ini_from_case_config(self, case_no: str, ini_config: str,
                                        variables: Dict[str, Any] = None,
                                        para_config: str = "") -> Dict[str, str]:
        """
        从用例映射生成ini文件

        ini 文件将写入到当前 cfg 文件所在目录，以便 CANoe 能够读取

        Args:
            case_no: 用例编号
            ini_config: SelectInfo.ini配置内容（从用例映射获取）
            variables: 属性参数字典（从任务variables或用例params获取，优先级高）
            para_config: ParaInfo.ini默认参数（从用例映射获取，JSON格式）

        Returns:
            包含ini文件路径的字典
        """
        # ini 文件应该写入到 cfg 文件所在目录
        if self._current_cfg_path and os.path.exists(self._current_cfg_path):
            cfg_dir = os.path.dirname(self._current_cfg_path)
        else:
            # 回退到 generated 目录
            cfg_dir = os.path.join(self.base_config_dir, 'generated', case_no)
            os.makedirs(cfg_dir, exist_ok=True)

        select_info_path = os.path.join(cfg_dir, "SelectInfo.ini")
        para_info_path = os.path.join(cfg_dir, "ParaInfo.ini")

        # SelectInfo.ini - 用例选择配置（从ini_config获取）
        if ini_config:
            with open(select_info_path, 'w', encoding='utf-8') as f:
                f.write(ini_config)
            logger.info(f"写入 SelectInfo.ini: {select_info_path}")
        else:
            with open(select_info_path, 'w', encoding='utf-8') as f:
                f.write(f"[CFG_PARA]\n{case_no}=1\n")
            logger.info(f"写入默认 SelectInfo.ini: {select_info_path}")

        # ParaInfo.ini - 属性参数配置（合并默认参数和执行时参数）
        self._generate_para_info_ini(para_info_path, variables, para_config)
        logger.info(f"写入 ParaInfo.ini: {para_info_path}")

        logger.debug(f"为用例 {case_no} 生成ini文件到目录: {cfg_dir}")

        return {
            'select_info': select_info_path,
            'para_info': para_info_path
        }


class RuntimeConfigManager:
    """
    运行时配置管理器

    管理应用的运行时配置，支持：
    - 动态配置更新
    - 配置变更通知
    - 配置导入导出
    - 配置验证
    """

    def __init__(self):
        """初始化运行时配置管理器"""
        self._config = get_config()
        self._lock = threading.Lock()
        self._listeners: Dict[str, List[Callable]] = {}
        self._global_listeners: List[Callable] = []

    def get(self, key: str, default: Any = None) -> Any:
        """
        获取配置项

        Args:
            key: 配置键，支持点号分隔
            default: 默认值

        Returns:
            配置值
        """
        return self._config.get(key, default)

    def set(self, key: str, value: Any, notify: bool = True) -> bool:
        """
        设置配置项

        Args:
            key: 配置键
            value: 配置值
            notify: 是否通知监听器

        Returns:
            是否设置成功
        """
        with self._lock:
            old_value = self.get(key)

            if self._config.set(key, value):
                if notify:
                    self._notify_change(key, old_value, value)
                return True
            return False

    def get_http_config(self) -> Dict[str, Any]:
        """获取HTTP服务配置"""
        return {
            "port": self.get("http.port", 8180),
            "host": self.get("http.host", "0.0.0.0"),
            "debug": self.get("http.debug", False)
        }

    def set_http_config(self, config: Dict[str, Any]) -> bool:
        """
        设置HTTP服务配置

        Args:
            config: HTTP配置字典

        Returns:
            是否设置成功
        """
        with self._lock:
            old_port = self.get("http.port")
            old_host = self.get("http.host")
            old_debug = self.get("http.debug")

            success = True
            if "port" in config:
                success = success and self._config.set("http.port", config["port"])
            if "host" in config:
                success = success and self._config.set("http.host", config["host"])
            if "debug" in config:
                success = success and self._config.set("http.debug", config["debug"])

            if success:
                self._notify_change("http", {
                    "port": old_port,
                    "host": old_host,
                    "debug": old_debug
                }, self.get_http_config())

            return success

    def get_websocket_config(self) -> Dict[str, Any]:
        """获取WebSocket服务配置"""
        return {
            "enabled": self.get("websocket.enabled", False),
            "port": self.get("websocket.port", 8080),
            "host": self.get("websocket.host", "0.0.0.0")
        }

    def set_websocket_config(self, config: Dict[str, Any]) -> bool:
        """
        设置WebSocket服务配置

        Args:
            config: WebSocket配置字典

        Returns:
            是否设置成功
        """
        with self._lock:
            old_config = self.get_websocket_config()

            success = True
            if "enabled" in config:
                success = success and self._config.set("websocket.enabled", config["enabled"])
            if "port" in config:
                success = success and self._config.set("websocket.port", config["port"])
            if "host" in config:
                success = success and self._config.set("websocket.host", config["host"])

            if success:
                self._notify_change("websocket", old_config, self.get_websocket_config())

            return success

    def get_all_config(self) -> Dict[str, Any]:
        """获取所有配置"""
        return self._config.get_all()

    def update_config(self, config: Dict[str, Any]) -> bool:
        """
        批量更新配置

        Args:
            config: 配置字典

        Returns:
            是否更新成功
        """
        with self._lock:
            old_config = self.get_all_config()

            if self._config.update(config):
                for key in config:
                    old_value = self._get_nested_value(old_config, key)
                    new_value = self._get_nested_value(config, key)
                    if old_value != new_value:
                        self._notify_change(key, old_value, new_value)
                return True
            return False

    def _get_nested_value(self, config: Dict, key: str) -> Any:
        """获取嵌套配置值"""
        keys = key.split('.')
        value = config
        for k in keys:
            if isinstance(value, dict) and k in value:
                value = value[k]
            else:
                return None
        return value

    def add_listener(self, key: str, callback: Callable):
        """添加配置变更监听器"""
        with self._lock:
            if key not in self._listeners:
                self._listeners[key] = []
            self._listeners[key].append(callback)

    def remove_listener(self, key: str, callback: Callable):
        """移除配置变更监听器"""
        with self._lock:
            if key in self._listeners and callback in self._listeners[key]:
                self._listeners[key].remove(callback)

    def add_global_listener(self, callback: Callable):
        """添加全局配置变更监听器"""
        with self._lock:
            self._global_listeners.append(callback)

    def remove_global_listener(self, callback: Callable):
        """移除全局配置变更监听器"""
        with self._lock:
            if callback in self._global_listeners:
                self._global_listeners.remove(callback)

    def _notify_change(self, key: str, old_value: Any, new_value: Any):
        """通知配置变更"""
        if key in self._listeners:
            for callback in self._listeners[key]:
                try:
                    callback(key, old_value, new_value)
                except Exception as e:
                    logger.error(f"配置变更监听器执行失败: {e}")

        for callback in self._global_listeners:
            try:
                callback(key, old_value, new_value)
            except Exception as e:
                logger.error(f"全局配置变更监听器执行失败: {e}")

    def export_config(self, filepath: str) -> bool:
        """导出配置到文件"""
        try:
            config = self.get_all_config()
            export_data = {
                "export_time": datetime.now().isoformat(),
                "config": config
            }
            with open(filepath, 'w', encoding='utf-8') as f:
                json.dump(export_data, f, ensure_ascii=False, indent=2)
            return True
        except Exception as e:
            logger.error(f"导出配置失败: {e}")
            return False

    def import_config(self, filepath: str, merge: bool = True) -> bool:
        """从文件导入配置"""
        try:
            with open(filepath, 'r', encoding='utf-8') as f:
                data = json.load(f)

            config = data.get("config", {})

            if merge:
                return self.update_config(config)
            else:
                with self._lock:
                    old_config = self.get_all_config()
                    self._config._config = config
                    if self._config.save_config():
                        self._notify_change("*", old_config, config)
                        return True
                    return False

        except Exception as e:
            logger.error(f"导入配置失败: {e}")
            return False

    def validate_config(self, config: Dict[str, Any]) -> Dict[str, Any]:
        """验证配置"""
        errors = []

        if "http" in config:
            http_config = config["http"]
            if "port" in http_config:
                port = http_config["port"]
                if not isinstance(port, int) or port < 1 or port > 65535:
                    errors.append("HTTP端口必须是1-65535之间的整数")

        if "websocket" in config:
            ws_config = config["websocket"]
            if "enabled" in ws_config and not isinstance(ws_config["enabled"], bool):
                errors.append("WebSocket启用状态必须是布尔值")
            if "port" in ws_config:
                port = ws_config["port"]
                if not isinstance(port, int) or port < 1 or port > 65535:
                    errors.append("WebSocket端口必须是1-65535之间的整数")

        return {
            "valid": len(errors) == 0,
            "errors": errors
        }

    def reset_to_default(self) -> bool:
        """重置为默认配置"""
        with self._lock:
            old_config = self.get_all_config()

            if self._config.reset_to_default():
                self._notify_change("*", old_config, self.get_all_config())
                return True
            return False


# 全局运行时配置管理器实例
_runtime_config_manager: Optional[RuntimeConfigManager] = None


def get_runtime_config() -> RuntimeConfigManager:
    """获取全局运行时配置管理器实例"""
    global _runtime_config_manager
    if _runtime_config_manager is None:
        _runtime_config_manager = RuntimeConfigManager()
    return _runtime_config_manager
