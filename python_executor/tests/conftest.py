from __future__ import annotations

import os
from pathlib import Path

import pytest


@pytest.fixture(scope="session")
def config_path() -> str:
    """Provide a real CANoe config path for manual tests, or skip safely."""
    raw_path = os.environ.get("CANOE_CONFIG_PATH", "").strip()
    if raw_path:
        candidate = Path(raw_path).expanduser()
        if candidate.is_file():
            return str(candidate)

    pytest.skip(
        "manual CANoe config path is unavailable; set CANOE_CONFIG_PATH to a real .cfg file"
    )

