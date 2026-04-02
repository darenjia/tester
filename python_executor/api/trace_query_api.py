"""
Trace 查询 API
"""
from __future__ import annotations

from datetime import datetime

from flask import Blueprint, jsonify, request

from core.trace_query_service import TraceQueryService


trace_query_bp = Blueprint("trace_query", __name__, url_prefix="/api")

TRACE_QUERY_KEYS = ("trace_id", "attempt_id", "task_no", "report_id")


def get_trace_query_service() -> TraceQueryService:
    """获取 Trace 查询服务实例。"""
    return TraceQueryService()


def _create_error_response(message: str, code: str, status_code: int):
    return (
        jsonify(
            {
                "success": False,
                "error": {
                    "code": code,
                    "message": message,
                    "timestamp": datetime.now().isoformat(),
                },
            }
        ),
        status_code,
    )


def _extract_query_param():
    provided = {}
    for key in TRACE_QUERY_KEYS:
        values = [value for value in request.args.getlist(key) if value != ""]
        if not values:
            continue
        if len(values) != 1:
            return None, None, _create_error_response(
                "Exactly one of trace_id, attempt_id, task_no, report_id is required",
                "TRACE_QUERY_INVALID",
                400,
            )
        provided[key] = values[0]
    if len(provided) != 1:
        return None, None, _create_error_response(
            "Exactly one of trace_id, attempt_id, task_no, report_id is required",
            "TRACE_QUERY_INVALID",
            400,
        )
    field, value = next(iter(provided.items()))
    return field, value, None


@trace_query_bp.route("/trace/query", methods=["GET"])
def trace_query():
    field, value, error_response = _extract_query_param()
    if error_response is not None:
        return error_response

    service = get_trace_query_service()

    try:
        result = service.query(
            trace_id=value if field == "trace_id" else None,
            attempt_id=value if field == "attempt_id" else None,
            task_no=value if field == "task_no" else None,
            report_id=value if field == "report_id" else None,
        )
    except LookupError as exc:
        if str(exc) == "TRACE_NOT_FOUND":
            return _create_error_response("Trace not found", "TRACE_NOT_FOUND", 404)
        raise
    except ValueError:
        return _create_error_response(
            "Exactly one of trace_id, attempt_id, task_no, report_id is required",
            "TRACE_QUERY_INVALID",
            400,
        )

    return jsonify({"success": True, "data": result})
