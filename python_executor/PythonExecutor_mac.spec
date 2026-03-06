# -*- mode: python ; coding: utf-8 -*-
"""
PyInstaller spec文件 - macOS版本
用于将Python执行器打包为macOS可执行文件

使用方法:
    pyinstaller PythonExecutor_mac.spec

打包后的文件位于:
    - 命令行版本: dist/PythonExecutor
    - App Bundle: dist/PythonExecutor.app (如启用)
"""

import sys
import os

block_cipher = None

# 项目根目录
base_dir = os.path.abspath('.')

# 数据文件
datas = []

# 收集所有Python模块
for root, dirs, files in os.walk(base_dir):
    # 跳过不需要的目录
    dirs[:] = [d for d in dirs if not d.startswith('.') and d not in ['__pycache__', 'tests', 'test', 'deploy', 'docker', 'build', 'dist']]
    
    rel_root = os.path.relpath(root, base_dir)
    if rel_root == '.':
        rel_root = ''
    
    for file in files:
        if file.endswith(('.py', '.json', '.txt', '.md')):
            src = os.path.join(root, file)
            dst = rel_root
            datas.append((src, dst))

# 分析依赖
a = Analysis(
    ['main_entry.py'],  # 使用入口文件
    pathex=[base_dir],
    binaries=[],
    datas=datas,
    hiddenimports=[
        # Flask及其依赖
        'flask',
        'flask_socketio',
        'flask_cors',
        'werkzeug',
        'werkzeug.middleware.proxy_fix',
        'werkzeug.routing',
        'jinja2',
        'jinja2.ext',
        'markupsafe',
        'itsdangerous',
        'click',
        
        # SocketIO和WebSocket
        'socketio',
        'socketio.asyncio_client',
        'socketio.asyncio_server',
        'engineio',
        'engineio.asyncio_client',
        'engineio.asyncio_server',
        'gevent',
        'gevent.monkey',
        'geventwebsocket',
        'geventwebsocket.handler',
        
        # 项目模块 - 显式导入所有核心模块
        'main',
        'app',
        'core',
        'core.task_executor',
        'core.task_store',
        'core.result_collector',
        'core.canoe_controller',
        'core.tsmaster_controller',
        'core.adapters',
        'core.adapters.base_adapter',
        'core.adapters.canoe_adapter',
        'core.adapters.tsmaster_adapter',
        'core.adapters.ttworkbench_adapter',
        'core.adapters.canoe',
        'core.adapters.canoe.adapter',
        'core.adapters.canoe.com_wrapper',
        'core.adapters.canoe.test_engine',
        'core.adapters.tsmaster',
        'core.adapters.tsmaster.rpc_client',
        'api',
        'api.routes',
        'api.task_executor',
        'models',
        'models.task',
        'models.result',
        'utils',
        'utils.logger',
        'utils.exceptions',
        'utils.validators',
        'utils.retry',
        'utils.metrics',
        'config',
        'config.settings',
        'config.config_manager',
        'ws_server',
        'ws_server.client',
        
        # Python标准库常用模块
        'json',
        'datetime',
        'time',
        'signal',
        'sys',
        'os',
        'logging',
        'pathlib',
        'dataclasses',
        'enum',
        'threading',
        'queue',
        'uuid',
        'hashlib',
        'base64',
        'typing',
        'collections',
        'functools',
        'inspect',
        'traceback',
        'asyncio',
        'concurrent.futures',
        'contextlib',
        'copy',
        're',
        'string',
        'warnings',
        'weakref',
        'urllib.parse',
        'urllib.request',
        'http.client',
        'socket',
        'ssl',
        'email',
        'email.mime.text',
        'email.mime.multipart',
        'mimetypes',
        'gzip',
        'zlib',
        'tempfile',
        'shutil',
        'stat',
        'pickle',
        'csv',
        'configparser',
        'argparse',
        'platform',
        'subprocess',
        'multiprocessing',
        'multiprocessing.pool',
        'xml.etree.ElementTree',
    ],
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[
        # 排除不必要的模块以减小体积
        'matplotlib',
        'numpy',
        'pandas',
        'scipy',
        'sklearn',
        'tensorflow',
        'torch',
        'PIL',
        'Pillow',
        'tkinter',
        'PyQt5',
        'PyQt6',
        'PySide2',
        'PySide6',
        'wx',
        'wxPython',
        'gi',
        'cairo',
        'pycairo',
        'pygobject',
        'pytest',
        'unittest',
        'doctest',
        'pdb',
        'pydoc',
        'idlelib',
        'turtle',
        'lib2to3',
        'test',
        'tests',
        '_pytest',
        'mypy',
        'mypyc',
        'black',
        'flake8',
        'pylint',
        'isort',
        'coverage',
        'sphinx',
        'jedi',
        'parso',
        'prompt_toolkit',
        'IPython',
        'jupyter',
        'notebook',
        'ipywidgets',
        'setuptools',
        'wheel',
        'pip',
        'pkg_resources',
        'easy_install',
        'distutils',
        # Mac特定排除
        'pywin32',
        'win32com',
        'winreg',
    ],
    win_no_prefer_redirects=False,
    win_private_assemblies=False,
    cipher=block_cipher,
    noarchive=False,
)

# 去除重复项
pyz = PYZ(a.pure, a.zipped_data, cipher=block_cipher)

# 创建可执行文件 - macOS版本
exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.zipfiles,
    a.datas,
    [],
    name='PythonExecutor',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,  # 使用UPX压缩减小体积
    upx_exclude=[],
    runtime_tmpdir=None,
    console=True,  # 显示终端窗口（便于调试和查看日志）
    disable_windowed_traceback=False,
    argv_emulation=True,  # macOS需要启用参数模拟
    target_arch=None,  # 自动检测架构 (x86_64 或 arm64)
    codesign_identity=None,  # 如需代码签名，填写证书ID
    entitlements_file=None,  # 如需权限配置，填写entitlements文件路径
)

# 创建Mac App Bundle (可选)
# 取消下面的注释以生成 .app 应用程序包
# app = BUNDLE(
#     exe,
#     name='PythonExecutor.app',
#     icon=None,  # 可指定 .icns 图标文件
#     bundle_identifier='com.yourcompany.pythonexecutor',
#     info_plist={
#         'CFBundleShortVersionString': '1.0.0',
#         'CFBundleVersion': '1.0.0',
#         'NSHighResolutionCapable': 'True',
#         'LSBackgroundOnly': 'False',
#     },
# )
