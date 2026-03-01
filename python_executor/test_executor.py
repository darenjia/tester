"""
测试脚本 - 验证Python执行器功能
"""
import json
import time
import websocket
import threading

def test_websocket_connection():
    """测试WebSocket连接"""
    print("测试WebSocket连接...")
    
    try:
        ws = websocket.WebSocket()
        ws.connect("ws://localhost:8180/ws/executor")
        
        # 发送心跳
        ws.send(json.dumps({
            "type": "heartbeat",
            "timestamp": int(time.time() * 1000)
        }))
        
        # 接收响应
        response = ws.recv()
        print(f"收到响应: {response}")
        
        ws.close()
        print("WebSocket连接测试通过")
        return True
        
    except Exception as e:
        print(f"WebSocket连接测试失败: {e}")
        return False

def test_task_dispatch():
    """测试任务下发"""
    print("\n测试任务下发...")
    
    try:
        ws = websocket.WebSocket()
        ws.connect("ws://localhost:8180/ws/executor")
        
        # 构造测试任务
        task_data = {
            "type": "TASK_DISPATCH",
            "taskNo": "TEST_TASK_001",
            "deviceId": "DEVICE_001",
            "toolType": "canoe",
            "configPath": "C:\\TestConfigs\\EngineTest.cfg",
            "testItems": [
                {
                    "name": "初始化检查",
                    "type": "signal_check",
                    "signalName": "EngineStatus",
                    "expectedValue": 0
                },
                {
                    "name": "启动发动机",
                    "type": "signal_set",
                    "signalName": "IgnitionSwitch",
                    "value": 1
                }
            ],
            "timeout": 3600,
            "timestamp": int(time.time() * 1000)
        }
        
        ws.send(json.dumps(task_data))
        print(f"发送任务: {task_data['taskNo']}")
        
        # 接收响应
        response = ws.recv()
        print(f"收到响应: {response}")
        
        # 等待任务完成
        print("等待任务执行...")
        time.sleep(5)
        
        ws.close()
        print("任务下发测试完成")
        return True
        
    except Exception as e:
        print(f"任务下发测试失败: {e}")
        return False

def test_task_cancel():
    """测试任务取消"""
    print("\n测试任务取消...")
    
    try:
        ws = websocket.WebSocket()
        ws.connect("ws://localhost:8180/ws/executor")
        
        # 发送取消任务
        cancel_data = {
            "type": "TASK_CANCEL",
            "taskNo": "TEST_TASK_001",
            "timestamp": int(time.time() * 1000)
        }
        
        ws.send(json.dumps(cancel_data))
        print(f"发送取消任务: {cancel_data['taskNo']}")
        
        # 接收响应
        response = ws.recv()
        print(f"收到响应: {response}")
        
        ws.close()
        print("任务取消测试完成")
        return True
        
    except Exception as e:
        print(f"任务取消测试失败: {e}")
        return False

def main():
    """主测试函数"""
    print("=" * 50)
    print("Python执行器功能测试")
    print("=" * 50)
    
    # 测试用例
    test_cases = [
        ("WebSocket连接", test_websocket_connection),
        ("任务下发", test_task_dispatch),
        ("任务取消", test_task_cancel)
    ]
    
    passed = 0
    total = len(test_cases)
    
    for name, test_func in test_cases:
        print(f"\n>>> 执行测试: {name}")
        if test_func():
            passed += 1
            print(f"✅ {name} - 通过")
        else:
            print(f"❌ {name} - 失败")
    
    print("\n" + "=" * 50)
    print(f"测试结果: {passed}/{total} 通过")
    print("=" * 50)
    
    return passed == total

if __name__ == '__main__':
    success = main()
    exit(0 if success else 1)