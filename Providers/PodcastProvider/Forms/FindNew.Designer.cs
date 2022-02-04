// Disable warnings in the generated code
#if !__MonoCS__
#pragma warning disable IDE0001 // Simplify Names
#pragma warning disable IDE0002 // Simplify Member Access
#pragma warning disable IDE0004 // Remove Unnecessary Cast
#endif

namespace PodcastProvider
{
    internal partial class FindNew
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
            this.PanelFindNew = new System.Windows.Forms.Panel();
            this.LabelResult = new System.Windows.Forms.Label();
            this.LabelFeedUrl = new System.Windows.Forms.Label();
            this.ButtonView = new System.Windows.Forms.Button();
            this.TextFeedUrl = new System.Windows.Forms.TextBox();
            this.PanelFindNew.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelFindNew
            // 
            this.PanelFindNew.Controls.Add(this.LabelResult);
            this.PanelFindNew.Controls.Add(this.LabelFeedUrl);
            this.PanelFindNew.Controls.Add(this.ButtonView);
            this.PanelFindNew.Controls.Add(this.TextFeedUrl);
            this.PanelFindNew.Location = new System.Drawing.Point(0, 0);
            this.PanelFindNew.Name = "PanelFindNew";
            this.PanelFindNew.Size = new System.Drawing.Size(570, 416);
            this.PanelFindNew.TabIndex = 0;
            // 
            // LabelResult
            // 
            this.LabelResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelResult.Location = new System.Drawing.Point(35, 108);
            this.LabelResult.Margin = new System.Windows.Forms.Padding(4, 20, 3, 0);
            this.LabelResult.Name = "LabelResult";
            this.LabelResult.Size = new System.Drawing.Size(500, 75);
            this.LabelResult.TabIndex = 3;
            // 
            // LabelFeedUrl
            // 
            this.LabelFeedUrl.AutoSize = true;
            this.LabelFeedUrl.Location = new System.Drawing.Point(35, 49);
            this.LabelFeedUrl.Name = "LabelFeedUrl";
            this.LabelFeedUrl.Size = new System.Drawing.Size(189, 13);
            this.LabelFeedUrl.TabIndex = 0;
            this.LabelFeedUrl.Text = "&Enter the URL of a podcast RSS feed:";
            // 
            // ButtonView
            // 
            this.ButtonView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonView.Location = new System.Drawing.Point(460, 63);
            this.ButtonView.Name = "ButtonView";
            this.ButtonView.Size = new System.Drawing.Size(75, 23);
            this.ButtonView.TabIndex = 2;
            this.ButtonView.Text = "&View";
            this.ButtonView.UseVisualStyleBackColor = true;
            this.ButtonView.Click += new System.EventHandler(this.ButtonView_Click);
            // 
            // TextFeedUrl
            // 
            this.TextFeedUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextFeedUrl.Location = new System.Drawing.Point(38, 65);
            this.TextFeedUrl.Name = "TextFeedUrl";
            this.TextFeedUrl.Size = new System.Drawing.Size(416, 20);
            this.TextFeedUrl.TabIndex = 1;
            this.TextFeedUrl.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextFeedUrl_KeyPress);
            // 
            // FindNew
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(570, 416);
            this.Controls.Add(this.PanelFindNew);
            this.Name = "FindNew";
            this.Text = "Find New";
            this.PanelFindNew.ResumeLayout(false);
            this.PanelFindNew.PerformLayout();
            this.ResumeLayout(false);

        }
        internal System.Windows.Forms.Panel PanelFindNew;
        private System.Windows.Forms.TextBox TextFeedUrl;
        private System.Windows.Forms.Button ButtonView;
        private System.Windows.Forms.Label LabelFeedUrl;
        private System.Windows.Forms.Label LabelResult;

        #endregion
    }
}
