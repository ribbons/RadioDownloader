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

        private void InitEpisodeListThread(int progid)
        {
            List<string> episodeExtIDs = Model.Programme.GetAvailableEpisodes(progid);

            if (episodeExtIDs != null)
            {
                foreach (string episodeExtId in episodeExtIDs)
                {
                    int? epid = null;

                    epid = Model.Episode.FetchInfo(progid, episodeExtId);

                    if (epid == null)
                    {
                        continue;
                    }

                    Model.Episode.UpdateInfoIfRequired(epid.Value);

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
