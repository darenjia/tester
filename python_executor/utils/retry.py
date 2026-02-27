"""
重试装饰器和工具
用于生产环境的健壮性增强
"""
import time
import functools
import logging
from typing import Callable, Type, Tuple, Optional

logger = logging.getLogger(__name__)

class RetryConfig:
    """重试配置"""
    def __init__(
        self,
        max_attempts: int = 3,
        delay: float = 1.0,
        backoff: float = 2.0,
        exceptions: Tuple[Type[Exception], ...] = (Exception,),
        on_retry: Optional[Callable] = None,
        on_failure: Optional[Callable] = None
    ):
        self.max_attempts = max_attempts
        self.delay = delay
        self.backoff = backoff
        self.exceptions = exceptions
        self.on_retry = on_retry
        self.on_failure = on_failure

def retry_with_config(config: RetryConfig = None):
    """
    带配置的重试装饰器
    
    Args:
        config: 重试配置，如果为None则使用默认配置
    """
    if config is None:
        config = RetryConfig()
    
    def decorator(func: Callable) -> Callable:
        @functools.wraps(func)
        def wrapper(*args, **kwargs):
            last_exception = None
            current_delay = config.delay
            
            for attempt in range(1, config.max_attempts + 1):
                try:
                    return func(*args, **kwargs)
                except config.exceptions as e:
                    last_exception = e
                    
                    if attempt == config.max_attempts:
                        # 最后一次重试失败
                        logger.error(
                            f"函数 {func.__name__} 在 {config.max_attempts} 次尝试后失败: {e}"
                        )
                        if config.on_failure:
                            config.on_failure(e, attempt)
                        raise last_exception
                    
                    # 记录重试
                    logger.warning(
                        f"函数 {func.__name__} 第 {attempt} 次尝试失败: {e}, "
                        f"{current_delay}秒后重试..."
                    )
                    
                    if config.on_retry:
                        config.on_retry(e, attempt)
                    
                    # 等待后重试
                    time.sleep(current_delay)
                    current_delay *= config.backoff
            
            # 不应该到达这里，但为了类型检查
            raise last_exception if last_exception else Exception("未知错误")
        
        return wrapper
    return decorator

def retry(
    max_attempts: int = 3,
    delay: float = 1.0,
    backoff: float = 2.0,
    exceptions: Tuple[Type[Exception], ...] = (Exception,)
):
    """
    简化的重试装饰器
    
    使用示例:
        @retry(max_attempts=3, delay=1.0)
        def connect_to_service():
            # 可能失败的代码
            pass
    """
    config = RetryConfig(
        max_attempts=max_attempts,
        delay=delay,
        backoff=backoff,
        exceptions=exceptions
    )
    return retry_with_config(config)

class CircuitBreaker:
    """
    熔断器模式
    用于防止级联故障
    """
    
    STATE_CLOSED = 'closed'      # 正常状态
    STATE_OPEN = 'open'          # 熔断状态
    STATE_HALF_OPEN = 'half_open'  # 半开状态
    
    def __init__(
        self,
        failure_threshold: int = 5,
        recovery_timeout: float = 60.0,
        expected_exception: Type[Exception] = Exception
    ):
        self.failure_threshold = failure_threshold
        self.recovery_timeout = recovery_timeout
        self.expected_exception = expected_exception
        
        self.failure_count = 0
        self.last_failure_time = None
        self.state = self.STATE_CLOSED
    
    def can_execute(self) -> bool:
        """检查是否可以执行"""
        if self.state == self.STATE_CLOSED:
            return True
        
        if self.state == self.STATE_OPEN:
            # 检查是否过了恢复时间
            if self.last_failure_time and \
               (time.time() - self.last_failure_time) >= self.recovery_timeout:
                self.state = self.STATE_HALF_OPEN
                logger.info("熔断器进入半开状态，尝试恢复")
                return True
            return False
        
        if self.state == self.STATE_HALF_OPEN:
            return True
        
        return False
    
    def record_success(self):
        """记录成功"""
        self.failure_count = 0
        self.state = self.STATE_CLOSED
        logger.debug("熔断器记录成功，状态重置为关闭")
    
    def record_failure(self):
        """记录失败"""
        self.failure_count += 1
        self.last_failure_time = time.time()
        
        if self.failure_count >= self.failure_threshold:
            self.state = self.STATE_OPEN
            logger.error(
                f"熔断器打开，失败次数: {self.failure_count}, "
                f"恢复超时: {self.recovery_timeout}秒"
            )
    
    def __call__(self, func: Callable) -> Callable:
        """作为装饰器使用"""
        @functools.wraps(func)
        def wrapper(*args, **kwargs):
            if not self.can_execute():
                raise Exception(f"熔断器处于打开状态，服务暂时不可用")
            
            try:
                result = func(*args, **kwargs)
                self.record_success()
                return result
            except self.expected_exception as e:
                self.record_failure()
                raise
        
        return wrapper

class TimeoutGuard:
    """超时保护上下文管理器"""
    
    def __init__(self, timeout: float, timeout_message: str = "操作超时"):
        self.timeout = timeout
        self.timeout_message = timeout_message
        self._timer = None
        self._timed_out = False
    
    def _timeout_handler(self):
        """超时处理"""
        self._timed_out = True
        logger.error(self.timeout_message)
    
    def __enter__(self):
        """进入上下文"""
        import threading
        self._timer = threading.Timer(self.timeout, self._timeout_handler)
        self._timer.start()
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        """退出上下文"""
        if self._timer:
            self._timer.cancel()
        
        if self._timed_out:
            raise TimeoutError(self.timeout_message)
    
    def is_timed_out(self) -> bool:
        """检查是否已超时"""
        return self._timed_out