/*
 * This file is part of Radio Downloader.
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

        private bool cancelled;
        private bool cancelResponse;

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

                    this.providerProgInfo = new ProgrammeInfo(this.progInfo);

                    if (reader.IsDBNull(reader.GetOrdinal("progimg")))
                    {
                        this.providerProgInfo.Image = null;
                    }
                    else
                    {
                        this.providerProgInfo.Image = Database.RetrieveImage(reader.GetInt32(reader.GetOrdinal("progimg")));
                    }

                    this.providerEpisodeInfo = new EpisodeInfo(this.episodeInfo);

                    if (reader.IsDBNull(reader.GetOrdinal("duration")))
                    {
                        this.providerEpisodeInfo.Duration = null;
                    }
                    else
                    {
                        this.providerEpisodeInfo.Duration = reader.GetInt32(reader.GetOrdinal("duration"));
                    }

                    if (reader.IsDBNull(reader.GetOrdinal("epimg")))
                    {
                        this.providerEpisodeInfo.Image = null;
                    }
                    else
                    {
                        this.providerEpisodeInfo.Image = Database.RetrieveImage(reader.GetInt32(reader.GetOrdinal("epimg")));
                    }

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

        public int ProgressValue { get; private set; }

        public void Start()
        {
            lock (this.downloadThreadLock)
            {
                this.downloadThread = new Thread(this.DownloadProgThread);
                this.downloadThread.IsBackground = true;
                this.downloadThread.Start();
            }
        }

        public bool Cancel()
        {
            lock (this.downloadThreadLock)
            {
                lock (this.pluginInstanceLock)
                {
                    this.cancelled = true;

                    if (this.downloadThread == null || !this.downloadThread.IsAlive)
                    {
                        return true;
                    }

                    this.pluginInstance.CancelDownload();

                    for (int wait = 0; wait < 20; wait++)
                    {
                        Thread.Sleep(500);

                        if (this.cancelResponse)
                        {
                            return true;
                        }
                    }

                    // No timely response to request, kill the thread
                    this.downloadThread.Abort();
                    return false;
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

            if (!Provider.Exists(this.pluginId))
            {
                this.DownloadError(ErrorType.LocalProblem, "The plugin provider required to download this episode is not currently available.  Please try updating the Radio Downloader providers or cancelling the download.");
                return;
            }

            lock (this.pluginInstanceLock)
            {
                this.pluginInstance = Provider.GetFromId(this.pluginId).CreateInstance();
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
                    this.DownloadError(ErrorType.LocalProblem, "Your chosen location for saving downloaded programmes no longer exists.  Select a new one under Options -> Main Options.");
                    return;
                }

                const int FreeMb = 250;
                ulong availableSpace = OsUtils.PathAvailableSpace(saveLocation);

                if (availableSpace <= FreeMb * 1024 * 1024)
                {
                    this.DownloadError(ErrorType.LocalProblem, "Your chosen location for saving downloaded programmes does not have enough free space.  Make sure that you have at least " + FreeMb.ToString(CultureInfo.CurrentCulture) + " MB free, or select a new location under Options -> Main Options.");
                    return;
                }

                try
                {
                    finalName = Model.Download.FindFreeSaveFileName(Settings.FileNameFormat, this.progInfo, this.episodeInfo, saveLocation);
                }
                catch (IOException ioExp)
                {
                    this.DownloadError(ErrorType.LocalProblem, "Encountered an error generating the download file name.  " + ioExp.Message + "  You may need to select a new location for saving downloaded programmes under Options -> Main Options.");
                    return;
                }
                catch (UnauthorizedAccessException unAuthExp)
                {
                    this.DownloadError(ErrorType.LocalProblem, "Encountered a permissions problem generating the download file name.  " + unAuthExp.Message + "  You may need to select a new location for saving downloaded programmes under Options -> Main Options.");
                    return;
                }

                fileExtension = this.pluginInstance.DownloadProgramme(this.progExtId, this.episodeExtId, this.providerProgInfo, this.providerEpisodeInfo, finalName);
            }
            catch (DownloadException downloadExp)
            {
                if (downloadExp.ErrorType == ErrorType.UnknownError)
                {
                    this.DownloadError(downloadExp);
                }
                else
                {
                    this.DownloadError(downloadExp.ErrorType, downloadExp.Message);
                }

                return;
            }
            catch (Exception unknownExp)
            {
                this.DownloadError(unknownExp);
                return;
            }

            if (this.cancelled)
            {
                this.cancelResponse = true;
            }

            finalName += "." + fileExtension;

            lock (Database.DbUpdateLock)
            {
                Model.Download.SetComplete(this.episodeInfo.Epid, finalName);
                Model.Programme.SetLatestDownload(this.progInfo.Progid, this.episodeInfo.Date);
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

        private void DownloadError(ErrorType errorType, string errorDetails)
        {
            Model.Download.SetErrorred(this.episodeInfo.Epid, errorType, errorDetails);
            this.DownloadFinished();
        }

        private void DownloadError(Exception unhandled)
        {
            string epDetails = this.episodeInfo.ToString() +
                "\r\nExtID: " + this.episodeExtId;

            string progDetails = this.progInfo.ToString() +
                "\r\nExtID: " + this.progExtId;

            unhandled.Data.Add("Episode", epDetails);
            unhandled.Data.Add("Programme", progDetails);
            unhandled.Data.Add("Provider", this.pluginInstance.ToString());

            Model.Download.SetErrorred(this.episodeInfo.Epid, ErrorType.UnknownError, new ErrorReporting("Download Error", unhandled));
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
