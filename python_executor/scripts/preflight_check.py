from __future__ import annotations

import json

from core.runtime_operations import get_preflight_checker


def main() -> None:
    print(json.dumps(get_preflight_checker().run().to_dict(), ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
