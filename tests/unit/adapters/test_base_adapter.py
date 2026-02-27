"""
适配器基类单元测试

测试BaseTestAdapter的通用功能
"""

import pytest
from python_executor.core.adapters import (
    BaseTestAdapter, TestToolType, AdapterStatus
)


class MockAdapter(BaseTestAdapter):
    """Mock适配器用于测试基类"""
    
    @property
    def tool_type(self) -> TestToolType:
        return TestToolType.CANOE
    
    def connect(self) -> bool:
        self.status = AdapterStatus.CONNECTED
        return True
    
    def disconnect(self) -> bool:
        self.status = AdapterStatus.DISCONNECTED
        return True
    
    def load_configuration(self, config_path: str) -> bool:
        return True
    
    def start_test(self) -> bool:
        self.status = AdapterStatus.RUNNING
        return True
    
    def stop_test(self) -> bool:
        self.status = AdapterStatus.CONNECTED
        return True
    
    def execute_test_item(self, item: dict) -> dict:
        return {"status": "passed"}


class TestBaseAdapter:
    """测试适配器基类"""
    
    def test_initial_state(self):
        """测试初始状态"""
        adapter = MockAdapter()
        
        assert adapter.status == AdapterStatus.IDLE
        assert adapter.is_connected is False
        assert adapter.is_running is False
        assert adapter.last_error is None
        assert adapter.tool_type == TestToolType.CANOE
    
    def test_connect_changes_status(self):
        """测试连接改变状态"""
        adapter = MockAdapter()
        
        result = adapter.connect()
        
        assert result is True
        assert adapter.status == AdapterStatus.CONNECTED
        assert adapter.is_connected is True
        assert adapter.is_running is False
    
    def test_start_test_changes_status(self):
        """测试启动测试改变状态"""
        adapter = MockAdapter()
        adapter.connect()
        
        result = adapter.start_test()
        
        assert result is True
        assert adapter.status == AdapterStatus.RUNNING
        assert adapter.is_connected is True
        assert adapter.is_running is True
    
    def test_stop_test_changes_status(self):
        """测试停止测试改变状态"""
        adapter = MockAdapter()
        adapter.connect()
        adapter.start_test()
        
        result = adapter.stop_test()
        
        assert result is True
        assert adapter.status == AdapterStatus.CONNECTED
        assert adapter.is_connected is True
        assert adapter.is_running is False
    
    def test_disconnect_changes_status(self):
        """测试断开连接改变状态"""
        adapter = MockAdapter()
        adapter.connect()
        
        result = adapter.disconnect()
        
        assert result is True
        assert adapter.status == AdapterStatus.DISCONNECTED
        assert adapter.is_connected is False
    
    def test_config_validation_success(self):
        """测试配置验证成功"""
        adapter = MockAdapter(config={"key1": "value1", "key2": "value2"})
        
        result = adapter.validate_config(["key1", "key2"])
        
        assert result is True
        assert adapter.last_error is None
    
    def test_config_validation_failure(self):
        """测试配置验证失败"""
        adapter = MockAdapter(config={"key1": "value1"})
        
        result = adapter.validate_config(["key1", "key2", "key3"])
        
        assert result is False
        assert adapter.last_error is not None
        assert "key2" in adapter.last_error
        assert "key3" in adapter.last_error
        assert adapter.status == AdapterStatus.ERROR
    
    def test_get_status(self):
        """测试获取状态"""
        adapter = MockAdapter()
        adapter.connect()
        
        status = adapter.get_status()
        
        assert status["tool_type"] == "canoe"
        assert status["status"] == "CONNECTED"
        assert status["is_connected"] is True
        assert status["is_running"] is False
        assert status["last_error"] is None
    
    def test_set_error(self):
        """测试设置错误"""
        adapter = MockAdapter()
        
        adapter._set_error("Test error message")
        
        assert adapter.last_error == "Test error message"
        assert adapter.status == AdapterStatus.ERROR
    
    def test_clear_error(self):
        """测试清除错误"""
        adapter = MockAdapter()
        adapter._set_error("Test error")
        
        adapter._clear_error()
        
        assert adapter.last_error is None
        assert adapter.status == AdapterStatus.IDLE
    
    def test_clear_error_when_not_error(self):
        """测试在非错误状态下清除错误"""
        adapter = MockAdapter()
        adapter.connect()  # 设置为CONNECTED状态
        
        adapter._clear_error()
        
        assert adapter.last_error is None
        assert adapter.status == AdapterStatus.CONNECTED  # 状态不应改变


class TestAdapterStatus:
    """测试适配器状态枚举"""
    
    def test_status_values(self):
        """测试状态值"""
        assert AdapterStatus.IDLE.name == "IDLE"
        assert AdapterStatus.CONNECTING.name == "CONNECTING"
        assert AdapterStatus.CONNECTED.name == "CONNECTED"
        assert AdapterStatus.RUNNING.name == "RUNNING"
        assert AdapterStatus.ERROR.name == "ERROR"
        assert AdapterStatus.DISCONNECTED.name == "DISCONNECTED"


class TestTestToolType:
    """测试测试工具类型枚举"""
    
    def test_tool_type_values(self):
        """测试工具类型值"""
        assert TestToolType.CANOE.value == "canoe"
        assert TestToolType.TSMASTER.value == "tsmaster"
        assert TestToolType.TTWORKBENCH.value == "ttworkbench"
    
    def test_tool_type_from_string(self):
        """测试从字符串创建工具类型"""
        assert TestToolType("canoe") == TestToolType.CANOE
        assert TestToolType("tsmaster") == TestToolType.TSMASTER
        assert TestToolType("ttworkbench") == TestToolType.TTWORKBENCH
