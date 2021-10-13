/*
 * Copyright © 2007-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Windows.Forms;

    internal static class FileUtils
    {
        public static string GetSaveFolder()
        {
            const string DefaultFolder = "Downloaded Radio";

            string saveFolder;

            if (!string.IsNullOrEmpty(Settings.SaveFolder))
            {
                if (!new DirectoryInfo(Settings.SaveFolder).Exists)
                {
                    throw new DirectoryNotFoundException();
                }

                return Settings.SaveFolder;
            }

            try
            {
                saveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DefaultFolder);
            }
            catch (DirectoryNotFoundException)
            {
                // The user's Documents folder could not be found, so fall back to a folder under the system drive
                saveFolder = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), DefaultFolder);
            }

            Directory.CreateDirectory(saveFolder);
            return saveFolder;
        }

        public static string GetAppDataFolder()
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.CompanyName, Application.ProductName);

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["AppDataDir"]))
            {
                folderPath = ConfigurationManager.AppSettings["AppDataDir"];
            }

            Directory.CreateDirectory(folderPath);
            return folderPath;
        }
    }
}
