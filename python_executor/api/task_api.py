"""
任务管理API
提供任务相关的RESTful接口
"""
import uuid
from flask import Blueprint, request, jsonify
from typing import Dict, Any, Optional

from models.executor_task import Task, TaskStatus, TaskPriority, task_queue
from models.task_log import task_log_manager
from core.case_mapping_manager import get_case_mapping_manager
from core.task_executor_production import get_task_executor
from core.task_compiler import TaskCompiler, TaskCompileError
task_executor = get_task_executor()
from core.task_scheduler import task_scheduler


# 创建蓝图
task_bp = Blueprint('task', __name__, url_prefix='/api')


def _compile_tdm2_execution_plan(data: Dict[str, Any]):
    compiler = TaskCompiler(get_case_mapping_manager())
    case_list = data.get("caseList") or []
    payload = {
        "taskNo": data.get("taskNo") or str(uuid.uuid4()),
        "projectNo": data.get("projectNo", ""),
        "taskName": data.get("taskName", ""),
        "deviceId": data.get("deviceId"),
        "toolType": data.get("toolType"),
        "configPath": data.get("configPath"),
        "configName": data.get("configName"),
        "baseConfigDir": data.get("baseConfigDir"),
        "variables": data.get("variables"),
        "canoeNamespace": data.get("canoeNamespace"),
        "timeout": data.get("timeout", 3600),
        "testItems": [
            {
                "caseNo": case.get("caseNo") or case.get("case_no") or "",
                "name": case.get("caseName") or case.get("name") or "",
                "type": case.get("caseType") or case.get("type") or "test_module",
                "dtcInfo": case.get("dtcInfo") or case.get("dtc_info"),
                "params": case.get("params"),
                "repeat": case.get("repeat", 1),
            }
            for case in case_list
        ],
    }
    return compiler.compile_payload(payload)


@task_bp.route('/tasks', methods=['POST'])
def create_task():
    """
    创建新任务

    支持两种请求格式：
    1. TDM2.0格式（推荐）:
    {
        "taskNo": "TT26020203",
        "projectNo": "yyy",
        "deviceId": "测试",
        "caseList": [
            {
                "caseNo": "ANM_TG1_TC02_SC01",
                "caseName": "测试用例名称",
                "caseSource": "标准要求",
                "caseType": "",
                "expectedResult": "预期结果",
                "maintainer": "维护人",
                "moduleLevel1": "通信",
                "moduleLevel2": "",
                "moduleLevel3": "",
                "preCondition": "前置条件",
                "priority": "P3低",
                "stepDescription": "步骤描述"
            }
        ]
    }

    2. 内部格式（兼容）:
    {
        "taskNo": "任务ID",
        "name": "任务名称",
        "type": "任务类型",
        "priority": 1,
        "params": {},
        "timeout": 3600,
        "delay": 0,
        "metadata": {}
    }
    """
    try:
        data = request.get_json()
        if not data:
            return jsonify({"success": False, "message": "请求体不能为空"}), 400

        # 检测是否为TDM2.0格式（包含caseList）
        if 'caseList' in data:
            try:
                execution_plan = _compile_tdm2_execution_plan(data)
            except TaskCompileError as exc:
                return jsonify({"success": False, "message": str(exc)}), 400

            task_no = execution_plan.task_no

            if task_executor.execute_plan(execution_plan):
                return jsonify({
                    "success": True,
                    "message": "任务已创建并提交到队列",
                    "data": {
                        "taskNo": task_no,
                        "projectNo": execution_plan.project_no,
                        "deviceId": execution_plan.device_id,
                        "caseCount": len(execution_plan.cases)
                    }
                })
            else:
                return jsonify({"success": False, "message": "任务提交失败"}), 500

        # 内部格式处理（兼容旧接口）
        name = data.get('name') or data.get('projectNo', '未命名任务')
        task_type = data.get('type', 'default')

        # 支持自定义 taskNo（用于幂等性/覆盖）
        task_id = data.get('taskNo') or data.get('id')

        # 创建任务
        task = Task(
            id=task_id,  # 如果为 None，会自动生成 UUID
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
            try:
                task_executor._build_execution_plan_from_queue_task(task)
            except TaskCompileError as exc:
                return jsonify({"success": False, "message": str(exc)}), 400

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

        # 安全解析分页参数
        try:
            page = int(request.args.get('page', 1))
            per_page = int(request.args.get('per_page', 20))
        except ValueError as e:
            return jsonify({"success": False, "message": f"分页参数无效: {str(e)}"}), 400

        sort_by = request.args.get('sort_by', 'created_at')
        sort_order = request.args.get('sort_order', 'desc')

        # 获取任务列表
        if status:
            tasks = task_queue.get_tasks_by_status(status)
        else:
            tasks = task_queue.get_all_tasks()

        # 确保tasks是列表
        if tasks is None:
            tasks = []
            
        # 排序（处理None值）
        reverse = sort_order.lower() == 'desc'
        if sort_by == 'created_at':
            tasks.sort(key=lambda x: x.created_at or '', reverse=reverse)
        elif sort_by == 'priority':
            tasks.sort(key=lambda x: x.priority or 0, reverse=reverse)
        elif sort_by == 'status':
            tasks.sort(key=lambda x: x.status or '', reverse=reverse)
            
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


def _extract_ttman_result(result: Optional[Dict[str, Any]]) -> Optional[Dict[str, Any]]:
    """从任务结果中提取TTworkbench专用结果"""
    if not result:
        return None

    # 从results列表中查找包含log_details的结果
    if isinstance(result, dict):
        results_list = result.get('results', [])
        for r in results_list:
            if isinstance(r, dict) and r.get('log_details'):
                log_details = r.get('log_details')
                if log_details.get('parsed'):
                    return {
                        "verdict": log_details.get('verdict', 'NONE'),
                        "verdict_code": log_details.get('return_code'),
                        "total_cases": log_details.get('total_cases', 1),
                        "passed_cases": log_details.get('passed_cases', 0),
                        "failed_cases": log_details.get('failed_cases', 0),
                        "inconclusive_cases": log_details.get('inconclusive_cases', 0),
                        "case_results": log_details.get('case_results', []),
                        "execution_time": log_details.get('execution_time', 0),
                        "parse_errors": log_details.get('parse_errors')
                    }

        # 也检查summary中是否有ttman_result
        summary = result.get('summary', {})
        if isinstance(summary, dict) and summary.get('ttman_result'):
            return summary.get('ttman_result')

    return None


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

        # 获取最近的日志（最近20条）
        recent_logs = task_log_manager.get_latest_logs(count=20, task_id=task_id)

        # 构建执行信息
        execution_info = {
            "started_at": task.started_at,
            "completed_at": task.completed_at,
            "duration": task.get_duration(),
            "wait_time": task.get_wait_time(),
            "timeout": task.timeout,
            "retry_count": task.retry_count,
            "max_retries": task.max_retries
        }

        # 构建测试结果列表
        test_results = []
        if task.result and isinstance(task.result, dict):
            results_list = task.result.get('results', [])
            for r in results_list:
                test_results.append({
                    "name": r.get('name', ''),
                    "type": r.get('type', ''),
                    "verdict": r.get('verdict', ''),
                    "passed": r.get('passed'),
                    "error": r.get('error'),
                    "details": r.get('details')
                })

        # 构建结果摘要
        result_summary = None
        if task.result and isinstance(task.result, dict):
            result_summary = task.result.get('summary')
        elif test_results:
            total = len(test_results)
            passed = sum(1 for r in test_results if r.get('passed') is True or r.get('verdict') == 'PASS')
            failed = sum(1 for r in test_results if r.get('passed') is False or r.get('verdict') == 'FAIL')
            blocked = sum(1 for r in test_results if r.get('verdict') == 'BLOCK')
            result_summary = {
                "total": total,
                "passed": passed,
                "failed": failed,
                "blocked": blocked,
                "pass_rate": f"{(passed / total * 100):.1f}%" if total > 0 else "0%"
            }

        return jsonify({
            "success": True,
            "data": {
                # 基本信息
                "id": task.id,
                "name": task.name,
                "status": task.status,
                "priority": task.priority,
                "task_type": task.task_type,
                "created_at": task.created_at,
                "created_by": task.created_by,
                "error_message": task.error_message,
                "can_retry": task.can_retry(),

                # 执行信息
                "execution": execution_info,

                # 任务参数
                "params": task.params,

                # 元数据
                "metadata": task.metadata,

                # 分类信息（从params或metadata获取）
                "category": (
                    task.params.get('category')
                    or task.params.get('tool_type')
                    or task.metadata.get('category')
                    or task.metadata.get('toolType')
                    or task.metadata.get('tool_type')
                    or 'canoe'
                ).lower(),

                # 测试结果摘要
                "result_summary": result_summary,

                # 测试结果列表
                "test_results": test_results,

                # TTworkbench专用结果
                "ttman_result": _extract_ttman_result(task.result),

                # 日志统计
                "log_stats": log_stats,

                # 最近日志
                "recent_logs": [log.to_dict() for log in recent_logs]
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
        if task_scheduler.cancel_scheduled_task(task_id):
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
            
        # 正在运行的任务不能删除
        if task.is_running():
            return jsonify({"success": False, "message": "任务正在执行中，无法删除"}), 400

        if task.status == TaskStatus.PENDING.value:
            task_executor.cancel_task(task_id) or task_scheduler.cancel_scheduled_task(task_id)

        # 从队列中移除
        if task_queue.remove(task_id) or task_queue.get_task(task_id) is None:
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
        scheduler_stats = task_scheduler.get_stats()
        
        return jsonify({
            "success": True,
            "data": {
                "queue": queue_stats,
                "executor": executor_stats,
                "scheduler": scheduler_stats,
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
