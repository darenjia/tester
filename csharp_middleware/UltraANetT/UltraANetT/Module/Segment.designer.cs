namespace UltraANetT.Module
{
    partial class Segment
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
            this.DMSegment = new DevExpress.XtraBars.Docking.DockManager(this.components);
            this.hideContainerRight = new DevExpress.XtraBars.Docking.AutoHideContainer();
            this.dpSegment = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel1_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.btnSubmit = new DevExpress.XtraEditors.SimpleButton();
            this.txtBaud = new DevExpress.XtraEditors.TextEdit();
            this.txtSegmentName = new DevExpress.XtraEditors.TextEdit();
            this.cmbSegmentType = new DevExpress.XtraEditors.ComboBoxEdit();
            this.seCorrespond = new DevExpress.XtraEditors.SpinEdit();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            this.simpleLabelItem1 = new DevExpress.XtraLayout.SimpleLabelItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.pcContainer = new DevExpress.XtraEditors.PanelControl();
            this.gcSegment = new DevExpress.XtraGrid.GridControl();
            this.CMSOrder = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiModify = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDel = new System.Windows.Forms.ToolStripMenuItem();
            this.gvSegment = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.SegmentName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Baud = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Correspond = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gvcbMaster = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.DMSegment)).BeginInit();
            this.hideContainerRight.SuspendLayout();
            this.dpSegment.SuspendLayout();
            this.dockPanel1_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtBaud.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSegmentName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbSegmentType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.seCorrespond.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.simpleLabelItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pcContainer)).BeginInit();
            this.pcContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcSegment)).BeginInit();
            this.CMSOrder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvSegment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvcbMaster)).BeginInit();
            this.SuspendLayout();
            // 
            // DMSegment
            // 
            this.DMSegment.AutoHideContainers.AddRange(new DevExpress.XtraBars.Docking.AutoHideContainer[] {
            this.hideContainerRight});
            this.DMSegment.Form = this;
            this.DMSegment.TopZIndexControls.AddRange(new string[] {
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
            this.hideContainerRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(236)))), ((int)(((byte)(239)))));
            this.hideContainerRight.Controls.Add(this.dpSegment);
            this.hideContainerRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.hideContainerRight.Location = new System.Drawing.Point(1002, 0);
            this.hideContainerRight.Name = "hideContainerRight";
            this.hideContainerRight.Size = new System.Drawing.Size(20, 551);
            // 
            // dpSegment
            // 
            this.dpSegment.Appearance.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dpSegment.Appearance.Options.UseFont = true;
            this.dpSegment.Controls.Add(this.dockPanel1_Container);
            this.dpSegment.Dock = DevExpress.XtraBars.Docking.DockingStyle.Right;
            this.dpSegment.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dpSegment.ID = new System.Guid("35a1b513-18d3-4042-aa30-ced23f42124f");
            this.dpSegment.Location = new System.Drawing.Point(0, 0);
            this.dpSegment.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.dpSegment.Name = "dpSegment";
            this.dpSegment.OriginalSize = new System.Drawing.Size(252, 200);
            this.dpSegment.SavedDock = DevExpress.XtraBars.Docking.DockingStyle.Right;
            this.dpSegment.SavedIndex = 0;
            this.dpSegment.Size = new System.Drawing.Size(252, 551);
            this.dpSegment.Text = "网段信息";
            this.dpSegment.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            // 
            // dockPanel1_Container
            // 
            this.dockPanel1_Container.Controls.Add(this.layoutControl1);
            this.dockPanel1_Container.Location = new System.Drawing.Point(4, 23);
            this.dockPanel1_Container.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.dockPanel1_Container.Name = "dockPanel1_Container";
            this.dockPanel1_Container.Size = new System.Drawing.Size(244, 524);
            this.dockPanel1_Container.TabIndex = 0;
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomization = false;
            this.layoutControl1.Controls.Add(this.btnSubmit);
            this.layoutControl1.Controls.Add(this.txtBaud);
            this.layoutControl1.Controls.Add(this.txtSegmentName);
            this.layoutControl1.Controls.Add(this.cmbSegmentType);
            this.layoutControl1.Controls.Add(this.seCorrespond);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(1126, 165, 250, 350);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(244, 524);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // btnSubmit
            // 
            this.btnSubmit.Appearance.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSubmit.Appearance.Options.UseFont = true;
            this.btnSubmit.Location = new System.Drawing.Point(6, 142);
            this.btnSubmit.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnSubmit.MinimumSize = new System.Drawing.Size(0, 27);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(232, 27);
            this.btnSubmit.StyleController = this.layoutControl1;
            this.btnSubmit.TabIndex = 9;
            this.btnSubmit.Text = "提交";
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // txtBaud
            // 
            this.txtBaud.EditValue = "";
            this.txtBaud.Location = new System.Drawing.Point(74, 74);
            this.txtBaud.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtBaud.Name = "txtBaud";
            this.txtBaud.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtBaud.Properties.Appearance.Options.UseFont = true;
            this.txtBaud.Size = new System.Drawing.Size(120, 24);
            this.txtBaud.StyleController = this.layoutControl1;
            this.txtBaud.TabIndex = 5;
            this.txtBaud.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtBaud_MouseUp);
            // 
            // txtSegmentName
            // 
            this.txtSegmentName.EditValue = "";
            this.txtSegmentName.Location = new System.Drawing.Point(74, 40);
            this.txtSegmentName.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtSegmentName.Name = "txtSegmentName";
            this.txtSegmentName.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtSegmentName.Properties.Appearance.Options.UseFont = true;
            this.txtSegmentName.Size = new System.Drawing.Size(164, 24);
            this.txtSegmentName.StyleController = this.layoutControl1;
            this.txtSegmentName.TabIndex = 4;
            this.txtSegmentName.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtSegmentName_MouseUp);
            // 
            // cmbSegmentType
            // 
            this.cmbSegmentType.EditValue = "";
            this.cmbSegmentType.Location = new System.Drawing.Point(74, 6);
            this.cmbSegmentType.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cmbSegmentType.Name = "cmbSegmentType";
            this.cmbSegmentType.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cmbSegmentType.Properties.Appearance.Options.UseFont = true;
            this.cmbSegmentType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbSegmentType.Properties.Items.AddRange(new object[] {
            "CAN",
            "LIN"});
            this.cmbSegmentType.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbSegmentType.Size = new System.Drawing.Size(164, 24);
            this.cmbSegmentType.StyleController = this.layoutControl1;
            this.cmbSegmentType.TabIndex = 4;
            this.cmbSegmentType.MouseUp += new System.Windows.Forms.MouseEventHandler(this.cmbSegmentType_MouseUp);
            // 
            // seCorrespond
            // 
            this.seCorrespond.EditValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.seCorrespond.Location = new System.Drawing.Point(74, 108);
            this.seCorrespond.Name = "seCorrespond";
            this.seCorrespond.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.seCorrespond.Properties.Appearance.Options.UseFont = true;
            this.seCorrespond.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.seCorrespond.Properties.EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Default;
            this.seCorrespond.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.None;
            this.seCorrespond.Properties.MaxLength = 2;
            this.seCorrespond.Properties.MaxValue = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.seCorrespond.Properties.MinValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.seCorrespond.Size = new System.Drawing.Size(164, 24);
            this.seCorrespond.StyleController = this.layoutControl1;
            this.seCorrespond.TabIndex = 4;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlGroup1.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem2,
            this.layoutControlItem6,
            this.simpleLabelItem1,
            this.layoutControlItem3,
            this.layoutControlItem4});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(1, 1, 1, 1);
            this.layoutControlGroup1.Size = new System.Drawing.Size(244, 524);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.layoutControlItem1.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem1.BestFitWeight = 50;
            this.layoutControlItem1.Control = this.txtSegmentName;
            this.layoutControlItem1.CustomizationFormText = "网段：";
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 34);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 5, 5);
            this.layoutControlItem1.Size = new System.Drawing.Size(242, 34);
            this.layoutControlItem1.Text = "网段名称：";
            this.layoutControlItem1.TextSize = new System.Drawing.Size(65, 19);
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem2.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem2.Control = this.txtBaud;
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 68);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 5, 5);
            this.layoutControlItem2.Size = new System.Drawing.Size(198, 34);
            this.layoutControlItem2.Text = "波特率：";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(65, 17);
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.Control = this.btnSubmit;
            this.layoutControlItem6.Location = new System.Drawing.Point(0, 136);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 5, 5);
            this.layoutControlItem6.Size = new System.Drawing.Size(242, 386);
            this.layoutControlItem6.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem6.TextVisible = false;
            // 
            // simpleLabelItem1
            // 
            this.simpleLabelItem1.AllowHotTrack = false;
            this.simpleLabelItem1.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.simpleLabelItem1.AppearanceItemCaption.Options.UseFont = true;
            this.simpleLabelItem1.Location = new System.Drawing.Point(198, 68);
            this.simpleLabelItem1.Name = "simpleLabelItem1";
            this.simpleLabelItem1.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 5, 5);
            this.simpleLabelItem1.Size = new System.Drawing.Size(44, 34);
            this.simpleLabelItem1.Text = "kbps";
            this.simpleLabelItem1.TextAlignMode = DevExpress.XtraLayout.TextAlignModeItem.AutoSize;
            this.simpleLabelItem1.TextSize = new System.Drawing.Size(29, 19);
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.cmbSegmentType;
            this.layoutControlItem3.CustomizationFormText = "网段：";
            this.layoutControlItem3.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 5, 5);
            this.layoutControlItem3.Size = new System.Drawing.Size(242, 34);
            this.layoutControlItem3.Text = "网段类型：";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(65, 19);
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.seCorrespond;
            this.layoutControlItem4.CustomizationFormText = "网段：";
            this.layoutControlItem4.Location = new System.Drawing.Point(0, 102);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 5, 5);
            this.layoutControlItem4.Size = new System.Drawing.Size(242, 34);
            this.layoutControlItem4.Text = "对应值：";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(65, 19);
            // 
            // pcContainer
            // 
            this.pcContainer.Controls.Add(this.gcSegment);
            this.pcContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pcContainer.Location = new System.Drawing.Point(0, 0);
            this.pcContainer.Name = "pcContainer";
            this.pcContainer.Size = new System.Drawing.Size(1002, 551);
            this.pcContainer.TabIndex = 1;
            // 
            // gcSegment
            // 
            this.gcSegment.ContextMenuStrip = this.CMSOrder;
            this.gcSegment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcSegment.Location = new System.Drawing.Point(2, 2);
            this.gcSegment.MainView = this.gvSegment;
            this.gcSegment.Name = "gcSegment";
            this.gcSegment.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.gvcbMaster});
            this.gcSegment.Size = new System.Drawing.Size(998, 547);
            this.gcSegment.TabIndex = 3;
            this.gcSegment.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvSegment});
            this.gcSegment.DoubleClick += new System.EventHandler(this.gcSegment_DoubleClick);
            // 
            // CMSOrder
            // 
            this.CMSOrder.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.CMSOrder.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAdd,
            this.tsmiModify,
            this.tsmiDel});
            this.CMSOrder.Name = "CMSDel";
            this.CMSOrder.Size = new System.Drawing.Size(125, 70);
            // 
            // tsmiAdd
            // 
            this.tsmiAdd.Name = "tsmiAdd";
            this.tsmiAdd.Size = new System.Drawing.Size(124, 22);
            this.tsmiAdd.Text = "添加网段";
            this.tsmiAdd.Click += new System.EventHandler(this.tsmiAdd_Click);
            // 
            // tsmiModify
            // 
            this.tsmiModify.Name = "tsmiModify";
            this.tsmiModify.Size = new System.Drawing.Size(124, 22);
            this.tsmiModify.Text = "修改网段";
            this.tsmiModify.Click += new System.EventHandler(this.tsmiModify_Click);
            // 
            // tsmiDel
            // 
            this.tsmiDel.Enabled = false;
            this.tsmiDel.Name = "tsmiDel";
            this.tsmiDel.Size = new System.Drawing.Size(124, 22);
            this.tsmiDel.Text = "删除网段";
            this.tsmiDel.Click += new System.EventHandler(this.tsmiDel_Click);
            // 
            // gvSegment
            // 
            this.gvSegment.Appearance.FilterPanel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvSegment.Appearance.FilterPanel.Options.UseFont = true;
            this.gvSegment.Appearance.FocusedRow.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.gvSegment.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gvSegment.Appearance.GroupPanel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvSegment.Appearance.GroupPanel.Options.UseFont = true;
            this.gvSegment.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.SegmentName,
            this.Baud,
            this.Correspond});
            this.gvSegment.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFullFocus;
            this.gvSegment.GridControl = this.gcSegment;
            this.gvSegment.GroupPanelText = "拖 拽 列 头 以 按 照 此 列 分 组...";
            this.gvSegment.Name = "gvSegment";
            this.gvSegment.OptionsBehavior.Editable = false;
            this.gvSegment.OptionsBehavior.ReadOnly = true;
            this.gvSegment.OptionsCustomization.AllowColumnMoving = false;
            this.gvSegment.OptionsFind.AlwaysVisible = true;
            this.gvSegment.OptionsView.ShowGroupPanel = false;
            this.gvSegment.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gvSegment_RowClick);
            this.gvSegment.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gvSegment_MouseDown);
            // 
            // SegmentName
            // 
            this.SegmentName.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SegmentName.AppearanceCell.Options.UseFont = true;
            this.SegmentName.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SegmentName.AppearanceHeader.Options.UseFont = true;
            this.SegmentName.Caption = "网段";
            this.SegmentName.FieldName = "SegmentName";
            this.SegmentName.Name = "SegmentName";
            this.SegmentName.Visible = true;
            this.SegmentName.VisibleIndex = 0;
            // 
            // Baud
            // 
            this.Baud.Caption = "波特率";
            this.Baud.FieldName = "Baud";
            this.Baud.Name = "Baud";
            this.Baud.Visible = true;
            this.Baud.VisibleIndex = 1;
            // 
            // Correspond
            // 
            this.Correspond.Caption = "对应值";
            this.Correspond.FieldName = "Correspond";
            this.Correspond.Name = "Correspond";
            this.Correspond.Visible = true;
            this.Correspond.VisibleIndex = 2;
            // 
            // gvcbMaster
            // 
            this.gvcbMaster.AutoHeight = false;
            this.gvcbMaster.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.gvcbMaster.Name = "gvcbMaster";
            // 
            // Segment
            // 
            this.Appearance.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Appearance.Options.UseFont = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pcContainer);
            this.Controls.Add(this.hideContainerRight);
            this.Name = "Segment";
            this.Size = new System.Drawing.Size(1022, 551);
            ((System.ComponentModel.ISupportInitialize)(this.DMSegment)).EndInit();
            this.hideContainerRight.ResumeLayout(false);
            this.dpSegment.ResumeLayout(false);
            this.dockPanel1_Container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtBaud.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSegmentName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbSegmentType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.seCorrespond.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.simpleLabelItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pcContainer)).EndInit();
            this.pcContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcSegment)).EndInit();
            this.CMSOrder.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gvSegment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvcbMaster)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraBars.Docking.DockManager DMSegment;
        private DevExpress.XtraBars.Docking.DockPanel dpSegment;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel1_Container;
        private DevExpress.XtraEditors.PanelControl pcContainer;
        private DevExpress.XtraGrid.GridControl gcSegment;
        private DevExpress.XtraGrid.Views.Grid.GridView gvSegment;
        private DevExpress.XtraGrid.Columns.GridColumn SegmentName;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox gvcbMaster;
        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraEditors.SimpleButton btnSubmit;
        private DevExpress.XtraEditors.TextEdit txtBaud;
        private DevExpress.XtraEditors.TextEdit txtSegmentName;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private System.Windows.Forms.ContextMenuStrip CMSOrder;
        private System.Windows.Forms.ToolStripMenuItem tsmiAdd;
        private System.Windows.Forms.ToolStripMenuItem tsmiModify;
        private System.Windows.Forms.ToolStripMenuItem tsmiDel;
        private DevExpress.XtraGrid.Columns.GridColumn Baud;
        private DevExpress.XtraLayout.SimpleLabelItem simpleLabelItem1;
        private DevExpress.XtraBars.Docking.AutoHideContainer hideContainerRight;
        private DevExpress.XtraEditors.ComboBoxEdit cmbSegmentType;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraGrid.Columns.GridColumn Correspond;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraEditors.SpinEdit seCorrespond;
    }
}
