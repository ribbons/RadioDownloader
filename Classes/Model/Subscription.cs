/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2019 by the authors - see the AUTHORS file for details.
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

namespace RadioDld.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Threading;

    internal class Subscription : Programme
    {
        private static Dictionary<int, int> sortCache;
        private static object sortCacheLock = new object();

        private static SubscriptionCols sortBy = SubscriptionCols.ProgrammeName;
        private static bool sortAsc;

        static Subscription()
        {
            Programme.Updated += Programme_Updated;

            // Start regularly checking for new subscriptions in the background
            ThreadPool.QueueUserWorkItem(delegate
            {
                // Wait for 2 minutes while the application starts
                Thread.Sleep(120000);
                CheckSubscriptions();
            });
        }

        public Subscription(SQLiteMonDataReader reader)
            : base(reader)
        {
        }

        public Subscription(int progid)
            : base(progid)
        {
        }

        public static event ProgrammeEventHandler Added;

        public static new event ProgrammeEventHandler Updated;

        public static event ProgrammeEventHandler Removed;

        internal enum SubscriptionCols
        {
            ProgrammeName = 0,
            LastDownload = 1,
            Provider = 2
        }

        public static SubscriptionCols SortByColumn
        {
            get
            {
                return sortBy;
            }

            set
            {
                lock (sortCacheLock)
                {
                    if (value != sortBy)
                    {
                        sortCache = null;
                    }

                    sortBy = value;
                }
            }
        }

        public static bool SortAscending
        {
            get
            {
                return sortAsc;
            }

            set
            {
                lock (sortCacheLock)
                {
                    if (value != sortAsc)
                    {
                        sortCache = null;
                    }

                    sortAsc = value;
                }
            }
        }

        public static List<Subscription> FetchAll()
        {
            List<Subscription> items = new List<Subscription>();

            using (SQLiteCommand command = new SQLiteCommand("select " + Columns + " from subscriptions, programmes where subscriptions.progid=programmes.progid", FetchDbConn()))
            {
                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    while (reader.Read())
                    {
                        items.Add(new Subscription(reader));
                    }
                }
            }

            return items;
        }

        public static bool IsSubscribed(int progid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select count(*) from subscriptions where progid=@progid", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));
                return (long)command.ExecuteScalar() != 0;
            }
        }

        public static bool Add(int progid)
        {
            Programme progInfo = new Programme(progid);

            if (progInfo.SingleEpisode)
            {
                using (SQLiteCommand command = new SQLiteCommand("select downloads.epid from downloads, episodes where downloads.epid=episodes.epid and progid=@progid", FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("progid", progid));

                    using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            // The one download for this programme is already in the downloads list
                            return false;
                        }
                    }
                }
            }

            ThreadPool.QueueUserWorkItem(delegate { AddAsync(progid); });

            return true;
        }

        public static void Remove(int progid)
        {
            ThreadPool.QueueUserWorkItem(delegate { RemoveAsync(progid); });
        }

        public static int Compare(int progid1, int progid2)
        {
            lock (sortCacheLock)
            {
                if (sortCache == null || !sortCache.ContainsKey(progid1) || !sortCache.ContainsKey(progid2))
                {
                    // The sort cache is either empty or missing one of the values that are required, so recreate it
                    sortCache = new Dictionary<int, int>();

                    int sort = 0;
                    string orderBy = null;

                    switch (sortBy)
                    {
                        case SubscriptionCols.ProgrammeName:
                            orderBy = "name" + (sortAsc ? string.Empty : " desc");
                            break;
                        case SubscriptionCols.LastDownload:
                            orderBy = "latestdownload" + (sortAsc ? string.Empty : " desc");
                            break;
                        case SubscriptionCols.Provider:
                            orderBy = "pluginid" + (sortAsc ? " desc" : string.Empty);
                            break;
                        default:
                            throw new InvalidDataException("Invalid column: " + sortBy.ToString());
                    }

                    using (SQLiteCommand command = new SQLiteCommand("select subscriptions.progid from subscriptions, programmes where programmes.progid=subscriptions.progid order by " + orderBy, FetchDbConn()))
                    {
                        using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                        {
                            int progidOrdinal = reader.GetOrdinal("progid");

                            while (reader.Read())
                            {
                                sortCache.Add(reader.GetInt32(progidOrdinal), sort);
                                sort += 1;
                            }
                        }
                    }
                }

                return sortCache[progid1] - sortCache[progid2];
            }
        }

        private static void Programme_Updated(int progid)
        {
            if (IsSubscribed(progid))
            {
                lock (sortCacheLock)
                {
                    sortCache = null;
                }

                Updated?.Invoke(progid);
            }
        }

        private static void AddAsync(int progid)
        {
            lock (DbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("insert into subscriptions (progid) values (@progid)", FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@progid", progid));

                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (SQLiteException sqliteExp)
                    {
                        if (sqliteExp.ErrorCode == SQLiteErrorCode.Constraint)
                        {
                            // Already added while this was waiting in the threadpool
                            return;
                        }

                        throw;
                    }
                }
            }

            Added?.Invoke(progid);
            RaiseUpdated(progid);
        }

        private static void RemoveAsync(int progid)
        {
            lock (DbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("delete from subscriptions where progid=@progid", FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@progid", progid));

                    if (command.ExecuteNonQuery() == 0)
                    {
                        // Subscription has already been removed
                        return;
                    }
                }
            }

            RaiseUpdated(progid);
            Removed?.Invoke(progid);
        }

        private static void CheckSubscriptions()
        {
            // Fetch the current subscriptions into a list, so that the reader doesn't remain open while
            // checking all of the subscriptions, as this blocks writes to the database from other threads
            List<Subscription> subscriptions = FetchAll();

            // Work through the list of subscriptions and check for new episodes
            foreach (Subscription subscription in subscriptions)
            {
                List<string> episodeExtIds = null;

                try
                {
                    episodeExtIds = GetAvailableEpisodes(subscription.Progid, false);
                }
                catch (ProviderException)
                {
                    // Ignore any unhandled provider exceptions
                    continue;
                }

                if (episodeExtIds != null)
                {
                    foreach (string episodeExtId in episodeExtIds)
                    {
                        int? epid = null;

                        try
                        {
                            epid = Episode.FetchInfo(subscription.Progid, episodeExtId);
                        }
                        catch (ProviderException)
                        {
                            // Ignore any unhandled provider exceptions
                            continue;
                        }

                        if (epid == null)
                        {
                            continue;
                        }

                        try
                        {
                            Episode.UpdateInfoIfRequired(epid.Value);
                        }
                        catch (ProviderException)
                        {
                            // Ignore any unhandled provider exceptions
                            continue;
                        }

                        if (!subscription.SingleEpisode)
                        {
                            Episode info = new Episode(epid.Value);

                            if (!info.AutoDownload)
                            {
                                // Don't download the episode automatically, skip to the next one
                                continue;
                            }
                            else if (info.Date.AddDays(14) < DateTime.Now)
                            {
                                // Stop checking the episodes for this subscription, this and the rest
                                // are past the automatic download threshold
                                break;
                            }
                        }

                        if (!Download.IsDownload(epid.Value))
                        {
                            Download.Add(new int[] { epid.Value });
                        }
                    }
                }
            }

            // Wait for 10 minutes to give a pause between each check for new episodes
            Thread.Sleep(600000);

            // Queue the next subscription check.  This is used instead of a loop as
            // it frees up a slot in the thread pool in case other actions are waiting.
            ThreadPool.QueueUserWorkItem(delegate { CheckSubscriptions(); });
        }
    }
}
