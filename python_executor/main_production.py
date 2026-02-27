"""
Python执行器主应用 - 生产环境版本
集成所有生产环境增强功能
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
from config.config_manager import config_manager
from utils.logger import get_logger, setup_logger
from utils.exceptions import ExecutorException
from utils.metrics import performance_monitor, metric_collector
from utils.validators import InputValidator, ValidationError
from models.task import Task, Message
from models.result import StatusUpdate, LogEntry
from core.task_executor_production import TaskExecutorProduction

# 设置日志
logger = setup_logger("executor_production")

class PythonExecutorProduction:
    """Python执行器主类 - 生产环境版本"""
    
    def __init__(self):
        self.app = Flask(__name__)
        self.app.config['SECRET_KEY'] = 'python-executor-secret-key-production'
        
        # 初始化SocketIO
        self.socketio = SocketIO(
            self.app,
            cors_allowed_origins="*",
            async_mode='threading',
            logger=False,
            engineio_logger=False
        )
        
        # 初始化组件
        self.task_executor = None
        self.clients = {}  # sid -> client_info
        self.running = True
        
        # 设置路由和事件处理器
        self._setup_routes()
        self._setup_websocket_handlers()
        
        # 设置信号处理
        signal.signal(signal.SIGINT, self._signal_handler)
        signal.signal(signal.SIGTERM, self._signal_handler)
        
        # 启动配置监控
        config_manager.start_watcher(interval=5.0)
        
        # 启动性能监控
        if config_manager.get('performance.monitor_enabled', True):
            performance_monitor.start(interval=config_manager.get('performance.monitor_interval', 60.0))
            
            # 设置告警阈值
            performance_monitor.set_alert_threshold('system.cpu_percent', 80.0)
            performance_monitor.set_alert_threshold('system.memory_percent', 85.0)
            performance_monitor.register_alert_callback(self._on_performance_alert)
        
        logger.info("Python执行器（生产环境版）初始化完成")
    
    def _setup_routes(self):
        """设置HTTP路由"""
        @self.app.route('/')
        def index():
            return {
                'name': 'Python执行器（生产环境版）',
                'version': '2.0.0',
                'status': 'running',
                'timestamp': datetime.now().isoformat(),
                'features': [
                    '熔断器保护',
                    '自动重试',
                    '配置热更新',
                    '性能监控',
                    '输入验证'
                ]
            }
        
        @self.app.route('/health')
        def health_check():
            """健康检查"""
            return {
                'status': 'healthy',
                'timestamp': datetime.now().isoformat(),
                'clients': len(self.clients),
                'current_task': self.task_executor.get_current_status() if self.task_executor else None,
                'config_valid': len(config_manager.validate_config()) == 0
            }
        
        @self.app.route('/status')
        def status():
            """状态查询"""
            return {
                'clients': len(self.clients),
                'running': self.running,
                'uptime': time.time() - getattr(self, 'start_time', time.time()),
                'current_task': self.task_executor.get_current_status() if self.task_executor else None,
                'metrics': performance_monitor.get_metrics_report() if performance_monitor else None
            }
        
        @self.app.route('/metrics')
        def metrics():
            """指标查询"""
            return {
                'timestamp': datetime.now().isoformat(),
                'metrics': metric_collector.get_all_metrics(),
                'performance': performance_monitor.get_metrics_report() if performance_monitor else None
            }
        
        @self.app.route('/config', methods=['GET'])
        def get_config():
            """获取配置"""
            return {
                'config': config_manager.get_all(),
                'validation_errors': config_manager.validate_config()
            }
        
        @self.app.route('/config/reload', methods=['POST'])
        def reload_config():
            """重新加载配置"""
            config_manager.reload()
            return {
                'status': 'success',
                'message': '配置已重新加载',
                'validation_errors': config_manager.validate_config()
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
                'last_heartbeat': datetime.now(),
                'ip': request.remote_addr
            }
            self.clients[sid] = client_info
            
            logger.info(f"客户端连接: {sid} from {request.remote_addr}")
            
            # 发送欢迎消息
            emit('welcome', {
                'message': 'Python执行器（生产环境版）已连接',
                'executor_info': {
                    'version': '2.0.0',
                    'status': 'ready',
                    'features': [
                        'circuit_breaker',
                        'auto_retry',
                        'hot_reload',
                        'performance_monitoring',
                        'input_validation'
                    ]
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
                    'timestamp': int(time.time() * 1000),
                    'server_time': datetime.now().isoformat()
                })
        
        @self.socketio.on('message')
        def handle_message(data):
            """处理通用消息"""
            try:
                # 验证消息格式
                if not isinstance(data, dict):
                    raise ValidationError("消息必须是字典类型")
                
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
                
            except ValidationError as e:
                logger.error(f"消息验证失败: {e}")
                emit('error', {
                    'error': f"消息验证失败: {e}",
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
            
            # 验证任务数据
            task_data = message.payload or {}
            task_data.update({
                'taskId': message.task_id,
                'deviceId': message.device_id
            })
            
            # 验证输入
            validated_data = InputValidator.validate_task_data(task_data)
            
            # 创建任务对象
            task = Task.from_dict(validated_data)
            
            # 初始化任务执行器（如果还没初始化）
            if not self.task_executor:
                self.task_executor = TaskExecutorProduction(
                    message_sender=lambda msg: self._send_message_to_client(client_sid, msg)
                )
                self.task_executor.start()
            
            # 执行任务
            success = self.task_executor.execute_task(task)
            
            if success:
                logger.info(f"任务已加入队列: {task.task_id}")
                emit('task_response', {
                    'taskId': task.task_id,
                    'status': 'queued',
                    'message': '任务已加入队列',
                    'timestamp': int(time.time() * 1000)
                })
            else:
                logger.warning(f"任务加入队列失败: {task.task_id}")
                emit('task_response', {
                    'taskId': task.task_id,
                    'status': 'rejected',
                    'message': '任务加入队列失败',
                    'timestamp': int(time.time() * 1000)
                })
                
        except ValidationError as e:
            logger.error(f"任务数据验证失败: {e}")
            emit('error', {
                'error': f"任务数据验证失败: {e}",
                'timestamp': int(time.time() * 1000)
            })
        except Exception as e:
            logger.error(f"处理任务下发失败: {e}")
            emit('error', {
                'error': f"任务下发失败: {e}",
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
                    emit('cancel_response', {
                        'taskId': message.task_id,
                        'status': 'cancelled',
                        'message': '任务已取消',
                        'timestamp': int(time.time() * 1000)
                    })
                else:
                    logger.warning(f"任务取消失败: {message.task_id}")
                    emit('cancel_response', {
                        'taskId': message.task_id,
                        'status': 'failed',
                        'message': '任务取消失败',
                        'timestamp': int(time.time() * 1000)
                    })
            else:
                logger.warning("没有正在执行的任务")
                emit('cancel_response', {
                    'taskId': message.task_id,
                    'status': 'failed',
                    'message': '没有正在执行的任务',
                    'timestamp': int(time.time() * 1000)
                })
                
        except Exception as e:
            logger.error(f"处理任务取消失败: {e}")
            emit('error', {
                'error': f"任务取消失败: {e}",
                'timestamp': int(time.time() * 1000)
            })
    
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
    
    def _on_performance_alert(self, metric_name: str, value: float, threshold: float):
        """性能告警回调"""
        alert_msg = f"性能告警: {metric_name} = {value:.2f}，超过阈值 {threshold:.2f}"
        logger.warning(alert_msg)
        
        # 广播告警给所有客户端
        for client_sid in self.clients:
            try:
                self.socketio.emit('alert', {
                    'type': 'performance',
                    'metric': metric_name,
                    'value': value,
                    'threshold': threshold,
                    'message': alert_msg,
                    'timestamp': int(time.time() * 1000)
                }, room=client_sid)
            except Exception as e:
                logger.error(f"发送告警失败: {e}")
    
    def _signal_handler(self, signum, frame):
        """信号处理"""
        logger.info(f"收到信号 {signum}，正在关闭执行器...")
        self.shutdown()
        sys.exit(0)
    
    def shutdown(self):
        """关闭执行器"""
        logger.info("正在关闭Python执行器...")
        self.running = False
        
        # 停止配置监控
        config_manager.stop_watcher()
        
        # 停止性能监控
        if performance_monitor:
            performance_monitor.stop()
        
        # 停止任务执行器
        if self.task_executor:
            try:
                self.task_executor.stop()
            except Exception as e:
                logger.error(f"停止任务执行器失败: {e}")
        
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
        host = host or config_manager.get('websocket.host', '0.0.0.0')
        port = port or config_manager.get('websocket.port', 8080)
        
        self.start_time = time.time()
        
        logger.info("=" * 60)
        logger.info("Python执行器（生产环境版）启动")
        logger.info(f"版本: 2.0.0")
        logger.info(f"监听地址: ws://{host}:{port}")
        logger.info("生产环境特性:")
        logger.info("  - 熔断器保护")
        logger.info("  - 自动重试机制")
        logger.info("  - 配置热更新")
        logger.info("  - 性能监控")
        logger.info("  - 输入验证")
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
        executor = PythonExecutorProduction()
        executor.run()
    except Exception as e:
        logger.error(f"执行器启动失败: {e}")
        sys.exit(1)

if __name__ == '__main__':
    main()