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

namespace RadioDld.Model
{
    using System;
    using System.Data.SQLite;
    using System.Globalization;
    using System.Threading;

    internal class Episode : Database
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
            using (SQLiteCommand command = new SQLiteCommand("select " + Columns + " from episodes where epid=@epid", FetchDbConn()))
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

        public delegate void EpisodeEventHandler(int epid);

        public static event EpisodeEventHandler Updated;

        public int Epid { get; private set; }

        public int Progid { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime EpisodeDate { get; set; }

        public int Duration { get; set; }

        public bool AutoDownload { get; set; }

        public static void SetAutoDownload(int[] epids, bool autoDownload)
        {
            ThreadPool.QueueUserWorkItem(delegate { SetAutoDownloadAsync(epids, autoDownload); });
        }

        public static System.Drawing.Bitmap GetImage(int epid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select image, progid from episodes where epid=@epid", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@epid", epid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read())
                    {
                        int imageOrdinal = reader.GetOrdinal("image");

                        if (!reader.IsDBNull(imageOrdinal))
                        {
                            return Database.RetrieveImage(reader.GetInt32(imageOrdinal));
                        }

                        int progidOrdinal = reader.GetOrdinal("progid");

                        if (!reader.IsDBNull(progidOrdinal))
                        {
                            using (SQLiteCommand progCmd = new SQLiteCommand("select image from programmes where progid=@progid and image not null", FetchDbConn()))
                            {
                                progCmd.Parameters.Add(new SQLiteParameter("@progid", reader.GetInt32(progidOrdinal)));

                                using (SQLiteMonDataReader progReader = new SQLiteMonDataReader(progCmd.ExecuteReader()))
                                {
                                    if (progReader.Read())
                                    {
                                        return Database.RetrieveImage(progReader.GetInt32(progReader.GetOrdinal("image")));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public override string ToString()
        {
            string infoString = this.Name +
                "\r\nDate: " + this.EpisodeDate.ToString("yyyy-MM-dd hh:mm", CultureInfo.InvariantCulture);

            if (this.Description != null)
            {
                infoString += "\r\nDescription: " + this.Description;
            }

            if (this.Duration != 0)
            {
                infoString += "\r\nDuration: " + this.Duration.ToString(CultureInfo.InvariantCulture) + "s";
            }

            return infoString;
        }

        protected static void SetAutoDownloadAsync(int[] epids, bool autoDownload)
        {
            lock (Database.DbUpdateLock)
            {
                using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(FetchDbConn().BeginTransaction()))
                {
                    using (SQLiteCommand command = new SQLiteCommand("update episodes set autodownload=@autodownload where epid=@epid", FetchDbConn()))
                    {
                        SQLiteParameter epidParam = new SQLiteParameter("@epid");
                        SQLiteParameter autodownloadParam = new SQLiteParameter("@autodownload");

                        command.Parameters.Add(epidParam);
                        command.Parameters.Add(autodownloadParam);

                        foreach (int epid in epids)
                        {
                            epidParam.Value = epid;
                            autodownloadParam.Value = autoDownload ? 1 : 0;
                            command.ExecuteNonQuery();
                        }
                    }

                    transMon.Trans.Commit();
                }
            }

            if (Updated != null)
            {
                foreach (int epid in epids)
                {
                    Updated(epid);
                }
            }
        }

        protected void FetchData(SQLiteMonDataReader reader)
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
