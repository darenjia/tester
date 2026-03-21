"""
HTTP API任务执行模块
处理通过HTTP接口下发的任务执行

此模块使用core.task_executor.TaskExecutor进行真实任务执行
"""
import threading
import time
from datetime import datetime
from typing import Dict, Any, Optional

from models.executor_task import Task, TaskStatus, task_queue
from utils.logger import get_logger

logger = get_logger("http_task_executor")

# 全局任务执行器实例
_executor_instance = None
_cancelled_tasks = set()

# 测试执行状态存储（与case_mapping_api共享）
_test_execution_store: Dict[str, Dict[str, Any]] = {}


def _get_executor():
    """
    获取任务执行器实例（懒加载）

    使用真实的TaskExecutor进行任务执行
    """
    global _executor_instance
    if _executor_instance is None:
        from core.task_executor_production import TaskExecutor
        _executor_instance = TaskExecutor(message_sender=_on_task_message)
        _executor_instance.start()
        logger.info("TaskExecutor初始化完成")
    return _executor_instance


def _update_task_in_queue(task_no: str, status: str = None, message: str = None,
                          progress: int = None, result: Dict = None, error_message: str = None):
    """
    更新任务队列中的任务属性

    Args:
        task_no: 任务编号
        status: 新状态
        message: 状态消息
        progress: 进度
        result: 执行结果
        error_message: 错误信息
    """
    task = task_queue.get_task(task_no)
    if not task:
        logger.warning(f"更新任务失败，任务不存在: {task_no}")
        return

    if status:
        task.status = status
        if status == TaskStatus.RUNNING.value and not task.started_at:
            task.started_at = datetime.now().isoformat()
        if status in [TaskStatus.COMPLETED.value, TaskStatus.FAILED.value,
                      TaskStatus.CANCELLED.value, TaskStatus.TIMEOUT.value]:
            task.completed_at = datetime.now().isoformat()

    if message is not None:
        if hasattr(task, 'message'):
            task.message = message
        if task.params and isinstance(task.params, dict):
            task.params['message'] = message

    if progress is not None:
        if hasattr(task, 'progress'):
            task.progress = max(0, min(100, progress))

    if result is not None:
        task.result = result

    if error_message is not None:
        task.error_message = error_message


def _on_task_message(message: Dict[str, Any]):
    """
    任务执行器消息回调
    将执行器的消息同步到任务存储和测试执行状态存储
    """
    try:
        msg_type = message.get("type")
        task_no = message.get("taskNo") or message.get("taskId")

        logger.debug(f"[消息回调] type={msg_type}, task_no={task_no}, message={message}")

        if not task_no:
            logger.warning(f"消息缺少taskNo/taskId: {message}")
            return

        test_id = task_no

        if msg_type == "TASK_STATUS":
            status = message.get("status")
            msg = message.get("message")
            progress = message.get("progress")

            logger.info(f"[TASK_STATUS] task_no={task_no}, status={status}, progress={progress}, msg={msg}")

            _update_task_in_queue(task_no, status=status, message=msg, progress=progress)

            if test_id in _test_execution_store:
                _test_execution_store[test_id]['status'] = status.lower() if status else _test_execution_store[test_id]['status']
                _test_execution_store[test_id]['progress'] = progress if progress is not None else _test_execution_store[test_id]['progress']
                if msg:
                    _test_execution_store[test_id]['logs'].append(msg)

        elif msg_type == "RESULT_REPORT":
            results = message.get("results", [])
            summary = message.get("summary")

            logger.info(f"[RESULT_REPORT] task_no={task_no}, results={results}, summary={summary}")

            _update_task_in_queue(task_no, status=TaskStatus.COMPLETED.value,
                                  message="任务执行完成", progress=100, result={"results": results, "summary": summary})

            if test_id in _test_execution_store:
                _test_execution_store[test_id]['status'] = 'completed'
                _test_execution_store[test_id]['progress'] = 100
                _test_execution_store[test_id]['result'] = {'results': results, 'summary': summary}
                _test_execution_store[test_id]['logs'].append("任务执行完成")

        elif msg_type == "LOG_STREAM":
            result = message.get("result")
            logger.debug(f"[LOG_STREAM] task_no={task_no}, result={result}")

            if result and test_id in _test_execution_store:
                _test_execution_store[test_id]['logs'].append(str(result))

    except Exception as e:
        logger.error(f"处理任务消息失败: {e}", exc_info=True)


def execute_task_async(task_no: str, task_data: Dict[str, Any]):
    """
    异步执行任务

    Args:
        task_no: 任务编号
        task_data: 任务数据
    """
    logger.info(f"[execute_task_async] 收到任务请求: task_no={task_no}, task_data={task_data}")

    def run_task():
        try:
            logger.info(f"[run_task] 开始执行任务: {task_no}")

            _update_task_in_queue(task_no, status=TaskStatus.RUNNING.value,
                                  message="任务开始执行", progress=0)

            executor = _get_executor()
            logger.info(f"[run_task] 获取到执行器: {executor}")

            from models.task import Task
            task = Task.from_dict(task_data)
            logger.info(f"[run_task] 构建任务对象: task.task_id={task.task_id}, task.task_type={task.task_type}, task.tool_type={task.tool_type}")

            result = executor.execute_task(task)
            logger.info(f"[run_task] execute_task 返回: {result}, task_id={task.task_id}")

            wait_count = 0
            while True:
                status = executor.get_current_status()
                current_status = status.get("status")

                if current_status != "idle" and status.get("task_id") == task.task_id:
                    logger.info(f"[run_task] 任务正在执行: task_no={task_no}, status={current_status}")
                    while True:
                        status = executor.get_current_status()
                        if status.get("status") == "idle":
                            break
                        if task_no in _cancelled_tasks:
                            executor.cancel_task(task_no)
                            _cancelled_tasks.discard(task_no)
                            logger.info(f"[run_task] 任务已取消: {task_no}")
                            break
                        time.sleep(0.5)
                    break

                wait_count += 1
                if wait_count % 10 == 0:
                    logger.info(f"[run_task] 等待任务开始执行: task_no={task_no}, 当前状态={current_status}, 队列大小={status.get('queue_size')}")

                if task_no in _cancelled_tasks:
                    logger.info(f"[run_task] 任务在等待期间被取消: {task_no}")
                    break

                time.sleep(0.5)

            logger.info(f"[run_task] 任务执行完成: {task_no}")

        except Exception as e:
            logger.error(f"[run_task] 任务执行失败: {task_no}, error={e}", exc_info=True)
            _update_task_in_queue(task_no, status=TaskStatus.FAILED.value,
                                  message=f"任务执行失败: {str(e)}", error_message=str(e))

    thread = threading.Thread(target=run_task, daemon=True)
    thread.start()
    logger.info(f"[execute_task_async] 已启动执行线程: task_no={task_no}")


def cancel_task_execution(task_no: str):
    """
    取消任务执行

    Args:
        task_no: 任务编号
    """
    _cancelled_tasks.add(task_no)

    try:
        executor = _get_executor()
        executor.cancel_task()
        logger.info(f"已发送取消任务指令: {task_no}")
    except Exception as e:
        logger.warning(f"通过执行器取消任务失败: {e}")