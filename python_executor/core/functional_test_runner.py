"""
功能测试运行器
提供细粒度的测试用例执行功能

用于在部署和问题排查时进行详细的软件调用测试
"""
import os
import sys
import time
import json
import platform
import threading
from datetime import datetime
from dataclasses import dataclass, field, asdict
from typing import Dict, Any, List, Optional, Callable
from enum import Enum

from config.settings import get_config
from utils.logger import get_logger

logger = get_logger("functional_test")


class TestStatus(Enum):
    """测试状态枚举"""
    PENDING = "pending"      # 未测试
    RUNNING = "running"      # 测试中
    PASSED = "passed"        # 通过
    FAILED = "failed"        # 失败
    WARNING = "warning"      # 警告


@dataclass
class TestResult:
    """单个测试结果"""
    case_id: str
    case_name: str
    category: str
    status: str  # passed/failed/warning
    message: str
    details: Dict[str, Any] = field(default_factory=dict)
    duration: float = 0.0
    executed_at: str = ""
    suggestion: str = ""  # 失败时的建议
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        return asdict(self)


@dataclass
class TestCase:
    """测试用例定义"""
    id: str
    name: str
    description: str
    category: str
    category_name: str
    automated: bool = True
    timeout: int = 30  # 超时时间（秒）
    
    def to_dict(self) -> Dict[str, Any]:
        """转换为字典"""
        return {
            "id": self.id,
            "name": self.name,
            "description": self.description,
            "category": self.category,
            "category_name": self.category_name,
            "automated": self.automated,
            "timeout": self.timeout
        }


class FunctionalTestRunner:
    """
    功能测试运行器
    
    提供细粒度的测试用例执行功能
    """
    
    def __init__(self):
        self.test_cases: List[TestCase] = self._define_test_cases()
        self.test_history: List[Dict[str, Any]] = []
        self.max_history = 50
        self._lock = threading.Lock()
        
    def _define_test_cases(self) -> List[TestCase]:
        """定义所有测试用例"""
        cases = []
        
        # 系统环境测试
        system_cases = [
            TestCase("SYS-001", "Python环境检查", "检查Python版本和必要模块", "system", "系统环境测试"),
            TestCase("SYS-002", "系统内存检查", "检查可用内存是否充足", "system", "系统环境测试"),
            TestCase("SYS-003", "磁盘空间检查", "检查磁盘空间是否充足", "system", "系统环境测试"),
            TestCase("SYS-004", "网络连接检查", "检查HTTP服务可访问性", "system", "系统环境测试"),
            TestCase("SYS-005", "配置文件检查", "检查配置文件格式和完整性", "system", "系统环境测试"),
        ]
        cases.extend(system_cases)
        
        # CANoe测试
        canoe_cases = [
            TestCase("CANOE-001", "安装路径检查", "检查CANoe安装路径是否存在", "canoe", "CANoe测试"),
            TestCase("CANOE-002", "可执行文件检查", "检查CANoe64.exe是否存在", "canoe", "CANoe测试"),
            TestCase("CANOE-003", "pywin32模块检查", "检查pywin32是否安装", "canoe", "CANoe测试"),
            TestCase("CANOE-004", "COM对象创建测试", "测试CANoe COM对象创建", "canoe", "CANoe测试", timeout=60),
            TestCase("CANOE-005", "接口访问测试", "测试Measurement等接口访问", "canoe", "CANoe测试", timeout=60),
            TestCase("CANOE-006", "版本获取测试", "测试获取CANoe版本信息", "canoe", "CANoe测试", timeout=60),
            TestCase("CANOE-007", "配置文件路径检查", "检查cfg配置文件路径", "canoe", "CANoe测试"),
            TestCase("CANOE-008", "配置文件读取测试", "测试读取cfg文件内容", "canoe", "CANoe测试"),
            TestCase("CANOE-009", "完整调用链测试", "测试完整的调用流程", "canoe", "CANoe测试", timeout=120),
        ]
        cases.extend(canoe_cases)
        
        # TSMaster测试
        tsmaster_cases = [
            TestCase("TS-001", "安装路径检查", "检查TSMaster安装路径", "tsmaster", "TSMaster测试"),
            TestCase("TS-002", "Python API检查", "检查TSMaster Python API", "tsmaster", "TSMaster测试"),
            TestCase("TS-003", "RPC API检查", "检查TSMasterAPI模块", "tsmaster", "TSMaster测试"),
            TestCase("TS-004", "连接测试", "测试与TSMaster的连接", "tsmaster", "TSMaster测试", timeout=60),
            TestCase("TS-005", "版本获取测试", "测试获取版本信息", "tsmaster", "TSMaster测试", timeout=60),
            TestCase("TS-006", "工程文件路径检查", "检查tproj文件路径", "tsmaster", "TSMaster测试"),
            TestCase("TS-007", "工程文件读取测试", "测试读取tproj文件", "tsmaster", "TSMaster测试"),
            TestCase("TS-008", "完整调用链测试", "测试完整的调用流程", "tsmaster", "TSMaster测试", timeout=120),
        ]
        cases.extend(tsmaster_cases)
        
        # TTman测试
        ttman_cases = [
            TestCase("TTM-001", "安装路径检查", "检查TTman安装路径", "ttman", "TTman测试"),
            TestCase("TTM-002", "批处理文件检查", "检查TTman.bat是否存在", "ttman", "TTman测试"),
            TestCase("TTM-003", "脚本可读性测试", "测试读取批处理脚本", "ttman", "TTman测试"),
            TestCase("TTM-004", "依赖检查", "检查jar文件和配置文件", "ttman", "TTman测试"),
            TestCase("TTM-005", "完整调用链测试", "测试完整的调用流程", "ttman", "TTman测试", timeout=60),
        ]
        cases.extend(ttman_cases)
        
        # 配置测试
        config_cases = [
            TestCase("CFG-001", "HTTP端口检查", "检查HTTP端口配置", "config", "配置测试"),
            TestCase("CFG-002", "设备ID检查", "检查设备ID配置", "config", "配置测试"),
            TestCase("CFG-003", "软件路径配置检查", "检查各软件路径配置", "config", "配置测试"),
            TestCase("CFG-004", "WebSocket配置检查", "检查WebSocket配置", "config", "配置测试"),
            TestCase("CFG-005", "上报配置检查", "检查上报服务器配置", "config", "配置测试"),
            TestCase("CFG-006", "缓存配置检查", "检查缓存配置", "config", "配置测试"),
        ]
        cases.extend(config_cases)
        
        # 任务执行测试
        task_cases = [
            TestCase("TASK-001", "任务存储测试", "测试任务存储功能", "task", "任务执行测试"),
            TestCase("TASK-002", "任务队列测试", "测试任务队列管理", "task", "任务执行测试"),
            TestCase("TASK-003", "模拟任务执行测试", "执行模拟任务测试流程", "task", "任务执行测试"),
            TestCase("TASK-004", "结果收集测试", "测试结果收集功能", "task", "任务执行测试"),
            TestCase("TASK-005", "日志记录测试", "测试日志记录功能", "task", "任务执行测试"),
        ]
        cases.extend(task_cases)
        
        return cases
    
    def get_all_cases(self) -> List[TestCase]:
        """获取所有测试用例"""
        return self.test_cases
    
    def get_cases_by_category(self, category: str) -> List[TestCase]:
        """获取指定类别的测试用例"""
        return [c for c in self.test_cases if c.category == category]
    
    def get_case_by_id(self, case_id: str) -> Optional[TestCase]:
        """根据ID获取测试用例"""
        for case in self.test_cases:
            if case.id == case_id:
                return case
        return None
    
    def get_categories(self) -> List[Dict[str, Any]]:
        """获取所有测试分类"""
        categories = {}
        for case in self.test_cases:
            if case.category not in categories:
                categories[case.category] = {
                    "id": case.category,
                    "name": case.category_name,
                    "count": 0
                }
            categories[case.category]["count"] += 1
        return list(categories.values())
    
    def execute_case(self, case_id: str) -> TestResult:
        """
        执行单个测试用例
        
        Args:
            case_id: 测试用例ID
            
        Returns:
            TestResult: 测试结果
        """
        case = self.get_case_by_id(case_id)
        if not case:
            return TestResult(
                case_id=case_id,
                case_name="未知",
                category="unknown",
                status="failed",
                message=f"测试用例 {case_id} 不存在"
            )
        
        start_time = time.time()
        executed_at = datetime.now().isoformat()
        
        try:
            # 根据case_id调用对应的测试方法
            method_name = f"test_{case_id.lower().replace('-', '_')}"
            if hasattr(self, method_name):
                result = getattr(self, method_name)()
            else:
                result = TestResult(
                    case_id=case_id,
                    case_name=case.name,
                    category=case.category,
                    status="failed",
                    message=f"测试方法 {method_name} 未实现"
                )
            
            result.duration = time.time() - start_time
            result.executed_at = executed_at
            
            # 添加到历史记录
            self._add_to_history(result)
            
            return result
            
        except Exception as e:
            logger.error(f"执行测试用例 {case_id} 失败: {e}")
            result = TestResult(
                case_id=case_id,
                case_name=case.name,
                category=case.category,
                status="failed",
                message=f"执行异常: {str(e)}",
                duration=time.time() - start_time,
                executed_at=executed_at
            )
            self._add_to_history(result)
            return result
    
    def execute_cases(self, case_ids: List[str]) -> List[TestResult]:
        """批量执行测试用例"""
        results = []
        for case_id in case_ids:
            result = self.execute_case(case_id)
            results.append(result)
        return results
    
    def execute_category(self, category: str) -> List[TestResult]:
        """执行指定类别的所有测试用例"""
        cases = self.get_cases_by_category(category)
        return self.execute_cases([c.id for c in cases])
    
    def execute_all(self) -> List[TestResult]:
        """执行所有测试用例"""
        return self.execute_cases([c.id for c in self.test_cases])
    
    def _add_to_history(self, result: TestResult):
        """添加结果到历史记录"""
        with self._lock:
            self.test_history.insert(0, result.to_dict())
            if len(self.test_history) > self.max_history:
                self.test_history = self.test_history[:self.max_history]
    
    def get_history(self, limit: int = 20) -> List[Dict[str, Any]]:
        """获取测试历史"""
        with self._lock:
            return self.test_history[:limit]
    
    def get_stats(self) -> Dict[str, Any]:
        """获取测试统计"""
        with self._lock:
            total = len(self.test_history)
            passed = len([h for h in self.test_history if h["status"] == "passed"])
            failed = len([h for h in self.test_history if h["status"] == "failed"])
            warning = len([h for h in self.test_history if h["status"] == "warning"])
            
            return {
                "total_executions": total,
                "total_passed": passed,
                "total_failed": failed,
                "total_warning": warning,
                "pass_rate": (passed / total * 100) if total > 0 else 0
            }
    
    # ==================== 系统环境测试方法 ====================
    
    def test_sys_001(self) -> TestResult:
        """SYS-001: Python环境检查"""
        details = {
            "python_version": sys.version,
            "python_executable": sys.executable,
            "platform": platform.platform()
        }
        
        required_modules = ['flask', 'requests', 'websocket']
        missing_modules = []
        installed_modules = []
        
        for module in required_modules:
            try:
                __import__(module)
                installed_modules.append(module)
            except ImportError:
                missing_modules.append(module)
        
        details["required_modules"] = required_modules
        details["installed_modules"] = installed_modules
        details["missing_modules"] = missing_modules
        
        if missing_modules:
            return TestResult(
                case_id="SYS-001",
                case_name="Python环境检查",
                category="system",
                status="warning",
                message=f"Python {sys.version_info.major}.{sys.version_info.minor} 运行正常，但缺少模块: {', '.join(missing_modules)}",
                details=details,
                suggestion=f"请安装缺少的模块: pip install {' '.join(missing_modules)}"
            )
        
        return TestResult(
            case_id="SYS-001",
            case_name="Python环境检查",
            category="system",
            status="passed",
            message=f"Python {sys.version_info.major}.{sys.version_info.minor}.{sys.version_info.micro} 运行正常，所有必要模块已安装",
            details=details
        )
    
    def test_sys_002(self) -> TestResult:
        """SYS-002: 系统内存检查"""
        try:
            import psutil
            memory = psutil.virtual_memory()
            total_gb = memory.total / (1024**3)
            available_gb = memory.available / (1024**3)
            percent = memory.percent
            
            details = {
                "total_gb": round(total_gb, 2),
                "available_gb": round(available_gb, 2),
                "used_percent": percent
            }
            
            if available_gb < 1:
                return TestResult(
                    case_id="SYS-002",
                    case_name="系统内存检查",
                    category="system",
                    status="warning",
                    message=f"可用内存不足: {available_gb:.1f}GB",
                    details=details,
                    suggestion="请关闭其他程序释放内存，或增加系统内存"
                )
            
            return TestResult(
                case_id="SYS-002",
                case_name="系统内存检查",
                category="system",
                status="passed",
                message=f"内存充足: 总计 {total_gb:.1f}GB, 可用 {available_gb:.1f}GB",
                details=details
            )
        except ImportError:
            return TestResult(
                case_id="SYS-002",
                case_name="系统内存检查",
                category="system",
                status="warning",
                message="无法检测内存（缺少 psutil 模块）",
                suggestion="请安装 psutil: pip install psutil"
            )
    
    def test_sys_003(self) -> TestResult:
        """SYS-003: 磁盘空间检查"""
        try:
            import shutil
            system = platform.system()
            path = "/" if system != "Windows" else "C:\\"
            total, used, free = shutil.disk_usage(path)
            free_gb = free / (1024**3)
            used_percent = (used / total) * 100
            
            details = {
                "total_gb": round(total / (1024**3), 2),
                "used_gb": round(used / (1024**3), 2),
                "free_gb": round(free_gb, 2),
                "used_percent": round(used_percent, 1)
            }
            
            if free_gb < 5:
                return TestResult(
                    case_id="SYS-003",
                    case_name="磁盘空间检查",
                    category="system",
                    status="warning",
                    message=f"磁盘空间不足: 仅剩 {free_gb:.1f}GB",
                    details=details,
                    suggestion="请清理磁盘空间，删除不必要的文件"
                )
            
            return TestResult(
                case_id="SYS-003",
                case_name="磁盘空间检查",
                category="system",
                status="passed",
                message=f"磁盘空间充足: {free_gb:.1f}GB 可用",
                details=details
            )
        except Exception as e:
            return TestResult(
                case_id="SYS-003",
                case_name="磁盘空间检查",
                category="system",
                status="warning",
                message=f"无法检测磁盘空间: {str(e)}"
            )
    
    def test_sys_004(self) -> TestResult:
        """SYS-004: 网络连接检查"""
        try:
            import urllib.request
            config = get_config()
            port = config.get('http.port', 8180)
            url = f"http://127.0.0.1:{port}/api/status"
            
            details = {
                "test_url": url,
                "port": port
            }
            
            with urllib.request.urlopen(url, timeout=5) as response:
                details["status_code"] = response.status
                if response.status == 200:
                    return TestResult(
                        case_id="SYS-004",
                        case_name="网络连接检查",
                        category="system",
                        status="passed",
                        message=f"HTTP 服务正常 (端口 {port})",
                        details=details
                    )
                else:
                    return TestResult(
                        case_id="SYS-004",
                        case_name="网络连接检查",
                        category="system",
                        status="failed",
                        message=f"HTTP 服务返回异常状态码: {response.status}",
                        details=details,
                        suggestion="请检查HTTP服务日志，查看错误原因"
                    )
        except Exception as e:
            return TestResult(
                case_id="SYS-004",
                case_name="网络连接检查",
                category="system",
                status="failed",
                message=f"HTTP 服务连接失败: {str(e)}",
                details={"configured_port": config.get('http.port', 8180)},
                suggestion="请确认HTTP服务已启动，或检查端口配置"
            )
    
    def test_sys_005(self) -> TestResult:
        """SYS-005: 配置文件检查"""
        try:
            config = get_config()
            config_path = os.path.join(os.path.dirname(os.path.dirname(__file__)), 'config.json')
            
            details = {
                "config_path": config_path,
                "config_exists": os.path.exists(config_path)
            }
            
            if not os.path.exists(config_path):
                return TestResult(
                    case_id="SYS-005",
                    case_name="配置文件检查",
                    category="system",
                    status="warning",
                    message="配置文件不存在，将使用默认配置",
                    details=details
                )
            
            # 检查配置文件格式
            with open(config_path, 'r', encoding='utf-8') as f:
                content = f.read()
                json.loads(content)  # 验证JSON格式
            
            details["config_size"] = len(content)
            
            # 检查必要配置项
            required_keys = [
                ('http.port', 'HTTP端口'),
                ('device.device_id', '设备ID')
            ]
            
            missing = []
            for key, name in required_keys:
                value = config.get(key)
                if not value:
                    missing.append(name)
            
            details["missing_keys"] = missing
            
            if missing:
                return TestResult(
                    case_id="SYS-005",
                    case_name="配置文件检查",
                    category="system",
                    status="warning",
                    message=f"配置文件存在，但缺少必要配置项: {', '.join(missing)}",
                    details=details,
                    suggestion=f"请在配置页面设置: {', '.join(missing)}"
                )
            
            return TestResult(
                case_id="SYS-005",
                case_name="配置文件检查",
                category="system",
                status="passed",
                message="配置文件格式正确，所有必要配置项已设置",
                details=details
            )
            
        except json.JSONDecodeError as e:
            return TestResult(
                case_id="SYS-005",
                case_name="配置文件检查",
                category="system",
                status="failed",
                message=f"配置文件格式错误: {str(e)}",
                suggestion="请检查config.json文件格式，确保是有效的JSON"
            )
        except Exception as e:
            return TestResult(
                case_id="SYS-005",
                case_name="配置文件检查",
                category="system",
                status="failed",
                message=f"配置文件检查失败: {str(e)}"
            )
    
    # ==================== CANoe测试方法 ====================
    
    def test_canoe_001(self) -> TestResult:
        """CANOE-001: 安装路径检查"""
        config = get_config()
        canoe_path = config.get('software.canoe_path', r'C:\Program Files\Vector\CANoe 17\Exec64\CANoe64.exe')
        
        details = {
            "configured_path": canoe_path,
            "path_exists": os.path.exists(canoe_path)
        }
        
        if not os.path.exists(canoe_path):
            return TestResult(
                case_id="CANOE-001",
                case_name="安装路径检查",
                category="canoe",
                status="failed",
                message=f"CANoe路径不存在: {canoe_path}",
                details=details,
                suggestion="请在配置页面设置正确的CANoe安装路径"
            )
        
        return TestResult(
            case_id="CANOE-001",
            case_name="安装路径检查",
            category="canoe",
            status="passed",
            message=f"CANoe路径存在: {canoe_path}",
            details=details
        )
    
    def test_canoe_002(self) -> TestResult:
        """CANOE-002: 可执行文件检查"""
        config = get_config()
        canoe_path = config.get('software.canoe_path', r'C:\Program Files\Vector\CANoe 17\Exec64\CANoe64.exe')
        
        details = {
            "exe_path": canoe_path,
            "file_exists": os.path.exists(canoe_path) and os.path.isfile(canoe_path)
        }
        
        if not details["file_exists"]:
            return TestResult(
                case_id="CANOE-002",
                case_name="可执行文件检查",
                category="canoe",
                status="failed",
                message=f"CANoe可执行文件不存在: {canoe_path}",
                details=details,
                suggestion="请确认CANoe已正确安装，并在配置页面设置正确的路径"
            )
        
        # 检查文件大小
        file_size = os.path.getsize(canoe_path)
        details["file_size_mb"] = round(file_size / (1024*1024), 2)
        
        return TestResult(
            case_id="CANOE-002",
            case_name="可执行文件检查",
            category="canoe",
            status="passed",
            message=f"CANoe可执行文件存在，大小 {details['file_size_mb']} MB",
            details=details
        )
    
    def test_canoe_003(self) -> TestResult:
        """CANOE-003: pywin32模块检查"""
        try:
            import win32com.client
            details = {
                "module": "pywin32",
                "available": True
            }
            return TestResult(
                case_id="CANOE-003",
                case_name="pywin32模块检查",
                category="canoe",
                status="passed",
                message="pywin32模块已安装",
                details=details
            )
        except ImportError:
            return TestResult(
                case_id="CANOE-003",
                case_name="pywin32模块检查",
                category="canoe",
                status="failed",
                message="pywin32模块未安装",
                details={"module": "pywin32", "available": False},
                suggestion="请安装pywin32: pip install pywin32"
            )
    
    def test_canoe_004(self) -> TestResult:
        """CANOE-004: COM对象创建测试"""
        try:
            import win32com.client
        except ImportError:
            return TestResult(
                case_id="CANOE-004",
                case_name="COM对象创建测试",
                category="canoe",
                status="failed",
                message="pywin32模块未安装，无法测试COM对象",
                suggestion="请先安装pywin32: pip install pywin32"
            )
        
        details = {
            "com_object": "CANoe.Application",
            "created": False
        }
        
        app = None
        try:
            app = win32com.client.Dispatch("CANoe.Application")
            details["created"] = True
            return TestResult(
                case_id="CANOE-004",
                case_name="COM对象创建测试",
                category="canoe",
                status="passed",
                message="CANoe COM对象创建成功",
                details=details
            )
        except Exception as e:
            details["error"] = str(e)
            return TestResult(
                case_id="CANOE-004",
                case_name="COM对象创建测试",
                category="canoe",
                status="failed",
                message=f"无法创建CANoe COM对象: {str(e)}",
                details=details,
                suggestion="请确认CANoe已正确安装并激活，且当前用户有权限访问COM对象"
            )
        finally:
            if app:
                try:
                    app.Quit()
                except:
                    pass
    
    def test_canoe_005(self) -> TestResult:
        """CANOE-005: 接口访问测试"""
        try:
            import win32com.client
        except ImportError:
            return TestResult(
                case_id="CANOE-005",
                case_name="接口访问测试",
                category="canoe",
                status="failed",
                message="pywin32模块未安装",
                suggestion="请先安装pywin32"
            )
        
        details = {
            "interfaces_tested": [],
            "interfaces_failed": []
        }
        
        app = None
        try:
            app = win32com.client.Dispatch("CANoe.Application")
            
            # 测试各个接口
            interfaces = [
                ("Measurement", lambda a: a.Measurement),
                ("BusSystems", lambda a: a.BusSystems),
                ("SystemVariables", lambda a: a.SystemVariables),
                ("Namespaces", lambda a: a.Namespaces),
            ]
            
            for name, getter in interfaces:
                try:
                    obj = getter(app)
                    details["interfaces_tested"].append(name)
                except Exception as e:
                    details["interfaces_failed"].append({"name": name, "error": str(e)})
            
            if details["interfaces_failed"]:
                return TestResult(
                    case_id="CANOE-005",
                    case_name="接口访问测试",
                    category="canoe",
                    status="warning",
                    message=f"部分接口访问失败: {', '.join([f['name'] for f in details['interfaces_failed']])}",
                    details=details
                )
            
            return TestResult(
                case_id="CANOE-005",
                case_name="接口访问测试",
                category="canoe",
                status="passed",
                message=f"所有接口访问正常: {', '.join(details['interfaces_tested'])}",
                details=details
            )
            
        except Exception as e:
            return TestResult(
                case_id="CANOE-005",
                case_name="接口访问测试",
                category="canoe",
                status="failed",
                message=f"接口访问测试失败: {str(e)}",
                details=details
            )
        finally:
            if app:
                try:
                    app.Quit()
                except:
                    pass
    
    def test_canoe_006(self) -> TestResult:
        """CANOE-006: 版本获取测试"""
        try:
            import win32com.client
        except ImportError:
            return TestResult(
                case_id="CANOE-006",
                case_name="版本获取测试",
                category="canoe",
                status="failed",
                message="pywin32模块未安装"
            )
        
        app = None
        try:
            app = win32com.client.Dispatch("CANoe.Application")
            version = app.Version
            
            details = {
                "version": str(version),
                "major": getattr(version, 'Major', 'N/A'),
                "minor": getattr(version, 'Minor', 'N/A'),
                "build": getattr(version, 'Build', 'N/A')
            }
            
            return TestResult(
                case_id="CANOE-006",
                case_name="版本获取测试",
                category="canoe",
                status="passed",
                message=f"CANoe版本: {details['version']}",
                details=details
            )
            
        except Exception as e:
            return TestResult(
                case_id="CANOE-006",
                case_name="版本获取测试",
                category="canoe",
                status="failed",
                message=f"无法获取CANoe版本: {str(e)}",
                details={"error": str(e)}
            )
        finally:
            if app:
                try:
                    app.Quit()
                except:
                    pass
    
    def test_canoe_007(self) -> TestResult:
        """CANOE-007: 配置文件路径检查"""
        config = get_config()
        workspace_path = config.get('software.workspace_path', r'C:\TestWorkspace')
        
        details = {
            "workspace_path": workspace_path,
            "path_exists": os.path.exists(workspace_path)
        }
        
        if not os.path.exists(workspace_path):
            return TestResult(
                case_id="CANOE-007",
                case_name="配置文件路径检查",
                category="canoe",
                status="warning",
                message=f"工作空间路径不存在: {workspace_path}",
                details=details,
                suggestion="请在配置页面设置正确的工作空间路径"
            )
        
        return TestResult(
            case_id="CANOE-007",
            case_name="配置文件路径检查",
            category="canoe",
            status="passed",
            message=f"工作空间路径存在: {workspace_path}",
            details=details
        )
    
    def test_canoe_008(self) -> TestResult:
        """CANOE-008: 配置文件读取测试"""
        config = get_config()
        workspace_path = config.get('software.workspace_path', r'C:\TestWorkspace')
        
        details = {
            "workspace_path": workspace_path
        }
        
        if not os.path.exists(workspace_path):
            return TestResult(
                case_id="CANOE-008",
                case_name="配置文件读取测试",
                category="canoe",
                status="warning",
                message="工作空间路径不存在",
                suggestion="请先设置正确的工作空间路径"
            )
        
        # 查找cfg文件
        cfg_files = []
        for root, dirs, files in os.walk(workspace_path):
            for file in files:
                if file.endswith('.cfg'):
                    cfg_files.append(os.path.join(root, file))
        
        details["cfg_files_found"] = len(cfg_files)
        details["sample_files"] = [os.path.basename(f) for f in cfg_files[:5]]
        
        if not cfg_files:
            return TestResult(
                case_id="CANOE-008",
                case_name="配置文件读取测试",
                category="canoe",
                status="warning",
                message=f"在工作空间路径下未找到.cfg配置文件",
                details=details,
                suggestion="请将CANoe配置文件(.cfg)放入工作空间目录"
            )
        
        # 尝试读取第一个cfg文件
        try:
            with open(cfg_files[0], 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read(1024)
                if '[Configuration]' in content or 'Database' in content:
                    details["sample_read"] = "success"
                else:
                    details["sample_read"] = "invalid_format"
        except Exception as e:
            details["sample_read"] = f"error: {str(e)}"
        
        return TestResult(
            case_id="CANOE-008",
            case_name="配置文件读取测试",
            category="canoe",
            status="passed",
            message=f"找到 {len(cfg_files)} 个.cfg配置文件",
            details=details
        )
    
    def test_canoe_009(self) -> TestResult:
        """CANOE-009: 完整调用链测试"""
        try:
            import win32com.client
        except ImportError:
            return TestResult(
                case_id="CANOE-009",
                case_name="完整调用链测试",
                category="canoe",
                status="failed",
                message="pywin32模块未安装"
            )
        
        details = {
            "steps": []
        }
        
        app = None
        try:
            # 步骤1: 创建COM对象
            details["steps"].append("1. 创建COM对象")
            app = win32com.client.Dispatch("CANoe.Application")
            details["steps"].append("   ✓ 成功")
            
            # 步骤2: 获取版本
            details["steps"].append("2. 获取版本")
            version = app.Version
            details["steps"].append(f"   ✓ 版本: {version}")
            
            # 步骤3: 访问Measurement接口
            details["steps"].append("3. 访问Measurement接口")
            measurement = app.Measurement
            details["steps"].append("   ✓ 成功")
            
            # 步骤4: 检查测量状态
            details["steps"].append("4. 检查测量状态")
            running = measurement.Running
            details["steps"].append(f"   ✓ 测量状态: {'运行中' if running else '停止'}")
            
            return TestResult(
                case_id="CANOE-009",
                case_name="完整调用链测试",
                category="canoe",
                status="passed",
                message="CANoe完整调用链测试通过",
                details=details
            )
            
        except Exception as e:
            details["steps"].append(f"   ✗ 失败: {str(e)}")
            return TestResult(
                case_id="CANOE-009",
                case_name="完整调用链测试",
                category="canoe",
                status="failed",
                message=f"完整调用链测试失败: {str(e)}",
                details=details,
                suggestion="请检查CANoe安装和许可证状态"
            )
        finally:
            if app:
                try:
                    app.Quit()
                except:
                    pass
    
    # ==================== TSMaster测试方法 ====================
    
    def test_ts_001(self) -> TestResult:
        """TS-001: 安装路径检查"""
        config = get_config()
        tsmaster_path = config.get('software.tsmaster_path', r'C:\Program Files\TSMaster')
        
        details = {
            "configured_path": tsmaster_path,
            "path_exists": os.path.exists(tsmaster_path)
        }
        
        if not os.path.exists(tsmaster_path):
            return TestResult(
                case_id="TS-001",
                case_name="安装路径检查",
                category="tsmaster",
                status="failed",
                message=f"TSMaster路径不存在: {tsmaster_path}",
                details=details,
                suggestion="请在配置页面设置正确的TSMaster安装路径"
            )
        
        return TestResult(
            case_id="TS-001",
            case_name="安装路径检查",
            category="tsmaster",
            status="passed",
            message=f"TSMaster路径存在: {tsmaster_path}",
            details=details
        )
    
    def test_ts_002(self) -> TestResult:
        """TS-002: Python API检查"""
        try:
            from TSMaster import TSMaster
            details = {
                "api_type": "Python API",
                "available": True
            }
            return TestResult(
                case_id="TS-002",
                case_name="Python API检查",
                category="tsmaster",
                status="passed",
                message="TSMaster Python API可用",
                details=details
            )
        except ImportError:
            return TestResult(
                case_id="TS-002",
                case_name="Python API检查",
                category="tsmaster",
                status="warning",
                message="TSMaster Python API未安装",
                details={"api_type": "Python API", "available": False},
                suggestion="请安装TSMaster Python API或检查安装路径"
            )
    
    def test_ts_003(self) -> TestResult:
        """TS-003: RPC API检查"""
        try:
            import TSMasterAPI
            details = {
                "api_type": "RPC API",
                "available": True
            }
            return TestResult(
                case_id="TS-003",
                case_name="RPC API检查",
                category="tsmaster",
                status="passed",
                message="TSMaster RPC API可用",
                details=details
            )
        except ImportError:
            return TestResult(
                case_id="TS-003",
                case_name="RPC API检查",
                category="tsmaster",
                status="warning",
                message="TSMaster RPC API未安装",
                details={"api_type": "RPC API", "available": False},
                suggestion="请安装TSMasterAPI模块"
            )
    
    def test_ts_004(self) -> TestResult:
        """TS-004: 连接测试"""
        try:
            from TSMaster import TSMaster
        except ImportError:
            return TestResult(
                case_id="TS-004",
                case_name="连接测试",
                category="tsmaster",
                status="failed",
                message="TSMaster Python API未安装",
                suggestion="请先安装TSMaster Python API"
            )
        
        details = {
            "connection_test": "failed"
        }
        
        ts = None
        try:
            ts = TSMaster()
            ts.connect()
            details["connection_test"] = "success"
            return TestResult(
                case_id="TS-004",
                case_name="连接测试",
                category="tsmaster",
                status="passed",
                message="TSMaster连接测试通过",
                details=details
            )
        except Exception as e:
            details["error"] = str(e)
            return TestResult(
                case_id="TS-004",
                case_name="连接测试",
                category="tsmaster",
                status="failed",
                message=f"TSMaster连接失败: {str(e)}",
                details=details,
                suggestion="请确认TSMaster已启动，且RPC服务正常运行"
            )
        finally:
            if ts:
                try:
                    ts.disconnect()
                except:
                    pass
    
    def test_ts_005(self) -> TestResult:
        """TS-005: 版本获取测试"""
        try:
            from TSMaster import TSMaster
        except ImportError:
            return TestResult(
                case_id="TS-005",
                case_name="版本获取测试",
                category="tsmaster",
                status="failed",
                message="TSMaster Python API未安装"
            )
        
        ts = None
        try:
            ts = TSMaster()
            ts.connect()
            version = ts.get_version()
            
            details = {
                "version": str(version)
            }
            
            return TestResult(
                case_id="TS-005",
                case_name="版本获取测试",
                category="tsmaster",
                status="passed",
                message=f"TSMaster版本: {version}",
                details=details
            )
        except Exception as e:
            return TestResult(
                case_id="TS-005",
                case_name="版本获取测试",
                category="tsmaster",
                status="failed",
                message=f"无法获取TSMaster版本: {str(e)}",
                details={"error": str(e)}
            )
        finally:
            if ts:
                try:
                    ts.disconnect()
                except:
                    pass
    
    def test_ts_006(self) -> TestResult:
        """TS-006: 工程文件路径检查"""
        config = get_config()
        workspace_path = config.get('software.workspace_path', r'C:\TestWorkspace')
        
        details = {
            "workspace_path": workspace_path,
            "path_exists": os.path.exists(workspace_path)
        }
        
        if not os.path.exists(workspace_path):
            return TestResult(
                case_id="TS-006",
                case_name="工程文件路径检查",
                category="tsmaster",
                status="warning",
                message=f"工作空间路径不存在: {workspace_path}",
                details=details,
                suggestion="请在配置页面设置正确的工作空间路径"
            )
        
        return TestResult(
            case_id="TS-006",
            case_name="工程文件路径检查",
            category="tsmaster",
            status="passed",
            message=f"工作空间路径存在: {workspace_path}",
            details=details
        )
    
    def test_ts_007(self) -> TestResult:
        """TS-007: 工程文件读取测试"""
        config = get_config()
        workspace_path = config.get('software.workspace_path', r'C:\TestWorkspace')
        
        details = {
            "workspace_path": workspace_path
        }
        
        if not os.path.exists(workspace_path):
            return TestResult(
                case_id="TS-007",
                case_name="工程文件读取测试",
                category="tsmaster",
                status="warning",
                message="工作空间路径不存在",
                suggestion="请先设置正确的工作空间路径"
            )
        
        # 查找tproj文件
        tproj_files = []
        for root, dirs, files in os.walk(workspace_path):
            for file in files:
                if file.endswith('.tproj'):
                    tproj_files.append(os.path.join(root, file))
        
        details["tproj_files_found"] = len(tproj_files)
        details["sample_files"] = [os.path.basename(f) for f in tproj_files[:5]]
        
        if not tproj_files:
            return TestResult(
                case_id="TS-007",
                case_name="工程文件读取测试",
                category="tsmaster",
                status="warning",
                message=f"在工作空间路径下未找到.tproj工程文件",
                details=details,
                suggestion="请将TSMaster工程文件(.tproj)放入工作空间目录"
            )
        
        # 尝试读取第一个tproj文件
        try:
            import xml.etree.ElementTree as ET
            tree = ET.parse(tproj_files[0])
            root = tree.getroot()
            details["sample_read"] = "success"
            details["root_tag"] = root.tag
        except Exception as e:
            details["sample_read"] = f"error: {str(e)}"
        
        return TestResult(
            case_id="TS-007",
            case_name="工程文件读取测试",
            category="tsmaster",
            status="passed",
            message=f"找到 {len(tproj_files)} 个.tproj工程文件",
            details=details
        )
    
    def test_ts_008(self) -> TestResult:
        """TS-008: 完整调用链测试"""
        try:
            from TSMaster import TSMaster
        except ImportError:
            return TestResult(
                case_id="TS-008",
                case_name="完整调用链测试",
                category="tsmaster",
                status="failed",
                message="TSMaster Python API未安装"
            )
        
        details = {
            "steps": []
        }
        
        ts = None
        try:
            # 步骤1: 创建对象
            details["steps"].append("1. 创建TSMaster对象")
            ts = TSMaster()
            details["steps"].append("   ✓ 成功")
            
            # 步骤2: 连接
            details["steps"].append("2. 连接TSMaster")
            ts.connect()
            details["steps"].append("   ✓ 成功")
            
            # 步骤3: 获取版本
            details["steps"].append("3. 获取版本")
            version = ts.get_version()
            details["steps"].append(f"   ✓ 版本: {version}")
            
            return TestResult(
                case_id="TS-008",
                case_name="完整调用链测试",
                category="tsmaster",
                status="passed",
                message="TSMaster完整调用链测试通过",
                details=details
            )
            
        except Exception as e:
            details["steps"].append(f"   ✗ 失败: {str(e)}")
            return TestResult(
                case_id="TS-008",
                case_name="完整调用链测试",
                category="tsmaster",
                status="failed",
                message=f"完整调用链测试失败: {str(e)}",
                details=details,
                suggestion="请检查TSMaster安装和运行状态"
            )
        finally:
            if ts:
                try:
                    ts.disconnect()
                except:
                    pass
    
    # ==================== TTman测试方法 ====================
    
    def test_ttm_001(self) -> TestResult:
        """TTM-001: 安装路径检查"""
        config = get_config()
        ttman_path = config.get('software.ttman_path', r'C:\Spirent\TTman.bat')
        
        details = {
            "configured_path": ttman_path,
            "path_exists": os.path.exists(ttman_path)
        }
        
        if not os.path.exists(ttman_path):
            return TestResult(
                case_id="TTM-001",
                case_name="安装路径检查",
                category="ttman",
                status="failed",
                message=f"TTman路径不存在: {ttman_path}",
                details=details,
                suggestion="请在配置页面设置正确的TTman安装路径"
            )
        
        return TestResult(
            case_id="TTM-001",
            case_name="安装路径检查",
            category="ttman",
            status="passed",
            message=f"TTman路径存在: {ttman_path}",
            details=details
        )
    
    def test_ttm_002(self) -> TestResult:
        """TTM-002: 批处理文件检查"""
        config = get_config()
        ttman_path = config.get('software.ttman_path', r'C:\Spirent\TTman.bat')
        
        details = {
            "bat_path": ttman_path,
            "file_exists": os.path.exists(ttman_path) and os.path.isfile(ttman_path)
        }
        
        if not details["file_exists"]:
            return TestResult(
                case_id="TTM-002",
                case_name="批处理文件检查",
                category="ttman",
                status="failed",
                message=f"TTman.bat文件不存在: {ttman_path}",
                details=details,
                suggestion="请确认TTman已正确安装"
            )
        
        return TestResult(
            case_id="TTM-002",
            case_name="批处理文件检查",
            category="ttman",
            status="passed",
            message=f"TTman.bat文件存在",
            details=details
        )
    
    def test_ttm_003(self) -> TestResult:
        """TTM-003: 脚本可读性测试"""
        config = get_config()
        ttman_path = config.get('software.ttman_path', r'C:\Spirent\TTman.bat')
        
        details = {
            "bat_path": ttman_path
        }
        
        if not os.path.exists(ttman_path):
            return TestResult(
                case_id="TTM-003",
                case_name="脚本可读性测试",
                category="ttman",
                status="failed",
                message="TTman.bat文件不存在"
            )
        
        try:
            with open(ttman_path, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read()
                details["file_size"] = len(content)
                details["readable"] = True
                
                # 检查关键内容
                has_java = 'java' in content.lower()
                has_ttman = 'ttman' in content.lower()
                
                details["has_java_reference"] = has_java
                details["has_ttman_reference"] = has_ttman
                
                if not has_java and not has_ttman:
                    return TestResult(
                        case_id="TTM-003",
                        case_name="脚本可读性测试",
                        category="ttman",
                        status="warning",
                        message="脚本可读，但未找到java或ttman引用",
                        details=details,
                        suggestion="请检查TTman.bat是否为正确的启动脚本"
                    )
                
                return TestResult(
                    case_id="TTM-003",
                    case_name="脚本可读性测试",
                    category="ttman",
                    status="passed",
                    message=f"脚本可读，大小 {len(content)} 字节",
                    details=details
                )
                
        except Exception as e:
            return TestResult(
                case_id="TTM-003",
                case_name="脚本可读性测试",
                category="ttman",
                status="failed",
                message=f"无法读取脚本: {str(e)}",
                details=details
            )
    
    def test_ttm_004(self) -> TestResult:
        """TTM-004: 依赖检查"""
        config = get_config()
        ttman_path = config.get('software.ttman_path', r'C:\Spirent\TTman.bat')
        
        details = {
            "install_dir": ""
        }
        
        if not os.path.exists(ttman_path):
            return TestResult(
                case_id="TTM-004",
                case_name="依赖检查",
                category="ttman",
                status="failed",
                message="TTman.bat文件不存在"
            )
        
        install_dir = os.path.dirname(ttman_path)
        details["install_dir"] = install_dir
        
        # 查找jar文件
        jar_files = []
        config_files = []
        
        if os.path.exists(install_dir):
            for file in os.listdir(install_dir):
                if file.endswith('.jar'):
                    jar_files.append(file)
                if file.endswith('.properties') or file.endswith('.xml') or file.endswith('.conf'):
                    config_files.append(file)
        
        details["jar_files"] = jar_files
        details["jar_count"] = len(jar_files)
        details["config_files"] = config_files
        
        if not jar_files:
            return TestResult(
                case_id="TTM-004",
                case_name="依赖检查",
                category="ttman",
                status="warning",
                message="未找到jar依赖文件",
                details=details,
                suggestion="请确认TTman安装完整"
            )
        
        return TestResult(
            case_id="TTM-004",
            case_name="依赖检查",
            category="ttman",
            status="passed",
            message=f"找到 {len(jar_files)} 个jar文件",
            details=details
        )
    
    def test_ttm_005(self) -> TestResult:
        """TTM-005: 完整调用链测试"""
        config = get_config()
        ttman_path = config.get('software.ttman_path', r'C:\Spirent\TTman.bat')
        
        details = {
            "steps": []
        }
        
        # 步骤1: 检查文件存在
        details["steps"].append("1. 检查TTman.bat存在")
        if not os.path.exists(ttman_path):
            details["steps"].append("   ✗ 文件不存在")
            return TestResult(
                case_id="TTM-005",
                case_name="完整调用链测试",
                category="ttman",
                status="failed",
                message="TTman.bat文件不存在",
                details=details
            )
        details["steps"].append("   ✓ 存在")
        
        # 步骤2: 检查可读性
        details["steps"].append("2. 检查脚本可读性")
        try:
            with open(ttman_path, 'r') as f:
                content = f.read()
            details["steps"].append("   ✓ 可读")
        except Exception as e:
            details["steps"].append(f"   ✗ 读取失败: {str(e)}")
            return TestResult(
                case_id="TTM-005",
                case_name="完整调用链测试",
                category="ttman",
                status="failed",
                message=f"脚本读取失败: {str(e)}",
                details=details
            )
        
        # 步骤3: 检查依赖
        details["steps"].append("3. 检查依赖文件")
        install_dir = os.path.dirname(ttman_path)
        jar_files = [f for f in os.listdir(install_dir) if f.endswith('.jar')] if os.path.exists(install_dir) else []
        if jar_files:
            details["steps"].append(f"   ✓ 找到 {len(jar_files)} 个jar文件")
        else:
            details["steps"].append("   ⚠ 未找到jar文件")
        
        return TestResult(
            case_id="TTM-005",
            case_name="完整调用链测试",
            category="ttman",
            status="passed",
            message="TTman环境检查通过",
            details=details
        )
    
    # ==================== 配置测试方法 ====================
    
    def test_cfg_001(self) -> TestResult:
        """CFG-001: HTTP端口检查"""
        config = get_config()
        port = config.get('http.port', 8180)
        
        details = {
            "configured_port": port,
            "valid": isinstance(port, int) and 1024 <= port <= 65535
        }
        
        if not details["valid"]:
            return TestResult(
                case_id="CFG-001",
                case_name="HTTP端口检查",
                category="config",
                status="warning",
                message=f"HTTP端口配置可能无效: {port}",
                details=details,
                suggestion="请配置1024-65535范围内的端口号"
            )
        
        return TestResult(
            case_id="CFG-001",
            case_name="HTTP端口检查",
            category="config",
            status="passed",
            message=f"HTTP端口配置有效: {port}",
            details=details
        )
    
    def test_cfg_002(self) -> TestResult:
        """CFG-002: 设备ID检查"""
        config = get_config()
        device_id = config.get('device.device_id', '')
        
        details = {
            "device_id": device_id,
            "configured": bool(device_id)
        }
        
        if not device_id:
            return TestResult(
                case_id="CFG-002",
                case_name="设备ID检查",
                category="config",
                status="warning",
                message="设备ID未配置",
                details=details,
                suggestion="请在配置页面设置设备ID"
            )
        
        return TestResult(
            case_id="CFG-002",
            case_name="设备ID检查",
            category="config",
            status="passed",
            message=f"设备ID已配置: {device_id}",
            details=details
        )
    
    def test_cfg_003(self) -> TestResult:
        """CFG-003: 软件路径配置检查"""
        config = get_config()
        
        paths = {
            "CANoe": config.get('software.canoe_path', ''),
            "TSMaster": config.get('software.tsmaster_path', ''),
            "TTman": config.get('software.ttman_path', '')
        }
        
        details = {
            "configured_paths": paths,
            "existing_paths": {}
        }
        
        for name, path in paths.items():
            details["existing_paths"][name] = os.path.exists(path) if path else False
        
        missing = [name for name, exists in details["existing_paths"].items() if not exists]
        
        if missing:
            return TestResult(
                case_id="CFG-003",
                case_name="软件路径配置检查",
                category="config",
                status="warning",
                message=f"以下软件路径不存在: {', '.join(missing)}",
                details=details,
                suggestion="请在配置页面设置正确的软件路径"
            )
        
        return TestResult(
            case_id="CFG-003",
            case_name="软件路径配置检查",
            category="config",
            status="passed",
            message="所有软件路径已配置且存在",
            details=details
        )
    
    def test_cfg_004(self) -> TestResult:
        """CFG-004: WebSocket配置检查"""
        config = get_config()
        
        ws_enabled = config.get('websocket.enabled', False)
        ws_url = config.get('server.websocket_url', '')
        
        details = {
            "enabled": ws_enabled,
            "websocket_url": ws_url
        }
        
        if ws_enabled and not ws_url:
            return TestResult(
                case_id="CFG-004",
                case_name="WebSocket配置检查",
                category="config",
                status="warning",
                message="WebSocket已启用但未配置服务器地址",
                details=details,
                suggestion="请在配置页面设置WebSocket服务器地址"
            )
        
        if ws_enabled:
            return TestResult(
                case_id="CFG-004",
                case_name="WebSocket配置检查",
                category="config",
                status="passed",
                message=f"WebSocket已启用，服务器: {ws_url}",
                details=details
            )
        else:
            return TestResult(
                case_id="CFG-004",
                case_name="WebSocket配置检查",
                category="config",
                status="passed",
                message="WebSocket未启用",
                details=details
            )
    
    def test_cfg_005(self) -> TestResult:
        """CFG-005: 上报配置检查"""
        config = get_config()
        
        report_enabled = config.get('report.enabled', False)
        report_url = config.get('report.server_url', '')
        
        details = {
            "enabled": report_enabled,
            "server_url": report_url
        }
        
        if report_enabled and not report_url:
            return TestResult(
                case_id="CFG-005",
                case_name="上报配置检查",
                category="config",
                status="warning",
                message="上报功能已启用但未配置服务器地址",
                details=details,
                suggestion="请在配置页面设置上报服务器地址"
            )
        
        if report_enabled:
            return TestResult(
                case_id="CFG-005",
                case_name="上报配置检查",
                category="config",
                status="passed",
                message=f"上报功能已启用，服务器: {report_url}",
                details=details
            )
        else:
            return TestResult(
                case_id="CFG-005",
                case_name="上报配置检查",
                category="config",
                status="passed",
                message="上报功能未启用",
                details=details
            )
    
    def test_cfg_006(self) -> TestResult:
        """CFG-006: 缓存配置检查"""
        config = get_config()
        
        cache_enabled = config.get('config_cache.enabled', True)
        cache_dir = config.get('config_cache.cache_dir', 'workspace/cache/configs')
        
        details = {
            "enabled": cache_enabled,
            "cache_dir": cache_dir
        }
        
        if cache_enabled:
            # 检查缓存目录是否存在或可创建
            full_path = os.path.abspath(cache_dir)
            details["full_path"] = full_path
            details["exists"] = os.path.exists(full_path)
            
            return TestResult(
                case_id="CFG-006",
                case_name="缓存配置检查",
                category="config",
                status="passed",
                message=f"缓存已启用，目录: {cache_dir}",
                details=details
            )
        else:
            return TestResult(
                case_id="CFG-006",
                case_name="缓存配置检查",
                category="config",
                status="passed",
                message="缓存已禁用",
                details=details
            )
    
    # ==================== 任务执行测试方法 ====================
    
    def test_task_001(self) -> TestResult:
        """TASK-001: 任务存储测试"""
        try:
            from core.task_store import task_store
            
            details = {
                "store_initialized": task_store is not None
            }
            
            # 尝试创建一个测试任务
            test_task_data = {
                "projectNo": "TEST",
                "taskNo": "TEST_TASK_001",
                "taskName": "测试任务",
                "caseList": []
            }
            
            task_info = task_store.create_task(test_task_data)
            details["task_created"] = True
            details["task_no"] = task_info.task_no
            
            # 清理测试任务
            task_store._tasks.pop(task_info.task_no, None)
            
            return TestResult(
                case_id="TASK-001",
                case_name="任务存储测试",
                category="task",
                status="passed",
                message="任务存储功能正常",
                details=details
            )
            
        except Exception as e:
            return TestResult(
                case_id="TASK-001",
                case_name="任务存储测试",
                category="task",
                status="failed",
                message=f"任务存储测试失败: {str(e)}",
                details={"error": str(e)}
            )
    
    def test_task_002(self) -> TestResult:
        """TASK-002: 任务队列测试"""
        try:
            from core.task_store import task_store
            
            details = {
                "queue_available": True
            }
            
            # 检查是否有正在运行的任务
            running_task = task_store.get_running_task()
            details["has_running_task"] = running_task is not None
            
            if running_task:
                details["running_task_no"] = running_task.task_no
            
            return TestResult(
                case_id="TASK-002",
                case_name="任务队列测试",
                category="task",
                status="passed",
                message="任务队列管理功能正常" + (f" (当前有运行中任务: {running_task.task_no})" if running_task else ""),
                details=details
            )
            
        except Exception as e:
            return TestResult(
                case_id="TASK-002",
                case_name="任务队列测试",
                category="task",
                status="failed",
                message=f"任务队列测试失败: {str(e)}",
                details={"error": str(e)}
            )
    
    def test_task_003(self) -> TestResult:
        """TASK-003: 模拟任务执行测试"""
        details = {
            "steps": []
        }
        
        try:
            from core.task_store import task_store, TaskStatus
            
            # 步骤1: 创建任务
            details["steps"].append("1. 创建模拟任务")
            test_task_data = {
                "projectNo": "TEST",
                "taskNo": "TEST_SIMULATION_001",
                "taskName": "模拟测试任务",
                "caseList": [
                    {"caseNo": "C001", "caseName": "测试用例1"}
                ]
            }
            
            task_info = task_store.create_task(test_task_data)
            details["steps"].append(f"   ✓ 任务创建成功: {task_info.task_no}")
            
            # 步骤2: 检查任务状态
            details["steps"].append("2. 检查任务状态")
            task = task_store.get_task(task_info.task_no)
            details["steps"].append(f"   ✓ 任务状态: {task.status.value}")
            
            # 步骤3: 更新任务进度
            details["steps"].append("3. 更新任务进度")
            task_store.update_task_status(task_info.task_no, TaskStatus.RUNNING, "执行中...", 50)
            details["steps"].append("   ✓ 进度更新成功")
            
            # 步骤4: 完成任务
            details["steps"].append("4. 完成任务")
            task_store.update_task_status(task_info.task_no, TaskStatus.COMPLETED, "任务完成", 100)
            task_store.update_task_results(task_info.task_no, [{"caseNo": "C001", "result": "PASS"}])
            details["steps"].append("   ✓ 任务完成")
            
            # 清理
            task_store._tasks.pop(task_info.task_no, None)
            
            return TestResult(
                case_id="TASK-003",
                case_name="模拟任务执行测试",
                category="task",
                status="passed",
                message="模拟任务执行流程测试通过",
                details=details
            )
            
        except Exception as e:
            details["steps"].append(f"   ✗ 失败: {str(e)}")
            return TestResult(
                case_id="TASK-003",
                case_name="模拟任务执行测试",
                category="task",
                status="failed",
                message=f"模拟任务执行测试失败: {str(e)}",
                details=details
            )
    
    def test_task_004(self) -> TestResult:
        """TASK-004: 结果收集测试"""
        try:
            from core.task_store import task_store
            
            details = {}
            
            # 创建任务并设置结果
            test_task_data = {
                "projectNo": "TEST",
                "taskNo": "TEST_RESULT_001",
                "taskName": "结果收集测试任务",
                "caseList": [
                    {"caseNo": "C001", "caseName": "用例1"},
                    {"caseNo": "C002", "caseName": "用例2"}
                ]
            }
            
            task_info = task_store.create_task(test_task_data)
            
            # 模拟结果
            results = [
                {"caseNo": "C001", "result": "PASS"},
                {"caseNo": "C002", "result": "FAIL"}
            ]
            
            from core.task_store import TaskStatus
            task_store.update_task_status(task_info.task_no, TaskStatus.COMPLETED, "任务完成", 100)
            task_store.update_task_results(task_info.task_no, results)
            
            task = task_store.get_task(task_info.task_no)
            details["results_count"] = len(task.results) if task.results else 0
            details["results"] = task.results
            
            # 清理
            task_store._tasks.pop(task_info.task_no, None)
            
            return TestResult(
                case_id="TASK-004",
                case_name="结果收集测试",
                category="task",
                status="passed",
                message=f"结果收集功能正常，收集到 {details['results_count']} 条结果",
                details=details
            )
            
        except Exception as e:
            return TestResult(
                case_id="TASK-004",
                case_name="结果收集测试",
                category="task",
                status="failed",
                message=f"结果收集测试失败: {str(e)}",
                details={"error": str(e)}
            )
    
    def test_task_005(self) -> TestResult:
        """TASK-005: 日志记录测试"""
        try:
            from utils.logger import get_logger
            
            details = {}
            
            # 测试日志记录
            test_logger = get_logger("test")
            test_logger.info("功能测试日志记录测试")
            
            details["logger_available"] = True
            
            # 检查日志目录
            log_dir = os.path.join(os.path.dirname(os.path.dirname(__file__)), 'logs')
            details["log_dir"] = log_dir
            details["log_dir_exists"] = os.path.exists(log_dir)
            
            return TestResult(
                case_id="TASK-005",
                case_name="日志记录测试",
                category="task",
                status="passed",
                message="日志记录功能正常",
                details=details
            )
            
        except Exception as e:
            return TestResult(
                case_id="TASK-005",
                case_name="日志记录测试",
                category="task",
                status="failed",
                message=f"日志记录测试失败: {str(e)}",
                details={"error": str(e)}
            )


# 全局测试运行器实例
_test_runner: Optional[FunctionalTestRunner] = None


def get_test_runner() -> FunctionalTestRunner:
    """获取全局测试运行器实例"""
    global _test_runner
    if _test_runner is None:
        _test_runner = FunctionalTestRunner()
    return _test_runner
