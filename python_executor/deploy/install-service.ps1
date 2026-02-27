<#
.SYNOPSIS
    Python执行器Windows服务安装脚本
.DESCRIPTION
    将Python执行器安装为Windows服务，支持生产环境部署
.PARAMETER Action
    操作类型: install, uninstall, start, stop, restart, status
.PARAMETER ServiceName
    服务名称，默认为 "PythonExecutor"
.PARAMETER InstallDir
    安装目录，默认为脚本所在目录的父目录
.PARAMETER LogDir
    日志目录，默认为安装目录下的logs文件夹
.PARAMETER ConfigDir
    配置目录，默认为安装目录下的config文件夹
.EXAMPLE
    .\install-service.ps1 -Action install
    安装Python执行器服务
.EXAMPLE
    .\install-service.ps1 -Action uninstall
    卸载Python执行器服务
.EXAMPLE
    .\install-service.ps1 -Action status
    查看服务状态
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("install", "uninstall", "start", "stop", "restart", "status", "update")]
    [string]$Action = "status",
    
    [Parameter(Mandatory=$false)]
    [string]$ServiceName = "PythonExecutor",
    
    [Parameter(Mandatory=$false)]
    [string]$InstallDir = $null,
    
    [Parameter(Mandatory=$false)]
    [string]$LogDir = $null,
    
    [Parameter(Mandatory=$false)]
    [string]$ConfigDir = $null,
    
    [Parameter(Mandatory=$false)]
    [string]$PythonPath = "python",
    
    [Parameter(Mandatory=$false)]
    [switch]$Force
)

# 设置错误操作偏好
$ErrorActionPreference = "Stop"

# 颜色定义
$Colors = @{
    Success = "Green"
    Error = "Red"
    Warning = "Yellow"
    Info = "Cyan"
}

# 日志函数
function Write-Log {
    param(
        [string]$Message,
        [string]$Level = "Info"
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $color = $Colors[$Level]
    Write-Host "[$timestamp] [$Level] $Message" -ForegroundColor $color
}

# 检查管理员权限
function Test-AdminRights {
    $currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    return $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# 初始化路径
function Initialize-Paths {
    if ([string]::IsNullOrEmpty($InstallDir)) {
        $script:InstallDir = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
    }
    
    if ([string]::IsNullOrEmpty($LogDir)) {
        $script:LogDir = Join-Path $InstallDir "logs"
    }
    
    if ([string]::IsNullOrEmpty($ConfigDir)) {
        $script:ConfigDir = Join-Path $InstallDir "config"
    }
    
    $script:MainScript = Join-Path $InstallDir "main_production.py"
    $script:NssmPath = Join-Path $PSScriptRoot "tools\nssm.exe"
    $script:ServiceLog = Join-Path $LogDir "service.log"
    
    Write-Log "安装目录: $InstallDir" "Info"
    Write-Log "日志目录: $LogDir" "Info"
    Write-Log "配置目录: $ConfigDir" "Info"
}

# 检查环境
function Test-Environment {
    Write-Log "检查部署环境..." "Info"
    
    # 检查Python
    try {
        $pythonVersion = & $PythonPath --version 2>&1
        Write-Log "Python版本: $pythonVersion" "Success"
    }
    catch {
        Write-Log "Python未找到或无法执行: $PythonPath" "Error"
        throw "Python环境检查失败"
    }
    
    # 检查安装目录
    if (-not (Test-Path $InstallDir)) {
        Write-Log "安装目录不存在: $InstallDir" "Error"
        throw "安装目录检查失败"
    }
    
    # 检查主脚本
    if (-not (Test-Path $MainScript)) {
        Write-Log "主脚本不存在: $MainScript" "Error"
        throw "主脚本检查失败"
    }
    
    # 检查依赖
    $requirementsFile = Join-Path $InstallDir "requirements_production.txt"
    if (Test-Path $requirementsFile) {
        Write-Log "检查Python依赖..." "Info"
        try {
            & $PythonPath -m pip install -r $requirementsFile -q
            Write-Log "依赖检查完成" "Success"
        }
        catch {
            Write-Log "依赖安装失败: $_" "Warning"
        }
    }
    
    # 创建必要目录
    @($LogDir, $ConfigDir) | ForEach-Object {
        if (-not (Test-Path $_)) {
            New-Item -ItemType Directory -Path $_ -Force | Out-Null
            Write-Log "创建目录: $_" "Info"
        }
    }
    
    Write-Log "环境检查完成" "Success"
}

# 下载NSSM
function Get-Nssm {
    if (Test-Path $NssmPath) {
        Write-Log "NSSM已存在: $NssmPath" "Info"
        return
    }
    
    Write-Log "下载NSSM..." "Info"
    
    $nssmUrl = "https://nssm.cc/release/nssm-2.24.zip"
    $nssmZip = Join-Path $env:TEMP "nssm.zip"
    $nssmExtractPath = Join-Path $env:TEMP "nssm"
    
    try {
        # 下载
        Invoke-WebRequest -Uri $nssmUrl -OutFile $nssmZip -UseBasicParsing
        
        # 解压
        Expand-Archive -Path $nssmZip -DestinationPath $nssmExtractPath -Force
        
        # 复制到工具目录
        $toolsDir = Split-Path $NssmPath
        if (-not (Test-Path $toolsDir)) {
            New-Item -ItemType Directory -Path $toolsDir -Force | Out-Null
        }
        
        $nssmSource = Join-Path $nssmExtractPath "nssm-2.24\win64\nssm.exe"
        Copy-Item $nssmSource $NssmPath -Force
        
        # 清理
        Remove-Item $nssmZip -Force -ErrorAction SilentlyContinue
        Remove-Item $nssmExtractPath -Recurse -Force -ErrorAction SilentlyContinue
        
        Write-Log "NSSM下载完成" "Success"
    }
    catch {
        Write-Log "NSSM下载失败: $_" "Error"
        throw "NSSM下载失败"
    }
}

# 安装服务
function Install-Service {
    Write-Log "安装服务: $ServiceName..." "Info"
    
    # 检查服务是否已存在
    $existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($existingService) {
        if ($Force) {
            Write-Log "服务已存在，强制重新安装..." "Warning"
            Uninstall-Service
        }
        else {
            Write-Log "服务已存在，使用 -Force 参数强制重新安装" "Error"
            throw "服务已存在"
        }
    }
    
    # 获取NSSM
    Get-Nssm
    
    # 安装服务
    $arguments = @(
        "install",
        $ServiceName,
        $PythonPath,
        "`"$MainScript`""
    )
    
    $process = Start-Process -FilePath $NssmPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -ne 0) {
        throw "NSSM安装服务失败，退出码: $($process.ExitCode)"
    }
    
    # 配置服务
    & $NssmPath set $ServiceName DisplayName "Python Test Executor"
    & $NssmPath set $ServiceName Description "Python测试执行器服务 - 远程CANoe/TSMaster任务执行"
    & $NssmPath set $ServiceName Start SERVICE_AUTO_START
    & $NssmPath set $ServiceName AppDirectory $InstallDir
    & $NssmPath set $ServiceName AppStdout $ServiceLog
    & $NssmPath set $ServiceName AppStderr $ServiceLog
    & $NssmPath set $ServiceName AppRotateFiles 1
    & $NssmPath set $ServiceName AppRotateOnline 1
    & $NssmPath set $ServiceName AppRotateBytes 10485760  # 10MB
    
    # 设置环境变量
    $envPath = "PYTHONPATH=$InstallDir;PATH=$InstallDir;$env:PATH"
    & $NssmPath set $ServiceName AppEnvironmentExtra $envPath
    
    Write-Log "服务安装成功" "Success"
    
    # 启动服务
    Start-Service
}

# 卸载服务
function Uninstall-Service {
    Write-Log "卸载服务: $ServiceName..." "Info"
    
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if (-not $service) {
        Write-Log "服务不存在" "Warning"
        return
    }
    
    # 停止服务
    if ($service.Status -eq "Running") {
        Stop-Service
    }
    
    # 卸载服务
    $process = Start-Process -FilePath $NssmPath -ArgumentList "remove", $ServiceName, "confirm" -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -ne 0) {
        Write-Log "服务卸载可能失败，退出码: $($process.ExitCode)" "Warning"
    }
    else {
        Write-Log "服务卸载成功" "Success"
    }
}

# 启动服务
function Start-Service {
    Write-Log "启动服务: $ServiceName..." "Info"
    
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if (-not $service) {
        throw "服务不存在，请先安装"
    }
    
    if ($service.Status -eq "Running") {
        Write-Log "服务已在运行" "Warning"
        return
    }
    
    Start-Service -Name $ServiceName
    
    # 等待服务启动
    $timeout = 30
    $timer = [Diagnostics.Stopwatch]::StartNew()
    
    while ($timer.Elapsed.TotalSeconds -lt $timeout) {
        $service.Refresh()
        if ($service.Status -eq "Running") {
            Write-Log "服务启动成功" "Success"
            return
        }
        Start-Sleep -Seconds 1
    }
    
    throw "服务启动超时"
}

# 停止服务
function Stop-Service {
    Write-Log "停止服务: $ServiceName..." "Info"
    
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if (-not $service) {
        Write-Log "服务不存在" "Warning"
        return
    }
    
    if ($service.Status -eq "Stopped") {
        Write-Log "服务已停止" "Warning"
        return
    }
    
    Stop-Service -Name $ServiceName -Force
    
    # 等待服务停止
    $timeout = 30
    $timer = [Diagnostics.Stopwatch]::StartNew()
    
    while ($timer.Elapsed.TotalSeconds -lt $timeout) {
        $service.Refresh()
        if ($service.Status -eq "Stopped") {
            Write-Log "服务停止成功" "Success"
            return
        }
        Start-Sleep -Seconds 1
    }
    
    throw "服务停止超时"
}

# 重启服务
function Restart-Service {
    Write-Log "重启服务: $ServiceName..." "Info"
    Stop-Service
    Start-Sleep -Seconds 2
    Start-Service
}

# 查看服务状态
function Get-ServiceStatus {
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    
    if (-not $service) {
        Write-Log "服务不存在: $ServiceName" "Error"
        return
    }
    
    Write-Log "服务名称: $($service.Name)" "Info"
    Write-Log "显示名称: $($service.DisplayName)" "Info"
    Write-Log "状态: $($service.Status)" $(if ($service.Status -eq "Running") { "Success" } else { "Warning" })
    Write-Log "启动类型: $($service.StartType)" "Info"
    
    # 获取进程信息
    if ($service.Status -eq "Running") {
        $process = Get-CimInstance Win32_Process | Where-Object { $_.Name -like "python*" -and $_.CommandLine -like "*$MainScript*" }
        if ($process) {
            Write-Log "进程ID: $($process.ProcessId)" "Info"
            Write-Log "内存使用: $([math]::Round($process.WorkingSetSize / 1MB, 2)) MB" "Info"
            Write-Log "启动时间: $($process.CreationDate)" "Info"
        }
    }
    
    # 检查日志
    if (Test-Path $ServiceLog) {
        $logSize = (Get-Item $ServiceLog).Length
        Write-Log "服务日志: $ServiceLog ($([math]::Round($logSize / 1KB, 2)) KB)" "Info"
    }
}

# 更新服务
function Update-Service {
    Write-Log "更新服务: $ServiceName..." "Info"
    
    # 备份当前版本
    $backupDir = Join-Path $InstallDir "backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    Write-Log "备份当前版本到: $backupDir" "Info"
    
    # 创建备份（排除logs和backup目录）
    $exclude = @("logs", "backup*", "*.pyc", "__pycache__")
    Copy-Item -Path $InstallDir -Destination $backupDir -Recurse -Exclude $exclude -Force
    
    # 停止服务
    Stop-Service
    
    # 更新代码（这里可以添加Git pull或其他更新逻辑）
    Write-Log "请手动更新代码文件，然后按任意键继续..." "Warning"
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    
    # 更新依赖
    $requirementsFile = Join-Path $InstallDir "requirements_production.txt"
    if (Test-Path $requirementsFile) {
        Write-Log "更新依赖..." "Info"
        & $PythonPath -m pip install -r $requirementsFile --upgrade -q
    }
    
    # 启动服务
    Start-Service
    
    # 验证
    Start-Sleep -Seconds 3
    Get-ServiceStatus
    
    Write-Log "服务更新完成" "Success"
}

# 主函数
function Main {
    Write-Log "========================================" "Info"
    Write-Log "Python执行器服务管理脚本" "Info"
    Write-Log "操作: $Action" "Info"
    Write-Log "========================================" "Info"
    
    # 检查管理员权限
    if (-not (Test-AdminRights)) {
        Write-Log "需要管理员权限运行此脚本" "Error"
        Write-Log "请以管理员身份重新运行PowerShell并执行脚本" "Error"
        exit 1
    }
    
    try {
        Initialize-Paths
        
        switch ($Action) {
            "install" {
                Test-Environment
                Install-Service
            }
            "uninstall" {
                Uninstall-Service
            }
            "start" {
                Start-Service
            }
            "stop" {
                Stop-Service
            }
            "restart" {
                Restart-Service
            }
            "status" {
                Get-ServiceStatus
            }
            "update" {
                Update-Service
            }
        }
        
        Write-Log "操作完成" "Success"
    }
    catch {
        Write-Log "操作失败: $_" "Error"
        Write-Log $_.ScriptStackTrace "Error"
        exit 1
    }
}

# 执行主函数
Main
