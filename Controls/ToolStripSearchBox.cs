/*
 * Copyright © 2010-2012 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System.Windows.Forms;

    internal class ToolStripSearchBox : ToolStripControlHost
    {
        public ToolStripSearchBox()
            : base(new SearchBox())
        {
        }

        public string CueBanner
        {
            get { return ((SearchBox)this.Control).CueBanner; }
            set { ((SearchBox)this.Control).CueBanner = value; }
        }
    }
}
