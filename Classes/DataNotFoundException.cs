/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2012 Matt Robinson
 * 
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
 * implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
 * License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with this program.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

namespace RadioDld
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    [Serializable]
    public class DataNotFoundException : Exception
    {
        public DataNotFoundException()
            : base()
        {
            this.NotFoundId = null;
        }

        public DataNotFoundException(string message)
            : base(message)
        {
            this.NotFoundId = null;
        }

        public DataNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.NotFoundId = null;
        }

        public DataNotFoundException(int notFoundId)
            : base()
        {
            this.NotFoundId = notFoundId;
        }

        public DataNotFoundException(int notFoundId, string message)
            : base(message)
        {
            this.NotFoundId = notFoundId;
        }

        public DataNotFoundException(int notFoundId, string message, Exception innerException)
            : base(message, innerException)
        {
            this.NotFoundId = notFoundId;
        }

        protected DataNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.NotFoundId = (int?)info.GetValue("NotFoundId", typeof(int?));
        }

        public int? NotFoundId { get; private set; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("NotFoundId", this.NotFoundId);
        }
    }
}
