"""
PyInstaller打包脚本 - 跨平台版本
将Python执行器打包为独立的可执行文件

支持平台:
    - Windows (.exe)
    - macOS (可执行文件 或 .app)
    - Linux (可执行文件)

使用方法:
    python build_exe.py [选项]

选项:
    --platform [win|mac|linux]  指定目标平台（默认自动检测）
    --onefile                   打包为单个文件（默认启用）
    --windowed                  不显示控制台窗口（仅Windows/Mac）
    --clean                     清理之前的构建文件
    --help                      显示帮助信息

示例:
    python build_exe.py
    python build_exe.py --platform mac
    python build_exe.py --onefile --windowed
"""

import PyInstaller.__main__
import os
import sys
import shutil
import platform
import argparse

def get_platform():
    """获取当前平台"""
    system = platform.system().lower()
    if system == 'darwin':
        return 'mac'
    elif system == 'windows':
        return 'win'
    elif system == 'linux':
        return 'linux'
    return system

def clean_build():
    """清理之前的构建文件"""
    dirs_to_remove = ['build', 'dist', '__pycache__']
    for dir_name in dirs_to_remove:
        if os.path.exists(dir_name):
            print(f"清理 {dir_name}...")
            shutil.rmtree(dir_name)
    
    # 清理 .spec 文件
    for file in os.listdir('.'):
        if file.endswith('.spec') and file != 'PythonExecutor.spec' and file != 'PythonExecutor_mac.spec':
            print(f"删除 {file}...")
            os.remove(file)
    
    # 清理Python缓存
    for root, dirs, files in os.walk('.'):
        for dir_name in dirs:
            if dir_name == '__pycache__':
                cache_path = os.path.join(root, dir_name)
                print(f"清理 {cache_path}...")
                shutil.rmtree(cache_path)

def get_spec_file(target_platform):
    """获取对应平台的spec文件"""
    if target_platform == 'mac':
        return 'PythonExecutor_mac.spec'
    else:
        return 'PythonExecutor.spec'

def build_exe(target_platform=None, onefile=True, windowed=False, clean=True):
    """执行打包"""
    
    # 自动检测平台
    if target_platform is None:
        target_platform = get_platform()
    
    print("=" * 60)
    print(f"开始打包 Python执行器")
    print(f"目标平台: {target_platform}")
    print(f"单文件模式: {onefile}")
    print(f"窗口模式: {windowed}")
    print("=" * 60)
    
    # 清理旧文件
    if clean:
        clean_build()
    
    # 获取spec文件
    spec_file = get_spec_file(target_platform)
    
    if not os.path.exists(spec_file):
        print(f"错误: 找不到spec文件 {spec_file}")
        print(f"请确保 {spec_file} 存在于当前目录")
        return False
    
    print(f"使用配置: {spec_file}")
    print("-" * 60)
    
    # 构建参数
    args = [
        spec_file,
        '--clean',
        '--noconfirm',
    ]
    
    print(f"打包参数: {' '.join(args)}")
    print("-" * 60)
    
    # 执行打包
    try:
        PyInstaller.__main__.run(args)
    except Exception as e:
        print(f"打包失败: {e}")
        import traceback
        traceback.print_exc()
        return False
    
    print("-" * 60)
    print("打包完成!")
    
    # 显示输出路径
    if target_platform == 'win':
        exe_path = os.path.abspath('dist/PythonExecutor.exe')
    elif target_platform == 'mac':
        exe_path = os.path.abspath('dist/PythonExecutor')
        app_path = os.path.abspath('dist/PythonExecutor.app')
    else:
        exe_path = os.path.abspath('dist/PythonExecutor')
    
    print(f"可执行文件: {exe_path}")
    if target_platform == 'mac' and os.path.exists(app_path):
        print(f"App Bundle: {app_path}")
    
    print("=" * 60)
    return True

def main():
    """主函数"""
    parser = argparse.ArgumentParser(
        description='Python执行器打包工具',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
示例:
    python build_exe.py                          # 自动检测平台并打包
    python build_exe.py --platform mac           # 打包为Mac版本
    python build_exe.py --platform win           # 打包为Windows版本
    python build_exe.py --clean                  # 清理后打包
        """
    )
    
    parser.add_argument(
        '--platform',
        choices=['win', 'mac', 'linux'],
        help='目标平台 (默认: 自动检测)'
    )
    
    parser.add_argument(
        '--no-clean',
        action='store_true',
        help='不清理之前的构建文件'
    )
    
    parser.add_argument(
        '--windowed',
        action='store_true',
        help='窗口模式（不显示控制台）'
    )
    
    args = parser.parse_args()
    
    # 执行打包
    success = build_exe(
        target_platform=args.platform,
        onefile=True,
        windowed=args.windowed,
        clean=not args.no_clean
    )
    
    if success:
        print("\n✓ 打包成功!")
        sys.exit(0)
    else:
        print("\n✗ 打包失败!")
        sys.exit(1)

if __name__ == '__main__':
    main()
