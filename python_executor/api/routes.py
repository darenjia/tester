"""
HTTP API路由定义
提供任务管理的RESTful接口
"""
from datetime import datetime
from flask import Blueprint, request, jsonify
from typing import Dict, Any, Optional

from models.executor_task import Task, TaskStatus, TaskPriority, task_queue
from core.case_mapping_manager import get_case_mapping_manager
from utils.logger import get_logger

logger = get_logger("api_routes")

# 创建蓝图
api_bp = Blueprint('api', __name__, url_prefix='/api/tdm2')


def determine_tool_type_from_cases(case_list: list) -> Optional[str]:
    """
    从用例列表通过用例映射确定tool_type

    Args:
        case_list: 用例列表

    Returns:
        tool_type字符串，如果无法确定则返回None
    """
    if not case_list:
        return None

    mapping_manager = get_case_mapping_manager()
    tool_types = set()

    for case in case_list:
        case_no = case.get('caseNo') or case.get('case_no')
        if case_no:
            mapping = mapping_manager.get_mapping(case_no)
            if mapping and mapping.category:
                tool_types.add(mapping.category.lower())

    if len(tool_types) == 1:
        tool_type = list(tool_types)[0]
        logger.info(f"从用例映射确定tool_type: {tool_type}")
        return tool_type
    elif len(tool_types) > 1:
        logger.warning(f"用例列表包含多种tool_type: {tool_types}")
        raise ValueError(f"用例列表包含多个工具类型: {', '.join(sorted(tool_types))}")

    logger.info("无法从用例映射确定tool_type")
    return None


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


def task_to_dict(task: Task) -> Dict[str, Any]:
    """将Task对象转换为字典格式"""
    return {
        "id": task.id,
        "name": task.name,
        "status": task.status,
        "priority": task.priority,
        "task_type": task.task_type,
        "params": task.params,
        "result": task.result,
        "error_message": task.error_message,
        "created_at": task.created_at,
        "started_at": task.started_at,
        "completed_at": task.completed_at,
        "timeout": task.timeout,
        "retry_count": task.retry_count,
        "max_retries": task.max_retries,
        "metadata": task.metadata
    }


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
        required_fields = ['projectNo', 'taskNo']
        missing_fields = [f for f in required_fields if not data.get(f)]
        if missing_fields:
            return create_error_response(
                f"缺少必填字段: {', '.join(missing_fields)}",
                "MISSING_FIELDS",
                400
            )

        task_no = data.get('taskNo')

        case_list = data.get('caseList', [])
        if not isinstance(case_list, list) or not case_list:
            return create_error_response("caseList不能为空", "INVALID_CASE_LIST", 400)

        # 检查是否已有相同taskNo的任务在执行
        existing_task = task_queue.get_task(task_no)
        if existing_task and existing_task.status in [TaskStatus.PENDING.value, TaskStatus.RUNNING.value]:
            return create_error_response(
                f"任务 {task_no} 正在执行中",
                "TASK_ALREADY_EXISTS",
                409
            )

        # 检查是否有正在执行的任务（当前只支持单任务）
        running_tasks = task_queue.get_running_tasks()
        if running_tasks:
            return create_error_response(
                "已有任务正在执行，请等待完成后再提交新任务",
                "TASK_QUEUE_FULL",
                503
            )

        # 从用例映射获取tool_type（不依赖请求参数中的toolType）
        tool_type = data.get('toolType')
        if case_list and not tool_type:
            try:
                determined_tool_type = determine_tool_type_from_cases(case_list)
            except ValueError as e:
                return create_error_response(str(e), "TASK_COMPILE_FAILED", 400)

            if determined_tool_type:
                tool_type = determined_tool_type
                data['toolType'] = determined_tool_type
                logger.info(f"从用例映射自动设置toolType: {determined_tool_type}")

        # 构建params，包含TDM2.0的原始数据
        params = {
            "projectNo": data.get('projectNo', ''),
            "taskNo": task_no,
            "taskName": data.get('taskName', ''),
            "caseList": case_list,
            "deviceId": data.get('deviceId'),
            "toolType": tool_type,
            "configPath": data.get('configPath'),
            "timeout": data.get('timeout', 3600)
        }

        # 创建任务
        task = Task(
            id=task_no,
            name=data.get('taskName') or data.get('projectNo', '未命名任务'),
            status=TaskStatus.PENDING.value,
            priority=data.get('priority', TaskPriority.NORMAL.value),
            task_type=tool_type or 'default',
            params=params,
            timeout=data.get('timeout', 3600),
            max_retries=data.get('max_retries', 3),
            metadata=data
        )

        # 添加到队列
        if task_queue.add(task):
            logger.info(f"HTTP API创建任务: taskNo={task_no}")
        else:
            return create_error_response(f"任务 {task_no} 添加失败", "ADD_TASK_FAILED", 500)

        # 触发任务执行（异步）
        from api.task_executor import execute_task_async
        execute_task_async(task.id, data)

        return jsonify(create_response(
            data={
                "projectNo": data.get('projectNo', ''),
                "taskNo": task_no,
                "taskName": data.get('taskName', ''),
                "status": task.status,
                "message": "任务已创建并开始执行",
                "createdAt": task.created_at
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

        # 获取任务列表
        if status_filter:
            try:
                status_enum = TaskStatus(status_filter.lower())
                tasks = task_queue.get_tasks_by_status(status_enum.value)
            except ValueError:
                return create_error_response(
                    f"无效的状态值: {status_filter}",
                    "INVALID_STATUS",
                    400
                )
        else:
            tasks = task_queue.get_all_tasks()

        # 按创建时间倒序排序
        tasks.sort(key=lambda x: x.created_at, reverse=True)

        # 分页
        total = len(tasks)
        start_idx = (page - 1) * page_size
        end_idx = start_idx + page_size
        paginated_tasks = tasks[start_idx:end_idx]

        return jsonify(create_response(data={
            "tasks": [task_to_dict(task) for task in paginated_tasks],
            "total": total,
            "page": page,
            "pageSize": page_size,
            "totalPages": (total + page_size - 1) // page_size if page_size > 0 else 0
        })), 200

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
            "taskNo": "任务编号",
            "status": "pending",
            ...
        }
    }
    """
    try:
        task = task_queue.get_task(task_no)
        if not task:
            return create_error_response(f"任务 {task_no} 不存在", "TASK_NOT_FOUND", 404)

        return jsonify(create_response(data=task_to_dict(task))), 200

    except Exception as e:
        logger.error(f"获取任务详情失败: {e}", exc_info=True)
        return create_error_response(f"获取任务详情失败: {str(e)}", "INTERNAL_ERROR", 500)


@api_bp.route('/tasks/<task_no>', methods=['DELETE'])
def delete_task(task_no: str):
    """
    删除任务

    响应:
    {
        "status": "success",
        "message": "任务已删除"
    }
    """
    try:
        task = task_queue.get_task(task_no)
        if not task:
            return create_error_response(f"任务 {task_no} 不存在", "TASK_NOT_FOUND", 404)

        if task.status == TaskStatus.RUNNING.value:
            return create_error_response("无法删除运行中的任务", "TASK_RUNNING", 400)

        if task_queue.remove(task_no):
            return jsonify(create_response(message="任务已删除")), 200
        else:
            return create_error_response("删除任务失败", "DELETE_FAILED", 500)

    except Exception as e:
        logger.error(f"删除任务失败: {e}", exc_info=True)
        return create_error_response(f"删除任务失败: {str(e)}", "INTERNAL_ERROR", 500)


@api_bp.route('/tasks/<task_no>/results', methods=['GET'])
def get_task_results(task_no: str):
    """
    获取任务结果

    响应:
    {
        "status": "success",
        "data": {
            "taskNo": "任务编号",
            "platform": "NETWORK",
            "caseList": [...],
            "status": "completed",
            "progress": 100
        }
    }
    """
    try:
        task = task_queue.get_task(task_no)
        if not task:
            return create_error_response(f"任务 {task_no} 不存在", "TASK_NOT_FOUND", 404)

        result_data = {
            "taskNo": task.id,
            "platform": "NETWORK",
            "caseList": task.params.get('caseList', []) if task.params else [],
            "status": task.status,
            "progress": 100 if task.is_completed() else (50 if task.status == TaskStatus.RUNNING.value else 0)
        }

        if task.result:
            result_data["results"] = task.result

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
            "taskNo": "任务编号",
            "status": "running",
            "progress": 50,
            "message": "执行中..."
        }
    }
    """
    try:
        task = task_queue.get_task(task_no)
        if not task:
            return create_error_response(f"任务 {task_no} 不存在", "TASK_NOT_FOUND", 404)

        progress = 0
        if task.is_completed():
            progress = 100
        elif task.status == TaskStatus.RUNNING.value:
            progress = 50
        elif task.status == TaskStatus.PENDING.value:
            progress = 0

        return jsonify(create_response(data={
            "taskNo": task.id,
            "status": task.status,
            "progress": progress,
            "message": task.params.get('message', '') if task.params else ''
        })), 200

    except Exception as e:
        logger.error(f"获取任务进度失败: {e}", exc_info=True)
        return create_error_response(f"获取任务进度失败: {str(e)}", "INTERNAL_ERROR", 500)


@api_bp.route('/status', methods=['GET'])
def get_status():
    """
    获取系统状态

    响应:
    {
        "status": "success",
        "data": {
            "tasks": {
                "pending": 0,
                "running": 1,
                "completed": 10
            },
            "system": {
                "uptime": 3600
            }
        }
    }
    """
    try:
        all_tasks = task_queue.get_all_tasks()
        pending_count = len(task_queue.get_pending_tasks())
        running_count = len(task_queue.get_running_tasks())
        completed_tasks = task_queue.get_completed_tasks()

        return jsonify(create_response(data={
            "tasks": {
                "total": len(all_tasks),
                "pending": pending_count,
                "running": running_count,
                "completed": len(completed_tasks)
            }
        })), 200

    except Exception as e:
        logger.error(f"获取系统状态失败: {e}", exc_info=True)
        return create_error_response(f"获取系统状态失败: {str(e)}", "INTERNAL_ERROR", 500)


@api_bp.errorhandler(404)
def not_found(error):
    """404错误处理"""
    return create_error_response("接口不存在", "NOT_FOUND", 404)


@api_bp.errorhandler(405)
def method_not_allowed(error):
    """405错误处理"""
    return create_error_response("不支持的请求方法", "METHOD_NOT_ALLOWED", 405)


@api_bp.errorhandler(500)
def internal_error(error):
    """500错误处理"""
    return create_error_response("服务器内部错误", "INTERNAL_ERROR", 500)
