using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraNavBar;
using DevExpress.XtraReports.UI;
using ProcessEngine;

namespace ReportEditor
{
    public partial class ReportView : DevExpress.XtraBars.Ribbon.RibbonForm
    {

        ProcStore _store = new ProcStore();
        ProcFile _file = new ProcFile();
        Dictionary<string, object> dictnote = new Dictionary<string, object>();
        Dictionary<string,object>_dictNote = new Dictionary<string, object>();
        private static NavBarItem OldBarItem = null;
        public ReportView()
        {
            InitializeComponent();

            var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.Report, 0);
            //把集合赋值给控件
            string curruntVehicel = "";
            cbTaskNo.Properties.Items.Clear();
            List<string> taskNo = new List<string>();
           
            foreach (var item in dept)
            {
                bool same = false;
                foreach (string tasNo in taskNo)
                {
                    if (item == tasNo)
                    {
                        same = true;
                        break;
                    }
                    
                }
                if (!same)
                {                    
                    cbTaskNo.Properties.Items.Add(item);
                    taskNo.Add(item);
                    
                }
            }
            

        }
        //旧的测试入口处查看报告入口

        //public ReportView(Dictionary<string, List<Dictionary<string, Dictionary<string, List<string>>>>> dict)
        //{
        //    InitializeComponent();

        //    TestReport test = new TestReport(dict);
        //    docReport.DocumentSource = test;
            
        //}

        //TestStart页面处查看报告
        public ReportView(Dictionary<string, List<Dictionary<string, List<List<string>>>>> dict, Dictionary<string, List<string>> drVarList,Dictionary<string,List<string>> errorInfo)
        {
            InitializeComponent();
            List<object> dictList = new List<object>();
            if (GlobalVar.ReportTestOrder.Count != 11 )
            {
                MessageBox.Show(@"请测试完了再查看报告");
                return;
            }
            foreach (KeyValuePair<string, string> dictTask in GlobalVar.ReportTestOrder)
            {
                if (dictTask.Key.Trim() == "TaskNo")
                {
                    cbTaskNo.Text = dictTask.Value.Trim();
                    cbTaskNo.ReadOnly = true;
                }
                else if (dictTask.Key.Trim() == "TaskRound")
                {
                    cbRound.Text = dictTask.Value.Trim();
                    cbRound.ReadOnly = true;
                }
                else if (dictTask.Key.Trim() == "TestType")
                {
                    cbTaskName.Text = dictTask.Value.Trim();
                    cbTaskName.ReadOnly = true;
                }
                else if (dictTask.Key.Trim() == "CANRoad")
                {
                    cbCAN.Text = dictTask.Value.Trim();
                    cbCAN.ReadOnly = true;
                }
                else if (dictTask.Key.Trim() == "Module")
                {
                    cbNode.Text = dictTask.Value.Trim();
                    cbNode.ReadOnly = true;
                }
               
            }
            dictList.Add(GlobalVar.ReportTestOrder);
            dictList.Add(GlobalVar.Reference);
            dictList.Add(GlobalVar.imgTply);
            dictList.Add(GlobalVar.TestItem);
            dictList.Add(GlobalVar.ReportTitle);
                        
            TestReport test = new TestReport(dict,dictList, drVarList, errorInfo);
            docReport.DocumentSource = test;
            test.CreateDocument();
        }
        void DrawNav(IList<object[]> reportList )
        {

            //得到所有模板表中的模板信息
            //IList<object[]> report = _store.GetRegularByEnum(EnumLibrary.EnumTable.Report);
            //var row = 0;
            //foreach (var name in report)
            //{
            //    reportGroup.AddItem();
            //    navReport.Items[row].Caption = name[0].ToString() + "&" + name[2].ToString() + "&" + name[3].ToString() + "&" + name[4].ToString() + "&" + name[1].ToString() + "&" + name[6].ToString();
            //    navReport.Items[row].Appearance.Font = new Font("微软雅黑", 9, FontStyle.Regular);
            //    navReport.Items[row].AppearancePressed.Font = new Font("微软雅黑", 9, FontStyle.Regular);
            //    navReport.Items[row].LinkClicked += NameItem_Click;
            //    row++;
            //}

            ////得到所有模板表中的模板信息
            //IList<object[]> report = _store.GetRegularByEnum(EnumLibrary.EnumTable.Report);
            //var row = 0;
            //foreach (var name in report)
            //{
            //    reportGroup.AddItem();
            //    navReport.Items[row].Caption = name[0].ToString()+"&"+ name[2].ToString()+"&"+ name[3].ToString() + "&" + name[4].ToString() + "&" + name[1].ToString() + "&" + name[6].ToString();
            //    navReport.Items[row].Appearance.Font = new Font("微软雅黑", 9, FontStyle.Regular);
            //    navReport.Items[row].AppearancePressed.Font = new Font("微软雅黑", 9, FontStyle.Regular);
            //    navReport.Items[row].LinkClicked += NameItem_Click;
            //    row++;
            //}

            var row = 0;
            navReport.Items.Clear();
            foreach (var name in reportList)
            {
                reportGroup.AddItem();
                navReport.Items[row].Caption = name[5].ToString();
                navReport.Items[row].Appearance.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                navReport.Items[row].AppearancePressed.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                navReport.Items[row].LinkClicked += NameItem_Click;
                row++;
            }

        }

        private void NameItem_Click(object sender, EventArgs e)
        {
            //判断是否依赖节点
            var item = sender as NavBarItem;
            if (OldBarItem != null && OldBarItem != item)
            {
                OldBarItem.Appearance.ForeColor = Color.Black;

            }
            if (item != null)
            {
                item.Appearance.ForeColor = Color.Red;
                OldBarItem = item;
            }
            string[] selectedName = new string[6];
            //var selectedName = item.Caption.Split('&');
            selectedName[0] = cbTaskNo.Text;
            selectedName[1] = cbRound.Text;
            selectedName[2] = cbTaskName.Text;
            selectedName[3] = cbCAN.Text;
            selectedName[4] = cbNode.Text;
            selectedName[5] = item.Caption;
            IList<object> report = _store.GetReportById(selectedName);


            //string path = _file.ReportXmlSave(_dictreport);
            //var data = _file.ImportReportData(pathR, _dictreport);
            Dictionary<string, object> dictNote = GetDataFromUI();
            Dictionary<string, List<List<string>>> dictReport = new Dictionary<string, List<List<string>>>();
            //dictReport = _file.AnalysisXml(EnumLibrary.ReportPath);
            //if (report[7].ToString() == "" || report[7].ToString() == "{}")
            //{
                
            //    return;
            //}
            dictReport = Json.DeserJsonDStrLList(report[7].ToString());
            
            //dictReport = _file.AnalysisXml(EnumLibrary.ReportPath);
            dictNote["Module"] = GetModuleToJson(dictNote["TaskName"].ToString(), dictNote["Module"].ToString());
            IList<object[]> listTask = _store.GetSpecialByEnum(EnumLibrary.EnumTable.TaskTest, dictNote);
            if (listTask.Count == 0)
                return;
            Dictionary<string, Dictionary<string, List<object>>> dictExap =
                Json.DeserJsonToDDict(listTask[0][11].ToString());
            var data = _file.DerReportToNewDict(dictReport, dictExap);

            //从数据库中查询指定表的数据信息
            dictNote["Module"] = cbNode.Text.Trim();
            //var reportList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ReportSole, dictNote);

            Dictionary<string, string> remark = Json.DerJsonToDict(report[9].ToString());
            remark.Add("TestUser", report[8].ToString());
            remark.Add("TestTime", report[5].ToString());
            remark.Add("TaskRound", report[1].ToString());
            remark.Add("Module", report[4].ToString());
            remark.Add("TestType", report[2].ToString() + "测试报告");
            remark.Add("TaskNo", report[0].ToString());
            remark.Add("TaskName", report[2].ToString());
            remark.Add("CANRoad", report[3].ToString());
            string[] nodes = report[4].ToString().Split('/');
            Dictionary<string,List<string>> dListNode = new Dictionary<string, List<string>>();
            foreach (var node in nodes)
            {
                List<string> nodeList = new List<string>();
                nodeList.Add(remark["HardwareId"]);
                nodeList.Add(remark["TestOrderId"]);
                nodeList.Add(remark["HardwareVersion"]);
                nodeList.Add(remark["SoftWareVersion"]);
                dListNode.Add(node,nodeList);
            }
            
            //Dictionary<string, List<string>> moduleInfor = new Dictionary<string, List<string>>();//模块信息
            Dictionary<string, List<string>> dictErrorInfo = new Dictionary<string, List<string>>();//错误信息和截图
            dictErrorInfo = Json.DeserJsonDListStr(report[10].ToString());
            
            TestReport test = new TestReport(data, remark, dListNode, dictErrorInfo);
            docReport.DocumentSource = test;
            test.CreateDocument();
            //IsReport();

            //GlobalVar.ReportTestOrder["TestUser"] = report[9].ToString();
            //GlobalVar.ReportTestOrder["TestTime"] = report[6].ToString();
            //GlobalVar.ReportTestOrder["TestType"] = report[2].ToString();
            //GlobalVar.ReportTestOrder["TestRound"] = report[1].ToString();
            //GlobalVar.ReportTestOrder["Module"] = report[4].ToString();
            //GlobalVar.ReportTestOrder["TestPosition"] = report[10].ToString();
            //GlobalVar.ReportTestOrder["HardwareVersionM"] = report[15].ToString();
            //GlobalVar.ReportTestOrder["TestElementID"] = report[11].ToString();
            //GlobalVar.ReportTestOrder["TestOrderId"] = report[16].ToString();

            //int all = int.Parse(report[12].ToString());//测试总数
            //int pass = int.Parse(report[13].ToString());//通过数
            //int skip = int.Parse(report[14].ToString());//跳过数

            ////int all = 10;//测试总数
            ////int pass = 10;//通过数
            ////int skip = 0;//跳过数

            //List<int> TestItem = new List<int>
            //{
            //    all,
            //    all,
            //    skip,
            //    pass,
            //    all-pass
            //};
            //GlobalVar.TestItem = TestItem;

            //_dictreport["TaskNo"] = report[0].ToString();
            //_dictreport["TaskRound"] = report[1].ToString();
            //_dictreport["TaskName"] = report[2].ToString();
            //_dictreport["CANRoad"] = report[3].ToString();
            //_dictreport["Module"] = report[4].ToString();
            //_dictreport["TestOrder"] = report[5].ToString();
            //_dictreport["TestTime"] = report[6].ToString();
            //_dictreport["AutoReport"] = report[8].ToString();

            //dictnote["TaskNo"] = report[0];
            //dictnote["TaskRound"] = report[1];
            //dictnote["TaskName"] = report[2];
            //dictnote["CANRoad"] = report[3];
            //dictnote["Module"] = report[4];
            //GlobalVar.ReportTitle = report[0] + "-" + report[1] + "-" + report[3] + "-" + report[2];
            //ReportFixed(_dictreport["TaskNo"].ToString());

            //string path = _file.ReportXmlSave(_dictreport);
            ////var data = _file.ReportData(path, _dictreport);
            //var data = _file.ImportReportData(path, _dictreport);
            ////TestRert test = new TestReport(data);
            ////TestReport test = new TestReport(data);
            ////docReport.DocumentSource = test;
            ////test.CreateDocument();

        }



     

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            Dictionary<string,object> drReport = new Dictionary<string, object>();
            drReport = GetDataFromUI();
            //drReport.Add("TaskNo",cbTaskNo.SelectedItem.ToString());
            //drReport.Add("TaskRound", cbRound.SelectedItem.ToString());
            //drReport.Add("TaskName", cbTaskName.SelectedItem.ToString());
            //drReport.Add("CANRoad", cbCAN.SelectedItem.ToString());
            //drReport.Add("Module", cbNode.SelectedItem.ToString());
            IList<object[]> reportList = _store.GetReportM(drReport);
            GlobalVar.ReportCache = reportList;
            DrawNav(GlobalVar.ReportCache);

        }


        private void cbTaskNo_SelectedValueChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cbTaskNo.Text.Trim()))
                return;
            Dictionary<string, object> dictReportCon = new Dictionary<string, object>();
            dictReportCon["condition"] = "TaskNo";
            dictReportCon["TaskNo"] = cbTaskNo.Text.Trim();
            IList<object[]> reportList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Report, dictReportCon);
            //var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.Report, 1);
            cbRound.Properties.Items.Clear();
            cbRound.Text = "";
            cbTaskName.Text = "";
            cbCAN.Text = "";
            cbNode.Text = "";
            List<string> taskNo = new List<string>();
            bool same = false;
            foreach (var item in reportList)
            {
                foreach (string tasNo in taskNo)
                {
                    if (item[1].ToString() == tasNo)
                    {
                        same = true;
                        break;
                    }

                }
                if (!same)
                {
                    cbRound.Properties.Items.Add(item[1]);
                    taskNo.Add(item[1].ToString());

                }
            }
                        
        }

        private void cbRound_SelectedValueChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cbRound.Text.Trim()))
                return;
            //var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.Report, 2);
            Dictionary<string, object> dictReportCon = new Dictionary<string, object>();
            dictReportCon["condition"] = "TaskRound";
            dictReportCon["TaskNo"] = cbTaskNo.Text.Trim();
            dictReportCon["TaskRound"] = cbRound.Text.Trim();
            IList<object[]> reportList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Report, dictReportCon);
            //把集合赋值给控件
            List<string> taskNo = new List<string>();
            cbTaskName.Properties.Items.Clear();
            cbTaskName.Text = "";
            cbCAN.Text = "";
            cbNode.Text = "";
            //bool same = false;
            foreach (var item in reportList)
            {
                bool same = false;
                foreach (string tasno in taskNo)
                {
                    if (item[2].ToString() == tasno)
                    {
                        same = true;
                        break;
                    }
                    
                }
                if (!same)
                {
                    cbTaskName.Properties.Items.Add(item[2]);
                    taskNo.Add(item[2].ToString());

                }
            }

        }

        private void cbTaskName_SelectedValueChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cbTaskName.Text.Trim()))
                return;
            //var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.Report, 3);
            //把集合赋值给控件
            Dictionary<string, object> dictReportCon = new Dictionary<string, object>();
            dictReportCon["condition"] = "TaskName";
            dictReportCon["TaskNo"] = cbTaskNo.Text.Trim();
            dictReportCon["TaskRound"] = cbRound.Text.Trim();
            dictReportCon["TaskName"] = cbTaskName.Text.Trim();
            IList<object[]> reportList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Report, dictReportCon);
            cbCAN.Properties.Items.Clear();
            cbCAN.Text = "";
            cbNode.Text = "";
            List<string> taskNo = new List<string>();
            
            foreach (var item in reportList)
            {
                bool same = false;
                foreach (string tasNo in taskNo)
                {
                    if (item[3].ToString() == tasNo)
                    {
                        same = true;
                        break;
                    }

                }
                if (!same)
                {
                    cbCAN.Properties.Items.Add(item[3]);
                    taskNo.Add(item[3].ToString());

                }
            }
            
        }

        private void cbCAN_SelectedValueChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cbCAN.Text.Trim()))
                return;
            //var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.Report, 4);
            //把集合赋值给控件
            Dictionary<string, object> dictReportCon = new Dictionary<string, object>();
            dictReportCon["condition"] = "CANRoad";
            dictReportCon["TaskNo"] = cbTaskNo.Text.Trim();
            dictReportCon["TaskRound"] = cbRound.Text.Trim();
            dictReportCon["TaskName"] = cbTaskName.Text.Trim();
            dictReportCon["CANRoad"] = cbCAN.Text.Trim();
            IList<object[]> reportList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Report, dictReportCon);
            cbNode.Properties.Items.Clear();
            cbNode.Text = "";
            List<string> taskNo = new List<string>();
            //bool same = false;
            foreach (var item in reportList)
            {
                //string strModule = ConvertModuleJsonToString(cbTaskName.Text, item[4].ToString());
                bool same = false;
                foreach (string tasNo in taskNo)
                {
                    if (item[4].ToString() == tasNo)
                    {
                        same = true;
                        break;
                    }

                }
                if (!same)
                {
                    cbNode.Properties.Items.Add(item[4].ToString());
                    taskNo.Add(item[4].ToString());

                }
            }

        }
        private string GetModuleToJson(string node, string listNode)
        {
            string module = "";
            if (node != "")
            {

                //cbName.Properties.Items.AddRange(type);
                switch (node)
                {
                    case "CAN单节点":
                        module = listNode.Split(' ')[0];
                        break;
                    case "CAN集成":
                        module = GetMultiModuleJson(listNode);
                        break;
                    case "J1939单节点":
                        module = listNode.Split(' ')[0];
                        break;
                    case "J1939集成":
                        module = GetMultiModuleJson(listNode);
                        break;
                    case "LIN单节点":
                        module = listNode.Split(' ')[0];
                        break;
                    case "LIN集成":
                        module = GetMultiModuleJson(listNode);
                        break;
                    case "OSEK单节点":
                        module = listNode.Split(' ')[0];
                        break;
                    case "OSEK集成":
                        module = GetMultiModuleJson(listNode);
                        break;
                    case "总线相关DTC":
                        module = listNode.Split(' ')[0];
                        break;
                    case "诊断协议":
                        module = listNode.Split(' ')[0];
                        break;
                    default:
                        break;
                }

            }
            return module;
        }
        private string GetMultiModuleJson(string nodeStr)
        {
            string name = "";
            Dictionary<string, string> module = new Dictionary<string, string>();
            string remModule = nodeStr.Split(' ')[0];
            string[] a = remModule.Split('/');
            if (a.Length > 1)
            {
                string norStr = "";
                string virStr = "";
                foreach (var b in a)
                {
                    string[] nor = b.Split('(');
                    if (nor.Length > 1)
                    {
                        virStr = virStr + b + "/";
                    }
                    else
                    {
                        norStr = norStr + b + "/";
                    }
                }
                if (norStr != "")
                    module.Add("Normal", norStr.Remove(norStr.Length - 1));
                //else
                //    module.Add("Normal", norStr);
                if (virStr != "")
                    module.Add("Virtual", virStr.Remove(virStr.Length - 1));
                //else
                //{
                //    module.Add("Virtual", virStr);
                //}
                name = Json.SerJson(module);
            }
            else
            {
                string[] b = a[0].Split('(');
                if (b.Length < 1)
                {
                    module.Add("Normal", a[0]);
                    name = Json.SerJson(module);
                }
                else
                {
                    module.Add("Normal", "");
                    module.Add("Virtual", a[0]);
                    name = Json.SerJson(module);
                }
            }
            return name;
        }





        private Dictionary<string, object> GetDataFromUI()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["TaskNo"] = cbTaskNo.Text.Trim();
            dict["TaskRound"] = cbRound.Text.Trim();
            dict["TaskName"] = cbTaskName.Text.Trim();
            dict["CANRoad"] = cbCAN.Text.Trim();
            dict["Module"] = cbNode.Text.Trim();
            return dict;
        }





        private void ReportView_FormClosing(object sender, FormClosingEventArgs e)
        {
            GlobalVar.isRun = false;
        }
    }
}