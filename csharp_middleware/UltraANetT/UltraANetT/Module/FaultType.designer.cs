namespace UltraANetT.Module
{
    partial class FaultType
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
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.gcFault = new DevExpress.XtraGrid.GridControl();
            this.CMSFault = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiModify = new System.Windows.Forms.ToolStripMenuItem();
            this.Del = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiChechk = new System.Windows.Forms.ToolStripMenuItem();
            this.gvFault = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.ErrorType = new DevExpress.XtraGrid.Columns.GridColumn();
            this.IsMessage = new DevExpress.XtraGrid.Columns.GridColumn();
            this.MessageCount = new DevExpress.XtraGrid.Columns.GridColumn();
            this.MsgInformation = new DevExpress.XtraGrid.Columns.GridColumn();
            this.CheckInfor = new DevExpress.XtraGrid.Columns.GridColumn();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.dockManager1 = new DevExpress.XtraBars.Docking.DockManager(this.components);
            this.hideContainerRight = new DevExpress.XtraBars.Docking.AutoHideContainer();
            this.dpFault = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel1_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.layoutControl2 = new DevExpress.XtraLayout.LayoutControl();
            this.btnOK = new DevExpress.XtraEditors.SimpleButton();
            this.txtCount = new DevExpress.XtraEditors.TextEdit();
            this.ceIMessage = new DevExpress.XtraEditors.CheckEdit();
            this.txtType = new DevExpress.XtraEditors.TextEdit();
            this.layoutControlGroup2 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcFault)).BeginInit();
            this.CMSFault.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvFault)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).BeginInit();
            this.hideContainerRight.SuspendLayout();
            this.dpFault.SuspendLayout();
            this.dockPanel1_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl2)).BeginInit();
            this.layoutControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtCount.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ceIMessage.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomization = false;
            this.layoutControl1.Controls.Add(this.gcFault);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(720, 343);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // gcFault
            // 
            this.gcFault.ContextMenuStrip = this.CMSFault;
            this.gcFault.Location = new System.Drawing.Point(12, 12);
            this.gcFault.MainView = this.gvFault;
            this.gcFault.Name = "gcFault";
            this.gcFault.Size = new System.Drawing.Size(696, 319);
            this.gcFault.TabIndex = 4;
            this.gcFault.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvFault});
            this.gcFault.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.gcFault_MouseDoubleClick);
            // 
            // CMSFault
            // 
            this.CMSFault.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.CMSFault.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAdd,
            this.tsmiModify,
            this.Del,
            this.tsmiChechk});
            this.CMSFault.Name = "CMSFault";
            this.CMSFault.Size = new System.Drawing.Size(149, 92);
            // 
            // tsmiAdd
            // 
            this.tsmiAdd.Name = "tsmiAdd";
            this.tsmiAdd.Size = new System.Drawing.Size(148, 22);
            this.tsmiAdd.Text = "增加故障信息";
            this.tsmiAdd.Click += new System.EventHandler(this.tsmiAdd_Click);
            // 
            // tsmiModify
            // 
            this.tsmiModify.Name = "tsmiModify";
            this.tsmiModify.Size = new System.Drawing.Size(148, 22);
            this.tsmiModify.Text = "修改故障信息";
            this.tsmiModify.Click += new System.EventHandler(this.tsmiModify_Click);
            // 
            // Del
            // 
            this.Del.Name = "Del";
            this.Del.Size = new System.Drawing.Size(148, 22);
            this.Del.Text = "删除故障信息";
            this.Del.Click += new System.EventHandler(this.Del_Click);
            // 
            // tsmiChechk
            // 
            this.tsmiChechk.Name = "tsmiChechk";
            this.tsmiChechk.Size = new System.Drawing.Size(148, 22);
            this.tsmiChechk.Text = "查看故障信息";
            this.tsmiChechk.Click += new System.EventHandler(this.tsmiCheck_Click);
            // 
            // gvFault
            // 
            this.gvFault.Appearance.FocusedRow.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.gvFault.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gvFault.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.ErrorType,
            this.IsMessage,
            this.MessageCount,
            this.MsgInformation,
            this.CheckInfor});
            this.gvFault.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFullFocus;
            this.gvFault.GridControl = this.gcFault;
            this.gvFault.Name = "gvFault";
            this.gvFault.OptionsBehavior.Editable = false;
            this.gvFault.OptionsBehavior.ReadOnly = true;
            this.gvFault.OptionsCustomization.AllowColumnMoving = false;
            this.gvFault.OptionsFind.AlwaysVisible = true;
            this.gvFault.OptionsView.ShowGroupPanel = false;
            this.gvFault.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gvFault_MouseDown);
            // 
            // ErrorType
            // 
            this.ErrorType.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ErrorType.AppearanceCell.Options.UseFont = true;
            this.ErrorType.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ErrorType.AppearanceHeader.Options.UseFont = true;
            this.ErrorType.Caption = "故障类型";
            this.ErrorType.FieldName = "ErrorType";
            this.ErrorType.Name = "ErrorType";
            this.ErrorType.Visible = true;
            this.ErrorType.VisibleIndex = 0;
            // 
            // IsMessage
            // 
            this.IsMessage.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IsMessage.AppearanceCell.Options.UseFont = true;
            this.IsMessage.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IsMessage.AppearanceHeader.Options.UseFont = true;
            this.IsMessage.Caption = "是否有DTC相关报文";
            this.IsMessage.FieldName = "IsMessage";
            this.IsMessage.Name = "IsMessage";
            this.IsMessage.Visible = true;
            this.IsMessage.VisibleIndex = 1;
            // 
            // MessageCount
            // 
            this.MessageCount.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MessageCount.AppearanceCell.Options.UseFont = true;
            this.MessageCount.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MessageCount.AppearanceHeader.Options.UseFont = true;
            this.MessageCount.Caption = "DTC报文的项数";
            this.MessageCount.FieldName = "MessageCount";
            this.MessageCount.Name = "MessageCount";
            this.MessageCount.Visible = true;
            this.MessageCount.VisibleIndex = 2;
            // 
            // MsgInformation
            // 
            this.MsgInformation.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MsgInformation.AppearanceCell.Options.UseFont = true;
            this.MsgInformation.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MsgInformation.AppearanceHeader.Options.UseFont = true;
            this.MsgInformation.Caption = "DTC各项数对应的信息";
            this.MsgInformation.FieldName = "MsgInformation";
            this.MsgInformation.Name = "MsgInformation";
            // 
            // CheckInfor
            // 
            this.CheckInfor.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CheckInfor.AppearanceCell.Options.UseFont = true;
            this.CheckInfor.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CheckInfor.AppearanceHeader.Options.UseFont = true;
            this.CheckInfor.Caption = "每项基本信息";
            this.CheckInfor.FieldName = "CheckInfor";
            this.CheckInfor.Name = "CheckInfor";
            this.CheckInfor.Visible = true;
            this.CheckInfor.VisibleIndex = 3;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "layoutControlGroup1";
            this.layoutControlGroup1.Size = new System.Drawing.Size(720, 343);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.gcFault;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(700, 323);
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
            this.hideContainerRight.Controls.Add(this.dpFault);
            this.hideContainerRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.hideContainerRight.Location = new System.Drawing.Point(720, 0);
            this.hideContainerRight.Name = "hideContainerRight";
            this.hideContainerRight.Size = new System.Drawing.Size(24, 343);
            // 
            // dpFault
            // 
            this.dpFault.Controls.Add(this.dockPanel1_Container);
            this.dpFault.Dock = DevExpress.XtraBars.Docking.DockingStyle.Right;
            this.dpFault.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dpFault.ID = new System.Guid("2f42419f-fc90-4dec-b17f-647b5341ad74");
            this.dpFault.Location = new System.Drawing.Point(0, 0);
            this.dpFault.Name = "dpFault";
            this.dpFault.OriginalSize = new System.Drawing.Size(200, 200);
            this.dpFault.SavedDock = DevExpress.XtraBars.Docking.DockingStyle.Right;
            this.dpFault.SavedIndex = 0;
            this.dpFault.Size = new System.Drawing.Size(200, 343);
            this.dpFault.Text = "故障信息";
            this.dpFault.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            // 
            // dockPanel1_Container
            // 
            this.dockPanel1_Container.Controls.Add(this.layoutControl2);
            this.dockPanel1_Container.Location = new System.Drawing.Point(4, 39);
            this.dockPanel1_Container.Name = "dockPanel1_Container";
            this.dockPanel1_Container.Size = new System.Drawing.Size(192, 300);
            this.dockPanel1_Container.TabIndex = 0;
            // 
            // layoutControl2
            // 
            this.layoutControl2.AllowCustomization = false;
            this.layoutControl2.Controls.Add(this.btnOK);
            this.layoutControl2.Controls.Add(this.txtCount);
            this.layoutControl2.Controls.Add(this.ceIMessage);
            this.layoutControl2.Controls.Add(this.txtType);
            this.layoutControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl2.Location = new System.Drawing.Point(0, 0);
            this.layoutControl2.Name = "layoutControl2";
            this.layoutControl2.Root = this.layoutControlGroup2;
            this.layoutControl2.Size = new System.Drawing.Size(192, 300);
            this.layoutControl2.TabIndex = 0;
            this.layoutControl2.Text = "layoutControl2";
            // 
            // btnOK
            // 
            this.btnOK.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Appearance.Options.UseFont = true;
            this.btnOK.Location = new System.Drawing.Point(13, 159);
            this.btnOK.MaximumSize = new System.Drawing.Size(0, 24);
            this.btnOK.MinimumSize = new System.Drawing.Size(0, 24);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(166, 24);
            this.btnOK.StyleController = this.layoutControl2;
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "确定";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // txtCount
            // 
            this.txtCount.Location = new System.Drawing.Point(100, 105);
            this.txtCount.Name = "txtCount";
            this.txtCount.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCount.Properties.Appearance.Options.UseFont = true;
            this.txtCount.Size = new System.Drawing.Size(79, 24);
            this.txtCount.StyleController = this.layoutControl2;
            this.txtCount.TabIndex = 6;
            this.txtCount.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtCount_MouseUp);
            // 
            // ceIMessage
            // 
            this.ceIMessage.Location = new System.Drawing.Point(13, 64);
            this.ceIMessage.Name = "ceIMessage";
            this.ceIMessage.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ceIMessage.Properties.Appearance.Options.UseFont = true;
            this.ceIMessage.Properties.Caption = "是否有DTC相关报文";
            this.ceIMessage.Size = new System.Drawing.Size(166, 21);
            this.ceIMessage.StyleController = this.layoutControl2;
            this.ceIMessage.TabIndex = 5;
            this.ceIMessage.CheckedChanged += new System.EventHandler(this.ceIMessage_CheckedChanged);
            // 
            // txtType
            // 
            this.txtType.Location = new System.Drawing.Point(100, 20);
            this.txtType.Name = "txtType";
            this.txtType.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtType.Properties.Appearance.Options.UseFont = true;
            this.txtType.Size = new System.Drawing.Size(79, 24);
            this.txtType.StyleController = this.layoutControl2;
            this.txtType.TabIndex = 4;
            this.txtType.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtType_MouseUp);
            // 
            // layoutControlGroup2
            // 
            this.layoutControlGroup2.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup2.GroupBordersVisible = false;
            this.layoutControlGroup2.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem2,
            this.layoutControlItem3,
            this.layoutControlItem4,
            this.layoutControlItem5});
            this.layoutControlGroup2.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup2.Name = "layoutControlGroup2";
            this.layoutControlGroup2.Size = new System.Drawing.Size(192, 300);
            this.layoutControlGroup2.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem2.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem2.Control = this.txtType;
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 10, 10);
            this.layoutControlItem2.Size = new System.Drawing.Size(172, 44);
            this.layoutControlItem2.Text = "故障类型：";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(84, 17);
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.ceIMessage;
            this.layoutControlItem3.Location = new System.Drawing.Point(0, 44);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 10, 10);
            this.layoutControlItem3.Size = new System.Drawing.Size(172, 41);
            this.layoutControlItem3.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem3.TextVisible = false;
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem4.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem4.Control = this.txtCount;
            this.layoutControlItem4.Location = new System.Drawing.Point(0, 85);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 10, 10);
            this.layoutControlItem4.Size = new System.Drawing.Size(172, 44);
            this.layoutControlItem4.Text = "DTC报文项数：";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(84, 17);
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.Control = this.btnOK;
            this.layoutControlItem5.Location = new System.Drawing.Point(0, 129);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 20, 5);
            this.layoutControlItem5.Size = new System.Drawing.Size(172, 151);
            this.layoutControlItem5.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem5.TextVisible = false;
            // 
            // FaultType
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.layoutControl1);
            this.Controls.Add(this.hideContainerRight);
            this.Name = "FaultType";
            this.Size = new System.Drawing.Size(744, 343);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcFault)).EndInit();
            this.CMSFault.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gvFault)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).EndInit();
            this.hideContainerRight.ResumeLayout(false);
            this.dpFault.ResumeLayout(false);
            this.dockPanel1_Container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl2)).EndInit();
            this.layoutControl2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtCount.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ceIMessage.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraGrid.GridControl gcFault;
        private DevExpress.XtraGrid.Views.Grid.GridView gvFault;
        private DevExpress.XtraGrid.Columns.GridColumn ErrorType;
        private DevExpress.XtraGrid.Columns.GridColumn IsMessage;
        private DevExpress.XtraGrid.Columns.GridColumn MessageCount;
        private DevExpress.XtraGrid.Columns.GridColumn MsgInformation;
        private DevExpress.XtraGrid.Columns.GridColumn CheckInfor;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraBars.Docking.DockManager dockManager1;
        private DevExpress.XtraBars.Docking.DockPanel dpFault;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel1_Container;
        private DevExpress.XtraLayout.LayoutControl layoutControl2;
        private DevExpress.XtraEditors.SimpleButton btnOK;
        private DevExpress.XtraEditors.TextEdit txtCount;
        private DevExpress.XtraEditors.CheckEdit ceIMessage;
        private DevExpress.XtraEditors.TextEdit txtType;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraBars.Docking.AutoHideContainer hideContainerRight;
        private System.Windows.Forms.ContextMenuStrip CMSFault;
        private System.Windows.Forms.ToolStripMenuItem tsmiAdd;
        private System.Windows.Forms.ToolStripMenuItem tsmiModify;
        private System.Windows.Forms.ToolStripMenuItem Del;
        private System.Windows.Forms.ToolStripMenuItem tsmiChechk;
    }
}
