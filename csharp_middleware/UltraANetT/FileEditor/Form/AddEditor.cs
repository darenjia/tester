using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using FileEditor.pubClass;

namespace FileEditor.form
{
    public partial class AddEditor : DevExpress.XtraBars.Ribbon.RibbonForm
    {

        private DataOper _currentOperate;
        private Dictionary<string, object[]> _dictobj = new Dictionary<string, object[]>();
        private Dictionary<string, object> _dict = new Dictionary<string, object>();
        private List<object> emlList = new List<object>();
        public bool Flag = false;

        public AddEditor(DataRow dr)
        {
            InitializeComponent();
            _currentOperate = DataOper.Insert;
            int drNum = int.Parse(dr["Sequence"].ToString()) + 1;
            sequence.Text = drNum.ToString();
            sequence.ReadOnly = true;

        }

        public AddEditor(object seqNum)
        {
            InitializeComponent();
            _currentOperate = DataOper.Add;
            sequence.Text = seqNum.ToString();
            sequence.ReadOnly = true;
        }

        public AddEditor(Dictionary<string, object> dictEml)
        {
            InitializeComponent();
            sequence.ReadOnly = true;
            _currentOperate = DataOper.Modify;
            sequence.Text = dictEml["Sequence"].ToString();
            chiName.Text = dictEml["ChiName"].ToString();
            engName.Text = dictEml["EngName"].ToString();
            useState.Checked = bool.Parse(dictEml["UsingState"].ToString());

            isHexaddecimal.Checked = bool.Parse(dictEml["IsHexadecimal"].ToString());
            unit.Text = dictEml["Unit"].ToString();
            isStringLimit.Checked = bool.Parse(dictEml["IsStringLimit"].ToString());
            stringRange.Text = dictEml["StringRange"].ToString();
            isMaxMin.Checked = bool.Parse(dictEml["IsMaxMin"].ToString());
            maxValue.Text = dictEml["MaxValue"].ToString();
            minValue.Text = dictEml["MinValue"].ToString();
            isSpecialFormat.Checked = bool.Parse(dictEml["IsSpecialFormat"].ToString());
            formatName.Text = dictEml["FormatName"].ToString();

        }
        private enum DataOper
        {
            Add = 0,
            Insert = 1,
            Modify = 2
        }

        public List<object> Dict()
        {
            //if(sequence.Text ==""||chiName.Text==""||engName.Text=="")
            //    emlList.Add("");
            return emlList;
        }

        private void btnYes_Click(object sender, EventArgs e)
        {

            //EmlColEditor ec = new EmlColEditor();
            if (EditValidating())
                return;
            switch (_currentOperate)
            {
                case DataOper.Add:
                    GetList();
                    //GlobalVar.ChangList.Add(emlList);

                    Show(DLAF.LookAndFeel, this, "添加成功...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                    InitControlEnable();
                    this.Close();
                    break;
                case DataOper.Insert:
                    GetList();
                    //GlobalVar.ChangList.Add(emlList);
                    Show(DLAF.LookAndFeel, this, "插入成功...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                    InitControlEnable();
                    this.Close();
                    break;
                case DataOper.Modify:
                    sequence.ReadOnly = true;
                    GetList();
                    //GlobalVar.ChangList.Add(emlList);
                    Show(DLAF.LookAndFeel, this, "修改成功...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                    InitControlEnable();
                    this.Close();
                    break;

            }

        }

        private bool EditValidating()
        {
            errorProvider.ClearErrors();
            bool isError = false;
            string error = "";
            error = "此项不能为空";
            if (engName.Text == "")
            {
                errorProvider.SetError(engName, error);
                engName.Focus();
                isError = true;
                
            }
            else if (sequence.Text == "")
            {
                sequence.Focus();
                errorProvider.SetError(sequence, error);
                isError = true;
                
            }

            else if (chiName.Text == "")
            {
                chiName.Focus();
                errorProvider.SetError(chiName, error);
                isError = true;
                
            }
            else if (isStringLimit.Checked)
                if (stringRange.Text == "")
                {
                    stringRange.Focus();
                    errorProvider.SetError(stringRange, error);
                    isError = true;
                    
                }
            else if (isMaxMin.Checked)
                if (maxValue.Text == "")
                {
                    maxValue.Focus();
                    errorProvider.SetError(maxValue, error);
                    isError = true;
                    
                }
            else if (isMaxMin.Checked)
                if (minValue.Text == "")
                {
                    minValue.Focus();
                    errorProvider.SetError(minValue, error);
                    isError = true;
                    
                }
            else if (isSpecialFormat.Checked)
                if (formatName.Text == "")
                {
                    formatName.Focus();
                    errorProvider.SetError(formatName, error);
                    isError = true;
                    
                }
            return isError;
        }
        private void UpdateListArray()
        {
            List<List<object>> newList = new List<List<object>>();
            foreach (List<object> list in GlobalVar.ChangList)
            {
                if (list[4].ToString() == "Add")
                {
                    list[0] = emlList[0];
                }
                newList.Add(list);
            }


        }
        private void GetList()
        {
            emlList.Add(sequence.Text);
            emlList.Add(chiName.Text);
            emlList.Add(engName.Text);
            emlList.Add(useState.Checked.ToString());
            //emlList.Add(_currentOperate);
            emlList.Add(isHexaddecimal.Checked.ToString());
            emlList.Add(unit.Text);
            //if (!isHexaddecimal.Checked)
            //{
            emlList.Add(isStringLimit.Checked.ToString());
            string range = stringRange.Text.Replace("，", ",");
            emlList.Add(range);



            emlList.Add(isMaxMin.Checked.ToString());
            emlList.Add(minValue.Text);
            emlList.Add(maxValue.Text);

            emlList.Add(isSpecialFormat.Checked.ToString());
            emlList.Add(formatName.Text);
            //}         
            Flag = true;
        }

        public static DialogResult Show(UserLookAndFeel look, IWin32Window owner, string text, string caption, DialogResult[] buttons, Icon icon, int defaultButton, MessageBoxIcon messageBeepSound)
        {

            XtraMessageBoxForm form = new XtraMessageBoxForm();
            Font defaultFont = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            form.Appearance.Font = defaultFont;
            return form.ShowMessageBoxDialog(new XtraMessageBoxArgs(look, owner, text, caption, buttons, icon, defaultButton));
        }
        private void InitControlEnable()
        {
            unit.Enabled = true;
            isStringLimit.Enabled = true;
            stringRange.Enabled = false;
            isMaxMin.Enabled = true;
            minValue.Enabled = false;
            maxValue.Enabled = false;
            isSpecialFormat.Enabled = true;
            formatName.Enabled = false;
        }
        private void isHexaddecimal_CheckedChanged(object sender, EventArgs e)
        {
            if (isHexaddecimal.Checked)
            {
                unit.Enabled = false;
                isStringLimit.Enabled = false;
                stringRange.Enabled = false;
                isMaxMin.Enabled = false;
                minValue.Enabled = false;
                maxValue.Enabled = false;
                isSpecialFormat.Enabled = false;
                formatName.Enabled = false;
            }
        }

        private void isStringLimit_CheckedChanged(object sender, EventArgs e)
        {
            if (isStringLimit.Checked)
                stringRange.Enabled = true;

        }

        private void isMaxMin_CheckedChanged(object sender, EventArgs e)
        {
            if (isMaxMin.Checked)
            {
                minValue.Enabled = true;
                maxValue.Enabled = true;
            }
        }

        private void isSpecialFormat_CheckedChanged(object sender, EventArgs e)
        {
            if (isSpecialFormat.Checked)
                formatName.Enabled = true;
        }

        private void maxValue_EditValueChanged(object sender, EventArgs e)
        {
            float result;
            if(isMaxMin.Checked)
                if (maxValue.Text.ToString() != "" & float.TryParse(maxValue.Text.ToString(), out result))
                {

                    if (float.TryParse(minValue.Text.ToString(), out result))
                    {
                        float min = float.Parse(minValue.Text.ToString());
                        float max = float.Parse(maxValue.Text.ToString());
                        if (max <= min)
                        {
                            Show(DLAF.LookAndFeel, this, "请输入大于最小值的数字或修改最小值", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);

                        }
                    }
                }
                else 
                    Show(DLAF.LookAndFeel, this, "请输入数字", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);


        }

        private void minValue_EditValueChanged(object sender, EventArgs e)
        {
            float result;
            if (isMaxMin.Checked)
                if (minValue.Text.ToString() != "" & !float.TryParse(minValue.Text.ToString(), out result))
                {
                    Show(DLAF.LookAndFeel, this, "请输入数字", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);

                }
                else if (maxValue.Text.ToString() != "" & float.TryParse(maxValue.Text.ToString(), out result))
                {
                    float min = float.Parse(minValue.Text.ToString());
                    float max = float.Parse(maxValue.Text.ToString());
                    if (min >= max)
                        Show(DLAF.LookAndFeel, this, "最小值应小于最大值，请重新输入或修改最大值", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);

                }
        }

        private void engName_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            
        }

        private void sequence_Validating(object sender, CancelEventArgs e)
        {
            
        }

        private void chiName_Validating(object sender, CancelEventArgs e)
        {
            
        }

        private void Validating(object sender, CancelEventArgs e)
        {
            string error = "";
            error = "此项不能为空";
            if (engName.Text == "")
            {

                e.Cancel = true;
                engName.Focus();
            }
            errorProvider.SetError(engName, error);

            if (sequence.Text == "")
            {
                e.Cancel = true;
                sequence.Focus();
                errorProvider.SetError(sequence, error
                    );
            }

            if (chiName.Text == "")
            {
                e.Cancel = true;
                chiName.Focus();
                errorProvider.SetError(chiName, error);
            }
            if (isStringLimit.Checked)
                if (stringRange.Text == "")
                {
                    e.Cancel = true;
                    stringRange.Focus();
                    errorProvider.SetError(stringRange, error);
                }
            if (isMaxMin.Checked)
                if (maxValue.Text == "")
                {
                    e.Cancel = true;
                    maxValue.Focus();
                    errorProvider.SetError(maxValue, error);
                }
            if (isMaxMin.Checked)
                if (minValue.Text == "")
                {
                    e.Cancel = true;
                    minValue.Focus();
                    errorProvider.SetError(minValue, error);
                }
            if (isSpecialFormat.Checked)
                if (formatName.Text == "")
                {
                    e.Cancel = true;
                    formatName.Focus();
                    errorProvider.SetError(formatName, error);
                }

        }
    }
}