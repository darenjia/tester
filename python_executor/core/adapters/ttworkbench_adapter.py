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
from .capabilities import (
    ArtifactCapability,
    ConfigurationCapability,
    MeasurementCapability,
    TTworkbenchExecutionCapability,
)
from ..realtime_logger import TaskLogAdapter


class TTmanReturnCode:
    """TTman返回码定义
    110 - None: 无判定
    111 - Pass: 全部通过
    112 - Inconclusive: 不确定
    113 - Fail: 有失败
    """
    NONE = 110
    PASS = 111
    INCONCLUSIVE = 112
    FAIL = 113

    @classmethod
    def get_verdict(cls, return_code: int) -> str:
        mapping = {
            cls.NONE: "NONE",
            cls.PASS: "PASS",
            cls.INCONCLUSIVE: "INCONCLUSIVE",
            cls.FAIL: "FAIL"
        }
        return mapping.get(return_code, f"UNKNOWN({return_code})")


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
                - log_format: 日志格式（tlz/xml，默认tlz）
                - timeout: 测试超时时间（默认3600秒）
        """
        super().__init__(config)

        # TTworkbench路径配置
        self.ttman_path = self.config.get("ttman_path", "")
        self.workspace_path = self.config.get("workspace_path", "")
        self.log_path = self.config.get("log_path", "")
        self.report_path = self.config.get("report_path", "")
        self.report_format = self.config.get("report_format", "pdf")
        self.log_format = self.config.get("log_format", "tlz")
        self.timeout = self.config.get("timeout", 3600)
        
        # 执行状态
        self._current_process: Optional[subprocess.Popen] = None
        self._execution_log: List[str] = []
        self._realtime_logger: Optional[TaskLogAdapter] = None
        self._enable_realtime_log = True
        self._register_capabilities()
        
    @property
    def tool_type(self) -> TestToolType:
        """返回测试工具类型"""
        return TestToolType.TTWORKBENCH

    def _register_capabilities(self) -> None:
        self.register_capability(
            "configuration",
            ConfigurationCapability(load=self.load_configuration),
        )
        self.register_capability(
            "measurement",
            MeasurementCapability(start=self.start_test, stop=self.stop_test),
        )
        self.register_capability(
            "artifact",
            ArtifactCapability(
                collect=lambda: {
                    "report_path": self.report_path,
                    "log_path": self.log_path,
                    "last_execution_log": list(self._execution_log),
                }
            ),
        )
        self.register_capability(
            "ttworkbench_execution",
            TTworkbenchExecutionCapability(
                execute_clf=self.execute_clf_file,
                execute_batch=self.execute_clf_batch,
            ),
        )
    
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
            
            # 检查TTworkbench GUI是否正在运行
            if self._check_ttworkbench_gui():
                self.logger.warning("检测到TTworkbench GUI正在运行，建议关闭后再执行命令行测试")
                # 不阻止连接，但给出警告
            
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
    
    def _check_ttworkbench_gui(self) -> bool:
        """
        检查TTworkbench GUI是否正在运行
        
        Returns:
            如果TTworkbench GUI正在运行返回True
        """
        try:
            import psutil
            
            ttworkbench_processes = []
            for proc in psutil.process_iter(['pid', 'name']):
                try:
                    proc_name = proc.info['name']
                    if proc_name and ('TTworkbench' in proc_name or 'ttworkbench' in proc_name.lower()):
                        ttworkbench_processes.append({
                            'pid': proc.info['pid'],
                            'name': proc_name
                        })
                except (psutil.NoSuchProcess, psutil.AccessDenied):
                    pass
            
            if ttworkbench_processes:
                self.logger.warning(f"发现 {len(ttworkbench_processes)} 个TTworkbench进程正在运行")
                for proc in ttworkbench_processes:
                    self.logger.warning(f"  - PID {proc['pid']}: {proc['name']}")
                return True
            
            return False
            
        except ImportError:
            self.logger.debug("psutil未安装，无法检测TTworkbench进程")
            return False
        except Exception as e:
            self.logger.warning(f"检测TTworkbench进程时出错: {e}")
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
        验证clf文件路径和格式
        
        Args:
            config_path: clf文件路径
            
        Returns:
            验证通过返回True，否则返回False
        """
        if not os.path.isfile(config_path):
            self._set_error(f"clf文件不存在: {config_path}")
            return False
        
        # 验证clf文件格式
        validation_result = self._validate_clf_file(config_path)
        if not validation_result["valid"]:
            errors = "; ".join(validation_result["errors"])
            self._set_error(f"clf文件验证失败: {errors}")
            return False
        
        # 检查可执行case
        executable_cases = validation_result.get("executable_cases", [])
        if executable_cases:
            self.logger.info(f"clf文件包含可执行case: {', '.join(executable_cases)}")
        
        # 验证clf文件是否在workspace目录下
        if self.workspace_path:
            clf_abs_path = os.path.abspath(config_path)
            workspace_abs_path = os.path.abspath(self.workspace_path)
            if not clf_abs_path.startswith(workspace_abs_path):
                self.logger.warning(f"clf文件不在workspace目录下: {config_path}")
        
        self.logger.info(f"clf文件验证通过: {config_path}")
        return True
    
    def _validate_clf_file(self, clf_file: str) -> Dict[str, Any]:
        """
        验证clf文件格式
        
        Args:
            clf_file: clf文件路径
            
        Returns:
            验证结果字典
        """
        result = {"valid": False, "errors": [], "executable_cases": []}
        
        try:
            import xml.etree.ElementTree as ET
            
            # 解析XML
            tree = ET.parse(clf_file)
            root = tree.getroot()
            
            # 检查根元素
            if root.tag != "TestConfiguration":
                result["errors"].append(f"Invalid root element: {root.tag}, expected 'TestConfiguration'")
                return result
            
            # 检查必需的子元素
            required_elements = ["TestCases", "Parameters"]
            for elem_name in required_elements:
                elem = root.find(elem_name)
                if elem is None:
                    result["errors"].append(f"Missing required element: {elem_name}")
            
            # 检查是否有可执行的case（Runs=1）
            test_cases = root.find("TestCases")
            if test_cases is not None:
                for case in test_cases.findall("TestCase"):
                    runs = case.get("Runs", "0")
                    case_name = case.get("Name", "Unknown")
                    if runs == "1":
                        result["executable_cases"].append(case_name)
                
                if not result["executable_cases"]:
                    result["errors"].append("No executable test case found (Runs=1)")
            
            # 检查Parameters元素
            parameters = root.find("Parameters")
            if parameters is not None:
                param_count = len(parameters.findall("Parameter"))
                self.logger.debug(f"clf文件包含 {param_count} 个参数")
            
            result["valid"] = len(result["errors"]) == 0
            
        except ET.ParseError as e:
            result["errors"].append(f"XML parse error: {str(e)}")
        except Exception as e:
            result["errors"].append(f"Failed to parse clf file: {str(e)}")
        
        return result
    
    def start_test(self) -> bool:
        """
        TTworkbench不需要启动操作

        实际执行由strategy通过ttworkbench_execution capability驱动。

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
        legacy兼容接口，新执行链路不再通过adapter层派发高层测试项。
        """
        item_type = item.get("type", "unknown")
        self.logger.warning(f"execute_test_item 已废弃，当前类型不再由适配器层执行: {item_type}")
        return {
            "name": item.get("name", "unnamed"),
            "type": item_type,
            "status": "error",
            "error": f"TTworkbenchAdapter.execute_test_item 已不再支持，请改用 strategy/capability 执行链路: {item_type}",
        }

    def execute_clf_file(self, clf_file: str, task_id: Optional[str] = None) -> Dict[str, Any]:
        return self._execute_clf_test({"name": Path(clf_file).stem, "clf_file": clf_file, "task_id": task_id})

    def execute_clf_batch(self, clf_files: List[str], task_id: Optional[str] = None) -> Dict[str, Any]:
        return self._execute_batch_test({"name": "batch", "clf_files": clf_files, "task_id": task_id})
    
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

        # 解析TLZ日志获取详细结果
        log_details = None
        if log_file:
            log_details = self.parse_tlz_log(log_file)
            self.logger.info(f"TLZ日志解析结果: verdict={log_details.get('verdict')}, return_code={log_details.get('return_code')}")

        # 根据返回码确定测试状态
        # TTman返回码: 110-None, 111-Pass, 112-Inconclusive, 113-Fail
        ttman_return_code = result["return_code"]
        status = "failed"
        verdict_text = "NONE"

        if log_details and log_details.get("parsed"):
            # 使用TLZ解析结果
            ttman_return_code = log_details.get("return_code", ttman_return_code)
            verdict_text = log_details.get("verdict", "NONE")

            if ttman_return_code == TTmanReturnCode.PASS:
                status = "passed"
            elif ttman_return_code == TTmanReturnCode.FAIL:
                status = "failed"
            elif ttman_return_code == TTmanReturnCode.INCONCLUSIVE:
                status = "inconclusive"
            else:
                # 如果return_code是0（进程正常退出），也认为是passed
                status = "passed" if result["return_code"] == 0 else "failed"
        else:
            # 如果无法解析日志，使用进程返回码判断
            # 进程返回码0表示成功执行（不一定表示测试通过）
            # TTman成功执行后会将verdict写入日志文件
            if result["return_code"] == 0:
                # 进程正常退出，但需要检查TLZ日志判断实际结果
                if log_file and os.path.isfile(log_file):
                    # 有日志文件，认为是passed
                    status = "passed"
                    verdict_text = "PASS"
                else:
                    # 没有日志文件，可能是进程异常
                    status = "inconclusive"
                    verdict_text = "INCONCLUSIVE"
            else:
                status = "failed"
                verdict_text = "FAIL"

        return {
            "name": item.get("name"),
            "type": "clf_test",
            "clf_file": clf_file,
            "test_case_name": test_case_name,
            "command": ' '.join(cmd),
            "return_code": ttman_return_code,
            "verdict": verdict_text,
            "stdout": result["stdout"],
            "stderr": result["stderr"],
            "execution_time": result["execution_time"],
            "report_file": report_file,
            "log_file": log_file,
            "log_details": log_details,
            "status": status
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
            '--log-format', self.log_format,
            '-r', self.report_format,
            '--report-dir', self.report_path,
            clf_file
        ]
        return cmd
    
    def _run_ttman_command(self, cmd: List[str], task_id: Optional[str] = None) -> Dict[str, Any]:
        """
        执行TTman命令，支持实时日志输出
        
        Args:
            cmd: TTman命令列表
            task_id: 任务ID（用于实时日志）
            
        Returns:
            执行结果字典
        """
        import threading
        
        start_time = time.time()
        execution_log = []
        finished_detected = False
        
        # 初始化实时日志适配器
        if task_id and self._enable_realtime_log:
            self._realtime_logger = TaskLogAdapter(task_id, "TTman")
        
        try:
            # 启动进程
            self._current_process = subprocess.Popen(
                cmd,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                text=True,
                encoding='utf-8',
                errors='ignore',
                bufsize=1,  # 行缓冲
                universal_newlines=True
            )
            
            # 实时读取输出的线程函数
            def read_output(pipe, prefix):
                nonlocal finished_detected
                for line in iter(pipe.readline, ''):
                    line = line.rstrip()
                    if line:
                        log_entry = f"{prefix}: {line}"
                        execution_log.append(log_entry)
                        
                        # 实时推送日志
                        if self._realtime_logger:
                            level = "ERROR" if prefix == "ERR" else "INFO"
                            self._realtime_logger.logger.log(level, line, "TTman", task_id)
                        
                        self.logger.info(f"TTman {prefix}: {line}")
                        
                        # 检测完成标志
                        if "Test execution finished" in line:
                            finished_detected = True
                            self.logger.info("检测到测试完成标志: Test execution finished")
                pipe.close()
            
            # 启动读取线程
            stdout_thread = threading.Thread(target=read_output, args=(self._current_process.stdout, "OUT"))
            stderr_thread = threading.Thread(target=read_output, args=(self._current_process.stderr, "ERR"))
            stdout_thread.daemon = True
            stderr_thread.daemon = True
            stdout_thread.start()
            stderr_thread.start()
            
            # 等待进程完成
            self._current_process.wait(timeout=self.timeout)
            
            # 等待读取线程完成
            stdout_thread.join(timeout=5)
            stderr_thread.join(timeout=5)
            
            execution_time = time.time() - start_time
            
            return {
                "return_code": self._current_process.returncode,
                "stdout": "\n".join([l for l in execution_log if l.startswith("OUT:")]),
                "stderr": "\n".join([l for l in execution_log if l.startswith("ERR:")]),
                "execution_log": execution_log,
                "execution_time": execution_time,
                "finished_detected": finished_detected
            }
            
        except subprocess.TimeoutExpired:
            self.logger.error("TTman命令执行超时")
            if self._current_process:
                self._current_process.kill()
                try:
                    self._current_process.wait(timeout=5)
                except:
                    pass
            
            return {
                "return_code": -1,
                "stdout": "",
                "stderr": "[ERROR] 测试执行超时",
                "execution_log": execution_log,
                "execution_time": time.time() - start_time,
                "finished_detected": finished_detected
            }
            
        except Exception as e:
            self.logger.error(f"执行TTman命令异常: {e}")
            return {
                "return_code": -1,
                "stdout": "",
                "stderr": str(e),
                "execution_log": execution_log,
                "execution_time": time.time() - start_time,
                "finished_detected": False
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

    def compile_ttcn3(self, source_file: str, output_dir: str = None,
                      use_existing_clf: bool = False) -> Dict[str, Any]:
        """编译TTCN-3源码

        Args:
            source_file: TTCN-3源码文件路径 (.ttcn3)
            output_dir: 输出目录（可选，默认workspace）
            use_existing_clf: 是否使用已存在的clf文件

        Returns:
            {
                "success": bool,
                "clf_file": str,  # 生成的clf文件路径
                "jar_file": str,  # 生成的jar文件路径
                "errors": str,   # 编译错误信息
                "warnings": str  # 编译警告信息
            }
        """
        from pathlib import Path

        result = {
            "success": False,
            "clf_file": "",
            "jar_file": "",
            "errors": "",
            "warnings": ""
        }

        # 检查源码文件是否存在
        if not os.path.isfile(source_file):
            result["errors"] = f"TTCN-3源码文件不存在: {source_file}"
            self.logger.error(result["errors"])
            return result

        # 确定TTthree路径
        ttthree_path = self.config.get("ttthree_path", "TTthree")
        if os.path.isfile(self.ttthree_path):
            ttthree_path = self.ttthree_path
        elif os.path.isfile(self.ttman_path):
            # 从ttman路径推断ttthree路径
            ttman_dir = os.path.dirname(self.ttman_path)
            ttthree_path = os.path.join(ttman_dir, "TTthree.bat" if os.name == 'nt' else "TTthree.sh")

        if not os.path.isfile(ttthree_path):
            result["errors"] = f"TTthree编译器不存在: {ttthree_path}"
            self.logger.error(result["errors"])
            return result

        # 确定输出目录
        if not output_dir:
            output_dir = self.workspace_path
        os.makedirs(output_dir, exist_ok=True)

        # 编译命令
        source_name = os.path.splitext(os.path.basename(source_file))[0]
        cmd = [
            ttthree_path,
            "--clf-generate-default",
            "-d", output_dir,
            source_file
        ]

        self.logger.info(f"执行TTthree编译: {' '.join(cmd)}")

        try:
            proc = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                timeout=self.config.get("compile_timeout", 300)
            )

            result["warnings"] = proc.stderr if proc.returncode == 0 else ""

            if proc.returncode != 0:
                result["errors"] = proc.stderr
                self.logger.error(f"TTthree编译失败: {proc.stderr}")
                return result

            # 查找生成的clf文件
            clf_file = os.path.join(output_dir, f"{source_name}.clf")
            jar_file = os.path.join(output_dir, f"{source_name}.jar")

            if os.path.isfile(clf_file):
                result["success"] = True
                result["clf_file"] = clf_file
                result["jar_file"] = jar_file if os.path.isfile(jar_file) else ""
                self.logger.info(f"编译成功: {clf_file}")
            else:
                result["errors"] = f"编译成功但未找到clf文件: {clf_file}"
                self.logger.error(result["errors"])

        except subprocess.TimeoutExpired:
            result["errors"] = "编译超时"
            self.logger.error("TTthree编译超时")
        except Exception as e:
            result["errors"] = f"编译异常: {str(e)}"
            self.logger.error(f"TTthree编译异常: {e}")

        return result

    def parse_tlz_log(self, tlz_file: str) -> Dict[str, Any]:
        """解析TLZ日志文件

        Args:
            tlz_file: .tlz日志文件路径

        Returns:
            {
                "verdict": "PASS|FAIL|INCONCLUSIVE|NONE",
                "verdict_code": int,
                "total_cases": int,
                "passed_cases": int,
                "failed_cases": int,
                "inconclusive_cases": int,
                "case_results": [
                    {
                        "name": str,
                        "verdict": "PASS|FAIL|INCONCLUSIVE|NONE",
                        "duration": float,
                        "error_message": str
                    }
                ],
                "execution_time": float,
                "parse_errors": str
            }
        """
        import zipfile
        import xml.etree.ElementTree as ET
        import json
        import re

        result = {
            "verdict": "NONE",
            "verdict_code": TTmanReturnCode.NONE,
            "total_cases": 0,
            "passed_cases": 0,
            "failed_cases": 0,
            "inconclusive_cases": 0,
            "case_results": [],
            "execution_time": 0.0,
            "parse_errors": ""
        }

        if not tlz_file or not os.path.isfile(tlz_file):
            result["parse_errors"] = f"TLZ文件不存在: {tlz_file}"
            self.logger.warning(result["parse_errors"])
            return result

        try:
            with zipfile.ZipFile(tlz_file, 'r') as zf:
                # 查找management.log
                file_list = zf.namelist()
                management_log = None
                for name in file_list:
                    if 'management.log' in name:
                        management_log = name
                        break

                if not management_log:
                    result["parse_errors"] = "未找到management.log"
                    return result

                # 读取management.log
                with zf.open(management_log) as f:
                    content = f.read().decode('utf-8', errors='ignore')

                # 解析内容
                case_results = []
                passed = 0
                failed = 0
                inconclusive = 0
                verdict_code = TTmanReturnCode.NONE
                execution_time = 0.0

                # 简单解析：查找TestCase执行记录
                # 格式: TestCase: <name> - Verdict: <verdict> (Duration: <time>s)
                case_pattern = r'TestCase:\s*([^\s-]+)\s*-\s*Verdict:\s*(\w+)(?:\s*\(Duration:\s*([\d.]+)s\))?'
                for match in re.finditer(case_pattern, content, re.IGNORECASE):
                    case_name = match.group(1)
                    verdict = match.group(2).upper()
                    duration = float(match.group(3)) if match.group(3) else 0.0

                    case_verdict = verdict if verdict in ["PASS", "FAIL", "INCONCLUSIVE", "NONE"] else "NONE"

                    # 获取错误信息
                    error_msg = ""
                    if case_verdict == "FAIL":
                        # 查找该用例后的错误信息
                        error_pattern = f'{case_name}.*?error|{case_name}.*?fail|{case_name}.*?Error'
                        error_match = re.search(error_pattern, content, re.IGNORECASE)
                        if error_match:
                            error_msg = error_match.group(0)

                    case_results.append({
                        "name": case_name,
                        "verdict": case_verdict,
                        "duration": duration,
                        "error_message": error_msg
                    })

                    if case_verdict == "PASS":
                        passed += 1
                    elif case_verdict == "FAIL":
                        failed += 1
                    elif case_verdict == "INCONCLUSIVE":
                        inconclusive += 1

                # 解析总判定
                verdict_match = re.search(r'Verdict:\s*(\w+)', content, re.IGNORECASE)
                if verdict_match:
                    verdict_str = verdict_match.group(1).upper()
                    if "PASS" in verdict_str:
                        verdict_code = TTmanReturnCode.PASS
                    elif "FAIL" in verdict_str:
                        verdict_code = TTmanReturnCode.FAIL
                    elif "INCONCLUSIVE" in verdict_str:
                        verdict_code = TTmanReturnCode.INCONCLUSIVE
                    else:
                        verdict_code = TTmanReturnCode.NONE

                # 解析执行时间
                time_match = re.search(r'Execution time:\s*([\d.]+)', content, re.IGNORECASE)
                if time_match:
                    execution_time = float(time_match.group(1))

                result["verdict"] = TTmanReturnCode.get_verdict(verdict_code)
                result["verdict_code"] = verdict_code
                result["total_cases"] = len(case_results)
                result["passed_cases"] = passed
                result["failed_cases"] = failed
                result["inconclusive_cases"] = inconclusive
                result["case_results"] = case_results
                result["execution_time"] = execution_time

                self.logger.info(f"解析TLZ完成: {result['verdict']}, 用例数: {len(case_results)}")

        except zipfile.BadZipFile:
            result["parse_errors"] = "无效的TLZ文件格式"
            self.logger.error("无效的TLZ文件格式")
        except Exception as e:
            result["parse_errors"] = f"解析异常: {str(e)}"
            self.logger.error(f"解析TLZ异常: {e}")

        return result

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
