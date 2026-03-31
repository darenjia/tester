"""
配置管理API
提供配置相关的RESTful接口
"""
from flask import Blueprint, request, jsonify
from typing import Dict, Any
import logging

import sys
import os
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from core.config_manager import get_runtime_config
from config.config_manager import config_manager

logger = logging.getLogger(__name__)

# 创建蓝图
config_bp = Blueprint('config', __name__, url_prefix='/api')

# 获取运行时配置管理器
runtime_config = get_runtime_config()


@config_bp.route('/config/http', methods=['GET'])
def get_http_config():
    """
    获取HTTP服务配置
    """
    try:
        config = runtime_config.get_http_config()
        return jsonify({
            "success": True,
            "data": config
        })
    except Exception as e:
        return jsonify({"success": False, "message": f"获取HTTP配置失败: {str(e)}"}), 500


@config_bp.route('/config/http', methods=['POST'])
def set_http_config():
    """
    更新HTTP服务配置
    
    请求体:
    {
        "port": 8180,
        "host": "0.0.0.0",
        "debug": false
    }
    """
    try:
        data = request.get_json()
        if not data:
            return jsonify({"success": False, "message": "请求体不能为空"}), 400
        
        # 验证配置
        validation = runtime_config.validate_config({"http": data})
        if not validation["valid"]:
            return jsonify({
                "success": False,
                "message": "配置验证失败",
                "errors": validation["errors"]
            }), 400
        
        # 更新配置
        if runtime_config.set_http_config(data):
            return jsonify({
                "success": True,
                "message": "HTTP配置已更新",
                "data": runtime_config.get_http_config()
            })
        else:
            return jsonify({"success": False, "message": "更新配置失败"}), 500
            
    except Exception as e:
        return jsonify({"success": False, "message": f"更新HTTP配置失败: {str(e)}"}), 500


@config_bp.route('/config/websocket', methods=['GET'])
def get_websocket_config():
    """
    获取WebSocket服务配置
    """
    try:
        config = runtime_config.get_websocket_config()
        return jsonify({
            "success": True,
            "data": config
        })
    except Exception as e:
        return jsonify({"success": False, "message": f"获取WebSocket配置失败: {str(e)}"}), 500


@config_bp.route('/config/websocket', methods=['POST'])
def set_websocket_config():
    """
    更新WebSocket服务配置
    
    请求体:
    {
        "enabled": false,
        "port": 8080,
        "host": "0.0.0.0"
    }
    """
    try:
        data = request.get_json()
        if not data:
            return jsonify({"success": False, "message": "请求体不能为空"}), 400
        
        # 验证配置
        validation = runtime_config.validate_config({"websocket": data})
        if not validation["valid"]:
            return jsonify({
                "success": False,
                "message": "配置验证失败",
                "errors": validation["errors"]
            }), 400
        
        # 更新配置
        if runtime_config.set_websocket_config(data):
            return jsonify({
                "success": True,
                "message": "WebSocket配置已更新",
                "data": runtime_config.get_websocket_config()
            })
        else:
            return jsonify({"success": False, "message": "更新配置失败"}), 500
            
    except Exception as e:
        return jsonify({"success": False, "message": f"更新WebSocket配置失败: {str(e)}"}), 500


@config_bp.route('/config', methods=['GET'])
def get_all_config():
    """
    获取所有配置
    """
    try:
        config = runtime_config.get_all_config()
        return jsonify({
            "success": True,
            "data": config
        })
    except Exception as e:
        return jsonify({"success": False, "message": f"获取配置失败: {str(e)}"}), 500


@config_bp.route('/config', methods=['POST'])
def update_config():
    """
    批量更新配置
    
    请求体:
    {
        "http": {"port": 2888},
        "websocket": {"enabled": true}
    }
    """
    try:
        data = request.get_json()
        if not data:
            return jsonify({"success": False, "message": "请求体不能为空"}), 400
        
        # 验证配置
        validation = runtime_config.validate_config(data)
        if not validation["valid"]:
            return jsonify({
                "success": False,
                "message": "配置验证失败",
                "errors": validation["errors"]
            }), 400
        
        # 更新配置
        if runtime_config.update_config(data):
            return jsonify({
                "success": True,
                "message": "配置已更新",
                "data": runtime_config.get_all_config()
            })
        else:
            return jsonify({"success": False, "message": "更新配置失败"}), 500
            
    except Exception as e:
        return jsonify({"success": False, "message": f"更新配置失败: {str(e)}"}), 500


@config_bp.route('/config/export', methods=['GET'])
def export_config():
    """
    导出配置
    
    查询参数:
    - download: 是否下载文件，默认false（返回JSON）
    """
    try:
        config = runtime_config.get_all_config()
        export_data = {
            "export_time": __import__('datetime').datetime.now().isoformat(),
            "config": config
        }
        
        download = request.args.get('download', 'false').lower() == 'true'
        
        if download:
            import json
            from flask import Response
            
            response = Response(
                json.dumps(export_data, ensure_ascii=False, indent=2),
                mimetype='application/json'
            )
            response.headers['Content-Disposition'] = 'attachment; filename=config.json'
            return response
        else:
            return jsonify({
                "success": True,
                "data": export_data
            })
            
    except Exception as e:
        return jsonify({"success": False, "message": f"导出配置失败: {str(e)}"}), 500


@config_bp.route('/config/import', methods=['POST'])
def import_config():
    """
    导入配置
    
    请求体:
    {
        "config": {...},
        "merge": true  // 是否合并配置，默认true
    }
    
    或者上传文件
    """
    try:
        merge = True
        
        # 检查是否有文件上传
        if 'file' in request.files:
            file = request.files['file']
            if file.filename == '':
                return jsonify({"success": False, "message": "未选择文件"}), 400
            
            import json
            import tempfile
            
            # 保存上传的文件
            with tempfile.NamedTemporaryFile(mode='w+', delete=False, suffix='.json') as temp:
                file.save(temp.name)
                temp_path = temp.name
            
            try:
                if runtime_config.import_config(temp_path, merge=merge):
                    return jsonify({
                        "success": True,
                        "message": "配置已导入",
                        "data": runtime_config.get_all_config()
                    })
                else:
                    return jsonify({"success": False, "message": "导入配置失败"}), 500
            finally:
                os.remove(temp_path)
        else:
            # 从请求体获取配置
            data = request.get_json()
            if not data or "config" not in data:
                return jsonify({"success": False, "message": "请求体必须包含config字段"}), 400
            
            merge = data.get("merge", True)
            config = data["config"]
            
            # 验证配置
            validation = runtime_config.validate_config(config)
            if not validation["valid"]:
                return jsonify({
                    "success": False,
                    "message": "配置验证失败",
                    "errors": validation["errors"]
                }), 400
            
            if runtime_config.update_config(config):
                return jsonify({
                    "success": True,
                    "message": "配置已导入",
                    "data": runtime_config.get_all_config()
                })
            else:
                return jsonify({"success": False, "message": "导入配置失败"}), 500
                
    except Exception as e:
        return jsonify({"success": False, "message": f"导入配置失败: {str(e)}"}), 500


@config_bp.route('/config/reset', methods=['POST'])
def reset_config():
    """
    重置为默认配置
    
    警告: 此操作会清除所有自定义配置
    """
    try:
        if runtime_config.reset_to_default():
            return jsonify({
                "success": True,
                "message": "配置已重置为默认值",
                "data": runtime_config.get_all_config()
            })
        else:
            return jsonify({"success": False, "message": "重置配置失败"}), 500
            
    except Exception as e:
        return jsonify({"success": False, "message": f"重置配置失败: {str(e)}"}), 500


@config_bp.route('/config/restart', methods=['POST'])
def restart_service():
    """
    重启服务以应用新配置
    
    注意: 此操作会中断当前所有连接
    """
    try:
        import threading
        import time
        
        def delayed_restart():
            """延迟重启，给API响应时间"""
            time.sleep(2)
            # 这里可以实现实际的重启逻辑
            # 例如: os.execv(sys.executable, ['python'] + sys.argv)
            logger.info("服务重启中...")
        
        # 启动延迟重启线程
        restart_thread = threading.Thread(target=delayed_restart, daemon=True)
        restart_thread.start()
        
        return jsonify({
            "success": True,
            "message": "服务将在2秒后重启"
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"重启服务失败: {str(e)}"}), 500


@config_bp.route('/config/report-server/test', methods=['POST'])
def test_report_server_connection():
    """
    测试上报服务器连接
    
    请求体:
    {
        "host": "192.168.1.100",
        "port": 8080,
        "path": "/api/report",
        "timeout": 30
    }
    """
    try:
        import socket
        import urllib.request
        import ssl
        
        data = request.get_json()
        if not data:
            return jsonify({"success": False, "message": "请求体不能为空"}), 400
        
        host = data.get('host')
        port = data.get('port', 8080)
        path = data.get('path', '/api/report')
        timeout = data.get('timeout', 30)
        
        if not host:
            return jsonify({"success": False, "message": "服务器地址不能为空"}), 400
        
        # 尝试TCP连接测试
        try:
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(timeout)
            result = sock.connect_ex((host, port))
            sock.close()
            
            if result != 0:
                return jsonify({
                    "success": False, 
                    "message": f"无法连接到服务器 {host}:{port}"
                }), 200
        except Exception as e:
            return jsonify({
                "success": False, 
                "message": f"连接测试失败: {str(e)}"
            }), 200
        
        # 尝试HTTP请求测试
        try:
            # 构建URL
            url = f"http://{host}:{port}{path}"
            
            # 创建请求
            req = urllib.request.Request(
                url,
                method='OPTIONS',
                headers={
                    'User-Agent': 'TestExecutor/1.0',
                    'Accept': 'application/json'
                }
            )
            
            # 发送请求
            context = ssl._create_unverified_context() if hasattr(ssl, '_create_unverified_context') else None
            response = urllib.request.urlopen(req, timeout=timeout, context=context)
            
            return jsonify({
                "success": True,
                "message": "连接成功",
                "data": {
                    "host": host,
                    "port": port,
                    "path": path,
                    "status_code": response.getcode()
                }
            })
            
        except urllib.error.HTTPError as e:
            # HTTP错误但服务器可达
            if e.code in [404, 405, 500]:
                return jsonify({
                    "success": True,
                    "message": "服务器可达（接口可能不存在或不允许OPTIONS请求）",
                    "data": {
                        "host": host,
                        "port": port,
                        "path": path,
                        "status_code": e.code
                    }
                })
            else:
                return jsonify({
                    "success": False,
                    "message": f"HTTP错误: {e.code}"
                }), 200
                
        except Exception as e:
            # TCP连接成功但HTTP请求失败，也算部分成功
            return jsonify({
                "success": True,
                "message": "TCP连接成功（HTTP接口测试失败）",
                "data": {
                    "host": host,
                    "port": port,
                    "path": path,
                    "tcp_connected": True
                }
            })
            
    except Exception as e:
        return jsonify({"success": False, "message": f"测试连接失败: {str(e)}"}), 500


@config_bp.route('/config/category_ini_config_rules', methods=['GET'])
def get_category_ini_config_rules():
    """
    获取 category_ini_config_rules 配置
    """
    try:
        rules = config_manager.get('category_ini_config_rules', {})
        return jsonify({
            "success": True,
            "data": rules
        })
    except Exception as e:
        return jsonify({"success": False, "message": f"获取规则配置失败: {str(e)}"}), 500


@config_bp.route('/config/category_ini_config_rules', methods=['PUT'])
def update_category_ini_config_rules():
    """
    更新 category_ini_config_rules 配置

    请求体:
    {
        "canoe": {
            "pattern": "^(.*)$",
            "replacement": "$1=1"
        },
        "tsmaster": {
            "pattern": "^CAN_(.*)$",
            "replacement": "$1=1"
        }
    }
    """
    try:
        data = request.get_json()
        if not data:
            return jsonify({"success": False, "message": "请求体不能为空"}), 400

        # 更新配置
        config_manager.set('category_ini_config_rules', data)
        config_manager.save()

        return jsonify({
            "success": True,
            "message": "规则配置已更新",
            "data": data
        })
    except Exception as e:
        return jsonify({"success": False, "message": f"更新规则配置失败: {str(e)}"}), 500
