# TSMasterAdapter RPC改进使用指南

## 概述

TSMasterAdapter已成功集成RPC支持，现在支持两种通信模式：

1. **RPC模式（推荐）**：基于共享内存的高性能通信，延迟极低
2. **传统模式**：基于TSMaster Python API的通信方式

## 架构对比

### RPC模式架构

```
Python测试脚本
    │
    │ TSMasterAPI (共享内存)
    ▼
TSMaster应用程序
    │
    │ RPC客户端（小程序）
    ▼
CAN/LIN/FlexRay总线
```

### 传统模式架构

```
Python测试脚本
    │
    │ TSMaster Python API
    ▼
TSMaster应用程序
    │
    │ 直接API调用
    ▼
CAN/LIN/FlexRay总线
```

## 性能对比

| 特性 | RPC模式 | 传统模式 |
|------|---------|----------|
| 通信延迟 | 极低（<1ms） | 中等（5-10ms） |
| CPU占用 | 低 | 中等 |
| 实现复杂度 | 低 | 中等 |
| 稳定性 | 高 | 高 |
| 功能完整性 | 完整 | 完整 |

## 使用方法

### 1. 基本使用

```python
from core.adapters.tsmaster_adapter import TSMasterAdapter

# 创建适配器（默认使用RPC模式）
adapter = TSMasterAdapter()

# 连接TSMaster
if adapter.connect():
    print("连接成功")
    
    # 启动仿真
    if adapter.start_test():
        print("仿真已启动")
        
        # 读取信号
        value = adapter._get_signal("TestSignal")
        print(f"信号值: {value}")
        
        # 设置信号
        adapter._set_signal("TestSignal", 100.0)
        
        # 停止仿真
        adapter.stop_test()
    
    # 断开连接
    adapter.disconnect()
```

### 2. 配置RPC模式

```python
# 使用RPC模式（默认）
config = {
    "use_rpc": True,
    "rpc_app_name": None,  # 自动发现
    "fallback_to_traditional": True  # RPC失败时回退到传统模式
}

adapter = TSMasterAdapter(config)
```

### 3. 指定RPC应用程序

```python
# 连接到特定的TSMaster应用程序
config = {
    "use_rpc": True,
    "rpc_app_name": "MyTSMasterApp",
    "fallback_to_traditional": False  # 不回退
}

adapter = TSMasterAdapter(config)
```

### 4. 仅使用传统模式

```python
# 禁用RPC，仅使用传统模式
config = {
    "use_rpc": False
}

adapter = TSMasterAdapter(config)
```

## 测试项类型

### 1. 信号检查

```python
item = {
    "name": "检查发动机转速",
    "type": "signal_check",
    "signal_name": "EngineSpeed",
    "expected_value": 1000.0,
    "tolerance": 10.0
}

result = adapter.execute_test_item(item)
print(f"测试结果: {result['status']}")
```

### 2. 信号设置

```python
item = {
    "name": "设置发动机转速",
    "type": "signal_set",
    "signal_name": "EngineSpeed",
    "value": 2000.0
}

result = adapter.execute_test_item(item)
print(f"设置结果: {result['status']}")
```

### 3. 报文发送

```python
item = {
    "name": "发送测试报文",
    "type": "message_send",
    "channel": 0,
    "msg_id": 0x123,
    "data": [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08]
}

result = adapter.execute_test_item(item)
print(f"发送结果: {result['status']}")
```

### 4. 系统变量检查（新增）

```python
item = {
    "name": "检查系统变量",
    "type": "sysvar_check",
    "var_name": "Var0",
    "expected_value": "1000"
}

result = adapter.execute_test_item(item)
print(f"测试结果: {result['status']}")
```

### 5. 系统变量设置（新增）

```python
item = {
    "name": "设置系统变量",
    "type": "sysvar_set",
    "var_name": "Var0",
    "value": "2000"
}

result = adapter.execute_test_item(item)
print(f"设置结果: {result['status']}")
```

## RPC客户端直接使用

如果需要更细粒度的控制，可以直接使用RPC客户端：

```python
from core.adapters.tsmaster.rpc_client import TSMasterRPCClient

# 创建RPC客户端
client = TSMasterRPCClient()

# 初始化
client.initialize()

# 连接
if client.connect("MyTSMasterApp"):
    # 启动仿真
    client.start_simulation()
    
    # 读写信号
    value = client.get_can_signal("TestSignal")
    client.set_can_signal("TestSignal", 100.0)
    
    # 读写系统变量
    var_value = client.read_system_var("Var0")
    client.write_system_var("Var0", "2000")
    
    # 发送CAN报文
    client.transmit_can_message(
        channel=0,
        msg_id=0x123,
        data=[0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08],
        is_extended=False,
        is_fd=False
    )
    
    # 停止仿真
    client.stop_simulation()
    
    # 断开连接
    client.disconnect()

# 释放资源
client.finalize()
```

## 上下文管理器使用

```python
from core.adapters.tsmaster.rpc_client import TSMasterRPCClient

# 使用上下文管理器自动管理资源
with TSMasterRPCClient() as client:
    client.start_simulation()
    client.set_can_signal("TestSignal", 100.0)
    client.stop_simulation()
# 自动释放资源
```

## 信号路径格式

### 完整路径格式

```
通道/数据库/节点/报文/信号
```

示例：
```
0/CAN_FD_Powertrain/Engine/Test_Message_CAN_FD/Test_Signal_Byte_00
```

### 简化路径格式（TSMaster新版本支持）

```
信号名
```

示例：
```
Test_Signal_Byte_01_02
```

注意：使用简化路径需要：
1. 开启RBS（剩余总线仿真）
2. 信号名无重名

## 错误处理

```python
from core.adapters.tsmaster_adapter import TSMasterAdapter

adapter = TSMasterAdapter()

try:
    if not adapter.connect():
        print(f"连接失败: {adapter.get_error()}")
        return
    
    if not adapter.start_test():
        print(f"启动仿真失败: {adapter.get_error()}")
        return
    
    # 执行测试...
    
except Exception as e:
    print(f"发生异常: {str(e)}")
    
finally:
    adapter.disconnect()
```

## 日志配置

```python
import logging

# 配置日志级别
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)

adapter = TSMasterAdapter()
adapter.connect()
```

## 最佳实践

### 1. 优先使用RPC模式

```python
# 推荐：使用RPC模式
config = {
    "use_rpc": True,
    "fallback_to_traditional": True
}
adapter = TSMasterAdapter(config)
```

### 2. 使用上下文管理器

```python
# 推荐：使用上下文管理器自动管理资源
with TSMasterRPCClient() as client:
    client.start_simulation()
    # 执行操作...
    client.stop_simulation()
```

### 3. 检查连接状态

```python
# 检查连接状态
if not adapter.is_connected:
    print("未连接到TSMaster")
    return

# 检查仿真状态
if not adapter.is_running:
    print("仿真未运行")
    return
```

### 4. 合理设置超时时间

```python
# 根据实际需求设置超时时间
config = {
    "start_timeout": 60,  # 启动超时60秒
    "stop_timeout": 20    # 停止超时20秒
}
adapter = TSMasterAdapter(config)
```

## 故障排查

### 问题1：RPC连接失败

**症状**：连接时提示"RPC模式连接失败"

**解决方案**：
1. 确保TSMaster应用程序正在运行
2. 确保TSMaster中加载了RPC小程序
3. 检查RPC小程序是否已激活服务器
4. 启用回退机制：`fallback_to_traditional=True`

### 问题2：信号读写失败

**症状**：读写信号时返回None或False

**解决方案**：
1. 确保已启动仿真
2. 确保信号路径格式正确
3. 确保已加载相应的数据库
4. 检查RBS是否已激活

### 问题3：系统变量操作失败

**症状**：读写系统变量时返回None或False

**解决方案**：
1. 确保系统变量已定义
2. 检查系统变量名称是否正确
3. 确保变量类型匹配

## 迁移指南

### 从旧版本迁移

如果你之前使用的是传统模式的TSMasterAdapter，迁移到RPC模式非常简单：

```python
# 旧代码
adapter = TSMasterAdapter()
adapter.connect()

# 新代码（无需修改，默认使用RPC模式）
adapter = TSMasterAdapter()
adapter.connect()
```

所有现有代码都可以无缝使用，适配器会自动选择最佳通信方式。

## 总结

TSMasterAdapter的RPC改进提供了：

1. **更高的性能**：通过共享内存实现极低延迟通信
2. **更好的兼容性**：自动回退机制确保向后兼容
3. **更丰富的功能**：新增系统变量操作支持
4. **更简单的使用**：统一的API接口，无需关心底层实现

推荐在所有新项目中使用RPC模式，以获得最佳性能。
