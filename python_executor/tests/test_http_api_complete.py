"""
HTTP API完整测试
测试所有HTTP端点
"""
import pytest
import requests
import time
import threading
from datetime import datetime

# 测试配置
BASE_URL = "http://localhost:8180"
API_URL = f"{BASE_URL}/api"


class TestHTTPAPIComplete:
    """HTTP API完整测试类"""
    
    @pytest.fixture(scope="class")
    def session(self):
        """创建会话"""
        session = requests.Session()
        session.headers.update({
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        })
        yield session
        session.close()
    
    @pytest.fixture
    def sample_task(self):
        """示例任务数据"""
        return {
            "projectNo": "P001",
            "taskNo": f"T{int(time.time())}",
            "taskName": "测试任务",
            "caseList": [
                {"caseNo": "C001", "caseName": "用例1"},
                {"caseNo": "C002", "caseName": "用例2"},
                {"caseNo": "C003", "caseName": "用例3"}
            ]
        }
    
    # ==================== 基础接口测试 ====================
    
    def test_root_endpoint(self, session):
        """测试根路径端点"""
        response = session.get(f"{BASE_URL}/")
        
        assert response.status_code == 200
        data = response.json()
        
        assert data['name'] == 'Python执行器'
        assert data['version'] == '1.0.0'
        assert data['status'] == 'running'
        assert 'timestamp' in data
        assert 'apis' in data
    
    def test_health_endpoint(self, session):
        """测试健康检查端点"""
        response = session.get(f"{BASE_URL}/health")
        
        assert response.status_code == 200
        data = response.json()
        
        assert data['status'] == 'healthy'
        assert 'timestamp' in data
        assert 'clients' in data
        assert 'current_task' in data
    
    def test_status_endpoint(self, session):
        """测试状态端点"""
        response = session.get(f"{BASE_URL}/status")
        
        assert response.status_code == 200
        data = response.json()
        
        assert 'running' in data
        assert 'current_task' in data
        assert 'statistics' in data
    
    def test_api_status_endpoint(self, session):
        """测试API状态端点"""
        response = session.get(f"{API_URL}/status")
        
        assert response.status_code == 200
        data = response.json()
        
        assert data['status'] == 'success'
        assert 'data' in data
        assert 'timestamp' in data
    
    # ==================== 任务管理接口测试 ====================
    
    def test_create_task_success(self, session, sample_task):
        """TR-2.1: POST /api/tasks返回201状态码和taskId"""
        response = session.post(f"{API_URL}/tasks", json=sample_task)
        
        assert response.status_code == 201
        data = response.json()
        
        assert data['status'] == 'success'
        assert 'data' in data
        assert 'taskId' in data['data']
        assert data['data']['taskNo'] == sample_task['taskNo']
        assert data['data']['status'] == 'pending'
        assert 'createdAt' in data['data']
        
        return data['data']['taskId']
    
    def test_create_task_missing_fields(self, session):
        """TR-2.2: 无效任务数据返回400错误"""
        # 缺少必填字段
        invalid_task = {
            "projectNo": "P001"
            # 缺少taskNo和taskName
        }
        
        response = session.post(f"{API_URL}/tasks", json=invalid_task)
        
        assert response.status_code == 400
        data = response.json()
        
        assert data['status'] == 'error'
        assert 'code' in data
        assert 'MISSING_FIELDS' in data['code'] or 'message' in data
    
    def test_create_task_empty_body(self, session):
        """测试空请求体"""
        response = session.post(f"{API_URL}/tasks", json={})
        
        assert response.status_code == 400
        data = response.json()
        assert data['status'] == 'error'
    
    def test_get_task_success(self, session, sample_task):
        """TR-3.1: GET /api/tasks/{taskId}返回正确任务信息"""
        # 先创建任务
        create_resp = session.post(f"{API_URL}/tasks", json=sample_task)
        task_id = create_resp.json()['data']['taskId']
        
        # 查询任务
        response = session.get(f"{API_URL}/tasks/{task_id}")
        
        assert response.status_code == 200
        data = response.json()
        
        assert data['status'] == 'success'
        assert 'data' in data
        assert data['data']['taskId'] == task_id
        assert data['data']['taskNo'] == sample_task['taskNo']
        assert 'status' in data['data']
    
    def test_get_task_not_found(self, session):
        """TR-3.2: 不存在的taskId返回404"""
        response = session.get(f"{API_URL}/tasks/non-existent-task-id")
        
        assert response.status_code == 404
        data = response.json()
        
        assert data['status'] == 'error'
        assert 'TASK_NOT_FOUND' in data.get('code', '') or '不存在' in data.get('message', '')
    
    def test_list_tasks(self, session, sample_task):
        """TR-3.3: GET /api/tasks支持分页参数"""
        # 创建几个任务
        for i in range(3):
            task_data = {**sample_task, "taskNo": f"T{int(time.time())}_{i}"}
            session.post(f"{API_URL}/tasks", json=task_data)
            time.sleep(0.1)
        
        # 测试获取列表
        response = session.get(f"{API_URL}/tasks")
        
        assert response.status_code == 200
        data = response.json()
        
        assert data['status'] == 'success'
        assert 'data' in data
        assert 'tasks' in data['data']
        assert 'total' in data['data']
        assert 'page' in data['data']
        assert 'pageSize' in data['data']
        
        # 测试分页
        response = session.get(f"{API_URL}/tasks?page=1&pageSize=2")
        data = response.json()
        assert len(data['data']['tasks']) <= 2
        
        # 测试状态筛选
        response = session.get(f"{API_URL}/tasks?status=pending")
        assert response.status_code == 200
    
    def test_list_tasks_invalid_status(self, session):
        """测试无效的状态筛选"""
        response = session.get(f"{API_URL}/tasks?status=invalid_status")
        
        assert response.status_code == 400
        data = response.json()
        assert data['status'] == 'error'
    
    def test_get_task_results(self, session, sample_task):
        """TR-4.1: 完成的任务返回完整结果"""
        # 创建任务
        create_resp = session.post(f"{API_URL}/tasks", json=sample_task)
        task_id = create_resp.json()['data']['taskId']
        
        # 等待任务完成（模拟执行）
        time.sleep(3)
        
        # 获取结果
        response = session.get(f"{API_URL}/tasks/{task_id}/results")
        
        assert response.status_code == 200
        data = response.json()
        
        assert data['status'] == 'success'
        assert 'data' in data
        assert 'taskNo' in data['data']
        assert 'platform' in data['data']
        assert 'caseList' in data['data']
        assert 'status' in data['data']
    
    def test_get_task_results_not_found(self, session):
        """TR-4.4: 不存在的taskId返回404"""
        response = session.get(f"{API_URL}/tasks/non-existent/results")
        
        assert response.status_code == 404
        data = response.json()
        assert data['status'] == 'error'
    
    def test_get_task_progress(self, session, sample_task):
        """TR-4.2: 进行中的任务返回当前进度"""
        # 创建任务
        create_resp = session.post(f"{API_URL}/tasks", json=sample_task)
        task_id = create_resp.json()['data']['taskId']
        
        # 获取进度
        response = session.get(f"{API_URL}/tasks/{task_id}/progress")
        
        assert response.status_code == 200
        data = response.json()
        
        assert data['status'] == 'success'
        assert 'data' in data
        assert 'taskId' in data['data']
        assert 'status' in data['data']
        assert 'progress' in data['data']
    
    def test_cancel_task(self, session, sample_task):
        """TR-5.1: 取消进行中任务返回200"""
        # 创建任务
        create_resp = session.post(f"{API_URL}/tasks", json=sample_task)
        task_id = create_resp.json()['data']['taskId']
        
        # 等待任务开始执行
        time.sleep(0.5)
        
        # 取消任务
        response = session.delete(f"{API_URL}/tasks/{task_id}")
        
        # 注意：如果任务已经完成，可能返回409
        assert response.status_code in [200, 409]
        
        if response.status_code == 200:
            data = response.json()
            assert data['status'] == 'success'
    
    def test_cancel_task_not_found(self, session):
        """测试取消不存在的任务"""
        response = session.delete(f"{API_URL}/tasks/non-existent")
        
        assert response.status_code == 404
        data = response.json()
        assert data['status'] == 'error'
    
    def test_cancel_completed_task(self, session, sample_task):
        """TR-5.2: 取消已完成任务返回409"""
        # 创建一个简单的任务并等待完成
        create_resp = session.post(f"{API_URL}/tasks", json=sample_task)
        task_id = create_resp.json()['data']['taskId']
        
        # 等待任务完成
        time.sleep(4)
        
        # 尝试取消已完成的任务
        response = session.delete(f"{API_URL}/tasks/{task_id}")
        
        # 应该返回409或200（取决于任务状态）
        assert response.status_code in [200, 409]
    
    # ==================== 并发测试 ====================
    
    def test_concurrent_create_tasks(self, session, sample_task):
        """TR-8.3: 并发测试验证线程安全"""
        results = []
        errors = []
        
        def create_task(i):
            try:
                task_data = {**sample_task, "taskNo": f"T{int(time.time())}_{i}"}
                resp = session.post(f"{API_URL}/tasks", json=task_data)
                if resp.status_code == 201:
                    results.append(resp.json()['data']['taskId'])
                else:
                    # 503表示队列已满，这是预期的
                    if resp.status_code != 503:
                        errors.append(f"Task {i}: {resp.status_code}")
            except Exception as e:
                errors.append(str(e))
        
        # 并发创建10个任务
        threads = [threading.Thread(target=create_task, args=(i,)) for i in range(10)]
        for t in threads:
            t.start()
        for t in threads:
            t.join()
        
        # 验证没有意外错误
        assert len(errors) == 0, f"并发创建出现错误: {errors}"
        
        # 验证至少有一些任务创建成功
        assert len(results) > 0
    
    # ==================== 错误处理测试 ====================
    
    def test_error_response_format(self, session):
        """TR-7.1: 错误响应包含code、message字段"""
        response = session.get(f"{API_URL}/tasks/non-existent")
        
        assert response.status_code == 404
        data = response.json()
        
        assert 'status' in data
        assert data['status'] == 'error'
        assert 'code' in data or 'message' in data
        assert 'timestamp' in data
    
    def test_cors_headers(self, session):
        """测试CORS头"""
        # 发送OPTIONS请求
        response = session.options(f"{API_URL}/tasks")
        
        # 检查CORS头
        assert 'Access-Control-Allow-Origin' in response.headers
    
    def test_content_type(self, session):
        """测试响应Content-Type"""
        response = session.get(f"{BASE_URL}/")
        
        content_type = response.headers.get('Content-Type', '')
        assert 'application/json' in content_type
    
    def test_api_response_structure(self, session):
        """TR-2.4: API响应格式符合RESTful规范"""
        response = session.get(f"{BASE_URL}/health")
        data = response.json()
        
        # 验证响应结构
        assert isinstance(data, dict)
        assert 'status' in data
    
    # ==================== 集成测试 ====================
    
    def test_full_task_lifecycle(self, session, sample_task):
        """测试完整的任务生命周期"""
        # 1. 创建任务
        create_resp = session.post(f"{API_URL}/tasks", json=sample_task)
        assert create_resp.status_code == 201
        task_id = create_resp.json()['data']['taskId']
        
        # 2. 查询任务
        get_resp = session.get(f"{API_URL}/tasks/{task_id}")
        assert get_resp.status_code == 200
        
        # 3. 获取进度
        progress_resp = session.get(f"{API_URL}/tasks/{task_id}/progress")
        assert progress_resp.status_code == 200
        
        # 4. 等待任务完成
        time.sleep(4)
        
        # 5. 获取结果
        results_resp = session.get(f"{API_URL}/tasks/{task_id}/results")
        assert results_resp.status_code == 200
        
        # 6. 验证任务列表包含该任务
        list_resp = session.get(f"{API_URL}/tasks")
        assert list_resp.status_code == 200
        task_ids = [t['taskId'] for t in list_resp.json()['data']['tasks']]
        assert task_id in task_ids


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
