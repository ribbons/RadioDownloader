namespace RadioDld
{
    internal partial class CleanUp
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
            this.ButtonOk = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.LabelOrphan = new System.Windows.Forms.Label();
            this.RadioOrphan = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // ButtonOk
            // 
            this.ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOk.BackColor = System.Drawing.SystemColors.Control;
            this.ButtonOk.Cursor = System.Windows.Forms.Cursors.Default;
            this.ButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOk.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonOk.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ButtonOk.Location = new System.Drawing.Point(187, 109);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ButtonOk.Size = new System.Drawing.Size(77, 25);
            this.ButtonOk.TabIndex = 0;
            this.ButtonOk.Text = "OK";
            this.ButtonOk.UseVisualStyleBackColor = false;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.BackColor = System.Drawing.SystemColors.Control;
            this.ButtonCancel.Cursor = System.Windows.Forms.Cursors.Default;
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonCancel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ButtonCancel.Location = new System.Drawing.Point(270, 109);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ButtonCancel.Size = new System.Drawing.Size(77, 25);
            this.ButtonCancel.TabIndex = 1;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = false;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // LabelOrphan
            // 
            this.LabelOrphan.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelOrphan.BackColor = System.Drawing.SystemColors.Control;
            this.LabelOrphan.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelOrphan.ForeColor = System.Drawing.SystemColors.ControlText;
            this.LabelOrphan.Location = new System.Drawing.Point(36, 36);
            this.LabelOrphan.Name = "LabelOrphan";
            this.LabelOrphan.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelOrphan.Size = new System.Drawing.Size(311, 44);
            this.LabelOrphan.TabIndex = 3;
            this.LabelOrphan.Text = "This will remove the programmes in your download list for which the audio file ha" +
    "s been moved or deleted.";
            // 
            // RadioOrphan
            // 
            this.RadioOrphan.AutoSize = true;
            this.RadioOrphan.Checked = true;
            this.RadioOrphan.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.RadioOrphan.Location = new System.Drawing.Point(12, 15);
            this.RadioOrphan.Name = "RadioOrphan";
            this.RadioOrphan.Size = new System.Drawing.Size(153, 20);
            this.RadioOrphan.TabIndex = 2;
            this.RadioOrphan.TabStop = true;
            this.RadioOrphan.Text = "&Remove orphan entries";
            this.RadioOrphan.UseVisualStyleBackColor = true;
            // 
            // CleanUp
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(359, 146);
            this.Controls.Add(this.RadioOrphan);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOk);
            this.Controls.Add(this.LabelOrphan);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Label LabelOrphan;
        private System.Windows.Forms.RadioButton RadioOrphan;

        #endregion
    }
}
