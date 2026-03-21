"""
简单的CANoe配置加载测试脚本
"""
import logging
import sys
import os

# 配置日志
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)

# 添加项目路径
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from core.adapters.canoe import CANoeAdapter

def test_load_config(config_path: str):
    """测试加载配置文件"""
    print(f"\n{'='*60}")
    print(f"测试配置文件: {config_path}")
    print(f"{'='*60}\n")

    # 检查文件是否存在
    if not os.path.exists(config_path):
        print(f"错误: 配置文件不存在: {config_path}")
        return False

    # 创建适配器
    print("1. 创建CANoe适配器...")
    adapter = CANoeAdapter()

    # 连接CANoe
    print("2. 连接CANoe...")
    if not adapter.connect():
        print(f"连接失败: {adapter.error_message}")
        return False
    print(f"   连接成功! CANoe版本: {adapter.canoe_version}")

    # 加载配置
    print("3. 加载配置文件...")
    try:
        result = adapter.load_configuration(config_path)
        if result:
            print("   配置加载成功!")
        else:
            print(f"   配置加载失败: {adapter.error_message}")
            if adapter._canoe_wrapper and adapter._canoe_wrapper.last_error:
                print(f"   COM错误: {adapter._canoe_wrapper.last_error}")
    except Exception as e:
        print(f"   配置加载异常: {e}")
        result = False

    # 断开连接
    print("4. 断开连接...")
    adapter.disconnect()
    print("   已断开\n")

    return result


if __name__ == "__main__":
    # 测试配置文件路径 - 请修改为你的配置文件路径
    config_path = r"D:\TAMS\DTTC_CONFIG\S595\BCAN\MFTS\DNMAS_E\TestProjectFile\AUTOSARNMTest.cfg"

    # 或者从命令行参数获取
    if len(sys.argv) > 1:
        config_path = sys.argv[1]

    test_load_config(config_path)