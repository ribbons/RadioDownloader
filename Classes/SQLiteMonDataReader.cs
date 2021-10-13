/*
 * Copyright © 2010-2012 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Diagnostics;

    internal class SQLiteMonDataReader : IDisposable
    {
        private static Dictionary<SQLiteDataReader, string> readerInfo = new Dictionary<SQLiteDataReader, string>();
        private static object readerInfoLock = new object();

        private bool isDisposed;
        private SQLiteDataReader wrappedReader;

        public SQLiteMonDataReader(SQLiteDataReader reader)
        {
            this.wrappedReader = reader;

            StackTrace trace = new StackTrace(true);

            lock (readerInfoLock)
            {
                readerInfo.Add(this.wrappedReader, trace.ToString());
            }
        }

        ~SQLiteMonDataReader()
        {
            this.Dispose(false);
        }

        public static Exception AddReadersInfo(Exception exp)
        {
            string info = string.Empty;

            lock (readerInfoLock)
            {
                foreach (string entry in readerInfo.Values)
                {
                    info += entry + Environment.NewLine;
                }
            }

            if (!string.IsNullOrEmpty(info))
            {
                exp.Data.Add("Readers", info);
            }

            return exp;
        }

        public int GetOrdinal(string name)
        {
            return this.wrappedReader.GetOrdinal(name);
        }

        public bool GetBoolean(int i)
        {
            return this.wrappedReader.GetBoolean(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            return this.wrappedReader.GetBytes(i, fieldOffset, buffer, bufferOffset, length);
        }

        public DateTime GetDateTime(int i)
        {
            return this.wrappedReader.GetDateTime(i);
        }

        public int GetInt32(int i)
        {
            return this.wrappedReader.GetInt32(i);
        }

        public string GetString(int i)
        {
            return this.wrappedReader.GetString(i);
        }

        public object GetValue(int i)
        {
            return this.wrappedReader.GetValue(i);
        }

        public bool IsDBNull(int i)
        {
            return this.wrappedReader.IsDBNull(i);
        }

        public bool Read()
        {
            return this.wrappedReader.Read();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    lock (readerInfoLock)
                    {
                        readerInfo.Remove(this.wrappedReader);
                    }

                    this.wrappedReader.Dispose();
                }
            }

            this.isDisposed = true;
        }
    }
}
