namespace UltraANetT.Module
{
    partial class Suppier
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
            this.btnSubmit = new DevExpress.XtraEditors.SimpleButton();
            this.txtTel = new DevExpress.XtraEditors.TextEdit();
            this.txtContact = new DevExpress.XtraEditors.TextEdit();
            this.txtSupplier = new DevExpress.XtraEditors.TextEdit();
            this.txtType = new DevExpress.XtraEditors.TextEdit();
            this.txtModule = new DevExpress.XtraEditors.TextEdit();
            this.gcSupplier = new DevExpress.XtraGrid.GridControl();
            this.CMSOrder = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDel = new System.Windows.Forms.ToolStripMenuItem();
            this.gvSupplier = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.Module = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Type = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Supplier = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Contact = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Tel = new DevExpress.XtraGrid.Columns.GridColumn();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem7 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlGroup2 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtTel.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtContact.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSupplier.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtModule.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcSupplier)).BeginInit();
            this.CMSOrder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvSupplier)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomization = false;
            this.layoutControl1.Controls.Add(this.btnSubmit);
            this.layoutControl1.Controls.Add(this.txtTel);
            this.layoutControl1.Controls.Add(this.txtContact);
            this.layoutControl1.Controls.Add(this.txtSupplier);
            this.layoutControl1.Controls.Add(this.txtType);
            this.layoutControl1.Controls.Add(this.txtModule);
            this.layoutControl1.Controls.Add(this.gcSupplier);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Margin = new System.Windows.Forms.Padding(2);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(1022, 550);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // btnSubmit
            // 
            this.btnSubmit.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSubmit.Appearance.Options.UseFont = true;
            this.btnSubmit.Location = new System.Drawing.Point(12, 516);
            this.btnSubmit.Margin = new System.Windows.Forms.Padding(2);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(998, 22);
            this.btnSubmit.StyleController = this.layoutControl1;
            this.btnSubmit.TabIndex = 17;
            this.btnSubmit.Text = "提交";
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // txtTel
            // 
            this.txtTel.Location = new System.Drawing.Point(89, 476);
            this.txtTel.Margin = new System.Windows.Forms.Padding(2);
            this.txtTel.Name = "txtTel";
            this.txtTel.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTel.Properties.Appearance.Options.UseFont = true;
            this.txtTel.Size = new System.Drawing.Size(909, 24);
            this.txtTel.StyleController = this.layoutControl1;
            this.txtTel.TabIndex = 16;
            this.txtTel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtTel_MouseUp);
            // 
            // txtContact
            // 
            this.txtContact.Location = new System.Drawing.Point(89, 448);
            this.txtContact.Margin = new System.Windows.Forms.Padding(2);
            this.txtContact.Name = "txtContact";
            this.txtContact.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtContact.Properties.Appearance.Options.UseFont = true;
            this.txtContact.Size = new System.Drawing.Size(909, 24);
            this.txtContact.StyleController = this.layoutControl1;
            this.txtContact.TabIndex = 15;
            this.txtContact.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtContact_MouseUp);
            // 
            // txtSupplier
            // 
            this.txtSupplier.Location = new System.Drawing.Point(89, 420);
            this.txtSupplier.Margin = new System.Windows.Forms.Padding(2);
            this.txtSupplier.Name = "txtSupplier";
            this.txtSupplier.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSupplier.Properties.Appearance.Options.UseFont = true;
            this.txtSupplier.Size = new System.Drawing.Size(909, 24);
            this.txtSupplier.StyleController = this.layoutControl1;
            this.txtSupplier.TabIndex = 14;
            this.txtSupplier.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtSupplier_MouseUp);
            // 
            // txtType
            // 
            this.txtType.Location = new System.Drawing.Point(89, 392);
            this.txtType.Margin = new System.Windows.Forms.Padding(2);
            this.txtType.Name = "txtType";
            this.txtType.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtType.Properties.Appearance.Options.UseFont = true;
            this.txtType.Size = new System.Drawing.Size(909, 24);
            this.txtType.StyleController = this.layoutControl1;
            this.txtType.TabIndex = 13;
            this.txtType.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtType_MouseUp);
            // 
            // txtModule
            // 
            this.txtModule.Location = new System.Drawing.Point(89, 364);
            this.txtModule.Margin = new System.Windows.Forms.Padding(2);
            this.txtModule.Name = "txtModule";
            this.txtModule.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtModule.Properties.Appearance.Options.UseFont = true;
            this.txtModule.Size = new System.Drawing.Size(909, 24);
            this.txtModule.StyleController = this.layoutControl1;
            this.txtModule.TabIndex = 12;
            this.txtModule.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtModule_MouseUp);
            // 
            // gcSupplier
            // 
            this.gcSupplier.ContextMenuStrip = this.CMSOrder;
            this.gcSupplier.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(1);
            this.gcSupplier.Location = new System.Drawing.Point(12, 12);
            this.gcSupplier.MainView = this.gvSupplier;
            this.gcSupplier.Margin = new System.Windows.Forms.Padding(2);
            this.gcSupplier.Name = "gcSupplier";
            this.gcSupplier.Size = new System.Drawing.Size(998, 317);
            this.gcSupplier.TabIndex = 11;
            this.gcSupplier.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvSupplier});
            this.gcSupplier.DoubleClick += new System.EventHandler(this.gcSupplier_DoubleClick);
            this.gcSupplier.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gcSupplier_MouseDown);
            // 
            // CMSOrder
            // 
            this.CMSOrder.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.CMSOrder.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAdd,
            this.tsmiUpdate,
            this.tsmiDel});
            this.CMSOrder.Name = "CMSOrder";
            this.CMSOrder.Size = new System.Drawing.Size(101, 70);
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
            // gvSupplier
            // 
            this.gvSupplier.Appearance.FocusedRow.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.gvSupplier.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gvSupplier.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.Module,
            this.Type,
            this.Supplier,
            this.Contact,
            this.Tel});
            this.gvSupplier.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFullFocus;
            this.gvSupplier.GridControl = this.gcSupplier;
            this.gvSupplier.Name = "gvSupplier";
            this.gvSupplier.OptionsBehavior.Editable = false;
            this.gvSupplier.OptionsBehavior.ReadOnly = true;
            this.gvSupplier.OptionsCustomization.AllowColumnMoving = false;
            this.gvSupplier.OptionsFind.AlwaysVisible = true;
            this.gvSupplier.OptionsMenu.EnableColumnMenu = false;
            this.gvSupplier.OptionsMenu.EnableFooterMenu = false;
            this.gvSupplier.OptionsMenu.EnableGroupPanelMenu = false;
            this.gvSupplier.OptionsView.ShowGroupPanel = false;
            this.gvSupplier.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gvSupplier_MouseDown);
            // 
            // Module
            // 
            this.Module.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Module.AppearanceCell.Options.UseFont = true;
            this.Module.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Module.AppearanceHeader.Options.UseFont = true;
            this.Module.Caption = "模块";
            this.Module.FieldName = "Module";
            this.Module.Name = "Module";
            this.Module.Visible = true;
            this.Module.VisibleIndex = 0;
            // 
            // Type
            // 
            this.Type.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Type.AppearanceCell.Options.UseFont = true;
            this.Type.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Type.AppearanceHeader.Options.UseFont = true;
            this.Type.Caption = "编号";
            this.Type.FieldName = "Type";
            this.Type.Name = "Type";
            this.Type.Visible = true;
            this.Type.VisibleIndex = 1;
            // 
            // Supplier
            // 
            this.Supplier.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Supplier.AppearanceCell.Options.UseFont = true;
            this.Supplier.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Supplier.AppearanceHeader.Options.UseFont = true;
            this.Supplier.Caption = "供应商";
            this.Supplier.FieldName = "Supplier";
            this.Supplier.Name = "Supplier";
            this.Supplier.Visible = true;
            this.Supplier.VisibleIndex = 2;
            // 
            // Contact
            // 
            this.Contact.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Contact.AppearanceCell.Options.UseFont = true;
            this.Contact.AppearanceHeader.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Contact.AppearanceHeader.Options.UseFont = true;
            this.Contact.Caption = "联系人";
            this.Contact.FieldName = "Contact";
            this.Contact.Name = "Contact";
            this.Contact.Visible = true;
            this.Contact.VisibleIndex = 3;
            // 
            // Tel
            // 
            this.Tel.AppearanceCell.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Tel.AppearanceCell.Options.UseFont = true;
            this.Tel.Caption = "联系方式";
            this.Tel.FieldName = "Tel";
            this.Tel.Name = "Tel";
            this.Tel.Visible = true;
            this.Tel.VisibleIndex = 4;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem7,
            this.layoutControlItem1,
            this.layoutControlGroup2});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "layoutControlGroup1";
            this.layoutControlGroup1.OptionsItemText.TextToControlDistance = 5;
            this.layoutControlGroup1.Size = new System.Drawing.Size(1022, 550);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem7
            // 
            this.layoutControlItem7.Control = this.gcSupplier;
            this.layoutControlItem7.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem7.Name = "layoutControlItem7";
            this.layoutControlItem7.Size = new System.Drawing.Size(1002, 321);
            this.layoutControlItem7.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem7.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.btnSubmit;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 504);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(1002, 26);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // layoutControlGroup2
            // 
            this.layoutControlGroup2.AppearanceGroup.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlGroup2.AppearanceGroup.Options.UseFont = true;
            this.layoutControlGroup2.AppearanceItemCaption.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControlGroup2.AppearanceItemCaption.Options.UseFont = true;
            this.layoutControlGroup2.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem2,
            this.layoutControlItem6,
            this.layoutControlItem5,
            this.layoutControlItem4,
            this.layoutControlItem3});
            this.layoutControlGroup2.Location = new System.Drawing.Point(0, 321);
            this.layoutControlGroup2.Name = "layoutControlGroup2";
            this.layoutControlGroup2.OptionsItemText.TextToControlDistance = 5;
            this.layoutControlGroup2.Size = new System.Drawing.Size(1002, 183);
            this.layoutControlGroup2.Text = "供应商管理";
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.txtTel;
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 112);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(978, 28);
            this.layoutControlItem2.Text = "联系方式：";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(60, 17);
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.Control = this.txtModule;
            this.layoutControlItem6.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Size = new System.Drawing.Size(978, 28);
            this.layoutControlItem6.Text = "模块：";
            this.layoutControlItem6.TextSize = new System.Drawing.Size(60, 17);
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.Control = this.txtType;
            this.layoutControlItem5.Location = new System.Drawing.Point(0, 28);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Size = new System.Drawing.Size(978, 28);
            this.layoutControlItem5.Text = "编号：";
            this.layoutControlItem5.TextSize = new System.Drawing.Size(60, 17);
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.txtSupplier;
            this.layoutControlItem4.Location = new System.Drawing.Point(0, 56);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Size = new System.Drawing.Size(978, 28);
            this.layoutControlItem4.Text = "供应商：";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(60, 17);
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.txtContact;
            this.layoutControlItem3.Location = new System.Drawing.Point(0, 84);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Size = new System.Drawing.Size(978, 28);
            this.layoutControlItem3.Text = "联系人：";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(60, 17);
            // 
            // Suppier
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.layoutControl1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Suppier";
            this.Size = new System.Drawing.Size(1022, 550);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtTel.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtContact.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSupplier.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtModule.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcSupplier)).EndInit();
            this.CMSOrder.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gvSupplier)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraEditors.SimpleButton btnSubmit;
        private DevExpress.XtraEditors.TextEdit txtTel;
        private DevExpress.XtraEditors.TextEdit txtContact;
        private DevExpress.XtraEditors.TextEdit txtSupplier;
        private DevExpress.XtraEditors.TextEdit txtType;
        private DevExpress.XtraEditors.TextEdit txtModule;
        private DevExpress.XtraGrid.GridControl gcSupplier;
        private DevExpress.XtraGrid.Views.Grid.GridView gvSupplier;
        private DevExpress.XtraGrid.Columns.GridColumn Module;
        private DevExpress.XtraGrid.Columns.GridColumn Type;
        private DevExpress.XtraGrid.Columns.GridColumn Supplier;
        private DevExpress.XtraGrid.Columns.GridColumn Contact;
        private DevExpress.XtraGrid.Columns.GridColumn Tel;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem7;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup2;
        private System.Windows.Forms.ContextMenuStrip CMSOrder;
        private System.Windows.Forms.ToolStripMenuItem tsmiAdd;
        private System.Windows.Forms.ToolStripMenuItem tsmiUpdate;
        private System.Windows.Forms.ToolStripMenuItem tsmiDel;
    }
}
