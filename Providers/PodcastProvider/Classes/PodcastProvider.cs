/* 
 * This file is part of the Podcast Provider for Radio Downloader.
 * Copyright © 2007-2010 Matt Robinson
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

        private DownloadWrapper doDownload;

        public event FindNewViewChangeEventHandler FindNewViewChange
        {
            add { throw new NotSupportedException(); }
            remove { }
        }

        public event FindNewExceptionEventHandler FindNewException;

        public event FoundNewEventHandler FoundNew;

        public event ProgressEventHandler Progress;

        public event FinishedEventHandler Finished;

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

        public GetProgrammeInfoReturn GetProgrammeInfo(string progExtId)
        {
            GetProgrammeInfoReturn getProgInfo = new GetProgrammeInfoReturn();
            getProgInfo.Success = false;

            XmlDocument rss = null;
            XmlNamespaceManager namespaceMgr = null;

            try
            {
                rss = this.LoadFeedXml(new Uri(progExtId));
            }
            catch (WebException)
            {
                return getProgInfo;
            }
            catch (XmlException)
            {
                return getProgInfo;
            }

            try
            {
                namespaceMgr = this.CreateNamespaceMgr(rss);
            }
            catch
            {
                namespaceMgr = null;
            }

            XmlNode titleNode = rss.SelectSingleNode("./rss/channel/title");
            XmlNode descriptionNode = rss.SelectSingleNode("./rss/channel/description");

            if (titleNode == null | descriptionNode == null)
            {
                return getProgInfo;
            }

            getProgInfo.ProgrammeInfo.Name = titleNode.InnerText;

            if (string.IsNullOrEmpty(getProgInfo.ProgrammeInfo.Name))
            {
                return getProgInfo;
            }

            getProgInfo.ProgrammeInfo.Description = descriptionNode.InnerText;
            getProgInfo.ProgrammeInfo.Image = this.RSSNodeImage(rss.SelectSingleNode("./rss/channel"), namespaceMgr);

            getProgInfo.Success = true;
            return getProgInfo;
        }

        public string[] GetAvailableEpisodeIds(string progExtId)
        {
            List<string> episodeIDs = new List<string>();
            XmlDocument rss = null;

            try
            {
                rss = this.LoadFeedXml(new Uri(progExtId));
            }
            catch (WebException)
            {
                return episodeIDs.ToArray();
            }
            catch (XmlException)
            {
                return episodeIDs.ToArray();
            }

            XmlNodeList itemNodes = null;
            itemNodes = rss.SelectNodes("./rss/channel/item");

            if (itemNodes == null)
            {
                return episodeIDs.ToArray();
            }

            string itemId = null;

            foreach (XmlNode itemNode in itemNodes)
            {
                itemId = this.ItemNodeToEpisodeID(itemNode);

                if (!string.IsNullOrEmpty(itemId))
                {
                    episodeIDs.Add(itemId);
                }
            }

            return episodeIDs.ToArray();
        }

        public GetEpisodeInfoReturn GetEpisodeInfo(string progExtId, string episodeExtId)
        {
            GetEpisodeInfoReturn episodeInfoReturn = new GetEpisodeInfoReturn();
            episodeInfoReturn.Success = false;

            XmlDocument rss = null;
            XmlNamespaceManager namespaceMgr = null;

            try
            {
                rss = this.LoadFeedXml(new Uri(progExtId));
            }
            catch (WebException)
            {
                return episodeInfoReturn;
            }
            catch (XmlException)
            {
                return episodeInfoReturn;
            }

            try
            {
                namespaceMgr = this.CreateNamespaceMgr(rss);
            }
            catch
            {
                namespaceMgr = null;
            }

            XmlNodeList itemNodes = null;
            itemNodes = rss.SelectNodes("./rss/channel/item");

            if (itemNodes == null)
            {
                return episodeInfoReturn;
            }

            string itemId = null;

            foreach (XmlNode itemNode in itemNodes)
            {
                itemId = this.ItemNodeToEpisodeID(itemNode);

                if (itemId == episodeExtId)
                {
                    XmlNode titleNode = itemNode.SelectSingleNode("./title");
                    XmlNode descriptionNode = itemNode.SelectSingleNode("./description");
                    XmlNode pubDateNode = itemNode.SelectSingleNode("./pubDate");
                    XmlNode enclosureNode = itemNode.SelectSingleNode("./enclosure");

                    if (enclosureNode == null)
                    {
                        return episodeInfoReturn;
                    }

                    XmlAttribute urlAttrib = enclosureNode.Attributes["url"];

                    if (urlAttrib == null)
                    {
                        return episodeInfoReturn;
                    }

                    try
                    {
                        new Uri(urlAttrib.Value);
                    }
                    catch (UriFormatException)
                    {
                        // The enclosure url is empty or malformed, so return false for success
                        return episodeInfoReturn;
                    }

                    Dictionary<string, string> extInfo = new Dictionary<string, string>();
                    extInfo.Add("EnclosureURL", urlAttrib.Value);

                    if (titleNode != null)
                    {
                        episodeInfoReturn.EpisodeInfo.Name = titleNode.InnerText;
                    }

                    if (string.IsNullOrEmpty(episodeInfoReturn.EpisodeInfo.Name))
                    {
                        return episodeInfoReturn;
                    }

                    if (descriptionNode != null)
                    {
                        string description = descriptionNode.InnerText;

                        // Replace common block level tags with newlines
                        description = description.Replace("<br", Constants.vbCrLf + "<br");
                        description = description.Replace("<p", Constants.vbCrLf + "<p");
                        description = description.Replace("<div", Constants.vbCrLf + "<div");

                        // Replace HTML entities with their character counterparts
                        description = HttpUtility.HtmlDecode(description);

                        // Strip out any HTML tags
                        Regex stripTags = new Regex("<(.|\\n)+?>");
                        episodeInfoReturn.EpisodeInfo.Description = stripTags.Replace(description, string.Empty);
                    }

                    try
                    {
                        XmlNode durationNode = itemNode.SelectSingleNode("./itunes:duration", namespaceMgr);

                        if (durationNode != null)
                        {
                            string[] splitDuration = Strings.Split(durationNode.InnerText.Replace(".", ":"), ":");

                            if (splitDuration.GetUpperBound(0) == 0)
                            {
                                episodeInfoReturn.EpisodeInfo.DurationSecs = Convert.ToInt32(splitDuration[0], CultureInfo.InvariantCulture);
                            }
                            else if (splitDuration.GetUpperBound(0) == 1)
                            {
                                episodeInfoReturn.EpisodeInfo.DurationSecs = (Convert.ToInt32(splitDuration[0], CultureInfo.InvariantCulture) * 60) + Convert.ToInt32(splitDuration[1], CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                episodeInfoReturn.EpisodeInfo.DurationSecs = (((Convert.ToInt32(splitDuration[0], CultureInfo.InvariantCulture) * 60) + Convert.ToInt32(splitDuration[1], CultureInfo.InvariantCulture)) * 60) + Convert.ToInt32(splitDuration[2], CultureInfo.InvariantCulture);
                            }
                        }
                        else
                        {
                            episodeInfoReturn.EpisodeInfo.DurationSecs = null;
                        }
                    }
                    catch
                    {
                        episodeInfoReturn.EpisodeInfo.DurationSecs = null;
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
                                    if (zone.Length >= 4 & Information.IsNumeric(zone) | Information.IsNumeric(zone.Substring(1)))
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
                            episodeInfoReturn.EpisodeInfo.Date = System.DateTime.Parse(pubDate, null, DateTimeStyles.AssumeUniversal);
                        }
                        catch (FormatException)
                        {
                            episodeInfoReturn.EpisodeInfo.Date = DateAndTime.Now;
                            offset = new TimeSpan(0);
                        }

                        episodeInfoReturn.EpisodeInfo.Date = episodeInfoReturn.EpisodeInfo.Date.Subtract(offset);
                    }
                    else
                    {
                        episodeInfoReturn.EpisodeInfo.Date = DateAndTime.Now;
                    }

                    episodeInfoReturn.EpisodeInfo.Image = this.RSSNodeImage(itemNode, namespaceMgr);

                    if (episodeInfoReturn.EpisodeInfo.Image == null)
                    {
                        episodeInfoReturn.EpisodeInfo.Image = this.RSSNodeImage(rss.SelectSingleNode("./rss/channel"), namespaceMgr);
                    }

                    episodeInfoReturn.EpisodeInfo.ExtInfo = extInfo;
                    episodeInfoReturn.Success = true;

                    return episodeInfoReturn;
                }
            }

            return episodeInfoReturn;
        }

        public void DownloadProgramme(string progExtId, string episodeExtId, ProgrammeInfo progInfo, EpisodeInfo epInfo, string finalName)
        {
            Uri downloadUrl = new Uri(epInfo.ExtInfo["EnclosureURL"]);

            int fileNamePos = finalName.LastIndexOf("\\", StringComparison.Ordinal);
            int extensionPos = downloadUrl.AbsolutePath.LastIndexOf(".", StringComparison.Ordinal);
            string extension = "mp3";

            if (extensionPos > -1)
            {
                extension = downloadUrl.AbsolutePath.Substring(extensionPos + 1);
            }

            string downloadFileName = Path.Combine(System.IO.Path.GetTempPath(), Path.Combine("RadioDownloader", finalName.Substring(fileNamePos + 1) + "." + extension));
            finalName += "." + extension;

            this.doDownload = new DownloadWrapper(downloadUrl, downloadFileName);
            this.doDownload.DownloadProgress += this.DoDownload_DownloadProgress;
            this.doDownload.Download();

            while ((!this.doDownload.Complete) & this.doDownload.Error == null)
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
                        throw new DownloadException(ErrorType.NetworkProblem, "Unable to resolve the domain to download this episode from.  Check your internet connection, or try again later.");
                    }
                    else if (webExp.Response is HttpWebResponse)
                    {
                        HttpWebResponse webErrorResponse = (HttpWebResponse)webExp.Response;

                        if (webErrorResponse.StatusCode == HttpStatusCode.NotFound)
                        {
                            throw new DownloadException(ErrorType.NotAvailable, "This episode appears to be no longer available.  You can either try again later, or cancel the download to remove it from the list and clear the error.");
                        }
                    }
                }

                throw this.doDownload.Error;
            }

            if (this.Progress != null)
            {
                this.Progress(100, "Downloading...", ProgressIcon.Downloading);
            }

            File.Move(downloadFileName, finalName);

            if (this.Finished != null)
            {
                this.Finished(extension);
            }
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

            string feedString = cachedWeb.DownloadString(url, PodcastProvider.CacheHTTPHours);

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

        private Bitmap RSSNodeImage(XmlNode node, XmlNamespaceManager namespaceMgr)
        {
            CachedWebClient cachedWeb = CachedWebClient.GetInstance();

            try
            {
                XmlNode imageNode = node.SelectSingleNode("itunes:image", namespaceMgr);

                if (imageNode != null)
                {
                    Uri imageUrl = new Uri(imageNode.Attributes["href"].Value);
                    byte[] imageData = cachedWeb.DownloadData(imageUrl, CacheHTTPHours);

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
                    byte[] imageData = cachedWeb.DownloadData(imageUrl, CacheHTTPHours);

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
                    byte[] imageData = cachedWeb.DownloadData(imageUrl, CacheHTTPHours);

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
                this.Progress(percent, "Downloading...", ProgressIcon.Downloading);
            }
        }
    }
}
