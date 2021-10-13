/*
 * Copyright © 2012-2018 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception class used to wrap unhandled provider exceptions, so that they can
    /// easily be separated from exceptions within the application.
    /// </summary>
    [Serializable]
    internal class ProviderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderException" /> class with a specified error message,
        /// a reference to the inner provider exception that is the cause of this exception, and the
        /// ID of the provider that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The provider exception that is the cause of this exception.</param>
        /// <param name="pluginId">The ID of the provider that is the cause of this exception.</param>
        public ProviderException(string message, Exception innerException, Guid pluginId)
            : base(message, innerException)
        {
            this.ProviderId = pluginId;
            this.ProviderName = Provider.Handler.GetFromId(pluginId).Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderException" /> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected ProviderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets the ID of the provider that caused the current exception.
        /// </summary>
        public Guid ProviderId { get; private set; }

        /// <summary>
        /// Gets the display name of the provider that caused the current exception.
        /// </summary>
        public string ProviderName { get; private set; }

        /// <summary>
        /// Build an ErrorReporting object from the data in this exception ready to send to the server.
        /// </summary>
        /// <returns>An ErrorReporting object containing information about the provider exception.</returns>
        public ErrorReporting BuildReport()
        {
            Exception provExp = this.InnerException;
            provExp.Data.Add("Provider", Provider.Handler.GetFromId(this.ProviderId).ToString());

            ErrorReporting report = new ErrorReporting(Provider.Handler.GetFromId(this.ProviderId).Class, provExp);
            return report;
        }
    }
}
