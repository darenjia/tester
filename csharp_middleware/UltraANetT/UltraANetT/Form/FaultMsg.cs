using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using FileEditor.Control;
using FileEditor.pubClass;
using ProcessEngine;
using System.Text.RegularExpressions;

namespace FileEditor.Form
{
    public partial class FaultMsg : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private DataOper _currentOperate;
        private int UpFalseCount = 0;
        private bool UpResult;

        public FaultMsg()
        {
            InitializeComponent();
            Checked();
            _currentOperate = DataOper.Insert;
            ShieldRight();

        }

        public FaultMsg(int RowCount)
        {
            InitializeComponent();
            _currentOperate = DataOper.Add;
            //Checked();
            ShieldRight();
        }

        public FaultMsg(DataRow Row)
        {
            InitializeComponent();
            //Checked();
            _currentOperate = DataOper.Update;
            
            this.txtChsName.Text = Row["DTCChsName"].ToString();
            this.txtEngName.Text = Row["DTCEngName"].ToString();
            //this.chkResult.Checked = bool.Parse(Row["Result"].ToString());
            this.txtUnit.Text = Row["Unit"].ToString();
            //stringRange.Text = Row["StringRange"].ToString();
            //this.cmbUnit.Text = Row["Unit"].ToString();
            this.chkifHex.Checked = bool.Parse(Row["IfHex"].ToString());
            this.isInt.Checked = bool.Parse(Row["IsInt"].ToString());
            this.checkString.Checked = bool.Parse(Row["IsString"].ToString());
            //ifStringLimit.Checked = bool.Parse(Row["IsStringLimit"].ToString());
            ShieldRight();



        }

        public FaultMsg(int RowIndex, int count)
        {
            InitializeComponent();
            //Checked();
            _currentOperate = DataOper.Insert;
            
            UpFalseCount = count;
        }

        private enum DataOper
        {
            Add = 0,
            Update = 1,
            Insert = 2,
            Del = 3
        }

        private object[] GetObj()
        {
            string ChsName = this.txtChsName.Text;
            string EngName = this.txtEngName.Text;
            string Unit = this.txtUnit.Text;
            //string Unit = this.cmbUnit.Text;
            var ifHex = this.chkifHex.Checked;
            var ifInt = isInt.Checked;
            var ifString = checkString.Checked;
            object[] obj = new object[] { ChsName, EngName,ifHex, ifInt, ifString,Unit };
            return obj;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if(!Validate())
            {
                return;
            }
            if (!isInt.Checked && !chkifHex.Checked && !checkString.Checked)
            {
                XtraMessageBox.Show("请选择DTC相关报文的数据类型？", "提示");
                return;
            }
            object[] obj = GetObj();//new object[] {this.txtIndex.Text,this.txtChsName.Text,this.txtEngName.Text,this.chkResult.Checked};
  
                switch (_currentOperate)
                {
                    
                        //添加
                        case DataOper.Add:
                        if (
                            XtraMessageBox.Show("确认添加一行么？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) ==
                            DialogResult.OK)
                        {
                            FaultTypeCount cfgForm = (FaultTypeCount) this.Owner;
                            cfgForm.AddDataRow(obj);
                            //if (bool.Parse(obj[3].ToString()))
                            //{
                            //    //EditExcel(_currentOperate, obj);
                            //}
                            ////string json = cfgForm.SaveJson();
                            //string vehicel = "故障类型表.txt";
                            //cfgForm.Write(GlobalVar.TemporaryFilePath +  @"FaultType\"+vehicel, json);
                            //GlobalVar.IsColumEdit = true;
                            this.Close();
                        }
                        break;
                        case DataOper.Update:
                        if (XtraMessageBox.Show("确认修改？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) ==
                            DialogResult.OK)
                        {
                            FaultTypeCount cfgForm = (FaultTypeCount) this.Owner;
                            cfgForm.UpdataDataRow(obj);
                            //obj[0] = int.Parse(obj[0].ToString()) - UpFalseCount;
                            ////EditExcel(_currentOperate, obj);
                            //cfgForm.SaveJson();
                            this.Close();
                        }
                        break;
                        case DataOper.Insert:
                        if (XtraMessageBox.Show("确认插入一行？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) ==
                            DialogResult.OK)
                        {
                            FaultTypeCount cfgForm = (FaultTypeCount) this.Owner;
                            cfgForm.InsertDataRow(obj);
                            //obj[0] = int.Parse(obj[0].ToString()) - UpFalseCount;
                            //if (bool.Parse(obj[3].ToString()))
                            //{
                            //    //EditExcel(_currentOperate, obj);
                            //}
                            //string json = cfgForm.SaveJson();
                            //string vehicel = GlobalVar.VNode[0] + "-" + GlobalVar.VNode[1] + "-" + GlobalVar.VNode[2] +
                            //                 "-" + "配置表列编辑.txt";
                            //cfgForm.Write(GlobalVar.TemporaryFilePath + vehicel, json);
                            //GlobalVar.IsColumEdit = true;
                            //cfgForm.Write(GlobalVar.JsonTxtPath, json);
                            this.Close();
                        }
                        break;
                    }
     
        }

       

        private void chkifHex_CheckedChanged(object sender, EventArgs e)
        {
            if (chkifHex.Checked)
            {
                isInt.Checked = false;
                checkString.Checked = false;
                this.txtUnit.ReadOnly = true;
            }
            Checked();
            this.txtUnit.Focus();
        }

        private void Checked()
        {
            if (chkifHex.Checked)
            {
                isInt.Checked = false;
                checkString.Checked = false;
                txtUnit.Enabled = false;
                this.txtUnit.ReadOnly = true;
            }
            else if(isInt.Checked)
            {
                chkifHex.Checked = false;
                checkString.Checked = false;
                isInt.Enabled = true;
                txtUnit.Enabled = true;
                this.txtUnit.ReadOnly = false;
            }
            else if (checkString.Checked)
            {
                chkifHex.Checked = false;
                isInt.Checked = false;
                
                txtUnit.Enabled = false;
                this.txtUnit.ReadOnly = true;
            }
            

        }

        private bool Validate()
        {
            errorProvider1.Clear();
            bool Validate = true;
            string error = null;
            if (txtChsName.Text.Length == 0)
            {
                error = "此项不能为空！";
                Validate = false;
                errorProvider1.SetError(txtChsName, error);
                
                //e.Cancel = true;
            }
            else if (txtEngName.Text.Length == 0)
            {
                error = "此项不能为空！";
                //e.Cancel = true;
                Validate = false;
                errorProvider1.SetError(txtEngName, error);
            }
            else if (!chkifHex.Checked)
            {
                //Regex rg = new Regex(@"^\+?[1-9][0-9]*$");
                //string Index = this.txtUnit.Text;
                //if (!rg.IsMatch(Index))
                //{
                //    error = "请输入合法的数值！";
                //    //e.Cancel = true;
                //    Validate = false;
                //}
                //errorProvider1.SetError(txtUnit, error);
            }
            
            return Validate;
        }

        private void txtChsName_Validating(object sender, CancelEventArgs e)
        {
            string error = null;
            if (txtChsName.Text.Length == 0)
            {
                error = "此项不能为空！";
                //e.Cancel = true;
            }
            errorProvider1.SetError((System.Windows.Forms.Control)sender, error);
        }

        private void txtEngName_Validating(object sender, CancelEventArgs e)
        {
            string error = null;
            if (txtEngName.Text.Length == 0)
            {
                error = "此项不能为空！";
                //e.Cancel = true;
            }
            errorProvider1.SetError((System.Windows.Forms.Control)sender, error);
        }

        private void txtUnit_Validating(object sender, CancelEventArgs e)
        {
            //if (chkifHex.Checked)
            //{
            //    errorProvider1.Clear();
            //    return;
            //}
            //string error = null;
            //Regex rg = new Regex(@"^\+?[1-9][0-9]*$");
            //string Index = this.txtUnit.Text;
            //if (!rg.IsMatch(Index))
            //{
            //    error = "请输入合法的数值！";
            //    //e.Cancel = true;
            //}
            //errorProvider1.SetError((System.Windows.Forms.Control)sender, error);
        }

        private void isInt_CheckedChanged(object sender, EventArgs e)
        {
            if (isInt.Checked)
            {
                chkifHex.Checked = false;
                checkString.Checked = false;
                isInt.Enabled = true;
                txtUnit.Enabled = true;
                this.txtUnit.ReadOnly = false;
                this.txtUnit.Focus();
            }
            
            
        }

        private void checkString_CheckedChanged(object sender, EventArgs e)
        {
            if (checkString.Checked)
            {
                chkifHex.Checked = false;
                isInt.Checked = false;

                txtUnit.Enabled = false;
                this.txtUnit.ReadOnly = true;
            }
        }
        private void ShieldRight()
        {
            ContextMenu emptyMenu = new ContextMenu();
            txtChsName.Properties.ContextMenu = emptyMenu;
            txtEngName.Properties.ContextMenu = emptyMenu;
            txtUnit.Properties.ContextMenu = emptyMenu;
            
        }
    }
}