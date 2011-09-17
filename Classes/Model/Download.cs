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
    using System.Data.SQLite;

    internal class Download : Episode
    {
        public Download(SQLiteMonDataReader reader)
            : base(reader)
        {
            this.FetchData(reader);
        }

        public Download(int epid)
            : base(epid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select status, errortype, errordetails, filepath, playcount from downloads where downloads.epid=@epid", Data.FetchDbConn()))
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

        private void FetchData(SQLiteMonDataReader reader)
        {
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
