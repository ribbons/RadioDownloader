/*
 * Copyright © 2014-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
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
                return Regex.Replace(writer.ToString(), "(\\s)[\u00A0 ]+", "$1");
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
                            writer.Write("\n");
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
