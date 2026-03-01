"""
场景测试模块
测试端到端场景
"""
import pytest
import time
import json
from datetime import datetime

# 导入测试客户端
from .test_client import HTTPTestClient, WebSocketTestClient, TestDataGenerator, AssertionHelper


class TestEndToEndScenarios:
    """端到端场景测试类"""
    
    def test_complete_task_execution_flow(self):
        """测试完整的任务执行流程"""
        # 1. HTTP检查执行器状态
        with HTTPTestClient() as http_client:
            health = http_client.health_check()
            AssertionHelper.assert_response_status(health, 'healthy')
            print("✓ 执行器健康检查通过")
        
        # 2. WebSocket连接并发送任务
        with WebSocketTestClient() as ws_client:
            # 连接
            assert ws_client.connect(), "WebSocket连接失败"
            print("✓ WebSocket连接成功")
            
            # 等待欢迎消息
            welcome = ws_client.wait_for_event('welcome', timeout=5)
            assert welcome is not None, "未收到欢迎消息"
            print("✓ 收到欢迎消息")
            
            # 3. 生成并发送任务
            task = TestDataGenerator.generate_task(case_count=1)
            task_id = ws_client.send_task_dispatch(task)
            print(f"✓ 任务已发送: {task_id}")
            
            # 4. 等待任务确认
            ack = ws_client.wait_for_event('message_response', timeout=5)
            assert ack is not None, "未收到任务确认"
            assert ack.get('status') == 'received', "任务确认状态错误"
            print("✓ 收到任务确认")
            
            # 5. 等待状态更新
            status_update = ws_client.wait_for_event('status_update', timeout=10)
            if status_update:
                print(f"✓ 收到状态更新: {status_update}")
            
            # 6. 发送心跳
            ws_client.send_heartbeat()
            heartbeat_response = ws_client.wait_for_event('heartbeat_response', timeout=3)
            assert heartbeat_response is not None, "未收到心跳响应"
            print("✓ 心跳机制正常")
        
        print("\n✅ 完整任务执行流程测试通过")
    
    def test_task_cancel_scenario(self):
        """测试任务取消场景"""
        with WebSocketTestClient() as ws_client:
            # 连接
            ws_client.connect()
            
            # 发送任务
            task = TestDataGenerator.generate_task(case_count=1)
            task_id = ws_client.send_task_dispatch(task)
            print(f"✓ 任务已发送: {task_id}")
            
            # 等待任务确认
            ack = ws_client.wait_for_event('message_response', timeout=5)
            assert ack is not None
            
            # 等待一小段时间
            time.sleep(1)
            
            # 取消任务
            ws_client.send_task_cancel(task_id)
            print(f"✓ 取消任务已发送: {task_id}")
            
            # 等待取消确认
            cancel_ack = ws_client.wait_for_event('message_response', timeout=5)
            assert cancel_ack is not None
            print("✓ 收到取消确认")
        
        print("\n✅ 任务取消场景测试通过")
    
    def test_multiple_tasks_sequential(self):
        """测试顺序执行多个任务"""
        with WebSocketTestClient() as ws_client:
            ws_client.connect()
            
            task_ids = []
            for i in range(3):
                # 生成任务
                task = TestDataGenerator.generate_task(
                    task_no=f"BATCH-TASK-{i+1:03d}",
                    case_count=1
                )
                
                # 发送任务
                task_id = ws_client.send_task_dispatch(task)
                task_ids.append(task_id)
                print(f"✓ 任务 {i+1} 已发送: {task_id}")
                
                # 等待确认
                ack = ws_client.wait_for_event('message_response', timeout=5)
                assert ack is not None
                
                # 短暂等待
                time.sleep(0.5)
            
            print(f"\n✅ 顺序执行 {len(task_ids)} 个任务测试通过")
    
    def test_concurrent_connections(self):
        """测试多个客户端并发连接"""
        import threading
        
        results = []
        
        def client_task(client_id):
            try:
                with WebSocketTestClient() as ws_client:
                    ws_client.connect()
                    
                    # 发送心跳
                    ws_client.send_heartbeat()
                    response = ws_client.wait_for_event('heartbeat_response', timeout=3)
                    
                    results.append({
                        'client_id': client_id,
                        'success': response is not None
                    })
            except Exception as e:
                results.append({
                    'client_id': client_id,
                    'success': False,
                    'error': str(e)
                })
        
        # 启动多个客户端线程
        threads = []
        for i in range(5):
            t = threading.Thread(target=client_task, args=(i,))
            threads.append(t)
            t.start()
        
        # 等待所有线程完成
        for t in threads:
            t.join(timeout=15)
        
        # 验证结果
        success_count = sum(1 for r in results if r['success'])
        print(f"✓ {success_count}/{len(results)} 个客户端连接成功")
        
        assert success_count == len(results), f"部分客户端连接失败: {results}"
        print("\n✅ 并发连接测试通过")


class TestErrorHandlingScenarios:
    """错误处理场景测试类"""
    
    def test_invalid_task_data(self):
        """测试无效任务数据处理"""
        with WebSocketTestClient() as ws_client:
            ws_client.connect()
            
            # 发送无效格式的任务数据
            ws_client.send_event('message', {
                'type': 'TASK_DISPATCH',
                'taskId': 'INVALID-TASK',
                'payload': 'invalid data format',  # 应该是字典
                'timestamp': int(time.time() * 1000)
            })
            
            # 等待错误响应或确认
            responses = ws_client.receive_all(duration=3)
            
            # 验证连接仍然有效
            assert ws_client.is_connected(), "连接应保持有效"
            print("✓ 无效数据处理正确，连接保持")
        
        print("\n✅ 无效任务数据测试通过")
    
    def test_empty_task_list(self):
        """测试空任务列表"""
        with WebSocketTestClient() as ws_client:
            ws_client.connect()
            
            # 发送空任务列表
            task = TestDataGenerator.generate_minimal_task()
            task_id = ws_client.send_task_dispatch(task)
            
            # 等待确认
            ack = ws_client.wait_for_event('message_response', timeout=5)
            assert ack is not None
            print("✓ 空任务列表处理正确")
        
        print("\n✅ 空任务列表测试通过")
    
    def test_reconnection_after_disconnect(self):
        """测试断线重连"""
        # 第一次连接
        ws1 = WebSocketTestClient()
        ws1.connect()
        assert ws1.is_connected()
        ws1.disconnect()
        assert not ws1.is_connected()
        print("✓ 第一次连接和断开")
        
        # 等待一下
        time.sleep(1)
        
        # 重新连接
        ws2 = WebSocketTestClient()
        ws2.connect()
        assert ws2.is_connected()
        
        # 发送心跳验证连接正常
        ws2.send_heartbeat()
        response = ws2.wait_for_event('heartbeat_response', timeout=3)
        assert response is not None
        
        ws2.disconnect()
        print("✓ 重新连接成功")
        
        print("\n✅ 断线重连测试通过")


class TestPerformanceScenarios:
    """性能测试场景类"""
    
    def test_http_response_time(self):
        """测试HTTP响应时间"""
        with HTTPTestClient() as http_client:
            # 测试健康检查响应时间
            times = []
            for i in range(10):
                start = time.time()
                health = http_client.health_check()
                elapsed = time.time() - start
                times.append(elapsed)
            
            avg_time = sum(times) / len(times)
            max_time = max(times)
            min_time = min(times)
            
            print(f"✓ HTTP响应时间统计:")
            print(f"  - 平均: {avg_time*1000:.1f}ms")
            print(f"  - 最大: {max_time*1000:.1f}ms")
            print(f"  - 最小: {min_time*1000:.1f}ms")
            
            # 断言平均响应时间小于500ms
            assert avg_time < 0.5, f"平均响应时间应小于500ms，实际{avg_time*1000:.1f}ms"
        
        print("\n✅ HTTP响应时间测试通过")
    
    def test_websocket_message_throughput(self):
        """测试WebSocket消息吞吐量"""
        with WebSocketTestClient() as ws_client:
            ws_client.connect()
            
            # 发送多个消息
            message_count = 20
            start_time = time.time()
            
            for i in range(message_count):
                ws_client.send_heartbeat()
                time.sleep(0.05)  # 50ms间隔
            
            # 接收所有响应
            responses = ws_client.receive_all(duration=5)
            elapsed = time.time() - start_time
            
            # 计算吞吐量
            throughput = len(responses) / elapsed if elapsed > 0 else 0
            
            print(f"✓ WebSocket消息吞吐量:")
            print(f"  - 发送消息数: {message_count}")
            print(f"  - 接收响应数: {len(responses)}")
            print(f"  - 总耗时: {elapsed:.2f}s")
            print(f"  - 吞吐量: {throughput:.1f} 消息/秒")
        
        print("\n✅ WebSocket吞吐量测试通过")
    
    def test_large_task_payload(self):
        """测试大任务负载"""
        with WebSocketTestClient() as ws_client:
            ws_client.connect()
            
            # 生成包含多个用例的任务
            task = TestDataGenerator.generate_task(case_count=50)
            
            # 计算任务大小
            task_json = json.dumps(task)
            task_size = len(task_json.encode('utf-8'))
            
            print(f"✓ 任务负载大小: {task_size} 字节")
            
            # 发送任务
            start_time = time.time()
            task_id = ws_client.send_task_dispatch(task)
            
            # 等待确认
            ack = ws_client.wait_for_event('message_response', timeout=10)
            elapsed = time.time() - start_time
            
            assert ack is not None, "大任务应被正确处理"
            print(f"✓ 大任务处理时间: {elapsed:.2f}s")
        
        print("\n✅ 大任务负载测试通过")


class TestIntegrationScenarios:
    """集成测试场景类"""
    
    def test_http_and_websocket_integration(self):
        """测试HTTP和WebSocket集成"""
        # 1. HTTP获取初始状态
        with HTTPTestClient() as http_client:
            initial_status = http_client.get_status()
            initial_clients = initial_status.get('clients', 0)
            print(f"✓ 初始客户端数: {initial_clients}")
        
        # 2. WebSocket连接
        with WebSocketTestClient() as ws_client:
            ws_client.connect()
            time.sleep(1)  # 等待状态更新
            
            # 3. HTTP检查客户端数增加
            with HTTPTestClient() as http_client:
                new_status = http_client.get_status()
                new_clients = new_status.get('clients', 0)
                print(f"✓ 连接后客户端数: {new_clients}")
                
                assert new_clients > initial_clients, "客户端数应增加"
        
        # 4. 等待断开连接
        time.sleep(2)
        
        # 5. HTTP检查客户端数恢复
        with HTTPTestClient() as http_client:
            final_status = http_client.get_status()
            final_clients = final_status.get('clients', 0)
            print(f"✓ 断开后客户端数: {final_clients}")
            
            assert final_clients == initial_clients, "客户端数应恢复"
        
        print("\n✅ HTTP和WebSocket集成测试通过")
    
    def test_full_workflow_simulation(self):
        """测试完整工作流模拟"""
        print("开始完整工作流模拟...")
        
        # 步骤1: 检查执行器状态
        with HTTPTestClient() as http_client:
            info = http_client.get_info()
            assert info['status'] == 'running'
            print("1️⃣ 执行器状态检查通过")
        
        # 步骤2: 建立WebSocket连接
        with WebSocketTestClient() as ws_client:
            ws_client.connect()
            welcome = ws_client.wait_for_event('welcome', timeout=5)
            assert welcome is not None
            print("2️⃣ WebSocket连接建立")
            
            # 步骤3: 发送任务
            task = TestDataGenerator.generate_task(case_count=2)
            task_id = ws_client.send_task_dispatch(task)
            print(f"3️⃣ 任务已发送: {task_id}")
            
            # 步骤4: 等待确认
            ack = ws_client.wait_for_event('message_response', timeout=5)
            assert ack['status'] == 'received'
            print("4️⃣ 任务确认接收")
            
            # 步骤5: 持续心跳
            for i in range(3):
                ws_client.send_heartbeat()
                response = ws_client.wait_for_event('heartbeat_response', timeout=3)
                assert response is not None
                time.sleep(1)
            print("5️⃣ 心跳机制正常")
            
            # 步骤6: 检查执行器状态
            with HTTPTestClient() as http_client:
                status = http_client.get_status()
                assert status['running'] is True
                print("6️⃣ 执行器运行正常")
        
        print("\n✅ 完整工作流模拟通过")


if __name__ == '__main__':
    pytest.main([__file__, '-v'])
