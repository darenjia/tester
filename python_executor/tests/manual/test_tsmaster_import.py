"""
TSMasterAPI 导入验证测试
"""
import os
import sys

import pytest

pytestmark = pytest.mark.skip(reason="manual script; run directly instead of under pytest")

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))))


def test_tsmaster_api_import():
    """测试 TSMasterAPI 包导入"""
    try:
        import TSMasterAPI
        print("[OK] TSMasterAPI 导入成功")
        return True
    except ImportError as e:
        print(f"[FAIL] TSMasterAPI 导入失败: {e}")
        return False


def test_rpc_client():
    """测试 RPC 客户端标志"""
    try:
        from core.adapters.tsmaster.rpc_client import TSMASTER_RPC_AVAILABLE
        if TSMASTER_RPC_AVAILABLE:
            print("[OK] TSMASTER_RPC_AVAILABLE = True")
            return True
        else:
            print("[FAIL] TSMASTER_RPC_AVAILABLE = False")
            return False
    except ImportError as e:
        print(f"[FAIL] rpc_client 导入失败: {e}")
        return False


def test_adapter():
    """测试 TSMaster 适配器"""
    try:
        from core.adapters.tsmaster_adapter import TSMASTER_RPC_AVAILABLE
        if TSMASTER_RPC_AVAILABLE:
            print("[OK] tsmaster_adapter TSMASTER_RPC_AVAILABLE = True")
            return True
        else:
            print("[FAIL] tsmaster_adapter TSMASTER_RPC_AVAILABLE = False")
            return False
    except ImportError as e:
        print(f"[FAIL] tsmaster_adapter 导入失败: {e}")
        return False


if __name__ == "__main__":
    print("=" * 50)
    print("TSMasterAPI 导入验证")
    print("=" * 50)

    results = []
    print("\n[1] TSMasterAPI 包导入")
    results.append(test_tsmaster_api_import())

    print("\n[2] RPC 客户端")
    results.append(test_rpc_client())

    print("\n[3] TSMaster 适配器")
    results.append(test_adapter())

    print("\n" + "=" * 50)
    if all(results):
        print("全部测试通过 [OK]")
        sys.exit(0)
    else:
        print("部分测试失败 [FAIL]")
        sys.exit(1)
