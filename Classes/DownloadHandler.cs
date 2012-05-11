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
    using System.Data.SQLite;
    using System.Globalization;
    using System.IO;
    using System.Threading;

    internal class DownloadHandler : Database
    {
        private Guid pluginId;
        private string progExtId;
        private string episodeExtId;
        private Model.Programme progInfo;
        private Model.Episode episodeInfo;
        private ProgrammeInfo providerProgInfo;
        private EpisodeInfo providerEpisodeInfo;

        private IRadioProvider pluginInstance;
        private object pluginInstanceLock = new object();

        private Thread downloadThread;
        private object downloadThreadLock = new object();

        public DownloadHandler(int epid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select pr.progid, pluginid, pr.image as progimg, ep.duration, ep.image as epimg, pr.extid as progextid, ep.extid as epextid from episodes as ep, programmes as pr where ep.epid=@epid and ep.progid=pr.progid", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@epid", epid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (!reader.Read())
                    {
                        throw new DataNotFoundException(epid, "Episode does not exist");
                    }

                    this.pluginId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));
                    this.progExtId = reader.GetString(reader.GetOrdinal("progextid"));
                    this.episodeExtId = reader.GetString(reader.GetOrdinal("epextid"));

                    this.progInfo = new Model.Programme(reader.GetInt32(reader.GetOrdinal("progid")));
                    this.episodeInfo = new Model.Episode(epid);

                    this.providerProgInfo = new ProgrammeInfo();
                    this.providerProgInfo.Name = this.progInfo.Name;
                    this.providerProgInfo.Description = this.progInfo.Description;

                    if (reader.IsDBNull(reader.GetOrdinal("progimg")))
                    {
                        this.providerProgInfo.Image = null;
                    }
                    else
                    {
                        this.providerProgInfo.Image = RetrieveImage(reader.GetInt32(reader.GetOrdinal("progimg")));
                    }

                    this.providerEpisodeInfo = new EpisodeInfo();
                    this.providerEpisodeInfo.Name = this.episodeInfo.Name;
                    this.providerEpisodeInfo.Description = this.episodeInfo.Description;
                    this.providerEpisodeInfo.Date = this.episodeInfo.EpisodeDate;

                    if (reader.IsDBNull(reader.GetOrdinal("duration")))
                    {
                        this.providerEpisodeInfo.DurationSecs = null;
                    }
                    else
                    {
                        this.providerEpisodeInfo.DurationSecs = reader.GetInt32(reader.GetOrdinal("duration"));
                    }

                    if (reader.IsDBNull(reader.GetOrdinal("epimg")))
                    {
                        this.providerEpisodeInfo.Image = null;
                    }
                    else
                    {
                        this.providerEpisodeInfo.Image = RetrieveImage(reader.GetInt32(reader.GetOrdinal("epimg")));
                    }

                    this.providerEpisodeInfo.ExtInfo = new Dictionary<string, string>();

                    using (SQLiteCommand extCommand = new SQLiteCommand("select name, value from episodeext where epid=@epid", FetchDbConn()))
                    {
                        extCommand.Parameters.Add(new SQLiteParameter("@epid", epid));

                        using (SQLiteMonDataReader extReader = new SQLiteMonDataReader(extCommand.ExecuteReader()))
                        {
                            while (extReader.Read())
                            {
                                this.providerEpisodeInfo.ExtInfo.Add(extReader.GetString(extReader.GetOrdinal("name")), extReader.GetString(extReader.GetOrdinal("value")));
                            }
                        }
                    }
                }
            }
        }

        public delegate void FinishedEventHandler(int epid);

        public event DownloadManager.ProgressEventHandler Progress;

        public event FinishedEventHandler Finished;

        public int ProgressValue { get; set; }

        public void Start()
        {
            lock (this.downloadThreadLock)
            {
                this.downloadThread = new Thread(this.DownloadProgThread);
                this.downloadThread.IsBackground = true;
                this.downloadThread.Start();
            }
        }

        public void Cancel()
        {
            lock (this.downloadThreadLock)
            {
                if (this.downloadThread != null && this.downloadThread.IsAlive)
                {
                    this.downloadThread.Abort();
                }
            }
        }

        private void DownloadProgThread()
        {
            if (this.Progress != null)
            {
                // Raise a progress event to give the user some feedback
                this.Progress(this.episodeInfo.Epid, 0, ProgressType.Downloading);
            }

            if (!Plugins.PluginExists(this.pluginId))
            {
                this.DownloadError(ErrorType.LocalProblem, "The plugin provider required to download this episode is not currently available.  Please try updating Radio Downloader and providers and retrying the download.", null);
                return;
            }

            lock (this.pluginInstanceLock)
            {
                this.pluginInstance = Plugins.GetPluginInstance(this.pluginId);
                this.pluginInstance.Progress += this.DownloadPluginInst_Progress;
            }

            string finalName, fileExtension;

            try
            {
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
                    finalName = Model.Download.FindFreeSaveFileName(Settings.FileNameFormat, this.progInfo, this.episodeInfo, saveLocation);
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

                fileExtension = this.pluginInstance.DownloadProgramme(this.progExtId, this.episodeExtId, this.providerProgInfo, this.providerEpisodeInfo, finalName);
            }
            catch (ThreadAbortException)
            {
                // The download has been cancelled
                return;
            }
            catch (DownloadException downloadExp)
            {
                this.DownloadError(downloadExp.TypeOfError, downloadExp.Message, downloadExp.ErrorExtraDetails);
                return;
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
                return;
            }

            finalName += "." + fileExtension;

            lock (DbUpdateLock)
            {
                Model.Download.SetComplete(this.episodeInfo.Epid, finalName);
                Model.Programme.SetLatestDownload(this.progInfo.Progid, this.episodeInfo.EpisodeDate);
            }

            // Remove single episode subscriptions
            if (Model.Subscription.IsSubscribed(this.progInfo.Progid))
            {
                if (this.progInfo.SingleEpisode)
                {
                    Model.Subscription.Remove(this.progInfo.Progid);
                }
            }

            if (!string.IsNullOrEmpty(Settings.RunAfterCommand))
            {
                try
                {
                    // Use VB Interaction.Shell as Process.Start doesn't give the option of a non-focused window
                    // The "comspec" environment variable gives the path to cmd.exe
                    Microsoft.VisualBasic.Interaction.Shell("\"" + Environment.GetEnvironmentVariable("comspec") + "\" /c " + Settings.RunAfterCommand.Replace("%file%", finalName), Microsoft.VisualBasic.AppWinStyle.NormalNoFocus);
                }
                catch
                {
                    // Just ignore the error, as it just means that something has gone wrong with the run after command.
                }
            }

            this.DownloadFinished();
        }

        private void DownloadPluginInst_Progress(int percent, ProgressType type)
        {
            // Don't raise the progress event if the value is the same as last time
            if (percent == this.ProgressValue)
            {
                return;
            }

            if (percent < 0 || percent > 100)
            {
                throw new ArgumentOutOfRangeException("percent", percent, "Progress percentage must be between 0 and 100!");
            }

            this.ProgressValue = percent;

            if (this.Progress != null)
            {
                this.Progress(this.episodeInfo.Epid, percent, type);
            }
        }

        private void DownloadError(ErrorType errorType, string errorDetails, List<DldErrorDataItem> furtherDetails)
        {
            Model.Download.SetErrorred(this.episodeInfo.Epid, errorType, errorDetails, furtherDetails);
            this.DownloadFinished();
        }

        private void DownloadFinished()
        {
            lock (this.pluginInstanceLock)
            {
                if (this.pluginInstance != null)
                {
                    this.pluginInstance.Progress -= this.DownloadPluginInst_Progress;
                }
            }

            if (this.Finished != null)
            {
                this.Finished(this.episodeInfo.Epid);
            }
        }
    }
}
