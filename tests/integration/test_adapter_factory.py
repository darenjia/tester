"""
适配器工厂集成测试

测试适配器工厂和适配器注册表
"""

import pytest
from python_executor.core.adapters import (
    create_adapter, ADAPTER_REGISTRY,
    TestToolType, CANoeAdapter, TSMasterAdapter, TTworkbenchAdapter
)


class TestAdapterFactory:
    """测试适配器工厂"""
    
    def test_create_canoe_adapter(self):
        """测试创建CANoe适配器"""
        config = {"start_timeout": 30}
        
        adapter = create_adapter(TestToolType.CANOE, config)
        
        assert isinstance(adapter, CANoeAdapter)
        assert adapter.tool_type == TestToolType.CANOE
        assert adapter.config == config
    
    def test_create_tsmaster_adapter(self):
        """测试创建TSMaster适配器"""
        config = {"timeout": 60}
        
        adapter = create_adapter(TestToolType.TSMASTER, config)
        
        assert isinstance(adapter, TSMasterAdapter)
        assert adapter.tool_type == TestToolType.TSMASTER
        assert adapter.config == config
    
    def test_create_ttworkbench_adapter(self):
        """测试创建TTworkbench适配器"""
        config = {
            "ttman_path": "C:/Spirent/TTman.bat",
            "workspace_path": "C:/Workspace"
        }
        
        adapter = create_adapter(TestToolType.TTWORKBENCH, config)
        
        assert isinstance(adapter, TTworkbenchAdapter)
        assert adapter.tool_type == TestToolType.TTWORKBENCH
        assert adapter.config == config
    
    def test_create_adapter_without_config(self):
        """测试创建适配器（无配置）"""
        adapter = create_adapter(TestToolType.CANOE)
        
        assert isinstance(adapter, CANoeAdapter)
        assert adapter.config == {}
    
    def test_create_adapter_invalid_type(self):
        """测试创建适配器（无效类型）"""
        class InvalidType:
            pass
        
        with pytest.raises(ValueError) as exc_info:
            create_adapter(InvalidType())
        
        assert "不支持的测试工具类型" in str(exc_info.value)
    
    def test_adapter_registry_contents(self):
        """测试适配器注册表内容"""
        assert TestToolType.CANOE in ADAPTER_REGISTRY
        assert TestToolType.TSMASTER in ADAPTER_REGISTRY
        assert TestToolType.TTWORKBENCH in ADAPTER_REGISTRY
        
        assert ADAPTER_REGISTRY[TestToolType.CANOE] == CANoeAdapter
        assert ADAPTER_REGISTRY[TestToolType.TSMASTER] == TSMasterAdapter
        assert ADAPTER_REGISTRY[TestToolType.TTWORKBENCH] == TTworkbenchAdapter
