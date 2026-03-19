"""
用例映射API
提供用例映射相关的RESTful接口
"""
import os
from flask import Blueprint, request, jsonify, send_file
from typing import Dict, Any

from core.case_mapping_manager import get_case_mapping_manager
from models.case_mapping import CaseMapping
from utils.excel_parser import CaseMappingExcelParser


case_mapping_bp = Blueprint('case_mapping', __name__, url_prefix='/api/case-mappings')


def get_manager():
    """获取映射管理器实例"""
    return get_case_mapping_manager()


@case_mapping_bp.route('', methods=['GET'])
def list_case_mappings():
    """
    获取用例映射列表

    查询参数:
    - category: 分类过滤
    - enabled: 是否启用 (true/false)
    - search: 搜索关键字
    - tags: 标签过滤 (逗号分隔)
    - page: 页码，默认1
    - page_size: 每页数量，默认50
    """
    try:
        manager = get_manager()

        category = request.args.get('category')
        enabled_str = request.args.get('enabled', 'true').lower()
        enabled_only = enabled_str != 'false'
        search = request.args.get('search')
        tags_str = request.args.get('tags')
        tags = tags_str.split(',') if tags_str else None
        page = int(request.args.get('page', 1))
        page_size = int(request.args.get('page_size', 50))

        mappings = manager.list_mappings(
            category=category,
            enabled_only=enabled_only,
            search=search,
            tags=tags
        )

        total = len(mappings)
        start = (page - 1) * page_size
        end = start + page_size
        paginated_mappings = mappings[start:end]

        return jsonify({
            "success": True,
            "data": {
                "mappings": [m.to_dict() for m in paginated_mappings],
                "categories": manager.list_categories(),
                "pagination": {
                    "total": total,
                    "page": page,
                    "page_size": page_size,
                    "total_pages": (total + page_size - 1) // page_size
                }
            }
        })

    except Exception as e:
        return jsonify({"success": False, "message": f"获取用例映射列表失败: {str(e)}"}), 500


@case_mapping_bp.route('/categories', methods=['GET'])
def list_categories():
    """
    获取所有分类统计信息
    """
    try:
        manager = get_manager()
        categories = manager.list_categories()

        return jsonify({
            "success": True,
            "data": categories
        })

    except Exception as e:
        return jsonify({"success": False, "message": f"获取分类列表失败: {str(e)}"}), 500


@case_mapping_bp.route('/<case_no>', methods=['GET'])
def get_case_mapping(case_no: str):
    """
    获取单个用例映射

    路径参数:
    - case_no: Case编号
    """
    try:
        manager = get_manager()
        mapping = manager.get_mapping(case_no)

        if not mapping:
            return jsonify({"success": False, "message": f"用例映射不存在: {case_no}"}), 404

        return jsonify({
            "success": True,
            "data": mapping.to_dict()
        })

    except Exception as e:
        return jsonify({"success": False, "message": f"获取用例映射失败: {str(e)}"}), 500


@case_mapping_bp.route('', methods=['POST'])
def create_case_mapping():
    """
    创建用例映射

    请求体:
    {
        "case_no": "CANOE-001",           # Case编号 (必填)
        "case_name": "CANoe安装路径检查",   # 用例名称 (必填)
        "category": "canoe",              # 分类 (必填)
        "module": "CANoe测试",             # 模块名称
        "script_path": "core/...",        # 脚本路径
        "enabled": true,                 # 是否启用
        "priority": 0,                   # 优先级
        "tags": ["环境检查"],              # 标签
        "version": "1.0",               # 版本号
        "description": "描述"             # 描述
    }
    """
    try:
        data = request.get_json()
        if not data:
            return jsonify({"success": False, "message": "请求体不能为空"}), 400

        case_no = data.get('case_no')
        case_name = data.get('case_name')
        category = data.get('category')

        if not case_no:
            return jsonify({"success": False, "message": "Case编号不能为空"}), 400
        if not case_name:
            return jsonify({"success": False, "message": "用例名称不能为空"}), 400
        if not category:
            return jsonify({"success": False, "message": "分类不能为空"}), 400

        manager = get_manager()

        if manager.get_mapping(case_no):
            return jsonify({"success": False, "message": f"Case编号已存在: {case_no}"}), 409

        mapping = CaseMapping(
            case_no=case_no,
            case_name=case_name,
            category=category,
            module=data.get('module', ''),
            script_path=data.get('script_path', ''),
            enabled=data.get('enabled', True),
            priority=data.get('priority', 0),
            tags=data.get('tags', []),
            version=data.get('version', '1.0'),
            description=data.get('description', '')
        )

        changed_by = data.get('changed_by', 'system')
        change_reason = data.get('change_reason', '创建用例映射')

        if manager.register_mapping(mapping, changed_by, change_reason):
            return jsonify({
                "success": True,
                "message": "用例映射创建成功",
                "data": mapping.to_dict()
            }), 201
        else:
            return jsonify({"success": False, "message": "创建用例映射失败"}), 500

    except Exception as e:
        return jsonify({"success": False, "message": f"创建用例映射失败: {str(e)}"}), 500


@case_mapping_bp.route('/<case_no>', methods=['PUT'])
def update_case_mapping(case_no: str):
    """
    更新用例映射

    路径参数:
    - case_no: Case编号

    请求体:
    {
        "case_name": "新名称",
        "category": "new_category",
        "module": "新模块",
        "enabled": true,
        "priority": 1,
        "tags": ["新标签"],
        "version": "1.1",
        "description": "新描述"
    }
    """
    try:
        data = request.get_json()
        if not data:
            return jsonify({"success": False, "message": "请求体不能为空"}), 400

        manager = get_manager()
        mapping = manager.get_mapping(case_no)

        if not mapping:
            return jsonify({"success": False, "message": f"用例映射不存在: {case_no}"}), 404

        changed_by = data.get('changed_by', 'system')
        change_reason = data.get('change_reason', '更新用例映射')

        if manager.update_case(case_no, data, changed_by, change_reason):
            updated_mapping = manager.get_mapping(case_no)
            return jsonify({
                "success": True,
                "message": "用例映射更新成功",
                "data": updated_mapping.to_dict()
            })
        else:
            return jsonify({"success": False, "message": "更新用例映射失败"}), 500

    except Exception as e:
        return jsonify({"success": False, "message": f"更新用例映射失败: {str(e)}"}), 500


@case_mapping_bp.route('/<case_no>', methods=['DELETE'])
def delete_case_mapping(case_no: str):
    """
    删除用例映射

    路径参数:
    - case_no: Case编号

    请求体:
    {
        "changed_by": "admin",           # 变更人
        "reason": "删除原因"              # 删除原因
    }
    """
    try:
        data = request.get_json() or {}
        manager = get_manager()

        mapping = manager.get_mapping(case_no)
        if not mapping:
            return jsonify({"success": False, "message": f"用例映射不存在: {case_no}"}), 404

        changed_by = data.get('changed_by', 'system')
        reason = data.get('reason', '删除用例映射')

        if manager.delete_case(case_no, changed_by, reason):
            return jsonify({
                "success": True,
                "message": "用例映射删除成功"
            })
        else:
            return jsonify({"success": False, "message": "删除用例映射失败"}), 500

    except Exception as e:
        return jsonify({"success": False, "message": f"删除用例映射失败: {str(e)}"}), 500


@case_mapping_bp.route('/<case_no>/enable', methods=['POST'])
def enable_case(case_no: str):
    """
    启用用例

    路径参数:
    - case_no: Case编号

    请求体:
    {
        "changed_by": "admin",
        "reason": "启用原因"
    }
    """
    try:
        data = request.get_json() or {}
        manager = get_manager()

        changed_by = data.get('changed_by', 'system')
        reason = data.get('reason', '启用用例')

        if manager.enable_case(case_no, changed_by, reason):
            return jsonify({
                "success": True,
                "message": f"用例已启用: {case_no}"
            })
        else:
            return jsonify({"success": False, "message": "启用用例失败"}), 500

    except Exception as e:
        return jsonify({"success": False, "message": f"启用用例失败: {str(e)}"}), 500


@case_mapping_bp.route('/<case_no>/disable', methods=['POST'])
def disable_case(case_no: str):
    """
    禁用用例

    路径参数:
    - case_no: Case编号

    请求体:
    {
        "changed_by": "admin",
        "reason": "禁用原因"
    }
    """
    try:
        data = request.get_json() or {}
        manager = get_manager()

        changed_by = data.get('changed_by', 'system')
        reason = data.get('reason', '禁用用例')

        if manager.disable_case(case_no, changed_by, reason):
            return jsonify({
                "success": True,
                "message": f"用例已禁用: {case_no}"
            })
        else:
            return jsonify({"success": False, "message": "禁用用例失败"}), 500

    except Exception as e:
        return jsonify({"success": False, "message": f"禁用用例失败: {str(e)}"}), 500


@case_mapping_bp.route('/batch/enable', methods=['POST'])
def batch_enable_cases():
    """
    批量启用用例

    请求体:
    {
        "case_nos": ["CANOE-001", "CANOE-002"],
        "changed_by": "admin",
        "reason": "批量启用原因"
    }
    """
    try:
        data = request.get_json()
        if not data:
            return jsonify({"success": False, "message": "请求体不能为空"}), 400

        case_nos = data.get('case_nos', [])
        if not case_nos:
            return jsonify({"success": False, "message": "Case编号列表不能为空"}), 400

        manager = get_manager()
        changed_by = data.get('changed_by', 'system')
        reason = data.get('reason', '批量启用')

        stats = manager.batch_enable(case_nos, changed_by, reason)

        return jsonify({
            "success": True,
            "message": f"批量启用完成: 成功 {stats['success']}, 失败 {stats['failed']}",
            "data": stats
        })

    except Exception as e:
        return jsonify({"success": False, "message": f"批量启用失败: {str(e)}"}), 500


@case_mapping_bp.route('/batch/disable', methods=['POST'])
def batch_disable_cases():
    """
    批量禁用用例

    请求体:
    {
        "case_nos": ["CANOE-001", "CANOE-002"],
        "changed_by": "admin",
        "reason": "批量禁用原因"
    }
    """
    try:
        data = request.get_json()
        if not data:
            return jsonify({"success": False, "message": "请求体不能为空"}), 400

        case_nos = data.get('case_nos', [])
        if not case_nos:
            return jsonify({"success": False, "message": "Case编号列表不能为空"}), 400

        manager = get_manager()
        changed_by = data.get('changed_by', 'system')
        reason = data.get('reason', '批量禁用')

        stats = manager.batch_disable(case_nos, changed_by, reason)

        return jsonify({
            "success": True,
            "message": f"批量禁用完成: 成功 {stats['success']}, 失败 {stats['failed']}",
            "data": stats
        })

    except Exception as e:
        return jsonify({"success": False, "message": f"批量禁用失败: {str(e)}"}), 500


@case_mapping_bp.route('/history', methods=['GET'])
def get_change_history():
    """
    获取变更历史

    查询参数:
    - case_no: Case编号过滤 (可选)
    - limit: 返回记录数限制，默认100
    """
    try:
        case_no = request.args.get('case_no')
        limit = int(request.args.get('limit', 100))

        manager = get_manager()
        history = manager.get_change_history(case_no, limit)

        return jsonify({
            "success": True,
            "data": {
                "history": [h.to_dict() for h in history],
                "total": len(history)
            }
        })

    except Exception as e:
        return jsonify({"success": False, "message": f"获取变更历史失败: {str(e)}"}), 500


@case_mapping_bp.route('/statistics', methods=['GET'])
def get_statistics():
    """
    获取映射统计信息
    """
    try:
        manager = get_manager()
        stats = manager.get_statistics()

        return jsonify({
            "success": True,
            "data": stats
        })

    except Exception as e:
        return jsonify({"success": False, "message": f"获取统计信息失败: {str(e)}"}), 500


@case_mapping_bp.route('/export', methods=['GET'])
def export_mappings():
    """
    导出演射

    查询参数:
    - format: 导出格式 (json/csv)，默认json
    """
    try:
        manager = get_manager()
        export_data = manager.export_mappings()

        return jsonify({
            "success": True,
            "data": export_data
        })

    except Exception as e:
        return jsonify({"success": False, "message": f"导出演射失败: {str(e)}"}), 500


@case_mapping_bp.route('/import', methods=['POST'])
def import_mappings():
    """
    导入映射

    请求体:
    {
        "mappings": [...],     # 映射数据列表
        "overwrite": true      # 是否覆盖已存在的映射
    }
    """
    try:
        data = request.get_json()
        if not data:
            return jsonify({"success": False, "message": "请求体不能为空"}), 400

        mappings = data.get('mappings')
        if not mappings:
            return jsonify({"success": False, "message": "映射数据不能为空"}), 400

        manager = get_manager()
        overwrite = data.get('overwrite', True)

        stats = manager.import_mappings({"mappings": mappings}, overwrite)

        return jsonify({
            "success": True,
            "message": f"导入完成: 成功 {stats['success']}, 跳过 {stats['skipped']}, 失败 {stats['failed']}",
            "data": stats
        })

    except Exception as e:
        return jsonify({"success": False, "message": f"导入映射失败: {str(e)}"}), 500


@case_mapping_bp.route('/lookup', methods=['GET'])
def lookup_by_name():
    """
    根据用例名称查找映射

    查询参数:
    - name: 用例名称
    """
    try:
        name = request.args.get('name')
        if not name:
            return jsonify({"success": False, "message": "用例名称不能为空"}), 400

        manager = get_manager()
        mapping = manager.get_mapping_by_name(name)

        if mapping:
            return jsonify({
                "success": True,
                "data": mapping.to_dict()
            })
        else:
            return jsonify({
                "success": False,
                "message": f"未找到用例名称包含 '{name}' 的映射"
            }), 404

    except Exception as e:
        return jsonify({"success": False, "message": f"查找映射失败: {str(e)}"}), 500


_temp_files: Dict[str, str] = {}


@case_mapping_bp.route('/import/upload', methods=['POST'])
def upload_excel():
    """
    上传Excel文件并获取表头

    请求: multipart/form-data
        file: Excel文件 (.xlsx, .xls)
        sheet: 工作表索引（可选，默认0）

    返回:
    {
        "success": true,
        "data": {
            "file_id": "temp_xxx",
            "filename": "原始文件名.xlsx",
            "headers": ["列1", "列2", ...],
            "row_count": 100,
            "sheet_names": ["Sheet1", "数据"],
            "current_sheet": 0,
            "preview": [{...}, ...]  // 前5行
        }
    }
    """
    try:
        file = None
        file_id = None
        sheet_index = 0

        if 'file' in request.files:
            file = request.files['file']
            if not file.filename:
                return jsonify({"success": False, "message": "文件名为空"}), 400
            if not file.filename.endswith(('.xlsx', '.xls')):
                return jsonify({"success": False, "message": "只支持 .xlsx 和 .xls 格式"}), 400
            sheet_index = request.form.get('sheet', type=int, default=0)
        elif request.form.get('file_id'):
            file_id = request.form.get('file_id')
            if not os.path.exists(file_id):
                return jsonify({"success": False, "message": "文件不存在或已过期"}), 400
            sheet_index = request.form.get('sheet', type=int, default=0)
        else:
            return jsonify({"success": False, "message": "未上传文件或缺少file_id"}), 400

        if not CaseMappingExcelParser.is_available():
            return jsonify({"success": False, "message": "Excel解析功能不可用，请安装: pip install pandas openpyxl"}), 500

        if file:
            file_id = CaseMappingExcelParser.save_upload_file(file, file.filename)
            _temp_files[file_id] = file_id

        result = CaseMappingExcelParser.parse_file(file_id, sheet_index=sheet_index)
        headers = result['headers']
        row_count = result['row_count']
        sheet_names = result['sheet_names']
        current_sheet = result['current_sheet']

        preview_data = CaseMappingExcelParser.get_preview_data(file_id, max_rows=5, sheet_index=sheet_index)

        return jsonify({
            "success": True,
            "data": {
                "file_id": file_id,
                "filename": file.filename if file else os.path.basename(file_id),
                "headers": headers,
                "row_count": row_count,
                "sheet_names": sheet_names,
                "current_sheet": current_sheet,
                "preview": preview_data
            }
        })

    except Exception as e:
        return jsonify({"success": False, "message": f"上传失败: {str(e)}"}), 500


@case_mapping_bp.route('/import/sheets', methods=['POST'])
def get_excel_sheets():
    """
    获取Excel所有工作表信息

    请求: multipart/form-data
        file: Excel文件 (.xlsx, .xls)

    返回:
    {
        "success": true,
        "data": {
            "file_id": "temp_xxx",
            "filename": "原始文件名.xlsx",
            "sheets": [
                {"index": 0, "name": "Sheet1", "row_count": 100},
                {"index": 1, "name": "数据", "row_count": 50}
            ]
        }
    }
    """
    try:
        file = None
        file_id = None

        if 'file' in request.files:
            file = request.files['file']
        elif request.form.get('file_id'):
            file_id = request.form.get('file_id')

        if file_id and os.path.exists(file_id):
            sheets_info = CaseMappingExcelParser.get_sheets_info(file_id)
            filename = os.path.basename(file_id)
            if filename.startswith('case_mapping_import_'):
                filename = '已上传文件.xlsx'

            return jsonify({
                "success": True,
                "data": {
                    "file_id": file_id,
                    "filename": filename,
                    "sheets": sheets_info
                }
            })

        if not file or not file.filename:
            return jsonify({"success": False, "message": "未上传文件"}), 400

        if not file.filename.endswith(('.xlsx', '.xls')):
            return jsonify({"success": False, "message": "只支持 .xlsx 和 .xls 格式"}), 400

        if not CaseMappingExcelParser.is_available():
            return jsonify({"success": False, "message": "Excel解析功能不可用"}), 500

        file_id = CaseMappingExcelParser.save_upload_file(file, file.filename)
        _temp_files[file_id] = file_id

        sheets_info = CaseMappingExcelParser.get_sheets_info(file_id)

        return jsonify({
            "success": True,
            "data": {
                "file_id": file_id,
                "filename": file.filename,
                "sheets": sheets_info
            }
        })

    except Exception as e:
        return jsonify({"success": False, "message": f"获取工作表信息失败: {str(e)}"}), 500


@case_mapping_bp.route('/import/preview', methods=['POST'])
def preview_import():
    """
    预览Excel导入数据（根据字段映射）

    请求体:
    {
        "file_id": "temp文件路径",
        "column_mapping": {
            "Excel列名1": "case_no",
            "Excel列名2": "case_name",
            ...
        },
        "default_category": "canoe",
        "sheet_index": 0
    }

    返回:
    {
        "success": true,
        "data": {
            "mappings": [...],     // 映射后的数据
            "total": 100,
            "valid": 98,
            "invalid": 2,
            "errors": [{"row": 5, "errors": [...]}, ...]
        }
    }
    """
    try:
        data = request.get_json()
        if not data:
            return jsonify({"success": False, "message": "请求体不能为空"}), 400

        file_id = data.get('file_id')
        column_mapping = data.get('column_mapping', {})
        default_category = data.get('default_category')
        sheet_index = data.get('sheet_index', 0)

        if not file_id:
            return jsonify({"success": False, "message": "file_id不能为空"}), 400

        if not os.path.exists(file_id):
            return jsonify({"success": False, "message": "文件不存在或已过期"}), 400

        valid_mappings, invalid_mappings = CaseMappingExcelParser.apply_mapping(
            file_id, column_mapping, default_category=default_category, sheet_index=sheet_index
        )
        converted_valid = CaseMappingExcelParser.convert_mappings_for_import(valid_mappings)

        errors = [{"row": inv['row'], "errors": inv['errors']} for inv in invalid_mappings]

        return jsonify({
            "success": True,
            "data": {
                "mappings": converted_valid,
                "total": len(valid_mappings) + len(invalid_mappings),
                "valid": len(valid_mappings),
                "invalid": len(invalid_mappings),
                "errors": errors
            }
        })

    except Exception as e:
        return jsonify({"success": False, "message": f"预览失败: {str(e)}"}), 500


@case_mapping_bp.route('/import/execute', methods=['POST'])
def execute_import():
    """
    执行Excel导入

    请求体:
    {
        "file_id": "temp文件路径",
        "column_mapping": {...},
        "default_category": "canoe",
        "sheet_index": 0,
        "overwrite": true,
        "changed_by": "web_user",
        "change_reason": "从Excel批量导入"
    }

    返回:
    {
        "success": true,
        "message": "导入完成: 成功 X, 跳过 Y, 失败 Z",
        "data": {"success": X, "skipped": Y, "failed": Z}
    }
    """
    try:
        data = request.get_json()
        if not data:
            return jsonify({"success": False, "message": "请求体不能为空"}), 400

        file_id = data.get('file_id')
        column_mapping = data.get('column_mapping', {})
        default_category = data.get('default_category')
        sheet_index = data.get('sheet_index', 0)
        overwrite = data.get('overwrite', True)
        changed_by = data.get('changed_by', 'web_user')
        change_reason = data.get('change_reason', '从Excel批量导入')

        if not file_id:
            return jsonify({"success": False, "message": "file_id不能为空"}), 400

        if not os.path.exists(file_id):
            return jsonify({"success": False, "message": "文件不存在或已过期"}), 400

        valid_mappings, invalid_mappings = CaseMappingExcelParser.apply_mapping(
            file_id, column_mapping, default_category=default_category, sheet_index=sheet_index
        )
        converted_valid = CaseMappingExcelParser.convert_mappings_for_import(valid_mappings)

        manager = get_manager()
        stats = manager.import_mappings({"mappings": converted_valid}, overwrite)

        CaseMappingExcelParser.cleanup_temp_file(file_id)
        if file_id in _temp_files:
            del _temp_files[file_id]

        return jsonify({
            "success": True,
            "message": f"导入完成: 成功 {stats['success']}, 跳过 {stats['skipped']}, 失败 {stats['failed']}",
            "data": stats
        })

    except Exception as e:
        return jsonify({"success": False, "message": f"导入失败: {str(e)}"}), 500


@case_mapping_bp.route('/import/template', methods=['GET'])
def download_template():
    """
    下载导入模板Excel文件

    返回: Excel/CSV文件下载
    """
    try:
        template_path = CaseMappingExcelParser.create_template()

        if template_path.endswith('.csv'):
            mimetype = 'text/csv'
            download_name = 'case_mapping_template.csv'
        else:
            mimetype = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
            download_name = 'case_mapping_template.xlsx'

        return send_file(
            template_path,
            mimetype=mimetype,
            as_attachment=True,
            download_name=download_name
        )

    except Exception as e:
        return jsonify({"success": False, "message": f"生成模板失败: {str(e)}"}), 500


@case_mapping_bp.route('/import/fields', methods=['GET'])
def get_import_fields():
    """
    获取导入字段信息（用于前端映射选择）

    返回:
    {
        "success": true,
        "data": {
            "system_fields": [
                {"name": "case_no", "display": "Case编号", "required": true, "hint": "..."},
                ...
            ],
            "valid_categories": ["system", "canoe", ...]
        }
    }
    """
    try:
        fields = []
        for name, display, required in CaseMappingExcelParser.SYSTEM_FIELDS:
            fields.append({
                "name": name,
                "display": display,
                "required": required,
                "hint": CaseMappingExcelParser.FIELD_HINTS.get(name, '')
            })

        return jsonify({
            "success": True,
            "data": {
                "system_fields": fields,
                "valid_categories": CaseMappingExcelParser.VALID_CATEGORIES
            }
        })

    except Exception as e:
        return jsonify({"success": False, "message": f"获取字段信息失败: {str(e)}"}), 500


@case_mapping_bp.route('/sync', methods=['POST'])
def sync_mappings():
    """
    从任务用例同步映射

    请求体:
    {
        "cases": [
            {
                "case_name": "用例名称",
                "case_no": "脚本标识",
                "category": "canoe",
                "module": "模块名称",
                "tool_type": "CANoe",
                "config_path": "配置文件路径",
                "enabled": true
            },
            ...
        ],
        "changed_by": "system",
        "change_reason": "从任务同步"
    }

    返回:
    {
        "success": true,
        "message": "同步完成: 新增 X, 更新 Y",
        "data": {"added": X, "updated": Y}
    }
    """
    try:
        data = request.get_json()
        if not data:
            return jsonify({"success": False, "message": "请求体不能为空"}), 400

        cases = data.get('cases', [])
        if not cases:
            return jsonify({"success": False, "message": "用例列表不能为空"}), 400

        manager = get_manager()
        changed_by = data.get('changed_by', 'system')
        change_reason = data.get('change_reason', '从任务同步')

        added = 0
        updated = 0

        for case_data in cases:
            case_name = case_data.get('case_name', '').strip()
            case_no = case_data.get('case_no', '').strip()

            if not case_name:
                continue

            if not case_no:
                case_no = case_name

            existing = manager.get_mapping_by_name(case_name)

            if existing:
                updates = {
                    'case_no': case_no,
                    'category': case_data.get('category', existing.category),
                    'module': case_data.get('module', existing.module),
                    'enabled': case_data.get('enabled', existing.enabled),
                }
                manager.update_case(existing.case_no, updates, changed_by, change_reason)
                updated += 1
            else:
                mapping = CaseMapping(
                    case_no=case_no,
                    case_name=case_name,
                    category=case_data.get('category', ''),
                    module=case_data.get('module', ''),
                    script_path=case_data.get('config_path', ''),
                    enabled=case_data.get('enabled', True),
                    priority=case_data.get('priority', 0),
                    tags=case_data.get('tags', []),
                    version=case_data.get('version', '1.0'),
                    description=case_data.get('description', '')
                )
                manager.register_mapping(mapping, changed_by, change_reason)
                added += 1

        return jsonify({
            "success": True,
            "message": f"同步完成: 新增 {added}, 更新 {updated}",
            "data": {"added": added, "updated": updated}
        })

    except Exception as e:
        return jsonify({"success": False, "message": f"同步失败: {str(e)}"}), 500
