using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Helpers;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraSplashScreen;
using UltraANetT.Interface;
using UltraANetT.Module;
using DevExpress.XtraEditors;
using System.Diagnostics;
using System.Linq;
using ProcessEngine;
using ReportEditor;
using System.Threading;
using CANoeEngine;
using Model;
using Department = UltraANetT.Module.Department;
using Employee = UltraANetT.Module.Employee;
using OperationLog = UltraANetT.Module.OperationLog;
using Task = UltraANetT.Module.Task;

namespace UltraANetT.Form
{
    public partial class Main : RibbonForm, INode
    {
        /// <summary>
        ///     按钮缓存
        /// </summary>
        private BarBaseButtonItem _barContainer;

        public bool IsHaveCurrentNode = false;
        ProcStore _store = new ProcStore();
        ProcFile _file = new ProcFile();
        private LogicalControl _LogC = new LogicalControl();
        private ProcCANoeTest CANoeTest = new ProcCANoeTest();
        private string role;

        public Main()
        {
            InitializeComponent();
            InitSkinGallery();
            SkinSelect();
            siCurrentAccount.Caption = GlobalVar.UserName;
            gbtVehicel.SuperTip = null;
            role = _LogC.RoleSelect(GlobalVar.UserName);
            RoleFunction(role);
            siStatus.Caption =
                "主程序版本号：" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //IsConfigConfigurationPath();
            //ThreadActivation();
        }

        private void RoleFunction(string role)
        {
            switch (role)
            {
                case "superadminister":
                    pageTools.Visible = true;
                    pageManage.Visible = true;
                    break;
                case "administer":
                    pageTools.Visible = true;
                    pageManage.Visible = false;
                    gbtSegment.Visibility = BarItemVisibility.Never;
                    gbtNodeConfigurationBox.Visibility = BarItemVisibility.Never;
                    gbtFaultTypeBox.Visibility = BarItemVisibility.Never;
                    gbtSet.Visibility = BarItemVisibility.Never;
                    gbtExapChapter.Visibility = BarItemVisibility.Never;
                    break;
                case "configurator":
                case "tester":
                    pageTools.Visible = false;
                    pageManage.Visible = false;
                    break;
                default:
                    break;
            }
        }

        private void InitSkinGallery()
        {
            SkinHelper.InitSkinGallery(rgbiSkins, true);
        }

        private void ThreadActivation()
        {
            Thread th = new Thread(SoftwareActivation);
            th.Start();
        }

        private void SoftwareActivation()
        {
            while (true)
            {
                if (!ProcessEngine.HASPDog.IsActivate())
                {
                    this.Enabled = false;
                    Thread thr = new Thread(KillFrom);
                    thr.Start();
                    XtraMessageBox.Show(DLAF.LookAndFeel, "未检测到硬件狗，本软件将在5秒后关闭。", "激活", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Process.GetCurrentProcess().Kill();
                }

                Thread.Sleep(5000);
            }
        }

        private void KillFrom()
        {
            Thread.Sleep(5000);
            Process.GetCurrentProcess().Kill();
        }

        private void SkinSelect()
        {
            skinView.Gallery.Groups[0].Items.RemoveAt(0);
            skinView.Gallery.Groups[0].Items.RemoveAt(0);
            for (var i = 0; i < 10; i++)
                skinView.Gallery.Groups[0].Items.RemoveAt(3);
            for (var i = 0; i < 16; i++)
                skinView.Gallery.Groups[1].Items.RemoveAt(0);
            for (var i = 0; i < 9; i++)
                skinView.Gallery.Groups[1].Items.RemoveAt(1);
            skinView.Gallery.Groups[2].Items.Clear();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Prepare();
            var vehicel = new Vehicel();
            vehicel.NodeStr = this;
            DrawControl(vehicel, gbtVehicel);


            #region 赋值

            EnumLibrary.SelfPath = _file.ReadLocalXml(@"xml\path.xml", @"Product/SelfPath");
            EnumLibrary.CfgPath = _file.ReadLocalXml(@"xml\path.xml", @"Product/CfgPath");

            EnumLibrary.CANSigExam = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigExam");
            EnumLibrary.CANSigReport = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigReport");

            EnumLibrary.CANLtgExam = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgExam");
            EnumLibrary.CANLtgReport = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgReport");

            EnumLibrary.LINSigExam = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigExam");
            EnumLibrary.LINSigReport = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigReport");

            EnumLibrary.LINSigFromExam = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigFromExam");
            EnumLibrary.LINSigFromReport = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigFromReport");

            EnumLibrary.LINLtgExam = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgExam");
            EnumLibrary.LINLtgReport = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgReport");

            EnumLibrary.WifiReport = _file.ReadLocalXml(@"xml\path.xml", @"Product/WifiReport");
            EnumLibrary.WifiExam = _file.ReadLocalXml(@"xml\path.xml", @"Product/WifiExam");

            EnumLibrary.OSEKSigReport = _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigReport");
            EnumLibrary.OSEKSigExam = _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigExam");

            EnumLibrary.OSEKLtnReport = _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnReport");
            EnumLibrary.OSEKLtnExam = _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnExam");

            EnumLibrary.DTCExam = _file.ReadLocalXml(@"xml\path.xml", @"Product/DTCExam");
            EnumLibrary.DTCreport = _file.ReadLocalXml(@"xml\path.xml", @"Product/DTCreport");

            EnumLibrary.AutoSARNMExam = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMExam");
            EnumLibrary.AutoSARNMReport = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMReport");

            EnumLibrary.AutoSARNMLtgExam = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtgExam");
            EnumLibrary.AutoSARNMLtgReport = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtgReport");

            EnumLibrary.DynamicNMExam = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMExam");
            EnumLibrary.DynamicNMReport = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMReport");

            EnumLibrary.DynamicNMFromExam = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFromExam");
            EnumLibrary.DynamicNMFromReport = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFromReport");

            EnumLibrary.DynamicNMLtgExam = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtgExam");
            EnumLibrary.DynamicNMLtgReport = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtgReport");

            EnumLibrary.IndirectExam = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectExam");
            EnumLibrary.IndirectReport = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectReport");

            EnumLibrary.IndirectLtgExam = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtgExam");
            EnumLibrary.IndirectLtgReport = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtgReport");

            #region 1939
            EnumLibrary.CANSig1939Report = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSig1939Report");
            EnumLibrary.CANSig1939Exam = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSig1939Exam");

            EnumLibrary.CANLtg1939Report = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtg1939Report");
            EnumLibrary.CANLtg1939Exam = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtg1939Exam");

            EnumLibrary.AutoSARNM1939Report = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNM1939Report");
            EnumLibrary.AutoSARNM1939Exam = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNM1939Exam");

            EnumLibrary.AutoSARNMLtg1939Report = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtg1939Report");
            EnumLibrary.AutoSARNMLtg1939Exam = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtg1939Exam");

            EnumLibrary.DynamicNM1939Report = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNM1939Report");
            EnumLibrary.DynamicNM1939Exam = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNM1939Exam");

            EnumLibrary.DynamicNMFrom1939Report = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFrom1939Report");
            EnumLibrary.DynamicNMFrom1939Exam = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFrom1939Exam");

            EnumLibrary.DynamicNMLtg1939Report = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtg1939Report");
            EnumLibrary.DynamicNMLtg1939Exam = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtg1939Exam");

            EnumLibrary.Indirect1939Report = _file.ReadLocalXml(@"xml\path.xml", @"Product/Indirect1939Report");
            EnumLibrary.Indirect1939Exam = _file.ReadLocalXml(@"xml\path.xml", @"Product/Indirect1939Exam");

            EnumLibrary.IndirectLtg1939Report = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtg1939Report");
            EnumLibrary.IndirectLtg1939Exam = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtg1939Exam");

            #endregion
            #endregion

            //Thread td = new Thread(IsConfigConfigurationPath);
            //td.SetApartmentState(ApartmentState.STA);
            //td.Start();

        }

        private void IsConfigConfigurationPath()
        {
            bool isPath = Convert.ToBoolean(_file.ReadLocalXml(@"xml\path.xml", @"Product/IsFirst"));
            if (isPath)
            {
                XtraMessageBox.Show("该软件使用前，请先确认文件路径，确认后如需修改请联系超级管理员修改");
                FilePath filePath = new FilePath();
                filePath.ShowDialog();

            }

        }

        private void gbtEmployee_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                Prepare();
                var eml = new Employee();
                DrawControl(eml, gbtEmployee);
            }
        }

        private void gbtDepartment_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                Prepare();
                var dept = new Department();
                DrawControl(dept, gbtDepartment);
            }
        }

        private void gbtDeviceInfo_ItemClick(object sender, ItemClickEventArgs e)
        {
            //pcMain.Controls.Clear();
            //SplashScreenManager.ShowForm(typeof (wfMain), false, true);
            //var hardInfo = new HardwareInfo();
            //hardInfo.Dock = DockStyle.Fill;
            //pcMain.Controls.Add(hardInfo);
            //SplashScreenManager.CloseForm();
            //if (_barContainer != null)
            //    _barContainer.ItemAppearance.Normal.BackColor = Color.Transparent;
            //gbtDeviceInfo.ItemAppearance.Normal.BackColor = Color.Silver;
            //_barContainer = gbtDeviceInfo;
        }

        private void gbtDeviceCheck_ItemClick(object sender, ItemClickEventArgs e)
        {
            pcMain.Controls.Clear();
            SplashScreenManager.ShowForm(typeof(wfMain), false, true);
            //var hardCheck = new HardwareCheck();
            //hardCheck.Dock = DockStyle.Fill;
            //pcMain.Controls.Add(hardCheck);
            SplashScreenManager.CloseForm();
            if (_barContainer != null)
                _barContainer.ItemAppearance.Normal.BackColor = Color.Transparent;
            gbtDeviceInfo.ItemAppearance.Normal.BackColor = Color.Silver;
            _barContainer = gbtDeviceInfo;
        }

        private void gbtTools_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                pcMain.Controls.Clear();
                SplashScreenManager.ShowForm(typeof(wfMain), false, true);
                var tools = new Tools();
                tools.Dock = DockStyle.Fill;
                pcMain.Controls.Add(tools);
                SplashScreenManager.CloseForm();
                if (_barContainer != null)
                    _barContainer.ItemAppearance.Normal.BackColor = Color.Transparent;
                gbtTools.ItemAppearance.Normal.BackColor = Color.Silver;
                _barContainer = gbtTools;
            }
        }

        private void gbtPassword_ItemClick(object sender, ItemClickEventArgs e)
        {
            pcMain.Controls.Clear();
            //SplashScreenManager.ShowForm(typeof (wfMain), false, true);
            //var pwd = new UserPwd();
            //pwd.Dock = DockStyle.Fill;
            //pcMain.Controls.Add(pwd);
            //SplashScreenManager.CloseForm();
            if (_barContainer != null)
                _barContainer.ItemAppearance.Normal.BackColor = Color.Transparent;
            gbtPassword.ItemAppearance.Normal.BackColor = Color.Silver;
            _barContainer = gbtPassword;
            ModifyPwd Mpwd = new ModifyPwd();

            Mpwd.Show();
        }

        private void gbtVisitLog_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                Prepare();
                var LogInfo = new LogInfo();
                DrawControl(LogInfo, gbtAuto);
            }
        }

        private void gbtVehicel_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                Prepare();
                var vehicel = new Vehicel();
                vehicel.NodeStr = this;
                DrawControl(vehicel, gbtVehicel);
            }
        }

        private void gbtAuto_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                if (CheckTest())
                {
                    return;

                }

                Prepare();
                GlobalVar.ErrorInfo.Clear();
                var test = new Test();
                test.NodeStr = this;
                DrawControl(test, gbtAuto);
            }
        }

        private void gbtTask_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                Prepare();
                var task = new Task();
                task.NodeStr = this;
                DrawControl(task, gbtTask);
            }

        }

        bool INode.SetVNode(List<string> vNode)
        {
            if (vNode.Count == 3)
            {
                lblVNode.Caption = vNode[0] + @"-" + vNode[1] + @"-" + vNode[2];
                GlobalVar.VehicelNode = lblVNode.Caption;
            }
            else if (vNode.Count == 2)
            {
                lblVNode.Caption = vNode[0] + @"-" + vNode[1];
                GlobalVar.VehicelNode = lblVNode.Caption;

            }
            else if (vNode.Count == 1)
            {
                lblVNode.Caption = vNode[0];
                GlobalVar.VehicelNode = lblVNode.Caption;
            }
            else if (vNode.Count == 5)
            {
                lblVNode.Caption = vNode[0] + @"-" + vNode[1] + @"-" + vNode[2] + @"-" +
                                   vNode[3] + @"-" + vNode[4];
                GlobalVar.VehicelNode = lblVNode.Caption;
            }
            else if (vNode.Count == 6)
            {
                lblVNode.Caption = vNode[0] + @"-" + vNode[1] + @"-" + vNode[2] + @"-" +
                                   vNode[3] + @"-" + vNode[4] + @"-" + vNode[5];
            }

            IsHaveCurrentNode = true;
            return true;
        }

        private void Prepare()
        {
            pcMain.Controls.Clear();
            //SplashScreenManager.ShowForm(typeof(wfMain), false, true);
            //SplashScreenManager.CloseForm();
        }

        private void DrawControl(XtraUserControl userControl, BarButtonItem item)
        {
            userControl.Dock = DockStyle.Fill;
            pcMain.Controls.Add(userControl);
            if (_barContainer != null)
                _barContainer.ItemAppearance.Normal.BackColor = Color.Transparent;
            item.ItemAppearance.Normal.BackColor = Color.Silver;
            _barContainer = item;
        }

        private void gbtOperateLog_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                Prepare();
                var log = new OperationLog();
                DrawControl(log, gbtOperateLog);
            }
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormClose();
        }

        private void iOpen_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (XtraMessageBox.Show("确认要切换用户么？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                GlobalVar.UserLogin = false;
                Prepare();
            }
        }

        private void iExit_ItemClick(object sender, ItemClickEventArgs e)
        {
            FormClose();
        }

        private void FormClose()
        {
            ProcLog Login = new ProcLog();
            Login.UpdateLoginNo();
            Process.GetCurrentProcess().Kill();
        }

        private void iUpdataPwd_ItemClick(object sender, ItemClickEventArgs e)
        {
            ModifyPwd mpwd = new ModifyPwd();
            mpwd.ShowDialog();
        }

        private void ribbonControl_SelectedPageChanged(object sender, EventArgs e)
        {
            var Page = sender as RibbonControl;
            string PageName = Page.SelectedPage.ToString();
            switch (PageName)
            {
                case "配置管理":
                    Prepare();
                    var vehicel = new Vehicel();
                    vehicel.NodeStr = this;
                    DrawControl(vehicel, gbtVehicel);
                    break;
                case "测试管理":
                    Prepare();
                    var test = new Test();
                    test.NodeStr = this;
                    DrawControl(test, gbtAuto);
                    break;
                case "工具管理":
                    Prepare();
                    FilePathCfg filePath = new FilePathCfg();
                    DrawControl(filePath, gbtSet);
                    break;
                case "日志管理":
                    Prepare();
                    var LogInfo = new LogInfo();
                    DrawControl(LogInfo, gbtVisitLog);
                    break;
                case "数据管理":
                    Prepare();
                    var ReportL = new ReportList();
                    DrawControl(ReportL, gbtReportList);
                    break;
                case "用户管理":
                    Prepare();
                    var eml = new Employee();
                    DrawControl(eml, gbtEmployee);
                    break;
                default:
                    break;
            }
        }


        private void gbtQuestion_ItemClick(object sender, ItemClickEventArgs e)
        {
            Prepare();
            var quesNote = new Module.QuestionNote();
            //vehicel.NodeStr = this;
            DrawControl(quesNote, gbtQuestion);
        }

        private void gbtComparison_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            Prepare();
            var passNote = new Module.PassReportNote();
            //vehicel.NodeStr = this;
            DrawControl(passNote, gbtComparison);
        }

        private void ribbonControl_Click(object sender, EventArgs e)
        {

        }

        private void gbtSegment_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                Prepare();
                var Seg = new Module.Segment();
                DrawControl(Seg, gbtSegment);
            }
        }

        private void gbtNodeConfigurationBox_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                Prepare();
                var Box = new Module.NodeConfigurationBox();
                DrawControl(Box, gbtNodeConfigurationBox);
            }
        }

        private void gbtFaultType_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                Prepare();
                var Box = new Module.FaultType();
                DrawControl(Box, gbtFaultTypeBox);
            }
        }

        private void gbtSet_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                Prepare();
                FilePathCfg filePath = new FilePathCfg();
                DrawControl(filePath, gbtSet);
            }
        }

        private void barButtonItem10_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                Prepare();
                var Box = new Module.ExapChapter();
                DrawControl(Box, gbtExapChapter);
            }
        }

        private void SuppierMan_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                Prepare();
                var task = new Suppier();
                DrawControl(task, SuppierMan);
            }
        }



        private bool CheckTest()
        {
            if (_barContainer == gbtAuto && (GloalVar.IsHPause || GloalVar.IsDPause))
            {
                XtraMessageBox.Show("当前正在测试无法切换界面，如需必要请先停止当前测试...");
                return true;
            }

            return false;
        }

        private void ribbonControl_SelectedPageChanging(object sender, RibbonPageChangingEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                if (CheckTest())
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void gbtUpload_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                Upload upload = new Upload();
                upload.ShowDialog();
            }
        }

        private void gbtEmlTemplate_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                Prepare();
                var eml = new EmlLibrary();
                DrawControl(eml, gbtEmlTemplate);
            }
        }

        private void gbtReportList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GlobalVar.WhetherPerform())
            {
                Prepare();
                var ReportL = new ReportList();
                DrawControl(ReportL, gbtReportList);
            }
        }
    }
}