namespace RadioDld
{
    partial class CleanUp
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
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.lblExplainOrphan = new System.Windows.Forms.Label();
            this.radType = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
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
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
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
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // lblExplainOrphan
            // 
            this.lblExplainOrphan.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblExplainOrphan.BackColor = System.Drawing.SystemColors.Control;
            this.lblExplainOrphan.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblExplainOrphan.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblExplainOrphan.Location = new System.Drawing.Point(36, 36);
            this.lblExplainOrphan.Name = "lblExplainOrphan";
            this.lblExplainOrphan.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblExplainOrphan.Size = new System.Drawing.Size(311, 44);
            this.lblExplainOrphan.TabIndex = 3;
            this.lblExplainOrphan.Text = "This will remove the programmes in your download list for which the audio file ha" +
                "s been moved or deleted.";
            // 
            // radType
            // 
            this.radType.AutoSize = true;
            this.radType.Checked = true;
            this.radType.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radType.Location = new System.Drawing.Point(12, 15);
            this.radType.Name = "radType";
            this.radType.Size = new System.Drawing.Size(141, 18);
            this.radType.TabIndex = 2;
            this.radType.TabStop = true;
            this.radType.Text = "&Remove orphan entries";
            this.radType.UseVisualStyleBackColor = true;
            // 
            // CleanUp
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(359, 146);
            this.Controls.Add(this.radType);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.lblExplainOrphan);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::RadioDld.Properties.Resources.icon_main;
            this.Location = new System.Drawing.Point(3, 29);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CleanUp";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Clean Up";
            this.Load += new System.EventHandler(this.CleanUp_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        internal System.Windows.Forms.Button cmdOK;
        internal System.Windows.Forms.Button cmdCancel;
        internal System.Windows.Forms.Label lblExplainOrphan;
        internal System.Windows.Forms.RadioButton radType;

        #endregion
    }
}
