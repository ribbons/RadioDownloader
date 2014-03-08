/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2012 by the authors - see the AUTHORS file for details.
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
    using System.Data.SQLite;
    using System.Drawing;
    using System.IO;

    public abstract class Database
    {
        protected internal const int CurrentDbVersion = 5;

        [ThreadStatic]
        private static SQLiteConnection dbConn;

        private static object dbUpdateLock = new object();

        protected internal static object DbUpdateLock
        {
            get
            {
                return dbUpdateLock;
            }
        }

        protected internal static SQLiteConnection FetchDbConn()
        {
            if (dbConn == null)
            {
                dbConn = new SQLiteConnection("Data Source=" + Path.Combine(FileUtils.GetAppDataFolder(), "store.db") + ";Version=3;New=False");
                dbConn.Open();
            }

            return dbConn;
        }

        protected internal static Bitmap RetrieveImage(int imgid)
        {
            using (SQLiteCommand command = new SQLiteCommand("select image from images where imgid=@imgid", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@imgid", imgid));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    // Get the size of the image data by passing nothing to getbytes
                    int dataLength = (int)reader.GetBytes(reader.GetOrdinal("image"), 0, null, 0, 0);
                    byte[] content = new byte[dataLength];

                    reader.GetBytes(reader.GetOrdinal("image"), 0, content, 0, dataLength);

                    using (MemoryStream contentStream = new MemoryStream(content))
                    {
                        using (Bitmap streamBitmap = new Bitmap(contentStream))
                        {
                            return new Bitmap(streamBitmap);
                        }
                    }
                }
            }
        }

        protected internal static int? StoreImage(Image image)
        {
            if (image == null)
            {
                return null;
            }

            // Convert the image into a byte array
            byte[] imageAsBytes = null;

            using (MemoryStream memstream = new MemoryStream())
            {
                image.Save(memstream, System.Drawing.Imaging.ImageFormat.Png);
                imageAsBytes = memstream.ToArray();
            }

            lock (DbUpdateLock)
            {
                using (SQLiteCommand command = new SQLiteCommand("select imgid from images where image=@image", FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@image", imageAsBytes));

                    using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt32(reader.GetOrdinal("imgid"));
                        }
                    }
                }

                using (SQLiteCommand command = new SQLiteCommand("insert into images (image) values (@image)", FetchDbConn()))
                {
                    command.Parameters.Add(new SQLiteParameter("@image", imageAsBytes));
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand command = new SQLiteCommand("select last_insert_rowid()", FetchDbConn()))
                {
                    return (int)(long)command.ExecuteScalar();
                }
            }
        }
    }
}
