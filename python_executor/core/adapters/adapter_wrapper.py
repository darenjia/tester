"""
适配器包装器

兼容层只保留最小通用控制方法和 TSMaster 控制方法。
CANoe 的配置驱动/系统变量 facade 已彻底移除，执行语义交给 strategy + capability。
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

    def run_test_module(self, test_name: str) -> Dict[str, Any]:
        result = self._invoke_supported("run_test_module", test_name)
        if result is not None:
            return result

        result = self._invoke_supported("execute_test_module_direct", test_name)
        if result is not None:
            return result

        self.logger.warning("适配器不支持TestModule兼容执行入口")
        return {
            "success": False,
            "verdict": "ERROR",
            "error": "适配器不支持TestModule兼容执行入口",
            "module": test_name,
        }

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

        self.logger.warning("适配器不支持execute_test_module兼容入口")
        return {
            "success": False,
            "verdict": "ERROR",
            "error": "适配器不支持execute_test_module兼容入口",
            "module": module_name,
        }


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

    保留少量兼容接口，新的主执行链路应优先通过 capability 和 strategy 工作。
    """

    def __init__(self, adapter: BaseTestAdapter):
        self.adapter = adapter
        from utils.logger import get_logger

        self.logger = get_logger("adapters.AdapterWrapper")
        self.last_error: Optional[str] = None
        self.common = _CommonAdapterControl(self)
        self.tsmaster = _TSMasterExecutionControl(self)

    def get_capability(self, name: str, default=None):
        """Forward capability lookups to the underlying adapter."""
        if hasattr(self.adapter, "get_capability"):
            return self.adapter.get_capability(name, default)
        return default

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

    def run_test_module(self, test_name: str) -> Dict[str, Any]:
        return self.common.run_test_module(test_name)

    def get_test_modules(self) -> List[str]:
        return self.common.get_test_modules()

    def execute_test_module(self, module_name: str, timeout: int = None) -> Dict[str, Any]:
        return self.common.execute_test_module(module_name, timeout)

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
