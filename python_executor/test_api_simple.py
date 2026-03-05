#!/usr/bin/env python3
"""
HTTP API 简单测试脚本
使用taskNo作为标识符

注意：此测试脚本使用MockTaskExecutor进行模拟执行
真实环境请使用core.task_executor.TaskExecutor
"""
import requests
import time
import json
import sys
import os

# 添加项目根目录到路径
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

BASE_URL = "http://localhost:8180"
API_URL = f"{BASE_URL}/api"

# 是否使用Mock执行器（仅用于测试）
USE_MOCK_EXECUTOR = True

def print_response(response, title=""):
    """打印响应"""
    print(f"\n{'='*60}")
    print(f"{title}")
    print(f"{'='*60}")
    print(f"Status: {response.status_code}")
    try:
        data = response.json()
        print(json.dumps(data, indent=2, ensure_ascii=False))
    except:
        print(response.text)
    print(f"{'='*60}\n")

def test_basic_endpoints():
    """测试基础端点"""
    print("\n🚀 开始测试基础端点...")
    
    # 测试根路径
    resp = requests.get(f"{BASE_URL}/")
    print_response(resp, "1. 服务信息 (GET /)")
    
    # 测试健康检查
    resp = requests.get(f"{BASE_URL}/health")
    print_response(resp, "2. 健康检查 (GET /health)")
    
    # 测试状态
    resp = requests.get(f"{BASE_URL}/status")
    print_response(resp, "3. 系统状态 (GET /status)")
    
    # 测试API状态
    resp = requests.get(f"{API_URL}/status")
    print_response(resp, "4. API状态 (GET /api/status)")

def test_task_lifecycle():
    """测试任务生命周期"""
    print("\n📝 开始测试任务生命周期...")
    
    # 1. 创建任务
    task_no = f"T{int(time.time())}"
    task_data = {
        "projectNo": "P001",
        "taskNo": task_no,
        "taskName": "接口测试任务",
        "toolType": "canoe",  # 小写，与TestToolType枚举一致
        "configPath": "/path/to/config.cfg",
        "caseList": [
            {"caseNo": "C001", "caseName": "用例1"},
            {"caseNo": "C002", "caseName": "用例2"},
            {"caseNo": "C003", "caseName": "用例3"}
        ]
    }
    
    resp = requests.post(f"{API_URL}/tasks", json=task_data)
    print_response(resp, "5. 创建任务 (POST /api/tasks)")
    
    if resp.status_code != 201:
        print("❌ 创建任务失败，跳过后续测试")
        return None
    
    returned_task_no = resp.json()["data"]["taskNo"]
    print(f"✅ 任务创建成功，taskNo: {returned_task_no}")
    
    # 2. 查询任务
    time.sleep(0.5)
    resp = requests.get(f"{API_URL}/tasks/{returned_task_no}")
    print_response(resp, f"6. 查询任务 (GET /api/tasks/{returned_task_no})")
    
    # 3. 获取进度
    resp = requests.get(f"{API_URL}/tasks/{returned_task_no}/progress")
    print_response(resp, f"7. 获取进度 (GET /api/tasks/{returned_task_no}/progress)")
    
    # 4. 等待任务完成并轮询进度
    print("\n⏳ 等待任务完成...")
    max_wait = 10  # 最多等待10秒
    for i in range(max_wait):
        resp = requests.get(f"{API_URL}/tasks/{returned_task_no}/progress")
        data = resp.json()
        status = data.get("data", {}).get("status")
        progress = data.get("data", {}).get("progress", 0)
        print(f"  进度: {progress}%, 状态: {status}")
        
        if status in ["completed", "failed", "cancelled"]:
            break
        time.sleep(1)
    
    # 5. 获取结果
    resp = requests.get(f"{API_URL}/tasks/{returned_task_no}/results")
    print_response(resp, f"8. 获取结果 (GET /api/tasks/{returned_task_no}/results)")
    
    # 6. 获取任务列表
    resp = requests.get(f"{API_URL}/tasks")
    print_response(resp, "9. 任务列表 (GET /api/tasks)")
    
    return returned_task_no

def test_error_cases():
    """测试错误情况"""
    print("\n⚠️ 开始测试错误情况...")
    
    # 1. 查询不存在的任务
    resp = requests.get(f"{API_URL}/tasks/NON_EXISTENT_TASK")
    print_response(resp, "10. 查询不存在的任务")
    
    # 2. 创建无效任务（缺少必填字段）
    invalid_task = {"projectNo": "P001"}  # 缺少taskNo和taskName
    resp = requests.post(f"{API_URL}/tasks", json=invalid_task)
    print_response(resp, "11. 创建无效任务（缺少必填字段）")
    
    # 3. 空请求体
    resp = requests.post(f"{API_URL}/tasks", json={})
    print_response(resp, "12. 空请求体")
    
    # 4. 取消不存在的任务
    resp = requests.delete(f"{API_URL}/tasks/NON_EXISTENT")
    print_response(resp, "13. 取消不存在的任务")

def test_cors():
    """测试CORS"""
    print("\n🌐 测试CORS...")
    resp = requests.options(f"{API_URL}/tasks")
    print(f"CORS Headers: {dict(resp.headers)}")
    if 'Access-Control-Allow-Origin' in resp.headers:
        print("✅ CORS配置正确")
    else:
        print("⚠️ CORS头可能未设置")

def setup_mock_executor():
    """设置Mock执行器（仅用于测试）"""
    if not USE_MOCK_EXECUTOR:
        return
    
    print("\n🔧 设置Mock执行器用于测试...")
    
    # 替换api.task_executor中的执行器
    from api import task_executor
    from tests.mock_executor import MockTaskExecutor
    
    # 创建一个使用MockTaskExecutor的替代函数
    def _get_mock_executor():
        if task_executor._executor_instance is None:
            task_executor._executor_instance = MockTaskExecutor(
                message_sender=task_executor._on_task_message
            )
            print("✅ MockTaskExecutor初始化完成")
        return task_executor._executor_instance
    
    # 替换_get_executor函数
    task_executor._get_executor = _get_mock_executor
    print("✅ 已切换到Mock执行器")

def main():
    """主函数"""
    print("="*60)
    print("🧪 HTTP API 测试脚本 (使用taskNo)")
    print("="*60)
    
    # 设置Mock执行器（仅用于测试）
    setup_mock_executor()
    
    # 等待服务启动
    print("\n⏳ 检查服务是否可用...")
    for i in range(5):
        try:
            resp = requests.get(f"{BASE_URL}/health", timeout=2)
            if resp.status_code == 200:
                print("✅ 服务已启动\n")
                break
        except:
            pass
        time.sleep(1)
    else:
        print("❌ 无法连接到服务，请确保服务已启动")
        print(f"   期望地址: {BASE_URL}")
        return
    
    # 运行测试
    try:
        test_basic_endpoints()
        test_task_lifecycle()
        test_error_cases()
        test_cors()
        
        print("\n" + "="*60)
        print("✅ 所有测试完成！")
        print("="*60)
        
    except Exception as e:
        print(f"\n❌ 测试出错: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    main()
