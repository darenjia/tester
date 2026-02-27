namespace FileEditor.Form
{
    partial class FaultTypeCount
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FaultTypeCount));
            this.ribbonControl1 = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.barStaticItem1 = new DevExpress.XtraBars.BarStaticItem();
            this.defaultLookAndFeel1 = new DevExpress.LookAndFeel.DefaultLookAndFeel(this.components);
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.gcDTC = new DevExpress.XtraGrid.GridControl();
            this.cmsOrder = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiInsert = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDel = new System.Windows.Forms.ToolStripMenuItem();
            this.gvDTC = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.DTCChsName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.DTCEngName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.ifHex = new DevExpress.XtraGrid.Columns.GridColumn();
            this.IsInt = new DevExpress.XtraGrid.Columns.GridColumn();
            this.IsString = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Unit = new DevExpress.XtraGrid.Columns.GridColumn();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcDTC)).BeginInit();
            this.cmsOrder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvDTC)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
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
            this.ribbonControl1.Size = new System.Drawing.Size(1015, 55);
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
            this.layoutControl1.Controls.Add(this.gcDTC);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 55);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(414, 214, 250, 350);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(1015, 486);
            this.layoutControl1.TabIndex = 1;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // gcDTC
            // 
            this.gcDTC.ContextMenuStrip = this.cmsOrder;
            this.gcDTC.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.gcDTC.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gcDTC.Location = new System.Drawing.Point(5, 5);
            this.gcDTC.LookAndFeel.SkinName = "Office 2016 Colorful";
            this.gcDTC.MainView = this.gvDTC;
            this.gcDTC.Name = "gcDTC";
            this.gcDTC.Size = new System.Drawing.Size(1005, 476);
            this.gcDTC.TabIndex = 2;
            this.gcDTC.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvDTC});
            this.gcDTC.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.gcDTC_MouseDoubleClick);
            // 
            // cmsOrder
            // 
            this.cmsOrder.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cmsOrder.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAdd,
            this.tsmiInsert,
            this.tsmiUpdate,
            this.tsmiDel});
            this.cmsOrder.Name = "cmsOrder";
            this.cmsOrder.Size = new System.Drawing.Size(101, 92);
            // 
            // tsmiAdd
            // 
            this.tsmiAdd.Name = "tsmiAdd";
            this.tsmiAdd.Size = new System.Drawing.Size(100, 22);
            this.tsmiAdd.Text = "添加";
            this.tsmiAdd.Click += new System.EventHandler(this.tsmiAdd_Click);
            // 
            // tsmiInsert
            // 
            this.tsmiInsert.Name = "tsmiInsert";
            this.tsmiInsert.Size = new System.Drawing.Size(100, 22);
            this.tsmiInsert.Text = "插入";
            this.tsmiInsert.Click += new System.EventHandler(this.tsmiInsert_Click);
            // 
            // tsmiUpdate
            // 
            this.tsmiUpdate.Name = "tsmiUpdate";
            this.tsmiUpdate.Size = new System.Drawing.Size(100, 22);
            this.tsmiUpdate.Text = "修改";
            this.tsmiUpdate.Click += new System.EventHandler(this.tsmiUpdate_Click);
            // 
            // tsmiDel
            // 
            this.tsmiDel.Name = "tsmiDel";
            this.tsmiDel.Size = new System.Drawing.Size(100, 22);
            this.tsmiDel.Text = "删除";
            this.tsmiDel.Click += new System.EventHandler(this.tsmiDel_Click);
            // 
            // gvDTC
            // 
            this.gvDTC.Appearance.ColumnFilterButton.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.gvDTC.Appearance.ColumnFilterButton.Options.UseFont = true;
            this.gvDTC.Appearance.ColumnFilterButtonActive.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.gvDTC.Appearance.ColumnFilterButtonActive.Options.UseFont = true;
            this.gvDTC.Appearance.FocusedRow.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.gvDTC.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gvDTC.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.DTCChsName,
            this.DTCEngName,
            this.ifHex,
            this.IsInt,
            this.IsString,
            this.Unit});
            this.gvDTC.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFullFocus;
            this.gvDTC.GridControl = this.gcDTC;
            this.gvDTC.Name = "gvDTC";
            this.gvDTC.OptionsBehavior.Editable = false;
            this.gvDTC.OptionsBehavior.ReadOnly = true;
            this.gvDTC.OptionsCustomization.AllowColumnMoving = false;
            this.gvDTC.OptionsMenu.EnableColumnMenu = false;
            this.gvDTC.OptionsMenu.EnableFooterMenu = false;
            this.gvDTC.OptionsMenu.EnableGroupPanelMenu = false;
            this.gvDTC.OptionsView.ShowGroupPanel = false;
            this.gvDTC.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gvExcelCol_MouseDown);
            // 
            // DTCChsName
            // 
            this.DTCChsName.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.DTCChsName.AppearanceCell.Options.UseFont = true;
            this.DTCChsName.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.DTCChsName.AppearanceHeader.Options.UseFont = true;
            this.DTCChsName.Caption = "故障类型输入项名";
            this.DTCChsName.FieldName = "DTCChsName";
            this.DTCChsName.Name = "DTCChsName";
            this.DTCChsName.Visible = true;
            this.DTCChsName.VisibleIndex = 0;
            // 
            // DTCEngName
            // 
            this.DTCEngName.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.DTCEngName.AppearanceCell.Options.UseFont = true;
            this.DTCEngName.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.DTCEngName.AppearanceHeader.Options.UseFont = true;
            this.DTCEngName.Caption = "故障类型输入项英文名";
            this.DTCEngName.FieldName = "DTCEngName";
            this.DTCEngName.Name = "DTCEngName";
            this.DTCEngName.Visible = true;
            this.DTCEngName.VisibleIndex = 1;
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
            this.ifHex.VisibleIndex = 2;
            // 
            // IsInt
            // 
            this.IsInt.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IsInt.AppearanceCell.Options.UseFont = true;
            this.IsInt.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IsInt.AppearanceHeader.Options.UseFont = true;
            this.IsInt.Caption = "是否为十进制数";
            this.IsInt.FieldName = "IsInt";
            this.IsInt.Name = "IsInt";
            this.IsInt.Visible = true;
            this.IsInt.VisibleIndex = 3;
            this.IsInt.Width = 89;
            // 
            // IsString
            // 
            this.IsString.Caption = "是否为字符串";
            this.IsString.FieldName = "IsString";
            this.IsString.Name = "IsString";
            this.IsString.Visible = true;
            this.IsString.VisibleIndex = 4;
            // 
            // Unit
            // 
            this.Unit.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Unit.AppearanceCell.Options.UseFont = true;
            this.Unit.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Unit.AppearanceHeader.Options.UseFont = true;
            this.Unit.Caption = "故障类型输入项单位";
            this.Unit.FieldName = "Unit";
            this.Unit.Name = "Unit";
            this.Unit.Visible = true;
            this.Unit.VisibleIndex = 5;
            this.Unit.Width = 61;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 3, 3);
            this.layoutControlGroup1.Size = new System.Drawing.Size(1015, 486);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.gcDTC;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(1009, 480);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // FaultTypeCount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1015, 541);
            this.Controls.Add(this.layoutControl1);
            this.Controls.Add(this.ribbonControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FaultTypeCount";
            this.Ribbon = this.ribbonControl1;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FaultTypeCount_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcDTC)).EndInit();
            this.cmsOrder.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gvDTC)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1;
        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraGrid.GridControl gcDTC;
        private DevExpress.XtraGrid.Views.Grid.GridView gvDTC;
        private DevExpress.XtraGrid.Columns.GridColumn DTCChsName;
        private DevExpress.XtraGrid.Columns.GridColumn DTCEngName;
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
        private DevExpress.XtraGrid.Columns.GridColumn IsInt;
        private System.Windows.Forms.ToolStripMenuItem tsmiDel;
        private DevExpress.XtraGrid.Columns.GridColumn IsString;
    }
}