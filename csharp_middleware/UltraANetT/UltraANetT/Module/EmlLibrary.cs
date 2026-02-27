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
using System.Drawing;
using DevExpress.LookAndFeel;
using DevExpress.Utils;
using DevExpress.XtraNavBar;
using DevExpress.XtraLayout;
using UltraANetT.Form;

namespace UltraANetT.Module
{
    public partial class EmlLibrary : XtraUserControl,IDraw
    {
        #region 设置变量
        
        private readonly ProcShow _show = new ProcShow();

        private readonly ProcStore _store = new ProcStore();

        private readonly IDraw _draw;

        private readonly ProcLog _log = new ProcLog();
        
        private string role;
        private LogicalControl _LogC = new LogicalControl();

        #region 用例库相关
        private DataTable _dt;
        private DataRow _dr;
        /// <summary>
        /// 当前操作类别
        /// </summary>
        private DataOper _currentOperate;
        private NavBarItem _oldBarItem;
        private string _selectedName;
        private Dictionary<string, Dictionary<string, string>> _dictdictEmllist;
        private Dictionary<string, List<object>> _dictListDrScheme;
        private List<LayoutControlItem> _itemList = new List<LayoutControlItem>();
        private int _rowIndex=0;
        private Dictionary<string, List<string>> _dictListChapter;//用例章节
        #endregion
        #endregion

        public EmlLibrary()
        {
            InitializeComponent();
            _draw = this;
            _dt=new DataTable();
            DrawNav();
            role = _LogC.RoleSelect(GlobalVar.UserName);
            RoleFunction(role);
            hideContainerRight.Visible = false;
            _dictListChapter = GetChapterInfo();
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

        private Dictionary<string, List<string>> GetChapterInfo()
        {
            Dictionary<string, List<string>> dictListChapter = new Dictionary<string, List<string>>();
            var chapterList = _store.GetRegularByEnum(EnumLibrary.EnumTable.ExapChapter);
            foreach (var objArray in chapterList)
            {
                try
                {
                    if(!dictListChapter[objArray[1].ToString()].Contains(objArray[0].ToString()))
                        dictListChapter[objArray[1].ToString()].Add(objArray[0].ToString());
                }
                catch (Exception e)
                {
                    dictListChapter[objArray[1].ToString()] = new List<string>();
                    if (!dictListChapter[objArray[1].ToString()].Contains(objArray[0].ToString()))
                        dictListChapter[objArray[1].ToString()].Add(objArray[0].ToString());
                }
            }
            return dictListChapter;
        }

        private void DrawNav()
        {
            _dictdictEmllist = new Dictionary<string, Dictionary<string, string>>();
            var exampleList = _store.GetRegularByEnum(EnumLibrary.EnumTable.ExampleTemp);
            for (int row = 0; row < exampleList.Count; row++)
            {
                Dictionary<string, string> dictEmlList = new Dictionary<string, string>();
                dictEmlList["Version"] = exampleList[row][1].ToString();
                dictEmlList["Content"] = exampleList[row][2].ToString();
                dictEmlList["MatchSort"] = exampleList[row][3].ToString();
                dictEmlList["ImportDate"] = exampleList[row][4].ToString();
                dictEmlList["EmlTemplate"] = exampleList[row][5].ToString();
                _dictdictEmllist[exampleList[row][0].ToString()] = dictEmlList;
                nbgEml.AddItem();
                nbcEmlNameList.Items[row].Caption = exampleList[row][0].ToString();
                nbcEmlNameList.Items[row].Appearance.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                nbcEmlNameList.Items[row].AppearancePressed.Font = new Font("微软雅黑", 9, FontStyle.Regular);
                nbcEmlNameList.Items[row].Appearance.ForeColor = Color.Black;
                nbcEmlNameList.Items[row].LinkClicked += NameItem_Click;
            }
        }

        #region 名称点击事件
        private void NameItem_Click(object sender, EventArgs e)
        {
            _dr = null;
            //判断是否依赖节点
            var item = sender as NavBarItem;
            if (item != null)
            {
                _selectedName = item.Caption;
            }
            if (_oldBarItem != null && _oldBarItem != item)
            {
                _oldBarItem.Appearance.ForeColor = Color.Black;
            }
            if (item != null)
            {
                item.Appearance.ForeColor = Color.Blue;
                _oldBarItem = item;
            }
            DerDictToGridView(_dictdictEmllist[_selectedName]["EmlTemplate"], _dictdictEmllist[_selectedName]["Content"]);
            CreateDockPanel(_dictListDrScheme);
        }
        private void DerDictToGridView(string eml, string colEml)
        {
            _dictListDrScheme = Json.DeserJsonDList(colEml);
            if (_dictListDrScheme == null)
                return;
            _dt.Clear();
            CreateGridView(_dictListDrScheme);
            InitDataTable();
            _dt = DerJsonToDrs(eml);
            gcEmlLibrary.DataSource = _dt;
        }
        /// <summary>
        /// 把Json转换成DataRow,填充进已经搭好框架的DataTable里去
        /// </summary>
        /// <returns></returns>
        private DataTable DerJsonToDrs(string eml)
        {
            var dictChapter = Json.DeserJsonToDDict(eml) == null
                ? new Dictionary<string, Dictionary<string, List<object>>>()
                : Json.DeserJsonToDDict(eml);
            int colCount = gvEmlLibrary.Columns.Count;
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
                    _dt.Rows.Add(obj);
                }
            }
            return _dt;
        }
        /// <summary>
        /// 初始化gvEml
        /// </summary>
        private void CreateGridView(Dictionary<string, List<object>> listDrScheme)
        {
            gvEmlLibrary.Columns.Clear();
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
                        gvEmlLibrary.Columns.AddRange(new GridColumn[] { col });
                    }
                    else
                    {
                        GridColumn col = new GridColumn();
                        col.Caption = scheme.Value[1].ToString();
                        col.Name = scheme.Key;
                        col.FieldName = scheme.Key;
                        col.Visible = true;
                        gvEmlLibrary.Columns.AddRange(new GridColumn[] { col });
                    }
                }
            }
            //gvEml.Columns[gvEml.Columns.Count - 1].Visible = false;
            GridColumn colNew = new GridColumn();
            colNew.Caption = "评价项目相关";
            colNew.Name = "CheckItem";
            colNew.FieldName = "CheckItem";
            colNew.Visible = true;
            gvEmlLibrary.Columns.AddRange(new GridColumn[] { colNew });
        }
        /// <summary>
        /// 初始化gridview的列名
        /// </summary>
        private void InitDataTable()
        {
            _dt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvEmlLibrary.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList.ToArray())
                _dt.Columns.Add(new DataColumn(colName, typeof(object)));
            gcEmlLibrary.DataSource = _dt;
        }

        #region 动态控件

        private void CreateDockPanel(Dictionary<string, List<object>> colDictionary)
        {
            int i = 0;
            lcgDpEmlLibrary.Items.Clear();
            lcDpEmlLibrary.Controls.Clear();
            _itemList.Clear();
            foreach (KeyValuePair<string, List<object>> scheme in colDictionary)
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

            CreateButton();
            ContextMenu emptyMenu = new ContextMenu();
            foreach (LayoutControlItem item in _itemList)
            {
                lcgDpEmlLibrary.AddItem(item);
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
            lci.Location = new System.Drawing.Point(0, i * 28);
            boxEdit.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;

            if (scheme.Key == "Chapter")
            {
                var selectName = _selectedName.Replace("用例表", string.Empty).Trim();
                if(_dictListChapter.ContainsKey(selectName))
                    boxEdit.Properties.Items.AddRange(_dictListChapter[selectName]);
            }
            else
            {
                var boxStrings = scheme.Value[6].ToString().Split(',');
                object[] boxitem = new object[boxStrings.Length];
                boxStrings.CopyTo(boxitem, 0);
                boxEdit.Properties.Items.AddRange(boxStrings);
            }
            //dpEmlLcg.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {lci});
            _itemList.Add(lci);
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
            lci.Location = new System.Drawing.Point(0, i * 28);
            //this.dpEmlLcg.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {lci});
            _itemList.Add(lci);
        }


        private void CreateButton()
        {
            int sizeHeight = 28;
            int colCount = CountColumns();
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
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            btnSub.Appearance.Options.UseFont = true;

            btnSub.Location = new System.Drawing.Point(0, btnLocation);
            btnSub.Margin = new System.Windows.Forms.Padding(3, 3, 5, 5);
            btnSub.Name = "btnSub";
            btnSub.Size = new System.Drawing.Size(266, sizeHeight);
            btnSub.MinimumSize = new Size(0, 28);
            btnSub.Text = @"确定";
            //dpEmlLcg.Items.AddRange(btnSub);
            //layoutControl3.Controls.Add(btnSub);
            btnSub.MouseClick += btnSub_Click;
            _itemList.Add(simpleButtonLCI);
        }

        private void btnSub_Click(object sender, EventArgs e)
        {
            if (!PrimaryKey())
                return;
            foreach (LayoutControlItem item in _itemList)
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
                foreach (KeyValuePair<string, List<object>> scheme in _dictListDrScheme)
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
            _draw.Submit();
        }
        
        #endregion
        private int CountColumns()
        {
            int colCount = 0;
            foreach (KeyValuePair<string, List<object>> scheme in _dictListDrScheme)
            {
                if (bool.Parse(scheme.Value[2].ToString()))
                {
                    colCount++;
                }
            }
            return colCount;
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
                    if (_rowIndex == j)
                        continue;
                }
                bool boolChapter = false;
                bool boolExapID = false;
                primary = false;
                foreach (var lcItem in _itemList)
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
        private object[] GetDataFromDockPanel()
        {
            object[] obj = new object[gvEmlLibrary.Columns.Count];
            int i = 0;
            foreach (LayoutControlItem layoutItem in _itemList)
            {
                if (typeof(SimpleButton) == layoutItem.Control.GetType())
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
            return obj;
        }
        private DataTable UpdateGridViewByOperate()
        {
            DataTable newDt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvEmlLibrary.Columns)
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
            return newDt;
        }
        private string SerGridViewToJson()
        {
            var currentChapter = "";
            var currentAssItem = "";
            var dictChapter = new Dictionary<string, Dictionary<string, List<object>>>();
            var dictAssItem = new Dictionary<string, List<object>>();
            var listChapter = new List<Dictionary<string, string>>();
            var listExample = new List<object>();
            List<Dictionary<string, string>> listItem = new List<Dictionary<string, string>>();
            var coList = new List<string>();
            foreach (GridColumn col in gvEmlLibrary.Columns)
                coList.Add(col.FieldName);
            foreach (DataRow row in _dt.Rows)
            {
                int i = 0;
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
            }
            if (listItem.Count != 0)
            {
                listExample.Add(listItem);
                dictAssItem.Add(currentAssItem, listExample);
                dictChapter.Add(currentChapter, dictAssItem);
            }
            return Json.SerJson(dictChapter);
        }
        private static Dictionary<string, string> InitDictProperty(DataTable dt)
        {
            Dictionary<string, string> dictProperty = new Dictionary<string, string>();
            foreach (DataColumn col in dt.Columns)
            {
                if (col.ToString() == "Chapter")
                    continue;
                if (col.ToString() == "AssessItemRelevant")
                {
                    break;
                }
                dictProperty.Add(col.ToString(), "");
            }
            return dictProperty;

        }
        private static Dictionary<string, string> InitDictItem()
        {
            Dictionary<string, string> dictItem = new Dictionary<string, string>();
            dictItem.Add("AssessItem", "");
            dictItem.Add("MinValue", "");
            dictItem.Add("NormalValue", "");
            dictItem.Add("MaxValue", "");
            dictItem.Add("ValueDescription", "");
            return dictItem;
        }
        private void UpDataRow()
        {
            int i = 0;
            foreach (LayoutControlItem lItem in _itemList)
            {
                if (typeof(SimpleButton) == lItem.Control.GetType())
                    continue;
                _dt.Rows[_rowIndex][i] = lItem.Control.Text;
                i++;
            }
        }
        private void ClearUI()
        {
            foreach (LayoutControlItem item in _itemList)
            {
                if (typeof(SimpleButton) == item.Control.GetType())
                    continue;
                item.Control.Text = "";
            }
        }
        void IDraw.Submit()
        {
            string strDtJosn = "";
            string strOldDtJson = SerGridViewToJson();
            Dictionary<string, object> dictEml = new Dictionary<string, object>();
            dictEml["Name"] = _selectedName;
            dictEml["Version"] = _dictdictEmllist[_selectedName]["Version"];
            switch (_currentOperate)
            {
                //添加
                case DataOper.Add:
                    _dr = null;
                    object[] row = GetDataFromDockPanel();
                    row[gvEmlLibrary.Columns.Count - 2] = "";
                    row[gvEmlLibrary.Columns.Count - 1] = "请右击查看评价项目信息";
                    _dt.Rows.Add(row);
                    _dt = UpdateGridViewByOperate();
                    gcEmlLibrary.DataSource = _dt;
                    dpEmlLibrary.Visibility = DockVisibility.Visible;
                    break;
                //更新
                case DataOper.Modify:
                    UpDataRow();
                    _dt = UpdateGridViewByOperate();
                    gcEmlLibrary.DataSource = _dt;
                    ClearUI();
                    dpEmlLibrary.Visibility = DockVisibility.Hidden;
                    break;
                case DataOper.Del:
                    if (_dr == null) return;
                    _dr.Delete();
                    _dr = null;
                    gcEmlLibrary.DataSource = _dt;
                    break;
            }
            _draw.SwitchCtl(false);
            ClearUI();
            strDtJosn = SerGridViewToJson();//整个表格变为json
            dictEml["EmlTemplate"] = strDtJosn;
            string error = "";
            _store.Update(EnumLibrary.EnumTable.ExampleTempTemplate, dictEml, out error);
            _dictdictEmllist[dictEml["Name"].ToString()]["EmlTemplate"] = dictEml["EmlTemplate"].ToString();
            if (error == "")
            {
                switch (_currentOperate)
                {
                    case DataOper.Add:
                        XtraMessageBox.Show("添加成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case DataOper.Modify:
                        XtraMessageBox.Show("修改成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case DataOper.Del:
                        break;
                }
            }
            else
            {
                switch (_currentOperate)
                {
                    case DataOper.Add:
                        XtraMessageBox.Show("未知错误，添加失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case DataOper.Modify:
                        XtraMessageBox.Show("未知错误，修改失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case DataOper.Del:
                        XtraMessageBox.Show("未知错误，修改失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                }
                _dt = DerJsonToDrs(strOldDtJson);
                gcEmlLibrary.DataSource = _dt;
            }
        }

        private void Update()
        {
            if (_dr == null)
                return;
            _currentOperate = DataOper.Modify;
            SetDataToDockPanel();
            _draw.SwitchCtl(true);
        }

        private void SetDataToDockPanel()
        {
            foreach (LayoutControlItem item in _itemList)
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
        private void tsmiCheck_Click(object sender, EventArgs e)
        {
            if (_dr == null)
            {
                MessageBox.Show("请先选中一行数据");
                return;
            }
            EmlAssessItemRelevant eair = new EmlAssessItemRelevant(this,_dr["AssessItemRelevant"].ToString());
            eair.ShowDialog();
        }

        public bool SaveJsonToDB(string strJsonEAIR)
        {
            string strOldDtJson = SerGridViewToJson();
            Dictionary<string, object> dictEml = new Dictionary<string, object>();
            dictEml["Name"] = _selectedName;
            dictEml["Version"] = _dictdictEmllist[_selectedName]["Version"];
            _dr["AssessItemRelevant"] = strJsonEAIR;
            gcEmlLibrary.DataSource = _dt;
            var strDtJosn = SerGridViewToJson();//整个表格变为json
            dictEml["EmlTemplate"] = strDtJosn;
            string error = "";
            _store.Update(EnumLibrary.EnumTable.ExampleTempTemplate, dictEml, out error);
            if (error == "")
            {
                return true;
            }
            else
            {
                _dt = DerJsonToDrs(strOldDtJson);
                gcEmlLibrary.DataSource = _dt;
                return false;
            }
        }
        #endregion

        private void gcEmlLibrary_DoubleClick(object sender, EventArgs e)
        {
            cmsEnabled(false);
            //获得所点击的控件
            var control = sender as System.Windows.Forms.Control;
            if (control == null) return;
            //获得光标位置
            var hi = gvEmlLibrary.CalcHitInfo(control.PointToClient(MousePosition));
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
            //取值一行值
            _rowIndex = hi.RowHandle;
            _dr = gvEmlLibrary.GetDataRow(hi.RowHandle);
            cmsEnabled(true);
            //双击某一行也可以修改
            Update();
        }

        
        private void gvSegment_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                tsmiDel.Enabled = true;
                //_deptName = gvSegment.GetRowCellValue(e.RowHandle, gvSegment.Columns["SegmentName"]).ToString();
            }
        }

        void IDraw.InitGrid()
        {
            throw new NotImplementedException();
        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            ClearUI();
            _currentOperate = DataOper.Add;
            _draw.SwitchCtl(true);
        }

        private void tsmiModify_Click(object sender, EventArgs e)
        {
            Update();
        }

        private void tsmiDel_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Del;
            _draw.Submit();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Dictionary<string, object> IDraw.GetDataFromUI()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        void IDraw.InitDict()
        {
            throw new NotImplementedException();
        }

        void IDraw.SwitchCtl(bool isSwitch)
        {
            switch (isSwitch)
            {
                case true:
                    dpEmlLibrary.Visibility = DockVisibility.Visible;
                    break;
                case false:
                    dpEmlLibrary.Visibility = DockVisibility.Hidden;
                    break;
            }
        }

        private void cmsEnabled(bool isSwitch)
        {
            switch (isSwitch)
            {
                case true:
                    tsmiModify.Enabled = true;
                    tsmiDel.Enabled = true;
                    tsmiCheck.Enabled = true;
                    break;
                case false:
                    tsmiModify.Enabled = false;
                    tsmiDel.Enabled = false;
                    tsmiCheck.Enabled = false;
                    break;
            }
        }

        private void gvEmlLibrary_MouseDown(object sender, MouseEventArgs e)
        {
            cmsEnabled(false);
            if (e.Button == MouseButtons.Right)
            {
                //获得光标位置
                var hi = gvEmlLibrary.CalcHitInfo(e.Location);
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
                gvEmlLibrary.SelectRow(hi.RowHandle);
                _rowIndex = hi.RowHandle;
                _dr = gvEmlLibrary.GetDataRow(hi.RowHandle);
                cmsEnabled(true);
            }
        }
    }
}
