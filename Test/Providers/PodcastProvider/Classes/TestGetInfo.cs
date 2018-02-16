/*
 * This file is part of the Podcast Provider for Radio Downloader.
 * Copyright © 2018 by the authors - see the AUTHORS file for details.
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
    using PodcastProvider;
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
            var instance = new PodcastProvider();
            instance.CachedWebClient = new CachedWebClientTest();
            string extId = "http://example.com/BasicPodcast.xml";

            var programme = instance.GetProgrammeInfo(extId);
            Assert.Equal(programme.Name, "Basic Podcast");
            Assert.Equal(programme.Description, "Basic Podcast description");
            Assert.Null(programme.Image);

            var episode = instance.GetEpisodeInfo(extId, programme, "http://example.com/programme1/episode1.mp3");
            Assert.Equal(episode.Name, "Episode 1");
            Assert.Equal(episode.Description, "Episode 1 description");
            Assert.Null(episode.Image);
            Assert.Null(episode.Duration);
        }
    }
}
