/*
 * Copyright © 2018-2019 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld.Provider
{
    using System;

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
        public CompressedImage Image { get; set; }
    }
}
