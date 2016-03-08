/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2012 by the authors - see the AUTHORS file for details.
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

namespace RadioDld
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

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
        public Bitmap Image { get; set; }

        /// <summary>
        /// Gets a collection of name value pairs for the provider to use for storing arbitary data.
        /// </summary>
        public Dictionary<string, string> ExtInfo { get; private set; }
    }
}
