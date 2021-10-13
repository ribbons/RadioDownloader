/*
 * Copyright © 2018-2019 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
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
