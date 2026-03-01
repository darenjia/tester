"""
HTTP API测试模块
测试Flask HTTP端点
"""
import pytest
import requests
import time
from datetime import datetime


class TestHTTPAPI:
    """HTTP API测试类"""
    
    BASE_URL = "http://localhost:8180"
    
    def setup_method(self):
        """每个测试方法前的设置"""
        self.session = requests.Session()
        self.session.headers.update({
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        })
    
    def teardown_method(self):
        """每个测试方法后的清理"""
        if hasattr(self, 'session'):
            self.session.close()
    
    def test_root_endpoint(self):
        """测试根路径端点"""
        response = self.session.get(f"{self.BASE_URL}/")
        
        assert response.status_code == 200, f"期望状态码200，实际{response.status_code}"
        
        data = response.json()
        assert 'name' in data, "响应应包含'name'字段"
        assert 'version' in data, "响应应包含'version'字段"
        assert 'status' in data, "响应应包含'status'字段"
        assert 'timestamp' in data, "响应应包含'timestamp'字段"
        
        assert data['name'] == 'Python执行器', "名称应为'Python执行器'"
        assert data['status'] == 'running', "状态应为'running'"
        assert data['version'] == '1.0.0', "版本应为'1.0.0'"
        
        # 验证时间戳格式
        try:
            datetime.fromisoformat(data['timestamp'])
        except ValueError:
            pytest.fail("timestamp格式不正确")
    
    def test_health_endpoint(self):
        """测试健康检查端点"""
        response = self.session.get(f"{self.BASE_URL}/health")
        
        assert response.status_code == 200, f"期望状态码200，实际{response.status_code}"
        
        data = response.json()
        assert 'status' in data, "响应应包含'status'字段"
        assert 'timestamp' in data, "响应应包含'timestamp'字段"
        assert 'clients' in data, "响应应包含'clients'字段"
        assert 'current_task' in data, "响应应包含'current_task'字段"
        
        assert data['status'] == 'healthy', "健康状态应为'healthy'"
        assert isinstance(data['clients'], int), "clients应为整数"
        assert data['clients'] >= 0, "clients不应为负数"
        
        # 验证时间戳格式
        try:
            datetime.fromisoformat(data['timestamp'])
        except ValueError:
            pytest.fail("timestamp格式不正确")
    
    def test_status_endpoint(self):
        """测试状态查询端点"""
        response = self.session.get(f"{self.BASE_URL}/status")
        
        assert response.status_code == 200, f"期望状态码200，实际{response.status_code}"
        
        data = response.json()
        assert 'clients' in data, "响应应包含'clients'字段"
        assert 'running' in data, "响应应包含'running'字段"
        assert 'uptime' in data, "响应应包含'uptime'字段"
        assert 'current_task' in data, "响应应包含'current_task'字段"
        
        assert isinstance(data['clients'], int), "clients应为整数"
        assert isinstance(data['running'], bool), "running应为布尔值"
        assert isinstance(data['uptime'], (int, float)), "uptime应为数值"
        assert data['uptime'] >= 0, "uptime不应为负数"
    
    def test_cors_headers(self):
        """测试CORS头"""
        response = self.session.options(f"{self.BASE_URL}/")
        
        # 检查CORS相关头
        assert 'Access-Control-Allow-Origin' in response.headers or response.status_code == 200, \
            "应支持CORS或返回200"
    
    def test_response_content_type(self):
        """测试响应Content-Type"""
        response = self.session.get(f"{self.BASE_URL}/")
        
        content_type = response.headers.get('Content-Type', '')
        assert 'application/json' in content_type, f"Content-Type应为application/json，实际{content_type}"
    
    def test_health_endpoint_performance(self):
        """测试健康检查端点性能"""
        start_time = time.time()
        response = self.session.get(f"{self.BASE_URL}/health")
        elapsed_time = time.time() - start_time
        
        assert response.status_code == 200
        assert elapsed_time < 1.0, f"响应时间应小于1秒，实际{elapsed_time:.3f}秒"
    
    def test_concurrent_requests(self):
        """测试并发请求"""
        import concurrent.futures
        
        def make_request():
            return self.session.get(f"{self.BASE_URL}/health")
        
        # 并发发送10个请求
        with concurrent.futures.ThreadPoolExecutor(max_workers=10) as executor:
            futures = [executor.submit(make_request) for _ in range(10)]
            results = [future.result() for future in concurrent.futures.as_completed(futures)]
        
        # 验证所有请求都成功
        assert all(r.status_code == 200 for r in results), "所有并发请求应成功"
        
        # 验证响应数据一致性
        data_list = [r.json() for r in results]
        assert all('status' in d for d in data_list), "所有响应应包含status字段"
    
    def test_invalid_endpoint(self):
        """测试无效端点"""
        response = self.session.get(f"{self.BASE_URL}/invalid_endpoint")
        
        # Flask默认返回404
        assert response.status_code == 404, f"无效端点应返回404，实际{response.status_code}"


class TestHTTPAPIWithMock:
    """使用Mock的HTTP API测试"""
    
    def test_mock_health_endpoint(self, mocker):
        """测试健康检查端点（使用Mock）"""
        # Mock requests.get
        mock_response = mocker.Mock()
        mock_response.status_code = 200
        mock_response.json.return_value = {
            'status': 'healthy',
            'timestamp': datetime.now().isoformat(),
            'clients': 0,
            'current_task': None
        }
        mock_response.headers = {'Content-Type': 'application/json'}
        
        mocker.patch('requests.get', return_value=mock_response)
        
        response = requests.get("http://localhost:8180/health")
        
        assert response.status_code == 200
        data = response.json()
        assert data['status'] == 'healthy'


if __name__ == '__main__':
    pytest.main([__file__, '-v'])
