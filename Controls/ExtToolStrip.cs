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


using System.Security.Permissions;
namespace RadioDld
{

	public class ExtToolStrip : ToolStrip
	{

		private const int WM_NCHITTEST = 0x84;

		private const int HTTRANSPARENT = -0x1;
		public ExtToolStrip() : base()
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		protected override void WndProc(ref Message m)
		{
			switch (m.Msg) {
				case WM_NCHITTEST:
					int xPos = (Convert.ToInt32(m.LParam) << 16) >> 16;
					int yPos = Convert.ToInt32(m.LParam) >> 16;

					Point clientPos = this.PointToClient(new Point(xPos, yPos));
					bool onBackground = true;

					// Test to see if the mouse is over any of the toolstrip controls
					foreach (ToolStripItem child in this.Items) {
						if (child.Bounds.Contains(clientPos) && child.Visible) {
							ToolStripControlHost controlHost = child as ToolStripControlHost;

							if (controlHost != null) {
								// This is a control host, so check the click wasn't outside the child control
								if (!controlHost.Control.Bounds.Contains(clientPos)) {
									onBackground = true;
									break; // TODO: might not be correct. Was : Exit For
								}
							}

							onBackground = false;
							break; // TODO: might not be correct. Was : Exit For
						}
					}


					if (onBackground) {
						// Make the strip transparent to mouse actions in this area
						m.Result = new IntPtr(HTTRANSPARENT);
						return;
					}
					break;
			}

			base.WndProc(ref m);
		}
	}
}
