/* 
 * This file is part of the Podcast Provider for Radio Downloader.
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

namespace PodcastProvider
{
    using System;
    using System.Net;
    using System.Windows.Forms;
    using System.Xml;

    internal partial class FindNew : Form
    {
        private PodcastProvider pluginInst;

        public FindNew(PodcastProvider pluginInst)
        {
            // This call is required by the Windows Form Designer.
            this.InitializeComponent();

            this.pluginInst = pluginInst;
        }

        private void ButtonView_Click(object sender, EventArgs e)
        {
            try
            {
                this.ButtonView.Enabled = false;
                this.LabelResult.ForeColor = System.Drawing.Color.Black;
                this.LabelResult.Text = "Checking feed...";

                Application.DoEvents();

                Uri feedUrl = null;
                XmlDocument rss = null;

                try
                {
                    feedUrl = new Uri(this.TextFeedUrl.Text);
                }
                catch (UriFormatException)
                {
                    this.LabelResult.Text = "The specified URL was not valid.";
                    this.LabelResult.ForeColor = System.Drawing.Color.Red;
                    this.ButtonView.Enabled = true;
                    return;
                }

                // Test that we can load something from the URL, and it is valid XML
                try
                {
                    rss = this.pluginInst.LoadFeedXml(feedUrl);
                }
                catch (WebException)
                {
                    this.LabelResult.Text = "There was a problem requesting the feed from the specified URL.";
                    this.LabelResult.ForeColor = System.Drawing.Color.Red;
                    this.ButtonView.Enabled = true;
                    return;
                }
                catch (XmlException)
                {
                    this.LabelResult.Text = "The data returned from the specified URL was malformed.";
                    this.LabelResult.ForeColor = System.Drawing.Color.Red;
                    this.ButtonView.Enabled = true;
                    return;
                }

                // Finally, make sure that the required element that we need (e.g. title) exists
                XmlNode checkTitle = rss.SelectSingleNode("./rss/channel/title");

                if (checkTitle == null || string.IsNullOrEmpty(checkTitle.InnerText))
                {
                    this.LabelResult.Text = "The RSS feed returned from the specified URL is missing a title.";
                    this.LabelResult.ForeColor = System.Drawing.Color.Red;
                    this.ButtonView.Enabled = true;
                    return;
                }

                this.LabelResult.Text = "Loading information...";
                Application.DoEvents();

                this.pluginInst.RaiseFoundNew(feedUrl.ToString());

                this.LabelResult.Text = string.Empty;
                this.ButtonView.Enabled = true;
            }
            catch (Exception unandled)
            {
                this.pluginInst.RaiseFindNewException(unandled);
            }
        }

        private void TextFeedUrl_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    if (this.ButtonView.Enabled)
                    {
                        this.ButtonView_Click(sender, e);
                    }
                }
            }
            catch (Exception unhandled)
            {
                this.pluginInst.RaiseFindNewException(unhandled);
            }
        }
    }
}
