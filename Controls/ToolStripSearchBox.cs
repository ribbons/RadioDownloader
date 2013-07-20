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
