/*
 * Copyright © 2012-2018 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;

    public abstract class TempFileBase : Database, IDisposable
    {
        ~TempFileBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets or sets the generated temporary file path.
        /// </summary>
        public string FilePath { get; protected set; }

        /// <summary>
        /// Delete the temporary file at the path given by FilePath.  If the file is in use it will
        /// be cleaned up the next time a temporary file is created or deleted after it becomes free.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }
}
