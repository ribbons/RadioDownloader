/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2018 by the authors - see the AUTHORS file for details.
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
