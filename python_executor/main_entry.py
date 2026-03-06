"""
PyInstaller打包入口文件
处理资源路径和初始化
"""

import sys
import os

def get_resource_path(relative_path):
    """
    获取资源文件的绝对路径
    在开发环境和打包后的环境中都能正常工作
    """
    if hasattr(sys, '_MEIPASS'):
        # PyInstaller打包后的临时目录
        base_path = sys._MEIPASS
    else:
        # 开发环境
        base_path = os.path.abspath(os.path.dirname(__file__))
    
    return os.path.join(base_path, relative_path)

def setup_environment():
    """设置运行环境"""
    # 设置工作目录
    if hasattr(sys, '_MEIPASS'):
        # 打包后的环境
        os.chdir(sys._MEIPASS)
    
    # 添加必要的路径到sys.path
    current_dir = os.path.dirname(os.path.abspath(__file__))
    if current_dir not in sys.path:
        sys.path.insert(0, current_dir)
    
    # 设置环境变量
    os.environ['PYTHONEXECUTOR_HOME'] = current_dir
    
    # 确保配置目录存在
    config_dir = get_resource_path('config')
    if not os.path.exists(config_dir):
        os.makedirs(config_dir)
    
    # 创建默认配置文件（如果不存在）
    config_file = os.path.join(config_dir, 'executor_config.json')
    if not os.path.exists(config_file):
        create_default_config(config_file)

def create_default_config(config_path):
    """创建默认配置文件"""
    default_config = {
        "server": {
            "host": "0.0.0.0",
            "port": 5000,
            "debug": False
        },
        "websocket": {
            "cors_allowed_origins": "*",
            "ping_timeout": 60,
            "ping_interval": 25
        },
        "executor": {
            "max_concurrent_tasks": 5,
            "task_timeout": 3600,
            "auto_cleanup": True,
            "cleanup_interval": 300
        },
        "logging": {
            "level": "INFO",
            "format": "%(asctime)s - %(name)s - %(levelname)s - %(message)s",
            "file": "logs/executor.log",
            "max_bytes": 10485760,
            "backup_count": 5
        },
        "canoe": {
            "enabled": True,
            "auto_discover": True,
            "default_timeout": 300
        },
        "tsmaster": {
            "enabled": True,
            "rpc_host": "localhost",
            "rpc_port": 50051,
            "default_timeout": 300
        }
    }
    
    import json
    with open(config_path, 'w', encoding='utf-8') as f:
        json.dump(default_config, f, indent=4, ensure_ascii=False)

def main():
    """主函数"""
    # 设置环境
    setup_environment()
    
    # 导入并运行主程序
    try:
        from main import main as main_app
        main_app()
    except ImportError as e:
        print(f"导入主程序失败: {e}")
        print("尝试备用导入...")
        try:
            from app import app, socketio
            import config.settings as settings
            
            print("=" * 60)
            print("Python执行器服务启动中...")
            print("=" * 60)
            print(f"服务地址: http://{settings.SERVER_HOST}:{settings.SERVER_PORT}")
            print(f"API文档: http://{settings.SERVER_HOST}:{settings.SERVER_PORT}/api/docs")
            print("=" * 60)
            
            socketio.run(
                app,
                host=settings.SERVER_HOST,
                port=settings.SERVER_PORT,
                debug=settings.DEBUG
            )
        except Exception as e2:
            print(f"备用导入也失败: {e2}")
            sys.exit(1)
    except Exception as e:
        print(f"启动失败: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)

if __name__ == '__main__':
    main()
