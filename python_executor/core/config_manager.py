"""
测试配置管理器
负责根据任务信息生成和写入ini配置文件，查找cfg文件
"""
import os
import configparser
import logging
from typing import Dict, List, Any, Optional
from pathlib import Path

logger = logging.getLogger(__name__)


class TestConfigManager:
    """测试配置管理器"""

    def __init__(self, base_config_dir: str):
        """
        初始化配置管理器

        Args:
            base_config_dir: 基础配置目录路径
        """
        self.base_config_dir = base_config_dir
        self.current_cfg_path = None
        self.current_ini_path = None

    def find_cfg_file(self, task_config_name: str) -> str:
        """
        根据任务配置名称查找cfg文件

        搜索路径顺序：
        1. {base_config_dir}/{config_name}/{config_name}.cfg
        2. {base_config_dir}/{config_name}.cfg
        3. {base_config_dir}/TestProjectFile/{config_name}.cfg
        4. {base_config_dir}/*/{config_name}.cfg (递归搜索)

        Args:
            task_config_name: 任务配置名称（如：COMTest）

        Returns:
            cfg文件的完整路径

        Raises:
            FileNotFoundError: 未找到cfg文件
        """
        # 标准化配置名称
        config_name = task_config_name.replace('.cfg', '')

        # 定义搜索路径列表
        search_paths = [
            os.path.join(self.base_config_dir, config_name, f"{config_name}.cfg"),
            os.path.join(self.base_config_dir, f"{config_name}.cfg"),
            os.path.join(self.base_config_dir, "TestProjectFile", f"{config_name}.cfg"),
        ]

        logger.info(f"查找cfg文件: {config_name}")
        logger.info(f"基础配置目录: {self.base_config_dir}")

        # 按顺序搜索
        for path in search_paths:
            logger.debug(f"搜索路径: {path}")
            if os.path.exists(path):
                logger.info(f"找到cfg文件: {path}")
                return path

        # 如果上面没找到，递归搜索
        logger.info("在子目录中递归搜索...")
        for root, dirs, files in os.walk(self.base_config_dir):
            for file in files:
                if file == f"{config_name}.cfg":
                    full_path = os.path.join(root, file)
                    logger.info(f"找到cfg文件: {full_path}")
                    return full_path

        # 未找到，抛出异常
        error_msg = f"未找到cfg文件: {config_name}，搜索目录: {self.base_config_dir}"
        logger.error(error_msg)
        raise FileNotFoundError(error_msg)

    def write_test_cases_to_ini(self, cfg_path: str, test_cases: List[Dict[str, Any]],
                                variables: Dict[str, Any] = None) -> str:
        """
        将测试用例写入ini配置文件

        Args:
            cfg_path: cfg文件路径
            test_cases: 测试用例列表
            variables: 测试变量值字典

        Returns:
            生成的ini文件路径
        """
        # 根据cfg路径确定ini路径（放在cfg同目录下）
        cfg_dir = os.path.dirname(cfg_path)
        cfg_name = os.path.basename(cfg_path).replace('.cfg', '')
        ini_path = os.path.join(cfg_dir, f"{cfg_name}_test_config.ini")

        config = configparser.ConfigParser()

        # 写入测试用例配置
        config['TestCases'] = {
            'count': str(len(test_cases)),
            'names': ','.join([tc.get('name', '') for tc in test_cases])
        }

        # 写入每个用例的详细配置
        for i, tc in enumerate(test_cases):
            section_name = f'TestCase_{i+1}'
            config[section_name] = {
                'name': tc.get('name', ''),
                'type': tc.get('type', 'normal'),
                'repeat': str(tc.get('repeat', 1)),
                'dtc_info': tc.get('dtc_info', ''),
                'params': str(tc.get('params', {}))
            }

        # 写入测试变量
        if variables:
            config['Variables'] = {k: str(v) for k, v in variables.items()}

        # 写入文件
        with open(ini_path, 'w', encoding='utf-8') as f:
            config.write(f)

        self.current_ini_path = ini_path
        logger.info(f"生成ini配置文件: {ini_path}")
        return ini_path

    def prepare_config_for_task(self, task_config_name: str,
                                test_cases: List[Dict[str, Any]],
                                variables: Dict[str, Any] = None) -> Dict[str, str]:
        """
        为任务准备配置

        Args:
            task_config_name: 任务配置名称
            test_cases: 测试用例列表
            variables: 测试变量值

        Returns:
            包含cfg路径和ini路径的字典
        """
        # 1. 查找cfg文件
        cfg_path = self.find_cfg_file(task_config_name)
        self.current_cfg_path = cfg_path

        # 2. 生成ini配置文件
        ini_path = self.write_test_cases_to_ini(cfg_path, test_cases, variables)

        return {
            'cfg_path': cfg_path,
            'ini_path': ini_path
        }

    def read_canoe_variables_from_ini(self, ini_path: str) -> Dict[str, str]:
        """
        从ini文件读取CANoe变量配置

        Args:
            ini_path: ini文件路径

        Returns:
            变量字典
        """
        if not os.path.exists(ini_path):
            logger.warning(f"ini文件不存在: {ini_path}")
            return {}

        config = configparser.ConfigParser()
        config.read(ini_path, encoding='utf-8')

        variables = {}
        if 'Variables' in config:
            variables = dict(config['Variables'])

        return variables

    def get_test_cases_from_ini(self, ini_path: str) -> List[Dict[str, Any]]:
        """
        从ini文件读取测试用例列表

        Args:
            ini_path: ini文件路径

        Returns:
            测试用例列表
        """
        if not os.path.exists(ini_path):
            logger.warning(f"ini文件不存在: {ini_path}")
            return []

        config = configparser.ConfigParser()
        config.read(ini_path, encoding='utf-8')

        test_cases = []

        # 获取用例数量
        count = 0
        if 'TestCases' in config:
            count = int(config['TestCases'].get('count', 0))

        # 读取每个用例
        for i in range(1, count + 1):
            section_name = f'TestCase_{i}'
            if section_name in config:
                tc = dict(config[section_name])
                # 转换类型
                tc['repeat'] = int(tc.get('repeat', 1))
                # 解析params字符串
                params_str = tc.get('params', '{}')
                try:
                    tc['params'] = eval(params_str) if params_str else {}
                except:
                    tc['params'] = {}
                test_cases.append(tc)

        return test_cases
