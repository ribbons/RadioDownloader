/*
 * This file is part of the Podcast Provider for Radio Downloader.
 * Copyright © 2018-2019 by the authors - see the AUTHORS file for details.
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
    using RadioDld.Provider;
    using Xunit;

    /// <summary>
    /// Tests for the DownloadProgramme function
    /// </summary>
    public class TestDownload
    {
        /// <summary>
        /// Test for a successful download
        /// </summary>
        [Fact]
        public void Success()
        {
            string progExtId = "http://example.com/BasicPodcast.xml";
            string epExtId = "http://example.com/programme1/episode1.mp3";
            string tempFileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            var instance = TestCommon.CreateInstance();

            var programme = instance.GetProgrammeInfo(progExtId);
            var episode = instance.GetEpisodeInfo(progExtId, programme, epExtId);
            var download = instance.DownloadProgramme(progExtId, epExtId, programme, episode, tempFileName);

            Assert.True(File.Exists(tempFileName + "." + download.Extension));
            File.Delete(tempFileName + "." + download.Extension);
        }

        /// <summary>
        /// Test that DownloadException is thrown with the correct values for download errors
        /// </summary>
        [Fact]
        public void ErrorHandling()
        {
            string progExtId = "http://example.com/DownloadErrors.xml";
            string epExtId = "http://example.com/errors/http504";
            string tempFileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            var instance = TestCommon.CreateInstance();

            var programme = instance.GetProgrammeInfo(progExtId);
            var episode = instance.GetEpisodeInfo(progExtId, programme, epExtId);

            var e = Assert.Throws<DownloadException>(() =>
                instance.DownloadProgramme(progExtId, epExtId, programme, episode, tempFileName));

            Assert.Equal(e.ErrorType, ErrorType.RemoteProblem);
            Assert.Equal(e.Message, "The remote server returned an error: (504) Gateway Timeout.");

            epExtId = "http://example.com/errors/untrustedCert";
            episode = instance.GetEpisodeInfo(progExtId, programme, epExtId);

            e = Assert.Throws<DownloadException>(() =>
                instance.DownloadProgramme(progExtId, epExtId, programme, episode, tempFileName));

            Assert.Equal(e.ErrorType, ErrorType.NetworkProblem);
            Assert.Equal(e.Message, "The remote certificate is invalid according to the validation procedure.");
        }

        /// <summary>
        /// Test retrieval of chapters from Podlove Simple Chapters example
        /// </summary>
        [Fact]
        public void ChaptersExample()
        {
            string progExtId = "http://example.com/ChaptersExample.xml";
            string epExtId = "urn:uuid:3241ace2-ca21-dd12-2341-1412ce31fad2";
            string tempFileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            var instance = TestCommon.CreateInstance();

            var programme = instance.GetProgrammeInfo(progExtId);
            var episode = instance.GetEpisodeInfo(progExtId, programme, epExtId);
            var download = instance.DownloadProgramme(progExtId, epExtId, programme, episode, tempFileName);
            File.Delete(tempFileName + "." + download.Extension);

            Assert.Equal(4, download.Chapters.Count);

            Assert.Equal(0, download.Chapters[0].Start.TotalMilliseconds);
            Assert.Equal(187000, download.Chapters[1].Start.TotalMilliseconds);
            Assert.Equal(506250, download.Chapters[2].Start.TotalMilliseconds);
            Assert.Equal(762000, download.Chapters[3].Start.TotalMilliseconds);

            Assert.Equal("Welcome", download.Chapters[0].Name);
            Assert.Equal("Introducing Podlove", download.Chapters[1].Name);
            Assert.Equal("Podlove WordPress Plugin", download.Chapters[2].Name);
            Assert.Equal("Resumée", download.Chapters[3].Name);

            Assert.Null(download.Chapters[0].Link);
            Assert.Equal(new Uri("http://podlove.org/"), download.Chapters[1].Link);
            Assert.Equal(new Uri("http://podlove.org/podlove-podcast-publisher"), download.Chapters[2].Link);
            Assert.Null(download.Chapters[3].Link);

            for (int i = 0; i < 4; i++)
            {
                Assert.Null(download.Chapters[i].Image);
            }
        }

        /// <summary>
        /// Test retrieval of images from Podlove Simple Chapter data
        /// </summary>
        [Fact]
        public void ChapterImage()
        {
            string progExtId = "http://example.com/ChapterImage.xml";
            string epExtId = "http://example.com/programme1/episode1.mp3";
            string tempFileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            var instance = TestCommon.CreateInstance();

            var programme = instance.GetProgrammeInfo(progExtId);
            var episode = instance.GetEpisodeInfo(progExtId, programme, epExtId);
            var download = instance.DownloadProgramme(progExtId, epExtId, programme, episode, tempFileName);
            File.Delete(tempFileName + "." + download.Extension);

            Assert.Equal(1, download.Chapters.Count);
            Assert.NotNull(download.Chapters[0].Image);
        }
    }
}
