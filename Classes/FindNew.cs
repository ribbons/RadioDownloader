/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2013 by the authors - see the AUTHORS file for details.
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
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows.Forms;

    internal static class FindNew
    {
        private static Thread episodeListThread;

        private static object episodeListThreadLock = new object();
        private static IRadioProvider findNewPluginInst;

        public delegate void FindNewViewChangeEventHandler(object viewData);

        public delegate void FindNewFailedEventHandler();

        public delegate void FoundNewEventHandler(int progid);

        public delegate void EpisodeAddedEventHandler(int epid);

        public static event FindNewViewChangeEventHandler FindNewViewChange;

        public static event FindNewFailedEventHandler FindNewFailed;

        public static event FoundNewEventHandler FoundNew;

        public static event EpisodeAddedEventHandler EpisodeAdded;

        public static Panel GetFindNewPanel(Guid pluginID, object view)
        {
            if (Provider.Exists(pluginID))
            {
                findNewPluginInst = Provider.GetFromId(pluginID).CreateInstance();
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
            int? progid;

            try
            {
                progid = Model.Programme.FetchInfo(pluginId, progExtId);
            }
            catch (ProviderException provExp)
            {
                if (FindNewFailed != null)
                {
                    FindNewFailed();
                }

                if (MessageBox.Show("There was an unknown error encountered retrieving information about this programme." + Environment.NewLine + Environment.NewLine + "Would you like to send an error report to NerdoftheHerd.com to help improve the " + provExp.ProviderName + " provider?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    ErrorReporting report = provExp.BuildReport();
                    report.SendReport();
                }

                return;
            }

            if (progid == null)
            {
                if (FindNewFailed != null)
                {
                    FindNewFailed();
                }

                MessageBox.Show("There was a temporary problem retrieving information about this programme.  Please try again later.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (FoundNew != null)
            {
                FoundNew(progid.Value);
            }
        }

        private static void InitEpisodeListThread(int progid)
        {
            try
            {
                List<string> episodeExtIDs = Model.Programme.GetAvailableEpisodes(progid, true);

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
            catch (ProviderException provExp)
            {
                if (MessageBox.Show("There was an unknown error encountered fetching the list of available episodes." + Environment.NewLine + Environment.NewLine + "Would you like to send an error report to NerdoftheHerd.com to help improve the " + provExp.ProviderName + " provider?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    ErrorReporting report = provExp.BuildReport();
                    report.SendReport();
                }

                return;
            }
        }
    }
}
