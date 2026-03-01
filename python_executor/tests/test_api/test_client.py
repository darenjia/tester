"""
测试客户端封装模块
提供HTTP和WebSocket测试客户端
"""
import json
import time
import uuid
from typing import Dict, Any, Optional, Callable
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


class HTTPTestClient:
    """HTTP测试客户端"""
    
    def __init__(self, base_url: str = "http://localhost:8180"):
        """
        初始化HTTP测试客户端
        
        Args:
            base_url: 基础URL
        """
        self.base_url = base_url.rstrip('/')
        self.session = None
        self._init_session()
    
    def _init_session(self):
        """初始化会话"""
        try:
            import requests
            self.session = requests.Session()
            self.session.headers.update({
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            })
        except ImportError:
            raise ImportError("需要安装requests库: pip install requests")
    
    def get(self, endpoint: str, **kwargs) -> Dict[str, Any]:
        """
        发送GET请求
        
        Args:
            endpoint: 端点路径
            **kwargs: 额外参数
            
        Returns:
            响应数据
        """
        url = f"{self.base_url}{endpoint}"
        response = self.session.get(url, **kwargs)
        response.raise_for_status()
        return response.json()
    
    def post(self, endpoint: str, data: Optional[Dict] = None, **kwargs) -> Dict[str, Any]:
        """
        发送POST请求
        
        Args:
            endpoint: 端点路径
            data: 请求数据
            **kwargs: 额外参数
            
        Returns:
            响应数据
        """
        url = f"{self.base_url}{endpoint}"
        response = self.session.post(url, json=data, **kwargs)
        response.raise_for_status()
        return response.json()
    
    def health_check(self) -> Dict[str, Any]:
        """
        健康检查
        
        Returns:
            健康状态数据
        """
        return self.get('/health')
    
    def get_status(self) -> Dict[str, Any]:
        """
        获取执行器状态
        
        Returns:
            状态数据
        """
        return self.get('/status')
    
    def get_info(self) -> Dict[str, Any]:
        """
        获取执行器信息
        
        Returns:
            执行器信息
        """
        return self.get('/')
    
    def close(self):
        """关闭会话"""
        if self.session:
            self.session.close()
    
    def __enter__(self):
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        self.close()


class WebSocketTestClient:
    """WebSocket测试客户端"""
    
    def __init__(self, url: str = "ws://localhost:8180/socket.io/?EIO=4&transport=websocket"):
        """
        初始化WebSocket测试客户端
        
        Args:
            url: WebSocket URL
        """
        self.url = url
        self.ws = None
        self.message_handlers: Dict[str, Callable] = {}
        self.received_messages: list = []
        
        if not WEBSOCKET_AVAILABLE:
            raise ImportError("需要安装websocket-client库: pip install websocket-client")
        self.websocket_module = ws_client
    
    def connect(self, timeout: int = 10) -> bool:
        """
        连接WebSocket
        
        Args:
            timeout: 超时时间（秒）
            
        Returns:
            是否连接成功
        """
        self.ws = self.websocket_module.create_connection(self.url, timeout=timeout)
        
        self.ws.settimeout(5)
        try:
            handshake = self.ws.recv()
            if handshake and handshake.startswith('0'):
                self.ws.send('40')
        except:
            pass
        
        return self.ws.connected
    
    def disconnect(self):
        """断开连接"""
        if self.ws and self.ws.connected:
            self.ws.close()
    
    def send_event(self, event: str, data: Dict[str, Any]):
        """
        发送事件
        
        Args:
            event: 事件名称
            data: 事件数据
        """
        if not self.ws or not self.ws.connected:
            raise ConnectionError("WebSocket未连接")
        
        # Socket.IO事件格式: 42["event_name", data]
        message = f'42["{event}", {json.dumps(data)}]'
        self.ws.send(message)
    
    def send_message(self, message_type: str, task_id: str, payload: Dict[str, Any], 
                     device_id: str = "TEST-DEVICE"):
        """
        发送消息
        
        Args:
            message_type: 消息类型
            task_id: 任务ID
            payload: 消息负载
            device_id: 设备ID
        """
        data = {
            'type': message_type,
            'taskNo': task_id,
            'deviceId': device_id,
            'payload': payload,
            'timestamp': int(time.time() * 1000)
        }
        self.send_event('message', data)
    
    def send_task_dispatch(self, task_data: Dict[str, Any]) -> str:
        """
        发送任务下发
        
        Args:
            task_data: 任务数据
            
        Returns:
            任务ID
        """
        task_id = task_data.get('taskNo', str(uuid.uuid4()))
        self.send_message('TASK_DISPATCH', task_id, task_data)
        return task_id
    
    def send_task_cancel(self, task_id: str):
        """
        发送任务取消
        
        Args:
            task_id: 任务ID
        """
        self.send_message('TASK_CANCEL', task_id, {})
    
    def send_heartbeat(self):
        """发送心跳"""
        self.send_event('heartbeat', {
            'timestamp': int(time.time() * 1000)
        })
    
    def receive(self, timeout: int = 5) -> Optional[str]:
        """
        接收消息
        
        Args:
            timeout: 超时时间（秒）
            
        Returns:
            接收到的消息
        """
        if not self.ws or not self.ws.connected:
            raise ConnectionError("WebSocket未连接")
        
        self.ws.settimeout(timeout)
        try:
            message = self.ws.recv()
            self.received_messages.append({
                'timestamp': datetime.now(),
                'data': message
            })
            return message
        except:
            return None
    
    def receive_all(self, duration: float = 3.0) -> list:
        """
        接收所有消息（在指定时间内）
        
        Args:
            duration: 持续时间（秒）
            
        Returns:
            接收到的消息列表
        """
        messages = []
        start_time = time.time()
        
        while time.time() - start_time < duration:
            message = self.receive(timeout=1)
            if message:
                messages.append(message)
        
        return messages
    
    def wait_for_event(self, event_name: str, timeout: int = 10) -> Optional[Dict]:
        """
        等待特定事件
        
        Args:
            event_name: 事件名称
            timeout: 超时时间（秒）
            
        Returns:
            事件数据
        """
        start_time = time.time()
        
        while time.time() - start_time < timeout:
            message = self.receive(timeout=1)
            if message and event_name in message:
                # 解析Socket.IO消息
                try:
                    # 格式: 42["event_name", {...}]
                    if message.startswith('42['):
                        data = json.loads(message[2:])
                        if len(data) >= 2 and data[0] == event_name:
                            return data[1]
                except:
                    pass
        
        return None
    
    def is_connected(self) -> bool:
        """
        检查连接状态
        
        Returns:
            是否已连接
        """
        return self.ws is not None and self.ws.connected
    
    def __enter__(self):
        self.connect()
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        self.disconnect()


class TestDataGenerator:
    """测试数据生成器"""
    
    @staticmethod
    def generate_task_id() -> str:
        """生成任务ID"""
        return f"TASK-{uuid.uuid4().hex[:8].upper()}"
    
    @staticmethod
    def generate_case(case_no: str = "CASE-001", **kwargs) -> Dict[str, Any]:
        """
        生成测试用例数据
        
        Args:
            case_no: 用例编号
            **kwargs: 覆盖字段
            
        Returns:
            用例数据
        """
        case = {
            'moduleLevel1': kwargs.get('moduleLevel1', '模块1'),
            'moduleLevel2': kwargs.get('moduleLevel2', '子模块1'),
            'moduleLevel3': kwargs.get('moduleLevel3', '功能点1'),
            'caseName': kwargs.get('caseName', f'测试用例{case_no}'),
            'priority': kwargs.get('priority', '高'),
            'caseType': kwargs.get('caseType', '功能测试'),
            'preCondition': kwargs.get('preCondition', '预置条件'),
            'stepDescription': kwargs.get('stepDescription', '测试步骤'),
            'expectedResult': kwargs.get('expectedResult', '预期结果'),
            'maintainer': kwargs.get('maintainer', '测试人员'),
            'caseNo': case_no,
            'caseSource': kwargs.get('caseSource', 'TDM')
        }
        return case
    
    @staticmethod
    def generate_task(project_no: str = "TEST-001", task_no: str = None, 
                      case_count: int = 1, **kwargs) -> Dict[str, Any]:
        """
        生成任务数据
        
        Args:
            project_no: 项目编号
            task_no: 任务编号
            case_count: 用例数量
            **kwargs: 覆盖字段
            
        Returns:
            任务数据
        """
        if task_no is None:
            task_no = TestDataGenerator.generate_task_id()
        
        cases = []
        for i in range(case_count):
            case_no = f"CASE-{i+1:03d}"
            cases.append(TestDataGenerator.generate_case(case_no))
        
        task = {
            'projectNo': project_no,
            'taskNo': task_no,
            'taskName': kwargs.get('taskName', f'测试任务{task_no}'),
            'caseList': cases,
            'deviceId': kwargs.get('deviceId', 'TEST-DEVICE-001'),
            'toolType': kwargs.get('toolType', 'canoe'),
            'configPath': kwargs.get('configPath', 'config/test.cfg'),
            'timeout': kwargs.get('timeout', 3600)
        }
        
        return task
    
    @staticmethod
    def generate_minimal_task(task_no: str = None) -> Dict[str, Any]:
        """
        生成最小任务数据（用于快速测试）
        
        Args:
            task_no: 任务编号
            
        Returns:
            最小任务数据
        """
        if task_no is None:
            task_no = TestDataGenerator.generate_task_id()
        
        return {
            'projectNo': 'TEST-001',
            'taskNo': task_no,
            'taskName': '快速测试任务',
            'caseList': [],
            'toolType': 'canoe',
            'configPath': 'config/test.cfg'
        }


class AssertionHelper:
    """断言辅助类"""
    
    @staticmethod
    def assert_response_status(response: Dict, expected_status: str = 'healthy'):
        """
        断言响应状态
        
        Args:
            response: 响应数据
            expected_status: 期望状态
        """
        assert 'status' in response, f"响应缺少'status'字段: {response}"
        assert response['status'] == expected_status, \
            f"状态应为'{expected_status}'，实际'{response['status']}'"
    
    @staticmethod
    def assert_response_has_fields(response: Dict, required_fields: list):
        """
        断言响应包含必需字段
        
        Args:
            response: 响应数据
            required_fields: 必需字段列表
        """
        for field in required_fields:
            assert field in response, f"响应缺少'{field}'字段"
    
    @staticmethod
    def assert_valid_timestamp(timestamp_str: str):
        """
        断言时间戳有效
        
        Args:
            timestamp_str: 时间戳字符串
        """
        try:
            datetime.fromisoformat(timestamp_str)
        except ValueError:
            raise AssertionError(f"时间戳格式无效: {timestamp_str}")
    
    @staticmethod
    def assert_websocket_message_received(messages: list, expected_content: str):
        """
        断言收到包含特定内容的WebSocket消息
        
        Args:
            messages: 消息列表
            expected_content: 期望内容
        """
        found = any(expected_content in str(msg) for msg in messages)
        assert found, f"未找到包含'{expected_content}'的消息"


if __name__ == '__main__':
    # 测试数据生成器
    print("测试数据生成器示例:")
    
    task = TestDataGenerator.generate_task(case_count=2)
    print(f"\n生成的任务数据:")
    print(json.dumps(task, indent=2, ensure_ascii=False))
    
    minimal_task = TestDataGenerator.generate_minimal_task()
    print(f"\n最小任务数据:")
    print(json.dumps(minimal_task, indent=2, ensure_ascii=False))
