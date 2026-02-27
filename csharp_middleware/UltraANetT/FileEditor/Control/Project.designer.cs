namespace FileEditor.Control
{
    partial class Project
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
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject1 = new DevExpress.Utils.SerializableAppearanceObject();
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.label1 = new System.Windows.Forms.Label();
            this.btnDownload = new DevExpress.XtraEditors.SimpleButton();
            this.btnUpLoad = new DevExpress.XtraEditors.SimpleButton();
            this.btnFind = new DevExpress.XtraEditors.ButtonEdit();
            this.gcProject = new DevExpress.XtraGrid.GridControl();
            this.cmsBtn = new System.Windows.Forms.ContextMenuStrip();
            this.tsmiDel = new System.Windows.Forms.ToolStripMenuItem();
            this.gvProject = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.ProName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Content = new DevExpress.XtraGrid.Columns.GridColumn();
            this.MatchSort = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Update = new DevExpress.XtraGrid.Columns.GridColumn();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.DLAF = new DevExpress.LookAndFeel.DefaultLookAndFeel();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnFind.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcProject)).BeginInit();
            this.cmsBtn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvProject)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl1
            // 
            this.layoutControl1.Controls.Add(this.label1);
            this.layoutControl1.Controls.Add(this.btnDownload);
            this.layoutControl1.Controls.Add(this.btnUpLoad);
            this.layoutControl1.Controls.Add(this.btnFind);
            this.layoutControl1.Controls.Add(this.gcProject);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(645, 401, 375, 525);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(1274, 739);
            this.layoutControl1.TabIndex = 1;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(1024, 636);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(245, 38);
            this.label1.TabIndex = 10;
            this.label1.Text = "（请上传小于400M的文件）";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnDownload
            // 
            this.btnDownload.Appearance.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDownload.Appearance.Options.UseFont = true;
            this.btnDownload.Location = new System.Drawing.Point(659, 687);
            this.btnDownload.MinimumSize = new System.Drawing.Size(0, 40);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(593, 40);
            this.btnDownload.StyleController = this.layoutControl1;
            this.btnDownload.TabIndex = 9;
            this.btnDownload.Text = "下载工程文件";
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // btnUpLoad
            // 
            this.btnUpLoad.Appearance.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnUpLoad.Appearance.Options.UseFont = true;
            this.btnUpLoad.Enabled = false;
            this.btnUpLoad.Location = new System.Drawing.Point(22, 687);
            this.btnUpLoad.MinimumSize = new System.Drawing.Size(0, 40);
            this.btnUpLoad.Name = "btnUpLoad";
            this.btnUpLoad.Size = new System.Drawing.Size(597, 40);
            this.btnUpLoad.StyleController = this.layoutControl1;
            this.btnUpLoad.TabIndex = 6;
            this.btnUpLoad.Text = "上传工程文件";
            this.btnUpLoad.Click += new System.EventHandler(this.btnUpLoad_Click);
            // 
            // btnFind
            // 
            this.btnFind.Location = new System.Drawing.Point(117, 638);
            this.btnFind.Name = "btnFind";
            this.btnFind.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnFind.Properties.Appearance.Options.UseFont = true;
            this.btnFind.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Search, "", 60, true, true, false, DevExpress.XtraEditors.ImageLocation.MiddleCenter, null, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), serializableAppearanceObject1, "", null, null, true)});
            this.btnFind.Properties.ReadOnly = true;
            this.btnFind.Size = new System.Drawing.Size(884, 34);
            this.btnFind.StyleController = this.layoutControl1;
            this.btnFind.TabIndex = 5;
            this.btnFind.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.btnFind_ButtonClick);
            this.btnFind.EditValueChanged += new System.EventHandler(this.btnFind_EditValueChanged);
            // 
            // gcProject
            // 
            this.gcProject.ContextMenuStrip = this.cmsBtn;
            this.gcProject.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(1, 3, 1, 3);
            this.gcProject.Location = new System.Drawing.Point(5, 5);
            this.gcProject.MainView = this.gvProject;
            this.gcProject.Name = "gcProject";
            this.gcProject.Size = new System.Drawing.Size(1264, 625);
            this.gcProject.TabIndex = 4;
            this.gcProject.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvProject});
            // 
            // cmsBtn
            // 
            this.cmsBtn.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.cmsBtn.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiDel});
            this.cmsBtn.Name = "contextMenuStrip1";
            this.cmsBtn.Size = new System.Drawing.Size(189, 34);
            // 
            // tsmiDel
            // 
            this.tsmiDel.AutoSize = false;
            this.tsmiDel.Enabled = false;
            this.tsmiDel.Name = "tsmiDel";
            this.tsmiDel.Size = new System.Drawing.Size(160, 30);
            this.tsmiDel.Text = "删除工程文件";
            this.tsmiDel.Click += new System.EventHandler(this.tsmiDel_Click);
            // 
            // gvProject
            // 
            this.gvProject.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.ProName,
            this.Content,
            this.MatchSort,
            this.Update});
            this.gvProject.GridControl = this.gcProject;
            this.gvProject.Name = "gvProject";
            this.gvProject.OptionsView.ShowGroupPanel = false;
            this.gvProject.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gvProject_RowClick);
            this.gvProject.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gvProject_MouseDown);
            // 
            // ProName
            // 
            this.ProName.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ProName.AppearanceCell.Options.UseFont = true;
            this.ProName.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ProName.AppearanceHeader.Options.UseFont = true;
            this.ProName.Caption = "工程文件名称";
            this.ProName.FieldName = "ProName";
            this.ProName.Name = "ProName";
            this.ProName.OptionsColumn.AllowEdit = false;
            this.ProName.Visible = true;
            this.ProName.VisibleIndex = 0;
            // 
            // Content
            // 
            this.Content.Caption = "文件内容";
            this.Content.FieldName = "Content";
            this.Content.Name = "Content";
            this.Content.OptionsColumn.AllowEdit = false;
            // 
            // MatchSort
            // 
            this.MatchSort.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MatchSort.AppearanceCell.Options.UseFont = true;
            this.MatchSort.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MatchSort.AppearanceHeader.Options.UseFont = true;
            this.MatchSort.Caption = "上传人";
            this.MatchSort.FieldName = "MatchSort";
            this.MatchSort.Name = "MatchSort";
            this.MatchSort.OptionsColumn.AllowEdit = false;
            this.MatchSort.Visible = true;
            this.MatchSort.VisibleIndex = 1;
            // 
            // Update
            // 
            this.Update.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Update.AppearanceCell.Options.UseFont = true;
            this.Update.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Update.AppearanceHeader.Options.UseFont = true;
            this.Update.Caption = "更新日期";
            this.Update.DisplayFormat.FormatString = "\"yyyy-MM-dd\"";
            this.Update.FieldName = "Update";
            this.Update.Name = "Update";
            this.Update.OptionsColumn.AllowEdit = false;
            this.Update.Visible = true;
            this.Update.VisibleIndex = 2;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem4,
            this.layoutControlItem5,
            this.layoutControlItem3,
            this.layoutControlItem2});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.OptionsItemText.TextToControlDistance = 5;
            this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(2, 2, 2, 2);
            this.layoutControlGroup1.Size = new System.Drawing.Size(1274, 739);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.gcProject;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(1270, 631);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.btnDownload;
            this.layoutControlItem4.Location = new System.Drawing.Point(637, 675);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Padding = new DevExpress.XtraLayout.Utils.Padding(20, 20, 10, 10);
            this.layoutControlItem4.Size = new System.Drawing.Size(633, 60);
            this.layoutControlItem4.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem4.TextVisible = false;
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.Control = this.label1;
            this.layoutControlItem5.Location = new System.Drawing.Point(1019, 631);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Size = new System.Drawing.Size(251, 44);
            this.layoutControlItem5.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem5.TextVisible = false;
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.btnUpLoad;
            this.layoutControlItem3.Location = new System.Drawing.Point(0, 675);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Padding = new DevExpress.XtraLayout.Utils.Padding(20, 20, 10, 10);
            this.layoutControlItem3.Size = new System.Drawing.Size(637, 60);
            this.layoutControlItem3.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem3.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem2.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem2.Control = this.btnFind;
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 631);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Padding = new DevExpress.XtraLayout.Utils.Padding(20, 20, 5, 5);
            this.layoutControlItem2.Size = new System.Drawing.Size(1019, 44);
            this.layoutControlItem2.Text = "文件路径：";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(90, 24);
            // 
            // DLAF
            // 
            this.DLAF.LookAndFeel.SkinName = "Office 2016 Colorful";
            // 
            // Project
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.layoutControl1);
            this.Name = "Project";
            this.Size = new System.Drawing.Size(1274, 739);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnFind.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcProject)).EndInit();
            this.cmsBtn.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gvProject)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraGrid.GridControl gcProject;
        private DevExpress.XtraGrid.Views.Grid.GridView gvProject;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraEditors.ButtonEdit btnFind;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraEditors.SimpleButton btnUpLoad;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraGrid.Columns.GridColumn ProName;
        private DevExpress.XtraGrid.Columns.GridColumn Content;
        private DevExpress.XtraGrid.Columns.GridColumn MatchSort;
        private DevExpress.XtraGrid.Columns.GridColumn UpdatePro;
        private System.Windows.Forms.ContextMenuStrip cmsBtn;
        private System.Windows.Forms.ToolStripMenuItem tsmiDel;
        private DevExpress.XtraGrid.Columns.GridColumn UpdateCol;
        private DevExpress.LookAndFeel.DefaultLookAndFeel DLAF;
        private DevExpress.XtraGrid.Columns.GridColumn UpdateCopy;
        private DevExpress.XtraGrid.Columns.GridColumn Update;
        private DevExpress.XtraEditors.SimpleButton btnDownload;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private System.Windows.Forms.Label label1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
    }
}
