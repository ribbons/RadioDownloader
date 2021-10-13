/*
 * Copyright © 2016 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace PodcastProvider
{
    using System;
    using System.Windows.Forms;

    internal partial class MoreProgInfo : Form
    {
        private string progExtId;

        public MoreProgInfo(string progExtId)
        {
            this.progExtId = progExtId;
            this.InitializeComponent();
        }

        private void MoreProgInfo_Load(object sender, EventArgs e)
        {
            this.TextFeedUrl.Text = this.progExtId;
        }
    }
}
