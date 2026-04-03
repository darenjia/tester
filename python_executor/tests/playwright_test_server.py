"""
Playwright E2E test for the Test Executor web application.

Tests the actual rendered HTML content of the key pages.
"""
import subprocess
import time
import sys
import os

from playwright.sync_api import sync_playwright


SERVER_PORT = 5200
SERVER_URL = f"http://127.0.0.1:{SERVER_PORT}"


def start_server():
    """Start the Flask dev server as a subprocess."""
    project_root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    env = os.environ.copy()
    env["PYTHONUNBUFFERED"] = "1"
    env["PYTHONPATH"] = project_root
    server_script = os.path.join(os.path.dirname(os.path.abspath(__file__)), "run_flask_server.py")
    proc = subprocess.Popen(
        [sys.executable, server_script],
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        text=True,
        bufsize=1,
        cwd=project_root,
        env=env,
    )
    # Wait for server to start
    import urllib.request
    for _ in range(30):
        try:
            urllib.request.urlopen(f"{SERVER_URL}/", timeout=1)
            break
        except Exception:
            time.sleep(0.5)
    else:
        # Print server output for debugging
        print("Server output:", file=sys.stderr)
        for line in iter(proc.stdout.readline, ""):
            print(line, file=sys.stderr, end="")
        raise RuntimeError("Server failed to start")
    print(f"Server started on {SERVER_URL}")
    return proc


def test_dashboard_page():
    """Test that the dashboard page loads and shows key content."""
    with sync_playwright() as p:
        browser = p.chromium.launch(
            headless=True,
            args=["--disable-http2"],
        )
        context = browser.new_context()
        page = context.new_page()

        errors = []
        page.on("console", lambda msg: errors.append(msg.text) if msg.type == "error" else None)

        page.goto(f"{SERVER_URL}/dashboard", wait_until="domcontentloaded", timeout=15000)
        page.wait_for_timeout(2000)

        content = page.content()
        print(f"Dashboard HTML length: {len(content)}")

        title = page.title()
        print(f"Dashboard title: {title}")

        h1 = page.locator("h1").first
        if h1.count() > 0:
            print(f"First h1: {h1.inner_text()}")

        body_text = page.locator("body").inner_text()
        print(f"Body text preview: {body_text[:300]}")

        if errors:
            print(f"Console errors: {errors[:5]}")
        else:
            print("No console errors on dashboard")

        # Navigate away first to stop any periodic timers
        page.goto("about:blank")
        page.wait_for_timeout(500)
        context.close()
        browser.close()


def test_tasks_page():
    """Test that the tasks page loads."""
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        context = browser.new_context()
        page = context.new_page()

        errors = []
        page.on("console", lambda msg: errors.append(msg.text) if msg.type == "error" else None)

        page.goto(f"{SERVER_URL}/tasks", wait_until="domcontentloaded", timeout=30000)
        page.wait_for_timeout(3000)

        content = page.content()
        print(f"Tasks page HTML length: {len(content)}")

        title = page.title()
        print(f"Tasks title: {title}")

        body_text = page.locator("body").inner_text()
        print(f"Tasks body preview: {body_text[:300]}")

        if errors:
            print(f"Console errors on tasks: {errors[:5]}")
        else:
            print("No console errors on tasks page")

        # Navigate away first to stop any periodic timers/fetch calls
        page.goto("about:blank")
        page.wait_for_timeout(500)
        context.close()
        browser.close()


def test_settings_page():
    """Test that the settings page loads."""
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        context = browser.new_context()
        page = context.new_page()

        errors = []
        page.on("console", lambda msg: errors.append(msg.text) if msg.type == "error" else None)

        page.goto(f"{SERVER_URL}/settings", wait_until="domcontentloaded", timeout=15000)
        page.wait_for_timeout(2000)

        content = page.content()
        print(f"Settings page HTML length: {len(content)}")

        body_text = page.locator("body").inner_text()
        print(f"Settings body preview: {body_text[:300]}")

        if errors:
            print(f"Console errors on settings: {errors[:5]}")
        else:
            print("No console errors on settings page")

        page.goto("about:blank")
        page.wait_for_timeout(500)
        context.close()
        browser.close()


def test_logs_page():
    """Test that the logs page loads."""
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        context = browser.new_context()
        page = context.new_page()

        errors = []
        page.on("console", lambda msg: errors.append(msg.text) if msg.type == "error" else None)

        page.goto(f"{SERVER_URL}/logs", wait_until="domcontentloaded", timeout=15000)
        page.wait_for_timeout(2000)

        content = page.content()
        print(f"Logs page HTML length: {len(content)}")

        body_text = page.locator("body").inner_text()
        print(f"Logs body preview: {body_text[:300]}")

        if errors:
            print(f"Console errors on logs: {errors[:5]}")
        else:
            print("No console errors on logs page")

        page.goto("about:blank")
        page.wait_for_timeout(500)
        context.close()
        browser.close()


def test_env_check_page():
    """Test that the env-check page loads."""
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        context = browser.new_context()
        page = context.new_page()

        errors = []
        page.on("console", lambda msg: errors.append(msg.text) if msg.type == "error" else None)

        page.goto(f"{SERVER_URL}/env-check", wait_until="domcontentloaded", timeout=15000)
        page.wait_for_timeout(2000)

        content = page.content()
        print(f"Env-check page HTML length: {len(content)}")

        body_text = page.locator("body").inner_text()
        print(f"Env-check body preview: {body_text[:300]}")

        if errors:
            print(f"Console errors on env-check: {errors[:5]}")
        else:
            print("No console errors on env-check page")

        page.goto("about:blank")
        page.wait_for_timeout(500)
        context.close()
        browser.close()


def test_case_mapping_page():
    """Test that the case mapping page loads."""
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        context = browser.new_context()
        page = context.new_page()

        errors = []
        page.on("console", lambda msg: errors.append(msg.text) if msg.type == "error" else None)

        page.goto(f"{SERVER_URL}/case-mapping", wait_until="domcontentloaded", timeout=15000)
        page.wait_for_timeout(2000)

        content = page.content()
        print(f"Case mapping page HTML length: {len(content)}")

        body_text = page.locator("body").inner_text()
        print(f"Case mapping body preview: {body_text[:300]}")

        if errors:
            print(f"Console errors on case-mapping: {errors[:5]}")
        else:
            print("No console errors on case-mapping page")

        page.goto("about:blank")
        page.wait_for_timeout(500)
        context.close()
        browser.close()


def test_report_status_page():
    """Test that the report-status page loads."""
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        context = browser.new_context()
        page = context.new_page()

        errors = []
        page.on("console", lambda msg: errors.append(msg.text) if msg.type == "error" else None)

        page.goto(f"{SERVER_URL}/report-status", wait_until="domcontentloaded", timeout=15000)
        page.wait_for_timeout(2000)

        content = page.content()
        print(f"Report status page HTML length: {len(content)}")

        body_text = page.locator("body").inner_text()
        print(f"Report status body preview: {body_text[:300]}")

        if errors:
            print(f"Console errors on report-status: {errors[:5]}")
        else:
            print("No console errors on report-status page")

        page.goto("about:blank")
        page.wait_for_timeout(500)
        context.close()
        browser.close()


def test_api_status():
    """Test the /api/status endpoint returns JSON."""
    import urllib.request
    import json

    resp = urllib.request.urlopen(f"{SERVER_URL}/api/status", timeout=5)
    data = json.loads(resp.read())
    print(f"API /status response: success={data.get('success')}")
    assert data.get("success") is True, f"API status failed: {data}"


def test_api_dashboard_status():
    """Test the /api/dashboard/status endpoint."""
    import urllib.request
    import json

    resp = urllib.request.urlopen(f"{SERVER_URL}/api/dashboard/status", timeout=5)
    data = json.loads(resp.read())
    print(f"API /dashboard/status response: success={data.get('success')}")
    assert data.get("success") is True, f"Dashboard status failed: {data}"
    assert "system" in data.get("data", {}), "Dashboard data missing 'system' key"
    print(f"  system keys: {list(data.get('data', {}).get('system', {}).keys())}")


def test_api_tasks_list():
    """Test the /api/tasks endpoint."""
    import urllib.request
    import json

    resp = urllib.request.urlopen(f"{SERVER_URL}/api/tasks", timeout=5)
    data = json.loads(resp.read())
    print(f"API /tasks response: success={data.get('success')}, count={len(data.get('data', {}).get('tasks', []))}")


def test_api_config_get():
    """Test the /api/config GET endpoint."""
    import urllib.request
    import json

    resp = urllib.request.urlopen(f"{SERVER_URL}/api/config", timeout=5)
    data = json.loads(resp.read())
    print(f"API /config response: success={data.get('success')}")
    assert data.get("success") is True, f"Config API failed: {data}"


def main():
    print("=" * 60)
    print("Starting Test Executor E2E Tests with Playwright")
    print("=" * 60)

    proc = None
    try:
        print("\n[1] Starting Flask server...")
        proc = start_server()

        print("\n[2] Testing /api/status...")
        test_api_status()

        print("\n[3] Testing /api/dashboard/status...")
        test_api_dashboard_status()

        print("\n[4] Testing /api/tasks...")
        test_api_tasks_list()

        print("\n[5] Testing /api/config...")
        test_api_config_get()

        # Test page routes - do tasks/settings/logs BEFORE dashboard
        # to avoid dashboard's periodic requests blocking subsequent tests

        print("\n[6] Testing /tasks page...")
        test_tasks_page()

        print("\n    Waiting for server to settle...")
        time.sleep(2)

        print("\n[7] Testing /settings page...")
        test_settings_page()

        print("\n[8] Testing /logs page...")
        test_logs_page()

        print("\n[9] Testing /env-check page...")
        test_env_check_page()

        print("\n[10] Testing /case-mapping page...")
        test_case_mapping_page()

        print("\n[11] Testing /report-status page...")
        test_report_status_page()

        print("\n[12] Testing /dashboard page...")
        test_dashboard_page()

        print("\n" + "=" * 60)
        print("All E2E tests passed!")
        print("=" * 60)

    finally:
        if proc:
            print("\nStopping server...")
            proc.terminate()
            try:
                proc.wait(timeout=5)
            except subprocess.TimeoutExpired:
                proc.kill()


if __name__ == "__main__":
    main()
