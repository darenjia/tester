<#
.SYNOPSIS
    Python执行器批量部署脚本
.DESCRIPTION
    在多台测试设备上批量部署Python执行器服务
.PARAMETER DeviceList
    设备列表文件路径，每行一个设备名或IP
.PARAMETER Action
    操作类型: deploy, update, status, stop, start
.PARAMETER SourcePath
    源代码路径
.PARAMETER ConfigTemplate
    配置模板文件路径
.PARAMETER Parallel
    并行部署数量，默认为3
.EXAMPLE
    .\deploy-batch.ps1 -DeviceList devices.txt -Action deploy
    批量部署到设备列表中的所有设备
.EXAMPLE
    .\deploy-batch.ps1 -DeviceList devices.txt -Action update
    批量更新所有设备上的执行器
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$DeviceList,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("deploy", "update", "status", "stop", "start", "restart", "uninstall")]
    [string]$Action = "status",
    
    [Parameter(Mandatory=$false)]
    [string]$SourcePath = $null,
    
    [Parameter(Mandatory=$false)]
    [string]$ConfigTemplate = $null,
    
    [Parameter(Mandatory=$false)]
    [int]$Parallel = 3,
    
    [Parameter(Mandatory=$false)]
    [string]$InstallDir = "C:\PythonExecutor",
    
    [Parameter(Mandatory=$false)]
    [string]$ServiceName = "PythonExecutor",
    
    [Parameter(Mandatory=$false)]
    [PSCredential]$Credential = $null,
    
    [Parameter(Mandatory=$false)]
    [switch]$WhatIf
)

# 设置错误操作偏好
$ErrorActionPreference = "Stop"

# 颜色定义
$Colors = @{
    Success = "Green"
    Error = "Red"
    Warning = "Yellow"
    Info = "Cyan"
    Debug = "Gray"
}

# 日志函数
function Write-Log {
    param(
        [string]$Message,
        [string]$Level = "Info",
        [string]$Device = ""
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $color = $Colors[$Level]
    $devicePrefix = if ($Device) { "[$Device] " } else { "" }
    Write-Host "[$timestamp] [$Level] $devicePrefix$Message" -ForegroundColor $color
}

# 读取设备列表
function Get-DeviceList {
    if (-not (Test-Path $DeviceList)) {
        throw "设备列表文件不存在: $DeviceList"
    }
    
    $devices = Get-Content $DeviceList | Where-Object { 
        $_ -and $_ -notmatch '^\s*#' -and $_ -notmatch '^\s*$' 
    } | ForEach-Object { $_.Trim() }
    
    Write-Log "读取到 $($devices.Count) 个设备" "Info"
    return $devices
}

# 测试设备连接
function Test-DeviceConnection {
    param([string]$Device)
    
    try {
        $result = Test-Connection -ComputerName $Device -Count 1 -Quiet -ErrorAction SilentlyContinue
        return $result
    }
    catch {
        return $false
    }
}

# 在远程设备上执行命令
function Invoke-RemoteCommand {
    param(
        [string]$Device,
        [string]$Command,
        [string]$ArgumentList = ""
    )
    
    try {
        if ($Credential) {
            $session = New-PSSession -ComputerName $Device -Credential $Credential -ErrorAction Stop
        }
        else {
            $session = New-PSSession -ComputerName $Device -ErrorAction Stop
        }
        
        $result = Invoke-Command -Session $session -ScriptBlock {
            param($cmd, $args)
            $process = Start-Process -FilePath $cmd -ArgumentList $args -Wait -PassThru -NoNewWindow
            return $process.ExitCode
        } -ArgumentList $Command, $ArgumentList
        
        Remove-PSSession $session
        return $result
    }
    catch {
        Write-Log "远程命令执行失败: $_" "Error" $Device
        return -1
    }
}

# 复制文件到远程设备
function Copy-FilesToDevice {
    param(
        [string]$Device,
        [string]$Source,
        [string]$Destination
    )
    
    try {
        # 创建目标目录
        Invoke-Command -ComputerName $Device -ScriptBlock {
            param($dest)
            if (-not (Test-Path $dest)) {
                New-Item -ItemType Directory -Path $dest -Force | Out-Null
            }
        } -ArgumentList $Destination -Credential $Credential
        
        # 复制文件
        $session = New-PSSession -ComputerName $Device -Credential $Credential
        Copy-Item -Path "$Source\*" -Destination $Destination -Recurse -Force -ToSession $session
        Remove-PSSession $session
        
        return $true
    }
    catch {
        Write-Log "文件复制失败: $_" "Error" $Device
        return $false
    }
}

# 生成设备特定配置
function New-DeviceConfig {
    param(
        [string]$Device,
        [string]$TemplatePath,
        [string]$OutputPath
    )
    
    try {
        # 读取模板
        $config = Get-Content $TemplatePath -Raw | ConvertFrom-Json
        
        # 根据设备名修改配置
        $config.device_id = $Device
        $config.device_name = $Device
        
        # 生成设备特定端口（避免冲突）
        $portBase = 8180
        $deviceHash = [math]::Abs($Device.GetHashCode()) % 1000
        $config.websocket.port = $portBase + $deviceHash
        
        # 保存配置
        $config | ConvertTo-Json -Depth 10 | Out-File $OutputPath -Encoding UTF8
        
        return $true
    }
    catch {
        Write-Log "生成配置失败: $_" "Error" $Device
        return $false
    }
}

# 部署到单个设备
function Deploy-ToDevice {
    param([string]$Device)
    
    Write-Log "开始部署..." "Info" $Device
    
    # 测试连接
    if (-not (Test-DeviceConnection $Device)) {
        Write-Log "设备无法连接" "Error" $Device
        return @{ Device = $Device; Status = "Failed"; Reason = "Connection failed" }
    }
    Write-Log "设备连接正常" "Success" $Device
    
    if ($WhatIf) {
        Write-Log "[WhatIf] 将执行部署操作" "Warning" $Device
        return @{ Device = $Device; Status = "WhatIf"; Reason = "Simulation mode" }
    }
    
    try {
        # 1. 停止现有服务
        Write-Log "停止现有服务..." "Info" $Device
        Invoke-RemoteCommand -Device $Device -Command "sc" -ArgumentList "stop $ServiceName" | Out-Null
        Start-Sleep -Seconds 3
        
        # 2. 备份现有版本
        Write-Log "备份现有版本..." "Info" $Device
        $backupDir = "$InstallDir.backup.$(Get-Date -Format 'yyyyMMddHHmmss')"
        Invoke-Command -ComputerName $Device -ScriptBlock {
            param($source, $dest)
            if (Test-Path $source) {
                Rename-Item $source $dest -Force
            }
        } -ArgumentList $InstallDir, $backupDir -Credential $Credential
        
        # 3. 生成设备特定配置
        $tempConfigDir = Join-Path $env:TEMP "executor_config_$Device"
        if (-not (Test-Path $tempConfigDir)) {
            New-Item -ItemType Directory -Path $tempConfigDir -Force | Out-Null
        }
        
        if ($ConfigTemplate -and (Test-Path $ConfigTemplate)) {
            $configOutput = Join-Path $tempConfigDir "executor_config.json"
            New-DeviceConfig -Device $Device -TemplatePath $ConfigTemplate -OutputPath $configOutput
        }
        
        # 4. 复制文件
        Write-Log "复制文件..." "Info" $Device
        $sourcePath = if ($SourcePath) { $SourcePath } else { (Get-Location).Path }
        
        # 创建临时目录，排除不必要的文件
        $tempDir = Join-Path $env:TEMP "executor_deploy_$Device"
        if (Test-Path $tempDir) {
            Remove-Item $tempDir -Recurse -Force
        }
        Copy-Item $sourcePath $tempDir -Recurse -Exclude @("*.pyc", "__pycache__", ".git", "logs", "backup*")
        
        # 复制生成的配置
        if (Test-Path $configOutput) {
            Copy-Item $configOutput (Join-Path $tempDir "config\executor_config.json") -Force
        }
        
        # 复制到远程设备
        $copyResult = Copy-FilesToDevice -Device $Device -Source $tempDir -Destination $InstallDir
        if (-not $copyResult) {
            throw "文件复制失败"
        }
        
        # 5. 安装依赖
        Write-Log "安装依赖..." "Info" $Device
        $depResult = Invoke-Command -ComputerName $Device -ScriptBlock {
            param($installDir)
            Set-Location $installDir
            $requirements = Join-Path $installDir "requirements_production.txt"
            if (Test-Path $requirements) {
                python -m pip install -r $requirements -q 2>&1
            }
            return $LASTEXITCODE
        } -ArgumentList $InstallDir -Credential $Credential
        
        if ($depResult -ne 0) {
            Write-Log "依赖安装可能有问题，退出码: $depResult" "Warning" $Device
        }
        
        # 6. 安装服务
        Write-Log "安装服务..." "Info" $Device
        $installScript = Join-Path $InstallDir "deploy\install-service.ps1"
        $serviceResult = Invoke-Command -ComputerName $Device -ScriptBlock {
            param($script, $serviceName, $installDir)
            & $script -Action install -ServiceName $serviceName -InstallDir $installDir
            return $LASTEXITCODE
        } -ArgumentList $installScript, $ServiceName, $InstallDir -Credential $Credential
        
        if ($serviceResult -ne 0) {
            throw "服务安装失败"
        }
        
        # 7. 验证部署
        Write-Log "验证部署..." "Info" $Device
        Start-Sleep -Seconds 5
        $verifyResult = Invoke-Command -ComputerName $Device -ScriptBlock {
            param($serviceName)
            $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
            return $service -and $service.Status -eq "Running"
        } -ArgumentList $ServiceName -Credential $Credential
        
        if ($verifyResult) {
            Write-Log "部署成功" "Success" $Device
            return @{ Device = $Device; Status = "Success"; Reason = "" }
        }
        else {
            throw "服务验证失败"
        }
    }
    catch {
        Write-Log "部署失败: $_" "Error" $Device
        return @{ Device = $Device; Status = "Failed"; Reason = $_.Exception.Message }
    }
    finally {
        # 清理临时文件
        $tempPaths = @(
            (Join-Path $env:TEMP "executor_deploy_$Device"),
            (Join-Path $env:TEMP "executor_config_$Device")
        )
        $tempPaths | ForEach-Object {
            if (Test-Path $_) {
                Remove-Item $_ -Recurse -Force -ErrorAction SilentlyContinue
            }
        }
    }
}

# 更新设备
function Update-Device {
    param([string]$Device)
    
    Write-Log "开始更新..." "Info" $Device
    
    if (-not (Test-DeviceConnection $Device)) {
        Write-Log "设备无法连接" "Error" $Device
        return @{ Device = $Device; Status = "Failed"; Reason = "Connection failed" }
    }
    
    if ($WhatIf) {
        Write-Log "[WhatIf] 将执行更新操作" "Warning" $Device
        return @{ Device = $Device; Status = "WhatIf"; Reason = "Simulation mode" }
    }
    
    try {
        $updateScript = Join-Path $InstallDir "deploy\install-service.ps1"
        $result = Invoke-Command -ComputerName $Device -ScriptBlock {
            param($script, $serviceName)
            & $script -Action update -ServiceName $serviceName
            return $LASTEXITCODE
        } -ArgumentList $updateScript, $ServiceName -Credential $Credential
        
        if ($result -eq 0) {
            Write-Log "更新成功" "Success" $Device
            return @{ Device = $Device; Status = "Success"; Reason = "" }
        }
        else {
            throw "更新脚本返回错误码: $result"
        }
    }
    catch {
        Write-Log "更新失败: $_" "Error" $Device
        return @{ Device = $Device; Status = "Failed"; Reason = $_.Exception.Message }
    }
}

# 管理服务
function Manage-DeviceService {
    param(
        [string]$Device,
        [string]$ServiceAction
    )
    
    Write-Log "执行操作: $ServiceAction..." "Info" $Device
    
    if (-not (Test-DeviceConnection $Device)) {
        Write-Log "设备无法连接" "Error" $Device
        return @{ Device = $Device; Status = "Failed"; Reason = "Connection failed" }
    }
    
    try {
        $manageScript = Join-Path $InstallDir "deploy\install-service.ps1"
        $result = Invoke-Command -ComputerName $Device -ScriptBlock {
            param($script, $serviceName, $action)
            & $script -Action $action -ServiceName $serviceName
            return $LASTEXITCODE
        } -ArgumentList $manageScript, $ServiceName, $ServiceAction -Credential $Credential
        
        if ($result -eq 0) {
            Write-Log "操作成功" "Success" $Device
            return @{ Device = $Device; Status = "Success"; Reason = "" }
        }
        else {
            throw "操作返回错误码: $result"
        }
    }
    catch {
        Write-Log "操作失败: $_" "Error" $Device
        return @{ Device = $Device; Status = "Failed"; Reason = $_.Exception.Message }
    }
}

# 获取设备状态
function Get-DeviceStatus {
    param([string]$Device)
    
    Write-Log "获取状态..." "Info" $Device
    
    if (-not (Test-DeviceConnection $Device)) {
        return @{ Device = $Device; Status = "Offline"; ServiceStatus = "Unknown"; Version = "Unknown" }
    }
    
    try {
        $status = Invoke-Command -ComputerName $Device -ScriptBlock {
            param($serviceName, $installDir)
            
            $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
            $serviceStatus = if ($service) { $service.Status } else { "NotInstalled" }
            
            # 获取版本信息
            $versionFile = Join-Path $installDir "version.txt"
            $version = if (Test-Path $versionFile) { 
                Get-Content $versionFile -Raw 
            } else { 
                "Unknown" 
            }
            
            # 获取进程信息
            $process = Get-CimInstance Win32_Process | Where-Object { 
                $_.Name -like "python*" -and $_.CommandLine -like "*$installDir*" 
            } | Select-Object -First 1
            
            return @{
                ServiceStatus = $serviceStatus
                Version = $version
                ProcessId = if ($process) { $process.ProcessId } else { $null }
                MemoryMB = if ($process) { [math]::Round($process.WorkingSetSize / 1MB, 2) } else { $null }
            }
        } -ArgumentList $ServiceName, $InstallDir -Credential $Credential
        
        return @{
            Device = $Device
            Status = "Online"
            ServiceStatus = $status.ServiceStatus
            Version = $status.Version
            ProcessId = $status.ProcessId
            MemoryMB = $status.MemoryMB
        }
    }
    catch {
        Write-Log "获取状态失败: $_" "Error" $Device
        return @{ Device = $Device; Status = "Error"; ServiceStatus = "Unknown"; Version = "Unknown" }
    }
}

# 主函数
function Main {
    Write-Log "========================================" "Info"
    Write-Log "Python执行器批量部署工具" "Info"
    Write-Log "操作: $Action" "Info"
    Write-Log "========================================" "Info"
    
    # 读取设备列表
    $devices = Get-DeviceList
    
    if ($devices.Count -eq 0) {
        Write-Log "设备列表为空" "Error"
        exit 1
    }
    
    Write-Log "准备对 $($devices.Count) 个设备执行操作" "Info"
    
    if ($WhatIf) {
        Write-Log "[WhatIf模式] 不会实际执行操作" "Warning"
    }
    
    # 执行操作
    $results = @()
    $successCount = 0
    $failedCount = 0
    
    switch ($Action) {
        "deploy" {
            if (-not $SourcePath) {
                $SourcePath = Read-Host "请输入源代码路径 (留空使用当前目录)"
                if (-not $SourcePath) { $SourcePath = (Get-Location).Path }
            }
            
            Write-Log "开始批量部署..." "Info"
            $results = $devices | ForEach-Object -Parallel {
                # 注意：PowerShell 7+ 支持 -Parallel
                Deploy-ToDevice -Device $_
            } -ThrottleLimit $Parallel
        }
        
        "update" {
            Write-Log "开始批量更新..." "Info"
            foreach ($device in $devices) {
                $result = Update-Device -Device $device
                $results += $result
                
                if ($result.Status -eq "Success") { $successCount++ } else { $failedCount++ }
            }
        }
        
        { $_ -in @("start", "stop", "restart", "uninstall") } {
            Write-Log "开始批量管理服务..." "Info"
            foreach ($device in $devices) {
                $result = Manage-DeviceService -Device $device -ServiceAction $Action
                $results += $result
                
                if ($result.Status -eq "Success") { $successCount++ } else { $failedCount++ }
            }
        }
        
        "status" {
            Write-Log "开始批量获取状态..." "Info"
            foreach ($device in $devices) {
                $result = Get-DeviceStatus -Device $device
                $results += $result
                
                if ($result.Status -eq "Online") { $successCount++ } else { $failedCount++ }
            }
        }
    }
    
    # 输出结果汇总
    Write-Log "========================================" "Info"
    Write-Log "操作完成" "Info"
    Write-Log "成功: $successCount, 失败: $failedCount" $(if ($failedCount -eq 0) { "Success" } else { "Warning" })
    Write-Log "========================================" "Info"
    
    # 输出详细结果表格
    $results | Format-Table -AutoSize
    
    # 保存结果到文件
    $resultFile = "deploy_result_$(Get-Date -Format 'yyyyMMdd_HHmmss').json"
    $results | ConvertTo-Json -Depth 3 | Out-File $resultFile -Encoding UTF8
    Write-Log "结果已保存到: $resultFile" "Info"
    
    # 返回退出码
    exit $failedCount
}

# 执行主函数
Main
