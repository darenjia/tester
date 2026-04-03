"""Simple Flask server runner for Playwright tests."""
import os
import sys

# Add project root to path
project_root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(0, project_root)

from web.server import app

if __name__ == "__main__":
    app.run(host="127.0.0.1", port=5200, debug=False, use_reloader=False, threaded=True)
