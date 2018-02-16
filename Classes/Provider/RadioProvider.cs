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
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public delegate void FindNewViewChangeEventHandler(object view);

    public delegate void FindNewExceptionEventHandler(Exception findExp, bool unhandled);

    public delegate void FoundNewEventHandler(string progExtId);

    public delegate void ProgressEventHandler(int percent, ProgressType type);

    public delegate void ShowMoreProgInfoEventHandler(string progExtId);

    public enum ProgressType
    {
        Downloading,
        Processing
    }

    public abstract class RadioProvider
    {
        private CachedWebClientBase cachedWebClient;

        public virtual event FindNewViewChangeEventHandler FindNewViewChange
        {
            add { }
            remove { }
        }

        public virtual event FindNewExceptionEventHandler FindNewException
        {
            add { }
            remove { }
        }

        public abstract event FoundNewEventHandler FoundNew;

        public abstract event ProgressEventHandler Progress;

        public abstract Guid ProviderId { get; }

        public abstract string ProviderName { get; }

        /// <summary>
        /// Gets the icon to display in the Find New view for the provider, or null for default.
        /// </summary>
        public virtual Bitmap ProviderIcon
        {
            get
            {
                return null;
            }
        }

        public abstract string ProviderDescription { get; }

        public abstract int ProgInfoUpdateFreqDays { get; }

        /// <summary>
        /// Gets an event handler to be called to show an options dialog for the provider
        /// </summary>
        /// <returns>The event handler or null for none</returns>
        public virtual EventHandler ShowOptionsHandler
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets an event handler to be called to show provider specific details about a programme
        /// </summary>
        /// <returns>The event handler or null to for none</returns>
        public virtual ShowMoreProgInfoEventHandler ShowMoreProgInfoHandler
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="CachedWebClientBase"/> instance for this provider.
        /// </summary>
        public CachedWebClientBase CachedWebClient
        {
            get
            {
                if (this.cachedWebClient == null)
                {
                    this.cachedWebClient = new CachedWebClient();
                }

                return this.cachedWebClient;
            }

            set
            {
                this.cachedWebClient = value;
            }
        }

        public abstract Panel GetFindNewPanel(object view);

        public abstract ProgrammeInfo GetProgrammeInfo(string progExtId);

        /// <summary>
        /// Fetch a list of IDs for currently available episodes.
        /// </summary>
        /// <param name="progExtId">The external id of the programme to list episodes for.</param>
        /// <param name="progInfo">Data from the last call to GetProgrammeInfo for this programme (without image data).</param>
        /// <param name="page">The one-based page number to return results for.</param>
        /// <returns>An <see cref="AvailableEpisodes"/> object containing available episode data.</returns>
        public abstract AvailableEpisodes GetAvailableEpisodes(string progExtId, ProgrammeInfo progInfo, int page);

        /// <summary>
        /// Fetch information about the specified episode.
        /// </summary>
        /// <param name="progExtId">The external id of the programme that the episode belongs to.</param>
        /// <param name="progInfo">Data from the last call to GetProgrammeInfo for this programme (without image data).</param>
        /// <param name="episodeExtId">The external id of the episode to fetch information about.</param>
        /// <returns>An <see cref="EpisodeInfo"/> class populated with information about the episode, or null.</returns>
        public abstract EpisodeInfo GetEpisodeInfo(string progExtId, ProgrammeInfo progInfo, string episodeExtId);

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
        public abstract string DownloadProgramme(string progExtId, string episodeExtId, ProgrammeInfo progInfo, EpisodeInfo epInfo, string finalName);

        /// <summary>
        /// Cancel the episode download currently in progress.
        /// </summary>
        public abstract void CancelDownload();
    }
}
