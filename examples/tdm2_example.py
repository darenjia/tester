"""
TDM2.0字段标准使用示例

展示如何使用TDM2.0字段进行任务接收和结果上报
"""

import json
from python_executor.models import (
    Case, Task, CaseResult, ExecutionResult, TDM2Response,
    CaseResultStatus
)


def example_task_from_tdm2():
    """
    示例：从TDM2.0接收任务数据
    
    模拟网络测试平台从TDM2.0接收到的任务数据格式
    """
    # TDM2.0推送的任务数据（字典格式）
    tdm2_task_data = {
        "projectNo": "PROJ_2025_001",
        "taskNo": "TASK_001",
        "taskName": "CANoe网络测试任务",
        "caseList": [
            {
                "moduleLevel1": "通信测试",
                "moduleLevel2": "CAN总线",
                "moduleLevel3": "报文发送",
                "caseName": "发送标准CAN报文",
                "priority": "高",
                "caseType": "功能测试",
                "preCondition": "CANoe已启动，总线连接正常",
                "stepDescription": "1.配置报文ID为0x123; 2.设置数据长度为8字节; 3.发送报文",
                "expectedResult": "报文成功发送，总线监控显示正确",
                "maintainer": "张三",
                "caseNo": "CASE_CAN_001",
                "caseSource": "TDM2.0",
                "changeRecord": "2025-01-15: 更新预期结果",
                "tags": "CAN,报文发送"
            },
            {
                "moduleLevel1": "通信测试",
                "moduleLevel2": "CAN总线",
                "moduleLevel3": "报文接收",
                "caseName": "接收标准CAN报文",
                "priority": "高",
                "caseType": "功能测试",
                "preCondition": "CANoe已启动，报文发送设备就绪",
                "stepDescription": "1.启动接收监控; 2.等待外部设备发送报文; 3.验证接收数据",
                "expectedResult": "正确接收报文，数据显示正确",
                "maintainer": "李四",
                "caseNo": "CASE_CAN_002",
                "caseSource": "TDM2.0"
            }
        ]
    }
    
    # 创建Task对象
    task = Task.from_dict(tdm2_task_data)
    
    print("=" * 60)
    print("从TDM2.0接收的任务数据")
    print("=" * 60)
    print(f"项目编号: {task.projectNo}")
    print(f"任务编号: {task.taskNo}")
    print(f"任务名称: {task.taskName}")
    print(f"用例数量: {len(task.caseList)}")
    print()
    
    # 显示用例详情
    for i, case in enumerate(task.caseList, 1):
        print(f"用例 {i}:")
        print(f"  用例编号: {case.caseNo}")
        print(f"  用例名称: {case.caseName}")
        print(f"  模块: {case.moduleLevel1} > {case.moduleLevel2} > {case.moduleLevel3}")
        print(f"  优先级: {case.priority}")
        print(f"  维护人: {case.maintainer}")
        print()
    
    return task


def example_result_to_tdm2():
    """
    示例：向TDM2.0上报执行结果
    
    模拟执行完成后，将结果转换为TDM2.0格式并上报
    """
    # 创建执行结果
    execution_result = ExecutionResult(
        taskNo="TASK_001",
        platform="NETWORK"
    )
    
    # 添加用例执行结果
    execution_result.add_case_result(CaseResult(
        caseNo="CASE_CAN_001",
        result=CaseResultStatus.PASS.value,
        remark="报文发送成功，总线监控显示正确",
        reportPath="/reports/CASE_CAN_001.html"
    ))
    
    execution_result.add_case_result(CaseResult(
        caseNo="CASE_CAN_002",
        result=CaseResultStatus.FAIL.value,
        remark="报文接收超时，未收到预期数据",
        reportPath="/reports/CASE_CAN_002.html"
    ))
    
    execution_result.add_case_result(CaseResult(
        caseNo="CASE_CAN_003",
        result=CaseResultStatus.BLOCK.value,
        remark="前置条件不满足，测试环境未就绪"
    ))
    
    execution_result.add_case_result(CaseResult(
        caseNo="CASE_CAN_004",
        result=CaseResultStatus.SKIP.value,
        remark="用例不适用当前测试版本"
    ))
    
    # 生成结果摘要
    summary = execution_result.generate_summary()
    
    print("=" * 60)
    print("向TDM2.0上报的执行结果")
    print("=" * 60)
    print(f"任务编号: {execution_result.taskNo}")
    print(f"平台名称: {execution_result.platform}")
    print()
    print("结果摘要:")
    print(f"  总计: {summary['total']}")
    print(f"  通过: {summary['passed']}")
    print(f"  失败: {summary['failed']}")
    print(f"  阻塞: {summary['blocked']}")
    print(f"  跳过: {summary['skipped']}")
    print(f"  通过率: {summary['passRate']}")
    print()
    
    # 转换为TDM2.0格式
    tdm2_format = execution_result.to_tdm2_format()
    
    print("TDM2.0格式JSON:")
    print(json.dumps(tdm2_format, indent=2, ensure_ascii=False))
    print()
    
    return execution_result


def example_response_to_tdm2():
    """
    示例：向TDM2.0返回响应
    
    模拟任务接收后的响应格式
    """
    print("=" * 60)
    print("TDM2.0响应示例")
    print("=" * 60)
    
    # 成功响应
    success_response = TDM2Response.success("任务接收成功")
    print("成功响应:")
    print(json.dumps(success_response.to_dict(), indent=2, ensure_ascii=False))
    print()
    
    # 错误响应
    error_response = TDM2Response.error("任务格式错误：缺少必填字段taskNo", "0")
    print("错误响应:")
    print(json.dumps(error_response.to_dict(), indent=2, ensure_ascii=False))
    print()


def example_full_workflow():
    """
    示例：完整工作流程
    
    展示从接收任务到上报结果的完整流程
    """
    print("=" * 60)
    print("完整工作流程示例")
    print("=" * 60)
    print()
    
    # 1. 接收TDM2.0任务
    print("步骤1: 接收TDM2.0任务")
    task_data = {
        "projectNo": "PROJ_001",
        "taskNo": "TASK_FULL_001",
        "taskName": "完整流程测试",
        "caseList": [
            {
                "caseName": "测试用例1",
                "caseNo": "CASE_001",
                "priority": "高",
                "caseType": "功能测试",
                "expectedResult": "预期结果1"
            },
            {
                "caseName": "测试用例2",
                "caseNo": "CASE_002",
                "priority": "中",
                "caseType": "性能测试",
                "expectedResult": "预期结果2"
            }
        ]
    }
    
    task = Task.from_dict(task_data)
    print(f"  接收任务: {task.taskNo}")
    print(f"  任务名称: {task.taskName}")
    print(f"  用例数量: {len(task.caseList)}")
    print()
    
    # 2. 模拟执行
    print("步骤2: 执行测试用例")
    execution_result = ExecutionResult(
        taskNo=task.taskNo,
        platform="NETWORK"
    )
    
    for case in task.caseList:
        # 模拟执行结果（实际执行时会调用适配器）
        passed = case.caseNo == "CASE_001"  # 第一个通过，第二个失败
        result_status = CaseResultStatus.PASS.value if passed else CaseResultStatus.FAIL.value
        
        case_result = CaseResult(
            caseNo=case.caseNo,
            result=result_status,
            remark=f"执行{'成功' if passed else '失败'}"
        )
        execution_result.add_case_result(case_result)
        print(f"  {case.caseNo}: {result_status}")
    
    print()
    
    # 3. 生成并上报结果
    print("步骤3: 生成执行结果")
    execution_result.generate_summary()
    
    tdm2_result = execution_result.to_tdm2_format()
    print(f"  任务编号: {tdm2_result['taskNo']}")
    print(f"  平台名称: {tdm2_result['platform']}")
    print(f"  用例结果数: {len(tdm2_result['caseList'])}")
    print()
    
    print("TDM2.0上报JSON:")
    print(json.dumps(tdm2_result, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    print("\n" + "=" * 60)
    print("TDM2.0字段标准使用示例")
    print("=" * 60)
    print()
    
    # 运行示例
    example_task_from_tdm2()
    example_result_to_tdm2()
    example_response_to_tdm2()
    example_full_workflow()
    
    print()
    print("=" * 60)
    print("示例运行完成")
    print("=" * 60)
