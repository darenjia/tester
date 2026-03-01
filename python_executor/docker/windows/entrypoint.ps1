# Python执行器启动脚本
# Windows容器环境
#

param(
    [string]$ConfigPath = $env:CONFIG_PATH,
    [string]$LogLevel = $env:LOG_LEVEL,
    [string]$ExecutorId = $env:EXECUTOR_ID
)

# 设置错误处理
$ErrorActionPreference = "Stop"

# 颜色输出函数
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# 默认值
if (-not $ConfigPath) { $ConfigPath = "C:/app/config/config.json" }
if (-not $LogLevel) { $LogLevel = "INFO" }
if (-not $ExecutorId) { $ExecutorId = "executor-windows-01" }

Write-ColorOutput "========================================" -Color Cyan
Write-ColorOutput "  Python执行器 容器启动" -Color Cyan
Write-ColorOutput "========================================" -Color Cyan
Write-ColorOutput ""

# 检查Python安装
Write-ColorOutput "检查Python安装..." -Color Yellow
$pythonVersion = python --version 2>&1
Write-ColorOutput "  [OK] Python版本: $pythonVersion" -Color Green

# 检查配置文件
Write-ColorOutput ""
Write-ColorOutput "检查配置文件..." -Color Yellow
if (Test-Path $ConfigPath) {
    Write-ColorOutput "  [OK] 配置文件存在: $ConfigPath" -Color Green
} else {
    Write-ColorOutput "  [WARNING] 配置文件不存在: $ConfigPath" -Color Yellow
    Write-ColorOutput "  将使用默认配置..." -Color Yellow
}

# 显示环境变量
Write-ColorOutput ""
Write-ColorOutput "环境变量配置..." -Color Yellow
Write-ColorOutput "  执行器ID: $ExecutorId"
Write-ColorOutput "  日志级别: $LogLevel"
Write-ColorOutput "  CANoe启用: $env:CANOE_ENABLED"
Write-ColorOutput "  TSMaster启用: $env:TSMASTER_ENABLED"

# 检查端口占用
Write-ColorOutput ""
Write-ColorOutput "检查端口占用..." -Color Yellow
$port8180 = Get-NetTCPConnection -LocalPort 8180 -ErrorAction SilentlyContinue

if ($port8180) {
    Write-ColorOutput "  [WARNING] 端口8180已被占用" -Color Yellow
} else {
    Write-ColorOutput "  [OK] 端口8180可用" -Color Green
}

# 启动应用
Write-ColorOutput ""
Write-ColorOutput "========================================" -Color Cyan
Write-ColorOutput "  启动Python执行器..." -Color Cyan
Write-ColorOutput "========================================" -Color Cyan
Write-ColorOutput ""

# 切换到应用目录
Set-Location C:/app

# 使用python直接运行main_production.py
python C:/app/main_production.py

# 如果程序退出，记录退出码
$exitCode = $LASTEXITCODE
Write-ColorOutput ""
Write-ColorOutput "========================================" -Color Red
Write-ColorOutput "  应用已退出，退出码: $exitCode" -Color Red
Write-ColorOutput "========================================" -Color Red

exit $exitCode
