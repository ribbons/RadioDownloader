using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
namespace RadioDld
{
	[Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
	partial class Status : System.Windows.Forms.Form
	{

		//Form overrides dispose to clean up the component list.
		[System.Diagnostics.DebuggerNonUserCode()]
		protected override void Dispose(bool disposing)
		{
			try {
				if (disposing && components != null) {
					components.Dispose();
				}
			} finally {
				base.Dispose(disposing);
			}
		}

		//Required by the Windows Form Designer

		private System.ComponentModel.IContainer components;
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.  
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]
		private void InitializeComponent()
		{
			this.lblStatus = new System.Windows.Forms.Label();
			this.prgProgress = new System.Windows.Forms.ProgressBar();
			this.SuspendLayout();
			//
			//lblStatus
			//
			this.lblStatus.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.lblStatus.Location = new System.Drawing.Point(12, 9);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(414, 86);
			this.lblStatus.TabIndex = 0;
			this.lblStatus.Text = "Please wait";
			this.lblStatus.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			//
			//prgProgress
			//
			this.prgProgress.Location = new System.Drawing.Point(15, 110);
			this.prgProgress.Name = "prgProgress";
			this.prgProgress.Size = new System.Drawing.Size(411, 23);
			this.prgProgress.TabIndex = 1;
			//
			//Status
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(438, 250);
			this.ControlBox = false;
			this.Controls.Add(this.prgProgress);
			this.Controls.Add(this.lblStatus);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = Properties.Resources.icon_main;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Status";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Radio Downloader";
			this.ResumeLayout(false);

		}
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.ProgressBar prgProgress;
		public Status()
		{
			Shown += Status_Shown;
			Load += Status_Load;
			FormClosing += Status_FormClosing;
			InitializeComponent();
		}
	}
}
