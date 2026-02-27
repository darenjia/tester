using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ProcessEngine;
using System.Diagnostics;
using System.Security.Cryptography;

namespace UltraANetT.Form
{
    public partial class UserLogin : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private ProcLog Log = new ProcLog();
        private Dictionary<string, object> _dictUserLog = new Dictionary<string, object>();
        private ProcStore _store = new ProcStore();
        public UserLogin()
        {
            InitializeComponent();
            InitDict();
        }
        private void InitDict()
        {
            _dictUserLog.Add("EmployeeNo", "");
            _dictUserLog.Add("EmployeeName", "");
            _dictUserLog.Add("Department", "");
            _dictUserLog.Add("LoginDate", "");
            _dictUserLog.Add("LoginOffDate", "1900-1-1");
            ContextMenu emptyMenu_1 = new ContextMenu();
            txtUserName.Properties.ContextMenu = emptyMenu_1;
            ContextMenu emptyMenu_2 = new ContextMenu();
            txtPassword.Properties.ContextMenu = emptyMenu_2;
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
                    Log.WriteLoginLog(EnumLibrary.EnumTable.LoginLog, _dictUserLog, out error);
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