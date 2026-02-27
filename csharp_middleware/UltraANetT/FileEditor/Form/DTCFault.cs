using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ProcessEngine;
using DevExpress.XtraGrid.Columns;
using FileEditor.Control;
using FileEditor.pubClass;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraLayout;
using DevExpress.LookAndFeel;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraLayout.Utils;
using DevExpress.XtraSpreadsheet;
using DBCEngine;
using DevExpress.DataAccess.Native.Sql.QueryBuilder;

namespace FileEditor.Form
{
    #region DTC的一些信息
    #region 欠压故障
    //欠压电压最小值，double类型，选择12V时范围为[0,20]，24V时范围为[0,40];
    //欠压电压最大值，double类型，选择12V时范围为[0,20]，24V时范围为[0,40];
    #endregion
    #region 过压故障
    //过压电压最小值，double类型，选择12V时范围为[0,20]，24V时范围为[0,40];
    //过压电压最大值，double类型，选择12V时范围为[0,20]，24V时范围为[0,40];
    #endregion
    #region 报文丢失
    //报文丢失ID，从DBC内刷Rx报文ID;
    //报文丢失帧数，decimal类型，范围为[0,9999];
    #endregion
    #region 节点丢失
    //节点丢失名称，从DBC内刷出所有节点，排除当前的节点。
    //节点丢失时间，decimal类型，范围为[0,9999];
    #endregion
    #region 信号无效值
    //信号名称，从DBC内刷Rx信号;
    //无效值，decimal类型，范围为[0,999999];
    #endregion
    #region BusOff
    //总线关闭次数，decimal类型，范围为[0,20];
    #endregion
    #region 故障码老化
    //上下电次数，decimal类型，范围为[0,20];
    #endregion
    #region 诊断初始化时间
    //记录故障码时间，decimal类型，范围为[0,9999];
    #endregion
    #endregion

    public partial class DTCFault : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        #region MyRegion
        private Dictionary<string, List<LayoutControlItem>> _dictControlLci =
            new Dictionary<string, List<LayoutControlItem>>();
        private DataTable _dt = new DataTable();
        private readonly ProcShow _show = new ProcShow();
        private procDBC _dbcAnalysis = new procDBC();
        private readonly string _dbcPath = string.Empty;
        private readonly string _DUTName = string.Empty;
        Dictionary<string, string> _dictItem = new Dictionary<string, string>();
        private int _selectRow;
        private DataRow _dr;
        private DataOper _currentOperate;
        private ProcStore _store = new ProcStore();
        #endregion

        private IList<object[]> _faultType;
        public DTCFault()
        {
            InitializeComponent();
        }

        public DTCFault(Dictionary<string, string> dict)
        {
            InitializeComponent();

            #region 临时传值

            //dict["TestChannel"] = "CAN1";
            //dict["DUTname"] = "AC";
            //dict["SystemType"] = "12V";
            //GlobalVar.VNode = new List<string>
            //{
            //    "1",
            //    "1",
            //    "2"
            //};

            #endregion

            InitDictControl();
            _dbcPath = GetDbcPath(dict["TestChannel"]);
            _DUTName = dict["DUTname"];
            InitComBoxControl(dict["TestChannel"], dict["SystemType"]);
            InitDt();
            FillGridView(dict["DTCRelevant"]);
            hideContainerRight.Visible = false;
            ShieldRight();
        }

        public DTCFault(string itemJson)
        {
            InitializeComponent();
            InitDt();
            GlobalVar.strEvent = "";
            cmbFaultType.Properties.Items.Clear();
            _faultType = _store.GetRegularByEnum(EnumLibrary.EnumTable.FaultType);
            foreach (object[] list in _faultType)
            {
                cmbFaultType.Properties.Items.Add(list[0]);
            }

            if (itemJson == "")
            {
                return;
            }

            FillGridView(itemJson);
            hideContainerRight.Visible = false;
            ShieldRight();
        }

        private string GetDbcPath(string strTestChannel)
        {
            List<string> dbc = new List<string>();
            dbc.Add(GlobalVar.VNode[0]);
            dbc.Add(GlobalVar.VNode[1]);
            dbc.Add(GlobalVar.VNode[2]);
            dbc.Add(strTestChannel);
            var dbcList = _store.GetDBCListByVNodeAndCAN(dbc);
            string path = "";
            foreach (var can in dbcList)
            {
                path = AppDomain.CurrentDomain.BaseDirectory + can[5];
            }

            return path;
        }

        private void InitComBoxControl(string strTestChannel, string strSystemType)
        {
            //List<string> dbc = new List<string>();
            //dbc.Add(GlobalVar.VNode[0]);
            //dbc.Add(GlobalVar.VNode[1]);
            //dbc.Add(GlobalVar.VNode[2]);
            //dbc.Add(strTestChannel);

            #region 节点丢失名称

            //bool IsExistDBC;
            //List<string> nodeList = _show.GetDataFromDbc(dbc, out IsExistDBC);
            //nodeList.Remove(_DUTName);
            //cmbCheSumID.Properties.Items.Clear();
            //cmbCheSumID.Properties.Items.AddRange(nodeList);

            #endregion

            #region 信号名称;报文丢失ID

            //cmbInvaildSgValueName.Properties.Items.Clear();
            //cmbRolDtcID.Properties.Items.Clear();
            //Dictionary<string, string> Source = new Dictionary<string, string>();
            //var SourceMessage = _dbcAnalysis.GetNodeReciveMessage(_dbcPath, _DUTName);
            //for (int i = 0; i < 500; i++)
            //{
            //    if (SourceMessage[i, 0] == "" || SourceMessage[i, 1] == "")
            //        continue;
            //    Source[SourceMessage[i, 0]] = SourceMessage[i, 1];
            //    cmbInvaildSgValueName.Properties.Items.Add(SourceMessage[i, 0]); //信号名称
            //    cmbRolDtcID.Properties.Items.Add(SourceMessage[i, 1]); //报文丢失ID
            //}

            #endregion

            #region 欠点过压电压最大值最小值范围

            if (strSystemType.ToUpper() == "12V")
            {
                seLowVoltagemin.Properties.MaxValue = 20;
                seLowVoltagemax.Properties.MaxValue = 20;
                seUpVoltagemin.Properties.MaxValue = 20;
                seUpVoltagemax.Properties.MaxValue = 20;
            }
            else
            {
                seLowVoltagemin.Properties.MaxValue = 40;
                seLowVoltagemax.Properties.MaxValue = 40;
                seUpVoltagemin.Properties.MaxValue = 40;
                seUpVoltagemax.Properties.MaxValue = 40;
            }

            #endregion
        }

        private void InitDictControl()
        {
            #region 绑定LayoutControlItem
            _dictControlLci["欠压故障"] = new List<LayoutControlItem>();
            _dictControlLci["欠压故障"].Add(lciLowVoltagemin); //欠压电压最小值
            _dictControlLci["欠压故障"].Add(lciLowVoltagemax); //欠压电压最大值
            _dictControlLci["过压故障"] = new List<LayoutControlItem>();
            _dictControlLci["过压故障"].Add(lciUpVoltagemin); //过压电压最小值
            _dictControlLci["过压故障"].Add(lciUpVoltagemax); //过压电压最大值
            _dictControlLci["RollingCounter"] = new List<LayoutControlItem>();
            _dictControlLci["RollingCounter"].Add(lciRolDtcID); //RollingCounter ID
            _dictControlLci["RollingCounter"].Add(lciRolDtcCycle); //RollingCounter Cycle
            _dictControlLci["CheckSum"] = new List<LayoutControlItem>();
            _dictControlLci["CheckSum"].Add(lciCheSumID); //CheckSum ID
            _dictControlLci["CheckSum"].Add(lciCheSumCycle); //CheckSum Cycle
            _dictControlLci["BusOff"] = new List<LayoutControlItem>();
            _dictControlLci["BusOff"].Add(lciBusOffNum); //进入BusOff次数
            _dictControlLci["节点丢失"] = new List<LayoutControlItem>();
            _dictControlLci["节点丢失"].Add(lciECUname); //伙伴节点相对应ECU
            _dictControlLci["节点丢失"].Add(lciMsgDTCID); //伙伴节点ID 输入 不保存
            _dictControlLci["节点丢失"].Add(lciMsgDTCCycleTime); //伙伴节点周期 输入 不保存
            _dictControlLci["节点丢失"].Add(lciIDCycleTime); //伙伴节点ID/伙伴节点周期
            _dictControlLci["节点丢失"].Add(lciMsgAdd); //添加
            _dictControlLci["节点丢失"].Add(lciMsgDel); //删除
            #endregion
            cmbFaultType.Properties.Items.Clear();
            cmbFaultType.Properties.Items.AddRange(_dictControlLci.Keys);
        }

        private void spinEdit_EditValueChanging(object sender, ChangingEventArgs e)
        {
            if (e.NewValue.ToString() == string.Empty)
                return;
            if (double.Parse(e.NewValue.ToString()) < 0)
            {
                e.Cancel = true;
            }
        }

        private void cmbFaultType_SelectedValueChanged(object sender, EventArgs e)
        {
            string strFaultType = cmbFaultType.EditValue != null ? cmbFaultType.EditValue.ToString() : string.Empty;
            if (cmbFaultType.SelectedIndex == -1 || string.IsNullOrEmpty(strFaultType))
            {
                lciSubmit.Visibility = LayoutVisibility.Never;
            }
            foreach (var keyLci in _dictControlLci)
            {
                if (keyLci.Key == strFaultType)
                {
                    foreach (var lci in keyLci.Value)
                    {
                        lci.Visibility = LayoutVisibility.Always;
                    }
                }
                else
                {
                    foreach (var lci in keyLci.Value)
                    {
                        lci.Visibility = LayoutVisibility.Never;
                    }
                }
            }
            lciSubmit.Visibility = LayoutVisibility.Always;
        }

        private void ShieldRight()
        {
            ContextMenu emptyMenu = new ContextMenu();
            txtDTCHEX.Properties.ContextMenu = emptyMenu;
            txtDTC.Properties.ContextMenu = emptyMenu;
        }

        private void FillGridView(string json)
        {
            Dictionary<string, string> dictDtcAll = Json.DerJsonToDict(json) != null
                ? Json.DerJsonToDict(json)
                : new Dictionary<string, string>();
            string dtJson = dictDtcAll.ContainsKey("ECUDTCInfo") ? dictDtcAll["ECUDTCInfo"] : string.Empty;
            string ecuJson = dictDtcAll.ContainsKey("ECUGlobalVar") ? dictDtcAll["ECUGlobalVar"] : string.Empty;

            Dictionary<string, string> dictECU = Json.DerJsonToDict(ecuJson) != null
                ? Json.DerJsonToDict(ecuJson)
                : new Dictionary<string, string>();
            seMsgTimeoutCycle.Value =
                dictECU.ContainsKey("MsgTimeoutCycle") ? decimal.Parse(dictECU["MsgTimeoutCycle"]) : 10;
            seMsgRecoverCycle.Value =
                dictECU.ContainsKey("MsgRecoverCycle") ? decimal.Parse(dictECU["MsgRecoverCycle"]) : 5;
            seTDiagStartTestTime.Value =
                dictECU.ContainsKey("TDiagStartTestTime") ? decimal.Parse(dictECU["TDiagStartTestTime"]) : 1000;
            cmbNMawake.EditValue = dictECU.ContainsKey("NMawake") ? dictECU["NMawake"] : "是";
            cmbMsgTestID.EditValue =
                dictECU.ContainsKey("MsgTestID") ? dictECU["MsgTestID"] : string.Empty;

            List<Dictionary<string, string>> itemList = Json.DeserJsonDictStrList(dtJson) != null
                ? Json.DeserJsonDictStrList(dtJson)
                : new List<Dictionary<string, string>>();
            foreach (var item in itemList)
            {
                DataRow dr = _dt.NewRow();
                for (int i = 0; i < dr.ItemArray.Length; i++)
                {
                    dr[i] = "--";
                }

                foreach (var keyitem in item)
                {
                    dr[keyitem.Key] = keyitem.Value;
                }

                _dt.Rows.Add(dr);
            }

            gcDTCF.DataSource = _dt;

            MsgTestIDBind();
        }

        public void InitDt()
        {
            _dt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvDTCF.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList)
                _dt.Columns.Add(new DataColumn(colName, typeof(object)));
        }

        private string DerDtToJson()
        {
            Dictionary<string, string> dictDtcAll = new Dictionary<string, string>();
            dictDtcAll["ECUGlobalVar"] = string.Empty;
            dictDtcAll["ECUDTCInfo"] = string.Empty;
            string dtJson = string.Empty;
            string ecuJson = string.Empty;

            #region ECUDTCInfo

            var coList = new List<string>();
            foreach (GridColumn col in gvDTCF.Columns)
                coList.Add(col.FieldName);
            List<Dictionary<string, string>> dtList = new List<Dictionary<string, string>>();
            foreach (DataRow row in _dt.Rows)
            {
                _dictItem = new Dictionary<string, string>();
                foreach (string col in coList)
                {
                    if (row[col].ToString() != "--")
                    {
                        _dictItem[col] = row[col].ToString();
                    }
                }
                dtList.Add(_dictItem);
            }
            dtJson = Json.SerJson(dtList);
            dictDtcAll["ECUDTCInfo"] = dtJson;

            #endregion

            #region ECUGlobalVar

            Dictionary<string, string> dictECU = new Dictionary<string, string>();
            dictECU["MsgTimeoutCycle"] = seMsgTimeoutCycle.Value.ToString();
            dictECU["MsgRecoverCycle"] = seMsgRecoverCycle.Value.ToString();
            dictECU["TDiagStartTestTime"] = seTDiagStartTestTime.Value.ToString();
            dictECU["NMawake"] = cmbNMawake.EditValue != null ? cmbNMawake.EditValue.ToString() : string.Empty;
            dictECU["MsgTestID"] = cmbMsgTestID.EditValue != null ? cmbMsgTestID.EditValue.ToString() : string.Empty;
            ecuJson = Json.SerJson(dictECU);
            dictDtcAll["ECUGlobalVar"] = ecuJson;

            #endregion
            string strAllJson = Json.SerJson(dictDtcAll);

            return strAllJson;
        }

        private void AssessItemRelevant_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalVar.strEvent = DerDtToJson();
            if (GlobalVar.strEvent != "")
            {
                if (XtraMessageBox.Show("是否需要保存该用例的评价项目相关信息？", "提示", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning) ==
                    DialogResult.Cancel)
                {
                    GlobalVar.strEvent = "";
                    this.DialogResult = DialogResult.Cancel;
                }
                else
                    this.DialogResult = DialogResult.OK;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            switch (_currentOperate)
            {
                //添加
                case DataOper.Add:
                    if (!IsSatisfyCondition())
                        return;
                    DataRow rowAdd= GetDataFromUI();
                    _dt.Rows.Add(rowAdd);
                    gcDTCF.DataSource = _dt;
                    GlobalVar.strEvent = DerDtToJson();
                    ClearDataUI();
                    dpDTCFault.Visibility = DockVisibility.Visible;
                    break;
                //更新
                case DataOper.Modify:
                    if (!IsSatisfyCondition())
                        return;
                    DataRow rowModify = GetDataFromUI();
                    foreach (GridColumn col in gvDTCF.Columns)
                        _dt.Rows[_selectRow][col.FieldName] = rowModify[col.FieldName];
                    GlobalVar.strEvent = DerDtToJson();
                    ClearDataUI();
                    dpDTCFault.Visibility = DockVisibility.Hidden;
                    break;
            }
        }


        private void tsmiDel_Click(object sender, EventArgs e)
        {
            if (_dr == null)
                return;
            _dr.Delete();
            GlobalVar.strEvent = DerDtToJson();

        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Add;
            ClearDataUI();
            dpDTCFault.Visibility = DockVisibility.Visible;
        }

        private void tsmiModify_Click(object sender, EventArgs e)
        {
            UpdateRow();
        }

        private void UpdateRow()
        {
            if (_dr == null)
                return;
            _currentOperate = DataOper.Modify;
            dpDTCFault.Visibility = DockVisibility.Visible;
            SetDataToUI();
        }

        private void gvItem_MouseDown(object sender, MouseEventArgs e)
        {
            var hi = gvDTCF.CalcHitInfo(e.Location);
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
            gvDTCF.SelectRow(hi.RowHandle);
            _selectRow = hi.RowHandle;
            _dr = this.gvDTCF.GetDataRow(_selectRow);
        }

        private void SetDataToUI()
        {
            txtDTC.Text = _dr["DTC"].ToString();
            txtDTCHEX.Text = _dr["DTCHEX"].ToString();
            cmbTestType.EditValue= _dr["TestType"].ToString();
            cmbFaultType.EditValue= _dr["FaultType"].ToString();
            switch (_dr["FaultType"].ToString())
            {
                case "欠压故障":
                    seLowVoltagemin.Value = decimal.Parse(_dr["LowVoltagemin"].ToString());
                    seLowVoltagemax.Value = decimal.Parse(_dr["LowVoltagemax"].ToString());
                    break;
                case "过压故障":
                    seUpVoltagemin.Value = decimal.Parse(_dr["UpVoltagemin"].ToString());
                    seUpVoltagemax.Value = decimal.Parse(_dr["UpVoltagemax"].ToString());
                    break;
                case "RollingCounter":
                    txtRolDtcID.Text = _dr["RolDtcID"].ToString();
                    seRolDtcCycle.Value = decimal.Parse(_dr["RolDtcCycle"].ToString());
                    break;
                case "CheckSum":
                    txtCheSumID.Text = _dr["CheSumID"].ToString();
                    seCheSumCycle.Value = decimal.Parse(_dr["CheSumCycle"].ToString());
                    break;
                case "BusOff":
                    seBusOffNum.Value = decimal.Parse(_dr["BusOffNum"].ToString());
                    break;
                case "节点丢失":
                    txtECUname.Text = _dr["ECUname"].ToString();
                    var iDCycleTimeArray = _dr["MsgDTCIDCycleTime"].ToString().Split(',');
                    lbcIDCycleTime.Items.Clear();
                    lbcIDCycleTime.Items.AddRange(iDCycleTimeArray);
                    break;
                default:
                    break;
            }
        }

        private void ClearDataUI()
        {
            txtDTC.Text = string.Empty;
            txtDTCHEX.Text = string.Empty;
            cmbTestType.SelectedIndex = -1;
            cmbFaultType.SelectedIndex = -1;
            seLowVoltagemin.Value = 8;
            seLowVoltagemax.Value = 12;
            seUpVoltagemin.Value = 14;
            seUpVoltagemax.Value = 18;
            txtRolDtcID.Text = string.Empty;
            seRolDtcCycle.Value = 0;
            txtCheSumID.Text = string.Empty;
            seCheSumCycle.Value = 0;
            seBusOffNum.Value = 5;
            txtECUname.Text = string.Empty;
            txtMsgDTCID.Text = string.Empty;
            seMsgDTCCycleTime.Value = 0;
            lbcIDCycleTime.Items.Clear();
        }

        /// <summary>
        /// 判断字符串是否是十六进制数值，支持前面带0x和不带0x两种，返回的十六进制字符串带0x
        /// </summary>
        /// <param name="str">传进来的数值</param>
        /// <param name="hex">返回的十六进制字符串</param>
        /// <returns></returns>
        private bool isHexString(string str, out string strHex)
        {
            strHex = string.Empty;
            int iDec = 0;
            if (string.IsNullOrEmpty(str))
                return false;
            if (str.Length >= 2)
            {
                if (str.ToLower().Substring(0, 2) == "0x")
                {
                    strHex = str.Remove(0, 2);
                }
                else
                {
                    strHex = str;
                }
            }
            else
            {
                strHex = str;
            }

            if (int.TryParse(strHex, System.Globalization.NumberStyles.AllowHexSpecifier, null, out iDec))
            {
                strHex = @"0x" + strHex.ToUpper();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 判断一个十六进制数值是否在一定范围内
        /// </summary>
        /// <param name="strHexMin">对比的最小值</param>
        /// <param name="strHexMax">对比的最大值</param>
        /// <param name="strHex">对比值</param>
        /// <param name="hex">返回的十六进制字符串（带0x）</param>
        /// <returns></returns>
        private bool CompareHex(string strHexMin, string strHexMax, string strHex, out string hex)
        {
            int dMin;
            int dMax;
            hex = string.Empty;
            strHexMin = strHexMin.ToUpper().Replace(@"0X", string.Empty);
            strHexMax = strHexMax.ToUpper().Replace(@"0X", string.Empty);
            if (int.TryParse(strHexMin, System.Globalization.NumberStyles.AllowHexSpecifier, null, out dMin) &&
                int.TryParse(strHexMax, System.Globalization.NumberStyles.AllowHexSpecifier, null, out dMax))
            {
                int dihex;
                strHex = strHex.ToUpper().Replace(@"0X", string.Empty);
                if (int.TryParse(strHex, System.Globalization.NumberStyles.AllowHexSpecifier, null, out dihex))
                {
                    if (dMin <= dihex && dMax >= dihex)
                    {
                        hex = @"0x" + strHex;
                        return true;
                    }
                }
            }
            return false;
        }


        private bool IsSatisfyCondition()
        {
            if (txtDTC.Text.Trim() == string.Empty)
            {
                XtraMessageBox.Show("DTC项输入格式为空，请检查输入项！");
                return false;
            }

            if (txtDTCHEX.Text.Trim() == string.Empty)
            {
                XtraMessageBox.Show("DTC(HEX)项输入格式为空，请检查输入项！");
                return false;
            }
            else
            {
                string strHex = string.Empty;
                if (CompareHex("0x0", "0xFFFFFF", txtDTCHEX.Text.Trim(), out strHex))
                {
                    txtDTCHEX.Text = strHex;
                }
                else
                {
                    XtraMessageBox.Show("DTC(HEX)项输入格式有误，请输入0x0至0xFFFFFF之间的十六进制数值！");
                    return false;
                }
            }
            string strTestType = cmbTestType.EditValue != null
                ? cmbTestType.EditValue.ToString()
                : string.Empty;
            if (lciTestType.Visibility != LayoutVisibility.Never)
            {
                if (strTestType == string.Empty)
                {
                    XtraMessageBox.Show("测试方式项输入格式有误，请检查输入项！");
                    return false;
                }
            }
            string strFaultType = cmbFaultType.EditValue != null
                ? cmbFaultType.EditValue.ToString()
                : string.Empty;
            if (strFaultType == string.Empty)
            {
                XtraMessageBox.Show("故障类型项输入格式有误，请检查输入项！");
                return false;
            }
            else
            {
                switch (strFaultType)
                {
                    case "欠压故障":
                        if (seLowVoltagemin.Value >= seLowVoltagemax.Value)
                        {
                            XtraMessageBox.Show("欠压电压最小值需小于欠压电压最大值！");
                            return false;
                        }
                        break;
                    case "过压故障":
                        if (seUpVoltagemin.Value >= seUpVoltagemax.Value)
                        {
                            XtraMessageBox.Show("过压电压最小值需小于过压电压最大值！");
                            return false;
                        }
                        break;
                    case "RollingCounter":
                        string strRolDtcIDHex = string.Empty;
                        if (txtRolDtcID.Text.Trim() == string.Empty || !CompareHex("0x0", "0x1FFFFFFF",
                                txtRolDtcID.Text.Trim(), out strRolDtcIDHex))
                        {
                            XtraMessageBox.Show("RollingCounter ID项输入格式有误，请检查输入项！");
                            return false;
                        }
                        txtRolDtcID.Text = strRolDtcIDHex;
                        break;
                    case "CheckSum":
                        string strCheSumID = string.Empty;
                        if (txtCheSumID.Text == string.Empty || !CompareHex("0x0", "0x1FFFFFFF",
                                txtCheSumID.Text.Trim(), out strCheSumID))
                        {
                            XtraMessageBox.Show("CheckSum ID项输入格式有误，请检查输入项！");
                            return false;
                        }
                        txtCheSumID.Text = strCheSumID;
                        break;
                    case "节点丢失":
                        if (txtECUname.Text == string.Empty)
                        {
                            XtraMessageBox.Show("伙伴节点相对应ECU项输入格式有误，请检查输入项！");
                            return false;
                        }
                        if (lbcIDCycleTime.Items.Count == 0)
                        {
                            XtraMessageBox.Show("伙伴节点ID/周期项无数据，请检查输入项！");
                            return false;
                        }
                        break;
                    case "BusOff":
                        break;
                    default:
                        break;
                }
            }

            var drRows = _dt.Select("DTC=" + "'" + txtDTC.Text + "' and " + "DTCHEX=" + "'" + txtDTCHEX.Text + "' and " +
                               "FaultType=" + "'" + strFaultType + "'");
            if (drRows.Length >= 1)
            {
                if (_currentOperate == DataOper.Add)
                {
                    XtraMessageBox.Show("表格内已有重复项，请勿重复添加数据！");
                    return false;
                }
                else
                {
                    foreach (var dr in drRows)
                    {
                        int drIndex = _dt.Rows.IndexOf(dr);
                        if (drIndex != _selectRow)
                        {
                            XtraMessageBox.Show("表格内已有重复项，请勿重复添加数据！");
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        private DataRow GetDataFromUI()
        {
            DataRow dr = _dt.NewRow();
            foreach (GridColumn col in gvDTCF.Columns)
                dr[col.FieldName] = "--";
            dr["DTC"] = txtDTC.Text;
            dr["DTCHEX"] = txtDTCHEX.Text;
            if (lciTestType.Visibility != LayoutVisibility.Never)
            {
                dr["TestType"] = cmbTestType.EditValue != null ? cmbTestType.EditValue.ToString() : string.Empty;
            }
            dr["FaultType"]= cmbFaultType.EditValue != null ? cmbFaultType.EditValue.ToString() : string.Empty;
            string strFaultType = cmbFaultType.EditValue != null ? cmbFaultType.EditValue.ToString() : string.Empty;
            switch (strFaultType)
            {
                case "欠压故障":
                    dr["LowVoltagemin"] = seLowVoltagemin.Value;
                    dr["LowVoltagemax"] = seLowVoltagemax.Value;
                    break;
                case "过压故障":
                    dr["UpVoltagemin"] = seUpVoltagemin.Value;
                    dr["UpVoltagemax"] = seUpVoltagemax.Value;
                    break;
                case "RollingCounter":
                    dr["RolDtcID"] = txtRolDtcID.Text;
                    dr["RolDtcCycle"] = seRolDtcCycle.Value;
                    break;
                case "CheckSum":
                    dr["CheSumID"] = txtCheSumID.Text;
                    dr["CheSumCycle"] = seCheSumCycle.Value;
                    break;
                case "BusOff":
                    dr["BusOffNum"] = seBusOffNum.Value;
                    break;
                case "节点丢失":
                    dr["ECUname"] = txtECUname.Text;
                    string strIDCycleTime = string.Empty;
                    foreach (var item in lbcIDCycleTime.Items)
                    {
                        strIDCycleTime += item.ToString() + ",";
                    }
                    if (strIDCycleTime.Length != 0)
                    {
                        strIDCycleTime = strIDCycleTime.Remove(strIDCycleTime.Length - 1, 1);
                    }
                    dr["MsgDTCIDCycleTime"] = strIDCycleTime;
                    break;
                default:
                    break;
            }
            return dr;
        }

        private enum DataOper
        {
            Add = 0,
            Modify = 1,
        }

        private void gcDTCF_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var hi = gvDTCF.CalcHitInfo(e.Location);
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
            gvDTCF.SelectRow(hi.RowHandle);
            _selectRow = hi.RowHandle;
            _dr = this.gvDTCF.GetDataRow(_selectRow);

            UpdateRow();
        }

        private void sbtnMsgAdd_Click(object sender, EventArgs e)
        {
            if (txtMsgDTCID.Text.Trim() == string.Empty)
            {
                return;
            }

            string strHex = string.Empty;
            if (!CompareHex("0x0", "0x01FFFFFFF", txtMsgDTCID.Text.Trim(), out strHex))
            {
                XtraMessageBox.Show("伙伴节点ID项输入格式有误，请检查输入项！");
                return;
            }
            txtMsgDTCID.Text = strHex;
            string temp = txtMsgDTCID.Text.Trim() + "/" + seMsgDTCCycleTime.Value;
            if (!lbcIDCycleTime.Items.Contains(temp))
            {
                lbcIDCycleTime.Items.Add(temp);
                txtMsgDTCID.Text = string.Empty;
                seMsgDTCCycleTime.Value = 0;
            }
        }

        private void listboxSelectItem(ListBoxControl lbc)
        {
            int index = lbc.SelectedIndex;
            if (index == -1)
                return;
            string strSelect = lbc.GetItem(index).ToString();
            if (string.IsNullOrEmpty(strSelect))
                return;
            string[] strArray = strSelect.Split('/');
            txtMsgDTCID.Text = strArray.Length >= 2 ? strArray[0] : string.Empty;
            seMsgDTCCycleTime.Value= strArray.Length >= 2 ? decimal.Parse(strArray[1]) : 0;
        }

        private void lbcIDCycleTime_SelectedValueChanged(object sender, EventArgs e)
        {
            ListBoxControl lbc = sender as ListBoxControl;
            listboxSelectItem(lbc);
        }

        private void lbcIDCycleTime_MouseDown(object sender, MouseEventArgs e)
        {
            ListBoxControl lbc = sender as ListBoxControl;
            listboxSelectItem(lbc);
        }

        private void sbtnMsgDel_Click(object sender, EventArgs e)
        {
            string strTemp = txtMsgDTCID.Text + "/" + seMsgDTCCycleTime.Value;
            if (lbcIDCycleTime.Items.Contains(strTemp))
            {
                foreach (var litem in lbcIDCycleTime.Items)
                {
                    if (litem.ToString().ToLower() == strTemp.ToLower())
                    {
                        lbcIDCycleTime.Items.Remove(litem);
                        txtMsgDTCID.Text = string.Empty;
                        seMsgDTCCycleTime.Value = 0;
                        break;
                    }
                }
            }
        }

        private void gvDTCF_RowCountChanged(object sender, EventArgs e)
        {
            MsgTestIDBind();
        }

        private void MsgTestIDBind()
        {
            var drRows = _dt.Select("FaultType=" + "'" + "节点丢失" + "'");
            if (drRows.Length > 0)
            {
                cmbMsgTestID.Enabled = true;
            }
            else
            {
                cmbMsgTestID.SelectedIndex = -1;
                cmbMsgTestID.Enabled = false;
            }
            List<string> listMsgTestID = new List<string>();
            foreach (var row in drRows)
            {
                string[] strtempArray = row["MsgDTCIDCycleTime"].ToString().Split(',');
                foreach (var strtemp in strtempArray)
                {
                    if (strtemp.Trim() != string.Empty)
                    {
                        string[] strMsgArray = strtemp.Split('/');
                        if (strMsgArray.Length >= 2)
                        {
                            string strMsgTestID = strMsgArray[0];
                            if (strMsgTestID.Trim() != string.Empty)
                            {
                                if (!listMsgTestID.Contains(strMsgTestID))
                                {
                                    listMsgTestID.Add(strMsgTestID);
                                }
                            }
                        }
                    }
                }
            }

            if (cmbMsgTestID.SelectedIndex != -1)
            {
                if (!listMsgTestID.Contains(cmbMsgTestID.EditValue))
                {
                    cmbMsgTestID.SelectedIndex = -1;
                }
            }
            cmbMsgTestID.Properties.Items.Clear();
            cmbMsgTestID.Properties.Items.AddRange(listMsgTestID.ToArray());
        }
    }
}