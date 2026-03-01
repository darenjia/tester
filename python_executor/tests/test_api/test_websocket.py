"""
WebSocket测试模块
测试Flask-SocketIO WebSocket事件
"""
import pytest
import json
import time
import threading
from datetime import datetime

try:
    import websocket as _ws_check
    _ws_has_create_connection = hasattr(_ws_check, 'create_connection')
except ImportError:
    _ws_has_create_connection = False

if _ws_has_create_connection:
    import websocket as ws_client
    WEBSOCKET_AVAILABLE = True
else:
    ws_client = None
    WEBSOCKET_AVAILABLE = False


class TestWebSocket:
    """WebSocket测试类"""
    
    WS_URL = "ws://localhost:8180/socket.io/?EIO=4&transport=websocket"
    
    def setup_method(self):
        """每个测试方法前的设置"""
        if not WEBSOCKET_AVAILABLE:
            pytest.skip("websocket-client未安装，跳过WebSocket测试")
        self.ws_client = ws_client.WebSocket()
    
    def teardown_method(self):
        """每个测试方法后的清理"""
        if hasattr(self, 'ws_client') and self.ws_client.connected:
            self.ws_client.close()
    
    def test_websocket_connection(self):
        """测试WebSocket连接"""
        ws = ws_client.create_connection(self.WS_URL)
        
        assert ws.connected, "WebSocket连接应成功"
        
        ws.close()
        assert not ws.connected, "WebSocket连接应已关闭"
    
    def test_welcome_message(self):
        """测试欢迎消息"""
        ws = ws_client.create_connection(self.WS_URL)
        
        try:
            # 等待欢迎消息
            ws.settimeout(5)
            message = ws.recv()
            
            # 解析Socket.IO消息
            # Socket.IO消息格式: <packet_type><data>
            assert len(message) > 0, "应收到消息"
            
            # 第一个字符是包类型
            packet_type = message[0]
            assert packet_type in ['0', '4'], f"应为连接或消息包，实际{packet_type}"
            
        finally:
            ws.close()
    
    def test_heartbeat(self):
        """测试心跳机制"""
        ws = ws_client.create_connection(self.WS_URL)
        
        try:
            # 发送心跳
            heartbeat_data = {
                'timestamp': int(time.time() * 1000)
            }
            
            # Socket.IO事件格式: 42["event_name", data]
            event_message = f'42["heartbeat", {json.dumps(heartbeat_data)}]'
            ws.send(event_message)
            
            # 等待心跳响应
            ws.settimeout(5)
            response = ws.recv()
            
            # 验证收到响应
            assert len(response) > 0, "应收到心跳响应"
            
        finally:
            ws.close()
    
    def test_task_dispatch_event(self):
        """测试任务下发事件"""
        ws = ws_client.create_connection(self.WS_URL)
        
        try:
            # 准备任务数据
            task_data = {
                'type': 'TASK_DISPATCH',
                'taskNo': 'TEST-TASK-001',
                'deviceId': 'TEST-DEVICE-001',
                'payload': {
                    'projectNo': 'TEST-001',
                    'taskNo': 'TASK-001',
                    'taskName': 'API测试任务',
                    'caseList': [
                        {
                            'moduleLevel1': '模块1',
                            'moduleLevel2': '子模块1',
                            'moduleLevel3': '功能点1',
                            'caseName': '测试用例1',
                            'priority': '高',
                            'caseType': '功能测试',
                            'preCondition': '预置条件',
                            'stepDescription': '测试步骤',
                            'expectedResult': '预期结果',
                            'maintainer': '测试人员',
                            'caseNo': 'CASE-001',
                            'caseSource': 'TDM'
                        }
                    ]
                },
                'timestamp': int(time.time() * 1000)
            }
            
            # 发送任务下发事件
            event_message = f'42["message", {json.dumps(task_data)}]'
            ws.send(event_message)
            
            # 等待响应
            ws.settimeout(10)
            
            # 收集所有响应
            responses = []
            start_time = time.time()
            while time.time() - start_time < 5:
                try:
                    ws.settimeout(1)
                    response = ws.recv()
                    responses.append(response)
                except:
                    break
            
            # 验证收到响应
            assert len(responses) > 0, "应收到响应消息"
            
            # 检查是否有确认响应
            has_ack = any('message_response' in str(r) for r in responses)
            assert has_ack, "应收到消息确认响应"
            
        finally:
            ws.close()
    
    def test_task_cancel_event(self):
        """测试任务取消事件"""
        ws = ws_client.create_connection(self.WS_URL)
        
        try:
            # 先发送任务
            task_data = {
                'type': 'TASK_DISPATCH',
                'taskNo': 'TEST-TASK-002',
                'deviceId': 'TEST-DEVICE-001',
                'payload': {
                    'projectNo': 'TEST-001',
                    'taskNo': 'TASK-002',
                    'taskName': '待取消任务',
                    'caseList': []
                },
                'timestamp': int(time.time() * 1000)
            }
            
            event_message = f'42["message", {json.dumps(task_data)}]'
            ws.send(event_message)
            
            # 等待一下
            time.sleep(1)
            
            # 发送取消任务
            cancel_data = {
                'type': 'TASK_CANCEL',
                'taskNo': 'TEST-TASK-002',
                'timestamp': int(time.time() * 1000)
            }
            
            event_message = f'42["message", {json.dumps(cancel_data)}]'
            ws.send(event_message)
            
            # 等待响应
            ws.settimeout(5)
            responses = []
            start_time = time.time()
            while time.time() - start_time < 3:
                try:
                    ws.settimeout(1)
                    response = ws.recv()
                    responses.append(response)
                except:
                    break
            
            # 验证收到响应
            assert len(responses) > 0, "应收到响应消息"
            
        finally:
            ws.close()
    
    def test_invalid_message(self):
        """测试无效消息处理"""
        ws = ws_client.create_connection(self.WS_URL)
        
        try:
            # 发送无效格式的消息
            ws.send('invalid message format')
            
            # 等待错误响应
            ws.settimeout(5)
            responses = []
            start_time = time.time()
            while time.time() - start_time < 3:
                try:
                    ws.settimeout(1)
                    response = ws.recv()
                    responses.append(response)
                except:
                    break
            
            # 验证连接仍然有效（没有断开）
            assert ws.connected, "连接应保持有效"
            
        finally:
            ws.close()
    
    def test_concurrent_connections(self):
        """测试并发连接"""
        connections = []
        
        try:
            # 建立多个连接
            for i in range(5):
                ws = ws_client.create_connection(self.WS_URL)
                connections.append(ws)
                time.sleep(0.1)
            
            # 验证所有连接都成功
            assert all(ws.connected for ws in connections), "所有连接应成功"
            
            # 每个连接发送消息
            for i, ws in enumerate(connections):
                heartbeat_data = {'timestamp': int(time.time() * 1000), 'client': i}
                event_message = f'42["heartbeat", {json.dumps(heartbeat_data)}]'
                ws.send(event_message)
            
            # 等待响应
            time.sleep(2)
            
            # 验证所有连接仍然有效
            assert all(ws.connected for ws in connections), "所有连接应保持有效"
            
        finally:
            # 关闭所有连接
            for ws in connections:
                if ws.connected:
                    ws.close()
    
    def test_reconnection(self):
        """测试重新连接"""
        
        # 第一次连接
        ws1 = ws_client.create_connection(self.WS_URL)
        assert ws1.connected, "第一次连接应成功"
        ws1.close()
        
        # 等待一下
        time.sleep(1)
        
        # 重新连接
        ws2 = ws_client.create_connection(self.WS_URL)
        assert ws2.connected, "重新连接应成功"
        ws2.close()


class TestWebSocketWithMock:
    """使用Mock的WebSocket测试"""
    
    def test_mock_websocket_event(self, mocker):
        """测试WebSocket事件（使用Mock）"""
        # Mock websocket.create_connection
        mock_ws = mocker.Mock()
        mock_ws.connected = True
        mock_ws.recv.return_value = '42["welcome", {"message": "test"}]'
        
        mocker.patch('websocket.create_connection', return_value=mock_ws)
        
        ws = ws_client.create_connection("ws://test")
        
        assert ws.connected
        response = ws.recv()
        assert 'welcome' in response


if __name__ == '__main__':
    pytest.main([__file__, '-v'])
