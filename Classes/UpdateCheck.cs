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
using System.Net;

using Microsoft.VisualBasic;

namespace RadioDld
{
    internal class UpdateCheck
    {
        private string versionInfoURL;

        public UpdateCheck(string versionInfoURL)
        {
            this.versionInfoURL = versionInfoURL;
        }

        public bool IsUpdateAvailable()
        {
            if (Properties.Settings.Default.LastCheckForUpdates.AddDays(1) > DateAndTime.Now)
            {
                return false;
            }

            WebClient checkUpdate = new WebClient();
            checkUpdate.Headers.Add("user-agent", RadioDld.My.MyProject.Application.Info.AssemblyName + " " + RadioDld.My.MyProject.Application.Info.Version.ToString());

            string versionInfo = null;

            try
            {
                versionInfo = checkUpdate.DownloadString(versionInfoURL);
            }
            catch (WebException)
            {
                // Temporary problem downloading the information, try again later
                return false;
            }

            Properties.Settings.Default.LastCheckForUpdates = DateAndTime.Now;
            Properties.Settings.Default.Save(); // Save the last check time in case of unexpected termination

            if (!string.IsNullOrEmpty(versionInfo))
            {
                versionInfo = Strings.Split(versionInfo, Constants.vbCrLf)[0];

                // There is a new version available
                if (String.Compare(versionInfo, RadioDld.My.MyProject.Application.Info.Version.ToString(), StringComparison.Ordinal) > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
