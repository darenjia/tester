namespace UltraANetT.Form
{
    partial class Loading
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
            this.mpbProgress = new DevExpress.XtraEditors.MarqueeProgressBarControl();
            this.lblCopyright = new DevExpress.XtraEditors.LabelControl();
            this.lblStart = new DevExpress.XtraEditors.LabelControl();
            this.DLAF = new DevExpress.LookAndFeel.DefaultLookAndFeel(this.components);
            this.pictShow = new DevExpress.XtraEditors.PictureEdit();
            ((System.ComponentModel.ISupportInitialize)(this.mpbProgress.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictShow.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // mpbProgress
            // 
            this.mpbProgress.EditValue = 0;
            this.mpbProgress.Location = new System.Drawing.Point(23, 213);
            this.mpbProgress.Name = "mpbProgress";
            this.mpbProgress.Size = new System.Drawing.Size(404, 11);
            this.mpbProgress.TabIndex = 5;
            // 
            // lblCopyright
            // 
            this.lblCopyright.Appearance.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCopyright.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.lblCopyright.Location = new System.Drawing.Point(23, 237);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(118, 17);
            this.lblCopyright.TabIndex = 6;
            this.lblCopyright.Text = "Copyright © 2015 - ";
            // 
            // lblStart
            // 
            this.lblStart.Location = new System.Drawing.Point(23, 191);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(55, 14);
            this.lblStart.TabIndex = 7;
            this.lblStart.Text = "Starting...";
            // 
            // DLAF
            // 
            this.DLAF.LookAndFeel.SkinName = "Office 2016 Colorful";
            // 
            // pictShow
            // 
            this.pictShow.EditValue = global::UltraANetT.Properties.Resources.Loading;
            this.pictShow.Location = new System.Drawing.Point(12, 11);
            this.pictShow.Name = "pictShow";
            this.pictShow.Properties.AllowFocused = false;
            this.pictShow.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.pictShow.Properties.Appearance.Options.UseBackColor = true;
            this.pictShow.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pictShow.Properties.ShowMenu = false;
            this.pictShow.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            this.pictShow.Size = new System.Drawing.Size(426, 167);
            this.pictShow.TabIndex = 9;
            // 
            // Loading
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 276);
            this.Controls.Add(this.pictShow);
            this.Controls.Add(this.lblStart);
            this.Controls.Add(this.lblCopyright);
            this.Controls.Add(this.mpbProgress);
            this.Name = "Loading";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.mpbProgress.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictShow.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.MarqueeProgressBarControl mpbProgress;
        private DevExpress.XtraEditors.LabelControl lblCopyright;
        private DevExpress.XtraEditors.LabelControl lblStart;
        private DevExpress.XtraEditors.PictureEdit pictShow;
        private DevExpress.LookAndFeel.DefaultLookAndFeel DLAF;
    }
}
