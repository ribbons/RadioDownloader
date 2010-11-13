/* 
 * This file is part of Radio Downloader.
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

namespace RadioDld
{
    using System;
    using System.Windows.Forms;
    using Microsoft.VisualBasic;

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

        private void ButtonSend_Click(object sender, System.EventArgs e)
        {
            try
            {
                this.Visible = false;
                this.report.SendReport(Properties.Settings.Default.ErrorReportURL);
            }
            catch
            {
                // No way of reporting errors that have happened here, so just give up
            }

            this.Close();
        }

        private void LinkWhatData_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            Interaction.MsgBox(this.report.ToString());
        }

        private void ButtonDontSend_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void Error_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            // As there has been an error, blow away the rest of the app reasonably tidily
            System.Environment.Exit(1);
        }
    }
}
