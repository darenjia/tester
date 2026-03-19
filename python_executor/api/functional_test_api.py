"""
功能测试API
提供功能测试相关的RESTful接口
"""
import json
import os
import time
import threading
from datetime import datetime
from flask import Blueprint, request, jsonify, Response
from typing import Dict, Any, Generator, Optional

from core.functional_test_runner import get_test_runner
from utils.logger import get_logger

logger = get_logger("functional_test_api")

# 创建蓝图
functional_test_bp = Blueprint('functional_test', __name__, url_prefix='/api/functional-tests')

# 全局执行状态
_execution_lock = threading.Lock()
_execution_state = {
    "is_running": False,
    "is_complete": False,
    "results": [],
    "total": 0,
    "passed": 0,
    "failed": 0,
    "warning": 0,
    "error": None
}


def create_response(data: Dict[str, Any] = None, message: str = None, success: bool = True) -> Dict[str, Any]:
    """创建统一响应格式"""
    response = {
        "success": success,
        "timestamp": datetime.now().isoformat()
    }
    if message:
        response["message"] = message
    if data:
        response["data"] = data
    return response


def create_error_response(message: str, code: str = "ERROR") -> tuple:
    """创建错误响应"""
    response = {
        "success": False,
        "code": code,
        "message": message,
        "timestamp": datetime.now().isoformat()
    }
    return jsonify(response), 400


# ==================== 测试用例管理接口 ====================

@functional_test_bp.route('/cases', methods=['GET'])
def get_all_cases():
    """
    获取所有测试用例
    
    响应:
    {
        "success": true,
        "data": {
            "categories": [
                {
                    "id": "system",
                    "name": "系统环境测试",
                    "count": 5
                }
            ],
            "cases": [
                {
                    "id": "SYS-001",
                    "name": "Python环境检查",
                    "description": "...",
                    "category": "system",
                    "category_name": "系统环境测试"
                }
            ]
        }
    }
    """
    try:
        runner = get_test_runner()
        
        # 获取所有测试用例
        cases = runner.get_all_cases()
        categories = runner.get_categories()
        
        return jsonify(create_response(data={
            "categories": categories,
            "cases": [case.to_dict() for case in cases]
        }))
        
    except Exception as e:
        logger.error(f"获取测试用例失败: {e}")
        return create_error_response(f"获取测试用例失败: {str(e)}")


@functional_test_bp.route('/cases/<category>', methods=['GET'])
def get_cases_by_category(category: str):
    """
    获取指定类别的测试用例
    
    响应:
    {
        "success": true,
        "data": {
            "category": "canoe",
            "cases": [...]
        }
    }
    """
    try:
        runner = get_test_runner()
        cases = runner.get_cases_by_category(category)
        
        if not cases:
            return create_error_response(f"类别 {category} 不存在或没有测试用例", "CATEGORY_NOT_FOUND")
        
        return jsonify(create_response(data={
            "category": category,
            "cases": [case.to_dict() for case in cases]
        }))
        
    except Exception as e:
        logger.error(f"获取类别测试用例失败: {e}")
        return create_error_response(f"获取类别测试用例失败: {str(e)}")


@functional_test_bp.route('/categories', methods=['GET'])
def get_categories():
    """
    获取所有测试分类
    
    响应:
    {
        "success": true,
        "data": {
            "categories": [
                {"id": "system", "name": "系统环境测试", "count": 5}
            ]
        }
    }
    """
    try:
        runner = get_test_runner()
        categories = runner.get_categories()
        
        return jsonify(create_response(data={
            "categories": categories
        }))
        
    except Exception as e:
        logger.error(f"获取测试分类失败: {e}")
        return create_error_response(f"获取测试分类失败: {str(e)}")


# ==================== 测试执行接口 ====================

@functional_test_bp.route('/execute', methods=['POST'])
def execute_case():
    """
    执行单个测试用例
    
    请求体:
    {
        "case_id": "CANOE-004"
    }
    
    响应:
    {
        "success": true,
        "data": {
            "case_id": "CANOE-004",
            "case_name": "COM对象创建测试",
            "category": "canoe",
            "status": "passed",
            "message": "CANoe COM对象创建成功",
            "details": {...},
            "duration": 1.23,
            "executed_at": "2024-01-15T10:30:00",
            "suggestion": ""
        }
    }
    """
    try:
        data = request.get_json()
        if not data:
            return create_error_response("请求体不能为空", "INVALID_REQUEST")
        
        case_id = data.get('case_id')
        if not case_id:
            return create_error_response("缺少case_id参数", "MISSING_CASE_ID")
        
        runner = get_test_runner()
        result = runner.execute_case(case_id)
        
        return jsonify(create_response(data=result.to_dict()))
        
    except Exception as e:
        logger.error(f"执行测试用例失败: {e}")
        return create_error_response(f"执行测试用例失败: {str(e)}")


@functional_test_bp.route('/execute-batch', methods=['POST'])
def execute_batch():
    """
    批量执行测试用例
    
    请求体:
    {
        "case_ids": ["CANOE-001", "CANOE-002", "CANOE-003"]
    }
    
    响应:
    {
        "success": true,
        "data": {
            "total": 3,
            "passed": 2,
            "failed": 1,
            "warning": 0,
            "results": [...]
        }
    }
    """
    try:
        data = request.get_json()
        if not data:
            return create_error_response("请求体不能为空", "INVALID_REQUEST")
        
        case_ids = data.get('case_ids', [])
        if not case_ids:
            return create_error_response("case_ids不能为空", "MISSING_CASE_IDS")
        
        runner = get_test_runner()
        results = runner.execute_cases(case_ids)
        
        # 统计结果
        passed = len([r for r in results if r.status == "passed"])
        failed = len([r for r in results if r.status == "failed"])
        warning = len([r for r in results if r.status == "warning"])
        
        return jsonify(create_response(data={
            "total": len(results),
            "passed": passed,
            "failed": failed,
            "warning": warning,
            "results": [r.to_dict() for r in results]
        }))
        
    except Exception as e:
        logger.error(f"批量执行测试用例失败: {e}")
        return create_error_response(f"批量执行测试用例失败: {str(e)}")


@functional_test_bp.route('/execute-category', methods=['POST'])
def execute_category():
    """
    执行指定类别的所有测试用例
    
    请求体:
    {
        "category_id": "canoe"
    }
    
    响应:
    {
        "success": true,
        "data": {
            "category": "canoe",
            "total": 9,
            "passed": 7,
            "failed": 1,
            "warning": 1,
            "results": [...]
        }
    }
    """
    try:
        data = request.get_json()
        if not data:
            return create_error_response("请求体不能为空", "INVALID_REQUEST")
        
        category_id = data.get('category_id')
        if not category_id:
            return create_error_response("缺少category_id参数", "MISSING_CATEGORY_ID")
        
        runner = get_test_runner()
        
        # 检查类别是否存在
        cases = runner.get_cases_by_category(category_id)
        if not cases:
            return create_error_response(f"类别 {category_id} 不存在或没有测试用例", "CATEGORY_NOT_FOUND")
        
        results = runner.execute_category(category_id)
        
        # 统计结果
        passed = len([r for r in results if r.status == "passed"])
        failed = len([r for r in results if r.status == "failed"])
        warning = len([r for r in results if r.status == "warning"])
        
        return jsonify(create_response(data={
            "category": category_id,
            "total": len(results),
            "passed": passed,
            "failed": failed,
            "warning": warning,
            "results": [r.to_dict() for r in results]
        }))
        
    except Exception as e:
        logger.error(f"执行类别测试用例失败: {e}")
        return create_error_response(f"执行类别测试用例失败: {str(e)}")


@functional_test_bp.route('/execute-all', methods=['POST'])
def execute_all():
    """
    执行所有测试用例

    响应:
    {
        "success": true,
        "data": {
            "total": 38,
            "passed": 30,
            "failed": 5,
            "warning": 3,
            "results": [...]
        }
    }
    """
    try:
        runner = get_test_runner()
        results = runner.execute_all()

        # 统计结果
        passed = len([r for r in results if r.status == "passed"])
        failed = len([r for r in results if r.status == "failed"])
        warning = len([r for r in results if r.status == "warning"])

        return jsonify(create_response(data={
            "total": len(results),
            "passed": passed,
            "failed": failed,
            "warning": warning,
            "results": [r.to_dict() for r in results]
        }))

    except Exception as e:
        logger.error(f"执行所有测试用例失败: {e}")
        return create_error_response(f"执行所有测试用例失败: {str(e)}")


@functional_test_bp.route('/execute-all-stream', methods=['POST'])
def execute_all_stream():
    """
    开始执行所有测试用例（异步）
    返回执行已启动的确认
    """
    global _execution_state

    with _execution_lock:
        if _execution_state["is_running"]:
            return jsonify(create_response(
                success=False,
                message="已经有测试正在执行中"
            )), 409

        _execution_state = {
            "is_running": True,
            "is_complete": False,
            "results": [],
            "total": 0,
            "passed": 0,
            "failed": 0,
            "warning": 0,
            "error": None
        }

    thread = threading.Thread(target=_execute_all_background)
    thread.daemon = True
    thread.start()

    return jsonify(create_response(
        message="测试已开始执行"
    ))


def _execute_all_background():
    """后台执行所有测试"""
    global _execution_state

    try:
        runner = get_test_runner()
        cases = runner.get_all_cases()
        total = len(cases)

        with _execution_lock:
            _execution_state["total"] = total

        logger.info(f"后台开始流式执行 {total} 个测试用例")

        for case in cases:
            result = runner.execute_case(case.id)

            with _execution_lock:
                _execution_state["results"].append(result.to_dict())

                if result.status == "passed":
                    _execution_state["passed"] += 1
                elif result.status == "failed":
                    _execution_state["failed"] += 1
                elif result.status == "warning":
                    _execution_state["warning"] += 1

        logger.info(f"后台执行完成")

        with _execution_lock:
            _execution_state["is_complete"] = True
            _execution_state["is_running"] = False

    except Exception as e:
        logger.error(f"后台执行失败: {e}")
        with _execution_lock:
            _execution_state["error"] = str(e)
            _execution_state["is_running"] = False


@functional_test_bp.route('/execute-all-progress', methods=['GET'])
def get_execution_progress():
    """
    获取测试执行进度（轮询端点）
    """
    global _execution_state

    with _execution_lock:
        state = _execution_state.copy()

    return jsonify(create_response(data={
        "is_running": state["is_running"],
        "is_complete": state["is_complete"],
        "total": state["total"],
        "completed": len(state["results"]),
        "passed": state["passed"],
        "failed": state["failed"],
        "warning": state["warning"],
        "results": state["results"],
        "error": state["error"]
    }))


# ==================== 测试历史和统计接口 ====================

@functional_test_bp.route('/history', methods=['GET'])
def get_history():
    """
    获取测试历史
    
    查询参数:
    - limit: 返回记录数量，默认20
    - category: 按类别筛选（可选）
    
    响应:
    {
        "success": true,
        "data": {
            "total": 50,
            "logs": [...]
        }
    }
    """
    try:
        limit = request.args.get('limit', 20, type=int)
        category = request.args.get('category', None)
        
        runner = get_test_runner()
        history = runner.get_history(limit=limit)
        
        # 按类别筛选
        if category:
            history = [h for h in history if h.get('category') == category]
        
        return jsonify(create_response(data={
            "total": len(history),
            "logs": history
        }))
        
    except Exception as e:
        logger.error(f"获取测试历史失败: {e}")
        return create_error_response(f"获取测试历史失败: {str(e)}")


@functional_test_bp.route('/stats', methods=['GET'])
def get_stats():
    """
    获取测试统计
    
    响应:
    {
        "success": true,
        "data": {
            "total_executions": 100,
            "total_passed": 80,
            "total_failed": 15,
            "total_warning": 5,
            "pass_rate": 80.0
        }
    }
    """
    try:
        runner = get_test_runner()
        stats = runner.get_stats()
        
        return jsonify(create_response(data=stats))
        
    except Exception as e:
        logger.error(f"获取测试统计失败: {e}")
        return create_error_response(f"获取测试统计失败: {str(e)}")


# ==================== 错误处理 ====================

@functional_test_bp.errorhandler(404)
def not_found(error):
    """处理404错误"""
    return create_error_response("接口不存在", "NOT_FOUND")


@functional_test_bp.errorhandler(405)
def method_not_allowed(error):
    """处理405错误"""
    return create_error_response("请求方法不允许", "METHOD_NOT_ALLOWED")
