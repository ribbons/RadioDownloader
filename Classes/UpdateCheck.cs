/*
 * Copyright © 2007-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
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
            ThreadPool.QueueUserWorkItem(state =>
            {
                CachedWebClient checkUpdate = new CachedWebClient();
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
