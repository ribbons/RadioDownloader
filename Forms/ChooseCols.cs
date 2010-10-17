using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
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


using System.Collections.Generic;
using System.Globalization;
namespace RadioDld
{

    internal partial class ChooseCols : Form
	{
		private List<int> columnOrder;

		private Dictionary<int, string> columnNames;
		public string Columns {
			get {
				string[] stringCols = new string[columnOrder.Count];

				for (int column = 0; column <= columnOrder.Count - 1; column++) {
					stringCols[column] = columnOrder[column].ToString(CultureInfo.InvariantCulture);
				}

				return Strings.Join(stringCols, ",");
			}
			set {
				columnOrder = new List<int>();

				if (value != string.Empty) {
					string[] stringCols = Strings.Split(value, ",");

					foreach (string column in stringCols) {
						columnOrder.Add(Convert.ToInt32(column));
					}
				}
			}
		}

		public void StoreNameList(Dictionary<int, string> columnNames)
		{
			this.columnNames = columnNames;
		}

		private void ChooseCols_Load(System.Object sender, System.EventArgs e)
		{
			if (columnOrder == null) {
				throw new InvalidOperationException("Column order is not set");
			} else if (columnNames == null) {
				throw new InvalidOperationException("Column names are not set");
			}

			this.Font = SystemFonts.MessageBoxFont;

			// Add the current columns to the top of the list, in order
			foreach (int column in columnOrder) {
				ListViewItem addCol = new ListViewItem(columnNames[column]);
				addCol.Name = column.ToString(CultureInfo.InvariantCulture);
				addCol.Checked = true;

				ColumnsList.Items.Add(addCol);
			}

			// Add the rest of the columns to the list in their defined order
			foreach (int column in columnNames.Keys) {
				if (columnOrder.Contains(column) == false) {
					ListViewItem addCol = new ListViewItem(columnNames[column]);
					addCol.Name = column.ToString(CultureInfo.InvariantCulture);
					addCol.Checked = false;

					ColumnsList.Items.Add(addCol);
				}
			}
		}

		private void Okay_Click(object sender, System.EventArgs e)
		{
			columnOrder.Clear();

			foreach (ListViewItem item in ColumnsList.Items) {
				if (item.Checked) {
					columnOrder.Add(int.Parse(item.Name, CultureInfo.InvariantCulture));
				}
			}
		}

		private void ColumnsList_ItemChecked(object sender, System.Windows.Forms.ItemCheckedEventArgs e)
		{
			UpdateButtonState();
		}

		private void ColumnsList_SelectedIndexChanged(System.Object sender, System.EventArgs e)
		{
			UpdateButtonState();
		}

		private void UpdateButtonState()
		{
			if (ColumnsList.SelectedItems.Count == 0) {
				MoveUp.Enabled = false;
				MoveDown.Enabled = false;
				ShowButton.Enabled = false;
				HideButton.Enabled = false;
			} else {
				if (ColumnsList.SelectedItems[0].Index == 0) {
					MoveUp.Enabled = false;
				} else {
					MoveUp.Enabled = true;
				}

				if (ColumnsList.SelectedItems[0].Index == ColumnsList.Items.Count - 1) {
					MoveDown.Enabled = false;
				} else {
					MoveDown.Enabled = true;
				}

				if (ColumnsList.SelectedItems[0].Checked) {
					ShowButton.Enabled = false;
					HideButton.Enabled = true;
				} else {
					ShowButton.Enabled = true;
					HideButton.Enabled = false;
				}
			}
		}

		private void ShowButton_Click(System.Object sender, System.EventArgs e)
		{
			ColumnsList.SelectedItems[0].Checked = true;
		}

		private void HideButton_Click(System.Object sender, System.EventArgs e)
		{
			ColumnsList.SelectedItems[0].Checked = false;
		}

		private void MoveUp_Click(System.Object sender, System.EventArgs e)
		{
			ListViewItem moveItem = ColumnsList.SelectedItems[0];
			int origIndex = ColumnsList.SelectedItems[0].Index;

			ColumnsList.Items.Remove(moveItem);
			ColumnsList.Items.Insert(origIndex - 1, moveItem);
		}

		private void MoveDown_Click(System.Object sender, System.EventArgs e)
		{
			ListViewItem moveItem = ColumnsList.SelectedItems[0];
			int origIndex = ColumnsList.SelectedItems[0].Index;

			ColumnsList.Items.Remove(moveItem);
			ColumnsList.Items.Insert(origIndex + 1, moveItem);
		}
		public ChooseCols()
		{
			InitializeComponent();
		}
	}
}
