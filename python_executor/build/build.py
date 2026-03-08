"""
打包脚本
使用 PyInstaller 将应用打包成 Windows 可执行文件
"""
import os
import sys
import shutil
import subprocess
from pathlib import Path


def get_project_root():
    """获取项目根目录"""
    return os.path.dirname(os.path.dirname(os.path.abspath(__file__)))


def clean_build_dirs():
    """清理构建目录"""
    project_root = get_project_root()
    dirs_to_clean = ['build', 'dist', '__pycache__']
    
    for dir_name in dirs_to_clean:
        dir_path = os.path.join(project_root, dir_name)
        if os.path.exists(dir_path):
            print(f"清理目录: {dir_path}")
            shutil.rmtree(dir_path)
    
    # 清理 .pyc 文件和 __pycache__ 目录
    for root, dirs, files in os.walk(project_root):
        # 清理 .pyc 文件
        for file in files:
            if file.endswith('.pyc'):
                try:
                    os.remove(os.path.join(root, file))
                except Exception as e:
                    print(f"警告: 无法删除文件 {file}: {e}")
        
        # 清理 __pycache__ 目录（不修改 dirs 列表）
        for dir_name in dirs[:]:  # 使用切片复制列表
            if dir_name == '__pycache__':
                try:
                    shutil.rmtree(os.path.join(root, dir_name))
                except Exception as e:
                    print(f"警告: 无法删除目录 {dir_name}: {e}")


def create_spec_file():
    """创建 PyInstaller spec 文件"""
    project_root = get_project_root()
    
    # 转义路径中的特殊字符
    app_path = os.path.join(project_root, 'app.py').replace('\\', '/')
    template_path = os.path.join(project_root, 'web/templates').replace('\\', '/')
    static_path = os.path.join(project_root, 'web/static').replace('\\', '/')
    icon_path = os.path.join(project_root, 'build/icon.ico').replace('\\', '/')
    
    # 检查图标文件是否存在
    has_icon = os.path.exists(os.path.join(project_root, 'build', 'icon.ico'))
    
    spec_content = f'''# -*- mode: python ; coding: utf-8 -*-

block_cipher = None

a = Analysis(
    [r"{app_path}"],
    pathex=[r"{project_root.replace('\\', '/')}"],
    binaries=[],
    datas=[
        (r"{template_path}", "web/templates"),
        (r"{static_path}", "web/static"),
    ],
    hiddenimports=[
        'webview',
        'webview.platforms.winforms',
        'flask',
        'werkzeug',
        'werkzeug.routing',
        'jinja2',
        'jinja2.ext',
        'markupsafe',
        'itsdangerous',
        'click',
        'psutil',
        'psutil._pswindows',
        'websockets',
        'websockets.legacy',
        'websockets.legacy.client',
        'win32com',
        'win32com.client',
        'win32com.gen_py',
        'pythoncom',
        'config.settings',
        'core.status_monitor',
        'utils.logger',
        'web.server',
    ],
    hookspath=[],
    hooksconfig={{}},
    runtime_hooks=[],
    excludes=[
        'matplotlib',
        'numpy',
        'pandas',
        'scipy',
        'tkinter',
        'unittest',
        'pytest',
    ],
    win_no_prefer_redirects=False,
    win_private_assemblies=False,
    cipher=block_cipher,
    noarchive=False,
)

pyz = PYZ(a.pure, a.zipped_data, cipher=block_cipher)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.zipfiles,
    a.datas,
    [],
    name='测试执行器',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    upx_exclude=[],
    runtime_tmpdir=None,
    console=False,
    disable_windowed_traceback=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
    {f'icon=r"{icon_path}",' if has_icon else 'icon=None,'}
)
'''
    
    spec_path = os.path.join(project_root, 'TestExecutor.spec')
    with open(spec_path, 'w', encoding='utf-8') as f:
        f.write(spec_content)
    
    return spec_path


def create_version_file():
    """创建版本信息文件"""
    project_root = get_project_root()
    version_path = os.path.join(project_root, 'build', 'version.txt')
    
    version_content = """# UTF-8
#
# 有关如何创建版本信息文件的详细信息，请参阅：
# https://pyinstaller.readthedocs.io/en/stable/usage.html#capturing-windows-version-data

VSVersionInfo(
  ffi=FixedFileInfo(
    filevers=(1, 0, 0, 0),
    prodvers=(1, 0, 0, 0),
    mask=0x3f,
    flags=0x0,
    OS=0x40004,
    fileType=0x1,
    subtype=0x0,
    date=(0, 0)
  ),
  kids=[
    StringFileInfo(
      [
      StringTable(
        '080404b0',
        [StringStruct('CompanyName', '测试执行器'),
        StringStruct('FileDescription', '测试平台远程执行器'),
        StringStruct('FileVersion', '1.0.0.0'),
        StringStruct('InternalName', '测试执行器'),
        StringStruct('LegalCopyright', 'Copyright (C) 2025'),
        StringStruct('OriginalFilename', '测试执行器.exe'),
        StringStruct('ProductName', '测试执行器'),
        StringStruct('ProductVersion', '1.0.0.0')])
      ]), 
    VarFileInfo([VarStruct('Translation', [2052, 1200])])
  ]
)
"""
    
    # 确保 build 目录存在
    build_dir = os.path.join(project_root, 'build')
    if not os.path.exists(build_dir):
        os.makedirs(build_dir)
    
    with open(version_path, 'w', encoding='utf-8') as f:
        f.write(version_content)
    
    return version_path


def build_executable(onefile=True):
    """
    构建可执行文件
    
    Args:
        onefile: 是否打包成单个文件，False 则打包成目录
    """
    project_root = get_project_root()
    
    print("=" * 60)
    print("开始打包测试执行器")
    print(f"打包模式: {'单文件' if onefile else '单目录'}")
    print("=" * 60)
    
    # 清理旧的构建文件
    print("\n[1/5] 清理旧的构建文件...")
    clean_build_dirs()
    
    # 创建版本信息文件
    print("\n[2/5] 创建版本信息文件...")
    version_file = create_version_file()
    
    # 创建 spec 文件
    print("\n[3/5] 创建打包配置...")
    spec_file = create_spec_file()
    print(f"配置文件: {spec_file}")
    
    # 运行 PyInstaller
    print("\n[4/5] 正在打包...")
    cmd = [
        sys.executable, '-m', 'PyInstaller',
        spec_file,
        '--clean',
        '--noconfirm',
    ]
    
    if onefile:
        cmd.append('--onefile')
    else:
        cmd.append('--onedir')
    
    # 添加版本信息
    cmd.extend(['--version-file', version_file])
    
    try:
        result = subprocess.run(cmd, cwd=project_root, capture_output=False, text=True)
        if result.returncode != 0:
            print("❌ 打包失败!")
            return False
    except Exception as e:
        print(f"❌ 打包出错: {e}")
        return False
    
    # 验证打包结果
    print("\n[5/5] 验证打包结果...")
    dist_dir = os.path.join(project_root, 'dist')
    exe_path = os.path.join(dist_dir, '测试执行器.exe')
    
    if not os.path.exists(exe_path):
        print(f"❌ 错误: 找不到生成的可执行文件: {exe_path}")
        return False
    
    # 获取文件大小
    exe_size = os.path.getsize(exe_path)
    print(f"✅ 可执行文件生成成功")
    print(f"   路径: {exe_path}")
    print(f"   大小: {exe_size / 1024 / 1024:.2f} MB")
    
    # 复制额外文件到输出目录
    print("\n[6/6] 复制资源文件...")
    
    # 创建 README
    readme_content = '''测试执行器 v1.0.0
================

使用说明:
1. 直接运行 "测试执行器.exe" 即可启动应用程序
2. 首次运行会自动创建默认配置文件 config.json
3. 日志文件保存在 logs 目录下

配置文件:
- config.json: 应用程序配置文件，包含服务端地址、设备信息等

注意事项:
- 请确保已安装 CANoe、TSMaster 或 TTworkbench 测试软件
- 配置正确的软件路径后才能正常执行测试任务

技术支持:
如有问题请联系系统管理员
'''
    
    readme_path = os.path.join(dist_dir, 'README.txt')
    with open(readme_path, 'w', encoding='utf-8') as f:
        f.write(readme_content)
    
    print(f"✅ README 文件已创建")
    
    print("\n" + "=" * 60)
    print("🎉 打包完成!")
    print("=" * 60)
    print(f"输出目录: {dist_dir}")
    print(f"可执行文件: {exe_path}")
    print("=" * 60)
    
    return True


def main():
    """主函数"""
    # 检查 PyInstaller 是否已安装
    try:
        import PyInstaller.__main__
    except ImportError:
        print("❌ 错误: 未安装 PyInstaller")
        print("请先运行: pip install pyinstaller")
        sys.exit(1)
    
    # 解析命令行参数
    onefile = True
    if '--onedir' in sys.argv:
        onefile = False
    elif '--onefile' in sys.argv:
        onefile = True
    
    # 执行打包
    if build_executable(onefile=onefile):
        print("\n✅ 打包成功!")
        sys.exit(0)
    else:
        print("\n❌ 打包失败!")
        sys.exit(1)


if __name__ == '__main__':
    main()
