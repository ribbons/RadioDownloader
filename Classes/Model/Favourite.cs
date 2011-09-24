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

    internal class Favourite : Programme
    {
        public Favourite(SQLiteMonDataReader reader)
            : base(reader)
        {
        }

        public Favourite(int progid)
            : base(progid)
        {
        }

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
    }
}
