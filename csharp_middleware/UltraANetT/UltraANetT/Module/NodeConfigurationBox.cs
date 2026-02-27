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
    public partial class NodeConfigurationBox : XtraUserControl,IDraw
    {
        #region 设置变量
        
        private readonly ProcShow _show = new ProcShow();

        private readonly ProcStore _store = new ProcStore();

        private readonly IDraw _draw;

        private readonly ProcLog _log = new ProcLog();
        private string oldName;
        private string oldID;
        private string oldCount;
        /// <summary>
        /// 字典类型的员工信息
        /// </summary>
        private Dictionary<string, object> _dictNodeConfigurationBox = new Dictionary<string, object>();
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
        private bool BoolHex = false;
        private int RowIndex = 0;
        #endregion


        public NodeConfigurationBox()
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

        private void gcNodeConfigurationBox_DoubleClick(object sender, EventArgs e)
        {
            //获得所点击的控件
            var control = sender as System.Windows.Forms.Control;
            if (control == null) return;
            //获得光标位置
            var hi = gvNodeConfigurationBox.CalcHitInfo(control.PointToClient(MousePosition));
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //取值一行值
            gvNodeConfigurationBox.SelectRow(hi.RowHandle);
            var selectedRow = this.gvNodeConfigurationBox.GetDataRow(this.gvNodeConfigurationBox.FocusedRowHandle);
            //赋值
            _draw.SetDataToUI(selectedRow);
            //双击某一行也可以修改
            _draw.SwitchCtl(true);

            _currentOperate = DataOper.Modify;
        }

        
        private void gvNodeConfigurationBox_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                tsmiDel.Enabled = true;
                //_deptName = gvNodeConfigurationBox.GetRowCellValue(e.RowHandle, gvNodeConfigurationBox.Columns["NodeConfigurationBoxName"]).ToString();

            }
        }

        void IDraw.InitGrid()
        {
            var coList = new List<string>();
            foreach (GridColumn col in gvNodeConfigurationBox.Columns)
                coList.Add(col.FieldName);
            _dt = _show.DrawDtFromMultiple(coList.ToArray(), EnumLibrary.EnumTable.NodeConfigurationBox);
            gcNodeConfigurationBox.DataSource = _dt;
        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Add;
            _draw.SwitchCtl(true);
        }

        private void tsmiModify_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Modify;
            if (_dr != null)
            {
                _dictNodeConfigurationBox["OldName"] = _dr["Name"];
                _dictNodeConfigurationBox["OldID"] = _dr["ID"];
                _dictNodeConfigurationBox["OldCount"] = _dr["Count"];
                _draw.SetDataToUI(_dr);
                _draw.SwitchCtl(true);
                oldID = _dr["ID"].ToString();
                oldName = _dr["Name"].ToString();
                oldCount = _dr["Count"].ToString();
            }
        }

        private void tsmiDel_Click(object sender, EventArgs e)
        {
            if (_dr == null)
                return;
            _currentOperate = DataOper.Del;
            _draw.Submit();
            _dr = null;
        }

        /// <summary>
        /// 限制datatable里不能添加重复项
        /// </summary>
        /// <returns>false为有重复项</returns>
        private bool PrimaryKey()
        {
            string IDhex = "";
            ifHex(txtID.Text, out IDhex);

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
                if (_dt.Rows[j]["Name"].ToString() == txtName.Text&& _dt.Rows[j]["ID"].ToString() == IDhex)
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
            bool ifNull = true;
            if (string.IsNullOrEmpty(txtName.Text))
            {
                ifNull = false;
            }
            else if (string.IsNullOrEmpty(txtID.Text))
            {
                ifNull = false;
            }
            else if (string.IsNullOrEmpty(txtCount.Text))
            {
                ifNull = false;
            }
            if(!ifNull)
            {
                XtraMessageBox.Show("请完整填写信息，不要为空。");
            }

            return ifNull;
        }

        private bool ifInt()
        {
            int i;
            bool ifint = int.TryParse(txtCount.Text.Trim(), out i);
            if (!ifint)
            {
                XtraMessageBox.Show("配置盒IO数量项请填写十进制整数。");
            }
            return ifint;
        }
        void IDraw.Submit()
        {
            if (_currentOperate != DataOper.Del)
            {
                if (!ifNull())
                    return;
                _draw.GetDataFromUI();
                if (!BoolHex)
                    return;
                if (!ifInt())
                    return;
                if (!PrimaryKey())
                    return;
            }
            
            switch (_currentOperate)
            {
                //添加
                case DataOper.Add:
                    string errorQ;
                    _store.AddNodeConfigurationBox(_dictNodeConfigurationBox, out errorQ);
                    if (errorQ == "")
                    {
                        _draw.InitGrid();
                        ClearDataToUi();
                        _draw.SwitchCtl(true);
                        Dictionary<string, object> dictAConfig = new Dictionary<string, object>
                        {
                            {"EmployeeNo",GlobalVar.UserNo},
                             {"EmployeeName",GlobalVar.UserName},
                             {"OperTable","节点配置盒"},
                            {"Name",    _dictNodeConfigurationBox["Name"] },
                            {"ID",    _dictNodeConfigurationBox["ID"] },
                            {"Count",    _dictNodeConfigurationBox["Count"] },
                        };
                        _log.WriteLog(EnumLibrary.EnumLog.AddCfgBox, dictAConfig);
                    }
                    else
                    {
                        XtraMessageBox.Show("请不要添加相同编号的配置盒");
                    }

                    break;
                //更新
                case DataOper.Modify:
                    string errorU;
                    _store.Update(EnumLibrary.EnumTable.NodeConfigurationBox, _dictNodeConfigurationBox, out errorU);
                    if (errorU == "")
                    {
                        ClearDataToUi();
                        _draw.InitGrid();
                        _draw.SwitchCtl(false);
                        Dictionary<string, object> dictAConfig = new Dictionary<string, object>
                        {
                             {"EmployeeName",GlobalVar.UserName},
                             {"EmployeeNo",GlobalVar.UserNo},
                             {"OperTable","节点配置盒"},
                            {"oldName",  oldName},
                            {"oldID",  oldID},
                            {"oldCount",    oldCount },
                            {"newName",    _dictNodeConfigurationBox["Name"] },
                            {"newID",    _dictNodeConfigurationBox["ID"] },
                            {"newCount",    _dictNodeConfigurationBox["Count"] },
                        };
                        _log.WriteLog(EnumLibrary.EnumLog.UpdateCfgBox, dictAConfig);
                    }
                    else
                    {
                        XtraMessageBox.Show("请不要添加相同编号的配置盒...");
                    }
                    break;
                //删除
                case DataOper.Del:
                    if (
                        XtraMessageBox.Show("是否删除此配置盒？", "提示", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning) ==
                        DialogResult.OK)
                    {
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict["Name"] = _dr["Name"];
                        dict["ID"] = _dr["ID"];
                        dict["Count"] = _dr["Count"];
                        string error;
                        _store.Del(EnumLibrary.EnumTable.NodeConfigurationBox, dict, out error);
                        if (error == "")
                        {
                            _draw.InitGrid();
                            tsmiDel.Enabled = false;
                            Dictionary<string, object> dictAConfig = new Dictionary<string, object>
                            {
                                {"EmployeeName", GlobalVar.UserName},
                                {"EmployeeNo", GlobalVar.UserNo},
                                {"OperTable", "节点配置盒"},
                                {"Name", _dictNodeConfigurationBox["Name"]},
                                {"ID", _dictNodeConfigurationBox["ID"]},
                                {"Count", _dictNodeConfigurationBox["Count"]},
                            };
                            _log.WriteLog(EnumLibrary.EnumLog.DelCfgBox, dictAConfig);
                        }
                        else
                        {
                            XtraMessageBox.Show("删除操作出错，请联系工程师帮助解决");
                        }
                    }
                    break;
            }
        }
        /// <summary>
        /// 判断所输入字符串是否为16进制
        /// </summary>
        /// <param name="HexValue">所输入的16进制值</param>
        private bool ifHex(string HexValue,out string Hex)
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

        void ClearDataToUi()
        {
            txtName.Text = null;
            txtID.Text = null;
            txtCount.Text = null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Dictionary<string, object> IDraw.GetDataFromUI()
        {
            _dictNodeConfigurationBox["Name"] = txtName.Text;
            string ID;
            if(ifHex(txtID.Text, out ID))
            {
                _dictNodeConfigurationBox["ID"] = ID;
                BoolHex = true;
            }
            else
            {
                BoolHex = false;
                XtraMessageBox.Show("请输入16进制数值！", "提示", MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
            }
            _dictNodeConfigurationBox["Count"] = txtCount.Text;
            return _dictNodeConfigurationBox;
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
            txtID.Text = selectedRow["ID"].ToString();
            txtCount.Text = selectedRow["Count"].ToString();
        }

        void IDraw.InitDict()
        {
            _dictNodeConfigurationBox.Add("Name", "");
            _dictNodeConfigurationBox.Add("ID", "");
            _dictNodeConfigurationBox.Add("Count", "");

            _dictNodeConfigurationBox.Add("OldName", "");
            _dictNodeConfigurationBox.Add("OldID", "");
            _dictNodeConfigurationBox.Add("OldCount", "");
        }

        void IDraw.SwitchCtl(bool isSwitch)
        {
            switch (isSwitch)
            {
                case true:
                    btnSubmit.Enabled = true;
                    dpNodeConfigurationBox.Visibility = DockVisibility.Visible;
                    break;
                case false:
                    btnSubmit.Enabled = false;
                    dpNodeConfigurationBox.Visibility = DockVisibility.Hidden;
                    break;
            }
        }

        private void gvNodeConfigurationBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //获得光标位置
                var hi = gvNodeConfigurationBox.CalcHitInfo(e.Location);
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
                RowIndex = hi.RowHandle;
                gvNodeConfigurationBox.SelectRow(hi.RowHandle);
                _dr = gvNodeConfigurationBox.GetDataRow(hi.RowHandle);
            }
        }

        private void gcNodeConfigurationBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var hi = gvNodeConfigurationBox.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //获得选中行的数值
            int rowCount = hi.RowHandle;
            _dr = this.gvNodeConfigurationBox.GetDataRow(rowCount);
            _currentOperate = DataOper.Modify;
            if (_dr != null)
            {
                _dictNodeConfigurationBox["OldName"] = _dr["Name"];
                _dictNodeConfigurationBox["OldID"] = _dr["ID"];
                _dictNodeConfigurationBox["OldCount"] = _dr["Count"];
                _draw.SetDataToUI(_dr);
                _draw.SwitchCtl(true);
            }
        }
        private void ShieldRight()
        {
            ContextMenu emptyMenu = new ContextMenu();
            txtName.Properties.ContextMenu = emptyMenu;
            txtCount.Properties.ContextMenu = emptyMenu;
            txtID.Properties.ContextMenu = emptyMenu;
        }

        private void txtName_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtID_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtCount_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }
    }
}
