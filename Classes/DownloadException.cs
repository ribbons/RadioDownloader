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

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace RadioDld
{
    [Serializable()]
    public class DldErrorDataItem
    {
        public string Name { get; set; }

        public string Data { get; set; }

        protected DldErrorDataItem()
        {
            // Do nothing, just needed for deserialisation
        }

        public DldErrorDataItem(string name, string data)
        {
            this.Name = name;
            this.Data = data;
        }
    }

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

    [Serializable()]
    public class DownloadException : Exception
    {
        private readonly ErrorType type;
        private readonly List<DldErrorDataItem> extraDetails;

        public DownloadException()
            : base()
        {
            this.type = ErrorType.UnknownError;
        }

        public DownloadException(string message)
            : base(message)
        {
            this.type = ErrorType.UnknownError;
        }

        public DownloadException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.type = ErrorType.UnknownError;
        }

        public DownloadException(ErrorType type)
        {
            this.type = type;
        }

        public DownloadException(ErrorType type, string message)
            : base(message)
        {
            this.type = type;
        }

        public DownloadException(ErrorType type, string message, List<DldErrorDataItem> extraDetails)
            : base(message)
        {
            this.type = type;
            this.extraDetails = extraDetails;
        }

        protected DownloadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.type = (ErrorType)info.GetValue("type", typeof(ErrorType));
            this.extraDetails = (List<DldErrorDataItem>)info.GetValue("extraDetails", typeof(List<DldErrorDataItem>));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("type", type);
            info.AddValue("extraDetails", extraDetails);
        }

        public ErrorType TypeOfError
        {
            get { return type; }
        }

        public List<DldErrorDataItem> ErrorExtraDetails
        {
            get { return extraDetails; }
        }
    }
}
