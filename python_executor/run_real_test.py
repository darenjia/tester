"""
真实环境测试脚本
用于测试CANoe适配器在真实环境中的功能
"""

import sys
import os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from core.adapters.canoe import CANoeAdapter

def test_load_config_file():
    """测试加载指定配置文件"""
    config_path = r"D:\TAMS\DTTC_CONFIG\S59\BCANFD\SMFT\FDCANC_E\TestProjectFile\COMTest.cfg"
    adapter = CANoeAdapter()
    
    try:
        print("=" * 60)
        print("开始测试加载配置文件...")
        print(f"配置文件路径: {config_path}")
        print("=" * 60)
        
        # 1. 连接CANoe
        print("\n[1/4] 正在连接CANoe...")
        result = adapter.connect()
        if not result:
            print("❌ 连接CANoe失败!")
            return False
        print(f"✓ CANoe连接成功")
        print(f"  - 连接状态: {adapter.is_connected}")
        print(f"  - CANoe版本: {adapter.canoe_version}")
        
        # 2. 加载配置文件
        print(f"\n[2/4] 正在加载配置文件...")
        result = adapter.load_configuration(config_path)
        if not result:
            print(f"❌ 加载配置文件失败: {config_path}")
            return False
        print(f"✓ 配置文件加载成功")
        
        # 3. 验证配置已加载
        print(f"\n[3/4] 验证配置状态...")
        status = adapter.get_status()
        print(f"  - 工具类型: {status['tool_type']}")
        print(f"  - 连接状态: {status['is_connected']}")
        print(f"  - CANoe版本: {status['canoe_version']}")
        print(f"✓ 状态验证成功")
        
        # 4. 断开连接
        print(f"\n[4/4] 正在断开连接...")
        result = adapter.disconnect()
        if not result:
            print("❌ 断开连接失败!")
            return False
        print(f"✓ 断开连接成功")
        print(f"  - 连接状态: {adapter.is_connected}")
        
        print("\n" + "=" * 60)
        print("✅ 所有测试通过!")
        print("=" * 60)
        return True
        
    except Exception as e:
        print(f"\n❌ 测试失败: {e}")
        import traceback
        traceback.print_exc()
        return False
    finally:
        if adapter.is_connected:
            print("\n清理: 断开CANoe连接...")
            adapter.disconnect()

if __name__ == "__main__":
    success = test_load_config_file()
    sys.exit(0 if success else 1)
