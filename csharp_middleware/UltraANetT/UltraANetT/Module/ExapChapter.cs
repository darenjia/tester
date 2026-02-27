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
    public partial class ExapChapter : XtraUserControl,IDraw
    {
        #region 设置变量
        
        private readonly ProcShow _show = new ProcShow();

        private readonly ProcStore _store = new ProcStore();

        private readonly IDraw _draw;

        private ProcLog _log = new ProcLog();
        private string oldChapterName;
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


        public ExapChapter()
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

        private void gcExapChapter_DoubleClick(object sender, EventArgs e)
        {
            //获得所点击的控件
            var control = sender as System.Windows.Forms.Control;
            if (control == null) return;
            //获得光标位置
            var hi = gvExapChapter.CalcHitInfo(control.PointToClient(MousePosition));
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //取值一行值
            gvExapChapter.SelectRow(hi.RowHandle);
            var selectedRow = this.gvExapChapter.GetDataRow(this.gvExapChapter.FocusedRowHandle);
            //赋值
            _draw.SetDataToUI(selectedRow);
            ////双击某一行也可以修改
            //_draw.SwitchCtl(true);

            //_currentOperate = DataOper.Modify;
        }

        
        private void gvExapChapter_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
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
            foreach (GridColumn col in gvExapChapter.Columns)
                coList.Add(col.FieldName);

            _dt = _show.DrawDtFromExapChapter(coList.ToArray(), EnumLibrary.EnumTable.ExapChapter);
            gcExapChapter.DataSource = _dt;

            IList<object[]> list = _store.GetRegularByEnum(EnumLibrary.EnumTable.ExampleTemp);
            com_TestType.Properties.Items.Clear();
            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    String name = list[i][0].ToString().Trim().Replace("用例表", "").Trim(); ;
                    com_TestType.Properties.Items.Add(name);
                }
                com_TestType.SelectedItem = list[0][0].ToString().Trim().Replace("用例表", "").ToString().Trim();
            }
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
                _dictNodeConfigurationBox["OldChapterName"] = _dr["ChapterName"];
                _dictNodeConfigurationBox["OldTestType"] = _dr["TestType"];
                com_TestType.SelectedItem = _dr["TestType"].ToString().Trim();
                _draw.SetDataToUI(_dr);
                _draw.SwitchCtl(true);
                oldChapterName = _dr["ChapterName"].ToString();
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
                if (_dt.Rows[j]["ChapterName"].ToString() == txtName.Text && _dt.Rows[j]["TestType"].ToString()== com_TestType.SelectedItem.ToString().Trim())
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
            if (string.IsNullOrEmpty(txtName.Text) || txtName.Text.ToString().Trim()=="")
            {
                ifNull = false;
            }
            if(!ifNull)
            {
                XtraMessageBox.Show("请完整填写信息，不要为空。");
            }

            return ifNull;
        }


        void IDraw.Submit()
        {
            if (_currentOperate != DataOper.Del)
            {
                if (!ifNull())
                    return;
                _draw.GetDataFromUI();
                if (!PrimaryKey())
                    return;
            }
            _dictNodeConfigurationBox["TestType"] = com_TestType.SelectedItem.ToString().Trim();
            _dictNodeConfigurationBox["ChapterName"] = txtName.Text.ToString().Trim();
            
                switch (_currentOperate)
                {
                    //添加
                    case DataOper.Add: 
                        string errorQ;
                        _store.AddExapChapter(_dictNodeConfigurationBox, out errorQ);
                        if (errorQ == "")
                        {
                            _draw.InitGrid();
                            ClearDataToUi();
                            _draw.SwitchCtl(true);
                            Dictionary<string, object> dictAConfig = new Dictionary<string, object>
                        {
                            {"EmployeeNo", GlobalVar.UserNo},
                            {"EmployeeName", GlobalVar.UserName},
                            {"OperTable", "用例章节"},
                            {"ChapterName", _dictNodeConfigurationBox["ChapterName"]},
                        };
                            _log.WriteLog(EnumLibrary.EnumLog.AddChapter, dictAConfig);
                        }
                        else
                        {
                            XtraMessageBox.Show("请不要添加相同编号的配置盒");
                        } 
                    break;
                    //更新
                    case DataOper.Modify: 
                        string errorU;
                        _store.Update(EnumLibrary.EnumTable.ExapChapter, _dictNodeConfigurationBox, out errorU);
                        if (errorU == "")
                        {
                            ClearDataToUi();
                            _draw.InitGrid();
                            _draw.SwitchCtl(false);
                            Dictionary<string, object> dictAConfig = new Dictionary<string, object>
                        {
                            {"EmployeeNo", GlobalVar.UserNo},
                            {"EmployeeName", GlobalVar.UserName},
                            {"OperTable", "用例章节"},
                            {"oldChapterName", oldChapterName},
                            {"newChapterName", _dictNodeConfigurationBox["ChapterName"]}
                        };
                            _log.WriteLog(EnumLibrary.EnumLog.UpdateChapter, dictAConfig);
                        }
                        else
                        {
                            XtraMessageBox.Show("请不要添加相同章节...");
                        } 
                    break;
                    //删除
                    case DataOper.Del:
                        if (
                            XtraMessageBox.Show("是否删除此章节？", "提示", MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Warning) ==
                            DialogResult.OK)
                        {
                            Dictionary<string, object> dict = new Dictionary<string, object>();
                            dict["ChapterName"] = _dr["ChapterName"];
                            string error;
                            _store.Del(EnumLibrary.EnumTable.ExapChapter, dict, out error);
                            if (error == "")
                            {
                                _draw.InitGrid();
                                tsmiDel.Enabled = false;
                                Dictionary<string, object> dictAConfig = new Dictionary<string, object>
                            {
                                {"EmployeeNo", GlobalVar.UserNo},
                                {"EmployeeName", GlobalVar.UserName},
                                {"OperTable", "用例章节"},
                                {"ChapterName", _dictNodeConfigurationBox["ChapterName"]},
                            };
                                _log.WriteLog(EnumLibrary.EnumLog.DelChapter, dictAConfig);
                            }
                            else
                            {
                                XtraMessageBox.Show("删除操作出错，请联系工程师帮助解决");
                            }
                        }
                        break;
                }
           
        }

        void ClearDataToUi()
        {
            txtName.Text = null; 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Dictionary<string, object> IDraw.GetDataFromUI()
        {
            _dictNodeConfigurationBox["ChapterName"] = txtName.Text;           
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
            txtName.Text = selectedRow["ChapterName"].ToString();
        }

        void IDraw.InitDict()
        {
            _dictNodeConfigurationBox.Add("ChapterName", "");
            _dictNodeConfigurationBox.Add("TestType", "");
            _dictNodeConfigurationBox.Add("OldChapterName", "");
            _dictNodeConfigurationBox.Add("OldTestType", "");
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

        private void gvExapChapter_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //获得光标位置
                var hi = gvExapChapter.CalcHitInfo(e.Location);
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
                gvExapChapter.SelectRow(hi.RowHandle);
                _dr = gvExapChapter.GetDataRow(hi.RowHandle);
            }
        }

        private void gcExapChapter_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var hi = gvExapChapter.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //获得选中行的数值
            int rowCount = hi.RowHandle;
            _dr = this.gvExapChapter.GetDataRow(rowCount);
            _currentOperate = DataOper.Modify;
            if (_dr != null)
            {
                _dictNodeConfigurationBox["OldChapterName"] = _dr["ChapterName"];
                _dictNodeConfigurationBox["TestType"] = _dr["TestType"];
                _draw.SetDataToUI(_dr);
                _draw.SwitchCtl(true);
            }

        }
        private void ShieldRight()
        {
            ContextMenu emptyMenu = new ContextMenu();
            txtName.Properties.ContextMenu = emptyMenu;
        }

        private void com_TestType_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtName_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }
    }
}
