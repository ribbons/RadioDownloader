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

    internal class SQLiteMonTransaction : IDisposable
    {
        private static Dictionary<SQLiteTransaction, string> readerInfo = new Dictionary<SQLiteTransaction, string>();
        private static object readerInfoLock = new object();

        private bool isDisposed;
        private SQLiteTransaction wrappedTrans;

        public SQLiteMonTransaction(SQLiteTransaction transaction)
        {
            this.wrappedTrans = transaction;

            StackTrace trace = new StackTrace(true);

            lock (readerInfoLock)
            {
                readerInfo.Add(this.wrappedTrans, trace.ToString());
            }
        }

        ~SQLiteMonTransaction()
        {
            this.Dispose(false);
        }

        public SQLiteTransaction Trans
        {
            get { return this.wrappedTrans; }
        }

        public static Exception AddTransactionsInfo(Exception exp)
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
                exp.Data.Add("Transactions", info);
            }

            return exp;
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
                        readerInfo.Remove(this.wrappedTrans);
                    }

                    this.wrappedTrans.Dispose();
                }
            }

            this.isDisposed = true;
        }
    }
}
