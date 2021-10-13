/*
 * Copyright © 2008-2019 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld.Provider
{
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
        /// contained in the specified Programme object.
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
        public CompressedImage Image { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the programme consists of a single standalone episode or not.
        /// </summary>
        public bool SingleEpisode { get; set; }
    }
}
