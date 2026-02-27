# Python执行器生产环境部署指南

## 概述

本文档描述了Python执行器生产环境的部署流程和最佳实践。

## 生产环境特性

### 1. 熔断器保护 (Circuit Breaker)
- **功能**: 防止级联故障，当错误率达到阈值时自动断开
- **配置**: `failure_threshold=10`, `recovery_timeout=300s`
- **作用**: 保护CANoe/TSMaster连接稳定性

### 2. 自动重试机制
- **功能**: 自动重试失败的操作
- **配置**: 最大重试3次，指数退避
- **适用场景**: 网络波动、COM接口临时故障

### 3. 配置热更新
- **功能**: 无需重启服务即可更新配置
- **监控间隔**: 5秒检查一次配置文件变更
- **支持配置**: WebSocket端口、日志级别、超时时间等

### 4. 性能监控
- **功能**: 实时监控系统资源使用情况
- **指标**: CPU、内存、磁盘、网络IO
- **告警**: 可设置阈值自动告警

### 5. 输入验证
- **功能**: 严格验证所有输入数据
- **验证内容**: 任务ID、设备ID、文件路径、信号名称等
- **安全**: 防止路径遍历和注入攻击

## 环境要求

### 系统要求
- **操作系统**: Windows 10/11 或 Windows Server 2016+
- **Python**: 3.8+
- **内存**: 最少4GB，推荐8GB+
- **磁盘**: 最少10GB可用空间

### 依赖软件
- **CANoe**: 11.0+ (如使用CANoe)
- **TSMaster**: 2021+ (如使用TSMaster)
- **Visual C++ Redistributable**: 2015-2022

### Python依赖
```bash
pip install -r requirements.txt
pip install psutil  # 性能监控必需
```

## 部署步骤

### 1. 准备环境

```powershell
# 创建虚拟环境
python -m venv venv

# 激活虚拟环境
.\venv\Scripts\activate

# 安装依赖
pip install -r requirements.txt
pip install psutil
```

### 2. 配置文件

创建 `config/executor_config.json`:

```json
{
    "websocket": {
        "host": "0.0.0.0",
        "port": 8080,
        "heartbeat_interval": 30,
        "reconnect_interval": 5,
        "max_reconnect_attempts": 3
    },
    "logging": {
        "level": "INFO",
        "file": "logs/executor.log",
        "max_size": 10485760,
        "backup_count": 10
    },
    "task": {
        "timeout": 3600,
        "check_interval": 1,
        "max_concurrent": 1
    },
    "canoe": {
        "timeout": 30,
        "config_timeout": 60,
        "max_retries": 3,
        "retry_delay": 2.0
    },
    "tsmaster": {
        "timeout": 30,
        "config_timeout": 60,
        "max_retries": 3,
        "retry_delay": 2.0
    },
    "performance": {
        "monitor_enabled": true,
        "monitor_interval": 60,
        "metrics_retention_hours": 24
    },
    "security": {
        "validate_inputs": true,
        "max_config_file_size_mb": 100,
        "allowed_config_extensions": [".cfg", ".xml", ".json", ".dbc", ".ldf"]
    }
}
```

### 3. 创建目录结构

```powershell
mkdir logs
mkdir config
mkdir data
```

### 4. 启动服务

#### 开发模式
```powershell
python main_production.py
```

#### 生产模式（使用Waitress）
```powershell
# 安装Waitress
pip install waitress

# 启动
waitress-serve --host=0.0.0.0 --port=8080 main_production:app
```

#### Windows服务（使用NSSM）
```powershell
# 1. 下载并安装NSSM
# 2. 创建服务
nssm install PythonExecutor

# 3. 配置服务
# Path: C:\Python39\python.exe
# Startup directory: D:\Deng\can_test\python_executor
# Arguments: main_production.py

# 4. 启动服务
nssm start PythonExecutor
```

## 监控和运维

### 健康检查

```bash
# HTTP健康检查
curl http://localhost:8080/health

# 预期响应
{
    "status": "healthy",
    "timestamp": "2024-01-15T10:30:00",
    "clients": 2,
    "current_task": {...},
    "config_valid": true
}
```

### 性能指标

```bash
# 获取性能指标
curl http://localhost:8080/metrics

# 预期响应
{
    "timestamp": "2024-01-15T10:30:00",
    "metrics": {
        "system.cpu_percent": {...},
        "system.memory_percent": {...},
        "task.total_duration": {...}
    }
}
```

### 日志监控

```powershell
# 实时查看日志
tail -f logs/executor.log

# 查看错误日志
grep ERROR logs/executor.log

# 查看性能告警
grep "性能告警" logs/executor.log
```

### 配置热更新

```bash
# 手动重新加载配置
curl -X POST http://localhost:8080/config/reload
```

## 故障排查

### 常见问题

#### 1. CANoe连接失败
**症状**: 无法连接到CANoe
**排查**:
```powershell
# 检查CANoe是否运行
Get-Process CANoe*

# 检查COM接口注册
reg query "HKCR\CANoe.Application"

# 检查日志
grep "CANoe连接失败" logs/executor.log
```

#### 2. 熔断器触发
**症状**: 任务执行被熔断器阻止
**排查**:
```powershell
# 查看熔断器状态
grep "熔断器" logs/executor.log

# 查看错误统计
grep "record_failure" logs/executor.log
```

#### 3. 内存泄漏
**症状**: 内存使用持续增长
**排查**:
```bash
# 查看内存指标
curl http://localhost:8080/metrics | grep memory

# 检查任务完成情况
grep "任务执行完成" logs/executor.log
```

### 日志级别

修改配置文件调整日志级别:
```json
{
    "logging": {
        "level": "DEBUG"  // DEBUG, INFO, WARNING, ERROR
    }
}
```

## 性能优化

### 1. 调整并发设置
```json
{
    "task": {
        "max_concurrent": 1  // 根据硬件资源调整
    }
}
```

### 2. 优化超时设置
```json
{
    "canoe": {
        "timeout": 30,  // 根据实际网络情况调整
        "retry_delay": 2.0
    }
}
```

### 3. 日志轮转
```json
{
    "logging": {
        "max_size": 10485760,  // 10MB
        "backup_count": 10
    }
}
```

## 安全建议

### 1. 网络安全
- 使用防火墙限制访问IP
- 启用SSL/TLS加密通信
- 定期更新依赖库

### 2. 文件安全
- 配置文件只允许特定扩展名
- 限制配置文件大小
- 验证所有文件路径

### 3. 访问控制
- 实现客户端认证
- 记录所有操作日志
- 定期审计访问记录

## 备份和恢复

### 配置文件备份
```powershell
# 备份配置
Copy-Item config/executor_config.json config/executor_config.json.backup

# 恢复配置
Copy-Item config/executor_config.json.backup config/executor_config.json
```

### 日志备份
```powershell
# 压缩日志
Compress-Archive -Path logs/*.log -DestinationPath logs_backup.zip
```

## 升级指南

### 1. 备份当前版本
```powershell
# 备份整个目录
Compress-Archive -Path python_executor -DestinationPath python_executor_backup.zip
```

### 2. 更新代码
```powershell
# 拉取最新代码
git pull origin main

# 或手动更新文件
```

### 3. 更新依赖
```powershell
pip install -r requirements.txt --upgrade
```

### 4. 重启服务
```powershell
nssm restart PythonExecutor
```

## 联系支持

如有问题，请联系:
- 技术支持: support@example.com
- 紧急热线: +86-xxx-xxxx-xxxx
