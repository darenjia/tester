using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using FileEditor;
using FileEditor.Control;
using UltraANetT.Form;
using FileEditor.Form;
using ProcessEngine;
using ReportEditor;
using System.Collections.Generic;

namespace UltraANetT
{
    internal static class Program
    {
        private static AboutDevCompanion DevCompanion;

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            string proName = Process.GetCurrentProcess().ProcessName;
            if (Process.GetProcessesByName(proName).Length > 1)
            {
                MessageBox.Show(@"程序已经运行了一个实例，该程序只允许有一个实例");
                return;
            }

            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-Hans");
            DevCompanion = new AboutDevCompanion(1, false);
            DevCompanion.Run();

            #region 创建文件夹

            List<string> listDir = new List<string>
            {
                @"configini",
                @"dbc",
                @"ErrorInfo",
                @"ExcelReport",
                @"ExcelTemplate",
                @"log",
                @"template\config",
                @"template\example",
                @"temporary\config\GatewayRouting",
                @"temporary\example",
                @"temporary\taskEml",
                @"topology",
                @"xml"
            };
            foreach (var strDir in listDir)
            {
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + strDir)) //若文件夹不存在则新建文件夹   
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + strDir); //新建文件夹   
                }
            }

            #endregion
            #region 判断文件是否还存在

            List<string> ListDllName = new List<string>
            {
                "SQLite.Designer.dll",
                "SQLite.Interop.dll",
                "System.Data.SQLite.dll"
            };
            foreach (var name in ListDllName)
            {
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + name))
                {
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"dll\" + name))
                    {
                        File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"dll\" + name,
                            AppDomain.CurrentDomain.BaseDirectory + name);
                    }
                    else
                    {
                        MessageBox.Show("检测到必要文件" + name + "丢失，程序无法启动。", "严重", MessageBoxButtons.YesNo,
                            MessageBoxIcon.Error);
                        System.Environment.Exit(0);
                    }
                }
            }
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"ANetT.db"))
            {
                if (MessageBox.Show(@"检测到数据库文件丢失，程序将返回初始状态，是否现在重置？（第一次打开请直接点是）", "严重", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error) == DialogResult.Yes)
                {
                    File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"ANetT.db",
                        Properties.Resources.ANetT);
                    if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"xml")) //若文件夹不存在则新建文件夹   
                    {
                        Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"xml"); //新建文件夹   
                    }
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"xml\Tree.xml",
                        Properties.Resources.Tree);
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"xml\path.xml",
                        Properties.Resources.path); 
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"xml\testInfo.xml",
                        Properties.Resources.testInfo);
                }
                else
                {
                    System.Environment.Exit(0);
                }
            }
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"zh-Hans")) //若文件夹不存在则新建文件夹   
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"zh-Hans"); //新建文件夹
                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"dll\zh-Hans"))
                {
                    DirectoryInfo folder = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"dll\zh-Hans");
                    foreach (FileInfo file in folder.GetFiles("*.dll"))
                    {
                        File.Copy(file.FullName, AppDomain.CurrentDomain.BaseDirectory + @"zh-Hans\" + file.Name);
                    }
                }
            }
            #endregion

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            UserLookAndFeel.Default.SetSkinStyle("Office 2016 Colorful");
            WindowsFormsSettings.DefaultFont = new System.Drawing.Font("微软雅黑", 9);
            SkinManager.EnableFormSkins();
            BonusSkins.Register();
            SplashScreenManager.ShowForm(null, typeof(Loading), true, true, false, 500);
            Thread.Sleep(5000);
            FrmLogin ul = new FrmLogin();
            SplashScreenManager.CloseForm();
            ul.ShowDialog();
            try
            {
                if (ul.DialogResult == DialogResult.OK)
                {
                    Application.Run(new Main());
                }
                else
                {
                    Process.GetCurrentProcess().Kill();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}