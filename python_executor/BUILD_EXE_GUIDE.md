# Python执行器打包为EXE指南

本文档介绍如何将Python执行器打包为独立的Windows可执行文件（.exe）。

## 目录

- [环境要求](#环境要求)
- [打包方法](#打包方法)
- [打包文件说明](#打包文件说明)
- [常见问题](#常见问题)
- [高级配置](#高级配置)

## 环境要求

### 必需软件

1. **Python 3.8+** (推荐 3.10 或 3.11)
2. **Windows操作系统** (打包Windows exe必需)
3. **PyInstaller 6.0+**

### 安装依赖

```bash
# 安装项目依赖
pip install -r requirements.txt

# 或单独安装PyInstaller
pip install pyinstaller>=6.0.0
```

## 打包方法

### 方法一：使用批处理脚本（推荐）

在Windows上双击运行 `build_exe.bat` 文件：

```
build_exe.bat
```

脚本会自动：
1. 检查Python环境
2. 安装PyInstaller（如未安装）
3. 安装项目依赖
4. 清理旧构建文件
5. 执行打包

### 方法二：使用Python脚本

```bash
python build_exe.py
```

### 方法三：直接使用PyInstaller

```bash
pyinstaller PythonExecutor.spec
```

## 打包文件说明

打包后会生成以下文件：

```
dist/
└── PythonExecutor.exe          # 主可执行文件（单文件模式）

build/
└── ...                         # 构建临时文件（可删除）
```

### 输出文件特点

- **单文件模式**：所有内容打包到一个 `PythonExecutor.exe` 文件中
- **便携性**：无需Python环境，双击即可运行
- **包含资源**：自动包含所有Python模块和配置文件

## 使用方法

### 直接运行

```bash
# 双击运行
dist\PythonExecutor.exe

# 或在命令行运行
dist\PythonExecutor.exe
```

### 指定配置

```bash
# 使用自定义配置文件
dist\PythonExecutor.exe --config path/to/config.json
```

### 后台运行

```bash
# Windows后台运行（无控制台窗口）
start dist\PythonExecutor.exe
```

## 常见问题

### 1. 打包失败 - 缺少模块

**问题**：`ModuleNotFoundError: No module named 'xxx'`

**解决**：在 `PythonExecutor.spec` 文件的 `hiddenimports` 列表中添加缺失的模块

```python
hiddenimports=[
    '缺失的模块名',
    # ... 其他模块
]
```

### 2. 运行时缺少文件

**问题**：运行时提示找不到配置文件或资源文件

**解决**：确保在 `PythonExecutor.spec` 的 `datas` 部分正确添加了数据文件路径

```python
datas=[
    ('config/executor_config.json', 'config'),
    # ... 其他文件
]
```

### 3. 文件体积过大

**问题**：生成的exe文件太大（>100MB）

**解决**：
1. 在 `excludes` 列表中添加不需要的模块
2. 使用UPX压缩（已默认启用）
3. 考虑使用目录模式而非单文件模式

### 4. 杀毒软件误报

**问题**：杀毒软件将exe识别为病毒

**解决**：
1. 添加数字签名（需要证书）
2. 将exe添加到杀毒软件白名单
3. 使用 `--onefile` 模式可能减少误报

### 5. 运行时闪退

**问题**：双击exe后窗口一闪而过

**解决**：
1. 使用命令行运行查看错误信息
2. 检查 `console=True` 设置（已默认启用）
3. 查看日志文件了解详细错误

## 高级配置

### 修改入口文件

默认使用 `main_entry.py` 作为入口，如需修改：

1. 编辑 `PythonExecutor.spec`：
```python
Analysis(
    ['your_entry.py'],  # 修改此处
    # ...
)
```

2. 重新打包

### 添加图标

1. 准备 `.ico` 格式的图标文件（如 `icon.ico`）
2. 编辑 `PythonExecutor.spec`：
```python
exe = EXE(
    # ...
    icon='icon.ico',  # 取消注释此行
)
```

### 修改输出名称

编辑 `PythonExecutor.spec`：
```python
exe = EXE(
    # ...
    name='YourAppName',  # 修改此处
)
```

### 多文件模式（减小启动时间）

如需更快的启动速度，可使用多文件模式：

```python
# PythonExecutor.spec

# 修改最后部分
coll = COLLECT(
    exe,
    a.binaries,
    a.zipfiles,
    a.datas,
    strip=False,
    upx=True,
    upx_exclude=[],
    name='PythonExecutor'
)
```

然后运行：
```bash
pyinstaller PythonExecutor.spec
```

输出将是一个目录而非单个文件。

### 数字签名（Windows）

如需对exe进行数字签名：

```bash
# 使用signtool（需要安装Windows SDK）
signtool sign /f certificate.pfx /p password dist\PythonExecutor.exe
```

## 打包后测试

打包完成后，建议进行以下测试：

1. **基本功能测试**
   ```bash
   dist\PythonExecutor.exe
   # 访问 http://localhost:5000
   ```

2. **API测试**
   ```bash
   python test_api_simple.py
   ```

3. **WebSocket测试**
   ```bash
   python tests/test_websocket.py
   ```

## 发布建议

### 发布前检查清单

- [ ] 在干净的Windows环境测试
- [ ] 验证所有功能正常工作
- [ ] 检查日志输出是否正常
- [ ] 测试配置文件加载
- [ ] 确认端口未被占用

### 发布文件

建议发布以下文件：

```
PythonExecutor-v1.0.0/
├── PythonExecutor.exe      # 主程序
├── config/                 # 配置文件目录
│   └── executor_config.json
├── README.txt              # 使用说明
└── LICENSE                 # 许可证
```

## 技术支持

如遇到打包问题，请检查：

1. Python版本是否兼容（3.8+）
2. 所有依赖是否正确安装
3. PyInstaller版本是否最新
4. 查看PyInstaller日志获取详细信息

更多帮助请参考：
- [PyInstaller文档](https://pyinstaller.readthedocs.io/)
- 项目GitHub Issues
