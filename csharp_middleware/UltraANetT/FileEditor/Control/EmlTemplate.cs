using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using DevExpress.LookAndFeel;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraNavBar;
using FileEditor.form;
using FileEditor.pubClass;
using ProcessEngine;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraLayout;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraCharts.Native;
using DevExpress.XtraLayout.Utils;
using DevExpress.XtraRichEdit.API.Word;
using DevExpress.XtraSpreadsheet.Model;
using Font = System.Drawing.Font;
using Range = DevExpress.Spreadsheet.Range;
using Worksheet = DevExpress.Spreadsheet.Worksheet;
using FileEditor.Form;
using DBEngine;
using System.Collections;

namespace FileEditor.Control
{
    public partial class EmlTemplate : XtraUserControl, ITemplate
    {
        private ProcLog Log = new ProcLog();
        private Worksheet _sheet;
        private readonly ProcStore _store;
        private readonly ProcShow _show = new ProcShow();
        private readonly ProcFile _file = new ProcFile();
        private readonly SearchDTCByExaModule _searchDtc = new SearchDTCByExaModule();
        private readonly ITemplate _tem;
        private bool isSave = true;
        private readonly Dictionary<string, object> _dictExample = new Dictionary<string, object>();
        private string _selectedName = "";

        public bool _isOnlyEml = false;
        private bool fileEidtor = false;
        private Thread th;
        private List<LayoutControlItem> itemList = new List<LayoutControlItem>();
 
        DataTable _dt = new DataTable();

        private DataRow _dr;
        private int selectRow;
        private DataOper _currentOperate;
        private string emlTem = "";
        private NavBarItem oldBarItem = null;
        private string _itemName;
        public EmlTemplate(bool isOnlyEml)
        {
            //车型第二步进入和任务口进入

            InitializeComponent();
            fileEidtor = false;
            _isOnlyEml = isOnlyEml;
            _store = new ProcStore();
            _tem = this;
            _tem.InitDict();
            BtnSubmitImage();

            //InitEmlGrid();

            //从任务口
            if (_isOnlyEml)
            {
                //_tem.DrawExcel();
                nbcTempList.Enabled = false;
                btnColSet.Enabled = false;
                btnInitVersion.Enabled = false;
                string BelongNode =  GlobalVar.TaskNo + "-" + GlobalVar.TaskRound + "-" + GlobalVar.TaskName + "-" +
                                    GlobalVar.CANRoad + "-" + GlobalVar.TextBelongModule;
                txtBelongNode.Caption = BelongNode;
                InitTaskEml();
                if (GlobalVar.EmlClickItemName == "总线相关DTC用例表")
                {
                    Thread th = new Thread(() => XtraMessageBox.Show("请为总线相关DTC用例表添加DTC编号"));
                    th.Start();
                }
            }
            editMatch.EditValue = cmbMatch.Items[0];
            cmbMatch.ReadOnly = true;
            hideContainerRight.Visible = false;

        }

        public EmlTemplate()
        {
            //从文件编辑器
            InitializeComponent();
            btnInitVersion.Visibility = BarItemVisibility.Always;
            btnSave.Visibility = BarItemVisibility.Never;
            btnSubmit.Visibility = BarItemVisibility.Never;
            btnRefreah.Visibility = BarItemVisibility.Never;
            dpEml.Enabled = false;
            cmsEml.Enabled = false;
            fileEidtor = true;
            _store = new ProcStore();
            _tem = this;
            _tem.InitDict();
            editMatch.EditValue = cmbMatch.Items[0];
            cmbMatch.ReadOnly = true;
            btnColSet.Enabled = true;
            hideContainerRight.Visible = false;

        }

        /// <summary>
        /// 读取列编辑txt文件
        /// </summary>
        /// 
        private void InitTaskEml()
        {
            if (!string.IsNullOrWhiteSpace(GlobalVar.EmlTemplateJson) &&
                !string.IsNullOrWhiteSpace(GlobalVar.EmlTemplateColJson))
            {
                var i = GlobalVar.TaskNo.Split('-');
                Dictionary<string, object> _dictRelated = new Dictionary<string, object>
            {
                {"VehicelType", i[0]},
                {"VehicelConfig", i[1]},
                {"VehicelStage", i[2]},
                { "EmlTemplateName",GlobalVar.TaskName + "用例表"},
            };
                IList<object[]> flList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelDouble, _dictRelated);
                _dt = new DataTable();
                GlobalVar.EmlClickItemName = flList[0][9].ToString();
                _tem.DrawNav();
                _tem.DrawTem(GlobalVar.EmlClickItemName);
                DerDictToGridView(GlobalVar.EmlTemplateJson, GlobalVar.EmlTemplateColJson);
                CreateDockPanel(GlobalVar.ListDrScheme);
                    
                

            }
        }



        private void InitEmlPage()
        {
            _tem.DrawTem(_selectedName);
            if (!GlobalVar.IsIndependent && !_isOnlyEml)
            {
                string match = _file.SearchBusByEmlName(_selectedName);
                Dictionary<string, object> dictRelated = new Dictionary<string, object>{
                    {"VehicelType",GlobalVar.VNode[0]},
                    {"VehicelConfig",GlobalVar.VNode[1]},
                    {"VehicelStage",GlobalVar.VNode[2] },
                    { "EmlTemplateName",_selectedName}

                }; ;
                //IList<object[]> file = ReadDataByVehicel(GlobalVar.VNode);
                
                IList<object[]> flList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelDouble, dictRelated);
                string vehicel = GlobalVar.VNode[0] + "-" + GlobalVar.VNode[1] + "-" + GlobalVar.VNode[2] + "-";
                string path = GlobalVar.TemporaryFilePath + "example\\" + vehicel + _selectedName + ".txt";
                //var dictChapter = new Dictionary<string, Dictionary<string, List<object>>>();
                if (File.Exists(path)) //验证临时文件
                {
                    string JsonRead = File.ReadAllText(path);
                    if (JsonRead == "")
                    {
                        string warm = "请先删除该：" + path + "空文件，然后重新刷新界面！";
                        XtraMessageBox.Show(warm, "提示", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning);
                        return;
                    }
                    if (flList.Count != 0 && flList[0][13].ToString() != "")
                    {
                        DerDictToGridView(JsonRead, flList[0][13].ToString());
                        //Dictionary<string, List<object>> coldict = Json.DeserJsonDList(file[0][13].ToString());
                        CreateDockPanel(GlobalVar.ListDrScheme);
                    }
                    else
                    {
                        if (emlTem != "")
                        {
                            DerDictToGridView(JsonRead, emlTem);
                            //Dictionary<string, List<object>> coldict = Json.DeserJsonDList(emlTem);
                            CreateDockPanel(GlobalVar.ListDrScheme);
                        }
                        else
                        {
                            XtraMessageBox.Show("请先在左侧选择用例表类型", "提示", MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Warning);
                        }

                    }

                }
                else if (flList.Count != 0 && flList[0][10].ToString() != "")
                {
                    btnColSet.Enabled = false;
                    if (flList[0][13].ToString() != "")
                    {
                        DerDictToGridView(flList[0][10].ToString(), flList[0][13].ToString());
                        //Dictionary<string, List<object>> coldict = Json.DeserJsonDList(file[0][13].ToString());
                        CreateDockPanel(GlobalVar.ListDrScheme);
                    }
                    else
                    {

                        if (emlTem != "")
                        {
                            DerDictToGridView(flList[0][10].ToString(), emlTem);
                            Dictionary<string, List<object>> coldict = Json.DeserJsonDList(emlTem);
                            CreateDockPanel(GlobalVar.ListDrScheme);
                        }
                        else
                        {
                            XtraMessageBox.Show("请先在左侧选择用例表类型", "提示", MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Warning);
                        }

                    }
                }
                else if (flList.Count != 0 && flList[0][13].ToString() != "")
                {
                    GlobalVar.ListDrScheme = Json.DeserJsonDList(flList[0][13].ToString());
                    if (GlobalVar.ListDrScheme == null)
                        gcEmlTemplate.DataSource = null;
                    else
                    {
                        CreateGridView(GlobalVar.ListDrScheme);
                        InitDataTable();
                        CreateDockPanel(GlobalVar.ListDrScheme);
                    }
                }
                else
                {

                    if (emlTem == "")
                    {
                        XtraMessageBox.Show("用例表数据库出错", "提示", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning);
                        return;
                    }
                    GlobalVar.ListDrScheme = Json.DeserJsonDList(emlTem);
                    if (GlobalVar.ListDrScheme == null)
                        gcEmlTemplate.DataSource = null;
                    else
                    {
                        CreateGridView(GlobalVar.ListDrScheme);
                        //break;
                    }

                    InitDataTable();
                    CreateDockPanel(GlobalVar.ListDrScheme);
                }
            }
            /////文件编辑器入口
            else
            {
                btnColSet.Enabled = true;
                //_tem.DrawExcel(_selectedName);
                //emlTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ExampleTemp, dictTem);
                if (emlTem == "")
                {
                    XtraMessageBox.Show("用例表数据库出错", "提示", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning);
                    return;
                }
                GlobalVar.ListDrScheme = Json.DeserJsonDList(emlTem);
                if (GlobalVar.ListDrScheme == null)
                    gcEmlTemplate.DataSource = null;
                else
                {
                    CreateGridView(GlobalVar.ListDrScheme);
                    
                }


                InitDataTable();

                if (
                    XtraMessageBox.Show("是否需要修改该模板用例表的列数？", "提示", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning) ==
                    DialogResult.OK)
                {
                    ShowColEdit();
                }
            }
            //SaveTime();

        }

        //

        private void CreateDockPanel(Dictionary<string,List<object>> colDictionary)
        {
            int i = 0;
            dpEmlLcg.Items.Clear();
            layoutControl3.Controls.Clear();
            itemList.Clear();
            foreach (KeyValuePair<string, List<object>> scheme in colDictionary)
            {
                //try
                {
                    if (bool.Parse(scheme.Value[2].ToString()))
                    {
                        if (scheme.Key == "Chapter")
                        {
                            CreateComboxEdit(scheme, i);
                        }
                        else if (scheme.Key == "DTC")
                        {
                            CreateComboxEdit(scheme, i);

                        }
                        else if (bool.Parse(scheme.Value[5].ToString()))
                        {
                            CreateComboxEdit(scheme, i);

                        }
                        else
                        {
                            if (scheme.Key == "AssessItemRelevant")
                                continue;
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
            CreateButton();
            ContextMenu emptyMenu = new ContextMenu();
            foreach (LayoutControlItem item in itemList)
            {
                dpEmlLcg.AddItem(item);
                if (typeof(TextEdit) == item.Control.GetType())
                {
                    TextEdit text = item.Control as TextEdit;
                    text.Properties.ContextMenu = emptyMenu;
                }
            }
            
        }

        private void CreateComboxEdit(KeyValuePair<string, List<object>> scheme, int i)
        {
            LayoutControlItem lci = new LayoutControlItem();
            lci.TextLocation = DevExpress.Utils.Locations.Left;
            lci.Text = scheme.Value[1].ToString() + @"："; //中文列名
            lci.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 5, 5);
            lci.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            ComboBoxEdit boxEdit = new ComboBoxEdit();
            boxEdit.Name = scheme.Key;
            boxEdit.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lci.Control = boxEdit;
            lci.Location = new System.Drawing.Point(0, i*28);
            boxEdit.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;

            if (scheme.Key == "Chapter")
            {
                IList<string> chapterList = _store.GetSingnalCol(EnumLibrary.EnumTable.ExapChapter, 0);
                object[] boxitem = new object[chapterList.Count];
                for (int j = 0; j < chapterList.Count; j++)
                    boxitem[j] = chapterList[j];
                boxEdit.Properties.Items.AddRange(boxitem);
            }
            else if (scheme.Key == "DTC" && _isOnlyEml)
            {
                Dictionary<string, object> dictCNode = new Dictionary<string, object>();
                List<Dictionary<string, string>> listConfig = new List<Dictionary<string, string>>();
                List<Dictionary<string, string>> listDTC = new List<Dictionary<string, string>>();
                string[] nameLast = txtBelongNode.Caption.Split('：');
                string[] name = nameLast[0].Split('-');
                string match = _file.SearchBusByEmlName(GlobalVar.EmlClickItemName);
                dictCNode.Add("VehicelType", name[0]);
                dictCNode.Add("VehicelConfig", name[1]);
                dictCNode.Add("VehicelStage", name[2]);
                dictCNode.Add("MatchSort", match);
                dictCNode.Add("EmlTemplateName", _selectedName);
                IList<object[]> listFilecfg = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelRep,
                    dictCNode);
                foreach (var cfglist in listFilecfg)
                {
                    if (!string.IsNullOrWhiteSpace(cfglist[7].ToString()))
                    {
                        listConfig = Json.DerJsonToLDict(cfglist[7].ToString());
                        break;
                    }
                }
                //List<string> dtcList = new List<string>();
                Dictionary<string,object> dictRoad = new Dictionary<string, object>();
                dictRoad["CANRoad"] = GlobalVar.CANRoad;
                dictRoad["Module"] = GlobalVar.Module;
                Dictionary<string, string> dictDtc = new Dictionary<string, string>();
                dictDtc = _searchDtc.SearchDtcFaultInfor(listConfig, dictRoad);
                #region
                {
                    //foreach (Dictionary<string,string> dict in listConfig)
                    //{
                    //    listDTC = Json.DerJsonToLDict(dict["DTCRelevant"]);
                    //    foreach (Dictionary<string, string> dtcfault in listDTC)
                    //    {
                    //        List<Dictionary<string, string>> listfault = new List<Dictionary<string, string>>();
                    //        listfault = Json.DerJsonToLDict(dtcfault["FaultInfo"]);
                    //        foreach (Dictionary<string, string> dtc in listfault)
                    //        {
                    //            bool isSame = false;
                    //            foreach(var dtcname in dtcList )
                    //            {
                    //                if (dtcname == dtc["name"])
                    //                {
                    //                    isSame = true;
                    //                    break;
                    //                }
                    //            }
                    //            if(!isSame)
                    //            {
                    //                dtcList.Add(dtc["name"]);
                    //            }
                    //        }
                    //    }
                    //}
                }
                #endregion
                object[] boxitem = new object[dictDtc.Count];
                int j = 0;
                foreach (KeyValuePair<string,string> item in dictDtc)
                {
                    boxitem[j] = item.Key;
                    j++;
                }
                boxEdit.Properties.Items.AddRange(boxitem);
                boxEdit.ReadOnly = false;
            }
            else if (scheme.Key == "DTC" && !_isOnlyEml)
            {
                boxEdit.Text = "请在任务管理中用例编辑处加上DTC";
                boxEdit.Font = new System.Drawing.Font("微软雅黑", 6F, System.Drawing.FontStyle.Regular,
                    System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                boxEdit.ReadOnly = true;
            }
            else
            {
                var boxStrings = scheme.Value[6].ToString().Split(',');
                object[] boxitem = new object[boxStrings.Length];
                boxStrings.CopyTo(boxitem, 0);
                boxEdit.Properties.Items.AddRange(boxStrings);
            }
            //dpEmlLcg.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {lci});
            itemList.Add(lci);
        }

        private void CreateTextEdit(KeyValuePair<string, List<object>> scheme, int i)
        {
            LayoutControlItem lci = new LayoutControlItem();
            lci.TextLocation = DevExpress.Utils.Locations.Left;
            lci.Text = scheme.Value[1].ToString() + @"："; //中文列名
            lci.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 5, 5);
            lci.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            TextEdit textEdit = new TextEdit();
            textEdit.Name = scheme.Key;
            textEdit.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lci.Control = textEdit;
            lci.Location = new System.Drawing.Point(0, i*28);
            //this.dpEmlLcg.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {lci});
            itemList.Add(lci);
        }


        private void CreateButton()
        {
            int sizeHeight = 28;
            int colCount = CountColum();
            int btnLocation = sizeHeight * (colCount - 1);
            LayoutControlItem simpleButtonLCI = new LayoutControlItem();
            SimpleButton btnSub = new SimpleButton();
            simpleButtonLCI.Control = btnSub;
            simpleButtonLCI.ControlAlignment = System.Drawing.ContentAlignment.BottomCenter;
            simpleButtonLCI.Location = new System.Drawing.Point(0, btnLocation);
            //simpleButtonLCI.Name = "simpleButtonLCI";
            //simpleButtonLCI.Text = "simpleButtonLCI";
            simpleButtonLCI.TextVisible = false;

            //btnSub.Size = this.btnUp.Size = new System.Drawing.Size(228, 32);

            btnSub.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            btnSub.Appearance.Options.UseFont = true;

            

            btnSub.Location = new System.Drawing.Point(0, btnLocation);
            btnSub.Margin = new System.Windows.Forms.Padding(3, 3, 5, 5);
            btnSub.Name = "btnSub";
            btnSub.Size = new System.Drawing.Size(266, sizeHeight);
            btnSub.MinimumSize = new Size(0,28);
            btnSub.Text = @"确定";
            //dpEmlLcg.Items.AddRange(btnSub);
            //layoutControl3.Controls.Add(btnSub);
            btnSub.MouseClick += btnSub_Click;
            itemList.Add(simpleButtonLCI);
        }

        private enum DataOper
        {
            Add = 0,
            Modify = 1,

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
                if (_currentOperate == DataOper.Modify)
                {
                    if (selectRow == j)
                        continue;
                }
                bool boolChapter = false;
                bool boolExapID = false;
                primary = false;
                foreach (var lcItem in itemList)
                {
                    
                    if (typeof(SimpleButton) == lcItem.Control.GetType())
                        continue;
                    if (lcItem.Control.Name == "ReflectionID")
                    {
                        if (_dt.Rows[j]["ReflectionID"].ToString() == lcItem.Control.Text)
                        {
                            boolChapter = true;
                        }
                    }
                    else if (lcItem.Control.Name == "ExapID")
                    {
                        if (_dt.Rows[j]["ExapID"].ToString() == lcItem.Control.Text)
                        {
                            boolExapID = true;
                        }
                    }
                }
                if (boolChapter || boolExapID)
                {
                    primary = false;
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

        private void btnSub_Click(object sender, EventArgs e)
        {
            if (!PrimaryKey())
                return;
            foreach (LayoutControlItem item in itemList)
            {
                if (typeof(SimpleButton) == item.Control.GetType())
                    continue;
                if (item.Control.Name != "TestCaseDescription")
                {
                    if (item.Control.Text == "")
                    {
                        XtraMessageBox.Show("请检查所输入项是否填写完整");
                        return;
                    }
                }
                foreach (KeyValuePair<string, List<object>> scheme in GlobalVar.ListDrScheme)
                {
                    if (item.Control.Name == scheme.Key)
                    {
                        if (bool.Parse(scheme.Value[7].ToString()))
                        {
                            float result;
                            if (float.TryParse(item.Control.Text.ToString(), out result))
                            {
                                if (result < float.Parse(scheme.Value[8].ToString()) ||
                                    result > float.Parse(scheme.Value[9].ToString()))
                                {
                                    XtraMessageBox.Show("输入数据的大小超出指定范围，请重新输入！", "提示", MessageBoxButtons.OKCancel,
                                        MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                            else
                            {
                                XtraMessageBox.Show("输入数据格式有误，请输入数字！", "提示", MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Warning);
                                return;
                            }


                        }
                        if (bool.Parse(scheme.Value[3].ToString()))
                        {
                            //item.Control.Text
                        }

                    }
                }
            }

            switch (_currentOperate)
            {
                //添加
                case DataOper.Add:
                    
                    object[] row = GetDataFromDockPanel();
                    
                    row[gvEml.Columns.Count-2] = "";
                    row[gvEml.Columns.Count - 1] = "请右击查看评价项目信息";
                    //dr["AssessItemRelevant"] = "";
                    _dt.Rows.Add(row);
                    _dt = UpdateGridViewByOperate();
                    gcEmlTemplate.DataSource = _dt;
                    DerGridToJson();
                    Show(DLAF.LookAndFeel, this, "添加成功...", "", new[] { DialogResult.OK }, null, 0,
                        MessageBoxIcon.Information);
                    dpEml.Visibility = DockVisibility.Visible;
                    break;
                //更新
                case DataOper.Modify:
                    //object[] mrow = GetDataFromUI();
                    object[] mrow = GetDataFromDockPanel();
                    UpDataRow();
                    _dt = UpdateGridViewByOperate();
                    gcEmlTemplate.DataSource = _dt;
                    object[] newRow = new object[itemList.Count];
                    

                    DerGridToJson();
                    Show(DLAF.LookAndFeel, this, "修改成功...", "", new[] { DialogResult.OK }, null, 0,
                        MessageBoxIcon.Information);
                    ClearUI();
                    dpEml.Visibility = DockVisibility.Hidden;
                    break;

            }

        }

        private DataTable UpdateGridViewByOperate()
        {
            DataTable newDt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvEml.Columns)
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
                    if(!same)
                        chapterDiff.Add(row.ItemArray[0].ToString());
                    chapter = row.ItemArray[0].ToString();
                }
                
            }
            
            foreach(string chapterM in chapterDiff)
            {
                foreach (DataRow row in _dt.Rows)
                {
                    if (row.ItemArray[0].ToString() == chapterM)
                    {
                        newDt.Rows.Add(row.ItemArray);

                    }
                    
                }

            }
           return newDt;
        }



        private void UpDataRow()
        {
            int i = 0;
            foreach (LayoutControlItem lItem in itemList)
            {
                if (typeof(SimpleButton) == lItem.Control.GetType())
                    continue;
                _dt.Rows[selectRow][i] = lItem.Control.Text;
                i++;
            }

        }


        private object[] GetDataFromDockPanel()
        {
            object[] obj = new object[gvEml.Columns.Count];
            int i = 0;
            foreach (LayoutControlItem layoutItem in itemList)
            {
                if (typeof (SimpleButton) == layoutItem.Control.GetType())
                    continue;
                if (layoutItem.Control.Name == "TestCaseDescription")
                {
                    if (string.IsNullOrWhiteSpace(layoutItem.Control.Text))
                    {
                        obj[i] = "--";
                        i++;
                    }
                    else
                    {
                        obj[i] = layoutItem.Control.Text;
                        i++;
                    }
                }
                else
                {
                    obj[i] = layoutItem.Control.Text;
                    i++;
                }

            }
            //obj[i] = "请右击查看该用例的评价项目";
            //string chapterName = Chapter.Text;
            //string exapID = TestExapID.Text;
            //string reflectID = ReflectID.Text;
            //string exapName = ExapName.Text;
            //string testType = TestType.Text;
            //string testCount = TestCount.Text;
            //string assessItem = AssessItem.Text;
            //string minValue = MinValue.Text;
            //string normalValue = NormalValue.Text;
            //string maxValue = MaxValue.Text;
            //string description = Description.Text;
            //object[] obj = new object[]
            //{chapterName, exapID, reflectID, exapName, testType, testCount, assessItem, minValue, normalValue, maxValue,description};
            return obj;
        }

        private void ClearUI()
        {
            //Chapter.Text = "";
            //TestExapID.Text = "";
            //ReflectID.Text = "";
            //ExapName.Text = "";
            //TestType.Text = "";
            //TestCount.Text = "";
            //AssessItem.Text = "";
            //MinValue.Text = "";
            //NormalValue.Text = "";
            //MaxValue.Text = "";
            //Description.Text = "";
            foreach (LayoutControlItem item in itemList)
            {
                if (typeof(SimpleButton) == item.Control.GetType())
                    continue;
                item.Control.Text = "";
            }
        }

        /// <summary>
        /// 初始化gridview的列名
        /// </summary>
        private void InitDataTable()
        {
            _dt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvEml.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList.ToArray())
                _dt.Columns.Add(new DataColumn(colName, typeof (object)));
            gcEmlTemplate.DataSource = _dt;
        }

        public void DerGridToJson()
        {
            string error = "";
            string fjson = SerGridViewToJson(out error);
            SaveText(fjson);
            gcEmlTemplate.DataSource = _dt;
            //GlobalVar.EmlCache.SetExcell(_list);

        }

        private void SaveText(string fileJson)
        {
            string vehicel = GlobalVar.VNode[0] + "-" + GlobalVar.VNode[1] + "-" + GlobalVar.VNode[2] + "-";
            string path = GlobalVar.TemporaryFilePath + "example\\" + vehicel + _selectedName + ".txt";
            if (!_isOnlyEml)
            {
                path = GlobalVar.TemporaryFilePath + "example\\" + vehicel + _selectedName + ".txt";

            }
            else
            {
                string strModule = ConvertModuleJsonToString(GlobalVar.TaskName, GlobalVar.Module);
                strModule = strModule.Replace('/', '&');
                vehicel = GlobalVar.TaskNo + "-" + GlobalVar.TaskRound + "-" + GlobalVar.TaskName + "-" +
                          GlobalVar.CANRoad + strModule.Remove(strModule.Length - 1);
                path = GlobalVar.TemporaryFilePath + "taskEml\\" + vehicel+ ".txt";
            }
            StreamWriter sw = new StreamWriter(path,false, Encoding.UTF8);
            //写入
            sw.Write(fileJson);
            //清空
            sw.Flush();
            //关闭
            sw.Close();
            
        }

        private string ConvertModuleJsonToString(string node, string listNode)
        {
            string module = "";
            if (node != "")
            {

                //cbName.Properties.Items.AddRange(type);
                switch (node)
                {
                    case "CAN单节点":
                        module = listNode + "/";
                        break;
                    case "CAN集成":
                        module = GetMultiNodeString(listNode);
                        break;
                    case "J1939单节点":
                        module = listNode + "/";
                        break;
                    case "J1939集成":
                        module = GetMultiNodeString(listNode);
                        break;
                    case "LIN单节点":
                        module = listNode + "/";
                        break;
                    case "LIN集成":
                        module = GetMultiNodeString(listNode);
                        break;
                    case "OSEK单节点":
                        module = listNode + "/";
                        break;
                    case "OSEK集成":
                        module = GetMultiNodeString(listNode);
                        break;
                    case "总线相关DTC":
                        module = listNode + "/";
                        break;
                    case "路由信息":
                        module = listNode + "/";
                        break;
                    default:
                        break;
                }

            }
            return module;
        }
        private string GetMultiNodeString(string list)
        {

            Dictionary<string, string> name = Json.DerJsonToDict(list);
            string taskModule = "";
            foreach (KeyValuePair<string, string> item in name)
            {

                if (item.Key == "Virtual")
                {
                    if (taskModule != "")
                    {
                        //taskModule = taskModule.Split(' ')[0];
                        taskModule = taskModule + item.Value + "(" + "虚拟" + "）" + "/";
                    }
                    else
                    {
                        taskModule = item.Value + "(" + "虚拟" + "）" + "/";
                    }
                }
                else if (item.Key == "Normal")
                {
                    taskModule = taskModule + item.Value + "/";

                }

            }

            return taskModule;

        }

        private int CountColum()
        {
            int colCount = 0;
            foreach (KeyValuePair<string, List<object>> scheme in GlobalVar.ListDrScheme)
            {
                if (bool.Parse(scheme.Value[2].ToString()))
                {
                    colCount++;
                }

            }
            return colCount;
            //int dicCount = GlobalVar.ListDrScheme.Count();
        }

        /// <summary>
        /// 初始化gvEml
        /// </summary>
        private void CreateGridView(Dictionary<string, List<object>> listDrScheme)
        {
            gvEml.Columns.Clear();
            ///GridColumn[] collection = new GridColumn[] { };
            
            foreach (KeyValuePair<string, List<object>> scheme in listDrScheme)
            {

                if (bool.Parse(scheme.Value[2].ToString()))
                {
                    if (scheme.Key == "AssessItemRelevant")
                    {
                        GridColumn col = new GridColumn();
                        col.Caption = scheme.Value[1].ToString();
                        col.Name = scheme.Key;
                        col.FieldName = scheme.Key;
                        col.Visible = false;
                        gvEml.Columns.AddRange(new GridColumn[] { col });
                    }
                    else
                    {
                        GridColumn col = new GridColumn();
                        col.Caption = scheme.Value[1].ToString();
                        col.Name = scheme.Key;
                        col.FieldName = scheme.Key;
                        col.Visible = true;
                        gvEml.Columns.AddRange(new GridColumn[] { col });
                    }

                }

            }
            //gvEml.Columns[gvEml.Columns.Count - 1].Visible = false;
            GridColumn colNew = new GridColumn();
            colNew.Caption = "评价项目相关";
            colNew.Name = "CheckItem";
            colNew.FieldName = "CheckItem";
            colNew.Visible = true;
            gvEml.Columns.AddRange(new GridColumn[] { colNew });


        }


        //将txt装入gridView中的函数
        private void DerDictToGridView(string eml, string colEml)
        {

            var dictChapter = new Dictionary<string, Dictionary<string, List<object>>>();
            dictChapter = Json.DeserJsonToDDict(eml);
            //var dictChapter = new List<Dictionary<string, string>>();
            //dictChapter = Json.DerJsonToLDict(eml);
            GlobalVar.ListDrScheme = Json.DeserJsonDList(colEml);
            if (GlobalVar.ListDrScheme == null)
                return;
            _dt.Clear();
            DataTable dt = new DataTable();

            CreateGridView(GlobalVar.ListDrScheme);
            InitDataTable();
            int colCount = gvEml.Columns.Count;
            
            //GlobalVar.DictAssessItem.Clear();
            //foreach (Dictionary<string,string> scheme in dictChapter)
            //{
            //    object[] obje = new object[colCount];
            //    foreach (KeyValuePair<string, string> chapter in scheme)
            //    {
                    
            //        //if (chapter.Key != "AssessItemRelevant")
                    
            //            obje[col] = chapter.Value;
            //            col++;
            //    }
            //    obje[col] = "请先右击查看评价项目";
            //    _dt.Rows.Add(obje);
            //}
            gcEmlTemplate.DataSource = _dt;
            foreach (KeyValuePair<string, Dictionary<string, List<object>>> scheme in dictChapter)
            {
                object[] obj = new object[colCount];
                int i = 0;
                obj[i] = scheme.Key;
                i++;
                foreach (KeyValuePair<string, List<object>> col in scheme.Value)
                {
                    i = 1;
                    var ss = Json.DerJsonToDict(col.Value[0].ToString());
                    foreach (KeyValuePair<string, string> dict in ss)
                    {
                        obj[i] = dict.Value;
                        i++;
                    }
                    var dd = col.Value[1].ToString();
                    obj[i] = dd;
                    i++;
                    obj[i] = "请右击查看评价项目信息";
                    //int j = i;
                    //foreach (var list in dd)
                    //{
                    //    i = j;
                    //    foreach (var dict in list.Values)
                    //    {
                    //        obj[i] = dict;
                    //        i++;

                    //    }
                    _dt.Rows.Add(obj);
                    gcEmlTemplate.DataSource = _dt;
                    //}

                }

            }

        }

        /// <summary>
        /// 根据Json初始化GridView的列名
        /// </summary>
        /// <param name="_list"></param>



        private IList<object[]> ReadDataByVehicel(List<string> node)
        {
            IList<object[]> listFile = new List<object[]>();
            if (node.Count != 0)
            {
                Dictionary<string, object> dictCNode = new Dictionary<string, object>();
                dictCNode.Add("VehicelType", node[0]);
                dictCNode.Add("VehicelConfig", node[1]);
                dictCNode.Add("VehicelStage", node[2]);
                dictCNode.Add("EmlTemplateName", node[3]);
                listFile = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelDouble, dictCNode);

            }
            return listFile;
        }

        private void BtnSubmitImage()
        {
            Dictionary<string, object> VehicelNodes = new Dictionary<string, object>();
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["TaskNo"] = GlobalVar.TaskNo;
            dict["TaskRound"] = GlobalVar.TaskRound;
            dict["TaskName"] = GlobalVar.TaskName;
            dict["CANRoad"] = GlobalVar.CANRoad;
            dict["Module"] = GlobalVar.Module;

            VehicelNodes["VehicelType"] = GlobalVar.VNode[0];
            VehicelNodes["VehicelConfig"] = GlobalVar.VNode[1];
            VehicelNodes["VehicelStage"] = GlobalVar.VNode[2];
            VehicelNodes["EmlTemplateName"] = _selectedName;
           
            if (_isOnlyEml)
            {
                //任务
                IList<object[]> taskList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.TaskTest, dict);
                if (taskList .Count != 0 && taskList[0][11].ToString() == "")
                {
                    btnSubmit.ImageIndex = 6;
                }
                
                ///else if()
            }
            else
            {
                //车型
                IList<object[]> List = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelDouble, VehicelNodes);
                if (List.Count == 0 || List[0][10].ToString() == "System.Byte[]" || List[0][10].ToString() == "" ||
                    List[0][10].ToString() == null)
                {
                    btnSubmit.ImageIndex = 6;
                }
                else
                    btnSubmit.ImageIndex = 5;
            }
        }

        public void FormClosed()
        {
            if (th != null && th.IsAlive)
                th.Abort();
        }

        private string SerGridViewToJson(out string error)
        {
            //try
            //{
                var currentChapter = "";
                var currentAssItem = "";
                var dictChapter = new Dictionary<string, Dictionary<string, List<object>>>();
                var dictAssItem = new Dictionary<string, List<object>>();
                var listChapter = new List<Dictionary<string, string>>();
                var listExample = new List<object>();
                List<Dictionary<string, string>> listItem = new List<Dictionary<string, string>>();
                var coList = new List<string>();
                foreach (GridColumn col in gvEml.Columns)
                    coList.Add(col.FieldName);
                foreach (DataRow row in _dt.Rows)
                {
                    //Dictionary<string, string> chapter = new Dictionary<string, string>();
                    int i = 0;
                    //for(int i = 0;i < coList.Count-1;i ++)
                    //{
                    //    chapter[coList[i]] = row[coList[i]].ToString();
                    //}
                    //string key = row[coList[0]] + "-" + row[coList[1]];
                    //bool same = false;
                    //foreach (KeyValuePair<string, string> item in GlobalVar.DictAssessItem)
                    //{
                    //    if(item.Key == key)
                    //    {
                    //        chapter[coList[coList.Count - 1]] = item.Value;
                    //        same = true;
                    //        break;
                    //    }
                    //}
                    //if(!same)
                    //    chapter[coList[coList.Count - 1]] = "";
                    //listChapter.Add(chapter);
                    var col = 1;
                    var chapterName = row.ItemArray[0].ToString();
                    var assessItem = row.ItemArray[1].ToString();
                    if (assessItem != currentAssItem && assessItem != "")
                    {
                        if (listItem.Count != 0)
                        {
                            listExample.Add(listItem);
                            dictAssItem.Add(currentAssItem, listExample);
                            listExample = new List<object>();
                            listItem = new List<Dictionary<string, string>>();
                        }
                        currentAssItem = assessItem;
                    }
                    if (chapterName != currentChapter && chapterName != "")
                    {
                        if (dictAssItem.Count != 0)
                        {
                            dictChapter.Add(currentChapter, dictAssItem);
                            dictAssItem = new Dictionary<string, List<object>>();
                        }
                        currentChapter = chapterName;
                    }

                    if (listItem.Count == 0 && listExample.Count == 0)
                    {
                        var dictProperty = new Dictionary<string, string>();
                        dictProperty = InitDictProperty(_dt);
                        var keyNames = new string[dictProperty.Keys.Count];
                        dictProperty.Keys.CopyTo(keyNames, 0);
                        foreach (var name in keyNames)
                        {
                            dictProperty[name] = row.ItemArray[col].ToString();
                            col++;
                        }
                        listExample.Add(dictProperty);

                    }

                    //if(row.ItemArray[col].ToString() == "" )
                    //    listItem.Add("");



                    if (row.ItemArray[col].ToString() != "")
                    {
                        var itemList = Json.DerJsonToLDict(row.ItemArray[col].ToString());
                        foreach (var list in itemList)
                        {
                            var dictItem = new Dictionary<string, string>();
                            dictItem = InitDictItem();
                            foreach (KeyValuePair<string, string> dict in list)
                            {
                                dictItem[dict.Key] = dict.Value;
                            }
                            listItem.Add(dictItem);
                        }
                    }
                    else
                    {
                        var dictItem = new Dictionary<string, string>();
                        dictItem = InitDictItem();
                        listItem.Add(dictItem);
                    }
                    //row.ItemArray[col];

                    //var localkeyNames = new string[dictItem.Keys.Count];
                    //dictItem.Keys.CopyTo(localkeyNames, 0);
                    //col = 6;
                    //foreach (var name in localkeyNames)
                    //{
                    //    dictItem[name] = row.ItemArray[col].ToString();
                    //    col++;
                    //}
                    //listItem.Add(dictItem);
                }
                if (listItem.Count != 0)
                {
                    listExample.Add(listItem);
                    dictAssItem.Add(currentAssItem, listExample);
                    dictChapter.Add(currentChapter, dictAssItem);
                }

                error = "";
                return Json.SerJson(dictChapter);
            //}
            //catch (Exception e)
            //{
            //    error = e.ToString();
            //    throw;
            //}

        }
    


        private static Dictionary<string, string> InitDictProperty(DataTable dt)
        {
            Dictionary<string, string> dictProperty = new Dictionary<string, string>();
             foreach (DataColumn col in dt.Columns)
            {
                if(col.ToString() == "Chapter")
                    continue;
                if (col.ToString() == "AssessItemRelevant")
                {
                    break;
                }
                dictProperty.Add(col.ToString(),"");
            }
            //dictProperty.Add("ExapID", "");
            //dictProperty.Add("ReflectionID", "");
            //dictProperty.Add("ExapName", "");
            //dictProperty.Add("TestType", "");
            //dictProperty.Add("TestCount", "");
            return dictProperty;

        }
        private static Dictionary<string, string> InitDictItem()
        {
            Dictionary<string, string> dictItem = new Dictionary<string, string>();
            //for (int j = dt.Columns.IndexOf("AssessItem"); j < dt.Columns.Count; j ++)
            //{
            //    dictItem.Add(dt.Columns[j].ColumnName, "");
            //}
            dictItem.Add("AssessItem", "");
            dictItem.Add("MinValue", "");
            dictItem.Add("NormalValue", "");
            dictItem.Add("MaxValue", "");
            dictItem.Add("ValueDescription", "");
            return dictItem;
        }


        #region 名称点击事件
        private void NameItem_Click(object sender, EventArgs e)
        {
            _dr = null;
            ////判断是否依赖节点
            var item = sender as NavBarItem;
            if (item != null)
            {
                GlobalVar.SelectName = item.Caption;
                _selectedName = item.Caption;
                GlobalVar.EmlClickItemName = _selectedName;
                BtnSubmitImage();
            }
            if (oldBarItem != null && oldBarItem != item)
            {
                oldBarItem.Appearance.ForeColor = Color.Black;

            }
            if (item != null)
            {
                item.Appearance.ForeColor = Color.Blue;
                oldBarItem = item;
            }
            Dictionary<string, object> dictCNode = new Dictionary<string, object>();
            string[] nameLast = txtBelongNode.Caption.Split('：');
            string[] name = nameLast[1].Split('-');
            string match = _file.SearchBusByEmlName(_selectedName);
            dictCNode.Add("VehicelType", name[0]);
            dictCNode.Add("VehicelConfig", name[1]);
            dictCNode.Add("VehicelStage", name[2]);
            dictCNode.Add("MatchSort", match);
            dictCNode.Add("EmlTemplateName", _selectedName);
            IList<object[]> listFilecfg = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelRep,
                dictCNode);//配置表
            if (listFilecfg.Count == 0)
            {
                XtraMessageBox.Show("请先完成该车型下的" +match +"配置表");
                return;
            }
            InitEmlPage();
            //btnColSet.Enabled = true;
            //GetExampleTemp(fileEidtor);
            ////SaveTime();
        }
        #endregion




        void ITemplate.DrawNav()
        {
            nbcTempList.Items.Clear();
            GlobalVar.ListEmpTemp.Clear();
            //得到所有模板表中的模板信息
            IList<object[]> exampleList = new List<object[]>();
            IList<object[]> emlList = new List<object[]> ();
            IList<object[]> exampleListNew = new List<object[]>();
            var dictTem = new Dictionary<string, object>();
            if (GlobalVar.SelectName == "" && !GlobalVar.IsIndependent &&!_isOnlyEml)//车型处进入
            {
                Dictionary<string, string> dictCNode = new Dictionary<string, string>();
                string[] nameL = txtBelongNode.Caption.Split('：'); 
                string[] nameV = nameL[1].Split('-');
                dictCNode.Add("VehicelType", nameV[0]);
                dictCNode.Add("VehicelConfig", nameV[1]);
                dictCNode.Add("VehicelStage", nameV[2]);
                IList<object[]> fileLinkList = BaseSqlOrder.GetMultipleByQuery(ExcuteSqlCase.FileLinkByVehicel_Query(dictCNode));//配置表
                
                exampleList = _store.GetRegularByEnum(EnumLibrary.EnumTable.ExampleTemp);
                foreach (var emlItem in exampleList)
                {
                    string match = _file.SearchBusByEmlName(emlItem[0].ToString());
                    bool find = false;
                    foreach (var flist in fileLinkList)
                    {
                        if (match == flist[3].ToString())
                        {
                            find = true;
                            break;
                        }
                    }
                    if (find)
                        exampleListNew.Add(emlItem);
                }
            }
            //else if(GlobalVar.CfgClickItemName != "")
            //{
            //    string nameExap = GlobalVar.CfgClickItemName.Split('配')[0];
            //    //nameExap = nameExap + "用例表";
            //    dictTem.Add("MatchSort", nameExap);
            //    exampleList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ExampleMatch, dictTem);
            //}
            else if (GlobalVar.EmlClickItemName != "" && _isOnlyEml)//任务处进入
            {
                string nameExap = GlobalVar.EmlClickItemName;
                //nameExap = nameExap + "用例表";
                dictTem.Add("Name", nameExap);
                exampleListNew = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ExampleTemp, dictTem);
            }
            else if(!_isOnlyEml &&GlobalVar.IsIndependent)//文件编辑器进入
            {
                foreach (var list in exampleList)
                {
                    exampleListNew.Add(list);
                }
                //_tem.DrawExcel(GlobalVar.SelectName);
                //CreateDockPanel(emlTem);
            }
            var row = 0;
            foreach(var list in exampleListNew)
            //foreach (var list in exampleList)
            {
                if (GlobalVar.ListEmpTemp.Count != 0)
                {
                    emlList.Clear();
                    foreach (var eml in GlobalVar.ListEmpTemp)
                        emlList.Add(eml);
                    bool flag = false;
                    foreach (var newList in emlList)
                    {
                        if (list[0].ToString() == newList[0].ToString())
                        {
                            //if (list[1].ToString() == "V2.0")
                            {
                                var index = GlobalVar.ListEmpTemp.IndexOf(newList);
                                GlobalVar.ListEmpTemp[index] = list;
                                flag = true;
                                break;
                            }
                        }
                        
                    }
                    if (!flag)
                    {
                        GlobalVar.ListEmpTemp.Add(list);
                    }
                }
                else if (GlobalVar.ListEmpTemp.Count == 0)
                {
                    GlobalVar.ListEmpTemp.Add(list);
                }
               
            }

            GlobalVar.DictEmpTemp.Clear();
            foreach (var name in GlobalVar.ListEmpTemp)
            {
               var col = 0;
                var keys = new string[_dictExample.Keys.Count];
                _dictExample.Keys.CopyTo(keys, 0);
                foreach (var itemTemp in keys)
                {
                    _dictExample[itemTemp] = name[col];
                    col++;
                }
                temList.AddItem();
                nbcTempList.Items[row].Caption = _dictExample["Name"].ToString();
                nbcTempList.Items[row].Appearance.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                nbcTempList.Items[row].AppearancePressed.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                //nbcTempList.Items[row].a.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                nbcTempList.Items[row].LinkClicked += NameItem_Click;
                //nbcTempList.Items[row].+= NameItem_Click;
                row++;
                Dictionary<string,object> emlDictionary=new Dictionary<string, object>();
                emlDictionary["Name"] = _dictExample["Name"];
                emlDictionary["Version"] = _dictExample["Version"];
                emlDictionary["Content"] = _dictExample["Content"];
                emlDictionary["MatchSort"] = _dictExample["MatchSort"];
                GlobalVar.DictEmpTemp.Add(emlDictionary);
            }
            //if (GlobalVar.SelectName != "")
            //{
            //    string nameExap = GlobalVar.SelectName.Split('用')[0];
            //    nameExap = nameExap + "用例表";
            //    foreach (NavBarItem item in nbcTempList.Items)
            //    {
            //        if (item.Caption == nameExap)
            //        {
            //            item.Appearance.ForeColor = Color.Blue;
            //            oldBarItem = item;
            //        }
            //    }
            //}

        }

        void ITemplate.InitDict()
        {
            _dictExample.Add("Name", "");
            _dictExample.Add("Version", "");
            _dictExample.Add("Content", "");
            _dictExample.Add("MatchSort", "");
            _dictExample.Add("ImportTime", "");
        }

        void ITemplate.DrawTem(string temName)
        {

            bool isFind = false;
            //_tem.DrawNav();
            foreach (var dict in GlobalVar.DictEmpTemp)
            {
                if (dict["Name"].ToString() != temName)
                {
                   continue;
                }
                emlTem = dict["Content"].ToString();
                GlobalVar.VersionEmlTem = dict["Version"].ToString();
                isFind = true;
                break;
            }
            if (!isFind)
            {
                //th.Abort();
                Show(DLAF.LookAndFeel, this, "数据库异常...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
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


        private void btnSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!_isOnlyEml)
            {
                if (th != null && th.IsAlive)
                    th.Abort();
                else
                {
                   Show(DLAF.LookAndFeel, this, "用例表不能为空....", "", new[] { DialogResult.OK }, null, 0,
                          MessageBoxIcon.Information);
                   return;
                }
            }       
            try
            {
                if (th != null && th.IsAlive)
                {
                    th.Abort();
                    DerGridToJson();
                }
            }
            catch (Exception ex)
            {
                Show(DLAF.LookAndFeel, this, "当前需要保存的文档可能已经打开,请关闭后重试...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                throw;
            }
        }


        private void btnSubmit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_dt.Rows.Count == 0 )
            {
                Show(DLAF.LookAndFeel, this, "用例表不能为空....", "", new[] { DialogResult.OK }, null, 0,
                        MessageBoxIcon.Information);
                return;
            }
            try
            {
                if (!fileEidtor)
                {
                    SubmitExcelToFileLink(_isOnlyEml);
                }
                else
                {
                    btnInitVersion.Enabled = true;
                    GetFileEditorExmp();
                }
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                Show(DLAF.LookAndFeel, this, error, "", new[] {DialogResult.OK}, null, 0, MessageBoxIcon.Information);
            }

        }

        private void SubmitExcelToFileLink(bool eml)
        {
            if (eml == false)
            {
                string name = txtBelongNode.Caption;
                List<string> list = new List<string>();
                string newName = name.Split('：')[1];
                list.Add(newName.Split('-')[0]);
                list.Add(newName.Split('-')[1]);
                list.Add(newName.Split('-')[2]);
                Dictionary<string, object> dictCNode = new Dictionary<string, object>();

                dictCNode.Add("VehicelType", list[0]);
                dictCNode.Add("VehicelConfig", list[1]);
                dictCNode.Add("VehicelStage", list[2]);
                string match = _file.SearchBusByEmlName(_selectedName);
                dictCNode.Add("MatchSort", match);
                dictCNode.Add("EmlTemplateName", _selectedName);
                IList<object[]> listFile = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelDouble,
                    dictCNode);//查找用例表
                IList<object[]> listFilecfg = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelRep,
                    dictCNode);//查找配置表
                if (listFilecfg.Count == 0 || listFilecfg[0][7].ToString() == "System.Byte[]" ||
                    listFilecfg[0][7] == null || listFilecfg[0][7].ToString() == "")
                {
                    Show(DLAF.LookAndFeel, this, "请先完成配置管理...", "", new[] {DialogResult.OK}, null, 0,
                        MessageBoxIcon.Information);
                    return;
                }
                
            }
            if (!GlobalVar.IsOnlyEml && GlobalVar.IsIndependent)
            {
                Show(DLAF.LookAndFeel, this, "独立状态下此功能未开放...", "", new[] {DialogResult.OK}, null, 0,
                    MessageBoxIcon.Information);
            }
            bool isModify = false;
            //task里面调用用例表时
            if (_isOnlyEml)
            {
                isModify = SubmitPartExmp(); //任务
            }
            else
                isModify = SubmitGlobalExmp(); //车型
            if (isModify)
            {
                Show(DLAF.LookAndFeel, this, "提交成功...", "", new[] {DialogResult.OK}, null, 0,
                    MessageBoxIcon.Information);
                BtnSubmitImage();
                //ctlExcelExample.ActiveWorksheet.Clear(_sheet.GetDataRange());
                btnSubmit.ImageIndex = 5;

                GlobalVar.EmlTemplateIsOk = true;
                string vehicel = GlobalVar.VNode[0] + "-" + GlobalVar.VNode[1] + "-" + GlobalVar.VNode[2] + "-";
                //string[] strFiles = Directory.GetFiles(GlobalVar.TemporaryFilePath + "example" );
                string[] strFiles = Directory.GetFiles(GlobalVar.TemporaryFilePath + "example");

                if (strFiles.Length != 0 && !_isOnlyEml)
                {
                    File.Delete(GlobalVar.TemporaryFilePath + "example\\" + vehicel + _selectedName + ".txt");
                }
                else if (strFiles.Length != 0 && _isOnlyEml)
                {

                    File.Delete(GlobalVar.TemporaryFilePath + "taskEml\\" + vehicel.Remove(vehicel.Length - 1) + ".txt");
                }
            }
        }

        private void GetFileEditorExmp()
        {
            //_sheet = ctlExcelExample.ActiveWorksheet;
            if (_sheet.GetDataRange().RowCount == 0) return;
            string error, exstr;
            //byte[] byteEml = ctlExcelExample.SaveDocument(DocumentFormat.Xlsx);
           
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                {"Name", _selectedName},
            };
            IList<object[]> listEml = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ExampleTemp,
                            dict);
            //dict = new Dictionary<string, object>
            //        {
            //            {"Name", _selectedName},
            //            {"Version","V2.0" },
            //            {"Content",byteEml },
            //            {"MatchSort",listEml[0][3].ToString() },
            //            {"ImportDate",DateTime.Now },
            //        };
            if (listEml.Count == 1)
            {
                _store.AddExampleTemp(dict, out error);
            }
            else
            {
                _store.Update(EnumLibrary.EnumTable.ExampleTemp,dict, out error);
            }
            string[] strFiles = Directory.GetFiles(GlobalVar.TemporaryFilePath + "example");

            if (strFiles.Length != 0)
            {
                File.Delete(GlobalVar.TemporaryFilePath + "template\\" +  _selectedName + ".xlsx");
            }

        }
        private void btnRefreah_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _tem.DrawNav();
        }

        private bool SubmitGlobalExmp()
        {
            string error = "",exstr;
            string hexEml = SerGridViewToJson( out exstr);
            string match = _file.SearchBusByEmlName(_selectedName);
            string name = txtBelongNode.Caption;
            bool isSuc = false;
            List<string> list = new List<string>();
            string newName = name.Split('：')[1];
            list.Add(newName.Split('-')[0]);
            list.Add(newName.Split('-')[1]);
            list.Add(newName.Split('-')[2]);
            Dictionary<string, object> dictCNode = new Dictionary<string, object>();

            dictCNode.Add("VehicelType", list[0]);
            dictCNode.Add("VehicelConfig", list[1]);
            dictCNode.Add("VehicelStage", list[2]);
            dictCNode.Add("MatchSort", match);
            dictCNode.Add("EmlTemplateName", _selectedName);
            IList<object[]> listFile = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelDouble,
                dictCNode);//查找用例表
            IList<object[]> listFilecfg = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelRep,
                dictCNode);//配置表
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                {"EmployeeName", GlobalVar.UserName},
                {"EmployeeNo", GlobalVar.UserNo},
                {"Department", GlobalVar.UserDept},

                {"VehicelType", GlobalVar.VNode[0]},
                {"VehicelConfig", GlobalVar.VNode[1]},
                {"VehicelStage", GlobalVar.VNode[2]},
                {"MatchSort",match },
                {"EmlTemplate", hexEml},
                {"EmlTemplateName",_selectedName },
                { "EmlTableColEdit" ,Json.SerJson(GlobalVar.ListDrScheme)},
                { "OldEmlTemplateName",_selectedName }
            };
            bool isFind = false;
            if (listFilecfg.Count > 0 && listFile.Count > 0)
            {
                foreach (var listEml in listFile)
                {
                    if (listEml[9].ToString() == _selectedName)
                    {
                        if (_store.Update(EnumLibrary.EnumTable.FileLinkByVehicelEml, dict, out error))
                        {
                            isFind = true;
                            isSuc = true;
                            break;
                        }

                    }
                }
            }
            else if(listFilecfg.Count == 1 && string.IsNullOrEmpty(listFilecfg[0][10].ToString()))
            {
                if (_store.Update(EnumLibrary.EnumTable.FileLinkByVehicelEmlSort, dict, out error))
                {
                    isFind = true;
                    isSuc = true;
                }
                
            }
            if (!isFind&& listFile.Count == 0)
            {
                dict["VehicelType"] = listFilecfg[0][0];
                dict["VehicelConfig"] = listFilecfg[0][1];
                dict["VehicelStage"] = listFilecfg[0][2];
                dict["MatchSort"] = listFilecfg[0][3];
                dict["Topology"] = listFilecfg[0][4];
                dict["CfgTemplateName"] = listFilecfg[0][5];
                dict["CfgTemplate"] = listFilecfg[0][6];
                dict["CfgTemplateJson"] = listFilecfg[0][7];
                dict["CfgBaudJson"] = listFilecfg[0][8];
                dict["TplyDescrible"] = listFilecfg[0][11];
                dict["ConTableColEdit"] = listFilecfg[0][12];
                dict["EmlTableColEdit"] = Json.SerJson(GlobalVar.ListDrScheme);
                _store.AddFileLinkByVehicel(dict, out error);
                if (error == "")
                {
                    isSuc = true;
                }
            }
            //if ((listFilecfg.Count == 2 && listFile.Count == 1))
            //    _store.Update(EnumLibrary.EnumTable.FileLinkByVehicelEml, dict, out error);
            //else if (listFilecfg.Count == 1)
            //{
            //    if (listFilecfg[0][9].ToString() == "" || (listFilecfg[0][9].ToString() == _selectedName))
            //        _store.Update(EnumLibrary.EnumTable.FileLinkByVehicelEmlSort, dict, out error);
            //}
            //else if (listFilecfg.Count == 1 && listFilecfg[0][9].ToString() != "" &&listFilecfg[0][9].ToString() != _selectedName)
            //{
            //    dict["VehicelType"] = listFilecfg[0][0];
            //    dict["VehicelConfig"] = listFilecfg[0][1];
            //    dict["VehicelStage"] = listFilecfg[0][2];
            //    dict["MatchSort"] = listFilecfg[0][3];
            //    dict["Topology"] = listFilecfg[0][4];
            //    dict["CfgTemplateName"] = listFilecfg[0][5];
            //    dict["CfgTemplate"] = listFilecfg[0][6];
            //    dict["CfgTemplateJson"] = listFilecfg[0][7];
            //    dict["CfgBaudJson"] = listFilecfg[0][8];
            //    dict["TplyDescrible"] = listFilecfg[0][11];
            //    dict["ConTableColEdit"] = listFilecfg[0][12];
            //    dict["EmlTableColEdit"] = listFilecfg[0][13];
            //    _store.AddFileLinkByVehicel(dict, out error);
            //}

            if (error == "")
            {
                Dictionary<string, object> dictConfig = new Dictionary<string, object>
                 {
                   { "EmployeeNo",GlobalVar.UserNo},
                   {"EmployeeName",GlobalVar.UserName},
                   {"OperTable","上传用例表"},
                   {"VehicelType",   GlobalVar.VNode[0]},
                   {"VehicelConfig",  GlobalVar.VNode[1]},
                   {"VehicelStage",  GlobalVar.VNode[2]}
                 };

                Log.WriteLog(EnumLibrary.EnumLog.CfgTemplate, dictConfig);
               
            }
            return isSuc;
        }
        #region
        #endregion
        private bool SubmitPartExmp()
        {
            string error = "";
            bool isModify = true;
            if (GlobalVar.EmlClickItemName == "总线相关DTC用例表")
            {
                foreach (DataRow row in _dt.Rows)
                {
                    if (row["DTC"].ToString() == "请在任务管理中用例编辑处加上DTC")
                    {
                        XtraMessageBox.Show("用例表中DTC编号尚未全部加上，请加完再提交");
                        isModify = false;
                        break;
                    }
                }
                if (!isModify)
                    return isModify;
            }
            string hexExmp = SerGridViewToJson(out error);
            
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                {"EmployeeName", GlobalVar.UserName},
                {"EmployeeNo", GlobalVar.UserNo},
                {"Department", GlobalVar.UserDept},

                {"TaskNo", GlobalVar.TaskNo},
                {"TaskRound", GlobalVar.TaskRound},
                {"TaskName", GlobalVar.TaskName},
                {"CANRoad", GlobalVar.CANRoad},
                {"Module", GlobalVar.Module},
                {"ContainExmp", hexExmp},
            };
            if (GlobalVar.EmlClickItemName == "总线相关DTC用例表")
            {
                dict["Remark"] = "DTC";
            }
            else
            {
                dict["Remark"] = "Normal";
            }
            _store.Update(EnumLibrary.EnumTable.TaskDTC, dict, out error);
            
            if (error == "")
            {
                btnSubmit.ImageIndex = 5;
                //Log.WriteLog(EnumLibrary.EnumLog.TaskEmlTemplate, dict);
            }
            return isModify;
        }



        public static DialogResult Show(UserLookAndFeel look, IWin32Window owner, string text, string caption, DialogResult[] buttons, Icon icon, int defaultButton, MessageBoxIcon messageBeepSound)
        {

            XtraMessageBoxForm form = new XtraMessageBoxForm();
            Font defaultFont = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            form.Appearance.Font = defaultFont;
            return form.ShowMessageBoxDialog(new XtraMessageBoxArgs(look, owner, text, caption, buttons, icon, defaultButton));
        }

        private void btnColSet_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!GlobalVar.IsIndependent)
            {
                List<string> listV = new List<string>
                {
                    GlobalVar.VNode[0],
                    GlobalVar.VNode[1],
                    GlobalVar.VNode[2],
                    _selectedName
                };
                IList<object[]> file = ReadDataByVehicel(listV);
                if (file.Count != 0&&file[0][10].ToString() != "")
                {
                    if (
                        XtraMessageBox.Show("该车型已经上传过用例表，再次进行列编辑的话数据库中的数据将被删除，是否是否继续列编辑？", "提示",
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning) ==
                        DialogResult.OK)
                    {
                        ShowColEdit();
                        string match = _file.SearchBusByEmlName(_selectedName);
                        Dictionary<string, object> dict = new Dictionary<string, object>
                            {
                                {"EmployeeName", GlobalVar.UserName},
                                {"EmployeeNo", GlobalVar.UserNo},
                                {"Department", GlobalVar.UserDept},

                                {"VehicelType", GlobalVar.VNode[0]},
                                {"VehicelConfig", GlobalVar.VNode[1]},
                                {"VehicelStage", GlobalVar.VNode[2]},
                                {"MatchSort", match},
                                {"EmlTemplateName", ""},
                                {"EmlTemplate", ""},
                                {"OldEmlTemplateName", _selectedName}
                                
                            };
                        string error = "";
                        _store.Update(EnumLibrary.EnumTable.FileLinkByVehicelEml, dict, out error);
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

            //SetExcell(GlobalVar.ChangList);
            //ctlExcelExample.ActiveWorksheet.Columns.Insert(19);
            //ctlExcelExample.ActiveWorksheet.Rows[1][19].Value = "ss";
        }

        private void ShowColEdit()
        {
            EmlColEditor ad = new EmlColEditor(_isOnlyEml);
            ad.ShowDialog();
            //GlobalVar.SelectName = "";
            if (GlobalVar.IsColumEdit && !GlobalVar.IsIndependent)
            {
                
                List<string> listV = new List<string>
                {
                    GlobalVar.VNode[0],
                    GlobalVar.VNode[1],
                    GlobalVar.VNode[2],
                    _selectedName
                };
                IList<object[]> file = ReadDataByVehicel(listV);
                GlobalVar.ListDrScheme = Json.DeserJsonDList(file[0][13].ToString());
                CreateGridView(GlobalVar.ListDrScheme);
                InitDataTable();

                CreateDockPanel(GlobalVar.ListDrScheme);
                foreach (var content in GlobalVar.DictCfgTemp)
                {
                    if (content["Name"].ToString() == _selectedName)
                    {
                        content["Content"] = GlobalVar.ListDrScheme;
                        break;
                    }
                }
                GlobalVar.IsColumEdit = false;
                if (!GlobalVar.IsIndependent)
                    btnColSet.Enabled = false;
            }
            else if(GlobalVar.IsIndependent)
            {
               
                if (GlobalVar.SelectName == "")
                {
                    _tem.DrawNav();
                    if(_selectedName != "")
                        _tem.DrawTem(_selectedName);
                }
                if (emlTem != null && emlTem != "")
                {
                    //IList<object[]> file = ReadDataByVehicel(GlobalVar.VNode);
                    GlobalVar.ListDrScheme = Json.DeserJsonDList(emlTem);
                    CreateGridView(GlobalVar.ListDrScheme);
                    InitDataTable();
                }
                    //CreateDockPanel(GlobalVar.ListDrScheme);

                    GlobalVar.IsColumEdit = false;
                    if (!GlobalVar.IsIndependent)
                        btnColSet.Enabled = false;
                
            }

        }


        //private Range GetInsertCol(int col)
        //{
        //    char init = 'A';
        //    int examRow = _sheet.GetDataRange().RowCount;
        //    char colInit = (char)(init + col);
        //    string colRange = colInit.ToString() + (InitRow) + ":" + colInit.ToString() + examRow;
        //    Range rExample = _sheet.Range[colRange];
        //    return rExample;
        //}


        private void btnInitVersion_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //IList<object[]> emlList = new List<object[]>();
            //Dictionary<string ,object >  emldict = new Dictionary<string, object>();
            //emldict["Name"] = _selectedName;
            //emlList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ExampleTemp, emldict);
            //if (emlList.Count == 2)
            //    foreach (var em in emlList)
            //        if (em[1].ToString() == "V2.0")
            //        {
            //            _tem.DrawNav();
            //            _tem.DrawExcel(_selectedName);
            //        }
            btnColSet.Enabled = true;
        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            _dr = null;
            //dpEml.Visibility = DockVisibility.Visible;
            //if(itemList.Count == 0)

            //dpEml.Visibility = DockVisibility.Visible;
            dpEml.Visibility = DockVisibility.Visible;
            //CreateDockPanel();
            _currentOperate = DataOper.Add;
        }

        private void tsmiModify_Click(object sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            if (_dr == null)
                return;
            dpEml.Visibility = DockVisibility.Visible;
            //dpEml.Visibility = DockVisibility.Visible;
            //CreateDockPanel();
            _currentOperate = DataOper.Modify;
            //SetDataToUI();
            SetDataToDockPanel();
        }

        private void SetDataToDockPanel()
        {
            foreach (LayoutControlItem item in itemList)
            {
                if (typeof(SimpleButton) == item.Control.GetType())
                    continue;
                if (item.Control.Name == "TestCaseDescription")
                {
                    if (_dr[item.Control.Name].ToString() == "--")
                    {
                        item.Control.Text = "";
                    }
                    else
                    {
                        item.Control.Text = _dr[item.Control.Name].ToString();
                    }
                }
                else
                {
                    item.Control.Text = _dr[item.Control.Name].ToString();
                }
                
            }
        }
        private void tsmiDel_Click(object sender, EventArgs e)
        {
            if(_dr==null)
                return;
            _dr.Delete();
            string error = "";
            string fjson = SerGridViewToJson(out error);
            SaveText(fjson);
            _dr = null;
        }

        private void gvEml_MouseDown(object sender, MouseEventArgs e)
        {
            // if (e.Button == MouseButtons.Left)
            {
                //获得光标位置
                var hi = gvEml.CalcHitInfo(e.Location);
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
                gvEml.SelectRow(hi.RowHandle);
                selectRow = hi.RowHandle;
                _dr = this.gvEml.GetDataRow(selectRow);
            }
        }

        private void gvEml_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                tsmiDel.Enabled = true;
            }
        }

        private void nbcTempList_MouseHover(object sender, EventArgs e)
        {
            for (int i = 0; i < nbcTempList.Items.Count; i ++)
            {
                nbcTempList.Items[i].Appearance.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                nbcTempList.Items[i].AppearancePressed.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                //nbcTempList.Items[row].LinkClicked += NameItem_Click;
            }

            foreach (NavBarItem item in nbcTempList.Items)
            {
                
            }
        }

        private void tsmiCheck_Click(object sender, EventArgs e)
        {
            if (_dr == null)
            {
                MessageBox.Show("请先选中一行数据");
                return;
            }
            //string itemJson = "";
            
            //string key = _dr["Chapter"].ToString() + "-" + _dr["ExapID"].ToString();
            //foreach (KeyValuePair<string, string> item in GlobalVar.DictAssessItem)
            //{
            //    if (item.Key == key)
            //    {
            //        itemJson = item.Value;
            //        break;
            //    }
            //}
            AssessItemRelevant ad = new AssessItemRelevant(_dr["AssessItemRelevant"].ToString());
            ad.ShowDialog();

            if (ad.DialogResult == DialogResult.OK)
            {
                _dt.Rows[selectRow]["AssessItemRelevant"] = GlobalVar.DictAssessItem;
                gcEmlTemplate.DataSource = _dt;
                DerGridToJson();
            }
        }

        private void gcEmlTemplate_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //获得光标位置
            var hi = gvEml.CalcHitInfo(e.Location);
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
            gvEml.SelectRow(hi.RowHandle);
            selectRow = hi.RowHandle;
            _dr = this.gvEml.GetDataRow(selectRow);

            Update();
        }
    }
}
