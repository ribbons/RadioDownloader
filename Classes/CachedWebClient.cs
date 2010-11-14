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
    using System.Data.SQLite;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using Microsoft.VisualBasic;
    using Microsoft.VisualBasic.ApplicationServices;

    public class CachedWebClient
    {
        private static CachedWebClient instance = new CachedWebClient();

        [ThreadStatic()]
        private static SQLiteConnection dbConn;

        private CachedWebClient()
        {
            File.Delete(this.DatabasePath());

            // Create the the database and table for caching HTTP requests
            using (SQLiteCommand command = new SQLiteCommand("create table httpcache (uri varchar (1000) primary key, lastfetch datetime, success int, data blob)", this.FetchDbConn()))
            {
                command.ExecuteNonQuery();
            }
        }

        public static CachedWebClient GetInstance()
        {
            return instance;
        }

        public byte[] DownloadData(Uri uri, int fetchIntervalHrs)
        {
            if (fetchIntervalHrs == 0)
            {
                throw new ArgumentException("fetchIntervalHrs cannot be zero.", "fetchIntervalHrs");
            }

            System.DateTime? lastFetch = this.GetHTTPCacheLastUpdate(uri);

            if (lastFetch != null)
            {
                if (lastFetch.Value.AddHours(fetchIntervalHrs) > DateAndTime.Now)
                {
                    bool requestSuccess = false;
                    byte[] cacheData = this.GetHTTPCacheContent(uri, ref requestSuccess);

                    if (cacheData != null)
                    {
                        if (requestSuccess)
                        {
                            return cacheData;
                        }
                        else
                        {
                            using (MemoryStream memoryStream = new MemoryStream(cacheData))
                            {
                                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                                // Deserialise the CacheWebException structure
                                CacheWebExpInfo cachedException = default(CacheWebExpInfo);
                                cachedException = (CacheWebExpInfo)binaryFormatter.Deserialize(memoryStream);

                                // Crete a new WebException with the cached data and throw it
                                throw new WebException(cachedException.Message, cachedException.InnerException, cachedException.Status, cachedException.Response);
                            }
                        }
                    }
                }
            }

            Debug.Print("CachedWebClient: Fetching " + uri.ToString());

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.UserAgent = new ApplicationBase().Info.AssemblyName + " " + new ApplicationBase().Info.Version.ToString();
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");

            using (MemoryStream dataStream = new MemoryStream())
            {
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Stream responseStream = response.GetResponseStream();

                    if (response.ContentEncoding.ToUpperInvariant() == "GZIP")
                    {
                        // Decompress the gzipped response while it is being read into the memory stream
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                    }

                    // Read the response into a MemoryStream in chunks, ready for the byte array
                    byte[] buffer = new byte[10240];
                    int readBytes;

                    while ((readBytes = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        dataStream.Write(buffer, 0, readBytes);
                    }

                    if (response.ContentEncoding.ToUpperInvariant() == "GZIP")
                    {
                        responseStream.Dispose();
                    }
                }
                catch (WebException webExp)
                {
                    if (webExp.Status != WebExceptionStatus.NameResolutionFailure & webExp.Status != WebExceptionStatus.Timeout)
                    {
                        // A WebException doesn't serialise well, as Response and Status get lost,
                        // so store the information in a structure and then recreate it later
                        CacheWebExpInfo cacheException = new CacheWebExpInfo();
                        cacheException.Message = webExp.Message;
                        cacheException.InnerException = webExp.InnerException;
                        cacheException.Status = webExp.Status;
                        cacheException.Response = webExp.Response;

                        using (MemoryStream stream = new MemoryStream())
                        {
                            BinaryFormatter formatter = new BinaryFormatter();
                            
                            // Serialise the CacheWebException and store it in the cache
                            formatter.Serialize(stream, cacheException);
                            this.AddToHTTPCache(uri, false, stream.ToArray());
                        }
                    }

                    // Re-throw the WebException
                    throw;
                }

                byte[] data = dataStream.ToArray();
                this.AddToHTTPCache(uri, true, data);

                return data;
            }
        }

        public string DownloadString(Uri uri, int fetchIntervalHrs)
        {
            return Encoding.UTF8.GetString(this.DownloadData(uri, fetchIntervalHrs));
        }

        private string DatabasePath()
        {
            return Path.Combine(Path.GetTempPath(), "radiodld-httpcache.db");
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

        private void AddToHTTPCache(Uri uri, bool requestSuccess, byte[] data)
        {
            using (SQLiteCommand command = new SQLiteCommand("insert or replace into httpcache (uri, lastfetch, success, data) values(@uri, @lastfetch, @success, @data)", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@uri", uri.ToString()));
                command.Parameters.Add(new SQLiteParameter("@lastfetch", DateAndTime.Now));
                command.Parameters.Add(new SQLiteParameter("@success", requestSuccess));
                command.Parameters.Add(new SQLiteParameter("@data", data));
                command.ExecuteNonQuery();
            }
        }

        private System.DateTime? GetHTTPCacheLastUpdate(Uri uri)
        {
            using (SQLiteCommand command = new SQLiteCommand("select lastfetch from httpcache where uri=@uri", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@uri", uri.ToString()));

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetDateTime(reader.GetOrdinal("lastfetch"));
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        private byte[] GetHTTPCacheContent(Uri uri, ref bool requestSuccess)
        {
            using (SQLiteCommand command = new SQLiteCommand("select success, data from httpcache where uri=@uri", this.FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@uri", uri.ToString()));

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        requestSuccess = reader.GetBoolean(reader.GetOrdinal("success"));

                        // Get the length of the content by passing nothing to getbytes
                        int contentLength = Convert.ToInt32(reader.GetBytes(reader.GetOrdinal("data"), 0, null, 0, 0));

                        byte[] content = new byte[contentLength];
                        reader.GetBytes(reader.GetOrdinal("data"), 0, content, 0, contentLength);

                        return content;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        [Serializable()]
        public struct CacheWebExpInfo
        {
            public string Message;
            public Exception InnerException;
            public WebExceptionStatus Status;
            public WebResponse Response;
        }
    }
}
