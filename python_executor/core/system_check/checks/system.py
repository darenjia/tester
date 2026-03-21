"""
系统环境检测项
"""
import sys
import os
import platform
import shutil

from ..registry import register_check
from ..base import BaseCheck
from ..models import CheckResult


@register_check
class PythonEnvironmentCheck(BaseCheck):
    """Python环境检查"""
    id = "SYS-001"
    name = "Python环境检查"
    description = "检查Python版本和必要模块"
    category = "system"
    category_name = "系统环境"
    quick_check = True

    def execute(self) -> CheckResult:
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
            return self.create_result(
                status="warning",
                message=f"Python {sys.version_info.major}.{sys.version_info.minor} 运行正常，但缺少模块: {', '.join(missing_modules)}",
                details=details,
                suggestions=[f"请安装缺少的模块: pip install {' '.join(missing_modules)}"]
            )

        return self.create_result(
            status="passed",
            message=f"Python {sys.version_info.major}.{sys.version_info.minor}.{sys.version_info.micro} 运行正常，所有必要模块已安装",
            details=details
        )


@register_check
class SystemMemoryCheck(BaseCheck):
    """系统内存检查"""
    id = "SYS-002"
    name = "系统内存检查"
    description = "检查可用内存是否充足"
    category = "system"
    category_name = "系统环境"
    quick_check = True

    def execute(self) -> CheckResult:
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
                return self.create_result(
                    status="warning",
                    message=f"可用内存不足: {available_gb:.1f}GB",
                    details=details,
                    suggestions=["请关闭其他程序释放内存，或增加系统内存"]
                )

            return self.create_result(
                status="passed",
                message=f"内存充足: 总计 {total_gb:.1f}GB, 可用 {available_gb:.1f}GB",
                details=details
            )
        except ImportError:
            return self.create_result(
                status="warning",
                message="无法检测内存（缺少 psutil 模块）",
                suggestions=["请安装 psutil: pip install psutil"]
            )


@register_check
class DiskSpaceCheck(BaseCheck):
    """磁盘空间检查"""
    id = "SYS-003"
    name = "磁盘空间检查"
    description = "检查磁盘空间是否充足"
    category = "system"
    category_name = "系统环境"
    quick_check = True

    def execute(self) -> CheckResult:
        try:
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
                return self.create_result(
                    status="warning",
                    message=f"磁盘空间不足: 仅剩 {free_gb:.1f}GB",
                    details=details,
                    suggestions=["请清理磁盘空间，删除不必要的文件"]
                )

            return self.create_result(
                status="passed",
                message=f"磁盘空间充足: {free_gb:.1f}GB 可用",
                details=details
            )
        except Exception as e:
            return self.create_result(
                status="warning",
                message=f"无法检测磁盘空间: {str(e)}"
            )


@register_check
class NetworkConnectionCheck(BaseCheck):
    """网络连接检查"""
    id = "SYS-004"
    name = "网络连接检查"
    description = "检查HTTP服务可访问性"
    category = "system"
    category_name = "系统环境"
    quick_check = True

    def execute(self) -> CheckResult:
        try:
            import urllib.request
            from config.settings import get_config

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
                    return self.create_result(
                        status="passed",
                        message=f"HTTP 服务正常 (端口 {port})",
                        details=details
                    )
                else:
                    return self.create_result(
                        status="failed",
                        message=f"HTTP 服务返回异常状态码: {response.status}",
                        details=details,
                        suggestions=["请检查HTTP服务日志，查看错误原因"]
                    )
        except Exception as e:
            return self.create_result(
                status="failed",
                message=f"HTTP 服务连接失败: {str(e)}",
                details={"error": str(e)},
                suggestions=["请确认HTTP服务已启动，或检查端口配置"]
            )


@register_check
class ConfigFileCheck(BaseCheck):
    """配置文件检查"""
    id = "SYS-005"
    name = "配置文件检查"
    description = "检查配置文件格式和完整性"
    category = "system"
    category_name = "系统环境"
    quick_check = True

    def execute(self) -> CheckResult:
        import json
        from config.settings import get_config

        try:
            config = get_config()
            config_path = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(__file__)))), 'config.json')

            details = {
                "config_path": config_path,
                "config_exists": os.path.exists(config_path)
            }

            if not os.path.exists(config_path):
                return self.create_result(
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
                return self.create_result(
                    status="warning",
                    message=f"配置文件存在，但缺少必要配置项: {', '.join(missing)}",
                    details=details,
                    suggestions=[f"请在配置页面设置: {', '.join(missing)}"]
                )

            return self.create_result(
                status="passed",
                message="配置文件格式正确，所有必要配置项已设置",
                details=details
            )

        except json.JSONDecodeError as e:
            return self.create_result(
                status="failed",
                message=f"配置文件格式错误: {str(e)}",
                suggestions=["请检查config.json文件格式，确保是有效的JSON"]
            )
        except Exception as e:
            return self.create_result(
                status="failed",
                message=f"配置文件检查失败: {str(e)}"
            )