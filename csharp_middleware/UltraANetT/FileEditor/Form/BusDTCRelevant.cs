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
using DevExpress.XtraGrid.Columns;
using FileEditor.Control;
using FileEditor.pubClass;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraLayout;
using DevExpress.LookAndFeel;

namespace FileEditor.Form
{
    public partial class BusDTCRelevant : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        DataTable _dt = new DataTable();
        Dictionary<string, string> _dictItem = new Dictionary<string, string>();
        Dictionary<string, string> dictItem = new Dictionary<string, string>();
        string DictKey = "";
        private int selectRow;
        private DataRow _dr;
        private DataOper _currentOperate;
        private string strRequestID;
        private string strRespondID;
        private string _DUTname;
        public BusDTCRelevant()
        {
            InitializeComponent();
            ShieldRight();
        }
        public BusDTCRelevant(string itemJson,string DUTName)
        {
            InitializeComponent();
            IniteDt();
            ShieldRight();
            GlobalVar.strEvent = "";
            _DUTname = DUTName;
            if (itemJson == "")
            {
                //MessageBox.Show("该用例还没有录入评价项目");
                return;
            }


            List<Dictionary<string, string>> itemList = Json.DeserJsonDictStrList(itemJson);
            bool empty = false;
            //_DUTname = DUTName;
            foreach (var item in itemList)
            {
                int i = item.Count + 2;
                object[] obj = new object[i];
                int j = 0;
                foreach (KeyValuePair<string, string> dict in item)
                {
                    obj[j] = dict.Value;
                    j++;
                }
                obj[j] = "请右击查看DTC故障信息";
                j++;
                obj[j] = "请右击查看DTC报文信息"; 
                _dt.Rows.Add(obj);
            }
            gcDTC.DataSource = _dt;
            hideContainerRight.Visible = false;
        }

        private void ShieldRight()
        {
            ContextMenu emptyMenu_1 = new ContextMenu();
            txtCddName.Properties.ContextMenu = emptyMenu_1;
            ContextMenu emptyMenu_2 = new ContextMenu();
            txtDUTname.Properties.ContextMenu = emptyMenu_2;
            ContextMenu emptyMenu_3 = new ContextMenu();
            txtDiagTime.Properties.ContextMenu = emptyMenu_3;
            ContextMenu emptyMenu_4 = new ContextMenu();
            txtRequestID.Properties.ContextMenu = emptyMenu_4;
            ContextMenu emptyMenu_5 = new ContextMenu();
            txtRespondID.Properties.ContextMenu = emptyMenu_5;
            
        }
        public void IniteDt()
        {
            _dt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvDTC.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList)
                _dt.Columns.Add(new DataColumn(colName, typeof(object)));
            //gcItem.DataSource = _dt;


        }
        private void InitDict()
        {
            var coList = new List<string>();
            foreach (GridColumn col in gvDTC.Columns)
                coList.Add(col.FieldName);
            foreach(string col in coList)
            {
                _dictItem.Add(col, "");
            }
            //_dictItem.Add("AssessItem", "");
            //_dictItem.Add("MinValue", "");
            //_dictItem.Add("NormalValue", "");
            //_dictItem.Add("MaxValue", "");
        }

        private string DerDtToJson()
        {
            string dtJson = "";
            var coList = new List<string>();
            foreach (GridColumn col in gvDTC.Columns)
                coList.Add(col.FieldName);
            List<Dictionary<string,string>> dtList = new List<Dictionary<string, string>>();
            //int i = 0;
            foreach(DataRow row in _dt.Rows)
            {
                _dictItem = new Dictionary<string, string>();
                int colNum = 0;
                foreach (string col in coList)
                {
                    if (colNum < gvDTC.Columns.Count - 2) 
                    {
                        _dictItem[col] = row[col].ToString();
                        colNum++;
                    }
                }
                dtList.Add(_dictItem);
            }
            dtJson = Json.SerJson(dtList);
            return dtJson;
        }

        private void BusDTCRelevant_FormClosed(object sender, FormClosedEventArgs e)
        {
            //EmlTemplate emlTem = (EmlTemplate)this.Owner;
            if (GlobalVar.strEvent != "")
            {
                if (XtraMessageBox.Show("是否需要保存该用例的评价项目相关信息？", "提示", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning) ==
                        DialogResult.Cancel)
                {
                    GlobalVar.strEvent = "";
                    this.DialogResult = DialogResult.Cancel;
                }
                else
                    this.DialogResult = DialogResult.OK;

                //return;
            }
            ////string assessItem = DerDtToJson();
            ////bool same = false;
            ////foreach(KeyValuePair<string,string> item in dictItem)
            ////{
            ////    if (item.Key == DictKey)
            ////    {
            ////GlobalVar.strEvent = assessItem;
            
        
        //}
        //if (!same)
        //{
        //    GlobalVar.strEvent.Add(DictKey, assessItem);
        //}
        //emlTem.UpdateAssessItemRow(assessItem);
    }

        /// <summary>
        /// 限制datatable里不能添加重复项
        /// </summary>
        /// <returns>false为有重复项</returns>
        private bool PrimaryKey()
        {
            string RequestHex = "";
            ifHex(txtRequestID.Text, out RequestHex);
            string RespondIDHex = "";
            ifHex(txtRespondID.Text, out RespondIDHex);

            bool primary = false; //false默认为是有重复项
            int i = 0;
            for (int j = 0; j < _dt.Rows.Count; j++)
            {
                if (_currentOperate == DataOper.Modify)
                {
                    if (selectRow == j)
                        continue;
                }
                primary = false;
                if (_dt.Rows[j]["Cddname"].ToString() == txtCddName.Text&& _dt.Rows[j]["RequestID"].ToString() == RequestHex && _dt.Rows[j]["RespondID"].ToString() == RespondIDHex)
                {
                    i++;
                }
                else
                {
                    primary = true;
                }
            }
            if (i > 0)
            {
                if (_currentOperate == DataOper.Modify)
                {
                    XtraMessageBox.Show("不能更改为已添加过的数据");
                }
                if (_currentOperate == DataOper.Add)
                {
                    XtraMessageBox.Show("请不要添加重复项");
                }
                primary = false;
            }
            else
            {
                if (_currentOperate == DataOper.Modify)
                {
                    if (_dt.Rows.Count == 1)
                    {
                        primary = true;
                    }
                }
                if (_currentOperate == DataOper.Add)
                {
                    if (_dt.Rows.Count == 0)
                    {
                        primary = true;
                    }
                }
            }
            return primary;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!PrimaryKey())
                return;

            switch (_currentOperate)
            {
                //添加
                case DataOper.Add:
                    object[] row = new object[gvDTC.Columns.Count];
                    if (!IsSatisfyCondition())
                    {
                        return;
                    }
                    row = GetDataFromUI();
                    //IniteDt();
                    _dt.Rows.Add(row);
                    gcDTC.DataSource = _dt;
                    GlobalVar.strEvent = DerDtToJson();
                    Show(DLAF.LookAndFeel, this, "添加成功...", "", new[] { DialogResult.OK }, null, 0,
                        MessageBoxIcon.Information);
                    //ClearDataUI();
                    dpAssess.Visibility = DockVisibility.Visible;
                    txtDUTname.ReadOnly = false;
                    break;
                //更新
                case DataOper.Modify:
                    if (!IsSatisfyCondition())
                    {
                        return;
                    }
                    UpDataRow();
                    GlobalVar.strEvent = DerDtToJson();
                    Show(DLAF.LookAndFeel, this, "修改成功...", "", new[] { DialogResult.OK }, null, 0,
                        MessageBoxIcon.Information);
                    //ClearDataUI();
                    dpAssess.Visibility = DockVisibility.Hidden;
                    txtDUTname.ReadOnly = false;
                    break;

            }
        }

        public static DialogResult Show(UserLookAndFeel look, IWin32Window owner, string text, string caption, DialogResult[] buttons, Icon icon, int defaultButton, MessageBoxIcon messageBeepSound)
        {

            XtraMessageBoxForm form = new XtraMessageBoxForm();
            Font defaultFont = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            form.Appearance.Font = defaultFont;
            return form.ShowMessageBoxDialog(new XtraMessageBoxArgs(look, owner, text, caption, buttons, icon, defaultButton));
        }


        private void tsmiDel_Click(object sender, EventArgs e)
        {
            if(_dr==null)
                return;
            _dr.Delete();
            GlobalVar.strEvent = DerDtToJson();

        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Add;
            dpAssess.Visibility = DockVisibility.Visible;
            txtDUTname.Text = _DUTname;
            txtDUTname.ReadOnly = true;
        }

        private void tsmiModify_Click(object sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            if (_dr == null)
                return;
            _currentOperate = DataOper.Modify;
            dpAssess.Visibility = DockVisibility.Visible;
            SetDataToUI();
            txtDUTname.ReadOnly = true;
        }

        private void gvItem_MouseDown(object sender, MouseEventArgs e)
        {
            var hi = gvDTC.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //取一行值
            gvDTC.SelectRow(hi.RowHandle);
            selectRow = hi.RowHandle;
            _dr = this.gvDTC.GetDataRow(selectRow);
        }
        private void SetDataToUI()
        {
            if(_dr==null)
                return;
            txtDUTname.Text = _dr["DUTname"].ToString();
            txtDUTname.Enabled = false;
            txtCddName.Text = _dr["Cddname"].ToString();
            txtRequestID.Text = _dr["RequestID"].ToString();
            txtRespondID.Text = _dr["RespondID"].ToString();
            txtDiagTime.Text = _dr["InitTimeofDiag"].ToString();
            
        }

        private bool IsSatisfyCondition()
        {
            bool satisfy = true;
            string dtcHex;
            int dtcInt;
            if (txtRequestID.Text == "")
            {
                MessageBox.Show("请求ID不能为空");
                satisfy = false;
            }
            else if (!ifHex(txtRequestID.Text, out strRequestID))
            {
                MessageBox.Show("请在请求ID框中输入十六进制的数");
                satisfy = false;
            }
            if (txtRespondID.Text == "")
            {
                MessageBox.Show("响应ID不能为空");
                satisfy = false;
            }
            else if (!ifHex(txtRespondID.Text, out strRespondID))
            {
                MessageBox.Show("请在响应ID框中输入十六进制的数");
                satisfy = false;
            }
            if (txtCddName.Text == "")
            {
                MessageBox.Show("请输入Cdd名称");
                satisfy = false;
            }
            if (txtDiagTime.Text == "")
            {
                MessageBox.Show("请输入Cdd名称");
                satisfy = false;
            }
            else
            {
                double time;
                if(!double.TryParse(txtDiagTime.Text,out time))
                {
                    XtraMessageBox.Show("请输入数字");
                    satisfy = false;
                }
            }
            return satisfy;
        }
        private bool ifHex(string HexValue, out string Hex)
        {
            string strHex;//临时取出来的不带有0x的16进制值
            int strDec;//十进制值
            if (HexValue.Length > 2)
            {
                if (HexValue.ToLower().Substring(0, 2) == "0x")
                {
                    HexValue = "0x" + HexValue.Remove(0, 2);
                    strHex = HexValue.Remove(0, 2);
                }
                else
                {
                    strHex = HexValue;
                    HexValue = "0x" + HexValue;
                }
            }
            else
            {
                strHex = HexValue;
                HexValue = "0x" + HexValue;
            }
            if (!(int.TryParse(strHex, System.Globalization.NumberStyles.AllowHexSpecifier, null, out strDec)))
            {
                Hex = "";
                return false;
            }
            Hex = HexValue;
            return true;
        }
        private void ClearDataUI()
        {
            txtDUTname.Enabled = false;
            
            txtCddName.Text = "";
            txtRequestID.Text = "";
            txtRespondID.Text = "";
            txtDiagTime.Text = "";
        }
        private object[] GetDataFromUI()
        {
            object[] row = new object[gvDTC.Columns.Count];
            row[0] = txtDUTname.Text;
            row[1] = txtCddName.Text;
            row[2] = strRequestID;
            row[3] = strRespondID;
            row[4] = txtDiagTime.Text;
            row[5] = "";
            row[6] = "";
            row[7] = "请右击查看DTC故障信息";
            row[8] = "请右击查看DTC报文信息";
            return row;

        }
        private void UpDataRow()
        {
            int i = 0;
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["DUTname"] = txtDUTname.Text;
            dict["Cddname"] = txtCddName.Text;
            dict["RequestID"] = txtRequestID.Text;
            dict["RespondID"] = txtRespondID.Text;
            dict["InitTimeofDiag"] = txtDiagTime.Text;
            foreach (KeyValuePair<string,string> lItem in dict)
            {            
                _dt.Rows[selectRow][i] = lItem.Value;
                i++;
            }


        }
        private enum DataOper
        {
            Add = 0,
            Modify = 1,

        }

        private void tsmiMsg_Click(object sender, EventArgs e)
        {
            if (_dr == null)
                return;
            DTCMessage Ev = new DTCMessage(_dr["MessageInfo"].ToString());
            Ev.ShowDialog();
            if (Ev.DialogResult == DialogResult.OK)
            {
                _dt.Rows[selectRow]["MessageInfo"] = GlobalVar.DictAssessItem;
                gcDTC.DataSource = _dt;
                GlobalVar.strEvent = DerDtToJson();
                GlobalVar.DictAssessItem = "";
            }
        }

        private void tsmiFault_Click(object sender, EventArgs e)
        {
            if (_dr == null)
                return;
            DTCFault Ev = new DTCFault(_dr["FaultInfo"].ToString());
            Ev.ShowDialog();
            if (Ev.DialogResult == DialogResult.OK)
            {
                _dt.Rows[selectRow]["FaultInfo"] = GlobalVar.DictAssessItem;
                gcDTC.DataSource = _dt;
                GlobalVar.strEvent = DerDtToJson();
                GlobalVar.DictAssessItem = "";
            }
        }

        private void gcDTC_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var hi = gvDTC.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //取一行值
            gvDTC.SelectRow(hi.RowHandle);
            selectRow = hi.RowHandle;
            _dr = this.gvDTC.GetDataRow(selectRow);

            Update();
        }
    }
}