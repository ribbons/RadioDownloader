/*
 * Copyright © 2007-2017 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
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
