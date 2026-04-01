"""
远端结果上报客户端
支持任务执行结果上报和报告文件上传
"""
import json
import time
import os
from typing import Dict, Any, Optional, List
from concurrent.futures import ThreadPoolExecutor

from config.settings import get_config as get_runtime_config
from utils.logger import get_logger
from models.task import TaskResult

logger = get_logger("report_client")


class ReportClient:
    """远端上报客户端"""

    def __init__(self, config_manager=None):
        """
        初始化上报客户端

        Args:
            config_manager: 配置管理器实例，用于获取上报配置
        """
        self.config_manager = config_manager
        self._use_runtime_config = config_manager is None
        self._executor = ThreadPoolExecutor(max_workers=2, thread_name_prefix="report_")
        self._enabled = False
        self._api_url = None
        self._result_api_url = None
        self._file_upload_url = None
        self._timeout = 30
        self._max_retries = 3
        self._retry_delay = 2.0
        self._headers = {}

        self._load_config()

        logger.info(f"ReportClient初始化完成: enabled={self._enabled}, api_url={self._api_url}, file_upload_url={self._file_upload_url}")

    def _load_config(self):
        """从配置管理器加载配置"""
        if self._use_runtime_config:
            self.config_manager = get_runtime_config()

        if self.config_manager is None:
            logger.warning("无法导入配置管理器，使用默认配置")
            return

        self._enabled = self.config_manager.get('report_server.enabled', True)
        if not self._enabled:
            self._enabled = self.config_manager.get('report.enabled', True)

        # 构建完整的 API URL
        report_server_path = self.config_manager.get('report_server.path', '')
        report_server_host = self.config_manager.get('report_server.host', '')
        report_server_port = self.config_manager.get('report_server.port', '')

        if report_server_host and report_server_path:
            self._api_url = f"http://{report_server_host}:{report_server_port}{report_server_path}"
        else:
            self._api_url = self.config_manager.get('report.api_url', '')

        if not self._api_url:
            self._api_url = self.config_manager.get('report.api_url', '')

        self._result_api_url = self.config_manager.get('report.result_api_url', 'http://10.124.11.142:8315/api/python/report')
        self._file_upload_url = self.config_manager.get('report_server.upload_url', 'http://10.124.11.142:8204/upload')
        if not self._file_upload_url:
            self._file_upload_url = self.config_manager.get('report.file_upload_url', 'http://10.124.11.142:8204/upload')
        self._timeout = self.config_manager.get('report_server.timeout', 30)
        self._max_retries = self.config_manager.get('report_server.retry_count', 3)
        self._retry_delay = self.config_manager.get('report.retry_delay', 2.0)
        self._headers = self.config_manager.get('report.headers', {})

        logger.info(f"上报客户端配置加载: enabled={self._enabled}, api_url={self._api_url}, result_api_url={self._result_api_url}")

    @property
    def enabled(self) -> bool:
        """是否启用上报"""
        # 不再每次检查前刷新配置，避免重复加载造成性能问题
        # 配置变更时应调用 reload_config()
        return self._enabled and bool(self._result_api_url)

    def reload_config(self):
        """重新加载配置"""
        self._load_config()

    def report_task_result(self, task_result: TaskResult, task_info: Dict[str, Any] = None,
                          report_file_path: str = None) -> bool:
        """
        上报任务执行结果

        Args:
            task_result: 任务结果对象
            task_info: 额外的任务信息（如projectNo, deviceId等）
            report_file_path: 报告文件路径（可选）

        Returns:
            是否上报成功
        """
        if not self.enabled:
            logger.debug("上报功能未启用，跳过结果上报")
            return False

        try:
            report_url = None
            if report_file_path:
                report_url = self.upload_report_file(report_file_path)
                if report_url:
                    logger.info(f"报告文件上传成功: {report_file_path} -> {report_url}")
                else:
                    logger.warning(f"报告文件上传失败: {report_file_path}")

            if report_url and task_result.results:
                for result in task_result.results:
                    if hasattr(result, 'reAddress') and not result.reAddress:
                        result.reAddress = report_url

            report_data = self._build_report_data(task_result, task_info)

            self._executor.submit(self._do_report, report_data, task_info)

            return True
        except Exception as e:
            logger.error(f"提交结果上报任务失败: {e}")
            return False

    def _build_report_data(self, task_result: TaskResult, task_info: Dict[str, Any] = None) -> Dict[str, Any]:
        """构建上报数据 - TDM2.0格式"""
        data = {
            "taskNo": task_result.taskNo,
            "status": task_result.status,
            "startTime": task_result.startTime.isoformat() if task_result.startTime else None,
            "endTime": task_result.endTime.isoformat() if task_result.endTime else None,
            "duration": 0,
            "summary": task_result.summary or {},
            "results": [r.to_dict() for r in task_result.results] if task_result.results else [],
            "errorMessage": task_result.errorMessage,
            "timestamp": int(time.time() * 1000),
            "platform": "NETWORK"
        }

        # 计算执行时长
        if task_result.startTime and task_result.endTime:
            data["duration"] = (task_result.endTime - task_result.startTime).total_seconds()

        # 合并额外信息
        if task_info:
            data.update(task_info)

        return data

    def _do_report(self, report_data: Dict[str, Any], task_info: Dict[str, Any] = None):
        """执行实际上报（在线程中运行）

        Args:
            report_data: 上报数据
            task_info: 额外的任务信息
        """
        try:
            if not self.report_payload(report_data):
                # 上报失败，持久化以便重试
                self._handle_report_failure(report_data, task_info, "服务器无响应")
        except Exception as e:
            # 上报异常，持久化以便重试
            self._handle_report_failure(report_data, task_info, str(e))

    def report_payload(self, report_data: Dict[str, Any]) -> bool:
        """
        同步上报结果数据

        Args:
            report_data: 已构建的上报数据

        Returns:
            上报成功返回 True，否则返回 False
        """
        if not self.enabled:
            logger.debug("上报功能未启用，跳过结果上报")
            return False

        task_no = report_data.get('taskNo', 'unknown')
        response = self._make_request(
            method="POST",
            url=self._result_api_url,
            json=report_data
        )

        if response is None:
            return False

        logger.info(f"任务结果上报成功: taskNo={task_no}")
        self._reset_failure_counter()
        return True

    def _handle_report_failure(self, report_data: Dict[str, Any],
                                task_info: Dict[str, Any] = None,
                                error: str = "未知错误"):
        """
        处理上报失败 - 持久化以便重试

        Args:
            report_data: 上报数据
            task_info: 额外的任务信息
            error: 错误信息
        """
        task_no = report_data.get('taskNo', 'unknown')
        logger.error(f"任务结果上报失败: taskNo={task_no}, error={error}")

        # 检查是否启用失败持久化
        retry_enabled = True
        if self.config_manager:
            retry_enabled = self.config_manager.get('report_retry.enabled', True)

        if not retry_enabled:
            logger.warning("失败报告持久化未启用，丢弃上报数据")
            return

        try:
            from core.failed_report_manager import get_failed_report_manager

            manager = get_failed_report_manager(self.config_manager)

            # 计算优先级（失败任务优先级更高）
            priority = self._calculate_priority(report_data)

            # 获取最大重试次数
            max_retries = None
            if self.config_manager:
                max_retries = self.config_manager.get('report_retry.max_retries', 10)

            # 持久化失败报告
            report_id = manager.add_failed_report(
                report_data=report_data,
                task_info=task_info,
                max_retries=max_retries,
                priority=priority
            )

            logger.info(f"失败报告已持久化: report_id={report_id}, task_no={task_no}")

        except Exception as e:
            logger.error(f"持久化失败报告时出错: {e}")

    def _calculate_priority(self, report_data: Dict[str, Any]) -> int:
        """
        计算报告优先级

        Args:
            report_data: 上报数据

        Returns:
            优先级（0-10，越高越紧急）
        """
        priority = 0

        # 失败任务优先级更高
        results = report_data.get('results', [])
        for result in results:
            if isinstance(result, dict):
                verdict = result.get('verdict', '')
                if verdict == 'FAIL':
                    priority += 2
                elif verdict == 'BLOCK':
                    priority += 3

        # 任务状态为失败时提高优先级
        status = report_data.get('status', '')
        if status in ['failed', 'FAILED']:
            priority += 3

        return min(priority, 10)

    def _reset_failure_counter(self):
        """重置连续失败计数器"""
        try:
            from core.report_callback_handler import get_callback_handler
            handler = get_callback_handler(self.config_manager)
            handler.reset_consecutive_failures()
        except Exception:
            pass

    def upload_report_file(self, file_path: str) -> Optional[str]:
        """
        上传报告文件

        Args:
            file_path: 文件路径

        Returns:
            文件URL，上传失败返回None
        """
        if not self.enabled:
            logger.debug("上报功能未启用，跳过文件上传")
            return None

        if not self._file_upload_url:
            logger.warning("文件上传URL未配置")
            return None

        if not os.path.exists(file_path):
            logger.error(f"文件不存在: {file_path}")
            return None

        try:
            file_name = os.path.basename(file_path)
            file_size = os.path.getsize(file_path)

            logger.info(f"开始上传文件: {file_name} ({file_size} bytes)")

            with open(file_path, 'rb') as f:
                files = {'file': (file_name, f)}
                response = self._make_request(
                    method="POST",
                    url=self._file_upload_url,
                    files=files
                )

            if response and response.get('code') == 200 and 'data' in response:
                file_url = response['data'].get('url')
                if file_url:
                    logger.info(f"文件上传成功: {file_name} -> {file_url}")
                    return file_url
            logger.error(f"文件上传失败: {file_name}, response: {response}")
            return None

        except Exception as e:
            logger.error(f"文件上传异常: {e}")
            return None

    def upload_report_files(self, file_paths: List[str]) -> List[str]:
        """
        批量上传报告文件

        Args:
            file_paths: 文件路径列表

        Returns:
            成功上传的文件URL列表
        """
        urls = []
        for file_path in file_paths:
            url = self.upload_report_file(file_path)
            if url:
                urls.append(url)
        return urls

    def _make_request(self, method: str, url: str, **kwargs) -> Optional[Dict[str, Any]]:
        """
        发送HTTP请求（带重试机制）

        Args:
            method: 请求方法
            url: 请求URL
            **kwargs: 传递给requests的参数

        Returns:
            响应数据字典，失败返回None
        """
        # 动态导入requests，避免强制依赖
        try:
            import requests
        except ImportError:
            logger.error("requests库未安装，无法发送HTTP请求")
            return None

        # 设置默认超时
        if 'timeout' not in kwargs:
            kwargs['timeout'] = self._timeout

        # 设置请求头
        headers = kwargs.pop('headers', {})
        headers.update(self._headers)
        if 'Content-Type' not in headers and 'files' not in kwargs:
            headers['Content-Type'] = 'application/json'
        kwargs['headers'] = headers

        last_exception = None

        for attempt in range(self._max_retries):
            try:
                logger.debug(f"发送HTTP请求: {method} {url} (尝试 {attempt + 1}/{self._max_retries})")

                response = requests.request(method, url, **kwargs)
                response.raise_for_status()

                # 尝试解析JSON响应
                try:
                    return response.json()
                except ValueError:
                    return {"status": "success", "text": response.text}

            except Exception as e:
                last_exception = e
                logger.warning(f"HTTP请求失败 (尝试 {attempt + 1}/{self._max_retries}): {e}")

                if attempt < self._max_retries - 1:
                    time.sleep(self._retry_delay)

        logger.error(f"HTTP请求最终失败: {last_exception}")
        return None

    def shutdown(self):
        """关闭上报客户端"""
        try:
            self._executor.shutdown(wait=False)
            logger.info("上报客户端已关闭")
        except Exception as e:
            logger.warning(f"关闭上报客户端失败: {e}")


# 全局上报客户端实例
_report_client_instance: Optional[ReportClient] = None


def get_report_client(config_manager=None) -> ReportClient:
    """
    获取上报客户端实例（单例模式）

    Args:
        config_manager: 配置管理器实例

    Returns:
        ReportClient 实例
    """
    global _report_client_instance
    if _report_client_instance is None:
        _report_client_instance = ReportClient(config_manager)
    return _report_client_instance
