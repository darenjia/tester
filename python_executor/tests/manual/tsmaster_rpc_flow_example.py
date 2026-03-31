"""
TSMaster RPC 调用流程示例

演示符合文档的完整 RPC 调用流程:
1. 初始化 TSMaster 库
2. 连接 RPC 客户端
3. 启动 Master 小程序 (app.run_form)
4. 启动仿真 (start_simulation)
5. 选择测试用例
6. 开始/停止测试
7. 停止仿真
8. 停止 Master 小程序 (app.stop_form)
9. 断开连接

运行方式:
    python tests/manual/tsmaster_rpc_flow_example.py

前置条件:
    1. TSMaster 已启动
    2. 已安装 TSMasterAPI: pip install TSMasterAPI
"""

import sys
import os
import time

# 添加项目根目录到路径
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from core.adapters.tsmaster_adapter import TSMasterAdapter
from core.tsmaster_test_engine import TSMasterTestEngine, TestExecutionConfig


def example_1_basic_rpc_flow():
    """
    示例1: 基础 RPC 调用流程

    展示完整的 RPC 调用步骤
    """
    print("=" * 80)
    print("示例1: 基础 RPC 调用流程")
    print("=" * 80)

    # 创建适配器，配置 RPC 模式
    config = {
        "use_rpc": True,
        "fallback_to_traditional": False,  # RPC 模式失败时不回退
        "master_form_name": "C 代码编辑器 [Master]"  # Master 小程序名称
    }

    adapter = TSMasterAdapter(config)

    try:
        # Step 1: 连接
        print("\n[Step 1] 连接 TSMaster...")
        if not adapter.connect():
            print("连接失败!")
            return False
        print("连接成功!")

        # Step 2: 启动 Master 小程序 (RPC 调用)
        print("\n[Step 2] 启动 Master 小程序...")
        if not adapter.start_master_form("C 代码编辑器 [Master]"):
            print("启动 Master 小程序失败!")
        else:
            print("Master 小程序已启动!")

        # Step 3: 启动仿真
        print("\n[Step 3] 启动总线仿真...")
        if not adapter.start_test():
            print("启动仿真失败!")
        else:
            print("总线仿真已启动!")

        # Step 4: 选择测试用例
        print("\n[Step 4] 选择测试用例...")
        if adapter._write_system_var("TestSystem.SelectCases", "TG1_TC1=1,TG1_TC2=1"):
            print("测试用例选择成功!")
        else:
            print("测试用例选择失败!")

        # Step 5: 开始测试
        print("\n[Step 5] 开始测试...")
        if adapter._write_system_var("TestSystem.Controller", "1"):
            print("测试已开始!")
        else:
            print("开始测试失败!")

        # 等待测试执行
        print("\n等待测试执行...")
        time.sleep(5)

        # Step 6: 停止测试
        print("\n[Step 6] 停止测试...")
        if adapter._write_system_var("TestSystem.Controller", "0"):
            print("测试已停止!")
        else:
            print("停止测试失败!")

        # Step 7: 停止仿真
        print("\n[Step 7] 停止仿真...")
        if not adapter.stop_test():
            print("停止仿真失败!")
        else:
            print("总线仿真已停止!")

        # Step 8: 停止 Master 小程序
        print("\n[Step 8] 停止 Master 小程序...")
        if not adapter.stop_master_form("C 代码编辑器 [Master]"):
            print("停止 Master 小程序失败!")
        else:
            print("Master 小程序已停止!")

        return True

    except Exception as e:
        print(f"异常: {e}")
        return False

    finally:
        # Step 9: 断开连接
        print("\n[Step 9] 断开连接...")
        adapter.disconnect()
        print("连接已断开!")


def example_2_use_test_engine():
    """
    示例2: 使用 TSMasterTestEngine 执行测试步骤

    通过 TestExecutionConfig 配置完整的执行流程
    """
    print("\n" + "=" * 80)
    print("示例2: 使用 TSMasterTestEngine")
    print("=" * 80)

    # 创建测试执行配置
    config = TestExecutionConfig(
        use_rpc=True,
        fallback_to_traditional=False,
        project_path=None,  # 如果有 .tproj 文件路径则填入
        auto_start_simulation=True,
        auto_stop_simulation=True,
        master_form_name="C 代码编辑器 [Master]"  # Master 小程序名称
    )

    # 创建测试引擎
    engine = TSMasterTestEngine()

    # 创建测试步骤
    from core.tsmaster_test_engine import TestStep

    steps = [
        TestStep(
            id="step1",
            name="设置 KL15 信号",
            type="signal_set",
            parameters={"signal_name": "KL15", "value": 1}
        ),
        TestStep(
            id="step2",
            name="等待信号稳定",
            type="wait",
            parameters={"duration": 2000}  # 2秒
        ),
        TestStep(
            id="step3",
            name="检查车速信号",
            type="signal_check",
            parameters={"signal_name": "VehicleSpeed", "expected_value": 0, "tolerance": 1}
        ),
    ]

    try:
        # 执行测试步骤
        result = engine.execute_steps(steps, config)

        print(f"\n执行结果:")
        print(f"  状态: {result.status}")
        print(f"  总步骤: {result.summary.get('total', 0)}")
        print(f"  通过: {result.summary.get('passed', 0)}")
        print(f"  失败: {result.summary.get('failed', 0)}")
        print(f"  跳过: {result.summary.get('skipped', 0)}")

        return result.status == "completed"

    except Exception as e:
        print(f"执行异常: {e}")
        return False


def example_3_direct_rpc_client():
    """
    示例3: 直接使用 RPC 客户端

    展示更底层的 RPC 调用方式
    """
    print("\n" + "=" * 80)
    print("示例3: 直接使用 RPC 客户端")
    print("=" * 80)

    try:
        from core.adapters.tsmaster.rpc_client import TSMasterRPCClient

        # 创建 RPC 客户端
        client = TSMasterRPCClient(app_name="TSMasterTest")

        # Step 1: 初始化
        print("\n[Step 1] 初始化 TSMaster 库...")
        if not client.initialize():
            print("初始化失败!")
            return False
        print("初始化成功!")

        # Step 2: 连接
        print("\n[Step 2] 连接 TSMaster...")
        if not client.connect():
            print("连接失败!")
            return False
        print("连接成功!")

        # Step 3: 启动 Master 小程序
        print("\n[Step 3] 启动 Master 小程序...")
        if client.run_form("C 代码编辑器 [Master]"):
            print("Master 小程序已启动!")
        else:
            print("启动失败!")

        # Step 4: 启动仿真
        print("\n[Step 4] 启动仿真...")
        if client.start_simulation():
            print("仿真已启动!")
        else:
            print("启动仿真失败!")

        # 示例: 设置系统变量
        print("\n[Example] 设置系统变量...")
        client.write_system_var("TestSystem.SelectCases", "TG1_TC1=1")
        client.write_system_var("TestSystem.Controller", "1")

        # 等待
        time.sleep(2)

        # 停止测试
        client.write_system_var("TestSystem.Controller", "0")

        # Step 5: 停止仿真
        print("\n[Step 5] 停止仿真...")
        if client.stop_simulation():
            print("仿真已停止!")
        else:
            print("停止仿真失败!")

        # Step 6: 停止 Master 小程序
        print("\n[Step 6] 停止 Master 小程序...")
        if client.stop_form("C 代码编辑器 [Master]"):
            print("Master 小程序已停止!")
        else:
            print("停止失败!")

        # Step 7: 断开连接
        print("\n[Step 7] 断开连接...")
        client.disconnect()
        print("连接已断开!")

        # Step 8: 释放资源
        print("\n[Step 8] 释放资源...")
        client.finalize()
        print("资源已释放!")

        return True

    except ImportError as e:
        print(f"导入失败: {e}")
        print("请确保已安装 TSMasterAPI: pip install TSMasterAPI")
        return False
    except Exception as e:
        print(f"异常: {e}")
        return False


def main():
    """主函数"""
    print("TSMaster RPC 调用流程示例")
    print("=" * 80)

    # 检查 TSMasterAPI 是否可用
    try:
        from TSMasterAPI import *
        print("TSMasterAPI 已安装 ✓")
    except ImportError:
        print("TSMasterAPI 未安装")
        print("请运行: pip install TSMasterAPI")
        return 1

    # 运行示例
    print("\n" + "=" * 80)
    print("按 Enter 运行示例1 (基础 RPC 流程)...")
    input()

    success = example_1_basic_rpc_flow()

    if success:
        print("\n" + "=" * 80)
        print("示例1 执行完成!")
        print("=" * 80)

    return 0 if success else 1


if __name__ == "__main__":
    sys.exit(main())
