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


def _on_task_message(message: Dict[str, Any]):
    """
    任务执行器消息回调
    将执行器的消息同步到任务存储和测试执行状态存储
    """
    try:
        msg_type = message.get("type")
        # 支持taskNo或taskId字段
        task_no = message.get("taskNo") or message.get("taskId")

        logger.debug(f"[消息回调] type={msg_type}, task_no={task_no}, message={message}")

        if not task_no:
            logger.warning(f"消息缺少taskNo/taskId: {message}")
            return

        # 用于查找对应的test_id（task_no即test_id）
        test_id = task_no

        if msg_type == "TASK_STATUS":
            status = message.get("status")
            msg = message.get("message")
            progress = message.get("progress")

            logger.info(f"[TASK_STATUS] task_no={task_no}, status={status}, progress={progress}, msg={msg}")

            # 转换状态
            try:
                task_status = TaskStatus(status.lower())
                task_store.update_task_status(task_no, task_status, msg, progress)
            except ValueError:
                logger.warning(f"未知的状态值: {status}")

            # 同步状态到_test_execution_store（用于前端轮询）
            if test_id in _test_execution_store:
                _test_execution_store[test_id]['status'] = status.lower() if status else _test_execution_store[test_id]['status']
                _test_execution_store[test_id]['progress'] = progress if progress is not None else _test_execution_store[test_id]['progress']
                if msg:
                    _test_execution_store[test_id]['logs'].append(msg)

        elif msg_type == "RESULT_REPORT":
            # 最终结果报告
            results = message.get("results", [])
            summary = message.get("summary")

            logger.info(f"[RESULT_REPORT] task_no={task_no}, results={results}, summary={summary}")

            task_store.update_task_results(task_no, results, summary)

            # 更新为完成状态
            task_store.update_task_status(
                task_no,
                TaskStatus.COMPLETED,
                message="任务执行完成",
                progress=100
            )

            # 同步完成状态到_test_execution_store
            if test_id in _test_execution_store:
                _test_execution_store[test_id]['status'] = 'completed'
                _test_execution_store[test_id]['progress'] = 100
                _test_execution_store[test_id]['result'] = {'results': results, 'summary': summary}
                _test_execution_store[test_id]['logs'].append("任务执行完成")

        elif msg_type == "LOG_STREAM":
            # 执行日志，可以存储或转发
            result = message.get("result")
            logger.debug(f"[LOG_STREAM] task_no={task_no}, result={result}")

            if result:
                task_store.add_task_result(task_no, result)

                # 同步日志到_test_execution_store
                if test_id in _test_execution_store:
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

            # 更新任务状态为运行中
            task_store.update_task_status(
                task_no,
                TaskStatus.RUNNING,
                message="任务开始执行",
                progress=0
            )

            # 获取执行器
            executor = _get_executor()
            logger.info(f"[run_task] 获取到执行器: {executor}")

            # 构建任务对象
            from models.task import Task
            task = Task.from_dict(task_data)
            logger.info(f"[run_task] 构建任务对象: task.task_id={task.task_id}, task.task_type={task.task_type}, task.tool_type={task.tool_type}")

            # 执行任务
            result = executor.execute_task(task)
            logger.info(f"[run_task] execute_task 返回: {result}, task_id={task.task_id}")

            # 等待任务完成
            wait_count = 0
            while True:
                status = executor.get_current_status()
                current_status = status.get("status")

                # 检查任务是否已开始执行（不再是idle状态，或者current_task匹配）
                if current_status != "idle" and status.get("task_id") == task.task_id:
                    logger.info(f"[run_task] 任务正在执行: task_no={task_no}, status={current_status}")
                    # 任务已开始，进入正常等待
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

                # 如果还是idle，检查是否任务在队列中还没开始
                wait_count += 1
                if wait_count % 10 == 0:  # 每5秒打印一次日志
                    logger.info(f"[run_task] 等待任务开始执行: task_no={task_no}, 当前状态={current_status}, 队列大小={status.get('queue_size')}")

                # 检查是否被取消
                if task_no in _cancelled_tasks:
                    logger.info(f"[run_task] 任务在等待期间被取消: {task_no}")
                    break

                time.sleep(0.5)

            logger.info(f"[run_task] 任务执行完成: {task_no}")

        except Exception as e:
            logger.error(f"[run_task] 任务执行失败: {task_no}, error={e}", exc_info=True)
            task_store.set_task_error(task_no, str(e))

    # 启动执行线程
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
    
    # 尝试通过执行器取消
    try:
        executor = _get_executor()
        executor.cancel_task()
        logger.info(f"已发送取消任务指令: {task_no}")
    except Exception as e:
        logger.warning(f"通过执行器取消任务失败: {e}")
