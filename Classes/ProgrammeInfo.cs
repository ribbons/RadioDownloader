/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2012 Matt Robinson
 * 
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
 * implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
 * License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with this program.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

namespace RadioDld
{
    using System.Drawing;

    /// <summary>
    /// Class containing programme information to be passed to and from providers.
    /// </summary>
    public class ProgrammeInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgrammeInfo" /> class.
        /// </summary>
        public ProgrammeInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgrammeInfo" /> class with the values
        /// specified in the specified Programme object.
        /// </summary>
        /// <param name="progInfo">Programme object specifying initial values.</param>
        internal ProgrammeInfo(Model.Programme progInfo)
        {
            this.Name = progInfo.Name;
            this.Description = progInfo.Description;
            this.SingleEpisode = progInfo.SingleEpisode;
        }

        /// <summary>
        /// Gets or sets the programme name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the programme description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets an image associated with the programme.
        /// </summary>
        public Bitmap Image { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the programme consists of a single standalone episode or not.
        /// </summary>
        public bool SingleEpisode { get; set; }
    }
}
