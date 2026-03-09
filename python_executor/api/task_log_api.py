"""
任务日志API
提供任务日志相关的RESTful接口
"""
from flask import Blueprint, request, jsonify
from typing import Dict, Any, Optional

from models.task_log import task_log_manager, LogLevel
from models.executor_task import task_queue


# 创建蓝图
task_log_bp = Blueprint('task_log', __name__, url_prefix='/api')


@task_log_bp.route('/tasks/<task_id>/logs', methods=['GET'])
def get_task_logs(task_id: str):
    """
    获取任务执行日志
    
    路径参数:
    - task_id: 任务ID
    
    查询参数:
    - level: 日志级别筛选 (DEBUG/INFO/WARNING/ERROR/CRITICAL)
    - start_time: 开始时间筛选 (ISO格式)
    - end_time: 结束时间筛选 (ISO格式)
    - page: 页码，默认1
    - per_page: 每页数量，默认50
    """
    try:
        # 检查任务是否存在
        task = task_queue.get_task(task_id)
        if not task:
            return jsonify({"success": False, "message": "任务不存在"}), 404
            
        # 获取查询参数
        level = request.args.get('level')
        start_time = request.args.get('start_time')
        end_time = request.args.get('end_time')
        page = int(request.args.get('page', 1))
        per_page = int(request.args.get('per_page', 50))
        
        # 获取日志
        logs = task_log_manager.get_logs_by_task(
            task_id=task_id,
            level=level,
            start_time=start_time,
            end_time=end_time
        )
        
        # 分页
        total = len(logs)
        start = (page - 1) * per_page
        end = start + per_page
        paginated_logs = logs[start:end]
        
        return jsonify({
            "success": True,
            "data": {
                "logs": [log.to_dict() for log in paginated_logs],
                "pagination": {
                    "total": total,
                    "page": page,
                    "per_page": per_page,
                    "total_pages": (total + per_page - 1) // per_page
                }
            }
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取任务日志失败: {str(e)}"}), 500


@task_log_bp.route('/logs', methods=['GET'])
def get_all_logs():
    """
    获取所有日志
    
    查询参数:
    - task_id: 任务ID筛选
    - level: 日志级别筛选
    - start_time: 开始时间筛选
    - end_time: 结束时间筛选
    - page: 页码，默认1
    - per_page: 每页数量，默认50
    """
    try:
        # 获取查询参数
        task_id = request.args.get('task_id')
        level = request.args.get('level')
        start_time = request.args.get('start_time')
        end_time = request.args.get('end_time')
        page = int(request.args.get('page', 1))
        per_page = int(request.args.get('per_page', 50))
        
        # 获取日志
        logs = task_log_manager.get_all_logs(
            task_id=task_id,
            level=level,
            start_time=start_time,
            end_time=end_time
        )
        
        # 分页
        total = len(logs)
        start = (page - 1) * per_page
        end = start + per_page
        paginated_logs = logs[start:end]
        
        return jsonify({
            "success": True,
            "data": {
                "logs": [log.to_dict() for log in paginated_logs],
                "pagination": {
                    "total": total,
                    "page": page,
                    "per_page": per_page,
                    "total_pages": (total + per_page - 1) // per_page
                }
            }
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取日志失败: {str(e)}"}), 500


@task_log_bp.route('/logs/latest', methods=['GET'])
def get_latest_logs():
    """
    获取最新日志
    
    查询参数:
    - task_id: 任务ID筛选
    - count: 日志数量，默认100
    """
    try:
        task_id = request.args.get('task_id')
        count = int(request.args.get('count', 100))
        
        logs = task_log_manager.get_latest_logs(count=count, task_id=task_id)
        
        return jsonify({
            "success": True,
            "data": {
                "logs": [log.to_dict() for log in logs],
                "count": len(logs)
            }
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取最新日志失败: {str(e)}"}), 500


@task_log_bp.route('/tasks/<task_id>/logs', methods=['DELETE'])
def clear_task_logs(task_id: str):
    """
    清理任务日志
    
    路径参数:
    - task_id: 任务ID
    """
    try:
        count = task_log_manager.clear_task_logs(task_id)
        
        return jsonify({
            "success": True,
            "message": f"已清理 {count} 条日志",
            "data": {"cleared_count": count}
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"清理日志失败: {str(e)}"}), 500


@task_log_bp.route('/logs', methods=['DELETE'])
def clear_all_logs():
    """
    清理所有日志
    
    警告: 此操作会删除所有日志，请谨慎使用
    """
    try:
        count = task_log_manager.clear_all_logs()
        
        return jsonify({
            "success": True,
            "message": f"已清理 {count} 条日志",
            "data": {"cleared_count": count}
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"清理日志失败: {str(e)}"}), 500


@task_log_bp.route('/logs/stats', methods=['GET'])
def get_log_stats():
    """
    获取日志统计信息
    
    查询参数:
    - task_id: 任务ID筛选
    """
    try:
        task_id = request.args.get('task_id')
        stats = task_log_manager.get_log_stats(task_id)
        
        return jsonify({
            "success": True,
            "data": stats
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取日志统计失败: {str(e)}"}), 500


@task_log_bp.route('/tasks/<task_id>/logs/export', methods=['GET'])
def export_task_logs(task_id: str):
    """
    导出任务日志
    
    路径参数:
    - task_id: 任务ID
    
    查询参数:
    - format: 导出格式，默认txt (txt/json)
    """
    try:
        # 检查任务是否存在
        task = task_queue.get_task(task_id)
        if not task:
            return jsonify({"success": False, "message": "任务不存在"}), 404
            
        export_format = request.args.get('format', 'txt')
        
        if export_format == 'json':
            # 导出为JSON
            logs = task_log_manager.get_logs_by_task(task_id)
            return jsonify({
                "success": True,
                "data": {
                    "task_id": task_id,
                    "task_name": task.name,
                    "logs": [log.to_dict() for log in logs]
                }
            })
        else:
            # 导出为文本
            logs = task_log_manager.get_logs_by_task(task_id)
            log_text = "\n".join([log.format_message() for log in logs])
            
            return jsonify({
                "success": True,
                "data": {
                    "task_id": task_id,
                    "task_name": task.name,
                    "content": log_text
                }
            })
            
    except Exception as e:
        return jsonify({"success": False, "message": f"导出日志失败: {str(e)}"}), 500
