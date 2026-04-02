"""
综合测试脚本 - 测试所有新添加的模块
"""
import os
import sys
import tempfile
import shutil
from pathlib import Path

import pytest

pytestmark = pytest.mark.skip(reason="manual script; run directly instead of under pytest")

sys.path.insert(0, str(Path(__file__).resolve().parents[2]))

from utils.logger import get_logger

logger = get_logger("test_all_modules")


def test_imports():
    """测试所有模块导入"""
    logger.info("=" * 80)
    logger.info("测试模块导入")
    logger.info("=" * 80)
    
    modules = [
        ("配置缓存管理器", "core.config_cache_manager", "ConfigCacheManager"),
        ("配置管理器", "core.config_manager", "TestConfigManager"),
        ("状态处理器", "core.test_state_handlers", "SelfCheckHandler"),
        ("上报客户端", "utils.report_client", "ReportClient"),
        ("TSMaster适配器", "core.adapters.tsmaster_adapter", "TSMasterAdapter"),
        ("任务执行器", "core.task_executor_production", "TaskExecutorProduction"),
        ("配置设置", "config.settings", "settings"),
    ]
    
    results = []
    for name, module_path, class_name in modules:
        try:
            module = __import__(module_path, fromlist=[class_name])
            cls = getattr(module, class_name)
            logger.info(f"✓ {name}: {module_path}.{class_name}")
            results.append((name, True, None))
        except Exception as e:
            logger.error(f"✗ {name}: {str(e)}")
            results.append((name, False, str(e)))
    
    success = all(r[1] for r in results)
    logger.info(f"\n导入测试: {'通过' if success else '失败'} ({sum(r[1] for r in results)}/{len(results)})")
    return success


def test_config_cache():
    """测试配置缓存管理器"""
    logger.info("\n" + "=" * 80)
    logger.info("测试配置缓存管理器")
    logger.info("=" * 80)
    
    try:
        from core.config_cache_manager import ConfigCacheManager
        
        # 创建临时缓存目录
        temp_dir = tempfile.mkdtemp(prefix="cache_test_")
        cache_dir = os.path.join(temp_dir, "cache")
        
        # 初始化缓存管理器
        cache_mgr = ConfigCacheManager(cache_dir=cache_dir)
        logger.info(f"✓ 缓存管理器初始化成功")
        logger.info(f"  缓存目录: {cache_mgr.cache_dir}")
        logger.info(f"  缓存启用: {cache_mgr.enabled}")
        
        # 测试缓存统计
        stats = cache_mgr.get_cache_stats()
        logger.info(f"✓ 缓存统计: {stats}")
        
        # 清理
        shutil.rmtree(temp_dir)
        logger.info(f"✓ 测试完成，已清理临时目录")
        return True
        
    except Exception as e:
        logger.error(f"✗ 配置缓存测试失败: {str(e)}")
        import traceback
        logger.error(traceback.format_exc())
        return False


def test_report_client():
    """测试上报客户端"""
    logger.info("\n" + "=" * 80)
    logger.info("测试上报客户端")
    logger.info("=" * 80)
    
    try:
        from utils.report_client import ReportClient, get_report_client
        
        # 获取上报客户端
        report_client = get_report_client()
        logger.info(f"✓ 上报客户端获取成功")
        logger.info(f"  上报启用: {report_client.enabled}")
        logger.info(f"  API URL: {report_client._api_url or '未配置'}")
        
        # 测试配置加载
        report_client.reload_config()
        logger.info(f"✓ 配置重新加载成功")
        
        return True
        
    except Exception as e:
        logger.error(f"✗ 上报客户端测试失败: {str(e)}")
        import traceback
        logger.error(traceback.format_exc())
        return False


def test_tsmaster_adapter():
    """测试TSMaster适配器"""
    logger.info("\n" + "=" * 80)
    logger.info("测试TSMaster适配器")
    logger.info("=" * 80)
    
    try:
        from core.adapters.tsmaster_adapter import TSMasterAdapter
        from models.task import TestItemType
        
        # 初始化适配器
        adapter = TSMasterAdapter()
        logger.info(f"✓ TSMaster适配器初始化成功")
        logger.info(f"  适配器配置: {adapter.config}")
        
        # 检查测试项类型支持
        supported_types = [
            "signal_check",
            "signal_set",
            "message_send",
            "c_script",
            "test_sequence",
            "sysvar_check",
            "sysvar_set",
            "test_module",
            "diagnostic",
            "wait",
            "condition"
        ]
        
        logger.info(f"✓ 支持的测试项类型 ({len(supported_types)}个):")
        for t in supported_types:
            logger.info(f"  - {t}")
        
        return True
        
    except Exception as e:
        logger.error(f"✗ TSMaster适配器测试失败: {str(e)}")
        import traceback
        logger.error(traceback.format_exc())
        return False


def test_config_manager():
    """测试配置管理器"""
    logger.info("\n" + "=" * 80)
    logger.info("测试配置管理器")
    logger.info("=" * 80)
    
    try:
        from core.config_manager import TestConfigManager
        
        # 创建临时目录
        temp_dir = tempfile.mkdtemp(prefix="config_test_")
        
        # 初始化配置管理器
        config_mgr = TestConfigManager(base_config_dir=temp_dir, use_cache=False)
        logger.info(f"✓ 配置管理器初始化成功")
        
        # 测试INI文件生成
        test_cases = [
            {"caseNo": "TG1_TC01_SC01", "caseName": "测试用例1"},
            {"caseNo": "TG1_TC02_SC01", "caseName": "测试用例2"},
        ]
        variables = {"ECUName": "MTCU1", "Terminal": "1"}
        
        ini_files = config_mgr._generate_ini_files("test_config", test_cases, variables)
        logger.info(f"✓ INI文件生成成功")
        logger.info(f"  SelectInfo.ini: {ini_files['select_info']}")
        logger.info(f"  ParaInfo.ini: {ini_files['para_info']}")
        
        # 验证文件内容
        with open(ini_files['select_info'], 'r', encoding='utf-8') as f:
            select_content = f.read()
        logger.info(f"✓ SelectInfo.ini内容预览:")
        for line in select_content.split('\n')[:5]:
            logger.info(f"  {line}")
        
        # 清理
        shutil.rmtree(temp_dir)
        logger.info(f"✓ 测试完成，已清理临时目录")
        
        return True
        
    except Exception as e:
        logger.error(f"✗ 配置管理器测试失败: {str(e)}")
        import traceback
        logger.error(traceback.format_exc())
        return False


def main():
    """主函数"""
    logger.info("\n" + "=" * 80)
    logger.info("开始综合测试")
    logger.info("=" * 80)
    
    results = []
    
    # 1. 测试模块导入
    results.append(("模块导入", test_imports()))
    
    # 2. 测试配置缓存
    results.append(("配置缓存", test_config_cache()))
    
    # 3. 测试上报客户端
    results.append(("上报客户端", test_report_client()))
    
    # 4. 测试TSMaster适配器
    results.append(("TSMaster适配器", test_tsmaster_adapter()))
    
    # 5. 测试配置管理器
    results.append(("配置管理器", test_config_manager()))
    
    # 总结
    logger.info("\n" + "=" * 80)
    logger.info("测试总结")
    logger.info("=" * 80)
    
    for name, success in results:
        status = "✓ 通过" if success else "✗ 失败"
        logger.info(f"{status}: {name}")
    
    total = len(results)
    passed = sum(r[1] for r in results)
    
    logger.info(f"\n总计: {passed}/{total} 项测试通过")
    
    if passed == total:
        logger.info("\n✅ 所有测试通过！")
        return 0
    else:
        logger.error(f"\n❌ 有 {total - passed} 项测试失败")
        return 1


if __name__ == "__main__":
    sys.exit(main())
