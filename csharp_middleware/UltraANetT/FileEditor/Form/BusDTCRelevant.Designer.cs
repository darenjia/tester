namespace FileEditor.Form
{
    partial class BusDTCRelevant
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
            this.defaultLookAndFeel1 = new DevExpress.LookAndFeel.DefaultLookAndFeel(this.components);
            this.ribbonControl1 = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.gcDTC = new DevExpress.XtraGrid.GridControl();
            this.CMStable = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiModify = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDel = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiFault = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiMsg = new System.Windows.Forms.ToolStripMenuItem();
            this.gvDTC = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.DUTname = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Cddname = new DevExpress.XtraGrid.Columns.GridColumn();
            this.RequestID = new DevExpress.XtraGrid.Columns.GridColumn();
            this.RespondID = new DevExpress.XtraGrid.Columns.GridColumn();
            this.InitTimeofDiag = new DevExpress.XtraGrid.Columns.GridColumn();
            this.FaultInfo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.MessageInfo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.fault = new DevExpress.XtraGrid.Columns.GridColumn();
            this.message = new DevExpress.XtraGrid.Columns.GridColumn();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.dockManager1 = new DevExpress.XtraBars.Docking.DockManager(this.components);
            this.hideContainerRight = new DevExpress.XtraBars.Docking.AutoHideContainer();
            this.dpAssess = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel1_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.layoutControl2 = new DevExpress.XtraLayout.LayoutControl();
            this.txtDiagTime = new DevExpress.XtraEditors.TextEdit();
            this.btnOk = new DevExpress.XtraEditors.SimpleButton();
            this.txtRespondID = new DevExpress.XtraEditors.TextEdit();
            this.txtRequestID = new DevExpress.XtraEditors.TextEdit();
            this.txtCddName = new DevExpress.XtraEditors.TextEdit();
            this.txtDUTname = new DevExpress.XtraEditors.TextEdit();
            this.layoutControlGroup2 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem7 = new DevExpress.XtraLayout.LayoutControlItem();
            this.DLAF = new DevExpress.LookAndFeel.DefaultLookAndFeel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcDTC)).BeginInit();
            this.CMStable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvDTC)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).BeginInit();
            this.hideContainerRight.SuspendLayout();
            this.dpAssess.SuspendLayout();
            this.dockPanel1_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl2)).BeginInit();
            this.layoutControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtDiagTime.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRespondID.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRequestID.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCddName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDUTname.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).BeginInit();
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
            this.ribbonControl1.Size = new System.Drawing.Size(869, 50);
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomization = false;
            this.layoutControl1.Controls.Add(this.gcDTC);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 50);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(849, 395);
            this.layoutControl1.TabIndex = 1;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // gcDTC
            // 
            this.gcDTC.ContextMenuStrip = this.CMStable;
            this.gcDTC.Location = new System.Drawing.Point(12, 12);
            this.gcDTC.MainView = this.gvDTC;
            this.gcDTC.Name = "gcDTC";
            this.gcDTC.Size = new System.Drawing.Size(825, 371);
            this.gcDTC.TabIndex = 4;
            this.gcDTC.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvDTC});
            this.gcDTC.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.gcDTC_MouseDoubleClick);
            // 
            // CMStable
            // 
            this.CMStable.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.CMStable.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAdd,
            this.tsmiModify,
            this.tsmiDel,
            this.tsmiFault,
            this.tsmiMsg});
            this.CMStable.Name = "CMStable";
            this.CMStable.Size = new System.Drawing.Size(173, 136);
            // 
            // tsmiAdd
            // 
            this.tsmiAdd.Name = "tsmiAdd";
            this.tsmiAdd.Size = new System.Drawing.Size(172, 22);
            this.tsmiAdd.Text = "增加";
            this.tsmiAdd.Click += new System.EventHandler(this.tsmiAdd_Click);
            // 
            // tsmiModify
            // 
            this.tsmiModify.Name = "tsmiModify";
            this.tsmiModify.Size = new System.Drawing.Size(172, 22);
            this.tsmiModify.Text = "修改";
            this.tsmiModify.Click += new System.EventHandler(this.tsmiModify_Click);
            // 
            // tsmiDel
            // 
            this.tsmiDel.Name = "tsmiDel";
            this.tsmiDel.Size = new System.Drawing.Size(172, 22);
            this.tsmiDel.Text = "删除";
            this.tsmiDel.Click += new System.EventHandler(this.tsmiDel_Click);
            // 
            // tsmiFault
            // 
            this.tsmiFault.Name = "tsmiFault";
            this.tsmiFault.Size = new System.Drawing.Size(172, 22);
            this.tsmiFault.Text = "查看DTC故障信息";
            this.tsmiFault.Click += new System.EventHandler(this.tsmiFault_Click);
            // 
            // tsmiMsg
            // 
            this.tsmiMsg.Name = "tsmiMsg";
            this.tsmiMsg.Size = new System.Drawing.Size(172, 22);
            this.tsmiMsg.Text = "查看诊断报文信息";
            this.tsmiMsg.Click += new System.EventHandler(this.tsmiMsg_Click);
            // 
            // gvDTC
            // 
            this.gvDTC.Appearance.FocusedRow.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.gvDTC.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gvDTC.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.DUTname,
            this.Cddname,
            this.RequestID,
            this.RespondID,
            this.InitTimeofDiag,
            this.FaultInfo,
            this.MessageInfo,
            this.fault,
            this.message});
            this.gvDTC.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFullFocus;
            this.gvDTC.GridControl = this.gcDTC;
            this.gvDTC.Name = "gvDTC";
            this.gvDTC.OptionsBehavior.Editable = false;
            this.gvDTC.OptionsBehavior.ReadOnly = true;
            this.gvDTC.OptionsCustomization.AllowColumnMoving = false;
            this.gvDTC.OptionsFind.AlwaysVisible = true;
            this.gvDTC.OptionsView.ShowGroupPanel = false;
            this.gvDTC.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gvItem_MouseDown);
            // 
            // DUTname
            // 
            this.DUTname.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DUTname.AppearanceCell.Options.UseFont = true;
            this.DUTname.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DUTname.AppearanceHeader.Options.UseFont = true;
            this.DUTname.Caption = "节点名称";
            this.DUTname.FieldName = "DUTname";
            this.DUTname.Name = "DUTname";
            this.DUTname.Visible = true;
            this.DUTname.VisibleIndex = 0;
            // 
            // Cddname
            // 
            this.Cddname.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Cddname.AppearanceCell.Options.UseFont = true;
            this.Cddname.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Cddname.AppearanceHeader.Options.UseFont = true;
            this.Cddname.Caption = "Cdd名称";
            this.Cddname.FieldName = "Cddname";
            this.Cddname.Name = "Cddname";
            this.Cddname.Visible = true;
            this.Cddname.VisibleIndex = 1;
            // 
            // RequestID
            // 
            this.RequestID.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RequestID.AppearanceCell.Options.UseFont = true;
            this.RequestID.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RequestID.AppearanceHeader.Options.UseFont = true;
            this.RequestID.Caption = "请求ID";
            this.RequestID.FieldName = "RequestID";
            this.RequestID.Name = "RequestID";
            this.RequestID.Visible = true;
            this.RequestID.VisibleIndex = 2;
            // 
            // RespondID
            // 
            this.RespondID.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RespondID.AppearanceCell.Options.UseFont = true;
            this.RespondID.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RespondID.AppearanceHeader.Options.UseFont = true;
            this.RespondID.Caption = "响应ID";
            this.RespondID.FieldName = "RespondID";
            this.RespondID.Name = "RespondID";
            this.RespondID.Visible = true;
            this.RespondID.VisibleIndex = 3;
            // 
            // InitTimeofDiag
            // 
            this.InitTimeofDiag.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InitTimeofDiag.AppearanceCell.Options.UseFont = true;
            this.InitTimeofDiag.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InitTimeofDiag.AppearanceHeader.Options.UseFont = true;
            this.InitTimeofDiag.Caption = "初始化诊断时间";
            this.InitTimeofDiag.FieldName = "InitTimeofDiag";
            this.InitTimeofDiag.Name = "InitTimeofDiag";
            this.InitTimeofDiag.Visible = true;
            this.InitTimeofDiag.VisibleIndex = 4;
            // 
            // FaultInfo
            // 
            this.FaultInfo.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FaultInfo.AppearanceCell.Options.UseFont = true;
            this.FaultInfo.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FaultInfo.AppearanceHeader.Options.UseFont = true;
            this.FaultInfo.Caption = "故障相关信息";
            this.FaultInfo.FieldName = "FaultInfo";
            this.FaultInfo.Name = "FaultInfo";
            // 
            // MessageInfo
            // 
            this.MessageInfo.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MessageInfo.AppearanceCell.Options.UseFont = true;
            this.MessageInfo.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MessageInfo.AppearanceHeader.Options.UseFont = true;
            this.MessageInfo.Caption = "报文相关信息";
            this.MessageInfo.FieldName = "MessageInfo";
            this.MessageInfo.Name = "MessageInfo";
            // 
            // fault
            // 
            this.fault.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fault.AppearanceCell.Options.UseFont = true;
            this.fault.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fault.AppearanceHeader.Options.UseFont = true;
            this.fault.Caption = "故障信息相关";
            this.fault.FieldName = "fault";
            this.fault.Name = "fault";
            this.fault.Visible = true;
            this.fault.VisibleIndex = 5;
            // 
            // message
            // 
            this.message.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.message.AppearanceCell.Options.UseFont = true;
            this.message.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.message.AppearanceHeader.Options.UseFont = true;
            this.message.Caption = "报文信息相关";
            this.message.FieldName = "message";
            this.message.Name = "message";
            this.message.Visible = true;
            this.message.VisibleIndex = 6;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "layoutControlGroup1";
            this.layoutControlGroup1.Size = new System.Drawing.Size(849, 395);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.gcDTC;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(829, 375);
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
            this.hideContainerRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(236)))), ((int)(((byte)(239)))));
            this.hideContainerRight.Controls.Add(this.dpAssess);
            this.hideContainerRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.hideContainerRight.Location = new System.Drawing.Point(849, 50);
            this.hideContainerRight.Name = "hideContainerRight";
            this.hideContainerRight.Size = new System.Drawing.Size(20, 395);
            // 
            // dpAssess
            // 
            this.dpAssess.Controls.Add(this.dockPanel1_Container);
            this.dpAssess.Dock = DevExpress.XtraBars.Docking.DockingStyle.Right;
            this.dpAssess.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dpAssess.ID = new System.Guid("2c25fbec-7106-46bb-8cfa-6a5eee536fc0");
            this.dpAssess.Location = new System.Drawing.Point(695, 53);
            this.dpAssess.Name = "dpAssess";
            this.dpAssess.OriginalSize = new System.Drawing.Size(213, 200);
            this.dpAssess.SavedDock = DevExpress.XtraBars.Docking.DockingStyle.Right;
            this.dpAssess.SavedIndex = 0;
            this.dpAssess.Size = new System.Drawing.Size(149, 392);
            this.dpAssess.Text = "总线DTC相关";
            this.dpAssess.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            // 
            // dockPanel1_Container
            // 
            this.dockPanel1_Container.Controls.Add(this.layoutControl2);
            this.dockPanel1_Container.Location = new System.Drawing.Point(6, 62);
            this.dockPanel1_Container.Name = "dockPanel1_Container";
            this.dockPanel1_Container.Size = new System.Drawing.Size(201, 548);
            this.dockPanel1_Container.TabIndex = 0;
            // 
            // layoutControl2
            // 
            this.layoutControl2.AllowCustomization = false;
            this.layoutControl2.Controls.Add(this.txtDiagTime);
            this.layoutControl2.Controls.Add(this.btnOk);
            this.layoutControl2.Controls.Add(this.txtRespondID);
            this.layoutControl2.Controls.Add(this.txtRequestID);
            this.layoutControl2.Controls.Add(this.txtCddName);
            this.layoutControl2.Controls.Add(this.txtDUTname);
            this.layoutControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl2.Location = new System.Drawing.Point(0, 0);
            this.layoutControl2.Name = "layoutControl2";
            this.layoutControl2.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(667, 98, 250, 350);
            this.layoutControl2.Root = this.layoutControlGroup2;
            this.layoutControl2.Size = new System.Drawing.Size(201, 548);
            this.layoutControl2.TabIndex = 0;
            this.layoutControl2.Text = "layoutControl2";
            // 
            // txtDiagTime
            // 
            this.txtDiagTime.Location = new System.Drawing.Point(111, 151);
            this.txtDiagTime.MenuManager = this.ribbonControl1;
            this.txtDiagTime.Name = "txtDiagTime";
            this.txtDiagTime.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDiagTime.Properties.Appearance.Options.UseFont = true;
            this.txtDiagTime.Size = new System.Drawing.Size(78, 24);
            this.txtDiagTime.StyleController = this.layoutControl2;
            this.txtDiagTime.TabIndex = 9;
            // 
            // btnOk
            // 
            this.btnOk.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOk.Appearance.Options.UseFont = true;
            this.btnOk.Location = new System.Drawing.Point(12, 185);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(177, 24);
            this.btnOk.StyleController = this.layoutControl2;
            this.btnOk.TabIndex = 8;
            this.btnOk.Text = "确定";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // txtRespondID
            // 
            this.txtRespondID.Location = new System.Drawing.Point(111, 117);
            this.txtRespondID.MenuManager = this.ribbonControl1;
            this.txtRespondID.Name = "txtRespondID";
            this.txtRespondID.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRespondID.Properties.Appearance.Options.UseFont = true;
            this.txtRespondID.Size = new System.Drawing.Size(78, 24);
            this.txtRespondID.StyleController = this.layoutControl2;
            this.txtRespondID.TabIndex = 7;
            // 
            // txtRequestID
            // 
            this.txtRequestID.Location = new System.Drawing.Point(111, 83);
            this.txtRequestID.MenuManager = this.ribbonControl1;
            this.txtRequestID.Name = "txtRequestID";
            this.txtRequestID.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRequestID.Properties.Appearance.Options.UseFont = true;
            this.txtRequestID.Size = new System.Drawing.Size(78, 24);
            this.txtRequestID.StyleController = this.layoutControl2;
            this.txtRequestID.TabIndex = 6;
            // 
            // txtCddName
            // 
            this.txtCddName.Location = new System.Drawing.Point(111, 49);
            this.txtCddName.MenuManager = this.ribbonControl1;
            this.txtCddName.Name = "txtCddName";
            this.txtCddName.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCddName.Properties.Appearance.Options.UseFont = true;
            this.txtCddName.Size = new System.Drawing.Size(78, 24);
            this.txtCddName.StyleController = this.layoutControl2;
            this.txtCddName.TabIndex = 5;
            // 
            // txtDUTname
            // 
            this.txtDUTname.Location = new System.Drawing.Point(111, 15);
            this.txtDUTname.MenuManager = this.ribbonControl1;
            this.txtDUTname.Name = "txtDUTname";
            this.txtDUTname.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDUTname.Properties.Appearance.Options.UseFont = true;
            this.txtDUTname.Size = new System.Drawing.Size(78, 24);
            this.txtDUTname.StyleController = this.layoutControl2;
            this.txtDUTname.TabIndex = 4;
            // 
            // layoutControlGroup2
            // 
            this.layoutControlGroup2.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup2.GroupBordersVisible = false;
            this.layoutControlGroup2.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem2,
            this.layoutControlItem3,
            this.layoutControlItem4,
            this.layoutControlItem5,
            this.layoutControlItem6,
            this.layoutControlItem7});
            this.layoutControlGroup2.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup2.Name = "Root";
            this.layoutControlGroup2.Size = new System.Drawing.Size(201, 548);
            this.layoutControlGroup2.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem2.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem2.Control = this.txtDUTname;
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Padding = new DevExpress.XtraLayout.Utils.Padding(2, 2, 5, 5);
            this.layoutControlItem2.Size = new System.Drawing.Size(181, 34);
            this.layoutControlItem2.Text = "节点名称：";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(96, 17);
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem3.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem3.Control = this.txtCddName;
            this.layoutControlItem3.Location = new System.Drawing.Point(0, 34);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Padding = new DevExpress.XtraLayout.Utils.Padding(2, 2, 5, 5);
            this.layoutControlItem3.Size = new System.Drawing.Size(181, 34);
            this.layoutControlItem3.Text = "Cdd名称：";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(96, 17);
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem4.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem4.Control = this.txtRequestID;
            this.layoutControlItem4.Location = new System.Drawing.Point(0, 68);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Padding = new DevExpress.XtraLayout.Utils.Padding(2, 2, 5, 5);
            this.layoutControlItem4.Size = new System.Drawing.Size(181, 34);
            this.layoutControlItem4.Text = "请求ID：";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(96, 17);
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem5.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem5.Control = this.txtRespondID;
            this.layoutControlItem5.Location = new System.Drawing.Point(0, 102);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Padding = new DevExpress.XtraLayout.Utils.Padding(2, 2, 5, 5);
            this.layoutControlItem5.Size = new System.Drawing.Size(181, 34);
            this.layoutControlItem5.Text = "响应ID：";
            this.layoutControlItem5.TextSize = new System.Drawing.Size(96, 17);
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.Control = this.btnOk;
            this.layoutControlItem6.Location = new System.Drawing.Point(0, 170);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Padding = new DevExpress.XtraLayout.Utils.Padding(2, 2, 5, 10);
            this.layoutControlItem6.Size = new System.Drawing.Size(181, 358);
            this.layoutControlItem6.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem6.TextVisible = false;
            // 
            // layoutControlItem7
            // 
            this.layoutControlItem7.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem7.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem7.Control = this.txtDiagTime;
            this.layoutControlItem7.Location = new System.Drawing.Point(0, 136);
            this.layoutControlItem7.Name = "layoutControlItem7";
            this.layoutControlItem7.Padding = new DevExpress.XtraLayout.Utils.Padding(2, 2, 5, 5);
            this.layoutControlItem7.Size = new System.Drawing.Size(181, 34);
            this.layoutControlItem7.Text = "初始化诊断时间：";
            this.layoutControlItem7.TextSize = new System.Drawing.Size(96, 17);
            // 
            // BusDTCRelevant
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(869, 445);
            this.Controls.Add(this.layoutControl1);
            this.Controls.Add(this.hideContainerRight);
            this.Controls.Add(this.ribbonControl1);
            this.Name = "BusDTCRelevant";
            this.Ribbon = this.ribbonControl1;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "总线DTC相关";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BusDTCRelevant_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcDTC)).EndInit();
            this.CMStable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gvDTC)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).EndInit();
            this.hideContainerRight.ResumeLayout(false);
            this.dpAssess.ResumeLayout(false);
            this.dockPanel1_Container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl2)).EndInit();
            this.layoutControl2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtDiagTime.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRespondID.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRequestID.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCddName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDUTname.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.LookAndFeel.DefaultLookAndFeel defaultLookAndFeel1;
        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1;
        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraGrid.GridControl gcDTC;
        private DevExpress.XtraGrid.Views.Grid.GridView gvDTC;
        private DevExpress.XtraGrid.Columns.GridColumn DUTname;
        private DevExpress.XtraGrid.Columns.GridColumn Cddname;
        private DevExpress.XtraGrid.Columns.GridColumn RequestID;
        private DevExpress.XtraGrid.Columns.GridColumn RespondID;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraBars.Docking.DockManager dockManager1;
        private DevExpress.XtraBars.Docking.DockPanel dpAssess;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel1_Container;
        private DevExpress.XtraLayout.LayoutControl layoutControl2;
        private DevExpress.XtraEditors.SimpleButton btnOk;
        private DevExpress.XtraEditors.TextEdit txtRespondID;
        private DevExpress.XtraEditors.TextEdit txtRequestID;
        private DevExpress.XtraEditors.TextEdit txtCddName;
        private DevExpress.XtraEditors.TextEdit txtDUTname;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private System.Windows.Forms.ContextMenuStrip CMStable;
        private System.Windows.Forms.ToolStripMenuItem tsmiAdd;
        private System.Windows.Forms.ToolStripMenuItem tsmiModify;
        private System.Windows.Forms.ToolStripMenuItem tsmiDel;
        private DevExpress.LookAndFeel.DefaultLookAndFeel DLAF;
        private DevExpress.XtraGrid.Columns.GridColumn FaultInfo;
        private DevExpress.XtraGrid.Columns.GridColumn MessageInfo;
        private DevExpress.XtraGrid.Columns.GridColumn fault;
        private DevExpress.XtraGrid.Columns.GridColumn message;
        private DevExpress.XtraBars.Docking.AutoHideContainer hideContainerRight;
        private System.Windows.Forms.ToolStripMenuItem tsmiFault;
        private System.Windows.Forms.ToolStripMenuItem tsmiMsg;
        private DevExpress.XtraEditors.TextEdit txtDiagTime;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem7;
        private DevExpress.XtraGrid.Columns.GridColumn InitTimeofDiag;
    }
}