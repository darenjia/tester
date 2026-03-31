# TSMaster RPC 调用流程修复文档

## 1. 背景

根据 `DFVehNET使用说明v2.3.docx` 文档中第四章 "RPC 调用" 的规范，TSMaster 执行器的 RPC 调用流程缺少关键步骤。文档明确指出需要按照特定顺序调用 API 才能正确控制 TSMaster。

## 2. 问题分析

### 文档规定的 RPC 调用流程

```
1. initialize_lib_tsmaster()        # 初始化库
2. get_active_application_list()   # 获取运行中的 TSMaster 进程
3. rpc_tsmaster_create_client()     # 创建 RPC 客户端
4. rpc_tsmaster_activate_client()   # 激活客户端
5. app.run_form                    # 启动 Master 小程序 ⚠️ 原实现缺失
6. start_simulation()              # 启动仿真
7. TestSystem.SelectCases          # 选择测试用例
8. TestSystem.Controller = 1       # 开始测试
9. TestSystem.Controller = 0       # 停止测试
10. stop_simulation()               # 停止仿真
11. app.stop_form                   # 停止 Master 小程序 ⚠️ 原实现缺失
12. finalize_lib_tsmaster()        # 释放资源
```

### 原实现的问题

| 问题 | 说明 |
|------|------|
| 缺少 `app.run_form` | 没有显式启动 Master 小程序 |
| 缺少 `app.stop_form` | 没有显式停止 Master 小程序 |
| 流程顺序不正确 | 应该在启动仿真前先启动小程序 |

## 3. 修改内容

### 3.1 TestExecutionConfig 新增字段

**文件:** `core/tsmaster_test_engine.py`

```python
@dataclass
class TestExecutionConfig:
    # ... 其他字段 ...
    master_form_name: str = "C 代码编辑器 [Master]"  # Master小程序名称
```

### 3.2 TSMasterAdapter 新增方法

**文件:** `core/adapters/tsmaster_adapter.py`

```python
def __init__(self, config: dict = None):
    # ... 其他初始化 ...
    self.master_form_name = self.config.get("master_form_name", "C 代码编辑器 [Master]")
    self._master_form_started = False

def start_master_form(self, form_name: str = None) -> bool:
    """启动 Master 小程序 (app.run_form)"""
    # RPC 调用: rpc_tsmaster_call_system_api(rpchandle, "app.run_form", ...)

def stop_master_form(self, form_name: str = None) -> bool:
    """停止 Master 小程序 (app.stop_form)"""
    # RPC 调用: rpc_tsmaster_call_system_api(rpchandle, "app.stop_form", ...)
```

### 3.3 TSMasterTestEngine.connect() 修改

**文件:** `core/tsmaster_test_engine.py`

```python
def connect(self, config: TestExecutionConfig) -> bool:
    # ... 连接适配器 ...

    # RPC调用流程: 启动Master小程序 (新增)
    self.logger.info(f"启动Master小程序: {config.master_form_name}")
    self.adapter.start_master_form(config.master_form_name)

    # 启动仿真
    if config.auto_start_simulation:
        self.adapter.start_test()
```

### 3.4 TSMasterTestEngine.disconnect() 修改

**文件:** `core/tsmaster_test_engine.py`

```python
def disconnect(self):
    if self.adapter:
        # RPC调用流程: 停止Master小程序 (新增)
        self.logger.info("正在停止Master小程序...")
        self.adapter.stop_master_form()

        # 断开连接
        self.adapter.disconnect()
```

## 4. 修复后的 RPC 调用流程

| 步骤 | 函数 | 说明 |
|------|------|------|
| 1 | `initialize_lib_tsmaster()` | 初始化库 |
| 2 | `get_active_application_list()` | 获取进程列表 |
| 3 | `rpc_tsmaster_create_client()` | 创建客户端 |
| 4 | `rpc_tsmaster_activate_client()` | 激活客户端 |
| 5 | `start_master_form()` → `app.run_form` | **启动 Master 小程序** |
| 6 | `start_simulation()` | 启动仿真 |
| 7 | `TestSystem.SelectCases` | 选择用例 |
| 8 | `TestSystem.Controller = 1` | 开始测试 |
| 9 | `TestSystem.Controller = 0` | 停止测试 |
| 10 | `stop_simulation()` | 停止仿真 |
| 11 | `stop_master_form()` → `app.stop_form` | **停止 Master 小程序** |
| 12 | `finalize_lib_tsmaster()` | 释放资源 |

## 5. 新增文件

### 5.1 使用示例

**文件:** `tests/manual/tsmaster_rpc_flow_example.py`

提供了三个完整的示例：

1. **example_1_basic_rpc_flow()** - 基础 RPC 调用流程
2. **example_2_use_test_engine()** - 使用 TSMasterTestEngine
3. **example_3_direct_rpc_client()** - 直接使用 RPC 客户端

**运行方式:**
```bash
cd python_executor
python tests/manual/tsmaster_rpc_flow_example.py
```

### 5.2 更新的检测脚本

**文件:** `tests/manual/check_tsmaster_rpc.py`

新增检测项：
- `start_master_form` 方法检测
- `stop_master_form` 方法检测
- 小程序控制功能检测

## 6. 使用示例

### 6.1 基础用法

```python
from core.adapters.tsmaster_adapter import TSMasterAdapter

# 创建适配器
config = {
    "use_rpc": True,
    "master_form_name": "C 代码编辑器 [Master]"
}
adapter = TSMasterAdapter(config)

# 连接并自动启动 Master 小程序
adapter.connect()

# ... 执行测试 ...

# 断开连接并自动停止 Master 小程序
adapter.disconnect()
```

### 6.2 使用 TestExecutionConfig

```python
from core.tsmaster_test_engine import TSMasterTestEngine, TestExecutionConfig

config = TestExecutionConfig(
    use_rpc=True,
    master_form_name="C 代码编辑器 [Master]",
    auto_start_simulation=True,
    auto_stop_simulation=True
)

engine = TSMasterTestEngine()
engine.connect(config)

# ... 执行测试步骤 ...

engine.disconnect()
```

### 6.3 手动控制小程序

```python
# 手动启动/停止 Master 小程序
adapter.start_master_form("C 代码编辑器 [Master]")
# ... 执行操作 ...
adapter.stop_master_form("C 代码编辑器 [Master]")
```

## 7. 配置参数说明

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `use_rpc` | bool | True | 是否使用 RPC 模式 |
| `rpc_app_name` | str | None | 指定连接的 TSMaster 应用名 |
| `fallback_to_traditional` | bool | True | RPC 失败时是否回退到传统模式 |
| `start_timeout` | int | 30 | 启动超时时间(秒) |
| `stop_timeout` | int | 10 | 停止超时时间(秒) |
| `master_form_name` | str | "C 代码编辑器 [Master]" | Master 小程序名称 |

## 8. 注意事项

1. **Master 小程序名称**: 必须与 TSMaster 中显示的名称完全一致，包括中英文和空格
2. **RPC 模式优先**: 建议使用 RPC 模式，传统模式不支持 `run_form`/`stop_form` 调用
3. **资源释放**: 使用 `disconnect()` 方法会自动停止 Master 小程序和释放资源
4. **TSMasterAPI 安装**: RPC 模式需要安装 `TSMasterAPI` 包

## 9. 相关文件清单

| 文件 | 用途 |
|------|------|
| `core/adapters/tsmaster_adapter.py` | TSMaster 适配器 |
| `core/adapters/tsmaster/rpc_client.py` | RPC 客户端封装 |
| `core/tsmaster_test_engine.py` | 测试执行引擎 |
| `tests/manual/tsmaster_rpc_flow_example.py` | 使用示例 |
| `tests/manual/check_tsmaster_rpc.py` | 实现检测脚本 |

## 10. 更新日志

- **2026-03-30**: 初始修复
  - 添加 `master_form_name` 配置参数
  - 添加 `start_master_form()` 方法
  - 添加 `stop_master_form()` 方法
  - 更新 `connect()` 和 `disconnect()` 流程
  - 添加使用示例文档
