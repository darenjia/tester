"""
CANoe适配器单元测试

测试CANoe适配器的各项功能，包括：
- COM接口包装器
- 测试执行引擎
- 适配器主类

注意：部分测试需要真实的CANoe环境

作者: AI Assistant
创建日期: 2026-02-25
"""

import unittest
import logging
import time
from unittest.mock import Mock, patch, MagicMock
from datetime import datetime
from core.adapters.canoe import CANoeAdapter

# 配置日志
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)


class TestCANoeCOMWrapper(unittest.TestCase):
    """测试CANoe COM接口包装器"""
    
    def setUp(self):
        """测试前准备"""
        from core.adapters.canoe.com_wrapper import CANoeCOMWrapper
        self.wrapper = CANoeCOMWrapper()
    
    def test_initialization(self):
        """测试初始化"""
        self.assertIsNone(self.wrapper.version)
        self.assertFalse(self.wrapper.is_connected)
        self.assertFalse(self.wrapper.is_measurement_running)
    
    @patch('core.adapters.canoe.com_wrapper.WIN32COM_AVAILABLE', True)
    @patch('win32com.client.Dispatch')
    def test_connect_success(self, mock_dispatch):
        """测试连接成功"""
        # 模拟CANoe应用
        mock_app = Mock()
        mock_app.Version = "17.3.91"
        mock_app.Measurement = Mock()
        mock_app.System = Mock()
        mock_app.System.Namespaces = Mock()
        
        mock_dispatch.return_value = mock_app
        
        result = self.wrapper.connect()
        
        self.assertTrue(result)
        self.assertTrue(self.wrapper.is_connected)
        self.assertIsNotNone(self.wrapper.version)
        self.assertEqual(str(self.wrapper.version), "17.3.91")
    
    @patch('core.adapters.canoe.com_wrapper.WIN32COM_AVAILABLE', False)
    def test_connect_without_win32com(self):
        """测试没有win32com时的连接"""
        from core.adapters.canoe.com_wrapper import CANoeConnectionError
        
        with self.assertRaises(CANoeConnectionError):
            self.wrapper.connect()
    
    def test_disconnect(self):
        """测试断开连接"""
        # 先模拟连接状态
        self.wrapper.is_connected = True
        self.wrapper._app = Mock()
        self.wrapper._measurement = Mock()
        
        result = self.wrapper.disconnect()
        
        self.assertTrue(result)
        self.assertFalse(self.wrapper.is_connected)
    
    def test_context_manager(self):
        """测试上下文管理器"""
        with patch.object(self.wrapper, 'connect') as mock_connect, \
             patch.object(self.wrapper, 'disconnect') as mock_disconnect:
            
            mock_connect.return_value = True
            
            with self.wrapper as w:
                self.assertEqual(w, self.wrapper)
            
            mock_connect.assert_called_once()
            mock_disconnect.assert_called_once()


class TestCANoeTestEngine(unittest.TestCase):
    """测试CANoe测试执行引擎"""
    
    def setUp(self):
        """测试前准备"""
        from core.adapters.canoe.test_engine import CANoeTestEngine
        from core.adapters.canoe.com_wrapper import CANoeCOMWrapper
        
        self.mock_canoe = Mock(spec=CANoeCOMWrapper)
        self.engine = CANoeTestEngine(self.mock_canoe)
    
    def test_initialization(self):
        """测试初始化"""
        self.assertFalse(self.engine.is_running)
        self.assertFalse(self.engine.is_paused)
        self.assertIsNone(self.engine.current_case)
    
    def test_parse_case_name(self):
        """测试用例名称解析"""
        # 测试带次数的用例名
        name, count = self.engine._parse_case_name("CASE_001@3")
        self.assertEqual(name, "CASE_001")
        self.assertEqual(count, 3)
        
        # 测试不带次数的用例名
        name, count = self.engine._parse_case_name("CASE_002")
        self.assertEqual(name, "CASE_002")
        self.assertEqual(count, 1)
        
        # 测试无效格式
        name, count = self.engine._parse_case_name("CASE_003@invalid")
        self.assertEqual(name, "CASE_003@invalid")
        self.assertEqual(count, 1)
    
    def test_set_callbacks(self):
        """测试设置回调函数"""
        progress_callback = Mock()
        log_callback = Mock()
        
        self.engine.set_progress_callback(progress_callback)
        self.engine.set_log_callback(log_callback)
        
        self.assertEqual(self.engine._progress_callback, progress_callback)
        self.assertEqual(self.engine._log_callback, log_callback)
    
    def test_stop(self):
        """测试停止执行"""
        self.engine.is_running = True
        
        self.engine.stop()
        
        self.assertFalse(self.engine.is_running)
        self.assertTrue(self.engine._stop_event.is_set())
    
    def test_pause_resume(self):
        """测试暂停和恢复"""
        self.engine.pause()
        self.assertTrue(self.engine.is_paused)
        
        self.engine.resume()
        self.assertFalse(self.engine.is_paused)
    
    def test_get_execution_status(self):
        """测试获取执行状态"""
        status = self.engine.get_execution_status()
        
        self.assertIn("is_running", status)
        self.assertIn("is_paused", status)
        self.assertIn("current_case", status)
        self.assertIn("cached_cases", status)
        self.assertIn("cached_results", status)
    
    def test_clear_cache(self):
        """测试清除缓存"""
        self.engine._test_cache = ["case1", "case2"]
        self.engine._result_cache = {"case1": Mock()}
        
        self.engine.clear_cache()
        
        self.assertEqual(len(self.engine._test_cache), 0)
        self.assertEqual(len(self.engine._result_cache), 0)


class TestCANoeAdapter(unittest.TestCase):
    """测试CANoe适配器主类"""
    
    def setUp(self):
        """测试前准备"""
        from core.adapters.canoe.adapter import CANoeAdapter
        
        self.config = {
            "start_timeout": 30,
            "stop_timeout": 10,
            "measurement_timeout": 3600
        }
        
        with patch('core.adapters.canoe.adapter.CANoeCOMWrapper'), \
             patch('core.adapters.canoe.adapter.CANoeTestEngine'):
            self.adapter = CANoeAdapter(self.config)
    
    def test_initialization(self):
        """测试初始化"""
        self.assertEqual(self.adapter.start_timeout, 30)
        self.assertEqual(self.adapter.stop_timeout, 10)
        self.assertEqual(self.adapter.measurement_timeout, 3600)
    
    def test_tool_type(self):
        """测试工具类型"""
        from core.adapters.base_adapter import TestToolType
        
        self.assertEqual(self.adapter.tool_type, TestToolType.CANOE)
    
    def test_set_callbacks(self):
        """测试设置回调"""
        progress_callback = Mock()
        log_callback = Mock()
        
        self.adapter.set_progress_callback(progress_callback)
        self.adapter.set_log_callback(log_callback)
        
        self.assertEqual(self.adapter._progress_callback, progress_callback)
        self.adapter._test_engine.set_progress_callback.assert_called_with(progress_callback)
        self.adapter._test_engine.set_log_callback.assert_called_with(log_callback)
    
    def test_execute_signal_check(self):
        """测试执行信号检查"""
        self.adapter._canoe_wrapper.get_signal_value.return_value = 1500.0
        
        item = {
            "type": "signal_check",
            "name": "EngineSpeed检查",
            "signal_name": "EngineSpeed",
            "expected_value": 1500,
            "tolerance": 50
        }
        
        result = self.adapter._execute_signal_check(item)
        
        self.assertEqual(result["type"], "signal_check")
        self.assertEqual(result["signal_name"], "EngineSpeed")
        self.assertEqual(result["actual_value"], 1500.0)
        self.assertTrue(result["passed"])
        self.assertEqual(result["status"], "passed")
    
    def test_execute_signal_set(self):
        """测试执行信号设置"""
        self.adapter._canoe_wrapper.set_signal_value.return_value = True
        
        item = {
            "type": "signal_set",
            "signal_name": "EngineSpeed",
            "value": 2000.0
        }
        
        result = self.adapter._execute_signal_set(item)
        
        self.assertEqual(result["type"], "signal_set")
        self.assertTrue(result["success"])
        self.adapter._canoe_wrapper.set_signal_value.assert_called_with(
            "EngineSpeed", 2000.0, "CAN", 1
        )
    
    def test_execute_variable_check(self):
        """测试执行变量检查"""
        self.adapter._canoe_wrapper.get_system_variable.return_value = 1
        
        item = {
            "type": "variable_check",
            "namespace": "mutualVar",
            "variable": "testResult",
            "expected_value": 1
        }
        
        result = self.adapter._execute_variable_check(item)
        
        self.assertEqual(result["type"], "variable_check")
        self.assertTrue(result["passed"])
    
    def test_execute_variable_set(self):
        """测试执行变量设置"""
        self.adapter._canoe_wrapper.set_system_variable.return_value = True
        
        item = {
            "type": "variable_set",
            "namespace": "mutualVar",
            "variable": "startTest",
            "value": 1
        }
        
        result = self.adapter._execute_variable_set(item)
        
        self.assertEqual(result["type"], "variable_set")
        self.assertTrue(result["success"])
    
    def test_execute_wait_for_variable(self):
        """测试执行等待变量"""
        self.adapter._canoe_wrapper.wait_for_variable.return_value = True
        
        item = {
            "type": "wait_for_variable",
            "namespace": "mutualVar",
            "variable": "isEndTest",
            "expected_value": 1,
            "timeout": 30
        }
        
        result = self.adapter._execute_wait_for_variable(item)
        
        self.assertEqual(result["type"], "wait_for_variable")
        self.assertTrue(result["success"])
    
    def test_execute_send_can_message(self):
        """测试执行发送CAN报文"""
        self.adapter._canoe_wrapper.send_can_message.return_value = True
        
        item = {
            "type": "send_can_message",
            "channel": 1,
            "msg_id": 0x123,
            "data": [0x01, 0x02, 0x03, 0x04]
        }
        
        result = self.adapter._execute_send_can_message(item)
        
        self.assertEqual(result["type"], "send_can_message")
        self.assertEqual(result["msg_id"], 0x123)
        self.assertTrue(result["success"])
    
    def test_execute_unknown_type(self):
        """测试执行未知类型"""
        item = {
            "type": "unknown_type",
            "name": "test"
        }
        
        result = self.adapter.execute_test_item(item)
        
        self.assertEqual(result["status"], "error")
        self.assertIn("不支持的测试项类型", result["error"])
    
    def test_get_status(self):
        """测试获取状态"""
        status = self.adapter.get_status()
        
        self.assertIn("tool_type", status)
        self.assertIn("status", status)
        self.assertIn("is_connected", status)
        self.assertIn("canoe_version", status)
        self.assertIn("test_engine_status", status)
    
    def test_reset(self):
        """测试重置"""
        self.adapter._test_engine.is_running = True
        
        result = self.adapter.reset()
        
        self.assertTrue(result)
        self.adapter._test_engine.stop.assert_called_once()
        self.adapter._canoe_wrapper.clear_variable_cache.assert_called_once()
        self.adapter._test_engine.clear_cache.assert_called_once()


class TestAdapterFactory(unittest.TestCase):
    """测试适配器工厂"""
    
    def setUp(self):
        """测试前准备"""
        from core.adapters import AdapterFactory
        from core.adapters.base_adapter import TestToolType
        
        self.factory = AdapterFactory
        self.factory.clear_instances()
    
    def tearDown(self):
        """测试后清理"""
        self.factory.clear_instances()
    
    def test_create_adapter_singleton(self):
        """测试单例模式创建适配器"""
        from core.adapters.base_adapter import TestToolType
        
        adapter1 = self.factory.create_adapter(TestToolType.CANOE)
        adapter2 = self.factory.create_adapter(TestToolType.CANOE)
        
        self.assertIs(adapter1, adapter2)
    
    def test_create_adapter_non_singleton(self):
        """测试非单例模式创建适配器"""
        from core.adapters.base_adapter import TestToolType
        
        adapter1 = self.factory.create_adapter(TestToolType.CANOE, singleton=False)
        adapter2 = self.factory.create_adapter(TestToolType.CANOE, singleton=False)
        
        self.assertIsNot(adapter1, adapter2)
    
    def test_create_adapter_with_config(self):
        """测试带配置创建适配器"""
        from core.adapters.base_adapter import TestToolType
        
        config = {"start_timeout": 60}
        adapter = self.factory.create_adapter(TestToolType.CANOE, config)
        
        self.assertEqual(adapter.start_timeout, 60)
    
    def test_register_adapter(self):
        """测试注册适配器"""
        from core.adapters.base_adapter import TestToolType, BaseTestAdapter
        
        class MockAdapter(BaseTestAdapter):
            @property
            def tool_type(self):
                return TestToolType.CANOE
        
        # 临时注册
        original_registry = self.factory._registry.copy()
        
        self.factory.register_adapter(TestToolType.CANOE, MockAdapter)
        
        self.assertEqual(self.factory._registry[TestToolType.CANOE], MockAdapter)
        
        # 恢复
        self.factory._registry = original_registry
    
    def test_get_registered_types(self):
        """测试获取已注册类型"""
        types = self.factory.get_registered_types()
        
        self.assertIsInstance(types, list)
        self.assertGreater(len(types), 0)
    
    def test_clear_instances(self):
        """测试清除实例"""
        from core.adapters.base_adapter import TestToolType
        
        # 创建实例
        self.factory.create_adapter(TestToolType.CANOE)
        
        self.assertGreater(self.factory.get_instance_count(), 0)
        
        # 清除
        self.factory.clear_instances()
        
        self.assertEqual(self.factory.get_instance_count(), 0)


class TestIntegration(unittest.TestCase):
    """集成测试（需要真实CANoe环境）"""
    
    @classmethod
    def setUpClass(cls):
        """测试类开始前准备"""
        # 检查是否有CANoe环境
        cls.CANOE_AVAILABLE = True
        try:
            import win32com.client
            win32com.client.Dispatch("CANoe.Application")
            time.sleep(5)
            
        except Exception as e:
            print(f"【失败】错误详情: {e}")
            # cls.CANOE_AVAILABLE = False
    
    def test_full_workflow(self):
        """测试完整工作流程"""
        from core.adapters.canoe import CANoeAdapter
        
        adapter = CANoeAdapter()
        
        try:
            # 1. 连接
            self.assertTrue(adapter.connect())
            self.assertTrue(adapter.is_connected)
            
            # 2. 获取版本
            self.assertIsNotNone(adapter.canoe_version)
            
            # 3. 获取状态
            status = adapter.get_status()
            self.assertEqual(status["tool_type"], "canoe")
            
            # 4. 断开
            self.assertTrue(adapter.disconnect())
            self.assertFalse(adapter.is_connected)
            
        except Exception as e:
            self.fail(f"集成测试失败: {e}")
        finally:
            if adapter.is_connected:
                adapter.disconnect()
    
    def test_load_config_file(self):
        """测试加载指定配置文件"""
        from core.adapters.canoe import CANoeAdapter
        
        config_path = r"D:\TAMS\DTTC_CONFIG\S59\BCANFD\SMFT\FDCANC_E\TestProjectFile\COMTest.cfg"
        adapter = CANoeAdapter()
        
        try:
            # 1. 连接CANoe
            self.assertTrue(adapter.connect())
            self.assertTrue(adapter.is_connected)
            
            # 2. 加载配置文件
            result = adapter.load_configuration(config_path)
            self.assertTrue(result, f"加载配置文件失败: {config_path}")
            
            # 3. 验证配置已加载（通过检查状态）
            status = adapter.get_status()
            self.assertEqual(status["tool_type"], "canoe")
            self.assertTrue(status["is_connected"])
            
            # 4. 断开连接
            self.assertTrue(adapter.disconnect())
            self.assertFalse(adapter.is_connected)
            
        except Exception as e:
            self.fail(f"加载配置文件测试失败: {e}")
        finally:
            if adapter.is_connected:
                adapter.disconnect()

    def test_load_s59_bcanfd_config(self):
        """测试加载S59 BCANFD SMFT FDCANC_E配置文件"""
        from core.adapters.canoe import CANoeAdapter
        
        config_path = r"D:\TAMS\DTTC_CONFIG\S59\BCANFD\SMFT\FDCANC_E\TestProjectFile\COMTest.cfg"
        adapter = CANoeAdapter()
        
        try:
            # 1. 连接CANoe
            result = adapter.connect()
            self.assertTrue(result, "CANoe连接失败")
            self.assertTrue(adapter.is_connected)
            
            # 2. 加载S59 BCANFD配置文件
            result = adapter.load_configuration(config_path)
            self.assertTrue(result, f"加载配置文件失败: {config_path}")
            
            # 3. 验证配置已加载
            status = adapter.get_status()
            self.assertEqual(status["tool_type"], "canoe")
            self.assertTrue(status["is_connected"])
            
            # 4. 获取CANoe版本信息
            version = adapter.canoe_version
            self.assertIsNotNone(version)
            
            # 5. 断开连接
            self.assertTrue(adapter.disconnect())
            self.assertFalse(adapter.is_connected)
            
        except Exception as e:
            self.fail(f"加载S59 BCANFD配置文件测试失败: {e}")
        finally:
            if adapter.is_connected:
                adapter.disconnect()

    def test_s59_bcanfd_config_execution(self):
        """测试S59 BCANFD配置文件加载、执行测试和获取结果"""
        from core.adapters.canoe import CANoeAdapter
        
        config_path = r"D:\TAMS\DTTC_CONFIG\S59\BCAN\SGW_BCAN\DNMAS_E\TestProjectFile\AUTOSARNMTest.cfg"
        adapter = CANoeAdapter()
        
        try:
            # 1. 连接CANoe
            result = adapter.connect()
            self.assertTrue(result, "CANoe连接失败")
            self.assertTrue(adapter.is_connected)
            
            # 2. 加载S59 BCANFD配置文件
            result = adapter.load_configuration(config_path)
            self.assertTrue(result, f"加载配置文件失败: {config_path}")
            
            # 3. 启动测量
            result = adapter.start_test()
            self.assertTrue(result, "启动测量失败")
            self.assertTrue(adapter.is_running)
            
            # 4. 等待测量稳定运行
            import time
            time.sleep(5)
            
            # 5. 获取状态
            status = adapter.get_status()
            print(status)
            self.assertEqual(status["tool_type"], "canoe")
            self.assertTrue(status["is_connected"])
            self.assertTrue(status["is_running"])
            
            # 6. 执行设备自检
            self_check_result = adapter.execute_test_item({
                "type": "self_check",
                "name": "S59设备自检",
                "timeout": 300
            })
            self.assertIsNotNone(self_check_result)
            self.assertIn("status", self_check_result)
            
            # 7. 停止测量
            result = adapter.stop_test()
            self.assertTrue(result, "停止测量失败")
            self.assertFalse(adapter.is_running)
            
            # 8. 断开连接
            self.assertTrue(adapter.disconnect())
            self.assertFalse(adapter.is_connected)
            
        except Exception as e:
            self.fail(f"S59 BCANFD配置执行测试失败: {e}")
        finally:
            if adapter.is_connected:
                if adapter.is_running:
                    adapter.stop_test()
                adapter.disconnect()


if __name__ == "__main__":
    # 运行测试
    unittest.main(verbosity=2)
