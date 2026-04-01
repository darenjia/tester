"""
Python执行器主应用 - 生产环境版本
集成所有生产环境增强功能
"""
from gevent import monkey
monkey.patch_all()

print("正在初始化 Python 执行器...")

import json
import time
import signal
import sys
from datetime import datetime
from typing import Dict, Any
from functools import wraps

from flask import Flask, request
from flask_socketio import SocketIO, emit

from config.settings import get_config as get_runtime_config
from utils.logger import get_logger, setup_logging
from utils.exceptions import ExecutorException
from utils.metrics import (
    build_business_metrics_summary,
    metric_collector,
    performance_monitor,
    record_metric,
)
from utils.validators import InputValidator, ValidationError
from core.task_compiler import TaskCompiler, TaskCompileError
from models.result import Message, StatusUpdate, LogEntry
from core.execution_observability import (
    ExecutionLifecycleStage,
    get_execution_observability_manager,
)
from core.task_executor_production import TaskExecutorProduction
from core.status_monitor import get_status_monitor, ConnectionStatus

# 注册API蓝图
from api.task_api import task_bp
from api.task_log_api import task_log_bp
from api.service_api import service_bp
from api.config_api import config_bp
from api.docs_api import docs_bp
from api.env_api import env_bp
from api.functional_test_api import functional_test_bp
from api.case_mapping_api import case_mapping_bp
from api.routes import api_bp
from api.system_check_api import system_check_bp
from api.report_retry_api import report_retry_bp

print("正在设置日志...")
setup_logging()
logger = get_logger()
print("日志设置完成")


def _get_config_manager():
    """Return the active facade-backed config manager."""
    return get_runtime_config()


class _ConfigManagerProxy:
    """Compatibility proxy that forwards to the active config manager."""

    def __getattr__(self, name):
        return getattr(_get_config_manager(), name)


config_manager = _ConfigManagerProxy()

class PythonExecutorProduction:
    """Python执行器主类 - 生产环境版本"""
    
    def __init__(self):
        self.app = Flask(__name__)
        self.app.config['SECRET_KEY'] = 'python-executor-secret-key-production'
        
        # 初始化SocketIO
        self.socketio = SocketIO(
            self.app,
            cors_allowed_origins="*",
            async_mode='gevent',
            logger=False,
            engineio_logger=False
        )
        
        # 初始化组件
        self.task_executor = None
        self.clients = {}  # sid -> client_info
        self.running = True
        
        # 设置路由和事件处理器
        self._register_blueprints()
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

    def _register_blueprints(self):
        """注册API蓝图"""
        self.app.register_blueprint(task_bp)
        self.app.register_blueprint(task_log_bp)
        self.app.register_blueprint(service_bp)
        self.app.register_blueprint(config_bp)
        self.app.register_blueprint(docs_bp)
        self.app.register_blueprint(env_bp)
        self.app.register_blueprint(functional_test_bp)
        self.app.register_blueprint(case_mapping_bp)
        self.app.register_blueprint(api_bp)
        self.app.register_blueprint(system_check_bp)
        self.app.register_blueprint(report_retry_bp)
        logger.info("API蓝图注册完成")

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
            business_summary = get_execution_observability_manager().get_business_summary()
            config_valid = len(config_manager.validate_config()) == 0
            business_healthy = business_summary.get('recent_failed_count', 0) == 0
            health_result = {
                'status': 'healthy' if config_valid and business_healthy else 'degraded',
                'timestamp': datetime.now().isoformat(),
                'clients': len(self.clients),
                'current_task': None,
                'config_valid': config_valid,
                'business_health': {
                    'healthy': business_healthy,
                    'queued_count': business_summary.get('queued_count', 0),
                    'active_count': business_summary.get('active_count', 0),
                    'recent_failed_count': business_summary.get('recent_failed_count', 0),
                },
            }
            
            try:
                if self.task_executor:
                    health_result['current_task'] = self.task_executor.get_current_status()
            except Exception as e:
                logger.warning(f"获取任务状态失败: {e}")
            
            return health_result
        
        @self.app.route('/status')
        def status():
            """状态查询"""
            status_result = {
                'clients': len(self.clients),
                'running': self.running,
                'uptime': time.time() - getattr(self, 'start_time', time.time()),
                'current_task': None,
                'metrics': None,
                'business_summary': {},
            }
            
            try:
                if self.task_executor:
                    status_result['current_task'] = self.task_executor.get_current_status()
            except Exception as e:
                logger.warning(f"获取任务状态失败: {e}")
            
            try:
                status_result['metrics'] = performance_monitor.get_metrics_report()
            except Exception as e:
                logger.warning(f"获取性能指标失败: {e}")

            try:
                status_result['business_summary'] = (
                    get_execution_observability_manager().get_business_summary()
                )
            except Exception as e:
                logger.warning(f"获取业务状态摘要失败: {e}")
            
            return status_result
        
        @self.app.route('/metrics')
        def metrics():
            """指标查询"""
            metrics_result = {
                'timestamp': datetime.now().isoformat(),
                'metrics': {},
                'performance': None,
                'business_summary': {},
            }
            
            try:
                metrics_result['metrics'] = metric_collector.get_all_metrics()
            except Exception as e:
                logger.warning(f"获取指标失败: {e}")
            
            try:
                metrics_result['performance'] = performance_monitor.get_metrics_report()
            except Exception as e:
                logger.warning(f"获取性能报告失败: {e}")

            try:
                observability_summary = get_execution_observability_manager().get_business_summary()
                metrics_result['business_summary'] = build_business_metrics_summary(
                    observability_summary
                )
            except Exception as e:
                logger.warning(f"获取业务指标摘要失败: {e}")
            
            return metrics_result
        
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
            try:
                sid = request.sid
                client_info = {
                    'sid': sid,
                    'connected_at': datetime.now(),
                    'last_heartbeat': datetime.now(),
                    'ip': request.remote_addr
                }
                self.clients[sid] = client_info

                # 更新 StatusMonitor 的 WebSocket 状态
                get_status_monitor().update_websocket_status(
                    ConnectionStatus.CONNECTED,
                    datetime.now()
                )

                logger.info(f"客户端连接: {sid} from {request.remote_addr}")
                
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
            except Exception as e:
                logger.error(f"处理连接时出错: {e}")
                return False
            return True
        
        @self.socketio.on('disconnect')
        def handle_disconnect():
            """处理客户端断开连接"""
            sid = request.sid
            if sid in self.clients:
                client_info = self.clients.pop(sid)
                connected_time = datetime.now() - client_info['connected_at']
                logger.info(f"客户端断开连接: {sid}, 连接时长: {connected_time}")

            # 如果没有其他客户端，更新 StatusMonitor 的 WebSocket 状态
            if len(self.clients) == 0:
                get_status_monitor().update_websocket_status(
                    ConnectionStatus.DISCONNECTED
                )
        
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
            logger.info(f"收到任务下发: {message.taskNo}")
            
            task_data = message.payload or {}
            if isinstance(task_data, str):
                task_data = json.loads(task_data)
            if not isinstance(task_data, dict):
                task_data = {}
            
            task_data.update({
                'taskNo': message.taskNo,
                'deviceId': message.deviceId
            })

            observability_manager = get_execution_observability_manager()
            tool_type_hint = task_data.get("toolType") or ""
            execution_context = observability_manager.create_context(
                task_no=message.taskNo,
                device_id=message.deviceId or "",
                tool_type=str(tool_type_hint).lower(),
            )
            record_metric("task.received", 1, {"task_no": message.taskNo})

            # 验证输入
            validated_data = InputValidator.validate_task_data(task_data)
            observability_manager.transition(message.taskNo, ExecutionLifecycleStage.VALIDATED)
            record_metric("task.validated", 1, {"task_no": message.taskNo})

            # 从映射库编译执行计划
            from core.case_mapping_manager import get_case_mapping_manager
            mapping_manager = get_case_mapping_manager()
            compiler = TaskCompiler(mapping_manager=mapping_manager)
            execution_plan = compiler.compile_payload(validated_data)
            observability_manager.transition(message.taskNo, ExecutionLifecycleStage.COMPILED)
            record_metric("task.compiled", 1, {"task_no": message.taskNo})

            # 初始化任务执行器（如果还没初始化）
            if not self.task_executor:
                self.task_executor = TaskExecutorProduction(
                    message_sender=lambda msg: self._send_message_to_client(client_sid, msg)
                )
                self.task_executor.start()

            # 执行任务 (使用 ExecutionPlan)
            success = self.task_executor.execute_plan(execution_plan)

            if success:
                if not execution_context.tool_type:
                    execution_context.tool_type = execution_plan.tool_type or ""
                observability_manager.transition(message.taskNo, ExecutionLifecycleStage.QUEUED)
                record_metric("task.queued", 1, {"task_no": message.taskNo})
                logger.info(f"任务已加入队列: {execution_plan.task_no}")
                emit('task_response', {
                    'taskNo': execution_plan.task_no,
                    'status': 'queued',
                    'message': '任务已加入队列',
                    'timestamp': int(time.time() * 1000)
                })
            else:
                observability_manager.fail(
                    message.taskNo,
                    error_code="TASK_QUEUE_REJECTED",
                    error_message="任务加入队列失败",
                    retryable=False,
                )
                record_metric(
                    "task.failed",
                    1,
                    {"task_no": message.taskNo, "stage": "compiled"},
                )
                logger.warning(f"任务加入队列失败: {execution_plan.task_no}")
                emit('task_response', {
                    'taskNo': execution_plan.task_no,
                    'status': 'rejected',
                    'message': '任务加入队列失败',
                    'timestamp': int(time.time() * 1000)
                })
                
        except ValidationError as e:
            try:
                observability_manager = get_execution_observability_manager()
                if message.taskNo in observability_manager._contexts:
                    observability_manager.fail(
                        message.taskNo,
                        error_code="TASK_VALIDATION_FAILED",
                        error_message=str(e),
                        retryable=False,
                    )
                    record_metric(
                        "task.failed",
                        1,
                        {"task_no": message.taskNo, "stage": "validated"},
                    )
            except Exception:
                logger.warning("记录任务校验失败观测信息失败", exc_info=True)
            logger.error(f"任务数据验证失败: {e}")
            emit('error', {
                'error': f"任务数据验证失败: {e}",
                'timestamp': int(time.time() * 1000)
            })
        except TaskCompileError as e:
            try:
                observability_manager = get_execution_observability_manager()
                if message.taskNo in observability_manager._contexts:
                    observability_manager.fail(
                        message.taskNo,
                        error_code="TASK_COMPILE_FAILED",
                        error_message=str(e),
                        retryable=False,
                    )
                    record_metric(
                        "task.failed",
                        1,
                        {"task_no": message.taskNo, "stage": "validated"},
                    )
            except Exception:
                logger.warning("记录任务编译失败观测信息失败", exc_info=True)
            logger.error(f"任务编译失败: {e}")
            emit('error', {
                'error': f"任务编译失败: {e}",
                'timestamp': int(time.time() * 1000)
            })
        except Exception as e:
            try:
                observability_manager = get_execution_observability_manager()
                if message.taskNo in observability_manager._contexts:
                    observability_manager.fail(
                        message.taskNo,
                        error_code="TASK_DISPATCH_FAILED",
                        error_message=str(e),
                        retryable=False,
                    )
                    record_metric(
                        "task.failed",
                        1,
                        {"task_no": message.taskNo, "stage": "received"},
                    )
            except Exception:
                logger.warning("记录任务下发失败观测信息失败", exc_info=True)
            logger.error(f"处理任务下发失败: {e}")
            emit('error', {
                'error': f"任务下发失败: {e}",
                'timestamp': int(time.time() * 1000)
            })

    def _handle_task_cancel(self, message: Message, client_sid: str):
        """处理任务取消"""
        try:
            logger.info(f"收到任务取消: {message.taskNo}")
            
            if self.task_executor:
                success = self.task_executor.cancel_task()
                if success:
                    logger.info(f"任务取消成功: {message.taskNo}")
                    emit('cancel_response', {
                        'taskNo': message.taskNo,
                        'status': 'cancelled',
                        'message': '任务已取消',
                        'timestamp': int(time.time() * 1000)
                    })
                else:
                    logger.warning(f"任务取消失败: {message.taskNo}")
                    emit('cancel_response', {
                        'taskNo': message.taskNo,
                        'status': 'failed',
                        'message': '任务取消失败',
                        'timestamp': int(time.time() * 1000)
                    })
            else:
                logger.warning("没有正在执行的任务")
                emit('cancel_response', {
                    'taskNo': message.taskNo,
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
        port = port or config_manager.get('websocket.port', 8180)
        
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
