/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2020 by the authors - see the AUTHORS file for details.
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
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Windows.Forms;

    using Microsoft.Win32;

    public static class OsUtils
    {
        /// <summary>
        /// Launch the specified URL in the user's browser, with added tracking parameters if
        /// the URL to be launched is part of nerdoftheherd.com.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        /// <param name="action">The action which caused the URL to be launched, which will be set as the 'campaign'.</param>
        public static void LaunchUrl(Uri url, string action)
        {
            UriBuilder launchUri = new UriBuilder(url);

            if (launchUri.Host == "nerdoftheherd.com")
            {
                string analyticsVals = "utm_source=" + HttpUtility.UrlEncode(Application.ProductName + " " + Application.ProductVersion) + "&utm_medium=desktop&utm_campaign=" + HttpUtility.UrlEncode(action);

                // UriBuilder.Query always adds a ? to the start of a passed query
                if (launchUri.Query != null && launchUri.Query.Length > 1)
                {
                    launchUri.Query = url.Query.Substring(1) + "&" + analyticsVals;
                }
                else
                {
                    launchUri.Query = analyticsVals;
                }
            }

            Process.Start(launchUri.Uri.ToString());
        }

        internal static bool Windows()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
        }

        internal static bool WinVistaOrLater()
        {
            OperatingSystem curOs = Environment.OSVersion;

            if (Windows() && (((curOs.Version.Major == 6) && (curOs.Version.Minor >= 0)) || (curOs.Version.Major > 6)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static bool WinXpOrLater()
        {
            OperatingSystem curOs = Environment.OSVersion;

            if (Windows() && (((curOs.Version.Major == 5) && (curOs.Version.Minor >= 1)) || (curOs.Version.Major > 5)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static bool CompositionEnabled()
        {
            if (!WinVistaOrLater())
            {
                return false;
            }

            bool enabled = false;
            Marshal.ThrowExceptionForHR(NativeMethods.DwmIsCompositionEnabled(ref enabled));

            return enabled;
        }

        internal static void ApplyRunOnStartup()
        {
            try
            {
                var runKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

                if (Settings.RunOnStartup)
                {
                    runKey.SetValue(Application.ProductName, "\"" + Application.ExecutablePath + "\" /hidemainwindow");
                }
                else
                {
                    if (runKey.GetValue(Application.ProductName) != null)
                    {
                        runKey.DeleteValue(Application.ProductName);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Access denied by non-standard ACL or antivirus
            }
        }

        internal static bool VisibleOnScreen(Rectangle location)
        {
            foreach (Screen thisScreen in Screen.AllScreens)
            {
                if (thisScreen.WorkingArea.IntersectsWith(location))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Return the number of available bytes in the specified file path.
        /// </summary>
        /// <remarks>
        /// DriveInfo.AvailableFreeSpace gives this information, but only for drives.  This makes it less useful
        /// as paths may contain NTFS junction points, symbolic links, or be UNC paths.
        /// </remarks>
        /// <param name="path">The standard or UNC path to retrieve available bytes for.</param>
        /// <returns>The number of free bytes available to the current user in the specified location.</returns>
        internal static ulong PathAvailableSpace(string path)
        {
            if (Windows())
            {
                ulong freeBytesAvailable, totalAvailableBytes, totalFreeBytes;

                if (!NativeMethods.GetDiskFreeSpaceEx(path, out freeBytesAvailable, out totalAvailableBytes, out totalFreeBytes))
                {
                    throw new Win32Exception();
                }

                return freeBytesAvailable;
            }
            else
            {
                // Call the private Mono internal method System.IO.DriveInfo.GetDiskFreeSpace
                // as this does exactly the same as GetDiskFreeSpaceEx does under Windows
                MethodInfo dynMethod = typeof(DriveInfo).GetMethod("GetDiskFreeSpace", BindingFlags.NonPublic | BindingFlags.Static);

                object[] args = new object[] { path, null, null, null };
                dynMethod.Invoke(null, args);

                return (ulong)args[3];
            }
        }

        /// <summary>
        /// Open the specified file with the default application for that type of file.
        /// This is needed as Mono tries to run any files marked as executable which is a
        /// problem when using an SMB mount where all files are marked as executable.
        /// </summary>
        /// <param name="filePath">The path of the file to open.</param>
        internal static void OpenFileWithDefaultApplication(string filePath)
        {
            if (Windows())
            {
                Process.Start(filePath);
                return;
            }

            var info = new ProcessStartInfo();
            info.UseShellExecute = false;
            info.FileName = "xdg-open";
            info.Arguments = "\"" + filePath + "\"";

            Process.Start(info);
        }

        /// <summary>
        /// A very basic version of System.IO.Path.GetDirectoryName which
        /// doesn't mind if paths are longer than 260 characters.
        /// This will no-longer be required under .NET Core as
        /// GetDirectoryName can cope with long paths.
        /// </summary>
        /// <param name="path">The path to get the directory name for.</param>
        /// <returns>The directory name for the given path.</returns>
        internal static string GetDirectoryName(string path)
        {
            for (int i = path.Length - 1; i > 0; i--)
            {
                if (path[i] == Path.DirectorySeparatorChar || path[i] == Path.AltDirectorySeparatorChar)
                {
                    return path.Substring(0, i);
                }
            }

            return null;
        }

        /// <summary>
        /// Similar to System.IO.File.Move but able to handle paths longer than
        /// 260 characters and doesn't support relative paths.
        /// This will no-longer be required under .NET Core as File.Move
        /// can cope with long paths.
        /// </summary>
        /// <param name="sourceFileName">The fully qualified path of the file to move.</param>
        /// <param name="destFileName">The new fully qualified path and name for the file.</param>
        internal static void MoveFile(string sourceFileName, string destFileName)
        {
            if (Windows())
            {
                // MoveFileW can be replaced by File.Move if targeting the
                // .NET framework 4.6.2 or above.
                if (!NativeMethods.MoveFileW(ToLongPathFormat(sourceFileName), ToLongPathFormat(destFileName)))
                {
                    throw new Win32Exception();
                }
            }
            else
            {
                File.Move(sourceFileName, destFileName);
            }
        }

        /// <summary>
        /// Add the appropriate long path format prefix to the given path.
        /// This will no-longer be required under .NET Core as this is done
        /// internally if required.
        /// </summary>
        /// <param name="standardPath">The path in standard format.</param>
        /// <returns>The path in the long path format.</returns>
        private static string ToLongPathFormat(string standardPath)
        {
            if (standardPath.StartsWith(@"\\", StringComparison.Ordinal))
            {
                return @"\\?\UNC" + standardPath.Substring(1);
            }
            else
            {
                return @"\\?\" + standardPath;
            }
        }
    }
}
