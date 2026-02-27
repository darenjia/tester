using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraEditors;
using ProcessEngine;
using DevExpress.XtraSplashScreen;
using FileEditor.Control;
using FileEditor.pubClass;

namespace FileEditor
{
    public partial class Editor : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private ITemplate _temCfg;
        private ITemplate _temEml;
        private readonly ProcShow _show;
        private readonly ProcFile _file;
        private bool _isOnlyEml = false;
        private EmlTemplate eml;
        private CfgTemplate cfg;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isOnlyEml">是否仅显示用例模板</param>
        public Editor(bool isOnlyEml)
        {
            InitializeComponent();
            _isOnlyEml = isOnlyEml;
            //GetTemStauts(null);


            _temCfg = cfgTemp;
            GlobalVar.CfgCache = cfgTemp;
            _show = new ProcShow();
            if (GlobalVar.IsIndependent == false)
            {
                if (_isOnlyEml)
                {
                    //从任务进入
                    pcContainer.Controls.Clear();
                    SplashScreenManager.ShowForm(typeof (wfMain), false, true);
                    eml = new EmlTemplate(_isOnlyEml) {Dock = DockStyle.Fill};
                    pcContainer.Controls.Add(eml);
                    SplashScreenManager.CloseForm();
                    navEditor.Enabled = false;
                    navEditor.AppearanceItem.Normal.BackColor = Color.Gray;


                }
                else
                {
                    //_show.GetDataFromDbc(ref GlobalVar.ListCAN, ref GlobalVar.GetNodeList, GlobalVar.VNode);
                    _temCfg.SetAttribute(GlobalVar.VNode);
                    _temCfg.DrawNav();


                    cfgTemp.InitItemName();
                    navEditor.Items[0].Select();
                    GlobalVar.SelectName = "";
                }

            }
            else
            {
                _temCfg.SetAttribute(null);
                _temCfg.DrawNav();
                navEditor.Items[0].Select();
                GlobalVar.SelectName = "";
            }

        }

        private void navEditor_ItemClick(object sender, DevExpress.XtraBars.Navigation.NavigationBarItemEventArgs e)
        {
            //异常处理
            switch (e.Item.Name)
            {
                case "navCfg":
                    pcContainer.Controls.Clear();
                    SplashScreenManager.ShowForm(typeof (wfMain), false, true);
                    cfg = new CfgTemplate {Dock = DockStyle.Fill};
                    GlobalVar.CfgCache = cfg;
                    pcContainer.Controls.Add(cfg);
                    SplashScreenManager.CloseForm();
                    _temCfg = cfg;
                    _temCfg.SetAttribute(GlobalVar.VNode);
                    _temCfg.DrawNav();

                    GlobalVar.SelectName = "";
                    break;
                case "navExmp":
                    pcContainer.Controls.Clear();
                    SplashScreenManager.ShowForm(typeof (wfMain), false, true);
                    GlobalVar.SelectName = "";
                    if (!GlobalVar.IsIndependent) //从车型配置进入
                    {
                        var em = new EmlTemplate(false) {Dock = DockStyle.Fill};
                        GlobalVar.EmlCache = em;
                        pcContainer.Controls.Add(em);
                        SplashScreenManager.CloseForm();
                        _temEml = em;
                        _temEml.SetAttribute(GlobalVar.VNode);
                        _temEml.DrawNav();

                        break;
                    }
                    else //从文件编辑器
                    {
                        GlobalVar.SelectName = "";
                        var em = new EmlTemplate() {Dock = DockStyle.Fill};
                        GlobalVar.EmlCache = em;
                        pcContainer.Controls.Add(em);
                        SplashScreenManager.CloseForm();
                        _temEml = em;
                        _temEml.DrawNav();
                        break;
                    }


                case "navProject":
                    pcContainer.Controls.Clear();
                    SplashScreenManager.ShowForm(typeof (wfMain), false, true);
                    var pro = new Project {Dock = DockStyle.Fill};
                    pcContainer.Controls.Add(pro);
                    SplashScreenManager.CloseForm();
                    GlobalVar.SelectName = "";
                    break;
            }
        }

        private void Editor_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (cfg != null)
                cfg.FormClosed();
            if (eml != null)
                eml.FormClosed();
            GlobalVar.isRun = false;
        }

        private void Editor_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
    }

}