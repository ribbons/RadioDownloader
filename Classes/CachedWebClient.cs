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

using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.ApplicationServices;

namespace RadioDld
{
    public class CachedWebClient
    {
        [Serializable()]
        public struct CacheWebExpInfo
        {
            public string Message;
            public Exception InnerException;
            public WebExceptionStatus Status;
            public WebResponse Response;
        }

        private static CachedWebClient instance = new CachedWebClient();

        [ThreadStatic()]
        private static SQLiteConnection dbConn;

        public static CachedWebClient GetInstance()
        {
            return instance;
        }

        private string DatabasePath()
        {
            return Path.Combine(Path.GetTempPath(), "radiodld-httpcache.db");
        }

        private SQLiteConnection FetchDbConn()
        {
            if (dbConn == null)
            {
                dbConn = new SQLiteConnection("Data Source=" + DatabasePath() + ";Version=3");
                dbConn.Open();
            }

            return dbConn;
        }

        private CachedWebClient()
        {
            File.Delete(DatabasePath());

            // Create the the database and table for caching HTTP requests
            using (SQLiteCommand command = new SQLiteCommand("create table httpcache (uri varchar (1000) primary key, lastfetch datetime, success int, data blob)", FetchDbConn()))
            {
                command.ExecuteNonQuery();
            }
        }

        private void AddToHTTPCache(Uri uri, bool requestSuccess, byte[] data)
        {
            using (SQLiteCommand command = new SQLiteCommand("insert or replace into httpcache (uri, lastfetch, success, data) values(@uri, @lastfetch, @success, @data)", FetchDbConn()))
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
            using (SQLiteCommand command = new SQLiteCommand("select lastfetch from httpcache where uri=@uri", FetchDbConn()))
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
            using (SQLiteCommand command = new SQLiteCommand("select success, data from httpcache where uri=@uri", FetchDbConn()))
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

        public byte[] DownloadData(Uri uri, int fetchIntervalHrs)
        {
            if (fetchIntervalHrs == 0)
            {
                throw new ArgumentException("fetchIntervalHrs cannot be zero.", "fetchIntervalHrs");
            }

            System.DateTime? lastFetch = GetHTTPCacheLastUpdate(uri);

            if (lastFetch != null)
            {
                if (lastFetch.Value.AddHours(fetchIntervalHrs) > DateAndTime.Now)
                {
                    bool requestSuccess = false;
                    byte[] cacheData = GetHTTPCacheContent(uri, ref requestSuccess);

                    if (cacheData != null)
                    {
                        if (requestSuccess)
                        {
                            return cacheData;
                        }
                        else
                        {
                            MemoryStream memoryStream = new MemoryStream(cacheData);
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

            Debug.Print("Cached WebClient: Fetching " + uri.ToString());
            WebClient webClient = new WebClient();
            webClient.Headers.Add("user-agent", new ApplicationBase().Info.AssemblyName + " " + new ApplicationBase().Info.Version.ToString());

            byte[] data = null;

            try
            {
                data = webClient.DownloadData(uri);
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

                    MemoryStream stream = new MemoryStream();
                    BinaryFormatter formatter = new BinaryFormatter();

                    // Serialise the CacheWebException and store it in the cache
                    formatter.Serialize(stream, cacheException);
                    AddToHTTPCache(uri, false, stream.ToArray());
                }

                // Re-throw the WebException
                throw;
            }

            AddToHTTPCache(uri, true, data);

            return data;
        }

        public string DownloadString(Uri uri, int fetchIntervalHrs)
        {
            return Encoding.UTF8.GetString(DownloadData(uri, fetchIntervalHrs));
        }
    }
}
