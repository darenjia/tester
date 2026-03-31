# ini_config 自动填充规则设计

## 1. 背景

在导入用例时，`ini_config` 字段通常为空，但该字段与用例编号存在关联关系。为了减少手动配置工作，需要在导入预览时根据预设规则自动计算并填充 `ini_config` 值。

## 2. 设计目标

- 在用例导入预览界面显示 `ini_config` 的预览值
- 支持按 `category` 配置不同的转换规则
- 使用正则表达式实现灵活的转换逻辑
- **新增：导入时可查看和修改正则规则**
- **新增：CANoe执行时需将ini_config写入SelectInfo.ini文件**
- 不符合规则的用例，`ini_config` 留空

## 3. 配置设计

### 3.1 config.json 规则配置

```json
{
  "category_ini_config_rules": {
    "canoe": {
      "pattern": "^(.*)$",
      "replacement": "$1=1"
    },
    "tsmaster": {
      "pattern": "^CAN_(.*)$",
      "replacement": "$1=1"
    }
  }
}
```

### 3.2 规则说明

| category | 正则模式 | 替换规则 | 示例输入 | 示例输出 |
|----------|----------|----------|----------|----------|
| canoe | `^(.*)$` | `$1=1` | TG1_TC01_SC01 | TG1_TC01_SC01=1 |
| tsmaster | `^CAN_(.*)$` | `$1=1` | CAN_TG1_TC1 | TG1_TC1=1 |

## 4. 功能设计

### 4.1 后端修改

#### 4.1.1 正则规则应用方法

**文件**: `core/case_mapping_manager.py` 或 `api/case_mapping_api.py`

新增方法 `apply_ini_config_rule(case_no, category)`:
```python
def apply_ini_config_rule(case_no: str, category: str) -> str:
    """根据category对应的正则规则，将case_no转换为ini_config"""
    rules = config_manager.get('category_ini_config_rules', {})
    rule = rules.get(category)
    if not rule:
        return ""
    pattern = rule.get('pattern', '')
    replacement = rule.get('replacement', '')
    if not pattern:
        return ""
    try:
        import re
        if re.match(pattern, case_no):
            return re.sub(pattern, replacement, case_no)
    except Exception:
        pass
    return ""
```

#### 4.1.2 预览API修改

**文件**: `api/case_mapping_api.py`

修改 `preview_excel_import` 接口：
1. 获取 `category_ini_config_rules` 配置
2. 遍历预览数据，根据 `category` 选择对应规则
3. 对 `case_no` 应用正则转换，得到 `ini_config` 预览值
4. 将预览结果返回给前端（包含 `ini_config_preview` 字段）

#### 4.1.3 CANoe执行时ini_config写入逻辑

**文件**: `core/config_manager.py`

修改 `_generate_select_info_ini` 方法：
- 当 `ini_config` 有值时，直接使用其内容写入文件
- 当 `ini_config` 为空时，回退到原有逻辑（根据 test_cases 生成）

SelectInfo.ini 格式：
```ini
[CFG_PARA]
TG1_TC01_SC01=1
TG1_TC02_SC01=1
```

### 4.2 前端修改

#### 4.2.1 正则规则显示与编辑区域

**文件**: `web/templates/case_mapping.html`

在导入流程的"映射配置"和"预览"之间，增加规则显示编辑区域：

```html
<div id="ini-config-rules-area" style="margin-bottom: 16px; padding: 12px; background: #f9fafb; border-radius: 8px;">
    <div style="font-weight: 600; margin-bottom: 8px;">
        <i class="fas fa-code"></i> ini_config 转换规则
        <button class="btn btn-sm" onclick="toggleRulesEdit()" style="margin-left: 8px;">
            <i class="fas fa-edit"></i> 编辑规则
        </button>
    </div>
    <div id="rules-display-area">
        <!-- 显示当前规则 -->
        <table style="width: 100%; font-size: 13px;">
            <thead>
                <tr>
                    <th>分类</th>
                    <th>正则模式</th>
                    <th>替换规则</th>
                    <th>示例</th>
                </tr>
            </thead>
            <tbody id="rules-table-body">
            </tbody>
        </table>
    </div>
    <div id="rules-edit-area" style="display: none;">
        <!-- 编辑规则（用户可修改） -->
        <textarea id="rules-json-editor" style="width: 100%; font-family: monospace; font-size: 12px;"></textarea>
        <div style="margin-top: 8px;">
            <button class="btn btn-secondary btn-sm" onclick="cancelRulesEdit()">取消</button>
            <button class="btn btn-primary btn-sm" onclick="saveRulesEdit()">保存规则</button>
        </div>
    </div>
</div>
```

#### 4.2.2 预览表格增加ini_config列

```html
<th style="padding: 10px; text-align: left; border-bottom: 1px solid #e5e7eb;">ini_config预览</th>
```

预览列：
- 符合规则：显示计算后的值，背景浅紫色 (#f3f0ff)
- 不符合规则：显示"-"，背景浅灰色

## 5. 预览表格设计

| Case编号 | 用例名称 | 分类 | ini_config预览 |
|----------|----------|------|----------------|
| TG1_TC01_SC01 | 源地址测试 | canoe | TG1_TC01_SC01=1 |
| CAN_TG1_TC1 | TG1_TC1_0x10服务 | tsmaster | TG1_TC1=1 |
| PL001 | 显性电平 | canoe | PL001=1 |

## 6. CANoe执行流程

### 6.1 当前流程

```
task.test_items[].case_no
         ↓
    查询 CaseMapping
         ↓
    获取 ini_config (当前为空)
         ↓
    config_manager._generate_select_info_ini() 生成 SelectInfo.ini
         ↓
    写入文件
```

### 6.2 修改后流程

```
task.test_items[].case_no
         ↓
    查询 CaseMapping
         ↓
    获取 ini_config (有值)
         ↓
    config_manager._generate_select_info_ini() 直接使用 ini_config 写入
         ↓
    SelectInfo.ini 文件内容：
    [CFG_PARA]
    TG1_TC01_SC01=1
    TG1_TC02_SC01=1
```

### 6.3 写入逻辑修改

**文件**: `core/config_manager.py` - `_generate_select_info_ini` 方法

```python
def _generate_select_info_ini(self, file_path: str, test_cases: List[Dict], mappings: List[CaseMapping] = None):
    """
    生成SelectInfo.ini文件

    优先使用CaseMapping中的ini_config字段
    """
    lines = ['[CFG_PARA]', '']

    if mappings:
        # 使用CaseMapping中的ini_config（已通过正则规则转换）
        for mapping in mappings:
            if mapping.ini_config:
                # ini_config 格式: "TG1_TC1=1,TG1_TC2=1" 或单个 "TG1_TC1=1"
                for config in mapping.ini_config.split(','):
                    config = config.strip()
                    if config:
                        lines.append(config)
    elif test_cases:
        # 回退：使用原有逻辑从test_cases生成
        for case in test_cases:
            case_id = case.get('caseNo') or case.get('case_id')
            if case_id:
                lines.append(f"{case_id}=1")

    with open(file_path, 'w', encoding='utf-8') as f:
        f.write('\n'.join(lines))
```

## 7. 实施步骤

1. 在 `config.json` 中添加 `category_ini_config_rules` 配置
2. 在 `CaseMappingManager` 中添加 `apply_ini_config_rule(case_no, category)` 方法
3. 修改 `preview_excel_import` 接口，在预览数据中包含计算后的 `ini_config_preview`
4. 修改前端 `case_mapping.html`：
   - 增加规则显示区域（映射步骤和预览步骤之间）
   - 增加规则编辑功能
   - 预览表格增加 `ini_config_preview` 列
5. 修改 `config_manager.py` 的 `_generate_select_info_ini` 方法：
   - 优先使用 CaseMapping 的 ini_config
   - 回退到原有 test_cases 逻辑

## 8. 注意事项

- 正则匹配失败时，`ini_config` 留空
- `ini_config_preview` 用于预览展示
- 用户可在导入界面修改规则，修改后的规则仅影响本次导入预览
- CANoe执行时，ini_config直接写入SelectInfo.ini文件
