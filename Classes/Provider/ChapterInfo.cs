/*
 * This file is part of Radio Downloader.
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

namespace RadioDld.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    /// <summary>
    /// Class representing an episode chapter to be passed to and from providers.
    /// </summary>
    public class ChapterInfo
    {
        /// <summary>
        /// Gets or sets the chapter name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the start time for the chapter.
        /// </summary>
        public TimeSpan Start { get; set; }

        /// <summary>
        /// Gets or sets the link associated with the chapter.
        /// </summary>
        public Uri Link { get; set; }

        /// <summary>
        /// Gets or sets the image associated with the chapter.
        /// </summary>
        public Bitmap Image { get; set; }
    }
}
