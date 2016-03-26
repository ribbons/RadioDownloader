// Disable warnings in the generated code
#if !__MonoCS__
#pragma warning disable IDE0001 // Simplify Names
#pragma warning disable IDE0002 // Simplify Member Access
#pragma warning disable IDE0004 // Remove Unnecessary Cast
#endif

namespace RadioDld
{
    internal partial class ChooseCols
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
            System.Windows.Forms.Button ButtonCancel;
            System.Windows.Forms.Label LabelInfo;
            System.Windows.Forms.Label LabelColumns;
            this.ButtonHide = new System.Windows.Forms.Button();
            this.ButtonShow = new System.Windows.Forms.Button();
            this.ButtonMoveDown = new System.Windows.Forms.Button();
            this.ButtonMoveUp = new System.Windows.Forms.Button();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.ListColumns = new System.Windows.Forms.ListView();
            ButtonCancel = new System.Windows.Forms.Button();
            LabelInfo = new System.Windows.Forms.Label();
            LabelColumns = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ButtonCancel
            // 
            ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            ButtonCancel.Location = new System.Drawing.Point(253, 286);
            ButtonCancel.Name = "ButtonCancel";
            ButtonCancel.Size = new System.Drawing.Size(75, 23);
            ButtonCancel.TabIndex = 1;
            ButtonCancel.Text = "Cancel";
            ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // LabelInfo
            // 
            LabelInfo.Location = new System.Drawing.Point(9, 11);
            LabelInfo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 0);
            LabelInfo.Name = "LabelInfo";
            LabelInfo.Size = new System.Drawing.Size(315, 35);
            LabelInfo.TabIndex = 2;
            LabelInfo.Text = "Select the columns you want to display for this list.";
            // 
            // LabelColumns
            // 
            LabelColumns.AutoSize = true;
            LabelColumns.Location = new System.Drawing.Point(9, 46);
            LabelColumns.Margin = new System.Windows.Forms.Padding(3, 0, 3, 2);
            LabelColumns.Name = "LabelColumns";
            LabelColumns.Size = new System.Drawing.Size(58, 15);
            LabelColumns.TabIndex = 3;
            LabelColumns.Text = "&Columns:";
            // 
            // ButtonHide
            // 
            this.ButtonHide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonHide.Enabled = false;
            this.ButtonHide.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonHide.Location = new System.Drawing.Point(253, 151);
            this.ButtonHide.Name = "ButtonHide";
            this.ButtonHide.Size = new System.Drawing.Size(75, 23);
            this.ButtonHide.TabIndex = 8;
            this.ButtonHide.Text = "&Hide";
            this.ButtonHide.UseVisualStyleBackColor = true;
            this.ButtonHide.Click += new System.EventHandler(this.ButtonHide_Click);
            // 
            // ButtonShow
            // 
            this.ButtonShow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonShow.Enabled = false;
            this.ButtonShow.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonShow.Location = new System.Drawing.Point(253, 122);
            this.ButtonShow.Name = "ButtonShow";
            this.ButtonShow.Size = new System.Drawing.Size(75, 23);
            this.ButtonShow.TabIndex = 7;
            this.ButtonShow.Text = "&Show";
            this.ButtonShow.UseVisualStyleBackColor = true;
            this.ButtonShow.Click += new System.EventHandler(this.ButtonShow_Click);
            // 
            // ButtonMoveDown
            // 
            this.ButtonMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonMoveDown.Enabled = false;
            this.ButtonMoveDown.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonMoveDown.Location = new System.Drawing.Point(253, 93);
            this.ButtonMoveDown.Name = "ButtonMoveDown";
            this.ButtonMoveDown.Size = new System.Drawing.Size(75, 23);
            this.ButtonMoveDown.TabIndex = 6;
            this.ButtonMoveDown.Text = "Move &Down";
            this.ButtonMoveDown.UseVisualStyleBackColor = true;
            this.ButtonMoveDown.Click += new System.EventHandler(this.ButtonMoveDown_Click);
            // 
            // ButtonMoveUp
            // 
            this.ButtonMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonMoveUp.Enabled = false;
            this.ButtonMoveUp.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonMoveUp.Location = new System.Drawing.Point(253, 64);
            this.ButtonMoveUp.Name = "ButtonMoveUp";
            this.ButtonMoveUp.Size = new System.Drawing.Size(75, 23);
            this.ButtonMoveUp.TabIndex = 5;
            this.ButtonMoveUp.Text = "Move &Up";
            this.ButtonMoveUp.UseVisualStyleBackColor = true;
            this.ButtonMoveUp.Click += new System.EventHandler(this.ButtonMoveUp_Click);
            // 
            // ButtonOk
            // 
            this.ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOk.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ButtonOk.Location = new System.Drawing.Point(172, 286);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(75, 23);
            this.ButtonOk.TabIndex = 0;
            this.ButtonOk.Text = "OK";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // ListColumns
            // 
            this.ListColumns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ListColumns.CheckBoxes = true;
            this.ListColumns.HideSelection = false;
            this.ListColumns.Location = new System.Drawing.Point(12, 64);
            this.ListColumns.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            this.ListColumns.Name = "ListColumns";
            this.ListColumns.Size = new System.Drawing.Size(232, 199);
            this.ListColumns.TabIndex = 4;
            this.ListColumns.UseCompatibleStateImageBehavior = false;
            this.ListColumns.View = System.Windows.Forms.View.List;
            this.ListColumns.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.ListColumns_ItemChecked);
            this.ListColumns.Click += new System.EventHandler(this.ListColumns_SelectedIndexChanged);
            // 
            // ChooseCols
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = ButtonCancel;
            this.ClientSize = new System.Drawing.Size(340, 321);
            this.Controls.Add(LabelColumns);
            this.Controls.Add(LabelInfo);
            this.Controls.Add(this.ListColumns);
            this.Controls.Add(ButtonCancel);
            this.Controls.Add(this.ButtonOk);
            this.Controls.Add(this.ButtonMoveUp);
            this.Controls.Add(this.ButtonMoveDown);
            this.Controls.Add(this.ButtonShow);
            this.Controls.Add(this.ButtonHide);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
        private System.Windows.Forms.Button ButtonHide;
        private System.Windows.Forms.Button ButtonShow;
        private System.Windows.Forms.Button ButtonMoveDown;
        private System.Windows.Forms.Button ButtonMoveUp;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.ListView ListColumns;

        #endregion
    }
}
