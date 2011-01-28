/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2011 Matt Robinson
 * 
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
 * implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
 * License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with this program.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

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

                this.ListColumns.Items.Add(addCol);
            }

            // Add the rest of the columns to the list in their defined order
            foreach (int column in this.columnNames.Keys)
            {
                if (this.columnOrder.Contains(column) == false)
                {
                    ListViewItem addCol = new ListViewItem(this.columnNames[column]);
                    addCol.Name = column.ToString(CultureInfo.InvariantCulture);
                    addCol.Checked = false;

                    this.ListColumns.Items.Add(addCol);
                }
            }
        }

        private void ButtonOk_Click(object sender, System.EventArgs e)
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

        private void ListColumns_ItemChecked(object sender, System.Windows.Forms.ItemCheckedEventArgs e)
        {
            this.UpdateButtonState();
        }

        private void ListColumns_SelectedIndexChanged(object sender, System.EventArgs e)
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

        private void ButtonShow_Click(object sender, System.EventArgs e)
        {
            this.ListColumns.SelectedItems[0].Checked = true;
        }

        private void ButtonHide_Click(object sender, System.EventArgs e)
        {
            this.ListColumns.SelectedItems[0].Checked = false;
        }

        private void ButtonMoveUp_Click(object sender, System.EventArgs e)
        {
            ListViewItem moveItem = this.ListColumns.SelectedItems[0];
            int origIndex = this.ListColumns.SelectedItems[0].Index;

            this.ListColumns.Items.Remove(moveItem);
            this.ListColumns.Items.Insert(origIndex - 1, moveItem);
        }

        private void ButtonMoveDown_Click(object sender, System.EventArgs e)
        {
            ListViewItem moveItem = this.ListColumns.SelectedItems[0];
            int origIndex = this.ListColumns.SelectedItems[0].Index;

            this.ListColumns.Items.Remove(moveItem);
            this.ListColumns.Items.Insert(origIndex + 1, moveItem);
        }
    }
}
