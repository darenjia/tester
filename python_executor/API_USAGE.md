# HTTP API 使用指南

## 概述

Python执行器现在支持通过HTTP接口进行任务下发和结果获取，同时保持原有的WebSocket接口兼容。

**重要变更**: 所有API现在都使用 `taskNo` 作为任务的唯一标识符，不再使用内部的UUID。

## 服务端点

| 方法 | 端点 | 描述 |
|------|------|------|
| GET | `/` | 服务信息 |
| GET | `/health` | 健康检查 |
| GET | `/status` | 系统状态 |
| GET | `/api/status` | API状态 |
| POST | `/api/tasks` | 创建新任务 |
| GET | `/api/tasks` | 获取任务列表 |
| GET | `/api/tasks/{taskNo}` | 获取任务详情 |
| DELETE | `/api/tasks/{taskNo}` | 取消任务 |
| GET | `/api/tasks/{taskNo}/results` | 获取任务结果 |
| GET | `/api/tasks/{taskNo}/progress` | 获取任务进度 |

## 请求/响应示例

### 1. 创建任务

**请求：**
```bash
curl -X POST http://localhost:8180/api/tasks \
  -H "Content-Type: application/json" \
  -d '{
    "projectNo": "P001",
    "taskNo": "T001",
    "taskName": "测试任务",
    "caseList": [
      {"caseNo": "C001", "caseName": "用例1"},
      {"caseNo": "C002", "caseName": "用例2"}
    ]
  }'
```

**响应：**
```json
{
  "status": "success",
  "data": {
    "taskNo": "T001",
    "projectNo": "P001",
    "taskName": "测试任务",
    "status": "running",
    "message": "任务已创建并开始执行",
    "createdAt": "2024-01-01T12:00:00"
  },
  "timestamp": "2024-01-01T12:00:00"
}
```

### 2. 查询任务

**请求：**
```bash
curl http://localhost:8180/api/tasks/T001
```

**响应：**
```json
{
  "status": "success",
  "data": {
    "taskNo": "T001",
    "projectNo": "P001",
    "taskName": "测试任务",
    "status": "running",
    "progress": 50,
    "message": "执行中...",
    "createdAt": "2024-01-01T12:00:00",
    "startedAt": "2024-01-01T12:00:01"
  }
}
```

### 3. 获取任务结果

**请求：**
```bash
curl http://localhost:8180/api/tasks/T001/results
```

**响应：**
```json
{
  "status": "success",
  "data": {
    "taskNo": "T001",
    "platform": "NETWORK",
    "caseList": [
      {
        "caseNo": "C001",
        "result": "PASS",
        "remark": "用例 C001 执行成功"
      },
      {
        "caseNo": "C002",
        "result": "FAIL",
        "remark": "用例 C002 执行失败"
      }
    ],
    "status": "completed",
    "progress": 100,
    "summary": {
      "total": 2,
      "passed": 1,
      "failed": 1,
      "passRate": "50.0%"
    }
  }
}
```

### 4. 获取任务进度

**请求：**
```bash
curl http://localhost:8180/api/tasks/T001/progress
```

**响应：**
```json
{
  "status": "success",
  "data": {
    "taskNo": "T001",
    "status": "running",
    "progress": 50,
    "message": "执行用例 C002 (2/3)",
    "elapsedTime": 2.5
  }
}
```

### 5. 取消任务

**请求：**
```bash
curl -X DELETE http://localhost:8180/api/tasks/T001
```

**响应：**
```json
{
  "status": "success",
  "message": "任务已取消",
  "timestamp": "2024-01-01T12:00:03"
}
```

### 6. 获取任务列表

**请求：**
```bash
# 获取所有任务
curl http://localhost:8180/api/tasks

# 分页查询
curl "http://localhost:8180/api/tasks?page=1&pageSize=10"

# 按状态筛选
curl "http://localhost:8180/api/tasks?status=completed"
```

**响应：**
```json
{
  "status": "success",
  "data": {
    "tasks": [
      {
        "taskNo": "T001",
        "projectNo": "P001",
        "taskName": "测试任务",
        "status": "completed",
        "progress": 100,
        "createdAt": "2024-01-01T12:00:00"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 20,
    "totalPages": 1
  }
}
```

## 错误响应

所有错误响应都遵循以下格式：

```json
{
  "status": "error",
  "code": "ERROR_CODE",
  "message": "错误描述信息",
  "timestamp": "2024-01-01T12:00:00"
}
```

### 常见错误码

| HTTP状态码 | 错误码 | 描述 |
|------------|--------|------|
| 400 | MISSING_FIELDS | 缺少必填字段 |
| 400 | INVALID_REQUEST | 无效请求 |
| 400 | INVALID_STATUS | 无效的状态值 |
| 404 | TASK_NOT_FOUND | 任务不存在 |
| 404 | NOT_FOUND | 接口不存在 |
| 409 | TASK_ALREADY_EXISTS | 任务已存在（正在执行） |
| 409 | INVALID_TASK_STATUS | 无效的任务状态 |
| 503 | TASK_QUEUE_FULL | 任务队列已满 |
| 500 | INTERNAL_ERROR | 服务器内部错误 |

## 与WebSocket接口的对比

| 功能 | WebSocket | HTTP API |
|------|-----------|----------|
| 任务下发 | `task_dispatch`事件 | `POST /api/tasks` |
| 任务查询 | `task_query`事件 | `GET /api/tasks/{taskNo}` |
| 进度获取 | 实时推送 | `GET /api/tasks/{taskNo}/progress` |
| 结果获取 | `task_completed`事件 | `GET /api/tasks/{taskNo}/results` |
| 任务取消 | `task_cancel`事件 | `DELETE /api/tasks/{taskNo}` |

## 使用建议

1. **任务下发**：使用HTTP API提交任务，使用你提供的taskNo
2. **进度跟踪**：可以轮询`GET /api/tasks/{taskNo}/progress`或使用WebSocket接收实时推送
3. **结果获取**：任务完成后使用`GET /api/tasks/{taskNo}/results`获取结果
4. **错误处理**：根据HTTP状态码和错误码进行相应处理

## Python示例代码

```python
import requests
import time

# 配置
BASE_URL = "http://localhost:8180"
API_URL = f"{BASE_URL}/api"

# 1. 创建任务
task_data = {
    "projectNo": "P001",
    "taskNo": "T001",  # 使用你自己的taskNo
    "taskName": "测试任务",
    "caseList": [
        {"caseNo": "C001", "caseName": "用例1"},
        {"caseNo": "C002", "caseName": "用例2"}
    ]
}

response = requests.post(f"{API_URL}/tasks", json=task_data)
result = response.json()
task_no = result['data']['taskNo']  # 返回你提供的taskNo
print(f"任务创建成功: {task_no}")

# 2. 轮询任务进度
while True:
    response = requests.get(f"{API_URL}/tasks/{task_no}/progress")
    progress = response.json()['data']
    
    print(f"进度: {progress['progress']}%, 状态: {progress['status']}")
    
    if progress['status'] in ['completed', 'failed', 'cancelled']:
        break
    
    time.sleep(1)

# 3. 获取结果
response = requests.get(f"{API_URL}/tasks/{task_no}/results")
results = response.json()['data']
print(f"执行结果: {results}")
```

## 重要提示

- 所有API端点现在都使用 `{taskNo}` 作为路径参数，而不是之前的 `{taskId}`
- taskNo 在创建任务时由调用方提供，需要确保唯一性
- 如果尝试创建具有相同taskNo且正在执行的任务，将返回 409 错误
