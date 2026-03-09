"""
检查适配器是否完整支持所有测试项类型
"""
import os
import sys
import inspect

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from models.task import TestItemType

from utils.logger import get_logger

logger = get_logger("check_test_item_types")



def check_tsmaster_adapter():
    """检查TSMasterAdapter的测试项类型支持"""
    logger.info("=" * 80)
    logger.info("检查 TSMasterAdapter 测试项类型支持")
    logger.info("=" * 80)
    
    # 读取适配器源代码
    adapter_path = os.path.join(os.path.dirname(__file__), "core", "adapters", "tsmaster_adapter.py")
    with open(adapter_path, 'r', encoding='utf-8') as f:
        code = f.read()
    
    # 定义的测试项类型
    defined_types = {
        "signal_check": "信号检查",
        "signal_set": "信号设置",
        "message_send": "发送报文",
        "c_script": "执行C脚本",
        "test_sequence": "执行测试序列",
        "sysvar_check": "系统变量检查",
        "sysvar_set": "系统变量设置",
        "test_module": "测试模块",
        "diagnostic": "诊断",
        "wait": "等待",
        "condition": "条件"
    }
    
    # 模型中定义的测试项类型
    model_types = {
        TestItemType.SIGNAL_CHECK.value: "信号检查",
        TestItemType.SIGNAL_SET.value: "信号设置",
        TestItemType.TEST_MODULE.value: "测试模块",
        TestItemType.DIAGNOSTIC.value: "诊断",
        TestItemType.WAIT.value: "等待",
        TestItemType.CONDITION.value: "条件"
    }
    
    logger.info("\n" + "-" * 80)
    logger.info("1. 适配器中定义的测试项类型")
    logger.info("-" * 80)
    
    supported_types = []
    unsupported_types = []
    
    for type_name, type_desc in defined_types.items():
        # 检查execute_test_item方法中是否有对应的处理
        if f'elif item_type == "{type_name}"' in code or f'if item_type == "{type_name}"' in code:
            supported_types.append((type_name, type_desc))
            logger.info(f"  ✓ {type_name}: {type_desc}")
        else:
            unsupported_types.append((type_name, type_desc))
            logger.info(f"  ✗ {type_name}: {type_desc} - 未找到处理逻辑")
    
    logger.info("\n" + "-" * 80)
    logger.info("2. 与模型定义(TestItemType)的对比")
    logger.info("-" * 80)
    
    # 检查模型中定义的类型是否在适配器中支持
    for type_name, type_desc in model_types.items():
        if type_name in [t[0] for t in supported_types]:
            logger.info(f"  ✓ {type_name}: {type_desc} - 已支持")
        else:
            logger.info(f"  ✗ {type_name}: {type_desc} - 未支持")
    
    logger.info("\n" + "-" * 80)
    logger.info("3. 各测试项类型的执行方法检查")
    logger.info("-" * 80)
    
    # 检查每个测试项类型的执行方法
    execute_methods = {
        "signal_check": "_execute_signal_check",
        "signal_set": "_execute_signal_set",
        "message_send": "_execute_message_send",
        "c_script": "_execute_c_script",
        "test_sequence": "_execute_test_sequence",
        "sysvar_check": "_execute_sysvar_check",
        "sysvar_set": "_execute_sysvar_set",
        "test_module": "_execute_test_module",
        "diagnostic": "_execute_diagnostic",
        "wait": "_execute_wait",
        "condition": "_execute_condition"
    }
    
    for type_name, method_name in execute_methods.items():
        if f"def {method_name}(" in code:
            logger.info(f"  ✓ {type_name}: 方法 {method_name} 已定义")
        else:
            logger.info(f"  ✗ {type_name}: 方法 {method_name} 未定义")
    
    logger.info("\n" + "-" * 80)
    logger.info("4. 底层方法支持检查")
    logger.info("-" * 80)
    
    # 检查底层方法
    low_level_methods = {
        "_get_signal": "获取信号值",
        "_set_signal": "设置信号值",
        "_send_message": "发送CAN报文",
        "_read_system_var": "读取系统变量",
        "_write_system_var": "写入系统变量",
        "_call_c_script": "调用C脚本"
    }
    
    for method_name, method_desc in low_level_methods.items():
        if f"def {method_name}(" in code:
            logger.info(f"  ✓ {method_name}: {method_desc}")
        else:
            logger.info(f"  ✗ {method_name}: {method_desc}")
    
    logger.info("\n" + "-" * 80)
    logger.info("5. 双模式支持检查 (RPC和传统模式)")
    logger.info("-" * 80)
    
    # 检查是否支持双模式
    if "_using_rpc" in code:
        logger.info("  ✓ 支持双模式切换 (RPC和传统模式)")
    else:
        logger.info("  ✗ 可能不支持双模式切换")
    
    # 检查底层方法是否都支持双模式
    dual_mode_support = True
    for method_name in low_level_methods.keys():
        method_start = code.find(f"def {method_name}(")
        if method_start != -1:
            method_end = code.find("\n    def ", method_start + 1)
            if method_end == -1:
                method_end = len(code)
            method_code = code[method_start:method_end]
            
            if "_using_rpc" in method_code and "_rpc_client" in method_code:
                logger.info(f"  ✓ {method_name}: 支持双模式")
            else:
                logger.info(f"  ⚠ {method_name}: 可能不完全支持双模式")
                dual_mode_support = False
    
    return supported_types, unsupported_types


def check_canoe_adapter():
    """检查CANoeAdapter的测试项类型支持"""
    logger.info("\n" + "=" * 80)
    logger.info("检查 CANoeAdapter 测试项类型支持")
    logger.info("=" * 80)
    
    # 读取适配器源代码
    adapter_path = os.path.join(os.path.dirname(__file__), "core", "adapters", "canoe_adapter.py")
    
    if not os.path.exists(adapter_path):
        logger.info("  ✗ CANoeAdapter文件不存在")
        return [], []
    
    with open(adapter_path, 'r', encoding='utf-8') as f:
        code = f.read()
    
    # 检查execute_test_item方法
    if "def execute_test_item" in code:
        logger.info("  ✓ 实现了 execute_test_item 方法")
    else:
        logger.info("  ✗ 未实现 execute_test_item 方法")
    
    # 检查支持的测试项类型
    model_types = {
        TestItemType.SIGNAL_CHECK.value: "信号检查",
        TestItemType.SIGNAL_SET.value: "信号设置",
        TestItemType.TEST_MODULE.value: "测试模块",
        TestItemType.DIAGNOSTIC.value: "诊断",
        TestItemType.WAIT.value: "等待",
        TestItemType.CONDITION.value: "条件"
    }
    
    logger.info("\n  支持的测试项类型:")
    for type_name, type_desc in model_types.items():
        if type_name in code:
            logger.info(f"    ✓ {type_name}: {type_desc}")
        else:
            logger.info(f"    ✗ {type_name}: {type_desc} - 未找到")
    
    return [], []


def generate_report(tsmaster_supported, tsmaster_unsupported):
    """生成检测报告"""
    logger.info("\n" + "=" * 80)
    logger.info("检测报告")
    logger.info("=" * 80)
    
    logger.info("\nTSMasterAdapter:")
    logger.info(f"  支持的测试项类型: {len(tsmaster_supported)} 个")
    for type_name, type_desc in tsmaster_supported:
        logger.info(f"    - {type_name}: {type_desc}")
    
    if tsmaster_unsupported:
        logger.info(f"\n  未支持的测试项类型: {len(tsmaster_unsupported)} 个")
        for type_name, type_desc in tsmaster_unsupported:
            logger.info(f"    - {type_name}: {type_desc}")
    
    # 与模型定义对比
    logger.info("\n与模型定义(TestItemType)对比:")
    model_types = [t.value for t in TestItemType]
    adapter_types = [t[0] for t in tsmaster_supported]
    
    missing_in_adapter = set(model_types) - set(adapter_types)
    extra_in_adapter = set(adapter_types) - set(model_types)
    
    if missing_in_adapter:
        logger.info(f"  模型中有但适配器未支持的类型:")
        for type_name in missing_in_adapter:
            logger.info(f"    - {type_name}")
    else:
        logger.info("  ✓ 适配器支持所有模型定义的测试项类型")
    
    if extra_in_adapter:
        logger.info(f"  适配器中有但模型未定义的类型:")
        for type_name in extra_in_adapter:
            logger.info(f"    - {type_name}")
    
    logger.info("\n" + "=" * 80)
    logger.info("总结")
    logger.info("=" * 80)
    
    if not missing_in_adapter:
        logger.info("\n✅ TSMasterAdapter 完整支持所有模型定义的测试项类型！")
    else:
        logger.info(f"\n⚠️  TSMasterAdapter 缺少 {len(missing_in_adapter)} 个测试项类型的支持")


def main():
    """主函数"""
    # 检查TSMasterAdapter
    tsmaster_supported, tsmaster_unsupported = check_tsmaster_adapter()
    
    # 检查CANoeAdapter
    check_canoe_adapter()
    
    # 生成报告
    generate_report(tsmaster_supported, tsmaster_unsupported)


if __name__ == "__main__":
    main()
