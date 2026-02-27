# Python执行器 - 部署和使用指南

## 🚀 快速开始

### 1. 环境准备

**系统要求：**
- Windows 10/11 (推荐，因为CANoe只支持Windows)
- Python 3.8+
- CANoe 11.0+ 或 TSMaster 2023+

**安装Python依赖：**
```bash
pip install -r requirements.txt
```

### 2. 基本配置

**创建配置文件：**
```bash
cp config/executor_config.json.example config/executor_config.json
# 根据需要修改配置
```

**主要配置项：**
- `websocket.port`: WebSocket服务端口（默认8080）
- `logging.level`: 日志级别（INFO/DEBUG/ERROR）
- `task.timeout`: 任务超时时间（秒）
- `canoe.timeout`: CANoe连接超时时间

### 3. 启动执行器

```bash
# 方式1：使用运行脚本
python run.py

# 方式2：直接运行主模块
python main.py
```

**启动成功标志：**
```
============================================================
Python执行器启动
版本: 1.0.0
监听地址: ws://0.0.0.0:8080
============================================================
```

## 📋 功能测试

### 连接测试
```bash
# 检查WebSocket连接
curl http://localhost:8080/health

# 查看执行器状态
curl http://localhost:8080/status
```

### 运行测试用例
```bash
# 运行内置测试
python test_executor.py
```

## 🔧 高级配置

### 日志配置
```json
{
    "logging": {
        "level": "DEBUG",
        "file": "logs/executor.log",
        "max_size": 10485760,
        "backup_count": 5
    }
}
```

### 网络配置
```json
{
    "websocket": {
        "host": "0.0.0.0",
        "port": 8080,
        "heartbeat_interval": 30,
        "reconnect_interval": 5
    }
}
```

### 任务配置
```json
{
    "task": {
        "timeout": 3600,
        "check_interval": 1,
        "max_concurrent": 1
    }
}
```

## 📊 监控和调试

### 查看日志
```bash
# 实时查看日志
tail -f logs/executor.log

# 查看最近100行日志
tail -n 100 logs/executor.log
```

### 调试模式
修改配置文件中的日志级别为DEBUG：
```json
{
    "logging": {
        "level": "DEBUG"
    }
}
```

## 🐛 常见问题

### 1. CANoe连接失败
**问题：** `pywin32库未安装`
**解决：** `pip install pywin32`

### 2. TSMaster API未找到
**问题：** `TSMaster Python API未安装`
**解决：** 确保已安装TSMaster并在其Python环境中运行

### 3. WebSocket连接超时
**问题：** 连接Java服务端超时
**解决：** 检查网络配置，确保端口8080未被占用

### 4. 任务执行超时
**问题：** 任务执行时间超过配置的超时时间
**解决：** 增加`task.timeout`配置值

## 🔗 集成测试

### 与Java服务端集成
```java
// Java端连接示例
WebSocketClient client = new StandardWebSocketClient();
WebSocketStompClient stompClient = new WebSocketStompClient(client);
StompSession session = stompClient.connect("ws://localhost:8080/ws/executor").get();
```

### 消息格式验证
```json
// 任务下发消息
{
    "type": "TASK_DISPATCH",
    "taskId": "TASK_20250203_001",
    "deviceId": "DEVICE_001",
    "toolType": "canoe",
    "configPath": "C:\\TestConfigs\\EngineTest.cfg",
    "testItems": [
        {
            "name": "初始化检查",
            "type": "signal_check",
            "signalName": "EngineStatus",
            "expectedValue": 0
        }
    ],
    "timeout": 3600,
    "timestamp": 1706963200000
}
```

## 📈 性能优化

### 1. 内存优化
- 及时释放COM对象
- 控制日志文件大小
- 限制并发任务数量

### 2. 网络优化
- 调整心跳间隔
- 启用消息压缩
- 优化重连策略

### 3. 执行优化
- 合理设置超时时间
- 优化测试项执行顺序
- 启用结果缓存

## 🛡️ 安全考虑

### 1. 输入验证
- 验证配置文件路径
- 检查测试项参数
- 限制执行权限

### 2. 网络安全
- 使用安全的WebSocket连接
- 实现访问控制
- 加密敏感数据传输

### 3. 异常处理
- 完善的异常捕获
- 优雅的错误处理
- 安全的资源清理

## 📚 相关文档

- [技术任务书](技术任务书_测试平台远程执行任务.md)
- [CANoe COM接口文档](api/AN-AND-1-117_CANoe_CANalyzer_as_a_COM_Server.pdf)
- [TSMaster Python API指南](api/TSMaster_COM_API_Python编程指导.pdf)