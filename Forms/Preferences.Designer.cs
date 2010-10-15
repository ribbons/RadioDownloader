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
	partial class Preferences
	{
		#region "Windows Form Designer generated code "
		[System.Diagnostics.DebuggerNonUserCode()]
		public Preferences() : base()
		{
			Load += Preferences_Load;
			FormClosing += Preferences_FormClosing;
			//This call is required by the Windows Form Designer.
			InitializeComponent();
		}
//Form overrides dispose to clean up the component list.
		[System.Diagnostics.DebuggerNonUserCode()]
		protected override void Dispose(bool Disposing)
		{
			if (Disposing) {
				if ((components != null)) {
					components.Dispose();
				}
			}
			base.Dispose(Disposing);
		}
//Required by the Windows Form Designer
		private System.ComponentModel.IContainer components;
		public System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.Button withEventsField_cmdOK;
		public System.Windows.Forms.Button cmdOK {
			get { return withEventsField_cmdOK; }
			set {
				if (withEventsField_cmdOK != null) {
					withEventsField_cmdOK.Click -= cmdOK_Click;
				}
				withEventsField_cmdOK = value;
				if (withEventsField_cmdOK != null) {
					withEventsField_cmdOK.Click += cmdOK_Click;
				}
			}
		}
		private System.Windows.Forms.Button withEventsField_cmdChangeFolder;
		public System.Windows.Forms.Button cmdChangeFolder {
			get { return withEventsField_cmdChangeFolder; }
			set {
				if (withEventsField_cmdChangeFolder != null) {
					withEventsField_cmdChangeFolder.Click -= cmdChangeFolder_Click;
				}
				withEventsField_cmdChangeFolder = value;
				if (withEventsField_cmdChangeFolder != null) {
					withEventsField_cmdChangeFolder.Click += cmdChangeFolder_Click;
				}
			}
		}
		public System.Windows.Forms.TextBox txtSaveIn;
		public System.Windows.Forms.Label lblSaveIn;
//NOTE: The following procedure is required by the Windows Form Designer
//It can be modified using the Windows Form Designer.
//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]
		private void InitializeComponent()
		{
			this.cmdCancel = new System.Windows.Forms.Button();
			this.cmdOK = new System.Windows.Forms.Button();
			this.cmdChangeFolder = new System.Windows.Forms.Button();
			this.txtSaveIn = new System.Windows.Forms.TextBox();
			this.lblSaveIn = new System.Windows.Forms.Label();
			this.lblFileNameFormat = new System.Windows.Forms.Label();
			this.txtFileNameFormat = new System.Windows.Forms.TextBox();
			this.lblFilenameFormatResult = new System.Windows.Forms.Label();
			this.cmdReset = new System.Windows.Forms.Button();
			this.lblRunAfter = new System.Windows.Forms.Label();
			this.txtRunAfter = new System.Windows.Forms.TextBox();
			this.lblRunAfterFileDef = new System.Windows.Forms.Label();
			this.uxRunOnStartup = new System.Windows.Forms.CheckBox();
			this.uxCloseToSystray = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			//
			//cmdCancel
			//
			this.cmdCancel.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.cmdCancel.BackColor = System.Drawing.SystemColors.Control;
			this.cmdCancel.Cursor = System.Windows.Forms.Cursors.Default;
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdCancel.Location = new System.Drawing.Point(297, 313);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.cmdCancel.Size = new System.Drawing.Size(77, 25);
			this.cmdCancel.TabIndex = 1;
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UseVisualStyleBackColor = false;
			//
			//cmdOK
			//
			this.cmdOK.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.cmdOK.BackColor = System.Drawing.SystemColors.Control;
			this.cmdOK.Cursor = System.Windows.Forms.Cursors.Default;
			this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdOK.Location = new System.Drawing.Point(214, 313);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.cmdOK.Size = new System.Drawing.Size(77, 25);
			this.cmdOK.TabIndex = 0;
			this.cmdOK.Text = "OK";
			this.cmdOK.UseVisualStyleBackColor = false;
			//
			//cmdChangeFolder
			//
			this.cmdChangeFolder.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.cmdChangeFolder.BackColor = System.Drawing.SystemColors.Control;
			this.cmdChangeFolder.Cursor = System.Windows.Forms.Cursors.Default;
			this.cmdChangeFolder.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdChangeFolder.ForeColor = System.Drawing.SystemColors.ControlText;
			this.cmdChangeFolder.Location = new System.Drawing.Point(384, 106);
			this.cmdChangeFolder.Name = "cmdChangeFolder";
			this.cmdChangeFolder.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.cmdChangeFolder.Size = new System.Drawing.Size(73, 25);
			this.cmdChangeFolder.TabIndex = 7;
			this.cmdChangeFolder.Text = "&Change";
			this.cmdChangeFolder.UseVisualStyleBackColor = false;
			//
			//txtSaveIn
			//
			this.txtSaveIn.AcceptsReturn = true;
			this.txtSaveIn.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.txtSaveIn.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.txtSaveIn.ForeColor = System.Drawing.SystemColors.WindowText;
			this.txtSaveIn.Location = new System.Drawing.Point(37, 109);
			this.txtSaveIn.Margin = new System.Windows.Forms.Padding(3, 3, 3, 20);
			this.txtSaveIn.MaxLength = 0;
			this.txtSaveIn.Name = "txtSaveIn";
			this.txtSaveIn.ReadOnly = true;
			this.txtSaveIn.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.txtSaveIn.Size = new System.Drawing.Size(341, 20);
			this.txtSaveIn.TabIndex = 6;
			//
			//lblSaveIn
			//
			this.lblSaveIn.AutoSize = true;
			this.lblSaveIn.BackColor = System.Drawing.SystemColors.Control;
			this.lblSaveIn.Cursor = System.Windows.Forms.Cursors.Default;
			this.lblSaveIn.ForeColor = System.Drawing.SystemColors.ControlText;
			this.lblSaveIn.Location = new System.Drawing.Point(12, 88);
			this.lblSaveIn.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
			this.lblSaveIn.Name = "lblSaveIn";
			this.lblSaveIn.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.lblSaveIn.Size = new System.Drawing.Size(167, 13);
			this.lblSaveIn.TabIndex = 5;
			this.lblSaveIn.Text = "Save &downloaded programmes in:";
			//
			//lblFileNameFormat
			//
			this.lblFileNameFormat.AutoSize = true;
			this.lblFileNameFormat.Location = new System.Drawing.Point(12, 145);
			this.lblFileNameFormat.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
			this.lblFileNameFormat.Name = "lblFileNameFormat";
			this.lblFileNameFormat.Size = new System.Drawing.Size(202, 13);
			this.lblFileNameFormat.TabIndex = 8;
			this.lblFileNameFormat.Text = "Downloaded programme file name &format:";
			//
			//txtFileNameFormat
			//
			this.txtFileNameFormat.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.txtFileNameFormat.Location = new System.Drawing.Point(37, 166);
			this.txtFileNameFormat.Name = "txtFileNameFormat";
			this.txtFileNameFormat.Size = new System.Drawing.Size(420, 20);
			this.txtFileNameFormat.TabIndex = 9;
			//
			//lblFilenameFormatResult
			//
			this.lblFilenameFormatResult.AutoSize = true;
			this.lblFilenameFormatResult.Location = new System.Drawing.Point(34, 196);
			this.lblFilenameFormatResult.Margin = new System.Windows.Forms.Padding(3, 3, 3, 17);
			this.lblFilenameFormatResult.Name = "lblFilenameFormatResult";
			this.lblFilenameFormatResult.Size = new System.Drawing.Size(40, 13);
			this.lblFilenameFormatResult.TabIndex = 10;
			this.lblFilenameFormatResult.Text = "Result:";
			//
			//cmdReset
			//
			this.cmdReset.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.cmdReset.BackColor = System.Drawing.SystemColors.Control;
			this.cmdReset.Cursor = System.Windows.Forms.Cursors.Default;
			this.cmdReset.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdReset.Location = new System.Drawing.Point(380, 313);
			this.cmdReset.Name = "cmdReset";
			this.cmdReset.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.cmdReset.Size = new System.Drawing.Size(77, 25);
			this.cmdReset.TabIndex = 2;
			this.cmdReset.Text = "&Reset";
			this.cmdReset.UseVisualStyleBackColor = false;
			//
			//lblRunAfter
			//
			this.lblRunAfter.AutoSize = true;
			this.lblRunAfter.Location = new System.Drawing.Point(12, 223);
			this.lblRunAfter.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
			this.lblRunAfter.Name = "lblRunAfter";
			this.lblRunAfter.Size = new System.Drawing.Size(229, 13);
			this.lblRunAfter.TabIndex = 11;
			this.lblRunAfter.Text = "Run command &after download (blank for none):";
			//
			//txtRunAfter
			//
			this.txtRunAfter.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.txtRunAfter.Location = new System.Drawing.Point(37, 244);
			this.txtRunAfter.Name = "txtRunAfter";
			this.txtRunAfter.Size = new System.Drawing.Size(420, 20);
			this.txtRunAfter.TabIndex = 12;
			//
			//lblRunAfterFileDef
			//
			this.lblRunAfterFileDef.AutoSize = true;
			this.lblRunAfterFileDef.Location = new System.Drawing.Point(34, 271);
			this.lblRunAfterFileDef.Margin = new System.Windows.Forms.Padding(3, 3, 3, 17);
			this.lblRunAfterFileDef.Name = "lblRunAfterFileDef";
			this.lblRunAfterFileDef.Size = new System.Drawing.Size(192, 13);
			this.lblRunAfterFileDef.TabIndex = 13;
			this.lblRunAfterFileDef.Text = "%file% = full path to the downloaded file";
			//
			//uxRunOnStartup
			//
			this.uxRunOnStartup.AutoSize = true;
			this.uxRunOnStartup.Location = new System.Drawing.Point(15, 19);
			this.uxRunOnStartup.Margin = new System.Windows.Forms.Padding(6, 10, 3, 15);
			this.uxRunOnStartup.Name = "uxRunOnStartup";
			this.uxRunOnStartup.Size = new System.Drawing.Size(240, 17);
			this.uxRunOnStartup.TabIndex = 3;
			this.uxRunOnStartup.Text = "Run Radio Downloader on computer &startup?";
			this.uxRunOnStartup.UseVisualStyleBackColor = true;
			//
			//uxCloseToSystray
			//
			this.uxCloseToSystray.AutoSize = true;
			this.uxCloseToSystray.Location = new System.Drawing.Point(15, 51);
			this.uxCloseToSystray.Margin = new System.Windows.Forms.Padding(6, 0, 3, 20);
			this.uxCloseToSystray.Name = "uxCloseToSystray";
			this.uxCloseToSystray.Size = new System.Drawing.Size(397, 17);
			this.uxCloseToSystray.TabIndex = 4;
			this.uxCloseToSystray.Text = "&Minimise to notification area instead of taskbar button on close (requires resta" + "rt)";
			this.uxCloseToSystray.UseVisualStyleBackColor = true;
			//
			//Preferences
			//
			this.AcceptButton = this.cmdOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(469, 350);
			this.Controls.Add(this.uxCloseToSystray);
			this.Controls.Add(this.uxRunOnStartup);
			this.Controls.Add(this.lblRunAfterFileDef);
			this.Controls.Add(this.txtRunAfter);
			this.Controls.Add(this.lblRunAfter);
			this.Controls.Add(this.cmdReset);
			this.Controls.Add(this.lblFilenameFormatResult);
			this.Controls.Add(this.txtFileNameFormat);
			this.Controls.Add(this.lblFileNameFormat);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdOK);
			this.Controls.Add(this.cmdChangeFolder);
			this.Controls.Add(this.txtSaveIn);
			this.Controls.Add(this.lblSaveIn);
			this.Cursor = System.Windows.Forms.Cursors.Default;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Location = new System.Drawing.Point(3, 29);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Preferences";
			this.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Main Options";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		internal System.Windows.Forms.Label lblFileNameFormat;
		private System.Windows.Forms.TextBox withEventsField_txtFileNameFormat;
		internal System.Windows.Forms.TextBox txtFileNameFormat {
			get { return withEventsField_txtFileNameFormat; }
			set {
				if (withEventsField_txtFileNameFormat != null) {
					withEventsField_txtFileNameFormat.TextChanged -= txtFileNameFormat_TextChanged;
				}
				withEventsField_txtFileNameFormat = value;
				if (withEventsField_txtFileNameFormat != null) {
					withEventsField_txtFileNameFormat.TextChanged += txtFileNameFormat_TextChanged;
				}
			}
		}
		internal System.Windows.Forms.Label lblFilenameFormatResult;
		private System.Windows.Forms.Button withEventsField_cmdReset;
		public System.Windows.Forms.Button cmdReset {
			get { return withEventsField_cmdReset; }
			set {
				if (withEventsField_cmdReset != null) {
					withEventsField_cmdReset.Click -= cmdReset_Click;
				}
				withEventsField_cmdReset = value;
				if (withEventsField_cmdReset != null) {
					withEventsField_cmdReset.Click += cmdReset_Click;
				}
			}
		}
		internal System.Windows.Forms.Label lblRunAfter;
		internal System.Windows.Forms.TextBox txtRunAfter;
		internal System.Windows.Forms.Label lblRunAfterFileDef;
		internal System.Windows.Forms.CheckBox uxRunOnStartup;
			#endregion
		internal System.Windows.Forms.CheckBox uxCloseToSystray;
	}
}
