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
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using Microsoft.Win32;

namespace RadioDld
{

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}
namespace RadioDld
{

    internal class OsUtils
    {
        private const short GW_CHILD = 5;
        private const short GW_HWNDNEXT = 2;
        private const short IDANI_OPEN = 0x1;
        private const short IDANI_CLOSE = 0x2;

        private const short IDANI_CAPTION = 0x3;
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DrawAnimatedRects(IntPtr hWnd, int idAni, ref RECT lprcFrom, ref RECT lprcTo);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, int wCmd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        private static extern int GetClassName(System.IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("dwmapi.dll", SetLastError = true)]
        private static extern int DwmIsCompositionEnabled([MarshalAs(UnmanagedType.Bool)] ref bool pfEnabled);

        public static bool WinSevenOrLater()
        {
            OperatingSystem curOs = System.Environment.OSVersion;

            if (curOs.Platform == PlatformID.Win32NT && (((curOs.Version.Major == 6) && (curOs.Version.Minor >= 1)) || (curOs.Version.Major > 6))) {
                return true;
            } else {
                return false;
            }
        }

        public static bool WinVistaOrLater()
        {
            OperatingSystem curOs = System.Environment.OSVersion;

            if (curOs.Platform == PlatformID.Win32NT && (((curOs.Version.Major == 6) && (curOs.Version.Minor >= 0)) || (curOs.Version.Major > 6))) {
                return true;
            } else {
                return false;
            }
        }

        public static bool WinXpOrLater()
        {
            OperatingSystem curOs = System.Environment.OSVersion;

            if (curOs.Platform == PlatformID.Win32NT && (((curOs.Version.Major == 5) && (curOs.Version.Minor >= 1)) || (curOs.Version.Major > 5))) {
                return true;
            } else {
                return false;
            }
        }

        public static void TrayAnimate(Form form, bool down)
        {
            StringBuilder className = new StringBuilder(255);
            IntPtr taskbarHwnd = default(IntPtr);
            IntPtr trayHwnd = default(IntPtr);

            // Get taskbar handle
            taskbarHwnd = FindWindow("Shell_traywnd", null);

            if (taskbarHwnd == IntPtr.Zero) {
                throw new Win32Exception();
            }

            // Get system tray handle
            trayHwnd = GetWindow(taskbarHwnd, GW_CHILD);

            do {
                if (trayHwnd == IntPtr.Zero) {
                    throw new Win32Exception();
                }

                if (GetClassName(trayHwnd, className, className.Capacity) == 0) {
                    throw new Win32Exception();
                }

                if (className.ToString() == "TrayNotifyWnd") {
                    break; // TODO: might not be correct. Was : Exit Do
                }

                trayHwnd = GetWindow(trayHwnd, GW_HWNDNEXT);
            } while (true);

            RECT systray = default(RECT);
            RECT window = default(RECT);

            // Fetch the location of the systray from its window handle
            if (GetWindowRect(trayHwnd, ref systray) == false) {
                throw new Win32Exception();
            }

            // Fetch the location of the window from its window handle
            if (GetWindowRect(form.Handle, ref window) == false) {
                throw new Win32Exception();
            }

            // Perform the animation
            if (down == true) {
                if (DrawAnimatedRects(form.Handle, IDANI_CLOSE | IDANI_CAPTION, ref window, ref systray) == false) {
                    throw new Win32Exception();
                }
            } else {
                if (DrawAnimatedRects(form.Handle, IDANI_OPEN | IDANI_CAPTION, ref systray, ref window) == false) {
                    throw new Win32Exception();
                }
            }
        }

        public static bool CompositionEnabled()
        {
            if (!WinVistaOrLater()) {
                return false;
            }

            bool enabled = false;

            if (DwmIsCompositionEnabled(ref enabled) != 0) {
                throw new Win32Exception();
            }

            return enabled;
        }

        public static void ApplyRunOnStartup()
        {
            RegistryKey runKey = RadioDld.My.MyProject.Computer.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (Properties.Settings.Default.RunOnStartup) {
                runKey.SetValue(RadioDld.My.MyProject.Application.Info.Title, "\"" + Application.ExecutablePath + "\" /hidemainwindow");
            } else {
                if (runKey.GetValue(RadioDld.My.MyProject.Application.Info.Title) != null) {
                    runKey.DeleteValue(RadioDld.My.MyProject.Application.Info.Title);
                }
            }
        }

        public static bool VisibleOnScreen(Rectangle location)
        {
            foreach (Screen thisScreen in Screen.AllScreens) {
                if ((thisScreen.WorkingArea.IntersectsWith(location))) {
                    return true;
                }
            }

            return false;
        }
    }
}
