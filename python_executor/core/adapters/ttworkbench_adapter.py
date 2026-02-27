"""
TTworkbench测试工具适配器

基于TTman命令行调用实现自动化控制
支持TC8、AVB、AUTOSAR等一致性测试
"""

import os
import subprocess
import time
import logging
from typing import Optional, Dict, Any, List
from pathlib import Path

from .base_adapter import BaseTestAdapter, TestToolType, AdapterStatus


class TTworkbenchAdapter(BaseTestAdapter):
    """
    TTworkbench测试工具适配器
    
    通过TTman命令行调用控制TTworkbench软件，支持以下功能：
    - 配置TTworkbench路径
    - 执行clf测试文件
    - 获取测试报告和日志
    - 批量执行测试用例
    
    注意：TTworkbench需要预先在客户端创建可执行的clf文件
    """
    
    def __init__(self, config: dict = None):
        """
        初始化TTworkbench适配器
        
        Args:
            config: 配置字典，可包含：
                - ttman_path: TTman.bat路径
                - workspace_path: workspace路径
                - log_path: 日志输出路径
                - report_path: 报告输出路径
                - report_format: 报告格式（pdf/html/xml，默认pdf）
                - timeout: 测试超时时间（默认3600秒）
        """
        super().__init__(config)
        
        # TTworkbench路径配置
        self.ttman_path = self.config.get("ttman_path", "")
        self.workspace_path = self.config.get("workspace_path", "")
        self.log_path = self.config.get("log_path", "")
        self.report_path = self.config.get("report_path", "")
        self.report_format = self.config.get("report_format", "pdf")
        self.timeout = self.config.get("timeout", 3600)
        
        # 执行状态
        self._current_process: Optional[subprocess.Popen] = None
        self._execution_log: List[str] = []
        
    @property
    def tool_type(self) -> TestToolType:
        """返回测试工具类型"""
        return TestToolType.TTWORKBENCH
    
    def connect(self) -> bool:
        """
        验证TTworkbench配置
        
        TTworkbench不需要传统意义上的"连接"，
        此方法用于验证配置路径是否正确
        
        Returns:
            配置验证通过返回True，否则返回False
        """
        try:
            self.status = AdapterStatus.CONNECTING
            self.logger.info("正在验证TTworkbench配置...")
            
            # 验证必需配置
            required_paths = {
                "ttman_path": self.ttman_path,
                "workspace_path": self.workspace_path,
                "log_path": self.log_path,
                "report_path": self.report_path
            }
            
            for name, path in required_paths.items():
                if not path:
                    self._set_error(f"配置缺少必需的路径: {name}")
                    return False
                
                # 验证路径存在
                if name == "ttman_path":
                    if not os.path.isfile(path):
                        self._set_error(f"TTman路径不存在: {path}")
                        return False
                else:
                    # 创建目录（如果不存在）
                    os.makedirs(path, exist_ok=True)
            
            self.status = AdapterStatus.CONNECTED
            self._clear_error()
            self.logger.info("TTworkbench配置验证通过")
            return True
            
        except Exception as e:
            self._set_error(f"TTworkbench配置验证失败: {str(e)}")
            return False
    
    def disconnect(self) -> bool:
        """
        断开TTworkbench
        
        终止当前正在执行的测试进程
        
        Returns:
            断开成功返回True，否则返回False
        """
        try:
            # 终止正在执行的进程
            if self._current_process and self._current_process.poll() is None:
                self.logger.info("正在终止TTworkbench测试进程...")
                self._current_process.terminate()
                try:
                    self._current_process.wait(timeout=5)
                except subprocess.TimeoutExpired:
                    self._current_process.kill()
            
            self._current_process = None
            self.status = AdapterStatus.DISCONNECTED
            self.logger.info("TTworkbench已断开")
            return True
            
        except Exception as e:
            self._set_error(f"TTworkbench断开失败: {str(e)}")
            return False
    
    def load_configuration(self, config_path: str) -> bool:
        """
        验证clf文件路径
        
        Args:
            config_path: clf文件路径
            
        Returns:
            文件存在返回True，否则返回False
        """
        if not os.path.isfile(config_path):
            self._set_error(f"clf文件不存在: {config_path}")
            return False
        
        self.logger.info(f"clf文件验证通过: {config_path}")
        return True
    
    def start_test(self) -> bool:
        """
        TTworkbench不需要启动操作
        
        测试执行在execute_test_item中完成
        
        Returns:
            始终返回True
        """
        self.logger.info("TTworkbench测试准备就绪")
        return True
    
    def stop_test(self) -> bool:
        """
        停止TTworkbench测试
        
        终止当前正在执行的测试进程
        
        Returns:
            停止成功返回True，否则返回False
        """
        return self.disconnect()
    
    def execute_test_item(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """
        执行单个测试项（clf文件）
        
        支持的测试项类型：
        - clf_test: 执行clf测试文件
        - batch_test: 批量执行多个clf文件
        
        Args:
            item: 测试项配置字典
                - type: 测试项类型
                - name: 测试项名称
                - clf_file: clf文件路径（clf_test类型）
                - clf_files: clf文件路径列表（batch_test类型）
                
        Returns:
            测试结果字典
        """
        item_type = item.get("type")
        item_name = item.get("name", "unnamed")
        
        self.logger.info(f"执行测试项: {item_name} (类型: {item_type})")
        
        try:
            if item_type == "clf_test":
                return self._execute_clf_test(item)
            elif item_type == "batch_test":
                return self._execute_batch_test(item)
            else:
                return {
                    "name": item_name,
                    "type": item_type,
                    "status": "error",
                    "error": f"不支持的测试项类型: {item_type}"
                }
                
        except Exception as e:
            self.logger.error(f"执行测试项失败: {str(e)}")
            return {
                "name": item_name,
                "type": item_type,
                "status": "error",
                "error": str(e)
            }
    
    def _execute_clf_test(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """执行单个clf测试文件"""
        clf_file = item.get("clf_file")
        
        if not clf_file:
            raise ValueError("clf_test类型需要指定clf_file参数")
        
        if not os.path.isfile(clf_file):
            raise FileNotFoundError(f"clf文件不存在: {clf_file}")
        
        # 构建TTman命令
        cmd = self._build_ttman_command(clf_file)
        
        self.logger.info(f"执行TTman命令: {' '.join(cmd)}")
        
        # 执行命令
        result = self._run_ttman_command(cmd)
        
        # 获取测试用例名
        test_case_name = Path(clf_file).stem
        
        # 获取报告和日志文件
        report_file = self._get_report_file(test_case_name)
        log_file = self._get_log_file(test_case_name)
        
        return {
            "name": item.get("name"),
            "type": "clf_test",
            "clf_file": clf_file,
            "test_case_name": test_case_name,
            "command": ' '.join(cmd),
            "return_code": result["return_code"],
            "stdout": result["stdout"],
            "stderr": result["stderr"],
            "execution_time": result["execution_time"],
            "report_file": report_file,
            "log_file": log_file,
            "status": "passed" if result["return_code"] == 0 else "failed"
        }
    
    def _execute_batch_test(self, item: Dict[str, Any]) -> Dict[str, Any]:
        """批量执行多个clf测试文件"""
        clf_files = item.get("clf_files", [])
        
        if not clf_files:
            raise ValueError("batch_test类型需要指定clf_files参数")
        
        results = []
        total_passed = 0
        total_failed = 0
        
        for clf_file in clf_files:
            # 执行单个测试
            single_result = self._execute_clf_test({
                "name": Path(clf_file).stem,
                "clf_file": clf_file
            })
            
            results.append(single_result)
            
            if single_result["status"] == "passed":
                total_passed += 1
            else:
                total_failed += 1
        
        return {
            "name": item.get("name"),
            "type": "batch_test",
            "total_tests": len(clf_files),
            "passed": total_passed,
            "failed": total_failed,
            "results": results,
            "status": "passed" if total_failed == 0 else "failed"
        }
    
    def _build_ttman_command(self, clf_file: str) -> List[str]:
        """构建TTman命令"""
        cmd = [
            self.ttman_path,
            '--data', self.workspace_path,
            '--log', self.log_path,
            '-r', self.report_format,
            '--report-dir', self.report_path,
            clf_file
        ]
        return cmd
    
    def _run_ttman_command(self, cmd: List[str]) -> Dict[str, Any]:
        """执行TTman命令"""
        start_time = time.time()
        
        try:
            # 启动进程
            self._current_process = subprocess.Popen(
                cmd,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                text=True,
                encoding='utf-8',
                errors='ignore'
            )
            
            # 等待执行完成
            stdout, stderr = self._current_process.communicate(timeout=self.timeout)
            
            execution_time = time.time() - start_time
            
            return {
                "return_code": self._current_process.returncode,
                "stdout": stdout,
                "stderr": stderr,
                "execution_time": execution_time
            }
            
        except subprocess.TimeoutExpired:
            self._current_process.kill()
            stdout, stderr = self._current_process.communicate()
            
            return {
                "return_code": -1,
                "stdout": stdout,
                "stderr": stderr + "\n[ERROR] 测试执行超时",
                "execution_time": self.timeout
            }
            
        except Exception as e:
            return {
                "return_code": -1,
                "stdout": "",
                "stderr": str(e),
                "execution_time": time.time() - start_time
            }
            
        finally:
            self._current_process = None
    
    def _get_report_file(self, test_case_name: str) -> Optional[str]:
        """获取测试报告文件路径"""
        report_file = os.path.join(self.report_path, f"{test_case_name}.{self.report_format}")
        return report_file if os.path.isfile(report_file) else None
    
    def _get_log_file(self, test_case_name: str) -> Optional[str]:
        """获取测试日志文件路径"""
        log_file = os.path.join(self.log_path, f"{test_case_name}.tlz")
        return log_file if os.path.isfile(log_file) else None
    
    def read_report_content(self, report_file: str) -> Optional[str]:
        """
        读取报告文件内容
        
        Args:
            report_file: 报告文件路径
            
        Returns:
            报告内容（如果是文本格式）或None
        """
        try:
            if not os.path.isfile(report_file):
                return None
            
            # 如果是PDF，返回文件路径
            if report_file.endswith('.pdf'):
                return f"[PDF文件] {report_file}"
            
            # 如果是HTML/XML，读取内容
            with open(report_file, 'r', encoding='utf-8', errors='ignore') as f:
                return f.read()
                
        except Exception as e:
            self.logger.warning(f"读取报告文件失败: {str(e)}")
            return None
    
    def get_execution_summary(self) -> Dict[str, Any]:
        """
        获取执行摘要
        
        Returns:
            执行摘要字典
        """
        return {
            "tool_type": self.tool_type.value,
            "status": self.status.name,
            "workspace_path": self.workspace_path,
            "report_path": self.report_path,
            "log_path": self.log_path,
            "report_format": self.report_format
        }
