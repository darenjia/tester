namespace UltraANetT.Module
{
    partial class OperationLog
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.sbtnSelectTime = new DevExpress.XtraEditors.SimpleButton();
            this.dtdDown = new DevExpress.XtraEditors.DateEdit();
            this.dtdUp = new DevExpress.XtraEditors.DateEdit();
            this.nbcEmpList = new DevExpress.XtraNavBar.NavBarControl();
            this.EmpList = new DevExpress.XtraNavBar.NavBarGroup();
            this.gcOperationLog = new DevExpress.XtraGrid.GridControl();
            this.gvOperationLog = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.OperNo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.EmployeeNo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.EmployeeName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.OperRecord = new DevExpress.XtraGrid.Columns.GridColumn();
            this.OperTable = new DevExpress.XtraGrid.Columns.GridColumn();
            this.OperDate = new DevExpress.XtraGrid.Columns.GridColumn();
            this.sbtnClear = new DevExpress.XtraEditors.SimpleButton();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlGroup2 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlGroup3 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.simpleButtonLCI = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtdDown.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtdDown.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtdUp.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtdUp.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nbcEmpList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcOperationLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvOperationLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.simpleButtonLCI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomization = false;
            this.layoutControl1.Controls.Add(this.sbtnSelectTime);
            this.layoutControl1.Controls.Add(this.dtdDown);
            this.layoutControl1.Controls.Add(this.dtdUp);
            this.layoutControl1.Controls.Add(this.nbcEmpList);
            this.layoutControl1.Controls.Add(this.gcOperationLog);
            this.layoutControl1.Controls.Add(this.sbtnClear);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(418, 275, 698, 602);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(1460, 1040);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // sbtnSelectTime
            // 
            this.sbtnSelectTime.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sbtnSelectTime.Appearance.Options.UseFont = true;
            this.sbtnSelectTime.Location = new System.Drawing.Point(1244, 21);
            this.sbtnSelectTime.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.sbtnSelectTime.MaximumSize = new System.Drawing.Size(114, 39);
            this.sbtnSelectTime.MinimumSize = new System.Drawing.Size(71, 39);
            this.sbtnSelectTime.Name = "sbtnSelectTime";
            this.sbtnSelectTime.Size = new System.Drawing.Size(90, 39);
            this.sbtnSelectTime.StyleController = this.layoutControl1;
            this.sbtnSelectTime.TabIndex = 9;
            this.sbtnSelectTime.Text = "查询";
            this.sbtnSelectTime.Click += new System.EventHandler(this.sbtnSelectTime_Click);
            // 
            // dtdDown
            // 
            this.dtdDown.EditValue = null;
            this.dtdDown.Location = new System.Drawing.Point(834, 21);
            this.dtdDown.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dtdDown.Name = "dtdDown";
            this.dtdDown.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtdDown.Properties.Appearance.Options.UseFont = true;
            this.dtdDown.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtdDown.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtdDown.Properties.MinValue = new System.DateTime(1753, 1, 1, 0, 0, 0, 0);
            this.dtdDown.Size = new System.Drawing.Size(390, 30);
            this.dtdDown.StyleController = this.layoutControl1;
            this.dtdDown.TabIndex = 8;
            // 
            // dtdUp
            // 
            this.dtdUp.EditValue = null;
            this.dtdUp.Location = new System.Drawing.Point(314, 21);
            this.dtdUp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dtdUp.Name = "dtdUp";
            this.dtdUp.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtdUp.Properties.Appearance.Options.UseFont = true;
            this.dtdUp.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtdUp.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtdUp.Properties.MinValue = new System.DateTime(1753, 1, 1, 0, 0, 0, 0);
            this.dtdUp.Size = new System.Drawing.Size(396, 30);
            this.dtdUp.StyleController = this.layoutControl1;
            this.dtdUp.TabIndex = 7;
            // 
            // nbcEmpList
            // 
            this.nbcEmpList.ActiveGroup = this.EmpList;
            this.nbcEmpList.Appearance.Background.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.nbcEmpList.Appearance.Background.Options.UseFont = true;
            this.nbcEmpList.Appearance.GroupBackground.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nbcEmpList.Appearance.GroupBackground.Options.UseFont = true;
            this.nbcEmpList.Appearance.GroupHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.nbcEmpList.Appearance.GroupHeader.Options.UseFont = true;
            this.nbcEmpList.Appearance.GroupHeaderActive.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nbcEmpList.Appearance.GroupHeaderActive.Options.UseFont = true;
            this.nbcEmpList.Appearance.Item.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nbcEmpList.Appearance.Item.Options.UseFont = true;
            this.nbcEmpList.Appearance.NavigationPaneHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nbcEmpList.Appearance.NavigationPaneHeader.Options.UseFont = true;
            this.nbcEmpList.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nbcEmpList.Groups.AddRange(new DevExpress.XtraNavBar.NavBarGroup[] {
            this.EmpList});
            this.nbcEmpList.LinkSelectionMode = DevExpress.XtraNavBar.LinkSelectionModeType.OneInControl;
            this.nbcEmpList.Location = new System.Drawing.Point(13, 13);
            this.nbcEmpList.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nbcEmpList.Name = "nbcEmpList";
            this.nbcEmpList.OptionsNavPane.ExpandedWidth = 169;
            this.nbcEmpList.OptionsNavPane.ShowOverflowButton = false;
            this.nbcEmpList.OptionsNavPane.ShowOverflowPanel = false;
            this.nbcEmpList.OptionsNavPane.ShowSplitter = false;
            this.nbcEmpList.Size = new System.Drawing.Size(169, 1014);
            this.nbcEmpList.TabIndex = 5;
            this.nbcEmpList.Text = "navBarControl1";
            this.nbcEmpList.View = new DevExpress.XtraNavBar.ViewInfo.SkinNavigationPaneViewInfoRegistrator();
            // 
            // EmpList
            // 
            this.EmpList.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EmpList.Appearance.Options.UseFont = true;
            this.EmpList.Caption = "操作人列表";
            this.EmpList.Expanded = true;
            this.EmpList.Name = "EmpList";
            this.EmpList.SelectedLinkIndex = 0;
            // 
            // gcOperationLog
            // 
            this.gcOperationLog.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(1, 9, 1, 9);
            this.gcOperationLog.Location = new System.Drawing.Point(195, 81);
            this.gcOperationLog.MainView = this.gvOperationLog;
            this.gcOperationLog.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gcOperationLog.Name = "gcOperationLog";
            this.gcOperationLog.Size = new System.Drawing.Size(1259, 953);
            this.gcOperationLog.TabIndex = 4;
            this.gcOperationLog.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvOperationLog});
            // 
            // gvOperationLog
            // 
            this.gvOperationLog.Appearance.Empty.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvOperationLog.Appearance.Empty.Options.UseFont = true;
            this.gvOperationLog.Appearance.EvenRow.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvOperationLog.Appearance.EvenRow.Options.UseFont = true;
            this.gvOperationLog.Appearance.FocusedRow.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.gvOperationLog.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gvOperationLog.Appearance.HeaderPanel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvOperationLog.Appearance.HeaderPanel.Options.UseFont = true;
            this.gvOperationLog.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.OperNo,
            this.EmployeeNo,
            this.EmployeeName,
            this.OperRecord,
            this.OperTable,
            this.OperDate});
            this.gvOperationLog.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFullFocus;
            this.gvOperationLog.GridControl = this.gcOperationLog;
            this.gvOperationLog.Name = "gvOperationLog";
            this.gvOperationLog.OptionsBehavior.Editable = false;
            this.gvOperationLog.OptionsBehavior.ReadOnly = true;
            this.gvOperationLog.OptionsCustomization.AllowColumnMoving = false;
            this.gvOperationLog.OptionsFind.AlwaysVisible = true;
            this.gvOperationLog.OptionsMenu.EnableColumnMenu = false;
            this.gvOperationLog.OptionsMenu.EnableFooterMenu = false;
            this.gvOperationLog.OptionsMenu.EnableGroupPanelMenu = false;
            this.gvOperationLog.OptionsView.ShowGroupPanel = false;
            // 
            // OperNo
            // 
            this.OperNo.Caption = "记录编号";
            this.OperNo.FieldName = "OperNo";
            this.OperNo.Name = "OperNo";
            this.OperNo.Visible = true;
            this.OperNo.VisibleIndex = 0;
            // 
            // EmployeeNo
            // 
            this.EmployeeNo.Caption = "员工编号";
            this.EmployeeNo.FieldName = "EmployeeNo";
            this.EmployeeNo.Name = "EmployeeNo";
            this.EmployeeNo.Visible = true;
            this.EmployeeNo.VisibleIndex = 1;
            // 
            // EmployeeName
            // 
            this.EmployeeName.Caption = "员工姓名";
            this.EmployeeName.FieldName = "EmployeeName";
            this.EmployeeName.Name = "EmployeeName";
            this.EmployeeName.Visible = true;
            this.EmployeeName.VisibleIndex = 2;
            // 
            // OperRecord
            // 
            this.OperRecord.Caption = "操作记录";
            this.OperRecord.FieldName = "OperRecord";
            this.OperRecord.Name = "OperRecord";
            this.OperRecord.Visible = true;
            this.OperRecord.VisibleIndex = 4;
            // 
            // OperTable
            // 
            this.OperTable.Caption = "操作类型";
            this.OperTable.FieldName = "OperTable";
            this.OperTable.Name = "OperTable";
            this.OperTable.Visible = true;
            this.OperTable.VisibleIndex = 3;
            // 
            // OperDate
            // 
            this.OperDate.Caption = "操作日期";
            this.OperDate.FieldName = "OperDate";
            this.OperDate.Name = "OperDate";
            this.OperDate.Visible = true;
            this.OperDate.VisibleIndex = 5;
            // 
            // sbtnClear
            // 
            this.sbtnClear.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sbtnClear.Appearance.Options.UseFont = true;
            this.sbtnClear.AutoWidthInLayoutControl = true;
            this.sbtnClear.Location = new System.Drawing.Point(1354, 21);
            this.sbtnClear.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.sbtnClear.MaximumSize = new System.Drawing.Size(114, 39);
            this.sbtnClear.MinimumSize = new System.Drawing.Size(71, 39);
            this.sbtnClear.Name = "sbtnClear";
            this.sbtnClear.Size = new System.Drawing.Size(85, 39);
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
            this.layoutControlItem1,
            this.layoutControlGroup2,
            this.layoutControlGroup3});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.OptionsItemText.TextToControlDistance = 4;
            this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 3, 3);
            this.layoutControlGroup1.Size = new System.Drawing.Size(1460, 1040);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.gcOperationLog;
            this.layoutControlItem1.Location = new System.Drawing.Point(189, 75);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(1265, 959);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // layoutControlGroup2
            // 
            this.layoutControlGroup2.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem2});
            this.layoutControlGroup2.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup2.Name = "layoutControlGroup2";
            this.layoutControlGroup2.OptionsItemText.TextToControlDistance = 4;
            this.layoutControlGroup2.Padding = new DevExpress.XtraLayout.Utils.Padding(2, 2, 2, 2);
            this.layoutControlGroup2.Size = new System.Drawing.Size(189, 1034);
            this.layoutControlGroup2.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.nbcEmpList;
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(175, 1020);
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextVisible = false;
            // 
            // layoutControlGroup3
            // 
            this.layoutControlGroup3.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem4,
            this.layoutControlItem5,
            this.simpleButtonLCI,
            this.layoutControlItem6});
            this.layoutControlGroup3.Location = new System.Drawing.Point(189, 0);
            this.layoutControlGroup3.Name = "layoutControlGroup3";
            this.layoutControlGroup3.OptionsItemText.TextToControlDistance = 4;
            this.layoutControlGroup3.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 3, 3);
            this.layoutControlGroup3.Size = new System.Drawing.Size(1265, 75);
            this.layoutControlGroup3.TextVisible = false;
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.layoutControlItem4.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem4.Control = this.dtdUp;
            this.layoutControlItem4.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.layoutControlItem4.Size = new System.Drawing.Size(520, 59);
            this.layoutControlItem4.Text = "起始时间：";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(100, 27);
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.layoutControlItem5.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem5.Control = this.dtdDown;
            this.layoutControlItem5.Location = new System.Drawing.Point(520, 0);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.layoutControlItem5.Size = new System.Drawing.Size(514, 59);
            this.layoutControlItem5.Text = "截止时间：";
            this.layoutControlItem5.TextSize = new System.Drawing.Size(100, 27);
            // 
            // simpleButtonLCI
            // 
            this.simpleButtonLCI.Control = this.sbtnClear;
            this.simpleButtonLCI.CustomizationFormText = "simpleButtonLCI";
            this.simpleButtonLCI.Location = new System.Drawing.Point(1144, 0);
            this.simpleButtonLCI.Name = "simpleButtonLCI";
            this.simpleButtonLCI.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.simpleButtonLCI.Size = new System.Drawing.Size(105, 59);
            this.simpleButtonLCI.TextSize = new System.Drawing.Size(0, 0);
            this.simpleButtonLCI.TextVisible = false;
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.Control = this.sbtnSelectTime;
            this.layoutControlItem6.Location = new System.Drawing.Point(1034, 0);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.layoutControlItem6.Size = new System.Drawing.Size(110, 59);
            this.layoutControlItem6.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem6.TextVisible = false;
            // 
            // OperationLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.layoutControl1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "OperationLog";
            this.Size = new System.Drawing.Size(1460, 1040);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtdDown.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtdDown.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtdUp.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtdUp.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nbcEmpList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcOperationLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvOperationLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.simpleButtonLCI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraGrid.GridControl gcOperationLog;
        private DevExpress.XtraGrid.Views.Grid.GridView gvOperationLog;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraNavBar.NavBarControl nbcEmpList;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraGrid.Columns.GridColumn OperNo;
        private DevExpress.XtraGrid.Columns.GridColumn EmployeeNo;
        private DevExpress.XtraGrid.Columns.GridColumn EmployeeName;
        private DevExpress.XtraGrid.Columns.GridColumn OperTable;
        private DevExpress.XtraGrid.Columns.GridColumn OperRecord;
        private DevExpress.XtraNavBar.NavBarGroup EmpList;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup2;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup3;
        private DevExpress.XtraEditors.DateEdit dtdUp;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraEditors.DateEdit dtdDown;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraEditors.SimpleButton sbtnSelectTime;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private DevExpress.XtraEditors.SimpleButton sbtnClear;
        private DevExpress.XtraLayout.LayoutControlItem simpleButtonLCI;
        private DevExpress.XtraGrid.Columns.GridColumn OperDate;
    }
}
