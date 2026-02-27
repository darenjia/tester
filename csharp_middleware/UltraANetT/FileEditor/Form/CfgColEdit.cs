using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using ProcessEngine;
using System.IO;
using FileEditor.pubClass;
using FileEditor.Control;
using OSEKCLASS;

namespace FileEditor.Form
{
    public partial class CfgColEdit : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        DataTable _dt = new DataTable();
        List<object[]> ObjDataTable = new List<object[]>();
        private bool TxtNull = false;
        private DataRow _dr;
        private int RowCount;
        private int count = 0;
        private ProcStore _store = new ProcStore();
        
        bool cfgVersion = false;
        public CfgColEdit()
        {
            InitializeComponent();
            //lciVersion.Text = GlobalVar.SelectName + "版本选择";
            if (GlobalVar.IsIndependent)
                btnUp.Text = "提交新配置表模板";
            else
            {
                btnUp.Text = "关联到当前车型";
            }
            InitVersion();
            
            JsonToList();
            InitGrid();
        }

        private void InitVersion()
        {
            List<string> emlTem = new List<string>
            {
                "CAN总线配置表",
                "LIN总线配置表",
                "J1939总线配置表",
            };
            foreach (string item in emlTem)
            {
                cbeConfig.Properties.Items.Add(item);
            }
            if (!GlobalVar.IsIndependent)
            {
                cbVersion.Properties.Items.Clear();
                cbeConfig.Text = GlobalVar.SelectName ;
                cbeConfig.Enabled = false;
                cbVersion.Text = GlobalVar.VersionCfgTem;
                cbVersion.Enabled = false;
                //cfgVersion = true;
            }
            else
            {
                if (GlobalVar.SelectName != "")
                {
                    cbVersion.Properties.Items.Clear();
                    cbeConfig.Text = GlobalVar.SelectName;
                    //cbeConfig.Enabled = false;
                    cbVersion.Text = GlobalVar.VersionCfgTem;
                    cbVersion.Properties.Items.Clear();
                    cbVersion.Enabled = true;
                    //cfgVersion = true;
                    var dictTem = new Dictionary<string, object>();
                    dictTem.Add("Name", cbeConfig.Text);
                    IList<object[]> conTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ConfigTemp, dictTem);
                    foreach (object[] item in conTem)
                    {
                        cbVersion.Properties.Items.Add(item[1].ToString());
                    }
                }
                
                
            }
        }


        private void InitGrid()
        {
            _dt = new DataTable();
            _dt.Rows.Clear();
            var coList = new List<string>();
            foreach (GridColumn col in gvExcelCol.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList.ToArray())
                _dt.Columns.Add(new DataColumn(colName, typeof(object)));
            //if (TxtNull)
            {
                foreach (var dept in ObjDataTable)
                    _dt.Rows.Add(dept);
            }
            gcExcelCol.DataSource = _dt;
        }

        private void JsonToList()
        {
            if (!GlobalVar.IsIndependent)
            {
                string JsonRead = null;
                string vehicel = GlobalVar.VNode[0] + "-" + GlobalVar.VNode[1] + "-" + GlobalVar.VNode[2] + "-" + cbeConfig.Text +
                                 "列编辑.txt";
                IList<object[]> file = ReadDataByVehicel(GlobalVar.VNode);

                if (File.Exists(GlobalVar.TemporaryFilePath + "config\\" + vehicel))
                {
                    TxtNull = true;
                    JsonRead = File.ReadAllText(GlobalVar.TemporaryFilePath + "config\\" + vehicel);
                    if (JsonRead != null)
                    {
                        var JsonDict = Json.DeserJsonDList(JsonRead);
                        DerDictToDataTable(JsonDict);
                        InitGrid();
                    }
                }
                else if (file.Count != 0 && file[0][12].ToString() != "")
                {
                    GlobalVar.ListDrScheme = Json.DeserJsonDList(file[0][12].ToString());
                    DerDictToDataTable(GlobalVar.ListDrScheme);
                    InitGrid();
                }
                else
                {
                    var _dictTem = new Dictionary<string, object>();
                    _dictTem.Add("Name", GlobalVar.SelectName);
                    IList<object[]> emlTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ConfigTemp, _dictTem);
                    //if (emlTem.Count != 0 && emlTem[0][1].ToString() == "V1.0")
                    {
                        //GlobalVar.ListDrScheme = Json.DeserJsonDList(emlTem[0][2].ToString());
                        DerDictToDataTable(GlobalVar.ListDrScheme);
                        InitGrid();
                    }
                    //else
                    //{
                    //    var dictTem = new Dictionary<string, object>();
                    //    dictTem.Add("Name", GlobalVar.SelectName);
                    //    IList<object[]> emlTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ConfigTemp, dictTem);
                    //    if (emlTem.Count != 0 && emlTem[0][1].ToString() == "V1.0")
                    //    {
                    //        GlobalVar.ListDrScheme = Json.DeserJsonDList(emlTem[0][2].ToString());
                    //        DerDictToDataTable(GlobalVar.ListDrScheme);
                    //    }
                    //}
                }

            }

            else
            {
                if (GlobalVar.SelectName != "")
                {
                
                    string emlName = GlobalVar.SelectName + "列编辑";
                    string path = GlobalVar.TemporaryFilePath + "config\\" + emlName + ".txt";
                    if (File.Exists(path)) //验证临时文件
                    {
                        string JsonRead = File.ReadAllText(path);
                        GlobalVar.ListDrScheme = Json.DeserJsonDList(JsonRead);
                        if (GlobalVar.ListDrScheme == null)
                            gcExcelCol.DataSource = null;
                        //DataTable dt = CreateTable();
                        DerDictToDataTable(GlobalVar.ListDrScheme);
                        InitGrid();
                    }
                    else
                    {
                        var dictTem = new Dictionary<string, object>();
                        dictTem.Add("Name", GlobalVar.SelectName);
                        //IList<object[]> emlTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ConfigTemp, dictTem);
                        //if (emlTem.Count != 0 && emlTem[0][1].ToString() == "V2.0")
                        {
                            //GlobalVar.ListDrScheme = Json.DeserJsonDList(emlTem[0][2].ToString());
                            DerDictToDataTable(GlobalVar.ListDrScheme);
                            InitGrid();
                        }

                }
            }
            else
            {
                
            }
        }



        }

        private void DerDictToDataTable(Dictionary<string, List<object>> JsonDict)
        {
            foreach (KeyValuePair<string, List<object>> Temp in JsonDict)
            {
                object[] obj = new object[12];
                obj[0] = Temp.Value[0];
                obj[1] = Temp.Value[1];
                obj[2] = Temp.Key;
                obj[3] = Temp.Value[2];
                obj[4] = Temp.Value[3];
                obj[5] = Temp.Value[4];
                obj[6] = Temp.Value[5];
                obj[7] = Temp.Value[6];
                obj[8] = Temp.Value[7];
                obj[9] = Temp.Value[8];
                obj[10] = Temp.Value[9];
                //obj[11] = Temp.Value[10];
                ObjDataTable.Add(obj);
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

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            if (cbeConfig.Text == "")
            {
                XtraMessageBox.Show("请先选择配置表类型再操作...");
                return;
            }
            int RowCount = _dt.Rows.Count + 1;
            ColEdit ce = new ColEdit(RowCount);
            ce.ShowDialog(this);
        }

        private void tsmiUpdate_Click(object sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            if (_dr == null)
                return;
            if (cbeConfig.Text == "")
            {
                XtraMessageBox.Show("请先选择配置表类型再操作...");
                return;
            }
            ColEdit ce = new ColEdit(_dr, count);
            ce.ShowDialog(this);
        }

        private void tsmiInsert_Click(object sender, EventArgs e)
        {
            if (_dr == null)
                return;
            if (cbeConfig.Text == "")
            {
                XtraMessageBox.Show("请先选择配置表类型再操作...");
                return;
            }int RowIndex = int.Parse(_dr["Index"].ToString());
            ColEdit ce = new ColEdit(RowIndex, count);
            ce.ShowDialog(this);
        }

        public void AddDataRow(object[] obj)
        {
            _dt.Rows.Add(obj);
            gcExcelCol.DataSource = _dt;
        }

        public void UpdataDataRow(object[] obj)
        {
            _dt.Rows[RowCount][0] = obj[0];
            _dt.Rows[RowCount][1] = obj[1];
            _dt.Rows[RowCount][2] = obj[2];
            _dt.Rows[RowCount][3] = obj[3];
            _dt.Rows[RowCount][4] = obj[4];
            _dt.Rows[RowCount][5] = obj[5];
            _dt.Rows[RowCount][6] = obj[6];
            _dt.Rows[RowCount][7] = obj[7];
            _dt.Rows[RowCount][8] = obj[8];
            _dt.Rows[RowCount][9] = obj[9];
            _dt.Rows[RowCount][10] = obj[10];
            _dt.Rows[RowCount][11] = obj[11];
            _dt.Rows[RowCount][12] = obj[12];
            gcExcelCol.DataSource = _dt;
        }

        public void InsertDataRow(object[] obj)
        {
            DataTable Downdt = new DataTable();
            DataTable dt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvExcelCol.Columns)
            {
                coList.Add(col.FieldName);
            }
            foreach (var colName in coList.ToArray())
            {
                dt.Columns.Add(new DataColumn(colName, typeof(object)));
                Downdt.Columns.Add(new DataColumn(colName, typeof(object)));
            }

            for (int i = 0; i < _dt.Rows.Count; i++)
            {
                if (i <= RowCount)
                {
                    dt.Rows.Add(_dt.Rows[i].ItemArray);
                }
                else
                {
                    Downdt.Rows.Add(_dt.Rows[i].ItemArray);
                }
            }
            dt.Rows.Add(obj);
            foreach (DataRow ddt in Downdt.Rows)
            {
                ddt["Index"] = int.Parse(ddt["Index"].ToString()) + 1;
                dt.Rows.Add(ddt.ItemArray);
            }
            _dt = dt;
            gcExcelCol.DataSource = _dt;
        }

        public string SaveJson()
        {
            Dictionary<string, List<object>> Dict = new Dictionary<string, List<object>>();
            foreach (DataRow Row in _dt.Rows)
            {
                List<object> ListTemp = new List<object>()
                {
                    Row[0], Row[1], Row[3], Row[4], Row[5], Row[6], Row[7], Row[8],Row[9],Row[10],Row[11],Row[12]
                };
                Dict.Add(Row[2].ToString(), ListTemp);
            }
            var JsonSave=Json.SerJson(Dict);
            if (GlobalVar.IsIndependent)
            {
                
                GlobalVar.JsonTxtPath = GlobalVar.TemporaryFilePath + "config\\" + cbeConfig.Text +"列编辑.txt";
                Write(GlobalVar.JsonTxtPath, JsonSave);
            }
            else
            {
                string name = GlobalVar.VNode[0] + "-" + GlobalVar.VNode[1] + "-" + GlobalVar.VNode[2] + "-" +cbeConfig.Text +
                    "列编辑.txt";
                GlobalVar.JsonTxtPath = GlobalVar.TemporaryFilePath + "config\\" + name + ".txt";
                Write(GlobalVar.JsonTxtPath, JsonSave);
            }
            return JsonSave;
            
        }


        private void gvExcelCol_MouseDown(object sender, MouseEventArgs e)
        {
            //获得光标位置
            var hi = gvExcelCol.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //获得选中行的数值
            RowCount = hi.RowHandle;
            _dr = this.gvExcelCol.GetDataRow(RowCount);

            //获得当前选中行之前的行有几个False
            count = 0;
            for (int i = 0; i < int.Parse(_dr[0].ToString())-1; i++)
            {
                var dr = _dt.Rows[i];
                if (!(bool.Parse(dr["Result"].ToString())))
                {
                    count++;
                }
            }
        }

        public void Write(string path,string json)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                //开始写入
                sw.Write(json);
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();
            }
            catch (Exception e)
            {
                XtraMessageBox.Show("发生未知错误，请联系技术人员"+"\r\n"+"错误信息："+e.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            if (!GlobalVar.IsIndependent)
            {
                bool suc =SaveJsonToData(GlobalVar.VNode);
                if (!suc)
                    return;
                string vehicel = GlobalVar.VNode[0] + "-" + GlobalVar.VNode[1] + "-" + GlobalVar.VNode[2] + "-" + cbeConfig.Text+ "列编辑.txt";

                
                if (GlobalVar.SelectName != "")
                {
                    if(File.Exists(GlobalVar.TemporaryFilePath + "config\\" + vehicel))
                        File.Delete(GlobalVar.TemporaryFilePath + "config\\" + vehicel);
                }
                GlobalVar.IsColumEdit = true;
            }
            else
            {
                string fjson = SaveJson();  
                var dictTem = new Dictionary<string, object>();
                dictTem.Add("Name", cbeConfig.Text);
                dictTem.Add("Version", cbVersion.Text);
                dictTem.Add("Content",fjson);
                string match = cbeConfig.Text.Split('配')[0];
                dictTem.Add("MatchSort", match);
                dictTem.Add("ImportDate", DateTime.Now);
                IList<object[]> emlTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ConfigTemp, dictTem);
                IList<object[]> emlTemv = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ConfigTemp_Ver, dictTem);
                if (emlTemv.Count == 2 || emlTemv.Count == 1)
                {
                    string error = "";
                    _store.Update(EnumLibrary.EnumTable.ConfigTemp, dictTem, out error);
                    XtraMessageBox.Show("该版本的配置表更新成功！", "提示", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning);
                    string conName = cbeConfig.Text;
                    if(File.Exists(GlobalVar.TemporaryFilePath + "config\\" + cbeConfig.Text + "列编辑.txt"))
                        File.Delete(GlobalVar.TemporaryFilePath + "config\\" + conName + ".txt");

                    GlobalVar.IsColumEdit = true;
                }
                else 
                {
                    if(cbeConfig.Text==""|| cbVersion.Text=="")
                        return;
                    int i = emlTem.Count + 1;
                    string ver = "V" + i.ToString() + ".0";
                    dictTem["Version"] = ver;
                    string errorQ = "";
                    _store.AddConfigTemp(dictTem, out errorQ);
                    if (errorQ == "")
                    {
                        XtraMessageBox.Show("上传数据库成功！", "提示", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning);
                        string conName = cbeConfig.Text;
                        File.Delete(GlobalVar.TemporaryFilePath + "config\\" + conName+  ".txt");
                        GlobalVar.IsColumEdit = true;
                        //File.Delete(path);
                    }
                }

            }

        }

        private bool SaveJsonToData(List<string> node)
        {
            bool succ = false;
            string fjson = SaveJson();
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
                    Dictionary<string, object> dictCNode = new Dictionary<string, object>();
                    dictCNode.Add("VehicelType", node[0]);
                    dictCNode.Add("VehicelConfig", node[1]);
                    dictCNode.Add("VehicelStage", node[2]);
                    //dictCNode.Add("EmlTableColEdit", fjson);
                    //dictCNode.Add("EmlTemplate", "");
                    dictCNode.Add("MatchSort", "");
                    dictCNode.Add("Topology", "");
                    dictCNode.Add("CfgTemplateName", GlobalVar.SelectName);
                    dictCNode.Add("CfgTemplate", "");
                    dictCNode.Add("CfgTemplateJson", "");
                    dictCNode.Add("EmlTemplateName", "");
                    dictCNode.Add("EmlTemplate", "");
                    dictCNode.Add("BaudJson", "");
                    dictCNode.Add("TplyDescrible", "");
                    dictCNode.Add("ConTableColEdit", fjson);
                    dictCNode.Add("EmlTableColEdit", "");

                    listFile = ReadDataByVehicel(GlobalVar.VNode);
                    string error = "";
                    if (listFile.Count == 0)
                    {
                        if (_store.AddFileLinkByVehicel(dictCNode, out error))
                        {
                            XtraMessageBox.Show("上传数据库成功！", "提示", MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Warning);
                            succ = true;
                            GlobalVar.IsColumEdit = true;
                        }
                    }
                    else
                    {
                        bool i = _store.Update(EnumLibrary.EnumTable.FileLinkByVehicelColEml, dictCNode, out error);
                        if (i)
                        {
                            XtraMessageBox.Show("上传数据库成功！", "提示", MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Warning);
                            GlobalVar.IsColumEdit = true;
                            succ = true;
                        }
                    }
                }
            }
            return succ;
        }

        private void cbVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            string Version = cbVersion.Text;
            
            if (Version != "")
            {
                if (!cfgVersion && GlobalVar.SelectName != "")
                {
                    cfgVersion = true;
                    return;
                }
                Dictionary<string, object> _dictName = new Dictionary<string, object>();
                _dictName.Add("Name", cbeConfig.Text);
                _dictName.Add("Version", Version);
                IList<object[]> emlTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ConfigTemp_Ver, _dictName);
                if (emlTem.Count != 0)
                {
                    _dt = new DataTable();

                    _dt.Rows.Clear();
                    //string eml = emlTem[0][2].ToString();
                    //gcExcelCol.DataSource = null;
                    //gcExcelCol.DataSource
                    ObjDataTable.Clear();
                    GlobalVar.ListDrScheme = Json.DeserJsonDList(emlTem[0][2].ToString());
                    DerDictToDataTable(GlobalVar.ListDrScheme);
                    InitGrid();
                }
                tsmiAdd.Enabled = true;
                tsmiInsert.Enabled = true;
                tsmiUpdate.Enabled = true;
            }
        }

        private void cbeConfig_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbVersion.Properties.Items.Clear();
            cbVersion.Enabled = true;
            cbVersion.SelectedIndex = -1;
            _dt.Rows.Clear();
            var dictTem = new Dictionary<string, object>();
            dictTem.Add("Name", cbeConfig.Text);
            IList<object[]> emlTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ConfigTemp, dictTem);
            foreach (object[] item in emlTem)
            {
                cbVersion.Properties.Items.Add(item[1].ToString());
            }
        }

        private void gcExcelCol_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //获得光标位置
            var hi = gvExcelCol.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //获得选中行的数值
            RowCount = hi.RowHandle;
            _dr = this.gvExcelCol.GetDataRow(RowCount);

            //获得当前选中行之前的行有几个False
            count = 0;
            for (int i = 0; i < int.Parse(_dr[0].ToString()) - 1; i++)
            {
                var dr = _dt.Rows[i];
                if (!(bool.Parse(dr["Result"].ToString())))
                {
                    count++;
                }
            }
            Update();
        }
    }
}