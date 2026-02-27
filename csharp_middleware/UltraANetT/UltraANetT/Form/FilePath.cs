using System;

using System.Windows.Forms;
using DevExpress.XtraEditors;
using ProcessEngine;

namespace UltraANetT.Form
{
    public partial class FilePath : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        ProcFile _file = new ProcFile();

        
        public FilePath()
        {
            InitializeComponent();
        }

        private void btnSelfCheck_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void btnSelfCheck_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog() ;
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnSelfCheck.Text = _OFD.FileName;

            }
        }

        private void btnCfg_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnCfg.Text = _OFD.FileName; ;

            }
        }

        private void FilePath_Load(object sender, EventArgs e)
        {
            string firstStr = _file.ReadLocalXml(@"xml\path.xml", @"Product/IsFirst");
            bool isFirst = bool.Parse(firstStr);
            if (isFirst)
            {
                if(_file.ReadLocalXml(@"xml\path.xml", @"Product/SelfPath") == "无")
                    btnSelfCheck.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/SelfPath");
                else
                    btnSelfCheck.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/SelfPath");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/CfgPath") == "无")
                    btnCfg.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CfgPath");
                else
                    btnCfg.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/CfgPath");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigExam") == "无")
                    btnCANSigExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigExam");
                else
                    btnCANSigExam.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigExam");


                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigReport") == "无")
                    btnCANSigReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigReport");
                else
                    btnCANSigReport.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgExam") == "无")
                    btnCANLtgExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgExam");
                else
                    btnCANLtgExam.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgReport") == "无")
                    btnCANLtgReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgReport");
                else
                    btnCANLtgReport.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigExam") == "无")
                    btnLINSigExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigExam");
                else
                    btnLINSigExam.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigReport") == "无")
                    btnLinSigReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigReport");
                else
                    btnLinSigReport.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgExam") == "无")
                    btnLINLtgExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgExam");
                else
                    btnLINLtgExam.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgReport") == "无")
                    btnLINLtgReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgReport");
                else
                    btnLINLtgReport.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/WIifiReport") == "无")
                    btnWIifiReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/WIifiReport");
                else
                    btnWIifiReport.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/WIifiReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/WifiExam") == "无")
                    btnWifiExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/WifiExam");
                else
                    btnWifiExam.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/WifiExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/lgSigReport") == "无")
                    btn1939SigReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/lgSigReport");
                else
                    btn1939SigReport.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/lgSigReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/lgSigExam") == "无")
                    btn1939SigExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/lgSigExam");
                else
                    btn1939SigExam.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/lgSigExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigReport") == "无")
                    btnOSEKSigReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigReport");
                else
                    btnOSEKSigReport.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigExam") == "无")
                    btnOSEKSigExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigExam");
                else
                    btnOSEKSigExam.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/lgLtnExam") == "无")
                    btn1939LtnExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/lgLtnExam");
                else
                    btn1939LtnExam.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/lgLtnExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/lgLtnReport") == "无")
                    btn1939LtnReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/lgLtnReport");
                else
                    btn1939LtnReport.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/lgLtnReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnReport") == "无")
                    btnOSEKLtnReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnReport");
                else
                    btnOSEKLtnReport.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnExam") == "无")
                    btnOSEKLtnExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnExam");
                else
                    btnOSEKLtnExam.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/DTCExam") == "无")
                    btnDTCExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DTCExam");
                else
                    btnDTCExam.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/DTCExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/DTCreport") == "无")
                    btnDTCreport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DTCreport");
                else
                    btnDTCreport.Text = AppDomain.CurrentDomain.BaseDirectory + _file.ReadLocalXml(@"xml\path.xml", @"Product/DTCreport");


            }
            else
            {
                btnSelfCheck.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/SelfPath");
                btnCfg.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/CfgPath");

                btnCANSigExam.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigExam");
                btnCANSigReport.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigReport");

                btnCANLtgExam.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgExam");
                btnCANLtgReport.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgReport");

                btnLINSigExam.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigExam");
                btnLinSigReport.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigReport");

                btnLINLtgExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgExam");
                btnLINLtgReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgReport");

                btnWIifiReport.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/WIifiReport");
                btnWifiExam.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/WifiExam");

                btn1939SigReport.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/lgSigReport");
                btn1939SigExam.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/lgSigExam");

                btnOSEKSigReport.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigReport");
                btnOSEKSigExam.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigExam");

                btn1939LtnExam.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/lgLtnExam");
                btn1939LtnReport.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/lgLtnReport");

                btnOSEKLtnReport.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnReport");
                btnOSEKLtnExam.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnExam");

                btnDTCExam.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/DTCExam");
                btnDTCreport.Text =   _file.ReadLocalXml(@"xml\path.xml", @"Product/DTCreport");

            }

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/CfgPath", btnCfg.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/SelfPath", btnSelfCheck.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/IsFirst", "false");

            _file.UpdateLocalXml(@"xml\path.xml", @"Product/CANSigExam", btnCANSigExam.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/CANSigReport", btnCANSigReport.Text);

            _file.UpdateLocalXml(@"xml\path.xml", @"Product/CANLtgExam", btnCANLtgExam.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/CANLtgReport", btnCANLtgReport.Text);

            _file.UpdateLocalXml(@"xml\path.xml", @"Product/LINSigExam", btnLINSigExam.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/LINSigReport", btnLinSigReport.Text);

            _file.UpdateLocalXml(@"xml\path.xml", @"Product/LINLtgExam", btnLINLtgExam.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/LINLtgReport", btnLINLtgReport.Text);

            _file.UpdateLocalXml(@"xml\path.xml", @"Product/WIifiReport", btnWIifiReport.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/WifiExam", btnWifiExam.Text);

            _file.UpdateLocalXml(@"xml\path.xml", @"Product/lgSigReport", btn1939SigReport.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/lgSigExam", btn1939SigExam.Text);

            _file.UpdateLocalXml(@"xml\path.xml", @"Product/OSEKSigReport", btnOSEKSigReport.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/OSEKSigExam", btnOSEKSigExam.Text);

            _file.UpdateLocalXml(@"xml\path.xml", @"Product/lgLtnExam", btn1939LtnExam.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/lgLtnReport", btn1939LtnReport.Text);

            _file.UpdateLocalXml(@"xml\path.xml", @"Product/OSEKLtnReport", btnOSEKLtnReport.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/OSEKLtnExam", btnOSEKLtnExam.Text);

            _file.UpdateLocalXml(@"xml\path.xml", @"Product/DTCExam", btnDTCExam.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/DTCreport", btnDTCreport.Text);


             EnumLibrary.SelfPath =  btnSelfCheck.Text;
            EnumLibrary.CfgPath  = btnCfg.Text;
            EnumLibrary.CANSigExam = btnCANSigExam.Text;
            EnumLibrary.CANSigReport = btnCANSigReport.Text;

            EnumLibrary.CANLtgExam = btnCANLtgExam.Text;
            EnumLibrary.CANLtgReport = btnCANLtgReport.Text;
            EnumLibrary.LINSigExam = btnLINSigExam.Text;
            EnumLibrary.LINSigReport = btnLinSigReport.Text;

            EnumLibrary.LINLtgExam = btnLINLtgExam.Text;
            EnumLibrary.LINLtgReport = btnLINLtgReport.Text;

            EnumLibrary.WifiExam = btnWifiExam.Text;
            EnumLibrary.WifiReport = btnWIifiReport.Text;
            //EnumLibrary.SigReport_1939 = btn1939SigReport.Text;
            //EnumLibrary.SigExam_1939 = btn1939SigExam.Text;
            EnumLibrary.OSEKSigReport = btnOSEKSigReport.Text;
            EnumLibrary.OSEKSigExam = btnOSEKSigExam.Text;
            //EnumLibrary.LtnExam_1939 = btn1939LtnExam.Text;
            //EnumLibrary.LtnReport_1939 = btn1939LtnReport.Text;

            EnumLibrary.OSEKLtnReport = btnOSEKLtnReport.Text;
            EnumLibrary.OSEKLtnExam = btnOSEKLtnExam.Text;
            EnumLibrary.DTCExam = btnDTCExam.Text;
            EnumLibrary.DTCreport = btnDTCreport.Text;

            GlobalVar.NumberChanges = 0;
            MessageBox.Show(@"保存成功..");
        }

        private void FilePath_FormClosing(object sender, FormClosingEventArgs e)
        {
            var KF = false;
            if (GlobalVar.NumberChanges != 0)
            {
                if (XtraMessageBox.Show("检测到当前数据未保存！是否关闭页面?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    KF = true;
                    GlobalVar.NumberChanges = 0;
                }
            }
            else { KF = true; }
            if (KF)
            {
                bool isPath = Convert.ToBoolean(_file.ReadLocalXml(@"xml\path.xml", @"Product/IsFirst"));
                if (isPath)
                {
                    XtraMessageBox.Show("请先确认文件路径再关闭并点击保存后再关闭软件...");
                    e.Cancel = true;
                }
            }
            else {
                e.Cancel = true;
            }   
        }

        private void btnCANSigReport_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnCANSigReport.Text = _OFD.FileName;

            }

        }

        private void btnCANSigExam_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnCANSigExam.Text = _OFD.FileName;

            }
        }

        private void btnCANLtgReport_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnCANLtgReport.Text = _OFD.FileName;

            }
        }

        private void btnCANLtgExam_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnCANLtgExam.Text = _OFD.FileName;

            }
        }

        private void btnLinSigReport_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnLinSigReport.Text = _OFD.FileName;

            }
        }

        private void btnLINSigExam_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnLINSigExam.Text = _OFD.FileName;

            }
        }

        private void btnLINLtgReport_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnLINLtgReport.Text = _OFD.FileName;

            }
        }

        private void btnLINLtgExam_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnLINLtgExam.Text = _OFD.FileName;

            }
        }


        private void btn1939SigReport_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btn1939SigReport.Text = _OFD.FileName;

            }
        }

        private void btn1939SigExam_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btn1939SigExam.Text = _OFD.FileName;

            }
        }

        private void btn1939LtnReport_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btn1939LtnReport.Text = _OFD.FileName;

            }
        }

        private void btn1939LtnExam_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btn1939LtnExam.Text = _OFD.FileName;

            }
        }

        private void btnOSEKSigReport_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnOSEKSigReport.Text = _OFD.FileName;

            }
        }

        private void btnOSEKSigExam_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnOSEKSigExam.Text = _OFD.FileName;

            }
        }

        private void btnOSEKLtnReport_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnOSEKLtnReport.Text = _OFD.FileName;

            }
        }

        private void btnOSEKLtnExam_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnOSEKLtnExam.Text = _OFD.FileName;

            }
        }

        private void btnDTCreport_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnDTCreport.Text = _OFD.FileName;

            }
        }

        private void btnDTCExam_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnDTCExam.Text = _OFD.FileName;

            }
        }

        private void btnWIifiReport_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnWIifiReport.Text = _OFD.FileName;

            }
        }

        private void btnWifiExam_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnWifiExam.Text = _OFD.FileName;

            }
        }

        private void btnSelfCheck_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnCfg_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnCANSigReport_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnCANSigExam_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnCANLtgReport_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnCANLtgExam_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnLinSigReport_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnLINSigExam_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void buttonEdit6_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void buttonEdit5_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnLINLtgReport_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnLINLtgExam_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void buttonEdit14_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void buttonEdit13_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void buttonEdit36_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void buttonEdit35_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void buttonEdit16_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void buttonEdit15_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void buttonEdit24_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void buttonEdit23_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void buttonEdit38_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void buttonEdit37_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void buttonEdit25_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void buttonEdit26_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btn1939SigReport_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btn1939SigExam_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btn1939LtnReport_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btn1939LtnExam_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnOSEKSigReport_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnOSEKSigExam_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnOSEKLtnReport_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnOSEKLtnExam_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnDTCreport_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnDTCExam_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void buttonEdit33_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void buttonEdit34_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnWIifiReport_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnWifiExam_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }
    }
}