"""
PyInstaller打包脚本
将Python执行器打包为独立的exe可执行文件

使用方法:
    python build_exe.py

打包后的文件位于 dist/PythonExecutor 目录
"""

import PyInstaller.__main__
import os
import sys
import shutil

def clean_build():
    """清理之前的构建文件"""
    dirs_to_remove = ['build', 'dist', '__pycache__']
    for dir_name in dirs_to_remove:
        if os.path.exists(dir_name):
            print(f"清理 {dir_name}...")
            shutil.rmtree(dir_name)
    
    # 清理 .spec 文件
    for file in os.listdir('.'):
        if file.endswith('.spec'):
            print(f"删除 {file}...")
            os.remove(file)

def get_hidden_imports():
    """获取需要显式导入的隐藏模块"""
    return [
        # Flask相关
        'flask',
        'flask_socketio',
        'flask_cors',
        'werkzeug',
        'jinja2',
        'markupsafe',
        'itsdangerous',
        'click',
        # SocketIO相关
        'socketio',
        'engineio',
        'gevent',
        'geventwebsocket',
        # 项目模块
        'core.task_executor',
        'core.task_store',
        'core.result_collector',
        'core.adapters',
        'core.adapters.canoe',
        'core.adapters.canoe.adapter',
        'core.adapters.tsmaster',
        'core.adapters.tsmaster.rpc_client',
        'api.routes',
        'api.task_executor',
        'models.task',
        'models.result',
        'utils.logger',
        'utils.exceptions',
        'utils.validators',
        'utils.retry',
        'utils.metrics',
        'config.settings',
        'ws_server.client',
        # 其他依赖
        'json',
        'datetime',
        'typing',
        'signal',
        'time',
        'logging',
        'pathlib',
        'dataclasses',
        'enum',
        'threading',
        'queue',
    ]

def get_data_files():
    """获取需要包含的数据文件"""
    datas = []
    
    # 配置文件
    if os.path.exists('config/executor_config.json'):
        datas.append(('config/executor_config.json', 'config'))
    
    # 模板文件
    if os.path.exists('deploy/config'):
        datas.append(('deploy/config', 'deploy/config'))
    
    return datas

def build_exe():
    """执行打包"""
    print("=" * 60)
    print("开始打包 Python执行器")
    print("=" * 60)
    
    # 清理旧文件
    clean_build()
    
    # 构建参数
    args = [
        'main.py',  # 入口文件
        '--name=PythonExecutor',  # 输出名称
        '--onefile',  # 打包为单个文件
        '--windowed',  # Windows下不显示控制台窗口（如需调试可改为 --console）
        # '--console',  # 如需调试，注释掉 --windowed，取消注释此行
        '--clean',  # 清理临时文件
        '--noconfirm',  # 不确认覆盖
    ]
    
    # 添加隐藏导入
    for hidden_import in get_hidden_imports():
        args.append(f'--hidden-import={hidden_import}')
    
    # 添加数据文件
    for src, dst in get_data_files():
        args.append(f'--add-data={src}{os.pathsep}{dst}')
    
    # 添加图标（如果有）
    if os.path.exists('icon.ico'):
        args.append('--icon=icon.ico')
    
    print(f"打包参数: {' '.join(args)}")
    print("-" * 60)
    
    # 执行打包
    PyInstaller.__main__.run(args)
    
    print("-" * 60)
    print("打包完成!")
    print(f"可执行文件位于: {os.path.abspath('dist/PythonExecutor.exe')}")
    print("=" * 60)

if __name__ == '__main__':
    build_exe()
