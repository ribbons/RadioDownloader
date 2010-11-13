namespace RadioDld
{
    internal partial class Status
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
            this.LabelStatus = new System.Windows.Forms.Label();
            this.Progress = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // LabelStatus
            // 
            this.LabelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelStatus.Location = new System.Drawing.Point(12, 9);
            this.LabelStatus.Name = "LabelStatus";
            this.LabelStatus.Size = new System.Drawing.Size(414, 86);
            this.LabelStatus.TabIndex = 0;
            this.LabelStatus.Text = "Please wait";
            this.LabelStatus.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // Progress
            // 
            this.Progress.Location = new System.Drawing.Point(15, 110);
            this.Progress.Name = "Progress";
            this.Progress.Size = new System.Drawing.Size(411, 23);
            this.Progress.TabIndex = 1;
            // 
            // Status
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(438, 250);
            this.ControlBox = false;
            this.Controls.Add(this.Progress);
            this.Controls.Add(this.LabelStatus);
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
        private System.Windows.Forms.Label LabelStatus;
        private System.Windows.Forms.ProgressBar Progress;

        #endregion
    }
}
