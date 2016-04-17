/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2016 by the authors - see the AUTHORS file for details.
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
    using System.Data.SQLite;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    public class Settings : Database
    {
        internal static DateTime LastUpdatePrompt
        {
            get
            {
                return GetValue("LastUpdatePrompt", DateTime.MinValue);
            }

            set
            {
                SetValue("LastUpdatePrompt", value);
            }
        }

        internal static DateTime LastCheckForUpdates
        {
            get
            {
                return GetValue("LastCheckForUpdates", DateTime.MinValue);
            }

            set
            {
                SetValue("LastCheckForUpdates", value);
            }
        }

        internal static bool ShownTrayBalloon
        {
            get
            {
                return GetValue("ShownTrayBalloon", false);
            }

            set
            {
                SetValue("ShownTrayBalloon", value);
            }
        }

        internal static Model.Favourite.FavouriteCols FavouriteColSortBy
        {
            get
            {
                return (Model.Favourite.FavouriteCols)GetValue("FavouriteColSortBy", (int)Model.Favourite.FavouriteCols.ProgrammeName);
            }

            set
            {
                SetValue("FavouriteColSortBy", (int)value);
            }
        }

        internal static bool FavouriteColSortAsc
        {
            get
            {
                return GetValue("FavouriteColSortAsc", true);
            }

            set
            {
                SetValue("FavouriteColSortAsc", value);
            }
        }

        internal static Model.Subscription.SubscriptionCols SubscriptionColSortBy
        {
            get
            {
                return (Model.Subscription.SubscriptionCols)GetValue("SubscriptionColSortBy", (int)Model.Subscription.SubscriptionCols.ProgrammeName);
            }

            set
            {
                SetValue("SubscriptionColSortBy", (int)value);
            }
        }

        internal static bool SubscriptionColSortAsc
        {
            get
            {
                return GetValue("SubscriptionColSortAsc", true);
            }

            set
            {
                SetValue("SubscriptionColSortAsc", value);
            }
        }

        internal static string DownloadCols
        {
            get
            {
                return GetValue("DownloadCols", "0,1,2,3");
            }

            set
            {
                SetValue("DownloadCols", value);
            }
        }

        internal static string DownloadColSizes
        {
            get
            {
                return GetValue("DownloadColSizes");
            }

            set
            {
                SetValue("DownloadColSizes", value);
            }
        }

        internal static Model.Download.DownloadCols DownloadColSortBy
        {
            get
            {
                return (Model.Download.DownloadCols)GetValue("DownloadColSortBy", (int)Model.Download.DownloadCols.EpisodeDate);
            }

            set
            {
                SetValue("DownloadColSortBy", (int)value);
            }
        }

        internal static bool DownloadColSortAsc
        {
            get
            {
                return GetValue("DownloadColSortAsc", false);
            }

            set
            {
                SetValue("DownloadColSortAsc", value);
            }
        }

        internal static bool RunOnStartup
        {
            get
            {
                return GetValue("RunOnStartup", true);
            }

            set
            {
                SetValue("RunOnStartup", value);
            }
        }

        internal static bool CloseToSystray
        {
            get
            {
                return GetValue("CloseToSystray", false);
            }

            set
            {
                SetValue("CloseToSystray", value);
            }
        }

        internal static Rectangle MainFormPos
        {
            get
            {
                RectangleConverter conv = new RectangleConverter();
                return (Rectangle)conv.ConvertFromInvariantString(GetValue("MainFormPos", conv.ConvertToInvariantString(Rectangle.Empty)));
            }

            set
            {
                RectangleConverter conv = new RectangleConverter();
                SetValue("MainFormPos", conv.ConvertToInvariantString(value));
            }
        }

        internal static FormWindowState MainFormState
        {
            get
            {
                return (FormWindowState)GetValue("MainFormState", (int)FormWindowState.Normal);
            }

            set
            {
                SetValue("MainFormState", (int)value);
            }
        }

        internal static DateTime LastVacuum
        {
            get
            {
                return GetValue("LastVacuum", DateTime.MinValue);
            }

            set
            {
                SetValue("LastVacuum", value);
            }
        }

        internal static int DatabaseVersion
        {
            get
            {
                return GetValue("DatabaseVersion", CurrentDbVersion);
            }

            set
            {
                SetValue("DatabaseVersion", value);
            }
        }

        internal static int ParallelDownloads
        {
            get
            {
                return GetValue("ParallelDownloads", Environment.ProcessorCount > 1 ? Environment.ProcessorCount - 1 : 1);
            }

            set
            {
                SetValue("ParallelDownloads", value);
            }
        }

        internal static string UniqueUserId
        {
            get
            {
                string uniqueId = GetValue("UniqueUserId");

                if (uniqueId == null)
                {
                    uniqueId = Guid.NewGuid().ToString();
                    SetValue("UniqueUserId", uniqueId);
                }

                return uniqueId;
            }
        }

        internal static DateTime LastPrune
        {
            get
            {
                return GetValue("LastPrune", DateTime.MinValue);
            }

            set
            {
                SetValue("LastPrune", value);
            }
        }

        internal static bool RssServer
        {
            get
            {
                return GetValue("RssServer", false);
            }

            set
            {
                SetValue("RssServer", value);
            }
        }

        internal static int RssServerPort
        {
            get
            {
                return GetValue("RssServerPort", 8888);
            }

            set
            {
                SetValue("RssServerPort", value);
            }
        }

        internal static int RssServerNumRecentEps
        {
            get
            {
                return GetValue("RssServerNumRecentEps", 25);
            }

            set
            {
                SetValue("RssServerNumRecentEps", value);
            }
        }

        internal static string SetSaveFolder(Model.Programme progInfo)
        {
            return GetValuePostfix(progInfo, "SaveFolder", null);
        }

        internal static void SetSaveFolder(Model.Programme progInfo, string value)
        {
            SetValuePostfix(progInfo, "SaveFolder", value);
        }

        internal static string GetFileNameFormat(Model.Programme progInfo)
        {
            return GetValuePostfix(progInfo, "FileNameFormat", "%epname% %day%-%month%-%year%");
        }

        internal static void SetFileNameFormat(Model.Programme progInfo, string value)
        {
            SetValuePostfix(progInfo, "FileNameFormat", value);
        }

        internal static string GetRunAfterCommand(Model.Programme progInfo)
        {
            return GetValuePostfix(progInfo, "RunAfterCommand", null);
        }

        internal static void SetRunAfterCommand(Model.Programme progInfo, string value)
        {
            SetValuePostfix(progInfo, "RunAfterCommand", value);
        }

        internal static void ResetUserSettings()
        {
            SetValue("RunOnStartup", null);
            SetValue("CloseToSystray", null);
            SetValue("SaveFolder", null);
            SetValue("FileNameFormat", null);
            SetValue("RunAfterCommand", null);
            SetValue("ParallelDownloads", null);
        }

        internal static void ResetDownloadCols()
        {
            SetValue("DownloadCols", null);
            SetValue("DownloadColSizes", null);
            SetValue("DownloadColSortBy", null);
            SetValue("DownloadColSortAsc", null);
        }

        protected static bool GetValue(string propertyName, bool defaultValue)
        {
            return bool.Parse(GetValue(propertyName, defaultValue.ToString(CultureInfo.InvariantCulture)));
        }

        protected static int GetValue(string propertyName, int defaultValue)
        {
            return int.Parse(GetValue(propertyName, defaultValue.ToString(CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture);
        }

        protected static decimal GetValue(string propertyName, decimal defaultValue)
        {
            return decimal.Parse(GetValue(propertyName, defaultValue.ToString(CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture);
        }

        protected static DateTime GetValue(string propertyName, DateTime defaultValue)
        {
            return DateTime.ParseExact(GetValue(propertyName, defaultValue.ToString("O", CultureInfo.InvariantCulture)), "O", CultureInfo.InvariantCulture);
        }

        protected static string GetValue(string propertyName, string defaultValue)
        {
            string value = GetValue(propertyName);

            if (value == null)
            {
                return defaultValue;
            }

            return value;
        }

        protected static string GetValue(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Property name for value must be specified", "propertyName");
            }

            using (SQLiteCommand command = new SQLiteCommand("select value from settings where property=@property", FetchDbConn()))
            {
                command.Parameters.Add(new SQLiteParameter("@property", propertyName));

                using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader()))
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return reader.GetString(reader.GetOrdinal("value"));
                }
            }
        }

        protected static void SetValue(string propertyName, bool value)
        {
            SetValue(propertyName, value.ToString(CultureInfo.InvariantCulture));
        }

        protected static void SetValue(string propertyName, int value)
        {
            SetValue(propertyName, value.ToString(CultureInfo.InvariantCulture));
        }

        protected static void SetValue(string propertyName, decimal value)
        {
            SetValue(propertyName, value.ToString(CultureInfo.InvariantCulture));
        }

        protected static void SetValue(string propertyName, DateTime value)
        {
            SetValue(propertyName, value.ToString("O", CultureInfo.InvariantCulture));
        }

        protected static void SetValue(string propertyName, string value)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Property name for value must be specified", "propertyName");
            }

            lock (DbUpdateLock)
            {
                if (string.IsNullOrEmpty(value))
                {
                    using (SQLiteCommand command = new SQLiteCommand("delete from settings where property=@property", FetchDbConn()))
                    {
                        command.Parameters.Add(new SQLiteParameter("@property", propertyName));
                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (SQLiteCommand command = new SQLiteCommand("insert or replace into settings (property, value) values (@property, @value)", FetchDbConn()))
                    {
                        command.Parameters.Add(new SQLiteParameter("@property", propertyName));
                        command.Parameters.Add(new SQLiteParameter("@value", value));
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private static string GetValuePostfix(Model.Programme progInfo, string name, string defaultValue)
        {
            string value = null;
            if (progInfo != null)
            {
                value = GetValue(name + progInfo.Progid.ToString(CultureInfo.InvariantCulture));
            }

            if (string.IsNullOrEmpty(value))
            {
                value = GetValue(name, defaultValue);
            }

            return value;
        }

        private static void SetValuePostfix(Model.Programme progInfo, string name, string value)
        {
            string postfix = string.Empty;
            if (progInfo != null)
            {
                postfix = progInfo.Progid.ToString(CultureInfo.InvariantCulture);
            }

            SetValue(name = postfix, value);
        }
    }
}
