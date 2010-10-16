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


using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
namespace RadioDld
{

	internal class FileUtils
	{
		public static string GetSaveFolder()
		{
			string functionReturnValue = null;
			const string defaultFolder = "Downloaded Radio";

			if (!string.IsNullOrEmpty(Properties.Settings.Default.SaveFolder)) {
                if (new DirectoryInfo(Properties.Settings.Default.SaveFolder).Exists == false)
                {
					throw new DirectoryNotFoundException();
				}

				return Properties.Settings.Default.SaveFolder;
			}

			try {
				functionReturnValue = Path.Combine(RadioDld.My.MyProject.Computer.FileSystem.SpecialDirectories.MyDocuments, defaultFolder);
			} catch (DirectoryNotFoundException dirNotFoundExp) {
				// The user's Documents folder could not be found, so fall back to a folder under the system drive
				functionReturnValue = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), defaultFolder);
			}

			Directory.CreateDirectory(GetSaveFolder());
			return functionReturnValue;
		}

		public static string GetAppDataFolder()
		{
			return new DirectoryInfo(RadioDld.My.MyProject.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData).Parent.FullName;
		}

		public static string FindFreeSaveFileName(string formatString, string programmeName, string episodeName, System.DateTime? episodeDate, string baseSavePath)
		{
			string saveName = CreateSaveFileName(formatString, programmeName, episodeName, episodeDate);
			string savePath = Path.Combine(baseSavePath, saveName);
			int diffNum = 1;

			//Make sure the save folder exists (to support subfolders in the save file name template)
			Directory.CreateDirectory(Path.GetDirectoryName(savePath));

			while (Directory.GetFiles(Path.GetDirectoryName(savePath), Path.GetFileName(savePath) + ".*").Length > 0) {
				savePath = Path.Combine(baseSavePath, saveName + " (" + Convert.ToString(diffNum) + ")");
				diffNum += 1;
			}

			return savePath;
		}

		public static string CreateSaveFileName(string formatString, string programmeName, string episodeName, System.DateTime? episodeDate)
		{
			if (formatString == string.Empty) {
				// The format string is an empty string, so the output must be an empty string
				return string.Empty;
			}

			string fileName = formatString;
            DateTime substDate = DateTime.Now;

            if (episodeDate != null)
            {
                substDate = episodeDate.Value;
            }

			// Convert %title% -> %epname% for backwards compatability
			fileName = fileName.Replace("%title%", "%epname%");

			// Make variable substitutions
			fileName = fileName.Replace("%progname%", programmeName);
			fileName = fileName.Replace("%epname%", episodeName);
            fileName = fileName.Replace("%day%", substDate.ToString("dd", CultureInfo.CurrentCulture));
            fileName = fileName.Replace("%month%", substDate.ToString("MM", CultureInfo.CurrentCulture));
            fileName = fileName.Replace("%shortmonthname%", substDate.ToString("MMM", CultureInfo.CurrentCulture));
            fileName = fileName.Replace("%monthname%", substDate.ToString("MMMM", CultureInfo.CurrentCulture));
            fileName = fileName.Replace("%year%", substDate.ToString("yy", CultureInfo.CurrentCulture));
            fileName = fileName.Replace("%longyear%", substDate.ToString("yyyy", CultureInfo.CurrentCulture));

			// Replace invalid file name characters with spaces (except for directory separators
			// as this then allows the flexibility of storing the downloads in subdirectories)
			foreach (char removeChar in Path.GetInvalidFileNameChars()) {
				if (removeChar != Path.DirectorySeparatorChar) {
                    fileName = fileName.Replace(removeChar, ' ');
				}
			}

			// Replace runs of spaces with a single space
			fileName = Regex.Replace(fileName, " {2,}", " ");

			return Strings.Trim(fileName);
		}
	}
}
