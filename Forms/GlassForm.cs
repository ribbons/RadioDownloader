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
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    public class GlassForm : Form
    {
        private bool glassSet;
        private NativeMethods.MARGINS glassMargins;

        public void SetGlassMargins(int leftMargin, int rightMargin, int topMargin, int bottomMargin)
        {
            this.glassMargins = new NativeMethods.MARGINS();

            this.glassMargins.cxLeftWidth = leftMargin;
            this.glassMargins.cxRightWidth = rightMargin;
            this.glassMargins.cyTopHeight = topMargin;
            this.glassMargins.cyButtomheight = bottomMargin;

            this.glassSet = true;
            this.ExtendFrameIntoClientArea();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                case NativeMethods.WM_DWMCOMPOSITIONCHANGED:
                    if (this.glassSet)
                    {
                        this.ExtendFrameIntoClientArea();
                    }

                    break;
                case NativeMethods.WM_NCHITTEST:
                    DefWndProc(ref m);

                    if (OsUtils.WinVistaOrLater() && VisualStyleRenderer.IsSupported && this.glassSet)
                    {
                        if ((int)m.Result == NativeMethods.HTCLIENT)
                        {
                            // Pretend that the mouse was over the title bar, making the form draggable
                            m.Result = new IntPtr(NativeMethods.HTCAPTION);
                            return;
                        }
                    }

                    break;
            }

            base.WndProc(ref m);
        }

        private void ExtendFrameIntoClientArea()
        {
            if (!OsUtils.CompositionEnabled())
            {
                return;
            }

            if (NativeMethods.DwmExtendFrameIntoClientArea(this.Handle, ref this.glassMargins) != 0)
            {
                throw new Win32Exception();
            }
        }
    }
}
