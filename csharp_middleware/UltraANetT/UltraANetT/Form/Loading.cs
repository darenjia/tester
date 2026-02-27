using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraSplashScreen;

namespace UltraANetT.Form
{
    public partial class Loading : SplashScreen
    {
        private int _dotCount = 0;
        public Loading()
        {
            InitializeComponent();
            //lblCopyright.Text = $"{lblCopyright.Text}{GetYearString()}";
            lblCopyright.Text = "Copyright © 中汽研（天津）汽车工程研究院有限公司";
            //pictureEdit2.Image = global::DevExpress.MailClient.Win.Properties.Resources.SplashScreen;
            var tmr = new Timer {Interval = 1000};
            tmr.Tick += tmr_Tick;
            tmr.Start();
        }

        #region Overrides

        public override void ProcessCommand(Enum cmd, object arg)
        {
            base.ProcessCommand(cmd, arg);
        }

        #endregion

        public enum SplashScreenCommand
        {
        }

        void tmr_Tick(object sender, EventArgs e)
        {
            if (++_dotCount > 3) _dotCount = 0;
            lblStart.Text = string.Format("{1}{0}", GetDots(_dotCount), "Starting");
        }

        string GetDots(int count)
        {
            string ret = string.Empty;
            for (int i = 0; i < count; i++) ret += ".";
            return ret;
        }
        int GetYearString()
        {
            int ret = DateTime.Now.Year;
            return (ret < 2012 ? 2012 : ret);
        }
    }
}