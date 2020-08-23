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

    using Xunit;

    /// <summary>
    /// Tests for the GetProgrammeInfo and GetEpisodeInfo functions.
    /// </summary>
    public class TestGetInfo
    {
        /// <summary>
        /// Test retrieval of basic podcast information.
        /// </summary>
        [Fact]
        public void BasicInfo()
        {
            var instance = TestCommon.CreateInstance();
            string extId = "http://example.com/BasicPodcast.xml";

            var programme = instance.GetProgrammeInfo(extId);
            Assert.Equal("Basic Podcast", programme.Name);
            Assert.Equal("Basic Podcast description", programme.Description);
            Assert.Null(programme.Image);

            var episodes = instance.GetAvailableEpisodes(extId, programme, 0);
            Assert.False(episodes.MoreAvailable);
            Assert.Equal(2, episodes.EpisodeIds.Count);
            Assert.Equal("http://example.com/programme1/episode1.mp3", episodes.EpisodeIds[1]);

            var episode = instance.GetEpisodeInfo(extId, programme, "http://example.com/programme1/episode1.mp3");
            Assert.Equal("Episode 1", episode.Name);
            Assert.Equal("Episode 1 description", episode.Description);
            Assert.Equal(episode.Date, new DateTime(2018, 02, 09, 20, 00, 00, DateTimeKind.Utc));
            Assert.Null(episode.Image);
            Assert.Null(episode.Duration);
        }

        /// <summary>
        /// Test retrieval of programme (e.g. podcast-level) images.
        /// Ensure that itunes:image is used in preference to image/url, but
        /// fallback to image/url happens if an error occurs.
        /// </summary>
        [Fact]
        public void ProgrammeImages()
        {
            var instance = TestCommon.CreateInstance();

            var programme = instance.GetProgrammeInfo("http://example.com/Images1.xml");
            Assert.Equal(16, programme.Image.Height);

            programme = instance.GetProgrammeInfo("http://example.com/Images2.xml");
            Assert.Equal(16, programme.Image.Height);
        }

        /// <summary>
        /// Test retrieval of episode images.
        /// Ensure that itunes:image is used in preference to media:thumbnail, but
        /// fallback to media:thumbnail happens if various types of error occur.
        /// </summary>
        [Fact]
        public void EpisodeImages()
        {
            var instance = TestCommon.CreateInstance();

            string extId = "http://example.com/Images1.xml";
            var programme = instance.GetProgrammeInfo(extId);
            var episode = instance.GetEpisodeInfo(extId, programme, "http://example.com/programme1/episode1.mp3");
            Assert.Equal(16, episode.Image.Height);

            extId = "http://example.com/Images2.xml";
            programme = instance.GetProgrammeInfo(extId);
            episode = instance.GetEpisodeInfo(extId, programme, "skip-image-http-404");
            Assert.Equal(16, episode.Image.Height);

            episode = instance.GetEpisodeInfo(extId, programme, "skip-image-invalid-uri");
            Assert.Equal(16, episode.Image.Height);

            episode = instance.GetEpisodeInfo(extId, programme, "skip-image-invalid");
            Assert.Equal(16, episode.Image.Height);
        }

        /// <summary>
        /// Test that items with invalid data are filtered from available episodes.
        /// </summary>
        [Fact]
        public void EpisodeFiltering()
        {
            var instance = TestCommon.CreateInstance();
            string extId = "http://example.com/EpisodeFiltering.xml";

            var programme = instance.GetProgrammeInfo(extId);
            var available = instance.GetAvailableEpisodes(extId, programme, 0).EpisodeIds;

            Assert.DoesNotContain("filter-title-missing", available);
            Assert.DoesNotContain("filter-title-empty", available);
            Assert.DoesNotContain("filter-enclosure-missing", available);
            Assert.DoesNotContain("filter-enclosure-no-url", available);
            Assert.DoesNotContain("filter-enclosure-invalid-url", available);

            Assert.Contains("nofilter-valid", available);
            Assert.Contains("nofilter-enclosure-encoded-url", available);
        }

        /// <summary>
        /// Test that correct podcast information can be retrieved for episodes
        /// with the same id as a previous filtered episode.
        /// </summary>
        [Fact]
        public void EpisodeSameIdAsFilteredInfo()
        {
            var instance = TestCommon.CreateInstance();
            string extId = "http://example.com/EpisodeFiltering.xml";

            var programme = instance.GetProgrammeInfo(extId);
            var episode = instance.GetEpisodeInfo(extId, programme, "filter-nofilter-sameid");

            Assert.Equal("NOT FILTERED", episode.Name);
        }
    }
}
