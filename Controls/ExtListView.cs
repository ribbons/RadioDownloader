/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2012 by the authors - see the AUTHORS file for details.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace RadioDld
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class ExtListView : ListView
    {
        private Dictionary<ListViewItem, EmbeddedProgress> embeddedInfo = new Dictionary<ListViewItem, EmbeddedProgress>();
        private bool waitingSelChange = false;

        // Extra events
        public event ColumnClickEventHandler ColumnRightClick;

        public void ShowProgress(ListViewItem item, int column, int percent)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (column >= this.Columns.Count)
            {
                throw new ArgumentOutOfRangeException("column", column, "Value of column must be less than the current number of columns!");
            }

            if (this.embeddedInfo.ContainsKey(item))
            {
                this.embeddedInfo[item].Progress.Value = percent;
            }
            else
            {
                EmbeddedProgress embedded = default(EmbeddedProgress);
                embedded.Progress = new ProgressBar();
                embedded.Progress.Value = percent;
                embedded.Progress.Visible = false;
                embedded.Column = column;

                // Add an event handler to select the ListView row when the progress bar is clicked
                embedded.Progress.Click += this.EmbeddedControl_Click;

                this.embeddedInfo.Add(item, embedded);
                this.Controls.Add(embedded.Progress);
            }
        }

        public void HideProgress(ListViewItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (this.embeddedInfo.ContainsKey(item))
            {
                this.Controls.Remove(this.embeddedInfo[item].Progress);
                this.embeddedInfo.Remove(item);
            }
        }

        public void HideAllProgress()
        {
            foreach (EmbeddedProgress embedded in this.embeddedInfo.Values)
            {
                this.Controls.Remove(embedded.Progress);
            }

            this.embeddedInfo.Clear();
        }

        public void ShowSortOnHeader(int column, SortOrder order)
        {
            IntPtr headersHwnd = NativeMethods.SendMessage(this.Handle, NativeMethods.LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);

            for (int processCols = 0; processCols <= this.Columns.Count; processCols++)
            {
                NativeMethods.HDITEM headerInfo = default(NativeMethods.HDITEM);
                headerInfo.mask = NativeMethods.HDI_FORMAT;

                NativeMethods.SendMessage(headersHwnd, NativeMethods.HDM_GETITEM, (IntPtr)processCols, ref headerInfo);

                if (order != SortOrder.None && processCols == column)
                {
                    switch (order)
                    {
                        case SortOrder.Ascending:
                            headerInfo.fmt = headerInfo.fmt & ~NativeMethods.HDF_SORTDOWN;
                            headerInfo.fmt = headerInfo.fmt | NativeMethods.HDF_SORTUP;
                            break;
                        case SortOrder.Descending:
                            headerInfo.fmt = headerInfo.fmt & ~NativeMethods.HDF_SORTUP;
                            headerInfo.fmt = headerInfo.fmt | NativeMethods.HDF_SORTDOWN;
                            break;
                    }
                }
                else
                {
                    headerInfo.fmt = headerInfo.fmt & ~NativeMethods.HDF_SORTDOWN & ~NativeMethods.HDF_SORTUP;
                }

                NativeMethods.SendMessage(headersHwnd, NativeMethods.HDM_SETITEM, (IntPtr)processCols, ref headerInfo);
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case NativeMethods.WM_CREATE:
                    if (OsUtils.WinXpOrLater())
                    {
                        // Set the theme of the control to "explorer", to give the
                        // correct styling under Vista.  This has no effect under XP.
                        Marshal.ThrowExceptionForHR(NativeMethods.SetWindowTheme(this.Handle, "explorer", null));
                    }

                    // Remove the focus rectangle from the control (and as a side effect, all other controls on the
                    // form) if the last input event came from the mouse, or add them if it came from the keyboard.
                    NativeMethods.SendMessage(this.Handle, NativeMethods.WM_CHANGEUISTATE, this.MakeLParam(NativeMethods.UIS_INITIALIZE, NativeMethods.UISF_HIDEFOCUS), IntPtr.Zero);
                    break;
                case NativeMethods.LVM_SETEXTENDEDLISTVIEWSTYLE:
                    if (OsUtils.WinXpOrLater())
                    {
                        int styles = (int)m.LParam;

                        if ((styles & NativeMethods.LVS_EX_DOUBLEBUFFER) != NativeMethods.LVS_EX_DOUBLEBUFFER)
                        {
                            styles = styles | NativeMethods.LVS_EX_DOUBLEBUFFER;
                            m.LParam = (IntPtr)styles;
                        }
                    }

                    break;
                case NativeMethods.WM_SETFOCUS:
                    // Remove the focus rectangle from the control (and as a side effect, all other controls on the
                    // form) if the last input event came from the mouse, or add them if it came from the keyboard.
                    NativeMethods.SendMessage(this.Handle, NativeMethods.WM_CHANGEUISTATE, this.MakeLParam(NativeMethods.UIS_INITIALIZE, NativeMethods.UISF_HIDEFOCUS), IntPtr.Zero);
                    break;
                case NativeMethods.WM_NOTIFY:
                    // Test to see if the notification was for a right-click in the header
                    if (((NativeMethods.NMHDR)m.GetLParam(typeof(NativeMethods.NMHDR))).code == NativeMethods.NM_RCLICK)
                    {
                        // Fire an event to indicate the click has occurred.  Set the column number
                        // to -1 for all clicks, as this information isn't currently required.
                        if (this.ColumnRightClick != null)
                        {
                            this.ColumnRightClick(this, new ColumnClickEventArgs(-1));
                        }
                    }

                    break;
                case NativeMethods.WM_PAINT:
                    if (this.View != View.Details)
                    {
                        break;
                    }

                    // Calculate the position of all embedded controls
                    foreach (KeyValuePair<ListViewItem, EmbeddedProgress> embedded in this.embeddedInfo)
                    {
                        Rectangle rect = this.GetSubItemBounds(embedded.Key, embedded.Value.Column);

                        if (((this.HeaderStyle != ColumnHeaderStyle.None) && (rect.Top < this.Font.Height)) || (rect.Top + rect.Height) <= 0 || (rect.Top > this.ClientRectangle.Height))
                        {
                            // Control overlaps ColumnHeader, is off the top, or is off the bottom of the listview
                            embedded.Value.Progress.Visible = false;
                            continue;
                        }
                        else
                        {
                            embedded.Value.Progress.Visible = true;
                        }

                        // Set embedded control's bounds
                        embedded.Value.Progress.Bounds = rect;
                    }

                    break;
            }

            base.WndProc(ref m);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control)
            {
                if (this.MultiSelect)
                {
                    foreach (ListViewItem item in this.Items)
                    {
                        item.Selected = true;
                    }

                    return;
                }
            }

            base.OnKeyDown(e);
        }

        // Normally, the SelectedIndexChanged event gets fired once for each item that is affected
        // by a user's action.  For instance, clicking on a different item raises two - one for the
        // deselection of the old item, and one for the new.  Even worse, selecting multiple items
        // with the shift key causes an event to be raised for each one.
        //
        // To prevent this behavior, override OnSelectedIndexChanged, and instead set up a handler
        // to be called when the application is just about to become idle.
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            if (!this.waitingSelChange)
            {
                Application.Idle += this.Application_Idle;
                this.waitingSelChange = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.waitingSelChange)
            {
                Application.Idle -= this.Application_Idle;
            }

            base.Dispose(disposing);
        }

        private int[] GetColumnOrder()
        {
            int[] order = new int[this.Columns.Count + 1];

            for (int process = 0; process <= this.Columns.Count - 1; process++)
            {
                order[process] = this.Columns[process].DisplayIndex;
            }

            return order;
        }

        private Rectangle GetSubItemBounds(ListViewItem item, int subItem)
        {
            Rectangle subItemRect = Rectangle.Empty;

            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            int[] order = this.GetColumnOrder();

            if (order == null)
            {
                // No Columns
                return subItemRect;
            }

            if (subItem >= order.Length)
            {
                throw new ArgumentOutOfRangeException("SubItem " + subItem.ToString(CultureInfo.InvariantCulture) + " out of range");
            }

            // Retrieve the bounds of the entire ListViewItem (all subitems)
            Rectangle bounds = item.GetBounds(ItemBoundsPortion.Entire);

            int subItemX = bounds.Left;
            ColumnHeader header = null;
            int process = 0;

            // Calculate the X position of the SubItem.
            for (process = 0; process <= order.Length - 1; process++)
            {
                header = this.Columns[order[process]];

                if (header.Index == subItem)
                {
                    break;
                }

                subItemX += header.Width;
            }

            subItemRect = new Rectangle(subItemX, bounds.Top, this.Columns[order[process]].Width, bounds.Height);

            return subItemRect;
        }

        private void EmbeddedControl_Click(object sender, EventArgs e)
        {
            // When a progress bar is clicked the ListViewItem holding it is selected
            foreach (KeyValuePair<ListViewItem, EmbeddedProgress> control in this.embeddedInfo)
            {
                if (control.Value.Progress.Equals((ProgressBar)sender))
                {
                    this.SelectedItems.Clear();
                    control.Key.Selected = true;
                }
            }
        }

        // Called when a SelectedIndexChanged (or several) has been suppressed, and the application
        // is just about to become idle (e.g. no more events to be raised), so raise one event.
        private void Application_Idle(object sender, EventArgs e)
        {
            Application.Idle -= this.Application_Idle;
            this.waitingSelChange = false;

            base.OnSelectedIndexChanged(e);
        }

        private IntPtr MakeLParam(int loWord, int hiWord)
        {
            IntPtr hiWordPart = (IntPtr)(hiWord << 16);
            IntPtr loWordPart = (IntPtr)(loWord & 0xffff);

            return (IntPtr)(hiWordPart.ToInt32() | loWordPart.ToInt32());
        }

        private struct EmbeddedProgress
        {
            public ProgressBar Progress;
            public int Column;
        }
    }
}
