namespace UltraANetT.Module
{
    partial class LogInfo
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
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.sbtnSelectTime = new DevExpress.XtraEditors.SimpleButton();
            this.nbcEmpList = new DevExpress.XtraNavBar.NavBarControl();
            this.EmpList = new DevExpress.XtraNavBar.NavBarGroup();
            this.dtdDown = new DevExpress.XtraEditors.DateEdit();
            this.dtdUp = new DevExpress.XtraEditors.DateEdit();
            this.txtLoginCount = new DevExpress.XtraEditors.TextEdit();
            this.gcLogInfo = new DevExpress.XtraGrid.GridControl();
            this.gvLogInfo = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.LoginNo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.EmployeeNo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.EmployeeName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Department = new DevExpress.XtraGrid.Columns.GridColumn();
            this.LoginDate = new DevExpress.XtraGrid.Columns.GridColumn();
            this.LoginOffDate = new DevExpress.XtraGrid.Columns.GridColumn();
            this.sbtnClear = new DevExpress.XtraEditors.SimpleButton();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlGroup4 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.simpleButtonLCI = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlGroup2 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem8 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nbcEmpList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtdDown.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtdDown.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtdUp.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtdUp.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLoginCount.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcLogInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvLogInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.simpleButtonLCI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomization = false;
            this.layoutControl1.Controls.Add(this.sbtnSelectTime);
            this.layoutControl1.Controls.Add(this.nbcEmpList);
            this.layoutControl1.Controls.Add(this.dtdDown);
            this.layoutControl1.Controls.Add(this.dtdUp);
            this.layoutControl1.Controls.Add(this.txtLoginCount);
            this.layoutControl1.Controls.Add(this.gcLogInfo);
            this.layoutControl1.Controls.Add(this.sbtnClear);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(297, 244, 779, 350);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(1022, 552);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // sbtnSelectTime
            // 
            this.sbtnSelectTime.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sbtnSelectTime.Appearance.Options.UseFont = true;
            this.sbtnSelectTime.Location = new System.Drawing.Point(869, 18);
            this.sbtnSelectTime.MaximumSize = new System.Drawing.Size(80, 25);
            this.sbtnSelectTime.MinimumSize = new System.Drawing.Size(50, 25);
            this.sbtnSelectTime.Name = "sbtnSelectTime";
            this.sbtnSelectTime.Size = new System.Drawing.Size(50, 25);
            this.sbtnSelectTime.StyleController = this.layoutControl1;
            this.sbtnSelectTime.TabIndex = 27;
            this.sbtnSelectTime.Text = "查询";
            this.sbtnSelectTime.Click += new System.EventHandler(this.sbtnSelectTime_Click);
            // 
            // nbcEmpList
            // 
            this.nbcEmpList.ActiveGroup = this.EmpList;
            this.nbcEmpList.Appearance.Background.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nbcEmpList.Appearance.Background.Options.UseFont = true;
            this.nbcEmpList.Appearance.GroupBackground.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nbcEmpList.Appearance.GroupBackground.Options.UseFont = true;
            this.nbcEmpList.Appearance.GroupHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nbcEmpList.Appearance.GroupHeader.Options.UseFont = true;
            this.nbcEmpList.Appearance.GroupHeaderActive.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nbcEmpList.Appearance.GroupHeaderActive.Options.UseFont = true;
            this.nbcEmpList.Appearance.Item.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nbcEmpList.Appearance.Item.Options.UseFont = true;
            this.nbcEmpList.Appearance.NavigationPaneHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nbcEmpList.Appearance.NavigationPaneHeader.Options.UseFont = true;
            this.nbcEmpList.Appearance.NavPaneContentButton.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nbcEmpList.Appearance.NavPaneContentButton.Options.UseFont = true;
            this.nbcEmpList.Groups.AddRange(new DevExpress.XtraNavBar.NavBarGroup[] {
            this.EmpList});
            this.nbcEmpList.LinkSelectionMode = DevExpress.XtraNavBar.LinkSelectionModeType.OneInControl;
            this.nbcEmpList.Location = new System.Drawing.Point(8, 8);
            this.nbcEmpList.Margin = new System.Windows.Forms.Padding(1);
            this.nbcEmpList.Name = "nbcEmpList";
            this.nbcEmpList.OptionsNavPane.ExpandedWidth = 125;
            this.nbcEmpList.OptionsNavPane.ShowOverflowButton = false;
            this.nbcEmpList.OptionsNavPane.ShowOverflowPanel = false;
            this.nbcEmpList.OptionsNavPane.ShowSplitter = false;
            this.nbcEmpList.Size = new System.Drawing.Size(125, 536);
            this.nbcEmpList.TabIndex = 26;
            this.nbcEmpList.Text = "navBarControl1";
            this.nbcEmpList.View = new DevExpress.XtraNavBar.ViewInfo.SkinNavigationPaneViewInfoRegistrator();
            // 
            // EmpList
            // 
            this.EmpList.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EmpList.Appearance.Options.UseFont = true;
            this.EmpList.Caption = "操作人列表";
            this.EmpList.Expanded = true;
            this.EmpList.GroupStyle = DevExpress.XtraNavBar.NavBarGroupStyle.SmallIconsText;
            this.EmpList.Name = "EmpList";
            this.EmpList.SelectedLinkIndex = 1;
            // 
            // dtdDown
            // 
            this.dtdDown.EditValue = null;
            this.dtdDown.Location = new System.Drawing.Point(688, 18);
            this.dtdDown.Name = "dtdDown";
            this.dtdDown.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtdDown.Properties.Appearance.Options.UseFont = true;
            this.dtdDown.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtdDown.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtdDown.Size = new System.Drawing.Size(161, 26);
            this.dtdDown.StyleController = this.layoutControl1;
            this.dtdDown.TabIndex = 14;
            // 
            // dtdUp
            // 
            this.dtdUp.EditValue = null;
            this.dtdUp.Location = new System.Drawing.Point(411, 18);
            this.dtdUp.Name = "dtdUp";
            this.dtdUp.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtdUp.Properties.Appearance.Options.UseFont = true;
            this.dtdUp.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtdUp.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtdUp.Size = new System.Drawing.Size(163, 26);
            this.dtdUp.StyleController = this.layoutControl1;
            this.dtdUp.TabIndex = 13;
            // 
            // txtLoginCount
            // 
            this.txtLoginCount.Location = new System.Drawing.Point(247, 18);
            this.txtLoginCount.MaximumSize = new System.Drawing.Size(50, 0);
            this.txtLoginCount.Name = "txtLoginCount";
            this.txtLoginCount.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLoginCount.Properties.Appearance.Options.UseFont = true;
            this.txtLoginCount.Properties.ReadOnly = true;
            this.txtLoginCount.Size = new System.Drawing.Size(50, 26);
            this.txtLoginCount.StyleController = this.layoutControl1;
            this.txtLoginCount.TabIndex = 12;
            // 
            // gcLogInfo
            // 
            this.gcLogInfo.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gcLogInfo.Location = new System.Drawing.Point(140, 63);
            this.gcLogInfo.MainView = this.gvLogInfo;
            this.gcLogInfo.Name = "gcLogInfo";
            this.gcLogInfo.Size = new System.Drawing.Size(877, 484);
            this.gcLogInfo.TabIndex = 11;
            this.gcLogInfo.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvLogInfo});
            // 
            // gvLogInfo
            // 
            this.gvLogInfo.Appearance.Empty.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvLogInfo.Appearance.Empty.Options.UseFont = true;
            this.gvLogInfo.Appearance.EvenRow.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvLogInfo.Appearance.EvenRow.Options.UseFont = true;
            this.gvLogInfo.Appearance.FocusedRow.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.gvLogInfo.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gvLogInfo.Appearance.HeaderPanel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvLogInfo.Appearance.HeaderPanel.Options.UseFont = true;
            this.gvLogInfo.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.LoginNo,
            this.EmployeeNo,
            this.EmployeeName,
            this.Department,
            this.LoginDate,
            this.LoginOffDate});
            this.gvLogInfo.GridControl = this.gcLogInfo;
            this.gvLogInfo.Name = "gvLogInfo";
            this.gvLogInfo.OptionsBehavior.Editable = false;
            this.gvLogInfo.OptionsBehavior.ReadOnly = true;
            this.gvLogInfo.OptionsCustomization.AllowColumnMoving = false;
            this.gvLogInfo.OptionsFind.AlwaysVisible = true;
            this.gvLogInfo.OptionsMenu.EnableColumnMenu = false;
            this.gvLogInfo.OptionsMenu.EnableFooterMenu = false;
            this.gvLogInfo.OptionsMenu.EnableGroupPanelMenu = false;
            this.gvLogInfo.OptionsView.ShowGroupPanel = false;
            this.gvLogInfo.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.gvLogInfo_RowCellStyle);
            // 
            // LoginNo
            // 
            this.LoginNo.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoginNo.AppearanceCell.Options.UseFont = true;
            this.LoginNo.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoginNo.AppearanceHeader.Options.UseFont = true;
            this.LoginNo.Caption = "编号";
            this.LoginNo.FieldName = "LoginNo";
            this.LoginNo.Name = "LoginNo";
            this.LoginNo.Visible = true;
            this.LoginNo.VisibleIndex = 0;
            // 
            // EmployeeNo
            // 
            this.EmployeeNo.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EmployeeNo.AppearanceCell.Options.UseFont = true;
            this.EmployeeNo.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EmployeeNo.AppearanceHeader.Options.UseFont = true;
            this.EmployeeNo.Caption = "人员编号";
            this.EmployeeNo.FieldName = "EmployeeNo";
            this.EmployeeNo.Name = "EmployeeNo";
            this.EmployeeNo.Visible = true;
            this.EmployeeNo.VisibleIndex = 1;
            // 
            // EmployeeName
            // 
            this.EmployeeName.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EmployeeName.AppearanceCell.Options.UseFont = true;
            this.EmployeeName.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EmployeeName.AppearanceHeader.Options.UseFont = true;
            this.EmployeeName.Caption = "姓名";
            this.EmployeeName.FieldName = "EmployeeName";
            this.EmployeeName.Name = "EmployeeName";
            this.EmployeeName.Visible = true;
            this.EmployeeName.VisibleIndex = 2;
            // 
            // Department
            // 
            this.Department.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Department.AppearanceCell.Options.UseFont = true;
            this.Department.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Department.AppearanceHeader.Options.UseFont = true;
            this.Department.Caption = "所属部门";
            this.Department.FieldName = "Department";
            this.Department.Name = "Department";
            this.Department.Visible = true;
            this.Department.VisibleIndex = 3;
            // 
            // LoginDate
            // 
            this.LoginDate.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoginDate.AppearanceCell.Options.UseFont = true;
            this.LoginDate.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoginDate.AppearanceHeader.Options.UseFont = true;
            this.LoginDate.Caption = "登陆时间";
            this.LoginDate.FieldName = "LoginDate";
            this.LoginDate.Name = "LoginDate";
            this.LoginDate.Visible = true;
            this.LoginDate.VisibleIndex = 4;
            // 
            // LoginOffDate
            // 
            this.LoginOffDate.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoginOffDate.AppearanceCell.Options.UseFont = true;
            this.LoginOffDate.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoginOffDate.AppearanceHeader.Options.UseFont = true;
            this.LoginOffDate.Caption = "退出时间";
            this.LoginOffDate.FieldName = "LoginOffDate";
            this.LoginOffDate.Name = "LoginOffDate";
            this.LoginOffDate.Visible = true;
            this.LoginOffDate.VisibleIndex = 5;
            // 
            // sbtnClear
            // 
            this.sbtnClear.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sbtnClear.Appearance.Options.UseFont = true;
            this.sbtnClear.AutoWidthInLayoutControl = true;
            this.sbtnClear.Location = new System.Drawing.Point(939, 18);
            this.sbtnClear.MaximumSize = new System.Drawing.Size(80, 25);
            this.sbtnClear.MinimumSize = new System.Drawing.Size(50, 25);
            this.sbtnClear.Name = "sbtnClear";
            this.sbtnClear.Padding = new System.Windows.Forms.Padding(3);
            this.sbtnClear.Size = new System.Drawing.Size(65, 25);
            this.sbtnClear.StyleController = this.layoutControl1;
            this.sbtnClear.TabIndex = 4;
            this.sbtnClear.Text = "查看全部";
            this.sbtnClear.Click += new System.EventHandler(this.sbtnClear_Click);
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem3,
            this.layoutControlGroup4,
            this.layoutControlGroup2});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 3, 3);
            this.layoutControlGroup1.Size = new System.Drawing.Size(1022, 552);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.gcLogInfo;
            this.layoutControlItem3.Location = new System.Drawing.Point(135, 58);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Size = new System.Drawing.Size(881, 488);
            this.layoutControlItem3.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem3.TextVisible = false;
            // 
            // layoutControlGroup4
            // 
            this.layoutControlGroup4.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem4,
            this.layoutControlItem5,
            this.layoutControlItem6,
            this.layoutControlItem2,
            this.simpleButtonLCI});
            this.layoutControlGroup4.Location = new System.Drawing.Point(135, 0);
            this.layoutControlGroup4.Name = "layoutControlGroup4";
            this.layoutControlGroup4.Padding = new DevExpress.XtraLayout.Utils.Padding(2, 2, 2, 2);
            this.layoutControlGroup4.Size = new System.Drawing.Size(881, 58);
            this.layoutControlGroup4.TextVisible = false;
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem4.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem4.Control = this.txtLoginCount;
            this.layoutControlItem4.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.layoutControlItem4.Size = new System.Drawing.Size(164, 48);
            this.layoutControlItem4.Text = "历史登陆次数：";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(91, 19);
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem5.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem5.Control = this.dtdUp;
            this.layoutControlItem5.Location = new System.Drawing.Point(164, 0);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.layoutControlItem5.Size = new System.Drawing.Size(277, 48);
            this.layoutControlItem5.Text = "起始时间：";
            this.layoutControlItem5.TextSize = new System.Drawing.Size(91, 19);
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem6.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem6.Control = this.dtdDown;
            this.layoutControlItem6.Location = new System.Drawing.Point(441, 0);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.layoutControlItem6.Size = new System.Drawing.Size(275, 48);
            this.layoutControlItem6.Text = "截止日期：";
            this.layoutControlItem6.TextSize = new System.Drawing.Size(91, 19);
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.sbtnSelectTime;
            this.layoutControlItem2.Location = new System.Drawing.Point(716, 0);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.layoutControlItem2.Size = new System.Drawing.Size(70, 48);
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextVisible = false;
            // 
            // simpleButtonLCI
            // 
            this.simpleButtonLCI.Control = this.sbtnClear;
            this.simpleButtonLCI.ControlAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.simpleButtonLCI.CustomizationFormText = "simpleButtonLCI";
            this.simpleButtonLCI.Location = new System.Drawing.Point(786, 0);
            this.simpleButtonLCI.Name = "simpleButtonLCI";
            this.simpleButtonLCI.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.simpleButtonLCI.Size = new System.Drawing.Size(85, 48);
            this.simpleButtonLCI.TextLocation = DevExpress.Utils.Locations.Left;
            this.simpleButtonLCI.TextSize = new System.Drawing.Size(0, 0);
            this.simpleButtonLCI.TextVisible = false;
            this.simpleButtonLCI.TrimClientAreaToControl = false;
            // 
            // layoutControlGroup2
            // 
            this.layoutControlGroup2.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem8});
            this.layoutControlGroup2.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup2.Name = "layoutControlGroup2";
            this.layoutControlGroup2.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.layoutControlGroup2.Size = new System.Drawing.Size(135, 546);
            this.layoutControlGroup2.TextVisible = false;
            // 
            // layoutControlItem8
            // 
            this.layoutControlItem8.Control = this.nbcEmpList;
            this.layoutControlItem8.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem8.Name = "layoutControlItem8";
            this.layoutControlItem8.Size = new System.Drawing.Size(129, 540);
            this.layoutControlItem8.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem8.TextVisible = false;
            // 
            // LogInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.layoutControl1);
            this.Name = "LogInfo";
            this.Size = new System.Drawing.Size(1022, 552);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nbcEmpList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtdDown.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtdDown.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtdUp.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtdUp.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLoginCount.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcLogInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvLogInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.simpleButtonLCI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraGrid.GridControl gcLogInfo;
        private DevExpress.XtraGrid.Views.Grid.GridView gvLogInfo;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraEditors.DateEdit dtdDown;
        private DevExpress.XtraEditors.DateEdit dtdUp;
        private DevExpress.XtraEditors.TextEdit txtLoginCount;
        private DevExpress.XtraGrid.Columns.GridColumn LoginNo;
        private DevExpress.XtraGrid.Columns.GridColumn EmployeeName;
        private DevExpress.XtraGrid.Columns.GridColumn Department;
        private DevExpress.XtraGrid.Columns.GridColumn LoginDate;
        private DevExpress.XtraGrid.Columns.GridColumn LoginOffDate;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup4;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup2;
        private DevExpress.XtraNavBar.NavBarControl nbcEmpList;
        private DevExpress.XtraNavBar.NavBarGroup EmpList;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem8;
        private DevExpress.XtraGrid.Columns.GridColumn EmployeeNo;
        private DevExpress.XtraEditors.SimpleButton sbtnSelectTime;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraEditors.SimpleButton sbtnClear;
        private DevExpress.XtraLayout.LayoutControlItem simpleButtonLCI;
    }
}
