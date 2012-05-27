/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2012 Matt Robinson
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
    using System.Runtime.InteropServices;
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
                    this.DefWndProc(ref m);

                    if (OsUtils.WinVistaOrLater() && VisualStyleRenderer.IsSupported && this.glassSet)
                    {
                        if ((int)m.Result == NativeMethods.HTCLIENT)
                        {
                            // Pretend that the mouse was over the title bar, making the form draggable
                            m.Result = (IntPtr)NativeMethods.HTCAPTION;
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

            Marshal.ThrowExceptionForHR(NativeMethods.DwmExtendFrameIntoClientArea(this.Handle, ref this.glassMargins));
        }
    }
}
