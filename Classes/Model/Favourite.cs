/*
 * Copyright © 2010-2020 Matt Robinson
 * Copyright © 2014 Isabelle Riverain
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld.Model
{
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Threading;

    internal class Favourite : Programme
    {
        private static Dictionary<int, int> sortCache;
        private static object sortCacheLock = new object();

        private static FavouriteCols sortBy = FavouriteCols.ProgrammeName;
        private static bool sortAsc;

        static Favourite()
        {
            Programme.Updated += Programme_Updated;
        }

        public Favourite(SQLiteMonDataReader reader)
            : base(reader)
        {
        }

        public Favourite(int progid)
            : base(progid)
        {
        }

        public static event ProgrammeEventHandler Added;

        public static new event ProgrammeEventHandler Updated;

        public static event ProgrammeEventHandler Removed;

        internal enum FavouriteCols
        {
            ProgrammeName = 0,
            Provider = 1,
        }

        public static FavouriteCols SortByColumn
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

        public static List<Favourite> FetchAll()
        {
            List<Favourite> items = new List<Favourite>();

            using (SQLiteCommand command = new SQLiteCommand("select " + Columns + " from favourites, programmes where favourites.progid=programmes.progid", FetchDbConn()))
            using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
            {
                while (reader.Read())
                {
                    items.Add(new Favourite(reader));
                }
            }

            return items;
        }

        public static bool IsFavourite(int progid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select count(*) from favourites where progid=@progid", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));
                return (long)command.ExecuteScalar() != 0;
            }
        }

        public static void Add(int progid)
        {
            ThreadPool.QueueUserWorkItem(delegate { AddAsync(progid); });
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
                        case FavouriteCols.ProgrammeName:
                            orderBy = "programmes.name" + (sortAsc ? string.Empty : " desc");
                            break;
                        case FavouriteCols.Provider:
                            orderBy = "programmes.pluginid" + (sortAsc ? " desc" : string.Empty);
                            break;
                        default:
                            throw new InvalidDataException("Invalid column: " + sortBy.ToString());
                    }

                    using (SQLiteCommand command = new SQLiteCommand("select favourites.progid from favourites, programmes where programmes.progid=favourites.progid order by " + orderBy, FetchDbConn()))
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

                return sortCache[progid1] - sortCache[progid2];
            }
        }

        private static void Programme_Updated(int progid)
        {
            if (IsFavourite(progid))
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
                using (SQLiteCommand command = new SQLiteCommand("insert into favourites (progid) values (@progid)", FetchDbConn()))
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
                using (SQLiteCommand command = new SQLiteCommand("delete from favourites where progid=@progid", FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@progid", progid));

                    if (command.ExecuteNonQuery() == 0)
                    {
                        // Favourite has already been removed
                        return;
                    }
                }
            }

            RaiseUpdated(progid);
            Removed?.Invoke(progid);
        }
    }
}
