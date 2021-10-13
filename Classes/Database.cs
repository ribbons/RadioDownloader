/*
 * Copyright © 2007-2019 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;
    using System.Data.SQLite;
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

                using (SQLiteCommand command = new SQLiteCommand("pragma foreign_keys = on", FetchDbConn()))
                {
                    command.ExecuteNonQuery();
                }
            }

            return dbConn;
        }

        protected internal static CompressedImage RetrieveImage(int imgid)
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
                    return new CompressedImage(content, false);
                }
            }
        }

        protected internal static int? StoreImage(CompressedImage image)
        {
            if (image == null)
            {
                return null;
            }

            byte[] imageAsBytes = image.GetBytes();

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
