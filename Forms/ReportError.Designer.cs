// Disable warnings in the generated code
#pragma warning disable IDE0001 // Simplify Names
#pragma warning disable IDE0002 // Simplify Member Access
#pragma warning disable IDE0004 // Remove Unnecessary Cast

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportError));
            this.ImageIcon = new System.Windows.Forms.PictureBox();
            this.ButtonSend = new System.Windows.Forms.Button();
            this.ButtonDontSend = new System.Windows.Forms.Button();
            this.LinkWhatData = new ExtLinkLabel();
            LabelTopMessage = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.ImageIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // LabelTopMessage
            // 
            LabelTopMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            LabelTopMessage.Location = new System.Drawing.Point(53, 12);
            LabelTopMessage.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            LabelTopMessage.Name = "LabelTopMessage";
            LabelTopMessage.Size = new System.Drawing.Size(412, 92);
            LabelTopMessage.TabIndex = 3;
            LabelTopMessage.Text = resources.GetString("LabelTopMessage.Text");
            // 
            // ImageIcon
            // 
            this.ImageIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ImageIcon.Location = new System.Drawing.Point(12, 12);
            this.ImageIcon.Name = "ImageIcon";
            this.ImageIcon.Size = new System.Drawing.Size(32, 32);
            this.ImageIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.ImageIcon.TabIndex = 5;
            this.ImageIcon.TabStop = false;
            // 
            // ButtonSend
            // 
            this.ButtonSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonSend.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonSend.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonSend.Location = new System.Drawing.Point(267, 134);
            this.ButtonSend.Name = "ButtonSend";
            this.ButtonSend.Size = new System.Drawing.Size(123, 23);
            this.ButtonSend.TabIndex = 0;
            this.ButtonSend.Text = "Send Error Report";
            this.ButtonSend.Click += new System.EventHandler(this.ButtonSend_Click);
            // 
            // ButtonDontSend
            // 
            this.ButtonDontSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonDontSend.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonDontSend.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonDontSend.Location = new System.Drawing.Point(396, 134);
            this.ButtonDontSend.Name = "ButtonDontSend";
            this.ButtonDontSend.Size = new System.Drawing.Size(69, 23);
            this.ButtonDontSend.TabIndex = 1;
            this.ButtonDontSend.Text = "Close";
            // 
            // LinkWhatData
            // 
            this.LinkWhatData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LinkWhatData.AutoSize = true;
            this.LinkWhatData.Location = new System.Drawing.Point(53, 139);
            this.LinkWhatData.Name = "LinkWhatData";
            this.LinkWhatData.Size = new System.Drawing.Size(84, 13);
            this.LinkWhatData.TabIndex = 10;
            this.LinkWhatData.TabStop = true;
            this.LinkWhatData.Text = "&View report data";
            this.LinkWhatData.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkWhatData_LinkClicked);
            // 
            // ReportError
            // 
            this.AcceptButton = this.ButtonSend;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.ButtonDontSend;
            this.ClientSize = new System.Drawing.Size(477, 169);
            this.Controls.Add(this.LinkWhatData);
            this.Controls.Add(this.ButtonDontSend);
            this.Controls.Add(this.ButtonSend);
            this.Controls.Add(this.ImageIcon);
            this.Controls.Add(LabelTopMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::RadioDld.Properties.Resources.icon_main;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ReportError";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Radio Downloader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Error_FormClosing);
            this.Load += new System.EventHandler(this.ReportError_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ImageIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.Button ButtonSend;
        private System.Windows.Forms.Button ButtonDontSend;
        private ExtLinkLabel LinkWhatData;
        private System.Windows.Forms.PictureBox ImageIcon;

        #endregion
    }
}
