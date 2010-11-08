// Plugin for Radio Downloader to download general podcasts.
// Copyright © 2007-2010 Matt Robinson
//
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General
// Public License as published by the Free Software Foundation; either version 2 of the License, or (at your
// option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
// implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
// License for more details.
//
// You should have received a copy of the GNU General Public License along with this program; if not, write
// to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

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
        public event FindNewViewChangeEventHandler FindNewViewChange
        {
            add { throw new NotSupportedException(); }
            remove { }
        }

        public event FindNewExceptionEventHandler FindNewException;

        public event FoundNewEventHandler FoundNew;

        public event ProgressEventHandler Progress;

        public event FinishedEventHandler Finished;

        internal const int intCacheHTTPHours = 2;
        private DownloadWrapper doDownload;

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
            FindNew FindNewInst = new FindNew(this);
            return FindNewInst.pnlFindNew;
        }

        public GetProgrammeInfoReturn GetProgrammeInfo(string progExtId)
        {
            GetProgrammeInfoReturn getProgInfo = new GetProgrammeInfoReturn();
            getProgInfo.Success = false;

            XmlDocument xmlRSS = null;
            XmlNamespaceManager xmlNamespaceMgr = null;

            try
            {
                xmlRSS = this.LoadFeedXml(new Uri(progExtId));
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
                xmlNamespaceMgr = this.CreateNamespaceMgr(xmlRSS);
            }
            catch
            {
                xmlNamespaceMgr = null;
            }

            XmlNode xmlTitle = xmlRSS.SelectSingleNode("./rss/channel/title");
            XmlNode xmlDescription = xmlRSS.SelectSingleNode("./rss/channel/description");

            if (xmlTitle == null | xmlDescription == null)
            {
                return getProgInfo;
            }

            getProgInfo.ProgrammeInfo.Name = xmlTitle.InnerText;

            if (string.IsNullOrEmpty(getProgInfo.ProgrammeInfo.Name))
            {
                return getProgInfo;
            }

            getProgInfo.ProgrammeInfo.Description = xmlDescription.InnerText;
            getProgInfo.ProgrammeInfo.Image = this.RSSNodeImage(xmlRSS.SelectSingleNode("./rss/channel"), xmlNamespaceMgr);

            getProgInfo.Success = true;
            return getProgInfo;
        }

        public string[] GetAvailableEpisodeIds(string progExtId)
        {
            List<string> episodeIDs = new List<string>();
            XmlDocument xmlRSS = null;

            try
            {
                xmlRSS = this.LoadFeedXml(new Uri(progExtId));
            }
            catch (WebException)
            {
                return episodeIDs.ToArray();
            }
            catch (XmlException)
            {
                return episodeIDs.ToArray();
            }

            XmlNodeList xmlItems = null;
            xmlItems = xmlRSS.SelectNodes("./rss/channel/item");

            if (xmlItems == null)
            {
                return episodeIDs.ToArray();
            }

            string strItemID = null;

            foreach (XmlNode xmlItem in xmlItems)
            {
                strItemID = this.ItemNodeToEpisodeID(xmlItem);

                if (!string.IsNullOrEmpty(strItemID))
                {
                    episodeIDs.Add(strItemID);
                }
            }

            return episodeIDs.ToArray();
        }

        public GetEpisodeInfoReturn GetEpisodeInfo(string progExtId, string episodeExtId)
        {
            GetEpisodeInfoReturn episodeInfoReturn = new GetEpisodeInfoReturn();
            episodeInfoReturn.Success = false;

            XmlDocument xmlRSS = null;
            XmlNamespaceManager xmlNamespaceMgr = null;

            try
            {
                xmlRSS = this.LoadFeedXml(new Uri(progExtId));
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
                xmlNamespaceMgr = this.CreateNamespaceMgr(xmlRSS);
            }
            catch
            {
                xmlNamespaceMgr = null;
            }

            XmlNodeList xmlItems = null;
            xmlItems = xmlRSS.SelectNodes("./rss/channel/item");

            if (xmlItems == null)
            {
                return episodeInfoReturn;
            }

            string strItemID = null;

            foreach (XmlNode xmlItem in xmlItems)
            {
                strItemID = this.ItemNodeToEpisodeID(xmlItem);

                if (strItemID == episodeExtId)
                {
                    XmlNode xmlTitle = xmlItem.SelectSingleNode("./title");
                    XmlNode xmlDescription = xmlItem.SelectSingleNode("./description");
                    XmlNode xmlPubDate = xmlItem.SelectSingleNode("./pubDate");
                    XmlNode xmlEnclosure = xmlItem.SelectSingleNode("./enclosure");

                    if (xmlEnclosure == null)
                    {
                        return episodeInfoReturn;
                    }

                    XmlAttribute xmlUrl = xmlEnclosure.Attributes["url"];

                    if (xmlUrl == null)
                    {
                        return episodeInfoReturn;
                    }

                    try
                    {
                        Uri uriTestValid = new Uri(xmlUrl.Value);
                    }
                    catch (UriFormatException)
                    {
                        // The enclosure url is empty or malformed, so return false for success
                        return episodeInfoReturn;
                    }

                    Dictionary<string, string> dicExtInfo = new Dictionary<string, string>();
                    dicExtInfo.Add("EnclosureURL", xmlUrl.Value);

                    if (xmlTitle != null)
                    {
                        episodeInfoReturn.EpisodeInfo.Name = xmlTitle.InnerText;
                    }

                    if (string.IsNullOrEmpty(episodeInfoReturn.EpisodeInfo.Name))
                    {
                        return episodeInfoReturn;
                    }

                    if (xmlDescription != null)
                    {
                        string description = xmlDescription.InnerText;

                        // Replace common block level tags with newlines
                        description = description.Replace("<br", Constants.vbCrLf + "<br");
                        description = description.Replace("<p", Constants.vbCrLf + "<p");
                        description = description.Replace("<div", Constants.vbCrLf + "<div");

                        // Replace HTML entities with their character counterparts
                        description = HttpUtility.HtmlDecode(description);

                        // Strip out any HTML tags
                        Regex RegExpression = new Regex("<(.|\\n)+?>");
                        episodeInfoReturn.EpisodeInfo.Description = RegExpression.Replace(description, string.Empty);
                    }

                    try
                    {
                        XmlNode xmlDuration = xmlItem.SelectSingleNode("./itunes:duration", xmlNamespaceMgr);

                        if (xmlDuration != null)
                        {
                            string[] strSplitDuration = Strings.Split(xmlDuration.InnerText.Replace(".", ":"), ":");

                            if (strSplitDuration.GetUpperBound(0) == 0)
                            {
                                episodeInfoReturn.EpisodeInfo.DurationSecs = Convert.ToInt32(strSplitDuration[0], CultureInfo.InvariantCulture);
                            }
                            else if (strSplitDuration.GetUpperBound(0) == 1)
                            {
                                episodeInfoReturn.EpisodeInfo.DurationSecs = (Convert.ToInt32(strSplitDuration[0], CultureInfo.InvariantCulture) * 60) + Convert.ToInt32(strSplitDuration[1], CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                episodeInfoReturn.EpisodeInfo.DurationSecs = (((Convert.ToInt32(strSplitDuration[0], CultureInfo.InvariantCulture) * 60) + Convert.ToInt32(strSplitDuration[1], CultureInfo.InvariantCulture)) * 60) + Convert.ToInt32(strSplitDuration[2], CultureInfo.InvariantCulture);
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

                    if (xmlPubDate != null)
                    {
                        string strPubDate = xmlPubDate.InnerText.Trim();
                        int intZonePos = strPubDate.LastIndexOf(" ", StringComparison.Ordinal);
                        TimeSpan tspOffset = new TimeSpan(0);

                        if (intZonePos > 0)
                        {
                            string strZone = strPubDate.Substring(intZonePos + 1);
                            string strZoneFree = strPubDate.Substring(0, intZonePos);

                            switch (strZone)
                            {
                                case "GMT":
                                    // No need to do anything
                                    break;
                                case "UT":
                                    tspOffset = new TimeSpan(0);
                                    strPubDate = strZoneFree;
                                    break;
                                case "EDT":
                                    tspOffset = new TimeSpan(-4, 0, 0);
                                    strPubDate = strZoneFree;
                                    break;
                                case "EST":
                                case "CDT":
                                    tspOffset = new TimeSpan(-5, 0, 0);
                                    strPubDate = strZoneFree;
                                    break;
                                case "CST":
                                case "MDT":
                                    tspOffset = new TimeSpan(-6, 0, 0);
                                    strPubDate = strZoneFree;
                                    break;
                                case "MST":
                                case "PDT":
                                    tspOffset = new TimeSpan(-7, 0, 0);
                                    strPubDate = strZoneFree;
                                    break;
                                case "PST":
                                    tspOffset = new TimeSpan(-8, 0, 0);
                                    strPubDate = strZoneFree;
                                    break;
                                default:
                                    if (strZone.Length >= 4 & Information.IsNumeric(strZone) | Information.IsNumeric(strZone.Substring(1)))
                                    {
                                        try
                                        {
                                            int intValue = int.Parse(strZone, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                                            tspOffset = new TimeSpan(intValue / 100, intValue % 100, 0);
                                            strPubDate = strZoneFree;
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
                        string[] strDays =
                        {
                            "mon,",
                            "tue,",
                            "wed,",
                            "thu,",
                            "fri,",
                            "sat,",
                            "sun,"
                        };

                        foreach (string strDay in strDays)
                        {
                            if (strPubDate.StartsWith(strDay, StringComparison.OrdinalIgnoreCase))
                            {
                                strPubDate = strPubDate.Substring(strDay.Length).Trim();
                                break;
                            }
                        }

                        try
                        {
                            episodeInfoReturn.EpisodeInfo.Date = System.DateTime.Parse(strPubDate, null, DateTimeStyles.AssumeUniversal);
                        }
                        catch (FormatException)
                        {
                            episodeInfoReturn.EpisodeInfo.Date = DateAndTime.Now;
                            tspOffset = new TimeSpan(0);
                        }

                        episodeInfoReturn.EpisodeInfo.Date = episodeInfoReturn.EpisodeInfo.Date.Subtract(tspOffset);
                    }
                    else
                    {
                        episodeInfoReturn.EpisodeInfo.Date = DateAndTime.Now;
                    }

                    episodeInfoReturn.EpisodeInfo.Image = this.RSSNodeImage(xmlItem, xmlNamespaceMgr);

                    if (episodeInfoReturn.EpisodeInfo.Image == null)
                    {
                        episodeInfoReturn.EpisodeInfo.Image = this.RSSNodeImage(xmlRSS.SelectSingleNode("./rss/channel"), xmlNamespaceMgr);
                    }

                    episodeInfoReturn.EpisodeInfo.ExtInfo = dicExtInfo;
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
            this.doDownload.DownloadProgress += this.doDownload_DownloadProgress;
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

        internal void RaiseFindNewException(Exception expException)
        {
            if (this.FindNewException != null)
            {
                this.FindNewException(expException, true);
            }
        }

        internal void RaiseFoundNew(string strExtID)
        {
            if (this.FoundNew != null)
            {
                this.FoundNew(strExtID);
            }
        }

        internal XmlDocument LoadFeedXml(Uri url)
        {
            XmlDocument feedXml = new XmlDocument();
            CachedWebClient cachedWeb = CachedWebClient.GetInstance();

            string feedString = cachedWeb.DownloadString(url, PodcastProvider.intCacheHTTPHours);

            // The LoadXml method of XmlDocument doesn't work correctly all of the time,
            // so convert the string to a UTF-8 byte array
            byte[] encodedString = Encoding.UTF8.GetBytes(feedString);

            // And then load this into the XmlDocument via a stream
            MemoryStream feedStream = new MemoryStream(encodedString);
            feedStream.Flush();
            feedStream.Position = 0;

            feedXml.Load(feedStream);
            return feedXml;
        }

        private string ItemNodeToEpisodeID(XmlNode xmlItem)
        {
            string strItemID = string.Empty;
            XmlNode xmlItemID = xmlItem.SelectSingleNode("./guid");

            if (xmlItemID != null)
            {
                strItemID = xmlItemID.InnerText;
            }

            if (string.IsNullOrEmpty(strItemID))
            {
                xmlItemID = xmlItem.SelectSingleNode("./enclosure");

                if (xmlItemID != null)
                {
                    XmlAttribute xmlUrl = xmlItemID.Attributes["url"];

                    if (xmlUrl != null)
                    {
                        strItemID = xmlUrl.Value;
                    }
                }
            }

            return strItemID;
        }

        private Bitmap RSSNodeImage(XmlNode xmlNode, XmlNamespaceManager xmlNamespaceMgr)
        {
            CachedWebClient cachedWeb = CachedWebClient.GetInstance();

            try
            {
                XmlNode xmlImageNode = xmlNode.SelectSingleNode("itunes:image", xmlNamespaceMgr);

                if (xmlImageNode != null)
                {
                    Uri imageUrl = new Uri(xmlImageNode.Attributes["href"].Value);
                    byte[] bteImageData = cachedWeb.DownloadData(imageUrl, intCacheHTTPHours);
                    return new Bitmap(new System.IO.MemoryStream(bteImageData));
                }
            }
            catch
            {
                // Ignore errors and try the next option instead
            }

            try
            {
                XmlNode xmlImageUrlNode = xmlNode.SelectSingleNode("image/url");

                if (xmlImageUrlNode != null)
                {
                    Uri imageUrl = new Uri(xmlImageUrlNode.InnerText);
                    byte[] bteImageData = cachedWeb.DownloadData(imageUrl, intCacheHTTPHours);
                    return new Bitmap(new System.IO.MemoryStream(bteImageData));
                }
            }
            catch
            {
                // Ignore errors and try the next option instead
            }

            try
            {
                XmlNode xmlImageNode = xmlNode.SelectSingleNode("media:thumbnail", xmlNamespaceMgr);

                if (xmlImageNode != null)
                {
                    Uri imageUrl = new Uri(xmlImageNode.Attributes["url"].Value);
                    byte[] bteImageData = cachedWeb.DownloadData(imageUrl, intCacheHTTPHours);
                    return new Bitmap(new System.IO.MemoryStream(bteImageData));
                }
            }
            catch
            {
                // Ignore errors
            }

            return null;
        }

        private XmlNamespaceManager CreateNamespaceMgr(XmlDocument xmlDocument)
        {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDocument.NameTable);

            foreach (XmlAttribute xmlAttrib in xmlDocument.SelectSingleNode("/*").Attributes)
            {
                if (xmlAttrib.Prefix == "xmlns")
                {
                    nsManager.AddNamespace(xmlAttrib.LocalName, xmlAttrib.Value);
                }
            }

            return nsManager;
        }

        private void doDownload_DownloadProgress(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            int intPercent = e.ProgressPercentage;

            if (intPercent > 99)
            {
                intPercent = 99;
            }

            if (this.Progress != null)
            {
                this.Progress(intPercent, "Downloading...", ProgressIcon.Downloading);
            }
        }
    }
}
