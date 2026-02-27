namespace UltraANetT.Module
{
    partial class ExapChapter
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
            this.DMExapChapter = new DevExpress.XtraBars.Docking.DockManager(this.components);
            this.hideContainerRight = new DevExpress.XtraBars.Docking.AutoHideContainer();
            this.dpNodeConfigurationBox = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel1_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.com_TestType = new DevExpress.XtraEditors.ComboBoxEdit();
            this.btnSubmit = new DevExpress.XtraEditors.SimpleButton();
            this.txtName = new DevExpress.XtraEditors.TextEdit();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.pcContainer = new DevExpress.XtraEditors.PanelControl();
            this.gcExapChapter = new DevExpress.XtraGrid.GridControl();
            this.CMSOrder = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiModify = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDel = new System.Windows.Forms.ToolStripMenuItem();
            this.gvExapChapter = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.Num = new DevExpress.XtraGrid.Columns.GridColumn();
            this.ChapterName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.TestType = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gvcbMaster = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.DMExapChapter)).BeginInit();
            this.hideContainerRight.SuspendLayout();
            this.dpNodeConfigurationBox.SuspendLayout();
            this.dockPanel1_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.com_TestType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pcContainer)).BeginInit();
            this.pcContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcExapChapter)).BeginInit();
            this.CMSOrder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvExapChapter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvcbMaster)).BeginInit();
            this.SuspendLayout();
            // 
            // DMExapChapter
            // 
            this.DMExapChapter.AutoHideContainers.AddRange(new DevExpress.XtraBars.Docking.AutoHideContainer[] {
            this.hideContainerRight});
            this.DMExapChapter.Form = this;
            this.DMExapChapter.TopZIndexControls.AddRange(new string[] {
            "DevExpress.XtraBars.BarDockControl",
            "DevExpress.XtraBars.StandaloneBarDockControl",
            "System.Windows.Forms.StatusBar",
            "System.Windows.Forms.MenuStrip",
            "System.Windows.Forms.StatusStrip",
            "DevExpress.XtraBars.Ribbon.RibbonStatusBar",
            "DevExpress.XtraBars.Ribbon.RibbonControl",
            "DevExpress.XtraBars.Navigation.OfficeNavigationBar",
            "DevExpress.XtraBars.Navigation.TileNavPane"});
            // 
            // hideContainerRight
            // 
            this.hideContainerRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.hideContainerRight.Controls.Add(this.dpNodeConfigurationBox);
            this.hideContainerRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.hideContainerRight.Location = new System.Drawing.Point(998, 0);
            this.hideContainerRight.Name = "hideContainerRight";
            this.hideContainerRight.Size = new System.Drawing.Size(24, 551);
            // 
            // dpNodeConfigurationBox
            // 
            this.dpNodeConfigurationBox.Appearance.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dpNodeConfigurationBox.Appearance.Options.UseFont = true;
            this.dpNodeConfigurationBox.Controls.Add(this.dockPanel1_Container);
            this.dpNodeConfigurationBox.Dock = DevExpress.XtraBars.Docking.DockingStyle.Right;
            this.dpNodeConfigurationBox.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dpNodeConfigurationBox.ID = new System.Guid("35a1b513-18d3-4042-aa30-ced23f42124f");
            this.dpNodeConfigurationBox.Location = new System.Drawing.Point(0, 0);
            this.dpNodeConfigurationBox.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.dpNodeConfigurationBox.Name = "dpNodeConfigurationBox";
            this.dpNodeConfigurationBox.OriginalSize = new System.Drawing.Size(252, 200);
            this.dpNodeConfigurationBox.SavedDock = DevExpress.XtraBars.Docking.DockingStyle.Right;
            this.dpNodeConfigurationBox.SavedIndex = 0;
            this.dpNodeConfigurationBox.Size = new System.Drawing.Size(252, 551);
            this.dpNodeConfigurationBox.Text = "章节名称信息";
            this.dpNodeConfigurationBox.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            // 
            // dockPanel1_Container
            // 
            this.dockPanel1_Container.Controls.Add(this.layoutControl1);
            this.dockPanel1_Container.Location = new System.Drawing.Point(4, 39);
            this.dockPanel1_Container.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.dockPanel1_Container.Name = "dockPanel1_Container";
            this.dockPanel1_Container.Size = new System.Drawing.Size(244, 508);
            this.dockPanel1_Container.TabIndex = 0;
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomization = false;
            this.layoutControl1.Controls.Add(this.com_TestType);
            this.layoutControl1.Controls.Add(this.btnSubmit);
            this.layoutControl1.Controls.Add(this.txtName);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(1126, 165, 250, 350);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(244, 508);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // com_TestType
            // 
            this.com_TestType.Location = new System.Drawing.Point(74, 6);
            this.com_TestType.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.com_TestType.Name = "com_TestType";
            this.com_TestType.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.com_TestType.Properties.Appearance.Options.UseFont = true;
            this.com_TestType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.com_TestType.Size = new System.Drawing.Size(164, 24);
            this.com_TestType.StyleController = this.layoutControl1;
            this.com_TestType.TabIndex = 10;
            this.com_TestType.MouseUp += new System.Windows.Forms.MouseEventHandler(this.com_TestType_MouseUp);
            // 
            // btnSubmit
            // 
            this.btnSubmit.Appearance.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSubmit.Appearance.Options.UseFont = true;
            this.btnSubmit.Location = new System.Drawing.Point(6, 74);
            this.btnSubmit.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.btnSubmit.MinimumSize = new System.Drawing.Size(0, 18);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(232, 24);
            this.btnSubmit.StyleController = this.layoutControl1;
            this.btnSubmit.TabIndex = 9;
            this.btnSubmit.Text = "提交";
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // txtName
            // 
            this.txtName.EditValue = "";
            this.txtName.Location = new System.Drawing.Point(74, 40);
            this.txtName.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.txtName.Name = "txtName";
            this.txtName.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtName.Properties.Appearance.Options.UseFont = true;
            this.txtName.Size = new System.Drawing.Size(164, 24);
            this.txtName.StyleController = this.layoutControl1;
            this.txtName.TabIndex = 4;
            this.txtName.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtName_MouseUp);
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlGroup1.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem6,
            this.layoutControlItem2});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(1, 1, 1, 1);
            this.layoutControlGroup1.Size = new System.Drawing.Size(244, 508);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.layoutControlItem1.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem1.BestFitWeight = 50;
            this.layoutControlItem1.Control = this.txtName;
            this.layoutControlItem1.CustomizationFormText = "网段：";
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 34);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 5, 5);
            this.layoutControlItem1.Size = new System.Drawing.Size(242, 34);
            this.layoutControlItem1.Text = "章节名称：";
            this.layoutControlItem1.TextSize = new System.Drawing.Size(65, 19);
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.Control = this.btnSubmit;
            this.layoutControlItem6.Location = new System.Drawing.Point(0, 68);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 5, 5);
            this.layoutControlItem6.Size = new System.Drawing.Size(242, 438);
            this.layoutControlItem6.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem6.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.com_TestType;
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 5, 5);
            this.layoutControlItem2.Size = new System.Drawing.Size(242, 34);
            this.layoutControlItem2.Text = "测试类型：";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(65, 19);
            // 
            // pcContainer
            // 
            this.pcContainer.Controls.Add(this.gcExapChapter);
            this.pcContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pcContainer.Location = new System.Drawing.Point(0, 0);
            this.pcContainer.Name = "pcContainer";
            this.pcContainer.Size = new System.Drawing.Size(998, 551);
            this.pcContainer.TabIndex = 1;
            // 
            // gcExapChapter
            // 
            this.gcExapChapter.ContextMenuStrip = this.CMSOrder;
            this.gcExapChapter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcExapChapter.Location = new System.Drawing.Point(2, 2);
            this.gcExapChapter.MainView = this.gvExapChapter;
            this.gcExapChapter.Name = "gcExapChapter";
            this.gcExapChapter.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.gvcbMaster});
            this.gcExapChapter.Size = new System.Drawing.Size(994, 547);
            this.gcExapChapter.TabIndex = 3;
            this.gcExapChapter.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvExapChapter});
            this.gcExapChapter.DoubleClick += new System.EventHandler(this.gcExapChapter_DoubleClick);
            this.gcExapChapter.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.gcExapChapter_MouseDoubleClick);
            // 
            // CMSOrder
            // 
            this.CMSOrder.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.CMSOrder.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAdd,
            this.tsmiModify,
            this.tsmiDel});
            this.CMSOrder.Name = "CMSDel";
            this.CMSOrder.Size = new System.Drawing.Size(149, 70);
            // 
            // tsmiAdd
            // 
            this.tsmiAdd.Name = "tsmiAdd";
            this.tsmiAdd.Size = new System.Drawing.Size(148, 22);
            this.tsmiAdd.Text = "添加章节配置";
            this.tsmiAdd.Click += new System.EventHandler(this.tsmiAdd_Click);
            // 
            // tsmiModify
            // 
            this.tsmiModify.Name = "tsmiModify";
            this.tsmiModify.Size = new System.Drawing.Size(148, 22);
            this.tsmiModify.Text = "修改章节配置";
            this.tsmiModify.Click += new System.EventHandler(this.tsmiModify_Click);
            // 
            // tsmiDel
            // 
            this.tsmiDel.Enabled = false;
            this.tsmiDel.Name = "tsmiDel";
            this.tsmiDel.Size = new System.Drawing.Size(148, 22);
            this.tsmiDel.Text = "删除章节配置";
            this.tsmiDel.Click += new System.EventHandler(this.tsmiDel_Click);
            // 
            // gvExapChapter
            // 
            this.gvExapChapter.Appearance.FilterPanel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvExapChapter.Appearance.FilterPanel.Options.UseFont = true;
            this.gvExapChapter.Appearance.FocusedRow.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.gvExapChapter.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gvExapChapter.Appearance.GroupPanel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvExapChapter.Appearance.GroupPanel.Options.UseFont = true;
            this.gvExapChapter.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.Num,
            this.ChapterName,
            this.TestType});
            this.gvExapChapter.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFullFocus;
            this.gvExapChapter.GridControl = this.gcExapChapter;
            this.gvExapChapter.GroupPanelText = "拖 拽 列 头 以 按 照 此 列 分 组...";
            this.gvExapChapter.Name = "gvExapChapter";
            this.gvExapChapter.OptionsBehavior.Editable = false;
            this.gvExapChapter.OptionsBehavior.ReadOnly = true;
            this.gvExapChapter.OptionsCustomization.AllowColumnMoving = false;
            this.gvExapChapter.OptionsFind.AlwaysVisible = true;
            this.gvExapChapter.OptionsView.ShowGroupPanel = false;
            this.gvExapChapter.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gvExapChapter_RowClick);
            this.gvExapChapter.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gvExapChapter_MouseDown);
            // 
            // Num
            // 
            this.Num.Caption = "序号";
            this.Num.FieldName = "Num";
            this.Num.Name = "Num";
            this.Num.Visible = true;
            this.Num.VisibleIndex = 0;
            // 
            // ChapterName
            // 
            this.ChapterName.Caption = "章节名称";
            this.ChapterName.FieldName = "ChapterName";
            this.ChapterName.Name = "ChapterName";
            this.ChapterName.Visible = true;
            this.ChapterName.VisibleIndex = 1;
            // 
            // TestType
            // 
            this.TestType.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TestType.AppearanceCell.Options.UseFont = true;
            this.TestType.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TestType.AppearanceHeader.Options.UseFont = true;
            this.TestType.Caption = "测试类型";
            this.TestType.FieldName = "TestType";
            this.TestType.Name = "TestType";
            this.TestType.Visible = true;
            this.TestType.VisibleIndex = 2;
            // 
            // gvcbMaster
            // 
            this.gvcbMaster.AutoHeight = false;
            this.gvcbMaster.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.gvcbMaster.Name = "gvcbMaster";
            // 
            // ExapChapter
            // 
            this.Appearance.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Appearance.Options.UseFont = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pcContainer);
            this.Controls.Add(this.hideContainerRight);
            this.Name = "ExapChapter";
            this.Size = new System.Drawing.Size(1022, 551);
            ((System.ComponentModel.ISupportInitialize)(this.DMExapChapter)).EndInit();
            this.hideContainerRight.ResumeLayout(false);
            this.dpNodeConfigurationBox.ResumeLayout(false);
            this.dockPanel1_Container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.com_TestType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pcContainer)).EndInit();
            this.pcContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcExapChapter)).EndInit();
            this.CMSOrder.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gvExapChapter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvcbMaster)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraBars.Docking.DockManager DMExapChapter;
        private DevExpress.XtraBars.Docking.DockPanel dpNodeConfigurationBox;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel1_Container;
        private DevExpress.XtraEditors.PanelControl pcContainer;
        private DevExpress.XtraGrid.GridControl gcExapChapter;
        private DevExpress.XtraGrid.Views.Grid.GridView gvExapChapter;
        private DevExpress.XtraGrid.Columns.GridColumn TestType;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox gvcbMaster;
        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraEditors.SimpleButton btnSubmit;
        private DevExpress.XtraEditors.TextEdit txtName;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private System.Windows.Forms.ContextMenuStrip CMSOrder;
        private System.Windows.Forms.ToolStripMenuItem tsmiAdd;
        private System.Windows.Forms.ToolStripMenuItem tsmiModify;
        private System.Windows.Forms.ToolStripMenuItem tsmiDel;
        private DevExpress.XtraBars.Docking.AutoHideContainer hideContainerRight;
        private DevExpress.XtraGrid.Columns.GridColumn ChapterName;
        private DevExpress.XtraGrid.Columns.GridColumn Num;
        private DevExpress.XtraEditors.ComboBoxEdit com_TestType;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
    }
}
