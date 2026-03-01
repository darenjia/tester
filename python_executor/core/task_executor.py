"""
任务执行核心引擎
"""
import threading
import time
from datetime import datetime
from typing import Dict, Any, Optional, Callable
from enum import Enum

from utils.logger import get_logger
from utils.exceptions import TaskException, ToolException
from config.settings import settings
from models.task import Task, TaskStatus, TestToolType, TestResult, TestItemType, TaskResult
from core.result_collector import ResultCollector
from core.canoe_controller import CANoeController
from core.tsmaster_controller import TSMasterController

logger = get_logger("task_executor")

class TaskExecutor:
    """任务执行器"""
    
    def __init__(self, message_sender: Callable):
        """
        初始化任务执行器
        
        Args:
            message_sender: 消息发送函数，用于向WebSocket客户端发送消息
        """
        self.message_sender = message_sender
        self.current_task: Optional[Task] = None
        self.current_collector: Optional[ResultCollector] = None
        self.controller = None
        self._stop_event = threading.Event()
        self._task_thread = None
        self._start_time = None
        
        logger.info("任务执行器初始化完成")
    
    def execute_task(self, task: Task) -> bool:
        """
        执行测试任务
        
        Args:
            task: 任务对象
            
        Returns:
            bool: 任务启动成功返回True
        """
        if self.current_task is not None:
            logger.warning("已有任务正在执行")
            return False
        
        self.current_task = task
        self._stop_event.clear()
        
        # 启动任务执行线程
        self._task_thread = threading.Thread(target=self._execute_task, args=(task,))
        self._task_thread.start()
        
        logger.info(f"任务启动成功: {task.task_id}")
        return True
    
    def cancel_task(self) -> bool:
        """
        取消当前任务
        
        Returns:
            bool: 取消成功返回True
        """
        if self.current_task is None:
            logger.warning("没有正在执行的任务")
            return False
        
        logger.info(f"正在取消任务: {self.current_task.task_id}")
        self._stop_event.set()
        return True
    
    def _execute_task(self, task: Task):
        """执行任务主逻辑"""
        self._start_time = time.time()
        
        try:
            logger.info(f"开始执行任务: {task.task_id}")
            
            # 初始化结果收集器
            self.current_collector = ResultCollector(task.task_id)
            
            # 更新状态为运行中
            self._update_task_status(task.task_id, TaskStatus.RUNNING)
            
            # 根据工具类型选择控制器
            if task.tool_type == TestToolType.CANOE.value:
                self.controller = CANoeController()
            elif task.tool_type == TestToolType.TSMASTER.value:
                self.controller = TSMasterController()
            else:
                raise TaskException(f"不支持的测试工具类型: {task.tool_type}")
            
            # 连接测试软件
            self._connect_tool(task)
            
            # 加载配置
            self._load_configuration(task)
            
            # 启动测量/仿真
            self._start_measurement(task)
            
            # 执行测试项
            results = self._execute_test_items(task)
            
            # 停止测量/仿真
            self._stop_measurement(task)
            
            # 完成任务
            self._complete_task(task, results)
            
        except TaskException as e:
            logger.error(f"任务执行失败: {e}")
            self._fail_task(task, str(e))
            
        except ToolException as e:
            logger.error(f"工具操作失败: {e}")
            self._fail_task(task, f"工具操作失败: {e}")
            
        except Exception as e:
            logger.error(f"任务执行异常: {e}", exc_info=True)
            self._fail_task(task, f"任务执行异常: {e}")
            
        finally:
            # 清理资源
            self._cleanup()
            self.current_task = None
            self.current_collector = None
    
    def _connect_tool(self, task: Task):
        """连接测试工具"""
        logger.info(f"正在连接{task.tool_type}...")
        self.current_collector.add_log("INFO", f"正在连接{task.tool_type}测试软件")
        
        max_retries = 3
        for attempt in range(max_retries):
            try:
                if self.controller.connect():
                    logger.info(f"{task.tool_type}连接成功")
                    self.current_collector.add_log("INFO", f"{task.tool_type}连接成功")
                    return
            except Exception as e:
                if attempt < max_retries - 1:
                    logger.warning(f"连接失败，重试中... ({attempt + 1}/{max_retries})")
                    time.sleep(2)
                else:
                    raise ToolException(f"连接{task.tool_type}失败: {e}")
        
        raise ToolException(f"连接{task.tool_type}失败，重试{max_retries}次后仍失败")
    
    def _load_configuration(self, task: Task):
        """加载配置文件"""
        logger.info(f"正在加载配置文件: {task.config_path}")
        self.current_collector.add_log("INFO", f"正在加载配置文件: {task.config_path}")
        
        try:
            self.controller.open_configuration(task.config_path)
            logger.info("配置文件加载成功")
            self.current_collector.add_log("INFO", "配置文件加载成功")
        except Exception as e:
            raise ToolException(f"配置文件加载失败: {e}")
    
    def _start_measurement(self, task: Task):
        """启动测量/仿真"""
        logger.info("正在启动测量/仿真...")
        self.current_collector.add_log("INFO", "正在启动测量/仿真")
        
        try:
            if task.tool_type == TestToolType.CANOE.value:
                success = self.controller.start_measurement()
            else:
                success = self.controller.start_simulation()
            
            if success:
                logger.info("测量/仿真启动成功")
                self.current_collector.add_log("INFO", "测量/仿真启动成功")
            else:
                raise ToolException("测量/仿真启动失败")
                
        except Exception as e:
            raise ToolException(f"启动测量/仿真失败: {e}")
    
    def _execute_test_items(self, task: Task) -> list:
        """执行测试项"""
        results = []
        total_items = len(task.test_items)
        
        logger.info(f"开始执行{total_items}个测试项")
        self.current_collector.add_log("INFO", f"开始执行{total_items}个测试项")
        
        for i, test_item in enumerate(task.test_items, 1):
            # 检查是否被取消
            if self._stop_event.is_set():
                logger.info("任务被取消")
                self.current_collector.add_log("INFO", "任务被取消")
                break
            
            progress = int((i / total_items) * 100)
            logger.info(f"执行测试项 {i}/{total_items}: {test_item.name}")
            
            # 更新进度
            self._update_task_status(
                task.task_id, 
                TaskStatus.RUNNING, 
                f"执行测试项 {i}/{total_items}: {test_item.name}",
                progress
            )
            
            # 执行测试项
            result = self._execute_single_item(test_item)
            results.append(result)
            
            # 收集结果
            self.current_collector.add_test_result(result)
            
            # 上报进度
            self._report_progress(task.task_id, test_item, result)
            
            # 短暂延时，避免执行过快
            time.sleep(0.5)
        
        return results
    
    def _execute_single_item(self, test_item) -> TestResult:
        """执行单个测试项"""
        try:
            if test_item.type == TestItemType.SIGNAL_CHECK.value:
                # 信号检查
                actual_value = self.controller.get_signal(test_item.signal_name)
                passed = abs(actual_value - test_item.expected_value) < 0.01 if actual_value is not None else False
                
                return TestResult(
                    name=test_item.name,
                    type=test_item.type,
                    expected=test_item.expected_value,
                    actual=actual_value,
                    passed=passed
                )
            
            elif test_item.type == TestItemType.SIGNAL_SET.value:
                # 信号设置
                success = self.controller.set_signal(test_item.signal_name, test_item.value)
                
                return TestResult(
                    name=test_item.name,
                    type=test_item.type,
                    success=success
                )
            
            elif test_item.type == TestItemType.TEST_MODULE.value:
                # 测试模块执行
                result = self.controller.run_test_module(test_item.name)
                
                return TestResult(
                    name=test_item.name,
                    type=test_item.type,
                    verdict=result.get("verdict", "UNKNOWN"),
                    details=result
                )
            
            else:
                return TestResult(
                    name=test_item.name,
                    type=test_item.type,
                    error=f"未知的测试项类型: {test_item.type}"
                )
                
        except Exception as e:
            logger.error(f"执行测试项失败: {test_item.name}, 错误: {e}")
            return TestResult(
                name=test_item.name,
                type=test_item.type,
                error=f"执行失败: {e}"
            )
    
    def _stop_measurement(self, task: Task):
        """停止测量/仿真"""
        logger.info("正在停止测量/仿真...")
        self.current_collector.add_log("INFO", "正在停止测量/仿真")
        
        try:
            if task.tool_type == TestToolType.CANOE.value:
                self.controller.stop_measurement()
            else:
                self.controller.stop_simulation()
            
            logger.info("测量/仿真停止成功")
            self.current_collector.add_log("INFO", "测量/仿真停止成功")
            
        except Exception as e:
            logger.warning(f"停止测量/仿真失败: {e}")
            self.current_collector.add_log("WARNING", f"停止测量/仿真失败: {e}")
    
    def _complete_task(self, task: Task, results: list):
        """完成任务"""
        duration = time.time() - self._start_time
        
        logger.info(f"任务执行完成: {task.task_id}, 耗时: {duration:.1f}秒")
        
        # 完成结果收集
        task_result = self.current_collector.finalize(TaskStatus.COMPLETED.value)
        
        # 上报最终结果
        self._report_final_result(task.task_id, task_result)
        
        # 更新状态
        self._update_task_status(task.task_id, TaskStatus.COMPLETED, f"任务执行完成，耗时{duration:.1f}秒")
    
    def _fail_task(self, task: Task, error_message: str):
        """任务失败"""
        logger.error(f"任务失败: {task.task_id}, 错误: {error_message}")
        
        # 完成结果收集（失败状态）
        if self.current_collector:
            task_result = self.current_collector.finalize(TaskStatus.FAILED.value, error_message)
            self._report_final_result(task.task_id, task_result)
        
        # 更新状态
        self._update_task_status(task.task_id, TaskStatus.FAILED, error_message)
    
    def _cleanup(self):
        """清理资源"""
        try:
            if self.controller:
                self.controller.disconnect()
                self.controller = None
        except Exception as e:
            logger.warning(f"清理控制器资源失败: {e}")
    
    def _update_task_status(self, task_id: str, status: TaskStatus, message: str = None, progress: int = None):
        """更新任务状态"""
        if self.current_collector:
            self.current_collector.add_status_update(status.value, message, progress)
        
        # 发送到WebSocket客户端
        if self.message_sender:
            self.message_sender({
                "type": "TASK_STATUS",
                "taskId": task_id,
                "status": status.value,
                "message": message,
                "progress": progress,
                "timestamp": int(time.time() * 1000)
            })
    
    def _report_progress(self, task_id: str, test_item, result: TestResult):
        """上报执行进度"""
        if self.message_sender:
            self.message_sender({
                "type": "LOG_STREAM",
                "taskId": task_id,
                "level": "INFO",
                "message": f"执行测试项: {test_item.name}",
                "result": result.to_dict(),
                "timestamp": int(time.time() * 1000)
            })
    
    def _report_final_result(self, task_id: str, task_result: TaskResult):
        """上报最终结果"""
        if self.message_sender:
            self.message_sender({
                "type": "RESULT_REPORT",
                "taskId": task_id,
                "status": task_result.status,
                "results": [r.to_dict() for r in task_result.results],
                "summary": task_result.summary,
                "timestamp": int(time.time() * 1000)
            })
    
    def get_current_status(self) -> Dict[str, Any]:
        """获取当前状态"""
        if not self.current_task:
            return {"status": "idle"}
        
        return {
            "status": "running",
            "task_id": self.current_task.task_id,
            "tool_type": self.current_task.tool_type,
            "start_time": datetime.fromtimestamp(self._start_time).isoformat(),
            "duration": time.time() - self._start_time,
            "collector_status": self.current_collector.get_current_status() if self.current_collector else None
        }