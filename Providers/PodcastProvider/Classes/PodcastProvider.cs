/*
 * This file is part of the Podcast Provider for Radio Downloader.
 * Copyright © 2007-2020 by the authors - see the AUTHORS file for details.
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

namespace PodcastProvider
{
    using System;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml;
    using RadioDld;
    using RadioDld.Provider;

    internal delegate DownloadWrapper GetDownloadWrapperInstance(Uri downloadUrl, string destPath);

    public class PodcastProvider : RadioProvider
    {
        internal const int CacheHTTPHours = 2;
        internal static readonly string UserAgent = "Podcast Provider " + ((AssemblyInformationalVersionAttribute)typeof(PodcastProvider).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)[0]).InformationalVersion + " for " + Application.ProductName + " " + Application.ProductVersion;

        private DownloadWrapper doDownload;
        private GetDownloadWrapperInstance getDownloadWrapperInstance;

        public override event FindNewExceptionEventHandler FindNewException;

        public override event FoundNewEventHandler FoundNew;

        public override event ProgressEventHandler Progress;

        public override Guid ProviderId
        {
            get { return new Guid("3cfbe63e-95b8-4f80-8570-4ace909e0921"); }
        }

        public override string ProviderName
        {
            get { return "Podcast"; }
        }

        public override Bitmap ProviderIcon
        {
            get { return Properties.Resources.provider_icon; }
        }

        public override string ProviderDescription
        {
            get { return "Audio files made available as enclosures on an RSS feed."; }
        }

        public override int ProgInfoUpdateFreqDays
        {
            // Updating the programme info every week should be a reasonable trade-off
            get { return 7; }
        }

        public override ShowMoreProgInfoEventHandler ShowMoreProgInfoHandler
        {
            get
            {
                return this.ShowMoreProgInfo;
            }
        }

        /// <summary>
        /// Gets or sets the method for constructing <see cref="DownloadWrapper"/> instances.
        /// </summary>
        internal GetDownloadWrapperInstance GetDownloadWrapperInstance
        {
            get
            {
                if (this.getDownloadWrapperInstance == null)
                {
                    this.getDownloadWrapperInstance = (Uri downloadUrl, string destPath) =>
                    {
                        return new DownloadWrapper(downloadUrl, destPath);
                    };
                }

                return this.getDownloadWrapperInstance;
            }

            set
            {
                this.getDownloadWrapperInstance = value;
            }
        }

        public void ShowMoreProgInfo(string progExtId)
        {
            MoreProgInfo showInfo = new MoreProgInfo(progExtId);
            showInfo.ShowDialog();
        }

        public override Panel GetFindNewPanel(object view)
        {
            FindNew findNewInst = new FindNew(this);
            return findNewInst.PanelFindNew;
        }

        public override ProgrammeInfo GetProgrammeInfo(string progExtId)
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

            // If the channel has an itunes:summary tag use this for the description (as it shouldn't contain HTML)
            var descriptionNode = rss.SelectSingleNode("./rss/channel/itunes:summary", namespaceMgr);

            if (!string.IsNullOrEmpty(descriptionNode?.InnerText))
            {
                progInfo.Description = this.TidyUpWhitespace(descriptionNode.InnerText);
            }
            else
            {
                // Fall back to the standard description tag, but strip the HTML
                descriptionNode = rss.SelectSingleNode("./rss/channel/description");

                if (!string.IsNullOrEmpty(descriptionNode?.InnerText))
                {
                    progInfo.Description = this.TidyUpWhitespace(HtmlToText.ConvertHtml(descriptionNode.InnerText));
                }
            }

            progInfo.Image = this.FetchImage(rss, "./rss/channel/itunes:image/@href", namespaceMgr);

            if (progInfo.Image == null)
            {
                progInfo.Image = this.FetchImage(rss, "./rss/channel/image/url/text()", namespaceMgr);
            }

            return progInfo;
        }

        public override AvailableEpisodes GetAvailableEpisodes(string progExtId, ProgrammeInfo progInfo, int page)
        {
            AvailableEpisodes available = new AvailableEpisodes();
            XmlDocument rss = this.LoadFeedXml(new Uri(progExtId));

            XmlNodeList itemNodes = null;
            itemNodes = rss.SelectNodes("./rss/channel/item");

            if (itemNodes == null)
            {
                return available;
            }

            foreach (XmlNode itemNode in itemNodes)
            {
                string itemId = this.ItemNodeToEpisodeID(itemNode);

                if (string.IsNullOrEmpty(itemId))
                {
                    continue;
                }

                if (itemNode.SelectSingleNode("./title[text()]") == null)
                {
                    continue;
                }

                var urlAttrib = itemNode.SelectSingleNode("./enclosure/@url") as XmlAttribute;

                if (urlAttrib == null)
                {
                    continue;
                }

                Uri uri;
                Uri.TryCreate(urlAttrib.Value, UriKind.Absolute, out uri);

                if (uri == null)
                {
                    continue;
                }

                available.EpisodeIds.Add(itemId);
            }

            return available;
        }

        public override EpisodeInfo GetEpisodeInfo(string progExtId, ProgrammeInfo progInfo, string episodeExtId)
        {
            XmlDocument rss = this.LoadFeedXml(new Uri(progExtId));
            XmlNamespaceManager namespaceMgr = this.CreateNamespaceMgr(rss);
            XmlNode itemNode = this.ItemNodeFromEpisodeID(rss, episodeExtId);

            if (itemNode == null)
            {
                return null;
            }

            EpisodeInfo episodeInfo = new EpisodeInfo();
            episodeInfo.Name = itemNode.SelectSingleNode("./title").InnerText;

            // If the item has an itunes:summary tag use this for the description (as it shouldn't contain HTML)
            var descriptionNode = itemNode.SelectSingleNode("./itunes:summary", namespaceMgr);

            if (!string.IsNullOrEmpty(descriptionNode?.InnerText))
            {
                episodeInfo.Description = this.TidyUpWhitespace(descriptionNode.InnerText);
            }
            else
            {
                // Fall back to the standard description tag, but strip the HTML
                descriptionNode = itemNode.SelectSingleNode("./description");

                if (!string.IsNullOrEmpty(descriptionNode?.InnerText))
                {
                    episodeInfo.Description = this.TidyUpWhitespace(HtmlToText.ConvertHtml(descriptionNode.InnerText));
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

            XmlNode pubDateNode = itemNode.SelectSingleNode("./pubDate");

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
                            int value;

                            if (zone.Length >= 4 && int.TryParse(zone, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value))
                            {
                                offset = new TimeSpan(value / 100, value % 100, 0);
                                pubDate = zoneFree;
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
                            "sun,",
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

            episodeInfo.Image = this.FetchImage(itemNode, "./itunes:image/@href", namespaceMgr);

            if (episodeInfo.Image == null)
            {
                episodeInfo.Image = this.FetchImage(itemNode, "./media:thumbnail/@url", namespaceMgr);
            }

            return episodeInfo;
        }

        public override DownloadInfo DownloadProgramme(string progExtId, string episodeExtId, ProgrammeInfo progInfo, EpisodeInfo epInfo, string finalName)
        {
            XmlDocument rss = this.LoadFeedXml(new Uri(progExtId));
            XmlNode itemNode = this.ItemNodeFromEpisodeID(rss, episodeExtId);

            if (itemNode == null)
            {
                throw new DownloadException(ErrorType.RemoveFromList);
            }

            XmlNode enclosureNode = itemNode.SelectSingleNode("./enclosure");
            XmlAttribute urlAttrib = enclosureNode.Attributes["url"];
            Uri downloadUrl = new Uri(urlAttrib.Value);

            int extensionPos = downloadUrl.AbsolutePath.LastIndexOf(".", StringComparison.Ordinal);

            DownloadInfo info = new DownloadInfo();
            this.EpisodeChapters(info.Chapters, itemNode, this.CreateNamespaceMgr(rss));

            info.Extension = "mp3";

            if (extensionPos > -1)
            {
                info.Extension = downloadUrl.AbsolutePath.Substring(extensionPos + 1);
            }

            using (TempFileBase downloadFile = this.GetTempFileInstance(info.Extension))
            {
                finalName += "." + info.Extension;

                this.doDownload = this.GetDownloadWrapperInstance(downloadUrl, downloadFile.FilePath);
                this.doDownload.DownloadProgress += this.DoDownload_DownloadProgress;
                this.doDownload.Download();

                while ((!this.doDownload.Complete) && this.doDownload.Error == null)
                {
                    Thread.Sleep(500);

                    if (this.doDownload.Canceled)
                    {
                        return null;
                    }
                }

                if (this.doDownload.Error != null)
                {
                    if (this.doDownload.Error is WebException)
                    {
                        WebException webExp = (WebException)this.doDownload.Error;

                        throw new DownloadException(
                            webExp.Status == WebExceptionStatus.ProtocolError
                                ? ErrorType.RemoteProblem
                                : ErrorType.NetworkProblem,
                            webExp.GetBaseException().Message);
                    }

                    throw this.doDownload.Error;
                }

                this.Progress?.Invoke(100, ProgressType.Processing);
                File.Move(downloadFile.FilePath, finalName);
            }

            return info;
        }

        public override void CancelDownload()
        {
            this.doDownload.Cancel();
        }

        internal void RaiseFindNewException(Exception exception)
        {
            this.FindNewException?.Invoke(exception, true);
        }

        internal void RaiseFoundNew(string extId)
        {
            this.FoundNew?.Invoke(extId);
        }

        internal XmlDocument LoadFeedXml(Uri url)
        {
            XmlDocument feedXml = new XmlDocument();
            string feedString = this.CachedWebClient.DownloadString(url, CacheHTTPHours, UserAgent);

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
            itemId = itemIdNode?.InnerText;

            if (string.IsNullOrEmpty(itemId))
            {
                itemIdNode = itemNode.SelectSingleNode("./enclosure");
                XmlAttribute urlAttrib = itemIdNode?.Attributes["url"];

                if (urlAttrib != null)
                {
                    itemId = urlAttrib.Value;
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

        private CompressedImage FetchImage(XmlNode parent, string xpath, XmlNamespaceManager namespaceMgr)
        {
            var urlNode = parent.SelectSingleNode(xpath, namespaceMgr);

            if (urlNode == null)
            {
                return null;
            }

            byte[] imageData;

            try
            {
                imageData = this.CachedWebClient.DownloadData(
                    new Uri(urlNode.Value),
                    CacheHTTPHours,
                    UserAgent);
            }
            catch (Exception e) when (e is UriFormatException || e is WebException)
            {
                return null;
            }

            try
            {
                return new CompressedImage(imageData);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private void EpisodeChapters(Collection<ChapterInfo> chapters, XmlNode itemNode, XmlNamespaceManager namespaceMgr)
        {
            XmlNode chaptersNode = itemNode.SelectSingleNode("./psc:chapters", namespaceMgr);

            if (chaptersNode == null)
            {
                return;
            }

            foreach (XmlNode chapterNode in chaptersNode.SelectNodes("./psc:chapter", namespaceMgr))
            {
                var chapter = new ChapterInfo();
                chapter.Name = chapterNode.Attributes["title"].Value;
                var start = this.ParseChapterStart(chapterNode.Attributes["start"].Value);

                if (start == null)
                {
                    continue;
                }

                chapter.Start = start.Value;

                var hrefAttr = chapterNode.Attributes["href"];

                if (!string.IsNullOrEmpty(hrefAttr?.Value))
                {
                    chapter.Link = new Uri(hrefAttr.Value);
                }

                chapter.Image = this.FetchImage(chapterNode, "./@image", namespaceMgr);
                chapters.Add(chapter);
            }
        }

        private TimeSpan? ParseChapterStart(string value)
        {
            var pattern = new Regex(@"^(?:(?:(?<hours>[0-9]+):)?(?<minutes>[0-9]+):)?(?<seconds>[0-9]+)(?:\.(?<millisec>[0-9]{3}))?$");
            var match = pattern.Match(value);

            if (!match.Success)
            {
                return null;
            }

            int hours = 0;
            int minutes = 0;
            int milliseconds = 0;

            if (match.Groups["hours"].Success)
            {
                hours = int.Parse(match.Groups["hours"].Value, CultureInfo.InvariantCulture);
            }

            if (match.Groups["minutes"].Success)
            {
                minutes = int.Parse(match.Groups["minutes"].Value, CultureInfo.InvariantCulture);
            }

            int seconds = int.Parse(match.Groups["seconds"].Value, CultureInfo.InvariantCulture);

            if (match.Groups["millisec"].Success)
            {
                milliseconds = int.Parse(match.Groups["millisec"].Value, CultureInfo.InvariantCulture);
            }

            return new TimeSpan(0, hours, minutes, seconds, milliseconds);
        }

        private XmlNamespaceManager CreateNamespaceMgr(XmlDocument document)
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(document.NameTable);

            manager.AddNamespace("itunes", "http://www.itunes.com/dtds/podcast-1.0.dtd");
            manager.AddNamespace("media", "http://search.yahoo.com/mrss");
            manager.AddNamespace("psc", "http://podlove.org/simple-chapters");

            return manager;
        }

        private void DoDownload_DownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            int percent = e.ProgressPercentage;

            if (percent > 99)
            {
                percent = 99;
            }

            this.Progress?.Invoke(percent, ProgressType.Downloading);
        }

        /// <summary>
        /// Convert instances of CRLF to LF and replace runs of more than two line breaks in
        /// a row with two line breaks.
        /// </summary>
        /// <param name="input">The string to process.</param>
        /// <returns>The processed string.</returns>
        private string TidyUpWhitespace(string input)
        {
            string output = input.Replace("\r\n", "\n");
            return Regex.Replace(output, "\n{3,}", "\n\n").Trim();
        }
    }
}
