namespace RadioDld
{
    internal partial class ReportError
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
            System.Windows.Forms.Label LabelTopMessage;
            System.Windows.Forms.Label LabelTopMessage2;
            System.Windows.Forms.Label LabelIfYouWere;
            System.Windows.Forms.Label LabelPleaseTell;
            System.Windows.Forms.PictureBox ImageIcon;
            System.Windows.Forms.Label LabelWeHaveCreated;
            System.Windows.Forms.Label LabelWeWill;
            System.Windows.Forms.PictureBox ImageTopBar;
            System.Windows.Forms.PictureBox ImageTopBorder;
            this.ButtonSend = new System.Windows.Forms.Button();
            this.ButtonDontSend = new System.Windows.Forms.Button();
            this.LinkWhatData = new System.Windows.Forms.LinkLabel();
            LabelTopMessage = new System.Windows.Forms.Label();
            LabelTopMessage2 = new System.Windows.Forms.Label();
            LabelIfYouWere = new System.Windows.Forms.Label();
            LabelPleaseTell = new System.Windows.Forms.Label();
            ImageIcon = new System.Windows.Forms.PictureBox();
            LabelWeHaveCreated = new System.Windows.Forms.Label();
            LabelWeWill = new System.Windows.Forms.Label();
            ImageTopBar = new System.Windows.Forms.PictureBox();
            ImageTopBorder = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(ImageIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(ImageTopBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(ImageTopBorder)).BeginInit();
            this.SuspendLayout();
            // 
            // LabelTopMessage
            // 
            LabelTopMessage.AutoSize = true;
            LabelTopMessage.BackColor = System.Drawing.Color.White;
            LabelTopMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            LabelTopMessage.Location = new System.Drawing.Point(21, 15);
            LabelTopMessage.Name = "LabelTopMessage";
            LabelTopMessage.Size = new System.Drawing.Size(385, 13);
            LabelTopMessage.TabIndex = 3;
            LabelTopMessage.Text = "Radio Downloader has encountered a problem and needs to close.";
            // 
            // LabelTopMessage2
            // 
            LabelTopMessage2.AutoSize = true;
            LabelTopMessage2.BackColor = System.Drawing.Color.White;
            LabelTopMessage2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            LabelTopMessage2.Location = new System.Drawing.Point(21, 30);
            LabelTopMessage2.Name = "LabelTopMessage2";
            LabelTopMessage2.Size = new System.Drawing.Size(211, 13);
            LabelTopMessage2.TabIndex = 4;
            LabelTopMessage2.Text = "We are sorry for the inconvenience.";
            // 
            // LabelIfYouWere
            // 
            LabelIfYouWere.AutoSize = true;
            LabelIfYouWere.Location = new System.Drawing.Point(30, 69);
            LabelIfYouWere.Name = "LabelIfYouWere";
            LabelIfYouWere.Size = new System.Drawing.Size(425, 13);
            LabelIfYouWere.TabIndex = 6;
            LabelIfYouWere.Text = "If you were in the middle of something, the information you were working on might" +
                " be lost.";
            // 
            // LabelPleaseTell
            // 
            LabelPleaseTell.AutoSize = true;
            LabelPleaseTell.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            LabelPleaseTell.Location = new System.Drawing.Point(30, 91);
            LabelPleaseTell.Name = "LabelPleaseTell";
            LabelPleaseTell.Size = new System.Drawing.Size(292, 13);
            LabelPleaseTell.TabIndex = 7;
            LabelPleaseTell.Text = "Please tell NerdoftheHerd.com about this problem.";
            // 
            // ImageIcon
            // 
            ImageIcon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            ImageIcon.BackColor = System.Drawing.Color.White;
            ImageIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            ImageIcon.Image = global::RadioDld.Properties.Resources.icon_main_img32;
            ImageIcon.Location = new System.Drawing.Point(449, 13);
            ImageIcon.Name = "ImageIcon";
            ImageIcon.Size = new System.Drawing.Size(32, 32);
            ImageIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            ImageIcon.TabIndex = 5;
            ImageIcon.TabStop = false;
            // 
            // LabelWeHaveCreated
            // 
            LabelWeHaveCreated.AutoSize = true;
            LabelWeHaveCreated.Location = new System.Drawing.Point(30, 113);
            LabelWeHaveCreated.Name = "LabelWeHaveCreated";
            LabelWeHaveCreated.Size = new System.Drawing.Size(430, 13);
            LabelWeHaveCreated.TabIndex = 8;
            LabelWeHaveCreated.Text = "We have created an error report that you can send to help us improve Radio Downlo" +
                "ader.";
            // 
            // LabelWeWill
            // 
            LabelWeWill.AutoSize = true;
            LabelWeWill.Location = new System.Drawing.Point(30, 135);
            LabelWeWill.Name = "LabelWeWill";
            LabelWeWill.Size = new System.Drawing.Size(266, 13);
            LabelWeWill.TabIndex = 9;
            LabelWeWill.Text = "We will treat this report as confidential and anonymous.";
            // 
            // ButtonSend
            // 
            this.ButtonSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonSend.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonSend.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonSend.Location = new System.Drawing.Point(277, 188);
            this.ButtonSend.Margin = new System.Windows.Forms.Padding(4);
            this.ButtonSend.Name = "ButtonSend";
            this.ButtonSend.Size = new System.Drawing.Size(113, 26);
            this.ButtonSend.TabIndex = 0;
            this.ButtonSend.Text = "&Send Error Report";
            this.ButtonSend.Click += new System.EventHandler(this.ButtonSend_Click);
            // 
            // ButtonDontSend
            // 
            this.ButtonDontSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonDontSend.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonDontSend.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonDontSend.Location = new System.Drawing.Point(397, 188);
            this.ButtonDontSend.Name = "ButtonDontSend";
            this.ButtonDontSend.Size = new System.Drawing.Size(84, 26);
            this.ButtonDontSend.TabIndex = 1;
            this.ButtonDontSend.Text = "&Don\'t Send";
            this.ButtonDontSend.Click += new System.EventHandler(this.ButtonDontSend_Click);
            // 
            // ImageTopBar
            // 
            ImageTopBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            ImageTopBar.BackColor = System.Drawing.Color.White;
            ImageTopBar.Location = new System.Drawing.Point(0, -1);
            ImageTopBar.Name = "ImageTopBar";
            ImageTopBar.Size = new System.Drawing.Size(493, 58);
            ImageTopBar.TabIndex = 2;
            ImageTopBar.TabStop = false;
            // 
            // ImageTopBorder
            // 
            ImageTopBorder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            ImageTopBorder.BackColor = System.Drawing.Color.Silver;
            ImageTopBorder.ForeColor = System.Drawing.SystemColors.ControlText;
            ImageTopBorder.Location = new System.Drawing.Point(0, 56);
            ImageTopBorder.Name = "ImageTopBorder";
            ImageTopBorder.Size = new System.Drawing.Size(493, 1);
            ImageTopBorder.TabIndex = 5;
            ImageTopBorder.TabStop = false;
            // 
            // LinkWhatData
            // 
            this.LinkWhatData.AutoSize = true;
            this.LinkWhatData.Location = new System.Drawing.Point(30, 158);
            this.LinkWhatData.Name = "LinkWhatData";
            this.LinkWhatData.Size = new System.Drawing.Size(200, 13);
            this.LinkWhatData.TabIndex = 10;
            this.LinkWhatData.TabStop = true;
            this.LinkWhatData.Text = "&What data does this error report contain?";
            this.LinkWhatData.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkWhatData_LinkClicked);
            // 
            // ReportError
            // 
            this.AcceptButton = this.ButtonSend;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.ButtonDontSend;
            this.ClientSize = new System.Drawing.Size(493, 226);
            this.Controls.Add(this.LinkWhatData);
            this.Controls.Add(LabelWeWill);
            this.Controls.Add(LabelWeHaveCreated);
            this.Controls.Add(LabelPleaseTell);
            this.Controls.Add(LabelIfYouWere);
            this.Controls.Add(this.ButtonDontSend);
            this.Controls.Add(this.ButtonSend);
            this.Controls.Add(ImageIcon);
            this.Controls.Add(ImageTopBorder);
            this.Controls.Add(LabelTopMessage2);
            this.Controls.Add(LabelTopMessage);
            this.Controls.Add(ImageTopBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::RadioDld.Properties.Resources.icon_main;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ReportError";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Radio Downloader";
            ((System.ComponentModel.ISupportInitialize)(ImageIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(ImageTopBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(ImageTopBorder)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.Button ButtonSend;
        private System.Windows.Forms.Button ButtonDontSend;
        private System.Windows.Forms.LinkLabel LinkWhatData;

        #endregion
    }
}
