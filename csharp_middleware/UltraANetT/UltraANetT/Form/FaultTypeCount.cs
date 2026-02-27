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


namespace FileEditor.Form
{
    public partial class FaultTypeCount : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        DataTable _dt = new DataTable();
        List<object[]> ObjDataTable = new List<object[]>();
        private bool TxtNull = false;
        private DataRow _dr;
        private int RowCount;
        private int count = 0;
        private ProcStore _store = new ProcStore();
        private int MessageCount;
        Dictionary<string, List<object>> Dict = new Dictionary<string, List<object>>();
        bool cfgVersion = false;
        public FaultTypeCount(DataRow dr)
        {
            InitializeComponent();
            
            MessageCount = int.Parse(dr["MessageCount"].ToString());
           
            if (dr["MsgInformation"].ToString() != "")
            { 
                JsonToList(dr["MsgInformation"].ToString());
                
            }
             else
                InitGrid();

        }

        


        private void InitGrid()
        {
            _dt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvDTC.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList.ToArray())
                _dt.Columns.Add(new DataColumn(colName, typeof(object)));
            
            gcDTC.DataSource = _dt;
        }

        private void JsonToList(string json)
        {
            Dictionary<string, List<object>> listDict = Json.DeserJsonDList(json);
            InitGrid();
            foreach (KeyValuePair<string, List<object>> dict in listDict)
            {
                object[] obj = new object[gvDTC.Columns.Count];
                //int i = 0;
                obj[0] = dict.Value[0];
                obj[1] = dict.Key;
                obj[2] = dict.Value[1];
                obj[3] = dict.Value[2];
                obj[4] = dict.Value[3];
                obj[5] = dict.Value[4];
                _dt.Rows.Add(obj);
            }
            gcDTC.DataSource = _dt;
        }

        private void DerDictToDataTable(Dictionary<string, List<object>> JsonDict)
        {
            foreach (KeyValuePair<string, List<object>> Temp in JsonDict)
            {
                object[] obj = new object[11];
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
        
        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            int RowCount = _dt.Rows.Count + 1;
            FaultMsg ce = new FaultMsg(RowCount);
            ce.ShowDialog(this);
        }
        
        private void tsmiUpdate_Click(object sender, EventArgs e)
        {
            if (_dr == null)
            {
                MessageBox.Show("请先选中一行再修改...");
                return;
            }
            FaultMsg ce = new FaultMsg(_dr);
            ce.ShowDialog(this);
        }
        private void tsmiInsert_Click(object sender, EventArgs e)
        {
            if (_dr == null)
            {
                MessageBox.Show("请先选中一行再插入...");
                return;
            }
            //int RowIndex = int.Parse(_dr["Index"].ToString());
            FaultMsg ce = new FaultMsg (RowCount,gvDTC.Columns.Count);
            ce.ShowDialog(this);
        }

        public void AddDataRow(object[] obj)
        {
            _dt.Rows.Add(obj);
            gcDTC.DataSource = _dt;
        }

        public void UpdataDataRow(object[] obj)
        {
            _dt.Rows[RowCount][0] = obj[0];
            _dt.Rows[RowCount][1] = obj[1];
            _dt.Rows[RowCount][2] = obj[2];
            _dt.Rows[RowCount][3] = obj[3];
            _dt.Rows[RowCount][4] = obj[4];
            _dt.Rows[RowCount][5] = obj[5];
            gcDTC.DataSource = _dt;
        }

        public void InsertDataRow(object[] obj)
        {
            DataTable Downdt = new DataTable();
            DataTable dt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvDTC.Columns)
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
                //ddt["Index"] = int.Parse(ddt["Index"].ToString()) + 1;
                dt.Rows.Add(ddt.ItemArray);
            }
            _dt = dt;
            gcDTC.DataSource = _dt;
        }

        public string SaveJson(out bool same)
        {
            same = false;
            Dict = new Dictionary<string, List<object>>();
            foreach (DataRow Row in _dt.Rows)
            {
                List<object> ListTemp = new List<object>()
                {
                    Row[0], Row[2], Row[3], Row[4],Row[5]
                };
                Dict.Add(Row[1].ToString(), ListTemp);
            }

            
            //if (Dict.Count < MessageCount||Dict.Count >MessageCount)
            //{
            //    if(XtraMessageBox.Show("当前故障类型的DTC报文项数与前面表格中的项数不符，是否关闭？", "提示", MessageBoxButtons.OKCancel,
            //        MessageBoxIcon.Warning) == DialogResult.Cancel)
            //        same = true;

            //}

            var JsonSave=Json.SerJson(Dict);
            return JsonSave;
            //Write(GlobalVar.JsonTxtPath, JsonSave);
        }


        private void gvExcelCol_MouseDown(object sender, MouseEventArgs e)
        {
            //获得光标位置
            var hi = gvDTC.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //获得选中行的数值
            RowCount = hi.RowHandle;
            _dr = this.gvDTC.GetDataRow(RowCount);

            //获得当前选中行之前的行有几个False
            
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

        
       

        private void tsmiDel_Click(object sender, EventArgs e)
        {
            if (_dr == null)
                return;
            _dr.Delete();
            _dr = null;
        }

        private void FaultTypeCount_FormClosing(object sender, FormClosingEventArgs e)
        {
            Dict = new Dictionary<string, List<object>>();
            foreach (DataRow Row in _dt.Rows)
            {
                List<object> ListTemp = new List<object>()
                {
                    Row[0], Row[2], Row[3], Row[4],Row[5]
                };
                Dict.Add(Row[1].ToString(), ListTemp);
            }
            if (Dict.Count < MessageCount )
            {
                XtraMessageBox.Show("当前故障类型的DTC报文项数小于前面表格中所填写项数，请再添加" + (MessageCount-Dict.Count) +"项", "提示");
                 e.Cancel = false;
            }
            if (Dict.Count > MessageCount)
            {
                XtraMessageBox.Show("当前故障类型的DTC报文项数大于前面表格中所填写项数，请删除多余"+(Dict.Count - MessageCount) + "项", "提示");
                e.Cancel = false;
            }
        }

        private void gcDTC_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //获得光标位置
            var hi = gvDTC.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //获得选中行的数值
            RowCount = hi.RowHandle;
            _dr = this.gvDTC.GetDataRow(RowCount);
            if (_dr == null)
            {
                MessageBox.Show("请先选中一行再修改...");
                return;
            }
            FaultMsg ce = new FaultMsg(_dr);
            ce.ShowDialog(this);
        }
    }
}