# 执行摘要

使用 Python 本地调用 TTworkbench 脚本执行测试用例的方案，关键在于利用 TTworkbench 提供的命令行工具（如 `TTthree` 编译器和 `TTman` 管理器）完成编译和测试执行，并在 Python 脚本中捕获返回码和日志文件（.tlz）进行解析。假设使用最新稳定版 TTworkbench，支持 Windows 和 Linux 平台【50†L999-L1003】【50†L1014-L1020】，需要安装 Java 环境（至少 Java SE 5.0【50†L1014-L1020】）并配置好系统 PATH。实现流程包括：配置环境（Java、许可证、环境变量）、调用 `TTthree` 编译 TTCN‑3 模块并生成测试活动文件（*.clf），调用 `TTman` 执行测试并生成日志（*.tlz），然后解析日志提取结果。返回码方面，`TTman` 的退出码对应测试最严重判定（例如 111=Pass，113=Fail【51†L6945-L6951】），`subprocess` 调用的 returncode 可用来判断流程状态。报告将给出详细步骤、三个 Python 3.x 示例（包含 `subprocess.run`、带超时的异步执行、并发执行）、日志解析策略、常见错误及解决方案、并发管理建议，以及测试验证清单。流程图描述本地调用过程。所有方案均基于官方文档和最佳实践实现，必要时给出替代方案。

## 假设与环境准备

- **TTworkbench 版本/平台**：未指定具体版本，假设使用最新版，支持 Windows（Vista/XP/2000 等）和 Linux（如 RedHat/SuSE）【50†L999-L1003】。  
- **Java 环境**：TTworkbench 基于 Eclipse 3.x，需要 Java SE 5.0 及以上（JRE/JDK）【50†L1014-L1020】。安装 TTworkbench 前需先安装 Java。  
- **系统 PATH**：将 TTworkbench 安装目录下的脚本路径加入 `PATH`（Windows 下为 `TTthree.bat`、`TTman.bat`，Linux 为 `TTthree.sh`、`TTman.sh`）。确保可以直接在命令行执行这些命令。  
- **权限与许可证**：TTworkbench 使用 FLEXnet 许可证【50†L1028-L1036】，需配置有效的许可证文件。用户运行脚本时应有读写权限来生成日志和报告。  
- **工作目录**：每次执行应指定独立工作目录（例如通过 `subprocess` 的 `cwd` 参数），避免并发冲突。也可在目录中存放编译产物和日志。  
- **环境变量**：可设置 `TTWB_CORE`、`TTTHREE_HOME` 等（高级用法），普通用户无需手动设置。主要保证 `JAVA_HOME` 等必要环境变量已配置。

## 本地调用流程概述

整体流程如下所示：首先在 Python 中准备环境并构造命令，调用 `TTthree` 编译 TTCN-3 模块并生成默认测试活动文件（*.clf）；然后调用 `TTman` 执行该测试活动文件，生成压缩日志文件（*.tlz）；最后使用 Python 解压 `.tlz` 并解析日志（例如 `management.log`）提取测试结果或失败信息，最终生成 JSON/报告文件。流程如下（示意图使用 mermaid 标注）：

```mermaid
flowchart LR
    A[环境准备] --> B[TTthree 编译模块(.ttcn3) \n生成 .jar 和 .clf]
    B --> C[TTman 执行测试活动(.clf)]
    C --> D[生成 .tlz 日志文件]
    D --> E[Python 解压解析日志(如 management.log)]
    E --> F{检查测试结果}
    F -->|失败| G[记录错误并可重试]
    F -->|通过| H[生成报告/结束]
```

- **环境准备**：确认 Java、许可证、环境变量及必要依赖可用。  
- **调用编译工具**：使用 `TTthree` 命令编译模块。例如：  
  ```bash
  TTthree.sh --clf-generate-default -d out_dir MyModule
  ```  
  该命令将编译 `MyModule.ttcn3` 并生成 `MyModule.jar`，默认生成 `MyModule.clf`（测试活动加载文件）【23†L4903-L4912】。常用选项有 `--clf-generate-default`（自动生成测试活动）和 `-d <path>`（指定输出目录）等。  
- **调用执行工具**：使用 `TTman` 命令执行测试。例如：  
  ```bash
  TTman.sh -l logs MyModule.clf
  ```  
  `-l logs` 指定日志输出目录【40†L6891-L6899】。可以加 `-t <testcase>` 运行指定用例，`-r html` 生成 HTML 报告等。  
- **结果检查**：`TTman` 执行完成后返回码表示测试判定（110=None, 111=Pass, 112=Inconclusive, 113=Fail【51†L6945-L6951】）。在 Python 中通过 `subprocess` 捕获 `returncode` 来判断执行状态。  
- **日志解析**：`TTman` 会生成一个 `.tlz` 压缩日志文件【54†L6242-L6249】，其中包含管理日志和各用例日志。Python 脚本应解压 `.tlz`，读取如 `management.log` 等内部文件，查找失败用例或关键字段，提取测试数据。  
- **报告生成**：根据解析结果生成 JSON 或其他格式的测试报告。可以结合上文生成的 HTML/PDF 报告一起归档。

## 实现步骤与关键细节

1. **环境准备**：  
   - 安装 Java（JRE/JDK）并设置 `JAVA_HOME`。【50†L1014-L1020】  
   - 安装 TTworkbench，将安装路径下 `bin` 目录加入系统 `PATH`。确保可在命令行直接运行 `TTthree.sh/.bat` 和 `TTman.sh/.bat`。  
   - 配置许可证：将许可证文件放置在默认目录，或设置环境变量如 `TTWB_LICENSE` 指向许可证路径。  
   - 创建 Python 脚本工作目录，并根据需要创建子目录（如 `build/`、`logs/`、`reports/` 等）以便隔离不同执行。  
2. **编译 TTCN‑3 模块**：  
   - 使用 Python 调用 `subprocess.run` 或 `subprocess.Popen` 执行 `TTthree`。示例命令（Linux）：  
     ```bash
     TTthree.sh --clf-generate-default -d build MyModule
     ```  
     Windows 相应使用 `TTthree.bat`。该命令编译位于当前目录的 `MyModule.ttcn3`，输出 `.jar` 到 `build/`，并生成 `MyModule.clf`【23†L4903-L4912】。  
   - 捕获 `stdout`/`stderr` 与返回码：若编译失败，`returncode` 非零，可在 Python 中捕获并记录错误。  
   - 常见参数说明：  
     - `--clf-generate-default`：自动生成包含所有测试用例的默认测试活动文件（.clf）。  
     - `-d <dir>`：指定编译输出目录。  
     - `--implicit-import` 等高级选项：可根据需要设置模块依赖。  
3. **执行测试活动**：  
   - 调用 `TTman` 运行生成的 `.clf` 文件。例如：  
     ```bash
     TTman.sh -l logs MyModule.clf
     ```  
     `-l logs` 将执行日志输出到 `logs/` 目录。【40†L6891-L6899】其它常用选项：  
     - `-t <testcase>`：只执行指定用例。  
     - `-r <format>`：生成测试报告（html/pdf/excel/word）。  
     - `-P <tt3-plugin-dir>`：指定额外插件目录（如果有）。  
   - **返回码**：`subprocess.run` 返回的 `returncode` 即为 TTman 退出码。根据官方文档，当有失败用例时会返回 113【51†L6945-L6951】；可以根据此码判断是否需重试或失败处理。  
4. **收集与解析日志**：  
   - 执行结束后，在 `logs/` 目录下会生成一个 `.tlz` 文件（例如 `MyModule.tlz`）【54†L6242-L6249】。该文件是一个 ZIP 压缩包，包含完整的测试日志和执行配置。  
   - 使用 Python 的 `zipfile` 模块解压 `.tlz`。常见内部文件包括：`management.log`（记录测试流程的概要事件）、各用例的 `.log` 或 `.tl` 文件等。  
   - 解析策略示例：可以逐行扫描 `management.log` 查找 `<verdict>` 或 “Failed” 等关键字；或使用 XML 解析库定位失败用例的 `<verdict>` 节点。示例：  
     ```python
     import zipfile
     with zipfile.ZipFile('logs/MyModule.tlz') as zf:
         for name in zf.namelist():
             if name.endswith('management.log'):
                 data = zf.read(name).decode('utf-8')
                 # 使用正则或字符串查找失败标记
                 if 'FAILURE' in data:
                     print("测试失败详情：", extract_failure_info(data))
     ```  
     具体文件名和内容结构可参见 TTworkbench 用户手册【54†L6242-L6249】。  
5. **错误处理与恢复**：  
   - **路径/环境错误**：捕获 `FileNotFoundError`（命令未找到），提示检查 `PATH` 或脚本名称（Linux `.sh` vs Windows `.bat`）。  
   - **编译失败**：如果 `TTthree` 返回非零，记录 `stderr` 中的编译错误，按需退出或重新尝试。  
   - **TLZ 未生成**：可能是执行失败导致未产生日志。检查 `TTman` 返回码，若非 111/113，说明中断；可尝试重新运行或人工检查配置。  
   - **并发冲突**：若并行运行多个 `TTman` 导致文件覆盖或锁定，应为每个任务使用独立目录（配置不同 `-l`）。  
   - **许可证问题**：若 TTworkbench 启动失败或报错 `license` 字样，提示许可证不足。可提前在 Python 脚本中检查许可证文件存在或运行如 `TTman -v` 查看版本信息。  
   - **自动重试**：对可重现的临时错误（如资源竞争、瞬时网络问题），可在脚本中实现有限次数的重试（带延迟回退），并在日志中记录多次失败。  
   - **超时处理**：对于长期挂起的执行，可在 Python 中设置超时（参见下文示例B）。超时后安全终止进程并收集当前日志。  

## 示例代码

以下示例均为 Python 3.x，可在 Windows/Linux 环境中运行。请根据需要修改命令中的脚本扩展名（Linux 用 `.sh`，Windows 用 `.bat`）和路径。

### 示例A：使用 `subprocess.run`（同步调用）

```python
import subprocess, zipfile, json

# 1. 调用 TTthree 编译模块并生成 .clf
cmd_compile = ["TTthree.sh", "--clf-generate-default", "-d", "build", "MyModule"]
res = subprocess.run(cmd_compile, capture_output=True, text=True)
if res.returncode != 0:
    print("编译失败：", res.stderr)
    # 根据需要退出或记录
else:
    print("编译输出：", res.stdout)

# 2. 执行生成的测试活动(.clf)
cmd_execute = ["TTman.sh", "-l", "logs", "build/MyModule.clf"]
res2 = subprocess.run(cmd_execute, capture_output=True, text=True)
print("执行输出：", res2.stdout)
returncode = res2.returncode

# 3. 检查返回码
if returncode == 111:
    verdict = "PASS"
elif returncode == 113:
    verdict = "FAIL"
else:
    verdict = f"OTHER({returncode})"
print("测试结果：", verdict)

# 4. 解压并解析 .tlz 日志
tlz_path = "logs/MyModule.tlz"
report = {"module": "MyModule", "result": verdict, "failures": []}
try:
    with zipfile.ZipFile(tlz_path) as zf:
        # 假设 management.log 记录全局结果
        if "MyModule/management.log" in zf.namelist():
            data = zf.read("MyModule/management.log").decode('utf-8')
            # 查找失败信息（示例：查找字符串 'FAIL'）
            for line in data.splitlines():
                if "FAIL" in line:
                    report["failures"].append(line)
        else:
            print("未找到管理日志")
except FileNotFoundError:
    print("TLZ 日志未找到，检查 TTman 是否成功生成")

# 5. 将结果写入 JSON 报告
with open("report.json", "w") as f:
    json.dump(report, f, ensure_ascii=False, indent=2)
```

该示例中，`subprocess.run` 用于同步调用命令，`capture_output=True` 获取输出并存储在 `stdout`/`stderr`。最后将测试结果和解析出的失败信息写入 JSON 文件。

### 示例B：使用 `subprocess.Popen` 和超时控制

```python
import subprocess, threading, time, zipfile

# 定义执行函数，带超时和中断支持
def run_with_timeout(cmd, timeout, log_dir):
    proc = subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    timer = threading.Timer(timeout, proc.kill)
    try:
        timer.start()
        out, err = proc.communicate()
    finally:
        timer.cancel()

    return proc.returncode, out, err

# 示例调用
cmd_execute = ["TTman.sh", "-l", "logs", "MyModule.clf"]
timeout_seconds = 60  # 1 minute timeout
returncode, out, err = run_with_timeout(cmd_execute, timeout_seconds, "logs")
if returncode is None:
    print("执行超时，已终止进程")
elif returncode != 0:
    print(f"执行出错，返回码 {returncode}, stderr: {err}")
else:
    print("执行成功")

# 如果超时或异常，可尝试再次解析已有的日志
try:
    with zipfile.ZipFile("logs/MyModule.tlz") as zf:
        data = zf.read("MyModule/management.log").decode()
        print("已有日志内容：", data[:100])
except FileNotFoundError:
    print("未生成日志或日志路径错误")
```

该示例用 `threading.Timer` 在超时后调用 `proc.kill()` 强制终止进程，并获取部分输出。超时后仍尝试读取日志文件（如果有写入）以分析中途结果。

### 示例C：并发执行示例

```python
import concurrent.futures
import subprocess, zipfile

def run_test(module_name):
    # 每个任务在独立目录运行
    build_dir = f"build_{module_name}"
    log_dir = f"logs_{module_name}"
    subprocess.run(["mkdir", "-p", build_dir, log_dir])
    # 编译
    res = subprocess.run(["TTthree.sh", "--clf-generate-default", "-d", build_dir, module_name],
                         capture_output=True, text=True)
    if res.returncode != 0:
        return (module_name, "compile_error", res.stderr)
    # 执行
    res2 = subprocess.run(["TTman.sh", "-l", log_dir, f"{build_dir}/{module_name}.clf"],
                          capture_output=True, text=True)
    verdict = res2.returncode
    # 解析日志
    failures = []
    try:
        with zipfile.ZipFile(f"{log_dir}/{module_name}.tlz") as zf:
            data = zf.read(f"{module_name}/management.log").decode()
            if "FAIL" in data:
                failures.append("Found FAIL in logs")
    except Exception as e:
        failures.append(f"Log parsing error: {e}")
    return (module_name, verdict, failures)

# 假设有多个模块并行执行
modules = ["TestMod1", "TestMod2", "TestMod3"]
results = []
max_workers = 2  # 并发限制
with concurrent.futures.ProcessPoolExecutor(max_workers=max_workers) as executor:
    futures = {executor.submit(run_test, mod): mod for mod in modules}
    for future in concurrent.futures.as_completed(futures):
        mod = futures[future]
        try:
            result = future.result()
            results.append(result)
        except Exception as exc:
            results.append((mod, "exception", str(exc)))

print("并发执行结果：", results)
```

在此示例中，使用 `ProcessPoolExecutor` 并行执行多个测试模块。优点是每个任务独立进程、无 GIL 限制，适用于 CPU 密集型操作；缺点是进程启动开销和共享数据难度。每个模块使用独立的构建目录和日志目录，防止文件冲突。并发数量受 `max_workers` 控制。这里简化了错误聚合，只返回简单失败信息列表；可根据需要扩展异常处理和重试逻辑。对于许可问题，可在任务开始时检查许可（例如执行 `TTman.sh -v`），若失败则跳过并上报。

## 日志解析策略

- **解压 `.tlz`**：`.tlz` 文件本质上是一个 ZIP 压缩包【54†L6242-L6249】。Python 可用 `zipfile.ZipFile` 打开。示例代码：  
  ```python
  import zipfile
  with zipfile.ZipFile('MyModule.tlz') as zf:
      files = zf.namelist()
      print("日志包含文件：", files)
      management = zf.read('MyModule/management.log').decode('utf-8')
  ```  
- **常见内部文件**：`management.log` 通常记录测试执行过程中的关键事件和总体结果【54†L6242-L6249】；每个测试用例可能对应一个 `.log` 或 `.tl` 文件（根据配置）。  
- **定位失败用例**：在 `management.log` 或各用例日志中查找失败标识，例如查找 `<verdict>` 标签或 “FAIL” 关键词。可以使用正则或 XML 解析。例如：  
  ```python
  import re
  fails = re.findall(r'Verdict: FAIL', management)
  ```  
- **XPath/解析示例**：若日志为 XML 格式，可用 `xml.etree.ElementTree` 解析：  
  ```python
  import xml.etree.ElementTree as ET
  root = ET.fromstring(management)
  for test in root.findall('.//TestCase'):
      verdict = test.find('Verdict').text
      if verdict == 'FAIL':
          print("失败用例：", test.find('Name').text)
  ```  
  根据具体日志结构调整路径。  
- **关键字段**：通常关注测试名称、输入参数、预期值和实际值、错误消息等。解析后将这些信息汇总到 JSON 或数据库中，以便自动生成测试报告。

## 常见错误与解决方案

- **命令未找到 / 脚本错误**：若执行 `TTthree`/`TTman` 时抛出 `FileNotFoundError`，请检查脚本名和路径。Linux 下使用 `.sh` 脚本，Windows 下使用 `.bat`。可在调用时指定全路径来避免环境变量问题。  
- **环境变量缺失**：确保 `JAVA_HOME`、`PATH` 中包含 Java 和 TTworkbench 路径。如路径中有空格，注意用引号或转义。  
- **编译失败**：`TTthree` 返回非零时，通常是语法错误或缺少依赖。应在 Python 中捕获 `returncode` 并输出 `stderr` 错误。开发时可在命令行手动运行排查。  
- **TLZ 未生成**：可能是执行中断或失败。检查 `TTman` 返回码；如果返回异常，TTman 可能没有生成 `.tlz`。可检查日志目录路径是否正确（`-l` 选项），并查找是否有 `.tl` 或其他临时日志文件。  
- **并发冲突**：若并发运行出现文件覆盖或目录争用，需保证每个任务使用不同目录（例如示例C所示）。避免多个进程同时写入同一个 `.clf` 文件或相同日志路径。  
- **许可证不足**：运行 TTworkbench 时可能弹出许可错误。检测策略：调用 `TTman -v` 或检查输出中是否包含 “license” 相关错误。如果无有效许可证，测试无法执行。解决：确保许可证文件可读或更新许可证。  
- **资源不足 / 超时**：大型测试可能超时或耗尽内存。可在 Python 中设定超时机制（示例B），并在日志中捕获部分结果。可配置重试（退避算法）避免反复快速失败。  
- **自动恢复建议**：对非致命错误（如临时网络断连、文件锁等），可在脚本中加入重试逻辑（可指定重试次数和间隔）。对致命错误（如许可证问题），应立即中断流程并告警。

## 并发执行与资源管理建议

- **并发方法**：对于多模块或多用例场景，可使用 `concurrent.futures`（多进程）或 `asyncio`（异步）并发执行。  
  - *多进程（示例C）*：适合 CPU 密集型任务（TTCN-3 编译/执行），每个进程独立，避免 GIL 限制。但进程创建开销大、内存使用高。  
  - *异步 I/O（`asyncio`）*：适合 I/O 密集型场景，可同时监听多个子进程的输出。但 Python 的异步对 CPU 绑定任务提升有限，需要结合 `asyncio.create_subprocess_exec`。  
  - 可根据实际测试需求选择。  
- **资源限制**：为避免系统过载，限定最大并发数量（如只开设与CPU核心数相当的进程）。可使用信号量控制并发度。  
- **独立工作目录**：并发时每个任务使用独立的工作目录，以防止文件冲突（示例C为每个模块创建 `build_modX/` 和 `logs_modX/`）。  
- **许可证并发**：多进程同时运行可能需要多许可。建议在任务开始前尝试获取许可证（如果 TTworkbench 支持命令行分配许可），或串行化执行许可有限的关键任务。  
- **监控与清理**：在长时间运行或多轮迭代后，监控磁盘空间和内存占用。执行结束后清理临时目录和日志（可选择保留失败相关日志）。

## 测试与验证清单

1. **环境验证**：手动运行 `TTthree.sh -v` 和 `TTman.sh -v` 确认工具可用，检查 Java 和许可证状态。  
2. **功能验证**：选择已知测试用例（通过/失败的例子），先在 GUI 中运行确认结果，再用 Python 脚本调用，对比两者输出和生成报告的一致性。  
3. **错误场景测试**：模拟常见错误，如强制无许可证、故意编译错误、指定不存在路径等，验证脚本能正确捕获并报错。  
4. **回归测试**：每次更改脚本后，用一组示例测试集进行回归，确保之前正常的流程依然有效。  
5. **性能与并发测试**：在有限资源条件下测试并发执行，观察执行时间、资源占用，调整并发数量或异步策略。  
6. **日志解析测试**：编写单元测试对示例 `.tlz` 文件解析函数进行验证，确保失败用例能被正确提取。  
7. **集成测试**：将脚本集成到 CI/CD 流水线中，自动触发测试执行，并通过 JSON/报告通知结果。  

## 参考文献

- TTworkbench 用户手册【23†L4903-L4912】【51†L6945-L6951】【54†L6242-L6249】（命令行用法、返回码与日志格式）  
- TTworkbench 数据表【50†L999-L1003】【50†L1014-L1020】（支持平台与 Java 版本、许可证）  

