# 测试平台 - 测试模块

## 概述

本测试模块提供完整的测试覆盖，包括单元测试、集成测试和功能测试，确保测试平台远程执行任务模块的正确性和稳定性。

## 测试结构

```
tests/
├── conftest.py                    # pytest配置和共享fixture
├── run_all_tests.py               # 测试运行入口
├── requirements_test.txt          # 测试依赖
├── README.md                      # 本文件
├── unit/                          # 单元测试
│   ├── adapters/
│   │   ├── test_base_adapter.py   # 适配器基类测试 (194行)
│   │   └── test_ttworkbench_adapter.py # TTworkbench适配器测试 (361行)
│   └── test_task_executor.py      # 任务执行引擎测试 (272行)
├── integration/                   # 集成测试
│   └── test_adapter_factory.py    # 适配器工厂测试 (76行)
└── fixtures/                      # 测试数据
    └── configs/                   # 测试配置文件
```

## 快速开始

### 1. 安装依赖

```bash
cd d:\Deng\can_test
pip install -r tests/requirements_test.txt
```

### 2. 运行所有测试

```bash
python tests/run_all_tests.py
```

### 3. 运行特定类型测试

```bash
# 仅运行单元测试
python tests/run_all_tests.py --type unit

# 仅运行集成测试
python tests/run_all_tests.py --type integration

# 安静模式
python tests/run_all_tests.py --quiet
```

### 4. 使用pytest直接运行

```bash
# 运行所有测试
pytest tests/ -v

# 运行单元测试
pytest tests/unit -v -m unit

# 运行集成测试
pytest tests/integration -v -m integration

# 生成覆盖率报告
pytest tests/ --cov=python_executor --cov-report=html
```

## 测试类型说明

### 单元测试 (Unit Tests)

测试单个组件的功能，使用Mock隔离依赖。

- **适配器基类测试**: 测试状态管理、错误处理、配置验证
- **TTworkbench适配器测试**: 测试命令构建、执行流程、结果处理
- **任务执行引擎测试**: 测试任务调度、状态管理、消息发送

运行单元测试：
```bash
pytest tests/unit -v
```

### 集成测试 (Integration Tests)

测试多个组件的协作，验证整体流程。

- **适配器工厂测试**: 测试适配器创建、注册表管理

运行集成测试：
```bash
pytest tests/integration -v
```

### 功能测试 (Functional Tests)

测试完整功能流程，需要真实环境（可选）。

运行功能测试：
```bash
pytest tests/functional -v -m functional
```

## 测试标记

使用pytest标记组织测试：

- `@pytest.mark.unit` - 单元测试
- `@pytest.mark.integration` - 集成测试
- `@pytest.mark.functional` - 功能测试
- `@pytest.mark.canoe` - CANoe相关测试
- `@pytest.mark.tsmaster` - TSMaster相关测试
- `@pytest.mark.ttworkbench` - TTworkbench相关测试
- `@pytest.mark.slow` - 耗时较长的测试

运行特定标记的测试：
```bash
pytest -v -m ttworkbench
```

## 测试报告

运行测试后会生成以下报告：

1. **控制台输出**: 彩色显示测试结果
2. **JSON报告**: `tests/test_report.json`
3. **覆盖率报告**: 控制台显示代码覆盖率

## 系统健康检查

测试运行器会自动检查系统健康状态：

- Python版本检查 (需要3.7+)
- 依赖包检查
- 测试通过率检查 (>90%为健康)
- 错误数量检查

## 测试数据

### Fixture

`conftest.py` 提供了丰富的fixture：

- `mock_message_sender` - Mock消息发送函数
- `task_executor` - 任务执行引擎实例
- `sample_task_config` - 示例任务配置
- `sample_ttworkbench_task` - TTworkbench示例任务
- `mock_canoe_adapter` - Mock CANoe适配器
- `mock_tsmaster_adapter` - Mock TSMaster适配器
- `mock_ttworkbench_adapter` - Mock TTworkbench适配器
- `sample_test_items` - 示例测试项列表
- `temp_config_file` - 临时配置文件

### 使用Fixture

```python
def test_example(task_executor, sample_task_config):
    """使用fixture的示例测试"""
    result = task_executor.execute_task(sample_task_config)
    assert result is True
```

## 编写新测试

### 1. 创建测试文件

在对应目录创建测试文件，命名规范：`test_*.py`

### 2. 编写测试函数

```python
import pytest

def test_new_feature(mock_message_sender):
    """测试新功能"""
    # 准备
    executor = TaskExecutorV2(mock_message_sender)
    
    # 执行
    result = executor.do_something()
    
    # 验证
    assert result is True
    mock_message_sender.assert_called()
```

### 3. 添加标记

```python
@pytest.mark.unit
@pytest.mark.ttworkbench
def test_ttworkbench_feature():
    """TTworkbench功能测试"""
    pass
```

## 最佳实践

1. **独立性**: 每个测试应该独立运行，不依赖其他测试
2. **可重复性**: 测试应该可以重复运行，结果一致
3. **快速性**: 单元测试应该快速执行
4. **清晰性**: 测试名称应该清晰描述测试内容
5. **Mock使用**: 使用Mock隔离外部依赖

## 故障排除

### 测试依赖未安装

```bash
pip install -r tests/requirements_test.txt
```

### Python版本过低

需要Python 3.7或更高版本：
```bash
python --version
```

### 测试发现失败

检查测试文件命名是否符合规范：`test_*.py`

### 覆盖率报告未生成

确保安装了pytest-cov：
```bash
pip install pytest-cov
```

## 持续集成

在CI/CD中运行测试：

```yaml
# .github/workflows/test.yml 示例
name: Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Set up Python
        uses: actions/setup-python@v2
        with:
          python-version: 3.9
      - name: Install dependencies
        run: |
          pip install -r tests/requirements_test.txt
      - name: Run tests
        run: |
          python tests/run_all_tests.py --type all
```

## 统计

- **单元测试文件**: 3个
- **集成测试文件**: 1个
- **总测试代码**: 约900行
- **测试覆盖率**: 目标 >80%

## 更新日志

### v1.0 (2026-02-04)
- 初始版本
- 适配器基类测试
- TTworkbench适配器测试
- 任务执行引擎测试
- 适配器工厂集成测试
- 测试运行入口
