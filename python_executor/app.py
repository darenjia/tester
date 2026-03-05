"""
Flask-SocketIO WebSocket服务端 + HTTP API
需要安装: pip install flask flask-socketio
"""
from gevent import monkey
monkey.patch_all()

from flask import Flask, request, jsonify
from flask_socketio import SocketIO, emit
from flask_cors import CORS
from datetime import datetime
import json

from core.task_store import task_store, TaskStatus
from api.routes import api_bp
from utils.logger import get_logger

logger = get_logger("app")

app = Flask(__name__)
app.config['SECRET_KEY'] = 'python-executor-secret-key'

# 启用CORS
CORS(app, resources={
    r"/api/*": {
        "origins": "*",
        "methods": ["GET", "POST", "DELETE", "OPTIONS"],
        "allow_headers": ["Content-Type", "Authorization"]
    }
})

# 注册API蓝图
app.register_blueprint(api_bp)

# 配置SocketIO
socketio = SocketIO(
    app,
    cors_allowed_origins="*",
    async_mode='gevent',
    logger=True,
    engineio_logger=True
)

# 存储客户端信息
clients = {}


@app.route('/')
def index():
    """根路径 - 服务信息"""
    return jsonify({
        'name': 'Python执行器',
        'version': '1.0.0',
        'status': 'running',
        'timestamp': datetime.now().isoformat(),
        'apis': {
            'websocket': '/socket.io/',
            'http_api': '/api/',
            'health': '/health',
            'status': '/api/status'
        }
    })


@app.route('/health')
def health_check():
    """健康检查端点"""
    running_task = task_store.get_running_task()
    return jsonify({
        'status': 'healthy',
        'timestamp': datetime.now().isoformat(),
        'clients': len(clients),
        'current_task': running_task.to_dict() if running_task else None
    })


@app.route('/status')
def status():
    """系统状态端点"""
    running_task = task_store.get_running_task()
    stats = task_store.get_statistics()
    
    return jsonify({
        'running': running_task is not None,
        'current_task': running_task.to_dict() if running_task else None,
        'uptime': None,  # 可以添加启动时间统计
        'clients': len(clients),
        'statistics': stats
    })


# ==================== WebSocket事件处理 ====================

@socketio.on('connect')
def handle_connect():
    """处理客户端连接"""
    sid = request.sid
    clients[sid] = {
        'connected_at': datetime.now(),
        'last_heartbeat': datetime.now()
    }
    logger.info(f"客户端连接: {sid}")
    
    # 发送欢迎消息
    emit('welcome', {
        'message': 'Python执行器已连接',
        'timestamp': int(datetime.now().timestamp() * 1000)
    })


@socketio.on('disconnect')
def handle_disconnect():
    """处理客户端断开连接"""
    sid = request.sid
    if sid in clients:
        client_info = clients.pop(sid)
        connected_time = datetime.now() - client_info['connected_at']
        logger.info(f"客户端断开连接: {sid}, 连接时长: {connected_time}")


@socketio.on('heartbeat')
def handle_heartbeat(data):
    """处理心跳消息"""
    sid = request.sid
    if sid in clients:
        clients[sid]['last_heartbeat'] = datetime.now()
        logger.debug(f"收到心跳: {sid}")
        
        # 回复心跳
        emit('heartbeat_response', {
            'timestamp': int(datetime.now().timestamp() * 1000)
        })


@socketio.on('message')
def handle_message(data):
    """处理通用消息"""
    try:
        logger.info(f"收到消息: {data}")
        
        # 这里可以添加消息处理逻辑
        message_type = data.get('type')
        
        # 回复确认
        emit('message_response', {
            'status': 'received',
            'type': message_type,
            'timestamp': int(datetime.now().timestamp() * 1000)
        })
        
    except Exception as e:
        logger.error(f"处理消息失败: {e}")
        emit('error', {
            'error': str(e),
            'timestamp': int(datetime.now().timestamp() * 1000)
        })


@socketio.on('task_dispatch')
def handle_task_dispatch(data):
    """
    处理WebSocket任务下发
    兼容原有WebSocket接口
    """
    try:
        logger.info(f"收到WebSocket任务下发: {data}")
        
        task_no = data.get('taskNo')
        if not task_no:
            emit('error', {
                'error': 'taskNo不能为空',
                'timestamp': int(datetime.now().timestamp() * 1000)
            })
            return
        
        # 检查是否已有相同taskNo的任务在执行
        existing_task = task_store.get_task(task_no)
        if existing_task and existing_task.status in [TaskStatus.PENDING, TaskStatus.RUNNING]:
            emit('task_response', {
                'taskNo': task_no,
                'status': 'rejected',
                'message': f'任务 {task_no} 正在执行中',
                'timestamp': int(datetime.now().timestamp() * 1000)
            })
            return
        
        # 检查是否有正在执行的任务
        if task_store.has_running_task():
            emit('task_response', {
                'taskNo': task_no,
                'status': 'queued',
                'message': '任务已排队，等待执行',
                'timestamp': int(datetime.now().timestamp() * 1000)
            })
            return
        
        # 创建任务
        task_info = task_store.create_task(data)
        
        # 发送接收确认
        emit('task_response', {
            'taskNo': task_no,
            'status': 'accepted',
            'message': '任务已接收，正在执行',
            'timestamp': int(datetime.now().timestamp() * 1000)
        })
        
        # 触发任务执行
        from api.task_executor import execute_task_async
        execute_task_async(task_info.task_no, data)
        
    except Exception as e:
        logger.error(f"处理任务下发失败: {e}")
        emit('error', {
            'error': str(e),
            'timestamp': int(datetime.now().timestamp() * 1000)
        })


@socketio.on('task_cancel')
def handle_task_cancel(data):
    """处理任务取消请求"""
    try:
        task_no = data.get('taskNo')
        
        if not task_no:
            emit('cancel_response', {
                'status': 'failed',
                'message': 'taskNo不能为空',
                'timestamp': int(datetime.now().timestamp() * 1000)
            })
            return
        
        # 查询任务
        task = task_store.get_task(task_no)
        
        if not task:
            emit('cancel_response', {
                'status': 'failed',
                'message': '任务不存在',
                'timestamp': int(datetime.now().timestamp() * 1000)
            })
            return
        
        # 尝试取消任务
        if task_store.cancel_task(task.task_no):
            from api.task_executor import cancel_task_execution
            cancel_task_execution(task.task_no)
            
            emit('cancel_response', {
                'taskNo': task.task_no,
                'status': 'success',
                'message': '任务已取消',
                'timestamp': int(datetime.now().timestamp() * 1000)
            })
        else:
            emit('cancel_response', {
                'taskNo': task.task_no,
                'status': 'failed',
                'message': f'无法取消任务，当前状态: {task.status.value}',
                'timestamp': int(datetime.now().timestamp() * 1000)
            })
        
    except Exception as e:
        logger.error(f"处理任务取消失败: {e}")
        emit('error', {
            'error': str(e),
            'timestamp': int(datetime.now().timestamp() * 1000)
        })


@socketio.on('task_query')
def handle_task_query(data):
    """处理任务查询请求"""
    try:
        task_no = data.get('taskNo')
        
        if not task_no:
            emit('task_info', {
                'status': 'error',
                'message': 'taskNo不能为空',
                'timestamp': int(datetime.now().timestamp() * 1000)
            })
            return
        
        task = task_store.get_task(task_no)
        
        if task:
            emit('task_info', {
                'status': 'success',
                'data': task.to_dict(),
                'timestamp': int(datetime.now().timestamp() * 1000)
            })
        else:
            emit('task_info', {
                'status': 'error',
                'message': '任务不存在',
                'timestamp': int(datetime.now().timestamp() * 1000)
            })
        
    except Exception as e:
        logger.error(f"处理任务查询失败: {e}")
        emit('error', {
            'error': str(e),
            'timestamp': int(datetime.now().timestamp() * 1000)
        })


if __name__ == '__main__':
    logger.info("=" * 60)
    logger.info("启动Python执行器服务端...")
    logger.info("=" * 60)
    logger.info("WebSocket地址: ws://localhost:8180")
    logger.info("HTTP API地址: http://localhost:8180/api/")
    logger.info("健康检查: http://localhost:8180/health")
    logger.info("=" * 60)
    
    socketio.run(
        app,
        host='0.0.0.0',
        port=8180,
        debug=True
    )
