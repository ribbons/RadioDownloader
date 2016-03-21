/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2012 by the authors - see the AUTHORS file for details.
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

namespace RadioDld
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal partial class ReportError : Form
    {
        private static object showingLock = new object();
        private static bool showing = false;

        private ErrorReporting report;

        public ReportError()
        {
            this.InitializeComponent();
        }

        public void ShowReport(ErrorReporting report)
        {
            this.report = report;

            lock (showingLock)
            {
                if (showing)
                {
                    // Another instance of this form is currently being shown
                    return;
                }

                showing = true;
            }

            this.ShowDialog();

            lock (showingLock)
            {
                showing = false;
            }
        }

        private void ButtonSend_Click(object sender, EventArgs e)
        {
            try
            {
                this.Visible = false;
                this.report.SendReport();
            }
            catch
            {
                // No way of reporting errors that have happened here, so just give up
            }

            this.Close();
        }

        private void LinkWhatData_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(this.report.ToString(), Application.ProductName);
        }

        private void Error_FormClosing(object sender, FormClosingEventArgs e)
        {
            // As there has been an error, blow away the rest of the app reasonably tidily
            Environment.Exit(1);
        }

        private void ReportError_Load(object sender, EventArgs e)
        {
            try
            {
                this.Font = SystemFonts.MessageBoxFont;
                this.ImageIcon.Image = SystemIcons.Error.ToBitmap();
            }
            catch
            {
                // Just show the dialog with the default font & without an icon
            }
        }
    }
}
