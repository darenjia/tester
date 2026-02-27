using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;
using DevExpress.DataAccess.Native.Sql.QueryBuilder;
using DevExpress.XtraEditors;
using FileEditor.pubClass;
using ProcessEngine;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using FileEditor.Control;

namespace FileEditor.form
{
    public partial class EmlColEditor : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private Dictionary<string,object> dict = new Dictionary<string, object>(); 
        private List<object> _list = new List<object>();
        DataTable _dt = new DataTable();
        private DataRow _dr;
        string path;
        private int selectRow;
        private bool _isOnlyEml;
        private readonly ProcStore _store = new ProcStore() ;
        private string fjson;
        bool emlVersion = false;
        private readonly ProcFile _file = new ProcFile();
        //private int colTrue;
        public EmlColEditor(bool onlyEml)
        {
            InitializeComponent();
            IniteGrid();
            _isOnlyEml = onlyEml;
            if(GlobalVar.IsIndependent)
                btnSub.Text = "提交新用例表模板";
            else
            {
                btnSub.Text = "关联到当前车型";
            }
            InitVersion();
            DrawGridView();
        }

        //private void EmlColEditor_Load(object sender, EventArgs e)
        //{
            
        //    cbeVersion.Properties.Items.Clear();
        //    lciVersion.Text = GlobalVar.SelectName + "版本选择";
        //    var dictTem = new Dictionary<string, object>();
        //    dictTem.Add("Name", GlobalVar.SelectName);
        //    IList<object[]> emlTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ExampleTemp, dictTem);
        //    foreach (object[] item in emlTem)
        //    {
        //        cbeVersion.Properties.Items.Add(item[1].ToString());
        //    }
        //    DrawGridView();

        //}
        private void InitVersion()
        {
            cbeType.Properties.Items.Clear();
            cbeType.ReadOnly = false;
            cbeVersion.ReadOnly = false;
            List<string> emlTem = new List<string>
            {
                "CAN单节点用例表",
                "CAN集成用例表",
                "LIN单节点用例表",
                "LIN集成用例表",
                "J1939单节点用例表",
                "J1939集成用例表",
                "OSEK单节点用例表",
                "OSEK集成用例表",
                "总线相关DTC用例表",
                //"诊断协议用例表"
            };
            foreach (string item in emlTem)
            {
                cbeType.Properties.Items.Add(item);
            }
            if (!GlobalVar.IsIndependent)
            {
                cbeVersion.Properties.Items.Clear();
                cbeType.Text = GlobalVar.SelectName;
                cbeType.Enabled = false;
                cbeVersion.Text = GlobalVar.VersionEmlTem;
                cbeVersion.Enabled = false;
            }
            else
            {
                if (GlobalVar.SelectName != "")
                {
                    cbeVersion.Properties.Items.Clear();
                    cbeType.Text = GlobalVar.SelectName;
                    cbeType.ReadOnly = true;
                    //cbeType.Enabled = false;
                    cbeVersion.Text = GlobalVar.VersionCfgTem;
                    cbeVersion.Properties.Items.Clear();
                    cbeVersion.Enabled = true;
                    var dictTem = new Dictionary<string, object>();
                    dictTem.Add("Name", cbeType.Text);
                    IList<object[]> conTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ExampleTemp, dictTem);
                    foreach (object[] item in conTem)
                    {
                        cbeVersion.Properties.Items.Add(item[1].ToString());
                    }
                    cbeVersion.ReadOnly = true;
                }
                else
                {
                    cbeType.ReadOnly = false;
                    cbeVersion.ReadOnly = false;
                }

            }
        }
        public void IniteGrid()
        {
            
            var coList = new List<string>();
            foreach (GridColumn col in gvEml.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList)
                _dt.Columns.Add(new DataColumn(colName, typeof(object)));
            gcEml.DataSource = _dt;


        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            if (cbeType.Text == "")
            {
                XtraMessageBox.Show("请先选择用例表类型再操作...");
                return;
            }
            object drNum;
                //直接在表后面添加
                drNum = gvEml.RowCount + 1;
                AddEditor ad = new AddEditor(drNum);
                ad.ShowDialog();
                _list = ad.Dict();
                if(ad.Flag)
                    AddTable(_list);
                
                //EmlTemplate at = new EmlTemplate();
           
            
        }

        public void AddTable(List< object> list)
        {
            //_list = ad.Dict();
            object[] newRow = { list[0], list[1], list[2], list[3], list[4], list[5], list[6], list[7], list[8], list[9], list[10], list[11], list[12] };
            _dt = (DataTable)gcEml.DataSource;
            _dt.Rows.Add(newRow);
            gcEml.DataSource = _dt;
            fjson = GridToDict(_dt);
            SaveText(fjson);
            //GlobalVar.IsColumEdit = true;
            //GlobalVar.EmlCache.SetExcell(_list);

        }
        private DataTable CreateTable()
        {
            DataTable dt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvEml.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList)
                dt.Columns.Add(new DataColumn(colName, typeof(object)));
            return dt;
        }

        
        

        private Dictionary<string, object> GetDict()
        {
            dict.Clear();
            dict.Add("Sequence", _dr["Sequence"]);
            dict.Add("ChiName",_dr["ChiName"]);
            dict.Add("EngName", _dr["EngName"]);
            dict.Add("UsingState", _dr["UsingState"]);

            dict.Add("Unit", _dr["Unit"]);
            dict.Add("IsHexadecimal", _dr["IsHexadecimal"]);
            dict.Add("IsStringLimit", _dr["IsStringLimit"]);
            dict.Add("StringRange", _dr["StringRange"]);
            dict.Add("IsMaxMin", _dr["IsMaxMin"]);
            dict.Add("MinValue", _dr["MinValue"]);
            dict.Add("MaxValue", _dr["MaxValue"]);
            dict.Add("IsSpecialFormat", _dr["IsSpecialFormat"]);
            dict.Add("FormatName", _dr["FormatName"]);
            return dict;
        }

        private void tsmiModify_Click(object sender, EventArgs e)
        {
            Update();
        }


        private void Update()
        {
            if (cbeType.Text == "")
            {
                XtraMessageBox.Show("请先选择用例表类型再操作...");
                return;
            }
            if (_dr == null)
                return;
            Dictionary<string, object> dicdr = GetDict();

            GlobalVar.ChangList.Add(_dr["UsingState"]);
            AddEditor ad = new AddEditor(dicdr);
            ad.ShowDialog();
            _list = ad.Dict();
            if (ad.Flag)
            {
                object[] upRow = { _list[0], _list[1], _list[2], _list[3], _list[4], _list[5], _list[6], _list[7], _list[8], _list[9], _list[10], _list[11], _list[12] };
                _dt = (DataTable)gcEml.DataSource;
                UpdateRow(upRow);
                gcEml.DataSource = _dt;
                fjson = GridToDict(_dt);
                SaveText(fjson);
                //GlobalVar.IsColumEdit = true;
                //if (dicdr["UsingState"].ToString() == "True"&&_list[3].ToString()=="False")
                //{
                //    int colTrue = GetColNum(int.Parse(_list[0].ToString()) , _dt);
                //    _list[0] = colTrue;
                //    GlobalVar.EmlCache.SetExcell(_list);
                //}
                //if (dicdr["UsingState"].ToString() == "False" && _list[3].ToString() == "True")
                //{
                //    int colTrue = GetColNum(int.Parse(_list[0].ToString()) , _dt);
                //    _list[0] = colTrue;
                //    GlobalVar.EmlCache.SetExcell(_list);
                //}
                //else if (dicdr["UsingState"].ToString() == "True" && _list[3].ToString() == "True")
                //{
                //    int colTrue = GetColNum(int.Parse(_list[0].ToString()), _dt);
                //        _list[0] = colTrue;
                //        GlobalVar.EmlCache.SetExcell(_list);

                //}

            }
        }


        private void SetGlobalList(List<object> list)
        {
            GlobalVar.ChangList.Add(list[3]);

        }

        private void UpdateRow(object[] obj)
        {
            int j = int.Parse(obj[0].ToString()) - 1;
            //int j = obj[0].ToString() - 1;
            _dt.Rows[j][0] = obj[0];
            _dt.Rows[j][1] = obj[1];
            _dt.Rows[j][2] = obj[2];
            _dt.Rows[j][3] = obj[3];
            _dt.Rows[j][4] = obj[4];
            _dt.Rows[j][5] = obj[5];
            _dt.Rows[j][6] = obj[6];
            _dt.Rows[j][7] = obj[7];
            _dt.Rows[j][8] = obj[8];
            _dt.Rows[j][9] = obj[9];
            _dt.Rows[j][10] = obj[10];
            _dt.Rows[j][11] = obj[11];
            _dt.Rows[j][12] = obj[12];
        }
        private void tsmiInsert_Click(object sender, EventArgs e)
        {
            if (cbeType.Text == "")
            {
                XtraMessageBox.Show("请先选择用例表类型再操作...");
                return;
            }
            object drNum;
            //在已有行中插入一行
            if (_dr != null)
            {
                
                AddEditor ad = new AddEditor(_dr);
                ad.ShowDialog();
                _list = ad.Dict();
                if (ad.Flag)
                    InsertRow(_list);

            }
        }

        public void InsertRow(List<object> _list)
        {
            object[] newRow = { _list[0], _list[1], _list[2], _list[3], _list[4], _list[5], _list[6], _list[7], _list[8], _list[9], _list[10], _list[11], _list[12] };
            _dt = (DataTable)gcEml.DataSource;
            DataTable dt = CreateTable();

            int j = int.Parse(_dr["Sequence"].ToString());

            for (int i = 0; i < j; i++)
            {
                dt.Rows.Add(_dt.Rows[i].ItemArray);
            }
            dt.Rows.Add(newRow);
            int con = gvEml.RowCount - int.Parse(_dr["Sequence"].ToString());
            for (int i = 0; i < con; i++)
            {
                gvEml.SetRowCellValue(j + i, "Sequence", j + i + 2);
                dt.Rows.Add(_dt.Rows[j + i].ItemArray);
            }
            //_dt = dt;
            gcEml.DataSource = dt;
            fjson = GridToDict(dt);
            SaveText(fjson);
            //GlobalVar.IsColumEdit = true;
            //int colTrue = GetColNum(int.Parse(_list[0].ToString()), dt);
            //_list[0] = colTrue;
            //GlobalVar.EmlCache.SetExcell(_list);

        }
        private void tsmiDel_Click(object sender, EventArgs e)
        {

        }

        private void btnRefresh_Click_1(object sender, EventArgs e)
        {

        }

        private void gvEml_MouseDown(object sender, MouseEventArgs e)
        {
           // if (e.Button == MouseButtons.Left)
            {
                //获得光标位置
                var hi = gvEml.CalcHitInfo(e.Location);
                //判断位置是否在行位置上
                if (!hi.InRow && !hi.InRowCell) return;
                if (hi.RowHandle < 0) return;
                //取一行值
                gvEml.SelectRow(hi.RowHandle);
                selectRow = hi.RowHandle;
                _dr = this.gvEml.GetDataRow(selectRow);
            }
        }

        private int GetColNum(int col, DataTable dtable)
        {
            int i = 0;
            foreach (DataRow row in dtable.Rows)
            {
                
                if(int.Parse(row[0].ToString())>=col)
                    break;
                if (bool.Parse(row[3].ToString()))
                    i++;
            }
            return i;
        }

        private string  GridToDict(DataTable  dtable)
        {
            string gridJson;
            Dictionary<string, List<object>> dictTable = new Dictionary<string, List<object>>();
            foreach (DataRow row in dtable.Rows)
            {
                List<object> listCache = new List<object>()
                {
                    row[0],row[1],row[3],row[4],row[5],row[6],row[7],row[8],row[9],row[10],row[11],row[12]
                };
                dictTable.Add(row[2].ToString(),listCache);
            }
            gridJson = Json.SerJson(dictTable);

            return gridJson;
        }
        
        private void SaveText(string fileJson)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            //写入
            sw.Write(fileJson);
            //清空
            sw.Flush();
            //关闭
            sw.Close();
            fs.Close();
        }

        private void btnSub_Click(object sender, EventArgs e)
        {
            if (!GlobalVar.IsIndependent)
            {
                bool succes =  SaveJsonToData(GlobalVar.VNode);
                if (!succes)
                    return;
                if(File.Exists(path))
                    File.Delete(path);
                //File.Delete(GlobalVar.TemporaryFilePath + "example\\" + GlobalVar.VNode[0] + "-" + GlobalVar.VNode[1] + "-" + GlobalVar.VNode[2] + "-" + GlobalVar.SelectName + ".txt");
                GlobalVar.IsColumEdit = true;

            }
            else
            {
                string fjson = GridToDict((DataTable) gcEml.DataSource);
                var dictTem = new Dictionary<string, object>();
                dictTem.Add("Name", cbeType.Text);
                dictTem.Add("Version",cbeVersion.Text);
                dictTem.Add("Content",fjson);
                string match = _file.SearchEmlTemName(cbeType.Text.Trim());
                dictTem.Add("MatchSort", match);
                dictTem.Add("ImportDate", DateTime.Now);
                IList<object[]> emlTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ExampleTemp, dictTem);
                IList<object[]> emlTemv = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ExampleTemp_Ver, dictTem);
                if (emlTemv.Count == 2 || emlTemv.Count == 1)
                {
                    string error = "";
                    if (_store.Update(EnumLibrary.EnumTable.ExampleTemp, dictTem, out error))
                    {
                        XtraMessageBox.Show("上传数据库成功！", "提示", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning);
                        if (File.Exists(path))
                            File.Delete(path);
                        GlobalVar.IsColumEdit = true;
                    }
                }
                else
                {
                    if (cbeType.Text == "" )
                    {
                        XtraMessageBox.Show("请选择用例表类型", "提示", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning);
                        return;
                    }
                        
                    int i = emlTem.Count + 1;
                    string ver = "V" + i.ToString() + ".0";
                    dictTem["Version"] = ver;
                    string errorQ = "";
                    _store.AddExampleTemp(dictTem, out errorQ);
                    if (errorQ == "")
                    {
                        XtraMessageBox.Show("上传数据库成功！", "提示", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning);
                        //File.Delete(path);
                        GlobalVar.IsColumEdit = true;
                        if (File.Exists(path))
                            File.Delete(path);
                    }
                }
                
            }

        }

        private void DrawGridView()
        {
            if (!GlobalVar.IsIndependent)
            {
                string emlName = cbeType.Text + "列编辑";
                path = GlobalVar.TemporaryFilePath +"example\\"+ GlobalVar.VNode[0] + "-" + GlobalVar.VNode[1] + "-" +
                       GlobalVar.VNode[2] + "-" + emlName + ".txt";
                if (File.Exists(path)) //验证临时文件
                {
                    string JsonRead = File.ReadAllText(path);
                    GlobalVar.ListDrScheme = Json.DeserJsonDList(JsonRead);
                    if (GlobalVar.ListDrScheme == null)
                        gcEml.DataSource = null;
                    //DataTable dt = CreateTable();
                    DerDictToDataTable(GlobalVar.ListDrScheme);
                }
                else
                {
                    string match = _file.SearchBusByEmlName(GlobalVar.SelectName);
                   
                    List<string> listM = new List<string>
                    {
                        GlobalVar.VNode[0],
                        GlobalVar.VNode[1],
                        GlobalVar.VNode[2],
                        match
                    };
                    IList<object[]> file = ReadDataByVehicel(listM);
                    if (file.Count != 0&&file[0][13].ToString() != "")
                    {
                        GlobalVar.ListDrScheme = Json.DeserJsonDList(file[0][13].ToString());
                        DerDictToDataTable(GlobalVar.ListDrScheme);
                    }
                    else
                    {
                        var dictTem = new Dictionary<string, object>();
                        dictTem.Add("Name", GlobalVar.SelectName);
                        IList<object[]> emlTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ExampleTemp, dictTem);
                        if (emlTem.Count != 0 && emlTem[0][1].ToString() == "V1.0")
                        {
                            GlobalVar.ListDrScheme = Json.DeserJsonDList(emlTem[0][2].ToString());
                            DerDictToDataTable(GlobalVar.ListDrScheme);
                        }
                    }
                }
            }
            else
            {
                if (GlobalVar.SelectName != "")
                {
                    string emlName = GlobalVar.SelectName + "列编辑";
                    path = GlobalVar.TemporaryFilePath + "example\\" + emlName + ".txt";
                    if (File.Exists(path)) //验证临时文件
                    {
                        string JsonRead = File.ReadAllText(path);
                        GlobalVar.ListDrScheme = Json.DeserJsonDList(JsonRead);
                        if (GlobalVar.ListDrScheme == null)
                            gcEml.DataSource = null;
                        //DataTable dt = CreateTable();
                        DerDictToDataTable(GlobalVar.ListDrScheme);
                    }
                    else
                    {
                        //var dictTem = new Dictionary<string, object>();
                        //dictTem.Add("Name", GlobalVar.SelectName);
                        //IList<object[]> emlTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ExampleTemp, dictTem);
                        //if (emlTem.Count != 0 && emlTem[0][1].ToString() == "V1.0")
                        {
                            //GlobalVar.ListDrScheme = Json.DeserJsonDList(emlTem[0][2].ToString());
                            DerDictToDataTable(GlobalVar.ListDrScheme);
                        }

                    }
                }
                else
                {
                    
                }
            }

        }

        private void DerDictToDataTable(Dictionary<string, List<object>> listDictionary)
        {
            foreach (KeyValuePair<string, List<object>> scheme in listDictionary)
            {
                object[] obj = new object[13];
                obj[0] = scheme.Value[0]; //顺序
                obj[1] = scheme.Value[1]; //中文列名
                obj[2] = scheme.Key; //英文列名
                obj[3] = scheme.Value[2]; //启用状态

                obj[4] = scheme.Value[3]; //单位
                obj[5] = scheme.Value[4]; //是否启用16进制
                obj[6] = scheme.Value[5]; //是否有字符串限制
                obj[7] = scheme.Value[6]; //字符串范围

                obj[8] = scheme.Value[7]; //是否有最大最小值
                obj[9] = scheme.Value[8]; //最小值
                obj[10] = scheme.Value[9]; //最大值
                obj[11] = scheme.Value[10]; //是否有指定的正则化格式
                obj[12] = scheme.Value[11]; //正则化格式
                _dt.Rows.Add(obj);
            }
            gcEml.DataSource = _dt;
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
                dictCNode.Add("MatchSort", node[3]);
                listFile = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicel, dictCNode);

            }
            return listFile;
        }
        private bool  SaveJsonToData(List<string> node)
        {
            bool isCon = false;
            string fjson = GridToDict((DataTable)gcEml.DataSource);
            if (fjson == "")
            {
                XtraMessageBox.Show("此表格为空，不能上传！", "提示", MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

            }
            else
            {


                IList<object[]> listFile = new List<object[]>();
                if (node.Count != 0)
                {
                    string match = _file.SearchBusByEmlName(GlobalVar.SelectName);
                    
                    List<string> listM = new List<string>
                    {
                        GlobalVar.VNode[0],
                        GlobalVar.VNode[1],
                        GlobalVar.VNode[2],
                        match
                    };
                    Dictionary<string, object> dictCNode = new Dictionary<string, object>();
                    dictCNode.Add("VehicelType", node[0]);
                    dictCNode.Add("VehicelConfig", node[1]);
                    dictCNode.Add("VehicelStage", node[2]);
                    dictCNode.Add("EmlTableColEdit", fjson);
                    dictCNode.Add("EmlTemplate", "");
                    dictCNode.Add("MatchSort", match);
                    dictCNode.Add("EmlTemplateName", GlobalVar.SelectName);
                    listFile = ReadDataByVehicel(listM);
                    string error = "";
                    IList<object[]> listFileEml = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelDouble,
                dictCNode);
                    if (listFile.Count == 0 || listFile[0][7].ToString() == "")
                    {
                        //if (_store.AddFileLinkByVehicel(dictCNode, out error))
                        XtraMessageBox.Show("请先完成配置表提交工作！", "提示", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning);
                        isCon = true;
                        GlobalVar.IsColumEdit = true;
                    }
                    else if (listFileEml.Count == 1)
                    {

                        bool i = _store.Update(EnumLibrary.EnumTable.FileLinkByVehicelEml_Col, dictCNode, out error);
                        if (i)
                        {
                            XtraMessageBox.Show("上传数据库成功！", "提示", MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Warning);
                            isCon = true;
                            GlobalVar.IsColumEdit = true;
                        }
                    }
                    else if(listFile.Count == 1&&listFileEml.Count == 0)
                    {
                        dictCNode["VehicelType"] = listFile[0][0];
                        dictCNode["VehicelConfig"] = listFile[0][1];
                        dictCNode["VehicelStage"] = listFile[0][2];
                        dictCNode["MatchSort"] = listFile[0][3];
                        dictCNode["Topology"] = listFile[0][4];
                        dictCNode["CfgTemplateName"] = listFile[0][5];
                        dictCNode["CfgTemplate"] = listFile[0][6];
                        dictCNode["CfgTemplateJson"] = listFile[0][7];
                        dictCNode["CfgBaudJson"] = listFile[0][8];
                        dictCNode["TplyDescrible"] = listFile[0][11];
                        dictCNode["ConTableColEdit"] = listFile[0][12];
                        dictCNode["EmlTableColEdit"] = fjson;
                        _store.AddFileLinkByVehicel(dict, out error);
                    }

                }
            }
            return isCon;
        }

        private void cbeVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            string Version = cbeVersion.Text;

            if (Version != "")
            {
                //if (!emlVersion)
                //{
                //    emlVersion = true;
                //    return;
                //}
                Dictionary<string, object> _dictName = new Dictionary<string, object>();
                _dictName.Add("Name", cbeType.Text);
                _dictName.Add("Version", Version);
                IList<object[]> emlTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ExampleTemp_Ver, _dictName);
                if (emlTem.Count != 0)
                {
                    _dt = new DataTable();
                    IniteGrid();
                    //string eml = emlTem[0][2].ToString();
                    GlobalVar.ListDrScheme = Json.DeserJsonDList(emlTem[0][2].ToString());
                    DerDictToDataTable(GlobalVar.ListDrScheme);
                }
            }
        }

        private void cbeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbeVersion.Properties.Items.Clear();
            cbeVersion.Enabled = true;
            cbeVersion.SelectedIndex = -1;
            _dt.Rows.Clear();
            var dictTem = new Dictionary<string, object>();
            dictTem.Add("Name", cbeType.Text);
            string emlName = cbeType.Text + "列编辑";
            //GlobalVar.SelectName = cbeType.Text;
            path = GlobalVar.TemporaryFilePath + "example\\" + emlName + ".txt";
            IList<object[]> emlTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ExampleTemp, dictTem);
            foreach (object[] item in emlTem)
            {
                cbeVersion.Properties.Items.Add(item[1].ToString());
            }
        }

        private void gcEml_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //获得光标位置
            var hi = gvEml.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //取一行值
            gvEml.SelectRow(hi.RowHandle);
            selectRow = hi.RowHandle;
            _dr = this.gvEml.GetDataRow(selectRow);

            Update();
        }
    }
}