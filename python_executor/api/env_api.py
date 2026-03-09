"""
环境检测API
提供环境检测相关的RESTful接口
"""
from flask import Blueprint, request, jsonify
from typing import Dict, Any
import threading

import sys
import os
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from core.env_checker import env_checker
from models.env_check_log import env_check_log_manager

# 创建蓝图
env_bp = Blueprint('env', __name__, url_prefix='/api')


@env_bp.route('/env/check', methods=['POST'])
def start_env_check():
    """
    手动触发环境检测
    
    如果检测正在进行中，返回当前状态
    """
    try:
        # 检查是否正在进行中
        if env_checker.is_checking():
            return jsonify({
                "success": True,
                "message": "检测正在进行中",
                "data": {
                    "checking": True,
                    "progress": env_checker.get_progress(),
                    "current": env_checker.get_current_check()
                }
            })
        
        # 在后台线程执行检测
        def run_check():
            result = env_checker.check_all()
            print(f"环境检测完成: {result}")
        
        thread = threading.Thread(target=run_check, daemon=True)
        thread.start()
        
        return jsonify({
            "success": True,
            "message": "环境检测已启动",
            "data": {
                "checking": True,
                "progress": 0,
                "current": "准备开始"
            }
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"启动检测失败: {str(e)}"}), 500


@env_bp.route('/env/check/status', methods=['GET'])
def get_check_status():
    """
    获取当前检测状态
    """
    try:
        return jsonify({
            "success": True,
            "data": {
                "checking": env_checker.is_checking(),
                "progress": env_checker.get_progress(),
                "current": env_checker.get_current_check()
            }
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取状态失败: {str(e)}"}), 500


@env_bp.route('/env/check/logs', methods=['GET'])
def get_check_logs():
    """
    获取检测日志列表
    
    查询参数:
    - limit: 限制数量，默认20
    """
    try:
        limit = request.args.get('limit', 20, type=int)
        logs = env_check_log_manager.get_all_logs(limit=limit)
        
        return jsonify({
            "success": True,
            "data": {
                "logs": [log.to_dict() for log in logs],
                "total": len(logs)
            }
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取日志失败: {str(e)}"}), 500


@env_bp.route('/env/check/logs/<log_id>', methods=['GET'])
def get_check_log_detail(log_id: str):
    """
    获取指定检测日志详情
    
    路径参数:
    - log_id: 日志ID
    """
    try:
        log = env_check_log_manager.get_log(log_id)
        if not log:
            return jsonify({"success": False, "message": "日志不存在"}), 404
        
        return jsonify({
            "success": True,
            "data": log.to_dict()
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取日志详情失败: {str(e)}"}), 500


@env_bp.route('/env/check/logs/<log_id>', methods=['DELETE'])
def delete_check_log(log_id: str):
    """
    删除指定检测日志
    
    路径参数:
    - log_id: 日志ID
    """
    try:
        if env_check_log_manager.delete_log(log_id):
            return jsonify({
                "success": True,
                "message": "日志已删除"
            })
        else:
            return jsonify({"success": False, "message": "日志不存在"}), 404
        
    except Exception as e:
        return jsonify({"success": False, "message": f"删除日志失败: {str(e)}"}), 500


@env_bp.route('/env/check/logs', methods=['DELETE'])
def clear_check_logs():
    """
    清空所有检测日志
    
    警告: 此操作不可恢复
    """
    try:
        count = env_check_log_manager.clear_logs()
        
        return jsonify({
            "success": True,
            "message": f"已清空 {count} 条日志",
            "data": {"cleared_count": count}
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"清空日志失败: {str(e)}"}), 500


@env_bp.route('/env/check/stats', methods=['GET'])
def get_check_stats():
    """
    获取检测统计信息
    """
    try:
        stats = env_check_log_manager.get_stats()
        
        return jsonify({
            "success": True,
            "data": stats
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取统计失败: {str(e)}"}), 500
