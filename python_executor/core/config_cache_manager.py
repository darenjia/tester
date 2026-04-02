"""
配置缓存管理器
管理测试配置文件的本地缓存，提高配置加载效率
"""
import os
import sys
import json
import hashlib
import shutil
import threading
from datetime import datetime, timedelta
from typing import Dict, Any, Optional, List
from pathlib import Path

from config.unified_config import get_config_manager
from utils.logger import get_logger

logger = get_logger("config_cache")


class ConfigCacheManager:
    """配置缓存管理器"""

    CACHE_VERSION = "1.0"
    CACHE_INFO_FILE = "cache_info.json"
    CFG_FILENAME = "config.cfg"
    INI_FILENAME = "config.ini"

    def __init__(self, cache_dir: str = None):
        """
        初始化缓存管理器

        Args:
            cache_dir: 缓存目录路径，默认使用 workspace/cache/configs
        """
        if cache_dir is None:
            if getattr(sys, 'frozen', False):
                base_dir = os.path.dirname(sys.executable)
            else:
                base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
            cache_dir = os.path.join(base_dir, "workspace", "cache", "configs")

        self.cache_dir = cache_dir
        self._lock = threading.RLock()

        # 确保缓存目录存在
        self._ensure_cache_dir()

        # 加载配置
        self._load_config()

        logger.info(f"配置缓存管理器初始化完成: cache_dir={self.cache_dir}")

    def _ensure_cache_dir(self):
        """确保缓存目录存在"""
        try:
            os.makedirs(self.cache_dir, exist_ok=True)
        except Exception as e:
            logger.error(f"创建缓存目录失败: {e}")
            raise

    def _load_config(self):
        """从全局配置加载缓存相关配置"""
        try:
            config_manager = get_config_manager()
            self._enabled = config_manager.get('config_cache.enabled', True)
            self._max_cache_count = config_manager.get('config_cache.max_cache_count', 50)
            self._cache_ttl_hours = config_manager.get('config_cache.cache_ttl_hours', 168)
            self._auto_cleanup = config_manager.get('config_cache.auto_cleanup', True)
        except ImportError:
            logger.warning("无法导入配置管理器，使用默认缓存配置")
            self._enabled = True
            self._max_cache_count = 50
            self._cache_ttl_hours = 168
            self._auto_cleanup = True

    @property
    def enabled(self) -> bool:
        """是否启用缓存"""
        self._load_config()  # 刷新配置
        return self._enabled

    def get_cache_path(self, config_name: str) -> str:
        """
        获取配置的缓存目录路径

        Args:
            config_name: 配置名称

        Returns:
            缓存目录路径
        """
        # 清理配置名称中的非法字符
        safe_name = self._sanitize_config_name(config_name)
        return os.path.join(self.cache_dir, safe_name)

    def _sanitize_config_name(self, config_name: str) -> str:
        """清理配置名称，移除非法字符"""
        import re
        # 替换非法字符为下划线
        safe_name = re.sub(r'[<>:"/\\|?*]', '_', config_name)
        return safe_name.strip()

    def get_cached_config(self, config_name: str, source_path: str = None) -> Optional[Dict]:
        """
        获取缓存的配置信息

        Args:
            config_name: 配置名称
            source_path: 源文件路径（用于验证）

        Returns:
            缓存信息字典，如果缓存不存在或无效返回None
        """
        if not self.enabled:
            return None

        with self._lock:
            cache_path = self.get_cache_path(config_name)
            cache_info_path = os.path.join(cache_path, self.CACHE_INFO_FILE)

            # 检查缓存是否存在
            if not os.path.exists(cache_info_path):
                return None

            try:
                # 读取缓存元数据
                with open(cache_info_path, 'r', encoding='utf-8') as f:
                    cache_info = json.load(f)

                # 验证缓存版本
                if cache_info.get('cache_version') != self.CACHE_VERSION:
                    logger.debug(f"缓存版本不匹配: {config_name}")
                    return None

                # 验证缓存文件是否存在
                cfg_path = os.path.join(cache_path, self.CFG_FILENAME)
                if not os.path.exists(cfg_path):
                    logger.debug(f"缓存cfg文件不存在: {config_name}")
                    return None

                # 如果提供了源路径，验证源文件是否变化
                if source_path and os.path.exists(source_path):
                    if not self._is_source_unchanged(cache_info, source_path):
                        logger.debug(f"源文件已变化: {config_name}")
                        return None

                # 检查缓存是否过期
                if self._is_cache_expired(cache_info):
                    logger.debug(f"缓存已过期: {config_name}")
                    return None

                # 缓存有效，返回缓存信息
                cache_info['cfg_path'] = cfg_path
                cache_info['cache_path'] = cache_path
                return cache_info

            except Exception as e:
                logger.warning(f"读取缓存信息失败: {config_name}, {e}")
                return None

    def _is_source_unchanged(self, cache_info: Dict, source_path: str) -> bool:
        """检查源文件是否未变化"""
        try:
            # 检查修改时间
            current_modified = os.path.getmtime(source_path)
            cached_modified = cache_info.get('source_modified', 0)

            if current_modified != cached_modified:
                return False

            # 检查文件hash
            current_hash = self._calculate_file_hash(source_path)
            cached_hash = cache_info.get('file_hash', '')

            return current_hash == cached_hash

        except Exception as e:
            logger.warning(f"检查源文件变化失败: {e}")
            return False

    def _is_cache_expired(self, cache_info: Dict) -> bool:
        """检查缓存是否过期"""
        try:
            cached_at_str = cache_info.get('cached_at')
            if not cached_at_str:
                return True

            cached_at = datetime.fromisoformat(cached_at_str)
            expire_time = cached_at + timedelta(hours=self._cache_ttl_hours)

            return datetime.now() > expire_time

        except Exception as e:
            logger.warning(f"检查缓存过期时间失败: {e}")
            return True

    def _calculate_file_hash(self, file_path: str) -> str:
        """计算文件MD5 hash"""
        try:
            hash_md5 = hashlib.md5()
            with open(file_path, "rb") as f:
                for chunk in iter(lambda: f.read(4096), b""):
                    hash_md5.update(chunk)
            return hash_md5.hexdigest()
        except Exception as e:
            logger.error(f"计算文件hash失败: {file_path}, {e}")
            return ""

    def cache_config(self, config_name: str, source_path: str) -> Dict:
        """
        将配置复制到缓存目录

        Args:
            config_name: 配置名称
            source_path: 源文件路径

        Returns:
            缓存信息字典
        """
        with self._lock:
            cache_path = self.get_cache_path(config_name)

            try:
                # 确保缓存目录存在
                os.makedirs(cache_path, exist_ok=True)

                # 复制cfg文件
                cached_cfg_path = os.path.join(cache_path, self.CFG_FILENAME)
                shutil.copy2(source_path, cached_cfg_path)

                # 计算文件hash和修改时间
                file_hash = self._calculate_file_hash(source_path)
                source_modified = os.path.getmtime(source_path)

                # 创建缓存元数据
                cache_info = {
                    "config_name": config_name,
                    "source_path": source_path,
                    "cached_at": datetime.now().isoformat(),
                    "source_modified": source_modified,
                    "cache_version": self.CACHE_VERSION,
                    "file_hash": file_hash,
                    "ini_generated": False,
                    "ini_path": None
                }

                # 保存缓存元数据
                cache_info_path = os.path.join(cache_path, self.CACHE_INFO_FILE)
                with open(cache_info_path, 'w', encoding='utf-8') as f:
                    json.dump(cache_info, f, indent=2, ensure_ascii=False)

                # 更新缓存信息
                cache_info['cfg_path'] = cached_cfg_path
                cache_info['cache_path'] = cache_path

                logger.info(f"配置已缓存: {config_name} -> {cache_path}")

                # 自动清理
                if self._auto_cleanup:
                    self._cleanup_if_needed()

                return cache_info

            except Exception as e:
                logger.error(f"缓存配置失败: {config_name}, {e}")
                # 清理失败的缓存
                self._remove_cache_dir(cache_path)
                raise

    def get_or_create_cache(self, config_name: str, source_path: str) -> Dict:
        """
        获取缓存，如果不存在或无效则创建

        Args:
            config_name: 配置名称
            source_path: 源文件路径

        Returns:
            缓存信息字典
        """
        # 先尝试获取缓存
        cached = self.get_cached_config(config_name, source_path)
        if cached:
            logger.debug(f"缓存命中: {config_name}")
            return cached

        # 缓存未命中，创建新缓存
        logger.info(f"缓存未命中，创建新缓存: {config_name}")
        return self.cache_config(config_name, source_path)

    def update_ini_info(self, config_name: str, ini_path: str):
        """
        更新缓存中的ini文件信息

        Args:
            config_name: 配置名称
            ini_path: ini文件路径
        """
        with self._lock:
            cache_path = self.get_cache_path(config_name)
            cache_info_path = os.path.join(cache_path, self.CACHE_INFO_FILE)

            try:
                if os.path.exists(cache_info_path):
                    with open(cache_info_path, 'r', encoding='utf-8') as f:
                        cache_info = json.load(f)

                    cache_info['ini_generated'] = True
                    cache_info['ini_path'] = ini_path

                    with open(cache_info_path, 'w', encoding='utf-8') as f:
                        json.dump(cache_info, f, indent=2, ensure_ascii=False)

            except Exception as e:
                logger.warning(f"更新ini信息失败: {config_name}, {e}")

    def get_ini_path(self, config_name: str) -> Optional[str]:
        """获取缓存中的ini文件路径"""
        cache_path = self.get_cache_path(config_name)
        ini_path = os.path.join(cache_path, self.INI_FILENAME)

        if os.path.exists(ini_path):
            return ini_path
        return None

    def _remove_cache_dir(self, cache_path: str):
        """删除缓存目录"""
        try:
            if os.path.exists(cache_path):
                shutil.rmtree(cache_path)
        except Exception as e:
            logger.warning(f"删除缓存目录失败: {cache_path}, {e}")

    def _cleanup_if_needed(self):
        """如果需要则执行清理"""
        try:
            # 获取所有缓存
            caches = self._list_all_caches()

            # 按缓存时间排序
            caches.sort(key=lambda x: x.get('cached_at', ''), reverse=True)

            # 检查数量限制
            if len(caches) > self._max_cache_count:
                # 删除最旧的缓存
                to_remove = caches[self._max_cache_count:]
                for cache_info in to_remove:
                    config_name = cache_info.get('config_name')
                    cache_path = self.get_cache_path(config_name)
                    self._remove_cache_dir(cache_path)
                    logger.info(f"清理过期缓存: {config_name}")

            # 检查过期缓存
            for cache_info in caches[:self._max_cache_count]:
                if self._is_cache_expired(cache_info):
                    config_name = cache_info.get('config_name')
                    cache_path = self.get_cache_path(config_name)
                    self._remove_cache_dir(cache_path)
                    logger.info(f"清理过期缓存: {config_name}")

        except Exception as e:
            logger.error(f"清理缓存失败: {e}")

    def _list_all_caches(self) -> List[Dict]:
        """列出所有缓存信息"""
        caches = []

        try:
            if not os.path.exists(self.cache_dir):
                return caches

            for item in os.listdir(self.cache_dir):
                cache_path = os.path.join(self.cache_dir, item)
                if os.path.isdir(cache_path):
                    cache_info_path = os.path.join(cache_path, self.CACHE_INFO_FILE)
                    if os.path.exists(cache_info_path):
                        try:
                            with open(cache_info_path, 'r', encoding='utf-8') as f:
                                cache_info = json.load(f)
                                caches.append(cache_info)
                        except Exception:
                            pass

        except Exception as e:
            logger.error(f"列出缓存失败: {e}")

        return caches

    def cleanup_expired_cache(self):
        """手动清理过期缓存"""
        logger.info("开始手动清理过期缓存...")
        self._cleanup_if_needed()
        logger.info("过期缓存清理完成")

    def clear_all_cache(self):
        """清空所有缓存"""
        with self._lock:
            try:
                if os.path.exists(self.cache_dir):
                    shutil.rmtree(self.cache_dir)
                    os.makedirs(self.cache_dir, exist_ok=True)
                logger.info("所有缓存已清空")
            except Exception as e:
                logger.error(f"清空缓存失败: {e}")
                raise

    def get_cache_stats(self) -> Dict:
        """获取缓存统计信息"""
        try:
            caches = self._list_all_caches()
            total_size = 0

            for cache_info in caches:
                config_name = cache_info.get('config_name', '')
                cache_path = self.get_cache_path(config_name)
                if os.path.exists(cache_path):
                    for root, dirs, files in os.walk(cache_path):
                        for file in files:
                            file_path = os.path.join(root, file)
                            total_size += os.path.getsize(file_path)

            return {
                "cache_count": len(caches),
                "total_size_bytes": total_size,
                "total_size_mb": round(total_size / (1024 * 1024), 2),
                "cache_dir": self.cache_dir,
                "max_cache_count": self._max_cache_count,
                "cache_ttl_hours": self._cache_ttl_hours
            }

        except Exception as e:
            logger.error(f"获取缓存统计失败: {e}")
            return {"error": str(e)}


# 全局缓存管理器实例
_cache_manager_instance: Optional[ConfigCacheManager] = None


def get_config_cache_manager(cache_dir: str = None) -> ConfigCacheManager:
    """
    获取配置缓存管理器实例（单例模式）

    Args:
        cache_dir: 缓存目录路径

    Returns:
        ConfigCacheManager 实例
    """
    global _cache_manager_instance
    if _cache_manager_instance is None:
        _cache_manager_instance = ConfigCacheManager(cache_dir)
    return _cache_manager_instance
