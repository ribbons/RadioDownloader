/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2015 by the authors - see the AUTHORS file for details.
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
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Windows.Forms;
    using System.Xml;

    internal class RssServer
    {
        private const string ItunesNS = "http://www.itunes.com/dtds/podcast-1.0.dtd";

        private HttpListener listener;

        public RssServer(int port)
        {
            // Initialise and start the http listener
            this.listener = new HttpListener();
            this.listener.Prefixes.Add(string.Format(CultureInfo.InvariantCulture, "http://+:{0}/", port));

            try
            {
                this.listener.Start();
            }
            catch (HttpListenerException exp)
            {
                if (exp.ErrorCode == 5)
                {
                    MessageBox.Show(string.Format(CultureInfo.InvariantCulture, "Unable to start the RSS server, please run the following command as an administrator to resolve the problem:" + Environment.NewLine + Environment.NewLine + "netsh http add urlacl url=http://+:{0}/ user={1}", port, System.Security.Principal.WindowsIdentity.GetCurrent().Name), Application.ProductName);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Start waiting for the next client request.
        /// </summary>
        public void Start()
        {
            this.listener.BeginGetContext(new AsyncCallback(this.GetContextCallback), this.listener);
        }

        private void GetContextCallback(IAsyncResult result)
        {
            HttpListener listener = result.AsyncState as HttpListener;
            HttpListenerContext context = listener.EndGetContext(result);

            this.Start();

            switch (context.Request.Url.AbsolutePath)
            {
                case "/":
                    using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                    {
                        writer.WriteLine("<!DOCTYPE html><html><head><title>Radio Downloader</title></head>" +
                                         "<body><h1>Radio Downloader</h1>" +
                                         "<p><a href=\"downloaded.rss\">Downloaded epiodes feed</a></p></body></html>");
                    }

                    break;
                case "/downloaded.rss":
                    this.RssFeed(context);
                    break;
                case "/feed-image":
                    this.FeedImageContent(context);
                    break;
                case "/episode-image":
                    this.EpisodeImageContent(context);
                    break;
                case "/file":
                    this.DownloadContent(context);
                    break;
                default:
                    this.ErrorPage404(context);
                    break;
            }

            try
            {
                context.Response.Close();
            }
            catch (InvalidOperationException)
            {
                // Trying to close the stream after a dropped connection
            }
        }

        private void RssFeed(HttpListenerContext context)
        {
            context.Response.ContentType = "text/xml";

            using (XmlTextWriter writer = new XmlTextWriter(context.Response.OutputStream, Encoding.UTF8))
            {
                writer.WriteStartElement("rss");
                writer.WriteAttributeString("version", "2.0");
                writer.WriteAttributeString("xmlns", "itunes", null, ItunesNS);

                writer.WriteStartElement("channel");
                writer.WriteElementString("title", Application.ProductName);
                writer.WriteElementString("description", "The latest episodes downloaded by Radio Downloader.");
                writer.WriteElementString("link", "http://" + context.Request.UserHostName);

                writer.WriteStartElement("image");
                writer.WriteAttributeString("url", "http://" + context.Request.UserHostName + "/feed-image");
                writer.WriteAttributeString("title", Application.ProductName);
                writer.WriteAttributeString("link", "http://" + context.Request.UserHostName);
                writer.WriteEndElement();

                writer.WriteStartElement("image", ItunesNS);
                writer.WriteAttributeString("href", "http://" + context.Request.UserHostName + "/feed-image");
                writer.WriteEndElement();

                List<Model.Download> downloads = Model.Download.FetchLatest(Settings.RssServerNumRecentEps);

                foreach (Model.Download download in downloads)
                {
                    FileInfo file = new FileInfo(download.DownloadPath);

                    if (file.Exists)
                    {
                        writer.WriteStartElement("item");
                        writer.WriteElementString("title", download.Name);
                        writer.WriteElementString("description", this.DescriptionHTML(download.Description));
                        writer.WriteElementString("pubDate", download.Date.ToString("r", CultureInfo.InvariantCulture));
                        writer.WriteElementString("duration", ItunesNS, this.DurationString(download));

                        writer.WriteStartElement("image", ItunesNS);
                        writer.WriteAttributeString("href", string.Format(CultureInfo.InvariantCulture, "http://{0}/episode-image?epid={1}", context.Request.UserHostName, download.Epid));
                        writer.WriteEndElement();

                        writer.WriteStartElement("guid");
                        writer.WriteAttributeString("isPermaLink", "false");
                        writer.WriteString(string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}", Settings.UniqueUserId, download.Progid, download.Epid));
                        writer.WriteEndElement();

                        writer.WriteStartElement("enclosure");
                        writer.WriteAttributeString("url", string.Format(CultureInfo.InvariantCulture, "http://{0}/file?epid={1}", context.Request.UserHostName, download.Epid));
                        writer.WriteAttributeString("length", file.Length.ToString(CultureInfo.InvariantCulture));
                        writer.WriteAttributeString("type", this.MimeTypeForFile(download.DownloadPath));
                        writer.WriteEndElement();

                        writer.WriteEndElement(); // item
                        writer.Flush();
                    }
                }

                writer.WriteEndElement(); // channel
                writer.WriteEndElement(); // rss
            }
        }

        private void FeedImageContent(HttpListenerContext context)
        {
            context.Response.ContentType = "image/png";

            // Find the appropriate codec for saving the episode image as a png
            System.Drawing.Imaging.ImageCodecInfo pngCodec = null;

            foreach (System.Drawing.Imaging.ImageCodecInfo codec in System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders())
            {
                if (codec.MimeType == context.Response.ContentType)
                {
                    pngCodec = codec;
                    break;
                }
            }

            try
            {
                Properties.Resources.icon_main_img256.Save(context.Response.OutputStream, pngCodec, null);
            }
            catch (HttpListenerException)
            {
                // Don't worry about dropped connections etc.
            }
        }

        private void EpisodeImageContent(HttpListenerContext context)
        {
            Regex epidPattern = new Regex(@"^\?epid=([0-9]+)$");
            Match match = epidPattern.Match(context.Request.Url.Query);

            int epid;

            if (!int.TryParse(match.Groups[1].Value, out epid))
            {
                // Request doesn't contain a valid episode id
                this.ErrorPage404(context);
                return;
            }

            Bitmap image = Model.Episode.GetImage(epid);

            if (image == null)
            {
                // Specified episode does not have an image
                this.ErrorPage404(context);
                return;
            }

            context.Response.ContentType = "image/png";

            // Find the appropriate codec for saving the image as a png
            System.Drawing.Imaging.ImageCodecInfo pngCodec = null;

            foreach (System.Drawing.Imaging.ImageCodecInfo codec in System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders())
            {
                if (codec.MimeType == context.Response.ContentType)
                {
                    pngCodec = codec;
                    break;
                }
            }

            try
            {
                image.Save(context.Response.OutputStream, pngCodec, null);
            }
            catch (HttpListenerException)
            {
                // Don't worry about dropped connections etc.
            }
        }

        private void DownloadContent(HttpListenerContext context)
        {
            Regex epidPattern = new Regex(@"^\?epid=([0-9]+)$");
            Match match = epidPattern.Match(context.Request.Url.Query);

            int epid;

            if (!int.TryParse(match.Groups[1].Value, out epid))
            {
                // Request doesn't contain a valid episode id
                this.ErrorPage404(context);
                return;
            }

            Model.Download download;

            try
            {
                download = new Model.Download(epid);
            }
            catch (DataNotFoundException)
            {
                // Specified download does not exist
                this.ErrorPage404(context);
                return;
            }

            FileInfo file = new FileInfo(download.DownloadPath);

            if (!file.Exists)
            {
                // File for specified download no-longer exists
                this.ErrorPage404(context);
                return;
            }

            context.Response.ContentType = this.MimeTypeForFile(download.DownloadPath);
            context.Response.AddHeader("Content-Disposition", "attachment; filename=" + file.Name);
            context.Response.ContentLength64 = file.Length;

            using (FileStream fs = File.OpenRead(download.DownloadPath))
            {
                byte[] buffer = new byte[4096];
                int count;

                try
                {
                    while ((count = fs.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        context.Response.OutputStream.Write(buffer, 0, count);
                    }
                }
                catch (HttpListenerException)
                {
                    // Don't worry about dropped connections etc.
                }
            }
        }

        private void ErrorPage404(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;

            using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.WriteLine("<!DOCTYPE html><html><head><title>404 Not Found</title></head>" +
                                 "<body><h1>Not Found</h1><p>The requested URL " + HttpUtility.HtmlEncode(HttpUtility.UrlDecode(context.Request.RawUrl)) +
                                 " was not found on this server.</p></body></html>");
            }
        }

        private string DurationString(Model.Download download)
        {
            int hours = (int)(download.Duration / 3600);
            int mins = (int)((download.Duration - (hours * 3600)) / 60);
            int secs = (int)((download.Duration - (hours * 3600)) - (mins * 60));

            return string.Format(CultureInfo.InvariantCulture, "{0:0:; ; }{1:00}:{2:00}", hours, mins, secs);
        }

        private string DescriptionHTML(string text)
        {
            string html = HttpUtility.HtmlEncode(text).Replace("\n", "<br>");

            return Regex.Replace(html, @"(https?://([-\w\.]+)+(:\d+)?(/([\w/_\.]*(\?\S+)?)?)?)", "<a href=\"$1\">$1</a>");
        }

        private string MimeTypeForFile(string filePath)
        {
            switch (Path.GetExtension(filePath).ToUpperInvariant())
            {
                case ".MP3":
                    return "audio/mpeg";
                case ".M4A":
                case ".MP4A":
                    return "audio/mp4";
                case ".OGA":
                case ".OGG":
                case ".OPUS":
                    return "audio/ogg";
                case ".FLAC":
                    return "audio/flac";
                default:
                    throw new NotImplementedException("MIME type not known for file extension " + Path.GetExtension(filePath));
            }
        }
    }
}
