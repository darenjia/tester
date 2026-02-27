namespace FileEditor.Form
{
    partial class CfgColEdit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CfgColEdit));
            this.ribbonControl1 = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.barStaticItem1 = new DevExpress.XtraBars.BarStaticItem();
            this.defaultLookAndFeel1 = new DevExpress.LookAndFeel.DefaultLookAndFeel();
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.cbeConfig = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cbVersion = new DevExpress.XtraEditors.ComboBoxEdit();
            this.btnUp = new DevExpress.XtraEditors.SimpleButton();
            this.gcExcelCol = new DevExpress.XtraGrid.GridControl();
            this.cmsOrder = new System.Windows.Forms.ContextMenuStrip();
            this.tsmiAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiInsert = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.gvExcelCol = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.Index = new DevExpress.XtraGrid.Columns.GridColumn();
            this.ChsName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.EngName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Result = new DevExpress.XtraGrid.Columns.GridColumn();
            this.ifHex = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Unit = new DevExpress.XtraGrid.Columns.GridColumn();
            this.IsStringLimit = new DevExpress.XtraGrid.Columns.GridColumn();
            this.StringRange = new DevExpress.XtraGrid.Columns.GridColumn();
            this.IsMaxMin = new DevExpress.XtraGrid.Columns.GridColumn();
            this.MinInt = new DevExpress.XtraGrid.Columns.GridColumn();
            this.MaxInt = new DevExpress.XtraGrid.Columns.GridColumn();
            this.ifFormat = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Format = new DevExpress.XtraGrid.Columns.GridColumn();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.lciVersion = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cbeConfig.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbVersion.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcExcelCol)).BeginInit();
            this.cmsOrder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvExcelCol)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciVersion)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbonControl1
            // 
            this.ribbonControl1.ExpandCollapseItem.Id = 0;
            this.ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbonControl1.ExpandCollapseItem,
            this.barStaticItem1});
            this.ribbonControl1.Location = new System.Drawing.Point(0, 0);
            this.ribbonControl1.MaxItemId = 2;
            this.ribbonControl1.Name = "ribbonControl1";
            this.ribbonControl1.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonControlStyle.MacOffice;
            this.ribbonControl1.Size = new System.Drawing.Size(1023, 55);
            this.ribbonControl1.ToolbarLocation = DevExpress.XtraBars.Ribbon.RibbonQuickAccessToolbarLocation.Hidden;
            // 
            // barStaticItem1
            // 
            this.barStaticItem1.Caption = "barStaticItem1";
            this.barStaticItem1.Id = 1;
            this.barStaticItem1.Name = "barStaticItem1";
            this.barStaticItem1.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // defaultLookAndFeel1
            // 
            this.defaultLookAndFeel1.LookAndFeel.SkinName = "Office 2016 Colorful";
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomization = false;
            this.layoutControl1.Controls.Add(this.cbeConfig);
            this.layoutControl1.Controls.Add(this.cbVersion);
            this.layoutControl1.Controls.Add(this.btnUp);
            this.layoutControl1.Controls.Add(this.gcExcelCol);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 55);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(414, 214, 250, 350);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(1023, 490);
            this.layoutControl1.TabIndex = 1;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // cbeConfig
            // 
            this.cbeConfig.Location = new System.Drawing.Point(265, 13);
            this.cbeConfig.Margin = new System.Windows.Forms.Padding(2);
            this.cbeConfig.MenuManager = this.ribbonControl1;
            this.cbeConfig.Name = "cbeConfig";
            this.cbeConfig.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbeConfig.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cbeConfig.Size = new System.Drawing.Size(115, 20);
            this.cbeConfig.StyleController = this.layoutControl1;
            this.cbeConfig.TabIndex = 5;
            this.cbeConfig.SelectedIndexChanged += new System.EventHandler(this.cbeConfig_SelectedIndexChanged);
            // 
            // cbVersion
            // 
            this.cbVersion.Location = new System.Drawing.Point(465, 13);
            this.cbVersion.Margin = new System.Windows.Forms.Padding(2);
            this.cbVersion.MenuManager = this.ribbonControl1;
            this.cbVersion.Name = "cbVersion";
            this.cbVersion.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbVersion.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cbVersion.Size = new System.Drawing.Size(87, 20);
            this.cbVersion.StyleController = this.layoutControl1;
            this.cbVersion.TabIndex = 4;
            this.cbVersion.SelectedIndexChanged += new System.EventHandler(this.cbVersion_SelectedIndexChanged);
            // 
            // btnUp
            // 
            this.btnUp.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnUp.Appearance.Options.UseFont = true;
            this.btnUp.Location = new System.Drawing.Point(13, 13);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(133, 22);
            this.btnUp.StyleController = this.layoutControl1;
            this.btnUp.TabIndex = 0;
            this.btnUp.Text = "提交数据库";
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // gcExcelCol
            // 
            this.gcExcelCol.ContextMenuStrip = this.cmsOrder;
            this.gcExcelCol.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.gcExcelCol.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gcExcelCol.Location = new System.Drawing.Point(5, 47);
            this.gcExcelCol.LookAndFeel.SkinName = "Office 2016 Colorful";
            this.gcExcelCol.MainView = this.gvExcelCol;
            this.gcExcelCol.Name = "gcExcelCol";
            this.gcExcelCol.Size = new System.Drawing.Size(1013, 438);
            this.gcExcelCol.TabIndex = 2;
            this.gcExcelCol.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvExcelCol});
            this.gcExcelCol.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.gcExcelCol_MouseDoubleClick);
            // 
            // cmsOrder
            // 
            this.cmsOrder.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cmsOrder.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAdd,
            this.tsmiInsert,
            this.tsmiUpdate});
            this.cmsOrder.Name = "cmsOrder";
            this.cmsOrder.Size = new System.Drawing.Size(101, 70);
            // 
            // tsmiAdd
            // 
            this.tsmiAdd.Enabled = false;
            this.tsmiAdd.Name = "tsmiAdd";
            this.tsmiAdd.Size = new System.Drawing.Size(100, 22);
            this.tsmiAdd.Text = "添加";
            this.tsmiAdd.Click += new System.EventHandler(this.tsmiAdd_Click);
            // 
            // tsmiInsert
            // 
            this.tsmiInsert.Enabled = false;
            this.tsmiInsert.Name = "tsmiInsert";
            this.tsmiInsert.Size = new System.Drawing.Size(100, 22);
            this.tsmiInsert.Text = "插入";
            this.tsmiInsert.Click += new System.EventHandler(this.tsmiInsert_Click);
            // 
            // tsmiUpdate
            // 
            this.tsmiUpdate.Enabled = false;
            this.tsmiUpdate.Name = "tsmiUpdate";
            this.tsmiUpdate.Size = new System.Drawing.Size(100, 22);
            this.tsmiUpdate.Text = "修改";
            this.tsmiUpdate.Click += new System.EventHandler(this.tsmiUpdate_Click);
            // 
            // gvExcelCol
            // 
            this.gvExcelCol.Appearance.ColumnFilterButton.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.gvExcelCol.Appearance.ColumnFilterButton.Options.UseFont = true;
            this.gvExcelCol.Appearance.ColumnFilterButtonActive.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.gvExcelCol.Appearance.ColumnFilterButtonActive.Options.UseFont = true;
            this.gvExcelCol.Appearance.FocusedRow.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.gvExcelCol.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gvExcelCol.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.Index,
            this.ChsName,
            this.EngName,
            this.Result,
            this.ifHex,
            this.Unit,
            this.IsStringLimit,
            this.StringRange,
            this.IsMaxMin,
            this.MinInt,
            this.MaxInt,
            this.ifFormat,
            this.Format});
            this.gvExcelCol.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFullFocus;
            this.gvExcelCol.GridControl = this.gcExcelCol;
            this.gvExcelCol.Name = "gvExcelCol";
            this.gvExcelCol.OptionsBehavior.Editable = false;
            this.gvExcelCol.OptionsBehavior.ReadOnly = true;
            this.gvExcelCol.OptionsCustomization.AllowColumnMoving = false;
            this.gvExcelCol.OptionsView.ShowGroupPanel = false;
            this.gvExcelCol.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gvExcelCol_MouseDown);
            // 
            // Index
            // 
            this.Index.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Index.AppearanceCell.Options.UseFont = true;
            this.Index.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Index.AppearanceHeader.Options.UseFont = true;
            this.Index.Caption = "序号";
            this.Index.FieldName = "Index";
            this.Index.Name = "Index";
            this.Index.Visible = true;
            this.Index.VisibleIndex = 0;
            // 
            // ChsName
            // 
            this.ChsName.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.ChsName.AppearanceCell.Options.UseFont = true;
            this.ChsName.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.ChsName.AppearanceHeader.Options.UseFont = true;
            this.ChsName.Caption = "中文列名";
            this.ChsName.FieldName = "ChsName";
            this.ChsName.Name = "ChsName";
            this.ChsName.Visible = true;
            this.ChsName.VisibleIndex = 1;
            // 
            // EngName
            // 
            this.EngName.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.EngName.AppearanceCell.Options.UseFont = true;
            this.EngName.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.EngName.AppearanceHeader.Options.UseFont = true;
            this.EngName.Caption = "英文列名";
            this.EngName.FieldName = "EngName";
            this.EngName.Name = "EngName";
            this.EngName.Visible = true;
            this.EngName.VisibleIndex = 2;
            // 
            // Result
            // 
            this.Result.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Result.AppearanceCell.Options.UseFont = true;
            this.Result.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Result.AppearanceHeader.Options.UseFont = true;
            this.Result.Caption = "启用状态";
            this.Result.FieldName = "Result";
            this.Result.Name = "Result";
            this.Result.Visible = true;
            this.Result.VisibleIndex = 3;
            // 
            // ifHex
            // 
            this.ifHex.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ifHex.AppearanceCell.Options.UseFont = true;
            this.ifHex.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ifHex.AppearanceHeader.Options.UseFont = true;
            this.ifHex.Caption = "是否启用16进制";
            this.ifHex.FieldName = "ifHex";
            this.ifHex.Name = "ifHex";
            this.ifHex.Visible = true;
            this.ifHex.VisibleIndex = 4;
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
            this.Unit.Width = 61;
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
            this.IsStringLimit.Width = 89;
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
            this.StringRange.Width = 73;
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
            this.IsMaxMin.Width = 73;
            // 
            // MinInt
            // 
            this.MinInt.Caption = "最小值";
            this.MinInt.FieldName = "MinInt";
            this.MinInt.Name = "MinInt";
            this.MinInt.Visible = true;
            this.MinInt.VisibleIndex = 9;
            this.MinInt.Width = 73;
            // 
            // MaxInt
            // 
            this.MaxInt.Caption = "最大值";
            this.MaxInt.FieldName = "MaxInt";
            this.MaxInt.Name = "MaxInt";
            this.MaxInt.Visible = true;
            this.MaxInt.VisibleIndex = 10;
            this.MaxInt.Width = 73;
            // 
            // ifFormat
            // 
            this.ifFormat.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ifFormat.AppearanceCell.Options.UseFont = true;
            this.ifFormat.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ifFormat.AppearanceHeader.Options.UseFont = true;
            this.ifFormat.Caption = "指定格式状态";
            this.ifFormat.FieldName = "ifFormat";
            this.ifFormat.Name = "ifFormat";
            this.ifFormat.Visible = true;
            this.ifFormat.VisibleIndex = 11;
            this.ifFormat.Width = 73;
            // 
            // Format
            // 
            this.Format.Caption = "指定格式";
            this.Format.FieldName = "Format";
            this.Format.Name = "Format";
            this.Format.Visible = true;
            this.Format.VisibleIndex = 12;
            this.Format.Width = 94;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.lciVersion,
            this.emptySpaceItem1,
            this.layoutControlItem3,
            this.layoutControlItem2});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 3, 3);
            this.layoutControlGroup1.Size = new System.Drawing.Size(1023, 490);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.gcExcelCol;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 42);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(1017, 442);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // lciVersion
            // 
            this.lciVersion.Control = this.cbVersion;
            this.lciVersion.Enabled = false;
            this.lciVersion.Location = new System.Drawing.Point(387, 0);
            this.lciVersion.Name = "lciVersion";
            this.lciVersion.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.lciVersion.Size = new System.Drawing.Size(172, 42);
            this.lciVersion.Text = "版本选择：";
            this.lciVersion.TextAlignMode = DevExpress.XtraLayout.TextAlignModeItem.AutoSize;
            this.lciVersion.TextSize = new System.Drawing.Size(60, 14);
            this.lciVersion.TextToControlDistance = 5;
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.Location = new System.Drawing.Point(559, 0);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(458, 42);
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem3.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem3.Control = this.cbeConfig;
            this.layoutControlItem3.Location = new System.Drawing.Point(153, 0);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.layoutControlItem3.Size = new System.Drawing.Size(234, 42);
            this.layoutControlItem3.Text = "配置表类型选择：";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(96, 17);
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.btnUp;
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.layoutControlItem2.Size = new System.Drawing.Size(153, 42);
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextVisible = false;
            // 
            // CfgColEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1023, 545);
            this.Controls.Add(this.layoutControl1);
            this.Controls.Add(this.ribbonControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CfgColEdit";
            this.Ribbon = this.ribbonControl1;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "配置表列编辑";
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cbeConfig.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbVersion.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcExcelCol)).EndInit();
            this.cmsOrder.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gvExcelCol)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lciVersion)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1;
        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraGrid.GridControl gcExcelCol;
        private DevExpress.XtraGrid.Views.Grid.GridView gvExcelCol;
        private DevExpress.XtraGrid.Columns.GridColumn ChsName;
        private DevExpress.XtraGrid.Columns.GridColumn EngName;
        private DevExpress.XtraGrid.Columns.GridColumn Index;
        private DevExpress.XtraGrid.Columns.GridColumn Result;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraBars.BarStaticItem barStaticItem1;
        private System.Windows.Forms.ContextMenuStrip cmsOrder;
        private DevExpress.LookAndFeel.DefaultLookAndFeel defaultLookAndFeel1;
        private System.Windows.Forms.ToolStripMenuItem tsmiUpdate;
        private System.Windows.Forms.ToolStripMenuItem tsmiInsert;
        private System.Windows.Forms.ToolStripMenuItem tsmiAdd;
        private DevExpress.XtraGrid.Columns.GridColumn Unit;
        private DevExpress.XtraGrid.Columns.GridColumn ifHex;
        private DevExpress.XtraGrid.Columns.GridColumn StringRange;
        private DevExpress.XtraGrid.Columns.GridColumn IsMaxMin;
        private DevExpress.XtraGrid.Columns.GridColumn Format;
        private DevExpress.XtraGrid.Columns.GridColumn IsStringLimit;
        private DevExpress.XtraGrid.Columns.GridColumn ifFormat;
        private DevExpress.XtraEditors.ComboBoxEdit cbVersion;
        private DevExpress.XtraEditors.SimpleButton btnUp;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraLayout.LayoutControlItem lciVersion;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraEditors.ComboBoxEdit cbeConfig;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraGrid.Columns.GridColumn MinInt;
        private DevExpress.XtraGrid.Columns.GridColumn MaxInt;
    }
}