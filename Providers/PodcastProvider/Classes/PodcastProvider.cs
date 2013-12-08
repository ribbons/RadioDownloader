/* 
 * This file is part of the Podcast Provider for Radio Downloader.
 * Copyright © 2007-2013 Matt Robinson
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

namespace PodcastProvider
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Windows.Forms;
    using System.Xml;
    using Microsoft.VisualBasic;
    using RadioDld;

    public class PodcastProvider : IRadioProvider
    {
        internal const int CacheHTTPHours = 2;
        internal static readonly string UserAgent = "Podcast Provider " + ((AssemblyInformationalVersionAttribute)typeof(PodcastProvider).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)[0]).InformationalVersion + " for " + Application.ProductName + " " + Application.ProductVersion;

        private DownloadWrapper doDownload;

        public event FindNewViewChangeEventHandler FindNewViewChange
        {
            add { }
            remove { }
        }

        public event FindNewExceptionEventHandler FindNewException;

        public event FoundNewEventHandler FoundNew;

        public event ProgressEventHandler Progress;

        public Guid ProviderId
        {
            get { return new Guid("3cfbe63e-95b8-4f80-8570-4ace909e0921"); }
        }

        public string ProviderName
        {
            get { return "Podcast"; }
        }

        public Bitmap ProviderIcon
        {
            get { return Properties.Resources.provider_icon; }
        }

        public string ProviderDescription
        {
            get { return "Audio files made available as enclosures on an RSS feed."; }
        }

        public int ProgInfoUpdateFreqDays
        {
            // Updating the programme info every week should be a reasonable trade-off
            get { return 7; }
        }

        public EventHandler GetShowOptionsHandler()
        {
            return null;
        }

        public Panel GetFindNewPanel(object view)
        {
            FindNew findNewInst = new FindNew(this);
            return findNewInst.PanelFindNew;
        }

        public ProgrammeInfo GetProgrammeInfo(string progExtId)
        {
            XmlDocument rss = this.LoadFeedXml(new Uri(progExtId));
            XmlNamespaceManager namespaceMgr = this.CreateNamespaceMgr(rss);

            XmlNode titleNode = rss.SelectSingleNode("./rss/channel/title");

            if (titleNode == null || string.IsNullOrEmpty(titleNode.InnerText))
            {
                throw new InvalidDataException("Channel title node is missing or empty");
            }

            ProgrammeInfo progInfo = new ProgrammeInfo();
            progInfo.Name = titleNode.InnerText;

            XmlNode descriptionNode = null;

            // If the channel has an itunes:summary tag use this for the description (as it shouldn't contain HTML)
            if (namespaceMgr.HasNamespace("itunes"))
            {
                descriptionNode = rss.SelectSingleNode("./rss/channel/itunes:summary", namespaceMgr);
            }

            if (descriptionNode != null && !string.IsNullOrEmpty(descriptionNode.InnerText))
            {
                progInfo.Description = descriptionNode.InnerText;
            }
            else
            {
                // Fall back to the standard description tag, but strip the HTML
                descriptionNode = rss.SelectSingleNode("./rss/channel/description");

                if (descriptionNode != null && !string.IsNullOrEmpty(descriptionNode.InnerText))
                {
                    progInfo.Description = this.HtmlToText(descriptionNode.InnerText);
                }
            }

            progInfo.Image = this.RSSNodeImage(rss.SelectSingleNode("./rss/channel"), namespaceMgr);

            return progInfo;
        }

        public AvailableEpisodes GetAvailableEpisodes(string progExtId, ProgrammeInfo progInfo, int page)
        {
            AvailableEpisodes available = new AvailableEpisodes();
            XmlDocument rss = this.LoadFeedXml(new Uri(progExtId));

            XmlNodeList itemNodes = null;
            itemNodes = rss.SelectNodes("./rss/channel/item");

            if (itemNodes == null)
            {
                return available;
            }

            List<string> episodeIDs = new List<string>();

            foreach (XmlNode itemNode in itemNodes)
            {
                string itemId = this.ItemNodeToEpisodeID(itemNode);

                if (!string.IsNullOrEmpty(itemId))
                {
                    episodeIDs.Add(itemId);
                }
            }

            available.EpisodeIds = episodeIDs.ToArray();
            return available;
        }

        public EpisodeInfo GetEpisodeInfo(string progExtId, ProgrammeInfo progInfo, string episodeExtId)
        {
            XmlDocument rss = this.LoadFeedXml(new Uri(progExtId));
            XmlNamespaceManager namespaceMgr = this.CreateNamespaceMgr(rss);
            XmlNode itemNode = this.ItemNodeFromEpisodeID(rss, episodeExtId);

            if (itemNode == null)
            {
                return null;
            }

            XmlNode titleNode = itemNode.SelectSingleNode("./title");
            XmlNode pubDateNode = itemNode.SelectSingleNode("./pubDate");
            XmlNode enclosureNode = itemNode.SelectSingleNode("./enclosure");

            if (enclosureNode == null)
            {
                return null;
            }

            XmlAttribute urlAttrib = enclosureNode.Attributes["url"];

            if (urlAttrib == null)
            {
                return null;
            }

            try
            {
                new Uri(urlAttrib.Value);
            }
            catch (UriFormatException)
            {
                // The enclosure url is empty or malformed
                return null;
            }

            EpisodeInfo episodeInfo = new EpisodeInfo();

            if (titleNode == null || string.IsNullOrEmpty(titleNode.InnerText))
            {
                return null;
            }

            episodeInfo.Name = titleNode.InnerText;

            XmlNode descriptionNode = null;

            // If the item has an itunes:summary tag use this for the description (as it shouldn't contain HTML)
            if (namespaceMgr.HasNamespace("itunes"))
            {
                descriptionNode = itemNode.SelectSingleNode("./itunes:summary", namespaceMgr);
            }

            if (descriptionNode != null && !string.IsNullOrEmpty(descriptionNode.InnerText))
            {
                episodeInfo.Description = descriptionNode.InnerText;
            }
            else
            {
                // Fall back to the standard description tag, but strip the HTML
                descriptionNode = itemNode.SelectSingleNode("./description");

                if (descriptionNode != null && !string.IsNullOrEmpty(descriptionNode.InnerText))
                {
                    episodeInfo.Description = this.HtmlToText(descriptionNode.InnerText);
                }
            }

            try
            {
                XmlNode durationNode = itemNode.SelectSingleNode("./itunes:duration", namespaceMgr);

                if (durationNode != null)
                {
                    string[] splitDuration = durationNode.InnerText.Split(new char[] { '.', ':' });

                    if (splitDuration.GetUpperBound(0) == 0)
                    {
                        episodeInfo.Duration = Convert.ToInt32(splitDuration[0], CultureInfo.InvariantCulture);
                    }
                    else if (splitDuration.GetUpperBound(0) == 1)
                    {
                        episodeInfo.Duration = (Convert.ToInt32(splitDuration[0], CultureInfo.InvariantCulture) * 60) + Convert.ToInt32(splitDuration[1], CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        episodeInfo.Duration = (((Convert.ToInt32(splitDuration[0], CultureInfo.InvariantCulture) * 60) + Convert.ToInt32(splitDuration[1], CultureInfo.InvariantCulture)) * 60) + Convert.ToInt32(splitDuration[2], CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    episodeInfo.Duration = null;
                }
            }
            catch
            {
                episodeInfo.Duration = null;
            }

            if (pubDateNode != null)
            {
                string pubDate = pubDateNode.InnerText.Trim();
                int zonePos = pubDate.LastIndexOf(" ", StringComparison.Ordinal);
                TimeSpan offset = new TimeSpan(0);

                if (zonePos > 0)
                {
                    string zone = pubDate.Substring(zonePos + 1);
                    string zoneFree = pubDate.Substring(0, zonePos);

                    switch (zone)
                    {
                        case "GMT":
                            // No need to do anything
                            break;
                        case "UT":
                            offset = new TimeSpan(0);
                            pubDate = zoneFree;
                            break;
                        case "EDT":
                            offset = new TimeSpan(-4, 0, 0);
                            pubDate = zoneFree;
                            break;
                        case "EST":
                        case "CDT":
                            offset = new TimeSpan(-5, 0, 0);
                            pubDate = zoneFree;
                            break;
                        case "CST":
                        case "MDT":
                            offset = new TimeSpan(-6, 0, 0);
                            pubDate = zoneFree;
                            break;
                        case "MST":
                        case "PDT":
                            offset = new TimeSpan(-7, 0, 0);
                            pubDate = zoneFree;
                            break;
                        case "PST":
                            offset = new TimeSpan(-8, 0, 0);
                            pubDate = zoneFree;
                            break;
                        default:
                            if (zone.Length >= 4 && (Information.IsNumeric(zone) || Information.IsNumeric(zone.Substring(1))))
                            {
                                try
                                {
                                    int value = int.Parse(zone, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                                    offset = new TimeSpan(value / 100, value % 100, 0);
                                    pubDate = zoneFree;
                                }
                                catch (FormatException)
                                {
                                    // The last part of the date was not a time offset
                                }
                            }

                            break;
                    }
                }

                // Strip the day of the week from the beginning of the date string if it is there,
                // as it can contradict the date itself.
                string[] days =
                        {
                            "mon,",
                            "tue,",
                            "wed,",
                            "thu,",
                            "fri,",
                            "sat,",
                            "sun,"
                        };

                foreach (string day in days)
                {
                    if (pubDate.StartsWith(day, StringComparison.OrdinalIgnoreCase))
                    {
                        pubDate = pubDate.Substring(day.Length).Trim();
                        break;
                    }
                }

                try
                {
                    episodeInfo.Date = DateTime.Parse(pubDate, null, DateTimeStyles.AssumeUniversal);
                    episodeInfo.Date = episodeInfo.Date.Value.Subtract(offset);
                }
                catch (FormatException)
                {
                    episodeInfo.Date = null;
                }
            }
            else
            {
                episodeInfo.Date = null;
            }

            episodeInfo.Image = this.RSSNodeImage(itemNode, namespaceMgr);

            return episodeInfo;
        }

        public string DownloadProgramme(string progExtId, string episodeExtId, ProgrammeInfo progInfo, EpisodeInfo epInfo, string finalName)
        {
            XmlDocument rss = this.LoadFeedXml(new Uri(progExtId));
            XmlNamespaceManager namespaceMgr = this.CreateNamespaceMgr(rss);
            XmlNode itemNode = this.ItemNodeFromEpisodeID(rss, episodeExtId);
            XmlNode enclosureNode = itemNode.SelectSingleNode("./enclosure");
            XmlAttribute urlAttrib = enclosureNode.Attributes["url"];
            Uri downloadUrl = new Uri(urlAttrib.Value);

            int fileNamePos = finalName.LastIndexOf("\\", StringComparison.Ordinal);
            int extensionPos = downloadUrl.AbsolutePath.LastIndexOf(".", StringComparison.Ordinal);
            string extension = "mp3";

            if (extensionPos > -1)
            {
                extension = downloadUrl.AbsolutePath.Substring(extensionPos + 1);
            }

            using (TempFile downloadFile = new TempFile(extension))
            {
                finalName += "." + extension;

                this.doDownload = new DownloadWrapper(downloadUrl, downloadFile.FilePath);
                this.doDownload.DownloadProgress += this.DoDownload_DownloadProgress;
                this.doDownload.Download();

                while ((!this.doDownload.Complete) && this.doDownload.Error == null)
                {
                    Thread.Sleep(500);
                }

                if (this.doDownload.Error != null)
                {
                    if (this.doDownload.Error is WebException)
                    {
                        WebException webExp = (WebException)this.doDownload.Error;

                        if (webExp.Status == WebExceptionStatus.NameResolutionFailure)
                        {
                            throw new DownloadException(ErrorType.NetworkProblem, "Unable to resolve " + downloadUrl.Host + " to download this episode from.  Check your internet connection or try again later.");
                        }
                        else if (webExp.Response is HttpWebResponse)
                        {
                            HttpWebResponse webErrorResponse = (HttpWebResponse)webExp.Response;

                            switch (webErrorResponse.StatusCode)
                            {
                                case HttpStatusCode.Forbidden:
                                    throw new DownloadException(ErrorType.RemoteProblem, downloadUrl.Host + " returned a status 403 (Forbidden) in response to the request for this episode.  You may need to contact the podcast publisher if this problem persists.");
                                case HttpStatusCode.NotFound:
                                    throw new DownloadException(ErrorType.NotAvailable, "This episode appears to be no longer available.  You can either try again later, or cancel the download to remove it from the list and clear the error.");
                            }
                        }
                    }

                    throw this.doDownload.Error;
                }

                if (this.Progress != null)
                {
                    this.Progress(100, ProgressType.Processing);
                }

                File.Move(downloadFile.FilePath, finalName);
            }

            return extension;
        }

        internal void RaiseFindNewException(Exception exception)
        {
            if (this.FindNewException != null)
            {
                this.FindNewException(exception, true);
            }
        }

        internal void RaiseFoundNew(string extId)
        {
            if (this.FoundNew != null)
            {
                this.FoundNew(extId);
            }
        }

        internal XmlDocument LoadFeedXml(Uri url)
        {
            XmlDocument feedXml = new XmlDocument();
            CachedWebClient cachedWeb = CachedWebClient.GetInstance();

            string feedString = cachedWeb.DownloadString(url, PodcastProvider.CacheHTTPHours, UserAgent);

            // The LoadXml method of XmlDocument doesn't work correctly all of the time,
            // so convert the string to a UTF-8 byte array
            byte[] encodedString = Encoding.UTF8.GetBytes(feedString);

            // And then load this into the XmlDocument via a stream
            using (MemoryStream feedStream = new MemoryStream(encodedString))
            {
                feedStream.Flush();
                feedStream.Position = 0;

                feedXml.Load(feedStream);
            }

            return feedXml;
        }

        private string ItemNodeToEpisodeID(XmlNode itemNode)
        {
            string itemId = string.Empty;
            XmlNode itemIdNode = itemNode.SelectSingleNode("./guid");

            if (itemIdNode != null)
            {
                itemId = itemIdNode.InnerText;
            }

            if (string.IsNullOrEmpty(itemId))
            {
                itemIdNode = itemNode.SelectSingleNode("./enclosure");

                if (itemIdNode != null)
                {
                    XmlAttribute urlAttrib = itemIdNode.Attributes["url"];

                    if (urlAttrib != null)
                    {
                        itemId = urlAttrib.Value;
                    }
                }
            }

            return itemId;
        }

        private XmlNode ItemNodeFromEpisodeID(XmlDocument rss, string episodeExtId)
        {
            XmlNodeList itemNodes = rss.SelectNodes("./rss/channel/item");

            if (itemNodes == null)
            {
                return null;
            }

            foreach (XmlNode itemNode in itemNodes)
            {
                string itemId = this.ItemNodeToEpisodeID(itemNode);

                if (itemId == episodeExtId)
                {
                    return itemNode;
                }
            }

            return null;
        }

        private Bitmap RSSNodeImage(XmlNode node, XmlNamespaceManager namespaceMgr)
        {
            CachedWebClient cachedWeb = CachedWebClient.GetInstance();

            try
            {
                XmlNode imageNode = node.SelectSingleNode("itunes:image", namespaceMgr);

                if (imageNode != null)
                {
                    Uri imageUrl = new Uri(imageNode.Attributes["href"].Value);
                    byte[] imageData = cachedWeb.DownloadData(imageUrl, CacheHTTPHours, UserAgent);

                    using (MemoryStream imageStream = new MemoryStream(imageData))
                    {
                        using (Bitmap streamBitmap = new Bitmap(imageStream))
                        {
                            return new Bitmap(streamBitmap);
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors and try the next option instead
            }

            try
            {
                XmlNode imageUrlNode = node.SelectSingleNode("image/url");

                if (imageUrlNode != null)
                {
                    Uri imageUrl = new Uri(imageUrlNode.InnerText);
                    byte[] imageData = cachedWeb.DownloadData(imageUrl, CacheHTTPHours, UserAgent);

                    using (MemoryStream imageStream = new MemoryStream(imageData))
                    {
                        using (Bitmap streamBitmap = new Bitmap(imageStream))
                        {
                            return new Bitmap(streamBitmap);
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors and try the next option instead
            }

            try
            {
                XmlNode imageNode = node.SelectSingleNode("media:thumbnail", namespaceMgr);

                if (imageNode != null)
                {
                    Uri imageUrl = new Uri(imageNode.Attributes["url"].Value);
                    byte[] imageData = cachedWeb.DownloadData(imageUrl, CacheHTTPHours, UserAgent);

                    using (MemoryStream imageStream = new MemoryStream(imageData))
                    {
                        using (Bitmap streamBitmap = new Bitmap(imageStream))
                        {
                            return new Bitmap(streamBitmap);
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            return null;
        }

        private XmlNamespaceManager CreateNamespaceMgr(XmlDocument document)
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(document.NameTable);

            foreach (XmlAttribute attrib in document.SelectSingleNode("/*").Attributes)
            {
                if (attrib.Prefix == "xmlns")
                {
                    manager.AddNamespace(attrib.LocalName, attrib.Value);
                }
            }

            return manager;
        }

        private void DoDownload_DownloadProgress(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            int percent = e.ProgressPercentage;

            if (percent > 99)
            {
                percent = 99;
            }

            if (this.Progress != null)
            {
                this.Progress(percent, ProgressType.Downloading);
            }
        }

        private string HtmlToText(string html)
        {
            // Add line breaks before common block level tags
            html = html.Replace("<br", "\r\n<br");
            html = html.Replace("<p", "\r\n<p");
            html = html.Replace("<div", "\r\n<div");

            // Replace HTML entities with their character counterparts
            html = HttpUtility.HtmlDecode(html);

            // Strip out the HTML tags
            Regex stripTags = new Regex("<[^>]+>");
            return stripTags.Replace(html, string.Empty);
        }
    }
}
