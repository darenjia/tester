using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraSpreadsheet;
using ProcessEngine;
using UltraANetT.Interface;

namespace UltraANetT.Module
{
    public partial class PassReportNote : DevExpress.XtraEditors.XtraUserControl, IDraw
    {
        private readonly ProcStore _store;
        private readonly ProcShow _show;
        private readonly IDraw _draw;
        ProcFile _file = new ProcFile();
        Dictionary<string, object> _dictNote = new Dictionary<string, object>();
        private bool reportImport = false;
        private bool isfirstAddColp = false;
        private bool isfirstAddColf = false;
        private SortType _curruntSort;

        public PassReportNote()
        {
            InitializeComponent();
            _draw = this;
            _store = new ProcStore();
            _show = new ProcShow();
            //初始化表格
            _draw.InitGrid();
            //从数据库中查找出所有的车型放入车型Item中
            InitVehicel();
            radioGroup.SelectedIndex = -1;
        }
        private void InitVehicel()
        {
            cbVehicel.Properties.Items.Clear();
            cbeVehicel.Properties.Items.Clear();
            
            List<string> itemAllList = new List<string>();
            var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 0);
            var deptF = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 0);

            itemAllList = SortItem(dept, deptF);

            cbVehicel.Properties.Items.Clear();
            cbeVehicel.Properties.Items.Clear();
            foreach (var item in itemAllList)
            {
                {
                    cbVehicel.Properties.Items.Add(item);
                    cbeVehicel.Properties.Items.Add(item);
                    //curruntFVehicel = item.ToString();
                }
            }
        }
        private void gridControl1_Click(object sender, EventArgs e)
        {

        }

        public void InitGrid()
        {
            //把Grid控件中的列名称依次添加到List结构中
            var coList = new List<string>();
            foreach (GridColumn col in gvPassNote.Columns)
                coList.Add(col.FieldName);
            ////从数据库中查询指定数据
            ////var dt = _show.DrawDtFromQuestionNote(coList.ToArray(), EnumLibrary.EnumTable.PassReportNote);
            //////将数据源赋值给控件
            ////gcPass.DataSource = dt;
            ////从数据中读取车型名称列的数据集合


        }
        /// <summary>
        /// 根据筛选条件从数据库中筛选出数据，合并分析处理好后给GridView
        /// </summary>
        void IDraw.Submit()
        {
            switch (_curruntSort)
            {
                case SortType.Pass:
                    Dictionary<string, object> dictPNote = _draw.GetDataFromUI();
                    Dictionary<string, object> dictFNote = GetDataFromFailUI();
                    if (dictFNote.Count != 6&& dictPNote.Count != 6)
                    {
                        return;
                    }
                    gcFail.DataSource = SearchDataTable(dictFNote, gvFailNote, EnumLibrary.EnumTable.PassReportNote);
                    gcPass.DataSource = SearchDataTable(dictPNote, gvPassNote, EnumLibrary.EnumTable.PassReportNote);
                    //SetControlEnable();
                    break;
                case SortType.Fail:
                    Dictionary<string, object> dictPANote = _draw.GetDataFromUI();
                    Dictionary<string, object> dictFANote = GetDataFromFailUI();
                    if (dictFANote.Count != 6&& dictPANote.Count !=6)
                    {
                        return;
                    }
                    gcFail.DataSource = SearchDataTable(dictFANote, gvFailNote, EnumLibrary.EnumTable.QuestionNoteSort);
                    gcPass.DataSource = SearchDataTable(dictPANote, gvPassNote, EnumLibrary.EnumTable.QuestionNoteSort);
                    //SetControlEnable();
                    break;
                case SortType.All:
                    Dictionary<string, object> dictPaNote = _draw.GetDataFromUI();
                    Dictionary<string, object> dictFaNote = GetDataFromFailUI();

                    var passalist = _store.GetSpecialByEnum(EnumLibrary.EnumTable.PassReportNote, dictPaNote);
                    var failalist = _store.GetSpecialByEnum(EnumLibrary.EnumTable.QuestionNoteSort, dictPaNote);

                    var passbList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.PassReportNote, dictFaNote);
                    var failbList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.QuestionNoteSort, dictFaNote);

                    int countA =(passalist.Count >= failalist.Count)? passalist.Count:failalist.Count;
                    int countB = (passbList.Count >= failbList.Count) ? passbList.Count : failbList.Count;

                    AddGridViewColumn(countA, gvPassNote);
                    AddGridViewColumn(countB, gvFailNote);

                    //DataTable dt = SearchDataTable(dictPaNote, gvPassNote, EnumLibrary.EnumTable.PassReportNote);
                    //DataTable fdt = SearchDataTable(dictFaNote, gvFailNote, EnumLibrary.EnumTable.QuestionNoteSort);
                    //foreach (DataRow row in fdt.Rows)
                    //{
                    //    object[] rowObj = row.ItemArray;
                    //    dt.Rows.Add(rowObj);
                    //}

                    gcPass.DataSource = FillGridViewByAllContion(passalist, failalist,  gvPassNote);
                    gcFail.DataSource = FillGridViewByAllContion(passbList, failbList,  gvFailNote);
                    //SetControlEnable();
                    break;
            }
        }
        /// <summary>
        /// 将几次测试相同映射ID拼接到一起，分为两种情况
        /// 相同映射ID和相同评价项目的数据拼接到同一行或者相同映射ID下新出现的评价项目新加一行
        /// 当其中某一次测试结果没有的时候用空表示，不能省略，函数返回一个datatable
        /// </summary>
        /// <param name="dtaList">要拼接的数据</param>
        /// <param name="grid">要显示的GridView名称</param>
        /// <returns></returns>
        private DataTable RevertDataToGridView(IList<object[]> dtaList, GridView grid)
        {
            DataTable dt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in grid.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList)
                dt.Columns.Add(new DataColumn(colName, typeof(object)));
            int colcount = grid.Columns.Count;
            object[] row = new object[colcount];
            //int i = 0;
            //if (dtaList.Count > 20)
            //{
            //    i = dtaList.Count - 20;
            //}
            row[0] = dtaList[0][0];
            row[1] = dtaList[0][1];
            row[2] = dtaList[0][2];
            row[3] = dtaList[0][3];
            row[4] = dtaList[0][4];
            row[5] = dtaList[0][5];

            Dictionary<string, List<Dictionary<string, string>>> assItemdict = new Dictionary<string, List<Dictionary<string, string>>>();
            assItemdict = Json.DerJsonDictLd(dtaList[0][6].ToString());
            foreach (KeyValuePair<string, List<Dictionary<string, string>>> listitem in assItemdict)
            {

                foreach (var dictitem in listitem.Value)
                {
                    row[6] = dictitem["ExapID"];
                    row[7] = dictitem["ExapName"];
                    row[8] = dictitem["AssessItem"];
                    row[9] = dictitem["DescriptionOfValue"];
                    row[10] = dictitem["MinValue"];
                    row[11] = dictitem["MaxValue"];
                    row[12] = dictitem["NormalValue"];
                    row[13] = dictitem["TestValue"];
                    row[14] = dictitem["Result"];
                    row[15] = dtaList[0][7];

                    //其他次测试数据中是否有在相同的映射ID下有相同的评价项目，用bool型变量isHave表示
                    //如果有则把数据拼接一起，没有则用空表示
                    for (int j = 1; j < dtaList.Count; j++)
                    {
                        bool isHave = false;
                        Dictionary<string, List<Dictionary<string, string>>> assItemdictl =
                            new Dictionary<string, List<Dictionary<string, string>>>();
                        assItemdictl = Json.DerJsonDictLd(dtaList[j][6].ToString());
                        foreach (KeyValuePair<string, List<Dictionary<string, string>>> listiteml in assItemdictl)
                        {
                            bool isfind = false;//判断此次测试数据中是否找到与dictitem["ExapID"]和dictitem["AssessItem"]都相同的
                            foreach (var dictiteml in listiteml.Value)
                            {
                                if (dictitem["ExapID"] == dictiteml["ExapID"] && dictitem["AssessItem"] == dictiteml["AssessItem"])
                                {
                                    int index = 15 + (j - 1) * 3 + 1;
                                    row[index] = dictiteml["TestValue"];
                                    index++;
                                    row[index] = dictiteml["Result"];
                                    index++;
                                    row[index] = dtaList[j][7];
                                    isfind = true;
                                    isHave = true;
                                    break;
                                }
                            }
                            if (isfind)
                                break;
                        }
                        if (!isHave)
                        {
                            int index = 15 + (j - 1) * 3 + 1;
                            row[index] = "";
                            index++;
                            row[index] = "";
                            index++;
                            row[index] = "";
                        }
                    }
                    dt.Rows.Add(row);

                }
                bool isHavenew = false;//判断其他次测试中有没有出现第一次测试中没有出现的评价项目，有新的评价项目时则新加一行
                for (int j = 1; j < dtaList.Count; j++)
                {
                    Dictionary<string, List<Dictionary<string, string>>> assItemdictl =
                        new Dictionary<string, List<Dictionary<string, string>>>();
                    assItemdictl = Json.DerJsonDictLd(dtaList[j][6].ToString());
                    foreach (KeyValuePair<string, List<Dictionary<string, string>>> listiteml in assItemdictl)
                    {
                        bool isfind = false;//判断第此次测试数据中是否有之前相同映射ID下测试没有出现过得评价项目

                        foreach (var dictiteml in listiteml.Value)
                        {
                            isHavenew = false;
                            if (dictiteml["ExapID"] != listitem.Value[0]["ExapID"])
                            {
                                continue;
                            }
                            foreach (var item in listitem.Value)
                            {
                                if (item["ExapID"] == dictiteml["ExapID"] && item["AssessItem"] == dictiteml["AssessItem"])
                                {
                                    isfind = true;

                                }

                            }
                            if (!isfind)//有没有出现过的则要新加一行数据，并把所其他几次测试中的该映射ID下的这个评价项目的数据拼接到一行
                            {
                                isHavenew = true;
                                row[0] = dtaList[0][0];
                                row[1] = dtaList[0][1];
                                row[2] = dtaList[0][2];
                                row[3] = dtaList[0][3];
                                row[4] = dtaList[0][4];
                                row[5] = dtaList[0][5];
                                row[6] = dictiteml["ExapID"];
                                row[7] = dictiteml["ExapName"];
                                row[8] = dictiteml["AssessItem"];
                                row[9] = dictiteml["DescriptionOfValue"];
                                row[10] = dictiteml["MinValue"];
                                row[11] = dictiteml["MaxValue"];
                                row[12] = dictiteml["NormalValue"];
                                row[13] = "";
                                row[14] = "";
                                row[15] = "";
                                int index = 15 + (j - 1) * 3 + 1;
                                row[index] = dictiteml["TestValue"];
                                index++;
                                row[index] = dictiteml["Result"];
                                index++;
                                row[index] = dtaList[j][7];
                                //把其他几次测试中的该映射ID下的这个评价项目的数据拼接到一行
                                for (int i = 1; i < dtaList.Count; i++)
                                {
                                    if (i == j)
                                        continue;
                                    Dictionary<string, List<Dictionary<string, string>>> assItemdictll =
                                        new Dictionary<string, List<Dictionary<string, string>>>();
                                    assItemdictll = Json.DerJsonDictLd(dtaList[i][6].ToString());
                                    foreach (KeyValuePair<string, List<Dictionary<string, string>>> listitemll in
                                        assItemdictll)
                                    {
                                        bool isame = false;
                                        string nameId = "";
                                        Dictionary<string, string> dictsame = new Dictionary<string, string>();
                                        foreach (var dictitem in listitemll.Value)
                                        {
                                            nameId = dictitem["ExapID"];
                                            if (dictitem["ExapID"] == dictiteml["ExapID"] && dictitem["AssessItem"] == dictiteml["AssessItem"])
                                            {
                                                isame = true;
                                                dictsame = dictitem;
                                                break;
                                                
                                            }
                                        }
                                        //如果找到在相同映射ID下有相同评价项目的话，就把数据加入新行
                                        if (isame && dictsame.Count != 0 && dictiteml["ExapID"] == nameId)
                                        {
                                            int indexl = 15 + (i - 1) * 3 + 1;
                                            row[indexl] = dictiteml["TestValue"];
                                            indexl++;
                                            row[indexl] = dictiteml["Result"];
                                            indexl++;
                                            row[indexl] = dtaList[i][7];
                                        }
                                        // 否则在相同映射ID下没有找到相同评价项目的话，这次的测试数据就为空，加入新行
                                        else if (!isame && dictiteml["ExapID"] == nameId)
                                        {
                                            int indexl = 15 + (i - 1) * 3 + 1;
                                            row[indexl] = "";
                                            indexl++;
                                            row[indexl] = "";
                                            indexl++;
                                            row[indexl] = "";
                                        }
                                    }

                                }


                            }
                            //把拼接好的新行加入datatable中
                            if (isHavenew)
                            {
                                dt.Rows.Add(row);
                                break;
                            }
                        }
                        if (isHavenew)
                        {
                            break;
                        }
                    }
                    if (isHavenew)
                    {
                        break;
                    }
                }
            }
            return dt;
        }
        /// <summary>
        /// 根据查询出的数据条数为相应的GridView动态添加列，每一条数据增加三列
        /// </summary>
        /// <param name="icol">数据条数</param>
        /// <param name="grid">要增加列的GridView名称</param>
        private void AddGridViewColumn(int icol, GridView grid)
        {
            for (int k = 16; grid.Columns.Count > 16; )
            {
                grid.Columns.RemoveAt(k);
            }
            for (int i = 0; i < icol - 1; i++)
            { 
               GridColumn colv = new GridColumn();
               colv.Caption = @"第" + (i + 2) + @"次测试值";
               colv.Name = "value" + i;
               colv.FieldName = "value" + i;
               colv.Visible = true;
            
               GridColumn colr = new GridColumn();
               colr.Caption = @"第" + (i + 2) + @"次测试结果";
               colr.Name = "result" + i;
               colr.FieldName = "result" + i;
               colr.Visible = true;
            
               GridColumn colt = new GridColumn();
               colt.Caption = @"第" + (i + 2) + @"次测试时间";
            
               colt.Name = "time" + i;
               colt.FieldName = "time" + i;
               colt.Visible = true;
               if (grid == gvFailNote)
               {
                   gvFailNote.Columns.AddRange(new GridColumn[] {colv, colr, colt});
               }
               if (grid == gvPassNote)
               {
                   gvPassNote.Columns.AddRange(new GridColumn[] { colv, colr, colt });
               }
            }
            if (grid == gvFailNote)
            {
                isfirstAddColf = true;
            }
            else if (grid == gvPassNote)
            {
                isfirstAddColp = true;
            }
            
        }
        /// <summary>
        /// 将同一个测试映射ID下的拼接失败的和成功的评价项目拼接到一起，得到新的Json串
        /// 再将这个Json串调用RevertDataToGridView方法把数据填充到GridView中
        /// </summary>
        /// <param name="passList">成功的数据</param>
        /// <param name="failList">失败的数据</param>
        /// <param name="gridView">显示的表名</param>
        /// <returns></returns>
        private DataTable FillGridViewByAllContion(IList<object[]> passList, IList<object[]> failList,GridView gridView)
        {
            var dt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gridView.Columns)
                coList.Add(col.FieldName);

            foreach (var colName in coList)
                dt.Columns.Add(new DataColumn(colName, typeof(object)));
            var dtf = new DataTable();
            var coListf = new List<string>();
            foreach (GridColumn col in gridView.Columns)
                coListf.Add(col.FieldName);

            foreach (var colName in coList)
                dtf.Columns.Add(new DataColumn(colName, typeof(object)));
            object[] objf = new object[8];
            object[] objp = new object[8];
            IList<object[]> faiList = new List<object[]>();
            //找到失败和成功中是在同一次测试时间并且同一个映射ID的找出来拼接起来或者，转成Json格式
            //再调用RevertDataToGridView转成datatable
            foreach (var dtafList in failList)
            {
                bool isFind = false;
                foreach (var dtapList in passList)
                {
                    if (DateTime.Parse(dtafList[7].ToString()).ToString("yyyy-MM-dd HH:mm") == DateTime.Parse(dtapList[7].ToString()).ToString("yyyy-MM-dd HH:mm"))
                    {
                        isFind = true;
                        objf = dtafList;
                        objp = dtapList;
                        break;
                    }
                }
                if (isFind)
                {
                    Dictionary<string, List<Dictionary<string, string>>> assItemdictf =
                        new Dictionary<string, List<Dictionary<string, string>>>();
                    Dictionary<string, List<Dictionary<string, string>>> assItemdictp =
                        new Dictionary<string, List<Dictionary<string, string>>>();
                    Dictionary<string, List<Dictionary<string, string>>> assItemdictnew =
                        new Dictionary<string, List<Dictionary<string, string>>>();
                    assItemdictf = Json.DerJsonDictLd(objf[6].ToString());
                    assItemdictp = Json.DerJsonDictLd(objp[6].ToString());
                    foreach (var itemp in assItemdictp)
                    {
                        bool issame = false;
                        foreach (var item in assItemdictf)
                        {
                            if (itemp.Key == item.Key)
                            {
                                List<Dictionary<string, string>> list = new DetailList<Dictionary<string, string>>();
                                foreach (var itl in item.Value)
                                {
                                    list.Add(itl);
                                }
                                foreach (var itl in itemp.Value)
                                {
                                    list.Add(itl);
                                }
                                assItemdictnew.Add(item.Key, list);
                                issame = true;
                            }
                            else
                            {
                                assItemdictnew.Add(item.Key, item.Value);
                            }
                        }
                        if (!issame)
                        {
                            assItemdictnew.Add(itemp.Key, itemp.Value);
                            issame = false;
                        }
                    }

                    string report = Json.SerJson(assItemdictnew);
                    objf[6] = report;
                    faiList.Add(objf);
                }
                else
                {
                    faiList.Add(dtafList);
                }
            }

            //if (passList.Count > 0)
            //    dt = RevertDataToGridView(passList, gridView);
            if(faiList.Count >0)
                dtf = RevertDataToGridView(failList, gridView);
            foreach (DataRow row in dtf.Rows)
            {
                object[] rowObj = row.ItemArray;
                dt.Rows.Add(rowObj);
            }
            return dt;

        }
        /// <summary>
        /// 根据不同的查询条件，将查出来的数据放入对应的GridView中
        /// </summary>
        /// <param name="dictNote">车型配置阶段等其他查询条件</param>
        /// <param name="grid">对应的GridView名称</param>
        /// <param name="table">查询数据库表名</param>
        /// <returns></returns>
        private DataTable SearchDataTable(Dictionary<string,object> dictNote,GridView grid, EnumLibrary.EnumTable table)
        {
            var dt = new DataTable();
            //从数据库中查询指定表的数据信息
            var departmentList = _store.GetSpecialByEnum(table, dictNote);
            if (departmentList.Count > 0)
            {
                if (grid == gvFailNote)
                {
                    AddGridViewColumn(departmentList.Count, grid);
                }
                else if (grid == gvPassNote)
                {
                   AddGridViewColumn(departmentList.Count, grid);
                }

                var coList = new List<string>();
                foreach (GridColumn col in grid.Columns)
                    coList.Add(col.FieldName);
                
                foreach (var colName in coList)
                    dt.Columns.Add(new DataColumn(colName, typeof(object)));
                dt = RevertDataToGridView(departmentList, grid);
                //dt.Rows.Add(departmentList[departmentList.Count - 1]);
                //int count = 0;
                //foreach (var dept in departmentList)
                //{
                //    if (count < departmentList.Count)
                //        dt.Rows.Add(dept);
                //    count++;
                //}
            }
            else
            {
                dt = null;
                Show(DLAF.LookAndFeel, this, "此筛选条件："+ dictNote["VehicelType"] + dictNote["VehicelConfig"]  + dictNote["VehicelStage"]  + dictNote["TaskRound"]  + dictNote["TestType"]  + dictNote["Module"]  + "下没有通过项...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                //XtraMessageBox.Show("此筛选条件下没有通过项...");
            }
                
            return dt;
        }

        public static DialogResult Show(UserLookAndFeel look, IWin32Window owner, string text, string caption, DialogResult[] buttons, Icon icon, int defaultButton, MessageBoxIcon messageBeepSound)
        {

            XtraMessageBoxForm form = new XtraMessageBoxForm();
            Font defaultFont = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            form.Appearance.Font = defaultFont;
            return form.ShowMessageBoxDialog(new XtraMessageBoxArgs(look, owner, text, caption, buttons, icon, defaultButton));
        }

        Dictionary<string, object> IDraw.GetDataFromUI()
        {
            Dictionary<string, object> dictNote = new Dictionary<string, object>();
            dictNote.Add("VehicelType", cbVehicel.Text);
            dictNote.Add("VehicelConfig", cbConfig.Text);
            dictNote.Add("VehicelStage", cbStage.Text);
            dictNote.Add("TaskRound", cbRound.Text);
            dictNote.Add("TestType", cbTestType.Text);
            dictNote.Add("Module", cbMoudle.Text);
            return dictNote;
        }

        private Dictionary<string, object> GetDataFromFailUI()
        {
            Dictionary<string, object> dictNote = new Dictionary<string, object>();
            dictNote.Add("VehicelType", cbeVehicel.Text);
            dictNote.Add("VehicelConfig", cbeConfig.Text);
            dictNote.Add("VehicelStage", cbeStage.Text);
            dictNote.Add("TaskRound", cbeRound.Text);
            dictNote.Add("TestType", cbeTestType.Text);
            dictNote.Add("Module", cbeMoudle.Text);
            return dictNote;
        }

        public void SetDataToUI(DataRow selectedRow)
        {
            throw new NotImplementedException();
        }

        public void InitDict()
        {
            _dictNote.Add("VehicelType", "");
            _dictNote.Add("VehicelConfig", "");
            _dictNote.Add("VehicelStage", "");
            _dictNote.Add("TaskRound", "");
            _dictNote.Add("TestType", "");
            _dictNote.Add("Module", "");
            _dictNote.Add("ExapID", "");
            _dictNote.Add("ExapName", "");
            _dictNote.Add("AssessItem", "");
            _dictNote.Add("DescriptionOfValue", "");
            _dictNote.Add("MinValue", "");
            _dictNote.Add("Maxvalue", "");
            _dictNote.Add("NormalValue", "");
            _dictNote.Add("TestValue", "");
            _dictNote.Add("Result", "");
            _dictNote.Add("TestTime", "");
        }

        public void SwitchCtl(bool isSwitch)
        {
            throw new NotImplementedException();
        }

        private void cbVehicel_SelectedValueChanged(object sender, EventArgs e)
        {

            if (cbVehicel.SelectedIndex == -1)
            {
                cbConfig.SelectedIndex = -1;
                cbStage.SelectedIndex = -1;
                cbRound.SelectedIndex = -1;
                cbTestType.SelectedIndex = -1;
                cbMoudle.SelectedIndex = -1;

                cbConfig.Enabled = false;
                cbStage.Enabled = false;
                cbRound.Enabled = false;
                cbTestType.Enabled = false;
                cbMoudle.Enabled = false;
                return;
            }
            cbConfig.Enabled = true;
            cbConfig.Properties.Items.Clear();
            cbConfig.Text = "";
            cbStage.Text = "";
            cbRound.Text = "";
            cbTestType.Text = "";
            cbMoudle.Text = "";    
            //cbeConfig.Properties.Items.Clear();
            List<string> itemList = new List<string>();
            List<string> itemFList = new List<string>();
            List<string> itemAllList = new List<string>();
            Dictionary<string,object> dictNode = new Dictionary<string, object>();
            dictNode["VehicelType"] = cbVehicel.Text.Trim();
            dictNode["Condition"] = "VehicelType";
            IList<string> dept = _store.GetSingnalColByCon(EnumLibrary.EnumTable.PassReportNoteByCondition, dictNode,1);
            IList<string> deptF = _store.GetSingnalColByCon(EnumLibrary.EnumTable.QuestionNoteByCondition, dictNode,1);
            //var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 1);
            //var deptF = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 1);
            //把集合赋值给控件
            itemAllList = SortItem(dept, deptF);
            cbConfig.Properties.Items.Clear();
            //cbConfig.Properties.Items.Clear();
            foreach (var item in itemAllList)
                {
                    {
                        cbConfig.Properties.Items.Add(item);
                        
                    }
                }
            
            //var config = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 1);
            ////把集合赋值给控件
            //string curruntConfig = "";
            //foreach (var item in config)
            //{
            //    if (item == curruntConfig)
            //        continue;
            //    else
            //    {
            //        cbConfig.Properties.Items.Add(item);
            //        curruntConfig = item.ToString();
            //    }
            //}
        }

        private void cbeVehicel_SelectedValueChanged(object sender, EventArgs e)
       {
            if (cbeVehicel.SelectedIndex == -1)
            {
                cbeConfig.SelectedIndex = -1;
                cbeStage.SelectedIndex = -1;
                cbeRound.SelectedIndex = -1;
                cbeTestType.SelectedIndex = -1;
                cbeMoudle.SelectedIndex = -1;
                radioGroup.SelectedIndex = -1;
                radioGroup.Enabled = false;

                cbeConfig.Enabled = false;
                cbeStage.Enabled = false;
                cbeRound.Enabled = false;
                cbeTestType.Enabled = false;
                cbeMoudle.Enabled = false;
                return;
            }
            cbeConfig.Enabled = true;
            cbeConfig.Properties.Items.Clear();
            cbeConfig.Text = "";
            cbeStage.Text = "";
            cbeRound.Text = "";
            cbeTestType.Text = "";
            cbeMoudle.Text = "";
            //cbeConfig.Properties.Items.Clear();
            //List<string> itemList = new List<string>();
            //List<string> itemFList = new List<string>();
            List<string> itemAllList = new List<string>();
            Dictionary<string, object> dictNode = new Dictionary<string, object>();
            dictNode["VehicelType"] = cbeVehicel.Text.Trim();
            dictNode["Condition"] = "VehicelType";
            IList<string> dept = _store.GetSingnalColByCon(EnumLibrary.EnumTable.PassReportNoteByCondition, dictNode, 1);
            IList<string> deptF = _store.GetSingnalColByCon(EnumLibrary.EnumTable.QuestionNoteByCondition, dictNode, 1);
            //var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 1);
            //var deptF = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 1);
            //把集合赋值给控件
            itemAllList = SortItem(dept, deptF);
            
            cbeConfig.Properties.Items.Clear();
            foreach (var item in itemAllList)
            {
                
               cbeConfig.Properties.Items.Add(item);
               
                
            }

            //var config = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 1);
            ////把集合赋值给控件
            //string curruntConfig = "";
            //foreach (var item in config)
            //{
            //    if (item == curruntConfig)
            //        continue;
            //    else
            //    {
            //        cbeConfig.Properties.Items.Add(item);
            //        curruntConfig = item.ToString();
            //    }
            //}
        }

        private void cbConfig_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbVehicel.SelectedIndex == -1)
            {
                cbStage.SelectedIndex = -1;
                cbRound.SelectedIndex = -1;
                cbTestType.SelectedIndex = -1;
                cbMoudle.SelectedIndex = -1;
                radioGroup.SelectedIndex = -1;
                radioGroup.Enabled = false;

                cbStage.Enabled = false;
                cbRound.Enabled = false;
                cbTestType.Enabled = false;
                cbMoudle.Enabled = false;
                return;
            }
            cbStage.Enabled = true;
            cbStage.Properties.Items.Clear();
            cbStage.Text = "";
            cbRound.Text = "";
            cbTestType.Text = "";
            cbMoudle.Text = "";
            //cbeConfig.Properties.Items.Clear();
            List<string> itemList = new List<string>();
            List<string> itemFList = new List<string>();
            List<string> itemAllList = new List<string>();
            Dictionary<string, object> dictNode = new Dictionary<string, object>();
            dictNode["VehicelType"] = cbVehicel.Text.Trim();
            dictNode["VehicelConfig"] = cbConfig.Text.Trim();
            dictNode["Condition"] = "VehicelConfig";
            IList<string> dept = _store.GetSingnalColByCon(EnumLibrary.EnumTable.PassReportNoteByCondition, dictNode, 2);
            IList<string> deptF = _store.GetSingnalColByCon(EnumLibrary.EnumTable.QuestionNoteByCondition, dictNode, 2);
            //var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 2);
            //var deptF = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 2);
            //把集合赋值给控件
            itemAllList = SortItem(dept, deptF);
            
            //cbeConfig.Properties.Items.Clear();
            foreach (var item in itemAllList)
            {

               cbStage.Properties.Items.Add(item);
                

            }

            //var stage = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 2);
            ////把集合赋值给控件
            //string curruntStage = "";
            //foreach (var item in stage)
            //{
            //    if (item == curruntStage)
            //        continue;
            //    else
            //    {
            //        cbStage.Properties.Items.Add(item);
            //        curruntStage = item.ToString();
            //    }
            //}
        }

        private void cbeConfig_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbeConfig.SelectedIndex == -1)
            {
                cbeStage.SelectedIndex = -1;
                cbeRound.SelectedIndex = -1;
                cbeTestType.SelectedIndex = -1;
                cbeMoudle.SelectedIndex = -1;
                
                cbeStage.Enabled = false;
                cbeRound.Enabled = false;
                cbeTestType.Enabled = false;
                cbeMoudle.Enabled = false;
                return;
            }
            cbeStage.Enabled = true;
            cbeStage.Properties.Items.Clear();
            cbeStage.Text = "";
            cbeRound.Text = "";
            cbeTestType.Text = "";
            cbeMoudle.Text = "";
            List<string> itemAllList = new List<string>();
            Dictionary<string, object> dictNode = new Dictionary<string, object>();
            dictNode["VehicelType"] = cbeVehicel.Text.Trim();
            dictNode["VehicelConfig"] = cbeConfig.Text.Trim();
            dictNode["Condition"] = "VehicelConfig";
            IList<string> dept = _store.GetSingnalColByCon(EnumLibrary.EnumTable.PassReportNoteByCondition, dictNode, 2);
            IList<string> deptF = _store.GetSingnalColByCon(EnumLibrary.EnumTable.QuestionNoteByCondition, dictNode, 2);
            //var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 2);
            //var deptF = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 2);
            //把集合赋值给控件
            itemAllList = SortItem(dept, deptF);

            //cbeConfig.Properties.Items.Clear();
            foreach (var item in itemAllList)
            {
                cbeStage.Properties.Items.Add(item);
            }

            
            //var stage = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 2);
            ////把集合赋值给控件
            //string curruntStage = "";
            //foreach (var item in stage)
            //{
            //    if (item == curruntStage)
            //        continue;
            //    else
            //    {
            //        cbeStage.Properties.Items.Add(item);
            //        curruntStage = item.ToString();
            //    }
            //}
        }

        private void cbStage_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbStage.SelectedIndex == -1)
            {
                cbRound.SelectedIndex = -1;
                cbTestType.SelectedIndex = -1;
                cbMoudle.SelectedIndex = -1;

                cbRound.Enabled = false;
                cbTestType.Enabled = false;
                cbMoudle.Enabled = false;
                return;
            }
                
            cbRound.Enabled = true;
            cbRound.Properties.Items.Clear();
            cbRound.Text = "";
            cbTestType.Text = "";
            cbMoudle.Text = "";
            //cbeRound.Properties.Items.Clear();
            Dictionary<string, object> dictNode = new Dictionary<string, object>();
            dictNode["VehicelType"] = cbVehicel.Text.Trim();
            dictNode["VehicelConfig"] = cbConfig.Text.Trim();
            dictNode["VehicelStage"] = cbStage.Text.Trim();
            dictNode["Condition"] = "VehicelStage";
            IList<string> dept = _store.GetSingnalColByCon(EnumLibrary.EnumTable.PassReportNoteByCondition, dictNode, 3);
            IList<string> deptF = _store.GetSingnalColByCon(EnumLibrary.EnumTable.QuestionNoteByCondition, dictNode, 3);
            //var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 3);
            //var deptF = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 3);
            //把集合赋值给控件
            List<string> itemAllList = new List<string>();
            itemAllList = SortItem(dept, deptF);

            //cbeConfig.Properties.Items.Clear();
            foreach (var item in itemAllList)
            {

                cbRound.Properties.Items.Add(item);


            }

            

            //var round = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 3);
            ////把集合赋值给控件
            //string curruntRound = "";
            //foreach (var item in round)
            //{
            //    if (item == curruntRound)
            //        continue;
            //    else
            //    {
            //        cbRound.Properties.Items.Add(item);
            //        curruntRound = item.ToString();
            //    }
            //}
        }

        private void cbeStage_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbeStage.SelectedIndex == -1)
            {
                cbeRound.SelectedIndex = -1;
                cbeTestType.SelectedIndex = -1;
                cbeMoudle.SelectedIndex = -1;

                cbeRound.Enabled = false;
                cbeTestType.Enabled = false;
                cbeMoudle.Enabled = false;
                return;
            }

            cbeRound.Enabled = true;
            cbeRound.Properties.Items.Clear();
            cbeRound.Text = "";
            cbeTestType.Text = "";
            cbeMoudle.Text = "";
            Dictionary<string, object> dictNode = new Dictionary<string, object>();
            dictNode["VehicelType"] = cbeVehicel.Text.Trim();
            dictNode["VehicelConfig"] = cbeConfig.Text.Trim();
            dictNode["VehicelStage"] = cbeStage.Text.Trim();
            dictNode["Condition"] = "VehicelStage";
            IList<string> dept = _store.GetSingnalColByCon(EnumLibrary.EnumTable.PassReportNoteByCondition, dictNode, 3);
            IList<string> deptF = _store.GetSingnalColByCon(EnumLibrary.EnumTable.QuestionNoteByCondition, dictNode, 3);
            //var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 3);
            //var deptF = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 3);
            //把集合赋值给控件
            List<string> itemAllList = new List<string>();
            itemAllList = SortItem(dept, deptF);

            //cbeConfig.Properties.Items.Clear();
            foreach (var item in itemAllList)
            {

                cbeRound.Properties.Items.Add(item);


            }
            
            //var round = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 3);
            ////把集合赋值给控件
            //string curruntRound = "";
            //foreach (var item in round)
            //{
            //    if (item == curruntRound)
            //        continue;
            //    else
            //    {
            //        cbeRound.Properties.Items.Add(item);
            //        curruntRound = item.ToString();
            //    }
            //}
        }

        private void cbRound_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbRound.SelectedIndex == -1)
            {
                cbTestType.SelectedIndex = -1;
                cbMoudle.SelectedIndex = -1;

                cbTestType.Enabled = false;
                cbMoudle.Enabled = false;
                return;
            }
                
            cbTestType.Enabled = true;
            cbTestType.Properties.Items.Clear();
            cbTestType.Text = "";
            cbMoudle.Text = "";
            //cbeTestType.Properties.Items.Clear();
            Dictionary<string, object> dictNode = new Dictionary<string, object>();
            dictNode["VehicelType"] = cbVehicel.Text.Trim();
            dictNode["VehicelConfig"] = cbConfig.Text.Trim();
            dictNode["VehicelStage"] = cbStage.Text.Trim();
            dictNode["TaskRound"] = cbRound.Text.Trim();
            dictNode["Condition"] = "TaskRound";
            IList<string> dept = _store.GetSingnalColByCon(EnumLibrary.EnumTable.PassReportNoteByCondition, dictNode, 4);
            IList<string> deptF = _store.GetSingnalColByCon(EnumLibrary.EnumTable.QuestionNoteByCondition, dictNode, 4);
            //var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 4);
            //var deptF = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 4);
            //把集合赋值给控件
            List<string> itemAllList = new List<string>();
            itemAllList = SortItem(dept, deptF);

            //cbeConfig.Properties.Items.Clear();
            foreach (var item in itemAllList)
            {

                cbTestType.Properties.Items.Add(item);


            }
            
            //var type = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 4);
            ////把集合赋值给控件
            //string curruntType = "";
            //foreach (var item in type)
            //{
            //    if (item == curruntType)
            //        continue;
            //    else
            //    {
            //        cbTestType.Properties.Items.Add(item);
            //        curruntType = item.ToString();
            //    }
            //}
        }

        private void cbeRound_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbeRound.SelectedIndex == -1)
            {
                cbeTestType.SelectedIndex = -1;
                cbeMoudle.SelectedIndex = -1;
                
                cbeTestType.Enabled = false;
                cbeMoudle.Enabled = false;
                return;
            }
                
            cbeTestType.Enabled = true;
            cbeTestType.Properties.Items.Clear();
            cbeTestType.Text = "";
            cbeMoudle.Text = "";
            Dictionary<string, object> dictNode = new Dictionary<string, object>();
            dictNode["VehicelType"] = cbeVehicel.Text.Trim();
            dictNode["VehicelConfig"] = cbeConfig.Text.Trim();
            dictNode["VehicelStage"] = cbeStage.Text.Trim();
            dictNode["TaskRound"] = cbeRound.Text.Trim();
            dictNode["Condition"] = "TaskRound";
            IList<string> dept = _store.GetSingnalColByCon(EnumLibrary.EnumTable.PassReportNoteByCondition, dictNode, 4);
            IList<string> deptF = _store.GetSingnalColByCon(EnumLibrary.EnumTable.QuestionNoteByCondition, dictNode, 4);
            //var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 4);
            //var deptF = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 4);
            //把集合赋值给控件
            List<string> itemAllList = new List<string>();
            itemAllList = SortItem(dept, deptF);

            //cbeConfig.Properties.Items.Clear();
            foreach (var item in itemAllList)
            {

                cbeTestType.Properties.Items.Add(item);


            }

            
            //var type = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 4);
            ////把集合赋值给控件
            //string curruntType = "";
            //foreach (var item in type)
            //{
            //    if (item == curruntType)
            //        continue;
            //    else
            //    {
            //        cbeTestType.Properties.Items.Add(item);
            //        curruntType = item.ToString();
            //    }
            //}
        }

        private void cbTestType_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbTestType.SelectedIndex == -1)
            {
                cbMoudle.SelectedIndex = -1;
                
                cbMoudle.Enabled = false;
                return;
            }
                
            cbMoudle.Enabled = true;
            cbMoudle.Properties.Items.Clear();
            cbMoudle.Text = "";
            //cbMoudle.Properties.Items.Clear();
            Dictionary<string, object> dictNode = new Dictionary<string, object>();
            dictNode["VehicelType"] = cbVehicel.Text.Trim();
            dictNode["VehicelConfig"] = cbConfig.Text.Trim();
            dictNode["VehicelStage"] = cbStage.Text.Trim();
            dictNode["TaskRound"] = cbRound.Text.Trim();
            dictNode["TestType"] = cbTestType.Text.Trim();
            dictNode["Condition"] = "TestType";
            IList<string> dept = _store.GetSingnalColByCon(EnumLibrary.EnumTable.PassReportNoteByCondition, dictNode, 5);
            IList<string> deptF = _store.GetSingnalColByCon(EnumLibrary.EnumTable.QuestionNoteByCondition, dictNode, 5);
            //var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 5);
            //var deptF = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 5);
            //把集合赋值给控件
            List<string> itemAllList = new List<string>();
            itemAllList = SortItem(dept, deptF);

            //cbeConfig.Properties.Items.Clear();
            foreach (var item in itemAllList)
            {

                cbMoudle.Properties.Items.Add(item);


            }

            
            //var name = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 5);
            ////把集合赋值给控件
            //string curruntName = "";
            //foreach (var item in name)
            //{
            //    if (item == curruntName)
            //        continue;
            //    else
            //    {
            //        cbMoudle.Properties.Items.Add(item);
            //        curruntName = item.ToString();
            //    }
            //}
        }

        private void cbeTestType_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbeTestType.SelectedIndex == -1)
            {
                cbeMoudle.SelectedIndex = -1;
                
                cbeMoudle.Enabled = false;
                return;
            }
                
            cbeMoudle.Enabled = true;
            cbeMoudle.Properties.Items.Clear();
            cbeMoudle.Text = "";
            Dictionary<string, object> dictNode = new Dictionary<string, object>();
            dictNode["VehicelType"] = cbeVehicel.Text.Trim();
            dictNode["VehicelConfig"] = cbeConfig.Text.Trim();
            dictNode["VehicelStage"] = cbeStage.Text.Trim();
            dictNode["TaskRound"] = cbeRound.Text.Trim();
            dictNode["TestType"] = cbeTestType.Text.Trim();
            dictNode["Condition"] = "TestType";
            IList<string> dept = _store.GetSingnalColByCon(EnumLibrary.EnumTable.PassReportNoteByCondition, dictNode, 5);
            IList<string> deptF = _store.GetSingnalColByCon(EnumLibrary.EnumTable.QuestionNoteByCondition, dictNode, 5);
            //IList<object[]> dept = _store.GetSpecialByEnum(EnumLibrary.EnumTable.PassReportNoteByCondition, dictNode);
            //IList<object[]> deptF = _store.GetSpecialByEnum(EnumLibrary.EnumTable.QuestionNoteByCondition, dictNode);
            //var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 5);
            //var deptF = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 5);
            //把集合赋值给控件
            List<string> itemAllList = new List<string>();
            itemAllList = SortItem(dept, deptF);

            //cbeConfig.Properties.Items.Clear();
            foreach (var item in itemAllList)
            {

                cbeMoudle.Properties.Items.Add(item);


            }

            //var name = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 5);
            ////把集合赋值给控件
            //string curruntName = "";
            //foreach (var item in name)
            //{
            //    if (item == curruntName)
            //        continue;
            //    else
            //    {
            //        cbeMoudle.Properties.Items.Add(item);
            //        curruntName = item.ToString();
            //    }
            //}
        }
        /// <summary>
        /// 找出失败的报告表中的车型和成功的报告表中的车型两者中不相同的车型，放到一个list中
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="fail"></param>
        /// <returns></returns>
        private List<string> SortItem(IList<string> pass, IList<string> fail)
        {
            List<string> itemList = new List<string>();
            List<string> itemFList = new List<string>();
            List<string> itemAllList = new List<string>();
            //var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 2);
            //var deptF = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 2);
            //把集合赋值给控件
            string curruntVehicel = "";
            foreach (var item in pass)
            {
                bool same = false;
                foreach (var pList in itemList)
                {
                    if (pList == item)
                    {
                        same = true;
                        break;
                    }
                }
                if (same)
                    continue;
                itemList.Add(item);
                itemAllList.Add(item);
            }
            string curruntV = "";
            foreach (var item in fail)
            {
                bool same = false;
                foreach (var fList in itemFList)
                {
                    if (fList == item)
                    {
                        same = true;
                        break;
                    }
                }
                if (same)
                    continue;
                itemFList.Add(item);

            }
            string curruntVe = "";
            for (int i = 0; i < itemFList.Count; i++)
            {
                bool same = false;
                foreach (var item in itemList)
                {
                    if (itemFList[i] == item)
                    {
                        same = true;
                        break;
                    }

                }
                if (!same)
                {
                    itemAllList.Add(itemFList[i]);

                }
            }
            return itemAllList;
        }
        private List<string> SortItem(IList<object[]> pass,IList<object[]> fail)
        {
            List<string> itemList = new List<string>();
            List<string> itemFList = new List<string>();
            List<string> itemAllList = new List<string>();
            //var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.PassReportNote, 2);
            //var deptF = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 2);
            //把集合赋值给控件
            string curruntVehicel = "";
            foreach (var item in pass)
            {
                bool same = false;
                foreach (var pList in itemList)
                {
                    if (pList == item[0].ToString())
                    {
                        same = true;
                        break;
                    }
                }
                if (same)
                    continue;
                itemList.Add(item[0].ToString());
                itemAllList.Add(item[0].ToString());
            }
            string curruntV = "";
            foreach (var item in fail)
            {
                bool same = false;
                foreach (var fList in itemFList)
                {
                    if (fList == item[0].ToString())
                    {
                        same = true;
                        break;
                    }
                }
                if (same)
                    continue;
                itemFList.Add(item[0].ToString());
                
            }
            string curruntVe = "";
            for (int i = 0; i < itemFList.Count; i++)
            {
                bool same = false;
                foreach (var item in itemList)
                {
                    if (itemFList[i] == item)
                    {
                        same = true;
                        break;
                    }
                    
                }
                if (!same)
                {
                    itemAllList.Add(itemFList[i]);
                    
                }
            }
            return itemAllList;
        }

        private void cbMoudle_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbMoudle.SelectedIndex == -1)
                return;
            if (cbeMoudle.SelectedIndex != -1)
            {
                radioGroup.Enabled = true;
                radioGroup.SelectedIndex = -1;
            }
        }

        private void cbeMoudle_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbeMoudle.SelectedIndex == -1)
                return;
            if (cbMoudle.SelectedIndex != -1)
            {
                radioGroup.Enabled = true;
                radioGroup.SelectedIndex = -1;
            }

        }

        private enum SortType
        {
            Pass = 0,
            Fail = 1,
            All = 2
        }
        private void ClearUI()
        {
            cbVehicel.Text = "";
            cbStage.Text = "";
            cbRound.Text = "";
            cbTestType.Text = "";
            cbMoudle.Text = "";
            cbConfig.Text = "";
            cbVehicel.SelectedIndex = -1;
            cbConfig.SelectedIndex = -1;
            cbStage.SelectedIndex = -1;
            cbRound.SelectedIndex = -1;
            cbTestType.SelectedIndex = -1;
            cbMoudle.SelectedIndex = -1;

            cbeVehicel.Text = "";
            cbeConfig.Text = "";
            cbeStage.Text = "";
            cbeRound.Text = "";
            cbeTestType.Text = "";
            cbeMoudle.Text = "";
            
            cbeVehicel.SelectedIndex = -1;
            cbeConfig.SelectedIndex = -1;
            cbeStage.SelectedIndex = -1;
            cbeRound.SelectedIndex = -1;
            cbeTestType.SelectedIndex = -1;
            cbeMoudle.SelectedIndex = -1;
            radioGroup.SelectedIndex = -1;
        }

        private void radioGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (radioGroup.SelectedIndex == -1)
                return;
            int index = radioGroup.SelectedIndex;
            string nodeStr = radioGroup.Properties.Items[index].Description;
            if (nodeStr == "通过")
                _curruntSort = SortType.Pass;
            else if (nodeStr == "失败")
                _curruntSort = SortType.Fail;
            else if (nodeStr == "全部")
                _curruntSort = SortType.All;
            btnSort.Enabled = true;
        }

        private void btnSort_Click(object sender, EventArgs e)
        {

            _draw.Submit();
            //ClearUI();
            SetControlEnable();
        }

        private void SetControlEnable()
        {
            cbConfig.Enabled = false;
            cbStage.Enabled = false;
            cbRound.Enabled = false;
            cbTestType.Enabled = false;
            cbMoudle.Enabled = false;
           
            cbeConfig.Enabled = false;
            cbeStage.Enabled = false;
            cbeRound.Enabled = false;
            cbeTestType.Enabled = false;
            cbeMoudle.Enabled = false;

            radioGroup.Enabled = false;
            btnSort.Enabled = false;
        }

    }
}
