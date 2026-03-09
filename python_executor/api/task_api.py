"""
任务管理API
提供任务相关的RESTful接口
"""
from flask import Blueprint, request, jsonify
from typing import Dict, Any, Optional

from models.executor_task import Task, TaskStatus, TaskPriority, task_queue
from models.task_log import task_log_manager
from core.task_executor import task_executor
from core.task_scheduler import task_scheduler


# 创建蓝图
task_bp = Blueprint('task', __name__, url_prefix='/api')


@task_bp.route('/tasks', methods=['POST'])
def create_task():
    """
    创建新任务
    
    请求体:
    {
        "name": "任务名称",
        "type": "任务类型",
        "priority": 1,  // 0-3, 可选
        "params": {},   // 任务参数
        "timeout": 3600,  // 超时时间(秒), 可选
        "delay": 0,     // 延迟执行时间(秒), 可选
        "metadata": {}  // 额外元数据, 可选
    }
    """
    try:
        data = request.get_json()
        if not data:
            return jsonify({"success": False, "message": "请求体不能为空"}), 400
            
        # 必填字段
        name = data.get('name')
        task_type = data.get('type', 'default')
        
        if not name:
            return jsonify({"success": False, "message": "任务名称不能为空"}), 400
            
        # 创建任务
        task = Task(
            name=name,
            task_type=task_type,
            priority=data.get('priority', TaskPriority.NORMAL.value),
            params=data.get('params', {}),
            timeout=data.get('timeout', 3600),
            max_retries=data.get('max_retries', 3),
            created_by=request.remote_addr,
            metadata=data.get('metadata', {})
        )
        
        # 检查是否有延迟
        delay = data.get('delay', 0)
        
        if delay > 0:
            # 定时任务
            if task_scheduler.schedule_task(task, delay):
                return jsonify({
                    "success": True,
                    "message": f"任务已创建，将在{delay}秒后执行",
                    "data": task.to_dict()
                })
        else:
            # 立即执行
            if task_executor.submit_task(task):
                return jsonify({
                    "success": True,
                    "message": "任务已创建并提交到队列",
                    "data": task.to_dict()
                })
                
        return jsonify({"success": False, "message": "任务提交失败"}), 500
        
    except Exception as e:
        return jsonify({"success": False, "message": f"创建任务失败: {str(e)}"}), 500


@task_bp.route('/tasks', methods=['GET'])
def get_tasks():
    """
    获取任务列表
    
    查询参数:
    - status: 任务状态筛选 (pending/running/completed/failed/cancelled/timeout)
    - page: 页码，默认1
    - per_page: 每页数量，默认20
    - sort_by: 排序字段，默认created_at
    - sort_order: 排序方向，默认desc
    """
    try:
        # 获取查询参数
        status = request.args.get('status')
        page = int(request.args.get('page', 1))
        per_page = int(request.args.get('per_page', 20))
        sort_by = request.args.get('sort_by', 'created_at')
        sort_order = request.args.get('sort_order', 'desc')
        
        # 获取任务列表
        if status:
            tasks = task_queue.get_tasks_by_status(status)
        else:
            tasks = task_queue.get_all_tasks()
            
        # 排序
        reverse = sort_order.lower() == 'desc'
        if sort_by == 'created_at':
            tasks.sort(key=lambda x: x.created_at, reverse=reverse)
        elif sort_by == 'priority':
            tasks.sort(key=lambda x: x.priority, reverse=reverse)
        elif sort_by == 'status':
            tasks.sort(key=lambda x: x.status, reverse=reverse)
            
        # 分页
        total = len(tasks)
        start = (page - 1) * per_page
        end = start + per_page
        paginated_tasks = tasks[start:end]
        
        return jsonify({
            "success": True,
            "data": {
                "tasks": [task.to_dict() for task in paginated_tasks],
                "pagination": {
                    "total": total,
                    "page": page,
                    "per_page": per_page,
                    "total_pages": (total + per_page - 1) // per_page
                }
            }
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取任务列表失败: {str(e)}"}), 500


@task_bp.route('/tasks/<task_id>', methods=['GET'])
def get_task(task_id: str):
    """
    获取任务详情
    
    路径参数:
    - task_id: 任务ID
    """
    try:
        task = task_queue.get_task(task_id)
        if not task:
            return jsonify({"success": False, "message": "任务不存在"}), 404
            
        # 获取任务日志统计
        log_stats = task_log_manager.get_log_stats(task_id)
        
        return jsonify({
            "success": True,
            "data": {
                **task.to_dict(),
                "duration": task.get_duration(),
                "wait_time": task.get_wait_time(),
                "can_retry": task.can_retry(),
                "log_stats": log_stats
            }
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取任务详情失败: {str(e)}"}), 500


@task_bp.route('/tasks/<task_id>/cancel', methods=['POST'])
def cancel_task(task_id: str):
    """
    取消任务
    
    路径参数:
    - task_id: 任务ID
    """
    try:
        if task_executor.cancel_task(task_id):
            return jsonify({
                "success": True,
                "message": "任务已取消"
            })
        else:
            return jsonify({"success": False, "message": "任务不存在或无法取消"}), 400
            
    except Exception as e:
        return jsonify({"success": False, "message": f"取消任务失败: {str(e)}"}), 500


@task_bp.route('/tasks/<task_id>/retry', methods=['POST'])
def retry_task(task_id: str):
    """
    重试任务
    
    路径参数:
    - task_id: 任务ID
    """
    try:
        new_task = task_executor.retry_task(task_id)
        if new_task:
            return jsonify({
                "success": True,
                "message": "任务已重试",
                "data": new_task.to_dict()
            })
        else:
            return jsonify({"success": False, "message": "任务不存在或无法重试"}), 400
            
    except Exception as e:
        return jsonify({"success": False, "message": f"重试任务失败: {str(e)}"}), 500


@task_bp.route('/tasks/<task_id>', methods=['DELETE'])
def delete_task(task_id: str):
    """
    删除任务
    
    路径参数:
    - task_id: 任务ID
    """
    try:
        task = task_queue.get_task(task_id)
        if not task:
            return jsonify({"success": False, "message": "任务不存在"}), 404
            
        # 只能删除已完成的任务
        if not task.is_completed():
            return jsonify({"success": False, "message": "只能删除已完成的任务"}), 400
            
        # 从队列中移除
        if task_queue.remove(task_id):
            return jsonify({
                "success": True,
                "message": "任务已删除"
            })
        else:
            return jsonify({"success": False, "message": "删除任务失败"}), 500
            
    except Exception as e:
        return jsonify({"success": False, "message": f"删除任务失败: {str(e)}"}), 500


@task_bp.route('/tasks/stats', methods=['GET'])
def get_task_stats():
    """
    获取任务统计信息
    """
    try:
        queue_stats = task_queue.get_stats()
        executor_stats = task_executor.get_stats()
        
        return jsonify({
            "success": True,
            "data": {
                "queue": queue_stats,
                "executor": executor_stats
            }
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取统计信息失败: {str(e)}"}), 500


@task_bp.route('/tasks/clear', methods=['POST'])
def clear_completed_tasks():
    """
    清理已完成的任务
    
    请求体:
    {
        "max_age": 3600  // 最大保留时间(秒)，可选，不填则清理所有
    }
    """
    try:
        data = request.get_json() or {}
        max_age = data.get('max_age')
        
        count = task_queue.clear_completed(max_age)
        
        return jsonify({
            "success": True,
            "message": f"已清理 {count} 个任务",
            "data": {"cleared_count": count}
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"清理任务失败: {str(e)}"}), 500


@task_bp.route('/tasks/scheduled', methods=['GET'])
def get_scheduled_tasks():
    """
    获取定时任务列表
    """
    try:
        scheduled_tasks = task_scheduler.get_scheduled_tasks()
        
        return jsonify({
            "success": True,
            "data": scheduled_tasks
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取定时任务失败: {str(e)}"}), 500
