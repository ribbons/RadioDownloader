namespace RadioDld
{
    partial class ReportError
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
            this.cmdSend = new System.Windows.Forms.Button();
            this.cmdDontSend = new System.Windows.Forms.Button();
            this.lblWhiteBar = new System.Windows.Forms.Label();
            this.lblTopMessage = new System.Windows.Forms.Label();
            this.lblTopMessage2 = new System.Windows.Forms.Label();
            this.lblTopBorder = new System.Windows.Forms.Label();
            this.lblIfYouWere = new System.Windows.Forms.Label();
            this.lblPleaseTell = new System.Windows.Forms.Label();
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblWeHaveCreated = new System.Windows.Forms.Label();
            this.lblWeWill = new System.Windows.Forms.Label();
            this.lnkWhatData = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdSend
            // 
            this.cmdSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdSend.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdSend.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmdSend.Location = new System.Drawing.Point(277, 188);
            this.cmdSend.Margin = new System.Windows.Forms.Padding(4);
            this.cmdSend.Name = "cmdSend";
            this.cmdSend.Size = new System.Drawing.Size(113, 26);
            this.cmdSend.TabIndex = 0;
            this.cmdSend.Text = "&Send Error Report";
            this.cmdSend.Click += new System.EventHandler(this.cmdSend_Click);
            // 
            // cmdDontSend
            // 
            this.cmdDontSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdDontSend.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdDontSend.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmdDontSend.Location = new System.Drawing.Point(397, 188);
            this.cmdDontSend.Name = "cmdDontSend";
            this.cmdDontSend.Size = new System.Drawing.Size(84, 26);
            this.cmdDontSend.TabIndex = 1;
            this.cmdDontSend.Text = "&Don\'t Send";
            this.cmdDontSend.Click += new System.EventHandler(this.cmdDontSend_Click);
            // 
            // lblWhiteBar
            // 
            this.lblWhiteBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWhiteBar.BackColor = System.Drawing.Color.White;
            this.lblWhiteBar.Location = new System.Drawing.Point(0, -1);
            this.lblWhiteBar.Name = "lblWhiteBar";
            this.lblWhiteBar.Size = new System.Drawing.Size(493, 58);
            this.lblWhiteBar.TabIndex = 2;
            // 
            // lblTopMessage
            // 
            this.lblTopMessage.AutoSize = true;
            this.lblTopMessage.BackColor = System.Drawing.Color.White;
            this.lblTopMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTopMessage.Location = new System.Drawing.Point(21, 15);
            this.lblTopMessage.Name = "lblTopMessage";
            this.lblTopMessage.Size = new System.Drawing.Size(385, 13);
            this.lblTopMessage.TabIndex = 3;
            this.lblTopMessage.Text = "Radio Downloader has encountered a problem and needs to close.";
            // 
            // lblTopMessage2
            // 
            this.lblTopMessage2.AutoSize = true;
            this.lblTopMessage2.BackColor = System.Drawing.Color.White;
            this.lblTopMessage2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTopMessage2.Location = new System.Drawing.Point(21, 30);
            this.lblTopMessage2.Name = "lblTopMessage2";
            this.lblTopMessage2.Size = new System.Drawing.Size(211, 13);
            this.lblTopMessage2.TabIndex = 4;
            this.lblTopMessage2.Text = "We are sorry for the inconvenience.";
            // 
            // lblTopBorder
            // 
            this.lblTopBorder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTopBorder.BackColor = System.Drawing.Color.Silver;
            this.lblTopBorder.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblTopBorder.Location = new System.Drawing.Point(0, 56);
            this.lblTopBorder.Name = "lblTopBorder";
            this.lblTopBorder.Size = new System.Drawing.Size(493, 1);
            this.lblTopBorder.TabIndex = 5;
            // 
            // lblIfYouWere
            // 
            this.lblIfYouWere.AutoSize = true;
            this.lblIfYouWere.Location = new System.Drawing.Point(30, 69);
            this.lblIfYouWere.Name = "lblIfYouWere";
            this.lblIfYouWere.Size = new System.Drawing.Size(425, 13);
            this.lblIfYouWere.TabIndex = 6;
            this.lblIfYouWere.Text = "If you were in the middle of something, the information you were working on might" +
                " be lost.";
            // 
            // lblPleaseTell
            // 
            this.lblPleaseTell.AutoSize = true;
            this.lblPleaseTell.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPleaseTell.Location = new System.Drawing.Point(30, 91);
            this.lblPleaseTell.Name = "lblPleaseTell";
            this.lblPleaseTell.Size = new System.Drawing.Size(292, 13);
            this.lblPleaseTell.TabIndex = 7;
            this.lblPleaseTell.Text = "Please tell NerdoftheHerd.com about this problem.";
            // 
            // PictureBox1
            // 
            this.PictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PictureBox1.BackColor = System.Drawing.Color.White;
            this.PictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.PictureBox1.Image = global::RadioDld.Properties.Resources.icon_main_img32;
            this.PictureBox1.Location = new System.Drawing.Point(449, 13);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(32, 32);
            this.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PictureBox1.TabIndex = 5;
            this.PictureBox1.TabStop = false;
            // 
            // lblWeHaveCreated
            // 
            this.lblWeHaveCreated.AutoSize = true;
            this.lblWeHaveCreated.Location = new System.Drawing.Point(30, 113);
            this.lblWeHaveCreated.Name = "lblWeHaveCreated";
            this.lblWeHaveCreated.Size = new System.Drawing.Size(430, 13);
            this.lblWeHaveCreated.TabIndex = 8;
            this.lblWeHaveCreated.Text = "We have created an error report that you can send to help us improve Radio Downlo" +
                "ader.";
            // 
            // lblWeWill
            // 
            this.lblWeWill.AutoSize = true;
            this.lblWeWill.Location = new System.Drawing.Point(30, 135);
            this.lblWeWill.Name = "lblWeWill";
            this.lblWeWill.Size = new System.Drawing.Size(266, 13);
            this.lblWeWill.TabIndex = 9;
            this.lblWeWill.Text = "We will treat this report as confidential and anonymous.";
            // 
            // lnkWhatData
            // 
            this.lnkWhatData.AutoSize = true;
            this.lnkWhatData.Location = new System.Drawing.Point(30, 158);
            this.lnkWhatData.Name = "lnkWhatData";
            this.lnkWhatData.Size = new System.Drawing.Size(200, 13);
            this.lnkWhatData.TabIndex = 10;
            this.lnkWhatData.TabStop = true;
            this.lnkWhatData.Text = "&What data does this error report contain?";
            this.lnkWhatData.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkWhatData_LinkClicked);
            // 
            // ReportError
            // 
            this.AcceptButton = this.cmdSend;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cmdDontSend;
            this.ClientSize = new System.Drawing.Size(493, 226);
            this.Controls.Add(this.lnkWhatData);
            this.Controls.Add(this.lblWeWill);
            this.Controls.Add(this.lblWeHaveCreated);
            this.Controls.Add(this.lblPleaseTell);
            this.Controls.Add(this.lblIfYouWere);
            this.Controls.Add(this.cmdDontSend);
            this.Controls.Add(this.cmdSend);
            this.Controls.Add(this.PictureBox1);
            this.Controls.Add(this.lblTopBorder);
            this.Controls.Add(this.lblTopMessage2);
            this.Controls.Add(this.lblTopMessage);
            this.Controls.Add(this.lblWhiteBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::RadioDld.Properties.Resources.icon_main;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ReportError";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Radio Downloader";
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        internal System.Windows.Forms.Button cmdSend;
        internal System.Windows.Forms.Button cmdDontSend;
        internal System.Windows.Forms.Label lblWhiteBar;
        internal System.Windows.Forms.Label lblTopMessage;
        internal System.Windows.Forms.Label lblTopMessage2;
        internal System.Windows.Forms.Label lblTopBorder;
        internal System.Windows.Forms.PictureBox PictureBox1;
        internal System.Windows.Forms.Label lblIfYouWere;
        internal System.Windows.Forms.Label lblPleaseTell;
        internal System.Windows.Forms.Label lblWeHaveCreated;
        internal System.Windows.Forms.Label lblWeWill;
        internal System.Windows.Forms.LinkLabel lnkWhatData;

        #endregion
    }
}
