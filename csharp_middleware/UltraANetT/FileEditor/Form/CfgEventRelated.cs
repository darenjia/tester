using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraGrid.Columns;
using ProcessEngine;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraEditors;
using FileEditor.pubClass;

namespace FileEditor.Form
{
    public partial class CfgEventRelated : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        DataTable _dt = new DataTable();
        private DataRow _dr;
        private ProcStore _store = new ProcStore();
        private DataOper _currentOperate;
        private int RowIndex = 0;
        private bool Change = false;

        public CfgEventRelated(string Json,string SlaveboxID)
        {
            InitializeComponent();
            InitGrid(Json);
            findLocalEventIOList(SlaveboxID);
            hideContainerRight.Visible = false;
            ShieldRight();
        }
        private void ShieldRight()
        {
            ContextMenu emptyMenu_1 = new ContextMenu();
            txtLocalEventName.Properties.ContextMenu = emptyMenu_1;
            

        }

        private enum DataOper
        {
            Add = 0,
            Update = 1,
        }

        private void InitGrid(string json)
        {
            _dt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvEvent.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList.ToArray())
                _dt.Columns.Add(new DataColumn(colName, typeof(object)));
            if (json == "" || json == "{}")
            {
                return;
            }
            var ListDt = Json.DerJsonToLDict(json);
            foreach (var dictDr in ListDt)
            {
                object[] objDr = new object[4];
                objDr[0] = dictDr["AwakeType"];
                objDr[1] = dictDr["LocalEventIO"];
                objDr[2] = dictDr["EnableLevel"];
                objDr[3] = dictDr["LocalEventName"];
                _dt.Rows.Add(objDr);
            }
            gcEvent.DataSource = _dt;
        }

        private void findLocalEventIOList(string SlaveboxID)
        {
            int BoxCount = 0;
            Dictionary<string,object> dict=new Dictionary<string, object>();
            dict["Name"] = SlaveboxID;
            bool BoxCfg =int.TryParse(_store.GetSpecialByEnum(EnumLibrary.EnumTable.NodeConfigurationBox, dict)[0][2].ToString(),out BoxCount);
            if (BoxCfg)
            {
                for (int i = 1; i <= BoxCount; i++)
                {
                    cmbLocalEventIO.Properties.Items.Add("IO" + i);
                }
            }
            else
            {
                XtraMessageBox.Show("当前所选" + SlaveboxID + "节点配置盒数量错误，请重新配置当前节点配置盒数量。");
            }
        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            dpEvent.Visibility = DockVisibility.Visible;
            _currentOperate = DataOper.Add;
        }

        private void tsmiUpdate_Click(object sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            if (_dr == null)
                return;
            _currentOperate = DataOper.Update;
            dpEvent.Visibility = DockVisibility.Visible;
            SetDataToDockPanel();
        }

        private void tsmiDel_Click(object sender, EventArgs e)
        {
            if (_dr != null)
            {
                Change = true;
                _dr.Delete();
            }
        }

        private void SetDataToDockPanel()
        {
            if (_dr == null)
                return;
            cmbAwakeType.Text = _dr["AwakeType"].ToString();
            cmbLocalEventIO.Text = _dr["LocalEventIO"].ToString();
            cmbEnableLevel.Text = _dr["EnableLevel"].ToString();
            txtLocalEventName.Text = _dr["LocalEventName"].ToString();
        }

        private void ClearDataToUI()
        {
            cmbAwakeType.SelectedIndex = cmbAwakeType.Properties.Items.Count > 0 ? 0 : -1;
            cmbLocalEventIO.SelectedIndex = -1;
            cmbEnableLevel.SelectedIndex = cmbEnableLevel.Properties.Items.Count > 0 ? 0 : -1;
            txtLocalEventName.Text = "";
        }

        private void gvEvent_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _dr = null;
                //获得光标位置
                var hi = gvEvent.CalcHitInfo(e.Location);
                //判断位置是否在行位置上
                if (!hi.InRow && !hi.InRowCell) return;
                if (hi.RowHandle < 0) return;
                //取一行值
                gvEvent.SelectRow(hi.RowHandle);
                RowIndex = hi.RowHandle;
                _dr = gvEvent.GetDataRow(hi.RowHandle);
            }
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
                //if (_dt.Rows[j]["AwakeType"].ToString() == cmbAwakeType.Text && _dt.Rows[j]["LocalEventIO"].ToString() == cmbLocalEventIO.Text && _dt.Rows[j]["EnableLevel"].ToString() == cmbEnableLevel.Text)
                if (_dt.Rows[j]["LocalEventIO"].ToString() == cmbLocalEventIO.Text)
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

        private void sbtnSubmit_Click(object sender, EventArgs e)
        {
            if (cmbAwakeType.Text == "" || cmbLocalEventIO.Text == "" || cmbEnableLevel.Text == "")
            {
                XtraMessageBox.Show("请检查输入项是否存在空值！");
                return;
            }

            if (_dt.Rows.Count >= 8)
            {
                XtraMessageBox.Show("最多只能添加8项！");
                return;
            }
            if(!PrimaryKey())
                return;
            switch (_currentOperate)
            {
                case DataOper.Add:
                    object[] obj = new object[]
                    {
                        cmbAwakeType.Text,cmbLocalEventIO.Text,cmbEnableLevel.Text,txtLocalEventName.Text
                    };
                    _dt.Rows.Add(obj);
                    gcEvent.DataSource = _dt;
                    dpEvent.Visibility = DockVisibility.Visible;
                    break;
                case DataOper.Update:
                    if (_dr == null)
                    {
                        dpEvent.Visibility = DockVisibility.Hidden;
                        return;
                    }
                    _dt.Rows[RowIndex][0] = cmbAwakeType.Text;
                    _dt.Rows[RowIndex][1] = cmbLocalEventIO.Text;
                    _dt.Rows[RowIndex][2] = cmbEnableLevel.Text;
                    _dt.Rows[RowIndex][3] = txtLocalEventName.Text;
                    gcEvent.DataSource = _dt;
                    dpEvent.Visibility = DockVisibility.Hidden;
                    break;
            }
            Change = true;
            ClearDataToUI();
            
        }
        private string DtToJson()
        {
            var listLocalEvent = new List<Dictionary<string, string>>();
            var listConfig = new List<object>();
            foreach (DataRow row in _dt.Rows)
            {
                Dictionary<string, string> rowDict = new Dictionary<string, string>();
                foreach (GridColumn col in gvEvent.Columns)
                {
                    rowDict[col.FieldName] = row[col.FieldName].ToString();
                }
                listLocalEvent.Add(rowDict);
            }
            var json = Json.SerJson(listLocalEvent);
            return json;
        }

        private void CfgEventRelated_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Change)
            {
                GlobalVar.strEvent = DtToJson();
                if (XtraMessageBox.Show("是否确定保存进行的修改？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    this.DialogResult = DialogResult.OK;
                }
            }
        }

        private void gcEvent_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //获得光标位置
            var hi = gvEvent.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //取一行值
            gvEvent.SelectRow(hi.RowHandle);
            RowIndex = hi.RowHandle;
            _dr = gvEvent.GetDataRow(hi.RowHandle);
            Update();
        }
    }
}