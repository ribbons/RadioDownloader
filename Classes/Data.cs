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
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;
    using Microsoft.VisualBasic;

    internal class Data : Database
    {
        private static Data dataInstance;
        private static object dataInstanceLock = new object();

        private Thread episodeListThread;

        private object episodeListThreadLock = new object();
        private IRadioProvider findNewPluginInst;

        private Data()
            : base()
        {
            // Start regularly checking for new subscriptions in the background
            ThreadPool.QueueUserWorkItem(delegate
            {
                // Wait for 2 minutes while the application starts
                Thread.Sleep(120000);
                this.CheckSubscriptions();
            });
        }

        public delegate void ProviderAddedEventHandler(Guid providerId);

        public delegate void FindNewViewChangeEventHandler(object viewData);

        public delegate void FoundNewEventHandler(int progid);

        public delegate void EpisodeAddedEventHandler(int epid);

        public event ProviderAddedEventHandler ProviderAdded;

        public event FindNewViewChangeEventHandler FindNewViewChange;

        public event FoundNewEventHandler FoundNew;

        public event EpisodeAddedEventHandler EpisodeAdded;

        public static Data GetInstance()
        {
            // Need to use a lock instead of declaring the instance variable as New, as otherwise
            // on first run the constructor gets called before the template database is in place
            lock (dataInstanceLock)
            {
                if (dataInstance == null)
                {
                    dataInstance = new Data();
                }

                return dataInstance;
            }
        }

        public Panel GetFindNewPanel(Guid pluginID, object view)
        {
            if (Plugins.PluginExists(pluginID))
            {
                this.findNewPluginInst = Plugins.GetPluginInstance(pluginID);
                this.findNewPluginInst.FindNewException += this.FindNewPluginInst_FindNewException;
                this.findNewPluginInst.FindNewViewChange += this.FindNewPluginInst_FindNewViewChange;
                this.findNewPluginInst.FoundNew += this.FindNewPluginInst_FoundNew;
                return this.findNewPluginInst.GetFindNewPanel(view);
            }
            else
            {
                return new Panel();
            }
        }

        public void InitProviderList()
        {
            Guid[] pluginIdList = null;
            pluginIdList = Plugins.GetPluginIdList();

            foreach (Guid pluginId in pluginIdList)
            {
                if (this.ProviderAdded != null)
                {
                    this.ProviderAdded(pluginId);
                }
            }
        }

        public void InitEpisodeList(int progid)
        {
            lock (this.episodeListThreadLock)
            {
                this.episodeListThread = new Thread(() => this.InitEpisodeListThread(progid));
                this.episodeListThread.IsBackground = true;
                this.episodeListThread.Start();
            }
        }

        public void CancelEpisodeListing()
        {
            lock (this.episodeListThreadLock)
            {
                this.episodeListThread = null;
            }
        }

        public ProviderData FetchProviderData(Guid providerId)
        {
            IRadioProvider providerInstance = Plugins.GetPluginInstance(providerId);

            ProviderData info = new ProviderData();
            info.Name = providerInstance.ProviderName;
            info.Description = providerInstance.ProviderDescription;
            info.Icon = providerInstance.ProviderIcon;
            info.ShowOptionsHandler = providerInstance.GetShowOptionsHandler();

            return info;
        }

        private void CheckSubscriptions()
        {
            // Fetch the current subscriptions into a list, so that the reader doesn't remain open while
            // checking all of the subscriptions, as this blocks writes to the database from other threads
            List<Model.Subscription> subscriptions = Model.Subscription.FetchAll();

            // Work through the list of subscriptions and check for new episodes
            using (SQLiteCommand progInfCmd = new SQLiteCommand("select pluginid, extid from programmes where progid=@progid", FetchDbConn()))
            {
                SQLiteParameter progidParam = new SQLiteParameter("@progid");
                progInfCmd.Parameters.Add(progidParam);

                foreach (Model.Subscription subscription in subscriptions)
                {
                    Guid providerId;
                    string progExtId = null;

                    progidParam.Value = subscription.Progid;

                    using (SQLiteMonDataReader progInfReader = new SQLiteMonDataReader(progInfCmd.ExecuteReader()))
                    {
                        if (!progInfReader.Read())
                        {
                            continue;
                        }

                        providerId = new Guid(progInfReader.GetString(progInfReader.GetOrdinal("pluginid")));
                        progExtId = progInfReader.GetString(progInfReader.GetOrdinal("extid"));
                    }

                    List<string> episodeExtIds = null;

                    try
                    {
                        episodeExtIds = this.GetAvailableEpisodes(providerId, progExtId);
                    }
                    catch (Exception)
                    {
                        // Catch any unhandled provider exceptions
                        continue;
                    }

                    if (episodeExtIds != null)
                    {
                        foreach (string episodeExtId in episodeExtIds)
                        {
                            int? epid = null;

                            try
                            {
                                epid = Model.Episode.FetchInfo(providerId, subscription.Progid, progExtId, episodeExtId);
                            }
                            catch
                            {
                                // Catch any unhandled provider exceptions
                                continue;
                            }

                            if (epid == null)
                            {
                                continue;
                            }

                            if (!subscription.SingleEpisode)
                            {
                                if (!new Model.Episode(epid.Value).AutoDownload)
                                {
                                    // Don't download the episode automatically, skip to the next one
                                    continue;
                                }
                            }

                            if (!Model.Download.IsDownload(epid.Value))
                            {
                                Model.Download.Add(new int[] { epid.Value });
                            }
                        }
                    }
                }
            }

            // Wait for 10 minutes to give a pause between each check for new episodes
            Thread.Sleep(600000);

            // Queue the next subscription check.  This is used instead of a loop
            // as it frees up a slot in the thread pool other actions are waiting.
            ThreadPool.QueueUserWorkItem(delegate { this.CheckSubscriptions(); });
        }

        private void FindNewPluginInst_FindNewException(Exception exception, bool unhandled)
        {
            if (unhandled)
            {
                ErrorReporting report = new ErrorReporting(exception);

                using (ReportError showError = new ReportError())
                {
                    showError.ShowReport(report);
                }
            }
            else
            {
                ErrorReporting reportException = new ErrorReporting("Find New Error", exception);
                reportException.SendReport();
            }
        }

        private void FindNewPluginInst_FindNewViewChange(object view)
        {
            if (this.FindNewViewChange != null)
            {
                this.FindNewViewChange(view);
            }
        }

        private void FindNewPluginInst_FoundNew(string progExtId)
        {
            ThreadPool.QueueUserWorkItem(delegate { this.FoundNewAsync(progExtId); });
        }

        private void FoundNewAsync(string progExtId)
        {
            Guid pluginId = this.findNewPluginInst.ProviderId;
            int? progid = Model.Programme.FetchInfo(pluginId, progExtId);

            if (progid == null)
            {
                Interaction.MsgBox("There was a problem retrieving information about this programme.  You might like to try again later.", MsgBoxStyle.Exclamation);
                return;
            }

            if (this.FoundNew != null)
            {
                this.FoundNew(progid.Value);
            }
        }

        private List<string> GetAvailableEpisodes(Guid providerId, string progExtId)
        {
            if (!Plugins.PluginExists(providerId))
            {
                return null;
            }

            string[] extIds = null;
            IRadioProvider providerInst = Plugins.GetPluginInstance(providerId);

            extIds = providerInst.GetAvailableEpisodeIds(progExtId);

            if (extIds == null)
            {
                return null;
            }

            // Remove any duplicates from the list of episodes
            List<string> extIdsUnique = new List<string>();

            foreach (string removeDups in extIds)
            {
                if (!extIdsUnique.Contains(removeDups))
                {
                    extIdsUnique.Add(removeDups);
                }
            }

            return extIdsUnique;
        }

        private void InitEpisodeListThread(int progid)
        {
            Guid providerId;
            string progExtId = null;

            using (SQLiteCommand command = new SQLiteCommand("select pluginid, extid from programmes where progid=@progid", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (!reader.Read())
                    {
                        return;
                    }

                    providerId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));
                    progExtId = reader.GetString(reader.GetOrdinal("extid"));
                }
            }

            List<string> episodeExtIDs = this.GetAvailableEpisodes(providerId, progExtId);

            if (episodeExtIDs != null)
            {
                foreach (string episodeExtId in episodeExtIDs)
                {
                    int? epid = null;

                    epid = Model.Episode.FetchInfo(providerId, progid, progExtId, episodeExtId);

                    if (epid == null)
                    {
                        continue;
                    }

                    lock (this.episodeListThreadLock)
                    {
                        if (!object.ReferenceEquals(Thread.CurrentThread, this.episodeListThread))
                        {
                            return;
                        }

                        if (this.EpisodeAdded != null)
                        {
                            this.EpisodeAdded(epid.Value);
                        }
                    }
                }
            }
        }

        public struct ProviderData
        {
            public string Name;
            public string Description;
            public Bitmap Icon;
            public EventHandler ShowOptionsHandler;
        }
    }
}
