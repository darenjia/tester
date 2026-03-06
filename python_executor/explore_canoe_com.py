"""
探索CANoe COM接口
用于测试和了解CANoe COM接口的正确用法
"""

import sys
import os
import time

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

def explore_canoe_com():
    """探索CANoe COM接口"""
    try:
        import win32com.client
        
        print("正在连接CANoe...")
        app = win32com.client.Dispatch("CANoe.Application")
        
        print(f"CANoe类型: {type(app)}")
        print(f"CANoe版本: {app.Version}")
        
        # 探索可用的方法和属性
        print("\n探索CANoe.Application的方法和属性:")
        
        # 检查Open方法
        if hasattr(app, 'Open'):
            print("✓ 找到Open方法")
            try:
                # 获取Open方法的参数信息
                import inspect
                print(f"  Open方法: {app.Open}")
            except Exception as e:
                print(f"  无法获取Open方法信息: {e}")
        else:
            print("✗ 未找到Open方法")
        
        # 检查Configuration属性
        if hasattr(app, 'Configuration'):
            print("\n✓ 找到Configuration属性")
            config = app.Configuration
            print(f"  Configuration类型: {type(config)}")
            
            # 检查OpenConfigurationResult
            if hasattr(config, 'OpenConfigurationResult'):
                print("  ✓ 找到OpenConfigurationResult属性")
                try:
                    result = config.OpenConfigurationResult
                    print(f"    当前值: {result}")
                except Exception as e:
                    print(f"    无法获取值: {e}")
            else:
                print("  ✗ 未找到OpenConfigurationResult属性")
        else:
            print("\n✗ 未找到Configuration属性")
        
        # 检查Measurement属性
        if hasattr(app, 'Measurement'):
            print("\n✓ 找到Measurement属性")
            measurement = app.Measurement
            print(f"  Measurement类型: {type(measurement)}")
            
            if hasattr(measurement, 'Running'):
                try:
                    running = measurement.Running
                    print(f"  测量运行状态: {running}")
                except Exception as e:
                    print(f"  无法获取运行状态: {e}")
        else:
            print("\n✗ 未找到Measurement属性")
        
        # 检查System属性
        if hasattr(app, 'System'):
            print("\n✓ 找到System属性")
            system = app.System
            print(f"  System类型: {type(system)}")
        else:
            print("\n✗ 未找到System属性")
        
        # 检查Namespaces
        if hasattr(app, 'Namespaces'):
            print("\n✓ 找到Namespaces属性")
            try:
                namespaces = app.Namespaces
                print(f"  Namespaces类型: {type(namespaces)}")
            except Exception as e:
                print(f"  无法获取Namespaces: {e}")
        else:
            print("\n✗ 未找到Namespaces属性")
        
        # 检查其他可能的方法
        print("\n其他可用属性/方法:")
        for attr in dir(app):
            if not attr.startswith('_'):
                try:
                    obj = getattr(app, attr)
                    if callable(obj):
                        print(f"  方法: {attr}")
                    else:
                        print(f"  属性: {attr}")
                except:
                    pass
        
        print("\n探索完成!")
        
    except ImportError as e:
        print(f"无法导入win32com.client: {e}")
    except Exception as e:
        print(f"探索失败: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    explore_canoe_com()
