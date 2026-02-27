# TSMaster RPC Demo 程序分析

## 1. 项目概述

这是一个TSMaster RPC（远程过程调用）示例项目，演示了如何通过Python脚本远程控制TSMaster应用程序进行CAN总线测试。

### 1.1 项目结构

```
RPC/
├── .TSProj/                    # TSMaster项目文件
├── DB/CAN/                     # CAN数据库
│   └── CAN_FD_Powertrain.pdbc  # CAN FD数据库文件
├── Diagnostic/                 # 诊断配置
│   └── Diagnostic/
│       ├── *.tsbinary         # 诊断数据库
│       ├── DiagDBTable.TSC    # 诊断数据库表
│       ├── Diagnostic.TSC     # 诊断配置
│       ├── DiagnosticStruct.json  # 诊断结构定义
│       └── TSDiagnosticFlow.xml   # 诊断流程
├── MiniProgram/               # 小程序（C代码）
│   ├── CSrc/                  # C源码
│   │   └── CCode7852.cpp     # 主程序
│   ├── CSrcExtern/           # 外部C源码
│   │   └── CCode7852Extern.cpp
│   ├── Build/                # 编译输出
│   ├── Conf/                 # 配置文件
│   │   ├── MPLibrariesLookup.ini
│   │   ├── MPLibraryPrototypes.ini
│   │   ├── TSMasterFunctionPrototypesC.ini
│   │   ├── TSMasterFunctionPrototypesPython.ini
│   │   ├── TSMasterMPAPIsC_1033.ini
│   │   └── TSMasterMPAPIsPython_1033.ini
│   └── h/                    # 头文件
│       └── TSMaster.h
├── Simulation/               # 仿真配置
│   ├── RBSCAN.ini
│   └── RBSFlexRay.ini
├── bin/                      # 二进制输出
│   └── CCode7852.mp         # 编译后的小程序
├── conf/                     # 运行配置
│   ├── Perf.ini
│   ├── SignalTester.ini
│   └── mp.ini
├── lyt/                      # 布局文件
│   ├── Empty.lyt
│   ├── TSMaster_CurrentLayout.lyt
│   └── TSMaster_DefaultLayout.lyt
├── test.py                   # Python RPC客户端示例
├── TSMaster.ini             # TSMaster主配置
├── UI.ini                   # UI配置
├── RPC_demo.t7z             # 项目压缩包
├── desktop.ini              # 桌面配置
└── TSMaster RPC 编程指导V2.0.pdf  # 编程指导文档
```

---

## 2. 核心文件分析

### 2.1 test.py - Python RPC客户端

这是核心的Python RPC客户端示例代码：

```python
from TSMasterAPI import *

# 1. 初始化TSMaster库
initialize_lib_tsmaster(b'TSMasterTest')

# 2. 获取RPC句柄和应用程序列表
rpchandle = size_t(0)
namelist = pchar()
get_active_application_list(namelist)

# 3. 获取正在运行的TSMaster进程名称
print(namelist.value.decode())
APPNameList = namelist.value.decode().split(';')

if len(APPNameList) != 0:
    # 4. 创建RPC客户端
    ret = rpc_tsmaster_create_client(APPNameList[0].encode(), rpchandle)
    
    # 5. 激活客户端
    ret = rpc_tsmaster_activate_client(rpchandle, True)
    
    # 6. 设置系统变量
    ret = rpc_tsmaster_cmd_write_system_var(rpchandle, b'Var0', b'1000')
    
    # 7. 启动仿真
    ret = rpc_tsmaster_cmd_start_simulation(rpchandle)
    
    # 8. 设置CAN信号值
    ret = rpc_tsmaster_cmd_set_can_signal(
        rpchandle,
        b'0/CAN_FD_Powertrain/Engine/Test_Message_CAN_FD/Test_Signal_Byte_00',
        100
    )
    
    # 9. 读取CAN信号值
    d = double(0)
    ret = rpc_tsmaster_cmd_get_can_signal(
        rpchandle,
        b'0/CAN_FD_Powertrain/Engine/Test_Message_CAN_FD/Test_Signal_Byte_00',
        d
    )
    print(d)

# 10. 释放资源
finalize_lib_tsmaster()
```

### 2.2 CCode7852.cpp - 小程序主程序

这是运行在TSMaster内部的C代码小程序：

```cpp
#include "TSMaster.h"
#include "MPLibrary.h"
#include "Database.h"
#include "TSMasterBaseInclude.h"
#include "Configuration.h"

// On Start事件处理
void on_start_NewOn_Start1(void) {
    try {
        // 启动时激活RPC服务器
        com.rpc_tsmaster_activate_server(true);
    } catch (...) {
        log_nok("CRASH detected");
        app.terminate_application();
    }
}

// 主循环函数（每5ms执行一次）
void step(void) {
    try {
        // 循环处理逻辑
    } catch (...) {
        log_nok("CRASH detected");
        app.terminate_application();
    }
}
```

### 2.3 CCode7852Extern.cpp - 外部函数定义

包含小程序的外部函数和工具函数：

```cpp
#include "TSMaster.h"
#include "MPLibrary.h"
#include "Configuration.h"
#include "TSMasterBaseInclude.h"

// 全局对象
TTSApp app;      // 应用程序对象
TTSCOM com;      // 通信对象
TTSTest test;    // 测试对象

// 报文输出模板
template <>
void output<TCAN>(TCAN* canMsg) {
    com.transmit_can_async(canMsg);
}

// 日志函数
void internal_log(const char* AFile, const char* AFunc, 
                  const s32 ALine, const TLogLevel ALevel, 
                  const char* fmt, ...) {
    // 日志实现
}

// 初始化函数
DLLEXPORT s32 __stdcall initialize_miniprogram(const PTSMasterConfiguration AConf) {
    app = AConf->FTSApp;
    com = AConf->FTSCOM;
    test = AConf->FTSTest;
    return 0;
}

// 注册小程序能力
DLLEXPORT s32 __stdcall retrieve_mp_abilities(const void* AObj, 
                                               const TRegTSMasterFunction AReg) {
    // 注册版本信息
    AReg(AObj, "check_mp_internal", "version", "2024.6.7.1126", 0, "");
    
    // 注册结构体大小
    AReg(AObj, "check_mp_internal", "struct_size", "struct_size_app", 
         (void *)sizeof(TTSMasterConfiguration), "");
    
    // 注册step函数（5ms周期）
    AReg(AObj, "step_function", "step", "5", 
         reinterpret_cast<const void*>(&step), "");
    
    // 注册on_start回调
    AReg(AObj, "on_start_callback", "on_start_NewOn_Start1", "", 
         reinterpret_cast<const void*>(&on_start_NewOn_Start1), "");
    
    return 2;
}
```

---

## 3. RPC通信架构

### 3.1 架构图

```
┌─────────────────────────────────────────────────────────────┐
│                    Python RPC Client                         │
│                      (test.py)                               │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  TSMasterAPI (Python封装)                             │   │
│  │  - initialize_lib_tsmaster()                         │   │
│  │  - rpc_tsmaster_create_client()                      │   │
│  │  - rpc_tsmaster_cmd_xxx()                            │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                           │
                           │ RPC (共享内存/TCP)
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                    TSMaster Application                      │
│                      (Server)                                │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  MiniProgram (CCode7852.mp)                          │   │
│  │  - on_start: rpc_tsmaster_activate_server(true)      │   │
│  │  - step: 5ms周期执行                                  │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  CAN RBS (剩余总线仿真)                               │   │
│  │  - CAN数据库加载                                      │   │
│  │  - 信号仿真                                           │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  System Variables                                     │   │
│  │  - Var0 (用户变量)                                    │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                           │
                           │ CAN/LIN/FlexRay
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                    硬件设备 / 仿真环境                        │
└─────────────────────────────────────────────────────────────┘
```

### 3.2 通信流程

```
1. 初始化阶段
   Python: initialize_lib_tsmaster()
   └─> 加载TSMasterAPI动态库

2. 发现阶段
   Python: get_active_application_list()
   └─> 获取运行中的TSMaster实例列表

3. 连接阶段
   Python: rpc_tsmaster_create_client()
   └─> 创建RPC客户端连接
   
   Python: rpc_tsmaster_activate_client()
   └─> 激活客户端

4. 控制阶段
   Python: rpc_tsmaster_cmd_start_simulation()
   └─> 启动仿真
   
   Python: rpc_tsmaster_cmd_write_system_var()
   └─> 写系统变量
   
   Python: rpc_tsmaster_cmd_set_can_signal()
   └─> 设置CAN信号
   
   Python: rpc_tsmaster_cmd_get_can_signal()
   └─> 读取CAN信号

5. 清理阶段
   Python: finalize_lib_tsmaster()
   └─> 释放资源
```

---

## 4. TSMasterAPI Python接口

### 4.1 核心API函数

#### 初始化函数
```python
# 初始化TSMaster库
initialize_lib_tsmaster(app_name: bytes) -> int

# 释放TSMaster库
finalize_lib_tsmaster() -> int
```

#### 应用程序管理
```python
# 获取活动应用程序列表
get_active_application_list(namelist: pchar) -> int

# 创建RPC客户端
rpc_tsmaster_create_client(app_name: bytes, rpchandle: size_t) -> int

# 激活/停用客户端
rpc_tsmaster_activate_client(rpchandle: size_t, activate: bool) -> int
```

#### 仿真控制
```python
# 启动仿真
rpc_tsmaster_cmd_start_simulation(rpchandle: size_t) -> int

# 停止仿真
rpc_tsmaster_cmd_stop_simulation(rpchandle: size_t) -> int
```

#### 系统变量操作
```python
# 写系统变量
rpc_tsmaster_cmd_write_system_var(
    rpchandle: size_t, 
    var_name: bytes, 
    value: bytes
) -> int

# 读系统变量
rpc_tsmaster_cmd_read_system_var(
    rpchandle: size_t, 
    var_name: bytes
) -> Tuple[int, bytes]
```

#### CAN信号操作
```python
# 设置CAN信号值
rpc_tsmaster_cmd_set_can_signal(
    rpchandle: size_t,
    signal_path: bytes,  # 格式: "通道/数据库/节点/报文/信号"
    value: float
) -> int

# 获取CAN信号值
rpc_tsmaster_cmd_get_can_signal(
    rpchandle: size_t,
    signal_path: bytes,
    value: double  # 输出参数
) -> int
```

### 4.2 信号路径格式

```
完整路径格式:
    "通道/数据库/节点/报文/信号"
    例如: "0/CAN_FD_Powertrain/Engine/Test_Message_CAN_FD/Test_Signal_Byte_00"

简化路径格式（TSMaster新版本支持）:
    "信号名"
    例如: "Test_Signal_Byte_01_02"
    注意: 需要开启RBS且信号名无重名
```

---

## 5. 配置文件分析

### 5.1 TSMaster.ini

主要配置项：

```ini
[TSMaster]
DebugMode=0                    # 调试模式
LogVerbose=6                   # 日志详细程度
DisplayHex=1                   # 十六进制显示
DisplaySym=1                   # 符号显示

[TsRPC]
Actv=1                         # RPC激活状态
SizeBytes=1048576             # RPC缓冲区大小

[ChannelMappings]
CANChnCount=2                  # CAN通道数
CAN0=0,0,1,0,0,-1,0,500,2000,TSMaster,default
CAN1=1,0,1,0,1,-1,0,500,2000,TSMaster,default

[UserVariables]
Count=1                        # 用户变量数量
u0=Var0,,,User,0,0,0,100,1000,,-1,,,0,-1,  # 变量定义

[CAN_Databases]
DBFilesCount=1
DBFiles0=.\DB\CAN\CAN_FD_Powertrain.pdbc  # CAN数据库
```

### 5.2 TSMasterFunctionPrototypesPython.ini

定义了Python可调用的所有API函数原型，包括：

- **app对象**: 应用程序管理、系统变量、文件操作等
- **com对象**: 通信相关（CAN/LIN/FlexRay收发）
- **test对象**: 测试相关功能

---

## 6. 关键技术点

### 6.1 RPC通信机制

TSMaster RPC使用共享内存进行进程间通信：

1. **Server端**（TSMaster小程序）：
   - 在`on_start`事件中调用`rpc_tsmaster_activate_server(true)`
   - 创建共享内存区域
   - 等待客户端连接

2. **Client端**（Python脚本）：
   - 调用`get_active_application_list()`发现Server
   - 调用`rpc_tsmaster_create_client()`创建连接
   - 通过RPC命令控制TSMaster

### 6.2 小程序机制

TSMaster小程序是运行在TSMaster内部的C代码：

```cpp
// 必须实现的导出函数
DLLEXPORT s32 __stdcall initialize_miniprogram(const PTSMasterConfiguration AConf);
DLLEXPORT s32 __stdcall finalize_miniprogram(void);
DLLEXPORT s32 __stdcall retrieve_mp_abilities(const void* AObj, const TRegTSMasterFunction AReg);
```

### 6.3 信号读写前提条件

1. **开启RBS**（剩余总线仿真）
2. **启动仿真**（调用`rpc_tsmaster_cmd_start_simulation`）
3. **正确的信号路径**

---

## 7. 与测试系统集成建议

### 7.1 TSMasterAdapter改进

基于此Demo，可以改进现有的TSMasterAdapter：

```python
from TSMasterAPI import *

class TSMasterRPCClient:
    """TSMaster RPC客户端封装"""
    
    def __init__(self, app_name: str = "TSMasterTest"):
        self.app_name = app_name
        self.rpchandle = size_t(0)
        self.connected = False
        
    def connect(self) -> bool:
        """连接到TSMaster"""
        initialize_lib_tsmaster(self.app_name.encode())
        
        namelist = pchar()
        get_active_application_list(namelist)
        app_list = namelist.value.decode().split(';')
        
        if len(app_list) > 0:
            ret = rpc_tsmaster_create_client(app_list[0].encode(), self.rpchandle)
            if ret == 0:
                rpc_tsmaster_activate_client(self.rpchandle, True)
                self.connected = True
                return True
        return False
    
    def start_simulation(self) -> bool:
        """启动仿真"""
        if self.connected:
            return rpc_tsmaster_cmd_start_simulation(self.rpchandle) == 0
        return False
    
    def set_signal(self, signal_path: str, value: float) -> bool:
        """设置信号值"""
        if self.connected:
            return rpc_tsmaster_cmd_set_can_signal(
                self.rpchandle, 
                signal_path.encode(), 
                value
            ) == 0
        return False
    
    def get_signal(self, signal_path: str) -> Optional[float]:
        """获取信号值"""
        if self.connected:
            d = double(0)
            ret = rpc_tsmaster_cmd_get_can_signal(
                self.rpchandle,
                signal_path.encode(),
                d
            )
            if ret == 0:
                return d.value
        return None
    
    def disconnect(self):
        """断开连接"""
        if self.connected:
            rpc_tsmaster_activate_client(self.rpchandle, False)
        finalize_lib_tsmaster()
```

### 7.2 集成到适配器

```python
# 在tsmaster_adapter.py中使用RPC

class TSMasterAdapter(BaseTestAdapter):
    
    def __init__(self, config: dict = None):
        super().__init__(config)
        self._rpc_client = TSMasterRPCClient()
        
    def connect(self) -> bool:
        # 优先使用RPC连接
        if self._rpc_client.connect():
            self.logger.info("通过RPC连接TSMaster成功")
            return True
        # 回退到传统方式
        return self._traditional_connect()
```

---

## 8. 总结

### 8.1 Demo核心功能

| 功能 | 实现方式 | 状态 |
|------|---------|------|
| RPC连接 | 共享内存 | ✅ |
| 仿真控制 | RPC命令 | ✅ |
| 系统变量读写 | RPC命令 | ✅ |
| CAN信号读写 | RPC命令 | ✅ |
| LIN信号读写 | RPC命令 | ⚠️ 待实现 |
| FlexRay信号读写 | RPC命令 | ⚠️ 待实现 |

### 8.2 优势

1. **低延迟**: 使用共享内存通信，延迟极低
2. **无需额外服务**: 不需要启动额外的Web服务
3. **Python友好**: 提供完整的Python API封装
4. **功能完整**: 支持信号读写、仿真控制、系统变量等

### 8.3 局限性

1. **仅限Windows**: TSMaster仅支持Windows平台
2. **需要TSMaster运行**: 必须先启动TSMaster应用程序
3. **依赖小程序**: 需要在TSMaster中加载小程序激活RPC服务器

### 8.4 与CANoe对比

| 特性 | TSMaster RPC | CANoe COM |
|------|-------------|-----------|
| 通信方式 | 共享内存 | COM接口 |
| 性能 | 高 | 中 |
| 实现复杂度 | 低 | 中 |
| 跨进程 | 是 | 是 |
| Python支持 | 原生 | 需pywin32 |

---

**文档版本**: 1.0  
**创建日期**: 2026-02-25  
**作者**: AI Assistant
