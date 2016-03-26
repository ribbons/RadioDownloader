// Disable warnings in the generated code
#if !__MonoCS__
#pragma warning disable IDE0001 // Simplify Names
#pragma warning disable IDE0002 // Simplify Member Access
#pragma warning disable IDE0004 // Remove Unnecessary Cast
#endif

namespace PodcastProvider
{
    partial class MoreProgInfo
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
            this.ButtonOk = new System.Windows.Forms.Button();
            this.LabelFeedUrl = new System.Windows.Forms.Label();
            this.TextFeedUrl = new System.Windows.Forms.TextBox();
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
            this.ButtonOk.Location = new System.Drawing.Point(259, 83);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ButtonOk.Size = new System.Drawing.Size(75, 23);
            this.ButtonOk.TabIndex = 1;
            this.ButtonOk.Text = "OK";
            this.ButtonOk.UseVisualStyleBackColor = false;
            // 
            // LabelFeedUrl
            // 
            this.LabelFeedUrl.AutoSize = true;
            this.LabelFeedUrl.Location = new System.Drawing.Point(13, 13);
            this.LabelFeedUrl.Name = "LabelFeedUrl";
            this.LabelFeedUrl.Size = new System.Drawing.Size(59, 13);
            this.LabelFeedUrl.TabIndex = 2;
            this.LabelFeedUrl.Text = "Feed URL:";
            // 
            // TextFeedUrl
            // 
            this.TextFeedUrl.Location = new System.Drawing.Point(13, 30);
            this.TextFeedUrl.Name = "TextFeedUrl";
            this.TextFeedUrl.ReadOnly = true;
            this.TextFeedUrl.Size = new System.Drawing.Size(321, 20);
            this.TextFeedUrl.TabIndex = 3;
            // 
            // MoreProgInfo
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 118);
            this.Controls.Add(this.TextFeedUrl);
            this.Controls.Add(this.LabelFeedUrl);
            this.Controls.Add(this.ButtonOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MoreProgInfo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "More Programme Info";
            this.Load += new System.EventHandler(this.MoreProgInfo_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.Label LabelFeedUrl;
        private System.Windows.Forms.TextBox TextFeedUrl;

        #endregion
    }
}
