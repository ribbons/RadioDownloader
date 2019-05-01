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
    using Xunit;

    /// <summary>
    /// Tests for the GetProgrammeInfo and GetEpisodeInfo functions
    /// </summary>
    public class TestGetInfo
    {
        /// <summary>
        /// Test retrieval of basic podcast information
        /// </summary>
        [Fact]
        public void BasicInfo()
        {
            var instance = TestCommon.CreateInstance();
            string extId = "http://example.com/BasicPodcast.xml";

            var programme = instance.GetProgrammeInfo(extId);
            Assert.Equal(programme.Name, "Basic Podcast");
            Assert.Equal(programme.Description, "Basic Podcast description");
            Assert.Null(programme.Image);

            var episodes = instance.GetAvailableEpisodes(extId, programme, 0);
            Assert.False(episodes.MoreAvailable);
            Assert.Equal(2, episodes.EpisodeIds.Count);
            Assert.Equal("http://example.com/programme1/episode1.mp3", episodes.EpisodeIds[1]);

            var episode = instance.GetEpisodeInfo(extId, programme, "http://example.com/programme1/episode1.mp3");
            Assert.Equal(episode.Name, "Episode 1");
            Assert.Equal(episode.Description, "Episode 1 description");
            Assert.Equal(episode.Date, new DateTime(2018, 02, 09, 20, 00, 00, DateTimeKind.Utc));
            Assert.Null(episode.Image);
            Assert.Null(episode.Duration);
        }

        /// <summary>
        /// Test that items with invalid data are filtered from available episodes
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
    }
}
