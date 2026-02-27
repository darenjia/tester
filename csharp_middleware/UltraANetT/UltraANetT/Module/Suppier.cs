using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using ProcessEngine;
using UltraANetT.Form;
using UltraANetT.Interface;

namespace UltraANetT.Module
{
    public partial class Suppier : DevExpress.XtraEditors.XtraUserControl,IDraw
    {
        #region 设置变量
        private readonly ProcShow _show = new ProcShow();

        private readonly ProcStore _store = new ProcStore();

        private readonly IDraw _draw;
        private List<string> drList = new List<string>();
        private readonly ProcLog _log = new ProcLog();
        private string oldSuppier = "";
        private string oldType = "";
        private string oldMoudle = "";
        private int targetIndex = 0;
        /// <summary>
        /// 字典类型的员工信息
        /// </summary>
        private Dictionary<string, object> _dictSup = new Dictionary<string, object>();
        /// <summary>
        /// 当前操作类别
        /// </summary>
        private DataOper _currentOperate;
        /// <summary>
        /// 定义一行数据
        /// </summary>
        private DataRow _dr;
        /// <summary>
        /// 操作记录
        /// </summary>

        private DataRow oldDr = null;

        #endregion
        public Suppier()
        {
            InitializeComponent();
            _draw = this;
            _draw.InitGrid();
            _draw.InitDict();
            ShieldRight();
        }

        private enum DataOper
        {
            Add = 0,
            Update = 1,
            Del = 2
        }

        void ClearDataToUI()
        {
            txtModule.Text = null;
            txtType.Text = null;
            txtSupplier.Text = null;
            txtContact.Text = null;
            txtTel.Text = null;
        }

        Dictionary<string, object> IDraw.GetDataFromUI()
        {
            _dictSup["Module"] = txtModule.Text;
            _dictSup["Type"] = txtType.Text;
            _dictSup["Supplier"] = txtSupplier.Text;
            _dictSup["Contact"] = txtContact.Text;
            _dictSup["Tel"] = txtTel.Text;
            return _dictSup;
        }

        void IDraw.InitDict()
        {
            _dictSup.Add("Module", "");
            _dictSup.Add("Type", "");
            _dictSup.Add("Supplier", "");
            _dictSup.Add("Contact", "");
            _dictSup.Add("Tel", "");
        }

        private void ListConvertToDict()
        {
            _dictSup["Module"] = drList[0];
            _dictSup["Type"] = drList[1];
            _dictSup["Supplier"] = drList[2];
            _dictSup["Contact"] = drList[3];
            _dictSup["Tel"] = drList[4];
        }

        private void DrConvertToDict()
        {
            _dictSup["Module"] = _dr["Module"];
            _dictSup["Type"] = _dr["Type"];
            _dictSup["Supplier"] = _dr["Supplier"];
            _dictSup["Contact"] = _dr["Contact"];
            _dictSup["Tel"] = _dr["Tel"];
        }

        void IDraw.InitGrid()
        {
            var coList = new List<string>();
            foreach (GridColumn col in gvSupplier.Columns)
                coList.Add(col.FieldName);
            var dt = _show.DrawDtFromMultiple(coList.ToArray(), EnumLibrary.EnumTable.Suppliers);
            gcSupplier.DataSource = dt;
        }

        void IDraw.SetDataToUI(DataRow selectedRow)
        {
            txtModule.Text = selectedRow["Module"].ToString();
            txtType.Text = selectedRow["Type"].ToString();
            txtSupplier.Text = selectedRow["Supplier"].ToString();
            txtContact.Text = selectedRow["Contact"].ToString();
            txtTel.Text = selectedRow["Tel"].ToString();

            //获取修改前的值用作修改时的判断
            _dictSup["OldModule"] = selectedRow["Module"];
            _dictSup["OldType"] = selectedRow["Type"];
            _dictSup["OldSupplier"] = selectedRow["Supplier"];
        }

        /// <summary>
        /// 添加员工操作记录时 如果记录是Dictionary类型可直接调用这个方法
        /// </summary>
        /// <param name="Oper">操作类型</param>
        /// <returns></returns>
        private Dictionary<string, object> UserDict(DataOper Oper)
        {
            _dictSup["EmployeeName"] = GlobalVar.UserName;
            _dictSup["EmployeeNo"] = GlobalVar.UserNo;
            _dictSup["Department"] = GlobalVar.UserDept;
            _dictSup["Oper"] = Oper.ToString();
            return _dictSup;
        }

        /// <summary>
        /// 添加员工操作记录时 如果记录是DataRow类型则调用这个方法
        /// </summary>
        /// <param name="Oper">操作类型</param>
        /// <returns></returns>
        private Dictionary<string, object> UserDr(DataOper Oper)
        {
            UserDict(Oper);
            _dictSup["Module"] = _dr["Module"];
            _dictSup["Type"] = _dr["Type"];
            _dictSup["Supplier"] = _dr["Supplier"];
            _dictSup["Contact"] = _dr["Contact"];
            _dictSup["Tel"] = _dr["Tel"];
            return _dictSup;
        }

        void IDraw.Submit()
        {
            string errorua = "";
            switch (_currentOperate)
            {
                //添加
                case DataOper.Add:
                    _draw.GetDataFromUI();
                    if (_dictSup["Module"].ToString() == "" || _dictSup["Type"].ToString() == "" ||
                        _dictSup["Supplier"].ToString() == "")
                    {
                        XtraMessageBox.Show("添加信息不能为空", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        _draw.SwitchCtl(false);
                        return;
                    }
                    _store.AddSuppliers(_dictSup, out errorua);
                    _draw.SwitchCtl(false);
                    if (errorua == "")
                    {
                        Dictionary<string, object> dictAConfig = new Dictionary<string, object>
                        {
                            {"EmployeeNo",GlobalVar.UserNo},
                            {"EmployeeName",GlobalVar.UserName},
                            {"Suppier",   _dictSup["Supplier"]},
                             {"OperTable","供应商"},
                            {"Type",   _dictSup["Type"]},
                            {"Moudle",   _dictSup["Module"]}
                        };
                        _log.WriteLog(EnumLibrary.EnumLog.AddSuppiers, dictAConfig);
                    }
                    else
                    {
                        XtraMessageBox.Show("已经存在重复项，请重新上传...", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    _draw.InitGrid();
                    break;
                //更新
                case DataOper.Update:
                    _draw.GetDataFromUI();
                    if (IsSaveModify())
                    {
                        _store.Update(EnumLibrary.EnumTable.Suppliers, _dictSup, out errorua);
                    }
                    drList.Clear();
                    oldDr = null;
                    _draw.SwitchCtl(false);
                    if (errorua == "")
                    {
                        Dictionary<string, object> dictAConfig = new Dictionary<string, object>
                        {
                            {"EmployeeNo",GlobalVar.UserNo},
                            {"EmployeeName",GlobalVar.UserName},
                             {"OperTable","供应商"},
                            {"oldSuppier",  oldSuppier},
                            {"oldType",   oldType},
                            {"oldMoudle", oldMoudle},
                            {"newSuppier",   _dictSup["Supplier"]},
                            {"newType",   _dictSup["Type"]},
                            {"newMoudle",   _dictSup["Module"]}
                        };
                        _log.WriteLog(EnumLibrary.EnumLog.UpdateSuppiers, dictAConfig);
                        
                    }
                    else
                    {
                        XtraMessageBox.Show("已经存在重复项，请重新上传...", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    _draw.InitGrid();
                    break;
                //删除
                case DataOper.Del:
                    string error;
                    if (_dr != null)
                    {
                        DrConvertToDict();
                        if (XtraMessageBox.Show("确定要删除设备模块'" + _dictSup["Module"] + "'么？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                        {
                            _store.Del(EnumLibrary.EnumTable.Suppliers, _dictSup, out error);
                            if (error == "")
                            {
                                Dictionary<string, object> dictAConfig = new Dictionary<string, object>
                                {
                                    {"EmployeeNo",GlobalVar.UserNo},
                                    {"EmployeeName",GlobalVar.UserName},
                                    {"OperTable","供应商"},
                                    {"Suppier",   _dictSup["Supplier"]},
                                    {"Type",   _dictSup["Type"]},
                                    {"Moudle",   _dictSup["Module"]}
                                 };
                                _log.WriteLog(EnumLibrary.EnumLog.DelSuppiers, dictAConfig);
                            }
                            else
                            {
                                XtraMessageBox.Show(error, "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            _draw.InitGrid();
                        }
                    }
                    else
                    {
                        XtraMessageBox.Show("没有选中行", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    break;
            }
        }

        void IDraw.SwitchCtl(bool isSwitch)
        {
            switch (isSwitch)
            {
                case true:
                    btnSubmit.Enabled = true;
                    break;
                case false:
                    btnSubmit.Enabled = false;
                    break;
            }
        }



        private bool IsSaveModify()
        {
            bool isModify = false;
            {

                int n = 0;
                if (drList.Count != 0)
                {
                    List<string> uiText = new List<string>();
                    uiText = GetListFromUI();
                    foreach (var dr in uiText)
                    {
                        if (dr != drList[n] && btnSubmit.Enabled)
                        {
                            if (
                                XtraMessageBox.Show("是否需要保存修改...？", "提示", MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Warning) ==
                                DialogResult.OK)
                            {
                                //ListConvertToDict();
                                _draw.SwitchCtl(false);
                                isModify = true;
                            }
                            else
                            {
                                break;
                            }
                        }
                        if(isModify)
                            break;
                        n++;
                    }

                }
            }
            return isModify;
        }

        private List<string> GetListFromUI()
        {
            List<string> uiList = new List<string>();
            uiList.Add(txtModule.Text.Trim());
            uiList.Add(txtType.Text.Trim());
            uiList.Add(txtSupplier.Text.Trim());
            uiList.Add(txtContact.Text.Trim());
            uiList.Add(txtTel.Text.Trim());
            return uiList;
        }








        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Add;
            _draw.SwitchCtl(true);
            ClearDataToUI();
        }

        private void gcSupplier_MouseDown(object sender, MouseEventArgs e)
        {
            var hi = gvSupplier.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //获得选中行的数值
            int rowCount = hi.RowHandle;
            _dr = this.gvSupplier.GetDataRow(rowCount);
            _currentOperate = DataOper.Update;
            if (_dr != null)
            {
                _draw.SetDataToUI(_dr);
                _draw.SwitchCtl(true);
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            GlobalVar.NumberChanges = 0;
            _draw.Submit();
            ClearDataToUI();
        }

        private void tsmiDel_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Del;
            _draw.Submit();
            _dr = null;
        }

        private void gcSupplier_DoubleClick(object sender, EventArgs e)
        {
            var control = sender as System.Windows.Forms.Control;
            if (control == null) return;
            //获得光标位置
            var hi = gvSupplier.CalcHitInfo(control.PointToClient(MousePosition));
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //取值一行值
            gvSupplier.SelectRow(hi.RowHandle);
            var selectedRow = this.gvSupplier.GetDataRow(hi.RowHandle);
            //赋值
            _draw.SetDataToUI(selectedRow);
            _draw.SwitchCtl(true);
            _currentOperate = DataOper.Update;
        }

        private void gvSupplier_MouseDown(object sender, MouseEventArgs e)
        {
            {
                //获得光标位置
                var hi = gvSupplier.CalcHitInfo(e.Location);
                //判断位置是否在行位置上
                if (!hi.InRow && !hi.InRowCell) return;
                if (hi.RowHandle < 0) return;
                //取一行值
                gvSupplier.SelectRow(hi.RowHandle);
                _dr = this.gvSupplier.GetDataRow(hi.RowHandle);
 
                if (oldDr != null)
                {
                    drList.Clear();
                    drList.Add(oldDr.ItemArray[0].ToString());
                    drList.Add(oldDr.ItemArray[1].ToString());
                    drList.Add(oldDr.ItemArray[2].ToString());
                    drList.Add(oldDr.ItemArray[3].ToString());
                    drList.Add(oldDr.ItemArray[4].ToString());
                }

                if (_dr != null)
                {
                    this.tsmiDel.Enabled = true;
                }
                else
                {
                    this.tsmiDel.Enabled = false;
                }
                if (e.Clicks == 2)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        _draw.SetDataToUI(_dr);
                        _draw.SwitchCtl(true);
                    }
                    

                }

                

                //if (e.Clicks == 1)
                //{
                //    if (e.Button == MouseButtons.Left)
                //    {
                //        _draw.SetDataToUI(_dr);
                //        _draw.SwitchCtl(false);
                //    }
                //}
                oldDr = _dr;
            }
        }

        private void tsmiUpdate_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Update;
            if (_dr != null)
            {
                _draw.SetDataToUI(_dr);
                _draw.SwitchCtl(true);
                oldSuppier = _dr["Supplier"].ToString();
                oldType = _dr["Type"].ToString();
                oldMoudle = _dr["Module"].ToString();

            }
        }
        private void ShieldRight()
        {
            ContextMenu emptyMenu = new ContextMenu();
            txtContact.Properties.ContextMenu = emptyMenu;
            txtModule.Properties.ContextMenu = emptyMenu;
            txtSupplier.Properties.ContextMenu = emptyMenu;
            txtTel.Properties.ContextMenu = emptyMenu;
            txtType.Properties.ContextMenu = emptyMenu;
        }

        private void txtModule_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtType_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtSupplier_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtContact_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtTel_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }
    }
}
