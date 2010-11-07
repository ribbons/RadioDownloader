// Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
// Copyright © 2007-2010 Matt Robinson
//
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General
// Public License as published by the Free Software Foundation; either version 2 of the License, or (at your
// option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
// implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
// License for more details.
//
// You should have received a copy of the GNU General Public License along with this program; if not, write
// to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

namespace RadioDld
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;
    using Microsoft.VisualBasic;

    internal partial class ChooseCols : Form
    {
        private List<int> columnOrder;
        private Dictionary<int, string> columnNames;

        public string Columns
        {
            get
            {
                string[] stringCols = new string[this.columnOrder.Count];

                for (int column = 0; column <= this.columnOrder.Count - 1; column++)
                {
                    stringCols[column] = this.columnOrder[column].ToString(CultureInfo.InvariantCulture);
                }

                return Strings.Join(stringCols, ",");
            }

            set
            {
                this.columnOrder = new List<int>();

                if (!string.IsNullOrEmpty(value))
                {
                    string[] stringCols = Strings.Split(value, ",");

                    foreach (string column in stringCols)
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

        private void ChooseCols_Load(object sender, System.EventArgs e)
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

                this.ColumnsList.Items.Add(addCol);
            }

            // Add the rest of the columns to the list in their defined order
            foreach (int column in this.columnNames.Keys)
            {
                if (this.columnOrder.Contains(column) == false)
                {
                    ListViewItem addCol = new ListViewItem(this.columnNames[column]);
                    addCol.Name = column.ToString(CultureInfo.InvariantCulture);
                    addCol.Checked = false;

                    this.ColumnsList.Items.Add(addCol);
                }
            }
        }

        private void Okay_Click(object sender, System.EventArgs e)
        {
            this.columnOrder.Clear();

            foreach (ListViewItem item in this.ColumnsList.Items)
            {
                if (item.Checked)
                {
                    this.columnOrder.Add(int.Parse(item.Name, CultureInfo.InvariantCulture));
                }
            }
        }

        private void ColumnsList_ItemChecked(object sender, System.Windows.Forms.ItemCheckedEventArgs e)
        {
            this.UpdateButtonState();
        }

        private void ColumnsList_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            this.UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            if (this.ColumnsList.SelectedItems.Count == 0)
            {
                this.MoveUp.Enabled = false;
                this.MoveDown.Enabled = false;
                this.ShowButton.Enabled = false;
                this.HideButton.Enabled = false;
            }
            else
            {
                if (this.ColumnsList.SelectedItems[0].Index == 0)
                {
                    this.MoveUp.Enabled = false;
                }
                else
                {
                    this.MoveUp.Enabled = true;
                }

                if (this.ColumnsList.SelectedItems[0].Index == this.ColumnsList.Items.Count - 1)
                {
                    this.MoveDown.Enabled = false;
                }
                else
                {
                    this.MoveDown.Enabled = true;
                }

                if (this.ColumnsList.SelectedItems[0].Checked)
                {
                    this.ShowButton.Enabled = false;
                    this.HideButton.Enabled = true;
                }
                else
                {
                    this.ShowButton.Enabled = true;
                    this.HideButton.Enabled = false;
                }
            }
        }

        private void ShowButton_Click(object sender, System.EventArgs e)
        {
            this.ColumnsList.SelectedItems[0].Checked = true;
        }

        private void HideButton_Click(object sender, System.EventArgs e)
        {
            this.ColumnsList.SelectedItems[0].Checked = false;
        }

        private void MoveUp_Click(object sender, System.EventArgs e)
        {
            ListViewItem moveItem = this.ColumnsList.SelectedItems[0];
            int origIndex = this.ColumnsList.SelectedItems[0].Index;

            this.ColumnsList.Items.Remove(moveItem);
            this.ColumnsList.Items.Insert(origIndex - 1, moveItem);
        }

        private void MoveDown_Click(object sender, System.EventArgs e)
        {
            ListViewItem moveItem = this.ColumnsList.SelectedItems[0];
            int origIndex = this.ColumnsList.SelectedItems[0].Index;

            this.ColumnsList.Items.Remove(moveItem);
            this.ColumnsList.Items.Insert(origIndex + 1, moveItem);
        }

        public ChooseCols()
        {
            this.InitializeComponent();
        }
    }
}
