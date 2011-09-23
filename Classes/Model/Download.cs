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
    using System.Windows.Forms;

    internal class Download : Episode
    {
        internal new const string Columns = Episode.Columns + ", status, errortype, errordetails, filepath, playcount";

        public Download(SQLiteMonDataReader reader)
        {
            this.FetchData(reader);
        }

        public Download(int epid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select " + Columns + " from episodes, downloads where episodes.epid=@epid and downloads.epid=episodes.epid", Data.FetchDbConn()))
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

        public enum DownloadStatus
        {
            Waiting = 0,
            Downloaded = 1,
            Errored = 2
        }

        public DownloadStatus Status { get; set; }

        public ErrorType ErrorType { get; set; }

        public string ErrorDetails { get; set; }

        public string DownloadPath { get; set; }

        public int PlayCount { get; set; }

        public static int CountNew()
        {
            using (SQLiteCommand command = new SQLiteCommand("select count(epid) from downloads where playcount=0 and status=@status", Data.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@status", Model.Download.DownloadStatus.Downloaded));
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        public static int CountErrored()
        {
            using (SQLiteCommand command = new SQLiteCommand("select count(epid) from downloads where status=@status", Data.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@status", Model.Download.DownloadStatus.Errored));
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
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
            if (typeof(Model.Download) == epInfo.GetType())
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

        public static void UpdatePaths(string newPath, string newFormat)
        {
            List<Download> downloads = new List<Download>();

            using (SQLiteCommand command = new SQLiteCommand("select episodes.epid, progid, name, description, date, duration, autodownload, status, errortype, errordetails, filepath, playcount from episodes, downloads where episodes.epid=downloads.epid", Data.FetchDbConn()))
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

                using (Status showStatus = new Status())
                {
                    showStatus.StatusText = "Moving downloads...";
                    showStatus.ProgressBarMax = downloads.Count;
                    showStatus.Show();
                    Application.DoEvents();

                    using (SQLiteCommand command = new SQLiteCommand("update downloads set filepath=@filepath where epid=@epid", Data.FetchDbConn()))
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

                                string newDownloadPath = Download.FindFreeSaveFileName(newFormat, programmes[download.Progid], download, newPath) + Path.GetExtension(download.DownloadPath);

                                if (newDownloadPath != download.DownloadPath)
                                {
                                    lock (Data.DbUpdateLock)
                                    {
                                        using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(Data.FetchDbConn().BeginTransaction()))
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

                            showStatus.ProgressBarValue = ++progress;
                        }
                    }

                    showStatus.Hide();
                    Application.DoEvents();
                }
            }
        }

        internal new void FetchData(SQLiteMonDataReader reader)
        {
            base.FetchData(reader);

            int filepathOrdinal = reader.GetOrdinal("filepath");

            this.Status = (Model.Download.DownloadStatus)reader.GetInt32(reader.GetOrdinal("status"));

            if (this.Status == Model.Download.DownloadStatus.Errored)
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
    }
}
