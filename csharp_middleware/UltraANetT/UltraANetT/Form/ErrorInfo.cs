using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ProcessEngine;
using UltraANetT.Interface;
using System.Drawing.Imaging;
using DevExpress.LookAndFeel;
using Model;

namespace UltraANetT.Form
{
    public partial class ErrorInfo : DevExpress.XtraBars.Ribbon.RibbonForm 
    {
        private ProcStore _store;
        
        private readonly IDraw _draw;
        private ProcLog Log = new ProcLog();
        ProcFile file = new ProcFile();

        private string RID;
        private string fileName;
        private string fileLogName;
        public ErrorInfo(string rID)
        {
            
            _store = new ProcStore();

            InitializeComponent();
      
            RID = rID;
            ShieldRight();
        }





        private void upLoadBtn_Click(object sender, EventArgs e)
        {
            string folder = GlobalVar.CurrentTsNode.Aggregate("", (current, nodetype) => current + (nodetype + "-"));
            folder = folder.Remove(folder.Length - 1);
            folder = folder.Replace("/", "&");
            string path = "ErrorInfo\\" + folder + "-" + RID;
            file.CreateFolder(path);
            
            List<string> listError = new List<string>();
            
            path = path + "\\" + fileName;
            File.Copy(btnSearch.Text, path, true);
            listError.Add(path);
            listError.Add(txtScript.Text);
            if(!GlobalVar.ErrorInfo.ContainsKey(RID))
                 GlobalVar.ErrorInfo.Add(RID, listError);
            //else
            //    GlobalVar.ErrorInfo[RID].AddRange(listError);
            Dictionary<string, object> _dict = GlobalVar.ReportCopy;
            string errorInfo = Json.SerJson(GlobalVar.ErrorInfo);
            if(!_dict.ContainsKey("ErrorInfo"))
                _dict.Add("ErrorInfo", errorInfo);
            else
            {
                _dict["ErrorInfo"] = errorInfo;
            }
            string error = "";
           bool isSuccess = _store.Update(EnumLibrary.EnumTable.ErrorInfo, _dict, out error);
            if (isSuccess)
            {
                MessageBox.Show(@"上传成功...");
                GlobalVar.IsUpload = true;
                this.DialogResult = DialogResult.OK;
            }
           
        }


        public static DialogResult Show(UserLookAndFeel look, IWin32Window owner, string text, string caption, DialogResult[] buttons, Icon icon, int defaultButton, MessageBoxIcon messageBeepSound)
        {

            XtraMessageBoxForm form = new XtraMessageBoxForm();
            Font defaultFont = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            form.Appearance.Font = defaultFont;
            return form.ShowMessageBoxDialog(new XtraMessageBoxArgs(look, owner, text, caption, buttons, icon, defaultButton));
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", "/n, " + AppDomain.CurrentDomain.BaseDirectory + "ErrorInfo");
        }

        private void GetRecordByTime(string path, float timeStart, float timeEnd)
        {
            List<string> listRecord = new List<string>();
            bool isStart = false;
            if (path == "")
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
                txtScript.Text += record + "\r\n";
            }

        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            int timeStart, timeEnd;
            if (!int.TryParse(txtTimeStart.Text, out timeStart)) return;
            if (!int.TryParse(txtTimeEnd.Text, out timeEnd)) return;
            GetRecordByTime(beLog.Text, timeStart, timeEnd);
        }


        private void beLog_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            var ofd = new OpenFileDialog { Filter = "(*.txt,*.asc;)|*.txt;*.asc;" };
            var dr = ofd.ShowDialog();
            if (dr != DialogResult.OK) return;
            fileLogName = ofd.SafeFileName;
            beLog.Text = ofd.FileName;
        }

        private void btnSearch_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            try
            {
                var ofd = new OpenFileDialog { Filter = "(*.jpg,*.gif,*.bmp;*.png;)|*.jpg;*.gif;*.bmp*;.png;" };
                var dr = ofd.ShowDialog();
                if (dr != DialogResult.OK) return;
                fileName = ofd.SafeFileName;
                Bitmap bmp = new Bitmap(ofd.FileName);
                ptTopology.Image = bmp;
                btnSearch.Text = ofd.FileName;


            }
            catch (Exception ex)
            {
                Show(DLAF.LookAndFeel, this, "不是有效地图片文件...", "", new[] { DialogResult.OK }, null, 0,
                    MessageBoxIcon.Information);
                throw;
            }
        }

        private void ShieldRight()
        {
            ContextMenu emptyMenu = new ContextMenu();
            txtScript.Properties.ContextMenu = emptyMenu;
            txtTimeEnd.Properties.ContextMenu = emptyMenu;
            txtTimeStart.Properties.ContextMenu = emptyMenu;
        }
    }
}