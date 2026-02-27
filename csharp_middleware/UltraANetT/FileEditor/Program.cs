using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using DevExpress.UserSkins;
using DevExpress.Skins;
using DevExpress.LookAndFeel;
using FileEditor.form;
using System.Threading;
using DevExpress.XtraEditors;

namespace FileEditor
{
    static class Program
    {
        private static AboutDevCompanion DevCompanion;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            int index = Process.GetProcessesByName("UltraANetT").Length;
            if (index == 0)
            {
                MessageBox.Show(@"无法独立运行，请于主程序中打开...");
                return;
            }
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-Hans");
            DevCompanion = new AboutDevCompanion(1, false);
            DevCompanion.Run();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            UserLookAndFeel.Default.SetSkinStyle("Office 2016 Colorful");
            WindowsFormsSettings.DefaultFont = new System.Drawing.Font("微软雅黑", 9);
            SkinManager.EnableFormSkins();
            BonusSkins.Register();
            //UserLookAndFeel.Default.SetSkinStyle("DevExpress Style");
            Application.Run(new Editor(false));
        }
    }
}
