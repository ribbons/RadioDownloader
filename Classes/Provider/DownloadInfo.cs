/*
 * Copyright © 2018-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld.Provider
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Class containing information returned from providers after a successful download.
    /// </summary>
    public class DownloadInfo : IDisposable
    {
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadInfo" /> class.
        /// </summary>
        public DownloadInfo()
        {
            this.Chapters = new Collection<ChapterInfo>();
        }

        /// <summary>
        /// Gets or sets the downloaded content file.
        /// </summary>
        public TempFileBase Downloaded { get; set; }

        /// <summary>
        /// Gets or sets the file extension for the downloaded audio file.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Gets a list containing chapters for the download.
        /// </summary>
        public Collection<ChapterInfo> Chapters { get; private set; }

        /// <summary>
        /// Dispose the disposable objects owned by this instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.Downloaded?.Dispose();
                }

                this.isDisposed = true;
            }
        }
    }
}
