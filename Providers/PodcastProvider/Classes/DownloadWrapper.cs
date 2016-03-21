/*
 * This file is part of the Podcast Provider for Radio Downloader.
 * Copyright © 2007-2014 by the authors - see the AUTHORS file for details.
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

namespace PodcastProvider
{
    using System;
    using System.Net;
    using System.Windows.Forms;
    using Microsoft.Win32;

    internal class DownloadWrapper
    {
        private WebClient downloadClient;

        private Uri downloadUrl;
        private string destPath;

        public DownloadWrapper(Uri downloadUrl, string destPath)
        {
            this.downloadUrl = downloadUrl;
            this.destPath = destPath;
        }

        public delegate void DownloadProgressEventHandler(object sender, DownloadProgressChangedEventArgs e);

        public event DownloadProgressEventHandler DownloadProgress;

        public bool Complete { get; private set; }

        public Exception Error { get; private set; }

        public bool Cancelled { get; private set; }

        public void Download()
        {
            SystemEvents.PowerModeChanged += this.PowerModeChange;

            this.downloadClient = new WebClient();
            this.downloadClient.DownloadProgressChanged += this.DownloadClient_DownloadProgressChanged;
            this.downloadClient.DownloadFileCompleted += this.DownloadClient_DownloadFileCompleted;

            this.downloadClient.Headers.Add("user-agent", PodcastProvider.UserAgent);
            this.downloadClient.DownloadFileAsync(this.downloadUrl, this.destPath);
        }

        public void Cancel()
        {
            this.Cancelled = true;
            this.downloadClient.CancelAsync();
        }

        private void DownloadClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (this.DownloadProgress != null)
            {
                this.DownloadProgress(this, e);
            }
        }

        private void DownloadClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Cancelled && !this.Cancelled)
            {
                // Cancelled before retry
                return;
            }

            SystemEvents.PowerModeChanged -= this.PowerModeChange;

            if (e.Error != null)
            {
                this.Error = e.Error;
            }
            else if (!this.Cancelled)
            {
                this.Complete = true;
            }
        }

        private void PowerModeChange(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                // Restart the download, as it is quite likely to have hung during the suspend / hibernate
                if (this.downloadClient.IsBusy)
                {
                    this.downloadClient.CancelAsync();

                    // Pause for 30 seconds to be give the pc a chance to settle down after the suspend.
                    System.Threading.Thread.Sleep(30000);

                    // Restart the download
                    this.downloadClient.DownloadFileAsync(this.downloadUrl, this.destPath);
                }
            }
        }
    }
}
