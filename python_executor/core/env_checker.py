"""
环境检测模块
用于检测系统环境、软件安装、配置等
"""
import os
import sys
import time
import threading
import subprocess
import json
from typing import Dict, Any, List, Optional, Callable
from datetime import datetime
from dataclasses import dataclass, field

import sys
import os
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from models.env_check_log import EnvCheckLog, env_check_log_manager
from config.settings import get_config
from core.software_validator import get_software_validator


@dataclass
class CheckResult:
    """单项检测结果"""
    name: str
    status: str  # passed/failed/warning
    message: str
    details: Dict[str, Any] = field(default_factory=dict)


class EnvironmentChecker:
    """
    环境检测器
    
    检测系统环境、软件安装、配置等
    """
    
    def __init__(self):
        self._checking = False
        self._progress = 0
        self._current_check = ""
        self._lock = threading.Lock()
        self._callbacks: List[Callable] = []
        self._check_results: List[CheckResult] = []
        self._log_messages: List[str] = []
        
    def is_checking(self) -> bool:
        """是否正在检测"""
        with self._lock:
            return self._checking
    
    def get_progress(self) -> int:
        """获取检测进度"""
        with self._lock:
            return self._progress
    
    def get_current_check(self) -> str:
        """获取当前检测项"""
        with self._lock:
            return self._current_check
    
    def add_callback(self, callback: Callable):
        """添加进度回调函数"""
        self._callbacks.append(callback)
    
    def _update_progress(self, progress: int, current: str):
        """更新进度"""
        with self._lock:
            self._progress = progress
            self._current_check = current
        
        # 通知回调
        for callback in self._callbacks:
            try:
                callback(progress, current)
            except Exception as e:
                print(f"回调函数执行失败: {e}")
    
    def _log(self, message: str):
        """记录日志"""
        timestamp = datetime.now().strftime("%H:%M:%S")
        log_msg = f"[{timestamp}] {message}"
        self._log_messages.append(log_msg)
        print(log_msg)
    
    def check_all(self) -> Dict[str, Any]:
        """
        执行所有环境检测
        
        Returns:
            检测结果字典
        """
        with self._lock:
            if self._checking:
                return {"success": False, "message": "检测正在进行中"}
            self._checking = True
            self._progress = 0
            self._current_check = ""
            self._check_results = []
            self._log_messages = []
        
        start_time = time.time()
        
        try:
            self._log("开始环境检测...")
            
            # 1. 检测 Python 环境
            self._update_progress(10, "检测 Python 环境")
            self._check_python()
            
            # 2. 检测系统环境
            self._update_progress(30, "检测系统环境")
            self._check_system()
            
            # 3. 检测软件安装
            self._update_progress(50, "检测软件安装")
            self._check_software()
            
            # 4. 检测配置文件
            self._update_progress(70, "检测配置文件")
            self._check_config()
            
            # 5. 检测网络连接
            self._update_progress(90, "检测网络连接")
            self._check_network()
            
            # 完成
            self._update_progress(100, "检测完成")
            
            duration = time.time() - start_time
            
            # 统计结果
            passed = len([r for r in self._check_results if r.status == "passed"])
            failed = len([r for r in self._check_results if r.status == "failed"])
            warning = len([r for r in self._check_results if r.status == "warning"])
            
            overall_status = "passed" if failed == 0 else "failed"
            
            self._log(f"检测完成，耗时 {duration:.2f} 秒")
            self._log(f"通过: {passed}, 失败: {failed}, 警告: {warning}")
            
            # 保存日志
            log = EnvCheckLog(
                check_time=datetime.now().isoformat(),
                duration=duration,
                overall_status=overall_status,
                details={
                    "passed": passed,
                    "failed": failed,
                    "warning": warning,
                    "results": [self._result_to_dict(r) for r in self._check_results]
                },
                logs=self._log_messages
            )
            env_check_log_manager.add_log(log)
            
            return {
                "success": True,
                "status": overall_status,
                "duration": duration,
                "passed": passed,
                "failed": failed,
                "warning": warning,
                "results": [self._result_to_dict(r) for r in self._check_results],
                "log_id": log.id
            }
            
        except Exception as e:
            self._log(f"检测过程出错: {e}")
            return {
                "success": False,
                "message": f"检测失败: {str(e)}"
            }
        finally:
            with self._lock:
                self._checking = False
    
    def _check_python(self):
        """检测 Python 环境"""
        self._log("检测 Python 环境...")
        
        try:
            version = sys.version
            self._log(f"Python 版本: {version}")
            
            # 检测必要模块
            required_modules = ['flask', 'requests', 'websocket']
            missing = []
            
            for module in required_modules:
                try:
                    __import__(module)
                    self._log(f"✓ 模块 {module} 已安装")
                except ImportError:
                    missing.append(module)
                    self._log(f"✗ 模块 {module} 未安装")
            
            if missing:
                self._add_result("Python 环境", "warning", 
                    f"Python 运行正常，但缺少模块: {', '.join(missing)}",
                    {"version": version, "missing_modules": missing})
            else:
                self._add_result("Python 环境", "passed", 
                    f"Python {version} 运行正常，所有必要模块已安装",
                    {"version": version})
                    
        except Exception as e:
            self._log(f"✗ Python 环境检测失败: {e}")
            self._add_result("Python 环境", "failed", f"检测失败: {str(e)}")
    
    def _check_system(self):
        """检测系统环境"""
        self._log("检测系统环境...")
        
        try:
            import platform
            system = platform.system()
            release = platform.release()
            
            self._log(f"操作系统: {system} {release}")
            
            # 检测内存
            try:
                import psutil
                memory = psutil.virtual_memory()
                total_gb = memory.total / (1024**3)
                available_gb = memory.available / (1024**3)
                
                self._log(f"内存: 总计 {total_gb:.1f}GB, 可用 {available_gb:.1f}GB")
                
                if available_gb < 1:
                    self._add_result("系统内存", "warning", 
                        f"可用内存不足: {available_gb:.1f}GB",
                        {"total": total_gb, "available": available_gb})
                else:
                    self._add_result("系统内存", "passed", 
                        f"内存充足: 总计 {total_gb:.1f}GB, 可用 {available_gb:.1f}GB",
                        {"total": total_gb, "available": available_gb})
            except ImportError:
                self._log("未安装 psutil，跳过内存检测")
                self._add_result("系统内存", "warning", "无法检测内存（缺少 psutil 模块）")
            
            # 检测磁盘空间
            try:
                import shutil
                total, used, free = shutil.disk_usage("/" if system != "Windows" else "C:\\")
                free_gb = free / (1024**3)
                
                self._log(f"磁盘空间: 可用 {free_gb:.1f}GB")
                
                if free_gb < 5:
                    self._add_result("磁盘空间", "warning", 
                        f"磁盘空间不足: 仅剩 {free_gb:.1f}GB",
                        {"free_gb": free_gb})
                else:
                    self._add_result("磁盘空间", "passed", 
                        f"磁盘空间充足: {free_gb:.1f}GB",
                        {"free_gb": free_gb})
            except Exception as e:
                self._log(f"磁盘空间检测失败: {e}")
                self._add_result("磁盘空间", "warning", f"无法检测磁盘空间: {str(e)}")
            
            self._add_result("操作系统", "passed", 
                f"{system} {release}",
                {"system": system, "release": release})
                
        except Exception as e:
            self._log(f"✗ 系统环境检测失败: {e}")
            self._add_result("系统环境", "failed", f"检测失败: {str(e)}")
    
    def _check_software(self):
        """检测软件安装和调用能力"""
        self._log("检测软件安装和调用能力...")
        
        # 使用软件验证器进行深度检测
        validator = get_software_validator()
        
        def progress_callback(step, total, desc):
            self._update_progress(50 + int(step / total * 20), desc)
        
        software_results = validator.validate_all(progress_callback)
        
        # 处理 CANoe 检测结果
        canoe_result = software_results.get('canoe', {})
        self._process_canoe_result(canoe_result)
        
        # 处理 TSMaster 检测结果
        tsmaster_result = software_results.get('tsmaster', {})
        self._process_tsmaster_result(tsmaster_result)
        
        # 处理 TTman 检测结果
        ttman_result = software_results.get('ttman', {})
        self._process_ttman_result(ttman_result)
    
    def _process_canoe_result(self, result: Dict[str, Any]):
        """处理 CANoe 检测结果"""
        if not result.get('installed'):
            self._log(f"✗ CANoe 未安装: {result.get('error', '未知错误')}")
            self._add_result("CANoe", "warning", 
                f"未安装: {result.get('error', '路径不存在')}", 
                {"path": result.get('path', '')})
            return
        
        adapter_test = result.get('adapter_test', {})
        config_check = result.get('config_check', {})
        
        # 检查适配器测试结果
        if adapter_test.get('interface_accessible'):
            version = adapter_test.get('version', '未知版本')
            self._log(f"✓ CANoe 调用测试通过，版本: {version}")
            
            details = {
                "path": result.get('path', ''),
                "version": version,
                "pywin32_available": adapter_test.get('pywin32_available', False),
                "com_object_created": adapter_test.get('com_object_created', False),
                "interface_accessible": True,
                "config_path": config_check.get('config_path', ''),
                "config_path_exists": config_check.get('path_exists', False),
                "cfg_files_count": config_check.get('valid_cfg_files', 0)
            }
            
            # 检查配置文件
            if config_check.get('path_exists'):
                cfg_count = config_check.get('valid_cfg_files', 0)
                self._log(f"✓ CANoe 配置文件路径存在，找到 {cfg_count} 个cfg文件")
                
                if cfg_count > 0:
                    self._add_result("CANoe", "passed", 
                        f"调用测试通过，版本 {version}，找到 {cfg_count} 个配置文件", 
                        details)
                else:
                    self._add_result("CANoe", "warning", 
                        f"调用测试通过，但未找到有效的cfg配置文件", 
                        details)
            else:
                self._log(f"⚠ CANoe 配置文件路径不存在")
                self._add_result("CANoe", "warning", 
                    f"调用测试通过，但配置文件路径不存在", 
                    details)
        else:
            error = result.get('error', '调用测试失败')
            self._log(f"✗ CANoe 调用测试失败: {error}")
            
            details = {
                "path": result.get('path', ''),
                "pywin32_available": adapter_test.get('pywin32_available', False),
                "com_object_created": adapter_test.get('com_object_created', False),
                "error": error
            }
            
            self._add_result("CANoe", "failed", f"调用测试失败: {error}", details)
    
    def _process_tsmaster_result(self, result: Dict[str, Any]):
        """处理 TSMaster 检测结果"""
        if not result.get('installed'):
            self._log(f"✗ TSMaster 未安装: {result.get('error', '未知错误')}")
            self._add_result("TSMaster", "warning", 
                f"未安装: {result.get('error', '路径不存在')}", 
                {"path": result.get('path', '')})
            return
        
        adapter_test = result.get('adapter_test', {})
        config_check = result.get('config_check', {})
        
        # 检查适配器测试结果
        if adapter_test.get('connection_test') == 'success':
            version = adapter_test.get('version', '未知版本')
            self._log(f"✓ TSMaster 调用测试通过，版本: {version}")
            
            details = {
                "path": result.get('path', ''),
                "version": version,
                "python_api_available": adapter_test.get('python_api_available', False),
                "rpc_api_available": adapter_test.get('rpc_api_available', False),
                "connection_test": "success",
                "config_path": config_check.get('config_path', ''),
                "config_path_exists": config_check.get('path_exists', False),
                "tproj_files_count": config_check.get('valid_tproj_files', 0)
            }
            
            # 检查配置文件
            if config_check.get('path_exists'):
                tproj_count = config_check.get('valid_tproj_files', 0)
                self._log(f"✓ TSMaster 配置文件路径存在，找到 {tproj_count} 个tproj文件")
                
                if tproj_count > 0:
                    self._add_result("TSMaster", "passed", 
                        f"调用测试通过，版本 {version}，找到 {tproj_count} 个工程文件", 
                        details)
                else:
                    self._add_result("TSMaster", "warning", 
                        f"调用测试通过，但未找到有效的tproj工程文件", 
                        details)
            else:
                self._log(f"⚠ TSMaster 配置文件路径不存在")
                self._add_result("TSMaster", "warning", 
                    f"调用测试通过，但配置文件路径不存在", 
                    details)
        else:
            error = result.get('error', '调用测试失败')
            connection_error = adapter_test.get('connection_test', 'failed')
            self._log(f"✗ TSMaster 调用测试失败: {error}")
            
            details = {
                "path": result.get('path', ''),
                "python_api_available": adapter_test.get('python_api_available', False),
                "rpc_api_available": adapter_test.get('rpc_api_available', False),
                "connection_test": connection_error,
                "error": error
            }
            
            self._add_result("TSMaster", "failed", f"调用测试失败: {error}", details)
    
    def _process_ttman_result(self, result: Dict[str, Any]):
        """处理 TTman 检测结果"""
        if not result.get('installed'):
            self._log(f"✗ TTman 未安装: {result.get('error', '未知错误')}")
            self._add_result("TTman", "warning", 
                f"未安装: {result.get('error', '路径不存在')}", 
                {"path": result.get('path', '')})
            return
        
        adapter_test = result.get('adapter_test', {})
        config_check = result.get('config_check', {})
        
        if adapter_test.get('script_readable'):
            self._log(f"✓ TTman 脚本可读")
            
            details = {
                "path": result.get('path', ''),
                "script_readable": True,
                "valid_script": adapter_test.get('valid_script', False),
                "install_dir": config_check.get('install_dir', ''),
                "jar_files": config_check.get('jar_files', []),
                "config_files": config_check.get('config_files', [])
            }
            
            jar_count = len(config_check.get('jar_files', []))
            if jar_count > 0:
                self._log(f"✓ TTman 找到 {jar_count} 个jar文件")
                self._add_result("TTman", "passed", 
                    f"环境检查通过，找到 {jar_count} 个jar文件", 
                    details)
            else:
                self._log(f"⚠ TTman 未找到jar文件")
                self._add_result("TTman", "warning", 
                    f"脚本可读，但未找到jar文件", 
                    details)
        else:
            error = result.get('error', '脚本读取失败')
            self._log(f"✗ TTman 检查失败: {error}")
            
            details = {
                "path": result.get('path', ''),
                "script_readable": False,
                "error": error
            }
            
            self._add_result("TTman", "failed", f"检查失败: {error}", details)
    
    def _check_config(self):
        """检测配置文件"""
        self._log("检测配置文件...")
        
        try:
            config = get_config()
            
            # 检测必要配置项
            required_configs = [
                ('http.port', 'HTTP 端口'),
                ('device.device_id', '设备ID'),
            ]
            
            all_ok = True
            for key, name in required_configs:
                value = config.get(key)
                if value:
                    self._log(f"✓ {name}: {value}")
                else:
                    self._log(f"✗ {name} 未配置")
                    all_ok = False
            
            if all_ok:
                self._add_result("配置文件", "passed", "所有必要配置项已设置")
            else:
                self._add_result("配置文件", "warning", "部分配置项未设置")
                
        except Exception as e:
            self._log(f"✗ 配置文件检测失败: {e}")
            self._add_result("配置文件", "failed", f"检测失败: {str(e)}")
    
    def _check_network(self):
        """检测网络连接"""
        self._log("检测网络连接...")
        
        # 检测本地 HTTP 服务
        try:
            import urllib.request
            port = get_config().get('http.port', 8180)
            url = f"http://127.0.0.1:{port}/api/status"
            
            self._log(f"测试连接: {url}")
            
            with urllib.request.urlopen(url, timeout=5) as response:
                if response.status == 200:
                    self._log("✓ HTTP 服务正常")
                    self._add_result("HTTP 服务", "passed", f"端口 {port} 运行正常")
                else:
                    self._log(f"✗ HTTP 服务异常: {response.status}")
                    self._add_result("HTTP 服务", "failed", f"返回状态码: {response.status}")
        except Exception as e:
            self._log(f"✗ HTTP 服务检测失败: {e}")
            self._add_result("HTTP 服务", "failed", f"连接失败: {str(e)}")
    
    def _add_result(self, name: str, status: str, message: str, details: Dict = None):
        """添加检测结果"""
        result = CheckResult(
            name=name,
            status=status,
            message=message,
            details=details or {}
        )
        self._check_results.append(result)
    
    def _result_to_dict(self, result: CheckResult) -> Dict[str, Any]:
        """转换结果为字典"""
        return {
            "name": result.name,
            "status": result.status,
            "message": result.message,
            "details": result.details
        }


# 全局环境检测器实例
env_checker = EnvironmentChecker()
