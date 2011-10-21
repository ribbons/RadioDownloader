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

namespace RadioDld
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Globalization;
    using System.IO;
    using Microsoft.VisualBasic;

    internal class DataSearch
    {
        [ThreadStatic]
        private static SQLiteConnection dbConn;
        private static DataSearch searchInstance;

        private static object searchInstanceLock = new object();

        private string downloadQuery = string.Empty;

        private List<int> downloadsVisible;
        private object updateIndexLock = new object();
        private object downloadVisLock = new object();

        private DataSearch(Data instance)
        {
            Dictionary<string, string[]> tableCols = new Dictionary<string, string[]>();
            tableCols.Add("downloads", new string[] { "name", "description" });

            if (!this.CheckIndex(tableCols))
            {
                // Close & clean up the connection used for testing
                dbConn.Close();
                dbConn.Dispose();
                dbConn = null;
                
                using (Status status = new Status())
                {
                    status.ShowDialog(delegate
                    {
                        this.RebuildIndex(status, tableCols);
                    });
                }
            }

            Model.Download.Added += this.Download_Added;
            Model.Download.Updated += this.Download_Updated;
            Model.Download.Removed += this.Download_Removed;
        }

        public delegate void DownloadEventHandler(int epid);

        public event DownloadEventHandler DownloadAdded;

        public event DownloadEventHandler DownloadUpdated;

        public event DownloadEventHandler DownloadRemoved;

        public string DownloadQuery
        {
            get
            {
                return this.downloadQuery;
            }

            set
            {
                if (value != this.downloadQuery)
                {
                    try
                    {
                        this.RunQuery(value);
                        this.downloadQuery = value;
                    }
                    catch (SQLiteException)
                    {
                        // The search query is badly formed, so keep the old query
                    }
                }
            }
        }

        public static DataSearch GetInstance(Data instance)
        {
            // Need to use a lock instead of declaring the instance variable as New,
            // as otherwise New gets called before the Data class is ready
            lock (searchInstanceLock)
            {
                if (searchInstance == null)
                {
                    searchInstance = new DataSearch(instance);
                }

                return searchInstance;
            }
        }

        public bool DownloadIsVisible(int epid)
        {
            if (string.IsNullOrEmpty(this.downloadQuery))
            {
                return true;
            }

            lock (this.downloadVisLock)
            {
                if (this.downloadsVisible == null)
                {
                    this.RunQuery(this.downloadQuery);
                }

                return this.downloadsVisible.Contains(epid);
            }
        }

        private string DatabasePath()
        {
            return Path.Combine(FileUtils.GetAppDataFolder(), "searchindex.db");
        }

        private SQLiteConnection FetchDbConn()
        {
            if (dbConn == null)
            {
                dbConn = new SQLiteConnection("Data Source=" + this.DatabasePath() + ";Version=3");
                dbConn.Open();
            }

            return dbConn;
        }

        private bool CheckIndex(Dictionary<string, string[]> tableCols)
        {
            using (SQLiteCommand command = new SQLiteCommand("select count(*) from sqlite_master where type='table' and name=@name and sql=@sql", this.FetchDbConn()))
            {
                SQLiteParameter nameParam = new SQLiteParameter("@name");
                SQLiteParameter sqlParam = new SQLiteParameter("@sql");

                command.Parameters.Add(nameParam);
                command.Parameters.Add(sqlParam);

                foreach (KeyValuePair<string, string[]> table in tableCols)
                {
                    nameParam.Value = table.Key;
                    sqlParam.Value = this.TableSql(table.Key, table.Value);

                    if (Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) != 1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private string TableSql(string tableName, string[] columns)
        {
            return "CREATE VIRTUAL TABLE " + tableName + " USING fts3(" + Strings.Join(columns, ", ") + ")";
        }

        private void RebuildIndex(Status status, Dictionary<string, string[]> tableCols)
        {
            // Clean up the old index
            File.Delete(this.DatabasePath());

            status.StatusText = "Building search index...";
            status.ProgressBarMax = 100 * tableCols.Count;
            status.ProgressBarMarquee = false;

            lock (this.updateIndexLock)
            {
                using (SQLiteTransaction trans = this.FetchDbConn().BeginTransaction())
                {
                    // Create the index tables
                    foreach (KeyValuePair<string, string[]> table in tableCols)
                    {
                        using (SQLiteCommand command = new SQLiteCommand(this.TableSql(table.Key, table.Value), this.FetchDbConn(), trans))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    status.StatusText = "Building search index for downloads...";

                    int progress = 1;
                    List<Model.Download> downloadItems = Model.Download.FetchAll();

                    foreach (Model.Download downloadItem in downloadItems)
                    {
                        this.AddDownload(downloadItem);

                        status.ProgressBarValue = Convert.ToInt32((progress / downloadItems.Count) * 100);
                        progress += 1;
                    }

                    status.ProgressBarValue = 100;
                    trans.Commit();
                }
            }
        }

        private void RunQuery(string query)
        {
            lock (this.downloadVisLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("select docid from downloads where downloads match @query", this.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@query", query + "*"));

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        int docidOrdinal = reader.GetOrdinal("docid");

                        this.downloadsVisible = new List<int>();

                        while (reader.Read())
                        {
                            this.downloadsVisible.Add(reader.GetInt32(docidOrdinal));
                        }
                    }
                }
            }
        }

        private void Download_Added(int epid)
        {
            this.AddDownload(epid);

            if (this.DownloadAdded != null && this.DownloadIsVisible(epid))
            {
                this.DownloadAdded(epid);
            }
        }

        private void Download_Updated(int epid)
        {
            lock (this.updateIndexLock)
            {
                using (SQLiteTransaction trans = this.FetchDbConn().BeginTransaction())
                {
                    this.RemoveDownload(epid);
                    this.AddDownload(epid);
                }
            }

            if (this.DownloadUpdated != null && this.DownloadIsVisible(epid))
            {
                this.DownloadUpdated(epid);
            }
        }

        private void Download_Removed(int epid)
        {
            if (this.DownloadRemoved != null && this.DownloadIsVisible(epid))
            {
                this.DownloadRemoved(epid);
            }

            this.RemoveDownload(epid);
        }

        private void AddDownload(int epid)
        {
            Model.Download downloadData = new Model.Download(epid);
            this.AddDownload(downloadData);
        }

        private void RemoveDownload(int epid)
        {
            lock (this.updateIndexLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("delete from downloads where docid = @epid", this.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@epid", epid));
                    command.ExecuteNonQuery();
                }
            }

            // No need to clear the visibility cache, as having an extra entry won't cause an issue
        }

        private void AddDownload(Model.Download storeData)
        {
            lock (this.updateIndexLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("insert or replace into downloads (docid, name, description) values (@epid, @name, @description)", this.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@epid", storeData.Epid));
                    command.Parameters.Add(new SQLiteParameter("@name", storeData.Name));
                    command.Parameters.Add(new SQLiteParameter("@description", storeData.Description));

                    command.ExecuteNonQuery();
                }
            }

            lock (this.downloadVisLock)
            {
                this.downloadsVisible = null;
            }
        }
    }
}
