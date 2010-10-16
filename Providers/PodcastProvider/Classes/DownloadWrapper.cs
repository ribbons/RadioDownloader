using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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


using System.Net;

using Microsoft.Win32;
namespace PodcastProvider
{

	internal class DownloadWrapper
	{
		public event DownloadProgressEventHandler DownloadProgress;
		public delegate void DownloadProgressEventHandler(object sender, System.Net.DownloadProgressChangedEventArgs e);
		private WebClient withEventsField_downloadClient;

		private WebClient downloadClient {
			get { return withEventsField_downloadClient; }
			set {
				if (withEventsField_downloadClient != null) {
					withEventsField_downloadClient.DownloadProgressChanged -= downloadClient_DownloadProgressChanged;
					withEventsField_downloadClient.DownloadFileCompleted -= downloadClient_DownloadFileCompleted;
				}
				withEventsField_downloadClient = value;
				if (withEventsField_downloadClient != null) {
					withEventsField_downloadClient.DownloadProgressChanged += downloadClient_DownloadProgressChanged;
					withEventsField_downloadClient.DownloadFileCompleted += downloadClient_DownloadFileCompleted;
				}
			}

		}
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
			SystemEvents.PowerModeChanged += PowerModeChange;
            
			downloadClient = new WebClient();
            downloadClient.Headers.Add("user-agent", new My.MyApplication().Info.AssemblyName + " " + new My.MyApplication().Info.Version.ToString());
			downloadClient.DownloadFileAsync(downloadUrl, destPath);
		}

		public bool Complete {
			get { return downloadComplete; }
		}

		public Exception Error {
			get { return downloadError; }
		}

		private void downloadClient_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
		{
			if (DownloadProgress != null) {
				DownloadProgress(this, e);
			}
		}

		private void downloadClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			if (e.Cancelled == false) {
				SystemEvents.PowerModeChanged -= PowerModeChange;

				if (e.Error != null) {
					downloadError = e.Error;
				} else {
					downloadComplete = true;
				}
			}
		}

		private void PowerModeChange(object sender, PowerModeChangedEventArgs e)
		{
			if (e.Mode == PowerModes.Resume) {
				// Restart the download, as it is quite likely to have hung during the suspend / hibernate
				if (downloadClient.IsBusy) {
					downloadClient.CancelAsync();

					// Pause for 30 seconds to be give the pc a chance to settle down after the suspend. 
					System.Threading.Thread.Sleep(30000);

					// Restart the download
					downloadClient.DownloadFileAsync(downloadUrl, destPath);
				}
			}
		}
	}
}
