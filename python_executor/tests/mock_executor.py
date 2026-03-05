"""
模拟任务执行器
用于测试和演示，不用于生产环境
"""
import threading
import time
from datetime import datetime
from typing import Dict, Any, Optional, Callable

from utils.logger import get_logger

logger = get_logger("mock_executor")


class MockTaskExecutor:
    """
    模拟任务执行器
    
    用于测试和演示，模拟执行过程并生成随机结果
    不要在生产环境中使用！
    """
    
    def __init__(self, message_sender: Callable = None):
        self.current_task = None
        self._stop_event = threading.Event()
        self._task_thread = None
        self.message_sender = message_sender
        
        logger.info("MockTaskExecutor初始化完成")
    
    def execute_task(self, task):
        """执行模拟任务"""
        self.current_task = task
        self._stop_event.clear()
        
        self._task_thread = threading.Thread(target=self._mock_execution, args=(task,))
        self._task_thread.start()
        return True
    
    def cancel_task(self):
        """取消任务"""
        self._stop_event.set()
        return True
    
    def get_current_status(self):
        """获取当前状态"""
        if self.current_task is None:
            return {"status": "idle"}
        return {
            "status": "running",
            "taskNo": getattr(self.current_task, 'taskNo', 'unknown')
        }
    
    def _send_message(self, message: Dict[str, Any]):
        """发送消息"""
        if self.message_sender:
            self.message_sender(message)
    
    def _mock_execution(self, task):
        """模拟执行过程"""
        try:
            task_no = getattr(task, 'taskNo', 'unknown')
            case_list = getattr(task, 'caseList', [])
            
            if not case_list:
                case_list = [{"caseNo": f"C{i:03d}"} for i in range(1, 6)]
            
            total = len(case_list)
            
            for i, case in enumerate(case_list):
                if self._stop_event.is_set():
                    logger.info(f"任务被取消: {task_no}")
                    self._send_message({
                        "type": "TASK_STATUS",
                        "taskNo": task_no,
                        "status": "cancelled",
                        "message": "任务已取消"
                    })
                    return
                
                # 模拟执行时间
                time.sleep(0.5)
                
                # 更新进度
                progress = int((i + 1) / total * 100)
                case_no = case.get("caseNo", f"C{i+1:03d}") if isinstance(case, dict) else f"C{i+1:03d}"
                
                self._send_message({
                    "type": "TASK_STATUS",
                    "taskNo": task_no,
                    "status": "running",
                    "message": f"执行用例 {case_no} ({i+1}/{total})",
                    "progress": progress
                })
                
                # 添加结果
                self._send_message({
                    "type": "LOG_STREAM",
                    "taskNo": task_no,
                    "result": {
                        "caseNo": case_no,
                        "result": "PASS" if i % 3 != 0 else "FAIL",
                        "remark": f"用例 {case_no} 执行{'成功' if i % 3 != 0 else '失败'}"
                    }
                })
            
            # 发送完成消息
            self._send_message({
                "type": "RESULT_REPORT",
                "taskNo": task_no,
                "status": "completed",
                "results": [],
                "summary": {
                    "total": total,
                    "passed": total - total // 3,
                    "failed": total // 3,
                    "passRate": f"{((total - total // 3) / total * 100):.1f}%"
                }
            })
            
        except Exception as e:
            logger.error(f"模拟执行失败: {e}")
            self._send_message({
                "type": "TASK_STATUS",
                "taskNo": getattr(task, 'taskNo', 'unknown'),
                "status": "failed",
                "message": str(e)
            })
        finally:
            self.current_task = None
