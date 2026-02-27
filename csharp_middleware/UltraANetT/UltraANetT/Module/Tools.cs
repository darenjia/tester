using System;
using System.Collections.Generic;
using System.Drawing;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using FileEditor;
using ProcessEngine;
using ReportEditor;

namespace UltraANetT.Module
{
    public partial class Tools : XtraUserControl
    {

        private PictureEdit _pictCache;
        ProcStore _store = new ProcStore();
        ProcFile _file = new ProcFile();
        public Tools()
        {
            InitializeComponent();
        }

        private void pictureEditDocEdit_Click(object sender, System.EventArgs e)
        {
            if (!FileEditor.pubClass.GlobalVar.isRun)
            {
                _pictCache = pictureEditDocEdit;
                FileEditor.pubClass.GlobalVar.IsIndependent = true;
                FileEditor.Editor edit = new Editor(false);
                edit.Show();
                Recognize();
                FileEditor.pubClass.GlobalVar.isRun = true;
            }
            else
            {
                XtraMessageBox.Show("已经运行了一个实例...");
            }

        }

        private void pictureEditReportView_Click(object sender, System.EventArgs e)
        {
            if (!ReportEditor.GlobalVar.isRun)
            {
                _pictCache = pictureEditReportView;
                Recognize();
                RibbonForm report = new ReportView();
                report.Show();
                ReportEditor.GlobalVar.isRun = true;
            }
            else
            {
                XtraMessageBox.Show("已经运行了一个实例...");
            }
           


        }
        private void Recognize()
        {
            if (_pictCache == pictureEditDocEdit)
            {
                pictureEditDocEdit.Image = Properties.Resources.DocEditor_hover;
                pictureEditReportView.Image = Properties.Resources.ReportViewer;
                pictureEditHardwareCheck.Image = Properties.Resources.HardwareCheck;
            }
            if (_pictCache == pictureEditReportView)
            {
                pictureEditReportView.Image = Properties.Resources.ReportViewer_hover;
                pictureEditDocEdit.Image = Properties.Resources.DocEditor;
                pictureEditHardwareCheck.Image = Properties.Resources.HardwareCheck;
            }
            if (_pictCache == pictureEditHardwareCheck)
            {
                pictureEditHardwareCheck.Image = Properties.Resources.HardwareCheck_hover;
                pictureEditReportView.Image = Properties.Resources.ReportViewer;
                pictureEditDocEdit.Image = Properties.Resources.DocEditor;
            }
        }

        private void pictureEditHardwareCheck_Click(object sender, EventArgs e)
        {
            //_pictCache = pictureEditHardwareCheck;
            //Form.HardwareCheck hc = new Form.HardwareCheck();
            //hc.Show();
        }
    }
}