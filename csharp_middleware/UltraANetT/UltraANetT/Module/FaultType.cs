using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using UltraANetT.Interface;
using ProcessEngine;
using DevExpress.XtraBars.Docking;
using FileEditor.Form;
using DevExpress.XtraGrid.Columns;

namespace UltraANetT.Module
{
    public partial class FaultType : DevExpress.XtraEditors.XtraUserControl,IDraw
    {

        private LogicalControl _LogC = new LogicalControl();
        private string Role;
        private DataRow _dr;
        private DataTable _dt;
        private readonly IDraw _draw;
        private ProcLog Log = new ProcLog();
        private DataOper _currentOperate;
        private Dictionary<string, object> _dictFault = new Dictionary<string, object>();
        private readonly ProcStore _store = new ProcStore();
        private readonly ProcShow _show = new ProcShow();
        private Dictionary<string, object> _dictError = new Dictionary<string, object>();
        private int RowIndex = 0;
        private string oldTpye;
        public FaultType()
        {
            InitializeComponent();
            _draw = this;
            Role = _LogC.RoleSelect(GlobalVar.UserName);
            RoleFunction(Role);
            _draw.InitGrid();
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
                    break;
                case "tester":
                    CMSFault.Enabled = false;
                    dpFault.Visibility = DockVisibility.Hidden;
                    break;
                default:
                    break;
            }
        }
        private enum DataOper
        {
            Add = 0,
            Update = 1,
            Del = 2
        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            _draw.SwitchCtl(true);
            _currentOperate = DataOper.Add;
        }

        private void tsmiModify_Click(object sender, EventArgs e)
        {
            if (_dr == null)
                return;
            _draw.SwitchCtl(true);
            _draw.SetDataToUI(_dr);
            _currentOperate = DataOper.Update;
            oldTpye = txtType.Text;
        }

        private void Del_Click(object sender, EventArgs e)
        {
            if (_dr == null)
                return;
            _currentOperate = DataOper.Del;
            _draw.Submit();
            _dr = null;
        }

        private void gvFault_MouseDown(object sender, MouseEventArgs e)
        {
            //获得光标位置
            var hi = gvFault.CalcHitInfo(e.Location);
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
            RowIndex = hi.RowHandle;
            //取一行值
            gvFault.SelectRow(hi.RowHandle);
            _dr = this.gvFault.GetDataRow(hi.RowHandle);
        }

        void IDraw.InitGrid()
        {
            //把Grid控件中的列名称依次添加到List结构中
            var coList = new List<string>();
            foreach (GridColumn col in gvFault.Columns)
                coList.Add(col.FieldName);
            //从数据库中查询指定数据
            _dt = _show.DrawDtFromMultiple(coList.ToArray(), EnumLibrary.EnumTable.FaultType);
            //将数据源赋值给控件
            gcFault.DataSource = _dt;
            //从数据中读取部门名称列的数据集合
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
                if (_currentOperate == DataOper.Update)
                {
                    if (RowIndex == j)
                        continue;
                }
                primary = false;
                if (_dt.Rows[j]["ErrorType"].ToString() == txtType.Text)
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
                if (_currentOperate == DataOper.Update)
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
                if (_currentOperate == DataOper.Update)
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

        void IDraw.Submit()
        {
           
            if(!PrimaryKey())
                return;
            if (_currentOperate != DataOper.Del)
            {
                if (string.IsNullOrEmpty(txtCount.Text.Trim()))
                {
                    XtraMessageBox.Show("请输入DTC报文项数，没有项数的情况下请输入0");
                    return;
                }
                if (string.IsNullOrEmpty(txtType.Text.Trim()))
                {
                    XtraMessageBox.Show("请输入故障类型...");
                    return;
                }
            }
            var error = "";
            switch (_currentOperate)
            {
                case DataOper.Add:
                    if (ceIMessage.Checked)
                    {
                        if (int.Parse(txtCount.Text) <= 0)
                        {
                            MessageBox.Show("请输入大0的整数");
                            return;
                        }
                    }
                    else
                    {
                        if (int.Parse(txtCount.Text) > 0)
                        {
                            MessageBox.Show("此故障类型没有DTC报文项，请输入0");
                            return;
                        }
                    }
                    _draw.GetDataFromUI();
                    _dictFault["MsgInformation"] = "";
                    _store.AddFaultType(_dictFault, out error);
                    _draw.InitGrid();
                    ClearDataToUI();
                    _draw.SwitchCtl(true);
                    //if (error == "")
                    //{
                    //    Log.WriteLog(EnumLibrary.EnumLog.AddTask, UserDict());
                    //    Log.WriteLog(EnumLibrary.EnumLog.AddTasker, UserDict());
                    //}
                    //NodeAddStr = "";
                    break;
                case DataOper.Update:
                    if (ceIMessage.Checked)
                    {
                        if (int.Parse(txtCount.Text) <= 0)
                        {
                            MessageBox.Show("请输入大0的整数");
                            return;
                        }
                    }
                    else
                    {
                        if (int.Parse(txtCount.Text) >0)
                        {
                            MessageBox.Show("此故障类型没有DTC报文项，请输入0");
                            return;
                        }
                    }
                    _draw.GetDataFromUI();
                    _dictFault["MsgInformation"] = _dr["MsgInformation"];
                    _store.Update(EnumLibrary.EnumTable.FaultType, _dictFault, out error);
                    _draw.InitGrid();
                    ClearDataToUI();
                    _draw.SwitchCtl(false);
                    break;
                case DataOper.Del:
                    if (
                        XtraMessageBox.Show("是否删除此故障类型？", "提示", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning) ==
                        DialogResult.OK)
                    {
                        bool su = _store.Del(EnumLibrary.EnumTable.FaultType, UserDr(false), out error);
                        _draw.InitGrid();
                    }
                    break;
            }
            
        }
        private Dictionary<string, object> UserDict()
        {
            _dictFault["EmployeeName"] = GlobalVar.UserName;
            _dictFault["EmployeeNo"] = GlobalVar.UserNo;
            _dictFault["Department"] = GlobalVar.UserDept;
            return _dictFault;
        }
        private Dictionary<string, object> UserDr(bool su)
        {
            UserDict();
            _dictFault["oldErrorType"] = _dr["ErrorType"];
            _dictFault["ErrorType"] = _dr["ErrorType"];
            _dictFault["IsMessage"] = _dr["IsMessage"];
            _dictFault["MessageCount"] = _dr["MessageCount"];
            _dictFault["MsgInformation"] = _dr["MsgInformation"];
            _dictFault["CheckInfor"] = _dr["CheckInfor"];
            return _dictFault;
        }
        Dictionary<string, object> IDraw.GetDataFromUI()
        {
            _dictFault["ErrorType"] = txtType.Text;
            _dictFault["IsMessage"] = ceIMessage.Checked;
            _dictFault["MessageCount"] = txtCount.Text; 
            _dictFault["CheckInfor"] = "请右键查看该故障的详细信息";
            _dictFault["oldErrorType"] = oldTpye;
            return _dictFault;
        }

        void IDraw.SetDataToUI(DataRow selectedRow)
        {
            if(_dr==null)
                return;
            txtType.Text = _dr["ErrorType"].ToString();
            oldTpye = _dr["ErrorType"].ToString();
            if (_dr["IsMessage"].ToString() == "True")
                ceIMessage.Checked = true;
            else if(_dr["IsMessage"].ToString() == "Frue")
                ceIMessage.Checked = false;
            txtCount.Text = _dr["MessageCount"].ToString();
        }

        void IDraw.InitDict()
        {
            _dictError.Add("ErrorType", "");
            _dictError.Add("IsMessage", "");
            _dictError.Add("MessageCount", "");
            _dictError.Add("MsgInformation", "");
            _dictError.Add("CheckInfor", "");
        }

        void IDraw.SwitchCtl(bool isSwitch)
        {
            switch (isSwitch)
            {
                case true:
                    btnOK.Enabled = true;
                    dpFault.Visibility = DockVisibility.Visible;
                    if (!ceIMessage.Checked)
                        txtCount.Text = 0.ToString(); 
                    break;
                case false:
                    btnOK.Enabled = false;
                    dpFault.Visibility = DockVisibility.Hidden;
                    ClearDataToUI();
                    break;
            }
        }
        private void ClearDataToUI()
        {
            txtType.Text = null;
            txtType.Text = null;
            ceIMessage.Checked = false;
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            GlobalVar.NumberChanges = 0;
            _draw.Submit();
        }

        private void tsmiCheck_Click(object sender, EventArgs e)
        {
            if (_dr == null)
            {
                MessageBox.Show("请选中一行后再查看...");
                return;
            }
            if (int.Parse(_dr["MessageCount"].ToString()) <= 0)
            {
                MessageBox.Show("改故障类型的DTC相关报文项为 0 项，不需要添加相关信息，如需添加请返回修改报文项数...");
                return;

            }
            FaultTypeCount ftpCount = new FaultTypeCount(_dr);
            string strOldJson = _dr[3].ToString();//保存进入之前的json
            ftpCount.ShowDialog();
            string messType = "";
            bool count = false;
            messType = ftpCount.SaveJson(out count);
            if (strOldJson.Equals(messType))//如果和保存之前的json一样，则不保存进数据库
                count = true;
            if (count)
                return;
            string error;
            if (messType != "")
            {
                _dr["MsgInformation"] = messType;
                _store.Update(EnumLibrary.EnumTable.FaultType, UserDr(false), out error);
            }
        }

        private void gcFault_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var hi = gvFault.CalcHitInfo(e.Location);
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
            //获得选中行的数值
            int rowCount = hi.RowHandle;
            _dr = this.gvFault.GetDataRow(rowCount);
            if (_dr == null)
                return;
            _draw.SwitchCtl(true);
            _draw.SetDataToUI(_dr);
            _currentOperate = DataOper.Update;
        }

        private void ceIMessage_CheckedChanged(object sender, EventArgs e)
        {
            if (ceIMessage.Checked)
                txtCount.Text = "";
            else
            {
                txtCount.Text = 0.ToString();
            }
        }
        private void ShieldRight()
        {
            ContextMenu emptyMenu = new ContextMenu();
            txtCount.Properties.ContextMenu = emptyMenu;
            txtType.Properties.ContextMenu = emptyMenu;
        }

        private void txtType_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtCount_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }
    }
}
