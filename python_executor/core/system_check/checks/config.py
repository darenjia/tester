"""
配置检测项
"""
import os

from ..registry import register_check
from ..base import BaseCheck
from ..models import CheckResult


@register_check
class ConfigHTTPPortCheck(BaseCheck):
    """HTTP端口配置检查"""
    id = "CFG-001"
    name = "HTTP端口检查"
    description = "检查HTTP端口配置"
    category = "config"
    category_name = "配置"
    quick_check = True

    def execute(self) -> CheckResult:
        from config.settings import get_config

        config = get_config()
        port = config.get('http.port', 8180)

        details = {
            "configured_port": port,
            "valid": isinstance(port, int) and 1024 <= port <= 65535
        }

        if not details["valid"]:
            return self.create_result(
                status="warning",
                message=f"HTTP端口配置可能无效: {port}",
                details=details,
                suggestions=["请配置1024-65535范围内的端口号"]
            )

        return self.create_result(
            status="passed",
            message=f"HTTP端口配置有效: {port}",
            details=details
        )


@register_check
class ConfigDeviceIDCheck(BaseCheck):
    """设备ID配置检查"""
    id = "CFG-002"
    name = "设备ID检查"
    description = "检查设备ID配置"
    category = "config"
    category_name = "配置"
    quick_check = True

    def execute(self) -> CheckResult:
        from config.settings import get_config

        config = get_config()
        device_id = config.get('device.device_id', '')

        details = {
            "device_id": device_id,
            "configured": bool(device_id)
        }

        if not device_id:
            return self.create_result(
                status="warning",
                message="设备ID未配置",
                details=details,
                suggestions=["请在配置页面设置设备ID"]
            )

        return self.create_result(
            status="passed",
            message=f"设备ID已配置: {device_id}",
            details=details
        )


@register_check
class ConfigSoftwarePathCheck(BaseCheck):
    """软件路径配置检查"""
    id = "CFG-003"
    name = "软件路径配置检查"
    description = "检查各软件路径配置"
    category = "config"
    category_name = "配置"
    quick_check = True

    def execute(self) -> CheckResult:
        from config.settings import get_config

        config = get_config()

        paths = {
            "CANoe": config.get('software.canoe_path', ''),
            "TSMaster": config.get('software.tsmaster_path', ''),
            "TTworkbench": config.get('software.ttman_path', '')
        }

        details = {
            "configured_paths": paths,
            "existing_paths": {}
        }

        for name, path in paths.items():
            details["existing_paths"][name] = os.path.exists(path) if path else False

        missing = [name for name, exists in details["existing_paths"].items() if not exists]

        if missing:
            return self.create_result(
                status="warning",
                message=f"以下软件路径不存在: {', '.join(missing)}",
                details=details,
                suggestions=["请在配置页面设置正确的软件路径"]
            )

        return self.create_result(
            status="passed",
            message="所有软件路径已配置且存在",
            details=details
        )


@register_check
class ConfigWebSocketCheck(BaseCheck):
    """WebSocket配置检查"""
    id = "CFG-004"
    name = "WebSocket配置检查"
    description = "检查WebSocket配置"
    category = "config"
    category_name = "配置"
    quick_check = False

    def execute(self) -> CheckResult:
        from config.settings import get_config

        config = get_config()

        ws_enabled = config.get('websocket.enabled', False)
        ws_url = config.get('server.websocket_url', '')

        details = {
            "enabled": ws_enabled,
            "websocket_url": ws_url
        }

        if ws_enabled and not ws_url:
            return self.create_result(
                status="warning",
                message="WebSocket已启用但未配置服务器地址",
                details=details,
                suggestions=["请在配置页面设置WebSocket服务器地址"]
            )

        if ws_enabled:
            return self.create_result(
                status="passed",
                message=f"WebSocket已启用，服务器: {ws_url}",
                details=details
            )
        else:
            return self.create_result(
                status="passed",
                message="WebSocket未启用",
                details=details
            )


@register_check
class ConfigReportCheck(BaseCheck):
    """上报配置检查"""
    id = "CFG-005"
    name = "上报配置检查"
    description = "检查上报服务器配置"
    category = "config"
    category_name = "配置"
    quick_check = False

    def execute(self) -> CheckResult:
        from config.settings import get_config

        config = get_config()

        report_enabled = config.get('report.enabled', False)
        report_url = config.get('report.server_url', '')

        details = {
            "enabled": report_enabled,
            "server_url": report_url
        }

        if report_enabled and not report_url:
            return self.create_result(
                status="warning",
                message="上报功能已启用但未配置服务器地址",
                details=details,
                suggestions=["请在配置页面设置上报服务器地址"]
            )

        if report_enabled:
            return self.create_result(
                status="passed",
                message=f"上报功能已启用，服务器: {report_url}",
                details=details
            )
        else:
            return self.create_result(
                status="passed",
                message="上报功能未启用",
                details=details
            )


@register_check
class ConfigCacheCheck(BaseCheck):
    """缓存配置检查"""
    id = "CFG-006"
    name = "缓存配置检查"
    description = "检查缓存配置"
    category = "config"
    category_name = "配置"
    quick_check = False

    def execute(self) -> CheckResult:
        from config.settings import get_config

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

            return self.create_result(
                status="passed",
                message=f"缓存已启用，目录: {cache_dir}",
                details=details
            )
        else:
            return self.create_result(
                status="passed",
                message="缓存已禁用",
                details=details
            )