#!/usr/bin/env python3
"""
测试运行入口

运行所有测试并生成测试报告
"""

import sys
import os
import subprocess
import json
import time
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Any

# 添加项目根目录到Python路径
project_root = Path(__file__).parent.parent
sys.path.insert(0, str(project_root))


class Colors:
    """终端颜色"""
    HEADER = '\033[95m'
    OKBLUE = '\033[94m'
    OKCYAN = '\033[96m'
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'


def print_header(text: str):
    """打印标题"""
    print(f"\n{Colors.HEADER}{'='*60}{Colors.ENDC}")
    print(f"{Colors.HEADER}{text.center(60)}{Colors.ENDC}")
    print(f"{Colors.HEADER}{'='*60}{Colors.ENDC}\n")


def print_success(text: str):
    """打印成功信息"""
    print(f"{Colors.OKGREEN}✓ {text}{Colors.ENDC}")


def print_error(text: str):
    """打印错误信息"""
    print(f"{Colors.FAIL}✗ {text}{Colors.ENDC}")


def print_warning(text: str):
    """打印警告信息"""
    print(f"{Colors.WARNING}⚠ {text}{Colors.ENDC}")


def print_info(text: str):
    """打印信息"""
    print(f"{Colors.OKBLUE}ℹ {text}{Colors.ENDC}")


def check_python_version() -> bool:
    """检查Python版本"""
    print_info("检查Python版本...")
    version = sys.version_info
    if version.major < 3 or (version.major == 3 and version.minor < 7):
        print_error(f"Python版本过低: {version.major}.{version.minor}")
        print_info("需要Python 3.7或更高版本")
        return False
    print_success(f"Python版本: {version.major}.{version.minor}.{version.micro}")
    return True


def check_dependencies() -> bool:
    """检查依赖安装"""
    print_info("检查依赖安装...")
    
    required_packages = [
        'pytest',
        'pytest-asyncio',
        'pytest-mock',
        'pytest-cov',
    ]
    
    missing_packages = []
    for package in required_packages:
        try:
            __import__(package.replace('-', '_'))
        except ImportError:
            missing_packages.append(package)
    
    if missing_packages:
        print_error(f"缺少依赖包: {', '.join(missing_packages)}")
        print_info("请运行: pip install " + " ".join(missing_packages))
        return False
    
    print_success("所有依赖已安装")
    return True


def run_tests(test_type: str = "all", verbose: bool = True) -> Dict[str, Any]:
    """
    运行测试
    
    Args:
        test_type: 测试类型 (all/unit/integration/functional)
        verbose: 是否显示详细信息
        
    Returns:
        测试结果字典
    """
    print_header(f"运行 {test_type.upper()} 测试")
    
    # 构建pytest命令
    cmd = [
        sys.executable, "-m", "pytest",
        "-v" if verbose else "-q",
        "--tb=short",
        "--color=yes",
    ]
    
    # 添加覆盖率报告
    if test_type == "all":
        cmd.extend(["--cov=python_executor", "--cov-report=term-missing"])
    
    # 添加测试路径
    tests_dir = project_root / "tests"
    
    if test_type == "unit":
        cmd.append(str(tests_dir / "unit"))
        cmd.extend(["-m", "unit"])
    elif test_type == "integration":
        cmd.append(str(tests_dir / "integration"))
        cmd.extend(["-m", "integration"])
    elif test_type == "functional":
        cmd.append(str(tests_dir / "functional"))
        cmd.extend(["-m", "functional"])
    else:
        cmd.append(str(tests_dir))
    
    # 运行测试
    start_time = time.time()
    result = subprocess.run(cmd, capture_output=True, text=True)
    elapsed_time = time.time() - start_time
    
    # 解析结果
    output = result.stdout + result.stderr
    
    # 提取测试统计
    passed = output.count("PASSED")
    failed = output.count("FAILED")
    error = output.count("ERROR")
    skipped = output.count("SKIPPED")
    
    return {
        "test_type": test_type,
        "return_code": result.returncode,
        "passed": passed,
        "failed": failed,
        "error": error,
        "skipped": skipped,
        "total": passed + failed + error + skipped,
        "elapsed_time": elapsed_time,
        "output": output
    }


def generate_report(results: List[Dict[str, Any]]) -> Dict[str, Any]:
    """生成测试报告"""
    print_header("生成测试报告")
    
    total_passed = sum(r["passed"] for r in results)
    total_failed = sum(r["failed"] for r in results)
    total_error = sum(r["error"] for r in results)
    total_skipped = sum(r["skipped"] for r in results)
    total_tests = sum(r["total"] for r in results)
    total_time = sum(r["elapsed_time"] for r in results)
    
    report = {
        "timestamp": datetime.now().isoformat(),
        "summary": {
            "total": total_tests,
            "passed": total_passed,
            "failed": total_failed,
            "error": total_error,
            "skipped": total_skipped,
            "success_rate": (total_passed / total_tests * 100) if total_tests > 0 else 0,
            "total_time": total_time
        },
        "details": results
    }
    
    # 保存JSON报告
    report_file = project_root / "tests" / "test_report.json"
    with open(report_file, 'w', encoding='utf-8') as f:
        json.dump(report, f, indent=2, ensure_ascii=False)
    
    print_success(f"测试报告已保存: {report_file}")
    
    return report


def print_summary(report: Dict[str, Any]):
    """打印测试摘要"""
    print_header("测试摘要")
    
    summary = report["summary"]
    
    print(f"{Colors.BOLD}总测试数:{Colors.ENDC} {summary['total']}")
    print(f"{Colors.OKGREEN}通过: {summary['passed']}{Colors.ENDC}")
    print(f"{Colors.FAIL}失败: {summary['failed']}{Colors.ENDC}")
    print(f"{Colors.FAIL}错误: {summary['error']}{Colors.ENDC}")
    print(f"{Colors.WARNING}跳过: {summary['skipped']}{Colors.ENDC}")
    print(f"{Colors.BOLD}成功率: {summary['success_rate']:.2f}%{Colors.ENDC}")
    print(f"{Colors.BOLD}总耗时: {summary['total_time']:.2f}秒{Colors.ENDC}")
    
    # 打印详细结果
    print(f"\n{Colors.BOLD}详细结果:{Colors.ENDC}")
    for detail in report["details"]:
        status_color = Colors.OKGREEN if detail["return_code"] == 0 else Colors.FAIL
        print(f"  {status_color}{detail['test_type'].upper()}: "
              f"{detail['passed']} passed, "
              f"{detail['failed']} failed, "
              f"{detail['error']} error "
              f"({detail['elapsed_time']:.2f}s){Colors.ENDC}")


def check_system_health(report: Dict[str, Any]) -> bool:
    """检查系统健康状态"""
    print_header("系统健康检查")
    
    summary = report["summary"]
    
    # 检查测试通过率
    if summary["success_rate"] >= 90:
        print_success("测试通过率高 (>90%)，系统健康")
    elif summary["success_rate"] >= 70:
        print_warning("测试通过率一般 (70-90%)，建议检查")
    else:
        print_error("测试通过率低 (<70%)，系统存在问题")
        return False
    
    # 检查是否有严重错误
    if summary["error"] > 0:
        print_error(f"发现 {summary['error']} 个错误")
        return False
    
    # 检查单元测试是否全部通过
    unit_result = next((r for r in report["details"] if r["test_type"] == "unit"), None)
    if unit_result and unit_result["failed"] > 0:
        print_warning(f"单元测试有 {unit_result['failed']} 个失败")
    
    print_success("系统健康检查完成")
    return True


def main():
    """主函数"""
    import argparse
    
    parser = argparse.ArgumentParser(description="测试平台测试运行器")
    parser.add_argument(
        "--type", "-t",
        choices=["all", "unit", "integration", "functional"],
        default="all",
        help="测试类型 (默认: all)"
    )
    parser.add_argument(
        "--quiet", "-q",
        action="store_true",
        help="安静模式，减少输出"
    )
    parser.add_argument(
        "--no-health-check",
        action="store_true",
        help="跳过健康检查"
    )
    
    args = parser.parse_args()
    
    # 打印欢迎信息
    print_header("测试平台 - 测试运行器")
    print(f"项目路径: {project_root}")
    print(f"测试时间: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    
    # 环境检查
    if not check_python_version():
        sys.exit(1)
    
    if not check_dependencies():
        sys.exit(1)
    
    # 运行测试
    results = []
    
    if args.type == "all":
        # 运行所有类型测试
        for test_type in ["unit", "integration"]:
            result = run_tests(test_type, verbose=not args.quiet)
            results.append(result)
            
            if result["return_code"] != 0 and test_type == "unit":
                print_error("单元测试失败，跳过后续测试")
                break
    else:
        # 运行指定类型测试
        result = run_tests(args.type, verbose=not args.quiet)
        results.append(result)
    
    # 生成报告
    report = generate_report(results)
    
    # 打印摘要
    if not args.quiet:
        print_summary(report)
    
    # 健康检查
    if not args.no_health_check:
        healthy = check_system_health(report)
        if not healthy:
            print_error("系统健康检查未通过")
            sys.exit(1)
    
    # 最终状态
    print_header("测试完成")
    
    total_failed = sum(r["failed"] + r["error"] for r in results)
    if total_failed == 0:
        print_success("所有测试通过！")
        sys.exit(0)
    else:
        print_error(f"有 {total_failed} 个测试失败")
        sys.exit(1)


if __name__ == "__main__":
    main()
