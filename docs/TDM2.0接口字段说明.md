# TDM2.0接口字段说明

本文档说明Python执行器与网络测试平台之间的数据交换字段标准，确保与TDM2.0接口保持一致。

## 一、任务推送接口字段

### 1.1 任务级别字段

| 字段名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| projectNo | String | Y | 项目编号 |
| taskNo | String | Y | 任务编号 |
| taskName | String | Y | 任务名称 |
| caseList | List<Object> | Y | 用例集合 |

### 1.2 用例级别字段（14个）

| 字段名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| moduleLevel1 | String | Y | 一级模块 |
| moduleLevel2 | String | Y | 二级模块 |
| moduleLevel3 | String | Y | 三级模块 |
| caseName | String | Y | 用例名称 |
| priority | String | Y | 优先级 |
| caseType | String | Y | 用例类型 |
| preCondition | String | Y | 前置条件 |
| stepDescription | String | Y | 步骤描述 |
| expectedResult | String | Y | 预期结果 |
| maintainer | String | Y | 维护人 |
| caseNo | String | Y | 用例编号 |
| caseSource | String | Y | 用例来源 |
| changeRecord | String | N | 用例变更记录 |
| tags | String | N | 用例标签 |

### 1.3 内部扩展字段

| 字段名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| deviceId | String | N | 设备ID |
| toolType | String | N | 测试工具类型(canoe/tsmaster/ttworkbench) |
| configPath | String | N | 配置文件路径 |
| timeout | Integer | N | 超时时间(秒)，默认3600 |

## 二、结果上报接口字段

### 2.1 任务级别字段

| 字段名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| taskNo | String | Y | 任务编号 |
| platform | String | Y | 平台名称，固定值"NETWORK" |
| caseList | List<Object> | Y | 用例执行结果集合 |

### 2.2 用例结果字段

| 字段名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| caseNo | String | Y | 用例编号 |
| result | String | Y | 结果: PASS/FAIL/BLOCK/SKIP |
| remark | String | N | 备注 |
| created | String | N | 测试执行时间(格式: YYYY-MM-DD HH:MM:SS) |
| reportPath | String | N | 报告地址 |

## 三、响应字段

### 3.1 响应字段

| 字段名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| result | String | Y | 状态码: 1=成功, 0=失败 |
| msg | String | Y | 状态描述 |
| extInfo | String | N | 扩展信息 |

## 四、数据模型类

### 4.1 Case类（用例）

```python
@dataclass
class Case:
    # 必填字段
    moduleLevel1: str = ""           # 一级模块
    moduleLevel2: str = ""           # 二级模块
    moduleLevel3: str = ""           # 三级模块
    caseName: str = ""               # 用例名称
    priority: str = ""               # 优先级
    caseType: str = ""               # 用例类型
    preCondition: str = ""           # 前置条件
    stepDescription: str = ""        # 步骤描述
    expectedResult: str = ""         # 预期结果
    maintainer: str = ""             # 维护人
    caseNo: str = ""                 # 用例编号
    caseSource: str = ""             # 用例来源
    
    # 可选字段
    changeRecord: Optional[str] = None   # 用例变更记录
    tags: Optional[str] = None           # 用例标签
```

### 4.2 Task类（任务）

```python
@dataclass
class Task:
    # TDM2.0标准字段
    projectNo: str = ""              # 项目编号
    taskNo: str = ""                 # 任务编号
    taskName: str = ""               # 任务名称
    caseList: List[Case] = field(default_factory=list)  # 用例集合
    
    # 内部使用字段
    deviceId: Optional[str] = None   # 设备ID
    toolType: Optional[str] = None   # 测试工具类型
    configPath: Optional[str] = None # 配置文件路径
    timeout: int = 3600              # 超时时间(秒)
```

### 4.3 CaseResult类（用例结果）

```python
@dataclass
class CaseResult:
    caseNo: str                          # 用例编号 (必填)
    result: str                          # 结果: PASS/FAIL/BLOCK/SKIP (必填)
    remark: Optional[str] = None         # 备注 (可选)
    created: Optional[str] = None        # 测试执行时间 (可选)
    reportPath: Optional[str] = None     # 报告地址 (可选)
```

### 4.4 ExecutionResult类（执行结果）

```python
@dataclass
class ExecutionResult:
    taskNo: str                              # 任务编号 (必填)
    caseList: List[CaseResult] = field(default_factory=list)  # 用例集合 (必填)
    platform: str = "NETWORK"                # 平台名称 (必填)
```

## 五、使用示例

### 5.1 接收任务

```python
from python_executor.models import Task

# TDM2.0格式的任务数据
task_data = {
    "projectNo": "PROJ_001",
    "taskNo": "TASK_001",
    "taskName": "测试任务",
    "caseList": [
        {
            "caseName": "用例1",
            "caseNo": "CASE_001",
            "priority": "高",
            "caseType": "功能测试",
            "expectedResult": "预期结果"
        }
    ]
}

# 创建Task对象
task = Task.from_dict(task_data)
```

### 5.2 上报结果

```python
from python_executor.models import ExecutionResult, CaseResult, CaseResultStatus

# 创建执行结果
execution_result = ExecutionResult(
    taskNo="TASK_001",
    platform="NETWORK"
)

# 添加用例结果
execution_result.add_case_result(CaseResult(
    caseNo="CASE_001",
    result=CaseResultStatus.PASS.value,
    remark="执行成功"
))

# 转换为TDM2.0格式
tdm2_format = execution_result.to_tdm2_format()
```

## 六、状态值对照

| 内部状态 | TDM2.0状态 | 说明 |
|----------|-----------|------|
| PASSED | PASS | 通过 |
| FAILED | FAIL | 失败 |
| BLOCKED | BLOCK | 阻塞 |
| SKIPPED | SKIP | 跳过 |

## 七、文件位置

- 模型定义: `python_executor/models/task.py`
- 结果定义: `python_executor/models/result.py`
- 使用示例: `examples/tdm2_example.py`
- 单元测试: `tests/unit/test_tdm2_models.py`
