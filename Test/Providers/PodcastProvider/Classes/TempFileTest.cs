/*
 * This file is part of the Podcast Provider for Radio Downloader.
 * Copyright © 2018 by the authors - see the AUTHORS file for details.
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

namespace PodcastProviderTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using RadioDld;

    internal sealed class TempFileTest : TempFileBase, IDisposable
    {
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TempFileTest"/> class and generates a temporary
        /// file name with a random extension.
        /// </summary>
        public TempFileTest()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TempFileTest"/> class and generates a temporary
        /// file name with the specified extension.
        /// </summary>
        /// <param name="fileExtension">The extension of the file, not including a leading dot.</param>
        public TempFileTest(string fileExtension)
        {
            do
            {
                string testFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                if (!string.IsNullOrEmpty(fileExtension))
                {
                    // Replace the random extension with the one requested
                    testFilePath = testFilePath.Substring(0, testFilePath.Length - 3) + fileExtension;
                }

                if (File.Exists(testFilePath))
                {
                    // Generated name already exists - fetch another
                    continue;
                }

                this.FilePath = testFilePath;
            }
            while (this.FilePath == null);
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.isDisposed && this.FilePath != null)
            {
                File.Delete(this.FilePath);
            }

            this.isDisposed = true;
        }
    }
}
