"""
输入验证工具
用于生产环境的数据验证和清理
"""
import re
import os
from pathlib import Path
from typing import Any, Dict, List, Optional, Union
from urllib.parse import urlparse

class ValidationError(Exception):
    """验证错误"""
    pass

class InputValidator:
    """输入验证器"""
    
    # 允许的文件扩展名
    ALLOWED_CONFIG_EXTENSIONS = {'.cfg', '.xml', '.json', '.dbc', '.ldf'}
    
    # 危险路径模式
    DANGEROUS_PATH_PATTERNS = [
        r'\.\.',  # 上级目录
        r'[~%]',   # 特殊字符
        r'\0',     # 空字符
    ]
    
    @staticmethod
    def validate_task_id(task_id: str) -> str:
        """验证任务ID"""
        if not task_id:
            raise ValidationError("任务ID不能为空")
        
        if len(task_id) > 128:
            raise ValidationError("任务ID长度不能超过128字符")
        
        # 只允许字母、数字、下划线、连字符
        if not re.match(r'^[a-zA-Z0-9_-]+$', task_id):
            raise ValidationError("任务ID只能包含字母、数字、下划线和连字符")
        
        return task_id
    
    @staticmethod
    def validate_device_id(device_id: str) -> str:
        """验证设备ID"""
        if not device_id:
            raise ValidationError("设备ID不能为空")
        
        if len(device_id) > 64:
            raise ValidationError("设备ID长度不能超过64字符")
        
        return device_id
    
    @staticmethod
    def validate_tool_type(tool_type: str) -> str:
        """验证工具类型"""
        allowed_types = {'canoe', 'tsmaster'}
        
        if not tool_type:
            raise ValidationError("工具类型不能为空")
        
        tool_type_lower = tool_type.lower()
        if tool_type_lower not in allowed_types:
            raise ValidationError(f"不支持的工具类型: {tool_type}，支持: {allowed_types}")
        
        return tool_type_lower
    
    @staticmethod
    def validate_config_path(config_path: str, base_dir: Optional[str] = None) -> str:
        """
        验证配置文件路径
        
        Args:
            config_path: 配置文件路径
            base_dir: 基础目录，用于限制访问范围
        """
        if not config_path:
            raise ValidationError("配置文件路径不能为空")
        
        # 检查危险路径模式
        for pattern in InputValidator.DANGEROUS_PATH_PATTERNS:
            if re.search(pattern, config_path):
                raise ValidationError(f"配置文件路径包含非法字符: {config_path}")
        
        # 转换为绝对路径
        path = Path(config_path).resolve()
        
        # 如果指定了基础目录，检查路径是否在范围内
        if base_dir:
            base_path = Path(base_dir).resolve()
            try:
                path.relative_to(base_path)
            except ValueError:
                raise ValidationError(
                    f"配置文件路径超出允许范围: {config_path}"
                )
        
        # 检查文件扩展名
        if path.suffix.lower() not in InputValidator.ALLOWED_CONFIG_EXTENSIONS:
            raise ValidationError(
                f"不支持的配置文件类型: {path.suffix}，"
                f"支持: {InputValidator.ALLOWED_CONFIG_EXTENSIONS}"
            )
        
        # 检查文件是否存在（可选，因为可能在执行时才创建）
        # if not path.exists():
        #     raise ValidationError(f"配置文件不存在: {config_path}")
        
        return str(path)
    
    @staticmethod
    def validate_signal_name(signal_name: str) -> str:
        """验证信号名称"""
        if not signal_name:
            raise ValidationError("信号名称不能为空")
        
        if len(signal_name) > 256:
            raise ValidationError("信号名称长度不能超过256字符")
        
        # 检查非法字符
        if re.search(r'[<>&"\']', signal_name):
            raise ValidationError("信号名称包含非法字符")
        
        return signal_name
    
    @staticmethod
    def validate_test_item(item: Dict[str, Any]) -> Dict[str, Any]:
        """验证测试项"""
        if not isinstance(item, dict):
            raise ValidationError("测试项必须是字典类型")

        name = item.get('name')
        case_no = item.get('case_no') or item.get('caseNo')

        if not name and case_no:
            name = case_no
            item['name'] = name

        if not name:
            raise ValidationError("测试项名称不能为空（name和case_no都为空）")

        if len(str(name)) > 256:
            raise ValidationError("测试项名称长度不能超过256字符")
        
        # 验证类型
        item_type = item.get('type')
        allowed_types = {'signal_check', 'signal_set', 'test_module'}
        if item_type not in allowed_types:
            raise ValidationError(f"不支持的测试项类型: {item_type}，支持: {allowed_types}")
        
        # 根据类型验证特定字段
        if item_type in ('signal_check', 'signal_set'):
            signal_name = item.get('signalName')
            if not signal_name:
                raise ValidationError(f"测试项 '{name}' 缺少信号名称")
            InputValidator.validate_signal_name(signal_name)
        
        return item
    
    @staticmethod
    def validate_timeout(timeout: Any, default: int = 3600, max_timeout: int = 86400) -> int:
        """验证超时时间"""
        try:
            timeout_int = int(timeout)
        except (TypeError, ValueError):
            return default
        
        if timeout_int <= 0:
            raise ValidationError("超时时间必须大于0")
        
        if timeout_int > max_timeout:
            raise ValidationError(f"超时时间不能超过{max_timeout}秒")
        
        return timeout_int
    
    @staticmethod
    def sanitize_string(value: str, max_length: int = 1024) -> str:
        """清理字符串输入"""
        if not isinstance(value, str):
            return str(value)[:max_length]
        
        # 移除控制字符
        sanitized = ''.join(char for char in value if ord(char) >= 32 or char == '\n')
        
        # 截断长度
        return sanitized[:max_length]
    
    @staticmethod
    def validate_task_data(task_data: Dict[str, Any]) -> Dict[str, Any]:
        """
        验证完整的任务数据
        
        Returns:
            验证和清理后的任务数据
        """
        if not isinstance(task_data, dict):
            raise ValidationError("任务数据必须是字典类型")
        
        validated_data = {}
        
        # 验证任务ID
        task_no = task_data.get('taskNo')
        if task_no:
            validated_data['taskNo'] = InputValidator.validate_task_id(str(task_no))
        else:
            raise ValidationError("任务ID不能为空")
        
        # 验证设备ID
        device_id = task_data.get('deviceId')
        if device_id:
            validated_data['deviceId'] = InputValidator.validate_device_id(str(device_id))
        
        # 验证工具类型
        tool_type = task_data.get('toolType')
        if tool_type:
            validated_data['toolType'] = InputValidator.validate_tool_type(str(tool_type))
        else:
            raise ValidationError("工具类型不能为空")
        
        # 验证配置文件路径（可选，当使用configName+baseConfigDir或直接传testItems时）
        config_path = task_data.get('configPath')
        if config_path and str(config_path).strip():
            validated_data['configPath'] = InputValidator.validate_config_path(str(config_path))
        # configPath为空时允许通过，使用configName或直接传testItems的场景
        
        # 验证测试项
        test_items = task_data.get('testItems', [])
        if not isinstance(test_items, list):
            raise ValidationError("测试项必须是列表类型")
        
        validated_items = []
        for i, item in enumerate(test_items):
            try:
                validated_item = InputValidator.validate_test_item(item)
                validated_items.append(validated_item)
            except ValidationError as e:
                raise ValidationError(f"测试项 {i+1} 验证失败: {e}")
        
        validated_data['testItems'] = validated_items
        
        # 验证超时时间
        timeout = task_data.get('timeout', 3600)
        validated_data['timeout'] = InputValidator.validate_timeout(timeout)
        
        # 清理其他字段
        for key, value in task_data.items():
            if key not in validated_data:
                if isinstance(value, str):
                    validated_data[key] = InputValidator.sanitize_string(value)
                else:
                    validated_data[key] = value
        
        return validated_data

class SecurityValidator:
    """安全验证器"""
    
    @staticmethod
    def validate_ip_address(ip: str) -> bool:
        """验证IP地址格式"""
        import ipaddress
        try:
            ipaddress.ip_address(ip)
            return True
        except ValueError:
            return False
    
    @staticmethod
    def validate_port(port: int) -> bool:
        """验证端口号"""
        return isinstance(port, int) and 1 <= port <= 65535
    
    @staticmethod
    def is_safe_path(path: str, allowed_base_paths: List[str]) -> bool:
        """检查路径是否在允许的范围内"""
        try:
            target_path = Path(path).resolve()
            for base_path in allowed_base_paths:
                base = Path(base_path).resolve()
                try:
                    target_path.relative_to(base)
                    return True
                except ValueError:
                    continue
            return False
        except Exception:
            return False