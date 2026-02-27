using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraLayout;
using DevExpress.XtraNavBar;
using DevExpress.XtraNavBar.ViewInfo;
using DevExpress.XtraTab;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using UltraANetT.Properties;

namespace UltraANetT.Module
{
    partial class Vehicel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Vehicel));
            this.imageCollection = new DevExpress.Utils.ImageCollection(this.components);
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.label1 = new System.Windows.Forms.Label();
            this.vehicelTree = new DevExpress.XtraTreeList.TreeList();
            this.colName = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.imageTree = new DevExpress.Utils.ImageCollection(this.components);
            this.comboBoxEdit1 = new DevExpress.XtraEditors.ComboBoxEdit();
            this.tabAuthorize = new DevExpress.XtraTab.XtraTabControl();
            this.pageAuthorize = new DevExpress.XtraTab.XtraTabPage();
            this.gcVechiel = new DevExpress.XtraGrid.GridControl();
            this.gvVechiel = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.VehicelType = new DevExpress.XtraGrid.Columns.GridColumn();
            this.VehicelConfig = new DevExpress.XtraGrid.Columns.GridColumn();
            this.VehicelStage = new DevExpress.XtraGrid.Columns.GridColumn();
            this.CreateTime = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Creater = new DevExpress.XtraGrid.Columns.GridColumn();
            this.AuthorizeTo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.FromDepartment = new DevExpress.XtraGrid.Columns.GridColumn();
            this.AuthorizationTime = new DevExpress.XtraGrid.Columns.GridColumn();
            this.InvalidTime = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Remark = new DevExpress.XtraGrid.Columns.GridColumn();
            this.pageSet = new DevExpress.XtraTab.XtraTabPage();
            this.layoutControl2 = new DevExpress.XtraLayout.LayoutControl();
            this.pictSecond = new DevExpress.XtraEditors.PictureEdit();
            this.pictFirst = new DevExpress.XtraEditors.PictureEdit();
            this.pictureEdit3 = new DevExpress.XtraEditors.PictureEdit();
            this.pictureEdit2 = new DevExpress.XtraEditors.PictureEdit();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.pictureEdit1 = new DevExpress.XtraEditors.PictureEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.layoutControlItem15 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlGroup3 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem5 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem7 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem8 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem10 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem9 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem4 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem29 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem12 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem2 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem24 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlGroup2 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.CMSTable = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDel = new System.Windows.Forms.ToolStripMenuItem();
            this.CMSVehicel = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiCreateConfig = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDelVehicel = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiReNameVehicel = new System.Windows.Forms.ToolStripMenuItem();
            this.CMSCreate = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiCreateVehicel = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiView = new System.Windows.Forms.ToolStripMenuItem();
            this.CMSConfig = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiCreateStage = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDelConfig = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiReNameConfig = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiPasteStage = new System.Windows.Forms.ToolStripMenuItem();
            this.CMSStage = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAuthorize = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDelStage = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCopyStage = new System.Windows.Forms.ToolStripMenuItem();
            this.emptySpaceItem3 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.dmVehicel = new DevExpress.XtraBars.Docking.DockManager(this.components);
            this.hideContainerRight = new DevExpress.XtraBars.Docking.AutoHideContainer();
            this.dpVehicel = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel1_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.layoutControl3 = new DevExpress.XtraLayout.LayoutControl();
            this.txtCreater = new DevExpress.XtraEditors.ComboBoxEdit();
            this.dateInvalid = new DevExpress.XtraEditors.DateEdit();
            this.dateAuth = new DevExpress.XtraEditors.DateEdit();
            this.dateCreate = new DevExpress.XtraEditors.DateEdit();
            this.btnSubmit = new DevExpress.XtraEditors.SimpleButton();
            this.txtRemark = new DevExpress.XtraEditors.MemoEdit();
            this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
            this.cbVehicelConfig = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cbvehicelStage = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cbvehicelType = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cbAuthTo = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.cbDepartment = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.layoutControlGroup4 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem20 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem21 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem22 = new DevExpress.XtraLayout.LayoutControlItem();
            this.configer = new DevExpress.XtraLayout.LayoutControlItem();
            this.configeDep = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem30 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem26 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem31 = new DevExpress.XtraLayout.LayoutControlItem();
            this.AuthorizingTime = new DevExpress.XtraLayout.LayoutControlItem();
            this.startTime = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem28 = new DevExpress.XtraLayout.LayoutControlItem();
            this.Author = new DevExpress.XtraLayout.LayoutControlItem();
            this.DLAF = new DevExpress.LookAndFeel.DefaultLookAndFeel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.imageCollection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vehicelTree)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageTree)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabAuthorize)).BeginInit();
            this.tabAuthorize.SuspendLayout();
            this.pageAuthorize.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcVechiel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvVechiel)).BeginInit();
            this.pageSet.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl2)).BeginInit();
            this.layoutControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictSecond.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictFirst.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureEdit3.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureEdit2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem15)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem10)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem29)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem12)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem24)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            this.CMSTable.SuspendLayout();
            this.CMSVehicel.SuspendLayout();
            this.CMSCreate.SuspendLayout();
            this.CMSConfig.SuspendLayout();
            this.CMSStage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dmVehicel)).BeginInit();
            this.hideContainerRight.SuspendLayout();
            this.dpVehicel.SuspendLayout();
            this.dockPanel1_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl3)).BeginInit();
            this.layoutControl3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtCreater.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateInvalid.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateInvalid.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateAuth.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateAuth.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateCreate.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateCreate.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbVehicelConfig.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbvehicelStage.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbvehicelType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbAuthTo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbDepartment.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem20)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem21)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem22)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.configer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.configeDep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem30)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem26)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem31)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AuthorizingTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.startTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem28)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Author)).BeginInit();
            this.SuspendLayout();
            // 
            // imageCollection
            // 
            this.imageCollection.ImageStream = ((DevExpress.Utils.ImageCollectionStreamer)(resources.GetObject("imageCollection.ImageStream")));
            this.imageCollection.Images.SetKeyName(0, "carList.png");
            this.imageCollection.Images.SetKeyName(1, "carList1.png");
            this.imageCollection.Images.SetKeyName(2, "carList2.png");
            this.imageCollection.Images.SetKeyName(3, "Calendar_16x16.png");
            this.imageCollection.Images.SetKeyName(4, "Drafts_16x16.png");
            this.imageCollection.Images.SetKeyName(5, "Organizer_16x16.png");
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomization = false;
            this.layoutControl1.Controls.Add(this.label1);
            this.layoutControl1.Controls.Add(this.vehicelTree);
            this.layoutControl1.Controls.Add(this.comboBoxEdit1);
            this.layoutControl1.Controls.Add(this.tabAuthorize);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.HiddenItems.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1});
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(523, 101, 914, 617);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(998, 550);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // label1
            // 
            this.label1.AutoEllipsis = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(192, 20);
            this.label1.TabIndex = 10;
            this.label1.Text = "车型列表";
            // 
            // vehicelTree
            // 
            this.vehicelTree.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.vehicelTree.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.colName});
            this.vehicelTree.Font = new System.Drawing.Font("微软雅黑", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.vehicelTree.Location = new System.Drawing.Point(9, 41);
            this.vehicelTree.LookAndFeel.SkinName = "Office 2016 Dark";
            this.vehicelTree.MaximumSize = new System.Drawing.Size(200, 0);
            this.vehicelTree.Name = "vehicelTree";
            this.vehicelTree.OptionsBehavior.Editable = false;
            this.vehicelTree.OptionsBehavior.PopulateServiceColumns = true;
            this.vehicelTree.OptionsView.AutoWidth = false;
            this.vehicelTree.OptionsView.ShowColumns = false;
            this.vehicelTree.OptionsView.ShowHorzLines = false;
            this.vehicelTree.OptionsView.ShowIndentAsRowStyle = true;
            this.vehicelTree.OptionsView.ShowIndicator = false;
            this.vehicelTree.OptionsView.ShowVertLines = false;
            this.vehicelTree.RowHeight = 25;
            this.vehicelTree.Size = new System.Drawing.Size(200, 500);
            this.vehicelTree.StateImageList = this.imageTree;
            this.vehicelTree.TabIndex = 9;
            this.vehicelTree.FocusedNodeChanged += new DevExpress.XtraTreeList.FocusedNodeChangedEventHandler(this.vehicelTree_FocusedNodeChanged);
            this.vehicelTree.HiddenEditor += new System.EventHandler(this.vehicelTree_HiddenEditor);
            this.vehicelTree.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.vehicelTree_MouseDoubleClick);
            this.vehicelTree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.vehicelTree_MouseDown);
            // 
            // colName
            // 
            this.colName.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.colName.AppearanceCell.Options.UseFont = true;
            this.colName.Caption = "colName";
            this.colName.FieldName = "colName";
            this.colName.MinWidth = 88;
            this.colName.Name = "colName";
            this.colName.Visible = true;
            this.colName.VisibleIndex = 0;
            this.colName.Width = 500;
            // 
            // imageTree
            // 
            this.imageTree.ImageStream = ((DevExpress.Utils.ImageCollectionStreamer)(resources.GetObject("imageTree.ImageStream")));
            this.imageTree.Images.SetKeyName(0, "carList.png");
            this.imageTree.Images.SetKeyName(1, "carList1.png");
            this.imageTree.Images.SetKeyName(2, "carList2.png");
            this.imageTree.Images.SetKeyName(3, "List-node.png");
            // 
            // comboBoxEdit1
            // 
            this.comboBoxEdit1.Location = new System.Drawing.Point(120, 11);
            this.comboBoxEdit1.Name = "comboBoxEdit1";
            this.comboBoxEdit1.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxEdit1.Properties.Appearance.Options.UseFont = true;
            this.comboBoxEdit1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.comboBoxEdit1.Size = new System.Drawing.Size(104, 26);
            this.comboBoxEdit1.StyleController = this.layoutControl1;
            this.comboBoxEdit1.TabIndex = 8;
            // 
            // tabAuthorize
            // 
            this.tabAuthorize.Location = new System.Drawing.Point(218, 4);
            this.tabAuthorize.Name = "tabAuthorize";
            this.tabAuthorize.SelectedTabPage = this.pageAuthorize;
            this.tabAuthorize.Size = new System.Drawing.Size(776, 542);
            this.tabAuthorize.TabIndex = 6;
            this.tabAuthorize.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.pageAuthorize,
            this.pageSet});
            this.tabAuthorize.Click += new System.EventHandler(this.tabAuthorize_Click);
            // 
            // pageAuthorize
            // 
            this.pageAuthorize.Appearance.Header.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pageAuthorize.Appearance.Header.Options.UseFont = true;
            this.pageAuthorize.Controls.Add(this.gcVechiel);
            this.pageAuthorize.Name = "pageAuthorize";
            this.pageAuthorize.Size = new System.Drawing.Size(774, 513);
            this.pageAuthorize.Text = "车型授权";
            // 
            // gcVechiel
            // 
            this.gcVechiel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcVechiel.EmbeddedNavigator.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gcVechiel.EmbeddedNavigator.Appearance.Options.UseFont = true;
            this.gcVechiel.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gcVechiel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gcVechiel.Location = new System.Drawing.Point(0, 0);
            this.gcVechiel.MainView = this.gvVechiel;
            this.gcVechiel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gcVechiel.Name = "gcVechiel";
            this.gcVechiel.Size = new System.Drawing.Size(774, 513);
            this.gcVechiel.TabIndex = 4;
            this.gcVechiel.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvVechiel});
            this.gcVechiel.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.gcVechiel_MouseDoubleClick);
            // 
            // gvVechiel
            // 
            this.gvVechiel.Appearance.Empty.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvVechiel.Appearance.Empty.Options.UseFont = true;
            this.gvVechiel.Appearance.EvenRow.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvVechiel.Appearance.EvenRow.Options.UseFont = true;
            this.gvVechiel.Appearance.FocusedRow.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.gvVechiel.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gvVechiel.Appearance.HeaderPanel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvVechiel.Appearance.HeaderPanel.Options.UseFont = true;
            this.gvVechiel.Appearance.Row.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvVechiel.Appearance.Row.Options.UseFont = true;
            this.gvVechiel.AppearancePrint.Row.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvVechiel.AppearancePrint.Row.Options.UseFont = true;
            this.gvVechiel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.gvVechiel.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.VehicelType,
            this.VehicelConfig,
            this.VehicelStage,
            this.CreateTime,
            this.Creater,
            this.AuthorizeTo,
            this.FromDepartment,
            this.AuthorizationTime,
            this.InvalidTime,
            this.Remark});
            this.gvVechiel.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFullFocus;
            this.gvVechiel.GridControl = this.gcVechiel;
            this.gvVechiel.Name = "gvVechiel";
            this.gvVechiel.OptionsBehavior.Editable = false;
            this.gvVechiel.OptionsBehavior.ReadOnly = true;
            this.gvVechiel.OptionsCustomization.AllowColumnMoving = false;
            this.gvVechiel.OptionsFind.AlwaysVisible = true;
            this.gvVechiel.OptionsView.ShowGroupPanel = false;
            this.gvVechiel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gvVechiel_MouseDown);
            // 
            // VehicelType
            // 
            this.VehicelType.Caption = "车型";
            this.VehicelType.FieldName = "VehicelType";
            this.VehicelType.Name = "VehicelType";
            this.VehicelType.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            this.VehicelType.Visible = true;
            this.VehicelType.VisibleIndex = 0;
            // 
            // VehicelConfig
            // 
            this.VehicelConfig.Caption = "配置";
            this.VehicelConfig.FieldName = "VehicelConfig";
            this.VehicelConfig.Name = "VehicelConfig";
            this.VehicelConfig.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            this.VehicelConfig.Visible = true;
            this.VehicelConfig.VisibleIndex = 1;
            // 
            // VehicelStage
            // 
            this.VehicelStage.Caption = "阶段";
            this.VehicelStage.FieldName = "VehicelStage";
            this.VehicelStage.Name = "VehicelStage";
            this.VehicelStage.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            this.VehicelStage.Visible = true;
            this.VehicelStage.VisibleIndex = 2;
            // 
            // CreateTime
            // 
            this.CreateTime.Caption = "授权时间";
            this.CreateTime.DisplayFormat.FormatString = "yyyy-MM-dd";
            this.CreateTime.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.CreateTime.FieldName = "CreateTime";
            this.CreateTime.Name = "CreateTime";
            this.CreateTime.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            this.CreateTime.Visible = true;
            this.CreateTime.VisibleIndex = 3;
            // 
            // Creater
            // 
            this.Creater.Caption = "创建人";
            this.Creater.FieldName = "Creater";
            this.Creater.Name = "Creater";
            this.Creater.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            this.Creater.Visible = true;
            this.Creater.VisibleIndex = 4;
            // 
            // AuthorizeTo
            // 
            this.AuthorizeTo.Caption = "授权配置员";
            this.AuthorizeTo.FieldName = "AuthorizeTo";
            this.AuthorizeTo.Name = "AuthorizeTo";
            this.AuthorizeTo.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            this.AuthorizeTo.Visible = true;
            this.AuthorizeTo.VisibleIndex = 5;
            // 
            // FromDepartment
            // 
            this.FromDepartment.Caption = "部门";
            this.FromDepartment.FieldName = "FromDepartment";
            this.FromDepartment.Name = "FromDepartment";
            this.FromDepartment.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            // 
            // AuthorizationTime
            // 
            this.AuthorizationTime.Caption = "配置开始时间";
            this.AuthorizationTime.DisplayFormat.FormatString = "yyyy-MM-dd";
            this.AuthorizationTime.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.AuthorizationTime.FieldName = "AuthorizationTime";
            this.AuthorizationTime.Name = "AuthorizationTime";
            this.AuthorizationTime.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            // 
            // InvalidTime
            // 
            this.InvalidTime.Caption = "配置失效时间";
            this.InvalidTime.DisplayFormat.FormatString = "yyyy-MM-dd";
            this.InvalidTime.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.InvalidTime.FieldName = "InvalidTime";
            this.InvalidTime.Name = "InvalidTime";
            this.InvalidTime.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            // 
            // Remark
            // 
            this.Remark.Caption = "备注";
            this.Remark.FieldName = "Remark";
            this.Remark.Name = "Remark";
            this.Remark.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            this.Remark.Visible = true;
            this.Remark.VisibleIndex = 6;
            // 
            // pageSet
            // 
            this.pageSet.Appearance.Header.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pageSet.Appearance.Header.Options.UseFont = true;
            this.pageSet.Appearance.PageClient.BackColor = System.Drawing.Color.Transparent;
            this.pageSet.Appearance.PageClient.Options.UseBackColor = true;
            this.pageSet.Controls.Add(this.layoutControl2);
            this.pageSet.Name = "pageSet";
            this.pageSet.PageEnabled = false;
            this.pageSet.Size = new System.Drawing.Size(774, 513);
            this.pageSet.Text = "车型配置";
            // 
            // layoutControl2
            // 
            this.layoutControl2.AllowCustomization = false;
            this.layoutControl2.Controls.Add(this.pictSecond);
            this.layoutControl2.Controls.Add(this.pictFirst);
            this.layoutControl2.Controls.Add(this.pictureEdit3);
            this.layoutControl2.Controls.Add(this.pictureEdit2);
            this.layoutControl2.Controls.Add(this.labelControl5);
            this.layoutControl2.Controls.Add(this.labelControl2);
            this.layoutControl2.Controls.Add(this.pictureEdit1);
            this.layoutControl2.Controls.Add(this.labelControl3);
            this.layoutControl2.Controls.Add(this.labelControl1);
            this.layoutControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl2.HiddenItems.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem15});
            this.layoutControl2.Location = new System.Drawing.Point(0, 0);
            this.layoutControl2.Name = "layoutControl2";
            this.layoutControl2.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(782, 262, 250, 350);
            this.layoutControl2.Root = this.layoutControlGroup3;
            this.layoutControl2.Size = new System.Drawing.Size(774, 513);
            this.layoutControl2.TabIndex = 0;
            this.layoutControl2.Text = "layoutControl2";
            // 
            // pictSecond
            // 
            this.pictSecond.EditValue = global::UltraANetT.Properties.Resources.second;
            this.pictSecond.Location = new System.Drawing.Point(439, 220);
            this.pictSecond.Margin = new System.Windows.Forms.Padding(1, 5, 1, 5);
            this.pictSecond.Name = "pictSecond";
            this.pictSecond.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
            this.pictSecond.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            this.pictSecond.Size = new System.Drawing.Size(282, 190);
            this.pictSecond.StyleController = this.layoutControl2;
            this.pictSecond.TabIndex = 22;
            // 
            // pictFirst
            // 
            this.pictFirst.EditValue = global::UltraANetT.Properties.Resources.first;
            this.pictFirst.Location = new System.Drawing.Point(53, 220);
            this.pictFirst.Margin = new System.Windows.Forms.Padding(1, 5, 1, 5);
            this.pictFirst.Name = "pictFirst";
            this.pictFirst.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
            this.pictFirst.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            this.pictFirst.Size = new System.Drawing.Size(286, 190);
            this.pictFirst.StyleController = this.layoutControl2;
            this.pictFirst.TabIndex = 21;
            // 
            // pictureEdit3
            // 
            this.pictureEdit3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureEdit3.Location = new System.Drawing.Point(657, 67);
            this.pictureEdit3.MaximumSize = new System.Drawing.Size(52, 48);
            this.pictureEdit3.MinimumSize = new System.Drawing.Size(52, 48);
            this.pictureEdit3.Name = "pictureEdit3";
            this.pictureEdit3.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.pictureEdit3.Properties.Appearance.Options.UseBackColor = true;
            this.pictureEdit3.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pictureEdit3.Properties.NullText = " ";
            this.pictureEdit3.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
            this.pictureEdit3.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            this.pictureEdit3.Size = new System.Drawing.Size(52, 48);
            this.pictureEdit3.StyleController = this.layoutControl2;
            this.pictureEdit3.TabIndex = 13;
            // 
            // pictureEdit2
            // 
            this.pictureEdit2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureEdit2.Location = new System.Drawing.Point(434, 67);
            this.pictureEdit2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pictureEdit2.MaximumSize = new System.Drawing.Size(52, 48);
            this.pictureEdit2.MinimumSize = new System.Drawing.Size(52, 48);
            this.pictureEdit2.Name = "pictureEdit2";
            this.pictureEdit2.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.pictureEdit2.Properties.Appearance.Options.UseBackColor = true;
            this.pictureEdit2.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pictureEdit2.Properties.NullText = "  ";
            this.pictureEdit2.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
            this.pictureEdit2.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            this.pictureEdit2.Size = new System.Drawing.Size(52, 48);
            this.pictureEdit2.StyleController = this.layoutControl2;
            this.pictureEdit2.TabIndex = 12;
            // 
            // labelControl5
            // 
            this.labelControl5.Appearance.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl5.Location = new System.Drawing.Point(493, 72);
            this.labelControl5.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.labelControl5.MaximumSize = new System.Drawing.Size(157, 0);
            this.labelControl5.MinimumSize = new System.Drawing.Size(157, 0);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(157, 40);
            this.labelControl5.StyleController = this.layoutControl2;
            this.labelControl5.TabIndex = 10;
            this.labelControl5.Text = "  第三步：\r\n车型数据配置";
            // 
            // labelControl2
            // 
            this.labelControl2.Appearance.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl2.Location = new System.Drawing.Point(270, 72);
            this.labelControl2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.labelControl2.MaximumSize = new System.Drawing.Size(157, 0);
            this.labelControl2.MinimumSize = new System.Drawing.Size(157, 0);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(157, 40);
            this.labelControl2.StyleController = this.layoutControl2;
            this.labelControl2.TabIndex = 8;
            this.labelControl2.Text = "    第二步：\r\n网络拓扑图";
            // 
            // pictureEdit1
            // 
            this.pictureEdit1.Location = new System.Drawing.Point(211, 67);
            this.pictureEdit1.Margin = new System.Windows.Forms.Padding(0);
            this.pictureEdit1.MaximumSize = new System.Drawing.Size(52, 48);
            this.pictureEdit1.MinimumSize = new System.Drawing.Size(52, 48);
            this.pictureEdit1.Name = "pictureEdit1";
            this.pictureEdit1.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.pictureEdit1.Properties.Appearance.Options.UseBackColor = true;
            this.pictureEdit1.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pictureEdit1.Properties.NullText = "  ";
            this.pictureEdit1.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
            this.pictureEdit1.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            this.pictureEdit1.Size = new System.Drawing.Size(52, 48);
            this.pictureEdit1.StyleController = this.layoutControl2;
            this.pictureEdit1.TabIndex = 7;
            // 
            // labelControl3
            // 
            this.labelControl3.Appearance.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl3.Location = new System.Drawing.Point(47, 72);
            this.labelControl3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.labelControl3.MaximumSize = new System.Drawing.Size(157, 0);
            this.labelControl3.MinimumSize = new System.Drawing.Size(157, 0);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(157, 40);
            this.labelControl3.StyleController = this.layoutControl2;
            this.labelControl3.TabIndex = 6;
            this.labelControl3.Text = "     第一步：\r\n  DBC上传\r\n";
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelControl1.Location = new System.Drawing.Point(7, 13);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(171, 25);
            this.labelControl1.StyleController = this.layoutControl2;
            this.labelControl1.TabIndex = 4;
            this.labelControl1.Text = "车辆配置信息检测：";
            // 
            // layoutControlItem15
            // 
            this.layoutControlItem15.Location = new System.Drawing.Point(0, 334);
            this.layoutControlItem15.Name = "layoutControlItem15";
            this.layoutControlItem15.Padding = new DevExpress.XtraLayout.Utils.Padding(80, 30, 80, 23);
            this.layoutControlItem15.Size = new System.Drawing.Size(675, 158);
            this.layoutControlItem15.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem15.TextVisible = false;
            // 
            // layoutControlGroup3
            // 
            this.layoutControlGroup3.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup3.GroupBordersVisible = false;
            this.layoutControlGroup3.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.emptySpaceItem1,
            this.layoutControlItem4,
            this.emptySpaceItem5,
            this.layoutControlItem7,
            this.layoutControlItem6,
            this.layoutControlItem8,
            this.layoutControlItem10,
            this.layoutControlItem9,
            this.emptySpaceItem4,
            this.layoutControlItem29,
            this.layoutControlItem12,
            this.emptySpaceItem2,
            this.layoutControlItem24});
            this.layoutControlGroup3.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup3.Name = "Root";
            this.layoutControlGroup3.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 3, 3);
            this.layoutControlGroup3.Size = new System.Drawing.Size(774, 513);
            this.layoutControlGroup3.TextVisible = false;
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.Location = new System.Drawing.Point(179, 0);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(589, 45);
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.labelControl1;
            this.layoutControlItem4.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Padding = new DevExpress.XtraLayout.Utils.Padding(4, 4, 10, 10);
            this.layoutControlItem4.Size = new System.Drawing.Size(179, 45);
            this.layoutControlItem4.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem4.TextVisible = false;
            // 
            // emptySpaceItem5
            // 
            this.emptySpaceItem5.AllowHotTrack = false;
            this.emptySpaceItem5.Location = new System.Drawing.Point(0, 59);
            this.emptySpaceItem5.Name = "emptySpaceItem5";
            this.emptySpaceItem5.Size = new System.Drawing.Size(39, 58);
            this.emptySpaceItem5.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem7
            // 
            this.layoutControlItem7.Control = this.labelControl3;
            this.layoutControlItem7.ControlAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.layoutControlItem7.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.layoutControlItem7.Location = new System.Drawing.Point(39, 59);
            this.layoutControlItem7.Name = "layoutControlItem7";
            this.layoutControlItem7.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 10, 5);
            this.layoutControlItem7.Size = new System.Drawing.Size(167, 58);
            this.layoutControlItem7.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem7.TextVisible = false;
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.Control = this.pictureEdit1;
            this.layoutControlItem6.Location = new System.Drawing.Point(206, 59);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Padding = new DevExpress.XtraLayout.Utils.Padding(2, 2, 5, 5);
            this.layoutControlItem6.Size = new System.Drawing.Size(56, 58);
            this.layoutControlItem6.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem6.TextVisible = false;
            // 
            // layoutControlItem8
            // 
            this.layoutControlItem8.Control = this.labelControl2;
            this.layoutControlItem8.Location = new System.Drawing.Point(262, 59);
            this.layoutControlItem8.Name = "layoutControlItem8";
            this.layoutControlItem8.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 10, 5);
            this.layoutControlItem8.Size = new System.Drawing.Size(167, 58);
            this.layoutControlItem8.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem8.TextVisible = false;
            // 
            // layoutControlItem10
            // 
            this.layoutControlItem10.Control = this.labelControl5;
            this.layoutControlItem10.Location = new System.Drawing.Point(485, 59);
            this.layoutControlItem10.Name = "layoutControlItem10";
            this.layoutControlItem10.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 10, 5);
            this.layoutControlItem10.Size = new System.Drawing.Size(167, 58);
            this.layoutControlItem10.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem10.TextVisible = false;
            // 
            // layoutControlItem9
            // 
            this.layoutControlItem9.Control = this.pictureEdit2;
            this.layoutControlItem9.Location = new System.Drawing.Point(429, 59);
            this.layoutControlItem9.Name = "layoutControlItem9";
            this.layoutControlItem9.Padding = new DevExpress.XtraLayout.Utils.Padding(2, 2, 5, 5);
            this.layoutControlItem9.Size = new System.Drawing.Size(56, 58);
            this.layoutControlItem9.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem9.TextVisible = false;
            // 
            // emptySpaceItem4
            // 
            this.emptySpaceItem4.AllowHotTrack = false;
            this.emptySpaceItem4.ControlAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.emptySpaceItem4.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.emptySpaceItem4.Location = new System.Drawing.Point(0, 45);
            this.emptySpaceItem4.Name = "emptySpaceItem4";
            this.emptySpaceItem4.Size = new System.Drawing.Size(768, 14);
            this.emptySpaceItem4.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem29
            // 
            this.layoutControlItem29.Control = this.pictSecond;
            this.layoutControlItem29.Location = new System.Drawing.Point(386, 117);
            this.layoutControlItem29.Name = "layoutControlItem29";
            this.layoutControlItem29.Padding = new DevExpress.XtraLayout.Utils.Padding(50, 50, 100, 100);
            this.layoutControlItem29.Size = new System.Drawing.Size(382, 390);
            this.layoutControlItem29.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem29.TextVisible = false;
            // 
            // layoutControlItem12
            // 
            this.layoutControlItem12.Control = this.pictureEdit3;
            this.layoutControlItem12.Location = new System.Drawing.Point(652, 59);
            this.layoutControlItem12.Name = "layoutControlItem12";
            this.layoutControlItem12.Padding = new DevExpress.XtraLayout.Utils.Padding(2, 2, 5, 5);
            this.layoutControlItem12.Size = new System.Drawing.Size(56, 58);
            this.layoutControlItem12.TextLocation = DevExpress.Utils.Locations.Left;
            this.layoutControlItem12.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem12.TextVisible = false;
            // 
            // emptySpaceItem2
            // 
            this.emptySpaceItem2.AllowHotTrack = false;
            this.emptySpaceItem2.Location = new System.Drawing.Point(708, 59);
            this.emptySpaceItem2.Name = "emptySpaceItem2";
            this.emptySpaceItem2.Size = new System.Drawing.Size(60, 58);
            this.emptySpaceItem2.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem24
            // 
            this.layoutControlItem24.Control = this.pictFirst;
            this.layoutControlItem24.Location = new System.Drawing.Point(0, 117);
            this.layoutControlItem24.Name = "layoutControlItem24";
            this.layoutControlItem24.Padding = new DevExpress.XtraLayout.Utils.Padding(50, 50, 100, 100);
            this.layoutControlItem24.Size = new System.Drawing.Size(386, 390);
            this.layoutControlItem24.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem24.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlItem1.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlItem1.Control = this.comboBoxEdit1;
            this.layoutControlItem1.Location = new System.Drawing.Point(108, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(108, 30);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem3,
            this.layoutControlGroup2});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(2, 2, 2, 2);
            this.layoutControlGroup1.Size = new System.Drawing.Size(998, 550);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.tabAuthorize;
            this.layoutControlItem3.Location = new System.Drawing.Point(214, 0);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Size = new System.Drawing.Size(780, 546);
            this.layoutControlItem3.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem3.TextVisible = false;
            // 
            // layoutControlGroup2
            // 
            this.layoutControlGroup2.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem2,
            this.layoutControlItem5});
            this.layoutControlGroup2.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup2.Name = "layoutControlGroup2";
            this.layoutControlGroup2.Padding = new DevExpress.XtraLayout.Utils.Padding(2, 2, 2, 2);
            this.layoutControlGroup2.Size = new System.Drawing.Size(214, 546);
            this.layoutControlGroup2.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.vehicelTree;
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 32);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(204, 504);
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextVisible = false;
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.Control = this.label1;
            this.layoutControlItem5.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Padding = new DevExpress.XtraLayout.Utils.Padding(6, 6, 6, 6);
            this.layoutControlItem5.Size = new System.Drawing.Size(204, 32);
            this.layoutControlItem5.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem5.TextVisible = false;
            // 
            // CMSTable
            // 
            this.CMSTable.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.CMSTable.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiUpdate,
            this.tsmiAdd,
            this.tsmiDel});
            this.CMSTable.Name = "CMSTable";
            this.CMSTable.Size = new System.Drawing.Size(101, 70);
            // 
            // tsmiUpdate
            // 
            this.tsmiUpdate.Name = "tsmiUpdate";
            this.tsmiUpdate.Size = new System.Drawing.Size(100, 22);
            this.tsmiUpdate.Text = "修改";
            this.tsmiUpdate.Click += new System.EventHandler(this.tsmiUpdate_Click);
            // 
            // tsmiAdd
            // 
            this.tsmiAdd.Name = "tsmiAdd";
            this.tsmiAdd.Size = new System.Drawing.Size(100, 22);
            this.tsmiAdd.Text = "添加";
            this.tsmiAdd.Visible = false;
            // 
            // tsmiDel
            // 
            this.tsmiDel.Name = "tsmiDel";
            this.tsmiDel.Size = new System.Drawing.Size(100, 22);
            this.tsmiDel.Text = "删除";
            this.tsmiDel.Click += new System.EventHandler(this.tsmiDel_Click);
            // 
            // CMSVehicel
            // 
            this.CMSVehicel.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.CMSVehicel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCreateConfig,
            this.tsmiDelVehicel,
            this.tsmiReNameVehicel});
            this.CMSVehicel.Name = "CMSTask";
            this.CMSVehicel.Size = new System.Drawing.Size(137, 70);
            // 
            // tsmiCreateConfig
            // 
            this.tsmiCreateConfig.Name = "tsmiCreateConfig";
            this.tsmiCreateConfig.Size = new System.Drawing.Size(136, 22);
            this.tsmiCreateConfig.Text = "创建配置";
            // 
            // tsmiDelVehicel
            // 
            this.tsmiDelVehicel.Name = "tsmiDelVehicel";
            this.tsmiDelVehicel.Size = new System.Drawing.Size(136, 22);
            this.tsmiDelVehicel.Text = "删除车型";
            // 
            // tsmiReNameVehicel
            // 
            this.tsmiReNameVehicel.Name = "tsmiReNameVehicel";
            this.tsmiReNameVehicel.Size = new System.Drawing.Size(136, 22);
            this.tsmiReNameVehicel.Text = "重命名车型";
            this.tsmiReNameVehicel.Visible = false;
            // 
            // CMSCreate
            // 
            this.CMSCreate.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.CMSCreate.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCreateVehicel,
            this.tsmiView});
            this.CMSCreate.Name = "CMSTask";
            this.CMSCreate.Size = new System.Drawing.Size(161, 48);
            // 
            // tsmiCreateVehicel
            // 
            this.tsmiCreateVehicel.Name = "tsmiCreateVehicel";
            this.tsmiCreateVehicel.Size = new System.Drawing.Size(160, 22);
            this.tsmiCreateVehicel.Text = "创建车型";
            // 
            // tsmiView
            // 
            this.tsmiView.Name = "tsmiView";
            this.tsmiView.Size = new System.Drawing.Size(160, 22);
            this.tsmiView.Text = "查看授权表全部";
            this.tsmiView.Click += new System.EventHandler(this.tsmiView_Click);
            // 
            // CMSConfig
            // 
            this.CMSConfig.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.CMSConfig.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCreateStage,
            this.tsmiDelConfig,
            this.tsmiReNameConfig,
            this.tsmiPasteStage});
            this.CMSConfig.Name = "CMSTask";
            this.CMSConfig.Size = new System.Drawing.Size(137, 92);
            // 
            // tsmiCreateStage
            // 
            this.tsmiCreateStage.Name = "tsmiCreateStage";
            this.tsmiCreateStage.Size = new System.Drawing.Size(136, 22);
            this.tsmiCreateStage.Text = "创建阶段";
            // 
            // tsmiDelConfig
            // 
            this.tsmiDelConfig.Name = "tsmiDelConfig";
            this.tsmiDelConfig.Size = new System.Drawing.Size(136, 22);
            this.tsmiDelConfig.Text = "删除配置";
            // 
            // tsmiReNameConfig
            // 
            this.tsmiReNameConfig.Name = "tsmiReNameConfig";
            this.tsmiReNameConfig.Size = new System.Drawing.Size(136, 22);
            this.tsmiReNameConfig.Text = "重命名配置";
            this.tsmiReNameConfig.Visible = false;
            // 
            // tsmiPasteStage
            // 
            this.tsmiPasteStage.Enabled = false;
            this.tsmiPasteStage.Name = "tsmiPasteStage";
            this.tsmiPasteStage.Size = new System.Drawing.Size(136, 22);
            this.tsmiPasteStage.Text = "粘贴阶段";
            this.tsmiPasteStage.Click += new System.EventHandler(this.tsmiPasteStage_Click);
            // 
            // CMSStage
            // 
            this.CMSStage.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.CMSStage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAuthorize,
            this.tsmiDelStage,
            this.tsmiCopyStage});
            this.CMSStage.Name = "CMSTask";
            this.CMSStage.Size = new System.Drawing.Size(125, 70);
            // 
            // tsmiAuthorize
            // 
            this.tsmiAuthorize.Name = "tsmiAuthorize";
            this.tsmiAuthorize.Size = new System.Drawing.Size(124, 22);
            this.tsmiAuthorize.Text = "授权";
            this.tsmiAuthorize.Click += new System.EventHandler(this.tsmiAdd_Click);
            // 
            // tsmiDelStage
            // 
            this.tsmiDelStage.Name = "tsmiDelStage";
            this.tsmiDelStage.Size = new System.Drawing.Size(124, 22);
            this.tsmiDelStage.Text = "删除阶段";
            // 
            // tsmiCopyStage
            // 
            this.tsmiCopyStage.Name = "tsmiCopyStage";
            this.tsmiCopyStage.Size = new System.Drawing.Size(124, 22);
            this.tsmiCopyStage.Text = "复制阶段";
            this.tsmiCopyStage.Click += new System.EventHandler(this.tsmiCopyStage_Click);
            // 
            // emptySpaceItem3
            // 
            this.emptySpaceItem3.AllowHotTrack = false;
            this.emptySpaceItem3.Location = new System.Drawing.Point(0, 33);
            this.emptySpaceItem3.Name = "emptySpaceItem3";
            this.emptySpaceItem3.Size = new System.Drawing.Size(118, 451);
            this.emptySpaceItem3.TextSize = new System.Drawing.Size(0, 0);
            // 
            // dmVehicel
            // 
            this.dmVehicel.AutoHideContainers.AddRange(new DevExpress.XtraBars.Docking.AutoHideContainer[] {
            this.hideContainerRight});
            this.dmVehicel.Form = this;
            this.dmVehicel.TopZIndexControls.AddRange(new string[] {
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
            this.hideContainerRight.Controls.Add(this.dpVehicel);
            this.hideContainerRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.hideContainerRight.Location = new System.Drawing.Point(998, 0);
            this.hideContainerRight.Name = "hideContainerRight";
            this.hideContainerRight.Size = new System.Drawing.Size(24, 550);
            this.hideContainerRight.Visible = false;
            // 
            // dpVehicel
            // 
            this.dpVehicel.Controls.Add(this.dockPanel1_Container);
            this.dpVehicel.Dock = DevExpress.XtraBars.Docking.DockingStyle.Right;
            this.dpVehicel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dpVehicel.ID = new System.Guid("a04f6222-4e48-44b0-94d3-459edb317bbb");
            this.dpVehicel.Location = new System.Drawing.Point(0, 0);
            this.dpVehicel.Margin = new System.Windows.Forms.Padding(1, 5, 1, 5);
            this.dpVehicel.Name = "dpVehicel";
            this.dpVehicel.OriginalSize = new System.Drawing.Size(280, 200);
            this.dpVehicel.SavedDock = DevExpress.XtraBars.Docking.DockingStyle.Right;
            this.dpVehicel.SavedIndex = 0;
            this.dpVehicel.Size = new System.Drawing.Size(280, 550);
            this.dpVehicel.TabsPosition = DevExpress.XtraBars.Docking.TabsPosition.Right;
            this.dpVehicel.Text = "授权信息";
            this.dpVehicel.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            // 
            // dockPanel1_Container
            // 
            this.dockPanel1_Container.Controls.Add(this.layoutControl3);
            this.dockPanel1_Container.Location = new System.Drawing.Point(4, 39);
            this.dockPanel1_Container.Margin = new System.Windows.Forms.Padding(1, 8, 1, 8);
            this.dockPanel1_Container.Name = "dockPanel1_Container";
            this.dockPanel1_Container.Size = new System.Drawing.Size(272, 507);
            this.dockPanel1_Container.TabIndex = 0;
            // 
            // layoutControl3
            // 
            this.layoutControl3.AllowCustomization = false;
            this.layoutControl3.Controls.Add(this.txtCreater);
            this.layoutControl3.Controls.Add(this.dateInvalid);
            this.layoutControl3.Controls.Add(this.dateAuth);
            this.layoutControl3.Controls.Add(this.dateCreate);
            this.layoutControl3.Controls.Add(this.btnSubmit);
            this.layoutControl3.Controls.Add(this.txtRemark);
            this.layoutControl3.Controls.Add(this.labelControl8);
            this.layoutControl3.Controls.Add(this.cbVehicelConfig);
            this.layoutControl3.Controls.Add(this.cbvehicelStage);
            this.layoutControl3.Controls.Add(this.cbvehicelType);
            this.layoutControl3.Controls.Add(this.cbAuthTo);
            this.layoutControl3.Controls.Add(this.cbDepartment);
            this.layoutControl3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl3.Location = new System.Drawing.Point(0, 0);
            this.layoutControl3.Margin = new System.Windows.Forms.Padding(1, 5, 1, 5);
            this.layoutControl3.Name = "layoutControl3";
            this.layoutControl3.Root = this.layoutControlGroup4;
            this.layoutControl3.Size = new System.Drawing.Size(272, 507);
            this.layoutControl3.TabIndex = 0;
            this.layoutControl3.Text = "layoutControl3";
            // 
            // txtCreater
            // 
            this.txtCreater.Location = new System.Drawing.Point(94, 117);
            this.txtCreater.Margin = new System.Windows.Forms.Padding(4);
            this.txtCreater.Name = "txtCreater";
            this.txtCreater.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCreater.Properties.Appearance.Options.UseFont = true;
            this.txtCreater.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.txtCreater.Properties.ReadOnly = true;
            this.txtCreater.Size = new System.Drawing.Size(173, 24);
            this.txtCreater.StyleController = this.layoutControl3;
            this.txtCreater.TabIndex = 21;
            // 
            // dateInvalid
            // 
            this.dateInvalid.EditValue = null;
            this.dateInvalid.Location = new System.Drawing.Point(94, 229);
            this.dateInvalid.Margin = new System.Windows.Forms.Padding(1, 4, 1, 4);
            this.dateInvalid.Name = "dateInvalid";
            this.dateInvalid.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateInvalid.Properties.Appearance.Options.UseFont = true;
            this.dateInvalid.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateInvalid.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateInvalid.Size = new System.Drawing.Size(173, 24);
            this.dateInvalid.StyleController = this.layoutControl3;
            this.dateInvalid.TabIndex = 20;
            // 
            // dateAuth
            // 
            this.dateAuth.EditValue = "";
            this.dateAuth.Location = new System.Drawing.Point(94, 201);
            this.dateAuth.Margin = new System.Windows.Forms.Padding(1, 4, 1, 4);
            this.dateAuth.Name = "dateAuth";
            this.dateAuth.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateAuth.Properties.Appearance.Options.UseFont = true;
            this.dateAuth.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateAuth.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateAuth.Size = new System.Drawing.Size(173, 24);
            this.dateAuth.StyleController = this.layoutControl3;
            this.dateAuth.TabIndex = 19;
            // 
            // dateCreate
            // 
            this.dateCreate.EditValue = null;
            this.dateCreate.Location = new System.Drawing.Point(94, 89);
            this.dateCreate.Margin = new System.Windows.Forms.Padding(1, 4, 1, 4);
            this.dateCreate.Name = "dateCreate";
            this.dateCreate.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dateCreate.Properties.Appearance.Options.UseFont = true;
            this.dateCreate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateCreate.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateCreate.Properties.ReadOnly = true;
            this.dateCreate.Size = new System.Drawing.Size(173, 24);
            this.dateCreate.StyleController = this.layoutControl3;
            this.dateCreate.TabIndex = 18;
            // 
            // btnSubmit
            // 
            this.btnSubmit.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSubmit.Appearance.Options.UseFont = true;
            this.btnSubmit.Location = new System.Drawing.Point(5, 480);
            this.btnSubmit.Margin = new System.Windows.Forms.Padding(1, 6, 1, 6);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(262, 22);
            this.btnSubmit.StyleController = this.layoutControl3;
            this.btnSubmit.TabIndex = 17;
            this.btnSubmit.Text = "提交";
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // txtRemark
            // 
            this.txtRemark.Location = new System.Drawing.Point(5, 278);
            this.txtRemark.Margin = new System.Windows.Forms.Padding(1, 4, 1, 4);
            this.txtRemark.Name = "txtRemark";
            this.txtRemark.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRemark.Properties.Appearance.Options.UseFont = true;
            this.txtRemark.Size = new System.Drawing.Size(262, 198);
            this.txtRemark.StyleController = this.layoutControl3;
            this.txtRemark.TabIndex = 16;
            this.txtRemark.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtRemark_MouseUp);
            // 
            // labelControl8
            // 
            this.labelControl8.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl8.Location = new System.Drawing.Point(5, 257);
            this.labelControl8.Margin = new System.Windows.Forms.Padding(1, 4, 1, 4);
            this.labelControl8.Name = "labelControl8";
            this.labelControl8.Size = new System.Drawing.Size(36, 17);
            this.labelControl8.StyleController = this.layoutControl3;
            this.labelControl8.TabIndex = 15;
            this.labelControl8.Text = "备注：";
            // 
            // cbVehicelConfig
            // 
            this.cbVehicelConfig.Enabled = false;
            this.cbVehicelConfig.Location = new System.Drawing.Point(94, 33);
            this.cbVehicelConfig.Margin = new System.Windows.Forms.Padding(1, 4, 1, 4);
            this.cbVehicelConfig.Name = "cbVehicelConfig";
            this.cbVehicelConfig.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbVehicelConfig.Properties.Appearance.Options.UseFont = true;
            this.cbVehicelConfig.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbVehicelConfig.Properties.ReadOnly = true;
            this.cbVehicelConfig.Size = new System.Drawing.Size(173, 24);
            this.cbVehicelConfig.StyleController = this.layoutControl3;
            this.cbVehicelConfig.TabIndex = 6;
            // 
            // cbvehicelStage
            // 
            this.cbvehicelStage.Enabled = false;
            this.cbvehicelStage.Location = new System.Drawing.Point(94, 61);
            this.cbvehicelStage.Margin = new System.Windows.Forms.Padding(1, 4, 1, 4);
            this.cbvehicelStage.Name = "cbvehicelStage";
            this.cbvehicelStage.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbvehicelStage.Properties.Appearance.Options.UseFont = true;
            this.cbvehicelStage.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbvehicelStage.Properties.ReadOnly = true;
            this.cbvehicelStage.Size = new System.Drawing.Size(173, 24);
            this.cbvehicelStage.StyleController = this.layoutControl3;
            this.cbvehicelStage.TabIndex = 5;
            // 
            // cbvehicelType
            // 
            this.cbvehicelType.Location = new System.Drawing.Point(94, 5);
            this.cbvehicelType.Margin = new System.Windows.Forms.Padding(1, 4, 1, 4);
            this.cbvehicelType.Name = "cbvehicelType";
            this.cbvehicelType.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbvehicelType.Properties.Appearance.Options.UseFont = true;
            this.cbvehicelType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbvehicelType.Properties.ReadOnly = true;
            this.cbvehicelType.Size = new System.Drawing.Size(173, 24);
            this.cbvehicelType.StyleController = this.layoutControl3;
            this.cbvehicelType.TabIndex = 4;
            // 
            // cbAuthTo
            // 
            this.cbAuthTo.Location = new System.Drawing.Point(94, 145);
            this.cbAuthTo.Margin = new System.Windows.Forms.Padding(1, 4, 1, 4);
            this.cbAuthTo.Name = "cbAuthTo";
            this.cbAuthTo.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbAuthTo.Properties.Appearance.Options.UseFont = true;
            this.cbAuthTo.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbAuthTo.Size = new System.Drawing.Size(173, 24);
            this.cbAuthTo.StyleController = this.layoutControl3;
            this.cbAuthTo.TabIndex = 7;
            // 
            // cbDepartment
            // 
            this.cbDepartment.Location = new System.Drawing.Point(94, 173);
            this.cbDepartment.Margin = new System.Windows.Forms.Padding(1, 4, 1, 4);
            this.cbDepartment.Name = "cbDepartment";
            this.cbDepartment.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbDepartment.Properties.Appearance.Options.UseFont = true;
            this.cbDepartment.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbDepartment.Size = new System.Drawing.Size(173, 24);
            this.cbDepartment.StyleController = this.layoutControl3;
            this.cbDepartment.TabIndex = 11;
            // 
            // layoutControlGroup4
            // 
            this.layoutControlGroup4.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlGroup4.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlGroup4.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup4.GroupBordersVisible = false;
            this.layoutControlGroup4.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem20,
            this.layoutControlItem21,
            this.layoutControlItem22,
            this.configer,
            this.configeDep,
            this.layoutControlItem30,
            this.layoutControlItem26,
            this.layoutControlItem31,
            this.AuthorizingTime,
            this.startTime,
            this.layoutControlItem28,
            this.Author});
            this.layoutControlGroup4.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup4.Name = "layoutControlGroup4";
            this.layoutControlGroup4.OptionsItemText.TextToControlDistance = 5;
            this.layoutControlGroup4.Padding = new DevExpress.XtraLayout.Utils.Padding(3, 3, 3, 3);
            this.layoutControlGroup4.Size = new System.Drawing.Size(272, 507);
            this.layoutControlGroup4.TextVisible = false;
            // 
            // layoutControlItem20
            // 
            this.layoutControlItem20.Control = this.cbvehicelType;
            this.layoutControlItem20.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem20.Name = "layoutControlItem20";
            this.layoutControlItem20.Size = new System.Drawing.Size(266, 28);
            this.layoutControlItem20.Text = "车型：";
            this.layoutControlItem20.TextSize = new System.Drawing.Size(84, 17);
            // 
            // layoutControlItem21
            // 
            this.layoutControlItem21.Control = this.cbvehicelStage;
            this.layoutControlItem21.Location = new System.Drawing.Point(0, 56);
            this.layoutControlItem21.Name = "layoutControlItem21";
            this.layoutControlItem21.Size = new System.Drawing.Size(266, 28);
            this.layoutControlItem21.Text = "阶段：";
            this.layoutControlItem21.TextSize = new System.Drawing.Size(84, 17);
            // 
            // layoutControlItem22
            // 
            this.layoutControlItem22.Control = this.cbVehicelConfig;
            this.layoutControlItem22.Location = new System.Drawing.Point(0, 28);
            this.layoutControlItem22.Name = "layoutControlItem22";
            this.layoutControlItem22.Size = new System.Drawing.Size(266, 28);
            this.layoutControlItem22.Text = "配置:";
            this.layoutControlItem22.TextSize = new System.Drawing.Size(84, 17);
            // 
            // configer
            // 
            this.configer.Control = this.cbAuthTo;
            this.configer.CustomizationFormText = "配置员：";
            this.configer.Location = new System.Drawing.Point(0, 140);
            this.configer.Name = "configer";
            this.configer.Size = new System.Drawing.Size(266, 28);
            this.configer.Text = "配置员：";
            this.configer.TextSize = new System.Drawing.Size(84, 17);
            // 
            // configeDep
            // 
            this.configeDep.Control = this.cbDepartment;
            this.configeDep.Location = new System.Drawing.Point(0, 168);
            this.configeDep.Name = "configeDep";
            this.configeDep.Size = new System.Drawing.Size(266, 28);
            this.configeDep.Text = "配置部门：";
            this.configeDep.TextSize = new System.Drawing.Size(84, 17);
            this.configeDep.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            // 
            // layoutControlItem30
            // 
            this.layoutControlItem30.Control = this.labelControl8;
            this.layoutControlItem30.Location = new System.Drawing.Point(0, 252);
            this.layoutControlItem30.Name = "layoutControlItem30";
            this.layoutControlItem30.Size = new System.Drawing.Size(266, 21);
            this.layoutControlItem30.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem30.TextVisible = false;
            // 
            // layoutControlItem26
            // 
            this.layoutControlItem26.Control = this.txtRemark;
            this.layoutControlItem26.Location = new System.Drawing.Point(0, 273);
            this.layoutControlItem26.Name = "layoutControlItem26";
            this.layoutControlItem26.Size = new System.Drawing.Size(266, 202);
            this.layoutControlItem26.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem26.TextVisible = false;
            // 
            // layoutControlItem31
            // 
            this.layoutControlItem31.Control = this.btnSubmit;
            this.layoutControlItem31.Location = new System.Drawing.Point(0, 475);
            this.layoutControlItem31.Name = "layoutControlItem31";
            this.layoutControlItem31.Size = new System.Drawing.Size(266, 26);
            this.layoutControlItem31.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem31.TextVisible = false;
            // 
            // AuthorizingTime
            // 
            this.AuthorizingTime.Control = this.dateCreate;
            this.AuthorizingTime.CustomizationFormText = "授权时间：";
            this.AuthorizingTime.Location = new System.Drawing.Point(0, 84);
            this.AuthorizingTime.Name = "AuthorizingTime";
            this.AuthorizingTime.Size = new System.Drawing.Size(266, 28);
            this.AuthorizingTime.Text = "授权时间：";
            this.AuthorizingTime.TextSize = new System.Drawing.Size(84, 17);
            // 
            // startTime
            // 
            this.startTime.Control = this.dateAuth;
            this.startTime.Location = new System.Drawing.Point(0, 196);
            this.startTime.Name = "startTime";
            this.startTime.Size = new System.Drawing.Size(266, 28);
            this.startTime.Text = "配置开始时间：";
            this.startTime.TextSize = new System.Drawing.Size(84, 17);
            this.startTime.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            // 
            // layoutControlItem28
            // 
            this.layoutControlItem28.Control = this.dateInvalid;
            this.layoutControlItem28.Location = new System.Drawing.Point(0, 224);
            this.layoutControlItem28.Name = "layoutControlItem28";
            this.layoutControlItem28.Size = new System.Drawing.Size(266, 28);
            this.layoutControlItem28.Text = "配置失效时间：";
            this.layoutControlItem28.TextSize = new System.Drawing.Size(84, 17);
            this.layoutControlItem28.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            // 
            // Author
            // 
            this.Author.Control = this.txtCreater;
            this.Author.Location = new System.Drawing.Point(0, 112);
            this.Author.Name = "Author";
            this.Author.Size = new System.Drawing.Size(266, 28);
            this.Author.Text = "授权人：";
            this.Author.TextSize = new System.Drawing.Size(84, 17);
            // 
            // DLAF
            // 
            this.DLAF.LookAndFeel.SkinName = "Office 2016 Colorful";
            // 
            // Vehicel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.layoutControl1);
            this.Controls.Add(this.hideContainerRight);
            this.LookAndFeel.SkinName = "Office 2016 Dark";
            this.Name = "Vehicel";
            this.Size = new System.Drawing.Size(1022, 550);
            ((System.ComponentModel.ISupportInitialize)(this.imageCollection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.vehicelTree)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageTree)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabAuthorize)).EndInit();
            this.tabAuthorize.ResumeLayout(false);
            this.pageAuthorize.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcVechiel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvVechiel)).EndInit();
            this.pageSet.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl2)).EndInit();
            this.layoutControl2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictSecond.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictFirst.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureEdit3.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureEdit2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem15)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem10)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem29)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem12)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem24)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            this.CMSTable.ResumeLayout(false);
            this.CMSVehicel.ResumeLayout(false);
            this.CMSCreate.ResumeLayout(false);
            this.CMSConfig.ResumeLayout(false);
            this.CMSStage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dmVehicel)).EndInit();
            this.hideContainerRight.ResumeLayout(false);
            this.dpVehicel.ResumeLayout(false);
            this.dockPanel1_Container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl3)).EndInit();
            this.layoutControl3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtCreater.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateInvalid.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateInvalid.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateAuth.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateAuth.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateCreate.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateCreate.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbVehicelConfig.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbvehicelStage.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbvehicelType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbAuthTo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbDepartment.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem20)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem21)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem22)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.configer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.configeDep)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem30)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem26)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem31)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AuthorizingTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.startTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem28)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Author)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private ImageCollection imageCollection;
        private LayoutControl layoutControl1;
        private LayoutControlGroup layoutControlGroup1;
        private XtraTabControl tabAuthorize;
        private XtraTabPage pageAuthorize;
        private XtraTabPage pageSet;
        private LayoutControlItem layoutControlItem3;
        private ComboBoxEdit comboBoxEdit1;
        private LayoutControlItem layoutControlItem1;
        private LayoutControlGroup layoutControlGroup2;
        private ImageCollection imageTree;
        private ContextMenuStrip CMSVehicel;
        private ToolStripMenuItem tsmiCreateConfig;
        private ToolStripMenuItem tsmiDelVehicel;
        private ContextMenuStrip CMSCreate;
        private ToolStripMenuItem tsmiCreateVehicel;
        private ToolStripMenuItem tsmiReNameVehicel;
        private ContextMenuStrip CMSConfig;
        private ToolStripMenuItem tsmiCreateStage;
        private ToolStripMenuItem tsmiDelConfig;
        private ToolStripMenuItem tsmiReNameConfig;
        private ContextMenuStrip CMSStage;
        private ToolStripMenuItem tsmiAuthorize;
        private ToolStripMenuItem tsmiDelStage;
        private GridControl gcVechiel;
        private GridView gvVechiel;
        private GridColumn VehicelType;
        private GridColumn VehicelConfig;
        private GridColumn VehicelStage;
        private GridColumn CreateTime;
        private GridColumn Creater;
        private GridColumn AuthorizeTo;
        private GridColumn FromDepartment;
        private GridColumn AuthorizationTime;
        private GridColumn InvalidTime;
        private LayoutControl layoutControl2;
        private PictureEdit pictureEdit3;
        private PictureEdit pictureEdit2;
        private LabelControl labelControl5;
        private LabelControl labelControl2;
        private PictureEdit pictureEdit1;
        private LabelControl labelControl3;
        private LabelControl labelControl1;
        private LayoutControlGroup layoutControlGroup3;
        private EmptySpaceItem emptySpaceItem1;
        private LayoutControlItem layoutControlItem4;
        private EmptySpaceItem emptySpaceItem5;
        private LayoutControlItem layoutControlItem7;
        private LayoutControlItem layoutControlItem6;
        private LayoutControlItem layoutControlItem8;
        private LayoutControlItem layoutControlItem10;
        private LayoutControlItem layoutControlItem9;
        private LayoutControlItem layoutControlItem12;
        private EmptySpaceItem emptySpaceItem3;
        private DevExpress.XtraBars.Docking.DockManager dmVehicel;
        private DevExpress.XtraBars.Docking.DockPanel dpVehicel;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel1_Container;
        private LayoutControl layoutControl3;
        private ComboBoxEdit cbVehicelConfig;
        private ComboBoxEdit cbvehicelStage;
        private ComboBoxEdit cbvehicelType;
        private LayoutControlGroup layoutControlGroup4;
        private LayoutControlItem layoutControlItem20;
        private LayoutControlItem layoutControlItem21;
        private LayoutControlItem layoutControlItem22;
        private LayoutControlItem configer;
        private GridColumn Remark;
        private MemoEdit txtRemark;
        private LabelControl labelControl8;
        private LayoutControlItem configeDep;
        private LayoutControlItem layoutControlItem30;
        private LayoutControlItem layoutControlItem26;
        private SimpleButton btnSubmit;
        private LayoutControlItem layoutControlItem31;
        private DateEdit dateInvalid;
        private DateEdit dateAuth;
        private DateEdit dateCreate;
        private LayoutControlItem AuthorizingTime;
        private LayoutControlItem startTime;
        private LayoutControlItem layoutControlItem28;
        private PictureEdit pictFirst;
        private LayoutControlItem layoutControlItem15;
        private LayoutControlItem layoutControlItem24;
        private PictureEdit pictSecond;
        private LayoutControlItem layoutControlItem29;
        private ContextMenuStrip CMSTable;
        private ToolStripMenuItem tsmiAdd;
        private ToolStripMenuItem tsmiUpdate;
        private ToolStripMenuItem tsmiDel;
        private DevExpress.LookAndFeel.DefaultLookAndFeel DLAF;
        private ComboBoxEdit txtCreater;
        private LayoutControlItem Author;
        private ToolStripMenuItem tsmiView;
        private DevExpress.XtraBars.Docking.AutoHideContainer hideContainerRight;
        private EmptySpaceItem emptySpaceItem4;
        private CheckedComboBoxEdit cbAuthTo;
        private CheckedComboBoxEdit cbDepartment;
        private ToolStripMenuItem tsmiPasteStage;
        private ToolStripMenuItem tsmiCopyStage;
        private EmptySpaceItem emptySpaceItem2;
        private Label label1;
        private TreeList vehicelTree;
        private TreeListColumn colName;
        private LayoutControlItem layoutControlItem2;
        private LayoutControlItem layoutControlItem5;
    }
}
