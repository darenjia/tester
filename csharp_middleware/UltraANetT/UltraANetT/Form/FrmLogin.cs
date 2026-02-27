using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ProcessEngine;
using System.Diagnostics;
using System.Security.Cryptography;

namespace UltraANetT.Form
{
    public partial class FrmLogin : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private Point _mousePoint = new Point();
        private ProcLog Log = new ProcLog();
        private Dictionary<string, object> _dictUserLog = new Dictionary<string, object>();
        private ProcStore _store = new ProcStore();
        public FrmLogin()
        {
            InitializeComponent();
            SetLastLoginUser();
            InitDict();
        }

        private void FrmLogin_Activated(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.txtUserName.Text))
            {
                txtUserName.Focus();
            }
            else
            {
                txtPassword.Focus();
            }
        }

        private void SetLastLoginUser()
        {
            var lastUser = _store.GetRegularByEnum(EnumLibrary.EnumTable.LastLoginUser);
            if (lastUser.Count > 0)
            {
                this.txtUserName.Text = lastUser[0][2].ToString();
            }
        }

        private void FrmLogin_MouseDown(object sender, MouseEventArgs e)
        {
            _mousePoint = new Point(e.X, e.Y);
        }

        private void FrmLogin_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = new Point(this.Location.X + e.X - _mousePoint.X,
                    this.Location.Y + e.Y - _mousePoint.Y);
            }
        }
        private void InitDict()
        {
            _dictUserLog.Add("EmployeeNo", "");
            _dictUserLog.Add("EmployeeName", "");
            _dictUserLog.Add("Department", "");
            _dictUserLog.Add("LoginDate", "");
            _dictUserLog.Add("LoginOffDate", "1900-1-1");
        }
        private void btnOk_Click(object sender, EventArgs e)
        {
            string error = "";
            string pwd = Md5(this.txtPassword.Text);
            Dictionary<string, object> key = new Dictionary<string, object>();
            key.Add("ElyName", this.txtUserName.Text);
            var User = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Employee, key);
            if (User.Count > 0)
            {
                if (User[0][7].ToString() == pwd)
                {
                    _dictUserLog["EmployeeNo"] = User[0][0].ToString();
                    _dictUserLog["EmployeeName"] = User[0][1].ToString();
                    _dictUserLog["Department"] = User[0][3].ToString();
                    _dictUserLog["LoginDate"] = DateTime.Now;
                    GlobalVar.LoginNo = Log.WriteLoginLog(EnumLibrary.EnumTable.LoginLog, _dictUserLog, out error);
                    if (error != "")
                    {
                        XtraMessageBox.Show("登陆出现未知异常！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        GlobalVar.UserLogin = true;
                        GlobalVar.UserName = _dictUserLog["EmployeeName"].ToString();
                        GlobalVar.UserNo = _dictUserLog["EmployeeNo"].ToString();
                        GlobalVar.UserDept = _dictUserLog["Department"].ToString();
                        FileEditor.pubClass.GlobalVar.UserLogin = true;
                        FileEditor.pubClass.GlobalVar.UserName = _dictUserLog["EmployeeName"].ToString();
                        FileEditor.pubClass.GlobalVar.UserNo = _dictUserLog["EmployeeNo"].ToString();
                        FileEditor.pubClass.GlobalVar.UserDept = _dictUserLog["Department"].ToString();
                        this.DialogResult = DialogResult.OK;
                    }
                }
                else
                {
                    XtraMessageBox.Show("密码错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                XtraMessageBox.Show("用户名错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #region MD5加密
        public static string Md5(string encryptString)
        {
            byte[] result = Encoding.Default.GetBytes(encryptString);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            string encryptResult = BitConverter.ToString(output).Replace("-", "");
            return encryptResult;
        }
        #endregion
        private void UserLogin_FormClosed(object sender, FormClosedEventArgs e)
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

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        
    }
}