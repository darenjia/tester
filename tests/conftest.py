"""
Pytest配置文件

提供测试共享的fixture和配置
"""

import pytest
import sys
import os
from pathlib import Path
from unittest.mock import MagicMock, Mock
from typing import Generator

# 添加项目根目录到Python路径
project_root = Path(__file__).parent.parent
sys.path.insert(0, str(project_root))

# 导入被测模块
from python_executor.core.adapters import (
    BaseTestAdapter, TestToolType, AdapterStatus,
    CANoeAdapter, TSMasterAdapter, TTworkbenchAdapter
)
from python_executor.core.task_executor_v2 import TaskExecutorV2, TaskStatus


# =============================================================================
# 基础Fixture
# =============================================================================

@pytest.fixture
def mock_message_sender():
    """Mock消息发送函数"""
    return MagicMock()


@pytest.fixture
def task_executor(mock_message_sender) -> TaskExecutorV2:
    """创建任务执行引擎实例"""
    return TaskExecutorV2(mock_message_sender)


@pytest.fixture
def sample_task_config():
    """示例任务配置"""
    return {
        "taskId": "TEST_TASK_001",
        "deviceId": "DEVICE_001",
        "toolType": "canoe",
        "configPath": "C:/Test/config.cfg",
        "testItems": [
            {
                "name": "信号检查测试",
                "type": "signal_check",
                "channel": 1,
                "signal": "EngineSpeed",
                "expected_value": 3000.0,
                "tolerance": 100.0
            },
            {
                "name": "信号设置测试",
                "type": "signal_set",
                "channel": 1,
                "signal": "VehicleSpeed",
                "value": 60.0
            }
        ],
        "adapterConfig": {
            "start_timeout": 30,
            "stop_timeout": 10
        },
        "timeout": 300
    }


@pytest.fixture
def sample_ttworkbench_task():
    """TTworkbench示例任务配置"""
    return {
        "taskId": "TEST_TASK_002",
        "deviceId": "DEVICE_002",
        "toolType": "ttworkbench",
        "configPath": "C:/Test/test.clf",
        "testItems": [
            {
                "name": "TC8测试",
                "type": "clf_test",
                "clf_file": "C:/Test/TC8_001.clf"
            }
        ],
        "adapterConfig": {
            "ttman_path": "C:/Spirent/TTman.bat",
            "workspace_path": "C:/Workspace",
            "log_path": "C:/Logs",
            "report_path": "C:/Reports",
            "report_format": "pdf",
            "timeout": 3600
        },
        "timeout": 3600
    }


# =============================================================================
# 适配器Mock Fixture
# =============================================================================

@pytest.fixture
def mock_canoe_adapter(mocker):
    """Mock CANoe适配器"""
    adapter = MagicMock(spec=CANoeAdapter)
    adapter.tool_type = TestToolType.CANOE
    adapter.status = AdapterStatus.IDLE
    adapter.is_connected = False
    adapter.is_running = False
    adapter.last_error = None
    
    # 设置默认返回值
    adapter.connect.return_value = True
    adapter.disconnect.return_value = True
    adapter.load_configuration.return_value = True
    adapter.start_test.return_value = True
    adapter.stop_test.return_value = True
    adapter.get_status.return_value = {
        "tool_type": "canoe",
        "status": "IDLE",
        "is_connected": False,
        "is_running": False
    }
    
    return adapter


@pytest.fixture
def mock_tsmaster_adapter(mocker):
    """Mock TSMaster适配器"""
    adapter = MagicMock(spec=TSMasterAdapter)
    adapter.tool_type = TestToolType.TSMASTER
    adapter.status = AdapterStatus.IDLE
    adapter.is_connected = False
    adapter.is_running = False
    adapter.last_error = None
    
    adapter.connect.return_value = True
    adapter.disconnect.return_value = True
    adapter.load_configuration.return_value = True
    adapter.start_test.return_value = True
    adapter.stop_test.return_value = True
    adapter.get_status.return_value = {
        "tool_type": "tsmaster",
        "status": "IDLE",
        "is_connected": False,
        "is_running": False
    }
    
    return adapter


@pytest.fixture
def mock_ttworkbench_adapter(mocker):
    """Mock TTworkbench适配器"""
    adapter = MagicMock(spec=TTworkbenchAdapter)
    adapter.tool_type = TestToolType.TTWORKBENCH
    adapter.status = AdapterStatus.IDLE
    adapter.is_connected = False
    adapter.is_running = False
    adapter.last_error = None
    
    adapter.connect.return_value = True
    adapter.disconnect.return_value = True
    adapter.load_configuration.return_value = True
    adapter.start_test.return_value = True
    adapter.stop_test.return_value = True
    adapter.get_status.return_value = {
        "tool_type": "ttworkbench",
        "status": "IDLE",
        "is_connected": False,
        "is_running": False
    }
    
    return adapter


# =============================================================================
# 测试数据Fixture
# =============================================================================

@pytest.fixture
def sample_test_items():
    """示例测试项列表"""
    return [
        {
            "name": "信号检查",
            "type": "signal_check",
            "channel": 1,
            "signal": "EngineSpeed",
            "expected_value": 3000.0
        },
        {
            "name": "信号设置",
            "type": "signal_set",
            "channel": 1,
            "signal": "VehicleSpeed",
            "value": 60.0
        },
        {
            "name": "系统变量检查",
            "type": "sysvar_check",
            "namespace": "Engine",
            "variable": "Temperature",
            "expected_value": 90.0
        },
        {
            "name": "系统变量设置",
            "type": "sysvar_set",
            "namespace": "Engine",
            "variable": "Mode",
            "value": 1
        }
    ]


@pytest.fixture
def sample_signal_check_result():
    """示例信号检查结果"""
    return {
        "name": "信号检查",
        "type": "signal_check",
        "channel": 1,
        "signal": "EngineSpeed",
        "expected_value": 3000.0,
        "actual_value": 2998.5,
        "tolerance": 100.0,
        "passed": True,
        "status": "passed"
    }


@pytest.fixture
def sample_signal_set_result():
    """示例信号设置结果"""
    return {
        "name": "信号设置",
        "type": "signal_set",
        "channel": 1,
        "signal": "VehicleSpeed",
        "value": 60.0,
        "success": True,
        "status": "passed"
    }


@pytest.fixture
def sample_ttworkbench_result():
    """示例TTworkbench测试结果"""
    return {
        "name": "TC8测试",
        "type": "clf_test",
        "clf_file": "C:/Test/TC8_001.clf",
        "test_case_name": "TC8_001",
        "command": "TTman.bat --data C:/Workspace --log C:/Logs -r pdf --report-dir C:/Reports C:/Test/TC8_001.clf",
        "return_code": 0,
        "stdout": "Test execution finished successfully",
        "stderr": "",
        "execution_time": 125.5,
        "report_file": "C:/Reports/TC8_001.pdf",
        "log_file": "C:/Logs/TC8_001.tlz",
        "status": "passed"
    }


# =============================================================================
# 配置Fixture
# =============================================================================

@pytest.fixture
def temp_config_file(tmp_path):
    """创建临时配置文件"""
    config_file = tmp_path / "test_config.cfg"
    config_file.write_text("// Test CANoe configuration\n")
    return str(config_file)


@pytest.fixture
def temp_clf_file(tmp_path):
    """创建临时clf文件"""
    clf_file = tmp_path / "test.clf"
    clf_file.write_text("// Test CLF file\n")
    return str(clf_file)


# =============================================================================
# Pytest配置
# =============================================================================

def pytest_configure(config):
    """Pytest配置"""
    config.addinivalue_line(
        "markers", "unit: 单元测试"
    )
    config.addinivalue_line(
        "markers", "integration: 集成测试"
    )
    config.addinivalue_line(
        "markers", "functional: 功能测试（需要真实环境）"
    )
    config.addinivalue_line(
        "markers", "canoe: CANoe相关测试"
    )
    config.addinivalue_line(
        "markers", "tsmaster: TSMaster相关测试"
    )
    config.addinivalue_line(
        "markers", "ttworkbench: TTworkbench相关测试"
    )
    config.addinivalue_line(
        "markers", "slow: 耗时较长的测试"
    )


def pytest_collection_modifyitems(config, items):
    """修改测试项"""
    # 自动添加标记
    for item in items:
        # 根据路径自动添加标记
        if "unit" in str(item.fspath):
            item.add_marker(pytest.mark.unit)
        elif "integration" in str(item.fspath):
            item.add_marker(pytest.mark.integration)
        elif "functional" in str(item.fspath):
            item.add_marker(pytest.mark.functional)
        
        # 根据文件名添加工具标记
        if "canoe" in item.name.lower():
            item.add_marker(pytest.mark.canoe)
        elif "tsmaster" in item.name.lower():
            item.add_marker(pytest.mark.tsmaster)
        elif "ttworkbench" in item.name.lower():
            item.add_marker(pytest.mark.ttworkbench)
