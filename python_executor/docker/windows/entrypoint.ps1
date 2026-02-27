#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Python执行器启动脚本

.DESCRIPTION
    用于在Windows容器中启动Python执行器

.PARAMETER ConfigPath
    配置文件路径

.PARAMETER LogLevel
    日志级别 (DEBUG, INFO, WARNING, ERROR)

.PARAMETER Debug
    启用调试模式

.EXAMPLE
    .\entrypoint.ps1
    .\entrypoint.ps1 -ConfigPath C:/app/config/config.json -LogLevel DEBUG
#>

param(
    [string]$ConfigPath = $env:CONFIG_PATH,
    [string]$LogLevel = $env:LOG_LEVEL,
    [switch]$Debug = $false
)

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Python执行器 容器启动" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 默认值
if ([string]::IsNullOrEmpty($ConfigPath)) {
    $ConfigPath = "C:/app/config/config.json"
}
if ([string]::IsNullOrEmpty($LogLevel)) {
    $LogLevel = "INFO"
}

# 检查Python安装
Write-Host "检查Python安装..." -ForegroundColor Yellow
$pythonVersion = python --version 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "  [OK] Python版本: $pythonVersion" -ForegroundColor Green
} else {
    Write-Host "  [ERROR] Python未安装" -ForegroundColor Red
    exit 1
}

# 检查配置文件
Write-Host ""
Write-Host "检查配置文件..." -ForegroundColor Yellow
if (Test-Path $ConfigPath) {
    Write-Host "  [OK] 配置文件存在: $ConfigPath" -ForegroundColor Green
} else {
    Write-Host "  [WARNING] 配置文件不存在: $ConfigPath" -ForegroundColor Yellow
    Write-Host "  将使用默认配置..." -ForegroundColor Yellow
}

# 检查CANoe安装
Write-Host ""
Write-Host "检查测试工具安装..." -ForegroundColor Yellow

$canoePath = "C:\Program Files\CANoe"
$tsmasterPath = "C:\Program Files\TSMaster"

if (Test-Path $canoePath) {
    $canoeVersion = (Get-ChildItem "$canoePath\*.exe" -ErrorAction SilentlyContinue | 
                     Where-Object { $_.Name -match "CANoe" } | 
                     Select-Object -First 1).Name
    Write-Host "  [OK] CANoe已安装: $canoePath" -ForegroundColor Green
} else {
    Write-Host "  [WARNING] CANoe未安装" -ForegroundColor Yellow
    $env:CANOE_ENABLED = "false"
}

if (Test-Path $tsmasterPath) {
    Write-Host "  [OK] TSMaster已安装: $tsmasterPath" -ForegroundColor Green
} else {
    Write-Host "  [WARNING] TSMaster未安装" -ForegroundColor Yellow
    $env:TSMASTER_ENABLED = "false"
}

# 设置环境变量
Write-Host ""
Write-Host "配置环境变量..." -ForegroundColor Yellow

$env:PYTHONEXECUTOR_CONFIG = $ConfigPath
$env:PYTHONEXECUTOR_LOGLEVEL = $LogLevel

if ($Debug) {
    $env:FLASK_DEBUG = "1"
    $env:PYTHONEXECUTOR_DEBUG = "1"
}

Write-Host "  配置文件: $ConfigPath"
Write-Host "  日志级别: $LogLevel"
Write-Host "  调试模式: $Debug"

# 检查端口占用
Write-Host ""
Write-Host "检查端口占用..." -ForegroundColor Yellow
$port5000 = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue
$port5001 = Get-NetTCPConnection -LocalPort 5001 -ErrorAction SilentlyContinue

if ($port5000) {
    Write-Host "  [WARNING] 端口5000已被占用" -ForegroundColor Yellow
} else {
    Write-Host "  [OK] 端口5000可用" -ForegroundColor Green
}

if ($port5001) {
    Write-Host "  [WARNING] 端口5001已被占用" -ForegroundColor Yellow
} else {
    Write-Host "  [OK] 端口5001可用" -ForegroundColor Green
}

# 启动应用
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  启动Python执行器..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 切换到应用目录
Set-Location C:/app

# 使用python直接运行main.py
python C:/app/main.py

# 如果程序退出，记录退出码
$exitCode = $LASTEXITCODE
Write-Host ""
Write-Host "========================================" -ForegroundColor Red
Write-Host "  应用已退出，退出码: $exitCode" -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red

exit $exitCode
