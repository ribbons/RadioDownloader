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

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace RadioDld
{
    public class GlassForm : Form
    {
        private const int WM_NCHITTEST = 0x84;
        private const int WM_DWMCOMPOSITIONCHANGED = 0x31e;

        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyButtomheight;
        }

        [DllImport("dwmapi.dll", SetLastError = true)]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        private bool glassSet;
        private MARGINS glassMargins;

        public void SetGlassMargins(int leftMargin, int rightMargin, int topMargin, int bottomMargin)
        {
            glassMargins = new MARGINS();

            glassMargins.cxLeftWidth = leftMargin;
            glassMargins.cxRightWidth = rightMargin;
            glassMargins.cyTopHeight = topMargin;
            glassMargins.cyButtomheight = bottomMargin;

            glassSet = true;
            ExtendFrameIntoClientArea();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                case WM_DWMCOMPOSITIONCHANGED:
                    if (glassSet)
                    {
                        ExtendFrameIntoClientArea();
                    }

                    break;
                case WM_NCHITTEST:
                    DefWndProc(ref m);

                    if (OsUtils.WinVistaOrLater() && VisualStyleRenderer.IsSupported && glassSet)
                    {
                        if ((int)m.Result == HTCLIENT)
                        {
                            // Pretend that the mouse was over the title bar, making the form draggable
                            m.Result = new IntPtr(HTCAPTION);
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

            if (DwmExtendFrameIntoClientArea(this.Handle, ref glassMargins) != 0)
            {
                throw new Win32Exception();
            }
        }
    }
}
