/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2010 Matt Robinson
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

namespace RadioDld
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml.Serialization;
    using Microsoft.VisualBasic;
    using Microsoft.VisualBasic.ApplicationServices;

    internal class Data
    {
        [ThreadStatic()]
        private static SQLiteConnection dbConn;
        private static Data dataInstance;

        private static object dataInstanceLock = new object();

        private object dbUpdateLock = new object();
        private Plugins pluginsInst;

        private DataSearch search;
        private Thread episodeListThread;

        private object episodeListThreadLock = new object();
        private DldProgData curDldProgData;
        private Thread downloadThread;

        private IRadioProvider downloadPluginInst;
        private IRadioProvider findNewPluginInst;

        private Dictionary<int, int> favouriteSortCache;
        private object favouriteSortCacheLock = new object();

        private Dictionary<int, int> subscriptionSortCache;
        private object subscriptionSortCacheLock = new object();

        private DownloadCols downloadSortBy = DownloadCols.EpisodeDate;
        private bool downloadSortAsc;

        private Dictionary<int, int> downloadSortCache;
        private object downloadSortCacheLock = new object();

        private object findDownloadLock = new object();
        private int lastProgressVal = -1;

        private Data()
            : base()
        {
            // Vacuum the database every so often.  Works best as the first command, as reduces risk of conflicts.
            this.VacuumDatabase();

            // Setup an instance of the plugins class
            this.pluginsInst = new Plugins();

            // Fetch the version of the database
            int currentVer = 0;

            if (this.GetDBSetting("databaseversion") == null)
            {
                currentVer = 1;
            }
            else
            {
                currentVer = Convert.ToInt32(this.GetDBSetting("databaseversion"), CultureInfo.InvariantCulture);
            }

            // Set the current database version.  This is done before the upgrades are attempted so that
            // if the upgrade throws an exception this can be reported, but the programme will run next time.
            // NB: Remember to change default version in the database if this next line is changed!
            this.SetDBSetting("databaseversion", 3);

            switch (currentVer)
            {
                case 2:
                    this.UpgradeDBv2to3();
                    break;
                case 3:
                    // Nothing to do, this is the current version.
                    break;
            }

            this.search = DataSearch.GetInstance(this);

            // Start regularly checking for new subscriptions in the background
            ThreadPool.QueueUserWorkItem(delegate { this.CheckSubscriptions(); });
        }

        public delegate void ProviderAddedEventHandler(Guid providerId);

        public delegate void FindNewViewChangeEventHandler(object viewData);

        public delegate void FoundNewEventHandler(int progid);

        public delegate void ProgrammeUpdatedEventHandler(int progid);

        public delegate void EpisodeAddedEventHandler(int epid);

        public delegate void FavouriteAddedEventHandler(int progid);

        public delegate void FavouriteUpdatedEventHandler(int progid);

        public delegate void FavouriteRemovedEventHandler(int progid);

        public delegate void SubscriptionAddedEventHandler(int progid);

        public delegate void SubscriptionUpdatedEventHandler(int progid);

        public delegate void SubscriptionRemovedEventHandler(int progid);

        public delegate void DownloadAddedEventHandler(int epid);

        public delegate void DownloadUpdatedEventHandler(int epid);

        public delegate void DownloadProgressEventHandler(int epid, int percent, string statusText, ProgressIcon icon);

        public delegate void DownloadRemovedEventHandler(int epid);

        public delegate void DownloadProgressTotalEventHandler(bool downloading, int percent);

        public event ProviderAddedEventHandler ProviderAdded;

        public event FindNewViewChangeEventHandler FindNewViewChange;

        public event FoundNewEventHandler FoundNew;

        public event ProgrammeUpdatedEventHandler ProgrammeUpdated;

        public event EpisodeAddedEventHandler EpisodeAdded;

        public event FavouriteAddedEventHandler FavouriteAdded;

        public event FavouriteUpdatedEventHandler FavouriteUpdated;

        public event FavouriteRemovedEventHandler FavouriteRemoved;

        public event SubscriptionAddedEventHandler SubscriptionAdded;

        public event SubscriptionUpdatedEventHandler SubscriptionUpdated;

        public event SubscriptionRemovedEventHandler SubscriptionRemoved;

        public event DownloadAddedEventHandler DownloadAdded;

        public event DownloadUpdatedEventHandler DownloadUpdated;

        public event DownloadProgressEventHandler DownloadProgress;

        public event DownloadRemovedEventHandler DownloadRemoved;

        public event DownloadProgressTotalEventHandler DownloadProgressTotal;

        public enum DownloadStatus
        {
            Waiting = 0,
            Downloaded = 1,
            Errored = 2
        }

        public enum DownloadCols
        {
            EpisodeName = 0,
            EpisodeDate = 1,
            Status = 2,
            Progress = 3,
            Duration = 4
        }

        public DownloadCols DownloadSortByCol
        {
            get
            {
                return this.downloadSortBy;
            }

            set
            {
                lock (this.downloadSortCacheLock)
                {
                    if (value != this.downloadSortBy)
                    {
                        this.downloadSortCache = null;
                    }

                    this.downloadSortBy = value;
                }
            }
        }

        public bool DownloadSortAscending
        {
            get
            {
                return this.downloadSortAsc;
            }

            set
            {
                lock (this.downloadSortCacheLock)
                {
                    if (value != this.downloadSortAsc)
                    {
                        this.downloadSortCache = null;
                    }

                    this.downloadSortAsc = value;
                }
            }
        }

        public string DownloadQuery
        {
            get { return this.search.DownloadQuery; }
            set { this.search.DownloadQuery = value; }
        }

        public static Data GetInstance()
        {
            // Need to use a lock instead of declaring the instance variable as New, as otherwise
            // on first run the constructor gets called before the template database is in place
            lock (dataInstanceLock)
            {
                if (dataInstance == null)
                {
                    dataInstance = new Data();
                }

                return dataInstance;
            }
        }

        public void StartDownload()
        {
            ThreadPool.QueueUserWorkItem(delegate { this.StartDownloadAsync(); });
        }

        public void UpdateProgInfoIfRequired(int progid)
        {
            ThreadPool.QueueUserWorkItem(delegate { this.UpdateProgInfoIfRequiredAsync(progid); });
        }

        public Bitmap FetchProgrammeImage(int progid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select image from programmes where progid=@progid and image not null", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read())
                    {
                        return this.RetrieveImage(reader.GetInt32(reader.GetOrdinal("image")));
                    }
                    else
                    {
                        // Find the id of the latest episode's image
                        using (SQLiteCommand latestCmd = new SQLiteCommand("select image from episodes where progid=@progid and image not null order by date desc limit 1", this.FetchDbConn()))
                        {
                            latestCmd.Parameters.Add(new SQLiteParameter("@progid", progid));

                            using (SQLiteMonDataReader latestRdr = new SQLiteMonDataReader(latestCmd.ExecuteReader()))
                            {
                                if (latestRdr.Read())
                                {
                                    return this.RetrieveImage(latestRdr.GetInt32(latestRdr.GetOrdinal("image")));
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public Bitmap FetchEpisodeImage(int epid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select image, progid from episodes where epid=@epid", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@epid", epid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read())
                    {
                        int imageOrdinal = reader.GetOrdinal("image");

                        if (!reader.IsDBNull(imageOrdinal))
                        {
                            return this.RetrieveImage(reader.GetInt32(imageOrdinal));
                        }

                        int progidOrdinal = reader.GetOrdinal("progid");

                        if (!reader.IsDBNull(progidOrdinal))
                        {
                            using (SQLiteCommand progCmd = new SQLiteCommand("select image from programmes where progid=@progid and image not null", this.FetchDbConn()))
                            {
                                progCmd.Parameters.Add(new SQLiteParameter("@epid", reader.GetInt32(progidOrdinal)));

                                using (SQLiteMonDataReader progReader = new SQLiteMonDataReader(progCmd.ExecuteReader()))
                                {
                                    if (progReader.Read())
                                    {
                                        return this.RetrieveImage(progReader.GetInt32(progReader.GetOrdinal("image")));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public void EpisodeSetAutoDownload(int epid, bool autoDownload)
        {
            ThreadPool.QueueUserWorkItem(delegate { this.EpisodeSetAutoDownloadAsync(epid, autoDownload); });
        }

        public int CountDownloadsNew()
        {
            using (SQLiteCommand command = new SQLiteCommand("select count(epid) from downloads where playcount=0 and status=@status", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Downloaded));
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        public int CountDownloadsErrored()
        {
            using (SQLiteCommand command = new SQLiteCommand("select count(epid) from downloads where status=@status", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Errored));
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        public bool AddDownload(int epid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select epid from downloads where epid=@epid", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@epid", epid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read())
                    {
                        return false;
                    }
                }
            }

            ThreadPool.QueueUserWorkItem(delegate { this.AddDownloadAsync(epid); });

            return true;
        }

        public bool AddFavourite(int progid)
        {
            if (this.IsFavourite(progid))
            {
                return false;
            }

            ThreadPool.QueueUserWorkItem(delegate { this.AddFavouriteAsync(progid); });

            return true;
        }

        public void RemoveFavourite(int progid)
        {
            ThreadPool.QueueUserWorkItem(delegate { this.RemoveFavouriteAsync(progid); });
        }

        public bool AddSubscription(int progid)
        {
            if (this.IsSubscribed(progid))
            {
                return false;
            }

            ThreadPool.QueueUserWorkItem(delegate { this.AddSubscriptionAsync(progid); });

            return true;
        }

        public void RemoveSubscription(int progid)
        {
            ThreadPool.QueueUserWorkItem(delegate { this.RemoveSubscriptionAsync(progid); });
        }

        public void ResetDownload(int epid)
        {
            ThreadPool.QueueUserWorkItem(delegate { this.ResetDownloadAsync(epid, false); });
        }

        public void DownloadRemove(int epid)
        {
            ThreadPool.QueueUserWorkItem(delegate { this.DownloadRemoveAsync(epid, false); });
        }

        public void DownloadBumpPlayCount(int epid)
        {
            ThreadPool.QueueUserWorkItem(delegate { this.DownloadBumpPlayCountAsync(epid); });
        }

        public void DownloadReportError(int epid)
        {
            ErrorType errorType = default(ErrorType);
            string errorText = null;
            string extraDetailsString = null;
            Dictionary<string, string> errorExtraDetails = new Dictionary<string, string>();

            XmlSerializer detailsSerializer = new XmlSerializer(typeof(List<DldErrorDataItem>));

            using (SQLiteCommand command = new SQLiteCommand("select errortype, errordetails, ep.name as epname, ep.description as epdesc, date, duration, ep.extid as epextid, pr.name as progname, pr.description as progdesc, pr.extid as progextid, pluginid from downloads as dld, episodes as ep, programmes as pr where dld.epid=@epid and ep.epid=@epid and ep.progid=pr.progid", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@epid", epid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (!reader.Read())
                    {
                        throw new ArgumentException("Episode " + epid.ToString(CultureInfo.InvariantCulture) + " does not exit, or is not in the download list!", "epid");
                    }

                    errorType = (ErrorType)reader.GetInt32(reader.GetOrdinal("errortype"));
                    extraDetailsString = reader.GetString(reader.GetOrdinal("errordetails"));

                    errorExtraDetails.Add("episode:name", reader.GetString(reader.GetOrdinal("epname")));
                    errorExtraDetails.Add("episode:description", reader.GetString(reader.GetOrdinal("epdesc")));
                    errorExtraDetails.Add("episode:date", reader.GetDateTime(reader.GetOrdinal("date")).ToString("yyyy-MM-dd hh:mm", CultureInfo.InvariantCulture));
                    errorExtraDetails.Add("episode:duration", Convert.ToString(reader.GetInt32(reader.GetOrdinal("duration")), CultureInfo.InvariantCulture));
                    errorExtraDetails.Add("episode:extid", reader.GetString(reader.GetOrdinal("epextid")));

                    errorExtraDetails.Add("programme:name", reader.GetString(reader.GetOrdinal("progname")));
                    errorExtraDetails.Add("programme:description", reader.GetString(reader.GetOrdinal("progdesc")));
                    errorExtraDetails.Add("programme:extid", reader.GetString(reader.GetOrdinal("progextid")));

                    Guid pluginId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));
                    IRadioProvider providerInst = this.pluginsInst.GetPluginInstance(pluginId);

                    errorExtraDetails.Add("provider:id", pluginId.ToString());
                    errorExtraDetails.Add("provider:name", providerInst.ProviderName);
                    errorExtraDetails.Add("provider:description", providerInst.ProviderDescription);
                }
            }

            if (extraDetailsString != null)
            {
                try
                {
                    List<DldErrorDataItem> extraDetails = null;
                    extraDetails = (List<DldErrorDataItem>)detailsSerializer.Deserialize(new StringReader(extraDetailsString));

                    foreach (DldErrorDataItem detailItem in extraDetails)
                    {
                        switch (detailItem.Name)
                        {
                            case "error":
                                errorText = detailItem.Data;
                                break;
                            case "details":
                                extraDetailsString = detailItem.Data;
                                break;
                            default:
                                errorExtraDetails.Add(detailItem.Name, detailItem.Data);
                                break;
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    // Do nothing, and fall back to reporting all the details as one string
                }
                catch (InvalidCastException)
                {
                    // Do nothing, and fall back to reporting all the details as one string
                }
            }

            if (string.IsNullOrEmpty(errorText))
            {
                errorText = errorType.ToString();
            }

            ErrorReporting report = new ErrorReporting("Download Error: " + errorText, extraDetailsString, errorExtraDetails);
            report.SendReport(Properties.Settings.Default.ErrorReportURL);
        }

        public void PerformCleanup()
        {
            using (SQLiteCommand command = new SQLiteCommand("select epid, filepath from downloads where status=@status", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Downloaded));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    int epidOrd = reader.GetOrdinal("epid");
                    int filepathOrd = reader.GetOrdinal("filepath");

                    while (reader.Read())
                    {
                        // Remove programmes for which the associated audio file no longer exists
                        if (File.Exists(reader.GetString(filepathOrd)) == false)
                        {
                            // Take the download out of the list and set the auto download flag to false
                            this.DownloadRemoveAsync(reader.GetInt32(epidOrd), false);
                        }
                    }
                }
            }
        }

        public Panel GetFindNewPanel(Guid pluginID, object view)
        {
            if (this.pluginsInst.PluginExists(pluginID))
            {
                this.findNewPluginInst = this.pluginsInst.GetPluginInstance(pluginID);
                this.findNewPluginInst.FindNewException += this.FindNewPluginInst_FindNewException;
                this.findNewPluginInst.FindNewViewChange += this.FindNewPluginInst_FindNewViewChange;
                this.findNewPluginInst.FoundNew += this.FindNewPluginInst_FoundNew;
                return this.findNewPluginInst.GetFindNewPanel(view);
            }
            else
            {
                return new Panel();
            }
        }

        public int CompareDownloads(int epid1, int epid2)
        {
            lock (this.downloadSortCacheLock)
            {
                if (this.downloadSortCache == null || !this.downloadSortCache.ContainsKey(epid1) || !this.downloadSortCache.ContainsKey(epid2))
                {
                    // The sort cache is either empty or missing one of the values that are required, so recreate it
                    this.downloadSortCache = new Dictionary<int, int>();

                    int sort = 0;
                    string orderBy = null;

                    switch (this.downloadSortBy)
                    {
                        case DownloadCols.EpisodeName:
                            orderBy = "name" + (this.downloadSortAsc ? string.Empty : " desc");
                            break;
                        case DownloadCols.EpisodeDate:
                            orderBy = "date" + (this.downloadSortAsc ? string.Empty : " desc");
                            break;
                        case DownloadCols.Status:
                            orderBy = "status = 0" + (this.downloadSortAsc ? " desc" : string.Empty) + ", status" + (this.downloadSortAsc ? " desc" : string.Empty) + ", playcount > 0" + (this.downloadSortAsc ? string.Empty : " desc") + ", date" + (this.downloadSortAsc ? " desc" : string.Empty);
                            break;
                        case DownloadCols.Duration:
                            orderBy = "duration" + (this.downloadSortAsc ? string.Empty : " desc");
                            break;
                        default:
                            throw new InvalidDataException("Invalid column: " + this.downloadSortBy.ToString());
                    }

                    using (SQLiteCommand command = new SQLiteCommand("select downloads.epid from downloads, episodes where downloads.epid=episodes.epid order by " + orderBy, this.FetchDbConn()))
                    {
                        using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                        {
                            int epidOrdinal = reader.GetOrdinal("epid");

                            while (reader.Read())
                            {
                                this.downloadSortCache.Add(reader.GetInt32(epidOrdinal), sort);
                                sort += 1;
                            }
                        }
                    }
                }

                try
                {
                    return this.downloadSortCache[epid1] - this.downloadSortCache[epid2];
                }
                catch (KeyNotFoundException)
                {
                    // One of the entries has been removed from the database, but not yet from the list
                    return 0;
                }
            }
        }

        public int CompareSubscriptions(int progid1, int progid2)
        {
            lock (this.subscriptionSortCacheLock)
            {
                if (this.subscriptionSortCache == null || !this.subscriptionSortCache.ContainsKey(progid1) || !this.subscriptionSortCache.ContainsKey(progid2))
                {
                    // The sort cache is either empty or missing one of the values that are required, so recreate it
                    this.subscriptionSortCache = new Dictionary<int, int>();

                    int sort = 0;

                    using (SQLiteCommand command = new SQLiteCommand("select subscriptions.progid from subscriptions, programmes where programmes.progid=subscriptions.progid order by name", this.FetchDbConn()))
                    {
                        using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                        {
                            int progidOrdinal = reader.GetOrdinal("progid");

                            while (reader.Read())
                            {
                                this.subscriptionSortCache.Add(reader.GetInt32(progidOrdinal), sort);
                                sort += 1;
                            }
                        }
                    }
                }

                return this.subscriptionSortCache[progid1] - this.subscriptionSortCache[progid2];
            }
        }

        public int CompareFavourites(int progid1, int progid2)
        {
            lock (this.favouriteSortCacheLock)
            {
                if (this.favouriteSortCache == null || !this.favouriteSortCache.ContainsKey(progid1) || !this.favouriteSortCache.ContainsKey(progid2))
                {
                    // The sort cache is either empty or missing one of the values that are required, so recreate it
                    this.favouriteSortCache = new Dictionary<int, int>();

                    int sort = 0;

                    using (SQLiteCommand command = new SQLiteCommand("select favourites.progid from favourites, programmes where programmes.progid=favourites.progid order by name", this.FetchDbConn()))
                    {
                        using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                        {
                            int progidOrdinal = reader.GetOrdinal("progid");

                            while (reader.Read())
                            {
                                this.favouriteSortCache.Add(reader.GetInt32(progidOrdinal), sort);
                                sort += 1;
                            }
                        }
                    }
                }

                return this.favouriteSortCache[progid1] - this.favouriteSortCache[progid2];
            }
        }

        public void InitProviderList()
        {
            Guid[] pluginIdList = null;
            pluginIdList = this.pluginsInst.GetPluginIdList();

            foreach (Guid pluginId in pluginIdList)
            {
                if (this.ProviderAdded != null)
                {
                    this.ProviderAdded(pluginId);
                }
            }
        }

        public void InitEpisodeList(int progid)
        {
            lock (this.episodeListThreadLock)
            {
                this.episodeListThread = new Thread(() => this.InitEpisodeListThread(progid));
                this.episodeListThread.IsBackground = true;
                this.episodeListThread.Start();
            }
        }

        public void CancelEpisodeListing()
        {
            lock (this.episodeListThreadLock)
            {
                this.episodeListThread = null;
            }
        }

        public void InitSubscriptionList()
        {
            using (SQLiteCommand command = new SQLiteCommand("select subscriptions.progid from subscriptions, programmes where subscriptions.progid = programmes.progid", this.FetchDbConn()))
            {
                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    int progidOrdinal = reader.GetOrdinal("progid");

                    while (reader.Read())
                    {
                        if (this.SubscriptionAdded != null)
                        {
                            this.SubscriptionAdded(reader.GetInt32(progidOrdinal));
                        }
                    }
                }
            }
        }

        public List<DownloadData> FetchDownloadList(bool filtered)
        {
            List<DownloadData> downloadList = new List<DownloadData>();

            using (SQLiteCommand command = new SQLiteCommand("select downloads.epid, name, description, date, duration, status, errortype, errordetails, filepath, playcount from downloads, episodes where downloads.epid=episodes.epid", this.FetchDbConn()))
            {
                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    int epidOrdinal = reader.GetOrdinal("epid");

                    while (reader.Read())
                    {
                        int epid = reader.GetInt32(epidOrdinal);

                        if (!filtered || this.search.DownloadIsVisible(epid))
                        {
                            downloadList.Add(this.ReadDownloadData(epid, reader));
                        }
                    }
                }
            }

            return downloadList;
        }

        public DownloadData FetchDownloadData(int epid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select name, description, date, duration, status, errortype, errordetails, filepath, playcount from downloads, episodes where downloads.epid=@epid and episodes.epid=@epid", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@epid", epid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read() == false)
                    {
                        throw new DataNotFoundException(epid, "Download does not exist");
                    }

                    return this.ReadDownloadData(epid, reader);
                }
            }
        }

        public List<FavouriteData> FetchFavouriteList()
        {
            List<FavouriteData> favouriteList = new List<FavouriteData>();

            using (SQLiteCommand command = new SQLiteCommand("select favourites.progid, name, description, singleepisode, pluginid from favourites, programmes where favourites.progid = programmes.progid", this.FetchDbConn()))
            {
                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    int progidOrdinal = reader.GetOrdinal("progid");

                    while (reader.Read())
                    {
                        favouriteList.Add(this.ReadFavouriteData(reader.GetInt32(progidOrdinal), reader));
                    }
                }
            }

            return favouriteList;
        }

        public FavouriteData FetchFavouriteData(int progid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select name, description, singleepisode, pluginid from programmes where progid=@progid", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read() == false)
                    {
                        throw new DataNotFoundException(progid, "Programme does not exist");
                    }

                    return this.ReadFavouriteData(progid, reader);
                }
            }
        }

        public SubscriptionData FetchSubscriptionData(int progid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select name, description, pluginid from programmes where progid=@progid", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read() == false)
                    {
                        throw new DataNotFoundException(progid, "Programme does not exist");
                    }

                    int descriptionOrdinal = reader.GetOrdinal("description");

                    SubscriptionData info = new SubscriptionData();
                    info.Name = reader.GetString(reader.GetOrdinal("name"));

                    if (!reader.IsDBNull(descriptionOrdinal))
                    {
                        info.Description = reader.GetString(descriptionOrdinal);
                    }

                    info.LatestDownload = this.LatestDownloadDate(progid);

                    Guid pluginId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));
                    IRadioProvider providerInst = this.pluginsInst.GetPluginInstance(pluginId);
                    info.ProviderName = providerInst.ProviderName;

                    info.Favourite = this.IsFavourite(progid);

                    return info;
                }
            }
        }

        public EpisodeData FetchEpisodeData(int epid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select name, description, date, duration, autodownload from episodes where epid=@epid", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@epid", epid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read() == false)
                    {
                        throw new DataNotFoundException(epid, "Episode does not exist");
                    }

                    int descriptionOrdinal = reader.GetOrdinal("description");

                    EpisodeData info = new EpisodeData();
                    info.EpisodeDate = reader.GetDateTime(reader.GetOrdinal("date"));
                    info.Name = TextUtils.StripDateFromName(reader.GetString(reader.GetOrdinal("name")), info.EpisodeDate);

                    if (!reader.IsDBNull(descriptionOrdinal))
                    {
                        info.Description = reader.GetString(descriptionOrdinal);
                    }

                    info.Duration = reader.GetInt32(reader.GetOrdinal("duration"));
                    info.AutoDownload = reader.GetInt32(reader.GetOrdinal("autodownload")) == 1;

                    return info;
                }
            }
        }

        public ProgrammeData FetchProgrammeData(int progid)
        {
            ProgrammeData info = new ProgrammeData();

            using (SQLiteCommand command = new SQLiteCommand("select name, description, singleepisode from programmes where progid=@progid", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read() == false)
                    {
                        throw new DataNotFoundException(progid, "Programme does not exist");
                    }

                    int descriptionOrdinal = reader.GetOrdinal("description");

                    info.Name = reader.GetString(reader.GetOrdinal("name"));

                    if (!reader.IsDBNull(descriptionOrdinal))
                    {
                        info.Description = reader.GetString(descriptionOrdinal);
                    }

                    info.SingleEpisode = reader.GetBoolean(reader.GetOrdinal("singleepisode"));
                }
            }

            info.Favourite = this.IsFavourite(progid);
            info.Subscribed = this.IsSubscribed(progid);

            return info;
        }

        public ProviderData FetchProviderData(Guid providerId)
        {
            IRadioProvider providerInstance = this.pluginsInst.GetPluginInstance(providerId);

            ProviderData info = new ProviderData();
            info.Name = providerInstance.ProviderName;
            info.Description = providerInstance.ProviderDescription;
            info.Icon = providerInstance.ProviderIcon;
            info.ShowOptionsHandler = providerInstance.GetShowOptionsHandler();

            return info;
        }

        private SQLiteConnection FetchDbConn()
        {
            if (dbConn == null)
            {
                dbConn = new SQLiteConnection("Data Source=" + Path.Combine(FileUtils.GetAppDataFolder(), "store.db") + ";Version=3;New=False");
                dbConn.Open();
            }

            return dbConn;
        }

        private void UpgradeDBv2to3()
        {
            int count = 0;
            string[] unusedTables =
            {
                "tblDownloads",
                "tblInfo",
                "tblLastFetch",
                "tblSettings",
                "tblStationVisibility",
                "tblSubscribed"
            };

            using (Status showStatus = new Status())
            {
                showStatus.StatusText = "Removing unused tables...";
                showStatus.ProgressBarMarquee = false;
                showStatus.ProgressBarValue = 0;
                showStatus.ProgressBarMax = unusedTables.GetUpperBound(0) + 1;
                showStatus.Show();
                Application.DoEvents();

                // Delete the unused (v0.4 era) tables if they exist
                foreach (string unusedTable in unusedTables)
                {
                    showStatus.StatusText = "Removing unused table " + Convert.ToString(count, CultureInfo.CurrentCulture) + " of " + Convert.ToString(unusedTables.GetUpperBound(0) + 1, CultureInfo.CurrentCulture) + "...";
                    showStatus.ProgressBarValue = count;
                    Application.DoEvents();

                    lock (this.dbUpdateLock)
                    {
                        using (SQLiteCommand command = new SQLiteCommand("drop table if exists " + unusedTable, this.FetchDbConn()))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    count += 1;
                }

                showStatus.StatusText = "Finished removing unused tables";
                showStatus.ProgressBarValue = count;
                Application.DoEvents();

                // Work through the images and re-save them to ensure they are compressed
                List<int> compressImages = new List<int>();

                using (SQLiteCommand command = new SQLiteCommand("select imgid from images", this.FetchDbConn()))
                {
                    using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            compressImages.Add(reader.GetInt32(reader.GetOrdinal("imgid")));
                        }
                    }
                }

                showStatus.StatusText = "Compressing images...";
                showStatus.ProgressBarValue = 0;
                showStatus.ProgressBarMax = compressImages.Count;
                Application.DoEvents();

                using (SQLiteCommand deleteCmd = new SQLiteCommand("delete from images where imgid=@imgid", this.FetchDbConn()))
                {
                    using (SQLiteCommand updateProgs = new SQLiteCommand("update programmes set image=@newimgid where image=@oldimgid", this.FetchDbConn()))
                    {
                        using (SQLiteCommand updateEps = new SQLiteCommand("update episodes set image=@newimgid where image=@oldimgid", this.FetchDbConn()))
                        {
                            int newImageID = 0;
                            Bitmap image = null;
                            count = 1;

                            lock (this.dbUpdateLock)
                            {
                                foreach (int oldImageID in compressImages)
                                {
                                    showStatus.StatusText = "Compressing image " + Convert.ToString(count, CultureInfo.CurrentCulture) + " of " + Convert.ToString(compressImages.Count, CultureInfo.CurrentCulture) + "...";
                                    showStatus.ProgressBarValue = count - 1;
                                    Application.DoEvents();

                                    image = this.RetrieveImage(oldImageID);

                                    deleteCmd.Parameters.Add(new SQLiteParameter("@imgid", oldImageID));
                                    deleteCmd.ExecuteNonQuery();

                                    newImageID = this.StoreImage(image).Value;
                                    Application.DoEvents();

                                    updateProgs.Parameters.Add(new SQLiteParameter("@oldimgid", oldImageID));
                                    updateProgs.Parameters.Add(new SQLiteParameter("@newimgid", newImageID));

                                    updateProgs.ExecuteNonQuery();

                                    updateEps.Parameters.Add(new SQLiteParameter("@oldimgid", oldImageID));
                                    updateEps.Parameters.Add(new SQLiteParameter("@newimgid", newImageID));

                                    updateEps.ExecuteNonQuery();

                                    count += 1;
                                }
                            }
                        }
                    }
                }

                showStatus.StatusText = "Finished compressing images";
                showStatus.ProgressBarValue = compressImages.Count;
                Application.DoEvents();

                showStatus.Hide();
                Application.DoEvents();
            }
        }

        private void StartDownloadAsync()
        {
            lock (this.findDownloadLock)
            {
                if (this.downloadThread == null)
                {
                    using (SQLiteCommand command = new SQLiteCommand("select pluginid, pr.name as progname, pr.description as progdesc, pr.image as progimg, ep.name as epname, ep.description as epdesc, ep.duration, ep.date, ep.image as epimg, pr.extid as progextid, ep.extid as epextid, dl.status, ep.epid from downloads as dl, episodes as ep, programmes as pr where dl.epid=ep.epid and ep.progid=pr.progid and (dl.status=@statuswait or (dl.status=@statuserr and dl.errortime < datetime('now', '-' || power(2, dl.errorcount) || ' hours'))) order by ep.date", this.FetchDbConn()))
                    {
                        command.Parameters.Add(new SQLiteParameter("@statuswait", DownloadStatus.Waiting));
                        command.Parameters.Add(new SQLiteParameter("@statuserr", DownloadStatus.Errored));

                        using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                        {
                            while (this.downloadThread == null)
                            {
                                if (!reader.Read())
                                {
                                    return;
                                }

                                Guid pluginId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));
                                int epid = reader.GetInt32(reader.GetOrdinal("epid"));

                                if (this.pluginsInst.PluginExists(pluginId))
                                {
                                    this.curDldProgData = new DldProgData();
                                    ProgrammeInfo progInfo = default(ProgrammeInfo);

                                    if (reader.IsDBNull(reader.GetOrdinal("progname")))
                                    {
                                        progInfo.Name = null;
                                    }
                                    else
                                    {
                                        progInfo.Name = reader.GetString(reader.GetOrdinal("progname"));
                                    }

                                    if (reader.IsDBNull(reader.GetOrdinal("progdesc")))
                                    {
                                        progInfo.Description = null;
                                    }
                                    else
                                    {
                                        progInfo.Description = reader.GetString(reader.GetOrdinal("progdesc"));
                                    }

                                    if (reader.IsDBNull(reader.GetOrdinal("progimg")))
                                    {
                                        progInfo.Image = null;
                                    }
                                    else
                                    {
                                        progInfo.Image = this.RetrieveImage(reader.GetInt32(reader.GetOrdinal("progimg")));
                                    }

                                    EpisodeInfo epiEpInfo = default(EpisodeInfo);

                                    if (reader.IsDBNull(reader.GetOrdinal("epname")))
                                    {
                                        epiEpInfo.Name = null;
                                    }
                                    else
                                    {
                                        epiEpInfo.Name = reader.GetString(reader.GetOrdinal("epname"));
                                    }

                                    if (reader.IsDBNull(reader.GetOrdinal("epdesc")))
                                    {
                                        epiEpInfo.Description = null;
                                    }
                                    else
                                    {
                                        epiEpInfo.Description = reader.GetString(reader.GetOrdinal("epdesc"));
                                    }

                                    if (reader.IsDBNull(reader.GetOrdinal("duration")))
                                    {
                                        epiEpInfo.DurationSecs = null;
                                    }
                                    else
                                    {
                                        epiEpInfo.DurationSecs = reader.GetInt32(reader.GetOrdinal("duration"));
                                    }

                                    epiEpInfo.Date = reader.GetDateTime(reader.GetOrdinal("date"));

                                    if (reader.IsDBNull(reader.GetOrdinal("epimg")))
                                    {
                                        epiEpInfo.Image = null;
                                    }
                                    else
                                    {
                                        epiEpInfo.Image = this.RetrieveImage(reader.GetInt32(reader.GetOrdinal("epimg")));
                                    }

                                    epiEpInfo.ExtInfo = new Dictionary<string, string>();

                                    using (SQLiteCommand extCommand = new SQLiteCommand("select name, value from episodeext where epid=@epid", this.FetchDbConn()))
                                    {
                                        extCommand.Parameters.Add(new SQLiteParameter("@epid", reader.GetInt32(reader.GetOrdinal("epid"))));

                                        using (SQLiteMonDataReader extReader = new SQLiteMonDataReader(extCommand.ExecuteReader()))
                                        {
                                            while (extReader.Read())
                                            {
                                                epiEpInfo.ExtInfo.Add(extReader.GetString(extReader.GetOrdinal("name")), extReader.GetString(extReader.GetOrdinal("value")));
                                            }
                                        }
                                    }

                                    this.curDldProgData.PluginId = pluginId;
                                    this.curDldProgData.ProgExtId = reader.GetString(reader.GetOrdinal("progextid"));
                                    this.curDldProgData.EpId = epid;
                                    this.curDldProgData.EpisodeExtId = reader.GetString(reader.GetOrdinal("epextid"));
                                    this.curDldProgData.ProgInfo = progInfo;
                                    this.curDldProgData.EpisodeInfo = epiEpInfo;

                                    if ((DownloadStatus)reader.GetInt32(reader.GetOrdinal("status")) == DownloadStatus.Errored)
                                    {
                                        this.ResetDownloadAsync(epid, true);
                                    }

                                    this.downloadThread = new Thread(this.DownloadProgThread);
                                    this.downloadThread.IsBackground = true;
                                    this.downloadThread.Start();

                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DownloadProgThread()
        {
            this.lastProgressVal = -1;

            this.downloadPluginInst = this.pluginsInst.GetPluginInstance(this.curDldProgData.PluginId);
            this.downloadPluginInst.Finished += this.DownloadPluginInst_Finished;
            this.downloadPluginInst.Progress += this.DownloadPluginInst_Progress;

            try
            {
                // Make sure that the temp folder still exists
                Directory.CreateDirectory(Path.Combine(System.IO.Path.GetTempPath(), "RadioDownloader"));

                try
                {
                    this.curDldProgData.FinalName = FileUtils.FindFreeSaveFileName(Properties.Settings.Default.FileNameFormat, this.curDldProgData.ProgInfo.Name, this.curDldProgData.EpisodeInfo.Name, this.curDldProgData.EpisodeInfo.Date, FileUtils.GetSaveFolder());
                }
                catch (DirectoryNotFoundException)
                {
                    this.DownloadError(ErrorType.LocalProblem, "Your chosen location for saving downloaded programmes no longer exists.  Select a new one under Options -> Main Options.", null);
                    return;
                }
                catch (IOException ioExp)
                {
                    this.DownloadError(ErrorType.LocalProblem, "Encountered an error generating the download file name.  The error message was '" + ioExp.Message + "'.  You may need to select a new location for saving downloaded programmes under Options -> Main Options.", null);
                    return;
                }

                this.downloadPluginInst.DownloadProgramme(this.curDldProgData.ProgExtId, this.curDldProgData.EpisodeExtId, this.curDldProgData.ProgInfo, this.curDldProgData.EpisodeInfo, this.curDldProgData.FinalName);
            }
            catch (ThreadAbortException)
            {
                // The download has been aborted, so ignore the exception
            }
            catch (DownloadException downloadExp)
            {
                this.DownloadError(downloadExp.TypeOfError, downloadExp.Message, downloadExp.ErrorExtraDetails);
            }
            catch (Exception unknownExp)
            {
                List<DldErrorDataItem> extraDetails = new List<DldErrorDataItem>();
                extraDetails.Add(new DldErrorDataItem("error", unknownExp.GetType().ToString() + ": " + unknownExp.Message));
                extraDetails.Add(new DldErrorDataItem("exceptiontostring", unknownExp.ToString()));

                if (unknownExp.Data != null)
                {
                    foreach (DictionaryEntry dataEntry in unknownExp.Data)
                    {
                        if (object.ReferenceEquals(dataEntry.Key.GetType(), typeof(string)) & object.ReferenceEquals(dataEntry.Value.GetType(), typeof(string)))
                        {
                            extraDetails.Add(new DldErrorDataItem("expdata:Data:" + (string)dataEntry.Key, (string)dataEntry.Value));
                        }
                    }
                }

                this.DownloadError(ErrorType.UnknownError, unknownExp.GetType().ToString() + Environment.NewLine + unknownExp.StackTrace, extraDetails);
            }
        }

        private void UpdateProgInfoIfRequiredAsync(int progid)
        {
            Guid providerId = Guid.Empty;
            string updateExtid = null;

            // Test to see if an update is required, and then free up the database
            using (SQLiteCommand command = new SQLiteCommand("select pluginid, extid, lastupdate from programmes where progid=@progid", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read())
                    {
                        providerId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));

                        if (this.pluginsInst.PluginExists(providerId))
                        {
                            IRadioProvider pluginInstance = null;
                            pluginInstance = this.pluginsInst.GetPluginInstance(providerId);

                            if (reader.GetDateTime(reader.GetOrdinal("lastupdate")).AddDays(pluginInstance.ProgInfoUpdateFreqDays) < DateAndTime.Now)
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
                this.StoreProgrammeInfo(providerId, updateExtid);
            }
        }

        private void EpisodeSetAutoDownloadAsync(int epid, bool autoDownload)
        {
            lock (this.dbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("update episodes set autodownload=@autodownload where epid=@epid", this.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@epid", epid));
                    command.Parameters.Add(new SQLiteParameter("@autodownload", autoDownload ? 1 : 0));
                    command.ExecuteNonQuery();
                }
            }
        }

        private void CheckSubscriptions()
        {
            // Wait for 10 minutes to give a pause between each check for new episodes
            Thread.Sleep(600000);

            List<int> progids = new List<int>();

            // Fetch the current subscriptions into a list, so that the reader doesn't remain open while
            // checking all of the subscriptions, as this blocks writes to the database from other threads
            using (SQLiteCommand command = new SQLiteCommand("select progid from subscriptions", this.FetchDbConn()))
            {
                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    int progidOrdinal = reader.GetOrdinal("progid");

                    while (reader.Read())
                    {
                        progids.Add(reader.GetInt32(progidOrdinal));
                    }
                }
            }

            // Work through the list of subscriptions and check for new episodes
            using (SQLiteCommand progInfCmd = new SQLiteCommand("select pluginid, extid from programmes where progid=@progid", this.FetchDbConn()))
            {
                using (SQLiteCommand checkCmd = new SQLiteCommand("select epid from downloads where epid=@epid", this.FetchDbConn()))
                {
                    using (SQLiteCommand findCmd = new SQLiteCommand("select epid, autodownload from episodes where progid=@progid and extid=@extid", this.FetchDbConn()))
                    {
                        SQLiteParameter epidParam = new SQLiteParameter("@epid");
                        SQLiteParameter progidParam = new SQLiteParameter("@progid");
                        SQLiteParameter extidParam = new SQLiteParameter("@extid");

                        progInfCmd.Parameters.Add(progidParam);
                        findCmd.Parameters.Add(progidParam);
                        findCmd.Parameters.Add(extidParam);
                        checkCmd.Parameters.Add(epidParam);

                        foreach (int progid in progids)
                        {
                            Guid providerId = default(Guid);
                            string progExtId = null;

                            progidParam.Value = progid;

                            using (SQLiteMonDataReader progInfReader = new SQLiteMonDataReader(progInfCmd.ExecuteReader()))
                            {
                                if (progInfReader.Read() == false)
                                {
                                    continue;
                                }

                                providerId = new Guid(progInfReader.GetString(progInfReader.GetOrdinal("pluginid")));
                                progExtId = progInfReader.GetString(progInfReader.GetOrdinal("extid"));
                            }

                            List<string> episodeExtIds = null;

                            try
                            {
                                episodeExtIds = this.GetAvailableEpisodes(providerId, progExtId);
                            }
                            catch (Exception)
                            {
                                // Catch any unhandled provider exceptions
                                continue;
                            }

                            if (episodeExtIds != null)
                            {
                                foreach (string episodeExtId in episodeExtIds)
                                {
                                    extidParam.Value = episodeExtId;

                                    bool needEpInfo = true;
                                    int epid = 0;

                                    using (SQLiteMonDataReader findReader = new SQLiteMonDataReader(findCmd.ExecuteReader()))
                                    {
                                        if (findReader.Read())
                                        {
                                            needEpInfo = false;
                                            epid = findReader.GetInt32(findReader.GetOrdinal("epid"));

                                            if (findReader.GetInt32(findReader.GetOrdinal("autodownload")) != 1)
                                            {
                                                // Don't download the episode automatically, skip to the next one
                                                continue;
                                            }
                                        }
                                    }

                                    if (needEpInfo)
                                    {
                                        try
                                        {
                                            epid = this.StoreEpisodeInfo(providerId, progid, progExtId, episodeExtId);
                                        }
                                        catch
                                        {
                                            // Catch any unhandled provider exceptions
                                            continue;
                                        }

                                        if (epid < 0)
                                        {
                                            continue;
                                        }
                                    }

                                    epidParam.Value = epid;

                                    using (SQLiteMonDataReader checkRdr = new SQLiteMonDataReader(checkCmd.ExecuteReader()))
                                    {
                                        if (checkRdr.Read() == false)
                                        {
                                            this.AddDownloadAsync(epid);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Queue the next subscription check.  This is used instead of a loop
            // as it frees up a slot in the thread pool other actions are waiting.
            ThreadPool.QueueUserWorkItem(delegate { this.CheckSubscriptions(); });
        }

        private void AddDownloadAsync(int epid)
        {
            lock (this.dbUpdateLock)
            {
                // Check again that the download doesn't exist, as it may have been
                // added while this call was waiting in the thread pool
                using (SQLiteCommand command = new SQLiteCommand("select epid from downloads where epid=@epid", this.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@epid", epid));

                    using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            return;
                        }
                    }
                }

                using (SQLiteCommand command = new SQLiteCommand("insert into downloads (epid) values (@epid)", this.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@epid", epid));
                    command.ExecuteNonQuery();
                }
            }

            this.search.AddDownload(epid);

            if (this.search.DownloadIsVisible(epid))
            {
                if (this.DownloadAdded != null)
                {
                    this.DownloadAdded(epid);
                }
            }

            this.StartDownload();
        }

        private void AddFavouriteAsync(int progid)
        {
            lock (this.dbUpdateLock)
            {
                // Check again that the favourite doesn't exist, as it may have been
                // added while this call was waiting in the thread pool
                if (this.IsFavourite(progid))
                {
                    return;
                }

                using (SQLiteCommand command = new SQLiteCommand("insert into favourites (progid) values (@progid)", this.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@progid", progid));
                    command.ExecuteNonQuery();
                }
            }

            if (this.ProgrammeUpdated != null)
            {
                this.ProgrammeUpdated(progid);
            }

            if (this.FavouriteAdded != null)
            {
                this.FavouriteAdded(progid);
            }

            if (this.IsSubscribed(progid))
            {
                if (this.SubscriptionUpdated != null)
                {
                    this.SubscriptionUpdated(progid);
                }
            }
        }

        private void RemoveFavouriteAsync(int progid)
        {
            lock (this.dbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("delete from favourites where progid=@progid", this.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@progid", progid));
                    command.ExecuteNonQuery();
                }
            }

            if (this.ProgrammeUpdated != null)
            {
                this.ProgrammeUpdated(progid);
            }

            if (this.FavouriteRemoved != null)
            {
                this.FavouriteRemoved(progid);
            }

            if (this.IsSubscribed(progid))
            {
                if (this.SubscriptionUpdated != null)
                {
                    this.SubscriptionUpdated(progid);
                }
            }
        }

        private void AddSubscriptionAsync(int progid)
        {
            lock (this.dbUpdateLock)
            {
                // Check again that the subscription doesn't exist, as it may have been
                // added while this call was waiting in the thread pool
                if (this.IsSubscribed(progid))
                {
                    return;
                }

                using (SQLiteCommand command = new SQLiteCommand("insert into subscriptions (progid) values (@progid)", this.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@progid", progid));
                    command.ExecuteNonQuery();
                }
            }

            if (this.ProgrammeUpdated != null)
            {
                this.ProgrammeUpdated(progid);
            }

            if (this.SubscriptionAdded != null)
            {
                this.SubscriptionAdded(progid);
            }

            if (this.IsFavourite(progid))
            {
                if (this.FavouriteUpdated != null)
                {
                    this.FavouriteUpdated(progid);
                }
            }
        }

        private void RemoveSubscriptionAsync(int progid)
        {
            lock (this.dbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("delete from subscriptions where progid=@progid", this.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@progid", progid));
                    command.ExecuteNonQuery();
                }
            }

            if (this.ProgrammeUpdated != null)
            {
                this.ProgrammeUpdated(progid);
            }

            if (this.SubscriptionRemoved != null)
            {
                this.SubscriptionRemoved(progid);
            }

            if (this.IsFavourite(progid))
            {
                if (this.FavouriteUpdated != null)
                {
                    this.FavouriteUpdated(progid);
                }
            }
        }

        private DateTime? LatestDownloadDate(int progid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select date from episodes, downloads where episodes.epid=downloads.epid and progid=@progid order by date desc limit 1", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read() == false)
                    {
                        // No downloads of this program
                        return null;
                    }
                    else
                    {
                        return reader.GetDateTime(reader.GetOrdinal("date"));
                    }
                }
            }
        }

        private void ResetDownloadAsync(int epid, bool auto)
        {
            lock (this.dbUpdateLock)
            {
                using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(this.FetchDbConn().BeginTransaction()))
                {
                    using (SQLiteCommand command = new SQLiteCommand("update downloads set status=@status, errortype=null, errortime=null, errordetails=null where epid=@epid", this.FetchDbConn(), transMon.Trans))
                    {
                        command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Waiting));
                        command.Parameters.Add(new SQLiteParameter("@epid", epid));
                        command.ExecuteNonQuery();
                    }

                    if (auto == false)
                    {
                        using (SQLiteCommand command = new SQLiteCommand("update downloads set errorcount=0 where epid=@epid", this.FetchDbConn(), transMon.Trans))
                        {
                            command.Parameters.Add(new SQLiteParameter("@epid", epid));
                            command.ExecuteNonQuery();
                        }
                    }

                    transMon.Trans.Commit();
                }
            }

            lock (this.downloadSortCacheLock)
            {
                this.downloadSortCache = null;
            }

            this.search.UpdateDownload(epid);

            if (this.search.DownloadIsVisible(epid))
            {
                if (this.DownloadUpdated != null)
                {
                    this.DownloadUpdated(epid);
                }
            }

            if (auto == false)
            {
                this.StartDownloadAsync();
            }
        }

        private void DownloadRemoveAsync(int epid, bool auto)
        {
            lock (this.dbUpdateLock)
            {
                using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(this.FetchDbConn().BeginTransaction()))
                {
                    using (SQLiteCommand command = new SQLiteCommand("delete from downloads where epid=@epid", this.FetchDbConn(), transMon.Trans))
                    {
                        command.Parameters.Add(new SQLiteParameter("@epid", epid));
                        command.ExecuteNonQuery();
                    }

                    if (auto == false)
                    {
                        // Unet the auto download flag, so if the user is subscribed it doesn't just download again
                        this.EpisodeSetAutoDownloadAsync(epid, false);
                    }

                    transMon.Trans.Commit();
                }
            }

            lock (this.downloadSortCacheLock)
            {
                // No need to clear the sort cache, just remove this episodes entry
                if (this.downloadSortCache != null)
                {
                    this.downloadSortCache.Remove(epid);
                }
            }

            if (this.search.DownloadIsVisible(epid))
            {
                if (this.DownloadRemoved != null)
                {
                    this.DownloadRemoved(epid);
                }
            }

            if (this.DownloadProgressTotal != null)
            {
                this.DownloadProgressTotal(false, 0);
            }

            this.search.RemoveDownload(epid);

            if (this.curDldProgData != null)
            {
                if (this.curDldProgData.EpId == epid)
                {
                    // This episode is currently being downloaded
                    if (this.downloadThread != null)
                    {
                        if (auto == false)
                        {
                            // This is called by the download thread if it is an automatic removal
                            this.downloadThread.Abort();
                        }

                        this.downloadThread = null;
                    }
                }

                this.StartDownload();
            }
        }

        private void DownloadBumpPlayCountAsync(int epid)
        {
            lock (this.dbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("update downloads set playcount=playcount+1 where epid=@epid", this.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@epid", epid));
                    command.ExecuteNonQuery();
                }
            }

            lock (this.downloadSortCacheLock)
            {
                this.downloadSortCache = null;
            }

            this.search.UpdateDownload(epid);

            if (this.search.DownloadIsVisible(epid))
            {
                if (this.DownloadUpdated != null)
                {
                    this.DownloadUpdated(epid);
                }
            }
        }

        private void DownloadError(ErrorType errorType, string errorDetails, List<DldErrorDataItem> furtherDetails)
        {
            switch (errorType)
            {
                case ErrorType.RemoveFromList:
                    this.DownloadRemoveAsync(this.curDldProgData.EpId, true);
                    return;
                case ErrorType.UnknownError:
                    if (furtherDetails == null)
                    {
                        furtherDetails = new List<DldErrorDataItem>();
                    }

                    if (errorDetails != null)
                    {
                        furtherDetails.Add(new DldErrorDataItem("details", errorDetails));
                    }

                    StringWriter detailsStringWriter = new StringWriter(CultureInfo.InvariantCulture);
                    XmlSerializer detailsSerializer = new XmlSerializer(typeof(List<DldErrorDataItem>));
                    detailsSerializer.Serialize(detailsStringWriter, furtherDetails);
                    errorDetails = detailsStringWriter.ToString();
                    break;
            }

            lock (this.dbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("update downloads set status=@status, errortime=datetime('now'), errortype=@errortype, errordetails=@errordetails, errorcount=errorcount+1, totalerrors=totalerrors+1 where epid=@epid", this.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Errored));
                    command.Parameters.Add(new SQLiteParameter("@errortype", errorType));
                    command.Parameters.Add(new SQLiteParameter("@errordetails", errorDetails));
                    command.Parameters.Add(new SQLiteParameter("@epid", this.curDldProgData.EpId));
                    command.ExecuteNonQuery();
                }
            }

            lock (this.downloadSortCacheLock)
            {
                this.downloadSortCache = null;
            }

            this.search.UpdateDownload(this.curDldProgData.EpId);

            if (this.search.DownloadIsVisible(this.curDldProgData.EpId))
            {
                if (this.DownloadUpdated != null)
                {
                    this.DownloadUpdated(this.curDldProgData.EpId);
                }
            }

            if (this.DownloadProgressTotal != null)
            {
                this.DownloadProgressTotal(false, 0);
            }

            this.downloadThread = null;
            this.curDldProgData = null;

            this.StartDownloadAsync();
        }

        private void DownloadPluginInst_Finished(string fileExtension)
        {
            this.curDldProgData.FinalName += "." + fileExtension;

            lock (this.dbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("update downloads set status=@status, filepath=@filepath where epid=@epid", this.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Downloaded));
                    command.Parameters.Add(new SQLiteParameter("@filepath", this.curDldProgData.FinalName));
                    command.Parameters.Add(new SQLiteParameter("@epid", this.curDldProgData.EpId));
                    command.ExecuteNonQuery();
                }
            }

            lock (this.downloadSortCacheLock)
            {
                this.downloadSortCache = null;
            }

            this.search.UpdateDownload(this.curDldProgData.EpId);

            if (this.search.DownloadIsVisible(this.curDldProgData.EpId))
            {
                if (this.DownloadUpdated != null)
                {
                    this.DownloadUpdated(this.curDldProgData.EpId);
                }
            }

            if (this.DownloadProgressTotal != null)
            {
                this.DownloadProgressTotal(false, 100);
            }

            // If the episode's programme is a subscription, clear the sort cache and raise an updated event
            using (SQLiteCommand command = new SQLiteCommand("select subscriptions.progid from episodes, subscriptions where epid=@epid and subscriptions.progid = episodes.progid", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@epid", this.curDldProgData.EpId));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read())
                    {
                        lock (this.subscriptionSortCacheLock)
                        {
                            this.subscriptionSortCache = null;
                        }

                        if (this.SubscriptionUpdated != null)
                        {
                            this.SubscriptionUpdated(reader.GetInt32(reader.GetOrdinal("progid")));
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.RunAfterCommand))
            {
                try
                {
                    // Environ("comspec") will give the path to cmd.exe or command.com
                    Interaction.Shell("\"" + Interaction.Environ("comspec") + "\" /c " + Properties.Settings.Default.RunAfterCommand.Replace("%file%", this.curDldProgData.FinalName), AppWinStyle.NormalNoFocus);
                }
                catch
                {
                    // Just ignore the error, as it just means that something has gone wrong with the run after command.
                }
            }

            this.downloadThread = null;
            this.curDldProgData = null;

            this.StartDownloadAsync();
        }

        private void DownloadPluginInst_Progress(int percent, string statusText, ProgressIcon icon)
        {
            // Don't raise the progress event if the value is the same as last time, or is outside the range
            if (percent == this.lastProgressVal || percent < 0 || percent > 100)
            {
                return;
            }

            this.lastProgressVal = percent;

            if (this.search.DownloadIsVisible(this.curDldProgData.EpId))
            {
                if (this.DownloadProgress != null)
                {
                    this.DownloadProgress(this.curDldProgData.EpId, percent, statusText, icon);
                }
            }

            if (this.DownloadProgressTotal != null)
            {
                this.DownloadProgressTotal(true, percent);
            }
        }

        private void SetDBSetting(string propertyName, object value)
        {
            lock (this.dbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("insert or replace into settings (property, value) values (@property, @value)", this.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@property", propertyName));
                    command.Parameters.Add(new SQLiteParameter("@value", value));
                    command.ExecuteNonQuery();
                }
            }
        }

        private object GetDBSetting(string propertyName)
        {
            using (SQLiteCommand command = new SQLiteCommand("select value from settings where property=@property", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@property", propertyName));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read() == false)
                    {
                        return null;
                    }

                    return reader.GetValue(reader.GetOrdinal("value"));
                }
            }
        }

        private void VacuumDatabase()
        {
            // Vacuum the database every few months - vacuums are spaced like this as they take ages to run
            bool runVacuum = false;
            object lastVacuum = this.GetDBSetting("lastvacuum");

            if (lastVacuum == null)
            {
                runVacuum = true;
            }
            else
            {
                runVacuum = DateTime.ParseExact((string)lastVacuum, "O", CultureInfo.InvariantCulture).AddMonths(3) < DateAndTime.Now;
            }

            if (runVacuum)
            {
                using (Status showStatus = new Status())
                {
                    showStatus.StatusText = "Compacting Database..." + Environment.NewLine + Environment.NewLine + "This may take some time if you have downloaded a lot of programmes.";
                    showStatus.ProgressBarMarquee = true;
                    showStatus.Show();
                    Application.DoEvents();

                    // Make SQLite recreate the database to reduce the size on disk and remove fragmentation
                    lock (this.dbUpdateLock)
                    {
                        using (SQLiteCommand command = new SQLiteCommand("vacuum", this.FetchDbConn()))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    this.SetDBSetting("lastvacuum", DateAndTime.Now.ToString("O", CultureInfo.InvariantCulture));

                    showStatus.Hide();
                    Application.DoEvents();
                }
            }
        }

        private void FindNewPluginInst_FindNewException(Exception exception, bool unhandled)
        {
            ErrorReporting reportException = new ErrorReporting("Find New Error", exception);

            if (unhandled)
            {
                ErrorReporting report = new ErrorReporting(exception);

                using (ReportError showError = new ReportError())
                {
                    showError.ShowReport(report);
                }
            }
            else
            {
                reportException.SendReport(Properties.Settings.Default.ErrorReportURL);
            }
        }

        private void FindNewPluginInst_FindNewViewChange(object view)
        {
            if (this.FindNewViewChange != null)
            {
                this.FindNewViewChange(view);
            }
        }

        private void FindNewPluginInst_FoundNew(string progExtId)
        {
            Guid pluginId = this.findNewPluginInst.ProviderId;
            int? progid = this.StoreProgrammeInfo(pluginId, progExtId);

            if (progid == null)
            {
                Interaction.MsgBox("There was a problem retrieving information about this programme.  You might like to try again later.", MsgBoxStyle.Exclamation);
                return;
            }

            if (this.FoundNew != null)
            {
                this.FoundNew(progid.Value);
            }
        }

        private int? StoreProgrammeInfo(Guid pluginId, string progExtId)
        {
            if (this.pluginsInst.PluginExists(pluginId) == false)
            {
                return null;
            }

            IRadioProvider pluginInstance = this.pluginsInst.GetPluginInstance(pluginId);
            GetProgrammeInfoReturn progInfo = default(GetProgrammeInfoReturn);

            progInfo = pluginInstance.GetProgrammeInfo(progExtId);

            if (progInfo.Success == false)
            {
                return null;
            }

            int? progid = null;

            lock (this.dbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("select progid from programmes where pluginid=@pluginid and extid=@extid", this.FetchDbConn()))
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

                using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(this.FetchDbConn().BeginTransaction()))
                {
                    if (progid == null)
                    {
                        using (SQLiteCommand command = new SQLiteCommand("insert into programmes (pluginid, extid) values (@pluginid, @extid)", this.FetchDbConn()))
                        {
                            command.Parameters.Add(new SQLiteParameter("@pluginid", pluginId.ToString()));
                            command.Parameters.Add(new SQLiteParameter("@extid", progExtId));
                            command.ExecuteNonQuery();
                        }

                        using (SQLiteCommand command = new SQLiteCommand("select last_insert_rowid()", this.FetchDbConn()))
                        {
                            progid = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
                        }
                    }

                    using (SQLiteCommand command = new SQLiteCommand("update programmes set name=@name, description=@description, image=@image, singleepisode=@singleepisode, lastupdate=@lastupdate where progid=@progid", this.FetchDbConn()))
                    {
                        command.Parameters.Add(new SQLiteParameter("@name", progInfo.ProgrammeInfo.Name));
                        command.Parameters.Add(new SQLiteParameter("@description", progInfo.ProgrammeInfo.Description));
                        command.Parameters.Add(new SQLiteParameter("@image", this.StoreImage(progInfo.ProgrammeInfo.Image)));
                        command.Parameters.Add(new SQLiteParameter("@singleepisode", progInfo.ProgrammeInfo.SingleEpisode));
                        command.Parameters.Add(new SQLiteParameter("@lastupdate", DateAndTime.Now));
                        command.Parameters.Add(new SQLiteParameter("@progid", progid));
                        command.ExecuteNonQuery();
                    }

                    transMon.Trans.Commit();
                }
            }

            // If the programme is in the list of favourites, clear the sort cache and raise an updated event
            if (this.IsFavourite(progid.Value))
            {
                lock (this.favouriteSortCacheLock)
                {
                    this.favouriteSortCache = null;
                }

                if (this.FavouriteUpdated != null)
                {
                    this.FavouriteUpdated(progid.Value);
                }
            }

            // If the programme is in the list of subscriptions, clear the sort cache and raise an updated event
            if (this.IsSubscribed(progid.Value))
            {
                lock (this.subscriptionSortCacheLock)
                {
                    this.subscriptionSortCache = null;
                }

                if (this.SubscriptionUpdated != null)
                {
                    this.SubscriptionUpdated(progid.Value);
                }
            }

            return progid;
        }

        private int? StoreImage(Bitmap image)
        {
            if (image == null)
            {
                return null;
            }

            // Convert the image into a byte array
            byte[] imageAsBytes = null;

            using (MemoryStream memstream = new MemoryStream())
            {
                image.Save(memstream, System.Drawing.Imaging.ImageFormat.Png);
                imageAsBytes = memstream.ToArray();
            }

            lock (this.dbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("select imgid from images where image=@image", this.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@image", imageAsBytes));

                    using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt32(reader.GetOrdinal("imgid"));
                        }
                    }
                }

                using (SQLiteCommand command = new SQLiteCommand("insert into images (image) values (@image)", this.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@image", imageAsBytes));
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand command = new SQLiteCommand("select last_insert_rowid()", this.FetchDbConn()))
                {
                    return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
                }
            }
        }

        private Bitmap RetrieveImage(int imgid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select image from images where imgid=@imgid", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@imgid", imgid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    // Get the size of the image data by passing nothing to getbytes
                    int dataLength = Convert.ToInt32(reader.GetBytes(reader.GetOrdinal("image"), 0, null, 0, 0));
                    byte[] content = new byte[dataLength];

                    reader.GetBytes(reader.GetOrdinal("image"), 0, content, 0, dataLength);

                    using (MemoryStream contentStream = new MemoryStream(content))
                    {
                        return new Bitmap(contentStream);
                    }
                }
            }
        }

        private List<string> GetAvailableEpisodes(Guid providerId, string progExtId)
        {
            if (this.pluginsInst.PluginExists(providerId) == false)
            {
                return null;
            }

            string[] extIds = null;
            IRadioProvider providerInst = this.pluginsInst.GetPluginInstance(providerId);

            extIds = providerInst.GetAvailableEpisodeIds(progExtId);

            if (extIds == null)
            {
                return null;
            }

            // Remove any duplicates from the list of episodes
            List<string> extIdsUnique = new List<string>();

            foreach (string removeDups in extIds)
            {
                if (extIdsUnique.Contains(removeDups) == false)
                {
                    extIdsUnique.Add(removeDups);
                }
            }

            return extIdsUnique;
        }

        private int StoreEpisodeInfo(Guid pluginId, int progid, string progExtId, string episodeExtId)
        {
            IRadioProvider providerInst = this.pluginsInst.GetPluginInstance(pluginId);
            GetEpisodeInfoReturn episodeInfoReturn = default(GetEpisodeInfoReturn);

            episodeInfoReturn = providerInst.GetEpisodeInfo(progExtId, episodeExtId);

            if (episodeInfoReturn.Success == false)
            {
                return -1;
            }

            if (string.IsNullOrEmpty(episodeInfoReturn.EpisodeInfo.Name))
            {
                throw new InvalidDataException("Episode name cannot be null or an empty string");
            }

            if (episodeInfoReturn.EpisodeInfo.Date == null)
            {
                throw new InvalidDataException("Episode date cannot be null ");
            }

            lock (this.dbUpdateLock)
            {
                using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(this.FetchDbConn().BeginTransaction()))
                {
                    int epid = 0;

                    using (SQLiteCommand addEpisodeCmd = new SQLiteCommand("insert into episodes (progid, extid, name, description, duration, date, image) values (@progid, @extid, @name, @description, @duration, @date, @image)", this.FetchDbConn(), transMon.Trans))
                    {
                        addEpisodeCmd.Parameters.Add(new SQLiteParameter("@progid", progid));
                        addEpisodeCmd.Parameters.Add(new SQLiteParameter("@extid", episodeExtId));
                        addEpisodeCmd.Parameters.Add(new SQLiteParameter("@name", episodeInfoReturn.EpisodeInfo.Name));
                        addEpisodeCmd.Parameters.Add(new SQLiteParameter("@description", episodeInfoReturn.EpisodeInfo.Description));
                        addEpisodeCmd.Parameters.Add(new SQLiteParameter("@duration", episodeInfoReturn.EpisodeInfo.DurationSecs));
                        addEpisodeCmd.Parameters.Add(new SQLiteParameter("@date", episodeInfoReturn.EpisodeInfo.Date));
                        addEpisodeCmd.Parameters.Add(new SQLiteParameter("@image", this.StoreImage(episodeInfoReturn.EpisodeInfo.Image)));
                        addEpisodeCmd.ExecuteNonQuery();
                    }

                    using (SQLiteCommand getRowIDCmd = new SQLiteCommand("select last_insert_rowid()", this.FetchDbConn(), transMon.Trans))
                    {
                        epid = Convert.ToInt32(getRowIDCmd.ExecuteScalar(), CultureInfo.InvariantCulture);
                    }

                    if (episodeInfoReturn.EpisodeInfo.ExtInfo != null)
                    {
                        using (SQLiteCommand addExtInfoCmd = new SQLiteCommand("insert into episodeext (epid, name, value) values (@epid, @name, @value)", this.FetchDbConn(), transMon.Trans))
                        {
                            foreach (KeyValuePair<string, string> extItem in episodeInfoReturn.EpisodeInfo.ExtInfo)
                            {
                                addExtInfoCmd.Parameters.Add(new SQLiteParameter("@epid", epid));
                                addExtInfoCmd.Parameters.Add(new SQLiteParameter("@name", extItem.Key));
                                addExtInfoCmd.Parameters.Add(new SQLiteParameter("@value", extItem.Value));
                                addExtInfoCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    transMon.Trans.Commit();
                    return epid;
                }
            }
        }

        private void InitEpisodeListThread(int progid)
        {
            Guid providerId = default(Guid);
            string progExtId = null;

            using (SQLiteCommand command = new SQLiteCommand("select pluginid, extid from programmes where progid=@progid", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (reader.Read() == false)
                    {
                        return;
                    }

                    providerId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));
                    progExtId = reader.GetString(reader.GetOrdinal("extid"));
                }
            }

            List<string> episodeExtIDs = this.GetAvailableEpisodes(providerId, progExtId);

            if (episodeExtIDs != null)
            {
                using (SQLiteCommand findCmd = new SQLiteCommand("select epid from episodes where progid=@progid and extid=@extid", this.FetchDbConn()))
                {
                    SQLiteParameter progidParam = new SQLiteParameter("@progid");
                    SQLiteParameter extidParam = new SQLiteParameter("@extid");

                    findCmd.Parameters.Add(progidParam);
                    findCmd.Parameters.Add(extidParam);

                    foreach (string episodeExtId in episodeExtIDs)
                    {
                        progidParam.Value = progid;
                        extidParam.Value = episodeExtId;

                        bool needEpInfo = true;
                        int epid = 0;

                        using (SQLiteMonDataReader reader = new SQLiteMonDataReader(findCmd.ExecuteReader()))
                        {
                            if (reader.Read())
                            {
                                needEpInfo = false;
                                epid = reader.GetInt32(reader.GetOrdinal("epid"));
                            }
                        }

                        if (needEpInfo)
                        {
                            epid = this.StoreEpisodeInfo(providerId, progid, progExtId, episodeExtId);

                            if (epid < 0)
                            {
                                continue;
                            }
                        }

                        lock (this.episodeListThreadLock)
                        {
                            if (!object.ReferenceEquals(Thread.CurrentThread, this.episodeListThread))
                            {
                                return;
                            }

                            if (this.EpisodeAdded != null)
                            {
                                this.EpisodeAdded(epid);
                            }
                        }
                    }
                }
            }
        }

        private DownloadData ReadDownloadData(int epid, SQLiteMonDataReader reader)
        {
            int descriptionOrdinal = reader.GetOrdinal("description");
            int filepathOrdinal = reader.GetOrdinal("filepath");

            DownloadData info = new DownloadData();
            info.Epid = epid;
            info.EpisodeDate = reader.GetDateTime(reader.GetOrdinal("date"));
            info.Name = TextUtils.StripDateFromName(reader.GetString(reader.GetOrdinal("name")), info.EpisodeDate);

            if (!reader.IsDBNull(descriptionOrdinal))
            {
                info.Description = reader.GetString(descriptionOrdinal);
            }

            info.Duration = reader.GetInt32(reader.GetOrdinal("duration"));
            info.Status = (DownloadStatus)reader.GetInt32(reader.GetOrdinal("status"));

            if (info.Status == DownloadStatus.Errored)
            {
                info.ErrorType = (ErrorType)reader.GetInt32(reader.GetOrdinal("errortype"));

                if (info.ErrorType != ErrorType.UnknownError)
                {
                    info.ErrorDetails = reader.GetString(reader.GetOrdinal("errordetails"));
                }
            }

            if (!reader.IsDBNull(filepathOrdinal))
            {
                info.DownloadPath = reader.GetString(filepathOrdinal);
            }

            info.PlayCount = reader.GetInt32(reader.GetOrdinal("playcount"));

            return info;
        }

        private FavouriteData ReadFavouriteData(int progid, SQLiteMonDataReader reader)
        {
            int descriptionOrdinal = reader.GetOrdinal("description");

            FavouriteData info = new FavouriteData();
            info.Progid = progid;
            info.Name = reader.GetString(reader.GetOrdinal("name"));

            if (!reader.IsDBNull(descriptionOrdinal))
            {
                info.Description = reader.GetString(descriptionOrdinal);
            }

            info.SingleEpisode = reader.GetBoolean(reader.GetOrdinal("singleepisode"));

            Guid pluginId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));
            IRadioProvider providerInst = this.pluginsInst.GetPluginInstance(pluginId);
            info.ProviderName = providerInst.ProviderName;

            info.Subscribed = this.IsSubscribed(progid);

            return info;
        }

        private bool IsFavourite(int progid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select count(*) from favourites where progid=@progid", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
            }
        }

        private bool IsSubscribed(int progid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select count(*) from subscriptions where progid=@progid", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@progid", progid));
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
            }
        }

        public struct ProviderData
        {
            public string Name;
            public string Description;
            public Bitmap Icon;
            public EventHandler ShowOptionsHandler;
        }

        public struct EpisodeData
        {
            public string Name;
            public string Description;
            public System.DateTime EpisodeDate;
            public int Duration;
            public bool AutoDownload;
        }

        public struct ProgrammeData
        {
            public string Name;
            public string Description;
            public bool Favourite;
            public bool Subscribed;
            public bool SingleEpisode;
        }

        public struct FavouriteData
        {
            public int Progid;
            public string Name;
            public string Description;
            public bool Subscribed;
            public bool SingleEpisode;
            public string ProviderName;
        }

        public struct SubscriptionData
        {
            public string Name;
            public string Description;
            public bool Favourite;
            public DateTime? LatestDownload;
            public string ProviderName;
        }

        public struct DownloadData
        {
            public int Epid;
            public string Name;
            public string Description;
            public int Duration;
            public System.DateTime EpisodeDate;
            public DownloadStatus Status;
            public ErrorType ErrorType;
            public string ErrorDetails;
            public string DownloadPath;
            public int PlayCount;
        }
    }
}
