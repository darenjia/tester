using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using ProcessEngine;
using UltraANetT.Interface;

namespace UltraANetT.Module
{
    public partial class Segment : XtraUserControl,IDraw
    {
        #region 设置变量
        
        private readonly ProcShow _show = new ProcShow();

        private readonly ProcStore _store = new ProcStore();

        private readonly IDraw _draw;

        private readonly ProcLog _log = new ProcLog();

        private string oldSegName;
        private string oldBaud;
        /// <summary>
        /// 字典类型的员工信息
        /// </summary>
        private Dictionary<string, object> _dictSegment = new Dictionary<string, object>();
        /// <summary>
        /// 当前操作类别
        /// </summary>
        private DataOper _currentOperate;
        /// <summary>
        /// 定义一行数据
        /// </summary>
        private DataRow _dr;
        private string role;
        private LogicalControl _LogC = new LogicalControl();
        private DataTable _dt;
        private int RowIndex = 0;
        #endregion


        public Segment()
        {
            InitializeComponent();
            _draw = this;
            _draw.InitGrid();
            _draw.InitDict();
            role = _LogC.RoleSelect(GlobalVar.UserName);
            RoleFunction(role);
            hideContainerRight.Visible = false;
            ShieldRight();
        }
        private void RoleFunction(string role)
        {
            switch (role)
            {
                case "administer":
                    break;
                case "configurator":
                    CMSOrder.Enabled = false;
                    break;
                case "tester":
                    CMSOrder.Enabled = false;
                    break;
                default:
                    break;
            }
        }
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            GlobalVar.NumberChanges = 0;
            _draw.Submit();
        }

        private void gcSegment_DoubleClick(object sender, EventArgs e)
        {
            //获得所点击的控件
            var control = sender as System.Windows.Forms.Control;
            if (control == null) return;
            //获得光标位置
            var hi = gvSegment.CalcHitInfo(control.PointToClient(MousePosition));
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
            //取值一行值
            RowIndex = hi.RowHandle;
            var selectedRow = this.gvSegment.GetDataRow(this.gvSegment.FocusedRowHandle);
            //赋值
            _draw.SetDataToUI(selectedRow);
            isSegmentUse(DataOper.Modify,selectedRow);
            //双击某一行也可以修改 
            _draw.SwitchCtl(true);
            string sdfsd = selectedRow["SegmentName"].ToString();
            _dictSegment["OldSegmentName"] = selectedRow["SegmentName"].ToString();
            _dictSegment["OldBaud"] = selectedRow["Baud"];
            _dictSegment["OldCorrespond"] = selectedRow["Correspond"];

            oldSegName = selectedRow["SegmentName"].ToString();
            oldBaud = selectedRow["Baud"].ToString();
            _currentOperate = DataOper.Modify;
        }

        
        private void gvSegment_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                tsmiDel.Enabled = true;
                //_deptName = gvSegment.GetRowCellValue(e.RowHandle, gvSegment.Columns["SegmentName"]).ToString();
            }
        }

        private void isSegmentUse(DataOper oper,DataRow dr)
        {
            if(dr == null)
                return;
            Dictionary<string, object> SegmentdictStr = new Dictionary<string, object>();
            SegmentdictStr["SegmentName"] = dr["SegmentName"];
            var dept =
                _store.GetSingnalColByName(EnumLibrary.EnumTable.SegmentByName, SegmentdictStr, 0);
            if (dept.Count == 0)
            {
                if (oper == DataOper.Modify)
                {
                    cmbSegmentType.Enabled = true;
                    txtSegmentName.Enabled = true;
                }
                else
                {
                    tsmiDel.Enabled = true;
                }
            }
            else
            {
                if (oper == DataOper.Modify)
                {
                    cmbSegmentType.Enabled = false;
                    txtSegmentName.Enabled = false;
                }
                else
                {
                    tsmiDel.Enabled = false;
                }
            }
        }

        void IDraw.InitGrid()
        {
            var coList = new List<string>();
            foreach (GridColumn col in gvSegment.Columns)
                coList.Add(col.FieldName);
            _dt = _show.DrawDtFromMultiple(coList.ToArray(), EnumLibrary.EnumTable.Segment);
            gcSegment.DataSource = _dt;
        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            txtSegmentName.Text = "";
            txtBaud.Text = "";
            _currentOperate = DataOper.Add;
            _draw.SwitchCtl(true);
        }

        private void tsmiModify_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Modify;
            if (_dr != null)
            {
                isSegmentUse(DataOper.Modify,_dr);
                string sdfsd = _dr["SegmentName"].ToString();
                _dictSegment["OldSegmentName"] = _dr["SegmentName"].ToString();
                _dictSegment["OldBaud"] = _dr["Baud"];
                _dictSegment["OldCorrespond"] = _dr["Correspond"];
                _draw.SetDataToUI(_dr);
                _draw.SwitchCtl(true);
                oldSegName = _dr["SegmentName"].ToString();
                oldBaud = _dr["Baud"].ToString();
            }
        }

        private void tsmiDel_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Del;
            _draw.Submit();
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
                    if (RowIndex == j)
                        continue;
                }
                primary = false;
                if (_dt.Rows[j]["SegmentName"].ToString() == cmbSegmentType.Text+txtSegmentName.Text.Trim() && _dt.Rows[j]["Baud"].ToString() == txtBaud.Text)
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

        private bool ifNull()
        {
            bool txtnull = false;
            double baud = 0;
            //if (txtSegmentName.Text.Length > 3)
            //{
            //    if (this.txtSegmentName.Text.Trim().Substring(0, 3).ToUpper() == "CAN" ||
            //        this.txtSegmentName.Text.Trim().Substring(0, 3).ToUpper() == "LIN")
            //    {
                    
            //    }
            //    else
            //    {
            //        XtraMessageBox.Show("请检查网段项，开头首字母必须为CAN或者LIN");
            //        txtnull = true;
            //    }
            //}
            //else
            //{
            //    XtraMessageBox.Show("请检查网段项，开头首字母必须为CAN或者LIN");
            //    txtnull = true;
            //}
            if (txtSegmentName.Text==""||txtBaud.Text=="")
            {
                XtraMessageBox.Show("请检查输入项，不能为空");
                txtnull = true;
            }
            else
            {
                if (!(double.TryParse(txtBaud.Text, out baud)))
                {
                    XtraMessageBox.Show("波特率为数字，请输入正确数值");
                    txtnull = true;
                }
            }
            return txtnull;
        }

        void IDraw.Submit()
        {
            if (!PrimaryKey())
                return;
            if (_currentOperate != DataOper.Del)
            {
                if (ifNull())
                    return;
            }

            switch (_currentOperate)
            {
                //添加
                case DataOper.Add:
                    string errorQ;
                    _draw.GetDataFromUI();
                    _store.AddSegment(_dictSegment, out errorQ);
                    if (errorQ == "")
                    {
                        _draw.InitGrid();
                        ClearDataToUi();
                        _draw.SwitchCtl(true);
                        Dictionary<string, object> dictAConfig = new Dictionary<string, object>
                        {
                            {"EmployeeNo", GlobalVar.UserNo},
                            {"EmployeeName", GlobalVar.UserName},
                            {"OperTable", "网段"},
                            {"SegName", _dictSegment["SegmentName"]},
                            {"Baud", _dictSegment["Baud"]},
                        };
                        _log.WriteLog(EnumLibrary.EnumLog.AddCAN, dictAConfig);
                    }
                    else
                    {
                        XtraMessageBox.Show("不能添加相同的数据记录...");
                    }

                    break;
                //更新
                case DataOper.Modify:
                    _draw.GetDataFromUI();
                    string errorU;
                    _store.Update(EnumLibrary.EnumTable.Segment, _dictSegment, out errorU);
                    if (errorU == "")
                    {
                        ClearDataToUi();
                        _draw.InitGrid();
                        _draw.SwitchCtl(false);
                        Dictionary<string, object> dictAConfig = new Dictionary<string, object>
                            {
                                {"EmployeeNo", GlobalVar.UserNo},
                                {"EmployeeName", GlobalVar.UserName},
                                {"OperTable", "网段"},
                                {"oldSegName", oldSegName},
                                {"oldBaud", oldBaud},
                                {"newSegName", _dictSegment["SegmentName"]},
                                {"newBaud", _dictSegment["Baud"]}
                            };
                        _log.WriteLog(EnumLibrary.EnumLog.UpdateCAN, dictAConfig);
                    }
                    else
                    {
                        XtraMessageBox.Show("修改操作出错");
                    }
                    break;
                //删除
                case DataOper.Del:
                    if (
                        XtraMessageBox.Show("是否删除此网段？", "提示", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning) ==
                        DialogResult.OK)
                    {
                        Dictionary<string, object> SegmentdictStr = new Dictionary<string, object>();
                        SegmentdictStr["SegmentName"] = _dr["SegmentName"];
                        var deptDel =
                            _store.GetSingnalColByName(EnumLibrary.EnumTable.SegmentByName, SegmentdictStr, 0);
                        if (deptDel.Count == 0)
                        {
                            Dictionary<string, object> dict = new Dictionary<string, object>();
                            dict["SegmentName"] = _dr["SegmentName"];
                            dict["Baud"] = _dr["Baud"];
                            dict["Correspond"] = _dr["Correspond"];
                            string error;
                            _store.Del(EnumLibrary.EnumTable.Segment, dict, out error);
                            if (error == "")
                            {
                                _draw.InitGrid();
                                tsmiDel.Enabled = false;
                                Dictionary<string, object> dictAConfig = new Dictionary<string, object>
                                {
                                    {"EmployeeNo", GlobalVar.UserNo},
                                    {"EmployeeName", GlobalVar.UserName},
                                    {"OperTable", "网段"},
                                    {"SegName", _dictSegment["SegmentName"]},
                                    {"Baud", _dictSegment["Baud"]},
                                };
                                _log.WriteLog(EnumLibrary.EnumLog.DelCAN, dictAConfig);
                            }
                            else
                            {
                                XtraMessageBox.Show("删除操作出错");
                            }
                        }
                        else
                        {
                            XtraMessageBox.Show("改网段已被DBC使用");
                        }
                    }

                    break;
            }
        }

        void ClearDataToUi()
        {
            txtSegmentName.Text = null;
            txtBaud.Text = null;
            seCorrespond.Value = 1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Dictionary<string, object> IDraw.GetDataFromUI()
        {
            _dictSegment["SegmentName"] = cmbSegmentType.Text +"-"+txtSegmentName.Text.Trim();
            _dictSegment["Baud"] = double.Parse(txtBaud.Text);
            _dictSegment["Correspond"] = seCorrespond.Value;
            return _dictSegment;
        }

        /// <summary>
        /// 操作行为枚举
        /// </summary>
        private enum DataOper
        {
            Add = 0,
            Modify = 1,
            Del = 2
        }

        void IDraw.SetDataToUI(DataRow selectedRow)
        {
            string strSegmentAllName = selectedRow["SegmentName"].ToString();

            //if (selectedRow["SegmentName"].ToString().Contains("LIN"))
            //{
            //    cmbSegmentType.EditValue = selectedRow["SegmentName"].ToString().Substring(0, 3);
            //}
            //else
            //{
            //    int endIndex = selectedRow["SegmentName"].ToString().IndexOf("-");
            //    cmbSegmentType.EditValue = selectedRow["SegmentName"].ToString().Substring(endIndex + 1, 3);
            //}

            //string txtSegmentNameStr = selectedRow["SegmentName"].ToString().Substring(3, selectedRow["SegmentName"].ToString().Length - 3);
            //string NewtxtSegmentName = txtSegmentNameStr.ToString().Trim().Substring(1, txtSegmentNameStr.Length - 1);
            cmbSegmentType.EditValue = strSegmentAllName.Substring(0, 3);
            txtSegmentName.Text = strSegmentAllName.Substring(4, strSegmentAllName.Length - 4);
            txtBaud.Text = selectedRow["Baud"].ToString();
            seCorrespond.Value = decimal.Parse(selectedRow["Correspond"].ToString());
        }

        void IDraw.InitDict()
        {
            _dictSegment.Add("OldSegmentName", "");
            _dictSegment.Add("OldBaud", "");
            _dictSegment.Add("OldCorrespond", "");

            _dictSegment.Add("SegmentName", "");
            _dictSegment.Add("Baud", "");
            _dictSegment.Add("Correspond", "");
            
        }

        void IDraw.SwitchCtl(bool isSwitch)
        {
            switch (isSwitch)
            {
                case true:
                    btnSubmit.Enabled = true;
                    dpSegment.Visibility = DockVisibility.Visible;
                    break;
                case false:
                    btnSubmit.Enabled = false;
                    dpSegment.Visibility = DockVisibility.Hidden;
                    break;
            }
        }

        private void gvSegment_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //获得光标位置
                var hi = gvSegment.CalcHitInfo(e.Location);
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
                gvSegment.SelectRow(hi.RowHandle);
                _dr = gvSegment.GetDataRow(hi.RowHandle);
                isSegmentUse(DataOper.Del,_dr);
            }
        }
        private void ShieldRight()
        {
            ContextMenu emptyMenu = new ContextMenu();
            txtBaud.Properties.ContextMenu = emptyMenu;
            txtSegmentName.Properties.ContextMenu = emptyMenu;
        }

        private void cmbSegmentType_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtSegmentName_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtBaud_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }
    }
}
