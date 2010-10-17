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
namespace RadioDld
{

	public class SQLiteMonDataReader : IDisposable
	{

		private static Dictionary<SQLiteDataReader, string> readerInfo = new Dictionary<SQLiteDataReader, string>();

		private static object readerInfoLock = new object();
		private bool isDisposed;

		private SQLiteDataReader wrappedReader;
		public SQLiteMonDataReader(SQLiteDataReader reader)
		{
			wrappedReader = reader;

			StackTrace trace = new StackTrace(true);

			lock (readerInfoLock) {
				readerInfo.Add(wrappedReader, trace.ToString());
			}
		}

		public static Exception AddReadersInfo(Exception exp)
		{
			string info = string.Empty;

			lock (readerInfoLock) {
				foreach (string entry in readerInfo.Values) {
					info += entry + Environment.NewLine;
				}
			}

			if (!string.IsNullOrEmpty(info)) {
				exp.Data.Add("readers", info);
			}

			return exp;
		}

		public int GetOrdinal(string name)
		{
			return wrappedReader.GetOrdinal(name);
		}

		public bool GetBoolean(int i)
		{
			return wrappedReader.GetBoolean(i);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
		{
			return wrappedReader.GetBytes(i, fieldOffset, buffer, bufferOffset, length);
		}

		public System.DateTime GetDateTime(int i)
		{
			return wrappedReader.GetDateTime(i);
		}

		public int GetInt32(int i)
		{
			return wrappedReader.GetInt32(i);
		}

		public string GetString(int i)
		{
			return wrappedReader.GetString(i);
		}

		public object GetValue(int i)
		{
			return wrappedReader.GetValue(i);
		}

		public bool IsDBNull(int i)
		{
			return wrappedReader.IsDBNull(i);
		}

		public bool Read()
		{
			return wrappedReader.Read();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.isDisposed) {
				if (disposing) {
					lock (readerInfoLock) {
						readerInfo.Remove(wrappedReader);
					}

					wrappedReader.Dispose();
				}
			}

			this.isDisposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

        ~SQLiteMonDataReader()
		{
			Dispose(false);
		}
	}
}
