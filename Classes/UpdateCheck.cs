/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2015 by the authors - see the AUTHORS file for details.
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
    using System.Net;
    using System.Threading;
    using System.Windows.Forms;

    internal static class UpdateCheck
    {
        private const string BaseUrl = "https://nerdoftheherd.com/tools/radiodld/latestversion.txt?reqver=";
        private const int CacheHours = 24;

        /// <summary>
        /// Check to see if an update to the application is available.
        /// </summary>
        /// <param name="available">A <see cref="MethodInvoker"/> delegate which is called if an update is available.</param>
        internal static void CheckAvailable(MethodInvoker available)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                CachedWebClient checkUpdate = CachedWebClient.GetInstance();
                string versionInfo = null;

                try
                {
                    versionInfo = checkUpdate.DownloadString(new Uri(BaseUrl + Application.ProductVersion), CacheHours);
                }
                catch (WebException)
                {
                    // Temporary problem downloading the information, try again later
                    return;
                }

                Settings.LastCheckForUpdates = DateTime.Now;

                if (!string.IsNullOrEmpty(versionInfo))
                {
                    versionInfo = versionInfo.Split(new string[] { "\r\n" }, StringSplitOptions.None)[0];

                    if (string.Compare(versionInfo, Application.ProductVersion, StringComparison.Ordinal) > 0)
                    {
                        // There is a new version available
                        available();
                    }
                }

                return;
            });
        }
    }
}
