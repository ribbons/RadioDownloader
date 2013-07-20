namespace RadioDld
{
    internal partial class UpdateNotify
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
            System.Windows.Forms.Label LabelMessage;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateNotify));
            System.Windows.Forms.Button ButtonYes;
            System.Windows.Forms.Button ButtonNo;
            LabelMessage = new System.Windows.Forms.Label();
            ButtonYes = new System.Windows.Forms.Button();
            ButtonNo = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LabelMessage
            // 
            LabelMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            LabelMessage.Location = new System.Drawing.Point(12, 12);
            LabelMessage.Margin = new System.Windows.Forms.Padding(3);
            LabelMessage.Name = "LabelMessage";
            LabelMessage.Size = new System.Drawing.Size(408, 160);
            LabelMessage.TabIndex = 2;
            LabelMessage.Text = resources.GetString("LabelMessage.Text");
            // 
            // ButtonYes
            // 
            ButtonYes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            ButtonYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            ButtonYes.FlatStyle = System.Windows.Forms.FlatStyle.System;
            ButtonYes.Location = new System.Drawing.Point(264, 178);
            ButtonYes.Name = "ButtonYes";
            ButtonYes.Size = new System.Drawing.Size(75, 23);
            ButtonYes.TabIndex = 0;
            ButtonYes.Text = "Yes";
            ButtonYes.UseVisualStyleBackColor = true;
            // 
            // ButtonNo
            // 
            ButtonNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            ButtonNo.DialogResult = System.Windows.Forms.DialogResult.No;
            ButtonNo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            ButtonNo.Location = new System.Drawing.Point(345, 178);
            ButtonNo.Name = "ButtonNo";
            ButtonNo.Size = new System.Drawing.Size(75, 23);
            ButtonNo.TabIndex = 1;
            ButtonNo.Text = "No";
            ButtonNo.UseVisualStyleBackColor = true;
            // 
            // UpdateNotify
            // 
            this.AcceptButton = ButtonYes;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = ButtonNo;
            this.ClientSize = new System.Drawing.Size(432, 213);
            this.Controls.Add(ButtonNo);
            this.Controls.Add(ButtonYes);
            this.Controls.Add(LabelMessage);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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

        #endregion
    }
}
