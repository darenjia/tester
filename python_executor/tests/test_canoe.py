import win32com.client
import time

print("尝试直接启动新进程 (Dispatch)...")

try:
    # 使用 Dispatch 强制启动一个新的 CANoe 实例
    # 这一步不需要你预先手动打开 CANoe
    canoe_app = win32com.client.Dispatch("CANoe.Application")
    
    print("CANoe 正在启动，请稍候...")
    time.sleep(5) # 给 CANoe 17 一些启动时间
    
    print(f"【成功】已连接到版本: {canoe_app.Version}")

except Exception as e:
    print(f"【失败】错误详情: {e}")
    print("\n终极排查：请检查你的 Python 和 CANoe 架构是否一致。")
    print("1. 检查 Python 是 32位 还是 64位。")
    print("2. CANoe 17 通常是 64位 软件。")
    print("   -> 如果你的 Python 是 32位，将无法连接 64位的 CANoe。")
    print("   -> 解决方法：安装 64位的 Python。")
