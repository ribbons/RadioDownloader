/*
 * This file is part of the Podcast Provider for Radio Downloader.
 * Copyright © 2018-2020 by the authors - see the AUTHORS file for details.
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
