/*
 * Copyright © 2010-2012 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

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
