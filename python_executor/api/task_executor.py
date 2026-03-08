"""
HTTP API任务执行模块
处理通过HTTP接口下发的任务执行

此模块使用core.task_executor.TaskExecutor进行真实任务执行
"""
import threading
import time
from datetime import datetime
from typing import Dict, Any, Optional

from core.task_store import task_store, TaskStatus
from utils.logger import get_logger

logger = get_logger("http_task_executor")

# 全局任务执行器实例
_executor_instance = None
_cancelled_tasks = set()


def _get_executor():
    """
    获取任务执行器实例（懒加载）
    
    使用真实的TaskExecutor进行任务执行
    """
    global _executor_instance
    if _executor_instance is None:
        from core.task_executor import TaskExecutor
        _executor_instance = TaskExecutor(message_sender=_on_task_message)
        logger.info("TaskExecutor初始化完成")
    return _executor_instance


def _on_task_message(message: Dict[str, Any]):
    """
    任务执行器消息回调
    将执行器的消息同步到任务存储
    """
    try:
        msg_type = message.get("type")
        # 支持taskNo或taskId字段
        task_no = message.get("taskNo") or message.get("taskId")
        
        if not task_no:
            logger.warning(f"消息缺少taskNo/taskId: {message}")
            return
        
        if msg_type == "TASK_STATUS":
            status = message.get("status")
            msg = message.get("message")
            progress = message.get("progress")
            
            # 转换状态
            try:
                task_status = TaskStatus(status.lower())
                task_store.update_task_status(task_no, task_status, msg, progress)
            except ValueError:
                logger.warning(f"未知的状态值: {status}")
        
        elif msg_type == "RESULT_REPORT":
            # 最终结果报告
            results = message.get("results", [])
            summary = message.get("summary")
            
            task_store.update_task_results(task_no, results, summary)
            
            # 更新为完成状态
            task_store.update_task_status(
                task_no, 
                TaskStatus.COMPLETED,
                message="任务执行完成",
                progress=100
            )
        
        elif msg_type == "LOG_STREAM":
            # 执行日志，可以存储或转发
            result = message.get("result")
            if result:
                task_store.add_task_result(task_no, result)
    
    except Exception as e:
        logger.error(f"处理任务消息失败: {e}", exc_info=True)


def execute_task_async(task_no: str, task_data: Dict[str, Any]):
    """
    异步执行任务
    
    Args:
        task_no: 任务编号
        task_data: 任务数据
    """
    def run_task():
        try:
            logger.info(f"开始执行任务: {task_no}")
            
            # 更新任务状态为运行中
            task_store.update_task_status(
                task_no,
                TaskStatus.RUNNING,
                message="任务开始执行",
                progress=0
            )
            
            # 获取执行器
            executor = _get_executor()
            
            # 构建任务对象
            from models.task import Task
            task = Task.from_dict(task_data)
            
            # 执行任务
            executor.execute_task(task)
            
            # 等待任务完成
            while True:
                status = executor.get_current_status()
                if status.get("status") == "idle":
                    break
                
                # 检查是否被取消
                if task_no in _cancelled_tasks:
                    executor.cancel_task()
                    _cancelled_tasks.discard(task_no)
                
                time.sleep(0.5)
            
            logger.info(f"任务执行完成: {task_no}")
            
        except Exception as e:
            logger.error(f"任务执行失败: {task_no}, error={e}", exc_info=True)
            task_store.set_task_error(task_no, str(e))
    
    # 启动执行线程
    thread = threading.Thread(target=run_task, daemon=True)
    thread.start()


def cancel_task_execution(task_no: str):
    """
    取消任务执行
    
    Args:
        task_no: 任务编号
    """
    _cancelled_tasks.add(task_no)
    
    # 尝试通过执行器取消
    try:
        executor = _get_executor()
        executor.cancel_task()
        logger.info(f"已发送取消任务指令: {task_no}")
    except Exception as e:
        logger.warning(f"通过执行器取消任务失败: {e}")
