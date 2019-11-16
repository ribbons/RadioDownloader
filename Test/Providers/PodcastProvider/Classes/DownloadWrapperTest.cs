/*
 * This file is part of the Podcast Provider for Radio Downloader.
 * Copyright © 2018-2019 by the authors - see the AUTHORS file for details.
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

namespace PodcastProviderTest
{
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Authentication;

    internal class DownloadWrapperTest : PodcastProvider.DownloadWrapper
    {
        private Uri downloadUrl;

        public DownloadWrapperTest(Uri downloadUrl, string destPath)
            : base(downloadUrl, destPath)
        {
            this.downloadUrl = downloadUrl;
        }

        public override void Download()
        {
            switch (this.downloadUrl.AbsolutePath)
            {
                case "/errors/http504":
                    this.Error = new WebException(
                        "The remote server returned an error: (504) Gateway Timeout.",
                        null,
                        WebExceptionStatus.ProtocolError,
                        null);
                    break;
                case "/errors/untrustedCert":
                    this.Error = new WebException(
                        "Could not establish trust relationship for the SSL/TLS secure channel.",
                        new AuthenticationException("The remote certificate is invalid according to the validation procedure."),
                        WebExceptionStatus.SecureChannelFailure,
                        null);
                    break;
                default:
                    File.Create(this.DestPath).Dispose();
                    this.Complete = true;
                    break;
            }
        }
    }
}
