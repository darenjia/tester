# 内部任务模型统一与执行流水线收口设计

- 日期：2026-04-02
- 分支：`codex/remediation-hardening`
- 范围：统一内部执行链路的任务模型与执行主流程，不改外部 API 协议与持久化展示模型

## 1. 背景与目标

当前项目已经完成了生产止血、配置系统统一、执行可观测性重构三轮整改，但内部执行链路仍然存在多种任务表示并存的问题：

- 平台入口消息和 `models.task.Task` 混在 `main_production.py` 中直接组装
- `TaskExecutorProduction` 同时承担任务解释、执行调度、结果上报
- `models.task.Task` 为兼容执行器暴露了大量 `task_id`、`tool_type`、`test_items` 等桥接属性
- 队列层仍保留 `models.executor_task.Task` 作为查询和持久化模型

这些问题虽然暂时没有阻断生产，但会持续带来几个隐患：

- 执行器边界不清晰，入口层和执行层职责混杂
- 新增能力时容易继续把平台字段、执行字段、展示字段揉在一起
- 任务执行主流程不够显式，重试、补偿、审计演进成本高

本阶段目标是：

1. 统一内部执行链路，只保留一套内部执行模型
2. 把平台任务解析与执行器运行彻底解耦
3. 固化任务编译与执行流水线
4. 保持外部 API 和持久化任务模型不做大改，降低本轮风险

## 2. 非目标

本阶段明确不做以下事项：

- 不统一外部 API 返回结构
- 不重写 `models.executor_task.Task` 和任务看板/持久化语义
- 不修改 TDM2.0 平台消息协议
- 不在本阶段重构适配器内部实现细节
- 不把 observability、metrics、status API 再做第二轮大改

## 3. 目标架构

本阶段将内部执行链路收成三层职责：

1. 平台入口层
   - 接收 WebSocket message / payload
   - 解析成平台任务输入
   - 调用编译器产出内部执行计划

2. 任务编译层
   - 校验任务结构
   - 解析 `toolType`
   - 从映射库补全 case 信息
   - 决议配置来源
   - 产出单一 `ExecutionPlan`

3. 执行运行层
   - 仅消费 `ExecutionPlan`
   - 执行固定生命周期
   - 负责执行、采集、上报、收尾

队列与状态查询层继续保留现有 `models.executor_task.Task`，但它不再承担内部执行模型职责。

## 4. 数据模型设计

### 4.1 PlatformTask

`PlatformTask` 不是本阶段新增的长期模型，可以继续由现有入口解析结果承担。它代表“平台发送过来的任务长什么样”，停留在入口层，不进入执行器深处。

它的来源仍然是：

- `Message`
- WebSocket payload
- `taskNo` / `deviceId`
- TDM2.0 原始字段

### 4.2 ExecutionPlan

`ExecutionPlan` 是本阶段新增的内部唯一执行模型，也是执行器的唯一输入。

它表达的是：

“这个平台任务经过验证、映射解析、配置决议之后，最终应该如何执行。”

建议字段分成四组。

#### 身份字段

- `task_no`
- `project_no`
- `task_name`
- `device_id`

#### 解析结果

- `tool_type`
- `cases`
- `config_path`
- `config_name`
- `base_config_dir`
- `variables`
- `canoe_namespace`

#### 执行约束

- `timeout_seconds`
- `max_concurrency`
- `retry_policy`
- `report_required`

#### 补充策略

- `config_source`
  - 取值建议：`direct_path` / `config_manager` / `case_mapping` / `tsmaster_inline`
- `resolution_notes`
  - 记录编译阶段关键决议，供调试和审计使用
- `raw_refs`
  - 可选，仅保留少量平台关联字段，不保留整份原始 payload

### 4.3 PlannedCase

`ExecutionPlan.cases` 不建议继续直接使用当前面向 TDM2.0 展示字段的 `Case`。本阶段应引入偏执行视角的 `PlannedCase`，只保留执行真正需要的信息：

- `case_no`
- `case_name`
- `case_type`
- `repeat`
- `dtc_info`
- `execution_params`
- `mapping_metadata`

这样可以避免内部执行模型继续绑死在平台协议字段上。

### 4.4 ExecutionRecord

本阶段不强制新增一个完整的 `ExecutionRecord` 大模型。执行过程状态、错误、阶段耗时、上报状态等信息，先继续由：

- `core.execution_observability`
- 队列状态
- result collector

共同承担。

它和 `ExecutionPlan` 的边界必须清晰：

- `ExecutionPlan` 只存“执行前已确定信息”
- `ExecutionRecord` 负责“执行过程中实际发生了什么”

## 5. 任务编译器设计

在入口层和执行器之间新增 `TaskCompiler`，作为内部模型统一的核心边界。

`TaskCompiler` 的职责不是执行任务，而是把平台任务编译成 `ExecutionPlan`。

编译流程固定为五步：

1. `normalize`
   - 合并 message 字段与 payload
   - 处理命名差异和兼容别名

2. `validate`
   - 校验任务结构
   - 校验 `testItems` 非空
   - 校验 `toolType` 是否唯一
   - 校验映射可解析

3. `resolve`
   - 确定最终 `tool_type`
   - 从映射库补全 case 信息
   - 决议 `config_path` / `config_name` / `config_source`

4. `plan`
   - 生成 `ExecutionPlan`
   - 补齐默认超时、执行约束、上报策略

5. `handoff`
   - 交给执行器 `execute_plan()`

任何 `validate` 或 `resolve` 阶段失败，都不进入执行器。

## 6. 执行器边界调整

`TaskExecutorProduction` 在本阶段新增 `execute_plan()`，作为新的主执行入口。

目标状态如下：

- `main_production.py` 只负责入口处理和调用编译器
- `TaskExecutorProduction` 只负责运行 `ExecutionPlan`
- `models.task.Task` 不再作为生产主链路的执行器输入

为降低迁移风险，可以短期保留 `execute_task()`，但它只作为兼容入口，内部应尽快转换为 `ExecutionPlan` 后再继续执行，而不是保留两套主逻辑。

## 7. 执行流水线

内部执行流水线固定为：

`received -> compiled -> queued -> preparing -> executing -> collecting -> reporting -> finished`

各阶段职责如下：

- `received`
  - 任务到达入口

- `compiled`
  - `TaskCompiler` 编译完成，得到 `ExecutionPlan`

- `queued`
  - 任务进入内部执行队列

- `preparing`
  - 创建适配器、准备配置、连接工具

- `executing`
  - 真正执行测试

- `collecting`
  - 收集结果、报告文件、摘要信息

- `reporting`
  - 向远端上报

- `finished`
  - 成功、失败或取消后的统一收尾状态

第三阶段已经完成的 observability 可以继续复用，但本阶段需要补齐 `compiled` 和 `collecting` 两个真实阶段，使阶段定义与代码结构完全一致。

## 8. 渐进迁移策略

本阶段采用“渐进替换，但不长期双轨共存”的方式：

1. 新增 `ExecutionPlan` 与 `TaskCompiler`
2. 给 `TaskExecutorProduction` 新增 `execute_plan()`
3. 把 `main_production.py` 切到 `TaskCompiler -> ExecutionPlan -> execute_plan()`
4. 清理执行器内部对 `models.task.Task` 兼容属性的依赖
5. 补自动化测试并删除不再需要的内部兼容逻辑

这里的关键不是“永远兼容两套入口”，而是“短期兼容，完成迁移后删掉旧内部路径”。

## 9. 风险与控制

### 风险 1：执行器改造影响现有生产链路

控制方式：

- 先保留兼容入口，再切主链路
- 用自动化测试覆盖编译失败、映射解析、上报和执行主链路

### 风险 2：内部模型统一顺手扩散到外部协议

控制方式：

- 明确本阶段只统一内部执行模型
- 不改 API 协议字段，不改队列展示字段

### 风险 3：阶段定义与现有 observability 不一致

控制方式：

- 本阶段同步更新阶段定义
- 所有关键阶段均纳入 observability 和测试

## 10. 测试策略

本阶段至少补以下自动化保护：

1. `TaskCompiler` 测试
   - 空任务拒绝
   - 多工具任务拒绝
   - 映射补全成功
   - 配置来源决议正确

2. `ExecutionPlan` 测试
   - 平台任务编译后字段完整
   - 默认执行约束补齐

3. 执行器主链路测试
   - `execute_plan()` 成功路径
   - `prepare`/`execute`/`report` 失败路径
   - 观测阶段流转正确

4. 集成测试
   - `main_production.py` 通过编译器把任务送入执行器
   - 队列状态和执行状态依旧可查询

## 11. 交付标准

本阶段完成的判断标准为：

1. 生产主链路不再直接构造执行器使用的 `models.task.Task`
2. `TaskExecutorProduction` 主流程只消费 `ExecutionPlan`
3. `TaskCompiler` 成为入口层与执行层之间的固定边界
4. 内部执行流水线固定为：
   - `received -> compiled -> queued -> preparing -> executing -> collecting -> reporting -> finished`
5. 全量 `pytest` 继续通过

## 12. 实施建议顺序

建议按以下顺序落地：

1. 新增 `ExecutionPlan` / `PlannedCase`
2. 新增 `TaskCompiler`
3. 为执行器增加 `execute_plan()`
4. 切换 `main_production.py`
5. 更新 observability 阶段
6. 清理旧内部兼容路径
7. 补测试与文档

这个顺序可以把风险集中在内部链路，避免同时扰动外部接口与持久化查询模型。
