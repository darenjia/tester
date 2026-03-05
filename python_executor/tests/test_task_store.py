"""
TaskStore模块测试
"""
import pytest
import threading
import time
from datetime import datetime, timedelta

from core.task_store import TaskStore, TaskStatus, TaskInfo


class TestTaskStore:
    """TaskStore测试类"""
    
    @pytest.fixture
    def store(self):
        """创建一个新的TaskStore实例"""
        # 由于TaskStore是单例，我们需要重置它
        store = TaskStore()
        store._tasks = {}
        store._task_order = []
        return store
    
    @pytest.fixture
    def sample_task_data(self):
        """示例任务数据"""
        return {
            "projectNo": "P001",
            "taskNo": "T001",
            "taskName": "测试任务",
            "caseList": [
                {"caseNo": "C001", "caseName": "用例1"},
                {"caseNo": "C002", "caseName": "用例2"}
            ]
        }
    
    def test_create_task(self, store, sample_task_data):
        """测试创建任务"""
        task = store.create_task(sample_task_data)
        
        assert task is not None
        assert task.task_id is not None
        assert task.project_no == "P001"
        assert task.task_no == "T001"
        assert task.task_name == "测试任务"
        assert task.status == TaskStatus.PENDING
        assert task.progress == 0
        
        # TR-1.1: 任务创建后可以通过taskId查询
        retrieved = store.get_task(task.task_id)
        assert retrieved is not None
        assert retrieved.task_id == task.task_id
    
    def test_get_task_not_found(self, store):
        """测试查询不存在的任务"""
        result = store.get_task("non-existent-id")
        assert result is None
    
    def test_get_task_by_task_no(self, store, sample_task_data):
        """测试通过taskNo查询任务"""
        task = store.create_task(sample_task_data)
        
        retrieved = store.get_task_by_task_no("T001")
        assert retrieved is not None
        assert retrieved.task_id == task.task_id
        
        # 查询不存在的taskNo
        not_found = store.get_task_by_task_no("NON_EXISTENT")
        assert not_found is None
    
    def test_update_task_status(self, store, sample_task_data):
        """测试更新任务状态"""
        task = store.create_task(sample_task_data)
        
        # 更新为运行中
        result = store.update_task_status(
            task.task_id, 
            TaskStatus.RUNNING,
            message="开始执行",
            progress=0
        )
        assert result is True
        
        # TR-1.2: 任务状态更新后查询返回最新状态
        updated = store.get_task(task.task_id)
        assert updated.status == TaskStatus.RUNNING
        assert updated.message == "开始执行"
        assert updated.started_at is not None
        
        # 更新进度
        store.update_task_status(task.task_id, TaskStatus.RUNNING, progress=50)
        updated = store.get_task(task.task_id)
        assert updated.progress == 50
        
        # 完成任务
        store.update_task_status(task.task_id, TaskStatus.COMPLETED, progress=100)
        updated = store.get_task(task.task_id)
        assert updated.status == TaskStatus.COMPLETED
        assert updated.completed_at is not None
    
    def test_update_nonexistent_task(self, store):
        """测试更新不存在的任务"""
        result = store.update_task_status("non-existent", TaskStatus.RUNNING)
        assert result is False
    
    def test_list_tasks(self, store, sample_task_data):
        """测试获取任务列表"""
        # 创建多个任务
        task1 = store.create_task(sample_task_data)
        task2 = store.create_task({**sample_task_data, "taskNo": "T002"})
        task3 = store.create_task({**sample_task_data, "taskNo": "T003"})
        
        # 更新状态
        store.update_task_status(task1.task_id, TaskStatus.RUNNING)
        store.update_task_status(task2.task_id, TaskStatus.COMPLETED)
        
        # 测试获取所有任务
        result = store.list_tasks()
        assert result["total"] == 3
        assert len(result["tasks"]) == 3
        
        # 测试状态筛选
        running_tasks = store.list_tasks(status=TaskStatus.RUNNING)
        assert running_tasks["total"] == 1
        assert running_tasks["tasks"][0]["taskNo"] == "T001"
        
        # 测试分页
        page1 = store.list_tasks(page=1, page_size=2)
        assert page1["total"] == 3
        assert len(page1["tasks"]) == 2
        assert page1["page"] == 1
        assert page1["totalPages"] == 2
        
        page2 = store.list_tasks(page=2, page_size=2)
        assert len(page2["tasks"]) == 1
    
    def test_update_task_results(self, store, sample_task_data):
        """测试更新任务结果"""
        task = store.create_task(sample_task_data)
        
        results = [
            {"caseNo": "C001", "result": "PASS"},
            {"caseNo": "C002", "result": "FAIL"}
        ]
        summary = {"total": 2, "passed": 1, "failed": 1}
        
        result = store.update_task_results(task.task_id, results, summary)
        assert result is True
        
        updated = store.get_task(task.task_id)
        assert len(updated.results) == 2
        assert updated.summary == summary
    
    def test_add_task_result(self, store, sample_task_data):
        """测试添加单个任务结果"""
        task = store.create_task(sample_task_data)
        
        store.add_task_result(task.task_id, {"caseNo": "C001", "result": "PASS"})
        store.add_task_result(task.task_id, {"caseNo": "C002", "result": "FAIL"})
        
        updated = store.get_task(task.task_id)
        assert len(updated.results) == 2
    
    def test_set_task_error(self, store, sample_task_data):
        """测试设置任务错误"""
        task = store.create_task(sample_task_data)
        
        result = store.set_task_error(task.task_id, "执行失败")
        assert result is True
        
        updated = store.get_task(task.task_id)
        assert updated.status == TaskStatus.FAILED
        assert updated.error_message == "执行失败"
        assert updated.completed_at is not None
    
    def test_cancel_task(self, store, sample_task_data):
        """测试取消任务"""
        task = store.create_task(sample_task_data)
        
        # 取消待执行的任务
        result = store.cancel_task(task.task_id)
        assert result is True
        
        updated = store.get_task(task.task_id)
        assert updated.status == TaskStatus.CANCELLED
        assert updated.completed_at is not None
        
        # 尝试取消已完成的任务
        result = store.cancel_task(task.task_id)
        assert result is False
    
    def test_cancel_nonexistent_task(self, store):
        """测试取消不存在的任务"""
        result = store.cancel_task("non-existent")
        assert result is False
    
    def test_delete_task(self, store, sample_task_data):
        """测试删除任务"""
        task = store.create_task(sample_task_data)
        
        result = store.delete_task(task.task_id)
        assert result is True
        
        # 确认已删除
        assert store.get_task(task.task_id) is None
        
        # 再次删除应返回False
        result = store.delete_task(task.task_id)
        assert result is False
    
    def test_get_running_task(self, store, sample_task_data):
        """测试获取正在执行的任务"""
        # 初始时没有运行中的任务
        assert store.get_running_task() is None
        assert store.has_running_task() is False
        
        # 创建并启动任务
        task = store.create_task(sample_task_data)
        store.update_task_status(task.task_id, TaskStatus.RUNNING)
        
        # 应该有运行中的任务
        running = store.get_running_task()
        assert running is not None
        assert running.task_id == task.task_id
        assert store.has_running_task() is True
    
    def test_get_statistics(self, store, sample_task_data):
        """测试获取任务统计"""
        # 创建任务并设置不同状态
        task1 = store.create_task(sample_task_data)
        task2 = store.create_task({**sample_task_data, "taskNo": "T002"})
        task3 = store.create_task({**sample_task_data, "taskNo": "T003"})
        
        store.update_task_status(task1.task_id, TaskStatus.RUNNING)
        store.update_task_status(task2.task_id, TaskStatus.COMPLETED)
        store.cancel_task(task3.task_id)
        
        stats = store.get_statistics()
        assert stats["total"] == 3
        assert stats["pending"] == 0
        assert stats["running"] == 1
        assert stats["completed"] == 1
        assert stats["cancelled"] == 1
    
    def test_cleanup_old_tasks(self, store, sample_task_data):
        """测试清理过期任务"""
        # 创建一个完成的任务
        task = store.create_task(sample_task_data)
        store.update_task_status(task.task_id, TaskStatus.COMPLETED)
        
        # 手动设置完成时间为25小时前
        store._tasks[task.task_id].completed_at = datetime.now() - timedelta(hours=25)
        
        # 清理24小时前的任务
        cleaned = store.cleanup_old_tasks(max_age_hours=24)
        assert cleaned == 1
        assert store.get_task(task.task_id) is None
    
    def test_task_info_to_dict(self, store, sample_task_data):
        """测试TaskInfo转换为字典"""
        task = store.create_task(sample_task_data)
        store.update_task_status(task.task_id, TaskStatus.RUNNING, progress=50)
        
        # 基本转换
        task_dict = task.to_dict()
        assert task_dict["taskId"] == task.task_id
        assert task_dict["taskNo"] == "T001"
        assert task_dict["status"] == "running"
        assert task_dict["progress"] == 50
        assert "createdAt" in task_dict
        
        # 包含原始数据
        task_dict_full = task.to_dict(include_raw=True)
        assert "rawData" in task_dict_full
        assert "results" in task_dict_full
    
    def test_concurrent_create(self, store, sample_task_data):
        """TR-1.3: 测试并发创建任务"""
        task_ids = []
        errors = []
        
        def create_task():
            try:
                task = store.create_task(sample_task_data)
                task_ids.append(task.task_id)
            except Exception as e:
                errors.append(str(e))
        
        # 并发创建100个任务
        threads = [threading.Thread(target=create_task) for _ in range(100)]
        for t in threads:
            t.start()
        for t in threads:
            t.join()
        
        # 验证没有错误
        assert len(errors) == 0, f"并发创建出现错误: {errors}"
        
        # 验证创建了100个不同的任务
        assert len(task_ids) == 100
        assert len(set(task_ids)) == 100  # 所有ID都是唯一的
        
        # 验证存储中的任务数量
        stats = store.get_statistics()
        assert stats["total"] == 100
    
    def test_concurrent_update(self, store, sample_task_data):
        """测试并发更新任务状态"""
        task = store.create_task(sample_task_data)
        errors = []
        
        def update_task():
            try:
                for i in range(10):
                    store.update_task_status(
                        task.task_id, 
                        TaskStatus.RUNNING,
                        progress=i * 10
                    )
                    time.sleep(0.001)
            except Exception as e:
                errors.append(str(e))
        
        # 并发更新
        threads = [threading.Thread(target=update_task) for _ in range(10)]
        for t in threads:
            t.start()
        for t in threads:
            t.join()
        
        # 验证没有数据竞争导致的错误
        assert len(errors) == 0
        
        # 验证任务状态最终是一致的
        final_task = store.get_task(task.task_id)
        assert final_task.status == TaskStatus.RUNNING


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
