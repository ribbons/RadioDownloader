/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2018 by the authors - see the AUTHORS file for details.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
