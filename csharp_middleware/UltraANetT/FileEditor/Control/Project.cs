using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using FileEditor.pubClass;
using ProcessEngine;

namespace FileEditor.Control
{
    public partial class Project : XtraUserControl
    {
        #region 参数变量
        Dictionary<string, object> _dictProject = new Dictionary<string, object>();
        ProcStore _store = new ProcStore();
        private readonly ProcShow _show = new ProcShow();
        ProcFile _file = new ProcFile();
        private string _proName;
        private DataRow _dr;
        #endregion


        public Project()
        {
            InitializeComponent();
            InitGrid();
            InitDict();
           
        }

        private void btnFind_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            
            var ofd = new FolderBrowserDialog();
            var pro = ofd.ShowDialog();
            if (pro != DialogResult.OK) return;
            btnFind.Text = ofd.SelectedPath;
        }

        private void btnUpLoad_Click(object sender, EventArgs e)
        {
            var byteListStream = new List<byte>();
            var byteStream = new byte[2048];
            string folderPath = btnFind.Text;
            string[] arrayPath = folderPath.Split('\\');
            string folderName = arrayPath[arrayPath.Length - 1] + ".zip";
            _file.ZipFile(folderPath, AppDomain.CurrentDomain.BaseDirectory + "project\\" + folderName);
            Stream sr = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "project\\" + folderName, FileMode.Open);
            while (sr.Read(byteStream, 0, byteStream.Length) > 0)
                byteListStream.AddRange(byteStream);
            _dictProject["ProName"] = folderName;
            _dictProject["Content"] = byteListStream.ToArray();
            _dictProject["UploadUser"] = GlobalVar.UserName;
            _dictProject["UploadDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            string error = "";
            _store.AddProject(_dictProject, out error);
            if (error != "")
            {
                Show(DLAF.LookAndFeel, this, "文件添加失败，请联系工程师帮助解决...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                return;
            }
            Show(DLAF.LookAndFeel, this, "上传成功...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
            InitGrid();
            btnFind.Text = "";


        }

        private void InitDict()
        {
            _dictProject.Add("ProName","");
            _dictProject.Add("Content",null);
            _dictProject.Add("UploadUser", "");
            _dictProject.Add("UploadDate", null);
        }

        void InitGrid()
        {
            var coList = new List<string>();
            foreach (GridColumn col in gvProject.Columns)
                coList.Add(col.FieldName);
            var dt = _show.DrawDtFromMultiple(coList.ToArray(), EnumLibrary.EnumTable.Project);
            gcProject.DataSource = dt;
            
        }


        

        private void gvProject_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Right)
            {
                //获得光标位置
                var hi = gvProject.CalcHitInfo(e.Location);
                //判断位置是否在行位置上
                if (!hi.InRow && !hi.InRowCell) return;
                if (hi.RowHandle < 0) return;
                //取一行值
                gvProject.SelectRow(hi.RowHandle);
                _dr = gvProject.GetDataRow(gvProject.FocusedRowHandle);

                if (_dr == null)
                {
                    tsmiDel.Enabled = false;
                }

            }

        }

        
        
        
        private void tsmiDel_Click(object sender, EventArgs e)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("ProName", "");
            dict["ProName"] = _proName;
            string error;
            _store.Del(EnumLibrary.EnumTable.Project, dict, out error);
            if (error == "")
            {
                InitGrid();
                Show(DLAF.LookAndFeel, this, "删除成功...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                tsmiDel.Enabled = false;
            }
            else
            {
                Show(DLAF.LookAndFeel, this, "删除操作出错，请联系工程师帮助解决...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
            }
            
        }
        private void gvProject_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left) /*return;*/
            {
                tsmiDel.Enabled = true;
                _proName = gvProject.GetRowCellValue(e.RowHandle, gvProject.Columns["ProName"]).ToString();

            }
        }

        

        private void btnFind_EditValueChanged(object sender, EventArgs e)
        {
            if (btnFind.Text != "")
                btnUpLoad.Enabled = true;
        }

        public static DialogResult Show(UserLookAndFeel look, IWin32Window owner, string text, string caption, DialogResult[] buttons, Icon icon, int defaultButton, MessageBoxIcon messageBeepSound)
        {

            XtraMessageBoxForm form = new XtraMessageBoxForm();
            Font defaultFont = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            form.Appearance.Font = defaultFont;
            return form.ShowMessageBoxDialog(new XtraMessageBoxArgs(look, owner, text, caption, buttons, icon, defaultButton));
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            UnzipProject();
        }
        private void UnzipProject()
        {
            IList<object> prObjects = _store.GetProject();
            var filePro = prObjects[1] as byte[];
            var fileName = prObjects[0].ToString();
            string filePath = "";
            FolderBrowserDialog _fbd = new FolderBrowserDialog();
            if (_fbd.ShowDialog() == DialogResult.OK)
            {
                filePath = _fbd.SelectedPath;
            }
            FileStream fs = new FileStream(filePath + "\\" + fileName,
                FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(filePro);
            bw.Close();
            fs.Close();

            _file.UnZipFile(AppDomain.CurrentDomain.BaseDirectory + "project\\" + fileName,
                AppDomain.CurrentDomain.BaseDirectory + "project");
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "project\\" + fileName))
            {
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "project\\" + fileName);
            }
        }

    }
}
