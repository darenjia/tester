from __future__ import annotations

import subprocess
import sys
from pathlib import Path


def test_manual_scripts_do_not_return_values_to_pytest():
    repo_root = Path(__file__).resolve().parents[1]
    result = subprocess.run(
        [
            sys.executable,
            "-m",
            "pytest",
            "-q",
            "-W",
            "error::pytest.PytestReturnNotNoneWarning",
            "test_com_reset.py",
            "tests/manual/test_all_modules.py",
            "tests/manual/test_tsmaster_import.py",
        ],
        cwd=repo_root,
        capture_output=True,
        text=True,
        check=False,
    )

    output = (result.stdout + result.stderr).replace("\r", "")
    assert result.returncode == 0, output
    assert "PytestReturnNotNoneWarning" not in output, output
    assert "skipped" in output.lower(), output
