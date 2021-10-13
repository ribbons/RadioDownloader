/*
 * Copyright © 2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
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
