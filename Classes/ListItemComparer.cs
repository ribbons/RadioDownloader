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
                    return Model.Download.Compare(itemXId, itemYId);
                default:
                    throw new InvalidOperationException("Unknown compare type of " + this.compareType.ToString());
            }
        }
    }
}
