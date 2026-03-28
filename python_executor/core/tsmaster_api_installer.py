"""
TSMasterAPI 安装器模块

提供 TSMasterAPI 的安装、检查、验证功能
安装策略：本地优先，网络备用
"""
import os
import sys
import subprocess
import logging
from typing import Optional, Dict, Any, List


class TSMasterAPIInstaller:
    """TSMasterAPI 安装器"""

    # 常见的 TSMaster API 包存放路径（相对于 TSMaster 安装目录）
    COMMON_API_PATHS = [
        "bin/pythonapi",
        "pythonapi",
        "API/Python",
        "lib/python",
        "TSMasterAPI",
        "TSMasterAPI/lib",
    ]

    # API 模块名称
    MODULE_NAME = "TSMasterAPI"

    def __init__(self, tsmaster_path: str = None):
        """
        初始化安装器

        Args:
            tsmaster_path: TSMaster 安装路径，如果为 None 则从配置读取
        """
        self.tsmaster_path = tsmaster_path
        self.logger = logging.getLogger(__name__)

        # 如果未指定路径，从配置读取
        if self.tsmaster_path is None:
            try:
                from config.settings import get_config
                config = get_config()
                self.tsmaster_path = config.get('software.tsmaster_path', r'C:\Program Files\TSMaster')
            except Exception as e:
                self.logger.warning(f"无法读取 TSMaster 路径配置: {e}")
                self.tsmaster_path = r'C:\Program Files\TSMaster'

    def check_installation_status(self) -> Dict[str, Any]:
        """
        检查 TSMasterAPI 安装状态

        Returns:
            dict: 包含以下字段：
                - installed: 是否已安装
                - version: 版本号（如果已安装）
                - module_path: 模块路径（如果已安装）
                - tsmaster_path: TSMaster 安装路径
                - tsmaster_path_valid: TSMaster 路径是否有效
                - local_whl_found: 是否找到本地 whl 文件
                - local_whl_path: 本地 whl 文件路径
        """
        result = {
            "installed": False,
            "version": None,
            "module_path": None,
            "tsmaster_path": self.tsmaster_path,
            "tsmaster_path_valid": False,
            "local_whl_found": False,
            "local_whl_path": None
        }

        # 检查 TSMaster 安装路径是否有效
        if self.tsmaster_path and os.path.exists(self.tsmaster_path):
            result["tsmaster_path_valid"] = True

        # 检查 TSMasterAPI 是否已安装
        try:
            import TSMasterAPI
            result["installed"] = True
            result["module_path"] = getattr(TSMasterAPI, '__file__', None)

            # 尝试获取版本
            try:
                version = getattr(TSMasterAPI, '__version__', None)
                if version:
                    result["version"] = str(version)
            except Exception:
                pass

        except ImportError:
            pass

        # 查找本地 whl 文件
        whl_path = self.find_local_whl()
        if whl_path:
            result["local_whl_found"] = True
            result["local_whl_path"] = whl_path

        return result

    def find_local_whl(self) -> Optional[str]:
        """
        在 TSMaster 安装目录中查找 whl 文件

        Returns:
            str: whl 文件的完整路径，未找到返回 None
        """
        if not self.tsmaster_path or not os.path.exists(self.tsmaster_path):
            self.logger.warning(f"TSMaster 路径无效: {self.tsmaster_path}")
            return None

        # 在常见路径中搜索
        search_paths = [self.tsmaster_path] + [
            os.path.join(self.tsmaster_path, rel_path)
            for rel_path in self.COMMON_API_PATHS
        ]

        whl_files = []
        for search_path in search_paths:
            if not os.path.exists(search_path):
                continue

            # 在目录中查找 whl 文件
            for root, dirs, files in os.walk(search_path):
                for file in files:
                    if file.endswith('.whl') and 'TSMaster' in file:
                        whl_files.append(os.path.join(root, file))
                        self.logger.debug(f"找到 whl 文件: {file}")

                # 限制搜索深度
                if len(whl_files) >= 10:
                    break

            if whl_files:
                break

        if not whl_files:
            self.logger.info("未找到本地 TSMasterAPI whl 文件")
            return None

        # 优先选择最新版本（假设文件名包含版本号）
        # 文件名格式通常为: TSMasterAPI-1.0.0-py3-none-any.whl
        whl_files.sort(reverse=True)
        selected = whl_files[0]
        self.logger.info(f"选择 whl 文件: {selected}")
        return selected

    def install_from_local(self, whl_path: str = None) -> Dict[str, Any]:
        """
        从本地 whl 文件安装 TSMasterAPI

        Args:
            whl_path: whl 文件路径，如果为 None 则自动查找

        Returns:
            dict: 安装结果
        """
        if whl_path is None:
            whl_path = self.find_local_whl()

        if not whl_path or not os.path.exists(whl_path):
            return {
                "success": False,
                "message": "未找到本地 TSMasterAPI whl 文件",
                "install_method": "local",
                "installed_from": whl_path
            }

        self.logger.info(f"开始从本地安装 TSMasterAPI: {whl_path}")

        try:
            # 使用 pip 安装
            result = subprocess.run(
                [sys.executable, "-m", "pip", "install", whl_path],
                capture_output=True,
                text=True,
                timeout=300  # 5分钟超时
            )

            if result.returncode == 0:
                self.logger.info(f"TSMasterAPI 本地安装成功")
                return {
                    "success": True,
                    "message": "TSMasterAPI 本地安装成功",
                    "install_method": "local",
                    "installed_from": whl_path,
                    "output": result.stdout
                }
            else:
                self.logger.error(f"TSMasterAPI 本地安装失败: {result.stderr}")
                return {
                    "success": False,
                    "message": f"安装失败: {result.stderr}",
                    "install_method": "local",
                    "installed_from": whl_path,
                    "output": result.stderr
                }

        except subprocess.TimeoutExpired:
            self.logger.error("TSMasterAPI 安装超时")
            return {
                "success": False,
                "message": "安装超时（超过5分钟）",
                "install_method": "local",
                "installed_from": whl_path
            }
        except Exception as e:
            self.logger.error(f"TSMasterAPI 安装异常: {e}")
            return {
                "success": False,
                "message": f"安装异常: {str(e)}",
                "install_method": "local",
                "installed_from": whl_path
            }

    def install_from_network(self) -> Dict[str, Any]:
        """
        从 PyPI 网络安装 TSMasterAPI

        Returns:
            dict: 安装结果
        """
        self.logger.info("开始从网络安装 TSMasterAPI")

        try:
            # 使用 pip 从 PyPI 安装
            result = subprocess.run(
                [sys.executable, "-m", "pip", "install", self.MODULE_NAME],
                capture_output=True,
                text=True,
                timeout=600  # 10分钟超时（网络可能较慢）
            )

            if result.returncode == 0:
                self.logger.info("TSMasterAPI 网络安装成功")
                return {
                    "success": True,
                    "message": "TSMasterAPI 网络安装成功",
                    "install_method": "network",
                    "installed_from": "PyPI",
                    "output": result.stdout
                }
            else:
                self.logger.error(f"TSMasterAPI 网络安装失败: {result.stderr}")
                return {
                    "success": False,
                    "message": f"网络安装失败: {result.stderr}",
                    "install_method": "network",
                    "installed_from": "PyPI",
                    "output": result.stderr
                }

        except subprocess.TimeoutExpired:
            self.logger.error("TSMasterAPI 网络安装超时")
            return {
                "success": False,
                "message": "网络安装超时（超过10分钟）",
                "install_method": "network",
                "installed_from": "PyPI"
            }
        except Exception as e:
            self.logger.error(f"TSMasterAPI 网络安装异常: {e}")
            return {
                "success": False,
                "message": f"网络安装异常: {str(e)}",
                "install_method": "network",
                "installed_from": "PyPI"
            }

    def install(self, prefer_local: bool = True) -> Dict[str, Any]:
        """
        执行安装（本地优先，网络备用）

        Args:
            prefer_local: 是否优先使用本地安装，默认 True

        Returns:
            dict: 安装结果
        """
        self.logger.info("开始安装 TSMasterAPI")

        # 首先检查是否已安装
        status = self.check_installation_status()
        if status["installed"]:
            return {
                "success": True,
                "message": "TSMasterAPI 已安装，无需重复安装",
                "install_method": "already_installed",
                "version": status["version"],
                "module_path": status["module_path"]
            }

        if prefer_local:
            # 策略：本地优先
            # 1. 尝试本地安装
            if status["local_whl_found"]:
                result = self.install_from_local(status["local_whl_path"])
                if result["success"]:
                    return result

                self.logger.warning(f"本地安装失败，尝试网络安装: {result['message']}")

            # 2. 本地失败或无本地文件，尝试网络安装
            return self.install_from_network()
        else:
            # 策略：直接网络安装
            return self.install_from_network()

    def verify_installation(self) -> Dict[str, Any]:
        """
        验证安装结果

        Returns:
            dict: 验证结果
        """
        self.logger.info("验证 TSMasterAPI 安装")

        result = {
            "success": False,
            "importable": False,
            "version": None,
            "module_path": None,
            "error": None
        }

        try:
            import TSMasterAPI
            result["importable"] = True
            result["success"] = True
            result["module_path"] = getattr(TSMasterAPI, '__file__', None)

            # 尝试获取版本
            try:
                version = getattr(TSMasterAPI, '__version__', None)
                if version:
                    result["version"] = str(version)
            except Exception:
                pass

            self.logger.info(f"TSMasterAPI 验证成功: {result['module_path']}")

        except ImportError as e:
            result["error"] = f"无法导入 TSMasterAPI: {str(e)}"
            self.logger.error(result["error"])

        except Exception as e:
            result["error"] = f"验证过程出错: {str(e)}"
            self.logger.error(result["error"])

        return result

    def uninstall(self) -> Dict[str, Any]:
        """
        卸载 TSMasterAPI

        Returns:
            dict: 卸载结果
        """
        self.logger.info("卸载 TSMasterAPI")

        try:
            result = subprocess.run(
                [sys.executable, "-m", "pip", "uninstall", "-y", self.MODULE_NAME],
                capture_output=True,
                text=True,
                timeout=120
            )

            if result.returncode == 0:
                return {
                    "success": True,
                    "message": "TSMasterAPI 卸载成功",
                    "output": result.stdout
                }
            else:
                return {
                    "success": False,
                    "message": f"卸载失败: {result.stderr}",
                    "output": result.stderr
                }

        except Exception as e:
            self.logger.error(f"卸载异常: {e}")
            return {
                "success": False,
                "message": f"卸载异常: {str(e)}"
            }


# 全局安装器实例
_installer_instance: Optional[TSMasterAPIInstaller] = None


def get_tsmaster_api_installer(tsmaster_path: str = None) -> TSMasterAPIInstaller:
    """
    获取 TSMasterAPI 安装器实例

    Args:
        tsmaster_path: TSMaster 安装路径（可选，用于覆盖默认值）

    Returns:
        TSMasterAPIInstaller: 安装器实例
    """
    global _installer_instance

    # 创建新实例的情况：
    # 1. 实例不存在
    # 2. 指定了不同的路径
    if _installer_instance is None:
        _installer_instance = TSMasterAPIInstaller(tsmaster_path)
    elif tsmaster_path is not None and tsmaster_path != _installer_instance.tsmaster_path:
        _installer_instance = TSMasterAPIInstaller(tsmaster_path)

    return _installer_instance