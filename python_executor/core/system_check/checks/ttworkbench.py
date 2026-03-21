"""
TTworkbench 检测项
"""
import os

from ..registry import register_check
from ..base import BaseCheck
from ..models import CheckResult


@register_check
class TTworkbenchPathCheck(BaseCheck):
    """TTworkbench安装路径检查"""
    id = "TTM-001"
    name = "安装路径检查"
    description = "检查TTworkbench安装路径"
    category = "ttworkbench"
    category_name = "TTworkbench"
    quick_check = True

    def execute(self) -> CheckResult:
        from config.settings import get_config

        config = get_config()
        ttman_path = config.get('software.ttman_path', r'C:\Spirent\TTman.bat')

        details = {
            "configured_path": ttman_path,
            "path_exists": os.path.exists(ttman_path)
        }

        if not os.path.exists(ttman_path):
            return self.create_result(
                status="failed",
                message=f"TTworkbench路径不存在: {ttman_path}",
                details=details,
                suggestions=["请在配置页面设置正确的TTworkbench安装路径"]
            )

        return self.create_result(
            status="passed",
            message=f"TTworkbench路径存在: {ttman_path}",
            details=details
        )


@register_check
class TTworkbenchBatchFileCheck(BaseCheck):
    """TTworkbench批处理文件检查"""
    id = "TTM-002"
    name = "批处理文件检查"
    description = "检查TTman.bat是否存在"
    category = "ttworkbench"
    category_name = "TTworkbench"
    quick_check = False

    def execute(self) -> CheckResult:
        from config.settings import get_config

        config = get_config()
        ttman_path = config.get('software.ttman_path', r'C:\Spirent\TTman.bat')

        details = {
            "bat_path": ttman_path,
            "file_exists": os.path.exists(ttman_path) and os.path.isfile(ttman_path)
        }

        if not details["file_exists"]:
            return self.create_result(
                status="failed",
                message=f"TTman.bat文件不存在: {ttman_path}",
                details=details,
                suggestions=["请确认TTworkbench已正确安装"]
            )

        return self.create_result(
            status="passed",
            message="TTman.bat文件存在",
            details=details
        )


@register_check
class TTworkbenchScriptCheck(BaseCheck):
    """TTworkbench脚本可读性测试"""
    id = "TTM-003"
    name = "脚本可读性测试"
    description = "测试读取批处理脚本"
    category = "ttworkbench"
    category_name = "TTworkbench"
    quick_check = True

    def execute(self) -> CheckResult:
        from config.settings import get_config

        config = get_config()
        ttman_path = config.get('software.ttman_path', r'C:\Spirent\TTman.bat')

        details = {
            "bat_path": ttman_path
        }

        if not os.path.exists(ttman_path):
            return self.create_result(
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
                    return self.create_result(
                        status="warning",
                        message="脚本可读，但未找到java或ttman引用",
                        details=details,
                        suggestions=["请检查TTman.bat是否为正确的启动脚本"]
                    )

                return self.create_result(
                    status="passed",
                    message=f"脚本可读，大小 {len(content)} 字节",
                    details=details
                )

        except Exception as e:
            return self.create_result(
                status="failed",
                message=f"无法读取脚本: {str(e)}",
                details=details
            )


@register_check
class TTworkbenchDependencyCheck(BaseCheck):
    """TTworkbench依赖检查"""
    id = "TTM-004"
    name = "依赖检查"
    description = "检查jar文件和配置文件"
    category = "ttworkbench"
    category_name = "TTworkbench"
    quick_check = True

    def execute(self) -> CheckResult:
        from config.settings import get_config

        config = get_config()
        ttman_path = config.get('software.ttman_path', r'C:\Spirent\TTman.bat')

        details = {
            "install_dir": ""
        }

        if not os.path.exists(ttman_path):
            return self.create_result(
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
            return self.create_result(
                status="warning",
                message="未找到jar依赖文件",
                details=details,
                suggestions=["请确认TTworkbench安装完整"]
            )

        return self.create_result(
            status="passed",
            message=f"找到 {len(jar_files)} 个jar文件",
            details=details
        )


@register_check
class TTworkbenchFullChainCheck(BaseCheck):
    """TTworkbench完整调用链测试"""
    id = "TTM-005"
    name = "完整调用链测试"
    description = "测试完整的调用流程"
    category = "ttworkbench"
    category_name = "TTworkbench"
    quick_check = False
    timeout = 60

    def execute(self) -> CheckResult:
        from config.settings import get_config

        config = get_config()
        ttman_path = config.get('software.ttman_path', r'C:\Spirent\TTman.bat')

        details = {
            "steps": []
        }

        # 步骤1: 检查文件存在
        details["steps"].append("1. 检查TTman.bat存在")
        if not os.path.exists(ttman_path):
            details["steps"].append("   ✗ 文件不存在")
            return self.create_result(
                status="failed",
                message="TTman.bat文件不存在",
                details=details
            )
        details["steps"].append("   ✓ 存在")

        # 步骤2: 检查可读性
        details["steps"].append("2. 检查脚本可读性")
        try:
            with open(ttman_path, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read()
            details["steps"].append("   ✓ 可读")
        except Exception as e:
            details["steps"].append(f"   ✗ 读取失败: {str(e)}")
            return self.create_result(
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

        return self.create_result(
            status="passed",
            message="TTworkbench环境检查通过",
            details=details
        )