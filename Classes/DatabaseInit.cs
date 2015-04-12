/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2015 by the authors - see the AUTHORS file for details.
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
    using System.Data;
    using System.Data.SQLite;
    using System.IO;
    using System.Windows.Forms;

    internal class DatabaseInit : Database
    {
        private enum UpdateType
        {
            None,
            Create,
            Update
        }

        public static bool Startup()
        {
            const string DbFileName = "store.db";
            string specDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DbFileName);
            string appDbPath = Path.Combine(FileUtils.GetAppDataFolder(), DbFileName);

            // Ensure that the template database exists
            if (!File.Exists(specDbPath))
            {
                MessageBox.Show("The Radio Downloader template database was not found at '" + specDbPath + "'." + Environment.NewLine + Environment.NewLine + "Try repairing the Radio Downloader installation or installing the latest version from nerdoftheherd.com", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }

            using (SQLiteConnection specConn = new SQLiteConnection("Data Source=" + specDbPath + ";Version=3;New=False;Read Only=True"))
            {
                specConn.Open();

                using (SQLiteCommand command = new SQLiteCommand("pragma integrity_check(1)", specConn))
                {
                    string result = (string)command.ExecuteScalar();

                    if (result.ToUpperInvariant() != "OK")
                    {
                        MessageBox.Show("The Radio Downloader template database at '" + specDbPath + "' appears to be corrupted." + Environment.NewLine + Environment.NewLine + "Try repairing the Radio Downloader installation or installing the latest version from nerdoftheherd.com", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return false;
                    }
                }

                // Migrate old (pre 0.26) version databases from www.nerdoftheherd.com -> NerdoftheHerd.com
                string oldDbPath = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "www.nerdoftheherd.com"), Application.ProductName), DbFileName);

                if (File.Exists(oldDbPath) && !File.Exists(appDbPath))
                {
                    File.Move(oldDbPath, appDbPath);
                }

                // Test if there is an existing application database
                if (!File.Exists(appDbPath))
                {
                    // Start with a copy of the template database
                    File.Copy(specDbPath, appDbPath);

                    // Set the current database version in the new database
                    Settings.DatabaseVersion = Database.CurrentDbVersion;
                }
                else
                {
                    using (SQLiteCommand command = new SQLiteCommand("pragma integrity_check(1)", FetchDbConn()))
                    {
                        string result = (string)command.ExecuteScalar();

                        if (result.ToUpperInvariant() != "OK")
                        {
                            if (MessageBox.Show("Unfortunately Radio Downloader cannot start because your database has become corrupted." + Environment.NewLine + Environment.NewLine + "Would you like to view some help about resolving this issue?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Stop) == DialogResult.Yes)
                            {
                                OsUtils.LaunchUrl(new Uri("https://nerdoftheherd.com/tools/radiodld/help/corrupt-database"), "corruptdb");
                            }

                            return false;
                        }
                    }

                    // Start a transaction so we can roll back a half-completed upgrade on error
                    using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(FetchDbConn().BeginTransaction()))
                    {
                        try
                        {
                            // Perform a check and automatic update of the database table structure
                            UpdateStructure(specConn, Database.FetchDbConn());

                            // Perform any updates required which were not handled by UpdateStructure
                            switch (Settings.DatabaseVersion)
                            {
                                case 4:
                                    // Clear error details previously serialised as XML
                                    using (SQLiteCommand command = new SQLiteCommand("update downloads set errordetails=null where errortype=@errortype", FetchDbConn()))
                                    {
                                        command.Parameters.Add(new SQLiteParameter("errortype", ErrorType.UnknownError));
                                        command.ExecuteNonQuery();
                                    }

                                    break;
                                case Database.CurrentDbVersion:
                                    // Nothing to do, this is the current version.
                                    break;
                            }

                            // Set the current database version
                            Settings.DatabaseVersion = Database.CurrentDbVersion;
                        }
                        catch (SQLiteException)
                        {
                            transMon.Trans.Rollback();
                            throw;
                        }

                        transMon.Trans.Commit();
                    }
                }
            }

            // Prune the database once a week
            if (Settings.LastPrune.AddDays(7) < DateTime.Now)
            {
                using (Status status = new Status())
                {
                    status.ShowDialog(delegate
                    {
                        Prune(status);
                    });
                }
            }

            // Vacuum the database every three months
            if (Settings.LastVacuum.AddMonths(3) < DateTime.Now)
            {
                using (Status status = new Status())
                {
                    status.ShowDialog(delegate
                    {
                        Vacuum(status);
                    });
                }
            }

            return true;
        }

        private static void UpdateStructure(SQLiteConnection specConn, SQLiteConnection updateConn)
        {
            using (SQLiteCommand specCommand = new SQLiteCommand("select name, sql from sqlite_master where type='table'", specConn))
            {
                using (SQLiteCommand checkUpdateCmd = new SQLiteCommand("select sql from sqlite_master where type='table' and name=@name", updateConn))
                {
                    SQLiteParameter nameParam = new SQLiteParameter("@name");
                    checkUpdateCmd.Parameters.Add(nameParam);

                    using (SQLiteMonDataReader specReader = new SQLiteMonDataReader(specCommand.ExecuteReader()))
                    {
                        int nameOrd = specReader.GetOrdinal("name");
                        int sqlOrd = specReader.GetOrdinal("sql");

                        while (specReader.Read())
                        {
                            string specName = specReader.GetString(nameOrd);
                            string specSql = specReader.GetString(sqlOrd);

                            nameParam.Value = specName;
                            UpdateType updateReqd;

                            using (SQLiteMonDataReader checkUpdateRdr = new SQLiteMonDataReader(checkUpdateCmd.ExecuteReader()))
                            {
                                if (!checkUpdateRdr.Read())
                                {
                                    // The table doesn't exist
                                    updateReqd = UpdateType.Create;
                                }
                                else
                                {
                                    if (specSql == checkUpdateRdr.GetString(checkUpdateRdr.GetOrdinal("sql")))
                                    {
                                        // The table does not require an update
                                        updateReqd = UpdateType.None;
                                    }
                                    else
                                    {
                                        // The structure of the table doesn't match, so update it
                                        updateReqd = UpdateType.Update;
                                    }
                                }
                            }

                            if (updateReqd == UpdateType.Create)
                            {
                                // Create the table
                                using (SQLiteCommand updateCommand = new SQLiteCommand(specSql, updateConn))
                                {
                                    updateCommand.ExecuteNonQuery();
                                }
                            }
                            else if (updateReqd == UpdateType.Update)
                            {
                                // Fetch a list of common column names for transferring the data
                                string columnNames = ColNames(specConn, updateConn, specName);

                                // Rename the existing table to table_name_old
                                using (SQLiteCommand updateCommand = new SQLiteCommand("alter table [" + specName + "] rename to [" + specName + "_old]", updateConn))
                                {
                                    updateCommand.ExecuteNonQuery();
                                }

                                // Create the new table with the correct structure
                                using (SQLiteCommand updateCommand = new SQLiteCommand(specSql, updateConn))
                                {
                                    updateCommand.ExecuteNonQuery();
                                }

                                // Copy across the data (discarding rows which violate any new constraints)
                                if (!string.IsNullOrEmpty(columnNames))
                                {
                                    using (SQLiteCommand updateCommand = new SQLiteCommand("insert or ignore into [" + specName + "] (" + columnNames + ") select " + columnNames + " from [" + specName + "_old]", updateConn))
                                    {
                                        updateCommand.ExecuteNonQuery();
                                    }
                                }

                                // Delete the old table
                                using (SQLiteCommand updateCommand = new SQLiteCommand("drop table [" + specName + "_old]", updateConn))
                                {
                                    updateCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
        }

        private static string ColNames(SQLiteConnection specConn, SQLiteConnection updateConn, string tableName)
        {
            List<string> fromCols = ListTableColumns(updateConn, tableName);
            List<string> toCols = ListTableColumns(specConn, tableName);
            List<string> resultCols = new List<string>();

            // Store an intersect of the from and to columns in resultCols
            foreach (string fromCol in fromCols)
            {
                if (toCols.Contains(fromCol))
                {
                    resultCols.Add(fromCol);
                }
            }

            if (resultCols.Count > 0)
            {
                return "[" + string.Join("], [", resultCols.ToArray()) + "]";
            }
            else
            {
                return string.Empty;
            }
        }

        private static List<string> ListTableColumns(SQLiteConnection connection, string tableName)
        {
            List<string> returnList = new List<string>();

            string[] restrictionValues =
            {
                null,
                null,
                tableName,
                null
            };
            DataTable columns = connection.GetSchema(System.Data.SQLite.SQLiteMetaDataCollectionNames.Columns, restrictionValues);

            foreach (DataRow columnRow in columns.Rows)
            {
                returnList.Add(columnRow["COLUMN_NAME"].ToString());
            }

            return returnList;
        }

        private static void Vacuum(Status status)
        {
            status.StatusText = "Compacting database.  This may take several minutes...";

            // Make SQLite recreate the database to reduce the size on disk and remove fragmentation
            lock (Database.DbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("vacuum", FetchDbConn()))
                {
                    command.ExecuteNonQuery();
                }

                Settings.LastVacuum = DateTime.Now;
            }
        }

        private static void Prune(Status status)
        {
            lock (Database.DbUpdateLock)
            {
                using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(FetchDbConn().BeginTransaction()))
                {
                    status.StatusText = "Pruning episode images...";

                    // Set image to null for unavailable episodes, except:
                    // - episodes in the downloads list
                    // - the most recent episode for subscriptions or favourites (as this may be used as the programme image)
                    using (SQLiteCommand command = new SQLiteCommand("update episodes set image=null where epid in (select e1.epid from episodes as e1 left outer join downloads on e1.epid=downloads.epid where available=0 and image is not null and downloads.epid is null and ((not (exists(select 1 from subscriptions where subscriptions.progid=e1.progid) or exists(select 1 from favourites where favourites.progid=e1.progid))) or exists(select 1 from episodes as e2 where e1.progid=e2.progid and image is not null and e2.date > e1.date)))", FetchDbConn()))
                    {
                        command.ExecuteNonQuery();
                    }

                    status.StatusText = "Cleaning up unused images...";

                    // Remove images which are now no-longer referenced by a programme or episode
                    using (SQLiteCommand command = new SQLiteCommand("delete from images where imgid in (select imgid from images left outer join programmes on imgid=programmes.image left outer join episodes on imgid=episodes.image where programmes.image is null and episodes.image is null)", FetchDbConn()))
                    {
                        command.ExecuteNonQuery();
                    }

                    transMon.Trans.Commit();
                }

                Settings.LastPrune = DateTime.Now;
            }
        }
    }
}
