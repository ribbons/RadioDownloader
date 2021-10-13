/*
 * Copyright © 2008-2018 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;
    using System.Text;
    using System.Windows.Forms;

    public abstract class CachedWebClientBase
    {
        public abstract byte[] DownloadData(Uri uri, int fetchIntervalHrs, string userAgent);

        public string DownloadString(Uri uri, int fetchIntervalHrs, string userAgent)
        {
            return Encoding.UTF8.GetString(this.DownloadData(uri, fetchIntervalHrs, userAgent));
        }

        internal byte[] DownloadData(Uri uri, int fetchIntervalHrs)
        {
            return this.DownloadData(uri, fetchIntervalHrs, Application.ProductName + " " + Application.ProductVersion);
        }

        internal string DownloadString(Uri uri, int fetchIntervalHrs)
        {
            return this.DownloadString(uri, fetchIntervalHrs, Application.ProductName + " " + Application.ProductVersion);
        }
    }
}
