namespace RadioDld
{
    partial class UpdateNotify
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateNotify));
            this.uxMessage = new System.Windows.Forms.Label();
            this.uxYes = new System.Windows.Forms.Button();
            this.uxNo = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // uxMessage
            // 
            this.uxMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.uxMessage.Location = new System.Drawing.Point(12, 12);
            this.uxMessage.Margin = new System.Windows.Forms.Padding(3);
            this.uxMessage.Name = "uxMessage";
            this.uxMessage.Size = new System.Drawing.Size(355, 108);
            this.uxMessage.TabIndex = 2;
            this.uxMessage.Text = resources.GetString("uxMessage.Text");
            // 
            // uxYes
            // 
            this.uxYes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.uxYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.uxYes.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.uxYes.Location = new System.Drawing.Point(207, 126);
            this.uxYes.Name = "uxYes";
            this.uxYes.Size = new System.Drawing.Size(77, 25);
            this.uxYes.TabIndex = 0;
            this.uxYes.Text = "Yes";
            this.uxYes.UseVisualStyleBackColor = true;
            // 
            // uxNo
            // 
            this.uxNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.uxNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.uxNo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.uxNo.Location = new System.Drawing.Point(290, 126);
            this.uxNo.Name = "uxNo";
            this.uxNo.Size = new System.Drawing.Size(77, 25);
            this.uxNo.TabIndex = 1;
            this.uxNo.Text = "No";
            this.uxNo.UseVisualStyleBackColor = true;
            // 
            // UpdateNotify
            // 
            this.AcceptButton = this.uxYes;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.uxNo;
            this.ClientSize = new System.Drawing.Size(379, 163);
            this.Controls.Add(this.uxNo);
            this.Controls.Add(this.uxYes);
            this.Controls.Add(this.uxMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateNotify";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Radio Downloader";
            this.Load += new System.EventHandler(this.Update_Load);
            this.ResumeLayout(false);

        }
        internal System.Windows.Forms.Label uxMessage;
        internal System.Windows.Forms.Button uxYes;
        internal System.Windows.Forms.Button uxNo;

        #endregion
    }
}
