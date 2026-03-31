# TTworkbench适配器完善实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 完善TTworkbench适配器，实现编译、日志解析、正确返回码判断和前端两级结果展示

**Architecture:** 基于适配器模式扩展，CaseMapping增加TTworkbench专用字段，适配器增加编译和TLZ解析功能，前端增加两级展示

**Tech Stack:** Python 3.10+, Flask, HTML/CSS/JavaScript

---

## 文件结构

| 文件 | 职责 |
|------|------|
| `models/case_mapping.py` | 增加TTworkbench专用字段 |
| `core/adapters/ttworkbench_adapter.py` | 完善编译、日志解析、执行逻辑 |
| `web/templates/case_mapping.html` | 增加TTworkbench字段表单 |
| `web/templates/tasks.html` | 增加两级结果展示 |
| `api/case_mapping_api.py` | 适配新增字段的API |

---

## 任务分解

### Task 1: 扩展CaseMapping模型 - 增加TTworkbench专用字段

**Files:**
- Modify: `python_executor/models/case_mapping.py:19-83`

- [ ] **Step 1: 在CaseMapping类中添加TTworkbench专用字段**

```python
@dataclass
class CaseMapping:
    """用例映射模型

    用于建立接口用例名称与脚本Case编号之间的对应关系
    """
    case_no: str = ""                    # 脚本Case编号 (如 "CANOE-001")
    case_name: str = ""                  # 接口用例名称 (如 "CANoe安装路径检查")
    category: str = ""                   # 分类 (如 "canoe", "system", "tsmaster", "ttworkbench")
    module: str = ""                     # 模块名称 (如 "CANoe测试")
    script_path: str = ""                # cfg工程文件路径 / clf文件路径
    ini_config: str = ""                 # SelectInfo.ini配置内容 (原始INI格式)
    para_config: str = ""                # ParaInfo.ini默认参数 (JSON格式)
    enabled: bool = True                 # 是否启用
    priority: int = 0                   # 优先级 (数字越大优先级越高)
    tags: List[str] = field(default_factory=list)  # 标签列表
    version: str = "1.0"                 # 版本号
    description: str = ""                # 用例描述
    created_at: str = ""                 # 创建时间
    updated_at: str = ""                 # 更新时间

    # TTworkbench专用字段（新增）
    ttcn3_source: str = ""               # TTCN-3源码路径（如 TestModule.ttcn3）
    ttthree_path: str = ""               # TTthree编译器路径
    compile_params: str = ""             # 编译参数字典（JSON格式）
    clf_file: str = ""                   # 预生成的CLF文件路径
    log_format: str = "pdf"              # 日志格式: pdf/html/xml
    test_timeout: int = 3600             # 测试超时时间（秒）
```

- [ ] **Step 2: 更新to_dict方法添加新字段**

```python
def to_dict(self) -> Dict[str, Any]:
    """转换为字典"""
    return {
        "case_no": self.case_no,
        "case_name": self.case_name,
        "category": self.category,
        "module": self.module,
        "script_path": self.script_path,
        "ini_config": self.ini_config,
        "para_config": self.para_config,
        "enabled": self.enabled,
        "priority": self.priority,
        "tags": self.tags,
        "version": self.version,
        "description": self.description,
        "created_at": self.created_at,
        "updated_at": self.updated_at,
        # TTworkbench专用字段
        "ttcn3_source": self.ttcn3_source,
        "ttthree_path": self.ttthree_path,
        "compile_params": self.compile_params,
        "clf_file": self.clf_file,
        "log_format": self.log_format,
        "test_timeout": self.test_timeout
    }
```

- [ ] **Step 3: 更新from_dict方法读取新字段**

```python
@classmethod
def from_dict(cls, data: Dict[str, Any]) -> 'CaseMapping':
    """从字典创建"""
    return cls(
        case_no=data.get("case_no", ""),
        case_name=data.get("case_name", ""),
        category=data.get("category", ""),
        module=data.get("module", ""),
        script_path=data.get("script_path", ""),
        ini_config=data.get("ini_config", ""),
        para_config=data.get("para_config", ""),
        enabled=data.get("enabled", True),
        priority=data.get("priority", 0),
        tags=data.get("tags", []),
        version=data.get("version", "1.0"),
        description=data.get("description", ""),
        created_at=data.get("created_at", ""),
        updated_at=data.get("updated_at", ""),
        # TTworkbench专用字段
        ttcn3_source=data.get("ttcn3_source", ""),
        ttthree_path=data.get("ttthree_path", ""),
        compile_params=data.get("compile_params", ""),
        clf_file=data.get("clf_file", ""),
        log_format=data.get("log_format", "pdf"),
        test_timeout=data.get("test_timeout", 3600)
    )
```

- [ ] **Step 4: 提交更改**

```bash
cd d:/Deng/can_test/python_executor
git add models/case_mapping.py
git commit -m "feat(models): add TTworkbench-specific fields to CaseMapping"
```

---

### Task 2: 完善TTworkbench适配器 - 增加编译、日志解析功能

**Files:**
- Modify: `python_executor/core/adapters/ttworkbench_adapter.py`

- [ ] **Step 1: 添加TTman返回码常量类**

在文件开头导入部分后添加：

```python
class TTmanReturnCode:
    """TTman返回码定义

    110 - None: 无判定
    111 - Pass: 全部通过
    112 - Inconclusive: 不确定
    113 - Fail: 有失败
    """
    NONE = 110
    PASS = 111
    INCONCLUSIVE = 112
    FAIL = 113

    @classmethod
    def get_verdict(cls, return_code: int) -> str:
        """根据返回码获取判定结果"""
        mapping = {
            cls.NONE: "NONE",
            cls.PASS: "PASS",
            cls.INCONCLUSIVE: "INCONCLUSIVE",
            cls.FAIL: "FAIL"
        }
        return mapping.get(return_code, f"UNKNOWN({return_code})")
```

- [ ] **Step 2: 添加TTthree编译方法**

在TTworkbenchAdapter类中添加：

```python
def compile_ttcn3(self, source_file: str, output_dir: str = None,
                  use_existing_clf: bool = False) -> Dict[str, Any]:
    """编译TTCN-3源码

    Args:
        source_file: TTCN-3源码文件路径 (.ttcn3)
        output_dir: 输出目录（可选，默认workspace）
        use_existing_clf: 是否使用已存在的clf文件

    Returns:
        {
            "success": bool,
            "clf_file": str,  # 生成的clf文件路径
            "jar_file": str,  # 生成的jar文件路径
            "errors": str,   # 编译错误信息
            "warnings": str  # 编译警告信息
        }
    """
    import shutil

    result = {
        "success": False,
        "clf_file": "",
        "jar_file": "",
        "errors": "",
        "warnings": ""
    }

    # 检查源码文件是否存在
    if not os.path.isfile(source_file):
        result["errors"] = f"TTCN-3源码文件不存在: {source_file}"
        self.logger.error(result["errors"])
        return result

    # 确定TTthree路径
    ttthree_path = self.config.get("ttthree_path", "TTthree")
    if os.path.isfile(self.ttthree_path):
        ttthree_path = self.ttthree_path
    elif os.path.isfile(self.ttman_path):
        # 从ttman路径推断ttthree路径
        ttman_dir = os.path.dirname(self.ttman_path)
        ttthree_path = os.path.join(ttman_dir, "TTthree.bat" if os.name == 'nt' else "TTthree.sh")

    # 确定输出目录
    if not output_dir:
        output_dir = self.workspace_path
    os.makedirs(output_dir, exist_ok=True)

    # 编译命令
    source_name = os.path.splitext(os.path.basename(source_file))[0]
    cmd = [
        ttthree_path,
        "--clf-generate-default",
        "-d", output_dir,
        source_file
    ]

    self.logger.info(f"执行TTthree编译: {' '.join(cmd)}")

    try:
        proc = subprocess.run(
            cmd,
            capture_output=True,
            text=True,
            timeout=self.config.get("compile_timeout", 300)
        )

        result["warnings"] = proc.stderr if proc.returncode == 0 else ""

        if proc.returncode != 0:
            result["errors"] = proc.stderr
            self.logger.error(f"TTthree编译失败: {proc.stderr}")
            return result

        # 查找生成的clf文件
        clf_file = os.path.join(output_dir, f"{source_name}.clf")
        jar_file = os.path.join(output_dir, f"{source_name}.jar")

        if os.path.isfile(clf_file):
            result["success"] = True
            result["clf_file"] = clf_file
            result["jar_file"] = jar_file if os.path.isfile(jar_file) else ""
            self.logger.info(f"编译成功: {clf_file}")
        else:
            result["errors"] = f"编译成功但未找到clf文件: {clf_file}"
            self.logger.error(result["errors"])

    except subprocess.TimeoutExpired:
        result["errors"] = "编译超时"
        self.logger.error("TTthree编译超时")
    except Exception as e:
        result["errors"] = f"编译异常: {str(e)}"
        self.logger.error(f"TTthree编译异常: {e}")

    return result
```

- [ ] **Step 3: 添加TLZ日志解析方法**

```python
def parse_tlz_log(self, tlz_file: str) -> Dict[str, Any]:
    """解析TLZ日志文件

    Args:
        tlz_file: .tlz日志文件路径

    Returns:
        {
            "verdict": "PASS|FAIL|INCONCLUSIVE|NONE",
            "verdict_code": int,
            "total_cases": int,
            "passed_cases": int,
            "failed_cases": int,
            "inconclusive_cases": int,
            "case_results": [
                {
                    "name": str,
                    "verdict": "PASS|FAIL|INCONCLUSIVE|NONE",
                    "duration": float,
                    "error_message": str
                }
            ],
            "execution_time": float,
            "parse_errors": str
        }
    """
    import zipfile

    result = {
        "verdict": "NONE",
        "verdict_code": TTmanReturnCode.NONE,
        "total_cases": 0,
        "passed_cases": 0,
        "failed_cases": 0,
        "inconclusive_cases": 0,
        "case_results": [],
        "execution_time": 0.0,
        "parse_errors": ""
    }

    if not os.path.isfile(tlz_file):
        result["parse_errors"] = f"TLZ文件不存在: {tlz_file}"
        self.logger.error(result["parse_errors"])
        return result

    try:
        with zipfile.ZipFile(tlz_file, 'r') as zf:
            # 查找management.log
            file_list = zf.namelist()
            management_log = None
            for name in file_list:
                if 'management.log' in name:
                    management_log = name
                    break

            if not management_log:
                result["parse_errors"] = "未找到management.log"
                return result

            # 读取management.log
            with zf.open(management_log) as f:
                content = f.read().decode('utf-8', errors='ignore')

            # 解析内容
            case_results = []
            passed = 0
            failed = 0
            inconclusive = 0
            verdict_code = TTmanReturnCode.NONE
            execution_time = 0.0

            # 简单解析：查找TestCase执行记录
            import re

            # 匹配用例执行记录
            # 格式: TestCase: <name> - Verdict: <verdict> (Duration: <time>s)
            case_pattern = r'TestCase:\s*([^\s-]+)\s*-\s*Verdict:\s*(\w+)(?:\s*\(Duration:\s*([\d.]+)s\))?'
            for match in re.finditer(case_pattern, content, re.IGNORECASE):
                case_name = match.group(1)
                verdict = match.group(2).upper()
                duration = float(match.group(3)) if match.group(3) else 0.0

                case_verdict = verdict if verdict in ["PASS", "FAIL", "INCONCLUSIVE", "NONE"] else "NONE"

                # 获取错误信息
                error_msg = ""
                if case_verdict == "FAIL":
                    # 查找该用例后的错误信息
                    error_pattern = f'{case_name}.*?error|{case_name}.*?fail|{case_name}.*?Error'
                    error_match = re.search(error_pattern, content, re.IGNORECASE)
                    if error_match:
                        error_msg = error_match.group(0)

                case_results.append({
                    "name": case_name,
                    "verdict": case_verdict,
                    "duration": duration,
                    "error_message": error_msg
                })

                if case_verdict == "PASS":
                    passed += 1
                elif case_verdict == "FAIL":
                    failed += 1
                elif case_verdict == "INCONCLUSIVE":
                    inconclusive += 1

            # 解析总判定
            verdict_match = re.search(r'Verdict:\s*(\w+)', content, re.IGNORECASE)
            if verdict_match:
                verdict_str = verdict_match.group(1).upper()
                if "PASS" in verdict_str:
                    verdict_code = TTmanReturnCode.PASS
                elif "FAIL" in verdict_str:
                    verdict_code = TTmanReturnCode.FAIL
                elif "INCONCLUSIVE" in verdict_str:
                    verdict_code = TTmanReturnCode.INCONCLUSIVE
                else:
                    verdict_code = TTmanReturnCode.NONE

            # 解析执行时间
            time_match = re.search(r'Execution time:\s*([\d.]+)', content, re.IGNORECASE)
            if time_match:
                execution_time = float(time_match.group(1))

            result["verdict"] = TTmanReturnCode.get_verdict(verdict_code)
            result["verdict_code"] = verdict_code
            result["total_cases"] = len(case_results)
            result["passed_cases"] = passed
            result["failed_cases"] = failed
            result["inconclusive_cases"] = inconclusive
            result["case_results"] = case_results
            result["execution_time"] = execution_time

            self.logger.info(f"解析TLZ完成: {result['verdict']}, 用例数: {len(case_results)}")

    except zipfile.BadZipFile:
        result["parse_errors"] = "无效的TLZ文件格式"
        self.logger.error("无效的TLZ文件格式")
    except Exception as e:
        result["parse_errors"] = f"解析异常: {str(e)}"
        self.logger.error(f"解析TLZ异常: {e}")

    return result
```

- [ ] **Step 4: 修改_execute_clf_test方法使用正确的返回码判断**

找到现有的`_execute_clf_test`方法，修改返回结果中的status字段：

```python
def _execute_clf_test(self, item: Dict[str, Any]) -> Dict[str, Any]:
    """执行单个clf测试文件"""
    clf_file = item.get("clf_file")

    if not clf_file:
        raise ValueError("clf_test类型需要指定clf_file参数")

    if not os.path.isfile(clf_file):
        raise FileNotFoundError(f"clf文件不存在: {clf_file}")

    # 构建TTman命令
    cmd = self._build_ttman_command(clf_file)

    self.logger.info(f"执行TTman命令: {' '.join(cmd)}")

    # 执行命令
    result = self._run_ttman_command(cmd)

    # 获取测试用例名
    test_case_name = Path(clf_file).stem

    # 使用正确的返回码判断
    return_code = result["return_code"]
    verdict = TTmanReturnCode.get_verdict(return_code)

    # 根据判定确定状态
    if return_code == TTmanReturnCode.PASS:
        status = "passed"
    elif return_code == TTmanReturnCode.FAIL:
        status = "failed"
    elif return_code == TTmanReturnCode.INCONCLUSIVE:
        status = "inconclusive"
    else:
        status = "unknown"

    # 解析TLZ日志获取详细结果
    log_file = self._get_log_file(test_case_name)
    detailed_result = {}
    if log_file and os.path.isfile(log_file):
        detailed_result = self.parse_tlz_log(log_file)

    # 获取报告文件
    report_file = self._get_report_file(test_case_name)

    return {
        "name": item.get("name"),
        "type": "clf_test",
        "clf_file": clf_file,
        "test_case_name": test_case_name,
        "command": ' '.join(cmd),
        "return_code": return_code,
        "verdict": verdict,
        "stdout": result["stdout"],
        "stderr": result["stderr"],
        "execution_time": result["execution_time"],
        "report_file": report_file,
        "log_file": log_file,
        "status": status,
        # 详细结果（新增）
        "detailed_result": detailed_result
    }
```

- [ ] **Step 5: 修改_build_ttman_command支持配置中的log_format**

```python
def _build_ttman_command(self, clf_file: str) -> List[str]:
    """构建TTman命令"""
    # 使用配置中的log_format
    report_format = self.config.get("log_format", self.report_format)

    cmd = [
        self.ttman_path,
        '--data', self.workspace_path,
        '--log', self.log_path,
        '-r', report_format,
        '--report-dir', self.report_path,
        clf_file
    ]
    return cmd
```

- [ ] **Step 6: 提交更改**

```bash
cd d:/Deng/can_test/python_executor
git add core/adapters/ttworkbench_adapter.py
git commit -m "feat(adapter): enhance TTworkbench adapter with compile, TLZ parse, correct return codes"
```

---

### Task 3: 更新前端用例映射页面 - 增加TTworkbench字段表单

**Files:**
- Modify: `python_executor/web/templates/case_mapping.html`

- [ ] **Step 1: 读取现有case_mapping.html了解结构**

```bash
# 先查看文件结构确定插入位置
head -100 d:/Deng/can_test/python_executor/web/templates/case_mapping.html
```

- [ ] **Step 2: 在表单中添加TTworkbench专用字段区域**

在category选择为"ttworkbench"时显示的字段区域：

```html
<!-- TTworkbench专用字段 (当category为ttworkbench时显示) -->
<div id="ttworkbench-fields" class="config-section" style="display: none;">
    <h4><i class="fas fa-cog"></i> TTworkbench配置</h4>

    <div class="form-group">
        <label>TTCN-3源码路径:</label>
        <input type="text" id="ttcn3_source" name="ttcn3_source"
               placeholder="如: C:\Tests\TestModule.ttcn3">
        <small>TTCN-3源码文件路径，指定后将自动编译</small>
    </div>

    <div class="form-group">
        <label>TTthree编译器路径:</label>
        <input type="text" id="ttthree_path" name="ttthree_path"
               placeholder="如: C:\TTworkbench\TTthree.bat">
        <small>TTthree编译器路径，默认自动检测</small>
    </div>

    <div class="form-group">
        <label>CLF文件路径:</label>
        <input type="text" id="clf_file" name="clf_file"
               placeholder="如: C:\Tests\TestModule.clf">
        <small>预生成的CLF测试活动文件路径</small>
    </div>

    <div class="form-group">
        <label>编译参数 (JSON):</label>
        <textarea id="compile_params" name="compile_params" rows="3"
                  placeholder='{"include_paths": ["./include"], "defines": ["DEBUG"]}'></textarea>
        <small>TTthree编译参数，JSON格式</small>
    </div>

    <div class="form-group">
        <label>日志格式:</label>
        <select id="log_format" name="log_format">
            <option value="pdf">PDF</option>
            <option value="html">HTML</option>
            <option value="xml">XML</option>
        </select>
    </div>

    <div class="form-group">
        <label>测试超时时间 (秒):</label>
        <input type="number" id="test_timeout" name="test_timeout"
               value="3600" min="60" max="86400">
    </div>
</div>
```

- [ ] **Step 3: 添加JavaScript逻辑 - 根据category显示/隐藏TTworkbench字段**

```javascript
// 在现有的category切换逻辑中添加
function toggleCategoryFields() {
    const category = document.getElementById('category').value;

    // 隐藏所有专用字段
    document.getElementById('canoe-fields')?.style.display('none');
    document.getElementById('tsmaster-fields')?.style.display('none');
    document.getElementById('ttworkbench-fields').style.display = 'none';

    // 显示对应字段
    if (category === 'canoe') {
        const canoeFields = document.getElementById('canoe-fields');
        if (canoeFields) canoeFields.style.display = 'block';
    } else if (category === 'tsmaster') {
        const tsmasterFields = document.getElementById('tsmaster-fields');
        if (tsmasterFields) tsmasterFields.style.display = 'block';
    } else if (category === 'ttworkbench') {
        document.getElementById('ttworkbench-fields').style.display = 'block';
    }
}

// 绑定category变更事件
document.getElementById('category')?.addEventListener('change', toggleCategoryFields);
```

- [ ] **Step 4: 提交更改**

```bash
cd d:/Deng/can_test/python_executor
git add web/templates/case_mapping.html
git commit -m "feat(frontend): add TTworkbench fields to case mapping form"
```

---

### Task 4: 更新前端任务结果展示 - 增加两级结果显示

**Files:**
- Modify: `python_executor/web/templates/tasks.html`

- [ ] **Step 1: 读取现有tasks.html了解结构**

- [ ] **Step 2: 添加TTworkbench结果展示模板**

在现有的任务结果展示区域添加：

```html
<!-- TTworkbench测试结果展示 -->
<template id="ttworkbench-result-template">
    <div class="ttworkbench-result">
        <!-- 模块级汇总 -->
        <div class="result-summary-card">
            <h4><i class="fas fa-chart-pie"></i> TTworkbench测试结果</h4>
            <div class="summary-stats">
                <div class="stat-item">
                    <span class="stat-label">总用例</span>
                    <span class="stat-value total">{{total_cases}}</span>
                </div>
                <div class="stat-item passed">
                    <span class="stat-label">通过</span>
                    <span class="stat-value">{{passed_cases}}</span>
                </div>
                <div class="stat-item failed">
                    <span class="stat-label">失败</span>
                    <span class="stat-value">{{failed_cases}}</span>
                </div>
                <div class="stat-item inconclusive">
                    <span class="stat-label">不确定</span>
                    <span class="stat-value">{{inconclusive_cases}}</span>
                </div>
            </div>
        </div>

        <!-- 用例级详情 -->
        <div class="result-detail-section">
            <h5 onclick="toggleDetails(this)">
                <i class="fas fa-chevron-down"></i> 用例详情
            </h5>
            <table class="case-detail-table" style="display: none;">
                <thead>
                    <tr>
                        <th>用例名称</th>
                        <th>判定</th>
                        <th>耗时(s)</th>
                        <th>错误信息</th>
                    </tr>
                </thead>
                <tbody>
                    {{#each case_results}}
                    <tr class="verdict-{{toLowerCase verdict}}">
                        <td>{{name}}</td>
                        <td><span class="verdict-badge {{verdict}}">{{verdict}}</span></td>
                        <td>{{duration}}</td>
                        <td class="error-message">{{error_message}}</td>
                    </tr>
                    {{/each}}
                </tbody>
            </table>
        </div>
    </div>
</template>
```

- [ ] **Step 3: 添加CSS样式**

```css
/* TTworkbench结果展示样式 */
.ttworkbench-result {
    margin: 15px 0;
}

.result-summary-card {
    background: white;
    border-radius: 8px;
    padding: 15px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.result-summary-card h4 {
    margin: 0 0 15px 0;
    color: #333;
}

.summary-stats {
    display: flex;
    gap: 20px;
    flex-wrap: wrap;
}

.stat-item {
    text-align: center;
    padding: 10px 20px;
    border-radius: 4px;
    background: #f8f9fa;
}

.stat-item.passed { border-left: 3px solid #28a745; }
.stat-item.failed { border-left: 3px solid #dc3545; }
.stat-item.inconclusive { border-left: 3px solid #ffc107; }

.stat-label {
    display: block;
    font-size: 0.85em;
    color: #666;
}

.stat-value {
    display: block;
    font-size: 1.5em;
    font-weight: bold;
}

.case-detail-table {
    width: 100%;
    border-collapse: collapse;
    margin-top: 10px;
}

.case-detail-table th,
.case-detail-table td {
    padding: 8px 12px;
    text-align: left;
    border-bottom: 1px solid #e9ecef;
}

.verdict-badge {
    padding: 3px 8px;
    border-radius: 3px;
    font-size: 0.85em;
}

.verdict-badge.PASS { background: #d4edda; color: #155724; }
.verdict-badge.FAIL { background: #f8d7da; color: #721c24; }
.verdict-badge.INCONCLUSIVE { background: #fff3cd; color: #856404; }
.verdict-badge.NONE { background: #e2e3e5; color: #383d41; }

tr.verdict-fail td {
    background: #fff5f5;
}

.error-message {
    max-width: 300px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}
```

- [ ] **Step 4: 提交更改**

```bash
cd d:/Deng/can_test/python_executor
git add web/templates/tasks.html
git commit -m "feat(frontend): add two-level TTworkbench result display"
```

---

### Task 5: 集成测试验证

**Files:**
- Modify: 根据测试需要可能需要修改API层

- [ ] **Step 1: 验证CaseMapping新字段可以正常保存和加载**

```python
# 测试脚本
from models.case_mapping import CaseMapping

# 创建带TTworkbench字段的映射
mapping = CaseMapping(
    case_no="TTW-001",
    case_name="TCP一致性测试",
    category="ttworkbench",
    ttcn3_source="C:/Tests/TCPTest.ttcn3",
    ttthree_path="C:/TTworkbench/TTthree.bat",
    clf_file="C:/Tests/TCPTest.clf",
    log_format="xml",
    test_timeout=1800
)

# 测试序列化
data = mapping.to_dict()
print("序列化成功:", "ttcn3_source" in data)

# 测试反序列化
mapping2 = CaseMapping.from_dict(data)
print("反序列化成功:", mapping2.ttcn3_source == "C:/Tests/TCPTest.ttcn3")
```

- [ ] **Step 2: 验证适配器可以正确解析返回码**

```python
# 测试返回码转换
from core.adapters.ttworkbench_adapter import TTmanReturnCode

assert TTmanReturnCode.get_verdict(111) == "PASS"
assert TTmanReturnCode.get_verdict(113) == "FAIL"
assert TTmanReturnCode.get_verdict(112) == "INCONCLUSIVE"
assert TTmanReturnCode.get_verdict(110) == "NONE"
print("返回码测试通过")
```

- [ ] **Step 3: 手动测试完整流程**

1. 启动应用
2. 在用例映射页面添加TTworkbench类型用例
3. 执行测试任务
4. 验证前端显示两级结果

- [ ] **Step 4: 提交最终更改**

```bash
cd d:/Deng/can_test/python_executor
git add .
git commit -m "feat: complete TTworkbench adapter integration"
```

---

## 执行方式选择

**Plan complete and saved to `C:\Users\deng\.claude\plans\snazzy-hatching-whistle.md` (design) and being created as detailed plan.**

Two execution options:

1. **Subagent-Driven (recommended)** - I dispatch a fresh subagent per task, review between tasks, fast iteration

2. **Inline Execution** - Execute tasks in this session using executing-plans, batch execution with checkpoints

Which approach?