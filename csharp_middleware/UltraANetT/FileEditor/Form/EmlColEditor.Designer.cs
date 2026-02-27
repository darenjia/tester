namespace FileEditor.form
{
    partial class EmlColEditor
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmsGrid = new System.Windows.Forms.ContextMenuStrip();
            this.tsmiAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiModify = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiInsert = new System.Windows.Forms.ToolStripMenuItem();
            this.ribbonControl1 = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.cbeType = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cbeVersion = new DevExpress.XtraEditors.ComboBoxEdit();
            this.btnSub = new DevExpress.XtraEditors.SimpleButton();
            this.gcEml = new DevExpress.XtraGrid.GridControl();
            this.gvEml = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.Sequence = new DevExpress.XtraGrid.Columns.GridColumn();
            this.ChiName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.EngName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.UsingState = new DevExpress.XtraGrid.Columns.GridColumn();
            this.IsHexadecimal = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Unit = new DevExpress.XtraGrid.Columns.GridColumn();
            this.IsStringLimit = new DevExpress.XtraGrid.Columns.GridColumn();
            this.StringRange = new DevExpress.XtraGrid.Columns.GridColumn();
            this.IsMaxMin = new DevExpress.XtraGrid.Columns.GridColumn();
            this.MinValue = new DevExpress.XtraGrid.Columns.GridColumn();
            this.MaxValue = new DevExpress.XtraGrid.Columns.GridColumn();
            this.IsSpecialFormat = new DevExpress.XtraGrid.Columns.GridColumn();
            this.FormatName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.lciVersion = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.cmsGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cbeType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbeVersion.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcEml)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvEml)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciVersion)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            this.SuspendLayout();
            // 
            // cmsGrid
            // 
            this.cmsGrid.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.cmsGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAdd,
            this.tsmiModify,
            this.tsmiInsert});
            this.cmsGrid.Name = "cmsGrid";
            this.cmsGrid.Size = new System.Drawing.Size(117, 88);
            // 
            // tsmiAdd
            // 
            this.tsmiAdd.Name = "tsmiAdd";
            this.tsmiAdd.Size = new System.Drawing.Size(116, 28);
            this.tsmiAdd.Text = "添加";
            this.tsmiAdd.Click += new System.EventHandler(this.tsmiAdd_Click);
            // 
            // tsmiModify
            // 
            this.tsmiModify.Name = "tsmiModify";
            this.tsmiModify.Size = new System.Drawing.Size(116, 28);
            this.tsmiModify.Text = "修改";
            this.tsmiModify.Click += new System.EventHandler(this.tsmiModify_Click);
            // 
            // tsmiInsert
            // 
            this.tsmiInsert.Name = "tsmiInsert";
            this.tsmiInsert.Size = new System.Drawing.Size(116, 28);
            this.tsmiInsert.Text = "插入";
            this.tsmiInsert.Click += new System.EventHandler(this.tsmiInsert_Click);
            // 
            // ribbonControl1
            // 
            this.ribbonControl1.ExpandCollapseItem.AllowGlyphSkinning = DevExpress.Utils.DefaultBoolean.False;
            this.ribbonControl1.ExpandCollapseItem.AllowRightClickInMenu = false;
            this.ribbonControl1.ExpandCollapseItem.Id = 0;
            this.ribbonControl1.ExpandCollapseItem.ShowItemShortcut = DevExpress.Utils.DefaultBoolean.False;
            this.ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbonControl1.ExpandCollapseItem});
            this.ribbonControl1.Location = new System.Drawing.Point(0, 0);
            this.ribbonControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ribbonControl1.MaxItemId = 1;
            this.ribbonControl1.Name = "ribbonControl1";
            this.ribbonControl1.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonControlStyle.MacOffice;
            this.ribbonControl1.Size = new System.Drawing.Size(1392, 84);
            this.ribbonControl1.ToolbarLocation = DevExpress.XtraBars.Ribbon.RibbonQuickAccessToolbarLocation.Hidden;
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomization = false;
            this.layoutControl1.Controls.Add(this.cbeType);
            this.layoutControl1.Controls.Add(this.cbeVersion);
            this.layoutControl1.Controls.Add(this.btnSub);
            this.layoutControl1.Controls.Add(this.gcEml);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 84);
            this.layoutControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(457, 170, 250, 350);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(1392, 703);
            this.layoutControl1.TabIndex = 2;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // cbeType
            // 
            this.cbeType.Location = new System.Drawing.Point(440, 13);
            this.cbeType.MenuManager = this.ribbonControl1;
            this.cbeType.Name = "cbeType";
            this.cbeType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbeType.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cbeType.Size = new System.Drawing.Size(180, 28);
            this.cbeType.StyleController = this.layoutControl1;
            this.cbeType.TabIndex = 9;
            this.cbeType.SelectedIndexChanged += new System.EventHandler(this.cbeType_SelectedIndexChanged);
            // 
            // cbeVersion
            // 
            this.cbeVersion.Location = new System.Drawing.Point(735, 13);
            this.cbeVersion.MenuManager = this.ribbonControl1;
            this.cbeVersion.Name = "cbeVersion";
            this.cbeVersion.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbeVersion.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cbeVersion.Size = new System.Drawing.Size(147, 28);
            this.cbeVersion.StyleController = this.layoutControl1;
            this.cbeVersion.TabIndex = 8;
            this.cbeVersion.SelectedIndexChanged += new System.EventHandler(this.cbeVersion_SelectedIndexChanged);
            // 
            // btnSub
            // 
            this.btnSub.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSub.Appearance.Options.UseFont = true;
            this.btnSub.Location = new System.Drawing.Point(13, 13);
            this.btnSub.Name = "btnSub";
            this.btnSub.Size = new System.Drawing.Size(260, 32);
            this.btnSub.StyleController = this.layoutControl1;
            this.btnSub.TabIndex = 7;
            this.btnSub.Text = "提交数据库";
            this.btnSub.Click += new System.EventHandler(this.btnSub_Click);
            // 
            // gcEml
            // 
            this.gcEml.ContextMenuStrip = this.cmsGrid;
            this.gcEml.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gcEml.Location = new System.Drawing.Point(6, 58);
            this.gcEml.MainView = this.gvEml;
            this.gcEml.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gcEml.Name = "gcEml";
            this.gcEml.Size = new System.Drawing.Size(1380, 639);
            this.gcEml.TabIndex = 6;
            this.gcEml.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvEml});
            this.gcEml.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.gcEml_MouseDoubleClick);
            // 
            // gvEml
            // 
            this.gvEml.Appearance.FocusedRow.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.gvEml.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gvEml.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.Sequence,
            this.ChiName,
            this.EngName,
            this.UsingState,
            this.IsHexadecimal,
            this.Unit,
            this.IsStringLimit,
            this.StringRange,
            this.IsMaxMin,
            this.MinValue,
            this.MaxValue,
            this.IsSpecialFormat,
            this.FormatName});
            this.gvEml.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFullFocus;
            this.gvEml.GridControl = this.gcEml;
            this.gvEml.Name = "gvEml";
            this.gvEml.OptionsBehavior.Editable = false;
            this.gvEml.OptionsBehavior.ReadOnly = true;
            this.gvEml.OptionsCustomization.AllowColumnMoving = false;
            this.gvEml.OptionsFind.AlwaysVisible = true;
            this.gvEml.OptionsView.ShowGroupPanel = false;
            this.gvEml.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gvEml_MouseDown);
            // 
            // Sequence
            // 
            this.Sequence.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Sequence.AppearanceCell.Options.UseFont = true;
            this.Sequence.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Sequence.AppearanceHeader.Options.UseFont = true;
            this.Sequence.Caption = "顺序";
            this.Sequence.FieldName = "Sequence";
            this.Sequence.Name = "Sequence";
            this.Sequence.Visible = true;
            this.Sequence.VisibleIndex = 0;
            this.Sequence.Width = 50;
            // 
            // ChiName
            // 
            this.ChiName.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ChiName.AppearanceCell.Options.UseFont = true;
            this.ChiName.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ChiName.AppearanceHeader.Options.UseFont = true;
            this.ChiName.Caption = "中文列名";
            this.ChiName.FieldName = "ChiName";
            this.ChiName.Name = "ChiName";
            this.ChiName.Visible = true;
            this.ChiName.VisibleIndex = 1;
            this.ChiName.Width = 82;
            // 
            // EngName
            // 
            this.EngName.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EngName.AppearanceCell.Options.UseFont = true;
            this.EngName.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EngName.AppearanceHeader.Options.UseFont = true;
            this.EngName.Caption = "英文列名";
            this.EngName.FieldName = "EngName";
            this.EngName.Name = "EngName";
            this.EngName.Visible = true;
            this.EngName.VisibleIndex = 2;
            this.EngName.Width = 82;
            // 
            // UsingState
            // 
            this.UsingState.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UsingState.AppearanceCell.Options.UseFont = true;
            this.UsingState.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UsingState.AppearanceHeader.Options.UseFont = true;
            this.UsingState.Caption = "启用状态";
            this.UsingState.FieldName = "UsingState";
            this.UsingState.Name = "UsingState";
            this.UsingState.Visible = true;
            this.UsingState.VisibleIndex = 3;
            this.UsingState.Width = 82;
            // 
            // IsHexadecimal
            // 
            this.IsHexadecimal.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IsHexadecimal.AppearanceCell.Options.UseFont = true;
            this.IsHexadecimal.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IsHexadecimal.AppearanceHeader.Options.UseFont = true;
            this.IsHexadecimal.Caption = "是否启用16进制";
            this.IsHexadecimal.FieldName = "IsHexadecimal";
            this.IsHexadecimal.Name = "IsHexadecimal";
            this.IsHexadecimal.Visible = true;
            this.IsHexadecimal.VisibleIndex = 4;
            this.IsHexadecimal.Width = 103;
            // 
            // Unit
            // 
            this.Unit.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Unit.AppearanceCell.Options.UseFont = true;
            this.Unit.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Unit.AppearanceHeader.Options.UseFont = true;
            this.Unit.Caption = "单位";
            this.Unit.FieldName = "Unit";
            this.Unit.Name = "Unit";
            this.Unit.Visible = true;
            this.Unit.VisibleIndex = 5;
            this.Unit.Width = 50;
            // 
            // IsStringLimit
            // 
            this.IsStringLimit.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IsStringLimit.AppearanceCell.Options.UseFont = true;
            this.IsStringLimit.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IsStringLimit.AppearanceHeader.Options.UseFont = true;
            this.IsStringLimit.Caption = "字符串限制";
            this.IsStringLimit.FieldName = "IsStringLimit";
            this.IsStringLimit.Name = "IsStringLimit";
            this.IsStringLimit.Visible = true;
            this.IsStringLimit.VisibleIndex = 6;
            this.IsStringLimit.Width = 81;
            // 
            // StringRange
            // 
            this.StringRange.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StringRange.AppearanceCell.Options.UseFont = true;
            this.StringRange.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StringRange.AppearanceHeader.Options.UseFont = true;
            this.StringRange.Caption = "字符串范围";
            this.StringRange.FieldName = "StringRange";
            this.StringRange.Name = "StringRange";
            this.StringRange.Visible = true;
            this.StringRange.VisibleIndex = 7;
            this.StringRange.Width = 81;
            // 
            // IsMaxMin
            // 
            this.IsMaxMin.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IsMaxMin.AppearanceCell.Options.UseFont = true;
            this.IsMaxMin.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IsMaxMin.AppearanceHeader.Options.UseFont = true;
            this.IsMaxMin.Caption = "最大最小值";
            this.IsMaxMin.FieldName = "IsMaxMin";
            this.IsMaxMin.Name = "IsMaxMin";
            this.IsMaxMin.Visible = true;
            this.IsMaxMin.VisibleIndex = 8;
            this.IsMaxMin.Width = 81;
            // 
            // MinValue
            // 
            this.MinValue.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinValue.AppearanceCell.Options.UseFont = true;
            this.MinValue.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinValue.AppearanceHeader.Options.UseFont = true;
            this.MinValue.Caption = "最小值";
            this.MinValue.FieldName = "MinValue";
            this.MinValue.Name = "MinValue";
            this.MinValue.Visible = true;
            this.MinValue.VisibleIndex = 9;
            this.MinValue.Width = 81;
            // 
            // MaxValue
            // 
            this.MaxValue.Caption = "最大值";
            this.MaxValue.FieldName = "MaxValue";
            this.MaxValue.Name = "MaxValue";
            this.MaxValue.Visible = true;
            this.MaxValue.VisibleIndex = 10;
            // 
            // IsSpecialFormat
            // 
            this.IsSpecialFormat.Caption = "指定格式";
            this.IsSpecialFormat.FieldName = "IsSpecialFormat";
            this.IsSpecialFormat.Name = "IsSpecialFormat";
            this.IsSpecialFormat.Visible = true;
            this.IsSpecialFormat.VisibleIndex = 11;
            this.IsSpecialFormat.Width = 64;
            // 
            // FormatName
            // 
            this.FormatName.Caption = "格式";
            this.FormatName.FieldName = "FormatName";
            this.FormatName.Name = "FormatName";
            this.FormatName.Visible = true;
            this.FormatName.VisibleIndex = 12;
            this.FormatName.Width = 99;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem2,
            this.lciVersion,
            this.emptySpaceItem1,
            this.layoutControlItem3});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 3, 3);
            this.layoutControlGroup1.Size = new System.Drawing.Size(1392, 703);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.gcEml;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 52);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(1386, 645);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.btnSub;
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.layoutControlItem2.Size = new System.Drawing.Size(280, 52);
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextVisible = false;
            // 
            // lciVersion
            // 
            this.lciVersion.Control = this.cbeVersion;
            this.lciVersion.Enabled = false;
            this.lciVersion.Location = new System.Drawing.Point(627, 0);
            this.lciVersion.Name = "lciVersion";
            this.lciVersion.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.lciVersion.Size = new System.Drawing.Size(262, 52);
            this.lciVersion.Text = "版本选择：";
            this.lciVersion.TextAlignMode = DevExpress.XtraLayout.TextAlignModeItem.AutoSize;
            this.lciVersion.TextSize = new System.Drawing.Size(90, 22);
            this.lciVersion.TextToControlDistance = 5;
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.Location = new System.Drawing.Point(889, 0);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(497, 52);
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem3.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem3.Control = this.cbeType;
            this.layoutControlItem3.Location = new System.Drawing.Point(280, 0);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.layoutControlItem3.Size = new System.Drawing.Size(347, 52);
            this.layoutControlItem3.Text = "用例表类型选择：";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(144, 24);
            // 
            // ribbonPage1
            // 
            this.ribbonPage1.Name = "ribbonPage1";
            this.ribbonPage1.Text = "ribbonPage1";
            // 
            // EmlColEditor
            // 
            this.Appearance.Options.UseFont = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1392, 787);
            this.Controls.Add(this.layoutControl1);
            this.Controls.Add(this.ribbonControl1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "EmlColEditor";
            this.Ribbon = this.ribbonControl1;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "用例表列编辑";
            this.cmsGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cbeType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbeVersion.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcEml)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvEml)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciVersion)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip cmsGrid;
        private System.Windows.Forms.ToolStripMenuItem tsmiAdd;
        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1;
        private DevExpress.LookAndFeel.DefaultLookAndFeel DLAF;
        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage1;
        private DevExpress.XtraGrid.GridControl gcEml;
        private DevExpress.XtraGrid.Views.Grid.GridView gvEml;
        private DevExpress.XtraGrid.Columns.GridColumn Sequence;
        private DevExpress.XtraGrid.Columns.GridColumn ChiName;
        private DevExpress.XtraGrid.Columns.GridColumn EngName;
        private DevExpress.XtraGrid.Columns.GridColumn UsingState;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private System.Windows.Forms.ToolStripMenuItem tsmiModify;
        private System.Windows.Forms.ToolStripMenuItem tsmiInsert;
        private DevExpress.XtraGrid.Columns.GridColumn IsHexadecimal;
        private DevExpress.XtraGrid.Columns.GridColumn IsStringLimit;
        private DevExpress.XtraGrid.Columns.GridColumn StringRange;
        private DevExpress.XtraGrid.Columns.GridColumn IsMaxMin;
        private DevExpress.XtraGrid.Columns.GridColumn MinValue;
        private DevExpress.XtraGrid.Columns.GridColumn IsSpecialFormat;
        private DevExpress.XtraGrid.Columns.GridColumn FormatName;
        private DevExpress.XtraGrid.Columns.GridColumn Unit;
        private DevExpress.XtraGrid.Columns.GridColumn MaxValue;
        private DevExpress.XtraEditors.SimpleButton btnSub;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraEditors.ComboBoxEdit cbeVersion;
        private DevExpress.XtraLayout.LayoutControlItem lciVersion;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraEditors.ComboBoxEdit cbeType;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
    }
}