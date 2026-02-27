using System.Collections.Generic;
using System.Data;
using Model;

namespace DBEngine
{
    public class ExcuteSqlCase
    {
        #region 更新的SQL语句

        public static string Suppliers_Update(Dictionary<string, object> dictDep)
        {
            string queryStr = "update Suppliers set Module='" + dictDep["Module"] + "'" +
                              ", Type='" + dictDep["Type"] + "'" +
                              ", Supplier='" + dictDep["Supplier"] + "'" +
                              ", Contact='" + dictDep["Contact"] + "'" +
                              ", Tel='" + dictDep["Tel"] + "'" +
                              " where Module='" + dictDep["Module"] + "'" +
                              "and Type='" + dictDep["Type"] + "'" +
                              "and Supplier='" + dictDep["Supplier"] + "'";
            return queryStr;
        }

        public static string FileLinkByVehicel_Update(Dictionary<string, object> dictDep)
        {
            var queryStr = "update FileLinkByVehicel set Topology='" + dictDep["Topology"] + "'" +
                            ", TplyDescrible='" + dictDep["TplyDescrible"] + "'" + 
                           " where VehicelType='" + dictDep["VehicelType"] + "'" +
                           "and VehicelConfig='" + dictDep["VehicelConfig"] + "'" +
                           "and VehicelStage='" + dictDep["VehicelStage"] + "'";
            return queryStr;
        }
        public static string Topology_Update(Dictionary<string, object> dictDep)
        {
            var queryStr = "update Topology set Tply='" + dictDep["Tply"] + "'" +
                           ", TplyDescrible='" + dictDep["TplyDescrible"] + "'" +
                           ", ImportUser='" + dictDep["ImportUser"] + "'" +
                           ", ImportTime='" + dictDep["ImportTime"] + "'" +
                           " where VehicelType='" + dictDep["VehicelType"] + "'" +
                           "and VehicelConfig='" + dictDep["VehicelConfig"] + "'" +
                           "and VehicelStage='" + dictDep["VehicelStage"] + "'";
            return queryStr;
        }
        public static string QuestionNote_Update(Dictionary<string, object> dictEmp)
        {
            string queryStr = "update QuestionNote set FailItemInfo='" + dictEmp["FailItemInfo"] + "'" +
                              " where VehicelType = '" + dictEmp["VehicelType"] + "'" +
                              " and VehicelConfig='" + dictEmp["VehicelConfig"] + "'" +
                              " and VehicelStage='" + dictEmp["VehicelStage"] + "'" +
                              " and TaskRound='" + dictEmp["TaskRound"] + "'" +
                              " and TestType='" + dictEmp["TestType"] + "'" +
                              " and Module='" + dictEmp["Module"] + "'" +
                              " and TestTime='" + dictEmp["TestTime"] + "'";
            return queryStr;
        }
        public static string PassReportNote_Update(Dictionary<string, object> dictEmp)
        {
            string queryStr = "update PassReportNote set FailItemInfo='" + dictEmp["FailItemInfo"] + "'" +
                              " where VehicelType = '" + dictEmp["VehicelType"] + "'" +
                              " and VehicelConfig='" + dictEmp["VehicelConfig"] + "'" +
                              " and VehicelStage='" + dictEmp["VehicelStage"] + "'" +
                              " and TaskRound='" + dictEmp["TaskRound"] + "'" +
                              " and TestType='" + dictEmp["TestType"] + "'" +
                              " and Module='" + dictEmp["Module"] + "'" +
                              " and TestTime='" + dictEmp["TestTime"] + "'";
            return queryStr;
        }
        public static string DBC_Update(Dictionary<string, object> dictDBC)
        {
            string queryStr = "update DBC set FormerDBCName='" + dictDBC["FormerDBCName"] + "'" +
                              ", CANType='" + dictDBC["CANType"] + "'" + 
                              ", ImportTime='" + dictDBC["ImportTime"] + "'" +
                              " where VehicelType = '" + dictDBC["VehicelType"] + "'" +
                              " and VehicelConfig='" + dictDBC["VehicelConfig"] + "'" +
                              " and VehicelStage='" + dictDBC["VehicelStage"] + "'" +
                              " and DBCName='" + dictDBC["DBCName"] + "'" +
                              " and BelongCAN='" + dictDBC["BelongCAN"] + "'" +
                              " and DBCContent='" + dictDBC["DBCContent"] + "'" +
                              " and ImportUser='" + dictDBC["ImportUser"] + "'";
            return queryStr;  
        }
        public static string FileLinkByVehicelEml_Col(Dictionary<string, object> dictDep)
        {
            var queryStr = "update FileLinkByVehicel set EmlTableColEdit='" + dictDep["EmlTableColEdit"] + "'" +
                            " where VehicelType='" + dictDep["VehicelType"] + "'" +
                           "and VehicelConfig='" + dictDep["VehicelConfig"] + "'" +
                           "and VehicelStage='" + dictDep["VehicelStage"] + "'" +
                           " and MatchSort='" + dictDep["MatchSort"] + "'" +
                           " and EmlTemplateName='" + dictDep["EmlTemplateName"] + "'";
            return queryStr;
        }

        public static string FileLinkByVehicel_ColEml(Dictionary<string, object> dictDep)
        {
            var queryStr = "update FileLinkByVehicel set ConTableColEdit='" + dictDep["ConTableColEdit"] + "'" +
                            " where VehicelType='" + dictDep["VehicelType"] + "'" +
                           "and VehicelConfig='" + dictDep["VehicelConfig"] + "'" +
                           "and VehicelStage='" + dictDep["VehicelStage"] + "'" +
                           " and MatchSort='" + dictDep["MatchSort"] + "'";
            return queryStr;
        }
        public static string Password_Update(Dictionary<string, object> dictElyPwd)
        {
           var delStr = "update  Employee set Password='" + dictElyPwd["Password"] + "'" +
                             "where ElyName='" + dictElyPwd["ElyName"] + "'";
                return delStr;
            
        }

        public static string Photo_Update(Dictionary<string, object> dictElyPwd)
        {
            var delStr = "update  Employee set Photo='" + dictElyPwd["Photo"] + "'" +
               "where ElyName='" + dictElyPwd["ElyName"] + "'";
            return delStr;
        }


        public static string EmlTemp_Update(Dictionary<string, object> dictEmp)
        {
            string queryStr = "update FileLinkByVehicel set EmlTemplateName='" + dictEmp["OldEmlTemplateName"] + "'" +
                              ", EmlTemplate='" + dictEmp["EmlTemplate"] + "'" +
                              ", EmlTableColEdit='" + dictEmp["EmlTableColEdit"] + "'" +
                              " where VehicelType= '" + dictEmp["VehicelType"] + "'" +
                              " and VehicelConfig='" + dictEmp["VehicelConfig"] + "'" +
                              " and VehicelStage='" + dictEmp["VehicelStage"] + "'" +
                              " and MatchSort='" + dictEmp["MatchSort"] + "'" +
                              " and EmlTemplateName='" + dictEmp["OldEmlTemplateName"] + "'"; 
            return queryStr;
        }

        public static string CfgTmp_Update(Dictionary<string, object> dictEmp)
        {
            string queryStr = "update FileLinkByVehicel set CfgTemplateJson='" + dictEmp["CfgTemplateJson"] + "'" +
                              " where VehicelType= '" + dictEmp["VehicelType"] + "'" +
                              " and VehicelConfig='" + dictEmp["VehicelConfig"] + "'" +
                              " and VehicelStage='" + dictEmp["VehicelStage"] + "'" +
                              " and MatchSort='" + dictEmp["MatchSort"] + "'" +
                              " and CfgTemplateName='" + dictEmp["CfgTemplateName"] + "'";
            return queryStr;
        }

        public static string EmlTempSort_Update(Dictionary<string, object> dictEmp)
        {
            string queryStr = "update FileLinkByVehicel set EmlTemplateName='" + dictEmp["EmlTemplateName"] + "'" +
                              ", EmlTemplate='" + dictEmp["EmlTemplate"] + "'" +
                              ", EmlTableColEdit='" + dictEmp["EmlTableColEdit"] + "'" +
                              " where VehicelType= '" + dictEmp["VehicelType"] + "'" +
                              " and VehicelConfig='" + dictEmp["VehicelConfig"] + "'" +
                              " and VehicelStage='" + dictEmp["VehicelStage"] + "'" +
                              " and MatchSort='" + dictEmp["MatchSort"] + "'" ;
            return queryStr;
        }
        public static string CfgTemp_Update(Dictionary<string, object> dictEmp)
        {
            string queryStr = "update FileLinkByVehicel set CfgTemplate='" + dictEmp["CfgTemplate"] + "'" + 
                             ", CfgTemplateJson='" + dictEmp["CfgTemplateJson"] + "'" +
                             " where VehicelType= '" + dictEmp["VehicelType"] + "'" +
                              " and VehicelConfig='" + dictEmp["VehicelConfig"] + "'" +
                              " and VehicelStage='" + dictEmp["VehicelStage"] + "'" +
                              " and MatchSort='" + dictEmp["MatchSort"] + "'";
            return queryStr;
        }

        public static string CfgTempBaud_Update(Dictionary<string, object> dictEmp)
        {
            string queryStr = "update FileLinkByVehicel set CfgTemplate='" + dictEmp["CfgTemplate"] + "'" +
                            ", CfgBaudJson='" + dictEmp["CfgBaudJson"] + "'" +
                             ", CfgTemplateJson='" + dictEmp["CfgTemplateJson"] + "'" +
                             ", ConTableColEdit='" + dictEmp["ConTableColEdit"] + "'" +
                             " where VehicelType= '" + dictEmp["VehicelType"] + "'" +
                              " and VehicelConfig='" + dictEmp["VehicelConfig"] + "'" +
                              " and VehicelStage='" + dictEmp["VehicelStage"] + "'" +
                              " and MatchSort='" + dictEmp["MatchSort"] + "'";
            return queryStr;
        }

        public static string CfgBaud_Update(Dictionary<string, object> dictEmp)
        {
            string queryStr = "update FileLinkByVehicel set CfgBaudJson='" + dictEmp["CfgBaudJson"] + "'" +
                              " where VehicelType= '" + dictEmp["VehicelType"] + "'" +
                              " and VehicelConfig='" + dictEmp["VehicelConfig"] + "'" +
                              " and VehicelStage='" + dictEmp["VehicelStage"] + "'";
            return queryStr;
        }

        public static string Task_Update(Dictionary<string, object> dictTask)
        {
            string queryStr = "update Task set ContainExmp='" + dictTask["ContainExmp"] + "'" +
                              " where TaskNo= '" + dictTask["TaskNo"] + "'" +
                              " and TaskRound='" + dictTask["TaskRound"] + "'" +
                              " and TaskName='" + dictTask["TaskName"] + "'" +
                              " and CANRoad='" + dictTask["CANRoad"] + "'" +
                              " and Module='" + dictTask["Module"] + "'";
            return queryStr;
        }
        public static string TaskDTC_Update(Dictionary<string, object> dictTask)
        {
            string queryStr = "update Task set ContainExmp='" + dictTask["ContainExmp"] + "'" +
                              " where TaskNo= '" + dictTask["TaskNo"] + "'" +
                              " and TaskRound='" + dictTask["TaskRound"] + "'" +
                              " and TaskName='" + dictTask["TaskName"] + "'" +
                              " and CANRoad='" + dictTask["CANRoad"] + "'" +
                              " and Module='" + dictTask["Module"] + "'";
            return queryStr;
        }
        public static string EmlToly_Update(Dictionary<string, object> dictEmp)
        {
            string queryStr = "update FileLinkByVehicel set Topology='" + dictEmp["Topology"] + "'" +
                              " where VehicelType = '" + dictEmp["VehicelType"] + "'" +
                              " and VehicelConfig='" + dictEmp["VehicelConfig"] + "'" +
                              " and VehicelStage='" + dictEmp["VehicelStage"] + "'";
            return queryStr;
        }

        public static string Employee_Update(Dictionary<string, object> dictEmp)
        {
            string queryStr = "update Employee set ElyName='" + dictEmp["ElyName"] + "'" +
                                ", ElyRole='" + dictEmp["ElyRole"] + "'" +
                                ", Department='" + dictEmp["Department"] + "'" +
                                ", Sex='" + dictEmp["Sex"] + "'" +
                                ", Contact='" + dictEmp["Contact"] + "'" +
                                ", Mail='" + dictEmp["Mail"] + "'" +
                                ", Remark='" + dictEmp["Remark"] + "'" +
                                " where ElyNo='" + dictEmp["ElyNo"] + "'";
            return queryStr;
        }

        public static string FaultType_Update(Dictionary<string, object> dictEmp)
        {
            string queryStr = "update FaultType set ErrorType='" + dictEmp["ErrorType"] + "'" +
                                ", IsMessage='" + dictEmp["IsMessage"] + "'" +
                                ", MessageCount='" + dictEmp["MessageCount"] + "'" +
                                ", MsgInformation='" + dictEmp["MsgInformation"] + "'" +
                                ", CheckInfor='" + dictEmp["CheckInfor"] + "'" +
                                " where ErrorType='" + dictEmp["oldErrorType"] + "'";
            return queryStr;
        }
        public static string Department_Update(Dictionary<string, object> dictDep)
        {
            string queryStr = "update Department set Name='" + dictDep["Name"] + "'" +
                                ", Master='" + dictDep["Master"] + "'" +
                                ", NumForDept='" + dictDep["NumForDept"] + "'" +
                                ", Remark='" + dictDep["Remark"] + "'" +
                                " where Name='" + dictDep["oldName"] + "'";
            return queryStr;
        }

        public static string Authorization_Update(Dictionary<string, object> dictAuth)
        {
            string queryStr = "update Authorization set VehicelType='" + dictAuth["VehicelType"] + "'" +
                                ", VehicelConfig='" + dictAuth["VehicelConfig"] + "'" +
                                ", VehicelStage='" + dictAuth["VehicelStage"] + "'" +
                                ", CreateTime='" + dictAuth["CreateTime"] + "'" +
                                ", Creater='" + dictAuth["Creater"] + "'" +
                                ", AuthorizeTo='" + dictAuth["AuthorizeTo"] + "'" +
                                ", AuthorizationTime='" + dictAuth["AuthorizationTime"] + "'" +
                                ", InvalidTime='" + dictAuth["InvalidTime"] + "'" +
                                ", Remark='" + dictAuth["Remark"] + "'" +
                                " where VehicelType='" + dictAuth["OldVehicelType"] + "'" +
                                " and VehicelConfig='" + dictAuth["OldVehicelConfig"] + "'" +
                                " and VehicelStage='" + dictAuth["OldVehicelStage"] + "'";
            return queryStr;
        }

        public static string Supplier_Update(Dictionary<string, object> dictSup)
        {
            string queryStr = "update Suppliers set Module='" + dictSup["Module"] + "'" +
                                ", Type='" + dictSup["Type"] + "'" +
                                ", Supplier='" + dictSup["Supplier"] + "'" +
                                ", Contact='" + dictSup["Contact"] + "'" +
                                ", Tel='" + dictSup["Tel"] + "'" +
                                " where Module='" + dictSup["OldModule"] + "'" +
                                "and Type='" + dictSup["OldType"] + "'" +
                                "and Supplier='" + dictSup["OldSupplier"] + "'";
            return queryStr;
        }

        public static string AddTask_Update(Dictionary<string, object> drKey)
        {
            var delStr = "update Task set TaskName='" + drKey["TaskName"] + "'" +
                                ", CANRoad='" + drKey["CANRoad"] + "'" +
                                ", Module='" + drKey["Module"] + "'" +
                                ", CreateTime='" + drKey["CreateTime"] + "'" +
                                ", Creater='" + drKey["Creater"] + "'" +
                                ", AuthTester='" + drKey["AuthTester"] + "'" +
                                ", AuthorizedFromDept='" + drKey["AuthorizedFromDept"] + "'" +
                                ", Supplier='" + drKey["Supplier"] + "'" +
                                ", AuthorizationTime='" + drKey["AuthorizationTime"] + "'" +
                                ", InvalidTime='" + drKey["InvalidTime"] + "'" +
                                ", Remark='" + drKey["Remark"] + "'" +
                                " where TaskNo='" + drKey["TaskNo"] + "'" +
                                " and TaskRound='" + drKey["TaskRound"] + "'" +
                                " and TaskName='" + drKey["OldTaskName"] + "'" +
                                " and CANRoad='" + drKey["OldCANRoad"] + "'";
            return delStr;
        }


        public static string DBCExample_Update(Dictionary<string, object> drKey)
        {
            var delStr = "update Task set SupplyPower='" + drKey["SupplyPower"] + "'" +
                                ", Oscilloscope='" + drKey["Oscilloscope"] + "'" +
                                ", Multimeter='" + drKey["Multimeter"] + "'" +
                                ", PNPower='" + drKey["PNPower"] + "'" +
                                ", IsPrototypeHidden='" + drKey["IsPrototypeHidden"] + "'" +
                                ", DBCMessage='" + drKey["DBCMessage"] + "'" +
                                ", isLinkDBC='" + drKey["isLinkDBC"] + "'" +
                                " where TaskNo='" + drKey["TaskNo"] + "'" +
                                "and TaskRound='" + drKey["TaskRound"] + "'" +
                                "and TaskName='" + drKey["TaskName"] + "'" +
                                "and CANRoad='" + drKey["CANRoad"] + "'";
            return delStr;
        }

        public static string Report_Update(Dictionary<string, object> drKey)
        {
            var delStr = "update Report set AutoReport='" + drKey["AutoReport"] + "'" +
                         ", TestTime='" + drKey["TestTime"] + "'" +
                         "where TaskNo='" + drKey["TaskNo"] + "'" +
                         "and TaskRound='" + drKey["TaskRound"] + "'" +
                         "and TaskName='" + drKey["TaskName"] + "'" +
                         "and CANRoad='" + drKey["CANRoad"] + "'" +
                         "and Moudle='" + drKey["Moudle"] + "'";
            return delStr; 
        }

        public static string Segment_Update(Dictionary<string, object> drKey)
        {
            var delStr = "update Segment set SegmentName='" + drKey["SegmentName"] + "'" +
                         ", Baud='" + drKey["Baud"] + "'" +
                         ", Correspond='" + drKey["Correspond"] + "'" +
                         " where SegmentName='" + drKey["OldSegmentName"] + "'" +
                         " and Baud='" + drKey["OldBaud"] + "'" + " and Correspond='" + drKey["OldCorrespond"] + "'";
            return delStr; 
        }
        public static string ExapChapter_Update(Dictionary<string, object> drKey)
        {
            var delStr = "update ExapChapter set ChapterName='" + drKey["ChapterName"] + "'" +
                        ", TestType='" + drKey["TestType"] + "'" +
                         "where ChapterName='" + drKey["OldChapterName"] + "'"+
                          "and TestType='" + drKey["OldTestType"] + "'"; ;
            return delStr;
        }
        public static string NodeConfigurationBox_Update(Dictionary<string, object> drKey)
        {
            var delStr = "update NodeConfigurationBox set Name='" + drKey["Name"] + "'" +
                         ", ID='" + drKey["ID"] + "'" + ", Count='" + drKey["Count"] + "'" +
                         "where Name='" + drKey["OldName"] + "'" +
                         "and ID='" + drKey["OldID"] + "'" +
                         "and Count='" + drKey["OldCount"] + "'";
            return delStr;
        }

        #endregion

        #region 无条件查询的SQL语句
        public static string DBC_Query()
        {
            const string queryStr = "select * from DBC";
            return queryStr;
        }

        public static string UPload_Query()
        {
            const string queryStr = "select * from UploadInfo";
            return queryStr;
        }
        public static string NodeConfigurationBox_Query()
        {
            const string queryStr = "select * from DBC";
            return queryStr;
        }

        public static string Sup_Query()
        {
            const string queryStr = "select * from Suppliers";
            return queryStr;
        }

        
        
        public static string Task_Query()
        {
            const string selStr = "select * from Task";
            return selStr;
        }

        
        /// <summary>
        /// 查询车辆授权
        /// </summary>
        /// <returns></returns>
        public static string VehicelAuth_Query()
        {
            const string queryStr = "select * from Authorization";
            return queryStr;
        }
        /// <summary>
        /// 查询FileLink表
        /// </summary>
        /// <returns></returns>
        public static string FileLink_Query()
        {
            const string queryStr = "select * from FileLinkByVehicel";
            return queryStr;
        }
        /// <summary>
        /// 车辆配置查询
        /// </summary>
        /// <returns></returns>
        public static string Config_Query()
        {
            const string queryStr = "select * from ConfigTemp";
            return queryStr;
        }
        /// <summary>
        /// 查询例子
        /// </summary>
        /// <returns></returns>
        public static string Example_Query()
        {
            const string queryStr = "select * from ExampleTemp";
            return queryStr;
        }
        /// <summary>
        /// 查询测试类型通过总线类型
        /// </summary>
        /// <returns></returns>
        public static string Example_QueryByBusType(Dictionary<string, object> dictDtc)
        {
            string queryStr = "select * from ExampleTemp where BusType='" + dictDtc["BusType"] + "'";
            return queryStr;
        }
        /// <summary>
        /// 工程文件表查询
        /// </summary>
        /// <returns></returns>
        public static string ProjectFile_Query()
        {
            const string queryStr = "select * from ProjectFiles";
            return queryStr;
        }

        /// <summary>
        /// 部门查询
        /// </summary>
        /// <returns></returns>
        public static string Department_Query()
        {
            const string queryStr = "select * from Department";
            return queryStr;
        }

        public static string FaultType_Query()
        {
            const string queryStr = "select * from FaultType";
            return queryStr;
        }
        /// <summary>
        /// 员工查询
        /// </summary>
        /// <returns></returns>
        public static string Employee_Query()
        {
            const string queryStr = "select * from Employee";
            return queryStr;
        }

        public static string ExapChapter_Query()
        {
            const string queryStr = "select * from ExapChapter";
            return queryStr;
        }
        public static string QuestionNote_Query()
        {
            const string queryStr = "select * from QuestionNote";
            return queryStr;
        }

        /// <summary>
        /// 供应商查询
        /// </summary>
        /// <returns></returns>
        public static string Supplier_Query()
        {
            const string queryStr = "select * from Suppliers";
            return queryStr;
        }
        /// <summary>
        /// 员工操作记录查询
        ///  </summary>
        /// <returns></returns>
        public static string OperationLogNo()
        {
            const string queryStr = "select * from OperationLog";
            return queryStr;
        }
        
        /// <summary>
        /// 查询通过车辆的型号配置阶段查询DBC
        /// </summary>
        /// <param name="dbc"></param>
        /// <returns></returns>
        
        public static string DBCFile_Query()
        {
            const string queryStr = "select * from DBC";
            return queryStr;
        }
        public static string Report_Query()
        {
            const string queryStr = "select * from Report";
            return queryStr;
        }

        public static string LoginLogNo()
        {
            const string queryStr = "select * from LoginLog";
            return queryStr;
        }

        public static string PassReportNote()
        {
            const string queryStr = "select * from PassReportNote";
            return queryStr;
        }
        
        public static string Segment()
        {
            const string queryStr = "select * from Segment";
            return queryStr;
        }
        public static string DBCByName(Dictionary<string, object> Segment)
        {
            string queryStr = "select * from DBC where BelongCAN='" + Segment["SegmentName"] + "'";
            return queryStr;
        }
        public static string Segment_Query(Dictionary<string, object> Segment)
        {
            string queryStr = "select * from Segment where SegmentName ='" + Segment["SegmentName"] + "'";
            return queryStr;
        }
        

        public static string NodeConfigurationBox()
        {
            const string queryStr = "select * from NodeConfigurationBox";
            return queryStr;
        }

        public static string QuestionNote()
        {
            const string queryStr = "select * from QuestionNote";
            return queryStr;
        }
        #endregion

        #region 带条件的查询SQL语句

        public static string DBCDataComparison_Query(Dictionary<string, object> keydict)
        {
            string sqlStr = "select * from DBC where VehicelType='" + keydict["VehicelType"] + "'" +
                            "and VehicelConfig='" + keydict["VehicelConfig"] + "'" +
                            "and VehicelStage='" + keydict["VehicelStage"] + "'";
            return sqlStr;
        }

        public static string DBCDataComparison(Dictionary<string, object> keydict)
        {
            string sqlStr = "select * from DBC where VehicelType='" + keydict["VehicelType"] + "'" +
                            "and VehicelConfig='" + keydict["VehicelConfig"] + "'" +
                            "and VehicelStage='" + keydict["VehicelStage"] + "'" +
                            "and BelongCAN='" + keydict["BelongCAN"] + "'";
            return sqlStr;
        }


        public static string QuestionNoteSort(Dictionary<string, object> keydict)
        {
            string sqlStr = "select * from QuestionNote where VehicelType='" + keydict["VehicelType"] + "'" +
                            "and VehicelConfig='" + keydict["VehicelConfig"] + "'" +
                            "and VehicelStage='" + keydict["VehicelStage"] + "'" +
                            "and TaskRound='" + keydict["TaskRound"] + "'" +
                            "and TestType='" + keydict["TestType"] + "'" +
                            "and Module='" + keydict["Module"] + "'" ;
            return sqlStr;
        }

        public static string PassReportNote(Dictionary<string, object> keydict)
        {
            string sqlStr = "select * from PassReportNote where VehicelType='" + keydict["VehicelType"] + "'" +
                            "and VehicelConfig='" + keydict["VehicelConfig"] + "'" +
                            "and VehicelStage='" + keydict["VehicelStage"] + "'" +
                            "and TaskRound='" + keydict["TaskRound"] + "'" +
                            "and TestType='" + keydict["TestType"] + "'" +
                            "and Module='" + keydict["Module"] + "'";
            return sqlStr;
        }
        public static string PassReportNoteByCondition_Query(Dictionary<string, object> keydict)
        {
            string sqlStr = "";
            if (keydict["Condition"].ToString() == "VehicelType")
            {
                sqlStr = "select * from PassReportNote where VehicelType='" + keydict["VehicelType"] + "'";
            }
            else if (keydict["Condition"].ToString() == "VehicelConfig")
            {
                sqlStr = "select * from PassReportNote where VehicelType='" + keydict["VehicelType"] + "'" +
                            "and VehicelConfig='" + keydict["VehicelConfig"] + "'";
            }
            else if (keydict["Condition"].ToString() == "VehicelStage")
            {
                sqlStr = "select * from PassReportNote where VehicelType='" + keydict["VehicelType"] + "'" +
                            "and VehicelConfig='" + keydict["VehicelConfig"] + "'" +
                            "and VehicelStage='" + keydict["VehicelStage"] + "'";
            }
            else if (keydict["Condition"].ToString() == "TaskRound")
            {
                sqlStr = "select * from PassReportNote where VehicelType='" + keydict["VehicelType"] + "'" +
                         "and VehicelConfig='" + keydict["VehicelConfig"] + "'" +
                         "and VehicelStage='" + keydict["VehicelStage"] + "'" +
                         "and TaskRound='" + keydict["TaskRound"] + "'";
            }
            else if (keydict["Condition"].ToString() == "TestType")
            {
                sqlStr = "select * from PassReportNote where VehicelType='" + keydict["VehicelType"] + "'" +
                         "and VehicelConfig='" + keydict["VehicelConfig"] + "'" +
                         "and VehicelStage='" + keydict["VehicelStage"] + "'" +
                         "and TaskRound='" + keydict["TaskRound"] + "'" +
                         "and TestType='" + keydict["TestType"] + "'";
            }
            return sqlStr;
        }
        public static string QuestionNoteByCondition_Query(Dictionary<string, object> keydict)
        {
            string sqlStr = "";
            if (keydict["Condition"].ToString() == "VehicelType")
            {
                sqlStr = "select * from QuestionNote where VehicelType='" + keydict["VehicelType"] + "'";
            }
            else if (keydict["Condition"].ToString() == "VehicelConfig")
            {
                sqlStr = "select * from QuestionNote where VehicelType='" + keydict["VehicelType"] + "'" +
                            "and VehicelConfig='" + keydict["VehicelConfig"] + "'";
            }
            else if (keydict["Condition"].ToString() == "VehicelStage")
            {
                sqlStr = "select * from QuestionNote where VehicelType='" + keydict["VehicelType"] + "'" +
                            "and VehicelConfig='" + keydict["VehicelConfig"] + "'" +
                            "and VehicelStage='" + keydict["VehicelStage"] + "'";
            }
            else if (keydict["Condition"].ToString() == "TaskRound")
            {
                sqlStr = "select * from QuestionNote where VehicelType='" + keydict["VehicelType"] + "'" +
                         "and VehicelConfig='" + keydict["VehicelConfig"] + "'" +
                         "and VehicelStage='" + keydict["VehicelStage"] + "'" +
                         "and TaskRound='" + keydict["TaskRound"] + "'";
            }
            else if (keydict["Condition"].ToString() == "TestType")
            {
                sqlStr = "select * from QuestionNote where VehicelType='" + keydict["VehicelType"] + "'" +
                         "and VehicelConfig='" + keydict["VehicelConfig"] + "'" +
                         "and VehicelStage='" + keydict["VehicelStage"] + "'" +
                         "and TaskRound='" + keydict["TaskRound"] + "'" +
                         "and TestType='" + keydict["TestType"] + "'";
            }
            else if (keydict["Condition"].ToString() == "Module")
            {
                sqlStr = "select * from QuestionNote where VehicelType='" + keydict["VehicelType"] + "'" +
                            "and VehicelConfig='" + keydict["VehicelConfig"] + "'" +
                            "and VehicelStage='" + keydict["VehicelStage"] + "'" +
                            "and TaskRound='" + keydict["TaskRound"] + "'" +
                            "and TestType='" + keydict["TestType"] + "'" +
                            "and Module='" + keydict["Module"] + "'";
            }

            return sqlStr;
        }
        public static string QuestionNote_Query(Dictionary<string, object> keydict)
        {
            string sqlStr = "select * from QuestionNote where AssessItem='" + keydict["AssessItem"] + "'"; 
            return sqlStr;
        }
        public static string DBC_Query(Dictionary<string, string> dbc)
        {
            var selStr = "select * from DBC where VehicelType='" + dbc["VehicelType"] + "'" +
                         "and VehicelConfig='" + dbc["VehicelConfig"] + "'" +
                         "and VehicelStage='" + dbc["VehicelStage"] + "'";
            return selStr;
        }

        public static string DBCCAN_Query(Dictionary<string, string> dbc)
        {
            var selStr = "select * from DBC where VehicelType='" + dbc["VehicelType"] + "'" +
                         "and VehicelConfig='" + dbc["VehicelConfig"] + "'" +
                         "and VehicelStage='" + dbc["VehicelStage"] + "'" +
                         "and BelongCAN='" + dbc["BelongCAN"] + "'";
            return selStr;
        }
        public static string Peportlnfo_Query()
        {
            string selStr = "select * from ReportInfo";
            return selStr;
        }

        public static string LastLoginUser_Query()
        {
            string selStr = "select * from LoginLog order by LoginNo desc limit 1";
            return selStr;
        }
        
        public static string NodeCfg_Query(Dictionary<string, object> nodeCfg)
        {
            var selStr = "select * from NodeConfigurationBox where Name='" + nodeCfg["Name"] + "'";
            return selStr;
        }
        public static string DTC_Query(Dictionary<string, object> dictDtc)
        {
            var selStr = "select * from Task where TaskNo='" + dictDtc["TaskNo"] + "'" +
                         " and TaskRound='" + dictDtc["TaskRound"] + "'" +
                         " and TaskName='" + dictDtc["TaskName"] + "'" +
                         " and CANRoad='" + dictDtc["CANRoad"] + "'" +
                         " and Module='" + dictDtc["Module"] + "'";
            return selStr;
        }

        public static string DBC_Query(Dictionary<string, object> dbc)
        {
            var selStr = "select * from DBC where VehicelType='" + dbc["VehicelType"] + "'"+
                         "and VehicelConfig='" + dbc["VehicelConfig"] + "'" +
                         "and VehicelStage='" + dbc["VehicelStage"] + "'" ; 
            return selStr;
        }
        public static string DBC_Query1(Dictionary<string, object> dbc)
        {
            var selStr = "";
            if (dbc["Type"].ToString().Equals("VehicelType"))
            {
                selStr = "select * from DBC where VehicelType='" + dbc["VehicelType"] + "'";
            }
            if (dbc["Type"].ToString().Equals("VehicelConfig"))
            {
                selStr = "select * from DBC where VehicelType='" + dbc["VehicelType"] + "'" +
                    "and VehicelConfig='" + dbc["VehicelConfig"] + "'";
            }
            if (dbc["Type"].ToString().Equals("VehicelStage"))
            {
                selStr = "select * from DBC where VehicelType='" + dbc["VehicelType"] + "'" +
                         "and VehicelConfig='" + dbc["VehicelConfig"] + "'" +
                         "and VehicelStage='" + dbc["VehicelStage"] + "'";
            }
            return selStr;
        }
        //通过车型查询所有该车型下所有的拓扑图
        public static string ALLToplogy_Query(Dictionary<string, object> dbc)
        {
            var selStr = "select * from Topology where VehicelType='" + dbc["VehicelType"] + "'";
            return selStr;
        }
        public static string OperationLogToTime(Dictionary<string, object> dictValue)
        {
            string queryStr = "select * from OperationLog where OperDate between '" + dictValue["TimeUp"] + "'" + " and '" + dictValue["TimeDown"] + "'";
            return queryStr;
        }
        public static string OperationLogToName(Dictionary<string, object> dictValue)
        {
            string queryStr = "select * from OperationLog where EmployeeName='" + dictValue["EmployeeName"] + "'";
            return queryStr;
        }

        public static string Task_GetSpecByQuery(Dictionary<string, string> dictTask)
        {
            var selStr = "select * from Task where TaskNo='" + dictTask["TaskNo"] + "'" +
                         " and TaskRound='" + dictTask["TaskRound"] + "'" +
                         " and TaskName='" + dictTask["TaskName"] + "'" +
                         " and CANRoad='" + dictTask["CANRoad"] + "'" +
                         " and Module='" + dictTask["Module"] + "'";

            return selStr;
        }

        public static string TaskModuleRepeat_Query(Dictionary<string, object> dictTask)
        {
            var selStr = "select * from Task where TaskNo='" + dictTask["TaskNo"] + "'" +
                        " and CANRoad='" + dictTask["CANRoad"] + "'" +
                        " and Module='" + dictTask["Module"] + "'";

            return selStr;
        }

        public static string FileLinkByVehicel_Query(Dictionary<string, string> file)
        {
            var selStr = "select * from FileLinkByVehicel where VehicelType='" + file["VehicelType"] + "'" +
                       " and VehicelConfig='" + file["VehicelConfig"] + "'" +
                       " and VehicelStage='" + file["VehicelStage"] + "'";
            return selStr;
        }

        public static string Topology_Query(Dictionary<string, object> file)
        {
            var selStr = "select * from Topology where VehicelType='" + file["VehicelType"] + "'" +
                         " and VehicelConfig='" + file["VehicelConfig"] + "'" +
                         " and VehicelStage='" + file["VehicelStage"] + "'";
            return selStr;
        }
        

        public static string Project_Query()
        {
            var selStr = "select * from ProJectFiles";

            return selStr;
        }

        public static string Exmp_SpecQuery(string matchCfg)
        {
            var selStr = "select * from ExampleTemp where MatchSort ='" + matchCfg + "'";
            return selStr;
        }

        public static string FileLink_SpecQuery(Dictionary<string, string> fileLink)
        {
            var selStr = "select * from FileLinkByVehicel " +
                         "where VehicelType='" + fileLink["VehicelType"] + "'" +
                         " and VehicelConfig='" + fileLink["VehicelConfig"] + "'" +
                         " and VehicelStage='" + fileLink["VehicelStage"] + "'" +
                         " and MatchSort ='" + fileLink["MatchSort"] + "'";
            return selStr;
        }

        public static string Cfg_SpecQuery(string cfgName)
        {
            var selStr = "select * from ConfigTemp where Name ='" + cfgName + "'";
            return selStr;
        }

        public static string ReportById_Query(string[] report)
        {
            var selStr = "select * from Report where TaskNo ='" + report[0] + "'" + " and " + "TaskName ='" + report[2] + "'" + " and " + "CANRoad ='" + report[3] + "'" + " and " + "Module ='" + report[4] + "'" + " and " + "TaskRound ='" + report[1] + "'" + " and " + "TestTime ='" + report[5] + "'";
            return selStr;
        }
        public static string ReportByText_Query(Dictionary<string, object> dr)
        {
            var selStr = "";
            if (dr["condition"].ToString() == "TaskNo")
            {
                selStr = "select * from Report where TaskNo ='" + dr["TaskNo"] + "'";  
                
            }
            else if (dr["condition"].ToString() == "TaskRound")
            {
                selStr = "select * from Report where TaskNo ='" + dr["TaskNo"] + "'" + "and" +
                                " TaskRound='" + dr["TaskRound"] + "'";

            }
            else if (dr["condition"].ToString() == "TaskName")
            {
                selStr = "select * from Report where TaskNo ='" + dr["TaskNo"] + "'" + "and" +
                                " TaskRound='" + dr["TaskRound"] + "'" + "and" +
                                " TaskName='" + dr["TaskName"] + "'";
                
            }
            else if (dr["condition"].ToString() == "CANRoad")
            {
                selStr = "select * from Report where TaskNo ='" + dr["TaskNo"] + "'" +
                    "and" + " TaskRound='" + dr["TaskRound"] + "'" +
                    "and" + " TaskName='" + dr["TaskName"] + "'" +
                    "and" + " CANRoad='" + dr["CANRoad"] + "'";
                
            }
            
            return selStr;
        }
        public static string Supplier_Get(Dictionary<string, object> dictDep)
        {
            string sql = "select * from Suppliers where Module='" + dictDep["Module"] + "'";
            return sql;
        }

        public static string LoginLogToTime(Dictionary<string, object> dictValue)
        {
            string queryStr = "select * from LoginLog where LoginDate between '" + dictValue["TimeUp"] + "'" + " and '" + dictValue["TimeDown"] + "'" + " or LoginOffDate between '" + dictValue["TimeUp"] + "'" + " and '" + dictValue["TimeDown"] + "'";
            return queryStr;
        }

        public static string LoginLogToName(Dictionary<string, object> dictValue)
        {
            string queryStr = "select * from LoginLog where EmployeeName='" + dictValue["EmployeeName"] + "'";
            return queryStr;
        }

        /// <summary>
        /// 根据任务条件查询DeviceStatus表
        /// </summary>
        /// <param name="dictValue"></param>
        /// <returns></returns>
        public static string TaskToDeviceStatus(Dictionary<string, object> dictValue)
        {
            string queryStr = "select * from DeviceStatus where TaskNo='" + dictValue["TaskNo"] + "'" +
                         " and TaskRound ='" + dictValue["TaskRound"] + "'" +
                         " and TaskName ='" + dictValue["TaskName"] + "'" +
                         " and CANRoad ='" + dictValue["CANRoad"] + "'" +
                         " and Module ='" + dictValue["Module"] + "'";
            return queryStr;
        }

        public static string GetExampleTemp(Dictionary<string, object> dictValue)
        {
            string queryStr = "select * from ExampleTemp where Name = '" + dictValue["Name"] + "'" ;
            return queryStr;
        }
        public static string GetExampleCon(Dictionary<string, object> dictValue)
        {
            string queryStr = "select * from ConfigTemp where Name = '" + dictValue["Name"] + "'";
            return queryStr;
        }

        public static string Exmp_MatchQuery(Dictionary<string, object> dictValue)
        {
            var selStr = "select * from ExampleTemp where MatchSort ='" + dictValue["MatchSort"] + "'";
            return selStr;
        }

        public static string GetConfigTempVer(Dictionary<string, object> dictValue)
        {
            string queryStr = "select * from ConfigTemp where Name = '" + dictValue["Name"] + "'" +
                "and Version = '" + dictValue["Version"] + "'";
            return queryStr;
        }
        public static string GetExampleTempVer(Dictionary<string, object> dictValue)
        {
            string queryStr = "select * from ExampleTemp where Name = '" + dictValue["Name"] + "'" +
                "and Version = '" + dictValue["Version"] + "'";
            return queryStr;
        }
        
        public static string ExapBoolToDeviceStatus(Dictionary<string, object> dictValue)
        {
            string queryStr = "select SupplyPower,Oscilloscope,Multimeter,PNPower,IsPrototypeHidden,DBCMessage,isLinkDBC from DeviceStatus where TaskNo='" + dictValue["TaskNo"] + "'" +
                         " and TaskRound ='" + dictValue["TaskRound"] + "'" +
                         " and TaskName ='" + dictValue["TaskName"] + "'" +
                         " and CANRoad ='" + dictValue["CANRoad"] + "'" +
                         " and Module ='" + dictValue["Module"] + "'";
            return queryStr;
        }

        public static string Report_MQuery(Dictionary<string, object> dictValue)
        {
            string queryStr = "select * from Report where TaskNo='" + dictValue["TaskNo"] + "'" +
                              " and TaskRound ='" + dictValue["TaskRound"] + "'" +
                              " and TaskName ='" + dictValue["TaskName"] + "'" +
                              " and CANRoad ='" + dictValue["CANRoad"] + "'" +
                              " and Module ='" + dictValue["Module"] + "'";
            return queryStr;
        }

        public static string ReportByTime_MQuery(Dictionary<string, object> dictValue)
        {
            string queryStr = "select * from Report where TaskNo='" + dictValue["TaskNo"] + "'" +
                              " and TaskRound ='" + dictValue["TaskRound"] + "'" +
                              " and TaskName ='" + dictValue["TaskName"] + "'" +
                              " and CANRoad ='" + dictValue["CANRoad"] + "'" +
                              " and Module ='" + dictValue["Module"] + "'" +
                              " and TestTime ='" + dictValue["TestTime"] + "'";
            return queryStr;
        }


        public static string Report_Query(Dictionary<string, object> dictValue)
        {
            string queryStr = "select * from Report where TaskNo='" + dictValue["TaskNo"] + "'" +
                      " and TaskRound ='" + dictValue["TaskRound"] + "'" +
                      " and TaskName ='" + dictValue["TaskName"] + "'" +
                      " and CANRoad ='" + dictValue["CANRoad"] + "'" +
                      " and Module ='" + dictValue["Module"] + "'" ;
            return queryStr;
        }

        #endregion

        #region 删除SQL语句
        //删除DBC
        public static string DBCFile_Del(Dictionary<string, object> dr)
        {
            string delStr = "delete from DBC where VehicelType='" + dr["VehicelType"] + "'" +
                         "and VehicelConfig='" + dr["VehicelConfig"] + "'" +
                         "and VehicelStage='" + dr["VehicelStage"] + "'" +
                         "and DBCName='" + dr["DBCName"] + "'" +
                         "and BelongCAN='" + dr["BelongCAN"] + "'";
            return delStr;
        }
        //删除拓扑图
        public static string TopologyFile_Del(Dictionary<string, object> dr)
        {
            string delStr = "delete from Topology where VehicelType='" + dr["VehicelType"] + "'" +
                         "and VehicelConfig='" + dr["VehicelConfig"] + "'" +
                         "and VehicelStage='" + dr["VehicelStage"] + "'" ;
            return delStr;
        }
        public static string FaultType_Del(Dictionary<string, object> dr)
        {
            string delStr = "delete from FaultType where ErrorType='" + dr["ErrorType"] + "'" +
                         "and IsMessage='" + dr["IsMessage"] + "'" +
                         "and MessageCount='" + dr["MessageCount"] + "'" +
                         "and MsgInformation='" + dr["MsgInformation"] + "'" +
                         "and CheckInfor='" + dr["CheckInfor"] + "'";
            return delStr;
        }
        public static string DBC_Del(DataRow _dr)
        {
            var delStr = "delete from DBC where VehicelType='" + _dr["VehicelType"] + "'" +
                         "and VehicelConfig='" + _dr["VehicelConfig"] + "'" +
                         "and VehicelStage='" + _dr["VehicelStage"] + "'" +
                         "and DBCName='" + _dr["DBCName"] + "'" +
                         "and BelongCAN='" + _dr["BelongCAN"] + "'";
            return delStr;
        }
        public static string DBCByVehicel_Del(Dictionary<string, object> dr)
        {
            var delStr = "delete from DBC where VehicelType='" + dr["VehicelType"] + "'" +
                         "and VehicelConfig='" + dr["VehicelConfig"] + "'" +
                         "and VehicelStage='" + dr["VehicelStage"] + "'" ;
            return delStr;
        }
        //查询节点所有信息通过车型
        public static string DBCQueryByCarModel(Dictionary<string, object> dr)
        {
            var delStr = "select * from DBC where VehicelType='" + dr["VehicelType"] + "'" +
                         "and VehicelConfig='" + dr["VehicelConfig"] + "'" +
                         "and VehicelStage='" + dr["VehicelStage"] + "'";
            return delStr;
        }
        public static string Sup_Del(DataRow _dr)
        {
            var delStr = "delete from Suppliers where Module='" + _dr["Module"] + "'" +
                         "and Type='" + _dr["Type"] + "'" +
                         "and Supplier='" + _dr["Supplier"] + "'";
            return delStr;
        }

        /// <summary>
        /// 车辆授权删除
        /// </summary>
        /// <param name="type">车辆类型</param>
        /// <param name="config">车辆配置</param>
        /// <param name="stage">车辆阶段</param>
        /// <returns></returns>
        public static string VehicelAuth_Del(string type, string config, string stage)
        {
            var delStr = "delete from Authorization where VehicelType = '" + type + "' and VehicelConfig = '" + config +
                            "' and VehicelStage = '" + stage + "'";
            return delStr;
        }
        /// <summary>
        /// 部门删除
        /// </summary>
        /// <param name="drKey">部门编号</param>
        /// <returns></returns>
        public static string Department_Del(Dictionary<string, object> drKey)
        {
            var delStr = "delete  from Department where Name = '" + drKey["Name"] + "'";
            return delStr;
        }
        
        public static string Authorization_Del(Dictionary<string, object> dr)
        {
            var delStr = "";
            if (dr["ifVehicel"].ToString() == "VehicelType")
            {
                delStr = "delete from Authorization where VehicelType='" + dr["VehicelType"] + "'";
                return delStr;
            }
            else if (dr["ifVehicel"].ToString() == "VehicelConfig")
            {
                delStr = "delete from Authorization where VehicelType='" + dr["VehicelType"] + "'" + "and" +
                                " VehicelConfig='" + dr["VehicelConfig"] + "'";
                return delStr;
            }
            delStr = "delete from Authorization where VehicelType='" + dr["VehicelType"] + "'" + "and" + 
                " VehicelConfig='" + dr["VehicelConfig"] + "'" + "and" + " VehicelStage='" + dr["VehicelStage"] + "'";
            return delStr;
        }

        public static string Supplier_Del(Dictionary<string, object> dr)
        {
            var delStr = "delete from Suppliers where Module='" + dr["Module"] + "'" + "and" +
                                " Type='" + dr["Type"] + "'" + "and" + " Supplier='" + dr["Supplier"] + "'";
            return delStr;
        }

        /// <summary>
        /// 员工删除
        /// </summary>
        /// <param name="drKey">员工编号</param>
        /// <returns></returns>
        public static string Empolyee_Del(Dictionary<string, object> drKey)
        {
            var delStr = "delete from Employee where ElyNo = '" + drKey["ElyNo"] + "'";
            return delStr;
        }
        public static string ExapChapter_Del(Dictionary<string, object> drKey)
        {
            var delStr = "delete from ExapChapter where ChapterName = '" + drKey["ChapterName"] + "'";
            return delStr;
        }
        /// <summary>
        /// Project数据表的删除
        /// </summary>
        /// <param name="dr">文件名称</param>
        /// <returns></returns>
        public static string ProjectFile_Del(Dictionary<string, object> dr)
        {
            string delStr = "delete from ProjectFiles where ProName='" + dr["ProName"] + "'" ;
            return delStr;
        }
        

        public static string QuestionNote_Del(Dictionary<string, object> dr)
        {
            string delStr = "delete from QuestionNote where VehicelType='" + dr["VehicelType"] + "'" +
                                    "and " + "VehicelConfig = '" + dr["VehicelConfig"] + "'" + 
                                    "and " + "VehicelStage ='" + dr["VehicelStage"] + "'" +
                                    "and " + "TaskRound ='" + dr["TaskRound"] + "'" +
                                    "and " + "TestType ='" + dr["TestType"] + "'" +
                                    "and " + "Module ='" + dr["Module"] + "'";
            return delStr;
        }
        public static string QuestionNoteByVehicel_Del(Dictionary<string, object> dr)
        {
            string delStr = "delete from QuestionNote where VehicelType='" + dr["VehicelType"] + "'" +
                                    "and " + "VehicelConfig = '" + dr["VehicelConfig"] + "'" +
                                    "and " + "VehicelStage ='" + dr["VehicelStage"] + "'" ;
            return delStr;
        }
        public static string QuestionNoteByTestType_Del(Dictionary<string, object> dr)
        {
            string delStr = "delete from QuestionNote where VehicelType='" + dr["VehicelType"] + "'" +
                                    "and " + "VehicelConfig = '" + dr["VehicelConfig"] + "'" +
                                    "and " + "VehicelStage ='" + dr["VehicelStage"] + "'" +
                                    "and " + "TestType ='" + dr["TestType"] + "'";
            return delStr;
        }
        public static string QuestionNoteByTask_Del(Dictionary<string, object> dr)
        {
            string delStr = "delete from QuestionNote where VehicelType='" + dr["VehicelType"] + "'" +
                                    "and " + "VehicelConfig = '" + dr["VehicelConfig"] + "'" +
                                    "and " + "VehicelStage ='" + dr["VehicelStage"] + "'" +
                                    "and " + "TaskRound = '" + dr["TaskRound"] + "'" +
                                    "and " + "TestType ='" + dr["TestType"] + "'" +
                                   "and " + "Module ='" + dr["Module"] + "'";
            return delStr;
        }
        public static string PassReportNoteByVehicel_Del(Dictionary<string, object> dr)
        {
            string delStr = "delete from PassReportNote where VehicelType='" + dr["VehicelType"] + "'" +
                                    "and " + "VehicelConfig = '" + dr["VehicelConfig"] + "'" +
                                    "and " + "VehicelStage ='" + dr["VehicelStage"] + "'";
            return delStr;
        }
        public static string PassReportNoteByTestType_Del(Dictionary<string, object> dr)
        {
            string delStr = "delete from PassReportNote where VehicelType='" + dr["VehicelType"] + "'" +
                                    "and " + "VehicelConfig = '" + dr["VehicelConfig"] + "'" +
                                    "and " + "VehicelStage ='" + dr["VehicelStage"] + "'" +
                                    "and " + "TestType ='" + dr["TestType"] + "'";
            return delStr;
        }
        public static string PassReportNoteByTask_Del(Dictionary<string, object> dr)
        {
            string delStr = "delete from PassReportNote where VehicelType='" + dr["VehicelType"] + "'" +
                                    "and " + "VehicelConfig = '" + dr["VehicelConfig"] + "'" +
                                    "and " + "VehicelStage ='" + dr["VehicelStage"] + "'" +
                                    "and " + "TaskRound = '" + dr["TaskRound"] + "'" +
                                    "and " + "TestType ='" + dr["TestType"] + "'" +
                                   "and " + "Module ='" + dr["Module"] + "'"; 
            return delStr;
        }
        public static string Segment_Del(Dictionary<string, object> dr)
        {
            string delStr = "delete from Segment where SegmentName='" + dr["SegmentName"] + "'" +
                                    "and " + "Baud = '" + dr["Baud"] + "'"+ " and " + "Correspond = '" + dr["Correspond"] + "'";
            return delStr;
        }

        public static string NodeConfigurationBox_Del(Dictionary<string, object> dr)
        {
            string delStr = "delete from NodeConfigurationBox where Name='" + dr["Name"] + "'" +
                            "and " + "ID = '" + dr["ID"] + "'" + "and " + "Count = '" + dr["Count"] + "'";
            return delStr;
        }

        public static string FileLink_Del(Dictionary<string, object> dr)
        {
            string delStr = "delete from FileLinkByVehicel where VehicelType='" + dr["VehicelType"] + "'" + 
                "and " + "VehicelConfig = '" + dr["VehicelConfig"] + "'" + "and " + "VehicelStage ='" + dr["VehicelStage"] + "'";
            return delStr;
        }
        public static string FileLinkBySort_Del(Dictionary<string, object> dr)
        {
            string delStr = "delete from FileLinkByVehicel where VehicelType='" + dr["VehicelType"] + "'" +
                "and " + "VehicelConfig = '" + dr["VehicelConfig"] + "'" + "and " + "VehicelStage ='" + dr["VehicelStage"] + "'" + "and " + "MatchSort ='" + dr["MatchSort"] + "'";
            return delStr;
        }
        public static string FileLinkByCfg_Del(Dictionary<string, object> dr)
        {
            string delStr = "delete from FileLinkByVehicel where VehicelType='" + dr["VehicelType"] + "'" +
                            "and " + "VehicelConfig = '" + dr["VehicelConfig"] + "'" + "and " + "VehicelStage ='" +
                            dr["VehicelStage"] + "'" + "and " + "MatchSort ='" + dr["MatchSort"] + "'" + "and " +
                            "CfgTemplateName ='" + dr["CfgTemplateName"] + "'";
            return delStr; 
        }
        public static string TaskDbc_Del(Dictionary<string, object> drKey)
        {
            string delStr;
            delStr = "delete from Task where TaskNo = '" + drKey["TaskNo"] + "'" ;
            return delStr;
        }
        public static string TaskDbcByTaskName_Del(Dictionary<string, object> drKey)
        {
            string delStr;
            delStr = "delete from Task where TaskNo = '" + drKey["TaskNo"] + "'" + "and " + "TaskName ='" + drKey["TaskName"] + "'";
            return delStr;
        }
        public static string TaskDbcByCANRoad_Del(Dictionary<string, object> drKey)
        {
            string delStr;
            delStr = "delete from Task where TaskNo = '" + drKey["TaskNo"] + "'" + "and " + "CANRoad ='" + drKey["CANRoad"] + "'";
            return delStr;
        }
        public static string Report_Del(Dictionary<string, object> drKey)
        {
            string delStr;
            delStr = "delete from Report where TaskNo = '" + drKey["TaskNo"] + "'";
            return delStr;
        }
        public static string ReportByTaskName_Del(Dictionary<string, object> drKey)
        {
            string delStr;
            delStr = "delete from Report where TaskNo = '" + drKey["TaskNo"] + "'" + "and" +
                                " TaskName='" + drKey["TaskName"] + "'";
            return delStr;
        }
        public static string ReportByTask_Del(Dictionary<string, object> drKey)
        {
            string delStr;
            delStr = "delete from Task where TaskNo = '" + drKey["TaskNo"] + "'" + "and" +
                                " TaskRound='" + drKey["TaskRound"] + "'" + "and" +
                                " TaskName='" + drKey["TaskName"] + "'" + "and" +
                                " CANRoad='" + drKey["CANRoad"] + "'" + "and" +
                                " Module='" + drKey["Module"] + "'";
            return delStr;
        }
        public static string Upload_Del(Dictionary<string, object> drKey)
        {
            string delStr;
            delStr = "delete from UploadInfo where IP = '" + drKey["IP"] + "'";
            return delStr;
        }
        public static string Task_Del(Dictionary<string, object> drKey)
        {
            var delStr = "";
            if (drKey["ifTask"].ToString() == "Round")
            {
                delStr = "delete from Task where TaskNo = '" + drKey["TaskNo"] + "'" + "and" +
                                " TaskRound='" + drKey["TaskRound"] + "'" ;
                return delStr;
            }
            else if (drKey["ifTask"].ToString() == "Task")
            {
                delStr = "delete from Task where TaskNo = '" + drKey["TaskNo"] + "'" + "and" +
                                " TaskRound='" + drKey["TaskRound"] + "'" + "and" +
                                " TaskName='" + drKey["TaskName"] + "'";
                return delStr;
            }
            else if (drKey["ifTask"].ToString() == "Node")
            {
                delStr = "delete from Task where TaskNo = '" + drKey["TaskNo"] + "'" + "and" +
                                " TaskRound='" + drKey["TaskRound"] + "'" + "and" +
                                " TaskName='" + drKey["TaskName"] + "'"  + "and" +
                                " Module='" + drKey["Module"] + "'";
                return delStr;
            }
            delStr = "delete from Task where TaskNo = '" + drKey["TaskNo"] + "'" + "and" +
                                " TaskRound='" + drKey["TaskRound"] + "'" + "and" +
                                " TaskName='" + drKey["TaskName"] + "'" + "and" +
                                " CANRoad='" + drKey["CANRoad"] + "'" + "and" +
                                " Module='" + drKey["Module"] + "'";
            return delStr;
        }
        #endregion

        #region 从数据库中获得数据的SQL语句
        public static string FileLinkByVehicel_Get(Dictionary<string, object> dictDep)
        {
            string sql = "select * from FileLinkByVehicel where VehicelType='" + dictDep["VehicelType"] + "'" +
                     "and VehicelConfig='" + dictDep["VehicelConfig"] + "'" +
                     "and VehicelStage='" + dictDep["VehicelStage"] + "'";
            return sql;
        }
        public static string FileLinkByVehicel_Double(Dictionary<string, object> dictDep)
        {
            string sql = "select * from FileLinkByVehicel where VehicelType='" + dictDep["VehicelType"] + "'" +
                     "and VehicelConfig='" + dictDep["VehicelConfig"] + "'" +
                     "and VehicelStage='" + dictDep["VehicelStage"] + "'" +
                     "and MatchSort='" + dictDep["MatchSort"] + "'";
            return sql;
        }
        public static string FileLinkByVehicel_GetMatch(Dictionary<string, string> dictDep)
        {
            string sql = "select * from FileLinkByVehicel where VehicelType='" + dictDep["VehicelType"] + "'" +
                     "and VehicelConfig='" + dictDep["VehicelConfig"] + "'" +
                     "and VehicelStage='" + dictDep["VehicelStage"] + "'";
            return sql;
        }
        
        
        public static string FileLinkByVehicel_Count(Dictionary<string, object> dr)
        {
            var delStr = "";
            if (dr["ifVehicel"].ToString() == "VehicelType")
            {
                delStr = "select * from FileLinkByVehicel where VehicelType='" + dr["VehicelType"] + "'";
                return delStr;
            }
            else if (dr["ifVehicel"].ToString() == "VehicelConfig")
            {
                delStr = "select * from FileLinkByVehicel where VehicelType='" + dr["VehicelType"] + "'" + "and" +
                                " VehicelConfig='" + dr["VehicelConfig"] + "'";
                return delStr;
            }
            delStr = "select * from FileLinkByVehicel where VehicelType='" + dr["VehicelType"] + "'" + "and" +
                " VehicelConfig='" + dr["VehicelConfig"] + "'" + "and" + " VehicelStage='" + dr["VehicelStage"] + "'";
            return delStr;
            
        }

        public static string FileLinkByVehicel_Report(Dictionary<string, object> dictDep)
        {
            string sql = "select * from FileLinkByVehicel where VehicelType='" + dictDep["VehicelType"] + "'" +
                     "and VehicelConfig='" + dictDep["VehicelConfig"] + "'" +
                     "and VehicelStage='" + dictDep["VehicelStage"] + "'" +
                     "and MatchSort='" + dictDep["MatchSort"] + "'";
            return sql;
        }

        public static string ElyRole_Get(Dictionary<string, object> dictDep)
        {
            string sql = "select * from Employee where ElyRole='" + dictDep["ElyRole"] + "'";
            return sql;
        }

       
        

        public static string Password_Get(Dictionary<string, object> dictDep)
        {
            string sql = "select * from Employee where ElyName='" + dictDep["ElyName"] + "'";
            return sql;
        }
        public static string Role_Get(Dictionary<string, object> dictDep)
        {
            string sql = "select * from Employee where ElyName='" + dictDep["ElyName"] + "'";
            return sql;
        }
        public static string Config_Get(Dictionary<string, object> dictDep)
        {
            string sql = "select * from Authorization where VehicelType='" + dictDep["VehicelType"] + "'"+
                "and VehicelConfig ='" + dictDep["VehicelConfig"] + "'" +
                "and VehicelStage = '" + dictDep["VehicelStage"] +"'";
            return sql;
        }
        public static string ContainExmp_Get(Dictionary<string, object> dictDep)
        {
            string sql = "select * from Task where TaskNo='" + dictDep["TaskNo"] + "'" +
                "and TaskRound ='" + dictDep["TaskRound"] + "'" +
                "and TaskName = '" + dictDep["TaskName"] + "'"+
                "and CANRoad = '"+dictDep["CANRoad"]+"'"+
                "and Module = '" + dictDep["Module"]+"'";
            return sql;
        }

        public static string Authorization_Table(Dictionary<string, object> dr)
        {
            var delStr = "";
            if (dr["level"].ToString() == "level1")
            {
                delStr = "select * from Authorization where VehicelType='" + dr["VehicelType"] + "'";
                return delStr;
            }
            else if (dr["level"].ToString() == "level2")
            {
                delStr = "select * from Authorization where VehicelType='" + dr["VehicelType"] + "'" + "and" +
                                " VehicelConfig='" + dr["VehicelConfig"] + "'";
                return delStr;
            }
            delStr = "select * from Authorization where VehicelType='" + dr["VehicelType"] + "'" + "and" +
                " VehicelConfig='" + dr["VehicelConfig"] + "'" + "and" + " VehicelStage='" + dr["VehicelStage"] + "'";
            return delStr;
        }

        public static string TaskTable_Query(Dictionary<string, object> dr)
        {
            var delStr = "";
            if (dr["level"].ToString() == "level1")
            {
                delStr = "select * from Task where TaskNo like '" + dr["VehicelType"] + "%" + "'";
                return delStr;
            }
            else if (dr["level"].ToString() == "level2")
            {
                delStr = "select * from Task where TaskNo like '" + dr["VehicelType"] + "%" + "'";
                return delStr;
            }
            else if (dr["level"].ToString() == "level3")
            {
                delStr = "select * from Task where TaskNo='" + dr["TaskNo"] + "'"; 
                return delStr;
            }
            else if (dr["level"].ToString() == "level4")
            {
                delStr = "select * from Task where TaskNo='" + dr["TaskNo"] + "'" + "and" +
                " TaskRound='" + dr["TaskRound"] + "'";
                return delStr;
            }
            else if (dr["level"].ToString() == "level5")
            {
                delStr = "select * from Task where TaskNo='" + dr["TaskNo"] + "'" + "and" +
                " TaskRound='" + dr["TaskRound"] + "'" + "and" + " TaskName='" + dr["TaskName"] + "'";
                return delStr;
            }
            delStr = "select * from Task where TaskNo ='" + dr["TaskNo"] + "'" + "and" +
                " TaskRound='" + dr["TaskRound"] + "'" + "and" + " TaskName='" + dr["TaskName"] + "'" + "and" + " Module ='" + dr["Module"] + "'";
            return delStr;

        }

        #endregion

        #region 计数查询SQL语句
        public static string DBC_Count(Dictionary<string, object> keyname)
        {
            var sqlStr = "select * from DBC where VehicelType='" + keyname["VehicelType"] + "'" +
                         "and VehicelConfig='" + keyname["VehicelConfig"] + "'" +
                         "and VehicelStage='" + keyname["VehicelStage"] + "'" +
                         "and DBCName='" + keyname["DBCName"] + "'" +
                         "and BelongCAN='" + keyname["BelongCAN"] + "'";
            return sqlStr;
        }
        public static string Mail_Count(Dictionary<string, object> drKey)
        {
            var Str = "select * from Employee where Mail='" + drKey["Mail"] + "'";
            return Str;
        }
        #endregion

        /// <summary>
        /// 根据配件查询供应商信息
        /// </summary>
        /// <param name="dictDep"></param>
        /// <returns></returns>

        public static string LoginOffDate_Update(Dictionary<string, object> dictSup)
        {
            string queryStr = "update LoginLog set LoginOffDate='" + dictSup["LoginOffDate"] + "'" + " where LoginNo='" + dictSup["LoginNo"] + "'";
            return queryStr;
        }

        public static string ExampleTemp_Update(Dictionary<string, object> key)
        {
            string queryStr = "update ExampleTemp set Content='" + key["Content"] + "'" + " where Name='" + key["Name"] + "'" +
                "and Version='" + key["Version"] + "'";
            return queryStr;
        }
        public static string ConfigTemp_Update(Dictionary<string, object> key)
        {
            string queryStr = "update ConfigTemp set Content='" + key["Content"] + "'" + " where Name='" + key["Name"] + "'" +
                "and Version='" + key["Version"] + "'";
            return queryStr;
        }

        /// <summary>
        /// 修改表ExampleTemp：根据Name列修改EmlTemplate列
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ExampleTempTemplate_Update(Dictionary<string, object> key)
        {
            string queryStr = "update ExampleTemp set EmlTemplate='" + key["EmlTemplate"] + "'" + " where Name='" + key["Name"] + "'" +
                              "and Version='" + key["Version"] + "'";
            return queryStr;
        }

        public static string Error_Update(Dictionary<string, object> report)
        {
            string queryStr = "update Report set ErrorInfo='" + report["ErrorInfo"] + "'" + 
               " where TaskNo='" + report["TaskNo"] + "'" +
               " and TaskRound='" + report["TaskRound"] + "'" +
               " and TaskName='" + report["TaskName"] + "'" +
               " and CANRoad='" + report["CANRoad"] + "'" +
               " and Module='" + report["Module"] + "'"+
               " and TestTime='" + report["TestTime"] + "'";
            return queryStr;
        }
    }
}