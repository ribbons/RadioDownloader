/*
 * This file is part of Radio Downloader.
 * Copyright © 2020 by the authors - see the AUTHORS file for details.
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

namespace RadioDldTest
{
    using System.IO;

    using RadioDld;
    using Xunit;

    /// <summary>
    /// Tests for functions in the static OsUtils class.
    /// </summary>
    public class TestOsUtils
    {
        /// <summary>
        /// Test that the MoveFile function throws the correct exceptions for
        /// errors encountered when attempting to move a file.
        /// </summary>
        [Fact]
        public void MoveFileExceptions()
        {
            string file1 = Path.GetTempFileName();
            string file2 = Path.GetTempFileName();

            Assert.Throws<IOException>(() => OsUtils.MoveFile(file1, file2));

            if (OsUtils.Windows())
            {
                // Pretty much everything is valid in a path under Linux, so only test on Windows
                Assert.Throws<IOException>(() => OsUtils.MoveFile(file1, file2 + ":"));
            }

            Assert.Throws<DirectoryNotFoundException>(
                () => OsUtils.MoveFile(file1, Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "filename")));
        }
    }
}
