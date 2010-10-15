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
	partial class CleanUp
	{
		#region "Windows Form Designer generated code "
		[System.Diagnostics.DebuggerNonUserCode()]
		public CleanUp() : base()
		{
			Load += CleanUp_Load;
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
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]
		private void InitializeComponent()
		{
			this.cmdOK = new System.Windows.Forms.Button();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.lblExplainOrphan = new System.Windows.Forms.Label();
			this.radType = new System.Windows.Forms.RadioButton();
			this.SuspendLayout();
			//
			//cmdOK
			//
			this.cmdOK.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.cmdOK.BackColor = System.Drawing.SystemColors.Control;
			this.cmdOK.Cursor = System.Windows.Forms.Cursors.Default;
			this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdOK.ForeColor = System.Drawing.SystemColors.ControlText;
			this.cmdOK.Location = new System.Drawing.Point(187, 109);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.cmdOK.Size = new System.Drawing.Size(77, 25);
			this.cmdOK.TabIndex = 0;
			this.cmdOK.Text = "OK";
			this.cmdOK.UseVisualStyleBackColor = false;
			//
			//cmdCancel
			//
			this.cmdCancel.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.cmdCancel.BackColor = System.Drawing.SystemColors.Control;
			this.cmdCancel.Cursor = System.Windows.Forms.Cursors.Default;
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdCancel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.cmdCancel.Location = new System.Drawing.Point(270, 109);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.cmdCancel.Size = new System.Drawing.Size(77, 25);
			this.cmdCancel.TabIndex = 1;
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UseVisualStyleBackColor = false;
			//
			//lblExplainOrphan
			//
			this.lblExplainOrphan.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.lblExplainOrphan.BackColor = System.Drawing.SystemColors.Control;
			this.lblExplainOrphan.Cursor = System.Windows.Forms.Cursors.Default;
			this.lblExplainOrphan.ForeColor = System.Drawing.SystemColors.ControlText;
			this.lblExplainOrphan.Location = new System.Drawing.Point(36, 36);
			this.lblExplainOrphan.Name = "lblExplainOrphan";
			this.lblExplainOrphan.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.lblExplainOrphan.Size = new System.Drawing.Size(311, 44);
			this.lblExplainOrphan.TabIndex = 3;
			this.lblExplainOrphan.Text = "This will remove the programmes in your download list for which the audio file ha" + "s been moved or deleted.";
			//
			//radType
			//
			this.radType.AutoSize = true;
			this.radType.Checked = true;
			this.radType.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radType.Location = new System.Drawing.Point(12, 15);
			this.radType.Name = "radType";
			this.radType.Size = new System.Drawing.Size(143, 18);
			this.radType.TabIndex = 2;
			this.radType.TabStop = true;
			this.radType.Text = "&Remove orphan entries";
			this.radType.UseVisualStyleBackColor = true;
			//
			//CleanUp
			//
			this.AcceptButton = this.cmdOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(359, 146);
			this.Controls.Add(this.radType);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdOK);
			this.Controls.Add(this.lblExplainOrphan);
			this.Cursor = System.Windows.Forms.Cursors.Default;
			this.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, Convert.ToByte(0));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = global::RadioDld.My.Resources.Resources.icon_main;
			this.Location = new System.Drawing.Point(3, 29);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CleanUp";
			this.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Clean Up";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private System.Windows.Forms.Button withEventsField_cmdCancel;
		public System.Windows.Forms.Button cmdCancel {
			get { return withEventsField_cmdCancel; }
			set {
				if (withEventsField_cmdCancel != null) {
					withEventsField_cmdCancel.Click -= cmdCancel_Click;
				}
				withEventsField_cmdCancel = value;
				if (withEventsField_cmdCancel != null) {
					withEventsField_cmdCancel.Click += cmdCancel_Click;
				}
			}
		}
		public System.Windows.Forms.Label lblExplainOrphan;
			#endregion
		internal System.Windows.Forms.RadioButton radType;
	}
}
