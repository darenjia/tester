"""
任务执行引擎单元测试

测试TaskExecutorV2的各项功能
"""

import pytest
import time
import threading
from unittest.mock import MagicMock, patch

from python_executor.core.task_executor_v2 import (
    TaskExecutorV2, TaskStatus, get_task_executor, reset_task_executor
)
from python_executor.core.adapters import TestToolType


class TestTaskExecutorV2:
    """测试任务执行引擎V2"""
    
    def test_initial_state(self, mock_message_sender):
        """测试初始状态"""
        executor = TaskExecutorV2(mock_message_sender)
        
        assert executor.is_busy is False
        assert executor.current_task_id is None
        assert executor.message_sender == mock_message_sender
    
    def test_execute_task_success(self, mock_message_sender, sample_task_config, mocker):
        """测试执行任务成功"""
        executor = TaskExecutorV2(mock_message_sender)
        
        # Mock适配器
        mock_adapter = MagicMock()
        mock_adapter.connect.return_value = True
        mock_adapter.load_configuration.return_value = True
        mock_adapter.start_test.return_value = True
        mock_adapter.stop_test.return_value = True
        mock_adapter.execute_test_item.return_value = {"status": "passed"}
        mock_adapter.get_status.return_value = {"status": "CONNECTED"}
        
        with patch('python_executor.core.task_executor_v2.create_adapter', return_value=mock_adapter):
            result = executor.execute_task(sample_task_config)
            
            assert result is True
            assert executor.is_busy is True
            assert executor.current_task_id == "TEST_TASK_001"
            
            # 等待任务完成
            time.sleep(0.5)
            
            # 验证适配器方法被调用
            mock_adapter.connect.assert_called_once()
            mock_adapter.load_configuration.assert_called_once()
            mock_adapter.start_test.assert_called_once()
            mock_adapter.stop_test.assert_called_once()
            mock_adapter.disconnect.assert_called_once()
    
    def test_execute_task_when_busy(self, mock_message_sender, sample_task_config):
        """测试忙时执行任务"""
        executor = TaskExecutorV2(mock_message_sender)
        
        # 先执行一个任务
        with patch('python_executor.core.task_executor_v2.create_adapter'):
            executor.execute_task(sample_task_config)
            
            # 再尝试执行另一个任务
            second_task = sample_task_config.copy()
            second_task["taskId"] = "TEST_TASK_002"
            result = executor.execute_task(second_task)
            
            assert result is False
    
    def test_cancel_task_success(self, mock_message_sender, sample_task_config, mocker):
        """测试取消任务成功"""
        executor = TaskExecutorV2(mock_message_sender)
        
        # Mock适配器
        mock_adapter = MagicMock()
        
        with patch('python_executor.core.task_executor_v2.create_adapter', return_value=mock_adapter):
            # 启动任务
            executor.execute_task(sample_task_config)
            time.sleep(0.1)
            
            # 取消任务
            result = executor.cancel_task("TEST_TASK_001")
            
            assert result is True
            mock_adapter.stop_test.assert_called_once()
    
    def test_cancel_task_wrong_id(self, mock_message_sender, sample_task_config):
        """测试取消任务 - 错误的任务ID"""
        executor = TaskExecutorV2(mock_message_sender)
        
        with patch('python_executor.core.task_executor_v2.create_adapter'):
            executor.execute_task(sample_task_config)
            time.sleep(0.1)
            
            result = executor.cancel_task("WRONG_TASK_ID")
            
            assert result is False
    
    def test_cancel_task_no_task(self, mock_message_sender):
        """测试取消任务 - 没有正在执行的任务"""
        executor = TaskExecutorV2(mock_message_sender)
        
        result = executor.cancel_task("TEST_TASK_001")
        
        assert result is False
    
    def test_get_task_status_idle(self, mock_message_sender):
        """测试获取任务状态 - 空闲"""
        executor = TaskExecutorV2(mock_message_sender)
        
        status = executor.get_task_status()
        
        assert status["taskId"] is None
        assert status["status"] == "PENDING"
        assert status["isBusy"] is False
        assert status["results"] == []
    
    def test_get_task_status_running(self, mock_message_sender, sample_task_config):
        """测试获取任务状态 - 运行中"""
        executor = TaskExecutorV2(mock_message_sender)
        
        with patch('python_executor.core.task_executor_v2.create_adapter'):
            executor.execute_task(sample_task_config)
            time.sleep(0.1)
            
            status = executor.get_task_status()
            
            assert status["taskId"] == "TEST_TASK_001"
            assert status["isBusy"] is True
            assert "elapsedTime" in status
    
    def test_task_execution_flow(self, mock_message_sender, sample_task_config, mocker):
        """测试任务执行完整流程"""
        executor = TaskExecutorV2(mock_message_sender)
        
        # Mock适配器
        mock_adapter = MagicMock()
        mock_adapter.connect.return_value = True
        mock_adapter.load_configuration.return_value = True
        mock_adapter.start_test.return_value = True
        mock_adapter.stop_test.return_value = True
        mock_adapter.execute_test_item.side_effect = [
            {"name": "test1", "status": "passed"},
            {"name": "test2", "status": "passed"}
        ]
        mock_adapter.get_status.return_value = {"status": "CONNECTED"}
        
        with patch('python_executor.core.task_executor_v2.create_adapter', return_value=mock_adapter):
            # 执行任务
            result = executor.execute_task(sample_task_config)
            assert result is True
            
            # 等待任务完成
            time.sleep(0.5)
            
            # 验证消息发送
            assert mock_message_sender.call_count >= 5  # 状态、日志、进度、结果
            
            # 验证适配器调用
            assert mock_adapter.connect.called
            assert mock_adapter.load_configuration.called
            assert mock_adapter.start_test.called
            assert mock_adapter.execute_test_item.call_count == 2
            assert mock_adapter.stop_test.called
            assert mock_adapter.disconnect.called
    
    def test_task_execution_with_error(self, mock_message_sender, sample_task_config, mocker):
        """测试任务执行出错"""
        executor = TaskExecutorV2(mock_message_sender)
        
        # Mock适配器连接失败
        mock_adapter = MagicMock()
        mock_adapter.connect.return_value = False
        mock_adapter.last_error = "Connection failed"
        
        with patch('python_executor.core.task_executor_v2.create_adapter', return_value=mock_adapter):
            result = executor.execute_task(sample_task_config)
            assert result is True
            
            # 等待任务完成
            time.sleep(0.3)
            
            # 验证发送了失败状态
            calls = mock_message_sender.call_args_list
            failed_calls = [call for call in calls if call[0][0] == "TASK_STATUS" and call[0][1].get("status") == "FAILED"]
            assert len(failed_calls) > 0
    
    def test_ttworkbench_task_execution(self, mock_message_sender, sample_ttworkbench_task, mocker):
        """测试TTworkbench任务执行"""
        executor = TaskExecutorV2(mock_message_sender)
        
        # Mock适配器
        mock_adapter = MagicMock()
        mock_adapter.connect.return_value = True
        mock_adapter.load_configuration.return_value = True
        mock_adapter.start_test.return_value = True
        mock_adapter.stop_test.return_value = True
        mock_adapter.execute_test_item.return_value = {
            "name": "TC8测试",
            "status": "passed",
            "report_file": "C:/Reports/TC8_001.pdf"
        }
        mock_adapter.get_status.return_value = {"status": "CONNECTED"}
        
        with patch('python_executor.core.task_executor_v2.create_adapter', return_value=mock_adapter):
            result = executor.execute_task(sample_ttworkbench_task)
            assert result is True
            
            # 等待任务完成
            time.sleep(0.5)
            
            # 验证适配器被正确创建
            mock_adapter.connect.assert_called_once()
            # TTworkbench不需要start_test
            mock_adapter.start_test.assert_not_called()


class TestTaskExecutorSingleton:
    """测试任务执行引擎单例模式"""
    
    def test_singleton_pattern(self, mock_message_sender):
        """测试单例模式"""
        reset_task_executor()
        
        executor1 = get_task_executor(mock_message_sender)
        executor2 = get_task_executor()
        
        assert executor1 is executor2
        
        reset_task_executor()
    
    def test_reset_task_executor(self, mock_message_sender):
        """测试重置单例"""
        reset_task_executor()
        
        executor1 = get_task_executor(mock_message_sender)
        reset_task_executor()
        executor2 = get_task_executor(mock_message_sender)
        
        assert executor1 is not executor2
        
        reset_task_executor()
    
    def test_get_task_executor_without_sender_first_time(self):
        """测试首次获取不提供sender"""
        reset_task_executor()
        
        with pytest.raises(ValueError) as exc_info:
            get_task_executor()
        
        assert "message_sender" in str(exc_info.value)
        
        reset_task_executor()


class TestTaskStatus:
    """测试任务状态枚举"""
    
    def test_task_status_values(self):
        """测试任务状态值"""
        assert TaskStatus.PENDING.name == "PENDING"
        assert TaskStatus.RUNNING.name == "RUNNING"
        assert TaskStatus.COMPLETED.name == "COMPLETED"
        assert TaskStatus.FAILED.name == "FAILED"
        assert TaskStatus.CANCELLED.name == "CANCELLED"
        assert TaskStatus.TIMEOUT.name == "TIMEOUT"
