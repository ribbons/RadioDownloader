using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
namespace PodcastProvider
{
	[Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
	partial class FindNew : System.Windows.Forms.Form
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
			this.pnlFindNew = new System.Windows.Forms.Panel();
			this.lblResult = new System.Windows.Forms.Label();
			this.lblInstructions = new System.Windows.Forms.Label();
			this.cmdViewEps = new System.Windows.Forms.Button();
			this.txtFeedURL = new System.Windows.Forms.TextBox();
			this.pnlFindNew.SuspendLayout();
			this.SuspendLayout();
			//
			//pnlFindNew
			//
			this.pnlFindNew.Controls.Add(this.lblResult);
			this.pnlFindNew.Controls.Add(this.lblInstructions);
			this.pnlFindNew.Controls.Add(this.cmdViewEps);
			this.pnlFindNew.Controls.Add(this.txtFeedURL);
			this.pnlFindNew.Location = new System.Drawing.Point(0, 0);
			this.pnlFindNew.Name = "pnlFindNew";
			this.pnlFindNew.Size = new System.Drawing.Size(570, 416);
			this.pnlFindNew.TabIndex = 0;
			//
			//lblResult
			//
			this.lblResult.AutoSize = true;
			this.lblResult.Location = new System.Drawing.Point(36, 108);
			this.lblResult.Margin = new System.Windows.Forms.Padding(4, 20, 3, 0);
			this.lblResult.Name = "lblResult";
			this.lblResult.Size = new System.Drawing.Size(0, 13);
			this.lblResult.TabIndex = 3;
			//
			//lblInstructions
			//
			this.lblInstructions.AutoSize = true;
			this.lblInstructions.Location = new System.Drawing.Point(36, 39);
			this.lblInstructions.Margin = new System.Windows.Forms.Padding(4, 0, 3, 10);
			this.lblInstructions.Name = "lblInstructions";
			this.lblInstructions.Size = new System.Drawing.Size(189, 13);
			this.lblInstructions.TabIndex = 0;
			this.lblInstructions.Text = "&Enter the URL of a podcast RSS feed:";
			//
			//cmdViewEps
			//
			this.cmdViewEps.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.cmdViewEps.Location = new System.Drawing.Point(461, 63);
			this.cmdViewEps.Name = "cmdViewEps";
			this.cmdViewEps.Size = new System.Drawing.Size(73, 23);
			this.cmdViewEps.TabIndex = 2;
			this.cmdViewEps.Text = "&View";
			this.cmdViewEps.UseVisualStyleBackColor = true;
			//
			//txtFeedURL
			//
			this.txtFeedURL.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.txtFeedURL.Location = new System.Drawing.Point(39, 65);
			this.txtFeedURL.Name = "txtFeedURL";
			this.txtFeedURL.Size = new System.Drawing.Size(416, 20);
			this.txtFeedURL.TabIndex = 1;
			//
			//FindNew
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(570, 416);
			this.Controls.Add(this.pnlFindNew);
			this.Name = "FindNew";
			this.Text = "Find New";
			this.pnlFindNew.ResumeLayout(false);
			this.pnlFindNew.PerformLayout();
			this.ResumeLayout(false);

		}
		internal System.Windows.Forms.Panel pnlFindNew;
		private System.Windows.Forms.TextBox withEventsField_txtFeedURL;
		internal System.Windows.Forms.TextBox txtFeedURL {
			get { return withEventsField_txtFeedURL; }
			set {
				if (withEventsField_txtFeedURL != null) {
					withEventsField_txtFeedURL.KeyPress -= txtFeedURL_KeyPress;
				}
				withEventsField_txtFeedURL = value;
				if (withEventsField_txtFeedURL != null) {
					withEventsField_txtFeedURL.KeyPress += txtFeedURL_KeyPress;
				}
			}
		}
		private System.Windows.Forms.Button withEventsField_cmdViewEps;
		internal System.Windows.Forms.Button cmdViewEps {
			get { return withEventsField_cmdViewEps; }
			set {
				if (withEventsField_cmdViewEps != null) {
					withEventsField_cmdViewEps.Click -= cmdViewEps_Click;
				}
				withEventsField_cmdViewEps = value;
				if (withEventsField_cmdViewEps != null) {
					withEventsField_cmdViewEps.Click += cmdViewEps_Click;
				}
			}
		}
		internal System.Windows.Forms.Label lblInstructions;
		internal System.Windows.Forms.Label lblResult;
	}
}
