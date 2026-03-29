"""
COM环境重置修复验证测试

验证场景：
1. 在线程池中连接CANoe
2. 模拟CANoe进程被关闭的情况
3. 再次连接，验证第一次就能成功

运行方式：
python test_com_reset.py
"""

import sys
import time
import logging
from concurrent.futures import ThreadPoolExecutor

# 配置日志
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger("COM_TEST")

# 检查运行环境
import platform
if platform.system() != 'Windows':
    logger.warning("此测试需要在Windows系统上运行")
    logger.info("演示模式：将显示代码逻辑而不实际执行COM操作")
    DEMO_MODE = True
else:
    DEMO_MODE = False


def test_simple_adapter():
    """
    测试简单版本适配器（canoe_adapter.py）
    这是AdapterFactory默认加载的适配器
    """
    if DEMO_MODE:
        logger.info("=== 演示模式：简单适配器测试 ===")
        return True

    from core.adapters.canoe_adapter import CANoeAdapter

    logger.info("=== 测试简单版本适配器 (canoe_adapter.py) ===")

    with ThreadPoolExecutor(max_workers=1) as executor:

        def first_connection():
            logger.info("第一次连接...")
            adapter = CANoeAdapter()
            try:
                result = adapter.connect()
                logger.info(f"第一次连接结果: {result}")
                return result, adapter
            except Exception as e:
                logger.error(f"第一次连接异常: {e}")
                return False, adapter

        def second_connection():
            logger.info("\n第二次连接...")
            adapter = CANoeAdapter()
            try:
                result = adapter.connect()
                logger.info(f"第二次连接结果: {result}")
                return result
            except Exception as e:
                logger.error(f"第二次连接异常: {e}")
                return False

        # 第一次连接
        future1 = executor.submit(first_connection)
        result1, adapter1 = future1.result(timeout=60)

        if not result1:
            logger.warning("第一次连接失败，请确保CANoe已安装并运行")
            return False

        logger.info(f"CANoe版本: {adapter1.canoe_version if hasattr(adapter1, 'canoe_version') else 'N/A'}")

        logger.info("\n" + "="*50)
        logger.info("请手动关闭CANoe进程，然后按Enter继续测试...")
        logger.info("="*50)
        input("按Enter继续...")

        # 第二次连接
        future2 = executor.submit(second_connection)
        result2 = future2.result(timeout=60)

        if result2:
            logger.info("\n✓ 测试通过！第二次连接第一次尝试就成功了")
            return True
        else:
            logger.error("\n✗ 测试失败！第二次连接失败")
            return False


def test_com_wrapper():
    """
    测试完善版本适配器（canoe/adapter.py 使用 com_wrapper.py）
    """
    if DEMO_MODE:
        logger.info("=== 演示模式：COM包装器测试 ===")
        return True

    from core.adapters.canoe.com_wrapper import CANoeCOMWrapper

    logger.info("=== 测试完善版本适配器 (canoe/com_wrapper.py) ===")

    with ThreadPoolExecutor(max_workers=1) as executor:

        def first_connection():
            logger.info("第一次连接...")
            wrapper = CANoeCOMWrapper()
            try:
                result = wrapper.connect(retry_count=1)
                logger.info(f"第一次连接结果: {result}")
                if result:
                    logger.info(f"CANoe版本: {wrapper.version}")
                return result, wrapper
            except Exception as e:
                logger.error(f"第一次连接异常: {e}")
                return False, wrapper

        def second_connection():
            logger.info("\n第二次连接...")
            wrapper = CANoeCOMWrapper()
            try:
                result = wrapper.connect(retry_count=3)
                logger.info(f"第二次连接结果: {result}")
                if result:
                    logger.info(f"CANoe版本: {wrapper.version}")
                return result
            except Exception as e:
                logger.error(f"第二次连接异常: {e}")
                return False

        # 第一次连接
        future1 = executor.submit(first_connection)
        result1, wrapper1 = future1.result(timeout=60)

        if not result1:
            logger.warning("第一次连接失败，请确保CANoe已安装")
            return False

        logger.info("\n" + "="*50)
        logger.info("请手动关闭CANoe进程，然后按Enter继续测试...")
        logger.info("="*50)
        input("按Enter继续...")

        # 第二次连接
        future2 = executor.submit(second_connection)
        result2 = future2.result(timeout=60)

        if result2:
            logger.info("\n✓ 测试通过！第二次连接第一次尝试就成功了")
            return True
        else:
            logger.error("\n✗ 测试失败！第二次连接失败")
            return False


def test_load_config_after_canoe_restart():
    """
    测试场景：加载配置文件在CANoe重启后是否正常

    这是用户描述的具体问题场景
    """
    if DEMO_MODE:
        logger.info("=== 演示模式：加载配置测试 ===")
        return True

    from core.adapters.canoe_adapter import CANoeAdapter

    test_cfg = r"D:\TAMS\DTTC_CONFIG\S59\BCANFD\SMFT\FDCANC_E\TestProjectFile\COMTest.cfg"

    logger.info("=== 测试加载配置文件 ===")

    with ThreadPoolExecutor(max_workers=1) as executor:

        def first_load():
            logger.info("第一次加载配置...")
            adapter = CANoeAdapter()
            try:
                if adapter.connect():
                    result = adapter.load_configuration(test_cfg)
                    logger.info(f"第一次加载结果: {result}")
                    adapter.disconnect()
                    return True
            except Exception as e:
                logger.error(f"第一次加载异常: {e}")
            return False

        def second_load():
            logger.info("第二次加载配置...")
            adapter = CANoeAdapter()
            try:
                if adapter.connect():
                    result = adapter.load_configuration(test_cfg)
                    logger.info(f"第二次加载结果: {result}")
                    adapter.disconnect()
                    return True
            except Exception as e:
                logger.error(f"第二次加载异常: {e}")
            return False

        # 第一次
        future1 = executor.submit(first_load)
        result1 = future1.result(timeout=120)

        if not result1:
            logger.warning("第一次加载失败")
            return False

        logger.info("\n请关闭CANoe进程，然后按Enter继续...")
        input("按Enter继续...")

        # 第二次
        future2 = executor.submit(second_load)
        result2 = future2.result(timeout=120)

        if result2:
            logger.info("✓ 第二次加载成功！")
            return True
        else:
            logger.error("✗ 第二次加载失败！")
            return False


if __name__ == "__main__":
    print("="*60)
    print("COM环境重置修复验证测试")
    print("="*60)
    print("\n选择测试:")
    print("1. 测试简单版本适配器 (canoe_adapter.py)")
    print("2. 测试完善版本适配器 (canoe/com_wrapper.py)")
    print("3. 测试加载配置文件（用户场景）")
    print("4. 运行所有测试")

    choice = input("\n请选择 (1/2/3/4): ").strip()

    if choice == "1":
        test_simple_adapter()
    elif choice == "2":
        test_com_wrapper()
    elif choice == "3":
        test_load_config_after_canoe_restart()
    else:
        logger.info("\n运行所有测试...")
        test_simple_adapter()
        test_com_wrapper()
        test_load_config_after_canoe_restart()