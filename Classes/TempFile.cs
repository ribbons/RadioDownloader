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

namespace RadioDld
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;

    /// <summary>
    /// Handle generating temporary file names and guarantee that any files with those names will be cleaned up.
    /// </summary>
    public sealed class TempFile : TempFileBase, IDisposable
    {
        private static List<string> notInUse = new List<string>();

        private bool isDisposed;

        /// <summary>
        /// Initializes static members of the <see cref="TempFile"/> class.
        /// Any file names still listed in the database are attempted to be deleted.
        /// </summary>
        static TempFile()
        {
            lock (notInUse)
            {
                using (SQLiteCommand command = new SQLiteCommand("select filepath from tempfiles", FetchDbConn()))
                {
                    using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                    {
                        int filePathOrdinal = reader.GetOrdinal("filepath");

                        while (reader.Read())
                        {
                            notInUse.Add(reader.GetString(filePathOrdinal));
                        }
                    }
                }
            }

            DeleteFiles();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TempFile"/> class and generates a temporary
        /// file name with a random extension.
        /// </summary>
        public TempFile()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TempFile"/> class and generates a temporary
        /// file name with the specified extension.
        /// </summary>
        /// <param name="fileExtension">The extension of the file, not including a leading dot.</param>
        public TempFile(string fileExtension)
        {
            DeleteFiles();

            lock (DbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("insert into tempfiles (filepath) values (@filepath)", FetchDbConn()))
                {
                    SQLiteParameter filepathParam = new SQLiteParameter("@filepath");
                    command.Parameters.Add(filepathParam);

                    do
                    {
                        string testFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                        if (!string.IsNullOrEmpty(fileExtension))
                        {
                            // Replace the random extension with the one requested
                            testFilePath = testFilePath.Substring(0, testFilePath.Length - 3) + fileExtension;
                        }

                        filepathParam.Value = testFilePath;

                        if (File.Exists(testFilePath))
                        {
                            // Generated name already exists - fetch another
                            continue;
                        }

                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (SQLiteException sqliteExp)
                        {
                            if (sqliteExp.ErrorCode == SQLiteErrorCode.Constraint)
                            {
                                // Generated name is already in the database - fetch another
                                continue;
                            }

                            throw;
                        }

                        this.FilePath = testFilePath;
                    }
                    while (this.FilePath == null);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.isDisposed && this.FilePath != null)
            {
                lock (notInUse)
                {
                    notInUse.Add(this.FilePath);
                }

                DeleteFiles();
            }

            this.isDisposed = true;
        }

        private static void DeleteFiles()
        {
            lock (notInUse)
            {
                using (SQLiteCommand command = new SQLiteCommand("delete from tempfiles where filepath=@filepath", FetchDbConn()))
                {
                    SQLiteParameter filepathParam = new SQLiteParameter("@filepath");
                    command.Parameters.Add(filepathParam);

                    foreach (string filePath in notInUse)
                    {
                        try
                        {
                            File.Delete(filePath);
                        }
                        catch (IOException)
                        {
                            // File is still in use - try again later
                            continue;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Do not have the correct permissions to delete - try again later
                            continue;
                        }

                        lock (DbUpdateLock)
                        {
                            filepathParam.Value = filePath;
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
