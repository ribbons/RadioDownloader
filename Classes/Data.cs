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
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml.Serialization;
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
                using (SQLiteCommand checkCmd = new SQLiteCommand("select epid from downloads where epid=@epid", FetchDbConn()))
                {
                    using (SQLiteCommand findCmd = new SQLiteCommand("select epid, autodownload from episodes where progid=@progid and extid=@extid", FetchDbConn()))
                    {
                        SQLiteParameter epidParam = new SQLiteParameter("@epid");
                        SQLiteParameter progidParam = new SQLiteParameter("@progid");
                        SQLiteParameter extidParam = new SQLiteParameter("@extid");

                        progInfCmd.Parameters.Add(progidParam);
                        findCmd.Parameters.Add(progidParam);
                        findCmd.Parameters.Add(extidParam);
                        checkCmd.Parameters.Add(epidParam);

                        foreach (Model.Subscription subscription in subscriptions)
                        {
                            Guid providerId = default(Guid);
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
                                    extidParam.Value = episodeExtId;

                                    bool needEpInfo = true;
                                    int epid = 0;

                                    using (SQLiteMonDataReader findReader = new SQLiteMonDataReader(findCmd.ExecuteReader()))
                                    {
                                        if (findReader.Read())
                                        {
                                            needEpInfo = false;
                                            epid = findReader.GetInt32(findReader.GetOrdinal("epid"));

                                            if (!subscription.SingleEpisode)
                                            {
                                                if (findReader.GetInt32(findReader.GetOrdinal("autodownload")) != 1)
                                                {
                                                    // Don't download the episode automatically, skip to the next one
                                                    continue;
                                                }
                                            }
                                        }
                                    }

                                    if (needEpInfo)
                                    {
                                        try
                                        {
                                            epid = this.StoreEpisodeInfo(providerId, subscription.Progid, progExtId, episodeExtId);
                                        }
                                        catch
                                        {
                                            // Catch any unhandled provider exceptions
                                            continue;
                                        }

                                        if (epid < 0)
                                        {
                                            continue;
                                        }
                                    }

                                    epidParam.Value = epid;

                                    using (SQLiteMonDataReader checkRdr = new SQLiteMonDataReader(checkCmd.ExecuteReader()))
                                    {
                                        if (!checkRdr.Read())
                                        {
                                            Model.Download.Add(new int[] { epid });
                                        }
                                    }
                                }
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

        private int StoreEpisodeInfo(Guid pluginId, int progid, string progExtId, string episodeExtId)
        {
            IRadioProvider providerInst = Plugins.GetPluginInstance(pluginId);
            GetEpisodeInfoReturn episodeInfoReturn = default(GetEpisodeInfoReturn);

            episodeInfoReturn = providerInst.GetEpisodeInfo(progExtId, episodeExtId);

            if (!episodeInfoReturn.Success)
            {
                return -1;
            }

            if (string.IsNullOrEmpty(episodeInfoReturn.EpisodeInfo.Name))
            {
                throw new InvalidDataException("Episode name cannot be null or an empty string");
            }

            if (episodeInfoReturn.EpisodeInfo.Date == null)
            {
                // The date of the episode isn't known, so use the current date
                episodeInfoReturn.EpisodeInfo.Date = DateTime.Now;
            }

            lock (Database.DbUpdateLock)
            {
                using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(FetchDbConn().BeginTransaction()))
                {
                    int epid = 0;

                    using (SQLiteCommand addEpisodeCmd = new SQLiteCommand("insert into episodes (progid, extid, name, description, duration, date, image) values (@progid, @extid, @name, @description, @duration, @date, @image)", FetchDbConn(), transMon.Trans))
                    {
                        addEpisodeCmd.Parameters.Add(new SQLiteParameter("@progid", progid));
                        addEpisodeCmd.Parameters.Add(new SQLiteParameter("@extid", episodeExtId));
                        addEpisodeCmd.Parameters.Add(new SQLiteParameter("@name", episodeInfoReturn.EpisodeInfo.Name));
                        addEpisodeCmd.Parameters.Add(new SQLiteParameter("@description", episodeInfoReturn.EpisodeInfo.Description));
                        addEpisodeCmd.Parameters.Add(new SQLiteParameter("@duration", episodeInfoReturn.EpisodeInfo.DurationSecs));
                        addEpisodeCmd.Parameters.Add(new SQLiteParameter("@date", episodeInfoReturn.EpisodeInfo.Date));
                        addEpisodeCmd.Parameters.Add(new SQLiteParameter("@image", StoreImage(episodeInfoReturn.EpisodeInfo.Image)));
                        addEpisodeCmd.ExecuteNonQuery();
                    }

                    using (SQLiteCommand getRowIDCmd = new SQLiteCommand("select last_insert_rowid()", FetchDbConn(), transMon.Trans))
                    {
                        epid = (int)(long)getRowIDCmd.ExecuteScalar();
                    }

                    if (episodeInfoReturn.EpisodeInfo.ExtInfo != null)
                    {
                        using (SQLiteCommand addExtInfoCmd = new SQLiteCommand("insert into episodeext (epid, name, value) values (@epid, @name, @value)", FetchDbConn(), transMon.Trans))
                        {
                            foreach (KeyValuePair<string, string> extItem in episodeInfoReturn.EpisodeInfo.ExtInfo)
                            {
                                addExtInfoCmd.Parameters.Add(new SQLiteParameter("@epid", epid));
                                addExtInfoCmd.Parameters.Add(new SQLiteParameter("@name", extItem.Key));
                                addExtInfoCmd.Parameters.Add(new SQLiteParameter("@value", extItem.Value));
                                addExtInfoCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    transMon.Trans.Commit();
                    return epid;
                }
            }
        }

        private void InitEpisodeListThread(int progid)
        {
            Guid providerId = default(Guid);
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
                using (SQLiteCommand findCmd = new SQLiteCommand("select epid from episodes where progid=@progid and extid=@extid", FetchDbConn()))
                {
                    SQLiteParameter progidParam = new SQLiteParameter("@progid");
                    SQLiteParameter extidParam = new SQLiteParameter("@extid");

                    findCmd.Parameters.Add(progidParam);
                    findCmd.Parameters.Add(extidParam);

                    foreach (string episodeExtId in episodeExtIDs)
                    {
                        progidParam.Value = progid;
                        extidParam.Value = episodeExtId;

                        bool needEpInfo = true;
                        int epid = 0;

                        using (SQLiteMonDataReader reader = new SQLiteMonDataReader(findCmd.ExecuteReader()))
                        {
                            if (reader.Read())
                            {
                                needEpInfo = false;
                                epid = reader.GetInt32(reader.GetOrdinal("epid"));
                            }
                        }

                        if (needEpInfo)
                        {
                            epid = this.StoreEpisodeInfo(providerId, progid, progExtId, episodeExtId);

                            if (epid < 0)
                            {
                                continue;
                            }
                        }

                        lock (this.episodeListThreadLock)
                        {
                            if (!object.ReferenceEquals(Thread.CurrentThread, this.episodeListThread))
                            {
                                return;
                            }

                            if (this.EpisodeAdded != null)
                            {
                                this.EpisodeAdded(epid);
                            }
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
