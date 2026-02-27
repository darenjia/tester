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
using DevExpress.XtraBars.Docking;
using DevExpress.XtraLayout;
using DevExpress.LookAndFeel;
using UltraANetT.Module;

namespace UltraANetT.Form
{
    public partial class EmlAssessItemRelevant : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        DataTable _dt = new DataTable();
        Dictionary<string, string> _dictItem = new Dictionary<string, string>();
        private int _rowIndex;
        private DataRow _dr;
        private DataOper _currentOperate;
        private EmlLibrary _el;

        public EmlAssessItemRelevant()
        {
            InitializeComponent();
            ShieldRight();
        }

        public EmlAssessItemRelevant(EmlLibrary el, string itemJson)
        {
            InitializeComponent();
            _el = el;
            InitGrid();
            JsonToDt(itemJson);
            gcItem.DataSource = _dt;
            hideContainerRight.Visible = false;
            ShieldRight();
        }

        private void JsonToDt(string itemJson)
        {
            if (itemJson == "" || itemJson == "{}")
            {
                return;
            }
            List<Dictionary<string, string>> itemList = Json.DeserJsonDictStrList(itemJson);
            bool empty = false;
            foreach (var item in itemList)
            {
                foreach (KeyValuePair<string, string> dict in item)
                {
                    empty = empty || (dict.Value != "");
                }
            }
            if (!empty)
            {
                //MessageBox.Show("该用例还没有录入评价项目");
                return;
            }
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
        }

        private void ShieldRight()
        {
            ContextMenu emptyMenu_1 = new ContextMenu();
            teAssess.Properties.ContextMenu = emptyMenu_1;
            ContextMenu emptyMenu_2 = new ContextMenu();
            teMaxValue.Properties.ContextMenu = emptyMenu_2;
            ContextMenu emptyMenu_3 = new ContextMenu();
            teMinValue.Properties.ContextMenu = emptyMenu_3;
            ContextMenu emptyMenu_4 = new ContextMenu();
            teNormalValue.Properties.ContextMenu = emptyMenu_4;
            ContextMenu emptyMenu_5 = new ContextMenu();
            teValueDescription.Properties.ContextMenu = emptyMenu_5;
        }



        public void InitGrid()
        {
            _dt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvItem.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList)
                _dt.Columns.Add(new DataColumn(colName, typeof(object)));
            //gcItem.DataSource = _dt;


        }
        private void InitDict()
        {
            var coList = new List<string>();
            foreach (GridColumn col in gvItem.Columns)
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
            foreach (GridColumn col in gvItem.Columns)
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

        /// <summary>
        /// 限制datatable里不能添加重复项
        /// </summary>
        /// <returns>false为有重复项</returns>
        private bool PrimaryKey()
        {
            bool primary = false; //false默认为是有重复项
            int i = 0;
            for (int j = 0; j < _dt.Rows.Count; j++)
            {
                if (_currentOperate == DataOper.Modify)
                {
                    if (_rowIndex == j)
                        continue;
                }
                primary = false;
                if (_dt.Rows[j]["AssessItem"].ToString() != teAssess.Text)
                {
                    primary = true;
                }
                else
                {
                    i++;
                }
            }
            if (i > 0)
            {
                if (_currentOperate == DataOper.Modify)
                    XtraMessageBox.Show("不能更改为已添加过的数据");
                if (_currentOperate == DataOper.Add)
                    XtraMessageBox.Show("请不要添加重复项");
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
        private bool CompareMaxMinNormalValue()
        {
            bool suc = false;
            if (teValueDescription.Text == "")
            {
                if (teAssess.Text == "" || teMaxValue.Text == "" || teNormalValue.Text == "" || teMinValue.Text == "")
                {
                    XtraMessageBox.Show("除值描述以外，其他项的内容都不能为空，请先填写完整");
                    suc = true;
                }
                else
                {
                    int index = radioGroup.SelectedIndex;
                    if (index == 0)
                    {
                        double min;
                        double max;
                        double normal;
                        if (!double.TryParse(teMinValue.Text, out min))
                        {
                            XtraMessageBox.Show("请在最小值文本框中输入数字");
                            suc = true;
                        }
                        else if (!double.TryParse(teNormalValue.Text, out normal))
                        {
                            XtraMessageBox.Show("请在正常值文本框中输入数字");
                            suc = true;
                        }
                        else if (!double.TryParse(teMaxValue.Text, out max))
                        {
                            XtraMessageBox.Show("请在最大值文本框中输入数字");
                            suc = true;
                        }
                        else if (double.TryParse(teMinValue.Text, out min) &&
                                 double.TryParse(teNormalValue.Text, out normal) &&
                                 double.TryParse(teMaxValue.Text, out max))
                        {
                            if (min > normal)
                            {
                                XtraMessageBox.Show("最小值不能大于正常值");
                                suc = true;
                            }
                            else if (normal > max)
                            {
                                XtraMessageBox.Show("正常值不能大于最大值");
                                suc = true;
                            }
                            else if (min > max)
                            {
                                XtraMessageBox.Show("最小值不能大于最大值");
                                suc = true;
                            }
                        }
                    }
                    else if (index == 1)
                    {
                        int min;
                        int max;
                        int normal;
                        if (!ifHexText(teMinValue, out min))
                        {
                            XtraMessageBox.Show("请在最小值文本框中输入十六进制值");
                            suc = true;
                        }
                        else if (!ifHexText(teNormalValue, out normal))
                        {
                            XtraMessageBox.Show("请在正常值文本框中输入十六进制值");
                            suc = true;
                        }
                        else if (!ifHexText(teMaxValue, out max))
                        {
                            XtraMessageBox.Show("请在最大值文本框中输入十六进制值");
                            suc = true;
                        }
                        else if (ifHexText(teMinValue, out min) &&
                                 ifHexText(teNormalValue, out normal) &&
                                 ifHexText(teMaxValue, out max))
                        {
                            if (min > normal)
                            {
                                XtraMessageBox.Show("最小值不能大于正常值");
                                suc = true;
                            }
                            else if (normal > max)
                            {
                                XtraMessageBox.Show("正常值不能大于最大值");
                                suc = true;
                            }
                            else if (min > max)
                            {
                                XtraMessageBox.Show("最小值不能大于最大值");
                                suc = true;
                            }
                        }
                    }
                }
                
            }
            else
            {
                teMaxValue.Text = @"";
                teNormalValue.Text = @"";
                teMinValue.Text = @"";
                if (teAssess.Text == "")
                {
                    XtraMessageBox.Show("评价项目不能为空");
                    suc = true;
                }
            }
            return suc;
        }

        private bool ifHexText(TextEdit txtControl,out int Dec)
        {
            string strHex;
            if (txtControl.Text.Length > 2)
            {
                if (txtControl.Text.ToLower().Substring(0, 2) == "0x")
                {
                    txtControl.Text = "0x" + txtControl.Text.Remove(0, 2).ToUpper();
                    strHex = txtControl.Text.Remove(0, 2);
                }
                else
                {
                    strHex = txtControl.Text;
                    txtControl.Text = "0x" + txtControl.Text.ToUpper();
                }
            }
            else
            {
                strHex = txtControl.Text;
                txtControl.Text = "0x" + txtControl.Text.ToUpper();
            }
            bool ifHex = int.TryParse(strHex, System.Globalization.NumberStyles.AllowHexSpecifier, null, out Dec);
            return ifHex;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (CompareMaxMinNormalValue())
                return;
            if (!PrimaryKey())
                return;
            Submit();
        }

        private void Submit()
        {
            string strAssessItem = "";
            string strOldAssessItem= DerDtToJson();
            switch (_currentOperate)
            {
                //添加
                case DataOper.Add:
                    object[] row = GetDataFromUI();
                    if (row[4].ToString() == "")
                    {
                        row[4] = "--";
                    }
                    else
                    {
                        row[1] = "--";
                        row[2] = "--";
                        row[3] = "--";
                    }
                    _dt.Rows.Add(row);
                    gcItem.DataSource = _dt;
                    strAssessItem = DerDtToJson();
                    ClearDataUI();
                    dpAssess.Visibility = DockVisibility.Visible;
                    break;
                //更新
                case DataOper.Modify:
                    UpDataRow();
                    strAssessItem = DerDtToJson();
                    ClearDataUI();
                    dpAssess.Visibility = DockVisibility.Hidden;
                    break;
                case DataOper.Del:
                    _dr.Delete();
                    strAssessItem = DerDtToJson();
                    break;
            }

            if (_el.SaveJsonToDB(strAssessItem))
            {
                switch (_currentOperate)
                {
                    case DataOper.Add:
                        XtraMessageBox.Show("添加成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case DataOper.Modify:
                        XtraMessageBox.Show("修改成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                }
            }
            else
            {
                switch (_currentOperate)
                {
                    case DataOper.Add:
                        XtraMessageBox.Show("未知错误，添加失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case DataOper.Modify:
                        XtraMessageBox.Show("未知错误，修改失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case DataOper.Del:
                        XtraMessageBox.Show("未知错误，删除失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                }
                JsonToDt(strOldAssessItem);
                gcItem.DataSource = _dt;
            }
        }

        private void tsmiDel_Click(object sender, EventArgs e)
        {
            if(_dr==null)
                return;
            _currentOperate = DataOper.Del;
            Submit();
        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Add;
            ClearDataUI();
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
            cmsEnabled(false);
            var hi = gvItem.CalcHitInfo(e.Location);
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
            gvItem.SelectRow(hi.RowHandle);
            _rowIndex = hi.RowHandle;
            cmsEnabled(true);
            _dr = this.gvItem.GetDataRow(_rowIndex);
        }
        private void SetDataToUI()
        {
            if (_dr["MinValue"].ToString().Length > 2)
            {
                if (_dr["MinValue"].ToString().Substring(0, 2).ToLower() == "0x")
                {
                    radioGroup.SelectedIndex = 1;
                }
                else
                {
                    radioGroup.SelectedIndex = 0;
                }
            }
            else
            {
                radioGroup.SelectedIndex = 0;
            }
            if (_dr["ValueDescription"].ToString() == @"--")
            {
                teValueDescription.Text = "";
                teMinValue.Text = _dr["MinValue"].ToString();
                teNormalValue.Text = _dr["NormalValue"].ToString();
                teMaxValue.Text = _dr["MaxValue"].ToString();
            }
            else
            {
                teMinValue.Text = "";
                teNormalValue.Text = "";
                teMaxValue.Text = "";
                teValueDescription.Text = _dr["ValueDescription"].ToString();
            }
            teAssess.Text = _dr["AssessItem"].ToString();
        }

        private void ClearDataUI()
        {
            teAssess.Text = "";
            teMinValue.Text = "";
            teNormalValue.Text = "";
            teMaxValue.Text = "";
            teValueDescription.Text = "";
        }
        private object[] GetDataFromUI()
        {
            object[] row = new object[]
            {
                teAssess.Text,
                teMinValue.Text,
                teNormalValue.Text,
                teMaxValue.Text,
                teValueDescription.Text
            };
                    
            return row;

        }
        private void UpDataRow()
        {
            int i = 0;
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["AssessItem"] = teAssess.Text.Trim();
            dict["MinValue"] = teMinValue.Text.Trim();
            dict["NormalValue"] = teNormalValue.Text.Trim();
            dict["MaxValue"] = teMaxValue.Text.Trim();
            dict["ValueDescription"] = teValueDescription.Text.Trim();
            if (dict["ValueDescription"] == "")
            {
                dict["ValueDescription"] = "--";
            }
            else
            {
                dict["MinValue"] = "--";
                dict["NormalValue"] = "--";
                dict["MaxValue"] = "--";
            }
            foreach (KeyValuePair<string,string> lItem in dict)
            {            
                _dt.Rows[_rowIndex][i] = lItem.Value;
                i++;
            }


        }
        private enum DataOper
        {
            Add = 0,
            Modify = 1,
            Del=2,
        }

        private void cmsEnabled(bool isSwitch)
        {
            switch (isSwitch)
            {
                case true:
                    tsmiModify.Enabled = true;
                    tsmiDel.Enabled = true;
                    break;
                case false:
                    tsmiModify.Enabled = false;
                    tsmiDel.Enabled = false;
                    break;
            }
        }

        private void gcItem_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            cmsEnabled(false);
            var hi = gvItem.CalcHitInfo(e.Location);
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
            gvItem.SelectRow(hi.RowHandle);
            _rowIndex = hi.RowHandle;
            _dr = this.gvItem.GetDataRow(_rowIndex);
            cmsEnabled(true);
            Update();
        }
    }
}