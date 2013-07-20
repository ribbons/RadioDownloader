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
    using System.Collections;
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
            this.ProviderName = Plugins.PluginName(pluginId);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderException" /> class with serialized data
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination. </param>
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
            provExp.Data.Add("Provider", Plugins.PluginInfo(this.ProviderId));

            ErrorReporting report = new ErrorReporting(Plugins.PluginClass(this.ProviderId), provExp);
            return report;
        }
    }
}
