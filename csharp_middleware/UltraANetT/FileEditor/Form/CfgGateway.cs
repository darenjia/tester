using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraGrid.Columns;
using ProcessEngine;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraEditors;
using FileEditor.pubClass;
using DBCEngine;

namespace FileEditor.Form
{
    public partial class CfgGateway : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        DataTable _dt = new DataTable();
        private DataRow _dr;
        private ProcStore _store = new ProcStore();
        private DataOper _currentOperate;
        private int RowIndex = 0;
        private bool Change = false;
        private Dictionary<string, string> Source = new Dictionary<string, string>();
        private Dictionary<string, string> Target = new Dictionary<string, string>();
        private Dictionary<string, string> CANPath = new Dictionary<string, string>();
        private  procDBC _dbcAnalysis = new procDBC();
        
        public CfgGateway(string Json, Dictionary<string, string> Path)
        {
            InitializeComponent();
            
            CANPath = Path;
            BindCombox();
            InitGrid(Json);
            cmbRoutingType.SelectedIndex = 0;
            hideContainerRight.Visible = false;
            ShieldRight();
        }

        private void ShieldRight()
        {
            ContextMenu emptyMenu_1 = new ContextMenu();
            txtSourceMessageID.Properties.ContextMenu = emptyMenu_1;
            ContextMenu emptyMenu_2 = new ContextMenu();
            txtTargetMessageID.Properties.ContextMenu = emptyMenu_2;
            

        }

        private enum DataOper
        {
            Add = 0,
            Update = 1,
        }

        private void BindCombox()
        {
            cmbSourceMessageName.Properties.Items.Clear();//来源报文名称
            cmbSourceSignalName.Properties.Items.Clear();//来源信号名称
            cmbTargetMessageName.Properties.Items.Clear();//目标报文名称
            cmbTargetSignalName.Properties.Items.Clear();//目标信号名称

            Source = new Dictionary<string, string>();
            var SourceMessage = _dbcAnalysis.GetNodeReciveMessage(CANPath["SourceDBC"], CANPath["Gateway"]);
            for (int i = 0; i < 500; i++)
            {
                if(SourceMessage[i, 0]==""|| SourceMessage[i, 1]=="")
                    continue;
                Source[SourceMessage[i, 0]] = SourceMessage[i, 1];
                cmbSourceMessageName.Properties.Items.Add(SourceMessage[i, 0]);
            }
            Target = new Dictionary<string, string>();
            var TargetMessage = _dbcAnalysis.GetNodeSendMessage(CANPath["TargetDBC"], CANPath["Gateway"]);
            for (int i = 0; i < 500; i++)
            {
                if (TargetMessage[i, 0] == "" || TargetMessage[i, 1] == "")
                    continue;
                Target[TargetMessage[i, 0]] = TargetMessage[i, 1];
                cmbTargetMessageName.Properties.Items.Add(TargetMessage[i, 0]);
            }
            
            var SourceSignalName = _dbcAnalysis.GetNodeReceiveSingal(CANPath["SourceDBC"], CANPath["Gateway"]);
            foreach (var Name in SourceSignalName)
            {
                if (Name == "")
                    continue;
                cmbSourceSignalName.Properties.Items.Add(Name);
            }

            var TargetSignalName = _dbcAnalysis.GetNodeSendSingal(CANPath["TargetDBC"], CANPath["Gateway"]);
            foreach (var Name in TargetSignalName)
            {
                if (Name == "")
                    continue;
                cmbTargetSignalName.Properties.Items.Add(Name);
            }
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
                object[] objDr = new object[7];
                objDr[0] = dictDr["SourceMessageName"];
                objDr[1] = dictDr["SourceMessageID"];
                objDr[2] = dictDr["SourceSignalName"];
                objDr[3] = dictDr["TargetMessageName"];
                objDr[4] = dictDr["TargetMessageID"];
                objDr[5] = dictDr["TargetSignalName"];
                objDr[6] = dictDr["RoutingType"];
                _dt.Rows.Add(objDr);
            }
            gcEvent.DataSource = _dt;
        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            SwitchCtl(true);
            _currentOperate = DataOper.Add;
        }

        private void tsmiUpdate_Click(object sender, EventArgs e)
        {
            SwitchCtl(true);
            Update();
        }

        private void Update()
        {
            if (_dr == null)
                return;
            _currentOperate = DataOper.Update;
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
            cmbSourceMessageName.Text = _dr["SourceMessageName"].ToString();
            txtSourceMessageID.Text = _dr["SourceMessageID"].ToString();
            if (_dr["SourceSignalName"].ToString() == "--")
            {
                cmbSourceSignalName.Text = null;
            }
            else
            {
                cmbSourceSignalName.Text = _dr["SourceSignalName"].ToString();
            }
            cmbTargetMessageName.Text = _dr["TargetMessageName"].ToString();
            txtTargetMessageID.Text = _dr["TargetMessageID"].ToString();
            if (_dr["TargetSignalName"].ToString() == "--")
            {
                cmbTargetSignalName.Text = null;
            }
            else
            {
                cmbTargetSignalName.Text = _dr["TargetSignalName"].ToString();
            }
            for (int i = 0; i < cmbRoutingType.Properties.Items.Count; i++)
            {
                if (_dr["RoutingType"].ToString() == cmbRoutingType.Properties.Items[i].ToString())
                {
                    cmbRoutingType.SelectedIndex = i;
                }
            }
        }

        private void ClearDataToUI()
        {
            cmbSourceMessageName.SelectedIndex = -1;
            txtSourceMessageID.Text = "";
            cmbSourceSignalName.SelectedIndex = -1;
            cmbTargetMessageName.SelectedIndex = -1;
            txtTargetMessageID.Text = "";
            cmbTargetSignalName.SelectedIndex = -1;
            cmbRoutingType.SelectedIndex = 0;
        }

        private void gvEvent_MouseDown(object sender, MouseEventArgs e)
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
                if (_dt.Rows[j]["SourceMessageName"].ToString() == cmbSourceMessageName.Text && _dt.Rows[j]["SourceMessageID"].ToString() == txtSourceMessageID.Text &&
                    _dt.Rows[j]["SourceSignalName"].ToString() == cmbSourceSignalName.Text && _dt.Rows[j]["TargetMessageName"].ToString() == cmbTargetMessageName.Text &&
                    _dt.Rows[j]["TargetMessageID"].ToString() == txtTargetMessageID.Text && _dt.Rows[j]["TargetSignalName"].ToString() == cmbTargetSignalName.Text &&
                    _dt.Rows[j]["RoutingType"].ToString() == cmbRoutingType.Text)
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
            if(cmbRoutingType.EditValue.ToString()==""|| cmbRoutingType.EditValue == null)
            {
                XtraMessageBox.Show("请检查输入项是否存在空值！");
                return;
            }
            else if (cmbRoutingType.EditValue.ToString().Substring(2, 2) == "报文")
            {
                if (cmbSourceMessageName.Text == "" || txtSourceMessageID.Text == "" ||  cmbTargetMessageName.Text == "" || txtTargetMessageID.Text == "" ||  cmbRoutingType.Text == "")
                {
                    XtraMessageBox.Show("请检查输入项是否存在空值！");
                    return;
                }
            }
            else if (cmbRoutingType.EditValue.ToString().Substring(2, 2) == "信号")
            {
                if (cmbSourceMessageName.Text == "" || txtSourceMessageID.Text == "" || cmbSourceSignalName.Text == "" || cmbTargetMessageName.Text == "" || txtTargetMessageID.Text == "" || cmbTargetSignalName.Text == "" || cmbRoutingType.Text == "")
                {
                    XtraMessageBox.Show("请检查输入项是否存在空值！");
                    return;
                }
            }
            if(!PrimaryKey())
                return;
            string SourceSignalName;
            string TargetSignalName;
            if (string.IsNullOrEmpty(this.cmbSourceSignalName.Text))
            {
                SourceSignalName = "--";
            }
            else
            {
                SourceSignalName = this.cmbSourceSignalName.Text;
            }
            if (string.IsNullOrEmpty(this.cmbTargetSignalName.Text))
            {
                TargetSignalName = "--";
            }
            else
            {
                TargetSignalName = this.cmbTargetSignalName.Text;
            }
            switch (_currentOperate)
            {
                case DataOper.Add:
                    object[] obj = new object[]
                    {
                        cmbSourceMessageName.Text,txtSourceMessageID.Text,SourceSignalName,cmbTargetMessageName.Text,txtTargetMessageID.Text,TargetSignalName,cmbRoutingType.Text
                    };
                    _dt.Rows.Add(obj);
                    gcEvent.DataSource = _dt;
                    SwitchCtl(true);
                    break;
                case DataOper.Update:
                    _dt.Rows[RowIndex][0] = SourceMessageName;
                    _dt.Rows[RowIndex][1] = txtSourceMessageID.Text;
                    _dt.Rows[RowIndex][2] = SourceSignalName;
                    _dt.Rows[RowIndex][3] = TargetMessageName;
                    _dt.Rows[RowIndex][4] = txtTargetMessageID.Text;
                    _dt.Rows[RowIndex][5] = TargetSignalName;
                    _dt.Rows[RowIndex][6] = cmbRoutingType.Text;
                    gcEvent.DataSource = _dt;
                    SwitchCtl(false);
                    break;
            }
            Change = true;
            ClearDataToUI();
        }

        private void SwitchCtl(bool isSwitch)
        {
            switch (isSwitch)
            {
                case true:
                    sbtnSubmit.Enabled = true;
                    dpEvent.Visibility = DockVisibility.Visible;
                    break;
                case false:
                    sbtnSubmit.Enabled = false;
                    dpEvent.Visibility = DockVisibility.Hidden;
                    break;
            }
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

        private void CfgGateway_FormClosing(object sender, FormClosingEventArgs e)
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

        private void cmbRoutingType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbRoutingType.Text == "" || cmbRoutingType.EditValue == null)
                return;
            if(cmbRoutingType.EditValue.ToString().Substring(2,2)=="报文")
            {
                lciSourceSignalName.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                lciTargetSignalName.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;

                cmbSourceSignalName.SelectedIndex = -1;
                cmbTargetSignalName.SelectedIndex = -1;
            }
            else if(cmbRoutingType.EditValue.ToString().Substring(2, 2) == "信号")
            {
                lciSourceSignalName.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
                lciTargetSignalName.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
            }
        }

        private void cmbSourceMessageName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSourceMessageName.SelectedIndex == -1 || cmbSourceMessageName.Text == "")
            {
                txtSourceMessageID.Text = "";
            }
            else
            {
                txtSourceMessageID.Text = Source[cmbSourceMessageName.Text];
            }
        }

        private void cmbTargetMessageName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTargetMessageName.SelectedIndex == -1 || cmbTargetMessageName.Text == "")
            {
                txtTargetMessageID.Text = "";
            }
            else
            {
                txtTargetMessageID.Text = Target[cmbTargetMessageName.Text];
            }
        }

        private void gcEvent_MouseDoubleClick(object sender, MouseEventArgs e)
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

            Update();
        }

        private void sbtnAdd_Click(object sender, EventArgs e)
        {
            SwitchCtl(true);
            _currentOperate = DataOper.Add;
        }

        private void sbtnUpdate_Click(object sender, EventArgs e)
        {
            SwitchCtl(true);
            Update();
        }

        private void sbtnDel_Click(object sender, EventArgs e)
        {
            if (_dr != null)
            {
                Change = true;
                _dr.Delete();
            }
        }
    }
}