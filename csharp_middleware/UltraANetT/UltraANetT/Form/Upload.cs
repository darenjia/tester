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
    public partial class Upload : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private ProcStore _store = new ProcStore();

        Dictionary<string, object> drKey = new Dictionary<string, object>();

        private bool isEmpty = true;
        public Upload()
        {
            InitializeComponent();
            InitUpload();
            IList<object[]> info = _store.GetRegularByEnum(EnumLibrary.EnumTable.UploadInfo);
            if (info.Count > 0)
            {
                isEmpty = false;
                txtIP.Text = info[0][0].ToString();
                txtPort.Text = info[0][1].ToString();
                txtUser.Text = info[0][2].ToString();
                txtPwd.Text = info[0][3].ToString();
                txtUploadPath.Text = info[0][4].ToString();
            }
            
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            IList<object[]> info = _store.GetRegularByEnum(EnumLibrary.EnumTable.UploadInfo);
            if (info.Count > 0)
            {
                isEmpty = false;
            }
            if (txtIP.Text.TrimEnd() == "" || txtPort.Text.TrimEnd() == "" || txtPwd.Text.TrimEnd() == "" ||
                txtUser.Text.TrimEnd() == "" || txtUploadPath.Text.TrimEnd() == "")
            {
                MessageBox.Show(@"配置信息不能为空..");
                return;
            }
            drKey["IP"] = txtIP.Text;
            drKey["Port"] = txtPort.Text;
            drKey["User"] = txtUser.Text;
            drKey["Password"] = txtPwd.Text;
            drKey["UploadPath"] = txtUploadPath.Text;
            string error = "";
            if (!isEmpty)
            {
                _store.Del(EnumLibrary.EnumTable.UploadInfo, drKey, out error);
            }
            _store.AddUpload(drKey,out error);
        }

        private void InitUpload()
        {
            drKey.Add("IP","");
            drKey.Add("Port", "");
            drKey.Add("User", "");
            drKey.Add("Password", "");
            drKey.Add("UploadPath", "");
        }
    }
}