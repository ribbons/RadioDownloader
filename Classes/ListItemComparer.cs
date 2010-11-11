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
    using System.Collections;
    using System.Globalization;
    using System.Windows.Forms;

    internal class ListItemComparer : IComparer
    {
        private Data dataInstance;
        private ListType compareType;

        public ListItemComparer(ListType compareType)
        {
            this.dataInstance = Data.GetInstance();
            this.compareType = compareType;
        }

        public enum ListType
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
                    return this.dataInstance.CompareFavourites(itemXId, itemYId);
                case ListType.Subscription:
                    return this.dataInstance.CompareSubscriptions(itemXId, itemYId);
                case ListType.Download:
                    return this.dataInstance.CompareDownloads(itemXId, itemYId);
                default:
                    throw new InvalidOperationException("Unknown compare type of " + this.compareType.ToString());
            }
        }
    }
}
