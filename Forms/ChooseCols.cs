/*
 * Copyright © 2010-2012 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    internal partial class ChooseCols : Form
    {
        private List<int> columnOrder;
        private Dictionary<int, string> columnNames;

        public ChooseCols()
        {
            this.InitializeComponent();
        }

        public string Columns
        {
            get
            {
                string[] stringCols = new string[this.columnOrder.Count];

                for (int column = 0; column <= this.columnOrder.Count - 1; column++)
                {
                    stringCols[column] = this.columnOrder[column].ToString(CultureInfo.InvariantCulture);
                }

                return string.Join(",", stringCols);
            }

            set
            {
                this.columnOrder = new List<int>();

                if (!string.IsNullOrEmpty(value))
                {
                    foreach (string column in value.Split(','))
                    {
                        this.columnOrder.Add(Convert.ToInt32(column, CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        public void StoreNameList(Dictionary<int, string> columnNames)
        {
            this.columnNames = columnNames;
        }

        private void ChooseCols_Load(object sender, EventArgs e)
        {
            if (this.columnOrder == null)
            {
                throw new InvalidOperationException("Column order is not set");
            }
            else if (this.columnNames == null)
            {
                throw new InvalidOperationException("Column names are not set");
            }

            this.Font = SystemFonts.MessageBoxFont;

            // Add the current columns to the top of the list, in order
            foreach (int column in this.columnOrder)
            {
                ListViewItem addCol = new ListViewItem(this.columnNames[column]);
                addCol.Name = column.ToString(CultureInfo.InvariantCulture);
                addCol.Checked = true;

                this.ListColumns.Items.Add(addCol);
            }

            // Add the rest of the columns to the list in their defined order
            foreach (int column in this.columnNames.Keys)
            {
                if (!this.columnOrder.Contains(column))
                {
                    ListViewItem addCol = new ListViewItem(this.columnNames[column]);
                    addCol.Name = column.ToString(CultureInfo.InvariantCulture);
                    addCol.Checked = false;

                    this.ListColumns.Items.Add(addCol);
                }
            }
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            this.columnOrder.Clear();

            foreach (ListViewItem item in this.ListColumns.Items)
            {
                if (item.Checked)
                {
                    this.columnOrder.Add(int.Parse(item.Name, CultureInfo.InvariantCulture));
                }
            }
        }

        private void ListColumns_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            this.UpdateButtonState();
        }

        private void ListColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            if (this.ListColumns.SelectedItems.Count == 0)
            {
                this.ButtonMoveUp.Enabled = false;
                this.ButtonMoveDown.Enabled = false;
                this.ButtonShow.Enabled = false;
                this.ButtonHide.Enabled = false;
            }
            else
            {
                if (this.ListColumns.SelectedItems[0].Index == 0)
                {
                    this.ButtonMoveUp.Enabled = false;
                }
                else
                {
                    this.ButtonMoveUp.Enabled = true;
                }

                if (this.ListColumns.SelectedItems[0].Index == this.ListColumns.Items.Count - 1)
                {
                    this.ButtonMoveDown.Enabled = false;
                }
                else
                {
                    this.ButtonMoveDown.Enabled = true;
                }

                if (this.ListColumns.SelectedItems[0].Checked)
                {
                    this.ButtonShow.Enabled = false;
                    this.ButtonHide.Enabled = true;
                }
                else
                {
                    this.ButtonShow.Enabled = true;
                    this.ButtonHide.Enabled = false;
                }
            }
        }

        private void ButtonShow_Click(object sender, EventArgs e)
        {
            this.ListColumns.SelectedItems[0].Checked = true;
        }

        private void ButtonHide_Click(object sender, EventArgs e)
        {
            this.ListColumns.SelectedItems[0].Checked = false;
        }

        private void ButtonMoveUp_Click(object sender, EventArgs e)
        {
            ListViewItem moveItem = this.ListColumns.SelectedItems[0];
            int origIndex = this.ListColumns.SelectedItems[0].Index;

            this.ListColumns.Items.Remove(moveItem);
            this.ListColumns.Items.Insert(origIndex - 1, moveItem);
        }

        private void ButtonMoveDown_Click(object sender, EventArgs e)
        {
            ListViewItem moveItem = this.ListColumns.SelectedItems[0];
            int origIndex = this.ListColumns.SelectedItems[0].Index;

            this.ListColumns.Items.Remove(moveItem);
            this.ListColumns.Items.Insert(origIndex + 1, moveItem);
        }
    }
}
