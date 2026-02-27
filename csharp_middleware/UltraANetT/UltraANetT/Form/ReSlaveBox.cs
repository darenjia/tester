using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using ProcessEngine;

namespace UltraANetT.Form
{
    public partial class ReSlaveBox : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private ProcStore _store = new ProcStore();
        private List<LayoutControlItem> _LcItems = new List<LayoutControlItem>();
        private Dictionary<string, string> _dictAllSlaveID = new Dictionary<string, string>();
        public ReSlaveBox(string busType)
        {
            InitializeComponent();
            var _vNode = GlobalVar.CurrentTsNode[0].Split('-').ToList();
            _vNode.Add(GlobalVar.CurrentTsNode[3]);
            //
            Dictionary<string, object> dictFile = new Dictionary<string, object>();
            dictFile.Add("VehicelType", _vNode[0]);
            dictFile.Add("VehicelConfig", _vNode[1]);
            dictFile.Add("VehicelStage", _vNode[2]);
            dictFile.Add("MatchSort", busType);
            //
            string node = GlobalVar.CurrentTsNode[4];
            List<string> nodeList = new List<string>();
            if (node.Contains("/"))
                nodeList = node.Split('/').ToList();
            else
                nodeList.Add(node);
            var flList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelDouble, dictFile);
            var sbList = _store.GetRegularByEnum(EnumLibrary.EnumTable.NodeConfigurationBox);
            foreach (var slaveBoxObject in sbList)
            {
                _dictAllSlaveID[slaveBoxObject[0].ToString()] = slaveBoxObject[1].ToString();
            }
            var dictContent = Json.DerJsonToLDict(flList[0][7].ToString());
            Dictionary<string, string> dictTakeSlaveID = new Dictionary<string, string>();
            int intSlaveIndex = 0;
            foreach (var KeyContent in dictContent)
            {
                if (KeyContent.ContainsKey("SlaveboxID"))
                {
                    if (nodeList.Contains(KeyContent["DUTname"]))
                    {
                        dictTakeSlaveID[KeyContent["DUTname"]] = KeyContent["SlaveboxID"];
                        CreateComboBox(KeyContent, intSlaveIndex);
                        intSlaveIndex++;
                    }
                }
                else
                {
                    continue;
                }
            }

            CreateButton(intSlaveIndex);
            foreach (LayoutControlItem item in _LcItems)
            {
                lcGroup.AddItem(item);
            }

            List<string> listTemp = new List<string>();
            bool isNoRepeat = true;
            foreach (var item in _LcItems)
            {
                if (typeof(SimpleButton) == item.Control.GetType())
                    continue;
                if (!listTemp.Contains(item.Control.Text))
                {
                    listTemp.Add(item.Control.Text);
                }
                else
                {
                    isNoRepeat = false;
                    break;
                }
            }
            if (!isNoRepeat)
            {
                GlobalVar.isGetSlaveBoxID = false;
            }
            this.MinimumSize = new Size(this.Size.Width, _LcItems.Count * 38+50);
            this.Size = new Size(this.Size.Width, _LcItems.Count * 38 + 50);
        }

        private void button_Click(object sender, EventArgs e)
        {
            List<string> listTemp = new List<string>();//临时变量，用来判断是否有重复选择的配置盒
            Dictionary<string, string> dictGetAllSlaveID = new Dictionary<string, string>();
            bool isNoRepeat = true;
            foreach (var item in _LcItems)
            {
                if (typeof(SimpleButton) == item.Control.GetType())
                    continue;
                if (!listTemp.Contains(item.Control.Text))
                {
                    dictGetAllSlaveID[item.Control.Name] = _dictAllSlaveID[item.Control.Text];
                    listTemp.Add(item.Control.Text);
                }
                else
                {
                    isNoRepeat = false;
                    break;
                }
            }
            if (!isNoRepeat)
            {
                XtraMessageBox.Show("同一个节点配置盒不可同时配置多个节点！");
                GlobalVar.isGetSlaveBoxID = false;
            }
            else
            {
                GlobalVar.isGetSlaveBoxID = true;
                GlobalVar.dictSlaveBoxID = dictGetAllSlaveID;
                
                this.Close();
            }
        }

        private void ReSlaveBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (GlobalVar.isGetSlaveBoxID)
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                if (XtraMessageBox.Show("节点配置盒选项还未保存，关闭后将无法进行测试操作！", "提示", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning) == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                else
                {
                    this.DialogResult = DialogResult.Cancel;
                }
            }
        }

        private void CreateComboBox(Dictionary<string, string> scheme, int y)
        {
            ComboBoxEdit combedit = new ComboBoxEdit();
            LayoutControlItem lc = new LayoutControlItem();
            // textEdit
            combedit.Name = scheme["DUTname"];
            combedit.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            combedit.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            combedit.Properties.Items.AddRange(_dictAllSlaveID.Keys);
            combedit.EditValue = scheme["SlaveboxID"];
            //combedit.TextChanged += ComboBoxEdit_TextChanged;
            // LayoutControlItem
            lc.Control = combedit;
            //lc.Name = "LC" + DictTextEdit["EngName"];
            lc.Text = scheme["DUTname"] + @"：";

            //lc.TextLocation = DevExpress.Utils.Locations.Left;
            lc.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 5, 5);
            lc.Location = new System.Drawing.Point(0, y * 28);
            lc.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            //绑定
            _LcItems.Add(lc);
        }

        private void CreateButton(int y)
        {
            SimpleButton simpleButton = new SimpleButton();
            LayoutControlItem simpleButtonLCI = new LayoutControlItem();
            // 
            // simpleButton
            // 
            simpleButton.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            simpleButton.Appearance.Options.UseFont = true;
            //simpleButton.AutoWidthInLayoutControl = true;
            simpleButton.Size = new System.Drawing.Size(260, 28);
            simpleButton.Name = "simpleButton";
            simpleButton.Text = @"确定";
            simpleButton.Location = new System.Drawing.Point(0, y * 28);
            simpleButton.Margin = new System.Windows.Forms.Padding(3, 3, 5, 5);
            simpleButton.MinimumSize = new Size(0, 28);
            // 
            // simpleButtonLCI
            // 
            simpleButtonLCI.Control = simpleButton;
            simpleButtonLCI.ControlAlignment = System.Drawing.ContentAlignment.BottomCenter;
            simpleButtonLCI.Location = new System.Drawing.Point(0, y * 28);
            simpleButtonLCI.Name = "simpleButtonLCI";
            simpleButtonLCI.Text = "simpleButtonLCI";
            simpleButtonLCI.TextVisible = false;

            simpleButton.Click += button_Click;
            _LcItems.Add(simpleButtonLCI);
            //this.dpCfgGroup.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] { simpleButtonLCI });
        }
    }
}