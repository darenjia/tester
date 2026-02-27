using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CANoeEngine;
using DevExpress.LookAndFeel;
using DevExpress.XtraCharts.GLGraphics;
using DevExpress.XtraEditors;
using EnvDTE;
using ProcessEngine;
using Thread = System.Threading.Thread;

namespace UltraANetT.Form
{
    public partial class Manual : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private string _fileName = "";
        private readonly ProcCANoeTest _cANoeTest = new ProcCANoeTest();
        private readonly Dictionary<string, string> _dictManual = new Dictionary<string, string>();
        private readonly Dictionary<string, object> _dictReport = new Dictionary<string, object>();
        private readonly List<string> _rltIdCopy = new List<string>();
        private readonly Dictionary<string, List<Dictionary<string, string>>> _dictExmpCopy;
        private int index = 0;
        public Manual(List<string> rltId,Dictionary<string, List<Dictionary<string, string>>> dictExmp)
        {
            InitializeComponent();
            foreach (var id in rltId)
                _rltIdCopy.Add(id);
            _dictExmpCopy = dictExmp;
            lblExapID.Caption = rltId[0];
            DrawDataItem(dictExmp[rltId[0]]);
            InitDict();
        }
       

        private void DrawDataItem( List<Dictionary<string, string>> dictExmp)
        {  
            DataTable dt = new DataTable();
            dt = GetDataCol(dt);
            foreach (var item in dictExmp)
            {
                    DataRow dr = dt.NewRow();
                    dr["AssessItem"] = item["AssessItem"];
                    dr["MinValue"] = item["MinValue"];
                    dr["NormalValue"] = item["NormalValue"];
                    dr["MaxValue"] = item["MaxValue"];
                    dr["Description"] = "等待填写...";
                    dr["Result"] = "";
                    dt.Rows.Add(dr);
            }
            gcData.DataSource = dt;
        }

        private DataTable GetDataCol(DataTable dt)
        {
            string[] colNames = new[] { "AssessItem", "MinValue", "NormalValue", "MaxValue", "Description", "Result" };
            foreach (var colName in colNames)
            {
                dt.Columns.Add(new DataColumn(colName, typeof(object)));
            }
            return dt;
        }

        private void GetRecordByTime(string path,float timeStart,float timeEnd)
        {
            List<string> listRecord = new List<string>();
            bool isStart = false;
            if (!togRecord.IsOn)
            {
                Show(DLAF.LookAndFeel, this, "未导入记录文件...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                return;
                
            }
            Stream instream = File.OpenRead(path);
            StreamReader sr = new StreamReader(instream);
            while (!sr.EndOfStream)
            {
                if (isStart)
                {
                    string recordStr = sr.ReadLine();
                    if (recordStr != null)
                    {
                        if (recordStr.Contains("End")) break;
                        string r = recordStr.Trim().Remove(8);
                        float timeFlag = float.Parse(r);
                        if (!(timeFlag >= timeStart) || !(timeFlag <= timeEnd)) continue;
                        listRecord.Add(recordStr);
                    }
                }
                else
                {
                    var readLine = sr.ReadLine();
                    if (readLine != null && readLine.Contains("Begin"))
                        isStart = true;
                }
            }
            foreach (var record in listRecord)
            {
                txtRecord.Text += record + "\r\n";
            }
            
        }


        private void btnSelect_Click(object sender, EventArgs e)
        {
            txtRecord.Text = "";
            int timeStart,timeEnd;
            if (!int.TryParse(txtTimeStart.Text, out timeStart)) return;
            if (!int.TryParse(txtTimeEnd.Text, out timeEnd)) return;
            GetRecordByTime(_fileName, timeStart, timeEnd);
        }

        private void btOpenDir_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", "/n, " + AppDomain.CurrentDomain.BaseDirectory + "screenshot");
        }

        private void btImportImg_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            OpenFileDialog ofg = new OpenFileDialog();
            if (ofg.ShowDialog() != DialogResult.OK) return;
            string filePath = ofg.FileName;
            _dictManual["OgScreenPath"] = filePath;
            Show(DLAF.LookAndFeel, this, "导入成功...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
        }

        private void btImportRecord_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _fileName = ofd.FileName;
                    togRecord.IsOn = true;
                }        
            }
            catch (Exception ex)
            {
                Show(DLAF.LookAndFeel, this, "文件导入异常...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                throw;
            }
            
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", "/n, " + AppDomain.CurrentDomain.BaseDirectory + "filerecord");
        }



        public static DialogResult Show(UserLookAndFeel look, IWin32Window owner, string text, string caption, DialogResult[] buttons, Icon icon, int defaultButton, MessageBoxIcon messageBeepSound)
        {
            
            XtraMessageBoxForm form = new XtraMessageBoxForm();
            Font defaultFont = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            form.Appearance.Font = defaultFont;
            return form.ShowMessageBoxDialog(new XtraMessageBoxArgs(look, owner, text, caption, buttons, icon, defaultButton));
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            GetValueFromRow();
            _dictManual["Log"] = txtRecord.Text;
            GlobalVar.DictManualReport = _dictManual;
            if (index < _rltIdCopy.Count)
            {
                index++;
                Reset(_rltIdCopy[index]);
                Show(DLAF.LookAndFeel, this, index + "项结果已经提交，还剩 " + (_rltIdCopy.Count - index) + "项手动测试", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
            }
            else
            {
                Show(DLAF.LookAndFeel, this, "手动测试已经完成", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            
        
        }

        private void btGetScreen_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            try
            {
                Thread trd = new Thread(WaitSceenResult);
                trd.Start();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
            
        }

        private void WaitSceenResult()
        {
            while (true)
            {
                if (_cANoeTest.IsEndScreen)
                {
                    _cANoeTest.IsEndScreen = false;
                    MessageBox.Show(@"示波器已经成功截图...");
                }
            }
        }

        private void InitDict()
        {
           
            _dictManual.Add("OgScreenPath","");
            _dictManual.Add("Log", "");
            _dictManual.Add("ItemVaule", "");
        }

        private void GetValueFromRow()
        {
            Dictionary<string, List<object>> dictValue = new Dictionary<string, List<object>>();
          

            for (int i = 0; i < gvData.RowCount; i++)
            {
                DataRow dr = gvData.GetDataRow(i);
                List<object> subVaule = new List<object>();
                for (int j = 0; j < dr.ItemArray.Length; j++)
                {
                    subVaule.Add(dr.ItemArray[j].ToString());
                }
                dictValue.Add(dr.ItemArray[0].ToString(), subVaule);
            }
            string manualReport = Json.SerJson(dictValue);
            _dictManual["ItemVaule"] = manualReport;
        }

        private void Reset(string rlt)
        {
            lblExapID.Caption = rlt;
            DrawDataItem(_dictExmpCopy[rlt]);
            togRecord.IsOn = false;
        }
    }
}