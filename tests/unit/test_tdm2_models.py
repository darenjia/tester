"""
TDM2.0模型单元测试

验证TDM2.0字段标准的正确性和完整性
"""

import pytest
from datetime import datetime
from python_executor.models import (
    Case, Task, CaseResult, ExecutionResult, TDM2Response,
    CaseResultStatus, TestToolType
)


class TestCaseModel:
    """测试用例模型测试"""
    
    def test_case_required_fields(self):
        """测试用例必填字段"""
        case = Case(
            moduleLevel1="模块1",
            moduleLevel2="模块2",
            moduleLevel3="模块3",
            caseName="测试用例1",
            priority="高",
            caseType="功能测试",
            preCondition="前置条件",
            stepDescription="步骤描述",
            expectedResult="预期结果",
            maintainer="张三",
            caseNo="CASE_001",
            caseSource="TDM2.0"
        )
        
        assert case.moduleLevel1 == "模块1"
        assert case.caseName == "测试用例1"
        assert case.caseNo == "CASE_001"
    
    def test_case_optional_fields(self):
        """测试用例可选字段"""
        case = Case(
            caseName="测试用例2",
            caseNo="CASE_002",
            changeRecord="变更记录",
            tags="标签1,标签2"
        )
        
        assert case.changeRecord == "变更记录"
        assert case.tags == "标签1,标签2"
    
    def test_case_from_dict(self):
        """测试从字典创建用例"""
        data = {
            "moduleLevel1": "模块A",
            "moduleLevel2": "模块B",
            "caseName": "测试用例A",
            "caseNo": "CASE_A001",
            "priority": "中",
            "caseType": "接口测试"
        }
        
        case = Case.from_dict(data)
        
        assert case.moduleLevel1 == "模块A"
        assert case.caseName == "测试用例A"
        assert case.caseNo == "CASE_A001"
    
    def test_case_to_dict(self):
        """测试用例转换为字典"""
        case = Case(
            caseName="测试用例3",
            caseNo="CASE_003",
            priority="低"
        )
        
        result = case.to_dict()
        
        assert result["caseName"] == "测试用例3"
        assert result["caseNo"] == "CASE_003"
        assert result["priority"] == "低"
        assert "changeRecord" not in result  # 可选字段为None时不包含


class TestTaskModel:
    """任务模型测试"""
    
    def test_task_required_fields(self):
        """测试任务必填字段"""
        task = Task(
            projectNo="PROJ_001",
            taskNo="TASK_001",
            taskName="测试任务1"
        )
        
        assert task.projectNo == "PROJ_001"
        assert task.taskNo == "TASK_001"
        assert task.taskName == "测试任务1"
    
    def test_task_with_cases(self):
        """测试包含用例的任务"""
        case1 = Case(caseName="用例1", caseNo="CASE_001")
        case2 = Case(caseName="用例2", caseNo="CASE_002")
        
        task = Task(
            taskNo="TASK_002",
            taskName="测试任务2",
            caseList=[case1, case2]
        )
        
        assert len(task.caseList) == 2
        assert task.caseList[0].caseNo == "CASE_001"
    
    def test_task_from_dict(self):
        """测试从字典创建任务"""
        data = {
            "projectNo": "PROJ_002",
            "taskNo": "TASK_003",
            "taskName": "测试任务3",
            "caseList": [
                {"caseName": "用例A", "caseNo": "CASE_A"},
                {"caseName": "用例B", "caseNo": "CASE_B"}
            ],
            "toolType": "canoe",
            "configPath": "C:/test.cfg"
        }
        
        task = Task.from_dict(data)
        
        assert task.projectNo == "PROJ_002"
        assert task.taskNo == "TASK_003"
        assert len(task.caseList) == 2
        assert task.toolType == "canoe"
    
    def test_task_to_dict(self):
        """测试任务转换为字典"""
        case = Case(caseName="用例1", caseNo="CASE_001")
        task = Task(
            projectNo="PROJ_003",
            taskNo="TASK_004",
            taskName="测试任务4",
            caseList=[case]
        )
        
        result = task.to_dict()
        
        assert result["projectNo"] == "PROJ_003"
        assert result["taskNo"] == "TASK_004"
        assert len(result["caseList"]) == 1


class TestCaseResultModel:
    """用例结果模型测试"""
    
    def test_case_result_required_fields(self):
        """测试用例结果必填字段"""
        result = CaseResult(
            caseNo="CASE_001",
            result=CaseResultStatus.PASS.value
        )
        
        assert result.caseNo == "CASE_001"
        assert result.result == "PASS"
        assert result.created is not None  # 自动填充时间
    
    def test_case_result_optional_fields(self):
        """测试用例结果可选字段"""
        result = CaseResult(
            caseNo="CASE_002",
            result=CaseResultStatus.FAIL.value,
            remark="执行失败",
            reportPath="C:/report.html"
        )
        
        assert result.remark == "执行失败"
        assert result.reportPath == "C:/report.html"
    
    def test_case_result_from_test_result(self):
        """测试从测试结果创建用例结果"""
        result = CaseResult.from_test_result(
            case_no="CASE_003",
            passed=True,
            message="执行成功",
            report_path="C:/report.pdf"
        )
        
        assert result.caseNo == "CASE_003"
        assert result.result == "PASS"
        assert result.remark == "执行成功"
        assert result.reportPath == "C:/report.pdf"
    
    def test_case_result_to_dict(self):
        """测试用例结果转换为字典"""
        result = CaseResult(
            caseNo="CASE_004",
            result=CaseResultStatus.BLOCK.value,
            remark="阻塞"
        )
        
        data = result.to_dict()
        
        assert data["caseNo"] == "CASE_004"
        assert data["result"] == "BLOCK"
        assert data["remark"] == "阻塞"


class TestExecutionResultModel:
    """执行结果模型测试"""
    
    def test_execution_result_creation(self):
        """测试执行结果创建"""
        result = ExecutionResult(
            taskNo="TASK_001",
            platform="NETWORK"
        )
        
        assert result.taskNo == "TASK_001"
        assert result.platform == "NETWORK"
        assert result.caseList == []
    
    def test_add_case_result(self):
        """测试添加用例结果"""
        execution = ExecutionResult(taskNo="TASK_002")
        
        case_result = CaseResult(
            caseNo="CASE_001",
            result=CaseResultStatus.PASS.value
        )
        
        execution.add_case_result(case_result)
        
        assert len(execution.caseList) == 1
        assert execution.caseList[0].caseNo == "CASE_001"
    
    def test_generate_summary(self):
        """测试生成结果摘要"""
        execution = ExecutionResult(taskNo="TASK_003")
        
        execution.add_case_result(CaseResult(caseNo="C1", result="PASS"))
        execution.add_case_result(CaseResult(caseNo="C2", result="PASS"))
        execution.add_case_result(CaseResult(caseNo="C3", result="FAIL"))
        execution.add_case_result(CaseResult(caseNo="C4", result="BLOCK"))
        execution.add_case_result(CaseResult(caseNo="C5", result="SKIP"))
        
        summary = execution.generate_summary()
        
        assert summary["total"] == 5
        assert summary["passed"] == 2
        assert summary["failed"] == 1
        assert summary["blocked"] == 1
        assert summary["skipped"] == 1
        assert summary["passRate"] == "40.0%"
    
    def test_to_tdm2_format(self):
        """测试转换为TDM2.0格式"""
        execution = ExecutionResult(taskNo="TASK_004")
        execution.add_case_result(CaseResult(caseNo="CASE_001", result="PASS"))
        
        data = execution.to_tdm2_format()
        
        assert data["taskNo"] == "TASK_004"
        assert data["platform"] == "NETWORK"
        assert len(data["caseList"]) == 1
        assert data["caseList"][0]["caseNo"] == "CASE_001"


class TestTDM2ResponseModel:
    """TDM2.0响应模型测试"""
    
    def test_success_response(self):
        """测试成功响应"""
        response = TDM2Response.success("操作成功")
        
        assert response.result == "1"
        assert response.msg == "操作成功"
    
    def test_error_response(self):
        """测试错误响应"""
        response = TDM2Response.error("操作失败", "0")
        
        assert response.result == "0"
        assert response.msg == "操作失败"
    
    def test_response_to_dict(self):
        """测试响应转换为字典"""
        response = TDM2Response(
            result="1",
            msg="成功",
            extInfo="扩展信息"
        )
        
        data = response.to_dict()
        
        assert data["result"] == "1"
        assert data["msg"] == "成功"
        assert data["extInfo"] == "扩展信息"


class TestTDM2FieldMapping:
    """TDM2.0字段映射测试"""
    
    def test_all_case_fields_present(self):
        """测试所有用例字段都存在"""
        case = Case(
            moduleLevel1="M1",
            moduleLevel2="M2",
            moduleLevel3="M3",
            caseName="测试",
            priority="高",
            caseType="功能",
            preCondition="条件",
            stepDescription="步骤",
            expectedResult="预期",
            maintainer="张三",
            caseNo="C001",
            caseSource="TDM",
            changeRecord="记录",
            tags="标签"
        )
        
        data = case.to_dict()
        
        # 验证所有14个字段
        assert "moduleLevel1" in data
        assert "moduleLevel2" in data
        assert "moduleLevel3" in data
        assert "caseName" in data
        assert "priority" in data
        assert "caseType" in data
        assert "preCondition" in data
        assert "stepDescription" in data
        assert "expectedResult" in data
        assert "maintainer" in data
        assert "caseNo" in data
        assert "caseSource" in data
        assert "changeRecord" in data
        assert "tags" in data
    
    def test_result_status_values(self):
        """测试结果状态值测试"""
        assert CaseResultStatus.PASS.value == "PASS"
        assert CaseResultStatus.FAIL.value == "FAIL"
        assert CaseResultStatus.BLOCK.value == "BLOCK"
        assert CaseResultStatus.SKIP.value == "SKIP"
