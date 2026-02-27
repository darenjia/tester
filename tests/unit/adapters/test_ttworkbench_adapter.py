"""
TTworkbench适配器单元测试

测试TTworkbenchAdapter的各项功能（使用Mock）
"""

import pytest
import os
from unittest.mock import patch, MagicMock, mock_open
from pathlib import Path

from python_executor.core.adapters import (
    TTworkbenchAdapter, TestToolType, AdapterStatus
)


class TestTTworkbenchAdapter:
    """测试TTworkbench适配器"""
    
    def test_tool_type(self):
        """测试工具类型"""
        adapter = TTworkbenchAdapter()
        assert adapter.tool_type == TestToolType.TTWORKBENCH
    
    def test_initial_state(self):
        """测试初始状态"""
        adapter = TTworkbenchAdapter()
        
        assert adapter.status == AdapterStatus.IDLE
        assert adapter.ttman_path == ""
        assert adapter.workspace_path == ""
        assert adapter.log_path == ""
        assert adapter.report_path == ""
        assert adapter.report_format == "pdf"
        assert adapter.timeout == 3600
    
    def test_config_initialization(self):
        """测试配置初始化"""
        config = {
            "ttman_path": "C:/Spirent/TTman.bat",
            "workspace_path": "C:/Workspace",
            "log_path": "C:/Logs",
            "report_path": "C:/Reports",
            "report_format": "html",
            "timeout": 1800
        }
        
        adapter = TTworkbenchAdapter(config)
        
        assert adapter.ttman_path == "C:/Spirent/TTman.bat"
        assert adapter.workspace_path == "C:/Workspace"
        assert adapter.log_path == "C:/Logs"
        assert adapter.report_path == "C:/Reports"
        assert adapter.report_format == "html"
        assert adapter.timeout == 1800
    
    @patch('os.path.isfile')
    @patch('os.makedirs')
    def test_connect_success(self, mock_makedirs, mock_isfile):
        """测试连接成功"""
        mock_isfile.return_value = True
        
        config = {
            "ttman_path": "C:/Spirent/TTman.bat",
            "workspace_path": "C:/Workspace",
            "log_path": "C:/Logs",
            "report_path": "C:/Reports"
        }
        adapter = TTworkbenchAdapter(config)
        
        result = adapter.connect()
        
        assert result is True
        assert adapter.status == AdapterStatus.CONNECTED
        assert adapter.is_connected is True
    
    @patch('os.path.isfile')
    def test_connect_missing_ttman_path(self, mock_isfile):
        """测试连接失败 - 缺少TTman路径"""
        mock_isfile.return_value = False
        
        config = {
            "ttman_path": "C:/Spirent/TTman.bat",
            "workspace_path": "C:/Workspace",
            "log_path": "C:/Logs",
            "report_path": "C:/Reports"
        }
        adapter = TTworkbenchAdapter(config)
        
        result = adapter.connect()
        
        assert result is False
        assert adapter.status == AdapterStatus.ERROR
        assert "TTman路径不存在" in adapter.last_error
    
    def test_connect_missing_config(self):
        """测试连接失败 - 缺少配置"""
        adapter = TTworkbenchAdapter()
        
        result = adapter.connect()
        
        assert result is False
        assert adapter.status == AdapterStatus.ERROR
    
    def test_disconnect(self):
        """测试断开连接"""
        config = {
            "ttman_path": "C:/Spirent/TTman.bat",
            "workspace_path": "C:/Workspace",
            "log_path": "C:/Logs",
            "report_path": "C:/Reports"
        }
        adapter = TTworkbenchAdapter(config)
        adapter.connect()
        
        result = adapter.disconnect()
        
        assert result is True
        assert adapter.status == AdapterStatus.DISCONNECTED
    
    @patch('os.path.isfile')
    def test_load_configuration_success(self, mock_isfile):
        """测试加载配置成功"""
        mock_isfile.return_value = True
        
        adapter = TTworkbenchAdapter()
        
        result = adapter.load_configuration("C:/Test/test.clf")
        
        assert result is True
    
    @patch('os.path.isfile')
    def test_load_configuration_failure(self, mock_isfile):
        """测试加载配置失败"""
        mock_isfile.return_value = False
        
        adapter = TTworkbenchAdapter()
        
        result = adapter.load_configuration("C:/Test/test.clf")
        
        assert result is False
        assert "clf文件不存在" in adapter.last_error
    
    def test_start_test(self):
        """测试启动测试（TTworkbench不需要启动操作）"""
        adapter = TTworkbenchAdapter()
        
        result = adapter.start_test()
        
        assert result is True
    
    def test_stop_test(self):
        """测试停止测试"""
        adapter = TTworkbenchAdapter()
        
        result = adapter.stop_test()
        
        assert result is True
    
    def test_build_ttman_command(self):
        """测试构建TTman命令"""
        config = {
            "ttman_path": "C:/Spirent/TTman.bat",
            "workspace_path": "C:/Workspace",
            "log_path": "C:/Logs",
            "report_path": "C:/Reports",
            "report_format": "pdf"
        }
        adapter = TTworkbenchAdapter(config)
        
        cmd = adapter._build_ttman_command("C:/Test/test.clf")
        
        assert cmd[0] == "C:/Spirent/TTman.bat"
        assert "--data" in cmd
        assert "C:/Workspace" in cmd
        assert "--log" in cmd
        assert "C:/Logs" in cmd
        assert "-r" in cmd
        assert "pdf" in cmd
        assert "--report-dir" in cmd
        assert "C:/Reports" in cmd
        assert "C:/Test/test.clf" in cmd
    
    @patch('subprocess.Popen')
    def test_run_ttman_command_success(self, mock_popen):
        """测试运行TTman命令成功"""
        # Mock进程
        mock_process = MagicMock()
        mock_process.returncode = 0
        mock_process.communicate.return_value = ("stdout content", "stderr content")
        mock_popen.return_value = mock_process
        
        config = {
            "ttman_path": "C:/Spirent/TTman.bat",
            "workspace_path": "C:/Workspace",
            "log_path": "C:/Logs",
            "report_path": "C:/Reports",
            "timeout": 3600
        }
        adapter = TTworkbenchAdapter(config)
        
        cmd = ["C:/Spirent/TTman.bat", "--data", "C:/Workspace", "C:/Test/test.clf"]
        result = adapter._run_ttman_command(cmd)
        
        assert result["return_code"] == 0
        assert result["stdout"] == "stdout content"
        assert result["stderr"] == "stderr content"
        assert "execution_time" in result
    
    @patch('subprocess.Popen')
    def test_run_ttman_command_timeout(self, mock_popen):
        """测试运行TTman命令超时"""
        import subprocess
        
        # Mock进程超时
        mock_process = MagicMock()
        mock_process.communicate.side_effect = subprocess.TimeoutExpired(cmd="test", timeout=1)
        mock_process.kill.return_value = None
        mock_popen.return_value = mock_process
        
        config = {
            "ttman_path": "C:/Spirent/TTman.bat",
            "workspace_path": "C:/Workspace",
            "log_path": "C:/Logs",
            "report_path": "C:/Reports",
            "timeout": 1
        }
        adapter = TTworkbenchAdapter(config)
        
        cmd = ["C:/Spirent/TTman.bat", "C:/Test/test.clf"]
        result = adapter._run_ttman_command(cmd)
        
        assert result["return_code"] == -1
        assert "超时" in result["stderr"]
    
    @patch('os.path.isfile')
    @patch.object(TTworkbenchAdapter, '_build_ttman_command')
    @patch.object(TTworkbenchAdapter, '_run_ttman_command')
    def test_execute_clf_test(self, mock_run, mock_build_cmd, mock_isfile):
        """测试执行clf测试"""
        mock_isfile.return_value = True
        mock_build_cmd.return_value = ["TTman.bat", "test.clf"]
        mock_run.return_value = {
            "return_code": 0,
            "stdout": "success",
            "stderr": "",
            "execution_time": 10.5
        }
        
        config = {
            "ttman_path": "C:/Spirent/TTman.bat",
            "workspace_path": "C:/Workspace",
            "log_path": "C:/Logs",
            "report_path": "C:/Reports"
        }
        adapter = TTworkbenchAdapter(config)
        adapter.connect()
        
        item = {
            "name": "TC8测试",
            "type": "clf_test",
            "clf_file": "C:/Test/TC8_001.clf"
        }
        
        result = adapter._execute_clf_test(item)
        
        assert result["name"] == "TC8测试"
        assert result["type"] == "clf_test"
        assert result["clf_file"] == "C:/Test/TC8_001.clf"
        assert result["status"] == "passed"
        assert result["return_code"] == 0
    
    def test_execute_clf_test_missing_file(self):
        """测试执行clf测试 - 缺少文件"""
        adapter = TTworkbenchAdapter()
        
        item = {
            "name": "TC8测试",
            "type": "clf_test"
        }
        
        result = adapter.execute_test_item(item)
        
        assert result["status"] == "error"
        assert "clf_file" in result["error"]
    
    @patch.object(TTworkbenchAdapter, '_execute_clf_test')
    def test_execute_batch_test(self, mock_execute_clf):
        """测试批量执行"""
        mock_execute_clf.side_effect = [
            {"name": "test1", "status": "passed"},
            {"name": "test2", "status": "passed"},
            {"name": "test3", "status": "failed"}
        ]
        
        adapter = TTworkbenchAdapter()
        
        item = {
            "name": "批量测试",
            "type": "batch_test",
            "clf_files": ["test1.clf", "test2.clf", "test3.clf"]
        }
        
        result = adapter._execute_batch_test(item)
        
        assert result["name"] == "批量测试"
        assert result["type"] == "batch_test"
        assert result["total_tests"] == 3
        assert result["passed"] == 2
        assert result["failed"] == 1
        assert result["status"] == "failed"
    
    @patch('os.path.isfile')
    def test_get_report_file(self, mock_isfile):
        """测试获取报告文件"""
        mock_isfile.return_value = True
        
        config = {
            "report_path": "C:/Reports",
            "report_format": "pdf"
        }
        adapter = TTworkbenchAdapter(config)
        
        result = adapter._get_report_file("TC8_001")
        
        assert result == "C:/Reports/TC8_001.pdf"
    
    @patch('os.path.isfile')
    def test_get_log_file(self, mock_isfile):
        """测试获取日志文件"""
        mock_isfile.return_value = True
        
        config = {
            "log_path": "C:/Logs"
        }
        adapter = TTworkbenchAdapter(config)
        
        result = adapter._get_log_file("TC8_001")
        
        assert result == "C:/Logs/TC8_001.tlz"
    
    def test_get_execution_summary(self):
        """测试获取执行摘要"""
        config = {
            "workspace_path": "C:/Workspace",
            "report_path": "C:/Reports",
            "log_path": "C:/Logs",
            "report_format": "pdf"
        }
        adapter = TTworkbenchAdapter(config)
        adapter.connect()
        
        summary = adapter.get_execution_summary()
        
        assert summary["tool_type"] == "ttworkbench"
        assert summary["status"] == "CONNECTED"
        assert summary["workspace_path"] == "C:/Workspace"
        assert summary["report_path"] == "C:/Reports"
        assert summary["log_path"] == "C:/Logs"
        assert summary["report_format"] == "pdf"
