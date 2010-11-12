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
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Globalization;
    using System.IO;
    using Microsoft.VisualBasic;

    internal class DataSearch
    {
        [ThreadStatic()]
        private static SQLiteConnection dbConn;
        private static DataSearch searchInstance;

        private static object searchInstanceLock = new object();

        private Data dataInstance;
        private string _downloadQuery = string.Empty;

        private List<int> downloadsVisible;
        private object updateIndexLock = new object();
        private object downloadVisLock = new object();

        private DataSearch(Data instance)
        {
            this.dataInstance = instance;

            Dictionary<string, string[]> tableCols = new Dictionary<string, string[]>();
            tableCols.Add("downloads", new string[] { "name", "description" });

            using (Status showStatus = new Status())
            {
                if (this.CheckIndex(tableCols) == false)
                {
                    // Close & clean up the connection used for testing
                    dbConn.Close();
                    dbConn.Dispose();
                    dbConn = null;

                    // Clean up the old index
                    File.Delete(this.DatabasePath());

                    showStatus.StatusText = "Building search index...";
                    showStatus.ProgressBarMarquee = false;
                    showStatus.ProgressBarValue = 0;
                    showStatus.ProgressBarMax = 100 * tableCols.Count;
                    showStatus.Show();

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

                            showStatus.StatusText = "Building search index for downloads...";

                            int progress = 1;
                            List<Data.DownloadData> downloadItems = this.dataInstance.FetchDownloadList(false);

                            foreach (Data.DownloadData downloadItem in downloadItems)
                            {
                                this.AddDownload(downloadItem);

                                showStatus.ProgressBarValue = Convert.ToInt32((progress / downloadItems.Count) * 100);
                                progress += 1;
                            }

                            showStatus.ProgressBarValue = 100;

                            trans.Commit();
                        }
                    }

                    showStatus.Hide();
                }
            }
        }

        public string DownloadQuery
        {
            get
            {
                return this._downloadQuery;
            }

            set
            {
                if (value != this._downloadQuery)
                {
                    try
                    {
                        this.RunQuery(value);
                        this._downloadQuery = value;
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
            if (string.IsNullOrEmpty(this._downloadQuery))
            {
                return true;
            }

            lock (this.downloadVisLock)
            {
                if (this.downloadsVisible == null)
                {
                    this.RunQuery(this._downloadQuery);
                }

                return this.downloadsVisible.Contains(epid);
            }
        }

        public void AddDownload(int epid)
        {
            Data.DownloadData downloadData = this.dataInstance.FetchDownloadData(epid);
            this.AddDownload(downloadData);
        }

        public void UpdateDownload(int epid)
        {
            lock (this.updateIndexLock)
            {
                using (SQLiteTransaction trans = this.FetchDbConn().BeginTransaction())
                {
                    this.RemoveDownload(epid);
                    this.AddDownload(epid);
                }
            }
        }

        public void RemoveDownload(int epid)
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

        private void AddDownload(Data.DownloadData storeData)
        {
            lock (this.updateIndexLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("insert or replace into downloads (docid, name, description) values (@epid, @name, @description)", this.FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@epid", storeData.epid));
                    command.Parameters.Add(new SQLiteParameter("@name", storeData.name));
                    command.Parameters.Add(new SQLiteParameter("@description", storeData.description));

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
