"""
开发调试入口
用于直接运行 Flask 应用进行开发调试
"""
import os
import sys

# 添加当前目录到路径
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from web.server import app
from config.settings import get_config
from utils.logger import setup_logging

if __name__ == '__main__':
    # 初始化配置和日志
    config = get_config()
    setup_logging(
        log_dir=config.get('logging.log_dir', 'logs'),
        level=config.get('logging.level', 'INFO')
    )
    
    print("=" * 50)
    print("测试执行器 - 开发模式")
    print("=" * 50)
    print(f"访问地址: http://127.0.0.1:5000")
    print("=" * 50)
    
    # 运行 Flask 开发服务器
    app.run(host='127.0.0.1', port=5000, debug=True, use_reloader=True)
