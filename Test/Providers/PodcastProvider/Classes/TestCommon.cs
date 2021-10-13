/*
 * Copyright © 2018-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
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
