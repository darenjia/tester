using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace ProcessEngine
{
    public class ProcLog
    {
        private ProcStore _store = new ProcStore();
        public static string LoginNo;
        /// <summary>
        /// _dictLog为数据库记录中主要的值
        /// </summary>
        private Dictionary<string, object> _dictLog = new Dictionary<string, object>();

        #region 操作记录
        /// <summary>
        /// 如果是Dictionary格式可以直接调用
        /// </summary>
        /// <param name="enumTable">表</param>
        /// <param name="dictConfig">输入的参数</param>
        public void WriteLog(EnumLibrary.EnumLog EnumLog, Dictionary<string, object> dictConfig)
        {
            string error = "";
            _dictLog["OperNo"] = GetStrOprationNo();
            _dictLog["EmployeeNo"] = dictConfig["EmployeeNo"];
            _dictLog["EmployeeName"] = dictConfig["EmployeeName"];
            _dictLog["OperTable"] = dictConfig["OperTable"];
            IsSwitch(EnumLog, dictConfig);
            _store.AddOperationLog(_dictLog, out error);
        }

        /// <summary>
        /// 判断是操作到哪个表
        /// </summary>
        /// <param name="enumTable">表</param>
        private void IsSwitch(EnumLibrary.EnumLog EnumLog, Dictionary<string, object> dictConfig)
        {
            switch (EnumLog)
            {
                #region 车型
                case EnumLibrary.EnumLog.AddVehicel:
                    string aVehicelContent = dictConfig["AuthorizeTo"].ToString() + dictConfig["VehicelType"] + "-" + dictConfig["VehicelConfig"] + "-" +
                                             dictConfig["VehicelStage"];
                    AddToLog(dictConfig["EmployeeName"].ToString(), aVehicelContent);

                    break;
                case EnumLibrary.EnumLog.DelVehicel:
                    string dVehicelContent = dictConfig["AuthorizeTo"].ToString() + dictConfig["VehicelType"] + "-" + dictConfig["VehicelConfig"] + "-" +
                                             dictConfig["VehicelStage"];
                    DelToLog(dictConfig["EmployeeName"].ToString(), dVehicelContent);
                    break;

                #endregion


                #region 任务
                case EnumLibrary.EnumLog.AddTask:
                    string aTaskContent = dictConfig["AuthTester"].ToString() + dictConfig["TaskNo"] + "-" + dictConfig["TaskRound"] + "-" + dictConfig["TaskName"] + "-" + dictConfig["CANRoad"] + "-" + dictConfig["Module"];
                    AddToLog(dictConfig["EmployeeName"].ToString(), aTaskContent);
                    break;
                case EnumLibrary.EnumLog.DelTask:
                    string dTaskContent = dictConfig["AuthTester"].ToString() + dictConfig["TaskNo"] + "-" + dictConfig["TaskRound"] + "-" + dictConfig["TaskName"] + "-" + dictConfig["CANRoad"] + "-" + dictConfig["Module"];
                    DelToLog(dictConfig["EmployeeName"].ToString(), dTaskContent);
                    break;
                #endregion

                #region 上传DBC
                case EnumLibrary.EnumLog.AddDBC:
                    string aDBCContent = dictConfig["DBCName"].ToString() + dictConfig["BelongCAN"];
                    AddToLog(dictConfig["EmployeeName"].ToString(), aDBCContent);
                    break;
                case EnumLibrary.EnumLog.DelDBC:
                    string dDBCContent = dictConfig["DBCName"].ToString() + dictConfig["BelongCAN"];
                    DelToLog(dictConfig["EmployeeName"].ToString(), dDBCContent);
                    break;
                #endregion

                case EnumLibrary.EnumLog.Tply:
                    _dictLog["OperRecord"] = "[" + dictConfig["EmployeeName"] + "]对车型[" + dictConfig["VehicelType"] + "-" + dictConfig["VehicelConfig"] + "-" + dictConfig["VehicelStage"] + "]上传拓扑图";
                    break;
                case EnumLibrary.EnumLog.UpdateTply:
                    _dictLog["OperRecord"] = "[" + dictConfig["EmployeeName"] + "]对车型[" + dictConfig["VehicelType"] + "-" + dictConfig["VehicelConfig"] + "-" + dictConfig["VehicelStage"] + "]修改拓扑图";
                    break;
                case EnumLibrary.EnumLog.CfgTemplate:
                    _dictLog["OperRecord"] = "[" + dictConfig["EmployeeName"]+ "]对车型[" + dictConfig["VehicelType"] + "-" + dictConfig["VehicelConfig"] + "-" + dictConfig["VehicelStage"] + "]上传配置表";
                    break;
                case EnumLibrary.EnumLog.UpdateCfgTemplate:
                    _dictLog["OperRecord"] = "[" + dictConfig["EmployeeName"] + "]对车型[" + dictConfig["VehicelType"] + "-" + dictConfig["VehicelConfig"] + "-" + dictConfig["VehicelStage"] + "]更新配置表";
                    break;
                case EnumLibrary.EnumLog.EmlTemplate:
                    _dictLog["OperRecord"] = "[" + dictConfig["EmployeeName"] + "]对车型[" + dictConfig["VehicelType"] + "-" + dictConfig["VehicelConfig"] + "-" + dictConfig["VehicelStage"] + "]上传用例表";
                    break;
                case EnumLibrary.EnumLog.TaskEmlTemplate:
                    _dictLog["OperRecord"] = "[" + dictConfig["EmployeeName"] + "]对任务[" + dictConfig["TaskNo"] + "-" + dictConfig["TaskRound"] + "-" + dictConfig["TaskName"] + "-" + dictConfig["CANRoad"] + "-" + dictConfig["Module"] + "]上传用例表";
                    break;

                #region 供应商
                case EnumLibrary.EnumLog.AddSuppiers:
                    string addContent = dictConfig["Suppier"] + "-" + dictConfig["Type"] + "-" +
      dictConfig["Moudle"];
                    AddToLog(dictConfig["EmployeeName"].ToString(), addContent);
                    break;
                case EnumLibrary.EnumLog.UpdateSuppiers:
                    string srcContent = dictConfig["oldSuppier"] + "-" + dictConfig["oldType"] + "-" +
                  dictConfig["oldMoudle"];
                    string tarContent = dictConfig["newSuppier"] + "-" + dictConfig["newType"] + "-" +
                  dictConfig["newMoudle"];
                    UpdateToLog(dictConfig["EmployeeName"].ToString(), srcContent, tarContent);
                    break;
                case EnumLibrary.EnumLog.DelSuppiers:
                    string content = dictConfig["Suppier"] + "-" + dictConfig["Type"] + "-" +
                  dictConfig["Moudle"];
                    DelToLog(dictConfig["EmployeeName"].ToString(), content);
                    break;
                #endregion

                #region 部门
                case EnumLibrary.EnumLog.AddDepartment:
                    string aDepContent = dictConfig["Master"] + "-" + dictConfig["Name"];
                    AddToLog(dictConfig["EmployeeName"].ToString(), aDepContent);
                    break;
                case EnumLibrary.EnumLog.UpdateDepartment:
                    string uDepSrcContent = dictConfig["oldMaster"] + "-" + dictConfig["oldName"];
                    string uDepTarContent = dictConfig["newMaster"] + "-" + dictConfig["newName"];
                    UpdateToLog(dictConfig["EmployeeName"].ToString(), uDepSrcContent, uDepTarContent);
                    break;
                case EnumLibrary.EnumLog.DelDepartment:
                    string dDepcontent = dictConfig["Master"] + "-" + dictConfig["Name"];
                    DelToLog(dictConfig["EmployeeName"].ToString(), dDepcontent);
                    break;
                #endregion

                #region 员工
                case EnumLibrary.EnumLog.AddEmployee:

                    string aEmpContent = dictConfig["ElyName"] + "-" + dictConfig["ElyRole"] + "-" + dictConfig["Department"];
                    AddToLog(dictConfig["EmployeeName"].ToString(), aEmpContent);
                    break;
                case EnumLibrary.EnumLog.UpdateEmployee:
                    string uEmpSrcContent = dictConfig["oldElyName"] + "-" + dictConfig["oldRole"] + "-" + dictConfig["oldDepartment"];
                    string uEmpTarContent = dictConfig["newElyName"] + "-" + dictConfig["newRole"] + "-" + dictConfig["newDepartment"];
                    UpdateToLog(dictConfig["EmployeeName"].ToString(), uEmpSrcContent, uEmpTarContent);
                    break;
                case EnumLibrary.EnumLog.DelEmployee:
                    string dEmpcontent = dictConfig["ElyName"] + "-" + dictConfig["ElyRole"] + "-" + dictConfig["Department"];
                    DelToLog(dictConfig["EmployeeName"].ToString(), dEmpcontent);
                    break;
                #endregion

                #region 章节
                case EnumLibrary.EnumLog.AddChapter:

                    string aCptContent = dictConfig["ChapterName"].ToString();
                    AddToLog(dictConfig["EmployeeName"].ToString(), aCptContent);
                    break;
                case EnumLibrary.EnumLog.UpdateChapter:
                    string uCptSrcContent = dictConfig["oldChapterName"].ToString();
                    string uCptTarContent = dictConfig["newChapterName"].ToString();
                    UpdateToLog(dictConfig["EmployeeName"].ToString(), uCptSrcContent, uCptTarContent);
                    break;
                case EnumLibrary.EnumLog.DelChapter:
                    string dCptContent = dictConfig["ChapterName"].ToString();
                    DelToLog(dictConfig["EmployeeName"].ToString(), dCptContent);
                    break;
                #endregion

                #region 路径
                case EnumLibrary.EnumLog.UpdatePath:
                    string uPathSrcContent = dictConfig["oldChapterName"].ToString();
                    string uPathTarContent = dictConfig["newChapterName"].ToString();
                    UpdateToLog(dictConfig["EmployeeName"].ToString(), uPathSrcContent, uPathTarContent);
                    break;
                #endregion

                #region 故障类型
                case EnumLibrary.EnumLog.AddErrorType:

                    string aErrContent = dictConfig["ErrorType"].ToString();
                    AddToLog(dictConfig["EmployeeName"].ToString(), aErrContent);
                    break;
                case EnumLibrary.EnumLog.UpdateErrorType:
                    string uErrSrcContent = dictConfig["oldErrorType"].ToString();
                    string uErrTarContent = dictConfig["newErrorType"].ToString();
                    UpdateToLog(dictConfig["EmployeeName"].ToString(), uErrSrcContent, uErrTarContent);
                    break;
                case EnumLibrary.EnumLog.DelErrorType:
                    string dErrContent = dictConfig["ErrorType"].ToString();
                    DelToLog(dictConfig["EmployeeName"].ToString(), dErrContent);
                    break;
                #endregion

                #region 节点配置盒
                case EnumLibrary.EnumLog.AddCfgBox:

                    string aCfgContent = dictConfig["Name"] + "-" + dictConfig["ID"] + "-" + dictConfig["Count"];
                    AddToLog(dictConfig["EmployeeName"].ToString(), aCfgContent);
                    break;
                case EnumLibrary.EnumLog.UpdateCfgBox:
                    string uCfgSrcContent = dictConfig["oldName"] + "-" + dictConfig["oldID"] + "-" + dictConfig["oldCount"];
                    string uCfgTarContent = dictConfig["newName"] + "-" + dictConfig["newID"] + "-" + dictConfig["newCount"];
                    UpdateToLog(dictConfig["EmployeeName"].ToString(), uCfgSrcContent, uCfgTarContent);
                    break;
                case EnumLibrary.EnumLog.DelCfgBox:
                    string dCfgContent = dictConfig["Name"] + "-" + dictConfig["ID"] + "-" + dictConfig["Count"];
                    DelToLog(dictConfig["EmployeeName"].ToString(), dCfgContent);
                    break;
                #endregion

                #region 网段编辑器
                case EnumLibrary.EnumLog.AddCAN:

                    string aCANContent = dictConfig["SegName"] + "-" + dictConfig["Baud"];
                    AddToLog(dictConfig["EmployeeName"].ToString(), aCANContent);
                    break;
                case EnumLibrary.EnumLog.UpdateCAN:
                    string uCANSrcContent = dictConfig["oldSegName"] + "-" + dictConfig["oldBaud"];
                    string uCANTarContent = dictConfig["newSegName"] + "-" + dictConfig["newBaud"];
                    UpdateToLog(dictConfig["EmployeeName"].ToString(), uCANSrcContent, uCANTarContent);
                    break;
                case EnumLibrary.EnumLog.DelCAN:
                    string dCANContent = dictConfig["SegName"] + "-" + dictConfig["Baud"];
                    DelToLog(dictConfig["EmployeeName"].ToString(), dCANContent);
                    break;
                #endregion

                #region 导出
                case EnumLibrary.EnumLog.ExportReport:
                    _dictLog["Operate"] = "导出报告";
                    _dictLog["OperRecord"] = "[" + dictConfig["EmployeeName"] + "] 将 [" + dictConfig["TaskNo"] + "-" + dictConfig["TaskRound"] + "-" + dictConfig["TaskName"] + "-" + dictConfig["CANRoad"] + "-" + dictConfig["Moudle"] + "] 报告导出";
                    break;
                #endregion

                #region 复制粘贴
                case EnumLibrary.EnumLog.Copy:
                    _dictLog["OperRecord"] = "[" + dictConfig["EmployeeName"] + "] 将 [" + dictConfig["oldVehicelType"] + "-" + dictConfig["oldVehicelConfig"] + " 下的 " + dictConfig["oldVehicelStage"] + "] 复制到 [" + dictConfig["newVehicelType"] + " - " + dictConfig["newVehicelConfig"] + "]";
                    break;
                #endregion

                default:;
                    break;
            }
        }

        /// <summary>
        /// 获取数据库条数+1作为编号
        /// </summary>
        /// <returns></returns>
        private string GetStrOprationNo()
        {
            IList<object[]> listNo = _store.GetRegularByEnum(EnumLibrary.EnumTable.OperationLog);
            int No = listNo.Count + 1;
            string strElyNo = "";
            if (No < 10)
                strElyNo = @"No.00000" + No;
            else if (No >= 10 && No < 100)
                strElyNo = @"No.0000" + No;
            else if (No >= 100 && No < 1000)
                strElyNo = @"No.000" + No;
            else if (No >= 1000 && No < 10000)
                strElyNo = @"No.00" + No;
            else if (No >= 10000 && No < 100000)
                strElyNo = @"No.0" + No;
            else
                strElyNo = @"No." + No;
            return strElyNo;
        }
        #endregion

        #region 登陆数据
        /// <summary>
        /// 记录用户登陆的数据
        /// </summary>
        private Dictionary<string, object> _dictUserLog = new Dictionary<string, object>();

        /// <summary>
        /// 从临时变量里赋值到Dictionary型记录
        /// </summary>
        /// <param name="enumTable">表</param>
        /// <param name="dictConfig">输入的参数</param>
        public string WriteLoginLog(EnumLibrary.EnumTable enumTable, Dictionary<string, object> dictConfig, out string error)
        {
            ConfigToUserLog(dictConfig);
            _store.AddLoginLog(_dictUserLog, out error);
            return LoginNo;
        }

        private void ConfigToUserLog(Dictionary<string, object> dictConfig)
        {
            LoginNo = GetStrLoginNo();
            _dictUserLog["LoginNo"] = LoginNo;
            _dictUserLog["EmployeeNo"] = dictConfig["EmployeeNo"];
            _dictUserLog["EmployeeName"] = dictConfig["EmployeeName"];
            _dictUserLog["Department"] = dictConfig["Department"];
            _dictUserLog["LoginDate"] = dictConfig["LoginDate"];
            _dictUserLog["LoginOffDate"] = dictConfig["LoginOffDate"];
        }

        /// <summary>
        /// 获取数据库条数+1作为编号
        /// </summary>
        /// <returns></returns>
        private string GetStrLoginNo()
        {
            IList<object[]> listNo = _store.GetRegularByEnum(EnumLibrary.EnumTable.LoginLog);
            int No = listNo.Count + 1;
            string strElyNo = "";
            if (No < 10)
                strElyNo = @"No.000" + No;
            else if (No >= 10 && No < 100)
                strElyNo = @"No.00" + No;
            else if (No >= 100 && No < 1000)
                strElyNo = @"No.0" + No;
            else if (No >= 1000 && No < 10000)
                strElyNo = @"No." + No;
            return strElyNo;
        }

        public void UpdateLoginNo()
        {
            string error = "";
            _dictUserLog["LoginNo"] = LoginNo;
            _dictUserLog["LoginOffDate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _store.Update(EnumLibrary.EnumTable.LoginLog, _dictUserLog, out error);
        }
        #endregion

        #region  添加

        private void AddToLog(string userName,string content)
        {

            _dictLog["OperRecord"] = "[" + userName + "] 添加了 [" + content + "] 的记录";
        }

        private void UpdateToLog(string userName, string srcContent,string tarContent)
        {
            _dictLog["OperRecord"] = "[" + userName + "] 将 [" + srcContent + "] 变更为   [" + tarContent + "] ";
        }

        private void DelToLog(string userName, string content)
        {
            _dictLog["OperRecord"] = "[" + userName + "] 删除了 [" + content + "] 的记录";
        }
        #endregion
    }
}
