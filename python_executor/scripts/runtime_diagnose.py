from __future__ import annotations

from core.runtime_operations import dump_runtime_diagnose_json


def main() -> None:
    print(dump_runtime_diagnose_json())


if __name__ == "__main__":
    main()
