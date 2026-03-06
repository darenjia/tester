"""
真实环境测试脚本 V3 - 带详细调试
用于测试CANoe适配器在真实环境中的功能
"""

import sys
import os
import time
import logging

# 设置详细日志
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

print(f"[{time.strftime('%H:%M:%S')}] 开始导入模块...")

from core.adapters.canoe import CANoeAdapter
from core.adapters.canoe.com_wrapper import CANoeCOMWrapper

print(f"[{time.strftime('%H:%M:%S')}] 模块导入完成")

def test_basic_connection():
    """测试基本连接功能"""
    print("\n" + "=" * 60)
    print("测试1: 基本连接功能")
    print("=" * 60)
    
    wrapper = CANoeCOMWrapper()
    
    try:
        print(f"[{time.strftime('%H:%M:%S')}] 正在连接CANoe...")
        result = wrapper.connect()
        print(f"[{time.strftime('%H:%M:%S')}] 连接结果: {result}")
        print(f"[{time.strftime('%H:%M:%S')}] 连接状态: {wrapper.is_connected}")
        print(f"[{time.strftime('%H:%M:%S')}] CANoe版本: {wrapper.version}")
        
        if wrapper.is_connected:
            print(f"[{time.strftime('%H:%M:%S')}] 正在断开连接...")
            wrapper.disconnect()
            print(f"[{time.strftime('%H:%M:%S')}] 断开完成")
        
        return True
    except Exception as e:
        print(f"[{time.strftime('%H:%M:%S')}] ❌ 测试失败: {e}")
        import traceback
        traceback.print_exc()
        return False

def test_load_config_file():
    """测试加载指定配置文件"""
    config_path = r"D:\TAMS\DTTC_CONFIG\S59\BCANFD\SMFT\FDCANC_E\TestProjectFile\COMTest.cfg"
    adapter = CANoeAdapter()
    
    try:
        print("\n" + "=" * 60)
        print("测试2: 加载配置文件")
        print(f"配置文件路径: {config_path}")
        print("=" * 60)
        
        # 1. 连接CANoe
        print(f"\n[{time.strftime('%H:%M:%S')}] [1/4] 正在连接CANoe...")
        result = adapter.connect()
        if not result:
            print("❌ 连接CANoe失败!")
            return False
        print(f"[{time.strftime('%H:%M:%S')}] ✓ CANoe连接成功")
        print(f"  - 连接状态: {adapter.is_connected}")
        print(f"  - CANoe版本: {adapter.canoe_version}")
        
        # 2. 加载配置文件
        print(f"\n[{time.strftime('%H:%M:%S')}] [2/4] 正在加载配置文件...")
        print(f"  超时设置: {adapter.open_timeout}秒")
        
        # 使用线程来避免阻塞
        import threading
        result_container = [None]
        exception_container = [None]
        
        def load_config():
            try:
                result_container[0] = adapter.load_configuration(config_path)
            except Exception as e:
                exception_container[0] = e
        
        thread = threading.Thread(target=load_config)
        thread.daemon = True
        thread.start()
        thread.join(timeout=35)  # 等待35秒
        
        if thread.is_alive():
            print(f"[{time.strftime('%H:%M:%S')}] ⚠️ 加载配置文件超时（超过35秒）")
            print(f"[{time.strftime('%H:%M:%S')}] 这可能是因为CANoe正在显示对话框或配置加载需要时间")
            return False
        
        if exception_container[0]:
            raise exception_container[0]
        
        result = result_container[0]
        if not result:
            print(f"[{time.strftime('%H:%M:%S')}] ❌ 加载配置文件失败: {config_path}")
            return False
        print(f"[{time.strftime('%H:%M:%S')}] ✓ 配置文件加载成功")
        
        # 3. 验证配置已加载
        print(f"\n[{time.strftime('%H:%M:%S')}] [3/4] 验证配置状态...")
        status = adapter.get_status()
        print(f"  - 工具类型: {status['tool_type']}")
        print(f"  - 连接状态: {status['is_connected']}")
        print(f"  - CANoe版本: {status['canoe_version']}")
        print(f"[{time.strftime('%H:%M:%S')}] ✓ 状态验证成功")
        
        # 4. 断开连接
        print(f"\n[{time.strftime('%H:%M:%S')}] [4/4] 正在断开连接...")
        result = adapter.disconnect()
        if not result:
            print(f"[{time.strftime('%H:%M:%S')}] ❌ 断开连接失败!")
            return False
        print(f"[{time.strftime('%H:%M:%S')}] ✓ 断开连接成功")
        print(f"  - 连接状态: {adapter.is_connected}")
        
        print("\n" + "=" * 60)
        print(f"[{time.strftime('%H:%M:%S')}] ✅ 所有测试通过!")
        print("=" * 60)
        return True
        
    except Exception as e:
        print(f"\n[{time.strftime('%H:%M:%S')}] ❌ 测试失败: {e}")
        import traceback
        traceback.print_exc()
        return False
    finally:
        if adapter.is_connected:
            print(f"\n[{time.strftime('%H:%M:%S')}] 清理: 断开CANoe连接...")
            adapter.disconnect()

if __name__ == "__main__":
    print(f"[{time.strftime('%H:%M:%S')}] 脚本启动")
    
    # 先测试基本连接
    success1 = test_basic_connection()
    
    # 再测试加载配置文件
    success2 = test_load_config_file()
    
    print(f"\n[{time.strftime('%H:%M:%S')}] 脚本结束")
    print(f"基本连接测试: {'✅ 通过' if success1 else '❌ 失败'}")
    print(f"配置文件加载测试: {'✅ 通过' if success2 else '❌ 失败'}")
    
    sys.exit(0 if (success1 and success2) else 1)
