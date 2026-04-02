"""
服务状态API
提供服务状态监控相关的RESTful接口
"""
from flask import Blueprint, request, jsonify
from typing import Dict, Any, Optional
from datetime import datetime

from core.task_executor_production import get_task_executor
task_executor = get_task_executor()
from core.task_scheduler import task_scheduler
from models.executor_task import task_queue
from models.task_log import task_log_manager
from core.runtime_operations import get_preflight_checker, get_runtime_diagnose_service


# 创建蓝图
service_bp = Blueprint('service', __name__, url_prefix='/api')

# 服务状态存储
_service_status = {
    "executor": {
        "name": "任务执行器",
        "status": "running",
        "started_at": datetime.now().isoformat(),
        "last_heartbeat": datetime.now().isoformat()
    },
    "scheduler": {
        "name": "任务调度器",
        "status": "running",
        "started_at": datetime.now().isoformat(),
        "last_heartbeat": datetime.now().isoformat()
    }
}

# 接口调用统计
_endpoint_stats = {}


def _build_runtime_operations_summary() -> Dict[str, Any]:
    """构建运行时运维摘要。"""
    preflight_report = get_preflight_checker().run()
    diagnose_snapshot = get_runtime_diagnose_service().build_snapshot()
    return {
        "preflight": {
            "status": preflight_report.status,
            "summary": preflight_report.summary,
        },
        "diagnose": {
            "status": diagnose_snapshot.get("status", "unknown"),
            "services": diagnose_snapshot.get("services", {}),
        },
    }


def record_endpoint_call(endpoint: str, response_time: float, success: bool = True):
    """记录接口调用"""
    if endpoint not in _endpoint_stats:
        _endpoint_stats[endpoint] = {
            "total_calls": 0,
            "successful_calls": 0,
            "failed_calls": 0,
            "total_response_time": 0,
            "avg_response_time": 0,
            "last_call_time": None
        }
    
    stats = _endpoint_stats[endpoint]
    stats["total_calls"] += 1
    stats["total_response_time"] += response_time
    stats["avg_response_time"] = stats["total_response_time"] / stats["total_calls"]
    stats["last_call_time"] = datetime.now().isoformat()
    
    if success:
        stats["successful_calls"] += 1
    else:
        stats["failed_calls"] += 1


@service_bp.route('/services/status', methods=['GET'])
def get_all_services_status():
    """
    获取所有服务状态
    """
    try:
        # 更新执行器状态
        executor_stats = task_executor.get_stats()
        _service_status["executor"]["stats"] = executor_stats
        _service_status["executor"]["last_heartbeat"] = datetime.now().isoformat()
        
        # 更新调度器状态
        scheduler_stats = task_scheduler.get_stats()
        _service_status["scheduler"]["stats"] = scheduler_stats
        _service_status["scheduler"]["last_heartbeat"] = datetime.now().isoformat()

        runtime_operations = _build_runtime_operations_summary()
        
        return jsonify({
            "success": True,
            "data": {
                **_service_status,
                "runtime_operations": runtime_operations,
            }
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取服务状态失败: {str(e)}"}), 500


@service_bp.route('/services/<service_name>/status', methods=['GET'])
def get_service_status(service_name: str):
    """
    获取指定服务状态
    
    路径参数:
    - service_name: 服务名称
    """
    try:
        if service_name not in _service_status:
            return jsonify({"success": False, "message": "服务不存在"}), 404
            
        # 更新状态
        if service_name == "executor":
            _service_status[service_name]["stats"] = task_executor.get_stats()
        elif service_name == "scheduler":
            _service_status[service_name]["stats"] = task_scheduler.get_stats()
            
        _service_status[service_name]["last_heartbeat"] = datetime.now().isoformat()
        
        return jsonify({
            "success": True,
            "data": _service_status[service_name]
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取服务状态失败: {str(e)}"}), 500


@service_bp.route('/services/<service_name>/heartbeat', methods=['POST'])
def service_heartbeat(service_name: str):
    """
    服务心跳上报
    
    路径参数:
    - service_name: 服务名称
    """
    try:
        if service_name not in _service_status:
            return jsonify({"success": False, "message": "服务不存在"}), 404
            
        _service_status[service_name]["last_heartbeat"] = datetime.now().isoformat()
        _service_status[service_name]["status"] = "running"
        
        return jsonify({
            "success": True,
            "message": "心跳上报成功"
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"心跳上报失败: {str(e)}"}), 500


@service_bp.route('/endpoints/stats', methods=['GET'])
def get_endpoint_stats():
    """
    获取接口调用统计
    """
    try:
        return jsonify({
            "success": True,
            "data": _endpoint_stats
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取接口统计失败: {str(e)}"}), 500


@service_bp.route('/system/stats', methods=['GET'])
def get_system_stats():
    """
    获取系统整体统计
    """
    try:
        # 任务统计
        task_stats = task_queue.get_stats()
        
        # 执行器统计
        executor_stats = task_executor.get_stats()
        
        # 调度器统计
        scheduler_stats = task_scheduler.get_stats()
        
        # 日志统计
        log_stats = task_log_manager.get_log_stats()
        
        # 计算系统运行时间
        uptime = None
        if "executor" in _service_status:
            started = datetime.fromisoformat(_service_status["executor"]["started_at"])
            uptime = (datetime.now() - started).total_seconds()
        
        return jsonify({
            "success": True,
            "data": {
                "uptime": uptime,
                "tasks": task_stats,
                "executor": executor_stats,
                "scheduler": scheduler_stats,
                "logs": log_stats,
                "runtime_operations": _build_runtime_operations_summary(),
                "services": {
                    name: {
                        "status": info["status"],
                        "last_heartbeat": info["last_heartbeat"]
                    }
                    for name, info in _service_status.items()
                }
            }
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取系统统计失败: {str(e)}"}), 500


@service_bp.route('/system/health', methods=['GET'])
def health_check():
    """
    系统健康检查
    """
    try:
        health_status = {
            "status": "healthy",
            "checks": {}
        }
        
        # 检查执行器
        try:
            executor_stats = task_executor.get_stats()
            health_status["checks"]["executor"] = {
                "status": "healthy",
                "running_tasks": executor_stats["running_count"]
            }
        except Exception as e:
            health_status["checks"]["executor"] = {
                "status": "unhealthy",
                "error": str(e)
            }
            health_status["status"] = "unhealthy"
        
        # 检查调度器
        try:
            scheduler_stats = task_scheduler.get_stats()
            health_status["checks"]["scheduler"] = {
                "status": "healthy",
                "running": scheduler_stats["running"]
            }
        except Exception as e:
            health_status["checks"]["scheduler"] = {
                "status": "unhealthy",
                "error": str(e)
            }
            health_status["status"] = "unhealthy"
        
        # 检查任务队列
        try:
            queue_stats = task_queue.get_stats()
            health_status["checks"]["task_queue"] = {
                "status": "healthy",
                "pending": queue_stats["pending"],
                "running": queue_stats["running"]
            }
        except Exception as e:
            health_status["checks"]["task_queue"] = {
                "status": "unhealthy",
                "error": str(e)
            }
            health_status["status"] = "unhealthy"
        
        status_code = 200 if health_status["status"] == "healthy" else 503
        
        return jsonify({
            "success": health_status["status"] == "healthy",
            "data": health_status
        }), status_code
        
    except Exception as e:
        return jsonify({
            "success": False,
            "message": f"健康检查失败: {str(e)}"
        }), 500
