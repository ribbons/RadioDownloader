namespace RadioDld
{
    internal partial class Preferences
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
            System.Windows.Forms.Label LabelSaveIn;
            System.Windows.Forms.Label LabelFileNameFormat;
            System.Windows.Forms.Label LabelRunAfter;
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.ButtonChangeFolder = new System.Windows.Forms.Button();
            this.TextSaveIn = new System.Windows.Forms.TextBox();
            this.TextFileNameFormat = new System.Windows.Forms.TextBox();
            this.LabelFilenameFormatResult = new System.Windows.Forms.Label();
            this.ButtonReset = new System.Windows.Forms.Button();
            this.TextRunAfter = new System.Windows.Forms.TextBox();
            this.LabelRunAfterTokDef = new System.Windows.Forms.Label();
            this.CheckRunOnStartup = new System.Windows.Forms.CheckBox();
            this.CheckCloseToSystray = new System.Windows.Forms.CheckBox();
            LabelSaveIn = new System.Windows.Forms.Label();
            LabelFileNameFormat = new System.Windows.Forms.Label();
            LabelRunAfter = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LabelSaveIn
            // 
            LabelSaveIn.AutoSize = true;
            LabelSaveIn.BackColor = System.Drawing.SystemColors.Control;
            LabelSaveIn.Cursor = System.Windows.Forms.Cursors.Default;
            LabelSaveIn.ForeColor = System.Drawing.SystemColors.ControlText;
            LabelSaveIn.Location = new System.Drawing.Point(12, 88);
            LabelSaveIn.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
            LabelSaveIn.Name = "LabelSaveIn";
            LabelSaveIn.RightToLeft = System.Windows.Forms.RightToLeft.No;
            LabelSaveIn.Size = new System.Drawing.Size(167, 13);
            LabelSaveIn.TabIndex = 5;
            LabelSaveIn.Text = "Save &downloaded programmes in:";
            // 
            // LabelFileNameFormat
            // 
            LabelFileNameFormat.AutoSize = true;
            LabelFileNameFormat.Location = new System.Drawing.Point(12, 145);
            LabelFileNameFormat.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
            LabelFileNameFormat.Name = "LabelFileNameFormat";
            LabelFileNameFormat.Size = new System.Drawing.Size(202, 13);
            LabelFileNameFormat.TabIndex = 8;
            LabelFileNameFormat.Text = "Downloaded programme file name &format:";
            // 
            // LabelRunAfter
            // 
            LabelRunAfter.AutoSize = true;
            LabelRunAfter.Location = new System.Drawing.Point(12, 223);
            LabelRunAfter.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
            LabelRunAfter.Name = "LabelRunAfter";
            LabelRunAfter.Size = new System.Drawing.Size(229, 13);
            LabelRunAfter.TabIndex = 11;
            LabelRunAfter.Text = "Run command &after download (blank for none):";
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.BackColor = System.Drawing.SystemColors.Control;
            this.ButtonCancel.Cursor = System.Windows.Forms.Cursors.Default;
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonCancel.Location = new System.Drawing.Point(297, 313);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ButtonCancel.Size = new System.Drawing.Size(77, 25);
            this.ButtonCancel.TabIndex = 1;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = false;
            // 
            // ButtonOk
            // 
            this.ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOk.BackColor = System.Drawing.SystemColors.Control;
            this.ButtonOk.Cursor = System.Windows.Forms.Cursors.Default;
            this.ButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOk.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonOk.Location = new System.Drawing.Point(214, 313);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ButtonOk.Size = new System.Drawing.Size(77, 25);
            this.ButtonOk.TabIndex = 0;
            this.ButtonOk.Text = "OK";
            this.ButtonOk.UseVisualStyleBackColor = false;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // ButtonChangeFolder
            // 
            this.ButtonChangeFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonChangeFolder.BackColor = System.Drawing.SystemColors.Control;
            this.ButtonChangeFolder.Cursor = System.Windows.Forms.Cursors.Default;
            this.ButtonChangeFolder.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonChangeFolder.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ButtonChangeFolder.Location = new System.Drawing.Point(384, 106);
            this.ButtonChangeFolder.Name = "ButtonChangeFolder";
            this.ButtonChangeFolder.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ButtonChangeFolder.Size = new System.Drawing.Size(73, 25);
            this.ButtonChangeFolder.TabIndex = 7;
            this.ButtonChangeFolder.Text = "&Change";
            this.ButtonChangeFolder.UseVisualStyleBackColor = false;
            this.ButtonChangeFolder.Click += new System.EventHandler(this.ButtonChangeFolder_Click);
            // 
            // TextSaveIn
            // 
            this.TextSaveIn.AcceptsReturn = true;
            this.TextSaveIn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TextSaveIn.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.TextSaveIn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.TextSaveIn.Location = new System.Drawing.Point(37, 109);
            this.TextSaveIn.Margin = new System.Windows.Forms.Padding(3, 3, 3, 20);
            this.TextSaveIn.MaxLength = 0;
            this.TextSaveIn.Name = "TextSaveIn";
            this.TextSaveIn.ReadOnly = true;
            this.TextSaveIn.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.TextSaveIn.Size = new System.Drawing.Size(341, 20);
            this.TextSaveIn.TabIndex = 6;
            // 
            // TextFileNameFormat
            // 
            this.TextFileNameFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TextFileNameFormat.Location = new System.Drawing.Point(37, 166);
            this.TextFileNameFormat.Name = "TextFileNameFormat";
            this.TextFileNameFormat.Size = new System.Drawing.Size(420, 20);
            this.TextFileNameFormat.TabIndex = 9;
            this.TextFileNameFormat.TextChanged += new System.EventHandler(this.TextFileNameFormat_TextChanged);
            // 
            // LabelFilenameFormatResult
            // 
            this.LabelFilenameFormatResult.AutoSize = true;
            this.LabelFilenameFormatResult.Location = new System.Drawing.Point(34, 196);
            this.LabelFilenameFormatResult.Margin = new System.Windows.Forms.Padding(3, 3, 3, 17);
            this.LabelFilenameFormatResult.Name = "LabelFilenameFormatResult";
            this.LabelFilenameFormatResult.Size = new System.Drawing.Size(40, 13);
            this.LabelFilenameFormatResult.TabIndex = 10;
            this.LabelFilenameFormatResult.Text = "Result:";
            // 
            // ButtonReset
            // 
            this.ButtonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonReset.BackColor = System.Drawing.SystemColors.Control;
            this.ButtonReset.Cursor = System.Windows.Forms.Cursors.Default;
            this.ButtonReset.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonReset.Location = new System.Drawing.Point(380, 313);
            this.ButtonReset.Name = "ButtonReset";
            this.ButtonReset.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ButtonReset.Size = new System.Drawing.Size(77, 25);
            this.ButtonReset.TabIndex = 2;
            this.ButtonReset.Text = "&Reset";
            this.ButtonReset.UseVisualStyleBackColor = false;
            this.ButtonReset.Click += new System.EventHandler(this.ButtonReset_Click);
            // 
            // TextRunAfter
            // 
            this.TextRunAfter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TextRunAfter.Location = new System.Drawing.Point(37, 244);
            this.TextRunAfter.Name = "TextRunAfter";
            this.TextRunAfter.Size = new System.Drawing.Size(420, 20);
            this.TextRunAfter.TabIndex = 12;
            // 
            // LabelRunAfterTokDef
            // 
            this.LabelRunAfterTokDef.AutoSize = true;
            this.LabelRunAfterTokDef.Location = new System.Drawing.Point(34, 271);
            this.LabelRunAfterTokDef.Margin = new System.Windows.Forms.Padding(3, 3, 3, 17);
            this.LabelRunAfterTokDef.Name = "LabelRunAfterTokDef";
            this.LabelRunAfterTokDef.Size = new System.Drawing.Size(192, 13);
            this.LabelRunAfterTokDef.TabIndex = 13;
            this.LabelRunAfterTokDef.Text = "%file% = full path to the downloaded file";
            // 
            // CheckRunOnStartup
            // 
            this.CheckRunOnStartup.AutoSize = true;
            this.CheckRunOnStartup.Location = new System.Drawing.Point(15, 19);
            this.CheckRunOnStartup.Margin = new System.Windows.Forms.Padding(6, 10, 3, 15);
            this.CheckRunOnStartup.Name = "CheckRunOnStartup";
            this.CheckRunOnStartup.Size = new System.Drawing.Size(240, 17);
            this.CheckRunOnStartup.TabIndex = 3;
            this.CheckRunOnStartup.Text = "Run Radio Downloader on computer &startup?";
            this.CheckRunOnStartup.UseVisualStyleBackColor = true;
            // 
            // CheckCloseToSystray
            // 
            this.CheckCloseToSystray.AutoSize = true;
            this.CheckCloseToSystray.Location = new System.Drawing.Point(15, 51);
            this.CheckCloseToSystray.Margin = new System.Windows.Forms.Padding(6, 0, 3, 20);
            this.CheckCloseToSystray.Name = "CheckCloseToSystray";
            this.CheckCloseToSystray.Size = new System.Drawing.Size(397, 17);
            this.CheckCloseToSystray.TabIndex = 4;
            this.CheckCloseToSystray.Text = "&Minimise to notification area instead of taskbar button on close (requires resta" +
                "rt)";
            this.CheckCloseToSystray.UseVisualStyleBackColor = true;
            // 
            // Preferences
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(469, 350);
            this.Controls.Add(this.CheckCloseToSystray);
            this.Controls.Add(this.CheckRunOnStartup);
            this.Controls.Add(this.LabelRunAfterTokDef);
            this.Controls.Add(this.TextRunAfter);
            this.Controls.Add(LabelRunAfter);
            this.Controls.Add(this.ButtonReset);
            this.Controls.Add(this.LabelFilenameFormatResult);
            this.Controls.Add(this.TextFileNameFormat);
            this.Controls.Add(LabelFileNameFormat);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOk);
            this.Controls.Add(this.ButtonChangeFolder);
            this.Controls.Add(this.TextSaveIn);
            this.Controls.Add(LabelSaveIn);
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
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Preferences_FormClosing);
            this.Load += new System.EventHandler(this.Preferences_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.TextBox TextFileNameFormat;
        private System.Windows.Forms.Label LabelFilenameFormatResult;
        private System.Windows.Forms.Button ButtonReset;
        private System.Windows.Forms.TextBox TextRunAfter;
        private System.Windows.Forms.Label LabelRunAfterTokDef;
        private System.Windows.Forms.CheckBox CheckRunOnStartup;
        private System.Windows.Forms.CheckBox CheckCloseToSystray;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.Button ButtonChangeFolder;
        private System.Windows.Forms.TextBox TextSaveIn;

        #endregion
    }
}
