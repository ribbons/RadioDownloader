/*
 * Copyright © 2010-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld.Provider
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
        RemoteProblem = 7,
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
