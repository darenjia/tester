"""
CANoe配置加载测试脚本 - 完全独立版

不依赖项目内部代码，不依赖CANoe外部程序
直接测试配置加载的等待逻辑

使用方法:
    python test_canoe_standalone.py --cfg="D:\\test.cfg"
    python test_canoe_standalone.py --cfg="D:\\test.cfg" --timeout=30
"""

import sys
import os
import time
import logging
import argparse

# 配置日志
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# 默认配置
DEFAULT_TIMEOUT = 30
DEFAULT_CONFIG_PATH = r"D:\TAMS\DTTC_CONFIG\S59\BCANFD\SMFT\FDCANC_E\TestProjectFile\COMTest.cfg"


class CANoeConfigLoader:
    """CANoe配置加载器 - 独立实现"""

    def __init__(self):
        self._app = None
        self._measurement = None
        self._system = None
        self._namespaces = None
        self.is_connected = False
        self.is_measurement_running = False
        self.last_error = None

    def connect(self, retry_count: int = 3, retry_interval: float = 2.0) -> bool:
        """连接CANoe"""
        for attempt in range(retry_count):
            try:
                logger.info(f"正在连接CANoe... (尝试 {attempt + 1}/{retry_count})")

                import win32com.client
                self._app = win32com.client.Dispatch("CANoe.Application")
                self._measurement = self._app.Measurement
                self._system = self._app.System
                self._namespaces = self._system.Namespaces

                self.is_connected = True
                self.last_error = None

                # 获取版本
                try:
                    version = self._app.Version
                    logger.info(f"CANoe连接成功，版本: {version}")
                except:
                    logger.info("CANoe连接成功")

                return True

            except Exception as e:
                self.last_error = str(e)
                logger.warning(f"连接失败: {e}")
                if attempt < retry_count - 1:
                    time.sleep(retry_interval)

        logger.error(f"连接失败，已重试{retry_count}次")
        return False

    def disconnect(self):
        """断开连接"""
        try:
            if self._measurement and self._measurement.Running:
                self._measurement.Stop()
                time.sleep(1)

            self._app = None
            self._measurement = None
            self._system = None
            self._namespaces = None
            self.is_connected = False
            logger.info("已断开CANoe连接")

        except Exception as e:
            logger.warning(f"断开连接异常: {e}")

    def open_config_v1_no_wait(self, config_path: str, timeout: int = 30) -> bool:
        """
        方式1: Open后直接返回（原有问题方式）

        Args:
            config_path: 配置文件路径
            timeout: 未使用（保持接口一致）
        """
        logger.info(">>> 方式1: Open后直接返回")
        self.last_error = None

        try:
            # 关闭当前配置
            try:
                self._app.CloseConfiguration()
                time.sleep(0.5)
            except:
                pass

            # 打开配置
            start_time = time.time()
            self._app.Open(config_path)
            elapsed = time.time() - start_time

            logger.info(f"Open()完成，耗时: {elapsed:.2f}秒")
            return True

        except Exception as e:
            self.last_error = str(e)
            logger.error(f"打开配置失败: {e}")
            return False

    def open_config_v2_wait_namespaces(self, config_path: str, timeout: int = 30) -> bool:
        """
        方式2: Open后等待命名空间可访问
        """
        logger.info(">>> 方式2: Open后等待命名空间可访问")
        self.last_error = None

        try:
            # 关闭当前配置
            try:
                self._app.CloseConfiguration()
                time.sleep(0.5)
            except:
                pass

            # 打开配置
            self._app.Open(config_path)
            logger.info("Open()已调用，开始等待验证...")

            # 等待命名空间可访问
            start_time = time.time()
            while time.time() - start_time < timeout:
                try:
                    namespaces = self._app.System.Namespaces
                    count = namespaces.Count
                    self._namespaces = namespaces
                    elapsed = time.time() - start_time
                    logger.info(f"配置加载成功! 命名空间数量={count}, 等待时间={elapsed:.2f}秒")
                    return True
                except Exception as check_err:
                    logger.debug(f"等待中... ({check_err})")
                    time.sleep(0.5)

            self.last_error = f"配置加载超时（{timeout}秒）"
            logger.error(self.last_error)
            return False

        except Exception as e:
            self.last_error = str(e)
            logger.error(f"打开配置失败: {e}")
            return False

    def open_config_v3_wait_configuration(self, config_path: str, timeout: int = 30) -> bool:
        """
        方式3: Open后等待Configuration对象可访问
        """
        logger.info(">>> 方式3: Open后等待Configuration对象可访问")
        self.last_error = None

        try:
            # 关闭当前配置
            try:
                self._app.CloseConfiguration()
                time.sleep(0.5)
            except:
                pass

            # 打开配置
            self._app.Open(config_path)
            logger.info("Open()已调用，开始等待验证...")

            # 等待Configuration可访问
            start_time = time.time()
            while time.time() - start_time < timeout:
                try:
                    config = self._app.Configuration
                    name = config.Name
                    elapsed = time.time() - start_time
                    logger.info(f"配置加载成功! 配置名称={name}, 等待时间={elapsed:.2f}秒")
                    return True
                except Exception as check_err:
                    logger.debug(f"等待中... ({check_err})")
                    time.sleep(0.5)

            self.last_error = f"配置加载超时（{timeout}秒）"
            logger.error(self.last_error)
            return False

        except Exception as e:
            self.last_error = str(e)
            logger.error(f"打开配置失败: {e}")
            return False

    def open_config_v4_fixed_wait(self, config_path: str, timeout: int = 30) -> bool:
        """
        方式4: Open后固定等待时间

        Args:
            config_path: 配置文件路径
            timeout: 作为等待时间使用
        """
        wait_time = min(timeout, 5.0)  # 最多等待5秒
        logger.info(f">>> 方式4: Open后固定等待{wait_time}秒")
        self.last_error = None

        try:
            # 关闭当前配置
            try:
                self._app.CloseConfiguration()
                time.sleep(0.5)
            except:
                pass

            # 打开配置
            self._app.Open(config_path)
            logger.info(f"Open()已调用，等待{wait_time}秒...")

            time.sleep(wait_time)

            logger.info("等待完成")
            return True

        except Exception as e:
            self.last_error = str(e)
            logger.error(f"打开配置失败: {e}")
            return False

    def open_config_v5_combined(self, config_path: str, timeout: int = 30) -> bool:
        """
        方式5: 组合验证（命名空间 + Configuration）
        """
        logger.info(">>> 方式5: 组合验证（命名空间 + Configuration）")
        self.last_error = None

        try:
            # 关闭当前配置
            try:
                self._app.CloseConfiguration()
                time.sleep(0.5)
            except:
                pass

            # 打开配置
            self._app.Open(config_path)
            logger.info("Open()已调用，开始组合验证...")

            start_time = time.time()
            while time.time() - start_time < timeout:
                try:
                    # 尝试访问命名空间
                    try:
                        namespaces = self._app.System.Namespaces
                        if namespaces.Count >= 0:
                            self._namespaces = namespaces
                            elapsed = time.time() - start_time
                            logger.info(f"命名空间验证通过 (数量={namespaces.Count}, {elapsed:.2f}秒)")
                            return True
                    except:
                        pass

                    # 尝试访问Configuration
                    try:
                        config = self._app.Configuration
                        _ = config.Name
                        elapsed = time.time() - start_time
                        logger.info(f"Configuration验证通过 ({elapsed:.2f}秒)")
                        return True
                    except:
                        pass

                except Exception as check_err:
                    logger.debug(f"验证中... {check_err}")

                time.sleep(0.5)

            self.last_error = f"配置加载超时（{timeout}秒）"
            logger.error(self.last_error)
            return False

        except Exception as e:
            self.last_error = str(e)
            logger.error(f"打开配置失败: {e}")
            return False

    def start_measurement(self, timeout: int = 30) -> bool:
        """启动测量"""
        try:
            logger.info("正在启动测量...")

            if self._measurement.Running:
                logger.info("测量已在运行")
                return True

            self._measurement.Start()

            start_time = time.time()
            while time.time() - start_time < timeout:
                if self._measurement.Running:
                    self.is_measurement_running = True
                    logger.info("测量启动成功")
                    return True
                time.sleep(0.5)

            logger.error("测量启动超时")
            return False

        except Exception as e:
            self.last_error = str(e)
            logger.error(f"启动测量失败: {e}")
            return False

    def stop_measurement(self):
        """停止测量"""
        try:
            if self._measurement.Running:
                logger.info("正在停止测量...")
                self._measurement.Stop()
                time.sleep(1)

            self.is_measurement_running = False
            logger.info("测量已停止")

        except Exception as e:
            logger.warning(f"停止测量异常: {e}")


def run_test(loader: CANoeConfigLoader, config_path: str, timeout: int, method_name: str, method_func):
    """运行单个测试方法"""
    logger.info("\n" + "=" * 60)

    # 断开重连
    loader.disconnect()
    time.sleep(1)

    if not loader.connect():
        logger.error("连接失败，跳过测试")
        return None

    # 执行配置加载
    result = method_func(config_path, timeout)

    if result is None:
        result = False

    # 验证：尝试启动测量
    logger.info("--- 验证: 尝试启动测量 ---")
    measure_result = False
    try:
        measure_result = loader.start_measurement(timeout=10)
        logger.info(f"测量启动结果: {'成功' if measure_result else '失败'}")
    except Exception as e:
        logger.error(f"测量启动异常: {e}")

    loader.stop_measurement()

    return {
        'config_result': result,
        'measure_result': measure_result,
        'overall': result and measure_result
    }


def main():
    parser = argparse.ArgumentParser(description='CANoe配置加载测试 - 独立版')
    parser.add_argument('--cfg', type=str, default=DEFAULT_CONFIG_PATH, help='配置文件路径')
    parser.add_argument('--timeout', type=int, default=DEFAULT_TIMEOUT, help='超时时间（秒）')
    parser.add_argument('--method', type=str, default='all',
                        choices=['all', '1', '2', '3', '4', '5'],
                        help='测试方法: 1=不等待, 2=等命名空间, 3=等Configuration, 4=固定等待, 5=组合验证')

    args = parser.parse_args()

    logger.info("=" * 60)
    logger.info("CANoe配置加载测试 - 独立版")
    logger.info("=" * 60)
    logger.info(f"配置文件: {args.cfg}")
    logger.info(f"超时时间: {args.timeout}秒")
    logger.info(f"测试方法: {args.method}")
    logger.info("=" * 60)

    # 检查配置文件
    if not os.path.exists(args.cfg):
        logger.error(f"配置文件不存在: {args.cfg}")
        logger.info("请使用 --cfg 参数指定有效的cfg文件路径")
        return

    # 检查win32com
    try:
        import win32com.client
        logger.info("win32com.client 已就绪")
    except ImportError:
        logger.error("请安装pywin32: pip install pywin32")
        return

    loader = CANoeConfigLoader()

    # 定义测试方法
    methods = {
        '1': ('方式1: 不等待', loader.open_config_v1_no_wait),
        '2': ('方式2: 等待命名空间', loader.open_config_v2_wait_namespaces),
        '3': ('方式3: 等待Configuration', loader.open_config_v3_wait_configuration),
        '4': ('方式4: 固定等待5秒', loader.open_config_v4_fixed_wait),
        '5': ('方式5: 组合验证', loader.open_config_v5_combined),
    }

    results = {}

    try:
        if args.method == 'all':
            # 测试所有方法
            for method_id, (method_desc, method_func) in methods.items():
                result = run_test(loader, args.cfg, args.timeout, method_desc, method_func)
                results[method_id] = result
        else:
            # 测试指定方法
            method_id = args.method
            method_desc, method_func = methods[method_id]
            result = run_test(loader, args.cfg, args.timeout, method_desc, method_func)
            results[method_id] = result

    finally:
        loader.disconnect()

    # 打印结果汇总
    logger.info("\n" + "=" * 60)
    logger.info("测试结果汇总:")
    logger.info("-" * 60)
    logger.info(f"{'方法':<25} {'配置加载':<10} {'测量启动':<10} {'整体结果':<10}")
    logger.info("-" * 60)

    for method_id, result in results.items():
        if result:
            method_name = methods[method_id][0]
            config_status = "成功" if result['config_result'] else "失败"
            measure_status = "成功" if result['measure_result'] else "失败"
            overall_status = "通过" if result['overall'] else "未通过"
            logger.info(f"{method_name:<25} {config_status:<10} {measure_status:<10} {overall_status:<10}")
        else:
            logger.info(f"{methods[method_id][0]:<25} {'跳过':<10} {'跳过':<10} {'跳过':<10}")

    logger.info("=" * 60)

    # 推荐
    logger.info("\n推荐:")
    passed_methods = [mid for mid, r in results.items() if r and r['overall']]
    if passed_methods:
        logger.info(f"  可使用的方法: {', '.join([methods[mid][0] for mid in passed_methods])}")
    else:
        logger.info("  所有方法都未通过，请检查CANoe和配置文件")


if __name__ == "__main__":
    main()