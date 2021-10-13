/*
 * Copyright © 2018-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace PodcastProviderTest
{
    using System;
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
