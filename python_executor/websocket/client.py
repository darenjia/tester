"""
WebSocket服务端实现
"""
import json
import time
import threading
from datetime import datetime
from typing import Dict, Any, Optional, Callable
from flask import Flask, request
from flask_socketio import SocketIO, emit, disconnect

from config.settings import settings
from utils.logger import get_logger
from utils.exceptions import CommunicationException
from models.result import Message, LogEntry, StatusUpdate

logger = get_logger("websocket")

class WebSocketServer:
    """WebSocket服务端"""
    
    def __init__(self):
        self.app = Flask(__name__)
        self.socketio = SocketIO(
            self.app,
            cors_allowed_origins="*",
            async_mode='threading',
            logger=False,
            engineio_logger=False
        )
        
        self.clients = {}  # sid -> client_info
        self.message_handlers = {}
        self.heartbeat_thread = None
        self.running = False
        
        self._setup_routes()
        self._setup_event_handlers()
    
    def _setup_routes(self):
        """设置HTTP路由"""
        @self.app.route('/health')
        def health_check():
            return {'status': 'healthy', 'timestamp': datetime.now().isoformat()}
        
        @self.app.route('/status')
        def status():
            return {
                'clients': len(self.clients),
                'running': self.running,
                'uptime': time.time() - getattr(self, 'start_time', time.time())
            }
    
    def _setup_event_handlers(self):
        """设置WebSocket事件处理器"""
        @self.socketio.on('connect')
        def handle_connect():
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
                'timestamp': int(time.time() * 1000)
            })
        
        @self.socketio.on('disconnect')
        def handle_disconnect():
            sid = request.sid
            if sid in self.clients:
                client_info = self.clients.pop(sid)
                connected_time = datetime.now() - client_info['connected_at']
                logger.info(f"客户端断开连接: {sid}, 连接时长: {connected_time}")
        
        @self.socketio.on('heartbeat')
        def handle_heartbeat(data):
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
                
                # 调用对应的消息处理器
                if message.type in self.message_handlers:
                    handler = self.message_handlers[message.type]
                    response = handler(message)
                    
                    if response:
                        emit('message_response', response.to_dict())
                else:
                    logger.warning(f"未找到消息处理器: {message.type}")
                    
            except Exception as e:
                logger.error(f"处理消息失败: {e}")
                emit('error', {
                    'error': str(e),
                    'timestamp': int(time.time() * 1000)
                })
    
    def register_handler(self, message_type: str, handler: Callable):
        """注册消息处理器"""
        self.message_handlers[message_type] = handler
        logger.info(f"注册消息处理器: {message_type}")
    
    def send_message(self, sid: str, message: Dict[str, Any]):
        """发送消息到指定客户端"""
        if sid in self.clients:
            try:
                self.socketio.emit('message', message, room=sid)
                logger.debug(f"发送消息到 {sid}: {message.get('type')}")
            except Exception as e:
                logger.error(f"发送消息失败: {e}")
                raise CommunicationException(f"发送消息失败: {e}")
        else:
            logger.warning(f"客户端不存在: {sid}")
    
    def broadcast_message(self, message: Dict[str, Any]):
        """广播消息到所有客户端"""
        try:
            self.socketio.emit('message', message)
            logger.debug(f"广播消息: {message.get('type')}")
        except Exception as e:
            logger.error(f"广播消息失败: {e}")
            raise CommunicationException(f"广播消息失败: {e}")
    
    def send_log(self, sid: str, level: str, message: str, task_id: str = None, details: Dict[str, Any] = None):
        """发送日志消息"""
        log_entry = LogEntry(
            level=level,
            message=message,
            timestamp=datetime.now(),
            task_id=task_id,
            details=details
        )
        
        self.send_message(sid, {
            'type': 'LOG_STREAM',
            'payload': log_entry.to_dict()
        })
    
    def send_status_update(self, sid: str, task_id: str, status: str, message: str = None, progress: int = None):
        """发送状态更新"""
        status_update = StatusUpdate(
            task_id=task_id,
            status=status,
            message=message,
            progress=progress
        )
        
        self.send_message(sid, {
            'type': 'TASK_STATUS',
            'payload': status_update.to_dict()
        })
    
    def _heartbeat_worker(self):
        """心跳工作线程"""
        while self.running:
            try:
                current_time = datetime.now()
                disconnected_clients = []
                
                for sid, client_info in self.clients.items():
                    last_heartbeat = client_info['last_heartbeat']
                    
                    # 检查心跳超时
                    if (current_time - last_heartbeat).total_seconds() > settings.heartbeat_interval * 2:
                        logger.warning(f"客户端心跳超时: {sid}")
                        disconnected_clients.append(sid)
                        continue
                    
                    # 发送心跳请求
                    self.socketio.emit('heartbeat_request', {
                        'timestamp': int(time.time() * 1000)
                    }, room=sid)
                
                # 断开超时客户端
                for sid in disconnected_clients:
                    if sid in self.clients:
                        self.clients.pop(sid)
                        self.socketio.emit('disconnect', room=sid)
                
                time.sleep(settings.heartbeat_interval)
                
            except Exception as e:
                logger.error(f"心跳工作线程异常: {e}")
                time.sleep(1)
    
    def start(self, host: str = None, port: int = None):
        """启动WebSocket服务器"""
        host = host or settings.websocket_host
        port = port or settings.websocket_port
        
        self.running = True
        self.start_time = time.time()
        
        # 启动心跳线程
        self.heartbeat_thread = threading.Thread(target=self._heartbeat_worker, daemon=True)
        self.heartbeat_thread.start()
        
        logger.info(f"启动WebSocket服务器: {host}:{port}")
        
        try:
            self.socketio.run(
                self.app,
                host=host,
                port=port,
                debug=False,
                use_reloader=False
            )
        except KeyboardInterrupt:
            logger.info("收到中断信号，正在关闭服务器...")
            self.stop()
        except Exception as e:
            logger.error(f"WebSocket服务器异常: {e}")
            raise CommunicationException(f"WebSocket服务器启动失败: {e}")
    
    def stop(self):
        """停止WebSocket服务器"""
        self.running = False
        
        if self.heartbeat_thread and self.heartbeat_thread.is_alive():
            self.heartbeat_thread.join(timeout=5)
        
        logger.info("WebSocket服务器已停止")

# 全局WebSocket服务器实例
websocket_server = WebSocketServer()