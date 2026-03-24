"""
报告重试API
提供失败报告管理和重试相关的RESTful接口
"""
from flask import Blueprint, request, jsonify
from typing import Dict, Any

from core.failed_report_manager import get_failed_report_manager
from core.report_retry_scheduler import get_report_retry_scheduler
from utils.logger import get_logger

logger = get_logger("report_retry_api")

report_retry_bp = Blueprint('report_retry', __name__, url_prefix='/api/report-retry')


def get_manager():
    """获取失败报告管理器实例"""
    return get_failed_report_manager()


def get_scheduler():
    """获取重试调度器实例"""
    return get_report_retry_scheduler()


@report_retry_bp.route('/status', methods=['GET'])
def get_status():
    """
    获取上报队列状态

    返回:
    - total: 总报告数
    - pending: 待重试数
    - failed: 已失败数
    - success: 已成功数
    - retrying: 重试中数
    """
    try:
        manager = get_manager()
        stats = manager.get_statistics()

        return jsonify({
            "success": True,
            "data": stats
        })

    except Exception as e:
        logger.error(f"获取上报状态失败: {e}")
        return jsonify({"success": False, "message": f"获取状态失败: {str(e)}"}), 500


@report_retry_bp.route('/list', methods=['GET'])
def list_reports():
    """
    获取失败报告列表

    查询参数:
    - status: 状态过滤 (pending/failed/success/retrying)
    - page: 页码，默认1
    - page_size: 每页数量，默认20
    """
    try:
        manager = get_manager()

        status = request.args.get('status')
        page = int(request.args.get('page', 1))
        page_size = int(request.args.get('page_size', 20))

        offset = (page - 1) * page_size
        reports = manager.list_reports(status=status, limit=page_size, offset=offset)

        # 获取总数
        stats = manager.get_statistics()
        if status:
            total = stats.get(status, 0)
        else:
            total = stats.get('total', 0)

        return jsonify({
            "success": True,
            "data": {
                "reports": [r.to_dict() for r in reports],
                "pagination": {
                    "total": total,
                    "page": page,
                    "page_size": page_size,
                    "total_pages": (total + page_size - 1) // page_size if page_size > 0 else 0
                },
                "statistics": stats
            }
        })

    except Exception as e:
        logger.error(f"获取报告列表失败: {e}")
        return jsonify({"success": False, "message": f"获取列表失败: {str(e)}"}), 500


@report_retry_bp.route('/<report_id>', methods=['GET'])
def get_report(report_id: str):
    """
    获取单个报告详情
    """
    try:
        manager = get_manager()
        report = manager.get_report(report_id)

        if not report:
            return jsonify({"success": False, "message": "报告不存在"}), 404

        return jsonify({
            "success": True,
            "data": report.to_dict()
        })

    except Exception as e:
        logger.error(f"获取报告详情失败: {e}")
        return jsonify({"success": False, "message": f"获取报告失败: {str(e)}"}), 500


@report_retry_bp.route('/<report_id>/retry', methods=['POST'])
def retry_report(report_id: str):
    """
    手动重试单个报告
    """
    try:
        scheduler = get_scheduler()
        success = scheduler.retry_now(report_id)

        if success:
            return jsonify({
                "success": True,
                "message": f"报告 {report_id} 已触发重试"
            })
        else:
            return jsonify({
                "success": False,
                "message": "报告不存在或无法重试"
            }), 400

    except Exception as e:
        logger.error(f"重试报告失败: {e}")
        return jsonify({"success": False, "message": f"重试失败: {str(e)}"}), 500


@report_retry_bp.route('/retry-all', methods=['POST'])
def retry_all():
    """
    重试所有待重试的报告
    """
    try:
        scheduler = get_scheduler()
        stats = scheduler.retry_all_pending()

        return jsonify({
            "success": True,
            "data": stats,
            "message": f"批量重试完成: 成功 {stats['success']}, 失败 {stats['failed']}"
        })

    except Exception as e:
        logger.error(f"批量重试失败: {e}")
        return jsonify({"success": False, "message": f"批量重试失败: {str(e)}"}), 500


@report_retry_bp.route('/<report_id>', methods=['DELETE'])
def delete_report(report_id: str):
    """
    删除报告记录
    """
    try:
        manager = get_manager()
        success = manager.delete_report(report_id)

        if success:
            return jsonify({
                "success": True,
                "message": f"报告 {report_id} 已删除"
            })
        else:
            return jsonify({
                "success": False,
                "message": "报告不存在"
            }), 404

    except Exception as e:
        logger.error(f"删除报告失败: {e}")
        return jsonify({"success": False, "message": f"删除失败: {str(e)}"}), 500


@report_retry_bp.route('/<report_id>/reset', methods=['POST'])
def reset_report(report_id: str):
    """
    重置报告状态以便重试
    """
    try:
        manager = get_manager()
        success = manager.reset_report_for_retry(report_id)

        if success:
            return jsonify({
                "success": True,
                "message": f"报告 {report_id} 已重置，将在下次检查时重试"
            })
        else:
            return jsonify({
                "success": False,
                "message": "报告不存在或状态不允许重置"
            }), 400

    except Exception as e:
        logger.error(f"重置报告失败: {e}")
        return jsonify({"success": False, "message": f"重置失败: {str(e)}"}), 500


@report_retry_bp.route('/cleanup', methods=['POST'])
def cleanup():
    """
    清理旧的报告记录

    请求体:
    - days: 保留天数，默认7
    """
    try:
        manager = get_manager()
        data = request.get_json() or {}
        days = data.get('days', 7)

        count = manager.cleanup_old_reports(days=days)

        return jsonify({
            "success": True,
            "message": f"已清理 {count} 条旧报告"
        })

    except Exception as e:
        logger.error(f"清理报告失败: {e}")
        return jsonify({"success": False, "message": f"清理失败: {str(e)}"}), 500