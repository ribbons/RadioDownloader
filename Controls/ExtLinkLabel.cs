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
    using System.ComponentModel;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    /// <summary>
    /// An enhanced version of the standard LinkLabel control which displays the correct system
    /// link 'hand' cursor instead of the old style one built into the .NET framework.
    /// </summary>
    internal class ExtLinkLabel : LinkLabel
    {
        /// <summary>
        /// Handle the WM_SETCURSOR message and set the correct system link cursor.
        /// </summary>
        /// <param name="m">The Windows <see cref="Message" /> to process.</param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_SETCURSOR)
            {
                // Fetch a handle to the system 'hand' cursor.
                IntPtr cursor = NativeMethods.LoadCursor(IntPtr.Zero, (IntPtr)NativeMethods.IDC_HAND);

                // NULL cursor
                if (cursor == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                // Set this control's cursor to the 'hand'
                NativeMethods.SetCursor(cursor);

                // Show that the message has been handled successfully
                m.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref m);
        }
    }
}
