from __future__ import annotations

import json

from core.runtime_operations import get_runtime_housekeeping_service


def main() -> None:
    print(json.dumps(get_runtime_housekeeping_service().run(), ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
