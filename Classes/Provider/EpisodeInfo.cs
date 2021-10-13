/*
 * Copyright © 2008-2019 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld.Provider
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class containing episode information to be passed to and from providers.
    /// </summary>
    public class EpisodeInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EpisodeInfo" /> class.
        /// </summary>
        public EpisodeInfo()
        {
            this.ExtInfo = new Dictionary<string, string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EpisodeInfo" /> class with the values
        /// contained in the specified Episode object.
        /// </summary>
        /// <param name="epInfo">Episode object specifying initial values.</param>
        internal EpisodeInfo(Model.Episode epInfo)
            : this()
        {
            this.Name = epInfo.Name;
            this.Description = epInfo.Description;
            this.Date = epInfo.Date;
            this.Duration = epInfo.Duration;
        }

        /// <summary>
        /// Gets or sets the episode name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the episode description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the duration of the episode in seconds.
        /// </summary>
        public int? Duration { get; set; }

        /// <summary>
        /// Gets or sets the date that the episode was broadcast or published.
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Gets or sets an image associated with the episode.
        /// </summary>
        public CompressedImage Image { get; set; }

        /// <summary>
        /// Gets a collection of name value pairs for the provider to use for storing arbitary data.
        /// </summary>
        public Dictionary<string, string> ExtInfo { get; private set; }
    }
}
