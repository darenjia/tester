# C#项目网络任务执行器设计文档

## 概述

**文档日期**: 2026-03-22
**项目**: UltraANetT (C# 测试执行器)
**目标**: 在旧版CANoe软件环境中接入网络任务模块，实现自动接收、执行、上报测试任务的完整流程

## 背景

### 现有系统

1. **Python执行器** (`python_executor/`)
   - 支持新版CANoe软件
   - 已实现完整的网络任务接收、执行、上报流程
   - 使用TDM2.0格式与调度服务器交互

2. **C#执行器** (`csharp_middleware/UltraANetT/`)
   - 支持旧版CANoe软件
   - 已有网络任务模块框架：
     - `NetworkTaskManager` - 网络任务管理器
     - `HttpServerManager` - HTTP服务器
     - `TaskQueueManager` - 任务队列管理
     - `ResultReporter` - 结果上报客户端
     - `TaskController` - 任务控制器
     - `Models.cs` - 数据模型
   - 缺少自动执行引擎和完整的执行流程集成

### 需求确认

| 需求项 | 决策 |
|--------|------|
| 执行模式 | 自动执行模式 - 收到网络任务后自动开始测试 |
| 任务映射 | taskName → C#任务类别（如"CAN通信单元"） |
| 车型配置 | 设备绑定模式 - 设备预先绑定车型配置信息 |
| 用例匹配 | caseNo编号匹配 - 根据用例编号查询数据库 |
| 结果上报 | TDM2.0接口 - `http://10.124.11.142:8315/api/python/report` |

## 架构设计

### 整体架构

```
网络任务调度服务器
       │
       ▼ HTTP POST /api/tasks/submit
┌─────────────────────────────────────────────────┐
│           HttpServerManager                      │
│         (已实现 - 监听HTTP请求)                   │
└─────────────────────────────────────────────────┘
       │
       ▼ TaskController.SubmitTask()
┌─────────────────────────────────────────────────┐
│           TaskQueueManager                       │
│         (已实现 - 任务队列管理)                   │
└─────────────────────────────────────────────────┘
       │
       ▼ 触发 TaskReceived 事件
┌─────────────────────────────────────────────────┐
│      NetworkTaskExecutor (新增)                  │
│  - 监听任务队列，自动触发执行                      │
│  - 调用设备配置获取绑定信息                        │
│  - 映射任务类别到C#任务类型                        │
│  - 查询用例库，匹配testItems                      │
│  - 调用TestStart执行测试                          │
│  - 收集结果并上报                                 │
└─────────────────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────────────┐
│           TestStart (现有)                       │
│  - 加载cfg配置                                    │
│  - 生成INI文件                                    │
│  - 执行CANoe测试                                  │
│  - 生成报告                                       │
└─────────────────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────────────┐
│           ResultReporter (已实现)                │
│  - 上报到TDM2.0接口                               │
│  - http://10.124.11.142:8315/api/python/report   │
└─────────────────────────────────────────────────┘
```

### 模块职责

| 模块 | 状态 | 职责 |
|------|------|------|
| HttpServerManager | 已实现 | HTTP服务监听，接收任务提交请求 |
| TaskQueueManager | 已实现 | 管理待执行、执行中、已完成任务队列 |
| TaskController | 已实现 | 任务CRUD操作业务逻辑 |
| Models.cs | 已实现 | NetworkTask、TaskResult等数据模型 |
| ResultReporter | 已实现(需适配) | HTTP结果上报客户端 |
| NetworkTaskManager | 已实现(需完善) | 网络任务管理器入口 |
| **DeviceConfig** | **新增** | 设备绑定配置管理 |
| **TaskCategoryMapper** | **新增** | 任务类别映射 |
| **NetworkTaskExecutor** | **新增** | 自动执行引擎 |

## 详细设计

### 1. DeviceConfig - 设备配置模块

**文件**: `NetworkTask/DeviceConfig.cs`

**职责**: 管理设备绑定的车型配置信息

**属性**:
```csharp
public class DeviceBindingConfig
{
    public string VehicleType { get; set; }      // 车型，如 "车型A"
    public string VehicleConfig { get; set; }    // 配置，如 "配置1"
    public string VehicleStage { get; set; }     // 阶段，如 "阶段2"
    public string TestChannel { get; set; }      // 测试通道，如 "CAN1"
    public string DbcPath { get; set; }          // DBC文件路径
    public string DefaultBaudRate { get; set; }  // 默认波特率
}
```

**方法**:
```csharp
public class DeviceConfig
{
    public static DeviceConfig Load();                    // 从配置文件加载
    public DeviceBindingConfig GetBindingConfig();        // 获取绑定配置
    public void Save(DeviceBindingConfig config);         // 保存配置
}
```

### 2. TaskCategoryMapper - 任务类别映射

**文件**: `NetworkTask/TaskCategoryMapper.cs`

**职责**: 将网络任务的taskName映射到C#项目的任务类别

**映射表**:
| 网络任务taskName | C#任务类别 | 说明 |
|------------------|------------|------|
| CAN通信单元 | CANUnit | CAN单节点测试 |
| CAN通信集成 | CANIntegration | CAN集成测试 |
| LIN通信主节点 | LINMaster | LIN主节点测试 |
| LIN通信从节点 | LINSalve | LIN从节点测试 |
| LIN通信集成 | LINIntegration | LIN集成测试 |
| 直接NM单元 | DirectNMUnit | 直接NM单节点 |
| 直接NM集成 | DirectNMIntegration | 直接NM集成 |
| 动力域NM主节点 | DynamicNMMaster | 动力域NM主节点 |
| 动力域NM从节点 | DynamicNMSlave | 动力域NM从节点 |
| 动力域NM集成 | DynamicNMIntegration | 动力域NM集成 |
| 间接NM单元 | IndirectNMUnit | 间接NM单节点 |
| 间接NM集成 | IndirectNMIntegration | 间接NM集成 |
| 通信DTC | CommunicationDTC | DTC诊断测试 |
| OSEK NM单元 | OSEKNMUnit | OSEK NM单节点 |
| OSEK NM集成 | OSEKNMIntegration | OSEK NM集成 |
| 网关路由 | GatewayRouting | 网关路由测试 |

**方法**:
```csharp
public class TaskCategoryMapper
{
    public static string MapToCategory(string taskName);      // 映射到类别
    public static bool IsValidCategory(string category);      // 验证类别
    public static List<string> GetAllCategories();            // 获取所有类别
}
```

### 3. NetworkTaskExecutor - 自动执行引擎

**文件**: `NetworkTask/NetworkTaskExecutor.cs`

**职责**: 后台监听任务队列，自动触发测试执行

**核心流程**:
```
1. 启动后台线程，轮询TaskQueueManager
2. 从队列取出待执行任务
3. 获取设备绑定配置 → 设置GlobalVar.CurrentVNode
4. 映射taskName → 任务类别
5. 根据caseNo查询用例库
6. 准备配置文件 (cfg、ini)
7. 调用TestStart/ProcCANoeTest执行测试
8. 收集执行结果
9. 调用ResultReporter上报
10. 标记任务完成
```

**类设计**:
```csharp
public class NetworkTaskExecutor : IDisposable
{
    private Thread _workerThread;
    private bool _isRunning;
    private readonly TaskQueueManager _taskQueue;
    private readonly NetworkTaskManager _networkTaskManager;

    public event EventHandler<TaskExecutionEventArgs> TaskStarted;
    public event EventHandler<TaskExecutionEventArgs> TaskCompleted;
    public event EventHandler<TaskExecutionEventArgs> TaskFailed;

    public void Start();
    public void Stop();
    private void WorkerLoop();
    private void ExecuteTask(NetworkTask task);
    private bool PrepareTestEnvironment(NetworkTask task);
    private void RunTest(NetworkTask task);
    private TaskResult CollectResult(NetworkTask task);
    private void ReportResult(string taskNo, TaskResult result);
}
```

### 4. ResultReporter 适配

**文件**: `NetworkTask/ResultReporter.cs` (修改)

**修改内容**:
- 上报地址改为 `http://10.124.11.142:8315/api/python/report`
- 数据格式适配TDM2.0

**TDM2.0上报格式**:
```json
{
    "taskNo": "NET-PROJ-20260322-001",
    "projectNo": "PROJ001",
    "deviceId": "DEVICE_001",
    "taskName": "CAN通信单元",
    "toolType": "CANoe",
    "status": "completed",
    "startTime": "2026-03-22T10:00:00",
    "endTime": "2026-03-22T10:30:00",
    "caseList": [
        {
            "caseNo": "CANOE-001",
            "result": "PASS",
            "remark": "测试通过",
            "created": "2026-03-22 10:15:00"
        }
    ],
    "summary": {
        "total": 1,
        "pass": 1,
        "fail": 0,
        "block": 0,
        "skip": 0
    }
}
```

### 5. 配置文件设计

**文件**: `configuration/NetworkTaskConfig.json`

```json
{
    "enabled": true,
    "httpServerEnabled": true,
    "listenUrl": "http://+:8180/",
    "autoConnect": true,
    "autoExecute": true,
    "schedulerUrl": "http://10.124.11.142:8315",
    "reportApiPath": "/api/python/report",
    "pollingIntervalMs": 1000,

    "deviceBinding": {
        "vehicleType": "车型A",
        "vehicleConfig": "配置1",
        "vehicleStage": "阶段2",
        "testChannel": "CAN1",
        "dbcPath": "C:\\DBC\\车型A\\",
        "defaultBaudRate": "500000"
    },

    "taskCategoryMapping": {
        "CAN通信单元": "CANUnit",
        "CAN通信集成": "CANIntegration",
        "LIN通信主节点": "LINMaster",
        "LIN通信从节点": "LINSalve",
        "LIN通信集成": "LINIntegration",
        "直接NM单元": "DirectNMUnit",
        "直接NM集成": "DirectNMIntegration",
        "动力域NM主节点": "DynamicNMMaster",
        "动力域NM从节点": "DynamicNMSlave",
        "动力域NM集成": "DynamicNMIntegration",
        "间接NM单元": "IndirectNMUnit",
        "间接NM集成": "IndirectNMIntegration",
        "通信DTC": "CommunicationDTC",
        "OSEK NM单元": "OSEKNMUnit",
        "OSEK NM集成": "OSEKNMIntegration",
        "网关路由": "GatewayRouting"
    }
}
```

## 数据流

### 1. 任务提交流程

```
POST /api/tasks/submit
    │
    ▼
{
    "taskNo": "NET-PROJ-20260322-001",
    "taskName": "CAN通信单元",
    "projectNo": "PROJ001",
    "testItems": [
        {"name": "测试项1", "type": "auto", "parameters": {...}},
        {"name": "测试项2", "type": "auto", "parameters": {...}}
    ],
    "variables": {"var1": "value1"},
    "timeout": 3600,
    "priority": 0
}
    │
    ▼ TaskController.SubmitTask()
NetworkTask 对象创建
    │
    ▼ TaskQueueManager.EnqueueTask()
任务入队，状态: Pending
```

### 2. 任务执行流程

```
NetworkTaskExecutor.WorkerLoop()
    │ 轮询队列
    ▼
取出待执行任务
    │
    ├─ 标记状态: Running
    │
    ├─ 获取设备配置
    │  DeviceConfig.GetBindingConfig()
    │  └─ GlobalVar.CurrentVNode = [vehicleType, vehicleConfig, vehicleStage]
    │
    ├─ 映射任务类别
    │  TaskCategoryMapper.MapToCategory("CAN通信单元")
    │  └─ 返回 "CANUnit"
    │
    ├─ 查询用例库
    │  根据 caseNo 从数据库查询用例详情
    │
    ├─ 准备测试环境
    │  ├─ 加载 cfg 文件
    │  ├─ 生成 INI 文件
    │  └─ 设置测试参数
    │
    ├─ 执行测试
    │  ProcCANoeTest.Execute()
    │  └─ CANoe COM 接口调用
    │
    ├─ 收集结果
    │  解析测试报告，生成 TaskResult
    │
    └─ 上报结果
       ResultReporter.ReportResultAsync()
       └─ POST http://10.124.11.142:8315/api/python/report
```

### 3. 结果上报格式

```json
{
    "taskNo": "NET-PROJ-20260322-001",
    "projectNo": "PROJ001",
    "deviceId": "DEVICE_001",
    "taskName": "CAN通信单元",
    "toolType": "CANoe",
    "status": "completed",
    "startTime": "2026-03-22T10:00:00",
    "endTime": "2026-03-22T10:30:00",
    "caseList": [
        {
            "caseNo": "CANOE-001",
            "result": "PASS",
            "remark": "测试通过",
            "created": "2026-03-22 10:15:00"
        },
        {
            "caseNo": "CANOE-002",
            "result": "FAIL",
            "remark": "信号值不匹配",
            "created": "2026-03-22 10:20:00"
        }
    ],
    "summary": {
        "total": 2,
        "pass": 1,
        "fail": 1,
        "block": 0,
        "skip": 0
    }
}
```

## 错误处理

### 异常类型

| 异常场景 | 处理方式 |
|----------|----------|
| 任务类别不支持 | 标记任务失败，上报错误信息 |
| 用例编号不存在 | 标记该用例为BLOCK，继续执行其他用例 |
| cfg文件加载失败 | 标记任务失败，上报错误信息 |
| CANoe连接失败 | 重试3次，仍失败则标记任务失败 |
| 测试执行超时 | 强制停止测量，标记任务失败 |
| 结果上报失败 | 重试3次，记录本地日志 |

### 日志记录

- 所有操作记录到调试输出
- 关键错误记录到文件日志
- 任务执行状态变更记录

## 测试计划

### 单元测试

1. `DeviceConfig` 加载/保存测试
2. `TaskCategoryMapper` 映射正确性测试
3. `NetworkTaskExecutor` 队列处理测试
4. `ResultReporter` 上报格式测试

### 集成测试

1. HTTP任务提交 → 队列入队测试
2. 任务自动执行 → 结果收集测试
3. 结果上报 → TDM2.0接口测试

### 端到端测试

1. 完整流程测试：提交 → 执行 → 上报
2. 异常场景测试：网络断开、CANoe异常等
3. 并发任务测试：多任务排队执行

## 文件清单

| 文件路径 | 状态 | 说明 |
|----------|------|------|
| `NetworkTask/DeviceConfig.cs` | 新增 | 设备配置管理 |
| `NetworkTask/TaskCategoryMapper.cs` | 新增 | 任务类别映射 |
| `NetworkTask/NetworkTaskExecutor.cs` | 新增 | 自动执行引擎 |
| `NetworkTask/ResultReporter.cs` | 修改 | 适配TDM2.0上报 |
| `NetworkTask/NetworkTaskManager.cs` | 修改 | 集成执行引擎 |
| `NetworkTask/NetworkTaskConfig.cs` | 修改 | 新增配置项 |
| `Module/Task.cs` | 修改 | 集成执行引擎启动 |
| `configuration/NetworkTaskConfig.json` | 新增 | 配置文件 |

## 实施步骤

1. **Phase 1: 配置模块**
   - 实现 DeviceConfig
   - 实现 TaskCategoryMapper
   - 更新 NetworkTaskConfig

2. **Phase 2: 执行引擎**
   - 实现 NetworkTaskExecutor
   - 集成到 NetworkTaskManager

3. **Phase 3: 结果上报**
   - 修改 ResultReporter 适配TDM2.0
   - 实现结果收集逻辑

4. **Phase 4: 集成测试**
   - 修改 Task.cs 启动执行引擎
   - 端到端测试

## 版本信息

- **文档版本**: 1.0
- **创建日期**: 2026-03-22
- **作者**: Claude Code