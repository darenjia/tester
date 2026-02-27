"""
TSMaster适配器模块

提供TSMaster测试工具的适配器实现，支持RPC模式和传统模式
"""

from .rpc_client import TSMasterRPCClient

__all__ = [
    'TSMasterRPCClient',
]
