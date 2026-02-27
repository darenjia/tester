using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using UltraANetT.Properties;
using ProcessEngine;
using System.Diagnostics;
using System.Threading;
using DevExpress.LookAndFeel;

namespace UltraANetT.Form
{
    public partial class GuideMain : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        Thread th;
        public GuideMain()
        {
            if (!ProcessEngine.HASPDog.IsActivate())
            {
                //Show(defaultLookAndFeel1.LookAndFeel, this, "未检测到硬件狗...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                XtraMessageBox.Show("未检测到硬件狗", "激活", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Process.GetCurrentProcess().Kill();
            }
            InitializeComponent();
            ThreadActivation();
        }

        private void ThreadActivation()
        {
            th = new Thread(SoftwareActivation);
            th.Start();
        }
        public static DialogResult Show(UserLookAndFeel look, IWin32Window owner, string text, string caption, DialogResult[] buttons, Icon icon, int defaultButton, MessageBoxIcon messageBeepSound)
        {

            DevExpress.XtraEditors.XtraMessageBoxForm form = new XtraMessageBoxForm();
            Font defaultFont = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            form.Appearance.Font = defaultFont;
            return form.ShowMessageBoxDialog(new XtraMessageBoxArgs(look, owner, text, caption, buttons, icon, defaultButton));
        }
        private void SoftwareActivation()
        {
            while(true)
            {
                if (!ProcessEngine.HASPDog.IsActivate())
                {
                    Thread thr = new Thread(KillFrom);
                    thr.Start();
                    XtraMessageBox.Show(defaultLookAndFeel1.LookAndFeel, "未检测到硬件狗，本软件将在5秒后关闭。", "激活", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        
        private void picNetTest_MouseMove(object sender, MouseEventArgs e)
        {
            picNetTest.Image = Resources.NetTest_hover;
        }

        private void picNetTest_MouseLeave(object sender, EventArgs e)
        {
            picNetTest.Image = Resources.NetTest;
        }

        private void picHelp_MouseMove(object sender, MouseEventArgs e)
        {
            picHelp.Image = Resources.Help_hover;
        }

        private void picHelp_MouseLeave(object sender, EventArgs e)
        {
            picHelp.Image = Resources.Help;
        }

        private void picExit_MouseMove(object sender, MouseEventArgs e)
        {
            picExit.Image = Resources.Exit_hover;
        }

        private void picExit_MouseLeave(object sender, EventArgs e)
        {
            picExit.Image = Resources.Exit;
        }

        private void picNetTest_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                return;
            LoginAndRegister();
        }

        private void LoginAndRegister()
        {
            this.Hide();
            var login = new UserLogin();
            login.ShowDialog();
            if (login.DialogResult == DialogResult.OK)
            {
                this.DialogResult = DialogResult.OK;
                //th.Abort();
                this.Close();
            }
        }

        private void picExit_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                return;
            Process.GetCurrentProcess().Kill();
        }


        private void GuideMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!GlobalVar.UserLogin)
            {
                Process.GetCurrentProcess().Kill();
            }
            else
            {
                this.Close();
            }
        }
    }
}