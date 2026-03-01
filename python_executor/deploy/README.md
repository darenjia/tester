# Python执行器部署工具包

## 概述

本目录包含Python执行器的完整部署工具和文档，支持单节点部署和批量部署。

## 目录结构

```
deploy/
├── README.md                      # 本文件
├── DEPLOYMENT_ARCHITECTURE.md     # 部署架构文档
├── DEPLOYMENT_GUIDE.md            # 详细部署操作手册
├── install-service.ps1            # Windows服务安装脚本
├── deploy-batch.ps1               # 批量部署脚本
├── quick-start.bat                # 快速启动菜单（Windows）
├── .gitlab-ci.yml                 # GitLab CI/CD配置
├── config/
│   ├── config.template.json       # 配置模板
│   └── config.production.json     # 生产环境配置示例
└── devices.txt                    # 设备列表示例
```

## 快速开始

### 方式1：使用快速启动菜单（推荐）

1. 右键点击 `quick-start.bat`，选择"以管理员身份运行"
2. 按照菜单提示选择操作

### 方式2：命令行部署

#### 单节点部署

```powershell
# 以管理员身份运行PowerShell
cd deploy

# 安装服务
.\install-service.ps1 -Action install

# 查看状态
.\install-service.ps1 -Action status

# 其他操作
.\install-service.ps1 -Action start      # 启动
.\install-service.ps1 -Action stop       # 停止
.\install-service.ps1 -Action restart    # 重启
.\install-service.ps1 -Action update     # 更新
.\install-service.ps1 -Action uninstall  # 卸载
```

#### 批量部署

```powershell
# 1. 编辑设备列表
notepad devices.txt

# 2. 编辑配置模板
notepad config\config.template.json

# 3. 执行批量部署
.\deploy-batch.ps1 -DeviceList devices.txt -Action deploy

# 4. 批量查看状态
.\deploy-batch.ps1 -DeviceList devices.txt -Action status
```

## 部署流程

### 首次部署

1. **环境准备**
   - 确保目标设备满足系统要求
   - 安装Python 3.8+
   - 安装CANoe/TSMaster（如需要）

2. **配置准备**
   - 复制 `config/config.template.json` 为 `config/config.production.json`
   - 根据实际情况修改配置

3. **执行部署**
   - 单节点：使用 `install-service.ps1`
   - 多节点：使用 `deploy-batch.ps1`

4. **验证部署**
   - 检查服务状态
   - 测试WebSocket连接
   - 验证日志输出

### 升级部署

```powershell
# 单节点升级
.\install-service.ps1 -Action update

# 批量升级
.\deploy-batch.ps1 -DeviceList devices.txt -Action update
```

## 配置说明

### 关键配置项

| 配置项 | 说明 | 示例 |
|--------|------|------|
| `device_id` | 设备唯一标识 | `TEST-PC-001` |
| `websocket.port` | WebSocket服务端口 | `8180` |
| `platform.host` | 测试平台服务器地址 | `platform.company.com` |
| `logging.level` | 日志级别 | `INFO` |
| `canoe.enabled` | 启用CANoe支持 | `true` |
| `tsmaster.enabled` | 启用TSMaster支持 | `true` |

### 环境变量覆盖

可以通过环境变量覆盖配置文件中的设置：

```powershell
$env:EXECUTOR_WEBSOCKET_PORT = "9090"
$env:EXECUTOR_LOGGING_LEVEL = "DEBUG"
$env:EXECUTOR_PLATFORM_HOST = "new-platform.company.com"
```

## 故障排查

### 常见问题

#### 1. 服务无法启动

```powershell
# 查看详细错误
Get-WinEvent -FilterHashtable @{LogName='Application'; ID=1000} -MaxEvents 10

# 手动运行查看错误
python main_production.py
```

#### 2. 批量部署连接失败

```powershell
# 测试设备连通性
Test-NetConnection -ComputerName DEVICE-001 -Port 5985

# 检查WinRM服务
Invoke-Command -ComputerName DEVICE-001 { Get-Service WinRM }
```

#### 3. 配置文件错误

```powershell
# 验证JSON格式
python -c "import json; json.load(open('config/executor_config.json'))"
```

## 安全建议

1. **使用专用服务账户**
   - 创建低权限账户运行服务
   - 限制账户的本地和远程访问权限

2. **配置防火墙**
   - 仅开放必要的端口
   - 限制访问来源IP

3. **定期更新**
   - 及时更新Python和依赖库
   - 关注安全公告

4. **备份策略**
   - 定期备份配置文件
   - 保留多个版本备份

## CI/CD集成

### GitLab CI

已提供 `.gitlab-ci.yml` 配置，支持：
- 自动构建和测试
- 代码质量检查
- 多环境部署（开发/测试/生产）
- 自动回滚

### Jenkins集成

可以参考 `.gitlab-ci.yml` 创建Jenkins Pipeline。

## 监控和告警

### 内置监控

```powershell
# 查看性能指标
curl http://localhost:8180/metrics

# 健康检查
curl http://localhost:8180/health
```

### 集成外部监控

- **Prometheus**: 通过 `/metrics` 端点收集指标
- **Zabbix**: 使用健康检查API
- **Nagios**: 自定义检查脚本

## 支持和反馈

- **文档**: [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md)
- **架构**: [DEPLOYMENT_ARCHITECTURE.md](DEPLOYMENT_ARCHITECTURE.md)
- **问题反馈**: 联系技术团队

## 更新日志

### v2.0.0 (2024-01-15)
- 新增批量部署功能
- 支持配置热更新
- 集成熔断器和重试机制
- 完善监控和告警
- 新增CI/CD流水线配置

### v1.0.0 (2024-01-01)
- 初始版本
- 基础部署功能
- Windows服务支持
