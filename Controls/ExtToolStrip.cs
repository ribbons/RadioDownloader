/*
 * Copyright © 2010-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class ExtToolStrip : ToolStrip
    {
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case NativeMethods.WM_NCHITTEST:
                    int xPos = ((int)m.LParam << 16) >> 16;
                    int yPos = (int)m.LParam >> 16;

                    Point clientPos = this.PointToClient(new Point(xPos, yPos));
                    bool onBackground = true;

                    // Test to see if the mouse is over any of the toolstrip controls
                    foreach (ToolStripItem child in this.Items)
                    {
                        if (child.Bounds.Contains(clientPos) && child.Visible)
                        {
                            ToolStripControlHost controlHost = child as ToolStripControlHost;

                            if (controlHost != null)
                            {
                                // This is a control host, so check the click wasn't outside the child control
                                if (!controlHost.Control.Bounds.Contains(clientPos))
                                {
                                    onBackground = true;
                                    break;
                                }
                            }

                            onBackground = false;
                            break;
                        }
                    }

                    if (onBackground)
                    {
                        // Make the strip transparent to mouse actions in this area
                        m.Result = (IntPtr)NativeMethods.HTTRANSPARENT;
                        return;
                    }

                    break;
            }

            base.WndProc(ref m);
        }
    }
}
