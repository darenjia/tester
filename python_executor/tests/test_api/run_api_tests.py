"""
API测试运行脚本
便捷地运行所有API测试
"""
import sys
import os
import subprocess
import argparse
from pathlib import Path


class APITestRunner:
    """API测试运行器"""
    
    def __init__(self):
        """初始化"""
        self.script_dir = Path(__file__).parent
        self.project_root = self.script_dir.parent.parent
        
    def run_all_tests(self, verbose: bool = True, html_report: bool = False):
        """
        运行所有API测试
        
        Args:
            verbose: 是否显示详细信息
            html_report: 是否生成HTML报告
        """
        print("=" * 60)
        print("  Python执行器 API测试")
        print("=" * 60)
        print()
        
        # 构建pytest命令
        cmd = ["python", "-m", "pytest", str(self.script_dir)]
        
        if verbose:
            cmd.append("-v")
        
        if html_report:
            report_path = self.script_dir / "reports" / "api_test_report.html"
            report_path.parent.mkdir(exist_ok=True)
            cmd.extend(["--html", str(report_path), "--self-contained-html"])
            print(f"📊 HTML报告将保存至: {report_path}")
            print()
        
        # 运行测试
        print("🚀 开始运行测试...")
        print("-" * 60)
        
        result = subprocess.run(cmd, cwd=str(self.project_root))
        
        print("-" * 60)
        
        if result.returncode == 0:
            print("✅ 所有测试通过!")
        else:
            print("❌ 部分测试失败")
        
        return result.returncode
    
    def run_http_tests(self, verbose: bool = True):
        """
        仅运行HTTP API测试
        
        Args:
            verbose: 是否显示详细信息
        """
        print("=" * 60)
        print("  HTTP API测试")
        print("=" * 60)
        print()
        
        test_file = self.script_dir / "test_http_api.py"
        cmd = ["python", "-m", "pytest", str(test_file)]
        
        if verbose:
            cmd.append("-v")
        
        print("🚀 开始运行HTTP测试...")
        print("-" * 60)
        
        result = subprocess.run(cmd, cwd=str(self.project_root))
        
        print("-" * 60)
        return result.returncode
    
    def run_websocket_tests(self, verbose: bool = True):
        """
        仅运行WebSocket测试
        
        Args:
            verbose: 是否显示详细信息
        """
        print("=" * 60)
        print("  WebSocket测试")
        print("=" * 60)
        print()
        
        test_file = self.script_dir / "test_websocket.py"
        cmd = ["python", "-m", "pytest", str(test_file)]
        
        if verbose:
            cmd.append("-v")
        
        print("🚀 开始运行WebSocket测试...")
        print("-" * 60)
        
        result = subprocess.run(cmd, cwd=str(self.project_root))
        
        print("-" * 60)
        return result.returncode
    
    def run_scenario_tests(self, verbose: bool = True):
        """
        仅运行场景测试
        
        Args:
            verbose: 是否显示详细信息
        """
        print("=" * 60)
        print("  场景测试")
        print("=" * 60)
        print()
        
        test_file = self.script_dir / "test_scenarios.py"
        cmd = ["python", "-m", "pytest", str(test_file)]
        
        if verbose:
            cmd.append("-v")
        
        print("🚀 开始运行场景测试...")
        print("-" * 60)
        
        result = subprocess.run(cmd, cwd=str(self.project_root))
        
        print("-" * 60)
        return result.returncode
    
    def check_dependencies(self):
        """检查测试依赖"""
        print("🔍 检查测试依赖...")
        print()
        
        missing = []
        
        # 检查pytest
        try:
            import pytest
            print("✅ pytest 已安装")
        except ImportError:
            missing.append("pytest")
            print("❌ pytest 未安装")
        
        # 检查requests
        try:
            import requests
            print("✅ requests 已安装")
        except ImportError:
            missing.append("requests")
            print("❌ requests 未安装")
        
        # 检查websocket-client
        try:
            import websocket
            print("✅ websocket-client 已安装")
        except ImportError:
            missing.append("websocket-client")
            print("❌ websocket-client 未安装")
        
        # 检查pytest-html（可选）
        try:
            import pytest_html
            print("✅ pytest-html 已安装")
        except ImportError:
            print("⚠️  pytest-html 未安装（可选，用于生成HTML报告）")
        
        print()
        
        if missing:
            print("❌ 缺少以下依赖:")
            for dep in missing:
                print(f"   - {dep}")
            print()
            print("请运行以下命令安装:")
            print(f"   pip install {' '.join(missing)}")
            return False
        else:
            print("✅ 所有必需依赖已安装")
            return True
    
    def show_help(self):
        """显示帮助信息"""
        help_text = """
Python执行器 API测试运行脚本

用法:
    python run_api_tests.py [选项]

选项:
    -a, --all           运行所有测试（默认）
    -h, --http          仅运行HTTP API测试
    -w, --websocket     仅运行WebSocket测试
    -s, --scenario      仅运行场景测试
    -r, --report        生成HTML测试报告
    -c, --check         检查依赖
    --help              显示此帮助信息

示例:
    # 运行所有测试
    python run_api_tests.py

    # 运行所有测试并生成报告
    python run_api_tests.py -r

    # 仅运行HTTP测试
    python run_api_tests.py -h

    # 仅运行WebSocket测试
    python run_api_tests.py -w

    # 检查依赖
    python run_api_tests.py -c
"""
        print(help_text)


def main():
    """主函数"""
    parser = argparse.ArgumentParser(
        description='Python执行器 API测试运行脚本',
        formatter_class=argparse.RawDescriptionHelpFormatter
    )
    
    parser.add_argument(
        '-a', '--all',
        action='store_true',
        help='运行所有测试（默认）'
    )
    parser.add_argument(
        '-t', '--http',
        action='store_true',
        help='仅运行HTTP API测试'
    )
    parser.add_argument(
        '-w', '--websocket',
        action='store_true',
        help='仅运行WebSocket测试'
    )
    parser.add_argument(
        '-s', '--scenario',
        action='store_true',
        help='仅运行场景测试'
    )
    parser.add_argument(
        '-r', '--report',
        action='store_true',
        help='生成HTML测试报告'
    )
    parser.add_argument(
        '-c', '--check',
        action='store_true',
        help='检查依赖'
    )
    parser.add_argument(
        '-q', '--quiet',
        action='store_true',
        help='静默模式（减少输出）'
    )
    
    args = parser.parse_args()
    
    runner = APITestRunner()
    
    # 检查依赖
    if args.check:
        success = runner.check_dependencies()
        return 0 if success else 1
    
    # 如果没有指定任何选项，默认运行所有测试
    if not any([args.http, args.websocket, args.scenario]):
        args.all = True
    
    # 先检查依赖
    if not runner.check_dependencies():
        return 1
    
    print()
    
    # 根据选项运行测试
    verbose = not args.quiet
    
    if args.http:
        return runner.run_http_tests(verbose=verbose)
    elif args.websocket:
        return runner.run_websocket_tests(verbose=verbose)
    elif args.scenario:
        return runner.run_scenario_tests(verbose=verbose)
    else:
        return runner.run_all_tests(verbose=verbose, html_report=args.report)


if __name__ == '__main__':
    sys.exit(main())
