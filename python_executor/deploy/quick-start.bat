@echo off
chcp 65001 >nul
title Python执行器快速部署工具

:: 检查管理员权限
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [错误] 需要管理员权限运行此脚本
    echo 请右键点击此脚本，选择"以管理员身份运行"
    pause
    exit /b 1
)

:: 设置颜色
color 0B

echo ========================================
echo    Python执行器快速部署工具
echo ========================================
echo.

:: 检查PowerShell 7
where pwsh >nul 2>&1
if %errorLevel% neq 0 (
    echo [警告] 未检测到PowerShell 7，尝试使用PowerShell 5
    set POWERSHELL=powershell
) else (
    set POWERSHELL=pwsh
)

:: 菜单
:menu
cls
echo ========================================
echo    Python执行器快速部署工具
echo ========================================
echo.
echo  1. 安装服务 (首次部署)
echo  2. 卸载服务
echo  3. 启动服务
echo  4. 停止服务
echo  5. 重启服务
echo  6. 查看状态
echo  7. 更新服务
echo  8. 查看日志
echo  9. 运行健康检查
echo  0. 退出
echo.
echo ========================================
set /p choice=请选择操作 [0-9]: 

if "%choice%"=="1" goto install
if "%choice%"=="2" goto uninstall
if "%choice%"=="3" goto start
if "%choice%"=="4" goto stop
if "%choice%"=="5" goto restart
if "%choice%"=="6" goto status
if "%choice%"=="7" goto update
if "%choice%"=="8" goto logs
if "%choice%"=="9" goto health
if "%choice%"=="0" goto exit

echo [错误] 无效的选择，请重新输入
pause
goto menu

:install
cls
echo [信息] 正在安装服务...
%POWERSHELL% -ExecutionPolicy Bypass -File "%~dp0install-service.ps1" -Action install
if %errorLevel% neq 0 (
    echo [错误] 安装失败
) else (
    echo [成功] 安装完成
)
pause
goto menu

:uninstall
cls
echo [信息] 正在卸载服务...
%POWERSHELL% -ExecutionPolicy Bypass -File "%~dp0install-service.ps1" -Action uninstall
if %errorLevel% neq 0 (
    echo [错误] 卸载失败
) else (
    echo [成功] 卸载完成
)
pause
goto menu

:start
cls
echo [信息] 正在启动服务...
%POWERSHELL% -ExecutionPolicy Bypass -File "%~dp0install-service.ps1" -Action start
if %errorLevel% neq 0 (
    echo [错误] 启动失败
) else (
    echo [成功] 启动完成
)
pause
goto menu

:stop
cls
echo [信息] 正在停止服务...
%POWERSHELL% -ExecutionPolicy Bypass -File "%~dp0install-service.ps1" -Action stop
if %errorLevel% neq 0 (
    echo [错误] 停止失败
) else (
    echo [成功] 停止完成
)
pause
goto menu

:restart
cls
echo [信息] 正在重启服务...
%POWERSHELL% -ExecutionPolicy Bypass -File "%~dp0install-service.ps1" -Action restart
if %errorLevel% neq 0 (
    echo [错误] 重启失败
) else (
    echo [成功] 重启完成
)
pause
goto menu

:status
cls
echo [信息] 正在查询服务状态...
%POWERSHELL% -ExecutionPolicy Bypass -File "%~dp0install-service.ps1" -Action status
echo.
pause
goto menu

:update
cls
echo [信息] 正在更新服务...
echo [警告] 更新前请确保已备份重要数据
echo.
set /p confirm=确认继续? [Y/N]: 
if /i not "%confirm%"=="Y" goto menu

%POWERSHELL% -ExecutionPolicy Bypass -File "%~dp0install-service.ps1" -Action update
if %errorLevel% neq 0 (
    echo [错误] 更新失败
) else (
    echo [成功] 更新完成
)
pause
goto menu

:logs
cls
echo [信息] 查看日志...
echo 1. 查看执行器日志
echo 2. 查看服务日志
echo 3. 实时跟踪日志
echo 0. 返回
set /p logchoice=请选择: 

if "%logchoice%"=="1" (
    if exist "%~dp0..\logs\executor.log" (
        type "%~dp0..\logs\executor.log" | more
    ) else (
        echo [错误] 日志文件不存在
    )
    pause
)
if "%logchoice%"=="2" (
    if exist "%~dp0..\logs\service.log" (
        type "%~dp0..\logs\service.log" | more
    ) else (
        echo [错误] 服务日志不存在
    )
    pause
)
if "%logchoice%"=="3" (
    echo [信息] 按 Ctrl+C 停止跟踪
    if exist "%~dp0..\logs\executor.log" (
        %POWERSHELL% -Command "Get-Content '%~dp0..\logs\executor.log' -Wait -Tail 10"
    ) else (
        echo [错误] 日志文件不存在
        pause
    )
)
goto menu

:health
cls
echo [信息] 运行健康检查...
%POWERSHELL% -Command "
try {
    $response = Invoke-RestMethod -Uri 'http://localhost:8080/health' -TimeoutSec 5
    Write-Host '[成功] 健康检查通过' -ForegroundColor Green
    Write-Host '状态:' $response.status
    Write-Host '客户端数:' $response.clients
    Write-Host '当前任务:' ($response.current_task | ConvertTo-Json -Compress)
} catch {
    Write-Host '[错误] 健康检查失败:' $_.Exception.Message -ForegroundColor Red
}
"
pause
goto menu

:exit
cls
echo 感谢使用 Python执行器快速部署工具
echo.
pause
exit /b 0
