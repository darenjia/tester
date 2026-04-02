"""
适配器包装器

包装适配器类，提供与原有 Controller 类相同的接口，便于迁移。
内部按能力拆分为通用控制、CANoe 用例执行、TSMaster RPC 控制三层，
避免继续把所有工具特性堆进一个平面类。
"""
from __future__ import annotations

from typing import Optional, Dict, Any, List

from .base_adapter import BaseTestAdapter


class _AdapterCapabilityBase:
    def __init__(self, wrapper: "AdapterWrapper"):
        self.wrapper = wrapper

    @property
    def adapter(self) -> BaseTestAdapter:
        return self.wrapper.adapter

    @property
    def logger(self):
        return self.wrapper.logger

    def _invoke_supported(self, method_name: str, *args, warn_message: str | None = None, **kwargs):
        method = getattr(self.adapter, method_name, None)
        if callable(method):
            return method(*args, **kwargs)
        if warn_message:
            self.logger.warning(warn_message)
        return None


class _CommonAdapterControl(_AdapterCapabilityBase):
    @property
    def is_connected(self) -> bool:
        return self.adapter.is_connected

    def connect(self) -> bool:
        return self.adapter.connect()

    def disconnect(self) -> bool:
        return self.adapter.disconnect()

    def open_configuration(self, config_path: str) -> bool:
        result = self.adapter.load_configuration(config_path)
        if not result:
            if hasattr(self.adapter, "last_error"):
                self.wrapper.last_error = self.adapter.last_error
            self.logger.error(
                f"配置文件加载失败: {self.adapter.last_error if hasattr(self.adapter, 'last_error') else '未知错误'}"
            )
        return result

    def start_measurement(self, timeout: int = None) -> bool:
        return self.adapter.start_test()

    def stop_measurement(self, timeout: int = 30) -> bool:
        return self.adapter.stop_test()

    def start_simulation(self, timeout: int = None) -> bool:
        return self.adapter.start_test()

    def get_signal(self, signal_name: str) -> Optional[float]:
        result = self._invoke_supported(
            "get_signal",
            signal_name,
            warn_message="适配器不支持get_signal方法",
        )
        return result if result is not None else None

    def set_signal(self, signal_name: str, value: float) -> bool:
        result = self._invoke_supported(
            "set_signal",
            signal_name,
            value,
            warn_message="适配器不支持set_signal方法",
        )
        return bool(result) if result is not None else False

    def run_test_module(self, test_name: str) -> Dict[str, Any]:
        result = self._invoke_supported("run_test_module", test_name)
        if result is not None:
            return result

        result = self._invoke_supported("execute_test_module_direct", test_name)
        if result is not None:
            return result

        return self.adapter.execute_test_item({"type": "test_module", "name": test_name})

    def get_test_modules(self) -> List[str]:
        result = self._invoke_supported("get_test_modules")
        if result is not None:
            return result

        canoe_wrapper = getattr(self.adapter, "_canoe_wrapper", None)
        if canoe_wrapper and hasattr(canoe_wrapper, "get_test_modules"):
            return canoe_wrapper.get_test_modules()

        self.logger.warning("适配器不支持获取测试模块列表")
        return []

    def execute_test_module(self, module_name: str, timeout: int = None) -> Dict[str, Any]:
        result = self._invoke_supported("execute_test_module_direct", module_name, timeout)
        if result is not None:
            return result

        item = {"type": "test_module", "name": module_name, "timeout": timeout}
        return self.adapter.execute_test_item(item)


class _CANoeExecutionControl(_AdapterCapabilityBase):
    def _get_system_variable(self, var_name: str, namespace: str = "mutualVar"):
        try:
            method = getattr(self.adapter, "get_system_variable", None)
            if callable(method):
                return method(namespace, var_name)
        except Exception as exc:
            self.logger.error(f"获取系统变量失败 [{namespace}.{var_name}]: {exc}")
        return None

    def _set_system_variable(self, var_name: str, value: Any, namespace: str = "mutualVar") -> bool:
        try:
            method = getattr(self.adapter, "set_system_variable", None)
            if callable(method):
                return bool(method(namespace, var_name, value))
        except Exception as exc:
            self.logger.error(f"设置系统变量失败 [{namespace}.{var_name}]: {exc}")
        return False

    def set_test_case_name(self, test_case_name: str, namespace: str = "mutualVar") -> bool:
        result = self._set_system_variable("testScriptName", test_case_name, namespace)
        if result:
            self.logger.info(f"设置测试用例名称: {test_case_name}")
        else:
            self.logger.error(f"设置测试用例名称失败: {test_case_name}")
        return result

    def set_test_variable(self, var_name: str, value: Any, namespace: str = "mutualVar") -> bool:
        result = self._set_system_variable(var_name, value, namespace)
        if result:
            self.logger.debug(f"设置变量 {var_name} = {value}")
        return result

    def get_test_variable(self, var_name: str, namespace: str = "mutualVar") -> Any:
        value = self._get_system_variable(var_name, namespace)
        if value is not None:
            self.logger.debug(f"获取变量 {var_name} = {value}")
        return value

    def start_test_case(self, namespace: str = "mutualVar") -> bool:
        try:
            self.set_test_variable("endTest", 0, namespace)
            self.set_test_variable("testCaseResultState", 0, namespace)
            self.logger.debug("已重置 endTest 和 testCaseResultState")

            result = self.set_test_variable("startTest", 1, namespace)
            if result:
                self.logger.info("启动测试用例")
            return result
        except Exception as exc:
            self.logger.error(f"启动测试用例失败: {exc}")
            return False

    def check_test_case_complete(self, namespace: str = "mutualVar") -> tuple[bool, Any]:
        try:
            end_test_value = self._get_system_variable("endTest", namespace)
            if end_test_value is None:
                return False, None

            if end_test_value == 1:
                result_state = self._get_system_variable("testCaseResultState", namespace)
                self._set_system_variable("endTest", 0, namespace)
                self.logger.info(f"测试用例完成，结果: {result_state}")
                return True, result_state

            return False, None
        except Exception as exc:
            self.logger.error(f"检查测试完成状态失败: {exc}")
            return False, None

    def run_test_case_with_config(
        self, test_case_name: str, config: Dict[str, Any], timeout: int = 300
    ) -> Dict[str, Any]:
        try:
            self.logger.info(f"开始执行测试用例: {test_case_name}")
            result = self.wrapper.common.execute_test_module(test_case_name, timeout)

            if result.get("success") or result.get("verdict") == "Passed":
                return {
                    "test_case": test_case_name,
                    "result": "PASS" if result.get("verdict") == "Passed" else result.get("verdict"),
                    "duration": result.get("duration", 0),
                    "success": True,
                }

            return {
                "error": result.get("error", "测试失败"),
                "test_case": test_case_name,
                "verdict": result.get("verdict"),
                "duration": result.get("duration", 0),
                "success": False,
            }
        except Exception as exc:
            self.logger.error(f"执行测试用例失败: {test_case_name}, 错误: {exc}")
            return {"error": str(exc), "test_case": test_case_name, "success": False}

    def run_test_cases_batch(
        self, test_cases: List[Dict[str, Any]], timeout_per_case: int = 300
    ) -> List[Dict[str, Any]]:
        results = []
        total = len(test_cases)

        self.logger.info(f"开始批量执行{total}个测试用例")

        for index, test_case in enumerate(test_cases, 1):
            self.logger.info(f"执行用例 {index}/{total}: {test_case.get('name')}")
            result = self.run_test_case_with_config(
                test_case_name=test_case.get("name"),
                config={"dtc_info": test_case.get("dtc_info"), "params": test_case.get("params", {})},
                timeout=timeout_per_case,
            )
            results.append(result)

            repeat = test_case.get("repeat", 1)
            for repeat_index in range(1, repeat):
                self.logger.info(
                    f"重复执行用例 {test_case.get('name')} - 第{repeat_index + 1}/{repeat}次"
                )
                results.append(
                    self.run_test_case_with_config(
                        test_case_name=f"{test_case.get('name')}@{repeat_index + 1}",
                        config={"dtc_info": test_case.get("dtc_info"), "params": test_case.get("params", {})},
                        timeout=timeout_per_case,
                    )
                )

        self.logger.info(f"批量执行完成，共执行{len(results)}个结果")
        return results


class _TSMasterExecutionControl(_AdapterCapabilityBase):
    def start_test_execution(
        self,
        test_cases: Optional[str] = None,
        wait_for_complete: bool = True,
        timeout: Optional[int] = None,
    ) -> bool:
        result = self._invoke_supported(
            "start_test_execution",
            test_cases=test_cases,
            wait_for_complete=wait_for_complete,
            timeout=timeout,
            warn_message="适配器不支持start_test_execution方法",
        )
        return bool(result) if result is not None else False

    def wait_for_test_complete(self, timeout: Optional[int] = None) -> bool:
        result = self._invoke_supported(
            "wait_for_test_complete",
            timeout=timeout,
            warn_message="适配器不支持wait_for_test_complete方法",
        )
        return bool(result) if result is not None else False

    def get_test_report_info(self) -> Optional[Dict[str, Any]]:
        result = self._invoke_supported(
            "get_test_report_info",
            warn_message="适配器不支持get_test_report_info方法",
        )
        return result if result is not None else None

    def start_master_form(self, form_name: str = None) -> bool:
        result = self._invoke_supported(
            "start_master_form",
            form_name,
            warn_message="适配器不支持start_master_form方法",
        )
        return bool(result) if result is not None else False

    def stop_master_form(self, form_name: str = None) -> bool:
        result = self._invoke_supported(
            "stop_master_form",
            form_name,
            warn_message="适配器不支持stop_master_form方法",
        )
        return bool(result) if result is not None else False


class AdapterWrapper:
    """
    适配器包装器

    将适配器接口包装为与原有 Controller 类相同的接口，便于任务执行器统一调用。
    内部能力拆分后，旧方法名继续保留为 facade，减少生产链路迁移成本。
    """

    def __init__(self, adapter: BaseTestAdapter):
        self.adapter = adapter
        from utils.logger import get_logger

        self.logger = get_logger("adapters.AdapterWrapper")
        self.last_error: Optional[str] = None
        self.common = _CommonAdapterControl(self)
        self.canoe = _CANoeExecutionControl(self)
        self.tsmaster = _TSMasterExecutionControl(self)

    @property
    def is_connected(self) -> bool:
        return self.common.is_connected

    def connect(self) -> bool:
        return self.common.connect()

    def disconnect(self) -> bool:
        return self.common.disconnect()

    def open_configuration(self, config_path: str) -> bool:
        return self.common.open_configuration(config_path)

    def start_measurement(self, timeout: int = None) -> bool:
        return self.common.start_measurement(timeout)

    def stop_measurement(self, timeout: int = 30) -> bool:
        return self.common.stop_measurement(timeout)

    def start_simulation(self, timeout: int = None) -> bool:
        return self.common.start_simulation(timeout)

    def get_signal(self, signal_name: str) -> Optional[float]:
        return self.common.get_signal(signal_name)

    def set_signal(self, signal_name: str, value: float) -> bool:
        return self.common.set_signal(signal_name, value)

    def run_test_module(self, test_name: str) -> Dict[str, Any]:
        return self.common.run_test_module(test_name)

    def get_test_modules(self) -> List[str]:
        return self.common.get_test_modules()

    def execute_test_module(self, module_name: str, timeout: int = None) -> Dict[str, Any]:
        return self.common.execute_test_module(module_name, timeout)

    def _get_system_variable(self, var_name: str, namespace: str = "mutualVar"):
        return self.canoe._get_system_variable(var_name, namespace)

    def _set_system_variable(self, var_name: str, value: Any, namespace: str = "mutualVar") -> bool:
        return self.canoe._set_system_variable(var_name, value, namespace)

    def set_test_case_name(self, test_case_name: str, namespace: str = "mutualVar") -> bool:
        return self.canoe.set_test_case_name(test_case_name, namespace)

    def set_test_variable(self, var_name: str, value: Any, namespace: str = "mutualVar") -> bool:
        return self.canoe.set_test_variable(var_name, value, namespace)

    def get_test_variable(self, var_name: str, namespace: str = "mutualVar") -> Any:
        return self.canoe.get_test_variable(var_name, namespace)

    def start_test_case(self, namespace: str = "mutualVar") -> bool:
        return self.canoe.start_test_case(namespace)

    def check_test_case_complete(self, namespace: str = "mutualVar") -> tuple[bool, Any]:
        return self.canoe.check_test_case_complete(namespace)

    def run_test_case_with_config(
        self, test_case_name: str, config: Dict[str, Any], timeout: int = 300
    ) -> Dict[str, Any]:
        return self.canoe.run_test_case_with_config(test_case_name, config, timeout)

    def run_test_cases_batch(
        self, test_cases: List[Dict[str, Any]], timeout_per_case: int = 300
    ) -> List[Dict[str, Any]]:
        return self.canoe.run_test_cases_batch(test_cases, timeout_per_case)

    def start_test_execution(
        self,
        test_cases: Optional[str] = None,
        wait_for_complete: bool = True,
        timeout: Optional[int] = None,
    ) -> bool:
        return self.tsmaster.start_test_execution(test_cases, wait_for_complete, timeout)

    def wait_for_test_complete(self, timeout: Optional[int] = None) -> bool:
        return self.tsmaster.wait_for_test_complete(timeout)

    def get_test_report_info(self) -> Optional[Dict[str, Any]]:
        return self.tsmaster.get_test_report_info()

    def start_master_form(self, form_name: str = None) -> bool:
        return self.tsmaster.start_master_form(form_name)

    def stop_master_form(self, form_name: str = None) -> bool:
        return self.tsmaster.stop_master_form(form_name)
