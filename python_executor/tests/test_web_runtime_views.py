from __future__ import annotations

from pathlib import Path

from flask import Flask, render_template


def _build_template_app() -> Flask:
    repo_root = Path(__file__).resolve().parents[1]
    template_dir = repo_root / "web" / "templates"
    static_dir = repo_root / "web" / "static"
    app = Flask(
        __name__,
        template_folder=str(template_dir),
        static_folder=str(static_dir),
    )

    @app.route("/system-check")
    def system_check_page():
        return render_template("system_check.html", active_page="system-check")

    @app.route("/dashboard")
    def dashboard_page():
        return render_template("dashboard.html", active_page="dashboard")

    @app.route("/report-status")
    def report_status_page():
        return render_template("report_status.html", active_page="report-status")

    @app.route("/report-status/<report_id>/view")
    def report_detail_page(report_id: str):
        return render_template("report_detail.html", active_page="report-status", report_id=report_id)

    @app.route("/tasks")
    def tasks_page():
        return render_template("tasks.html", active_page="tasks")

    @app.route("/tasks/<task_id>/view")
    def task_detail_page(task_id: str):
        return render_template("task_detail.html", active_page="tasks", task_id=task_id)

    @app.route("/logs")
    def logs_page():
        return render_template("logs.html", active_page="logs")

    @app.route("/settings")
    def settings_page():
        return render_template("settings.html", active_page="settings")

    return app


def test_system_check_page_exposes_runtime_and_detailed_views():
    app = _build_template_app()
    client = app.test_client()

    response = client.get("/system-check")

    assert response.status_code == 200
    html = response.get_data(as_text=True)
    assert "运行诊断" in html
    assert "详细检测" in html
    assert "runtime-tab" in html
    assert "detailed-tab" in html
    assert "housekeeping" in html
    assert "preflight-list" in html
    assert "runtime-services-grid" in html
    assert "可观测链路" in html
    assert "failed-report-traceability" in html


def test_report_status_page_exposes_list_and_detail_views():
    app = _build_template_app()
    client = app.test_client()

    response = client.get("/report-status")

    assert response.status_code == 200
    html = response.get_data(as_text=True)
    assert "失败报告列表" in html
    assert "报告详情" in html
    assert "attempt-history" in html
    assert "report-status-feedback" in html
    assert "reports-table-feedback" in html
    assert "report-detail-panel" in html


def test_report_detail_page_exposes_report_and_attempt_sections():
    app = _build_template_app()
    client = app.test_client()

    response = client.get("/report-status/RPT-001/view")

    assert response.status_code == 200
    html = response.get_data(as_text=True)
    assert "失败报告详情" in html
    assert "attempt-history" in html
    assert "ExecutionOutcome 摘要" in html
    assert "trace_id" in html
    assert "error_category" in html
    assert "report-detail-root" in html
    assert "data-ui-state" in html


def test_dashboard_page_exposes_runtime_overview_links():
    app = _build_template_app()
    client = app.test_client()

    response = client.get("/dashboard")

    assert response.status_code == 200
    html = response.get_data(as_text=True)
    assert "运行就绪度" in html
    assert "失败报告队列" in html
    assert "/system-check" in html
    assert "/report-status" in html
    assert "runtime-overview-feedback" in html


def test_tasks_page_exposes_unified_task_detail_tabs():
    app = _build_template_app()
    client = app.test_client()

    response = client.get("/tasks")

    assert response.status_code == 200
    html = response.get_data(as_text=True)
    assert "任务详情" in html
    assert "执行过程" in html
    assert "测试结果" in html
    assert "日志" in html
    assert 'data-tab="diagnosis"' in html
    assert 'panel-diagnosis' in html
    assert "task-list-feedback" in html
    assert "task-detail-body" in html


def test_task_detail_page_exposes_standalone_runtime_sections():
    app = _build_template_app()
    client = app.test_client()

    response = client.get("/tasks/TASK-001/view")

    assert response.status_code == 200
    html = response.get_data(as_text=True)
    assert "任务详情" in html
    assert "执行时间线" in html
    assert "测试结果" in html
    assert "诊断上下文" in html
    assert "attempt_id" in html
    assert "trace_id" in html
    assert "task-detail-root" in html
    assert "data-ui-state" in html


def test_logs_page_exposes_runtime_summary_and_log_stream():
    app = _build_template_app()
    client = app.test_client()

    response = client.get("/logs")

    assert response.status_code == 200
    html = response.get_data(as_text=True)
    assert "日志流与运行上下文" in html
    assert "实时日志流" in html
    assert "运行摘要" in html
    assert "scheduler-status" in html
    assert "logs-container" in html


def test_settings_page_exposes_overview_and_configuration_sections():
    app = _build_template_app()
    client = app.test_client()

    response = client.get("/settings")

    assert response.status_code == 200
    html = response.get_data(as_text=True)
    assert "统一设置与运行配置" in html
    assert "Configuration Control" in html
    assert "settings-summary-grid" in html
    assert "配置管理" in html
    assert "settings-overview-feedback" in html
