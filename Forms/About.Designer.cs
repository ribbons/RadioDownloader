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
            System.Windows.Forms.PictureBox ImageLogo;
            System.Windows.Forms.TextBox TextLicense;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            System.Windows.Forms.Label LabelLicense;
            this.LabelNameAndVer = new System.Windows.Forms.Label();
            this.LabelCopyright = new System.Windows.Forms.Label();
            this.LinkHomepage = new System.Windows.Forms.LinkLabel();
            this.ButtonOk = new System.Windows.Forms.Button();
            ImageLogo = new System.Windows.Forms.PictureBox();
            TextLicense = new System.Windows.Forms.TextBox();
            LabelLicense = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(ImageLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // ImageLogo
            // 
            ImageLogo.Image = global::RadioDld.Properties.Resources.icon_main_img64;
            ImageLogo.Location = new System.Drawing.Point(12, 12);
            ImageLogo.Name = "ImageLogo";
            ImageLogo.Size = new System.Drawing.Size(64, 64);
            ImageLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            ImageLogo.TabIndex = 1;
            ImageLogo.TabStop = false;
            // 
            // TextLicense
            // 
            TextLicense.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            TextLicense.Location = new System.Drawing.Point(12, 108);
            TextLicense.Multiline = true;
            TextLicense.Name = "TextLicense";
            TextLicense.ReadOnly = true;
            TextLicense.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            TextLicense.Size = new System.Drawing.Size(489, 139);
            TextLicense.TabIndex = 5;
            TextLicense.Text = resources.GetString("TextLicense.Text");
            // 
            // LabelLicense
            // 
            LabelLicense.AutoSize = true;
            LabelLicense.Location = new System.Drawing.Point(12, 92);
            LabelLicense.Name = "LabelLicense";
            LabelLicense.Size = new System.Drawing.Size(49, 15);
            LabelLicense.TabIndex = 4;
            LabelLicense.Text = "&License:";
            // 
            // LabelNameAndVer
            // 
            this.LabelNameAndVer.AutoSize = true;
            this.LabelNameAndVer.Location = new System.Drawing.Point(82, 16);
            this.LabelNameAndVer.Name = "LabelNameAndVer";
            this.LabelNameAndVer.Size = new System.Drawing.Size(136, 15);
            this.LabelNameAndVer.TabIndex = 1;
            this.LabelNameAndVer.Text = "Radio Downloader ?.?.?.?";
            // 
            // LabelCopyright
            // 
            this.LabelCopyright.AutoSize = true;
            this.LabelCopyright.Location = new System.Drawing.Point(82, 37);
            this.LabelCopyright.Name = "LabelCopyright";
            this.LabelCopyright.Size = new System.Drawing.Size(99, 15);
            this.LabelCopyright.TabIndex = 2;
            this.LabelCopyright.Text = "Copyright © 20??";
            // 
            // LinkHomepage
            // 
            this.LinkHomepage.AutoSize = true;
            this.LinkHomepage.Location = new System.Drawing.Point(82, 56);
            this.LinkHomepage.Name = "LinkHomepage";
            this.LinkHomepage.Size = new System.Drawing.Size(260, 15);
            this.LinkHomepage.TabIndex = 3;
            this.LinkHomepage.TabStop = true;
            this.LinkHomepage.Text = "http://www.nerdoftheherd.com/tools/radiodld/";
            this.LinkHomepage.Click += new System.EventHandler(this.LinkHomepage_Click);
            // 
            // ButtonOk
            // 
            this.ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOk.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonOk.Location = new System.Drawing.Point(426, 253);
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
            this.ClientSize = new System.Drawing.Size(513, 288);
            this.Controls.Add(this.ButtonOk);
            this.Controls.Add(LabelLicense);
            this.Controls.Add(this.LinkHomepage);
            this.Controls.Add(this.LabelCopyright);
            this.Controls.Add(TextLicense);
            this.Controls.Add(this.LabelNameAndVer);
            this.Controls.Add(ImageLogo);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::RadioDld.Properties.Resources.icon_main;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "About";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            this.Load += new System.EventHandler(this.About_Load);
            ((System.ComponentModel.ISupportInitialize)(ImageLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.Label LabelNameAndVer;
        private System.Windows.Forms.Label LabelCopyright;
        private System.Windows.Forms.LinkLabel LinkHomepage;
        private System.Windows.Forms.Button ButtonOk;

        #endregion
    }
}
