// Disable warnings in the generated code
#pragma warning disable IDE0001 // Simplify Names
#pragma warning disable IDE0002 // Simplify Member Access
#pragma warning disable IDE0004 // Remove Unnecessary Cast

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
            this.CheckOrphan = new System.Windows.Forms.CheckBox();
            this.CheckByDate = new System.Windows.Forms.CheckBox();
            this.DateOlderThan = new System.Windows.Forms.DateTimePicker();
            this.CheckKeepFiles = new System.Windows.Forms.CheckBox();
            this.ListProgrammes = new System.Windows.Forms.ComboBox();
            this.CheckByProgramme = new System.Windows.Forms.CheckBox();
            this.CheckPlayed = new System.Windows.Forms.CheckBox();
            this.LabelRemove = new System.Windows.Forms.Label();
            this.LabelAdditional = new System.Windows.Forms.Label();
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
            this.ButtonOk.Location = new System.Drawing.Point(130, 297);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ButtonOk.Size = new System.Drawing.Size(75, 23);
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
            this.ButtonCancel.Location = new System.Drawing.Point(211, 297);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ButtonCancel.Size = new System.Drawing.Size(75, 23);
            this.ButtonCancel.TabIndex = 1;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = false;
            // 
            // CheckOrphan
            // 
            this.CheckOrphan.AutoSize = true;
            this.CheckOrphan.Checked = true;
            this.CheckOrphan.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CheckOrphan.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CheckOrphan.Location = new System.Drawing.Point(12, 147);
            this.CheckOrphan.Name = "CheckOrphan";
            this.CheckOrphan.Size = new System.Drawing.Size(160, 20);
            this.CheckOrphan.TabIndex = 2;
            this.CheckOrphan.Text = "Have missing audio files";
            this.CheckOrphan.CheckedChanged += new System.EventHandler(this.CheckOrphan_CheckedChanged);
            // 
            // CheckByDate
            // 
            this.CheckByDate.AutoSize = true;
            this.CheckByDate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CheckByDate.Location = new System.Drawing.Point(12, 43);
            this.CheckByDate.Name = "CheckByDate";
            this.CheckByDate.Size = new System.Drawing.Size(107, 20);
            this.CheckByDate.TabIndex = 4;
            this.CheckByDate.Text = "Are older than";
            this.CheckByDate.CheckedChanged += new System.EventHandler(this.CheckByDate_CheckedChanged);
            // 
            // DateOlderThan
            // 
            this.DateOlderThan.Enabled = false;
            this.DateOlderThan.Location = new System.Drawing.Point(29, 66);
            this.DateOlderThan.Margin = new System.Windows.Forms.Padding(20, 0, 3, 3);
            this.DateOlderThan.Name = "DateOlderThan";
            this.DateOlderThan.Size = new System.Drawing.Size(257, 23);
            this.DateOlderThan.TabIndex = 5;
            // 
            // CheckKeepFiles
            // 
            this.CheckKeepFiles.AutoSize = true;
            this.CheckKeepFiles.Enabled = false;
            this.CheckKeepFiles.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CheckKeepFiles.Location = new System.Drawing.Point(12, 257);
            this.CheckKeepFiles.Name = "CheckKeepFiles";
            this.CheckKeepFiles.Size = new System.Drawing.Size(160, 20);
            this.CheckKeepFiles.TabIndex = 6;
            this.CheckKeepFiles.Text = "Do not delete audio files";
            this.CheckKeepFiles.UseVisualStyleBackColor = true;
            this.CheckKeepFiles.CheckedChanged += new System.EventHandler(this.CheckKeepFiles_CheckedChanged);
            // 
            // ListProgrammes
            // 
            this.ListProgrammes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ListProgrammes.Enabled = false;
            this.ListProgrammes.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ListProgrammes.FormattingEnabled = true;
            this.ListProgrammes.Location = new System.Drawing.Point(29, 118);
            this.ListProgrammes.Margin = new System.Windows.Forms.Padding(20, 0, 3, 3);
            this.ListProgrammes.Name = "ListProgrammes";
            this.ListProgrammes.Size = new System.Drawing.Size(257, 23);
            this.ListProgrammes.Sorted = true;
            this.ListProgrammes.TabIndex = 7;
            // 
            // CheckByProgramme
            // 
            this.CheckByProgramme.AutoSize = true;
            this.CheckByProgramme.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CheckByProgramme.Location = new System.Drawing.Point(12, 95);
            this.CheckByProgramme.Name = "CheckByProgramme";
            this.CheckByProgramme.Size = new System.Drawing.Size(169, 20);
            this.CheckByProgramme.TabIndex = 8;
            this.CheckByProgramme.Text = "Belong to the programme";
            this.CheckByProgramme.CheckedChanged += new System.EventHandler(this.CheckByProgramme_CheckedChanged);
            // 
            // CheckPlayed
            // 
            this.CheckPlayed.AutoSize = true;
            this.CheckPlayed.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CheckPlayed.Location = new System.Drawing.Point(12, 173);
            this.CheckPlayed.Margin = new System.Windows.Forms.Padding(3, 3, 3, 30);
            this.CheckPlayed.Name = "CheckPlayed";
            this.CheckPlayed.Size = new System.Drawing.Size(126, 20);
            this.CheckPlayed.TabIndex = 9;
            this.CheckPlayed.Text = "Have been played";
            this.CheckPlayed.UseVisualStyleBackColor = true;
            this.CheckPlayed.CheckedChanged += new System.EventHandler(this.CheckPlayed_CheckedChanged);
            // 
            // LabelRemove
            // 
            this.LabelRemove.AutoSize = true;
            this.LabelRemove.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelRemove.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.LabelRemove.Location = new System.Drawing.Point(8, 9);
            this.LabelRemove.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
            this.LabelRemove.Name = "LabelRemove";
            this.LabelRemove.Size = new System.Drawing.Size(192, 21);
            this.LabelRemove.TabIndex = 10;
            this.LabelRemove.Text = "Remove downloads which";
            // 
            // LabelAdditional
            // 
            this.LabelAdditional.AutoSize = true;
            this.LabelAdditional.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelAdditional.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.LabelAdditional.Location = new System.Drawing.Point(8, 223);
            this.LabelAdditional.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
            this.LabelAdditional.Name = "LabelAdditional";
            this.LabelAdditional.Size = new System.Drawing.Size(137, 21);
            this.LabelAdditional.TabIndex = 11;
            this.LabelAdditional.Text = "Additional options";
            // 
            // CleanUp
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(298, 332);
            this.Controls.Add(this.LabelAdditional);
            this.Controls.Add(this.LabelRemove);
            this.Controls.Add(this.CheckPlayed);
            this.Controls.Add(this.CheckByProgramme);
            this.Controls.Add(this.ListProgrammes);
            this.Controls.Add(this.CheckKeepFiles);
            this.Controls.Add(this.DateOlderThan);
            this.Controls.Add(this.CheckByDate);
            this.Controls.Add(this.CheckOrphan);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOk);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.Icon = global::RadioDld.Properties.Resources.icon_main;
            this.KeyPreview = true;
            this.Location = new System.Drawing.Point(3, 29);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CleanUp";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Clean Up";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.CleanUp_HelpButtonClicked);
            this.Load += new System.EventHandler(this.CleanUp_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CleanUp_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Label LabelRemove;
        private System.Windows.Forms.Label LabelAdditional;
        private System.Windows.Forms.CheckBox CheckOrphan;
        private System.Windows.Forms.CheckBox CheckByDate;
        private System.Windows.Forms.DateTimePicker DateOlderThan;
        private System.Windows.Forms.CheckBox CheckKeepFiles;
        private System.Windows.Forms.ComboBox ListProgrammes;
        private System.Windows.Forms.CheckBox CheckByProgramme;
        private System.Windows.Forms.CheckBox CheckPlayed;

        #endregion
    }
}
