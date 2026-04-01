from __future__ import annotations

import os
import subprocess
import sys
from pathlib import Path


def test_manual_canoe_tests_skip_without_real_config(tmp_path):
    repo_root = Path(__file__).resolve().parents[1]
    env = os.environ.copy()
    env["CANOE_CONFIG_PATH"] = str(tmp_path / "missing" / "canoe.cfg")

    result = subprocess.run(
        [
            sys.executable,
            "-m",
            "pytest",
            "tests/manual/test_load_config.py",
            "tests/manual/test_canoe_config_internal.py",
            "-q",
        ],
        cwd=repo_root,
        env=env,
        capture_output=True,
        text=True,
        check=False,
    )

    output = (result.stdout + result.stderr).replace("\r", "")
    assert result.returncode == 0, output
    assert "fixture 'config_path' not found" not in output
    assert "ERROR at setup" not in output
    assert "skipped" in output

