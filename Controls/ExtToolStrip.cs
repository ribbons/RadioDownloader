/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2011 Matt Robinson
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
    using System.Drawing;
    using System.Security.Permissions;
    using System.Windows.Forms;

    public class ExtToolStrip : ToolStrip
    {
        public ExtToolStrip()
            : base()
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
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
