namespace UltraANetT.Form
{
    partial class EmlDTC
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
            this.components = new System.ComponentModel.Container();
            this.ribbonControl1 = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.gcEmlDTC = new DevExpress.XtraGrid.GridControl();
            this.CMSOrder = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiModify = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDel = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.gvEmlDTC = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.dockManager1 = new DevExpress.XtraBars.Docking.DockManager(this.components);
            this.hideContainerRight = new DevExpress.XtraBars.Docking.AutoHideContainer();
            this.dpEmlDTC = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel1_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.lcDpEmlDTC = new DevExpress.XtraLayout.LayoutControl();
            this.lcgDpEmlDTC = new DevExpress.XtraLayout.LayoutControlGroup();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcEmlDTC)).BeginInit();
            this.CMSOrder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvEmlDTC)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).BeginInit();
            this.hideContainerRight.SuspendLayout();
            this.dpEmlDTC.SuspendLayout();
            this.dockPanel1_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lcDpEmlDTC)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lcgDpEmlDTC)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbonControl1
            // 
            this.ribbonControl1.ExpandCollapseItem.Id = 0;
            this.ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbonControl1.ExpandCollapseItem});
            this.ribbonControl1.Location = new System.Drawing.Point(0, 0);
            this.ribbonControl1.MaxItemId = 1;
            this.ribbonControl1.Name = "ribbonControl1";
            this.ribbonControl1.Size = new System.Drawing.Size(868, 32);
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomization = false;
            this.layoutControl1.Controls.Add(this.gcEmlDTC);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 32);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(844, 411);
            this.layoutControl1.TabIndex = 1;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // gcEmlDTC
            // 
            this.gcEmlDTC.ContextMenuStrip = this.CMSOrder;
            this.gcEmlDTC.Location = new System.Drawing.Point(12, 12);
            this.gcEmlDTC.MainView = this.gvEmlDTC;
            this.gcEmlDTC.Name = "gcEmlDTC";
            this.gcEmlDTC.Size = new System.Drawing.Size(820, 387);
            this.gcEmlDTC.TabIndex = 4;
            this.gcEmlDTC.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvEmlDTC});
            this.gcEmlDTC.DoubleClick += new System.EventHandler(this.gcEmlDTC_DoubleClick);
            // 
            // CMSOrder
            // 
            this.CMSOrder.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.CMSOrder.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAdd,
            this.tsmiModify,
            this.tsmiDel,
            this.tsmiCheck});
            this.CMSOrder.Name = "CMSOrder";
            this.CMSOrder.Size = new System.Drawing.Size(149, 92);
            // 
            // tsmiAdd
            // 
            this.tsmiAdd.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tsmiAdd.Name = "tsmiAdd";
            this.tsmiAdd.Size = new System.Drawing.Size(148, 22);
            this.tsmiAdd.Text = "增加";
            this.tsmiAdd.Visible = false;
            this.tsmiAdd.Click += new System.EventHandler(this.tsmiAdd_Click);
            // 
            // tsmiModify
            // 
            this.tsmiModify.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tsmiModify.Name = "tsmiModify";
            this.tsmiModify.Size = new System.Drawing.Size(148, 22);
            this.tsmiModify.Text = "绑定DTC信息";
            this.tsmiModify.Click += new System.EventHandler(this.tsmiModify_Click);
            // 
            // tsmiDel
            // 
            this.tsmiDel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tsmiDel.Name = "tsmiDel";
            this.tsmiDel.Size = new System.Drawing.Size(148, 22);
            this.tsmiDel.Text = "删除";
            this.tsmiDel.Visible = false;
            this.tsmiDel.Click += new System.EventHandler(this.tsmiDel_Click);
            // 
            // tsmiCheck
            // 
            this.tsmiCheck.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tsmiCheck.Name = "tsmiCheck";
            this.tsmiCheck.Size = new System.Drawing.Size(148, 22);
            this.tsmiCheck.Text = "查看评价信息";
            this.tsmiCheck.Visible = false;
            this.tsmiCheck.Click += new System.EventHandler(this.tsmiCheck_Click);
            // 
            // gvEmlDTC
            // 
            this.gvEmlDTC.Appearance.FocusedRow.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.gvEmlDTC.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gvEmlDTC.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFullFocus;
            this.gvEmlDTC.GridControl = this.gcEmlDTC;
            this.gvEmlDTC.Name = "gvEmlDTC";
            this.gvEmlDTC.OptionsBehavior.Editable = false;
            this.gvEmlDTC.OptionsBehavior.ReadOnly = true;
            this.gvEmlDTC.OptionsCustomization.AllowColumnMoving = false;
            this.gvEmlDTC.OptionsFind.AlwaysVisible = true;
            this.gvEmlDTC.OptionsView.ShowGroupPanel = false;
            this.gvEmlDTC.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gvSegment_RowClick);
            this.gvEmlDTC.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gvEmlDTC_MouseDown);
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "layoutControlGroup1";
            this.layoutControlGroup1.Size = new System.Drawing.Size(844, 411);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.gcEmlDTC;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(824, 391);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // dockManager1
            // 
            this.dockManager1.AutoHideContainers.AddRange(new DevExpress.XtraBars.Docking.AutoHideContainer[] {
            this.hideContainerRight});
            this.dockManager1.Form = this;
            this.dockManager1.TopZIndexControls.AddRange(new string[] {
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
            this.hideContainerRight.Controls.Add(this.dpEmlDTC);
            this.hideContainerRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.hideContainerRight.Location = new System.Drawing.Point(844, 32);
            this.hideContainerRight.Name = "hideContainerRight";
            this.hideContainerRight.Size = new System.Drawing.Size(24, 411);
            // 
            // dpEmlDTC
            // 
            this.dpEmlDTC.Controls.Add(this.dockPanel1_Container);
            this.dpEmlDTC.Dock = DevExpress.XtraBars.Docking.DockingStyle.Right;
            this.dpEmlDTC.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dpEmlDTC.ID = new System.Guid("2c25fbec-7106-46bb-8cfa-6a5eee536fc0");
            this.dpEmlDTC.Location = new System.Drawing.Point(610, 32);
            this.dpEmlDTC.Name = "dpEmlDTC";
            this.dpEmlDTC.OriginalSize = new System.Drawing.Size(234, 200);
            this.dpEmlDTC.SavedDock = DevExpress.XtraBars.Docking.DockingStyle.Right;
            this.dpEmlDTC.SavedIndex = 0;
            this.dpEmlDTC.Size = new System.Drawing.Size(234, 411);
            this.dpEmlDTC.Text = "用例信息";
            this.dpEmlDTC.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            // 
            // dockPanel1_Container
            // 
            this.dockPanel1_Container.Controls.Add(this.lcDpEmlDTC);
            this.dockPanel1_Container.Location = new System.Drawing.Point(4, 39);
            this.dockPanel1_Container.Name = "dockPanel1_Container";
            this.dockPanel1_Container.Size = new System.Drawing.Size(226, 368);
            this.dockPanel1_Container.TabIndex = 0;
            // 
            // lcDpEmlDTC
            // 
            this.lcDpEmlDTC.AllowCustomization = false;
            this.lcDpEmlDTC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lcDpEmlDTC.Location = new System.Drawing.Point(0, 0);
            this.lcDpEmlDTC.Name = "lcDpEmlDTC";
            this.lcDpEmlDTC.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(667, 98, 250, 350);
            this.lcDpEmlDTC.Root = this.lcgDpEmlDTC;
            this.lcDpEmlDTC.Size = new System.Drawing.Size(226, 368);
            this.lcDpEmlDTC.TabIndex = 0;
            this.lcDpEmlDTC.Text = "layoutControl2";
            // 
            // lcgDpEmlDTC
            // 
            this.lcgDpEmlDTC.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.lcgDpEmlDTC.GroupBordersVisible = false;
            this.lcgDpEmlDTC.Location = new System.Drawing.Point(0, 0);
            this.lcgDpEmlDTC.Name = "lcgDpEmlDTC";
            this.lcgDpEmlDTC.Size = new System.Drawing.Size(226, 368);
            this.lcgDpEmlDTC.TextVisible = false;
            // 
            // EmlDTC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(868, 443);
            this.Controls.Add(this.layoutControl1);
            this.Controls.Add(this.hideContainerRight);
            this.Controls.Add(this.ribbonControl1);
            this.Name = "EmlDTC";
            this.Ribbon = this.ribbonControl1;
            this.RibbonVisibility = DevExpress.XtraBars.Ribbon.RibbonVisibility.Hidden;
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "用例信息";
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcEmlDTC)).EndInit();
            this.CMSOrder.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gvEmlDTC)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).EndInit();
            this.hideContainerRight.ResumeLayout(false);
            this.dpEmlDTC.ResumeLayout(false);
            this.dockPanel1_Container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lcDpEmlDTC)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lcgDpEmlDTC)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1;
        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraGrid.GridControl gcEmlDTC;
        private DevExpress.XtraGrid.Views.Grid.GridView gvEmlDTC;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraBars.Docking.DockManager dockManager1;
        private DevExpress.XtraBars.Docking.DockPanel dpEmlDTC;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel1_Container;
        private DevExpress.XtraLayout.LayoutControl lcDpEmlDTC;
        private DevExpress.XtraLayout.LayoutControlGroup lcgDpEmlDTC;
        private System.Windows.Forms.ContextMenuStrip CMSOrder;
        private System.Windows.Forms.ToolStripMenuItem tsmiAdd;
        private System.Windows.Forms.ToolStripMenuItem tsmiModify;
        private System.Windows.Forms.ToolStripMenuItem tsmiDel;
        private DevExpress.XtraBars.Docking.AutoHideContainer hideContainerRight;
        private System.Windows.Forms.ToolStripMenuItem tsmiCheck;
    }
}