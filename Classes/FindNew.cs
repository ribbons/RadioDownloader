/*
 * Copyright © 2008-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
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
        private static Provider.RadioProvider findNewPluginInst;

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
            if (Provider.Handler.Exists(pluginID))
            {
                findNewPluginInst = Provider.Handler.GetFromId(pluginID).CreateInstance();
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
            FindNewViewChange?.Invoke(view);
        }

        private static void FindNewPluginInst_FoundNew(string progExtId)
        {
            ThreadPool.QueueUserWorkItem(state => { FoundNewAsync(progExtId); });
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
                FindNewFailed?.Invoke();

                if (MessageBox.Show("There was an unknown error encountered retrieving information about this programme." + Environment.NewLine + Environment.NewLine + "Would you like to send an error report to NerdoftheHerd.com to help improve the " + provExp.ProviderName + " provider?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    ErrorReporting report = provExp.BuildReport();
                    report.SendReport();
                }

                return;
            }

            if (progid == null)
            {
                FindNewFailed?.Invoke();

                MessageBox.Show("There was a temporary problem retrieving information about this programme.  Please try again later.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            FoundNew?.Invoke(progid.Value);
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
                            if (!ReferenceEquals(Thread.CurrentThread, episodeListThread))
                            {
                                return;
                            }

                            EpisodeAdded?.Invoke(epid.Value);
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
