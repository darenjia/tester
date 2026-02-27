"""
运行脚本 - 启动Python执行器
"""
import sys
import os

# 添加当前目录到Python路径
current_dir = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, current_dir)

def main():
    """主函数"""
    try:
        # 检查依赖
        print("检查依赖...")
        
        # 检查Python版本
        if sys.version_info < (3, 8):
            print("❌ 需要Python 3.8或更高版本")
            return False
        
        # 检查必需的模块
        required_modules = [
            ('flask', 'Flask'),
            ('flask_socketio', 'SocketIO'),
        ]
        
        missing_modules = []
        for module_name, class_name in required_modules:
            try:
                if module_name == 'flask_socketio':
                    from flask_socketio import SocketIO
                else:
                    __import__(module_name)
            except ImportError:
                missing_modules.append(module_name)
        
        if missing_modules:
            print(f"❌ 缺少依赖模块: {', '.join(missing_modules)}")
            print("请运行: pip install -r requirements.txt")
            return False
        
        print("✅ 依赖检查通过")
        
        # 检查CANoe环境（可选）
        try:
            import win32com.client
            print("✅ CANoe COM接口可用")
        except ImportError:
            print("⚠️  pywin32未安装，CANoe功能不可用")
        
        # 检查TSMaster环境（可选）
        try:
            from TSMaster import initialize_lib_tsmaster, get_application
            print("✅ TSMaster Python API可用")
        except ImportError:
            print("⚠️  TSMaster Python API未安装，TSMaster功能不可用")
        
        # 启动执行器
        print("\n启动Python执行器...")
        
        from main import PythonExecutor
        
        executor = PythonExecutor()
        executor.run()
        
    except KeyboardInterrupt:
        print("\n用户中断，正在退出...")
    except Exception as e:
        print(f"\n❌ 启动失败: {e}")
        import traceback
        traceback.print_exc()
        return False
    
    return True

if __name__ == '__main__':
    success = main()
    sys.exit(0 if success else 1)