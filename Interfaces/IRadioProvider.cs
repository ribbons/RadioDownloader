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
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public delegate void FindNewViewChangeEventHandler(object view);

    public delegate void FindNewExceptionEventHandler(Exception findExp, bool unhandled);

    public delegate void FoundNewEventHandler(string progExtId);

    public delegate void ProgressEventHandler(int percent, ProgressType type);

    public enum ProgressType
    {
        Downloading,
        Processing
    }

    public interface IRadioProvider
    {
        event FindNewViewChangeEventHandler FindNewViewChange;

        event FindNewExceptionEventHandler FindNewException;

        event FoundNewEventHandler FoundNew;

        event ProgressEventHandler Progress;

        Guid ProviderId { get; }

        string ProviderName { get; }

        Bitmap ProviderIcon { get; }

        string ProviderDescription { get; }

        int ProgInfoUpdateFreqDays { get; }

        EventHandler GetShowOptionsHandler();

        Panel GetFindNewPanel(object view);

        ProgrammeInfo GetProgrammeInfo(string progExtId);

        /// <summary>
        /// Fetch a list of IDs for currently available episodes.
        /// </summary>
        /// <param name="progExtId">The external id of the programme to list episodes for.</param>
        /// <param name="progInfo">Data from the last call to GetProgrammeInfo for this programme (without image data).</param>
        /// <returns>A list of currently available episode IDs, or null.</returns>
        string[] GetAvailableEpisodeIds(string progExtId, ProgrammeInfo progInfo);

        /// <summary>
        /// Fetch information about the specified episode.
        /// </summary>
        /// <param name="progExtId">The external id of the programme that the episode belongs to.</param>
        /// <param name="progInfo">Data from the last call to GetProgrammeInfo for this programme (without image data).</param>
        /// <param name="episodeExtId">The external id of the episode to fetch information about.</param>
        /// <returns>An <see cref="EpisodeInfo"/> class populated with information about the episode, or null.</returns>
        EpisodeInfo GetEpisodeInfo(string progExtId, ProgrammeInfo progInfo, string episodeExtId);

        /// <summary>
        /// Perform a download of the specified episode.
        /// </summary>
        /// <param name="progExtId">The external id of the programme that the episode belongs to.</param>
        /// <param name="episodeExtId">The external id of the episode to download.</param>
        /// <param name="progInfo">Data from the last call to GetProgrammeInfo for this programme.</param>
        /// <param name="epInfo">Data from the last call to GetEpisodeInfo for this episode.</param>
        /// <param name="finalName">The path and filename (minus file extension) to save this download as.</param>
        /// <exception cref="DownloadException">Thrown when an expected error is encountered whilst downloading.</exception>
        /// <returns>The file extension of a successful download.</returns>
        string DownloadProgramme(string progExtId, string episodeExtId, ProgrammeInfo progInfo, EpisodeInfo epInfo, string finalName);
    }
}
