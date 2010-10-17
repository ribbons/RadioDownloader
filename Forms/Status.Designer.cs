namespace RadioDld
{
	partial class Status
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
            this.lblStatus = new System.Windows.Forms.Label();
            this.prgProgress = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Location = new System.Drawing.Point(12, 9);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(414, 86);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Please wait";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // prgProgress
            // 
            this.prgProgress.Location = new System.Drawing.Point(15, 110);
            this.prgProgress.Name = "prgProgress";
            this.prgProgress.Size = new System.Drawing.Size(411, 23);
            this.prgProgress.TabIndex = 1;
            // 
            // Status
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(438, 250);
            this.ControlBox = false;
            this.Controls.Add(this.prgProgress);
            this.Controls.Add(this.lblStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::RadioDld.Properties.Resources.icon_main;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Status";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Radio Downloader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Status_FormClosing);
            this.Load += new System.EventHandler(this.Status_Load);
            this.Shown += new System.EventHandler(this.Status_Shown);
            this.ResumeLayout(false);

		}
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.ProgressBar prgProgress;

        #endregion
    }
}
