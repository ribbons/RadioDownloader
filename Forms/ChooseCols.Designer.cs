namespace RadioDld
{
    partial class ChooseCols
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
            this.HideButton = new System.Windows.Forms.Button();
            this.ShowButton = new System.Windows.Forms.Button();
            this.MoveDown = new System.Windows.Forms.Button();
            this.MoveUp = new System.Windows.Forms.Button();
            this.Okay = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.ColumnsList = new System.Windows.Forms.ListView();
            this.InfoLabel = new System.Windows.Forms.Label();
            this.ColumnsLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // HideButton
            // 
            this.HideButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.HideButton.Enabled = false;
            this.HideButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.HideButton.Location = new System.Drawing.Point(251, 157);
            this.HideButton.Name = "HideButton";
            this.HideButton.Size = new System.Drawing.Size(77, 25);
            this.HideButton.TabIndex = 8;
            this.HideButton.Text = "&Hide";
            this.HideButton.UseVisualStyleBackColor = true;
            this.HideButton.Click += new System.EventHandler(this.HideButton_Click);
            // 
            // ShowButton
            // 
            this.ShowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowButton.Enabled = false;
            this.ShowButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ShowButton.Location = new System.Drawing.Point(251, 126);
            this.ShowButton.Name = "ShowButton";
            this.ShowButton.Size = new System.Drawing.Size(77, 25);
            this.ShowButton.TabIndex = 7;
            this.ShowButton.Text = "&Show";
            this.ShowButton.UseVisualStyleBackColor = true;
            this.ShowButton.Click += new System.EventHandler(this.ShowButton_Click);
            // 
            // MoveDown
            // 
            this.MoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MoveDown.Enabled = false;
            this.MoveDown.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.MoveDown.Location = new System.Drawing.Point(251, 95);
            this.MoveDown.Name = "MoveDown";
            this.MoveDown.Size = new System.Drawing.Size(77, 25);
            this.MoveDown.TabIndex = 6;
            this.MoveDown.Text = "Move &Down";
            this.MoveDown.UseVisualStyleBackColor = true;
            this.MoveDown.Click += new System.EventHandler(this.MoveDown_Click);
            // 
            // MoveUp
            // 
            this.MoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MoveUp.Enabled = false;
            this.MoveUp.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.MoveUp.Location = new System.Drawing.Point(251, 64);
            this.MoveUp.Name = "MoveUp";
            this.MoveUp.Size = new System.Drawing.Size(77, 25);
            this.MoveUp.TabIndex = 5;
            this.MoveUp.Text = "Move &Up";
            this.MoveUp.UseVisualStyleBackColor = true;
            this.MoveUp.Click += new System.EventHandler(this.MoveUp_Click);
            // 
            // Okay
            // 
            this.Okay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Okay.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Okay.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.Okay.Location = new System.Drawing.Point(168, 286);
            this.Okay.Name = "Okay";
            this.Okay.Size = new System.Drawing.Size(77, 25);
            this.Okay.TabIndex = 0;
            this.Okay.Text = "OK";
            this.Okay.UseVisualStyleBackColor = true;
            this.Okay.Click += new System.EventHandler(this.Okay_Click);
            // 
            // Cancel
            // 
            this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.Cancel.Location = new System.Drawing.Point(251, 286);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(77, 25);
            this.Cancel.TabIndex = 1;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // ColumnsList
            // 
            this.ColumnsList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ColumnsList.CheckBoxes = true;
            this.ColumnsList.HideSelection = false;
            this.ColumnsList.Location = new System.Drawing.Point(12, 64);
            this.ColumnsList.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            this.ColumnsList.Name = "ColumnsList";
            this.ColumnsList.Size = new System.Drawing.Size(230, 199);
            this.ColumnsList.TabIndex = 4;
            this.ColumnsList.UseCompatibleStateImageBehavior = false;
            this.ColumnsList.View = System.Windows.Forms.View.List;
            this.ColumnsList.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.ColumnsList_ItemChecked);
            this.ColumnsList.Click += new System.EventHandler(this.ColumnsList_SelectedIndexChanged);
            // 
            // InfoLabel
            // 
            this.InfoLabel.Location = new System.Drawing.Point(9, 11);
            this.InfoLabel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 0);
            this.InfoLabel.Name = "InfoLabel";
            this.InfoLabel.Size = new System.Drawing.Size(315, 35);
            this.InfoLabel.TabIndex = 2;
            this.InfoLabel.Text = "Select the columns you want to display for this list.";
            // 
            // ColumnsLabel
            // 
            this.ColumnsLabel.AutoSize = true;
            this.ColumnsLabel.Location = new System.Drawing.Point(9, 46);
            this.ColumnsLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 2);
            this.ColumnsLabel.Name = "ColumnsLabel";
            this.ColumnsLabel.Size = new System.Drawing.Size(50, 13);
            this.ColumnsLabel.TabIndex = 3;
            this.ColumnsLabel.Text = "&Columns:";
            // 
            // ChooseCols
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(340, 321);
            this.Controls.Add(this.ColumnsLabel);
            this.Controls.Add(this.InfoLabel);
            this.Controls.Add(this.ColumnsList);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Okay);
            this.Controls.Add(this.MoveUp);
            this.Controls.Add(this.MoveDown);
            this.Controls.Add(this.ShowButton);
            this.Controls.Add(this.HideButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChooseCols";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose Columns";
            this.Load += new System.EventHandler(this.ChooseCols_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        internal System.Windows.Forms.Button HideButton;
        internal System.Windows.Forms.Button ShowButton;
        internal System.Windows.Forms.Button MoveDown;
        internal System.Windows.Forms.Button MoveUp;
        internal System.Windows.Forms.Button Okay;
        internal System.Windows.Forms.Button Cancel;
        internal System.Windows.Forms.ListView ColumnsList;
        internal System.Windows.Forms.Label InfoLabel;
        internal System.Windows.Forms.Label ColumnsLabel;

        #endregion
    }
}
