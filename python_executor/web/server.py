"""
Flask Web 应用
"""
import os
import sys
from flask import Flask, jsonify, request, render_template
from datetime import datetime

# 添加父目录到路径
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from config.settings import get_config
from utils.logger import get_logger, logger_manager
from core.status_monitor import get_status_monitor
from api.task_api import task_bp
from api.task_log_api import task_log_bp
from api.service_api import service_bp
from api.config_api import config_bp
from api.docs_api import docs_bp
from api.env_api import env_bp


def create_app() -> Flask:
    """创建 Flask 应用"""
    # 确定模板和静态文件路径
    if getattr(sys, 'frozen', False):
        # 打包后的exe运行 - PyInstaller 使用 _MEIPASS 作为临时目录
        if hasattr(sys, '_MEIPASS'):
            base_dir = sys._MEIPASS
        else:
            base_dir = os.path.dirname(os.sys.executable)
        template_dir = os.path.join(base_dir, 'web', 'templates')
        static_dir = os.path.join(base_dir, 'web', 'static')
    else:
        # 开发环境运行
        base_dir = os.path.dirname(os.path.abspath(__file__))
        template_dir = os.path.join(base_dir, 'templates')
        static_dir = os.path.join(base_dir, 'static')
    
    app = Flask(__name__, 
                template_folder=template_dir,
                static_folder=static_dir)
    
    # 禁用缓存（开发时）
    app.config['SEND_FILE_MAX_AGE_DEFAULT'] = 0
    
    # 初始化日志（仅在未初始化时）
    config = get_config()
    log_dir = config.get('logging.log_dir', 'logs')
    log_level = config.get('logging.level', 'INFO')
    
    # 检查是否已经有处理器，避免重复配置
    if not logger_manager.logger.handlers:
        logger_manager.setup(log_dir, log_level)
    
    logger = get_logger()
    logger.info("Flask Web 应用启动")
    
    # 注册蓝图
    app.register_blueprint(task_bp)
    app.register_blueprint(task_log_bp)
    app.register_blueprint(service_bp)
    app.register_blueprint(config_bp)
    app.register_blueprint(docs_bp)
    app.register_blueprint(env_bp)
    
    # 注册路由
    register_routes(app)
    
    return app


def register_routes(app: Flask):
    """注册路由"""
    
    # ========== 页面路由 ==========
    
    @app.route('/')
    def index():
        """首页 - 仪表盘"""
        return render_template('dashboard.html')
    
    @app.route('/dashboard')
    def dashboard():
        """仪表盘页面"""
        return render_template('dashboard.html')
    
    @app.route('/tasks')
    def tasks_page():
        """任务管理页面"""
        return render_template('tasks.html')
    
    @app.route('/config')
    def config_page():
        """配置页面"""
        return render_template('config.html')
    
    @app.route('/logs')
    def logs_page():
        """日志页面"""
        return render_template('logs.html')
    
    @app.route('/service-config')
    def service_config_page():
        """服务配置页面"""
        return render_template('service_config.html')
    
    @app.route('/api-docs')
    def api_docs_page():
        """接口文档页面"""
        return render_template('api_docs.html')
    
    @app.route('/env-check')
    def env_check_page():
        """环境检测页面"""
        return render_template('env_check.html')
    
    @app.route('/report-config')
    def report_config_page():
        """上报配置页面"""
        return render_template('report_config.html')
    
    # ========== API 路由 ==========
    
    @app.route('/api/status')
    def api_status():
        """获取系统状态"""
        monitor = get_status_monitor()
        return jsonify({
            'success': True,
            'data': monitor.get_all_status()
        })
    
    @app.route('/api/config', methods=['GET'])
    def api_config_get():
        """获取配置"""
        config = get_config()
        return jsonify({
            'success': True,
            'data': config.get_all()
        })
    
    @app.route('/api/config', methods=['POST'])
    def api_config_post():
        """保存配置"""
        try:
            config = get_config()
            new_config = request.get_json()
            
            if not new_config:
                return jsonify({
                    'success': False,
                    'message': '无效的配置数据'
                }), 400
            
            if config.update(new_config):
                get_logger().info("配置已更新")
                return jsonify({
                    'success': True,
                    'message': '配置保存成功'
                })
            else:
                return jsonify({
                    'success': False,
                    'message': '配置保存失败'
                }), 500
        except Exception as e:
            get_logger().error(f"保存配置失败: {e}")
            return jsonify({
                'success': False,
                'message': f'保存配置失败: {str(e)}'
            }), 500
    
    @app.route('/api/config/reset', methods=['POST'])
    def api_config_reset():
        """重置配置为默认值"""
        try:
            config = get_config()
            if config.reset_to_default():
                get_logger().info("配置已重置为默认值")
                return jsonify({
                    'success': True,
                    'message': '配置已重置为默认值',
                    'data': config.get_all()
                })
            else:
                return jsonify({
                    'success': False,
                    'message': '配置重置失败'
                }), 500
        except Exception as e:
            get_logger().error(f"重置配置失败: {e}")
            return jsonify({
                'success': False,
                'message': f'重置配置失败: {str(e)}'
            }), 500
    
    @app.route('/api/logs')
    def api_logs():
        """获取日志"""
        try:
            level = request.args.get('level')
            limit = request.args.get('limit', type=int)
            search = request.args.get('search')
            
            logs = logger_manager.get_memory_logs(level=level, limit=limit, search=search)
            return jsonify({
                'success': True,
                'data': logs,
                'total': len(logs)
            })
        except Exception as e:
            return jsonify({
                'success': False,
                'message': f'获取日志失败: {str(e)}'
            }), 500
    
    @app.route('/api/logs/clear', methods=['POST'])
    def api_logs_clear():
        """清空日志"""
        try:
            logger_manager.clear_memory_logs()
            get_logger().info("内存日志已清空")
            return jsonify({
                'success': True,
                'message': '日志已清空'
            })
        except Exception as e:
            return jsonify({
                'success': False,
                'message': f'清空日志失败: {str(e)}'
            }), 500
    
    @app.route('/api/logs/export', methods=['POST'])
    def api_logs_export():
        """导出日志"""
        try:
            data = request.get_json() or {}
            level = data.get('level')
            
            # 生成导出文件名
            timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
            filename = f'logs_export_{timestamp}.txt'
            
            # 确定导出路径
            if getattr(sys, 'frozen', False):
                base_dir = os.path.dirname(os.sys.executable)
            else:
                base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
            
            export_path = os.path.join(base_dir, filename)
            
            if logger_manager.export_logs(export_path, level=level):
                get_logger().info(f"日志已导出到: {export_path}")
                return jsonify({
                    'success': True,
                    'message': '日志导出成功',
                    'data': {'path': export_path, 'filename': filename}
                })
            else:
                return jsonify({
                    'success': False,
                    'message': '日志导出失败'
                }), 500
        except Exception as e:
            return jsonify({
                'success': False,
                'message': f'导出日志失败: {str(e)}'
            }), 500
    
    @app.route('/api/system/info')
    def api_system_info():
        """获取系统信息"""
        try:
            import platform
            config = get_config()
            
            return jsonify({
                'success': True,
                'data': {
                    'platform': platform.platform(),
                    'python_version': platform.python_version(),
                    'device_id': config.get('device.device_id'),
                    'device_name': config.get('device.device_name'),
                    'version': '1.0.0'
                }
            })
        except Exception as e:
            return jsonify({
                'success': False,
                'message': f'获取系统信息失败: {str(e)}'
            }), 500


# 创建应用实例
app = create_app()

if __name__ == '__main__':
    app.run(host='127.0.0.1', port=5000, debug=True)
