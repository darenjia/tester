# Python执行器

测试平台远程执行任务的Python端实现

## 项目结构

```
python_executor/
├── main.py                   # 主应用入口（标准版）
├── main_production.py        # 生产环境入口（推荐）
├── app.py                    # Flask WebSocket服务端入口（简化版）
├── config/
│   ├── settings.py          # 统一配置 facade
│   ├── config_manager.py    # 统一配置 facade
│   └── unified_config.py    # 统一配置真源（读取根目录 config.json）
├── core/
│   ├── execution_plan.py          # 内部执行模型（ExecutionPlan / PlannedCase）
│   ├── task_compiler.py           # 平台任务编译器
│   ├── task_executor_production.py # 任务执行引擎（生产环境版）
│   ├── execution_strategies/      # 工具执行策略
│   ├── adapters/            # 测试工具适配器
│   │   ├── adapter_factory.py    # 适配器工厂
│   │   ├── capabilities.py       # capability 定义
│   │   ├── canoe/adapter.py      # CANoe适配器
│   │   ├── tsmaster_adapter.py   # TSMaster适配器
│   │   └── ttworkbench_adapter.py # TTworkbench适配器
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

- ✅ **多工具支持**：支持CANoe、TSMaster和TTworkbench
- ✅ **WebSocket通信**：实时双向通信，支持断线重连
- ✅ **状态机管理**：完整的任务状态流转控制
- ✅ **多线程执行**：支持任务取消和超时控制
- ✅ **实时上报**：执行进度、日志、结果实时推送
- ✅ **异常处理**：完善的异常捕获和错误上报机制
- ✅ **配置管理**：灵活的配置文件支持
- ✅ **日志系统**：分级日志，支持上下文追踪
- ✅ **内部执行模型统一**：`TaskCompiler -> ExecutionPlan -> execute_plan()`
- ✅ **业务可观测性**：生命周期、结构化日志、业务健康摘要

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
- HTTP API 链路也复用 `TaskCompiler -> ExecutionPlan`
- 外部 API / 队列展示模型继续独立保留

适配器边界目前已经收成：

- `core.adapters.adapter_factory` 是唯一工厂真源
- `core.adapters` 包级导出只保留 raw adapter 入口
- `TaskExecutorProduction` 通过 `ExecutionStrategySelector` 选择工具策略
- CANoe 当前只保留 `test_module` 执行路径
- TSMaster 通过 `tsmaster_execution` / `project_control` capability 执行
- TTworkbench 通过 `ttworkbench_execution` capability 执行 clf / batch 任务
- `AdapterWrapper` 已删除，主路径只保留 `raw adapter + strategy + capability`

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
- `/api/runtime/preflight`：返回发布前自检结果，状态分为 `ready / warning / blocked`
- `/api/runtime/diagnose`：返回运行时诊断摘要，聚合服务、队列、失败报告和业务指标
- `/api/runtime/housekeeping`：执行低风险运维清理，补齐关键目录并清理过期失败报告

运维辅助入口：

- `python scripts/preflight_check.py`：输出发布前自检 JSON 摘要
- `python scripts/runtime_diagnose.py`：输出当前运行诊断 JSON 摘要
- `python scripts/runtime_housekeeping.py`：执行一次低风险 housekeeping 并输出摘要
- [docs/release_checklist.md](/C:/Users/deng/.codex/worktrees/can_test/codex-release-runtime-ops/python_executor/docs/release_checklist.md)：发布前检查清单
- [docs/rollback_checklist.md](/C:/Users/deng/.codex/worktrees/can_test/codex-release-runtime-ops/python_executor/docs/rollback_checklist.md)：回滚检查清单

前端运维视图：

- `/system-check`：现在优先展示 runtime ops 诊断页，聚合 preflight、diagnose、housekeeping 和发布/回滚提示，同时保留详细检测能力
- `/report-status`：展示失败报告列表、结构化详情和 `attempt-history`，支持单条/批量重试
- `/report-status/<report_id>/view`：独立失败报告详情页，聚合 `ExecutionOutcome` 摘要、attempt-history 和原始 payload
- `/tasks`：任务详情统一为摘要、执行、结果、日志、诊断五层结构，列表入口默认跳转独立详情页
- `/tasks/<task_id>/view`：独立任务详情页，统一展示时间线、测试结果、最近日志、诊断上下文和报告重试上下文
- `/logs`：运行日志页已升级为“日志流 + 运行摘要 + 联动入口”，会同步展示 scheduler / executor 状态
- `/settings`：设置页顶部新增运行总览层，先展示 preflight、diagnose、上报链路和缓存状态，再进入具体配置分区

## 开发文档

详见各模块源码中的docstring说明。
