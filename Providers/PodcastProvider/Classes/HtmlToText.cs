/* 
 * This file is part of the Podcast Provider for Radio Downloader.
 * Copyright © 2007-2014 by the authors - see the AUTHORS file for details.
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
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using HtmlAgilityPack;

    internal static class HtmlToText
    {
        public static string ConvertHtml(string html)
        {
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                ConvertNode(doc.DocumentNode, writer);
                writer.Flush();

                // Reduce whitespace chars followed by runs of (non-breaking) spaces to just the whitespace char
                string result = Regex.Replace(writer.ToString(), "(\\s)[\u00A0 ]+", "$1");

                // Replace runs of more than one blank lines in a row with a single blank line
                return Regex.Replace(result, "(?:\r\n){3,}", "\r\n\r\n").Trim();
            }
        }

        private static void ConvertNode(HtmlNode node, TextWriter writer)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    return;

                case HtmlNodeType.Document:
                    foreach (HtmlNode subnode in node.ChildNodes)
                    {
                        ConvertNode(subnode, writer);
                    }

                    break;

                case HtmlNodeType.Text:
                    string text = ((HtmlTextNode)node).Text;

                    // Special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(text))
                    {
                        break;
                    }

                    writer.Write(HtmlEntity.DeEntitize(text));

                    break;

                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        // Add line breaks before common block level tags (and br)
                        case "br":
                        case "p":
                        case "div":
                        case "li":
                            writer.Write("\r\n");
                            break;
                        case "script":
                        case "style":
                            // Suppress children
                            return;
                    }

                    if (node.HasChildNodes)
                    {
                        foreach (HtmlNode subnode in node.ChildNodes)
                        {
                            ConvertNode(subnode, writer);
                        }
                    }

                    break;
            }
        }
    }
}
