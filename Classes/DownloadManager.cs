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
    using System.Threading;

    internal class DownloadManager : Database
    {
        private const int MaxDownloads = 1;

        private static object findDownloadLock = new object();
        private static Dictionary<int, DownloadHandler> downloads = new Dictionary<int, DownloadHandler>();

        public delegate void ProgressEventHandler(int epid, int percent, string statusText, ProgressIcon icon);

        public delegate void ProgressTotalEventHandler(bool downloading, int percent);

        public static event ProgressEventHandler Progress;

        public static event ProgressTotalEventHandler ProgressTotal;

        public static void StartNextDownload()
        {
            ThreadPool.QueueUserWorkItem(delegate { StartNextDownloadAsync(); });
        }

        public static void CancelDownload(int epid)
        {
            lock (downloads)
            {
                if (downloads.ContainsKey(epid))
                {
                    downloads[epid].Cancel();
                }
            }
        }

        private static void StartNextDownloadAsync()
        {
            lock (findDownloadLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("select dl.status, ep.epid from downloads as dl, episodes as ep where dl.epid=ep.epid and (dl.status=@statuswait or (dl.status=@statuserr and dl.errortime < datetime('now', '-' || power(2, dl.errorcount) || ' hours'))) order by ep.date", FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@statuswait", Model.Download.DownloadStatus.Waiting));
                    command.Parameters.Add(new SQLiteParameter("@statuserr", Model.Download.DownloadStatus.Errored));

                    using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read() && downloads.Count < MaxDownloads)
                        {
                            int epid = reader.GetInt32(reader.GetOrdinal("epid"));

                            if (!downloads.ContainsKey(epid))
                            {
                                if ((Model.Download.DownloadStatus)reader.GetInt32(reader.GetOrdinal("status")) == Model.Download.DownloadStatus.Errored)
                                {
                                    Model.Download.ResetAsync(epid, true);
                                }

                                DownloadHandler download = new DownloadHandler(epid);

                                lock (downloads)
                                {
                                    downloads.Add(epid, download);

                                    download.Progress += DownloadHandler_Progress;
                                    download.Finished += DownloadHandler_Finished;

                                    download.Start();
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void DownloadHandler_Progress(int epid, int percent, string statusText, ProgressIcon icon)
        {
            if (Progress != null)
            {
                Progress(epid, percent, statusText, icon);
            }

            UpdateTotalProgress();
        }

        private static void DownloadHandler_Finished(int epid)
        {
            lock (downloads)
            {
                downloads.Remove(epid);
            }

            UpdateTotalProgress();
            StartNextDownload();
        }

        private static void UpdateTotalProgress()
        {
            bool downloading = false;
            int totalProgress = 0;

            lock (downloads)
            {
                if (downloads.Count > 0)
                {
                    downloading = true;

                    foreach (DownloadHandler download in downloads.Values)
                    {
                        totalProgress += download.ProgressValue;
                    }

                    totalProgress = totalProgress / downloads.Count;
                }
            }

            if (ProgressTotal != null)
            {
                ProgressTotal(downloading, totalProgress);
            }
        }
    }
}
