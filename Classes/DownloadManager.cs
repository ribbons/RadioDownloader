/*
 * Copyright © 2007-2019 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Threading;

    internal class DownloadManager : Database
    {
        private static Queue<int> downloadQueue = new Queue<int>();
        private static Dictionary<int, DownloadHandler> downloads = new Dictionary<int, DownloadHandler>();
        private static List<int> startedDownloads = new List<int>();

        private DownloadManager()
        {
            // Prevent instance creation - would be a static class if we didn't need to inherit from Database
        }

        public delegate void ProgressEventHandler(int epid, int percent, Provider.ProgressType type);

        public delegate void ProgressTotalEventHandler(bool downloading, int percent);

        public static event ProgressEventHandler Progress;

        public static event ProgressTotalEventHandler ProgressTotal;

        public static void ResumeDownloads()
        {
            ThreadPool.QueueUserWorkItem(delegate { ResumeDownloadsAsync(); });
        }

        public static void AddDownloads(int[] epids)
        {
            lock (downloads)
            {
                foreach (int epid in epids)
                {
                    DownloadHandler download = new DownloadHandler(epid);

                    downloads.Add(epid, download);
                    downloadQueue.Enqueue(epid);

                    download.Progress += DownloadHandler_Progress;
                    download.Finished += DownloadHandler_Finished;
                }
            }

            StartNextDownload();
        }

        public static bool CancelDownload(int epid)
        {
            lock (downloads)
            {
                if (!downloads.ContainsKey(epid))
                {
                    return true;
                }

                if (!downloads[epid].Cancel())
                {
                    return false;
                }

                downloads.Remove(epid);
                startedDownloads.Remove(epid);
            }

            UpdateTotalProgress();
            StartNextDownload();
            return true;
        }

        private static void ResumeDownloadsAsync()
        {
            List<int> epids = new List<int>();

            lock (downloads)
            {
                using (SQLiteCommand command = new SQLiteCommand("select episodes.epid from downloads, episodes where downloads.epid=episodes.epid and status=@statuswait order by date", FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@statuswait", Model.Download.DownloadStatus.Waiting));

                    using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                    {
                        int epidOrdinal = reader.GetOrdinal("epid");

                        while (reader.Read())
                        {
                            epids.Add(reader.GetInt32(epidOrdinal));
                        }
                    }
                }

                if (epids.Count > 0)
                {
                    AddDownloads(epids.ToArray());
                }
            }

            RetryErrored();
        }

        private static void RetryErrored()
        {
            List<int> epids = new List<int>();

            using (SQLiteCommand command = new SQLiteCommand("select epid from downloads where status=@statuserr and errortime < datetime('now', '-' || (1 << errorcount) || ' hours')", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@statuserr", Model.Download.DownloadStatus.Errored));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    int epidOrdinal = reader.GetOrdinal("epid");

                    while (reader.Read())
                    {
                        epids.Add(reader.GetInt32(epidOrdinal));
                    }
                }
            }

            foreach (int epid in epids)
            {
                Model.Download.ResetAsync(epid, true);
            }

            ThreadPool.QueueUserWorkItem(delegate
            {
                // Wait for 30 minutes, and check again
                Thread.Sleep(1800000);
                RetryErrored();
            });
        }

        private static void StartNextDownload()
        {
            lock (downloads)
            {
                while (downloadQueue.Count > 0 && startedDownloads.Count < Settings.ParallelDownloads)
                {
                    int epid = downloadQueue.Dequeue();

                    // If the download hasn't been cancelled while waiting, start it
                    if (downloads.ContainsKey(epid))
                    {
                        downloads[epid].Start();
                        startedDownloads.Add(epid);
                    }
                }
            }
        }

        private static void DownloadHandler_Progress(int epid, int percent, Provider.ProgressType type)
        {
            Progress?.Invoke(epid, percent, type);
            UpdateTotalProgress();
        }

        private static void DownloadHandler_Finished(int epid)
        {
            lock (downloads)
            {
                downloads.Remove(epid);
                startedDownloads.Remove(epid);
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

                    foreach (int epid in startedDownloads)
                    {
                        totalProgress += downloads[epid].ProgressValue;
                    }

                    totalProgress = totalProgress / downloads.Count;
                }
            }

            ProgressTotal?.Invoke(downloading, totalProgress);
        }
    }
}
