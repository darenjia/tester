"""
测试CANoe规范的ini文件生成
"""
import os
import sys
import tempfile
import shutil

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from core.config_manager import TestConfigManager

from utils.logger import get_logger

logger = get_logger("test_canoe_ini")



def test_canoe_ini_generation():
    """测试CANoe规范的ini文件生成"""
    logger.info("=" * 60)
    logger.info("测试CANoe规范的ini文件生成")
    logger.info("=" * 60)
    
    # 创建临时目录
    temp_dir = tempfile.mkdtemp(prefix="canoe_ini_test_")
    logger.info(f"\n测试目录: {temp_dir}")
    
    try:
        # 创建TestConfigManager
        test_mgr = TestConfigManager(base_config_dir=temp_dir, use_cache=False)
        
        # 模拟测试用例（符合TDM2.0规范）
        test_cases = [
            {
                "caseNo": "TG1_TC04_SC01",
                "caseName": "测试用例1",
                "moduleLevel1": "Module1",
                "moduleLevel2": "SubModule1"
            },
            {
                "caseNo": "TG1_TC05_SC01",
                "caseName": "测试用例2",
                "moduleLevel1": "Module1",
                "moduleLevel2": "SubModule2"
            },
            {
                "caseNo": "TG2_TC01_SC01",
                "caseName": "测试用例3",
                "moduleLevel1": "Module2",
                "moduleLevel2": "SubModule1"
            }
        ]
        
        # 模拟参数配置（参考ParaInfo.ini）
        variables = {
            "ECUName": "MTCU1",
            "Terminal": "1",
            "Cfg_ProjectType": "2",
            "Cfg_PowerControl": "0",
            "Cfg_VH6501": "0",
            "Cfg_NodeType": "1",
            "Cfg_TestID": "0x8CF181EF",
            "Cfg_JudgeVoltmin": "8",
            "Cfg_JudgeVoltmax": "18",
            "Cfg_tRecover_LowVolt": "200",
            "Cfg_tRecover_HighVolt": "200",
        }
        
        # 生成ini文件
        logger.info("\n生成ini文件...")
        ini_files = test_mgr._generate_ini_files("test_config", test_cases, variables)
        
        logger.info(f"SelectInfo.ini: {ini_files['select_info']}")
        logger.info(f"ParaInfo.ini: {ini_files['para_info']}")
        
        # 验证SelectInfo.ini
        logger.info("\n" + "-" * 60)
        logger.info("验证 SelectInfo.ini")
        logger.info("-" * 60)
        
        with open(ini_files['select_info'], 'r', encoding='utf-8') as f:
            select_content = f.read()
        
        logger.info("文件内容:")
        print(select_content)
        
        # 验证格式
        checks = [
            ("[CFG_PARA]" in select_content, "包含[CFG_PARA]节"),
            ("TG1_TC04_SC01=1" in select_content, "包含用例TG1_TC04_SC01"),
            ("TG1_TC05_SC01=1" in select_content, "包含用例TG1_TC05_SC01"),
            ("TG2_TC01_SC01=1" in select_content, "包含用例TG2_TC01_SC01"),
        ]
        
        logger.info("\n格式检查:")
        for check, desc in checks:
            status = "✓" if check else "✗"
            logger.info(f"  {status} {desc}")
        
        # 验证ParaInfo.ini
        logger.info("\n" + "-" * 60)
        logger.info("验证 ParaInfo.ini")
        logger.info("-" * 60)
        
        with open(ini_files['para_info'], 'r', encoding='utf-8') as f:
            para_content = f.read()
        
        logger.info("文件内容:")
        print(para_content)
        
        # 验证格式
        checks = [
            ("[CFG_PARA]" in para_content, "包含[CFG_PARA]节"),
            ("ECUName=MTCU1" in para_content, "包含ECUName参数"),
            ("Terminal=1" in para_content, "包含Terminal参数"),
            ("Cfg_TestID=0x8CF181EF" in para_content, "包含Cfg_TestID参数"),
            ("Cfg_JudgeVoltmin=8" in para_content, "包含Cfg_JudgeVoltmin参数"),
        ]
        
        logger.info("\n格式检查:")
        for check, desc in checks:
            status = "✓" if check else "✗"
            logger.info(f"  {status} {desc}")
        
        # 验证与你的示例文件对比
        logger.info("\n" + "-" * 60)
        logger.info("与示例文件对比")
        logger.info("-" * 60)
        
        # 读取你的示例文件
        your_select_path = r"d:\Deng\can_test\SelectInfo.ini"
        your_para_path = r"d:\Deng\can_test\ParaInfo.ini"
        
        if os.path.exists(your_select_path):
            with open(your_select_path, 'r', encoding='utf-8') as f:
                your_select = f.read()
            logger.info("\n你的SelectInfo.ini示例:")
            print(your_select)
        
        if os.path.exists(your_para_path):
            with open(your_para_path, 'r', encoding='utf-8') as f:
                your_para = f.read()
            logger.info("\n你的ParaInfo.ini示例（前10行）:")
            logger.info('\n'.join(your_para.split('\n')[:10]))
        
        # 结构对比
        logger.info("\n结构对比:")
        logger.info("  生成的SelectInfo.ini结构:")
        logger.info(f"    - 节: {'[CFG_PARA]' in select_content}")
        logger.info(f"    - 键值对格式: key=value")
        
        logger.info("  生成的ParaInfo.ini结构:")
        logger.info(f"    - 节: {'[CFG_PARA]' in para_content}")
        logger.info(f"    - 键值对格式: key=value")
        
        logger.info("\n" + "=" * 60)
        logger.info("测试完成")
        logger.info("=" * 60)
        
    finally:
        # 清理
        if os.path.exists(temp_dir):
            shutil.rmtree(temp_dir)
            logger.info(f"\n已清理测试目录: {temp_dir}")


if __name__ == "__main__":
    test_canoe_ini_generation()
