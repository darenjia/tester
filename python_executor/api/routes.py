"""
HTTP API路由定义
提供任务管理的RESTful接口
"""
from datetime import datetime
from flask import Blueprint, request, jsonify
from typing import Dict, Any

from core.task_store import task_store, TaskStatus
from utils.logger import get_logger

logger = get_logger("api_routes")

# 创建蓝图
api_bp = Blueprint('api', __name__, url_prefix='/api')


def create_response(data: Dict[str, Any] = None, message: str = None, status: str = "success") -> Dict[str, Any]:
    """创建统一响应格式"""
    response = {
        "status": status,
        "timestamp": datetime.now().isoformat()
    }
    if message:
        response["message"] = message
    if data:
        response["data"] = data
    return response


def create_error_response(message: str, code: str = "ERROR", status_code: int = 400) -> tuple:
    """创建错误响应"""
    response = {
        "status": "error",
        "code": code,
        "message": message,
        "timestamp": datetime.now().isoformat()
    }
    return jsonify(response), status_code


# ==================== 任务管理接口 ====================

@api_bp.route('/tasks', methods=['POST'])
def create_task():
    """
    创建新任务
    
    请求体格式 (TDM2.0):
    {
        "projectNo": "项目编号",
        "taskNo": "任务编号",
        "taskName": "任务名称",
        "caseList": [...]
    }
    
    响应:
    {
        "status": "success",
        "data": {
            "taskNo": "任务编号",
            "status": "pending",
            "message": "任务已创建",
            "createdAt": "2024-01-01T00:00:00"
        }
    }
    """
    try:
        # 获取请求数据
        data = request.get_json()
        if not data:
            return create_error_response("请求体不能为空", "INVALID_REQUEST", 400)
        
        # 验证必填字段
        required_fields = ['projectNo', 'taskNo', 'taskName']
        missing_fields = [f for f in required_fields if not data.get(f)]
        if missing_fields:
            return create_error_response(
                f"缺少必填字段: {', '.join(missing_fields)}",
                "MISSING_FIELDS",
                400
            )
        
        # 检查是否已有相同taskNo的任务在执行
        existing_task = task_store.get_task(data['taskNo'])
        if existing_task and existing_task.status in [TaskStatus.PENDING, TaskStatus.RUNNING]:
            return create_error_response(
                f"任务 {data['taskNo']} 正在执行中",
                "TASK_ALREADY_EXISTS",
                409
            )
        
        # 检查是否有正在执行的任务（当前只支持单任务）
        if task_store.has_running_task():
            return create_error_response(
                "已有任务正在执行，请等待完成后再提交新任务",
                "TASK_QUEUE_FULL",
                503
            )
        
        # 创建任务
        task_info = task_store.create_task(data)
        
        logger.info(f"HTTP API创建任务: taskNo={task_info.task_no}")
        
        # 触发任务执行（异步）
        from api.task_executor import execute_task_async
        execute_task_async(task_info.task_no, data)
        
        return jsonify(create_response(
            data={
                "projectNo": task_info.project_no,
                "taskNo": task_info.task_no,
                "taskName": task_info.task_name,
                "status": task_info.status.value,
                "message": "任务已创建并开始执行",
                "createdAt": task_info.created_at.isoformat()
            },
            message="任务创建成功"
        )), 201
        
    except Exception as e:
        logger.error(f"创建任务失败: {e}", exc_info=True)
        return create_error_response(f"创建任务失败: {str(e)}", "INTERNAL_ERROR", 500)


@api_bp.route('/tasks', methods=['GET'])
def list_tasks():
    """
    获取任务列表
    
    查询参数:
    - status: 任务状态筛选 (pending, running, completed, failed, cancelled)
    - page: 页码，默认1
    - pageSize: 每页数量，默认20
    
    响应:
    {
        "status": "success",
        "data": {
            "tasks": [...],
            "total": 100,
            "page": 1,
            "pageSize": 20,
            "totalPages": 5
        }
    }
    """
    try:
        # 获取查询参数
        status_filter = request.args.get('status')
        page = request.args.get('page', 1, type=int)
        page_size = request.args.get('pageSize', 20, type=int)
        
        # 限制分页大小
        page_size = min(max(page_size, 1), 100)
        
        # 转换状态筛选
        status = None
        if status_filter:
            try:
                status = TaskStatus(status_filter.lower())
            except ValueError:
                return create_error_response(
                    f"无效的状态值: {status_filter}",
                    "INVALID_STATUS",
                    400
                )
        
        # 获取任务列表
        result = task_store.list_tasks(status=status, page=page, page_size=page_size)
        
        return jsonify(create_response(data=result)), 200
        
    except Exception as e:
        logger.error(f"获取任务列表失败: {e}", exc_info=True)
        return create_error_response(f"获取任务列表失败: {str(e)}", "INTERNAL_ERROR", 500)


@api_bp.route('/tasks/<task_no>', methods=['GET'])
def get_task(task_no: str):
    """
    获取任务详情
    
    响应:
    {
        "status": "success",
        "data": {
            "taskNo": "...",
            "projectNo": "...",
            "status": "running",
            "progress": 50,
            ...
        }
    }
    """
    try:
        task = task_store.get_task(task_no)
        if not task:
            return create_error_response("任务不存在", "TASK_NOT_FOUND", 404)
        
        return jsonify(create_response(data=task.to_dict())), 200
        
    except Exception as e:
        logger.error(f"获取任务详情失败: {e}", exc_info=True)
        return create_error_response(f"获取任务详情失败: {str(e)}", "INTERNAL_ERROR", 500)


@api_bp.route('/tasks/<task_no>', methods=['DELETE'])
def cancel_task(task_no: str):
    """
    取消任务
    
    响应:
    {
        "status": "success",
        "message": "任务已取消"
    }
    """
    try:
        task = task_store.get_task(task_no)
        if not task:
            return create_error_response("任务不存在", "TASK_NOT_FOUND", 404)
        
        # 尝试取消任务
        if task_store.cancel_task(task_no):
            # 通知执行器停止任务
            from api.task_executor import cancel_task_execution
            cancel_task_execution(task_no)
            
            logger.info(f"HTTP API取消任务: {task_no}")
            return jsonify(create_response(message="任务已取消")), 200
        else:
            return create_error_response(
                f"无法取消任务，当前状态: {task.status.value}",
                "INVALID_TASK_STATUS",
                409
            )
        
    except Exception as e:
        logger.error(f"取消任务失败: {e}", exc_info=True)
        return create_error_response(f"取消任务失败: {str(e)}", "INTERNAL_ERROR", 500)


@api_bp.route('/tasks/<task_no>/results', methods=['GET'])
def get_task_results(task_no: str):
    """
    获取任务结果
    
    响应 (TDM2.0格式):
    {
        "status": "success",
        "data": {
            "taskNo": "T001",
            "platform": "NETWORK",
            "caseList": [
                {
                    "caseNo": "C001",
                    "result": "PASS",
                    "remark": "..."
                }
            ]
        }
    }
    """
    try:
        task = task_store.get_task(task_no)
        if not task:
            return create_error_response("任务不存在", "TASK_NOT_FOUND", 404)
        
        # 构建TDM2.0格式的结果
        result_data = {
            "taskNo": task.task_no,
            "platform": "NETWORK",
            "caseList": task.results if task.results else [],
            "status": task.status.value,
            "progress": task.progress
        }
        
        # 如果任务已完成，添加摘要信息
        if task.summary:
            result_data["summary"] = task.summary
        
        if task.error_message:
            result_data["errorMessage"] = task.error_message
        
        return jsonify(create_response(data=result_data)), 200
        
    except Exception as e:
        logger.error(f"获取任务结果失败: {e}", exc_info=True)
        return create_error_response(f"获取任务结果失败: {str(e)}", "INTERNAL_ERROR", 500)


@api_bp.route('/tasks/<task_no>/progress', methods=['GET'])
def get_task_progress(task_no: str):
    """
    获取任务进度
    
    响应:
    {
        "status": "success",
        "data": {
            "taskNo": "...",
            "status": "running",
            "progress": 50,
            "message": "执行中...",
            "elapsedTime": 120
        }
    }
    """
    try:
        task = task_store.get_task(task_no)
        if not task:
            return create_error_response("任务不存在", "TASK_NOT_FOUND", 404)
        
        # 计算已执行时间
        elapsed_time = None
        if task.started_at:
            if task.completed_at:
                elapsed_time = (task.completed_at - task.started_at).total_seconds()
            else:
                elapsed_time = (datetime.now() - task.started_at).total_seconds()
        
        progress_data = {
            "taskNo": task.task_no,
            "status": task.status.value,
            "progress": task.progress,
            "message": task.message,
            "elapsedTime": elapsed_time
        }
        
        return jsonify(create_response(data=progress_data)), 200
        
    except Exception as e:
        logger.error(f"获取任务进度失败: {e}", exc_info=True)
        return create_error_response(f"获取任务进度失败: {str(e)}", "INTERNAL_ERROR", 500)


# ==================== 系统状态接口 ====================

@api_bp.route('/status', methods=['GET'])
def get_system_status():
    """
    获取系统状态
    
    响应:
    {
        "status": "success",
        "data": {
            "running": true,
            "currentTask": {...},
            "statistics": {...}
        }
    }
    """
    try:
        # 获取当前运行中的任务
        running_task = task_store.get_running_task()
        
        # 获取统计信息
        stats = task_store.get_statistics()
        
        data = {
            "running": running_task is not None,
            "currentTask": running_task.to_dict() if running_task else None,
            "statistics": stats
        }
        
        return jsonify(create_response(data=data)), 200
        
    except Exception as e:
        logger.error(f"获取系统状态失败: {e}", exc_info=True)
        return create_error_response(f"获取系统状态失败: {str(e)}", "INTERNAL_ERROR", 500)


# ==================== 错误处理 ====================

@api_bp.errorhandler(404)
def not_found(error):
    """处理404错误"""
    return create_error_response("接口不存在", "NOT_FOUND", 404)


@api_bp.errorhandler(405)
def method_not_allowed(error):
    """处理405错误"""
    return create_error_response("请求方法不允许", "METHOD_NOT_ALLOWED", 405)


@api_bp.errorhandler(500)
def internal_error(error):
    """处理500错误"""
    return create_error_response("服务器内部错误", "INTERNAL_ERROR", 500)
