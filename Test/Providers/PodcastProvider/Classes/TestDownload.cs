/*
 * Copyright © 2018-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace PodcastProviderTest
{
    using System;
    using System.IO;

    using RadioDld.Provider;
    using Xunit;

    /// <summary>
    /// Tests for the DownloadProgramme function.
    /// </summary>
    public class TestDownload
    {
        /// <summary>
        /// Test for a successful download.
        /// </summary>
        [Fact]
        public void Success()
        {
            string progExtId = "http://example.com/BasicPodcast.xml";
            string epExtId = "http://example.com/programme1/episode1.mp3";

            var instance = TestCommon.CreateInstance();

            var programme = instance.GetProgrammeInfo(progExtId);
            var episode = instance.GetEpisodeInfo(progExtId, programme, epExtId);

            using (var download = instance.DownloadProgramme(progExtId, epExtId, programme, episode))
            {
                Assert.True(File.Exists(download.Downloaded.FilePath));
            }
        }

        /// <summary>
        /// Test that DownloadException is thrown with an ErrorType of RemoveFromList
        /// when an episode is no-longer listed in the podcast feed.
        /// </summary>
        [Fact]
        public void NoLongerInFeed()
        {
            string progExtId = "http://example.com/BasicPodcast.xml";
            string epExtId = "http://example.com/programme1/episode1.mp3";
            string notExistId = "http://example.com/programme1/episode0.mp3";

            var instance = TestCommon.CreateInstance();

            var programme = instance.GetProgrammeInfo(progExtId);
            var episode = instance.GetEpisodeInfo(progExtId, programme, epExtId);

            var e = Assert.Throws<DownloadException>(() =>
                instance.DownloadProgramme(progExtId, notExistId, programme, episode));

            Assert.Equal(ErrorType.RemoveFromList, e.ErrorType);
        }

        /// <summary>
        /// Test that DownloadException is thrown with the correct values for download errors.
        /// </summary>
        [Fact]
        public void ErrorHandling()
        {
            string progExtId = "http://example.com/DownloadErrors.xml";
            string epExtId = "http://example.com/errors/http504";

            var instance = TestCommon.CreateInstance();

            var programme = instance.GetProgrammeInfo(progExtId);
            var episode = instance.GetEpisodeInfo(progExtId, programme, epExtId);

            var e = Assert.Throws<DownloadException>(() =>
                instance.DownloadProgramme(progExtId, epExtId, programme, episode));

            Assert.Equal(ErrorType.RemoteProblem, e.ErrorType);
            Assert.Equal("The remote server returned an error: (504) Gateway Timeout.", e.Message);

            epExtId = "http://example.com/errors/untrustedCert";
            episode = instance.GetEpisodeInfo(progExtId, programme, epExtId);

            e = Assert.Throws<DownloadException>(() =>
                instance.DownloadProgramme(progExtId, epExtId, programme, episode));

            Assert.Equal(ErrorType.NetworkProblem, e.ErrorType);
            Assert.Equal("The remote certificate is invalid according to the validation procedure.", e.Message);
        }

        /// <summary>
        /// Test retrieval of chapters from Podlove Simple Chapters example.
        /// </summary>
        [Fact]
        public void ChaptersExample()
        {
            string progExtId = "http://example.com/ChaptersExample.xml";
            string epExtId = "urn:uuid:3241ace2-ca21-dd12-2341-1412ce31fad2";

            var instance = TestCommon.CreateInstance();

            var programme = instance.GetProgrammeInfo(progExtId);
            var episode = instance.GetEpisodeInfo(progExtId, programme, epExtId);

            using (var download = instance.DownloadProgramme(progExtId, epExtId, programme, episode))
            {
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
        }

        /// <summary>
        /// Test retrieval of images from Podlove Simple Chapter data.
        /// </summary>
        [Fact]
        public void ChapterImage()
        {
            string progExtId = "http://example.com/Images1.xml";
            string epExtId = "http://example.com/programme1/episode1.mp3";

            var instance = TestCommon.CreateInstance();

            var programme = instance.GetProgrammeInfo(progExtId);
            var episode = instance.GetEpisodeInfo(progExtId, programme, epExtId);

            using (var download = instance.DownloadProgramme(progExtId, epExtId, programme, episode))
            {
                Assert.Single(download.Chapters);
                Assert.NotNull(download.Chapters[0].Image);
            }
        }
    }
}
