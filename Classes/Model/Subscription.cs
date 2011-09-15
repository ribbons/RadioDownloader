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
    using System.Data.SQLite;
    using System.Globalization;

    internal class Subscription : Programme
    {
        public Subscription(SQLiteMonDataReader reader)
            : base(reader)
        {
            int latestdownloadOrdinal = reader.GetOrdinal("latestdownload");

            if (!reader.IsDBNull(latestdownloadOrdinal))
            {
                this.LatestDownload = reader.GetDateTime(latestdownloadOrdinal);
            }
        }

        public DateTime? LatestDownload { get; set; }

        public static bool IsSubscribed(int progid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select count(*) from subscriptions where progid=@progid", Data.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
            }
        }
    }
}
