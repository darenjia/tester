"""
CANoe配置文件加载测试脚本

独立测试脚本，不依赖其他类，用于验证CANoe配置加载是否正常

使用方法:
    python test_canoe_config_load.py

修改 CONFIG_PATH 为你的cfg文件路径
"""

import time
import logging

# 配置日志
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# ==================== 配置区域 ====================
# 修改为你的cfg文件路径
CONFIG_PATH = r"D:\TAMS\DTTC_CONFIG\S59\BCANFD\SMFT\FDCANC_E\TestProjectFile\COMTest.cfg"

# 超时时间（秒）
OPEN_TIMEOUT = 30
# ==================================================


class CANoeConfigTest:
    """CANoe配置加载测试类"""

    def __init__(self):
        self._app = None
        self._measurement = None
        self._system = None
        self._namespaces = None
        self.is_connected = False
        self.is_measurement_running = False

    def connect(self) -> bool:
        """连接CANoe"""
        try:
            logger.info("正在连接CANoe...")

            import win32com.client
            self._app = win32com.client.Dispatch("CANoe.Application")
            self._measurement = self._app.Measurement
            self._system = self._app.System
            self._namespaces = self._system.Namespaces

            self.is_connected = True
            logger.info("CANoe连接成功")
            return True

        except Exception as e:
            logger.error(f"连接CANoe失败: {e}")
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
            logger.info("CANoe已断开")

        except Exception as e:
            logger.warning(f"断开连接异常: {e}")

    def open_configuration_v1(self, config_path: str, timeout: int = 30) -> bool:
        """
        方式1: 直接Open后返回（原有方式，可能有问题）
        """
        logger.info(">>> 方式1: 直接Open后返回")

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

            logger.info(f"Open()调用完成，耗时: {elapsed:.2f}秒")
            logger.info("配置文件打开成功（未验证）")
            return True

        except Exception as e:
            logger.error(f"打开配置失败: {e}")
            return False

    def open_configuration_v2(self, config_path: str, timeout: int = 30) -> bool:
        """
        方式2: Open后等待，验证命名空间可访问
        """
        logger.info(">>> 方式2: Open后等待命名空间可访问")

        try:
            # 关闭当前配置
            try:
                self._app.CloseConfiguration()
                time.sleep(0.5)
            except:
                pass

            # 打开配置
            self._app.Open(config_path)
            logger.info("Open()调用完成，开始等待验证...")

            # 等待配置加载完成
            start_time = time.time()
            while time.time() - start_time < timeout:
                try:
                    # 尝试访问命名空间
                    namespaces = self._app.System.Namespaces
                    count = namespaces.Count
                    logger.info(f"配置加载成功! 命名空间数量: {count}")
                    return True
                except Exception as check_err:
                    logger.debug(f"验证中... {check_err}")
                    time.sleep(0.5)

            logger.error(f"配置加载超时（{timeout}秒）")
            return False

        except Exception as e:
            logger.error(f"打开配置失败: {e}")
            return False

    def open_configuration_v3(self, config_path: str, timeout: int = 30) -> bool:
        """
        方式3: Open后等待，验证Configuration对象可访问
        """
        logger.info(">>> 方式3: Open后等待Configuration可访问")

        try:
            # 关闭当前配置
            try:
                self._app.CloseConfiguration()
                time.sleep(0.5)
            except:
                pass

            # 打开配置
            self._app.Open(config_path)
            logger.info("Open()调用完成，开始等待验证...")

            # 等待配置加载完成
            start_time = time.time()
            while time.time() - start_time < timeout:
                try:
                    # 尝试访问Configuration对象
                    config = self._app.Configuration
                    # 尝试访问一些配置属性
                    _ = config.Name
                    logger.info("配置加载成功! Configuration对象可访问")
                    return True
                except Exception as check_err:
                    logger.debug(f"验证中... {check_err}")
                    time.sleep(0.5)

            logger.error(f"配置加载超时（{timeout}秒）")
            return False

        except Exception as e:
            logger.error(f"打开配置失败: {e}")
            return False

    def open_configuration_v4(self, config_path: str, timeout: int = 30) -> bool:
        """
        方式4: Open后固定等待时间
        """
        logger.info(">>> 方式4: Open后固定等待时间")

        try:
            # 关闭当前配置
            try:
                self._app.CloseConfiguration()
                time.sleep(0.5)
            except:
                pass

            # 打开配置
            self._app.Open(config_path)
            logger.info("Open()调用完成，固定等待5秒...")

            time.sleep(5)

            logger.info("等待完成")
            return True

        except Exception as e:
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
            logger.error(f"启动测量失败: {e}")
            return False

    def stop_measurement(self) -> bool:
        """停止测量"""
        try:
            if not self._measurement.Running:
                return True

            logger.info("正在停止测量...")
            self._measurement.Stop()

            time.sleep(1)
            self.is_measurement_running = False
            logger.info("测量已停止")
            return True

        except Exception as e:
            logger.warning(f"停止测量失败: {e}")
            return False

    def get_system_variable(self, namespace: str, variable: str):
        """获取系统变量"""
        try:
            ns = self._namespaces[namespace]
            var = ns.Variables[variable]
            return var.Value
        except Exception as e:
            logger.warning(f"获取变量失败 [{namespace}.{variable}]: {e}")
            return None


def main():
    """主测试函数"""
    logger.info("=" * 60)
    logger.info("CANoe配置文件加载测试")
    logger.info("=" * 60)
    logger.info(f"配置文件路径: {CONFIG_PATH}")
    logger.info(f"超时时间: {OPEN_TIMEOUT}秒")
    logger.info("=" * 60)

    # 检查配置文件
    import os
    if not os.path.exists(CONFIG_PATH):
        logger.error(f"配置文件不存在: {CONFIG_PATH}")
        logger.info("请修改脚本中的 CONFIG_PATH 变量为有效的cfg文件路径")
        return

    tester = CANoeConfigTest()

    try:
        # 连接CANoe
        if not tester.connect():
            logger.error("连接失败，退出测试")
            return

        # 测试方式1
        logger.info("\n" + "=" * 60)
        result1 = tester.open_configuration_v1(CONFIG_PATH, OPEN_TIMEOUT)
        logger.info(f"方式1结果: {'成功' if result1 else '失败'}")

        # 尝试启动测量验证
        if result1:
            measure_result = tester.start_measurement(timeout=10)
            logger.info(f"测量启动: {'成功' if measure_result else '失败'}")
            tester.stop_measurement()

        # 断开重连，测试方式2
        logger.info("\n" + "=" * 60)
        tester.disconnect()
        time.sleep(2)
        tester.connect()

        # 测试方式2
        logger.info("\n" + "=" * 60)
        result2 = tester.open_configuration_v2(CONFIG_PATH, OPEN_TIMEOUT)
        logger.info(f"方式2结果: {'成功' if result2 else '失败'}")

        # 尝试启动测量验证
        if result2:
            measure_result = tester.start_measurement(timeout=10)
            logger.info(f"测量启动: {'成功' if measure_result else '失败'}")
            tester.stop_measurement()

        # 断开重连，测试方式3
        logger.info("\n" + "=" * 60)
        tester.disconnect()
        time.sleep(2)
        tester.connect()

        # 测试方式3
        logger.info("\n" + "=" * 60)
        result3 = tester.open_configuration_v3(CONFIG_PATH, OPEN_TIMEOUT)
        logger.info(f"方式3结果: {'成功' if result3 else '失败'}")

        # 尝试启动测量验证
        if result3:
            measure_result = tester.start_measurement(timeout=10)
            logger.info(f"测量启动: {'成功' if measure_result else '失败'}")
            tester.stop_measurement()

        # 断开重连，测试方式4
        logger.info("\n" + "=" * 60)
        tester.disconnect()
        time.sleep(2)
        tester.connect()

        # 测试方式4
        logger.info("\n" + "=" * 60)
        result4 = tester.open_configuration_v4(CONFIG_PATH, OPEN_TIMEOUT)
        logger.info(f"方式4结果: {'成功' if result4 else '失败'}")

        # 尝试启动测量验证
        if result4:
            measure_result = tester.start_measurement(timeout=10)
            logger.info(f"测量启动: {'成功' if measure_result else '失败'}")
            tester.stop_measurement()

        # 总结
        logger.info("\n" + "=" * 60)
        logger.info("测试总结:")
        logger.info(f"  方式1 (直接返回):      {'成功' if result1 else '失败'}")
        logger.info(f"  方式2 (等待命名空间):  {'成功' if result2 else '失败'}")
        logger.info(f"  方式3 (等待Config):    {'成功' if result3 else '失败'}")
        logger.info(f"  方式4 (固定等待5秒):   {'成功' if result4 else '失败'}")
        logger.info("=" * 60)

    except Exception as e:
        logger.error(f"测试异常: {e}", exc_info=True)

    finally:
        tester.disconnect()
        logger.info("测试结束")


if __name__ == "__main__":
    main()