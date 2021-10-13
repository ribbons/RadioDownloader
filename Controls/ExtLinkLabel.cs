/*
 * Copyright © 2012-2016 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

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
            if (OsUtils.Windows() && m.Msg == NativeMethods.WM_SETCURSOR)
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
