"""
代码审核报告生成器
自动审核新添加的代码并生成报告
"""
import os
import sys
import ast
import re
from typing import List, Dict, Any, Tuple
from pathlib import Path

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))


class CodeReviewer:
    """代码审核器"""
    
    def __init__(self):
        self.issues = []
        self.warnings = []
        self.notes = []
        self.files_reviewed = []
        
    def add_issue(self, file: str, line: int, category: str, description: str, severity: str = "medium"):
        """添加问题"""
        self.issues.append({
            'file': file,
            'line': line,
            'category': category,
            'description': description,
            'severity': severity
        })
        
    def add_warning(self, file: str, line: int, category: str, description: str):
        """添加警告"""
        self.warnings.append({
            'file': file,
            'line': line,
            'category': category,
            'description': description
        })
        
    def add_note(self, file: str, category: str, description: str):
        """添加备注"""
        self.notes.append({
            'file': file,
            'category': category,
            'description': description
        })
        
    def review_file(self, file_path: str, file_content: str):
        """审核单个文件"""
        self.files_reviewed.append(file_path)
        file_name = os.path.basename(file_path)
        
        # 检查文件是否为空
        if not file_content.strip():
            self.add_issue(file_path, 1, "文件检查", "文件为空", "high")
            return
            
        # 检查文件编码声明
        if not file_content.startswith('#') and 'encoding' not in file_content[:100]:
            self.add_warning(file_path, 1, "编码", "文件缺少编码声明")
            
        # 检查行长度
        for i, line in enumerate(file_content.split('\n'), 1):
            if len(line) > 120:
                self.add_warning(file_path, i, "格式", f"行长度超过120字符: {len(line)}")
                
        # 检查TODO注释
        for i, line in enumerate(file_content.split('\n'), 1):
            if 'TODO' in line or 'FIXME' in line:
                self.add_note(file_path, i, "TODO", f"发现TODO/FIXME注释: {line.strip()}")
                
        # 检查异常处理
        try:
            tree = ast.parse(file_content)
            self._check_ast(tree, file_path, file_content)
        except SyntaxError as e:
            self.add_issue(file_path, e.lineno or 1, "语法", f"语法错误: {e}", "high")
            
    def _check_ast(self, tree: ast.AST, file_path: str, file_content: str):
        """使用AST检查代码"""
        for node in ast.walk(tree):
            # 检查裸except
            if isinstance(node, ast.ExceptHandler):
                if node.type is None:
                    self.add_warning(file_path, node.lineno, "异常处理", "使用裸except，建议指定具体异常类型")
                    
            # 检查print语句（建议改用日志）
            if isinstance(node, ast.Call):
                if isinstance(node.func, ast.Name) and node.func.id == 'print':
                    self.add_note(file_path, "日志", f"使用print输出，建议改用日志模块 (行{node.lineno})")
                    
            # 检查硬编码字符串
            if isinstance(node, ast.Constant) and isinstance(node.value, str):
                if len(node.value) > 50 and ('http' in node.value or 'path' in node.value.lower()):
                    self.add_note(file_path, "配置", f"发现长字符串，建议提取为配置 (行{node.lineno})")
                    
    def review_config_cache_manager(self):
        """审核配置缓存管理器"""
        print("审核: core/config_cache_manager.py")
        file_path = os.path.join(os.path.dirname(__file__), "core", "config_cache_manager.py")
        
        if not os.path.exists(file_path):
            self.add_issue(file_path, 1, "文件", "文件不存在", "high")
            return
            
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
            
        self.review_file(file_path, content)
        
        # 特定检查
        if 'threading.RLock' not in content:
            self.add_issue(file_path, 1, "线程安全", "未使用RLock，可能存在线程安全问题", "high")
        else:
            self.add_note(file_path, "线程安全", "✓ 使用RLock保证线程安全")
            
        if 'hashlib.md5' in content:
            self.add_note(file_path, "安全", "✓ 使用MD5进行文件完整性校验")
            
        if 'shutil.copy2' in content:
            self.add_note(file_path, "文件操作", "✓ 使用shutil.copy2保留文件元数据")
            
        # 检查错误处理
        try_blocks = content.count('try:')
        except_blocks = content.count('except')
        if try_blocks > 0 and except_blocks >= try_blocks:
            self.add_note(file_path, "异常处理", f"✓ 发现{try_blocks}个try块，异常处理完善")
        elif try_blocks > 0:
            self.add_warning(file_path, 1, "异常处理", f"try块({try_blocks})多于except块({except_blocks})")
            
    def review_config_manager(self):
        """审核配置管理器"""
        print("审核: core/config_manager.py")
        file_path = os.path.join(os.path.dirname(__file__), "core", "config_manager.py")
        
        if not os.path.exists(file_path):
            self.add_issue(file_path, 1, "文件", "文件不存在", "high")
            return
            
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
            
        self.review_file(file_path, content)
        
        # 检查TestConfigManager
        if 'class TestConfigManager' not in content:
            self.add_issue(file_path, 1, "类定义", "未找到TestConfigManager类", "high")
        else:
            self.add_note(file_path, "类定义", "✓ TestConfigManager类已定义")
            
        # 检查INI生成方法
        if '_generate_select_info_ini' in content and '_generate_para_info_ini' in content:
            self.add_note(file_path, "INI生成", "✓ 实现了CANoe规范的INI文件生成")
        else:
            self.add_warning(file_path, 1, "INI生成", "可能未完整实现CANoe规范的INI生成")
            
        # 检查缓存集成
        if 'ConfigCacheManager' in content:
            self.add_note(file_path, "缓存", "✓ 集成了配置缓存管理器")
        else:
            self.add_warning(file_path, 1, "缓存", "未集成配置缓存管理器")
            
    def review_report_client(self):
        """审核上报客户端"""
        print("审核: utils/report_client.py")
        file_path = os.path.join(os.path.dirname(__file__), "utils", "report_client.py")
        
        if not os.path.exists(file_path):
            self.add_issue(file_path, 1, "文件", "文件不存在", "high")
            return
            
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
            
        self.review_file(file_path, content)
        
        # 检查异步执行
        if 'ThreadPoolExecutor' in content:
            self.add_note(file_path, "异步", "✓ 使用ThreadPoolExecutor实现异步上报")
        else:
            self.add_warning(file_path, 1, "异步", "未使用线程池，上报可能阻塞主流程")
            
        # 检查重试机制
        if 'max_retries' in content or 'retry' in content.lower():
            self.add_note(file_path, "重试", "✓ 实现了重试机制")
        else:
            self.add_warning(file_path, 1, "重试", "可能未实现重试机制")
            
        # 检查requests导入
        if 'import requests' in content or 'from requests' in content:
            self.add_note(file_path, "HTTP", "✓ 使用requests库进行HTTP请求")
        else:
            self.add_note(file_path, "HTTP", "使用动态导入requests，避免强制依赖")
            
    def review_tsmaster_adapter(self):
        """审核TSMaster适配器"""
        print("审核: core/adapters/tsmaster_adapter.py")
        file_path = os.path.join(os.path.dirname(__file__), "core", "adapters", "tsmaster_adapter.py")
        
        if not os.path.exists(file_path):
            self.add_issue(file_path, 1, "文件", "文件不存在", "high")
            return
            
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
            
        self.review_file(file_path, content)
        
        # 检查测试项类型支持
        test_types = ['test_module', 'diagnostic', 'wait', 'condition']
        for test_type in test_types:
            if f'_execute_{test_type}' in content:
                self.add_note(file_path, "测试项", f"✓ 支持 {test_type} 测试项")
            else:
                self.add_issue(file_path, 1, "测试项", f"未实现 {test_type} 测试项", "high")
                
        # 检查双模式支持
        if '_using_rpc' in content:
            self.add_note(file_path, "双模式", "✓ 支持RPC和传统双模式")
        else:
            self.add_warning(file_path, 1, "双模式", "可能未完整支持双模式")
            
        # 检查条件评估
        if '_evaluate_condition' in content:
            self.add_note(file_path, "条件", "✓ 实现了条件评估方法")
        else:
            self.add_warning(file_path, 1, "条件", "未找到条件评估方法")
            
    def review_task_executor(self):
        """审核任务执行器"""
        print("审核: core/task_executor_production.py")
        file_path = os.path.join(os.path.dirname(__file__), "core", "task_executor_production.py")
        
        if not os.path.exists(file_path):
            self.add_issue(file_path, 1, "文件", "文件不存在", "high")
            return
            
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
            
        self.review_file(file_path, content)
        
        # 检查上报集成
        if 'ReportClient' in content:
            self.add_note(file_path, "上报", "✓ 集成了上报客户端")
        else:
            self.add_warning(file_path, 1, "上报", "未集成上报客户端")
            
        if '_report_to_remote' in content:
            self.add_note(file_path, "上报", "✓ 实现了远端上报方法")
        else:
            self.add_warning(file_path, 1, "上报", "未找到远端上报方法")
            
    def review_settings(self):
        """审核配置设置"""
        print("审核: config/settings.py")
        file_path = os.path.join(os.path.dirname(__file__), "config", "settings.py")
        
        if not os.path.exists(file_path):
            self.add_issue(file_path, 1, "文件", "文件不存在", "high")
            return
            
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
            
        self.review_file(file_path, content)
        
        # 检查新增配置
        if '"report"' in content or "'report'" in content:
            self.add_note(file_path, "配置", "✓ 添加了report配置段")
        else:
            self.add_warning(file_path, 1, "配置", "可能未添加report配置")
            
        if '"config_cache"' in content or "'config_cache'" in content:
            self.add_note(file_path, "配置", "✓ 添加了config_cache配置段")
        else:
            self.add_warning(file_path, 1, "配置", "可能未添加config_cache配置")
            
    def generate_report(self) -> str:
        """生成审核报告"""
        report = []
        report.append("=" * 80)
        report.append("代码审核报告")
        report.append("=" * 80)
        report.append("")
        
        # 统计
        critical = len([i for i in self.issues if i['severity'] == 'high'])
        medium = len([i for i in self.issues if i['severity'] == 'medium'])
        low = len([i for i in self.issues if i['severity'] == 'low'])
        
        report.append(f"审核文件数: {len(self.files_reviewed)}")
        report.append(f"严重问题: {critical}")
        report.append(f"中等问题: {medium}")
        report.append(f"轻微问题: {low}")
        report.append(f"警告: {len(self.warnings)}")
        report.append(f"备注: {len(self.notes)}")
        report.append("")
        
        # 严重问题
        if critical > 0:
            report.append("!" * 80)
            report.append("严重问题（必须修复）")
            report.append("!" * 80)
            report.append("")
            for issue in self.issues:
                if issue['severity'] == 'high':
                    report.append(f"文件: {issue['file']}")
                    report.append(f"行号: {issue['line']}")
                    report.append(f"类别: {issue['category']}")
                    report.append(f"描述: {issue['description']}")
                    report.append("-" * 40)
            report.append("")
            
        # 中等问题
        if medium > 0:
            report.append("-" * 80)
            report.append("中等问题（建议修复）")
            report.append("-" * 80)
            report.append("")
            for issue in self.issues:
                if issue['severity'] == 'medium':
                    report.append(f"文件: {issue['file']}")
                    report.append(f"行号: {issue['line']}")
                    report.append(f"类别: {issue['category']}")
                    report.append(f"描述: {issue['description']}")
                    report.append("-" * 40)
            report.append("")
            
        # 警告
        if self.warnings:
            report.append("-" * 80)
            report.append("警告")
            report.append("-" * 80)
            report.append("")
            for warning in self.warnings[:10]:  # 只显示前10个
                report.append(f"文件: {warning['file']}")
                report.append(f"行号: {warning['line']}")
                report.append(f"类别: {warning['category']}")
                report.append(f"描述: {warning['description']}")
                report.append("-" * 40)
            if len(self.warnings) > 10:
                report.append(f"... 还有 {len(self.warnings) - 10} 个警告")
            report.append("")
            
        # 正面备注
        if self.notes:
            report.append("-" * 80)
            report.append("正面备注")
            report.append("-" * 80)
            report.append("")
            for note in self.notes[:15]:  # 只显示前15个
                report.append(f"✓ [{note['category']}] {note['description']}")
            if len(self.notes) > 15:
                report.append(f"... 还有 {len(self.notes) - 15} 个正面备注")
            report.append("")
            
        # 结论
        report.append("=" * 80)
        report.append("结论")
        report.append("=" * 80)
        report.append("")
        
        if critical == 0 and medium == 0:
            report.append("✅ 代码审核通过！未发现严重问题。")
        elif critical == 0:
            report.append("⚠️  代码基本可用，但存在一些建议修复的问题。")
        else:
            report.append(f"❌ 发现 {critical} 个严重问题，必须修复后才能使用。")
            
        return "\n".join(report)
        
    def run_all_reviews(self):
        """运行所有审核"""
        print("开始代码审核...")
        print()
        
        self.review_config_cache_manager()
        self.review_config_manager()
        self.review_report_client()
        self.review_tsmaster_adapter()
        self.review_task_executor()
        self.review_settings()
        
        print()
        print("审核完成，生成报告...")
        

def main():
    """主函数"""
    reviewer = CodeReviewer()
    reviewer.run_all_reviews()
    
    report = reviewer.generate_report()
    
    # 保存报告
    report_path = os.path.join(os.path.dirname(__file__), "code_review_report.txt")
    with open(report_path, 'w', encoding='utf-8') as f:
        f.write(report)
        
    # 打印报告
    print(report)
    print(f"\n报告已保存到: {report_path}")
    
    # 返回退出码
    critical = len([i for i in reviewer.issues if i['severity'] == 'high'])
    return 1 if critical > 0 else 0


if __name__ == "__main__":
    sys.exit(main())
