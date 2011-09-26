/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2011 Matt Robinson
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

namespace RadioDld.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Globalization;
    using System.Threading;

    internal class Favourite : Programme
    {
        private static Dictionary<int, int> sortCache;
        private static object sortCacheLock = new object();

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

        public static List<Favourite> FetchAll()
        {
            List<Favourite> items = new List<Favourite>();

            using (SQLiteCommand command = new SQLiteCommand("select " + Columns + " from favourites, programmes where favourites.progid=programmes.progid", Data.FetchDbConn()))
            {
                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    while (reader.Read())
                    {
                        items.Add(new Favourite(reader));
                    }
                }
            }

            return items;
        }

        public static bool IsFavourite(int progid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select count(*) from favourites where progid=@progid", Data.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
            }
        }

        public static bool Add(int progid)
        {
            if (Model.Favourite.IsFavourite(progid))
            {
                return false;
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

                    using (SQLiteCommand command = new SQLiteCommand("select favourites.progid from favourites, programmes where programmes.progid=favourites.progid order by name", Data.FetchDbConn()))
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
            if (IsFavourite(progid))
            {
                lock (sortCacheLock)
                {
                    sortCache = null;
                }

                if (Updated != null)
                {
                    Updated(progid);
                }
            }
        }

        private static void AddAsync(int progid)
        {
            lock (Data.DbUpdateLock)
            {
                // Check again that the favourite doesn't exist, as it may have been
                // added while this call was waiting in the thread pool
                if (Model.Favourite.IsFavourite(progid))
                {
                    return;
                }

                using (SQLiteCommand command = new SQLiteCommand("insert into favourites (progid) values (@progid)", Data.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@progid", progid));
                    command.ExecuteNonQuery();
                }
            }

            if (Added != null)
            {
                Added(progid);
            }

            Programme.RaiseUpdated(progid);
        }

        private static void RemoveAsync(int progid)
        {
            lock (Data.DbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("delete from favourites where progid=@progid", Data.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@progid", progid));
                    command.ExecuteNonQuery();
                }
            }

            Programme.RaiseUpdated(progid);

            if (Removed != null)
            {
                Removed(progid);
            }
        }
    }
}
