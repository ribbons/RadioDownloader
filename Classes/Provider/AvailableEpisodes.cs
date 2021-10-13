/*
 * Copyright © 2012-2018 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
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
