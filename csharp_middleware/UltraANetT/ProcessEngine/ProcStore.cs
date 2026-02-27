using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBEngine;
using Model;
using System.Data;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Metadata.W3cXsd2001;


namespace ProcessEngine
{
    public class ProcStore
    {
        #region 添加操作
        public bool AddConfigTemp(Dictionary<string, object> dictConfig, out string error)
        {
            var config = new ConfigTemp
            {
                Name = dictConfig["Name"].ToString(),
                Version = dictConfig["Version"].ToString(),
                Content = dictConfig["Content"].ToString(),
                MatchSort = dictConfig["MatchSort"].ToString(),
                ImportDate = DateTime.Parse(dictConfig["ImportDate"].ToString())
            };
            return BaseSqlOrder.Add(config, out error);

        }

        public bool AddDepartment(Dictionary<string, object> dictDept, out string error)
        {
            var dept = new Department
            { 
                Name = dictDept["Name"].ToString(),
                Master = dictDept["Master"].ToString(),
                NumForDept = int.Parse(dictDept["NumForDept"].ToString()),
                Remark = dictDept["Remark"].ToString(),
            };
            return BaseSqlOrder.Add(dept, out error);
        }

        public bool AddSegment(Dictionary<string, object> dictSegment, out string error)
        {
            var Seg = new Segment
            {
                SegmentName = dictSegment["SegmentName"].ToString(),
                Baud = dictSegment["Baud"].ToString(),
                Correspond = dictSegment["Correspond"].ToString()
            };
            return BaseSqlOrder.Add(Seg, out error);
        }
        public bool AddExapChapter(Dictionary<string, object> dictExapChapter, out string error)
        {
            var Seg = new ExapChapter
            {
                ChapterName = dictExapChapter["ChapterName"].ToString(),
                TestType = dictExapChapter["TestType"].ToString(),
            };
            return BaseSqlOrder.Add(Seg, out error);
        }
        public bool AddNodeConfigurationBox(Dictionary<string, object> dictSegment, out string error)
        {
            var Seg = new NodeConfigurationBox
            {
                Name = dictSegment["Name"].ToString(),
                ID = dictSegment["ID"].ToString(),
                Count= int.Parse(dictSegment["Count"].ToString()),
            };
            return BaseSqlOrder.Add(Seg, out error);
        }

        public bool AddSuppliers(Dictionary<string, object> dictSup, out string error)
        {
            var sup = new Suppliers();
            {
                sup.Module = dictSup["Module"].ToString();
                sup.Type = dictSup["Type"].ToString();
                sup.Supplier = dictSup["Supplier"].ToString();
                sup.Contact = dictSup["Contact"].ToString();
                sup.Tel = dictSup["Tel"].ToString();
            };
            return BaseSqlOrder.Add(sup, out error);
        }

        public bool AddProject(Dictionary<string, object> dictPro, out string error)
        {
            var pro = new ProjectFiles()
            {
                ProName = dictPro["ProName"].ToString(),
                Content = dictPro["Content"] as byte[],
                UploadUser = dictPro["UploadUser"].ToString(),
                UploadDate = DateTime.Parse(dictPro["UploadDate"].ToString()) 
            };
            return BaseSqlOrder.Add(pro, out error);
        }

        public bool AddUpload(Dictionary<string, object> dictUpload, out string error)
        {
            var pro = new UploadInfo()
            {
                IP = dictUpload["IP"].ToString(),
                Port = dictUpload["Port"].ToString(),
                User = dictUpload["User"].ToString(),
                Password = dictUpload["Password"].ToString(),
                UploadPath = dictUpload["UploadPath"].ToString()
            };
            return BaseSqlOrder.Add(pro, out error);
        }

        public bool AddExampleTemp(Dictionary<string, object> dictExam, out string error)
        {
            var example = new ExampleTemp();
            {
                example.Name = dictExam["Name"].ToString();
                example.Version = dictExam["Version"].ToString();
                example.Content = dictExam["Content"].ToString();
                example.MatchSort = dictExam["MatchSort"].ToString(); ;
                example.ImportDate = DateTime.Parse(dictExam["ImportDate"].ToString());
            };
            return BaseSqlOrder.Add(example, out error);
        }

        public bool AddDBC(Dictionary<string, object> dictDbc, out string error)
        {
            var dbc = new DBC();
            {
                dbc.VehicelType = dictDbc["VehicelType"].ToString();
                dbc.VehicelStage = dictDbc["VehicelStage"].ToString();
                dbc.VehicelConfig = dictDbc["VehicelConfig"].ToString();
                dbc.DBCName = dictDbc["DBCName"].ToString();
                dbc.BelongCAN = dictDbc["BelongCAN"].ToString();
                dbc.DBCContent = dictDbc["DBCContent"].ToString();
                dbc.ImportUser = dictDbc["ImportUser"].ToString();
                dbc.FormerDBCName= dictDbc["FormerDBCName"].ToString();
                dbc.CANType = dictDbc["CANType"].ToString();
                dbc.ImportTime = DateTime.Parse(dictDbc["ImportTime"].ToString());
            };
            return BaseSqlOrder.Add(dbc, out error);
        }
        //更改DBC信息
        public bool UpdateDBC(EnumLibrary.EnumTable enumTable, Dictionary<string, object> dict, out string error)
        {
            //修改前的其他操作
            return BaseSqlOrder.Update(GetUpdateStrByEnum(enumTable, dict), out error);
        }
        public bool AddFaultType(Dictionary<string, object> dictDbc, out string error)
        {
            var ftp = new FaultType();
            {
                ftp.ErrorType = dictDbc["ErrorType"].ToString();
                ftp.IsMessage = dictDbc["IsMessage"].ToString();
                ftp.MessageCount = dictDbc["MessageCount"].ToString();
                ftp.MsgInformation = dictDbc["MsgInformation"].ToString();
                ftp.CheckInfor = dictDbc["CheckInfor"].ToString();
                
            };
            return BaseSqlOrder.Add(ftp, out error);
        }
        public bool AddAuthorization(Dictionary<string, object> dictAuth, out string error)
        {
            var auth = new Authorization();
            {
                auth.VehicelType = dictAuth["VehicelType"].ToString();
                auth.VehicelConfig = dictAuth["VehicelConfig"].ToString();
                auth.VehicelStage = dictAuth["VehicelStage"].ToString();
                auth.CreateTime = DateTime.Parse(dictAuth["CreateTime"].ToString());
                auth.Creater = dictAuth["Creater"].ToString();
                auth.AuthorizeTo = dictAuth["AuthorizeTo"].ToString();
                //auth.AuthorizedDept = dictAuth["FromDepartment"].ToString();
                auth.AuthorizationTime = DateTime.Parse(dictAuth["AuthorizationTime"].ToString());
                auth.InvalidTime = DateTime.Parse(dictAuth["InvalidTime"].ToString());
                auth.Remark = dictAuth["Remark"].ToString();
            };
            return BaseSqlOrder.Add(auth, out error);
        }

        public bool AddEmployee(Dictionary<string, object> dictEly, out string error)
        {
            var ely = new Employee();
            {
                ely.ElyNo = dictEly["ElyNo"].ToString();
                ely.ElyName = dictEly["ElyName"].ToString();
                ely.ElyRole = dictEly["ElyRole"].ToString();
                ely.Sex = dictEly["Sex"].ToString();
                ely.Contact = dictEly["Contact"].ToString();
                ely.Department = dictEly["Department"].ToString();
                ely.Mail = dictEly["Mail"].ToString();
                ely.Password = dictEly["Password"].ToString();
                ely.Remark = dictEly["Remark"].ToString();
            };
            return BaseSqlOrder.Add(ely, out error);
        }

        public bool AddFileLinkByVehicel(Dictionary<string, object> dictFile, out string error)
        {
            var file = new FileLinkByVehicel();
            {
                file.VehicelType = dictFile["VehicelType"].ToString();
                file.VehicelStage = dictFile["VehicelStage"].ToString();
                file.VehicelConfig = dictFile["VehicelConfig"].ToString();
                file.MatchSort = dictFile["MatchSort"].ToString();
                file.Topology ="";
                file.CfgTemplateName = dictFile["CfgTemplateName"].ToString(); 
                file.CfgTemplate = dictFile["CfgTemplate"].ToString();
                file.CfgTemplateJson = dictFile["CfgTemplateJson"].ToString();
                file.EmlTemplateName = dictFile["EmlTemplateName"].ToString();

                //file.EmlTemplate = dictFile["CfgTemplete"] as byte[];
               // file.EmlTemplateJson = dictFile["CfgTempleteJson"].ToString();

                //file.EmlTemplate = dictFile["CfgTemplete"] as byte[];

                file.EmlTemplate = dictFile["EmlTemplate"].ToString();

                file.CfgBaudJson = dictFile["CfgBaudJson"].ToString();
                file.TplyDescrible = dictFile["TplyDescrible"].ToString();
                file.ConTableColEdit = dictFile["ConTableColEdit"].ToString();
                file.EmlTableColEdit = dictFile["EmlTableColEdit"].ToString();
            };
            return BaseSqlOrder.Add(file, out error);
        }
        public bool AddPasteFileLinkByVehicel(Dictionary<string, object> dictFile, out string error)
        {
            var file = new FileLinkByVehicel();
            {
                file.VehicelType = dictFile["VehicelType"].ToString();
                file.VehicelStage = dictFile["VehicelStage"].ToString();
                file.VehicelConfig = dictFile["VehicelConfig"].ToString();
                file.MatchSort = dictFile["MatchSort"].ToString();
                file.Topology = dictFile["Topology"].ToString(); 
                file.CfgTemplateName = dictFile["CfgTemplateName"].ToString();
                file.CfgTemplate = dictFile["CfgTemplate"].ToString();
                file.CfgTemplateJson = dictFile["CfgTemplateJson"].ToString();
                file.EmlTemplateName = dictFile["EmlTemplateName"].ToString();

                //file.EmlTemplate = dictFile["CfgTemplete"] as byte[];
                // file.EmlTemplateJson = dictFile["CfgTempleteJson"].ToString();

                //file.EmlTemplate = dictFile["CfgTemplete"] as byte[];

                file.EmlTemplate = dictFile["EmlTemplate"].ToString();

                file.CfgBaudJson = dictFile["CfgBaudJson"].ToString();
                file.TplyDescrible = dictFile["TplyDescrible"].ToString();
                file.ConTableColEdit = dictFile["ConTableColEdit"].ToString();
                file.EmlTableColEdit = dictFile["EmlTableColEdit"].ToString();
            };
            return BaseSqlOrder.Add(file, out error);
        }
        public bool AddTopology(Dictionary<string, object> dictFile, out string error)
        {
            var file = new Topology();
            {
                file.VehicelType = dictFile["VehicelType"].ToString();
                file.VehicelStage = dictFile["VehicelStage"].ToString();
                file.VehicelConfig = dictFile["VehicelConfig"].ToString();
                file.Tply = dictFile["Tply"].ToString(); 
                file.TplyDescrible = dictFile["TplyDescrible"].ToString();
                file.ImportUser = dictFile["ImportUser"].ToString();
                file.ImportTime = (dictFile["ImportTime"].ToString());
            };
            return BaseSqlOrder.Add(file, out error);
        }

        public bool AddQuestionNote(Dictionary<string, object> dictFile, out string error)
        {
            var file = new QuestionNote();
            {
                file.VehicelType = dictFile["VehicelType"].ToString();
                file.VehicelStage = dictFile["VehicelStage"].ToString();
                file.VehicelConfig = dictFile["VehicelConfig"].ToString();
                file.TaskRound= dictFile["TaskRound"].ToString();
                file.TestType = dictFile["TestType"].ToString(); ;
                file.Module = dictFile["Module"].ToString();
                file.FailItemInfo = dictFile["FailItemInfo"].ToString();
                //file.ExapID = dictFile["ExapID"].ToString();
                //file.ExapName = dictFile["ExapName"].ToString();
                //file.AssessItem= dictFile["AssessItem"].ToString();
                //file.DescriptionOfValue = dictFile["DescriptionOfValue"].ToString();
                //file.MinValue = dictFile["MinValue"].ToString();
                //file.MaxValue = dictFile["MaxValue"].ToString();
                //file.NormalValue = dictFile["NormalValue"].ToString();
                //file.TestValue = dictFile["TestValue"].ToString();
                //file.Result = dictFile["Result"].ToString();
                file.TestTime = DateTime.Parse(dictFile["TestTime"].ToString());

            };
            return BaseSqlOrder.Add(file, out error);
        }

        public bool AddPassReportNote(Dictionary<string, object> dictFile, out string error)
        {
            var file = new PassReportNote();
            {
                file.VehicelType = dictFile["VehicelType"].ToString();
                file.VehicelStage = dictFile["VehicelStage"].ToString();
                file.VehicelConfig = dictFile["VehicelConfig"].ToString();
                file.TaskRound = dictFile["TaskRound"].ToString();
                file.TestType = dictFile["TestType"].ToString(); 
                file.Module = dictFile["Module"].ToString();
                file.FailItemInfo = dictFile["FailItemInfo"].ToString();
                //file.ExapID = dictFile["ExapID"].ToString();
                //file.ExapName = dictFile["ExapName"].ToString();
                //file.AssessItem = dictFile["AssessItem"].ToString();
                //file.DescriptionOfValue = dictFile["DescriptionOfValue"].ToString();
                //file.MinValue = dictFile["MinValue"].ToString();
                //file.MaxValue = dictFile["MaxValue"].ToString();
                //file.NormalValue = dictFile["NormalValue"].ToString();
                //file.TestValue = dictFile["TestValue"].ToString();
                //file.Result = dictFile["Result"].ToString();
                file.TestTime = DateTime.Parse(dictFile["TestTime"].ToString());

            };
            return BaseSqlOrder.Add(file, out error);
        }


        public bool AddTask(Dictionary<string, object> dictTask, out string error)
        {
            var file = new Task();
            {
                file.TaskNo = dictTask["TaskNo"].ToString();
                file.TaskRound = dictTask["TaskRound"].ToString();
                file.TaskName = dictTask["TaskName"].ToString();
                file.CANRoad = dictTask["CANRoad"].ToString();
                file.Module = dictTask["Module"].ToString();
                file.CreateTime = DateTime.Parse(dictTask["CreateTime"].ToString());
                file.Creater = dictTask["Creater"].ToString();
                file.AuthTester = dictTask["AuthTester"].ToString();
                file.AuthorizedFromDept = dictTask["AuthorizedFromDept"].ToString();
                file.Supplier = dictTask["Supplier"].ToString();
                file.TestType = dictTask["TestType"].ToString();
                file.ContainExmp = dictTask["ContainExmp"].ToString();
                file.AuthorizationTime = DateTime.Parse(dictTask["AuthorizationTime"].ToString());
                file.InvalidTime = DateTime.Parse(dictTask["InvalidTime"].ToString());
                file.Remark = dictTask["Remark"].ToString();
            };
            return BaseSqlOrder.Add(file, out error);
        }

        public bool AddOperationLog(Dictionary<string, object> dictLog, out string error)
        {
            var operation = new Model.OperationLog();
            {
                operation.OperNo = dictLog["OperNo"].ToString();
                operation.OperDate = DateTime.Now;
                operation.EmployeeNo = dictLog["EmployeeNo"].ToString();
                operation.EmployeeName = dictLog["EmployeeName"].ToString();
                operation.OperRecord = dictLog["OperRecord"].ToString();
                operation.OperTable = dictLog["OperTable"].ToString();
            };
            return BaseSqlOrder.Add(operation, out error);
        }

        public bool AddReport(Dictionary<string, object> dictReport, out string error)
        {
            var report = new Report();
            report.TaskNo = dictReport["TaskNo"].ToString();
            report.TaskRound = dictReport["TaskRound"].ToString();
            report.TaskName = dictReport["TaskName"].ToString();
            report.CANRoad = dictReport["CANRoad"].ToString();
            report.Module = dictReport["Module"].ToString();
            report.TestTime = dictReport["TestTime"].ToString();
            report.ManualReport = dictReport["ManualReport"].ToString();
            report.AutoReport = dictReport["AutoReport"].ToString();
            report.TestUser = dictReport["TestUser"].ToString();
            report.Remark = dictReport["Remark"].ToString();
            report.ErrorInfo = dictReport["ErrorInfo"].ToString();
            return BaseSqlOrder.Add(report, out error);
        }

        public bool AddExcelReport(Dictionary<string, object> dictReport, out string error)
        {
            var reportInfo = new ReportInfo();
            reportInfo.TaskNo = dictReport["TaskNo"].ToString();
            reportInfo.TaskRound = dictReport["TaskRound"].ToString();
            reportInfo.TaskName = dictReport["TaskName"].ToString();
            reportInfo.CANRoad = dictReport["CANRoad"].ToString();
            reportInfo.Module = dictReport["Module"].ToString();
            reportInfo.TestTime = DateTime.Parse(dictReport["TestTime"].ToString());
            reportInfo.ReportTestList = dictReport["ReportTestList"].ToString();
            reportInfo.ReportCoverInfo = dictReport["ReportCoverInfo"].ToString();
            reportInfo.ReportMainInfo = dictReport["ReportMainInfo"].ToString();
            reportInfo.ReportPathInfo = dictReport["ReportPathInfo"].ToString();
            reportInfo.TestUser = dictReport["TestUser"].ToString();
            reportInfo.Remark = dictReport["Remark"].ToString();
            reportInfo.ErrorInfo = dictReport["ErrorInfo"].ToString();
            return BaseSqlOrder.Add(reportInfo, out error);
        }
        #endregion

        #region 修改操作
        public bool Update(EnumLibrary.EnumTable enumTable, Dictionary<string, object> dict, out string error)
        {
            //修改前的其他操作
            return BaseSqlOrder.Update(GetUpdateStrByEnum(enumTable, dict), out error);
        }
        #endregion

        #region  删除操作
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="enumTable">指定表</param>
        /// <param name="keyName">指定行</param>
        /// <param name="error">异常信息</param>
        /// <returns></returns>
        public bool Del(EnumLibrary.EnumTable enumTable, Dictionary<string,object> drKey, out string error)
        {
            //删除前的其他操作
            return BaseSqlOrder.Del(GetDelStrByEnum(enumTable, drKey), out error);
        }
        #endregion

        #region 查询操作[无条件]
        public IList<object[]> GetRegularByEnum(EnumLibrary.EnumTable enumTable)
        {
            return BaseSqlOrder.GetMultipleByQuery(GetQueryByEnum(enumTable));
        }
        #endregion

        #region 查询章节名称表
        public IList<object> GetChapterNameByEnum(EnumLibrary.EnumTable enumTable)
        {
            return BaseSqlOrder.GetExapChapterByQuery(GetQueryByEnum(enumTable));
        }
        #endregion

        #region 查询操作[有条件]
        public IList<object[]> GetSpecialByEnum(EnumLibrary.EnumTable enumTable, Dictionary<string, object> dictEmpt)
        {
            return BaseSqlOrder.GetMultipleByQuery(GetQueryByEnum(enumTable, dictEmpt));
        }
        #endregion

        #region 计数操作
        public bool Count(EnumLibrary.EnumTable enumTable, Dictionary<string, object> drKey)
        {
            //计数前的其他操作
            return BaseSqlOrder.IsExist(GetCountStrByEnum(enumTable, drKey));
        }
        #endregion

        #region 其他操作
        public IList<object[]> GetDBCListByVNode(List<string> currentVehicel)
        {
            var dictDBC = new Dictionary<string, string>
            {
                {"VehicelType", currentVehicel[0]},
                {"VehicelConfig", currentVehicel[1]},
                {"VehicelStage", currentVehicel[2]},
            };
            return BaseSqlOrder.GetMultipleByQuery(ExcuteSqlCase.DBC_Query(dictDBC));
        }


        public IList<object[]> GetDBCListByVNodeAndCAN(List<string> currentVehicel)
        {
            var dictDBC = new Dictionary<string, string>
            {
                {"VehicelType", currentVehicel[0]},
                {"VehicelConfig", currentVehicel[1]},
                {"VehicelStage", currentVehicel[2]},
                {"BelongCAN", currentVehicel[3]},
            };
            return BaseSqlOrder.GetMultipleByQuery(ExcuteSqlCase.DBCCAN_Query(dictDBC));
        }

        public byte[] GetConfigBytesByName(string configName)
        {
            IList<object> config = BaseSqlOrder.GetMultipleByQuery(ExcuteSqlCase.Cfg_SpecQuery(configName))[0];
            byte[] configContent = config[2] as byte[];
            return configContent;
        }

        public byte[] GetExampleBytesByName(string exampleName)
        {
            IList<object> example = BaseSqlOrder.GetMultipleByQuery(ExcuteSqlCase.Exmp_SpecQuery(exampleName))[0];
            byte[] exampleContent = example[2] as byte[];
            return exampleContent;
        }

       public IList<object> GetTaskByName(Dictionary<string, string> dictTask)
        {
            return BaseSqlOrder.GetMultipleByQuery(ExcuteSqlCase.Task_GetSpecByQuery(dictTask))[0];
        }

        public Dictionary<string, string> GetConfigJsonByVNode(List<string> currentVehicel)
        {
            var dictConfig = new Dictionary<string, string>
            {
                {"VehicelType", currentVehicel[0]},
                {"VehicelConfig", currentVehicel[1]},
                {"VehicelStage", currentVehicel[2]},
                
            };

            IList<object> configObjectses = BaseSqlOrder.GetMultipleByQuery(ExcuteSqlCase.FileLinkByVehicel_Query(dictConfig))[0];
            string jsonProperty = configObjectses[7].ToString();
            string jsonBaud = configObjectses[8].ToString();
            var dictJson = new Dictionary<string, string>
            {
                {"Baud", jsonBaud},
                {"Property", jsonProperty},
            };
            return dictJson;
        }

        public string GetExmpJsonByName(Dictionary<string, string> drEml)
        {
            return BaseSqlOrder.GetMultipleByQuery(ExcuteSqlCase.Task_GetSpecByQuery(drEml))[0][11].ToString();
        }

        public IList<string> GetSingnalCol(EnumLibrary.EnumTable enumTable, int colIndex)
        {
            return BaseSqlOrder.GetSignalColByQuery(GetQueryByEnum(enumTable), colIndex); 
        }
        public IList<string> GetSingnalColByName(EnumLibrary.EnumTable enumTable,Dictionary<string, object> dict, int colIndex)
        {
            return BaseSqlOrder.GetSignalColByQuery(GetQueryByEnum(enumTable, dict), colIndex);//根据SegmentName查询DBC BelongCAN
        }
        public IList<string> GetSingnalColByCon(EnumLibrary.EnumTable enumTable, Dictionary<string, object> dictEmpt, int colIndex)
        {
            return BaseSqlOrder.GetSignalColByQuery(GetQueryByEnum(enumTable, dictEmpt), colIndex);
        }
        public IList<object> GetFileByName(Dictionary<string, string> dictFileLink)
        {
            return BaseSqlOrder.GetMultipleByQuery(ExcuteSqlCase.FileLink_SpecQuery(dictFileLink))[0];
        }

        public IList<object> GetProject()
        {
            return BaseSqlOrder.GetMultipleByQuery(ExcuteSqlCase.Project_Query())[0];
        }

        public IList<object> GetReportById(string[] reportId)
        {
            return BaseSqlOrder.GetMultipleByQuery(ExcuteSqlCase.ReportById_Query(reportId))[0];
        }
        public IList<object[]> GetReportM(Dictionary<string, object> dictReport)
        {
            IList<object[]> ss = BaseSqlOrder.GetMultipleByQuery(ExcuteSqlCase.Report_MQuery(dictReport));
            return ss;
        }
        public IList<object> GetReportByTime(Dictionary<string, object> dictReport)
        {
            IList<object> ss = BaseSqlOrder.GetMultipleByQuery(ExcuteSqlCase.ReportByTime_MQuery(dictReport))[0];
            return ss;
        }
        #endregion


        #region 从枚举中获得修改字符串
        private string GetUpdateStrByEnum(EnumLibrary.EnumTable enumTable, Dictionary<string, object> dict)
        {
            switch (enumTable)
            {
                case EnumLibrary.EnumTable.Employee:
                    return ExcuteSqlCase.Employee_Update(dict);
                case EnumLibrary.EnumTable.FaultType:
                    return ExcuteSqlCase.FaultType_Update(dict);
                case EnumLibrary.EnumTable.Department:
                    return ExcuteSqlCase.Department_Update(dict);
                case EnumLibrary.EnumTable.Authorization:
                    return ExcuteSqlCase.Authorization_Update(dict);
                case EnumLibrary.EnumTable.Suppliers:
                    return ExcuteSqlCase.Supplier_Update(dict);
                case EnumLibrary.EnumTable.EmployeePwd:
                    return ExcuteSqlCase.Password_Update(dict);
                case EnumLibrary.EnumTable.EmployeePhoto:
                    return ExcuteSqlCase.Photo_Update(dict);
                case EnumLibrary.EnumTable.FileLinkByVehicelEml:
                    return ExcuteSqlCase.EmlTemp_Update(dict);
                case EnumLibrary.EnumTable.FileLinkByVehicelEmlSort:
                    return ExcuteSqlCase.EmlTempSort_Update(dict);
                case EnumLibrary.EnumTable.FileLinkByVehicelCfg:
                    return ExcuteSqlCase.CfgTemp_Update(dict);
                case EnumLibrary.EnumTable.FileLinkByVehicelBau:
                    return ExcuteSqlCase.CfgTempBaud_Update(dict); 
                case EnumLibrary.EnumTable.FileLinkByVehicelBaudOnly:
                    return ExcuteSqlCase.CfgBaud_Update(dict);
                case EnumLibrary.EnumTable.FileLinkByVehicel:
                    return ExcuteSqlCase.FileLinkByVehicel_Update(dict);
                case EnumLibrary.EnumTable.FileLinkByVehicelColEml:
                    return ExcuteSqlCase.FileLinkByVehicel_ColEml(dict);
                case EnumLibrary.EnumTable.FileLinkByVehicelEml_Col:
                    return ExcuteSqlCase.FileLinkByVehicelEml_Col(dict);
                case EnumLibrary.EnumTable.FileLinkByCfg:
                    return ExcuteSqlCase.CfgTmp_Update(dict);
                case EnumLibrary.EnumTable.ExampleToly:
                    return ExcuteSqlCase.EmlToly_Update(dict);
                case EnumLibrary.EnumTable.Task:
                    return ExcuteSqlCase.Task_Update(dict);
                case EnumLibrary.EnumTable.TaskDTC:
                    return ExcuteSqlCase.TaskDTC_Update(dict);
                case EnumLibrary.EnumTable.TaskUpdate:
                    return ExcuteSqlCase.AddTask_Update(dict);
                case EnumLibrary.EnumTable.LoginLog:
                    return ExcuteSqlCase.LoginOffDate_Update(dict);
                case EnumLibrary.EnumTable.ExampleTemp:
                    return ExcuteSqlCase.ExampleTemp_Update(dict);
                case EnumLibrary.EnumTable.ConfigTemp:
                    return ExcuteSqlCase.ConfigTemp_Update(dict);
                case EnumLibrary.EnumTable.ReportUpdate:
                    return ExcuteSqlCase.Report_Update(dict);
                case EnumLibrary.EnumTable.Segment:
                    return ExcuteSqlCase.Segment_Update(dict);
                case EnumLibrary.EnumTable.ExapChapter:
                    return ExcuteSqlCase.ExapChapter_Update(dict);
                case EnumLibrary.EnumTable.NodeConfigurationBox:
                    return ExcuteSqlCase.NodeConfigurationBox_Update(dict);
                case EnumLibrary.EnumTable.ErrorInfo:
                    return ExcuteSqlCase.Error_Update(dict);
                case EnumLibrary.EnumTable.Topology:
                    return ExcuteSqlCase.Topology_Update(dict);
                case EnumLibrary.EnumTable.QuestionNote:
                    return ExcuteSqlCase.QuestionNote_Update(dict);
                case EnumLibrary.EnumTable.PassReportNote:
                    return ExcuteSqlCase.PassReportNote_Update(dict);
                case EnumLibrary.EnumTable.DBC:
                    return ExcuteSqlCase.DBC_Update(dict);
                case EnumLibrary.EnumTable.ExampleTempTemplate:
                    return ExcuteSqlCase.ExampleTempTemplate_Update(dict);
                default:
                    return "";
            }
        }
        #endregion

        #region 从枚举中获得删除字符串

        /// <summary>
        /// 从枚举中获得删除字符串
        /// </summary>
        /// <param name="enumTable">枚举值</param>
        /// <param name="drKey">参数</param>
        /// <returns></returns>
        private string GetDelStrByEnum(EnumLibrary.EnumTable enumTable, Dictionary<string,object> drKey)
        {
            switch (enumTable)
            {
                case EnumLibrary.EnumTable.DBC:
                    return ExcuteSqlCase.DBCFile_Del(drKey);
                case EnumLibrary.EnumTable.Topology:
                    return ExcuteSqlCase.TopologyFile_Del(drKey);
                case EnumLibrary.EnumTable.DBCByVehicel:
                    return ExcuteSqlCase.DBCByVehicel_Del(drKey);
                case EnumLibrary.EnumTable.FaultType:
                    return ExcuteSqlCase.FaultType_Del(drKey);
                case EnumLibrary.EnumTable.Suppliers:
                    return ExcuteSqlCase.Supplier_Del(drKey);
                case EnumLibrary.EnumTable.Authorization:
                    return ExcuteSqlCase.Authorization_Del(drKey);
                case EnumLibrary.EnumTable.Employee:
                    return ExcuteSqlCase.Empolyee_Del(drKey);
                case EnumLibrary.EnumTable.ExapChapter:
                    return ExcuteSqlCase.ExapChapter_Del(drKey);
                case EnumLibrary.EnumTable.Department:
                    return ExcuteSqlCase.Department_Del(drKey);
                case EnumLibrary.EnumTable.Project:
                    return ExcuteSqlCase.ProjectFile_Del(drKey);
                case EnumLibrary.EnumTable.Task:
                    return ExcuteSqlCase.Task_Del(drKey);
                case EnumLibrary.EnumTable.Report:
                    return ExcuteSqlCase.Report_Del(drKey);
                case EnumLibrary.EnumTable.ReportSole:
                    return ExcuteSqlCase.ReportByTaskName_Del(drKey);
                case EnumLibrary.EnumTable.ReportByTask:
                    return ExcuteSqlCase.ReportByTask_Del(drKey);
                case EnumLibrary.EnumTable.TaskDBC:
                    return ExcuteSqlCase.TaskDbc_Del(drKey);
                case EnumLibrary.EnumTable.TaskTable:
                    return ExcuteSqlCase.TaskDbcByTaskName_Del(drKey);
                case EnumLibrary.EnumTable.TaskByCANRoad:
                    return ExcuteSqlCase.TaskDbcByCANRoad_Del(drKey);
                case EnumLibrary.EnumTable.FileLinkByVehicelCfg:
                    return ExcuteSqlCase.FileLink_Del(drKey);
                case EnumLibrary.EnumTable.FileLinkByVehicelColEml:
                    return ExcuteSqlCase.FileLinkBySort_Del(drKey);
                case EnumLibrary.EnumTable.FileLinkByVehicel:
                    return ExcuteSqlCase.FileLink_Del(drKey);
                case EnumLibrary.EnumTable.FileLinkByCfg:
                    return ExcuteSqlCase.FileLinkByCfg_Del(drKey);
                case EnumLibrary.EnumTable.QuestionNote:
                    return ExcuteSqlCase.QuestionNote_Del(drKey);
                case EnumLibrary.EnumTable.QuestionNoteByTask:
                    return ExcuteSqlCase.QuestionNoteByTask_Del(drKey);
                case EnumLibrary.EnumTable.QuestionNoteByCondition:
                    return ExcuteSqlCase.PassReportNoteByTestType_Del(drKey);
                case EnumLibrary.EnumTable.QuestionNoteSort:
                    return ExcuteSqlCase.QuestionNoteByVehicel_Del(drKey);
                case EnumLibrary.EnumTable.PassReportNote:
                    return ExcuteSqlCase.PassReportNoteByVehicel_Del(drKey);
                case EnumLibrary.EnumTable.PassReportNoteByTask:
                    return ExcuteSqlCase.PassReportNoteByTask_Del(drKey);
                case EnumLibrary.EnumTable.PassReportNoteByCondition:
                    return ExcuteSqlCase.QuestionNoteByTestType_Del(drKey);
                case EnumLibrary.EnumTable.Segment:
                    return ExcuteSqlCase.Segment_Del(drKey);
                case EnumLibrary.EnumTable.NodeConfigurationBox:
                    return ExcuteSqlCase.NodeConfigurationBox_Del(drKey);
                case EnumLibrary.EnumTable.UploadInfo:
                    return ExcuteSqlCase.Upload_Del(drKey);
                default:
                    return "";
            }
        }

        #endregion

        #region 从枚举中获得查询字符串[有重载]
        /// <summary>
        /// 从枚举中获得查询字符串
        /// </summary>
        /// <param name="enumQuery">枚举值</param>
        /// <returns>查询条件</returns>
        private string GetQueryByEnum(EnumLibrary.EnumTable enumQuery)
        {
            switch (enumQuery)
            {
                case EnumLibrary.EnumTable.Department:
                    return ExcuteSqlCase.Department_Query();
                case EnumLibrary.EnumTable.FaultType:
                    return ExcuteSqlCase.FaultType_Query();
                case EnumLibrary.EnumTable.Employee:
                    return ExcuteSqlCase.Employee_Query();
                case EnumLibrary.EnumTable.ExapChapter:
                    return ExcuteSqlCase.ExapChapter_Query();
                case EnumLibrary.EnumTable.Task:
                    return ExcuteSqlCase.Task_Query();
                case EnumLibrary.EnumTable.ConfigTemp:
                    return ExcuteSqlCase.Config_Query();
                case EnumLibrary.EnumTable.ExampleTemp:
                    return ExcuteSqlCase.Example_Query(); 
                case EnumLibrary.EnumTable.Authorization:
                    return ExcuteSqlCase.VehicelAuth_Query();
                case EnumLibrary.EnumTable.DBC:
                    return ExcuteSqlCase.DBCFile_Query();
                case EnumLibrary.EnumTable.FileLinkByVehicel:
                    return ExcuteSqlCase.FileLink_Query();
                case EnumLibrary.EnumTable.Suppliers:
                    return ExcuteSqlCase.Supplier_Query();
                case EnumLibrary.EnumTable.OperationLog:
                    return ExcuteSqlCase.OperationLogNo();
                case EnumLibrary.EnumTable.Project:
                    return ExcuteSqlCase.ProjectFile_Query();
                case EnumLibrary.EnumTable.Report:
                    return ExcuteSqlCase.Report_Query();
                case EnumLibrary.EnumTable.LoginLog:
                    return ExcuteSqlCase.LoginLogNo();
                case EnumLibrary.EnumTable.QuestionNote:
                    return ExcuteSqlCase.QuestionNote();
                case EnumLibrary.EnumTable.PassReportNote:
                    return ExcuteSqlCase.PassReportNote();
                case EnumLibrary.EnumTable.Segment:
                    return ExcuteSqlCase.Segment();
                case EnumLibrary.EnumTable.NodeConfigurationBox:
                    return ExcuteSqlCase.NodeConfigurationBox();
                case EnumLibrary.EnumTable.UploadInfo:
                    return ExcuteSqlCase.UPload_Query();
                case EnumLibrary.EnumTable.Peportlnfo:
                    return ExcuteSqlCase.Peportlnfo_Query();
                case EnumLibrary.EnumTable.LastLoginUser:
                    return ExcuteSqlCase.LastLoginUser_Query();
                default:
                    return "";
            }
        }

        /// <summary>
        /// 通过查询条件获得查询字符串
        /// </summary>
        /// <param name="enumQuery">枚举值</param>
        /// <param name="drValue">查询条件</param>
        /// <returns></returns>
        private string GetQueryByEnum(EnumLibrary.EnumTable enumQuery, Dictionary<string, object> drValue)
        {
            switch (enumQuery)
            {
                case EnumLibrary.EnumTable.OperationLog:
                    return ExcuteSqlCase.OperationLogToName(drValue);
                case EnumLibrary.EnumTable.OperationLogToTime:
                    return ExcuteSqlCase.OperationLogToTime(drValue);
                case EnumLibrary.EnumTable.DBC:
                    return ExcuteSqlCase.DBCDataComparison_Query(drValue);
                case EnumLibrary.EnumTable.DBCBelongCAN:
                    return ExcuteSqlCase.DBCDataComparison(drValue);
                case EnumLibrary.EnumTable.DbcCheck:
                    return ExcuteSqlCase.DBC_Query(drValue);
                case EnumLibrary.EnumTable.FileLinkByVehicel:
                    return ExcuteSqlCase.FileLinkByVehicel_Get(drValue);
                case EnumLibrary.EnumTable.FileLinkByVehicelDouble:
                    return ExcuteSqlCase.FileLinkByVehicel_Double(drValue);
                case EnumLibrary.EnumTable.FileLinkByVehicelCount:
                    return ExcuteSqlCase.FileLinkByVehicel_Count(drValue);
                case EnumLibrary.EnumTable.FileLinkByVehicelRep:
                    return ExcuteSqlCase.FileLinkByVehicel_Report(drValue);
                case EnumLibrary.EnumTable.Topology:
                    return ExcuteSqlCase.Topology_Query(drValue);
                case EnumLibrary.EnumTable.Employee:
                    return ExcuteSqlCase.Password_Get(drValue);
                case EnumLibrary.EnumTable.LoginLog:
                    return ExcuteSqlCase.LoginLogToName(drValue);
                case EnumLibrary.EnumTable.LoginLogToTime:
                    return ExcuteSqlCase.LoginLogToTime(drValue);
                case EnumLibrary.EnumTable.Suppliers:
                    return ExcuteSqlCase.Supplier_Get(drValue);
                case EnumLibrary.EnumTable.Employee_role:
                    return ExcuteSqlCase.Role_Get(drValue);
                case EnumLibrary.EnumTable.Authorization_Auth:
                    return ExcuteSqlCase.Config_Get(drValue);
                case EnumLibrary.EnumTable.Authorization_Table:
                    return ExcuteSqlCase.Authorization_Table(drValue);
                case EnumLibrary.EnumTable.TaskTest:
                    return ExcuteSqlCase.ContainExmp_Get(drValue); 
                case EnumLibrary.EnumTable.DeviceStatus:
                    return ExcuteSqlCase.ExapBoolToDeviceStatus(drValue);
                case EnumLibrary.EnumTable.DeviceStatusCount:
                    return ExcuteSqlCase.TaskToDeviceStatus(drValue);
                case EnumLibrary.EnumTable.ExampleTemp:
                    return ExcuteSqlCase.GetExampleTemp(drValue);
                case EnumLibrary.EnumTable.ExampleTemp_Ver:
                    return ExcuteSqlCase.GetExampleTempVer(drValue);
                case EnumLibrary.EnumTable.ExampleMatch:
                    return ExcuteSqlCase.Exmp_MatchQuery(drValue);
                case EnumLibrary.EnumTable.ConfigTemp:
                    return ExcuteSqlCase.GetExampleCon(drValue);
                case EnumLibrary.EnumTable.ConfigTemp_Ver:
                    return ExcuteSqlCase.GetConfigTempVer(drValue);
                case EnumLibrary.EnumTable.TaskTable:
                    return ExcuteSqlCase.TaskTable_Query(drValue);
                case EnumLibrary.EnumTable.Task:
                    return ExcuteSqlCase.TaskModuleRepeat_Query(drValue);
                case EnumLibrary.EnumTable.QuestionNote:
                    return ExcuteSqlCase.QuestionNote_Query(drValue);
                case EnumLibrary.EnumTable.QuestionNoteSort:
                    return ExcuteSqlCase.QuestionNoteSort(drValue);
                case EnumLibrary.EnumTable.QuestionNoteByCondition:
                    return ExcuteSqlCase.QuestionNoteByCondition_Query(drValue);
                case EnumLibrary.EnumTable.PassReportNote:
                    return ExcuteSqlCase.PassReportNote(drValue);
                case EnumLibrary.EnumTable.PassReportNoteByCondition:
                    return ExcuteSqlCase.PassReportNoteByCondition_Query(drValue);
                case EnumLibrary.EnumTable.ReportSole:
                    return ExcuteSqlCase.Report_Query(drValue);
                case EnumLibrary.EnumTable.Report:
                    return ExcuteSqlCase.ReportByText_Query(drValue);
                case EnumLibrary.EnumTable.NodeConfigurationBox:
                    return ExcuteSqlCase.NodeCfg_Query(drValue);
                case EnumLibrary.EnumTable.DBCInformation:
                    return ExcuteSqlCase.DBC_Query1(drValue); 
                case EnumLibrary.EnumTable.TopologySelect:
                    return ExcuteSqlCase.ALLToplogy_Query(drValue);
                case EnumLibrary.EnumTable.SegmentByName:
                    return ExcuteSqlCase.DBCByName(drValue);
                case EnumLibrary.EnumTable.Segment:
                    return ExcuteSqlCase.Segment_Query(drValue);
                case EnumLibrary.EnumTable.DTC:
                    return ExcuteSqlCase.DTC_Query(drValue);
                case EnumLibrary.EnumTable.Example_QueryByBusType:
                    return ExcuteSqlCase.Example_QueryByBusType(drValue);
                default:
                    return "";

                    
            }
        }
        #endregion

        #region 从枚举中获得删除字符串

        /// <summary>
        /// 从枚举中获得删除字符串
        /// </summary>
        /// <param name="enumTable">枚举值</param>
        /// <param name="drKey">参数</param>
        /// <returns></returns>
        private string GetCountStrByEnum(EnumLibrary.EnumTable enumTable, Dictionary<string, object> drKey)
        {
            switch (enumTable)
            {
                case EnumLibrary.EnumTable.DBC:
                    return ExcuteSqlCase.DBC_Count(drKey);
                case EnumLibrary.EnumTable.Employee:
                    return ExcuteSqlCase.Mail_Count(drKey);
                default:
                    return "";
            }
        }

        #endregion

        public bool AddLoginLog(Dictionary<string, object> dictLog, out string error)
        {
            var loginlog = new Model.LoginLog();
            {
                loginlog.LoginNo = dictLog["LoginNo"].ToString();
                loginlog.EmployeeNo = dictLog["EmployeeNo"].ToString();
                loginlog.EmployeeName = dictLog["EmployeeName"].ToString();
                loginlog.Department = dictLog["Department"].ToString();
                loginlog.LoginDate = DateTime.Parse(dictLog["LoginDate"].ToString());
                loginlog.LoginOffDate = DateTime.Parse(dictLog["LoginOffDate"].ToString());
            };
            return BaseSqlOrder.Add(loginlog, out error);
        }

        public Dictionary<string, Dictionary<string, bool>> boolExample(Dictionary<string,object> dictTask )
        {
            Dictionary <string, string> ExampleJsonList=new Dictionary<string, string>();
            Dictionary<string,bool> ExampleBoolList=new Dictionary<string, bool>();
            Dictionary<string,Dictionary<string, bool>> ExampleList = new Dictionary<string, Dictionary<string, bool>>();
            var dictStrTask = DictObjToStr(dictTask);
            string examStr = GetExmpJsonByName(dictStrTask);
            //var examSql = GetSpecialByEnum(EnumLibrary.EnumTable.DeviceStatus, dictTask);
            //var examSqlList = ObjListToDict(dictTask)//ObjListToDict(examSql[0]);
            var dictExap = Json.DeserJsonToDDict(examStr);
            foreach (KeyValuePair<string, Dictionary<string, List<object>>> ExapOne in dictExap)
            {
                var ExapToOne = ExapOne.Value.ToArray();
                foreach (KeyValuePair<string, List<object>> ExapTwo in ExapToOne)
                {
                    var ExapToTwo = ExapTwo.Value[0];
                    ExampleJsonList = Json.DerJsonToDict(ExapToTwo.ToString());
                    ExampleBoolList = ExapBool(JsonListToBoolList(ExampleJsonList), dictStrTask);
                    ExampleList.Add(ExampleJsonList["ExapID"], ExampleBoolList);
                }
            }
            return ExampleList;
        }

        private Dictionary<string, string> DictObjToStr(Dictionary<string, object> dict)
        {
            Dictionary<string, string> _dict = new Dictionary<string, string>();
            _dict["TaskNo"] = dict["TaskNo"].ToString();
            _dict["TaskRound"] = dict["TaskRound"].ToString();
            _dict["TaskName"] = dict["TaskName"].ToString();
            _dict["CANRoad"] = dict["CANRoad"].ToString();
            _dict["Module"] = dict["Module"].ToString();

            //_dict["SupplyPower"] = dict["SupplyPower"].ToString();
            //_dict["Oscilloscope"] = dict["Oscilloscope"].ToString();
            //_dict["Multimeter"] = dict["Multimeter"].ToString();
            //_dict["PNPower"] = dict["PNPower"].ToString();
            //_dict["IsPrototypeHidden"] = dict["IsPrototypeHidden"].ToString();
            //_dict["DBCMessage"] = dict["DBCMessage"].ToString();
            //_dict["isLinkDBC"] = dict["isLinkDBC"].ToString();
            return _dict;
        }

        private Dictionary<string, string> JsonListToBoolList(Dictionary<string, string> ExampleJsonList)
        {
            #region True的值
            string SupplyPower = "使用";
            string Oscilloscope = "使用";
            string Multimeter = "使用";
            string PNPower = "使用";
            string IsPrototypeHidden = "显性";
            string DBCMessage = "依赖";
            string isLinkDBC = "依赖";
            #endregion
            #region 对Json转换出来的数据进行赋Bool字符串的操作
            if (ExampleJsonList["SupplyPower"] == SupplyPower)
            {
                ExampleJsonList["SupplyPower"] = "True";
            }
            else
            {
                ExampleJsonList["SupplyPower"] = "False";
            }
            if (ExampleJsonList["Oscilloscope"] == Oscilloscope)
            {
                ExampleJsonList["Oscilloscope"] = "True";
            }
            else
            {
                ExampleJsonList["Oscilloscope"] = "False";
            }
            if (ExampleJsonList["Multimeter"] == Multimeter)
            {
                ExampleJsonList["Multimeter"] = "True";
            }
            else
            {
                ExampleJsonList["Multimeter"] = "False";
            }
            if (ExampleJsonList["PNPower"] == PNPower)
            {
                ExampleJsonList["PNPower"] = "True";
            }
            else
            {
                ExampleJsonList["PNPower"] = "False";
            }
            if (ExampleJsonList["IsPrototypeHidden"] == IsPrototypeHidden)
            {
                ExampleJsonList["IsPrototypeHidden"] = "True";
            }
            else
            {
                ExampleJsonList["IsPrototypeHidden"] = "False";
            }
            if (ExampleJsonList["DBCMessage"] == DBCMessage)
            {
                ExampleJsonList["DBCMessage"] = "True";
            }
            else
            {
                ExampleJsonList["DBCMessage"] = "False";
            }
            if (ExampleJsonList["isLinkDBC"] == isLinkDBC)
            {
                ExampleJsonList["isLinkDBC"] = "True";
            }
            else
            {
                ExampleJsonList["isLinkDBC"] = "False";
            }
            #endregion
            return ExampleJsonList;
        }

        private Dictionary<string, bool> ExapBool(Dictionary<string, string> ExampleJsonList, Dictionary<string, string> examSqlList)
        {
            Dictionary<string, bool> ExampleBoolList = new Dictionary<string, bool>();
            int i = 0;
            #region 硬件判断
            if (ExampleJsonList["SupplyPower"] == examSqlList["SupplyPower"])
            {
                ExampleBoolList["SupplyPower"] = true;
                i++;
            }
            else
            {
                if (ExampleJsonList["SupplyPower"] == "False")
                {
                    ExampleBoolList["SupplyPower"] = true;
                    i++;
                }
                else
                {
                    ExampleBoolList["SupplyPower"] = false;
                }
            }
            if (ExampleJsonList["Oscilloscope"] == examSqlList["Oscilloscope"])
            {
                ExampleBoolList["Oscilloscope"] = true;
                i++;
            }
            else
            {
                if (ExampleJsonList["Oscilloscope"] == "False")
                {
                    ExampleBoolList["Oscilloscope"] = true;
                    i++;
                }
                else
                {
                    ExampleBoolList["Oscilloscope"] = false;
                }
            }
            if (ExampleJsonList["Multimeter"] == examSqlList["Multimeter"])
            {
                ExampleBoolList["Multimeter"] = true;
                i++;
            }
            else
            {
                if (ExampleJsonList["Multimeter"] == "False")
                {
                    ExampleBoolList["Multimeter"] = true;
                    i++;
                }
                else
                {
                    ExampleBoolList["Multimeter"] = false;
                }
            }
            if (ExampleJsonList["PNPower"] == examSqlList["PNPower"])
            {
                ExampleBoolList["PNPower"] = true;
                i++;
            }
            else
            {
                if (ExampleJsonList["PNPower"] == "False")
                {
                    ExampleBoolList["PNPower"] = true;
                    i++;
                }
                else
                {
                    ExampleBoolList["PNPower"] = false;
                }
            }
            #endregion
            #region 样件判断
            if (ExampleJsonList["IsPrototypeHidden"] == examSqlList["IsPrototypeHidden"])
            {
                ExampleBoolList["IsPrototypeHidden"] = true;
                i++;
            }
            else
            {
                ExampleBoolList["IsPrototypeHidden"] = false;
            }
            if (ExampleJsonList["DBCMessage"] == examSqlList["DBCMessage"])
            {
                ExampleBoolList["DBCMessage"] = true;
                i++;
            }
            else
            {
                ExampleBoolList["DBCMessage"] = false;
            }
            if (ExampleJsonList["isLinkDBC"] == examSqlList["isLinkDBC"])
            {
                ExampleBoolList["isLinkDBC"] = true;
                i++;
            }
            else
            {
                ExampleBoolList["isLinkDBC"] = false;
            }
            if (i == 7)
            {
                ExampleBoolList["Result"] = true;
            }
            else
            {
                ExampleBoolList["Result"] = false;
            }
            #endregion
            return ExampleBoolList;
        }

        
    }
}
