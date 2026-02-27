using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using ProcessEngine;
using System.Linq;
using DevExpress.XtraNavBar;
using System.Drawing;
using UltraANetT.Interface;

namespace UltraANetT.Module
{
    public partial class ReportList : XtraUserControl, IDraw
    {
        #region 设置变量

        private readonly ProcShow _show = new ProcShow();

        private readonly ProcStore _store = new ProcStore();
        
        private readonly IDraw _draw;

        private DataTable _dt;

        private Dictionary<string, List<string>> TypeDict;

        #endregion

        public ReportList()
        {
            InitializeComponent();
            _draw = this;
            _draw.InitGrid();
            
            //_draw.InitDict();
        }

        void IDraw.InitGrid()
        {
            cmbVType.Properties.Items.Clear();
            cmbStandardCalculationMethods.Properties.Items.Clear();
            var coList = new List<string>();
            foreach (GridColumn col in gvTestScore.Columns)
                coList.Add(col.FieldName);
            _dt = _show.DrawDtFromMultiple(coList.ToArray(), EnumLibrary.EnumTable.Peportlnfo);

            int j = 0;
            foreach (var col in _dt.Columns)
            {
                int count = 0; var carriedOut = false;
                //var dtData = new DataSet();
                List<string> myList = new List<string>();
                List<string> nonDuplicateList3 = new List<string>();
                if (Convert.ToString(col) == "VehicelType" || Convert.ToString(col) == "TestUser")
                {
                    count = j;
                    carriedOut = true;
                }
                if (carriedOut)
                {
                    for (int x = 0; x < _dt.Rows.Count; x++)
                    {
                        string VehicelType = Convert.ToString(_dt.Rows[x][count]);
                        if (VehicelType != "")
                        {
                            myList.Add(VehicelType.ToString().Trim());

                        }
                    }
                    myList = myList.Distinct().ToList();
                    for (int k = 0; k < myList.Count; k++)
                    {
                        if (Convert.ToString(col) == "VehicelType")
                        {
                            cmbVType.Properties.Items.Add(myList[k]);
                        }
                        else if (Convert.ToString(col) == "TestUser")
                        {
                            cmbStandardCalculationMethods.Properties.Items.Add(myList[k]);
                        }
                    }
                }
                j++;
            }
            gcTestScore.DataSource = _dt;

            //List<string> ListVType = new List<string>();
            //var coList = new List<string>();
            //foreach (GridColumn col in gvTestScore.Columns)
            //    coList.Add(col.FieldName);
            //_dt = new DataTable();
            //foreach (var colName in coList)
            //    _dt.Columns.Add(new DataColumn(colName, typeof(object)));

            //var TestScoreList = _store.GetRegularByEnum(EnumLibrary.EnumTable.TestRecord);
            //var ImportScoreList = _store.GetRegularByEnum(EnumLibrary.EnumTable.ImportScore);
            //foreach (var Tlist in TestScoreList)
            //{
            //    var dictdictScore = Json.DerJsonToDictDict(Tlist[7].ToString());
            //    Tlist[7] = ScoreCalculationMethods(dictdictScore);
            //    if (!ListVType.Contains(Tlist[1].ToString()))
            //        ListVType.Add(Tlist[1].ToString());
            //    _dt.Rows.Add(Tlist);
            //}
            //foreach (var Ilist in ImportScoreList)
            //{
            //    var dictdictScore = Json.DerJsonToDictDict(Ilist[7].ToString());
            //    Ilist[7] = ScoreCalculationMethods(dictdictScore);
            //    if (!ListVType.Contains(Ilist[1].ToString()))
            //        ListVType.Add(Ilist[1].ToString());
            //    _dt.Rows.Add(Ilist);
            //}
            //InitTask();
            ////将数据源赋值给控件
            //cmbVType.Properties.Items.Clear();
            //cmbVType.Properties.Items.AddRange(ListVType);
            //_dt = DtScoreDesc(_dt);
            //gcTestScore.DataSource = _dt;
        }

        /// <summary>
        /// 对Score字段根据选项进行计算
        /// </summary>
        /// <param name="dictdictScore"></param>
        /// <returns></returns>
        private decimal ScoreCalculationMethods(Dictionary<string, Dictionary<string, string>> dictdictScore)
        {
            string strTaskCalculationMethods = cmbTaskCalculationMethods.EditValue.ToString();
            string strStandardCalculationMethods = cmbStandardCalculationMethods.EditValue.ToString();
            
            List<decimal> ListdsScore = new List<decimal>();
            foreach (var dictScore in dictdictScore)
            {
                int i = 0;
                decimal dsScore = 0;
                decimal dsMinScore = 0;
                decimal dsMaxScore = 0;
                foreach (var Score in dictScore.Value)
                {
                    if (i == 0)
                    {
                        dsScore = decimal.Parse(Score.Value);
                        dsMinScore = decimal.Parse(Score.Value);
                        dsMaxScore = decimal.Parse(Score.Value);
                    }
                    else
                    {
                        if (dsMaxScore < decimal.Parse(Score.Value))
                            dsMaxScore = decimal.Parse(Score.Value);
                        if (dsMinScore > decimal.Parse(Score.Value))
                            dsMinScore = decimal.Parse(Score.Value);
                        dsScore += decimal.Parse(Score.Value);
                    }
                    i++;
                }
                switch (strStandardCalculationMethods)
                {
                    case "最大值":
                        dsScore = dsMaxScore;
                        break;
                    case "最小值":
                        dsScore = dsMinScore;
                        break;
                    case "平均值":
                        dsScore = dsScore/dictScore.Value.Count;
                        break;
                }
                ListdsScore.Add(dsScore);
            }
            decimal dtScore = 0;
            decimal dtMinScore = 0;
            decimal dtMaxScore = 0;
            int n = 0;
            foreach (var dScore in ListdsScore)
            {
                if (n == 0)
                {
                    dtScore = dScore;
                    dtMinScore = dScore;
                    dtMaxScore = dScore;
                }
                else
                {
                    if (dtMaxScore < dScore)
                        dtMaxScore = dScore;
                    if (dtMinScore > dScore)
                        dtMinScore = dScore;
                    dtScore += dScore;
                }
                n++;
            }
            decimal dLastScore = 0;
            switch (strTaskCalculationMethods)
            {
                case "最大值":
                    dLastScore = dtMaxScore;
                    break;
                case "最小值":
                    dLastScore = dtMinScore;
                    break;
                case "平均值":
                    dLastScore = dtScore / ListdsScore.Count;
                    break;
            }
            return dLastScore;
        }

        /// <summary>
        /// 对DataTable以Score字段进行排序，并把序号复制到Index字段
        /// </summary>
        /// <param name="dt">排序前的DataTable</param>
        /// <returns>排序后的DataTable</returns>
        private DataTable DtScoreDesc(DataTable dt)
        {
            DataTable dtNew = dt.Copy();
            //DataTable dtTemp = dtNew.Clone();
            //var drArr = dtNew.Select("1=1", "Score DESC");
            //decimal lastIndex = 1;
            //decimal lastScore = 0;
            //for (int i = 0; i < drArr.Length; i++)
            //{
            //    if (i == 0)
            //    {
            //        lastScore = decimal.Parse(drArr[i]["Score"].ToString());
            //        drArr[i]["Index"] = (decimal)1;
            //    }
            //    else
            //    {
            //        if (lastScore.ToString() == drArr[i]["Score"].ToString())
            //        {
            //            drArr[i]["Index"] = (decimal)lastIndex;
            //        }
            //        else
            //        {
            //            lastIndex = i + 1;
            //            drArr[i]["Index"] = (decimal)i + (decimal)1;
            //            lastScore = decimal.Parse(drArr[i]["Score"].ToString());
            //        }
            //    }
            //    dtTemp.ImportRow(drArr[i]);
            //}
            //dtNew = dtTemp.Copy();
            return dtNew;
        }

        private void InitTask()
        {
            //cmbTask.Properties.Items.Clear();
            //var EvaItemsList = _store.GetRegularByEnum(EnumLibrary.EnumTable.EvaItems);
            //string JsonStr = EvaItemsList.Count > 0 ? EvaItemsList[0][1].ToString() : string.Empty;
            //var JsonDict = string.IsNullOrEmpty(JsonStr) ? new Dictionary<string, List<Dictionary<string, string>>>() : Json.DerJsonToDictList(JsonStr);
            //TypeDict = new Dictionary<string, List<string>>();
            //foreach (KeyValuePair<string, List<Dictionary<string, string>>> ListDict in JsonDict)
            //{
            //    string TypeName = "";
            //    List<string> NormList = new List<string>();
            //    foreach (Dictionary<string, string> Dict in ListDict.Value)
            //    {
            //        TypeName = Dict["TestType"];
            //        NormList.Add(Dict["NormName"]);
            //    }
            //    if (TypeName != "")
            //    {
            //        TypeDict.Add(TypeName, NormList);
            //        cmbTask.Properties.Items.Add(TypeName);
            //    }
            //}
        }

        private void ClearAll()
        {
            cmbVType.SelectedIndex = -1;
            cmbVConfig.SelectedIndex = -1;
            cmbVStage.SelectedIndex = -1;
            cmbRound.SelectedIndex = -1;
            cmbTask.SelectedIndex = -1;
            cmbStandard.SelectedIndex = -1;
            cmbTaskCalculationMethods.SelectedIndex = -1;
            cmbStandardCalculationMethods.SelectedIndex = -1;
            deStart.Text = null;
            deEnd.Text = null;
        }
        void IDraw.InitDict()
        {

        }

        Dictionary<string, object> IDraw.GetDataFromUI()
        {
            throw new NotImplementedException();
        }

        void IDraw.SetDataToUI(DataRow selectedRow)
        {
            throw new NotImplementedException();
        }

        void IDraw.SwitchCtl(bool isSwitch)
        {
            throw new NotImplementedException();
        }

        void IDraw.Submit()
        {
            DataTable dtNew = _dt.Copy();

            if (cmbVType.SelectedIndex != -1)
            {
                DataTable dtTemp = dtNew.Clone();
                var drArr = dtNew.Select("VehicelType='" + cmbVType.EditValue + "'", "TaskNo ASC");
                for (int i = 0; i < drArr.Length; i++)
                {
                    dtTemp.ImportRow(drArr[i]);
                }
                dtNew = dtTemp.Copy();
            }
            if (cmbVConfig.SelectedIndex != -1)
            {
                DataTable dtTemp = dtNew.Clone();
                var drArr = dtNew.Select("VehicelType='" + cmbVType.EditValue + "'" + "and VehicelConfig='" + cmbVConfig.EditValue + "'", "TaskNo ASC");
                for (int i = 0; i < drArr.Length; i++)
                {
                    dtTemp.ImportRow(drArr[i]);
                }
                dtNew = dtTemp.Copy();
            }
            if (cmbVStage.SelectedIndex != -1)
            {
                DataTable dtTemp = dtNew.Clone();
                var drArr = dtNew.Select("VehicelType='" + cmbVType.EditValue + "'" + "and VehicelConfig='" + cmbVConfig.EditValue + "'" + "and VehicelStage='" + cmbVStage.EditValue + "'", "TaskNo ASC");
                for (int i = 0; i < drArr.Length; i++)
                {
                    dtTemp.ImportRow(drArr[i]);
                }
                dtNew = dtTemp.Copy();
            }
            if (cmbRound.SelectedIndex != -1)
            {
                DataTable dtTemp = dtNew.Clone();
                var drArr = dtNew.Select("VehicelType='" + cmbVType.EditValue + "'" + "and VehicelConfig='" + cmbVConfig.EditValue + "'" + "and VehicelStage='" + cmbVStage.EditValue + "'" + "and TaskRound='" + cmbRound.EditValue + "'", "TaskNo ASC");
                for (int i = 0; i < drArr.Length; i++)
                {
                    dtTemp.ImportRow(drArr[i]);
                }
                dtNew = dtTemp.Copy();
            }
            if (cmbTask.SelectedIndex != -1)
            {
                DataTable dtTemp = dtNew.Clone();
                var drArr = dtNew.Select("TaskName='" + cmbTask.EditValue + "'", "TaskNo ASC");
                for (int i = 0; i < drArr.Length; i++)
                {
                    dtTemp.ImportRow(drArr[i]);
                }
                dtNew = dtTemp.Copy();
            }
            if (cmbStandard.SelectedIndex != -1)
            {
                DataTable dtTemp = dtNew.Clone();
                var drArr = dtNew.Select("CANRoad='" + cmbStandard.EditValue + "'", "TaskNo ASC");
                for (int i = 0; i < drArr.Length; i++)
                {
                    dtTemp.ImportRow(drArr[i]);
                }
                dtNew = dtTemp.Copy();
            }
            if (cmbTaskCalculationMethods.SelectedIndex != -1)
            {
                DataTable dtTemp = dtNew.Clone();
                var drArr = dtNew.Select("Module='" + cmbTaskCalculationMethods.EditValue + "'", "TaskNo ASC");
                for (int i = 0; i < drArr.Length; i++)
                {
                    dtTemp.ImportRow(drArr[i]);
                }
                dtNew = dtTemp.Copy();
            }
            if (cmbStandardCalculationMethods.SelectedIndex != -1)
            {
                DataTable dtTemp = dtNew.Clone();
                var drArr = dtNew.Select("TestUser='" + cmbStandardCalculationMethods.EditValue + "'", "TaskNo ASC");
                for (int i = 0; i < drArr.Length; i++)
                {
                    dtTemp.ImportRow(drArr[i]);
                }
                dtNew = dtTemp.Copy();
            }
            if (deStart.EditValue != null)
            {
                DataTable dtTemp = dtNew.Clone();
                var drArr = dtNew.Select("TestTime >= '" + deStart.DateTime + "'", "TaskNo ASC");
                for (int i = 0; i < drArr.Length; i++)
                {
                    dtTemp.ImportRow(drArr[i]);
                }
                dtNew = dtTemp.Copy();
            }
            if (deEnd.EditValue != null)
            {
                DataTable dtTemp = dtNew.Clone();
                var drArr = dtNew.Select("TestTime <= '" + deEnd.DateTime.AddDays(1).AddSeconds(-1) + "'", "TaskNo ASC");
                for (int i = 0; i < drArr.Length; i++)
                {
                    dtTemp.ImportRow(drArr[i]);
                }
                dtNew = dtTemp.Copy();
            }
            gcTestScore.DataSource = DtScoreDesc(dtNew);
        }

        private void cmbVType_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbVConfig.SelectedIndex = -1;
            cmbVStage.SelectedIndex = -1;
            cmbRound.SelectedIndex = -1;
            cmbVConfig.Properties.Items.Clear();
            cmbVStage.Properties.Items.Clear();
            cmbRound.Properties.Items.Clear();
            if (cmbVType.SelectedIndex == -1)
                return;
            List<string> ListVConfig=new List<string>();
            var drArr=_dt.Select("VehicelType='" + cmbVType.EditValue+"'");
            for (int i = 0; i < drArr.Length; i++)
            {
                if(!ListVConfig.Contains(drArr[i]["VehicelConfig"].ToString()))
                    ListVConfig.Add(drArr[i]["VehicelConfig"].ToString());
            }
            cmbVConfig.Properties.Items.AddRange(ListVConfig);
        }

        private void cmbVConfig_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbVStage.SelectedIndex = -1;
            cmbRound.SelectedIndex = -1; 
            cmbVStage.Properties.Items.Clear();
            cmbRound.Properties.Items.Clear();
            if (cmbVConfig.SelectedIndex == -1)
                return;
            List<string> ListVStage = new List<string>();
            var drArr = _dt.Select("VehicelType='" + cmbVType.EditValue + "'" + "and VehicelConfig='" + cmbVConfig.EditValue + "'");
            DataTable dtNew = _dt.Clone();
            for (int i = 0; i < drArr.Length; i++)
            {
                if (!ListVStage.Contains(drArr[i]["VehicelStage"].ToString()))
                    ListVStage.Add(drArr[i]["VehicelStage"].ToString());
                dtNew.ImportRow(drArr[i]);
            }
            cmbVStage.Properties.Items.AddRange(ListVStage);
        }

        private void cmbVStage_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbRound.SelectedIndex = -1;
            cmbRound.Properties.Items.Clear();
            if (cmbVStage.SelectedIndex == -1)
                return;
            List<string> ListRound = new List<string>();
            var drArr = _dt.Select("VehicelType='" + cmbVType.EditValue + "'" + "and VehicelConfig='" + cmbVConfig.EditValue + "'" + "and VehicelStage='" + cmbVStage.EditValue + "'");
            DataTable dtNew = _dt.Clone();
            for (int i = 0; i < drArr.Length; i++)
            {
                if (!ListRound.Contains(drArr[i]["TaskRound"].ToString()))
                    ListRound.Add(drArr[i]["TaskRound"].ToString());
                dtNew.ImportRow(drArr[i]);
            }
            cmbRound.Properties.Items.AddRange(ListRound);
        }

        private void sbtnClearAll_Click(object sender, EventArgs e)
        {
            ClearAll();
            _draw.InitGrid();
        }

        private void sbtnSelect_Click(object sender, EventArgs e)
        {
            //_draw.InitGrid();
            _draw.Submit();
        }

        private void cmbVConfig_MouseUp(object sender, MouseEventArgs e)
        {
            if (cmbVType.SelectedIndex == -1)
            {
                cmbVConfig.Properties.Items.Clear();
            }
        }
        private void cmbVStage_MouseUp(object sender, MouseEventArgs e)
        {
           
            if (cmbVConfig.SelectedIndex == -1)
            {
                cmbVStage.Properties.Items.Clear();
            }
        }

        private void cmbRound_MouseUp(object sender, MouseEventArgs e)
        {
            if (cmbVStage.SelectedIndex == -1)
            {
                cmbRound.Properties.Items.Clear();
            }
        }

        private void cmbTask_MouseUp(object sender, MouseEventArgs e)
        {
            if (cmbRound.SelectedIndex == -1)
            {
                cmbTask.Properties.Items.Clear();
            }
        }

        private void cmbStandard_MouseUp(object sender, MouseEventArgs e)
        {
            if (cmbTask.SelectedIndex == -1)
            {
                cmbStandard.Properties.Items.Clear();
            }
        }

        private void cmbTaskCalculationMethods_MouseUp(object sender, MouseEventArgs e)
        {
            if (cmbStandard.SelectedIndex == -1)
            {
                cmbTaskCalculationMethods.Properties.Items.Clear();
            }
        }

        private void cmbRound_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbTask.SelectedIndex = -1;
            cmbTask.Properties.Items.Clear();
            if (cmbRound.SelectedIndex == -1)
                return;
            List<string> ListRound = new List<string>();
            var drArr = _dt.Select("VehicelType='" + cmbVType.EditValue + "'" + "and VehicelConfig='" + cmbVConfig.EditValue + "'" + "and VehicelStage='" + cmbVStage.EditValue + "'" + "and TaskRound='" + cmbRound.EditValue + "'");
            DataTable dtNew = _dt.Clone();
            for (int i = 0; i < drArr.Length; i++)
            {
                if (!ListRound.Contains(drArr[i]["TaskName"].ToString()))
                    ListRound.Add(drArr[i]["TaskName"].ToString());
                dtNew.ImportRow(drArr[i]);
            }
            cmbTask.Properties.Items.AddRange(ListRound);
        }

        private void cmbTask_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbStandard.SelectedIndex = -1;
            cmbStandard.Properties.Items.Clear();
            if (cmbTask.SelectedIndex == -1)
                return;
            List<string> ListRound = new List<string>();
            var drArr = _dt.Select("VehicelType='" + cmbVType.EditValue + "'" + "and VehicelConfig='" + cmbVConfig.EditValue + "'" + "and VehicelStage='" 
                + cmbVStage.EditValue + "'" + "and TaskRound='" + cmbRound.EditValue + "'" + "and TaskName='" + cmbTask.EditValue + "'");
            DataTable dtNew = _dt.Clone();
            for (int i = 0; i < drArr.Length; i++)
            {
                if (!ListRound.Contains(drArr[i]["CANRoad"].ToString()))
                    ListRound.Add(drArr[i]["CANRoad"].ToString());
                dtNew.ImportRow(drArr[i]);
            }
            cmbStandard.Properties.Items.AddRange(ListRound);
        }
        private void cmbStandard_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbTaskCalculationMethods.SelectedIndex = -1;
            cmbTaskCalculationMethods.Properties.Items.Clear();
            if (cmbStandard.SelectedIndex == -1)
                return;
            List<string> ListRound = new List<string>();
            var drArr = _dt.Select("VehicelType='" + cmbVType.EditValue + "'" + "and VehicelConfig='" + cmbVConfig.EditValue + "'" + "and VehicelStage='"
                + cmbVStage.EditValue + "'" + "and TaskRound='" + cmbRound.EditValue + "'" + "and TaskName='" + cmbTask.EditValue + "'" + "and CANRoad='" + cmbStandard.EditValue + "'");
            DataTable dtNew = _dt.Clone();
            for (int i = 0; i < drArr.Length; i++)
            {
                if (!ListRound.Contains(drArr[i]["Module"].ToString()))
                    ListRound.Add(drArr[i]["Module"].ToString());
                dtNew.ImportRow(drArr[i]);
            }
            cmbTaskCalculationMethods.Properties.Items.AddRange(ListRound);
        }

       
    }
}
