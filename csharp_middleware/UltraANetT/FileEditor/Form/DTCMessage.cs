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
    public partial class DTCMessage : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        DataTable _dt = new DataTable();
        Dictionary<string, string> _dictItem = new Dictionary<string, string>();
        Dictionary<string, string> dictItem = new Dictionary<string, string>();
        string DictKey = "";
        private int selectRow;
        private DataRow _dr;
        private DataOper _currentOperate;
        private string strMsgID;

        public DTCMessage()
        {
            InitializeComponent();
            BingEvent();
            ShieldRight();
        }
        public DTCMessage(string itemJson)
        {
            InitializeComponent();
            BingEvent();
            //DictKey = keyDict;
            //foreach (KeyValuePair<string, string> dict in GlobalVar.DictAssessItem)
            //{
            //    dictItem[dict.Key] = dict.Value;
            //}
            //dictItem = GlobalVar.DictAssessItem;
            IniteDt();
            GlobalVar.DictAssessItem = "";
            if (itemJson == "")
            {
                MessageBox.Show("该用例还没有录入评价项目");
                return;
            }
            List<Dictionary<string, string>> itemList = Json.DeserJsonDictStrList(itemJson);
            bool empty = false;
            foreach (var item in itemList)
            {
                //int i = item.Count;
                //object[] obj = new object[i];
                //int j = 0;
                
                foreach (KeyValuePair<string, string> dict in item)
                {
                    empty = empty||(dict.Value != "") ;
                   
                }
                //_dt.Rows.Add(obj);
            }
            if (!empty)
            {
                //if (itemJson == "")
                {
                    MessageBox.Show("该用例还没有录入评价项目");
                    return;
                }
            }
            //gcItem.DataSource = _dt;
            foreach (var item in itemList)
            {
                int i = item.Count;
                object[] obj = new object[i];
                int j = 0;
                foreach (KeyValuePair<string, string> dict in item)
                {
                    obj[j] = dict.Value;
                    j++;
                }
                _dt.Rows.Add(obj);
            }
            gcMsg.DataSource = _dt;
            hideContainerRight.Visible = false;
            ShieldRight();
        }
        private void ShieldRight()
        {
            ContextMenu emptyMenu_1 = new ContextMenu();
            txtMsgID.Properties.ContextMenu = emptyMenu_1;
            ContextMenu emptyMenu_2 = new ContextMenu();
            txtPeriod.Properties.ContextMenu = emptyMenu_2;
            ContextMenu emptyMenu_3 = new ContextMenu();
            txtByteFirst.Properties.ContextMenu = emptyMenu_3;
            ContextMenu emptyMenu_4 = new ContextMenu();
            txtByteSec.Properties.ContextMenu = emptyMenu_4;
            ContextMenu emptyMenu_5 = new ContextMenu();
            txtByteThird.Properties.ContextMenu = emptyMenu_5;
            ContextMenu emptyMenu_6 = new ContextMenu();
            txtByteFour.Properties.ContextMenu = emptyMenu_6;
            ContextMenu emptyMenu_7 = new ContextMenu();
            txtByteFive.Properties.ContextMenu = emptyMenu_7;
            ContextMenu emptyMenu_8 = new ContextMenu();
            txtByteSix.Properties.ContextMenu = emptyMenu_8;
            ContextMenu emptyMenu_9 = new ContextMenu();
            txtByteSeven.Properties.ContextMenu = emptyMenu_9;
            ContextMenu emptyMenu_10 = new ContextMenu();
            txtByteEigh.Properties.ContextMenu = emptyMenu_10;
        }
        public void IniteDt()
        {
            _dt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvMsg.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList)
                _dt.Columns.Add(new DataColumn(colName, typeof(object)));
            //gcItem.DataSource = _dt;


        }
        private void InitDict()
        {
            var coList = new List<string>();
            foreach (GridColumn col in gvMsg.Columns)
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
            foreach (GridColumn col in gvMsg.Columns)
                coList.Add(col.FieldName);
            List<Dictionary<string,string>> dtList = new List<Dictionary<string, string>>();
            //int i = 0;
            foreach(DataRow row in _dt.Rows)
            {
                _dictItem = new Dictionary<string, string>();
                foreach(string col in coList)
                    _dictItem[col] = row[col].ToString();
                dtList.Add(_dictItem);
            }
            dtJson = Json.SerJson(dtList);
            return dtJson;
        }

        private void DTCMessage_FormClosed(object sender, FormClosedEventArgs e)
        {
            //EmlTemplate emlTem = (EmlTemplate)this.Owner;
            if (GlobalVar.DictAssessItem != "")
            {
                if (XtraMessageBox.Show("是否需要保存该用例的评价项目相关信息？", "提示", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning) ==
                        DialogResult.Cancel)
                {
                    GlobalVar.DictAssessItem = "";
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
            ////GlobalVar.DictAssessItem = assessItem;
            
        
        //}
        //if (!same)
        //{
        //    GlobalVar.DictAssessItem.Add(DictKey, assessItem);
        //}
        //emlTem.UpdateAssessItemRow(assessItem);
    }

        /// <summary>
        /// 限制datatable里不能添加重复项
        /// </summary>
        /// <returns>false为有重复项</returns>
        private bool PrimaryKey()
        {
            string MessageIDhex = "";
            ifHex(txtMsgID.Text, out MessageIDhex);


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
                if (_dt.Rows[j]["MessageID"].ToString() == MessageIDhex && _dt.Rows[j]["Period"].ToString() == txtPeriod.Text)
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
                    //object[] row = GetDataFromUI();
                    bool suc = IsMatchCondition();
                    if (!suc)
                        return;
                    object[] row = GetDataFromUI();
                    //IniteDt();
                    _dt.Rows.Add(row);
                    gcMsg.DataSource = _dt;
                    GlobalVar.DictAssessItem = DerDtToJson();
                    Show(DLAF.LookAndFeel, this, "添加成功...", "", new[] { DialogResult.OK }, null, 0,
                        MessageBoxIcon.Information);
                    ClearDataUI();
                    dpAssess.Visibility = DockVisibility.Visible;
                    break;
                //更新
                case DataOper.Modify:
                    //object[] mrow = GetDataFromUI();
                    bool sucM = IsMatchCondition();
                    if (!sucM)
                        return;
                    //object[] mrow = GetDataFromUI();
                    UpDataRow();
                    GlobalVar.DictAssessItem = DerDtToJson();
                    Show(DLAF.LookAndFeel, this, "修改成功...", "", new[] { DialogResult.OK }, null, 0,
                        MessageBoxIcon.Information);
                    ClearDataUI();
                    dpAssess.Visibility = DockVisibility.Hidden;
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
            GlobalVar.DictAssessItem = DerDtToJson();

        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Add;
            dpAssess.Visibility = DockVisibility.Visible;

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
        }

        private void gvItem_MouseDown(object sender, MouseEventArgs e)
        {
            var hi = gvMsg.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell)
            {
                _dr = null;
                return;
            }
            if (hi.RowHandle < 0)
            {
                _dr = null;
                return;
            }
            //取一行值
            gvMsg.SelectRow(hi.RowHandle);
            selectRow = hi.RowHandle;
            _dr = this.gvMsg.GetDataRow(selectRow);
        }
        private void SetDataToUI()
        {
            
            if(_dr==null)
                return;
            txtMsgID.Text = _dr["MessageID"].ToString();
            txtPeriod.Text = _dr["Period"].ToString();
            txtByteFirst.Text = _dr["ByteFirst"].ToString();
            txtByteSec.Text = _dr["ByteSecond"].ToString();
            txtByteThird.Text = _dr["ByteThird"].ToString();
            txtByteFour.Text = _dr["ByteFourth"].ToString();
            txtByteFive.Text = _dr["ByteFive"].ToString();
            txtByteSix.Text = _dr["ByteSixth"].ToString();
            txtByteSeven.Text = _dr["ByteSeventh"].ToString();
            txtByteEigh.Text = _dr["ByteEighth"].ToString();
        }

        private void ClearDataUI()
        {
            txtMsgID.Text = "";
            txtPeriod.Text = "";
            txtByteFirst.Text = "";
            txtByteSec.Text = "";
            txtByteThird.Text = "";
            txtByteFour.Text = "";
            txtByteFive.Text = "";
            txtByteSix.Text = "";
            txtByteSeven.Text = "";
            txtByteEigh.Text = "";
        }
        private bool IsMatchCondition()
        {
            bool suc = true;
            
            if (txtMsgID.Text == "")
            {
                XtraMessageBox.Show("诊断工作报文ID不能为空");
                suc = false;
            }
            strMsgID = "";
            if (!ifHex(txtMsgID.Text, out strMsgID))
            {
                XtraMessageBox.Show("诊断工作报文ID必须是十六进制的数");
                suc = false;
            }
            else if (txtPeriod.Text == "")
            {
                XtraMessageBox.Show("周期不能为空");
                suc = false;
            }
            else if (!IsOneOrZero(txtByteFirst.Text))
            {
                XtraMessageBox.Show("高字节第一项必须输入0或者1，不能为空");
                suc = false;
            }
            else if (!IsOneOrZero(txtByteSec.Text))
            {
                MessageBox.Show("高字节第二项必须输入0或者1，不能为空");
                suc = false;
            }
            else if (!IsOneOrZero(txtByteFirst.Text))
            {
                XtraMessageBox.Show("高字节第三项必须输入0或者1，不能为空");
                suc = false;
            }
            else if (!IsOneOrZero(txtByteFour.Text))
            {
                XtraMessageBox.Show("高字节第四项必须输入0或者1，不能为空");
                suc = false;
            }
            else if (!IsOneOrZero(txtByteFive.Text))
            {
                XtraMessageBox.Show("低字节中第一项必须输入0或者1，不能为空");
                suc = false;
            }
            else if (!IsOneOrZero(txtByteSix.Text))
            {
                XtraMessageBox.Show("低字节中第二项必须输入0或者1，不能为空");
                suc = false;
            }
            else if (!IsOneOrZero(txtByteSeven.Text))
            {
                XtraMessageBox.Show("低字节中第三项必须输入0或者1，不能为空");
                suc = false;
            }
            else if (!IsOneOrZero(txtByteEigh.Text))
            {
                XtraMessageBox.Show("低字节中第四项必须输入0或者1，不能为空");
                suc = false;
            }
            return suc;
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
        private bool IsOneOrZero(string txtText)
        {
            bool suc = false;
            int a;
            if (IsHex(txtText))
            {
                suc = true;
            }
            return suc;
        }

        private bool IsHex(string txtText)
        {
            bool isHex = false;
            int Byte = 0;
            if (int.TryParse(txtText, System.Globalization.NumberStyles.AllowHexSpecifier, null, out Byte))
            {
                if (Byte>=0 && Byte< 256)
                    isHex = true;
            }
            return isHex;
        }

        private object[] GetDataFromUI()
        {
            
            object[] row = new object[]
            {
                strMsgID,
                txtPeriod.Text,
                txtByteFirst.Text,
                txtByteSec.Text,
                txtByteThird.Text,
                txtByteFour.Text,
                txtByteFive.Text,
                txtByteSix.Text,
                txtByteSeven.Text,
                txtByteEigh.Text,
            };
                    
            return row;

        }
        private void UpDataRow()
        {
            int i = 0;
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["MessageID"] = txtMsgID.Text;
            dict["Period"] = txtPeriod.Text;
            dict["ByteFirst"] = txtByteFirst.Text;
            dict["ByteSecond"] = txtByteSec.Text;
            dict["ByteThird"] = txtByteThird.Text;
            dict["ByteFourth"] = txtByteFirst.Text;
            dict["ByteFive"] = txtByteFive.Text;
            dict["ByteSixth"] = txtByteSix.Text;
            dict["ByteSeventh"] = txtByteSeven.Text;
            dict["ByteEighth"] = txtByteEigh.Text;
            
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

        private void BingEvent()
        {
            txtByteSec.KeyPress += txtByte_KeyPress;
            txtByteFirst.KeyPress += txtByte_KeyPress;
            txtByteThird.KeyPress += txtByte_KeyPress;
            txtByteFour.KeyPress += txtByte_KeyPress;
            txtByteFive.KeyPress += txtByte_KeyPress;
            txtByteSix.KeyPress += txtByte_KeyPress;
            txtByteSeven.KeyPress += txtByte_KeyPress;
            txtByteEigh.KeyPress += txtByte_KeyPress;   
            

        }
        private void txtByte_KeyPress(object sender, KeyPressEventArgs e)
        {
            char keychar;
            keychar = e.KeyChar;
            if((keychar>= '0' && keychar<='9')||(keychar>= 'a' && keychar<= 'f') || (keychar >= 'A' && keychar <= 'F') || (e.KeyChar == (char)Keys.Back) || (e.KeyChar == (char)Keys.Delete))
            //if ((e.KeyChar == '0') || (e.KeyChar == '1') || (e.KeyChar == (char)Keys.Back) || (e.KeyChar == (char)Keys.Delete))//这是允许输入0或者1数字
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }

        private void gcMsg_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var hi = gvMsg.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell)
            {
                _dr = null;
                return;
            }
            if (hi.RowHandle < 0)
            {
                _dr = null;
                return;
            }
            //取一行值
            gvMsg.SelectRow(hi.RowHandle);
            selectRow = hi.RowHandle;
            _dr = this.gvMsg.GetDataRow(selectRow);

            Update();
        }
    }
}