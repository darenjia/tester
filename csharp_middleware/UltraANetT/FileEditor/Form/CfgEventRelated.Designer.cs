namespace FileEditor.Form
{
    partial class CfgEventRelated
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
            this.ribbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.gcEvent = new DevExpress.XtraGrid.GridControl();
            this.cmsOrder = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDel = new System.Windows.Forms.ToolStripMenuItem();
            this.gvEvent = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.AwakeType = new DevExpress.XtraGrid.Columns.GridColumn();
            this.LocalEventIO = new DevExpress.XtraGrid.Columns.GridColumn();
            this.EnableLevel = new DevExpress.XtraGrid.Columns.GridColumn();
            this.LocalEventName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.dockManager1 = new DevExpress.XtraBars.Docking.DockManager(this.components);
            this.hideContainerRight = new DevExpress.XtraBars.Docking.AutoHideContainer();
            this.dpEvent = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel1_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.layoutControl2 = new DevExpress.XtraLayout.LayoutControl();
            this.sbtnSubmit = new DevExpress.XtraEditors.SimpleButton();
            this.cmbEnableLevel = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cmbLocalEventIO = new DevExpress.XtraEditors.ComboBoxEdit();
            this.txtLocalEventName = new DevExpress.XtraEditors.TextEdit();
            this.cmbAwakeType = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lcgEvent = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.defaultLookAndFeel1 = new DevExpress.LookAndFeel.DefaultLookAndFeel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcEvent)).BeginInit();
            this.cmsOrder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvEvent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).BeginInit();
            this.hideContainerRight.SuspendLayout();
            this.dpEvent.SuspendLayout();
            this.dockPanel1_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl2)).BeginInit();
            this.layoutControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbEnableLevel.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbLocalEventIO.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLocalEventName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbAwakeType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lcgEvent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbon
            // 
            this.ribbon.ExpandCollapseItem.Id = 0;
            this.ribbon.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbon.ExpandCollapseItem});
            this.ribbon.Location = new System.Drawing.Point(0, 0);
            this.ribbon.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ribbon.MaxItemId = 1;
            this.ribbon.Name = "ribbon";
            this.ribbon.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonControlStyle.MacOffice;
            this.ribbon.ShowApplicationButton = DevExpress.Utils.DefaultBoolean.False;
            this.ribbon.ShowExpandCollapseButton = DevExpress.Utils.DefaultBoolean.False;
            this.ribbon.ShowFullScreenButton = DevExpress.Utils.DefaultBoolean.False;
            this.ribbon.Size = new System.Drawing.Size(833, 55);
            this.ribbon.ToolbarLocation = DevExpress.XtraBars.Ribbon.RibbonQuickAccessToolbarLocation.Hidden;
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomization = false;
            this.layoutControl1.Controls.Add(this.gcEvent);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 55);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(809, 454);
            this.layoutControl1.TabIndex = 2;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // gcEvent
            // 
            this.gcEvent.ContextMenuStrip = this.cmsOrder;
            this.gcEvent.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gcEvent.Location = new System.Drawing.Point(12, 12);
            this.gcEvent.MainView = this.gvEvent;
            this.gcEvent.Name = "gcEvent";
            this.gcEvent.Size = new System.Drawing.Size(785, 430);
            this.gcEvent.TabIndex = 4;
            this.gcEvent.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvEvent});
            this.gcEvent.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.gcEvent_MouseDoubleClick);
            // 
            // cmsOrder
            // 
            this.cmsOrder.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.cmsOrder.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAdd,
            this.tsmiUpdate,
            this.tsmiDel});
            this.cmsOrder.Name = "cmsOrder";
            this.cmsOrder.Size = new System.Drawing.Size(101, 70);
            // 
            // tsmiAdd
            // 
            this.tsmiAdd.Name = "tsmiAdd";
            this.tsmiAdd.Size = new System.Drawing.Size(100, 22);
            this.tsmiAdd.Text = "添加";
            this.tsmiAdd.Click += new System.EventHandler(this.tsmiAdd_Click);
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
            // gvEvent
            // 
            this.gvEvent.Appearance.FocusedRow.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.gvEvent.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gvEvent.Appearance.Row.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvEvent.Appearance.Row.Options.UseFont = true;
            this.gvEvent.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.AwakeType,
            this.LocalEventIO,
            this.EnableLevel,
            this.LocalEventName});
            this.gvEvent.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFullFocus;
            this.gvEvent.GridControl = this.gcEvent;
            this.gvEvent.Name = "gvEvent";
            this.gvEvent.OptionsBehavior.Editable = false;
            this.gvEvent.OptionsBehavior.ReadOnly = true;
            this.gvEvent.OptionsCustomization.AllowColumnMoving = false;
            this.gvEvent.OptionsFind.AlwaysVisible = true;
            this.gvEvent.OptionsView.ShowGroupPanel = false;
            this.gvEvent.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gvEvent_MouseDown);
            // 
            // AwakeType
            // 
            this.AwakeType.Caption = "唤醒类型";
            this.AwakeType.FieldName = "AwakeType";
            this.AwakeType.Name = "AwakeType";
            this.AwakeType.Visible = true;
            this.AwakeType.VisibleIndex = 0;
            // 
            // LocalEventIO
            // 
            this.LocalEventIO.Caption = "对应配置盒";
            this.LocalEventIO.FieldName = "LocalEventIO";
            this.LocalEventIO.Name = "LocalEventIO";
            this.LocalEventIO.Visible = true;
            this.LocalEventIO.VisibleIndex = 1;
            // 
            // EnableLevel
            // 
            this.EnableLevel.Caption = "本地事件有效值";
            this.EnableLevel.FieldName = "EnableLevel";
            this.EnableLevel.Name = "EnableLevel";
            this.EnableLevel.Visible = true;
            this.EnableLevel.VisibleIndex = 2;
            // 
            // LocalEventName
            // 
            this.LocalEventName.Caption = "本地事件名称";
            this.LocalEventName.FieldName = "LocalEventName";
            this.LocalEventName.Name = "LocalEventName";
            this.LocalEventName.Visible = true;
            this.LocalEventName.VisibleIndex = 3;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "layoutControlGroup1";
            this.layoutControlGroup1.Size = new System.Drawing.Size(809, 454);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.gcEvent;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(789, 434);
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
            this.hideContainerRight.Controls.Add(this.dpEvent);
            this.hideContainerRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.hideContainerRight.Location = new System.Drawing.Point(809, 55);
            this.hideContainerRight.Name = "hideContainerRight";
            this.hideContainerRight.Size = new System.Drawing.Size(24, 454);
            // 
            // dpEvent
            // 
            this.dpEvent.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dpEvent.Appearance.Options.UseFont = true;
            this.dpEvent.Controls.Add(this.dockPanel1_Container);
            this.dpEvent.Dock = DevExpress.XtraBars.Docking.DockingStyle.Right;
            this.dpEvent.FloatSize = new System.Drawing.Size(200, 407);
            this.dpEvent.ID = new System.Guid("b7f5401d-d4cc-44aa-b758-b9b3b48dda3b");
            this.dpEvent.Location = new System.Drawing.Point(563, 55);
            this.dpEvent.Name = "dpEvent";
            this.dpEvent.OriginalSize = new System.Drawing.Size(246, 200);
            this.dpEvent.SavedDock = DevExpress.XtraBars.Docking.DockingStyle.Right;
            this.dpEvent.SavedIndex = 0;
            this.dpEvent.Size = new System.Drawing.Size(246, 454);
            this.dpEvent.Text = "本地事件";
            this.dpEvent.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            // 
            // dockPanel1_Container
            // 
            this.dockPanel1_Container.Controls.Add(this.layoutControl2);
            this.dockPanel1_Container.Location = new System.Drawing.Point(4, 39);
            this.dockPanel1_Container.Name = "dockPanel1_Container";
            this.dockPanel1_Container.Size = new System.Drawing.Size(238, 411);
            this.dockPanel1_Container.TabIndex = 0;
            // 
            // layoutControl2
            // 
            this.layoutControl2.AllowCustomization = false;
            this.layoutControl2.Controls.Add(this.sbtnSubmit);
            this.layoutControl2.Controls.Add(this.cmbEnableLevel);
            this.layoutControl2.Controls.Add(this.cmbLocalEventIO);
            this.layoutControl2.Controls.Add(this.txtLocalEventName);
            this.layoutControl2.Controls.Add(this.cmbAwakeType);
            this.layoutControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl2.Location = new System.Drawing.Point(0, 0);
            this.layoutControl2.Name = "layoutControl2";
            this.layoutControl2.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(276, 541, 842, 521);
            this.layoutControl2.Root = this.lcgEvent;
            this.layoutControl2.Size = new System.Drawing.Size(238, 411);
            this.layoutControl2.TabIndex = 0;
            this.layoutControl2.Text = "layoutControl2";
            // 
            // sbtnSubmit
            // 
            this.sbtnSubmit.Appearance.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.sbtnSubmit.Appearance.Options.UseFont = true;
            this.sbtnSubmit.Location = new System.Drawing.Point(20, 174);
            this.sbtnSubmit.Name = "sbtnSubmit";
            this.sbtnSubmit.Size = new System.Drawing.Size(198, 24);
            this.sbtnSubmit.StyleController = this.layoutControl2;
            this.sbtnSubmit.TabIndex = 8;
            this.sbtnSubmit.Text = "确定";
            this.sbtnSubmit.Click += new System.EventHandler(this.sbtnSubmit_Click);
            // 
            // cmbEnableLevel
            // 
            this.cmbEnableLevel.Location = new System.Drawing.Point(120, 51);
            this.cmbEnableLevel.Name = "cmbEnableLevel";
            this.cmbEnableLevel.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.cmbEnableLevel.Properties.Appearance.Options.UseFont = true;
            this.cmbEnableLevel.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbEnableLevel.Properties.Items.AddRange(new object[] {
            "低有效",
            "高有效"});
            this.cmbEnableLevel.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbEnableLevel.Size = new System.Drawing.Size(105, 26);
            this.cmbEnableLevel.StyleController = this.layoutControl2;
            this.cmbEnableLevel.TabIndex = 6;
            // 
            // cmbLocalEventIO
            // 
            this.cmbLocalEventIO.Location = new System.Drawing.Point(120, 87);
            this.cmbLocalEventIO.Name = "cmbLocalEventIO";
            this.cmbLocalEventIO.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.cmbLocalEventIO.Properties.Appearance.Options.UseFont = true;
            this.cmbLocalEventIO.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbLocalEventIO.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbLocalEventIO.Size = new System.Drawing.Size(105, 26);
            this.cmbLocalEventIO.StyleController = this.layoutControl2;
            this.cmbLocalEventIO.TabIndex = 5;
            // 
            // txtLocalEventName
            // 
            this.txtLocalEventName.Location = new System.Drawing.Point(120, 123);
            this.txtLocalEventName.Name = "txtLocalEventName";
            this.txtLocalEventName.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.txtLocalEventName.Properties.Appearance.Options.UseFont = true;
            this.txtLocalEventName.Size = new System.Drawing.Size(105, 26);
            this.txtLocalEventName.StyleController = this.layoutControl2;
            this.txtLocalEventName.TabIndex = 7;
            // 
            // cmbAwakeType
            // 
            this.cmbAwakeType.Location = new System.Drawing.Point(120, 15);
            this.cmbAwakeType.Name = "cmbAwakeType";
            this.cmbAwakeType.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.cmbAwakeType.Properties.Appearance.Options.UseFont = true;
            this.cmbAwakeType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbAwakeType.Properties.Items.AddRange(new object[] {
            "本地事件"});
            this.cmbAwakeType.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbAwakeType.Size = new System.Drawing.Size(105, 26);
            this.cmbAwakeType.StyleController = this.layoutControl2;
            this.cmbAwakeType.TabIndex = 4;
            // 
            // lcgEvent
            // 
            this.lcgEvent.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.lcgEvent.GroupBordersVisible = false;
            this.lcgEvent.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem4,
            this.layoutControlItem5,
            this.layoutControlItem3,
            this.layoutControlItem6,
            this.layoutControlItem2});
            this.lcgEvent.Location = new System.Drawing.Point(0, 0);
            this.lcgEvent.Name = "Root";
            this.lcgEvent.Size = new System.Drawing.Size(238, 411);
            this.lcgEvent.TextVisible = false;
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.layoutControlItem4.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem4.Control = this.cmbEnableLevel;
            this.layoutControlItem4.Location = new System.Drawing.Point(0, 36);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 5, 5);
            this.layoutControlItem4.Size = new System.Drawing.Size(218, 36);
            this.layoutControlItem4.Text = "本地事件有效值：";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(104, 19);
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.layoutControlItem5.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem5.Control = this.txtLocalEventName;
            this.layoutControlItem5.Location = new System.Drawing.Point(0, 108);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 5, 5);
            this.layoutControlItem5.Size = new System.Drawing.Size(218, 36);
            this.layoutControlItem5.Text = "本地事件名称：";
            this.layoutControlItem5.TextSize = new System.Drawing.Size(104, 19);
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.layoutControlItem3.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem3.Control = this.cmbLocalEventIO;
            this.layoutControlItem3.Location = new System.Drawing.Point(0, 72);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 5, 5);
            this.layoutControlItem3.Size = new System.Drawing.Size(218, 36);
            this.layoutControlItem3.Text = "对应配置盒：";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(104, 19);
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.Control = this.sbtnSubmit;
            this.layoutControlItem6.Location = new System.Drawing.Point(0, 144);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 20, 10);
            this.layoutControlItem6.Size = new System.Drawing.Size(218, 247);
            this.layoutControlItem6.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem6.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.layoutControlItem2.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem2.Control = this.cmbAwakeType;
            this.layoutControlItem2.CustomizationFormText = "唤醒类型：";
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 5, 5);
            this.layoutControlItem2.Size = new System.Drawing.Size(218, 36);
            this.layoutControlItem2.Text = "唤醒类型：";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(104, 19);
            // 
            // defaultLookAndFeel1
            // 
            this.defaultLookAndFeel1.LookAndFeel.SkinName = "Office 2016 Colorful";
            // 
            // CfgEventRelated
            // 
            this.Appearance.Options.UseFont = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(833, 509);
            this.Controls.Add(this.layoutControl1);
            this.Controls.Add(this.hideContainerRight);
            this.Controls.Add(this.ribbon);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "CfgEventRelated";
            this.Ribbon = this.ribbon;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "本地事件";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CfgEventRelated_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcEvent)).EndInit();
            this.cmsOrder.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gvEvent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).EndInit();
            this.hideContainerRight.ResumeLayout(false);
            this.dpEvent.ResumeLayout(false);
            this.dockPanel1_Container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl2)).EndInit();
            this.layoutControl2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cmbEnableLevel.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbLocalEventIO.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLocalEventName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbAwakeType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lcgEvent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonControl ribbon;
        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraGrid.GridControl gcEvent;
        private DevExpress.XtraGrid.Views.Grid.GridView gvEvent;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraBars.Docking.DockManager dockManager1;
        private DevExpress.XtraBars.Docking.DockPanel dpEvent;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel1_Container;
        private DevExpress.XtraLayout.LayoutControl layoutControl2;
        private DevExpress.XtraLayout.LayoutControlGroup lcgEvent;
        private DevExpress.XtraEditors.ComboBoxEdit cmbEnableLevel;
        private DevExpress.XtraEditors.ComboBoxEdit cmbLocalEventIO;
        private DevExpress.XtraEditors.SimpleButton sbtnSubmit;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private DevExpress.XtraEditors.TextEdit txtLocalEventName;
        private DevExpress.XtraGrid.Columns.GridColumn AwakeType;
        private DevExpress.XtraGrid.Columns.GridColumn LocalEventIO;
        private DevExpress.XtraGrid.Columns.GridColumn EnableLevel;
        private DevExpress.XtraGrid.Columns.GridColumn LocalEventName;
        private System.Windows.Forms.ContextMenuStrip cmsOrder;
        private System.Windows.Forms.ToolStripMenuItem tsmiAdd;
        private System.Windows.Forms.ToolStripMenuItem tsmiUpdate;
        private System.Windows.Forms.ToolStripMenuItem tsmiDel;
        private DevExpress.XtraBars.Docking.AutoHideContainer hideContainerRight;
        private DevExpress.LookAndFeel.DefaultLookAndFeel defaultLookAndFeel1;
        private DevExpress.XtraEditors.ComboBoxEdit cmbAwakeType;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
    }
}