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
    public partial class Department : XtraUserControl,IDraw
    {
        #region 设置变量
        

        private string _deptName = "";
        /// <summary>
        /// /
        /// </summary>
        private readonly ProcShow _show = new ProcShow();

        private readonly ProcStore _store = new ProcStore();

        private readonly ProcLog _log = new ProcLog();
        private readonly IDraw _draw;

        private string oldMaster;
        private string oldName;
        /// <summary>
        /// 字典类型的员工信息
        /// </summary>
        private Dictionary<string, object> _dictDtm = new Dictionary<string, object>();
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
        #endregion


        public Department()
        {
            InitializeComponent();
            _draw = this;
            _draw.InitGrid();
            _draw.InitDict();
            role = _LogC.RoleSelect(GlobalVar.UserName);
            RoleFunction(role);
            hideContainerRight.Visible = false;
            //屏蔽右键菜单
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
            int depNumber;

            if (!int.TryParse(txtNumber.Text, out depNumber))
            {
                MessageBox.Show(@"无法识别非法数字，请核对...");
                return;
            }
            if (depNumber < 0)
            {
                MessageBox.Show(@"部门人数不能为负数...");
                return;
            }
            _draw.Submit();
        }

        private void gcDepartment_DoubleClick(object sender, EventArgs e)
        {
            //获得所点击的控件
            var control = sender as System.Windows.Forms.Control;
            if (control == null) return;
            //获得光标位置
            var hi = gvDepartment.CalcHitInfo(control.PointToClient(MousePosition));
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //取值一行值
            gvDepartment.SelectRow(hi.RowHandle);
            var selectedRow = this.gvDepartment.GetDataRow(this.gvDepartment.FocusedRowHandle);
            //赋值
            _draw.SetDataToUI(selectedRow);
            //双击某一行也可以修改
            _draw.SwitchCtl(true);

            _currentOperate = DataOper.Modify;
        }


        private void gvDepartment_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                tsmiDel.Enabled = true;
                _deptName = gvDepartment.GetRowCellValue(e.RowHandle, gvDepartment.Columns["Name"]).ToString();

            }
        }

        void IDraw.InitGrid()
        {
            var coList = new List<string>();
            foreach (GridColumn col in gvDepartment.Columns)
                coList.Add(col.FieldName);
            var dt = _show.DrawDtFromMultiple(coList.ToArray(), EnumLibrary.EnumTable.Department);
            gcDepartment.DataSource = dt;
            //从数据中读取部门名称列的数据集合
            var emp = _store.GetSingnalCol(EnumLibrary.EnumTable.Employee, 1);
            //把集合赋值给控件
            if (cbMaster.Properties.Items.Count > 0)
                cbMaster.Properties.Items.Clear();
            cbMaster.Properties.Items.AddRange(emp.ToArray());
        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Add;
            _draw.SwitchCtl(true);
            ClearDataToUi();
        }

        private void tsmiModify_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Modify;
            if (_dr != null)
            {
                _draw.SetDataToUI(_dr);
                _draw.SwitchCtl(true);
                oldName = _dr["Name"].ToString();
                oldMaster = _dr["Master"].ToString();
            }
        }

        private void tsmiDel_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Del;
            _draw.Submit();
            
        }

        void IDraw.Submit()
        {
            int number;
            bool isSuccess = int.TryParse(txtNumber.Text, out number);
            switch (_currentOperate)
            {
                //添加
                case DataOper.Add:
                    string errorQ;
                    _draw.GetDataFromUI();
                    _store.AddDepartment(_dictDtm, out errorQ);
                    if (errorQ == "")
                    {
                        _draw.InitGrid();
                        ClearDataToUi();
                        _draw.SwitchCtl(true);
                        Dictionary<string, object> dictConfig = new Dictionary<string, object>
                        {
                            {"EmployeeNo",GlobalVar.UserNo},
                            {"EmployeeName",GlobalVar.UserName},
                            {"OperTable","部门"},
                            {"Master",   _dictDtm["Master"]},
                            {"Name",   _dictDtm["Name"]},
                        };
                        _log.WriteLog(EnumLibrary.EnumLog.AddDepartment, dictConfig);
                    }
                    else
                    {
                        XtraMessageBox.Show("增加操作出错，请联系工程师帮助解决");
                    }

                    break;
                //更新
                case DataOper.Modify:
                    _draw.GetDataFromUI();
                    string errorU;
                    _store.Update(EnumLibrary.EnumTable.Department, _dictDtm, out errorU);
                    if (errorU == "")
                    {
                        ClearDataToUi();
                        _draw.InitGrid();
                        _draw.SwitchCtl(false);
                        if (oldMaster == _dictDtm["Master"].ToString() && oldName == _dictDtm["Name"].ToString())
                            break;
                        Dictionary<string, object> dictConfig = new Dictionary<string, object>
                        {
                            {"EmployeeNo",GlobalVar.UserNo},
                            {"EmployeeName",GlobalVar.UserName},
                             {"OperTable","部门"},
                            {"oldMaster",   oldMaster},
                            {"oldName",   oldName},
                             {"newMaster",   _dictDtm["Master"]},
                            {"newName",   _dictDtm["Name"]},

                        };
                        _log.WriteLog(EnumLibrary.EnumLog.UpdateDepartment, dictConfig);
                    }
                    else
                    {
                        XtraMessageBox.Show("修改操作出错，请联系工程师帮助解决");
                    }
                    break;
                //删除
                case DataOper.Del:
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict.Add("Name", _deptName);
                    //dict["Name"] = _deptName;
                    string error;
                    _store.Del(EnumLibrary.EnumTable.Department, dict, out error);
                    if (error == "")
                    {
                        _draw.InitGrid();
                        tsmiDel.Enabled = false;
                        Dictionary<string, object> dictConfig = new Dictionary<string, object>
                        {
                            {"EmployeeNo",GlobalVar.UserNo},
                            {"EmployeeName",GlobalVar.UserName},
                            {"OperTable","部门"},
                            {"Master",   _dictDtm["Master"]},
                            {"Name",   _dictDtm["Name"]},

                        };
                        _log.WriteLog(EnumLibrary.EnumLog.DelDepartment, dictConfig);
                    }
                    else
                    {
                        XtraMessageBox.Show("删除操作出错，请联系工程师帮助解决");
                    }
                    break;
            }
        }

        void ClearDataToUi()
        {
            txtName.Text = null;
            cbMaster.Text = null;
            txtNumber.Text = null;
            txtRemark.Text = null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Dictionary<string, object> IDraw.GetDataFromUI()
        {
            _dictDtm["Name"] = txtName.Text;
            _dictDtm["Master"] = cbMaster.Text;
            _dictDtm["NumForDept"] = txtNumber.Text;
            _dictDtm["Remark"] = txtRemark.Text;
            _dictDtm["oldName"] = oldName;
            return _dictDtm;
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
            txtName.Text = selectedRow["Name"].ToString();
            txtNumber.Text = selectedRow["NumForDept"].ToString();
            cbMaster.Text = selectedRow["Master"].ToString();
            txtRemark.Text = selectedRow["Remark"].ToString();
        }

        void IDraw.InitDict()
        {
            _dictDtm.Add("Name", "");
            _dictDtm.Add("Master", "");
            _dictDtm.Add("NumForDept", "");
            _dictDtm.Add("Remark", "");
        }

        void IDraw.SwitchCtl(bool isSwitch)
        {
            switch (isSwitch)
            {
                case true:
                    btnSubmit.Enabled = true;
                    dpDept.Visibility = DockVisibility.Visible;
                    break;
                case false:
                    btnSubmit.Enabled = false;
                    dpDept.Visibility = DockVisibility.Hidden;
                    break;
            }
        }

        private void gvDepartment_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //获得光标位置
                var hi = gvDepartment.CalcHitInfo(e.Location);
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
                gvDepartment.SelectRow(hi.RowHandle);
                _dr = gvDepartment.GetDataRow(hi.RowHandle);
            }
        }

        private void gcDepartment_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //获得光标位置
            var hi = gvDepartment.CalcHitInfo(e.Location);
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
            gvDepartment.SelectRow(hi.RowHandle);
            _dr = gvDepartment.GetDataRow(hi.RowHandle);

            _currentOperate = DataOper.Modify;
            if (_dr != null)
            {
                _draw.SetDataToUI(_dr);
                _draw.SwitchCtl(true);
            }
        }
        private void ShieldRight()
        {
            ContextMenu emptyMenu = new ContextMenu();
            txtName.Properties.ContextMenu = emptyMenu;
            txtNumber.Properties.ContextMenu = emptyMenu;
            txtRemark.Properties.ContextMenu = emptyMenu;
        }

        private void txtName_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void cbMaster_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtNumber_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtRemark_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }
    }
    
}
