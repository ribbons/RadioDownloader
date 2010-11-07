// Plugin for Radio Downloader to download general podcasts.
// Copyright © 2007-2010 Matt Robinson
//
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General
// Public License as published by the Free Software Foundation; either version 2 of the License, or (at your
// option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
// implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
// License for more details.
//
// You should have received a copy of the GNU General Public License along with this program; if not, write
// to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

using System;
using System.Net;

using Microsoft.Win32;
using Microsoft.VisualBasic.ApplicationServices;

namespace PodcastProvider
{
    internal class DownloadWrapper
    {
        public event DownloadProgressEventHandler DownloadProgress;

        public delegate void DownloadProgressEventHandler(object sender, System.Net.DownloadProgressChangedEventArgs e);

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

        public void Download()
        {
            SystemEvents.PowerModeChanged += this.PowerModeChange;

            this.downloadClient = new WebClient();
            this.downloadClient.DownloadProgressChanged += this.downloadClient_DownloadProgressChanged;
            this.downloadClient.DownloadFileCompleted += this.downloadClient_DownloadFileCompleted;

            this.downloadClient.Headers.Add("user-agent", new ApplicationBase().Info.AssemblyName + " " + new ApplicationBase().Info.Version.ToString());
            this.downloadClient.DownloadFileAsync(this.downloadUrl, this.destPath);
        }

        public bool Complete
        {
            get { return this.downloadComplete; }
        }

        public Exception Error
        {
            get { return this.downloadError; }
        }

        private void downloadClient_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            if (this.DownloadProgress != null)
            {
                this.DownloadProgress(this, e);
            }
        }

        private void downloadClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
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
