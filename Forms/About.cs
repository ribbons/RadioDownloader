/*
 * Copyright © 2007-2015 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed partial class About : Form
    {
        public About()
        {
            this.InitializeComponent();
        }

        private void About_Load(object sender, EventArgs e)
        {
            this.Font = SystemFonts.MessageBoxFont;

            this.Text = "About " + Application.ProductName;
            this.LabelNameAndVer.Text = Application.ProductName + " " + Application.ProductVersion;
            this.LabelCopyright.Text = new Microsoft.VisualBasic.ApplicationServices.ApplicationBase().Info.Copyright;

            UpdateCheck.CheckAvailable(this.UpdateAvailable);
        }

        private void UpdateAvailable()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.UpdateAvailable(); }));
                return;
            }

            this.LinkUpdate.Visible = true;
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LinkHomepage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OsUtils.LaunchUrl(new Uri(this.LinkHomepage.Text), "About Dialog Link");
        }

        private void LinkUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (UpdateNotify showUpdate = new UpdateNotify())
            {
                if (showUpdate.ShowDialog(this) == DialogResult.Yes)
                {
                    OsUtils.LaunchUrl(new Uri("https://nerdoftheherd.com/tools/radiodld/"), "Download Update (About)");
                    this.Close();
                }
            }
        }
    }
}
