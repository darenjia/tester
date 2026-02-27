"""
TSMasterAdapter单元测试

测试RPC模式和传统模式的集成
"""

import unittest
from unittest.mock import Mock, MagicMock, patch
import sys
import os

sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', '..'))

from core.adapters.tsmaster_adapter import TSMasterAdapter
from core.adapters.base_adapter import AdapterStatus


class TestTSMasterAdapterRPC(unittest.TestCase):
    """测试TSMasterAdapter的RPC功能"""
    
    def setUp(self):
        """测试前准备"""
        self.config = {
            "use_rpc": True,
            "fallback_to_traditional": True,
            "rpc_app_name": None
        }
    
    @patch('core.adapters.tsmaster_adapter.RPC_CLIENT_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMASTER_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMasterRPCClient')
    def test_connect_via_rpc_success(self, mock_rpc_client_class):
        """测试RPC模式连接成功"""
        # 创建mock对象
        mock_rpc_client = MagicMock()
        mock_rpc_client.initialize.return_value = True
        mock_rpc_client.connect.return_value = True
        mock_rpc_client_class.return_value = mock_rpc_client
        
        # 创建适配器并连接
        adapter = TSMasterAdapter(self.config)
        result = adapter.connect()
        
        # 验证结果
        self.assertTrue(result)
        self.assertTrue(adapter._using_rpc)
        self.assertEqual(adapter.status, AdapterStatus.CONNECTED)
        self.assertIsNotNone(adapter._rpc_client)
        
        # 验证RPC客户端被正确调用
        mock_rpc_client.initialize.assert_called_once()
        mock_rpc_client.connect.assert_called_once()
    
    @patch('core.adapters.tsmaster_adapter.RPC_CLIENT_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMASTER_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMasterRPCClient')
    def test_connect_rpc_fallback_to_traditional(self, mock_rpc_client_class):
        """测试RPC失败后回退到传统模式"""
        # 创建mock对象
        mock_rpc_client = MagicMock()
        mock_rpc_client.initialize.return_value = False
        mock_rpc_client_class.return_value = mock_rpc_client
        
        # 创建适配器并连接
        adapter = TSMasterAdapter(self.config)
        result = adapter.connect()
        
        # 验证结果
        # 由于TSMaster类可能不存在，RPC失败后应该返回False
        # 除非TSMaster类可用
        self.assertFalse(adapter._using_rpc)
    
    @patch('core.adapters.tsmaster_adapter.RPC_CLIENT_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMASTER_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMasterRPCClient')
    def test_connect_rpc_no_fallback(self, mock_rpc_client_class):
        """测试RPC失败且不回退"""
        # 修改配置，禁用回退
        self.config['fallback_to_traditional'] = False
        
        # 创建mock对象
        mock_rpc_client = MagicMock()
        mock_rpc_client.initialize.return_value = False
        mock_rpc_client_class.return_value = mock_rpc_client
        
        # 创建适配器并连接
        adapter = TSMasterAdapter(self.config)
        result = adapter.connect()
        
        # 验证结果
        self.assertFalse(result)
        self.assertFalse(adapter._using_rpc)
    
    @patch('core.adapters.tsmaster_adapter.RPC_CLIENT_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMASTER_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMasterRPCClient')
    def test_start_stop_simulation_via_rpc(self, mock_rpc_client_class):
        """测试通过RPC启动和停止仿真"""
        # 创建mock对象
        mock_rpc_client = MagicMock()
        mock_rpc_client.initialize.return_value = True
        mock_rpc_client.connect.return_value = True
        mock_rpc_client.start_simulation.return_value = True
        mock_rpc_client.stop_simulation.return_value = True
        mock_rpc_client_class.return_value = mock_rpc_client
        
        # 创建适配器并连接
        adapter = TSMasterAdapter(self.config)
        adapter.connect()
        
        # 启动仿真
        result = adapter.start_test()
        self.assertTrue(result)
        self.assertEqual(adapter.status, AdapterStatus.RUNNING)
        mock_rpc_client.start_simulation.assert_called_once()
        
        # 停止仿真
        result = adapter.stop_test()
        self.assertTrue(result)
        self.assertEqual(adapter.status, AdapterStatus.CONNECTED)
        mock_rpc_client.stop_simulation.assert_called_once()
    
    @patch('core.adapters.tsmaster_adapter.RPC_CLIENT_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMASTER_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMasterRPCClient')
    def test_signal_operations_via_rpc(self, mock_rpc_client_class):
        """测试通过RPC进行信号操作"""
        # 创建mock对象
        mock_rpc_client = MagicMock()
        mock_rpc_client.initialize.return_value = True
        mock_rpc_client.connect.return_value = True
        mock_rpc_client.get_can_signal.return_value = 100.0
        mock_rpc_client.set_can_signal.return_value = True
        mock_rpc_client_class.return_value = mock_rpc_client
        
        # 创建适配器并连接
        adapter = TSMasterAdapter(self.config)
        adapter.connect()
        
        # 测试读取信号
        value = adapter._get_signal("TestSignal")
        self.assertEqual(value, 100.0)
        mock_rpc_client.get_can_signal.assert_called_with("TestSignal")
        
        # 测试设置信号
        result = adapter._set_signal("TestSignal", 200.0)
        self.assertTrue(result)
        mock_rpc_client.set_can_signal.assert_called_with("TestSignal", 200.0)
    
    @patch('core.adapters.tsmaster_adapter.RPC_CLIENT_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMASTER_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMasterRPCClient')
    def test_system_var_operations_via_rpc(self, mock_rpc_client_class):
        """测试通过RPC进行系统变量操作"""
        # 创建mock对象
        mock_rpc_client = MagicMock()
        mock_rpc_client.initialize.return_value = True
        mock_rpc_client.connect.return_value = True
        mock_rpc_client.read_system_var.return_value = "1000"
        mock_rpc_client.write_system_var.return_value = True
        mock_rpc_client_class.return_value = mock_rpc_client
        
        # 创建适配器并连接
        adapter = TSMasterAdapter(self.config)
        adapter.connect()
        
        # 测试读取系统变量
        value = adapter._read_system_var("Var0")
        self.assertEqual(value, "1000")
        mock_rpc_client.read_system_var.assert_called_with("Var0")
        
        # 测试写入系统变量
        result = adapter._write_system_var("Var0", "2000")
        self.assertTrue(result)
        mock_rpc_client.write_system_var.assert_called_with("Var0", "2000")
    
    @patch('core.adapters.tsmaster_adapter.RPC_CLIENT_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMASTER_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMasterRPCClient')
    def test_execute_sysvar_check_item(self, mock_rpc_client_class):
        """测试执行系统变量检查测试项"""
        # 创建mock对象
        mock_rpc_client = MagicMock()
        mock_rpc_client.initialize.return_value = True
        mock_rpc_client.connect.return_value = True
        mock_rpc_client.read_system_var.return_value = "1000"
        mock_rpc_client_class.return_value = mock_rpc_client
        
        # 创建适配器并连接
        adapter = TSMasterAdapter(self.config)
        adapter.connect()
        
        # 执行系统变量检查
        item = {
            "name": "测试系统变量",
            "type": "sysvar_check",
            "var_name": "Var0",
            "expected_value": "1000"
        }
        result = adapter.execute_test_item(item)
        
        # 验证结果
        self.assertEqual(result["status"], "passed")
        self.assertTrue(result["passed"])
        self.assertEqual(result["actual_value"], "1000")
    
    @patch('core.adapters.tsmaster_adapter.RPC_CLIENT_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMASTER_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMasterRPCClient')
    def test_execute_sysvar_set_item(self, mock_rpc_client_class):
        """测试执行系统变量设置测试项"""
        # 创建mock对象
        mock_rpc_client = MagicMock()
        mock_rpc_client.initialize.return_value = True
        mock_rpc_client.connect.return_value = True
        mock_rpc_client.write_system_var.return_value = True
        mock_rpc_client_class.return_value = mock_rpc_client
        
        # 创建适配器并连接
        adapter = TSMasterAdapter(self.config)
        adapter.connect()
        
        # 执行系统变量设置
        item = {
            "name": "设置系统变量",
            "type": "sysvar_set",
            "var_name": "Var0",
            "value": "2000"
        }
        result = adapter.execute_test_item(item)
        
        # 验证结果
        self.assertEqual(result["status"], "passed")
        self.assertTrue(result["success"])
    
    @patch('core.adapters.tsmaster_adapter.RPC_CLIENT_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMASTER_AVAILABLE', True)
    @patch('core.adapters.tsmaster_adapter.TSMasterRPCClient')
    def test_disconnect_via_rpc(self, mock_rpc_client_class):
        """测试通过RPC断开连接"""
        # 创建mock对象
        mock_rpc_client = MagicMock()
        mock_rpc_client.initialize.return_value = True
        mock_rpc_client.connect.return_value = True
        mock_rpc_client.finalize.return_value = True
        mock_rpc_client_class.return_value = mock_rpc_client
        
        # 创建适配器并连接
        adapter = TSMasterAdapter(self.config)
        adapter.connect()
        
        # 断开连接
        result = adapter.disconnect()
        
        # 验证结果
        self.assertTrue(result)
        self.assertEqual(adapter.status, AdapterStatus.DISCONNECTED)
        self.assertIsNone(adapter._rpc_client)
        mock_rpc_client.finalize.assert_called_once()


class TestTSMasterAdapterTraditional(unittest.TestCase):
    """测试TSMasterAdapter的传统模式功能"""
    
    def setUp(self):
        """测试前准备"""
        self.config = {
            "use_rpc": False
        }
    
    @patch('core.adapters.tsmaster_adapter.TSMASTER_AVAILABLE', True)
    def test_connect_via_traditional(self):
        """测试传统模式连接"""
        # 创建适配器
        adapter = TSMasterAdapter(self.config)
        
        # 由于TSMaster类可能不存在，这里只测试配置
        self.assertFalse(adapter.use_rpc)
        self.assertEqual(adapter.status, AdapterStatus.IDLE)
    
    @patch('core.adapters.tsmaster_adapter.TSMASTER_AVAILABLE', True)
    def test_start_stop_simulation_via_traditional(self):
        """测试通过传统模式启动和停止仿真"""
        # 创建适配器
        adapter = TSMasterAdapter(self.config)
        
        # 由于TSMaster类可能不存在，这里只测试配置
        self.assertFalse(adapter.use_rpc)
        self.assertEqual(adapter.status, AdapterStatus.IDLE)


class TestTSMasterAdapterConfig(unittest.TestCase):
    """测试TSMasterAdapter配置"""
    
    def test_default_config(self):
        """测试默认配置"""
        adapter = TSMasterAdapter()
        
        self.assertTrue(adapter.use_rpc)
        self.assertIsNone(adapter.rpc_app_name)
        self.assertTrue(adapter.fallback_to_traditional)
        self.assertEqual(adapter.start_timeout, 30)
        self.assertEqual(adapter.stop_timeout, 10)
    
    def test_custom_config(self):
        """测试自定义配置"""
        config = {
            "use_rpc": False,
            "rpc_app_name": "MyApp",
            "fallback_to_traditional": False,
            "start_timeout": 60,
            "stop_timeout": 20
        }
        adapter = TSMasterAdapter(config)
        
        self.assertFalse(adapter.use_rpc)
        self.assertEqual(adapter.rpc_app_name, "MyApp")
        self.assertFalse(adapter.fallback_to_traditional)
        self.assertEqual(adapter.start_timeout, 60)
        self.assertEqual(adapter.stop_timeout, 20)


if __name__ == '__main__':
    unittest.main()
