# Python执行器Docker部署方案

## 1. 概述

本文档详细介绍Python测试执行器的Docker容器化部署方案。

### 1.1 项目背景

Python执行器是一个用于自动化测试的核心组件，支持：
- WebSocket实时通信
- 任务调度和执行
- CANoe/TSMaster/TTworkbench测试工具集成
- 分布式部署

### 1.2 部署挑战

由于项目特性，存在以下挑战：

| 挑战 | 说明 | 解决方案 |
|------|------|----------|
| Windows专有依赖 | pywin32, CANoe, TSMaster仅支持Windows | Windows容器 + 硬件直通 |
| 硬件访问 | CAN卡等硬件设备需要直接访问 | 设备直通/共享 |
| COM组件 | CANoe COM接口需要Windows环境 | Windows容器 |

### 1.3 部署模式选择

考虑到测试工具的依赖，我们提供两种部署模式：

```
┌─────────────────────────────────────────────────────────────┐
│                    部署模式选择                              │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  模式一：Windows容器部署（推荐）                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  适用场景：生产环境、需要CANoe/TSMaster集成           │   │
│  │  优点：完整支持所有功能                              │   │
│  │  缺点：需要Windows Server容器主机                    │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                              │
│  模式二：Linux容器部署（开发/测试）                          │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  适用场景：开发测试、无硬件依赖的功能验证             │   │
│  │  优点：部署简单、资源占用低                          │   │
│  │  缺点：无法使用CANoe/TSMaster                        │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. 架构设计

### 2.1 Windows容器部署架构

```
┌────────────────────────────────────────────────────────────────┐
│                     Windows Server主机                           │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                    Docker Desktop / ContainerD            │  │
│  │                                                            │  │
│  │  ┌─────────────────────────────────────────────────────┐  │  │
│  │  │            Python执行器容器                          │  │  │
│  │  │  ┌─────────┐  ┌─────────┐  ┌─────────────────────┐ │  │  │
│  │  │  │  Flask  │  │WebSocket│  │   任务执行器        │ │  │  │
│  │  │  │   API   │  │  Server │  │                     │ │  │  │
│  │  │  └────┬────┘  └────┬────┘  └──────────┬────────┘ │  │  │
│  │  │       │             │                    │          │  │  │
│  │  │       └─────────────┼────────────────────┘          │  │  │
│  │  │                     │                               │  │  │
│  │  │              ┌──────▼──────┐                        │  │  │
│  │  │              │ CANoe/TSMaster│                       │  │  │
│  │  │              │   控制器     │                        │  │  │
│  │  │              └──────┬──────┘                        │  │  │
│  │  └─────────────────────┼─────────────────────────────┘  │  │
│  │                        │                                    │  │
│  │                   ┌────▼────┐                              │  │
│  │                   │ CAN卡/  │                              │  │
│  │                   │ 硬件设备 │                              │  │
│  │                   └─────────┘                              │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
└────────────────────────────────────────────────────────────────┘
```

### 2.2 分布式部署架构

```
┌─────────────────────────────────────────────────────────────────┐
│                         测试平台 (Java服务)                        │
│                    (任务调度、结果收集、监控)                       │
└────────────────────────────┬────────────────────────────────────┘
                             │
                    WebSocket / HTTP
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│  执行器1    │      │  执行器2    │      │  执行器N    │
│ (Windows)   │      │ (Windows)   │      │ (Windows)   │
│ CANoe       │      │ TSMaster    │      │ CANoe       │
└─────────────┘      └─────────────┘      └─────────────┘
    │                     │                    │
    ▼                     ▼                    ▼
┌─────────┐         ┌─────────┐          ┌─────────┐
│ CAN硬件 │         │ CAN硬件 │          │ CAN硬件 │
└─────────┘         └─────────┘          └─────────┘
```

### 2.3 网络架构

```
┌─────────────────────────────────────────────────────────────────┐
│                          网络拓扑                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  内部网络 (Container Network)                                    │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  python-executor-network                                │   │
│  │                                                          │   │
│  │   ┌──────────────┐                                      │   │
│  │   │   Redis     │  (可选：任务队列)                      │   │
│  │   │   :6379     │                                      │   │
│  │   └──────┬───────┘                                      │   │
│  │          │                                               │   │
│  │   ┌──────▼───────┐    ┌──────────────────┐             │   │
│  │   │  Executor    │◄───│  Test Platform    │             │   │
│  │   │  :5000       │    │  (Java Service)   │             │   │
│  │   │  :5001(WS)   │    │                   │             │   │
│  │   └──────────────┘    └──────────────────┘             │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                  │
│  端口映射：                                                     │
│  - 5000: HTTP API                                              │
│  - 5001: WebSocket                                             │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 3. Windows容器部署（推荐）

### 3.1 环境要求

| 项目 | 最低要求 | 推荐配置 |
|------|----------|----------|
| 操作系统 | Windows Server 2019+ / Windows 10/11 | Windows Server 2022 |
| Docker | Docker Desktop 4.0+ | Docker Desktop最新版本 |
| 内存 | 8GB | 16GB |
| CPU | 4核 | 8核 |
| 存储 | 50GB | 100GB SSD |
| CANoe/TSMaster | 已安装（需要授权） | 已安装最新版本 |

### 3.2 目录结构

```
python_executor/
├── docker/
│   ├── windows/
│   │   ├── Dockerfile
│   │   ├── Dockerfile.dev
│   │   └── entrypoint.ps1
│   ├── config/
│   │   ├── config.json
│   │   ├── logging.json
│   │   └── devices.json
│   ├── .env
│   ├── docker-compose.yml
│   └── README.md
```

### 3.3 Dockerfile

```dockerfile
# Windows容器镜像
# 基础镜像：Windows Server Core with Python
FROM mcr.microsoft.com/windows/servercore:ltsc2022

# 设置工作目录
WORKDIR C:/app

# 设置环境变量
ENV PYTHONUNBUFFERED=1 `
    PYTHONDONTWRITEBYTECODE=1 `
    POWERSHELL_TELEMETRY_OPTOUT=1

# 安装 Chocolatey
RUN powershell -Command \
    Set-ExecutionPolicy Bypass -Scope Process -Force; \
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; \
    iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

# 安装 Python
RUN choco install python --version=3.10.11 -y

# 刷新环境变量并安装依赖
RUN powershell -Command \
    refreshenv; \
    python -m pip install --upgrade pip; \
    pip install flask==2.3.0 flask-socketio==5.3.0 python-socketio==5.9.0

# 复制项目文件
COPY requirements.txt C:/app/
RUN pip install -r requirements.txt

COPY . C:/app/

# 创建日志目录
RUN powershell -New-Item -ItemType Directory -Force -Path C:/app/logs

# 暴露端口
EXPOSE 5000 5001

# 设置入口点
ENTRYPOINT ["powershell", "C:/app/docker/windows/entrypoint.ps1"]
```

### 3.4 entrypoint.ps1

```powershell
#!/usr/bin/env pwsh
# Python执行器启动脚本

param(
    [string]$ConfigPath = "C:/app/docker/config/config.json",
    [string]$LogLevel = "INFO",
    [switch]$Debug = $false
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Python执行器启动中..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 检查配置文件
if (-not (Test-Path $ConfigPath)) {
    Write-Warning "配置文件不存在: $ConfigPath"
    Write-Host "使用默认配置..."
}

# 检查CANoe/TSMaster
$canoePath = "C:\Program Files\CANoe"
$tsmasterPath = "C:\Program Files\TSMaster"

if (Test-Path $canoePath) {
    Write-Host "[OK] CANoe已安装" -ForegroundColor Green
} else {
    Write-Warning "CANoe未安装"
}

if (Test-Path $tsmasterPath) {
    Write-Host "[OK] TSMaster已安装" -ForegroundColor Green
} else {
    Write-Warning "TSMaster未安装"
}

# 设置环境变量
$env:PYTHONEXECUTOR_CONFIG = $ConfigPath
$env:PYTHONEXECUTOR_LOGLEVEL = $LogLevel
if ($Debug) {
    $env:FLASK_DEBUG = "1"
}

Write-Host ""
Write-Host "启动配置:" -ForegroundColor Yellow
Write-Host "  配置文件: $ConfigPath"
Write-Host "  日志级别: $LogLevel"
Write-Host "  调试模式: $Debug"
Write-Host ""

# 启动应用
Write-Host "启动Flask应用..." -ForegroundColor Cyan
python C:/app/main.py

# 如果退出，记录日志
Write-Host "应用已退出" -ForegroundColor Red
exit 1
```

### 3.5 docker-compose.yml

```yaml
version: '3.8'

services:
  python-executor:
    build:
      context: ../..
      dockerfile: docker/windows/Dockerfile
    image: python-executor:latest
    container_name: python-executor
    hostname: python-executor
    
    environment:
      - CONFIG_PATH=C:/app/docker/config/config.json
      - LOG_LEVEL=INFO
      - PLATFORM_HOST=platform-api:8080
      - EXECUTOR_ID=${EXECUTOR_ID:-executor-01}
      
    volumes:
      # 配置文件
      - ./config:/app/docker/config:ro
      
      # 日志目录
      - executor_logs:/app/logs
      
      # CANoe安装目录（只读）
      - type: bind
        source: C:/Program Files/CANoe
        target: C:/Program Files/CANoe
        read_only: true
      
      # TSMaster安装目录（只读）
      - type: bind
        target: C:/Program Files/TSMaster
        source: C:/Program Files/TSMaster
        read_only: true
      
      # CAN硬件设备
      - type: bind
        source: //./pipe/CAN
        target: //./pipe/CAN

    ports:
      - "5000:5000"  # HTTP API
      - "5001:5001"  # WebSocket

    networks:
      - executor-network

    restart: unless-stopped

    healthcheck:
      test: ["CMD", "powershell", "-Command", "Invoke-WebRequest -Uri http://localhost:5000/health -UseBasicParsing"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 60s

    # Windows特定配置
    security_opt:
      - credentialspec=credentialspec.json
      
    # 资源限制
    deploy:
      resources:
        limits:
          cpus: '4'
          memory: 8G
        reservations:
          cpus: '2'
          memory: 4G

networks:
  executor-network:
    driver: nat
    ipam:
      config:
        - subnet: 172.20.0.0/24

volumes:
  executor_logs:
```

### 3.6 配置文件

#### config.json

```json
{
  "executor": {
    "id": "executor-01",
    "name": "执行器1",
    "platform": "windows",
    "max_concurrent_tasks": 1
  },
  "platform": {
    "host": "platform-api",
    "port": 8080,
    "ws_endpoint": "ws://platform-api:8080/ws/executor"
  },
  "adapters": {
    "canoe": {
      "enabled": true,
      "com_port": 0,
      "startup_timeout": 30
    },
    "tsmaster": {
      "enabled": true,
      "use_rpc": true,
      "fallback_to_traditional": true
    },
    "ttworkbench": {
      "enabled": false,
      "ttman_path": "C:/Spirent/TTman.bat"
    }
  },
  "logging": {
    "level": "INFO",
    "file": "logs/executor.log",
    "max_size": "100MB",
    "backup_count": 5
  },
  "network": {
    "api_port": 5000,
    "ws_port": 5001,
    "cors_enabled": true
  }
}
```

#### .env

```bash
# 执行器配置
EXECUTOR_ID=executor-01
EXECUTOR_NAME=执行器1

# 平台配置
PLATFORM_API_HOST=platform-api
PLATFORM_API_PORT=8080

# 日志配置
LOG_LEVEL=INFO

# CANoe配置
CANOE_ENABLED=true
CANOE_PATH=C:/Program Files/CANoe

# TSMaster配置
TSMASTER_ENABLED=true
TSMASTER_USE_RPC=true
```

---

## 4. Linux容器部署（开发/测试）

### 4.1 适用场景

- 开发环境
- 单元测试
- 功能验证
- CI/CD流水线

### 4.2 目录结构

```
python_executor/
├── docker/
│   ├── linux/
│   │   ├── Dockerfile
│   │   ├── Dockerfile.alpine
│   │   └── entrypoint.sh
│   ├── config/
│   ├── .env
│   └── docker-compose.linux.yml
```

### 4.3 Dockerfile

```dockerfile
# Linux容器镜像
FROM python:3.10-slim

# 设置工作目录
WORKDIR /app

# 设置环境变量
ENV PYTHONUNBUFFERED=1 \
    PYTHONDONTWRITEBYTECODE=1 \
    DEBIAN_FRONTEND=noninteractive

# 安装系统依赖
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    curl \
    git \
    && rm -rf /var/lib/apt/lists/*

# 安装Python依赖
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# 复制项目文件
COPY . .

# 创建非root用户
RUN useradd -m -s /bin/bash executor && \
    chown -R executor:executor /app

# 切换到非root用户
USER executor

# 暴露端口
EXPOSE 5000 5001

# 健康检查
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# 入口点
ENTRYPOINT ["/app/docker/linux/entrypoint.sh"]
```

### 4.4 docker-compose.linux.yml

```yaml
version: '3.8'

services:
  python-executor:
    build:
      context: ../..
      dockerfile: docker/linux/Dockerfile
    image: python-executor:linux
    container_name: python-executor-linux
    
    environment:
      - CONFIG_PATH=/app/config/config.json
      - LOG_LEVEL=INFO
      - PLATFORM_HOST=host.docker.internal:8080
      - EXECUTOR_ID=executor-linux-01
      - CANOE_ENABLED=false
      - TSMASTER_ENABLED=false
      
    volumes:
      - ./config:/app/config:ro
      - executor_logs:/app/logs

    ports:
      - "5000:5000"
      - "5001:5001"

    networks:
      - executor-network

    restart: unless-stopped

networks:
  executor-network:
    driver: bridge

volumes:
  executor_logs:
```

### 4.5 entrypoint.sh

```bash
#!/bin/bash
set -e

echo "========================================"
echo "  Python执行器启动中..."
echo "========================================"

# 检查配置
if [ ! -f "$CONFIG_PATH" ]; then
    echo "警告: 配置文件不存在: $CONFIG_PATH"
    echo "使用默认配置..."
fi

# 设置环境变量
export PYTHONEXECUTOR_CONFIG=${CONFIG_PATH:-/app/config/config.json}
export PYTHONEXECUTOR_LOGLEVEL=${LOG_LEVEL:-INFO}

echo ""
echo "启动配置:"
echo "  配置文件: $PYTHONEXECUTOR_CONFIG"
echo "  日志级别: $PYTHONEXECUTOR_LOGLEVEL"
echo "  CANoe启用: $CANOE_ENABLED"
echo "  TSMaster启用: $TSMASTER_ENABLED"
echo ""

# 启动应用
exec python /app/main.py
```

---

## 5. 部署步骤

### 5.1 Windows容器部署

#### 步骤1：准备环境

```powershell
# 1. 安装Docker Desktop for Windows
# 下载地址: https://www.docker.com/products/docker-desktop

# 2. 启用Windows容器
# Docker Desktop -> Settings -> Containers -> Enable Windows containers

# 3. 验证Docker安装
docker --version
docker-compose --version
```

#### 步骤2：配置项目

```powershell
# 1. 进入部署目录
cd python_executor/docker

# 2. 复制配置文件
cp config/config.json config/config.json.bak

# 3. 编辑配置文件
notepad config/config.json
```

#### 步骤3：构建镜像

```powershell
# 1. 构建镜像
docker build -f windows/Dockerfile -t python-executor:latest ../..

# 2. 查看镜像
docker images python-executor
```

#### 步骤4：运行容器

```powershell
# 1. 运行容器
docker-compose up -d

# 2. 查看运行状态
docker-compose ps

# 3. 查看日志
docker-compose logs -f
```

#### 步骤5：验证部署

```powershell
# 1. 检查健康状态
Invoke-WebRequest -Uri http://localhost:5000/health

# 2. 检查API
Invoke-WebRequest -Uri http://localhost:5000/

# 3. 检查WebSocket连接
# 使用浏览器或工具连接 ws://localhost:5001
```

### 5.2 Linux容器部署

```bash
# 1. 进入部署目录
cd python_executor/docker

# 2. 复制配置文件
cp config/config.json config/config.json.bak

# 3. 编辑配置文件
vim config/config.json

# 4. 构建镜像
docker build -f linux/Dockerfile -t python-executor:linux ../..

# 5. 运行容器
docker-compose -f docker-compose.linux.yml up -d

# 6. 查看日志
docker-compose -f docker-compose.linux.yml logs -f
```

---

## 6. 配置说明

### 6.1 执行器配置

```json
{
  "executor": {
    "id": "executor-01",
    "name": "执行器1",
    "platform": "windows",
    "max_concurrent_tasks": 1,
    "device_id": "DEVICE-001"
  }
}
```

### 6.2 平台配置

```json
{
  "platform": {
    "host": "platform-api",
    "port": 8080,
    "ws_endpoint": "ws://platform-api:8080/ws/executor",
    "api_key": "your-api-key-here",
    "retry_count": 3,
    "retry_delay": 5
  }
}
```

### 6.3 适配器配置

```json
{
  "adapters": {
    "canoe": {
      "enabled": true,
      "com_port": 0,
      "startup_timeout": 30,
      "license_server": "5053@localhost"
    },
    "tsmaster": {
      "enabled": true,
      "use_rpc": true,
      "rpc_app_name": null,
      "fallback_to_traditional": true
    },
    "ttworkbench": {
      "enabled": false,
      "ttman_path": "C:/Spirent/TTman.bat",
      "workspace_path": "C:/Workspace"
    }
  }
}
```

---

## 7. 监控和维护

### 7.1 健康检查

```powershell
# HTTP健康检查
Invoke-WebRequest -Uri http://localhost:5000/health

# 响应示例
{
  "status": "healthy",
  "timestamp": "2026-02-27T10:00:00Z",
  "clients": 0,
  "current_task": null
}
```

### 7.2 日志管理

```powershell
# 查看实时日志
docker logs -f python-executor

# 查看最近100行
docker logs --tail 100 python-executor

# 导出日志
docker logs python-executor > executor.log
```

### 7.3 性能监控

```powershell
# 查看资源使用
docker stats python-executor

# 容器信息
docker inspect python-executor
```

---

## 8. 故障排除

### 8.1 常见问题

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| 容器启动失败 | 配置文件缺失 | 检查配置文件路径 |
| CANoe连接失败 | CANoe未安装或版本不匹配 | 验证CANoe安装 |
| 端口冲突 | 5000/5001端口被占用 | 修改端口映射 |
| 内存不足 | 容器内存限制过低 | 调整资源限制 |

### 8.2 日志分析

```powershell
# 查看错误日志
docker logs python-executor 2>&1 | Select-String -Pattern "ERROR"

# 查看异常堆栈
docker logs python-executor 2>&1 | Select-String -Pattern "Exception|Traceback"
```

---

## 9. 附录

### 9.1 端口说明

| 端口 | 协议 | 说明 |
|------|------|------|
| 5000 | HTTP | REST API |
| 5001 | WebSocket | 实时通信 |

### 9.2 环境变量

| 变量名 | 说明 | 默认值 |
|--------|------|--------|
| CONFIG_PATH | 配置文件路径 | C:/app/config/config.json |
| LOG_LEVEL | 日志级别 | INFO |
| PLATFORM_HOST | 平台API地址 | platform-api |
| EXECUTOR_ID | 执行器ID | executor-01 |
| CANOE_ENABLED | 启用CANoe | true |
| TSMASTER_ENABLED | 启用TSMaster | true |

### 9.3 目录映射

| 容器内路径 | 主机路径 | 说明 |
|------------|----------|------|
| C:/app | 项目根目录 | 应用代码 |
| C:/app/logs | executor_logs卷 | 日志目录 |
| C:/app/config | ./config | 配置文件目录 |
| C:/Program Files/CANoe | 主机安装目录 | CANoe（只读） |
| C:/Program Files/TSMaster | 主机安装目录 | TSMaster（只读） |

---

**文档版本**: 1.0  
**创建日期**: 2026-02-27  
**作者**: AI Assistant
