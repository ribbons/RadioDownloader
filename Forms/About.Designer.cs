namespace RadioDld
{
    internal partial class About
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
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
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
            System.Windows.Forms.TextBox TextLicense;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            System.Windows.Forms.Label LabelLicense;
            System.Windows.Forms.TableLayoutPanel LayoutMain;
            System.Windows.Forms.PictureBox ImageLogo;
            System.Windows.Forms.TableLayoutPanel LayoutTop;
            this.LabelNameAndVer = new System.Windows.Forms.Label();
            this.LinkHomepage = new RadioDld.ExtLinkLabel();
            this.LabelCopyright = new System.Windows.Forms.Label();
            this.LinkUpdate = new RadioDld.ExtLinkLabel();
            this.ButtonOk = new System.Windows.Forms.Button();
            TextLicense = new System.Windows.Forms.TextBox();
            LabelLicense = new System.Windows.Forms.Label();
            LayoutMain = new System.Windows.Forms.TableLayoutPanel();
            ImageLogo = new System.Windows.Forms.PictureBox();
            LayoutTop = new System.Windows.Forms.TableLayoutPanel();
            LayoutMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(ImageLogo)).BeginInit();
            LayoutTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // TextLicense
            // 
            LayoutMain.SetColumnSpan(TextLicense, 2);
            TextLicense.Dock = System.Windows.Forms.DockStyle.Fill;
            TextLicense.Location = new System.Drawing.Point(11, 104);
            TextLicense.Margin = new System.Windows.Forms.Padding(11, 0, 11, 0);
            TextLicense.Multiline = true;
            TextLicense.Name = "TextLicense";
            TextLicense.ReadOnly = true;
            TextLicense.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            TextLicense.Size = new System.Drawing.Size(491, 141);
            TextLicense.TabIndex = 5;
            TextLicense.Text = resources.GetString("TextLicense.Text");
            // 
            // LabelLicense
            // 
            LabelLicense.AutoSize = true;
            LayoutMain.SetColumnSpan(LabelLicense, 2);
            LabelLicense.Location = new System.Drawing.Point(11, 86);
            LabelLicense.Margin = new System.Windows.Forms.Padding(11, 11, 0, 3);
            LabelLicense.Name = "LabelLicense";
            LabelLicense.Size = new System.Drawing.Size(49, 15);
            LabelLicense.TabIndex = 4;
            LabelLicense.Text = "&License:";
            // 
            // LayoutMain
            // 
            LayoutMain.ColumnCount = 2;
            LayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            LayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            LayoutMain.Controls.Add(ImageLogo, 0, 0);
            LayoutMain.Controls.Add(LayoutTop, 1, 0);
            LayoutMain.Controls.Add(this.ButtonOk, 0, 3);
            LayoutMain.Controls.Add(LabelLicense, 0, 1);
            LayoutMain.Controls.Add(TextLicense, 0, 2);
            LayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            LayoutMain.Location = new System.Drawing.Point(0, 0);
            LayoutMain.Name = "LayoutMain";
            LayoutMain.RowCount = 4;
            LayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            LayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            LayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            LayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            LayoutMain.Size = new System.Drawing.Size(513, 290);
            LayoutMain.TabIndex = 6;
            // 
            // ImageLogo
            // 
            ImageLogo.Image = global::RadioDld.Properties.Resources.icon_main_img64;
            ImageLogo.Location = new System.Drawing.Point(11, 11);
            ImageLogo.Margin = new System.Windows.Forms.Padding(11, 11, 0, 0);
            ImageLogo.Name = "ImageLogo";
            ImageLogo.Size = new System.Drawing.Size(64, 64);
            ImageLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            ImageLogo.TabIndex = 8;
            ImageLogo.TabStop = false;
            // 
            // LayoutTop
            // 
            LayoutTop.ColumnCount = 2;
            LayoutTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            LayoutTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            LayoutTop.Controls.Add(this.LabelNameAndVer, 0, 1);
            LayoutTop.Controls.Add(this.LinkHomepage, 0, 3);
            LayoutTop.Controls.Add(this.LabelCopyright, 0, 2);
            LayoutTop.Controls.Add(this.LinkUpdate, 1, 1);
            LayoutTop.Dock = System.Windows.Forms.DockStyle.Fill;
            LayoutTop.Location = new System.Drawing.Point(75, 11);
            LayoutTop.Margin = new System.Windows.Forms.Padding(0, 11, 0, 0);
            LayoutTop.Name = "LayoutTop";
            LayoutTop.RowCount = 5;
            LayoutTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            LayoutTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            LayoutTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            LayoutTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            LayoutTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            LayoutTop.Size = new System.Drawing.Size(438, 64);
            LayoutTop.TabIndex = 7;
            // 
            // LabelNameAndVer
            // 
            this.LabelNameAndVer.AutoSize = true;
            this.LabelNameAndVer.Location = new System.Drawing.Point(3, 6);
            this.LabelNameAndVer.Margin = new System.Windows.Forms.Padding(3, 0, 3, 2);
            this.LabelNameAndVer.Name = "LabelNameAndVer";
            this.LabelNameAndVer.Size = new System.Drawing.Size(136, 15);
            this.LabelNameAndVer.TabIndex = 1;
            this.LabelNameAndVer.Text = "Radio Downloader ?.?.?.?";
            // 
            // LinkHomepage
            // 
            this.LinkHomepage.AutoSize = true;
            LayoutTop.SetColumnSpan(this.LinkHomepage, 2);
            this.LinkHomepage.Location = new System.Drawing.Point(3, 43);
            this.LinkHomepage.Margin = new System.Windows.Forms.Padding(3, 1, 3, 0);
            this.LinkHomepage.Name = "LinkHomepage";
            this.LinkHomepage.Size = new System.Drawing.Size(260, 15);
            this.LinkHomepage.TabIndex = 3;
            this.LinkHomepage.TabStop = true;
            this.LinkHomepage.Text = "https://nerdoftheherd.com/tools/radiodld/";
            this.LinkHomepage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkHomepage_LinkClicked);
            // 
            // LabelCopyright
            // 
            this.LabelCopyright.AutoSize = true;
            LayoutTop.SetColumnSpan(this.LabelCopyright, 2);
            this.LabelCopyright.Location = new System.Drawing.Point(3, 25);
            this.LabelCopyright.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.LabelCopyright.Name = "LabelCopyright";
            this.LabelCopyright.Size = new System.Drawing.Size(99, 15);
            this.LabelCopyright.TabIndex = 2;
            this.LabelCopyright.Text = "Copyright © 20??";
            // 
            // LinkUpdate
            // 
            this.LinkUpdate.AutoSize = true;
            this.LinkUpdate.Location = new System.Drawing.Point(142, 6);
            this.LinkUpdate.Margin = new System.Windows.Forms.Padding(0, 0, 3, 2);
            this.LinkUpdate.Name = "LinkUpdate";
            this.LinkUpdate.Size = new System.Drawing.Size(94, 15);
            this.LinkUpdate.TabIndex = 4;
            this.LinkUpdate.TabStop = true;
            this.LinkUpdate.Text = "Update available";
            this.LinkUpdate.Visible = false;
            this.LinkUpdate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkUpdate_LinkClicked);
            // 
            // ButtonOk
            // 
            this.ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            LayoutMain.SetColumnSpan(this.ButtonOk, 2);
            this.ButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOk.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonOk.Location = new System.Drawing.Point(427, 256);
            this.ButtonOk.Margin = new System.Windows.Forms.Padding(0, 11, 11, 11);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(75, 23);
            this.ButtonOk.TabIndex = 0;
            this.ButtonOk.Text = "OK";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // About
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(513, 290);
            this.Controls.Add(LayoutMain);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::RadioDld.Properties.Resources.icon_main;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "About";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            this.Load += new System.EventHandler(this.About_Load);
            LayoutMain.ResumeLayout(false);
            LayoutMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(ImageLogo)).EndInit();
            LayoutTop.ResumeLayout(false);
            LayoutTop.PerformLayout();
            this.ResumeLayout(false);

        }
        private System.Windows.Forms.Label LabelNameAndVer;
        private System.Windows.Forms.Label LabelCopyright;
        private ExtLinkLabel LinkHomepage;
        private System.Windows.Forms.Button ButtonOk;
        private ExtLinkLabel LinkUpdate;

        #endregion
    }
}
