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
    public partial class ColEdit : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private DataOper _currentOperate;
        private int UpFalseCount = 0;
        private bool UpResult;

        public ColEdit()
        {
            InitializeComponent();
            Checked();
            ShieldRight();
        }

        public ColEdit(int RowCount)
        {
            InitializeComponent();
            ShieldRight();
            this.txtIndex.ReadOnly = true;
            this.txtIndex.Text = RowCount.ToString();
            _currentOperate = DataOper.Add;
            Checked();
        }
        private void ShieldRight()
        {
            ContextMenu emptyMenu_1 = new ContextMenu();
            txtChsName.Properties.ContextMenu = emptyMenu_1;
            ContextMenu emptyMenu_2 = new ContextMenu();
            txtEngName.Properties.ContextMenu = emptyMenu_2;
            ContextMenu emptyMenu_3 = new ContextMenu();
            txtFormat.Properties.ContextMenu = emptyMenu_3;
            ContextMenu emptyMenu_4 = new ContextMenu();
            txtIndex.Properties.ContextMenu = emptyMenu_4;
            ContextMenu emptyMenu_5 = new ContextMenu();
            txtMaxInt.Properties.ContextMenu = emptyMenu_5;
            ContextMenu emptyMenu_6 = new ContextMenu();
            txtMinInt.Properties.ContextMenu = emptyMenu_6;
            ContextMenu emptyMenu_7 = new ContextMenu();
            txtUnit.Properties.ContextMenu = emptyMenu_7;
            
        }
        public ColEdit(DataRow Row, int count)
        {
            InitializeComponent();
            Checked();
            _currentOperate = DataOper.Update;
            this.txtIndex.ReadOnly = true;
            this.txtIndex.Text = Row["Index"].ToString();
            this.txtChsName.Text = Row["ChsName"].ToString();
            this.txtEngName.Text = Row["EngName"].ToString();
            this.chkResult.Checked = bool.Parse(Row["Result"].ToString());
            this.txtUnit.Text = Row["Unit"].ToString();
            stringRange.Text = Row["StringRange"].ToString();
            //this.cmbUnit.Text = Row["Unit"].ToString();
            this.chkifHex.Checked = bool.Parse(Row["ifHex"].ToString());

            if(Row["StringRange"].ToString() == "")
            {
                this.ifStringLimit.Checked = false;
            }
            else
            {
                this.ifStringLimit.Checked = true;
            }
            //ifStringLimit.Checked = bool.Parse(Row["IsStringLimit"].ToString());
            if (Row["MinInt"].ToString() == "" && Row["MaxInt"].ToString() == "")
            {
                this.chkifMinMaxInt.Checked = false;
            }
            else
            {
                this.chkifMinMaxInt.Checked = true;
            }
            this.txtMinInt.Text = Row["MinInt"].ToString();
            this.txtMaxInt.Text = Row["MaxInt"].ToString();
            if (Row["Format"].ToString() == "")
            {
                this.chkifFormat.Checked = false;
            }
            else
            {
                this.chkifFormat.Checked = true;
            }
            this.txtFormat.Text = Row["Format"].ToString();

            UpResult = bool.Parse(Row["Result"].ToString());
            UpFalseCount = count;
        }

        public ColEdit(int RowIndex, int count)
        {
            InitializeComponent();
            Checked();
            _currentOperate = DataOper.Insert;
            this.txtIndex.ReadOnly = true;
            this.txtIndex.Text = (RowIndex + 1).ToString();
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
            string Index = this.txtIndex.Text;
            string ChsName = this.txtChsName.Text;
            string EngName = this.txtEngName.Text;
            var Result = this.chkResult.Checked;
            string Unit = this.txtUnit.Text;
            //string Unit = this.cmbUnit.Text;
            var ifHex = this.chkifHex.Checked;
            var ifString = ifStringLimit.Checked;
            string strRange = stringRange.Text;
            var ifMinMaxInt = this.chkifMinMaxInt.Checked;
            string intMin = "";
            string intMax = "";
            if (this.chkifMinMaxInt.Checked)
            {
                intMin = this.txtMinInt.Text;
                intMax = this.txtMaxInt.Text;
            }
            var ifFormat = this.chkifFormat.Checked;
            string Format = "";
            if (this.chkifFormat.Checked)
            {
                Format = this.txtFormat.Text;
            }

            object[] obj = new object[] { Index, ChsName, EngName, Result, ifHex, Unit,ifString,strRange,  ifMinMaxInt, intMin, intMax, ifFormat, Format };
            return obj;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if(!Validate())
            {
                return;
            }
            object[] obj = GetObj();//new object[] {this.txtIndex.Text,this.txtChsName.Text,this.txtEngName.Text,this.chkResult.Checked};
            if (!GlobalVar.IsIndependent)
            {
                switch (_currentOperate)
                {
                    
                        //添加
                        case DataOper.Add:
                        if (
                            XtraMessageBox.Show("确认添加一行么？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) ==
                            DialogResult.OK)
                        {
                            CfgColEdit cfgForm = (CfgColEdit) this.Owner;
                            cfgForm.AddDataRow(obj);
                            if (bool.Parse(obj[3].ToString()))
                            {
                                //EditExcel(_currentOperate, obj);
                            }
                            string json = cfgForm.SaveJson();
                            string vehicel = GlobalVar.VNode[0] + "-" + GlobalVar.VNode[1] + "-" + GlobalVar.VNode[2] +
                                             "-" + "配置表列编辑.txt";
                            cfgForm.Write(GlobalVar.TemporaryFilePath +  @"config\"+vehicel, json);
                            //GlobalVar.IsColumEdit = true;
                            this.Close();
                        }
                        break;
                        case DataOper.Update:
                        if (XtraMessageBox.Show("确认修改？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) ==
                            DialogResult.OK)
                        {
                            CfgColEdit cfgForm = (CfgColEdit) this.Owner;
                            cfgForm.UpdataDataRow(obj);

                            #region 判断操作

                            if (UpResult) //如果修改之前的值为True
                            {
                                if (!(bool.Parse(obj[3].ToString()))) //修改后的值为False
                                {
                                    _currentOperate = DataOper.Del; //操作变为删除
                                }
                            }
                            else //如果修改之前的值为False
                            {
                                if (bool.Parse(obj[3].ToString())) //修改后的值为True
                                {
                                    _currentOperate = DataOper.Insert; //操作变为插入
                                }
                                else //如果修改后的值依然为False
                                {
                                    string json = cfgForm.SaveJson();
                                    string vehicel = GlobalVar.VNode[0] + "-" + GlobalVar.VNode[1] + "-" +
                                                     GlobalVar.VNode[2] + "-" + "配置表列编辑.txt";
                                    cfgForm.Write(GlobalVar.TemporaryFilePath + vehicel, json);
                                    //GlobalVar.IsColumEdit = true;
                                    //cfgForm.Write(GlobalVar.JsonTxtPath, json);
                                    this.Close();
                                    break;
                                }
                            }

                            #endregion

                            obj[0] = int.Parse(obj[0].ToString()) - UpFalseCount;
                            //EditExcel(_currentOperate, obj);
                            cfgForm.SaveJson();
                            this.Close();
                        }
                        break;
                        case DataOper.Insert:
                        if (XtraMessageBox.Show("确认插入一行？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) ==
                            DialogResult.OK)
                        {
                            CfgColEdit cfgForm = (CfgColEdit) this.Owner;
                            cfgForm.InsertDataRow(obj);
                            obj[0] = int.Parse(obj[0].ToString()) - UpFalseCount;
                            if (bool.Parse(obj[3].ToString()))
                            {
                                //EditExcel(_currentOperate, obj);
                            }
                            string json = cfgForm.SaveJson();
                            string vehicel = GlobalVar.VNode[0] + "-" + GlobalVar.VNode[1] + "-" + GlobalVar.VNode[2] +
                                             "-" + "配置表列编辑.txt";
                            cfgForm.Write(GlobalVar.TemporaryFilePath + vehicel, json);
                            //GlobalVar.IsColumEdit = true;
                            //cfgForm.Write(GlobalVar.JsonTxtPath, json);
                            this.Close();
                        }
                        break;
                    }
                    
            }
            else
            {
                switch (_currentOperate)
                {
                    case DataOper.Add:
                        if (
                            XtraMessageBox.Show("确认添加一行么？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) ==
                            DialogResult.OK)
                        {
                            CfgColEdit cfgForm = (CfgColEdit)this.Owner;
                            cfgForm.AddDataRow(obj);
                            if (bool.Parse(obj[3].ToString()))
                            {
                                //EditExcel(_currentOperate, obj);
                            }
                            string json = cfgForm.SaveJson();
                            string vehicel = GlobalVar.SelectName +"-列编辑.txt";
                            cfgForm.Write(GlobalVar.TemporaryFilePath + vehicel, json);
                            GlobalVar.IsColumEdit = true;
                            this.Close();
                        }
                        break;
                     case DataOper.Update:
                          if (XtraMessageBox.Show("确认修改？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) ==
                            DialogResult.OK)
                          {
                              CfgColEdit cfgForm = (CfgColEdit)this.Owner;
                              cfgForm.UpdataDataRow(obj);

                              #region 判断操作

                              if (UpResult) //如果修改之前的值为True
                              {
                                  if (!(bool.Parse(obj[3].ToString()))) //修改后的值为False
                                  {
                                      _currentOperate = DataOper.Del; //操作变为删除
                                  }
                              }
                              else //如果修改之前的值为False
                              {
                                  if (bool.Parse(obj[3].ToString())) //修改后的值为True
                                  {
                                      _currentOperate = DataOper.Insert; //操作变为插入
                                  }
                                  else //如果修改后的值依然为False
                                  {
                                      string jsonm = cfgForm.SaveJson();
                                      string vehicelm = GlobalVar.SelectName+"-列编辑.txt";
                                      cfgForm.Write(GlobalVar.TemporaryFilePath + vehicelm, jsonm);
                                      GlobalVar.IsColumEdit = true;
                                      //cfgForm.Write(GlobalVar.JsonTxtPath, json);
                                      this.Close();
                                      break;
                                  }
                              }

                              #endregion

                              obj[0] = int.Parse(obj[0].ToString()) - UpFalseCount;
                                      //EditExcel(_currentOperate, obj);
                              string json = cfgForm.SaveJson();
                              string vehicel = GlobalVar.SelectName + "-列编辑.txt";
                              cfgForm.Write(GlobalVar.TemporaryFilePath + vehicel, json);
                              //cfgForm.SaveJson();
                              this.Close();
                          }
                          break;
                     case DataOper.Insert:
                          if (XtraMessageBox.Show("确认插入一行？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) ==
                            DialogResult.OK)
                          {
                              CfgColEdit cfgForm = (CfgColEdit)this.Owner;
                              cfgForm.InsertDataRow(obj);
                              obj[0] = int.Parse(obj[0].ToString()) - UpFalseCount;
                              if (bool.Parse(obj[3].ToString()))
                              {
                                  //EditExcel(_currentOperate, obj);
                              }
                                string json = cfgForm.SaveJson();
                                string vehicel = GlobalVar.SelectName + "-列编辑.txt";
                                cfgForm.Write(GlobalVar.TemporaryFilePath + vehicel, json);
                                GlobalVar.IsColumEdit = true;
                                //cfgForm.Write(GlobalVar.JsonTxtPath, json);
                                this.Close();
                          }
                          break;
            }
        }
        }


        private void chkifFormat_CheckedChanged(object sender, EventArgs e)
        {
            Checked();
        }

        private void chkifMinMaxInt_CheckedChanged(object sender, EventArgs e)
        {
            Checked();
        }

        private void chkifHex_CheckedChanged(object sender, EventArgs e)
        {
            Checked();
            this.txtUnit.Focus();
        }

        private void Checked()
        {
            if (chkifFormat.Checked)
            {
                this.txtFormat.ReadOnly = false;
            }
            else
            {
                this.txtFormat.ReadOnly = true;
                this.txtFormat.Text = "";
            }
            if (chkifMinMaxInt.Checked)
            {
                this.txtMinInt.ReadOnly = false;
                this.txtMaxInt.ReadOnly = false;
            }
            else
            {
                this.txtMinInt.ReadOnly = true;
                this.txtMaxInt.ReadOnly = true;
                this.txtMinInt.Text = "";
                this.txtMaxInt.Text = "";
            }
            if (!chkifHex.Checked)
            {
                this.txtUnit.ReadOnly = false;
                stringRange.ReadOnly = false;
                //this.cmbUnit.ReadOnly = false;
                chkifFormat.Enabled = true;
                chkifMinMaxInt.Enabled = true;
                ifStringLimit.Enabled = true;
            }
            else
            {
                this.txtUnit.ReadOnly = true;
                //this.cmbUnit.ReadOnly = true;
                this.txtMinInt.ReadOnly = true;
                this.txtMaxInt.ReadOnly = true;
                this.txtFormat.ReadOnly = true;
                stringRange.ReadOnly = true;
                chkifFormat.Enabled = false;
                chkifMinMaxInt.Enabled = false;
                ifStringLimit.Enabled = false;
                chkifFormat.Checked = false;
                chkifMinMaxInt.Checked = false;
                ifStringLimit.Checked = false;
                this.txtUnit.Text = "";
                //this.cmbUnit.Text = "";
                this.txtMinInt.Text = "";
                this.txtMaxInt.Text = "";
                this.txtFormat.Text = "";
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
    }
}