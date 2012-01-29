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
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading;

    internal class DownloadHandler : Database
    {
        private IRadioProvider downloadPluginInst;
        private Thread downloadThread;

        private string finalName;

        public delegate void FinishedEventHandler(int epid);

        public event DownloadManager.ProgressEventHandler Progress;

        public event FinishedEventHandler Finished;

        public Guid PluginId { get; set; }

        public string ProgExtId { get; set; }

        public string EpisodeExtId { get; set; }

        public Model.Programme ProgInfo { get; set; }

        public Model.Episode EpisodeInfo { get; set; }

        public ProgrammeInfo ProviderProgInfo { get; set; }

        public EpisodeInfo ProviderEpisodeInfo { get; set; }

        public int ProgressValue { get; set; }

        public void Start()
        {
            this.downloadThread = new Thread(this.DownloadProgThread);
            this.downloadThread.IsBackground = true;
            this.downloadThread.Start();
        }

        public void Cancel()
        {
            if (this.downloadThread != null && this.downloadThread.IsAlive)
            {
                this.downloadThread.Abort();
            }

            this.DownloadFinished();
        }

        private void DownloadProgThread()
        {
            this.downloadPluginInst = Plugins.GetPluginInstance(this.PluginId);
            this.downloadPluginInst.Finished += this.DownloadPluginInst_Finished;
            this.downloadPluginInst.Progress += this.DownloadPluginInst_Progress;

            try
            {
                // Make sure that the temp folder still exists
                Directory.CreateDirectory(Path.Combine(System.IO.Path.GetTempPath(), "RadioDownloader"));

                string saveLocation;

                try
                {
                    saveLocation = FileUtils.GetSaveFolder();
                }
                catch (DirectoryNotFoundException)
                {
                    this.DownloadError(ErrorType.LocalProblem, "Your chosen location for saving downloaded programmes no longer exists.  Select a new one under Options -> Main Options.", null);
                    return;
                }

                const int FreeMb = 250;
                ulong availableSpace = OsUtils.PathAvailableSpace(saveLocation);

                if (availableSpace <= FreeMb * 1024 * 1024)
                {
                    this.DownloadError(ErrorType.LocalProblem, "Your chosen location for saving downloaded programmes does not have enough free space.  Make sure that you have at least " + FreeMb.ToString(CultureInfo.CurrentCulture) + " MB free, or select a new location under Options -> Main Options.", null);
                    return;
                }

                try
                {
                    this.finalName = Model.Download.FindFreeSaveFileName(Settings.FileNameFormat, this.ProgInfo, this.EpisodeInfo, saveLocation);
                }
                catch (IOException ioExp)
                {
                    this.DownloadError(ErrorType.LocalProblem, "Encountered an error generating the download file name.  " + ioExp.Message + "  You may need to select a new location for saving downloaded programmes under Options -> Main Options.", null);
                    return;
                }
                catch (UnauthorizedAccessException unAuthExp)
                {
                    this.DownloadError(ErrorType.LocalProblem, "Encountered a permissions problem generating the download file name.  " + unAuthExp.Message + "  You may need to select a new location for saving downloaded programmes under Options -> Main Options.", null);
                    return;
                }

                this.downloadPluginInst.DownloadProgramme(this.ProgExtId, this.EpisodeExtId, this.ProviderProgInfo, this.ProviderEpisodeInfo, this.finalName);
            }
            catch (ThreadAbortException)
            {
                // The download has been aborted, so ignore the exception
            }
            catch (DownloadException downloadExp)
            {
                this.DownloadError(downloadExp.TypeOfError, downloadExp.Message, downloadExp.ErrorExtraDetails);
            }
            catch (Exception unknownExp)
            {
                List<DldErrorDataItem> extraDetails = new List<DldErrorDataItem>();
                extraDetails.Add(new DldErrorDataItem("error", unknownExp.GetType().ToString() + ": " + unknownExp.Message));
                extraDetails.Add(new DldErrorDataItem("exceptiontostring", unknownExp.ToString()));

                if (unknownExp.Data != null)
                {
                    foreach (DictionaryEntry dataEntry in unknownExp.Data)
                    {
                        if (object.ReferenceEquals(dataEntry.Key.GetType(), typeof(string)) && object.ReferenceEquals(dataEntry.Value.GetType(), typeof(string)))
                        {
                            extraDetails.Add(new DldErrorDataItem("expdata:Data:" + (string)dataEntry.Key, (string)dataEntry.Value));
                        }
                    }
                }

                this.DownloadError(ErrorType.UnknownError, unknownExp.GetType().ToString() + Environment.NewLine + unknownExp.StackTrace, extraDetails);
            }
        }

        private void DownloadPluginInst_Progress(int percent, string statusText, ProgressIcon icon)
        {
            // Don't raise the progress event if the value is the same as last time, or is outside the range
            if (percent == this.ProgressValue || percent < 0 || percent > 100)
            {
                return;
            }

            this.ProgressValue = percent;

            if (this.Progress != null)
            {
                this.Progress(this.EpisodeInfo.Epid, percent, statusText, icon);
            }
        }

        private void DownloadPluginInst_Finished(string fileExtension)
        {
            this.finalName += "." + fileExtension;

            lock (DbUpdateLock)
            {
                Model.Download.SetComplete(this.EpisodeInfo.Epid, this.finalName);
                Model.Programme.SetLatestDownload(this.ProgInfo.Progid, this.EpisodeInfo.EpisodeDate);
            }

            // Remove single episode subscriptions
            if (Model.Subscription.IsSubscribed(this.ProgInfo.Progid))
            {
                if (this.ProgInfo.SingleEpisode)
                {
                    Model.Subscription.Remove(this.ProgInfo.Progid);
                }
            }

            if (!string.IsNullOrEmpty(Settings.RunAfterCommand))
            {
                try
                {
                    // Use VB Interaction.Shell as Process.Start doesn't give the option of a non-focused window
                    // The "comspec" environment variable gives the path to cmd.exe
                    Microsoft.VisualBasic.Interaction.Shell("\"" + Environment.GetEnvironmentVariable("comspec") + "\" /c " + Settings.RunAfterCommand.Replace("%file%", this.finalName), Microsoft.VisualBasic.AppWinStyle.NormalNoFocus);
                }
                catch
                {
                    // Just ignore the error, as it just means that something has gone wrong with the run after command.
                }
            }

            this.DownloadFinished();
        }

        private void DownloadError(ErrorType errorType, string errorDetails, List<DldErrorDataItem> furtherDetails)
        {
            Model.Download.SetErrorred(this.EpisodeInfo.Epid, errorType, errorDetails, furtherDetails);
            this.DownloadFinished();
        }

        private void DownloadFinished()
        {
            this.downloadPluginInst.Finished -= this.DownloadPluginInst_Finished;
            this.downloadPluginInst.Progress -= this.DownloadPluginInst_Progress;

            if (this.Finished != null)
            {
                this.Finished(this.EpisodeInfo.Epid);
            }
        }
    }
}
