"""
HTTP API任务执行模块
处理通过HTTP接口下发的任务执行

此模块使用core.task_executor.TaskExecutor进行真实任务执行
"""
import threading
import time
from datetime import datetime
from typing import Dict, Any, Optional, List

from models.executor_task import Task, TaskStatus, task_queue
from core.case_mapping_manager import get_case_mapping_manager
from core.task_compiler import TaskCompiler
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


def enrich_task_data_from_mappings(task_data: Dict[str, Any]) -> Dict[str, Any]:
    """
    从用例映射库丰富任务数据

    优先从映射库获取以下信息：
    - toolType: 测试工具类型
    - configPath: 配置文件路径(script_path)
    - ini_config: INI配置

    Args:
        task_data: 原始任务数据

    Returns:
        丰富后的任务数据
    """
    mapping_manager = get_case_mapping_manager()
    case_list = task_data.get('caseList', [])

    if not case_list:
        logger.info("[enrich_task_data_from_mappings] caseList为空，跳过映射丰富")
        return task_data

    # 1. 确定tool_type（如果请求中没有指定）
    tool_type = task_data.get('toolType')
    if not tool_type:
        tool_types = set()
        for case in case_list:
            case_no = case.get('caseNo') or case.get('case_no')
            if case_no:
                mapping = mapping_manager.get_mapping(case_no)
                if mapping and mapping.category:
                    tool_types.add(mapping.category.lower())

        if len(tool_types) == 1:
            tool_type = list(tool_types)[0]
            task_data['toolType'] = tool_type
            logger.info(f"[enrich_task_data_from_mappings] 从映射库确定toolType: {tool_type}")
        elif len(tool_types) > 1:
            tool_type = list(tool_types)[0]
            task_data['toolType'] = tool_type
            logger.warning(f"[enrich_task_data_from_mappings] 多种tool_type: {tool_types}，使用第一个: {tool_type}")

    # 2. 尝试获取configPath（如果请求中没有指定）
    config_path = task_data.get('configPath')
    if not config_path:
        # 从第一个有效映射获取configPath
        for case in case_list:
            case_no = case.get('caseNo') or case.get('case_no')
            if case_no:
                mapping = mapping_manager.get_mapping(case_no)
                if mapping and mapping.enabled and mapping.script_path:
                    config_path = mapping.script_path
                    task_data['configPath'] = config_path
                    logger.info(f"[enrich_task_data_from_mappings] 从映射库获取configPath: case_no={case_no}, path={config_path}")
                    break

    # 3. 丰富caseList中的用例信息
    enriched_case_list = []
    for case in case_list:
        case_no = case.get('caseNo') or case.get('case_no')
        if case_no:
            mapping = mapping_manager.get_mapping(case_no)
            if mapping:
                # 用映射信息丰富用例数据
                enriched_case = case.copy()
                if mapping.case_name and not case.get('caseName'):
                    enriched_case['caseName'] = mapping.case_name
                if mapping.script_path and not case.get('scriptPath'):
                    enriched_case['scriptPath'] = mapping.script_path
                if mapping.ini_config and not case.get('iniConfig'):
                    enriched_case['iniConfig'] = mapping.ini_config
                if mapping.para_config and not case.get('paraConfig'):
                    enriched_case['paraConfig'] = mapping.para_config
                enriched_case_list.append(enriched_case)
                logger.debug(f"[enrich_task_data_from_mappings] 用例已丰富: case_no={case_no}")
                continue

        # 没有映射，保留原始数据
        enriched_case_list.append(case)

    task_data['caseList'] = enriched_case_list

    logger.info(f"[enrich_task_data_from_mappings] 任务数据丰富完成: toolType={task_data.get('toolType')}, configPath={task_data.get('configPath')}, cases={len(enriched_case_list)}")

    return task_data


def _build_compile_payload(task_no: str, task_data: Dict[str, Any]) -> Dict[str, Any]:
    compile_payload = {
        "taskNo": task_data.get("taskNo", task_no),
        "projectNo": task_data.get("projectNo", ""),
        "taskName": task_data.get("taskName", ""),
        "deviceId": task_data.get("deviceId"),
        "toolType": task_data.get("toolType"),
        "configPath": task_data.get("configPath"),
        "configName": task_data.get("configName"),
        "baseConfigDir": task_data.get("baseConfigDir"),
        "variables": task_data.get("variables", {}),
        "canoeNamespace": task_data.get("canoeNamespace"),
        "timeout": task_data.get("timeout", 3600),
        "testItems": [],
    }

    for case_data in task_data.get("caseList", []):
        compile_payload["testItems"].append(
            {
                "caseNo": case_data.get("caseNo") or case_data.get("case_no", ""),
                "name": case_data.get("caseName") or case_data.get("name", ""),
                "type": case_data.get("caseType", "test_module"),
                "dtcInfo": case_data.get("dtcInfo") or case_data.get("dtc_info"),
                "params": case_data.get("params", {}),
                "repeat": case_data.get("repeat", 1),
            }
        )

    return compile_payload


def _compile_execution_plan(task_no: str, task_data: Dict[str, Any]):
    compiler = TaskCompiler(get_case_mapping_manager())
    compile_payload = _build_compile_payload(task_no, task_data)
    return compiler.compile_payload(compile_payload)


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
            execution_plan = _compile_execution_plan(task_no, task_data.copy())
            logger.info(
                f"[run_task] 编译执行计划完成: task_no={execution_plan.task_no}, "
                f"tool_type={execution_plan.tool_type}, configPath={execution_plan.config_path}, "
                f"cases={len(execution_plan.cases)}"
            )

            result = executor.execute_plan(execution_plan)
            logger.info(f"[run_task] execute_plan 返回: {result}, task_id={execution_plan.task_no}")

            if not result:
                error_message = "提交执行计划失败"
                logger.error(f"[run_task] {error_message}: task_no={task_no}")
                _update_task_in_queue(
                    task_no,
                    status=TaskStatus.FAILED.value,
                    message=error_message,
                    error_message=error_message,
                )
                return

            wait_count = 0
            while True:
                status = executor.get_current_status()
                current_status = status.get("status")

                if current_status != "idle" and status.get("task_id") == execution_plan.task_no:
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
        executor.cancel_task(task_no)
        logger.info(f"已发送取消任务指令: {task_no}")
    except Exception as e:
        logger.warning(f"通过执行器取消任务失败: {e}")
