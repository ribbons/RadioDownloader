using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
// Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
// Copyright © 2007-2010 Matt Robinson
//
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General
// Public License as published by the Free Software Foundation; either version 2 of the License, or (at your
// option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
// implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
// License for more details.
//
// You should have received a copy of the GNU General Public License along with this program; if not, write
// to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.


using System.Collections.Generic;
using System.Data.SQLite;
using System.Text.RegularExpressions;
namespace RadioDld
{

	internal class UpdateDB : IDisposable
	{


		private bool isDisposed;
		private SQLiteConnection specConn;

		private SQLiteConnection updateConn;
		private enum UpdateType
		{
			None,
			Create,
			Update
		}

		public UpdateDB(string specDbPath, string updateDbPath) : base()
		{

			specConn = new SQLiteConnection("Data Source=" + specDbPath + ";Version=3;New=False");
			specConn.Open();

			updateConn = new SQLiteConnection("Data Source=" + updateDbPath + ";Version=3;New=False");
			updateConn.Open();
		}

		public bool UpdateStructure()
		{
			UpdateType updateReqd = default(UpdateType);

			using (SQLiteCommand specCommand = new SQLiteCommand("select name, sql from sqlite_master where type='table'", specConn)) {
				using (SQLiteCommand checkUpdateCmd = new SQLiteCommand("select sql from sqlite_master where type='table' and name=@name", updateConn)) {

					SQLiteParameter nameParam = new SQLiteParameter("@name");
					checkUpdateCmd.Parameters.Add(nameParam);

					using (SQLiteDataReader specReader = specCommand.ExecuteReader()) {
						int nameOrd = specReader.GetOrdinal("name");
						int sqlOrd = specReader.GetOrdinal("sql");

						while (specReader.Read()) {
							string specName = specReader.GetString(nameOrd);
							string specSql = specReader.GetString(sqlOrd);

							nameParam.Value = specName;

							using (SQLiteDataReader checkUpdateRdr = checkUpdateCmd.ExecuteReader()) {
								if (checkUpdateRdr.Read() == false) {
									// The table doesn't exist
									updateReqd = UpdateType.Create;
								} else {
									if (specSql == checkUpdateRdr.GetString(checkUpdateRdr.GetOrdinal("sql"))) {
										// The table does not require an update
										updateReqd = UpdateType.None;
									} else {
										// The structure of the table doesn't match, so update it
										updateReqd = UpdateType.Update;
									}
								}
							}

							if (updateReqd == UpdateType.Create) {
								// Create the table
								using (SQLiteCommand updateCommand = new SQLiteCommand(specSql, updateConn)) {
									updateCommand.ExecuteNonQuery();
								}
							} else if (updateReqd == UpdateType.Update) {
								// Fetch a list of common column names for transferring the data
								string columnNames = ColNames(specName);

								// Start a transaction, so we can roll back if there is an error
								using (SQLiteTransaction trans = updateConn.BeginTransaction()) {
									try {
										// Rename the existing table to table_name_old
										using (SQLiteCommand updateCommand = new SQLiteCommand("alter table [" + specName + "] rename to [" + specName + "_old]", updateConn, trans)) {
											updateCommand.ExecuteNonQuery();
										}

										// Create the new table with the correct structure
										using (SQLiteCommand updateCommand = new SQLiteCommand(specSql, updateConn, trans)) {
											updateCommand.ExecuteNonQuery();
										}

										// Copy across the data (discarding rows which violate any new constraints)
										if (!string.IsNullOrEmpty(columnNames)) {
											using (SQLiteCommand updateCommand = new SQLiteCommand("insert or ignore into [" + specName + "] (" + columnNames + ") select " + columnNames + " from [" + specName + "_old]", updateConn, trans)) {
												updateCommand.ExecuteNonQuery();
											}
										}

										// Delete the old table
										using (SQLiteCommand updateCommand = new SQLiteCommand("drop table [" + specName + "_old]", updateConn, trans)) {
											updateCommand.ExecuteNonQuery();
										}
									} catch (SQLiteException) {
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
			List<string> fromCols = ListTableColumns(updateConn, tableName);
			List<string> toCols = ListTableColumns(specConn, tableName);
			List<string> resultCols = new List<string>();

			// Store an intersect of the from and to columns in resultCols
			foreach (string fromCol in fromCols) {
				if (toCols.Contains(fromCol)) {
					resultCols.Add(fromCol);
				}
			}

			if (resultCols.Count > 0) {
				return "[" + Strings.Join(resultCols.ToArray(), "], [") + "]";
			} else {
				return "";
			}
		}

		private List<string> ListTableColumns(SQLiteConnection connection, string tableName)
		{
			List<string> returnList = new List<string>();

			string[] restrictionValues = {
				null,
				null,
				tableName,
				null
			};
			DataTable columns = connection.GetSchema(System.Data.SQLite.SQLiteMetaDataCollectionNames.Columns, restrictionValues);

			foreach (DataRow columnRow in columns.Rows) {
				returnList.Add(columnRow["COLUMN_NAME"].ToString());
			}

			return returnList;
		}

		private void Dispose(bool disposing)
		{
			if (!this.isDisposed) {
				if (disposing) {
					specConn.Dispose();
					updateConn.Dispose();
				}

				this.isDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

        ~UpdateDB()
		{
			Dispose(false);
		}
	}
}
