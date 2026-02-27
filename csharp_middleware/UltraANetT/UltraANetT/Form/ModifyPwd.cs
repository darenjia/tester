using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using ProcessEngine;

namespace UltraANetT.Form
{
    public partial class ModifyPwd : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private ProcStore _store = new ProcStore();
        private string _name;
        string _pic;
        Dictionary<string, object> drKey = new Dictionary<string, object>();
        private string picPath;
        private string picName;
        public ModifyPwd()
        {
            InitializeComponent();
            _name = GlobalVar.UserName;
            drKey.Add("ElyName", _name);
            //PhotoFromDataBase();
        }
        #region 提交新密码按钮点击事件
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            Dictionary<string, object> key = new Dictionary<string, object>();
            key.Add("ElyName", _name);
            string oneOldPwd = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Employee, key)[0][7].ToString();
            string oldPwd = Md5(txtoldPW.Text);
            IsOldPassWord(oldPwd, oneOldPwd);

        }
        #endregion

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

        #region 判断输入的原密码是否正确和两次输入新密码是否一样
        private void IsOldPassWord(string oldPwd, string oldPwdFromDb)
        {

            if (oldPwd != oldPwdFromDb)
            {
                XtraMessageBox.Show("请输入正确的原密码");
                return;
            }

            if (txtNewPWFirst.Text != txtNewPWSecond.Text)
            {
                XtraMessageBox.Show("两次输入密码不同，请重新输入！");
                return;
            }
            string error;
            string newPwd = Md5(txtNewPWFirst.Text);
            drKey.Add("Password", newPwd);
            if (_store.Update(EnumLibrary.EnumTable.EmployeePwd, drKey, out error))
                XtraMessageBox.Show("密码修改成功！");

        }

        #endregion

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}