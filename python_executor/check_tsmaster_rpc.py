"""
TSMaster RPC任务执行流程检测脚本
根据TSMaster_RPC_Demo分析.md文档检测实现是否正确
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from utils.logger import get_logger

logger = get_logger("check_tsmaster_rpc")


class TSMasterRPCChecker:
    """TSMaster RPC实现检测器"""
    
    def __init__(self):
        self.issues = []
        self.warnings = []
        self.checks_passed = []
        
    def add_issue(self, level, category, description, suggestion=None):
        """添加问题"""
        self.issues.append({
            'level': level,
            'category': category,
            'description': description,
            'suggestion': suggestion
        })
        
    def add_warning(self, category, description, suggestion=None):
        """添加警告"""
        self.warnings.append({
            'category': category,
            'description': description,
            'suggestion': suggestion
        })
        
    def add_pass(self, category, description):
        """添加通过项"""
        self.checks_passed.append({
            'category': category,
            'description': description
        })
        
    def check_rpc_client(self):
        """检测TSMasterRPCClient实现"""
        logger.info("=" * 80)
        logger.info("检测 TSMasterRPCClient 实现")
        logger.info("=" * 80)
        
        # 读取源代码
        rpc_client_path = os.path.join(os.path.dirname(__file__), "core", "adapters", "tsmaster", "rpc_client.py")
        with open(rpc_client_path, 'r', encoding='utf-8') as f:
            code = f.read()
        
        # 1. 初始化检测
        logger.info("-" * 80)
        logger.info("1. 初始化检测")
        logger.info("-" * 80)
        
        if "initialize_lib_tsmaster" in code:
            self.add_pass("初始化", "正确调用 initialize_lib_tsmaster")
            logger.info("  ✓ 正确调用 initialize_lib_tsmaster")
        else:
            self.add_issue("严重", "初始化", "未找到 initialize_lib_tsmaster 调用")
            logger.error("  ✗ 未找到 initialize_lib_tsmaster 调用")
            
        if "_initialized" in code:
            self.add_pass("初始化", "使用 _initialized 标志防止重复初始化")
            logger.info("  ✓ 使用 _initialized 标志防止重复初始化")
        else:
            self.add_warning("初始化", "未使用标志位防止重复初始化")
            logger.warning("  ⚠ 未使用标志位防止重复初始化")
            
        # 2. 连接流程检测
        logger.info("-" * 80)
        logger.info("2. 连接流程检测")
        logger.info("-" * 80)
        
        if "get_active_application_list" in code:
            self.add_pass("连接", "正确调用 get_active_application_list")
            logger.info("  ✓ 正确调用 get_active_application_list")
        else:
            self.add_issue("严重", "连接", "未找到 get_active_application_list 调用")
            logger.error("  ✗ 未找到 get_active_application_list 调用")
            
        if "rpc_tsmaster_create_client" in code:
            self.add_pass("连接", "正确调用 rpc_tsmaster_create_client")
            logger.info("  ✓ 正确调用 rpc_tsmaster_create_client")
        else:
            self.add_issue("严重", "连接", "未找到 rpc_tsmaster_create_client 调用")
            logger.error("  ✗ 未找到 rpc_tsmaster_create_client 调用")
            
        if "rpc_tsmaster_activate_client" in code:
            self.add_pass("连接", "正确调用 rpc_tsmaster_activate_client")
            logger.info("  ✓ 正确调用 rpc_tsmaster_activate_client")
        else:
            self.add_issue("严重", "连接", "未找到 rpc_tsmaster_activate_client 调用")
            logger.error("  ✗ 未找到 rpc_tsmaster_activate_client 调用")
            
        # 检查错误码处理
        if "ret == 0" in code or "ret != 0" in code:
            self.add_pass("连接", "正确处理API返回的错误码")
            logger.info("  ✓ 正确处理API返回的错误码")
        else:
            self.add_warning("连接", "可能未正确处理API错误码")
            logger.warning("  ⚠ 可能未正确处理API错误码")
            
        # 3. 仿真控制检测
        logger.info("-" * 80)
        logger.info("3. 仿真控制检测")
        logger.info("-" * 80)
        
        if "rpc_tsmaster_cmd_start_simulation" in code:
            self.add_pass("仿真控制", "正确调用 rpc_tsmaster_cmd_start_simulation")
            logger.info("  ✓ 正确调用 rpc_tsmaster_cmd_start_simulation")
        else:
            self.add_issue("严重", "仿真控制", "未找到 rpc_tsmaster_cmd_start_simulation 调用")
            logger.error("  ✗ 未找到 rpc_tsmaster_cmd_start_simulation 调用")
            
        if "rpc_tsmaster_cmd_stop_simulation" in code:
            self.add_pass("仿真控制", "正确调用 rpc_tsmaster_cmd_stop_simulation")
            logger.info("  ✓ 正确调用 rpc_tsmaster_cmd_stop_simulation")
        else:
            self.add_issue("严重", "仿真控制", "未找到 rpc_tsmaster_cmd_stop_simulation 调用")
            logger.error("  ✗ 未找到 rpc_tsmaster_cmd_stop_simulation 调用")
            
        if "simulation_running" in code:
            self.add_pass("仿真控制", "使用 simulation_running 跟踪仿真状态")
            logger.info("  ✓ 使用 simulation_running 跟踪仿真状态")
        else:
            self.add_warning("仿真控制", "未跟踪仿真状态")
            logger.warning("  ⚠ 未跟踪仿真状态")
            
        # 4. 信号操作检测
        logger.info("-" * 80)
        logger.info("4. 信号操作检测")
        logger.info("-" * 80)
        
        if "rpc_tsmaster_cmd_set_can_signal" in code:
            self.add_pass("信号操作", "正确调用 rpc_tsmaster_cmd_set_can_signal")
            logger.info("  ✓ 正确调用 rpc_tsmaster_cmd_set_can_signal")
        else:
            self.add_issue("严重", "信号操作", "未找到 rpc_tsmaster_cmd_set_can_signal 调用")
            logger.error("  ✗ 未找到 rpc_tsmaster_cmd_set_can_signal 调用")
            
        if "rpc_tsmaster_cmd_get_can_signal" in code:
            self.add_pass("信号操作", "正确调用 rpc_tsmaster_cmd_get_can_signal")
            logger.info("  ✓ 正确调用 rpc_tsmaster_cmd_get_can_signal")
        else:
            self.add_issue("严重", "信号操作", "未找到 rpc_tsmaster_cmd_get_can_signal 调用")
            logger.error("  ✗ 未找到 rpc_tsmaster_cmd_get_can_signal 调用")
            
        if "rpc_tsmaster_cmd_set_lin_signal" in code:
            self.add_pass("信号操作", "支持LIN信号设置")
            logger.info("  ✓ 支持LIN信号设置")
        else:
            self.add_warning("信号操作", "未实现LIN信号设置")
            logger.warning("  ⚠ 未实现LIN信号设置")
            
        if "rpc_tsmaster_cmd_get_lin_signal" in code:
            self.add_pass("信号操作", "支持LIN信号获取")
            logger.info("  ✓ 支持LIN信号获取")
        else:
            self.add_warning("信号操作", "未实现LIN信号获取")
            logger.warning("  ⚠ 未实现LIN信号获取")
            
        # 5. 系统变量检测
        logger.info("-" * 80)
        logger.info("5. 系统变量检测")
        logger.info("-" * 80)
        
        if "rpc_tsmaster_cmd_write_system_var" in code:
            self.add_pass("系统变量", "正确调用 rpc_tsmaster_cmd_write_system_var")
            logger.info("  ✓ 正确调用 rpc_tsmaster_cmd_write_system_var")
        else:
            self.add_issue("严重", "系统变量", "未找到 rpc_tsmaster_cmd_write_system_var 调用")
            logger.error("  ✗ 未找到 rpc_tsmaster_cmd_write_system_var 调用")
            
        if "rpc_tsmaster_cmd_read_system_var" in code:
            self.add_pass("系统变量", "正确调用 rpc_tsmaster_cmd_read_system_var")
            logger.info("  ✓ 正确调用 rpc_tsmaster_cmd_read_system_var")
        else:
            self.add_issue("严重", "系统变量", "未找到 rpc_tsmaster_cmd_read_system_var 调用")
            logger.error("  ✗ 未找到 rpc_tsmaster_cmd_read_system_var 调用")
            
        # 6. 资源释放检测
        logger.info("-" * 80)
        logger.info("6. 资源释放检测")
        logger.info("-" * 80)
        
        if "finalize_lib_tsmaster" in code:
            self.add_pass("资源释放", "正确调用 finalize_lib_tsmaster")
            logger.info("  ✓ 正确调用 finalize_lib_tsmaster")
        else:
            self.add_issue("严重", "资源释放", "未找到 finalize_lib_tsmaster 调用")
            logger.error("  ✗ 未找到 finalize_lib_tsmaster 调用")
            
        if "rpc_tsmaster_activate_client" in code and "False" in code:
            self.add_pass("资源释放", "断开连接时停用客户端")
            logger.info("  ✓ 断开连接时停用客户端")
        else:
            self.add_warning("资源释放", "可能未正确停用客户端")
            logger.warning("  ⚠ 可能未正确停用客户端")
            
        # 检查断开连接时是否停止仿真
        if "disconnect" in code and "stop_simulation" in code:
            self.add_pass("资源释放", "断开连接前先停止仿真")
            logger.info("  ✓ 断开连接前先停止仿真")
        else:
            self.add_warning("资源释放", "断开连接时可能未停止仿真")
            logger.warning("  ⚠ 断开连接时可能未停止仿真")
            
        # 7. 其他功能检测
        logger.info("-" * 80)
        logger.info("7. 其他功能检测")
        logger.info("-" * 80)
        
        if "transmit_can" in code or "transmit_can_async" in code:
            self.add_pass("其他功能", "支持CAN报文发送")
            logger.info("  ✓ 支持CAN报文发送")
        else:
            self.add_warning("其他功能", "未实现CAN报文发送")
            logger.warning("  ⚠ 未实现CAN报文发送")
            
        if "__enter__" in code and "__exit__" in code:
            self.add_pass("其他功能", "支持上下文管理器(with语句)")
            logger.info("  ✓ 支持上下文管理器(with语句)")
        else:
            self.add_warning("其他功能", "未实现上下文管理器")
            logger.warning("  ⚠ 未实现上下文管理器")
            
    def check_adapter(self):
        """检测TSMasterAdapter实现"""
        logger.info("=" * 80)
        logger.info("检测 TSMasterAdapter 实现")
        logger.info("=" * 80)
        
        # 读取源代码
        adapter_path = os.path.join(os.path.dirname(__file__), "core", "adapters", "tsmaster_adapter.py")
        with open(adapter_path, 'r', encoding='utf-8') as f:
            code = f.read()
            
        # 1. 连接流程检测
        logger.info("-" * 80)
        logger.info("1. 适配器连接流程检测")
        logger.info("-" * 80)
        
        if "TSMasterRPCClient" in code:
            self.add_pass("适配器", "使用TSMasterRPCClient")
            logger.info("  ✓ 使用TSMasterRPCClient")
        else:
            self.add_issue("严重", "适配器", "未使用TSMasterRPCClient")
            logger.error("  ✗ 未使用TSMasterRPCClient")
            
        if "connect" in code and "rpc_client" in code:
            self.add_pass("适配器", "适配器正确调用RPC客户端连接")
            logger.info("  ✓ 适配器正确调用RPC客户端连接")
        else:
            self.add_warning("适配器", "可能未正确集成RPC客户端")
            logger.warning("  ⚠ 可能未正确集成RPC客户端")
            
        # 2. 仿真控制检测
        logger.info("-" * 80)
        logger.info("2. 适配器仿真控制检测")
        logger.info("-" * 80)
        
        if "start_simulation" in code:
            self.add_pass("适配器", "适配器实现启动仿真")
            logger.info("  ✓ 适配器实现启动仿真")
        else:
            self.add_warning("适配器", "适配器未实现启动仿真")
            logger.warning("  ⚠ 适配器未实现启动仿真")
            
        if "stop_simulation" in code:
            self.add_pass("适配器", "适配器实现停止仿真")
            logger.info("  ✓ 适配器实现停止仿真")
        else:
            self.add_warning("适配器", "适配器未实现停止仿真")
            logger.warning("  ⚠ 适配器未实现停止仿真")
            
        # 3. 测试执行检测
        logger.info("-" * 80)
        logger.info("3. 适配器测试执行检测")
        logger.info("-" * 80)
        
        if "execute_test" in code:
            self.add_pass("适配器", "适配器实现测试执行")
            logger.info("  ✓ 适配器实现测试执行")
        else:
            self.add_warning("适配器", "适配器未实现测试执行")
            logger.warning("  ⚠ 适配器未实现测试执行")
            
        if "TestItemType" in code or "test_type" in code:
            self.add_pass("适配器", "适配器支持不同测试类型")
            logger.info("  ✓ 适配器支持不同测试类型")
        else:
            self.add_warning("适配器", "适配器可能未支持不同测试类型")
            logger.warning("  ⚠ 适配器可能未支持不同测试类型")
            
    def generate_report(self):
        """生成检测报告"""
        logger.info("=" * 80)
        logger.info("检测报告")
        logger.info("=" * 80)
        
        # 统计
        total_checks = len(self.checks_passed) + len(self.warnings) + len([i for i in self.issues if i['level'] == '严重']) + len([i for i in self.issues if i['level'] == '中等'])
        critical_issues = [i for i in self.issues if i['level'] == '严重']
        medium_issues = [i for i in self.issues if i['level'] == '中等']
        
        logger.info(f"检测统计:")
        logger.info(f"  通过项: {len(self.checks_passed)}")
        logger.info(f"  警告: {len(self.warnings)}")
        logger.info(f"  严重问题: {len(critical_issues)}")
        logger.info(f"  中等问题: {len(medium_issues)}")
        logger.info(f"  总计: {total_checks}")
        
        # 严重问题
        if critical_issues:
            logger.error("!" * 80)
            logger.error("严重问题 (必须修复)")
            logger.error("!" * 80)
            for i, issue in enumerate(critical_issues, 1):
                logger.error(f"{i}. [{issue['category']}] {issue['description']}")
                if issue['suggestion']:
                    logger.error(f"   建议: {issue['suggestion']}")
                    
        # 中等问题
        if medium_issues:
            logger.warning("-" * 80)
            logger.warning("中等问题 (建议修复)")
            logger.warning("-" * 80)
            for i, issue in enumerate(medium_issues, 1):
                logger.warning(f"{i}. [{issue['category']}] {issue['description']}")
                if issue['suggestion']:
                    logger.warning(f"   建议: {issue['suggestion']}")
                    
        # 警告
        if self.warnings:
            logger.warning("-" * 80)
            logger.warning("警告 (优化建议)")
            logger.warning("-" * 80)
            for i, warning in enumerate(self.warnings, 1):
                logger.warning(f"{i}. [{warning['category']}] {warning['description']}")
                if warning['suggestion']:
                    logger.warning(f"   建议: {warning['suggestion']}")
                    
        # 通过的检测
        if self.checks_passed:
            logger.info("-" * 80)
            logger.info(f"通过的检测项 ({len(self.checks_passed)}项)")
            logger.info("-" * 80)
            for i, check in enumerate(self.checks_passed[:10], 1):  # 只显示前10项
                logger.info(f"  ✓ [{check['category']}] {check['description']}")
            if len(self.checks_passed) > 10:
                logger.info(f"  ... 还有 {len(self.checks_passed) - 10} 项通过")
                
        # 总结
        logger.info("=" * 80)
        logger.info("总结")
        logger.info("=" * 80)
        
        if not critical_issues and not medium_issues:
            logger.info("✅ 所有关键检测项通过！实现符合TSMaster RPC Demo规范。")
        elif not critical_issues:
            logger.warning("⚠️  存在一些问题，建议修复以完善功能。")
        else:
            logger.error(f"❌ 发现 {len(critical_issues)} 个严重问题，必须修复后才能正常使用。")
            
        return len(critical_issues) == 0


def main():
    """主函数"""
    checker = TSMasterRPCChecker()
    
    # 检测RPC客户端
    checker.check_rpc_client()
    
    # 检测适配器
    checker.check_adapter()
    
    # 生成报告
    success = checker.generate_report()
    
    return 0 if success else 1


if __name__ == "__main__":
    sys.exit(main())
