#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Docker部署脚本

.DESCRIPTION
    用于构建和启动Python执行器容器

.EXAMPLE
    .\deploy.ps1 -Mode build
    .\deploy.ps1 -Mode up
    .\deploy.ps1 -Mode down
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("build", "up", "down", "restart", "logs", "status", "clean")]
    [string]$Mode = "up",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("windows", "linux")]
    [string]$Platform = "windows",
    
    [Parameter(Mandatory=$false)]
    [string]$ConfigFile = ""
)

$ErrorActionPreference = "Stop"

# 脚本目录
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$DockerDir = $ScriptDir

# 颜色输出函数
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# 显示横幅
function Show-Banner {
    Write-ColorOutput "`n========================================" -Color Cyan
    Write-ColorOutput "  Python执行器 Docker部署" -Color Cyan
    Write-ColorOutput "========================================`n" -Color Cyan
}

# 构建镜像
function Build-Image {
    Write-ColorOutput "开始构建Docker镜像..." -Color Yellow
    
    $dockerFile = "$DockerDir\$Platform\Dockerfile"
    if (-not (Test-Path $dockerFile)) {
        Write-ColorOutput "Dockerfile不存在: $dockerFile" -Color Red
        exit 1
    }
    
    $ ($Platform -eq "windows") {
        "$composeFile = ifDockerDir\docker-compose.yml"
    } else {
        "$DockerDir\docker-compose.linux.yml"
    }
    
    Push-Location $DockerDir
    try {
        if ($Platform -eq "windows") {
            docker-compose build
        } else {
            docker-compose -f docker-compose.linux.yml build
        }
function Start-Container {
    Write-ColorOutput "启动容器..." -Color Yellow
    
    Push-Location $DockerDir
    try {
        if ($Platform -eq "windows") {
            docker-compose up -d
        } else {
            docker-compose -f docker-compose.linux.yml up -d
        }
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "容器启动成功!" -Color Green
            Show-ContainerStatus
        } else {
            Write-ColorOutput "容器启动失败!" -Color Red
            exit 1
        }
    }
    finally {
        Pop-Location
    }
}

# 停止容器
function Stop-Container {
    Write-ColorOutput "停止容器..." -Color Yellow
    
    Push-Location $DockerDir
    try {
        if ($Platform -eq "windows") {
            docker-compose down
        } else {
            docker-compose -f docker-compose.linux.yml down
        }
        
        Write-ColorOutput "容器已停止!" -Color Green
    }
    finally {
        Pop-Location
    }
}

# 重启容器
function Restart-Container {
    Stop-Container
    Start-Container
}

# 查看日志
function Show-Logs {
    Write-ColorOutput "查看容器日志 (Ctrl+C退出)..." -Color Yellow
    
    Push-Location $DockerDir
    try {
        if ($Platform -eq "windows") {
            docker-compose logs -f
        } else {
            docker-compose -f docker-compose.linux.yml logs -f
        }
    }
    finally {
        Pop-Location
    }
}

# 显示状态
function Show-ContainerStatus {
    Write-ColorOutput "`n容器状态:" -Color Yellow
    Write-ColorOutput "----------------------------------------" -Color Gray
    
    docker ps --filter "name=python-executor" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    
    Write-ColorOutput "`n健康检查:" -Color Yellow
    try {
        $health = Invoke-WebRequest -Uri http://localhost:8180/health -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($health) {
            Write-ColorOutput "  HTTP API: 正常" -Color Green
        }
    } catch {
        Write-ColorOutput "  HTTP API: 异常" -Color Red
    }
}

# 清理
function Clean-Container {
    Write-ColorOutput "清理容器和镜像..." -Color Yellow
    
    Push-Location $DockerDir
    try {
        if ($Platform -eq "windows") {
            docker-compose down --rmi local --volumes
        } else {
            docker-compose -f docker-compose.linux.yml down --rmi local --volumes
        }
        
        Write-ColorOutput "清理完成!" -Color Green
    }
    finally {
        Pop-Location
    }
}

# 主程序
Show-Banner

switch ($Mode) {
    "build" {
        Build-Image
    }
    "up" {
        Start-Container
    }
    "down" {
        Stop-Container
    }
    "restart" {
        Restart-Container
    }
    "logs" {
        Show-Logs
    }
    "status" {
        Show-ContainerStatus
    }
    "clean" {
        Clean-Container
    }
}

Write-ColorOutput "`n操作完成!" -Color Green
