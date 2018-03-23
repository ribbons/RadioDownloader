/*
 * This file is part of Radio Downloader.
 * Copyright © 2018 by the authors - see the AUTHORS file for details.
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

namespace RadioDld.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Drawing;

    internal class Chapter : Database
    {
        private const string Columns = "start, name, link, image";

        private int? imgid;

        public Chapter(Download download, int start)
        {
            using (SQLiteCommand command = new SQLiteCommand("select " + Columns + " from chapters where epid=@epid and start=@start", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@epid", download.Epid));
                command.Parameters.Add(new SQLiteParameter("@start", start));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (!reader.Read())
                    {
                        throw new DataNotFoundException(start, "Chapter does not exist");
                    }

                    if (reader.Read())
                    {
                        this.FetchData(reader);
                    }
                }
            }
        }

        private Chapter(SQLiteMonDataReader reader)
        {
            this.FetchData(reader);
        }

        public TimeSpan Start { get; private set; }

        public string Name { get; private set; }

        public Uri Link { get; private set; }

        public bool HasImage
        {
            get
            {
                return this.imgid != null;
            }
        }

        public Bitmap Image
        {
            get
            {
                if (this.imgid == null)
                {
                    return null;
                }

                return RetrieveImage(this.imgid.Value);
            }
        }

        public static void AddRange(int epid, ICollection<Provider.ChapterInfo> chapters)
        {
            lock (DbUpdateLock)
            {
                using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(FetchDbConn().BeginTransaction()))
                {
                    using (SQLiteCommand command = new SQLiteCommand("insert into chapters (epid, start, name, link, image) values (@epid, @start, @name, @link, @image)", FetchDbConn(), transMon.Trans))
                    {
                        foreach (var chapter in chapters)
                        {
                            command.Parameters.Add(new SQLiteParameter("@epid", epid));
                            command.Parameters.Add(new SQLiteParameter("@start", chapter.Start.TotalMilliseconds));
                            command.Parameters.Add(new SQLiteParameter("@name", chapter.Name));
                            command.Parameters.Add(new SQLiteParameter("@link", chapter.Link));
                            command.Parameters.Add(new SQLiteParameter("@image", StoreImage(chapter.Image)));
                            command.ExecuteNonQuery();
                        }
                    }

                    transMon.Trans.Commit();
                }
            }
        }

        public static ICollection<Chapter> FetchAll(Download download)
        {
            var results = new List<Chapter>();

            using (SQLiteCommand command = new SQLiteCommand("select " + Columns + " from chapters where epid=@epid", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@epid", download.Epid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    while (reader.Read())
                    {
                        results.Add(new Chapter(reader));
                    }
                }
            }

            return results;
        }

        private void FetchData(SQLiteMonDataReader reader)
        {
            int linkOrdinal = reader.GetOrdinal("link");
            int imageOrdinal = reader.GetOrdinal("image");

            this.Start = TimeSpan.FromMilliseconds(reader.GetInt32(reader.GetOrdinal("start")));
            this.Name = reader.GetString(reader.GetOrdinal("name"));

            if (!reader.IsDBNull(linkOrdinal))
            {
                this.Link = new Uri(reader.GetString(linkOrdinal));
            }

            if (!reader.IsDBNull(imageOrdinal))
            {
                this.imgid = reader.GetInt32(imageOrdinal);
            }
        }
    }
}
