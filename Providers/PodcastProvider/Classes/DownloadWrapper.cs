/* 
 * This file is part of the Podcast Provider for Radio Downloader.
 * Copyright © 2007-2011 Matt Robinson
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
        private bool downloadComplete;

        private Exception downloadError;

        public DownloadWrapper(Uri downloadUrl, string destPath)
        {
            this.downloadUrl = downloadUrl;
            this.destPath = destPath;
        }

        public delegate void DownloadProgressEventHandler(object sender, System.Net.DownloadProgressChangedEventArgs e);

        public event DownloadProgressEventHandler DownloadProgress;

        public bool Complete
        {
            get { return this.downloadComplete; }
        }

        public Exception Error
        {
            get { return this.downloadError; }
        }

        public void Download()
        {
            SystemEvents.PowerModeChanged += this.PowerModeChange;

            this.downloadClient = new WebClient();
            this.downloadClient.DownloadProgressChanged += this.DownloadClient_DownloadProgressChanged;
            this.downloadClient.DownloadFileCompleted += this.DownloadClient_DownloadFileCompleted;

            this.downloadClient.Headers.Add("user-agent", Application.ProductName + " " + Application.ProductVersion);
            this.downloadClient.DownloadFileAsync(this.downloadUrl, this.destPath);
        }

        private void DownloadClient_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            if (this.DownloadProgress != null)
            {
                this.DownloadProgress(this, e);
            }
        }

        private void DownloadClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Cancelled == false)
            {
                SystemEvents.PowerModeChanged -= this.PowerModeChange;

                if (e.Error != null)
                {
                    this.downloadError = e.Error;
                }
                else
                {
                    this.downloadComplete = true;
                }
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
