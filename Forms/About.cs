/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2014 Matt Robinson
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

namespace RadioDld
{
    using System;
    using System.Diagnostics;
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
                this.Invoke((MethodInvoker)delegate { this.UpdateAvailable(); });
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
                    OsUtils.LaunchUrl(new Uri("http://nerdoftheherd.com/tools/radiodld/"), "Download Update (About)");
                    this.Close();
                }
            }
        }
    }
}
