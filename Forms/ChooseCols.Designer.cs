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
	partial class ChooseCols : System.Windows.Forms.Form
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
			this.HideButton = new System.Windows.Forms.Button();
			this.ShowButton = new System.Windows.Forms.Button();
			this.MoveDown = new System.Windows.Forms.Button();
			this.MoveUp = new System.Windows.Forms.Button();
			this.Okay = new System.Windows.Forms.Button();
			this.Cancel = new System.Windows.Forms.Button();
			this.ColumnsList = new System.Windows.Forms.ListView();
			this.InfoLabel = new System.Windows.Forms.Label();
			this.ColumnsLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			//HideButton
			//
			this.HideButton.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.HideButton.Enabled = false;
			this.HideButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HideButton.Location = new System.Drawing.Point(251, 157);
			this.HideButton.Name = "HideButton";
			this.HideButton.Size = new System.Drawing.Size(77, 25);
			this.HideButton.TabIndex = 8;
			this.HideButton.Text = "&Hide";
			this.HideButton.UseVisualStyleBackColor = true;
			//
			//ShowButton
			//
			this.ShowButton.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.ShowButton.Enabled = false;
			this.ShowButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowButton.Location = new System.Drawing.Point(251, 126);
			this.ShowButton.Name = "ShowButton";
			this.ShowButton.Size = new System.Drawing.Size(77, 25);
			this.ShowButton.TabIndex = 7;
			this.ShowButton.Text = "&Show";
			this.ShowButton.UseVisualStyleBackColor = true;
			//
			//MoveDown
			//
			this.MoveDown.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.MoveDown.Enabled = false;
			this.MoveDown.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MoveDown.Location = new System.Drawing.Point(251, 95);
			this.MoveDown.Name = "MoveDown";
			this.MoveDown.Size = new System.Drawing.Size(77, 25);
			this.MoveDown.TabIndex = 6;
			this.MoveDown.Text = "Move &Down";
			this.MoveDown.UseVisualStyleBackColor = true;
			//
			//MoveUp
			//
			this.MoveUp.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.MoveUp.Enabled = false;
			this.MoveUp.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MoveUp.Location = new System.Drawing.Point(251, 64);
			this.MoveUp.Name = "MoveUp";
			this.MoveUp.Size = new System.Drawing.Size(77, 25);
			this.MoveUp.TabIndex = 5;
			this.MoveUp.Text = "Move &Up";
			this.MoveUp.UseVisualStyleBackColor = true;
			//
			//Okay
			//
			this.Okay.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.Okay.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Okay.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Okay.Location = new System.Drawing.Point(168, 286);
			this.Okay.Name = "Okay";
			this.Okay.Size = new System.Drawing.Size(77, 25);
			this.Okay.TabIndex = 0;
			this.Okay.Text = "OK";
			this.Okay.UseVisualStyleBackColor = true;
			//
			//Cancel
			//
			this.Cancel.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Cancel.Location = new System.Drawing.Point(251, 286);
			this.Cancel.Name = "Cancel";
			this.Cancel.Size = new System.Drawing.Size(77, 25);
			this.Cancel.TabIndex = 1;
			this.Cancel.Text = "Cancel";
			this.Cancel.UseVisualStyleBackColor = true;
			//
			//ColumnsList
			//
			this.ColumnsList.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.ColumnsList.CheckBoxes = true;
			this.ColumnsList.HideSelection = false;
			this.ColumnsList.Location = new System.Drawing.Point(12, 64);
			this.ColumnsList.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
			this.ColumnsList.Name = "ColumnsList";
			this.ColumnsList.Size = new System.Drawing.Size(230, 199);
			this.ColumnsList.TabIndex = 4;
			this.ColumnsList.UseCompatibleStateImageBehavior = false;
			this.ColumnsList.View = System.Windows.Forms.View.List;
			//
			//InfoLabel
			//
			this.InfoLabel.Location = new System.Drawing.Point(9, 11);
			this.InfoLabel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 0);
			this.InfoLabel.Name = "InfoLabel";
			this.InfoLabel.Size = new System.Drawing.Size(315, 35);
			this.InfoLabel.TabIndex = 2;
			this.InfoLabel.Text = "Select the columns you want to display for this list.";
			//
			//ColumnsLabel
			//
			this.ColumnsLabel.AutoSize = true;
			this.ColumnsLabel.Location = new System.Drawing.Point(9, 46);
			this.ColumnsLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 2);
			this.ColumnsLabel.Name = "ColumnsLabel";
			this.ColumnsLabel.Size = new System.Drawing.Size(50, 13);
			this.ColumnsLabel.TabIndex = 3;
			this.ColumnsLabel.Text = "&Columns:";
			//
			//ChooseCols
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(340, 321);
			this.Controls.Add(this.ColumnsLabel);
			this.Controls.Add(this.InfoLabel);
			this.Controls.Add(this.ColumnsList);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.Okay);
			this.Controls.Add(this.MoveUp);
			this.Controls.Add(this.MoveDown);
			this.Controls.Add(this.ShowButton);
			this.Controls.Add(this.HideButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ChooseCols";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Choose Columns";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private System.Windows.Forms.Button withEventsField_HideButton;
		internal System.Windows.Forms.Button HideButton {
			get { return withEventsField_HideButton; }
			set {
				if (withEventsField_HideButton != null) {
					withEventsField_HideButton.Click -= HideButton_Click;
				}
				withEventsField_HideButton = value;
				if (withEventsField_HideButton != null) {
					withEventsField_HideButton.Click += HideButton_Click;
				}
			}
		}
		private System.Windows.Forms.Button withEventsField_ShowButton;
		internal System.Windows.Forms.Button ShowButton {
			get { return withEventsField_ShowButton; }
			set {
				if (withEventsField_ShowButton != null) {
					withEventsField_ShowButton.Click -= ShowButton_Click;
				}
				withEventsField_ShowButton = value;
				if (withEventsField_ShowButton != null) {
					withEventsField_ShowButton.Click += ShowButton_Click;
				}
			}
		}
		private System.Windows.Forms.Button withEventsField_MoveDown;
		internal System.Windows.Forms.Button MoveDown {
			get { return withEventsField_MoveDown; }
			set {
				if (withEventsField_MoveDown != null) {
					withEventsField_MoveDown.Click -= MoveDown_Click;
				}
				withEventsField_MoveDown = value;
				if (withEventsField_MoveDown != null) {
					withEventsField_MoveDown.Click += MoveDown_Click;
				}
			}
		}
		private System.Windows.Forms.Button withEventsField_MoveUp;
		internal System.Windows.Forms.Button MoveUp {
			get { return withEventsField_MoveUp; }
			set {
				if (withEventsField_MoveUp != null) {
					withEventsField_MoveUp.Click -= MoveUp_Click;
				}
				withEventsField_MoveUp = value;
				if (withEventsField_MoveUp != null) {
					withEventsField_MoveUp.Click += MoveUp_Click;
				}
			}
		}
		private System.Windows.Forms.Button withEventsField_Okay;
		internal System.Windows.Forms.Button Okay {
			get { return withEventsField_Okay; }
			set {
				if (withEventsField_Okay != null) {
					withEventsField_Okay.Click -= Okay_Click;
				}
				withEventsField_Okay = value;
				if (withEventsField_Okay != null) {
					withEventsField_Okay.Click += Okay_Click;
				}
			}
		}
		internal System.Windows.Forms.Button Cancel;
		private System.Windows.Forms.ListView withEventsField_ColumnsList;
		internal System.Windows.Forms.ListView ColumnsList {
			get { return withEventsField_ColumnsList; }
			set {
				if (withEventsField_ColumnsList != null) {
					withEventsField_ColumnsList.ItemChecked -= ColumnsList_ItemChecked;
					withEventsField_ColumnsList.SelectedIndexChanged -= ColumnsList_SelectedIndexChanged;
				}
				withEventsField_ColumnsList = value;
				if (withEventsField_ColumnsList != null) {
					withEventsField_ColumnsList.ItemChecked += ColumnsList_ItemChecked;
					withEventsField_ColumnsList.SelectedIndexChanged += ColumnsList_SelectedIndexChanged;
				}
			}
		}
		internal System.Windows.Forms.Label InfoLabel;
		internal System.Windows.Forms.Label ColumnsLabel;
	}
}
