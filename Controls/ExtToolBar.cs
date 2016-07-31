/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2016 by the authors - see the AUTHORS file for details.
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
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class ExtToolBar : ToolBar
    {
        private List<ToolBarButton> wholeDropDownButtons = new List<ToolBarButton>();

        public void SetWholeDropDown(ToolBarButton button)
        {
            if (button == null)
            {
                throw new ArgumentNullException("button");
            }

            if (!this.wholeDropDownButtons.Contains(button))
            {
                this.wholeDropDownButtons.Add(button);

                if (!OsUtils.Windows())
                {
                    // Append a down arrow as BTNS_WHOLEDROPDOWN is only supported under Windows
                    button.Text += " ▾";
                }
            }

            NativeMethods.TBBUTTONINFO buttonInfo = default(NativeMethods.TBBUTTONINFO);

            buttonInfo.cbSize = Marshal.SizeOf(buttonInfo);
            buttonInfo.dwMask = NativeMethods.TBIF_STYLE | NativeMethods.TBIF_BYINDEX;
            buttonInfo.fsStyle = NativeMethods.BTNS_WHOLEDROPDOWN | NativeMethods.BTNS_AUTOSIZE;

            NativeMethods.SendMessage(this.Handle, NativeMethods.TB_SETBUTTONINFO, (IntPtr)this.Buttons.IndexOf(button), ref buttonInfo);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case NativeMethods.TB_SETBUTTONINFO:
                    NativeMethods.TBBUTTONINFO tbrInfo = default(NativeMethods.TBBUTTONINFO);
                    tbrInfo = (NativeMethods.TBBUTTONINFO)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.TBBUTTONINFO));

                    if ((tbrInfo.dwMask & NativeMethods.TBIF_SIZE) == NativeMethods.TBIF_SIZE)
                    {
                        // If the .net wrapper is trying to set the size, then prevent this
                        tbrInfo.dwMask = tbrInfo.dwMask ^ NativeMethods.TBIF_SIZE;
                    }

                    if ((tbrInfo.dwMask & NativeMethods.TBIF_STYLE) == NativeMethods.TBIF_STYLE)
                    {
                        if ((tbrInfo.fsStyle & NativeMethods.BTNS_AUTOSIZE) != NativeMethods.BTNS_AUTOSIZE)
                        {
                            // Make sure that the autosize style is set for all buttons, and doesn't
                            // get inadvertantly unset at any point by the .net wrapper
                            tbrInfo.fsStyle = (byte)(tbrInfo.fsStyle | NativeMethods.BTNS_AUTOSIZE);
                        }
                    }

                    Marshal.StructureToPtr(tbrInfo, m.LParam, true);
                    break;
            }

            base.WndProc(ref m);
        }

        protected override void OnButtonClick(ToolBarButtonClickEventArgs e)
        {
            if (this.wholeDropDownButtons.Contains(e.Button))
            {
                // As the click event has fired for a whole dropdown we aren't
                // running under Windows, so show it ourselves
                this.ShowDropdownMenu(e.Button);
                return;
            }

            base.OnButtonClick(e);
        }

        protected override bool ProcessMnemonic(char inputChar)
        {
            foreach (ToolBarButton checkButton in this.Buttons)
            {
                if (checkButton.Visible && IsMnemonic(inputChar, checkButton.Text))
                {
                    if (this.wholeDropDownButtons.Contains(checkButton))
                    {
                        this.ShowDropdownMenu(checkButton);
                    }
                    else
                    {
                        // Just fire the click code for the button
                        this.OnButtonClick(new ToolBarButtonClickEventArgs(checkButton));
                    }

                    // Let the calling function know that we found a matching mnemonic
                    return true;
                }
            }

            return false;
        }

        private void ShowDropdownMenu(ToolBarButton button)
        {
            // Give the toolbar button a pressed appearance
            button.Pushed = true;

            // Set the whole dropdown flag again as setting pushed will have cleared it
            this.SetWholeDropDown(button);

            // Calculate where the menu should be shown
            Point menuLocation = new Point(button.Rectangle.Left, button.Rectangle.Bottom);

            // Show the menu (modally)
            ((ContextMenu)button.DropDownMenu).Show(this, menuLocation);

            // Remove the pressed appearance
            button.Pushed = false;
            this.SetWholeDropDown(button);
        }
    }
}
