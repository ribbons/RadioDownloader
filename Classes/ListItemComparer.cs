/*
 * Copyright © 2010-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
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
            Download,
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
