/*
 * Copyright © 2010-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal class GlassForm : Form
    {
        private bool glassSet;
        private NativeMethods.MARGINS glassMargins;

        public void SetGlassMargins(int leftMargin, int rightMargin, int topMargin, int bottomMargin)
        {
            this.glassMargins = default(NativeMethods.MARGINS);

            this.glassMargins.cxLeftWidth = leftMargin;
            this.glassMargins.cxRightWidth = rightMargin;
            this.glassMargins.cyTopHeight = topMargin;
            this.glassMargins.cyButtomheight = bottomMargin;

            this.glassSet = true;
            this.ExtendFrameIntoClientArea();
        }

        protected override void WndProc(ref Message m)
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
