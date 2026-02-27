using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using ProcessEngine;
using UltraANetT.Interface;
using System.Linq;
using DevExpress.XtraNavBar;
using System.Drawing;

namespace UltraANetT.Module
{
    public partial class OperationLog : XtraUserControl, IDraw
    {
        #region 设置变量
        private readonly ProcShow _show = new ProcShow();

        private readonly ProcStore _store = new ProcStore();
        
        private readonly IDraw _draw;

        /// <summary>
        /// 字典类型的员工信息
        /// </summary>
        private Dictionary<string, object> _dictOperLog = new Dictionary<string, object>();

        private readonly Dictionary<string, object> _dictConfig = new Dictionary<string, object>();

        #endregion

        public OperationLog()
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
            foreach (GridColumn col in gvOperationLog.Columns)
                coList.Add(col.FieldName);
            var dt = _show.DrawDtFromMultiple(coList.ToArray(), EnumLibrary.EnumTable.OperationLog);
            gcOperationLog.DataSource = dt;
        }
        void IDraw.InitDict()
        {
            _dictOperLog.Add("OperNo", "");
            _dictOperLog.Add("EmployeeNo", "");
            _dictOperLog.Add("EmployeeName", "");
            _dictOperLog.Add("Department", "");
            _dictOperLog.Add("OperRecord", "");
            _dictOperLog.Add("OperDate", "");

            
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
            _dictOperLog["EmployeeName"] = name;
            var coList = new List<string>();
            foreach (GridColumn col in gvOperationLog.Columns)
                coList.Add(col.FieldName);
            var dt = _show.DrawDtFromMultiple(coList.ToArray(), _dictOperLog, EnumLibrary.EnumTable.OperationLog);
            gcOperationLog.DataSource = dt;
        }

        private void TimeToInitGrid(DateTime up,DateTime down)
        {
            _dictOperLog["TimeUp"] = up.ToString("yyyy-MM-dd");
            _dictOperLog["TimeDown"] = down.AddDays(1).ToString("yyyy-MM-dd");
            var coList = new List<string>();
            foreach (GridColumn col in gvOperationLog.Columns)
                coList.Add(col.FieldName);
            var dt = _show.DrawDtFromMultiple(coList.ToArray(), _dictOperLog, EnumLibrary.EnumTable.OperationLogToTime);
            gcOperationLog.DataSource = dt;
        }


        private void sbtnSelectTime_Click(object sender, EventArgs e)
        {
            if(dtdUp.Text==""&&dtdDown.Text=="")
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
        }
    }
}
