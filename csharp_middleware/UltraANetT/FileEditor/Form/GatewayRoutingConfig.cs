using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.Spreadsheet;
using DevExpress.XtraEditors;
using DevExpress.XtraRichEdit.Import;
using DevExpress.XtraSpreadsheet;
using FileEditor.pubClass;
using System.Threading;

namespace FileEditor.Form
{
    public partial class GatewayRoutingConfig : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private bool _isReLoad = false;//防止重新加载Excel的时候发生死锁
        private string _Path = string.Empty;//加载Excel的副本路径
        private readonly string _newPath = string.Empty;//保存Excel的副本路径
        private bool _isSave = false;

        public GatewayRoutingConfig()
        {
            InitializeComponent();
        }

        public GatewayRoutingConfig(string path)
        {
            InitializeComponent();
            sscGatewayRoutingCfgTemplate.ReadOnly = true;
            bbtnSave.Enabled = false;
            this.Text = @"网关路由配置表 (" + GlobalVar.VNode[0] +@"-"+ GlobalVar.VNode[1] + @"-" + GlobalVar.VNode[2] + ")";
            _Path = AppDomain.CurrentDomain.BaseDirectory + @"temporary\config\GatewayRouting\" + GlobalVar.VNode[0] + @"\" +
                       GlobalVar.VNode[1] + @"\" +
                       GlobalVar.VNode[2] + @"\GatewayRouting.xlsx";
            _newPath = _Path;
            if (!string.IsNullOrEmpty(path.Trim()))
            {
                _Path = path;
                ReLoadExcel();
            }
        }

        private void ReLoadExcel()
        {
            if (File.Exists(_Path))
            {
                _isReLoad = true;
                sscGatewayRoutingCfgTemplate.LoadDocument(_Path);
                sscGatewayRoutingCfgTemplate.ReadOnly = false;
                GlobalVar.strEvent = _Path;
            }
        }

        private void sscGatewayRoutingCfgTemplate_BeforeImport(object sender,
            DevExpress.Spreadsheet.SpreadsheetBeforeImportEventArgs e)
        {
            if (_isReLoad)
            {
                _isReLoad = false;
            }
            else
            {
                string strDirInfo = Path.GetDirectoryName(_Path);
                Directory.CreateDirectory(strDirInfo);
                File.Copy(e.Options.SourceUri, _Path, true);
                _isReLoad = true;
                sscGatewayRoutingCfgTemplate.LoadDocument(_Path);
                sscGatewayRoutingCfgTemplate.ReadOnly = false;
                GlobalVar.strEvent = _Path;
            }
        }

        private void GatewayRoutingConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sscGatewayRoutingCfgTemplate.Modified)
            {
                if (XtraMessageBox.Show("还有未保存的操作，是否继续退出？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) ==
                    DialogResult.No)
                {
                    GlobalVar.strEvent = string.Empty;
                    e.Cancel = true;
                }
            }
            else
            {
                if (_isSave)
                {
                    this.DialogResult = DialogResult.OK;
                    GlobalVar.strEvent = _Path;
                }
            }
        }

        private void bbtnSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var grBytes = sscGatewayRoutingCfgTemplate.SaveDocument(DocumentFormat.Xlsx);
            try
            {
                FileInfo fi=new FileInfo(_newPath);
                Directory.CreateDirectory(fi.DirectoryName);
                File.WriteAllBytes(_newPath, grBytes);
                sscGatewayRoutingCfgTemplate.Modified = false;
                bbtnSave.Enabled = sscGatewayRoutingCfgTemplate.Modified;
                _isSave = true;
                _Path = _newPath;
                GlobalVar.strEvent = _newPath;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("保存失败，请重试！", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            }
        }

        private void sscGatewayRoutingCfgTemplate_ModifiedChanged(object sender, EventArgs e)
        {
            bbtnSave.Enabled = sscGatewayRoutingCfgTemplate.Modified;
        }

        private void sscGatewayRoutingCfgTemplate_DocumentLoaded(object sender, EventArgs e)
        {
            sscGatewayRoutingCfgTemplate.Modified = true;
            bbtnSave.Enabled = sscGatewayRoutingCfgTemplate.Modified;
        }
    }
}