using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.DataAccess.Native.Sql.QueryBuilder;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using ProcessEngine;
using UltraANetT.Interface;
using System.Globalization;
using DevExpress.XtraSpreadsheet;
using NHibernate.DebugHelpers;

namespace UltraANetT.Module
{
    public partial class QuestionNote : DevExpress.XtraEditors.XtraUserControl,IDraw
    {
        private readonly ProcStore _store;
        private readonly ProcShow _show;
        private readonly IDraw _draw;
        ProcFile _file = new ProcFile();
        Dictionary<string,object> _dictNote = new Dictionary<string, object>();
        private bool reportImport = false;
        private bool isfirstAddCol = false;
        private DataRow _dr = null;
        public QuestionNote()
        {
            InitializeComponent();
            _draw = this;
            _store = new ProcStore();
            _show = new ProcShow();
            
            _draw.InitGrid();
            //SortQueNoteData();
            //IsReport();
        }

        Dictionary<string, object> IDraw.GetDataFromUI()
        {
            Dictionary<string,object> dictNote = new Dictionary<string, object>();
            dictNote.Add("VehicelType", cbVehicel.Text);
            dictNote.Add("VehicelConfig", cbConfig.Text);
            dictNote.Add("VehicelStage", cbStage.Text);
            dictNote.Add("TaskRound", cbRound.Text);
            dictNote.Add("TestType", cbType.Text);
            dictNote.Add("Module", cbName.Text);
            return dictNote;
        }

         void IDraw.InitDict()
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

        void IDraw.InitGrid()
        {
            ////把Grid控件中的列名称依次添加到List结构中
            //var coList = new List<string>();
            //foreach (GridColumn col in gvQuesNote.Columns)
            //    coList.Add(col.FieldName);
            ////从数据库中查询指定数据
            //var dt = _show.DrawDtFromQuestionNote(coList.ToArray(), EnumLibrary.EnumTable.QuestionNote);
            ////将数据源赋值给控件
            //gcQuesNote.DataSource = dt;
            //从数据中读取部门名称列的数据集合
            //var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.Department, 0);
            //把集合赋值给控件
            //cbDept.Properties.Items.AddRange(dept.ToArray());
            var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.QuestionNote, 0);
            //把集合赋值给控件
            string curruntVehicel = "";
            cbVehicel.Properties.Items.Clear();
            List<string> itemList = new List<string>();
            foreach (var item in dept)
            {
                bool same = false;
                foreach (var vList in itemList)
                {
                    if (vList == item)
                    {
                        same = true;
                        break;
                    }
                }
                if(same)
                    continue;
               cbVehicel.Properties.Items.Add(item);
                itemList.Add(item);
            }
        }

        
        public void SetDataToUI(DataRow selectedRow)
        {
            throw new NotImplementedException();
        }

        public void Submit()
        {
            throw new NotImplementedException();
        }

        public void SwitchCtl(bool isSwitch)
        {
            throw new NotImplementedException();
        }
        
        private void cbVehicel_SelectedValueChanged(object sender, EventArgs e)
        {
            cbConfig.SelectedIndex = -1;
            cbStage.SelectedIndex = -1;
            cbRound.SelectedIndex = -1;
            cbType.SelectedIndex = -1;
            cbName.SelectedIndex = -1;
            if (cbVehicel.SelectedIndex == -1)
                return;
            cbConfig.Enabled = true;
            cbConfig.Properties.Items.Clear();
            Dictionary<string, object> dictNode = new Dictionary<string, object>();
            dictNode["VehicelType"] = cbVehicel.Text.Trim();
            dictNode["Condition"] = "VehicelType";
            IList<string> deptF = _store.GetSingnalColByCon(EnumLibrary.EnumTable.QuestionNoteByCondition, dictNode, 1);
           //把集合赋值给控件
            string curruntConfig = "";
            List<string> itemList = new List<string>();
            
            foreach (var item in deptF)
            {
                bool find = false;
                foreach (var iteml in itemList)
                {
                    if (item == iteml)
                    {
                        find = true;
                        break;
                    }
                }
                if(find)
                    continue;
                cbConfig.Properties.Items.Add(item);
                 itemList.Add(item);
           }
            Dictionary<string, object> dictNote = _draw.GetDataFromUI();
            dictNote["Condition"] = "VehicelType";
            //InitGridByCondition(dictNote);
            tsmiIdentify.Enabled = true;
        }

        private void cbConfig_SelectedValueChanged(object sender, EventArgs e)
        {
            cbStage.SelectedIndex = -1;
            cbRound.SelectedIndex = -1;
            cbType.SelectedIndex = -1;
            cbName.SelectedIndex = -1;
            if (cbConfig.SelectedIndex == -1)
                return;
            cbStage.Enabled = true;
            cbStage.Properties.Items.Clear();
            
            Dictionary<string, object> dictNode = new Dictionary<string, object>();
            dictNode["VehicelType"] = cbVehicel.Text.Trim();
            dictNode["VehicelConfig"] = cbConfig.Text.Trim();
            dictNode["Condition"] = "VehicelConfig";
           IList<string> deptF = _store.GetSingnalColByCon(EnumLibrary.EnumTable.QuestionNoteByCondition, dictNode, 2);
            
            //把集合赋值给控件
            string curruntStage = "";
            List<string> itemList = new List<string>();
            foreach (var item in deptF)
            {
                bool find = false;
                foreach (var iteml in itemList)
                {
                    if (item == iteml)
                    {
                        find = true;
                        break;
                    }
                }
                if (find)
                    continue;
               cbStage.Properties.Items.Add(item);
                itemList.Add(item);
            }
            Dictionary<string, object> dictNote = _draw.GetDataFromUI();
            dictNote["Condition"] = "VehicelConfig";
            //InitGridByCondition(dictNote);
            tsmiIdentify.Enabled = true;
        }

        private void cbStage_SelectedValueChanged(object sender, EventArgs e)
        {
            cbRound.SelectedIndex = -1;
            cbType.SelectedIndex = -1;
            cbName.SelectedIndex = -1;
            if (cbStage.SelectedIndex == -1)
                return;
            cbRound.Enabled = true;
            cbRound.Properties.Items.Clear();
            
            Dictionary<string, object> dictNode = new Dictionary<string, object>();
            dictNode["VehicelType"] = cbVehicel.Text.Trim();
            dictNode["VehicelConfig"] = cbConfig.Text.Trim();
            dictNode["VehicelStage"] = cbStage.Text.Trim();
            dictNode["Condition"] = "VehicelStage";
            IList<string> deptF = _store.GetSingnalColByCon(EnumLibrary.EnumTable.QuestionNoteByCondition, dictNode, 3);
            //把集合赋值给控件
            string curruntRound = "";
            List<string> itemList = new List<string>();
            foreach (var item in deptF)
            {
                bool find = false;
                foreach (var iteml in itemList)
                {
                    if (item == iteml)
                    {
                        find = true;
                        break;
                    }
                }
                if (find)
                    continue;
                cbRound.Properties.Items.Add(item);
                itemList.Add(item);
                
            }
            Dictionary<string, object> dictNote = _draw.GetDataFromUI();
            dictNote["Condition"] = "VehicelStage";
            //InitGridByCondition(dictNote);
            tsmiIdentify.Enabled = true;
        }

        private void cbRound_SelectedValueChanged(object sender, EventArgs e)
        {
            cbType.SelectedIndex = -1;
            cbName.SelectedIndex = -1;
            if (cbRound.SelectedIndex == -1)
                return;
            cbType.Enabled = true;
            cbType.Properties.Items.Clear();
            
            Dictionary<string, object> dictNode = new Dictionary<string, object>();
            dictNode["VehicelType"] = cbVehicel.Text.Trim();
            dictNode["VehicelConfig"] = cbConfig.Text.Trim();
            dictNode["VehicelStage"] = cbStage.Text.Trim();
            dictNode["TaskRound"] = cbRound.Text.Trim();
            dictNode["Condition"] = "TaskRound";
            IList<string> deptF = _store.GetSingnalColByCon(EnumLibrary.EnumTable.QuestionNoteByCondition, dictNode, 4);
           //把集合赋值给控件
            string curruntType = "";
            List<string> itemList = new List<string>();
            foreach (var item in deptF)
            {
                bool find = false;
                foreach (var iteml in itemList)
                {
                    if (item == iteml)
                    {
                        find = true;
                        break;
                    }
                }
                if (find)
                    continue;
                cbType.Properties.Items.Add(item);
                itemList.Add(item);
                
            }
            Dictionary<string, object> dictNote = _draw.GetDataFromUI();
            dictNote["Condition"] = "TaskRound";
            //InitGridByCondition(dictNote);
            tsmiIdentify.Enabled = true;

        }
        /// <summary>
        /// 根据上面控件的搜索条件填充GridView控件
        /// </summary>
        /// <param name="dictCon">搜索条件</param>
        private void InitGridByCondition(Dictionary<string, object> dictCon)
        {
            //从数据库中查询指定表的数据信息
            var failList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.QuestionNoteByCondition, dictCon);
            var passList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.PassReportNote, dictCon);
            //为gird添加列
            int countA = (passList.Count >= failList.Count) ? passList.Count : failList.Count;
            AddGridViewColumn(countA);
            
            var coList = new List<string>();
            foreach (GridColumn col in gvQuesNote.Columns)
                coList.Add(col.FieldName);
            var dt = new DataTable();
            foreach (var colName in coList)
                dt.Columns.Add(new DataColumn(colName, typeof(object)));
            //找到QuestionNoteByCondition和PassReportNote是在同一次测试时间并且同一个映射ID的找出来拼接起来或者，转成Json格式
            //再调用RevertDataToGridView转成datatable
            object[] objf = new object[8];
            object[] objp = new object[8];
            IList<object[]> faiList = new List<object[]>();
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

                
            //将拼接好的用例转化到GridView
            dt = RevertDataToGridView(faiList);
            //if (departmentList.Count > 0)
            //{
            //    //dt.Rows.Add(departmentList[departmentList.Count - 1]);
            //    int count = 0;
            //    foreach (var dept in departmentList)
            //    {
            //        if (count < departmentList.Count)
            //            dt.Rows.Add(dept);
            //        count++;
            //    }
            //}
            gcQuesNote.DataSource = dt;

            tsmiIdentify.Enabled = true;

        }

        /// <summary>
        /// 将几次测试相同映射ID拼接到一起，分为两种情况
        /// 相同映射ID和相同评价项目的数据拼接到同一行或者相同映射ID下新出现的评价项目新加一行
        /// 当其中某一次测试结果没有的时候用空表示，不能省略，函数返回一个datatable
        /// </summary>
        /// <param name="dtaList">要拼接的数据</param>
        /// <param name="grid">要显示的GridView名称</param>
        /// <returns>返回一个datatable</returns>
        private DataTable RevertDataToGridView(IList<object[]>  dtaList)
        {
            
            DataTable dt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvQuesNote.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList)
                dt.Columns.Add(new DataColumn(colName, typeof(object)));
            if (dtaList.Count != 0)
            {
                int colcount = gvQuesNote.Columns.Count ;
                object[] row = new object[colcount];
            //int i = 0;
            //if (dtaList.Count > 20)
            //{
            //    i = dtaList.Count - 20;
            //}
            //row[0] = dtaList[0][0];
            //row[1] = dtaList[0][1];
            //row[2] = dtaList[0][2];
            //row[3] = dtaList[0][3];
            //row[4] = dtaList[0][4];
            //row[5] = dtaList[0][5];

                Dictionary<string, List<Dictionary<string, string>>> assItemdict = new Dictionary<string, List<Dictionary<string, string>>>();
                assItemdict = Json.DerJsonDictLd(dtaList[0][6].ToString());
                int count = 0;
                List<Dictionary<string, string>> secondList = new DetailList<Dictionary<string, string>>();
                foreach (KeyValuePair<string, List<Dictionary<string, string>>> listitem in assItemdict)
                {
                    count = count + listitem.Value.Count;
                    foreach (var dictitem in listitem.Value)
                    {
                        row[0] = dictitem["ExapID"];
                        row[1] = dictitem["ExapName"];
                        row[2] = dictitem["AssessItem"];
                        row[3] = dictitem["DescriptionOfValue"];
                        row[4] = dictitem["MinValue"];
                        row[5] = dictitem["MaxValue"];
                        row[6] = dictitem["NormalValue"];
                        row[7] = dictitem["TestValue"];
                        row[8] = dictitem["Result"];
                        row[9] = dtaList[0][7];
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

                                    if (dictitem["ExapID"] == dictiteml["ExapID"] && dictitem["AssessItem"] == dictiteml["AssessItem"] )
                                    {
                                        int index = 9 + (j - 1)*3 + 1;
                                        row[index] = dictiteml["TestValue"];
                                        index ++;
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
                                int index = 9 + (j - 1) * 3 + 1;
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
                                     row[0] = dictiteml["ExapID"];
                                     row[1] = dictiteml["ExapName"];
                                     row[2] = dictiteml["AssessItem"];
                                     row[3] = dictiteml["DescriptionOfValue"];
                                     row[4] = dictiteml["MinValue"];
                                     row[5] = dictiteml["MaxValue"];
                                     row[6] = dictiteml["NormalValue"];
                                     row[7] = "";
                                     row[8] = "";
                                     row[9] = "";
                                     int index = 9 + (j - 1) * 3 + 1;
                                     row[index] = dictiteml["TestValue"];
                                     index++;
                                     row[index] = dictiteml["Result"];
                                     index++;
                                     row[index] = dtaList[j][7];
                                    //把其他几次测试中的该映射ID下的这个评价项目的数据拼接到一行
                                    for (int i =  1; i < dtaList.Count ; i++)
                                     {
                                        if(i == j)
                                            continue;
                                         Dictionary<string, List<Dictionary<string, string>>> assItemdictll =
                                             new Dictionary<string, List<Dictionary<string, string>>>();
                                         assItemdictll = Json.DerJsonDictLd(dtaList[i][6].ToString());
                                         foreach (KeyValuePair<string, List<Dictionary<string, string>>> listitemll in
                                             assItemdictll)
                                         {
                                             bool isame = false;
                                            Dictionary<string,string> dictsame = new Dictionary<string, string>();
                                             string nameId = "";
                                             foreach (var dictitem in listitemll.Value)
                                             {
                                                 nameId = dictitem["ExapID"];
                                                 if (dictitem["ExapID"] == dictiteml["ExapID"]&& dictitem["AssessItem"] == dictiteml["AssessItem"])
                                                 {
                                                     isame = true;
                                                     dictsame = dictitem;

                                                     break;
                                                     
                                                 }
                                             }
                                            //如果找到在相同映射ID下有相同评价项目的话，就把数据加入新行
                                            if (isame && dictsame.Count != 0 && dictiteml["ExapID"] == nameId)
                                             {
                                                 int indexl = 9 + (i - 1) * 3 + 1;
                                                 row[indexl] = dictsame["TestValue"];
                                                 indexl++;
                                                 row[indexl] = dictsame["Result"];
                                                 indexl++;
                                                 row[indexl] = dtaList[i][7];
                                                 
                                                 break;
                                             }
                                            // 否则在相同映射ID下没有找到相同评价项目的话，这次的测试数据就为空，加入新行
                                            else if (!isame && dictsame["ExapID"] == nameId)
                                             {
                                                int indexl = 9 + (i - 1) * 3 + 1;
                                                 row[indexl] = "";
                                                 indexl++;
                                                 row[indexl] = "";
                                                 indexl++;
                                                 row[indexl] = "";
                                                 break;
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
             
           }
           return dt;
        }
        /// <summary>
        /// 根据查询出的数据条数为相应的GridView动态添加列，每一条数据增加三列
        /// </summary>
        /// <param name="icol">数据条数</param>
        private void AddGridViewColumn(int icol)
        {
            for (int k = 10; gvQuesNote.Columns.Count >10; )
            {
                gvQuesNote.Columns.RemoveAt(k);
            }
            
            for (int i = 0; i < icol -1; i ++)
            {
                GridColumn colv = new GridColumn();
                colv.Caption = @"第" + (i + 2) +@"次测试值";
                colv.Name = "value" + i;
                colv.FieldName = "value" + i;
                colv.Visible = true;

                GridColumn colr = new GridColumn();
                colr.Caption = @"第" + (i + 2)+ @"次测试结果";
                colr.Name = "result" + i;
                colr.FieldName = "result" + i;
                colr.Visible = true;

                GridColumn colt = new GridColumn();
                colt.Caption = @"第" + (i + 2) + @"次测试时间";
                
                colt.Name = "time" + i;
                colt.FieldName = "time" + i;
                colt.Visible = true;

                gvQuesNote.Columns.AddRange(new GridColumn[] { colv,colr,colt });

            }
            isfirstAddCol = true;
        }

        private void cbType_SelectedValueChanged(object sender, EventArgs e)
        {
            cbName.SelectedIndex = -1;
            if (cbType.SelectedIndex == -1)
                return;
            cbName.Enabled = true;
            cbName.Properties.Items.Clear();
            
            Dictionary<string, object> dictNode = new Dictionary<string, object>();
            dictNode["VehicelType"] = cbVehicel.Text.Trim();
            dictNode["VehicelConfig"] = cbConfig.Text.Trim();
            dictNode["VehicelStage"] = cbStage.Text.Trim();
            dictNode["TaskRound"] = cbRound.Text.Trim();
            dictNode["TestType"] = cbType.Text.Trim();
            dictNode["Condition"] = "TestType";
           IList<string> deptF = _store.GetSingnalColByCon(EnumLibrary.EnumTable.QuestionNoteByCondition, dictNode, 5);
           //把集合赋值给控件
            string curruntName = "";
            List<string> itemList = new List<string>();
            foreach (var item in deptF)
            {
                bool find = false;
                foreach (var iteml in itemList)
                {
                    if (item == iteml)
                    {
                        find = true;
                        break;
                    }
                }
                if (find)
                    continue;
                cbName.Properties.Items.Add(item);
                itemList.Add(item);
            }
            Dictionary<string, object> dictNote = _draw.GetDataFromUI();
            dictNote["Condition"] = "TestType";

            //InitGridByCondition(dictNote);

            tsmiIdentify.Enabled = true;
            
        }

        private void cbName_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbName.SelectedIndex == -1)
                return;
            Dictionary<string, object> dictNote = _draw.GetDataFromUI();
            dictNote["Condition"] = "Module";
            InitGridByCondition(dictNote);

            //var coList = new List<string>();
            //foreach (GridColumn col in gvQuesNote.Columns)
            //    coList.Add(col.FieldName);
            //var dt = new DataTable();
            //foreach (var colName in coList)
            //    dt.Columns.Add(new DataColumn(colName, typeof(object)));
            ////从数据库中查询指定表的数据信息
            //var departmentList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.QuestionNoteSort, dictNote);
            //if (departmentList.Count > 0)
            //{
            //   int count = 0;
            //    foreach (var dept in departmentList)
            //    {
            //        if (count < departmentList.Count)
            //            dt.Rows.Add(dept);
            //        count++;
            //    }
            //}
            //gcQuesNote.DataSource = dt;

            tsmiIdentify.Enabled = true;
        }


        private void btnSort_Click(object sender, EventArgs e)
        {
            //Dictionary<string,object> dictNote = _draw.GetDataFromUI();
            //var coList = new List<string>();
            //foreach (GridColumn col in gvQuesNote.Columns)
            //    coList.Add(col.FieldName);
            //var dt = new DataTable();
            //foreach (var colName in coList)
            //    dt.Columns.Add(new DataColumn(colName, typeof(object)));
            ////从数据库中查询指定表的数据信息
            //var departmentList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.QuestionNoteSort,dictNote);
            //if (departmentList.Count > 0)
            //{
            //    //dt.Rows.Add(departmentList[departmentList.Count - 1]);
            //    int count = 0;
            //    foreach (var dept in departmentList)
            //    {
            //        if (count < departmentList.Count)
            //            dt.Rows.Add(dept);
            //        count++;
            //    }
            //}
            var coList = new List<string>();
            foreach (GridColumn col in gvQuesNote.Columns)
                coList.Add(col.FieldName);
            //从数据库中查询指定数据
            var dt = _show.DrawDtFromQuestionNote(coList.ToArray(), EnumLibrary.EnumTable.QuestionNote);
            //将数据源赋值给控件
            gcQuesNote.DataSource = dt;
            //ClearUI();
            SetControlEnable();
        }


        private void SetControlEnable()
        {
            cbConfig.Enabled = false;
            cbStage.Enabled = false;
            cbRound.Enabled = false;
            cbType.Enabled = false;
            cbName.Enabled = false;
            //btnSort.Enabled = false;
        }
        /// <summary>
        /// 验证通过功能，即将与选中行前六项相同的并且映射ID也与选中行相同的数据从
        /// 数据库表QuestionNoteByCondition和PassReportNote中的FailItemInfo列的数据中的删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmiIdentify_Click(object sender, EventArgs e)
        {
            if (_dr == null)
                return;
            Dictionary<string, object> dictNote = _draw.GetDataFromUI();
            string exapID = _dr["ExapID"].ToString();
            dictNote["Condition"] = "Module";
            var departmentList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.QuestionNoteByCondition, dictNote);
            var passalist = _store.GetSpecialByEnum(EnumLibrary.EnumTable.PassReportNote, dictNote);
            foreach (var list in departmentList)
            {
                Dictionary<string, List<Dictionary<string, string>>> assItemdict = new Dictionary<string, List<Dictionary<string, string>>>();
                Dictionary<string, List<Dictionary<string, string>>> assItemdictn = new Dictionary<string, List<Dictionary<string, string>>>();
                assItemdict = Json.DerJsonDictLd(list[6].ToString());
                assItemdictn = Json.DerJsonDictLd(list[6].ToString());
                //把QuestionNoteByCondition中所有测试中出现的这个映射ID的数据移除
                foreach (KeyValuePair<string, List<Dictionary<string, string>>> dict in assItemdictn)
                {
                    if (dict.Key == exapID)
                    {
                        assItemdict.Remove(exapID);
                    }
                }
                string error = "";
                dictNote["TestTime"] = DateTime.Parse(list[7].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                dictNote["FailItemInfo"] = Json.SerJson(assItemdict);
                _store.Update(EnumLibrary.EnumTable.QuestionNote, dictNote,out error);
            }
            foreach (var list in passalist)
            {
                Dictionary<string, List<Dictionary<string, string>>> assItemdict = new Dictionary<string, List<Dictionary<string, string>>>();
                Dictionary<string, List<Dictionary<string, string>>> assItemdictn = new Dictionary<string, List<Dictionary<string, string>>>();
                assItemdict = Json.DerJsonDictLd(list[6].ToString());
                assItemdictn = Json.DerJsonDictLd(list[6].ToString());
                //把PassReportNote中所有测试中出现的这个映射ID的数据移除
                foreach (KeyValuePair<string, List<Dictionary<string, string>>> dict in assItemdictn)
                {
                    if (dict.Key == exapID)
                    {
                        assItemdict.Remove(exapID);
                    }
                }
                string error = "";
                dictNote["TestTime"] = DateTime.Parse(list[7].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                dictNote["FailItemInfo"] = Json.SerJson(assItemdict);
                _store.Update(EnumLibrary.EnumTable.PassReportNote, dictNote, out error);
            }
            //string error;
            //_store.Del(EnumLibrary.EnumTable.QuestionNote, dictNote, out error);
            //if (error == "")
            {
                
                SetControlEnable();
                InitGridByCondition(dictNote);
                XtraMessageBox.Show("认证通过...");
            }
        }

        private void gvQuesNote_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //获得光标位置
                var hi = gvQuesNote.CalcHitInfo(e.Location);
                //判断位置是否在行位置上
                if (!hi.InRow && !hi.InRowCell) return;
                if (hi.RowHandle < 0) return;
                //取一行值
                gvQuesNote.SelectRow(hi.RowHandle);
                _dr = gvQuesNote.GetDataRow(hi.RowHandle);
            }
        }
    }
}
