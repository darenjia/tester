"""
CANoe配置加载测试脚本 - 项目内部测试

直接测试项目中的CANoeCOMWrapper类，不依赖外部程序

使用方法:
    python test_canoe_config_internal.py --mock          # 模拟模式（不需要CANoe）
    python test_canoe_config_internal.py --real          # 真实模式（需要CANoe）
    python test_canoe_config_internal.py --real --cfg="D:\\test.cfg"  # 指定cfg文件
"""

import sys
import os
import time
import logging
import argparse

# 添加项目路径
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

# 配置日志
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(levelname)s - %(name)s - %(message)s'
)
logger = logging.getLogger(__name__)

# 默认配置文件路径
DEFAULT_CONFIG_PATH = r"D:\TAMS\DTTC_CONFIG\S59\BCANFD\SMFT\FDCANC_E\TestProjectFile\COMTest.cfg"


class MockCANoeApp:
    """模拟CANoe Application对象"""

    def __init__(self):
        self._config_loaded = False
        self._config_path = None
        self._load_time = 0
        self._measurement_running = False

    def Open(self, config_path):
        """模拟打开配置"""
        logger.info(f"[Mock] Open({config_path})")
        self._config_path = config_path
        self._load_time = time.time()
        self._config_loaded = False  # 异步加载

    def CloseConfiguration(self):
        """模拟关闭配置"""
        logger.info("[Mock] CloseConfiguration()")
        self._config_loaded = False
        self._config_path = None

    @property
    def System(self):
        """模拟System对象"""
        if not self._config_loaded and time.time() - self._load_time < 2:
            raise Exception("配置未加载完成")
        return MockSystem()

    @property
    def Configuration(self):
        """模拟Configuration对象"""
        if not self._config_loaded and time.time() - self._load_time < 2:
            raise Exception("配置未加载完成")
        return MockConfiguration(self._config_path)

    @property
    def Measurement(self):
        """模拟Measurement对象"""
        return MockMeasurement(self)

    @property
    def Version(self):
        return "17.3.91"

    def Quit(self):
        pass

    # 模拟异步加载（2秒后完成）
    def _check_loaded(self):
        if self._config_path and time.time() - self._load_time >= 2:
            self._config_loaded = True
        return self._config_loaded


class MockSystem:
    """模拟System对象"""

    @property
    def Namespaces(self):
        return MockNamespaces()


class MockNamespaces:
    """模拟Namespaces集合"""

    @property
    def Count(self):
        return 5

    def Item(self, name):
        return MockNamespace(name)

    def __getitem__(self, name):
        return self.Item(name)


class MockNamespace:
    """模拟Namespace对象"""

    def __init__(self, name):
        self.name = name

    @property
    def Variables(self):
        return MockVariables()


class MockVariables:
    """模拟Variables集合"""

    def Item(self, name):
        return MockVariable(name, 0)

    def __getitem__(self, name):
        return self.Item(name)


class MockVariable:
    """模拟Variable对象"""

    def __init__(self, name, value):
        self.name = name
        self._value = value

    @property
    def Value(self):
        return self._value

    @Value.setter
    def Value(self, val):
        self._value = val


class MockConfiguration:
    """模拟Configuration对象"""

    def __init__(self, config_path):
        self._config_path = config_path

    @property
    def Name(self):
        return os.path.basename(self._config_path) if self._config_path else ""


class MockMeasurement:
    """模拟Measurement对象"""

    def __init__(self, app):
        self._app = app
        self._running = False

    @property
    def Running(self):
        return self._running

    def Start(self):
        logger.info("[Mock] Measurement.Start()")
        self._running = True

    def Stop(self):
        logger.info("[Mock] Measurement.Stop()")
        self._running = False


def test_with_mock():
    """使用模拟对象测试"""
    logger.info("=" * 60)
    logger.info("模式: 模拟测试（不需要CANoe）")
    logger.info("=" * 60)

    # 创建模拟对象
    mock_app = MockCANoeApp()

    # 测试配置加载（模拟异步加载）
    logger.info("\n>>> 测试: Open后直接访问（会失败）")
    mock_app.Open("test.cfg")
    try:
        ns = mock_app.System.Namespaces  # 此时还没加载完
        logger.info(f"命名空间数量: {ns.Count}")
    except Exception as e:
        logger.info(f"预期中的错误: {e}")

    logger.info("\n>>> 测试: 等待2秒后再访问")
    time.sleep(2.5)
    try:
        ns = mock_app.System.Namespaces
        logger.info(f"命名空间数量: {ns.Count} ✓")
    except Exception as e:
        logger.error(f"错误: {e}")

    logger.info("\n模拟测试完成")


def test_with_real_wrapper(config_path: str, timeout: int = 30):
    """使用真实的CANoeCOMWrapper测试"""
    logger.info("=" * 60)
    logger.info("模式: 真实测试（需要CANoe运行）")
    logger.info(f"配置文件: {config_path}")
    logger.info(f"超时时间: {timeout}秒")
    logger.info("=" * 60)

    # 检查配置文件
    if not os.path.exists(config_path):
        logger.error(f"配置文件不存在: {config_path}")
        return False

    # 导入真实的包装器
    try:
        from core.adapters.canoe.com_wrapper import CANoeCOMWrapper
        logger.info("成功导入 CANoeCOMWrapper")
    except ImportError as e:
        logger.error(f"导入失败: {e}")
        return False

    # 创建包装器实例
    wrapper = CANoeCOMWrapper(logger)

    try:
        # 连接CANoe
        logger.info("\n>>> 步骤1: 连接CANoe")
        if not wrapper.connect():
            logger.error("连接失败")
            return False
        logger.info(f"连接成功，版本: {wrapper.version}")

        # 测试配置加载
        logger.info("\n>>> 步骤2: 打开配置文件")
        start_time = time.time()

        result = wrapper.open_configuration(config_path, timeout)

        elapsed = time.time() - start_time
        logger.info(f"配置加载结果: {'成功' if result else '失败'}, 耗时: {elapsed:.2f}秒")

        if result:
            # 验证配置是否真的加载成功
            logger.info("\n>>> 步骤3: 验证配置状态")

            # 检查命名空间
            try:
                ns = wrapper._namespaces
                if ns:
                    logger.info(f"命名空间可访问: {ns.Count} 个命名空间")
            except Exception as e:
                logger.warning(f"命名空间访问失败: {e}")

            # 检查测量对象
            logger.info("\n>>> 步骤4: 尝试启动测量")
            try:
                measure_result = wrapper.start_measurement(timeout=10)
                logger.info(f"测量启动: {'成功' if measure_result else '失败'}")

                if measure_result:
                    time.sleep(1)
                    wrapper.stop_measurement()
                    logger.info("测量已停止")

            except Exception as e:
                logger.error(f"测量操作失败: {e}")

        return result

    except Exception as e:
        logger.error(f"测试异常: {e}", exc_info=True)
        return False

    finally:
        logger.info("\n>>> 清理资源")
        try:
            wrapper.disconnect()
        except:
            pass


def test_wrapper_methods(config_path: str):
    """测试CANoeCOMWrapper的各个方法"""
    logger.info("=" * 60)
    logger.info("测试 CANoeCOMWrapper 各方法")
    logger.info("=" * 60)

    try:
        from core.adapters.canoe.com_wrapper import CANoeCOMWrapper
    except ImportError as e:
        logger.error(f"导入失败: {e}")
        return

    wrapper = CANoeCOMWrapper(logger)

    methods = [
        ('connect', lambda: wrapper.connect()),
        ('open_configuration', lambda: wrapper.open_configuration(config_path, 30)),
        ('start_measurement', lambda: wrapper.start_measurement(10)),
        ('stop_measurement', lambda: wrapper.stop_measurement()),
        ('disconnect', lambda: wrapper.disconnect()),
    ]

    results = {}

    for method_name, method_func in methods:
        logger.info(f"\n>>> 测试方法: {method_name}")
        try:
            start = time.time()
            result = method_func()
            elapsed = time.time() - start
            results[method_name] = {
                'success': True if result else False,
                'time': elapsed,
                'error': None
            }
            logger.info(f"结果: {result}, 耗时: {elapsed:.2f}秒")
        except Exception as e:
            results[method_name] = {
                'success': False,
                'time': 0,
                'error': str(e)
            }
            logger.error(f"异常: {e}")

    # 打印结果汇总
    logger.info("\n" + "=" * 60)
    logger.info("测试结果汇总:")
    logger.info("-" * 40)
    for method, info in results.items():
        status = "✓" if info['success'] else "✗"
        error = f" ({info['error']})" if info['error'] else ""
        logger.info(f"  {status} {method}: {info['time']:.2f}秒{error}")
    logger.info("=" * 60)


def main():
    parser = argparse.ArgumentParser(description='CANoe配置加载测试')
    parser.add_argument('--mock', action='store_true', help='使用模拟模式（不需要CANoe）')
    parser.add_argument('--real', action='store_true', help='使用真实模式（需要CANoe）')
    parser.add_argument('--cfg', type=str, default=DEFAULT_CONFIG_PATH, help='配置文件路径')
    parser.add_argument('--timeout', type=int, default=30, help='超时时间（秒）')
    parser.add_argument('--methods', action='store_true', help='测试所有方法')

    args = parser.parse_args()

    if args.mock:
        test_with_mock()
    elif args.real:
        if args.methods:
            test_wrapper_methods(args.cfg)
        else:
            test_with_real_wrapper(args.cfg, args.timeout)
    else:
        # 默认显示帮助
        parser.print_help()
        logger.info("\n示例:")
        logger.info("  python test_canoe_config_internal.py --mock")
        logger.info("  python test_canoe_config_internal.py --real")
        logger.info("  python test_canoe_config_internal.py --real --cfg=\"D:\\test.cfg\"")
        logger.info("  python test_canoe_config_internal.py --real --methods")


if __name__ == "__main__":
    main()