using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CANoeEngine;
using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraSplashScreen;
using ProcessEngine;
using UltraANetT.Form;
using UltraANetT.Properties;
using XtraMessageBoxArgs = DevExpress.XtraEditors.XtraMessageBoxArgs;
using XtraMessageBoxForm = DevExpress.XtraEditors.XtraMessageBoxForm;
using System.Diagnostics;

namespace UltraANetT.Module
{
    public partial class TestStart : XtraUserControl
    {

        #region 委托事件，可以在线程中调用用户控件
        public delegate void ShowMsgCount(string count);
        public delegate void ShowStep(int value);
        public delegate void ShowGrid(int row, string colName, string value, string colTime,string time);
        public delegate void ShowReport(int row, string colName, string value);
        public delegate void Showtxt();
        public delegate void ResetPos();
        public delegate void CheckPos();
        public delegate void ShowExamID(string examId);

        private Dictionary<string, List<List<string>>> autoReport = new Dictionary<string, List<List<string>>>();
        #region Excel报告相关
        private Dictionary<string, string> excelCoverReport = new Dictionary<string, string>();
        private Dictionary<string, List<List<string>>> excelReport = new Dictionary<string, List<List<string>>>();
        private Dictionary<string, string> excelPathReport = new Dictionary<string, string>();
        private string _reportDirPath = string.Empty;//当前测试完成后报告存储文件夹
        private string _reportTime = string.Empty;//生成报告用的时间字符串
        
        #endregion
        #endregion

        #region 辅助类
        private ProcStore _store = new ProcStore();
        private ProcFile _file = new ProcFile();
        private ProcShow _show = new ProcShow();
        private ProcCANoeTest CANoeTest = new ProcCANoeTest();
        private SearchDTCByExaModule _search = new SearchDTCByExaModule();
        #endregion

        #region 存储解析用例的数据结构
        Dictionary<string, List<Dictionary<string, string>>> _dictValue =
            new Dictionary<string, List<Dictionary<string, string>>>();
        Dictionary<string, List<Dictionary<string, string>>> _dictReportValue =
            new Dictionary<string, List<Dictionary<string, string>>>();

        Dictionary<string, Dictionary<string, List<object>>> _dictExap =
            new Dictionary<string, Dictionary<string, List<object>>>();
        Dictionary<string, string> drContent = new Dictionary<string, string>();
        List<string> exampleHNamList = new List<string>();
        List<string> exampleDNamList = new List<string>();
        private Dictionary<string, Dictionary<string, string>> ExmpList = new Dictionary<string, Dictionary<string, string>>();
        private DateTime dtTime;
        Dictionary<string, object> drReport = new Dictionary<string, object>();
        private readonly string _selfCheckStr = AppDomain.CurrentDomain.BaseDirectory +
                                            @"configuration\selfCheck\\IncludeFiles\DevInfo.ini";

        #region Excel报告相关
        private Dictionary<string, Dictionary<string, string>> _dictdictTestList = new Dictionary<string, Dictionary<string, string>>();
        #endregion

        #endregion

        #region 其他变量
        private DataTable _dtDominance = new DataTable();
        private DataTable _dtHidden = new DataTable();
        private int rowUpload;
        private Thread threadH;
        private Thread threadHSet;
        private Thread thread;
        private Thread threadSet;
        private Thread threadexample;
        bool isExistDBC = true;
        private bool isClose = false;
        private bool isHExampleReport = false;//如果隐形用例测试完成后无显性用例则赋值为true进行生成报告，生成报告后重新赋值为false
        private bool _isTestStart = false;//用来判断是否点击了开始测试，点击了为true
        private bool _isReportCreate = false;

        /// <summary>
        /// 当前选中测试属于哪个总线
        /// </summary>
        private string _busType = string.Empty;
        #endregion

        #region ini转换变量
        List<string> _nodeColl = new List<string>();
        List<string>_nodeList = new List<string>();      
        List<string> _vNode = new List<string>();
        Dictionary<string, object> dictFile = new Dictionary<string, object>();
        private IList<object[]> flList;
        private IList<object[]> dbc;
        Dictionary<string, string> dictBaud = new Dictionary<string, string>();
        private List<Dictionary<string, string>> dictContent = new List<Dictionary<string, string>>();

        private Dictionary<string, string> dictDTC = new Dictionary<string, string>();
        Dictionary<string, object> _dictNote = new Dictionary<string, object>();
        #endregion

        private bool _isRun = false;
        private bool _isGet= false;
        private int _remainCount = 99;
        private bool _isReport = false;
        private DataRow _dr;
        string report;
        private bool IsFisrt = true;
        private string _testType;
        private string _suppier;
        public TestStart()
        {
            InitializeComponent();
            pictStart.Enabled = false;
            pictStart.Image = Resources.play_unable;
            pictStop.Enabled = false;
            pictStop.Image = Resources.stop_unbale;
            pictPretreatment.Enabled = false;
            pictPretreatment.Image = Resources.Pretreatment_unable;
            btnCheckRep.Enabled = false;
            btnImportTxtlog.Enabled = false;
            ShieldRight();
        }

        public TestStart(string module,string taskName)
        {
            InitializeComponent();
            //DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            Draw(module);
            //pictStart.Enabled = false;
            //pictStart.Image = Resources.play_unable;
            pictStop.Enabled = false;
            pictStop.Image = Resources.stop_unbale;
            btnUpload.Enabled = true;
            //
            List<string> vNode = GlobalVar.CurrentTsNode[0].Split('-').ToList();
            vNode.Add(GlobalVar.CurrentTsNode[3]);
            var dbcList = _store.GetDBCListByVNodeAndCAN(vNode);
            //
            string exam;
            if (dbcList[0][9].ToString().ToUpper() == "J1939")
            {
                GetPath(taskName, true, out report, out exam);
            }
            else
            {
                GetPath(taskName, false, out report, out exam);
            }

            EnumLibrary.CfgPath = exam;
            try
            {
                FileInfo fi = new FileInfo(exam);
                EnumLibrary.SelfPath = fi.Directory.FullName + @"\IncludeFiles\DevInfo.ini";
            }
            catch (Exception)
            {
                EnumLibrary.SelfPath = string.Empty;
            }

            if (exam == "无")
            {
                XtraMessageBox.Show("未添加用例工程文件路径，请到‘工具管理‘中添加");
                pictStart.Enabled = false;
                return;
            }
           
            if (report == "无")
            {
                XtraMessageBox.Show("未添加报告文件路径，请到‘工具管理‘中添加");
                pictStart.Enabled = false;
                return;
            }

            if (!File.Exists(@"ExcelTemplate\" + taskName + ".xlsx"))
            {
                XtraMessageBox.Show("未找到" + taskName + "的报告模板！");
                pictStart.Enabled = false;
                return;
            }

            //if (EnumLibrary.SelfPath == string.Empty)
            //{
            //    XtraMessageBox.Show("自检ini文件路径检查失败，请检查是否创建了正确的文件结构");
            //    pictStart.Enabled = false;
            //    return;
            //}

            if (_dictExap.Count <= 0)
            {
                XtraMessageBox.Show("用例库为空，请添加相应用例库。");
                pictStart.Enabled = false;
                return;
            }
            string strStateMachinePath = AppDomain.CurrentDomain.BaseDirectory + @"configuration\DLL\StateMachine.exe";
            if (!File.Exists(strStateMachinePath))
            {
                XtraMessageBox.Show("状态机不存在，请检查状态机是否放在了指定路径" + System.Environment.NewLine + "路径：" +
                                    strStateMachinePath);
            }
            CANoeTest.GetName(EnumLibrary.CfgPath, exam, strStateMachinePath);
            btnCheckRep.Enabled = false;
            btnImportTxtlog.Enabled = false;
            ShieldRight();
            if (_testType == string.Empty)
                return;
            if (taskName.Contains("集成"))
            {
                ReSlaveBox rsb = new ReSlaveBox(_busType);
                rsb.ShowDialog();
                if (rsb.DialogResult == DialogResult.OK)
                {
                    if (!GlobalVar.isGetSlaveBoxID)
                    {
                        pictStart.Enabled = false;
                    }
                    else
                    {
                        pictStart.Enabled = true;
                    }
                }
                else
                {
                    pictStart.Enabled = false;
                }
            }
        }

        /// <summary>
        /// 根据任务类别得到相应的报告路径和用例路径
        /// </summary>
        /// <param name="taskName">任务类别名称<param>
        /// <param name="is1939">是否是1939</param>
        /// <param name="report">报告路径</param>
        /// <param name="exam">用例路径</param>
        private void GetPath(string taskName, bool is1939, out string report, out string exam)
        {
            switch (taskName)
            {
                case "CAN通信单元":
                    report = is1939? EnumLibrary.CANSig1939Report : EnumLibrary.CANSigReport;
                    exam = is1939 ? EnumLibrary.CANSig1939Exam : EnumLibrary.CANSigExam;
                    break;
                case "CAN通信集成":
                    report = is1939 ? EnumLibrary.CANLtg1939Report : EnumLibrary.CANLtgReport;
                    exam = is1939 ? EnumLibrary.CANLtg1939Exam : EnumLibrary.CANLtgExam;
                    break;
                case "LIN通信主节点":
                    report = EnumLibrary.LINSigReport;
                    exam = EnumLibrary.LINSigExam;
                    break;
                case "LIN通信从节点":
                    report = EnumLibrary.LINSigFromReport;
                    exam = EnumLibrary.LINSigFromExam;
                    break;
                case "LIN通信集成":
                    report = EnumLibrary.LINLtgReport;
                    exam = EnumLibrary.LINLtgExam;
                    break;
                case "OSEK单节点":
                    report = EnumLibrary.OSEKSigReport;
                    exam = EnumLibrary.OSEKSigExam;
                    break;
                case "OSEK集成":
                    report = EnumLibrary.OSEKLtnReport;
                    exam = EnumLibrary.OSEKLtnExam;
                    break;
                case "通信DTC":
                    report = EnumLibrary.DTCreport;
                    exam = EnumLibrary.DTCExam;
                    break;
                case "网关路由":
                    report = EnumLibrary.WifiReport;
                    exam = EnumLibrary.WifiExam;
                    break;
                case "直接NM单元":
                    report = is1939 ? EnumLibrary.AutoSARNM1939Report : EnumLibrary.AutoSARNMReport;
                    exam = is1939 ? EnumLibrary.AutoSARNM1939Exam : EnumLibrary.AutoSARNMExam;
                    break;
                case "直接NM集成":
                    report = is1939 ? EnumLibrary.AutoSARNMLtg1939Report : EnumLibrary.AutoSARNMLtgReport;
                    exam = is1939 ? EnumLibrary.AutoSARNMLtg1939Exam : EnumLibrary.AutoSARNMLtgExam;
                    break;
                case "动力域NM主节点":
                    report = is1939 ? EnumLibrary.DynamicNM1939Report : EnumLibrary.DynamicNMReport;
                    exam = is1939 ? EnumLibrary.DynamicNM1939Exam : EnumLibrary.DynamicNMExam;
                    break;
                case "动力域NM从节点":
                    report = is1939 ? EnumLibrary.DynamicNMFrom1939Report : EnumLibrary.DynamicNMFromReport;
                    exam = is1939 ? EnumLibrary.DynamicNMFrom1939Exam : EnumLibrary.DynamicNMFromExam;
                    break;
                case "动力域NM集成":
                    report = is1939 ? EnumLibrary.DynamicNMLtg1939Report : EnumLibrary.DynamicNMLtgReport;
                    exam = is1939 ? EnumLibrary.DynamicNMLtg1939Exam : EnumLibrary.DynamicNMLtgExam;
                    break;
                case "间接NM单元":
                    report = is1939 ? EnumLibrary.Indirect1939Report : EnumLibrary.IndirectReport;
                    exam = is1939 ? EnumLibrary.Indirect1939Exam : EnumLibrary.IndirectExam;
                    break;
                case "间接NM集成":
                    report = is1939 ? EnumLibrary.IndirectLtg1939Report : EnumLibrary.IndirectLtgReport;
                    exam = is1939 ? EnumLibrary.IndirectLtg1939Exam : EnumLibrary.IndirectLtgExam;
                    break;
                default:
                    report = "";
                    exam = "";
                    break;
            }
        }

        /// <summary>
        /// datagird中展开折叠并选中所有行
        /// </summary>
        private void SelectRows()
        {
            gvAuto.ExpandAllGroups();
            gvAuto.SelectAll();
        }

        /// <summary>
        /// 再生成INI文件过程中得到所有需要的变量
        /// </summary>
        private void GetBaseVar()
        {
            //
            _vNode = GlobalVar.CurrentTsNode[0].Split('-').ToList();
            _vNode.Add(GlobalVar.CurrentTsNode[3]);
            //
            dictFile.Clear();
            dictFile.Add("VehicelType", _vNode[0]);
            dictFile.Add("VehicelConfig", _vNode[1]);
            dictFile.Add("VehicelStage", _vNode[2]);
            dictFile.Add("MatchSort", _busType);
            //
            string node = GlobalVar.CurrentTsNode[4];
            if (node.Contains("/"))
                _nodeList = node.Split('/').ToList();
            else
                _nodeList.Add(node);
          
            _nodeColl = _show.GetDataFromDbc(_vNode,out isExistDBC);
            if (!isExistDBC)
            {
                MessageBox.Show(@"不存在相应的DBC文件，请手动添加至指定目录...");
                return;
                
            }
            flList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelDouble, dictFile);
            dbc = _store.GetDBCListByVNodeAndCAN(_vNode);
            //
            dictBaud = Json.DerJsonToDict(flList[0][8].ToString());
            dictContent = Json.DerJsonToLDict(flList[0][7].ToString());
            if (flList[0][3].ToString() != "网关路由")
            {
                foreach (Dictionary<string, string> content in dictContent)
                {
                    foreach (var no in _nodeList)
                    {
                        if (no == content["DUTname"])
                        {
                            if (!content.ContainsKey("DTCRelevant"))
                                continue;
                            string dtcStr = content["DTCRelevant"];
                            if (dtcStr != "")
                            {
                                dictDTC = Json.DerJsonToDict(dtcStr);
                                //var listdictECUDTCInfo = dictAllTemp.ContainsKey("ECUDTCInfo")
                                //    ? Json.DerJsonToLDict(dictAllTemp["ECUDTCInfo"])
                                //    : new List<Dictionary<string, string>>();
                                //var listdictECUGlobalVar = dictAllTemp.ContainsKey("ECUGlobalVar")
                                //    ? Json.DerJsonToDict(dictAllTemp["ECUGlobalVar"])
                                //    : new Dictionary<string, string>();
                                //dictDTC["ECUDTCInfo"] = listdictECUDTCInfo;
                                //dictDTC["ECUGlobalVar"] = listdictECUGlobalVar;
                                //dictDTC = Json.DerJsonToLDict(dtcStr);
                                break;
                                
                            }
                        }
                    }

                    if (dictDTC.Count > 0)
                    {
                        if (dictDTC.ContainsKey("ECUDTCInfo"))
                        {
                            if (dictDTC["ECUDTCInfo"].Length > 0)
                            {
                                break;
                            }  
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 生成INI文件
        /// </summary>
        private void ConvertCfg()
        {
            WriteToLog("生成ini中...");
            GetBaseVar();
            if(!isExistDBC)
                return;
            GlobalVar.CurrentVNode = _vNode;
            if (_file.CANConvertToType(GlobalVar.CurrentTsNode[2]) == 0)
            {
                //生成"CAN通信单元": "直接NM单元": "动力域NM主节点":"动力域NM从节点":"间接NM单元":"通信DTC":
                GetCANSig();
            }
            else if (_file.CANConvertToType(GlobalVar.CurrentTsNode[2]) == 1)
            {
                //生成"LIN通信主节点": "LIN通信从节点"
                GetLINSig();
                //if (dtcStr != "")
                //    GetDTC();
            }
            else if (_file.CANConvertToType(GlobalVar.CurrentTsNode[2]) == 2)
            {
                //网关路由
                GetGateWaySig();
            }
            else if (_file.CANConvertToType(GlobalVar.CurrentTsNode[2]) == 3)
            {
                //生成"集成"
                GetLtg();
            }
            WriteToLog("ini生成完毕...");
        }

        private void GetDTC()
        {
            Dictionary<string, string> dictHead = new Dictionary<string, string>();
            List<Dictionary<string, string>> listDtcByte = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> listDtc = new List<Dictionary<string, string>>();
            string folder = GlobalVar.CurrentTsNode.Aggregate("", (current, nodetype) => current + (nodetype + "-"));
            folder = folder.Remove(folder.Length - 1);
            //dictHead.Add("CddFileName", dictDTC[0]["CddFileName"]);
            //dictHead.Add("RequestID", dictDTC[0]["RequestID"]);
            //dictHead.Add("RespondID", dictDTC[0]["RespondID"]);
            //dictHead.Add("InitTimeofDiag", dictDTC[0]["InitTimeofDiag"]);
            //listDtcByte = Json.DerJsonToLDict(dictDTC[0]["MessageInfo"]);
            //listDtc = Json.DerJsonToLDict(dictDTC[0]["FaultInfo"]);
            _file.CreateDTCini(folder, "DTC配置表", dictHead, listDtcByte, listDtc, dbc[0][5].ToString());
        }

        private void GetCANSig()
        {
            Dictionary<string, string> dictHead = new Dictionary<string, string>();
            Dictionary<string, string> dictBody = new Dictionary<string, string>();
            List<Dictionary<string, string>> dictEvent = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> dtc = new List<Dictionary<string, string>>();
            string folder = GlobalVar.CurrentTsNode.Aggregate("", (current, nodetype) => current + (nodetype + "-"));
            folder = folder.Remove(folder.Length - 1);
            string testType = GlobalVar.CurrentTsNode[2].ToString().Trim();
            foreach (Dictionary<string, string> content in dictContent)
            {
                if (content["TestChannel"] == GlobalVar.CurrentTsNode[3] && content["DUTname"] == _nodeList[0])
                {
                    dictEvent = Json.DerJsonToLDict(content["EventRelevant"]);
                    string channel = GlobalVar.CurrentTsNode[2].ToString().Trim();
                    if (channel.Contains("通信DTC"))
                    {
                        dictHead.Add("CddFileName", content["CddFileName"]);
                    }
                    if (channel.Contains("集成"))
                    {
                        if (channel.Contains("B(CF)-CAN)"))
                        {
                            dictHead.Add("TestChannel", "1");
                        }
                        if (channel.Contains("CH-CAN"))
                        {
                            dictHead.Add("TestChannel", "2");
                        }
                        if (channel.Contains("PT(E)-CAN)"))
                        {
                            dictHead.Add("TestChannel", "3");
                        }
                        if (channel.Contains("I-CAN"))
                        {
                            dictHead.Add("TestChannel", "4");
                        }
                        if (channel.Contains("D-CAN"))
                        {
                            dictHead.Add("TestChannel", "5");
                        }
                    }
                    else
                    {
                        dictHead.Add("TestChannel", "1");
                    }
                    if (dictBaud.ContainsKey(GlobalVar.CurrentTsNode[3]))
                        dictHead.Add("TestBaudRate", dictBaud[GlobalVar.CurrentTsNode[3]].ToString());
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict.Add("Name", content["SlaveboxID"]);
                    var nodeCfg = _store.GetSpecialByEnum(EnumLibrary.EnumTable.NodeConfigurationBox, dict);
                    if (nodeCfg.Count == 0)
                    {
                        MessageBox.Show("当前节点配置盒不存在，请检查...");
                        return;

                    }
                    dictBody.Add("Name", content["DUTname"]);
                    //dictBody.Add("SlaveboxID", nodeCfg[0][1].ToString());//在单节点中不存在
                    dictBody.Add("TerminalR", content["TerminalR"]);
                    if (testType != "间接NM单元")
                    {
                        dictBody.Add("NMBaseAddress", content["NMBaseAddress"]);
                        dictBody.Add("NMStationAddress", content["NMStationAddress"]);
                    }
                    dictBody.Add("SystemType", "0");
                    if (content.ContainsKey("SystemType"))
                    {
                        dictBody["SystemType"] = content["SystemType"].ToLower() == "12v" ? "0" : "1";
                    }
                    dictBody.Add("VirMsgNum", content["VirMsgNum"]);
                    dictBody.Add("LocalEventNum", dictEvent.Count.ToString());
                    for (int j = 0; j < dictEvent.Count; j++)
                    {
                        dictBody.Add("LocalEvent[" + j + "]", dictEvent[j]["LocalEventIO"].ToString());
                        dictBody.Add("LocalEventValid[" + j + "]", dictEvent[j]["EnableLevel"].ToString());
                    }
                    string[] result = null;
                    switch (testType)
                    {
                        case "CAN通信单元":
                            dictBody.Add("NodeType", content["NodeNetworkAttribute"]);
                            dictBody.Add("CRCType", content["CRCType"]);
                            dictBody.Add("EngineStartRelated", content["EngineStartRelated"]);
                            break;
                        case "直接NM单元":
                            dictBody.Add("NodeType", content["NodeNetworkAttribute"]);
                            result = (content["VirNodeID"].ToString()).Split(',');
                            for (int i = 0; i < result.Length; i++)
                            {
                                dictBody.Add("VirNodeID[" + i + "]", result[i].ToString());
                            }
                            dictBody.Add("VirNodeNum", result.Length.ToString());
                            break;
                        case "动力域NM主节点":
                            result = (content["VirNodeID"].ToString()).Split(',');
                            for (int i = 0; i < result.Length; i++)
                            {
                                dictBody.Add("VirNodeID[" + i + "]", result[i].ToString());
                            }
                            dictBody.Add("VirNodeNum", result.Length.ToString());
                            break;
                        case "动力域NM从节点":
                            result = (content["VirNodeID"].ToString()).Split(',');
                            for (int i = 0; i < result.Length; i++)
                            {
                                dictBody.Add("VirNodeID[" + i + "]", result[i].ToString());
                            }
                            dictBody.Add("VirNodeNum", result.Length.ToString());
                            break;
                        case "通信DTC":
                            dictBody.Remove("NMBaseAddress");
                            dictBody.Remove("NMStationAddress");
                            dictBody.Add("RequestID", content["RequestID"]);
                            dictBody.Add("ResponseID", content["ResponseID"]);
                            var dictAllTemp = Json.DerJsonToDict(content["DTCRelevant"])==null?new Dictionary<string, string>(): Json.DerJsonToDict(content["DTCRelevant"]);
                            //dict = Json.DerJsonToLDict(content["DTCRelevant"]);

                            var listdictECUDTCInfo = dictAllTemp.ContainsKey("ECUDTCInfo")
                                ? Json.DerJsonToLDict(dictAllTemp["ECUDTCInfo"])
                                : new List<Dictionary<string, string>>();
                            var dictECUGlobalVar = dictAllTemp.ContainsKey("ECUGlobalVar")
                                ? Json.DerJsonToDict(dictAllTemp["ECUGlobalVar"])
                                : new Dictionary<string, string>();
                            dtc = listdictECUDTCInfo;
                            foreach (var keyECUGlobalVar in dictECUGlobalVar)
                            {
                                if (keyECUGlobalVar.Key == "NMawake")
                                {
                                    int intResult = keyECUGlobalVar.Value.Trim() == "是" ? 0 : 1;
                                    dictBody.Add(keyECUGlobalVar.Key, intResult.ToString());
                                }
                                else
                                {
                                    dictBody.Add(keyECUGlobalVar.Key, keyECUGlobalVar.Value);
                                }
                            }
                            //下面DTC数据的部分
                            int intRollingCounter = 0;//RollingCounter
                            int intCheckSum = 0;//CheckSum
                            int intCooperateMsg = 0;//节点丢失，伙伴节点DTC数量
                            for (int i = 0; i < dtc.Count; i++)
                            {
                                //dictBody.Add("DTC[" + i + "]", dtc[i]["DTC"].ToString());
                                //dictBody.Add("DTCHEX[" + i + "]", dtc[i]["DTCHEX"].ToString());
                                if (dtc[i]["FaultType"].ToString().Trim() == "欠压故障")
                                {
                                    //dictBody.Add("FaultType[" + i + "]", "1");
                                    dictBody.Add("LowVolDtc", dtc[i]["DTC"].ToString());
                                    dictBody.Add("LowVolDtcHex", dtc[i]["DTCHEX"].ToString());
                                    dictBody.Add("LowVoltagemin", dtc[i]["LowVoltagemin"].ToString());
                                    dictBody.Add("LowVoltagemax", dtc[i]["LowVoltagemax"].ToString());
                                }
                                if (dtc[i]["FaultType"].ToString().Trim() == "过压故障")
                                {
                                    //dictBody.Add("FaultType[" + i + "]", "2");
                                    dictBody.Add("HighVolDtc", dtc[i]["DTC"].ToString());
                                    dictBody.Add("HighVolDtcHex", dtc[i]["DTCHEX"].ToString());
                                    dictBody.Add("UpVoltagemin", dtc[i]["UpVoltagemin"].ToString());
                                    dictBody.Add("UpVoltagemax", dtc[i]["UpVoltagemax"].ToString());
                                }
                                if (dtc[i]["FaultType"].ToString().Trim() == "RollingCounter")//多
                                {
                                    //dictBody.Add("FaultType[" + i + "]", "3");
                                    dictBody.Add("RolDtc[" + intRollingCounter + "]", dtc[i]["DTC"].ToString());
                                    dictBody.Add("RolDtcHex[" + intRollingCounter + "]", dtc[i]["DTCHEX"].ToString());
                                    dictBody.Add("RolDtcID[" + intRollingCounter + "]", dtc[i]["RolDtcID"].ToString());
                                    dictBody.Add("RolDtcCycle[" + intRollingCounter + "]", dtc[i]["RolDtcCycle"].ToString());
                                    intRollingCounter++;
                                }
                                if (dtc[i]["FaultType"].ToString().Trim() == "CheckSum")//多
                                {
                                    //dictBody.Add("FaultType[" + i + "]", "5");
                                    dictBody.Add("CheSumDtc[" + intCheckSum + "]", dtc[i]["DTC"].ToString());
                                    dictBody.Add("CheSumDtcHex[" + intCheckSum + "]", dtc[i]["DTCHEX"].ToString());
                                    dictBody.Add("CheSumID[" + intCheckSum + "]", dtc[i]["CheSumID"].ToString());
                                    dictBody.Add("CheSumCycle[" + intCheckSum + "]", dtc[i]["CheSumCycle"].ToString());
                                    intCheckSum++;
                                }
                                if (dtc[i]["FaultType"].ToString().Trim() == "BusOff")
                                {
                                    //dictBody.Add("FaultType[" + i + "]", "6");
                                    dictBody.Add("BusOffDtc", dtc[i]["DTC"].ToString());
                                    dictBody.Add("BusOffDtcHex", dtc[i]["DTCHEX"].ToString());
                                    dictBody.Add("BusOffTime", dtc[i]["BusOffNum"].ToString());
                                }
                                if (dtc[i]["FaultType"].ToString().Trim() == "节点丢失")//多
                                {
                                    //dictBody.Add("FaultType[" + i + "]", "4");
                                    dictBody.Add("CooperateMsgDTC[" + intCooperateMsg + "]"+".DtcName", dtc[i]["DTC"].ToString());
                                    dictBody.Add("CooperateMsgDTC[" + intCooperateMsg + "]" + ".DtcHex", dtc[i]["DTCHEX"].ToString());
                                    dictBody.Add("CooperateMsg[" + intCooperateMsg + "]" + ".ECUname", dtc[i]["ECUname"].ToString());
                                    var strIdCycleTimeArray = dtc[i]["MsgDTCIDCycleTime"].ToString().Split(',');
                                    int intCooperateMsgIDNum = 0;
                                    for (int j = 0; j < strIdCycleTimeArray.Length; j++)
                                    {
                                        if(strIdCycleTimeArray[j].Trim()==string.Empty)
                                            continue;
                                        var strIdCycleTimeSplit = strIdCycleTimeArray[j].Split('/');
                                        dictBody.Add("CooperateMsgDTC[" + intCooperateMsg + "]" + ".ID[" + j + "]",
                                            strIdCycleTimeSplit.Length >= 2 ? strIdCycleTimeSplit[0] : string.Empty);
                                        dictBody.Add(
                                            "CooperateMsgDTC[" + intCooperateMsg + "]" + ".CycleTime[" + j + "]",
                                            strIdCycleTimeSplit.Length >= 2 ? strIdCycleTimeSplit[1] : string.Empty);
                                        intCooperateMsgIDNum++;
                                    }
                                    dictBody.Add("CooperateMsgDTC[" + intCooperateMsg + "]" + ".CooperateMsgIDNum", intCooperateMsgIDNum.ToString());
                                    intCooperateMsg++;
                                }
                            }
                            dictBody.Add("CooperateMsgDTCNum", intCooperateMsg.ToString());
                            break;
                    }
                    List<string> virNode = _file.GetVirNode(_nodeColl, dbc[0][5].ToString());
                    _file.CreateCfginiFromCANS("emmm...这是个没啥用的字符串", GlobalVar.CurrentTsNode[2], dictHead, dictBody, dictEvent,
                        virNode, dbc[0][5].ToString());
                }
            }
        }

        private void GetGateWaySig()
        {
            string path = string.Empty;
            foreach (Dictionary<string, string> content in dictContent)
            {
                if (content["TestChannel"] == GlobalVar.CurrentTsNode[3] && content["DUTname"] == _nodeList[0])
                {
                    path = content["GatewayPath"];
                }
            }

            string error = _file.CreateCfginiFromGateway("文件夹名称，暂时未用", GlobalVar.CurrentTsNode[2], path);
            if (error != string.Empty)
                throw new Exception("网关路由表ini生成错误。",new Exception(error));
        }


        private void GetLINSig()
        {
            Dictionary<string, string> dictHead = new Dictionary<string, string>();
            Dictionary<string, string> dictBody = new Dictionary<string, string>();
            List<Dictionary<string, string>> dictEvent = new List<Dictionary<string, string>>();
            string folder = GlobalVar.CurrentTsNode.Aggregate("", (current, nodetype) => current + (nodetype + "-"));
            folder = folder.Remove(folder.Length - 1);
            foreach (Dictionary<string, string> content in dictContent)
            {
                if (content["TestChannel"] == GlobalVar.CurrentTsNode[3] && content["DUTname"] == _nodeList[0])
                {
                    string nodeType = content["MasterNodeType"];
                    dictEvent = Json.DerJsonToLDict(content["EventRelevant"]);
                    string channel = GlobalVar.CurrentTsNode[3].ToString().Trim();
                    if (GlobalVar.CurrentTsNode[2].ToString().Trim().Contains("集成"))
                    {
                        if (channel.Contains("LIN1")) {
                            dictHead.Add("TestChannel", "1");
                        }
                        if (channel.Contains("LIN2"))
                        {
                            dictHead.Add("TestChannel", "2");
                        }
                        if (channel.Contains("LIN3"))
                        {
                            dictHead.Add("TestChannel", "3");
                        }
                    }
                    else {
                        dictHead.Add("TestChannel", "1");
                    }
                    dictHead.Add("TestBaudRate", dictBaud[GlobalVar.CurrentTsNode[3]].ToString());
                    Dictionary<string, object> dict = new Dictionary<string, object>(); 
                   // var nodeCfg = _store.GetSpecialByEnum(EnumLibrary.EnumTable.NodeConfigurationBox, dict);
                    dictBody.Add("Name", content["DUTname"]); 
                    if (content["SystemType"].ToString().Trim() == "12V")
                    {
                        dictBody.Add("SystemType", "0");
                    }
                    else {
                        dictBody.Add("SystemType", "1");
                    }
                    if (content["MasterNodeType"].ToString().Trim() == "从节点")
                    {
                        if (content["Crystal"].ToString().Trim() == "有")
                        {
                            dictBody.Add("Crystal", "1");
                        }
                        else
                        {
                            dictBody.Add("Crystal", "0");
                        }
                        //dictBody.Add("SlaveID", content["DUTID"]);
                    }
                    //else
                    //{
                    //    dictBody.Add("MasterID", content["DUTID"]);
                    //}
                    List<string> nodeCopy = new List<string>();
                    nodeCopy.AddRange(_nodeColl); 

                    dictBody.Add("LocalEventNum", dictEvent.Count.ToString());
                    for (int j = 0; j < dictEvent.Count; j++)
                    {
                        dictBody.Add("LocalEvent[" + j + "]", dictEvent[j]["LocalEventIO"].ToString());
                        dictBody.Add("LocalEventValid[" + j + "]", dictEvent[j]["EnableLevel"].ToString());
                    }
                    foreach (string no in nodeCopy)
                    {
                        if (no == content["DUTname"])
                            _nodeColl.Remove(no);
                    }
                    List<string> virNode = _file.GetVirNode(_nodeColl, dbc[0][5].ToString());
                    if(nodeType  == "从节点")
                        _file.CreateCfginiFromLINS(folder, GlobalVar.CurrentTsNode[2], dictHead, dictBody, dictEvent, virNode, dbc[0][5].ToString(),true);
                    else
                        _file.CreateCfginiFromLINS(folder, GlobalVar.CurrentTsNode[2], dictHead, dictBody, dictEvent, virNode, dbc[0][5].ToString(), false);
                }
            }
        }

        private void GetLtg()
        {
            int nodeSum = _nodeList.Count;
            Dictionary<string, string> dictHead = new Dictionary<string, string>();
            dictHead["DUTNum"] = nodeSum.ToString();
            var dictTemp = new Dictionary<string, object>();
            dictTemp["SegmentName"] = GlobalVar.CurrentTsNode[3];
            var segmentList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Segment, dictTemp);
            dictHead["TestChannel"] = segmentList.Count > 0 ? segmentList[0][2].ToString() : 1.ToString();
            dictHead["TestBaudRate"] = dictBaud.ContainsKey(GlobalVar.CurrentTsNode[3])
                ? dictBaud[GlobalVar.CurrentTsNode[3]]
                : 0.ToString();

            List<Dictionary<string, string>> listDictBody = new List<Dictionary<string, string>>();
            List<List<Dictionary<string, string>>> listDictEvent = new List<List<Dictionary<string, string>>>();
            string folder = "";
            for (int i = 0; i < GlobalVar.CurrentTsNode.Count - 1;i++)
                folder = folder + (GlobalVar.CurrentTsNode[i] + "-");
            folder = folder + "CAN集成";
            string testType = GlobalVar.CurrentTsNode[2].ToString().Trim().ToUpper();
            foreach (var node in _nodeList)
            {
                Dictionary<string, string> dictBody = new Dictionary<string, string>();
                List<Dictionary<string, string>> listdictEvent = new List<Dictionary<string, string>>();
                foreach (Dictionary<string, string> content in dictContent)
                {
                    if (content["TestChannel"] == GlobalVar.CurrentTsNode[3] && content["DUTname"] == node)
                    {
                        listdictEvent = Json.DerJsonToLDict(content["EventRelevant"]) ??
                                    new List<Dictionary<string, string>>();
                         
                        //Dictionary<string, object> dict = new Dictionary<string, object>();
                        //dict.Add("Name", content["SlaveboxID"]);
                        //var nodeCfg = _store.GetSpecialByEnum(EnumLibrary.EnumTable.NodeConfigurationBox, dict);

                        dictBody.Add("Name", content["DUTname"]);
                        dictBody.Add("Slavebox", GlobalVar.dictSlaveBoxID[content["DUTname"]]);
                        dictBody.Add("SystemType", 0.ToString());
                        if (content.ContainsKey("SystemType"))
                        {
                            dictBody["SystemType"] = content["SystemType"].ToLower() == "12v" ? "0" : "1";
                        }

                        switch (testType)
                        {
                            case "CAN通信集成":
                                dictBody.Add("TerminalR", content["TerminalR"]);
                                dictBody.Add("NodeType", content["NodeNetworkAttribute"]);
                                dictBody.Add("CRCType", content["CRCType"]);
                                dictBody.Add("EngineStartRelated", content["EngineStartRelated"]);
                                dictBody.Add("VirMsgNum", content["VirMsgNum"]);
                                if (content.ContainsKey("NMBaseAddress"))
                                {
                                    if (content["NMBaseAddress"].ToLower() != string.Empty &&
                                        content["NMBaseAddress"].ToLower() != "--")
                                    {
                                        dictBody.Add("NMBaseAddress", content["NMBaseAddress"]);
                                    }
                                }
                                if (content.ContainsKey("NMStationAddress"))
                                {
                                    if (content["NMStationAddress"].ToLower() != string.Empty &&
                                        content["NMStationAddress"].ToLower() != "--")
                                    {
                                        dictBody.Add("NMStationAddress", content["NMStationAddress"]);
                                    }
                                }
                                break;
                            case "LIN通信集成":
                                dictBody.Add("Type", content["MasterNodeType"]);
                                break;
                            case "直接NM集成":
                            case "动力域NM集成":
                                dictBody.Add("TerminalR", content["TerminalR"]);
                                if (content.ContainsKey("NMBaseAddress"))
                                {
                                    if (content["NMBaseAddress"].ToLower() != string.Empty &&
                                        content["NMBaseAddress"].ToLower() != "--")
                                    {
                                        dictBody.Add("NMBaseAddress", content["NMBaseAddress"]);
                                    }
                                }
                                if (content.ContainsKey("NMStationAddress"))
                                {
                                    if (content["NMStationAddress"].ToLower() != string.Empty &&
                                        content["NMStationAddress"].ToLower() != "--")
                                    {
                                        dictBody.Add("NMStationAddress", content["NMStationAddress"]);
                                    }
                                }
                                if (content.ContainsKey("VirNodeID"))
                                {
                                    if (content["VirNodeID"].ToLower() != string.Empty &&
                                        content["VirNodeID"].ToLower() != "--")
                                    {
                                        var virids = content["VirNodeID"].Split(',');
                                        dictBody.Add("VirMsgNum", virids.Length.ToString());
                                        for (int i = 0; i < virids.Length; i++)
                                        {
                                            dictBody.Add("VirNodeID[" + i + "]", virids[i]);
                                        }
                                    }
                                    else
                                    {
                                        dictBody.Add("VirMsgNum", 0.ToString());
                                    }
                                }
                                else
                                {
                                    dictBody.Add("VirMsgNum", 0.ToString());
                                }
                                break;
                            case "间接NM集成":
                                dictBody.Add("TerminalR", content["TerminalR"]);
                                if (content.ContainsKey("VirNodeID"))
                                {
                                    if (content["VirNodeID"].ToLower() != string.Empty &&
                                        content["VirNodeID"].ToLower() != "--")
                                    {
                                        var virids = content["VirNodeID"].Split(',');
                                        dictBody.Add("VirMsgNum", virids.Length.ToString());
                                        for (int i = 0; i < virids.Length; i++)
                                        {
                                            dictBody.Add("VirNodeID[" + i + "]", virids[i]);
                                        }
                                    }
                                    else
                                    {
                                        dictBody.Add("VirMsgNum", 0.ToString());
                                    }
                                }
                                else
                                {
                                    dictBody.Add("VirMsgNum", 0.ToString());
                                }
                                break;
                            default:
                                break;
                        }
                        

                        #region 此处为添加本地事件，因外部已经有了添加本地事件的方法，此处仅为注释保留

                        //dictBody.Add("LocalEventNum", listdictEvent.Count.ToString());
                        //for (int i = 0; i < listdictEvent.Count; i++)
                        //{
                        //    dictBody.Add("LocalEvent[" + i + "]", listdictEvent[i]["LocalEventIO"].ToString());
                        //    dictBody.Add("LocalEventValid[" + i + "]", listdictEvent[i]["EnableLevel"].ToString());
                        //}

                        #endregion


                        List<string> nodeCopy = new List<string>();
                        nodeCopy.AddRange(_nodeColl);
                        foreach (string no in nodeCopy)
                        {
                            if (no == content["DUTname"])
                                _nodeColl.Remove(no);
                        }
                        listDictBody.Add(dictBody);
                        listDictEvent.Add(listdictEvent);
                    }
                }         
            }
            List<string> virNode = _file.GetVirNode(_nodeColl, dbc[0][5].ToString());
            _file.CreateCfginiFromCANM(folder, GlobalVar.CurrentTsNode[2], dictHead, listDictBody, listDictEvent, virNode, dbc[0][5].ToString());
        }

        private void GetLIN()
        {
            int nodeSum = _nodeList.Count;
            Dictionary<string, string> dictHead = new Dictionary<string, string>();
            dictHead.Add("TestSUTNum", nodeSum.ToString());

            List<Dictionary<string, string>> listDictBody = new List<Dictionary<string, string>>();
            List<List<Dictionary<string, string>>> listDictEvent = new List<List<Dictionary<string, string>>>();
            string folder = "";
            for (int i = 0; i < GlobalVar.CurrentTsNode.Count - 1; i++)
                folder = folder + (GlobalVar.CurrentTsNode[i] + "-");
            folder = folder + "LIN集成";
            foreach (var node in _nodeList)
            {
                Dictionary<string, string> dictBody = new Dictionary<string, string>();
                List<Dictionary<string, string>> dictEvent = new List<Dictionary<string, string>>();
                foreach (Dictionary<string, string> content in dictContent)
                {
                    if (content["TestChannel"] == GlobalVar.CurrentTsNode[3] && content["DUTname"] == node)
                    {
                        dictEvent = Json.DerJsonToLDict(content["EventRelevant"]);
                        string channel = GlobalVar.CurrentTsNode[3].Remove(0, 3);
                        if (!dictHead.ContainsKey("TestChannel"))
                        {
                            dictHead.Add("TestChannel", channel);
                            dictHead.Add("TestBaudRate", dictBaud[GlobalVar.CurrentTsNode[3]]);
                        }
                        Dictionary<string,object> dict = new Dictionary<string, object>();
                        dict.Add("Name", content["SlaveboxID"]);
                        int crystal = ConvertCrystal(content["Crystal"]);
                        int master = ConvertNodeType(content["MasterNodeType"]);
                        var nodeCfg = _store.GetSpecialByEnum(EnumLibrary.EnumTable.NodeConfigurationBox, dict);
                        dictBody.Add("Name", content["DUTname"]);
                        dictBody.Add("SlaveboxID", nodeCfg[0][1].ToString());
                        dictBody.Add("Crystal", crystal.ToString());
                        dictBody.Add("MasterType", master.ToString());
                        List<string> nodeCopy = new List<string>();
                        nodeCopy.AddRange(_nodeColl);
                        foreach (string no in nodeCopy)
                        {
                            if (no == content["DUTname"])
                                _nodeColl.Remove(no);
                        }
                        listDictBody.Add(dictBody);
                        listDictEvent.Add(dictEvent);
                    }
                }
            }
            List<string> virNode = _file.GetLINVirNode(_nodeColl, dbc[0][5].ToString());
            _file.CreateCfginiFromLINM(folder, GlobalVar.CurrentTsNode[2], dictHead, listDictBody, listDictEvent, virNode, dbc[0][5].ToString());
        }

        private int ConvertCrystal(string crystal)
        {
            switch (crystal)
            {
                case "有":
                    return 1;
                case "无":
                    return 0;
                default:
                    return -1;

            }
        }
        private int ConvertNodeType(string masterNodeType)
        {
            switch (masterNodeType)
            {
                case "主节点":
                    return 1;
                case "从节点":
                    return 0;
                default:
                    return -1;

            }
        }

        /// <summary>
        /// 在界面上展示自检结果内容
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, bool> ShowResult(ref List<string> listSelfFalseResult)
        {
            string resultStr = "";
            string cache = "";
            bool selfResult = false;
            listSelfFalseResult = new List<string>();
            Dictionary<string, bool> dictselfResult = new Dictionary<string, bool>();
            dictselfResult["Devinfo"] = true;
            dictselfResult["DUTState"] = true;
            if (!File.Exists(EnumLibrary.SelfPath))
            {
                WriteToLog("----------------------------未读取到自检信息，请检查是否生成自检信息-------------------------------------");
                dictselfResult.Clear();
                return dictselfResult;
            }
            Dictionary<string, List<string>> selfStr = _file.ReadSelfCheckResultFormTxt(EnumLibrary.SelfPath, ref dictselfResult);
            string[] keys = new string[selfStr.Keys.Count];
            selfStr.Keys.CopyTo(keys,0);
            foreach (var key in keys)
            {
                if (selfStr[key].Count == 2)
                {
                    resultStr = "名称：" + key + "    |   型号：" + selfStr[key][0] + "   |    状态：" + selfStr[key][1];
                    if(!selfStr[key][1].ToLower().Contains("normal"))
                        listSelfFalseResult.Add(resultStr);
                }   
                if (selfStr[key].Count == 1)
                {
                    resultStr = "名称：" + key + "     |   状态：" + selfStr[key][0];
                    if (!selfStr[key][0].ToLower().Contains("normal"))
                        listSelfFalseResult.Add(resultStr);
                }   
                WriteToLog(resultStr);
            }
            WriteToLog("----------------------------以上是自检信息-------------------------------------");
            return dictselfResult;
        }

        /// <summary>
        /// 开始执行用例测试
        /// </summary>
        /// <param name="list">用例名称列表</param>
        private void StartExample(object list)
        {
            List<List<string>> nameList = list as List<List<string>>;
            //分成显性用例和隐性用例两种
            List<string> hNamelist = nameList[0];
            List<string> DNamelist = nameList[1];
            int hNameCount = 0;
            int DNameCount = 0;
            foreach (var hName in hNamelist)
            {
                int iTestCount = 1;
                if (hName.Split('@').Length >= 2)
                {
                    int.TryParse(hName.Split('@')[1], out iTestCount);
                }
                hNameCount += iTestCount;
            }
            foreach (var DName in DNamelist)
            {
                int iTestCount = 1;
                if (DName.Split('@').Length >= 2)
                {
                    int.TryParse(DName.Split('@')[1], out iTestCount);
                }
                DNameCount += iTestCount;
            }

            CANoeTest.GetAllCount(hNameCount + DNameCount);
            //得到DTC
            Dictionary<string, string> dtcInfo = DtcInfo();
            //如果没有隐性用例则直接运行显性用例
            if (hNamelist.Count == 0)
            {
                //监控用例进度并根据进度改变界面上的数据或颜色
                thread = new Thread(MonitorDExample);
                threadSet = new Thread(SetDResult);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                threadSet.SetApartmentState(ApartmentState.STA);
                threadSet.Start();

                //和CANoe交互执行用例测试过程
                int result = CANoeTest.StartDExampleCheck(DNamelist, dtcInfo, false);
                if (result == -2)
                {
                    XtraMessageBox.Show("交互过程出现无法预知的错误...请重试....");
                }
            }
            //否则先运行隐性用例然后运行显性用例
            else
            {
                threadH = new Thread(MonitorHExample);
                threadHSet = new Thread(SetHResult);
                threadH.SetApartmentState(ApartmentState.STA);
                threadH.Start();

                threadHSet.SetApartmentState(ApartmentState.STA);
                threadHSet.Start();
                int result = CANoeTest.StartHExampleCheck(hNamelist, dtcInfo);
                if (result == -2)
                {
                    XtraMessageBox.Show("交互过程出现无法预知的错误...请重试....");
                    return;
                }
                bool isHExample = false;
                if (DNamelist.Count == 0)
                {
                    isHExampleReport = true;
                    CANoeTest.PauseCANoe();
                    XtraMessageBox.Show(@"后续无显性用例，测试已结束....");
                }
                //隐性结束后若不尽兴显性用例在弹框中选否则结束此次测试
                else if (XtraMessageBox.Show("隐性用例已完成是否继续？", "提示", MessageBoxButtons.OKCancel,
                             MessageBoxIcon.Asterisk) ==
                         DialogResult.OK)
                {
                    string strReportPath = _file.IfFolderExistSiftExtension(report, "xml");
                    if (string.IsNullOrEmpty(strReportPath))
                    {
                        GlobalVar.AutoReport = _file.AnalysisXml(strReportPath);
                        #region Excel报告相关
                        GlobalVar.ExcelCoverReport = _file.AnalysisXmlReportCover(strReportPath);
                        GlobalVar.ExcelReport = _file.AnalysisXmlReport(strReportPath);
                        GlobalVar.ExcelReportPath = _file.AnalysisXmlReportPath(strReportPath);
                        //File.Delete(strReportPath);
                        #endregion
                    }

                    Thread thread = new Thread(MonitorDExample);
                    Thread threadSet = new Thread(SetDResult);
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();

                    threadSet.SetApartmentState(ApartmentState.STA);
                    threadSet.Start();

                    int resultD = CANoeTest.StartDExampleCheck(DNamelist, dtcInfo, true);
                    if (resultD == -2)
                    {
                        XtraMessageBox.Show("交互过程出现无法预知的错误...请重试....");
                    }
                }
                else
                {
                    CANoeTest.PauseCANoe();
                    WriteToLog("用例测试已经被终止...");
                    if (
                        XtraMessageBox.Show("是否关闭CANoe软件？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) ==
                        DialogResult.OK)
                    {
                        CANoeTest.CloseCANoe();
                    }
                }
            }
            CANoeTest.IsEndDevice = false;
            CANoeTest.IsEndPro = false;
            pictStart.Enabled = false;
        }

        private Dictionary<string, string> DtcInfo()
        {
            Dictionary < string, object> dictCon = new Dictionary<string, object>();
            Dictionary<string, string> dictDTC = new Dictionary<string, string>();
            string[] vehicel = GlobalVar.CurrentTsNode[0].Split('-');
            dictCon.Add("TaskNo", GlobalVar.CurrentTsNode[0]);
            dictCon.Add("TaskRound", GlobalVar.CurrentTsNode[1]);
            dictCon.Add("TaskName", GlobalVar.CurrentTsNode[2]);
            dictCon.Add("CANRoad", GlobalVar.CurrentTsNode[3]);
            dictCon.Add("Module", GlobalVar.ModuleJson);
            dictCon.Add("VehicelType", vehicel[0]);
            dictCon.Add("VehicelConfig", vehicel[1]);
            dictCon.Add("VehicelStage", vehicel[2]);
            IList<object[]> taskList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.TaskTest, dictCon);
            _testType = taskList[0][5].ToString();
            _suppier = taskList[0][10].ToString();
            if (taskList[0][14].ToString() == "DTC")
            {
                dictCon.Add("ExapTableName", "通信DTC用例表");
                 dictDTC = _search.SearchDtcDefaltInfor(dictCon, taskList[0][11].ToString());
            }
            return dictDTC;

        }

        //监控隐性用例执行过程
        private void MonitorHExample()
        {
            while (!CANoeTest.endHFlag)
            {
                if (CANoeTest._dictHExampleCache.Count > 0 && CANoeTest.ExampleAllCount > 0)
                {

                    string[] caches = new string[CANoeTest._dictHExampleCache.Count];
                    CANoeTest._dictHExampleCache.CopyTo(caches, 0);

                    string[] names = new string[CANoeTest._dictHNameList.Count];
                    CANoeTest._dictHNameList.Keys.CopyTo(names, 0);
                    // ReSharper disable once PossibleLossOfFraction


                    foreach (var log in caches)
                    {
                        WriteToLog(log);
                        CANoeTest._dictHExampleCache.Remove(log);
                    }

                    int count = Convert.ToInt32((names.Length * 1.0 / (CANoeTest.ExampleAllCountCopy * 1.0)) *
                                                _remainCount);
                    foreach (var name in names)
                        CANoeTest._dictHNameList.Remove(name);
                    Calculate(count);
                }

                Thread.Sleep(500);
            }

            WriteToLog("----------------------------以上是隐性测试信息-------------------------------------");

            Thread.Sleep(2000);
            if (!isClose && isHExampleReport)
            {
                isHExampleReport = false;
                Check();
                //AddReport();
                try
                {
                    Thread cexcelThread = new Thread(AddExcelReport);
                    cexcelThread.Start();
                    //AddExcelReport();
                }
                catch (Exception e)
                {
                    XtraMessageBox.Show("报告生成错误，请联系维护人员。\r\n错误信息：" + e.Message);
                }
            }
        }

        //监控显性用例执行过程
        private void MonitorDExample()
        {

            while (!CANoeTest.endDFlag)
            {
                if (CANoeTest._dictDExampleCache.Count > 0 && CANoeTest.ExampleAllCount > 0)
                {

                    string[] caches = new string[CANoeTest._dictDExampleCache.Count];
                    CANoeTest._dictDExampleCache.CopyTo(caches, 0);

                    string[] names = new string[CANoeTest._dictDNameList.Count];
                    CANoeTest._dictDNameList.Keys.CopyTo(names, 0);
                    // ReSharper disable once PossibleLossOfFraction


                    foreach (var log in caches)
                    {
                        WriteToLog(log);
                        CANoeTest._dictDExampleCache.Remove(log);
                    }
                    int count = Convert.ToInt32((names.Length * 1.0 / (CANoeTest.ExampleAllCountCopy * 1.0)) *
                                                _remainCount);
                    foreach (var name in names)
                        CANoeTest._dictDNameList.Remove(name);
                    Calculate(count);
                }
                Thread.Sleep(500);
            }

            WriteToLog("----------------------------以上是显性测试信息-------------------------------------");

            Thread.Sleep(2000);
            if (!isClose)
            {
                Check();
                //AddReport();
                try
                {
                    Thread cexcelThread = new Thread(AddExcelReport);
                    cexcelThread.Start();
                    //AddExcelReport();
                }
                catch (Exception e)
                {
                    XtraMessageBox.Show("报告生成错误，请联系维护人员。\r\n错误信息：" + e.Message);
                }
            }
        }



        /// <summary>
        /// 收集一键上传需要的数据并将所有文件压缩
        /// </summary>
        private void CollectDataToXml()
        {
            string vehielType = GlobalVar.CurrentTsNode[0].Split('-')[0];
            string vehieConfig = GlobalVar.CurrentTsNode[0].Split('-')[1];
            string vehielStage = GlobalVar.CurrentTsNode[0].Split('-')[2];
            string testRound = GlobalVar.CurrentTsNode[1];
            string testName = GlobalVar.CurrentTsNode[2];
            string CANRoad = GlobalVar.CurrentTsNode[3];
            string testNode = GlobalVar.CurrentTsNode[4];
            string Suppier =_suppier;
            string testDate = txtDate.Text;
            string testType = _testType;
            string testPlace = txtTestPosition.Text;
            string testUser = GlobalVar.UserName;
            _file.UpdateLocalXml(@"xml\testInfo.xml", @"Product/VehielType", vehielType);
            _file.UpdateLocalXml(@"xml\testInfo.xml", @"Product/VehieConfig", vehieConfig);
            _file.UpdateLocalXml(@"xml\testInfo.xml", @"Product/VehielStage", vehielStage);
            _file.UpdateLocalXml(@"xml\testInfo.xml", @"Product/TestName", testName);
            _file.UpdateLocalXml(@"xml\testInfo.xml", @"Product/TestRound", testRound);
            _file.UpdateLocalXml(@"xml\testInfo.xml", @"Product/CANRoad", CANRoad);
            _file.UpdateLocalXml(@"xml\testInfo.xml", @"Product/TestNode", testNode);
            _file.UpdateLocalXml(@"xml\testInfo.xml", @"Product/Suppier", Suppier);
            _file.UpdateLocalXml(@"xml\testInfo.xml", @"Product/TestDate", testDate);
            _file.UpdateLocalXml(@"xml\testInfo.xml", @"Product/TestType", testType);
            _file.UpdateLocalXml(@"xml\testInfo.xml", @"Product/TestPlace", testPlace);
            _file.UpdateLocalXml(@"xml\testInfo.xml", @"Product/TestUser", testUser);
        }
        
        private void CopyAndZip()
        {
            string targetPath = _file.CreateFolder("backup");
            string srcExamDir = AppDomain.CurrentDomain.BaseDirectory + "\\configuration\\exampleTest\\";
            string srcSelfDir = AppDomain.CurrentDomain.BaseDirectory + "\\configuration\\selfCheck\\IncludeFiles";
            string srcXmlDir = AppDomain.CurrentDomain.BaseDirectory + "\\xml\\";
            string srcInIDir = AppDomain.CurrentDomain.BaseDirectory + "\\configini\\" + GlobalVar.CurrentTsNode[2] + "\\";
            _file.CopyDirectory(srcExamDir + "screenShot", targetPath);
            File.Copy(srcExamDir + "TG1_TC1_report.html", targetPath + "TG1_TC1_report.html",true);
            File.Copy(srcExamDir + "TG1_TC1_report.xml", targetPath + "TG1_TC1_report.xml", true);
            File.Copy(srcExamDir + "log.asc", targetPath + "log.asc", true);

            File.Copy(srcXmlDir + "testInfo.xml", targetPath + "testInfo.xml", true);

            File.Copy(srcSelfDir + "\\DevInfo.ini", targetPath + "DevInfo.ini", true);
            _file.CopyDirectory(srcInIDir, targetPath);
            _file.ZipFile(targetPath,AppDomain.CurrentDomain.BaseDirectory + "backup.zip");
        }
        //变更界面上隐性用例的颜色和内容
        private void SetHResult()
        {
            while (!CANoeTest.endHFlag)
            {
                if (CANoeTest._dictHNameListCopy.Count == 0)
                    continue;
                Debug.Print("进入变更颜色流程 隐性");
                string[] nameList = new string[CANoeTest._dictHNameListCopy.Count];
                CANoeTest._dictHNameListCopy.Keys.CopyTo(nameList, 0);
                for (int i = 0; i < nameList.Length; i++)
                {
                    int[] rows = gvAuto.GetSelectedRows();
                    foreach (var row in rows)
                    {
                        if (row < 0) continue;
                        DataRow dr = gvAuto.GetDataRow(row);
                        string nameId = dr["ExapID"].ToString();
                        if (nameId == nameList[i].Split('@')[0])
                        {
                            if (CANoeTest.span.Count > 0 && CANoeTest._dictHNameListCopy.Count > 0)
                            {
                                string strDebug = nameList[i] + " : " + CANoeTest._dictHNameListCopy[nameList[i]];
                                Debug.Print("隐性变更颜色时=" + strDebug);
                                string timeStr = CANoeTest.span[0].TotalSeconds.ToString(CultureInfo.InvariantCulture);
                                if (CANoeTest._dictHNameListCopy[nameList[i]] == 0)
                                {
                                    SetGrid(row, "TestResult", "失败", "TestTime", timeStr);
                                    SetCol(row, "TestUpload", "未上传");
                                }

                                else if (CANoeTest._dictHNameListCopy[nameList[i]] == 1)
                                {
                                    SetGrid(row, "TestResult", "成功", "TestTime", timeStr);
                                    SetCol(row, "TestUpload", "无需上传");
                                }

                                else if (CANoeTest._dictHNameListCopy[nameList[i]] == -1)
                                {
                                    SetGrid(row, "TestResult", "未执行", "TestTime", "0");
                                    SetCol(row, "TestUpload", "无需上传");
                                }
                                if (i + 1 >= nameList.Length)
                                    ShowExID("用例已执行完毕...");
                                else
                                    ShowExID(nameList[i + 1]);
                                if (CANoeTest.span.Count > 0)
                                {
                                    if (CANoeTest.span.Contains(CANoeTest.span[0]))
                                    {
                                        lock (CANoeTest.span)
                                            CANoeTest.span.RemoveAt(0);
                                    }
                                }
                            }
                            if (CANoeTest._dictHNameListCopy.ContainsKey(nameList[i]))
                            {
                                lock (CANoeTest._dictHNameListCopy)
                                {
                                    CANoeTest._dictHNameListCopy.Remove(nameList[i]);
                                    Debug.Print("隐性删除了_dictHNameListCopy内的=" + nameList[i]);
                                }
                            }
                            break;
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }
        //变更界面上显性用例的颜色和内容
        private void SetDResult()
        {
            while (!CANoeTest.endDFlag)
            {
                if (CANoeTest._dictDNameListCopy.Count == 0)
                    continue;
                Debug.Print("进入变更颜色流程");
                string[] nameList = new string[CANoeTest._dictDNameListCopy.Count];
                CANoeTest._dictDNameListCopy.Keys.CopyTo(nameList, 0);
                for (int i = 0; i <nameList.Length;i++)
                {
                    int[] rows = gvAuto.GetSelectedRows();
                    foreach (var row in rows)
                    {
                        if (row < 0) continue;
                        DataRow dr = gvAuto.GetDataRow(row);
                        string nameId = dr["ExapID"].ToString();
                        if (nameId == nameList[i].Split('@')[0])
                        {
                            if (CANoeTest.span.Count > 0 && CANoeTest._dictDNameListCopy.Count > 0)
                            {
                                string strDebug = nameList[i] + " : " + CANoeTest._dictDNameListCopy[nameList[i]];
                                Debug.Print("变更颜色时="+strDebug);
                                string timeStr = CANoeTest.span[0].TotalSeconds.ToString(CultureInfo.InvariantCulture);
                                if (CANoeTest._dictDNameListCopy[nameList[i]] == 0)
                                {
                                    SetGrid(row, "TestResult", "失败", "TestTime", timeStr);
                                    SetCol(row, "TestUpload", "未上传");
                                }

                                else if (CANoeTest._dictDNameListCopy[nameList[i]] == 1)
                                {
                                    SetGrid(row, "TestResult", "成功", "TestTime", timeStr);
                                    SetCol(row, "TestUpload", "无需上传");
                                }

                                else if (CANoeTest._dictDNameListCopy[nameList[i]] == -1)
                                {
                                    SetGrid(row, "TestResult", "未执行", "TestTime", "0");
                                    SetCol(row, "TestUpload", "无需上传");
                                }
                                if (i + 1 >= nameList.Length)
                                    ShowExID("用例已执行完毕...");
                                else
                                    ShowExID(nameList[i + 1]);
                                if (CANoeTest.span.Count > 0)
                                {
                                    if (CANoeTest.span.Contains(CANoeTest.span[0]))
                                    {
                                        lock (CANoeTest.span)
                                            CANoeTest.span.RemoveAt(0);
                                    }
                                }
                                if (CANoeTest._dictDNameListCopy.ContainsKey(nameList[i]))
                                {
                                    lock (CANoeTest._dictDNameListCopy)
                                    {
                                        CANoeTest._dictDNameListCopy.Remove(nameList[i]);
                                        Debug.Print("删除了_dictDNameListCopy内的=" + nameList[i]);
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }

        #region 委托方法
        public void WriteToLog(string content)
        {
            if (InvokeRequired)
            {
                ShowMsgCount smh = WriteToLog;
                Invoke(smh, content);
            }
            else
            {
                try
                {
                    txtLog.Text += content + "\r\n";
                    txtLog.Focus();//获取焦点
                    txtLog.Select(txtLog.SelectionLength, 0);//光标定位到文本最后
                    txtLog.ScrollToCaret();//滚动到光标处
                }
                catch
                {
                    //
                }
            }
        }

        public void Calculate(int value)
        {
            if (InvokeRequired)
            {
                ShowStep smh = Calculate;
                Invoke(smh, value);
            }
            else
            {
                try
                {
                    testPro.Position += value;
                }
                catch
                {
                    //
                }
            }
        }

        public void Reset()
        {
            if (InvokeRequired)
            {
                ResetPos smh = Reset;
                Invoke(smh);
            }
            else
            {
                try
                {
                    testPro.Position = 0;
                }
                catch
                {
                    //
                }
            }
        }

        public void Check()
        {
            if (InvokeRequired)
            {
                CheckPos smh = Check;
                Invoke(smh);
            }
            else
            {
                try
                {
                    if(testPro.Position != 100)
                        testPro.Position = 100;
                }
                catch
                {
                    //
                }
            }
        }

        public void SetGrid(int row, string col, string value,string coltime,string time)
        {
            if (InvokeRequired)
            {
                ShowGrid smh = SetGrid;
                Invoke(smh, row, col, value, coltime,time);
            }
            else
            {
                try
                {
                    gvAuto.SetRowCellValue(row, col, value);
                    gvAuto.SetRowCellValue(row, coltime, time);
                }
                catch
                {
                    //
                }
            }
        }

        public void SetCol(int row, string col, string value)
        {
            if (InvokeRequired)
            {
                ShowReport smh = SetCol;
                Invoke(smh, row, col, value);
            }
            else
            {
                try
                {
                    gvAuto.SetRowCellValue(row, col, value);
                }
                catch
                {
                    //
                }
            }
        }

        public void ResetLog()
        {
            if (InvokeRequired)
            {
                Showtxt smh = ResetLog;
                Invoke(smh);
            }
            else
            {
                try
                {
                    txtLog.Text = "";
                }
                catch
                {
                    //
                }
            }
        }
        #endregion

        #region 用例ID显示

        #endregion
        public void ShowExID(string examID)
        {
            if (InvokeRequired)
            {
                ShowExamID smh = ShowExID;
                Invoke(smh,examID);
            }
            else
            {
                try
                {
                    lblCurrentExam.Text = "[当前正在执行用例：" + examID + "]";
                }
                catch
                {
                    //
                }
            }
        }

        #region 点击事件
        private void ReportLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", "/n, " + _file.IfFolderExistSiftExtension(report,"xml"));
        }


        private void pictTestStart_MouseClick(object sender, MouseEventArgs e)
        {
            if (!ClassifyAandM())
                return;
            if (_isTestStart)
            {
                pictStop_Click(sender, e);
            }
            else
            {
                Reset();
                _isReportCreate = false;
                _isReport = false;
                StartPretreatment();
            }
        }
        private void StartPretreatment()
        {
            try
            {
                try
                {
                    _isTestStart = true;
                    pictStart.Image = Resources.stop;
                    pictStart.Enabled = false;
                    ConvertCfg(); //生成ini
                    WriteToLog("----------------------------以上是INI信息-------------------------------------");
                }
                catch (Exception ex)
                {
                    WriteToLog("生成ini文件异常，异常信息：" + ex.Message + "\r\n详细信息：" + ex.ToString());
                    WriteToLog("----------------------------以上是INI信息-------------------------------------");
                    return;
                }

                if (!isExistDBC)
                    return;
                //string path = EnumLibrary.SelfPath;
                //if (File.Exists(path))
                //    File.Delete(path);
                if (!CANoeTest.StartDeviceSelfCheck())
                {
                    MessageBox.Show("配置文件路径未找到...");
                    //SplashScreenManager.CloseForm();
                    //pictPretreatment.Enabled = true;
                    //pictPretreatment.Image = Resources.Pretreatment;
                    //pictPretreatment.Properties.SizeMode = PictureSizeMode.Clip;
                    return;
                }
                CANoeTest.StartPrototySelfCheck();
                Thread threadPre = new Thread(Pretreatment);
                threadPre.Start();
                btnImportTxtlog.Enabled = true;
            }
            catch (Exception ex)
            {

            }
        }

        private void StartTestStart()
        {
            try
            {
                GloalVar.IsDPause = true;
                GloalVar.IsHPause = true;
                pictStart.Enabled = true;
                isClose = false;
                threadexample = new Thread(StartExample);
                if (!_isRun)
                {
                    if (!_isGet)
                    {
                        //进行设备自检
                        Thread threadDevice = new Thread(SetDeviceInfo);
                        threadDevice.Start();
                        //_isGet = true;
                        Reset();
                        //隐形分类
                        ProcHidden(_dtHidden);
                        if (exampleHNamList.Count == 0)
                            GloalVar.IsHPause = false;
                        //显性分类
                        ProcDominance(_dtDominance);
                        if (exampleDNamList.Count == 0)
                            GloalVar.IsDPause = false;
                    }

                    _isRun = true;

                    lblReport.Enabled = false;
                    threadexample.SetApartmentState(ApartmentState.STA);
                    List<List<string>> nameList = new List<List<string>>();
                    nameList.Add(exampleHNamList);
                    nameList.Add(exampleDNamList);
                    //线程开始
                    threadexample.Start(nameList);
                    IsFisrt = false;
                    if (exampleHNamList.Count == 0)
                        ShowExID(exampleDNamList[0]);
                    else
                        ShowExID(exampleHNamList[0]);
                }

                #region 暂停功能，实现效果不好
                //         else
                //         {
                //             if (
                //                 XtraMessageBox.Show("是否执行暂停操作？", "提示", MessageBoxButtons.OKCancel,
                //                     MessageBoxIcon.Warning) ==
                //                 DialogResult.OK)
                //                 if (
                //XtraMessageBox.Show("是否执行暂停操作？", "提示", MessageBoxButtons.OKCancel,
                //    MessageBoxIcon.Warning) ==
                //DialogResult.OK)
                //                 {
                //                     pictStart.Image = Resources.play;
                //                     _isRun = false;
                //                     CANoeTest.PauseCANoe();
                //                     Thread.Sleep(3000);

                //                     Dictionary<string, List<List<string>>> autoReport = _file.AnalysisXml(report);
                //                     GlobalVar.AutoReportList.Add(autoReport);
                //                     if (thread != null && thread.IsAlive)
                //                         thread.Abort();
                //                     if (threadH != null && threadH.IsAlive)
                //                         threadH.Abort();
                //                     if (threadSet != null && threadSet.IsAlive)
                //                         threadSet.Abort();
                //                     if (threadHSet != null && threadHSet.IsAlive)
                //                         threadHSet.Abort();
                //                     if (threadexample != null && threadexample.IsAlive)
                //                         threadexample.Abort();


                //                 }
                //         }
                #endregion

            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("交互过程出现无法预知的异常，请关闭重试...");
            }
        }
        //测试开始事件
        private void pictStart_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                GloalVar.IsDPause = true;
                GloalVar.IsHPause = true;
                isClose = false;
                threadexample = new Thread(StartExample);
                if (!_isRun)
                {
                    if (!_isGet)
                    {
                        //进行设备自检
                        Thread threadDevice = new Thread(SetDeviceInfo);
                        threadDevice.Start();
                        //_isGet = true;
                        Reset();
                        if (!ClassifyAandM())
                            return;
                        //隐形分类
                        ProcHidden(_dtHidden);
                        if (exampleHNamList.Count == 0)
                            GloalVar.IsHPause = false;
                        //显性分类
                        ProcDominance(_dtDominance);
                        if (exampleDNamList.Count == 0)
                            GloalVar.IsDPause = false;
                    }

                    pictStart.Image = Resources.Pause;
                    _isRun = true;

                    lblReport.Enabled = false;
                    threadexample.SetApartmentState(ApartmentState.STA);
                    List<List<string>> nameList = new List<List<string>>();
                    nameList.Add(exampleHNamList);
                    nameList.Add(exampleDNamList);
                    //线程开始
                    threadexample.Start(nameList);
                    IsFisrt = false;
                    pictStart.Enabled = false;
                    if (exampleHNamList.Count == 0)
                        ShowExID(exampleDNamList[0]);
                    else
                        ShowExID(exampleHNamList[0]);
                }

                #region 暂停功能，实现效果不好
                //         else
                //         {
                //             if (
                //                 XtraMessageBox.Show("是否执行暂停操作？", "提示", MessageBoxButtons.OKCancel,
                //                     MessageBoxIcon.Warning) ==
                //                 DialogResult.OK)
                //                 if (
                //XtraMessageBox.Show("是否执行暂停操作？", "提示", MessageBoxButtons.OKCancel,
                //    MessageBoxIcon.Warning) ==
                //DialogResult.OK)
                //                 {
                //                     pictStart.Image = Resources.play;
                //                     _isRun = false;
                //                     CANoeTest.PauseCANoe();
                //                     Thread.Sleep(3000);

                //                     Dictionary<string, List<List<string>>> autoReport = _file.AnalysisXml(report);
                //                     GlobalVar.AutoReportList.Add(autoReport);
                //                     if (thread != null && thread.IsAlive)
                //                         thread.Abort();
                //                     if (threadH != null && threadH.IsAlive)
                //                         threadH.Abort();
                //                     if (threadSet != null && threadSet.IsAlive)
                //                         threadSet.Abort();
                //                     if (threadHSet != null && threadHSet.IsAlive)
                //                         threadHSet.Abort();
                //                     if (threadexample != null && threadexample.IsAlive)
                //                         threadexample.Abort();


                //                 }
                //         }
                #endregion

            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("交互过程出现无法预知的异常，请关闭重试...");
            }

        }

        private void SetDeviceInfo()
        {
            while (true)
            {
                if (CANoeTest._dictDeviceInfo.Count > 0 && CANoeTest.isStartDevice == 1)
                {
                    txt0xF187.Text = CANoeTest._dictDeviceInfo["sparePartsNumberOfAutomobileManufacturers"];
                    txt0xF188.Text = CANoeTest._dictDeviceInfo["carManufacturerECUSoftware"];
                    txt0xF189.Text = CANoeTest._dictDeviceInfo["softwareVersionNumber"];
                    txt0xF18A.Text = CANoeTest._dictDeviceInfo["systemVendorNameCode"];
                    txt0xF18B.Text = CANoeTest._dictDeviceInfo["ECUManufacturingDate"];
                    txt0xF190.Text = CANoeTest._dictDeviceInfo["VINCode"];
                    txt0xF191.Text = CANoeTest._dictDeviceInfo["carManufacturerECUHardwareNumber"];
                    txt0xF192.Text = CANoeTest._dictDeviceInfo["systemVendorECUHardwareNumber"];
                    txt0xF193.Text = CANoeTest._dictDeviceInfo["systemVendorHardwareVersionNumber"];
                    txt0xF194.Text = CANoeTest._dictDeviceInfo["systemVendorECUSoftware"];
                    txtF18C.Text = CANoeTest._dictDeviceInfo["ECUBatchNumber"];
                    txt0xF195.Text = CANoeTest._dictDeviceInfo["systemVendorECUSoftwareVersionNumber"];
                    _isGet = true;
                    break;
                }
                Thread.Sleep(1000);
            }
        }
        //预编译事件
        private void pictPretreatment_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                pictPretreatment.Enabled = false;
                pictPretreatment.Image = Resources.Pretreatment_unable;
                pictPretreatment.Properties.SizeMode = PictureSizeMode.Clip;
                try
                {
                    ConvertCfg();//生成ini
                }
                catch (Exception ex)
                {
                    WriteToLog("生成ini文件异常，异常信息：" + ex.Message+"\r\n详细信息："+ex.ToString());
                    return;
                }
                if (!isExistDBC)
                    return;
                string path = EnumLibrary.SelfPath;
                if (File.Exists(path))
                    File.Delete(path);
                SplashScreenManager.ShowForm(typeof(wfMain), false, true);
                if (!CANoeTest.StartDeviceSelfCheck())
                {
                    MessageBox.Show("配置文件路径未找到...");
                    SplashScreenManager.CloseForm();
                    pictPretreatment.Enabled = true;
                    pictPretreatment.Image = Resources.Pretreatment;
                    pictPretreatment.Properties.SizeMode = PictureSizeMode.Clip;
                    return;
                }
                CANoeTest.StartPrototySelfCheck();
                SplashScreenManager.CloseForm();
                Thread threadPre = new Thread(Pretreatment);
                threadPre.Start();
                btnImportTxtlog.Enabled = true;
                //XtraMessageBox.Show("软件会唤醒CANoe，请不要手动关闭CANoe");
            }
            catch (Exception ex)
            {
                //Show(defaultLookAndFeel.LookAndFeel, this, ex.ToString(), "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                return;
            }

        }
        #endregion

        #region 用例相关
        /// <summary>
        /// 隐性分类
        /// </summary>
        /// <param name="dtHidden"></param>
        private void ProcHidden(DataTable dtHidden)
        {
            exampleHNamList.Clear();
            for (int i = 0; i < dtHidden.Rows.Count; i++)
            {
                string exapID = dtHidden.Rows[i]["exapID"].ToString() + "@" + dtHidden.Rows[i]["TestCount"].ToString();
                exampleHNamList.Add(exapID);
            }
          
          
        }
        /// <summary>
        /// 显性分类
        /// </summary>
        /// <param name="dtDominance"></param>
        private void ProcDominance(DataTable dtDominance)
        {
            try
            {
                exampleDNamList.Clear();
                for (int i = 0; i < dtDominance.Rows.Count; i++)
                {
                    //string charpter = dtDominance.Rows[i]["Charpter"].ToString();
                    string exapID = dtDominance.Rows[i]["exapID"].ToString() + "@" + dtDominance.Rows[i]["TestCount"].ToString();
                    //var exap = Json.DerJsonToDict(_dictExap[charpter][exapID][0].ToString());
                    exampleDNamList.Add(exapID);
                }

            }
            catch (Exception)
            {
                Show(defaultLookAndFeel.LookAndFeel, this, "CANoe出现问题，请关闭软件重新测试...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
            }

        }

        /// <summary>
        /// 设备自检
        /// </summary>
        private void Pretreatment()
        {
            //try
            //{
                while (true)
                {
                    if (CANoeTest.IsEndDevice && CANoeTest.IsEndPro)
                    {
                        List<string> listSelfFalseResult = new List<string>();
                        Dictionary<string, bool> dictselfResult = ShowResult(ref listSelfFalseResult);
                        //Dictionary<string, object> drTest = new Dictionary<string, object>();
                        //drTest.Add("TaskNo", GlobalVar.CurrentTsNode[0]);
                        //drTest.Add("TaskRound", GlobalVar.CurrentTsNode[1]);
                        //drTest.Add("TaskName", GlobalVar.CurrentTsNode[2]);
                        //drTest.Add("CANRoad", GlobalVar.CurrentTsNode[3]);
                        //drTest.Add("Module", GlobalVar.ModuleJson);

                        //绘制表格
                        //PreDrawGird(selfResult);
                        //SelectRows();
                        //CANoeTest.PauseCANoe();
                        //CANoeTest.CloseCANoe();
                        if (dictselfResult.Count == 0)
                        {
                            Calculate(100);
                            CANoeTest.PauseCANoe();
                            pictStart.Enabled = true;
                            pictStart.Image = Resources.play;
                            _isTestStart = false;
                            CANoeTest.IsEndDevice = false;
                            CANoeTest.IsEndPro = false;
                            return;
                        }
                        if (dictselfResult["Devinfo"] && dictselfResult["DUTState"])
                        {
                            //pictStart.Enabled = true;
                            //pictStart.Image = Resources.play;
                            //pictStart.Properties.SizeMode = PictureSizeMode.Clip;
                            pictStart.Enabled = true;
                            pictStart.Image = Resources.stop;
                            pictStart.Properties.SizeMode = PictureSizeMode.Clip;
                            //SetContorlEnable(true);
                            StartTestStart();
                        }
                        else
                        {
                            string strError = "自检";
                            if (dictselfResult["Devinfo"] == false && dictselfResult["DUTState"] == false)
                            {
                                strError = "设备自检和样件自检";
                            }
                            else if (!dictselfResult["Devinfo"])
                            {
                                strError = "设备自检";
                            }
                            else if (!dictselfResult["DUTState"])
                            {
                                strError = "样件自检";
                            }
                            string strErrors = string.Empty;
                            foreach (var strSelfFalseResult in listSelfFalseResult)
                            {
                                strErrors += (strSelfFalseResult + "\r\n");
                            }
                            XtraMessageBox.Show(strError + "未通过，无法进行用例测试..."+"\r\n"+strErrors);
                            Calculate(100);
                            CANoeTest.PauseCANoe();
                            pictStart.Enabled = true;
                            pictStart.Image = Resources.play;
                            _isTestStart = false;
                            CANoeTest.IsEndDevice = false;
                            CANoeTest.IsEndPro = false;
                            return;
                        }
                        break;
                    }

                    Thread.Sleep(500);
                }
                //Calculate(100); //进度条
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //    throw;
            //}
        }

        private bool ClassifyAandM()
        {
            //if (_dtHidden.Rows.Count != 0 || _dtDominance.Rows.Count != 0)
            //{
            //    _dtHidden.Rows.Clear();
            //    _dtDominance.Rows.Clear();
            //}
            int[] rowList = gvAuto.GetSelectedRows();
            if (rowList.Length == 0)
            {
                MessageBox.Show(@"您未勾选任何测试项....");
                return false;  
            }

            _dtHidden.Rows.Clear();
            _dtDominance.Rows.Clear();
            foreach (int row in rowList)
            {
                if (row < 0)
                    continue;
                DataRow dr = gvAuto.GetDataRow(row);
                if (dr["TestType"].ToString() == "隐性" && dr["TestResult"].ToString() == "--")
                    _dtHidden.Rows.Add(dr.ItemArray);
                else if (dr["TestType"].ToString() == "显性" && dr["TestResult"].ToString() == "--")
                    _dtDominance.Rows.Add(dr.ItemArray);
            }
            //_allTestCount = _dtDominance.Rows.Count + _dtHidden.Rows.Count;
            return true;
            }


        #endregion

        #region 绘制表格
        public void Draw(string module)
        {
            DataTable dt = DrawGird();
            gcAuto.DataSource = dt;
            txtModule.Text = module;
            FillXml();
        }

        private DataTable DrawGird()
        {
            //try
            //{
                //Dictionary<string, string> drTest = new Dictionary<string, string>();
                //drTest.Add("TaskNo", GlobalVar.CurrentTsNode[0]);
                //drTest.Add("TaskRound", GlobalVar.CurrentTsNode[1]);
                //drTest.Add("TaskName", GlobalVar.CurrentTsNode[2]);
                //drTest.Add("CANRoad", GlobalVar.CurrentTsNode[3]);
                //drTest.Add("Module", GlobalVar.ModuleJson);
                //drTest.Add("Module",GetModuleText(GlobalVar.CurrentTsNode[2], GlobalVar.CurrentTsNode[4]));
                //drTest.Add("Module", GlobalVar.CurrentTsNode[4].Split(' ')[0]);
                txtModule.Text = GlobalVar.CurrentTsNode[4];
                txtDate.Text = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                //string examStr = _store.GetExmpJsonByName(drTest);
                Dictionary<string, object> dictExampleTemp = new Dictionary<string, object>();
                dictExampleTemp["MatchSort"] = GlobalVar.CurrentTsNode[2];
                var examVar = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ExampleMatch, dictExampleTemp);
                string examStr = string.Empty;
                if (examVar.Count > 0)
                {
                    if (!string.IsNullOrEmpty(examVar[0][5].ToString().Trim()))
                    {
                        examStr = examVar[0][5].ToString().Trim();
                        _busType = examVar[0][6].ToString().Trim();
                    }
                }
                _dictExap = Json.DeserJsonToDDict(examStr)==null?new Dictionary<string, Dictionary<string, List<object>>>(): Json.DeserJsonToDDict(examStr);

                var dt = new DataTable();
                dt = InitTestCol(dt);
                _dtDominance= InitTestCol(_dtDominance);
                _dtHidden = InitTestCol(_dtHidden);
                string[] charpterKeys = new string[_dictExap.Keys.Count];
                _dictExap.Keys.CopyTo(charpterKeys, 0);
                foreach (string charpter in charpterKeys)
                {
                    string[] exmpKeys = new string[_dictExap[charpter].Keys.Count];
                    _dictExap[charpter].Keys.CopyTo(exmpKeys, 0);
                    int i = 0;
                    #region Excel报告相关
                    Dictionary<string, string> dictTestList = new Dictionary<string, string>();
                    #endregion
                    foreach (var exmp in exmpKeys)
                    {
                       
                        var dictItem = Json.DerJsonToDict(_dictExap[charpter][exmp][0].ToString());
                        dictItem.Add("Charpter", charpter);
                        // ExmpList.Add(dictItem["ExapID"], dictItem);
                        ExmpList[dictItem["ExapID"]] = dictItem;

                        #region Excel报告相关
                        dictTestList[dictItem["ReflectionID"]] = dictItem["ExapName"];
                        #endregion

                        var listItem = Json.DerJsonToLDict(_dictExap[charpter][exmp][1].ToString());
                        _dictValue[exmp] = listItem;
                        var cache = Json.DerJsonToDict(_dictExap[charpter][exmp][0].ToString());
                        _dictReportValue.Add(cache["ReflectionID"], listItem);
                        var dr = dt.NewRow();
                        dr["Charpter"] = charpter;
                        dr["ExapID"] = dictItem["ExapID"];
                        dr["ReflectionID"] = dictItem["ReflectionID"];
                        dr["ExapName"] = dictItem["ExapName"];
                        dr["TestType"] = dictItem["TestType"];
                        dr["TestCount"] = dictItem["TestCount"];
                        dr["TestUpload"] = "--";
                        dr["TestTime"] = "--";
                        dr["TestResult"] = "--";
                        dr["ReadSubReport"] = "";
                        dt.Rows.Add(dr);
                    }
                    #region Excel报告相关
                    var listTemp = dictTestList.Keys.ToList();
                    listTemp.Sort(new NumericSortInString());
                    dictTestList = listTemp.ToDictionary(t => t, t => dictTestList[t]);
                    //dictTestList = dictTestList.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);//根据Key值对字典型排序
                    _dictdictTestList[charpter] = dictTestList;
                    #endregion
                }

                return dt;
            //}
            //catch (Exception e)
            //{

               // MessageBox.Show(e.ToString());
            //    return null;
            //}

        }

        private DataTable InitTestCol(DataTable dt)
        {
            dt.Columns.Clear();
            string[] colNames = new[] { "Charpter", "ExapID", "ReflectionID", "ExapName", "TestType", "TestCount", "TestTime", "TestUpload", "TestResult", "ReadSubReport" };
            foreach (var colName in colNames)
            {
                dt.Columns.Add(new DataColumn(colName, typeof(object)));
            }
            dt.Columns["TestCount"].ReadOnly = false;
            return dt;
        }
        private void PreDrawGird(bool isSucess)
        {
            var dt = new DataTable();
            dt = InitTestCol(dt);
            //var dt = _dt;
            dt.Rows.Clear();
            foreach (KeyValuePair<string, Dictionary<string, string>> ExmpStr in ExmpList)
            {
                if (isSucess)
                {
                    DataRow dr = dt.NewRow();
                    dr["Charpter"] = ExmpStr.Value["Charpter"];
                    dr["ExapID"] = ExmpStr.Value["ExapID"];
                    dr["ReflectionID"] = ExmpStr.Value["ReflectionID"];
                    dr["ExapName"] = ExmpStr.Value["ExapName"];
                    dr["TestType"] = ExmpStr.Value["TestType"];
                    dr["TestCount"] = ExmpStr.Value["TestCount"];
                    dr["TestUpload"] = "--";
                    dr["TestTime"] = "--";
                    dr["TestResult"] = "--";
                    dr["ReadSubReport"] = "";
                    dt.Rows.Add(dr);
                }
                else
                {
                    var dr = dt.NewRow();
                    dr["Charpter"] = ExmpStr.Value["Charpter"];
                    dr["ExapID"] = ExmpStr.Value["ExapID"];
                    dr["ReflectionID"] = ExmpStr.Value["ReflectionID"];
                    dr["ExapName"] = ExmpStr.Value["ExapName"];
                    dr["TestType"] = ExmpStr.Value["TestType"];
                    dr["TestCount"] = ExmpStr.Value["TestCount"];
                    dr["TestUpload"] = "--";
                    dr["TestTime"] = "--";
                    dr["TestResult"] = "--";
                    dr["ReadSubReport"] = "";
                    dt.Rows.Add(dr);
                }
            }
            this.Invoke(new MethodInvoker(() => { gcAuto.DataSource = dt; }));
            //gcAuto.DataSource = dt;
            SelectRows();
        }
        #endregion

        #region 其他

        private void FillXml()
        {
            string emlName = @"xml\Testplace.xml";
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + emlName))
            {
                string place = _file.ReadLocalXml(emlName, "Place");
                string version = _file.ReadLocalXml(emlName, "Ware");
                txtTestPosition.Text = place;
            }
        }
        #endregion

        public static DialogResult Show(UserLookAndFeel look, IWin32Window owner, string text, string caption, DialogResult[] buttons, Icon icon, int defaultButton, MessageBoxIcon messageBeepSound)
        {
            DevExpress.XtraEditors.XtraMessageBoxForm form = new XtraMessageBoxForm();
            Font defaultFont = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            form.Appearance.Font = defaultFont;
            return form.ShowMessageBoxDialog(new XtraMessageBoxArgs(look, owner, text, caption, buttons, icon, defaultButton));
        }

        /// <summary>
        /// 将CANoe生成的报告解析并存入数据库
        /// </summary>
        private void AddReport()
        {
            Dictionary<string,string> drReportRemark = new Dictionary<string, string>();
            string reportTime = "";
            string error = "";
            if (drContent.Count > 0)
                drContent.Clear();
            drReport.Add("TaskNo", GlobalVar.CurrentTsNode[0]);
            drReport.Add("TaskRound", GlobalVar.CurrentTsNode[1]);
            drReport.Add("TaskName", GlobalVar.CurrentTsNode[2]);
            drReport.Add("CANRoad", GlobalVar.CurrentTsNode[3]);
            drReport.Add("Module", GlobalVar.CurrentTsNode[4]);
            reportTime = DateTime.Now.ToString();
            drReport.Add("TestTime", reportTime);
            drReport.Add("TestUser", GlobalVar.UserName);

            if (!GlobalVar.ReportCopy.ContainsKey("TaskNo"))
            {
                GlobalVar.ReportCopy.Add("TaskNo", GlobalVar.CurrentTsNode[0]);
                GlobalVar.ReportCopy.Add("TaskRound", GlobalVar.CurrentTsNode[1]);
                GlobalVar.ReportCopy.Add("TaskName", GlobalVar.CurrentTsNode[2]);
                GlobalVar.ReportCopy.Add("CANRoad", GlobalVar.CurrentTsNode[3]);
                GlobalVar.ReportCopy.Add("Module", GlobalVar.CurrentTsNode[4]);
                GlobalVar.ReportCopy.Add("TestTime", reportTime);
            }
            else
            {
                GlobalVar.ReportCopy["TaskNo"] = GlobalVar.CurrentTsNode[0];
                GlobalVar.ReportCopy["TaskRound"] = GlobalVar.CurrentTsNode[1];
                GlobalVar.ReportCopy["TaskName"] = GlobalVar.CurrentTsNode[2];
                GlobalVar.ReportCopy["CANRoad"] = GlobalVar.CurrentTsNode[3];
                GlobalVar.ReportCopy["Module"] = GlobalVar.CurrentTsNode[4];
                GlobalVar.ReportCopy["TestTime"] = reportTime;
            }
            drReportRemark.Add("TestPosition", this.txtTestPosition.Text);
            drReportRemark.Add("TestElementID",txt0xF187.Text);
            int[] rows = gvAuto.GetSelectedRows();
            int skinCount = 0;
            int successCount = 0;
            int count = 0;
            foreach (var row in rows)
            {
                if (row >= 0)
                {
                    gvAuto.SelectRow(row);
                    _dr = this.gvAuto.GetDataRow(row);
                    if (_dr["TestResult"].ToString() == "成功")
                        successCount++;
                    if (_dr["TestResult"].ToString() == "未执行")
                        skinCount++;
                    count++;
                }        
            }
            drReportRemark.Add("TestItemCount", count.ToString());
            drReportRemark.Add("TestItemSuCount", successCount.ToString());
            drReportRemark.Add("TestItemSkipCount", skinCount.ToString());
            drReportRemark.Add("HardwareId", txt0xF192.Text);
            drReportRemark.Add("TestOrderId", txtF18C.Text);
            drReportRemark.Add("HardwareVersion", txt0xF193.Text);
            drReportRemark.Add("SoftWareVersion", txt0xF189.Text);
            //解析报告文件
            autoReport = _file.AnalysisXml(_file.IfFolderExistSiftExtension(report,"xml"));
            #region Excel报告相关
            excelReport = _file.AnalysisXmlReport(_file.IfFolderExistSiftExtension(report, "xml"));
            #endregion
            //如果出现本身就没有报告内容的，这部分需要重写
            while (true)
            {
                if (autoReport.Keys.Count == 0|| excelReport.Keys.Count==0)
                {
                    autoReport = _file.AnalysisXml(_file.IfFolderExistSiftExtension(report, "xml"));
                    #region Excel报告相关
                    excelReport = _file.AnalysisXmlReport(_file.IfFolderExistSiftExtension(report, "xml"));
                    #endregion
                } 
                else
                    break;
                Thread.Sleep(1000);
            }



            foreach (var key in GlobalVar.AutoReport.Keys)
            {
                autoReport.Add(key, GlobalVar.AutoReport[key]);
            }
            var listTemp = autoReport.Keys.ToList();
            listTemp.Sort(new NumericSortInString());
            autoReport = listTemp.ToDictionary(t => t, t => autoReport[t]);
            //autoReport = autoReport.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);//根据Key值对字典型排序
            string autoReportStr = Json.SerJson(autoReport);
            drReport.Add("AutoReport", autoReportStr);
            //string manualReport = Json.SerJson(GlobalVar.DictManualReport);
            drReport.Add("ManualReport", "");
            string remarkJson = Json.SerJson(drReportRemark);
            drReport.Add("Remark", remarkJson);
            drReport.Add("ErrorInfo", "");
            if (_store.AddReport(drReport, out error))
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    btnCheckRep.Enabled = true;
                }));
            }
            DerReportToQuestionNoteNew(autoReport, _dictExap);//将失败和成功的评价项分别存入数据库的QuestionNote表和PassReportNote表
            lblReport.Appearance.ForeColor = Color.Blue;
            lblReport.Enabled = true;
            
            foreach (var row in rows)
            {
                if (row >= 0)
                {
                    string state = gvAuto.GetRowCellValue(row, "TestResult").ToString();
                    if(state == "未执行")
                        SetCol(row, "ReadSubReport", "");
                    else
                        SetCol(row, "ReadSubReport", "查看数据");
                }
            }

            lblReport.Click += ReportLink_Click;
            WriteToLog("用例测试完成...");
            _isReport = true;
        }

        /// <summary>
        /// 将CANoe生成的报告解析并存入数据库
        /// </summary>
        private void AddExcelReport()
        {
            _isReportCreate = true;
            pictStart.Enabled = false;
            Dictionary<string, string> drReportRemark = new Dictionary<string, string>();
            string reportTime = "";
            string error = "";
            if (drContent.Count > 0)
                drContent.Clear();
            drReport.Add("TaskNo", GlobalVar.CurrentTsNode[0]);
            drReport.Add("TaskRound", GlobalVar.CurrentTsNode[1]);
            drReport.Add("TaskName", GlobalVar.CurrentTsNode[2]);
            drReport.Add("CANRoad", GlobalVar.CurrentTsNode[3]);
            drReport.Add("Module", GlobalVar.CurrentTsNode[4]);
            reportTime = DateTime.Now.ToString();
            drReport.Add("TestTime", reportTime);
            drReport.Add("TestUser", GlobalVar.UserName);

            if (!GlobalVar.ReportCopy.ContainsKey("TaskNo"))
            {
                GlobalVar.ReportCopy.Add("TaskNo", GlobalVar.CurrentTsNode[0]);
                GlobalVar.ReportCopy.Add("TaskRound", GlobalVar.CurrentTsNode[1]);
                GlobalVar.ReportCopy.Add("TaskName", GlobalVar.CurrentTsNode[2]);
                GlobalVar.ReportCopy.Add("CANRoad", GlobalVar.CurrentTsNode[3]);
                GlobalVar.ReportCopy.Add("Module", GlobalVar.CurrentTsNode[4]);
                GlobalVar.ReportCopy.Add("TestTime", reportTime);
            }
            else
            {
                GlobalVar.ReportCopy["TaskNo"] = GlobalVar.CurrentTsNode[0];
                GlobalVar.ReportCopy["TaskRound"] = GlobalVar.CurrentTsNode[1];
                GlobalVar.ReportCopy["TaskName"] = GlobalVar.CurrentTsNode[2];
                GlobalVar.ReportCopy["CANRoad"] = GlobalVar.CurrentTsNode[3];
                GlobalVar.ReportCopy["Module"] = GlobalVar.CurrentTsNode[4];
                GlobalVar.ReportCopy["TestTime"] = reportTime;

            }


            drReportRemark.Add("TestPosition", this.txtTestPosition.Text);
            drReportRemark.Add("TestElementID", txt0xF187.Text);
            int[] rows = gvAuto.GetSelectedRows();
            int skinCount = 0;
            int successCount = 0;
            int count = 0;
            foreach (var row in rows)
            {
                if (row >= 0)
                {
                    gvAuto.SelectRow(row);
                    _dr = this.gvAuto.GetDataRow(row);
                    if (_dr["TestResult"].ToString() == "成功")
                        successCount++;
                    if (_dr["TestResult"].ToString() == "未执行")
                        skinCount++;
                    count++;
                }
            }

            drReportRemark.Add("TestItemCount", count.ToString());
            drReportRemark.Add("TestItemSuCount", successCount.ToString());
            drReportRemark.Add("TestItemSkipCount", skinCount.ToString());
            drReportRemark.Add("HardwareId", txt0xF192.Text);
            drReportRemark.Add("TestOrderId", txtF18C.Text);
            drReportRemark.Add("HardwareVersion", txt0xF193.Text);
            drReportRemark.Add("SoftWareVersion", txt0xF189.Text);
            //解析报告文件

            #region Excel报告相关

            string strReportPath = _file.IfFolderExistSiftExtension(report, "xml");
            excelCoverReport = _file.AnalysisXmlReportCover(strReportPath);
            excelReport = _file.AnalysisXmlReport(strReportPath);
            excelPathReport = _file.AnalysisXmlReportPath(strReportPath);
            #endregion

            //如果出现本身就没有报告内容的，这部分需要重写
            for (int i = 0; i < 10; i++)
            {
                if (excelReport.Keys.Count == 0)
                {
                    WriteToLog("第" + (i + 1) + "次尝试获取CANoe生成的xml报告中...");
                    #region Excel报告相关
                    strReportPath = _file.IfFolderExistSiftExtension(report, "xml");
                    //WriteToLog("第" + (i + 1) + "次路径为：" + strReportPath + "，传入路径为：" + report);
                    excelCoverReport = _file.AnalysisXmlReportCover(strReportPath);
                    excelReport = _file.AnalysisXmlReport(strReportPath);
                    excelPathReport = _file.AnalysisXmlReportPath(strReportPath);
                    if (i == 9 && excelReport.Keys.Count == 0)
                    {
                        WriteToLog("获取失败，请检查CANoe生成的xml报告内是否无测试数据。");
                        WriteToLog("路径为：" + strReportPath + "，传入路径为：" + report);
                        _isReport = true;
                        return;
                    }
                    #endregion
                }
                else
                {
                    WriteToLog("获取CANoe生成的xml报告成功！");
                    break;
                }
                Thread.Sleep(1000);
            }

            foreach (var key in GlobalVar.ExcelReport.Keys)
            {
                excelReport.Add(key, GlobalVar.ExcelReport[key]);
            }

            foreach (var key in GlobalVar.ExcelReportPath.Keys)
            {
                excelPathReport.Add(key, GlobalVar.ExcelReportPath[key]);
            }

            GlobalVar.ExcelReport.Clear();
            GlobalVar.ExcelReportPath.Clear();
            var listReportTemp = excelReport.Keys.ToList();
            listReportTemp.Sort(new NumericSortInString());
            excelReport = listReportTemp.ToDictionary(t => t, t => excelReport[t]);
            var listPathReportTemp = excelPathReport.Keys.ToList();
            listPathReportTemp.Sort(new NumericSortInString());
            excelPathReport = listPathReportTemp.ToDictionary(t => t, t => excelPathReport[t]);
            //excelReport = excelReport.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);//根据Key值对字典型排序
            //excelPathReport = excelPathReport.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);
            string excelReportStr = Json.SerJson(excelReport);
            string excelReportPathStr = Json.SerJson(excelPathReport);
            string excelReportTestListStr = Json.SerJson(_dictdictTestList);
            string excelCoverReportStr = Json.SerJson(excelCoverReport);
            drReport.Add("ReportCoverInfo", excelCoverReportStr);
            drReport.Add("ReportMainInfo", excelReportStr);
            drReport.Add("ReportPathInfo", excelReportPathStr);
            drReport.Add("ReportTestList", excelReportTestListStr);
            string remarkJson = Json.SerJson(drReportRemark);
            drReport.Add("Remark", remarkJson);
            drReport.Add("ErrorInfo", "");
            _store.AddExcelReport(drReport, out error);
            //DerReportToQuestionNoteNew(autoReport, _dictExap);//将失败和成功的评价项分别存入数据库的QuestionNote表和PassReportNote表
            lblReport.Appearance.ForeColor = Color.Blue;
            lblReport.Enabled = true;

            foreach (var row in rows)
            {
                if (row >= 0)
                {
                    string state = gvAuto.GetRowCellValue(row, "TestResult").ToString();
                    if (state == "未执行")
                        SetCol(row, "ReadSubReport", "");
                    else
                        SetCol(row, "ReadSubReport", "查看数据");
                }
            }

            lblReport.Click += ReportLink_Click;
            WriteToLog("用例测试完成...");
            WriteToLog("报告生成中...");

            #region 临时方法 获取生成的Excel报告测试文件的路径

            string ReportDirpath = string.Empty;
            if (!Directory.Exists(report))
            {
                ReportDirpath = Path.GetDirectoryName(report) + @"\测试报告";
            }
            else
            {
                ReportDirpath = report + @"\测试报告";
            }

            #endregion

            #region 生成Excel报告

            try
            {
                CreateReport creport = new CreateReport();
                creport.ReportXmlPath = strReportPath;
                creport.dictReport = drReport;
                if (creport.CreateExcelReport(excelCoverReport, excelReport, excelPathReport,
                    _dictdictTestList, ReportDirpath))
                {
                    WriteToLog("报告生成完毕");
                    WriteToLog("报告保存路径：" + creport.ReportDirPath);
                    this.Invoke(new MethodInvoker(() => { btnCheckRep.Enabled = true; }));
                    _reportDirPath = creport.ReportDirPath;
                    _reportTime = creport.ReportTime.ToString("yyyyMMddHHmmss");
                    File.Delete(strReportPath); //删除报告xml
                    if (Directory.Exists(ReportDirpath))
                    {
                        Directory.Delete(ReportDirpath, true);
                        Directory.CreateDirectory(ReportDirpath);
                    }
                }
                else
                {
                    WriteToLog("报告生成失败");
                }

                #endregion
            }
            catch (Exception e)
            {
                XtraMessageBox.Show(e.ToString());
            }

            _isReport = true;
        }



        /// <summary>
        /// 将报告中的一部分数据存入问题记录表
        /// </summary>
        /// <param name="dictReport">报告内容</param>
        /// <param name="dictExap">用例表</param>
        private void DerReportToQuestionNoteNew(Dictionary<string, List<List<string>>> dictReport, Dictionary<string, Dictionary<string, List<object>>> dictExap)
        {
            List<string> queListf = new List<string>();
            List<string> queListp = new List<string>();
            Dictionary<string, List<Dictionary<string, string>>> assItemdict = new Dictionary<string, List<Dictionary<string, string>>>();
            Dictionary<string, List<Dictionary<string, string>>> assItemdictsuc = new Dictionary<string, List<Dictionary<string, string>>>();
            bool ifhaveFail = false;
            foreach (var report in dictReport)
            {
                foreach (var itemlist in report.Value)
                {
                    //因Excel报告修改 修改之前是 if (itemlist[5].ToLower() == "fail")
                    if (itemlist[4].ToLower() == "fail")
                    {
                        ifhaveFail = true;
                        break;
                    }
                }
                if (ifhaveFail)
                    break;
            }
            if (ifhaveFail)
            {

                foreach (var report in dictReport)
                {
                    string assess = report.Key;
                    List<Dictionary<string, string>> expList = new List<Dictionary<string, string>>();
                    List<Dictionary<string, string>> expsucList = new List<Dictionary<string, string>>();
                    string exapId = "";
                    string exapIdsuc = "";
                    foreach (var chapter in dictExap)
                    {
                        foreach (var exam in chapter.Value)
                        {
                            Dictionary<string, string> listItem = Json.DerJsonToDict(exam.Value[0].ToString());
                            if (assess == listItem["ExapID"])
                            {
                                foreach (var list in report.Value)
                                {
                                    if (queListf.Count == 0)
                                    {
                                        queListf.Add(GlobalVar.CurrentTsNode[0].Split('-')[0]);
                                        queListf.Add(GlobalVar.CurrentTsNode[0].Split('-')[1]);
                                        queListf.Add(GlobalVar.CurrentTsNode[0].Split('-')[2]);
                                        queListf.Add(GlobalVar.CurrentTsNode[1]);
                                        queListf.Add(GlobalVar.CurrentTsNode[2]);
                                        queListf.Add(GlobalVar.CurrentTsNode[4]);

                                        queListp.Add(GlobalVar.CurrentTsNode[0].Split('-')[0]);
                                        queListp.Add(GlobalVar.CurrentTsNode[0].Split('-')[1]);
                                        queListp.Add(GlobalVar.CurrentTsNode[0].Split('-')[2]);
                                        queListp.Add(GlobalVar.CurrentTsNode[1]);
                                        queListp.Add(GlobalVar.CurrentTsNode[2]);
                                        queListp.Add(GlobalVar.CurrentTsNode[4]);
                                    }
                                    List<string> listobj = new List<string>();
                                    bool same = false;
                                    foreach (var dictite in expList)
                                    {
                                        if (dictite.Values.Contains(list[0]))
                                        {
                                            same = true;
                                            break;

                                        }
                                    }
                                    if (same)
                                        continue;
                                    foreach (var dictite in expsucList)
                                    {
                                        if (dictite.Values.Contains(list[0]))
                                        {
                                            same = true;
                                            break;
                                        }
                                    }
                                    if (same)
                                        continue;
                                    if (list[5].ToLower() == "fail")
                                    {
                                        Dictionary<string, string> dictf = new Dictionary<string, string>();
                                        listobj.Add(listItem["ExapName"]);
                                        exapId = listItem["ExapID"];
                                        listobj.Add(assess);
                                        listobj.Add(list[1]);

                                        if (list[2].Split('-').Length > 1)
                                        {
                                            listobj.Add("");
                                            listobj.Add(list[2].Split('-')[0]);
                                            listobj.Add(list[2].Split('-')[1]);
                                        }
                                        else
                                        {
                                            listobj.Add(list[2]);
                                            listobj.Add("");
                                            listobj.Add("");
                                        }
                                        listobj.Add(list[3]);
                                        listobj.Add(list[4]);
                                        listobj.Add(list[5]);
                                        dictf = AssignItemDict(listobj);
                                        if (dictf.Count != 0)
                                            expList.Add(dictf);
                                    }
                                    else //成功的评价项目
                                    {
                                        Dictionary<string, string> dictp = new Dictionary<string, string>();
                                        //List<string> queList = new List<string>();
                                        listobj.Add(listItem["ExapID"]);
                                        exapIdsuc = listItem["ExapID"];
                                        listobj.Add(assess);
                                        listobj.Add(list[1]);

                                        if (list[2].Split('-').Length > 1)
                                        {
                                            //queList.Add("");
                                            //queList.Add(list[2].Split(',')[0].Split('[')[1]);
                                            //queList.Add(list[2].Split(',')[1].Split(']')[0]);
                                            listobj.Add("");
                                            listobj.Add(list[2].Split('-')[0]);
                                            listobj.Add(list[2].Split('-')[1]);
                                        }
                                        else
                                        {
                                            listobj.Add(list[2]);
                                            listobj.Add("");
                                            listobj.Add("");
                                        }
                                        listobj.Add(list[3]);
                                        listobj.Add(list[4]);
                                        listobj.Add(list[5]);
                                        dictp = AssignItemDict(listobj);
                                        if (dictp.Count != 0)
                                            expsucList.Add(dictp);
                                    }
                                }
                                break;
                            }
                        }
                    }
                    if (expList.Count != 0)
                        assItemdict.Add(exapId, expList);
                    if (expsucList.Count != 0)
                        assItemdictsuc.Add(exapIdsuc, expsucList);
                }
                if (assItemdict.Count != 0)
                {
                    string itemInfo = Json.SerJson(assItemdict);
                    queListf.Add(itemInfo);
                    queListf.Add(DateTime.Now.ToString());
                    AssignNDict(queListf);
                    string errorQ = "";
                    _store.AddQuestionNote(_dictNote, out errorQ);
                    _dictNote = new Dictionary<string, object>();
                }
                if (assItemdictsuc.Count != 0)
                {
                    string itemInfo = Json.SerJson(assItemdictsuc);
                    queListp.Add(itemInfo);
                    queListp.Add(DateTime.Now.ToString());
                    AssignNDict(queListp);
                    string errorQ = "";
                    _store.AddPassReportNote(_dictNote, out errorQ);
                    _dictNote = new Dictionary<string, object>();
                }
                //_draw.InitGrid();//刷新表格
            }
        }
        private void AssignNDict(List<string> dictList)
        {
            _dictNote.Add("VehicelType", dictList[0]);
            _dictNote.Add("VehicelConfig", dictList[1]);
            _dictNote.Add("VehicelStage", dictList[2]);
            _dictNote.Add("TaskRound", dictList[3]);
            _dictNote.Add("TestType", dictList[4]);
            _dictNote.Add("Module", dictList[5]);
            _dictNote.Add("FailItemInfo", dictList[6]);
            //_dictNote.Add("ExapName", dictList[7]);
            //_dictNote.Add("AssessItem", dictList[8]);
            //_dictNote.Add("DescriptionOfValue", dictList[9]);
            //_dictNote.Add("MinValue", dictList[10]);
            //_dictNote.Add("MaxValue", dictList[11]);
            //_dictNote.Add("NormalValue", dictList[12]);
            //_dictNote.Add("TestValue", dictList[13]);
            //_dictNote.Add("Result", dictList[14]);
            _dictNote.Add("TestTime", dictList[7]);
        }
        private Dictionary<string, string> AssignItemDict(List<string> dictList)
        {
            Dictionary<string, string> dictItem = new Dictionary<string, string>();
            dictItem.Add("ExapID", dictList[1]);
            dictItem.Add("ExapName", dictList[0]);
            dictItem.Add("AssessItem", dictList[2]);
            dictItem.Add("DescriptionOfValue", dictList[3]);
            dictItem.Add("MinValue", dictList[4]);
            dictItem.Add("MaxValue", dictList[5]);
            dictItem.Add("NormalValue", dictList[6]);
            dictItem.Add("TestValue", dictList[7]);
            dictItem.Add("Result", dictList[8]);

            return dictItem;
        }

        #region 旧的将报告数据分别导入问题记录表和测试成功表
        private void DerReportToQuestionNoteOld(Dictionary<string, List<List<string>>> dictReport, Dictionary<string, Dictionary<string, List<object>>> dictExap)
        {
            foreach (var report in dictReport)
            {
                string assess = report.Key;
                foreach (var chapter in dictExap)
                {
                    foreach (var exam in chapter.Value)
                    {
                        //if (exam.Key == assess)
                        {
                            Dictionary<string, string> listItem = Json.DerJsonToDict(exam.Value[0].ToString());
                            //foreach (var item in listItem)
                            {
                                if (assess == listItem["ExapName"])
                                {
                                    //List<Dictionary<string, string>> assItem = Json.DerJsonToLDict(exam.Value[1].ToString());
                                    foreach (var list in report.Value)
                                    {
                                        //Dictionary<string,object> dict = new Dictionary<string, object>();
                                        //dict.Add("",list[]);
                                        if (list[5].ToLower() == "fail")
                                        {
                                            List<string> queList = new List<string>();
                                            queList.Add(GlobalVar.CurrentTsNode[0].Split('-')[0]);
                                            queList.Add(GlobalVar.CurrentTsNode[0].Split('-')[1]);
                                            queList.Add(GlobalVar.CurrentTsNode[0].Split('-')[2]);
                                            queList.Add(GlobalVar.CurrentTsNode[1]);
                                            queList.Add(GlobalVar.CurrentTsNode[2]);
                                            queList.Add(GlobalVar.CurrentTsNode[4]);
                                            queList.Add(listItem["ExapID"]);
                                            queList.Add(assess);
                                            queList.Add(list[1]);
                                            if (list[2].Split('-').Length > 1)
                                            {
                                                //queList.Add("");
                                                //queList.Add(list[2].Split(',')[0].Split('[')[1]);
                                                //queList.Add(list[2].Split(',')[1].Split(']')[0]);
                                                queList.Add("");
                                                queList.Add(list[2].Split('-')[0]);
                                                queList.Add(list[2].Split('-')[1]);
                                            }
                                            else
                                            {
                                                queList.Add(list[2]);
                                                queList.Add("");
                                                queList.Add("");

                                            }
                                            queList.Add(list[3]);
                                            queList.Add(list[4]);
                                            queList.Add(list[5]);
                                            queList.Add(DateTime.Now.ToString());
                                            AssignDict(queList);
                                            string errorQ = "";
                                            //IList<object[]> ques = _store.GetSpecialByEnum(EnumLibrary.EnumTable.QuestionNote, _dictNote);
                                            //if(ques.Count != 0)
                                            //    continue;
                                            _store.AddQuestionNote(_dictNote, out errorQ); //添加到employee表中
                                            //_draw.InitGrid();//刷新表格
                                            _dictNote = new Dictionary<string, object>();
                                        }
                                        else
                                        {
                                            List<string> queList = new List<string>();
                                            queList.Add(GlobalVar.CurrentTsNode[0].Split('-')[0]);
                                            queList.Add(GlobalVar.CurrentTsNode[0].Split('-')[1]);
                                            queList.Add(GlobalVar.CurrentTsNode[0].Split('-')[2]);
                                            queList.Add(GlobalVar.CurrentTsNode[1]);
                                            queList.Add(GlobalVar.CurrentTsNode[2]);
                                            queList.Add(GlobalVar.CurrentTsNode[4]);
                                            queList.Add(listItem["ExapID"]);
                                            queList.Add(assess);
                                            queList.Add(list[1]);
                                            if (list[2].Split('-').Length > 1)
                                            {
                                                //queList.Add("");
                                                //queList.Add(list[2].Split(',')[0].Split('[')[1]);
                                                //queList.Add(list[2].Split(',')[1].Split(']')[0]);
                                                queList.Add("");
                                                queList.Add(list[2].Split('-')[0]);
                                                queList.Add(list[2].Split('-')[1]);
                                            }
                                            else
                                            {
                                                queList.Add(list[2]);
                                                queList.Add("");
                                                queList.Add("");
                                            }
                                            queList.Add(list[3]);
                                            queList.Add(list[4]);
                                            queList.Add(list[5]);
                                            queList.Add(DateTime.Now.ToString());
                                            AssignDict(queList);
                                            string errorQ = "";
                                            //IList<object[]> ques = _store.GetSpecialByEnum(EnumLibrary.EnumTable.QuestionNote, _dictNote);
                                            //if (ques.Count != 0)
                                            //    continue;
                                            _store.AddPassReportNote(_dictNote, out errorQ); //添加到employee表中
                                            //_draw.InitGrid();//刷新表格
                                            _dictNote = new Dictionary<string, object>();
                                        }

                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion
        private void AssignDict(List<string> dictList)
        {
            _dictNote["VehicelType"] = dictList[0];
            _dictNote["VehicelConfig"] = dictList[1];
            _dictNote["VehicelStage"] = dictList[2];
            _dictNote["TaskRound"] = dictList[3];
            _dictNote["TestType"] = dictList[4];
            _dictNote["Module"] = dictList[5];
            _dictNote["ExapID"] = dictList[6];

            _dictNote["ExapName"] = dictList[7];
            _dictNote["AssessItem"] = dictList[8];
            _dictNote["DescriptionOfValue"] = dictList[9];
            _dictNote["MinValue"] = dictList[10];
            _dictNote["MaxValue"] = dictList[11];
            _dictNote["NormalValue"] = dictList[12];
            _dictNote["TestValue"] = dictList[13];
            _dictNote["Result"] = dictList[14];
            _dictNote["TestTime"] = dictList[15];
        }







        private void gvAuto_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            object cellValue = gvAuto.GetRowCellValue(e.RowHandle, "TestResult");
            string state = "";
            if (cellValue != null)
                state = gvAuto.GetRowCellValue(e.RowHandle, "TestResult").ToString();

            //比较指定列的状态
            if (state == "成功")
            {
                e.Appearance.BackColor = Color.Green;//设置此行的背景颜色
            }
            else if (state == "失败")
            {
                e.Appearance.BackColor = Color.Red;//设置此行的背景颜色
            }
            else if (state == "未执行")
            {
                e.Appearance.BackColor = Color.Yellow;//设置此行的背景颜色
            }
        }

        private void gcAuto_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //获得光标位置
                var hi = gvAuto.CalcHitInfo(e.Location);
                //判断位置是否在行位置上
                if (!hi.InRow && !hi.InRowCell) return;
                if (hi.RowHandle < 0) return;
                rowUpload = hi.RowHandle;
                //取一行值
                gvAuto.SelectRow(hi.RowHandle);
                _dr = this.gvAuto.GetDataRow(hi.RowHandle);
                if (_dr["TestResult"].ToString() == "失败"&& _dr["TestUpload"].ToString() == "未上传")
                {
                    tsmiUpload.Enabled = true;
                }
                else
                    tsmiUpload.Enabled = false;
            }
        }

        private void tsmiUpload_Click(object sender, EventArgs e)
        {
            string rID = _dr["ReflectionID"].ToString();
            ErrorInfo error = new ErrorInfo(rID);
            if (error.ShowDialog() == DialogResult.OK)
            {
                SetCol(rowUpload, "TestUpload", "已上传");
            }
        }

        private void btnImportTxtlog_Click(object sender, EventArgs e)
        {
            try
            {
                string txtName = GlobalVar.CurrentTsNode[0] + "-" + GlobalVar.CurrentTsNode[1] + "-" + GlobalVar.CurrentTsNode[2] + "-" + GlobalVar.CurrentTsNode[3] + "-" + txtModule.Text + "节点";
                string path = AppDomain.CurrentDomain.BaseDirectory + "log\\" + txtName + ".txt";
                FileStream fs = new FileStream(path, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                //写入
                sw.Write(txtLog.Text);
                //清空
                sw.Flush();
                //关闭
                sw.Close();
                fs.Close();
                XtraMessageBox.Show("运行提示器文本导出成功...");
            }
            catch (Exception)
            {
                XtraMessageBox.Show("运行提示器文本导出失败，请联系工程师查看...");
            }
            
        }

        private void btnCheckReportDir(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_reportDirPath))
            {
                XtraMessageBox.Show("报告路径不存在，请检查是否被人为删除或移动了位置");
                return;
            }
            if (Directory.Exists(_reportDirPath))
            {
                System.Diagnostics.Process.Start(_reportDirPath + "TestReport" + _reportTime + ".xlsx");
            }
        }

        private void ShieldRight()
        {
            ContextMenu emptyMenu = new ContextMenu();
            txt0xF187.Properties.ContextMenu = emptyMenu;
            txt0xF188.Properties.ContextMenu = emptyMenu;
            txt0xF189.Properties.ContextMenu = emptyMenu;
            txt0xF18A.Properties.ContextMenu = emptyMenu;
            txt0xF18B.Properties.ContextMenu = emptyMenu;
            txt0xF190.Properties.ContextMenu = emptyMenu;
            txt0xF191.Properties.ContextMenu = emptyMenu;
            txt0xF192.Properties.ContextMenu = emptyMenu;
            txt0xF193.Properties.ContextMenu = emptyMenu;
            txt0xF194.Properties.ContextMenu = emptyMenu;
            txt0xF195.Properties.ContextMenu = emptyMenu;
            txtF18C.Properties.ContextMenu = emptyMenu;
            txtDate.Properties.ContextMenu = emptyMenu;
            txtLog.Properties.ContextMenu = emptyMenu;
            txtModule.Properties.ContextMenu = emptyMenu;
            txtTestPosition.Properties.ContextMenu = emptyMenu;
        }

        private void pictStop_Click(object sender, EventArgs e)
        {
            if (
                XtraMessageBox.Show("终止测试后无法恢复，是否要终止测试？？", "提示", MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning) ==
                DialogResult.OK)
            {
                if (CANoeTest.IsEndDevice && CANoeTest.IsEndPro && !_isReportCreate)
                {
                    if (XtraMessageBox.Show("是否要生成报告？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                    {
                        try
                        {
                            Thread cexcelThread = new Thread(AddExcelReport);
                            cexcelThread.Start();
                            //AddExcelReport();
                        }
                        catch (Exception ex)
                        {
                            XtraMessageBox.Show("报告生成错误，请联系维护人员。\r\n错误信息：" + ex.Message);
                            _isReport = true;
                        }
                    }
                    else
                    {
                        _isReport = true;
                    }
                }
                WriteToLog("测试停止");
                CANoeTest.PauseCANoe();
                Thread thread1 = new Thread(StopAfterReportCreateEnd);
                thread1.Start();
            }
        }

        private void StopAfterReportCreateEnd()
        {
            while (true)
            {
                if (_isReport)
                {
                    if (thread != null && thread.IsAlive)
                        thread.Abort();
                    if (threadH != null && threadH.IsAlive)
                        threadH.Abort();
                    if (threadSet != null && threadSet.IsAlive)
                        threadSet.Abort();
                    if (threadHSet != null && threadHSet.IsAlive)
                        threadHSet.Abort();
                    if (threadexample != null && threadexample.IsAlive)
                        threadexample.Abort();
                    GloalVar.IsDPause = false;
                    GloalVar.IsHPause = false;
                    _isReport = false;
                    isClose = true;
                    _isTestStart = false;
                    _isRun = false;
                    _isGet = false;
                    IsFisrt = true;
                    CANoeTest.endHFlag = true;
                    CANoeTest.endDFlag = true;
                    CANoeTest.IsEndDevice = false;
                    CANoeTest.IsEndPro = false;
                    pictStart.Enabled = false;
                    drReport.Clear();
                    break;
                }
                Thread.Sleep(500);
            }
        }


        private void StopClearUI()
        {
            this.Invoke(new MethodInvoker(() =>
            {
                pictStart.Enabled = false;
                pictStart.Image = Resources.play_unable;
                pictStop.Enabled = false;
                pictStop.Image = Resources.stop_unbale;
                pictPretreatment.Enabled = false;
                pictPretreatment.Image = Resources.Pretreatment_unable;
                btnCheckRep.Enabled = false;
                btnImportTxtlog.Enabled = false;
            }));
            ShieldRight();
            WriteToLog("------------------------------以上是测试信息---------------------------------------");
            WriteToLog("----------------------------以上是显性测试信息-------------------------------------");
            WriteToLog("测试停止");
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                Dictionary<string, string> dictUpload = new Dictionary<string, string>();
                IList<object[]> info = _store.GetRegularByEnum(EnumLibrary.EnumTable.UploadInfo);
                if (info.Count > 0)
                {
                    dictUpload["IP"] = info[0][0].ToString();
                    dictUpload["Port"] = info[0][1].ToString();
                    dictUpload["User"] = info[0][2].ToString();
                    dictUpload["Password"] = info[0][3].ToString();
                    dictUpload["UploadPath"] = info[0][4].ToString();
                }
                CollectDataToXml();
                CopyAndZip();
                string result = _file.UpLoadFile(dictUpload, AppDomain.CurrentDomain.BaseDirectory + "backup.zip");
                if (result != "8")
                    MessageBox.Show(@"上传失败...");
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "backup.zip");
                DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "backup");
                di.Delete(true);
                MessageBox.Show("上传成功...");
            }
            catch (Exception ex)
            {
                throw;

            }
            

        }

        private void txtModule_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtDate_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtTestPosition_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void textEdit3_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtLog_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }
    }
}