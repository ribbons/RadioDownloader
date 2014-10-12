/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2014 by the authors - see the AUTHORS file for details.
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
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Text;
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

        internal static bool WinSevenOrLater()
        {
            OperatingSystem curOs = System.Environment.OSVersion;

            if (curOs.Platform == PlatformID.Win32NT && (((curOs.Version.Major == 6) && (curOs.Version.Minor >= 1)) || (curOs.Version.Major > 6)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static bool WinVistaOrLater()
        {
            OperatingSystem curOs = System.Environment.OSVersion;

            if (curOs.Platform == PlatformID.Win32NT && (((curOs.Version.Major == 6) && (curOs.Version.Minor >= 0)) || (curOs.Version.Major > 6)))
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
            OperatingSystem curOs = System.Environment.OSVersion;

            if (curOs.Platform == PlatformID.Win32NT && (((curOs.Version.Major == 5) && (curOs.Version.Minor >= 1)) || (curOs.Version.Major > 5)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static void TrayAnimate(Form form, bool down)
        {
            StringBuilder className = new StringBuilder(255);
            IntPtr taskbarHwnd = default(IntPtr);
            IntPtr trayHwnd = default(IntPtr);

            // Get taskbar handle
            taskbarHwnd = NativeMethods.FindWindow("Shell_traywnd", null);

            if (taskbarHwnd == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            // Get system tray handle
            trayHwnd = NativeMethods.GetWindow(taskbarHwnd, NativeMethods.GW_CHILD);

            do
            {
                if (trayHwnd == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                if (NativeMethods.GetClassName(trayHwnd, className, className.Capacity) == 0)
                {
                    throw new Win32Exception();
                }

                if (className.ToString() == "TrayNotifyWnd")
                {
                    break;
                }

                trayHwnd = NativeMethods.GetWindow(trayHwnd, NativeMethods.GW_HWNDNEXT);
            }
            while (true);

            NativeMethods.RECT systray = default(NativeMethods.RECT);
            NativeMethods.RECT window = default(NativeMethods.RECT);

            // Fetch the location of the systray from its window handle
            if (!NativeMethods.GetWindowRect(trayHwnd, ref systray))
            {
                throw new Win32Exception();
            }

            // Fetch the location of the window from its window handle
            if (!NativeMethods.GetWindowRect(form.Handle, ref window))
            {
                throw new Win32Exception();
            }

            // Perform the animation
            if (down)
            {
                if (!NativeMethods.DrawAnimatedRects(form.Handle, NativeMethods.IDANI_CAPTION, ref window, ref systray))
                {
                    throw new Win32Exception();
                }
            }
            else
            {
                if (!NativeMethods.DrawAnimatedRects(form.Handle, NativeMethods.IDANI_CAPTION, ref systray, ref window))
                {
                    throw new Win32Exception();
                }
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
            RegistryKey runKey;

            try
            {
                runKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            }
            catch (UnauthorizedAccessException)
            {
                // Access denied by non-standard ACL or antivirus
                return;
            }

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
            ulong freeBytesAvailable, totalAvailableBytes, totalFreeBytes;

            if (!NativeMethods.GetDiskFreeSpaceEx(path, out freeBytesAvailable, out totalAvailableBytes, out totalFreeBytes))
            {
                throw new Win32Exception();
            }

            return freeBytesAvailable;
        }
    }
}
