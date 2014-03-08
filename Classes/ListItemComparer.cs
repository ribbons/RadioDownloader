/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2013 by the authors - see the AUTHORS file for details.
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
    using System.Collections;
    using System.Globalization;
    using System.Windows.Forms;

    internal class ListItemComparer : IComparer
    {
        private ListType compareType;

        public ListItemComparer(ListType compareType)
        {
            this.compareType = compareType;
        }

        internal enum ListType
        {
            Favourite,
            Subscription,
            Download
        }

        public int Compare(object x, object y)
        {
            int itemXId = Convert.ToInt32(((ListViewItem)x).Name, CultureInfo.InvariantCulture);
            int itemYId = Convert.ToInt32(((ListViewItem)y).Name, CultureInfo.InvariantCulture);

            switch (this.compareType)
            {
                case ListType.Favourite:
                    return Model.Favourite.Compare(itemXId, itemYId);
                case ListType.Subscription:
                    return Model.Subscription.Compare(itemXId, itemYId);
                case ListType.Download:
                    return Model.Download.Compare(itemXId, itemYId);
                default:
                    throw new InvalidOperationException("Unknown compare type of " + this.compareType.ToString());
            }
        }
    }
}
