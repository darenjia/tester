"""
用例映射管理器 - 统一管理接口用例名称与脚本Case编号的映射关系
"""
import os
import sys
import json
import threading
from typing import Dict, Any, List, Optional
from datetime import datetime

from models.case_mapping import CaseMapping, CaseChangeRecord, ChangeType
from utils.logger import get_logger

logger = get_logger("case_mapping_manager")


class CaseMappingManager:
    """用例映射管理器

    统一管理用例映射的CRUD操作和变更记录追踪
    """

    _instance = None
    _lock = threading.Lock()

    def __new__(cls, *args, **kwargs):
        if cls._instance is None:
            with cls._lock:
                if cls._instance is None:
                    cls._instance = super().__new__(cls)
        return cls._instance

    def __init__(self, storage_path: str = None):
        if hasattr(self, '_initialized'):
            return
        self._initialized = True

        if storage_path is None:
            if getattr(sys, 'frozen', False):
                base_dir = os.path.dirname(sys.executable)
            else:
                base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
            storage_path = os.path.join(base_dir, 'data', 'case_mappings.json')

        self.storage_path = storage_path
        self.mappings: Dict[str, CaseMapping] = {}
        self.change_history: List[CaseChangeRecord] = []
        self._data_lock = threading.Lock()
        self._ensure_storage_dir()
        self._load()

    def _ensure_storage_dir(self):
        """确保存储目录存在"""
        storage_dir = os.path.dirname(self.storage_path)
        if storage_dir and not os.path.exists(storage_dir):
            os.makedirs(storage_dir, exist_ok=True)

    def _load(self):
        """从文件加载数据"""
        try:
            if os.path.exists(self.storage_path):
                with open(self.storage_path, 'r', encoding='utf-8') as f:
                    data = json.load(f)

                self.mappings.clear()
                for case_no, mapping_data in data.get('mappings', {}).items():
                    self.mappings[case_no] = CaseMapping.from_dict(mapping_data)

                self.change_history.clear()
                for record_data in data.get('change_history', []):
                    self.change_history.append(CaseChangeRecord.from_dict(record_data))

                logger.info(f"加载了 {len(self.mappings)} 个用例映射，{len(self.change_history)} 条变更记录")
            else:
                logger.info("未找到映射文件，将使用默认数据")
                self._init_default_mappings()
        except Exception as e:
            logger.error(f"加载映射数据失败: {e}")
            self._init_default_mappings()

    def _save(self):
        """保存数据到文件"""
        try:
            with self._data_lock:
                data = {
                    "mappings": {case_no: mapping.to_dict() for case_no, mapping in self.mappings.items()},
                    "change_history": [record.to_dict() for record in self.change_history],
                    "version": "1.0",
                    "last_updated": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
                }

                with open(self.storage_path, 'w', encoding='utf-8') as f:
                    json.dump(data, f, ensure_ascii=False, indent=2)

                logger.debug("用例映射数据已保存")
        except Exception as e:
            logger.error(f"保存映射数据失败: {e}")

    def _init_default_mappings(self):
        """初始化空映射表

        用例映射需要用户手动导入或从任务同步，不再从 FunctionalTestRunner 初始化
        （FunctionalTestRunner 是环境检测用例，不是任务执行用例）
        """
        self.mappings.clear()
        self._save()
        logger.info("初始化空用例映射表，请通过导入或同步功能添加映射")

    def _add_change_record(self, record: CaseChangeRecord):
        """添加变更记录"""
        self.change_history.insert(0, record)
        max_history = 1000
        if len(self.change_history) > max_history:
            self.change_history = self.change_history[:max_history]

    def register_mapping(self, mapping: CaseMapping, changed_by: str = "system",
                        change_reason: str = "初始创建") -> bool:
        """注册/更新用例映射

        Args:
            mapping: 用例映射对象
            changed_by: 变更人
            change_reason: 变更原因

        Returns:
            bool: 是否成功
        """
        try:
            with self._data_lock:
                is_new = mapping.case_no not in self.mappings
                old_value = None
                change_type = ChangeType.ADD.value if is_new else ChangeType.MODIFY.value

                if not is_new:
                    old_value = self.mappings[mapping.case_no].to_dict()

                mapping.updated_at = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
                self.mappings[mapping.case_no] = mapping

                record = CaseChangeRecord(
                    case_no=mapping.case_no,
                    change_type=change_type,
                    old_value=old_value,
                    new_value=mapping.to_dict(),
                    change_reason=change_reason,
                    changed_by=changed_by
                )
                self._add_change_record(record)

            self._save()
            logger.info(f"{'创建' if is_new else '更新'}用例映射: {mapping.case_no}")
            return True
        except Exception as e:
            logger.error(f"注册用例映射失败: {e}")
            return False

    def get_mapping(self, case_no: str) -> Optional[CaseMapping]:
        """根据case_no获取映射

        Args:
            case_no: Case编号

        Returns:
            CaseMapping或None
        """
        return self.mappings.get(case_no)

    def get_mapping_by_name(self, case_name: str) -> Optional[CaseMapping]:
        """根据用例名称获取映射（模糊匹配）

        Args:
            case_name: 用例名称

        Returns:
            CaseMapping或None
        """
        case_name_lower = case_name.lower()
        for mapping in self.mappings.values():
            if case_name_lower in mapping.case_name.lower():
                return mapping
        return None

    def list_mappings(self, category: str = None, enabled_only: bool = True,
                     search: str = None, tags: List[str] = None) -> List[CaseMapping]:
        """列出映射，支持过滤

        Args:
            category: 分类过滤
            enabled_only: 只返回启用的
            search: 搜索关键字（case_no或case_name）
            tags: 标签过滤

        Returns:
            符合条件的映射列表
        """
        results = []

        for mapping in self.mappings.values():
            if category and mapping.category != category:
                continue
            if enabled_only and not mapping.enabled:
                continue
            if search:
                search_lower = search.lower()
                if search_lower not in mapping.case_no.lower() and \
                   search_lower not in mapping.case_name.lower():
                    continue
            if tags:
                if not any(tag in mapping.tags for tag in tags):
                    continue

            results.append(mapping)

        results.sort(key=lambda x: (x.category, x.priority, x.case_no))
        return results

    def list_categories(self) -> List[Dict[str, Any]]:
        """列出所有分类统计信息

        Returns:
            分类信息列表
        """
        categories = {}
        for mapping in self.mappings.values():
            if mapping.category not in categories:
                categories[mapping.category] = {
                    "category": mapping.category,
                    "category_name": mapping.module,
                    "total_count": 0,
                    "enabled_count": 0
                }
            categories[mapping.category]["total_count"] += 1
            if mapping.enabled:
                categories[mapping.category]["enabled_count"] += 1

        return list(categories.values())

    def enable_case(self, case_no: str, changed_by: str = "system",
                   reason: str = "") -> bool:
        """启用用例

        Args:
            case_no: Case编号
            changed_by: 变更人
            reason: 变更原因

        Returns:
            bool: 是否成功
        """
        mapping = self.get_mapping(case_no)
        if not mapping:
            logger.warning(f"用例映射不存在: {case_no}")
            return False

        if mapping.enabled:
            return True

        old_value = mapping.to_dict()
        mapping.enabled = True
        mapping.updated_at = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

        record = CaseChangeRecord(
            case_no=case_no,
            change_type=ChangeType.ENABLE.value,
            old_value=old_value,
            new_value=mapping.to_dict(),
            change_reason=reason or "启用用例",
            changed_by=changed_by
        )
        self._add_change_record(record)
        self._save()

        logger.info(f"启用用例: {case_no}")
        return True

    def disable_case(self, case_no: str, changed_by: str = "system",
                    reason: str = "") -> bool:
        """禁用用例

        Args:
            case_no: Case编号
            changed_by: 变更人
            reason: 变更原因

        Returns:
            bool: 是否成功
        """
        mapping = self.get_mapping(case_no)
        if not mapping:
            logger.warning(f"用例映射不存在: {case_no}")
            return False

        if not mapping.enabled:
            return True

        old_value = mapping.to_dict()
        mapping.enabled = False
        mapping.updated_at = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

        record = CaseChangeRecord(
            case_no=case_no,
            change_type=ChangeType.DISABLE.value,
            old_value=old_value,
            new_value=mapping.to_dict(),
            change_reason=reason or "禁用用例",
            changed_by=changed_by
        )
        self._add_change_record(record)
        self._save()

        logger.info(f"禁用用例: {case_no}")
        return True

    def update_case(self, case_no: str, updates: Dict[str, Any],
                   changed_by: str = "system", reason: str = "") -> bool:
        """更新用例信息

        Args:
            case_no: Case编号
            updates: 更新字段字典
            changed_by: 变更人
            reason: 变更原因

        Returns:
            bool: 是否成功
        """
        mapping = self.get_mapping(case_no)
        if not mapping:
            logger.warning(f"用例映射不存在: {case_no}")
            return False

        old_value = mapping.to_dict()

        allowed_fields = ['case_name', 'category', 'module', 'script_path',
                        'ini_config', 'enabled', 'priority', 'tags', 'version', 'description']
        for key, value in updates.items():
            if key in allowed_fields and hasattr(mapping, key):
                setattr(mapping, key, value)

        mapping.updated_at = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

        record = CaseChangeRecord(
            case_no=case_no,
            change_type=ChangeType.MODIFY.value,
            old_value=old_value,
            new_value=mapping.to_dict(),
            change_reason=reason or "更新用例信息",
            changed_by=changed_by
        )
        self._add_change_record(record)
        self._save()

        logger.info(f"更新用例: {case_no}")
        return True

    def delete_case(self, case_no: str, changed_by: str = "system",
                   reason: str = "") -> bool:
        """删除用例

        Args:
            case_no: Case编号
            changed_by: 变更人
            reason: 变更原因

        Returns:
            bool: 是否成功
        """
        mapping = self.get_mapping(case_no)
        if not mapping:
            logger.warning(f"用例映射不存在: {case_no}")
            return False

        old_value = mapping.to_dict()
        del self.mappings[case_no]

        record = CaseChangeRecord(
            case_no=case_no,
            change_type=ChangeType.DELETE.value,
            old_value=old_value,
            new_value=None,
            change_reason=reason or "删除用例",
            changed_by=changed_by
        )
        self._add_change_record(record)
        self._save()

        logger.info(f"删除用例: {case_no}")
        return True

    def get_change_history(self, case_no: str = None,
                          limit: int = 100) -> List[CaseChangeRecord]:
        """获取变更历史

        Args:
            case_no: Case编号过滤（可选）
            limit: 返回记录数限制

        Returns:
            变更记录列表
        """
        history = self.change_history

        if case_no:
            history = [r for r in history if r.case_no == case_no]

        return history[:limit]

    def export_mappings(self) -> Dict[str, Any]:
        """导出演射为字典格式

        Returns:
            导出数据字典
        """
        return {
            "mappings": [mapping.to_dict() for mapping in self.mappings.values()],
            "categories": self.list_categories(),
            "total_count": len(self.mappings),
            "export_time": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        }

    def import_mappings(self, data: Dict[str, Any],
                      overwrite: bool = True) -> Dict[str, int]:
        """导入映射

        Args:
            data: 导入数据字典
            overwrite: 是否覆盖已存在的映射

        Returns:
            导入统计 {"success": x, "skipped": y, "failed": z}
        """
        stats = {"success": 0, "skipped": 0, "failed": 0}

        mappings_data = data.get('mappings', [])
        if isinstance(mappings_data, dict):
            mappings_data = list(mappings_data.values())
        elif not isinstance(mappings_data, list):
            mappings_data = [mappings_data]

        for mapping_data in mappings_data:
            try:
                mapping = CaseMapping.from_dict(mapping_data)

                if mapping.case_no in self.mappings:
                    if overwrite:
                        self.mappings[mapping.case_no] = mapping
                        stats["success"] += 1
                    else:
                        stats["skipped"] += 1
                else:
                    self.mappings[mapping.case_no] = mapping
                    stats["success"] += 1

            except Exception as e:
                logger.error(f"导入映射失败: {e}")
                stats["failed"] += 1

        if stats["success"] > 0:
            self._save()

        return stats

    def batch_enable(self, case_nos: List[str], changed_by: str = "system",
                     reason: str = "批量启用") -> Dict[str, int]:
        """批量启用用例

        Args:
            case_nos: Case编号列表
            changed_by: 变更人
            reason: 变更原因

        Returns:
            操作统计
        """
        stats = {"success": 0, "failed": 0}
        for case_no in case_nos:
            if self.enable_case(case_no, changed_by, reason):
                stats["success"] += 1
            else:
                stats["failed"] += 1
        return stats

    def batch_disable(self, case_nos: List[str], changed_by: str = "system",
                     reason: str = "批量禁用") -> Dict[str, int]:
        """批量禁用用例

        Args:
            case_nos: Case编号列表
            changed_by: 变更人
            reason: 变更原因

        Returns:
            操作统计
        """
        stats = {"success": 0, "failed": 0}
        for case_no in case_nos:
            if self.disable_case(case_no, changed_by, reason):
                stats["success"] += 1
            else:
                stats["failed"] += 1
        return stats

    def get_statistics(self) -> Dict[str, Any]:
        """获取映射统计信息

        Returns:
            统计信息字典
        """
        total = len(self.mappings)
        enabled = sum(1 for m in self.mappings.values() if m.enabled)
        disabled = total - enabled

        categories = {}
        for mapping in self.mappings.values():
            if mapping.category not in categories:
                categories[mapping.category] = 0
            categories[mapping.category] += 1

        return {
            "total": total,
            "enabled": enabled,
            "disabled": disabled,
            "categories": categories,
            "history_count": len(self.change_history)
        }


_case_mapping_manager: Optional[CaseMappingManager] = None


def get_case_mapping_manager() -> CaseMappingManager:
    """获取用例映射管理器单例

    Returns:
        CaseMappingManager实例
    """
    global _case_mapping_manager
    if _case_mapping_manager is None:
        _case_mapping_manager = CaseMappingManager()
    return _case_mapping_manager
