using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace UltraANetT.Form
{
    public partial class ReName : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        List<string>_checkNodes = new List<string>();
        List<string> renameList = new  List<string>();
             
        public ReName(List<string> targetNodes,List<string>checkNodes )
        {
            InitializeComponent();
            vehName.Text = targetNodes[0];
            setName.Text = targetNodes[1];
            cfgName.Text = @"NewStage1";
            _checkNodes = checkNodes;
            renameList.Add(vehName.Text);
            renameList.Add(setName.Text);
            //屏蔽右键菜单
            ShieldRight();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!(Regex.IsMatch(cfgName.Text, "^[\u4E00-\u9FA5A-Za-z0-9]+$")))
            {
                XtraMessageBox.Show(@"输入有非法字符请核对...");
                return;
            }
            foreach (var node in _checkNodes)
            {
                if (cfgName.Text == node)
                {
                    XtraMessageBox.Show(@"此名称已被占用...");
                    return;
                }
            }
            renameList.Add(cfgName.Text);
            GlobalVar.RenameList = renameList;
            MessageBox.Show(@"修改完毕...");

            this.DialogResult = DialogResult.OK;
        }
        private void ShieldRight()
        {
            ContextMenu emptyMenu = new ContextMenu();
            cfgName.Properties.ContextMenu = emptyMenu;

        }
    }
}