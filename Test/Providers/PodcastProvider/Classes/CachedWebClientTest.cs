/*
 * Copyright © 2018-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace PodcastProviderTest
{
    using System;
    using System.IO;
    using System.Net;

    internal class CachedWebClientTest : RadioDld.CachedWebClientBase
    {
        public override byte[] DownloadData(Uri uri, int fetchIntervalHrs, string userAgent)
        {
            if (uri.AbsolutePath == "/errors/http404")
            {
                throw new WebException(
                    "The remote server returned an error: (404) Not Found.",
                    null,
                    WebExceptionStatus.ProtocolError,
                    null);
            }

            string file = uri.AbsolutePath.Substring(1);
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", file);
            return File.ReadAllBytes(path);
        }
    }
}
