# Python执行器部署操作手册

## 目录
1. [部署前准备](#1-部署前准备)
2. [单节点部署](#2-单节点部署)
3. [批量部署](#3-批量部署)
4. [配置管理](#4-配置管理)
5. [监控和运维](#5-监控和运维)
6. [故障处理](#6-故障处理)
7. [升级和回滚](#7-升级和回滚)

---

## 1. 部署前准备

### 1.1 环境检查清单

在部署前，请确保目标设备满足以下条件：

#### 系统要求
- [ ] Windows 10/11 或 Windows Server 2016+
- [ ] PowerShell 7.0 或更高版本
- [ ] Python 3.8 或更高版本
- [ ] 管理员权限

#### 网络要求
- [ ] 与测试平台服务器的网络连通
- [ ] WebSocket端口（默认8080）可访问
- [ ] 防火墙允许Python执行器通信

#### 软件依赖
- [ ] CANoe 11.0+ 或 TSMaster 2021+（根据实际需求）
- [ ] Visual C++ Redistributable 2015-2022
- [ ] Git（用于代码管理）

### 1.2 准备部署包

```powershell
# 1. 克隆或下载代码
git clone <repository-url> python_executor
cd python_executor

# 2. 创建版本文件
echo "2.0.0-$(Get-Date -Format 'yyyyMMdd')" > version.txt

# 3. 准备部署包
Compress-Archive -Path . -DestinationPath python_executor_v2.0.0.zip -Exclude @("*.pyc", "__pycache__", ".git", "logs", "backup*")
```

### 1.3 准备配置文件

复制配置模板并根据实际情况修改：

```powershell
copy deploy\config\config.template.json deploy\config\config.production.json
```

编辑 `config.production.json`，修改以下关键配置：

```json
{
    "platform": {
        "host": "your-platform-server.com",
        "port": 8080
    },
    "websocket": {
        "port": 8080
    }
}
```

### 1.4 准备设备列表

编辑 `deploy\devices.txt`，添加目标设备：

```
# 测试设备列表
TEST-PC-001
TEST-PC-002
192.168.1.100
192.168.1.101
```

---

## 2. 单节点部署

### 2.1 手动部署步骤

#### 步骤1：复制文件

```powershell
# 在目标设备上执行
$installDir = "C:\PythonExecutor"
New-Item -ItemType Directory -Path $installDir -Force

# 解压部署包
Expand-Archive -Path python_executor_v2.0.0.zip -DestinationPath $installDir
```

#### 步骤2：安装依赖

```powershell
cd $installDir
python -m pip install -r requirements_production.txt
```

#### 步骤3：配置服务

```powershell
# 复制配置文件
copy deploy\config\config.production.json config\executor_config.json

# 编辑配置（根据需要修改）
notepad config\executor_config.json
```

#### 步骤4：安装Windows服务

```powershell
# 以管理员身份运行PowerShell
cd $installDir
.\deploy\install-service.ps1 -Action install
```

#### 步骤5：验证部署

```powershell
# 检查服务状态
.\deploy\install-service.ps1 -Action status

# 查看日志
tail -f logs\executor.log

# 测试WebSocket连接（在另一台机器上）
curl http://<device-ip>:8080/health
```

### 2.2 使用安装脚本一键部署

```powershell
# 在目标设备上以管理员身份运行
$deployUrl = "http://your-deployment-server/python_executor_v2.0.0.zip"
$installDir = "C:\PythonExecutor"

# 下载部署包
Invoke-WebRequest -Uri $deployUrl -OutFile "$env:TEMP\python_executor.zip"

# 解压
Expand-Archive -Path "$env:TEMP\python_executor.zip" -DestinationPath $installDir -Force

# 安装服务
cd $installDir
.\deploy\install-service.ps1 -Action install -InstallDir $installDir

# 验证
.\deploy\install-service.ps1 -Action status
```

---

## 3. 批量部署

### 3.1 使用批量部署脚本

在管理服务器上执行：

```powershell
# 1. 准备设备列表
cd python_executor\deploy
notepad devices.txt  # 编辑设备列表

# 2. 准备配置模板
copy config\config.template.json config\config.production.json
notepad config\config.production.json  # 编辑配置

# 3. 执行批量部署（WhatIf模式预览）
.\deploy-batch.ps1 -DeviceList devices.txt -Action deploy -WhatIf

# 4. 正式部署
.\deploy-batch.ps1 -DeviceList devices.txt -Action deploy `
    -SourcePath "..\.." `
    -ConfigTemplate "config\config.production.json" `
    -Parallel 3
```

### 3.2 批量部署参数说明

| 参数 | 说明 | 示例 |
|------|------|------|
| `DeviceList` | 设备列表文件路径 | `devices.txt` |
| `Action` | 操作类型 | `deploy`, `update`, `status`, `start`, `stop`, `restart` |
| `SourcePath` | 源代码路径 | `C:\python_executor` |
| `ConfigTemplate` | 配置模板路径 | `config\production.json` |
| `Parallel` | 并行部署数量 | `3` |
| `InstallDir` | 远程安装目录 | `C:\PythonExecutor` |
| `ServiceName` | 服务名称 | `PythonExecutor` |
| `Credential` | 远程访问凭据 | `(Get-Credential)` |
| `WhatIf` | 预览模式 | `-WhatIf` |

### 3.3 使用凭据进行部署

如果需要指定访问凭据：

```powershell
# 方式1：交互式输入凭据
$cred = Get-Credential -Message "输入远程设备凭据"

.\deploy-batch.ps1 -DeviceList devices.txt -Action deploy -Credential $cred

# 方式2：使用保存的凭据（注意安全）
$username = "domain\admin"
$password = ConvertTo-SecureString "YourPassword" -AsPlainText -Force
$cred = New-Object System.Management.Automation.PSCredential($username, $password)

.\deploy-batch.ps1 -DeviceList devices.txt -Action deploy -Credential $cred
```

### 3.4 批量操作示例

```powershell
# 批量获取状态
.\deploy-batch.ps1 -DeviceList devices.txt -Action status

# 批量启动服务
.\deploy-batch.ps1 -DeviceList devices.txt -Action start

# 批量停止服务
.\deploy-batch.ps1 -DeviceList devices.txt -Action stop

# 批量重启服务
.\deploy-batch.ps1 -DeviceList devices.txt -Action restart

# 批量更新
.\deploy-batch.ps1 -DeviceList devices.txt -Action update

# 批量卸载
.\deploy-batch.ps1 -DeviceList devices.txt -Action uninstall
```

---

## 4. 配置管理

### 4.1 配置分层

配置按以下优先级加载（高优先级覆盖低优先级）：

1. **环境变量** - `EXECUTOR_*`
2. **本地配置文件** - `config/local.json`
3. **设备特定配置** - `config/device_{hostname}.json`
4. **全局配置** - `config/global.json`
5. **默认配置** - 代码内置

### 4.2 配置热更新

```powershell
# 修改配置文件后，发送热更新信号
# 方式1：HTTP接口
curl -X POST http://localhost:8080/config/reload

# 方式2：重启服务（会中断当前任务）
.\deploy\install-service.ps1 -Action restart
```

### 4.3 配置验证

```powershell
# 验证配置文件格式
python -c "import json; json.load(open('config/executor_config.json'))"

# 使用配置验证脚本（如果有）
python scripts\validate_config.py config\executor_config.json
```

---

## 5. 监控和运维

### 5.1 服务状态监控

```powershell
# 查看单个设备状态
.\deploy\install-service.ps1 -Action status

# 批量查看状态
.\deploy-batch.ps1 -DeviceList devices.txt -Action status
```

### 5.2 日志查看

```powershell
# 实时查看日志
tail -f logs\executor.log

# 查看最近100行
Get-Content logs\executor.log -Tail 100

# 搜索错误日志
Select-String -Path logs\executor.log -Pattern "ERROR" -Context 2,2

# 查看服务日志（NSSM生成）
tail -f logs\service.log
```

### 5.3 性能监控

```powershell
# 获取性能指标
curl http://localhost:8080/metrics | ConvertFrom-Json

# 查看CPU和内存使用
$metrics = curl http://localhost:8080/metrics | ConvertFrom-Json
$metrics.metrics.'system.cpu_percent'
$metrics.metrics.'system.memory_percent'
```

### 5.4 健康检查

```powershell
# HTTP健康检查
$response = curl http://localhost:8080/health | ConvertFrom-Json
$response.status
$response.clients
$response.current_task

# 自动化健康检查脚本
.\scripts\health-check.ps1 -DeviceList devices.txt
```

### 5.5 告警配置

编辑配置文件启用告警：

```json
{
    "performance": {
        "alert_enabled": true,
        "cpu_threshold": 80,
        "memory_threshold": 85,
        "disk_threshold": 90
    }
}
```

告警信息会输出到日志，可以通过日志收集系统（如ELK）进行集中告警。

---

## 6. 故障处理

### 6.1 服务无法启动

**现象**: 服务状态显示为 Stopped 或 StartPending

**排查步骤**:

```powershell
# 1. 查看服务日志
Get-Content logs\service.log -Tail 50

# 2. 检查Python环境
python --version
python -c "import flask; import socketio; print('Dependencies OK')"

# 3. 检查端口占用
Get-NetTCPConnection -LocalPort 8080

# 4. 手动运行测试
python main_production.py
# 观察控制台输出

# 5. 检查配置文件
python -c "import json; json.load(open('config/executor_config.json'))"
```

**常见原因**:
- Python依赖未安装或版本不匹配
- 配置文件格式错误
- 端口被其他程序占用
- 权限不足

### 6.2 连接测试平台失败

**现象**: 日志显示 WebSocket 连接失败

**排查步骤**:

```powershell
# 1. 测试网络连通
Test-NetConnection -ComputerName platform-server -Port 8080

# 2. 检查防火墙
Get-NetFirewallRule | Where-Object { $_.DisplayName -like "*Python*" }

# 3. 检查配置
cat config\executor_config.json | grep platform

# 4. 测试WebSocket
curl http://platform-server:8080/api/health
```

### 6.3 CANoe/TSMaster连接失败

**现象**: 无法连接到测试工具

**排查步骤**:

```powershell
# 1. 检查软件运行状态
Get-Process | Where-Object { $_.ProcessName -match "CANoe|TSMaster" }

# 2. 检查COM接口注册（CANoe）
reg query "HKCR\CANoe.Application"

# 3. 检查DLL文件（TSMaster）
Test-Path "C:\Program Files\TOSUN\TSMaster\lib\TSMaster.dll"

# 4. 权限检查
# 确保服务运行账户有权限访问测试软件
```

### 6.4 性能问题

**现象**: CPU或内存使用率过高

**排查步骤**:

```powershell
# 1. 查看性能指标
curl http://localhost:8080/metrics | ConvertFrom-Json

# 2. 查看进程信息
Get-Process python | Select-Object Name, Id, CPU, WorkingSet

# 3. 查看任务状态
curl http://localhost:8080/health | ConvertFrom-Json

# 4. 检查日志中的性能告警
grep "性能告警" logs\executor.log
```

### 6.5 日志轮转问题

**现象**: 日志文件过大，占用磁盘空间

**解决方案**:

```powershell
# 1. 手动清理旧日志
Get-ChildItem logs\*.log | Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-7) } | Remove-Item

# 2. 调整日志配置
# 编辑 config\executor_config.json
{
    "logging": {
        "max_size": 10485760,  # 10MB
        "backup_count": 5      # 保留5个备份
    }
}

# 3. 配置自动清理（Windows任务计划程序）
# 创建定时任务，每天清理7天前的日志
```

---

## 7. 升级和回滚

### 7.1 升级流程

#### 单节点升级

```powershell
# 1. 备份当前版本
$backupDir = "backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item C:\PythonExecutor $backupDir -Recurse -Exclude @("logs", "backup*")

# 2. 使用升级脚本
cd C:\PythonExecutor
.\deploy\install-service.ps1 -Action update

# 3. 验证升级
.\deploy\install-service.ps1 -Action status
curl http://localhost:8080/health
```

#### 批量升级

```powershell
cd python_executor\deploy
.\deploy-batch.ps1 -DeviceList devices.txt -Action update
```

### 7.2 回滚流程

#### 单节点回滚

```powershell
# 1. 停止服务
.\deploy\install-service.ps1 -Action stop

# 2. 恢复备份
$backupDir = "backup_20240115_120000"  # 指定备份目录
Remove-Item C:\PythonExecutor -Recurse -Force
Rename-Item $backupDir C:\PythonExecutor

# 3. 启动服务
.\deploy\install-service.ps1 -Action start

# 4. 验证
.\deploy\install-service.ps1 -Action status
```

#### 自动回滚脚本

```powershell
# rollback.ps1
param(
    [string]$BackupPath,
    [string]$InstallDir = "C:\PythonExecutor"
)

# 停止服务
Stop-Service PythonExecutor

# 备份当前（可能损坏的）版本
$corruptedDir = "$InstallDir.corrupted.$(Get-Date -Format 'yyyyMMddHHmmss')"
Move-Item $InstallDir $corruptedDir

# 恢复备份
Copy-Item $BackupPath $InstallDir -Recurse

# 启动服务
Start-Service PythonExecutor

# 验证
$service = Get-Service PythonExecutor
if ($service.Status -eq "Running") {
    Write-Host "回滚成功" -ForegroundColor Green
} else {
    Write-Host "回滚失败，请手动检查" -ForegroundColor Red
}
```

### 7.3 版本管理

建议维护版本清单：

```powershell
# versions.txt
# 格式: 设备名,当前版本,部署时间,状态
TEST-PC-001,2.0.0,2024-01-15,Active
TEST-PC-002,2.0.0,2024-01-15,Active
TEST-PC-003,1.9.0,2024-01-10,PendingUpdate
```

---

## 8. 附录

### 8.1 常用命令速查

```powershell
# 服务管理
.\deploy\install-service.ps1 -Action install    # 安装
.\deploy\install-service.ps1 -Action uninstall  # 卸载
.\deploy\install-service.ps1 -Action start      # 启动
.\deploy\install-service.ps1 -Action stop       # 停止
.\deploy\install-service.ps1 -Action restart    # 重启
.\deploy\install-service.ps1 -Action status     # 状态

# 批量操作
.\deploy-batch.ps1 -DeviceList devices.txt -Action deploy   # 部署
.\deploy-batch.ps1 -DeviceList devices.txt -Action update   # 更新
.\deploy-batch.ps1 -DeviceList devices.txt -Action status   # 状态

# Windows服务命令（备用）
sc query PythonExecutor          # 查询状态
sc start PythonExecutor          # 启动服务
sc stop PythonExecutor           # 停止服务
sc delete PythonExecutor         # 删除服务
```

### 8.2 配置文件示例

```json
{
    "device_id": "TEST-PC-001",
    "device_name": "测试设备001",
    "environment": "production",
    
    "websocket": {
        "host": "0.0.0.0",
        "port": 8080
    },
    
    "platform": {
        "host": "platform.company.com",
        "port": 8080
    },
    
    "logging": {
        "level": "INFO",
        "file": "logs/executor.log"
    },
    
    "canoe": {
        "enabled": true,
        "timeout": 30
    },
    
    "tsmaster": {
        "enabled": true,
        "timeout": 30
    }
}
```

### 8.3 联系支持

遇到问题时的支持渠道：
- **技术支持邮箱**: support@company.com
- **紧急热线**: +86-xxx-xxxx-xxxx
- **内部Wiki**: https://wiki.company.com/python-executor
- **问题跟踪**: https://jira.company.com/projects/EXEC

---

**文档版本**: 2.0.0  
**最后更新**: 2024-01-15  
**作者**: 技术团队
