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
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;
    using Microsoft.VisualBasic;

    internal static class Data
    {
        private static Thread episodeListThread;

        private static object episodeListThreadLock = new object();
        private static IRadioProvider findNewPluginInst;

        public delegate void ProviderAddedEventHandler(Guid providerId);

        public delegate void FindNewViewChangeEventHandler(object viewData);

        public delegate void FoundNewEventHandler(int progid);

        public delegate void EpisodeAddedEventHandler(int epid);

        public static event ProviderAddedEventHandler ProviderAdded;

        public static event FindNewViewChangeEventHandler FindNewViewChange;

        public static event FoundNewEventHandler FoundNew;

        public static event EpisodeAddedEventHandler EpisodeAdded;

        public static Panel GetFindNewPanel(Guid pluginID, object view)
        {
            if (Plugins.PluginExists(pluginID))
            {
                findNewPluginInst = Plugins.GetPluginInstance(pluginID);
                findNewPluginInst.FindNewException += FindNewPluginInst_FindNewException;
                findNewPluginInst.FindNewViewChange += FindNewPluginInst_FindNewViewChange;
                findNewPluginInst.FoundNew += FindNewPluginInst_FoundNew;
                return findNewPluginInst.GetFindNewPanel(view);
            }
            else
            {
                return new Panel();
            }
        }

        public static void InitProviderList()
        {
            Guid[] pluginIdList = null;
            pluginIdList = Plugins.GetPluginIdList();

            foreach (Guid pluginId in pluginIdList)
            {
                if (ProviderAdded != null)
                {
                    ProviderAdded(pluginId);
                }
            }
        }

        public static void InitEpisodeList(int progid)
        {
            lock (episodeListThreadLock)
            {
                episodeListThread = new Thread(() => InitEpisodeListThread(progid));
                episodeListThread.IsBackground = true;
                episodeListThread.Start();
            }
        }

        public static void CancelEpisodeListing()
        {
            lock (episodeListThreadLock)
            {
                episodeListThread = null;
            }
        }

        public static ProviderData FetchProviderData(Guid providerId)
        {
            IRadioProvider providerInstance = Plugins.GetPluginInstance(providerId);

            ProviderData info = new ProviderData();
            info.Name = providerInstance.ProviderName;
            info.Description = providerInstance.ProviderDescription;
            info.Icon = providerInstance.ProviderIcon;
            info.ShowOptionsHandler = providerInstance.GetShowOptionsHandler();

            return info;
        }

        private static void FindNewPluginInst_FindNewException(Exception exception, bool unhandled)
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

        private static void FindNewPluginInst_FindNewViewChange(object view)
        {
            if (FindNewViewChange != null)
            {
                FindNewViewChange(view);
            }
        }

        private static void FindNewPluginInst_FoundNew(string progExtId)
        {
            ThreadPool.QueueUserWorkItem(delegate { FoundNewAsync(progExtId); });
        }

        private static void FoundNewAsync(string progExtId)
        {
            Guid pluginId = findNewPluginInst.ProviderId;
            int? progid = Model.Programme.FetchInfo(pluginId, progExtId);

            if (progid == null)
            {
                Interaction.MsgBox("There was a problem retrieving information about this programme.  You might like to try again later.", MsgBoxStyle.Exclamation);
                return;
            }

            if (FoundNew != null)
            {
                FoundNew(progid.Value);
            }
        }

        private static void InitEpisodeListThread(int progid)
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

                    lock (episodeListThreadLock)
                    {
                        if (!object.ReferenceEquals(Thread.CurrentThread, episodeListThread))
                        {
                            return;
                        }

                        if (EpisodeAdded != null)
                        {
                            EpisodeAdded(epid.Value);
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
