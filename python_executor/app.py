"""
测试执行器主入口
使用 PyWebView 创建桌面应用程序
"""
import os
import sys
import threading
import time

# 添加当前目录到路径
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import webview
from web.server import app as flask_app
from config.settings import get_config
from utils.logger import get_logger, setup_logging
from core.task_executor_production import get_task_executor
task_executor = get_task_executor()
from core.task_scheduler import task_scheduler


class TestExecutorApp:
    """测试执行器应用程序"""
    
    def __init__(self):
        self.window = None
        self.server_thread = None
        self.server_running = False
        self.server_port = 0
        self.logger = None
        
    def start_server(self):
        """在后台线程启动 Flask 服务器"""
        try:
            from werkzeug.serving import make_server
            
            # 从配置获取端口
            config = get_config()
            self.server_port = config.get('http.port', 8180)
            host = config.get('http.host', '127.0.0.1')
            
            # 创建服务器
            self.server = make_server(host, self.server_port, flask_app)
            self.server_running = True
            
            # 初始化日志
            config = get_config()
            setup_logging(
                log_dir=config.get('logging.log_dir', 'logs'),
                level=config.get('logging.level', 'INFO')
            )
            self.logger = get_logger()
            self.logger.info(f"Flask 服务器启动在端口 {self.server_port}")
            
            # 启动任务执行器和调度器
            task_executor.start()
            task_scheduler.start()
            self.logger.info("任务执行器和调度器已启动")
            
            # 启动服务器
            self.server.serve_forever()
        except Exception as e:
            print(f"服务器启动失败: {e}")
            self.server_running = False
            raise
    
    def stop_server(self):
        """停止 Flask 服务器"""
        if self.server_running and self.server:
            self.server.shutdown()
            self.server_running = False
            if self.logger:
                self.logger.info("Flask 服务器已停止")
    
    def on_closed(self):
        """窗口关闭时的回调"""
        if self.logger:
            self.logger.info("应用程序关闭")
        # 停止任务执行器和调度器
        task_scheduler.stop()
        task_executor.stop()
        self.stop_server()
    
    def create_window(self):
        """创建主窗口"""
        # 等待服务器启动
        timeout = 10
        while self.server_port == 0 and timeout > 0:
            time.sleep(0.5)
            timeout -= 0.5
        
        if self.server_port == 0:
            print("服务器启动超时")
            sys.exit(1)
        
        # 获取配置
        config = get_config()
        device_name = config.get('device.device_name', '测试执行器')
        
        # 创建窗口
        self.window = webview.create_window(
            title=f'{device_name} - 测试执行器',
            url=f'http://127.0.0.1:{self.server_port}',
            width=1200,
            height=800,
            min_size=(900, 600),
            resizable=True,
            fullscreen=False,
            confirm_close=True,
            text_select=True
        )
        
        # 设置关闭回调
        self.window.events.closed += self.on_closed
        
        # 启动 WebView
        webview.start(debug=False)
    
    def run(self):
        """运行应用程序"""
        # 在后台线程启动 Flask 服务器
        self.server_thread = threading.Thread(target=self.start_server)
        self.server_thread.daemon = True
        self.server_thread.start()
        
        # 在主线程创建窗口
        self.create_window()


def main():
    """主函数"""
    app = TestExecutorApp()
    app.run()


if __name__ == '__main__':
    main()
