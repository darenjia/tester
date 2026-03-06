@echo off
chcp 65001 >nul
title Python执行器打包工具
echo ============================================
echo    Python执行器 - 打包为EXE可执行文件
echo ============================================
echo.

REM 检查Python是否安装
python --version >nul 2>&1
if errorlevel 1 (
    echo [错误] 未检测到Python，请先安装Python 3.8或更高版本
    pause
    exit /b 1
)

echo [1/5] 检查Python版本...
python --version
echo.

REM 检查并安装pyinstaller
echo [2/5] 检查PyInstaller...
pip show pyinstaller >nul 2>&1
if errorlevel 1 (
    echo PyInstaller未安装，正在安装...
    pip install pyinstaller
    if errorlevel 1 (
        echo [错误] PyInstaller安装失败
        pause
        exit /b 1
    )
) else (
    echo PyInstaller已安装
)
echo.

REM 安装项目依赖
echo [3/5] 安装项目依赖...
pip install -r requirements.txt
if errorlevel 1 (
    echo [警告] 部分依赖安装失败，继续打包...
)
echo.

REM 清理旧构建文件
echo [4/5] 清理旧构建文件...
if exist build rmdir /s /q build
if exist dist rmdir /s /q dist
if exist __pycache__ rmdir /s /q __pycache__
for %%f in (*.spec) do del "%%f" 2>nul
echo 清理完成
echo.

REM 执行打包
echo [5/5] 开始打包...
echo 使用配置: PythonExecutor.spec
echo.

pyinstaller PythonExecutor.spec

if errorlevel 1 (
    echo.
    echo [错误] 打包失败！
    echo 请检查错误信息并修复问题
    pause
    exit /b 1
)

echo.
echo ============================================
echo    打包成功！
echo ============================================
echo.
echo 可执行文件位置:
echo   dist\PythonExecutor.exe
echo.
echo 使用说明:
echo   1. 直接双击运行 PythonExecutor.exe
echo   2. 或在命令行运行: dist\PythonExecutor.exe
echo   3. 服务将在 http://localhost:5000 启动
echo.
echo 注意:
echo   - 首次运行可能需要几秒钟初始化
echo   - 如需调试，请使用命令行运行查看日志
echo   - 配置文件位于同目录的 config 文件夹中
echo.
pause
