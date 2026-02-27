"""
Python执行器主应用
集成WebSocket服务端和任务执行器
"""
import json
import time
import signal
import sys
from datetime import datetime
from typing import Dict, Any

from flask import Flask, request
from flask_socketio import SocketIO, emit

from config.settings import settings
from utils.logger import get_logger, setup_logger
from utils.exceptions import ExecutorException
from models.task import Task, Message
from models.result import StatusUpdate, LogEntry
from core.task_executor import TaskExecutor
from websocket.client import WebSocketServer

# 设置日志
logger = setup_logger("executor")

class PythonExecutor:
    """Python执行器主类"""
    
    def __init__(self):
        self.app = Flask(__name__)
        self.app.config['SECRET_KEY'] = 'python-executor-secret-key'
        
        # 初始化SocketIO
        self.socketio = SocketIO(
            self.app,
            cors_allowed_origins="*",
            async_mode='threading',
            logger=False,
            engineio_logger=False
        )
        
        # 初始化组件
        self.websocket_server = WebSocketServer()
        self.task_executor = None
        self.clients = {}  # sid -> client_info
        self.running = True
        
        # 设置路由和事件处理器
        self._setup_routes()
        self._setup_websocket_handlers()
        
        # 设置信号处理
        signal.signal(signal.SIGINT, self._signal_handler)
        signal.signal(signal.SIGTERM, self._signal_handler)
    
    def _setup_routes(self):
        """设置HTTP路由"""
        @self.app.route('/')
        def index():
            return {
                'name': 'Python执行器',
                'version': '1.0.0',
                'status': 'running',
                'timestamp': datetime.now().isoformat()
            }
        
        @self.app.route('/health')
        def health_check():
            return {
                'status': 'healthy',
                'timestamp': datetime.now().isoformat(),
                'clients': len(self.clients),
                'current_task': self.task_executor.get_current_status() if self.task_executor else None
            }
        
        @self.app.route('/status')
        def status():
            return {
                'clients': len(self.clients),
                'running': self.running,
                'uptime': time.time() - getattr(self, 'start_time', time.time()),
                'current_task': self.task_executor.get_current_status() if self.task_executor else None
            }
    
    def _setup_websocket_handlers(self):
        """设置WebSocket事件处理器"""
        
        @self.socketio.on('connect')
        def handle_connect():
            """处理客户端连接"""
            sid = request.sid
            client_info = {
                'sid': sid,
                'connected_at': datetime.now(),
                'last_heartbeat': datetime.now()
            }
            self.clients[sid] = client_info
            
            logger.info(f"客户端连接: {sid}")
            
            # 发送欢迎消息
            emit('welcome', {
                'message': 'Python执行器已连接',
                'executor_info': {
                    'version': '1.0.0',
                    'status': 'ready'
                },
                'timestamp': int(time.time() * 1000)
            })
        
        @self.socketio.on('disconnect')
        def handle_disconnect():
            """处理客户端断开连接"""
            sid = request.sid
            if sid in self.clients:
                client_info = self.clients.pop(sid)
                connected_time = datetime.now() - client_info['connected_at']
                logger.info(f"客户端断开连接: {sid}, 连接时长: {connected_time}")
        
        @self.socketio.on('heartbeat')
        def handle_heartbeat(data):
            """处理心跳消息"""
            sid = request.sid
            if sid in self.clients:
                self.clients[sid]['last_heartbeat'] = datetime.now()
                logger.debug(f"收到心跳: {sid}")
                
                # 回复心跳
                emit('heartbeat_response', {
                    'timestamp': int(time.time() * 1000)
                })
        
        @self.socketio.on('message')
        def handle_message(data):
            """处理通用消息"""
            try:
                message = Message.from_dict(data)
                logger.info(f"收到消息: {message.type} from {request.sid}")
                
                # 处理不同类型的消息
                if message.type == "TASK_DISPATCH":
                    self._handle_task_dispatch(message, request.sid)
                elif message.type == "TASK_CANCEL":
                    self._handle_task_cancel(message, request.sid)
                else:
                    logger.warning(f"未处理的消息类型: {message.type}")
                
                # 发送确认回复
                emit('message_response', {
                    'status': 'received',
                    'type': message.type,
                    'timestamp': int(time.time() * 1000)
                })
                
            except Exception as e:
                logger.error(f"处理消息失败: {e}")
                emit('error', {
                    'error': str(e),
                    'timestamp': int(time.time() * 1000)
                })
    
    def _handle_task_dispatch(self, message: Message, client_sid: str):
        """处理任务下发"""
        try:
            logger.info(f"收到任务下发: {message.task_id}")
            
            # 创建任务对象
            task_data = message.payload or {}
            task_data.update({
                'taskId': message.task_id,
                'deviceId': message.device_id
            })
            
            task = Task.from_dict(task_data)
            
            # 初始化任务执行器（如果还没初始化）
            if not self.task_executor:
                self.task_executor = TaskExecutor(
                    message_sender=lambda msg: self._send_message_to_client(client_sid, msg)
                )
            
            # 执行任务
            success = self.task_executor.execute_task(task)
            
            if success:
                logger.info(f"任务启动成功: {task.task_id}")
            else:
                logger.warning(f"任务启动失败: {task.task_id}")
                
        except Exception as e:
            logger.error(f"处理任务下发失败: {e}")
            # 发送错误消息
            self._send_message_to_client(client_sid, {
                'type': 'TASK_STATUS',
                'taskId': message.task_id,
                'status': 'failed',
                'message': f"任务下发失败: {e}",
                'timestamp': int(time.time() * 1000)
            })
    
    def _handle_task_cancel(self, message: Message, client_sid: str):
        """处理任务取消"""
        try:
            logger.info(f"收到任务取消: {message.task_id}")
            
            if self.task_executor:
                success = self.task_executor.cancel_task()
                if success:
                    logger.info(f"任务取消成功: {message.task_id}")
                else:
                    logger.warning(f"任务取消失败: {message.task_id}")
            else:
                logger.warning("没有正在执行的任务")
                
        except Exception as e:
            logger.error(f"处理任务取消失败: {e}")
    
    def _send_message_to_client(self, client_sid: str, message: Dict[str, Any]):
        """发送消息到指定客户端"""
        if client_sid in self.clients:
            try:
                self.socketio.emit('message', message, room=client_sid)
                logger.debug(f"发送消息到 {client_sid}: {message.get('type')}")
            except Exception as e:
                logger.error(f"发送消息失败: {e}")
        else:
            logger.warning(f"客户端不存在: {client_sid}")
    
    def _signal_handler(self, signum, frame):
        """信号处理"""
        logger.info(f"收到信号 {signum}，正在关闭执行器...")
        self.shutdown()
        sys.exit(0)
    
    def shutdown(self):
        """关闭执行器"""
        logger.info("正在关闭Python执行器...")
        self.running = False
        
        # 取消当前任务
        if self.task_executor:
            try:
                self.task_executor.cancel_task()
            except Exception as e:
                logger.error(f"取消任务失败: {e}")
        
        # 断开所有客户端连接
        for client_sid in list(self.clients.keys()):
            try:
                self.socketio.emit('shutdown', {
                    'message': '执行器正在关闭',
                    'timestamp': int(time.time() * 1000)
                }, room=client_sid)
            except Exception as e:
                logger.error(f"通知客户端失败: {e}")
        
        logger.info("Python执行器已关闭")
    
    def run(self, host: str = None, port: int = None):
        """运行执行器"""
        host = host or settings.websocket_host
        port = port or settings.websocket_port
        
        self.start_time = time.time()
        
        logger.info("=" * 60)
        logger.info("Python执行器启动")
        logger.info(f"版本: 1.0.0")
        logger.info(f"监听地址: ws://{host}:{port}")
        logger.info("=" * 60)
        
        try:
            self.socketio.run(
                self.app,
                host=host,
                port=port,
                debug=False,
                use_reloader=False
            )
        except KeyboardInterrupt:
            logger.info("收到中断信号，正在关闭执行器...")
            self.shutdown()
        except Exception as e:
            logger.error(f"执行器运行异常: {e}")
            self.shutdown()
            raise

def main():
    """主函数"""
    try:
        executor = PythonExecutor()
        executor.run()
    except Exception as e:
        logger.error(f"执行器启动失败: {e}")
        sys.exit(1)

if __name__ == '__main__':
    main()