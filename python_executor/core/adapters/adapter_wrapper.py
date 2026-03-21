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
        self.last_error: Optional[str] = None

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
        result = self.adapter.load_configuration(config_path)
        if not result:
            # 尝试获取详细错误信息
            if hasattr(self.adapter, 'last_error'):
                self.last_error = self.adapter.last_error
            self.logger.error(f"配置文件加载失败: {self.adapter.last_error if hasattr(self.adapter, 'last_error') else '未知错误'}")
        return result
    
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

    def start_simulation(self, timeout: int = None) -> bool:
        """
        启动仿真（兼容原有接口）

        Args:
            timeout: 超时时间（秒）

        Returns:
            启动成功返回True
        """
        return self.adapter.start_test()
    
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

        if hasattr(self.adapter, 'execute_test_module_direct'):
            return self.adapter.execute_test_module_direct(test_name)

        # 使用通用的execute_test_item方法
        return self.adapter.execute_test_item({"type": "test_module", "name": test_name})

    # ==================== TestModule 直接执行相关方法 ====================

    def get_test_modules(self) -> List[str]:
        """
        获取当前配置中的所有测试模块名称

        Returns:
            测试模块名称列表
        """
        if hasattr(self.adapter, 'get_test_modules'):
            return self.adapter.get_test_modules()

        if hasattr(self.adapter, '_canoe_wrapper'):
            return self.adapter._canoe_wrapper.get_test_modules()

        self.logger.warning("适配器不支持获取测试模块列表")
        return []

    def execute_test_module(self, module_name: str, timeout: int = None) -> Dict[str, Any]:
        """
        直接执行测试模块（不使用命名空间）

        Args:
            module_name: 测试模块名称
            timeout: 超时时间（秒）

        Returns:
            执行结果字典
        """
        if hasattr(self.adapter, 'execute_test_module_direct'):
            return self.adapter.execute_test_module_direct(module_name, timeout)

        # 回退到 execute_test_item
        item = {
            "type": "test_module",
            "name": module_name,
            "timeout": timeout
        }
        if timeout:
            item["timeout"] = timeout

        return self.adapter.execute_test_item(item)
    
    # ==================== 配置驱动用例执行相关方法 ====================
    
    def _get_system_variable(self, var_name: str, namespace: str = "mutualVar"):
        """
        获取CANoe系统变量值

        Args:
            var_name: 变量名称
            namespace: 命名空间名称

        Returns:
            变量值，失败返回None
        """
        try:
            if hasattr(self.adapter, 'get_system_variable'):
                # CANoeAdapter 参数顺序: (namespace, variable)
                return self.adapter.get_system_variable(namespace, var_name)
        except Exception as e:
            self.logger.error(f"获取系统变量失败 [{namespace}.{var_name}]: {e}")

        return None

    def _set_system_variable(self, var_name: str, value: Any, namespace: str = "mutualVar") -> bool:
        """
        设置CANoe系统变量值

        Args:
            var_name: 变量名称
            value: 变量值
            namespace: 命名空间名称

        Returns:
            设置成功返回True
        """
        try:
            if hasattr(self.adapter, 'set_system_variable'):
                # CANoeAdapter 参数顺序: (namespace, variable, value)
                return self.adapter.set_system_variable(namespace, var_name, value)
        except Exception as e:
            self.logger.error(f"设置系统变量失败 [{namespace}.{var_name}]: {e}")

        return False
    
    def set_test_case_name(self, test_case_name: str, namespace: str = "mutualVar") -> bool:
        """
        设置当前测试用例名称到CANoe系统变量

        Args:
            test_case_name: 测试用例名称
            namespace: 命名空间名称

        Returns:
            设置成功返回True
        """
        result = self._set_system_variable("testScriptName", test_case_name, namespace)
        if result:
            self.logger.info(f"设置测试用例名称: {test_case_name}")
        else:
            self.logger.error(f"设置测试用例名称失败: {test_case_name}")
        return result

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
        result = self._set_system_variable(var_name, value, namespace)
        if result:
            self.logger.debug(f"设置变量 {var_name} = {value}")
        return result

    def get_test_variable(self, var_name: str, namespace: str = "mutualVar") -> Any:
        """
        获取CANoe测试变量值

        Args:
            var_name: 变量名称
            namespace: 命名空间名称

        Returns:
            变量值，失败返回None
        """
        value = self._get_system_variable(var_name, namespace)
        if value is not None:
            self.logger.debug(f"获取变量 {var_name} = {value}")
        return value
    
    def start_test_case(self, namespace: str = "mutualVar") -> bool:
        """
        启动当前测试用例
        
        Args:
            namespace: 命名空间名称
            
        Returns:
            启动成功返回True
        """
        try:
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
            end_test_value = self._get_system_variable("endTest", namespace)
            if end_test_value is None:
                return False, None

            is_complete = end_test_value == 1

            if is_complete:
                result_state = self._get_system_variable("testCaseResultState", namespace)

                # 重置结束标志
                self._set_system_variable("endTest", 0, namespace)

                self.logger.info(f"测试用例完成，结果: {result_state}")
                return True, result_state

            return False, None
            
        except Exception as e:
            self.logger.error(f"检查测试完成状态失败: {e}")
            return False, None
    
    def run_test_case_with_config(self, test_case_name: str,
                                   config: Dict[str, Any],
                                   timeout: int = 300) -> Dict[str, Any]:
        """
        使用配置执行单个测试用例

        直接执行 TestModule，不使用命名空间/系统变量

        Args:
            test_case_name: 测试用例名称（对应 TestModule 名称）
            config: 测试配置（保留参数，当前未使用）
            timeout: 超时时间（秒）

        Returns:
            测试结果字典
        """
        try:
            self.logger.info(f"开始执行测试用例: {test_case_name}")

            # 直接执行 TestModule
            result = self.execute_test_module(test_case_name, timeout)

            if result.get("success") or result.get("verdict") == "Passed":
                return {
                    "test_case": test_case_name,
                    "result": "PASS" if result.get("verdict") == "Passed" else result.get("verdict"),
                    "duration": result.get("duration", 0),
                    "success": True
                }
            else:
                return {
                    "error": result.get("error", "测试失败"),
                    "test_case": test_case_name,
                    "verdict": result.get("verdict"),
                    "duration": result.get("duration", 0),
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
                             timeout_per_case: int = 300) -> List[Dict[str, Any]]:
        """
        批量执行测试用例

        Args:
            test_cases: 测试用例列表
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
                    timeout=timeout_per_case
                )
                results.append(result)

        self.logger.info(f"批量执行完成，共执行{len(results)}个结果")
        return results
