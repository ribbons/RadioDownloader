/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2020 by the authors - see the AUTHORS file for details.
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
    /// Class containing information returned from providers after a successful download.
    /// </summary>
    public class DownloadInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadInfo" /> class.
        /// </summary>
        public DownloadInfo()
        {
            this.Chapters = new Collection<ChapterInfo>();
        }

        /// <summary>
        /// Gets or sets the file extension for the downloaded audio file.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Gets a list containing chapters for the download.
        /// </summary>
        public Collection<ChapterInfo> Chapters { get; private set; }
    }
}
