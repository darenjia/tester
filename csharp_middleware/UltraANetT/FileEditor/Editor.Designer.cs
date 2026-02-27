using ProcessEngine;

namespace FileEditor
{
    partial class Editor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Editor));
            this.components = new System.ComponentModel.Container();
            this.navEditor = new DevExpress.XtraBars.Navigation.OfficeNavigationBar();
            this.navCfg = new DevExpress.XtraBars.Navigation.NavigationBarItem();
            this.navExmp = new DevExpress.XtraBars.Navigation.NavigationBarItem();
            this.navProject = new DevExpress.XtraBars.Navigation.NavigationBarItem();
            this.pcContainer = new DevExpress.XtraEditors.PanelControl();
            this.cfgTemp = new FileEditor.Control.CfgTemplate();
            this.DLAF = new DevExpress.LookAndFeel.DefaultLookAndFeel(this.components);
            this.ribbonControl1 = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroup1 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            ((System.ComponentModel.ISupportInitialize)(this.navEditor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pcContainer)).BeginInit();
            this.pcContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // navEditor
            // 
            this.navEditor.AppearanceItem.Normal.BackColor = System.Drawing.Color.Transparent;
            this.navEditor.AppearanceItem.Normal.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.navEditor.AppearanceItem.Normal.Options.UseBackColor = true;
            this.navEditor.AppearanceItem.Normal.Options.UseFont = true;
            this.navEditor.AutoSize = false;
            this.navEditor.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.navEditor.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.navEditor.Items.AddRange(new DevExpress.XtraBars.Navigation.NavigationBarItem[] {
            this.navCfg,
            this.navExmp,
            this.navProject});
            this.navEditor.Location = new System.Drawing.Point(0, 740);
            this.navEditor.Name = "navEditor";
            this.navEditor.Size = new System.Drawing.Size(1265, 43);
            this.navEditor.TabIndex = 0;
            this.navEditor.Text = "officeNavigationBar1";
            this.navEditor.Visible = false;
            this.navEditor.ItemClick += new DevExpress.XtraBars.Navigation.NavigationBarItemClickEventHandler(this.navEditor_ItemClick);
            // 
            // navCfg
            // 
            this.navCfg.Name = "navCfg";
            this.navCfg.Text = "配置管理";
            // 
            // navExmp
            // 
            this.navExmp.Name = "navExmp";
            this.navExmp.Text = "用例管理";
            // 
            // navProject
            // 
            this.navProject.Name = "navProject";
            this.navProject.Text = "工程文件管理";
            // 
            // pcContainer
            // 
            this.pcContainer.Controls.Add(this.cfgTemp);
            this.pcContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pcContainer.Location = new System.Drawing.Point(0, 32);
            this.pcContainer.Name = "pcContainer";
            this.pcContainer.Size = new System.Drawing.Size(1265, 708);
            this.pcContainer.TabIndex = 1;
            // 
            // cfgTemp
            // 
            this.cfgTemp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cfgTemp.Location = new System.Drawing.Point(2, 2);
            this.cfgTemp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cfgTemp.Name = "cfgTemp";
            this.cfgTemp.Size = new System.Drawing.Size(1261, 704);
            this.cfgTemp.TabIndex = 0;
            // 
            // DLAF
            // 
            this.DLAF.LookAndFeel.SkinName = "Office 2016 Colorful";
            // 
            // ribbonControl1
            // 
            this.ribbonControl1.ExpandCollapseItem.Id = 0;
            this.ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbonControl1.ExpandCollapseItem});
            this.ribbonControl1.Location = new System.Drawing.Point(0, 0);
            this.ribbonControl1.MaxItemId = 1;
            this.ribbonControl1.Name = "ribbonControl1";
            this.ribbonControl1.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            this.ribbonPage1});
            this.ribbonControl1.Size = new System.Drawing.Size(1265, 32);
            // 
            // ribbonPage1
            // 
            this.ribbonPage1.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.ribbonPageGroup1});
            this.ribbonPage1.Name = "ribbonPage1";
            this.ribbonPage1.Text = "ribbonPage1";
            // 
            // ribbonPageGroup1
            // 
            this.ribbonPageGroup1.Name = "ribbonPageGroup1";
            this.ribbonPageGroup1.Text = "ribbonPageGroup1";
            // 
            // Editor
            // 
            this.Appearance.Options.UseFont = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1265, 783);
            this.Controls.Add(this.pcContainer);
            this.Controls.Add(this.navEditor);
            this.Controls.Add(this.ribbonControl1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderEffect = DevExpress.XtraEditors.FormBorderEffect.Shadow;
            this.Name = "Editor";
            this.Text = "配置表";
            this.Ribbon = this.ribbonControl1;
            this.RibbonVisibility = DevExpress.XtraBars.Ribbon.RibbonVisibility.Hidden;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Editor_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Editor_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.navEditor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pcContainer)).EndInit();
            this.pcContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.Navigation.OfficeNavigationBar navEditor;
        private DevExpress.XtraBars.Navigation.NavigationBarItem navProject;
        private DevExpress.XtraEditors.PanelControl pcContainer;
        private DevExpress.LookAndFeel.DefaultLookAndFeel DLAF;
        private DevExpress.XtraBars.Navigation.NavigationBarItem navCfg;
        private DevExpress.XtraBars.Navigation.NavigationBarItem navExmp;
        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage1;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup1;
        private Control.CfgTemplate cfgTemp;
    }
}