namespace UltraANetT.Module
{
    partial class QuestionNote
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.gcQuesNote = new DevExpress.XtraGrid.GridControl();
            this.CMSTable = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiIdentify = new System.Windows.Forms.ToolStripMenuItem();
            this.gvQuesNote = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.ExapID = new DevExpress.XtraGrid.Columns.GridColumn();
            this.ExapName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.AssessItem = new DevExpress.XtraGrid.Columns.GridColumn();
            this.DescriptionOfValue = new DevExpress.XtraGrid.Columns.GridColumn();
            this.MinValue = new DevExpress.XtraGrid.Columns.GridColumn();
            this.MaxValue = new DevExpress.XtraGrid.Columns.GridColumn();
            this.NormalValue = new DevExpress.XtraGrid.Columns.GridColumn();
            this.TestValue = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Result = new DevExpress.XtraGrid.Columns.GridColumn();
            this.TestTime = new DevExpress.XtraGrid.Columns.GridColumn();
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.btnSort = new DevExpress.XtraEditors.SimpleButton();
            this.cbType = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cbName = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cbRound = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cbStage = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cbConfig = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cbVehicel = new DevExpress.XtraEditors.ComboBoxEdit();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem7 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem8 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem9 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.gcQuesNote)).BeginInit();
            this.CMSTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvQuesNote)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cbType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbRound.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbStage.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbConfig.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbVehicel.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem9)).BeginInit();
            this.SuspendLayout();
            // 
            // gcQuesNote
            // 
            this.gcQuesNote.ContextMenuStrip = this.CMSTable;
            this.gcQuesNote.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.gcQuesNote.Location = new System.Drawing.Point(18, 103);
            this.gcQuesNote.MainView = this.gvQuesNote;
            this.gcQuesNote.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gcQuesNote.Name = "gcQuesNote";
            this.gcQuesNote.Size = new System.Drawing.Size(1200, 592);
            this.gcQuesNote.TabIndex = 0;
            this.gcQuesNote.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvQuesNote});
            // 
            // CMSTable
            // 
            this.CMSTable.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.CMSTable.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiIdentify});
            this.CMSTable.Name = "CMSTable";
            this.CMSTable.Size = new System.Drawing.Size(153, 32);
            // 
            // tsmiIdentify
            // 
            this.tsmiIdentify.Enabled = false;
            this.tsmiIdentify.Name = "tsmiIdentify";
            this.tsmiIdentify.Size = new System.Drawing.Size(152, 28);
            this.tsmiIdentify.Text = "认证通过";
            this.tsmiIdentify.Click += new System.EventHandler(this.tsmiIdentify_Click);
            // 
            // gvQuesNote
            // 
            this.gvQuesNote.Appearance.Empty.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvQuesNote.Appearance.Empty.Options.UseFont = true;
            this.gvQuesNote.Appearance.FilterPanel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvQuesNote.Appearance.FilterPanel.Options.UseFont = true;
            this.gvQuesNote.Appearance.FocusedRow.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.gvQuesNote.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gvQuesNote.Appearance.GroupPanel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvQuesNote.Appearance.GroupPanel.Options.UseFont = true;
            this.gvQuesNote.Appearance.GroupRow.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvQuesNote.Appearance.GroupRow.Options.UseFont = true;
            this.gvQuesNote.Appearance.HeaderPanel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvQuesNote.Appearance.HeaderPanel.Options.UseFont = true;
            this.gvQuesNote.Appearance.Row.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvQuesNote.Appearance.Row.Options.UseFont = true;
            this.gvQuesNote.Appearance.SelectedRow.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvQuesNote.Appearance.SelectedRow.Options.UseFont = true;
            this.gvQuesNote.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.ExapID,
            this.ExapName,
            this.AssessItem,
            this.DescriptionOfValue,
            this.MinValue,
            this.MaxValue,
            this.NormalValue,
            this.TestValue,
            this.Result,
            this.TestTime});
            this.gvQuesNote.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFullFocus;
            this.gvQuesNote.GridControl = this.gcQuesNote;
            this.gvQuesNote.Name = "gvQuesNote";
            this.gvQuesNote.OptionsBehavior.Editable = false;
            this.gvQuesNote.OptionsBehavior.ReadOnly = true;
            this.gvQuesNote.OptionsCustomization.AllowColumnMoving = false;
            this.gvQuesNote.OptionsMenu.EnableColumnMenu = false;
            this.gvQuesNote.OptionsMenu.EnableFooterMenu = false;
            this.gvQuesNote.OptionsMenu.EnableGroupPanelMenu = false;
            this.gvQuesNote.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.gvQuesNote.OptionsSelection.EnableAppearanceFocusedRow = false;
            this.gvQuesNote.OptionsSelection.MultiSelect = true;
            this.gvQuesNote.OptionsView.ColumnAutoWidth = false;
            this.gvQuesNote.OptionsView.ShowGroupPanel = false;
            this.gvQuesNote.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gvQuesNote_MouseDown);
            // 
            // ExapID
            // 
            this.ExapID.Caption = "用例编号";
            this.ExapID.FieldName = "ExapID";
            this.ExapID.Name = "ExapID";
            this.ExapID.Visible = true;
            this.ExapID.VisibleIndex = 0;
            // 
            // ExapName
            // 
            this.ExapName.Caption = "用例名称";
            this.ExapName.FieldName = "ExapName";
            this.ExapName.Name = "ExapName";
            this.ExapName.Visible = true;
            this.ExapName.VisibleIndex = 1;
            this.ExapName.Width = 93;
            // 
            // AssessItem
            // 
            this.AssessItem.Caption = "评价项目";
            this.AssessItem.FieldName = "AssessItem";
            this.AssessItem.Name = "AssessItem";
            this.AssessItem.Visible = true;
            this.AssessItem.VisibleIndex = 2;
            this.AssessItem.Width = 85;
            // 
            // DescriptionOfValue
            // 
            this.DescriptionOfValue.Caption = "值描述";
            this.DescriptionOfValue.FieldName = "DescriptionOfValue";
            this.DescriptionOfValue.Name = "DescriptionOfValue";
            this.DescriptionOfValue.Visible = true;
            this.DescriptionOfValue.VisibleIndex = 3;
            // 
            // MinValue
            // 
            this.MinValue.Caption = "最小值";
            this.MinValue.FieldName = "MinValue";
            this.MinValue.Name = "MinValue";
            this.MinValue.Visible = true;
            this.MinValue.VisibleIndex = 4;
            // 
            // MaxValue
            // 
            this.MaxValue.Caption = "最大值";
            this.MaxValue.FieldName = "MaxValue";
            this.MaxValue.Name = "MaxValue";
            this.MaxValue.Visible = true;
            this.MaxValue.VisibleIndex = 5;
            // 
            // NormalValue
            // 
            this.NormalValue.Caption = "正常值";
            this.NormalValue.FieldName = "NormalValue";
            this.NormalValue.Name = "NormalValue";
            this.NormalValue.Visible = true;
            this.NormalValue.VisibleIndex = 6;
            // 
            // TestValue
            // 
            this.TestValue.Caption = "第1次测试值";
            this.TestValue.FieldName = "TestValue";
            this.TestValue.Name = "TestValue";
            this.TestValue.Visible = true;
            this.TestValue.VisibleIndex = 7;
            this.TestValue.Width = 89;
            // 
            // Result
            // 
            this.Result.Caption = "第1次结果";
            this.Result.FieldName = "Result";
            this.Result.Name = "Result";
            this.Result.Visible = true;
            this.Result.VisibleIndex = 8;
            this.Result.Width = 76;
            // 
            // TestTime
            // 
            this.TestTime.Caption = "第1次测试时间";
            this.TestTime.FieldName = "TestTime";
            this.TestTime.Name = "TestTime";
            this.TestTime.Visible = true;
            this.TestTime.VisibleIndex = 9;
            this.TestTime.Width = 85;
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomization = false;
            this.layoutControl1.Controls.Add(this.btnSort);
            this.layoutControl1.Controls.Add(this.cbType);
            this.layoutControl1.Controls.Add(this.cbName);
            this.layoutControl1.Controls.Add(this.cbRound);
            this.layoutControl1.Controls.Add(this.cbStage);
            this.layoutControl1.Controls.Add(this.cbConfig);
            this.layoutControl1.Controls.Add(this.cbVehicel);
            this.layoutControl1.Controls.Add(this.gcQuesNote);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(575, 222, 764, 350);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(1236, 713);
            this.layoutControl1.TabIndex = 1;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // btnSort
            // 
            this.btnSort.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnSort.Appearance.Options.UseFont = true;
            this.btnSort.Location = new System.Drawing.Point(1094, 18);
            this.btnSort.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSort.MinimumSize = new System.Drawing.Size(0, 79);
            this.btnSort.Name = "btnSort";
            this.btnSort.Size = new System.Drawing.Size(124, 79);
            this.btnSort.StyleController = this.layoutControl1;
            this.btnSort.TabIndex = 17;
            this.btnSort.Text = "查看全部数据";
            this.btnSort.Visible = false;
            this.btnSort.Click += new System.EventHandler(this.btnSort_Click);
            // 
            // cbType
            // 
            this.cbType.Enabled = false;
            this.cbType.Location = new System.Drawing.Point(413, 52);
            this.cbType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbType.Name = "cbType";
            this.cbType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbType.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cbType.Size = new System.Drawing.Size(258, 28);
            this.cbType.StyleController = this.layoutControl1;
            this.cbType.TabIndex = 16;
            this.cbType.SelectedValueChanged += new System.EventHandler(this.cbType_SelectedValueChanged);
            // 
            // cbName
            // 
            this.cbName.Enabled = false;
            this.cbName.Location = new System.Drawing.Point(752, 52);
            this.cbName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbName.Name = "cbName";
            this.cbName.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbName.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cbName.Size = new System.Drawing.Size(336, 28);
            this.cbName.StyleController = this.layoutControl1;
            this.cbName.TabIndex = 15;
            this.cbName.SelectedValueChanged += new System.EventHandler(this.cbName_SelectedValueChanged);
            // 
            // cbRound
            // 
            this.cbRound.Enabled = false;
            this.cbRound.Location = new System.Drawing.Point(93, 52);
            this.cbRound.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbRound.Name = "cbRound";
            this.cbRound.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbRound.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cbRound.Size = new System.Drawing.Size(239, 28);
            this.cbRound.StyleController = this.layoutControl1;
            this.cbRound.TabIndex = 14;
            this.cbRound.SelectedValueChanged += new System.EventHandler(this.cbRound_SelectedValueChanged);
            // 
            // cbStage
            // 
            this.cbStage.Enabled = false;
            this.cbStage.Location = new System.Drawing.Point(752, 18);
            this.cbStage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbStage.Name = "cbStage";
            this.cbStage.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbStage.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cbStage.Size = new System.Drawing.Size(336, 28);
            this.cbStage.StyleController = this.layoutControl1;
            this.cbStage.TabIndex = 13;
            this.cbStage.SelectedValueChanged += new System.EventHandler(this.cbStage_SelectedValueChanged);
            // 
            // cbConfig
            // 
            this.cbConfig.Enabled = false;
            this.cbConfig.Location = new System.Drawing.Point(413, 18);
            this.cbConfig.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbConfig.Name = "cbConfig";
            this.cbConfig.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbConfig.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cbConfig.Size = new System.Drawing.Size(258, 28);
            this.cbConfig.StyleController = this.layoutControl1;
            this.cbConfig.TabIndex = 12;
            this.cbConfig.SelectedValueChanged += new System.EventHandler(this.cbConfig_SelectedValueChanged);
            // 
            // cbVehicel
            // 
            this.cbVehicel.Location = new System.Drawing.Point(93, 18);
            this.cbVehicel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbVehicel.Name = "cbVehicel";
            this.cbVehicel.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbVehicel.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cbVehicel.Size = new System.Drawing.Size(239, 28);
            this.cbVehicel.StyleController = this.layoutControl1;
            this.cbVehicel.TabIndex = 11;
            this.cbVehicel.SelectedValueChanged += new System.EventHandler(this.cbVehicel_SelectedValueChanged);
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem5,
            this.layoutControlItem7,
            this.layoutControlItem4,
            this.layoutControlItem8,
            this.layoutControlItem3,
            this.layoutControlItem6,
            this.layoutControlItem2});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Size = new System.Drawing.Size(1236, 713);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.gcQuesNote;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 85);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(1206, 598);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem5.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem5.Control = this.cbStage;
            this.layoutControlItem5.Location = new System.Drawing.Point(659, 0);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Size = new System.Drawing.Size(417, 34);
            this.layoutControlItem5.Text = "阶段";
            this.layoutControlItem5.TextSize = new System.Drawing.Size(72, 24);
            // 
            // layoutControlItem7
            // 
            this.layoutControlItem7.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem7.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem7.Control = this.cbName;
            this.layoutControlItem7.Location = new System.Drawing.Point(659, 34);
            this.layoutControlItem7.Name = "layoutControlItem7";
            this.layoutControlItem7.Size = new System.Drawing.Size(417, 51);
            this.layoutControlItem7.Text = "节点名称";
            this.layoutControlItem7.TextSize = new System.Drawing.Size(72, 24);
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem4.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem4.Control = this.cbConfig;
            this.layoutControlItem4.Location = new System.Drawing.Point(320, 0);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Size = new System.Drawing.Size(339, 34);
            this.layoutControlItem4.Text = "配置";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(72, 24);
            // 
            // layoutControlItem8
            // 
            this.layoutControlItem8.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem8.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem8.Control = this.cbType;
            this.layoutControlItem8.Location = new System.Drawing.Point(320, 34);
            this.layoutControlItem8.Name = "layoutControlItem8";
            this.layoutControlItem8.Size = new System.Drawing.Size(339, 51);
            this.layoutControlItem8.Text = "测试类型";
            this.layoutControlItem8.TextSize = new System.Drawing.Size(72, 24);
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem3.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem3.Control = this.cbVehicel;
            this.layoutControlItem3.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Size = new System.Drawing.Size(320, 34);
            this.layoutControlItem3.Text = "车型";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(72, 24);
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem6.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem6.Control = this.cbRound;
            this.layoutControlItem6.Location = new System.Drawing.Point(0, 34);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Size = new System.Drawing.Size(320, 51);
            this.layoutControlItem6.Text = "轮次";
            this.layoutControlItem6.TextSize = new System.Drawing.Size(72, 24);
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.btnSort;
            this.layoutControlItem2.Location = new System.Drawing.Point(1076, 0);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(130, 85);
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextVisible = false;
            this.layoutControlItem2.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            // 
            // layoutControlItem9
            // 
            this.layoutControlItem9.Location = new System.Drawing.Point(718, 0);
            this.layoutControlItem9.Name = "layoutControlItem9";
            this.layoutControlItem9.Size = new System.Drawing.Size(25, 48);
            this.layoutControlItem9.TextSize = new System.Drawing.Size(50, 20);
            // 
            // QuestionNote
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.layoutControl1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "QuestionNote";
            this.Size = new System.Drawing.Size(1236, 713);
            ((System.ComponentModel.ISupportInitialize)(this.gcQuesNote)).EndInit();
            this.CMSTable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gvQuesNote)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cbType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbRound.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbStage.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbConfig.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbVehicel.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem9)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gcQuesNote;
        private DevExpress.XtraGrid.Views.Grid.GridView gvQuesNote;
        private DevExpress.XtraGrid.Columns.GridColumn ExapName;
        private DevExpress.XtraGrid.Columns.GridColumn AssessItem;
        private DevExpress.XtraGrid.Columns.GridColumn Result;
        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraEditors.ComboBoxEdit cbType;
        private DevExpress.XtraEditors.ComboBoxEdit cbName;
        private DevExpress.XtraEditors.ComboBoxEdit cbRound;
        private DevExpress.XtraEditors.ComboBoxEdit cbStage;
        private DevExpress.XtraEditors.ComboBoxEdit cbConfig;
        private DevExpress.XtraEditors.ComboBoxEdit cbVehicel;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem9;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem7;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem8;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private DevExpress.XtraEditors.SimpleButton btnSort;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraGrid.Columns.GridColumn ExapID;
        private DevExpress.XtraGrid.Columns.GridColumn MinValue;
        private DevExpress.XtraGrid.Columns.GridColumn MaxValue;
        private DevExpress.XtraGrid.Columns.GridColumn NormalValue;
        private DevExpress.XtraGrid.Columns.GridColumn TestValue;
        private DevExpress.XtraGrid.Columns.GridColumn DescriptionOfValue;
        private System.Windows.Forms.ContextMenuStrip CMSTable;
        private System.Windows.Forms.ToolStripMenuItem tsmiIdentify;
        private DevExpress.XtraGrid.Columns.GridColumn TestTime;
    }
}
