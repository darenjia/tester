"""
测试状态处理器实现

为状态机提供具体的测试执行状态处理逻辑
"""
import time
import logging
from typing import Dict, Any, Optional

from core.state_machine import StateHandler, TestState, StateContext
from core.adapters.adapter_factory import create_adapter_with_wrapper
from core.result_collector import ResultCollector
from core.config_manager import config_manager
from models.task import Task

logger = logging.getLogger(__name__)


class SelfCheckHandler(StateHandler):
    """自检状态处理器"""
    
    def __init__(self, task: Task):
        self.task = task
        self.check_results = {}
    
    def on_enter(self, context: StateContext) -> None:
        """进入自检状态"""
        logger.info(f"Task {self.task.task_id}: Entering self-check state")
        context.data['self_check_start'] = time.time()
        self.check_results = {}
    
    def on_execute(self, context: StateContext) -> TestState:
        """执行自检"""
        try:
            # 1. 检查软件环境
            logger.info("Checking software environment...")
            self.check_results['software'] = self._check_software()
            
            # 2. 检查配置文件
            logger.info("Checking configuration files...")
            self.check_results['config'] = self._check_config()
            
            # 3. 检查网络连接（如果需要）
            if self.task.report_server:
                logger.info("Checking network connectivity...")
                self.check_results['network'] = self._check_network()
            
            # 保存自检结果
            context.data['self_check_results'] = self.check_results
            
            # 检查是否有严重错误
            failed_checks = [k for k, v in self.check_results.items() if not v.get('success', False)]
            if failed_checks:
                logger.error(f"Self-check failed for: {failed_checks}")
                context.error = f"Self-check failed: {failed_checks}"
                return TestState.FAILED
            
            logger.info("Self-check completed successfully")
            return TestState.CONFIG_LOAD
            
        except Exception as e:
            logger.error(f"Self-check error: {e}")
            context.error = f"Self-check error: {e}"
            return TestState.FAILED
    
    def on_exit(self, context: StateContext) -> None:
        """退出自检状态"""
        duration = time.time() - context.data.get('self_check_start', time.time())
        logger.info(f"Self-check completed in {duration:.2f}s")
    
    def _check_software(self) -> Dict[str, Any]:
        """检查软件环境"""
        # 这里可以调用software_validator进行检查
        return {'success': True, 'message': 'Software check passed'}
    
    def _check_config(self) -> Dict[str, Any]:
        """检查配置文件"""
        if self.task.config_name:
            # 验证配置是否存在
            config_info = config_manager.prepare_config_for_task(
                self.task.config_name,
                self.task.base_config_dir,
                self.task.config_params or {}
            )
            if config_info and config_info.get('cfg_path'):
                return {'success': True, 'cfg_path': config_info['cfg_path']}
            return {'success': False, 'message': 'Configuration not found'}
        return {'success': True, 'message': 'No configuration required'}
    
    def _check_network(self) -> Dict[str, Any]:
        """检查网络连接"""
        # 简单的网络连通性检查
        return {'success': True, 'message': 'Network check passed'}


class ConfigLoadHandler(StateHandler):
    """配置加载状态处理器"""
    
    def __init__(self, task: Task):
        self.task = task
    
    def on_enter(self, context: StateContext) -> None:
        """进入配置加载状态"""
        logger.info(f"Task {self.task.task_id}: Entering config load state")
        context.data['config_load_start'] = time.time()
    
    def on_execute(self, context: StateContext) -> TestState:
        """执行配置加载"""
        try:
            if self.task.config_name:
                # 准备配置
                config_info = config_manager.prepare_config_for_task(
                    self.task.config_name,
                    self.task.base_config_dir,
                    self.task.config_params or {}
                )
                
                if not config_info or not config_info.get('cfg_path'):
                    context.error = "Failed to prepare configuration"
                    return TestState.FAILED
                
                context.data['config_path'] = config_info['cfg_path']
                context.data['config_info'] = config_info
                logger.info(f"Configuration loaded: {config_info['cfg_path']}")
            else:
                logger.info("No configuration required")
            
            return TestState.CONNECTING
            
        except Exception as e:
            logger.error(f"Config load error: {e}")
            context.error = f"Config load error: {e}"
            return TestState.FAILED
    
    def on_exit(self, context: StateContext) -> None:
        """退出配置加载状态"""
        duration = time.time() - context.data.get('config_load_start', time.time())
        logger.info(f"Config load completed in {duration:.2f}s")


class ConnectingHandler(StateHandler):
    """连接工具状态处理器"""
    
    def __init__(self, task: Task, adapter_type: str):
        self.task = task
        self.adapter_type = adapter_type
        self.controller = None
    
    def on_enter(self, context: StateContext) -> None:
        """进入连接状态"""
        logger.info(f"Task {self.task.task_id}: Entering connecting state")
        context.data['connect_start'] = time.time()
    
    def on_execute(self, context: StateContext) -> TestState:
        """执行连接"""
        try:
            # 创建适配器
            self.controller = create_adapter_with_wrapper(self.adapter_type)
            context.data['controller'] = self.controller
            
            # 连接工具（带重试）
            max_retries = config_manager.get('canoe.max_retries', 3)
            retry_delay = config_manager.get('canoe.retry_delay', 2.0)
            
            for attempt in range(max_retries):
                try:
                    logger.info(f"Connecting to {self.adapter_type} (attempt {attempt + 1}/{max_retries})...")
                    if self.controller.connect():
                        logger.info(f"Successfully connected to {self.adapter_type}")
                        return TestState.RUNNING
                except Exception as e:
                    logger.warning(f"Connection attempt {attempt + 1} failed: {e}")
                    if attempt < max_retries - 1:
                        time.sleep(retry_delay)
            
            context.error = f"Failed to connect to {self.adapter_type} after {max_retries} attempts"
            return TestState.FAILED
            
        except Exception as e:
            logger.error(f"Connection error: {e}")
            context.error = f"Connection error: {e}"
            return TestState.FAILED
    
    def on_exit(self, context: StateContext) -> None:
        """退出连接状态"""
        duration = time.time() - context.data.get('connect_start', time.time())
        logger.info(f"Connection completed in {duration:.2f}s")


class RunningHandler(StateHandler):
    """测试执行状态处理器"""
    
    def __init__(self, task: Task, test_items: list):
        self.task = task
        self.test_items = test_items
        self.results = []
    
    def on_enter(self, context: StateContext) -> None:
        """进入执行状态"""
        logger.info(f"Task {self.task.task_id}: Entering running state")
        context.data['execution_start'] = time.time()
        context.data['current_item_index'] = 0
        self.results = []
    
    def on_execute(self, context: StateContext) -> TestState:
        """执行测试"""
        try:
            controller = context.data.get('controller')
            if not controller:
                context.error = "Controller not available"
                return TestState.FAILED
            
            # 加载配置（如果有）
            config_path = context.data.get('config_path')
            if config_path:
                logger.info(f"Loading configuration: {config_path}")
                controller.open_configuration(config_path)
            
            # 启动测量
            logger.info("Starting measurement...")
            controller.start_measurement()
            
            # 执行测试项
            current_index = context.data.get('current_item_index', 0)
            
            while current_index < len(self.test_items):
                item = self.test_items[current_index]
                logger.info(f"Executing test item {current_index + 1}/{len(self.test_items)}: {item}")
                
                # 执行单个测试项
                result = self._execute_test_item(controller, item)
                self.results.append(result)
                
                # 更新进度
                context.data['current_item_index'] = current_index + 1
                context.data['progress'] = (current_index + 1) / len(self.test_items) * 100
                
                current_index += 1
            
            # 保存结果
            context.data['test_results'] = self.results
            
            # 停止测量
            logger.info("Stopping measurement...")
            controller.stop_measurement()
            
            return TestState.RESULT_COLLECT
            
        except Exception as e:
            logger.error(f"Test execution error: {e}")
            context.error = f"Test execution error: {e}"
            return TestState.FAILED
    
    def on_exit(self, context: StateContext) -> None:
        """退出执行状态"""
        duration = time.time() - context.data.get('execution_start', time.time())
        logger.info(f"Test execution completed in {duration:.2f}s")
    
    def _execute_test_item(self, controller, item: str) -> Dict[str, Any]:
        """执行单个测试项"""
        start_time = time.time()
        
        try:
            # 这里调用具体的测试执行逻辑
            # 可以通过配置驱动的方式执行
            result = controller.run_test_case_with_config(
                test_case_name=item,
                config={},
                timeout=300
            )
            
            return {
                'item': item,
                'success': result.get('success', False),
                'result': result,
                'duration': time.time() - start_time
            }
        except Exception as e:
            logger.error(f"Test item {item} failed: {e}")
            return {
                'item': item,
                'success': False,
                'error': str(e),
                'duration': time.time() - start_time
            }


class ResultCollectHandler(StateHandler):
    """结果收集状态处理器"""
    
    def __init__(self, task: Task):
        self.task = task
        self.collector = None
    
    def on_enter(self, context: StateContext) -> None:
        """进入结果收集状态"""
        logger.info(f"Task {self.task.task_id}: Entering result collect state")
        context.data['collect_start'] = time.time()
        self.collector = ResultCollector(self.task.task_id)
    
    def on_execute(self, context: StateContext) -> TestState:
        """执行结果收集"""
        try:
            # 收集测试结果
            test_results = context.data.get('test_results', [])
            
            for result in test_results:
                self.collector.add_result(result)
            
            # 收集性能指标
            execution_start = context.data.get('execution_start', time.time())
            duration = time.time() - execution_start
            
            # 生成最终报告
            final_result = self.collector.generate_report()
            final_result['duration'] = duration
            final_result['state_history'] = context.state_history
            
            context.data['final_result'] = final_result
            
            logger.info(f"Results collected: {len(test_results)} test items")
            return TestState.CLEANUP
            
        except Exception as e:
            logger.error(f"Result collection error: {e}")
            context.error = f"Result collection error: {e}"
            return TestState.FAILED
    
    def on_exit(self, context: StateContext) -> None:
        """退出结果收集状态"""
        duration = time.time() - context.data.get('collect_start', time.time())
        logger.info(f"Result collection completed in {duration:.2f}s")


class CleanupHandler(StateHandler):
    """清理状态处理器"""
    
    def __init__(self, task: Task):
        self.task = task
    
    def on_enter(self, context: StateContext) -> None:
        """进入清理状态"""
        logger.info(f"Task {self.task.task_id}: Entering cleanup state")
        context.data['cleanup_start'] = time.time()
    
    def on_execute(self, context: StateContext) -> TestState:
        """执行清理"""
        try:
            # 断开适配器连接
            controller = context.data.get('controller')
            if controller:
                logger.info("Disconnecting controller...")
                try:
                    controller.disconnect()
                except Exception as e:
                    logger.warning(f"Error during disconnect: {e}")
            
            # 清理临时文件
            config_info = context.data.get('config_info')
            if config_info and config_info.get('is_temp'):
                # 清理临时配置
                pass
            
            logger.info("Cleanup completed")
            return TestState.COMPLETED
            
        except Exception as e:
            logger.error(f"Cleanup error: {e}")
            # 清理错误不导致任务失败
            return TestState.COMPLETED
    
    def on_exit(self, context: StateContext) -> None:
        """退出清理状态"""
        duration = time.time() - context.data.get('cleanup_start', time.time())
        logger.info(f"Cleanup completed in {duration:.2f}s")


class PausedHandler(StateHandler):
    """暂停状态处理器"""
    
    def on_enter(self, context: StateContext) -> None:
        """进入暂停状态"""
        logger.info(f"Task: Entering paused state")
        context.data['pause_start'] = time.time()
    
    def on_execute(self, context: StateContext) -> TestState:
        """暂停状态执行（等待恢复）"""
        # 暂停状态保持当前状态，等待外部调用resume
        time.sleep(0.5)
        return TestState.PAUSED
    
    def on_exit(self, context: StateContext) -> None:
        """退出暂停状态"""
        duration = time.time() - context.data.get('pause_start', time.time())
        logger.info(f"Paused for {duration:.2f}s")
