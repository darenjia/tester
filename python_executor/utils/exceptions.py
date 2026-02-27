"""
自定义异常类定义
"""

class ExecutorException(Exception):
    """执行器基础异常"""
    pass

class ConnectionException(ExecutorException):
    """连接异常"""
    pass

class TaskException(ExecutorException):
    """任务执行异常"""
    pass

class ToolException(ExecutorException):
    """测试工具异常"""
    pass

class CommunicationException(ExecutorException):
    """通信异常"""
    pass

class ConfigurationException(ExecutorException):
    """配置异常"""
    pass