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

    internal class Episode
    {
        internal const string Columns = "episodes.epid, progid, name, description, date, duration, autodownload";

        public Episode()
        {
        }

        public Episode(SQLiteMonDataReader reader)
        {
            this.FetchData(reader);
        }

        public Episode(int epid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select " + Columns + " from episodes where epid=@epid", Data.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@epid", epid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (!reader.Read())
                    {
                        throw new DataNotFoundException(epid, "Episode does not exist");
                    }

                    this.FetchData(reader);
                }
            }
        }

        public int Epid { get; private set; }

        public int Progid { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime EpisodeDate { get; set; }

        public int Duration { get; set; }

        public bool AutoDownload { get; set; }

        protected virtual void FetchData(SQLiteMonDataReader reader)
        {
            int descriptionOrdinal = reader.GetOrdinal("description");
            int durationOrdinal = reader.GetOrdinal("duration");

            this.Epid = reader.GetInt32(reader.GetOrdinal("epid"));
            this.Progid = reader.GetInt32(reader.GetOrdinal("progid"));
            this.EpisodeDate = reader.GetDateTime(reader.GetOrdinal("date"));
            this.Name = reader.GetString(reader.GetOrdinal("name"));

            if (!reader.IsDBNull(descriptionOrdinal))
            {
                this.Description = reader.GetString(descriptionOrdinal);
            }

            if (!reader.IsDBNull(durationOrdinal))
            {
                this.Duration = reader.GetInt32(durationOrdinal);
            }

            this.AutoDownload = reader.GetInt32(reader.GetOrdinal("autodownload")) == 1;
        }
    }
}
