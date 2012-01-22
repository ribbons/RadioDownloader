/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2011 Matt Robinson
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
    using System.IO;
    using System.Windows.Forms;

    internal class FileUtils
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
            string folderPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.CompanyName), Application.ProductName);
            Directory.CreateDirectory(folderPath);

            return folderPath;
        }
    }
}
