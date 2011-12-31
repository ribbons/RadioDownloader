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
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml.Serialization;

    internal class Download : Episode
    {
        internal new const string Columns = Episode.Columns + ", status, errortype, errordetails, filepath, playcount";

        private static Dictionary<int, int> sortCache;
        private static object sortCacheLock = new object();

        private static DownloadCols sortBy = DownloadCols.EpisodeDate;
        private static bool sortAsc;

        static Download()
        {
            Episode.Updated += Episode_Updated;
        }

        public Download(SQLiteMonDataReader reader)
        {
            this.FetchData(reader);
        }

        public Download(int epid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select " + Columns + " from episodes, downloads where episodes.epid=@epid and downloads.epid=episodes.epid", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@epid", epid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (!reader.Read())
                    {
                        throw new DataNotFoundException(epid, "Download does not exist");
                    }

                    this.FetchData(reader);
                }
            }
        }

        public static event EpisodeEventHandler Added;

        public static new event EpisodeEventHandler Updated;

        public static event EpisodeEventHandler Removed;

        public enum DownloadCols
        {
            EpisodeName = 0,
            EpisodeDate = 1,
            Status = 2,
            Progress = 3,
            Duration = 4
        }

        public enum DownloadStatus
        {
            Waiting = 0,
            Downloaded = 1,
            Errored = 2
        }

        public static DownloadCols SortByColumn
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

        public DownloadStatus Status { get; set; }

        public ErrorType ErrorType { get; set; }

        public string ErrorDetails { get; set; }

        public string DownloadPath { get; set; }

        public int PlayCount { get; set; }

        public static long Count()
        {
            using (SQLiteCommand command = new SQLiteCommand("select count(*) from episodes, downloads where downloads.epid=episodes.epid", FetchDbConn()))
            {
                return (long)command.ExecuteScalar();
            }
        }

        public static long CountNew()
        {
            using (SQLiteCommand command = new SQLiteCommand("select count(*) from episodes, downloads where downloads.epid=episodes.epid and playcount=0 and status=@status", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Downloaded));
                return (long)command.ExecuteScalar();
            }
        }

        public static long CountErrored()
        {
            using (SQLiteCommand command = new SQLiteCommand("select count(*) from episodes, downloads where downloads.epid=episodes.epid and status=@status", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Errored));
                return (long)command.ExecuteScalar();
            }
        }

        public static List<Download> FetchAll()
        {
            List<Download> items = new List<Download>();

            using (SQLiteCommand command = new SQLiteCommand("select " + Columns + " from episodes, downloads where downloads.epid=episodes.epid", FetchDbConn()))
            {
                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    while (reader.Read())
                    {
                        items.Add(new Download(reader));
                    }
                }
            }

            return items;
        }

        public static List<Download> FetchVisible(DataSearch dataSearch)
        {
            List<Download> items = new List<Download>();

            using (SQLiteCommand command = new SQLiteCommand("select " + Columns + " from episodes, downloads where downloads.epid=episodes.epid", FetchDbConn()))
            {
                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    int epidOrdinal = reader.GetOrdinal("epid");

                    while (reader.Read())
                    {
                        if (dataSearch.DownloadIsVisible(reader.GetInt32(epidOrdinal)))
                        {
                            items.Add(new Download(reader));
                        }
                    }
                }
            }

            return items;
        }

        public static bool IsDownload(int epid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select count(*) from downloads where epid=@epid", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@epid", epid));
                return (long)command.ExecuteScalar() != 0;
            }
        }

        public static bool Add(int[] epids)
        {
            List<int> addEpids = new List<int>();

            foreach (int epid in epids)
            {
                if (!IsDownload(epid))
                {
                    addEpids.Add(epid);
                }
            }

            if (addEpids.Count == 0)
            {
                return false;
            }

            ThreadPool.QueueUserWorkItem(delegate { AddAsync(addEpids.ToArray()); });

            return true;
        }

        public static void SetComplete(int epid, string fileName)
        {
            lock (DbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("update downloads set status=@status, filepath=@filepath where epid=@epid", FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Downloaded));
                    command.Parameters.Add(new SQLiteParameter("@filepath", fileName));
                    command.Parameters.Add(new SQLiteParameter("@epid", epid));
                    command.ExecuteNonQuery();
                }
            }

            lock (sortCacheLock)
            {
                sortCache = null;
            }

            if (Updated != null)
            {
                Updated(epid);
            }
        }

        public static void SetErrorred(int epid, ErrorType errorType, string errorDetails, List<DldErrorDataItem> furtherDetails)
        {
            switch (errorType)
            {
                case ErrorType.RemoveFromList:
                    RemoveAsync(epid, true);
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

            lock (DbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("update downloads set status=@status, errortime=datetime('now'), errortype=@errortype, errordetails=@errordetails, errorcount=errorcount+1, totalerrors=totalerrors+1 where epid=@epid", FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Errored));
                    command.Parameters.Add(new SQLiteParameter("@errortype", errorType));
                    command.Parameters.Add(new SQLiteParameter("@errordetails", errorDetails));
                    command.Parameters.Add(new SQLiteParameter("@epid", epid));
                    command.ExecuteNonQuery();
                }
            }

            lock (sortCacheLock)
            {
                sortCache = null;
            }

            if (Updated != null)
            {
                Updated(epid);
            }
        }

        public static void Reset(int epid)
        {
            ThreadPool.QueueUserWorkItem(delegate { ResetAsync(epid, false); });
        }

        public static void ResetAsync(int epid, bool auto)
        {
            lock (DbUpdateLock)
            {
                using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(FetchDbConn().BeginTransaction()))
                {
                    using (SQLiteCommand command = new SQLiteCommand("update downloads set status=@status, errortype=null, errortime=null, errordetails=null where epid=@epid", FetchDbConn(), transMon.Trans))
                    {
                        command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Waiting));
                        command.Parameters.Add(new SQLiteParameter("@epid", epid));
                        command.ExecuteNonQuery();
                    }

                    if (!auto)
                    {
                        using (SQLiteCommand command = new SQLiteCommand("update downloads set errorcount=0 where epid=@epid", FetchDbConn(), transMon.Trans))
                        {
                            command.Parameters.Add(new SQLiteParameter("@epid", epid));
                            command.ExecuteNonQuery();
                        }
                    }

                    transMon.Trans.Commit();
                }
            }

            lock (sortCacheLock)
            {
                sortCache = null;
            }

            if (Updated != null)
            {
                Updated(epid);
            }

            if (!auto)
            {
                Data.GetInstance().StartDownload();
            }
        }

        public static void BumpPlayCount(int epid)
        {
            ThreadPool.QueueUserWorkItem(delegate { BumpPlayCountAsync(epid); });
        }

        public static void Remove(int epid)
        {
            ThreadPool.QueueUserWorkItem(delegate { RemoveAsync(epid, false); });
        }

        public static string FindFreeSaveFileName(string formatString, Model.Programme progInfo, Model.Episode epInfo, string baseSavePath)
        {
            string rootName = Path.Combine(baseSavePath, CreateSaveFileName(formatString, progInfo, epInfo));
            string savePath = rootName;

            // Make sure the save folder exists (to support subfolders in the save file name template)
            string saveDir = Path.GetDirectoryName(savePath);
            Directory.CreateDirectory(saveDir);

            string currentFileName = null;

            // If the passed episode info is actually a download, get it's current path
            if (typeof(Download) == epInfo.GetType())
            {
                currentFileName = ((Download)epInfo).DownloadPath;

                // Remove the extension from the current name if applicable
                if (currentFileName != null)
                {
                    int extensionPos = currentFileName.LastIndexOf('.');

                    if (extensionPos > -1)
                    {
                        currentFileName = currentFileName.Substring(0, extensionPos);
                    }
                }
            }

            int diffNum = 1;

            // Check for a pre-existing file with the same name (ignoring the current name for this file)
            while (Directory.GetFiles(saveDir, Path.GetFileName(savePath) + ".*").Length > 0 &&
                   savePath != currentFileName)
            {
                savePath = rootName + " (" + Convert.ToString(diffNum, CultureInfo.CurrentCulture) + ")";
                diffNum += 1;
            }

            return savePath;
        }

        public static string CreateSaveFileName(string formatString, Model.Programme progInfo, Model.Episode epInfo)
        {
            if (string.IsNullOrEmpty(formatString))
            {
                // The format string is an empty string, so the output must be an empty string
                return string.Empty;
            }

            string fileName = formatString;

            // Convert %title% -> %epname% for backwards compatability
            fileName = fileName.Replace("%title%", "%epname%");

            // Make variable substitutions
            fileName = fileName.Replace("%progname%", progInfo.Name);
            fileName = fileName.Replace("%epname%", epInfo.Name);
            fileName = fileName.Replace("%hour%", epInfo.EpisodeDate.ToString("HH", CultureInfo.CurrentCulture));
            fileName = fileName.Replace("%minute%", epInfo.EpisodeDate.ToString("mm", CultureInfo.CurrentCulture));
            fileName = fileName.Replace("%day%", epInfo.EpisodeDate.ToString("dd", CultureInfo.CurrentCulture));
            fileName = fileName.Replace("%month%", epInfo.EpisodeDate.ToString("MM", CultureInfo.CurrentCulture));
            fileName = fileName.Replace("%shortmonthname%", epInfo.EpisodeDate.ToString("MMM", CultureInfo.CurrentCulture));
            fileName = fileName.Replace("%monthname%", epInfo.EpisodeDate.ToString("MMMM", CultureInfo.CurrentCulture));
            fileName = fileName.Replace("%year%", epInfo.EpisodeDate.ToString("yy", CultureInfo.CurrentCulture));
            fileName = fileName.Replace("%longyear%", epInfo.EpisodeDate.ToString("yyyy", CultureInfo.CurrentCulture));

            // Replace invalid file name characters with spaces (except for directory separators
            // as this then allows the flexibility of storing the downloads in subdirectories)
            foreach (char removeChar in Path.GetInvalidFileNameChars())
            {
                if (removeChar != Path.DirectorySeparatorChar)
                {
                    fileName = fileName.Replace(removeChar, ' ');
                }
            }

            // Replace runs of spaces with a single space
            fileName = Regex.Replace(fileName, " {2,}", " ");

            return fileName.Trim();
        }

        public static void UpdatePaths(Status status, string newPath, string newFormat)
        {
            status.StatusText = "Fetching downloads...";

            List<Download> downloads = new List<Download>();

            using (SQLiteCommand command = new SQLiteCommand("select episodes.epid, progid, name, description, date, duration, autodownload, status, errortype, errordetails, filepath, playcount from episodes, downloads where episodes.epid=downloads.epid", FetchDbConn()))
            {
                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    while (reader.Read())
                    {
                        downloads.Add(new Download(reader));
                    }
                }
            }

            if (downloads.Count > 0)
            {
                Dictionary<int, Programme> programmes = new Dictionary<int, Programme>();
                int progress = 0;

                status.StatusText = "Updating download paths...";
                status.ProgressBarMax = downloads.Count;
                status.ProgressBarMarquee = false;

                using (SQLiteCommand command = new SQLiteCommand("update downloads set filepath=@filepath where epid=@epid", FetchDbConn()))
                {
                    SQLiteParameter epidParam = new SQLiteParameter("epid");
                    SQLiteParameter filepathParam = new SQLiteParameter("filepath");

                    command.Parameters.Add(epidParam);
                    command.Parameters.Add(filepathParam);

                    foreach (Download download in downloads)
                    {
                        if (File.Exists(download.DownloadPath))
                        {
                            if (!programmes.ContainsKey(download.Progid))
                            {
                                programmes.Add(download.Progid, new Programme(download.Progid));
                            }

                            string newDownloadPath = FindFreeSaveFileName(newFormat, programmes[download.Progid], download, newPath) + Path.GetExtension(download.DownloadPath);

                            if (newDownloadPath != download.DownloadPath)
                            {
                                lock (DbUpdateLock)
                                {
                                    using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(FetchDbConn().BeginTransaction()))
                                    {
                                        epidParam.Value = download.Epid;
                                        filepathParam.Value = newDownloadPath;
                                        command.ExecuteNonQuery();

                                        File.Move(download.DownloadPath, newDownloadPath);
                                        transMon.Trans.Commit();
                                    }
                                }
                            }
                        }

                        status.ProgressBarValue = ++progress;
                    }
                }

                status.ProgressBarValue = status.ProgressBarMax;
            }
        }

        public static void Cleanup(Status status, DateTime? olderThan, int? progid, bool orphans, bool played, bool noDeleteAudio)
        {
            if (!(olderThan != null || progid != null || orphans || played))
            {
                throw new ArgumentException("At least one kind of cleanup filter must be set!");
            }
            else if (orphans && noDeleteAudio)
            {
                throw new ArgumentException("Cannot remove orphans and not delete audio files at the same time!");
            }

            status.StatusText = "Fetching downloads...";

            string query = "select " + Columns + " from downloads, episodes where downloads.epid=episodes.epid and status=@status";

            if (olderThan != null)
            {
                query += " and date < @date";
            }

            if (progid != null)
            {
                query += " and progid=@progid";
            }

            if (played)
            {
                query += " and playcount > 0";
            }

            // Fetch a list of the downloads first to prevent locking the database during cleanup
            List<Download> downloads = new List<Download>();

            using (SQLiteCommand command = new SQLiteCommand(query, FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Downloaded));

                if (olderThan != null)
                {
                    command.Parameters.Add(new SQLiteParameter("@date", olderThan.Value));
                }

                if (progid != null)
                {
                    command.Parameters.Add(new SQLiteParameter("@progid", progid.Value));
                }

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    while (reader.Read())
                    {
                        downloads.Add(new Download(reader));
                    }
                }
            }

            status.ProgressBarMax = downloads.Count;
            status.StatusText = "Cleaning up downloads...";
            status.ProgressBarMarquee = false;

            foreach (Download download in downloads)
            {
                List<string> ignoreRoots = new List<string>();

                if (!orphans || !File.Exists(download.DownloadPath))
                {
                    if (orphans)
                    {
                        string pathRoot = Path.GetPathRoot(download.DownloadPath);

                        if (!Directory.Exists(pathRoot) && !ignoreRoots.Contains(pathRoot))
                        {
                            if (MessageBox.Show("\"" + pathRoot + "\" does not currently appear to be available." + Environment.NewLine + Environment.NewLine + "Continue cleaning up anyway?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                            {
                                break;
                            }

                            ignoreRoots.Add(pathRoot);
                        }
                    }

                    // Take the download out of the list and set the auto download flag to false
                    RemoveAsync(download.Epid, false);

                    // Delete the audio file too (if it exists, and the user wants to remove it)
                    if (!orphans && !noDeleteAudio)
                    {
                        File.Delete(download.DownloadPath);
                    }
                }

                status.ProgressBarValue++;
            }

            status.ProgressBarValue = status.ProgressBarMax;
        }

        public static int Compare(int epid1, int epid2)
        {
            lock (sortCacheLock)
            {
                if (sortCache == null || !sortCache.ContainsKey(epid1) || !sortCache.ContainsKey(epid2))
                {
                    // The sort cache is either empty or missing one of the values that are required, so recreate it
                    sortCache = new Dictionary<int, int>();

                    int sort = 0;
                    string orderBy = null;

                    switch (sortBy)
                    {
                        case DownloadCols.EpisodeName:
                            orderBy = "name" + (sortAsc ? string.Empty : " desc");
                            break;
                        case DownloadCols.EpisodeDate:
                            orderBy = "date" + (sortAsc ? string.Empty : " desc");
                            break;
                        case DownloadCols.Status:
                            orderBy = "status = 0" + (sortAsc ? " desc" : string.Empty) + ", status" + (sortAsc ? " desc" : string.Empty) + ", playcount > 0" + (sortAsc ? string.Empty : " desc") + ", date" + (sortAsc ? " desc" : string.Empty);
                            break;
                        case DownloadCols.Duration:
                            orderBy = "duration" + (sortAsc ? string.Empty : " desc");
                            break;
                        default:
                            throw new InvalidDataException("Invalid column: " + sortBy.ToString());
                    }

                    using (SQLiteCommand command = new SQLiteCommand("select downloads.epid from downloads, episodes where downloads.epid=episodes.epid order by " + orderBy, FetchDbConn()))
                    {
                        using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                        {
                            int epidOrdinal = reader.GetOrdinal("epid");

                            while (reader.Read())
                            {
                                sortCache.Add(reader.GetInt32(epidOrdinal), sort);
                                sort += 1;
                            }
                        }
                    }
                }

                try
                {
                    return sortCache[epid1] - sortCache[epid2];
                }
                catch (KeyNotFoundException)
                {
                    // One of the entries has been removed from the database, but not yet from the list
                    return 0;
                }
            }
        }

        protected new void FetchData(SQLiteMonDataReader reader)
        {
            base.FetchData(reader);

            int filepathOrdinal = reader.GetOrdinal("filepath");

            this.Status = (DownloadStatus)reader.GetInt32(reader.GetOrdinal("status"));

            if (this.Status == DownloadStatus.Errored)
            {
                this.ErrorType = (ErrorType)reader.GetInt32(reader.GetOrdinal("errortype"));

                if (this.ErrorType != ErrorType.UnknownError)
                {
                    this.ErrorDetails = reader.GetString(reader.GetOrdinal("errordetails"));
                }
            }

            if (!reader.IsDBNull(filepathOrdinal))
            {
                this.DownloadPath = reader.GetString(filepathOrdinal);
            }

            this.PlayCount = reader.GetInt32(reader.GetOrdinal("playcount"));
        }

        private static void Episode_Updated(int epid)
        {
            if (IsDownload(epid))
            {
                lock (sortCacheLock)
                {
                    sortCache = null;
                }

                if (Updated != null)
                {
                    Updated(epid);
                }
            }
        }

        private static void AddAsync(int[] epids)
        {
            List<int> added = new List<int>();

            lock (DbUpdateLock)
            {
                using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(FetchDbConn().BeginTransaction()))
                {
                    using (SQLiteCommand command = new SQLiteCommand("insert into downloads (epid) values (@epid)", FetchDbConn()))
                    {
                        SQLiteParameter epidParam = new SQLiteParameter("@epid");
                        command.Parameters.Add(epidParam);

                        foreach (int epid in epids)
                        {
                            epidParam.Value = epid;

                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (SQLiteException sqliteExp)
                            {
                                if (sqliteExp.ErrorCode == SQLiteErrorCode.Constraint)
                                {
                                    // Already added while this was waiting in the threadpool
                                    continue;
                                }

                                throw;
                            }

                            added.Add(epid);
                        }
                    }

                    transMon.Trans.Commit();
                }
            }

            if (Added != null)
            {
                foreach (int epid in added)
                {
                    Added(epid);
                }
            }
            
            Data.GetInstance().StartDownload();
        }

        private static void BumpPlayCountAsync(int epid)
        {
            lock (DbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("update downloads set playcount=playcount+1 where epid=@epid", FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@epid", epid));
                    command.ExecuteNonQuery();
                }
            }

            lock (sortCacheLock)
            {
                sortCache = null;
            }

            if (Updated != null)
            {
                Updated(epid);
            }
        }

        private static void RemoveAsync(int epid, bool auto)
        {
            lock (DbUpdateLock)
            {
                using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(FetchDbConn().BeginTransaction()))
                {
                    using (SQLiteCommand command = new SQLiteCommand("delete from downloads where epid=@epid", FetchDbConn(), transMon.Trans))
                    {
                        command.Parameters.Add(new SQLiteParameter("@epid", epid));
                        command.ExecuteNonQuery();
                    }

                    if (!auto)
                    {
                        // Unset the auto download flag, so if the user is subscribed it doesn't just download again
                        Episode.SetAutoDownloadAsync(new int[] { epid }, false);
                    }

                    transMon.Trans.Commit();
                }
            }

            lock (sortCacheLock)
            {
                // No need to clear the sort cache, just remove this episodes entry
                if (sortCache != null)
                {
                    sortCache.Remove(epid);
                }
            }

            if (Removed != null)
            {
                Removed(epid);
            }

            Data.GetInstance().DownloadCancel(epid, auto);
        }
    }
}
