"""
状态机核心实现模块

提供测试执行流程的状态机管理，支持复杂测试场景的状态转换
"""
import time
from enum import Enum, auto
from typing import Dict, Any, Optional, Callable, List
from dataclasses import dataclass, field
from abc import ABC, abstractmethod
import logging

logger = logging.getLogger(__name__)


class TestState(Enum):
    """测试执行状态枚举"""
    __test__ = False
    IDLE = auto()           # 空闲状态
    SELF_CHECK = auto()     # 自检状态
    CONFIG_LOAD = auto()    # 配置加载状态
    CONNECTING = auto()     # 连接工具状态
    RUNNING = auto()        # 测试执行状态
    PAUSED = auto()         # 暂停状态
    RESULT_COLLECT = auto() # 结果收集状态
    CLEANUP = auto()        # 清理状态
    COMPLETED = auto()      # 完成状态
    FAILED = auto()         # 失败状态


@dataclass
class StateTransition:
    """状态转换定义"""
    from_state: TestState
    to_state: TestState
    condition: Optional[Callable[[], bool]] = None
    action: Optional[Callable[[], None]] = None


@dataclass
class StateContext:
    """状态上下文信息"""
    task_id: str
    start_time: float = field(default_factory=time.time)
    current_state: TestState = TestState.IDLE
    state_history: List[Dict[str, Any]] = field(default_factory=list)
    data: Dict[str, Any] = field(default_factory=dict)
    error: Optional[str] = None

    def record_transition(self, from_state: TestState, to_state: TestState, 
                         reason: str = ""):
        """记录状态转换"""
        self.state_history.append({
            "from": from_state.name,
            "to": to_state.name,
            "timestamp": time.time(),
            "reason": reason
        })


class StateHandler(ABC):
    """状态处理器基类"""
    
    @abstractmethod
    def on_enter(self, context: StateContext) -> None:
        """进入状态时调用"""
        pass
    
    @abstractmethod
    def on_execute(self, context: StateContext) -> TestState:
        """执行状态逻辑，返回下一个状态"""
        pass
    
    @abstractmethod
    def on_exit(self, context: StateContext) -> None:
        """退出状态时调用"""
        pass


class TestStateMachine:
    """测试状态机"""
    __test__ = False
    
    def __init__(self, task_id: str):
        self.task_id = task_id
        self.context = StateContext(task_id=task_id)
        self.handlers: Dict[TestState, StateHandler] = {}
        self.transitions: Dict[TestState, List[StateTransition]] = {}
        self._running = False
        self._paused = False
        self._state_change_callbacks: List[Callable[[TestState, TestState], None]] = []
        
    def register_handler(self, state: TestState, handler: StateHandler) -> None:
        """注册状态处理器"""
        self.handlers[state] = handler
        logger.debug(f"Registered handler for state {state.name}")
    
    def register_transition(self, transition: StateTransition) -> None:
        """注册状态转换"""
        if transition.from_state not in self.transitions:
            self.transitions[transition.from_state] = []
        self.transitions[transition.from_state].append(transition)
        logger.debug(f"Registered transition: {transition.from_state.name} -> {transition.to_state.name}")
    
    def add_state_change_callback(self, callback: Callable[[TestState, TestState], None]) -> None:
        """添加状态变化回调"""
        self._state_change_callbacks.append(callback)
    
    def _notify_state_change(self, from_state: TestState, to_state: TestState) -> None:
        """通知状态变化"""
        for callback in self._state_change_callbacks:
            try:
                callback(from_state, to_state)
            except Exception as e:
                logger.error(f"State change callback error: {e}")
    
    def transition_to(self, to_state: TestState, reason: str = "") -> bool:
        """手动触发状态转换"""
        if not self._can_transition(self.context.current_state, to_state):
            logger.warning(f"Cannot transition from {self.context.current_state.name} to {to_state.name}")
            return False
        
        self._do_transition(to_state, reason)
        return True
    
    def _can_transition(self, from_state: TestState, to_state: TestState) -> bool:
        """检查是否可以转换"""
        # 允许从任何状态转换到FAILED或COMPLETED（终止状态）
        if to_state in [TestState.FAILED, TestState.COMPLETED]:
            return True
        
        # 允许从RUNNING转换到PAUSED
        if from_state == TestState.RUNNING and to_state == TestState.PAUSED:
            return True
        
        # 允许从PAUSED转换回RUNNING
        if from_state == TestState.PAUSED and to_state == TestState.RUNNING:
            return True
        
        # 检查注册的转换规则
        if from_state in self.transitions:
            for transition in self.transitions[from_state]:
                if transition.to_state == to_state:
                    if transition.condition is None or transition.condition():
                        return True
        
        return False
    
    def _do_transition(self, to_state: TestState, reason: str = "") -> None:
        """执行状态转换"""
        from_state = self.context.current_state
        
        # 调用当前状态的退出处理
        if from_state in self.handlers:
            try:
                self.handlers[from_state].on_exit(self.context)
            except Exception as e:
                logger.error(f"Error in on_exit for state {from_state.name}: {e}")
        
        # 记录状态转换
        self.context.record_transition(from_state, to_state, reason)
        
        # 更新当前状态
        self.context.current_state = to_state
        
        # 通知状态变化
        self._notify_state_change(from_state, to_state)
        
        # 调用新状态的进入处理
        if to_state in self.handlers:
            try:
                self.handlers[to_state].on_enter(self.context)
            except Exception as e:
                logger.error(f"Error in on_enter for state {to_state.name}: {e}")
        
        logger.info(f"State transition: {from_state.name} -> {to_state.name}, reason: {reason}")
    
    def start(self) -> None:
        """启动状态机"""
        self._running = True
        self._paused = False
        self.context.start_time = time.time()
        
        # 从IDLE开始
        if self.context.current_state == TestState.IDLE:
            self.transition_to(TestState.SELF_CHECK, "Starting test execution")
        
        logger.info(f"State machine started for task {self.task_id}")
    
    def stop(self) -> None:
        """停止状态机"""
        self._running = False
        logger.info(f"State machine stopped for task {self.task_id}")
    
    def pause(self) -> bool:
        """暂停状态机"""
        if self.context.current_state == TestState.RUNNING:
            self._paused = True
            self.transition_to(TestState.PAUSED, "User requested pause")
            return True
        return False
    
    def resume(self) -> bool:
        """恢复状态机"""
        if self.context.current_state == TestState.PAUSED:
            self._paused = False
            self.transition_to(TestState.RUNNING, "User requested resume")
            return True
        return False
    
    def step(self) -> bool:
        """执行一个状态步骤，返回是否继续运行"""
        if not self._running:
            return False
        
        if self._paused:
            time.sleep(0.1)
            return True
        
        current_state = self.context.current_state
        
        # 终止状态
        if current_state in [TestState.COMPLETED, TestState.FAILED]:
            self._running = False
            return False
        
        # 执行当前状态
        if current_state in self.handlers:
            try:
                next_state = self.handlers[current_state].on_execute(self.context)
                
                # 如果处理器返回了不同的状态，执行转换
                if next_state != current_state:
                    if self._can_transition(current_state, next_state):
                        self._do_transition(next_state, "State handler requested transition")
                    else:
                        logger.error(f"Invalid transition requested: {current_state.name} -> {next_state.name}")
                        self.context.error = f"Invalid state transition: {current_state.name} -> {next_state.name}"
                        self._do_transition(TestState.FAILED, "Invalid state transition")
                        
            except Exception as e:
                logger.error(f"Error executing state {current_state.name}: {e}")
                self.context.error = str(e)
                self._do_transition(TestState.FAILED, f"State execution error: {e}")
        else:
            logger.warning(f"No handler registered for state {current_state.name}")
            time.sleep(0.1)
        
        return self._running
    
    def run(self) -> None:
        """运行状态机直到完成"""
        self.start()
        
        while self.step():
            pass
        
        logger.info(f"State machine completed for task {self.task_id}, final state: {self.context.current_state.name}")
    
    def get_state(self) -> TestState:
        """获取当前状态"""
        return self.context.current_state
    
    def get_context(self) -> StateContext:
        """获取状态上下文"""
        return self.context
    
    def is_running(self) -> bool:
        """检查是否正在运行"""
        return self._running
    
    def is_paused(self) -> bool:
        """检查是否暂停"""
        return self._paused


# 预定义的标准测试状态转换
STANDARD_TRANSITIONS = [
    StateTransition(TestState.IDLE, TestState.SELF_CHECK),
    StateTransition(TestState.SELF_CHECK, TestState.CONFIG_LOAD),
    StateTransition(TestState.CONFIG_LOAD, TestState.CONNECTING),
    StateTransition(TestState.CONNECTING, TestState.RUNNING),
    StateTransition(TestState.RUNNING, TestState.RESULT_COLLECT),
    StateTransition(TestState.RESULT_COLLECT, TestState.CLEANUP),
    StateTransition(TestState.CLEANUP, TestState.COMPLETED),
]
