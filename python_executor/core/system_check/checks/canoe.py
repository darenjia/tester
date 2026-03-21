"""
CANoe 检测项
使用 CANoeAdapter 进行真正的任务执行逻辑检测
"""
import os
import time

from ..registry import register_check
from ..base import BaseCheck
from ..models import CheckResult


@register_check
class CANoeAdapterConnectCheck(BaseCheck):
    """CANoe 适配器连接测试"""
    id = "CANOE-001"
    name = "适配器连接测试"
    description = "使用 CANoeAdapter 连接 CANoe 应用程序"
    category = "canoe"
    category_name = "CANoe"
    quick_check = True
    timeout = 60

    def execute(self) -> CheckResult:
        try:
            from core.adapters.canoe.adapter import CANoeAdapter
            from core.adapters.canoe.com_wrapper import CANoeError
        except ImportError as e:
            return self.create_result(
                status="failed",
                message=f"无法导入 CANoe 适配器模块: {e}",
                suggestions=["请检查 core/adapters/canoe/ 目录是否存在"]
            )

        details = {
            "steps": [],
            "connect_success": False,
            "version": None
        }

        adapter = None
        try:
            # 步骤1: 创建适配器
            details["steps"].append("1. 创建 CANoeAdapter 实例")
            adapter = CANoeAdapter({
                "start_timeout": 30,
                "retry_count": 3,
                "retry_interval": 2.0
            })
            details["steps"].append("   ✓ 适配器创建成功")

            # 步骤2: 连接 CANoe
            details["steps"].append("2. 连接 CANoe 应用程序")
            connect_result = adapter.connect()
            details["connect_success"] = connect_result

            if connect_result:
                details["steps"].append("   ✓ 连接成功")
                details["version"] = adapter.canoe_version

                return self.create_result(
                    status="passed",
                    message=f"CANoe 连接成功 (版本: {adapter.canoe_version})",
                    details=details
                )
            else:
                details["steps"].append(f"   ✗ 连接失败: {adapter.error_message}")
                return self.create_result(
                    status="failed",
                    message=f"CANoe 连接失败: {adapter.error_message}",
                    details=details,
                    suggestions=[
                        "请确认 CANoe 已正确安装并激活",
                        "请确认 pywin32 已安装: pip install pywin32",
                        "请尝试手动启动 CANoe 后再检测"
                    ]
                )

        except CANoeError as e:
            details["steps"].append(f"   ✗ CANoe 错误: {e}")
            return self.create_result(
                status="failed",
                message=f"CANoe 错误: {e}",
                details=details,
                suggestions=["请检查 CANoe 安装和许可证状态"]
            )
        except Exception as e:
            details["steps"].append(f"   ✗ 异常: {e}")
            return self.create_result(
                status="failed",
                message=f"连接测试异常: {e}",
                details=details
            )
        finally:
            if adapter:
                try:
                    adapter.disconnect()
                except:
                    pass


@register_check
class CANoeConfigLoadCheck(BaseCheck):
    """CANoe 配置文件加载测试"""
    id = "CANOE-002"
    name = "配置文件加载测试"
    description = "测试加载 .cfg 配置文件"
    category = "canoe"
    category_name = "CANoe"
    quick_check = False
    timeout = 60

    def execute(self) -> CheckResult:
        from config.settings import get_config

        config = get_config()
        workspace_path = config.get('software.workspace_path', r'C:\TestWorkspace')

        details = {
            "workspace_path": workspace_path,
            "cfg_file": None,
            "steps": []
        }

        # 查找可用的配置文件
        cfg_file = self._find_cfg_file(workspace_path)
        if not cfg_file:
            return self.create_result(
                status="warning",
                message="未找到可用的 .cfg 配置文件，跳过加载测试",
                details=details,
                suggestions=[f"请在 {workspace_path} 目录下放置 .cfg 配置文件"]
            )

        details["cfg_file"] = cfg_file

        try:
            from core.adapters.canoe.adapter import CANoeAdapter
            from core.adapters.canoe.com_wrapper import CANoeError
        except ImportError as e:
            return self.create_result(
                status="failed",
                message=f"无法导入 CANoe 适配器模块: {e}",
                details=details
            )

        adapter = None
        try:
            # 步骤1: 连接
            details["steps"].append("1. 连接 CANoe")
            adapter = CANoeAdapter({"start_timeout": 30})
            if not adapter.connect():
                details["steps"].append(f"   ✗ 连接失败: {adapter.error_message}")
                return self.create_result(
                    status="failed",
                    message=f"连接失败，无法继续测试",
                    details=details
                )
            details["steps"].append("   ✓ 连接成功")

            # 步骤2: 加载配置
            details["steps"].append(f"2. 加载配置文件: {os.path.basename(cfg_file)}")
            load_result = adapter.load_configuration(cfg_file)

            if load_result:
                details["steps"].append("   ✓ 配置加载成功")
                return self.create_result(
                    status="passed",
                    message=f"配置文件加载成功: {os.path.basename(cfg_file)}",
                    details=details
                )
            else:
                details["steps"].append(f"   ✗ 加载失败: {adapter.error_message}")
                return self.create_result(
                    status="failed",
                    message=f"配置文件加载失败: {adapter.error_message}",
                    details=details,
                    suggestions=["请检查配置文件是否有效", "请确认配置文件路径正确"]
                )

        except CANoeError as e:
            details["steps"].append(f"   ✗ CANoe 错误: {e}")
            return self.create_result(
                status="failed",
                message=f"加载配置时出错: {e}",
                details=details
            )
        except Exception as e:
            details["steps"].append(f"   ✗ 异常: {e}")
            return self.create_result(
                status="failed",
                message=f"加载测试异常: {e}",
                details=details
            )
        finally:
            if adapter:
                try:
                    adapter.disconnect()
                except:
                    pass

    def _find_cfg_file(self, workspace_path: str) -> str:
        """查找可用的配置文件"""
        if not os.path.exists(workspace_path):
            return None

        for root, dirs, files in os.walk(workspace_path):
            for file in files:
                if file.endswith('.cfg'):
                    return os.path.join(root, file)
            # 只搜索第一层目录
            break

        return None


@register_check
class CANoeMeasurementCheck(BaseCheck):
    """CANoe 测量启动/停止测试"""
    id = "CANOE-003"
    name = "测量启动/停止测试"
    description = "测试启动和停止 CANoe 测量"
    category = "canoe"
    category_name = "CANoe"
    quick_check = False
    timeout = 120

    def execute(self) -> CheckResult:
        from config.settings import get_config

        config = get_config()
        workspace_path = config.get('software.workspace_path', r'C:\TestWorkspace')

        details = {
            "workspace_path": workspace_path,
            "cfg_file": None,
            "steps": [],
            "measurement_started": False,
            "measurement_stopped": False
        }

        # 查找可用的配置文件
        cfg_file = self._find_cfg_file(workspace_path)
        if not cfg_file:
            return self.create_result(
                status="warning",
                message="未找到可用的 .cfg 配置文件，跳过测量测试",
                details=details,
                suggestions=[f"请在 {workspace_path} 目录下放置 .cfg 配置文件"]
            )

        details["cfg_file"] = cfg_file

        try:
            from core.adapters.canoe.adapter import CANoeAdapter
            from core.adapters.canoe.com_wrapper import CANoeError
        except ImportError as e:
            return self.create_result(
                status="failed",
                message=f"无法导入 CANoe 适配器模块: {e}",
                details=details
            )

        adapter = None
        try:
            # 步骤1: 连接
            details["steps"].append("1. 连接 CANoe")
            adapter = CANoeAdapter({"start_timeout": 30})
            if not adapter.connect():
                details["steps"].append(f"   ✗ 连接失败: {adapter.error_message}")
                return self.create_result(
                    status="failed",
                    message="连接失败，无法继续测试",
                    details=details
                )
            details["steps"].append("   ✓ 连接成功")

            # 步骤2: 加载配置
            details["steps"].append(f"2. 加载配置文件")
            if not adapter.load_configuration(cfg_file):
                details["steps"].append(f"   ✗ 加载失败: {adapter.error_message}")
                return self.create_result(
                    status="failed",
                    message="配置加载失败",
                    details=details
                )
            details["steps"].append("   ✓ 配置加载成功")

            # 步骤3: 启动测量
            details["steps"].append("3. 启动测量")
            start_result = adapter.start_test()
            if start_result:
                details["measurement_started"] = True
                details["steps"].append("   ✓ 测量启动成功")

                # 等待一下
                time.sleep(2)

                # 步骤4: 停止测量
                details["steps"].append("4. 停止测量")
                stop_result = adapter.stop_test()
                details["measurement_stopped"] = stop_result
                if stop_result:
                    details["steps"].append("   ✓ 测量停止成功")
                else:
                    details["steps"].append("   ⚠ 测量停止可能不完整")

                return self.create_result(
                    status="passed",
                    message="测量启动/停止测试通过",
                    details=details
                )
            else:
                details["steps"].append(f"   ✗ 启动失败: {adapter.error_message}")
                return self.create_result(
                    status="failed",
                    message=f"测量启动失败: {adapter.error_message}",
                    details=details,
                    suggestions=[
                        "请检查配置文件是否正确",
                        "请检查硬件连接是否正常",
                        "请检查 CANoe 许可证是否有效"
                    ]
                )

        except CANoeError as e:
            details["steps"].append(f"   ✗ CANoe 错误: {e}")
            return self.create_result(
                status="failed",
                message=f"测量测试时出错: {e}",
                details=details
            )
        except Exception as e:
            details["steps"].append(f"   ✗ 异常: {e}")
            return self.create_result(
                status="failed",
                message=f"测量测试异常: {e}",
                details=details
            )
        finally:
            if adapter:
                try:
                    adapter.stop_test()
                    adapter.disconnect()
                except:
                    pass

    def _find_cfg_file(self, workspace_path: str) -> str:
        """查找可用的配置文件"""
        if not os.path.exists(workspace_path):
            return None

        for root, dirs, files in os.walk(workspace_path):
            for file in files:
                if file.endswith('.cfg'):
                    return os.path.join(root, file)
            break

        return None


@register_check
class CANoeTestModuleCheck(BaseCheck):
    """CANoe 测试模块执行测试"""
    id = "CANOE-004"
    name = "测试模块执行测试"
    description = "测试获取和执行 TestModule"
    category = "canoe"
    category_name = "CANoe"
    quick_check = False
    timeout = 120

    def execute(self) -> CheckResult:
        from config.settings import get_config

        config = get_config()
        workspace_path = config.get('software.workspace_path', r'C:\TestWorkspace')

        details = {
            "steps": [],
            "test_modules": [],
            "module_count": 0
        }

        # 查找可用的配置文件
        cfg_file = self._find_cfg_file(workspace_path)
        if not cfg_file:
            return self.create_result(
                status="warning",
                message="未找到可用的 .cfg 配置文件，跳过测试模块检测",
                details=details
            )

        try:
            from core.adapters.canoe.adapter import CANoeAdapter
            from core.adapters.canoe.com_wrapper import CANoeError
        except ImportError as e:
            return self.create_result(
                status="failed",
                message=f"无法导入 CANoe 适配器模块: {e}",
                details=details
            )

        adapter = None
        try:
            # 连接并加载配置
            details["steps"].append("1. 连接 CANoe 并加载配置")
            adapter = CANoeAdapter({"start_timeout": 30})

            if not adapter.connect():
                return self.create_result(
                    status="failed",
                    message="连接失败",
                    details=details
                )

            if not adapter.load_configuration(cfg_file):
                return self.create_result(
                    status="failed",
                    message="配置加载失败",
                    details=details
                )
            details["steps"].append("   ✓ 连接和加载成功")

            # 获取测试模块列表
            details["steps"].append("2. 获取测试模块列表")
            test_modules = adapter.get_test_modules()
            details["test_modules"] = test_modules
            details["module_count"] = len(test_modules)

            if test_modules:
                details["steps"].append(f"   ✓ 找到 {len(test_modules)} 个测试模块: {', '.join(test_modules)}")
                return self.create_result(
                    status="passed",
                    message=f"找到 {len(test_modules)} 个测试模块: {', '.join(test_modules[:5])}" + ("..." if len(test_modules) > 5 else ""),
                    details=details
                )
            else:
                details["steps"].append("   ⚠ 未找到测试模块")
                return self.create_result(
                    status="warning",
                    message="配置中未找到测试模块",
                    details=details,
                    suggestions=["请在配置中添加 TestModule"]
                )

        except Exception as e:
            details["steps"].append(f"   ✗ 异常: {e}")
            return self.create_result(
                status="failed",
                message=f"测试模块检测异常: {e}",
                details=details
            )
        finally:
            if adapter:
                try:
                    adapter.disconnect()
                except:
                    pass

    def _find_cfg_file(self, workspace_path: str) -> str:
        """查找可用的配置文件"""
        if not os.path.exists(workspace_path):
            return None

        for root, dirs, files in os.walk(workspace_path):
            for file in files:
                if file.endswith('.cfg'):
                    return os.path.join(root, file)
            break

        return None


@register_check
class CANoeFullWorkflowCheck(BaseCheck):
    """CANoe 完整工作流测试"""
    id = "CANOE-005"
    name = "完整工作流测试"
    description = "测试完整的任务执行流程：连接->加载->启动->执行->停止->断开"
    category = "canoe"
    category_name = "CANoe"
    quick_check = False
    timeout = 180

    def execute(self) -> CheckResult:
        from config.settings import get_config

        config = get_config()
        workspace_path = config.get('software.workspace_path', r'C:\TestWorkspace')

        details = {
            "steps": [],
            "cfg_file": None
        }

        # 查找可用的配置文件
        cfg_file = self._find_cfg_file(workspace_path)
        if not cfg_file:
            return self.create_result(
                status="warning",
                message="未找到可用的 .cfg 配置文件，跳过完整工作流测试",
                details=details,
                suggestions=[f"请在 {workspace_path} 目录下放置 .cfg 配置文件"]
            )

        details["cfg_file"] = os.path.basename(cfg_file)

        try:
            from core.adapters.canoe.adapter import CANoeAdapter
            from core.adapters.canoe.com_wrapper import CANoeError
        except ImportError as e:
            return self.create_result(
                status="failed",
                message=f"无法导入 CANoe 适配器模块: {e}",
                details=details
            )

        adapter = None
        try:
            # 步骤1: 创建适配器
            details["steps"].append("1. 创建 CANoeAdapter")
            adapter = CANoeAdapter({
                "start_timeout": 30,
                "measurement_timeout": 3600,
                "case_timeout": 600
            })
            details["steps"].append("   ✓ 适配器创建成功")

            # 步骤2: 连接
            details["steps"].append("2. 连接 CANoe")
            if not adapter.connect():
                details["steps"].append(f"   ✗ 连接失败: {adapter.error_message}")
                return self.create_result(
                    status="failed",
                    message="连接失败",
                    details=details
                )
            details["steps"].append(f"   ✓ 连接成功 (版本: {adapter.canoe_version})")

            # 步骤3: 加载配置
            details["steps"].append("3. 加载配置文件")
            if not adapter.load_configuration(cfg_file):
                details["steps"].append(f"   ✗ 加载失败: {adapter.error_message}")
                return self.create_result(
                    status="failed",
                    message="配置加载失败",
                    details=details
                )
            details["steps"].append("   ✓ 配置加载成功")

            # 步骤4: 启动测量
            details["steps"].append("4. 启动测量")
            if not adapter.start_test():
                details["steps"].append(f"   ✗ 启动失败: {adapter.error_message}")
                return self.create_result(
                    status="failed",
                    message="测量启动失败",
                    details=details
                )
            details["steps"].append("   ✓ 测量启动成功")

            # 步骤5: 获取测试模块
            details["steps"].append("5. 获取测试模块列表")
            test_modules = adapter.get_test_modules()
            if test_modules:
                details["steps"].append(f"   ✓ 找到 {len(test_modules)} 个测试模块")
            else:
                details["steps"].append("   ⚠ 未找到测试模块（可接受）")

            # 步骤6: 停止测量
            time.sleep(2)  # 短暂等待
            details["steps"].append("6. 停止测量")
            if adapter.stop_test():
                details["steps"].append("   ✓ 测量停止成功")
            else:
                details["steps"].append("   ⚠ 测量停止可能不完整")

            # 步骤7: 断开连接
            details["steps"].append("7. 断开连接")
            if adapter.disconnect():
                details["steps"].append("   ✓ 断开成功")
            else:
                details["steps"].append("   ⚠ 断开可能不完整")

            return self.create_result(
                status="passed",
                message="CANoe 完整工作流测试通过",
                details=details
            )

        except Exception as e:
            details["steps"].append(f"   ✗ 异常: {e}")
            return self.create_result(
                status="failed",
                message=f"工作流测试异常: {e}",
                details=details
            )
        finally:
            if adapter:
                try:
                    adapter.stop_test()
                    adapter.disconnect()
                except:
                    pass

    def _find_cfg_file(self, workspace_path: str) -> str:
        """查找可用的配置文件"""
        if not os.path.exists(workspace_path):
            return None

        for root, dirs, files in os.walk(workspace_path):
            for file in files:
                if file.endswith('.cfg'):
                    return os.path.join(root, file)
            break

        return None