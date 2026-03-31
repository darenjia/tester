import os
import subprocess
from pathlib import Path
from typing import List, Optional

# -----------------------------
# 配置区域
# -----------------------------
TESTCASE_DIR = Path("./testcases")  # 用例目录
BUILD_DIR = Path("./build")         # 可执行文件输出目录
LOG_DIR = Path("./logs")            # 日志输出目录

TTWORKBENCH_CLI = "ttworkbench_cli"  # TTworkbench 命令行工具
COMPILER = "gcc"                     # 编译器
CFLAGS = "-O2 -Wall"                 # 编译选项
INCLUDE_DIRS = ["./include"]         # 头文件路径
LIB_DIRS = ["./lib"]                 # 库文件路径
LIBS = ["somelib"]                   # 依赖库

# -----------------------------
# 工具函数
# -----------------------------
def ensure_dirs():
    """确保目录存在"""
    for d in [BUILD_DIR, LOG_DIR]:
        d.mkdir(parents=True, exist_ok=True)

def compile_clf(clf_path: Path) -> Optional[Path]:
    """编译单个 clf 用例"""
    output_file = BUILD_DIR / (clf_path.stem + ".out")
    include_flags = " ".join([f"-I{inc}" for inc in INCLUDE_DIRS])
    lib_flags = " ".join([f"-L{lib}" for lib in LIB_DIRS]) + " " + " ".join([f"-l{lib}" for lib in LIBS])
    
    cmd = f"{COMPILER} {CFLAGS} {include_flags} {clf_path} -o {output_file} {lib_flags}"
    
    try:
        subprocess.run(cmd, shell=True, check=True)
        return output_file
    except subprocess.CalledProcessError as e:
        print(f"[ERROR] 编译失败: {clf_path.name}, {e}")
        return None

def run_ttworkbench(executable_path: Path) -> Optional[str]:
    """调用 TTworkbench 执行测试用例"""
    log_file = LOG_DIR / (executable_path.stem + ".log")
    cmd = f"{TTWORKBENCH_CLI} run {executable_path} > {log_file} 2>&1"
    
    try:
        subprocess.run(cmd, shell=True, check=True)
        return str(log_file)
    except subprocess.CalledProcessError as e:
        print(f"[ERROR] 执行失败: {executable_path.name}, {e}")
        return None

def parse_result(log_file_path: str) -> dict:
    """解析执行日志获取结果（示例: 成功/失败）"""
    result = {"status": "UNKNOWN", "details": ""}
    try:
        with open(log_file_path, "r") as f:
            content = f.read()
            if "PASS" in content:
                result["status"] = "PASS"
            elif "FAIL" in content:
                result["status"] = "FAIL"
            result["details"] = content
    except Exception as e:
        result["status"] = "ERROR"
        result["details"] = str(e)
    return result

# -----------------------------
# 核心流程
# -----------------------------
def execute_testcases(case_names: List[str]) -> dict:
    """
    执行指定用例，并返回执行结果
    :param case_names: 用例文件名列表 (不带路径)
    :return: {用例名: {status, details}}
    """
    ensure_dirs()
    results = {}
    
    for case_name in case_names:
        clf_file = TESTCASE_DIR / case_name
        if not clf_file.exists():
            print(f"[WARN] 用例不存在: {case_name}")
            results[case_name] = {"status": "NOT_FOUND", "details": ""}
            continue
        
        exe_file = compile_clf(clf_file)
        if not exe_file:
            results[case_name] = {"status": "COMPILE_FAIL", "details": ""}
            continue
        
        log_file = run_ttworkbench(exe_file)
        if not log_file:
            results[case_name] = {"status": "EXEC_FAIL", "details": ""}
            continue
        
        results[case_name] = parse_result(log_file)
    
    return results

# -----------------------------
# 示例调用
# -----------------------------
if __name__ == "__main__":
    # 指定要执行的用例
    testcase_list = ["test1.clf", "test2.clf"]
    
    results = execute_testcases(testcase_list)
    
    for case, res in results.items():
        print(f"\n[RESULT] {case} -> {res['status']}")
        # 如果需要详细日志，可以打印 res['details']