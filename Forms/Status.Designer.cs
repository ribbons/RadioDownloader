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
            System.Windows.Forms.Panel PanelBottom;
            System.Windows.Forms.Panel PanelLine;
            this.LabelStatus = new System.Windows.Forms.Label();
            this.Progress = new System.Windows.Forms.ProgressBar();
            this.ButtonStop = new System.Windows.Forms.Button();
            PanelBottom = new System.Windows.Forms.Panel();
            PanelLine = new System.Windows.Forms.Panel();
            PanelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // LabelStatus
            // 
            this.LabelStatus.AutoSize = true;
            this.LabelStatus.Location = new System.Drawing.Point(19, 19);
            this.LabelStatus.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
            this.LabelStatus.Name = "LabelStatus";
            this.LabelStatus.Size = new System.Drawing.Size(74, 15);
            this.LabelStatus.TabIndex = 0;
            this.LabelStatus.Text = "Please wait...";
            this.LabelStatus.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // Progress
            // 
            this.Progress.Location = new System.Drawing.Point(22, 49);
            this.Progress.Margin = new System.Windows.Forms.Padding(3, 15, 3, 3);
            this.Progress.Name = "Progress";
            this.Progress.Size = new System.Drawing.Size(355, 15);
            this.Progress.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.Progress.TabIndex = 1;
            // 
            // ButtonStop
            // 
            this.ButtonStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonStop.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonStop.Location = new System.Drawing.Point(302, 12);
            this.ButtonStop.Name = "ButtonStop";
            this.ButtonStop.Size = new System.Drawing.Size(75, 23);
            this.ButtonStop.TabIndex = 2;
            this.ButtonStop.Text = "Stop";
            this.ButtonStop.UseVisualStyleBackColor = true;
            this.ButtonStop.Visible = false;
            // 
            // PanelBottom
            // 
            PanelBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            PanelBottom.BackColor = System.Drawing.SystemColors.ButtonFace;
            PanelBottom.Controls.Add(this.ButtonStop);
            PanelBottom.Location = new System.Drawing.Point(0, 94);
            PanelBottom.Name = "PanelBottom";
            PanelBottom.Size = new System.Drawing.Size(396, 46);
            PanelBottom.TabIndex = 3;
            // 
            // PanelLine
            // 
            PanelLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            PanelLine.BackColor = System.Drawing.SystemColors.ControlLight;
            PanelLine.Location = new System.Drawing.Point(0, 93);
            PanelLine.Name = "PanelLine";
            PanelLine.Size = new System.Drawing.Size(396, 1);
            PanelLine.TabIndex = 4;
            // 
            // Status
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(396, 140);
            this.ControlBox = false;
            this.Controls.Add(PanelLine);
            this.Controls.Add(PanelBottom);
            this.Controls.Add(this.Progress);
            this.Controls.Add(this.LabelStatus);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::RadioDld.Properties.Resources.icon_main;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Status";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Radio Downloader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Status_FormClosing);
            this.Load += new System.EventHandler(this.Status_Load);
            this.Shown += new System.EventHandler(this.Status_Shown);
            PanelBottom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.Label LabelStatus;
        private System.Windows.Forms.ProgressBar Progress;
        private System.Windows.Forms.Button ButtonStop;

        #endregion
    }
}
