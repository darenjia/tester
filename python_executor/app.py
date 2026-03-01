"""
Flask-SocketIO WebSocket服务端
需要安装: pip install flask flask-socketio
"""
from gevent import monkey
monkey.patch_all()

from flask import Flask
from flask_socketio import SocketIO, emit, request
from datetime import datetime
import json

app = Flask(__name__)
app.config['SECRET_KEY'] = 'python-executor-secret-key'

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
    return "Python执行器WebSocket服务端"

@app.route('/health')
def health_check():
    return {
        'status': 'healthy',
        'timestamp': datetime.now().isoformat(),
        'clients': len(clients)
    }

@socketio.on('connect')
def handle_connect():
    """处理客户端连接"""
    sid = request.sid
    clients[sid] = {
        'connected_at': datetime.now(),
        'last_heartbeat': datetime.now()
    }
    print(f"客户端连接: {sid}")
    
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
        print(f"客户端断开连接: {sid}, 连接时长: {connected_time}")

@socketio.on('heartbeat')
def handle_heartbeat(data):
    """处理心跳消息"""
    sid = request.sid
    if sid in clients:
        clients[sid]['last_heartbeat'] = datetime.now()
        print(f"收到心跳: {sid}")
        
        # 回复心跳
        emit('heartbeat_response', {
            'timestamp': int(datetime.now().timestamp() * 1000)
        })

@socketio.on('message')
def handle_message(data):
    """处理通用消息"""
    try:
        print(f"收到消息: {data}")
        
        # 这里可以添加消息处理逻辑
        message_type = data.get('type')
        
        # 回复确认
        emit('message_response', {
            'status': 'received',
            'type': message_type,
            'timestamp': int(datetime.now().timestamp() * 1000)
        })
        
    except Exception as e:
        print(f"处理消息失败: {e}")
        emit('error', {
            'error': str(e),
            'timestamp': int(datetime.now().timestamp() * 1000)
        })

@socketio.on('task_dispatch')
def handle_task_dispatch(data):
    """处理任务下发"""
    try:
        print(f"收到任务下发: {data}")
        
        # 这里可以添加任务执行逻辑
        task_id = data.get('taskId')
        
        # 模拟任务接收确认
        emit('task_response', {
            'taskId': task_id,
            'status': 'accepted',
            'message': '任务已接收，正在执行',
            'timestamp': int(datetime.now().timestamp() * 1000)
        })
        
        # 模拟任务执行
        import threading
        def execute_task():
            # 这里应该调用实际的执行器
            print(f"开始执行任务: {task_id}")
            
            # 模拟执行过程
            import time
            time.sleep(2)
            
            # 发送完成消息
            emit('task_completed', {
                'taskId': task_id,
                'status': 'completed',
                'results': [],
                'timestamp': int(datetime.now().timestamp() * 1000)
            })
        
        threading.Thread(target=execute_task, daemon=True).start()
        
    except Exception as e:
        print(f"处理任务下发失败: {e}")
        emit('error', {
            'error': str(e),
            'timestamp': int(datetime.now().timestamp() * 1000)
        })

if __name__ == '__main__':
    print("启动Python执行器WebSocket服务端...")
    print("监听地址: ws://localhost:8180")
    
    socketio.run(
        app,
        host='0.0.0.0',
        port=8180,
        debug=True
    )