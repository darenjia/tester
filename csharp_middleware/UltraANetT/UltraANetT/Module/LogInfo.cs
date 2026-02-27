using System.Data;
using System.Linq;
using DevExpress.XtraEditors;
using ProcessEngine;
using UltraANetT.Interface;
using System.Collections.Generic;
using DevExpress.XtraGrid.Columns;
using System;
using System.Drawing;
using DevExpress.XtraNavBar;

namespace UltraANetT.Module
{

    public partial class LogInfo : XtraUserControl, IDraw
    {

        #region 设置变量
        private readonly ProcShow _show = new ProcShow();

        private readonly ProcStore _store = new ProcStore();

        private readonly IDraw _draw;

        /// <summary>
        /// 字典类型的员工信息
        /// </summary>
        private Dictionary<string, object> _dictLoginLog = new Dictionary<string, object>();

        private readonly Dictionary<string, object> _dictConfig = new Dictionary<string, object>();

        #endregion

        public LogInfo()
        {
            InitializeComponent();
            _draw = this;
            _draw.InitGrid();
            _draw.InitDict();
            navBarBind();
           
        }

        void IDraw.InitGrid()
        {
            var coList = new List<string>();
            foreach (GridColumn col in gvLogInfo.Columns)
                coList.Add(col.FieldName);
            var dt = _show.DrawDtFromMultiple(coList.ToArray(), EnumLibrary.EnumTable.LoginLog);
            gcLogInfo.DataSource = dt;
        }
        void IDraw.InitDict()
        {
            _dictLoginLog.Add("LoginNo", "");
            _dictLoginLog.Add("EmployeeNo", "");
            _dictLoginLog.Add("EmployeeName", "");
            _dictLoginLog.Add("Department", "");
            _dictLoginLog.Add("LoginDate", "");
            _dictLoginLog.Add("LoginOffDate", "");


            _dictConfig.Add("ElyNo", "");
            _dictConfig.Add("ElyName", "");
            _dictConfig.Add("ElyRole", "");
            _dictConfig.Add("Department", "");
            _dictConfig.Add("Sex", "");
            _dictConfig.Add("Contact", "");
            _dictConfig.Add("Mail", "");
            _dictConfig.Add("Password", "");
            _dictConfig.Add("Remark", "");
        }

        Dictionary<string, object> IDraw.GetDataFromUI()
        {
            throw new NotImplementedException();
        }

        void IDraw.SetDataToUI(DataRow selectedRow)
        {
            throw new NotImplementedException();
        }

        void IDraw.SwitchCtl(bool isSwitch)
        {
            throw new NotImplementedException();
        }

        void IDraw.Submit()
        {
            throw new NotImplementedException();
        }

        private void navBarBind()
        {
            nbcEmpList.Items.Clear();
            GlobalVar.ListCfgTemp = _store.GetRegularByEnum(EnumLibrary.EnumTable.Employee);
            var row = 0;
            foreach (var name in GlobalVar.ListCfgTemp)
            {
                var col = 0;
                var keys = new string[_dictConfig.Keys.Count];
                _dictConfig.Keys.CopyTo(keys, 0);
                foreach (var itemTemp in keys)
                {
                    _dictConfig[itemTemp] = name[col];
                    col++;
                }
                EmpList.AddItem();
                nbcEmpList.Items[row].Caption = _dictConfig["ElyName"].ToString();
                nbcEmpList.Items[row].Appearance.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                nbcEmpList.Items[row].AppearancePressed.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                nbcEmpList.Items[row].LinkClicked += NameItem_Click;
                row++;
                GlobalVar.DictCfgTemp.Add(_dictConfig);
            }
        }

        private void NameItem_Click(object sender, EventArgs e)
        {
            var item = sender as NavBarItem;
            if (item == null) _draw.InitGrid();
            else NameToInitGrid(item.Caption);
        }

        private void NameToInitGrid(string name)
        {
            _dictLoginLog["EmployeeName"] = name;
            var coList = new List<string>();
            foreach (GridColumn col in gvLogInfo.Columns)
                coList.Add(col.FieldName);
            var dt = _show.DrawDtFromMultiple(coList.ToArray(), _dictLoginLog, EnumLibrary.EnumTable.LoginLog);
            gcLogInfo.DataSource = dt;
            this.txtLoginCount.Text = dt.Rows.Count.ToString();
        }

        private void TimeToInitGrid(DateTime up, DateTime down)
        {
            _dictLoginLog["TimeUp"] = up.ToString("yyyy-MM-dd");
            _dictLoginLog["TimeDown"] = down.AddDays(1).ToString("yyyy-MM-dd");
            var coList = new List<string>();
            foreach (GridColumn col in gvLogInfo.Columns)
                coList.Add(col.FieldName);
            var dt = _show.DrawDtFromMultiple(coList.ToArray(), _dictLoginLog, EnumLibrary.EnumTable.LoginLogToTime);
            gcLogInfo.DataSource = dt;
        }

        private void sbtnSelectTime_Click(object sender, EventArgs e)
        {
            if (dtdUp.Text == "" && dtdDown.Text == "")
            {
                _draw.InitGrid();
            }
            if (dtdUp.DateTime <= dtdDown.DateTime)
            {
                TimeToInitGrid(dtdUp.DateTime, dtdDown.DateTime);
            }
        }

        private void sbtnClear_Click(object sender, EventArgs e)
        {
            _draw.InitGrid();
            dtdUp.Text = "";
            dtdDown.Text = "";
            txtLoginCount.Text = "";
        }

        private void gvLogInfo_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            object cellValue = gvLogInfo.GetRowCellValue(e.RowHandle, "LoginOffDate");
            string state = string.Empty;
            string loginNo = string.Empty;
            if (cellValue != null)
            {
                state = gvLogInfo.GetRowCellValue(e.RowHandle, "LoginOffDate").ToString();
                loginNo = gvLogInfo.GetRowCellValue(e.RowHandle, "LoginNo").ToString();
            }
            if(state=="本次登录"|| state == "异常退出")
                return;
            if (state == "1900/1/1 0:00:00" && loginNo == GlobalVar.LoginNo)
            {
                gvLogInfo.SetRowCellValue(e.RowHandle, "LoginOffDate", "本次登录");
                //e.Appearance.BackColor = Color.Green;//设置此行的背景颜色
            }
            else if (state == "1900/1/1 0:00:00")
            {
                gvLogInfo.SetRowCellValue(e.RowHandle, "LoginOffDate", "异常退出");
            }
        }
    }
}