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
            System.Windows.Forms.Label LabelParallel;
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
            this.NumberParallel = new System.Windows.Forms.NumericUpDown();
            LabelSaveIn = new System.Windows.Forms.Label();
            LabelFileNameFormat = new System.Windows.Forms.Label();
            LabelRunAfter = new System.Windows.Forms.Label();
            LabelParallel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.NumberParallel)).BeginInit();
            this.SuspendLayout();
            // 
            // LabelSaveIn
            // 
            LabelSaveIn.AutoSize = true;
            LabelSaveIn.BackColor = System.Drawing.SystemColors.Control;
            LabelSaveIn.Cursor = System.Windows.Forms.Cursors.Default;
            LabelSaveIn.ForeColor = System.Drawing.SystemColors.ControlText;
            LabelSaveIn.Location = new System.Drawing.Point(9, 119);
            LabelSaveIn.Name = "LabelSaveIn";
            LabelSaveIn.RightToLeft = System.Windows.Forms.RightToLeft.No;
            LabelSaveIn.Size = new System.Drawing.Size(187, 15);
            LabelSaveIn.TabIndex = 7;
            LabelSaveIn.Text = "Save downloaded programmes in:";
            // 
            // LabelFileNameFormat
            // 
            LabelFileNameFormat.AutoSize = true;
            LabelFileNameFormat.Location = new System.Drawing.Point(9, 171);
            LabelFileNameFormat.Name = "LabelFileNameFormat";
            LabelFileNameFormat.Size = new System.Drawing.Size(179, 15);
            LabelFileNameFormat.TabIndex = 10;
            LabelFileNameFormat.Text = "&File name format for downloads:";
            // 
            // LabelRunAfter
            // 
            LabelRunAfter.AutoSize = true;
            LabelRunAfter.Location = new System.Drawing.Point(9, 245);
            LabelRunAfter.Name = "LabelRunAfter";
            LabelRunAfter.Size = new System.Drawing.Size(172, 15);
            LabelRunAfter.TabIndex = 13;
            LabelRunAfter.Text = "Run command &after download:";
            // 
            // LabelParallel
            // 
            LabelParallel.AutoSize = true;
            LabelParallel.BackColor = System.Drawing.SystemColors.Control;
            LabelParallel.Cursor = System.Windows.Forms.Cursors.Default;
            LabelParallel.ForeColor = System.Drawing.SystemColors.ControlText;
            LabelParallel.Location = new System.Drawing.Point(9, 67);
            LabelParallel.Name = "LabelParallel";
            LabelParallel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            LabelParallel.Size = new System.Drawing.Size(109, 15);
            LabelParallel.TabIndex = 5;
            LabelParallel.Text = "&Parallel downloads:";
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.BackColor = System.Drawing.SystemColors.Control;
            this.ButtonCancel.Cursor = System.Windows.Forms.Cursors.Default;
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonCancel.Location = new System.Drawing.Point(312, 323);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ButtonCancel.Size = new System.Drawing.Size(75, 23);
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
            this.ButtonOk.Location = new System.Drawing.Point(231, 323);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ButtonOk.Size = new System.Drawing.Size(75, 23);
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
            this.ButtonChangeFolder.Location = new System.Drawing.Point(393, 137);
            this.ButtonChangeFolder.Name = "ButtonChangeFolder";
            this.ButtonChangeFolder.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ButtonChangeFolder.Size = new System.Drawing.Size(75, 23);
            this.ButtonChangeFolder.TabIndex = 9;
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
            this.TextSaveIn.Location = new System.Drawing.Point(12, 137);
            this.TextSaveIn.Margin = new System.Windows.Forms.Padding(23, 3, 3, 11);
            this.TextSaveIn.MaxLength = 0;
            this.TextSaveIn.Name = "TextSaveIn";
            this.TextSaveIn.ReadOnly = true;
            this.TextSaveIn.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.TextSaveIn.Size = new System.Drawing.Size(375, 23);
            this.TextSaveIn.TabIndex = 8;
            // 
            // TextFileNameFormat
            // 
            this.TextFileNameFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextFileNameFormat.Location = new System.Drawing.Point(12, 189);
            this.TextFileNameFormat.Margin = new System.Windows.Forms.Padding(23, 3, 3, 3);
            this.TextFileNameFormat.Name = "TextFileNameFormat";
            this.TextFileNameFormat.Size = new System.Drawing.Size(456, 23);
            this.TextFileNameFormat.TabIndex = 11;
            this.TextFileNameFormat.TextChanged += new System.EventHandler(this.TextFileNameFormat_TextChanged);
            // 
            // LabelFilenameFormatResult
            // 
            this.LabelFilenameFormatResult.AutoSize = true;
            this.LabelFilenameFormatResult.Location = new System.Drawing.Point(9, 219);
            this.LabelFilenameFormatResult.Margin = new System.Windows.Forms.Padding(23, 0, 3, 15);
            this.LabelFilenameFormatResult.Name = "LabelFilenameFormatResult";
            this.LabelFilenameFormatResult.Size = new System.Drawing.Size(42, 15);
            this.LabelFilenameFormatResult.TabIndex = 12;
            this.LabelFilenameFormatResult.Text = "Result:";
            // 
            // ButtonReset
            // 
            this.ButtonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonReset.BackColor = System.Drawing.SystemColors.Control;
            this.ButtonReset.Cursor = System.Windows.Forms.Cursors.Default;
            this.ButtonReset.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonReset.Location = new System.Drawing.Point(393, 323);
            this.ButtonReset.Name = "ButtonReset";
            this.ButtonReset.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ButtonReset.Size = new System.Drawing.Size(75, 23);
            this.ButtonReset.TabIndex = 2;
            this.ButtonReset.Text = "&Reset";
            this.ButtonReset.UseVisualStyleBackColor = false;
            this.ButtonReset.Click += new System.EventHandler(this.ButtonReset_Click);
            // 
            // TextRunAfter
            // 
            this.TextRunAfter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextRunAfter.Location = new System.Drawing.Point(12, 263);
            this.TextRunAfter.Margin = new System.Windows.Forms.Padding(23, 3, 3, 3);
            this.TextRunAfter.Name = "TextRunAfter";
            this.TextRunAfter.Size = new System.Drawing.Size(456, 23);
            this.TextRunAfter.TabIndex = 14;
            // 
            // LabelRunAfterTokDef
            // 
            this.LabelRunAfterTokDef.AutoSize = true;
            this.LabelRunAfterTokDef.Location = new System.Drawing.Point(9, 289);
            this.LabelRunAfterTokDef.Margin = new System.Windows.Forms.Padding(23, 0, 3, 0);
            this.LabelRunAfterTokDef.Name = "LabelRunAfterTokDef";
            this.LabelRunAfterTokDef.Size = new System.Drawing.Size(223, 15);
            this.LabelRunAfterTokDef.TabIndex = 15;
            this.LabelRunAfterTokDef.Text = "%file% = full path to the downloaded file";
            // 
            // CheckRunOnStartup
            // 
            this.CheckRunOnStartup.AutoSize = true;
            this.CheckRunOnStartup.Location = new System.Drawing.Point(12, 12);
            this.CheckRunOnStartup.Name = "CheckRunOnStartup";
            this.CheckRunOnStartup.Size = new System.Drawing.Size(259, 19);
            this.CheckRunOnStartup.TabIndex = 3;
            this.CheckRunOnStartup.Text = "Run Radio Downloader on computer &startup";
            this.CheckRunOnStartup.UseVisualStyleBackColor = true;
            // 
            // CheckCloseToSystray
            // 
            this.CheckCloseToSystray.AutoSize = true;
            this.CheckCloseToSystray.Location = new System.Drawing.Point(12, 37);
            this.CheckCloseToSystray.Margin = new System.Windows.Forms.Padding(3, 3, 3, 11);
            this.CheckCloseToSystray.Name = "CheckCloseToSystray";
            this.CheckCloseToSystray.Size = new System.Drawing.Size(449, 19);
            this.CheckCloseToSystray.TabIndex = 4;
            this.CheckCloseToSystray.Text = "&Minimise to notification area instead of taskbar button on close (requires resta" +
    "rt)";
            this.CheckCloseToSystray.UseVisualStyleBackColor = true;
            // 
            // NumberParallel
            // 
            this.NumberParallel.Location = new System.Drawing.Point(12, 85);
            this.NumberParallel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 11);
            this.NumberParallel.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumberParallel.Name = "NumberParallel";
            this.NumberParallel.Size = new System.Drawing.Size(75, 23);
            this.NumberParallel.TabIndex = 6;
            this.NumberParallel.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // Preferences
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(480, 358);
            this.Controls.Add(LabelParallel);
            this.Controls.Add(this.NumberParallel);
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
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.KeyPreview = true;
            this.Location = new System.Drawing.Point(3, 29);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Preferences";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Main Options";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.Preferences_HelpButtonClicked);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Preferences_FormClosing);
            this.Load += new System.EventHandler(this.Preferences_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Preferences_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.NumberParallel)).EndInit();
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
        private System.Windows.Forms.NumericUpDown NumberParallel;

        #endregion
    }
}
