/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2018 by the authors - see the AUTHORS file for details.
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
    using System.Threading;

    internal class Programme : Database
    {
        internal const string Columns = "programmes.progid, programmes.extid, programmes.name, programmes.description, singleepisode, pluginid, latestdownload";

        private string extId = null;
        private Provider.ShowMoreProgInfoEventHandler moreInfoHandler = null;

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

        public bool HasMoreInfo
        {
            get
            {
                return this.moreInfoHandler != null;
            }
        }

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
            if (!Provider.Handler.Exists(pluginId))
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
                        return RetrieveImage(reader.GetInt32(reader.GetOrdinal("image")));
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
                                    return RetrieveImage(latestRdr.GetInt32(latestRdr.GetOrdinal("image")));
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static List<string> GetAvailableEpisodes(int progid, bool fetchAll)
        {
            Guid providerId;
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

                    providerId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));
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

            if (!Provider.Handler.Exists(providerId))
            {
                return null;
            }

            // Fetch a list of previously available episodes for the programme
            List<string> previousAvailable = new List<string>();

            using (SQLiteCommand command = new SQLiteCommand("select extid from episodes where progid=@progid and available=1 order by date desc", FetchDbConn()))
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

            List<string> allEpExtIds = new List<string>();
            int page = 1;

            Provider.RadioProvider providerInst = Provider.Handler.GetFromId(providerId).CreateInstance();
            Provider.AvailableEpisodes available;

            do
            {
                try
                {
                    available = providerInst.GetAvailableEpisodes(progExtId, progInfo, page);
                }
                catch (Exception provExp)
                {
                    provExp.Data.Add("Programme ExtID", progExtId);
                    throw new ProviderException("Call to GetAvailableEpisodeIds failed", provExp, providerId);
                }

                if (available.EpisodeIds.Count == 0)
                {
                    break;
                }

                int trackOverlap = -1;

                foreach (string epExtId in available.EpisodeIds)
                {
                    // Append the returned IDs to the list of all episodes (minus duplicates)
                    if (!allEpExtIds.Contains(epExtId))
                    {
                        allEpExtIds.Add(epExtId);
                    }

                    if (previousAvailable.Contains(epExtId))
                    {
                        // Store where the available & previously available ID lists overlap
                        trackOverlap = previousAvailable.IndexOf(epExtId);
                    }
                    else if (trackOverlap >= 0)
                    {
                        // Bump up the overlap index to show there are more after the overlap
                        trackOverlap++;
                    }
                }

                if (available.MoreAvailable && !fetchAll)
                {
                    if (trackOverlap >= 0)
                    {
                        // Remove previously available programmes before this page from the list so that they
                        // are not incorrectly un-flagged as available in the database
                        if (trackOverlap < previousAvailable.Count - 1)
                        {
                            previousAvailable.RemoveRange(trackOverlap + 1, previousAvailable.Count - (trackOverlap + 1));
                        }

                        // Stop fetching available episode pages
                        break;
                    }
                }

                page++;
            }
            while (available.MoreAvailable);

            // Remove the still available episodes from the previously available list
            foreach (string epExtId in allEpExtIds)
            {
                previousAvailable.Remove(epExtId);
            }

            // Unflag any no-longer available episodes in the database
            if (previousAvailable.Count > 0)
            {
                lock (DbUpdateLock)
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

            return allEpExtIds;
        }

        public void ShowMoreInfo()
        {
            this.moreInfoHandler(this.extId);
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
            this.extId = reader.GetString(reader.GetOrdinal("extid"));
            this.Name = reader.GetString(reader.GetOrdinal("name"));

            if (!reader.IsDBNull(descriptionOrdinal))
            {
                this.Description = reader.GetString(descriptionOrdinal);
            }

            this.SingleEpisode = reader.GetBoolean(reader.GetOrdinal("singleepisode"));

            Guid pluginId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));
            Provider.Handler provider = Provider.Handler.GetFromId(pluginId);

            if (provider != null)
            {
                this.ProviderName = provider.Name;
                this.moreInfoHandler = provider.ShowMoreProgInfoHandler;
            }
            else
            {
                this.ProviderName = "<missing>";
            }

            if (!reader.IsDBNull(latestdownloadOrdinal))
            {
                this.LatestDownload = reader.GetDateTime(latestdownloadOrdinal);
            }
        }

        private static int? UpdateInfo(Guid pluginId, string progExtId)
        {
            if (!Provider.Handler.Exists(pluginId))
            {
                return null;
            }

            Provider.RadioProvider pluginInstance = Provider.Handler.GetFromId(pluginId).CreateInstance();
            Provider.ProgrammeInfo progInfo;

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

            lock (DbUpdateLock)
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
                        using (SQLiteCommand command = new SQLiteCommand("insert into programmes (pluginid, extid, name) values (@pluginid, @extid, @name)", FetchDbConn()))
                        {
                            command.Parameters.Add(new SQLiteParameter("@pluginid", pluginId.ToString()));
                            command.Parameters.Add(new SQLiteParameter("@extid", progExtId));
                            command.Parameters.Add(new SQLiteParameter("@name", progExtId));
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

                        if (Provider.Handler.Exists(providerId))
                        {
                            Provider.RadioProvider pluginInstance = Provider.Handler.GetFromId(providerId).CreateInstance();

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
