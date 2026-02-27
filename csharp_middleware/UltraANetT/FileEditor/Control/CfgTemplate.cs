using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using DevExpress.XtraNavBar;
using FileEditor.Form;
using FileEditor.pubClass;
using ProcessEngine;
using System.Data;
using System.Linq;
using DBCEngine;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraGrid.Columns;
using DBEngine;
using DevExpress.DataAccess.Native.Sql.QueryBuilder;
using OSEKCLASS;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraSplashScreen;

namespace FileEditor.Control
{
    public partial class CfgTemplate : XtraUserControl, ITemplate
    {
        private ProcLog Log = new ProcLog();
        private readonly ProcShow _show = new ProcShow();
        private readonly GetOSEK _osek = new GetOSEK();
        private readonly procDBC _dbcAnalysis = new procDBC();
        private readonly ProcStore _store;
        private readonly ITemplate _tem;
        private string _selectedName = "";
        private readonly bool _isSave = true;
        private bool Viewer = false;
        private Thread th;
        private List<LayoutControlItem> LcItems = new List<LayoutControlItem>();
        private List<LayoutControlItem> LcIBauditems = new List<LayoutControlItem>();
        DataTable _dt = new DataTable();
        private DataRow _dr;
        private int selectRow;
        private DataOper _currentOperate;
        private readonly Dictionary<string, object> _dictRelated = new Dictionary<string, object>();
        private readonly Dictionary<string, object> _dictConfig = new Dictionary<string, object>();
        private NavBarItem oldBarItem = null;
        private string _itemName;
        private ComboBoxEdit combEdit;
        private ComboBoxEdit cbeMasterNodeType;
        private Dictionary<string, ComboBoxEdit> _dictCBEGateway = new Dictionary<string, ComboBoxEdit>();
        private bool _nameItem = false;
        private List<string> BaudValue = new List<string>();
        public CfgTemplate()
        {
            InitializeComponent();
            _store = new ProcStore();
            _tem = this;
            _tem.InitDict();
            InitDpBaud();
            GlobalVar.SelectName = "";
            //InitEmlPage();
            //Localizer_CN sss = new Localizer_CN();
            if (GlobalVar.IsIndependent == true)
            {
                Viewer = true;
            }
            else
            {
                Viewer = false;
                BtnSubmitImage();
                //InitCfgGrid();
            }
            if (GlobalVar.IsIndependent)
            {
                btnSave.Visibility = BarItemVisibility.Never;
                btnSubmit.Visibility = BarItemVisibility.Never;
                btnRefresh.Visibility = BarItemVisibility.Never;
                btnBaud.Visibility = BarItemVisibility.Never;

                //dpCfgTemplate.Visibility = DockVisibility.Hidden;
                //dpBaud.Visibility = DockVisibility.Hidden;

                dpBaud.Enabled = false;
                dpCfg.Enabled = false;
                cmsCfgTemplate.Enabled = false;
                btnEdit.Visibility= BarItemVisibility.Always;
                btnEdit.Enabled = true;
            }
            else
            {
                btnEdit.Visibility = BarItemVisibility.Never;
                btnEdit.Enabled = false;
            }
            //JsonToList();
            hideContainerRight.Visible = false;

            UpdateGridViewByOperate();
        }

        public void FormClosed()
        {
            if (th != null && th.IsAlive)
                th.Abort();
        }

        private void InitDpBaud()
        {
            lcgBaud.Clear();
            if (!GlobalVar.IsIndependent)
            {
                List<string> listV = new List<string>
                {
                    GlobalVar.VNode[0],
                    GlobalVar.VNode[1],
                    GlobalVar.VNode[2],
                    _selectedName.Split('配')[0]
                };
                Dictionary<string, object> dictV = new Dictionary<string, object>();
                dictV["VehicelType"] = GlobalVar.VNode[0];
                dictV["VehicelConfig"] = GlobalVar.VNode[1];
                dictV["VehicelStage"] = GlobalVar.VNode[2];
                IList<object[]> file = ReadDataByVehicel(listV);
                if (file.Count != 0 && file[0][8].ToString() != "")
                {
                    Dictionary<string, string> cbText = new Dictionary<string, string>();
                    cbText = Json.DerJsonToDict(file[0][8].ToString());
                    Dictionary<string, string> dictBaud = new Dictionary<string, string>();
                    BaudValue.Clear();
                    var ListBaud = _store.GetRegularByEnum(EnumLibrary.EnumTable.Segment);
                    var listDbc = _store.GetSpecialByEnum(EnumLibrary.EnumTable.DBC, dictV);
                    List<string> listDbcGetChannel = new List<string>();
                    foreach (var dbc in listDbc)
                    {
                        listDbcGetChannel.Add(dbc[4].ToString());
                    }
                    
                    foreach (var Bauditem in ListBaud)
                    {
                        if (!BaudValue.Contains(Bauditem[1].ToString()))
                        {
                            BaudValue.Add(Bauditem[1].ToString());
                        }
                        if (listDbcGetChannel.Contains(Bauditem[0].ToString()))
                        {
                            if (cbText.ContainsKey(Bauditem[0].ToString()))
                            {
                                dictBaud[Bauditem[0].ToString()] = cbText[Bauditem[0].ToString()];
                            }
                            else
                            {
                                dictBaud[Bauditem[0].ToString()] = Bauditem[1].ToString();
                            }
                        }
                    }
                    BaudValue = BaudValue.OrderBy(t =>double.Parse(t)).ToList();
                    DrawFillBaud(dictBaud);
                }
                else
                {
                    GetBaudDict();
                }
            }
        }

        private void AllowColumSort()
        {
            foreach (GridColumn col in gvCfgTemplate.Columns)
            {
                col.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True; 
            }
        }


        /// <summary>
        /// 点击左边配置表名称后刷新GridView
        /// </summary>
        public void InitItemName()
        {
            if (_itemName != "")
            {
                foreach (NavBarItem item in nbcTempList.Items)
                {
                    if (item.Caption == _itemName)
                    {
                        item.Appearance.ForeColor = Color.Blue;
                        oldBarItem = item;
                    }
                }
            }
        }

        private void InitEmlPage()
        {
            _tem.DrawTem(_selectedName);
            if (!GlobalVar.IsIndependent)
            {
                var dictTem = new Dictionary<string, object>();
                dictTem.Add("Name", _selectedName);
                //IList<object[]> emlTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ConfigTemp, dictTem);
                string match = _selectedName.Split('配')[0];
                Dictionary<string, object> _dictRelated = new Dictionary<string, object>
                {
                    {"VehicelType", GlobalVar.VNode[0]},
                    {"VehicelConfig", GlobalVar.VNode[1]},
                    {"VehicelStage", GlobalVar.VNode[2]},
                    {"MatchSort", match}

                };
                ;
                //IList<object[]> file = ReadDataByVehicel(GlobalVar.VNode);

                IList<object[]> flList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelRep,
                    _dictRelated);

                string JsonRead = CfgReadJson();
                if (JsonRead != "")
                {
                    if (flList.Count != 0 && flList[0][12].ToString() != "")
                    {
                        GlobalVar.ListDrScheme = Json.DeserJsonDList(flList[0][12].ToString());
                        JsonToList(GlobalVar.ListDrScheme);
                        InitGrid();
                        JsonToDt(JsonRead);
                        CreateDockPanel(GlobalVar.ListDrScheme);
                        AllowColumSort();
                    }
                    else
                    {
                        //_tem.DrawExcel(_selectedName)
                        //CreateConPageByTem(emlTem);
                        InitGrid();
                        JsonToDt(JsonRead);
                        CreateDockPanel(GlobalVar.ListDrScheme);
                        AllowColumSort();
                    }
                }
                else if (flList.Count != 0 && flList[0][7].ToString() != "")
                {
                    //matchSort.EditValue = file[0][3]; 
                    if (flList.Count != 0 && flList[0][12].ToString() != "")
                    {
                        GlobalVar.ListDrScheme = Json.DeserJsonDList(flList[0][12].ToString());
                        JsonToList(GlobalVar.ListDrScheme);
                        InitGrid();
                        JsonToDt(flList[0][7].ToString());
                        CreateDockPanel(GlobalVar.ListDrScheme);
                        AllowColumSort();
                    }
                    else
                    {
                        //_tem.DrawExcel(_selectedName)
                        //CreateConPageByTem(emlTem);
                        InitGrid();
                        JsonToDt(flList[0][7].ToString());
                        CreateDockPanel(GlobalVar.ListDrScheme);
                        AllowColumSort();
                    }

                }
                else if (flList.Count != 0 && flList[0][12].ToString() != "")
                {
                    GlobalVar.ListDrScheme = Json.DeserJsonDList(flList[0][12].ToString());
                    JsonToList(GlobalVar.ListDrScheme);

                    //var dictTem = new Dictionary<string, object>();
                    //dictTem.Add("Name", _selectedName);
                    //IList<object[]> conTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ConfigTemp, dictTem);
                    //CreateConPageByTem(conTem);
                    InitGrid();
                    CreateDockPanel(GlobalVar.ListDrScheme);
                    AllowColumSort();
                    //if (
                    //    XtraMessageBox.Show("是否需要修改该车型的配置表的列数？", "提示", MessageBoxButtons.OKCancel,
                    //        MessageBoxIcon.Warning) ==
                    //    DialogResult.OK)
                    //{
                    //    ShowColEdit();
                    //}
                }
                else
                {
                    var dictCon = new Dictionary<string, object>();
                    dictCon.Add("Name", _selectedName);
                    //IList<object[]> conTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ConfigTemp, dictCon);
                    //_tem.DrawExcel(_selectedName)
                    //CreateConPageByTem(conTem);
                    InitGrid();
                    CreateDockPanel(GlobalVar.ListDrScheme);
                    AllowColumSort();
                    //if (
                    //        XtraMessageBox.Show("是否需要修改该模板配置表的列数？", "提示", MessageBoxButtons.OKCancel,
                    //            MessageBoxIcon.Warning) ==
                    //        DialogResult.OK)
                    //{
                    //    ShowColEdit();
                    //}
                }
            }
            /////文件编辑器入口
            else
            {
                var dictTem = new Dictionary<string, object>();
                dictTem.Add("Name", _selectedName);
                //IList<object[]> emlTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ConfigTemp, dictTem);
                //_tem.DrawExcel(_selectedName)
                //CreateConPageByTem(emlTem);
                InitGrid();
                if (
                    XtraMessageBox.Show("是否需要修改该模板配置表的列数？", "提示", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning) ==
                    DialogResult.OK)
                {
                    ShowColEdit();
                }
            }
        }

        private IList<object[]> ReadDataByVehicel(List<string> node)
        {
            IList<object[]> listFile = new List<object[]>();
            if (node.Count != 0)
            {
                Dictionary<string, object> dictCNode = new Dictionary<string, object>();
                dictCNode.Add("VehicelType", node[0]);
                dictCNode.Add("VehicelConfig", node[1]);
                dictCNode.Add("VehicelStage", node[2]);
                listFile = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicel, dictCNode);

            }
            return listFile;
        }

        //
        private void BtnSubmitImage()
        {
            string strJson = CfgReadJson();
            if (strJson != string.Empty)
            {
                btnSubmit.ImageIndex = 3;
            }
            else
            {
                _dictRelated["VehicelType"] = GlobalVar.VNode[0];
                _dictRelated["VehicelConfig"] = GlobalVar.VNode[1];
                _dictRelated["VehicelStage"] = GlobalVar.VNode[2];
                _dictRelated["MatchSort"] = _selectedName.Split('配')[0];
                IList<object[]> flList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelRep, _dictRelated);
                if (flList.Count == 0)
                {
                    btnSubmit.ImageIndex = 6;
                }
                else
                {
                    if (flList[0][7].ToString() == string.Empty)
                        btnSubmit.ImageIndex = 6;
                    else
                        btnSubmit.ImageIndex = 5;
                }
            }
        }


        /// <summary>
        /// 添加列
        /// </summary>
        private void AddColumns(List<object> column)
        {

            if (bool.Parse(column[3].ToString()))
            {
                DevExpress.XtraGrid.Columns.GridColumn grColumn = new DevExpress.XtraGrid.Columns.GridColumn();
                grColumn.Caption = column[1].ToString();
                grColumn.FieldName = column[2].ToString();
                grColumn.Name = column[2].ToString();
                grColumn.Visible = true;
                //grColumn.VisibleIndex = int.Parse(column[0].ToString()) - 1;
                this.gvCfgTemplate.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {grColumn});
            }
        }

        /// <summary>
        /// 初始化GridView的列
        /// </summary>
        /// <param name="JsonDict"></param>
        private void JsonToList(Dictionary<string, List<object>> JsonDict)
        {
            gvCfgTemplate.Columns.Clear();

            foreach (KeyValuePair<string, List<object>> Temp in JsonDict)
            {
                //int count = Temp.Value.Count
                List<object> obj = new List<object>
                {
                    Temp.Value[0],
                    Temp.Value[1],
                    Temp.Key,
                    Temp.Value[2],
                    Temp.Value[3],
                    Temp.Value[4],
                    Temp.Value[5],
                    Temp.Value[6],
                    Temp.Value[7],
                    Temp.Value[8],
                    Temp.Value[9],
                    //Temp.Value[10],
                };
                AddColumns(obj);
            }
            if (_selectedName == "CAN总线配置表")
            {

                gvCfgTemplate.Columns[gvCfgTemplate.Columns.Count - 1].Visible = false;
                gvCfgTemplate.Columns[gvCfgTemplate.Columns.Count - 2].Visible = false;
                GridColumn colNew = new GridColumn();
                colNew.Caption = "事件相关";
                colNew.Name = "CheckItem";
                colNew.FieldName = "CheckItem";
                colNew.Visible = true;


                GridColumn colNewDTC = new GridColumn();
                colNewDTC.Caption = "通信DTC相关";
                colNewDTC.Name = "CheckDTC";
                colNewDTC.FieldName = "CheckDTC";
                colNewDTC.Visible = true;

                gvCfgTemplate.Columns.AddRange(new GridColumn[] {colNew, colNewDTC});

            }
            else if (_selectedName == "LIN总线配置表")
            {
                gvCfgTemplate.Columns[gvCfgTemplate.Columns.Count - 1].Visible = false;
                GridColumn colNew = new GridColumn();
                colNew.Caption = "事件相关";
                colNew.Name = "CheckItem";
                colNew.FieldName = "CheckItem";
                colNew.Visible = true;
                gvCfgTemplate.Columns.Add(colNew);
            }
            else if (_selectedName == "网关路由配置表")
            {
                gvCfgTemplate.Columns[gvCfgTemplate.Columns.Count - 1].Visible = false;
                GridColumn colNew = new GridColumn();
                colNew.Caption = "网关路由表";
                colNew.Name = "CheckItem";
                colNew.FieldName = "CheckItem";
                colNew.Visible = true;
                gvCfgTemplate.Columns.Add(colNew);
            }
            // InitGrid();
        }

        private void NameItem_Click(object sender, EventArgs e)
        {
            _dr = null;
            //判断是否依赖节点
            var item = sender as NavBarItem;
            if (oldBarItem != null && oldBarItem != item)
            {
                oldBarItem.Appearance.ForeColor = Color.Black;

            }
            if (item != null)
            {
                item.Appearance.ForeColor = Color.Blue;
                oldBarItem = item;
                _selectedName = item.Caption;
                if (!GlobalVar.IsIndependent)
                    BtnSubmitImage();
            }

            if (item != null)
            {
                GlobalVar.SelectName = item.Caption;
                _nameItem = true;
                if (_selectedName == "CAN总线配置表")
                {
                    tsmiCfgEvent.Visible = true;
                    tsmiCheckDTC.Visible = true;
                    tsmiRoute.Visible = false;
                }
                else if (_selectedName == "网关路由配置表")
                {
                    tsmiRoute.Visible = true;
                    tsmiCheckDTC.Visible = false;
                    tsmiCfgEvent.Visible = false;
                    //SplashScreenManager.ShowForm(typeof(wfMain), false, true);
                    //tsmiRoute.Visible = false;
                    //tsmiCheckDTC.Visible = false;
                    //tsmiCfgEvent.Visible = false;
                    //var listItems = dpCfgGroup.Items.ToList();
                    //foreach (var litem in listItems)
                    //{
                    //    dpCfgGroup.Remove(litem);
                    //}
                    //gvCfgTemplate.Columns.Clear();
                    //GatewayRoutingConfig grc = new GatewayRoutingConfig();
                    //SplashScreenManager.CloseForm();
                    //grc.ShowDialog();
                    //return;
                }
                else
                {
                    tsmiCfgEvent.Visible = true;
                    tsmiCheckDTC.Visible = false;
                    tsmiRoute.Visible = false;
                }

                GlobalVar.CfgClickItemName = _selectedName;
                //BtnSubmitImage();
                //matchSort.EditValue = _selectedName.Split('配')[0];
                //matchSort.Enabled = false;
                _dt = new DataTable();
                InitEmlPage();
                //InitDpBaud();
                //InitItem();
                //matchSort.EditValue = GlobalVar.ListCfgTemp[0][3].ToString();
                btnEdit.Enabled = true;
            }
            //if (GlobalVar.IsIndependent)
            //{
            //    NameItemTrue();
            //}
            //else
            //{
            //    NameItemFalse();
            //}

            //SaveTime();
            
            UpdateGridViewByOperate();

        }


        private void btnSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (th != null && th.IsAlive)
                th.Abort();
            else
            {
                Show(DLAF.LookAndFeel, this, "配置表不能为空....", "", new[] {DialogResult.OK}, null, 0,
                    MessageBoxIcon.Information);
                return;
            }
            try
            {
                string error = "";
                string json = DtToJson(out error);
                if (error == "")
                {
                    CfgSaveJson(json);
                }

            }
            catch (Exception ex)
            {
                Show(DLAF.LookAndFeel, this, ex.ToString(), "", new[] {DialogResult.OK}, null, 0,
                    MessageBoxIcon.Information);
            }

        }



        private void btnSubmit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_dt.Rows.Count == 0)

            {
                Show(DLAF.LookAndFeel, this, "配置表不能为空....", "", new[] {DialogResult.OK}, null, 0,
                    MessageBoxIcon.Information);
                return;
            }
            try
            {
   
                string error;
                string erro;
                string json = DtToJson(out erro);
                //var jsonBaud = SerBaudToJson(rBaud);
                //var jsonConfig = SerConfigToJson(rConfig, out error);
                _dictRelated["VehicelType"] = GlobalVar.VNode[0];
                _dictRelated["VehicelConfig"] = GlobalVar.VNode[1];
                _dictRelated["VehicelStage"] = GlobalVar.VNode[2];
                _dictRelated["MatchSort"] = _selectedName.Split('配')[0];
                _dictRelated["Topology"] = "";
                _dictRelated["CfgTemplateName"] = _selectedName;
                _dictRelated["CfgTemplate"] = "";
                if (erro != "")
                {
                    MessageBox.Show("提交失败");
                    return;
                }
                _dictRelated["CfgTemplateJson"] = json;

                _dictRelated["CfgBaudJson"] = GetControlTextToJson();

                _dictRelated["EmlTemplateName"] = "";
                _dictRelated["EmlTemplate"] = "";
                _dictRelated["ConTableColEdit"] = Json.SerJson(GlobalVar.ListDrScheme);
                IList<object[]> flList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelRep,
                    _dictRelated);
                if (flList.Count == 0)
                {
                    _dictRelated["TplyDescrible"] = "";
                    _dictRelated["ConTableColEdit"] = Json.SerJson(GlobalVar.ListDrScheme);
                    _dictRelated["EmlTableColEdit"] = "";
                    _store.AddFileLinkByVehicel(_dictRelated, out error);
                    if (error == "")
                    {
                        GlobalVar.CfgTemplateIsOk = true;
                        Show(DLAF.LookAndFeel, this, "提交成功...", "", new[] {DialogResult.OK}, null, 0,
                            MessageBoxIcon.Information);
                        Dictionary<string, object> dictConfig = new Dictionary<string, object>
                        {
                            {"EmployeeNo", GlobalVar.UserNo},
                            {"EmployeeName", GlobalVar.UserName},
                            {"OperTable", "上传配置表"},
                            {"VehicelType", GlobalVar.VNode[0]},
                            {"VehicelConfig", GlobalVar.VNode[1]},
                            {"VehicelStage", GlobalVar.VNode[2]}
                        };

                        Log.WriteLog(EnumLibrary.EnumLog.CfgTemplate, dictConfig);
                        dpBaud.Visibility = DockVisibility.AutoHide;
                        CfgDelJson();
                    }
                    else
                        Show(DLAF.LookAndFeel, this, "提交失败，请联系工程师帮助解决...", "", new[] {DialogResult.OK}, null, 0,
                            MessageBoxIcon.Information);

                }
                else
                {
                    _store.Update(EnumLibrary.EnumTable.FileLinkByVehicelBau, _dictRelated, out error);
                    if (error == "")
                    {
                        GlobalVar.CfgTemplateIsOk = true;
                        Show(DLAF.LookAndFeel, this, "更新成功...", "", new[] {DialogResult.OK}, null, 0,
                            MessageBoxIcon.Information);
                        CfgDelJson();
                        Dictionary<string, object> dictConfig = new Dictionary<string, object>
                        {
                            { "EmployeeNo",GlobalVar.UserNo},
                            {"EmployeeName",GlobalVar.UserName},
                            {"OperTable","上传配置表"},
                            {"VehicelType",   GlobalVar.VNode[0]},
                            {"VehicelConfig",  GlobalVar.VNode[1]},
                            {"VehicelStage",  GlobalVar.VNode[2]}
                      };
                        Log.WriteLog(EnumLibrary.EnumLog.UpdateCfgTemplate, dictConfig);
                        dpBaud.Visibility = DockVisibility.AutoHide;
                    }
                    else
                        Show(DLAF.LookAndFeel, this, "更新失败，请联系工程师帮助解决...", "", new[] {DialogResult.OK}, null, 0,
                            MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Show(DLAF.LookAndFeel, this, ex.ToString(), "", new[] {DialogResult.OK}, null, 0,
                    MessageBoxIcon.Information);
                //Show(DLAF.LookAndFeel, this, "此功能在独立状态下暂未开放...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
            }
            BtnSubmitImage();
        }

        /// <summary>
        /// 添加员工操作记录时 如果记录是Dictionary类型可直接调用这个方法
        /// </summary>
        /// <param name="Oper">操作类型</param>
        /// <returns></returns>





        /// <summary>
        /// 初始化左侧的配置表树并且将配置表加载到全局变量中
        /// </summary>
        void ITemplate.DrawNav()
        {
            nbcTempList.Items.Clear();
            GlobalVar.ListCfgTemp.Clear();
            //得到所有模板表中的模板信息
            IList<object[]> configList = new List<object[]>();
            IList<object[]> cfgList = new List<object[]>();
            IList<object[]> dbcList = new List<object[]>();
            bool CAN = false;
            bool LIN = false;
            configList = _store.GetRegularByEnum(EnumLibrary.EnumTable.ConfigTemp);
            if (!GlobalVar.IsIndependent)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict["VehicelType"] = GlobalVar.VNode[0];
                dict["VehicelConfig"] = GlobalVar.VNode[1];
                dict["VehicelStage"] = GlobalVar.VNode[2];
                dbcList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.DBC, dict);
                foreach (var List in dbcList)
                {
                    if (List[4].ToString().Substring(0, 3) == "CAN")
                    {
                        CAN = true;
                    }
                    else if (List[4].ToString().Substring(0, 3) == "LIN")
                    {
                        LIN = true;
                    }
                }
            }
            var row = 0;
            foreach (var list in configList)
            {
                if (GlobalVar.ListCfgTemp.Count != 0)
                {
                    cfgList.Clear();
                    foreach (var eml in GlobalVar.ListCfgTemp)
                        cfgList.Add(eml);
                    bool same = false;
                    foreach (var newList in cfgList)
                    {
                        if (list[0].ToString() == newList[0].ToString())
                        {
                            if (list[1].ToString() == "V2.0")
                            {
                                var index = GlobalVar.ListCfgTemp.IndexOf(newList);
                                GlobalVar.ListCfgTemp[index] = list;
                                same = true;
                                break;
                            }
                        }

                    }
                    if (!same)
                        GlobalVar.ListCfgTemp.Add(list);
                }
                else if (GlobalVar.ListCfgTemp.Count == 0)
                {
                    GlobalVar.ListCfgTemp.Add(list);
                }
            }
            GlobalVar.DictCfgTemp.Clear();
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
                if (!GlobalVar.IsIndependent)
                {
                    if (_dictConfig["Name"].ToString() == "CAN总线配置表")
                    {
                        if (CAN)
                        {
                            temList.AddItem();
                            nbcTempList.Items[row].Caption = _dictConfig["Name"].ToString();
                            nbcTempList.Items[row].Appearance.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                            nbcTempList.Items[row].AppearancePressed.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                            nbcTempList.Items[row].LinkClicked += NameItem_Click;
                            row++;
                            Dictionary<string, object> cfgDictionary = new Dictionary<string, object>();
                            cfgDictionary["Name"] = _dictConfig["Name"];
                            cfgDictionary["Version"] = _dictConfig["Version"];
                            cfgDictionary["Content"] = _dictConfig["Content"];
                            cfgDictionary["ImportDate"] = _dictConfig["ImportDate"];
                            GlobalVar.DictCfgTemp.Add(cfgDictionary);
                        }
                    }
                    if (_dictConfig["Name"].ToString() == "LIN总线配置表")
                    {
                        if (LIN)
                        {
                            temList.AddItem();
                            nbcTempList.Items[row].Caption = _dictConfig["Name"].ToString();
                            nbcTempList.Items[row].Appearance.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                            nbcTempList.Items[row].AppearancePressed.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                            nbcTempList.Items[row].LinkClicked += NameItem_Click;
                            row++;
                            Dictionary<string, object> cfgDictionary = new Dictionary<string, object>();
                            cfgDictionary["Name"] = _dictConfig["Name"];
                            cfgDictionary["Version"] = _dictConfig["Version"];
                            cfgDictionary["Content"] = _dictConfig["Content"];
                            cfgDictionary["ImportDate"] = _dictConfig["ImportDate"];
                            GlobalVar.DictCfgTemp.Add(cfgDictionary);
                        }
                    }
                    if (_dictConfig["Name"].ToString() == "网关路由配置表")
                    {
                        if (dbcList.Count >= 2)
                        {
                            temList.AddItem();
                            nbcTempList.Items[row].Caption = _dictConfig["Name"].ToString();
                            nbcTempList.Items[row].Appearance.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                            nbcTempList.Items[row].AppearancePressed.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                            nbcTempList.Items[row].LinkClicked += NameItem_Click;
                            row++;
                            Dictionary<string, object> cfgDictionary = new Dictionary<string, object>();
                            cfgDictionary["Name"] = _dictConfig["Name"];
                            cfgDictionary["Version"] = _dictConfig["Version"];
                            cfgDictionary["Content"] = _dictConfig["Content"];
                            cfgDictionary["ImportDate"] = _dictConfig["ImportDate"];
                            GlobalVar.DictCfgTemp.Add(cfgDictionary);
                        }
                    }
                }
                else
                {
                    temList.AddItem();
                    nbcTempList.Items[row].Caption = _dictConfig["Name"].ToString();
                    nbcTempList.Items[row].Appearance.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                    nbcTempList.Items[row].AppearancePressed.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                    nbcTempList.Items[row].LinkClicked += NameItem_Click;
                    row++;
                    Dictionary<string, object> cfgDictionary = new Dictionary<string, object>();
                    cfgDictionary["Name"] = _dictConfig["Name"];
                    cfgDictionary["Version"] = _dictConfig["Version"];
                    cfgDictionary["Content"] = _dictConfig["Content"];
                    cfgDictionary["ImportDate"] = _dictConfig["ImportDate"];
                    GlobalVar.DictCfgTemp.Add(cfgDictionary);
                }

            }
            if (GlobalVar.SelectName != "")
            {
                string nameExap = GlobalVar.SelectName.Split('用')[0];
                nameExap = nameExap + "用例表";
                foreach (NavBarItem item in nbcTempList.Items)
                {
                    if (item.Caption == nameExap)
                    {
                        item.Appearance.ForeColor = Color.Blue;
                        oldBarItem = item;
                    }
                }
            }
        }

        void ITemplate.InitDict()
        {
            _dictRelated.Add("VehicelType", "");
            _dictRelated.Add("VehicelConfig", "");
            _dictRelated.Add("VehicelStage", "");
            _dictRelated.Add("MatchSort", "");
            _dictRelated.Add("Topology", "");
            _dictRelated.Add("CfgTemplateName", "");
            _dictRelated.Add("CfgTemplate", "");
            _dictRelated.Add("CfgTemplateJson", "");
            _dictRelated.Add("BaudJson", "");
            _dictRelated.Add("EmlTemplateName", "");
            _dictRelated.Add("EmlTemplate", "");

            _dictConfig.Add("Name", "");
            _dictConfig.Add("Version", "");
            _dictConfig.Add("Content", null);
            _dictConfig.Add("ImportDate", "");

            //_dictBaud.Add("CAN1","");
            //_dictBaud.Add("CAN2", "");
            //_dictBaud.Add("CAN3", "");
            //_dictBaud.Add("CAN4", "");

        }

        /// <summary>
        /// 用于保存配置表的临时变量，防止未保存就关掉页面
        /// </summary>
        private void CfgSaveJson(string json)
        {
            Dictionary<string, Dictionary<string, string>> dictdictCfgJson = GlobalVar.DictdictCfgJson;
            if (GlobalVar.VNode.Count < 3)
                return;
            string strVehicle = GlobalVar.VNode[0] + "-" + GlobalVar.VNode[1] + "-" + GlobalVar.VNode[2];
            if (!dictdictCfgJson.ContainsKey(strVehicle))
            {
                dictdictCfgJson[strVehicle] = new Dictionary<string, string>();
            }
            if (!dictdictCfgJson[strVehicle].ContainsKey(_selectedName))
            {
                dictdictCfgJson[strVehicle][_selectedName] = string.Empty;
            }
            dictdictCfgJson[strVehicle][_selectedName] = json;
            GlobalVar.DictdictCfgJson = dictdictCfgJson;
            BtnSubmitImage();
        }

        private string CfgReadJson()
        {
            Dictionary<string, Dictionary<string, string>> dictdictCfgJson = GlobalVar.DictdictCfgJson;
            if (GlobalVar.VNode.Count < 3)
                return string.Empty;
            string strVehicle = GlobalVar.VNode[0] + "-" + GlobalVar.VNode[1] + "-" + GlobalVar.VNode[2];
            if (!dictdictCfgJson.ContainsKey(strVehicle))
            {
                return string.Empty;
            }
            if (!dictdictCfgJson[strVehicle].ContainsKey(_selectedName))
            {
                return string.Empty;
            }
            return dictdictCfgJson[strVehicle][_selectedName];
        }

        private void CfgDelJson()
        {
            Dictionary<string, Dictionary<string, string>> dictdictCfgJson = GlobalVar.DictdictCfgJson;
            if (GlobalVar.VNode.Count < 3)
                return;
            string strVehicle = GlobalVar.VNode[0] + "-" + GlobalVar.VNode[1] + "-" + GlobalVar.VNode[2];
            if (!dictdictCfgJson.ContainsKey(strVehicle))
            {
                dictdictCfgJson[strVehicle] = new Dictionary<string, string>();
            }
            if (!dictdictCfgJson[strVehicle].ContainsKey(_selectedName))
            {
                dictdictCfgJson[strVehicle][_selectedName] = string.Empty;
            }
            dictdictCfgJson[strVehicle][_selectedName] = string.Empty;
            GlobalVar.DictdictCfgJson = dictdictCfgJson;
            BtnSubmitImage();
        }

        /// <summary>
        /// 从配置表模板全局变量中选择当前配置表模板
        /// </summary>
        /// <param name="temName"></param>
        void ITemplate.DrawTem(string temName)
        {
            bool isFind = false;
            foreach (var dict in GlobalVar.DictCfgTemp)
            {
                if (dict["Name"].ToString() != temName)
                {
                    continue;
                }
                GlobalVar.ListDrScheme = Json.DeserJsonDList(dict["Content"].ToString());
                GlobalVar.VersionCfgTem = dict["Version"].ToString();
                if (GlobalVar.ListDrScheme == null)
                    gcCfgTemplate.DataSource = null;
                else
                {
                    JsonToList(GlobalVar.ListDrScheme);
                    isFind = true;
                    //break;
                }
                //var byteTemp = dict["Content"] as byte[];
                //ctlExcelConfig.Document.LoadDocument(byteTemp, DocumentFormat.Xlsx);
                //_sheet = ctlExcelConfig.ActiveWorksheet;
            }
            if (!isFind)
            {
                //th.Abort();
                Show(DLAF.LookAndFeel, this, "数据库异常...", "", new[] {DialogResult.OK}, null, 0,
                    MessageBoxIcon.Information);
            }

        }

        void ITemplate.SetAttribute(List<string> nodeList)
        {
            if (nodeList != null)
            {
                if (nodeList.Count != 0)
                {
                    txtBelongNode.Caption = @"所属节点：" + nodeList[0] + @"-" + nodeList[1] + @"-" + nodeList[2];
                    GlobalVar.IsIndependent = false;
                }
                else
                {
                    txtBelongNode.Caption = @"无所属节点";
                    GlobalVar.IsIndependent = true;
                }
            }
            else
            {
                txtBelongNode.Caption = @"无所属节点";
                GlobalVar.IsIndependent = true;
            }
        }

        public static DialogResult Show(UserLookAndFeel look, IWin32Window owner, string text, string caption,
            DialogResult[] buttons, Icon icon, int defaultButton, MessageBoxIcon messageBeepSound)
        {
            XtraMessageBoxForm form = new XtraMessageBoxForm();
            Font defaultFont = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            form.Appearance.Font = defaultFont;
            return
                form.ShowMessageBoxDialog(new XtraMessageBoxArgs(look, owner, text, caption, buttons, icon,
                    defaultButton));
        }

        private void btnEdit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!GlobalVar.IsIndependent)
            {
                List<string> listV = new List<string>
                {
                    GlobalVar.VNode[0],
                    GlobalVar.VNode[1],
                    GlobalVar.VNode[2],
                    _selectedName.Split('配')[0]
                };
                IList<object[]> file = ReadDataByVehicel(listV);
                if (file.Count != 0 && file[0][7].ToString() != "")
                {
                    if (
                        XtraMessageBox.Show("该车型已经上传过配置表，再次进行列编辑的话数据库中的数据将被删除，是否是否继续列编辑？", "提示",
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning) ==
                        DialogResult.OK)
                    {
                        Dictionary<string, object> dict = new Dictionary<string, object>
                        {
                            {"EmployeeName", GlobalVar.UserName},
                            {"EmployeeNo", GlobalVar.UserNo},
                            {"Department", GlobalVar.UserDept},

                            {"VehicelType", GlobalVar.VNode[0]},
                            {"VehicelConfig", GlobalVar.VNode[1]},
                            {"VehicelStage", GlobalVar.VNode[2]},
                            {"CfgTemplateName", ""},
                            {"CfgTemplate", ""},
                            {"CfgTemplateJson", ""},
                        };
                        string error = "";
                        _store.Update(EnumLibrary.EnumTable.FileLinkByVehicelCfg, dict, out error);
                        ShowColEdit();
                    }
                }
                else
                {
                    ShowColEdit();
                }
            }
            else
            {
                ShowColEdit();
            }
        }
        private void ShowColEdit()
        {
            CfgColEdit Edit = new CfgColEdit();
            Edit.ShowDialog(this);

            if (GlobalVar.IsColumEdit && !GlobalVar.IsIndependent)
            {
                List<string> listV = new List<string>
                {
                    GlobalVar.VNode[0],
                    GlobalVar.VNode[1],
                    GlobalVar.VNode[2],
                    _selectedName.Split('配')[0]
                };
                IList<object[]> file = ReadDataByVehicel(listV);
                if (file.Count != 0)
                {
                    var confiCol = Json.DeserJsonDList(file[0][12].ToString());
                    //foreach(KeyValuePair<string,List<object>> list in colEdit)
                    JsonToList(confiCol);
                    foreach (var content in GlobalVar.DictCfgTemp)
                    {
                        if (content["Name"].ToString() == _selectedName)
                        {
                            content["Content"] = confiCol;
                            break;
                        }
                    }
                    CreateDockPanel(confiCol);
                    AllowColumSort();
                    //AddColumns(list.Value);
                    //CreateGridView(colEdit);
                    InitGrid();
                    GlobalVar.IsColumEdit = false;
                    btnEdit.Enabled = false;
                }
            }
            else if (GlobalVar.IsIndependent && GlobalVar.IsColumEdit)
            {
                if (GlobalVar.SelectName != "")
                {
                    _tem.DrawNav();
                    if (_selectedName != "")
                        _tem.DrawTem(_selectedName);
                }
            }
            //GlobalVar.SelectName = "";
        }

        private void CreateDockPanel(Dictionary<string, List<object>> colDictionary)
        {
            int i = 0;
            dpCfgGroup.Items.Clear();
            //layoutControl3.Controls.Clear();
            LcItems.Clear();
            layoutControl6.Controls.Clear();
            foreach (KeyValuePair<string, List<object>> scheme in colDictionary)
            {
                //try
                {
                    if (bool.Parse(scheme.Value[2].ToString()))
                    {
                        if (bool.Parse(scheme.Value[5].ToString()))
                        {
                            CreateComboBox(scheme, i);
                        }
                        else
                        {
                            if (scheme.Key == "DUTname" || scheme.Key == "TestChannel" || scheme.Key == "SlaveboxID")
                            {
                                CreateComboBox(scheme, i);
                            }
                            else if (scheme.Key == "VirNodeID")
                            {
                                CreateVirNodeIDButtonEdit(scheme, i);
                            }
                            else if (scheme.Key == "CddFileName")
                            {
                                CreateSelectFileControl(scheme, i);
                            }
                            else if (scheme.Key == "VirMsgNum")
                            {
                                CreateSpinEdit(scheme, i);
                            }
                            else if (scheme.Key == "EventRelevant" || scheme.Key == "DTCRelevant"|| scheme.Key == "GatewayPath")
                            {
                                continue;
                            }
                            else
                                CreateTextEdit(scheme, i);
                        }
                        i++;
                    }
                }
                //catch (Exception)
                //{
                //    throw;
                //}
            }
            int count = LcItems.Count;
            CreateButton(count);
            ContextMenu emptyMenu = new ContextMenu();
            foreach (LayoutControlItem item in LcItems)
            {
                dpCfgGroup.AddItem(item);
                if (typeof(TextEdit) == item.Control.GetType())
                {
                    TextEdit text = item.Control as TextEdit;
                    text.Properties.ContextMenu = emptyMenu;
                }
            }
        }

        private void CreateTextEdit(KeyValuePair<string, List<object>> scheme, int y)
        {
            TextEdit te = new TextEdit();
            LayoutControlItem lc = new LayoutControlItem();
            // textEdit
            te.Name = scheme.Key;
            te.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            // LayoutControlItem
            lc.Control = te;
            //lc.Name = "LC" + DictTextEdit["EngName"];
            lc.Text = scheme.Value[1] + @"：";
            //lc.TextLocation = DevExpress.Utils.Locations.Left;
            lc.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 5, 5);
            lc.Location = new System.Drawing.Point(0, y*28);
            //lc.Size = new System.Drawing.Size(100, 25);
            lc.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            //lc.TextAlignMode = TextAlignModeItem.AutoSize;
            //绑定
            LcItems.Add(lc);
            //this.layoutControlGroup.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] { lc });
        }
        private void CreateSpinEdit(KeyValuePair<string, List<object>> scheme, int y)
        {
            SpinEdit se = new SpinEdit();
            LayoutControlItem lc = new LayoutControlItem();
            se.Name = scheme.Key;
            se.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            se.Properties.MinValue = 0;
            se.Properties.MaxValue = 999;
            se.Properties.MaxLength = 3;
            se.Properties.IsFloatValue = false;
            se.Properties.Increment = 1;
            se.EditValueChanging += spinEdit_EditValueChanging;
            lc.Control = se;
            lc.Text = scheme.Value[1] + @"：";
            lc.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 5, 5);
            lc.Location = new System.Drawing.Point(0, y * 28);
            lc.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //绑定
            LcItems.Add(lc);
        }

        private void CreateComboBox(KeyValuePair<string, List<object>> scheme, int y)
        {
            ComboBoxEdit combedit = new ComboBoxEdit();
            LayoutControlItem lc = new LayoutControlItem();
            // textEdit
            combedit.Name = scheme.Key;
            combedit.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            combedit.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            //combedit.TextChanged += ComboBoxEdit_TextChanged;
            // LayoutControlItem
            lc.Control = combedit;
            //lc.Name = "LC" + DictTextEdit["EngName"];
            lc.Text = scheme.Value[1] + @"：";

            //lc.TextLocation = DevExpress.Utils.Locations.Left;
            lc.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 5, 5);
            lc.Location = new System.Drawing.Point(0, y*28);
            lc.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            //lc.TextAlignMode = TextAlignModeItem.AutoSize;
            //var itemString = scheme.Value[9].ToString().Split(','); 
            //combedit.Properties.Items.AddRange(itemString);

            //绑定
            LcItems.Add(lc);
            if (scheme.Key == "SourceSegment" || scheme.Key == "TargetSegment")
            {
                if (scheme.Key == "SourceSegment")
                {
                    combedit.SelectedValueChanged += Gateway_SelectedValueChanged;
                    _dictCBEGateway["SourceSegment"] = combedit;
                }
                else
                {
                    combedit.SelectedValueChanged += Gateway_SelectedValueChanged;
                    _dictCBEGateway["TargetSegment"] = combedit;
                }
                Dictionary<string, string> vehicel = new Dictionary<string, string>();
                vehicel.Add("VehicelType", GlobalVar.VNode[0]);
                vehicel.Add("VehicelConfig", GlobalVar.VNode[1]);
                vehicel.Add("VehicelStage", GlobalVar.VNode[2]);
                //List<string> propItem = new List<string>();
                IList<object[]> can = BaseSqlOrder.GetMultipleByQuery(ExcuteSqlCase.DBC_Query(vehicel));
                foreach (var item in can)
                {
                    if (item[4].ToString().Substring(0, 3).Trim().ToUpper() == "CAN")
                    {
                        combedit.Properties.Items.Add(item[4].ToString());
                        //propItem.Add(item[4].ToString());
                    }
                }
                //CbmPropItem[scheme.Key] = propItem;
            }
            else if (scheme.Key == "Gateway")
            {
                _dictCBEGateway["Gateway"] = combedit;
            }
            else if (scheme.Key == "TestChannel")
            {
                //var segment = _store.GetSingnalCol(EnumLibrary.EnumTable.Segment , 0);
                //object[] seg = segment.ToArray();
                //combedit.Properties.Items.AddRange(seg);
                combedit.SelectedValueChanged += TestChannel_SelectedValueChanged;
                Dictionary<string, string> vehicel = new Dictionary<string, string>();
                vehicel.Add("VehicelType", GlobalVar.VNode[0]);
                vehicel.Add("VehicelConfig", GlobalVar.VNode[1]);
                vehicel.Add("VehicelStage", GlobalVar.VNode[2]);
                IList<object[]> can = BaseSqlOrder.GetMultipleByQuery(ExcuteSqlCase.DBC_Query(vehicel));
                int i = 0;
                //List<string> propItem = new List<string>();
                foreach (var item in can)
                {
                    if (_selectedName.Substring(0, 3).Trim().ToUpper() ==
                        item[4].ToString().Substring(0, 3).Trim().ToUpper() ||
                        _selectedName.Substring(0, 3).Trim().ToUpper() == "J19"|| _selectedName=="网关路由配置表")
                    {
                        combedit.Properties.Items.Add(item[4].ToString());
                        //propItem.Add(item[4].ToString());
                        i++;
                    }
                }
                //CbmPropItem[scheme.Key] = propItem;
            }
            else if (scheme.Key == "DUTname")
            {
                //if (nodeList.Count != 0)
                combEdit = new ComboBoxEdit();
                combEdit = combedit;
                combedit.SelectedValueChanged += DUTname_SelectedValueChanged;
                //combedit.Properties.Items.AddRange(nodeList);
            }
            else if (scheme.Key == "SlaveboxID")
            {
                IList<object[]> boxID = _store.GetRegularByEnum(EnumLibrary.EnumTable.NodeConfigurationBox);
                //List<string> propItem = new List<string>();
                foreach (var list in boxID)
                {
                    combedit.Properties.Items.Add(list[0]);
                    //propItem.Add(list[0].ToString());
                }
                //CbmPropItem[scheme.Key] = propItem;
            }
            else
            {
                //List<string> propItem = new List<string>();
                if (scheme.Key == "MasterNodeType")
                {
                    cbeMasterNodeType = new ComboBoxEdit();
                    cbeMasterNodeType = combedit;
                    cbeMasterNodeType.ReadOnly = true;
                }
                if (scheme.Key == "NodeNetworkAttribute")
                {
                    combedit.SelectedValueChanged += NodeNetworkAttribute_SelectedValueChanged;
                }
                if (scheme.Key == "DiagnosticNode")
                {
                    combedit.SelectedValueChanged += DiagnosticNode_SelectedValueChanged;
                }
                //object[] objItem = new object[] {scheme.Value[6].ToString().Split(',').ToArray()};
                //List<string> nodeList = objItem[0].ToString()
                if (scheme.Key != "TestChannel")
                {
                    combedit.Properties.Items.AddRange(scheme.Value[6].ToString().Split(',').ToArray());
                    //propItem = scheme.Value[6].ToString().Split(',').ToList<string>();
                    //CbmPropItem[scheme.Key] = propItem;
                } 
            }
            //this.layoutControlGroup.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] { lc });
        }

        private void CreateSelectFileControl(KeyValuePair<string, List<object>> scheme, int y)
        {
            ButtonEdit btnEdit = new ButtonEdit();
            LayoutControlItem lc = new LayoutControlItem();
            btnEdit.Name = scheme.Key;
            btnEdit.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lc.Control = btnEdit;
            btnEdit.ButtonClick += SelectFileControl_ButtonClick;

            lc.Text = scheme.Value[1] + @"：";
            lc.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 5, 5);
            lc.Location = new System.Drawing.Point(0, y * 28);
            lc.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            LcItems.Add(lc);
        }

        private void CreateVirNodeIDButtonEdit(KeyValuePair<string, List<object>> scheme, int y)
        {
            ButtonEdit btnEdit = new ButtonEdit();
            LayoutControlItem lc = new LayoutControlItem();
            btnEdit.Name = "btn" + scheme.Key;
            btnEdit.Font= new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lc.Control = btnEdit;
            btnEdit.Properties.Buttons.Clear();
            btnEdit.Properties.Buttons.AddRange(new EditorButton[] { new EditorButton(ButtonPredefines.Plus), new EditorButton(ButtonPredefines.Minus) });
            btnEdit.ButtonClick += VirNodeIDButtonEdit_ButtonClick;

            lc.Text = scheme.Value[1] + @"：";
            lc.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 5, 5);
            lc.Location = new System.Drawing.Point(0, y * 28);
            lc.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));


            ListBoxControl lbc = new ListBoxControl();
            LayoutControlItem lci = new LayoutControlItem();
            // textEdit
            lbc.Name = scheme.Key;
            lbc.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lbc.MouseDown += ListBoxControl_MouseDown;
            lbc.MinimumSize = new Size(0, 80);
            lbc.SelectedValueChanged += ListBoxControl_SelectedValueChanged;
            // LayoutControlItem
            lci.Control = lbc;
            //lci.Text = scheme.Value[1] + @"：";
            lci.TextVisible = false;
            lci.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 5, 5);
            lci.Location = new System.Drawing.Point(0, y * 28);
            //lc.Size = new System.Drawing.Size(100, 25);
            lci.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //绑定
            LcItems.Add(lc);
            LcItems.Add(lci);
        }
        
        private void CreateButton(int y)
        {
            SimpleButton simpleButton = new SimpleButton();
            LayoutControlItem simpleButtonLCI = new LayoutControlItem();
            // 
            // simpleButton
            // 
            simpleButton.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            simpleButton.Appearance.Options.UseFont = true;
            //simpleButton.AutoWidthInLayoutControl = true;
            simpleButton.Size = new System.Drawing.Size(260, 28);
            simpleButton.Name = "simpleButton";
            simpleButton.Text = @"确定";
            simpleButton.Location = new System.Drawing.Point(0, y*28);
            simpleButton.Margin = new System.Windows.Forms.Padding(3, 3, 5, 5);
            simpleButton.MinimumSize = new Size(0, 28);
            // 
            // simpleButtonLCI
            // 
            simpleButtonLCI.Control = simpleButton;
            simpleButtonLCI.ControlAlignment = System.Drawing.ContentAlignment.BottomCenter;
            simpleButtonLCI.Location = new System.Drawing.Point(0, y*28);
            simpleButtonLCI.Name = "simpleButtonLCI";
            simpleButtonLCI.Text = "simpleButtonLCI";
            simpleButtonLCI.TextVisible = false;

            simpleButton.Click += button_Click;
            LcItems.Add(simpleButtonLCI);
            //this.dpCfgGroup.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] { simpleButtonLCI });
        }



        private void NodeNetworkAttribute_SelectedValueChanged(object sender, EventArgs e)
        {
            ComboBoxEdit cmbNodeNetworkAttribute = sender as ComboBoxEdit;
            LayoutControlItem lcNMStationAddress =new LayoutControlItem();
            LayoutControlItem lcNMBaseAddress = new LayoutControlItem();
            LayoutControlItem lcVirNodeID = new LayoutControlItem();
            LayoutControlItem lcbtnVirNodeID = new LayoutControlItem();
            foreach (LayoutControlItem conItem in LcItems)
            {
                if (conItem.Control.Name == "NMStationAddress")
                {
                    lcNMStationAddress = conItem;
                }
                else if (conItem.Control.Name == "NMBaseAddress")
                {
                    lcNMBaseAddress = conItem;
                }
                else if (conItem.Control.Name == "VirNodeID")
                {
                    lcVirNodeID = conItem;
                }
                else if (conItem.Control.Name == "btnVirNodeID")
                {
                    lcbtnVirNodeID = conItem;
                }
            }
            if (cmbNodeNetworkAttribute.EditValue.ToString() == "间接NM")
            {
                lcNMStationAddress.Control.Text = null;
                lcNMStationAddress.Control.Enabled = false;
                lcNMBaseAddress.Control.Text = null;
                lcNMBaseAddress.Control.Enabled = false;
                lcVirNodeID.Control.Text = null;
                lcVirNodeID.Control.Enabled = false;
                if (typeof(ListBoxControl) == lcVirNodeID.Control.GetType())
                {
                    var VirNodeID = (ListBoxControl)lcVirNodeID.Control;
                    VirNodeID.Items.Clear();
                }
                lcbtnVirNodeID.Control.Enabled = false;
            }
            else
            {
                lcNMStationAddress.Control.Enabled = true;
                lcNMBaseAddress.Control.Enabled = true;
                lcVirNodeID.Control.Enabled = true;
                lcbtnVirNodeID.Control.Enabled = true;
            }
        }
        private void DiagnosticNode_SelectedValueChanged(object sender, EventArgs e)
        {
            ComboBoxEdit cmbNodeNetworkAttribute = sender as ComboBoxEdit;
            LayoutControlItem lcCddFileName = new LayoutControlItem();
            LayoutControlItem lcRequestID = new LayoutControlItem();
            LayoutControlItem lcResponseID = new LayoutControlItem();
            foreach (LayoutControlItem conItem in LcItems)
            {
                if (conItem.Control.Name == "CddFileName")
                {
                    lcCddFileName = conItem;
                }
                else if (conItem.Control.Name == "RequestID")
                {
                    lcRequestID = conItem;
                }
                else if (conItem.Control.Name == "ResponseID")
                {
                    lcResponseID = conItem;
                }
            }
            if (cmbNodeNetworkAttribute.EditValue.ToString() == "否")
            {
                lcCddFileName.Control.Text = null;
                lcCddFileName.Control.Enabled = false;
                lcRequestID.Control.Text = null;
                lcRequestID.Control.Enabled = false;
                lcResponseID.Control.Text = null;
                lcResponseID.Control.Enabled = false;
            }
            else
            {
                lcCddFileName.Control.Enabled = true;
                lcRequestID.Control.Enabled = true;
                lcResponseID.Control.Enabled = true;
            }
        }

        private void Gateway_SelectedValueChanged(object sender, EventArgs e)
        {
            if (_dictCBEGateway["SourceSegment"].Text == "" || _dictCBEGateway["TargetSegment"].Text == "")
                return;
            
            List<string> Listdbc = new List<string>();
            Listdbc.Add(GlobalVar.VNode[0]);
            Listdbc.Add(GlobalVar.VNode[1]);
            Listdbc.Add(GlobalVar.VNode[2]);
            Listdbc.Add("");
            bool IsExistDBC;
            Listdbc[3] = _dictCBEGateway["SourceSegment"].Text;
            List<string> SourceList = _show.GetDataFromDbc(Listdbc,out IsExistDBC);
            Listdbc[3] = _dictCBEGateway["TargetSegment"].Text;
            List<string> TargetList = _show.GetDataFromDbc(Listdbc,out IsExistDBC);

            //网关路由配置表 来源网段跟目标网段筛选 取并集
            foreach (var target in TargetList)
            {
                if (!SourceList.Contains(target))
                {
                    SourceList.Add(target);
                }
            }
            SourceList.Sort();
            _dictCBEGateway["Gateway"].Properties.Items.AddRange(SourceList);

            //网关路由配置表 来源网段跟目标网段筛选 取交集
            //foreach(var source in SourceList)
            //{
            //    foreach(var target in TargetList)
            //    {
            //        if (source != target)
            //            continue;
            //        _dictCBEGateway["Gateway"].Properties.Items.Add(source);
            //    }
            //}
        }

        private void DUTname_SelectedValueChanged(object sender, EventArgs e)
        {
            var item = sender as ComboBoxEdit;
            string canText = "";
            foreach (LayoutControlItem conItem in LcItems)
            {
                if (conItem.Control.Name == "TestChannel")
                {
                    canText = conItem.Control.Text;
                    break;
                }
            }
            if(canText == "")
            {
                MessageBox.Show("请先选择从属网段...");
                item.SelectedIndex = -1;
                return;
             }
            List<string> dbc = new List<string>();
            dbc.Add(GlobalVar.VNode[0]);
            dbc.Add(GlobalVar.VNode[1]);
            dbc.Add(GlobalVar.VNode[2]);
            dbc.Add(canText);
            //List<string> nodeList = _show.GetDataFromDbc(dbc);

            if (!(canText.Substring(0, 3).ToUpper().Trim() == "LIN"))
            {
                var dbcList = _store.GetDBCListByVNodeAndCAN(dbc);
                List<string> modules = new List<string>();
                string path = "";
                foreach (var can in dbcList)
                {
                    path = AppDomain.CurrentDomain.BaseDirectory + can[5];
                }
                int i = 0;
                var oskeBaseID = _osek.GetNMBaseAddressFromDBC(ref i, path);
                var osekMsgID = _osek.GetNMStationAddressFromDBC(ref i, path, item.Text);

                Dictionary<string, string> dictRequestID = new Dictionary<string, string>();
                Dictionary<string, string> dictResponseID = new Dictionary<string, string>();
                var requestIDArray = _dbcAnalysis.GetNodeReciveMessage(path, item.Text);//获取诊断请求ID数组
                var responseIDArray = _dbcAnalysis.GetNodeSendMessage(path, item.Text);//获取诊断响应ID数组
                for (int j = 0; j < 500; j++)
                {
                    if (!(requestIDArray[j, 0] == "" || requestIDArray[j, 1] == ""))
                        dictRequestID[requestIDArray[j, 0]] = requestIDArray[j, 1];//诊断请求ID
                    if (!(responseIDArray[j, 0] == "" || responseIDArray[j, 1] == ""))
                        dictResponseID[responseIDArray[j, 0]] = responseIDArray[j, 1];//诊断响应ID
                }
                var requestID = GetHexIntermediateValue("0x700", "0x7FF", dictRequestID.Values.ToList());
                var responseID = GetHexIntermediateValue("0x700", "0x7FF", dictResponseID.Values.ToList());

                foreach (LayoutControlItem conItem in LcItems)
                {
                    if (conItem.Control.Name == "NMStationAddress")
                    {
                        if (conItem.Control.Enabled == true)
                            conItem.Control.Text = osekMsgID;
                    }
                    else if (conItem.Control.Name == "NMBaseAddress")
                    {
                        if (conItem.Control.Enabled == true)
                            conItem.Control.Text = oskeBaseID;
                    }
                    else if (conItem.Control.Name == "RequestID")
                    {
                        if (conItem.Control.Enabled == true)
                            conItem.Control.Text = requestID;
                    }
                    else if (conItem.Control.Name == "ResponseID")
                    {
                        if (conItem.Control.Enabled == true)
                            conItem.Control.Text = responseID;
                    }
                }
            }
            else
            {
                var dbcList = _store.GetDBCListByVNodeAndCAN(dbc);
                foreach (var dbcitem in dbcList)
                {
                    var exValue = 0;
                    var dbcName = dbcitem[3].ToString();
                    var path = AppDomain.CurrentDomain.BaseDirectory + dbcitem[5];
                    LayoutControlItem lciCrystal=new LayoutControlItem();
                    Dictionary<string, string[]> dict = new Dictionary<string, string[]>();
                    dict = _show.GetDataFromLdf(dbc);
                    var slaveId = _show.GetSlaveIDFromLdf(dbc, item.Text);
                    foreach (LayoutControlItem conItem in LcItems)
                    {
                        if (conItem.Control.Name == "DUTID")
                        {
                            conItem.Control.Text = slaveId;
                            break;
                        }
                        if (conItem.Control.Name == "Crystal")
                        {
                            lciCrystal = conItem;
                        }
                    }
                    foreach (var nodetype in dict)
                    {
                        if (nodetype.Key == item.Text)
                        {
                            if (nodetype.Value[1].ToLower().Trim() == "yes")
                            {
                                cbeMasterNodeType.EditValue = "主节点";
                                lciCrystal.Control.Enabled = false;
                                lciCrystal.Control.Text = string.Empty;
                            }
                            else
                            {
                                cbeMasterNodeType.EditValue = "从节点";
                                lciCrystal.Control.Enabled = true;
                                lciCrystal.Control.Text = string.Empty;
                            }
                        }
                    }
                }
            }
        }

        private void TestChannel_SelectedValueChanged(object sender, EventArgs e)
        {
            var item = sender as ComboBoxEdit;
            
            List<string> dbc = new List<string>();
            dbc.Add(GlobalVar.VNode[0]);
            dbc.Add(GlobalVar.VNode[1]);
            dbc.Add(GlobalVar.VNode[2]);
            dbc.Add(item.Text);
            bool IsExistDBC;
            List<string> nodeList = _show.GetDataFromDbc(dbc,out IsExistDBC);

            var dbcList = _store.GetDBCListByVNodeAndCAN(dbc);

            if (dbcList.Count == 0)
            {
                MessageBox.Show("当前车型没有上传：" + item.Text + "对应的DBC文件，请先上传相应的DBC文件");
                item.SelectedIndex = -1;
                return;
            }
            foreach (var layoutItem in LcItems)
            {
                var control = layoutItem.Control;
                combEdit.Properties.Items.Clear();
                if (layoutItem.Control.Name == "DUTname"&& typeof(ComboBoxEdit) == layoutItem.Control.GetType())
                {
                    nodeList.Remove(string.Empty);
                    nodeList.Remove(" ");
                    combEdit.Properties.Items.AddRange(nodeList);
                    break;
                }
            }
            //item.Properties.Items.AddRange(nodeList);
        }

        /// <summary>
        /// 取十六进制两数之间的值，返回时如果为多个数值则排除掉0x7DF，如果只有一个数值直接返回即可
        /// </summary>
        /// <param name="strHexMin">最小值</param>
        /// <param name="strHexMax">最大值</param>
        /// <param name="listHexStr">传进的数值</param>
        /// <returns></returns>
        private string GetHexIntermediateValue(string strHexMin, string strHexMax, List<string> listHexStr)
        {
            int iMin;
            int iMax;
            List<string> listRightHex = new List<string>();
            strHexMin = strHexMin.ToUpper().Replace(@"0X", string.Empty);
            strHexMax = strHexMax.ToUpper().Replace(@"0X", string.Empty);
            if (int.TryParse(strHexMin, System.Globalization.NumberStyles.AllowHexSpecifier, null, out iMin) &&
                int.TryParse(strHexMax, System.Globalization.NumberStyles.AllowHexSpecifier, null, out iMax))
            {
                foreach (var hexStr in listHexStr)
                {
                    int ihex;
                    string strHex = hexStr.ToUpper().Replace(@"0X", string.Empty);
                    if (int.TryParse(strHex, System.Globalization.NumberStyles.AllowHexSpecifier, null, out ihex))
                    {
                        if (iMin <= ihex && iMax >= ihex)
                        {
                            listRightHex.Add(@"0x" + strHex);
                        }
                    }
                }
            }
            else
            {
                return string.Empty;
            }
            if (listRightHex.Count > 1)
            {
                listRightHex.Remove(@"0x7DF");
            }
            return listRightHex.Count > 0 ? listRightHex[0] : string.Empty;
        }

        /// <summary>
        /// 限制datatable里不能添加重复项
        /// </summary>
        /// <returns>false为有重复项</returns>
        private bool PrimaryKey()
        {
            string lcitemStr;
            bool primary = false; //false默认为是有重复项
            int i = 0;
            for (int j = 0; j < _dt.Rows.Count; j++)
            {
                if (_currentOperate == DataOper.Modify)
                {
                    if (selectRow == j)
                        continue;
                }
                primary = false;
                string TestChannel = "";
                string DUTname = "";
                foreach (var lcItem in LcItems)
                {
                    if (typeof (SimpleButton) == lcItem.Control.GetType())
                        continue;
                    if (lcItem.Control.Name == "NMStationAddress" || lcItem.Control.Name == "NMBaseAddress" ||
                        lcItem.Control.Name == "RequestID" || lcItem.Control.Name == "ResponseID")
                        continue;
                    if (lcItem.Control.Name == "TestChannel")
                        TestChannel = lcItem.Control.Text;
                    if (lcItem.Control.Name == "DUTname")
                        DUTname = lcItem.Control.Text;
                    if (_selectedName == "网关路由配置表")
                    {
                        if (_dt.Rows[j][lcItem.Control.Name].ToString() != lcItem.Control.Text)
                            primary = true;
                    }
                }
                if (_selectedName != "网关路由配置表")
                {
                    if (
                        !(_dt.Rows[j]["TestChannel"].ToString() == TestChannel &&
                          _dt.Rows[j]["DUTname"].ToString() == DUTname))
                    {
                        primary = true;
                    }
                }
                if (primary == false)
                {
                    i++;
                }
            }
            if (i > 0)
            {
                if (_currentOperate == DataOper.Modify)
                    XtraMessageBox.Show("不能更改为已添加过的数据");
                if (_currentOperate == DataOper.Add)
                    XtraMessageBox.Show("请不要添加重复项");
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

        private void button_Click(object sender, EventArgs e)
        {
            #region 判断控件空值等细节
            bool NodeTypeBool = false;
            bool NodeTypeNullBool = false;
            bool DiagnosticNodeBool = false;//诊断节点 是：ture、否：false
            bool MasterNodeTypeBool = false;//LIN主节点：true、从节点：false
            bool DiagnosticNodeNullBool = false;
            bool HexToDec = true;
            int NMS=0;
            int NMB =0;
            foreach (var Item in LcItems)
            {
                if (typeof(SimpleButton) == Item.Control.GetType() ||
                    Item.Control.Name == "btnVirNodeID")
                    continue;
                if (Item.Control.Name == "DiagnosticNode")
                {
                    if (Item.Control.Text == "否")
                    {
                        DiagnosticNodeBool = false;
                    }
                    else
                    {
                        DiagnosticNodeBool = true;
                    }
                }

                if (Item.Control.Name == "MasterNodeType")
                {
                    if (Item.Control.Text == "从节点")
                    {
                        MasterNodeTypeBool = false;
                    }
                    else
                    {
                        MasterNodeTypeBool = true;
                    }
                }
                else
                {
                    if (Item.Control.Text == "")
                    {
                        if (Item.Control.Name == "Crystal" || Item.Control.Name == "RequestID" ||
                            Item.Control.Name == "ResponseID" || Item.Control.Name == "CddFileName") 
                        {
                            if (DiagnosticNodeBool)
                            {
                                XtraMessageBox.Show("请检查所输入项是否填写完整。");
                                return;
                            }
                            if (Item.Control.Name == "Crystal" && !MasterNodeTypeBool)
                            {
                                XtraMessageBox.Show("请检查所输入项是否填写完整。");
                                return;
                            }
                        }
                        else
                        {
                            if (Item.Control.Name != "VirNodeID"&&Item.Control.Name != "NMStationAddress" && Item.Control.Name != "NMBaseAddress")
                            {
                                XtraMessageBox.Show("请检查所输入项是否填写完整。");
                                return;
                            }else if (Item.Control.Name == "VirNodeID" && Item.Control.Enabled == true)
                            {
                                ListBoxControl lbc = Item.Control as ListBoxControl;
                                if (lbc.Items.Count == 0)
                                {
                                    XtraMessageBox.Show("虚拟节点不能为空。");
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Item.Control.Name == "RequestID" || Item.Control.Name == "ResponseID")
                        {
                            DiagnosticNodeNullBool = true;
                        }
                    }
                }
                if (Item.Control.Name == "NodeNetworkAttribute")
                {
                    if (Item.Control.Text == "间接NM")
                    {
                        NodeTypeBool = false;
                    }
                    else
                    {
                        NodeTypeBool = true;
                    }
                }
                else
                {
                    if (Item.Control.Text == "")
                    {
                        if (Item.Control.Name == "NMStationAddress" || Item.Control.Name == "NMBaseAddress")
                        {
                            if (NodeTypeBool)
                            {
                                XtraMessageBox.Show("请检查所输入项是否填写完整。");
                                return;
                            }
                        }
                        else
                        {
                            if (Item.Control.Name != "Crystal" && Item.Control.Name != "VirNodeID" &&
                                Item.Control.Name != "CddFileName" && Item.Control.Name != "RequestID" &&
                                Item.Control.Name != "ResponseID")
                            {
                                XtraMessageBox.Show("请检查所输入项是否填写完整。");
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (Item.Control.Name == "NMStationAddress" || Item.Control.Name == "NMBaseAddress")
                        {
                            NodeTypeNullBool = true;
                        }
                    }
                }

                foreach (KeyValuePair<string, List<object>> scheme in GlobalVar.ListDrScheme)
                {
                    if (Item.Control.Name == scheme.Key)
                    {
                        if (bool.Parse(scheme.Value[7].ToString()))
                        {
                            float result;
                            if (float.TryParse(Item.Control.Text.ToString(), out result))
                            {
                                if (result < float.Parse(scheme.Value[8].ToString()) ||
                                    result > float.Parse(scheme.Value[9].ToString()))
                                {
                                    XtraMessageBox.Show("输入数据的大小超出指定范围，请重新输入！", "提示", MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                            else
                            {
                                XtraMessageBox.Show("输入数据格式有误，请输入数字！", "提示", MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                                return;
                            }
                        }
                        if (DiagnosticNodeBool || (!DiagnosticNodeBool && DiagnosticNodeNullBool))
                        {
                            if (Item.Control.Name == "RequestID" || Item.Control.Name == "ResponseID")
                            {
                                string strHex = string.Empty;
                                if (isHexString(Item.Control.Text.Trim(), out strHex))
                                {
                                    Item.Control.Text = strHex;
                                }
                                else
                                {
                                    if (Item.Control.Name == "RequestID")
                                    {
                                        XtraMessageBox.Show("诊断请求ID值错误，请输入正确的十六进制值！");
                                    }
                                    else
                                    {
                                        XtraMessageBox.Show("诊断响应ID值错误，请输入正确的十六进制值！");
                                    }
                                    return;
                                }
                            }
                        }
                        if (NodeTypeBool||(!NodeTypeBool&&NodeTypeNullBool))
                        {
                            string strHex;
                            if (bool.Parse(scheme.Value[3].ToString()))
                            {
                                if (Item.Control.Name == "NMStationAddress")
                                {
                                    if (Item.Control.Text.Length>2)
                                    {
                                        if (Item.Control.Text.ToLower().Substring(0, 2) == "0x")
                                        {
                                            Item.Control.Text = "0x" + Item.Control.Text.Remove(0,2).ToUpper();
                                            strHex = Item.Control.Text.Remove(0, 2);
                                        }
                                        else
                                        {
                                            strHex = Item.Control.Text;
                                            Item.Control.Text = "0x" + Item.Control.Text.ToUpper();
                                        }
                                    }
                                    else
                                    {
                                        strHex = Item.Control.Text;
                                        Item.Control.Text = "0x" + Item.Control.Text.ToUpper();
                                    }
                                    if (!(int.TryParse(strHex, System.Globalization.NumberStyles.AllowHexSpecifier, null, out NMS)))
                                    {
                                        HexToDec = false;
                                    }
                                }
                                else if (Item.Control.Name == "NMBaseAddress")
                                {
                                    if (Item.Control.Text.Length > 2)
                                    {
                                        if (Item.Control.Text.ToLower().Substring(0, 2) == "0x")
                                        {
                                            Item.Control.Text = "0x" + Item.Control.Text.Remove(0, 2).ToUpper();
                                            strHex = Item.Control.Text.Remove(0, 2);
                                        }
                                        else
                                        {
                                            strHex = Item.Control.Text;
                                            Item.Control.Text = "0x" + Item.Control.Text.ToUpper();
                                        }
                                    }
                                    else
                                    {
                                        strHex = Item.Control.Text;
                                        Item.Control.Text = "0x" + Item.Control.Text.ToUpper();
                                    }
                                    if (!(int.TryParse(strHex, System.Globalization.NumberStyles.AllowHexSpecifier, null, out NMB)))
                                    {
                                        HexToDec = false;
                                    }
                                }
                            }
                        }
                    }

                }
            }
            if(NodeTypeBool || (!NodeTypeBool && NodeTypeNullBool))
            {
                if (HexToDec)
                {
                    if (!(NMS > 0 && NMS <= 536870911))
                    {
                        {
                            XtraMessageBox.Show("输入数据的大小超出指定范围，请重新输入！", "提示", MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    if (!(NMB > 0 && NMB <= 536870911))
                    {
                        {
                            XtraMessageBox.Show("输入数据的大小超出指定范围，请重新输入！", "提示", MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }
                else
                {
                    XtraMessageBox.Show("请输入正确的十六进制数值！", "提示", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }
            #endregion

            if (!PrimaryKey())
                return;
            string itemStr = string.Empty;
            switch (_currentOperate)
            {
                //添加
                case DataOper.Add:
                    Dictionary<string, string> GetString = new Dictionary<string, string>();
                    if (LcItems.Count == 0)
                        return;
                    DataRow dr = _dt.NewRow();
                    foreach (var Item in LcItems)
                    {
                        if (typeof(SimpleButton) == Item.Control.GetType() ||
                            Item.Control.Name == "btnVirNodeID")
                            continue;
                        if (typeof(ListBoxControl) == Item.Control.GetType() && Item.Control.Name == "VirNodeID")
                        {
                            ListBoxControl lbc = (ListBoxControl) Item.Control;
                            itemStr = string.Empty;
                            foreach (var lbcItem in lbc.Items)
                            {
                                itemStr += lbcItem.ToString() + @",";
                            }
                            itemStr = itemStr.Length > 0 ? itemStr.Remove(itemStr.Length - 1) : string.Empty;
                        }
                        else if (string.IsNullOrEmpty(Item.Control.Text))
                        {
                            itemStr = "--";
                        }
                        else
                        {
                            itemStr = Item.Control.Text;
                        }
                        GetString[Item.Control.Name] = itemStr;
                        dr[Item.Control.Name] = itemStr;
                    }
                    if (_selectedName == "CAN总线配置表")
                    {
                        dr["EventRelevant"] = "";
                        dr["DTCRelevant"] = "";
                        dr["CheckItem"] = "请右键查看事件信息";
                        dr["CheckDTC"] = "请右键查看通信DTC信息";
                    }
                    else if (_selectedName == "LIN总线配置表")
                    {
                        dr["EventRelevant"] = "";
                        dr["CheckItem"] = "请右键查看事件信息";
                    }
                    else if (_selectedName == "网关路由配置表")
                    {
                        dr["GatewayPath"] = "";
                        dr["CheckItem"] = "请右键查看网关路由表";
                    }

                    _dt.Rows.Add(dr);
                    gcCfgTemplate.DataSource = _dt;
                    string error = "";
                    string json = DtToJson(out error);
                    if (error == "")
                        CfgSaveJson(json);
                    //button.Enabled = false;
                    dpCfg.Visibility = DockVisibility.Visible;
                    break;
                case DataOper.Modify:
                    //object[] newRow = new object[LcItems.Count];
                    int i = 0;
                    foreach (LayoutControlItem lItem in LcItems)
                    {
                        if (typeof(SimpleButton) == lItem.Control.GetType() ||
                            lItem.Control.Name == "btnVirNodeID") 
                            continue;
                        if (typeof(ListBoxControl) == lItem.Control.GetType() && lItem.Control.Name == "VirNodeID" &&
                            lItem.Control.Enabled == true)
                        {
                            ListBoxControl lbc = (ListBoxControl)lItem.Control;
                            itemStr = string.Empty;
                            foreach (var lbcItem in lbc.Items)
                            {
                                itemStr += lbcItem.ToString() + @",";
                            }

                            if (lbc.Items.Count > 0)
                            {
                                itemStr = itemStr.Remove(itemStr.Length - 1);
                            }
                            else
                            {
                                XtraMessageBox.Show("虚拟节点ID不能为空！");
                                return;
                            }
                        }
                        else if (string.IsNullOrEmpty(lItem.Control.Text))
                        {
                            itemStr = "--";
                        }
                        else
                        {
                            itemStr = lItem.Control.Text;
                        }
                        _dt.Rows[selectRow][i] = itemStr;
                        i++;
                    }
                    string errore = "";
                    string jsonj = DtToJson(out error);
                    if (errore == "")
                        CfgSaveJson(jsonj);
                    //btnUp.Enabled = false;
                    dpCfg.Visibility = DockVisibility.Hidden;

                    //DerGridToJson();
                    break;
            }
            UpdateGridViewByOperate();
        }
        /// <summary>
        /// 判断字符串是否是十六进制数值，支持前面带0x和不带0x两种，返回的十六进制字符串带0x
        /// </summary>
        /// <param name="str">传进来的数值</param>
        /// <param name="hex">返回的十六进制字符串</param>
        /// <returns></returns>
        private bool isHexString(string str,out string strHex)
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
        private void ListBoxControl_MouseDown(object sender, MouseEventArgs e)
        {
            ListBoxControl lbc = sender as ListBoxControl;
            listboxSelectItem(lbc);
        }

        private void ListBoxControl_SelectedValueChanged(object sender, EventArgs e)
        {
            ListBoxControl lbc = sender as ListBoxControl;
            listboxSelectItem(lbc);
        }

        private void listboxSelectItem(ListBoxControl lbc)
        {
            int index = lbc.SelectedIndex;
            if (index == -1)
                return;
            string strHex = lbc.GetItem(index).ToString();
            if (string.IsNullOrEmpty(strHex))
                return;
            foreach (var item in LcItems)
            {
                if (typeof(ButtonEdit) == item.Control.GetType() && item.Control.Name == "btnVirNodeID")
                {
                    ButtonEdit btnEdit = (ButtonEdit)item.Control;
                    btnEdit.Text = strHex;
                    break;
                }
            }
        }
        private void VirNodeIDButtonEdit_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            ButtonEdit btnEdit = sender as ButtonEdit;
            string strReadHex = btnEdit.Text.Trim();
            string strHex = string.Empty;
            if(string.IsNullOrEmpty(strReadHex))
                return;
            if (!isHexString(strReadHex, out strHex))
            {
                XtraMessageBox.Show(@"请输入十六进制数值！");
                return;
            }
            else
            {
                btnEdit.Text = strHex;
            }
            foreach (var item in LcItems)
            {
                if (typeof(ListBoxControl) == item.Control.GetType() && item.Control.Name == "VirNodeID")
                {
                    ListBoxControl lbc = (ListBoxControl) item.Control;
                    if (e.Button.Kind == ButtonPredefines.Plus)
                    {
                        if (!lbc.Items.Contains(strHex))
                            lbc.Items.Add(strHex);
                    }
                    else if (e.Button.Kind == ButtonPredefines.Minus)
                    {
                        if (lbc.Items.Contains(strHex))
                        {
                            foreach (var litem in lbc.Items)
                            {
                                if (litem.ToString().ToLower() == strHex.ToLower())
                                {
                                    lbc.Items.Remove(litem);
                                    break;
                                }
                            }
                        }
                    }
                    break;
                }
            }
        }

        private void SelectFileControl_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            ButtonEdit btnEdit = sender as ButtonEdit;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = @"请选择cdd文件";
            dialog.Filter = @"cdd文件(*.cdd)|*.cdd";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                btnEdit.Text = dialog.FileName;
            }
        }

        private void cmsCfgAdd_Click(object sender, EventArgs e)
        {
            _dr = null;
            if (!_nameItem)
            {
                MessageBox.Show(@"请先在左侧选择当前车型要编辑的配置表类型...");
                return;
            }
            DefaultValue();
            dpCfg.Visibility = DockVisibility.Visible;
            //JsonToListDraw(true);
            //btnUp.Enabled = true;
            _currentOperate = DataOper.Add;
        }

        private void DefaultValue()
        {
            List<string> itemNameList = new List<string>
            {
                "EngineStartRelated","CRCType","SystemType","DiagnosticNode","NodeNetworkAttribute","SlaveboxID","TerminalR"
                //发动机启动相关无关，CRC校验类型，12V/24V系统，诊断节点，网络节点管理属性，节点配置盒，终端电阻
            };
            foreach (LayoutControlItem item in LcItems)
            {
                if (typeof(ComboBoxEdit) != item.Control.GetType())
                    continue;
                if (itemNameList.Contains(item.Control.Name))
                {
                    ComboBoxEdit cmbEdit = item.Control as ComboBoxEdit;
                    cmbEdit.SelectedIndex = cmbEdit.Properties.Items.Count > 0 ? 0 : -1;
                }
            }
        }

        private void InitGrid()
        {
            _dt=new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvCfgTemplate.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList.ToArray())
                _dt.Columns.Add(new DataColumn(colName, typeof(object)));
            gcCfgTemplate.DataSource = _dt;
            //JsonToDt();
        }

        private string DtToJson(out string error)
        {
            error = "";
            try
            {
                var listChapter = new List<Dictionary<string, string>>();
                //var listExample = new List<object>();
                var coList = new List<string>();
                foreach (GridColumn col in gvCfgTemplate.Columns)
                    coList.Add(col.FieldName);
                foreach (DataRow row in _dt.Rows)
                {
                    Dictionary<string, string> chapter = new Dictionary<string, string>();
                    //int i = 0;
                    if (_selectedName == "LIN总线配置表"||_selectedName== "网关路由配置表")
                    {
                        for (int i = 0; i < coList.Count - 1; i++)
                        {
                            chapter[coList[i]] = row[coList[i]].ToString();
                        }
                    }
                    else if (_selectedName == "CAN总线配置表")
                    {
                        for (int i = 0; i < coList.Count - 2; i++)
                        {
                            chapter[coList[i]] = row[coList[i]].ToString();
                        }
                    }
       
                    listChapter.Add(chapter);
                }
                return Json.SerJson(listChapter);
            }
            catch (Exception e)
            {
                error = e.ToString();
                MessageBox.Show("此CAN路中已经添加了相同的节点");
                return "";
            }
}

        /// <summary>
        /// 将配置表Json串写入GridView中
        /// </summary>
        /// <param name="JsonRead"></param>
        private void JsonToDt(string JsonRead)
        {
            //int i = 0;
            Dictionary<string, string> ExampleJsonList = new Dictionary<string, string>();
            //string JsonRead = null;

            //var dictChapter = new List<Dictionary<string, string>>();
            //dictChapter = Json.DerJsonToLDict(eml);
            //GlobalVar.ListDrScheme = Json.DeserJsonDList(colEml);
            if (GlobalVar.ListDrScheme == null)
                return;

            if (JsonRead != null)
            {
                var dictExap = Json.DerJsonToLDict(JsonRead);
                int colCount = gvCfgTemplate.Columns.Count;
                
                foreach (Dictionary<string, string> ExapOne in dictExap)
                {
                    int col = 0;
                    object[] obje = new object[colCount];
                    foreach (KeyValuePair<string, string> chapter in ExapOne)
                    {

                            obje[col] = chapter.Value;
                            col++;  
                    }
                    if (_selectedName == "LIN总线配置表")
                    {
                        obje[col] = "请右键查看事件信息";
                        _dt.Rows.Add(obje);
                    }
                    else if (_selectedName == "CAN总线配置表")
                    {
                        obje[col] = "请右键查看事件信息";
                        col++;
                        obje[col] = "请右键查看通信DTC相关信息";
                        _dt.Rows.Add(obje);
                    }
                    else if(_selectedName== "网关路由配置表")
                    {
                        obje[col] = "请右键查看网关路由";
                        _dt.Rows.Add(obje);
                    }

                }

                    //foreach (var ExapToOne in ExapOne.Value)
                    //        object[] obj = new object[colCount];
                    //        int i = 0;
                    //        //     _dt = new DataTable();
                    //        //InitGrid();
                    //            ExampleJsonList = Json.DerJsonToDict(ExapOne.Value[0].ToString());
                    //            foreach (KeyValuePair<string, string> ExapTwo in ExampleJsonList)
                    //            {
                    //            //var key = ExapTwo.Key;
                    //            //var value = ExapTwo.Value;
                    //            //dr[key] = value;
                    //            obj[i] = ExapTwo.Value;
                    //            i ++;
                    //            }

                    //        var IOJsonList = Json.DerJsonToLDict(ExapOne.Value[1].ToString());
                    //        int j = i;
                    //        foreach (var ExapTwo in IOJsonList)
                    //        {
                    //            i = j;
                    //            foreach (KeyValuePair<string, string> dict in ExapTwo)
                    //            {
                    //                obj[i] = dict.Value;
                    //                i++;
                    //                //var key = dict.Key;
                    //                //var value = dict.Value;
                    //                //dr[key] = value;
                    //                //_dt.Rows.Add(dr);
                    //                //i++;

                    //            }
                    //            _dt.Rows.Add(obj);
                    //            //gcEmlTemplate.DataSource = _dt;
                    //            // }
                    //        }
                    //    }
                    //}
                    gcCfgTemplate.DataSource = _dt;
                }
        }

        public void Write(string path, string json)
        {
            try
            {
                StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8);
                //写入
                sw.Write(json);
                //清空
                sw.Flush();
                //关闭
                sw.Close();
            }
            catch (Exception e)
            {
                XtraMessageBox.Show("发生未知错误，请联系技术人员" + "\r\n" + "错误信息：" + e.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmsCfgDel_Click(object sender, EventArgs e)
        {
            if (_dr != null)
            {
                _dr.Delete();
                string error = "";
                string json = DtToJson(out error);
                if (error == "")
                {
                    CfgSaveJson(json);
                }
            }
        }

        private void gvCfgTemplate_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //获得光标位置
                var hi = gvCfgTemplate.CalcHitInfo(e.Location);
                //判断位置是否在行位置上
                if (!hi.InRow && !hi.InRowCell) return;
                if (hi.RowHandle < 0) return;
                //取一行值
                gvCfgTemplate.SelectRow(hi.RowHandle);
                selectRow = hi.RowHandle;
                _dr = gvCfgTemplate.GetDataRow(hi.RowHandle);
            }
        }
        private enum DataOper
        {
            Add = 0,
            Modify = 1,

        }
        

        private void tsmiModify_Click(object sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            if (_dr == null)
            {
                MessageBox.Show("请先在表中选择一行再修改...");
                return;
            }
            if (!_nameItem)
            {
                MessageBox.Show("请先在左侧选择当前车型要编辑的配置表类型...");
                return;
            }
            //dpCfgTemplate.Visibility = DockVisibility.Visible;
            //JsonToListDraw(true);

            _currentOperate = DataOper.Modify;
            dpCfg.Visibility = DockVisibility.Visible;
            //btnUp.Enabled = true;
            //SetDataToUI();
            SetDataToDockPanel();
        }


        private void SetDataToDockPanel()
        {
            string drStr = string.Empty;
            if (_dr == null)
                return;
            foreach (LayoutControlItem item in LcItems)
            {
                if (typeof(SimpleButton) == item.Control.GetType())
                    continue;
                if (item.Control.Name == "btnVirNodeID")
                {
                    item.Control.Text = string.Empty;
                    continue;
                }
                if (typeof(ListBoxControl) == item.Control.GetType() && item.Control.Name == "VirNodeID")
                {
                    ListBoxControl lbc = (ListBoxControl) item.Control;
                    lbc.Items.Clear();
                    var strArray = _dr[item.Control.Name].ToString().Split(',');
                    foreach (var str in strArray)
                    {
                        if (str.Trim() != string.Empty && str.Trim() != "--")
                        {
                            lbc.Items.Add(str);
                        }
                    }
                }
                else if (_dr[item.Control.Name].ToString() == "--")
                {
                    drStr = "";
                }
                else
                {
                    drStr = _dr[item.Control.Name].ToString();
                }
                item.Control.Text = drStr;
            }
        }


        private void btnBaud_ItemClick(object sender, ItemClickEventArgs e)
        {
            dpBaud.Visibility = DockVisibility.Visible;
        }

        private void GetBaudDict()
        {
            Dictionary<string, string> dictBaud = new Dictionary<string, string>();
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["VehicelType"] = GlobalVar.VNode[0];
            dict["VehicelConfig"] = GlobalVar.VNode[1];
            dict["VehicelStage"] = GlobalVar.VNode[2];
            var dbcList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.DBC, dict);
            var ListBaud = _store.GetRegularByEnum(EnumLibrary.EnumTable.Segment);
            var dictBaudTemp= ListBaud.ToDictionary(t => t[0].ToString(), t => t[1].ToString());
            foreach (var Bauditem in ListBaud)
            {
                if (!BaudValue.Contains(Bauditem[1].ToString()))
                {
                    BaudValue.Add(Bauditem[1].ToString());
                }
            }
            BaudValue = BaudValue.OrderBy(t => double.Parse(t)).ToList();
            foreach (var dbc in dbcList)
            {
                if (dictBaudTemp.ContainsKey(dbc[4].ToString()))
                {
                    dictBaud[dbc[4].ToString()] = dictBaudTemp[dbc[4].ToString()];
                }
            }
            DrawFillBaud(dictBaud);
        }

        private void DrawFillBaud(Dictionary<string,string> dictBaud)
        {
            foreach (var Bauditem in dictBaud)
            {
                CreateBaudComboBox(Bauditem);
            }
            if (LcIBauditems.Count > 0)
            {
                CreateBaudButton();
            }
            foreach (LayoutControlItem item in LcIBauditems)
            {
                lcgBaud.AddItem(item);
            }
        }

        private string GetControlTextToJson()
        {
            Dictionary<string, string> GetdictBaud = new Dictionary<string, string>();
            foreach (var Item in LcIBauditems)
            {
                if (typeof(SimpleButton) == Item.Control.GetType())
                    continue;
                GetdictBaud[Item.Control.Name] = Item.Control.Text;
            }
            return Json.SerJson(GetdictBaud);
        }

        private void CreateBaudComboBox(KeyValuePair<string,string> BaudKVP)
        {
            ComboBoxEdit combedit = new ComboBoxEdit();
            LayoutControlItem lc = new LayoutControlItem();
            //ComboBoxEdit
            combedit.Name = BaudKVP.Key;
            combedit.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //foreach (string Value in BaudValue)
            //{
            //    combedit.Properties.Items.Add(Value);
            //}
            combedit.Properties.Items.AddRange(BaudValue);
            combedit.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            combedit.Text = BaudKVP.Value;
            lc.Control = combedit;
            lc.Name = "LC" + BaudKVP.Key;
            lc.Text = BaudKVP.Key + @"波特率：";
            lc.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 5, 5);
            //lc.Location = new System.Drawing.Point(0, y * 28);
            lc.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //绑定
            LcIBauditems.Add(lc);
        }
        private void CreateBaudButton()
        {
            SimpleButton simpleButton = new SimpleButton();
            LayoutControlItem simpleButtonLCI = new LayoutControlItem();
            // 
            // simpleButton
            // 
            simpleButton.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            simpleButton.Appearance.Options.UseFont = true;
            //simpleButton.AutoWidthInLayoutControl = true;
            simpleButton.Size = new System.Drawing.Size(260, 28);
            simpleButton.Name = "simpleButton";
            simpleButton.Text = @"确定";
            //simpleButton.Location = new System.Drawing.Point(0, y * 28);
            simpleButton.Margin = new System.Windows.Forms.Padding(10, 10, 20, 10);
            simpleButton.MinimumSize = new Size(0, 28);
            // 
            // simpleButtonLCI
            // 
            simpleButtonLCI.Control = simpleButton;
            simpleButtonLCI.ControlAlignment = System.Drawing.ContentAlignment.BottomCenter;
            //simpleButtonLCI.Location = new System.Drawing.Point(0, y * 28);
            simpleButtonLCI.Name = "simpleButtonLCI";
            simpleButtonLCI.Text = "simpleButtonLCI";
            simpleButtonLCI.TextVisible = false;
            simpleButton.Click += Baudbutton_Click;
            LcIBauditems.Add(simpleButtonLCI);
            //this.dpCfgGroup.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] { simpleButtonLCI });
        }

        private void Baudbutton_Click(object sender, EventArgs e)
        {
            string error = string.Empty;
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["VehicelType"] = GlobalVar.VNode[0];
            dict["VehicelConfig"] = GlobalVar.VNode[1];
            dict["VehicelStage"] = GlobalVar.VNode[2];
            dict["CfgBaudJson"] = GetControlTextToJson();
            _store.Update(EnumLibrary.EnumTable.FileLinkByVehicelBaudOnly, dict, out error);
            if (error != string.Empty)
            {
                XtraMessageBox.Show("波特率更新异常，请联系管理员！");
            }
            dpBaud.Visibility = DockVisibility.Hidden;
        }

        private void tsmiCfgEvent_Click(object sender, EventArgs e)
        {
            if (_dr == null)
                return;
            CfgEventRelated Ev = new CfgEventRelated(_dr["EventRelevant"].ToString(),_dr["SlaveboxID"].ToString());
            Ev.ShowDialog();
            if (Ev.DialogResult == DialogResult.OK)
            {
                _dt.Rows[selectRow]["EventRelevant"] = GlobalVar.strEvent;
                gcCfgTemplate.DataSource = _dt;
                string error;
                string jsons = DtToJson(out error);
                if (error == "")
                    CfgSaveJson(jsons);
                GlobalVar.strEvent = "";
            }
        }

        private void tsmiCheckDTC_Click(object sender, EventArgs e)
        {
            if (_dr == null)
                return;
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["TestChannel"] = _dr["TestChannel"].ToString();
            dict["DUTname"] = _dr["DUTname"].ToString();
            dict["SystemType"] = _dr["SystemType"].ToString();
            dict["DTCRelevant"] = _dr["DTCRelevant"].ToString();
            //BusDTCRelevant Ev = new BusDTCRelevant(_dr["DTCRelevant"].ToString(),_dr["DUTname"].ToString());
            //Ev.ShowDialog();
            DTCFault df = new DTCFault(dict);
            df.ShowDialog();
            if (df.DialogResult == DialogResult.OK)
            {
                //_dt.Rows[selectRow]["DTCRelevant"] = GlobalVar.strEvent;
                _dr["DTCRelevant"]= GlobalVar.strEvent;
                gcCfgTemplate.DataSource = _dt;
                string error;
                string jsons = DtToJson(out error);
                if (error == "")
                    CfgSaveJson(jsons);
                GlobalVar.strEvent = "";
            }
        }

        private void tsmiRoute_Click(object sender, EventArgs e)
        {
            if (_dr == null)
                return;
            string gwPath = _dr["GatewayPath"] != null ? _dr["GatewayPath"].ToString() : string.Empty;
            GatewayRoutingConfig grc = new GatewayRoutingConfig(gwPath);
            grc.ShowDialog();
            if (grc.DialogResult == DialogResult.OK)
            {
                _dt.Rows[selectRow]["GatewayPath"] = GlobalVar.strEvent;
                gcCfgTemplate.DataSource = _dt;
            }


            //Dictionary<string, object> VehicelWithCan = new Dictionary<string, object>();
            //VehicelWithCan.Add("VehicelType", GlobalVar.VNode[0]);
            //VehicelWithCan.Add("VehicelConfig", GlobalVar.VNode[1]);
            //VehicelWithCan.Add("VehicelStage", GlobalVar.VNode[2]);
            //VehicelWithCan.Add("BelongCAN", _dr["SourceSegment"].ToString());
            //object[] SourceDBC = _store.GetSpecialByEnum(EnumLibrary.EnumTable.DBCBelongCAN, VehicelWithCan)[0];
            //VehicelWithCan["BelongCAN"] = _dr["TargetSegment"].ToString();
            //object[] TargetDBC = _store.GetSpecialByEnum(EnumLibrary.EnumTable.DBCBelongCAN, VehicelWithCan)[0];

            //Dictionary<string, string> CANPath = new Dictionary<string, string>();
            //CANPath.Add("SourceDBC", AppDomain.CurrentDomain.BaseDirectory+SourceDBC[5].ToString());
            //CANPath.Add("TargetDBC", AppDomain.CurrentDomain.BaseDirectory+TargetDBC[5].ToString());
            //CANPath.Add("Gateway", _dr["Gateway"].ToString());

            //CfgGateway gw = new CfgGateway(_dr["EventRelevant"].ToString(), CANPath);
            //gw.ShowDialog();
            //if (gw.DialogResult == DialogResult.OK)
            //{
            //    _dt.Rows[selectRow]["EventRelevant"] = GlobalVar.strEvent;
            //    gcCfgTemplate.DataSource = _dt;
            //}
        }

        private void UpdateGridViewByOperate()
        {
            DataTable newDt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvCfgTemplate.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList.ToArray())
                newDt.Columns.Add(new DataColumn(colName, typeof(object)));
            string chapter = "";
            List<string> chapterDiff = new List<string>();
            foreach (DataRow row in _dt.Rows)
            {
                if (row.ItemArray[0].ToString() != chapter)
                {
                    bool same = false;
                    foreach (string item in chapterDiff)
                    {
                        if (item == row.ItemArray[0].ToString())
                            same = true;
                    }
                    if (!same)
                        chapterDiff.Add(row.ItemArray[0].ToString());
                    chapter = row.ItemArray[0].ToString();
                }
            }
            foreach (string chapterM in chapterDiff)
            {
                foreach (DataRow row in _dt.Rows)
                {
                    if (row.ItemArray[0].ToString() == chapterM)
                    {
                        newDt.Rows.Add(row.ItemArray);
                    }
                }
            }
            _dt = newDt;
            gcCfgTemplate.DataSource = _dt;
            BtnSubmitImage();
        }

        private void gcCfgTemplate_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //获得光标位置
            var hi = gvCfgTemplate.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //取一行值
            gvCfgTemplate.SelectRow(hi.RowHandle);
            selectRow = hi.RowHandle;
            _dr = gvCfgTemplate.GetDataRow(hi.RowHandle);

            Update();
        }

        private void spinEdit_EditValueChanging(object sender, ChangingEventArgs e)
        {
            if (decimal.Parse(e.NewValue.ToString()) < 0)
            {
                e.Cancel = true;
            }
        }
    }
}
