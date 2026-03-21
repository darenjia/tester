"""
TSMaster 检测项
"""
import os

from ..registry import register_check
from ..base import BaseCheck
from ..models import CheckResult


@register_check
class TSMasterPathCheck(BaseCheck):
    """TSMaster安装路径检查"""
    id = "TS-001"
    name = "安装路径检查"
    description = "检查TSMaster安装路径"
    category = "tsmaster"
    category_name = "TSMaster"
    quick_check = True

    def execute(self) -> CheckResult:
        from config.settings import get_config

        config = get_config()
        tsmaster_path = config.get('software.tsmaster_path', r'C:\Program Files\TSMaster')

        details = {
            "configured_path": tsmaster_path,
            "path_exists": os.path.exists(tsmaster_path)
        }

        if not os.path.exists(tsmaster_path):
            return self.create_result(
                status="failed",
                message=f"TSMaster路径不存在: {tsmaster_path}",
                details=details,
                suggestions=["请在配置页面设置正确的TSMaster安装路径"]
            )

        return self.create_result(
            status="passed",
            message=f"TSMaster路径存在: {tsmaster_path}",
            details=details
        )


@register_check
class TSMasterPythonAPICheck(BaseCheck):
    """TSMaster Python API检查"""
    id = "TS-002"
    name = "Python API检查"
    description = "检查TSMaster Python API"
    category = "tsmaster"
    category_name = "TSMaster"
    quick_check = True

    def execute(self) -> CheckResult:
        try:
            from TSMaster import TSMaster
            details = {
                "api_type": "Python API",
                "available": True
            }
            return self.create_result(
                status="passed",
                message="TSMaster Python API可用",
                details=details
            )
        except ImportError:
            return self.create_result(
                status="warning",
                message="TSMaster Python API未安装",
                details={"api_type": "Python API", "available": False},
                suggestions=["请安装TSMaster Python API或检查安装路径"]
            )


@register_check
class TSMasterRPCCheck(BaseCheck):
    """TSMaster RPC API检查"""
    id = "TS-003"
    name = "RPC API检查"
    description = "检查TSMasterAPI模块"
    category = "tsmaster"
    category_name = "TSMaster"
    quick_check = True

    def execute(self) -> CheckResult:
        try:
            import TSMasterAPI
            details = {
                "api_type": "RPC API",
                "available": True
            }
            return self.create_result(
                status="passed",
                message="TSMaster RPC API可用",
                details=details
            )
        except ImportError:
            return self.create_result(
                status="warning",
                message="TSMaster RPC API未安装",
                details={"api_type": "RPC API", "available": False},
                suggestions=["请安装TSMasterAPI模块"]
            )


@register_check
class TSMasterConnectionCheck(BaseCheck):
    """TSMaster连接测试"""
    id = "TS-004"
    name = "连接测试"
    description = "测试与TSMaster的连接"
    category = "tsmaster"
    category_name = "TSMaster"
    quick_check = True
    timeout = 60

    def execute(self) -> CheckResult:
        try:
            from TSMaster import TSMaster
        except ImportError:
            return self.create_result(
                status="failed",
                message="TSMaster Python API未安装",
                suggestions=["请先安装TSMaster Python API"]
            )

        details = {
            "connection_test": "failed"
        }

        ts = None
        try:
            ts = TSMaster()
            ts.connect()
            details["connection_test"] = "success"
            return self.create_result(
                status="passed",
                message="TSMaster连接测试通过",
                details=details
            )
        except Exception as e:
            details["error"] = str(e)
            return self.create_result(
                status="failed",
                message=f"TSMaster连接失败: {str(e)}",
                details=details,
                suggestions=["请确认TSMaster已启动，且RPC服务正常运行"]
            )
        finally:
            if ts:
                try:
                    ts.disconnect()
                except:
                    pass


@register_check
class TSMasterVersionCheck(BaseCheck):
    """TSMaster版本获取测试"""
    id = "TS-005"
    name = "版本获取测试"
    description = "测试获取版本信息"
    category = "tsmaster"
    category_name = "TSMaster"
    quick_check = False
    timeout = 60

    def execute(self) -> CheckResult:
        try:
            from TSMaster import TSMaster
        except ImportError:
            return self.create_result(
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

            return self.create_result(
                status="passed",
                message=f"TSMaster版本: {version}",
                details=details
            )
        except Exception as e:
            return self.create_result(
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


@register_check
class TSMasterWorkspaceCheck(BaseCheck):
    """TSMaster工程文件路径检查"""
    id = "TS-006"
    name = "工程文件路径检查"
    description = "检查tproj文件路径"
    category = "tsmaster"
    category_name = "TSMaster"
    quick_check = True

    def execute(self) -> CheckResult:
        from config.settings import get_config

        config = get_config()
        workspace_path = config.get('software.workspace_path', r'C:\TestWorkspace')

        details = {
            "workspace_path": workspace_path,
            "path_exists": os.path.exists(workspace_path)
        }

        if not os.path.exists(workspace_path):
            return self.create_result(
                status="warning",
                message=f"工作空间路径不存在: {workspace_path}",
                details=details,
                suggestions=["请在配置页面设置正确的工作空间路径"]
            )

        return self.create_result(
            status="passed",
            message=f"工作空间路径存在: {workspace_path}",
            details=details
        )


@register_check
class TSMasterProjectFilesCheck(BaseCheck):
    """TSMaster工程文件读取测试"""
    id = "TS-007"
    name = "工程文件读取测试"
    description = "测试读取tproj文件"
    category = "tsmaster"
    category_name = "TSMaster"
    quick_check = False

    def execute(self) -> CheckResult:
        from config.settings import get_config

        config = get_config()
        workspace_path = config.get('software.workspace_path', r'C:\TestWorkspace')

        details = {
            "workspace_path": workspace_path
        }

        if not os.path.exists(workspace_path):
            return self.create_result(
                status="warning",
                message="工作空间路径不存在",
                suggestions=["请先设置正确的工作空间路径"]
            )

        # 查找tproj文件
        tproj_files = []
        for root, dirs, files in os.walk(workspace_path):
            for file in files:
                if file.endswith('.tproj'):
                    tproj_files.append(os.path.join(root, file))
            if len(tproj_files) >= 100:
                break

        details["tproj_files_found"] = len(tproj_files)
        details["sample_files"] = [os.path.basename(f) for f in tproj_files[:5]]

        if not tproj_files:
            return self.create_result(
                status="warning",
                message=f"在工作空间路径下未找到.tproj工程文件",
                details=details,
                suggestions=["请将TSMaster工程文件(.tproj)放入工作空间目录"]
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

        return self.create_result(
            status="passed",
            message=f"找到 {len(tproj_files)} 个.tproj工程文件",
            details=details
        )


@register_check
class TSMasterFullChainCheck(BaseCheck):
    """TSMaster完整调用链测试"""
    id = "TS-008"
    name = "完整调用链测试"
    description = "测试完整的调用流程"
    category = "tsmaster"
    category_name = "TSMaster"
    quick_check = False
    timeout = 120

    def execute(self) -> CheckResult:
        try:
            from TSMaster import TSMaster
        except ImportError:
            return self.create_result(
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

            return self.create_result(
                status="passed",
                message="TSMaster完整调用链测试通过",
                details=details
            )

        except Exception as e:
            details["steps"].append(f"   ✗ 失败: {str(e)}")
            return self.create_result(
                status="failed",
                message=f"完整调用链测试失败: {str(e)}",
                details=details,
                suggestions=["请检查TSMaster安装和运行状态"]
            )
        finally:
            if ts:
                try:
                    ts.disconnect()
                except:
                    pass