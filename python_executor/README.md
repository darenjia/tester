# Python执行器

测试平台远程执行任务的Python端实现

## 项目结构

```
python_executor/
├── app.py                    # Flask WebSocket服务端入口
├── config/
│   ├── settings.py          # 配置文件管理
│   └── executor_config.json # 自定义配置文件（可选）
├── core/
│   ├── task_executor.py     # 任务执行核心引擎
│   ├── canoe_controller.py  # CANoe COM接口封装
│   ├── tsmaster_controller.py # TSMaster Python API封装
│   └── result_collector.py  # 结果收集与格式化
├── websocket/
│   └── client.py            # WebSocket服务端实现
├── models/
│   ├── task.py             # 任务模型定义
│   └── result.py           # 结果模型定义
└── utils/
    ├── logger.py           # 统一日志工具
    └── exceptions.py       # 自定义异常类
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

### 安装依赖
```bash
pip install flask flask-socketio pywin32
# TSMaster需要单独安装Python API
```

### 启动执行器
```bash
python app.py
```

### 配置文件
创建 `config/executor_config.json` 自定义配置：
```json
{
    "websocket_port": 8080,
    "log_level": "INFO",
    "task_timeout": 3600,
    "canoe_timeout": 30
}
```

## 使用说明

执行器启动后会监听指定端口，等待Java服务端连接。支持以下消息类型：

- **TASK_DISPATCH**：任务下发
- **TASK_CANCEL**：任务取消
- **HEARTBEAT**：心跳检测

执行器会自动处理任务执行，并实时上报状态和结果。

## 开发文档

详见各模块源码中的docstring说明。