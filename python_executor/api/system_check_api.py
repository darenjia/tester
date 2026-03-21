"""
系统检测 API
统一的检测接口
"""
from flask import Blueprint, request, jsonify

from core.system_check.registry import check_registry
from core.system_check.executor import get_executor, CheckExecutor
from core.system_check.history import CheckHistory

# 创建蓝图
system_check_bp = Blueprint('system_check', __name__, url_prefix='/api/system-check')

# 历史记录管理器
_history: CheckHistory = None


def get_history() -> CheckHistory:
    """获取历史记录管理器"""
    global _history
    if _history is None:
        import os
        history_dir = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(__file__))), 'data')
        _history = CheckHistory(history_dir=history_dir)
    return _history


@system_check_bp.route('/categories', methods=['GET'])
def get_categories():
    """获取检测分类"""
    try:
        categories = check_registry.get_categories()
        return jsonify({
            "success": True,
            "data": categories
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "message": str(e)
        }), 500


@system_check_bp.route('/checks', methods=['GET'])
def get_all_checks():
    """获取所有检测项"""
    try:
        category = request.args.get('category')
        if category:
            definitions = check_registry.get_definitions_by_category(category)
        else:
            definitions = check_registry.get_all_definitions()

        return jsonify({
            "success": True,
            "data": [d.to_dict() for d in definitions]
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "message": str(e)
        }), 500


@system_check_bp.route('/quick', methods=['POST'])
def execute_quick():
    """执行快速检测"""
    try:
        executor = get_executor()

        if executor.is_running:
            return jsonify({
                "success": False,
                "message": "检测正在执行中"
            }), 400

        session = executor.execute_quick()

        return jsonify({
            "success": True,
            "data": session.to_dict()
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "message": str(e)
        }), 500


@system_check_bp.route('/execute', methods=['POST'])
def execute_checks():
    """执行指定的检测项"""
    try:
        data = request.get_json() or {}
        check_ids = data.get('check_ids', [])

        if not check_ids:
            return jsonify({
                "success": False,
                "message": "请指定要执行的检测项"
            }), 400

        executor = get_executor()

        if executor.is_running:
            return jsonify({
                "success": False,
                "message": "检测正在执行中"
            }), 400

        session = executor.execute_checks(check_ids)

        return jsonify({
            "success": True,
            "data": session.to_dict()
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "message": str(e)
        }), 500


@system_check_bp.route('/execute-category', methods=['POST'])
def execute_category():
    """执行指定类别的检测"""
    try:
        data = request.get_json() or {}
        category = data.get('category')

        if not category:
            return jsonify({
                "success": False,
                "message": "请指定检测类别"
            }), 400

        executor = get_executor()

        if executor.is_running:
            return jsonify({
                "success": False,
                "message": "检测正在执行中"
            }), 400

        session = executor.execute_category(category)

        return jsonify({
            "success": True,
            "data": session.to_dict()
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "message": str(e)
        }), 500


@system_check_bp.route('/execute-all', methods=['POST'])
def execute_all():
    """执行所有检测"""
    try:
        executor = get_executor()

        if executor.is_running:
            return jsonify({
                "success": False,
                "message": "检测正在执行中"
            }), 400

        session = executor.execute_all()

        return jsonify({
            "success": True,
            "data": session.to_dict()
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "message": str(e)
        }), 500


@system_check_bp.route('/status', methods=['GET'])
def get_status():
    """获取执行状态"""
    try:
        executor = get_executor()
        status = executor.get_status()

        return jsonify({
            "success": True,
            "data": status
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "message": str(e)
        }), 500


@system_check_bp.route('/history', methods=['GET'])
def get_history_list():
    """获取历史记录"""
    try:
        limit = request.args.get('limit', 20, type=int)
        mode = request.args.get('mode')

        history = get_history()
        items = history.get_history(limit=limit, mode=mode)

        return jsonify({
            "success": True,
            "data": items
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "message": str(e)
        }), 500


@system_check_bp.route('/history/<session_id>', methods=['GET'])
def get_history_detail(session_id):
    """获取历史详情"""
    try:
        history = get_history()
        session = history.get_session(session_id)

        if not session:
            return jsonify({
                "success": False,
                "message": "历史记录不存在"
            }), 404

        return jsonify({
            "success": True,
            "data": session.to_dict()
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "message": str(e)
        }), 500


@system_check_bp.route('/history/<session_id>', methods=['DELETE'])
def delete_history(session_id):
    """删除历史记录"""
    try:
        history = get_history()
        success = history.delete_session(session_id)

        if not success:
            return jsonify({
                "success": False,
                "message": "历史记录不存在"
            }), 404

        return jsonify({
            "success": True,
            "message": "删除成功"
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "message": str(e)
        }), 500


@system_check_bp.route('/history', methods=['DELETE'])
def clear_history():
    """清空历史记录"""
    try:
        history = get_history()
        history.clear_history()

        return jsonify({
            "success": True,
            "message": "历史记录已清空"
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "message": str(e)
        }), 500


@system_check_bp.route('/stats', methods=['GET'])
def get_stats():
    """获取统计信息"""
    try:
        history = get_history()
        stats = history.get_stats()

        return jsonify({
            "success": True,
            "data": stats
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "message": str(e)
        }), 500


def register_system_check_api(app, skip_blueprint=False):
    """注册系统检测 API"""
    if not skip_blueprint:
        app.register_blueprint(system_check_bp)

    # 注册检测项（导入检测项模块触发注册）
    from core.system_check.checks import system, canoe, tsmaster, ttworkbench, config