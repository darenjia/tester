"""
变量缓存管理器

缓存CANoe/TSMaster等工具的系统变量，减少COM/API调用次数，提高执行效率
"""
import time
import threading
from typing import Dict, Any, Optional, Callable, List, Tuple
from dataclasses import dataclass, field
from collections import OrderedDict
import logging

logger = logging.getLogger(__name__)


@dataclass
class CachedVariable:
    """缓存的变量"""
    name: str
    namespace: str
    value: Any = None
    var_object: Any = None  # 原始变量对象引用
    last_access_time: float = field(default_factory=time.time)
    access_count: int = 0
    is_valid: bool = True


class VariableCache:
    """变量缓存管理器"""
    
    def __init__(self, max_size: int = 1000, ttl: float = 300.0):
        """
        初始化变量缓存
        
        Args:
            max_size: 最大缓存数量
            ttl: 缓存过期时间（秒）
        """
        self.max_size = max_size
        self.ttl = ttl
        self._cache: Dict[str, CachedVariable] = {}
        self._lock = threading.RLock()
        self._access_order: OrderedDict = OrderedDict()  # LRU顺序
        self._hit_count = 0
        self._miss_count = 0
        
        # 统计信息
        self._stats = {
            'hits': 0,
            'misses': 0,
            'evictions': 0,
            'total_access': 0
        }
    
    def _make_key(self, name: str, namespace: str) -> str:
        """生成缓存键"""
        return f"{namespace}.{name}"
    
    def get(self, name: str, namespace: str = "mutualVar") -> Optional[CachedVariable]:
        """
        获取缓存的变量
        
        Args:
            name: 变量名
            namespace: 命名空间
            
        Returns:
            CachedVariable: 缓存的变量，未找到返回None
        """
        key = self._make_key(name, namespace)
        
        with self._lock:
            self._stats['total_access'] += 1
            
            if key in self._cache:
                cached_var = self._cache[key]
                
                # 检查是否过期
                if time.time() - cached_var.last_access_time > self.ttl:
                    logger.debug(f"Variable {key} expired, removing from cache")
                    self._remove(key)
                    self._stats['misses'] += 1
                    return None
                
                # 更新访问信息
                cached_var.last_access_time = time.time()
                cached_var.access_count += 1
                
                # 更新LRU顺序
                self._access_order.move_to_end(key)
                
                self._stats['hits'] += 1
                return cached_var
            
            self._stats['misses'] += 1
            return None
    
    def put(self, name: str, namespace: str, var_object: Any, 
            value: Any = None) -> CachedVariable:
        """
        添加变量到缓存
        
        Args:
            name: 变量名
            namespace: 命名空间
            var_object: 原始变量对象
            value: 变量值（可选）
            
        Returns:
            CachedVariable: 缓存的变量
        """
        key = self._make_key(name, namespace)
        
        with self._lock:
            # 检查是否需要淘汰
            if len(self._cache) >= self.max_size and key not in self._cache:
                self._evict_oldest()
            
            # 创建或更新缓存项
            cached_var = CachedVariable(
                name=name,
                namespace=namespace,
                value=value,
                var_object=var_object,
                last_access_time=time.time(),
                access_count=1
            )
            
            self._cache[key] = cached_var
            self._access_order[key] = True
            self._access_order.move_to_end(key)
            
            logger.debug(f"Variable {key} cached")
            return cached_var
    
    def invalidate(self, name: str, namespace: str = "mutualVar") -> bool:
        """
        使缓存项失效
        
        Args:
            name: 变量名
            namespace: 命名空间
            
        Returns:
            bool: 是否成功移除
        """
        key = self._make_key(name, namespace)
        
        with self._lock:
            return self._remove(key)
    
    def invalidate_namespace(self, namespace: str) -> int:
        """
        使整个命名空间的缓存失效
        
        Args:
            namespace: 命名空间
            
        Returns:
            int: 移除的缓存项数量
        """
        keys_to_remove = []
        
        with self._lock:
            for key, cached_var in self._cache.items():
                if cached_var.namespace == namespace:
                    keys_to_remove.append(key)
            
            for key in keys_to_remove:
                self._remove(key)
        
        logger.info(f"Invalidated {len(keys_to_remove)} variables in namespace {namespace}")
        return len(keys_to_remove)
    
    def invalidate_all(self) -> None:
        """使所有缓存失效"""
        with self._lock:
            self._cache.clear()
            self._access_order.clear()
        
        logger.info("All cache invalidated")
    
    def refresh(self, name: str, namespace: str = "mutualVar", 
                fetch_func: Optional[Callable[[], Any]] = None) -> Optional[CachedVariable]:
        """
        刷新缓存项
        
        Args:
            name: 变量名
            namespace: 命名空间
            fetch_func: 获取新值的函数
            
        Returns:
            CachedVariable: 刷新后的变量
        """
        key = self._make_key(name, namespace)
        
        with self._lock:
            if key in self._cache and fetch_func:
                try:
                    new_value = fetch_func()
                    self._cache[key].value = new_value
                    self._cache[key].last_access_time = time.time()
                    logger.debug(f"Variable {key} refreshed")
                    return self._cache[key]
                except Exception as e:
                    logger.error(f"Failed to refresh variable {key}: {e}")
                    return None
        
        return None
    
    def get_stats(self) -> Dict[str, Any]:
        """获取缓存统计信息"""
        with self._lock:
            total = self._stats['hits'] + self._stats['misses']
            hit_rate = self._stats['hits'] / total if total > 0 else 0
            
            return {
                'size': len(self._cache),
                'max_size': self.max_size,
                'hits': self._stats['hits'],
                'misses': self._stats['misses'],
                'hit_rate': hit_rate,
                'evictions': self._stats['evictions'],
                'total_access': self._stats['total_access']
            }
    
    def get_cached_variables(self, namespace: Optional[str] = None) -> List[Dict[str, Any]]:
        """
        获取缓存的变量列表
        
        Args:
            namespace: 命名空间过滤（可选）
            
        Returns:
            List[Dict]: 变量信息列表
        """
        with self._lock:
            variables = []
            for key, cached_var in self._cache.items():
                if namespace is None or cached_var.namespace == namespace:
                    variables.append({
                        'name': cached_var.name,
                        'namespace': cached_var.namespace,
                        'value': cached_var.value,
                        'access_count': cached_var.access_count,
                        'last_access_time': cached_var.last_access_time,
                        'is_valid': cached_var.is_valid
                    })
            return variables
    
    def _remove(self, key: str) -> bool:
        """移除缓存项"""
        if key in self._cache:
            del self._cache[key]
            if key in self._access_order:
                del self._access_order[key]
            return True
        return False
    
    def _evict_oldest(self) -> None:
        """淘汰最旧的缓存项（LRU）"""
        if self._access_order:
            oldest_key = next(iter(self._access_order))
            self._remove(oldest_key)
            self._stats['evictions'] += 1
            logger.debug(f"Evicted oldest variable: {oldest_key}")


class VariableCacheManager:
    """变量缓存管理器（单例）"""
    
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
        
        self._initialized = True
        self._caches: Dict[str, VariableCache] = {}
        self._default_cache = VariableCache()
        self._lock = threading.RLock()
        
        # 启动清理线程
        self._cleanup_thread = threading.Thread(target=self._cleanup_loop, daemon=True)
        self._cleanup_thread.start()
        
        logger.info("VariableCacheManager initialized")
    
    def get_cache(self, cache_name: str = "default") -> VariableCache:
        """
        获取指定名称的缓存
        
        Args:
            cache_name: 缓存名称
            
        Returns:
            VariableCache: 变量缓存实例
        """
        if cache_name == "default":
            return self._default_cache
        
        with self._lock:
            if cache_name not in self._caches:
                self._caches[cache_name] = VariableCache()
            return self._caches[cache_name]
    
    def create_cache(self, cache_name: str, max_size: int = 1000, 
                     ttl: float = 300.0) -> VariableCache:
        """
        创建新的缓存
        
        Args:
            cache_name: 缓存名称
            max_size: 最大缓存数量
            ttl: 过期时间
            
        Returns:
            VariableCache: 创建的缓存实例
        """
        with self._lock:
            if cache_name in self._caches:
                logger.warning(f"Cache {cache_name} already exists, returning existing")
                return self._caches[cache_name]
            
            cache = VariableCache(max_size=max_size, ttl=ttl)
            self._caches[cache_name] = cache
            logger.info(f"Created cache: {cache_name}")
            return cache
    
    def remove_cache(self, cache_name: str) -> bool:
        """
        移除缓存
        
        Args:
            cache_name: 缓存名称
            
        Returns:
            bool: 是否成功移除
        """
        with self._lock:
            if cache_name in self._caches:
                del self._caches[cache_name]
                logger.info(f"Removed cache: {cache_name}")
                return True
            return False
    
    def get_all_stats(self) -> Dict[str, Dict[str, Any]]:
        """获取所有缓存的统计信息"""
        stats = {'default': self._default_cache.get_stats()}
        
        with self._lock:
            for name, cache in self._caches.items():
                stats[name] = cache.get_stats()
        
        return stats
    
    def invalidate_all(self) -> None:
        """使所有缓存失效"""
        self._default_cache.invalidate_all()
        
        with self._lock:
            for cache in self._caches.values():
                cache.invalidate_all()
        
        logger.info("All caches invalidated")
    
    def _cleanup_loop(self) -> None:
        """清理循环，定期清理过期缓存"""
        while True:
            try:
                time.sleep(60)  # 每分钟检查一次
                
                # 清理默认缓存
                self._cleanup_expired(self._default_cache)
                
                # 清理所有命名缓存
                with self._lock:
                    for cache in self._caches.values():
                        self._cleanup_expired(cache)
                        
            except Exception as e:
                logger.error(f"Cleanup loop error: {e}")
    
    def _cleanup_expired(self, cache: VariableCache) -> None:
        """清理过期的缓存项"""
        expired_keys = []
        current_time = time.time()
        
        with cache._lock:
            for key, cached_var in cache._cache.items():
                if current_time - cached_var.last_access_time > cache.ttl:
                    expired_keys.append(key)
        
        for key in expired_keys:
            cache._remove(key)
        
        if expired_keys:
            logger.debug(f"Cleaned up {len(expired_keys)} expired variables")


# 全局变量缓存管理器实例
variable_cache_manager = VariableCacheManager()


def get_variable_cache(cache_name: str = "default") -> VariableCache:
    """
    获取变量缓存实例
    
    Args:
        cache_name: 缓存名称
        
    Returns:
        VariableCache: 变量缓存实例
    """
    return variable_cache_manager.get_cache(cache_name)
