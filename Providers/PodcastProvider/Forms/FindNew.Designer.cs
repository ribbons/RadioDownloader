namespace PodcastProvider
{
    partial class FindNew
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
            this.pnlFindNew = new System.Windows.Forms.Panel();
            this.lblResult = new System.Windows.Forms.Label();
            this.lblInstructions = new System.Windows.Forms.Label();
            this.cmdViewEps = new System.Windows.Forms.Button();
            this.txtFeedURL = new System.Windows.Forms.TextBox();
            this.pnlFindNew.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlFindNew
            // 
            this.pnlFindNew.Controls.Add(this.lblResult);
            this.pnlFindNew.Controls.Add(this.lblInstructions);
            this.pnlFindNew.Controls.Add(this.cmdViewEps);
            this.pnlFindNew.Controls.Add(this.txtFeedURL);
            this.pnlFindNew.Location = new System.Drawing.Point(0, 0);
            this.pnlFindNew.Name = "pnlFindNew";
            this.pnlFindNew.Size = new System.Drawing.Size(570, 416);
            this.pnlFindNew.TabIndex = 0;
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Location = new System.Drawing.Point(36, 108);
            this.lblResult.Margin = new System.Windows.Forms.Padding(4, 20, 3, 0);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(0, 13);
            this.lblResult.TabIndex = 3;
            // 
            // lblInstructions
            // 
            this.lblInstructions.AutoSize = true;
            this.lblInstructions.Location = new System.Drawing.Point(36, 39);
            this.lblInstructions.Margin = new System.Windows.Forms.Padding(4, 0, 3, 10);
            this.lblInstructions.Name = "lblInstructions";
            this.lblInstructions.Size = new System.Drawing.Size(189, 13);
            this.lblInstructions.TabIndex = 0;
            this.lblInstructions.Text = "&Enter the URL of a podcast RSS feed:";
            // 
            // cmdViewEps
            // 
            this.cmdViewEps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdViewEps.Location = new System.Drawing.Point(461, 63);
            this.cmdViewEps.Name = "cmdViewEps";
            this.cmdViewEps.Size = new System.Drawing.Size(73, 23);
            this.cmdViewEps.TabIndex = 2;
            this.cmdViewEps.Text = "&View";
            this.cmdViewEps.UseVisualStyleBackColor = true;
            this.cmdViewEps.Click += new System.EventHandler(this.cmdViewEps_Click);
            // 
            // txtFeedURL
            // 
            this.txtFeedURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFeedURL.Location = new System.Drawing.Point(39, 65);
            this.txtFeedURL.Name = "txtFeedURL";
            this.txtFeedURL.Size = new System.Drawing.Size(416, 20);
            this.txtFeedURL.TabIndex = 1;
            this.txtFeedURL.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtFeedURL_KeyPress);
            // 
            // FindNew
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(570, 416);
            this.Controls.Add(this.pnlFindNew);
            this.Name = "FindNew";
            this.Text = "Find New";
            this.pnlFindNew.ResumeLayout(false);
            this.pnlFindNew.PerformLayout();
            this.ResumeLayout(false);

        }
        internal System.Windows.Forms.Panel pnlFindNew;
        internal System.Windows.Forms.TextBox txtFeedURL;
        internal System.Windows.Forms.Button cmdViewEps;
        internal System.Windows.Forms.Label lblInstructions;
        internal System.Windows.Forms.Label lblResult;

        #endregion
    }
}
