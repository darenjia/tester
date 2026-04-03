# CANoe 批量执行与 SelectInfo.ini 生成设计

## 背景

CANoe 测试执行当前存在两个问题：

1. **用例未合并执行**：当前 `canoe_strategy.py` 逐个调用 `execute_module`，每个 case 单独启动一次测量，效率低。
2. **SelectInfo.ini 未生成**：CANoe 需要通过 SelectInfo.ini 文件获知要执行哪些用例，但当前执行前未生成该文件。

## 设计目标

1. 按 `config_path`（.cfg 文件路径）分组用例，同组用例一次测量执行
2. 在测量启动前生成 SelectInfo.ini，包含该组所有 case_no

## 设计方案（方案 A：最小改动）

### 改动范围

- `core/execution_strategies/canoe_strategy.py` — 主逻辑修改
- `core/config_manager.py` — 增强 `_generate_select_info_ini` 公开调用接口

### 核心流程

```
plan.cases
    ↓ 按 config_path 分组
对每组：
    1. 生成 SelectInfo.ini（包含该组所有 case_no）→ 写入 cfg 所在目录
    2. 加载 .cfg 配置
    3. 启动测量
    4. 依次执行该组所有 TestModule
    5. 停止测量
```

### 分组逻辑

执行计划中的用例按 `config_path` 字段分组：
- 拥有相同 `config_path` 的 case 属于同一批次
- 写入 SelectInfo.ini 时，将该组所有 case_no 按 `case_no=1` 格式写入

### SelectInfo.ini 格式

文件路径：`{cfg文件所在目录}/SelectInfo.ini`

内容格式：
```ini
[CFG_PARA]
CANOE-001=1
CANOE-002=1
CANOE-003=1
```

### 实现要点

#### 1. `canoe_strategy.py` 修改

**新增 `_generate_select_info_ini_for_group` 方法**：
- 输入：一组 cases 和 config_path
- 输出：将 SelectInfo.ini 写入 cfg 所在目录

**新增 `_group_cases_by_config_path` 方法**：
- 将 plan.cases 按 config_path 分组
- 返回 `dict[str, list[PlannedCase]]`

**修改 `run` 方法**：
- 执行前分组
- 对每组：先生成 SelectInfo.ini，再执行测量
- 由于 CANoe 一次测量内可以依次执行多个 TestModule，同组用例在一次测量内完成

#### 2. `config_manager.py` 修改

增强 `_generate_select_info_ini` 或提供新的公开方法 `generate_select_info_for_cases(case_nos, output_dir)`，支持外部调用。

### CANoe 执行行为确认

CANoe 在 `start_measurement()` 后会依次执行配置中启用的 TestModule（通过 SelectInfo.ini 选择）。因此同一 config_path 下的用例可以在一次测量中全部执行完毕。

### 错误处理

- 若 SelectInfo.ini 生成失败：任务标记为 ERROR，停止执行
- 若加载 .cfg 失败：任务标记为 ERROR，停止执行
- 若测量启动失败：任务标记为 RUNTIME ERROR，停止执行
- 单个用例执行失败：记录失败 verdict，继续执行下一个用例
