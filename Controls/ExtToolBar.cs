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
using System.Runtime.InteropServices;
namespace RadioDld
{

	internal class ExtToolBar : ToolBar
	{
		// Window Messages
		private const int WM_USER = 0x400;

		private const int TB_SETBUTTONINFO = WM_USER + 64;
		// TBBUTTONINFO Mask Flags
		private const uint TBIF_STYLE = 0x8;
		private const int TBIF_SIZE = 0x40;

        private const uint TBIF_BYINDEX = 0x80000000;
		// TBBUTTONINFO Style Flags
		private const int BTNS_AUTOSIZE = 0x10;

		private const int BTNS_WHOLEDROPDOWN = 0x80;
		//API Structs
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct TBBUTTONINFO
		{
			public int cbSize;
			public uint dwMask;
			public int idCommand;
			public int iImage;
			public byte fsState;
			public byte fsStyle;
			public short cx;
			public IntPtr lParam;
			public IntPtr pszText;
			public int cchText;
		}

		// API Declarations
		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref TBBUTTONINFO lParam);

		// Variables

		private List<ToolBarButton> wholeDropDownButtons = new List<ToolBarButton>();
		public void SetWholeDropDown(ToolBarButton button)
		{
			if (button == null) {
				throw new ArgumentNullException("button");
			}

			if (wholeDropDownButtons.Contains(button) == false) {
				wholeDropDownButtons.Add(button);
			}

			TBBUTTONINFO buttonInfo = default(TBBUTTONINFO);

			buttonInfo.cbSize = Marshal.SizeOf(buttonInfo);
            buttonInfo.dwMask = TBIF_STYLE | TBIF_BYINDEX;
            buttonInfo.fsStyle = BTNS_WHOLEDROPDOWN | BTNS_AUTOSIZE;

			SendMessage(this.Handle, TB_SETBUTTONINFO, (IntPtr)this.Buttons.IndexOf(button), ref buttonInfo);
		}

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg) {
				case TB_SETBUTTONINFO:
					TBBUTTONINFO tbrInfo = default(TBBUTTONINFO);
					tbrInfo = (TBBUTTONINFO)Marshal.PtrToStructure(m.LParam, typeof(TBBUTTONINFO));

					if ((tbrInfo.dwMask & TBIF_SIZE) == TBIF_SIZE) {
						// If the .net wrapper is trying to set the size, then prevent this
						tbrInfo.dwMask = tbrInfo.dwMask ^ TBIF_SIZE;
					}

					if ((tbrInfo.dwMask & TBIF_STYLE) == TBIF_STYLE) {
						if ((tbrInfo.fsStyle & BTNS_AUTOSIZE) != BTNS_AUTOSIZE) {
							// Make sure that the autosize style is set for all buttons, and doesn't
							// get inadvertantly unset at any point by the .net wrapper
                            tbrInfo.fsStyle = (byte)(tbrInfo.fsStyle | BTNS_AUTOSIZE);
						}
					}

					Marshal.StructureToPtr(tbrInfo, m.LParam, true);
					break;
			}

			base.WndProc(ref m);
		}

		protected override bool ProcessMnemonic(char inputChar)
		{
			foreach (ToolBarButton checkButton in this.Buttons) {
				if (checkButton.Visible && IsMnemonic(inputChar, checkButton.Text)) {
					if (wholeDropDownButtons.Contains(checkButton)) {
						// Give the toolbar button a pressed appearance
						checkButton.Pushed = true;
						// Set the whole dropdown flag again as setting pushed will have cleared it
						SetWholeDropDown(checkButton);

						// Calculate where the menu should be shown
						Point menuLocation = new Point(checkButton.Rectangle.Left, checkButton.Rectangle.Bottom);
						// Show the menu (modally)
						((ContextMenu)checkButton.DropDownMenu).Show(this, menuLocation);

						// Remove the pressed appearance
						checkButton.Pushed = false;
						SetWholeDropDown(checkButton);
					} else {
						// Just fire the click code for the button
						this.OnButtonClick(new ToolBarButtonClickEventArgs(checkButton));
					}

					// Let the calling function know that we found a matching mnemonic
					return true;
				}
			}

			return false;
		}
	}
}
