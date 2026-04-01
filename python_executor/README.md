# Python执行器

测试平台远程执行任务的Python端实现

## 项目结构

```
python_executor/
├── main.py                   # 主应用入口（标准版）
├── main_production.py        # 生产环境入口（推荐）
├── app.py                    # Flask WebSocket服务端入口（简化版）
├── config/
│   ├── settings.py          # 配置文件管理
│   ├── config_manager.py    # 配置热更新管理
│   └── unified_config.py    # 统一配置实现（读取根目录 config.json）
├── core/
│   ├── task_executor.py           # 任务执行核心引擎（标准版）
│   ├── task_executor_production.py # 任务执行引擎（生产环境版）
│   ├── adapters/            # 测试工具适配器
│   │   ├── adapter_factory.py    # 适配器工厂
│   │   ├── canoe_adapter.py      # CANoe适配器
│   │   ├── tsmaster_adapter.py   # TSMaster适配器
│   │   └── ttworkbench_adapter.py # TTworkbench适配器
│   ├── canoe_controller.py  # CANoe控制器（旧版）
│   ├── tsmaster_controller.py # TSMaster控制器（旧版）
│   └── result_collector.py  # 结果收集与格式化
├── ws_server/
│   └── client.py            # WebSocket服务端实现
├── models/
│   ├── task.py             # 任务模型定义
│   └── result.py           # 结果模型定义
├── utils/
│   ├── logger.py           # 统一日志工具
│   ├── exceptions.py       # 自定义异常类
│   ├── validators.py       # 输入验证器
│   ├── retry.py            # 重试和熔断器
│   └── metrics.py          # 性能监控
├── tests/                   # 测试目录
│   └── test_api/           # API测试
├── docker/                  # Docker部署配置
│   ├── linux/              # Linux容器配置
│   ├── windows/            # Windows容器配置
│   └── docker-compose.yml  # Docker Compose配置
└── deploy/                  # 部署脚本和配置
```

## 功能特性

- ✅ **双工具支持**：同时支持CANoe和TSMaster
- ✅ **WebSocket通信**：实时双向通信，支持断线重连
- ✅ **状态机管理**：完整的任务状态流转控制
- ✅ **多线程执行**：支持任务取消和超时控制
- ✅ **实时上报**：执行进度、日志、结果实时推送
- ✅ **异常处理**：完善的异常捕获和错误上报机制
- ✅ **配置管理**：灵活的配置文件支持
- ✅ **日志系统**：分级日志，支持上下文追踪

## 快速开始

### 环境要求
- Python 3.8+
- Windows 10/11（CANoe要求）
- CANoe 11.0+ 或 TSMaster 2023+

### 安装依赖**安装Python依赖：**
```bash
pip install -r requirements.txt
# 或生产环境依赖
pip install -r requirements_production.txt
```

### 启动执行器
```bash
# 方式1：生产环境（推荐）
python main_production.py

# 方式2：标准版本
python main.py

# 方式3：简化版本
python app.py
```

### 配置文件
项目统一使用根目录下的 `config.json` 作为唯一配置入口：
```json
{
    "websocket": {
        "port": 8180,
        "host": "0.0.0.0"
    },
    "logging": {
        "level": "INFO"
    },
    "task": {
        "timeout": 3600
    },
    "canoe": {
        "timeout": 30
    }
}
```

## 使用说明

执行器启动后会监听指定端口，等待Java服务端连接。支持以下消息类型：

- **TASK_DISPATCH**：任务下发
- **TASK_CANCEL**：任务取消
- **HEARTBEAT**：心跳检测

执行器会自动处理任务执行，并实时上报状态和结果。

## 可观测性

内部执行链路目前已经收成：

- WebSocket / 平台任务先进入入口层
- 入口层通过 `TaskCompiler` 编译成内部 `ExecutionPlan`
- `TaskExecutorProduction` 只消费内部执行计划
- 外部 API / 队列展示模型继续独立保留

当前任务执行生命周期会按以下阶段流转：

- `received`
- `validated`
- `compiled`
- `queued`
- `preparing`
- `executing`
- `collecting`
- `reporting`
- `finished`

监控接口说明：

- `/health`：返回服务健康状态和业务健康摘要，例如当前排队数、活跃任务数、最近失败任务数
- `/status`：返回运行状态、当前任务信息和业务摘要
- `/metrics`：返回原始指标、性能报告和业务执行摘要

## 开发文档

详见各模块源码中的docstring说明。
