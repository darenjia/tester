using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using ProcessEngine;
using UltraANetT.Interface;

namespace UltraANetT.Module
{
    public partial class Employee : XtraUserControl,IDraw
    {

        private readonly IDraw _draw;
        private readonly ProcStore _store;
        private readonly ProcShow _show;
        private readonly ProcLog _log = new ProcLog();
        /// <summary>
        /// 字典类型的员工信息
        /// </summary>
        private Dictionary<string,object> _dictEly = new Dictionary<string, object>();
        /// <summary>
        /// 员工编号
        /// </summary>
        private int _elyNo = 1;
        /// <summary>
        /// 当前操作类别
        /// </summary>
        private DataOper _currentOperate;
        /// <summary>
        /// 定义一行数据
        /// </summary>
        private DataRow _dr;

        private DataTable _dt;
        private string role;
        private LogicalControl _LogC = new LogicalControl();
        private string oldElyName;
        private string oldElyRole;
        private string oldDepartment;
        public Employee()
        {
            _draw = this;
            _store = new ProcStore();
            _show = new ProcShow();
            InitializeComponent();
            _draw.InitDict();//初始化字典型的员工信息
            _draw.InitGrid();//初始化表格
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
                    cmsDept.Enabled = false;
                    
                    break;
                case "tester":
                    cmsDept.Enabled = false;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 从界面层抓取数据存入字典数据结构
        /// </summary>
        /// <returns></returns>
        Dictionary<string, object> IDraw.GetDataFromUI()
        {
            _dictEly["ElyNo"] = txtNo.Text;
            _dictEly["ElyName"] = txtName.Text;
            _dictEly["ElyRole"] = cbRole.Text;
            _dictEly["Department"] = cbDept.Text;
            _dictEly["Contact"] = txtTel.Text;
            _dictEly["Mail"] = txtMail.Text;
            _dictEly["Sex"] = cbSex.Text;
            _dictEly["Password"] = Md5(_dictEly["Password"].ToString());
            _dictEly["Remark"] = txtRemark.Text;
            return _dictEly;
        }

        void IDraw.InitDict()
        {
            _dictEly.Add("ElyNo", "");
            _dictEly.Add("ElyName", "");
            _dictEly.Add("ElyRole", "");
            _dictEly.Add("Department", "");
            _dictEly.Add("Sex", "");
            _dictEly.Add("Contact", "");
            _dictEly.Add("Mail", "");
            _dictEly.Add("Password", "666666");
            _dictEly.Add("Remark", "");
        }

        /// <summary>
        /// 绘制Grid表格，刷新数据
        /// </summary>
        void IDraw.InitGrid()
        {
            //把Grid控件中的列名称依次添加到List结构中
            var coList = new List<string>();
            foreach (GridColumn col in gvEmployee.Columns)
                coList.Add(col.FieldName);
            //从数据库中查询指定数据
            _dt = _show.DrawDtFromMultiple(coList.ToArray(), EnumLibrary.EnumTable.Employee);
            //将数据源赋值给控件
            gcEmployee.DataSource = _dt;
            //从数据中读取部门名称列的数据集合
            var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.Department, 0);
            //把集合赋值给控件
            if( cbDept.Properties.Items.Count > 0)
                cbDept.Properties.Items.Clear();
            cbDept.Properties.Items.AddRange(dept.ToArray());

        }

        /// <summary>
        /// 向界面层中赋值数据
        /// </summary>
        /// <param name="selectedRow">选中行的数据</param>
        void IDraw.SetDataToUI(DataRow selectedRow)
        {
            txtNo.Text = selectedRow["ElyNo"].ToString();
            txtName.Text = selectedRow["ElyName"].ToString();
            cbRole.Text = selectedRow["ElyRole"].ToString();
            cbDept.Text = selectedRow["Department"].ToString();
            txtTel.Text = selectedRow["Contact"].ToString();
            txtMail.Text = selectedRow["Mail"].ToString();
            cbSex.Text = selectedRow["Sex"].ToString();
            txtRemark.Text = selectedRow["Remark"].ToString();
        }
        /// <summary>
        /// 提交功能
        /// </summary>
        void IDraw.Submit()
        {
            
            switch (_currentOperate)
            {
                //添加
                case DataOper.Add://判断当前进行的哪种操作

                    string errorQ;//
                    _draw.GetDataFromUI();//从界面获得数据
                    FindSuperAdminister();
                    _store.AddEmployee(_dictEly, out errorQ);//添加到employee表中
                    _draw.SwitchCtl(true);//关闭右边添加信息的弹出框
                    _draw.InitGrid();//刷新表格
                    Dictionary<string, object> dictConfig = new Dictionary<string, object>
                        {
                            {"EmployeeNo",GlobalVar.UserNo},
                            {"EmployeeName",GlobalVar.UserName},
                            {"OperTable","员工"},
                            {"ElyName",   _dictEly["ElyName"]},
                            {"ElyRole",   _dictEly["ElyRole"]},
                            {"Department",   _dictEly["Department"]}
                        };
                    _log.WriteLog(EnumLibrary.EnumLog.AddEmployee, dictConfig);
                    break; 
                //更新
                case DataOper.Update:
                    _draw.GetDataFromUI();
                    FindSuperAdminister();
                    string errorU;
                    _store.Update(EnumLibrary.EnumTable.Employee, _dictEly, out errorU);
                    _draw.SwitchCtl(false);
                    _draw.InitGrid();
                    if(oldElyName  == _dictEly["ElyName"].ToString() && oldElyRole == _dictEly["ElyRole"].ToString() && oldDepartment == _dictEly["Department"].ToString())
                        break;
                    Dictionary<string, object> dictUConfig = new Dictionary<string, object>
                        {
                            {"EmployeeNo",GlobalVar.UserNo},
                            {"EmployeeName",GlobalVar.UserName},
                            {"OperTable","员工"},
                            {"oldElyName",   oldElyName},
                            {"oldRole",   oldElyRole},
                            {"oldDepartment",   oldDepartment},
                            {"newElyName",   _dictEly["ElyName"]},
                            {"newRole",   _dictEly["ElyRole"]},
                            {"newDepartment",   _dictEly["Department"]}
                        };
                    _log.WriteLog(EnumLibrary.EnumLog.UpdateEmployee, dictUConfig);
                    break;
                //删除
                case DataOper.Del:
                    string error; 
                    _store.Del(EnumLibrary.EnumTable.Employee, _dictEly, out error);
                    if (error == "")
                    {
                        _draw.InitGrid();
                        Dictionary<string, object> dictDConfig = new Dictionary<string, object>
                        {

                            {"EmployeeNo",GlobalVar.UserNo},
                            {"EmployeeName",GlobalVar.UserName},
                            {"OperTable","员工"},
                            {"ElyName",   _dictEly["ElyName"]},
                            {"ElyRole",   _dictEly["ElyRole"]},
                            {"Department",   _dictEly["Department"]}
                        };
                        _log.WriteLog(EnumLibrary.EnumLog.DelEmployee, dictDConfig);
                    }
                    break;
            }
        }

        private void FindSuperAdminister()
        {
            if (cbRole.Text.Trim() == "超级管理员")
            {
                foreach (DataRow row in _dt.Rows)
                {
                    if (row[2].ToString().Trim() == "超级管理员")
                    {
                        if (
                            XtraMessageBox.Show("人员表中只能有一个超级管理员，是否要修改超级管理员", "提示", MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Warning) ==
                            DialogResult.OK)
                        {
                            if (
                                XtraMessageBox.Show("人员表中只能有一个超级管理员，是否要修改超级管理员", "提示", MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Warning) ==
                                DialogResult.OK)
                            {
                                string error;
                                Dictionary<string, object> dictEly = new Dictionary<string, object>();
                                dictEly["ElyNo"] = row[0];
                                _store.Del(EnumLibrary.EnumTable.Employee, dictEly, out error);
                                if (error == "")
                                {
                                    _draw.InitGrid();
                                 }
                            }
                        }
                        
                    }
                }
            }
           
        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Add;
            _draw.SwitchCtl(true);
            _elyNo = gvEmployee.RowCount + 1;
            txtNo.Text = GetStrElyNo(_elyNo);
        }

        private void tsmiDel_Click(object sender, EventArgs e)
        {
            if (_dr == null)
                return;
            if (_dr["ElyRole"].ToString() == "超级管理员")
            {
                XtraMessageBox.Show("超级管理员不可删除！");
                return;
            }
            _currentOperate = DataOper.Del;
            _draw.Submit();
            _dr = null;
        }

        private void tsmiUpdate_Click(object sender, EventArgs e)
        {
            if (_dr != null)
            {
                if (_dr["ElyRole"].ToString() == "超级管理员")
                {
                    XtraMessageBox.Show("超级管理员不可修改！");
                    return;
                }
                _currentOperate = DataOper.Update;
                _draw.SwitchCtl(true);
                _draw.SetDataToUI(_dr);
                oldElyName = _dr["ElyName"].ToString();
                oldElyRole = _dr["ElyRole"].ToString();
                oldDepartment = _dr["Department"].ToString();
            }
        }

        /// <summary>
        /// 根据当前员工数量，为下一个员工生成员工编号
        /// </summary>
        /// <param name="elyNoInt">下一个员工的员工号</param>
        /// <returns></returns>
        private string GetStrElyNo(int elyNoInt)
        {
            string strElyNo = "";
            if (elyNoInt < 10)
                strElyNo = @"No.00" + elyNoInt;
            else if(elyNoInt > 10 && elyNoInt <100)
                strElyNo = @"No.0" + elyNoInt;
            return strElyNo;
        }

        private void btSubmit_Click(object sender, EventArgs e)
        {
            GlobalVar.NumberChanges = 0;
            _draw.Submit();
        }

        /// <summary>
        /// 转换指定控件的控件属性
        /// </summary>
        /// <param name="isSwitch">指定控件是否可用</param>
        void IDraw.SwitchCtl(bool isSwitch)
        {
            switch (isSwitch)
            {
                case true:
                    btSubmit.Enabled = true;
                    dpEmployee.Visibility = DockVisibility.Visible;
                    break;
                case false:
                    btSubmit.Enabled = false;
                    dpEmployee.Visibility = DockVisibility.Hidden;
                    break;
            }           
        }

        /// <summary>
        /// 操作行为枚举
        /// </summary>
        private enum DataOper
        {
            Add = 0,
            Update = 1,
            Del = 2
        }

        private void gvEmployee_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //获得光标位置
                var hi = gvEmployee.CalcHitInfo(e.Location);
                //判断位置是否在行位置上
                if (!hi.InRow && !hi.InRowCell) return;
                if (hi.RowHandle < 0) return;
                //取一行值
                gvEmployee.SelectRow(hi.RowHandle);
                _dr = gvEmployee.GetDataRow(hi.RowHandle);
                DrTodict(_dr);
            }
        }

        private void DrTodict(DataRow dr)
        {
            _dictEly["ElyNo"] = dr["ElyNo"];
            _dictEly["ElyName"] = dr["ElyName"];
            _dictEly["ElyRole"] = dr["ElyRole"];
            _dictEly["Department"] = dr["Department"];
            _dictEly["Contact"] = dr["Contact"];
            _dictEly["Mail"] = dr["Mail"];
            _dictEly["Sex"] = dr["Sex"];
            _dictEly["Remark"] = dr["Remark"];
        }

        #region MD5加密
        public static string Md5(string encryptString)
        {
            byte[] result = Encoding.Default.GetBytes(encryptString);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            string encryptResult = BitConverter.ToString(output).Replace("-", "");
            return encryptResult;
        }
        #endregion

        private void gcEmployee_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var hi = gvEmployee.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //获得选中行的数值
            int rowCount = hi.RowHandle;
            _dr = this.gvEmployee.GetDataRow(rowCount);
            _currentOperate = DataOper.Update;
            _draw.SwitchCtl(true);
            if (_dr != null)
                _draw.SetDataToUI(_dr);
        }
        private void ShieldRight()
        {
            ContextMenu emptyMenu = new ContextMenu();
            txtName.Properties.ContextMenu = emptyMenu;

            txtMail.Properties.ContextMenu = emptyMenu;

            txtNo.Properties.ContextMenu = emptyMenu;

            txtRemark.Properties.ContextMenu = emptyMenu;

            txtRemark.Properties.ContextMenu = emptyMenu;
        }

        private void txtName_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges =1;
        }

        private void cbRole_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void cbDept_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void cbSex_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtTel_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtMail_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtRemark_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }
    }
}