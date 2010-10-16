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
	partial class About : System.Windows.Forms.Form
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
			this.LogoPictureBox = new System.Windows.Forms.PictureBox();
			this.LabelNameAndVer = new System.Windows.Forms.Label();
			this.TextboxLicense = new System.Windows.Forms.TextBox();
			this.LabelCopyright = new System.Windows.Forms.Label();
			this.HomepageLink = new System.Windows.Forms.LinkLabel();
			this.LabelLicense = new System.Windows.Forms.Label();
			this.ButtonOK = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)this.LogoPictureBox).BeginInit();
			this.SuspendLayout();
			//
			//LogoPictureBox
			//
			this.LogoPictureBox.Image = Properties.Resources.icon_main_img64;
			this.LogoPictureBox.Location = new System.Drawing.Point(12, 12);
			this.LogoPictureBox.Name = "LogoPictureBox";
			this.LogoPictureBox.Size = new System.Drawing.Size(64, 64);
			this.LogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.LogoPictureBox.TabIndex = 1;
			this.LogoPictureBox.TabStop = false;
			//
			//LabelNameAndVer
			//
			this.LabelNameAndVer.AutoSize = true;
			this.LabelNameAndVer.Location = new System.Drawing.Point(82, 16);
			this.LabelNameAndVer.Name = "LabelNameAndVer";
			this.LabelNameAndVer.Size = new System.Drawing.Size(131, 13);
			this.LabelNameAndVer.TabIndex = 1;
			this.LabelNameAndVer.Text = "Radio Downloader ?.?.?.?";
			//
			//TextboxLicense
			//
			this.TextboxLicense.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.TextboxLicense.Location = new System.Drawing.Point(12, 108);
			this.TextboxLicense.Multiline = true;
			this.TextboxLicense.Name = "TextboxLicense";
			this.TextboxLicense.ReadOnly = true;
			this.TextboxLicense.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.TextboxLicense.Size = new System.Drawing.Size(489, 140);
			this.TextboxLicense.TabIndex = 5;
			this.TextboxLicense.Text = resources.GetString("TextboxLicense.Text");
			//
			//LabelCopyright
			//
			this.LabelCopyright.AutoSize = true;
			this.LabelCopyright.Location = new System.Drawing.Point(82, 37);
			this.LabelCopyright.Name = "LabelCopyright";
			this.LabelCopyright.Size = new System.Drawing.Size(90, 13);
			this.LabelCopyright.TabIndex = 2;
			this.LabelCopyright.Text = "Copyright © 20??";
			//
			//HomepageLink
			//
			this.HomepageLink.AutoSize = true;
			this.HomepageLink.Location = new System.Drawing.Point(82, 56);
			this.HomepageLink.Name = "HomepageLink";
			this.HomepageLink.Size = new System.Drawing.Size(228, 13);
			this.HomepageLink.TabIndex = 3;
			this.HomepageLink.TabStop = true;
			this.HomepageLink.Text = "http://www.nerdoftheherd.com/tools/radiodld/";
			//
			//LabelLicense
			//
			this.LabelLicense.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.LabelLicense.AutoSize = true;
			this.LabelLicense.Location = new System.Drawing.Point(12, 92);
			this.LabelLicense.Name = "LabelLicense";
			this.LabelLicense.Size = new System.Drawing.Size(47, 13);
			this.LabelLicense.TabIndex = 4;
			this.LabelLicense.Text = "&License:";
			//
			//ButtonOK
			//
			this.ButtonOK.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ButtonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ButtonOK.Location = new System.Drawing.Point(419, 254);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(82, 26);
			this.ButtonOK.TabIndex = 0;
			this.ButtonOK.Text = "OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			//
			//About
			//
			this.AcceptButton = this.ButtonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(513, 292);
			this.Controls.Add(this.ButtonOK);
			this.Controls.Add(this.LabelLicense);
			this.Controls.Add(this.HomepageLink);
			this.Controls.Add(this.LabelCopyright);
			this.Controls.Add(this.TextboxLicense);
			this.Controls.Add(this.LabelNameAndVer);
			this.Controls.Add(this.LogoPictureBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = Properties.Resources.icon_main;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "About";
			this.Padding = new System.Windows.Forms.Padding(9);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "About";
			((System.ComponentModel.ISupportInitialize)this.LogoPictureBox).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		internal System.Windows.Forms.PictureBox LogoPictureBox;
		internal System.Windows.Forms.Label LabelNameAndVer;
		internal System.Windows.Forms.TextBox TextboxLicense;
		internal System.Windows.Forms.Label LabelCopyright;
		private System.Windows.Forms.LinkLabel withEventsField_HomepageLink;
		internal System.Windows.Forms.LinkLabel HomepageLink {
			get { return withEventsField_HomepageLink; }
			set {
				if (withEventsField_HomepageLink != null) {
					withEventsField_HomepageLink.Click -= HomepageLink_Click;
				}
				withEventsField_HomepageLink = value;
				if (withEventsField_HomepageLink != null) {
					withEventsField_HomepageLink.Click += HomepageLink_Click;
				}
			}
		}
		internal System.Windows.Forms.Label LabelLicense;
		private System.Windows.Forms.Button withEventsField_ButtonOK;
		internal System.Windows.Forms.Button ButtonOK {
			get { return withEventsField_ButtonOK; }
			set {
				if (withEventsField_ButtonOK != null) {
					withEventsField_ButtonOK.Click -= OKButton_Click;
				}
				withEventsField_ButtonOK = value;
				if (withEventsField_ButtonOK != null) {
					withEventsField_ButtonOK.Click += OKButton_Click;
				}
			}

		}
	}
}
