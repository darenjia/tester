from __future__ import annotations

from flask import Blueprint, jsonify

from core.runtime_operations import (
    PreflightChecker,
    RuntimeHousekeepingService,
    RuntimeDiagnoseService,
    get_runtime_housekeeping_service,
    get_preflight_checker,
    get_runtime_diagnose_service,
)


runtime_ops_bp = Blueprint("runtime_operations", __name__, url_prefix="/api/runtime")


@runtime_ops_bp.route("/preflight", methods=["GET"])
def get_preflight_report():
    report = get_preflight_checker().run()
    status_code = 200 if report.status != "blocked" else 503
    return jsonify({"success": report.status != "blocked", "data": report.to_dict()}), status_code


@runtime_ops_bp.route("/diagnose", methods=["GET"])
def get_runtime_diagnose():
    snapshot = get_runtime_diagnose_service().build_snapshot()
    status_code = 200 if snapshot["status"] != "blocked" else 503
    return jsonify({"success": snapshot["status"] != "blocked", "data": snapshot}), status_code


@runtime_ops_bp.route("/housekeeping", methods=["POST"])
def run_runtime_housekeeping():
    result = get_runtime_housekeeping_service().run()
    return jsonify({"success": True, "data": result}), 200
