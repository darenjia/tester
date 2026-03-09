"""
软件调用验证模块

使用执行器实际调用的适配器方法进行检测，相当于代码执行前的预验证
"""
import os
import sys
import time
from typing import Dict, Any, Optional
from dataclasses import dataclass, field

# 添加父目录到路径
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from config.settings import get_config


@dataclass
class SoftwareValidationResult:
    """软件验证结果"""
    installed: bool = False
    path: str = ""
    adapter_test: Dict[str, Any] = field(default_factory=dict)
    config_check: Dict[str, Any] = field(default_factory=dict)
    error: Optional[str] = None
    details: Dict[str, Any] = field(default_factory=dict)


class CANoeValidator:
    """CANoe调用验证器 - 复用CANoeAdapter逻辑"""
    
    def __init__(self):
        self.config = get_config()
        self.canoe_path = self.config.get('software.canoe_path', r'C:\Program Files\Vector\CANoe 17\Exec64\CANoe64.exe')
        self.config_path = self.config.get('software.workspace_path', r'C:\TestWorkspace')
    
    def validate(self) -> SoftwareValidationResult:
        """
        验证CANoe调用环境
        
        Returns:
            SoftwareValidationResult: 验证结果
        """
        result = SoftwareValidationResult()
        result.path = self.canoe_path
        
        # 1. 检查可执行文件是否存在
        if not os.path.exists(self.canoe_path):
            result.error = f"CANoe可执行文件不存在: {self.canoe_path}"
            return result
        
        result.installed = True
        result.adapter_test = {
            "pywin32_available": False,
            "com_object_created": False,
            "interface_accessible": False,
            "version": None
        }
        
        # 2. 检查pywin32
        try:
            import win32com.client
            result.adapter_test["pywin32_available"] = True
        except ImportError as e:
            result.error = f"pywin32未安装: {str(e)}"
            return result
        
        # 3. 尝试创建COM对象（不启动界面）
        app = None
        try:
            app = win32com.client.Dispatch("CANoe.Application")
            result.adapter_test["com_object_created"] = True
        except Exception as e:
            result.error = f"无法创建CANoe COM对象: {str(e)}"
            return result
        
        # 4. 验证接口可访问性
        try:
            measurement = app.Measurement
            bus_systems = app.BusSystems
            system_variables = app.SystemVariables
            result.adapter_test["interface_accessible"] = True
        except Exception as e:
            result.error = f"无法访问CANoe接口: {str(e)}"
            # 释放COM对象
            try:
                app.Quit()
            except:
                pass
            return result
        
        # 5. 获取版本信息
        try:
            version = app.Version
            result.adapter_test["version"] = str(version)
            result.details["product_version"] = str(version)
        except:
            pass
        
        # 6. 检查测量状态
        try:
            measurement_running = app.Measurement.Running
            result.details["measurement_running"] = bool(measurement_running)
        except:
            pass
        
        # 7. 释放COM对象
        try:
            app.Quit()
        except:
            pass
        
        # 8. 检查配置文件路径
        result.config_check = self._validate_config_path()
        
        return result
    
    def _validate_config_path(self) -> Dict[str, Any]:
        """验证配置文件路径"""
        config_result = {
            "config_path": self.config_path,
            "path_exists": False,
            "sample_load_test": "not_tested",
            "valid_cfg_files": 0,
            "cfg_files": []
        }
        
        if not os.path.exists(self.config_path):
            config_result["error"] = f"配置路径不存在: {self.config_path}"
            return config_result
        
        config_result["path_exists"] = True
        
        # 查找所有cfg文件
        cfg_files = []
        for root, dirs, files in os.walk(self.config_path):
            for file in files:
                if file.endswith('.cfg'):
                    cfg_files.append(os.path.join(root, file))
        
        config_result["cfg_files"] = [os.path.basename(f) for f in cfg_files]
        config_result["valid_cfg_files"] = len(cfg_files)
        
        # 验证第一个cfg文件的格式
        if cfg_files:
            try:
                with open(cfg_files[0], 'r', encoding='utf-8', errors='ignore') as f:
                    content = f.read(1024)  # 读取前1KB
                    if '[Configuration]' in content or 'Database' in content or 'BusManager' in content:
                        config_result["sample_load_test"] = "success"
                    else:
                        config_result["sample_load_test"] = "invalid_format"
            except Exception as e:
                config_result["sample_load_test"] = f"read_error: {str(e)}"
        
        return config_result


class TSMasterValidator:
    """TSMaster调用验证器 - 复用TSMasterAdapter逻辑"""
    
    def __init__(self):
        self.config = get_config()
        self.tsmaster_path = self.config.get('software.tsmaster_path', r'C:\Program Files\TSMaster')
        self.config_path = self.config.get('software.workspace_path', r'C:\TestWorkspace')
    
    def validate(self) -> SoftwareValidationResult:
        """
        验证TSMaster调用环境
        
        Returns:
            SoftwareValidationResult: 验证结果
        """
        result = SoftwareValidationResult()
        result.path = self.tsmaster_path
        
        # 1. 检查安装目录是否存在
        if not os.path.exists(self.tsmaster_path):
            result.error = f"TSMaster安装目录不存在: {self.tsmaster_path}"
            return result
        
        result.installed = True
        result.adapter_test = {
            "python_api_available": False,
            "rpc_api_available": False,
            "connection_test": "failed",
            "version": None
        }
        
        # 2. 检查Python API
        try:
            from TSMaster import TSMaster
            result.adapter_test["python_api_available"] = True
        except ImportError:
            pass
        
        # 3. 检查RPC API
        try:
            import TSMasterAPI
            result.adapter_test["rpc_api_available"] = True
        except ImportError:
            pass
        
        if not result.adapter_test["python_api_available"] and not result.adapter_test["rpc_api_available"]:
            result.error = "TSMaster API未安装（Python API和RPC API都不可用）"
            return result
        
        # 4. 尝试连接测试（使用Python API）
        if result.adapter_test["python_api_available"]:
            try:
                from TSMaster import TSMaster
                ts = TSMaster()
                ts.connect()
                result.adapter_test["connection_test"] = "success"
                
                # 获取版本信息
                try:
                    version = ts.get_version()
                    result.adapter_test["version"] = str(version)
                    result.details["version"] = str(version)
                except:
                    pass
                
                # 断开连接
                try:
                    ts.disconnect()
                except:
                    pass
                    
            except Exception as e:
                result.adapter_test["connection_test"] = f"failed: {str(e)}"
                result.details["connection_error"] = str(e)
        
        # 5. 检查配置文件路径
        result.config_check = self._validate_config_path()
        
        return result
    
    def _validate_config_path(self) -> Dict[str, Any]:
        """验证配置文件路径"""
        config_result = {
            "config_path": self.config_path,
            "path_exists": False,
            "sample_load_test": "not_tested",
            "valid_tproj_files": 0,
            "tproj_files": []
        }
        
        if not os.path.exists(self.config_path):
            config_result["error"] = f"配置路径不存在: {self.config_path}"
            return config_result
        
        config_result["path_exists"] = True
        
        # 查找所有tproj文件
        tproj_files = []
        for root, dirs, files in os.walk(self.config_path):
            for file in files:
                if file.endswith('.tproj'):
                    tproj_files.append(os.path.join(root, file))
        
        config_result["tproj_files"] = [os.path.basename(f) for f in tproj_files]
        config_result["valid_tproj_files"] = len(tproj_files)
        
        # 验证第一个tproj文件的格式
        if tproj_files:
            try:
                import xml.etree.ElementTree as ET
                tree = ET.parse(tproj_files[0])
                root = tree.getroot()
                if root.tag == 'Project' or 'TSMaster' in str(root.tag):
                    config_result["sample_load_test"] = "success"
                else:
                    config_result["sample_load_test"] = "invalid_format"
            except Exception as e:
                config_result["sample_load_test"] = f"read_error: {str(e)}"
        
        return config_result


class TTmanValidator:
    """TTman调用验证器"""
    
    def __init__(self):
        self.config = get_config()
        self.ttman_path = self.config.get('software.ttman_path', r'C:\Spirent\TTman.bat')
    
    def validate(self) -> SoftwareValidationResult:
        """
        验证TTman调用环境
        
        Returns:
            SoftwareValidationResult: 验证结果
        """
        result = SoftwareValidationResult()
        result.path = self.ttman_path
        
        # 1. 检查批处理文件是否存在
        if not os.path.exists(self.ttman_path):
            result.error = f"TTman批处理文件不存在: {self.ttman_path}"
            return result
        
        result.installed = True
        result.adapter_test = {
            "file_executable": False,
            "script_readable": False
        }
        
        # 2. 检查文件是否可执行
        try:
            import stat
            st = os.stat(self.ttman_path)
            # Windows批处理文件不需要执行权限，但检查是否可读
            result.adapter_test["file_executable"] = True
        except Exception as e:
            result.error = f"无法访问TTman文件: {str(e)}"
            return result
        
        # 3. 检查脚本内容
        try:
            with open(self.ttman_path, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read()
                result.adapter_test["script_readable"] = True
                
                # 检查是否包含关键命令
                if 'java' in content.lower() or 'ttman' in content.lower():
                    result.adapter_test["valid_script"] = True
                else:
                    result.adapter_test["valid_script"] = False
                    
                result.details["script_size"] = len(content)
        except Exception as e:
            result.adapter_test["script_readable"] = False
            result.error = f"无法读取TTman脚本: {str(e)}"
        
        # 4. 检查安装目录结构
        install_dir = os.path.dirname(self.ttman_path)
        result.config_check = {
            "install_dir": install_dir,
            "dir_exists": os.path.exists(install_dir),
            "jar_files": [],
            "config_files": []
        }
        
        if os.path.exists(install_dir):
            # 查找jar文件
            for file in os.listdir(install_dir):
                if file.endswith('.jar'):
                    result.config_check["jar_files"].append(file)
                if file.endswith('.properties') or file.endswith('.xml') or file.endswith('.conf'):
                    result.config_check["config_files"].append(file)
        
        return result


class SoftwareValidator:
    """软件验证管理器"""
    
    def __init__(self):
        self.canoe_validator = CANoeValidator()
        self.tsmaster_validator = TSMasterValidator()
        self.ttman_validator = TTmanValidator()
    
    def validate_all(self, progress_callback=None) -> Dict[str, Any]:
        """
        验证所有软件
        
        Args:
            progress_callback: 进度回调函数，接收(当前步骤, 总步骤, 描述)
        
        Returns:
            Dict[str, Any]: 所有软件的验证结果
        """
        results = {
            "canoe": {},
            "tsmaster": {},
            "ttman": {}
        }
        
        steps = [
            ("canoe", "正在验证 CANoe..."),
            ("tsmaster", "正在验证 TSMaster..."),
            ("ttman", "正在验证 TTman...")
        ]
        
        for i, (software, description) in enumerate(steps):
            if progress_callback:
                progress_callback(i + 1, len(steps), description)
            
            if software == "canoe":
                result = self.canoe_validator.validate()
            elif software == "tsmaster":
                result = self.tsmaster_validator.validate()
            else:
                result = self.ttman_validator.validate()
            
            results[software] = {
                "installed": result.installed,
                "path": result.path,
                "adapter_test": result.adapter_test,
                "config_check": result.config_check,
                "error": result.error,
                "details": result.details
            }
        
        return results


# 全局验证器实例
_validator_instance: Optional[SoftwareValidator] = None


def get_software_validator() -> SoftwareValidator:
    """获取全局软件验证器实例"""
    global _validator_instance
    if _validator_instance is None:
        _validator_instance = SoftwareValidator()
    return _validator_instance
