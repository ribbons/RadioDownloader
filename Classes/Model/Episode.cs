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
    using System.Globalization;
    using System.IO;
    using System.Threading;

    internal class Episode : Database
    {
        internal const string Columns = "episodes.epid, progid, name, description, date, duration, autodownload, image";

        private int? imgid = null;

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

        public DateTime Date { get; set; }

        public int Duration { get; set; }

        public bool AutoDownload { get; set; }

        public CompressedImage Image
        {
            get
            {
                if (this.imgid != null)
                {
                    return RetrieveImage(this.imgid.Value);
                }

                using (var command = new SQLiteCommand(
                    "select image from programmes where progid=@progid and image not null",
                    FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@progid", this.Progid));

                    using (var reader = new SQLiteMonDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            return RetrieveImage(reader.GetInt32(reader.GetOrdinal("image")));
                        }
                    }
                }

                return null;
            }
        }

        public static void SetAutoDownload(int[] epids, bool autoDownload)
        {
            ThreadPool.QueueUserWorkItem(delegate { SetAutoDownloadAsync(epids, autoDownload); });
        }

        public static int? FetchInfo(int progid, string episodeExtId)
        {
            int? epid = null;

            using (SQLiteCommand command = new SQLiteCommand("select epid from episodes where progid=@progid and extid=@extid", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));
                command.Parameters.Add(new SQLiteParameter("@extid", episodeExtId));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read())
                    {
                        epid = reader.GetInt32(reader.GetOrdinal("epid"));
                    }
                }
            }

            if (epid == null)
            {
                // Fetch episode information, as it currently doesn't have an ID
                epid = UpdateInfo(progid, episodeExtId);
            }

            return epid;
        }

        public static void UpdateInfoIfRequired(int epid)
        {
            int progid = 0;
            string updateExtid = null;

            // Test to see if the episode is marked as unavailable, and then free up the database
            using (SQLiteCommand command = new SQLiteCommand("select progid, extid from episodes where epid=@epid and available=0", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@epid", epid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read())
                    {
                        progid = reader.GetInt32(reader.GetOrdinal("progid"));
                        updateExtid = reader.GetString(reader.GetOrdinal("extid"));
                    }
                }
            }

            // Perform an update if the episode was marked as unavailable
            if (updateExtid != null)
            {
                if (Download.IsDownload(epid))
                {
                    // The episode is in the downloads list, so just mark as available
                    using (SQLiteCommand command = new SQLiteCommand("update episodes set available=1 where epid=@epid", FetchDbConn()))
                    {
                        command.Parameters.Add(new SQLiteParameter("@epid", epid));
                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    UpdateInfo(progid, updateExtid);
                }
            }
        }

        public override string ToString()
        {
            string infoString = this.Name +
                "\r\nDate: " + this.Date.ToString("yyyy-MM-dd hh:mm", CultureInfo.InvariantCulture);

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
            lock (DbUpdateLock)
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

                foreach (int epid in epids)
                {
                    Updated?.Invoke(epid);
                }
        }

        protected void FetchData(SQLiteMonDataReader reader)
        {
            int descriptionOrdinal = reader.GetOrdinal("description");
            int durationOrdinal = reader.GetOrdinal("duration");
            int imageOrdinal = reader.GetOrdinal("image");

            this.Epid = reader.GetInt32(reader.GetOrdinal("epid"));
            this.Progid = reader.GetInt32(reader.GetOrdinal("progid"));
            this.Date = reader.GetDateTime(reader.GetOrdinal("date"));
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

            if (!reader.IsDBNull(imageOrdinal))
            {
                this.imgid = reader.GetInt32(imageOrdinal);
            }
        }

        private static int? UpdateInfo(int progid, string episodeExtId)
        {
            Guid pluginId;
            string progExtId;
            Provider.ProgrammeInfo progInfo;

            using (SQLiteCommand command = new SQLiteCommand("select pluginid, extid, name, description, singleepisode from programmes where progid=@progid", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (!reader.Read())
                    {
                        throw new DataNotFoundException(progid, "Programme does not exist");
                    }

                    pluginId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));
                    progExtId = reader.GetString(reader.GetOrdinal("extid"));

                    progInfo = new Provider.ProgrammeInfo();
                    progInfo.Name = reader.GetString(reader.GetOrdinal("name"));
                    int descriptionOrdinal = reader.GetOrdinal("description");

                    if (!reader.IsDBNull(descriptionOrdinal))
                    {
                        progInfo.Description = reader.GetString(descriptionOrdinal);
                    }

                    progInfo.SingleEpisode = reader.GetBoolean(reader.GetOrdinal("singleepisode"));
                }
            }

            Provider.RadioProvider providerInst = Provider.Handler.GetFromId(pluginId).CreateInstance();
            Provider.EpisodeInfo episodeInfo;

            try
            {
                episodeInfo = providerInst.GetEpisodeInfo(progExtId, progInfo, episodeExtId);

                if (episodeInfo == null)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(episodeInfo.Name))
                {
                    throw new InvalidDataException("Episode name cannot be null or an empty string");
                }
            }
            catch (Exception provExp)
            {
                provExp.Data.Add("Programme", progInfo.ToString() + "\r\nExtID: " + progExtId);
                provExp.Data.Add("Episode ExtID", episodeExtId);
                throw new ProviderException("Call to GetEpisodeInfo failed", provExp, pluginId);
            }

            if (episodeInfo.Date == null)
            {
                // The date of the episode isn't known, so use the current date
                episodeInfo.Date = DateTime.Now;
            }

            lock (DbUpdateLock)
            {
                using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(FetchDbConn().BeginTransaction()))
                {
                    int? epid = null;

                    using (SQLiteCommand command = new SQLiteCommand("select epid from episodes where progid=@progid and extid=@extid", FetchDbConn(), transMon.Trans))
                    {
                        command.Parameters.Add(new SQLiteParameter("@progid", progid));
                        command.Parameters.Add(new SQLiteParameter("@extid", episodeExtId));

                        using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                        {
                            if (reader.Read())
                            {
                                epid = reader.GetInt32(reader.GetOrdinal("epid"));
                            }
                        }
                    }

                    if (epid == null)
                    {
                        using (SQLiteCommand command = new SQLiteCommand("insert into episodes (progid, extid, name, date) values (@progid, @extid, @name, @date)", FetchDbConn(), transMon.Trans))
                        {
                            command.Parameters.Add(new SQLiteParameter("@progid", progid));
                            command.Parameters.Add(new SQLiteParameter("@extid", episodeExtId));
                            command.Parameters.Add(new SQLiteParameter("@name", episodeInfo.Name));
                            command.Parameters.Add(new SQLiteParameter("@date", episodeInfo.Date));
                            command.ExecuteNonQuery();
                        }

                        using (SQLiteCommand command = new SQLiteCommand("select last_insert_rowid()", FetchDbConn(), transMon.Trans))
                        {
                            epid = (int)(long)command.ExecuteScalar();
                        }
                    }

                    using (SQLiteCommand command = new SQLiteCommand("update episodes set name=@name, description=@description, duration=@duration, date=@date, image=@image, available=1 where epid=@epid", FetchDbConn(), transMon.Trans))
                    {
                        command.Parameters.Add(new SQLiteParameter("@name", episodeInfo.Name));
                        command.Parameters.Add(new SQLiteParameter("@description", episodeInfo.Description));
                        command.Parameters.Add(new SQLiteParameter("@duration", episodeInfo.Duration));
                        command.Parameters.Add(new SQLiteParameter("@date", episodeInfo.Date));
                        command.Parameters.Add(new SQLiteParameter("@image", StoreImage(episodeInfo.Image)));
                        command.Parameters.Add(new SQLiteParameter("@epid", epid));
                        command.ExecuteNonQuery();
                    }

                    using (SQLiteCommand command = new SQLiteCommand("insert or replace into episodeext (epid, name, value) values (@epid, @name, @value)", FetchDbConn(), transMon.Trans))
                    {
                        foreach (KeyValuePair<string, string> extItem in episodeInfo.ExtInfo)
                        {
                            command.Parameters.Add(new SQLiteParameter("@epid", epid));
                            command.Parameters.Add(new SQLiteParameter("@name", extItem.Key));
                            command.Parameters.Add(new SQLiteParameter("@value", extItem.Value));
                            command.ExecuteNonQuery();
                        }
                    }

                    transMon.Trans.Commit();
                    return epid;
                }
            }
        }
    }
}
