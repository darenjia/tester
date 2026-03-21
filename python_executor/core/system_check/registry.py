"""
检测项注册表
管理所有可用的检测项
"""
from typing import Dict, List, Type, Optional
from .base import BaseCheck
from .models import CheckDefinition


class CheckRegistry:
    """检测项注册表"""

    def __init__(self):
        self._checks: Dict[str, Type[BaseCheck]] = {}
        self._instances: Dict[str, BaseCheck] = {}

    def register(self, check_class: Type[BaseCheck]) -> Type[BaseCheck]:
        """
        注册检测项

        Args:
            check_class: 检测项类

        Returns:
            注册的类（用于装饰器）
        """
        # 创建临时实例获取 ID
        instance = check_class()
        check_id = instance.id

        if check_id in self._checks:
            raise ValueError(f"检测项 {check_id} 已注册")

        self._checks[check_id] = check_class
        self._instances[check_id] = instance
        return check_class

    def get_check(self, check_id: str) -> Optional[BaseCheck]:
        """获取检测项实例"""
        return self._instances.get(check_id)

    def get_all_checks(self) -> List[BaseCheck]:
        """获取所有检测项"""
        return list(self._instances.values())

    def get_checks_by_category(self, category: str) -> List[BaseCheck]:
        """获取指定类别的检测项"""
        return [c for c in self._instances.values() if c.category == category]

    def get_quick_checks(self) -> List[BaseCheck]:
        """获取快速检测项"""
        return [c for c in self._instances.values() if c.quick_check]

    def get_categories(self) -> List[Dict[str, any]]:
        """获取所有分类"""
        categories = {}
        for check in self._instances.values():
            if check.category not in categories:
                categories[check.category] = {
                    "id": check.category,
                    "name": check.category_name,
                    "count": 0,
                    "quick_check_count": 0
                }
            categories[check.category]["count"] += 1
            if check.quick_check:
                categories[check.category]["quick_check_count"] += 1
        return list(categories.values())

    def get_all_definitions(self) -> List[CheckDefinition]:
        """获取所有检测项定义"""
        return [check.definition for check in self._instances.values()]

    def get_definitions_by_category(self, category: str) -> List[CheckDefinition]:
        """获取指定类别的检测项定义"""
        return [check.definition for check in self._instances.values() if check.category == category]


# 全局注册表实例
check_registry = CheckRegistry()


def register_check(check_class: Type[BaseCheck]) -> Type[BaseCheck]:
    """注册检测项的便捷函数"""
    return check_registry.register(check_class)