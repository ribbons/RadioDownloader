/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2018 by the authors - see the AUTHORS file for details.
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

namespace RadioDld.Provider
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Return type from providers when requesting a list of available episodes.
    /// </summary>
    public class AvailableEpisodes
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AvailableEpisodes" /> class.
        /// </summary>
        public AvailableEpisodes()
        {
            this.EpisodeIds = new Collection<string>();
        }

        /// <summary>
        /// Gets a list to populate with currently available episode IDs.
        /// The list must be populated in reverse date order.
        /// </summary>
        public Collection<string> EpisodeIds { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether there are further pages of IDs available.
        /// </summary>
        public bool MoreAvailable { get; set; }
    }
}
