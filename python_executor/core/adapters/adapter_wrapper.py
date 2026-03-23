"""
适配器包装器

包装适配器类，提供与原有Controller类相同的接口，便于迁移
"""
import time
import logging
from typing import Optional, Dict, Any, List

from .base_adapter import BaseTestAdapter, TestToolType


class AdapterWrapper:
    """
    适配器包装器
    
    将适配器接口包装为与原有Controller类相同的接口
    便于任务执行器统一调用
    """
    
    def __init__(self, adapter: BaseTestAdapter):
        """
        初始化包装器
        
        Args:
            adapter: 适配器实例
        """
        self.adapter = adapter
        self.logger = logging.getLogger(self.__class__.__name__)
        
    @property
    def is_connected(self) -> bool:
        """检查是否已连接"""
        return self.adapter.is_connected
    
    def connect(self) -> bool:
        """
        连接测试工具
        
        Returns:
            连接成功返回True
        """
        return self.adapter.connect()
    
    def disconnect(self) -> bool:
        """
        断开连接
        
        Returns:
            断开成功返回True
        """
        return self.adapter.disconnect()
    
    def open_configuration(self, config_path: str) -> bool:
        """
        加载配置文件（兼容原有接口）
        
        Args:
            config_path: 配置文件路径
            
        Returns:
            加载成功返回True
        """
        return self.adapter.load_configuration(config_path)
    
    def start_measurement(self, timeout: int = None) -> bool:
        """
        启动测量（兼容原有接口）
        
        Args:
            timeout: 超时时间（秒）
            
        Returns:
            启动成功返回True
        """
        return self.adapter.start_test()
    
    def stop_measurement(self, timeout: int = 30) -> bool:
        """
        停止测量（兼容原有接口）
        
        Args:
            timeout: 超时时间（秒）
            
        Returns:
            停止成功返回True
        """
        return self.adapter.stop_test()
    
    def get_signal(self, signal_name: str) -> Optional[float]:
        """
        获取信号值
        
        Args:
            signal_name: 信号名称
            
        Returns:
            信号值，失败返回None
        """
        if hasattr(self.adapter, 'get_signal'):
            return self.adapter.get_signal(signal_name)
        self.logger.warning("适配器不支持get_signal方法")
        return None
    
    def set_signal(self, signal_name: str, value: float) -> bool:
        """
        设置信号值
        
        Args:
            signal_name: 信号名称
            value: 信号值
            
        Returns:
            设置成功返回True
        """
        if hasattr(self.adapter, 'set_signal'):
            return self.adapter.set_signal(signal_name, value)
        self.logger.warning("适配器不支持set_signal方法")
        return False
    
    def run_test_module(self, test_name: str) -> Dict[str, Any]:
        """
        执行测试模块
        
        Args:
            test_name: 测试模块名称
            
        Returns:
            测试结果字典
        """
        if hasattr(self.adapter, 'run_test_module'):
            return self.adapter.run_test_module(test_name)
        
        # 使用通用的execute_test_item方法
        return self.adapter.execute_test_item({"type": "test_module", "name": test_name})
    
    # ==================== 配置驱动用例执行相关方法 ====================
    
    def _get_system_variable(self, var_name: str, namespace: str = "mutualVar"):
        """
        获取CANoe系统变量
        
        Args:
            var_name: 变量名称
            namespace: 命名空间名称
            
        Returns:
            变量值，失败返回None
        """
        if hasattr(self.adapter, 'get_system_variable'):
            return self.adapter.get_system_variable(var_name, namespace)
        
        # 尝试通过COM接口直接访问
        if hasattr(self.adapter, '_app'):
            try:
                system = self.adapter._app.System
                namespaces = system.Namespaces
                ns = namespaces.Item(namespace)
                if ns:
                    variables = ns.Variables
                    var = variables.Item(var_name)
                    return var
            except Exception as e:
                self.logger.error(f"获取系统变量失败 {var_name}: {e}")
        
        return None
    
    def set_test_case_name(self, test_case_name: str, namespace: str = "mutualVar") -> bool:
        """
        设置当前测试用例名称到CANoe系统变量
        
        Args:
            test_case_name: 测试用例名称
            namespace: 命名空间名称
            
        Returns:
            设置成功返回True
        """
        try:
            var = self._get_system_variable("testScriptName", namespace)
            if var:
                var.Value = test_case_name
                self.logger.info(f"设置测试用例名称: {test_case_name}")
                return True
            return False
        except Exception as e:
            self.logger.error(f"设置测试用例名称失败: {e}")
            return False
    
    def set_test_variable(self, var_name: str, value: Any, namespace: str = "mutualVar") -> bool:
        """
        设置CANoe测试变量值
        
        Args:
            var_name: 变量名称
            value: 变量值
            namespace: 命名空间名称
            
        Returns:
            设置成功返回True
        """
        try:
            var = self._get_system_variable(var_name, namespace)
            if var:
                var.Value = value
                self.logger.debug(f"设置变量 {var_name} = {value}")
                return True
            return False
        except Exception as e:
            self.logger.error(f"设置变量失败 {var_name}: {e}")
            return False
    
    def get_test_variable(self, var_name: str, namespace: str = "mutualVar") -> Any:
        """
        获取CANoe测试变量值
        
        Args:
            var_name: 变量名称
            namespace: 命名空间名称
            
        Returns:
            变量值，失败返回None
        """
        try:
            var = self._get_system_variable(var_name, namespace)
            if var:
                value = var.Value
                self.logger.debug(f"获取变量 {var_name} = {value}")
                return value
            return None
        except Exception as e:
            self.logger.error(f"获取变量失败 {var_name}: {e}")
            return None
    
    def start_test_case(self, namespace: str = "mutualVar") -> bool:
        """
        启动当前测试用例

        Args:
            namespace: 命名空间名称

        Returns:
            启动成功返回True
        """
        try:
            # 关键修复：先重置结束标志，避免上一次测试的endTest=1导致立即完成
            self.set_test_variable("endTest", 0, namespace)
            self.set_test_variable("testCaseResultState", 0, namespace)
            self.logger.debug("已重置 endTest 和 testCaseResultState")

            result = self.set_test_variable("startTest", 1, namespace)
            if result:
                self.logger.info("启动测试用例")
            return result
        except Exception as e:
            self.logger.error(f"启动测试用例失败: {e}")
            return False
    
    def check_test_case_complete(self, namespace: str = "mutualVar") -> tuple:
        """
        检查测试用例是否执行完成
        
        Args:
            namespace: 命名空间名称
            
        Returns:
            tuple: (是否完成, 结果状态)
        """
        try:
            end_test_var = self._get_system_variable("endTest", namespace)
            if not end_test_var:
                return False, None
            
            is_complete = end_test_var.Value == 1
            
            if is_complete:
                result_var = self._get_system_variable("testCaseResultState", namespace)
                result_state = result_var.Value if result_var else None
                
                # 重置结束标志
                end_test_var.Value = 0
                
                self.logger.info(f"测试用例完成，结果: {result_state}")
                return True, result_state
            
            return False, None
            
        except Exception as e:
            self.logger.error(f"检查测试完成状态失败: {e}")
            return False, None
    
    def run_test_case_with_config(self, test_case_name: str,
                                   config: Dict[str, Any],
                                   namespace: str = "mutualVar",
                                   timeout: int = 300) -> Dict[str, Any]:
        """
        使用配置执行单个测试用例
        
        Args:
            test_case_name: 测试用例名称
            config: 测试配置
            namespace: 命名空间名称
            timeout: 超时时间（秒）
            
        Returns:
            测试结果字典
        """
        try:
            self.logger.info(f"开始执行测试用例: {test_case_name}")
            
            # 1. 设置测试用例名称
            if not self.set_test_case_name(test_case_name, namespace):
                return {"error": "设置用例名称失败", "test_case": test_case_name}
            
            # 2. 设置DTC信息（如果有）
            dtc_info = config.get('dtc_info')
            if dtc_info:
                self.set_test_variable("dtcTestInformation", dtc_info, namespace)
                self.logger.info(f"设置DTC信息: {dtc_info}")
            
            # 3. 设置其他测试参数
            params = config.get('params', {})
            for key, value in params.items():
                self.set_test_variable(key, value, namespace)
            
            # 4. 启动测试
            if not self.start_test_case(namespace):
                return {"error": "启动测试失败", "test_case": test_case_name}
            
            # 5. 等待测试完成
            start_time = time.time()
            check_interval = 0.5
            
            while time.time() - start_time < timeout:
                is_complete, result = self.check_test_case_complete(namespace)
                if is_complete:
                    duration = time.time() - start_time
                    self.logger.info(f"测试用例执行完成: {test_case_name}, 耗时: {duration:.1f}秒, 结果: {result}")
                    return {
                        "test_case": test_case_name,
                        "result": result,
                        "duration": duration,
                        "success": True
                    }
                time.sleep(check_interval)
            
            # 超时
            self.logger.warning(f"测试用例超时: {test_case_name}")
            return {
                "error": "测试超时",
                "test_case": test_case_name,
                "timeout": timeout,
                "success": False
            }
            
        except Exception as e:
            self.logger.error(f"执行测试用例失败: {test_case_name}, 错误: {e}")
            return {
                "error": str(e),
                "test_case": test_case_name,
                "success": False
            }
    
    def run_test_cases_batch(self, test_cases: List[Dict[str, Any]],
                             namespace: str = "mutualVar",
                             timeout_per_case: int = 300) -> List[Dict[str, Any]]:
        """
        批量执行测试用例
        
        Args:
            test_cases: 测试用例列表
            namespace: 命名空间名称
            timeout_per_case: 每个用例的超时时间
            
        Returns:
            所有用例的执行结果
        """
        results = []
        total = len(test_cases)
        
        self.logger.info(f"开始批量执行{total}个测试用例")
        
        for i, tc in enumerate(test_cases, 1):
            self.logger.info(f"执行用例 {i}/{total}: {tc.get('name')}")
            
            result = self.run_test_case_with_config(
                test_case_name=tc.get('name'),
                config={
                    'dtc_info': tc.get('dtc_info'),
                    'params': tc.get('params', {})
                },
                namespace=namespace,
                timeout=timeout_per_case
            )
            results.append(result)
            
            # 检查是否有重复执行需求
            repeat = tc.get('repeat', 1)
            for r in range(1, repeat):
                self.logger.info(f"重复执行用例 {tc.get('name')} - 第{r+1}/{repeat}次")
                result = self.run_test_case_with_config(
                    test_case_name=f"{tc.get('name')}@{r+1}",
                    config={
                        'dtc_info': tc.get('dtc_info'),
                        'params': tc.get('params', {})
                    },
                    namespace=namespace,
                    timeout=timeout_per_case
                )
                results.append(result)
        
        self.logger.info(f"批量执行完成，共执行{len(results)}个结果")
        return results
