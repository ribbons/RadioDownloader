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
    using System.Data;
    using System.Data.SQLite;

    internal class UpdateDB : IDisposable
    {
        private bool isDisposed;

        private SQLiteConnection specConn;
        private SQLiteConnection updateConn;

        public UpdateDB(string specDbPath, string updateDbPath)
            : base()
        {
            this.specConn = new SQLiteConnection("Data Source=" + specDbPath + ";Version=3;New=False;Read Only=True");
            this.specConn.Open();

            this.updateConn = new SQLiteConnection("Data Source=" + updateDbPath + ";Version=3;New=False");
            this.updateConn.Open();
        }

        ~UpdateDB()
        {
            this.Dispose(false);
        }

        private enum UpdateType
        {
            None,
            Create,
            Update
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool UpdateStructure()
        {
            UpdateType updateReqd = default(UpdateType);

            using (SQLiteCommand specCommand = new SQLiteCommand("select name, sql from sqlite_master where type='table'", this.specConn))
            {
                using (SQLiteCommand checkUpdateCmd = new SQLiteCommand("select sql from sqlite_master where type='table' and name=@name", this.updateConn))
                {
                    SQLiteParameter nameParam = new SQLiteParameter("@name");
                    checkUpdateCmd.Parameters.Add(nameParam);

                    using (SQLiteDataReader specReader = specCommand.ExecuteReader())
                    {
                        int nameOrd = specReader.GetOrdinal("name");
                        int sqlOrd = specReader.GetOrdinal("sql");

                        while (specReader.Read())
                        {
                            string specName = specReader.GetString(nameOrd);
                            string specSql = specReader.GetString(sqlOrd);

                            nameParam.Value = specName;

                            using (SQLiteDataReader checkUpdateRdr = checkUpdateCmd.ExecuteReader())
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
                                using (SQLiteCommand updateCommand = new SQLiteCommand(specSql, this.updateConn))
                                {
                                    updateCommand.ExecuteNonQuery();
                                }
                            }
                            else if (updateReqd == UpdateType.Update)
                            {
                                // Fetch a list of common column names for transferring the data
                                string columnNames = this.ColNames(specName);

                                // Start a transaction, so we can roll back if there is an error
                                using (SQLiteTransaction trans = this.updateConn.BeginTransaction())
                                {
                                    try
                                    {
                                        // Rename the existing table to table_name_old
                                        using (SQLiteCommand updateCommand = new SQLiteCommand("alter table [" + specName + "] rename to [" + specName + "_old]", this.updateConn, trans))
                                        {
                                            updateCommand.ExecuteNonQuery();
                                        }

                                        // Create the new table with the correct structure
                                        using (SQLiteCommand updateCommand = new SQLiteCommand(specSql, this.updateConn, trans))
                                        {
                                            updateCommand.ExecuteNonQuery();
                                        }

                                        // Copy across the data (discarding rows which violate any new constraints)
                                        if (!string.IsNullOrEmpty(columnNames))
                                        {
                                            using (SQLiteCommand updateCommand = new SQLiteCommand("insert or ignore into [" + specName + "] (" + columnNames + ") select " + columnNames + " from [" + specName + "_old]", this.updateConn, trans))
                                            {
                                                updateCommand.ExecuteNonQuery();
                                            }
                                        }

                                        // Delete the old table
                                        using (SQLiteCommand updateCommand = new SQLiteCommand("drop table [" + specName + "_old]", this.updateConn, trans))
                                        {
                                            updateCommand.ExecuteNonQuery();
                                        }
                                    }
                                    catch (SQLiteException)
                                    {
                                        trans.Rollback();
                                        throw;
                                    }

                                    trans.Commit();
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        private string ColNames(string tableName)
        {
            List<string> fromCols = this.ListTableColumns(this.updateConn, tableName);
            List<string> toCols = this.ListTableColumns(this.specConn, tableName);
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

        private List<string> ListTableColumns(SQLiteConnection connection, string tableName)
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

        private void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.specConn.Dispose();
                    this.updateConn.Dispose();
                }

                this.isDisposed = true;
            }
        }
    }
}
