/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2012 by the authors - see the AUTHORS file for details.
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
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    public enum ErrorType
    {
        UnknownError = 0,
        LocalProblem = 1,
        ShorterThanExpected = 2,
        NotAvailable = 3,
        RemoveFromList = 4,
        NotAvailableInLocation = 5,
        NetworkProblem = 6,
        RemoteProblem = 7
    }

    [Serializable]
    public sealed class DownloadException : Exception
    {
        public DownloadException()
            : base()
        {
            this.ErrorType = ErrorType.UnknownError;
        }

        public DownloadException(string message)
            : base(message)
        {
            this.ErrorType = ErrorType.UnknownError;
        }

        public DownloadException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.ErrorType = ErrorType.UnknownError;
        }

        public DownloadException(ErrorType type)
        {
            this.ErrorType = type;
        }

        public DownloadException(ErrorType type, string message)
            : base(message)
        {
            this.ErrorType = type;
        }

        public DownloadException(string message, string details)
            : base(message)
        {
            this.ErrorType = ErrorType.UnknownError;

            if (!string.IsNullOrEmpty(details))
            {
                // Replace the exception stack trace with the supplied details
                this.Data.Add("errordetails", details);
            }
        }

        private DownloadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.ErrorType = (ErrorType)info.GetValue("ErrorType", typeof(ErrorType));
        }

        public ErrorType ErrorType { get; private set; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("ErrorType", this.ErrorType);
        }
    }
}
