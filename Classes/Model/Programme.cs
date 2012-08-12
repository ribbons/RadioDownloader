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
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Globalization;
    using System.Threading;

    internal class Programme : Database
    {
        internal const string Columns = "programmes.progid, programmes.name, programmes.description, singleepisode, pluginid, latestdownload";

        public Programme()
        {
        }

        public Programme(SQLiteMonDataReader reader)
        {
            this.FetchData(reader);
        }

        public Programme(int progid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select " + Columns + " from programmes where progid=@progid", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (!reader.Read())
                    {
                        throw new DataNotFoundException(progid, "Programme does not exist");
                    }

                    this.FetchData(reader);
                }
            }
        }

        public delegate void ProgrammeEventHandler(int progid);

        public static event ProgrammeEventHandler Updated;

        public int Progid { get; private set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool SingleEpisode { get; set; }

        public string ProviderName { get; set; }

        public DateTime? LatestDownload { get; set; }

        public static List<Programme> FetchAllWithDownloads()
        {
            List<Programme> items = new List<Programme>();

            using (SQLiteCommand command = new SQLiteCommand("select distinct " + Columns + " from programmes, episodes, downloads where downloads.epid=episodes.epid and episodes.progid=programmes.progid", FetchDbConn()))
            {
                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    while (reader.Read())
                    {
                        items.Add(new Programme(reader));
                    }
                }
            }

            return items;
        }

        public static int? FetchInfo(Guid pluginId, string progExtId)
        {
            if (!Plugins.PluginExists(pluginId))
            {
                return null;
            }

            int? progid = null;

            using (SQLiteCommand command = new SQLiteCommand("select progid from programmes where pluginid=@pluginid and extid=@extid", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@pluginid", pluginId.ToString()));
                command.Parameters.Add(new SQLiteParameter("@extid", progExtId));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read())
                    {
                        progid = reader.GetInt32(reader.GetOrdinal("progid"));
                    }
                }
            }

            if (progid == null)
            {
                // Need to fetch the data synchronously as the progid is currently unknown
                progid = UpdateInfo(pluginId, progExtId);
            }
            else
            {
                // Kick off an update in the background if required
                UpdateInfoIfRequired(progid.Value);
            }

            return progid;
        }

        public static void UpdateInfoIfRequired(int progid)
        {
            ThreadPool.QueueUserWorkItem(delegate { UpdateInfoIfRequiredAsync(progid); });
        }

        public static void SetLatestDownload(int progid, DateTime downloadDate)
        {
            using (SQLiteCommand command = new SQLiteCommand("update programmes set latestdownload=@latestdownload where progid=@progid", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@latestdownload", downloadDate));
                command.Parameters.Add(new SQLiteParameter("@progid", progid));
                command.ExecuteNonQuery();
            }

            if (Updated != null)
            {
                Updated(progid);
            }
        }

        public static System.Drawing.Bitmap GetImage(int progid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select image from programmes where progid=@progid and image not null", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read())
                    {
                        return Database.RetrieveImage(reader.GetInt32(reader.GetOrdinal("image")));
                    }
                    else
                    {
                        // Find the id of the latest episode's image
                        using (SQLiteCommand latestCmd = new SQLiteCommand("select image from episodes where progid=@progid and image not null order by date desc limit 1", FetchDbConn()))
                        {
                            latestCmd.Parameters.Add(new SQLiteParameter("@progid", progid));

                            using (SQLiteMonDataReader latestRdr = new SQLiteMonDataReader(latestCmd.ExecuteReader()))
                            {
                                if (latestRdr.Read())
                                {
                                    return Database.RetrieveImage(latestRdr.GetInt32(latestRdr.GetOrdinal("image")));
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static List<string> GetAvailableEpisodes(int progid)
        {
            Guid providerId;
            string progExtId;

            using (SQLiteCommand command = new SQLiteCommand("select pluginid, extid from programmes where progid=@progid", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (!reader.Read())
                    {
                        throw new DataNotFoundException(progid, "Programme does not exist");
                    }

                    providerId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));
                    progExtId = reader.GetString(reader.GetOrdinal("extid"));
                }
            }

            if (!Plugins.PluginExists(providerId))
            {
                return null;
            }

            IRadioProvider providerInst = Plugins.GetPluginInstance(providerId);
            string[] epExtIds;

            try
            {
                epExtIds = providerInst.GetAvailableEpisodeIds(progExtId);
            }
            catch (Exception provExp)
            {
                provExp.Data.Add("Programme ExtID", progExtId);
                throw new ProviderException("Call to GetAvailableEpisodeIds failed", provExp, providerId);
            }

            if (epExtIds == null)
            {
                return null;
            }

            // Remove any duplicates from the list of episodes
            List<string> epExtIdsUnique = new List<string>();

            foreach (string epExtId in epExtIds)
            {
                if (!epExtIdsUnique.Contains(epExtId))
                {
                    epExtIdsUnique.Add(epExtId);
                }
            }

            // Fetch a list of previously available episodes for the programme
            List<string> previousAvailable = new List<string>();

            using (SQLiteCommand command = new SQLiteCommand("select extid from episodes where progid=@progid and available=1", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    int epidOrdinal = reader.GetOrdinal("extid");

                    while (reader.Read())
                    {
                        previousAvailable.Add(reader.GetString(epidOrdinal));
                    }
                }
            }

            // Remove the still available episodes from the previously available list
            foreach (string epExtId in epExtIdsUnique)
            {
                previousAvailable.Remove(epExtId);
            }

            // Unflag any no-longer available episodes in the database
            if (previousAvailable.Count > 0)
            {
                lock (Database.DbUpdateLock)
                {
                    using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(FetchDbConn().BeginTransaction()))
                    {
                        using (SQLiteCommand command = new SQLiteCommand("update episodes set available=0 where progid=@progid and extid=@extid", FetchDbConn(), transMon.Trans))
                        {
                            SQLiteParameter extidParam = new SQLiteParameter("@extid");
                            command.Parameters.Add(new SQLiteParameter("@progid", progid));
                            command.Parameters.Add(extidParam);

                            foreach (string epExtId in previousAvailable)
                            {
                                extidParam.Value = epExtId;
                                command.ExecuteNonQuery();
                            }
                        }

                        transMon.Trans.Commit();
                    }
                }
            }

            return epExtIdsUnique;
        }

        public override string ToString()
        {
            string infoString = this.Name;

            if (this.Description != null)
            {
                infoString += "\r\nDescription: " + this.Description;
            }

            return infoString;
        }

        protected static void RaiseUpdated(int progid)
        {
            if (Updated != null)
            {
                Updated(progid);
            }
        }

        protected void FetchData(SQLiteMonDataReader reader)
        {
            int descriptionOrdinal = reader.GetOrdinal("description");
            int latestdownloadOrdinal = reader.GetOrdinal("latestdownload");

            this.Progid = reader.GetInt32(reader.GetOrdinal("progid"));
            this.Name = reader.GetString(reader.GetOrdinal("name"));

            if (!reader.IsDBNull(descriptionOrdinal))
            {
                this.Description = reader.GetString(descriptionOrdinal);
            }

            this.SingleEpisode = reader.GetBoolean(reader.GetOrdinal("singleepisode"));

            Guid pluginId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));
            IRadioProvider providerInst = Plugins.GetPluginInstance(pluginId);
            this.ProviderName = providerInst.ProviderName;

            if (!reader.IsDBNull(latestdownloadOrdinal))
            {
                this.LatestDownload = reader.GetDateTime(latestdownloadOrdinal);
            }
        }

        private static int? UpdateInfo(Guid pluginId, string progExtId)
        {
            if (!Plugins.PluginExists(pluginId))
            {
                return null;
            }

            IRadioProvider pluginInstance = Plugins.GetPluginInstance(pluginId);
            ProgrammeInfo progInfo;

            try
            {
                progInfo = pluginInstance.GetProgrammeInfo(progExtId);
            }
            catch (Exception provExp)
            {
                provExp.Data.Add("Programme ExtID", progExtId);
                throw new ProviderException("Call to GetProgrammeInfo failed", provExp, pluginId);
            }

            if (progInfo == null)
            {
                return null;
            }

            int? progid = null;

            lock (Database.DbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("select progid from programmes where pluginid=@pluginid and extid=@extid", FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@pluginid", pluginId.ToString()));
                    command.Parameters.Add(new SQLiteParameter("@extid", progExtId));

                    using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            progid = reader.GetInt32(reader.GetOrdinal("progid"));
                        }
                    }
                }

                using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(FetchDbConn().BeginTransaction()))
                {
                    if (progid == null)
                    {
                        using (SQLiteCommand command = new SQLiteCommand("insert into programmes (pluginid, extid) values (@pluginid, @extid)", FetchDbConn()))
                        {
                            command.Parameters.Add(new SQLiteParameter("@pluginid", pluginId.ToString()));
                            command.Parameters.Add(new SQLiteParameter("@extid", progExtId));
                            command.ExecuteNonQuery();
                        }

                        using (SQLiteCommand command = new SQLiteCommand("select last_insert_rowid()", FetchDbConn()))
                        {
                            progid = (int)(long)command.ExecuteScalar();
                        }
                    }

                    using (SQLiteCommand command = new SQLiteCommand("update programmes set name=@name, description=@description, image=@image, singleepisode=@singleepisode, lastupdate=@lastupdate where progid=@progid", FetchDbConn()))
                    {
                        command.Parameters.Add(new SQLiteParameter("@name", progInfo.Name));
                        command.Parameters.Add(new SQLiteParameter("@description", progInfo.Description));
                        command.Parameters.Add(new SQLiteParameter("@image", StoreImage(progInfo.Image)));
                        command.Parameters.Add(new SQLiteParameter("@singleepisode", progInfo.SingleEpisode));
                        command.Parameters.Add(new SQLiteParameter("@lastupdate", DateTime.Now));
                        command.Parameters.Add(new SQLiteParameter("@progid", progid));
                        command.ExecuteNonQuery();
                    }

                    transMon.Trans.Commit();
                }
            }

            if (Updated != null)
            {
                Updated(progid.Value);
            }

            return progid;
        }

        private static void UpdateInfoIfRequiredAsync(int progid)
        {
            Guid providerId = Guid.Empty;
            string updateExtid = null;

            // Test to see if an update is required, and then free up the database
            using (SQLiteCommand command = new SQLiteCommand("select pluginid, extid, lastupdate from programmes where progid=@progid", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read())
                    {
                        providerId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));

                        if (Plugins.PluginExists(providerId))
                        {
                            IRadioProvider pluginInstance = Plugins.GetPluginInstance(providerId);

                            if (reader.GetDateTime(reader.GetOrdinal("lastupdate")).AddDays(pluginInstance.ProgInfoUpdateFreqDays) < DateTime.Now)
                            {
                                updateExtid = reader.GetString(reader.GetOrdinal("extid"));
                            }
                        }
                    }
                }
            }

            // Now perform the update if required
            if (updateExtid != null)
            {
                try
                {
                    UpdateInfo(providerId, updateExtid);
                }
                catch (ProviderException)
                {
                    // Suppress any provider exceptions, as the user isn't waiting for this action
                }
            }
        }
    }
}