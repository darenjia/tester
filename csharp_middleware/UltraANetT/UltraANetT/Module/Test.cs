using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using ProcessEngine;
using UltraANetT.Module;
using UltraANetT.Form;
using System.Drawing;
using CANoeEngine;
using DevExpress.DataAccess.Native.Sql.QueryBuilder;
using DevExpress.XtraNavBar;
using DevExpress.LookAndFeel;
using NHibernate.Hql.Ast.ANTLR.Tree;
using NHibernate.Linq.Functions;
using UltraANetT.Interface;
using System.Text;

namespace UltraANetT.Module
{
    public partial class Test : XtraUserControl
    {
        private readonly ProcStore _store = new ProcStore();

        private Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>> leftNav
            = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>>();

        private LogicalControl _LogC = new LogicalControl();
        private string role;
        public INode NodeStr;
        private string Module;
        private int row;
        private string[] listTaskNo;
        private readonly ProcShow _show = new ProcShow();

        public Test()
        {
            InitializeComponent();
            InitControl();
            DrawNav();
            role = _LogC.RoleSelect(GlobalVar.UserName);
        }

        private void InitControl()
        {
            TestStart ts = new TestStart();
            pcTest.Controls.Clear();
            //TestStart ts = new TestStart() 
            ts.Dock = DockStyle.Fill;
            pcTest.Controls.Add(ts);
            //pcTest.Controls.Clear();
            //var ts = new TestWait(this) {Dock = DockStyle.Fill};
            //pcTest.Controls.Add(ts);
        }


        private void DrawNav()
        {
            IList<object[]> taskList = _store.GetRegularByEnum(EnumLibrary.EnumTable.Task);

            foreach (string taskNo in taskList.Select(task => task[0].ToString())
                .Where(taskNo => !leftNav.ContainsKey(taskNo)))
            {
                leftNav.Add(taskNo, new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>());
            }

            foreach (var task in taskList)
            {
                string taskNo = task[0].ToString();
                string taskRound = task[1].ToString();
                if (!leftNav[taskNo].ContainsKey(taskRound))
                    leftNav[taskNo].Add(taskRound, new Dictionary<string, Dictionary<string, List<string>>>());
            }

            foreach (var task in taskList)
            {
                string taskNo = task[0].ToString();
                string taskRound = task[1].ToString();
                string taskName = task[2].ToString();
                if (!leftNav[taskNo][taskRound].ContainsKey(taskName))
                    leftNav[taskNo][taskRound].Add(taskName, new Dictionary<string, List<string>>());
            }

            foreach (var task in taskList)
            {
                string taskNo = task[0].ToString();
                string taskRound = task[1].ToString();
                string taskName = task[2].ToString();
                string canRoad = task[3].ToString();
                if (!leftNav[taskNo][taskRound][taskName].ContainsKey(canRoad))
                    leftNav[taskNo][taskRound][taskName].Add(canRoad, new List<string>());
            }

            foreach (var task in taskList)
            {
                string taskNo = task[0].ToString();
                string taskRound = task[1].ToString();
                string taskName = task[2].ToString();
                string canRoad = task[3].ToString();
                string nodeName = task[4].ToString();
                leftNav[taskNo][taskRound][taskName][canRoad].Add(nodeName);
            }

            listTaskNo = new string[leftNav.Keys.Count];
            leftNav.Keys.CopyTo(listTaskNo, 0);


            cmbTaskNo.Properties.Items.AddRange(listTaskNo);
        }

        private void cmbTaskNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTaskNo.SelectedIndex == -1)
            {
                cmbTestRound.Enabled = false;
                cmbTestName.Enabled = false;
                cmbCANRoad.Enabled = false;
            }
            else
            {
                cmbTestRound.Enabled = true;
                cmbTestRound.Properties.Items.Clear();
                cmbTestName.Properties.Items.Clear();
                cmbCANRoad.Properties.Items.Clear();
                navTask.Items.Clear();
                cmbTestRound.SelectedIndex = 0;
                cmbTestName.SelectedIndex = 0;
                cmbCANRoad.SelectedIndex = 0;



                string taskNo = cmbTaskNo.SelectedItem.ToString();

                string[] listTaskRound = new string[leftNav[taskNo].Keys.Count];
                if (!leftNav.Keys.Contains(cmbTaskNo.SelectedItem.ToString()))
                {
                    XtraMessageBox.Show(DLAF.LookAndFeel, this, "没有该编号的车型，请重新选择...", "", new[] {DialogResult.OK}, null,
                        0, MessageBoxIcon.Information);
                    return;
                }

                leftNav[cmbTaskNo.SelectedItem.ToString()].Keys.CopyTo(listTaskRound, 0);
                cmbTestRound.Properties.Items.Clear();
                cmbTestRound.Properties.Items.AddRange(listTaskRound);
            }
        }

        private void cmbTestRound_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTaskNo.SelectedIndex == -1)
            {
                cmbTestName.Enabled = false;
                cmbCANRoad.Enabled = false;
            }
            else
            {
                string taskNo = cmbTaskNo.SelectedItem.ToString();
                string round = cmbTestRound.SelectedItem.ToString();
                cmbTestName.Enabled = true;
                if (!leftNav[taskNo].Keys.Contains(round))
                {
                    XtraMessageBox.Show(DLAF.LookAndFeel, this, "没有该轮次，请重新选择...", "", new[] {DialogResult.OK}, null, 0,
                        MessageBoxIcon.Information);
                    return;
                }

                string[] listTaskName = new string[leftNav[taskNo][round].Keys.Count];
                leftNav[taskNo][round].Keys.CopyTo(listTaskName, 0);
                cmbTestName.Properties.Items.Clear();
                cmbCANRoad.Properties.Items.Clear();
                navTask.Items.Clear();
                cmbTestName.SelectedIndex = 0;
                cmbCANRoad.SelectedIndex = 0;
                cmbTestName.Properties.Items.AddRange(listTaskName);
            }
        }

        private void cmbTestName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTaskNo.SelectedIndex == -1)
            {
                cmbCANRoad.Enabled = false;
            }
            else
            {
                cmbCANRoad.Enabled = true;
                string taskNo = cmbTaskNo.SelectedItem.ToString();
                string round = cmbTestRound.SelectedItem.ToString();
                string testName = cmbTestName.SelectedItem.ToString();
                if (!leftNav[taskNo][round].Keys.Contains(testName))
                {
                    XtraMessageBox.Show(DLAF.LookAndFeel, this, "没有该测试任务，请重新选择...", "", new[] {DialogResult.OK}, null,
                        0, MessageBoxIcon.Information);
                    return;
                }

                string[] listCANRound = new string[leftNav[taskNo][round][testName].Keys.Count];
                leftNav[taskNo][round][testName].Keys.CopyTo(listCANRound, 0);
                cmbCANRoad.Properties.Items.Clear();
                navTask.Items.Clear();
                cmbCANRoad.SelectedIndex = 0;
                cmbCANRoad.Properties.Items.AddRange(listCANRound);
            }

        }

        private void cbCANRoad_SelectedIndexChanged(object sender, EventArgs e)
        {
            row = 0;
            navTask.Items.Clear();
            string taskNo = cmbTaskNo.SelectedItem.ToString();
            string round = cmbTestRound.SelectedItem.ToString();
            string testName = cmbTestName.SelectedItem.ToString();
            string canRoad = cmbCANRoad.SelectedItem.ToString();
            if (!leftNav[taskNo][round][testName].Keys.Contains(canRoad))
            {
                XtraMessageBox.Show(DLAF.LookAndFeel, this, "没有该测试任务，请重新选择...", "", new[] {DialogResult.OK}, null, 0,
                    MessageBoxIcon.Information);
                return;
            }

            List<string> listNode = leftNav[taskNo][round][testName][canRoad];
            row = 0;
            GetModuleString(testName, listNode);

            //foreach (string node in listNode)
            //{
            //    //nodeList.AddItem();
            //    //navTask.Items[row].SmallImage = imageCollection.Images[0];
            //    //navTask.Items[row].Caption = node + @" 节点";

            //    //navTask.Items[row].LinkClicked += NameItem_Click;
            //    //row++;
            //}

        }

        private void GetModuleString(string node, List<string> listNode)
        {
            //string module = "";
            if (node != "")
            {
                switch (node)
                {
                    case "CAN通信单元":
                    case "LIN通信主节点":
                    case "LIN通信从节点":
                    case "直接NM单元":
                    case "动力域NM主节点":
                    case "动力域NM从节点":
                    case "间接NM单元":
                    case "通信DTC":
                    case "OSEK NM单元":
                    case "Bootloader":
                    case "网关路由":
                        GetSingleNode(listNode);
                        break;
                    case "CAN通信集成":
                    case "LIN通信集成":
                    case "直接NM集成":
                    case "间接NM集成":
                    case "动力域NM集成":
                    case "OSEK NM集成":
                        GetMultiNodeString(listNode);
                        break;
                    default:
                        break;
                }

            }

        }

        private void GetSingleNode(List<string> listNode)
        {
            foreach (string node in listNode)
            {
                nodeList.AddItem();
                navTask.Items[row].SmallImage = imageCollection.Images[0];
                navTask.Items[row].Caption = node + @" 节点";
                navTask.Items[row].Hint = node + @" 节点";
                ;

                navTask.Items[row].LinkClicked += NameItem_Click;
                row++;
            }
        }

        private void GetMultiNodeString(List<string> list)
        {
            foreach (string module in list)
            {
                Dictionary<string, string> name = Json.DerJsonToDict(module);
                string taskModule = "";
                foreach (KeyValuePair<string, string> item in name)
                {

                    if (item.Key == "Virtual")
                    {
                        if (taskModule != "")
                        {
                            taskModule = taskModule + item.Value + "/";
                        }

                    }
                    else if (item.Key == "Normal")
                    {
                        taskModule = taskModule + item.Value + "/";

                    }

                }

                taskModule = taskModule.Remove(taskModule.Length - 1) + @" 节点";
                nodeList.AddItem();
                navTask.Items[row].SmallImage = imageCollection.Images[0];
                navTask.Items[row].Caption = taskModule;
                navTask.Items[row].Hint = taskModule;
                navTask.Items[row].LinkClicked += NameItem_Click;
                row++;
            }

        }

        private void NameItem_Click(object sender, NavBarLinkEventArgs e)
        {
            if (GloalVar.IsDPause || GloalVar.IsHPause)
            {
                XtraMessageBox.Show("当前正在测试无法重新测试，如需必要请先停止当前测试...");
                return;
            }

            GlobalVar.ErrorInfo.Clear();
            try
            {
                SplashScreenManager.ShowForm(typeof(wfMain), false, true);
            }
            catch (Exception exception)
            {
                if (exception.Message == "Splash Form has already been displayed")
                {
                    SplashScreenManager.CloseForm();
                    SplashScreenManager.ShowForm(typeof(wfMain), false, true);
                }
            }
            var item = sender as NavBarItem;
            if (GlobalVar.OldBarItem != null && GlobalVar.OldBarItem != item)
            {
                GlobalVar.OldBarItem.Appearance.ForeColor = Color.Black;

            }

            GlobalVar.CurrentTsNode.Clear();
            if (item != null)
            {
                item.Appearance.ForeColor = Color.Red;
                GlobalVar.OldBarItem = item;
            }

            GlobalVar.CurrentTsNode.Add(cmbTaskNo.SelectedItem.ToString());
            GlobalVar.CurrentTsNode.Add(cmbTestRound.SelectedItem.ToString());
            GlobalVar.CurrentTsNode.Add(cmbTestName.SelectedItem.ToString());
            GlobalVar.CurrentTsNode.Add(cmbCANRoad.SelectedItem.ToString());
            GlobalVar.CurrentTsNode.Add(item.Caption.Split(' ')[0]);
            //pcTest.Controls.Clear();
            //var ts = new TestWait(this) { Dock = DockStyle.Fill };
            //pcTest.Controls.Add(ts);


            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("TaskNo", GlobalVar.CurrentTsNode[0]);
            dict.Add("TaskRound", GlobalVar.CurrentTsNode[1]);
            dict.Add("TaskName", GlobalVar.CurrentTsNode[2]);
            dict.Add("CANRoad", GlobalVar.CurrentTsNode[3]);
            string module = GetModuleText(cmbTestName.SelectedItem.ToString(), GlobalVar.CurrentTsNode[4].ToString());
            GlobalVar.ModuleJson = module;
            dict.Add("Module", module);
            Module = item.Caption.Split(' ')[0];
            string taskName = GlobalVar.CurrentTsNode[2];
            //var a = GlobalVar.CurrentTsNode[4].ToString().Split(' ');

            //dict.Add("Module", a[0]);
            //Module = a[0];
            IList<object[]> taskList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.TaskTest, dict);
            DateTime nowTime = DateTime.Today;

            if (role == "tester")
                if (taskList[0][7].ToString() != GlobalVar.UserName)
                {
                    XtraMessageBox.Show(DLAF.LookAndFeel, this, "您没有权限测试该模块...", "", new[] {DialogResult.OK}, null, 0,
                        MessageBoxIcon.Information);
                    SplashScreenManager.CloseForm();
                    return;
                }

            if (taskList.Count == 0)
            {
                XtraMessageBox.Show(DLAF.LookAndFeel, this, "任务管理中没有该项任务，请先去任务管理中添加...", "", new[] {DialogResult.OK},
                    null, 0, MessageBoxIcon.Information);
                SplashScreenManager.CloseForm();
                return;
            }

            //if (taskList[0][11].ToString() == "")
            //{
            //    XtraMessageBox.Show(DLAF.LookAndFeel, this, "任务管理中未完成用例表...", "", new[] {DialogResult.OK}, null, 0,
            //        MessageBoxIcon.Information);
            //    SplashScreenManager.CloseForm();
            //    return;
            //}

            if (DateTime.Compare(nowTime, Convert.ToDateTime(taskList[0][12].ToString())) < 0)
            {
                XtraMessageBox.Show(DLAF.LookAndFeel, this, "当前日期不在该任务的授权时间内，目前还不能测试...", "", new[] {DialogResult.OK},
                    null, 0, MessageBoxIcon.Information);
                SplashScreenManager.CloseForm();
                return;
            }

            if (DateTime.Compare(nowTime, Convert.ToDateTime(taskList[0][13].ToString())) > 0)
            {
                XtraMessageBox.Show(DLAF.LookAndFeel, this, "当前日期已经超出该任务的失效时间，已不能测试...", "", new[] {DialogResult.OK},
                    null, 0, MessageBoxIcon.Information);
                SplashScreenManager.CloseForm();
                return;
            }

            bool exitsingle = false; //单节点
            bool Integrate = false; //集成测试
            bool isCAN = false;
            List<string> dbc = new List<string>();
            string Task = cmbTaskNo.SelectedItem.ToString().Trim();
            string[] arrStr = Task.Split('-'); //按逗号截取 
            dbc.Add(arrStr[0]);
            dbc.Add(arrStr[1]);
            dbc.Add(arrStr[2]);
            dbc.Add(cmbCANRoad.SelectedItem.ToString().Trim());
            if (cmbCANRoad.SelectedItem.ToString().Trim().Contains("CAN"))
            {
                isCAN = true;
            }
            else
            {
                isCAN = false;
            }

            List<string> list = _show.ObtainCorrntImf(dbc);
            String strDBC = list[5].ToString().Trim();
            string path = AppDomain.CurrentDomain.BaseDirectory + strDBC;
            var exValue = 0;
            List<String> nodeListNew = _show.ObtainNode(isCAN, exValue, path);
            if (!GlobalVar.CurrentTsNode[2].ToString().Contains("集成"))
            {
                if (nodeListNew.Contains(GlobalVar.CurrentTsNode[4].ToString().Trim()))
                {
                    exitsingle = true;
                }
                else
                {
                    exitsingle = false;
                }
            }
            else
            {
                Integrate = true;
                string[] arrStr1 = GlobalVar.CurrentTsNode[4].Split('/'); //按逗号截取 
                string[] arrAdd = arrStr1.Except(nodeListNew).ToArray();

                StringBuilder sb = new StringBuilder();
                if (arrAdd.Length > 0)
                {
                    for (int k = 0; k < arrAdd.Length; k++)
                    {
                        sb.Append(arrAdd[k].ToString().Trim());
                    }

                    XtraMessageBox.Show(DLAF.LookAndFeel, this, "数据库文件中不存在 " + sb + "节点：请检查数据库文件...", "",
                        new[] {DialogResult.OK}, null, 0, MessageBoxIcon.Information);
                    SplashScreenManager.CloseForm();
                    return;
                }
            }

            if (!Integrate)
            {
                if (!exitsingle)
                {
                    XtraMessageBox.Show(DLAF.LookAndFeel, this, "数据库文件中不存在此节点：请检查数据库文件...", "", new[] {DialogResult.OK},
                        null, 0, MessageBoxIcon.Information);
                    SplashScreenManager.CloseForm();
                    return;
                }
            }

            GlobalVar.CurrentTsNode[4] = GlobalVar.CurrentTsNode[4].Split(' ')[0];
            bool issuccess = NodeStr.SetVNode(GlobalVar.CurrentTsNode);

            pcTest.Controls.Clear();
            TestStart ts = new TestStart(Module, taskName);
            ts.Dock = DockStyle.Fill;
            pcTest.Controls.Add(ts);
            //Loading();
            SplashScreenManager.CloseForm();
            dict.Clear();
        }

        private string GetModuleText(string node, string listNode)
        {
            string module = "";
            if (node != "")
            {
                switch (node)
                {
                    case "CAN通信单元":
                    case "LIN通信主节点":
                    case "LIN通信从节点":
                    case "直接NM单元":
                    case "动力域NM主节点":
                    case "动力域NM从节点":
                    case "间接NM单元":
                    case "通信DTC":
                    case "OSEK NM单元":
                    case "Bootloader":
                    case "网关路由":
                        module = listNode.Split(' ')[0];
                        break;
                    case "CAN通信集成":
                    case "LIN通信集成":
                    case "直接NM集成":
                    case "间接NM集成":
                    case "动力域NM集成":
                    case "OSEK NM集成":
                        module = GetMultiModuleString(listNode);
                        break;
                    default:
                        break;
                }
            }

            return module;
        }

        private string GetMultiModuleString(string nodeStr)
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

        public static DialogResult Show(UserLookAndFeel look, IWin32Window owner, string text, string caption,
            DialogResult[] buttons, Icon icon, int defaultButton, MessageBoxIcon messageBeepSound)
        {

            DevExpress.XtraEditors.XtraMessageBoxForm form = new XtraMessageBoxForm();
            Font defaultFont = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            form.Appearance.Font = defaultFont;
            return form.ShowMessageBoxDialog(new XtraMessageBoxArgs(look, owner, text, caption, buttons, icon,
                defaultButton));
        }
    }
}