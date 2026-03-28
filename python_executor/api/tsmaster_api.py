"""
TSMasterAPI 管理 API

提供 TSMasterAPI 安装状态的检查、安装、验证接口
"""
from flask import Blueprint, request, jsonify
import logging

import sys
import os
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from core.tsmaster_api_installer import get_tsmaster_api_installer
from config.settings import get_config

logger = logging.getLogger(__name__)

# 创建蓝图
tsmaster_bp = Blueprint('tsmaster', __name__, url_prefix='/api/tsmaster')


@tsmaster_bp.route('/check-status', methods=['POST'])
def check_api_status():
    """
    检查 TSMasterAPI 安装状态

    请求体（可选）:
    {
        "tsmaster_path": "C:\\Program Files\\TSMaster"  // 可选，覆盖配置中的路径
    }

    响应:
    {
        "success": true,
        "data": {
            "installed": false,
            "version": null,
            "module_path": null,
            "tsmaster_path": "C:\\Program Files\\TSMaster",
            "tsmaster_path_valid": true,
            "local_whl_found": true,
            "local_whl_path": "C:\\Program Files\\TSMaster\\bin\\pythonapi\\TSMasterAPI-1.0.0-py3-none-any.whl"
        }
    }
    """
    try:
        data = request.get_json() or {}
        tsmaster_path = data.get('tsmaster_path')

        # 获取安装器
        installer = get_tsmaster_api_installer(tsmaster_path)

        # 检查状态
        status = installer.check_installation_status()

        return jsonify({
            "success": True,
            "data": status
        })

    except Exception as e:
        logger.error(f"检查 TSMasterAPI 状态失败: {e}")
        return jsonify({
            "success": False,
            "message": f"检查状态失败: {str(e)}"
        }), 500


@tsmaster_bp.route('/install', methods=['POST'])
def install_api():
    """
    执行 TSMasterAPI 安装

    安装策略：本地优先，网络备用

    请求体（可选）:
    {
        "tsmaster_path": "C:\\Program Files\\TSMaster",  // 可选，覆盖配置中的路径
        "prefer_local": true,                            // 可选，默认 true
        "force": false                                   // 可选，强制重新安装
    }

    响应:
    {
        "success": true,
        "message": "TSMasterAPI 安装成功",
        "data": {
            "install_method": "local",
            "installed_from": "C:\\Program Files\\TSMaster\\bin\\pythonapi\\TSMasterAPI-1.0.0-py3-none-any.whl",
            "version": "1.0.0"
        }
    }
    """
    try:
        data = request.get_json() or {}
        tsmaster_path = data.get('tsmaster_path')
        prefer_local = data.get('prefer_local', True)
        force = data.get('force', False)

        # 获取安装器
        installer = get_tsmaster_api_installer(tsmaster_path)

        # 如果强制重新安装，先尝试卸载
        if force:
            logger.info("强制重新安装，尝试先卸载...")
            installer.uninstall()

        # 执行安装
        result = installer.install(prefer_local=prefer_local)

        if result.get("success"):
            # 安装成功，验证并获取版本
            verify_result = installer.verify_installation()
            if verify_result.get("success"):
                result["version"] = verify_result.get("version")
                result["module_path"] = verify_result.get("module_path")

            return jsonify({
                "success": True,
                "message": result.get("message", "TSMasterAPI 安装成功"),
                "data": result
            })
        else:
            return jsonify({
                "success": False,
                "message": result.get("message", "安装失败"),
                "data": result
            }), 500

    except Exception as e:
        logger.error(f"安装 TSMasterAPI 失败: {e}")
        return jsonify({
            "success": False,
            "message": f"安装失败: {str(e)}"
        }), 500


@tsmaster_bp.route('/verify', methods=['POST'])
def verify_api():
    """
    验证 TSMasterAPI 安装结果

    响应:
    {
        "success": true,
        "data": {
            "importable": true,
            "version": "1.0.0",
            "module_path": "C:\\Python310\\Lib\\site-packages\\TSMasterAPI\\__init__.py"
        }
    }
    """
    try:
        data = request.get_json() or {}
        tsmaster_path = data.get('tsmaster_path')

        # 获取安装器
        installer = get_tsmaster_api_installer(tsmaster_path)

        # 验证安装
        result = installer.verify_installation()

        return jsonify({
            "success": result.get("success", False),
            "data": result
        })

    except Exception as e:
        logger.error(f"验证 TSMasterAPI 失败: {e}")
        return jsonify({
            "success": False,
            "message": f"验证失败: {str(e)}"
        }), 500


@tsmaster_bp.route('/uninstall', methods=['POST'])
def uninstall_api():
    """
    卸载 TSMasterAPI

    响应:
    {
        "success": true,
        "message": "TSMasterAPI 卸载成功"
    }
    """
    try:
        installer = get_tsmaster_api_installer()
        result = installer.uninstall()

        if result.get("success"):
            return jsonify({
                "success": True,
                "message": result.get("message", "TSMasterAPI 卸载成功")
            })
        else:
            return jsonify({
                "success": False,
                "message": result.get("message", "卸载失败")
            }), 500

    except Exception as e:
        logger.error(f"卸载 TSMasterAPI 失败: {e}")
        return jsonify({
            "success": False,
            "message": f"卸载失败: {str(e)}"
        }), 500


@tsmaster_bp.route('/find-whl', methods=['POST'])
def find_local_whl():
    """
    查找本地 TSMasterAPI whl 文件

    请求体（可选）:
    {
        "tsmaster_path": "C:\\Program Files\\TSMaster"  // 可选
    }

    响应:
    {
        "success": true,
        "data": {
            "found": true,
            "whl_path": "C:\\Program Files\\TSMaster\\bin\\pythonapi\\TSMasterAPI-1.0.0-py3-none-any.whl"
        }
    }
    """
    try:
        data = request.get_json() or {}
        tsmaster_path = data.get('tsmaster_path')

        # 获取安装器
        installer = get_tsmaster_api_installer(tsmaster_path)

        # 查找 whl 文件
        whl_path = installer.find_local_whl()

        return jsonify({
            "success": True,
            "data": {
                "found": whl_path is not None,
                "whl_path": whl_path
            }
        })

    except Exception as e:
        logger.error(f"查找本地 whl 文件失败: {e}")
        return jsonify({
            "success": False,
            "message": f"查找失败: {str(e)}"
        }), 500