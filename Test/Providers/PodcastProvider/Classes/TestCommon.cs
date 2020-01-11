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

    using PodcastProvider;

    internal static class TestCommon
    {
        /// <summary>
        /// Create a Podcast Provider instance and inject the test dependencies into it.
        /// </summary>
        /// <returns>A <see cref="PodcastProvider"/> instance ready to test.</returns>
        public static PodcastProvider CreateInstance()
        {
            var instance = new PodcastProvider();
            instance.CachedWebClient = new CachedWebClientTest();
            instance.GetTempFileInstance = (string fileExtension) => { return new TempFileTest(); };
            instance.GetDownloadWrapperInstance = (Uri downloadUrl, string destPath) => { return new DownloadWrapperTest(downloadUrl, destPath); };
            return instance;
        }
    }
}
