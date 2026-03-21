"""
检测程序对.ini配置文件的写入操作是否正确
"""
import os
import sys
import tempfile
import shutil
from datetime import datetime

# 添加项目路径
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from utils.logger import get_logger
from core.config_manager import TestConfigManager
from core.config_cache_manager import ConfigCacheManager, get_config_cache_manager

logger = get_logger("check_ini_operations")


def test_ini_write_operation():
    """测试ini文件写入操作"""
    logger.info("=" * 60)
    logger.info("开始检测.ini文件写入操作")
    logger.info("=" * 60)
    
    # 创建临时目录用于测试
    temp_dir = tempfile.mkdtemp(prefix="ini_test_")
    logger.info(f"\n测试目录: {temp_dir}")
    
    try:
        # 测试1: 不使用缓存时的ini写入
        logger.info("\n" + "-" * 60)
        logger.info("测试1: 不使用缓存时的ini文件生成")
        logger.info("-" * 60)
        
        test_mgr_no_cache = TestConfigManager(base_config_dir=temp_dir, use_cache=False)
        
        test_cases = [
            {"name": "TestCase1", "caseName": "TestCase1"},
            {"name": "TestCase2", "caseName": "TestCase2"},
        ]
        variables = {"var1": "value1", "var2": "value2"}
        
        # 模拟生成ini文件（不依赖cfg文件）
        ini_path = test_mgr_no_cache._generate_ini_file("test_config", test_cases, variables)
        
        logger.info(f"生成的ini文件路径: {ini_path}")
        
        # 验证文件是否存在
        if os.path.exists(ini_path):
            logger.info(f"✓ ini文件成功创建")
            
            # 读取并验证内容
            with open(ini_path, 'r', encoding='utf-8') as f:
                content = f.read()
            
            logger.info(f"\nini文件内容:")
            print(content)
            
            # 验证内容格式
            checks = [
                ("[TestCases]" in content, "包含[TestCases]节"),
                ("Case1=TestCase1" in content, "包含Case1"),
                ("Case2=TestCase2" in content, "包含Case2"),
                ("[Variables]" in content, "包含[Variables]节"),
                ("var1=value1" in content, "包含变量var1"),
                ("var2=value2" in content, "包含变量var2"),
            ]
            
            for check, desc in checks:
                status = "✓" if check else "✗"
                logger.info(f"  {status} {desc}")
        else:
            logger.info(f"✗ ini文件未创建")
        
        # 测试2: 使用缓存时的ini写入
        logger.info("\n" + "-" * 60)
        logger.info("测试2: 使用缓存时的ini文件生成")
        logger.info("-" * 60)
        
        cache_dir = os.path.join(temp_dir, "cache")
        cache_mgr = ConfigCacheManager(cache_dir=cache_dir)
        
        test_mgr_with_cache = TestConfigManager(base_config_dir=temp_dir, use_cache=True)
        test_mgr_with_cache._cache_manager = cache_mgr
        
        ini_path2 = test_mgr_with_cache._generate_ini_file("test_config2", test_cases, variables)
        
        logger.info(f"生成的ini文件路径: {ini_path2}")
        
        # 验证文件是否在缓存目录
        if os.path.exists(ini_path2):
            logger.info(f"✓ ini文件成功创建")
            
            # 验证是否在缓存目录
            if cache_dir in ini_path2:
                logger.info(f"✓ ini文件正确生成在缓存目录")
            else:
                logger.info(f"✗ ini文件不在缓存目录中")
        else:
            logger.info(f"✗ ini文件未创建")
        
        # 测试3: 检查文件编码
        logger.info("\n" + "-" * 60)
        logger.info("测试3: 文件编码检测")
        logger.info("-" * 60)
        
        # 测试中文内容
        test_cases_cn = [
            {"name": "测试用例1", "caseName": "测试用例1"},
            {"name": "测试用例2", "caseName": "测试用例2"},
        ]
        variables_cn = {"变量1": "值1", "变量2": "值2"}
        
        ini_path_cn = test_mgr_no_cache._generate_ini_file("test_config_cn", test_cases_cn, variables_cn)
        
        with open(ini_path_cn, 'r', encoding='utf-8') as f:
            content_cn = f.read()
        
        logger.info(f"中文内容测试:")
        print(content_cn)
        
        if "测试用例1" in content_cn and "变量1=值1" in content_cn:
            logger.info(f"✓ UTF-8编码正确，中文内容正常")
        else:
            logger.info(f"✗ 中文内容可能编码有问题")
        
        # 测试4: 检查目录创建
        logger.info("\n" + "-" * 60)
        logger.info("测试4: 目录自动创建检测")
        logger.info("-" * 60)
        
        new_dir = os.path.join(temp_dir, "new_subdir", "deep")
        test_mgr_new_dir = TestConfigManager(base_config_dir=new_dir, use_cache=False)
        
        ini_path_new = test_mgr_new_dir._generate_ini_file("test_new", test_cases, variables)
        
        if os.path.exists(ini_path_new):
            logger.info(f"✓ 自动创建多级目录成功")
        else:
            logger.info(f"✗ 目录创建失败")
        
        # 测试5: 检查文件名格式
        logger.info("\n" + "-" * 60)
        logger.info("测试5: 文件名格式检测")
        logger.info("-" * 60)
        
        import re

        filename = os.path.basename(ini_path)
        logger.info(f"生成的文件名: {filename}")
        
        # 检查文件名格式: config_name_YYYYMMDD_HHMMSS.ini
        pattern = r'^test_config_\d{8}_\d{6}\.ini$'
        if re.match(pattern, filename):
            logger.info(f"✓ 文件名格式正确")
        else:
            logger.info(f"✗ 文件名格式不正确")
        
        # 测试6: 检查特殊字符处理
        logger.info("\n" + "-" * 60)
        logger.info("测试6: 特殊字符处理检测")
        logger.info("-" * 60)
        
        test_cases_special = [
            {"name": "Case<With>Special:Chars", "caseName": "Case<With>Special:Chars"},
        ]
        variables_special = {"key=with=equals": "value", "key:with:colons": "value2"}
        
        ini_path_special = test_mgr_no_cache._generate_ini_file("test_special", test_cases_special, variables_special)
        
        with open(ini_path_special, 'r', encoding='utf-8') as f:
            content_special = f.read()
        
        logger.info(f"特殊字符内容:")
        print(content_special)
        logger.info(f"注意: ini文件中包含特殊字符可能需要额外处理")
        
        logger.info("\n" + "=" * 60)
        logger.info("检测完成")
        logger.info("=" * 60)
        
    finally:
        # 清理临时目录
        if os.path.exists(temp_dir):
            shutil.rmtree(temp_dir)
            logger.info(f"\n已清理测试目录: {temp_dir}")


def check_existing_code():
    """检查现有代码中的ini写入操作"""
    logger.info("\n" + "=" * 60)
    logger.info("检查现有代码中的ini写入操作")
    logger.info("=" * 60)
    
    issues = []
    warnings = []
    
    # 检查1: _generate_ini_file方法
    logger.info("\n检查 _generate_ini_file 方法:")
    
    # 读取源代码
    config_manager_path = os.path.join(os.path.dirname(__file__), "core", "config_manager.py")
    with open(config_manager_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # 检查编码设置
    if "encoding='utf-8'" in content or 'encoding="utf-8"' in content:
        logger.info("  ✓ 文件写入使用UTF-8编码")
    else:
        issues.append("ini文件写入未指定UTF-8编码")
        logger.info("  ✗ 文件写入未指定UTF-8编码")
    
    # 检查目录创建
    if "os.makedirs" in content and "exist_ok=True" in content:
        logger.info("  ✓ 使用os.makedirs创建目录（支持多级目录）")
    else:
        warnings.append("可能未正确处理多级目录创建")
        logger.info("  ⚠ 可能未正确处理多级目录创建")
    
    # 检查换行符
    if "'\\n'.join" in content or '"\\n".join' in content:
        logger.info("  ✓ 使用\\n作为换行符")
    else:
        warnings.append("换行符处理可能需要检查")
        logger.info("  ⚠ 换行符处理可能需要检查")
    
    # 检查2: 缓存管理器中的文件操作
    logger.info("\n检查 ConfigCacheManager 中的文件操作:")
    
    cache_manager_path = os.path.join(os.path.dirname(__file__), "core", "config_cache_manager.py")
    with open(cache_manager_path, 'r', encoding='utf-8') as f:
        cache_content = f.read()
    
    # 检查线程安全
    if "threading.RLock" in cache_content or "self._lock" in cache_content:
        logger.info("  ✓ 使用了线程锁保证并发安全")
    else:
        warnings.append("缓存管理器可能未考虑线程安全")
        logger.info("  ⚠ 缓存管理器可能未考虑线程安全")
    
    # 检查shutil.copy2使用
    if "shutil.copy2" in cache_content:
        logger.info("  ✓ 使用shutil.copy2复制文件（保留元数据）")
    else:
        logger.info("  ℹ 未使用shutil.copy2")
    
    # 总结
    logger.info("\n" + "=" * 60)
    logger.info("检查结果总结")
    logger.info("=" * 60)
    
    if issues:
        logger.info(f"\n发现 {len(issues)} 个问题:")
        for i, issue in enumerate(issues, 1):
            logger.info(f"  {i}. {issue}")
    else:
        logger.info("\n✓ 未发现严重问题")
    
    if warnings:
        logger.info(f"\n发现 {len(warnings)} 个警告:")
        for i, warning in enumerate(warnings, 1):
            logger.info(f"  {i}. {warning}")
    else:
        logger.info("\n✓ 无警告")
    
    return len(issues) == 0


if __name__ == "__main__":
    # 运行测试
    test_ini_write_operation()
    
    # 检查现有代码
    check_existing_code()
    
    logger.info("\n" + "=" * 60)
    logger.info("所有检测完成")
    logger.info("=" * 60)
